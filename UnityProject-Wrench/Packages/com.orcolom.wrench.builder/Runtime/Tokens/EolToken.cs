using System.Text;

namespace Wrench.Builder.Tokens
{
	public class EolToken : IToken, IModuleScoped, IMethodScoped, IClassScoped
	{
		internal EolToken() { }

		public void Stringify(StringBuilder sb)
		{
			sb.Append("\n");
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(this);
		}
	}
}
