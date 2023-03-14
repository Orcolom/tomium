using System;
using Unity.Burst;

namespace Tomium
{
	internal static class ForeignValue<T>
		where T : unmanaged
	{
		private static readonly SharedStatic<StaticMap<T>> _values =
			SharedStatic<StaticMap<T>>.GetOrCreate<T>();

		static ForeignValue()
		{
			_values.Data.Init(64);
			ForeignMetadata.TryAddType<T>(Remove);
		}

		public static bool ContainsKey(IntPtr ptr) => _values.Data.ContainsKey(ptr);

		public static bool TryAdd(IntPtr ptr, T value)
		{
			return _values.Data.TryAdd(ptr, value);
		}

		public static bool TryGetValue(IntPtr ptr, out T value) => _values.Data.TryGetValue(ptr, out value);

		public static void Remove(IntPtr ptr)
		{
			_values.Data.Remove(ptr);
		}

		public static void Set(IntPtr ptr, T value)
		{
			_values.Data.Set(ptr, value);
		}
	}
}
