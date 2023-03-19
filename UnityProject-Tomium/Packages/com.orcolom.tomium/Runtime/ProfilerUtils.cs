using System;
using System.Diagnostics;
using Unity.Profiling;
using Unity.Profiling.LowLevel;

namespace Tomium
{
	public static class ProfilerUtils
	{
		public static ProfilerCategory Category = new ProfilerCategory("Tomium", ProfilerCategoryColor.Scripts);
		public static ProfilerMarker Create(string name) => new ProfilerMarker(Category, name, MarkerFlags.Script | MarkerFlags.Counter);
		
		public static ProfilerMarker AllocScope = Create("Tomium.Alloc");

		[Conditional("TOMIUM_DEBUG")]
		public static void Log(string s)
		{
			UnityEngine.Debug.Log($"[TOMIUM] {s}");
		}
	}
}
