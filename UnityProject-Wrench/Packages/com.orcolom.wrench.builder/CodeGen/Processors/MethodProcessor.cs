using Mono.Cecil;
using Mono.Cecil.Cil;
using Wrench.Builder;
using Wrench.Weaver;

namespace Wrench.CodeGen.Processors
{
	public class WrenchMethodDefinition
	{
		public MethodType MethodType;
		public MethodDefinition WrapperMethod;
		public MethodDefinition UserMethod;
	}

	public class MethodProcessor
	{
		public bool TryExtract(WrenchWeaver weaver, TypeDefinition @class, MethodDefinition input, out WrenchMethodDefinition data)
		{
			data = new WrenchMethodDefinition
			{
			};
			
			// implements attribute and has valid arguments
			if (input.HasAttribute<WrenchMethodAttribute>(out var attribute) == false) return false;
			if (attribute.HasConstructorArguments == false
				|| attribute.ConstructorArguments[0].Value is not int intValue)
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchMethodAttribute)}` but doesnt have a valid `{nameof(WrenchMethodAttribute.MethodType)}`",
					input);
				return false;
			}

			data.MethodType = (MethodType)intValue;

			if (input.IsConstructor) return weaver.Logger.Error($"`{input.FullName}` is an constructor and not an method", input);
			if (input.IsVirtual) return weaver.Logger.Error($"`{input.FullName}` is virtual method and not an basic method", input);
			if (input.IsAbstract) return weaver.Logger.Error($"`{input.FullName}` is an abstract method and not an basic method", input);

			if (input.HasParameters == false ||
				input.Parameters[0].ParameterType.Is<Vm>() == false)
			{
				return weaver.Logger.Error($"`{input.FullName}` does not implement {nameof(Vm)} as it's first parameter", input);
			}

			data.UserMethod = input;
			CreateWrapperMethod(weaver, data);
			@class.Methods.Add(data.WrapperMethod);

			return true;
		}

		private void CreateWrapperMethod(WrenchWeaver weaver, WrenchMethodDefinition data)
		{
			var wrapperMethod = new MethodDefinition($"{WrenchWeaver.Prefix}{data.UserMethod.Name}", 
				MethodAttributes.Private | MethodAttributes.HideBySig,
				weaver.Imports.VoidRef);
			data.WrapperMethod = wrapperMethod;
			
			wrapperMethod.Parameters.Add(new ParameterDefinition("vm",ParameterAttributes.In, weaver.Imports.Vm));
			
			// assumes that it has an empty body
			wrapperMethod.Body.Instructions.Clear();
			wrapperMethod.Body.Variables.Clear();

			var il = wrapperMethod.Body.GetILProcessor();
			
			// this.{name}(vm, {...});
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, data.UserMethod);
			il.Emit(OpCodes.Nop);
			
			il.Emit(OpCodes.Ret);
		}
	}
}
