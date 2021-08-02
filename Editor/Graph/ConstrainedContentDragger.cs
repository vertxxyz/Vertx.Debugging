using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx.Debugging.Editor
{
	public class ConstrainedContentDragger : MouseManipulator
	{
		private Vector2 start;
		private bool active;

		public ConstrainedContentDragger()
		{
			active = false;
			activators.Add(new ManipulatorActivationFilter
			{
				button = MouseButton.LeftMouse,
				modifiers = EventModifiers.Alt
			});
			activators.Add(new ManipulatorActivationFilter
			{
				button = MouseButton.MiddleMouse
			});
		}

		protected override void RegisterCallbacksOnTarget()
		{
			if (!(target is GraphView))
				throw new InvalidOperationException("Manipulator can only be added to a GraphView");
			target.RegisterCallback<MouseDownEvent>(OnMouseDown);
			target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
			target.RegisterCallback<MouseUpEvent>(OnMouseUp);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
			target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
			target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
		}

		protected void OnMouseDown(MouseDownEvent e)
		{
			if (active)
			{
				e.StopImmediatePropagation();
			}
			else
			{
				if (!CanStartManipulation(e) || !(target is GraphView target2))
					return;
				start = target2.ChangeCoordinatesTo(target2.contentViewContainer, e.localMousePosition);
				active = true;
				target.CaptureMouse();
				e.StopImmediatePropagation();
			}
		}

		protected void OnMouseMove(MouseMoveEvent e)
		{
			if (!active || !(this.target is GraphView target))
				return;
			Vector2 vector2 = target.ChangeCoordinatesTo(target.contentViewContainer, e.localMousePosition) - start;
			Vector3 scale = target.contentViewContainer.transform.scale;
			Vector3 position = target.viewTransform.position;
			position += Vector3.Scale(vector2, scale);

			// Constrain
			position.x = Mathf.Min(0, position.x);
			position.y = Mathf.Min(0, position.y);

			Vector2 size = target.contentViewContainer.layout.size;
			Vector2 scaledSize = size * scale;
			Vector2 maxPos = -(scaledSize - size);
			position.x = Mathf.Max(position.x, maxPos.x);
			position.y = Mathf.Max(position.y, maxPos.y);
			// ---------

			target.viewTransform.position = position;
			e.StopPropagation();
		}

		protected void OnMouseUp(MouseUpEvent e)
		{
			if (!active || !CanStopManipulation(e) || !(this.target is GraphView target))
				return;
			Vector3 position = target.contentViewContainer.transform.position;
			Vector3 scale = target.contentViewContainer.transform.scale;
			target.UpdateViewTransform(position, scale);
			active = false;
			this.target.ReleaseMouse();
			e.StopPropagation();
		}
	}
}