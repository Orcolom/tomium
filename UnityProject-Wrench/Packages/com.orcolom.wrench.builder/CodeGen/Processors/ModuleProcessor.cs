using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Wrench.Builder;
using Wrench.Weaver;

namespace Wrench.CodeGen.Processors
{
	public class WrenchModuleDefinition
	{
		public string Path;
		public TypeDefinition ModuleType;
		public ILProcessor InitializerBody;
		public List<TypeDefinition> Imports = new List<TypeDefinition>();
		public MethodDefinition CtorMethod;
	}

	public class ModuleProcessor
	{
		private const string InitializerMethodName = WrenchWeaver.Prefix + "init";

		public List<WrenchModuleDefinition> Modules = new List<WrenchModuleDefinition>();

		public bool TryExtract(WrenchWeaver weaver, TypeDefinition input)
		{
			var data = new WrenchModuleDefinition
			{
				ModuleType = input,
			};

			if (CanValidateAttribute(weaver, input, data) == false) return false;
			if (CanValidateImports(weaver, input, data) == false) return false;
			if (CanValidateMethod(weaver, input, data) == false) return false;

			// create initializer method
			var md = new MethodDefinition(InitializerMethodName,
				MethodAttributes.HideBySig | MethodAttributes.Private,
				weaver.Imports.Void);
			input.Methods.Add(md);
			data.InitializerBody = md.Body.GetILProcessor();
			data.InitializerBody.Emit(OpCodes.Nop);
			data.InitializerBody.Emit(OpCodes.Ret);

			WeaveInitMethodOnConstructor(weaver, data.CtorMethod, data);

			Modules.Add(data);
			weaver.Logger.Log($"extracted module: {input}");
			return true;
		}

		private bool CanValidateAttribute(WrenchWeaver weaver, TypeDefinition input, WrenchModuleDefinition data)
		{
			using var log = weaver.Logger.ErrorGroup($"extract {input} CanValidateAttribute");

			// implements attribute and has valid arguments
			if (input.HasAttribute<WrenchModuleAttribute>(out var attribute) == false) return false;
			if (attribute.HasConstructorArguments == false) log.Error("attribute has no constructor arguments");
			else if (attribute.ConstructorArguments[0].Value is not string str) log.Error("arg0 should be string");
			else if (string.IsNullOrEmpty(str)) log.Error("arg0 can not be null or empty");
			else data.Path = str;

			return log.HasIssues == false;
		}

		private bool CanValidateImports(WrenchWeaver weaver, TypeDefinition input, WrenchModuleDefinition data)
		{
			// implements attribute and has valid arguments
			if (input.HasAttributes<WrenchImport>(out var attributes) == false) return true;

			bool hasIssues = false;
			for (int i = 0; i < attributes.Count; i++)
			{
				using var log = weaver.Logger.ErrorGroup($"extract {input} CanValidateImport");

				var import = attributes[i];

				if (import.HasConstructorArguments == false) log.Error("attribute has no constructor arguments");
				if (import.ConstructorArguments[0].Value is not TypeDefinition importType) log.Error("arg0 should be Type");
				else
				{
					var importTypeRef = weaver.MainModule.ImportReference(importType);
					bool isClass = importTypeRef.IsDerivedFrom(weaver.Imports.Class);
					bool isModule = importTypeRef.IsDerivedFrom(weaver.Imports.Module);
					if (isClass == false && isModule == false) log.Error("type is not module or class");
					data.Imports.Add(importType);
				}

				hasIssues |= log.HasIssues;
			}

			return hasIssues == false;
		}

		private bool CanValidateMethod(WrenchWeaver weaver, TypeDefinition input, WrenchModuleDefinition data)
		{
			using var log = weaver.Logger.ErrorGroup($"extract {input} CanValidateMethod");

			// derives from module class
			if (input.IsDerivedFrom<Module>() == false) log.Error("type does not derive from Module");

			// weave the initializer in the constructors
			for (int i = 0; i < input.Methods.Count; i++)
			{
				var method = input.Methods[i];

				if (method.IsConstructor == false || method.IsStatic) continue;

				// allow an default constructor
				if (method.HasParameters == false)
				{
					data.CtorMethod = method;
					continue;
				}

				log.Error("has non-default constructors. this is not supported at the moment");
				return false;
			}

			return log.HasIssues == false;
		}


