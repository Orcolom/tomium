using System;

namespace Wrench.Builder
{
	[AttributeUsage(AttributeTargets.Class)]
	public class WrenchImport : System.Attribute
	{
		public readonly Type Type;

		public WrenchImport(Type type)
		{
			Type = type;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class WrenchModuleAttribute : System.Attribute
	{
		public readonly string Path;

		public WrenchModuleAttribute(string path)
		{
			Path = path;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class WrenchClassAttribute : System.Attribute
	{
		public readonly string Name;
		public readonly Type ModuleType;
		public readonly Type ForType;
		public readonly string Inherit;

		public WrenchClassAttribute(Type moduleType, string name, Type forType = null, string inherit = null)
		{
			ModuleType = moduleType;
			Name = name;
			ForType = forType;
			Inherit = inherit;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WrenchMethodAttribute : System.Attribute
	{
		public readonly MethodType MethodType;
		// public string Name;

		public WrenchMethodAttribute(MethodType methodType)
		{
			MethodType = methodType;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WrenchExpectAttribute : System.Attribute
	{
		public readonly Type Type;
		public readonly bool UseForChildren;

		public WrenchExpectAttribute(Type type, bool useForChildren = false)
		{
			Type = type;
			UseForChildren = useForChildren;
		}
	}
}
