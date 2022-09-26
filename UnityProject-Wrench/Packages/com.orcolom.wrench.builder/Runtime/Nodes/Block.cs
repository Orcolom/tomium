using System.Collections;
using System.Collections.Generic;
using Wrench.Builder.Tokens;
using Wrench;

namespace Wrench.Builder
{
	public class Block<T> : IMethodScoped, IEnumerable
		where T : INode
	{
		public readonly List<IScopedNode<T>> Elements;
		private bool _singleLine;

		public Block(params IScopedNode<T>[] nodes)
		{
			Elements = new List<IScopedNode<T>>(nodes);
		}

		public Block(bool multiLine, params IScopedNode<T>[] nodes)
		{
			_singleLine = !multiLine;
			Elements = new List<IScopedNode<T>>(nodes);
		}

		public void Add(IScopedNode<T> element)
		{
			Elements.Add(element);
		}

		// public void Add(IList<IScopedNode<T>> elementToCopy)
		// {
		// 	Elements.AddRange(elementToCopy);
		// }
		//
		// public void Add(IScopedNode<T>[] elementToCopy)
		// {
		// 	Elements.AddRange(elementToCopy);
		// }

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(Token.LeftBrace);
			if (_singleLine == false)
			{
				tokens.Add(Token.Eol);
				tokens.IncreaseIndent();
			}
			else
			{
				tokens.Add(Token.Space);
			}

			for (int i = 0; i < Elements.Count; i++)
			{
				Elements[i].Flatten(vm, tokens);
			}

			if (_singleLine == false)
			{
				tokens.Add(Token.Eol);
				tokens.DecreaseIndent();
			}
			else tokens.Add(Token.Space);

			tokens.Add(Token.RightBrace);
			tokens.Add(Token.Eol);
		}

		public IEnumerator GetEnumerator() => Elements.GetEnumerator();
	}
}
