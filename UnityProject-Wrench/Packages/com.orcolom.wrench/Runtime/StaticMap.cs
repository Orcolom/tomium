using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Tomia
{
	public struct StaticMap<T> : IDisposable
		where T : struct
	{
		public UnsafeHashMap<IntPtr, T> Map;

		public void Init(int alloc)
		{
			Map = new UnsafeHashMap<IntPtr, T>(alloc, AllocatorManager.Persistent);
		}

		public void Dispose()
		{
			Map.Dispose();
		}
	}
}
