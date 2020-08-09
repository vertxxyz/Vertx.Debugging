using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetStatics()
		{
			subscribedUpdate = false;
			debugTextUpdate.Clear();
			SceneView.duringSceneGui -= SceneViewGUIUpdate;
			subscribedFixed = false;
			debugTextFixed.Clear();
			SceneView.duringSceneGui -= SceneViewGUIFixed;
		}

		private readonly struct DebugText
		{
			public Vector3 Position { get; }
			public object Text { get; }
			public Color Color { get; }

			public DebugText(Vector3 position, object text, Color color)
			{
				Position = position;
				Text = text;
				Color = color;
			}
		}

		private static bool subscribedUpdate;
		private static readonly List<DebugText> debugTextUpdate = new List<DebugText>();

		private static bool subscribedFixed;
		private static readonly List<DebugText> debugTextFixed = new List<DebugText>();

		private static GUIStyle boxStyle;
		private static GUIStyle BoxStyle => boxStyle ?? (boxStyle = new GUIStyle(EditorStyles.boldLabel));

		#endif

		[Conditional("UNITY_EDITOR")]
		public static void DrawText(Vector3 position, object text) => DrawText(position, text, new Color(0.22f, 0.22f, 0.22f));

		[Conditional("UNITY_EDITOR")]
		public static void DrawText(Vector3 position, object text, Color color)
		{
			#if UNITY_EDITOR

			if (!Application.isPlaying) return;

			if (Time.deltaTime == Time.fixedDeltaTime)
			{
				if (!subscribedFixed)
				{
					subscribedFixed = true;
					SceneView.duringSceneGui += SceneViewGUIFixed;
					DebugUtilsRuntimeObject.Instance.RegisterFixedUpdateAction(WaitForNextFixed);
				}

				debugTextFixed.Add(new DebugText(position, text, color));
			}
			else
			{
				if (!subscribedUpdate)
				{
					subscribedUpdate = true;
					SceneView.duringSceneGui += SceneViewGUIUpdate;
					DebugUtilsRuntimeObject.Instance.RegisterUpdateAction(WaitForNextUpdate);
				}

				debugTextUpdate.Add(new DebugText(position, text, color));
			}

			#endif
		}

		#if UNITY_EDITOR
		static void WaitForNextUpdate()
		{
			subscribedUpdate = false;
			debugTextUpdate.Clear();
			SceneView.duringSceneGui -= SceneViewGUIUpdate;
		}

		static void WaitForNextFixed()
		{
			subscribedFixed = false;
			debugTextFixed.Clear();
			SceneView.duringSceneGui -= SceneViewGUIFixed;
		}

		private static void SceneViewGUIUpdate(SceneView obj) => SceneViewGUI(obj, debugTextUpdate);

		private static void SceneViewGUIFixed(SceneView obj) => SceneViewGUI(obj, debugTextFixed);

		private static void SceneViewGUI(SceneView obj, List<DebugText> debugTexts)
		{
			if (!Application.isPlaying)
			{
				SceneView.duringSceneGui -= SceneViewGUIFixed;
				SceneView.duringSceneGui -= SceneViewGUIUpdate;
				return;
			}

			Handles.BeginGUI();

			foreach (DebugText debugText in debugTexts)
				DrawText(debugText.Position, debugText.Text, debugText.Color);

			void DrawText(Vector3 position, object text, Color color)
			{
				if (!WorldToGUIPoint(position, out Vector2 screenPos)) return;
				//------DRAW-------
				string value;
				switch (text)
				{
					case Vector3 vector3:
						value = vector3.ToString("F3");
						break;
					case Vector2 vector2:
						value = vector2.ToString("F3");
						break;
					default:
						value = text.ToString();
						break;
				}

				var content = new GUIContent(value);
				Rect rect = new Rect(screenPos, BoxStyle.CalcSize(content));
				EditorGUI.DrawRect(rect, color);
				GUI.Label(rect, content, EditorStyles.boldLabel);
				//-----------------
			}

			Handles.EndGUI();
		}

		/// <summary>
		/// Converts world point to a point in screen space if valid.
		/// </summary>
		/// <param name="query">The world point query</param>
		/// <param name="point">The GUI point. Zero if behind the camera.</param>
		/// <returns>True if a valid position in front of the camera.</returns>
		private static bool WorldToGUIPoint(Vector3 query, out Vector2 point)
		{
			Camera cam = SceneView.currentDrawingSceneView.camera;
			Vector3 viewPos = cam.WorldToViewportPoint(query);
			bool behindScreen = viewPos.z < 0;

			if (behindScreen)
			{
				point = Vector2.zero;
				return false;
			}

			Vector2 viewScreenVector = new Vector2(viewPos.x, viewPos.y);
			point = new Vector2(viewScreenVector.x * cam.pixelWidth, (1 - viewScreenVector.y) * cam.pixelHeight);
			return true;
		}
		#endif
	}
}