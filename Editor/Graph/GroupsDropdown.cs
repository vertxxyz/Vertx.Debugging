using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx.Debugging.Editor
{
	internal class GroupsDropdown : AdvancedDropdown
	{
		private readonly Plot plot;

		public GroupsDropdown(AdvancedDropdownState state, Plot plot) : base(state) => this.plot = plot;

		protected override AdvancedDropdownItem BuildRoot()
		{
			AdvancedDropdownItem root = new AdvancedDropdownItem("Groups");

			if (!(DebugUtils.GraphValueRegistry is GraphValueRegistry graphValueRegistry))
				return root;
			foreach ((string label, GraphValueGroup group) in graphValueRegistry.GatherEdges())
			{
				root.AddChild(new AdvancedDropdownItem(label) { });
			}

			return root;
		}

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			if (!(DebugUtils.GraphValueRegistry is GraphValueRegistry graphValueRegistry))
				return;

			string label = item.name;
			if (!graphValueRegistry.TryGetGroup(label, out GraphValueGroup group))
			{
				Debug.LogError("Value not present in registry.");
				return;
			}

			if (group.Visible)
			{
				PlotEdge edge = plot.Q<PlotEdge>(label);
				edge?.RemoveFromHierarchy();
				group.Visible = false;
			}
			else
			{
				plot.AddEdge(new PlotEdge(label, group));
				group.Visible = true;
			}
		}
	}
}