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
	internal sealed class DebuggingSettingsProvider : SettingsProvider
	{
		private SerializedObject _serializedObject;
		private SerializedProperty
			_depthWrite,
			_depthTest;
		private AllocationsSerializedProperties _allocationsForGizmos, _allocationsWithDurations;

		private class AllocationsSerializedProperties
		{
			private readonly GUIContent _name;
			private readonly SerializedProperty _lines, _dashedLines, _arcs, _boxes, _outlines, _casts;

			public AllocationsSerializedProperties(GUIContent name, SerializedProperty property)
			{
				_name = name;
				_lines = property.FindPropertyRelative(nameof(DebuggingSettings.Allocations.Lines));
				_dashedLines = property.FindPropertyRelative(nameof(DebuggingSettings.Allocations.DashedLines));
				_arcs = property.FindPropertyRelative(nameof(DebuggingSettings.Allocations.Arcs));
				_boxes = property.FindPropertyRelative(nameof(DebuggingSettings.Allocations.Boxes));
				_outlines = property.FindPropertyRelative(nameof(DebuggingSettings.Allocations.Outlines));
				_casts = property.FindPropertyRelative(nameof(DebuggingSettings.Allocations.Casts));
			}

			public long Draw(int additionalBytesPerElement)
			{
				_lines.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_lines.isExpanded, _name);
				bool draw = _lines.isExpanded;

				int alternate = 0;
				long bytes = 0;
				EditorGUI.indentLevel++;
				bytes += DrawAllocation(_lines, UnsafeUtility.SizeOf<LineGroup>() + additionalBytesPerElement, ref alternate, draw);
				bytes += DrawAllocation(_dashedLines, UnsafeUtility.SizeOf<DashedLineGroup>() + additionalBytesPerElement, ref alternate, draw);
				bytes += DrawAllocation(_arcs, UnsafeUtility.SizeOf<ArcGroup>() + additionalBytesPerElement, ref alternate, draw);
				bytes += DrawAllocation(_boxes, UnsafeUtility.SizeOf<BoxGroup>() + additionalBytesPerElement, ref alternate, draw);
				bytes += DrawAllocation(_outlines, UnsafeUtility.SizeOf<OutlineGroup>() + additionalBytesPerElement, ref alternate, draw);
				bytes += DrawAllocation(_casts, UnsafeUtility.SizeOf<CastGroup>() + additionalBytesPerElement, ref alternate, draw);
				EditorGUI.indentLevel--;
				EditorGUILayout.EndFoldoutHeaderGroup();

				return bytes;
			}

			private static long DrawAllocation(SerializedProperty property, int allocationSize, ref int alternate, bool draw)
			{
				long bytes = property.intValue * allocationSize;
				if (!draw)
					return bytes;

				Rect rect = EditorGUILayout.GetControlRect(true, (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2);
				if (++alternate % 2 == 0)
				{
					Rect bg = rect;
					bg.y -= 1;
					bg.width += 6;
					bg.x -= 3;
					EditorGUI.DrawRect(bg, new Color(1, 1, 1, 0.05f));
				}

				rect.height = EditorGUIUtility.singleLineHeight;

				EditorGUI.PropertyField(rect, property);
				rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.LabelField(rect, $"Size: {FormatFileSize(bytes)}");
				return bytes;
			}
		}

		private readonly GUIContent _useDefaults = new GUIContent("Use Defaults");
		private static GUIStyle _frameBox;
		private static GUIStyle FrameBox => _frameBox ?? (_frameBox = "FrameBox");

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
			_allocationsForGizmos = new AllocationsSerializedProperties(
				new GUIContent("Allocations for Gizmos", "Drawn instantaneously from OnDrawGizmos, OnDrawGizmosSelected, or DrawGizmoAttribute calls."),
				_serializedObject.FindProperty(nameof(DebuggingSettings.AllocationsForGizmos))
			);
			_allocationsWithDurations = new AllocationsSerializedProperties(
				new GUIContent("Allocations with durations", "Drawn in all other cases with an optional duration parameter."),
				_serializedObject.FindProperty(nameof(DebuggingSettings.AllocationsWithDurations))
			);
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

					long bytes = _allocationsForGizmos.Draw(0);
					bytes += _allocationsWithDurations.Draw(sizeof(float));
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
			Reset(settings.AllocationsForGizmos);
			Reset(settings.AllocationsWithDurations);

			_serializedObject.Update();
			return;

			void Reset(DebuggingSettings.Allocations allocations)
			{
				allocations.Lines = Constants.AllocatedLines;
				allocations.DashedLines = Constants.AllocatedDashedLines;
				allocations.Arcs = Constants.AllocatedArcs;
				allocations.Boxes = Constants.AllocatedBoxes;
				allocations.Outlines = Constants.AllocatedOutlines;
				allocations.Casts = Constants.AllocatedCasts;
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

		public static string FormatFileSize(long bytes)
		{
			const int unit = 1024;
			if (bytes < unit)
			{
				return $"{bytes} B";
			}

			var exp = (int)(Math.Log(bytes) / Math.Log(unit));
			return $"{bytes / Math.Pow(unit, exp):F2} {"KMGTPE"[exp - 1]}B";
		}
	}
}