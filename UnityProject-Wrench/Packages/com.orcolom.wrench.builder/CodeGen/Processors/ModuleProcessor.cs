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

	public class ModuleProcessor : IProcessor<WrenchImports, WrenchWeaver, TypeDefinition, WrenchModuleDefinition>
	{
		private const string InitializerMethodName = WrenchWeaver.Prefix + "init";

		public List<WrenchModuleDefinition> Modules = new List<WrenchModuleDefinition>();

		public bool TryExtract(WrenchWeaver weaver, TypeDefinition input, out WrenchModuleDefinition data)
		{
			data = new WrenchModuleDefinition
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
				weaver.Logger.Error($"`{input.FullName}` {method.HasEmptyBody()} {method.Body.Instructions.Count} has constructors. this is not supported at the moment", method);
				return false;
			}
			
			Modules.Add(data);
			return true;
		}

		private static void WeaveInitMethodOnConstructor(WrenchWeaver weaver, MethodDefinition constructor, WrenchModuleDefinition module)
		{
			// assumes that it has an empty body
			
			constructor.Body.RemoveTrailingRet();

			var il = constructor.Body.Instructions;
			
			// : base() => : base({Path})
			il.Insert(1, Instruction.Create(OpCodes.Ldstr, module.Path));
			il.RemoveAt(2);
			il.Insert(2, Instruction.Create(OpCodes.Call, weaver.Imports.Module_ctor__string));
			
			// this.{Init}();
			il.Add(Instruction.Create(OpCodes.Ldarg_0));
			il.Add(Instruction.Create(OpCodes.Call, module.InitializerBody.Body.Method));
			il.Add(Instruction.Create(OpCodes.Nop));
			
			il.Add(Instruction.Create(OpCodes.Ret));
		}
	}
}
