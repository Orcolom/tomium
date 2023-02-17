using System;
using System.Runtime.InteropServices;

namespace Tomia.Native
{
	internal static class Interop
	{
		// Helper Info:
		// | c			          | c#			 | remark
		// |------------------|----------|------------- ----  ---  --   -
		// | size_t           | UIntPtr  | 
		// | char*            | string   | [MarshalAs(UnmanagedType.LPStr)]
		// | bool             | bool     | [MarshalAs(UnmanagedType.I1)]
		// | <struct>*        | <class>  | 
		// | <struct>         | <class>  |
		// | return <struct>  | <struct> | when struct needs to be returned all internals need to be a bittable  
		// | <any>*           | IntPtr   |
		// |				          |					 |

#if UNITY_EDITOR == false && (ENABLE_IL2CPP || UNITY_WEBGL)
		public const string DllName = "__Internal";
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		public const string DllName = "wren.dll";
#endif

		/// <summary>
		/// Get wren version
		/// </summary>
		[DllImport(DllName)]
		internal static extern int wrenGetVersionNumber();

		/// <summary>
		/// Get default initialized WrenConfig data
		/// </summary>
		/// <param name="nativeConfig">interop config filled with the default values</param>
		[DllImport(DllName)]
		internal static extern void wrenInitConfiguration([Out] NativeConfig nativeConfig);

		/// <summary>
		/// Creates a new Wren virtual machine using the given <paramref name="nativeConfig"/>.
		/// If <paramref name="nativeConfig"/> is `null`, uses a default configuration created by <see cref="wrenInitConfiguration"/>.
		/// </summary>
		/// <param name="nativeConfig">config with bindings and settings</param>
		/// <returns>pointer to c vm</returns>
		[DllImport(DllName)]
		internal static extern IntPtr wrenNewVM(NativeConfig nativeConfig);

		/// <summary>
		/// Disposes of all resources is use by <paramref name="vm"/>,
		/// which was previously created by a call to <see cref="wrenNewVM"/>.
		/// </summary>
		/// <param name="vm">pointer of the vm to free</param>
		[DllImport(DllName)]
		internal static extern void wrenFreeVM(IntPtr vm);

		/// <summary>
		/// Immediately run the garbage collector to free unused memory.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		[DllImport(DllName)]
		internal static extern void wrenCollectGarbage(IntPtr vm);

		/// <summary>
		/// Runs <paramref name="source"/>, a string of Wren source code in a new fiber in <paramref name="vm"/> in the context of resolved <paramref name="module"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="module">module name</param>
		/// <param name="source">module source</param>
		/// <returns>interpret result</returns>
		[DllImport(DllName)]
		internal static extern InterpretResult wrenInterpret(IntPtr vm, string module, string source);

		/// <summary>
		/// Creates a handle that can be used to invoke a method with <paramref name="signature"/> on
		/// using a receiver and arguments that are set up on the stack.
		///
		/// <para>
		/// 	This handle can be used repeatedly to directly invoke that method from C code using <see cref="wrenCall(IntPtr,IntPtr)"/>.
		/// </para>
		///
		/// <para>
		///		When you are done with this handle, it must be released using <see cref="wrenReleaseHandle(IntPtr,IntPtr)"/>.
		/// </para>
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="signature">method signature</param>
		/// <returns>pointer to handle</returns>
		[DllImport(DllName)]
		internal static extern IntPtr wrenMakeCallHandle(IntPtr vm,
			[MarshalAs(UnmanagedType.LPStr)]
			string signature);

		/// <summary>
		/// Calls <paramref name="method"/>, using the receiver and arguments previously set up on the stack.
		///
		/// <para>
		/// 	<paramref name="method"/> must have been created by a call to <see cref="wrenMakeCallHandle"/>. The
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
		[DllImport(DllName)]
		internal static extern InterpretResult wrenCall(IntPtr vm, IntPtr method);

		/// <summary>
		/// Releases the reference stored in <paramref name="handle"/>. After calling this, <paramref name="handle"/> can
		/// no longer be used.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="handle"></param>
		[DllImport(DllName)]
		internal static extern void wrenReleaseHandle(IntPtr vm, IntPtr handle);

