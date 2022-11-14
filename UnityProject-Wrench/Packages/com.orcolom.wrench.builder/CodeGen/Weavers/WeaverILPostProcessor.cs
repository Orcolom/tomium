using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using Wrench.CodeGen;


namespace Wrench.Weaver
{
	public class WeaverILPostProcessor : ILPostProcessor
	{
		public const string RuntimeAssemblyName = "Wrench";

		public override ILPostProcessor GetInstance() => this;

		private static void Log(string msg)
		{
			Console.WriteLine($"[WEAVER] {msg}");
		}

		public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
		{
			var willProcess = WillProcess(compiledAssembly);
			var logText = willProcess ? "Processing" : "Skipping";
			Log($"{logText} {compiledAssembly.Name}");
			if (!willProcess)
				return null;

			var verboseLogging = compiledAssembly.Defines.Contains("WEAVER_DEBUG_LOGS");
			var logger = new WeaverLogger(verboseLogging);

			// ---

			var weaver = new WrenchWeaver(logger);

			// ---

			var assemblyDefinition = weaver.DoWeave(compiledAssembly);

			// write
			var pe = new MemoryStream();
			var pdb = new MemoryStream();

			try
			{
				var writerParameters = new WriterParameters
				{
					SymbolWriterProvider = new PortablePdbWriterProvider(),
					SymbolStream = pdb,
					WriteSymbols = true,
				};

				assemblyDefinition?.Write(pe, writerParameters);
			}
			catch (Exception e)
			{
				logger.Exception(e);
			}

			logger.Close();
			return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), logger.Messages);
		}

		/// <summary>
		/// Process when assembly that references Mirage
		/// </summary>
		/// <param name="compiledAssembly"></param>
		/// <returns></returns>
		public override bool WillProcess(ICompiledAssembly compiledAssembly)
		{
			return compiledAssembly.Name.Contains("Wrench") == false && compiledAssembly.References.Any(filePath =>
				Path.GetFileNameWithoutExtension(filePath) == RuntimeAssemblyName);
		}
	}
}
