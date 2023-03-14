using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Unity.Tomia.CodeGen")]

namespace Tomium.Builder
{
	public static class TomiaBuilderUtils
	{
		public static readonly List<string> ReservedWords = new List<string>()
		{
			"as", "break", "class", "construct", "continue", "else", "false", "for", "foreign", "if", "import", "in", "is",
			"null", "return", "static", "super", "this", "true", "var", "while",
		};

		private static readonly StringBuilder Sb = new StringBuilder();

		public static bool IsNameSafe(this string s)
		{
			if (string.IsNullOrEmpty(s)) return false;
			if (char.IsLetter(s[0]) == false) return false;

			for (int i = 1; i < s.Length; i++)
			{
				if (s[i] == '_') continue;

				if (char.IsLetterOrDigit(s[i]) == false) return false;
			}

			if (ReservedWords.Contains(s)) return false;

			return true;
		}

		public static string MakeNameSafe(this string s)
		{
			if (string.IsNullOrEmpty(s)) return "FooName";
			Sb.Clear();
			for (int i = 0; i < s.Length; i++)
			{
				if (i == 0 && char.IsLetter(s[i]) == false)
				{
					Sb.Append('z');
				}

				if (char.IsWhiteSpace(s[i]))
				{
					Sb.Append('_');
					continue;
				}

				Sb.Append(char.IsLetterOrDigit(s[i]) ? s[i] : '_');
			}

			if (ReservedWords.Contains(Sb.ToString())) Sb.Insert(0, "z");

			string str = Sb.ToString();
			Sb.Clear();
			return str;
		}

		public static string ToCamelCase(this string s)
		{
			if (s.Length > 1) return char.ToLowerInvariant(s[0]) + s.Substring(1);
			return s;
		}

		public static bool IsNumber(this string name)
		{
			if (string.IsNullOrEmpty(name)) return false;
			
			bool hasDot = false;
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '.')
				{
					if (hasDot) return false;

					hasDot = true;
					continue;
				}

				if (char.IsNumber(name[i]) == false) return false;
			}

			return true;
		}
	}
}
