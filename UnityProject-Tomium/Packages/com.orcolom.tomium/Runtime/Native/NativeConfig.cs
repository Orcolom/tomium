using System;
using System.Runtime.InteropServices;

namespace Tomium.Native
{
	/// <summary>
	/// interop struct for WrenConfiguration
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal class NativeConfig
	{
		/// <summary>
		///		The callback Wren will use to allocate, reallocate, and deallocate memory.
		///		If `null`, defaults to a built-in function that uses `realloc` and `free`.
		/// </summary>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public NativeReallocateDelegate NativeReallocate;

		/// <inheritdoc cref="NativeResolveModule"/>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public NativeResolveModuleDelegate NativeResolveModule;

		/// <summary>
		/// The callback Wren uses to load a module.
		///
		/// <para>
		/// 	Since Wren does not talk directly to the file system, it relies on the
		/// 	embedder to physically locate and read the source code for a module. The
		/// 	first time an import appears, Wren will call this and pass in the name of
		/// 	the module being imported. The VM should return the source code for that
		/// 	module. Memory for the source should be allocated using <see cref="NativeReallocate"/> and
		/// 	Wren will take ownership over it.
		/// </para>
		///
		/// <para>
		/// 	This will only be called once for any given module name. Wren caches the
		/// 	result internally so subsequent imports of the same module will use the
		/// 	previous source and not call this.
		/// </para>
		///
		/// <para>
		/// 	If a module with the given name could not be found by the embedder, it
		/// 	should return NULL and Wren will report that as a runtime error.
		/// </para>
		/// </summary>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public NativeLoadModuleDelegate NativeLoadModule;

		/// <summary>
		///	The callback Wren uses to find a foreign method and bind it to a class.
		///
		/// <para>
		/// 	When a foreign method is declared in a class, this will be called with the
		/// 	foreign method's module, class, and signature when the class body is
		/// 	executed. It should return a pointer to the foreign function that will be
		/// 	bound to that method.
		/// </para>
		///
		/// <para>
		/// 	If the foreign function could not be found, this should return null and
		/// 	Wren will report it as runtime error.
		/// </para>
		/// </summary>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public NativeBindForeignMethodDelegate NativeBindForeignMethod;

		/// <summary>
		/// The callback Wren uses to find a foreign class and get its foreign methods.
		///
		/// <para>
		/// 	When a foreign class is declared, this will be called with the class's
		/// 	module and name when the class body is executed. It should return the
		/// 	foreign functions uses to allocate and (optionally) finalize the bytes
		/// 	stored in the foreign object when an instance is created.
		/// </para>
		/// </summary>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public NativeBindForeignClassDelegate NativeBindForeignClass;

		/// <inheritdoc cref="NativeWriteDelegate"/>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public NativeWriteDelegate NativeWrite;

		/// <inheritdoc cref="NativeErrorDelegate"/>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public NativeErrorDelegate NativeError;

		/// <summary>
		///	The number of bytes Wren will allocate before triggering the first garbage collection.
		///
		/// If zero, defaults to 10MB.
		/// </summary>
		public UIntPtr InitialHeapSize;

		/// <summary>
		/// After a collection occurs, the threshold for the next collection is
		/// determined based on the number of bytes remaining in use. This allows Wren
		/// to shrink its memory usage automatically after reclaiming a large amount
		/// of memory.
		///
		/// This can be used to ensure that the heap does not get too small, which can
		/// in turn lead to a large number of collections afterwards as the heap grows
		/// back to a usable size.
		///
		/// If zero, defaults to 1MB.
		/// </summary>
		public UIntPtr MinHeapSize;

		/// <summary>
		/// Wren will resize the heap automatically as the number of bytes
		/// remaining in use after a collection changes. This number determines the
		/// amount of additional memory Wren will use after a collection, as a
		/// percentage of the current heap size.
		///
		/// For example, say that this is 50. After a garbage collection, when there
		/// are 400 bytes of memory still in use, the next collection will be triggered
		/// after a total of 600 bytes are allocated (including the 400 already in
		/// use.)
		///
		/// Setting this to a smaller number wastes less memory, but triggers more
		/// frequent garbage collections.
		///
		/// If zero, defaults to 50.
		/// </summary>
		public int HeapGrowthPercent;

		/// <summary>
		/// User-defined data associated with the VM.
		/// <remarks>
		///		we doesn't give the option to provide this. here for struct layout
		/// </remarks>
		/// </summary>
		public unsafe void* UserData;
	}
}
