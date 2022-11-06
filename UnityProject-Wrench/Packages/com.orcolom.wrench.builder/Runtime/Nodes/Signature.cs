using System.Collections.Generic;
using Wrench.Builder.Tokens;

namespace Wrench.Builder
{
	public class SignatureStyle
	{
		public enum PrefixType
		{
			None,
			Bind,
			Wren,
		}

		public bool NamedArguments { get; private set; }
		public PrefixType Prefix { get; private set; }

		private SignatureStyle() { }

		public static readonly SignatureStyle Definition = new SignatureStyle
		{
			NamedArguments = true,
			Prefix = PrefixType.Wren,
		};

		public static readonly SignatureStyle Binding = new SignatureStyle
		{
			NamedArguments = false,
			Prefix = PrefixType.Bind,
		};

		public static readonly SignatureStyle Handle = new SignatureStyle
		{
			NamedArguments = false,
			Prefix = PrefixType.None,
		};

		public static readonly SignatureStyle Call = new SignatureStyle
		{
			NamedArguments = true,
			Prefix = PrefixType.None,
		};

		public static readonly SignatureStyle Attribute = new SignatureStyle
		{
			NamedArguments = false,
			Prefix = PrefixType.Wren,
		};
	}

	public class Signature : INode
	{
		public bool IsForeign { get; internal set; }

		public readonly MethodType Type;
		public readonly string Name;
		public readonly List<string> Parameters;
		public string BindingSignature { get; private set; }

		public Signature(MethodType type, string name, params string[] parameters)
		{
			Type = type;
			Name = name;
			Parameters = new List<string>(parameters);
		}

		internal void GenerateSignature()
		{
			BindingSignature = CreateString(SignatureStyle.Binding, true, Type, Name, Parameters);
		}

		public void FlattenToCall(TokenCollector tokens)
		{
			Flatten(tokens, SignatureStyle.Call, IsForeign, Type, Name, Parameters);
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			Flatten(tokens, SignatureStyle.Definition, IsForeign, Type, Name, Parameters);
		}

		#region Builder

		private static readonly ScriptBuilder ScratchBuilder = new ScriptBuilder();

		private static readonly BasicToken InitToken = new BasicToken("init");
		private static readonly BasicToken Underscore = new BasicToken("_");

		private static readonly NameToken[] GenericArgsNames = new NameToken[]
		{
			new NameToken("arg0"),
			new NameToken("arg1"),
			new NameToken("arg2"),
			new NameToken("arg3"),
			new NameToken("arg4"),
			new NameToken("arg5"),
			new NameToken("arg6"),
			new NameToken("arg7"),
			new NameToken("arg8"),
			new NameToken("arg9"),
			new NameToken("arg10"),
			new NameToken("arg11"),
			new NameToken("arg12"),
			new NameToken("arg13"),
			new NameToken("arg14"),
			new NameToken("arg15"),
			new NameToken("arg16"),
		};

		private static readonly Dictionary<MethodType, IToken> Characters = new Dictionary<MethodType, IToken>()
		{
			{MethodType.Is, Token.Is},
			{MethodType.Times, Token.Times},
			{MethodType.Divide, Token.Divide},
			{MethodType.Modulo, Token.Modulo},
			{MethodType.Plus, Token.Plus},
			{MethodType.Minus, Token.Minus},
			{MethodType.Not, Token.Exclamation},
			{MethodType.Inverse, Token.Minus},
			{MethodType.Tilda, Token.Tilda},
			{MethodType.RangeInclusive, Token.DotDot},
			{MethodType.RangeExclusive, Token.DotDotDot},
			{MethodType.BitwiseLeftShift, Token.LeftShift},
			{MethodType.BitwiseRightShift, Token.RightShift},
			{MethodType.BitwiseXor, Token.Caret},
			{MethodType.BitwiseOr, Token.Or},
			{MethodType.BitwiseAnd, Token.And},
			{MethodType.SmallerThen, Token.LessThan},
			{MethodType.SmallerEqualThen, Token.LessThanEqual},
			{MethodType.BiggerThen, Token.MoreThan},
			{MethodType.BiggerEqualThen, Token.MoreThanEqual},
			{MethodType.Equal, Token.EqualEqual},
			{MethodType.NotEqual, Token.NotEqual},

			// this isn't a real type. but an implicit feature of a class
			{MethodType.ToString, new BasicToken("toString")},
		};

		private static List<string> _tempArguments = new List<string>(12);
		public static string CreateString(MethodType type, string name = default, int arguments = default)
		{
			_tempArguments.Clear();
			for (int i = 0; i < arguments; i++)
			{
				_tempArguments.Add(null);
			}
			
			return CreateString(SignatureStyle.Handle, false, type, name, _tempArguments);
		}

		public static Signature Create(MethodType type, string name = default, int arguments = default)
		{
			_tempArguments.Clear();
			for (int i = 0; i < arguments; i++)
			{
				_tempArguments.Add(null);
			}
			
			return new Signature(type, name, _tempArguments.ToArray());
		}
		
		private static string CreateString(SignatureStyle style, bool isForeign, MethodType type, string name, IList<string> arguments)
		{
			ScratchBuilder.Clear();
			Flatten(ScratchBuilder.Collector, style, isForeign, type, name, arguments);
			ScratchBuilder.ProcessTokens();
			return ScratchBuilder.ToString();
		}

