using System.Collections;
using System.Collections.Generic;
using Tomia.Builder.Tokens;
using Tomia;

namespace Tomia.Builder
{
	public interface IModuleScope { }

	public interface IModuleScoped : IScopedNode<IModuleScope> { }

	public class Module : IEnumerable, IModuleScoped
	{
		public readonly string Path;
		public readonly List<IModuleScoped> Tokens;

		public Dictionary<string, Class> Classes;

		public Module(string path)
		{
			Path = path;
			Tokens = new List<IModuleScoped>();
			Classes = new Dictionary<string, Class>();
		}

		public Module(string path, IModuleScoped[] tokens)
		{
			Path = path;
			Tokens = new List<IModuleScoped>(tokens);

			Classes = new Dictionary<string, Class>();
			for (int i = 0; i < Tokens.Count; i++)
			{
				AddIfClass(Tokens[i]);
			}
		}

		protected Module() { }

		public void Add(IModuleScoped node)
		{
			Tokens.Add(node);
			AddIfClass(node);
		}

		private void AddIfClass(IModuleScoped node)
		{
			if (node is not Class @class) return;

			Classes.Add(@class.Name.Text, @class);
			@class.Module = this;
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			for (int i = 0; i < Tokens.Count; i++)
			{
				Tokens[i].Flatten(vm, tokens);
			}
		}

		public T Get<T>() where T : Class
		{
			foreach (var pair in Classes)
			{
				if (pair.Value is T @class) return @class;
			}
		
			return null;
		}
		
		public bool TryFindClass(string className, out Class @class)
		{
			return Classes.TryGetValue(className, out @class);
		}

		public bool TryFindMethod(string className, bool isStatic, string signature, out Method method)
		{
			method = null;
			if (TryFindClass(className, out Class wrenClass) == false) return false;

			for (int i = 0; i < wrenClass.Methods.Count; i++)
			{
				method = wrenClass.Methods[i];
				if (isStatic != method.Signature.Type.IsStatic()) continue;
				if (method.Signature.BindingSignature == signature) return true;
			}

			return false;
		}

		public IEnumerator GetEnumerator() => Tokens.GetEnumerator();
	}
}