		/// <summary>
		/// Returns the number of slots available to the current foreign method.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <returns>slot count</returns>		
		[DllImport(DllName)]
		internal static extern int wrenGetSlotCount(IntPtr vm);

		/// <summary>
		/// Ensures that the foreign method stack has at least <paramref name="slots"/> available for
		/// use, growing the stack if needed.
		///
		/// Does not shrink the stack if it has more than enough slots.
		///
		/// It is an error to call this from a finalizer.
		/// </summary>
		/// <param name="vm"></param>
		/// <param name="slots"></param>
		[DllImport(DllName)]
		internal static extern void wrenEnsureSlots(IntPtr vm, int slots);

		/// <summary>
		/// Gets the type of the object in <paramref name="slot"/>
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot index</param>
		/// <returns>resolved type</returns>
		[DllImport(DllName)]
		internal static extern ValueType wrenGetSlotType(IntPtr vm, int slot);

		/// <summary>
		/// Reads a boolean value from <paramref name="slot"/>.
		/// It is an error to call this if the slot does not contain a boolean value.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		[DllImport(DllName)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static extern bool wrenGetSlotBool(IntPtr vm, int slot);

		/// <summary>
		/// Stores the boolean <paramref name="value"/> in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store int</param>
		/// <param name="value">value to store</param>
		[DllImport(DllName)]
		internal static extern void wrenSetSlotBool(IntPtr vm, int slot, bool value);

		/// <summary>
		/// Reads a byte array from <paramref name="slot"/>.
		///
		/// <para>
		/// 	The memory for the returned string is owned by Wren. You can inspect it
		/// 	while in your foreign method, but cannot keep a pointer to it after the
		/// 	function returns, since the garbage collector may reclaim it.
		/// </para>
		///
		/// <para>
		/// 	Returns a pointer to the first byte of the array and fill [length] with the
		/// 	number of bytes in the array.
		/// </para>
		///
		/// It is an error to call this if the slot does not contain a string.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		/// <param name="length">number of bytes</param>
		[DllImport(DllName)]
		internal static extern IntPtr wrenGetSlotBytes(IntPtr vm, int slot, [Out] out int length);

		/// <summary>
		/// Stores the array <paramref name="length"/> of <paramref name="bytes"/> in <paramref name="slot"/>.
		///
		/// The bytes are copied to a new string within Wren's heap, so you can free
		/// memory used by them after this is called.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store in</param>
		/// <param name="bytes">bytes to store</param>
		/// <param name="length">bytes length</param>
		[DllImport(DllName)]
		internal static extern void wrenSetSlotBytes(IntPtr vm, int slot, IntPtr bytes, UIntPtr length);

		/// <summary>
		/// Reads a number from <paramref name="slot"/>.
		///
		/// It is an error to call this if the slot does not contain a number.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		[DllImport(DllName)]
		internal static extern double wrenGetSlotDouble(IntPtr vm, int slot);

		/// <summary>
		/// Stores the numeric <paramref name="value"/> in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store in</param>
		/// <param name="value">value to store</param>
		[DllImport(DllName)]
		internal static extern void wrenSetSlotDouble(IntPtr vm, int slot, double value);

		/// <summary>
		/// Creates a new instance of the foreign class stored in <paramref name="classSlot"/> with <paramref name="size"/>
		/// bytes of raw storage and places the resulting object in <paramref name="slot"/>.
		///
		/// <para>
		/// 	This does not invoke the foreign class's constructor on the new instance. If
		/// 	you need that to happen, call the constructor from Wren, which will then
		/// 	call the allocator foreign method. In there, call this to create the object
		/// 	and then the constructor will be invoked when the allocator returns.
		/// </para>
		///
		/// Returns a pointer to the foreign object's data.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		/// <param name="classSlot">slot to store in</param>
		/// <param name="size">size of foreign data</param>
		[DllImport(DllName)]
		internal static extern IntPtr wrenSetSlotNewForeign(IntPtr vm, int slot, int classSlot, IntPtr size);


		/// <summary>
		/// Reads a foreign object from <paramref name="slot"/> and returns a pointer to the foreign data
		/// stored with it.
		///
		/// It is an error to call this if the slot does not contain an instance of a
		/// foreign class.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		[DllImport(DllName)]
		internal static extern IntPtr wrenGetSlotForeign(IntPtr vm, int slot);

