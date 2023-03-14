using System;
using Tomia.Native;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace Tomia
{
	public struct Handle : IDisposable, IEquatable<Handle>
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
		internal IntPtr VmPtr => _vmPtr;

		internal Handle(IntPtr vmPtr, IntPtr ptr)
		{
			_ptr = ptr;
			_vmPtr = vmPtr;

			using (ProfilerUtils.AllocScope.Auto())
			{
				ProfilerUtils.Log($"Create {this}");
				Handles.Data.TryAdd(_ptr, this);
			}
		}

		internal static Handle New(Vm vm, string signature)
		{
			if (vm.ExpectedValid()) return new Handle();

			IntPtr handlePtr = Interop.wrenMakeCallHandle(vm.Ptr, signature);
			var handle = new Handle(vm.Ptr, handlePtr);

			using (ProfilerUtils.AllocScope.Auto())
			{
				Handles.Data.TryAdd(handlePtr, handle);
				Managed.ManagedClasses[vm.Ptr].CallHandles.Add(handlePtr);
			}

			return handle;
		}

		internal static Handle FromPtr(IntPtr ptr)
		{
			return Handles.Data.TryGetValue(ptr, out var handle) ? handle : new Handle();
		}

		public void Dispose()
		{
			if (IsValid == false) return;
			
			ProfilerUtils.Log($"Dispose {this}");
			Interop.wrenReleaseHandle(_vmPtr, _ptr);
			Handles.Data.Remove(_ptr);
			Managed.ManagedClasses[_vmPtr].CallHandles.Remove(_ptr);
			_ptr = IntPtr.Zero;
		}

		// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
		internal static bool IfInvalid(in Handle handle)
		{
			if (handle.IsValid) return false;
			
			throw new ObjectDisposedException(handle.ToString());
			// return true;
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

		public override string ToString()
		{
			return $"{nameof(Handle)}({_ptr})";
		}
	}
}
