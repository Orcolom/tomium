using System;
using Unity.Burst;
using Wrench.Native;

namespace Wrench
{
	public interface IForeignObject
	{
		public bool IsValueOfUnManagedType<T>() where T : unmanaged;
		UnManagedForeignObject<T> AsUnManaged<T>() where T : unmanaged;
	}
	
	public struct ForeignObject : IForeignObject
	{
		private readonly IntPtr _ptr;

		public ForeignObject(IntPtr ptr)
		{
			_ptr = ptr;
		}

		public bool IsValueOfUnManagedType<T>()
			where T : unmanaged
		{
			return UnManagedForeignObject<T>.ForeignObjects.Data.Map.ContainsKey(_ptr);
		}

		public bool IsValueOfManagedType<T>()
		{
			return Managed.ForeignObjects.ContainsKey(_ptr);
		}

		public UnManagedForeignObject<T> AsUnManaged<T>()
			where T : unmanaged
		{
			return UnManagedForeignObject<T>.FromPtr(_ptr);
		}

		public ManagedForeignObject<T> AsManaged<T>()
		{
			return ManagedForeignObject<T>.FromPtr(_ptr);
		}

		public static UnManagedForeignObject<T> NewUnManaged<T>(IntPtr vmPtr, in Slot storeSlot, in Slot classSlot, T value = default)
			where T : unmanaged
		{
			if (Vm.IfInvalid(vmPtr)) return new UnManagedForeignObject<T>();

			var ptr = Interop.wrenSetSlotNewForeign(vmPtr, storeSlot.Index, classSlot.Index, new IntPtr(IntPtr.Size));

			var obj = new UnManagedForeignObject<T>(ptr);
			UnManagedForeignObject<T>.ForeignObjects.Data.Map.TryAdd(ptr, value);

			return obj;
		}
		
		public static ManagedForeignObject<T> NewManaged<T>(IntPtr vmPtr, in Slot storeSlot, in Slot classSlot, T value = default)
		{
			if (Vm.IfInvalid(vmPtr)) return new ManagedForeignObject<T>();

			var ptr = Interop.wrenSetSlotNewForeign(vmPtr, storeSlot.Index, classSlot.Index, new IntPtr(IntPtr.Size));

			return new ManagedForeignObject<T>(ptr);
		}
	}

	public readonly struct UnManagedForeignObject<T> : IForeignObject
		where T : unmanaged
	{
		internal static readonly SharedStatic<StaticMap<T>> ForeignObjects = SharedStatic<StaticMap<T>>.GetOrCreate<UnManagedForeignObject<T>>();
		
		static UnManagedForeignObject()
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

		public bool IsValueOfUnManagedType<TOther>()
			where TOther : unmanaged
		{
			return ForeignObjects.Data.Map.ContainsKey(_ptr);
		}
		
		public UnManagedForeignObject<TOther> AsUnManaged<TOther>()
			where TOther : unmanaged
		{
			return UnManagedForeignObject<TOther>.FromPtr(_ptr);
		}
		
		internal UnManagedForeignObject(IntPtr ptr)
		{
			_ptr = ptr;
		}

		public static UnManagedForeignObject<T> FromPtr(IntPtr ptr)
		{
			var foreignObject = new UnManagedForeignObject<T>(ptr);
			IfInvalid(foreignObject);
			return foreignObject;
		}

		internal static bool IfInvalid(in UnManagedForeignObject<T> unManagedForeignObject)
		{
			if (unManagedForeignObject.IsValid) return false;

			Expected.ThrowException(new ObjectDisposedException("ForeignObject is already disposed"));
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
			
			Expected.ThrowException(new ObjectDisposedException("ForeignObject is already disposed"));
			return true;
		}
	}
}
