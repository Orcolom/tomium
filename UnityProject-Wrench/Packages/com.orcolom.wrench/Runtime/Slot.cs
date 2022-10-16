using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Wrench.Native;

namespace Wrench
{
	public readonly struct UnmanagedSlot : ISlotUnmanaged, IEquatable<ISlotUnmanaged>
	{
		private readonly int _index;

		[NativeDisableUnsafePtrRestriction]
		private readonly IntPtr _vmPtr;

		IntPtr IVmElement.VmPtr => _vmPtr;
		int ISlotUnmanaged.Index => _index;

		internal UnmanagedSlot(IntPtr vmPtr, int index)
		{
			_vmPtr = vmPtr;
			_index = index;
		}

		public override int GetHashCode() => SlotUtils.Slot_GetHashCode(this);
		public override bool Equals(object obj) => SlotUtils.Slot_Equals(this, obj);
		
		public bool Equals(ISlotUnmanaged other) => SlotUtils.Slot_Equals(this, other);

		public static bool operator ==(UnmanagedSlot left, UnmanagedSlot right) => SlotUtils.Slot_EqualsOp(left, right);
		public static bool operator !=(UnmanagedSlot left, UnmanagedSlot right) => SlotUtils.Slot_NotEqualOp(left, right);
		
		public static bool operator ==(Slot left, UnmanagedSlot right) => SlotUtils.Slot_EqualsOp(left, right);
		public static bool operator !=(Slot left, UnmanagedSlot right) => SlotUtils.Slot_EqualsOp(left, right);

		public static bool operator ==(UnmanagedSlot left, Slot right) => SlotUtils.Slot_NotEqualOp(left, right);
		public static bool operator !=(UnmanagedSlot left, Slot right) => SlotUtils.Slot_NotEqualOp(left, right);
	}

	public readonly struct Slot : ISlotManaged, IEquatable<ISlotUnmanaged>
	{
		private readonly int _index;
		private readonly IntPtr _vmPtr;

		IntPtr IVmElement.VmPtr => _vmPtr;
		int ISlotUnmanaged.Index => _index;

		internal Slot(IntPtr vmPtr, int index)
		{
			_vmPtr = vmPtr;
			_index = index;
		}

		public override int GetHashCode() => SlotUtils.Slot_GetHashCode(this);
		public override bool Equals(object obj) => SlotUtils.Equals(this, obj);
		
		public bool Equals(ISlotUnmanaged other) => SlotUtils.Slot_Equals(this, other);
		
		public static bool operator ==(Slot left, Slot right) => SlotUtils.Slot_EqualsOp(left, right);
		public static bool operator !=(Slot left, Slot right) => SlotUtils.Slot_NotEqualOp(left, right);
		
		public static bool operator ==(ISlotUnmanaged left, Slot right) => SlotUtils.Slot_EqualsOp(left, right);
		public static bool operator !=(ISlotUnmanaged left, Slot right) => SlotUtils.Slot_NotEqualOp(left, right);
		
		public static bool operator ==(Slot left, ISlotUnmanaged right) => SlotUtils.Slot_EqualsOp(left, right);
		public static bool operator !=(Slot left, ISlotUnmanaged right) => SlotUtils.Slot_NotEqualOp(left, right);
	}
	
	public interface ISlotUnmanaged : IVmElement
	{
		internal int Index { get; }
	}

	public interface ISlotManaged : ISlotUnmanaged { }

	public static class SlotUtils
	{
		#region Expected

		public static bool ExpectedValid(this ISlotUnmanaged slot)
		{
			if (VmUtils.ExpectedValid(slot.VmPtr)) return true;

			int count = Interop.wrenGetSlotCount(slot.VmPtr);
			if (slot.Index < count) return false;

			Expected.ThrowException(new ArgumentOutOfRangeException(nameof(slot),
				$"Slot {slot.Index} outside of ensured size {count}"));

			return true;
		}

