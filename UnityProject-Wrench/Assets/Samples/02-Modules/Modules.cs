using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrench.Samples
{
	public class Modules : MonoBehaviour
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

			vm.SetResolveModuleListener((_, importer, module) =>
			{
				Debug.Log($"[import] importer:{importer}  module:{module}");
				if (importer == "<main>" && module == "hw") return "hello_world";
				return module;
			});
			
			vm.SetLoadModuleListener((_, module) =>
			{
				Debug.Log($"[load] module:{module}");
				if (module == "hello_world") return GetTimeModule();
				return null;
			});


			vm.Interpret("<main>", "import \"hw\" for Time \n System.print(Time) \n System.print(Time)");
			
			
			vm.Dispose();
			
			SampleRunner.NextSample();
		}

		private string GetTimeModule()
		{
			return $"var Time = \"{DateTime.Now.ToShortDateString()}\"";
		}
	}
}
