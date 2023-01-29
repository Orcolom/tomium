using System;
using System.Collections.Generic;
using Tomia.Builder.Tokens;
using Tomia;

namespace Tomia.Builder
{
	public class Import : IModuleScoped, IMethodScoped
	{
		public readonly StringToken Path;
		public readonly List<ImportVariable> Variables;

		public Import(string path, params ImportVariable[] variables)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentException($"`{path}` can not be null");

			Path = Token.String(path);
			Variables = new List<ImportVariable>(variables);
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(Token.Import);
			tokens.Add(Token.Space);
			tokens.Add(Path);

			if (Variables.Count != 0)
			{
				tokens.Add(Token.Space);
				tokens.Add(Token.For);
				for (int i = 0; i < Variables.Count; i++)
				{
					tokens.Add(Token.Space);
					Variables[i].Flatten(vm, tokens);
					if (i + 1 != Variables.Count) tokens.Add(Token.Comma);
				}
			}

			tokens.Add(Token.Eol);
		}
	}
}
