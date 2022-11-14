using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Wrench.CodeGen;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using MethodBody = Mono.Cecil.Cil.MethodBody;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Wrench.Weaver
{
	public static class WeaverUtils
	{
		private const int EmptyString = 23;

		/// <summary>
		/// Gets a hash for a string. This hash will be the same on all platforms 
		/// </summary>
		/// <remarks>
		/// <see cref="string.GetHashCode"/> is not guaranteed to be the same on all platforms
		/// </remarks>
		/// based on: https://github.com/MirageNet/Mirage
		public static uint GetStableHashCode(this string text)
		{
			unchecked
			{
				var hash = EmptyString;
				for (int i = 0; i < text.Length; i++)
				{
					hash = (hash * 31) + text[i];
				}

				return (uint) hash;
			}
		}

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

		public static bool HasAttributes<T>(this ICustomAttributeProvider td, out List<CustomAttribute> attributes)
		{
			attributes = new List<CustomAttribute>();
			if (td.HasCustomAttributes == false) return false;

			for (int i = 0; i < td.CustomAttributes.Count; i++)
			{
				var attribute = td.CustomAttributes[i];
				if (attribute.AttributeType.Is<T>()) attributes.Add(attribute);
			}

			return attributes.Count > 0;
		}

		public static bool Is<T>(this TypeReference td) => Is(td, typeof(T));

		public static bool Is(this TypeReference td, TypeReference t, bool ignoreGeneric = false)
		{
			if (td is ByReferenceType tdByRef) td = tdByRef.ElementType;
			if (t is ByReferenceType tByRef) t = tByRef.ElementType;

			if (ignoreGeneric)
			{
				if (td.GetElementType().FullName != t.GetElementType().FullName) return false;
			}
			else if (td.FullName != t.FullName) return false;

			return td.IsArray == t.IsArray;
		}

		public static bool Is(this TypeReference td, Type t)
		{
			if (td is ByReferenceType tdByRef) td = tdByRef.ElementType;
			if (td.GetElementType().FullName != t.FullName) return false;
			return td.IsArray == t.IsArray;
		}

		public static bool IsDerivedFrom(this TypeReference td, TypeReference t)
		{
			int i = 0;
			return td.IsDerivedFrom(t, ref i);
		}

		public static bool IsDerivedFrom(this TypeReference td, TypeReference t, ref int depth)
		{
			while (true)
			{
				var type = td.Resolve();

				if (type.Is(t)) return true;
				if (type.BaseType == null) return false;

				depth++;
				td = type.BaseType;
			}
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

		public static void DEBUG_EmitNop(this ILProcessor il)
		{
#if DEBUG
			il.Emit(OpCodes.Nop);
#endif
		}

		public static void Emit_Ldarg_x(this ILProcessor il, int index, MethodDefinition caller)
		{
			index = caller.IsStatic ? index - 1 : index;
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
			return;
		}
	}

	public struct ConstructorInjector : IDisposable
	{
		private readonly MethodDefinition _definition;
		private readonly List<Instruction> _instructions;
		private readonly ILProcessor _il;
		private VariableDefinition[] _variables;

		public ConstructorInjector(MethodDefinition definition, out ILProcessor il)
		{
			_definition = definition;

			// we want to inject our own code in a default constructor and override the base call
			// we search the il list until we find the base call. (should be first call)
			// we safe keep all the other instruction and will re-add them at the end
			bool foundBase = false;
			_instructions = new List<Instruction>(definition.Body.Instructions.Count);
			for (int i = 0; i < definition.Body.Instructions.Count; i++)
			{
				var instruction = definition.Body.Instructions[i];
				if (foundBase == false)
				{
					if (instruction.OpCode == OpCodes.Call
						&& instruction.Operand is MethodReference methodReference
						&& methodReference.FullName.Contains("::.ctor"))
					{
						foundBase = true;
					}

					continue;
				}

				_instructions.Add(instruction);
			}

			definition.Body.Instructions.Clear();

			// save all the variables
			// its possible some of the se wont be needed anymore. TODO: is this an issue?
			_variables = new VariableDefinition[definition.Body.Variables.Count];
			for (int i = 0; i < definition.Body.Variables.Count; i++)
			{
				_variables[i] = definition.Body.Variables[i];
			}

			definition.Body.Variables.Clear();

			il = _il = definition.Body.GetILProcessor();
		}

		public void Dispose()
		{
			// add back original il code
			for (int i = 0; i < _variables.Length; i++)
			{
				_definition.Body.Variables.Add(_variables[i]);
			}

			if (_instructions.Count != 0)
			{
				for (int i = 0; i < _instructions.Count; i++)
				{
					_definition.Body.Instructions.Add(_instructions[i]);
				}
			}
			else _il.Emit(OpCodes.Ret);
		}
	}
}
