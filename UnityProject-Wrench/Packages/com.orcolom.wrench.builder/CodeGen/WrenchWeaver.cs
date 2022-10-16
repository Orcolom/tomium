﻿using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;
using Wrench.Builder;
using Wrench.CodeGen.Processors;
using Wrench.Weaver;
using Class = Wrench.Builder.Class;
using Imports = Wrench.Weaver.Imports;

namespace Wrench.CodeGen
{
	public class WrenchWeaver : AWeaver<WrenchImports>
	{
		public const string Prefix = "Wrench__";

		public WrenchWeaver(WeaverLogger logger) : base(logger) { }

		protected override void Weave()
		{
			var moduleProcessor = new ModuleProcessor();
			var classProcessor = new ClassProcessor();
			var methodProcessor = new MethodProcessor();

			var module = MainModule;

			// collect all info
			for (int i = 0; i < module.Types.Count; i++)
			{
				var input = module.Types[i];

				Logger.Log(input.ToString());

				if (moduleProcessor.TryExtract(this, input)) continue;
				if (classProcessor.TryExtract(this, input, out var classDefinition))
				{
					for (int j = 0; j < input.Methods.Count; j++)
					{
						methodProcessor.TryExtract(this, classDefinition, input.Methods[j]);
					}
				}
			}

			methodProcessor.Process(this);
			classProcessor.Process(this);
			moduleProcessor.Process(this, classProcessor);
		}
	}

	public class WrenchImports : Imports
	{
		public TypeReference Vm;
		public FieldReference[] Vm_Slots = new FieldReference[17];
		public TypeReference Slot;
		public MethodReference VmUtils_EnsureSlots__VM_int;


		public TypeReference Module;
		public MethodReference Module_ctor__string;
		public MethodReference Module_Add__IModuleScoped;

		public TypeReference Class;
		public TypeReference ForeignClass;
		public MethodReference Class_ctor__Attributes_string_string_ForeignClass_ClassBody;
		public MethodReference Class_Add__IClassScoped;
		public MethodReference Signature_Create__MethodType_string_int;

		public MethodReference Method_ctor__Signature_ForeignMethod;
		public MethodReference ForeignAction_ctor;
		public MethodReference ForeignMethod_ctor__ForeignAction;

		public override bool Populate(WeaverLogger logger, ModuleDefinition moduleDefinition)
		{
			if (base.Populate(logger, moduleDefinition) == false) return false;

			ForeignClass = moduleDefinition.ImportReference(typeof(ForeignClass));
			Slot = moduleDefinition.ImportReference(typeof(Slot));

			// import vm
			Vm = moduleDefinition.ImportReference(typeof(Vm));
			var vm = Vm.Resolve();
			for (int i = 0; i < vm.Fields.Count; i++)
			{
				var field = vm.Fields[i];

				if (field.FieldType.Is<Slot>() == false) continue;
				if (field.Name.StartsWith("Slot") == false) continue;

				if (int.TryParse(field.Name.Substring("Slot".Length), out int value))
				{
					Vm_Slots[value] = moduleDefinition.ImportReference(field);
				}
			}

			var vmUtils = moduleDefinition.ImportReference(typeof(VmUtils)).Resolve();
			for (int i = 0; i < vmUtils.Methods.Count; i++)
			{
				var method = vmUtils.Methods[i];
				if (method.Name != nameof(VmUtils.EnsureSlots)) continue;
				VmUtils_EnsureSlots__VM_int = moduleDefinition.ImportReference(method);
				break;
			}

			// import Module
			Module = moduleDefinition.ImportReference(typeof(Module));
			var module = Module.Resolve();

			for (int i = 0; i < module.Methods.Count; i++)
			{
				var method = module.Methods[i];

				if (method.IsConstructor && method.Parameters.Count == 1 && method.Parameters[0].ParameterType.Is<string>())
				{
					Module_ctor__string = moduleDefinition.ImportReference(method);
				}

				if (method.Name == nameof(Builder.Module.Add) && method.Parameters.Count == 1
					&& method.Parameters[0].ParameterType.Is<IModuleScoped>())
				{
					Module_Add__IModuleScoped = moduleDefinition.ImportReference(method);
				}
			}

			if (Module_ctor__string == null)
			{
				logger.Error($"could not find {nameof(Module_ctor__string)}");
				return false;
			}

			// import Class
			Class = moduleDefinition.ImportReference(typeof(Class));
			var @class = Class.Resolve();

			for (int i = 0; i < @class.Methods.Count; i++)
			{
				var method = @class.Methods[i];

				if (method.IsConstructor && method.HasParameters
					&& method.Parameters[0].ParameterType.Is<Attributes>()
					&& method.Parameters[1].ParameterType.Is<string>()
					&& method.Parameters[2].ParameterType.Is<string>()
					&& method.Parameters[3].ParameterType.Is<ForeignClass>()
					&& method.Parameters[4].ParameterType.Is<ClassBody>())
				{
					Class_ctor__Attributes_string_string_ForeignClass_ClassBody = moduleDefinition.ImportReference(method);
				}

				if (method.IsStatic == false && method.Name == nameof(Builder.Class.Add) && method.HasParameters
					&& method.Parameters[0].ParameterType.Is<IClassScoped>())
				{
					Class_Add__IClassScoped = moduleDefinition.ImportReference(method);
				}
			}

			// import signature
			var signature = moduleDefinition.ImportReference(typeof(Signature)).Resolve();
			for (int i = 0; i < signature.Methods.Count; i++)
			{
				var method = signature.Methods[i];
				if (method.Name == nameof(Signature.Create))
				{
					Signature_Create__MethodType_string_int = moduleDefinition.ImportReference(method);
				}
			}

			// import foreign action
			var foreignAction = moduleDefinition.ImportReference(typeof(ForeignAction)).Resolve();
			for (int i = 0; i < foreignAction.Methods.Count; i++)
			{
				var method = foreignAction.Methods[i];
				if (method.IsConstructor)
				{
					ForeignAction_ctor = moduleDefinition.ImportReference(method);
				}
			}

			// import foreign method
			var foreignMethod = moduleDefinition.ImportReference(typeof(ForeignMethod)).Resolve();
			for (int i = 0; i < foreignMethod.Methods.Count; i++)
			{
				var method = foreignMethod.Methods[i];
				if (method.IsStatic == false && method.IsConstructor && method.HasParameters)
				{
					ForeignMethod_ctor__ForeignAction = moduleDefinition.ImportReference(method);
				}
			}

			// import method
			var builder_method = moduleDefinition.ImportReference(typeof(Method)).Resolve();
			for (int i = 0; i < builder_method.Methods.Count; i++)
			{
				var method = builder_method.Methods[i];
				if (method.IsStatic == false && method.IsConstructor && method.HasParameters && method.Parameters.Count == 2
					&& method.Parameters[0].ParameterType.Is<Signature>() &&
					method.Parameters[1].ParameterType.Is<ForeignMethod>())
				{
					Method_ctor__Signature_ForeignMethod = moduleDefinition.ImportReference(method);
				}
			}

			logger.Log("Found all imports");
			return true;
		}
	}
}
