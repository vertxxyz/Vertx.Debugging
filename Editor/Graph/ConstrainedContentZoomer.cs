using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx.Debugging.Editor
{
	public class ConstrainedContentZoomer : Manipulator
	{
		/// <summary>
		///   <para>Reference zoom level.</para>
		/// </summary>
		public float ReferenceScale { get; set; } = ContentZoomer.DefaultReferenceScale;

		/// <summary>
		///   <para>Min zoom level.</para>
		/// </summary>
		public float MinScale { get; set; } = ContentZoomer.DefaultMinScale;

		/// <summary>
		///   <para>Max zoom level.</para>
		/// </summary>
		public float MaxScale { get; set; } = ContentZoomer.DefaultMaxScale;

		/// <summary>
		///   <para>Zoom step: percentage of variation between a zoom level and the next. For example, with a value of 0.15, which represents 15%, a zoom level of 200% will become 230% when zooming in.</para>
		/// </summary>
		public float ScaleStep { get; set; } = ContentZoomer.DefaultScaleStep;

		protected override void RegisterCallbacksOnTarget()
		{
			if (!(target is GraphView))
				throw new InvalidOperationException("Manipulator can only be added to a GraphView");
			target.RegisterCallback<WheelEvent>(OnWheel);
		}

		protected override void UnregisterCallbacksFromTarget() => target.UnregisterCallback<WheelEvent>(OnWheel);

		private static float CalculateNewZoom(
			float currentZoom,
			float wheelDelta,
			float zoomStep,
			float referenceZoom,
			float minZoom,
			float maxZoom)
		{
			if (minZoom <= 0.0)
			{
				Debug.LogError($"The minimum zoom ({minZoom}) must be greater than zero.");
				return currentZoom;
			}

			if (referenceZoom < (double)minZoom)
			{
				Debug.LogError($"The reference zoom ({referenceZoom}) must be greater than or equal to the minimum zoom ({minZoom}).");
				return currentZoom;
			}

			if (referenceZoom > (double)maxZoom)
			{
				Debug.LogError($"The reference zoom ({referenceZoom}) must be less than or equal to the maximum zoom ({maxZoom}).");
				return currentZoom;
			}

			if (zoomStep < 0.0)
			{
				Debug.LogError($"The zoom step ({zoomStep}) must be greater than or equal to zero.");
				return currentZoom;
			}

			currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
			if (Mathf.Approximately(wheelDelta, 0.0f))
				return currentZoom;
			double y = Math.Log(referenceZoom, 1.0 + zoomStep);
			double num1 = referenceZoom - Math.Pow(1.0 + zoomStep, y);
			double num2 = Math.Log(minZoom - num1, 1.0 + zoomStep) - y;
			double num3 = Math.Log(maxZoom - num1, 1.0 + zoomStep) - y;
			double num4 = Math.Log(currentZoom - num1, 1.0 + zoomStep) - y;
			wheelDelta = Math.Sign(wheelDelta);
			double a = num4 + wheelDelta;
			if (a > num3 - 0.5)
				return maxZoom;
			if (a < num2 + 0.5)
				return minZoom;
			double num5 = Math.Round(a);
			return (float)(Math.Pow(1.0 + zoomStep, num5 + y) + num1);
		}

		private void OnWheel(WheelEvent evt)
		{
			if (!(target is GraphView graphView) || (evt.target is VisualElement element ? element.panel : null).GetCapturingElement(PointerId.mousePointerId) != null)
				return;
			Vector3 position = graphView.viewTransform.position;
			Vector3 scale = graphView.viewTransform.scale;
			Vector2 vector2 = target.ChangeCoordinatesTo(graphView.contentViewContainer, evt.localMousePosition);
			float x = vector2.x + graphView.contentViewContainer.layout.x;
			float y = vector2.y + graphView.contentViewContainer.layout.y;
			Vector3 vector3 = position + Vector3.Scale(new Vector3(x, y, 0.0f), scale);
			float newZoom = CalculateNewZoom(scale.y, -evt.delta.y, ScaleStep, ReferenceScale, MinScale, MaxScale);
			scale.x = newZoom;
			scale.y = newZoom;
			scale.z = 1f;
			Vector3 newPosition = vector3 - Vector3.Scale(new Vector3(x, y, 0.0f), scale);
			newPosition.x = RoundToPixelGrid(newPosition.x);
			newPosition.y = RoundToPixelGrid(newPosition.y);
			
			// Constrain
			newPosition.x = Mathf.Min(0, newPosition.x);
			newPosition.y = Mathf.Min(0, newPosition.y);
			
			Vector2 size = graphView.contentViewContainer.layout.size;
			Vector2 scaledSize = size * scale;
			Vector2 maxPos = -(scaledSize - size);
			newPosition.x = Mathf.Max(newPosition.x, maxPos.x);
			newPosition.y = Mathf.Max(newPosition.y, maxPos.y);
			// ---------
			
			graphView.UpdateViewTransform(newPosition, scale);
			evt.StopPropagation();
		}

		private static float RoundToPixelGrid(float v) => Mathf.Floor((float)(v * (double)EditorGUIUtility.pixelsPerPoint + 0.479999989271164)) / EditorGUIUtility.pixelsPerPoint;
	}
}