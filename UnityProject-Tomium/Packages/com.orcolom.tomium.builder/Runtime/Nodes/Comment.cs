using System.Text;
using Tomium;
using Tomium.Builder.Tokens;

namespace Tomium.Builder
{
	public class Comment : IToken, IModuleScoped, IClassScoped, IMethodScoped
	{
		public readonly bool IsMultiLine;
		public readonly string Text;

		public Comment(string text)
		{
			IsMultiLine = text.Contains("\n") || text.Contains("\r\n");
			Text = text;
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(this);
		}

		public void Stringify(StringBuilder sb)
		{
			if (IsMultiLine == false)
			{
				Token.Comment.Stringify(sb);
				Token.Space.Stringify(sb);
			}
			else
			{
				Token.OpenComment.Stringify(sb);
				Token.Eol.Stringify(sb);
			}

			sb.Append(Text);

			if (IsMultiLine)
			{
				Token.Eol.Stringify(sb);
				Token.CloseComment.Stringify(sb);
			}

			Token.Eol.Stringify(sb);
		}
	}
}
