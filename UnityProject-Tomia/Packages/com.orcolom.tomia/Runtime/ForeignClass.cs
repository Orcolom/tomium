using System;
using System.Reflection;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using UnityEngine;

namespace Tomia
{
	internal static class ForeignClassStatics
	{
		static ForeignClassStatics()
		{
			// needs to happen here, il2pcc will strip the static constructor from ForeignClass. (guess: cause it doesn't have any statics itself?)
			// these statics are separate because mono.cecil weaver doesn't like to weave classes that have unity collections objects as statics 
			Classes.Data.Init(16);
			FinToAlloc.Data.Init(16);
		}
		
		internal static readonly SharedStatic<StaticMap<ForeignClass>> Classes =
			SharedStatic<StaticMap<ForeignClass>>.GetOrCreate<ForeignClass>();

		internal static readonly SharedStatic<StaticMap<IntPtr>> FinToAlloc =
			SharedStatic<StaticMap<IntPtr>>.GetOrCreate<ForeignClass, IntPtr>();
	}

	public struct ForeignClass
	{
		private IntPtr _allocPtr;
		private IntPtr _finPtr;

		internal IntPtr AllocPtr => _allocPtr;

		public bool IsValid => _allocPtr != IntPtr.Zero || _finPtr != IntPtr.Zero;

		#region Lifetime

		public ForeignClass(ForeignAction alloc)
		{
#if UNITY_EDITOR || DEBUG
			// TO//DO: only do this if il2cpp backend is selected
			// if (alloc.Method.IsStatic == false) Debug.LogError("Alloc methods have to be static for il2cpp");
#endif
			
			ProfilerUtils.Log($"Create({nameof(ForeignClass)}) {alloc.Method.Name}({alloc.Method.MethodHandle.Value}), no fin");

			_allocPtr = alloc.Method.MethodHandle.Value;
			_finPtr = IntPtr.Zero;
			using (ProfilerUtils.AllocScope.Auto())
			{
				Managed.Actions.TryAdd(_allocPtr, alloc);
				ForeignClassStatics.Classes.Data.TryAdd(_allocPtr, this);
			}
		}

		public ForeignClass(ForeignAction alloc, ForeignAction fin)
		{
			ProfilerUtils.Log($"Create({nameof(ForeignClass)}) {alloc.Method.Name}({alloc.Method.MethodHandle.Value}), {fin.Method.Name}({fin.Method.MethodHandle.Value})");
			
			_allocPtr = alloc.Method.MethodHandle.Value;
			_finPtr = fin.Method.MethodHandle.Value;

			using (ProfilerUtils.AllocScope.Auto())
			{
				Managed.Actions.TryAdd(_allocPtr, alloc);
				ForeignClassStatics.Classes.Data.TryAdd(_allocPtr, this);
				ForeignClassStatics.FinToAlloc.Data.TryAdd(_allocPtr, _finPtr);
			}
		}

		// public static ForeignClass DefaultUnmanagedAlloc<T>()
		// 	where T: unmanaged
		// {
		// 	return new ForeignClass(vm => vm.Slot0.SetNewForeign<T>(vm.Slot0));
		// }

		public static ForeignClass DefaultObjectAlloc<T>()
		{
			return new ForeignClass(DefaultObjectAllocAction<T>);
		}

		private static void DefaultObjectAllocAction<T>(Vm vm)
		{
			vm.Slot0.SetNewForeignObject<T>(vm.Slot0);
		}
		
		public static ForeignClass DefaultStructAlloc<T>()
			where T: unmanaged
		{
			return new ForeignClass(DefaultStructAllocAction<T>);
		}

		private static void DefaultStructAllocAction<T>(Vm vm)
			where T: unmanaged
		{
			vm.Slot0.SetNewForeignStruct<T>(vm.Slot0);
		}
		
		internal static ForeignClass FromAllocPtr(IntPtr ptr)
		{
			return ForeignClassStatics.Classes.Data.TryGetValue(ptr, out var @class) ? @class : new ForeignClass();
		}

		internal static ForeignClass FromFinPtr(IntPtr ptr)
		{
			return ForeignClassStatics.FinToAlloc.Data.TryGetValue(ptr, out var alloc) ? FromAllocPtr(alloc) : new ForeignClass();
		}

		#endregion

		public void InvokeAllocator(in Vm vm)
		{
			try
			{
				var action = Managed.Actions[_allocPtr];
				action.Invoke(vm);
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}
		}

		public void InvokeFinalizer(IntPtr intPtr)
		{
			// remove if managed
			Managed.ForeignObjects.Remove(intPtr);
			
			// remove if struct
			if (ForeignMetadata.TryGetValue(intPtr, out var metaData) 
				&& metaData.Style == ForeignStyle.Struct
				&& ForeignMetadata.TryGetRemoveAction(metaData.TypeID, out var remove))
			{
				remove?.Invoke(intPtr);
			}

			// remove the type link
			ForeignMetadata.Remove(intPtr);
		}
	}
}
