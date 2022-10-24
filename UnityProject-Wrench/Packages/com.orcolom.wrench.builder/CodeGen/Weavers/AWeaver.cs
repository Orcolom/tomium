using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

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
		where TImports : Imports, new()
	{
		protected internal TImports Imports { get; private set; }
		protected internal WeaverLogger Logger { get; }
		protected WeaverDiagnosticsTimer Timer { get; }

		protected internal AssemblyDefinition CurrentAssembly { get; private set; }
		protected internal ModuleDefinition MainModule { get; private set; }

		[Conditional("WEAVER_DEBUG_LOGS")]
		public static void DebugLog(TypeDefinition td, string message)
		{
			Console.WriteLine($"Weaver[{td.Name}]{message}");
		}

		static void Log(string msg)
		{
			Console.WriteLine($"[Weaver] {msg}");
		}

		public AWeaver(WeaverLogger logger)
		{
			Logger = logger;
			Timer = new WeaverDiagnosticsTimer() {writeToFile = true};
		}

		protected abstract void Weave();
		public AssemblyDefinition DoWeave(ICompiledAssembly compiledAssembly)
		{
			Log($"Starting weaver on {compiledAssembly.Name}");
			try
			{
				Timer.Start(compiledAssembly.Name);

				using (Timer.Sample("AssemblyDefinitionFor"))
				{
					CurrentAssembly = AssemblyDefinitionFor(compiledAssembly);
					
					// foreach (string reference in compiledAssembly.References)
						// Logger.Log($"{compiledAssembly.Name} references {reference}");
				}

				MainModule = CurrentAssembly.MainModule;
				Imports = new TImports();
				
				if (Imports.Populate(Logger, MainModule))
				{
					Weave();
				}
				
				// readers = new Readers(module, logger);
				// writers = new Writers(module, logger);
				// propertySiteProcessor = new PropertySiteProcessor();
				// var rwProcessor = new ReaderWriterProcessor(module, readers, writers);
				//
				// var modified = false;
				// using (timer.Sample("ReaderWriterProcessor"))
				// {
				// 	modified = rwProcessor.Process();
				// }
				//
				// var foundTypes = FindAllClasses(module);
				//
				// using (timer.Sample("AttributeProcessor"))
				// {
				// 	var attributeProcessor = new AttributeProcessor(module, logger);
				// 	modified |= attributeProcessor.ProcessTypes(foundTypes);
				// }
				//
				// using (timer.Sample("WeaveNetworkBehavior"))
				// {
				// 	foreach (var foundType in foundTypes)
				// 	{
				// 		if (foundType.IsNetworkBehaviour)
				// 			modified |= WeaveNetworkBehavior(foundType);
				// 	}
				// }
				//
				//
				// if (modified)
				// {
				// 	using (timer.Sample("propertySiteProcessor"))
				// 	{
				// 		propertySiteProcessor.Process(module);
				// 	}
				//
				// 	using (timer.Sample("InitializeReaderAndWriters"))
				// 	{
				// 		rwProcessor.InitializeReaderAndWriters();
				// 	}
				// }

				return CurrentAssembly;
			}
			catch (Exception e)
			{
				StringBuilder stringBuilder = new StringBuilder();

				Logger.Error("Exception :" + e +  e.StackTrace);
				// write line too because the error about doesn't show stacktrace
				Console.WriteLine("[WeaverException] :" + e);
				return null;
			}
			finally
			{
				Log($"Finished weaver on {compiledAssembly.Name}");
				// end in finally incase it return early
				Timer?.End();
			}
		}

		public static AssemblyDefinition AssemblyDefinitionFor(ICompiledAssembly compiledAssembly)
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

		private IReadOnlyList<FoundType> FindAllClasses(ModuleDefinition module)
		{
			using (Timer.Sample("FindAllClasses"))
			{
				var foundTypes = new List<FoundType>();
				foreach (var type in module.Types)
				{
					ProcessType(type, foundTypes);

					foreach (var nested in type.NestedTypes)
					{
						ProcessType(nested, foundTypes);
					}
				}

				return foundTypes;
			}
		}

		private void ProcessType(TypeDefinition type, List<FoundType> foundTypes)
		{
			if (!type.IsClass) return;

			var parent = type.BaseType;
			var isNetworkBehaviour = false;
			var isMonoBehaviour = false;
			while (parent != null)
			{
				// if (parent.Is<NetworkBehaviour>())
				// {
				// 	isNetworkBehaviour = true;
				// 	isMonoBehaviour = true;
				// 	break;
				// }
				//
				// if (parent.Is<MonoBehaviour>())
				// {
				// 	isMonoBehaviour = true;
				// 	break;
				// }
				//
				// parent = parent.TryResolveParent();
			}

			foundTypes.Add(new FoundType(type, isNetworkBehaviour, isMonoBehaviour));
		}

		private bool WeaveNetworkBehavior(FoundType foundType)
		{
			var behaviourClasses = FindAllBaseTypes(foundType);

			var modified = false;
			// process this and base classes from parent to child order
			for (var i = behaviourClasses.Count - 1; i >= 0; i--)
			{
				// var behaviour = behaviourClasses[i];
				// if (NetworkBehaviourProcessor.WasProcessed(behaviour))
				// {
				// 	continue;
				// }
				//
				// modified |= new NetworkBehaviourProcessor(behaviour, readers, writers, propertySiteProcessor, logger).Process();
			}

			return modified;
		}

		/// <summary>
		/// Returns all base types that are between the type and NetworkBehaviour
		/// </summary>
		/// <param name="foundType"></param>
		/// <returns></returns>
		private static List<TypeDefinition> FindAllBaseTypes(FoundType foundType)
		{
			var behaviourClasses = new List<TypeDefinition>();

			var type = foundType.TypeDefinition;
			while (type != null)
			{
				// if (type.Is<NetworkBehaviour>())
				// {
				// 	break;
				// }
				//
				// behaviourClasses.Add(type);
				// type = type.BaseType.TryResolve();
			}

			return behaviourClasses;
		}
	}


	public class Imports
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
	
	public class FoundType
	{
		public readonly TypeDefinition TypeDefinition;

		/// <summary>
		/// Is Derived From NetworkBehaviour
		/// </summary>
		public readonly bool IsNetworkBehaviour;

		public readonly bool IsMonoBehaviour;

		public FoundType(TypeDefinition typeDefinition, bool isNetworkBehaviour, bool isMonoBehaviour)
		{
			TypeDefinition = typeDefinition;
			IsNetworkBehaviour = isNetworkBehaviour;
			IsMonoBehaviour = isMonoBehaviour;
		}

		public override string ToString()
		{
			return TypeDefinition.ToString();
		}
	}
}