		private static void Flatten(TokenCollector tokens, SignatureStyle style, bool isForeign,
			MethodType type, string name, IList<string> arguments)
		{
			AddForeign(tokens, isForeign, style);

			int end;
			switch (type)
			{
				case MethodType.Construct:
				case MethodType.StaticMethod:
				case MethodType.Method:
					AddStatic(tokens, style, type);
					AddConstruct(tokens, style, type);

					AddName(tokens, name);
					tokens.Add(Token.LeftParen);
					AddArgs(tokens, style, arguments, 0, GetEnd(arguments));
					tokens.Add(Token.RightParen);
					break;

				case MethodType.FieldGetter:
				case MethodType.StaticFieldGetter:
					AddStatic(tokens, style, type);
					AddName(tokens, name);
					break;

				case MethodType.FieldSetter:
				case MethodType.StaticFieldSetter:
					AddStatic(tokens, style, type);
					AddName(tokens, name);

					tokens.Add(Token.Equal);
					tokens.Add(Token.LeftParen);
					AddArg(tokens, style, arguments, 0);
					tokens.Add(Token.RightParen);
					break;

				case MethodType.SubScriptSetter:
					end = GetEnd(arguments, 2);

					tokens.Add(Token.LeftBracket);
					AddArgs(tokens, style, arguments, 0, end - 1);
					tokens.Add(Token.RightBracket);
					tokens.Add(Token.Equal);
					tokens.Add(Token.LeftParen);
					AddArg(tokens, style, arguments, end - 1);
					tokens.Add(Token.RightParen);
					break;

				case MethodType.SubScriptGetter:
					end = GetEnd(arguments, 1);
					tokens.Add(Token.LeftBracket);
					AddArgs(tokens, style, arguments, 0, end);
					tokens.Add(Token.RightBracket);
					break;

				case MethodType.Times:
				case MethodType.Divide:
				case MethodType.Modulo:
				case MethodType.Plus:
				case MethodType.Minus:
				case MethodType.RangeInclusive:
				case MethodType.RangeExclusive:
				case MethodType.BitwiseLeftShift:
				case MethodType.BitwiseRightShift:
				case MethodType.BitwiseXor:
				case MethodType.BitwiseOr:
				case MethodType.BitwiseAnd:
				case MethodType.SmallerThen:
				case MethodType.SmallerEqualThen:
				case MethodType.BiggerThen:
				case MethodType.BiggerEqualThen:
				case MethodType.Equal:
				case MethodType.NotEqual:
				case MethodType.Is:
					AddSign(tokens, type);
					tokens.Add(Token.LeftParen);
					AddArg(tokens, style, arguments, 0);
					tokens.Add(Token.RightParen);
					break;

				case MethodType.ToString:
				case MethodType.Not:
				case MethodType.Inverse:
				case MethodType.Tilda:
					AddSign(tokens, type);
					break;
			}
		}

		public static bool IgnoresName(MethodType type)
		{
			return Characters.ContainsKey(type);
		}

		private static void AddForeign(TokenCollector tokens, bool isForeign, SignatureStyle style)
		{
			if (isForeign == false) return;
			if (style.Prefix != SignatureStyle.PrefixType.Wren) return;

			tokens.Add(Token.Foreign);
			tokens.Add(Token.Space);
		}

		private static void AddConstruct(TokenCollector tokens, SignatureStyle style, MethodType type)
		{
			if (type != MethodType.Construct) return;

			switch (style.Prefix)
			{
				case SignatureStyle.PrefixType.None: return;

				case SignatureStyle.PrefixType.Bind:
					tokens.Add(InitToken);
					tokens.Add(Token.Space);
					return;

				case SignatureStyle.PrefixType.Wren:
					tokens.Add(Token.Construct);
					tokens.Add(Token.Space);
					break;
			}
		}

		private static void AddStatic(TokenCollector tokens, SignatureStyle style, MethodType type)
		{
			if (type.IsStatic() == false) return;
			if (style.Prefix != SignatureStyle.PrefixType.Wren) return;

			tokens.Add(Token.Static);
			tokens.Add(Token.Space);
		}

		private static void AddArgs(TokenCollector tokens, SignatureStyle style, IList<string> arguments, int start,
			int end)
		{
			for (int i = start; i < end; i++)
			{
				AddArg(tokens, style, arguments, i);
				if (i + 1 < end) tokens.Add(Token.Comma);
			}
		}

		private static int GetEnd(IList<string> arguments, int min = 0)
		{
			if (arguments == null || arguments.Count < min) return min;

			return arguments.Count;
		}

		private static void AddArg(TokenCollector tokens, SignatureStyle style, IList<string> arguments, int index)
		{
			if (style.NamedArguments == false)
			{
				tokens.Add(Underscore);
			}
			else
			{
				if (arguments == null || arguments.Count <= 0)
				{
					tokens.Add(GenericArgsNames[0]);
					return;
				}

				string arg = arguments[index];
				if (string.IsNullOrEmpty(arg))
				{
					tokens.Add(GenericArgsNames[index]);
					return;
				}

				AddName(tokens, arg);
			}
		}

		private static void AddName(TokenCollector tokens, string name)
		{
			// REVIEW: make a cached version of scratch arguments?
			tokens.Add(Token.Name(name));
		}

		private static void AddSign(TokenCollector tokens, MethodType type)
		{
			tokens.Add(Characters[type]);
		}

		#endregion
	}
}
