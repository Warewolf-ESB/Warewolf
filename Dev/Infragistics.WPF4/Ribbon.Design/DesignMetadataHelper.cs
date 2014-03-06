using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;
using System.ComponentModel;
using Microsoft.Windows.Design.PropertyEditing;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Ribbon;
using System.Windows;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Design.Ribbon
{

	// JM 01-06-10 VS2010 Designer Support
	#region DesignMetadataHelper Static Class

	internal static class DesignMetadataHelper
	{
		internal static AttributeTableBuilder GetAttributeTableBuilder()
		{
			AttributeTableBuilder builder = new AttributeTableBuilder();


			//			// Infragistics.Windows.Editors.SectionPresenter
			//			// =============================================
			//			builder.AddCustomAttributes(typeof(Infragistics.Windows.Editors.SectionPresenter), CreateDescription("LD_SectionPresenter"));
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.XamRibbon), "AllowMinimize", CreateCategory("Behavior"), CreateDescription("Returns/sets whether the XamRibbon can be minimized.  When the XamRibbon is minimized only the tabs are visible."));

			// Infragistics.Windows.Ribbon.RibbonWindowContentHost
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.RibbonWindowContentHost), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("CaptionVisibility", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonWindowContentHost_P_CaptionVisibility"));
				callbackBuilder.AddCustomAttributes("IconResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonWindowContentHost_P_IconResolved"));
				callbackBuilder.AddCustomAttributes("Ribbon", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonWindowContentHost_P_Ribbon"));
				callbackBuilder.AddCustomAttributes("StatusBar", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonWindowContentHost_P_StatusBar"));

				// AS 12/2/09 TFS24267
				callbackBuilder.AddCustomAttributes("StatusBarPadding", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonWindowContentHost_P_StatusBarPadding"));
			});

			// Infragistics.Windows.Ribbon.GalleryToolPreviewPresenter
			// =======================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.GalleryToolPreviewPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("GalleryTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryToolPreviewPresenter_P_GalleryTool"));
				callbackBuilder.AddCustomAttributes("MaxColumns", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryToolPreviewPresenter_P_MaxColumns"));
				callbackBuilder.AddCustomAttributes("MinColumns", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryToolPreviewPresenter_P_MinColumns"));
			});

			// Infragistics.Windows.Ribbon.ToolMenuItem
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ToolMenuItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsSegmented", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolMenuItem_P_IsSegmented"));
				callbackBuilder.AddCustomAttributes("IsSeparator", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolMenuItem_P_IsSeparator"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolMenuItem_P_Location"));
				callbackBuilder.AddCustomAttributes("MenuItemDescriptionMinWidth", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolMenuItem_P_MenuItemDescriptionMinWidth"));
				callbackBuilder.AddCustomAttributes("PopupVerticalScrollBarVisibility", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolMenuItem_P_PopupVerticalScrollBarVisibility"));
				callbackBuilder.AddCustomAttributes("ResizeMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolMenuItem_P_ResizeMode"));
				callbackBuilder.AddCustomAttributes("UseLargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolMenuItem_P_UseLargeImage"));
				callbackBuilder.AddCustomAttributes("Tool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolMenuItem_P_Tool"));
			});

			// Infragistics.Windows.Ribbon.ApplicationMenuPresenter
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ApplicationMenuPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("HasFooterToolbar", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ApplicationMenuPresenter_P_HasFooterToolbar"));
			});

			// Infragistics.Windows.Ribbon.QuickAccessToolbar
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.QuickAccessToolbar), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsBelowRibbon", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_QuickAccessToolbar_P_IsBelowRibbon"));
				callbackBuilder.AddCustomAttributes("IsOverflowOpen", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_QuickAccessToolbar_P_IsOverflowOpen"));
				callbackBuilder.AddCustomAttributes("OverflowButtonVisibility", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_QuickAccessToolbar_P_OverflowButtonVisibility"));
				callbackBuilder.AddCustomAttributes("QuickCustomizeMenuOpening", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_QuickAccessToolbar_E_QuickCustomizeMenuOpening"));
			});

			// Infragistics.Windows.Ribbon.GalleryToolDropDownPresenter
			// ========================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.GalleryToolDropDownPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsFirstInMenu", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryToolDropDownPresenter_P_IsFirstInMenu"));
				callbackBuilder.AddCustomAttributes("IsLastInMenu", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryToolDropDownPresenter_P_IsLastInMenu"));
				callbackBuilder.AddCustomAttributes("VerticalScrollBarVisibility", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryToolDropDownPresenter_P_VerticalScrollBarVisibility"));
			});

			// Infragistics.Windows.Ribbon.MenuToolPresenter
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.MenuToolPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("PreviewGalleryVisibility", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolPresenter_P_PreviewGalleryVisibility"));
			});

			// Infragistics.Windows.Ribbon.MenuToolBase
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.MenuToolBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsOpen", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_IsOpen"));
				callbackBuilder.AddCustomAttributes("MenuItemDescriptionMinWidth", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_MenuItemDescriptionMinWidth"));
				callbackBuilder.AddCustomAttributes("UseLargeImages", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_UseLargeImages"));
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_Caption"));
				callbackBuilder.AddCustomAttributes("HasCaption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_HasCaption"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_Id"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsQatCommonTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_IsQatCommonTool"));
				callbackBuilder.AddCustomAttributes("IsOnQat", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_IsOnQat"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_Location"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuToolBase_P_SizingMode"));
				callbackBuilder.AddCustomAttributes("Cloned", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_MenuToolBase_E_Cloned"));
				callbackBuilder.AddCustomAttributes("CloneDiscarded", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_MenuToolBase_E_CloneDiscarded"));
				callbackBuilder.AddCustomAttributes("Closed", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_MenuToolBase_E_Closed"));
				callbackBuilder.AddCustomAttributes("Opened", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_MenuToolBase_E_Opened"));
				callbackBuilder.AddCustomAttributes("Opening", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_MenuToolBase_E_Opening"));
            });

			// Infragistics.Windows.Ribbon.GalleryWrapPanel
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.GalleryWrapPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ColumnSpacing", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryWrapPanel_P_ColumnSpacing"));
				callbackBuilder.AddCustomAttributes("ColumnWidth", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryWrapPanel_P_ColumnWidth"));
				callbackBuilder.AddCustomAttributes("MaxColumns", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryWrapPanel_P_MaxColumns"));
				callbackBuilder.AddCustomAttributes("MinColumns", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryWrapPanel_P_MinColumns"));
				callbackBuilder.AddCustomAttributes("PreferredColumns", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryWrapPanel_P_PreferredColumns"));
				callbackBuilder.AddCustomAttributes("RowHeight", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryWrapPanel_P_RowHeight"));
			});

			// Infragistics.Windows.Ribbon.GalleryItemSettings
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.GalleryItemSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("GalleryItemPresenterStyle", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemSettings_P_GalleryItemPresenterStyle"));
				callbackBuilder.AddCustomAttributes("GalleryItemPresenterStyleSelector", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemSettings_P_GalleryItemPresenterStyleSelector"));
				callbackBuilder.AddCustomAttributes("HorizontalTextAlignment", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemSettings_P_HorizontalTextAlignment"));
				callbackBuilder.AddCustomAttributes("SelectionDisplayMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemSettings_P_SelectionDisplayMode"));
				callbackBuilder.AddCustomAttributes("TextPlacement", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemSettings_P_TextPlacement"));
				callbackBuilder.AddCustomAttributes("TextDisplayMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemSettings_P_TextDisplayMode"));
				callbackBuilder.AddCustomAttributes("VerticalTextAlignment", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemSettings_P_VerticalTextAlignment"));
			});

			// Infragistics.Windows.Ribbon.GalleryItem
			// =======================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.GalleryItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ColumnSpan", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItem_P_ColumnSpan"));
				callbackBuilder.AddCustomAttributes("Image", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItem_P_Image"));
				callbackBuilder.AddCustomAttributes("IsSelected", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItem_P_IsSelected"));
				callbackBuilder.AddCustomAttributes("Key", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItem_P_Key"));
				callbackBuilder.AddCustomAttributes("Settings", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItem_P_Settings"));
				callbackBuilder.AddCustomAttributes("Text", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItem_P_Text"));
				callbackBuilder.AddCustomAttributes("Tag", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItem_P_Tag"));
			});

			// Infragistics.Windows.Ribbon.QatPlaceholderTool
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.QatPlaceholderTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Target", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_QatPlaceholderTool_P_Target"));
				callbackBuilder.AddCustomAttributes("TargetId", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_QatPlaceholderTool_P_TargetId"));
				callbackBuilder.AddCustomAttributes("TargetType", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_QatPlaceholderTool_P_TargetType"));
			});

			// Infragistics.Windows.Ribbon.GalleryItemPresenter
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.GalleryItemPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("HorizontalTextAlignmentResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemPresenter_P_HorizontalTextAlignmentResolved"));
				callbackBuilder.AddCustomAttributes("IsHighlighted", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemPresenter_P_IsHighlighted"));
				callbackBuilder.AddCustomAttributes("IsInPreviewArea", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemPresenter_P_IsInPreviewArea"));
				callbackBuilder.AddCustomAttributes("IsSelected", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemPresenter_P_IsSelected"));
				callbackBuilder.AddCustomAttributes("Item", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemPresenter_P_Item"));
				callbackBuilder.AddCustomAttributes("SelectionDisplayModeResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemPresenter_P_SelectionDisplayModeResolved"));
				callbackBuilder.AddCustomAttributes("TextPlacementResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemPresenter_P_TextPlacementResolved"));
				callbackBuilder.AddCustomAttributes("TextVisibility", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemPresenter_P_TextVisibility"));
				callbackBuilder.AddCustomAttributes("VerticalTextAlignmentResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemPresenter_P_VerticalTextAlignmentResolved"));
			});

			// Infragistics.Windows.Ribbon.XamRibbonScreenTip
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.XamRibbonScreenTip), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ContentImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbonScreenTip_P_ContentImage"));
			});

			// Infragistics.Windows.Ribbon.ToggleButtonTool
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ToggleButtonTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_Caption"));
				callbackBuilder.AddCustomAttributes("HasCaption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_HasCaption"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_HasImage"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_Id"));
				callbackBuilder.AddCustomAttributes("ImageResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_ImageResolved"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsQatCommonTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_IsQatCommonTool"));
				callbackBuilder.AddCustomAttributes("IsOnQat", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_IsOnQat"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_Location"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_SizingMode"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToggleButtonTool_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("Cloned", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_ToggleButtonTool_E_Cloned"));
				callbackBuilder.AddCustomAttributes("CloneDiscarded", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_ToggleButtonTool_E_CloneDiscarded"));
            });

			// Infragistics.Windows.Ribbon.DropDownToggle
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.DropDownToggle), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsDroppedDown", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_DropDownToggle_P_IsDroppedDown"));
				callbackBuilder.AddCustomAttributes("IsPressed", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_DropDownToggle_P_IsPressed"));
			});

			// Infragistics.Windows.Ribbon.XamRibbon
			// =====================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.XamRibbon), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamRibbon"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamRibbonAssetLibrary")));


				callbackBuilder.AddCustomAttributes("ActiveItem", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_ActiveItem"));
				callbackBuilder.AddCustomAttributes("AllowMinimize", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_AllowMinimize"));
				callbackBuilder.AddCustomAttributes("ApplicationMenu", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_ApplicationMenu"));
				callbackBuilder.AddCustomAttributes("AutoHideEnabled", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_AutoHideEnabled"));
				callbackBuilder.AddCustomAttributes("AutoHideHorizontalThreshold", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_AutoHideHorizontalThreshold"));
				callbackBuilder.AddCustomAttributes("AutoHideState", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_AutoHideState"));
				callbackBuilder.AddCustomAttributes("AutoHideVerticalThreshold", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_AutoHideVerticalThreshold"));
				callbackBuilder.AddCustomAttributes("ContextualTabGroups", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_ContextualTabGroups"));
				callbackBuilder.AddCustomAttributes("IsInHighContrastMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_IsInHighContrastMode"));
				callbackBuilder.AddCustomAttributes("IsMinimized", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_IsMinimized"));
				callbackBuilder.AddCustomAttributes("IsWithinRibbonWindow", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_IsWithinRibbonWindow"));
				callbackBuilder.AddCustomAttributes("IsRibbonGroupResizingEnabled", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_IsRibbonGroupResizingEnabled"));
                callbackBuilder.AddCustomAttributes("QuickAccessToolbar", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_QuickAccessToolbar"));
				callbackBuilder.AddCustomAttributes("QuickAccessToolbarLocation", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_QuickAccessToolbarLocation"));
				callbackBuilder.AddCustomAttributes("SelectedTab", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_SelectedTab"));
				callbackBuilder.AddCustomAttributes("Tabs", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_Tabs"));
				callbackBuilder.AddCustomAttributes("TabsInView", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_TabsInView"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_Theme"));
				callbackBuilder.AddCustomAttributes("ToolsNotInRibbon", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_XamRibbon_P_ToolsNotInRibbon"));
				callbackBuilder.AddCustomAttributes("ThemeChanged", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_XamRibbon_E_ThemeChanged"));
				callbackBuilder.AddCustomAttributes("ActiveItemChanged", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_XamRibbon_E_ActiveItemChanged"));
				callbackBuilder.AddCustomAttributes("ExecutingCommand", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_XamRibbon_E_ExecutingCommand"));
				callbackBuilder.AddCustomAttributes("ExecutedCommand", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_XamRibbon_E_ExecutedCommand"));
				callbackBuilder.AddCustomAttributes("RibbonTabItemCloseUp", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_XamRibbon_E_RibbonTabItemCloseUp"));
				callbackBuilder.AddCustomAttributes("RibbonTabItemOpened", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_XamRibbon_E_RibbonTabItemOpened"));
				callbackBuilder.AddCustomAttributes("RibbonTabItemOpening", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_XamRibbon_E_RibbonTabItemOpening"));
				callbackBuilder.AddCustomAttributes("RibbonTabItemSelected", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_XamRibbon_E_RibbonTabItemSelected"));
			});

			// Infragistics.Windows.Ribbon.RadioButtonTool
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.RadioButtonTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_Caption"));
				callbackBuilder.AddCustomAttributes("HasCaption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_HasCaption"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_HasImage"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_Id"));
				callbackBuilder.AddCustomAttributes("ImageResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_ImageResolved"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsQatCommonTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_IsQatCommonTool"));
				callbackBuilder.AddCustomAttributes("IsOnQat", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_IsOnQat"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_Location"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_SizingMode"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RadioButtonTool_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("Cloned", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_RadioButtonTool_E_Cloned"));
				callbackBuilder.AddCustomAttributes("CloneDiscarded", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_RadioButtonTool_E_CloneDiscarded"));
            });

			// Infragistics.Windows.Ribbon.ApplicationMenu
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ApplicationMenu), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FooterToolbar", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ApplicationMenu_P_FooterToolbar"));
				callbackBuilder.AddCustomAttributes("HasRecentItemsHeader", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ApplicationMenu_P_HasRecentItemsHeader"));
				callbackBuilder.AddCustomAttributes("Image", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ApplicationMenu_P_Image"));
				callbackBuilder.AddCustomAttributes("RecentItems", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ApplicationMenu_P_RecentItems"));
				callbackBuilder.AddCustomAttributes("RecentItemsHeader", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ApplicationMenu_P_RecentItemsHeader"));
				callbackBuilder.AddCustomAttributes("RecentItemsHeaderTemplate", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ApplicationMenu_P_RecentItemsHeaderTemplate"));
				callbackBuilder.AddCustomAttributes("RecentItemsHeaderTemplateSelector", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ApplicationMenu_P_RecentItemsHeaderTemplateSelector"));
			});

			// Infragistics.Windows.Ribbon.KeyTip
			// ==================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.KeyTip), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Value", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_KeyTip_P_Value"));
			});

			// Infragistics.Windows.Ribbon.ContextualTabGroup
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ContextualTabGroup), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("BaseBackColor", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ContextualTabGroup_P_BaseBackColor"));
				callbackBuilder.AddCustomAttributes("BaseBackColorResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ContextualTabGroup_P_BaseBackColorResolved"));
				callbackBuilder.AddCustomAttributes("BaseBackColorResolvedBrush", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ContextualTabGroup_P_BaseBackColorResolvedBrush"));
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ContextualTabGroup_P_Caption"));
				callbackBuilder.AddCustomAttributes("IsVisible", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ContextualTabGroup_P_IsVisible"));
				callbackBuilder.AddCustomAttributes("Key", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ContextualTabGroup_P_Key"));
				callbackBuilder.AddCustomAttributes("Tabs", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ContextualTabGroup_P_Tabs"));
			});

			// Infragistics.Windows.Ribbon.TextEditorTool
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.TextEditorTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_Caption"));
				callbackBuilder.AddCustomAttributes("HasCaption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_HasCaption"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_HasImage"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_Id"));
				callbackBuilder.AddCustomAttributes("ImageResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_ImageResolved"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsQatCommonTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_IsQatCommonTool"));
				callbackBuilder.AddCustomAttributes("IsOnQat", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_IsOnQat"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_Location"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_SizingMode"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("EditAreaWidth", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_TextEditorTool_P_EditAreaWidth"));
				callbackBuilder.AddCustomAttributes("Cloned", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_TextEditorTool_E_Cloned"));
				callbackBuilder.AddCustomAttributes("CloneDiscarded", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_TextEditorTool_E_CloneDiscarded"));
            });

			// Infragistics.Windows.Ribbon.MenuButtonArea
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.MenuButtonArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuButtonArea_P_HasImage"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuButtonArea_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsCheckable", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuButtonArea_P_IsCheckable"));
				callbackBuilder.AddCustomAttributes("IsQuickCustomizeMenu", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuButtonArea_P_IsQuickCustomizeMenu"));
				callbackBuilder.AddCustomAttributes("IsSegmented", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuButtonArea_P_IsSegmented"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuButtonArea_P_Location"));
				callbackBuilder.AddCustomAttributes("MenuToolPresenter", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuButtonArea_P_MenuToolPresenter"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuButtonArea_P_SizingMode"));
			});

			// Infragistics.Windows.Ribbon.GalleryItemGroup
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.GalleryItemGroup), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ItemKeys", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemGroup_P_ItemKeys"));
				callbackBuilder.AddCustomAttributes("ItemSettings", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemGroup_P_ItemSettings"));
				callbackBuilder.AddCustomAttributes("GalleryTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemGroup_P_GalleryTool"));
				callbackBuilder.AddCustomAttributes("Title", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryItemGroup_P_Title"));
			});

			// Infragistics.Windows.Ribbon.ToolVerticalWrapPanel
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ToolVerticalWrapPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("VerticalToolAlignment", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolVerticalWrapPanel_P_VerticalToolAlignment"));
			});

			// Infragistics.Windows.Ribbon.RibbonGroup
			// =======================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.RibbonGroup), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_Caption"));
				callbackBuilder.AddCustomAttributes("DialogBoxLauncherTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_DialogBoxLauncherTool"));
				callbackBuilder.AddCustomAttributes("HasDialogBoxLauncherTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_HasDialogBoxLauncherTool"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_Id"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsCollapsed", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_IsCollapsed"));
				callbackBuilder.AddCustomAttributes("IsInContextualTabGroup", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_IsInContextualTabGroup"));
				callbackBuilder.AddCustomAttributes("IsOpen", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_IsOpen"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_Location"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("Variants", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonGroup_P_Variants"));
				callbackBuilder.AddCustomAttributes("Closed", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_RibbonGroup_E_Closed"));
				callbackBuilder.AddCustomAttributes("Opened", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_RibbonGroup_E_Opened"));
				callbackBuilder.AddCustomAttributes("Opening", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_RibbonGroup_E_Opening"));

            });

			// Infragistics.Windows.Ribbon.ButtonTool
			// ======================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ButtonTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_Caption"));
				callbackBuilder.AddCustomAttributes("HasCaption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_HasCaption"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_HasImage"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_Id"));
				callbackBuilder.AddCustomAttributes("ImageResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_ImageResolved"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsQatCommonTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_IsQatCommonTool"));
				callbackBuilder.AddCustomAttributes("IsOnQat", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_IsOnQat"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_Location"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_SizingMode"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ButtonTool_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("Cloned", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_ButtonTool_E_Cloned"));
				callbackBuilder.AddCustomAttributes("CloneDiscarded", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_ButtonTool_E_CloneDiscarded"));
            });

			// Infragistics.Windows.Ribbon.GroupVariant
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.GroupVariant), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Priority", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GroupVariant_P_Priority"));
				callbackBuilder.AddCustomAttributes("ResizeAction", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GroupVariant_P_ResizeAction"));
			});

			// Infragistics.Windows.Ribbon.RibbonTabItem
			// =========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.RibbonTabItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsFirstTabInContextualTabGroup", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonTabItem_P_IsFirstTabInContextualTabGroup"));
				callbackBuilder.AddCustomAttributes("IsInContextualTabGroup", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonTabItem_P_IsInContextualTabGroup"));
				callbackBuilder.AddCustomAttributes("IsLastTabInContextualTabGroup", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonTabItem_P_IsLastTabInContextualTabGroup"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonTabItem_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("RibbonGroups", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonTabItem_P_RibbonGroups"));
            });

			// Infragistics.Windows.Ribbon.MenuTool
			// ====================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.MenuTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ButtonType", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_ButtonType"));
				callbackBuilder.AddCustomAttributes("Command", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_Command"));
				callbackBuilder.AddCustomAttributes("CommandParameter", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_CommandParameter"));
				callbackBuilder.AddCustomAttributes("CommandTarget", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_CommandTarget"));
				callbackBuilder.AddCustomAttributes("IsChecked", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_IsChecked"));
				callbackBuilder.AddCustomAttributes("KeyTipForSegmentedButton", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_KeyTipForSegmentedButton"));
				callbackBuilder.AddCustomAttributes("ShouldDisplayGalleryPreview", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_ShouldDisplayGalleryPreview"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_HasImage"));
				callbackBuilder.AddCustomAttributes("ImageResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_ImageResolved"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MenuTool_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("Checked", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_MenuTool_E_Checked"));
				callbackBuilder.AddCustomAttributes("Click", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_MenuTool_E_Click"));
				callbackBuilder.AddCustomAttributes("Unchecked", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_MenuTool_E_Unchecked"));
			});

			// Infragistics.Windows.Ribbon.SeparatorTool
			// =========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.SeparatorTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_SeparatorTool_P_Location"));
				callbackBuilder.AddCustomAttributes("Cloned", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_SeparatorTool_E_Cloned"));
				callbackBuilder.AddCustomAttributes("CloneDiscarded", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_SeparatorTool_E_CloneDiscarded"));
            });

			// Infragistics.Windows.Ribbon.GalleryTool
			// =======================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.GalleryTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ActivationActionDelay", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_ActivationActionDelay"));
				callbackBuilder.AddCustomAttributes("ActivationInitialActionDelay", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_ActivationInitialActionDelay"));
				callbackBuilder.AddCustomAttributes("AllowResizeDropDown", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_AllowResizeDropDown"));
				callbackBuilder.AddCustomAttributes("Groups", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_Groups"));
				callbackBuilder.AddCustomAttributes("ItemBehavior", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_ItemBehavior"));
				callbackBuilder.AddCustomAttributes("Items", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_Items"));
				callbackBuilder.AddCustomAttributes("ItemSettings", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_ItemSettings"));
				callbackBuilder.AddCustomAttributes("MaxDropDownColumns", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_MaxDropDownColumns"));
				callbackBuilder.AddCustomAttributes("MaxPreviewColumns", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_MaxPreviewColumns"));
				callbackBuilder.AddCustomAttributes("MinDropDownColumns", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_MinDropDownColumns"));
				callbackBuilder.AddCustomAttributes("MinPreviewColumns", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_MinPreviewColumns"));
				callbackBuilder.AddCustomAttributes("PreferredDropDownColumns", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_PreferredDropDownColumns"));
				callbackBuilder.AddCustomAttributes("PreviewItems", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_PreviewItems"));
				callbackBuilder.AddCustomAttributes("SelectedItem", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_SelectedItem"));
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_Caption"));
				callbackBuilder.AddCustomAttributes("HasCaption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_HasCaption"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_HasImage"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_Id"));
				callbackBuilder.AddCustomAttributes("ImageResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_ImageResolved"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsQatCommonTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_IsQatCommonTool"));
				callbackBuilder.AddCustomAttributes("IsOnQat", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_IsOnQat"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_Location"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_SizingMode"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_GalleryTool_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("Cloned", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_GalleryTool_E_Cloned"));
				callbackBuilder.AddCustomAttributes("CloneDiscarded", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_GalleryTool_E_CloneDiscarded"));
				callbackBuilder.AddCustomAttributes("ItemActivated", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_GalleryTool_E_ItemActivated"));
				callbackBuilder.AddCustomAttributes("ItemClicked", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_GalleryTool_E_ItemClicked"));
				callbackBuilder.AddCustomAttributes("ItemSelected", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_GalleryTool_E_ItemSelected"));
            });

			// Infragistics.Windows.Ribbon.ButtonGroup
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ButtonGroup), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_ButtonGroup"));
			});

			// Infragistics.Windows.Ribbon.ToolVerticalWrapPanel
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ToolVerticalWrapPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_ToolVerticalWrapPanel"));
			});

			// Infragistics.Windows.Ribbon.ToolHorizontalWrapPanel
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ToolHorizontalWrapPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_ToolHorizontalWrapPanel"));

				callbackBuilder.AddCustomAttributes("RowCount", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolHorizontalWrapPanel_P_RowCount"));
				callbackBuilder.AddCustomAttributes("MaxRows", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolHorizontalWrapPanel_P_MaxRows"));
				callbackBuilder.AddCustomAttributes("MinRows", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ToolHorizontalWrapPanel_P_MinRows"));
			});

			// Infragistics.Windows.Ribbon.MaskedEditorTool
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.MaskedEditorTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_Caption"));
				callbackBuilder.AddCustomAttributes("HasCaption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_HasCaption"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_HasImage"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_Id"));
				callbackBuilder.AddCustomAttributes("ImageResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_ImageResolved"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsQatCommonTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_IsQatCommonTool"));
				callbackBuilder.AddCustomAttributes("IsOnQat", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_IsOnQat"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_Location"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_SizingMode"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("EditAreaWidth", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_MaskedEditorTool_P_EditAreaWidth"));
				callbackBuilder.AddCustomAttributes("Cloned", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_MaskedEditorTool_E_Cloned"));
				callbackBuilder.AddCustomAttributes("CloneDiscarded", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_MaskedEditorTool_E_CloneDiscarded"));
            });

			// Infragistics.Windows.Ribbon.RibbonButtonChrome
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.RibbonButtonChrome), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsChecked", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonButtonChrome_P_IsChecked"));
				callbackBuilder.AddCustomAttributes("IsPressed", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonButtonChrome_P_IsPressed"));
				callbackBuilder.AddCustomAttributes("IsSegmentedButton", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonButtonChrome_P_IsSegmentedButton"));
				callbackBuilder.AddCustomAttributes("Padding", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonButtonChrome_P_Padding"));
			});

			// Infragistics.Windows.Ribbon.LabelTool
			// =====================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.LabelTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_Caption"));
				callbackBuilder.AddCustomAttributes("HasCaption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_HasCaption"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_HasImage"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_Id"));
				callbackBuilder.AddCustomAttributes("ImageResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_ImageResolved"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsQatCommonTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_IsQatCommonTool"));
				callbackBuilder.AddCustomAttributes("IsOnQat", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_IsOnQat"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_Location"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_SizingMode"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LabelTool_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("Cloned", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_LabelTool_E_Cloned"));
				callbackBuilder.AddCustomAttributes("CloneDiscarded", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_LabelTool_E_CloneDiscarded"));
            });

			// Infragistics.Windows.Ribbon.LargeToolCaptionPresenter
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.LargeToolCaptionPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Text", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LargeToolCaptionPresenter_P_Text"));
				callbackBuilder.AddCustomAttributes("Glyph", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_LargeToolCaptionPresenter_P_Glyph"));
			});

			// Infragistics.Windows.Ribbon.ComboEditorTool
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.ComboEditorTool), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_Caption"));
				callbackBuilder.AddCustomAttributes("HasCaption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_HasCaption"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_HasImage"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_Id"));
				callbackBuilder.AddCustomAttributes("ImageResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_ImageResolved"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsQatCommonTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_IsQatCommonTool"));
				callbackBuilder.AddCustomAttributes("IsOnQat", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_IsOnQat"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_Location"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_SizingMode"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_SmallImage"));
				callbackBuilder.AddCustomAttributes("EditAreaWidth", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_ComboEditorTool_P_EditAreaWidth"));
				callbackBuilder.AddCustomAttributes("Cloned", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_ComboEditorTool_E_Cloned"));
				callbackBuilder.AddCustomAttributes("CloneDiscarded", CreateCategory("LC_Ribbon Events"), CreateDescription("LD_ComboEditorTool_E_CloneDiscarded"));
			});

			// Infragistics.Windows.Ribbon.RibbonWindowContentHost
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.Ribbon.RibbonToolHelper), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_Caption"));
				callbackBuilder.AddCustomAttributes("HasCaption", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_HasCaption"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_HasImage"));
				callbackBuilder.AddCustomAttributes("Id", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_Id"));
				callbackBuilder.AddCustomAttributes("ImageResolved", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_ImageResolved"));
				callbackBuilder.AddCustomAttributes("IsOnQat", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_IsOnQat"));
				callbackBuilder.AddCustomAttributes("IsQatCommonTool", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_IsQatCommonTool"));
				callbackBuilder.AddCustomAttributes("KeyTip", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_KeyTip"));
				callbackBuilder.AddCustomAttributes("LargeImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_LargeImage"));
				callbackBuilder.AddCustomAttributes("SizingMode", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_SizingMode"));
				callbackBuilder.AddCustomAttributes("SmallImage", CreateCategory("LC_Ribbon Properties"), CreateDescription("LD_RibbonToolHelper_P_SmallImage"));
			});

			// JM BR28206 11-06-07
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.RibbonTabItem), ToolboxBrowsableAttribute.No);

			// JM BR28204 11-06-07
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.RibbonContextMenu), ToolboxBrowsableAttribute.No);

			// JM BR28203 11-06-07
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.QuickAccessToolbarPanel), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.QuickAccessToolbarOverflowPanel), ToolboxBrowsableAttribute.No);

			// AS 11/16/07 BR28520
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.QuickAccessToolbar), "Items", new NewItemTypesAttribute(typeof(QatPlaceholderTool), typeof(SeparatorTool)));

			// JM BR33928 06-16-08
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.QatPlaceholderTool), ToolboxBrowsableAttribute.No);


			#region ToolboxBrowsableAttribute

			// AS 1/8/08 ToolboxBrowsable
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ApplicationMenu), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ApplicationMenuFooterToolbar), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ApplicationMenuItemsPanel), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ApplicationMenuPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ApplicationMenuRecentItemsPanel), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.DropDownToggle), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.GalleryItemGroup), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.GalleryPreviewScroller), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.GalleryWrapPanel), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.KeyTip), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.LargeToolCaptionPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.MenuButtonArea), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.Internal.MenuToolPanel), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.MenuToolPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.QuickAccessToolbar), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.RibbonButtonChrome), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.RibbonCaptionPanel), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.RibbonGroup), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.RibbonGroupPanel), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.RibbonWindowBorder), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.RibbonWindowContentHost), ToolboxBrowsableAttribute.No);

            // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ScenicRibbonCaptionArea), ToolboxBrowsableAttribute.No);

			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ToolMenuItem), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.XamRibbonWindow), ToolboxBrowsableAttribute.No);

			// AS 1/15/08 BR29686
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.XamRibbonScreenTip), ToolboxBrowsableAttribute.No);

            // JJD 06/04/10 - TFS32695 
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ButtonGroup), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ButtonTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.CheckBoxTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ComboEditorTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.GalleryItemPresenter), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.GalleryTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.GalleryToolDropDownPresenter), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.GalleryToolPreviewPresenter), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.LabelTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ButtonTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.MaskedEditorTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.MenuTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.MenuToolBase), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.RadioButtonTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.SeparatorTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.TextEditorTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ToggleButtonTool), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ToolHorizontalWrapPanel), ToolboxBrowsableAttribute.No);
            builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.ToolVerticalWrapPanel), ToolboxBrowsableAttribute.No);

			// AS 8/19/11 TFS83576
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.OuterGlowDecorator), ToolboxBrowsableAttribute.No);

			#endregion //ToolboxBrowsableAttribute

			#region NewItemTypesAttribute

			// New ItemTypes Attributes
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.RibbonGroup), "Items", new NewItemTypesAttribute(new Type[] {typeof(ButtonTool), 
																																		typeof(CheckBoxTool), 
																																		typeof(ComboEditorTool), 
																																		typeof(LabelTool), 
																																		typeof(MaskedEditorTool), 
																																		typeof(MenuTool), 
																																		typeof(RadioButtonTool), 
																																		typeof(SeparatorTool), 
																																		typeof(TextEditorTool), 
																																		typeof(ToggleButtonTool) }));

			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.MenuTool), "Items", new NewItemTypesAttribute(new Type[] {typeof(ButtonTool), 
																																		typeof(CheckBoxTool), 
																																		typeof(ComboEditorTool), 
																																		typeof(GalleryTool), 
																																		typeof(LabelTool), 
																																		typeof(MaskedEditorTool), 
																																		typeof(MenuTool), 
																																		typeof(RadioButtonTool), 
																																		typeof(SeparatorTool), 
																																		typeof(TextEditorTool), 
																																		typeof(ToggleButtonTool) }));

			builder.AddCustomAttributes(typeof(Infragistics.Windows.Ribbon.XamRibbon), "ToolsNotInRibbon", new NewItemTypesAttribute(new Type[] {typeof(ButtonTool), 
																																		typeof(CheckBoxTool), 
																																		typeof(ComboEditorTool), 
																																		typeof(GalleryTool), 
																																		typeof(LabelTool), 
																																		typeof(MaskedEditorTool), 
																																		typeof(MenuTool), 
																																		typeof(RadioButtonTool), 
																																		typeof(SeparatorTool), 
																																		typeof(TextEditorTool), 
																																		typeof(ToggleButtonTool) }));

			#endregion //NewItemTypesAttribute

			//			// Infragistics.Windows.Editors.TextEditorBase
			//			// ===========================================
			//			builder.AddCustomAttributes(typeof(Infragistics.Windows.Editors.TextEditorBase), "DisplayText", CreateCategory("LC_Behavior"), CreateDescription("LD_TextEditorBase_P_DisplayText"));

			return builder;
		}

		#region Methods
    
		#region CreateDescription
		private static DescriptionAttribute CreateDescription(string resourceName)
		{
			return new System.ComponentModel.DescriptionAttribute(SR.GetString(resourceName));
		}
		#endregion //CreateDescription

		#region CreateCategory
		[ThreadStatic]
		private static Dictionary<string, CategoryAttribute> _categories;

		private static CategoryAttribute CreateCategory(string resourceName)
		{
			if (_categories == null)
				_categories = new Dictionary<string, CategoryAttribute>();

			CategoryAttribute category;

			if (!_categories.TryGetValue(resourceName, out category))
			{
				category = new System.ComponentModel.CategoryAttribute(SR.GetString(resourceName));
				_categories.Add(resourceName, category);
			}

			return category;
		}
		#endregion //CreateCategory

		#endregion //Methods
	}

	#endregion //DesignMetadataHelper Static Class
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