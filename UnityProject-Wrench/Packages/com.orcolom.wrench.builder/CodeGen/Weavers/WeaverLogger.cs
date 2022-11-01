using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace Wrench.Weaver
{
	public class WeaverLogger
	{
		public List<DiagnosticMessage> Diagnostics = new List<DiagnosticMessage>();

		public bool EnableTrace { get; }

		public WeaverLogger(bool enableTrace)
		{
			EnableTrace = enableTrace;
		}

		public void Error(string message)
		{
			AddMessage(message, null, DiagnosticType.Error);
		}

		public void Error(string message, MemberReference mr)
		{
			Error($"{message} (at {mr})");
		}

		public void Error(string message, MemberReference mr, SequencePoint sequencePoint)
		{
			AddMessage($"{message} (at {mr})", sequencePoint, DiagnosticType.Error);
		}

		public bool Error(string message, MethodDefinition md)
		{
			Error(message, md, md.DebugInformation.SequencePoints.FirstOrDefault());
			return false;
		}


		public void Warning(string message)
		{
			AddMessage($"{message}", null, DiagnosticType.Warning);
		}

		public void Warning(string message, MemberReference mr)
		{
			Warning($"{message} (at {mr})");
		}

		public void Warning(string message, MemberReference mr, SequencePoint sequencePoint)
		{
			AddMessage($"[WEAVER] {message} (at {mr})", sequencePoint, DiagnosticType.Warning);
		}

		public void Warning(string message, MethodDefinition md)
		{
			Warning(message, md, md.DebugInformation.SequencePoints.FirstOrDefault());
		}


		private void AddMessage(string message, SequencePoint sequencePoint, DiagnosticType diagnosticType)
		{
			Diagnostics.Add(new DiagnosticMessage
			{
				DiagnosticType = diagnosticType,
				File = sequencePoint?.Document.Url.Replace($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}", ""),
				Line = sequencePoint?.StartLine ?? 0,
				Column = sequencePoint?.StartColumn ?? 0,
				MessageData = message
			});
		}

		public void Log(string str)
		{
			// weaver.Logger.Log(DebugPrinter.Encoded(sb => DebugPrinter.Print(sb, 0, method)));

			Console.WriteLine($"[WEAVER] {str}");
			Warning(str);
		}
	}
}
