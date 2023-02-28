using System;
using Unity.Burst;

namespace Tomia
{
	internal static class ForeignValue<T>
		where T : unmanaged
	{
		public static readonly SharedStatic<StaticMap<T>> StaticMap =
			SharedStatic<StaticMap<T>>.GetOrCreate<T>();

		static ForeignValue()
		{
			StaticMap.Data.Init(64);
			ForeignMetadata.TypeIdToRemove.TryAdd(ForeignStruct<T>.TypeID, Remove);
		}

		private static void Remove(IntPtr ptr)
		{
			StaticMap.Data.Map.Remove(ptr);
		}
	}
}
