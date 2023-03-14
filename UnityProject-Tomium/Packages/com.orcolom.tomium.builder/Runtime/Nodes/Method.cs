using Tomia.Builder.Tokens;
using Tomia;

namespace Tomia.Builder
{
	public interface IMethodScope : INode { }

	public interface IMethodScoped : IScopedNode<IMethodScope> { }

	public class MethodBody : Block<IMethodScope> { }

	public enum MethodType
	{
		FieldGetter, // foo
		FieldSetter, // foo=(1)
		SubScriptGetter, // [...]
		SubScriptSetter, // [...]=(1)

		Is, // is(1)
		ToString, // toString {}

		Construct, // init foo(...)
		Method, // foo(...)

		StaticFieldGetter, // static foo
		StaticFieldSetter, // static foo=(1)
		StaticMethod, // static foo(...)

		Not, // !{}
		Inverse, // -{}
		Tilda, // ~{}

		Times, // *(1)
		Divide, // /(1) 
		Modulo, // %(1) 
		Plus, // +(1) 
		Minus, // -(1) 

		RangeInclusive, // ..(1)
		RangeExclusive, // ...(1)

		BitwiseLeftShift, // <<(1)
		BitwiseRightShift, // >>(1)
		BitwiseXor, // ^(1)
		BitwiseOr, // |(1)
		BitwiseAnd, // &(1)

		Equal, // ==(1)
		NotEqual, // !=(1)
		SmallerThen, // <(1)
		SmallerEqualThen, //<=(1)
		BiggerThen, // >(1)
		BiggerEqualThen, // >=(1)
	}

	public static class MethodTypeExtensions
	{
		public static bool IsStatic(this MethodType type)
		{
			return type switch
			{
				MethodType.StaticMethod => true,
				MethodType.StaticFieldGetter => true,
				MethodType.StaticFieldSetter => true,
				_ => false
			};
		}
	}

	public class Method : IMethodScope, IClassScoped
	{
		public readonly Attributes Attributes;
		public readonly ForeignMethod Foreign;
		public readonly Signature Signature;
		public readonly MethodBody Body;

		public Method(Signature signature)
		{
			Signature = signature;
			Signature.GenerateSignature();
		}

		public Method(Signature signature, ForeignMethod foreign)
		{
			Signature = signature;
			Signature.IsForeign = true;
			Signature.GenerateSignature();
			Foreign = foreign;
		}

		public Method(Signature signature, MethodBody body)
		{
			Signature = signature;
			Signature.GenerateSignature();
			Body = body;
		}

		public Method(Attributes attributes, Signature signature, ForeignMethod foreign)
		{
			Attributes = attributes;
			Signature = signature;
			Signature.IsForeign = true;
			Signature.GenerateSignature();
			Foreign = foreign;
		}

		public Method(Attributes attributes, Signature signature, MethodBody body)
		{
			Attributes = attributes;
			Signature = signature;
			Signature.GenerateSignature();
			Body = body;
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			Attributes?.Flatten(vm, tokens);
			Signature?.Flatten(vm, tokens);
			if (Foreign.IsValid) tokens.Add(Token.Eol);
			else if (Body != null)
			{
				tokens.Add(Token.Space);
				Body?.Flatten(vm, tokens);
			}
		}
	}
}
