using System;
using System.Diagnostics.CodeAnalysis;
using Tomia.Native;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using UnityEngine;

namespace Tomia
{
	public struct Vm : IEquatable<Vm>, IDisposable
	{
		[NativeDisableUnsafePtrRestriction]
		private IntPtr _ptr;
		internal IntPtr Ptr => _ptr;

		public readonly Slot Slot0;
		public readonly Slot Slot1;
		public readonly Slot Slot2;
		public readonly Slot Slot3;
		public readonly Slot Slot4;
		public readonly Slot Slot5;
		public readonly Slot Slot6;
		public readonly Slot Slot7;
		public readonly Slot Slot8;
		public readonly Slot Slot9;
		public readonly Slot Slot10;
		public readonly Slot Slot11;
		public readonly Slot Slot12;
		public readonly Slot Slot13;
		public readonly Slot Slot14;
		public readonly Slot Slot15;
		public readonly Slot Slot16;

		internal Vm(IntPtr ptr)
		{
			_ptr = ptr;

			Slot0 = new Slot(_ptr, 0);
			Slot1 = new Slot(_ptr, 1);
			Slot2 = new Slot(_ptr, 2);
			Slot3 = new Slot(_ptr, 3);
			Slot4 = new Slot(_ptr, 4);
			Slot5 = new Slot(_ptr, 5);
			Slot6 = new Slot(_ptr, 6);
			Slot7 = new Slot(_ptr, 7);
			Slot8 = new Slot(_ptr, 8);
			Slot9 = new Slot(_ptr, 9);
			Slot10 = new Slot(_ptr, 10);
			Slot11 = new Slot(_ptr, 11);
			Slot12 = new Slot(_ptr, 12);
			Slot13 = new Slot(_ptr, 13);
			Slot14 = new Slot(_ptr, 14);
			Slot15 = new Slot(_ptr, 15);
			Slot16 = new Slot(_ptr, 16);
		}

		public void Dispose()
		{
			ProfilerUtils.Log($"Disposed {nameof(Vm)} : {_ptr}");
			
			if (this.IsValid() == false) return;

			var callHandles = Managed.ManagedClasses[_ptr].CallHandles;
			Debug.Assert(callHandles.Count == 0, "Created CallHandles should have been disposed before the vm get disposed.");
			for (int i = callHandles.Count - 1; i >= 0; i--)
			{
				Handle.FromPtr(callHandles[i]).Dispose();
			}
			
			Interop.wrenFreeVM(_ptr);
			Managed.ManagedClasses.Remove(_ptr);
			VmUtils.Vms.Data.Map.Remove(_ptr);
			_ptr = IntPtr.Zero;
		}

		public static Vm New(Config? config = null)
		{
			// set config
			var configuration = Config.ToInterop(config ?? Config.Default);
			configuration.NativeWrite = Tomia.WriteCallback;
			configuration.NativeError = Tomia.ErrorCallback;
			configuration.NativeResolveModule = Tomia.ResolveCallback;
			configuration.NativeLoadModule = Tomia.LoadCallback;
			configuration.NativeBindForeignClass = Tomia.BindForeignClassCallback;
			configuration.NativeBindForeignMethod = Tomia.BindForeignMethodCallback;

			// get vm ptr
			IntPtr ptr = Interop.wrenNewVM(configuration);
			Vm vm = new Vm(ptr);

			// store data
			VmUtils.Vms.Data.Map.Add(ptr, vm);
			Managed.ManagedClasses.Add(ptr, new Managed()); // store *managed* events separately 

			ProfilerUtils.Log($"Created {nameof(Vm)} : {vm._ptr}");

			return vm;
		}

		public override int GetHashCode() => _ptr.GetHashCode();
		public override bool Equals(object obj)  => obj switch
		{
			Vm vm => Equals(vm),
			_ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null),
		};

		public bool Equals(Vm other) => other._ptr == _ptr;

