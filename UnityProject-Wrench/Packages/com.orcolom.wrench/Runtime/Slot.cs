using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Wrench.Native;

namespace Wrench
{
	public readonly struct Slot : IVmElement, IEquatable<Slot>
	{
		private readonly int _index;
		
		[NativeDisableUnsafePtrRestriction]
		private readonly IntPtr _vmPtr;

		IntPtr IVmElement.VmPtr => _vmPtr;
		internal int Index => _index;

		internal Slot(IntPtr vmPtr, int index)
		{
			_vmPtr = vmPtr;
			_index = index;
		}

		public ValueType Type
		{
			get
			{
				// return ValueType.Unknown;
				if (IfInvalid(this)) return ValueType.Unknown;
				return Interop.wrenGetSlotType(_vmPtr, _index);
			}
		}

		#region bool

		/// <summary>
		/// Reads a boolean value
		/// It is an error to call this if the slot does not contain a boolean value.
		/// </summary>
		public bool GetBool()
		{
			if (IfInvalidType(this, ValueType.Bool)) return false;
			return Interop.wrenGetSlotBool(_vmPtr, _index);
		}

		/// <summary>
		/// Stores the boolean <paramref name="value"/>
		/// </summary>
		public void SetBool(bool value)
		{
			if (IfInvalid(this)) return;
			Interop.wrenSetSlotBool(_vmPtr, _index, value);
		}

		#endregion

		/// <summary>
		/// Stores null
		/// </summary>
		public void SetNull()
		{
			if (IfInvalid(this)) return;
			Interop.wrenSetSlotNull(_vmPtr, _index);
		}

		/// <summary>
		/// Looks up the top level variable with <paramref name="variable"/> in resolved <paramref name="module"/> and store it
		/// </summary>
		public void GetVariable(string module, string variable)
		{
			if (IfInvalid(this)) return;
			Vm vm = Vm.FromPtr(_vmPtr);
			if (vm.HasModuleAndVariable(module, variable) == false) return;
			Interop.wrenGetVariable(_vmPtr, module, variable, _index);
		}

		#region String

		/// <summary>
		/// Reads a byte array.
		///
		/// It is an error to call this if the slot does not contain a string.
		/// </summary>
		public byte[] GetBytes()
		{
			if (IfInvalidType(this, ValueType.String)) return null;

			IntPtr arrayPtr = Interop.wrenGetSlotBytes(_vmPtr, _index, out int length);
			byte[] managedArray = new byte[length];
			Marshal.Copy(arrayPtr, managedArray, 0, length);
			return managedArray;
		}

		/// <summary>
		/// Stores the array of <paramref name="bytes"/>
		/// </summary>
		public void SetBytes(byte[] bytes)
		{
			if (IfInvalid(this)) return;

			IntPtr arrayPtr = Marshal.AllocHGlobal(bytes.Length);
			Marshal.Copy(bytes, 0, arrayPtr, bytes.Length);
			Interop.wrenSetSlotBytes(_vmPtr, _index, arrayPtr, new UIntPtr((uint) bytes.Length));
			Marshal.FreeHGlobal(arrayPtr);
		}

		/// <summary>
		/// Reads a string
		///
		/// It is an error to call this if the slot does not contain a string.
		/// </summary>
		public string GetString()
		{
			if (IfInvalidType(this, ValueType.String)) return null;

			IntPtr intPtr = Interop.wrenGetSlotString(_vmPtr, _index);
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
		public void SetString(string value)
		{
			if (IfInvalid(this)) return;

			Interop.wrenSetSlotString(_vmPtr, _index, value);
		}

		#endregion

		#region Double

		/// <inheritdoc cref="GetDouble"/>
		public float GetFloat()
		{
			return (float) GetDouble();
		}

		/// <inheritdoc cref="SetDouble"/>
		public void SetFloat(float value)
		{
			SetDouble(value);
		}

		/// <inheritdoc cref="GetDouble"/>
		public int GetInt()
		{
			return (int) Math.Round(GetDouble());
		}

		/// <inheritdoc cref="SetDouble"/>
		public void SetInt(int value)
		{
			SetDouble(value);
		}

		///  <summary>
		/// Reads a number
		///
		/// It is an error to call this if the slot does not contain a number.
		/// </summary>
		public double GetDouble()
		{
			if (IfInvalidType(this, ValueType.Number)) return 0;
			return Interop.wrenGetSlotDouble(_vmPtr, _index);
		}

		/// <summary>
		/// Stores the numeric <paramref name="value"/>
		/// </summary>
		public void SetDouble(double value)
		{
			if (IfInvalid(this)) return;

			Interop.wrenSetSlotDouble(_vmPtr, _index, value);
		}

		#endregion

		#region Handle

		/// <summary>
		/// Creates a handle for the value stored
		///
		/// This will prevent the object that is referred to from being garbage collected
		/// until the handle is released by calling <see cref="Handle.Dispose"/>.
		/// </summary>
		public Handle GetHandle()
		{
			if (IfInvalid(this)) return new Handle();

			var handlePtr = Interop.wrenGetSlotHandle(_vmPtr, _index);
			var handle = new Handle(_vmPtr, handlePtr);
			return handle;
		}

		/// <summary>
		/// Stores the value captured in <paramref name="handle"/>
		///
		/// This does not release the handle!
		/// </summary>
		public void SetHandle(Handle handle)
		{
			if (IfInvalid(this)) return;
			if (Handle.IfInvalid(handle)) return;
			if (Vm.IfNotSameVm(this, handle)) return;

			Interop.wrenSetSlotHandle(_vmPtr, _index, handle.Ptr);
		}

		#endregion

		public int Count
		{
			get
			{
				if (IfInvalidType(this, ValueType.List, ValueType.Map)) return 0;

				var type = Interop.wrenGetSlotType(_vmPtr, _index);
				return type switch
				{
					ValueType.Map => Interop.wrenGetMapCount(_vmPtr, _index),
					ValueType.List => Interop.wrenGetListCount(_vmPtr, _index),
					_ => 0,
				};
			}
		}

		#region List

		/// <summary>
		/// Stores a new empty list
		/// </summary>
		public void SetNewList()
		{
			if (IfInvalid(this)) return;
			Interop.wrenSetSlotNewList(_vmPtr, _index);
		}

		/// <summary>
		/// Sets the value stored at <paramref name="index"/> in the list
		/// to the value from <paramref name="element"/>. 
		/// </summary>
		public void SetListElement(int index, in Slot element)
		{
			if (IfInvalidType(this, ValueType.List)) return;
			if (IfInvalid(element)) return;
			if (Vm.IfNotSameVm(this, element)) return;

			Interop.wrenSetListElement(_vmPtr, _index, index, element._index);
		}

		/// <summary>
		/// Reads element <paramref name="index"/> from the list and stores it in <paramref name="element"/>.
		/// </summary>
		public void GetListElement(int index, in Slot element)
		{
			if (IfInvalidType(this, ValueType.List)) return;
			if (IfInvalid(element)) return;
			if (Vm.IfNotSameVm(this, element)) return;

			Interop.wrenGetListElement(_vmPtr, _index, index, element._index);
		}

		/// <summary>
		/// Takes the value stored at <paramref name="element"/> and inserts it into the list at <paramref name="index"/>.
		///
		/// <para>
		/// 	As in Wren, negative indexes can be used to insert from the end. To append an element, use `-1` for the index.
		/// </para>
		/// </summary>
		public void InsertInList(int index, in Slot element)
		{
			if (IfInvalidType(this, ValueType.List)) return;
			if (IfInvalid(element)) return;
			if (Vm.IfNotSameVm(this, element)) return;

			Interop.wrenInsertInList(_vmPtr, _index, index, element._index);
		}

		public void AddToList(in Slot element)
		{
			InsertInList(-1, element);
		}

		#endregion

		#region Map

		/// <summary>
		/// Stores a new empty map
		/// </summary>
		public void SetNewMap()
		{
			if (IfInvalid(this)) return;
			Interop.wrenSetSlotNewMap(_vmPtr, _index);
		}

		/// <summary>
		/// Returns true if the key in <paramref name="key"/> is found in the map
		/// </summary>
		public bool MapContainsKey(int mapSlot, in Slot key)
		{
			if (IfInvalidType(this, ValueType.Map)) return false;
			if (IfInvalid(key)) return false;
			if (Vm.IfNotSameVm(this, key)) return false;

			return Interop.wrenGetMapContainsKey(_vmPtr, mapSlot, key._index);
		}

		/// <summary>
		/// Retrieves a value with the key in <paramref name="key"/> from the map
		/// stores it in <paramref name="value"/>.
		/// </summary>
		public void GetMapValue(in Slot key, in Slot value)
		{
			if (IfInvalidType(this, ValueType.Map)) return;
			if (IfInvalid(key)) return;
			if (Vm.IfNotSameVm(this, key, value)) return;

			Interop.wrenGetMapValue(_vmPtr, _index, key._index, value._index);
		}

		/// <summary>
		/// Takes the value stored at <paramref name="value"/> and inserts it into the map with key <paramref name="key"/>.
		/// </summary>
		public void SetMapValue(in Slot key, in Slot value)
		{
			if (IfInvalidType(this, ValueType.Map)) return;
			if (IfInvalid(key)) return;
			if (Vm.IfNotSameVm(this, key, value)) return;

			Interop.wrenSetMapValue(_vmPtr, _index, key._index, value._index);
		}

		/// <summary>
		/// Removes a value from the map, with the key from <paramref name="key"/>,
		/// and place it in <paramref name="removedValue"/>. If not found, <paramref name="removedValue"/> is
		/// set to null, the same behaviour as the Wren Map API.
		/// </summary>
		public void RemoveMapValue(in Slot key, in Slot removedValue)
		{
			if (IfInvalidType(this, ValueType.Map)) return;
			if (IfInvalid(key)) return;
			if (Vm.IfNotSameVm(this, key, removedValue)) return;

			Interop.wrenRemoveMapValue(_vmPtr, _index, key._index, removedValue._index);
		}

		#endregion

		#region Foreign Object

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
		public ForeignObject<T> SetNewForeign<T>(in Slot @class, T data = default)
			where T : unmanaged
		{
			if (IfInvalidType(this, ValueType.Foreign)) return new ForeignObject<T>();
			return ForeignObject.New(_vmPtr, this, @class, data);
		}

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
		public ManagedForeignObject<T> SetNewManagedForeign<T>(in Slot @class, T data = default)
			where T : unmanaged
		{
			if (IfInvalidType(this, ValueType.Foreign)) return new ManagedForeignObject<T>();
			return ForeignObject.NewManaged(_vmPtr, this, @class, data);
		}

		public ForeignObject<T> GetForeign<T>()
			where T : unmanaged
		{
			if (IfInvalidType(this, ValueType.Foreign)) return new ForeignObject<T>();
			var ptr = Interop.wrenGetSlotForeign(_vmPtr, _index);
			return ForeignObject<T>.FromPtr(ptr);
		}

		public ManagedForeignObject<T> GetManagedForeign<T>()
			where T : unmanaged
		{
			if (IfInvalidType(this, ValueType.Foreign)) return new ManagedForeignObject<T>();
			var ptr = Interop.wrenGetSlotForeign(_vmPtr, _index);
			return ManagedForeignObject<T>.FromPtr(ptr);
		}

		/// <summary>
		/// Reads a foreign object
		///
		/// It is an error to call this if the slot does not contain an instance of a foreign class.
		/// </summary>
		public ForeignObject GetForeign()
		{
			if (IfInvalidType(this, ValueType.Foreign)) return new ForeignObject();
			var ptr = Interop.wrenGetSlotForeign(_vmPtr, _index);
			return new ForeignObject(ptr);
		}

		#endregion

		#region Validate

		private static bool IfInvalid(in Slot slot)
		{
			if (Vm.IfInvalid(slot._vmPtr)) return true;

			int count = Interop.wrenGetSlotCount(slot._vmPtr);
			if (slot._index < count) return false;

			Expect.ThrowException(new ArgumentOutOfRangeException(nameof(slot),
				$"Slot {slot._index} outside of ensured size {count}"));
			return true;
		}

		private static bool IfInvalidType(in Slot slot, ValueType typeA, ValueType? typeB = null)
		{
			if (IfInvalid(slot)) return true;

			var actualType = Interop.wrenGetSlotType(slot._vmPtr, slot._index);
			if (actualType == typeA || actualType == typeB) return false;
			
			Expect.ThrowException(
				new TypeAccessException($"slot {slot._index} is of type {actualType} not of types [{typeA}, {typeB}]"));
			return true;
		}

		#endregion

		#region Equality

		public bool Equals(Slot other)
		{
			return _index == other._index && _vmPtr.Equals(other._vmPtr);
		}

		public override bool Equals(object obj)
		{
			return obj is Slot other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_index, _vmPtr);
		}

		public static bool operator ==(Slot left, Slot right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Slot left, Slot right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}