		public static bool ExpectedValid(this ISlotUnmanaged slot, ValueType typeA, ValueType? typeB = null)
		{
			if (ExpectedValid(slot)) return true;

			var actualType = Interop.wrenGetSlotType(slot.VmPtr, slot.Index);
			if (actualType == typeA || actualType == typeB) return false;

			Expected.ThrowException(
				new TypeAccessException($"slot {slot.Index} is of type {actualType} not of types [{typeA}, {typeB}]"));
			return true;
		}

		#endregion

		public static ValueType GetValueType(this ISlotUnmanaged slot)
		{
			if (ExpectedValid(slot)) return ValueType.Unknown;
			return Interop.wrenGetSlotType(slot.VmPtr, slot.Index);
		}

		/// <summary>
		/// Stores null
		/// </summary>
		public static void SetNull(this ISlotUnmanaged slot)
		{
			if (ExpectedValid(slot)) return;
			Interop.wrenSetSlotNull(slot.VmPtr, slot.Index);
		}

		/// <summary>
		/// Looks up the top level variable with <paramref name="variable"/> in resolved <paramref name="module"/> and store it
		/// </summary>
		public static void GetVariable(this ISlotUnmanaged slot, string module, string variable)
		{
			// TODO: stop referencing vm here
			if (ExpectedValid(slot)) return;
			Vm vm = VmUtils.FromPtr(slot.VmPtr);
			if (vm.HasModuleAndVariable(module, variable) == false) return;
			Interop.wrenGetVariable(slot.VmPtr, module, variable, slot.Index);
		}
		
		#region bool

		/// <summary>
		/// Reads a boolean value
		/// It is an error to call this if the slot does not contain a boolean value.
		/// </summary>
		public static bool GetBool(this ISlotUnmanaged slot)
		{
			if (ExpectedValid(slot, ValueType.Bool)) return false;
			return Interop.wrenGetSlotBool(slot.VmPtr, slot.Index);
		}

		/// <summary>
		/// Stores the boolean <paramref name="value"/>
		/// </summary>
		public static void SetBool(this ISlotUnmanaged slot, bool value)
		{
			if (ExpectedValid(slot)) return;
			Interop.wrenSetSlotBool(slot.VmPtr, slot.Index, value);
		}

		#endregion
		
		#region String

		/// <summary>
		/// Reads a byte array.
		///
		/// It is an error to call this if the slot does not contain a string.
		/// </summary>
		public static byte[] GetBytes(this ISlotUnmanaged slot)
		{
			if (ExpectedValid(slot, ValueType.String)) return null;

			IntPtr arrayPtr = Interop.wrenGetSlotBytes(slot.VmPtr, slot.Index, out int length);
			byte[] managedArray = new byte[length];
			Marshal.Copy(arrayPtr, managedArray, 0, length);
			return managedArray;
		}

		/// <summary>
		/// Stores the array of <paramref name="bytes"/>
		/// </summary>
		public static void SetBytes(this ISlotUnmanaged slot, byte[] bytes)
		{
			if (ExpectedValid(slot)) return;

			IntPtr arrayPtr = Marshal.AllocHGlobal(bytes.Length);
			Marshal.Copy(bytes, 0, arrayPtr, bytes.Length);
			Interop.wrenSetSlotBytes(slot.VmPtr, slot.Index, arrayPtr, new UIntPtr((uint) bytes.Length));
			Marshal.FreeHGlobal(arrayPtr);
		}

		/// <summary>
		/// Reads a string
		///
		/// It is an error to call this if the slot does not contain a string.
		/// </summary>
		public static string GetString(this ISlotUnmanaged slot)
		{
			if (ExpectedValid(slot, ValueType.String)) return null;

			IntPtr intPtr = Interop.wrenGetSlotString(slot.VmPtr, slot.Index);
			return Marshal.PtrToStringAnsi(intPtr);
		}

		/// <summary>
		/// Stores the string <paramref name="value"/>
		///
		/// <para>
		/// 	If the string may contain any null bytes in the middle, then you
		/// 	should use <see cref="SetBytes"/> instead.
		/// </para>
		/// </summary>
		public static void SetString(this ISlotUnmanaged slot, string value)
		{
			if (ExpectedValid(slot)) return;

			Interop.wrenSetSlotString(slot.VmPtr, slot.Index, value);
		}

