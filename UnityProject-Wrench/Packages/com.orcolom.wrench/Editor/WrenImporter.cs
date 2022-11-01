using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Wrench.Editor
{
	[ScriptedImporter(2, "wren")]
	public class WrenImporter : ScriptedImporter
	{
		private static readonly StringBuilder Sb = new StringBuilder();

		public override void OnImportAsset(AssetImportContext ctx)
		{
			var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.orcolom.wrench/Editor/wren.png");
			var text = File.ReadAllText(ctx.assetPath);
			string scriptName = Path.GetFileName(ctx.assetPath);

			var asset = ScriptableObject.CreateInstance<WrenScript>();
			asset.name = scriptName;
			asset.Text = text;
			ctx.AddObjectToAsset(scriptName, asset, texture);
			ctx.SetMainObject(asset);

			Sb.Clear();
			int count = 0;
			var vm = Vm.New();

			vm.SetErrorListener((_, type, _, line, message) =>
			{
				if (type != ErrorType.CompileError) return;
				Sb.AppendLine($"line {line}: {message}");
				count++;
			});

			vm.Interpret(ctx.assetPath, text);

			if (Sb.Length == 0) return;

			var errors = new TextAsset(Sb.ToString());
			ctx.AddObjectToAsset($"{scriptName}_CompileLogs", errors);
			ctx.LogImportError($"{ctx.assetPath} has {count} compile errors\n{errors}", asset);
		}
	}
}
