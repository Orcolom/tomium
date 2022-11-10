using System;
using System.Collections.Generic;
using Wrench.Builder.Tokens;
using Wrench;

namespace Wrench.Builder
{
	public class ImportDeferred : IModuleScoped, IMethodScoped
	{
		private readonly Action<Vm, List<Class>> _action;

		public ImportDeferred(Action<Vm, List<Class>> action)
		{
			_action = action;
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			List<Class> list = new List<Class>();
			_action?.Invoke(vm, list);

			Dictionary<string, Import> dictionary = new Dictionary<string, Import>();

			for (int i = 0; i < list.Count; i++)
			{
				var cls = list[i];
				if (dictionary.TryGetValue(cls.Module.Path, out Import import) == false)
				{
					import = new Import(cls.Module.Path);
					dictionary.Add(cls.Module.Path, import);
				}

				var index = import.Variables.FindIndex(variable => variable.Name.Text == cls.Name.Text);
				if (index != -1) return;

				import.Variables.Add(new ImportVariable(cls.Name.Text));
			}

			foreach (var pair in dictionary)
			{
				pair.Value.Flatten(vm, tokens);
			}
		}
	}
}
