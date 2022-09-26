using System;
using Wrench.Builder.Tokens;
using Wrench;

namespace Wrench.Builder
{
	public class ImportVariable : INode
	{
		public NameToken Name;
		public NameToken As;
		public bool HasRename => As.IsEmpty == false;

		public ImportVariable(string name, string @as = null)
		{
			Name = Token.Name(name);
			As = Token.Name(@as, true);
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(Name);
			if (HasRename == false) return;

			tokens.Add(Token.Space);
			tokens.Add(Token.As);
			tokens.Add(Token.Space);
			tokens.Add(As);
		}

		public static implicit operator ImportVariable(string name) => new ImportVariable(name);

		public static implicit operator ImportVariable(ValueTuple<string, string> name) =>
			new ImportVariable(name.Item1, name.Item2);
	}
}
