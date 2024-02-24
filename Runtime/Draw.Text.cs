#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;
using static Vertx.Debugging.Shape;
using UnityEditor;
#if !UNITY_2022_1_OR_NEWER
using System.Reflection;
#endif
using UnityEngine.Pool;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment

namespace Vertx.Debugging
{
	[InitializeOnLoad]
	internal static class DrawText
	{
		internal static GUIStyle TextStyle => s_textStyle ?? (s_textStyle = new GUIStyle(EditorStyles.label) { font = AssetsUtility.JetBrainsMono });

		private static Type GameViewType => s_gameViewType ?? (s_gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));

		private static EditorWindow GameView
		{
			get
			{
				if (s_gameView != null)
					return s_gameView;
				Object[] gameViewQuery = Resources.FindObjectsOfTypeAll(GameViewType);
				if (gameViewQuery == null || gameViewQuery.Length == 0)
					return null;
				return s_gameView = (EditorWindow)gameViewQuery[0];
			}
		}

		private static readonly GUIContent s_sharedContent = new();
		private static GUIStyle s_textStyle;
		private static Type s_gameViewType;
		private static EditorWindow s_gameView;

		static DrawText()
		{
			SceneView.duringSceneGui -= SceneViewGUI;
			SceneView.duringSceneGui += SceneViewGUI;
		}

		private static void SceneViewGUI(SceneView obj)
		{
			if (!obj.drawGizmos)
				return;

			EventType eventType = Event.current.type;
			if (eventType != EventType.Repaint && eventType != EventType.MouseDown)
				return;

			Handles.BeginGUI();
			DoOnGUI(View.Scene);
			Handles.EndGUI();
		}

		public static void OnGUI()
		{
			if (Event.current.type != EventType.Repaint)
				return;

			DoOnGUI(View.Game);
		}

		private static void DoOnGUI(View view)
		{
			var commandBuilder = CommandBuilder.Instance;
			Draw3DText(commandBuilder);

			if ((view & View.Scene) == 0)
				DrawScreenTexts(commandBuilder, view);
		}

		private static void Draw3DText(CommandBuilder commandBuilder)
		{
			if (Event.current.type != EventType.Repaint)
				return;
			if (commandBuilder.DefaultTexts.Count == 0 && commandBuilder.GizmoTexts.Count == 0)
				return;

			bool uses3DIcons = Uses3DIcons;
			float size = IconSize;

			using (ListPool<TextData>.Get(out List<TextData> text3D))
			{
				Gather3DText(commandBuilder.DefaultTexts, text3D);
				Gather3DText(commandBuilder.GizmoTexts, text3D);

				// 3D text is collected and sorted by distance before being displayed.
				if (text3D.Count > 0)
				{
					text3D.Sort((a, b) => b.Distance.CompareTo(a.Distance));

					foreach (TextData textData in text3D)
					{
						//------DRAW-------
						Color backgroundColor = textData.BackgroundColor;
						Color textColor = textData.TextColor;
						backgroundColor.a *= textData.Alpha;
						textColor.a *= textData.Alpha;
						Camera camera = SceneView.currentDrawingSceneView?.camera ?? textData.Camera;
						
						GUIContent content = GetGUIContentFromObject(textData.Value);
						var rect = new Rect(textData.ScreenPosition, TextStyle.CalcSize(content));
						if(!camera.pixelRect.Overlaps(rect))
							continue;
						DrawAtScreenPosition(rect, content, backgroundColor, textColor, null);
					}
				}
			}

			void Gather3DText(CommandBuilder.TextDataLists list, List<TextData> text3D)
			{
				for (var i = 0; i < list.Count; i++)
				{
					TextData textData = list.Elements[i];
					Camera camera = SceneView.currentDrawingSceneView?.camera ?? textData.Camera;
					if (camera == null) continue;
					if (!WorldToGUIPoint(textData.Position, out Vector2 screenPos, out float distance, camera)) continue;
					float alpha;
					if (uses3DIcons)
					{
						float iconSize = size * 1000;
						alpha = 1 - math.unlerp(iconSize * 0.75f, iconSize, distance);
						if (alpha <= 0)
							continue;
					}
					else
					{
						alpha = 1;
					}

					textData.ScreenPosition = screenPos;
					textData.Distance = distance;
					textData.Alpha = alpha;
					text3D.Add(textData);
				}
			}
		}

