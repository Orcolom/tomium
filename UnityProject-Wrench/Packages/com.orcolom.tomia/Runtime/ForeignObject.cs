using System;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Tomia.Native;
using Object = UnityEngine.Object;

namespace Tomia
{
	// public static class ForeignObjectUtils
	// {
	// 	public static bool IsValueOfUnmanagedType<T>(IntPtr ptr)
	// 		where T : unmanaged
	// 	{
	// 		return UnmanagedForeignObject<T>.ForeignObjects.Data.Map.ContainsKey(ptr);
	// 	}
	//
	// 	public static bool IsValueOfManaged<T>(IntPtr ptr)
	// 	{
	// 		return Managed.ForeignObjects.ContainsKey(ptr);
	// 	}
	// }
	//
	// public readonly struct UnmanagedForeignObject<T>
	// 	where T : unmanaged
	// {
	// 	internal static readonly SharedStatic<StaticMap<T>> ForeignObjects = SharedStatic<StaticMap<T>>.GetOrCreate<UnmanagedForeignObject<T>>();
	// 	
	// 	static UnmanagedForeignObject()
	// 	{
	// 		ForeignObjects.Data.Init(16);
	// 	}
	// 	
	// 	[NativeDisableUnsafePtrRestriction]
	// 	private readonly IntPtr _ptr;
	//
	// 	public bool IsValid => ForeignObjects.Data.Map.ContainsKey(_ptr);
	//
	// 	public T Value
	// 	{
	// 		get
	// 		{
	// 			if (ExpectedValid(this)) return default;
	// 			return ForeignObjects.Data.Map.TryGetValue(_ptr, out var value) ? value : default;
	// 		}
	// 		set
	// 		{
	// 			if (ExpectedValid(this)) return;
	// 			ForeignObjects.Data.Map[_ptr] = value;
	// 		}
	// 	}
	//
	// 	internal UnmanagedForeignObject(IntPtr ptr)
	// 	{
	// 		_ptr = ptr;
	// 	}
	//
	// 	public static UnmanagedForeignObject<T> FromPtr(IntPtr ptr)
	// 	{
	// 		var foreignObject = new UnmanagedForeignObject<T>(ptr);
	// 		ExpectedValid(foreignObject);
	// 		return foreignObject;
	// 	}
	//
	// 	internal static bool ExpectedValid(in UnmanagedForeignObject<T> unmanagedForeignObject)
	// 	{
	// 		if (unmanagedForeignObject.IsValid) return false;
	//
	// 		ProfilerUtils.ThrowException(new ObjectDisposedException("ForeignObject is already disposed"));
	// 		return true;
	// 	}
	// }

	public readonly struct ForeignObject<T>
	{
		private readonly IntPtr _ptr;

		public bool IsValid => Managed.ForeignObjects.ContainsKey(_ptr);

		public T Value
		{
			get
			{
				if (ExpectedValid(this)) return default;
				return Managed.ForeignObjects.TryGetValue(_ptr, out var value) ? (T) value : default;
			}
			set
			{
				if (ExpectedValid(this)) return;
				using (ProfilerUtils.AllocScope.Auto())
				{
					Managed.ForeignObjects[_ptr] = value;
				}
			}
		}

		internal ForeignObject(IntPtr ptr)
		{
			_ptr = ptr;
		}

		public static ForeignObject<T> FromPtr(IntPtr ptr)
		{
			var foreignObject = new ForeignObject<T>(ptr);
			ExpectedValid(foreignObject);
			return foreignObject;
		}
		
		public ForeignObject<TType> As<TType>()
		{
			return new ForeignObject<TType>(_ptr);
		}

		internal static bool ExpectedValid(in ForeignObject<T> foreignObject)
		{
			if (foreignObject.IsValid) return false;
			
			throw new ObjectDisposedException("ForeignObject is already disposed");
			// return true;
		}
	}
}
