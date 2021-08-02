using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Vertx.Debugging.Editor
{
	internal class DebugGraphWindow : EditorWindow
	{
		[MenuItem("Window/Analysis/Debug Graph")]
		private static void Open()
		{
			DebugGraphWindow window = GetWindow<DebugGraphWindow>();
			window.titleContent = new GUIContent("Debug Graph");
			window.SetAntiAliasing(4);
			window.Show();
		}

		private void CreateGUI()
		{
			var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.vertx.debugging/Editor/Graph/Debugging.uss");
			var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.vertx.debugging/Editor/Graph/DebugGraphWindow.uxml");

			treeAsset.CloneTree(rootVisualElement);
			rootVisualElement.styleSheets.Add(styleSheet);

			// Queries
			toolbarMenu = rootVisualElement.Q<ToolbarButton>("GroupsDropdown");
			groupsRoot = rootVisualElement.Q<VisualElement>("GroupsRoot", "groupsRoot");
			contentRoot = rootVisualElement.Q<VisualElement>("GraphRoot");


			string toolbarMenuText = toolbarMenu.text;
			toolbarMenu.text = string.Empty;
			var textElement = new TextElement { text = toolbarMenuText };
			textElement.AddToClassList(ToolbarMenu.textUssClassName);
			textElement.pickingMode = PickingMode.Ignore;
			toolbarMenu.Add(textElement);

			var arrowElement = new VisualElement();
			arrowElement.AddToClassList(ToolbarMenu.arrowUssClassName);
			arrowElement.pickingMode = PickingMode.Ignore;
			toolbarMenu.Add(arrowElement);

#if UNITY_2020_1_OR_NEWER
			// Help Box
			helpBox = new HelpBox("No Active Items", HelpBoxMessageType.Warning);
			groupsRoot.Insert(1, helpBox);
			helpBox.visible = false;
			helpBox.style.display = DisplayStyle.None;
#endif
			plot = new Plot();
			contentRoot.Add(plot);
			plot.StretchToParentSize();
			if (DebugUtils.GraphValueRegistry is GraphValueRegistry graphValueRegistry)
			{
				foreach (var value in graphValueRegistry.GatherEdges())
					OnNewGraphValueGroup(value);
				graphValueRegistry.OnNewGraphValueGroup += OnNewGraphValueGroup;
			}

			toolbarMenu.clickable = new Clickable(GroupsMenu);

			UpdateActiveGroups();
		}
		
		private void GroupsMenu()
		{
			GroupsDropdown dropdown = new GroupsDropdown(new AdvancedDropdownState(), plot);
			Rect rect = toolbarMenu.layout;
			rect.y = rect.yMax;
			rect.width = 240;
			dropdown.Show(rect);
		}

		private void OnNewGraphValueGroup((string label, GraphValueGroup group) value)
		{
			(string label, GraphValueGroup group) = value;
			RegisterEdge(new PlotEdge(label, group));
		}

		internal void RegisterEdge(PlotEdge edge)
		{
			plot.AddEdge(edge);
		}

		private int groupCount = 0;
		private Plot plot;
		private ToolbarButton toolbarMenu;
		private VisualElement groupsRoot, contentRoot;
#if UNITY_2020_1_OR_NEWER
		private HelpBox helpBox;
#endif

		void UpdateActiveGroups()
		{
			if (groupCount == 0)
			{
#if UNITY_2020_1_OR_NEWER
				helpBox.visible = true;
				helpBox.style.display = DisplayStyle.Flex;
#endif
			}
			else
			{
#if UNITY_2020_1_OR_NEWER
				helpBox.visible = false;
				helpBox.style.display = DisplayStyle.None;
#endif
			}
		}
	}
}