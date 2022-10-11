﻿using System;

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

		public WrenchClassAttribute(Type moduleType, string name)
		{
			ModuleType = moduleType;
			Name = name;
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
}
