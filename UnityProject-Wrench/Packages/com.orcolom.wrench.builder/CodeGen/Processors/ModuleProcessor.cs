using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
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

			// implements attribute and has valid arguments
			if (input.HasAttribute<WrenchModuleAttribute>(out var attribute) == false) return false;
			if (attribute.HasConstructorArguments == false
				|| attribute.ConstructorArguments[0].Value is not string str
				|| string.IsNullOrEmpty(str))
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchModuleAttribute)}` but doesnt have a valid `{nameof(WrenchModuleAttribute.Path)}`",
					input);
				return false;
			}

			data.Path = str;

			// derives from module class
			if (input.IsDerivedFrom<Module>() == false)
			{
				weaver.Logger.Error(
					$"`{input.FullName}` wants to be weaved with `{nameof(WrenchModuleAttribute)}` but doesn't derive from type `{nameof(Builder.Module)}`",
					input);
				return false;
			}


			// implements attribute and has valid arguments
			if (input.HasAttributes<WrenchImport>(out var attributes))
			{
				for (int i = 0; i < attributes.Count; i++)
				{
					var import = attributes[i];

					if (import.HasConstructorArguments == false
						|| import.ConstructorArguments[0].Value is not TypeDefinition importType)
					{
						weaver.Logger.Error(
							$"`{nameof(WrenchImport)}` on {input} is invalid",
							input);
						return false;
					}

					bool isClass = importType.IsDerivedFrom<Class>();
					bool isModule = importType.IsDerivedFrom<Module>();
					if (isClass == false && isModule == false)
					{
						weaver.Logger.Error(
							$"`{nameof(WrenchImport)}` on {input} type is not module or class",
							input);
						return false;
					}

					data.Imports.Add(importType);
				}
			}

			// create initializer method
			var md = new MethodDefinition(InitializerMethodName,
				MethodAttributes.HideBySig | MethodAttributes.Private,
				weaver.Imports.Void);
			input.Methods.Add(md);
			data.InitializerBody = md.Body.GetILProcessor();
			data.InitializerBody.Emit(OpCodes.Nop);
			data.InitializerBody.Emit(OpCodes.Ret);

			// weave the initializer in the constructors
			for (int i = 0; i < input.Methods.Count; i++)
			{
				var method = input.Methods[i];

				if (method.IsConstructor == false || method.IsStatic) continue;

				// allow an default constructor
				if (method.HasParameters == false)
				{
					WeaveInitMethodOnConstructor(weaver, method, data);
					continue;
				}

				weaver.Logger.Error(
					$"`{input.FullName}` has constructors. this is not supported at the moment",
					method);
				return false;
			}

			Modules.Add(data);
			return true;
		}

		private static void WeaveInitMethodOnConstructor(WrenchWeaver weaver, MethodDefinition constructor,
			WrenchModuleDefinition module)
		{
			// store original il code and clear it
			bool foundBaseCall = false;
			List<Instruction> instructions = new List<Instruction>(constructor.Body.Instructions.Count);
			for (int i = 0; i < constructor.Body.Instructions.Count; i++)
			{
				var instruction = constructor.Body.Instructions[i];
				if (foundBaseCall == false)
				{
					if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference method
						&& method.FullName.Contains("::.ctor"))
					{
						foundBaseCall = true;
					}

					continue;
				}

				instructions.Add(constructor.Body.Instructions[i]);
			}

			constructor.Body.Instructions.Clear();

			VariableDefinition[] variables = new VariableDefinition[constructor.Body.Variables.Count];
			for (int i = 0; i < constructor.Body.Variables.Count; i++)
			{
				variables[i] = constructor.Body.Variables[i];
			}

			constructor.Body.Variables.Clear();

			var il = constructor.Body.GetILProcessor();

			// base..ctor({Path});
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldstr, module.Path);
			il.Emit(OpCodes.Call, weaver.Imports.Module_ctor__string);
			il.Emit(OpCodes.Nop);

			// this.{Init}();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, module.InitializerBody.Body.Method);
			il.Emit(OpCodes.Nop);

			// // add back original il code
			for (int i = 0; i < variables.Length; i++)
			{
				constructor.Body.Variables.Add(variables[i]);
			}

			if (instructions.Count != 0)
			{
				for (int i = 0; i < instructions.Count; i++)
				{
					constructor.Body.Instructions.Add(instructions[i]);
				}
			}
			else il.Emit(OpCodes.Ret);
		}

		public void Process(WrenchWeaver weaver, ClassProcessor classProcessor)
		{
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
					var classData = classProcessor.Classes.Find(definition => definition.ClassType.Is(importType));
					if (classData == null)
					{
						weaver.Logger.Error($"coul dnot find `{importType.FullName}`");
						return;
					}

					if (imports.TryGetValue(classData.ModuleType.FullName, out var list) == false)
					{
						list = new List<WrenchClassDefinition>();
						var importModuleData = Modules.Find(definition => definition.ModuleType.Is(classData.ModuleType));
						if (importModuleData == null)
						{
							weaver.Logger.Error($"could not find `{importType.FullName}`");
							return;
						}

						imports.Add(importModuleData.Path, list);
					}

					if (list.Contains(classData))
					{
						weaver.Logger.Error($"trying to add multiple imports of the same type");
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
