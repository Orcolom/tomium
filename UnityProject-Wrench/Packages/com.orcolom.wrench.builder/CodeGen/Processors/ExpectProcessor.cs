using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor.TerrainTools;
using Wrench.Builder;
using Wrench.Weaver;

namespace Wrench.CodeGen.Processors
{
	public class WrenchExpectDefinition
	{
		public MethodReference Method;
		public TypeReference ForType;
		public bool UseForChildren;
		public bool UsesForeignObject;
	}

	public class ExpectProcessor
	{
		private List<WrenchExpectDefinition> Definitions = new List<WrenchExpectDefinition>();

		public bool TryExtract(WrenchWeaver weaver, MethodDefinition input)
		{
			var data = new WrenchExpectDefinition()
				{ };

			if (input.HasAttribute<WrenchExpectAttribute>(out var attribute) == false) return false;

			if (attribute.HasConstructorArguments == false ||
				attribute.ConstructorArguments[0].Value is not TypeReference forType ||
				attribute.ConstructorArguments[1].Value is not bool useForChildren)
			{
				return false;
			}

			data.Method = weaver.MainModule.ImportReference(input);
			data.ForType = forType;
			data.UseForChildren = useForChildren;

			var existingData = Definitions.Find(definition => definition.ForType.Is(forType));
			
			if (existingData != null)
			{
				weaver.Logger.Error($"found multiple {nameof(WrenchExpectAttribute)} for type {forType.FullName}");
				return false;
			}

			if (CanValidateMethod(weaver, input, data) == false) return false;

			data.UsesForeignObject = false;

			Definitions.Add(data);
			return true;
		}

		private bool CanValidateMethod(WrenchWeaver weaver, MethodDefinition method, WrenchExpectDefinition expect)
		{
			StringBuilder sb = new StringBuilder();
			if (method.IsPublic == false) sb.AppendLine("- Method is not public");
			if (method.IsStatic == false) sb.AppendLine("- Method is not static");
			if (method.ReturnType.Is<bool>() == false) sb.AppendLine("- Return value should be bool");
			if (method.Parameters.Count != 3) sb.AppendLine("- wrong parameter count");
			else
			{
				var param0 = method.Parameters[0];
				if (param0.ParameterType.Is<Vm>() == false) sb.AppendLine("- Parameter 0 should be Vm");
				if (param0.IsIn || param0.IsOut || param0.IsOptional) sb.AppendLine("- Parameter 0 can not be `in`, `out` or `ref`");

				var param1 = method.Parameters[1];
				if (param1.ParameterType.Is<Slot>() == false) sb.AppendLine("- Parameter 1 should be Slot");
				if (param1.IsIn || param1.IsOut || param1.IsOptional) sb.AppendLine("- Parameter 1 can not be `in`, `out` or `ref`");

				var param2 = method.Parameters[2];
				if (param2.IsIn || param2.IsOut == false || param2.IsOptional) sb.AppendLine("- Parameter 2 can not be `in` or `ref` and should be `out`");

				if (expect.ForType.IsGenericInstance || method.HasGenericParameters || param2.ParameterType.ContainsGenericParameter)
				{
					var attributeType = (expect.ForType as GenericInstanceType)?.GenericArguments[0];
					var methodParameter = method.HasGenericParameters ? method.GenericParameters[0] : null;
					var methodConstraint = (methodParameter?.HasConstraints ?? false) ? methodParameter.Constraints[0] : null;
					
					TypeReference dereffedParameter = param2.ParameterType is ByReferenceType t ? t.ElementType : param2.ParameterType;
					var parameterType = (dereffedParameter as GenericInstanceType)?.GenericArguments[0];

					if (dereffedParameter.Is(weaver.Imports.ForeignObject, true) == false)
					{
						sb.AppendLine("- generic doesnt derive from ForeignObject");
					}
					else if (attributeType == null) sb.AppendLine("- no valid attribute type");
					else if (parameterType == null) sb.AppendLine("- invalid parameter type");
					else if (methodConstraint != null && attributeType.Is(methodConstraint) == false)
					{
						sb.AppendLine("- constraints do not equal");
					}
				}

				if (param2.ParameterType.Is(expect.ForType, true) == false)
				{
					sb.AppendLine("- Parameter 2 should be same as attribute");
				}
			}

			if (sb.Length == 0) return true;
			weaver.Logger.Error(
				$"{nameof(WrenchExpectAttribute)} expects to be on a method with signature `public static bool MethodName(in Vm vm, in Slot slot, out {expect.ForType.Name} value)`. {sb}");
			return false;
		}

		public bool EmitExpectIl(WrenchWeaver weaver, MethodDefinition method, ILProcessor il, TypeReference forType,
			int index, MethodDefinition called,
			VariableDefinition localVar, FieldReference slot, Instruction lastInstruction)
		{
			var existingData = FindDefinition(forType);
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

		public WrenchExpectDefinition FindDefinition(TypeReference type)
		{
			if (type is GenericInstanceType g) type = g.ElementType;
			
			int foundDepth = int.MaxValue;
			WrenchExpectDefinition found = null;
			for (int i = 0; i < Definitions.Count; i++)
			{
				int currentDepth = 0;
				var definition = Definitions[i];

				var forType = definition.ForType;
				if (definition.ForType is GenericInstanceType genericType) forType = genericType.ElementType;
				
				if (forType.IsDerivedFrom(type, ref currentDepth) == false) continue;
				if (currentDepth < foundDepth) found = definition;
			}

			return found;
		}
	}
}
