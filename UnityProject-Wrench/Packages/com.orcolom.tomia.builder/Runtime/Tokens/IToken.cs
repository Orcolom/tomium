using System.Text;

namespace Tomia.Builder.Tokens
{
	public interface IToken
	{
		void Stringify(StringBuilder sb);
	}

	public interface INode
	{
		public void Flatten(in Vm vm, TokenCollector tokens);
	}

	// ReSharper disable once UnusedTypeParameter
	// not used for actual code, but as an contract for its children 
	public interface IScopedNode<TScope> : INode { }
}
