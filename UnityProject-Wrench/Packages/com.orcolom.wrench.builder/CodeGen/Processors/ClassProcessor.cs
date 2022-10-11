using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Wrench.Builder;
using Wrench.Weaver;

namespace Wrench.CodeGen.Processors
{
	public class WrenchClassDefinition
	{
		public string Name;
		public TypeDefinition ModuleType;
		public ILProcessor InitializerBody;
	}

	
	public class ClassProcessor : IProcessor<WrenchImports, WrenchWeaver, TypeDefinition, WrenchClassDefinition>
	{
		private const string InitializerMethodName = WrenchWeaver.Prefix + "init";

		public List<WrenchClassDefinition> Classes = new List<WrenchClassDefinition>();

		public bool TryExtract(WrenchWeaver weaver, TypeDefinition input, out WrenchClassDefinition data)
		{
			data = new WrenchClassDefinition();

			// implements attribute and has valid arguments
			if (input.HasAttribute<WrenchClassAttribute>(out var attribute) == false) return false;
			if (attribute.HasConstructorArguments == false)
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchClassAttribute)}` but doesnt have any valid parameters",
					input);
				return false;
			}
			
			if (attribute.ConstructorArguments[1].Value is not string str || string.IsNullOrEmpty(str))
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchClassAttribute)}` but doesnt have a valid `{nameof(WrenchClassAttribute.Name)}`",
					input);
				return false;
			}

			data.Name = str;

			if (attribute.ConstructorArguments[0].Value is not TypeDefinition moduleType ||
				moduleType.IsDerivedFrom<Module>() == false)
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchClassAttribute)}` but doesnt have a valid `{nameof(WrenchClassAttribute.ModuleType)}`",
					input);
				return false;
			}

			data.ModuleType = moduleType;

			// derives from module class
			if (input.IsDerivedFrom<Class>() == false)
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchClassAttribute)}` but derive from type `{nameof(Class)}`",
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
				weaver.Logger.Error($"`{input.FullName}` has constructors. this is not supported at the moment", method);
				return false;
			}
			
			Classes.Add(data);
			return true;
		}
		
		private static void WeaveInitMethodOnConstructor(WrenchWeaver weaver, MethodDefinition constructor, WrenchClassDefinition module)
		{
			// assumes that it has an empty body
			constructor.Body.Instructions.Clear();
			constructor.Body.Variables.Clear();
			
			var il = constructor.Body.GetILProcessor();
			
			// local variables
			constructor.Body.InitLocals = true;
			var localForeign = new VariableDefinition(weaver.Imports.ForeignClass);
			constructor.Body.Variables.Add(localForeign);
			
			// base..ctor((Attributes) null, {Name}, (string) null, new ForeignClass(), (ClassBody) null);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Ldstr, module.Name);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Ldloca_S, localForeign);
			il.Emit(OpCodes.Initobj, weaver.Imports.ForeignClass);
			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Call, weaver.Imports.Class_ctor__Attributes_string_string_ForeignClass_ClassBody);
			il.Emit(OpCodes.Nop);
			
			// this.{Init}();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, module.InitializerBody.Body.Method);
			il.Emit(OpCodes.Nop);

			il.Emit(OpCodes.Ret);
		}
	}
}
