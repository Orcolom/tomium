using System.Text;

namespace Tomia.Builder.Tokens
{
	public class NameToken : IToken, IModuleScoped, IMethodScoped
	{
		public string Text { get; }
		public bool IsEmpty => string.IsNullOrEmpty(Text);

		internal NameToken(string text)
		{
			Text = text;
		}

		public void Stringify(StringBuilder sb)
		{
			sb.Append(Text);
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(this);
		}
	}
}
