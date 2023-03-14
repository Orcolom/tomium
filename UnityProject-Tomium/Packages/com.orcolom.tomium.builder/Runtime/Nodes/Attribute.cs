using System;
using System.Collections.Generic;
using Tomium;
using Tomium.Builder.Tokens;

namespace Tomium.Builder
{
	public class Attribute : Attributes.IScoped
	{
		public readonly bool IsRuntime;
		public readonly NameToken GroupName;
		public readonly List<AttributeVariable> Attributes;
		public bool IsGroup => GroupName != null;

		public Attribute(bool runtime, string name)
		{
			IsRuntime = runtime;
			Attributes = new List<AttributeVariable>() {new AttributeVariable(name),};
		}

		public Attribute(bool runtime, string group, AttributeVariable first,
			params AttributeVariable[] attributes)
		{
			IsRuntime = runtime;
			GroupName = Token.Name(@group);
			Attributes = new List<AttributeVariable>(attributes.Length + 1) {first};
			Attributes.AddRange(attributes);

			for (int i = 0; i < Attributes.Count; i++)
			{
				if (Attributes[i] == null) throw new ArgumentException();
			}
		}

		public Attribute(bool runtime, string name, bool value)
		{
			IsRuntime = runtime;
			Attributes = new List<AttributeVariable>() {new AttributeVariable(name, value),};
		}

		public Attribute(bool runtime, string name, string value)
		{
			IsRuntime = runtime;
			Attributes = new List<AttributeVariable>() {new AttributeVariable(name, value),};
		}

		public Attribute(bool runtime, string name, double value)
		{
			IsRuntime = runtime;
			Attributes = new List<AttributeVariable>() {new AttributeVariable(name, value),};
		}

		public void Flatten(in Vm vm, TokenCollector nodes)
		{
			nodes.Add(Token.Hash);
			if (IsRuntime) nodes.Add(Token.Exclamation);

			if (IsGroup)
			{
				nodes.Add(GroupName);
				nodes.Add(Token.LeftParen);
				nodes.Add(Token.Eol);
				nodes.IncreaseIndent();
			}

			for (int i = 0; i < Attributes.Count; i++)
			{
				Attributes[i].Flatten(vm, nodes);
				if (i + 1 < Attributes.Count)
				{
					nodes.Add(Token.Comma);
					nodes.Add(Token.Eol);
				}
			}

			if (IsGroup)
			{
				nodes.DecreaseIndent();
				nodes.Add(Token.Eol);
				nodes.Add(Token.RightParen);
			}

			nodes.Add(Token.Eol);
		}
	}
}
