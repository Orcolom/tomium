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
		public TypeDefinition ClassType;
		public TypeDefinition ForType;
		public MethodDefinition CtorMethod;
		public MethodDefinition InitMethod;
		public List<WrenchMethodDefinition> Methods = new List<WrenchMethodDefinition>();

		public void AddMethod(WrenchMethodDefinition data)
		{
			ClassType.Methods.Add(data.WrapperMethod);
			Methods.Add(data);
		}
	}


	public class ClassProcessor
	{
		private const string InitializerMethodName = WrenchWeaver.Prefix + "init";

		public List<WrenchClassDefinition> Classes = new List<WrenchClassDefinition>();

		public bool TryExtract(WrenchWeaver weaver, TypeDefinition input, out WrenchClassDefinition data)
		{
			data = new WrenchClassDefinition()
			{
				ClassType = input,
			};

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

			if (attribute.ConstructorArguments[2].Value is TypeReference type)
			{
				data.ForType = type.Resolve();
			}

			// derives from module class
			if (input.IsDerivedFrom<Class>() == false)
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchClassAttribute)}` but derive from type `{nameof(Class)}`",
					input);
				return false;
			}

			// create initializer method
			data.InitMethod = new MethodDefinition(InitializerMethodName,
				MethodAttributes.HideBySig | MethodAttributes.Private,
				weaver.Imports.Void);
			input.Methods.Add(data.InitMethod);
			
			var il = data.InitMethod.Body.GetILProcessor();
			il.Emit(OpCodes.Nop);
			il.Emit(OpCodes.Ret);
			
			// weave the initializer in the constructors
			for (int i = 0; i < input.Methods.Count; i++)
			{
				var method = input.Methods[i];

				if (method.IsConstructor == false) continue;

				// allow an empty default constructor
				if (method.HasEmptyBody() && method.HasParameters == false)
				{
					WeaveInitMethodOnConstructor(weaver, method, data);
					data.CtorMethod = method;
					continue;
				}

				weaver.Logger.Error($"`{input.FullName}` has constructors. this is not supported at the moment", method);
				return false;
			}

			Classes.Add(data);
			return true;
		}

		private static void WeaveInitMethodOnConstructor(WrenchWeaver weaver, MethodDefinition constructor,
			WrenchClassDefinition module)
		{
			// assumes that it has an empty body
			constructor.Body.Instructions.Clear();
			constructor.Body.Variables.Clear();

			var il = constructor.Body.GetILProcessor();

			// local variables
			constructor.Body.InitLocals = true;

			// base..ctor((Attributes) null, {Name}, (string) null, {ForeignClass}, (ClassBody) null);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Ldstr, module.Name);
			il.Emit(OpCodes.Ldnull);
			if (module.ForType == null)
			{
				// new ForeignClass();
				var localForeign = new VariableDefinition(weaver.Imports.ForeignClass);
				constructor.Body.Variables.Add(localForeign);
				
				il.Emit(OpCodes.Ldloca_S, localForeign);
				il.Emit(OpCodes.Initobj, weaver.Imports.ForeignClass);
				il.Emit(OpCodes.Ldloc_0);
			}
			else
			{
				// ForeignClass.DefaultAlloc<T>()
				var invokeMethodReferenceInstance = new GenericInstanceMethod(weaver.Imports.ForeignClass_DefaultAlloc__T);
				invokeMethodReferenceInstance.GenericArguments.Add(module.ForType);
				var imported = weaver.MainModule.ImportReference(invokeMethodReferenceInstance);
				
				il.Emit(OpCodes.Call, imported);
			}
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Call, weaver.Imports.Class_ctor__Attributes_string_string_ForeignClass_ClassBody);
			il.Emit(OpCodes.Nop);

			// this.{Init}();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, module.InitMethod);
			il.Emit(OpCodes.Nop);

			il.Emit(OpCodes.Ret);
		}

		public void Process(WrenchWeaver weaver)
		{
			for (int i = 0; i < Classes.Count; i++)
			{
				var classData = Classes[i];
				var body = classData.InitMethod.Body;
				
				body.Instructions.Clear();
				body.Variables.Clear();
				var il = body.GetILProcessor();
				
				
				for (int j = 0; j < classData.Methods.Count; j++)
				{
					var methodData = classData.Methods[j];
					
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldc_I4_S, (sbyte)methodData.MethodType);
					il.Emit(OpCodes.Ldstr, methodData.UserMethod.Name);
					il.Emit(OpCodes.Ldc_I4_S, (sbyte) (methodData.Parameters.Count - 1));
					il.Emit(OpCodes.Call, weaver.Imports.Signature_Create__MethodType_string_int);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldftn, methodData.WrapperMethod);
					il.Emit(OpCodes.Newobj, weaver.Imports.ForeignAction_ctor);
					il.Emit(OpCodes.Newobj, weaver.Imports.ForeignMethod_ctor__ForeignAction);
					il.Emit(OpCodes.Newobj, weaver.Imports.Method_ctor__Signature_ForeignMethod);
					il.Emit(OpCodes.Call, weaver.Imports.Class_Add__IClassScoped);
					il.DEBUG_EmitNop();
				}
				
				il.Emit(OpCodes.Ret);
			}
		}
	}
}
