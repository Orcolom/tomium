using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Wrench.Weaver
{
	public static class WeaverUtils
	{
		public static bool HasAttribute<T>(this ICustomAttributeProvider td, out CustomAttribute attribute)
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

		public static void DEBUG_EmitNop(this ILProcessor il)
		{
#if DEBUG
			il.Emit(OpCodes.Nop);
#endif
		}
		
		public static void Emit_Ldarg_x(this ILProcessor il, int index, MethodDefinition methodDefinition)
		{
			index = methodDefinition.IsStatic ? index - 1 : index;
			var op = index switch
			{
				-1 => OpCodes.Nop,
				0 => OpCodes.Ldarg_0,
				1 => OpCodes.Ldarg_1,
				2 => OpCodes.Ldarg_2,
				3 => OpCodes.Ldarg_3,
				_ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
			};

			if (op == OpCodes.Nop) return;
			il.Emit(op);
		}
	}
}
