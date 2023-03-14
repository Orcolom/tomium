using Tomium.Builder.Tokens;


namespace Tomium.Builder
{
	public class AttributeVariable : INode
	{
		public readonly NameToken Name;
		public readonly IToken Value;

		public AttributeVariable(string name)
		{
			Name = Token.Name(name);
		}

		public AttributeVariable(string name, bool value)
		{
			Name = Token.Name(name);
			Value = value ? Token.True : Token.False;
		}

		public AttributeVariable(string name, string value)
		{
			Name = Token.Name(name);
			Value = BuilderUtils.IsNameSafe(value) ? Token.Name(value) : Token.String(value);
		}

		public AttributeVariable(string name, double value)
		{
			Name = Token.Name(name);
			Value = Token.Number(value);
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(Name);
			if (Value == null) return;

			tokens.Add(Token.Equal);
			tokens.Add(Value);
		}
	}
}
