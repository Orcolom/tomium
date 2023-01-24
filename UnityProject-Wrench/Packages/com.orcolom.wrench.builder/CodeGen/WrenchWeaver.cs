using Mono.Cecil;
using Wrench.Builder;
using Wrench.CodeGen.Processors;
using Wrench.Weaver;

namespace Wrench.CodeGen
{
	public class WrenchWeaver : AWeaver<WrenchImporter>
	{
		public const string Prefix = "Wrench__";

		public WrenchWeaver(WeaverLogger logger) : base(logger) { }

		protected override void Weave()
		{
			var expectProcessor = new ExpectProcessor();

			var moduleProcessor = new ModuleProcessor();
			var classProcessor = new ClassProcessor();
			var methodProcessor = new MethodProcessor();

			using (var _ = Logger.Sample("Weave.SearchModuleForTypes"))
			{
				for (int i = 0; i < MainModule.AssemblyReferences.Count; i++)
				{
					var assembly = MainModule.AssemblyResolver.Resolve(MainModule.AssemblyReferences[i]);

					for (int j = 0; j < assembly.Modules.Count; j++)
					{
						var module = assembly.Modules[j];
						SearchModuleForTypes(module, expectProcessor);
					}
				}

				SearchModuleForTypes(MainModule, expectProcessor);
			}

			using (var _ = Logger.Sample("Weave.TryExtract"))
			{
				// collect all info
				for (int i = 0; i < MainModule.Types.Count; i++)
				{
					var input = MainModule.Types[i];

					if (moduleProcessor.TryExtract(this, input)) continue;
					bool foundClass = classProcessor.TryExtract(this, input, out var classDefinition);

					for (int j = 0; j < input.Methods.Count; j++)
					{
						if (foundClass) methodProcessor.TryExtract(this, classDefinition, input.Methods[j]);
					}
				}
			}

			using (var _ = Logger.Sample("Weave.Process"))
			{
				methodProcessor.Process(this, expectProcessor);
				classProcessor.Process(this);
				moduleProcessor.Process(this, classProcessor);
			}
		}

		private void SearchModuleForTypes(ModuleDefinition module, ExpectProcessor expectProcessor)
		{
			using var _ = Logger.Sample($"Weave.SearchModuleForTypes.{module.Name}");
			if (module.Name == "netstandard.dll") return;
			if (module.Name.StartsWith("UnityEngine")) return;

			for (int k = 0; k < module.Types.Count; k++)
			{
				var type = module.Types[k];

				for (int l = 0; l < type.Methods.Count; l++)
				{
					var method = type.Methods[l];

					expectProcessor.TryExtract(this, method);
				}
			}
		}
	}

	public class WrenchImporter : WeaverImporter
	{
		// the naming is very intentional to descriptive what method it imports
		// by using {class}_{MethodName}__{parameters}
		// ReSharper disable InconsistentNaming

		public TypeReference Vm;
		public readonly FieldReference[] Vm_Slots = new FieldReference[17];
		public TypeReference Slot;
		public MethodReference VmUtils_EnsureSlots__VM_int;


		public TypeReference Module;
		public MethodReference Module_ctor__string;
		public MethodReference Module_Add__IModuleScoped;

		public TypeReference ForeignClass;
		public MethodReference ForeignClass_DefaultAlloc;

		public TypeReference ForeignObject;
		public MethodReference ForeignObject_As__T;

		public TypeReference Class;
		public MethodReference Class_ctor__Attributes_string_string_ForeignClass_ClassBody;
		public MethodReference Class_Add__IClassScoped;
		public MethodReference Signature_Create__MethodType_string_int;

		public MethodReference Method_ctor__Signature_ForeignMethod;
		public MethodReference ForeignAction_ctor;
		public MethodReference ForeignMethod_ctor__ForeignAction_String;

		public MethodReference Import_ctor__string_ImportVariableArray;
		public TypeReference ImportVariable;
		public MethodReference ImportVariable_ctor__string_string;

		// ReSharper restore InconsistentNaming

