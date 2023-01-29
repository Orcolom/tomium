using System.Globalization;
using System.Text;

namespace Tomia.Builder.Tokens
{
	public class NumberToken : IToken, IModuleScoped, IMethodScoped
	{
		private double _value;

		internal NumberToken(double value)
		{
			_value = value;
		}

		public void Stringify(StringBuilder sb)
		{
			sb.Append(_value.ToString(CultureInfo.InvariantCulture));
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(this);
		}
	}
}
