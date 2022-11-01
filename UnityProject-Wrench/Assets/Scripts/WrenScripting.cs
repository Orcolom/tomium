using System.Collections;
using System.Collections.Generic;
using Binding;
using UnityEngine;
using Wrench;
using Wrench.Builder;

public class WrenScripting : MonoBehaviour
{
	[SerializeField]
	private WrenScript _script;

	private Vm _vm;
	private ModuleCollection _modules;

	void Start()
	{
		_modules = new ModuleCollection();
		var dModule = new DummyModule();
		var uModule = new UnityModule();
		_modules.Add(dModule);
		_modules.Add(uModule);

		_vm = Vm.New();
		_vm.SetErrorListener((_, type, module, line, message) =>
			Debug.LogError($"{type}: {module} {line} {message}"));
		_vm.SetWriteListener((_, text) => Debug.Log(text));

		_vm.SetLoadModuleListener((vm, s) =>
		{
			var str = _modules.LoadModuleHandler(vm, s);
			Debug.LogWarning($"Load {s}:\n{str}");
			return str;
		});
		
		_vm.SetBindForeignClassListener(_modules.BindForeignClassHandler);
		_vm.SetBindForeignMethodListener(_modules.BindForeignMethodHandler);

		_vm.Interpret("<script>", _script.Text);
	}
}
