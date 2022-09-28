using Wrench.Weaver;

namespace Wrench.CodeGen
{
	public class WrenchWeaver : AWeaver
	{
		public WrenchWeaver(WeaverLogger logger) : base(logger) { }

		protected override void Weave() { }
	}
}
