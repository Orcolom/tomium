using System;
using Mono.Cecil;

namespace Wrench.Weaver
{
	public static class WeaverUtils
	{
		public static bool Is<T>(this TypeReference td) => Is(td, typeof(T));
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
	}
}
