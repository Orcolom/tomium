using System;

namespace Wrench.Builder
{
	public class WrenchModuleAttribute : System.Attribute
	{
		public readonly string Path;

		public WrenchModuleAttribute(string path)
		{
			Path = path;
		}
	}

	public class WrenchClassAttribute : System.Attribute
	{
		public readonly string Name;
		public readonly Type ModuleType;
		public readonly Type ForType;

		public WrenchClassAttribute(Type moduleType, string name, Type forType = null)
		{
			ModuleType = moduleType;
			Name = name;
			ForType = forType;
		}
	}

	public class WrenchMethodAttribute : System.Attribute
	{
		public readonly MethodType MethodType;
		// public string Name;

		public WrenchMethodAttribute(MethodType methodType)
		{
			MethodType = methodType;
		}
	}

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
