using System.Text;

namespace Wrench.Builder.Tokens
{
	public class BoolToken : IToken, IModuleScoped, IMethodScoped
	{
		private bool _state;

		internal BoolToken(bool state)
		{
			_state = state;
		}

		public void Stringify(StringBuilder sb)
		{
			sb.Append(_state.ToString().ToLower());
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(this);
		}
	}
}
