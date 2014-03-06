using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Menus.XamMenu.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Menus.XamMenu.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Menus.XamMenu);
				Assembly controlAssembly = t.Assembly;

				#region ContextMenuService Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.ContextMenuService");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ContextMenuService Properties

				#region ContextMenuManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.ContextMenuManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ContextMenu",
					new DescriptionAttribute(SR.GetString("ContextMenuManager_ContextMenu_Property")),
				    new DisplayNameAttribute("ContextMenu")				);


				tableBuilder.AddCustomAttributes(t, "ModifierKeys",
					new DescriptionAttribute(SR.GetString("ContextMenuManager_ModifierKeys_Property")),
				    new DisplayNameAttribute("ModifierKeys")				);


				tableBuilder.AddCustomAttributes(t, "OpenMode",
					new DescriptionAttribute(SR.GetString("ContextMenuManager_OpenMode_Property")),
				    new DisplayNameAttribute("OpenMode")				);

				#endregion // ContextMenuManager Properties

				#region ItemClickedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.ItemClickedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ItemClickedEventArgs_Item_Property")),
				    new DisplayNameAttribute("Item")				);

				#endregion // ItemClickedEventArgs Properties

				#region OpenedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.OpenedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MouseClickLocation",
					new DescriptionAttribute(SR.GetString("OpenedEventArgs_MouseClickLocation_Property")),
				    new DisplayNameAttribute("MouseClickLocation")				);


				tableBuilder.AddCustomAttributes(t, "ContextMenuLocation",
					new DescriptionAttribute(SR.GetString("OpenedEventArgs_ContextMenuLocation_Property")),
				    new DisplayNameAttribute("ContextMenuLocation")				);

				#endregion // OpenedEventArgs Properties

				#region OpeningEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.OpeningEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MouseClickLocation",
					new DescriptionAttribute(SR.GetString("OpeningEventArgs_MouseClickLocation_Property")),
				    new DisplayNameAttribute("MouseClickLocation")				);

				#endregion // OpeningEventArgs Properties

				#region XamContextMenuAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamContextMenuAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamContextMenuAutomationPeer Properties

				#region XamMenu Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamMenu");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamMenuAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamMenuAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "MenuOrientation",
					new DescriptionAttribute(SR.GetString("XamMenu_MenuOrientation_Property")),
				    new DisplayNameAttribute("MenuOrientation"),
					new CategoryAttribute(SR.GetString("XamMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ExpandOnHover",
					new DescriptionAttribute(SR.GetString("XamMenu_ExpandOnHover_Property")),
				    new DisplayNameAttribute("ExpandOnHover"),
					new CategoryAttribute(SR.GetString("XamMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NavigationElement",
					new DescriptionAttribute(SR.GetString("XamMenu_NavigationElement_Property")),
				    new DisplayNameAttribute("NavigationElement"),
					new CategoryAttribute(SR.GetString("XamMenu_Properties"))
				);

				#endregion // XamMenu Properties

				#region XamMenuBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamMenuBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamMenuBase Properties

				#region XamHeaderedItemsControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.XamHeaderedItemsControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HierarchicalItemTemplate",
					new DescriptionAttribute(SR.GetString("XamHeaderedItemsControl_HierarchicalItemTemplate_Property")),
				    new DisplayNameAttribute("HierarchicalItemTemplate"),
					new CategoryAttribute(SR.GetString("XamHeaderedItemsControl_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DefaultItemsContainer",
					new DescriptionAttribute(SR.GetString("XamHeaderedItemsControl_DefaultItemsContainer_Property")),
				    new DisplayNameAttribute("DefaultItemsContainer"),
					new CategoryAttribute(SR.GetString("XamHeaderedItemsControl_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Header",
					new DescriptionAttribute(SR.GetString("XamHeaderedItemsControl_Header_Property")),
				    new DisplayNameAttribute("Header"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamHeaderedItemsControl_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderTemplate",
					new DescriptionAttribute(SR.GetString("XamHeaderedItemsControl_HeaderTemplate_Property")),
				    new DisplayNameAttribute("HeaderTemplate"),
					new CategoryAttribute(SR.GetString("XamHeaderedItemsControl_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemContainerStyle",
					new DescriptionAttribute(SR.GetString("XamHeaderedItemsControl_ItemContainerStyle_Property")),
				    new DisplayNameAttribute("ItemContainerStyle"),
					new CategoryAttribute(SR.GetString("XamHeaderedItemsControl_Properties"))
				);

				#endregion // XamHeaderedItemsControl Properties

				#region XamMenuItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamMenuItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamMenuSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamMenuSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxVisibilityResolved",
					new DescriptionAttribute(SR.GetString("XamMenuItem_CheckBoxVisibilityResolved_Property")),
				    new DisplayNameAttribute("CheckBoxVisibilityResolved"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Icon",
					new DescriptionAttribute(SR.GetString("XamMenuItem_Icon_Property")),
				    new DisplayNameAttribute("Icon"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IconVisibilityResolved",
					new DescriptionAttribute(SR.GetString("XamMenuItem_IconVisibilityResolved_Property")),
				    new DisplayNameAttribute("IconVisibilityResolved"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InputGestureText",
					new DescriptionAttribute(SR.GetString("XamMenuItem_InputGestureText_Property")),
				    new DisplayNameAttribute("InputGestureText"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsCheckable",
					new DescriptionAttribute(SR.GetString("XamMenuItem_IsCheckable_Property")),
				    new DisplayNameAttribute("IsCheckable"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsChecked",
					new DescriptionAttribute(SR.GetString("XamMenuItem_IsChecked_Property")),
				    new DisplayNameAttribute("IsChecked"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsHighlighted",
					new DescriptionAttribute(SR.GetString("XamMenuItem_IsHighlighted_Property")),
				    new DisplayNameAttribute("IsHighlighted"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSubmenuOpen",
					new DescriptionAttribute(SR.GetString("XamMenuItem_IsSubmenuOpen_Property")),
				    new DisplayNameAttribute("IsSubmenuOpen"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MenuOrientation",
					new DescriptionAttribute(SR.GetString("XamMenuItem_MenuOrientation_Property")),
				    new DisplayNameAttribute("MenuOrientation"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasChildren",
					new DescriptionAttribute(SR.GetString("XamMenuItem_HasChildren_Property")),
				    new DisplayNameAttribute("HasChildren"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ParentXamMenuItem",
					new DescriptionAttribute(SR.GetString("XamMenuItem_ParentXamMenuItem_Property")),
				    new DisplayNameAttribute("ParentXamMenuItem"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SubmenuPreferredLocation",
					new DescriptionAttribute(SR.GetString("XamMenuItem_SubmenuPreferredLocation_Property")),
				    new DisplayNameAttribute("SubmenuPreferredLocation"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "StaysOpenOnClick",
					new DescriptionAttribute(SR.GetString("XamMenuItem_StaysOpenOnClick_Property")),
				    new DisplayNameAttribute("StaysOpenOnClick"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NavigationElement",
					new DescriptionAttribute(SR.GetString("XamMenuItem_NavigationElement_Property")),
				    new DisplayNameAttribute("NavigationElement"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NavigationUri",
					new DescriptionAttribute(SR.GetString("XamMenuItem_NavigationUri_Property")),
				    new DisplayNameAttribute("NavigationUri"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NavigationParameter",
					new DescriptionAttribute(SR.GetString("XamMenuItem_NavigationParameter_Property")),
				    new DisplayNameAttribute("NavigationParameter"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NavigationOnClick",
					new DescriptionAttribute(SR.GetString("XamMenuItem_NavigationOnClick_Property")),
				    new DisplayNameAttribute("NavigationOnClick"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsKeyboardNavigable",
					new DescriptionAttribute(SR.GetString("XamMenuItem_IsKeyboardNavigable_Property")),
				    new DisplayNameAttribute("IsKeyboardNavigable"),
					new CategoryAttribute(SR.GetString("XamMenuItem_Properties"))
				);

				#endregion // XamMenuItem Properties

				#region XamContextMenu Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamContextMenu");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamMenuAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamMenuAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "IsOpen",
					new DescriptionAttribute(SR.GetString("XamContextMenu_IsOpen_Property")),
				    new DisplayNameAttribute("IsOpen"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Placement",
					new DescriptionAttribute(SR.GetString("XamContextMenu_Placement_Property")),
				    new DisplayNameAttribute("Placement"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlacementTarget",
					new DescriptionAttribute(SR.GetString("XamContextMenu_PlacementTarget_Property")),
				    new DisplayNameAttribute("PlacementTarget"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlacementTargetResolved",
					new DescriptionAttribute(SR.GetString("XamContextMenu_PlacementTargetResolved_Property")),
				    new DisplayNameAttribute("PlacementTargetResolved"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlacementRectangle",
					new DescriptionAttribute(SR.GetString("XamContextMenu_PlacementRectangle_Property")),
				    new DisplayNameAttribute("PlacementRectangle"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalOffset",
					new DescriptionAttribute(SR.GetString("XamContextMenu_HorizontalOffset_Property")),
				    new DisplayNameAttribute("HorizontalOffset"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalOffset",
					new DescriptionAttribute(SR.GetString("XamContextMenu_VerticalOffset_Property")),
				    new DisplayNameAttribute("VerticalOffset"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MouseClickLocation",
					new DescriptionAttribute(SR.GetString("XamContextMenu_MouseClickLocation_Property")),
				    new DisplayNameAttribute("MouseClickLocation"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContextMenuLocation",
					new DescriptionAttribute(SR.GetString("XamContextMenu_ContextMenuLocation_Property")),
				    new DisplayNameAttribute("ContextMenuLocation"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShouldRightClickBeHandled",
					new DescriptionAttribute(SR.GetString("XamContextMenu_ShouldRightClickBeHandled_Property")),
				    new DisplayNameAttribute("ShouldRightClickBeHandled"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PopupParent",
					new DescriptionAttribute(SR.GetString("XamContextMenu_PopupParent_Property")),
				    new DisplayNameAttribute("PopupParent"),
					new CategoryAttribute(SR.GetString("XamContextMenu_Properties"))
				);

				#endregion // XamContextMenu Properties

				#region XamContextMenuCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamContextMenuCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamContextMenuCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamContextMenuCommandSource_Properties"))
				);

				#endregion // XamContextMenuCommandSource Properties

				#region XamContextMenuCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.XamContextMenuCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamContextMenuCommandBase Properties

				#region OpenCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.OpenCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // OpenCommand Properties

				#region CloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.CloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CloseCommand Properties

				#region XamMenuSeparator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.XamMenuSeparator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamMenuSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamMenuSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "SeparatorOrientation",
					new DescriptionAttribute(SR.GetString("XamMenuSeparator_SeparatorOrientation_Property")),
				    new DisplayNameAttribute("SeparatorOrientation"),
					new CategoryAttribute(SR.GetString("XamMenuSeparator_Properties"))
				);

				#endregion // XamMenuSeparator Properties

				#region XamMenuItemAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamMenuItemAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ExpandCollapseState",
					new DescriptionAttribute(SR.GetString("XamMenuItemAutomationPeer_ExpandCollapseState_Property")),
				    new DisplayNameAttribute("ExpandCollapseState"),
					new CategoryAttribute(SR.GetString("XamMenuItemAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ToggleState",
					new DescriptionAttribute(SR.GetString("XamMenuItemAutomationPeer_ToggleState_Property")),
				    new DisplayNameAttribute("ToggleState"),
					new CategoryAttribute(SR.GetString("XamMenuItemAutomationPeer_Properties"))
				);

				#endregion // XamMenuItemAutomationPeer Properties

				#region XamMenuAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamMenuAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamMenuAutomationPeer Properties

				#region IconContentControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Menus.Primitives.IconContentControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // IconContentControl Properties
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