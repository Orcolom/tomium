using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Wrench
{
	public struct StaticMap<T>
		where T : struct
	{
		public UnsafeHashMap<IntPtr, T> Map;

		public void Init(int alloc)
		{
			Map = new UnsafeHashMap<IntPtr, T>(alloc, AllocatorManager.Persistent);
		}
	}
}
