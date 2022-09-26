using System.Collections.Generic;
using System.Text;
using Wrench.Builder;
using Wrench.Builder.Tokens;

namespace Wrench.Builder
{
	public class TokenCollector
	{
		public List<IToken> Tokens = new List<IToken>();

		private bool _hadEol;
		private int _indent;
		public void IncreaseIndent() => _indent++;
		public void DecreaseIndent() => _indent--;

		internal TokenCollector() { }

		private void TryIndent()
		{
			if (_hadEol == false) return;
			_hadEol = false;

			Indents(_indent);
		}

		private void Indents(int count)
		{
			for (int i = 0; i < count; i++)
			{
				Add(Token.Tab);
			}
		}
		
		public void Add(IToken token)
		{
			TryIndent();
			Tokens.Add(token);
			if (token is EolToken) _hadEol = true;
		}

		public void Process(StringBuilder sb)
		{
			for (int i = 0; i < Tokens.Count; i++)
			{
				Tokens[i].Stringify(sb);
			}
		}

		public void Clear()
		{
			Tokens.Clear();
		}
	}
}
