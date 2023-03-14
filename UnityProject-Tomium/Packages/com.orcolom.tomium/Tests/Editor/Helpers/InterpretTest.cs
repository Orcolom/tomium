using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Tomium;

namespace Tomium.Tests.Helpers
{
	public class InterpretTest : IDisposable
	{
		private InterpretResult? _expectResult;
		private bool _hasFail;
		private List<string> _expectedMessages;
		private StringBuilder _wrenErrors;
		private List<Marker> _markers;
		private Type _exceptionType;

		private List<string> _messages;
		private InterpretResult _result;

		private StringBuilder _errors;

		public void Dispose()
		{
			CheckFails();

			_markers = null;
			_messages = null;
			_expectedMessages = null;
			_expectResult = null;
			_wrenErrors?.Clear();
		}

		private void CheckFails()
		{
			if (_hasFail || _result == InterpretResult.CompileError) return;

			if (_result != InterpretResult.CompileError && _expectedMessages != null)
			{
				if (_expectedMessages.Count != _messages.Count)
				{
					Assert.Fail(
						$"Expected Messages didn't line up. expected {_expectedMessages.Count} but got {_messages.Count}");
					return;
				}
			}

			if (_markers != null)
			{
				for (int i = 0; i < _markers.Count; i++)
				{
					var marker = _markers[i];
					if (marker.IsReached == false)
					{
						marker.Verify();
						return;
					}
				}
			}
		}

		private void Setup(in Vm vm)
		{
			_wrenErrors = new StringBuilder();
			_messages = new List<string>();

			vm.SetErrorListener(HandleErrors);
			if (_expectedMessages != null) vm.SetWriteListener(HandleWrites);
			if (_markers != null)
			{
				for (int i = 0; i < _markers.Count; i++) _markers[i].Reset();
			}
		}
		
		private void Teardown(in Vm vm, Exception e)
		{
			if (e != null)
			{
				if (e.GetType() == _exceptionType) return;
				_hasFail = true;
				Assert.Fail($"Expected exception of type {_exceptionType} but got {e} with message {e.Message}");
				return;
			}
			
			if (_expectResult.HasValue && _expectResult.Value != _result)
			{
				_hasFail = true;
				Assert.AreEqual(_expectResult.Value, _result,
					$"expected {_expectResult.Value} but got {_result}\n{_wrenErrors}");
				return;
			}
		}
		
		public void Interpret(in Vm vm, string name, string source)
		{
			Setup(vm);
			
			Exception exception = null;
			try
			{
				_result = vm.Interpret(name, source);
			}
			catch (AssertionException)
			{
				_hasFail = true;
				throw;
			}
			catch (Exception e)
			{
				exception = e;
			}

			Teardown(vm, exception);
		}

		public void Call(in Vm vm, in Handle handle)
		{
			Setup(vm);
			
			Exception exception = null;
			try
			{
				_result = vm.Call(handle);
			}
			catch (AssertionException)
			{
				_hasFail = true;
				throw;
			}
			catch (Exception e)
			{
				exception = e;
			}

			Teardown(vm, exception);
		}
		
		private void HandleErrors(Vm vm, ErrorType type, string module, int line, string message)
		{
			var msg = type switch
			{
				ErrorType.CompileError => $"[{module} line {line}] {message}\n",
				ErrorType.RuntimeError => $"{message}\n",
				ErrorType.StackTrace => $"[{module} line {line}] in {message ?? "(Unknown)"}\n",
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
			};
			_wrenErrors.Append(msg);
		}

		private void HandleWrites(Vm vm, string text)
		{
			if (_hasFail) return;

			if (_messages.Count >= _expectedMessages.Count)
			{
				_hasFail = true;
				Assert.Fail($"did not expect any more messages");
				return;
			}

			string expect = _expectedMessages[_messages.Count];
			_messages.Add(text);

			if (expect != text)
			{
				Assert.AreEqual(expect, text, $"expected message '{expect}' but got '{text}'");
				_hasFail = true;
			}
		}

		public void ExpectException<TE>() where TE : Exception => _exceptionType = typeof(TE);

		public void ExpectResult(InterpretResult result) => _expectResult = result;

		public void ExpectMessage(string msg, params string[] msgs)
		{
			_expectedMessages = new List<string>();
			_expectedMessages.Add(msg);
			_expectedMessages.AddRange(msgs);
		}

		public void ExpectMarkers(Marker marker, params Marker[] markers)
		{
			_markers = new List<Marker>();
			_markers.Add(marker);
			_markers.AddRange(markers);
		}
	}
}
