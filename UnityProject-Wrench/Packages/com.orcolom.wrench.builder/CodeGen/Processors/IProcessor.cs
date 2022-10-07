using Mono.Cecil;
using Wrench.Weaver;

namespace Wrench.CodeGen.Processors
{
	public interface IProcessor<TImports, TWeaver, TInput, TData>
		where TImports : Imports, new()
		where TWeaver : AWeaver<TImports>
		where TInput : IMemberDefinition
	{
		bool TryExtract(TWeaver weaver, TInput input, out TData data);
	}
}
