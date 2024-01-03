#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
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
		private SerializedProperty 
			_depthWrite,
			_depthTest,
			_allocatedLines,
			_allocatedDashedLines,
			_allocatedArcs,
			_allocatedBoxes,
			_allocatedOutlines,
			_allocatedCasts;
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
			_depthWrite = _serializedObject.FindProperty(nameof(DebuggingSettings.DepthWrite));
			_depthTest = _serializedObject.FindProperty(nameof(DebuggingSettings.DepthTest));
			_allocatedLines = _serializedObject.FindProperty(nameof(DebuggingSettings.AllocatedLines));
			_allocatedDashedLines = _serializedObject.FindProperty(nameof(DebuggingSettings.AllocatedDashedLines));
			_allocatedArcs = _serializedObject.FindProperty(nameof(DebuggingSettings.AllocatedArcs));
			_allocatedBoxes = _serializedObject.FindProperty(nameof(DebuggingSettings.AllocatedBoxes));
			_allocatedOutlines = _serializedObject.FindProperty(nameof(DebuggingSettings.AllocatedOutlines));
			_allocatedCasts = _serializedObject.FindProperty(nameof(DebuggingSettings.AllocatedCasts));
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
				
				using (new GUILayout.VerticalScope(FrameBox))
				{
					GUILayout.Label("Allocated shapes", EditorStyles.miniBoldLabel);

					EditorGUILayout.HelpBox("The number of a type of shape that can be drawn by the debugging system. Increase a value to draw more of a shape, or decrease it to reduce memory overhead.\nNo allocations are made in builds.", MessageType.Info);

					int alternate = 0;
					long bytes = DrawAllocation(_allocatedLines, UnsafeUtility.SizeOf<LineGroup>(), ref alternate);
					bytes += DrawAllocation(_allocatedDashedLines, UnsafeUtility.SizeOf<DashedLineGroup>(), ref alternate);
					bytes += DrawAllocation(_allocatedArcs, UnsafeUtility.SizeOf<ArcGroup>(), ref alternate);
					bytes += DrawAllocation(_allocatedBoxes, UnsafeUtility.SizeOf<BoxGroup>(), ref alternate);
					bytes += DrawAllocation(_allocatedOutlines, UnsafeUtility.SizeOf<OutlineGroup>(), ref alternate);
					bytes += DrawAllocation(_allocatedCasts, UnsafeUtility.SizeOf<CastGroup>(), ref alternate);
					EditorGUILayout.LabelField($"Total: {FormatFileSize(bytes)}", EditorStyles.boldLabel);
					
					if (GUILayout.Button(_useDefaults, GUILayout.Width(120)))
					{
						RevertAllocations();
						GUI.changed = true;
					}
				}

				if (EditorGUI.EndChangeCheck())
				{
					_serializedObject.ApplyModifiedProperties();
					DebuggingSettings.instance.Save();
				}
			}
		}
		
		private void RevertAllocations()
		{
			DebuggingSettings settings = DebuggingSettings.instance;
			settings.AllocatedLines = Constants.AllocatedLines;
			settings.AllocatedDashedLines = Constants.AllocatedDashedLines;
			settings.AllocatedArcs = Constants.AllocatedArcs;
			settings.AllocatedBoxes = Constants.AllocatedBoxes;
			settings.AllocatedOutlines = Constants.AllocatedOutlines;
			settings.AllocatedCasts = Constants.AllocatedCasts;
			_serializedObject.Update();
		}

		private long DrawAllocation(SerializedProperty property, int allocationSize, ref int alternate)
		{
			Rect rect = EditorGUILayout.GetControlRect(true, (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2);
			if (++alternate % 2 == 0)
			{
				Rect bg = rect;
				bg.y -= 1;
				bg.width += 6;
				bg.x -= 3;
				EditorGUI.DrawRect(bg, new Color(1, 1, 1, 0.1f));
			}

			rect.height = EditorGUIUtility.singleLineHeight;
			
			EditorGUI.PropertyField(rect, property);
			rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			long bytes = property.intValue * allocationSize;
			EditorGUI.LabelField(rect, $"Size: {FormatFileSize(bytes)}");
			return bytes;
		}
		
		private static string FormatFileSize(long bytes)
		{
			const int unit = 1024;
			if (bytes < unit) { return $"{bytes} B"; }

			var exp = (int)(Math.Log(bytes) / Math.Log(unit));
			return $"{bytes / Math.Pow(unit, exp):F2} {"KMGTPE"[exp - 1]}B";
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