using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;

using System.ComponentModel;
using Microsoft.Windows.Design.PropertyEditing;
using Infragistics.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Design
{

	// JM 01-06-10 VS2010 Designer Support
	#region DesignMetadataHelper Static Class

	internal static class DesignMetadataHelper
	{
		internal static AttributeTableBuilder GetAttributeTableBuilder()
		{
			AttributeTableBuilder builder = new AttributeTableBuilder();

			#region Description, Category, etc.


			// Infragistics.Windows.Controls.CarouselPanelItem
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.CarouselPanelItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_CarouselPanelItem"));

				callbackBuilder.AddCustomAttributes("AutoScaleItemContentsToFit", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselPanelItem_P_AutoScaleItemContentsToFit"));
				callbackBuilder.AddCustomAttributes("IsListContinuous", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselPanelItem_P_IsListContinuous"));
				callbackBuilder.AddCustomAttributes("IsFirstItem", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselPanelItem_P_IsFirstItem"));
				callbackBuilder.AddCustomAttributes("IsLastItem", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselPanelItem_P_IsLastItem"));
				callbackBuilder.AddCustomAttributes("ItemHorizontalScrollBarVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselPanelItem_P_ItemHorizontalScrollBarVisibility"));
				callbackBuilder.AddCustomAttributes("ItemVerticalScrollBarVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselPanelItem_P_ItemVerticalScrollBarVisibility"));
			});



			// Infragistics.Windows.Controls.SortIndicator
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.SortIndicator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_SortIndicator"));

				callbackBuilder.AddCustomAttributes("SortStatus", CreateCategory("LC_Behavior"), CreateDescription("LD_SortIndicator_P_SortStatus"));
				callbackBuilder.AddCustomAttributes("SortStatusChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_SortIndicator_E_SortStatusChanged"));
			});


			// Infragistics.Windows.Controls.CarouselPanelNavigator
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.CarouselPanelNavigator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_CarouselPanelNavigator"));

				callbackBuilder.AddCustomAttributes("CarouselPanel", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselPanelNavigator_P_CarouselPanel"));
			});



			// Infragistics.Windows.Controls.ExpanderBar
			// =========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.ExpanderBar), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
                
                callbackBuilder.AddCustomAttributes(CreateDescription("LD_ExpanderBar"));

				callbackBuilder.AddCustomAttributes("BackgroundHover", CreateCategory("LC_Brushes"), CreateDescription("LD_ExpanderBar_P_BackgroundHover"));
				callbackBuilder.AddCustomAttributes("BorderHoverBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_ExpanderBar_P_BorderHoverBrush"));
			});


			// Infragistics.Windows.Controls.CarouselListBoxItem
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.CarouselListBoxItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_CarouselListBoxItem"));
			});



			// Infragistics.Windows.Controls.ExpansionIndicator
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.ExpansionIndicator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
                
                callbackBuilder.AddCustomAttributes(CreateDescription("LD_ExpansionIndicator"));
			});



			// Infragistics.Windows.Controls.EffectStop
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.EffectStop), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Offset", CreateCategory("LC_Data"), CreateDescription("LD_EffectStop_P_Offset"));
				callbackBuilder.AddCustomAttributes("Value", CreateCategory("LC_Data"), CreateDescription("LD_EffectStop_P_Value"));
			});

			// Infragistics.Windows.Controls.XamCarouselPanel
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.XamCarouselPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamCarouselPanel"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamCarouselPanelAssetLibrary")));


				callbackBuilder.AddCustomAttributes("CanNavigateToPreviousItem", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCarouselPanel_P_CanNavigateToPreviousItem"));
				callbackBuilder.AddCustomAttributes("CanNavigateToPreviousPage", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCarouselPanel_P_CanNavigateToPreviousPage"));
				callbackBuilder.AddCustomAttributes("CanNavigateToNextItem", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCarouselPanel_P_CanNavigateToNextItem"));
				callbackBuilder.AddCustomAttributes("CanNavigateToNextPage", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCarouselPanel_P_CanNavigateToNextPage"));
				callbackBuilder.AddCustomAttributes("FirstVisibleItemIndex", CreateCategory("LC_Data"), CreateDescription("LD_XamCarouselPanel_P_FirstVisibleItemIndex"));
				callbackBuilder.AddCustomAttributes("OpacityEffectStopsResolved", CreateDescription("LD_XamCarouselPanel_P_OpacityEffectStopsResolved"));
				callbackBuilder.AddCustomAttributes("OpacityEffectStopDirectionResolved", CreateDescription("LD_XamCarouselPanel_P_OpacityEffectStopDirectionResolved"));
				callbackBuilder.AddCustomAttributes("ReflectionVisibility", CreateDescription("LD_XamCarouselPanel_P_ReflectionVisibility"));
				callbackBuilder.AddCustomAttributes("ScalingEffectStopsResolved", CreateDescription("LD_XamCarouselPanel_P_ScalingEffectStopsResolved"));
				callbackBuilder.AddCustomAttributes("ScalingEffectStopDirectionResolved", CreateDescription("LD_XamCarouselPanel_P_ScalingEffectStopDirectionResolved"));
				callbackBuilder.AddCustomAttributes("SkewAngleXEffectStopsResolved", CreateDescription("LD_XamCarouselPanel_P_SkewAngleXEffectStopsResolved"));
				callbackBuilder.AddCustomAttributes("SkewAngleXEffectStopDirectionResolved", CreateDescription("LD_XamCarouselPanel_P_SkewAngleXEffectStopDirectionResolved"));
				callbackBuilder.AddCustomAttributes("SkewAngleYEffectStopsResolved", CreateDescription("LD_XamCarouselPanel_P_SkewAngleYEffectStopsResolved"));
				callbackBuilder.AddCustomAttributes("SkewAngleYEffectStopDirectionResolved", CreateDescription("LD_XamCarouselPanel_P_SkewAngleYEffectStopDirectionResolved"));
				callbackBuilder.AddCustomAttributes("ViewSettings", CreateCategory("LC_Appearance"), CreateDescription("LD_XamCarouselPanel_P_ViewSettings"));
				callbackBuilder.AddCustomAttributes("ZOrderEffectStopsResolved", CreateDescription("LD_XamCarouselPanel_P_ZOrderEffectStopsResolved"));
				callbackBuilder.AddCustomAttributes("ZOrderEffectStopDirectionResolved", CreateDescription("LD_XamCarouselPanel_P_ZOrderEffectStopDirectionResolved"));
				callbackBuilder.AddCustomAttributes("ExecutingCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCarouselPanel_E_ExecutingCommand"));
				callbackBuilder.AddCustomAttributes("ExecutedCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCarouselPanel_E_ExecutedCommand"));
			});

			// Infragistics.Windows.Controls.CarouselViewSettings
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.CarouselViewSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AutoScaleItemContentsToFit", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_AutoScaleItemContentsToFit"));
				callbackBuilder.AddCustomAttributes("HeightInInfiniteContainers", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_HeightInInfiniteContainers"));
				callbackBuilder.AddCustomAttributes("IsListContinuous", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_IsListContinuous"));
				callbackBuilder.AddCustomAttributes("IsNavigatorVisible", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_IsNavigatorVisible"));
				callbackBuilder.AddCustomAttributes("ItemHorizontalScrollBarVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemHorizontalScrollBarVisibility"));
				callbackBuilder.AddCustomAttributes("ItemVerticalScrollBarVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemVerticalScrollBarVisibility"));
				callbackBuilder.AddCustomAttributes("ItemPath", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemPath"));
				callbackBuilder.AddCustomAttributes("ItemPathAutoPad", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemPathAutoPad"));
				callbackBuilder.AddCustomAttributes("ItemPathHorizontalAlignment", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_ItemPathHorizontalAlignment"));
				callbackBuilder.AddCustomAttributes("ItemPathPadding", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemPathPadding"));
				callbackBuilder.AddCustomAttributes("ItemPathStretch", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemPathStretch"));
				callbackBuilder.AddCustomAttributes("ItemPathPrefixPercent", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemPathPrefixPercent"));
				callbackBuilder.AddCustomAttributes("ItemPathSuffixPercent", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemPathSuffixPercent"));
				callbackBuilder.AddCustomAttributes("ItemPathRenderBrush", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemPathRenderBrush"));
				callbackBuilder.AddCustomAttributes("ItemPathRenderPen", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemPathRenderPen"));
				callbackBuilder.AddCustomAttributes("ItemPathVerticalAlignment", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_ItemPathVerticalAlignment"));
				callbackBuilder.AddCustomAttributes("ItemSize", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemSize"));
				callbackBuilder.AddCustomAttributes("ItemsPerPage", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_ItemsPerPage"));
				callbackBuilder.AddCustomAttributes("ItemTransitionStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ItemTransitionStyle"));
				callbackBuilder.AddCustomAttributes("OpacityEffectStops", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_OpacityEffectStops"));
				callbackBuilder.AddCustomAttributes("OpacityEffectStopDirection", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_OpacityEffectStopDirection"));
				callbackBuilder.AddCustomAttributes("CarouselPanelNavigatorStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_CarouselPanelNavigatorStyle"));
				callbackBuilder.AddCustomAttributes("ReserveSpaceForReflections", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ReserveSpaceForReflections"));
				callbackBuilder.AddCustomAttributes("RotateItemsWithPathTangent", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_RotateItemsWithPathTangent"));
				callbackBuilder.AddCustomAttributes("ScalingEffectStops", CreateCategory("LC_Data"), CreateDescription("LD_CarouselViewSettings_P_ScalingEffectStops"));
				callbackBuilder.AddCustomAttributes("ScalingEffectStopDirection", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ScalingEffectStopDirection"));

				// JM 06-10-08 - TFS Work Item #4472
				callbackBuilder.AddCustomAttributes("ShouldAnimateItemsOnListChange", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_ShouldAnimateItemsOnListChange"));

				callbackBuilder.AddCustomAttributes("ShouldScrollItemsIntoInitialPosition", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_ShouldScrollItemsIntoInitialPosition"));
				callbackBuilder.AddCustomAttributes("SkewAngleXEffectStops", CreateCategory("LC_Data"), CreateDescription("LD_CarouselViewSettings_P_SkewAngleXEffectStops"));
				callbackBuilder.AddCustomAttributes("SkewAngleXEffectStopDirection", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_SkewAngleXEffectStopDirection"));
				callbackBuilder.AddCustomAttributes("SkewAngleYEffectStops", CreateCategory("LC_Data"), CreateDescription("LD_CarouselViewSettings_P_SkewAngleYEffectStops"));
				callbackBuilder.AddCustomAttributes("SkewAngleYEffectStopDirection", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_SkewAngleYEffectStopDirection"));
				callbackBuilder.AddCustomAttributes("UseOpacity", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_UseOpacity"));
				callbackBuilder.AddCustomAttributes("UseScaling", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_UseScaling"));
				callbackBuilder.AddCustomAttributes("UseSkewing", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_UseSkewing"));
				callbackBuilder.AddCustomAttributes("UseZOrder", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselViewSettings_P_UseZOrder"));
				callbackBuilder.AddCustomAttributes("WidthInInfiniteContainers", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_WidthInInfiniteContainers"));
				callbackBuilder.AddCustomAttributes("ZOrderEffectStops", CreateCategory("LC_Data"), CreateDescription("LD_CarouselViewSettings_P_ZOrderEffectStops"));
				callbackBuilder.AddCustomAttributes("ZOrderEffectStopDirection", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselViewSettings_P_ZOrderEffectStopDirection"));
			});

			// Infragistics.Windows.Controls.XamCarouselListBox
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.XamCarouselListBox), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamCarouselListBox"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamCarouselListBoxAssetLibrary")));


				callbackBuilder.AddCustomAttributes("ViewSettings", CreateCategory("LC_Appearance"), CreateDescription("LD_XamCarouselListBox_P_ViewSettings"));
			});



			// AS 1/7/08 NA 2007 Vol 2


			// Infragistics.Windows.Controls.AutoDisabledImage
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.AutoDisabledImage), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("DisabledSource", CreateCategory("LC_Appearance"), CreateDescription("LD_AutoDisabledImage_P_DisabledSource"));
			});

			// Infragistics.Windows.Controls.PopupResizerBar
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.PopupResizerBar), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("ResizeMode", CreateCategory("LC_Behavior"), CreateDescription("LD_PopupResizerBar_P_ResizeMode"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Behavior"), CreateDescription("LD_PopupResizerBar_P_Location"));
			});

			// Infragistics.Windows.Controls.PopupResizerDecorator
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.PopupResizerDecorator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("ResizeMode", CreateCategory("LC_Behavior"), CreateDescription("LD_PopupResizerDecorator_P_ResizeMode"));
				callbackBuilder.AddCustomAttributes("ResizerBarLocation", CreateCategory("LC_Behavior"), CreateDescription("LD_PopupResizerDecorator_P_ResizerBarLocation"));
				callbackBuilder.AddCustomAttributes("ResizerBarStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_PopupResizerDecorator_P_ResizerBarStyle"));
			});

			// Infragistics.Windows.Controls.TabItemEx
			// =======================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.TabItemEx), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
                
                callbackBuilder.AddCustomAttributes("IsMouseOverTab", CreateCategory("LC_Appearance"), CreateDescription("LD_TabItemEx_P_IsMouseOverTab"));
            });

			// Infragistics.Windows.Controls.TabItemPanel
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.TabItemPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("MaximumSizeToFitAdjustment", CreateCategory("LC_Layout"), CreateDescription("LD_TabItemPanel_P_MaximumSizeToFitAdjustment"));
				callbackBuilder.AddCustomAttributes("MinimumTabExtent", CreateCategory("LC_Layout"), CreateDescription("LD_TabItemPanel_P_MinimumTabExtent"));
				callbackBuilder.AddCustomAttributes("TabLayoutStyle", CreateCategory("LC_Layout"), CreateDescription("LD_TabItemPanel_P_TabLayoutStyle"));
				callbackBuilder.AddCustomAttributes("TabStripPlacement", CreateCategory("LC_Layout"), CreateDescription("LD_TabItemPanel_P_TabStripPlacement"));
			});



			// Infragistics.Windows.Themes.ResourceWasher
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.Themes.ResourceWasher), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AutoWash", CreateCategory("LC_Behavior"), CreateDescription("LD_ResourceWasher_P_AutoWash"));
				callbackBuilder.AddCustomAttributes("SourceDictionary", CreateCategory("LC_Data"), CreateDescription("LD_ResourceWasher_P_SourceDictionary"));
				callbackBuilder.AddCustomAttributes("WashColor", CreateCategory("LC_Appearance"), CreateDescription("LD_ResourceWasher_P_WashColor"));
				callbackBuilder.AddCustomAttributes("WashMode", CreateCategory("LC_Appearance"), CreateDescription("LD_ResourceWasher_P_WashMode"));
			});

			// Infragistics.Windows.Virtualization.RecyclingItemsControl
			// =========================================================
			builder.AddCallback(typeof(Infragistics.Windows.Virtualization.RecyclingItemsControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ItemContainerGenerationMode", CreateCategory("LC_Behavior"), CreateDescription("LD_RecyclingItemsControl_P_ItemContainerGenerationMode"));
			});


			// Infragistics.Windows.Controls.XamCarouselListBox
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.XamCarouselListBox), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsSynchronizedWithCurrentItem", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCarouselListBox_P_IsSynchronizedWithCurrentItem"));
				callbackBuilder.AddCustomAttributes("SelectedIndex", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCarouselListBox_P_SelectedIndex"));
				callbackBuilder.AddCustomAttributes("SelectedItem", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCarouselListBox_P_SelectedItem"));
				callbackBuilder.AddCustomAttributes("SelectedValue", CreateCategory("LC_Data"), CreateDescription("LD_XamCarouselListBox_P_SelectedValue"));
				callbackBuilder.AddCustomAttributes("SelectedValuePath", CreateCategory("LC_Data"), CreateDescription("LD_XamCarouselListBox_P_SelectedValuePath"));
				callbackBuilder.AddCustomAttributes("SelectionChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCarouselListBox_E_SelectionChanged"));
			});



			// Infragistics.Windows.Controls.XamPager
			// ======================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.XamPager), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("ScrollLeftVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_XamPager_P_ScrollLeftVisibility"));
				callbackBuilder.AddCustomAttributes("ScrollRightVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_XamPager_P_ScrollRightVisibility"));
				callbackBuilder.AddCustomAttributes("ScrollUpVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_XamPager_P_ScrollUpVisibility"));
				callbackBuilder.AddCustomAttributes("ScrollDownVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_XamPager_P_ScrollDownVisibility"));
				callbackBuilder.AddCustomAttributes("ScrollLeftButtonStyle", CreateCategory("LC_Behavior"), CreateDescription("LD_XamPager_P_ScrollLeftButtonStyle"));
				callbackBuilder.AddCustomAttributes("ScrollRightButtonStyle", CreateCategory("LC_Behavior"), CreateDescription("LD_XamPager_P_ScrollRightButtonStyle"));
				callbackBuilder.AddCustomAttributes("ScrollUpButtonStyle", CreateCategory("LC_Behavior"), CreateDescription("LD_XamPager_P_ScrollUpButtonStyle"));
				callbackBuilder.AddCustomAttributes("ScrollDownButtonStyle", CreateCategory("LC_Behavior"), CreateDescription("LD_XamPager_P_ScrollDownButtonStyle"));
			});



			// Infragistics.Windows.Controls.XamScreenTip
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.XamScreenTip), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("Footer", CreateCategory("LC_Data"), CreateDescription("LD_XamScreenTip_P_Footer"));
				callbackBuilder.AddCustomAttributes("FooterSeparatorVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_XamScreenTip_P_FooterSeparatorVisibility"));
				callbackBuilder.AddCustomAttributes("FooterTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_XamScreenTip_P_FooterTemplate"));
				callbackBuilder.AddCustomAttributes("FooterTemplateSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_XamScreenTip_P_FooterTemplateSelector"));
				callbackBuilder.AddCustomAttributes("Header", CreateCategory("LC_Data"), CreateDescription("LD_XamScreenTip_P_Header"));
				callbackBuilder.AddCustomAttributes("HeaderSeparatorVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_XamScreenTip_P_HeaderSeparatorVisibility"));
				callbackBuilder.AddCustomAttributes("HeaderTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_XamScreenTip_P_HeaderTemplate"));
				callbackBuilder.AddCustomAttributes("HeaderTemplateSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_XamScreenTip_P_HeaderTemplateSelector"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_Appearance"), CreateDescription("LD_XamScreenTip_P_Theme"));
				callbackBuilder.AddCustomAttributes("ThemeChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_XamScreenTip_E_ThemeChanged"));
			});

			// Infragistics.Windows.Controls.XamTabControl
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.XamTabControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamTabControl"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamTabControlAssetLibrary")));


				callbackBuilder.AddCustomAttributes("AllowMinimize", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_AllowMinimize"));
				callbackBuilder.AddCustomAttributes("IsDropDownOpen", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_IsDropDownOpen"));
				callbackBuilder.AddCustomAttributes("IsMinimized", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_IsMinimized"));
				callbackBuilder.AddCustomAttributes("IsTabItemPanelScrolling", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_IsTabItemPanelScrolling"));
				callbackBuilder.AddCustomAttributes("MaximumSizeToFitAdjustment", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_MaximumSizeToFitAdjustment"));
				callbackBuilder.AddCustomAttributes("MinimumTabExtent", CreateCategory("LC_Appearance"), CreateDescription("LD_XamTabControl_P_MinimumTabExtent"));
				callbackBuilder.AddCustomAttributes("PostTabItemContent", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_PostTabItemContent"));
				callbackBuilder.AddCustomAttributes("PostTabItemContentTemplate", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_PostTabItemContentTemplate"));
				callbackBuilder.AddCustomAttributes("PostTabItemContentTemplateSelector", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_PostTabItemContentTemplateSelector"));
				callbackBuilder.AddCustomAttributes("PreTabItemContent", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_PreTabItemContent"));
				callbackBuilder.AddCustomAttributes("PreTabItemContentTemplate", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_PreTabItemContentTemplate"));
				callbackBuilder.AddCustomAttributes("PreTabItemContentTemplateSelector", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_PreTabItemContentTemplateSelector"));
				callbackBuilder.AddCustomAttributes("TabItemContentHeight", CreateCategory("LC_Layout"), CreateDescription("LD_XamTabControl_P_TabItemContentHeight"));
				callbackBuilder.AddCustomAttributes("TabLayoutStyle", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_TabLayoutStyle"));
				callbackBuilder.AddCustomAttributes("DropDownClosed", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_E_DropDownClosed"));
				callbackBuilder.AddCustomAttributes("DropDownOpened", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_E_DropDownOpened"));
				callbackBuilder.AddCustomAttributes("DropDownOpening", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_E_DropDownOpening"));

				// JM 02-10-10 - Comment out
				// JM BR28082 11-09-07
				//callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});



			// AS 10/22/09 TFS24142

			// Infragistics.Windows.Controls.SynchronizedSizeDecorator
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.SynchronizedSizeDecorator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("SynchronizeHeight", CreateCategory("LC_Behavior"), CreateDescription("LD_SynchronizedSizeDecorator_P_SynchronizeHeight"));
				callbackBuilder.AddCustomAttributes("SynchronizeWidth", CreateCategory("LC_Behavior"), CreateDescription("LD_SynchronizedSizeDecorator_P_SynchronizeWidth"));
				callbackBuilder.AddCustomAttributes("Target", CreateCategory("LC_Behavior"), CreateDescription("LD_SynchronizedSizeDecorator_P_Target"));
			});



			#endregion //Description, Category, etc.

			#region NA 2008 Vol 1

			// Infragistics.Windows.Themes.ResourceSetLoader
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Themes.ResourceSetLoader), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Locator", CreateCategory("LC_Data"), CreateDescription("LD_ResourceSetLoader_P_Locator"));
			});


			// Infragistics.Windows.Controls.ToolWindowResizeElement
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.ToolWindowResizeElement), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("BorderLocation", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindowResizeElement_P_BorderLocation"));
			});

			// Infragistics.Windows.Controls.ToolWindow
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.ToolWindow), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("AllowsTransparency", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_AllowsTransparency")); // AS 1/20/11 TFS63757
				callbackBuilder.AddCustomAttributes("AllowClose", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_AllowClose"));
				callbackBuilder.AddCustomAttributes("HorizontalAlignmentMode", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_HorizontalAlignmentMode"));
				callbackBuilder.AddCustomAttributes("HorizontalAlignmentOffset", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_HorizontalAlignmentOffset"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsUsingOSNonClientArea", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_IsUsingOSNonClientArea"));
				callbackBuilder.AddCustomAttributes("Left", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_Left"));
				callbackBuilder.AddCustomAttributes("Owner", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_Owner"));
				callbackBuilder.AddCustomAttributes("ResizeMode", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_ResizeMode"));
				callbackBuilder.AddCustomAttributes("PreferredBorderThickness", CreateCategory("LC_Appearance"), CreateDescription("LD_ToolWindow_P_PreferredBorderThickness"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_Appearance"), CreateDescription("LD_ToolWindow_P_Theme"));
				callbackBuilder.AddCustomAttributes("Title", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_Title"));
				callbackBuilder.AddCustomAttributes("Top", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_Top"));
				callbackBuilder.AddCustomAttributes("Topmost", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_Topmost"));
				callbackBuilder.AddCustomAttributes("UseOSNonClientArea", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_UseOSNonClientArea"));
				callbackBuilder.AddCustomAttributes("VerticalAlignmentMode", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_VerticalAlignmentMode"));
				callbackBuilder.AddCustomAttributes("VerticalAlignmentOffset", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_VerticalAlignmentOffset"));
				callbackBuilder.AddCustomAttributes("ThemeChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_E_ThemeChanged"));
				callbackBuilder.AddCustomAttributes("Activated", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_E_Activated"));
				callbackBuilder.AddCustomAttributes("Deactivated", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_E_Deactivated"));
				callbackBuilder.AddCustomAttributes("Closing", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_E_Closing"));
				callbackBuilder.AddCustomAttributes("Closed", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_E_Closed"));

				// AS 3/31/11 NA 2011.1
				callbackBuilder.AddCustomAttributes("AllowMinimize", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_AllowMinimize"));
				callbackBuilder.AddCustomAttributes("AllowMaximize", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_AllowMaximize"));
				callbackBuilder.AddCustomAttributes("MaximizeButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_MaximizeButtonVisibility"));
				callbackBuilder.AddCustomAttributes("MinimizeButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_MinimizeButtonVisibility"));
				callbackBuilder.AddCustomAttributes("ShowInTaskbar", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_ShowInTaskbar"));
				callbackBuilder.AddCustomAttributes("WindowStartupLocation", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_WindowStartupLocation"));
				callbackBuilder.AddCustomAttributes("WindowState", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_WindowState"));
				callbackBuilder.AddCustomAttributes("IsOwnedWindow", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_IsOwnedWindow"));
			});



			#endregion //NA 2008 Vol 1

			#region NA 2008 Vol 2

			// Infragistics.Windows.Reporting.ReportSettings
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Reporting.ReportSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FileName", CreateCategory("LC_Data"), CreateDescription("LD_ReportSettings_P_FileName"));
				callbackBuilder.AddCustomAttributes("HorizontalPaginationMode", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportSettings_P_HorizontalPaginationMode"));
				callbackBuilder.AddCustomAttributes("Margin", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportSettings_P_Margin"));
				callbackBuilder.AddCustomAttributes("PageOrientation", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportSettings_P_PageOrientation"));
				callbackBuilder.AddCustomAttributes("PagePrintOrder", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportSettings_P_PagePrintOrder"));
				callbackBuilder.AddCustomAttributes("PageRange", CreateCategory("LC_Data"), CreateDescription("LD_ReportSettings_P_PageRange"));
				callbackBuilder.AddCustomAttributes("UserPageRangeEnabled", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportSettings_P_UserPageRangeEnabled"));
				callbackBuilder.AddCustomAttributes("PageSize", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportSettings_P_PageSize"));
				callbackBuilder.AddCustomAttributes("RepeatType", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportSettings_P_RepeatType"));
				callbackBuilder.AddCustomAttributes("PrintQueue", CreateCategory("LC_Data"), CreateDescription("LD_ReportSettings_P_PrintQueue"));
			});

			// Infragistics.Windows.Reporting.ReportSection
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Reporting.ReportSection), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsEndReached", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportSection_P_IsEndReached"));
				callbackBuilder.AddCustomAttributes("LogicalPageNumber", CreateCategory("LC_Data"), CreateDescription("LD_ReportSection_P_LogicalPageNumber"));
				callbackBuilder.AddCustomAttributes("LogicalPagePartNumber", CreateCategory("LC_Data"), CreateDescription("LD_ReportSection_P_LogicalPagePartNumber"));
				callbackBuilder.AddCustomAttributes("PageContentTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportSection_P_PageContentTemplate"));
				callbackBuilder.AddCustomAttributes("PageContentTemplateSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportSection_P_PageContentTemplateSelector"));
				callbackBuilder.AddCustomAttributes("PageFooter", CreateCategory("LC_Data"), CreateDescription("LD_ReportSection_P_PageFooter"));
				callbackBuilder.AddCustomAttributes("PageFooterTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportSection_P_PageFooterTemplate"));
				callbackBuilder.AddCustomAttributes("PageFooterTemplateSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportSection_P_PageFooterTemplateSelector"));
				callbackBuilder.AddCustomAttributes("PageHeader", CreateCategory("LC_Data"), CreateDescription("LD_ReportSection_P_PageHeader"));
				callbackBuilder.AddCustomAttributes("PageHeaderTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportSection_P_PageHeaderTemplate"));
				callbackBuilder.AddCustomAttributes("PageHeaderTemplateSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportSection_P_PageHeaderTemplateSelector"));
				callbackBuilder.AddCustomAttributes("PagePresenterStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportSection_P_PagePresenterStyle"));
				callbackBuilder.AddCustomAttributes("PhysicalPageNumber", CreateCategory("LC_Data"), CreateDescription("LD_ReportSection_P_PhysicalPageNumber"));
			});

			// Infragistics.Windows.Controls.TabItemPanel
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.TabItemPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("InterRowSpacing", CreateCategory("LC_Layout"), CreateDescription("LD_TabItemPanel_P_InterRowSpacing"));
				callbackBuilder.AddCustomAttributes("InterTabSpacing", CreateCategory("LC_Layout"), CreateDescription("LD_TabItemPanel_P_InterTabSpacing"));
				callbackBuilder.AddCustomAttributes("MaximumTabRows", CreateCategory("LC_Behavior"), CreateDescription("LD_TabItemPanel_P_MaximumTabRows"));
			});

			// Infragistics.Windows.Controls.TabItemEx
			// =======================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.TabItemEx), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowClosing", CreateCategory("LC_Behavior"), CreateDescription("LD_TabItemEx_P_AllowClosing"));
				callbackBuilder.AddCustomAttributes("CloseButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_TabItemEx_P_CloseButtonVisibility"));
				callbackBuilder.AddCustomAttributes("ComputedCloseButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_TabItemEx_P_ComputedCloseButtonVisibility"));
				callbackBuilder.AddCustomAttributes("Closing", CreateCategory("LC_Behavior"), CreateDescription("LD_TabItemEx_E_Closing"));
				callbackBuilder.AddCustomAttributes("Closed", CreateCategory("LC_Behavior"), CreateDescription("LD_TabItemEx_E_Closed"));
			});

			// JJD 4/11/11 - TFS72200
			// Keep the base UniformGrid class out on the toolbox as well.
			// Infragistics.Windows.Controls.UniformGrid
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Controls.Primitives.UniformGrid), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("Columns", CreateCategory("LC_Layout"), CreateDescription("LD_UniformGridEx_P_Columns"));
				callbackBuilder.AddCustomAttributes("FirstColumn", CreateCategory("LC_Layout"), CreateDescription("LD_UniformGridEx_P_FirstColumn"));
				callbackBuilder.AddCustomAttributes("FirstRow", CreateCategory("LC_Layout"), CreateDescription("LD_UniformGridEx_P_FirstRow"));
				callbackBuilder.AddCustomAttributes("Rows", CreateCategory("LC_Layout"), CreateDescription("LD_UniformGridEx_P_Rows"));
			});
			
			// Infragistics.Windows.Controls.UniformGridEx
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.UniformGridEx), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.Controls.XamTabControl
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.XamTabControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsMultiRow", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_IsMultiRow"));
				callbackBuilder.AddCustomAttributes("AllowTabClosing", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_AllowTabClosing"));
				callbackBuilder.AddCustomAttributes("DropDownAnimation", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_DropDownAnimation"));
				callbackBuilder.AddCustomAttributes("InterRowSpacing", CreateCategory("LC_Layout"), CreateDescription("LD_XamTabControl_P_InterRowSpacing"));
				callbackBuilder.AddCustomAttributes("InterTabSpacing", CreateCategory("LC_Layout"), CreateDescription("LD_XamTabControl_P_InterTabSpacing"));
				callbackBuilder.AddCustomAttributes("MaximumTabRows", CreateCategory("LC_Layout"), CreateDescription("LD_XamTabControl_P_MaximumTabRows"));
				callbackBuilder.AddCustomAttributes("ShowTabHeaderCloseButton", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_ShowTabHeaderCloseButton"));
				callbackBuilder.AddCustomAttributes("TabItemCloseButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTabControl_P_TabItemCloseButtonVisibility"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_Appearance"), CreateDescription("LD_XamTabControl_P_Theme"));
				callbackBuilder.AddCustomAttributes("ThemeChanged", CreateDescription("LD_XamTabControl_E_ThemeChanged"));
				callbackBuilder.AddCustomAttributes("Items", new NewItemTypesAttribute(new Type[] { typeof(TabItemEx) }));
			});

			// Infragistics.Windows.Controls.ToolWindow
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.ToolWindow), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DialogResult", CreateCategory("LC_Behavior"), CreateDescription("LD_ToolWindow_P_DialogResult"));
			});

			// Infragistics.Windows.Controls.DropIndicator
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.DropIndicator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
                
                callbackBuilder.AddCustomAttributes("DropLocation", CreateCategory("LC_Behavior"), CreateDescription("LD_DropIndicator_P_DropLocation"));
				callbackBuilder.AddCustomAttributes("DropTargetHeight", CreateCategory("LC_Appearance"), CreateDescription("LD_DropIndicator_P_DropTargetHeight"));
				callbackBuilder.AddCustomAttributes("DropTargetWidth", CreateCategory("LC_Appearance"), CreateDescription("LD_DropIndicator_P_DropTargetWidth"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Behavior"), CreateDescription("LD_DropIndicator_P_Orientation"));
			});

			// Infragistics.Windows.Reporting.ReportBase
			// =========================================
			builder.AddCallback(typeof(Infragistics.Windows.Reporting.ReportBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("PageContentTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportBase_P_PageContentTemplate"));
				callbackBuilder.AddCustomAttributes("PageContentTemplateSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportBase_P_PageContentTemplateSelector"));
				callbackBuilder.AddCustomAttributes("PageFooter", CreateCategory("LC_Data"), CreateDescription("LD_ReportBase_P_PageFooter"));
				callbackBuilder.AddCustomAttributes("PageFooterTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportBase_P_PageFooterTemplate"));
				callbackBuilder.AddCustomAttributes("PageFooterTemplateSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportBase_P_PageFooterTemplateSelector"));
				callbackBuilder.AddCustomAttributes("PageHeader", CreateCategory("LC_Data"), CreateDescription("LD_ReportBase_P_PageHeader"));
				callbackBuilder.AddCustomAttributes("PageHeaderTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportBase_P_PageHeaderTemplate"));
				callbackBuilder.AddCustomAttributes("PageHeaderTemplateSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportBase_P_PageHeaderTemplateSelector"));
				callbackBuilder.AddCustomAttributes("PagePresenterStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportBase_P_PagePresenterStyle"));
				callbackBuilder.AddCustomAttributes("ReportSettings", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportBase_P_ReportSettings"));
			});


			#endregion //NA 2008 Vol 2

			#region NA 2009 Vol 1

			// Infragistics.Windows.Controls.ComparisonOperatorListItem
			// ========================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.ComparisonOperatorListItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Image", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorListItem_P_Image"));
				callbackBuilder.AddCustomAttributes("Description", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorListItem_P_Description"));
			});

			// Infragistics.Windows.Controls.ComparisonOperatorSelector
			// ========================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.ComparisonOperatorSelector), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("AllowableOperators", CreateCategory("LC_Behavior"), CreateDescription("LD_ComparisonOperatorSelector_P_AllowableOperators"));
				callbackBuilder.AddCustomAttributes("DropDownButtonStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_DropDownButtonStyle"));
				callbackBuilder.AddCustomAttributes("IsDropDownOpen", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_IsDropDownOpen"));
				callbackBuilder.AddCustomAttributes("SelectedIndex", CreateCategory("LC_Behavior"), CreateDescription("LD_ComparisonOperatorSelector_P_SelectedIndex"));
				callbackBuilder.AddCustomAttributes("SelectedOperator", CreateCategory("LC_Behavior"), CreateDescription("LD_ComparisonOperatorSelector_P_SelectedOperator"));
				callbackBuilder.AddCustomAttributes("SelectedOperatorInfo", CreateCategory("LC_Behavior"), CreateDescription("LD_ComparisonOperatorSelector_P_SelectedOperatorInfo"));
				callbackBuilder.AddCustomAttributes("OperatorEqualsImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorEqualsImage"));
				callbackBuilder.AddCustomAttributes("OperatorNotEqualsImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorNotEqualsImage"));
				callbackBuilder.AddCustomAttributes("OperatorLessThanImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorLessThanImage"));
				callbackBuilder.AddCustomAttributes("OperatorLessThanOrEqualToImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorLessThanOrEqualToImage"));
				callbackBuilder.AddCustomAttributes("OperatorGreaterThanImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorGreaterThanImage"));
				callbackBuilder.AddCustomAttributes("OperatorGreaterThanOrEqualToImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorGreaterThanOrEqualToImage"));
				callbackBuilder.AddCustomAttributes("OperatorContainsImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorContainsImage"));
				callbackBuilder.AddCustomAttributes("OperatorDoesNotContainImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorDoesNotContainImage"));
				callbackBuilder.AddCustomAttributes("OperatorLikeImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorLikeImage"));
				callbackBuilder.AddCustomAttributes("OperatorNotLikeImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorNotLikeImage"));
				callbackBuilder.AddCustomAttributes("OperatorMatchImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorMatchImage"));
				callbackBuilder.AddCustomAttributes("OperatorDoesNotMatchImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorDoesNotMatchImage"));
				callbackBuilder.AddCustomAttributes("OperatorStartsWithImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorStartsWithImage"));
				callbackBuilder.AddCustomAttributes("OperatorDoesNotStartWithImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorDoesNotStartWithImage"));
				callbackBuilder.AddCustomAttributes("OperatorEndsWithImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorEndsWithImage"));
				callbackBuilder.AddCustomAttributes("OperatorDoesNotEndWithImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorDoesNotEndWithImage"));
				callbackBuilder.AddCustomAttributes("OperatorTopImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorTopImage"));
				callbackBuilder.AddCustomAttributes("OperatorBottomImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorBottomImage"));
				callbackBuilder.AddCustomAttributes("OperatorTopPercentileImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorTopPercentileImage"));
				callbackBuilder.AddCustomAttributes("OperatorBottomPercentileImage", CreateCategory("LC_Appearance"), CreateDescription("LD_ComparisonOperatorSelector_P_OperatorBottomPercentileImage"));
				callbackBuilder.AddCustomAttributes("SelectedOperatorChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_ComparisonOperatorSelector_E_SelectedOperatorChanged"));
			});

			// Infragistics.Windows.Layout.GridBagConstraint
			// =============================================
			builder.AddCallback(typeof(Infragistics.Controls.Layouts.Primitives.GridBagConstraint), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Column", CreateDescription("LD_GridBagConstraint_P_Column"));
				callbackBuilder.AddCustomAttributes("ColumnSpan", CreateDescription("LD_GridBagConstraint_P_ColumnSpan"));
				callbackBuilder.AddCustomAttributes("ColumnWeight", CreateDescription("LD_GridBagConstraint_P_ColumnWeight"));
				callbackBuilder.AddCustomAttributes("Margin", CreateDescription("LD_GridBagConstraint_P_Margin"));
				callbackBuilder.AddCustomAttributes("Row", CreateDescription("LD_GridBagConstraint_P_Row"));
				callbackBuilder.AddCustomAttributes("RowSpan", CreateDescription("LD_GridBagConstraint_P_RowSpan"));
				callbackBuilder.AddCustomAttributes("RowWeight", CreateDescription("LD_GridBagConstraint_P_RowWeight"));
			});

			#endregion //NA 2009 Vol 1

			#region NA 2009 Vol 2

			// Infragistics.Windows.ExpanderDecorator
			// ======================================
			builder.AddCallback(typeof(Infragistics.Windows.ExpanderDecorator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("IsExpanded", CreateCategory("LC_Behavior"), CreateDescription("LD_ExpanderDecorator_P_IsExpanded"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Behavior"), CreateDescription("LD_ExpanderDecorator_P_Orientation"));
			});

			// Infragistics.Windows.Controls.DesiredSizeDecorator
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.DesiredSizeDecorator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("ChildDesiredHeight", CreateCategory("LC_Behavior"), CreateDescription("LD_DesiredSizeDecorator_P_ChildDesiredHeight"));
				callbackBuilder.AddCustomAttributes("ChildDesiredWidth", CreateCategory("LC_Behavior"), CreateDescription("LD_DesiredSizeDecorator_P_ChildDesiredWidth"));
			});

			// Infragistics.Windows.Controls.ExpansionIndicator
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.ExpansionIndicator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ToggleMode", CreateCategory("LC_Behavior"), CreateDescription("LD_ExpansionIndicator_P_ToggleMode"));
			});

			#endregion //NA 2009 Vol 2

			#region NA 2010 Vol 1

            // Infragistics.Windows.Tiles.TilesPanelBase
			// =========================================
			builder.AddCallback(typeof(Infragistics.Windows.Tiles.TilesPanelBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                callbackBuilder.AddCustomAttributes("IsAnimationInProgress", BrowsableAttribute.No);
			});

			#endregion //NA 2010 Vol 1

			#region NA 2011 Vol 2

			// Infragistics.Windows.Controls.PopupResizerStackPanel
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.PopupResizerStackPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});


			// Infragistics.Controls.Primitives.PopupResizer
			// =============================================
			builder.AddCallback(typeof(Infragistics.Controls.Primitives.PopupResizer), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			#endregion //NA 2011 Vol 2

			#region ToolboxBrowsable

			// AS 1/8/08 ToolboxBrowsable

			// JM 1-30-09 TFS13196 - The AutoDisabledImage class actually exists in the Express version so move the Attribute definition
			//						 outside the #if/#endif
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.AutoDisabledImage), ToolboxBrowsableAttribute.No);

			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.CarouselListBoxItem), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.CarouselPanelNavigator), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.PopupResizerBar), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.PopupResizerDecorator), ToolboxBrowsableAttribute.No);
			// JJD 10/20/08 - Removed ToolboxBrowsableAttribute.No in 8.2 since this is now a supported control
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.TabItemEx), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.TabItemPanel), ToolboxBrowsableAttribute.No);
			// JJD 7/31/08 - Removed ToolboxBrowsableAttribute.No in 8.2 since this is now a supported control
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.XamTabControl), ToolboxBrowsableAttribute.No);

			// AS 1/22/08 BR29903
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.XamScreenTip), ToolboxBrowsableAttribute.No);

			// AS 5/20/08 BR33090
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.ToolWindow), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.ToolWindowResizeElement), ToolboxBrowsableAttribute.No);

			// JM 06-24-08 BR34190
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.CarouselPanelItem), ToolboxBrowsableAttribute.No);

			// AS 7/13/09 TFS18489
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.DesiredSizeDecorator), ToolboxBrowsableAttribute.No);

			// AS 4/15/10
			// This attribute wasn't added.
			//
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.GrowOnlyDecorator), ToolboxBrowsableAttribute.No);

			// AS 10/22/09 TFS24142
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.SynchronizedSizeDecorator), ToolboxBrowsableAttribute.No);

			// JM 10-08-08 TFS8777 - The DropIndicator class actually exists in the Express version so move the Attribute definition
			//						 outside the #if/#endif
			// JM 10-06-08 TFS8667
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.UniformGridEx), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.DropIndicator), ToolboxBrowsableAttribute.No);



			// Infragistics.Controls.Primitives.ExpansionIndicator
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Controls.Primitives.ExpansionIndicator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

            });
			// AS 5/27/10
			//builder.AddCustomAttributes(typeof(Infragistics.Controls.Primitives.ExpansionIndicator), ToolboxBrowsableAttribute.No);




			// Infragistics.Windows.Controls.GrowOnlyDecorator
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.GrowOnlyDecorator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

            });

            // Infragistics.Windows.Controls.CarouselPanelAdorner
			// ===============================================
            builder.AddCallback(typeof(Infragistics.Windows.Controls.CarouselPanelAdorner), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

            });

            // Infragistics.Windows.Controls.CardPanel
			// =======================================
            builder.AddCallback(typeof(Infragistics.Windows.Controls.CardPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

            });

            // Infragistics.Windows.Controls.MousePanningDecorator
			// ===================================================
            builder.AddCallback(typeof(Infragistics.Windows.Controls.MousePanningDecorator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

            });

            // Infragistics.Windows.Controls.PagerContentPresenter
			// ===================================================
            builder.AddCallback(typeof(Infragistics.Windows.Controls.PagerContentPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

            });

            // Infragistics.Windows.Controls.TileDragIndicator
			// ===============================================
            builder.AddCallback(typeof(Infragistics.Windows.Tiles.TileDragIndicator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

            });

            // Infragistics.Windows.Controls.AutomationControl
			// ===============================================
            builder.AddCallback(typeof(Infragistics.Windows.Controls.AutomationControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

            });

			// Infragistics.Windows.Controls.AutomationControl
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.Controls.SimpleTextBlock), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 10/1/10
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

			});
			// AS 5/27/10
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.CarouselPanelAdorner), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.TabItemEx), ToolboxBrowsableAttribute.No);

			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.DropIndicator), ToolboxBrowsableAttribute.No);

			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.CardPanel), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.ExpanderBar), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.ExpansionIndicator), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.MousePanningDecorator), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.PagerContentPresenter), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.SortIndicator), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.XamPager), ToolboxBrowsableAttribute.No);

			// JJD 12/22/08 - Added ComparisonOperatorSelector for v9.1
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.ComparisonOperatorSelector), ToolboxBrowsableAttribute.No);

			// JM 01-30-09 TFS13196 - Moved here from '#if !EXPRESS' section above
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.AutoDisabledImage), ToolboxBrowsableAttribute.No);

			// JM 08-17-09 TFS20757
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.ExpanderDecorator), ToolboxBrowsableAttribute.No);

			// JM 08-27-09 TFS21522
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.AutomationControl), ToolboxBrowsableAttribute.No);

			// JJD 02-02-10 TFS27003
            //builder.AddCustomAttributes(typeof(Infragistics.Windows.Tiles.TileDragIndicator), ToolboxBrowsableAttribute.No);

			// MD 3/22/11 - TFS61722
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Controls.SimpleTextBlock), ToolboxBrowsableAttribute.No);

			// Infragistics.Controls.Primitives.EmbeddedTextBox
			// ================================================
			builder.AddCallback(typeof(Infragistics.Controls.Primitives.EmbeddedTextBox), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

			});

			#endregion //ToolboxBrowsable

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