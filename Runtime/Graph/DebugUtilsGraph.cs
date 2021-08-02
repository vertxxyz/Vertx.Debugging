using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Vertx.Debugging.Graph;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		public static IGraphValueRegistry GraphValueRegistry { set; get; }

		[Conditional("UNITY_EDITOR")]
		public static void Graph(string label, float value)
			=> Graph(label, value, Color.white);

		[Conditional("UNITY_EDITOR")]
		public static void Graph(string label, float value, Color color)
		{
			GraphedValueContext context;
			double time;
			if (Application.isPlaying)
			{
				if (Time.deltaTime == Time.fixedDeltaTime)
				{
					context = GraphedValueContext.Fixed;
					time = Time.fixedTimeAsDouble;
				}
				else
				{
					context = GraphedValueContext.Variable;
					time = Time.unscaledTimeAsDouble;
				}
			}
			else
			{
				context = GraphedValueContext.Variable;
				time = EditorApplication.timeSinceStartup;
			}

			GraphValueRegistry?.RegisterValue(label, color, new GraphedValue(value, time), context);
		}
	}
}