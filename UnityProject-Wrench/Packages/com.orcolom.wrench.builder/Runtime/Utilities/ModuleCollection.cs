using System;
using System.Collections;
using System.Collections.Generic;

namespace Wrench.Builder
{
	public class ModuleCollection : IEnumerable
	{
		private static ScriptBuilder _wb = new ScriptBuilder();
		private Dictionary<string, Module> _modules = new Dictionary<string, Module>();
		private List<Class> _classes = new List<Class>();
		
		public ModuleCollection()
		{
		}

		public event Action<Module, string> ModuleLoadEvent;
		
		public void Add(Module module)
		{
			_modules.Add(module.Path, module);
			foreach (var pair in module.Classes)
			{
				_classes.Add(pair.Value);
			}
		}
		
		public void AddRange(Module[] modules)
		{
			for (int i = 0; i < modules.Length; i++)
			{
				Add(modules[i]);
			}
		}

		public T Get<T>() where T : Class
		{
			for (int i = 0; i < _classes.Count; i++)
			{
				if (_classes[i] is T @class) return @class;
			}
		
			return null;
		}
		
		public ForeignMethod BindForeignMethodHandler(in Vm vm, string module, string classname, bool isstatic,
			string signature)
		{
			if (_modules.TryGetValue(module, out Module wrenModule) == false) return default;
			if (wrenModule.TryFindMethod(classname, isstatic, signature, out Method wrenMethod) == false) return default;
			return wrenMethod.Foreign;
		}

		public ForeignClass BindForeignClassHandler(in Vm vm, string module, string classname)
		{
			if (_modules.TryGetValue(module, out Module wrenModule) == false) return default;
			if (wrenModule.TryFindClass(classname, out Class wrenClass) == false) return default;
			return wrenClass.Foreign;
		}

		public string LoadModuleHandler(in Vm vm, string module)
		{
			if (_modules.TryGetValue(module, out Module wrenModule) == false) return null;
			string str = _wb.CreateModuleSource(vm, wrenModule);
			ModuleLoadEvent?.Invoke(wrenModule, str);
			return str;
		}

		public IEnumerator GetEnumerator() => _modules.GetEnumerator();
	}
}
