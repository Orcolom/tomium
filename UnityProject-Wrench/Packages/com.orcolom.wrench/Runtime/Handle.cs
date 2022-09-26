﻿using System;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Wrench.Native;

namespace Wrench
{
	public struct Handle : IDisposable, IVmElement, IEquatable<Handle>
	{
		internal static readonly SharedStatic<StaticMap<Handle>> Handles = SharedStatic<StaticMap<Handle>>.GetOrCreate<Handle>();
		static Handle()
		{
			Handles.Data.Init(16);
		}

		[NativeDisableUnsafePtrRestriction]
		private IntPtr _ptr;
		[NativeDisableUnsafePtrRestriction]
		private readonly IntPtr _vmPtr;

		public bool IsValid => FromPtr(_ptr)._ptr != IntPtr.Zero;

		internal IntPtr Ptr => _ptr;
		IntPtr IVmElement.VmPtr => _vmPtr;

		internal Handle(IntPtr vmPtr, IntPtr ptr)
		{
			_ptr = ptr;
			_vmPtr = vmPtr;
			Handles.Data.Map.Add(_ptr, this);
		}

		public static Handle New(in Vm vm, string signature)
		{
			if (Vm.IfInvalid(vm)) return new Handle();

			IntPtr handlePtr = Interop.wrenMakeCallHandle(vm.Ptr, signature);
			var handle = new Handle(vm.Ptr, handlePtr);
			Handles.Data.Map.Add(handlePtr, handle);
			return handle;
		}

		internal static Handle FromPtr(IntPtr ptr)
		{
			return Handles.Data.Map.TryGetValue(ptr, out var handle) ? handle : new Handle();
		}

		public void Dispose()
		{
			if (IsValid == false) return;
			Interop.wrenReleaseHandle(_vmPtr, _ptr);
			Handles.Data.Map.Remove(_ptr);
			_ptr = IntPtr.Zero;
		}

		// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
		internal static bool IfInvalid(in Handle handle)
		{
			if (handle.IsValid) return false;
			
			Expect.ThrowException(new ObjectDisposedException("Handle is already disposed"));
			return true;
		}

		#region Equality

		public bool Equals(Handle other)
		{
			return _ptr.Equals(other._ptr) && _vmPtr.Equals(other._vmPtr);
		}

		public override bool Equals(object obj)
		{
			return obj is Handle other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_ptr, _vmPtr);
		}

		public static bool operator ==(Handle left, Handle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Handle left, Handle right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}
