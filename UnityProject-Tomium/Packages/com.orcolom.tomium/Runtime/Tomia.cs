using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Tomia.Native;
using UnityEngine;

[assembly: InternalsVisibleTo("Tomia.Editor")]
[assembly: InternalsVisibleTo("Tomia.Tests.Editor")]
[assembly: InternalsVisibleTo("Tomia.Builder")]
[assembly: InternalsVisibleTo("Unity.Tomia.Builder.CodeGen")]

namespace Tomia
{
	public static class Tomia
	{
		public static readonly bool IsSupported;
		public static readonly int CurrentWrenVersion;
		public static readonly string CurrentWrenVersionSemVer;

		private static readonly int[] WrenMinVersion = {0, 4, 0};
		private static readonly int[] WrenMaxVersion = {0, 4, -1};

		static Tomia()
		{
			CurrentWrenVersion = Interop.wrenGetVersionNumber();

			int patch = CurrentWrenVersion % 1000;
			int minor = ((CurrentWrenVersion - patch) / 1000) % 1000;
			int major = ((CurrentWrenVersion - (minor * 1000) - patch) / 1000000) % 1000;
			CurrentWrenVersionSemVer = $"{major}.{minor}.{patch}";

			IsSupported =
				major >= WrenMinVersion[0] && (major <= WrenMaxVersion[0] || WrenMaxVersion[0] == -1) &&
				minor >= WrenMinVersion[1] && (minor <= WrenMaxVersion[1] || WrenMaxVersion[1] == -1) &&
				patch >= WrenMinVersion[2] && (patch <= WrenMaxVersion[2] || WrenMaxVersion[2] == -1);

			if (IsSupported == false)
			{
				string minVersion = FormatVersionNumber(WrenMinVersion);
				string maxVersion = FormatVersionNumber(WrenMaxVersion);
				throw new NotSupportedException(
					$"included wren is of version {CurrentWrenVersion} but expected version between {minVersion} and {maxVersion}");
			}

			// preload
			var _ = Managed.Actions;
			
			// by subscribing the functions to an Action we keep the function in use
			// and ensures that the functions won't get garbage collected or stripped  
			// we get a Ptr if we will have to pass it to the native side 
			WriteCallback += OnWrenWrite;
			ErrorCallback += OnWrenError;
			ResolveCallback += OnWrenResolveModule;
			LoadCallback += OnWrenLoadModule;
			
			LoadCompleteCallback += OnWrenLoadComplete;
			LoadCompleteCallbackPtr = Marshal.GetFunctionPointerForDelegate(LoadCompleteCallback);

			BindForeignClassCallback += OnWrenBindForeignClass;
			BindForeignMethodCallback += OnWrenBindForeignMethod;
			
			ForeignMethodCallback += OnWrenCallForeign;
			ForeignMethodCallbackPtr = Marshal.GetFunctionPointerForDelegate(ForeignMethodCallback);
			
			ForeignAlloc += OnWrenCallForeignAllocator;
			ForeignAllocCallbackPtr = Marshal.GetFunctionPointerForDelegate(ForeignAlloc);
			
			ForeignFin += OnWrenCallForeignFinalizer;
			ForeignFinCallbackPtr = Marshal.GetFunctionPointerForDelegate(ForeignFin);
		}
		
		private static string FormatVersionNumber(int[] version)
		{
			string str = string.Empty;
			for (int i = 0; i < version.Length; i++)
			{
				int number = version[0];
				str += number < 0 ? "*" : number.ToString();
				if (i + 1 < version.Length) str += ".";
			}

			return str;
		}

		#region Callbacks

		internal static readonly NativeWriteDelegate WriteCallback;
		internal static readonly NativeErrorDelegate ErrorCallback;
		internal static readonly NativeResolveModuleDelegate ResolveCallback;
		internal static readonly NativeLoadModuleDelegate LoadCallback;
		private static readonly NativeLoadModuleCompleteDelegate LoadCompleteCallback;
		private static readonly IntPtr LoadCompleteCallbackPtr;
		internal static readonly NativeBindForeignClassDelegate BindForeignClassCallback;
		internal static readonly NativeBindForeignMethodDelegate BindForeignMethodCallback;
		private static readonly NativeForeignMethodDelegate ForeignMethodCallback;
		private static readonly IntPtr ForeignMethodCallbackPtr;
		private static readonly NativeForeignMethodDelegate ForeignAlloc;
		private static readonly IntPtr ForeignAllocCallbackPtr;
		private static readonly NativeForeignMethodDelegate ForeignFin;
		private static readonly IntPtr ForeignFinCallbackPtr;

		#region Msg

		/// <inheritdoc cref="NativeWriteDelegate"/>
#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(NativeWriteDelegate))]
#endif
		private static void OnWrenWrite(IntPtr vmPtr, string text)
		{
			VmUtils.GetData(vmPtr, out var vm, out var evt);
			evt.WriteEvent?.Invoke(vm, text);
		}

		/// <inheritdoc cref="NativeErrorDelegate"/>
#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(NativeErrorDelegate))]
#endif
		private static void OnWrenError(IntPtr vmPtr, ErrorType type, string module, int line, string message)
		{
			VmUtils.GetData(vmPtr, out var vm, out var evt);
			evt.ErrorEvent?.Invoke(vm, type, module, line, message);
		}

		#endregion

		#region Modules

