using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Wrench.Builder;
using Wrench.Weaver;

namespace Wrench.CodeGen.Processors
{
	public class WrenchModuleDefinition
	{
		public string Path;
		public TypeDefinition ModuleType;
		public ILProcessor InitializerBody;
	}

	public class ModuleProcessor
	{
		private const string InitializerMethodName = WrenchWeaver.Prefix + "init";

		public List<WrenchModuleDefinition> Modules = new List<WrenchModuleDefinition>();

		public bool TryExtract(WrenchWeaver weaver, TypeDefinition input)
		{
			var data = new WrenchModuleDefinition
			{
				ModuleType = input,
			};

			// implements attribute and has valid arguments
			if (input.HasAttribute<WrenchModuleAttribute>(out var attribute) == false) return false;
			if (attribute.HasConstructorArguments == false
				|| attribute.ConstructorArguments[0].Value is not string str
				|| string.IsNullOrEmpty(str))
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchModuleAttribute)}` but doesnt have a valid `{nameof(WrenchModuleAttribute.Path)}`",
					input);
				return false;
			}

			data.Path = str;

			// derives from module class
			if (input.IsDerivedFrom<Module>() == false)
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchModuleAttribute)}` but derive from type `{nameof(Builder.Module)}`",
					input);
				return false;
			}

			// create initializer method
			var md = new MethodDefinition(InitializerMethodName,
				MethodAttributes.HideBySig | MethodAttributes.Private,
				weaver.Imports.VoidRef);
			input.Methods.Add(md);
			data.InitializerBody = md.Body.GetILProcessor();
			data.InitializerBody.Emit(OpCodes.Nop);
			data.InitializerBody.Emit(OpCodes.Ret);

			// weave the initializer in the constructors
			for (int i = 0; i < input.Methods.Count; i++)
			{
				var method = input.Methods[i];

				if (method.IsConstructor == false) continue;

				// allow an empty default constructor
				if (method.HasEmptyBody() && method.HasParameters == false)
				{
					WeaveInitMethodOnConstructor(weaver, method, data);
					continue;
				}

				weaver.Logger.Error(
					$"`{input.FullName}` {method.HasEmptyBody()} {method.Body.Instructions.Count} has constructors. this is not supported at the moment",
					method);
				return false;
			}

			Modules.Add(data);
			return true;
		}

		private static void WeaveInitMethodOnConstructor(WrenchWeaver weaver, MethodDefinition constructor,
			WrenchModuleDefinition module)
		{
			// assumes that it has an empty body
			constructor.Body.Instructions.Clear();
			constructor.Body.Variables.Clear();

			var il = constructor.Body.GetILProcessor();

			// base..ctor({Path});
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldstr, module.Path);
			il.Emit(OpCodes.Call, weaver.Imports.Module_ctor__string);
			il.Emit(OpCodes.Nop);

			// this.{Init}();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, module.InitializerBody.Body.Method);
			il.Emit(OpCodes.Nop);

			il.Emit(OpCodes.Ret);
		}

		public void Process(WrenchWeaver weaver, ClassProcessor classProcessor)
		{
			for (int i = 0; i < Modules.Count; i++)
			{
				var moduleData = Modules[i];

				moduleData.InitializerBody.Body.Instructions.Clear();
				moduleData.InitializerBody.Body.Variables.Clear();

				var il = moduleData.InitializerBody.Body.GetILProcessor();

				for (int j = 0; j < classProcessor.Classes.Count; j++)
				{
					var classData = classProcessor.Classes[i];

					if (classData.ModuleType != moduleData.ModuleType) continue;

					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Newobj, classData.CtorMethod);
					il.Emit(OpCodes.Call, weaver.Imports.Module_Add__IModuleScoped);
					il.DEBUG_EmitNop();
				}

				il.Emit(OpCodes.Ret);
			}
		}
	}
}