		private static void WeaveInitMethodOnConstructor(WrenchWeaver weaver, MethodDefinition constructor,
			WrenchModuleDefinition module)
		{
			using var _ = new ConstructorInjector(constructor, out var il);

			// base..ctor({Path});
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldstr, module.Path);
			il.Emit(OpCodes.Call, weaver.Imports.Module_ctor__string);
			il.Emit(OpCodes.Nop);

			// this.{Init}();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, module.InitializerBody.Body.Method);
			il.Emit(OpCodes.Nop);
		}

		public void Process(WrenchWeaver weaver, ClassProcessor classProcessor)
		{
			using var _ = weaver.Logger.Sample("Weave.ModuleProcessor.Process");

			for (int i = 0; i < Modules.Count; i++)
			{
				var moduleData = Modules[i];

				moduleData.InitializerBody.Body.Instructions.Clear();
				moduleData.InitializerBody.Body.Variables.Clear();

				var il = moduleData.InitializerBody.Body.GetILProcessor();

				Dictionary<string, List<WrenchClassDefinition>> imports = new Dictionary<string, List<WrenchClassDefinition>>();
				for (int j = 0; j < moduleData.Imports.Count; j++)
				{
					var importType = moduleData.Imports[j];
					using var log = weaver.Logger.ErrorGroup($"Process import {importType} on {moduleData.ModuleType}");

					var classData = classProcessor.Classes.Find(definition => definition.ClassType.Is(importType));
					if (classData == null) 
					{
						log.Error("could not find class data");
						return;
					}

					if (imports.TryGetValue(classData.ModuleType.FullName, out var list) == false)
					{
						list = new List<WrenchClassDefinition>();
						var importModuleData = Modules.Find(definition => definition.ModuleType.Is(classData.ModuleType));
						if (importModuleData == null)
						{
							log.Error($"could not find `{importType.FullName}`");
							return;
						}

						imports.Add(importModuleData.Path, list);
					}

					if (list.Contains(classData))
					{
						log.Error($"trying to add multiple imports of the same type");
						return;
					}

					list.Add(classData);
				}

				foreach (var pair in imports)
				{
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldstr, pair.Key);

					if (pair.Value.Count == 0)
					{
						il.Emit(OpCodes.Ldnull);
					}
					else
					{
						il.Emit(OpCodes.Ldc_I4, pair.Value.Count);
						il.Emit(OpCodes.Newarr, weaver.Imports.ImportVariable);

						for (int j = 0; j < pair.Value.Count; j++)
						{
							var import = pair.Value[j];
							il.Emit(OpCodes.Dup);
							il.Emit(OpCodes.Ldc_I4, j);
							il.Emit(OpCodes.Ldstr, import.Name);
							il.Emit(OpCodes.Ldnull);
							il.Emit(OpCodes.Newobj, weaver.Imports.ImportVariable_ctor__string_string);
							il.Emit(OpCodes.Stelem_Ref);
						}
					}

					il.Emit(OpCodes.Newobj, weaver.Imports.Import_ctor__string_ImportVariableArray);
					il.Emit(OpCodes.Call, weaver.Imports.Module_Add__IModuleScoped);
					il.DEBUG_EmitNop();
				}

				for (int j = 0; j < classProcessor.Classes.Count; j++)
				{
					var classData = classProcessor.Classes[j];

					if (classData.ModuleType != moduleData.ModuleType) continue;

					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Newobj, classData.CtorMethod);
					il.Emit(OpCodes.Call, weaver.Imports.Module_Add__IModuleScoped);
					il.DEBUG_EmitNop();
				}

				il.Emit(OpCodes.Ret);
			}
		}
	}
}
