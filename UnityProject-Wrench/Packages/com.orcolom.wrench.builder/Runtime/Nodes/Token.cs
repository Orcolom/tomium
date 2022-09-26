using System;
using Wrench.Builder.Tokens;

namespace Wrench.Builder
{
	public static class Token
	{
		// keywords
		public static readonly BasicToken As = new BasicToken("as");
		public static readonly BasicToken Break = new BasicToken("break");
		public static readonly BasicToken Class = new BasicToken("class");
		public static readonly BasicToken Construct = new BasicToken("construct");
		public static readonly BasicToken Else = new BasicToken("else");
		public static readonly BasicToken For = new BasicToken("for");
		public static readonly BasicToken Foreign = new BasicToken("foreign");
		public static readonly BasicToken If = new BasicToken("if");
		public static readonly BasicToken Import = new BasicToken("import");
		public static readonly BasicToken In = new BasicToken("in");
		public static readonly BasicToken Is = new BasicToken("is");
		public static readonly BasicToken Null = new BasicToken("null");
		public static readonly BasicToken Return = new BasicToken("return");
		public static readonly BasicToken Static = new BasicToken("static");
		public static readonly BasicToken Super = new BasicToken("super");
		public static readonly BasicToken This = new BasicToken("this");
		public static readonly BasicToken Var = new BasicToken("var");
		public static readonly BasicToken While = new BasicToken("while");
		public static readonly BoolToken True = new BoolToken(true);
		public static readonly BoolToken False = new BoolToken(false);

		// punctuation
		public static readonly BasicToken Comma = new BasicToken(",");
		public static readonly BasicToken LeftBrace = new BasicToken("{");
		public static readonly BasicToken RightBrace = new BasicToken("}");
		public static readonly BasicToken LeftBracket = new BasicToken("[");
		public static readonly BasicToken RightBracket = new BasicToken("]");
		public static readonly BasicToken LeftParen = new BasicToken("(");
		public static readonly BasicToken RightParen = new BasicToken(")");
		public static readonly BasicToken Exclamation = new BasicToken("!");
		public static readonly BasicToken Hash = new BasicToken("#");
		public static readonly BasicToken Tilda = new BasicToken("~");
		public static readonly BasicToken Caret = new BasicToken("^");
		public static readonly BasicToken Equal = new BasicToken("=");
		public static readonly BasicToken EqualEqual = new BasicToken("==");
		public static readonly BasicToken NotEqual = new BasicToken("!=");

		public static readonly BasicToken Times = new BasicToken("*");
		public static readonly BasicToken Divide = new BasicToken("/");
		public static readonly BasicToken Plus = new BasicToken("+");
		public static readonly BasicToken Minus = new BasicToken("-");
		public static readonly BasicToken Modulo = new BasicToken("%");

		public static readonly BasicToken Dot = new BasicToken(".");
		public static readonly BasicToken DotDot = new BasicToken("..");
		public static readonly BasicToken DotDotDot = new BasicToken("...");

		public static readonly BasicToken LessThan = new BasicToken("<");
		public static readonly BasicToken MoreThan = new BasicToken(">");
		public static readonly BasicToken LessThanEqual = new BasicToken("<=");
		public static readonly BasicToken MoreThanEqual = new BasicToken(">=");

		public static readonly BasicToken LeftShift = new BasicToken("<<");
		public static readonly BasicToken RightShift = new BasicToken(">>");
		public static readonly BasicToken And = new BasicToken("&");
		public static readonly BasicToken AndAnd = new BasicToken("&&");
		public static readonly BasicToken Or = new BasicToken("|");
		public static readonly BasicToken OrOr = new BasicToken("||");

		// comments
		public static readonly BasicToken Comment = new BasicToken("//");
		public static readonly BasicToken OpenComment = new BasicToken("/*");
		public static readonly BasicToken CloseComment = new BasicToken("*/");

		// whitespace
		public static readonly BasicToken Tab = new BasicToken("\t");
		public static readonly BasicToken Space = new BasicToken(" ");
		public static readonly EolToken Eol = new EolToken();

		public static StringToken String(string text) => new StringToken(text);
		public static NumberToken Number(double value) => new NumberToken(value);

		public static NameToken Name(string text, bool canBeNull = false, MethodType? type = null)
		{
			if (canBeNull == false && text.IsNameSafe() == false && text.IsNumber() == false)
			{
				throw new ArgumentException(
					$"`{text}` is not name safe (or a number). eg. `{text.MakeNameSafe()}` or 12.345");
			}

			return new NameToken(text);
		}

		public static BasicToken DangerousInsert(string text) => new BasicToken(text);
	}
}