		/// <summary>
		/// Stores the string <paramref name="text"/> in <paramref name="slot"/>
		///
		/// <para>
		/// 	The <paramref name="text"/> is copied to a new string within Wren's heap, so you can free
		/// 	memory used by it after this is called. The length is calculated using
		/// 	[strlen()]. If the string may contain any null bytes in the middle, then you
		/// 	should use <see cref="wrenSetSlotBytes"/> instead.
		/// </para>
		/// </summary>
		[DllImport(DllName)]
		internal static extern void wrenSetSlotString(IntPtr vm, int slot,
			[MarshalAs(UnmanagedType.LPStr)]
			string text);

		/// <summary>
		/// Reads a string from <paramref name="slot"/>.
		///
		/// The memory for the returned string is owned by Wren. You can inspect it
		/// while in your foreign method, but cannot keep a pointer to it after the
		/// function returns, since the garbage collector may reclaim it.
		///
		/// It is an error to call this if the slot does not contain a string.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		[DllImport(DllName)]
		internal static extern IntPtr wrenGetSlotString(IntPtr vm, int slot);

		/// <summary>
		/// Stores null in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store in</param>
		[DllImport(DllName)]
		internal static extern void wrenSetSlotNull(IntPtr vm, int slot);

		/// <summary>
		/// Creates a handle for the value stored in <paramref name="slot"/>.
		///
		/// This will prevent the object that is referred to from being garbage collected
		/// until the handle is released by calling <see cref="wrenReleaseHandle(IntPtr,IntPtr)"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		[DllImport(DllName)]
		internal static extern IntPtr wrenGetSlotHandle(IntPtr vm, int slot);


		/// <summary>
		/// Stores the value captured in <paramref name="handle"/> in <paramref name="slot"/>.
		///
		/// This does not release the handle for the value.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store in</param>
		/// <param name="handle">pointer of handle to store</param>
		[DllImport(DllName)]
		internal static extern void wrenSetSlotHandle(IntPtr vm, int slot, IntPtr handle);

		/// <summary>
		/// Stores a new empty list in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store in</param>
		[DllImport(DllName)]
		internal static extern void wrenSetSlotNewList(IntPtr vm, int slot);

		/// <summary>
		/// Returns the number of elements in the list stored in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot get from</param>
		/// <returns>count of list elements</returns>
		[DllImport(DllName)]
		internal static extern int wrenGetListCount(IntPtr vm, int slot);

		/// <summary>
		/// Sets the value stored at <paramref name="index"/> in the list at <paramref name="listSlot"/>, 
		/// to the value from <paramref name="elementSlot"/>. 
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="listSlot">slot where the list is</param>
		/// <param name="index">index in the list</param>
		/// <param name="elementSlot">slot of value to store in list</param>
		[DllImport(DllName)]
		internal static extern void wrenSetListElement(IntPtr vm, int listSlot, int index, int elementSlot);

		/// <summary>
		/// Reads element <paramref name="index"/> from the list in <paramref name="listSlot"/> and stores it in <paramref name="elementSlot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="listSlot">slot where the list is</param>
		/// <param name="index">index in the list</param>
		/// <param name="elementSlot">slot to store the value in</param>
		[DllImport(DllName)]
		internal static extern void wrenGetListElement(IntPtr vm, int listSlot, int index, int elementSlot);

		/// <summary>
		/// Takes the value stored at <paramref name="elementSlot"/> and inserts it into the list stored
		/// at <paramref name="listSlot"/> at <paramref name="index"/>.
		///
		/// <para>
		/// 	As in Wren, negative indexes can be used to insert from the end. To append an element, use `-1` for the index.
		/// </para>
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="listSlot">slot where the list is</param>
		/// <param name="index">index to store element</param>
		/// <param name="elementSlot">slot of value to store in list</param>
		[DllImport(DllName)]
		internal static extern void wrenInsertInList(IntPtr vm, int listSlot, int index, int elementSlot);

		/// <summary>
		/// Stores a new empty map in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store</param>
		[DllImport(DllName)]
		internal static extern void wrenSetSlotNewMap(IntPtr vm, int slot);

