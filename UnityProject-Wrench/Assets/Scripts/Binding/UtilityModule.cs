using Wrench.Builder;

namespace Binding
{
	[WrenchModule("Utility")]
	public class UtilityModule : Module { }

	[WrenchClass(typeof(UtilityModule), WrenName)]
	public class UtilityBinding : Class
	{
		public const string WrenName = "Utility";
		public const string MetaClassDerivesFrom__MetaClass_MetaClass = "MetaClassDerivesFrom";

		public UtilityBinding()
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
