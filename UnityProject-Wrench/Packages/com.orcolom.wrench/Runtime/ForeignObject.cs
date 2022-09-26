using System;
using Unity.Burst;
using Wrench.Native;

namespace Wrench
{
	public interface IForeignObject
	{
		public bool IsValueOfType<T>() where T : unmanaged;
		ForeignObject<T> As<T>() where T : unmanaged;
	}
	
	public struct ForeignObject : IForeignObject
	{
		private readonly IntPtr _ptr;

		public ForeignObject(IntPtr ptr)
		{
			_ptr = ptr;
		}

		public bool IsValueOfType<T>()
			where T : unmanaged
		{
			return ForeignObject<T>.ForeignObjects.Data.Map.ContainsKey(_ptr);
		}

		public bool IsValueOfManagedType<T>()
		{
			return Managed.ForeignObjects.ContainsKey(_ptr);
		}

		public ForeignObject<T> As<T>()
			where T : unmanaged
		{
			return ForeignObject<T>.FromPtr(_ptr);
		}

		public ManagedForeignObject<T> AsManaged<T>()
		{
			return ManagedForeignObject<T>.FromPtr(_ptr);
		}

		public static ForeignObject<T> New<T>(IntPtr vmPtr, in Slot storeSlot, in Slot classSlot, T value = default)
			where T : unmanaged
		{
			if (Vm.IfInvalid(vmPtr)) return new ForeignObject<T>();

			var ptr = Interop.wrenSetSlotNewForeign(vmPtr, storeSlot.Index, classSlot.Index, new IntPtr(IntPtr.Size));

			var obj = new ForeignObject<T>(ptr);
			ForeignObject<T>.ForeignObjects.Data.Map.TryAdd(ptr, value);

			return obj;
		}
		
		public static ManagedForeignObject<T> NewManaged<T>(IntPtr vmPtr, in Slot storeSlot, in Slot classSlot, T value = default)
			where T : unmanaged
		{
			if (Vm.IfInvalid(vmPtr)) return new ManagedForeignObject<T>();

			var ptr = Interop.wrenSetSlotNewForeign(vmPtr, storeSlot.Index, classSlot.Index, new IntPtr(IntPtr.Size));

			var obj = new ManagedForeignObject<T>(ptr);
			ForeignObject<T>.ForeignObjects.Data.Map.TryAdd(ptr, value);

			return obj;
		}
	}

	public readonly struct ForeignObject<T> : IForeignObject
		where T : unmanaged
	{
		internal static readonly SharedStatic<StaticMap<T>> ForeignObjects = SharedStatic<StaticMap<T>>.GetOrCreate<ForeignObject<T>>();
		static ForeignObject()
		{
			ForeignObjects.Data.Init(16);
		}
		
		private readonly IntPtr _ptr;

		public bool IsValid => ForeignObjects.Data.Map.ContainsKey(_ptr);

		public T Value
		{
			get
			{
				if (IfInvalid(this)) return default;
				return ForeignObjects.Data.Map.TryGetValue(_ptr, out var value) ? value : default;
			}
			set
			{
				if (IfInvalid(this)) return;
				ForeignObjects.Data.Map[_ptr] = value;
			}
		}

		public bool IsValueOfType<TOther>()
			where TOther : unmanaged
		{
			return ForeignObjects.Data.Map.ContainsKey(_ptr);
		}
		
		public ForeignObject<TOther> As<TOther>()
			where TOther : unmanaged
		{
			return ForeignObject<TOther>.FromPtr(_ptr);
		}
		
		internal ForeignObject(IntPtr ptr)
		{
			_ptr = ptr;
		}

		public static ForeignObject<T> FromPtr(IntPtr ptr)
		{
			var foreignObject = new ForeignObject<T>(ptr);
			IfInvalid(foreignObject);
			return foreignObject;
		}

		internal static bool IfInvalid(in ForeignObject<T> foreignObject)
		{
			if (foreignObject.IsValid) return false;

			Expect.ThrowException(new ObjectDisposedException("ForeignObject is already disposed"));
			return true;
		}
	}

	public readonly struct ManagedForeignObject<T>
	{
		private readonly IntPtr _ptr;

		public bool IsValid => Managed.ForeignObjects.ContainsKey(_ptr);

		public T Value
		{
			get
			{
				if (IfInvalid(this)) return default;
				return Managed.ForeignObjects.TryGetValue(_ptr, out var value) ? (T) value : default;
			}
			set
			{
				if (IfInvalid(this)) return;
				Managed.ForeignObjects[_ptr] = value;
			}
		}

		internal ManagedForeignObject(IntPtr ptr)
		{
			_ptr = ptr;
		}

		public static ManagedForeignObject<T> FromPtr(IntPtr ptr)
		{
			var foreignObject = new ManagedForeignObject<T>(ptr);
			IfInvalid(foreignObject);
			return foreignObject;
		}

		internal static bool IfInvalid(in ManagedForeignObject<T> foreignObject)
		{
			if (foreignObject.IsValid) return false;
			
			Expect.ThrowException(new ObjectDisposedException("ForeignObject is already disposed"));
			return true;
		}
	}
}
