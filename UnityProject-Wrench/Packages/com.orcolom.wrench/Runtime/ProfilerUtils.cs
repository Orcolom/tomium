using System;
using System.Diagnostics;
using Unity.Profiling;
using Unity.Profiling.LowLevel;

namespace Wrench
{
	public static class ProfilerUtils
	{
		public static ProfilerCategory Category = new ProfilerCategory("Wrench", ProfilerCategoryColor.Scripts);
		public static ProfilerMarker Create(string name) => new ProfilerMarker(Category, name, MarkerFlags.Script | MarkerFlags.Counter);
		
		public static ProfilerMarker AllocScope = Create("Wrench.Alloc");
	}
}
