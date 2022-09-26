using System.Text;

namespace Wrench.Builder.Tokens
{
	public class BasicToken : IToken, IModuleScoped, IClassScoped, IMethodScoped
	{
		private readonly string _char;

		internal BasicToken(string characters)
		{
			_char = characters;
		}

		public void Stringify(StringBuilder sb)
		{
			sb.Append(_char);
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(this);
		}
	}
}
