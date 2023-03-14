using System.Text;

namespace Tomium.Builder.Tokens
{
	public class StringToken : IToken, IModuleScoped, IMethodScoped
	{
		private string _text;

		internal StringToken(string text)
		{
			_text = text;
		}

		public void Stringify(StringBuilder sb)
		{
			// REVIEW: should " be a token?
			sb.Append('\"');
			sb.Append(_text);
			sb.Append('\"');
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(this);
		}
	}
}
