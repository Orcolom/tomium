using Wrench.Native;

namespace Wrench
{
	/// <inheritdoc cref="NativeWriteDelegate"/>
	public delegate void WriteDelegate(in Vm vm, string text);

	/// <inheritdoc cref="NativeErrorDelegate"/>
	public delegate void ErrorDelegate(in Vm vm, ErrorType type, string module, int line, string message);
	
	/// <inheritdoc cref="NativeResolveModuleDelegate"/>
	public delegate string ResolveModuleDelegate(in Vm vm, string importer, string name);
	
	/// <inheritdoc cref="NativeLoadModuleDelegate"/>
	public delegate string LoadModuleDelegate(in Vm vm, string name);
	
	/// <inheritdoc cref="NativeBindForeignMethodDelegate"/>
	public delegate ForeignMethod BindForeignMethodDelegate(in Vm vm, string module, string className,
		bool isStatic, string signature);
	
	/// <inheritdoc cref="NativeBindForeignClassDelegate"/>
	public delegate ForeignClass BindForeignClassDelegate(in Vm vm, string module, string className);
}
