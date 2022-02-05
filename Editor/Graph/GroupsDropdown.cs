using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Vertx.Debugging.Editor
{
	internal class GroupsDropdown : AdvancedDropdown
	{
		private readonly DebugGraphWindow graphWindow;

		public GroupsDropdown(AdvancedDropdownState state, DebugGraphWindow graphWindow) : base(state) => this.graphWindow = graphWindow;

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
			
			group.Visible = !group.Visible;
			graphWindow.OnGroupChanged((label, group));
		}
	}
}