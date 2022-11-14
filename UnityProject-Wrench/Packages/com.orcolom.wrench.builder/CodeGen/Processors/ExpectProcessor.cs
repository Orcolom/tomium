using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Wrench.Builder;
using Wrench.Weaver;

namespace Wrench.CodeGen.Processors
{
	public class WrenchExpectDefinition
	{
		public MethodReference Method;
		public TypeReference ForType;
		public bool UseForChildren;
	}

	public class ExpectProcessor
	{
		private readonly List<WrenchExpectDefinition> _definitions = new List<WrenchExpectDefinition>();

		public bool TryExtract(WrenchWeaver weaver, MethodDefinition input)
		{
			var data = new WrenchExpectDefinition();

			if (CanValidateAttribute(weaver, input, data) == false) return false;
			if (CanValidateMethod(weaver, input, data) == false) return false;

			_definitions.Add(data);
			weaver.Logger.Log($"extracted expect: {input}");
			return true;
		}

		private bool CanValidateAttribute(WrenchWeaver weaver, MethodDefinition input, WrenchExpectDefinition data)
		{
			using var log = weaver.Logger.ErrorGroup($"extract {input} CanValidateAttribute");
			
			if (input.HasAttribute<WrenchExpectAttribute>(out var attribute) == false) return false;
			if (attribute.HasConstructorArguments == false) log.Error("attribute has no constructor arguments");

			if (attribute.ConstructorArguments[0].Value is not TypeReference forType) log.Error("arg 0 is not a Type");
			else data.ForType = weaver.MainModule.ImportReference(forType);

			if (attribute.ConstructorArguments[1].Value is not bool useForChildren) log.Error("arg 1 is not a bool");
			else data.UseForChildren = useForChildren;

			var existingData = _definitions.Find(definition => definition.ForType.Is(data.ForType));
			if (existingData != null)
			{
				log.Error($"found multiple {nameof(WrenchExpectAttribute)} for type {data.ForType.FullName}");
				return false;
			}
			
			return log.HasIssues == false;
		}

		private bool CanValidateMethod(WrenchWeaver weaver, MethodDefinition input, WrenchExpectDefinition data)
		{
			using var log =
				weaver.Logger.ErrorGroup(
					$"extract {input} CanValidateMethod expects a method with signature `public static bool MethodName(in Vm vm, in Slot slot, out {data.ForType.Name} value)`");

			data.Method = weaver.MainModule.ImportReference(input);
			
			if (input.IsPublic == false) log.Error("Method is not public");
			if (input.IsStatic == false) log.Error("Method is not static");
			if (input.ReturnType.Is<bool>() == false) log.Error("Return value should be bool");
			if (input.Parameters.Count != 3) log.Error("wrong parameter count");
			else
			{
				var param0 = input.Parameters[0];
				if (param0.ParameterType.Is<Vm>() == false) log.Error("Parameter 0 should be Vm");
				if (param0.IsIn || param0.IsOut || param0.IsOptional)
					log.Error("Parameter 0 can not be `in`, `out` or `ref`");

				var param1 = input.Parameters[1];
				if (param1.ParameterType.Is<Slot>() == false) log.Error("Parameter 1 should be Slot");
				if (param1.IsIn || param1.IsOut || param1.IsOptional)
					log.Error("Parameter 1 can not be `in`, `out` or `ref`");

				var param2 = input.Parameters[2];
				if (param2.IsIn || param2.IsOut == false || param2.IsOptional)
					log.Error("Parameter 2 can not be `in` or `ref` and should be `out`");

				if (data.ForType.IsGenericInstance || input.HasGenericParameters ||
					param2.ParameterType.ContainsGenericParameter)
				{
					var attributeType = (data.ForType as GenericInstanceType)?.GenericArguments[0];
					var methodParameter = input.HasGenericParameters ? input.GenericParameters[0] : null;
					var methodConstraint = (methodParameter?.HasConstraints ?? false) ? methodParameter.Constraints[0] : null;

					TypeReference dereffedParameter =
						param2.ParameterType is ByReferenceType t ? t.ElementType : param2.ParameterType;
					var parameterType = (dereffedParameter as GenericInstanceType)?.GenericArguments[0];
					parameterType = weaver.MainModule.ImportReference(parameterType);

					if (dereffedParameter.Is(weaver.Imports.ForeignObject, true) == false)
					{
						log.Error("generic doesnt derive from ForeignObject");
					}
					else if (attributeType == null) log.Error("no valid attribute type");
					else if (parameterType == null) log.Error("invalid parameter type");
					else if (methodConstraint != null && attributeType.Is(methodConstraint) == false)
					{
						log.Error("constraints do not equal");
					}
				}

				if (param2.ParameterType.Is(data.ForType, true) == false)
				{
					log.Error("Parameter 2 should be same as attribute");
				}
			}

			return log.HasIssues == false;
		}

		public bool EmitExpectIl(WrenchWeaver weaver, MethodDefinition method, ILProcessor il, TypeReference forType,
			int index, MethodDefinition called,
			VariableDefinition localVar, FieldReference slot, Instruction lastInstruction)
		{
			var existingData = FindDefinition(weaver, forType);
			if (existingData == null)
			{
				weaver.Logger.Error(
					$"There is no method that implements {nameof(WrenchExpectAttribute)} for {forType.FullName}");
				return false;
			}

			var localBool = new VariableDefinition(weaver.Imports.Bool);
			method.Body.Variables.Add(localBool);
			il.Emit_Ldarg_x(1, method);
			il.Emit_Ldarg_x(1, method);
			il.Emit(OpCodes.Ldfld, slot);
			il.Emit(OpCodes.Ldloca_S, localVar);

			if (existingData.Method.HasGenericParameters)
			{
				var genericMethod = new GenericInstanceMethod(existingData.Method);
				genericMethod.GenericArguments.Add(((GenericInstanceType) localVar.VariableType).GenericArguments[0]);
				il.Emit(OpCodes.Call, genericMethod);
			}
			else il.Emit(OpCodes.Call, existingData.Method);

			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Ceq);
			il.Emit(OpCodes.Stloc_S, localBool);
			il.Emit(OpCodes.Ldloc_S, localBool);
			il.Emit(OpCodes.Brtrue, lastInstruction);

			return true;
		}

		private readonly Dictionary<string, WrenchExpectDefinition> _cachedFinds = new();

		private WrenchExpectDefinition FindDefinition(WrenchWeaver weaver, TypeReference type)
		{
			if (_cachedFinds.TryGetValue(type.FullName, out var value)) return value;

			var normalizedType = type;
			if (type is GenericInstanceType g) normalizedType = g.GenericArguments[0];

			int foundDepth = int.MaxValue;
			WrenchExpectDefinition found = null;
			for (int i = 0; i < _definitions.Count; i++)
			{
				int currentDepth = 0;
				var definition = _definitions[i];

				var forType = definition.ForType;
				if (definition.ForType is GenericInstanceType genericType) forType = genericType.GenericArguments[0];

				if (normalizedType.IsDerivedFrom(forType, ref currentDepth) == false) continue;
				if (currentDepth < foundDepth) found = definition;
			}

			_cachedFinds.Add(type.FullName, found);
			return found;
		}
	}
}
