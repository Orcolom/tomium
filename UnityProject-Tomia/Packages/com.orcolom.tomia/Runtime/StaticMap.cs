using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Tomia
{
	public struct StaticMap<T> : IDisposable
		where T : unmanaged
	{
		private UnsafeHashMap<IntPtr, T> _map;
		private static SpinLock _sl;

		public void Init(int alloc)
		{
			_map = new UnsafeHashMap<IntPtr, T>(alloc, AllocatorManager.Persistent);
		}

		public bool ContainsKey(IntPtr ptr) => _map.ContainsKey(ptr);

		public bool TryAdd(IntPtr ptr, T value)
		{

			bool _hasLock = false;
			if (_sl.IsHeldByCurrentThread == false) _sl.Enter(ref _hasLock);

			bool success = false;
			using (ProfilerUtils.AllocScope.Auto())
			{
				success = _map.TryAdd(ptr, value);
			}

			if (_sl.IsHeldByCurrentThread && _hasLock) _sl.Exit();
			return success;
		}

		public bool TryGetValue(IntPtr ptr, out T value) => _map.TryGetValue(ptr, out value);

		public void Remove(IntPtr ptr)
		{
			_map.Remove(ptr);
		}

		public void Set(IntPtr ptr, T value)
		{
			_map[ptr] = value;
		}
		
		public void Dispose()
		{
			if (_sl.IsHeldByCurrentThread) _sl.Exit();
			_map.Dispose();
		}
	}
}
