using System;
using System.Collections;
using System.Collections.Generic;
using Binding;
using Unity.Profiling;
using UnityEngine;
using Wrench;
using Wrench.Builder;

public class WrenScripting : MonoBehaviour
{
	[SerializeField]
	private WrenScript _script;

	private Vm _vm;
	private ModuleCollection _modules;

	private static readonly ProfilerMarker PrefModuleCollections = ProfilerUtils.Create("ModuleCollections");
	private static readonly ProfilerMarker PrefNew = ProfilerUtils.Create("New");

	private Handle _handle;


	private void Awake()
	{
		Debug.Log(Wrench.Wrench.CurrentWrenVersionSemVer);
	}

	void Start()
	{
		PrefModuleCollections.Begin();
		_modules = new ModuleCollection();
		// var dModule = new DummyModule();
		var uModule = UnityModule.Instance;
		// _modules.Add(dModule);
		_modules.Add(uModule);
		_modules.Add(new UtilityModule());
		PrefModuleCollections.End();
		
		PrefNew.Begin();
		_vm = Vm.New();
		
		_vm.SetErrorListener((_, type, module, line, message) =>
			Debug.LogError($"{type}: {module} {line} {message}"));
		_vm.SetWriteListener((_, text) => Debug.Log(text));
		_vm.SetLoadModuleListener((vm, path) =>
		{
			var str = _modules.LoadModuleHandler(vm, path);
			Debug.LogWarning($"Load `{path}`\n{str}");
			return str;
		});
			
		_vm.SetBindForeignClassListener(_modules.BindForeignClassHandler);
		_vm.SetBindForeignMethodListener(_modules.BindForeignMethodHandler);
		PrefNew.End();
		
		var result = _vm.Interpret("<script>", _script.Text);
		// Debug.Log("x");
		enabled = result == InterpretResult.Success;
		
		_vm.EnsureSlots(1);
		_vm.Slot0.GetVariable("<script>", "X");
		_handle = _vm.Slot0.GetHandle();
	}

	private void Update()
	{
		if (_vm.IsValid() == false) return;
		using var handle = _vm.MakeCallHandle("call()");
		_vm.EnsureSlots(1);
		_vm.Slot0.SetHandle(_handle);
		_vm.Call(handle);
	}
}