		/// <inheritdoc cref="NativeResolveModuleDelegate"/>
#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(NativeResolveModuleDelegate))]
#endif
		private static IntPtr OnWrenResolveModule(IntPtr vmPtr, IntPtr importerPtr, IntPtr namePtr)
		{
			VmUtils.GetData(vmPtr, out var vm, out var evt);

			if (evt.ResolveModuleEvent == null) return namePtr;

			string name, importer;
			try
			{
				name = Marshal.PtrToStringAnsi(namePtr);
				importer = Marshal.PtrToStringAnsi(importerPtr);
			}
			catch (Exception)
			{
				return namePtr;
			}

			string resolved = evt.ResolveModuleEvent.Invoke(vm, importer, name);
			if (resolved == name || string.IsNullOrEmpty(resolved)) return namePtr;

			// the name needs to be given in wren's managed memory
			// 1. create an char* string
			var unmanagedName = Marshal.StringToHGlobalAnsi(resolved);
			var size = new UIntPtr((uint) (resolved.Length + 1) * (uint) IntPtr.Size);

			// 2. create pointer using same allocator that wren uses 
			IntPtr ptr = Config.NativeReallocate.Invoke(IntPtr.Zero, size, IntPtr.Zero);

			// 3. copy char* string over
			unsafe
			{
				Buffer.MemoryCopy(unmanagedName.ToPointer(), ptr.ToPointer(), size.ToUInt64(), size.ToUInt64());
			}

			Marshal.FreeHGlobal(unmanagedName);

			// 4. return wren managed pointer
			return ptr;
		}

		/// <inheritdoc cref="NativeLoadModuleDelegate"/>
#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(NativeLoadModuleDelegate))]
#endif
		private static NativeLoadModuleResult OnWrenLoadModule(IntPtr vmPtr, string name)
		{
			VmUtils.GetData(vmPtr, out var vm, out var evt);

			string result = evt.LoadModuleEvent?.Invoke(vm, name);
			if (string.IsNullOrEmpty(result)) return new NativeLoadModuleResult();

			IntPtr ptr = Marshal.StringToCoTaskMemAnsi(result);
			return new NativeLoadModuleResult()
			{
				Source = ptr,
				UserData = ptr,
				OnComplete = LoadCompleteCallbackPtr,
			};
		}

		/// <inheritdoc cref="NativeLoadModuleCompleteDelegate"/>
#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(NativeLoadModuleCompleteDelegate))]
#endif
		private static void OnWrenLoadComplete(IntPtr vm, string name, NativeLoadModuleResult result)
		{
			Marshal.FreeHGlobal(result.UserData);
		}

		#endregion

		#region Binding

		/// <inheritdoc cref="NativeBindForeignClassDelegate"/>
#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(NativeBindForeignClassDelegate))]
#endif
		internal static NativeForeignClass OnWrenBindForeignClass(IntPtr vmPtr, string module, string className)
		{
			VmUtils.GetData(vmPtr, out var vm, out var evt);

			if (evt.BindForeignClassEvent != null)
			{
				var foreignClass = evt.BindForeignClassEvent.Invoke(vm, module, className);
				return new NativeForeignClass
				{
					AllocateFn = ForeignAllocCallbackPtr,
					AllocateUserData = foreignClass.AllocPtr,
					FinalizeFn = ForeignFinCallbackPtr,
					FinalizeUserData = foreignClass.AllocPtr, 
				};
			}

			// wren defaults to aborting when no allocator is defined
			// to avoid sudden aborts we pass a dummy allocator, the following construct **will** fail
			return new NativeForeignClass
			{
				AllocateFn = ForeignAllocCallbackPtr,
			};
		}

		/// <inheritdoc cref="NativeBindForeignMethodDelegate"/>
#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(NativeBindForeignMethodDelegate))]
#endif
		private static NativeBindForeignMethodResult OnWrenBindForeignMethod(IntPtr vmPtr, string module,
			string className, bool isStatic, string signature)
		{
			VmUtils.GetData(vmPtr, out var vm, out var evt);

			if (evt.BindForeignMethodEvent == null) return new NativeBindForeignMethodResult();

			ForeignMethod method = evt.BindForeignMethodEvent.Invoke(vm, module, className, isStatic, signature);
			if (method.IsValid == false) return new NativeBindForeignMethodResult();

			return new NativeBindForeignMethodResult()
			{
				ExecuteFn = ForeignMethodCallbackPtr,
				UserData = method.Ptr,
			};
		}

#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(NativeForeignMethodDelegate))]
#endif
		private static void OnWrenCallForeign(IntPtr vmPtr, IntPtr userData)
		{
			var vm = VmUtils.FromPtr(vmPtr);
			ForeignMethod method = ForeignMethod.FromPtr(userData);
			method.Invoke(vm);
		}

#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(NativeForeignMethodDelegate))]
#endif
		private static void OnWrenCallForeignAllocator(IntPtr vmPtr, IntPtr userData)
		{
			var vm = VmUtils.FromPtr(vmPtr);
			var foreignClass = ForeignClass.FromAllocPtr(userData);
			if (foreignClass.IsValid == false) return;
			foreignClass.InvokeAllocator(vm);
		}

#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(NativeForeignMethodDelegate))]
#endif
		private static void OnWrenCallForeignFinalizer(IntPtr data, IntPtr userData)
		{
			var foreignClass = ForeignClass.FromAllocPtr(userData);
			if (foreignClass.IsValid == false) return;
			foreignClass.InvokeFinalizer(data);
		}

		#endregion

		#endregion
	}
}
