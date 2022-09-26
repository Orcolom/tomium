using System.Text;

namespace Wrench.Builder
{
	public class ScriptBuilder
	{
		private readonly StringBuilder _sb = new StringBuilder();
		internal readonly TokenCollector Collector = new TokenCollector();

		public override string ToString() => _sb.ToString();

		public void Clear()
		{
			_sb.Clear();
			Collector.Clear();
		}

		internal void ProcessTokens()
		{
			for (int i = 0; i < Collector.Tokens.Count; i++)
			{
				Collector.Tokens[i].Stringify(_sb);
			}
		}
		
		public string CreateModuleSource(in Vm vm, Module module) 
		{
			Clear();
			module.Flatten(vm, Collector);
			ProcessTokens();
			return ToString();
		}
	}
}
