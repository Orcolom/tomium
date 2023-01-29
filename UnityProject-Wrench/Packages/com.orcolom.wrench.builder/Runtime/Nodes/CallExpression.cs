﻿using Tomia.Builder.Tokens;
using Tomia;

namespace Tomia.Builder
{
	public class CallExpression : IModuleScoped, IMethodScoped
	{
		private NameToken Obj;
		private Signature Call;

		public CallExpression(NameToken obj, Signature call)
		{
			Obj = obj;
			Call = call;
		}

		public void Flatten(in Vm vm, TokenCollector tokens)
		{
			tokens.Add(Obj);
			tokens.Add(Token.Dot);
			Call.FlattenToCall(tokens);
		}
	}
}
