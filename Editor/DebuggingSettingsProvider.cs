#if UNITY_EDITOR
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
	internal class DebuggingSettingsProvider : SettingsProvider
	{
		private SerializedObject _serializedObject;
		private SerializedProperty _colors, _depthWrite, _depthTest;
		private readonly GUIContent _useDefaults = new GUIContent("Use Defaults");
		private GUIStyle _frameBox;
		private GUIStyle FrameBox => _frameBox ?? (_frameBox = "FrameBox");

		private DebuggingSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
			: base(path, scopes, keywords) { }

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			DebuggingSettings settings = DebuggingSettings.instance;
			settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
			settings.Save();
			_serializedObject = new SerializedObject(settings);
			_colors = _serializedObject.FindProperty(nameof(DebuggingSettings.Colors));
			_depthWrite = _serializedObject.FindProperty(nameof(DebuggingSettings.DepthWrite));
			_depthTest = _serializedObject.FindProperty(nameof(DebuggingSettings.DepthTest));
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

				using (new GUILayout.VerticalScope(FrameBox))
				{
					GUILayout.Label("Depth", EditorStyles.miniBoldLabel);

					EditorGUILayout.PropertyField(_depthWrite);
					if (((DebuggingSettings.Location)_depthWrite.intValue & DebuggingSettings.Location.GameView) != 0)
						EditorGUILayout.HelpBox("Under specific versions of some render pipelines you may find depth writing causes artifacts in the game view against other gizmos.", MessageType.Warning);
					EditorGUILayout.PropertyField(_depthTest);
					if (((DebuggingSettings.Location)_depthTest.intValue & DebuggingSettings.Location.GameView) != 0)
						EditorGUILayout.HelpBox("Under specific versions of some render pipelines you may find depth testing is resolved upside-down in the game view.\nSome do not depth test properly at all.", MessageType.Warning);
					if ((DebuggingSettings.Location)_depthTest.intValue != DebuggingSettings.Location.None)
					{
						EditorGUILayout.HelpBox("Depth testing may fail to work when Post Processing is enabled.", MessageType.Info);
					}
				}

				if (EditorGUI.EndChangeCheck())
				{
					_serializedObject.ApplyModifiedProperties();
					DebuggingSettings.instance.Save();
					Shape.SyncColors();
				}
			}
		}

		private void RevertColors()
		{
			DebuggingSettings settings = DebuggingSettings.instance;
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