using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Vertx.Debugging.Graph;

namespace Vertx.Debugging.Editor
{
	internal class Plot : GraphView
	{
		public Plot()
		{
			this.AddManipulator(new ConstrainedContentZoomer
			{
				MinScale = 1,
				MaxScale = 10
			});
			this.AddManipulator(new ConstrainedContentDragger());

			Insert(0, dimensionsRoot = new VisualElement());
			dimensionsRoot.pickingMode = PickingMode.Ignore;
			dimensionsRoot.StretchToParentSize();

			EditorApplication.update += Update;
			
			RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
			RegisterCallback<DetachFromPanelEvent>(Detached);
		}

		private void Detached(DetachFromPanelEvent evt) => EditorApplication.update -= Update;

		private const int rate = 2;
		private int counter;
		private void Update()
		{
			if (counter++ % rate != 0)
				return;
			
			bool hasChanges = false;
			foreach (var plotEdge in plotEdges)
			{
				if (!plotEdge.ConsumeHasChanged) continue;
				plotEdge.MarkDirtyRepaint();
				hasChanges = true;
			}
			if(hasChanges)
				MarkDirtyRepaint();
			
			HasUpdatedEdges();
		}

		private void GeometryChangedCallback(GeometryChangedEvent evt) => HasUpdatedEdges();

		private readonly VisualElement dimensionsRoot;
		private readonly List<PlotEdge> plotEdges = new List<PlotEdge>();

		public void AddEdge(PlotEdge edge)
		{
			plotEdges.Add(edge);
			AddElement(edge);
			edge.StretchToParentSize();
			HasUpdatedEdges();
		}

		public void RemoveEdge(string label)
		{
			PlotEdge edge = this.Q<PlotEdge>(label);
			if (edge == null) return;
			plotEdges.Remove(edge);
			edge.RemoveFromHierarchy();
		}

		private (float min, float max) GetBounds(CircularBuffer<GraphedValue> points)
		{
			float min = Mathf.Infinity;
			float max = Mathf.NegativeInfinity;
			for (int i = 0; i < points.Size; i++)
			{
				float value = points[i].Value;
				if (value < min)
					min = value;
				if (value > max)
					max = value;
			}

			return (min, max);
		}

		private float GetNearestOrderOfMagnitudeBound(float value)
		{
			if (value == 0)
				return 1;
			return Mathf.Pow(10, Mathf.Floor(Mathf.Log10(value)));
		}

		private float CeilToNearest(float value, float round) => Mathf.Ceil(value / round) * round;
		private float FloorToNearest(float value, float round) => Mathf.Floor(value / round) * round;

		(float min, float max) GetTotalBounds()
		{
			float min = Mathf.Infinity;
			float max = Mathf.NegativeInfinity;
			foreach (PlotEdge edge in plotEdges)
			{
				(float minBounds, float maxBounds) = GetBounds(edge.Values);
				if (minBounds < min)
					min = minBounds;
				if (maxBounds > max)
					max = maxBounds;
			}

			return (min, max);
		}

		private (float min, float max) currentBounds;

		private void HasUpdatedEdges()
		{
			(float min, float max) newBounds = GetTotalBounds();
			if (currentBounds.min == newBounds.min && currentBounds.max == newBounds.max)
				return;
			
			currentBounds = newBounds;
			(float min, float max) = newBounds;

			float absMax = Mathf.Max(Mathf.Abs(min), max);
			float orderOfMagnitude = GetNearestOrderOfMagnitudeBound(absMax);
			float primaryOoM = orderOfMagnitude * 10;
			float secondaryOoM = primaryOoM / 5;
			min = FloorToNearest(min, orderOfMagnitude) - orderOfMagnitude;
			max = CeilToNearest(max, orderOfMagnitude) + orderOfMagnitude;
			Vector2 size = layout.size;

			contentViewContainer.style.width = size.x;
			contentViewContainer.style.height = size.y;

			foreach (PlotEdge edge in plotEdges)
				edge.SetDimensions(min, max, size);

			ITransform contentTransform = contentViewContainer.transform;
			Vector3 position = contentTransform.position;
			Vector3 scale = contentTransform.scale;

			int i = 0;
			for (float p = min; p <= max; p += orderOfMagnitude, i++)
			{
				VisualElement rule;
				if (i < dimensionsRoot.childCount)
					rule = dimensionsRoot[i];
				else
				{
					dimensionsRoot.Add(rule = new VisualElement());
					rule.pickingMode = PickingMode.Ignore;
					rule.AddToClassList("rule");
				}

				if (p == 0 || p % primaryOoM == 0)
				{
					rule.AddToClassList("rulePrimary");
				}
				else
				{
					rule.RemoveFromClassList("rulePrimary");
					if (p % secondaryOoM == 0)
					{
						rule.AddToClassList("ruleSecondary");
					}
					else
					{
						rule.RemoveFromClassList("ruleSecondary");
					}
				}

				rule.style.top = Mathf.InverseLerp(min, max, p) * size.y;
			}

			while (dimensionsRoot.childCount > i)
				dimensionsRoot.RemoveAt(dimensionsRoot.childCount - 1);
		}
	}
}