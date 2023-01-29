using System;
using System.Text;
using UnityEngine;

namespace Tomia.Samples
{
	public class HelloWorld : MonoBehaviour
	{
		private void Start()
		{
			var vm = Vm.New();
			vm.SetWriteListener((_, text) => Debug.Log(text));
			vm.SetErrorListener((_, type, module, line, message) =>
			{
				string str = type switch
				{
					ErrorType.CompileError => $"[{module} line {line}] {message}",
					ErrorType.RuntimeError => message,
					ErrorType.StackTrace => $"[{module} line {line}] in {message}",
					_ => string.Empty,
				};
				Debug.LogError(str);
			});
			
			Debug.Log("Works");
			var result = vm.Interpret("<main>", "var CallMe = Fn.new{|arg|\nSystem.print(\"Hello World %(arg)\")\n}"
			);
			
			vm.EnsureSlots(2);
			vm.Slot0.GetVariable("<main>", "CallMe");
			vm.Slot1.SetString("\n-From Tamia");
			using (var handle = vm.MakeCallHandle("call(_)"))
			{
				vm.Call(handle);
			}
			
			Debug.Log($"result:{result}");

			Debug.Log("Compile Errors");
			result = vm.Interpret("<main>", "Sys.print(\"Hello World\")");
			Debug.Log($"result:{result}");

			Debug.Log("Runtime Errors");
			result = vm.Interpret("<main>", "System.do(\"Hello World\")");
			Debug.Log($"result:{result}");

			vm.Dispose();

			SampleRunner.NextSample();
		}
	}
}
