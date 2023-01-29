using System;
using System.Runtime.InteropServices;

namespace Tomia.Native
{
	// A generic allocation function that handles all explicit memory management
	// used by Wren. It's used like so:
	//
	// - To allocate new memory, [memory] is NULL and [newSize] is the desired
	//   size. It should return the allocated memory or NULL on failure.
	//
	// - To attempt to grow an existing allocation, [memory] is the memory, and
	//   [newSize] is the desired size. It should return [memory] if it was able to
	//   grow it in place, or a new pointer if it had to move it.
	//
	// - To shrink memory, [memory] and [newSize] are the same as above but it will
	//   always return [memory].
	//
	// - To free memory, [memory] will be the memory to free and [newSize] will be
	//   zero. It should return NULL.
	internal delegate IntPtr NativeReallocateDelegate(IntPtr memory, UIntPtr newSize, IntPtr userData);

	/// <summary>
	/// display text to the user
	/// </summary>
	/// <param name="vm">pointer to c vm</param>
	/// <param name="text">text to display</param>
	internal delegate void NativeWriteDelegate(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)]
		string text);

	/// <summary>
	/// Reports an error to the user.
	///
	/// <para>
	/// 	An error detected during compile time is reported by calling this once with
	/// 	<paramref name="type"/> <see cref="ErrorType.CompileError"/>,
	///		the resolved name of the <paramref name="module"/> and <paramref name="line"/>
	/// 	where the error occurs, and the compiler's error <paramref name="message"/>.
	/// </para>
	///
	/// <para>
	/// 	A runtime error is reported by calling this once with <paramref name="type"/> <see cref="ErrorType.RuntimeError"/>,
	/// 	no <paramref name="module"/> or <paramref name="line"/>, and the runtime error's <paramref name="message"/>.
	/// 	After that, a series of <paramref name="type"/> <see cref="ErrorType.StackTrace"/> calls are
	/// 	made for each line in the stack trace. Each of those has the resolved
	/// 	<paramref name="module"/> and <paramref name="line"/> where the method or function is defined
	///		and <paramref name="message"/> is the name of the method or function.
	/// </para>
	/// </summary>
	/// <param name="vm">pointer to c vm</param>
	/// <param name="type">error type</param>
	/// <param name="module">module name</param>
	/// <param name="line">line position</param>
	/// <param name="message">the message</param>
	internal delegate void NativeErrorDelegate(IntPtr vm, ErrorType type,
		[MarshalAs(UnmanagedType.LPStr)]
		string module,
		int line,
		[MarshalAs(UnmanagedType.LPStr)]
		string message);

	/// <summary>
	///	<para>
	///		The callback Wren uses to resolve a module name.
	/// </para>
	///
	/// <para>
	/// 	Some host applications may wish to support "relative" imports, where the
	/// 	meaning of an import string depends on the module that contains it. To
	/// 	support that without baking any policy into Wren itself, the VM gives the
	/// 	host a chance to resolve an import string.
	/// </para>
	///
	/// <para>
	/// 	Before an import is loaded, it calls this, passing in the name of the
	/// 	module that contains the import and the import string. The host app can
	/// 	look at both of those and produce a new "canonical" string that uniquely
	/// 	identifies the module. This string is then used as the name of the module
	/// 	going forward. It is what is passed to <see cref="NativeConfig.NativeLoadModule"/>, how duplicate
	/// 	imports of the same module are detected, and how the module is reported in
	/// 	stack traces.
	/// </para>
	///
	/// <para>
	/// 	If you leave this function null, then the original import string is
	/// 	treated as the resolved string.
	/// </para>
	///
	/// <para>
	/// 	If an import cannot be resolved by the embedder, it should return null and
	/// 	Wren will report that as a runtime error.
	/// </para>
	///
	/// <para>
	/// 	Wren will take ownership of the string you return and free it for you, so
	/// 	it should be allocated using the same allocation function you provide above,
	///		or accessible via <see cref="Config.NativeReallocate"/>
	/// </para>
	/// 
	/// </summary>
	internal delegate IntPtr NativeResolveModuleDelegate(IntPtr vm,
		// [MarshalAs(UnmanagedType.LPStr)]
		IntPtr importer,
		IntPtr name);

	/// <summary>
	/// Loads and returns the source code for the module <param name="name"/>
	/// </summary>
	/// <param name="vm">pointer to the c vm</param>
	/// <param name="name">name of the module</param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate NativeLoadModuleResult NativeLoadModuleDelegate(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)]
		string name);

	/// <summary>
	/// Called after <see cref="NativeConfig.NativeLoadModule"/> is called for module <param name="name"/>.
	/// The original returned result is handed back to you in this callback, so that you can free memory if appropriate.
	/// </summary>
	/// <param name="vm">pinter to the c vm</param>
	/// <param name="name">name of the module</param>
	/// <param name="result">result created by <see cref="NativeConfig.NativeLoadModule"/></param>
	internal delegate void NativeLoadModuleCompleteDelegate(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)]
		string name,
		NativeLoadModuleResult result);

	/// <summary>
	/// Returns a pointer to a foreign method on <paramref name="className"/> in <paramref name="module"/> with <paramref name="signature"/>.
	/// </summary>
	/// <param name="vm">pointer to c vm</param>
	/// <param name="module">module name</param>
	/// <param name="className">class name</param>
	/// <param name="isStatic">is function static</param>
	/// <param name="signature">function signature</param>
	internal delegate NativeBindForeignMethodResult NativeBindForeignMethodDelegate(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)]
		string module,
		[MarshalAs(UnmanagedType.LPStr)]
		string className,
		[MarshalAs(UnmanagedType.I1)]
		bool isStatic,
		[MarshalAs(UnmanagedType.LPStr)]
		string signature);

	/// <summary>
	/// Returns a pair of pointers to the foreign methods used to allocate and
	/// finalize the data for instances of <paramref name="className"/> in resolved <paramref name="module"/>.
	/// </summary>
	/// <param name="vm">pointer to c vm</param>
	/// <param name="module">module name</param>
	/// <param name="className">class name</param>
	internal delegate NativeForeignClass NativeBindForeignClassDelegate(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)]
		string module,
		[MarshalAs(UnmanagedType.LPStr)]
		string className);

	/// <summary>
	/// A function callable from Wren code, but implemented in C#.
	/// </summary>
	/// <param name="vm"></param>
	internal delegate void NativeForeignMethodDelegate(IntPtr vm, IntPtr userData);
}
