using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Wrench.Builder;
using Wrench.Weaver;

namespace Wrench.CodeGen.Processors
{
	public class WrenchMethodDefinition
	{
		public MethodType MethodType;
		public List<TypeReference> Parameters;
		public MethodDefinition WrapperMethod;
		public MethodDefinition UserMethod;
	}

	public class MethodProcessor
	{
		private readonly List<WrenchMethodDefinition> _methods = new List<WrenchMethodDefinition>();

		public void TryExtract(WrenchWeaver weaver, WrenchClassDefinition @class, MethodDefinition input)
		{
			var data = new WrenchMethodDefinition
			{
				Parameters = new List<TypeReference>(),
			};

			if (CanValidateAttribute(weaver, input, data) == false) return;
			if (CanValidateMethod(weaver, input, data) == false) return;

			CreateWrapperMethod(weaver, data);

			@class.AddMethod(data);
			weaver.Logger.Log($"extracted method: {input}");
			_methods.Add(data);
		}

		private bool CanValidateAttribute(WrenchWeaver weaver, MethodDefinition input, WrenchMethodDefinition data)
		{
			using var log = weaver.Logger.ErrorGroup($"extract {input} CanValidateAttribute");

			// implements attribute and has valid arguments
			if (input.HasAttribute<WrenchMethodAttribute>(out var attribute) == false) return false;
			if (attribute.HasConstructorArguments == false) log.Error("attribute has no constructor arguments");
			if (attribute.ConstructorArguments[0].Value is not int intValue) log.Error("Arg0 is expected to be an int");
			else data.MethodType = (MethodType) intValue;

			return log.HasIssues == false;
		}

		private bool CanValidateMethod(WrenchWeaver weaver, MethodDefinition input, WrenchMethodDefinition data)
		{
			using var log = weaver.Logger.ErrorGroup($"extract {input} CanValidateMethod");

			if (input.IsConstructor) log.Error("is an constructor not an method");
			if (input.IsVirtual) log.Error("is a virtual method");
			if (input.IsAbstract) log.Error("is an abstract method");
			if (input.HasParameters == false) log.Error("has no parameters. first should be `Vm`");
			else
			{
				if (input.Parameters[0].ParameterType.Is<Vm>() == false) log.Error("param 0 should be `Vm`");
				if (input.Parameters.Count > 18) log.Error("can have a maximum of 17 parameters");

				for (int i = 0; i < input.Parameters.Count; i++)
				{
					var param = input.Parameters[i];
					if (param.IsIn || param.IsOut || param.IsOptional)
					{
						log.Error("parameters can be `in`, `out`, or `ref`");
						break;
					}
				}
			}

			data.UserMethod = input;
			return log.HasIssues == false;
		}

		private void CreateWrapperMethod(WrenchWeaver weaver, WrenchMethodDefinition data)
		{
			for (int i = 1; i < data.UserMethod.Parameters.Count; i++)
			{
				var param = data.UserMethod.Parameters[i];
				data.Parameters.Add(param.ParameterType);
			}

			var wrapperMethod = new MethodDefinition(
				$"{WrenchWeaver.Prefix}{data.UserMethod.Name}__{data.UserMethod.FullName.GetStableHashCode()}",
				MethodAttributes.Private,
				weaver.Imports.Void)
			{
				// NOTE: Marshal.GetFunctionPointerForDelegate doesn't like il static methods? for ease lets not mimic UserMethods static 
				IsStatic = false,
			};

			data.WrapperMethod = wrapperMethod;

			wrapperMethod.Parameters.Add(new ParameterDefinition("vm", ParameterAttributes.None, weaver.Imports.Vm));
		}

		public void Process(WrenchWeaver weaver, ExpectProcessor expectProcessor)
		{
			using var _ = weaver.Logger.Sample("Weave.MethodProcessor.Process");

			for (int i = 0; i < _methods.Count; i++)
			{
				var methodData = _methods[i];
				var method = methodData.WrapperMethod;
				var body = method.Body;

				body.Instructions.Clear();
				body.Variables.Clear();

				var il = body.GetILProcessor();
				var lastInstruction = Instruction.Create(OpCodes.Ret);

				// ensure slot size
				il.Emit_Ldarg_x(1, method);
				il.Emit(OpCodes.Ldc_I4_S, (sbyte) methodData.Parameters.Count);
				il.Emit(OpCodes.Call, weaver.Imports.VmUtils_EnsureSlots__VM_int);
				il.DEBUG_EmitNop();

				// load parameters in local variables 
				for (int j = 0; j < methodData.Parameters.Count; j++)
				{
					var forType = methodData.Parameters[j];
					bool expectsSlot = forType.Is<Slot>();

					var localVar = new VariableDefinition(expectsSlot ? weaver.Imports.Slot : forType);
					body.Variables.Add(localVar);
				}

				// load parameters in local variables 
				for (int j = 0; j < methodData.Parameters.Count; j++)
				{
					var forType = methodData.Parameters[j];
					bool expectsSlot = forType.Is<Slot>();

					var localVar = body.Variables[j];

					var slot = weaver.Imports.Vm_Slots[j];
					if (expectsSlot)
					{
						il.Emit_Ldarg_x(1, method);
						il.Emit(OpCodes.Ldfld, slot);
						il.Emit(OpCodes.Stloc_S, localVar);
					}
					else
					{
						expectProcessor.EmitExpectIl(weaver, method, il, forType, j, methodData.UserMethod, localVar, slot,
							lastInstruction);
					}

					il.DEBUG_EmitNop();
				}

				// load all local variables
				if (methodData.UserMethod.IsStatic == false) il.Emit_Ldarg_x(0, method);
				il.Emit_Ldarg_x(1, method);
				for (int j = 0; j < methodData.Parameters.Count; j++)
				{
					il.Emit(OpCodes.Ldloc_S, body.Variables[j]);
				}

				// call method
				il.Emit(OpCodes.Call, methodData.UserMethod);
				il.DEBUG_EmitNop();

				body.Instructions.Add(lastInstruction);
			}
		}
	}
}
