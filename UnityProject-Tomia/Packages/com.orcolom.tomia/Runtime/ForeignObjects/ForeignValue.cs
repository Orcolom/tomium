using System;
using Unity.Burst;

namespace Tomia
{
	internal static class ForeignValue<T>
		where T : unmanaged
	{
		private static readonly SharedStatic<StaticMap<T>> _values =
			SharedStatic<StaticMap<T>>.GetOrCreate<T>();

		static ForeignValue()
		{
			_values.Data.Init(64);
			ForeignMetadata.TryAddRemoveAction<T>(Remove);
		}

		public static bool ContainsKey(IntPtr ptr) => _values.Data.Map.ContainsKey(ptr);

		public static bool TryAdd(IntPtr ptr, T value)
		{
			using (ProfilerUtils.AllocScope.Auto())
			{
				return _values.Data.Map.TryAdd(ptr, value);
			}
		}

		public static bool TryGetValue(IntPtr ptr, out T value) => _values.Data.Map.TryGetValue(ptr, out value);

		public static void Remove(IntPtr ptr)
		{
			_values.Data.Map.Remove(ptr);
		}

		public static void Set(IntPtr ptr, T value)
		{
			using (ProfilerUtils.AllocScope.Auto())
			{
				_values.Data.Map[ptr] = value;
			}
		}
	}
}
