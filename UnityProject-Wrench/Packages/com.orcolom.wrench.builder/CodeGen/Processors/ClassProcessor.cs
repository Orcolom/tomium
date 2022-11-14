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
		public string Inherit;
		public TypeReference ModuleType;
		public TypeDefinition ClassType;
		public TypeReference ForType;
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

		public readonly List<WrenchClassDefinition> Classes = new List<WrenchClassDefinition>();

		public bool TryExtract(WrenchWeaver weaver, TypeDefinition input, out WrenchClassDefinition data)
		{
			data = new WrenchClassDefinition()
			{
				ClassType = input,
			};

			if (CanValidateAttribute(weaver, input, data) == false) return false;
			if (CanValidateType(weaver, input, data) == false) return false;

			// create initializer method
			data.InitMethod = new MethodDefinition(InitializerMethodName,
				MethodAttributes.HideBySig | MethodAttributes.Private,
				weaver.Imports.Void);
			input.Methods.Add(data.InitMethod);

			var il = data.InitMethod.Body.GetILProcessor();
			il.Emit(OpCodes.Nop);
			il.Emit(OpCodes.Ret);

			WeaveInitMethodOnConstructor(weaver, data.CtorMethod, data);
			Classes.Add(data);
			weaver.Logger.Log($"extracted class: {input}");
			return true;
		}

		private static bool CanValidateAttribute(WrenchWeaver weaver, TypeDefinition input, WrenchClassDefinition data)
		{
			using var log = weaver.Logger.ErrorGroup($"extract {input} CanValidateAttribute");

			if (input.HasAttribute<WrenchClassAttribute>(out var attribute) == false) return false;
			if (attribute.HasConstructorArguments == false) log.Error("attribute has no constructor arguments");

			if (attribute.ConstructorArguments[0].Value is not TypeDefinition moduleType) log.Error("Arg 0 is not a Type");
			else if (moduleType.IsDerivedFrom<Module>() == false) log.Error($"Arg 0 doesnt derive from {nameof(Module)}");
			else data.ModuleType = weaver.MainModule.ImportReference(moduleType);

			if (attribute.ConstructorArguments[1].Value is not string str) log.Error("Arg 1 is not string");
			else if (string.IsNullOrEmpty(str)) log.Error("Arg 1 can not be null or empty");
			else data.Name = str;


			if (attribute.ConstructorArguments[2].Value is TypeReference type)
			{
				data.ForType = weaver.MainModule.ImportReference(type);
			}

			if (attribute.ConstructorArguments[3].Value is string strInherit)
			{
				if (string.IsNullOrEmpty(strInherit)) log.Error("Arg 3 can not be empty");
				else data.Inherit = strInherit;
			}

			return log.HasIssues == false;
		}

		private static bool CanValidateType(WrenchWeaver weaver, TypeDefinition input, WrenchClassDefinition data)
		{
			using var log = weaver.Logger.ErrorGroup($"extract {input} CanValidateType");

			// derives from module class
			if (input.IsDerivedFrom<Class>() == false) log.Error($"Has to derive from {nameof(Class)}");

			for (int i = 0; i < input.Methods.Count; i++)
			{
				var method = input.Methods[i];

				if (method.IsConstructor == false) continue;

				// allow an default constructor
				if (method.HasParameters == false)
				{
					data.CtorMethod = method;
					continue;
				}

				log.Error("has non-default constructors. this is not supported at the moment");
				break;
			}

			return log.HasIssues == false;
		}

		private static void WeaveInitMethodOnConstructor(WrenchWeaver weaver, MethodDefinition constructor,
			WrenchClassDefinition module)
		{
			using var _ = new ConstructorInjector(constructor, out var il);

			// base..ctor((Attributes) null, {Name}, {Inherit}, {ForeignClass}, (ClassBody) null);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Ldstr, module.Name);
			if (string.IsNullOrEmpty(module.Inherit)) il.Emit(OpCodes.Ldnull);
			else il.Emit(OpCodes.Ldstr, module.Inherit);
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
		}

		public void Process(WrenchWeaver weaver)
		{
			using var _ = weaver.Logger.Sample("Weave.ClassProcessor.Process");

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
					il.Emit(OpCodes.Ldc_I4_S, (sbyte) methodData.MethodType);
					il.Emit(OpCodes.Ldstr, methodData.UserMethod.Name);
					il.Emit(OpCodes.Ldc_I4_S, (sbyte) (methodData.Parameters.Count - 1));
					il.Emit(OpCodes.Call, weaver.Imports.Signature_Create__MethodType_string_int);
					il.Emit(methodData.WrapperMethod.IsStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldftn, methodData.WrapperMethod);
					il.Emit(OpCodes.Newobj, weaver.Imports.ForeignAction_ctor);
					il.Emit(OpCodes.Ldstr, $"{methodData.UserMethod.DeclaringType.Name}.{methodData.UserMethod.Name}");
					il.Emit(OpCodes.Newobj, weaver.Imports.ForeignMethod_ctor__ForeignAction_String);
					il.Emit(OpCodes.Newobj, weaver.Imports.Method_ctor__Signature_ForeignMethod);
					il.Emit(OpCodes.Call, weaver.Imports.Class_Add__IClassScoped);
					il.DEBUG_EmitNop();
				}

				il.Emit(OpCodes.Ret);
			}
		}
	}
}
