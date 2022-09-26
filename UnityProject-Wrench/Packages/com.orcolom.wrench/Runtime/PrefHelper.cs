using Unity.Profiling;

namespace Wrench
{
	public static class PrefHelper
	{
		public static ProfilerCategory Category = new ProfilerCategory("Wrench", ProfilerCategoryColor.Scripts);
		public static ProfilerMarker Create(string name) => new ProfilerMarker(Category, $"wrench.{name}");
	}
}
