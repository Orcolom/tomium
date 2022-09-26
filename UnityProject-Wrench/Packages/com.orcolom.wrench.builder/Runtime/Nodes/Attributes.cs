using System.Collections;
using System.Collections.Generic;
using Wrench.Builder.Tokens;
using Wrench;

namespace Wrench.Builder
{
	public class Attributes : INode, IEnumerable
	{
		#region Types

		public interface IScoped : IScopedNode<Attributes> { }

		#endregion

		public readonly List<IScoped> Elements;

		public Attributes(IScoped[] attributes)
		{
			Elements = new List<IScoped>(attributes);
		}

		public Attributes()
		{
			Elements = new List<IScoped>();
		}

		public void Add(IScoped scopedNodeNode)
		{
			Elements.Add(scopedNodeNode);
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			for (int i = 0; i < Elements.Count; i++)
			{
				Elements[i].Flatten(vm, tokens);
			}
		}

		/// <inheritdoc/>
		public IEnumerator GetEnumerator() => Elements.GetEnumerator();
	}
}
