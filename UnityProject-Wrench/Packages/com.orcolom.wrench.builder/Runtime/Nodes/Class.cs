using System;
using System.Collections;
using System.Collections.Generic;
using Wrench.Builder.Tokens;
using Wrench;

namespace Wrench.Builder
{
	public interface IClassScope : INode { }

	public interface IClassScoped : IScopedNode<IClassScope> { }

	public class ClassBody : Block<IClassScope>
	{
		public ClassBody(params IClassScoped[] nodes) : base(true, nodes) { }
	}

	public class Class : IClassScope, IModuleScoped, IMethodScoped, IEnumerable
	{
		public readonly Attributes Attributes;
		public bool IsForeign => Foreign.IsValid;
		public readonly ForeignClass Foreign;
		public readonly NameToken Name;
		public readonly NameToken InheritClass;
		public bool Inherits => InheritClass.IsEmpty == false;
		public Module Module { get; internal set; }

		public readonly ClassBody Body;

		internal readonly List<Method> Methods;

		public Class(string name, string inherits = null, ForeignClass foreign = default, ClassBody body = null)
		{
			Name = Token.Name(name);
			InheritClass = Token.Name(inherits, true);
			Foreign = foreign;
			Body = body ?? new ClassBody();

			Methods = new List<Method>();
			for (int i = 0; i < Body.Elements.Count; i++)
			{
				if (Body.Elements[i] is Method method)
				{
					Methods.Add(method);
				}
			}
		}

		public Class(Attributes attributes, string name, string inherits = null, ForeignClass foreign = default,
			ClassBody body = null)
		{
			Attributes = attributes;
			Name = Token.Name(name);
			InheritClass = Token.Name(inherits, true);
			Foreign = foreign;
			Body = body;

			Methods = new List<Method>();
			for (int i = 0; i < Body.Elements.Count; i++)
			{
				if (Body.Elements[i] is Method method)
				{
					Methods.Add(method);
				}
			}

			if (body == null) throw new ArgumentException("body == null");
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			Attributes?.Flatten(vm, tokens);

			if (IsForeign)
			{
				tokens.Add(Token.Foreign);
				tokens.Add(Token.Space);
			}

			tokens.Add(Token.Class);
			tokens.Add(Token.Space);
			tokens.Add(Name);

			if (Inherits)
			{
				tokens.Add(Token.Space);
				tokens.Add(Token.Is);
				tokens.Add(Token.Space);
				tokens.Add(InheritClass);
			}

			tokens.Add(Token.Space);
			Body.Flatten(vm, tokens);
		}

		public void Add(IClassScoped element)
		{
			if (element is Method method) Methods.Add(method);
			Body.Add(element);
		}

		public void Add(IClassScoped[] elements)
		{
			for (int i = 0; i < elements.Length; i++)
			{
				Add(elements[i]);
			}
		}

		public IEnumerator GetEnumerator() => Body.GetEnumerator();
	}
}
