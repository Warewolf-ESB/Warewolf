using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;
using System.ComponentModel;
using Infragistics.Windows.DockManager;
using Microsoft.Windows.Design.PropertyEditing;

namespace Infragistics.Windows.Design.DockManager
{

	// JM 01-06-10 VS2010 Designer Support
	#region DesignMetadataHelper Static Class

	internal static class DesignMetadataHelper
	{
		internal static AttributeTableBuilder GetAttributeTableBuilder()
		{
			AttributeTableBuilder builder = new AttributeTableBuilder();

			#region Description, Category, etc.

			// Infragistics.Windows.DockManager.XamDockManager
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.XamDockManager), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamDockManager"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamDockManagerAssetLibrary")));


				callbackBuilder.AddCustomAttributes("ActivePane", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_ActivePane"));
				callbackBuilder.AddCustomAttributes("CloseBehavior", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_CloseBehavior"));
				callbackBuilder.AddCustomAttributes("FlyoutAnimation", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_FlyoutAnimation"));
				callbackBuilder.AddCustomAttributes("HasDocumentContentHost", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_HasDocumentContentHost"));
				callbackBuilder.AddCustomAttributes("NavigationOrder", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_NavigationOrder"));
				callbackBuilder.AddCustomAttributes("PaneNavigatorButtonDisplayMode", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_PaneNavigatorButtonDisplayMode"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_Theme"));
				callbackBuilder.AddCustomAttributes("PinBehavior", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_PinBehavior"));
				callbackBuilder.AddCustomAttributes("ThemeChanged", CreateCategory("LC_DockManager Events"), CreateDescription("LD_XamDockManager_E_ThemeChanged"));
				callbackBuilder.AddCustomAttributes("ActivePaneChanged", CreateCategory("LC_DockManager Events"), CreateDescription("LD_XamDockManager_E_ActivePaneChanged"));
				callbackBuilder.AddCustomAttributes("ExecutingCommand", CreateCategory("LC_DockManager Events"), CreateDescription("LD_XamDockManager_E_ExecutingCommand"));
				callbackBuilder.AddCustomAttributes("ExecutedCommand", CreateCategory("LC_DockManager Events"), CreateDescription("LD_XamDockManager_E_ExecutedCommand"));
				callbackBuilder.AddCustomAttributes("InitializePaneContent", CreateCategory("LC_DockManager Events"), CreateDescription("LD_XamDockManager_E_InitializePaneContent"));
				callbackBuilder.AddCustomAttributes("PaneDragEnded", CreateCategory("LC_DockManager Events"), CreateDescription("LD_XamDockManager_E_PaneDragEnded"));
				callbackBuilder.AddCustomAttributes("PaneDragOver", CreateCategory("LC_DockManager Events"), CreateDescription("LD_XamDockManager_E_PaneDragOver"));
				callbackBuilder.AddCustomAttributes("PaneDragStarting", CreateCategory("LC_DockManager Events"), CreateDescription("LD_XamDockManager_E_PaneDragStarting"));
				callbackBuilder.AddCustomAttributes("ToolWindowLoaded", CreateCategory("LC_DockManager Events"), CreateDescription("LD_XamDockManager_E_ToolWindowLoaded"));
				callbackBuilder.AddCustomAttributes("ToolWindowUnloaded", CreateCategory("LC_DockManager Events"), CreateDescription("LD_XamDockManager_E_ToolWindowUnloaded"));
				callbackBuilder.AddCustomAttributes("Panes", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_Panes"));
				callbackBuilder.AddCustomAttributes("FloatingWindowDragMode", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_FloatingWindowDragMode"));

				// AS 11/12/09 TFS24789 - TabItemDragBehavior
				callbackBuilder.AddCustomAttributes("TabItemDragBehavior", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_TabItemDragBehavior"));

				// AS 6/23/11 TFS73499
				callbackBuilder.AddCustomAttributes("FloatingWindowVisibility", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_FloatingWindowVisibility"));

				// AS 3/31/11 NA 2011.1
				callbackBuilder.AddCustomAttributes("AllowMinimizeFloatingWindows", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_AllowMinimizeFloatingWindows"));
				callbackBuilder.AddCustomAttributes("AllowMaximizeFloatingWindows", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_AllowMaximizeFloatingWindows"));
				callbackBuilder.AddCustomAttributes("ShowFloatingWindowsInTaskbar", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_ShowFloatingWindowsInTaskbar"));
				callbackBuilder.AddCustomAttributes("UseOwnedFloatingWindows", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_UseOwnedFloatingWindows"));

				// AS 6/9/11 TFS76337
				callbackBuilder.AddCustomAttributes("FloatingWindowDoubleClickAction", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_FloatingWindowDoubleClickAction"));

				// AS 6/24/11 FloatingWindowCaptionSource
				callbackBuilder.AddCustomAttributes("FloatingWindowCaptionSource", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_FloatingWindowCaptionSource"));

				// AS 1/12/12 TFS24690
				callbackBuilder.AddCustomAttributes("GetFloatingLocation", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_FloatingLocation"));
				callbackBuilder.AddCustomAttributes("GetFloatingSize", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_FloatingSize"));
				callbackBuilder.AddCustomAttributes("GetInitialLocation", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_InitialLocation"));

			});

			// Infragistics.Windows.DockManager.UnpinnedTabItemPanel
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.UnpinnedTabItemPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("TabStripPlacement", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_UnpinnedTabItemPanel_P_TabStripPlacement"));
			});

			// Infragistics.Windows.DockManager.UnpinnedTabArea
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.UnpinnedTabArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.DockManager.PaneNavigatorItemsPanel
			// ========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.PaneNavigatorItemsPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.DockManager.DocumentContentHost
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.DocumentContentHost), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695 
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("ActiveDocument", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_DocumentContentHost_P_ActiveDocument"));
				callbackBuilder.AddCustomAttributes("ActiveDocumentChanged", CreateCategory("LC_DockManager Events"), CreateDescription("LD_DocumentContentHost_E_ActiveDocumentChanged"));
				callbackBuilder.AddCustomAttributes("Panes", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_DocumentContentHost_P_Panes"));
			});

			// Infragistics.Windows.DockManager.PaneSplitter
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.PaneSplitter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_PaneSplitter_P_Orientation"));
			});

			// Infragistics.Windows.DockManager.PaneToolWindow
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.PaneToolWindow), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("Pane", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_PaneToolWindow_P_Pane"));

				// AS 6/24/11 FloatingWindowCaptionSource
				callbackBuilder.AddCustomAttributes("FloatingWindowCaptionSource", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_PaneToolWindow_P_FloatingWindowCaptionSource"));
			});

			// Infragistics.Windows.DockManager.ContentPanePlaceholder
			// =======================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.ContentPanePlaceholder), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("PaneLocation", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPanePlaceholder_P_PaneLocation"));
			});

			// Infragistics.Windows.DockManager.PaneHeaderPresenter
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.PaneHeaderPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("Pane", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_PaneHeaderPresenter_P_Pane"));
			});

			// Infragistics.Windows.DockManager.DocumentTabPanel
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.DocumentTabPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("TabStripPlacement", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_DocumentTabPanel_P_TabStripPlacement"));
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.DockManager.TabGroupPane
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.TabGroupPane), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsTabItemAreaVisible", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_TabGroupPane_P_IsTabItemAreaVisible"));

				// AS 6/19/08 BR33075
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.DockManager.PaneNavigator
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.PaneNavigator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("SelectedPane", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_PaneNavigator_P_SelectedPane"));
				callbackBuilder.AddCustomAttributes("SelectedPaneAspectRatioIsWide", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_PaneNavigator_P_SelectedPaneAspectRatioIsWide"));
				callbackBuilder.AddCustomAttributes("SelectedPaneIsDocument", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_PaneNavigator_P_SelectedPaneIsDocument"));
			});

			// Infragistics.Windows.DockManager.UnpinnedTabFlyout
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.UnpinnedTabFlyout), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("Pane", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_UnpinnedTabFlyout_P_Pane"));
				callbackBuilder.AddCustomAttributes("Side", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_UnpinnedTabFlyout_P_Side"));
			});

			// Infragistics.Windows.DockManager.DocumentContentHostPanel
			// =========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.DocumentContentHostPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.DockManager.SplitPane
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.SplitPane), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695 
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("Panes", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_SplitPane_P_Panes"));
				callbackBuilder.AddCustomAttributes("SplitterOrientation", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_SplitPane_P_SplitterOrientation"));

				// AS 1/12/12 TFS24690
				callbackBuilder.AddCustomAttributes("GetRelativeSize", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_SplitPane_P_RelativeSize"));
			});

			// Infragistics.Windows.DockManager.DockManagerPanel
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.DockManagerPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.DockManager.DockingIndicator
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.DockingIndicator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("CanDockBottom", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_DockingIndicator_P_CanDockBottom"));
				callbackBuilder.AddCustomAttributes("CanDockTop", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_DockingIndicator_P_CanDockTop"));
				callbackBuilder.AddCustomAttributes("CanDockLeft", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_DockingIndicator_P_CanDockLeft"));
				callbackBuilder.AddCustomAttributes("CanDockRight", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_DockingIndicator_P_CanDockRight"));
				callbackBuilder.AddCustomAttributes("CanDockCenter", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_DockingIndicator_P_CanDockCenter"));
				callbackBuilder.AddCustomAttributes("HotTrackPosition", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_DockingIndicator_P_HotTrackPosition"));
				callbackBuilder.AddCustomAttributes("Position", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_DockingIndicator_P_Position"));
			});

			// Infragistics.Windows.DockManager.PaneTabItem
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.PaneTabItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("Pane", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_PaneTabItem_P_Pane"));
			});

			// Infragistics.Windows.DockManager.ContentPane
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.ContentPane), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695 
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("AllowDocking", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowDocking"));
				callbackBuilder.AddCustomAttributes("AllowDockingLeft", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowDockingLeft"));
				callbackBuilder.AddCustomAttributes("AllowDockingTop", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowDockingTop"));
				callbackBuilder.AddCustomAttributes("AllowDockingRight", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowDockingRight"));
				callbackBuilder.AddCustomAttributes("AllowDockingBottom", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowDockingBottom"));
				callbackBuilder.AddCustomAttributes("AllowDockingFloating", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowDockingFloating"));
				callbackBuilder.AddCustomAttributes("AllowInDocumentHost", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowInDocumentHost"));
				callbackBuilder.AddCustomAttributes("AllowFloatingOnly", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowFloatingOnly"));
				callbackBuilder.AddCustomAttributes("AllowPinning", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowPinning"));
				callbackBuilder.AddCustomAttributes("AllowClose", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowClose"));
				callbackBuilder.AddCustomAttributes("AllowDockingInTabGroup", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_AllowDockingInTabGroup"));
				callbackBuilder.AddCustomAttributes("CloseAction", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_CloseAction"));
				callbackBuilder.AddCustomAttributes("HasImage", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_HasImage"));
				callbackBuilder.AddCustomAttributes("HeaderVisibility", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_HeaderVisibility"));
				callbackBuilder.AddCustomAttributes("Image", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_Image"));
				callbackBuilder.AddCustomAttributes("IsActivePane", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_IsActivePane"));
				callbackBuilder.AddCustomAttributes("IsActiveDocument", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_IsActiveDocument"));
				callbackBuilder.AddCustomAttributes("IsPinned", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_IsPinned"));
				callbackBuilder.AddCustomAttributes("NavigatorDescription", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_NavigatorDescription"));
				callbackBuilder.AddCustomAttributes("NavigatorDescriptionTemplate", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_NavigatorDescriptionTemplate"));
				callbackBuilder.AddCustomAttributes("NavigatorDescriptionTemplateSelector", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_NavigatorDescriptionTemplateSelector"));
				callbackBuilder.AddCustomAttributes("NavigatorTitle", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_NavigatorTitle"));
				callbackBuilder.AddCustomAttributes("NavigatorTitleTemplate", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_NavigatorTitleTemplate"));
				callbackBuilder.AddCustomAttributes("NavigatorTitleTemplateSelector", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_NavigatorTitleTemplateSelector"));
				callbackBuilder.AddCustomAttributes("PaneLocation", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_PaneLocation"));
				callbackBuilder.AddCustomAttributes("SaveInLayout", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_SaveInLayout"));
				callbackBuilder.AddCustomAttributes("SerializationId", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_SerializationId"));
				callbackBuilder.AddCustomAttributes("TabHeader", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_TabHeader"));
				callbackBuilder.AddCustomAttributes("TabHeaderTemplate", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_TabHeaderTemplate"));
				callbackBuilder.AddCustomAttributes("TabHeaderTemplateSelector", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_TabHeaderTemplateSelector"));
				callbackBuilder.AddCustomAttributes("Closing", CreateCategory("LC_DockManager Events"), CreateDescription("LD_ContentPane_E_Closing"));
				callbackBuilder.AddCustomAttributes("Closed", CreateCategory("LC_DockManager Events"), CreateDescription("LD_ContentPane_E_Closed"));
				callbackBuilder.AddCustomAttributes("ExecutingCommand", CreateCategory("LC_DockManager Events"), CreateDescription("LD_ContentPane_E_ExecutingCommand"));
				callbackBuilder.AddCustomAttributes("ExecutedCommand", CreateCategory("LC_DockManager Events"), CreateDescription("LD_ContentPane_E_ExecutedCommand"));
				callbackBuilder.AddCustomAttributes("OptionsMenuOpening", CreateCategory("LC_DockManager Events"), CreateDescription("LD_ContentPane_E_OptionsMenuOpening"));
				callbackBuilder.AddCustomAttributes("ContentVisibility", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_ContentVisibility"));
			});

			#endregion //Description, Category, etc.

			#region NewItemTypesAttribute

			// New ItemTypes Attributes
			builder.AddCustomAttributes(typeof(SplitPane), "Panes",
				new NewItemTypesAttribute(new Type[] {typeof(SplitPane), 
													typeof(TabGroupPane), 
													typeof(ContentPane) }));

			builder.AddCustomAttributes(typeof(TabGroupPane), "Items",
				new NewItemTypesAttribute(new Type[] { typeof(ContentPane) }));

			#endregion //NewItemTypesAttribute

			#region TypeConverterAttribute

			builder.AddCallback(typeof(ContentPane), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 01-12-10 NA 10.1. 4.0 requires string here instead of DependencyProperty
				// AS 5/29/08 BR33465
				//callbackBuilder.AddCustomAttributes(ContentPane.TabHeaderProperty, new TypeConverterAttribute(typeof(StringConverter)));
				//callbackBuilder.AddCustomAttributes(ContentPane.NavigatorDescriptionProperty, new TypeConverterAttribute(typeof(StringConverter)));
				callbackBuilder.AddCustomAttributes("TabHeader", new TypeConverterAttribute(typeof(StringConverter)));
				callbackBuilder.AddCustomAttributes("NavigatorDescription", new TypeConverterAttribute(typeof(StringConverter)));
				callbackBuilder.AddCustomAttributes("NavigatorTitle", new TypeConverterAttribute(typeof(StringConverter)));
			});

			#endregion //TypeConverterAttribute

			#region BrowsableAttribute

			// JM 01-12-10 NA 10.1. 4.0 requires string here instead of DependencyProperty
			// AS 5/29/08 BR33465
			//builder.AddCustomAttributes(typeof(ContentPane), ContentPane.TabHeaderTemplateProperty, BrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(ContentPane), ContentPane.TabHeaderTemplateSelectorProperty, BrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(ContentPane), ContentPane.NavigatorTitleTemplateProperty, BrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(ContentPane), ContentPane.NavigatorTitleTemplateSelectorProperty, BrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(ContentPane), ContentPane.NavigatorDescriptionTemplateProperty, BrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(ContentPane), ContentPane.NavigatorDescriptionTemplateSelectorProperty, BrowsableAttribute.No);
			builder.AddCallback(typeof(ContentPane), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("TabHeaderTemplate", BrowsableAttribute.No);
				callbackBuilder.AddCustomAttributes("TabHeaderTemplateSelector", BrowsableAttribute.No);
				callbackBuilder.AddCustomAttributes("NavigatorTitleTemplate", BrowsableAttribute.No);
				callbackBuilder.AddCustomAttributes("NavigatorTitleTemplateSelector", BrowsableAttribute.No);
				callbackBuilder.AddCustomAttributes("NavigatorDescriptionTemplate", BrowsableAttribute.No);
				callbackBuilder.AddCustomAttributes("NavigatorDescriptionTemplateSelector", BrowsableAttribute.No);
			});

			#endregion //BrowsableAttribute

			#region NA 2010 Vol 1

			// Infragistics.Windows.DockManager.ContentPane
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.ContentPane), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("CloseButtonVisibility", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_CloseButtonVisibility"));
				callbackBuilder.AddCustomAttributes("PinButtonVisibility", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_PinButtonVisibility"));
				callbackBuilder.AddCustomAttributes("WindowPositionMenuVisibility", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_ContentPane_P_WindowPositionMenuVisibility"));
			});

			// Infragistics.Windows.DockManager.TabGroupPane
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.TabGroupPane), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FilesMenuOpening", CreateCategory("LC_DockManager Events"), CreateDescription("LD_TabGroupPane_E_FilesMenuOpening"));
			});

			// Infragistics.Windows.DockManager.XamDockManager
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DockManager.XamDockManager), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("LayoutMode", CreateCategory("LC_DockManager Properties"), CreateDescription("LD_XamDockManager_P_LayoutMode"));
				callbackBuilder.AddCustomAttributes("UnpinnedTabHoverAction", CreateCategory("LC_Behavior"), CreateDescription("LD_XamDockManager_P_UnpinnedTabHoverAction"));
			});

			#endregion

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