		#endregion
		
		#region Double

		/// <inheritdoc cref="GetDouble"/>
		public static float GetFloat(this ISlotUnmanaged slot)
		{
			return (float) GetDouble(slot);
		}

		/// <inheritdoc cref="SetDouble"/>
		public static void SetFloat(this ISlotUnmanaged slot, float value)
		{
			SetDouble(slot, value);
		}

		/// <inheritdoc cref="GetDouble"/>
		public static int GetInt(this ISlotUnmanaged slot)
		{
			return (int) Math.Round(GetDouble(slot));
		}

		/// <inheritdoc cref="SetDouble"/>
		public static void SetInt(this ISlotUnmanaged slot, int value)
		{
			SetDouble(slot, value);
		}

		///  <summary>
		/// Reads a number
		///
		/// It is an error to call this if the slot does not contain a number.
		/// </summary>
		public static double GetDouble(this ISlotUnmanaged slot)
		{
			if (ExpectedValid(slot, ValueType.Number)) return 0;
			return Interop.wrenGetSlotDouble(slot.VmPtr, slot.Index);
		}

		/// <summary>
		/// Stores the numeric <paramref name="value"/>
		/// </summary>
		public static void SetDouble(this ISlotUnmanaged slot, double value)
		{
			if (ExpectedValid(slot)) return;

			Interop.wrenSetSlotDouble(slot.VmPtr, slot.Index, value);
		}

		#endregion
		
		#region Handle

		/// <summary>
		/// Creates a handle for the value stored
		///
		/// This will prevent the object that is referred to from being garbage collected
		/// until the handle is released by calling <see cref="Handle.Dispose"/>.
		/// </summary>
		public static Handle GetHandle(this ISlotUnmanaged slot)
		{
			if (ExpectedValid(slot)) return new Handle();

			var handlePtr = Interop.wrenGetSlotHandle(slot.VmPtr, slot.Index);
			var handle = new Handle(slot.VmPtr, handlePtr);
			return handle;
		}

		/// <summary>
		/// Stores the value captured in <paramref name="handle"/>
		///
		/// This does not release the handle!
		/// </summary>
		public static void SetHandle(this ISlotUnmanaged slot, Handle handle)
		{
			if (ExpectedValid(slot)) return;
			if (Handle.IfInvalid(handle)) return;
			if (VmUtils.ExpectedSameVm(slot.VmPtr, handle)) return;

			Interop.wrenSetSlotHandle(slot.VmPtr, slot.Index, handle.Ptr);
		}

		#endregion
		
		public static int GetCount(this ISlotUnmanaged slot)
		{
			if (ExpectedValid(slot, ValueType.List, ValueType.Map)) return 0;

			var type = Interop.wrenGetSlotType(slot.VmPtr, slot.Index);
			return type switch
			{
				ValueType.Map => Interop.wrenGetMapCount(slot.VmPtr, slot.Index),
				ValueType.List => Interop.wrenGetListCount(slot.VmPtr, slot.Index),
				_ => 0,
			};
		}

		#region List

		/// <summary>
		/// Stores a new empty list
		/// </summary>
		public static void SetNewList(this ISlotUnmanaged slot)
		{
			if (ExpectedValid(slot)) return;
			Interop.wrenSetSlotNewList(slot.VmPtr, slot.Index);
		}

		/// <summary>
		/// Sets the value stored at <paramref name="index"/> in the list
		/// to the value from <paramref name="element"/>. 
		/// </summary>
		public static void SetListElement(this ISlotUnmanaged slot, int index, in ISlotUnmanaged element)
		{
			if (ExpectedValid(slot, ValueType.List)) return;
			if (ExpectedValid(element)) return;
			if (VmUtils.ExpectedSameVm(slot.VmPtr, element)) return;

			Interop.wrenSetListElement(slot.VmPtr, slot.Index, index, element.Index);
		}

