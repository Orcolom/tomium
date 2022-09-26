using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Wrench
{
	public class WrenScript : ScriptableObject
	{
		[SerializeField]
		private string _text;

		public string Text
		{
			internal set => _text = value;
			get => _text;
		}
		
		[CustomEditor(typeof(WrenScript))]
		private class Inspector : Editor
		{
			public override VisualElement CreateInspectorGUI()
			{
				return new TextField
				{
					multiline = true,
					bindingPath = "_text",
					isReadOnly = true,
				};
			}
		}
	}
}
