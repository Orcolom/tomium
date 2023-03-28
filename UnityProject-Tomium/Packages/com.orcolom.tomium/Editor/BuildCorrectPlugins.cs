using Tomium.Native;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Tomium
{
	public class BuildCorrectPlugins : IPreprocessBuildWithReport
	{
		public int callbackOrder { get; }
		
		
		public void OnPreprocessBuild(BuildReport report)
		{
			var allPlugins = PluginImporter.GetAllImporters();

			for (int i = 0; i < allPlugins.Length; i++)
			{
				var plugin = allPlugins[i];

				if (plugin.assetPath.Contains("/com.orcolom.tomium/") == false) continue;
				plugin.SetIncludeInBuildDelegate(IncludeInBuildDelegate);
			}
		}

		private static bool IncludeInBuildDelegate(string path)
		{
			var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
			var backend = PlayerSettings.GetScriptingBackend(group);

			// il2cpp or webgl always uses wren.c
			if (path.Contains("wren.c"))
			{
				return backend == ScriptingImplementation.IL2CPP
					|| EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL;
			}

			if (path.Contains("_d")) return EditorUserBuildSettings.development;
			return EditorUserBuildSettings.development == false;
		}
	}
}
