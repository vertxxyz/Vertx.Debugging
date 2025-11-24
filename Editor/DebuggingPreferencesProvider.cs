using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace Vertx.Debugging
{
	internal sealed class DebuggingPreferencesProvider : SettingsProvider
	{
		private SerializedObject _serializedObject;
		private SerializedProperty _colors;
		private readonly GUIContent _useDefaults = new GUIContent("Use Defaults");
		private GUIStyle _frameBox;
		private GUIStyle FrameBox => _frameBox ?? (_frameBox = "FrameBox");

		private DebuggingPreferencesProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
			: base(path, scopes, keywords) { }

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			DebuggingPreferences settings = DebuggingPreferences.instance;
			settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
			settings.Save();
			_serializedObject = new SerializedObject(settings);
			_colors = _serializedObject.FindProperty(nameof(DebuggingPreferences.Colors));
		}

		public override void OnGUI(string searchContext)
		{
			using (CreateSettingsWindowGUIScope())
			{
				GUI.enabled = true;
				_serializedObject.Update();
				EditorGUI.BeginChangeCheck();

				using (new GUILayout.VerticalScope(FrameBox))
				{
					GUILayout.Label("Colors", EditorStyles.miniBoldLabel);

					SerializedProperty prop = _colors.Copy();
					SerializedProperty end = prop.GetEndProperty();
					bool enterChildren = true;
					while (prop.NextVisible(enterChildren) && !SerializedProperty.EqualContents(prop, end))
					{
						EditorGUILayout.PropertyField(prop);
						enterChildren = false;
					}

					if (GUILayout.Button(_useDefaults, GUILayout.Width(120)))
					{
						RevertColors();
						GUI.changed = true;
					}
				}

				if (EditorGUI.EndChangeCheck())
				{
					_serializedObject.ApplyModifiedProperties();
					DebuggingPreferences.instance.Save();
					Shape.SyncColors();
				}
			}
		}

		private void RevertColors()
		{
			DebuggingPreferences settings = DebuggingPreferences.instance;
			settings.Colors.HitColor = Constants.HitColor;
			settings.Colors.CastColor = Constants.CastColor;
			settings.Colors.EnterColor = Constants.EnterColor;
			settings.Colors.StayColor = Constants.StayColor;
			settings.Colors.ExitColor = Constants.ExitColor;
			settings.Colors.XColor = Constants.XColor;
			settings.Colors.YColor = Constants.YColor;
			settings.Colors.ZColor = Constants.ZColor;
			_serializedObject.Update();
		}

		[SettingsProvider]
		public static SettingsProvider CreateProvider()
			=> new DebuggingPreferencesProvider("Preferences/Vertx/Debugging", SettingsScope.User);

		private static IDisposable CreateSettingsWindowGUIScope()
		{
			var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
			var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
			return Activator.CreateInstance(type) as IDisposable;
		}
	}
}