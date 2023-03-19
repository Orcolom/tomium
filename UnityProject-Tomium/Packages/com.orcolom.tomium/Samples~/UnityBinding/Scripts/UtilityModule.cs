using Tomium.Builder;

namespace Tomium.Samples.UnityBinding
{
	public class UtilityModule : Module
	{
		public UtilityModule() : base(UtilityBinding.WrenName)
		{
			Add(new UtilityBinding());
		}
	}

	public class UtilityBinding : Class
	{
		public const string WrenName = "Utility";
		public const string MetaClassDerivesFrom__MetaClass_MetaClass = "MetaClassDerivesFrom";

		public UtilityBinding() : base(WrenName)
		{
			Add(Token.DangerousInsert(@$"
	static {MetaClassDerivesFrom__MetaClass_MetaClass}(derivative, base) {{
		while(derivative.supertype != Object) {{
			if (derivative.supertype == base) return true
			derivative = derivative.supertype
		}}
		return false
	}}
"));
		}
	}
}
