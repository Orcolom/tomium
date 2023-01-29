using System.Text;

namespace Tomia.Builder
{
	public class ScriptBuilder
	{
		private readonly StringBuilder _sb = new StringBuilder(2048);
		internal readonly TokenCollector Collector = new TokenCollector();

		public override string ToString()
		{
			using (ProfilerUtils.AllocScope.Auto())
			{
				return _sb.ToString();
			}
		}

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
