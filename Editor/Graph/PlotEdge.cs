using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Vertx.Debugging.Graph;

namespace Vertx.Debugging.Editor
{
	internal class PlotEdge : GraphElement
	{
		public const float EdgeWidth = 2;

		public bool ConsumeHasChanged
		{
			get
			{
				if (!group.HasChanged)
					return false;
				group.HasChanged = false;
				return true;
			}
		}
		private readonly GraphValueGroup group;
		private readonly Color32 color;
		public CircularBuffer<GraphedValue> Values { get; private set; }
		
		public PlotEdge(string label, GraphValueGroup group)
		{
			this.group = group;
			Values = group.Values;
			color = group.Color;
			generateVisualContent += OnGenerateVisualContent;
			pickingMode = PickingMode.Ignore;
			AddToClassList("edge");
			name = label;
		}

		private void OnGenerateVisualContent(MeshGenerationContext mgc)
		{
			uint size = (uint)Values.Size;
			int capacity = Values.Capacity;
			if (size <= 1)
				return;

			uint wantedLength = size * 2;
			uint indexCount = (wantedLength - 2) * 3;

			MeshWriteData md = mgc.Allocate((int)wantedLength, (int)indexCount);
			if (md.vertexCount == 0)
				return;

			float vEnd = (float)capacity - 1;
			float width = layout.width;
			float height = layout.height;
			float halfEdgeWidth = EdgeWidth * 0.5f;
			
			var start = (int)(size - 1);
			float lastPoint = Values[start].Value;
			float thisPoint = lastPoint;
			float step = 1 / vEnd * width;
			for (int i = start; i >= 0; i--)
			{
				float v = 1 - (i / vEnd);

				float point = thisPoint;
				float nextPoint = i > 0 ? Values[i - 1].Value : Values[i].Value;
				float avg = ((point - lastPoint) + (nextPoint - point)) / 2f;
				Vector2 dir = new Vector2(avg, step);
				lastPoint = point;
				thisPoint = nextPoint;
				dir.Normalize();
				dir *= halfEdgeWidth;


				point = Mathf.InverseLerp(min, max, point);
				Vector2 pos = new Vector2(v * width, height - point * height);
				Vector2 uv = new Vector2(v, 0);
				md.SetNextVertex(new Vertex { position = new Vector3(pos.x - dir.x, pos.y - dir.y, Vertex.nearZ), uv = uv, tint = color });
				md.SetNextVertex(new Vertex { position = new Vector3(pos.x + dir.x, pos.y + dir.y, Vertex.nearZ), uv = uv, tint = color });
			}

			// Fill triangle indices as it is a triangle strip
			for (uint i = 0; i < wantedLength - 2; ++i)
			{
				if ((i & 0x01) == 0)
				{
					md.SetNextIndex((ushort)i);
					md.SetNextIndex((ushort)(i + 2));
					md.SetNextIndex((ushort)(i + 1));
				}
				else
				{
					md.SetNextIndex((ushort)i);
					md.SetNextIndex((ushort)(i + 1));
					md.SetNextIndex((ushort)(i + 2));
				}
			}
		}

		private float min, max;

		public void SetDimensions(float min, float max, Vector2 size)
		{
			this.min = min;
			this.max = max;
			style.width = size.x;
			style.height = size.y;
			MarkDirtyRepaint();
		}
	}
}