		/// <summary>
		/// Reads element <paramref name="index"/> from the list and stores it in <paramref name="element"/>.
		/// </summary>
		public static void GetListElement(this ISlotUnmanaged slot, int index, in ISlotUnmanaged element)
		{
			if (ExpectedValid(slot, ValueType.List)) return;
			if (ExpectedValid(element)) return;
			if (VmUtils.ExpectedSameVm(slot.VmPtr, element)) return;

			Interop.wrenGetListElement(slot.VmPtr, slot.Index, index, element.Index);
		}

		/// <summary>
		/// Takes the value stored at <paramref name="element"/> and inserts it into the list at <paramref name="index"/>.
		///
		/// <para>
		/// 	As in Wren, negative indexes can be used to insert from the end. To append an element, use `-1` for the index.
		/// </para>
		/// </summary>
		public static void InsertInList(this ISlotUnmanaged slot, int index, in ISlotUnmanaged element)
		{
			if (ExpectedValid(slot, ValueType.List)) return;
			if (ExpectedValid(element)) return;
			if (VmUtils.ExpectedSameVm(slot.VmPtr, element)) return;

			Interop.wrenInsertInList(slot.VmPtr, slot.Index, index, element.Index);
		}

		public static void AddToList(this ISlotUnmanaged slot, in ISlotUnmanaged element)
		{
			InsertInList(slot, -1, element);
		}

		#endregion

		#region Map

		/// <summary>
		/// Stores a new empty map
		/// </summary>
		public static void SetNewMap(this ISlotUnmanaged slot)
		{
			if (ExpectedValid(slot)) return;
			Interop.wrenSetSlotNewMap(slot.VmPtr, slot.Index);
		}

		/// <summary>
		/// Returns true if the key in <paramref name="key"/> is found in the map
		/// </summary>
		public static bool MapContainsKey(this ISlotUnmanaged slot, in ISlotUnmanaged key)
		{
			if (ExpectedValid(slot, ValueType.Map)) return false;
			if (ExpectedValid(key)) return false;
			if (slot.ExpectedSameVm(key)) return false;

			return Interop.wrenGetMapContainsKey(slot.VmPtr, slot.Index, key.Index);
		}

		/// <summary>
		/// Retrieves a value with the key in <paramref name="key"/> from the map
		/// stores it in <paramref name="value"/>.
		/// </summary>
		public static void GetMapValue(this ISlotUnmanaged slot, in ISlotUnmanaged key, in ISlotUnmanaged value)
		{
			if (ExpectedValid(slot, ValueType.Map)) return;
			if (ExpectedValid(key)) return;
			if (slot.ExpectedSameVm(key)) return;
			if (slot.ExpectedSameVm(value)) return;

			Interop.wrenGetMapValue(slot.VmPtr, slot.Index, key.Index, value.Index);
		}

		/// <summary>
		/// Takes the value stored at <paramref name="value"/> and inserts it into the map with key <paramref name="key"/>.
		/// </summary>
		public static void SetMapValue(this ISlotUnmanaged slot, in ISlotUnmanaged key, in ISlotUnmanaged value)
		{
			if (ExpectedValid(slot, ValueType.Map)) return;
			if (ExpectedValid(key)) return;
			if (slot.ExpectedSameVm(key)) return;
			if (slot.ExpectedSameVm(value)) return;

			Interop.wrenSetMapValue(slot.VmPtr, slot.Index, key.Index, value.Index);
		}

		/// <summary>
		/// Removes a value from the map, with the key from <paramref name="key"/>,
		/// and place it in <paramref name="removedValue"/>. If not found, <paramref name="removedValue"/> is
		/// set to null, the same behaviour as the Wren Map API.
		/// </summary>
		public static void RemoveMapValue(this ISlotUnmanaged slot, in ISlotUnmanaged key, in ISlotUnmanaged removedValue)
		{
			if (ExpectedValid(slot, ValueType.Map)) return;
			if (ExpectedValid(key)) return;
			if (slot.ExpectedSameVm(key)) return;
			if (slot.ExpectedSameVm(removedValue)) return;

			Interop.wrenRemoveMapValue(slot.VmPtr, slot.Index, key.Index, removedValue.Index);
		}

