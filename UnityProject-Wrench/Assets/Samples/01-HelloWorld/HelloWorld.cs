using System;
using UnityEngine;

namespace Wrench.Samples
{
	public class HelloWorld : MonoBehaviour
	{
		private void Awake()
		{
			var vm = Vm.New();
			vm.SetWriteListener((_, text) => Debug.Log(text));
			vm.SetErrorListener((_, type, module, line, message) =>
			{
				Debug.LogError($"type:{type} module:{module} line:{line} message:{message}");
			});
			
			var result = vm.Interpret("<main>", "System.print(\"Hello World\")");
			Debug.Log($"result:{result}");
			
			vm.Dispose();
		}
	}
}
