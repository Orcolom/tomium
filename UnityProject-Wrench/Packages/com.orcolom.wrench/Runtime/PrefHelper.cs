using System;
using System.Diagnostics;
using Unity.Profiling;

namespace Wrench
{
	public static class PrefHelper
	{
		public static ProfilerCategory Category = new ProfilerCategory("Wrench", ProfilerCategoryColor.Scripts);
		public static ProfilerMarker Create(string name) => new ProfilerMarker(Category, $"wrench.{name}");
		
		// ReSharper disable Unity.PerformanceAnalysis
		[DebuggerHidden, DebuggerStepThrough]
		internal static void ThrowException<T>(T exception)
			where T:Exception
		{
			// // throw the exception -> this will fill its callstack
			// // then debug log -> this will log it and send it to unity dashboard
			// // don't actually throw so that the code doesn't deadlock
			//
			// try
			// {
			throw exception;
			// }
			// catch (Exception e)
			// {
			// 	Debug.LogException(e);
			// }
		}
	}
}