		public override bool Populate(WeaverLogger logger, ModuleDefinition moduleDefinition)
		{
			if (base.Populate(logger, moduleDefinition) == false) return false;

			using var helper = new ImportHelper(logger, moduleDefinition, "predefined imports issues");

			if (helper.ImportType<ForeignClass>(out ForeignClass))
			{
				var foreignClass = ForeignClass.Resolve();

				helper.ImportMethod(foreignClass, nameof(ForeignClass_DefaultAlloc),
					out ForeignClass_DefaultAlloc, definition =>
						definition.IsStatic
						&& definition.Name == nameof(global::Wrench.ForeignClass.DefaultAlloc));
			}

			if (helper.ImportType(typeof(ForeignObject<>), out ForeignObject))
			{
				var foreignClass = ForeignObject.Resolve();

				helper.ImportMethod(foreignClass, nameof(ForeignObject_As__T), out ForeignObject_As__T, definition =>
					definition.IsStatic == false
					&& definition.Name == nameof(ForeignObject<int>.As)); // int is just placeholder
			}

			helper.ImportType<Slot>(out Slot);

			// import vm
			if (helper.ImportType<Vm>(out Vm))
			{
				var vm = Vm.Resolve();

				for (int i = 0; i < Vm_Slots.Length; i++)
				{
					string name = $"Slot{i}";

					helper.ImportField(vm, name, out var slot, definition =>
						definition.FieldType.Is<Slot>()
						&& definition.Name == name);

					Vm_Slots[i] = slot;
				}
			}

			if (helper.ImportType(typeof(VmUtils), out var utilsRef))
			{
				var utils = utilsRef.Resolve();
				helper.ImportMethod(utils, nameof(VmUtils_EnsureSlots__VM_int), out VmUtils_EnsureSlots__VM_int, definition =>
					definition.Name == nameof(VmUtils.EnsureSlots));
			}

			if (helper.ImportType<Module>(out Module))
			{
				var module = Module.Resolve();
				helper.ImportMethod(module, nameof(Module_ctor__string), out Module_ctor__string, definition =>
					definition.IsConstructor
					&& definition.HasParameters
					&& definition.Parameters.Count == 1
					&& definition.Parameters[0].ParameterType.Is<string>());

				helper.ImportMethod(module, nameof(Module_Add__IModuleScoped), out Module_Add__IModuleScoped, definition =>
					definition.Name == nameof(Builder.Module.Add)
					&& definition.HasParameters
					&& definition.Parameters.Count == 1
					&& definition.Parameters[0].ParameterType.Is<IModuleScoped>());
			}

			if (helper.ImportType<ImportVariable>(out ImportVariable))
			{
				var importVariable = ImportVariable.Resolve();
				var importVariableArr = new ArrayType(ImportVariable);

				helper.ImportMethod(importVariable, nameof(ImportVariable_ctor__string_string),
					out ImportVariable_ctor__string_string, definition =>
						definition.IsConstructor
						&& definition.HasParameters
						&& definition.Parameters.Count == 2
						&& definition.Parameters[0].ParameterType.Is<string>()
						&& definition.Parameters[1].ParameterType.Is<string>());

				// import Import
				if (helper.ImportType<Import>(out var importRef))
				{
					var import = importRef.Resolve();
					helper.ImportMethod(import, nameof(Import_ctor__string_ImportVariableArray),
						out Import_ctor__string_ImportVariableArray, definition =>
							definition.IsConstructor
							&& definition.HasParameters
							&& definition.Parameters.Count == 2
							&& definition.Parameters[0].ParameterType.Is<string>()
							&& definition.Parameters[1].ParameterType.Is(importVariableArr));
				}
			}

			if (helper.ImportType<Class>(out Class))
			{
				var @class = Class.Resolve();

				helper.ImportMethod(@class, nameof(Class_ctor__Attributes_string_string_ForeignClass_ClassBody),
					out Class_ctor__Attributes_string_string_ForeignClass_ClassBody, definition =>
						definition.IsConstructor
						&& definition.HasParameters
						&& definition.Parameters.Count == 5
						&& definition.Parameters[0].ParameterType.Is<Attributes>()
						&& definition.Parameters[1].ParameterType.Is<string>()
						&& definition.Parameters[2].ParameterType.Is<string>()
						&& definition.Parameters[3].ParameterType.Is<ForeignClass>()
						&& definition.Parameters[4].ParameterType.Is<ClassBody>());

				helper.ImportMethod(@class, nameof(Class_Add__IClassScoped),
					out Class_Add__IClassScoped, definition =>
						definition.Name == nameof(Builder.Class.Add)
						&& definition.HasParameters
						&& definition.Parameters.Count == 1
						&& definition.Parameters[0].ParameterType.Is<IClassScoped>());
			}

			if (helper.ImportType<Signature>(out var signatureRef))
			{
				var signature = signatureRef.Resolve();

				helper.ImportMethod(signature, nameof(Signature_Create__MethodType_string_int),
					out Signature_Create__MethodType_string_int, definition =>
						definition.Name == nameof(Signature.Create));
			}

			if (helper.ImportType<ForeignAction>(out var foreignActionRef))
			{
				var foreignAction = foreignActionRef.Resolve();

				helper.ImportMethod(foreignAction, nameof(ForeignAction_ctor), out ForeignAction_ctor, definition =>
					definition.IsConstructor);
			}

			if (helper.ImportType<ForeignMethod>(out var foreignMethodRef))
			{
				var foreignMethod = foreignMethodRef.Resolve();

				helper.ImportMethod(foreignMethod, nameof(ForeignMethod_ctor__ForeignAction_String),
					out ForeignMethod_ctor__ForeignAction_String, definition =>
						definition.IsConstructor
						&& definition.HasParameters
						&& definition.Parameters.Count == 2
						&& definition.Parameters[0].ParameterType.Is<ForeignAction>()
						&& definition.Parameters[1].ParameterType.Is<string>());
			}

			if (helper.ImportType<Method>(out var methodRef))
			{
				var method = methodRef.Resolve();

				helper.ImportMethod(method, nameof(Method_ctor__Signature_ForeignMethod),
					out Method_ctor__Signature_ForeignMethod, definition =>
						definition.IsConstructor
						&& definition.HasParameters
						&& definition.Parameters.Count == 2
						&& definition.Parameters[0].ParameterType.Is<Signature>()
						&& definition.Parameters[1].ParameterType.Is<ForeignMethod>());
			}

			return helper.HasIssues == false;
		}
	}
}
