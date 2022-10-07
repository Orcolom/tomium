using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Wrench.Weaver
{
	public static class WeaverUtils
	{
		public static bool HasAttribute<T>(this TypeDefinition td, out CustomAttribute attribute)
		{
			attribute = default;
			if (td.HasCustomAttributes == false) return false;

			for (int i = 0; i < td.CustomAttributes.Count; i++)
			{
				attribute = td.CustomAttributes[i];
				if (attribute.AttributeType.Is<T>()) return true;
			}

			return false;
		}
		
		public static bool Is<T>(this TypeReference td) => Is(td, typeof(T));

		public static bool Is(this TypeReference td, TypeReference t)
			=> td.GetElementType().FullName == t.FullName;
		public static bool Is(this TypeReference td, Type t)
		{
			if (t.IsGenericType) return td.GetElementType().FullName == t.FullName;
			return td.FullName == t.FullName;
		}
		
		public static bool IsDerivedFrom<T>(this TypeDefinition td) => IsDerivedFrom(td, typeof(T));
		public static bool IsDerivedFrom(this TypeDefinition td, Type t)
		{
			if (td == null) return false;
			if (td.IsClass == false) return false;

			// are ANY parent classes of baseClass?
			var parent = td.BaseType;

			if (parent == null) return false;
			if (parent.Is(t)) return true;

			if (parent.CanBeResolved())
				return IsDerivedFrom(parent.Resolve(), t);

			return false;
		}
		
		public static bool CanBeResolved(this TypeReference parent)
		{
			while (parent != null)
			{
				if (parent.Scope.Name == "Windows")
				{
					return false;
				}

				if (parent.Scope.Name == "mscorlib")
				{
					var resolved = parent.Resolve();
					return resolved != null;
				}

				try
				{
					parent = parent.Resolve().BaseType;
				}
				catch
				{
					return false;
				}
			}
			return true;
		}
		
		public static bool TryResolve(this TypeReference type, out TypeDefinition typeDef)
		{
			typeDef = null;
			if (type.Scope.Name == "Windows") return false;
			
			try
			{
				typeDef = type.Resolve();
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Methods have to end with a return statement. this removes it before we append the il body.
		/// Don't forget to ad a new return value at the end.
		/// </summary>
		/// <param name="body"></param>
		public static void RemoveTrailingRet(this MethodBody body)
		{
			if (body.Instructions[^1].OpCode != OpCodes.Ret) return;
			body.Instructions.RemoveAt(body.Instructions.Count - 1);
		}

		public static bool HasEmptyBody(this MethodDefinition method)
		{
			var instructions = method.Body.Instructions;
			if (instructions.Count != 4) return false;
			if (instructions[0].OpCode != OpCodes.Ldarg_0) return false;
			if (instructions[1].OpCode != OpCodes.Call) return false;
			if (instructions[2].OpCode != OpCodes.Nop) return false;
			if (instructions[3].OpCode != OpCodes.Ret) return false;
			return true;
		}
	}
}
