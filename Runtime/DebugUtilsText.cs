using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using System;
using System.Reflection;
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
			if (guiUpdate != null)
				SceneView.duringSceneGui -= guiUpdate;
			subscribedFixed = false;
			debugTextFixed.Clear();
			if (guiFixed != null)
				SceneView.duringSceneGui -= guiFixed;
		}

		private readonly struct DebugText
		{
			public Vector3 Position { get; }
			public object Text { get; }
			public Color Color { get; }
			public Camera Camera { get; }

			public DebugText(Vector3 position, object text, Color color, Camera camera)
			{
				Camera = camera;
				Position = position;
				Text = text;
				Color = color;
			}
		}

		private static Font font;

		private static Font Font
		{
			get
			{
				if (font != null)
					return font;
				return font = AssetDatabase.LoadAssetAtPath<Font>("Packages/com.vertx.debugging/Editor/JetbrainsMono-Regular.ttf");
			}
		}

		private static GUIStyle textStyle;
		private static GUIStyle TextStyle => textStyle ?? (textStyle = new GUIStyle(EditorStyles.label)
		{
			font = Font
		});

		private static bool subscribedUpdate;
		private static readonly List<DebugText> debugTextUpdate = new List<DebugText>();

		private static bool subscribedFixed;
		private static readonly List<DebugText> debugTextFixed = new List<DebugText>();

		private static Type gameViewType;
		private static Type GameViewType => gameViewType ?? (gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));

		private static EditorWindow gameView;

		private static EditorWindow GameView
		{
			get
			{
				if (gameView != null)
					return gameView;
				Object[] gameViewQuery = Resources.FindObjectsOfTypeAll(GameViewType);
				if (gameViewQuery == null || gameViewQuery.Length == 0)
					return null;
				return gameView = (EditorWindow)gameViewQuery[0];
			}
		}

		private static FieldInfo hasGizmos;
		private static FieldInfo HasGizmos => hasGizmos ?? (hasGizmos = GameViewType.GetField("m_Gizmos", BindingFlags.NonPublic | BindingFlags.Instance));

		public static bool GameViewGizmosEnabled
		{
			get
			{
				var gameView = GameView;
				if (gameView == null)
					return false;
				var hasGizmos = HasGizmos;
				if (hasGizmos == null)
					return false;
				return (bool)hasGizmos.GetValue(gameView);
			}
		}

#endif

		/// <summary>
		/// Draws text at a position in space.
		/// </summary>
		/// <param name="position">The position to render the text at</param>
		/// <param name="text">The object that is converted to text.</param>
		/// <param name="camera">A camera for rendering in the game view when gizmos are active. Game view rendering is optional.</param>
		[Conditional("UNITY_EDITOR")]
		public static void DrawText(Vector3 position, object text, Camera camera = null) => DrawText(position, text, new Color(0.22f, 0.22f, 0.22f), camera);

		/// <summary>
		/// Draws text at a position in space.
		/// </summary>
		/// <param name="position">The position to render the text at</param>
		/// <param name="text">The object that is converted to text.</param>
		/// <param name="color">The color of the label's background.</param>
		/// <param name="camera">A camera for rendering in the game view when gizmos are active. Game view rendering is optional.</param>
		[Conditional("UNITY_EDITOR")]
		public static void DrawText(Vector3 position, object text, Color color, Camera camera = null)
		{
#if UNITY_EDITOR

			if (!Application.isPlaying) return;

			var runtimeObject = DebugUtilsRuntimeObject.Instance;
			var debugText = new DebugText(position, text, color, camera);

			if (Time.deltaTime == Time.fixedDeltaTime)
			{
				if (!subscribedFixed)
				{
					subscribedFixed = true;
					if (guiFixed == null)
						guiFixed = SceneViewGUIFixed;
					SceneView.duringSceneGui += guiFixed;
					if (waitForNextFixed == null)
						waitForNextFixed = WaitForNextFixed;
					runtimeObject.RegisterFixedUpdateAction(waitForNextFixed);
					RegisterGUI();
				}

				debugTextFixed.Add(debugText);
			}
			else
			{
				if (!subscribedUpdate)
				{
					subscribedUpdate = true;
					if (guiUpdate == null)
						guiUpdate = SceneViewGUIUpdate;
					SceneView.duringSceneGui += guiUpdate;
					if (waitForNextUpdate == null)
						waitForNextUpdate = WaitForNextUpdate;
					runtimeObject.RegisterUpdateAction(waitForNextUpdate);
					RegisterGUI();
				}

				debugTextUpdate.Add(debugText);
			}

			void RegisterGUI()
			{
				runtimeObject.RegisterOnGUIAction(() =>
				{
					if (!GameViewGizmosEnabled) return;
					foreach (DebugText t in debugTextUpdate)
					{
						if (t.Camera == null) continue;
						DoDrawText(t.Position, t.Text, t.Color, t.Camera);
					}

					foreach (DebugText t in debugTextFixed)
					{
						if (t.Camera == null) continue;
						DoDrawText(t.Position, t.Text, t.Color, t.Camera);
					}
				});
			}

#endif
		}

#if UNITY_EDITOR
		private static Action<SceneView> guiUpdate, guiFixed;
		private static Action waitForNextUpdate, waitForNextFixed;

		static void WaitForNextUpdate()
		{
			subscribedUpdate = false;
			debugTextUpdate.Clear();
			SceneView.duringSceneGui -= guiUpdate;
		}

		static void WaitForNextFixed()
		{
			subscribedFixed = false;
			debugTextFixed.Clear();
			SceneView.duringSceneGui -= guiFixed;
		}

		private static void SceneViewGUIUpdate(SceneView obj) => SceneViewGUI(obj, debugTextUpdate);

		private static void SceneViewGUIFixed(SceneView obj) => SceneViewGUI(obj, debugTextFixed);

		private static void SceneViewGUI(SceneView obj, List<DebugText> debugTexts)
		{
			if (!Application.isPlaying)
			{
				if (guiFixed != null)
					SceneView.duringSceneGui -= guiFixed;
				if (guiUpdate != null)
					SceneView.duringSceneGui -= guiUpdate;
				return;
			}

			if (!obj.drawGizmos)
				return;

			Handles.BeginGUI();

			foreach (DebugText debugText in debugTexts)
				DoDrawText(debugText.Position, debugText.Text, debugText.Color, obj.camera);

			Handles.EndGUI();
		}

		private static GUIContent sharedContent = new GUIContent();

		private static void DoDrawText(Vector3 position, object text, Color color, Camera camera)
		{
			if (Event.current.type != EventType.Repaint)
				return;
			
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
			
			sharedContent.text = value;
			Rect rect = new Rect(screenPos, TextStyle.CalcSize(sharedContent));
			DrawGUIRect();
			GUI.Label(rect, sharedContent, TextStyle);
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
#endif
	}
}