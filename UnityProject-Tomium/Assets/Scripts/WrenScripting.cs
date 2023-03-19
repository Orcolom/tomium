using System.Text;
using Binding;
using Unity.Profiling;
using UnityEngine;
using Tomium;
using Tomium.Builder;
using Tomium.Samples.UnityBinding;

public class WrenScripting : MonoBehaviour
{
	[SerializeField]
	private WrenScript _script;

	private Vm _vm;
	private ModuleCollection _modules;

	private static readonly ProfilerMarker PrefModuleCollections = ProfilerUtils.Create("ModuleCollections");
	private static readonly ProfilerMarker PrefNew = ProfilerUtils.Create("New");

	private Handle _handle;
	
	private readonly StringBuilder _writeBuffer = new StringBuilder();
	private readonly StringBuilder _errorBuffer = new StringBuilder();

	private void Awake()
	{
		Debug.Log(Tomium.Tomium.CurrentWrenVersionSemVer);
	}

	void Start()
	{
		PrefModuleCollections.Begin();
		_modules = new ModuleCollection();
		// var dModule = new DummyModule();
		var uModule = new UnityModule();
		// _modules.Add(dModule);
		_modules.Add(uModule);
		_modules.Add(new UtilityModule());
		PrefModuleCollections.End();
		
		PrefNew.Begin();
		_vm = Vm.New();
		
		_vm.SetWriteListener((_, text) =>
		{
			if (text == "\n")
			{
				Debug.Log(_writeBuffer);
				_writeBuffer.Clear();
			} else _writeBuffer.Append(text);
		});
			
		_vm.SetErrorListener((_, type, module, line, message) =>
		{
			string str = type switch
			{
				ErrorType.CompileError => $"[{module} line {line}] {message}",
				ErrorType.RuntimeError => message,
				ErrorType.StackTrace => $"[{module} line {line}] in {message}",
				_ => string.Empty,
			};
				
			if (type == ErrorType.CompileError) Debug.LogWarning(str);
			else if (type == ErrorType.StackTrace)
			{
				_errorBuffer.AppendLine(str);
				Debug.LogWarning(_errorBuffer);
				if (message == "(script)") _errorBuffer.Clear();
			} else _errorBuffer.AppendLine(str);
		});
		
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
