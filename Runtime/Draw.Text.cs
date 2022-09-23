#if UNITY_EDITOR
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using static Vertx.Debugging.Shapes;
using UnityEditor;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment

namespace Vertx.Debugging
{
	[InitializeOnLoad]
	internal static class DrawText
	{
		private static Font Font
		{
			get
			{
				if (s_Font != null)
					return s_Font;
				return s_Font = AssetDatabase.LoadAssetAtPath<Font>("Packages/com.vertx.debugging/Editor/JetbrainsMono-Regular.ttf");
			}
		}

		private static GUIStyle TextStyle => textStyle ?? (textStyle = new GUIStyle(EditorStyles.label) { font = Font });

		private static Type GameViewType => s_GameViewType ?? (s_GameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));

		private static EditorWindow GameView
		{
			get
			{
				if (s_GameView != null)
					return s_GameView;
				Object[] gameViewQuery = Resources.FindObjectsOfTypeAll(GameViewType);
				if (gameViewQuery == null || gameViewQuery.Length == 0)
					return null;
				return s_GameView = (EditorWindow)gameViewQuery[0];
			}
		}

		private static readonly GUIContent s_SharedContent = new GUIContent();
		private static Font s_Font;
		private static GUIStyle textStyle;
		private static Type s_GameViewType;
		private static EditorWindow s_GameView;

		static DrawText()
		{
			SceneView.duringSceneGui -= SceneViewGUI;
			SceneView.duringSceneGui += SceneViewGUI;
		}

		private static void SceneViewGUI(SceneView obj)
		{
			if (!obj.drawGizmos)
				return;

			if (Event.current.type != EventType.Repaint)
				return;

			Handles.BeginGUI();
			var commandBuilder = CommandBuilder.Instance;
			var texts = commandBuilder.Texts;
			for (int i = 0; i < texts.Count; i++)
			{
				TextData textData = texts.InternalList[i];
				DoDrawText(textData.Position, textData.Value, texts.ColorsInternalList[i], obj.camera);
			}

			Handles.EndGUI();
		}

		public static void OnGUI()
		{
			if (Event.current.type != EventType.Repaint)
				return;

			var commandBuilder = CommandBuilder.Instance;
			var texts = commandBuilder.Texts;
			for (int i = 0; i < texts.Count; i++)
			{
				TextData textData = texts.InternalList[i];
				DoDrawText(textData.Position, textData.Value, texts.ColorsInternalList[i], textData.Camera);
			}
		}

		/// <summary>
		/// Only call in EventType.Repaint!
		/// </summary>
		private static void DoDrawText(Vector3 position, object text, Color color, Camera camera)
		{
			if (!WorldToGUIPoint(position, out Vector2 screenPos, camera)) return;
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

			s_SharedContent.text = value;
			Rect rect = new Rect(screenPos, TextStyle.CalcSize(s_SharedContent));
			DrawGUIRect();
			GUI.Label(rect, s_SharedContent, TextStyle);
			//-----------------

			void DrawGUIRect()
			{
				Color color1 = GUI.color;
				GUI.color *= color;
				GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
				GUI.color = color1;
			}
		}

		/// <summary>
		/// Converts world point to a point in screen space if valid.
		/// </summary>
		/// <param name="query">The world point query</param>
		/// <param name="point">The GUI point. Zero if behind the camera.</param>
		/// <param name="camera">The camera that owns the GUI space.</param>
		/// <returns>True if a valid position in front of the camera.</returns>
		private static bool WorldToGUIPoint(Vector3 query, out Vector2 point, Camera camera)
		{
			Vector3 viewPos = camera.WorldToViewportPoint(query);
			bool behindScreen = viewPos.z < 0;

			if (behindScreen)
			{
				point = Vector2.zero;
				return false;
			}

			Vector2 viewScreenVector = new Vector2(viewPos.x, 1 - viewPos.y);
			viewScreenVector /= EditorGUIUtility.pixelsPerPoint;
			point = new Vector2(viewScreenVector.x * camera.pixelWidth, viewScreenVector.y * camera.pixelHeight);
			return true;
		}
	}
}
#endif