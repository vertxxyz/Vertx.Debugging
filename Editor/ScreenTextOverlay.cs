#if UNITY_2021_1_OR_NEWER
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
		private Vector2 position;
		private static readonly Vector2 OverlayBounds = new Vector2(5, 18);
		private static readonly Vector2 _minSize = new Vector2(120, 50);
		private static readonly Vector2 _maxSize = new Vector2(500, 900);
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
#if UNITY_2022_1_OR_NEWER
			size = new Vector2(180, 300);
			minSize = _minSize;
			maxSize = _maxSize;
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

#if UNITY_2022_1_OR_NEWER
				if (size.y > maxHeight)
					size = new Vector2(Mathf.Min(_bounds.width + OverlayBounds.x, _maxSize.x), Mathf.Min(_bounds.height + OverlayBounds.y, maxHeight));
				if (size.y < _minSize.y)
					size = new Vector2(Mathf.Max(_bounds.width + OverlayBounds.x, _minSize.x), Mathf.Max(_bounds.height + OverlayBounds.y, _minSize.y));
#endif
				// If I don't perform any GUILayout then mouse click events are never sent. Hooray!
				GUILayoutUtility.GetRect(Mathf.Clamp(_bounds.width, _minSize.x, _maxSize.x) - OverlayBounds.x + 6, Mathf.Clamp(_bounds.height, _minSize.y, maxHeight) - OverlayBounds.y);
			}
			else if (Event.current.type != EventType.Layout)
			{
				using (var scope = new GUI.ScrollViewScope(
#if UNITY_2022_1_OR_NEWER
					       new Rect(0, 0, size.x - OverlayBounds.x, size.y - OverlayBounds.y),
#else
					       new Rect(0, 0, Mathf.Clamp(_bounds.width, _minSize.x, _maxSize.x) - OverlayBounds.x + 6, Mathf.Clamp(_bounds.height, _minSize.y, maxHeight) - OverlayBounds.y),
#endif
					       position, _bounds)
				      )
				{
					Color temp = GUI.color;
					GUI.color = Color.white;
					int c = DrawScreenTextLayout(commandBuilder.DefaultScreenTexts, _layout);
					DrawScreenTextLayout(commandBuilder.GizmoScreenTexts, _layout, c);
					position = scope.scrollPosition;
					GUI.color = temp;
				}
			}
		}

		private static readonly GUIContent s_SharedContent = new GUIContent();
		

		private static Rect GetScreenTextLayout(CommandBuilder.ScreenTextDataLists list, List<(Rect, string)> layout, Rect bounds)
		{
			for (int i = 0; i < list.Count; i++)
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
			for (int i = 0; i < list.Count; i++)
			{
				Shape.ScreenTextData textData = list.Elements[i];
				if ((textData.ActiveViews & Shape.View.Scene) == 0) continue;
				(Rect rect, string text) = layout[c++];
				s_SharedContent.text = text;
				DrawText.DrawAtScreenPosition(rect, s_SharedContent, textData.BackgroundColor, textData.TextColor, textData.Context);
			}

			return c;
		}
	}
}
#endif