		/// <summary>
		/// Returns the number of entries in the map stored in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to look at</param>
		/// <returns>count of entries</returns>
		[DllImport(DllName)]
		internal static extern int wrenGetMapCount(IntPtr vm, int slot);

		/// <summary>
		/// Returns true if the key in <paramref name="keySlot"/> is found in the map placed in <paramref name="mapSlot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="mapSlot">slot where the map is</param>
		/// <param name="keySlot">slot of value to check exists</param>
		[DllImport(DllName)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static extern bool wrenGetMapContainsKey(IntPtr vm, int mapSlot, int keySlot);

		/// <summary>
		/// Retrieves a value with the key in <paramref name="keySlot"/> from the map in <paramref name="mapSlot"/> and
		/// stores it in <paramref name="valueSlot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="mapSlot">slot where the map is</param>
		/// <param name="keySlot">slot of the key</param>
		/// <param name="valueSlot">slot to store the value in</param>
		[DllImport(DllName)]
		internal static extern void wrenGetMapValue(IntPtr vm, int mapSlot, int keySlot, int valueSlot);

		/// <summary>
		/// Takes the value stored at <paramref name="valueSlot"/> and inserts it into the map stored
		/// at <paramref name="mapSlot"/> with key <paramref name="keySlot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="mapSlot">slot where map is</param>
		/// <param name="keySlot">slot of the key to store in</param>
		/// <param name="valueSlot">slot of the value to store</param>
		[DllImport(DllName)]
		internal static extern void wrenSetMapValue(IntPtr vm, int mapSlot, int keySlot, int valueSlot);

		/// <summary>
		/// Removes a value from the map in <paramref name="mapSlot"/>, with the key from <paramref name="keySlot"/>,
		/// and place it in <paramref name="removedValueSlot"/>. If not found, <paramref name="removedValueSlot"/> is
		/// set to null, the same behaviour as the Wren Map API.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="mapSlot">slot where the map is</param>
		/// <param name="keySlot">slot of the key to remove</param>
		/// <param name="removedValueSlot">slot to store value that was removed</param>
		[DllImport(DllName)]
		internal static extern void wrenRemoveMapValue(IntPtr vm, int mapSlot, int keySlot, int removedValueSlot);

		/// <summary>
		/// Looks up the top level variable with <paramref name="name"/> in resolved <paramref name="module"/> and stores
		/// it in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="module">module to check in</param>
		/// <param name="name">name to get</param>
		/// <param name="slot">slot to store in</param>
		[DllImport(DllName)]
		internal static extern void wrenGetVariable(IntPtr vm,
			[MarshalAs(UnmanagedType.LPStr)]
			string module,
			[MarshalAs(UnmanagedType.LPStr)]
			string name, int slot);

		/// <summary>
		/// Looks up the top level variable with <paramref name="name"/> in resolved <paramref name="module"/>, 
		/// returns false if not found. The module must be imported at the time, 
		/// use <see cref="wrenHasModule(IntPtr,string)"/>  to ensure that before calling.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="module">module to check in</param>
		/// <param name="name">name to check for</param>
		[DllImport(DllName)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static extern bool wrenHasVariable(IntPtr vm,
			[MarshalAs(UnmanagedType.LPStr)]
			string module,
			[MarshalAs(UnmanagedType.LPStr)]
			string name);

		/// <summary>
		/// Returns true if <paramref name="module"/> has been imported/resolved before, false if not.
		/// </summary>
		/// <param name="vm">pointer to vm</param>
		/// <param name="module">module to check</param>
		[DllImport(DllName)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static extern bool wrenHasModule(IntPtr vm,
			[MarshalAs(UnmanagedType.LPStr)]
			string module);

		/// <summary>
		/// Sets the current fiber to be aborted, and uses the value in <paramref name="slot"/> as the runtime error object.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot for the runtime error</param>
		[DllImport(DllName)]
		internal static extern void wrenAbortFiber(IntPtr vm, int slot);

		/// <summary>
		/// get userdata
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <returns>pointer to userdata</returns>
		[DllImport(DllName)]
		internal static extern unsafe void* wrenGetUserData(IntPtr vm);

		/// <summary>
		/// set userdata
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="userData">pointer to userdata</param>
		[DllImport(DllName)]
		internal static extern unsafe void wrenSetUserData(IntPtr vm, void* userData);
	}
}
