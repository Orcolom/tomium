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
		_modules.Add(new UnityModule());

		_vm = Vm.New();
		_vm.SetErrorListener((in Vm vm, ErrorType type, string module, int line, string message) =>
			Debug.LogError($"{type}: {module} {line} {message}"));
		_vm.SetWriteListener((in Vm vm, string text) => Debug.Log(text));

		_vm.SetLoadModuleListener(_modules.LoadModuleHandler);
		_vm.SetBindForeignClassListener(_modules.BindForeignClassHandler);
		_vm.SetBindForeignMethodListener(_modules.BindForeignMethodHandler);

		_vm.Interpret("<script>", _script.Text);
	}
}