		public static bool operator ==(Vm left, Vm right) => left.Equals(right);
		public static bool operator !=(Vm left, Vm right) => left.Equals(right) == false;
	}

	public static class VmUtils
	{
		private static readonly ProfilerMarker PrefInterpret = ProfilerUtils.Create($"{nameof(Vm)}.{nameof(Interpret)}");
		private static readonly ProfilerMarker PrefCall = ProfilerUtils.Create($"{nameof(Vm)}.{nameof(Call)}");

		internal static readonly SharedStatic<StaticMap<Vm>> Vms = SharedStatic<StaticMap<Vm>>.GetOrCreate<Vm>();

		static VmUtils()
		{
			Vms.Data.Init(16);
		}

		#region Expected

		
		public static bool IsValid(this Vm vm) => IsValid(vm.Ptr);

		public static bool IsValid(IntPtr vmPtr)
		{
			Vm vm = FromPtr(vmPtr);
			return vm.Ptr != IntPtr.Zero;
		}

		public static bool ExpectedValid(this Vm vm) => ExpectedValid(vm.Ptr);

		public static bool ExpectedValid(IntPtr vmPtr)
		{
			if (IsValid(vmPtr)) return false;

			throw new ObjectDisposedException(nameof(Vm), $"Vm {vmPtr} is already disposed");
			// return true;
		}

		internal static bool ExpectedSameVm(this Vm self, Slot other) => ExpectedSameVm(self.Ptr, other.VmPtr, "Slot is not from this vm");
		internal static bool ExpectedSameVm(this Slot self, Slot other) => ExpectedSameVm(self.VmPtr, other.VmPtr, "Slots are not from the same vm");

		internal static bool ExpectedSameVm(IntPtr ptr, IntPtr other, string error)
		{
			if (ptr == other) return false;

			throw new ArgumentOutOfRangeException("ExpectedSameVm",error);
			// return true;
		}

		/// <summary>
		/// Get the true vm object attached to a pointer
		/// </summary>
		/// <param name="ptr">pointer behind the vm</param>
		/// <returns>the "true" vm</returns>
		internal static Vm FromPtr(IntPtr ptr)
		{
			return Vms.Data.Map.TryGetValue(ptr, out var vm) ? vm : new Vm();
		}

		/// <summary>
		/// Get the data attached to an ptr
		/// </summary>
		/// <param name="ptr">pointer</param>
		/// <param name="vm">vm attached to the pointer</param>
		/// <param name="managed">managed class attached to the pointer</param>
		internal static void GetData(IntPtr ptr, out Vm vm, out Managed managed)
		{
			vm = FromPtr(ptr);
			managed = Managed.ManagedClasses[ptr];
		}

		#endregion

		#region Methods
		
		/// <summary>
		/// Runs <paramref name="source"/>, a string of Wren source code, in a new fiber in <paramref name="vm"/> in the context of resolved <paramref name="module"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="module">module name</param>
		/// <param name="source">module source</param>
		/// <returns>interpret result</returns>
		public static InterpretResult Interpret(this Vm vm, [DisallowNull] string module, string source)
		{
			PrefInterpret.Begin();

			if (ExpectedValid(vm)) return InterpretResult.CompileError;
			if (string.IsNullOrEmpty(module)) throw new ArgumentNullException();
			var result = Interop.wrenInterpret(vm.Ptr, module, source);
			PrefInterpret.End();

			return result;
		}

		/// <summary>
		/// Calls <paramref name="method"/>, using the receiver and arguments previously set up on the stack.
		///
		/// <para>
		/// 	<paramref name="method"/> must have been created by a call to <see cref="MakeCallHandle"/>. The
		/// 	arguments to the method must be already on the stack. The receiver should be
		/// 	in slot 0 with the remaining arguments following it, in order. It is an
		/// 	error if the number of arguments provided does not match the method's
		/// 	signature.
		/// </para>
		///
		/// <para>
		///		After this returns, you can access the return value from slot 0 on the stack.
		/// </para>
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="method">pointer to c handle</param>
		/// <returns>interpret result</returns>
		public static InterpretResult Call(this Vm vm, Handle method)
		{
			PrefCall.Begin();

			if (ExpectedValid(vm)) return InterpretResult.CompileError;
			if (Handle.IfInvalid(method)) return InterpretResult.CompileError;
			var result = Interop.wrenCall(vm.Ptr, method.Ptr);

			PrefCall.End();

			return result;
		}

		/// <inheritdoc cref="Native.Interop.wrenCollectGarbage"/>
		public static void Gc(this Vm vm)
		{
			if (ExpectedValid(vm)) return;
			Interop.wrenCollectGarbage(vm.Ptr);
		}

		/// <inheritdoc cref="Native.Interop.wrenAbortFiber"/>
		public static void Abort(this Vm vm, in Slot msg)
		{
			if (ExpectedValid(vm)) return;
			Interop.wrenAbortFiber(vm.Ptr, msg.Index);
		}

		/// <summary>
		/// Creates a handle that can be used to invoke a method with <paramref name="signature"/> on
		/// using a receiver and arguments that are set up on the stack.
		///
		/// <para>
		/// 	This handle can be used repeatedly to directly invoke that method from C code using <see cref="Call"/>.
		/// </para>
		///
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="signature">method signature</param>
		/// <returns>pointer to handle</returns>
		public static Handle MakeCallHandle(this Vm vm, string signature)
		{
			return Handle.New(vm, signature);
		}

		/// <summary>
		/// Runs <see cref="HasModule"/> and <see cref="HasVariable"/> sequentially
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="module"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static bool HasModuleAndVariable(this Vm vm, string module, string variable)
		{
			if (vm.HasModule(module) == false) return false;
			if (vm.HasVariable(module, variable) == false) return false;
			return true;
		}

		/// <summary>
		/// Looks up the top level variable with <paramref name="name"/> in resolved <paramref name="module"/>, 
		/// returns false if not found. The module must be imported at the time, 
		/// use <see cref="HasModule"/>  to ensure that before calling.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="module">module the variable is part off</param>
		/// <param name="name">variable to check</param>
		public static bool HasVariable(this Vm vm, string module, string name)
		{
			return Interop.wrenHasVariable(vm.Ptr, module, name);
		}

		/// <summary>
		/// Returns true if <paramref name="module"/> has been imported/resolved before, false if not.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="module">module to check</param>
		public static bool HasModule(this Vm vm, string module)
		{
			return Interop.wrenHasModule(vm.Ptr, module);
		}

		#endregion

		#region Slots

		public static int GetSlotCount(this Vm vm)
		{
			if (ExpectedValid(vm)) return 0;
			return Interop.wrenGetSlotCount(vm.Ptr);
		}

		public static void EnsureSlots(this Vm vm, int size)
		{
			if (ExpectedValid(vm)) return;
			Interop.wrenEnsureSlots(vm.Ptr, size);
		}

		#endregion

		#region Managed

		public static void SetWriteListener(this Vm vm, WriteDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].WriteEvent = @delegate;
		}

		public static void SetErrorListener(this Vm vm, ErrorDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].ErrorEvent = @delegate;
		}

		public static void SetResolveModuleListener(this Vm vm, ResolveModuleDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].ResolveModuleEvent = @delegate;
		}

		public static void SetLoadModuleListener(this Vm vm, LoadModuleDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].LoadModuleEvent = @delegate;
		}

		public static void SetBindForeignMethodListener(this Vm vm, BindForeignMethodDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].BindForeignMethodEvent = @delegate;
		}

		public static void SetBindForeignClassListener(this Vm vm, BindForeignClassDelegate @delegate)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].BindForeignClassEvent = @delegate;
		}

		public static void SetUserData<T>(this Vm vm, T obj)
		{
			if (ExpectedValid(vm)) return;
			Managed.ManagedClasses[vm.Ptr].UserData = obj;
		}

		public static T GetUserData<T>(this Vm vm)
		{
			if (ExpectedValid(vm)) return default;
			return (T) Managed.ManagedClasses[vm.Ptr].UserData;
		}

		#endregion
	}
}
