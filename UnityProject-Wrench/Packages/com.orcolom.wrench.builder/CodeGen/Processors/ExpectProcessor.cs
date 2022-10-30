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

				var param1 = method.Parameters[1];
				if (param1.ParameterType.Is<Slot>() == false) sb.AppendLine("- Parameter 1 should be Slot");

				var param2 = method.Parameters[2];

				if (expect.ForType.IsGenericInstance)
				{
					weaver.Logger.Log(DebugPrinter.Encoded(sb => DebugPrinter.Print(sb, 0, method)));
				}
				
				// (0,0): warning System.Boolean Binding.UnityModule::ExpectObject( [...] ParameterReference(  ByReferenceTypes(  Wrench.ForeignObject`1(  GenericParameters(  T(  )  )  )  )  )  GenericParameters(  T(  Constraints(  UnityEngine.Object(  )  )  )  )  

				if (expect.ForType.IsGenericInstance || method.HasGenericParameters || param2.ParameterType.ContainsGenericParameter)
				{
					var attributeType = (expect.ForType as GenericInstanceType)?.GenericArguments[0];
					var methodParameter = method.GenericParameters[0];
					var methodConstraint = methodParameter.Constraints[0];
					
					TypeReference dereffedParameter = param2.ParameterType is ByReferenceType t ? t.ElementType : param2.ParameterType;
					var parameterType = (dereffedParameter as GenericInstanceType)?.GenericArguments[0];

					if (dereffedParameter.Is(weaver.Imports.ForeignObject) == false)
					{
						sb.AppendLine("- generic doesnt derive from ForeignObject");
					}
					else if (attributeType == null) sb.AppendLine("- no valid attribute type");
					else if (methodConstraint == null) sb.AppendLine("- invalid method constraint");
					else if (parameterType == null) sb.AppendLine("- invalid parameter type");
					else if (attributeType.Is(methodConstraint) == false)
					{
						sb.AppendLine("- constraints do not equal");
						// weaver.Logger.Log($"{attributeType} {methodConstraint} {parameterType} {methodParameter}");
					}
				}

				if (param2.ParameterType.Is(expect.ForType) == false)
				{
					weaver.Logger.Log(param2.ParameterType.FullName);
					weaver.Logger.Log(param2.ParameterType.GetElementType().FullName);
					weaver.Logger.Log(param2.ParameterType.IsArray.ToString());
					weaver.Logger.Log(expect.ForType.FullName);
					weaver.Logger.Log(expect.ForType.GetElementType().FullName);
					weaver.Logger.Log(expect.ForType.IsArray.ToString());
					sb.AppendLine("- Parameter 2 should be same as attribute");
				}

				if (param2.IsOut == false) sb.AppendLine("- Parameter 2 should have out");
			}

			if (sb.Length == 0) return true;
			weaver.Logger.Error(
				$"{nameof(WrenchExpectAttribute)} expects to be on a method with signature `public static bool MethodName(in Vm vm, in Slot slot, out {expect.ForType.Name} value)`. {sb}");
			return false;
		}

		public bool EmitExpectIl(WrenchWeaver weaver, MethodDefinition method, ILProcessor il, TypeReference forType,
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
			il.Emit(OpCodes.Ldflda, slot);
			il.Emit(OpCodes.Ldloca_S, localVar);
			if (existingData.ForType.IsDerivedFrom(forType) == true)
			{
			}
			else
			{
				il.Emit(OpCodes.Ldloca_S, localVar);
			}
			il.Emit(OpCodes.Call, existingData.Method);
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Ceq);
			il.Emit(OpCodes.Stloc_S, localBool);
			il.Emit(OpCodes.Ldloc_S, localBool);
			il.Emit(OpCodes.Brtrue, lastInstruction);

			return true;
		}

		public WrenchExpectDefinition FindDefinition(TypeReference type)
		{
			int foundDepth = int.MaxValue;
			WrenchExpectDefinition found = null;
			for (int i = 0; i < Definitions.Count; i++)
			{
				int currentDepth = 0;
				var definition = Definitions[i];
				if (definition.ForType.IsDerivedFrom(type, ref currentDepth) == false)continue;
				if (currentDepth < foundDepth) found = definition;
			}

			return found;
		}
	}
}
