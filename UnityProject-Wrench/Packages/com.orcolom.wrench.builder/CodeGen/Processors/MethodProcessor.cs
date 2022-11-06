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
		public List<WrenchMethodDefinition> Methods = new List<WrenchMethodDefinition>();

		public void TryExtract(WrenchWeaver weaver, WrenchClassDefinition @class, MethodDefinition input)
		{
			var data = new WrenchMethodDefinition
			{
				Parameters = new List<TypeReference>(),
			};

			// implements attribute and has valid arguments
			if (input.HasAttribute<WrenchMethodAttribute>(out var attribute) == false) return;
			if (attribute.HasConstructorArguments == false
				|| attribute.ConstructorArguments[0].Value is not int intValue)
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchMethodAttribute)}` but doesnt have a valid `{nameof(WrenchMethodAttribute.MethodType)}`",
					input);
				return;
			}

			data.MethodType = (MethodType) intValue;

			if (input.IsConstructor)
			{
				weaver.Logger.Error($"`{input.FullName}` is an constructor and not an method", input);
				return;
			}

			if (input.IsVirtual)
			{
				weaver.Logger.Error($"`{input.FullName}` is virtual method and not an basic method", input);
				return;
			}

			if (input.IsAbstract)
			{
				weaver.Logger.Error($"`{input.FullName}` is an abstract method and not an basic method", input);
				return;
			}

			if (input.HasParameters == false ||
				input.Parameters[0].ParameterType.Is<Vm>() == false)
			{
				weaver.Logger.Error($"`{input.FullName}` does not implement {nameof(Vm)} as it's first parameter", input);
				return;
			}

			if (input.Parameters.Count > 18)
			{
				weaver.Logger.Error($"`{input.FullName}` has to many parameters max 17. current:{input.Parameters.Count}",
					input);
				return;
			}

			for (int i = 0; i < input.Parameters.Count; i++)
			{
				var param = input.Parameters[i];
				if (param.IsIn || param.IsOut || param.IsOptional)
				{
					weaver.Logger.Error($"`{input.FullName}` is not allwed to have any `in`, `out` or `ref` parameters",
						input);
					return;
				}
			}

			data.UserMethod = input;
			CreateWrapperMethod(weaver, data);

			@class.AddMethod(data);
			Methods.Add(data);
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
			for (int i = 0; i < Methods.Count; i++)
			{
				var methodData = Methods[i];
				var method = methodData.WrapperMethod;
				var body = method.Body;

				body.Instructions.Clear();
				body.Variables.Clear();

				var il = body.GetILProcessor();
				var lastInstruction = Instruction.Create(OpCodes.Ret);

				// ensure slot size
				il.Emit_Ldarg_x(1, method);
				il.Emit(OpCodes.Ldc_I4_S, (sbyte)methodData.Parameters.Count);
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
						expectProcessor.EmitExpectIl(weaver, method, il, forType, j, methodData.UserMethod, localVar, slot, lastInstruction);
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
