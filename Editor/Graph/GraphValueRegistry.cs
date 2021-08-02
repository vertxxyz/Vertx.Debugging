using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Vertx.Debugging.Graph;

namespace Vertx.Debugging.Editor
{
	internal class GraphValueRegistry : IGraphValueRegistry
	{
		[InitializeOnLoadMethod]
		private static void Initialise()
		{
			DebugUtils.GraphValueRegistry = new GraphValueRegistry();
			sampleCount = EditorPrefs.GetInt(sampleCountPrefId, defaultSampleCount);
		}

		private readonly Dictionary<string, GraphValueGroup> lookup = new Dictionary<string, GraphValueGroup>();
		private const string sampleCountPrefId = "Vertx.Debugging.Graph.SampleCount";
		private const int defaultSampleCount = 512;
		private static int sampleCount;

		public event Action<(string, GraphValueGroup)> OnNewGraphValueGroup;

		public bool TryGetGroup(string label, out GraphValueGroup result) => lookup.TryGetValue(label, out result);

		public bool RegisterValue(string label, Color32 color, GraphedValue value, GraphedValueContext context)
		{
			if (!lookup.TryGetValue(label, out var group))
			{
				lookup.Add(label, group = new GraphValueGroup(context, sampleCount));
				OnNewGraphValueGroup?.Invoke((label, group));
			}
			else
			{
				if (context != group.Context)
				{
					Debug.LogWarning("Value is registered from an inconsistent context.\n" +
					                 $"Please call {nameof(DebugUtils.Graph)} from a consistent context. Ie. Update, FixedUpdate, or another consistently called context.");
					return false;
				}
			}

			group.Color = color;
			return group.RegisterValue(value);
		}

		public IEnumerable<(string, GraphValueGroup)> GatherEdges() => 
			lookup.Select(value => (value.Key, value.Value));
	}

	internal class GraphValueGroup
	{
		public readonly GraphedValueContext Context;
		public bool Visible = true;
		public CircularBuffer<GraphedValue> Values;
		public Color32 Color;
		public bool HasChanged = true;

		public GraphValueGroup(GraphedValueContext context, int length)
		{
			Context = context;
			Values = new CircularBuffer<GraphedValue>(length);
		}

		public bool RegisterValue(GraphedValue value)
		{
			Values.PushFront(value);
			HasChanged = true;
			return true;
		}
	}
}