		#endregion
		
		#region Managed Foreign Object

		/// <summary>
		/// Creates a new instance of the foreign class stored in <paramref name="class"/>
		/// 
		/// <para>
		/// 	This does not invoke the foreign class's constructor on the new instance. If
		/// 	you need that to happen, call the constructor from Wren, which will then
		/// 	call the allocator foreign method. In there, call this to create the object
		/// 	and then the constructor will be invoked when the allocator returns.
		/// </para>
		/// 
		/// </summary>
		public static ForeignObject<T> SetNewForeign<T>(this ISlotManaged slot, in ISlotUnmanaged @class, T data = default)
		{
			if (ExpectedValid(slot, ValueType.Foreign)) return new ForeignObject<T>();
			if (VmUtils.ExpectedValid(slot.VmPtr)) return new ForeignObject<T>();

			var ptr = Interop.wrenSetSlotNewForeign(slot.VmPtr, @class.Index, @class.Index, new IntPtr(IntPtr.Size));
			
			return new ForeignObject<T>(ptr);
		}
		
		public static ForeignObject<T> GetForeign<T>(this ISlotManaged slot)
		{
			if (ExpectedValid(slot, ValueType.Foreign)) return new ForeignObject<T>();
			var ptr = Interop.wrenGetSlotForeign(slot.VmPtr, slot.Index);
			return ForeignObject<T>.FromPtr(ptr);
		}
		
		#endregion
		
		#region Unmanaged Foreign Object

		/// <summary>
		/// Creates a new instance of the foreign class stored in <paramref name="class"/>
		/// 
		/// <para>
		/// 	This does not invoke the foreign class's constructor on the new instance. If
		/// 	you need that to happen, call the constructor from Wren, which will then
		/// 	call the allocator foreign method. In there, call this to create the object
		/// 	and then the constructor will be invoked when the allocator returns.
		/// </para>
		/// 
		/// </summary>
		public static UnmanagedForeignObject<T> SetNewUnmanagedForeign<T>(this ISlotUnmanaged slot, in ISlotUnmanaged @class, T data = default)
			where T : unmanaged
		{
			if (ExpectedValid(slot, ValueType.Foreign)) return new UnmanagedForeignObject<T>();
			if (VmUtils.ExpectedValid(slot.VmPtr)) return new UnmanagedForeignObject<T>();

			var ptr = Interop.wrenSetSlotNewForeign(slot.VmPtr, @class.Index, @class.Index, new IntPtr(IntPtr.Size));

			var obj = new UnmanagedForeignObject<T>(ptr);
			UnmanagedForeignObject<T>.ForeignObjects.Data.Map.TryAdd(ptr, data);

			return obj;
		}

		public static UnmanagedForeignObject<T> GetUnmanagedForeign<T>(this ISlotUnmanaged slot)
			where T : unmanaged
		{
			if (ExpectedValid(slot, ValueType.Foreign)) return new UnmanagedForeignObject<T>();
			var ptr = Interop.wrenGetSlotForeign(slot.VmPtr, slot.Index);
			return UnmanagedForeignObject<T>.FromPtr(ptr);
		}

		#endregion

		#region Equality

		public static bool Slot_Equals(ISlotUnmanaged self, object obj) => obj switch
		{
			ISlotUnmanaged slot => Slot_Equals(self, slot),
			_ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null),
		};

		public static bool Slot_Equals(in ISlotUnmanaged self, in ISlotUnmanaged other) =>
			self.Index == other.Index && self.VmPtr.Equals(other.VmPtr);
		
		public static int Slot_GetHashCode(in ISlotUnmanaged self) => (self.Index, self.VmPtr).GetHashCode();
		public static bool Slot_EqualsOp(in ISlotUnmanaged left, in ISlotUnmanaged right) => Slot_Equals(left, right);
		public static bool Slot_NotEqualOp(in ISlotUnmanaged left, in ISlotUnmanaged right) => Slot_Equals(left, right) == false;

		#endregion
	}
}
