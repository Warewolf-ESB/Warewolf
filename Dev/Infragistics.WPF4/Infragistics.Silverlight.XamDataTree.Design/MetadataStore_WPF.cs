using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Menus.XamDataTree.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Menus.XamDataTree.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Menus.XamDataTreeNodeDataContext);
				Assembly controlAssembly = t.Assembly;

				#region SettingsBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.SettingsBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SettingsBase Properties

				#region NodeLayout Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodeLayout");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Tree",
					new DescriptionAttribute(SR.GetString("NodeLayout_Tree_Property")),
				    new DisplayNameAttribute("Tree")				);


				tableBuilder.AddCustomAttributes(t, "TargetTypeName",
					new DescriptionAttribute(SR.GetString("NodeLayout_TargetTypeName_Property")),
				    new DisplayNameAttribute("TargetTypeName")				);


				tableBuilder.AddCustomAttributes(t, "Visibility",
					new DescriptionAttribute(SR.GetString("NodeLayout_Visibility_Property")),
				    new DisplayNameAttribute("Visibility")				);


				tableBuilder.AddCustomAttributes(t, "DisplayMemberPath",
					new DescriptionAttribute(SR.GetString("NodeLayout_DisplayMemberPath_Property")),
				    new DisplayNameAttribute("DisplayMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "DisplayMemberPathResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_DisplayMemberPathResolved_Property")),
				    new DisplayNameAttribute("DisplayMemberPathResolved")				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxMemberPath",
					new DescriptionAttribute(SR.GetString("NodeLayout_CheckBoxMemberPath_Property")),
				    new DisplayNameAttribute("CheckBoxMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxMemberPathResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_CheckBoxMemberPathResolved_Property")),
				    new DisplayNameAttribute("CheckBoxMemberPathResolved")				);


				tableBuilder.AddCustomAttributes(t, "NodeLayouts",
					new DescriptionAttribute(SR.GetString("NodeLayout_NodeLayouts_Property")),
				    new DisplayNameAttribute("NodeLayouts")				);


				tableBuilder.AddCustomAttributes(t, "Indentation",
					new DescriptionAttribute(SR.GetString("NodeLayout_Indentation_Property")),
				    new DisplayNameAttribute("Indentation")				);


				tableBuilder.AddCustomAttributes(t, "IndentationResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_IndentationResolved_Property")),
				    new DisplayNameAttribute("IndentationResolved")				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplate",
					new DescriptionAttribute(SR.GetString("NodeLayout_ItemTemplate_Property")),
				    new DisplayNameAttribute("ItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplateResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_ItemTemplateResolved_Property")),
				    new DisplayNameAttribute("ItemTemplateResolved")				);


				tableBuilder.AddCustomAttributes(t, "HeaderText",
					new DescriptionAttribute(SR.GetString("NodeLayout_HeaderText_Property")),
				    new DisplayNameAttribute("HeaderText")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTemplate",
					new DescriptionAttribute(SR.GetString("NodeLayout_HeaderTemplate_Property")),
				    new DisplayNameAttribute("HeaderTemplate")				);


				tableBuilder.AddCustomAttributes(t, "HeaderContentResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_HeaderContentResolved_Property")),
				    new DisplayNameAttribute("HeaderContentResolved")				);


				tableBuilder.AddCustomAttributes(t, "EditorTemplate",
					new DescriptionAttribute(SR.GetString("NodeLayout_EditorTemplate_Property")),
				    new DisplayNameAttribute("EditorTemplate")				);


				tableBuilder.AddCustomAttributes(t, "EditingSettings",
					new DescriptionAttribute(SR.GetString("NodeLayout_EditingSettings_Property")),
				    new DisplayNameAttribute("EditingSettings")				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxSettings",
					new DescriptionAttribute(SR.GetString("NodeLayout_CheckBoxSettings_Property")),
				    new DisplayNameAttribute("CheckBoxSettings")				);


				tableBuilder.AddCustomAttributes(t, "IsDraggable",
					new DescriptionAttribute(SR.GetString("NodeLayout_IsDraggable_Property")),
				    new DisplayNameAttribute("IsDraggable")				);


				tableBuilder.AddCustomAttributes(t, "IsDraggableResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_IsDraggableResolved_Property")),
				    new DisplayNameAttribute("IsDraggableResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsDropTarget",
					new DescriptionAttribute(SR.GetString("NodeLayout_IsDropTarget_Property")),
				    new DisplayNameAttribute("IsDropTarget")				);


				tableBuilder.AddCustomAttributes(t, "IsDropTargetResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_IsDropTargetResolved_Property")),
				    new DisplayNameAttribute("IsDropTargetResolved")				);


				tableBuilder.AddCustomAttributes(t, "NodeStyleResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_NodeStyleResolved_Property")),
				    new DisplayNameAttribute("NodeStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "NodeStyle",
					new DescriptionAttribute(SR.GetString("NodeLayout_NodeStyle_Property")),
				    new DisplayNameAttribute("NodeStyle")				);


				tableBuilder.AddCustomAttributes(t, "ExpandedIconTemplate",
					new DescriptionAttribute(SR.GetString("NodeLayout_ExpandedIconTemplate_Property")),
				    new DisplayNameAttribute("ExpandedIconTemplate")				);


				tableBuilder.AddCustomAttributes(t, "ExpandedIconTemplateResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_ExpandedIconTemplateResolved_Property")),
				    new DisplayNameAttribute("ExpandedIconTemplateResolved")				);


				tableBuilder.AddCustomAttributes(t, "CollapsedIconTemplate",
					new DescriptionAttribute(SR.GetString("NodeLayout_CollapsedIconTemplate_Property")),
				    new DisplayNameAttribute("CollapsedIconTemplate")				);


				tableBuilder.AddCustomAttributes(t, "CollapsedIconTemplateResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_CollapsedIconTemplateResolved_Property")),
				    new DisplayNameAttribute("CollapsedIconTemplateResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsExpandedMemberPath",
					new DescriptionAttribute(SR.GetString("NodeLayout_IsExpandedMemberPath_Property")),
				    new DisplayNameAttribute("IsExpandedMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "IsExpandedMemberPathResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_IsExpandedMemberPathResolved_Property")),
				    new DisplayNameAttribute("IsExpandedMemberPathResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsEnabledMemberPath",
					new DescriptionAttribute(SR.GetString("NodeLayout_IsEnabledMemberPath_Property")),
				    new DisplayNameAttribute("IsEnabledMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "IsEnabledMemberPathResolved",
					new DescriptionAttribute(SR.GetString("NodeLayout_IsEnabledMemberPathResolved_Property")),
				    new DisplayNameAttribute("IsEnabledMemberPathResolved")				);

				#endregion // NodeLayout Properties

				#region NodeLayoutBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodeLayoutBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Key",
					new DescriptionAttribute(SR.GetString("NodeLayoutBase_Key_Property")),
				    new DisplayNameAttribute("Key")				);

				#endregion // NodeLayoutBase Properties

				#region ActiveNodeIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.ActiveNodeIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActiveNodeIndicator Properties

				#region SettingsBaseOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.SettingsBaseOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SettingsBaseOverride Properties

				#region NodeLinePanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.NodeLinePanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Node",
					new DescriptionAttribute(SR.GetString("NodeLinePanel_Node_Property")),
				    new DisplayNameAttribute("Node")				);

				#endregion // NodeLinePanel Properties

				#region NodeLayoutEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodeLayoutEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NodeLayout",
					new DescriptionAttribute(SR.GetString("NodeLayoutEventArgs_NodeLayout_Property")),
				    new DisplayNameAttribute("NodeLayout")				);

				#endregion // NodeLayoutEventArgs Properties

				#region NodeEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodeEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Node",
					new DescriptionAttribute(SR.GetString("NodeEventArgs_Node_Property")),
				    new DisplayNameAttribute("Node")				);

				#endregion // NodeEventArgs Properties

				#region InitializeNodeEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.InitializeNodeEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // InitializeNodeEventArgs Properties

				#region CancellableNodeExpansionChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.CancellableNodeExpansionChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CancellableNodeExpansionChangedEventArgs Properties

				#region CancellableNodeEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.CancellableNodeEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Node",
					new DescriptionAttribute(SR.GetString("CancellableNodeEventArgs_Node_Property")),
				    new DisplayNameAttribute("Node")				);

				#endregion // CancellableNodeEventArgs Properties

				#region NodeExpansionChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodeExpansionChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NodeExpansionChangedEventArgs Properties

				#region NodeLayoutAssignedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodeLayoutAssignedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NodeLayout",
					new DescriptionAttribute(SR.GetString("NodeLayoutAssignedEventArgs_NodeLayout_Property")),
				    new DisplayNameAttribute("NodeLayout")				);


				tableBuilder.AddCustomAttributes(t, "Level",
					new DescriptionAttribute(SR.GetString("NodeLayoutAssignedEventArgs_Level_Property")),
				    new DisplayNameAttribute("Level")				);


				tableBuilder.AddCustomAttributes(t, "Key",
					new DescriptionAttribute(SR.GetString("NodeLayoutAssignedEventArgs_Key_Property")),
				    new DisplayNameAttribute("Key")				);


				tableBuilder.AddCustomAttributes(t, "DataType",
					new DescriptionAttribute(SR.GetString("NodeLayoutAssignedEventArgs_DataType_Property")),
				    new DisplayNameAttribute("DataType")				);

				#endregion // NodeLayoutAssignedEventArgs Properties

				#region ActiveNodeChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.ActiveNodeChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OriginalActiveTreeNode",
					new DescriptionAttribute(SR.GetString("ActiveNodeChangedEventArgs_OriginalActiveTreeNode_Property")),
				    new DisplayNameAttribute("OriginalActiveTreeNode")				);


				tableBuilder.AddCustomAttributes(t, "NewActiveTreeNode",
					new DescriptionAttribute(SR.GetString("ActiveNodeChangedEventArgs_NewActiveTreeNode_Property")),
				    new DisplayNameAttribute("NewActiveTreeNode")				);


				tableBuilder.AddCustomAttributes(t, "Cancel",
					new DescriptionAttribute(SR.GetString("ActiveNodeChangedEventArgs_Cancel_Property")),
				    new DisplayNameAttribute("Cancel")				);

				#endregion // ActiveNodeChangedEventArgs Properties

				#region BeginEditingNodeEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.BeginEditingNodeEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // BeginEditingNodeEventArgs Properties

				#region TreeEditingNodeEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.TreeEditingNodeEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Editor",
					new DescriptionAttribute(SR.GetString("TreeEditingNodeEventArgs_Editor_Property")),
				    new DisplayNameAttribute("Editor")				);

				#endregion // TreeEditingNodeEventArgs Properties

				#region TreeExitEditingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.TreeExitEditingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NewValue",
					new DescriptionAttribute(SR.GetString("TreeExitEditingEventArgs_NewValue_Property")),
				    new DisplayNameAttribute("NewValue")				);


				tableBuilder.AddCustomAttributes(t, "EditingCanceled",
					new DescriptionAttribute(SR.GetString("TreeExitEditingEventArgs_EditingCanceled_Property")),
				    new DisplayNameAttribute("EditingCanceled")				);


				tableBuilder.AddCustomAttributes(t, "Editor",
					new DescriptionAttribute(SR.GetString("TreeExitEditingEventArgs_Editor_Property")),
				    new DisplayNameAttribute("Editor")				);

				#endregion // TreeExitEditingEventArgs Properties

				#region NodeValidationErrorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodeValidationErrorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ValidationErrorEventArgs",
					new DescriptionAttribute(SR.GetString("NodeValidationErrorEventArgs_ValidationErrorEventArgs_Property")),
				    new DisplayNameAttribute("ValidationErrorEventArgs")				);


				tableBuilder.AddCustomAttributes(t, "Handled",
					new DescriptionAttribute(SR.GetString("NodeValidationErrorEventArgs_Handled_Property")),
				    new DisplayNameAttribute("Handled")				);

				#endregion // NodeValidationErrorEventArgs Properties

				#region XamDataTreeNodeDataContext Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamDataTreeNodeDataContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Data",
					new DescriptionAttribute(SR.GetString("XamDataTreeNodeDataContext_Data_Property")),
				    new DisplayNameAttribute("Data"),
					new CategoryAttribute(SR.GetString("XamDataTreeNodeDataContext_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Node",
					new DescriptionAttribute(SR.GetString("XamDataTreeNodeDataContext_Node_Property")),
				    new DisplayNameAttribute("Node"),
					new CategoryAttribute(SR.GetString("XamDataTreeNodeDataContext_Properties"))
				);

				#endregion // XamDataTreeNodeDataContext Properties

				#region XamDataTreeNodesManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamDataTreeNodesManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamDataTreeNodesManager Properties

				#region NodesManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodesManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Nodes",
					new DescriptionAttribute(SR.GetString("NodesManager_Nodes_Property")),
				    new DisplayNameAttribute("Nodes")				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("NodesManager_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource")				);


				tableBuilder.AddCustomAttributes(t, "ParentNode",
					new DescriptionAttribute(SR.GetString("NodesManager_ParentNode_Property")),
				    new DisplayNameAttribute("ParentNode")				);


				tableBuilder.AddCustomAttributes(t, "NodeLayout",
					new DescriptionAttribute(SR.GetString("NodesManager_NodeLayout_Property")),
				    new DisplayNameAttribute("NodeLayout")				);


				tableBuilder.AddCustomAttributes(t, "Level",
					new DescriptionAttribute(SR.GetString("NodesManager_Level_Property")),
				    new DisplayNameAttribute("Level")				);

				#endregion // NodesManager Properties

				#region XamDataTreeNodeControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamDataTreeNodeControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Node",
					new DescriptionAttribute(SR.GetString("XamDataTreeNodeControl_Node_Property")),
				    new DisplayNameAttribute("Node"),
					new CategoryAttribute(SR.GetString("XamDataTreeNodeControl_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxStyle",
					new DescriptionAttribute(SR.GetString("XamDataTreeNodeControl_CheckBoxStyle_Property")),
				    new DisplayNameAttribute("CheckBoxStyle"),
					new CategoryAttribute(SR.GetString("XamDataTreeNodeControl_Properties"))
				);

				#endregion // XamDataTreeNodeControl Properties

				#region XamDataTreeNode Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamDataTreeNode");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsExpanded",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsExpanded_Property")),
				    new DisplayNameAttribute("IsExpanded"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Data",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_Data_Property")),
				    new DisplayNameAttribute("Data"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Index",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_Index_Property")),
				    new DisplayNameAttribute("Index"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Manager",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_Manager_Property")),
				    new DisplayNameAttribute("Manager"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Control",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_Control_Property")),
				    new DisplayNameAttribute("Control"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsMouseOver",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsMouseOver_Property")),
				    new DisplayNameAttribute("IsMouseOver"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NodeLayout",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_NodeLayout_Property")),
				    new DisplayNameAttribute("NodeLayout"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasChildren",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_HasChildren_Property")),
				    new DisplayNameAttribute("HasChildren"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Nodes",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_Nodes_Property")),
				    new DisplayNameAttribute("Nodes"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsHeader",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsHeader_Property")),
				    new DisplayNameAttribute("IsHeader"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsChecked",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsChecked_Property")),
				    new DisplayNameAttribute("IsChecked"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_Style_Property")),
				    new DisplayNameAttribute("Style"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsActive",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsActive_Property")),
				    new DisplayNameAttribute("IsActive"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsEditing",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsEditing_Property")),
				    new DisplayNameAttribute("IsEditing"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDraggable",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsDraggable_Property")),
				    new DisplayNameAttribute("IsDraggable"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDropTarget",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsDropTarget_Property")),
				    new DisplayNameAttribute("IsDropTarget"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDraggableResolved",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsDraggableResolved_Property")),
				    new DisplayNameAttribute("IsDraggableResolved"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDropTargetResolved",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsDropTargetResolved_Property")),
				    new DisplayNameAttribute("IsDropTargetResolved"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ExpandedIconTemplate",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_ExpandedIconTemplate_Property")),
				    new DisplayNameAttribute("ExpandedIconTemplate"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CollapsedIconTemplate",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_CollapsedIconTemplate_Property")),
				    new DisplayNameAttribute("CollapsedIconTemplate"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ExpandedIconTemplateResolved",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_ExpandedIconTemplateResolved_Property")),
				    new DisplayNameAttribute("ExpandedIconTemplateResolved"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CollapsedIconTemplateResolved",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_CollapsedIconTemplateResolved_Property")),
				    new DisplayNameAttribute("CollapsedIconTemplateResolved"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsEnabled",
					new DescriptionAttribute(SR.GetString("XamDataTreeNode_IsEnabled_Property")),
				    new DisplayNameAttribute("IsEnabled"),
					new CategoryAttribute(SR.GetString("XamDataTreeNode_Properties"))
				);

				#endregion // XamDataTreeNode Properties

				#region XamDataTree Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamDataTree");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDataTreeAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDataTreeAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("XamDataTree_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxDepth",
					new DescriptionAttribute(SR.GetString("XamDataTree_MaxDepth_Property")),
				    new DisplayNameAttribute("MaxDepth"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "GlobalNodeLayouts",
					new DescriptionAttribute(SR.GetString("XamDataTree_GlobalNodeLayouts_Property")),
				    new DisplayNameAttribute("GlobalNodeLayouts"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NodeLayouts",
					new DescriptionAttribute(SR.GetString("XamDataTree_NodeLayouts_Property")),
				    new DisplayNameAttribute("NodeLayouts"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayMemberPath",
					new DescriptionAttribute(SR.GetString("XamDataTree_DisplayMemberPath_Property")),
				    new DisplayNameAttribute("DisplayMemberPath"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxMemberPath",
					new DescriptionAttribute(SR.GetString("XamDataTree_CheckBoxMemberPath_Property")),
				    new DisplayNameAttribute("CheckBoxMemberPath"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsEnabledMemberPath",
					new DescriptionAttribute(SR.GetString("XamDataTree_IsEnabledMemberPath_Property")),
				    new DisplayNameAttribute("IsEnabledMemberPath"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsExpandedMemberPath",
					new DescriptionAttribute(SR.GetString("XamDataTree_IsExpandedMemberPath_Property")),
				    new DisplayNameAttribute("IsExpandedMemberPath"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Nodes",
					new DescriptionAttribute(SR.GetString("XamDataTree_Nodes_Property")),
				    new DisplayNameAttribute("Nodes"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Indentation",
					new DescriptionAttribute(SR.GetString("XamDataTree_Indentation_Property")),
				    new DisplayNameAttribute("Indentation"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplate",
					new DescriptionAttribute(SR.GetString("XamDataTree_ItemTemplate_Property")),
				    new DisplayNameAttribute("ItemTemplate"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveNode",
					new DescriptionAttribute(SR.GetString("XamDataTree_ActiveNode_Property")),
				    new DisplayNameAttribute("ActiveNode"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowActiveNodeIndicator",
					new DescriptionAttribute(SR.GetString("XamDataTree_ShowActiveNodeIndicator_Property")),
				    new DisplayNameAttribute("ShowActiveNodeIndicator"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectionSettings",
					new DescriptionAttribute(SR.GetString("XamDataTree_SelectionSettings_Property")),
				    new DisplayNameAttribute("SelectionSettings"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveNodeIndicatorStyle",
					new DescriptionAttribute(SR.GetString("XamDataTree_ActiveNodeIndicatorStyle_Property")),
				    new DisplayNameAttribute("ActiveNodeIndicatorStyle"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NodeStyle",
					new DescriptionAttribute(SR.GetString("XamDataTree_NodeStyle_Property")),
				    new DisplayNameAttribute("NodeStyle"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EditingSettings",
					new DescriptionAttribute(SR.GetString("XamDataTree_EditingSettings_Property")),
				    new DisplayNameAttribute("EditingSettings"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxSettings",
					new DescriptionAttribute(SR.GetString("XamDataTree_CheckBoxSettings_Property")),
				    new DisplayNameAttribute("CheckBoxSettings"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDraggable",
					new DescriptionAttribute(SR.GetString("XamDataTree_IsDraggable_Property")),
				    new DisplayNameAttribute("IsDraggable"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDropTarget",
					new DescriptionAttribute(SR.GetString("XamDataTree_IsDropTarget_Property")),
				    new DisplayNameAttribute("IsDropTarget"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NodeLineVisibility",
					new DescriptionAttribute(SR.GetString("XamDataTree_NodeLineVisibility_Property")),
				    new DisplayNameAttribute("NodeLineVisibility"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ExpandedIconTemplate",
					new DescriptionAttribute(SR.GetString("XamDataTree_ExpandedIconTemplate_Property")),
				    new DisplayNameAttribute("ExpandedIconTemplate"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CollapsedIconTemplate",
					new DescriptionAttribute(SR.GetString("XamDataTree_CollapsedIconTemplate_Property")),
				    new DisplayNameAttribute("CollapsedIconTemplate"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EnsureNodeExpansion",
					new DescriptionAttribute(SR.GetString("XamDataTree_EnsureNodeExpansion_Property")),
				    new DisplayNameAttribute("EnsureNodeExpansion"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowDragDropCopy",
					new DescriptionAttribute(SR.GetString("XamDataTree_AllowDragDropCopy_Property")),
				    new DisplayNameAttribute("AllowDragDropCopy"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsTouchSupportEnabled",
					new DescriptionAttribute(SR.GetString("XamDataTree_IsTouchSupportEnabled_Property")),
				    new DisplayNameAttribute("IsTouchSupportEnabled"),
					new CategoryAttribute(SR.GetString("XamDataTree_Properties"))
				);

				#endregion // XamDataTree Properties

				#region SelectedCollectionBase`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.SelectedCollectionBase`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Tree",
					new DescriptionAttribute(SR.GetString("SelectedCollectionBase`1_Tree_Property")),
				    new DisplayNameAttribute("Tree")				);

				#endregion // SelectedCollectionBase`1 Properties

				#region SelectedNodesCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.SelectedNodesCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Tree",
					new DescriptionAttribute(SR.GetString("SelectedCollectionBase`1_Tree_Property")),
				    new DisplayNameAttribute("Tree")				);

				#endregion // SelectedNodesCollection Properties

				#region TreeEditingSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.TreeEditingSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowEditing",
					new DescriptionAttribute(SR.GetString("TreeEditingSettings_AllowEditing_Property")),
				    new DisplayNameAttribute("AllowEditing")				);


				tableBuilder.AddCustomAttributes(t, "IsF2EditingEnabled",
					new DescriptionAttribute(SR.GetString("TreeEditingSettings_IsF2EditingEnabled_Property")),
				    new DisplayNameAttribute("IsF2EditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsEnterKeyEditingEnabled",
					new DescriptionAttribute(SR.GetString("TreeEditingSettings_IsEnterKeyEditingEnabled_Property")),
				    new DisplayNameAttribute("IsEnterKeyEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsMouseActionEditingEnabled",
					new DescriptionAttribute(SR.GetString("TreeEditingSettings_IsMouseActionEditingEnabled_Property")),
				    new DisplayNameAttribute("IsMouseActionEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsOnNodeActiveEditingEnabled",
					new DescriptionAttribute(SR.GetString("TreeEditingSettings_IsOnNodeActiveEditingEnabled_Property")),
				    new DisplayNameAttribute("IsOnNodeActiveEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "AllowDeletion",
					new DescriptionAttribute(SR.GetString("TreeEditingSettings_AllowDeletion_Property")),
				    new DisplayNameAttribute("AllowDeletion")				);

				#endregion // TreeEditingSettings Properties

				#region TreeSelectionSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.TreeSelectionSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NodeSelection",
					new DescriptionAttribute(SR.GetString("TreeSelectionSettings_NodeSelection_Property")),
				    new DisplayNameAttribute("NodeSelection")				);


				tableBuilder.AddCustomAttributes(t, "SelectedNodes",
					new DescriptionAttribute(SR.GetString("TreeSelectionSettings_SelectedNodes_Property")),
				    new DisplayNameAttribute("SelectedNodes")				);

				#endregion // TreeSelectionSettings Properties

				#region NodeLayoutCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodeLayoutCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NodeLayoutCollection Properties

				#region CheckBoxSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.CheckBoxSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxStyle",
					new DescriptionAttribute(SR.GetString("CheckBoxSettings_CheckBoxStyle_Property")),
				    new DisplayNameAttribute("CheckBoxStyle")				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxVisibility",
					new DescriptionAttribute(SR.GetString("CheckBoxSettings_CheckBoxVisibility_Property")),
				    new DisplayNameAttribute("CheckBoxVisibility")				);


				tableBuilder.AddCustomAttributes(t, "IsCheckBoxThreeState",
					new DescriptionAttribute(SR.GetString("CheckBoxSettings_IsCheckBoxThreeState_Property")),
				    new DisplayNameAttribute("IsCheckBoxThreeState")				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxMode",
					new DescriptionAttribute(SR.GetString("CheckBoxSettings_CheckBoxMode_Property")),
				    new DisplayNameAttribute("CheckBoxMode")				);

				#endregion // CheckBoxSettings Properties

				#region NodesPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.NodesPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Tree",
					new DescriptionAttribute(SR.GetString("NodesPanel_Tree_Property")),
				    new DisplayNameAttribute("Tree")				);


				tableBuilder.AddCustomAttributes(t, "VisibleNodes",
					new DescriptionAttribute(SR.GetString("NodesPanel_VisibleNodes_Property")),
				    new DisplayNameAttribute("VisibleNodes")				);

				#endregion // NodesPanel Properties

				#region IntermediateNodesManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.IntermediateNodesManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // IntermediateNodesManager Properties

				#region GlobalNodeLayoutCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.GlobalNodeLayoutCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GlobalNodeLayoutCollection Properties

				#region NodeLineTerminator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.NodeLineTerminator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Node",
					new DescriptionAttribute(SR.GetString("NodeLineTerminator_Node_Property")),
				    new DisplayNameAttribute("Node")				);


				tableBuilder.AddCustomAttributes(t, "NodeLineEnd",
					new DescriptionAttribute(SR.GetString("NodeLineTerminator_NodeLineEnd_Property")),
				    new DisplayNameAttribute("NodeLineEnd")				);

				#endregion // NodeLineTerminator Properties

				#region TreeEditingSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.TreeEditingSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowEditing",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_AllowEditing_Property")),
				    new DisplayNameAttribute("AllowEditing")				);


				tableBuilder.AddCustomAttributes(t, "AllowEditingResolved",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_AllowEditingResolved_Property")),
				    new DisplayNameAttribute("AllowEditingResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsF2EditingEnabled",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_IsF2EditingEnabled_Property")),
				    new DisplayNameAttribute("IsF2EditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsF2EditingEnabledResolved",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_IsF2EditingEnabledResolved_Property")),
				    new DisplayNameAttribute("IsF2EditingEnabledResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsEnterKeyEditingEnabled",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_IsEnterKeyEditingEnabled_Property")),
				    new DisplayNameAttribute("IsEnterKeyEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsEnterKeyEditingEnabledResolved",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_IsEnterKeyEditingEnabledResolved_Property")),
				    new DisplayNameAttribute("IsEnterKeyEditingEnabledResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsMouseActionEditingEnabled",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_IsMouseActionEditingEnabled_Property")),
				    new DisplayNameAttribute("IsMouseActionEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsMouseActionEditingEnabledResolved",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_IsMouseActionEditingEnabledResolved_Property")),
				    new DisplayNameAttribute("IsMouseActionEditingEnabledResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsOnNodeActiveEditingEnabled",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_IsOnNodeActiveEditingEnabled_Property")),
				    new DisplayNameAttribute("IsOnNodeActiveEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsOnNodeActiveEditingEnabledResolved",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_IsOnNodeActiveEditingEnabledResolved_Property")),
				    new DisplayNameAttribute("IsOnNodeActiveEditingEnabledResolved")				);


				tableBuilder.AddCustomAttributes(t, "AllowDeletion",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_AllowDeletion_Property")),
				    new DisplayNameAttribute("AllowDeletion")				);


				tableBuilder.AddCustomAttributes(t, "AllowDeletionResolved",
					new DescriptionAttribute(SR.GetString("TreeEditingSettingsOverride_AllowDeletionResolved_Property")),
				    new DisplayNameAttribute("AllowDeletionResolved")				);

				#endregion // TreeEditingSettingsOverride Properties

				#region CheckBoxSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.CheckBoxSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxVisibility",
					new DescriptionAttribute(SR.GetString("CheckBoxSettingsOverride_CheckBoxVisibility_Property")),
				    new DisplayNameAttribute("CheckBoxVisibility")				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxVisibilityResolved",
					new DescriptionAttribute(SR.GetString("CheckBoxSettingsOverride_CheckBoxVisibilityResolved_Property")),
				    new DisplayNameAttribute("CheckBoxVisibilityResolved")				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxStyle",
					new DescriptionAttribute(SR.GetString("CheckBoxSettingsOverride_CheckBoxStyle_Property")),
				    new DisplayNameAttribute("CheckBoxStyle")				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxStyleResolved",
					new DescriptionAttribute(SR.GetString("CheckBoxSettingsOverride_CheckBoxStyleResolved_Property")),
				    new DisplayNameAttribute("CheckBoxStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsCheckBoxThreeStateResolved",
					new DescriptionAttribute(SR.GetString("CheckBoxSettingsOverride_IsCheckBoxThreeStateResolved_Property")),
				    new DisplayNameAttribute("IsCheckBoxThreeStateResolved")				);

				#endregion // CheckBoxSettingsOverride Properties

				#region NodeLineControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.NodeLineControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Node",
					new DescriptionAttribute(SR.GetString("NodeLineControl_Node_Property")),
				    new DisplayNameAttribute("Node")				);


				tableBuilder.AddCustomAttributes(t, "VerticalLine",
					new DescriptionAttribute(SR.GetString("NodeLineControl_VerticalLine_Property")),
				    new DisplayNameAttribute("VerticalLine")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalLine",
					new DescriptionAttribute(SR.GetString("NodeLineControl_HorizontalLine_Property")),
				    new DisplayNameAttribute("HorizontalLine")				);

				#endregion // NodeLineControl Properties

				#region TreeDataObjectCreationEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.TreeDataObjectCreationEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NewObject",
					new DescriptionAttribute(SR.GetString("TreeDataObjectCreationEventArgs_NewObject_Property")),
				    new DisplayNameAttribute("NewObject")				);


				tableBuilder.AddCustomAttributes(t, "ObjectType",
					new DescriptionAttribute(SR.GetString("TreeDataObjectCreationEventArgs_ObjectType_Property")),
				    new DisplayNameAttribute("ObjectType")				);


				tableBuilder.AddCustomAttributes(t, "ParentNode",
					new DescriptionAttribute(SR.GetString("TreeDataObjectCreationEventArgs_ParentNode_Property")),
				    new DisplayNameAttribute("ParentNode")				);


				tableBuilder.AddCustomAttributes(t, "CollectionType",
					new DescriptionAttribute(SR.GetString("TreeDataObjectCreationEventArgs_CollectionType_Property")),
				    new DisplayNameAttribute("CollectionType")				);

				#endregion // TreeDataObjectCreationEventArgs Properties

				#region NodeSelectionEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodeSelectionEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OriginalSelectedNodes",
					new DescriptionAttribute(SR.GetString("NodeSelectionEventArgs_OriginalSelectedNodes_Property")),
				    new DisplayNameAttribute("OriginalSelectedNodes")				);


				tableBuilder.AddCustomAttributes(t, "CurrentSelectedNodes",
					new DescriptionAttribute(SR.GetString("NodeSelectionEventArgs_CurrentSelectedNodes_Property")),
				    new DisplayNameAttribute("CurrentSelectedNodes")				);

				#endregion // NodeSelectionEventArgs Properties

				#region TreeDropEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.TreeDropEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DragDropEventArgs",
					new DescriptionAttribute(SR.GetString("TreeDropEventArgs_DragDropEventArgs_Property")),
				    new DisplayNameAttribute("DragDropEventArgs")				);


				tableBuilder.AddCustomAttributes(t, "DropDestination",
					new DescriptionAttribute(SR.GetString("TreeDropEventArgs_DropDestination_Property")),
				    new DisplayNameAttribute("DropDestination")				);

				#endregion // TreeDropEventArgs Properties

				#region CancellableNodeDeletionEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.CancellableNodeDeletionEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CancellableNodeDeletionEventArgs Properties

				#region NodeDeletedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.NodeDeletedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NodeDeletedEventArgs Properties

				#region ActiveNodeChangingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.ActiveNodeChangingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OriginalActiveTreeNode",
					new DescriptionAttribute(SR.GetString("ActiveNodeChangingEventArgs_OriginalActiveTreeNode_Property")),
				    new DisplayNameAttribute("OriginalActiveTreeNode")				);


				tableBuilder.AddCustomAttributes(t, "NewActiveTreeNode",
					new DescriptionAttribute(SR.GetString("ActiveNodeChangingEventArgs_NewActiveTreeNode_Property")),
				    new DisplayNameAttribute("NewActiveTreeNode")				);

				#endregion // ActiveNodeChangingEventArgs Properties

				#region XamDataTreeNodesCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamDataTreeNodesCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamDataTreeNodesCollection Properties
                this.AddCustomAttributes(tableBuilder);
				return tableBuilder.CreateTable();
			}
		}
	}
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved