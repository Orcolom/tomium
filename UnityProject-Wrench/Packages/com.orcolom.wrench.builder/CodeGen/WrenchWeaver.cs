using Wrench.Weaver;

namespace Wrench.CodeGen
{
	public class WrenchWeaver : AWeaver
	{
		public WrenchWeaver(WeaverLogger logger) : base(logger)
		{
			var v = new ForeignClass();
		}

		protected override void Weave()
		{
			var module = MainModule;

			var types = module.Types;
			var count = types.Count;
			for (int i = 0; i < count; i++)
			{
				var type = types[i];

				Logger.Log(type.Name);
				// if (type.IsDerivedFrom<Class>() == false) continue;
				// Logger.Warning(type.Name);
			}
		}
		
		
	}
}
