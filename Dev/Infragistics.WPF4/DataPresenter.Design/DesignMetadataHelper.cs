using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;
using System.ComponentModel;
using Microsoft.Windows.Design.PropertyEditing;
using System.Windows;

namespace Infragistics.Windows.Design.DataPresenter
{

	// JM 01-06-10 VS2010 Designer Support
	#region DesignMetadataHelper Static Class

	internal static class DesignMetadataHelper
	{
		internal static AttributeTableBuilder GetAttributeTableBuilder()
		{
			AttributeTableBuilder builder = new AttributeTableBuilder();

			#region Description, Category, etc.

			// Infragistics.Windows.DataPresenter.VirtualizingDataRecordCellPanel
			// ==================================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.VirtualizingDataRecordCellPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// JJD 5/21/07
			// Infragistics.Windows.DataPresenter.CellPlaceholder
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CellPlaceholder), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});


			// Infragistics.Windows.DataPresenter.HeaderPrefixArea
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.HeaderPrefixArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_HeaderPrefixArea"));

				callbackBuilder.AddCustomAttributes("FieldLayout", CreateCategory("LC_Behavior"), CreateDescription("LD_HeaderPrefixArea_P_FieldLayout"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Appearance"), CreateDescription("LD_HeaderPrefixArea_P_Orientation"));
				callbackBuilder.AddCustomAttributes("Record", CreateCategory("LC_Data"), CreateDescription("LD_HeaderPrefixArea_P_Record"));
			});

			// Infragistics.Windows.DataPresenter.FieldSettings
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowCellVirtualization", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowCellVirtualization"));
				callbackBuilder.AddCustomAttributes("AllowLabelVirtualization", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowLabelVirtualization"));
				callbackBuilder.AddCustomAttributes("AllowEdit", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowEdit"));
				callbackBuilder.AddCustomAttributes("AllowGroupBy", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowGroupBy"));
				callbackBuilder.AddCustomAttributes("AllowResize", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowResize"));
				callbackBuilder.AddCustomAttributes("CellClickAction", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_CellClickAction"));
				callbackBuilder.AddCustomAttributes("CellContentAlignment", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_CellContentAlignment"));
				callbackBuilder.AddCustomAttributes("CellHeight", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_CellHeight"));
				callbackBuilder.AddCustomAttributes("CellMaxHeight", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_CellMaxHeight"));
				callbackBuilder.AddCustomAttributes("CellMaxWidth", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_CellMaxWidth"));
				callbackBuilder.AddCustomAttributes("CellMinHeight", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_CellMinHeight"));
				callbackBuilder.AddCustomAttributes("CellMinWidth", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_CellMinWidth"));
				callbackBuilder.AddCustomAttributes("CellPresenterStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_CellPresenterStyle"));
				callbackBuilder.AddCustomAttributes("CellPresenterStyleSelector", CreateDescription("LD_FieldSettings_P_CellPresenterStyleSelector"));
				callbackBuilder.AddCustomAttributes("CellValuePresenterStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_CellValuePresenterStyle"));
				callbackBuilder.AddCustomAttributes("CellValuePresenterStyleSelector", CreateDescription("LD_FieldSettings_P_CellValuePresenterStyleSelector"));
				callbackBuilder.AddCustomAttributes("CellWidth", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_CellWidth"));
				callbackBuilder.AddCustomAttributes("EditorStyle", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_EditorStyle"));
				callbackBuilder.AddCustomAttributes("EditorType", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_EditorType"));
				callbackBuilder.AddCustomAttributes("ExpandedCellStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_ExpandedCellStyle"));
				callbackBuilder.AddCustomAttributes("ExpandableFieldRecordExpansionMode", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_ExpandableFieldRecordExpansionMode"));
				callbackBuilder.AddCustomAttributes("ExpandableFieldRecordHeaderDisplayMode", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_ExpandableFieldRecordHeaderDisplayMode"));
				callbackBuilder.AddCustomAttributes("ExpandableFieldRecordPresenterStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_ExpandableFieldRecordPresenterStyle"));
				callbackBuilder.AddCustomAttributes("ExpandableFieldRecordPresenterStyleSelector", CreateDescription("LD_FieldSettings_P_ExpandableFieldRecordPresenterStyleSelector"));
				callbackBuilder.AddCustomAttributes("GroupByMode", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_GroupByMode"));
				callbackBuilder.AddCustomAttributes("GroupByRecordPresenterStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_GroupByRecordPresenterStyle"));
				callbackBuilder.AddCustomAttributes("GroupByRecordPresenterStyleSelector", CreateDescription("LD_FieldSettings_P_GroupByRecordPresenterStyleSelector"));
				callbackBuilder.AddCustomAttributes("InvalidValueBehavior", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_InvalidValueBehavior"));
				callbackBuilder.AddCustomAttributes("LabelClickAction", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_LabelClickAction"));
				callbackBuilder.AddCustomAttributes("LabelHeight", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_LabelHeight"));
				callbackBuilder.AddCustomAttributes("LabelMaxHeight", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_LabelMaxHeight"));
				callbackBuilder.AddCustomAttributes("LabelMaxWidth", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_LabelMaxWidth"));
				callbackBuilder.AddCustomAttributes("LabelMinHeight", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_LabelMinHeight"));
				callbackBuilder.AddCustomAttributes("LabelMinWidth", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_LabelMinWidth"));
				callbackBuilder.AddCustomAttributes("LabelPresenterStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_LabelPresenterStyle"));
				callbackBuilder.AddCustomAttributes("LabelPresenterStyleSelector", CreateDescription("LD_FieldSettings_P_LabelPresenterStyleSelector"));
				callbackBuilder.AddCustomAttributes("LabelTextAlignment", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_LabelTextAlignment"));
				callbackBuilder.AddCustomAttributes("LabelTextTrimming", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_LabelTextTrimming"));
				callbackBuilder.AddCustomAttributes("LabelTextWrapping", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_LabelTextWrapping"));
				callbackBuilder.AddCustomAttributes("LabelWidth", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_LabelWidth"));
				callbackBuilder.AddCustomAttributes("SortComparer", CreateDescription("LD_FieldSettings_P_SortComparer"));
				callbackBuilder.AddCustomAttributes("SortComparisonType", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_SortComparisonType"));
			});

			// Infragistics.Windows.DataPresenter.CellPresenter
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CellPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_CellPresenter"));

				callbackBuilder.AddCustomAttributes("ContentLocation", CreateCategory("LC_Behavior"), CreateDescription("LD_CellPresenter_P_ContentLocation"));
				callbackBuilder.AddCustomAttributes("IsFieldSelected", CreateCategory("LC_Behavior"), CreateDescription("LD_CellPresenter_P_IsFieldSelected"));
				callbackBuilder.AddCustomAttributes("IsUnbound", CreateCategory("LC_Behavior"), CreateDescription("LD_CellPresenter_P_IsUnbound"));
				callbackBuilder.AddCustomAttributes("SortStatus", CreateCategory("LC_Behavior"), CreateDescription("LD_CellPresenter_P_SortStatus"));
			});

			// Infragistics.Windows.DataPresenter.ViewBase
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.ViewBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ViewStateChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_ViewBase_E_ViewStateChanged"));
			});


			// Infragistics.Windows.DataPresenter.CarouselView
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CarouselView), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ViewSettings", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselView_P_ViewSettings"));
			});



			// Infragistics.Windows.DataPresenter.CellPresenterLayoutElement
			// =============================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CellPresenterLayoutElement), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_CellPresenterLayoutElement"));
			});


			// Infragistics.Windows.DataPresenter.DataPresenterBase
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataPresenterBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("ActiveCell", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_ActiveCell"));
				callbackBuilder.AddCustomAttributes("ActiveRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_ActiveRecord"));
				callbackBuilder.AddCustomAttributes("AutoFit", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_AutoFit"));
				callbackBuilder.AddCustomAttributes("BindToSampleData", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_P_BindToSampleData"));
				callbackBuilder.AddCustomAttributes("DataSource", CreateCategory("LC_Content"), CreateDescription("LD_DataPresenterBase_P_DataSource"));
				callbackBuilder.AddCustomAttributes("FieldLayoutSettings", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_FieldLayoutSettings"));
				callbackBuilder.AddCustomAttributes("FieldSettings", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_FieldSettings"));
				callbackBuilder.AddCustomAttributes("GroupByAreaFieldLabelStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_DataPresenterBase_P_GroupByAreaFieldLabelStyle"));
				callbackBuilder.AddCustomAttributes("GroupByAreaFieldLabelStyleSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_DataPresenterBase_P_GroupByAreaFieldLabelStyleSelector"));
				callbackBuilder.AddCustomAttributes("GroupByAreaLocation", CreateCategory("LC_Appearance"), CreateDescription("LD_DataPresenterBase_P_GroupByAreaLocation"));
				callbackBuilder.AddCustomAttributes("GroupByAreaStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_DataPresenterBase_P_GroupByAreaStyle"));
				callbackBuilder.AddCustomAttributes("IsGroupByAreaExpanded", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_IsGroupByAreaExpanded"));
				callbackBuilder.AddCustomAttributes("IsNestedDataDisplayEnabled", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_IsNestedDataDisplayEnabled"));
				callbackBuilder.AddCustomAttributes("RecordLoadMode", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_RecordLoadMode"));
				callbackBuilder.AddCustomAttributes("RecordManager", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_P_RecordManager"));
				callbackBuilder.AddCustomAttributes("Records", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_P_Records"));
				callbackBuilder.AddCustomAttributes("ScrollingMode", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_ScrollingMode"));
				callbackBuilder.AddCustomAttributes("SortRecordsByDataType", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_SortRecordsByDataType"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_Appearance"), CreateDescription("LD_DataPresenterBase_P_Theme"));
				callbackBuilder.AddCustomAttributes("UpdateMode", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_UpdateMode"));
				callbackBuilder.AddCustomAttributes("ViewableRecords", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_P_ViewableRecords"));
				callbackBuilder.AddCustomAttributes("AssigningFieldLayoutToItem", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_AssigningFieldLayoutToItem"));
				callbackBuilder.AddCustomAttributes("AutoFitChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_AutoFitChanged"));
				callbackBuilder.AddCustomAttributes("CellActivated", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_CellActivated"));
				callbackBuilder.AddCustomAttributes("CellActivating", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_CellActivating"));
				callbackBuilder.AddCustomAttributes("CellChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_CellChanged"));
				callbackBuilder.AddCustomAttributes("CellDeactivating", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_CellDeactivating"));
				callbackBuilder.AddCustomAttributes("CellUpdating", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_E_CellUpdating"));
				callbackBuilder.AddCustomAttributes("CellUpdated", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_E_CellUpdated"));
				callbackBuilder.AddCustomAttributes("DataError", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_DataError"));
				callbackBuilder.AddCustomAttributes("EditModeEnded", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_EditModeEnded"));
				callbackBuilder.AddCustomAttributes("EditModeEnding", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_EditModeEnding"));
				callbackBuilder.AddCustomAttributes("EditModeStarted", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_EditModeStarted"));
				callbackBuilder.AddCustomAttributes("EditModeStarting", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_EditModeStarting"));
				callbackBuilder.AddCustomAttributes("EditModeValidationError", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_EditModeValidationError"));
				callbackBuilder.AddCustomAttributes("ExecutingCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_ExecutingCommand"));
				callbackBuilder.AddCustomAttributes("ExecutedCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_ExecutedCommand"));
				callbackBuilder.AddCustomAttributes("FieldLayoutInitializing", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_FieldLayoutInitializing"));
				callbackBuilder.AddCustomAttributes("FieldLayoutInitialized", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_FieldLayoutInitialized"));
				callbackBuilder.AddCustomAttributes("Grouping", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_Grouping"));
				callbackBuilder.AddCustomAttributes("Grouped", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_Grouped"));
				callbackBuilder.AddCustomAttributes("InitializeRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_InitializeRecord"));
				callbackBuilder.AddCustomAttributes("InitializeTemplateAddRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_InitializeTemplateAddRecord"));
				callbackBuilder.AddCustomAttributes("RecordActivated", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordActivated"));
				callbackBuilder.AddCustomAttributes("RecordActivating", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordActivating"));
				callbackBuilder.AddCustomAttributes("RecordAdding", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_E_RecordAdding"));
				callbackBuilder.AddCustomAttributes("RecordAdded", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_E_RecordAdded"));
				callbackBuilder.AddCustomAttributes("RecordCollapsed", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordCollapsed"));
				callbackBuilder.AddCustomAttributes("RecordCollapsing", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordCollapsing"));
				callbackBuilder.AddCustomAttributes("RecordDeactivating", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordDeactivating"));
				callbackBuilder.AddCustomAttributes("RecordExpanded", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordExpanded"));
				callbackBuilder.AddCustomAttributes("RecordExpanding", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordExpanding"));
				callbackBuilder.AddCustomAttributes("RecordsDeleted", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_E_RecordsDeleted"));
				callbackBuilder.AddCustomAttributes("RecordsDeleting", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_E_RecordsDeleting"));

				// JJD 3/23/12 - Metro theme support
				// Added properties to supply default RecordSelectorExtents from within a theme's style
				callbackBuilder.AddCustomAttributes("RecordSelectorExtent", CreateCategory("LC_Appearance"), CreateDescription("LD_DataPresenterBase_P_RecordSelectorExtent"));
				callbackBuilder.AddCustomAttributes("RecordSelectorErrorIconExtent", CreateCategory("LC_Appearance"), CreateDescription("LD_DataPresenterBase_P_RecordSelectorErrorIconExtent"));
				callbackBuilder.AddCustomAttributes("RecordSelectorFixButtonExtent", CreateCategory("LC_Appearance"), CreateDescription("LD_DataPresenterBase_P_RecordSelectorFixButtonExtent"));
				
				callbackBuilder.AddCustomAttributes("RecordsInViewChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordsInViewChanged"));
				callbackBuilder.AddCustomAttributes("RecordUpdateCanceling", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_E_RecordUpdateCanceling"));
				callbackBuilder.AddCustomAttributes("RecordUpdateCanceled", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_E_RecordUpdateCanceled"));
				callbackBuilder.AddCustomAttributes("RecordUpdating", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_E_RecordUpdating"));
				callbackBuilder.AddCustomAttributes("RecordUpdated", CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_E_RecordUpdated"));
				callbackBuilder.AddCustomAttributes("SelectedItemsChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_SelectedItemsChanged"));
				callbackBuilder.AddCustomAttributes("SelectedItemsChanging", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_SelectedItemsChanging"));
				callbackBuilder.AddCustomAttributes("Sorting", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_Sorting"));
				callbackBuilder.AddCustomAttributes("Sorted", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_Sorted"));
				callbackBuilder.AddCustomAttributes("ThemeChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_ThemeChanged"));
				callbackBuilder.AddCustomAttributes("CellContainerGenerationMode", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_CellContainerGenerationMode"));
				callbackBuilder.AddCustomAttributes("RecordContainerGenerationMode", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_RecordContainerGenerationMode"));
			});

			// Infragistics.Windows.DataPresenter.XamDataGrid
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.XamDataGrid), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamDataGrid"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamDataGridAssetLibrary")));


				callbackBuilder.AddCustomAttributes("ViewSettings", CreateCategory("LC_Appearance"), CreateDescription("LD_XamDataGrid_P_ViewSettings"));
			});

			// Infragistics.Windows.DataPresenter.HeaderLabelArea
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.HeaderLabelArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_HeaderLabelArea"));

				callbackBuilder.AddCustomAttributes("FieldLayout", CreateCategory("LC_Behavior"), CreateDescription("LD_HeaderLabelArea_P_FieldLayout"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Appearance"), CreateDescription("LD_HeaderLabelArea_P_Orientation"));
			});


			// Infragistics.Windows.DataPresenter.XamDataCarousel
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.XamDataCarousel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamDataCarousel"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamDataCarouselAssetLibrary")));


				callbackBuilder.AddCustomAttributes("ViewSettings", CreateCategory("LC_Appearance"), CreateDescription("LD_XamDataCarousel_P_ViewSettings"));
			});


			// Infragistics.Windows.DataPresenter.DataRecordCellArea
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataRecordCellArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_DataRecordCellArea"));

				callbackBuilder.AddCustomAttributes("FieldLayout", CreateCategory("LC_Behavior"), CreateDescription("LD_DataRecordCellArea_P_FieldLayout"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Behavior"), CreateDescription("LD_DataRecordCellArea_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsSelected", CreateCategory("LC_Behavior"), CreateDescription("LD_DataRecordCellArea_P_IsSelected"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Appearance"), CreateDescription("LD_DataRecordCellArea_P_Orientation"));
				callbackBuilder.AddCustomAttributes("Record", CreateCategory("LC_Data"), CreateDescription("LD_DataRecordCellArea_P_Record"));
				callbackBuilder.AddCustomAttributes("CornerRadius", CreateCategory("LC_Appearance"), CreateDescription("LD_DataRecordCellArea_P_CornerRadius"));
				callbackBuilder.AddCustomAttributes("BackgroundHover", CreateCategory("LC_Brushes"), CreateDescription("LD_DataRecordCellArea_P_BackgroundHover"));
				callbackBuilder.AddCustomAttributes("BorderHoverBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_DataRecordCellArea_P_BorderHoverBrush"));
				callbackBuilder.AddCustomAttributes("BackgroundActive", CreateCategory("LC_Brushes"), CreateDescription("LD_DataRecordCellArea_P_BackgroundActive"));
				callbackBuilder.AddCustomAttributes("BorderActiveBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_DataRecordCellArea_P_BorderActiveBrush"));
				callbackBuilder.AddCustomAttributes("BackgroundAlternate", CreateCategory("LC_Brushes"), CreateDescription("LD_DataRecordCellArea_P_BackgroundAlternate"));
				callbackBuilder.AddCustomAttributes("BorderAlternateBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_DataRecordCellArea_P_BorderAlternateBrush"));
				callbackBuilder.AddCustomAttributes("BackgroundSelected", CreateCategory("LC_Brushes"), CreateDescription("LD_DataRecordCellArea_P_BackgroundSelected"));
				callbackBuilder.AddCustomAttributes("BorderSelectedBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_DataRecordCellArea_P_BorderSelectedBrush"));
				callbackBuilder.AddCustomAttributes("ForegroundStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_DataRecordCellArea_P_ForegroundStyle"));
				callbackBuilder.AddCustomAttributes("ForegroundActiveStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_DataRecordCellArea_P_ForegroundActiveStyle"));
				callbackBuilder.AddCustomAttributes("ForegroundAlternateStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_DataRecordCellArea_P_ForegroundAlternateStyle"));
				callbackBuilder.AddCustomAttributes("ForegroundSelectedStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_DataRecordCellArea_P_ForegroundSelectedStyle"));
				callbackBuilder.AddCustomAttributes("ForegroundHoverStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_DataRecordCellArea_P_ForegroundHoverStyle"));
			});


			// Infragistics.Windows.DataPresenter.XamDataPresenter
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.XamDataPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamDataPresenter"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamDataPresenterAssetLibrary")));


				callbackBuilder.AddCustomAttributes("View", CreateCategory("LC_Appearance"), CreateDescription("LD_XamDataPresenter_P_View"));
			});

			// Infragistics.Windows.DataPresenter.DataItemPresenter
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataItemPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("HighlightAsPrimary", CreateCategory("LC_Behavior"), CreateDescription("LD_DataItemPresenter_P_HighlightAsPrimary"));
				callbackBuilder.AddCustomAttributes("IsUnbound", CreateCategory("LC_Behavior"), CreateDescription("LD_DataItemPresenter_P_IsUnbound"));
			});

			// Infragistics.Windows.DataPresenter.RecordSelector
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordSelector), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_RecordSelector"));

				callbackBuilder.AddCustomAttributes("FieldLayout", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordSelector_P_FieldLayout"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordSelector_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsAddRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordSelector_P_IsAddRecord"));
				callbackBuilder.AddCustomAttributes("IsDataChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordSelector_P_IsDataChanged"));
				callbackBuilder.AddCustomAttributes("Location", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordSelector_P_Location"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordSelector_P_Orientation"));
				callbackBuilder.AddCustomAttributes("Record", CreateCategory("LC_Data"), CreateDescription("LD_RecordSelector_P_Record"));
			});

			// Infragistics.Windows.DataPresenter.RecordPresenter
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("CellPresentation", CreateDescription("LD_RecordPresenter_P_CellPresentation"));
				callbackBuilder.AddCustomAttributes("Description", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_Description"));
				callbackBuilder.AddCustomAttributes("ExpansionIndicatorVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_ExpansionIndicatorVisibility"));
				callbackBuilder.AddCustomAttributes("FieldLayout", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_FieldLayout"));
				callbackBuilder.AddCustomAttributes("HasChildData", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_HasChildData"));
				callbackBuilder.AddCustomAttributes("HasHeaderContent", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_HasHeaderContent"));
				callbackBuilder.AddCustomAttributes("HasNestedContent", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_HasNestedContent"));
				callbackBuilder.AddCustomAttributes("HeaderContent", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_HeaderContent"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsExpanded", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_IsExpanded"));
				callbackBuilder.AddCustomAttributes("IsFixedOnBottom", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_IsFixedOnBottom"));
				callbackBuilder.AddCustomAttributes("IsFixedOnTop", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_IsFixedOnTop"));
				callbackBuilder.AddCustomAttributes("IsHeaderRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_IsHeaderRecord"));
				callbackBuilder.AddCustomAttributes("IsSelected", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_IsSelected"));
				callbackBuilder.AddCustomAttributes("NestedContent", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_NestedContent"));
				callbackBuilder.AddCustomAttributes("NestedContentMargin", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_NestedContentMargin"));
				callbackBuilder.AddCustomAttributes("NestedContentBackground", CreateCategory("LC_Brushes"), CreateDescription("LD_RecordPresenter_P_NestedContentBackground"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordPresenter_P_Orientation"));
				callbackBuilder.AddCustomAttributes("Record", CreateCategory("LC_Data"), CreateDescription("LD_RecordPresenter_P_Record"));
				callbackBuilder.AddCustomAttributes("ShouldDisplayExpandableRecordContent", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordPresenter_P_ShouldDisplayExpandableRecordContent"));
				callbackBuilder.AddCustomAttributes("ShouldDisplayGroupByRecordContent", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordPresenter_P_ShouldDisplayGroupByRecordContent"));
				callbackBuilder.AddCustomAttributes("ShouldDisplayRecordContent", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordPresenter_P_ShouldDisplayRecordContent"));
				callbackBuilder.AddCustomAttributes("TemplateCardView", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordPresenter_P_TemplateCardView"));
				callbackBuilder.AddCustomAttributes("TemplateGridView", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordPresenter_P_TemplateGridView"));
			});

			// Infragistics.Windows.DataPresenter.DataRecordPresenter
			// ======================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataRecordPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_DataRecordPresenter"));

				callbackBuilder.AddCustomAttributes("DataRecord", CreateCategory("LC_Data"), CreateDescription("LD_DataRecordPresenter_P_DataRecord"));
				callbackBuilder.AddCustomAttributes("IsAddRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_DataRecordPresenter_P_IsAddRecord"));
				callbackBuilder.AddCustomAttributes("IsDataChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataRecordPresenter_P_IsDataChanged"));
				callbackBuilder.AddCustomAttributes("HeaderAreaBackground", CreateCategory("LC_Brushes"), CreateDescription("LD_DataRecordPresenter_P_HeaderAreaBackground"));
			});

			// Infragistics.Windows.DataPresenter.ExpandableFieldRecordPresenter
			// =================================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.ExpandableFieldRecordPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_ExpandableFieldRecordPresenter"));

			});

			// Infragistics.Windows.DataPresenter.GroupByRecordPresenter
			// =========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GroupByRecordPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_GroupByRecordPresenter"));
			});


			// Infragistics.Windows.DataPresenter.FieldLayout
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayout), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AutoGenerateFieldsResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_AutoGenerateFieldsResolved"));
				callbackBuilder.AddCustomAttributes("DataRecordSizingModeResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_DataRecordSizingModeResolved"));
				callbackBuilder.AddCustomAttributes("Description", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_Description"));
				callbackBuilder.AddCustomAttributes("FieldSettings", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_FieldSettings"));
				callbackBuilder.AddCustomAttributes("HighlightAlternateRecordsResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_HighlightAlternateRecordsResolved"));
				callbackBuilder.AddCustomAttributes("HighlightPrimaryFieldResolved", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayout_P_HighlightPrimaryFieldResolved"));
				callbackBuilder.AddCustomAttributes("Key", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayout_P_Key"));
				callbackBuilder.AddCustomAttributes("LabelLocationResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_LabelLocationResolved"));
				callbackBuilder.AddCustomAttributes("MaxFieldsToAutoGenerateResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_MaxFieldsToAutoGenerateResolved"));
				callbackBuilder.AddCustomAttributes("MaxSelectedCellsResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_MaxSelectedCellsResolved"));
				callbackBuilder.AddCustomAttributes("MaxSelectedRecordsResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_MaxSelectedRecordsResolved"));
				callbackBuilder.AddCustomAttributes("RecordSelectorExtentResolved", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayout_P_RecordSelectorExtentResolved"));
				callbackBuilder.AddCustomAttributes("RecordSelectorLocationResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_RecordSelectorLocationResolved"));
				callbackBuilder.AddCustomAttributes("ResizingModeResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_ResizingModeResolved"));
				callbackBuilder.AddCustomAttributes("SelectionTypeCellResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_SelectionTypeCellResolved"));
				callbackBuilder.AddCustomAttributes("SelectionTypeFieldResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_SelectionTypeFieldResolved"));
				callbackBuilder.AddCustomAttributes("SelectionTypeRecordResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_SelectionTypeRecordResolved"));
				callbackBuilder.AddCustomAttributes("Settings", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_Settings"));
				callbackBuilder.AddCustomAttributes("SortedFields", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayout_P_SortedFields"));
                
                // JJD 4/08/10 - TFS30618
                callbackBuilder.AddCustomAttributes("Key", new TypeConverterAttribute(typeof(StringConverter)));
                callbackBuilder.AddCustomAttributes("ParentFieldLayoutKey", new TypeConverterAttribute(typeof(StringConverter)));
                callbackBuilder.AddCustomAttributes("Tag", new TypeConverterAttribute(typeof(StringConverter)));
                callbackBuilder.AddCustomAttributes("ToolTip", new TypeConverterAttribute(typeof(StringConverter)));
            });


			// Infragistics.Windows.DataPresenter.CarouselBreadcrumbControl
			// ============================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CarouselBreadcrumbControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_CarouselBreadcrumbControl"));

				callbackBuilder.AddCustomAttributes("Breadcrumbs", CreateCategory("LC_Data"), CreateDescription("LD_CarouselBreadcrumbControl_P_Breadcrumbs"));
				callbackBuilder.AddCustomAttributes("HasCrumbs", CreateCategory("LC_Data"), CreateDescription("LD_CarouselBreadcrumbControl_P_HasCrumbs"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Appearance"), CreateDescription("LD_CarouselBreadcrumbControl_P_Orientation"));
				callbackBuilder.AddCustomAttributes("CarouselBreadcrumbClick", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselBreadcrumbControl_E_CarouselBreadcrumbClick"));
			});



			// Infragistics.Windows.DataPresenter.GridViewPanel
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GridViewPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_GridViewPanel"));

				callbackBuilder.AddCustomAttributes("ViewSettings", CreateCategory("LC_Appearance"), CreateDescription("LD_GridViewPanel_P_ViewSettings"));
			});


			// Infragistics.Windows.DataPresenter.CarouselBreadcrumb
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CarouselBreadcrumb), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsLastBreadcrumb", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselBreadcrumb_P_IsLastBreadcrumb"));
			});

			// Infragistics.Windows.DataPresenter.CarouselItem
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CarouselItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_CarouselItem"));

				callbackBuilder.AddCustomAttributes("ExpansionIndicatorVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselItem_P_ExpansionIndicatorVisibility"));
				callbackBuilder.AddCustomAttributes("IsExpanded", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselItem_P_IsExpanded"));
				callbackBuilder.AddCustomAttributes("ItemDisappearingStoryboard", CreateCategory("LC_Behavior"), CreateDescription("LD_CarouselItem_P_ItemDisappearingStoryboard"));
			});



			// Infragistics.Windows.DataPresenter.GroupByArea
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GroupByArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_GroupByArea"));

				callbackBuilder.AddCustomAttributes("AvailableFieldLabels", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByArea_P_AvailableFieldLabels"));
				callbackBuilder.AddCustomAttributes("FieldLabelDragInProgress", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByArea_P_FieldLabelDragInProgress"));
				callbackBuilder.AddCustomAttributes("GroupedFieldLabels", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByArea_P_GroupedFieldLabels"));
				callbackBuilder.AddCustomAttributes("IsExpanded", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByArea_P_IsExpanded"));
				callbackBuilder.AddCustomAttributes("Prompt1", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByArea_P_Prompt1"));
				callbackBuilder.AddCustomAttributes("Prompt2", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByArea_P_Prompt2"));
				callbackBuilder.AddCustomAttributes("Collapsed", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByArea_E_Collapsed"));
				callbackBuilder.AddCustomAttributes("Expanded", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByArea_E_Expanded"));
				callbackBuilder.AddCustomAttributes("HideInsertionPoint", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByArea_E_HideInsertionPoint"));
				callbackBuilder.AddCustomAttributes("HidePrompts", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByArea_E_HidePrompts"));
				callbackBuilder.AddCustomAttributes("ShowInsertionPoint", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByArea_E_ShowInsertionPoint"));
				callbackBuilder.AddCustomAttributes("ShowPrompts", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByArea_E_ShowPrompts"));
			});

			// Infragistics.Windows.DataPresenter.GroupByAreaFieldLabel
			// ========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GroupByAreaFieldLabel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695 
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("Field", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByAreaFieldLabel_P_Field"));
				callbackBuilder.AddCustomAttributes("AddedToAvailableList", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByAreaFieldLabel_E_AddedToAvailableList"));
				callbackBuilder.AddCustomAttributes("AddedToGroupedList", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByAreaFieldLabel_E_AddedToGroupedList"));
				callbackBuilder.AddCustomAttributes("RemovedFromAvailableList", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByAreaFieldLabel_E_RemovedFromAvailableList"));
			});

			// Infragistics.Windows.DataPresenter.GridViewPanelAdorner
			// =======================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GridViewPanelAdorner), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695 
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.DataPresenter.GroupByAreaFieldListBox
			// ==========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GroupByAreaFieldListBox), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_GroupByAreaFieldListBox"));
			});


			// Infragistics.Windows.DataPresenter.RecordScrollTip
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordScrollTip), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});


			// Infragistics.Windows.DataPresenter.FieldLayoutSettings
			// ======================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayoutSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AddNewRecordLocation", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AddNewRecordLocation"));
				callbackBuilder.AddCustomAttributes("AllowAddNew", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AllowAddNew"));
				callbackBuilder.AddCustomAttributes("AllowDelete", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AllowDelete"));
				callbackBuilder.AddCustomAttributes("AutoGenerateFields", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AutoGenerateFields"));
				callbackBuilder.AddCustomAttributes("AutoArrangeCells", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_AutoArrangeCells"));
				callbackBuilder.AddCustomAttributes("AutoArrangeMaxColumns", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AutoArrangeMaxColumns"));
				callbackBuilder.AddCustomAttributes("AutoArrangeMaxRows", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AutoArrangeMaxRows"));
				callbackBuilder.AddCustomAttributes("AutoArrangePrimaryFieldReservation", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AutoArrangePrimaryFieldReservation"));
				callbackBuilder.AddCustomAttributes("DataRecordCellAreaGridTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_DataRecordCellAreaGridTemplate"));
				callbackBuilder.AddCustomAttributes("DataRecordCellAreaStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_DataRecordCellAreaStyle"));
				callbackBuilder.AddCustomAttributes("DataRecordCellAreaStyleSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_DataRecordCellAreaStyleSelector"));
				callbackBuilder.AddCustomAttributes("DataRecordPresenterStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_DataRecordPresenterStyle"));
				callbackBuilder.AddCustomAttributes("DataRecordPresenterStyleSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_DataRecordPresenterStyleSelector"));
				callbackBuilder.AddCustomAttributes("DataRecordSizingMode", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_DataRecordSizingMode"));
				callbackBuilder.AddCustomAttributes("DefaultColumnDefinition", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_DefaultColumnDefinition"));
				callbackBuilder.AddCustomAttributes("DefaultRowDefinition", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_DefaultRowDefinition"));
				callbackBuilder.AddCustomAttributes("ExpansionIndicatorDisplayMode", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_ExpansionIndicatorDisplayMode"));
				callbackBuilder.AddCustomAttributes("HeaderAreaStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_HeaderAreaStyle"));
				callbackBuilder.AddCustomAttributes("HeaderLabelAreaStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_HeaderLabelAreaStyle"));
				callbackBuilder.AddCustomAttributes("HeaderLabelAreaStyleSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_HeaderLabelAreaStyleSelector"));
				callbackBuilder.AddCustomAttributes("HeaderPrefixAreaStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_HeaderPrefixAreaStyle"));
				callbackBuilder.AddCustomAttributes("HeaderPrefixAreaStyleSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_HeaderPrefixAreaStyleSelector"));
				callbackBuilder.AddCustomAttributes("HighlightAlternateRecords", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_HighlightAlternateRecords"));
				callbackBuilder.AddCustomAttributes("HighlightPrimaryField", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_HighlightPrimaryField"));
				callbackBuilder.AddCustomAttributes("LabelLocation", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_LabelLocation"));
				callbackBuilder.AddCustomAttributes("MaxFieldsToAutoGenerate", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_MaxFieldsToAutoGenerate"));
				callbackBuilder.AddCustomAttributes("MaxSelectedCells", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_MaxSelectedCells"));
				callbackBuilder.AddCustomAttributes("MaxSelectedRecords", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_MaxSelectedRecords"));
				callbackBuilder.AddCustomAttributes("RecordListControlStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_RecordListControlStyle"));
				callbackBuilder.AddCustomAttributes("RecordSelectorLocation", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_RecordSelectorLocation"));
				callbackBuilder.AddCustomAttributes("RecordSelectorExtent", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_RecordSelectorExtent"));
				callbackBuilder.AddCustomAttributes("RecordSelectorStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_RecordSelectorStyle"));
				callbackBuilder.AddCustomAttributes("RecordSelectorStyleSelector", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_RecordSelectorStyleSelector"));
				callbackBuilder.AddCustomAttributes("ResizingMode", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_ResizingMode"));
				callbackBuilder.AddCustomAttributes("SelectionTypeCell", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_SelectionTypeCell"));
				callbackBuilder.AddCustomAttributes("SelectionTypeField", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_SelectionTypeField"));
				callbackBuilder.AddCustomAttributes("SelectionTypeRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_SelectionTypeRecord"));
			});

			// Infragistics.Windows.DataPresenter.GridViewSettings
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GridViewSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("HeightInInfiniteContainers", CreateCategory("LC_Appearance"), CreateDescription("LD_GridViewSettings_P_HeightInInfiniteContainers"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Behavior"), CreateDescription("LD_GridViewSettings_P_Orientation"));
				callbackBuilder.AddCustomAttributes("WidthInInfiniteContainers", CreateCategory("LC_Appearance"), CreateDescription("LD_GridViewSettings_P_WidthInInfiniteContainers"));
			});


			// Infragistics.Windows.DataPresenter.CarouselViewPanel
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CarouselViewPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_CarouselViewPanel"));
			});



			// Infragistics.Windows.DataPresenter.LabelPresenter
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.LabelPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_LabelPresenter"));

				callbackBuilder.AddCustomAttributes("InnerBorderBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_LabelPresenter_P_InnerBorderBrush"));
				callbackBuilder.AddCustomAttributes("IsPressed", CreateCategory("LC_Behavior"), CreateDescription("LD_LabelPresenter_P_IsPressed"));
				callbackBuilder.AddCustomAttributes("LabelHighlight", CreateCategory("LC_Brushes"), CreateDescription("LD_LabelPresenter_P_LabelHighlight"));
				callbackBuilder.AddCustomAttributes("OuterBorderBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_LabelPresenter_P_OuterBorderBrush"));
				callbackBuilder.AddCustomAttributes("SortStatus", CreateCategory("LC_Behavior"), CreateDescription("LD_LabelPresenter_P_SortStatus"));
			});

			// Infragistics.Windows.DataPresenter.CellValuePresenter
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CellValuePresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_CellValuePresenter"));

				callbackBuilder.AddCustomAttributes("IsFieldSelected", CreateCategory("LC_Behavior"), CreateDescription("LD_CellValuePresenter_P_IsFieldSelected"));
				callbackBuilder.AddCustomAttributes("SortStatus", CreateCategory("LC_Behavior"), CreateDescription("LD_CellValuePresenter_P_SortStatus"));
				callbackBuilder.AddCustomAttributes("BackgroundHover", CreateCategory("LC_Brushes"), CreateDescription("LD_CellValuePresenter_P_BackgroundHover"));
				callbackBuilder.AddCustomAttributes("BorderHoverBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_CellValuePresenter_P_BorderHoverBrush"));
				callbackBuilder.AddCustomAttributes("BackgroundActive", CreateCategory("LC_Brushes"), CreateDescription("LD_CellValuePresenter_P_BackgroundActive"));
				callbackBuilder.AddCustomAttributes("BorderActiveBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_CellValuePresenter_P_BorderActiveBrush"));
				callbackBuilder.AddCustomAttributes("BackgroundSelected", CreateCategory("LC_Brushes"), CreateDescription("LD_CellValuePresenter_P_BackgroundSelected"));
				callbackBuilder.AddCustomAttributes("BorderSelectedBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_CellValuePresenter_P_BorderSelectedBrush"));
				callbackBuilder.AddCustomAttributes("BackgroundFieldSelected", CreateCategory("LC_Brushes"), CreateDescription("LD_CellValuePresenter_P_BackgroundFieldSelected"));
				callbackBuilder.AddCustomAttributes("BorderFieldSelectedBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_CellValuePresenter_P_BorderFieldSelectedBrush"));
				callbackBuilder.AddCustomAttributes("BackgroundPrimary", CreateCategory("LC_Brushes"), CreateDescription("LD_CellValuePresenter_P_BackgroundPrimary"));
				callbackBuilder.AddCustomAttributes("BorderPrimaryBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_CellValuePresenter_P_BorderPrimaryBrush"));
				callbackBuilder.AddCustomAttributes("ForegroundStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_CellValuePresenter_P_ForegroundStyle"));
				callbackBuilder.AddCustomAttributes("ForegroundActiveStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_CellValuePresenter_P_ForegroundActiveStyle"));
				callbackBuilder.AddCustomAttributes("ForegroundAlternateStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_CellValuePresenter_P_ForegroundAlternateStyle"));
				callbackBuilder.AddCustomAttributes("ForegroundSelectedStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_CellValuePresenter_P_ForegroundSelectedStyle"));
				callbackBuilder.AddCustomAttributes("ForegroundFieldSelectedStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_CellValuePresenter_P_ForegroundFieldSelectedStyle"));
				callbackBuilder.AddCustomAttributes("ForegroundHoverStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_CellValuePresenter_P_ForegroundHoverStyle"));
				callbackBuilder.AddCustomAttributes("ForegroundPrimaryStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_CellValuePresenter_P_ForegroundPrimaryStyle"));
				callbackBuilder.AddCustomAttributes("CornerRadius", CreateCategory("LC_Appearance"), CreateDescription("LD_CellValuePresenter_P_CornerRadius"));
			});

			// Infragistics.Windows.DataPresenter.ExpandedCellPresenter
			// ========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.ExpandedCellPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Behavior"), CreateDescription("LD_ExpandedCellPresenter_P_Orientation"));
			});

			// Infragistics.Windows.DataPresenter.HeaderPresenter
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.HeaderPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_HeaderPresenter"));
			});


			// Infragistics.Windows.DataPresenter.GridView
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GridView), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ViewSettings", CreateCategory("LC_Appearance"), CreateDescription("LD_GridView_P_ViewSettings"));
			});

			// Infragistics.Windows.DataPresenter.Field
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.Field), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowGroupByResolved", CreateDescription("LD_Field_P_AllowGroupByResolved"));
				callbackBuilder.AddCustomAttributes("AllowResizeResolved", CreateDescription("LD_Field_P_AllowResizeResolved"));
				callbackBuilder.AddCustomAttributes("AutoGenerated", CreateDescription("LD_Field_P_AutoGenerated"));
				callbackBuilder.AddCustomAttributes("Column", CreateDescription("LD_Field_P_Column"));
				callbackBuilder.AddCustomAttributes("ColumnSpan", CreateDescription("LD_Field_P_ColumnSpan"));
				callbackBuilder.AddCustomAttributes("Converter", CreateCategory("LC_Data"), CreateDescription("LD_Field_P_Converter"));
				callbackBuilder.AddCustomAttributes("ConverterCulture", CreateCategory("LC_Data"), CreateDescription("LD_Field_P_ConverterCulture"));
				callbackBuilder.AddCustomAttributes("DataType", CreateDescription("LD_Field_P_DataType"));
				callbackBuilder.AddCustomAttributes("IsExpandable", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_IsExpandable"));
				callbackBuilder.AddCustomAttributes("IsExpandableResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_IsExpandableResolved"));
				callbackBuilder.AddCustomAttributes("IsGroupBy", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_IsGroupBy"));
				callbackBuilder.AddCustomAttributes("IsUnbound", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_IsUnbound"));
				callbackBuilder.AddCustomAttributes("Label", CreateDescription("LD_Field_P_Label"));
				callbackBuilder.AddCustomAttributes("Name", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_Name"));
				callbackBuilder.AddCustomAttributes("Row", CreateDescription("LD_Field_P_Row"));
				callbackBuilder.AddCustomAttributes("RowSpan", CreateDescription("LD_Field_P_RowSpan"));
				callbackBuilder.AddCustomAttributes("Settings", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_Settings"));
				callbackBuilder.AddCustomAttributes("SortStatus", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_SortStatus"));
				callbackBuilder.AddCustomAttributes("Visibility", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_Visibility"));
				callbackBuilder.AddCustomAttributes("SummaryStringFormats", CreateCategory("LC_Data"), CreateDescription("LD_Field_P_SummaryStringFormats"));

                // JJD 4/08/10 - TFS30618
                callbackBuilder.AddCustomAttributes("Label", new TypeConverterAttribute(typeof(StringConverter)));
                callbackBuilder.AddCustomAttributes("Tag", new TypeConverterAttribute(typeof(StringConverter)));
                callbackBuilder.AddCustomAttributes("ToolTip", new TypeConverterAttribute(typeof(StringConverter)));
            });

			// Infragistics.Windows.DataPresenter.UnboundField
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.UnboundField), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("BindingMode", CreateCategory("LC_Data"), CreateDescription("LD_UnboundField_P_BindingMode"));
				callbackBuilder.AddCustomAttributes("BindingPath", CreateCategory("LC_Data"), CreateDescription("LD_UnboundField_P_BindingPath"));
				callbackBuilder.AddCustomAttributes("Binding", CreateCategory("LC_Data"), CreateDescription("LD_UnboundField_P_Binding"));
			});

			// Infragistics.Windows.DataPresenter.RecordListControl
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordListControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes("HorizontalScrollBarVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordListControl_P_HorizontalScrollBarVisibility"));
				callbackBuilder.AddCustomAttributes("VerticalScrollBarVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordListControl_P_VerticalScrollBarVisibility"));
			});

            // Infragistics.Windows.DataPresenter.Internal.RecordListItemContainer
			// ===================================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.Internal.RecordListItemContainer), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			#endregion //Description, Category, etc.

			#region NA 2008 Vol 1

			// Infragistics.Windows.DataPresenter.SummaryResultCollection
			// ==========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryResultCollection), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("SummaryRecordHeader", CreateCategory("LC_Data"), CreateDescription("LD_SummaryResultCollection_P_SummaryRecordHeader"));
			});

			// Infragistics.Windows.DataPresenter.GroupBySummariesPresenter
			// ============================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GroupBySummariesPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ContentAreaMargins", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupBySummariesPresenter_P_ContentAreaMargins"));
				callbackBuilder.AddCustomAttributes("DisplaySummariesAsCells", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupBySummariesPresenter_P_DisplaySummariesAsCells"));
				callbackBuilder.AddCustomAttributes("GroupByRecord", CreateCategory("LC_Data"), CreateDescription("LD_GroupBySummariesPresenter_P_GroupByRecord"));
				callbackBuilder.AddCustomAttributes("HasSummaries", CreateCategory("LC_Data"), CreateDescription("LD_GroupBySummariesPresenter_P_HasSummaries"));
				callbackBuilder.AddCustomAttributes("SummaryResults", CreateCategory("LC_Data"), CreateDescription("LD_GroupBySummariesPresenter_P_SummaryResults"));
			});

			// Infragistics.Windows.DataPresenter.SummaryButton
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryButton), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Field", CreateCategory("LC_Data"), CreateDescription("LD_SummaryButton_P_Field"));
			});

			// Infragistics.Windows.DataPresenter.SummaryCalculatorHolder
			// ==========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryCalculatorHolder), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsChecked", CreateCategory("LC_Data"), CreateDescription("LD_SummaryCalculatorHolder_P_IsChecked"));
			});

			// Infragistics.Windows.DataPresenter.SummaryCalculatorSelectionControl
			// ====================================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryCalculatorSelectionControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowMultipleSummaries", CreateCategory("LC_Behavior"), CreateDescription("LD_SummaryCalculatorSelectionControl_P_AllowMultipleSummaries"));
				callbackBuilder.AddCustomAttributes("Field", CreateCategory("LC_Data"), CreateDescription("LD_SummaryCalculatorSelectionControl_P_Field"));
				callbackBuilder.AddCustomAttributes("SummaryCalculatorHolders", CreateCategory("LC_Data"), CreateDescription("LD_SummaryCalculatorSelectionControl_P_SummaryCalculatorHolders"));
			});

			// Infragistics.Windows.DataPresenter.SummaryRecordContentArea
			// ===========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryRecordContentArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("SummaryRecord", CreateCategory("LC_Data"), CreateDescription("LD_SummaryRecordContentArea_P_SummaryRecord"));
				callbackBuilder.AddCustomAttributes("SummaryRecordHeaderVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_SummaryRecordContentArea_P_SummaryRecordHeaderVisibility"));
				callbackBuilder.AddCustomAttributes("SummaryResultsCenter", CreateCategory("LC_Data"), CreateDescription("LD_SummaryRecordContentArea_P_SummaryResultsCenter"));
				callbackBuilder.AddCustomAttributes("SummaryResultsLeft", CreateCategory("LC_Data"), CreateDescription("LD_SummaryRecordContentArea_P_SummaryResultsLeft"));
				callbackBuilder.AddCustomAttributes("SummaryResultsRight", CreateCategory("LC_Data"), CreateDescription("LD_SummaryRecordContentArea_P_SummaryResultsRight"));
			});

			// Infragistics.Windows.DataPresenter.SummaryRecordHeaderPresenter
			// ===============================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryRecordHeaderPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("SummaryRecord", CreateCategory("LC_Data"), CreateDescription("LD_SummaryRecordHeaderPresenter_P_SummaryRecord"));
			});

			// Infragistics.Windows.DataPresenter.RecordPrefixArea
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordPrefixArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_RecordPrefixArea"));

				callbackBuilder.AddCustomAttributes("FieldLayout", CreateCategory("LC_Data"), CreateDescription("LD_RecordPrefixArea_P_FieldLayout"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordPrefixArea_P_Orientation"));
				callbackBuilder.AddCustomAttributes("Record", CreateCategory("LC_Data"), CreateDescription("LD_RecordPrefixArea_P_Record"));
			});

			// Infragistics.Windows.DataPresenter.SummaryRecordPrefixArea
			// ==========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryRecordPrefixArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_SummaryRecordPrefixArea"));
			});


			// Infragistics.Windows.DataPresenter.RecordPresenter
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("HasSeparatorAfter", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordPresenter_P_HasSeparatorAfter"));
				callbackBuilder.AddCustomAttributes("HasSeparatorBefore", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordPresenter_P_HasSeparatorBefore"));
			});

			// Infragistics.Windows.DataPresenter.SummaryRecordPresenter
			// =========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryRecordPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ContentAreaMargins", CreateCategory("LC_Appearance"), CreateDescription("LD_SummaryRecordPresenter_P_ContentAreaMargins"));
				callbackBuilder.AddCustomAttributes("HeaderAreaBackground", CreateCategory("LC_Brushes"), CreateDescription("LD_SummaryRecordPresenter_P_HeaderAreaBackground"));
				callbackBuilder.AddCustomAttributes("SummaryRecord", CreateCategory("LC_Data"), CreateDescription("LD_SummaryRecordPresenter_P_SummaryRecord"));
			});

			// Infragistics.Windows.DataPresenter.SummaryResultPresenter
			// =========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryResultPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("SummaryResult", CreateCategory("LC_Data"), CreateDescription("LD_SummaryResultPresenter_P_SummaryResult"));
			});

			// Infragistics.Windows.DataPresenter.SummaryResultsPresenter
			// ==========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryResultsPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("SummaryResults", CreateCategory("LC_Data"), CreateDescription("LD_SummaryResultsPresenter_P_SummaryResults"));
			});

			// Infragistics.Windows.DataPresenter.FieldSettings
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowSummaries", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowSummaries"));
				callbackBuilder.AddCustomAttributes("SummaryDisplayArea", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_SummaryDisplayArea"));
				callbackBuilder.AddCustomAttributes("SummaryUIType", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_SummaryUIType"));
			});

			// Infragistics.Windows.DataPresenter.DataPresenterBase
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataPresenterBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("SummaryResultChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_SummaryResultChanged"));
				callbackBuilder.AddCustomAttributes("SummarySelectionControlClosed", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_SummarySelectionControlClosed"));
				callbackBuilder.AddCustomAttributes("SummarySelectionControlOpening", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_SummarySelectionControlOpening"));
			});

			// Infragistics.Windows.DataPresenter.SpecialRecordOrder
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SpecialRecordOrder), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AddRecord", CreateCategory("LC_Appearance"), CreateDescription("LD_SpecialRecordOrder_P_AddRecord"));
				callbackBuilder.AddCustomAttributes("SummaryRecord", CreateCategory("LC_Appearance"), CreateDescription("LD_SpecialRecordOrder_P_SummaryRecord"));
			});

			// Infragistics.Windows.DataPresenter.SummaryResult
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryResult), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DisplayAreaResolved", CreateDescription("LD_SummaryResult_P_DisplayAreaResolved"));
				callbackBuilder.AddCustomAttributes("ToolTip", CreateCategory("LC_Data"), CreateDescription("LD_SummaryResult_P_ToolTip"));
			});

			// Infragistics.Windows.DataPresenter.GroupByRecordPresenter
			// =========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GroupByRecordPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ShouldDisplaySummaries", CreateCategory("LC_Data"), CreateDescription("LD_GroupByRecordPresenter_P_ShouldDisplaySummaries"));
			});

			// Infragistics.Windows.DataPresenter.FieldLayout
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayout), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("SummaryDefinitions", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayout_P_SummaryDefinitions"));
				callbackBuilder.AddCustomAttributes("SummaryDescriptionMask", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayout_P_SummaryDescriptionMask"));
				callbackBuilder.AddCustomAttributes("SummaryDescriptionMaskInGroupBy", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayout_P_SummaryDescriptionMaskInGroupBy"));
			});

			// Infragistics.Windows.DataPresenter.SummaryDefinition
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryDefinition), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Calculator", CreateCategory("LC_Data"), CreateDescription("LD_SummaryDefinition_P_Calculator"));
				callbackBuilder.AddCustomAttributes("DisplayArea", CreateCategory("LC_Behavior"), CreateDescription("LD_SummaryDefinition_P_DisplayArea"));
				callbackBuilder.AddCustomAttributes("DisplayFormat", CreateCategory("LC_Data"), CreateDescription("LD_SummaryDefinition_P_DisplayFormat"));
				callbackBuilder.AddCustomAttributes("DisplayFormatProvider", CreateCategory("LC_Data"), CreateDescription("LD_SummaryDefinition_P_DisplayFormatProvider"));
				callbackBuilder.AddCustomAttributes("Key", CreateCategory("LC_Data"), CreateDescription("LD_SummaryDefinition_P_Key"));
				callbackBuilder.AddCustomAttributes("Position", CreateCategory("LC_Appearance"), CreateDescription("LD_SummaryDefinition_P_Position"));
				callbackBuilder.AddCustomAttributes("PositionFieldName", CreateCategory("LC_Appearance"), CreateDescription("LD_SummaryDefinition_P_PositionFieldName"));
				callbackBuilder.AddCustomAttributes("SourceFieldName", CreateCategory("LC_Data"), CreateDescription("LD_SummaryDefinition_P_SourceFieldName"));
				callbackBuilder.AddCustomAttributes("ToolTip", CreateCategory("LC_Data"), CreateDescription("LD_SummaryDefinition_P_ToolTip"));
			});

			// Infragistics.Windows.DataPresenter.FieldLayoutSettings
			// ======================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayoutSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("CalculationScope", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayoutSettings_P_CalculationScope"));
				callbackBuilder.AddCustomAttributes("GroupBySummaryDisplayMode", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_GroupBySummaryDisplayMode"));
				callbackBuilder.AddCustomAttributes("RecordSeparatorLocation", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_RecordSeparatorLocation"));
				callbackBuilder.AddCustomAttributes("SpecialRecordOrder", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_SpecialRecordOrder"));
				callbackBuilder.AddCustomAttributes("SummaryDescriptionVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_SummaryDescriptionVisibility"));
			});

			// Infragistics.Windows.DataPresenter.Field
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.Field), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowSummariesResolved", CreateDescription("LD_Field_P_AllowSummariesResolved"));
				callbackBuilder.AddCustomAttributes("SummaryUITypeResolved", CreateDescription("LD_Field_P_SummaryUITypeResolved"));
			});

			#endregion //NA 2008 Vol 1

			#region NA 2008 Vol 2


			// Infragistics.Windows.DataPresenter.DataItemPresenter
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataItemPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsDragIndicator", CreateCategory("LC_Data"), CreateDescription("LD_DataItemPresenter_P_IsDragIndicator"));
			});

			// Infragistics.Windows.DataPresenter.FieldDragIndicator
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldDragIndicator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Field", CreateCategory("LC_Data"), CreateDescription("LD_FieldDragIndicator_P_Field"));
				callbackBuilder.AddCustomAttributes("IncludeCell", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldDragIndicator_P_IncludeCell"));
			});

			// Infragistics.Windows.DataPresenter.DataPresenterBase
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataPresenterBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ReportView", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_ReportView"));
				callbackBuilder.AddCustomAttributes("FieldPositionChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_FieldPositionChanged"));
				callbackBuilder.AddCustomAttributes("FieldPositionChanging", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_FieldPositionChanging"));
			});

			// Infragistics.Windows.DataPresenter.ReportViewBase
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.ReportViewBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("CellPageSpanStrategy", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_CellPageSpanStrategy"));
				callbackBuilder.AddCustomAttributes("ExcludeCellValuePresenterStyles", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeCellValuePresenterStyles"));
				callbackBuilder.AddCustomAttributes("ExcludeEditorSettings", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeEditorSettings"));
				callbackBuilder.AddCustomAttributes("ExcludeExpandedState", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeExpandedState"));
				callbackBuilder.AddCustomAttributes("ExcludeFieldLayoutSettings", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeFieldLayoutSettings"));
				callbackBuilder.AddCustomAttributes("ExcludeFieldSettings", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeFieldSettings"));
				callbackBuilder.AddCustomAttributes("ExcludeGroupBySettings", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeGroupBySettings"));
				callbackBuilder.AddCustomAttributes("ExcludeLabelPresenterStyles", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeLabelPresenterStyles"));
				callbackBuilder.AddCustomAttributes("ExcludeRecordVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeRecordVisibility"));
				callbackBuilder.AddCustomAttributes("ExcludeSortOrder", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeSortOrder"));
				callbackBuilder.AddCustomAttributes("ExcludeSummaries", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeSummaries"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_Appearance"), CreateDescription("LD_ReportViewBase_P_Theme"));
			});

			// Infragistics.Windows.DataPresenter.TabularReportView
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.TabularReportView), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("LevelIndentation", CreateCategory("LC_Appearance"), CreateDescription("LD_TabularReportView_P_LevelIndentation"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Appearance"), CreateDescription("LD_TabularReportView_P_Orientation"));
			});

			// Infragistics.Windows.DataPresenter.FieldLayoutSettings
			// ======================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayoutSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowFieldMoving", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AllowFieldMoving"));
				callbackBuilder.AddCustomAttributes("FieldMovingMaxColumns", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_FieldMovingMaxColumns"));
				callbackBuilder.AddCustomAttributes("FieldMovingMaxRows", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_FieldMovingMaxRows"));
			});

			// Infragistics.Windows.DataPresenter.LabelPresenter
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.LabelPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsDragSource", CreateCategory("LC_Data"), CreateDescription("LD_LabelPresenter_P_IsDragSource"));
			});


			#endregion //NA 2008 Vol 2

			#region NA 2009 Vol 1

			// Infragistics.Windows.DataPresenter.CellPresenter
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CellPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsFixed", CreateCategory("LC_Behavior"), CreateDescription("LD_CellPresenter_P_IsFixed"));
			});

			// Infragistics.Windows.DataPresenter.DataItemPresenter
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataItemPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsFixed", CreateCategory("LC_Behavior"), CreateDescription("LD_DataItemPresenter_P_IsFixed"));
			});

			// Infragistics.Windows.DataPresenter.FilterButton
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FilterButton), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Field", CreateCategory("LC_Data"), CreateDescription("LD_FilterButton_P_Field"));
				callbackBuilder.AddCustomAttributes("SelectedOperand", CreateCategory("LC_Behavior"), CreateDescription("LD_FilterButton_P_SelectedOperand"));
			});

			// Infragistics.Windows.DataPresenter.FilterCellValuePresenter
			// ===========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FilterCellValuePresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsOperandDropDownOpen", CreateCategory("LC_Behavior"), CreateDescription("LD_FilterCellValuePresenter_P_IsOperandDropDownOpen"));
				callbackBuilder.AddCustomAttributes("IsOperatorDropDownOpen", CreateCategory("LC_Behavior"), CreateDescription("LD_FilterCellValuePresenter_P_IsOperatorDropDownOpen"));
				callbackBuilder.AddCustomAttributes("Operator", CreateCategory("LC_Behavior"), CreateDescription("LD_FilterCellValuePresenter_P_Operator"));
			});

			// Infragistics.Windows.DataPresenter.CustomFilterSelectionControl
			// ===============================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CustomFilterSelectionControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AddConditionLabel", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_AddConditionLabel"));
				callbackBuilder.AddCustomAttributes("AndGroupLegendDescription", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_AndGroupLegendDescription"));
				callbackBuilder.AddCustomAttributes("AndLogicalOperatorBrush", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_AndLogicalOperatorBrush"));
				callbackBuilder.AddCustomAttributes("CancelButtonLabel", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_CancelButtonLabel"));
				callbackBuilder.AddCustomAttributes("CancelMessageText", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_CancelMessageText"));
				callbackBuilder.AddCustomAttributes("CancelMessageTitle", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_CancelMessageTitle"));
				callbackBuilder.AddCustomAttributes("FieldDescription", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_FieldDescription"));
				callbackBuilder.AddCustomAttributes("FilterSummaryDescription", CreateCategory("LC_Behavior"), CreateDescription("LD_CustomFilterSelectionControl_P_FilterSummaryDescription"));
				callbackBuilder.AddCustomAttributes("GroupSelectedConditionsAsAndGroupLabel", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_GroupSelectedConditionsAsAndGroupLabel"));
				callbackBuilder.AddCustomAttributes("GroupSelectedConditionsAsOrGroupLabel", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_GroupSelectedConditionsAsOrGroupLabel"));
				callbackBuilder.AddCustomAttributes("GroupSelectedLabel", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_GroupSelectedLabel"));
				callbackBuilder.AddCustomAttributes("OkButtonLabel", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_OkButtonLabel"));
				callbackBuilder.AddCustomAttributes("OrGroupLegendDescription", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_OrGroupLegendDescription"));
				callbackBuilder.AddCustomAttributes("OrLogicalOperatorBrush", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_OrLogicalOperatorBrush"));
				callbackBuilder.AddCustomAttributes("RemoveSelectedConditionsLabel", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_RemoveSelectedConditionsLabel"));
				callbackBuilder.AddCustomAttributes("TitleDescription", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_TitleDescription"));
				callbackBuilder.AddCustomAttributes("ToggleOperatorOfSelectedConditionsLabel", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_ToggleOperatorOfSelectedConditionsLabel"));
				callbackBuilder.AddCustomAttributes("UngroupSelectedConditionsLabel", CreateCategory("LC_Appearance"), CreateDescription("LD_CustomFilterSelectionControl_P_UngroupSelectedConditionsLabel"));
			});

			// Infragistics.Windows.DataPresenter.RecordFilter
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordFilter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FieldName", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordFilter_P_FieldName"));
			});

			// Infragistics.Windows.DataPresenter.FixedFieldButton
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FixedFieldButton), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Field", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedFieldButton_P_Field"));
				callbackBuilder.AddCustomAttributes("FixToFarEdgePrompt", CreateCategory("LC_Appearance"), CreateDescription("LD_FixedFieldButton_P_FixToFarEdgePrompt"));
				callbackBuilder.AddCustomAttributes("FixToNearEdgePrompt", CreateCategory("LC_Appearance"), CreateDescription("LD_FixedFieldButton_P_FixToNearEdgePrompt"));
				callbackBuilder.AddCustomAttributes("UnfixPrompt", CreateCategory("LC_Appearance"), CreateDescription("LD_FixedFieldButton_P_UnfixPrompt"));
				callbackBuilder.AddCustomAttributes("CurrentCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedFieldButton_P_CurrentCommand"));
				callbackBuilder.AddCustomAttributes("FixToNearEdgeCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedFieldButton_P_FixToNearEdgeCommand"));
				callbackBuilder.AddCustomAttributes("FixToFarEdgeCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedFieldButton_P_FixToFarEdgeCommand"));
				callbackBuilder.AddCustomAttributes("UnfixCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedFieldButton_P_UnfixCommand"));
			});

			// Infragistics.Windows.DataPresenter.FixedFieldSplitter
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FixedFieldSplitter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsHighlighted", CreateCategory("LC_Appearance"), CreateDescription("LD_FixedFieldSplitter_P_IsHighlighted"));
				callbackBuilder.AddCustomAttributes("IsInHeader", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedFieldSplitter_P_IsInHeader"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedFieldSplitter_P_Orientation"));
				callbackBuilder.AddCustomAttributes("SplitterType", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedFieldSplitter_P_SplitterType"));

				// JJD 07/11/12 - TFS113976 - added
				callbackBuilder.AddCustomAttributes("HorizontalCursor", CreateCategory("LC_Appearance"), CreateDescription("LD_FixedFieldSplitter_P_HorizontalCursor"));
				callbackBuilder.AddCustomAttributes("VerticalCursor", CreateCategory("LC_Appearance"), CreateDescription("LD_FixedFieldSplitter_P_VerticalCursor"));
			});

			// Infragistics.Windows.DataPresenter.DataPresenterBase
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataPresenterBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsSynchronizedWithCurrentItem", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_IsSynchronizedWithCurrentItem"));
				callbackBuilder.AddCustomAttributes("CustomFilterSelectionControlOpening", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_CustomFilterSelectionControlOpening"));
				callbackBuilder.AddCustomAttributes("RecordFilterChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordFilterChanged"));
				callbackBuilder.AddCustomAttributes("RecordFilterChanging", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordFilterChanging"));
				callbackBuilder.AddCustomAttributes("RecordFilterDropDownOpening", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordFilterDropDownOpening"));
				callbackBuilder.AddCustomAttributes("RecordFilterDropDownPopulating", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordFilterDropDownPopulating"));
			});


			// Infragistics.Windows.DataPresenter.ReportViewBase
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.ReportViewBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ExcludeRecordFilters", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeRecordFilters"));
				callbackBuilder.AddCustomAttributes("ExcludeRecordSizing", CreateCategory("LC_Behavior"), CreateDescription("LD_ReportViewBase_P_ExcludeRecordSizing"));
			});


			// Infragistics.Windows.DataPresenter.RecordPresenter
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FixedNearElementTransform", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_FixedNearElementTransform"));
				callbackBuilder.AddCustomAttributes("FixedFarElementTransform", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_FixedFarElementTransform"));
				callbackBuilder.AddCustomAttributes("ScrollableElementTransform", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_ScrollableElementTransform"));
			});

			// Infragistics.Windows.DataPresenter.FieldLayoutSettings
			// ======================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayoutSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FilterAction", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_FilterAction"));
				callbackBuilder.AddCustomAttributes("FilterClearButtonLocation", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_FilterClearButtonLocation"));
				callbackBuilder.AddCustomAttributes("FilterRecordLocation", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutSettings_P_FilterRecordLocation"));
				callbackBuilder.AddCustomAttributes("FilterUIType", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_FilterUIType"));
				callbackBuilder.AddCustomAttributes("FixedFieldUIType", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_FixedFieldUIType"));
				callbackBuilder.AddCustomAttributes("HeaderPlacement", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_HeaderPlacement"));
				callbackBuilder.AddCustomAttributes("HeaderPlacementInGroupBy", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_HeaderPlacementInGroupBy"));
				callbackBuilder.AddCustomAttributes("RecordFilterScope", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_RecordFilterScope"));
				callbackBuilder.AddCustomAttributes("RecordFiltersLogicalOperator", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_RecordFiltersLogicalOperator"));
				callbackBuilder.AddCustomAttributes("ReevaluateFiltersOnDataChange", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_ReevaluateFiltersOnDataChange"));
			});

			// Infragistics.Windows.DataPresenter.FieldSettings
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowFixing", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowFixing"));
				callbackBuilder.AddCustomAttributes("AllowRecordFiltering", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowRecordFiltering"));
				callbackBuilder.AddCustomAttributes("FilterCellValuePresenterStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_FilterCellValuePresenterStyle"));
				callbackBuilder.AddCustomAttributes("FilterCellEditorStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_FilterCellEditorStyle"));
				callbackBuilder.AddCustomAttributes("FilterClearButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_FilterClearButtonVisibility"));
				callbackBuilder.AddCustomAttributes("FilterEvaluationTrigger", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_FilterEvaluationTrigger"));
				callbackBuilder.AddCustomAttributes("FilterOperandUIType", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_FilterOperandUIType"));
				callbackBuilder.AddCustomAttributes("FilterOperatorDefaultValue", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_FilterOperatorDefaultValue"));
				callbackBuilder.AddCustomAttributes("FilterOperatorDropDownItems", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_FilterOperatorDropDownItems"));
				callbackBuilder.AddCustomAttributes("FilterStringComparisonType", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_FilterStringComparisonType"));
			});

			// Infragistics.Windows.DataPresenter.SpecialRecordOrder
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SpecialRecordOrder), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FilterRecord", CreateCategory("LC_Appearance"), CreateDescription("LD_SpecialRecordOrder_P_FilterRecord"));
			});

			// Infragistics.Windows.DataPresenter.Field
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.Field), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FixedButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_FixedButtonVisibility"));
				callbackBuilder.AddCustomAttributes("FixedLocation", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_FixedLocation"));
				callbackBuilder.AddCustomAttributes("IsFixed", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_IsFixed"));
				callbackBuilder.AddCustomAttributes("IsFixedStateChanging", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_IsFixedStateChanging"));
			});

			// Infragistics.Windows.DataPresenter.FieldLayout
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayout), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("RecordFilters", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayout_P_RecordFilters"));
			});

			// Infragistics.Windows.DataPresenter.LabelPresenter
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.LabelPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FilterButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_LabelPresenter_P_FilterButtonVisibility"));
				callbackBuilder.AddCustomAttributes("FixedButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_LabelPresenter_P_FixedButtonVisibility"));
			});

			// Infragistics.Windows.DataPresenter.RecordListControl
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordListControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FixedNearElementTransform", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordListControl_P_FixedNearElementTransform"));
				callbackBuilder.AddCustomAttributes("ScrollableElementTransform", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordListControl_P_ScrollableElementTransform"));
			});

			// Infragistics.Windows.DataPresenter.RecordManager
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordManager), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("RecordFilters", CreateCategory("LC_Data"), CreateDescription("LD_RecordManager_P_RecordFilters"));
			});

			// Infragistics.Windows.DataPresenter.RecordSelector
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordSelector), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FilterClearButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordSelector_P_FilterClearButtonVisibility"));
				callbackBuilder.AddCustomAttributes("IsFilterRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordSelector_P_IsFilterRecord"));
			});

			#endregion //NA 2009 Vol 1

			#region NA 2009 Vol 2

			// Infragistics.Windows.DataPresenter.DataPresenterBase
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataPresenterBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ClipboardCellDelimiter", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_ClipboardCellDelimiter"));
				callbackBuilder.AddCustomAttributes("ClipboardCellSeparator", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_ClipboardCellSeparator"));
				callbackBuilder.AddCustomAttributes("ClipboardRecordSeparator", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_ClipboardRecordSeparator"));
				callbackBuilder.AddCustomAttributes("GroupByArea", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_GroupByArea"));
				callbackBuilder.AddCustomAttributes("GroupByAreaMode", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_GroupByAreaMode"));
				callbackBuilder.AddCustomAttributes("GroupByAreaMulti", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_GroupByAreaMulti"));
				callbackBuilder.AddCustomAttributes("IsUndoEnabled", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_IsUndoEnabled"));
				callbackBuilder.AddCustomAttributes("UndoLimit", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_UndoLimit"));
				callbackBuilder.AddCustomAttributes("ClipboardCopying", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_ClipboardCopying"));
				callbackBuilder.AddCustomAttributes("ClipboardOperationError", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_ClipboardOperationError"));
				callbackBuilder.AddCustomAttributes("ClipboardPasting", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_ClipboardPasting"));
				callbackBuilder.AddCustomAttributes("DataValueChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_DataValueChanged"));
				callbackBuilder.AddCustomAttributes("FieldChooserOpening", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_FieldChooserOpening"));
				callbackBuilder.AddCustomAttributes("InitializeCellValuePresenter", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_InitializeCellValuePresenter"));
				callbackBuilder.AddCustomAttributes("RecordFixedLocationChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordFixedLocationChanged"));
				callbackBuilder.AddCustomAttributes("RecordFixedLocationChanging", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_E_RecordFixedLocationChanging"));
				callbackBuilder.AddCustomAttributes("DataSourceResetBehavior", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_DataSourceResetBehavior"));
			});

			// Infragistics.Windows.DataPresenter.FieldChooser
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldChooser), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllCurrentFieldsVisible", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooser_P_AllCurrentFieldsVisible"));
				callbackBuilder.AddCustomAttributes("CurrentFieldGroup", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooser_P_CurrentFieldGroup"));
				callbackBuilder.AddCustomAttributes("CurrentFields", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooser_P_CurrentFields"));
				callbackBuilder.AddCustomAttributes("DataPresenter", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooser_P_DataPresenter"));
				callbackBuilder.AddCustomAttributes("DisplayHiddenFieldsOnly", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldChooser_P_DisplayHiddenFieldsOnly"));
				callbackBuilder.AddCustomAttributes("FieldDisplayOrder", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldChooser_P_FieldDisplayOrder"));
				callbackBuilder.AddCustomAttributes("FieldDisplayOrderComparer", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldChooser_P_FieldDisplayOrderComparer"));
				callbackBuilder.AddCustomAttributes("FieldFilters", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldChooser_P_FieldFilters"));
				callbackBuilder.AddCustomAttributes("FieldGroups", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooser_P_FieldGroups"));
				callbackBuilder.AddCustomAttributes("FieldGroupSelectorVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldChooser_P_FieldGroupSelectorVisibility"));
				callbackBuilder.AddCustomAttributes("IsDragItemOver", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooser_P_IsDragItemOver"));
				callbackBuilder.AddCustomAttributes("IsDraggingItem", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooser_P_IsDraggingItem"));
				callbackBuilder.AddCustomAttributes("IsDraggingItemFromDataPresenter", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooser_P_IsDraggingItemFromDataPresenter"));
				callbackBuilder.AddCustomAttributes("SelectedField", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooser_P_SelectedField"));
			});

			// Infragistics.Windows.DataPresenter.DataRecordCellArea
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataRecordCellArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DataError", CreateCategory("LC_Data"), CreateDescription("LD_DataRecordCellArea_P_DataError"));
				callbackBuilder.AddCustomAttributes("HasDataError", CreateCategory("LC_Data"), CreateDescription("LD_DataRecordCellArea_P_HasDataError"));
			});


			// Infragistics.Windows.DataPresenter.GroupByAreaBase
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GroupByAreaBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Prompt1Template", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByAreaBase_P_Prompt1Template"));
				callbackBuilder.AddCustomAttributes("Prompt2Template", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByAreaBase_P_Prompt2Template"));
			});

			// Infragistics.Windows.DataPresenter.FieldDragIndicator
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldDragIndicator), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsSourceFromFieldChooser", CreateCategory("LC_Data"), CreateDescription("LD_FieldDragIndicator_P_IsSourceFromFieldChooser"));
				callbackBuilder.AddCustomAttributes("IsSourceFromGroupByArea", CreateCategory("LC_Data"), CreateDescription("LD_FieldDragIndicator_P_IsSourceFromGroupByArea"));
			});

			// Infragistics.Windows.DataPresenter.FixedRecordButton
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FixedRecordButton), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsFixedOnBottomAllowed", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedRecordButton_P_IsFixedOnBottomAllowed"));
				callbackBuilder.AddCustomAttributes("IsFixedOnTopAllowed", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedRecordButton_P_IsFixedOnTopAllowed"));
				callbackBuilder.AddCustomAttributes("FixToTopPrompt", CreateCategory("LC_Appearance"), CreateDescription("LD_FixedRecordButton_P_FixToTopPrompt"));
				callbackBuilder.AddCustomAttributes("FixToBottomPrompt", CreateCategory("LC_Appearance"), CreateDescription("LD_FixedRecordButton_P_FixToBottomPrompt"));
				callbackBuilder.AddCustomAttributes("UnfixPrompt", CreateCategory("LC_Appearance"), CreateDescription("LD_FixedRecordButton_P_UnfixPrompt"));
				callbackBuilder.AddCustomAttributes("CurrentCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedRecordButton_P_CurrentCommand"));
				callbackBuilder.AddCustomAttributes("FixToTopCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedRecordButton_P_FixToTopCommand"));
				callbackBuilder.AddCustomAttributes("FixToBottomCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedRecordButton_P_FixToBottomCommand"));
				callbackBuilder.AddCustomAttributes("UnfixCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_FixedRecordButton_P_UnfixCommand"));
			});

			// Infragistics.Windows.DataPresenter.CellValuePresenter
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CellValuePresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DataError", CreateCategory("LC_Data"), CreateDescription("LD_CellValuePresenter_P_DataError"));
				callbackBuilder.AddCustomAttributes("HasDataError", CreateCategory("LC_Data"), CreateDescription("LD_CellValuePresenter_P_HasDataError"));
				callbackBuilder.AddCustomAttributes("IsDataErrorDisplayModeHighlight", CreateCategory("LC_Appearance"), CreateDescription("LD_CellValuePresenter_P_IsDataErrorDisplayModeHighlight"));
				callbackBuilder.AddCustomAttributes("IsDataErrorDisplayModeIcon", CreateCategory("LC_Appearance"), CreateDescription("LD_CellValuePresenter_P_IsDataErrorDisplayModeIcon"));
				callbackBuilder.AddCustomAttributes("IsDataErrorTemplateActive", CreateCategory("LC_Data"), CreateDescription("LD_CellValuePresenter_P_IsDataErrorTemplateActive"));
				callbackBuilder.AddCustomAttributes("IsMouseOverRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_CellValuePresenter_P_IsMouseOverRecord"));
				callbackBuilder.AddCustomAttributes("IsRecordSelected", CreateCategory("LC_Behavior"), CreateDescription("LD_CellValuePresenter_P_IsRecordSelected"));
				callbackBuilder.AddCustomAttributes("ValueHistory", CreateCategory("LC_Behavior"), CreateDescription("LD_CellValuePresenter_P_ValueHistory"));
			});

			// Infragistics.Windows.DataPresenter.RecordSelector
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordSelector), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DataError", CreateCategory("LC_Data"), CreateDescription("LD_RecordSelector_P_DataError"));
				callbackBuilder.AddCustomAttributes("FixedButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordSelector_P_FixedButtonVisibility"));
				callbackBuilder.AddCustomAttributes("HasDataError", CreateCategory("LC_Data"), CreateDescription("LD_RecordSelector_P_HasDataError"));
				callbackBuilder.AddCustomAttributes("IsDataErrorDisplayModeHighlight", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordSelector_P_IsDataErrorDisplayModeHighlight"));
				callbackBuilder.AddCustomAttributes("IsDataErrorDisplayModeIcon", CreateCategory("LC_Appearance"), CreateDescription("LD_RecordSelector_P_IsDataErrorDisplayModeIcon"));
				callbackBuilder.AddCustomAttributes("IsFixedOnBottomAllowed", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordSelector_P_IsFixedOnBottomAllowed"));
				callbackBuilder.AddCustomAttributes("IsFixedOnTopAllowed", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordSelector_P_IsFixedOnTopAllowed"));
			});

			// Infragistics.Windows.DataPresenter.GroupByAreaMultiPanel
			// ========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GroupByAreaMultiPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ConnectorLinePen", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByAreaMultiPanel_P_ConnectorLinePen"));
			});

			// Infragistics.Windows.DataPresenter.FieldChooserFilter
			// =====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldChooserFilter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Description", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooserFilter_P_Description"));
				callbackBuilder.AddCustomAttributes("Filter", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldChooserFilter_P_Filter"));
				callbackBuilder.AddCustomAttributes("ToolTip", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooserFilter_P_ToolTip"));
			});

			// Infragistics.Windows.DataPresenter.FieldLayout
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayout), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("KeyMatchingEnforced", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayout_P_KeyMatchingEnforced"));
				callbackBuilder.AddCustomAttributes("ParentFieldLayoutKey", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayout_P_ParentFieldLayoutKey"));
				callbackBuilder.AddCustomAttributes("ParentFieldName", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayout_P_ParentFieldName"));
				callbackBuilder.AddCustomAttributes("ToolTip", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayout_P_ToolTip"));
			});

			// Infragistics.Windows.DataPresenter.Field
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.Field), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DisallowModificationViaClipboard", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_DisallowModificationViaClipboard"));
				callbackBuilder.AddCustomAttributes("Height", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_Height"));
				callbackBuilder.AddCustomAttributes("ToolTip", CreateCategory("LC_Data"), CreateDescription("LD_Field_P_ToolTip"));
				callbackBuilder.AddCustomAttributes("Width", CreateCategory("LC_Behavior"), CreateDescription("LD_Field_P_Width"));
			});

			// Infragistics.Windows.DataPresenter.FieldLayoutSettings
			// ======================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayoutSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowRecordFixing", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AllowRecordFixing"));
				callbackBuilder.AddCustomAttributes("AllowClipboardOperations", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AllowClipboardOperations"));
				callbackBuilder.AddCustomAttributes("AllowHidingViaFieldChooser", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AllowHidingViaFieldChooser"));
				callbackBuilder.AddCustomAttributes("AutoFitMode", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_AutoFitMode"));
				callbackBuilder.AddCustomAttributes("CopyFieldLabelsToClipboard", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_CopyFieldLabelsToClipboard"));
				callbackBuilder.AddCustomAttributes("DataErrorDisplayMode", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayoutSettings_P_DataErrorDisplayMode"));
				callbackBuilder.AddCustomAttributes("FixedRecordLimit", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_FixedRecordLimit"));
				callbackBuilder.AddCustomAttributes("FixedRecordUIType", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_FixedRecordUIType"));
				callbackBuilder.AddCustomAttributes("FixedRecordSortOrder", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_FixedRecordSortOrder"));
				callbackBuilder.AddCustomAttributes("HeaderPrefixAreaDisplayMode", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutSettings_P_HeaderPrefixAreaDisplayMode"));
				callbackBuilder.AddCustomAttributes("SupportDataErrorInfo", CreateCategory("LC_Data"), CreateDescription("LD_FieldLayoutSettings_P_SupportDataErrorInfo"));
			});

			// Infragistics.Windows.DataPresenter.LabelPresenter
			// =================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.LabelPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsInFieldChooser", CreateCategory("LC_Data"), CreateDescription("LD_LabelPresenter_P_IsInFieldChooser"));
				callbackBuilder.AddCustomAttributes("IsInGroupByArea", CreateCategory("LC_Data"), CreateDescription("LD_LabelPresenter_P_IsInGroupByArea"));
				callbackBuilder.AddCustomAttributes("IsSelectedInFieldChooser", CreateCategory("LC_Data"), CreateDescription("LD_LabelPresenter_P_IsSelectedInFieldChooser"));
			});

			// Infragistics.Windows.DataPresenter.FieldLayoutGroupByInfo
			// =========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayoutGroupByInfo), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FieldLayout", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutGroupByInfo_P_FieldLayout"));
				callbackBuilder.AddCustomAttributes("FieldLayoutDescriptionTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldLayoutGroupByInfo_P_FieldLayoutDescriptionTemplate"));
				callbackBuilder.AddCustomAttributes("FieldLayoutDescriptionVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldLayoutGroupByInfo_P_FieldLayoutDescriptionVisibility"));
			});

			// Infragistics.Windows.DataPresenter.FieldSettings
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowHidingViaDragging", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowHidingViaDragging"));
				callbackBuilder.AddCustomAttributes("AllowHidingViaFieldChooser", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowHidingViaFieldChooser"));
				callbackBuilder.AddCustomAttributes("AutoSizeOptions", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AutoSizeOptions"));
				callbackBuilder.AddCustomAttributes("AutoSizeScope", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AutoSizeScope"));
				callbackBuilder.AddCustomAttributes("DataValueChangedHistoryLimit", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_DataValueChangedHistoryLimit"));
				callbackBuilder.AddCustomAttributes("DataValueChangedNotificationsActive", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_DataValueChangedNotificationsActive"));
				callbackBuilder.AddCustomAttributes("DataValueChangedScope", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_DataValueChangedScope"));
				callbackBuilder.AddCustomAttributes("Height", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_Height"));
				callbackBuilder.AddCustomAttributes("SupportDataErrorInfo", CreateCategory("LC_Data"), CreateDescription("LD_FieldSettings_P_SupportDataErrorInfo"));
				callbackBuilder.AddCustomAttributes("Width", CreateCategory("LC_Layout"), CreateDescription("LD_FieldSettings_P_Width"));
			});

			// Infragistics.Windows.DataPresenter.GroupByAreaMulti
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GroupByAreaMulti), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ConnectorLinePen", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByAreaMulti_P_ConnectorLinePen"));
				callbackBuilder.AddCustomAttributes("FieldLayoutDescriptionTemplate", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByAreaMulti_P_FieldLayoutDescriptionTemplate"));
				callbackBuilder.AddCustomAttributes("FieldLayoutOrientation", CreateCategory("LC_Behavior"), CreateDescription("LD_GroupByAreaMulti_P_FieldLayoutOrientation"));
				callbackBuilder.AddCustomAttributes("FieldLayoutOffsetX", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByAreaMulti_P_FieldLayoutOffsetX"));
				callbackBuilder.AddCustomAttributes("FieldLayoutOffsetY", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByAreaMulti_P_FieldLayoutOffsetY"));
				callbackBuilder.AddCustomAttributes("FieldOffsetX", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByAreaMulti_P_FieldOffsetX"));
				callbackBuilder.AddCustomAttributes("FieldOffsetY", CreateCategory("LC_Appearance"), CreateDescription("LD_GroupByAreaMulti_P_FieldOffsetY"));
			});

			// Infragistics.Windows.DataPresenter.FieldChooserGroup
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldChooserGroup), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FieldFilter", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooserGroup_P_FieldFilter"));
				callbackBuilder.AddCustomAttributes("FieldLayout", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooserGroup_P_FieldLayout"));
				callbackBuilder.AddCustomAttributes("HasFieldFilter", CreateCategory("LC_Data"), CreateDescription("LD_FieldChooserGroup_P_HasFieldFilter"));
			});

			// Infragistics.Windows.DataPresenter.DataRecordPresenter
			// ======================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataRecordPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DataError", CreateCategory("LC_Data"), CreateDescription("LD_DataRecordPresenter_P_DataError"));
				callbackBuilder.AddCustomAttributes("HasDataError", CreateCategory("LC_Data"), CreateDescription("LD_DataRecordPresenter_P_HasDataError"));
			});

			// Infragistics.Windows.DataPresenter.GridViewSettings
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.GridViewSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("UseNestedPanels", CreateCategory("LC_Behavior"), CreateDescription("LD_GridViewSettings_P_UseNestedPanels"));
			});

			#endregion //NA 2009 Vol 2

			#region ToolboxBrowsable

			// AS 1/8/08 ToolboxBrowsable

			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CarouselBreadcrumb), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CarouselBreadcrumbControl), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CarouselItem), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CarouselViewPanel), ToolboxBrowsableAttribute.No);

			// JM TFS8590 10-13-08
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.TabularReportViewPanel), ToolboxBrowsableAttribute.No);

			// SSP 10/12/08 NAS 2008 Vol 2
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.FieldDragIndicator), ToolboxBrowsableAttribute.No);

			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CellPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CellPresenterLayoutElement), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CellValuePresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.DataRecordCellArea), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.DataRecordPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.ExpandableFieldRecordPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.GridViewPanel), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.GroupByArea), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.GroupByAreaFieldListBox), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.GroupByRecordPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.HeaderLabelArea), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.HeaderPrefixArea), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.HeaderPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.LabelPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.RecordSelector), ToolboxBrowsableAttribute.No);

			// AS 6/9/08 NA 2008 Vol 1
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryButton), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryCalculatorSelectionControl), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryCellPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryCellPresenterLayoutElement), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryRecordCellArea), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryRecordContentArea), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryRecordHeaderPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryRecordPrefixArea), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryRecordPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryResultPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.SummaryResultsPresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.GroupBySummariesPresenter), ToolboxBrowsableAttribute.No);

			// JM 01-30-09 TFS13196 - Since these classes exist in the Express version, remove the #if !EXPRESS / #endif
			//#if !EXPRESS		
			// JJD 12/22/08 - NA 2009 vol 1
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.FilterCellValuePresenter), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.FilterButton), ToolboxBrowsableAttribute.No);

			// JM 01-13-09 - NA 2009 vol 1
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CustomFilterSelectionControl), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.FixedFieldButton), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.FixedFieldSplitter), ToolboxBrowsableAttribute.No);

			// JJD 07-22-09 - NA 2009 vol 2
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.FieldChooser), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.FixedRecordButton), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.GridViewPanelFlat), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.GridViewPanelNested), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.GroupByAreaMulti), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.GroupByAreaMultiPanel), ToolboxBrowsableAttribute.No);

			// JM 02-02-10 - NA 2010 vol 1
			// Infragistics.Windows.DataPresenter.XamDataCards
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.XamDataCards), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamDataCards"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamDataCardsAssetLibrary")));

			});

			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CardViewCard), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CardViewPanel), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.CardHeaderPresenter), ToolboxBrowsableAttribute.No);

			// JJD 4/11/11 - TFS7220 - NA 2011 vol 1
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.RecordExportStatusControl), ToolboxBrowsableAttribute.No);

			// AS - NA 11.2 Excel Style Filtering
			builder.AddCustomAttributes(typeof(Infragistics.Windows.DataPresenter.RecordFilterTreeControl), ToolboxBrowsableAttribute.No);

			//#endif
			#endregion //ToolboxBrowsable

			#region NA 2010 Vol 1

			// Infragistics.Windows.DataPresenter.CardHeaderPresenter
			// ======================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CardHeaderPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_CardHeaderPresenter"));

				callbackBuilder.AddCustomAttributes("Card", CreateCategory("LC_CardsControl Properties"), CreateDescription("LD_CardHeaderPresenter_P_Card"));
			});

			// Infragistics.Windows.DataPresenter.CardView
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CardView), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ViewSettings", CreateCategory("LC_Appearance"), CreateDescription("LD_CardView_P_ViewSettings"));
			});

			// Infragistics.Windows.DataPresenter.CardViewSettings
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CardViewSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowCardWidthResizing", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewSettings_P_AllowCardWidthResizing"));
				callbackBuilder.AddCustomAttributes("AllowCardHeightResizing", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewSettings_P_AllowCardHeightResizing"));
				callbackBuilder.AddCustomAttributes("AutoFitCards", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewSettings_P_AutoFitCards"));
				callbackBuilder.AddCustomAttributes("CardHeight", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewSettings_P_CardHeight"));
				callbackBuilder.AddCustomAttributes("CardWidth", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewSettings_P_CardWidth"));
				callbackBuilder.AddCustomAttributes("CollapseCardButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewSettings_P_CollapseCardButtonVisibility"));
				callbackBuilder.AddCustomAttributes("CollapseEmptyCellsButtonVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewSettings_P_CollapseEmptyCellsButtonVisibility"));
				callbackBuilder.AddCustomAttributes("HeaderPath", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewSettings_P_HeaderPath"));
				callbackBuilder.AddCustomAttributes("HeaderVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewSettings_P_HeaderVisibility"));
				callbackBuilder.AddCustomAttributes("InterCardSpacingX", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewSettings_P_InterCardSpacingX"));
				callbackBuilder.AddCustomAttributes("InterCardSpacingY", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewSettings_P_InterCardSpacingY"));
				callbackBuilder.AddCustomAttributes("MaxCardCols", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewSettings_P_MaxCardCols"));
				callbackBuilder.AddCustomAttributes("MaxCardRows", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewSettings_P_MaxCardRows"));
				callbackBuilder.AddCustomAttributes("Orientation", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewSettings_P_Orientation"));
				callbackBuilder.AddCustomAttributes("Padding", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewSettings_P_Padding"));
				callbackBuilder.AddCustomAttributes("RepositionAnimation", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewSettings_P_RepositionAnimation"));
				callbackBuilder.AddCustomAttributes("ShouldAnimateCardPositioning", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewSettings_P_ShouldAnimateCardPositioning"));
				callbackBuilder.AddCustomAttributes("ShouldCollapseCards", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewSettings_P_ShouldCollapseCards"));
				callbackBuilder.AddCustomAttributes("ShouldCollapseEmptyCells", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewSettings_P_ShouldCollapseEmptyCells"));
			});

			// Infragistics.Windows.DataPresenter.SummaryRecordContentArea
			// ===========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryRecordContentArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("HasSummariesInCellArea", CreateCategory("LC_Behavior"), CreateDescription("LD_SummaryRecordContentArea_P_HasSummariesInCellArea"));
			});

			// Infragistics.Windows.DataPresenter.RecordPresenter
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsInCard", CreateCategory("LC_Behavior"), CreateDescription("LD_RecordPresenter_P_IsInCard"));
			});

			// Infragistics.Windows.DataPresenter.FieldSettings
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowHiding", CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_AllowHiding"));
				callbackBuilder.AddCustomAttributes("CollapseWhenEmpty", CreateCategory("LC_Appearance"), CreateDescription("LD_FieldSettings_P_CollapseWhenEmpty"));
			});

			// Infragistics.Windows.DataPresenter.CardViewPanel
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CardViewPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ViewSettings", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewPanel_P_ViewSettings"));
			});

			// Infragistics.Windows.DataPresenter.CardViewCard
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.CardViewCard), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("CollapseCardButtonVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewCard_P_CollapseCardButtonVisibility"));
				callbackBuilder.AddCustomAttributes("CollapseEmptyCellsButtonVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewCard_P_CollapseEmptyCellsButtonVisibility"));
				callbackBuilder.AddCustomAttributes("HasHeader", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewCard_P_HasHeader"));
				callbackBuilder.AddCustomAttributes("Header", CreateCategory("LC_Content"), CreateDescription("LD_CardViewCard_P_Header"));
				callbackBuilder.AddCustomAttributes("HeaderStringFormat", CreateCategory("LC_Control"), CreateDescription("LD_CardViewCard_P_HeaderStringFormat"));
				callbackBuilder.AddCustomAttributes("HeaderTemplate", CreateCategory("LC_Content"), CreateDescription("LD_CardViewCard_P_HeaderTemplate"));
				callbackBuilder.AddCustomAttributes("HeaderTemplateSelector", CreateCategory("LC_Content"), CreateDescription("LD_CardViewCard_P_HeaderTemplateSelector"));
				callbackBuilder.AddCustomAttributes("HeaderVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewCard_P_HeaderVisibility"));
				callbackBuilder.AddCustomAttributes("IsAddRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewCard_P_IsAddRecord"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewCard_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsCollapsed", CreateCategory("LC_Appearance"), CreateDescription("LD_CardViewCard_P_IsCollapsed"));
				callbackBuilder.AddCustomAttributes("IsFilterRecord", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewCard_P_IsFilterRecord"));
				callbackBuilder.AddCustomAttributes("IsSelected", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewCard_P_IsSelected"));
				callbackBuilder.AddCustomAttributes("ShouldCollapseEmptyCells", CreateCategory("LC_Behavior"), CreateDescription("LD_CardViewCard_P_ShouldCollapseEmptyCells"));
                
                // JJD 4/08/10 - TFS30618
                callbackBuilder.AddCustomAttributes("Header", new TypeConverterAttribute(typeof(StringConverter)));
            });

			// Infragistics.Windows.DataPresenter.XamDataCards
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.XamDataCards), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_XamDataCards"));

				callbackBuilder.AddCustomAttributes("ViewSettings", CreateCategory("LC_Appearance"), CreateDescription("LD_XamDataCards_P_ViewSettings"));
			});

			#endregion

			#region NA 2011 Vol 2

			// Infragistics.Windows.DataPresenter.FieldMenuItem
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldMenuItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.DataPresenter.RecordFilterTreeControl
			// ==========================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordFilterTreeControl), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});


			// Infragistics.Windows.DataPresenter.DataPresenterBase
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataPresenterBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("CalculationAdapter", 
					new NewItemTypesAttribute(new Type[] { typeof(Infragistics.Windows.DataPresenter.Calculations.DataPresenterCalculationAdapter) }),
					CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterBase_P_CalculationAdapter"));

			});

			// Infragistics.Windows.DataPresenter.FieldLayout
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayout), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("CalculationReferenceId",
					CreateCategory("LC_Data"), CreateDescription("LD_FieldLayout_P_CalculationReferenceId"));

			});

			// Infragistics.Windows.DataPresenter.Field
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.Field), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("CalculationSettings",
					CreateCategory("LC_Data"), CreateDescription("LD_Field_P_CalculationSettings"));

			});

			// Infragistics.Windows.DataPresenter.SummaryDefinition
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.SummaryDefinition), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("CalculationSettings",
					CreateCategory("LC_Data"), CreateDescription("LD_SummaryDefinition_P_CalculationSettings"));

			});

			// Infragistics.Windows.DataPresenter.DataPresenterCalculationSettingsBase
			// =======================================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.Calculations.DataPresenterCalculationSettingsBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Formula",
					CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterCalculationSettingsBase_P_Formula"));
				callbackBuilder.AddCustomAttributes("ReferenceId",
					CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterCalculationSettingsBase_P_ReferenceId"));
				callbackBuilder.AddCustomAttributes("TreatAsType",
					CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterCalculationSettingsBase_P_TreatAsType"));
				callbackBuilder.AddCustomAttributes("TreatAsTypeName",
					CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterCalculationSettingsBase_P_TreatAsTypeName"),
					new TypeConverterAttribute(typeof(TypeNameStnadardValuesConverter)));
				callbackBuilder.AddCustomAttributes("ValueConverter",
					CreateCategory("LC_Data"), CreateDescription("LD_DataPresenterCalculationSettingsBase_P_ValueConverter"));

			});


			// JM 10-14-11 TFS91820
			// Infragistics.Windows.DataPresenter.FieldSettings
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldSettings), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FilterLabelIconDropDownType",
					CreateCategory("LC_Behavior"), CreateDescription("LD_FieldSettings_P_FilterLabelIconDropDownType"));

			});

			// JJD 4/09/12 - TFS108549
			// Infragistics.Windows.DataPresenter.DataPresenterBase
			// ====================================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataPresenterBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("RecordContainerRetentionMode",
					CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_RecordContainerRetentionMode"));

			});

			#endregion

			// JM 02-26-10 NA 10.1
			#region Misc Attributes

			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.FieldLayout), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Key", new TypeConverterAttribute(typeof(StringConverter)));
			});

			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.RecordFilter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Conditions", new NewItemTypesAttribute(typeof(Infragistics.Windows.Controls.ConditionGroup)));
			});

			#endregion //Misc Attributes

			// JJD 11/30/10 - TFS31984 - added
			#region ScrollBehaviorOnListChange

			// Infragistics.Windows.DataPresenter.DataPresenterBase
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.DataPresenter.DataPresenterBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ScrollBehaviorOnListChange", CreateCategory("LC_Behavior"), CreateDescription("LD_DataPresenterBase_P_ScrollBehaviorOnListChange"));
			});

			#endregion //ScrollBehaviorOnListChange	
    
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