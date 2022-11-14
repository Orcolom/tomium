using System;
using System.IO;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Wrench.Weaver
{
	/// <summary>
	/// Weaves an Assembly
	/// <para>
	/// Debug Defines:<br />
	/// - <c>WEAVER_DEBUG_LOGS</c><br />
	/// - <c>WEAVER_DEBUG_TIMER</c><br />
	/// </para>
	/// </summary>
	public abstract class AWeaver<TImports>
		where TImports : WeaverImporter, new()
	{
		protected internal TImports Imports { get; private set; }
		protected internal WeaverLogger Logger { get; }

		protected internal AssemblyDefinition CurrentAssembly { get; private set; }
		protected internal ModuleDefinition MainModule { get; private set; }

		protected AWeaver(WeaverLogger logger)
		{
			Logger = logger;
		}

		protected abstract void Weave();

		public AssemblyDefinition DoWeave(ICompiledAssembly compiledAssembly)
		{
			Logger.Start(GetType().Name, compiledAssembly.Name);

			try
			{
				using (Logger.Sample("AssemblyDefinitionFor"))
				{
					CurrentAssembly = AssemblyDefinitionFor(compiledAssembly);
				}

				MainModule = CurrentAssembly.MainModule;
				Imports = new TImports();

				bool hasImports;
				using (Logger.Sample("Predefined Imports"))
				{
					hasImports = Imports.Populate(Logger, MainModule);
				}

				if (hasImports)
				{
					using (Logger.Sample("Weave"))
					{
						Weave();
					}
				}

				return CurrentAssembly;
			}
			catch (Exception e)
			{
				Logger.Exception(e);
				return null;
			}
			finally
			{
				// end in finally incase it return early
				Logger.End();
			}
		}

		private static AssemblyDefinition AssemblyDefinitionFor(ICompiledAssembly compiledAssembly)
		{
			var assemblyResolver = new PostProcessorAssemblyResolver(compiledAssembly);
			var readerParameters = new ReaderParameters
			{
				SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData),
				SymbolReaderProvider = new PortablePdbReaderProvider(),
				AssemblyResolver = assemblyResolver,
				ReflectionImporterProvider = new PostProcessorReflectionImporterProvider(),
				ReadingMode = ReadingMode.Immediate
			};

			var assemblyDefinition =
				AssemblyDefinition.ReadAssembly(new MemoryStream(compiledAssembly.InMemoryAssembly.PeData), readerParameters);

			//apparently, it will happen that when we ask to resolve a type that lives inside MLAPI.Runtime, and we
			//are also postprocessing MLAPI.Runtime, type resolving will fail, because we do not actually try to resolve
			//inside the assembly we are processing. Let's make sure we do that, so that we can use postprocessor features inside
			//MLAPI.Runtime itself as well.
			assemblyResolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition);

			return assemblyDefinition;
		}
	}


	public class WeaverImporter
	{
		public TypeReference Void;
		public TypeReference String;
		public TypeReference Bool;

		public virtual bool Populate(WeaverLogger logger, ModuleDefinition moduleDefinition)
		{
			Void = moduleDefinition.ImportReference(typeof(void));
			String = moduleDefinition.ImportReference(typeof(string));
			Bool = moduleDefinition.ImportReference(typeof(bool));
			return true;
		}
	}

	public struct ImportHelper : IDisposable
	{
		private WeaverLogger.GroupScope _log;
		private readonly ModuleDefinition _module;

		public ImportHelper(WeaverLogger logger, ModuleDefinition module, string errorMessage)
		{
			_log = logger.ErrorGroup(errorMessage);
			_module = module;
		}

		public bool HasIssues => _log.HasIssues;

		public void Dispose()
		{
			_log.Dispose();
		}

		public bool ImportType<TType>(out TypeReference reference) => ImportType(typeof(TType), out reference);

		public bool ImportType(Type t, out TypeReference reference)
		{
			reference = default;
			using var _ = _log.Logger.Sample($"import type {t}");

			try
			{
				reference = _module.ImportReference(t);
				if (reference == null) _log.Error($"{t} not found");
				return reference != null;
			}
			catch (Exception e)
			{
				_log.Error($"{t} has exception {e}");
				return false;
			}
		}

		public void ImportField(TypeDefinition definition, string name, out FieldReference field,
			Func<FieldDefinition, bool> action)
		{
			field = default;

			using var _ = _log.Logger.Sample($"import field `{definition.Name}.{name}`");

			try
			{
				bool found = false;
				for (int i = 0; i < definition.Fields.Count; i++)
				{
					var search = definition.Fields[i];
					if (action.Invoke(search) == false) continue;
					if (found)
					{
						_log.Error($"`found multiple fields that fit in the constrains for `{definition.Name}.{name}`");
						return;
					}

					found = true;
					field = _module.ImportReference(search);
				}

				if (found) return;
				_log.Error($"`{definition.Name}.{name}` not found");
			}
			catch (Exception e)
			{
				_log.Error($"`{definition.Name}.{name}` has exception {e}");
			}
		}

		public void ImportMethod(TypeDefinition definition, string name, out MethodReference method,
			Func<MethodDefinition, bool> action)
		{
			method = default;

			using var _ = _log.Logger.Sample($"import method `{definition.Name}.{name}`");

			try
			{
				bool found = false;
				for (int i = 0; i < definition.Methods.Count; i++)
				{
					var searchMethod = definition.Methods[i];
					if (action.Invoke(searchMethod) == false) continue;
					if (found)
					{
						_log.Error($"found multiple methods that fit in the constrains of `{definition.Name}.{name}`");
						return;
					}

					found = true;
					method = _module.ImportReference(searchMethod);
				}

				if (found) return;
				_log.Error($"`{definition.Name}.{name}` not found");
			}
			catch (Exception e)
			{
				_log.Error($"`{definition.Name}.{name}` has exception {e}");
			}
		}
	}
}
