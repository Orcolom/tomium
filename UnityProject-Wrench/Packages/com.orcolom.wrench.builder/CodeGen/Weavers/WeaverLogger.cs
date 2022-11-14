using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace Wrench.Weaver
{
	public enum LogType
	{
		Log,
		Diagnostics,
		Warning,
		Error,
		Exception,
	}

	public class WeaverLogger
	{
		#region Types

		public struct GroupScope : IDisposable
		{
			private readonly string _label;
			private readonly List<string> _errors;

			public bool HasIssues { get; private set; }
			public WeaverLogger Logger { get; }

			public GroupScope(WeaverLogger logger, string errorMessage)
			{
				this = default;

				_label = errorMessage;
				Logger = logger;
				_errors = new List<string>();
			}

			public void Dispose()
			{
				Logger.LogGroup(_label, _errors);
			}

			public void Error(string s)
			{
				HasIssues = true;
				_errors.Add(s);
			}
		}

		public struct SampleScope : IDisposable
		{
			private readonly WeaverLogger _timer;
			private readonly long _start;
			private readonly string _label;

			public SampleScope(WeaverLogger logger, string label)
			{
				_timer = logger;
				_start = logger.ElapsedMilliseconds;
				_label = label;
			}

			public void Dispose()
			{
				_timer.AddMessage($"{_label}: {_timer.ElapsedMilliseconds - _start}ms", null, LogType.Diagnostics);
			}
		}

		#endregion

		private string _weaver;
		private string _module;
		private StreamWriter _writer;
		private Stopwatch _stopwatch;
		private StringBuilder _sb = new StringBuilder();

		public bool WriteToFile = true;

		public readonly List<DiagnosticMessage> Messages = new List<DiagnosticMessage>();

		// public readonly WeaverDiagnosticsTimer Timer;
		public long ElapsedMilliseconds => _stopwatch?.ElapsedMilliseconds ?? 0;

		static bool _checkDirectory = false;

		public WeaverLogger(bool verboseLogging) { }

		~WeaverLogger() { }

		public void Start(string weaver, string module)
		{
			_weaver = weaver;
			_module = module;

			if (WriteToFile == false) return;
			var path = $"./Logs/WeaverLogs/{weaver}_{_module}.log";
			try
			{
				if (_checkDirectory == false)
				{
					_checkDirectory = true;
					Directory.CreateDirectory("./Logs/WeaverLogs");
				}

				_writer = new StreamWriter(path)
				{
					AutoFlush = true,
				};

				StartTimer();
			}
			catch (Exception e)
			{
				_writer?.Dispose();
				WriteToFile = false;
				Log($"Failed to open {path}: {e}");
			}
		}

		public long End()
		{
			Log($"Weave Finished: {ElapsedMilliseconds}ms ---");
			_stopwatch?.Stop();

			return ElapsedMilliseconds;
		}

		public void Close()
		{
			var errorCount = Messages.Count(x => x.DiagnosticType == DiagnosticType.Error);

			if (errorCount > 0)
			{
				Messages.Insert(0, new DiagnosticMessage()
				{
					DiagnosticType = DiagnosticType.Error,
					MessageData =
						$"Failed weaving {_module} with {errorCount} errors. Unity doesnt allow new lines in compile errors so for more in-depth or better formatted errors go to: `./Logs/WeaverLogs/{_weaver}_{_module}.log`",
				});
			}

			_writer?.Close();
		}

		[Conditional("WEAVER_DEBUG_TIMER")]
		public void StartTimer()
		{
			_stopwatch = Stopwatch.StartNew();
			Log($"Weave Started ---");
		}


		public SampleScope Sample(string label)
		{
			return new SampleScope(this, label);
		}

		public GroupScope ErrorGroup(string label)
		{
			return new GroupScope(this, label);
		}


		public void Exception(Exception e)
		{
			_sb.Clear();
			_sb.AppendLine(e.Message);
			_sb.AppendLine(e.StackTrace);
			AddMessage(_sb.ToString(), null, LogType.Exception);
		}

		public void LogGroup(string label, List<string> errors)
		{
			if (errors == null || errors.Count == 0) return;

			_sb.Clear();
			_sb.AppendLine($"{label} (errors: {errors.Count})");
			for (int i = 0; i < errors.Count; i++)
			{
				_sb.AppendLine(errors[i]);
			}

			AddMessage(_sb.ToString(), null, LogType.Exception);
		}

		public void Error(string message)
		{
			AddMessage(message, null, LogType.Error);
		}

		public void Error(string message, MemberReference mr)
		{
			Error($"{message} (at {mr})");
		}

		public void Error(string message, MemberReference mr, SequencePoint sequencePoint)
		{
			AddMessage($"{message} (at {mr})", sequencePoint, LogType.Warning);
		}

		public bool Error(string message, MethodDefinition md)
		{
			Error(message, md, md.DebugInformation.SequencePoints.FirstOrDefault());
			return false;
		}


		public void Warning(string message)
		{
			AddMessage($"{message}", null, LogType.Warning);
		}

		public void Warning(string message, MemberReference mr)
		{
			Warning($"{message} (at {mr})");
		}

		public void Warning(string message, MemberReference mr, SequencePoint sequencePoint)
		{
			AddMessage($"[WEAVER] {message} (at {mr})", sequencePoint, LogType.Warning);
		}

		public void Warning(string message, MethodDefinition md)
		{
			Warning(message, md, md.DebugInformation.SequencePoints.FirstOrDefault());
		}

		private void AddMessage(string message, SequencePoint sequencePoint, LogType type)
		{
			var fullMessage = $"[{_weaver}] [{_module}] [{type}] {message}";

			if (WriteToFile) _writer.WriteLine(fullMessage);

			if (type == LogType.Log || type == LogType.Diagnostics) return;


			fullMessage = fullMessage.Replace(Environment.NewLine, "  \\n  ");

			Messages.Add(new DiagnosticMessage
			{
				DiagnosticType = type switch
				{
					LogType.Error => DiagnosticType.Error,
					LogType.Exception => DiagnosticType.Error,
					LogType.Warning => DiagnosticType.Warning,
					_ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
				},
				File = sequencePoint?.Document.Url.Replace($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}", ""),
				Line = sequencePoint?.StartLine ?? 0,
				Column = sequencePoint?.StartColumn ?? 0,
				MessageData = fullMessage,
			});
		}

		public void Log(string str)
		{
			var fullMessage = $"[{_weaver}] [{_module}] {str}";

			if (WriteToFile) _writer.WriteLine(fullMessage);
		}
	}
}
