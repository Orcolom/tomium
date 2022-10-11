using System.Collections.Generic;
using Mono.Cecil;
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

			for (int i = 0; i < module.Types.Count; i++)
			{
				var input = module.Types[i];
				
				Logger.Log(input.ToString());

				if (moduleProcessor.TryExtract(this, input, out var moduleDefinition)) continue;
				if (classProcessor.TryExtract(this, input, out var classDefinition))
				{
					for (int j = 0; j < input.Methods.Count; j++)
					{
						methodProcessor.TryExtract(this, input, input.Methods[j], out var methodDefinition);
					}
				}

				// if (type.IsDerivedFrom<Class>() == false) continue;

				// var methods = type.Methods;
				// var methodCount = type.Methods.Count;
				// for (int j = 0; j < methodCount; j++)
				// {
				// var method = methods[j];

				// var parameters = method.Parameters;
				// var parametersCount = method.Parameters.Count;

				// if (parametersCount == 0) continue;
				// if (parameters[0].ParameterType.Is<Vm>() == false) continue;

				// Logger.Log(method.FullName);
				// }
			}
		}
	}

	public class WrenchImports : Imports
	{
		public TypeReference Vm;

		public TypeReference Module;
		public MethodReference Module_ctor__string;
		
		
		public TypeReference Class;
		public MethodReference Class_ctor__Attributes_string_string_ForeignClass_ClassBody;

		public TypeReference ForeignClass;

		public override bool Populate(WeaverLogger logger, ModuleDefinition moduleDefinition)
		{
			if (base.Populate(logger, moduleDefinition) == false) return false;

			ForeignClass = moduleDefinition.ImportReference(typeof(ForeignClass));
			Vm = moduleDefinition.ImportReference(typeof(Vm));
			
			// import Module
			Module = moduleDefinition.ImportReference(typeof(Module));
			var module = Module.Resolve();
			
			for (int i = 0; i < module.Methods.Count; i++)
			{
				var method = module.Methods[i];
				
				if (method.IsConstructor == false) continue;
				if (method.Parameters.Count != 1) continue;
				if (method.Parameters[0].ParameterType.Is(StringRef) == false) continue;

				Module_ctor__string = moduleDefinition.ImportReference(method);
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
				
				if (method.IsConstructor == false) continue;
				if (method.HasParameters == false) continue;
				if (method.Parameters[0].ParameterType.Is<Attributes>() == false) continue;
				if (method.Parameters[1].ParameterType.Is<string>() == false) continue;
				if (method.Parameters[2].ParameterType.Is<string>() == false) continue;
				if (method.Parameters[3].ParameterType.Is<ForeignClass>() == false) continue;
				if (method.Parameters[4].ParameterType.Is<ClassBody>() == false) continue;

				Class_ctor__Attributes_string_string_ForeignClass_ClassBody = moduleDefinition.ImportReference(method);
			}

			if (Class_ctor__Attributes_string_string_ForeignClass_ClassBody == null)
			{
				logger.Error($"could not find {nameof(Class_ctor__Attributes_string_string_ForeignClass_ClassBody)}");
				return false;
			}

			return true;
		}
	}
}
