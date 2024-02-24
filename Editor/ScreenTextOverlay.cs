using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Vertx.Debugging
{
	[Overlay(typeof(SceneView), Id, null
#if UNITY_2022_1_OR_NEWER
		, defaultLayout = Layout.Panel, defaultDockZone = DockZone.LeftColumn
#endif
	)]
	[Icon("d_UnityEditor.ConsoleWindow")]
	// If Unity wanted me to use UIToolkit for new things maybe they would make it easier to make this Overlay in a stateful way. I tried.
	internal sealed class ScreenTextOverlay : IMGUIOverlay, ITransientOverlay
	{
		private const string Id = "vertx-debugging-overlay";
		private Vector2 _position;
		private static readonly Vector2 s_overlayBounds = new Vector2(5, 18);
		private static readonly Vector2 s_minSize = new Vector2(120, 50);
		private static readonly Vector2 s_maxSize = new Vector2(500, 900);
		private readonly List<(Rect, string)> _layout = new List<(Rect, string)>();
		private Rect _bounds;

		public bool visible
		{
			get
			{
				var commandBuilder = CommandBuilder.Instance;
				return commandBuilder.DefaultScreenTexts.Elements.Any(e => (e.ActiveViews & Shape.View.Scene) != 0)
				       || commandBuilder.GizmoScreenTexts.Elements.Any(e => (e.ActiveViews & Shape.View.Scene) != 0);
			}
		}

		public ScreenTextOverlay()
		{
#if UNITY_2022_2_OR_NEWER
			size = new Vector2(180, 300);
			minSize = s_minSize;
			maxSize = s_maxSize;
#endif
		}


		protected override Layout supportedLayouts => Layout.Panel;

		public override void OnGUI()
		{
			var commandBuilder = CommandBuilder.Instance;

			// Overlays seem to hate size settings. It barely functions.
			float maxHeight = Screen.height * 0.75f;
			
			if (Event.current.type == EventType.Layout)
			{
				_layout.Clear();
				_bounds = GetScreenTextLayout(commandBuilder.DefaultScreenTexts, _layout, Rect.zero);
				_bounds = GetScreenTextLayout(commandBuilder.GizmoScreenTexts, _layout, _bounds);

#if UNITY_2022_2_OR_NEWER
				if (size.y > maxHeight)
					size = new Vector2(Mathf.Min(_bounds.width + s_overlayBounds.x, s_maxSize.x), Mathf.Min(_bounds.height + s_overlayBounds.y, maxHeight));
				if (size.y < s_minSize.y)
					size = new Vector2(Mathf.Max(_bounds.width + s_overlayBounds.x, s_minSize.x), Mathf.Max(_bounds.height + s_overlayBounds.y, s_minSize.y));
#endif
				// If I don't perform any GUILayout then mouse click events are never sent. Hooray!
				GUILayoutUtility.GetRect(Mathf.Clamp(_bounds.width, s_minSize.x, s_maxSize.x) - s_overlayBounds.x + 6, Mathf.Clamp(_bounds.height, s_minSize.y, maxHeight) - s_overlayBounds.y);
			}
			else if (Event.current.type != EventType.Layout)
			{
				using (var scope = new GUI.ScrollViewScope(
#if UNITY_2022_2_OR_NEWER
					       new Rect(0, 0, size.x - s_overlayBounds.x, size.y - s_overlayBounds.y),
#else
					       new Rect(0, 0, Mathf.Clamp(_bounds.width, s_minSize.x, s_maxSize.x) - s_overlayBounds.x + 6, Mathf.Clamp(_bounds.height, s_minSize.y, maxHeight) - s_overlayBounds.y),
#endif
					       _position, _bounds)
				      )
				{
					Color temp = GUI.color;
					GUI.color = Color.white;
					int c = DrawScreenTextLayout(commandBuilder.DefaultScreenTexts, _layout);
					DrawScreenTextLayout(commandBuilder.GizmoScreenTexts, _layout, c);
					_position = scope.scrollPosition;
					GUI.color = temp;
				}
			}
		}

		private static readonly GUIContent s_sharedContent = new GUIContent();
		

		private static Rect GetScreenTextLayout(CommandBuilder.ScreenTextDataLists list, List<(Rect, string)> layout, Rect bounds)
		{
			for (var i = 0; i < list.Count; i++)
			{
				Shape.ScreenTextData textData = list.Elements[i];
				if ((textData.ActiveViews & Shape.View.Scene) == 0) continue;
				GUIContent content = DrawText.GetGUIContentFromObject(textData.Value);
				Vector2 size = DrawText.TextStyle.CalcSize(content);
				layout.Add((new Rect(0, bounds.yMax, size.x, size.y), content.text));
				bounds.height += size.y + 1;
				bounds.width = Mathf.Max(bounds.width, size.x);
			}

			return bounds;
		}

		private static int DrawScreenTextLayout(CommandBuilder.ScreenTextDataLists list, List<(Rect rect, string text)> layout, int startIndex = 0)
		{
			int c = startIndex;
			for (var i = 0; i < list.Count; i++)
			{
				Shape.ScreenTextData textData = list.Elements[i];
				if ((textData.ActiveViews & Shape.View.Scene) == 0) continue;
				(Rect rect, string text) = layout[c++];
				s_sharedContent.text = text;
				DrawText.DrawAtScreenPosition(rect, s_sharedContent, textData.BackgroundColor, textData.TextColor, textData.Context);
			}

			return c;
		}
	}
}