		private static void DrawScreenTexts(CommandBuilder commandBuilder, View view)
		{
			int height = Screen.height;
			var position = new Vector2(10, 10);
			bool isNotGameView = (view & View.Game) == 0;
			DrawScreenText(commandBuilder.DefaultScreenTexts);
			DrawScreenText(commandBuilder.GizmoScreenTexts);
			return;

			void DrawScreenText(CommandBuilder.ScreenTextDataLists list)
			{
				for (var i = 0; i < list.Count; i++)
				{
					if (position.y > height)
						return;
					ScreenTextData textData = list.Elements[i];
					if ((textData.ActiveViews & view) == 0) continue;
					GUIContent content = GetGUIContentFromObject(textData.Value);
					var rect = new Rect(position, TextStyle.CalcSize(content));
					DrawAtScreenPosition(rect, content, textData.BackgroundColor, textData.TextColor, isNotGameView ? textData.Context : null);
					position.y = rect.yMax + 1;
				}
			}
		}

#if !UNITY_2022_1_OR_NEWER
		private static Func<bool> s_use3dGizmos;
		private static Func<float> s_iconSize;
#endif

		private static bool Uses3DIcons
		{
			get
			{
#if UNITY_2022_1_OR_NEWER
				return GizmoUtility.use3dIcons;
#else
				if (s_use3dGizmos == null)
					s_use3dGizmos = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), Type.GetType("UnityEditor.AnnotationUtility,UnityEditor").GetProperty("use3dGizmos", BindingFlags.Static | BindingFlags.NonPublic).GetMethod);
				return s_use3dGizmos();
#endif
			}
		}

		private static float IconSize
		{
			get
			{
#if UNITY_2022_1_OR_NEWER
				return GizmoUtility.iconSize;
#else
				if (s_iconSize == null)
					s_iconSize = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), Type.GetType("UnityEditor.AnnotationUtility,UnityEditor").GetProperty("iconSize", BindingFlags.Static | BindingFlags.NonPublic).GetMethod);
				return s_iconSize();
#endif
			}
		}


		internal static void DrawAtScreenPosition(Rect rect, GUIContent content, Color backgroundColor, Color textColor, Object context)
		{
			bool hasContext = context != null;
			if (hasContext && rect.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.MouseDown)
					EditorGUIUtility.PingObject(context);
				textColor = Color.white;
			}
			else
			{
				EditorGUI.DrawRect(rect, GUI.color * backgroundColor);
			}

			TextStyle.normal.textColor = textColor;
			GUI.Label(rect, content, TextStyle);
		}

		private static string GetContentFromObject(object text)
		{
			switch (text)
			{
				case Vector3 vector3:
					return vector3.ToString("F3");
				case Vector2 vector2:
					return vector2.ToString("F3");
				default:
					return text.ToString();
			}
		}

		internal static GUIContent GetGUIContentFromObject(object text)
		{
			s_sharedContent.text = GetContentFromObject(text);
			return s_sharedContent;
		}

		/// <summary>
		/// Converts world point to a point in screen space if valid.
		/// </summary>
		/// <param name="query">The world point query</param>
		/// <param name="point">The GUI point. Zero if behind the camera.</param>
		/// <param name="distance">The distance to the query from the camera.</param>
		/// <param name="camera">The camera that owns the GUI space.</param>
		/// <returns>True if a valid position in front of the camera.</returns>
		private static bool WorldToGUIPoint(Vector3 query, out Vector2 point, out float distance, Camera camera)
		{
			Vector3 viewPos = camera.WorldToViewportPoint(query);
			distance = viewPos.z;
			if (distance < 0)
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