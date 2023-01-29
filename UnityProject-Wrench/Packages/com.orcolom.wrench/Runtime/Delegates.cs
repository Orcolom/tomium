using Tomia.Native;

namespace Tomia
{
	/// <inheritdoc cref="NativeWriteDelegate"/>
	public delegate void WriteDelegate(Vm vm, string text);

	/// <inheritdoc cref="NativeErrorDelegate"/>
	public delegate void ErrorDelegate(Vm vm, ErrorType type, string module, int line, string message);
	
	/// <inheritdoc cref="NativeResolveModuleDelegate"/>
	public delegate string ResolveModuleDelegate(Vm vm, string importer, string module);
	
	/// <inheritdoc cref="NativeLoadModuleDelegate"/>
	public delegate string LoadModuleDelegate(Vm vm, string module);
	
	/// <inheritdoc cref="NativeBindForeignMethodDelegate"/>
	public delegate ForeignMethod BindForeignMethodDelegate(Vm vm, string module, string className,
		bool isStatic, string signature);
	
	/// <inheritdoc cref="NativeBindForeignClassDelegate"/>
	public delegate ForeignClass BindForeignClassDelegate(Vm vm, string module, string className);
}
