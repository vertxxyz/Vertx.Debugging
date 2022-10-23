#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx.Debugging
{
	internal class DebuggingSettingsProvider : SettingsProvider
	{
		private SerializedObject _serializedObject;

		private DebuggingSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
			: base(path, scopes, keywords) { }

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			DebuggingSettings settings = DebuggingSettings.instance;
			settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
			settings.Save();
			_serializedObject = new SerializedObject(settings);
		}

		public override void OnGUI(string searchContext)
		{
			using (CreateSettingsWindowGUIScope())
			{
				GUI.enabled = true;
				_serializedObject.Update();
				EditorGUI.BeginChangeCheck();

				SerializedProperty property = _serializedObject.GetIterator();
				bool enter = true;
				while (property.NextVisible(enter))
				{
					enter = false;
					if(property.name == "m_Script") continue;
					EditorGUILayout.PropertyField(property, true);
				}

				if (EditorGUI.EndChangeCheck())
				{
					_serializedObject.ApplyModifiedProperties();
					DebuggingSettings.instance.Save();
				}
			}
		}

		[SettingsProvider]
		public static SettingsProvider CreateProvider()
			=> new DebuggingSettingsProvider("Project/Vertx/Debugging", SettingsScope.Project);

		private static IDisposable CreateSettingsWindowGUIScope()
		{
			var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
			var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
			return Activator.CreateInstance(type) as IDisposable;
		}
	}
}
#endif