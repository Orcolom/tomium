using System;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Wrench
{
	public struct ForeignClass
	{
		internal static readonly SharedStatic<StaticMap<ForeignClass>> Classes = SharedStatic<StaticMap<ForeignClass>>.GetOrCreate<ForeignClass>();
		internal static readonly SharedStatic<StaticMap<IntPtr>> FinToAlloc = SharedStatic<StaticMap<IntPtr>>.GetOrCreate<ForeignClass, IntPtr>();
		static ForeignClass()
		{
			Classes.Data.Init(16);
			FinToAlloc.Data.Init(16);
		}
		
		private IntPtr _allocPtr;
		private IntPtr _finPtr;

		internal IntPtr AllocPtr => _allocPtr;
		internal IntPtr FinPtr => _allocPtr;

		public bool IsValid => _allocPtr != IntPtr.Zero || _finPtr != IntPtr.Zero;

		#region Lifetime

		public ForeignClass(ForeignAction alloc)
		{
			_allocPtr = Marshal.GetFunctionPointerForDelegate(alloc);
			_finPtr = IntPtr.Zero;
			Managed.Actions.TryAdd(_allocPtr, alloc);
			Classes.Data.Map.TryAdd(_allocPtr, this);
		}
		
		public ForeignClass(ForeignAction alloc, ForeignAction fin)
		{
			_allocPtr = Marshal.GetFunctionPointerForDelegate(alloc);
			_finPtr = Marshal.GetFunctionPointerForDelegate(fin);
			Managed.Actions.TryAdd(_allocPtr, alloc);
			Classes.Data.Map.TryAdd(_allocPtr, this);
			FinToAlloc.Data.Map.TryAdd(_allocPtr, _finPtr);
		}

		public static ForeignClass DefaultAlloc<T>()
			where T: unmanaged
		{
			return new ForeignClass((in Vm vm) => vm.Slot0.SetNewForeign<T>(vm.Slot0));
		}

		public static ForeignClass DefaultManagedAlloc<T>()
		{
			return new ForeignClass((in Vm vm) => vm.Slot0.SetNewManagedForeign<T>(vm.Slot0));
		}

		
		internal static ForeignClass FromAllocPtr(IntPtr ptr)
		{
			return Classes.Data.Map.TryGetValue(ptr, out var @class) ? @class : new ForeignClass();
		}

		internal static ForeignClass FromFinPtr(IntPtr ptr)
		{
			return FinToAlloc.Data.Map.TryGetValue(ptr, out var alloc) ? FromAllocPtr(alloc) : new ForeignClass();
		}

		#endregion

		public void InvokeAllocator(in Vm vm)
		{
			try
			{
				var action = Marshal.GetDelegateForFunctionPointer<ForeignAction>(_allocPtr);
				action.Invoke(vm);
			}
			catch
			{
				// ignored
			}
		}

		public void InvokeFinalizer()
		{
			throw new NotImplementedException();
		}
	}
}
