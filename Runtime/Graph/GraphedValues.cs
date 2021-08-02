using UnityEngine;

namespace Vertx.Debugging.Graph
{
	public interface IGraphValueRegistry
	{
		bool RegisterValue(string label, Color32 color, GraphedValue value, GraphedValueContext context);
	}

	public readonly struct GraphedValue
	{
		public readonly float Value;
		public readonly double Time;

		public GraphedValue(float value, double time)
		{
			Value = value;
			Time = time;
		}
	}

	public enum GraphedValueContext
	{
		Fixed,
		Variable
	}
}