using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Grids.XamGrid.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Grids.XamGrid.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Grids.Primitives.RowsPanel);
				Assembly controlAssembly = t.Assembly;

				#region TypeTypeConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.TypeTypeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TypeTypeConverter Properties

				#region RowsManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowsManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("RowsManager_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource")				);


				tableBuilder.AddCustomAttributes(t, "HeaderRow",
					new DescriptionAttribute(SR.GetString("RowsManager_HeaderRow_Property")),
				    new DisplayNameAttribute("HeaderRow")				);


				tableBuilder.AddCustomAttributes(t, "FooterRow",
					new DescriptionAttribute(SR.GetString("RowsManager_FooterRow_Property")),
				    new DisplayNameAttribute("FooterRow")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowTop",
					new DescriptionAttribute(SR.GetString("RowsManager_AddNewRowTop_Property")),
				    new DisplayNameAttribute("AddNewRowTop")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowBottom",
					new DescriptionAttribute(SR.GetString("RowsManager_AddNewRowBottom_Property")),
				    new DisplayNameAttribute("AddNewRowBottom")				);


				tableBuilder.AddCustomAttributes(t, "PagerRowTop",
					new DescriptionAttribute(SR.GetString("RowsManager_PagerRowTop_Property")),
				    new DisplayNameAttribute("PagerRowTop")				);


				tableBuilder.AddCustomAttributes(t, "PagerRowBottom",
					new DescriptionAttribute(SR.GetString("RowsManager_PagerRowBottom_Property")),
				    new DisplayNameAttribute("PagerRowBottom")				);


				tableBuilder.AddCustomAttributes(t, "FilterRowTop",
					new DescriptionAttribute(SR.GetString("RowsManager_FilterRowTop_Property")),
				    new DisplayNameAttribute("FilterRowTop")				);


				tableBuilder.AddCustomAttributes(t, "FilterRowBottom",
					new DescriptionAttribute(SR.GetString("RowsManager_FilterRowBottom_Property")),
				    new DisplayNameAttribute("FilterRowBottom")				);


				tableBuilder.AddCustomAttributes(t, "SummaryRowTop",
					new DescriptionAttribute(SR.GetString("RowsManager_SummaryRowTop_Property")),
				    new DisplayNameAttribute("SummaryRowTop")				);


				tableBuilder.AddCustomAttributes(t, "SummaryRowBottom",
					new DescriptionAttribute(SR.GetString("RowsManager_SummaryRowBottom_Property")),
				    new DisplayNameAttribute("SummaryRowBottom")				);


				tableBuilder.AddCustomAttributes(t, "CurrentPageIndex",
					new DescriptionAttribute(SR.GetString("RowsManager_CurrentPageIndex_Property")),
				    new DisplayNameAttribute("CurrentPageIndex")				);


				tableBuilder.AddCustomAttributes(t, "GroupByLevel",
					new DescriptionAttribute(SR.GetString("RowsManager_GroupByLevel_Property")),
				    new DisplayNameAttribute("GroupByLevel")				);


				tableBuilder.AddCustomAttributes(t, "GroupedColumn",
					new DescriptionAttribute(SR.GetString("RowsManager_GroupedColumn_Property")),
				    new DisplayNameAttribute("GroupedColumn")				);


				tableBuilder.AddCustomAttributes(t, "RowFiltersCollection",
					new DescriptionAttribute(SR.GetString("RowsManager_RowFiltersCollection_Property")),
				    new DisplayNameAttribute("RowFiltersCollection")				);


				tableBuilder.AddCustomAttributes(t, "RowFiltersCollectionResolved",
					new DescriptionAttribute(SR.GetString("RowsManager_RowFiltersCollectionResolved_Property")),
				    new DisplayNameAttribute("RowFiltersCollectionResolved")				);


				tableBuilder.AddCustomAttributes(t, "SummaryDefinitionCollection",
					new DescriptionAttribute(SR.GetString("RowsManager_SummaryDefinitionCollection_Property")),
				    new DisplayNameAttribute("SummaryDefinitionCollection")				);


				tableBuilder.AddCustomAttributes(t, "SummaryDefinitionCollectionResolved",
					new DescriptionAttribute(SR.GetString("RowsManager_SummaryDefinitionCollectionResolved_Property")),
				    new DisplayNameAttribute("SummaryDefinitionCollectionResolved")				);


				tableBuilder.AddCustomAttributes(t, "SummaryResultCollection",
					new DescriptionAttribute(SR.GetString("RowsManager_SummaryResultCollection_Property")),
				    new DisplayNameAttribute("SummaryResultCollection")				);


				tableBuilder.AddCustomAttributes(t, "Rows",
					new DescriptionAttribute(SR.GetString("RowsManager_Rows_Property")),
				    new DisplayNameAttribute("Rows")				);

				#endregion // RowsManager Properties

				#region RowsManagerBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowsManagerBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ParentRow",
					new DescriptionAttribute(SR.GetString("RowsManagerBase_ParentRow_Property")),
				    new DisplayNameAttribute("ParentRow")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("RowsManagerBase_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);


				tableBuilder.AddCustomAttributes(t, "Level",
					new DescriptionAttribute(SR.GetString("RowsManagerBase_Level_Property")),
				    new DisplayNameAttribute("Level")				);


				tableBuilder.AddCustomAttributes(t, "Rows",
					new DescriptionAttribute(SR.GetString("RowsManagerBase_Rows_Property")),
				    new DisplayNameAttribute("Rows")				);

				#endregion // RowsManagerBase Properties

				#region ColumnSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("ColumnSettings_Column_Property")),
				    new DisplayNameAttribute("Column")				);

				#endregion // ColumnSettings Properties

				#region GroupCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // GroupCellControl Properties

				#region CellControlBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CellControlBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ContentProvider",
					new DescriptionAttribute(SR.GetString("CellControlBase_ContentProvider_Property")),
				    new DisplayNameAttribute("ContentProvider")				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("CellControlBase_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("CellControlBase_Column_Property")),
				    new DisplayNameAttribute("Column")				);


				tableBuilder.AddCustomAttributes(t, "ResizingThreshold",
					new DescriptionAttribute(SR.GetString("CellControlBase_ResizingThreshold_Property")),
				    new DisplayNameAttribute("ResizingThreshold")				);

				#endregion // CellControlBase Properties

				#region GroupByColumnLayoutHeaderCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByColumnLayoutHeaderCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // GroupByColumnLayoutHeaderCellControl Properties

				#region GroupByHeaderCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByHeaderCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "IsFirst",
					new DescriptionAttribute(SR.GetString("GroupByHeaderCellControl_IsFirst_Property")),
				    new DisplayNameAttribute("IsFirst")				);


				tableBuilder.AddCustomAttributes(t, "IsLast",
					new DescriptionAttribute(SR.GetString("GroupByHeaderCellControl_IsLast_Property")),
				    new DisplayNameAttribute("IsLast")				);


				tableBuilder.AddCustomAttributes(t, "Indicator",
					new DescriptionAttribute(SR.GetString("GroupByHeaderCellControl_Indicator_Property")),
				    new DisplayNameAttribute("Indicator")				);

				#endregion // GroupByHeaderCellControl Properties

				#region HeaderCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.HeaderCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDragging",
					new DescriptionAttribute(SR.GetString("HeaderCellControl_IsDragging_Property")),
				    new DisplayNameAttribute("IsDragging")				);


				tableBuilder.AddCustomAttributes(t, "MovingHeader",
					new DescriptionAttribute(SR.GetString("HeaderCellControl_MovingHeader_Property")),
				    new DisplayNameAttribute("MovingHeader")				);


				tableBuilder.AddCustomAttributes(t, "Indicator",
					new DescriptionAttribute(SR.GetString("HeaderCellControl_Indicator_Property")),
				    new DisplayNameAttribute("Indicator")				);


				tableBuilder.AddCustomAttributes(t, "AvailableSummariesOperands",
					new DescriptionAttribute(SR.GetString("HeaderCellControl_AvailableSummariesOperands_Property")),
				    new DisplayNameAttribute("AvailableSummariesOperands")				);

				#endregion // HeaderCellControl Properties

				#region FixedBorderCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixedBorderCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FixedBorderCell Properties

				#region CellBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CellBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Content",
					new DescriptionAttribute(SR.GetString("CellBase_Content_Property")),
				    new DisplayNameAttribute("Content")				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("CellBase_Row_Property")),
				    new DisplayNameAttribute("Row")				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("CellBase_Column_Property")),
				    new DisplayNameAttribute("Column")				);


				tableBuilder.AddCustomAttributes(t, "Control",
					new DescriptionAttribute(SR.GetString("CellBase_Control_Property")),
				    new DisplayNameAttribute("Control")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("CellBase_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "IsActive",
					new DescriptionAttribute(SR.GetString("CellBase_IsActive_Property")),
				    new DisplayNameAttribute("IsActive")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("CellBase_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("CellBase_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "IsEditable",
					new DescriptionAttribute(SR.GetString("CellBase_IsEditable_Property")),
				    new DisplayNameAttribute("IsEditable")				);


				tableBuilder.AddCustomAttributes(t, "Tag",
					new DescriptionAttribute(SR.GetString("CellBase_Tag_Property")),
				    new DisplayNameAttribute("Tag")				);

				#endregion // CellBase Properties

				#region FilterRowSelectorCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterRowSelectorCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // FilterRowSelectorCellControl Properties

				#region RowSelectorCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowSelectorCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // RowSelectorCellControl Properties

				#region VisualSettingsBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.VisualSettingsBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Visibility",
					new DescriptionAttribute(SR.GetString("VisualSettingsBase_Visibility_Property")),
				    new DisplayNameAttribute("Visibility")				);

				#endregion // VisualSettingsBase Properties

				#region StyleSettingsBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.StyleSettingsBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("StyleSettingsBase_Style_Property")),
				    new DisplayNameAttribute("Style")				);

				#endregion // StyleSettingsBase Properties

				#region SettingsBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SettingsBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SettingsBase Properties

				#region SortingBaseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SortingBaseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SortingBaseCommand Properties

				#region ColumnCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnCommandBase Properties

				#region SortAscendingCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SortAscendingCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SortAscendingCommand Properties

				#region SortDescendingCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SortDescendingCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SortDescendingCommand Properties

				#region SortToggleCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SortToggleCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SortToggleCommand Properties

				#region UnsortCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.UnsortCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // UnsortCommand Properties

				#region SelectedRowsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SelectedRowsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Grid",
					new DescriptionAttribute(SR.GetString("SelectedCollectionBase`1_Grid_Property")),
				    new DisplayNameAttribute("Grid")				);

				#endregion // SelectedRowsCollection Properties

				#region SelectedCollectionBase`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SelectedCollectionBase`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Grid",
					new DescriptionAttribute(SR.GetString("SelectedCollectionBase`1_Grid_Property")),
				    new DisplayNameAttribute("Grid")				);

				#endregion // SelectedCollectionBase`1 Properties

				#region RowsFilter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowsFilter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Conditions",
					new DescriptionAttribute(SR.GetString("RowsFilter_Conditions_Property")),
				    new DisplayNameAttribute("Conditions")				);


				tableBuilder.AddCustomAttributes(t, "ObjectType",
					new DescriptionAttribute(SR.GetString("RowsFilter_ObjectType_Property")),
				    new DisplayNameAttribute("ObjectType")				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("RowsFilter_Column_Property")),
				    new DisplayNameAttribute("Column")				);


				tableBuilder.AddCustomAttributes(t, "FieldName",
					new DescriptionAttribute(SR.GetString("RowsFilter_FieldName_Property")),
				    new DisplayNameAttribute("FieldName")				);


				tableBuilder.AddCustomAttributes(t, "FieldType",
					new DescriptionAttribute(SR.GetString("RowsFilter_FieldType_Property")),
				    new DisplayNameAttribute("FieldType")				);


				tableBuilder.AddCustomAttributes(t, "ObjectTypedInfo",
					new DescriptionAttribute(SR.GetString("RowsFilter_ObjectTypedInfo_Property")),
				    new DisplayNameAttribute("ObjectTypedInfo")				);

				#endregion // RowsFilter Properties

				#region FilteringSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FilteringSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowFiltering",
					new DescriptionAttribute(SR.GetString("FilteringSettings_AllowFiltering_Property")),
				    new DisplayNameAttribute("AllowFiltering")				);


				tableBuilder.AddCustomAttributes(t, "AllowFilterRow",
					new DescriptionAttribute(SR.GetString("FilteringSettings_AllowFilterRow_Property")),
				    new DisplayNameAttribute("AllowFilterRow")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorStyle",
					new DescriptionAttribute(SR.GetString("FilteringSettings_ExpansionIndicatorStyle_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "RowFiltersCollection",
					new DescriptionAttribute(SR.GetString("FilteringSettings_RowFiltersCollection_Property")),
				    new DisplayNameAttribute("RowFiltersCollection")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("FilteringSettings_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorStyle",
					new DescriptionAttribute(SR.GetString("FilteringSettings_RowSelectorStyle_Property")),
				    new DisplayNameAttribute("RowSelectorStyle")				);


				tableBuilder.AddCustomAttributes(t, "FilteringScope",
					new DescriptionAttribute(SR.GetString("FilteringSettings_FilteringScope_Property")),
				    new DisplayNameAttribute("FilteringScope")				);


				tableBuilder.AddCustomAttributes(t, "FilterRowHeight",
					new DescriptionAttribute(SR.GetString("FilteringSettings_FilterRowHeight_Property")),
				    new DisplayNameAttribute("FilterRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuClearFiltersString",
					new DescriptionAttribute(SR.GetString("FilteringSettings_FilterMenuClearFiltersString_Property")),
				    new DisplayNameAttribute("FilterMenuClearFiltersString")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuCustomFilteringButtonVisibility",
					new DescriptionAttribute(SR.GetString("FilteringSettings_FilterMenuCustomFilteringButtonVisibility_Property")),
				    new DisplayNameAttribute("FilterMenuCustomFilteringButtonVisibility")				);


				tableBuilder.AddCustomAttributes(t, "FilterSelectionControlStyle",
					new DescriptionAttribute(SR.GetString("FilteringSettings_FilterSelectionControlStyle_Property")),
				    new DisplayNameAttribute("FilterSelectionControlStyle")				);


				tableBuilder.AddCustomAttributes(t, "CustomFilterDialogStyle",
					new DescriptionAttribute(SR.GetString("FilteringSettings_CustomFilterDialogStyle_Property")),
				    new DisplayNameAttribute("CustomFilterDialogStyle")				);


				tableBuilder.AddCustomAttributes(t, "SelectAllText",
					new DescriptionAttribute(SR.GetString("FilteringSettings_SelectAllText_Property")),
				    new DisplayNameAttribute("SelectAllText")				);


				tableBuilder.AddCustomAttributes(t, "EmptyValueString",
					new DescriptionAttribute(SR.GetString("FilteringSettings_EmptyValueString_Property")),
				    new DisplayNameAttribute("EmptyValueString")				);


				tableBuilder.AddCustomAttributes(t, "NullValueString",
					new DescriptionAttribute(SR.GetString("FilteringSettings_NullValueString_Property")),
				    new DisplayNameAttribute("NullValueString")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuSelectionListGeneration",
					new DescriptionAttribute(SR.GetString("FilteringSettings_FilterMenuSelectionListGeneration_Property")),
				    new DisplayNameAttribute("FilterMenuSelectionListGeneration")				);

				#endregion // FilteringSettings Properties

				#region EditingSettingsBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EditingSettingsBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsF2EditingEnabled",
					new DescriptionAttribute(SR.GetString("EditingSettingsBase_IsF2EditingEnabled_Property")),
				    new DisplayNameAttribute("IsF2EditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsEnterKeyEditingEnabled",
					new DescriptionAttribute(SR.GetString("EditingSettingsBase_IsEnterKeyEditingEnabled_Property")),
				    new DisplayNameAttribute("IsEnterKeyEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsMouseActionEditingEnabled",
					new DescriptionAttribute(SR.GetString("EditingSettingsBase_IsMouseActionEditingEnabled_Property")),
				    new DisplayNameAttribute("IsMouseActionEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsOnCellActiveEditingEnabled",
					new DescriptionAttribute(SR.GetString("EditingSettingsBase_IsOnCellActiveEditingEnabled_Property")),
				    new DisplayNameAttribute("IsOnCellActiveEditingEnabled")				);

				#endregion // EditingSettingsBase Properties

				#region ResizingIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ResizingIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ResizingIndicator Properties

				#region ColumnResizingSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnResizingSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Indicator",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_Indicator_Property")),
				    new DisplayNameAttribute("Indicator")				);


				tableBuilder.AddCustomAttributes(t, "IndicatorContainer",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_IndicatorContainer_Property")),
				    new DisplayNameAttribute("IndicatorContainer")				);


				tableBuilder.AddCustomAttributes(t, "IndicatorStyle",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_IndicatorStyle_Property")),
				    new DisplayNameAttribute("IndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "IndicatorStyleResolved",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_IndicatorStyleResolved_Property")),
				    new DisplayNameAttribute("IndicatorStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "AllowColumnResizing",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_AllowColumnResizing_Property")),
				    new DisplayNameAttribute("AllowColumnResizing")				);


				tableBuilder.AddCustomAttributes(t, "AllowColumnResizingResolved",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_AllowColumnResizingResolved_Property")),
				    new DisplayNameAttribute("AllowColumnResizingResolved")				);


				tableBuilder.AddCustomAttributes(t, "AllowDoubleClickToSize",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_AllowDoubleClickToSize_Property")),
				    new DisplayNameAttribute("AllowDoubleClickToSize")				);


				tableBuilder.AddCustomAttributes(t, "AllowDoubleClickToSizeResolved",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_AllowDoubleClickToSizeResolved_Property")),
				    new DisplayNameAttribute("AllowDoubleClickToSizeResolved")				);


				tableBuilder.AddCustomAttributes(t, "AllowMultipleColumnResize",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_AllowMultipleColumnResize_Property")),
				    new DisplayNameAttribute("AllowMultipleColumnResize")				);


				tableBuilder.AddCustomAttributes(t, "AllowMultipleColumnResizeResolved",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_AllowMultipleColumnResizeResolved_Property")),
				    new DisplayNameAttribute("AllowMultipleColumnResizeResolved")				);


				tableBuilder.AddCustomAttributes(t, "AllowCellAreaResizing",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_AllowCellAreaResizing_Property")),
				    new DisplayNameAttribute("AllowCellAreaResizing")				);


				tableBuilder.AddCustomAttributes(t, "AllowCellAreaResizingResolved",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettingsOverride_AllowCellAreaResizingResolved_Property")),
				    new DisplayNameAttribute("AllowCellAreaResizingResolved")				);

				#endregion // ColumnResizingSettingsOverride Properties

				#region SettingsOverrideBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SettingsOverrideBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SettingsOverrideBase Properties

				#region ColumnMovingSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnMovingSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IndicatorStyle",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettingsOverride_IndicatorStyle_Property")),
				    new DisplayNameAttribute("IndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "IndicatorStyleResolved",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettingsOverride_IndicatorStyleResolved_Property")),
				    new DisplayNameAttribute("IndicatorStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "AllowColumnMoving",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettingsOverride_AllowColumnMoving_Property")),
				    new DisplayNameAttribute("AllowColumnMoving")				);


				tableBuilder.AddCustomAttributes(t, "AllowColumnMovingResolved",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettingsOverride_AllowColumnMovingResolved_Property")),
				    new DisplayNameAttribute("AllowColumnMovingResolved")				);


				tableBuilder.AddCustomAttributes(t, "EasingFunction",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettingsOverride_EasingFunction_Property")),
				    new DisplayNameAttribute("EasingFunction")				);


				tableBuilder.AddCustomAttributes(t, "EasingFunctionResolved",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettingsOverride_EasingFunctionResolved_Property")),
				    new DisplayNameAttribute("EasingFunctionResolved")				);


				tableBuilder.AddCustomAttributes(t, "AnimationDuration",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettingsOverride_AnimationDuration_Property")),
				    new DisplayNameAttribute("AnimationDuration")				);


				tableBuilder.AddCustomAttributes(t, "AnimationDurationResolved",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettingsOverride_AnimationDurationResolved_Property")),
				    new DisplayNameAttribute("AnimationDurationResolved")				);

				#endregion // ColumnMovingSettingsOverride Properties

				#region ClipboardSortRowComparer Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ClipboardSortRowComparer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ClipboardSortRowComparer Properties

				#region NullableFloatConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.NullableFloatConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NullableFloatConverter Properties

				#region SummaryRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SummaryRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("SummaryRow_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("SummaryRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "HasChildren",
					new DescriptionAttribute(SR.GetString("SummaryRow_HasChildren_Property")),
				    new DisplayNameAttribute("HasChildren")				);

				#endregion // SummaryRow Properties

				#region Row Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Row");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HasChildren",
					new DescriptionAttribute(SR.GetString("Row_HasChildren_Property")),
				    new DisplayNameAttribute("HasChildren")				);


				tableBuilder.AddCustomAttributes(t, "IsAlternateRow",
					new DescriptionAttribute(SR.GetString("Row_IsAlternateRow_Property")),
				    new DisplayNameAttribute("IsAlternateRow")				);


				tableBuilder.AddCustomAttributes(t, "ChildBands",
					new DescriptionAttribute(SR.GetString("Row_ChildBands_Property")),
				    new DisplayNameAttribute("ChildBands")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("Row_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "CellStyle",
					new DescriptionAttribute(SR.GetString("Row_CellStyle_Property")),
				    new DisplayNameAttribute("CellStyle")				);


				tableBuilder.AddCustomAttributes(t, "ParentRow",
					new DescriptionAttribute(SR.GetString("Row_ParentRow_Property")),
				    new DisplayNameAttribute("ParentRow")				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("Row_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);

				#endregion // Row Properties

				#region ExpandableRowBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ExpandableRowBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsExpanded",
					new DescriptionAttribute(SR.GetString("ExpandableRowBase_IsExpanded_Property")),
				    new DisplayNameAttribute("IsExpanded")				);


				tableBuilder.AddCustomAttributes(t, "HasChildren",
					new DescriptionAttribute(SR.GetString("ExpandableRowBase_HasChildren_Property")),
				    new DisplayNameAttribute("HasChildren")				);

				#endregion // ExpandableRowBase Properties

				#region RowBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Data",
					new DescriptionAttribute(SR.GetString("RowBase_Data_Property")),
				    new DisplayNameAttribute("Data")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("RowBase_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);


				tableBuilder.AddCustomAttributes(t, "Manager",
					new DescriptionAttribute(SR.GetString("RowBase_Manager_Property")),
				    new DisplayNameAttribute("Manager")				);


				tableBuilder.AddCustomAttributes(t, "Cells",
					new DescriptionAttribute(SR.GetString("RowBase_Cells_Property")),
				    new DisplayNameAttribute("Cells")				);


				tableBuilder.AddCustomAttributes(t, "Control",
					new DescriptionAttribute(SR.GetString("RowBase_Control_Property")),
				    new DisplayNameAttribute("Control")				);


				tableBuilder.AddCustomAttributes(t, "Columns",
					new DescriptionAttribute(SR.GetString("RowBase_Columns_Property")),
				    new DisplayNameAttribute("Columns")				);


				tableBuilder.AddCustomAttributes(t, "IsAlternateRow",
					new DescriptionAttribute(SR.GetString("RowBase_IsAlternateRow_Property")),
				    new DisplayNameAttribute("IsAlternateRow")				);


				tableBuilder.AddCustomAttributes(t, "ActualHeight",
					new DescriptionAttribute(SR.GetString("RowBase_ActualHeight_Property")),
				    new DisplayNameAttribute("ActualHeight")				);


				tableBuilder.AddCustomAttributes(t, "IsMouseOver",
					new DescriptionAttribute(SR.GetString("RowBase_IsMouseOver_Property")),
				    new DisplayNameAttribute("IsMouseOver")				);


				tableBuilder.AddCustomAttributes(t, "IsActive",
					new DescriptionAttribute(SR.GetString("RowBase_IsActive_Property")),
				    new DisplayNameAttribute("IsActive")				);


				tableBuilder.AddCustomAttributes(t, "Tag",
					new DescriptionAttribute(SR.GetString("RowBase_Tag_Property")),
				    new DisplayNameAttribute("Tag")				);


				tableBuilder.AddCustomAttributes(t, "Index",
					new DescriptionAttribute(SR.GetString("RowBase_Index_Property")),
				    new DisplayNameAttribute("Index")				);


				tableBuilder.AddCustomAttributes(t, "Level",
					new DescriptionAttribute(SR.GetString("RowBase_Level_Property")),
				    new DisplayNameAttribute("Level")				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("RowBase_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "Height",
					new DescriptionAttribute(SR.GetString("RowBase_Height_Property")),
				    new DisplayNameAttribute("Height")				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("RowBase_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "MinimumRowHeight",
					new DescriptionAttribute(SR.GetString("RowBase_MinimumRowHeight_Property")),
				    new DisplayNameAttribute("MinimumRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "MinimumRowHeightResolved",
					new DescriptionAttribute(SR.GetString("RowBase_MinimumRowHeightResolved_Property")),
				    new DisplayNameAttribute("MinimumRowHeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "MergeData",
					new DescriptionAttribute(SR.GetString("RowBase_MergeData_Property")),
				    new DisplayNameAttribute("MergeData")				);

				#endregion // RowBase Properties

				#region RowBaseCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowBaseCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ActualCollection",
					new DescriptionAttribute(SR.GetString("RowBaseCollection_ActualCollection_Property")),
				    new DisplayNameAttribute("ActualCollection")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("RowBaseCollection_Count_Property")),
				    new DisplayNameAttribute("Count")				);

				#endregion // RowBaseCollection Properties

				#region ChildBand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ChildBand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ParentRow",
					new DescriptionAttribute(SR.GetString("ChildBand_ParentRow_Property")),
				    new DisplayNameAttribute("ParentRow")				);


				tableBuilder.AddCustomAttributes(t, "ResolveIsVisible",
					new DescriptionAttribute(SR.GetString("ChildBand_ResolveIsVisible_Property")),
				    new DisplayNameAttribute("ResolveIsVisible")				);


				tableBuilder.AddCustomAttributes(t, "Rows",
					new DescriptionAttribute(SR.GetString("ChildBand_Rows_Property")),
				    new DisplayNameAttribute("Rows")				);


				tableBuilder.AddCustomAttributes(t, "CurrentPageIndex",
					new DescriptionAttribute(SR.GetString("ChildBand_CurrentPageIndex_Property")),
				    new DisplayNameAttribute("CurrentPageIndex")				);


				tableBuilder.AddCustomAttributes(t, "RowFiltersCollection",
					new DescriptionAttribute(SR.GetString("ChildBand_RowFiltersCollection_Property")),
				    new DisplayNameAttribute("RowFiltersCollection")				);


				tableBuilder.AddCustomAttributes(t, "SummaryDefinitionCollection",
					new DescriptionAttribute(SR.GetString("ChildBand_SummaryDefinitionCollection_Property")),
				    new DisplayNameAttribute("SummaryDefinitionCollection")				);


				tableBuilder.AddCustomAttributes(t, "SummaryResultCollection",
					new DescriptionAttribute(SR.GetString("ChildBand_SummaryResultCollection_Property")),
				    new DisplayNameAttribute("SummaryResultCollection")				);


				tableBuilder.AddCustomAttributes(t, "HasChildren",
					new DescriptionAttribute(SR.GetString("ChildBand_HasChildren_Property")),
				    new DisplayNameAttribute("HasChildren")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("ChildBand_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);


				tableBuilder.AddCustomAttributes(t, "Cells",
					new DescriptionAttribute(SR.GetString("ChildBand_Cells_Property")),
				    new DisplayNameAttribute("Cells")				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("ChildBand_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("ChildBand_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);

				#endregion // ChildBand Properties

				#region AddNewRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.AddNewRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("AddNewRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("AddNewRow_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "HasChildren",
					new DescriptionAttribute(SR.GetString("AddNewRow_HasChildren_Property")),
				    new DisplayNameAttribute("HasChildren")				);

				#endregion // AddNewRow Properties

				#region TemplateColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.TemplateColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplate",
					new DescriptionAttribute(SR.GetString("TemplateColumn_ItemTemplate_Property")),
				    new DisplayNameAttribute("ItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "EditorTemplate",
					new DescriptionAttribute(SR.GetString("TemplateColumn_EditorTemplate_Property")),
				    new DisplayNameAttribute("EditorTemplate")				);


				tableBuilder.AddCustomAttributes(t, "FilterItemTemplate",
					new DescriptionAttribute(SR.GetString("TemplateColumn_FilterItemTemplate_Property")),
				    new DisplayNameAttribute("FilterItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "FilterEditorTemplate",
					new DescriptionAttribute(SR.GetString("TemplateColumn_FilterEditorTemplate_Property")),
				    new DisplayNameAttribute("FilterEditorTemplate")				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("TemplateColumn_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);


				tableBuilder.AddCustomAttributes(t, "EditorValueConverter",
					new DescriptionAttribute(SR.GetString("TemplateColumn_EditorValueConverter_Property")),
				    new DisplayNameAttribute("EditorValueConverter")				);


				tableBuilder.AddCustomAttributes(t, "EditorValueConverterParameter",
					new DescriptionAttribute(SR.GetString("TemplateColumn_EditorValueConverterParameter_Property")),
				    new DisplayNameAttribute("EditorValueConverterParameter")				);


				tableBuilder.AddCustomAttributes(t, "ValueConverter",
					new DescriptionAttribute(SR.GetString("TemplateColumn_ValueConverter_Property")),
				    new DisplayNameAttribute("ValueConverter")				);


				tableBuilder.AddCustomAttributes(t, "ValueConverterParameter",
					new DescriptionAttribute(SR.GetString("TemplateColumn_ValueConverterParameter_Property")),
				    new DisplayNameAttribute("ValueConverterParameter")				);

				#endregion // TemplateColumn Properties

				#region EditableColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EditableColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "EditorStyle",
					new DescriptionAttribute(SR.GetString("EditableColumn_EditorStyle_Property")),
				    new DisplayNameAttribute("EditorStyle")				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("EditableColumn_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);


				tableBuilder.AddCustomAttributes(t, "EditorValueConverter",
					new DescriptionAttribute(SR.GetString("EditableColumn_EditorValueConverter_Property")),
				    new DisplayNameAttribute("EditorValueConverter")				);


				tableBuilder.AddCustomAttributes(t, "EditorValueConverterParameter",
					new DescriptionAttribute(SR.GetString("EditableColumn_EditorValueConverterParameter_Property")),
				    new DisplayNameAttribute("EditorValueConverterParameter")				);


				tableBuilder.AddCustomAttributes(t, "AllowEditingValidation",
					new DescriptionAttribute(SR.GetString("EditableColumn_AllowEditingValidation_Property")),
				    new DisplayNameAttribute("AllowEditingValidation")				);


				tableBuilder.AddCustomAttributes(t, "EditorHorizontalContentAlignment",
					new DescriptionAttribute(SR.GetString("EditableColumn_EditorHorizontalContentAlignment_Property")),
				    new DisplayNameAttribute("EditorHorizontalContentAlignment")				);


				tableBuilder.AddCustomAttributes(t, "EditorVerticalContentAlignment",
					new DescriptionAttribute(SR.GetString("EditableColumn_EditorVerticalContentAlignment_Property")),
				    new DisplayNameAttribute("EditorVerticalContentAlignment")				);

				#endregion // EditableColumn Properties

				#region Column Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Column");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Width",
					new DescriptionAttribute(SR.GetString("Column_Width_Property")),
				    new DisplayNameAttribute("Width")				);


				tableBuilder.AddCustomAttributes(t, "WidthResolved",
					new DescriptionAttribute(SR.GetString("Column_WidthResolved_Property")),
				    new DisplayNameAttribute("WidthResolved")				);


				tableBuilder.AddCustomAttributes(t, "ActualWidth",
					new DescriptionAttribute(SR.GetString("Column_ActualWidth_Property")),
				    new DisplayNameAttribute("ActualWidth")				);


				tableBuilder.AddCustomAttributes(t, "MinimumWidth",
					new DescriptionAttribute(SR.GetString("Column_MinimumWidth_Property")),
				    new DisplayNameAttribute("MinimumWidth")				);


				tableBuilder.AddCustomAttributes(t, "MaximumWidth",
					new DescriptionAttribute(SR.GetString("Column_MaximumWidth_Property")),
				    new DisplayNameAttribute("MaximumWidth")				);


				tableBuilder.AddCustomAttributes(t, "FooterTemplate",
					new DescriptionAttribute(SR.GetString("Column_FooterTemplate_Property")),
				    new DisplayNameAttribute("FooterTemplate")				);


				tableBuilder.AddCustomAttributes(t, "FooterText",
					new DescriptionAttribute(SR.GetString("Column_FooterText_Property")),
				    new DisplayNameAttribute("FooterText")				);


				tableBuilder.AddCustomAttributes(t, "IsFixed",
					new DescriptionAttribute(SR.GetString("Column_IsFixed_Property")),
				    new DisplayNameAttribute("IsFixed")				);


				tableBuilder.AddCustomAttributes(t, "FixedIndicatorDirection",
					new DescriptionAttribute(SR.GetString("Column_FixedIndicatorDirection_Property")),
				    new DisplayNameAttribute("FixedIndicatorDirection")				);


				tableBuilder.AddCustomAttributes(t, "FixedIndicatorDirectionResolved",
					new DescriptionAttribute(SR.GetString("Column_FixedIndicatorDirectionResolved_Property")),
				    new DisplayNameAttribute("FixedIndicatorDirectionResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsFixable",
					new DescriptionAttribute(SR.GetString("Column_IsFixable_Property")),
				    new DisplayNameAttribute("IsFixable")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalContentAlignment",
					new DescriptionAttribute(SR.GetString("Column_HorizontalContentAlignment_Property")),
				    new DisplayNameAttribute("HorizontalContentAlignment")				);


				tableBuilder.AddCustomAttributes(t, "VerticalContentAlignment",
					new DescriptionAttribute(SR.GetString("Column_VerticalContentAlignment_Property")),
				    new DisplayNameAttribute("VerticalContentAlignment")				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("Column_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);


				tableBuilder.AddCustomAttributes(t, "AllowCaseSensitiveSort",
					new DescriptionAttribute(SR.GetString("Column_AllowCaseSensitiveSort_Property")),
				    new DisplayNameAttribute("AllowCaseSensitiveSort")				);


				tableBuilder.AddCustomAttributes(t, "SortComparer",
					new DescriptionAttribute(SR.GetString("Column_SortComparer_Property")),
				    new DisplayNameAttribute("SortComparer")				);


				tableBuilder.AddCustomAttributes(t, "IsMovable",
					new DescriptionAttribute(SR.GetString("Column_IsMovable_Property")),
				    new DisplayNameAttribute("IsMovable")				);


				tableBuilder.AddCustomAttributes(t, "IsResizable",
					new DescriptionAttribute(SR.GetString("Column_IsResizable_Property")),
				    new DisplayNameAttribute("IsResizable")				);


				tableBuilder.AddCustomAttributes(t, "IsFilterable",
					new DescriptionAttribute(SR.GetString("Column_IsFilterable_Property")),
				    new DisplayNameAttribute("IsFilterable")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("Column_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "IsSorted",
					new DescriptionAttribute(SR.GetString("Column_IsSorted_Property")),
				    new DisplayNameAttribute("IsSorted")				);


				tableBuilder.AddCustomAttributes(t, "IsSummable",
					new DescriptionAttribute(SR.GetString("Column_IsSummable_Property")),
				    new DisplayNameAttribute("IsSummable")				);


				tableBuilder.AddCustomAttributes(t, "IsGroupable",
					new DescriptionAttribute(SR.GetString("Column_IsGroupable_Property")),
				    new DisplayNameAttribute("IsGroupable")				);


				tableBuilder.AddCustomAttributes(t, "IsGroupBy",
					new DescriptionAttribute(SR.GetString("Column_IsGroupBy_Property")),
				    new DisplayNameAttribute("IsGroupBy")				);


				tableBuilder.AddCustomAttributes(t, "ValueConverter",
					new DescriptionAttribute(SR.GetString("Column_ValueConverter_Property")),
				    new DisplayNameAttribute("ValueConverter")				);


				tableBuilder.AddCustomAttributes(t, "ValueConverterParameter",
					new DescriptionAttribute(SR.GetString("Column_ValueConverterParameter_Property")),
				    new DisplayNameAttribute("ValueConverterParameter")				);


				tableBuilder.AddCustomAttributes(t, "GroupByItemTemplate",
					new DescriptionAttribute(SR.GetString("Column_GroupByItemTemplate_Property")),
				    new DisplayNameAttribute("GroupByItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "GroupByComparer",
					new DescriptionAttribute(SR.GetString("Column_GroupByComparer_Property")),
				    new DisplayNameAttribute("GroupByComparer")				);


				tableBuilder.AddCustomAttributes(t, "SummaryColumnSettings",
					new DescriptionAttribute(SR.GetString("Column_SummaryColumnSettings_Property")),
				    new DisplayNameAttribute("SummaryColumnSettings")				);


				tableBuilder.AddCustomAttributes(t, "FilterColumnSettings",
					new DescriptionAttribute(SR.GetString("Column_FilterColumnSettings_Property")),
				    new DisplayNameAttribute("FilterColumnSettings")				);


				tableBuilder.AddCustomAttributes(t, "FirstSortDirection",
					new DescriptionAttribute(SR.GetString("Column_FirstSortDirection_Property")),
				    new DisplayNameAttribute("FirstSortDirection")				);


				tableBuilder.AddCustomAttributes(t, "FirstSortDirectionResolved",
					new DescriptionAttribute(SR.GetString("Column_FirstSortDirectionResolved_Property")),
				    new DisplayNameAttribute("FirstSortDirectionResolved")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextHorizontalAlignment",
					new DescriptionAttribute(SR.GetString("Column_HeaderTextHorizontalAlignment_Property")),
				    new DisplayNameAttribute("HeaderTextHorizontalAlignment")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextHorizontalAlignmentResolved",
					new DescriptionAttribute(SR.GetString("Column_HeaderTextHorizontalAlignmentResolved_Property")),
				    new DisplayNameAttribute("HeaderTextHorizontalAlignmentResolved")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextVerticalAlignment",
					new DescriptionAttribute(SR.GetString("Column_HeaderTextVerticalAlignment_Property")),
				    new DisplayNameAttribute("HeaderTextVerticalAlignment")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextVerticalAlignmentResolved",
					new DescriptionAttribute(SR.GetString("Column_HeaderTextVerticalAlignmentResolved_Property")),
				    new DisplayNameAttribute("HeaderTextVerticalAlignmentResolved")				);


				tableBuilder.AddCustomAttributes(t, "ConditionalFormatCollection",
					new DescriptionAttribute(SR.GetString("Column_ConditionalFormatCollection_Property")),
				    new DisplayNameAttribute("ConditionalFormatCollection")				);


				tableBuilder.AddCustomAttributes(t, "ToolTipStyle",
					new DescriptionAttribute(SR.GetString("Column_ToolTipStyle_Property")),
				    new DisplayNameAttribute("ToolTipStyle")				);


				tableBuilder.AddCustomAttributes(t, "ToolTipContentTemplate",
					new DescriptionAttribute(SR.GetString("Column_ToolTipContentTemplate_Property")),
				    new DisplayNameAttribute("ToolTipContentTemplate")				);


				tableBuilder.AddCustomAttributes(t, "AllowToolTips",
					new DescriptionAttribute(SR.GetString("Column_AllowToolTips_Property")),
				    new DisplayNameAttribute("AllowToolTips")				);


				tableBuilder.AddCustomAttributes(t, "AllColumns",
					new DescriptionAttribute(SR.GetString("Column_AllColumns_Property")),
				    new DisplayNameAttribute("AllColumns")				);


				tableBuilder.AddCustomAttributes(t, "AllVisibleChildColumns",
					new DescriptionAttribute(SR.GetString("Column_AllVisibleChildColumns_Property")),
				    new DisplayNameAttribute("AllVisibleChildColumns")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowItemTemplate",
					new DescriptionAttribute(SR.GetString("Column_AddNewRowItemTemplate_Property")),
				    new DisplayNameAttribute("AddNewRowItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowEditorTemplate",
					new DescriptionAttribute(SR.GetString("Column_AddNewRowEditorTemplate_Property")),
				    new DisplayNameAttribute("AddNewRowEditorTemplate")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowItemTemplateHorizontalContentAlignment",
					new DescriptionAttribute(SR.GetString("Column_AddNewRowItemTemplateHorizontalContentAlignment_Property")),
				    new DisplayNameAttribute("AddNewRowItemTemplateHorizontalContentAlignment")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowItemTemplateVerticalContentAlignment",
					new DescriptionAttribute(SR.GetString("Column_AddNewRowItemTemplateVerticalContentAlignment_Property")),
				    new DisplayNameAttribute("AddNewRowItemTemplateVerticalContentAlignment")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowEditorTemplateVerticalContentAlignment",
					new DescriptionAttribute(SR.GetString("Column_AddNewRowEditorTemplateVerticalContentAlignment_Property")),
				    new DisplayNameAttribute("AddNewRowEditorTemplateVerticalContentAlignment")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowEditorTemplateHorizontalContentAlignment",
					new DescriptionAttribute(SR.GetString("Column_AddNewRowEditorTemplateHorizontalContentAlignment_Property")),
				    new DisplayNameAttribute("AddNewRowEditorTemplateHorizontalContentAlignment")				);


				tableBuilder.AddCustomAttributes(t, "MergedItemTemplate",
					new DescriptionAttribute(SR.GetString("Column_MergedItemTemplate_Property")),
				    new DisplayNameAttribute("MergedItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowCellStyle",
					new DescriptionAttribute(SR.GetString("Column_AddNewRowCellStyle_Property")),
				    new DisplayNameAttribute("AddNewRowCellStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupByRowStyle",
					new DescriptionAttribute(SR.GetString("Column_GroupByRowStyle_Property")),
				    new DisplayNameAttribute("GroupByRowStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupByHeaderStyle",
					new DescriptionAttribute(SR.GetString("Column_GroupByHeaderStyle_Property")),
				    new DisplayNameAttribute("GroupByHeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "MergeCellStyle",
					new DescriptionAttribute(SR.GetString("Column_MergeCellStyle_Property")),
				    new DisplayNameAttribute("MergeCellStyle")				);

				#endregion // Column Properties

				#region ColumnBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Key",
					new DescriptionAttribute(SR.GetString("ColumnBase_Key_Property")),
				    new DisplayNameAttribute("Key")				);


				tableBuilder.AddCustomAttributes(t, "Visibility",
					new DescriptionAttribute(SR.GetString("ColumnBase_Visibility_Property")),
				    new DisplayNameAttribute("Visibility")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("ColumnBase_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);


				tableBuilder.AddCustomAttributes(t, "CellStyle",
					new DescriptionAttribute(SR.GetString("ColumnBase_CellStyle_Property")),
				    new DisplayNameAttribute("CellStyle")				);


				tableBuilder.AddCustomAttributes(t, "CellStyleResolved",
					new DescriptionAttribute(SR.GetString("ColumnBase_CellStyleResolved_Property")),
				    new DisplayNameAttribute("CellStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyle",
					new DescriptionAttribute(SR.GetString("ColumnBase_HeaderStyle_Property")),
				    new DisplayNameAttribute("HeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("ColumnBase_HeaderStyleResolved_Property")),
				    new DisplayNameAttribute("HeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyle",
					new DescriptionAttribute(SR.GetString("ColumnBase_FooterStyle_Property")),
				    new DisplayNameAttribute("FooterStyle")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyleResolved",
					new DescriptionAttribute(SR.GetString("ColumnBase_FooterStyleResolved_Property")),
				    new DisplayNameAttribute("FooterStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsAutoGenerated",
					new DescriptionAttribute(SR.GetString("ColumnBase_IsAutoGenerated_Property")),
				    new DisplayNameAttribute("IsAutoGenerated")				);


				tableBuilder.AddCustomAttributes(t, "Tag",
					new DescriptionAttribute(SR.GetString("ColumnBase_Tag_Property")),
				    new DisplayNameAttribute("Tag")				);


				tableBuilder.AddCustomAttributes(t, "DataType",
					new DescriptionAttribute(SR.GetString("ColumnBase_DataType_Property")),
				    new DisplayNameAttribute("DataType")				);


				tableBuilder.AddCustomAttributes(t, "HeaderText",
					new DescriptionAttribute(SR.GetString("ColumnBase_HeaderText_Property")),
				    new DisplayNameAttribute("HeaderText")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTemplate",
					new DescriptionAttribute(SR.GetString("ColumnBase_HeaderTemplate_Property")),
				    new DisplayNameAttribute("HeaderTemplate")				);


				tableBuilder.AddCustomAttributes(t, "DisplayNameResolved",
					new DescriptionAttribute(SR.GetString("ColumnBase_DisplayNameResolved_Property")),
				    new DisplayNameAttribute("DisplayNameResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsHideable",
					new DescriptionAttribute(SR.GetString("ColumnBase_IsHideable_Property")),
				    new DisplayNameAttribute("IsHideable")				);

				#endregion // ColumnBase Properties

				#region GroupColumnsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GroupColumnsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("GroupColumnsCollection_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);


				tableBuilder.AddCustomAttributes(t, "AllColumns",
					new DescriptionAttribute(SR.GetString("GroupColumnsCollection_AllColumns_Property")),
				    new DisplayNameAttribute("AllColumns")				);

				#endregion // GroupColumnsCollection Properties

				#region ColumnContentProviderBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ColumnContentProviderBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RemovePaddingDuringEditing",
					new DescriptionAttribute(SR.GetString("ColumnContentProviderBase_RemovePaddingDuringEditing_Property")),
				    new DisplayNameAttribute("RemovePaddingDuringEditing")				);


				tableBuilder.AddCustomAttributes(t, "IsToolTip",
					new DescriptionAttribute(SR.GetString("ColumnContentProviderBase_IsToolTip_Property")),
				    new DisplayNameAttribute("IsToolTip")				);

				#endregion // ColumnContentProviderBase Properties

				#region CheckBoxColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CheckBoxColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CheckBoxColumnContentProvider Properties

				#region SummaryRowExpansionIndicatorCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SummaryRowExpansionIndicatorCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SummaryRowExpansionIndicatorCell Properties

				#region ExpansionIndicatorCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ExpansionIndicatorCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ExpansionIndicatorCell Properties

				#region GroupByCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentProvider",
					new DescriptionAttribute(SR.GetString("GroupByCellControl_ContentProvider_Property")),
				    new DisplayNameAttribute("ContentProvider")				);

				#endregion // GroupByCellControl Properties

				#region Cell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Cell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("Cell_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "IsEditing",
					new DescriptionAttribute(SR.GetString("Cell_IsEditing_Property")),
				    new DisplayNameAttribute("IsEditing")				);


				tableBuilder.AddCustomAttributes(t, "EditorStyle",
					new DescriptionAttribute(SR.GetString("Cell_EditorStyle_Property")),
				    new DisplayNameAttribute("EditorStyle")				);


				tableBuilder.AddCustomAttributes(t, "EditorStyleResolved",
					new DescriptionAttribute(SR.GetString("Cell_EditorStyleResolved_Property")),
				    new DisplayNameAttribute("EditorStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("Cell_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "IsEditable",
					new DescriptionAttribute(SR.GetString("Cell_IsEditable_Property")),
				    new DisplayNameAttribute("IsEditable")				);

				#endregion // Cell Properties

				#region GroupByAreaCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByAreaCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "EmptyContent",
					new DescriptionAttribute(SR.GetString("GroupByAreaCellControl_EmptyContent_Property")),
				    new DisplayNameAttribute("EmptyContent")				);


				tableBuilder.AddCustomAttributes(t, "IsExpanded",
					new DescriptionAttribute(SR.GetString("GroupByAreaCellControl_IsExpanded_Property")),
				    new DisplayNameAttribute("IsExpanded")				);

				#endregion // GroupByAreaCellControl Properties

				#region FilterRowExpansionIndicatorCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterRowExpansionIndicatorCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // FilterRowExpansionIndicatorCellControl Properties

				#region ExpansionIndicatorCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ExpansionIndicatorCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicator",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorCellControl_ExpansionIndicator_Property")),
				    new DisplayNameAttribute("ExpansionIndicator")				);

				#endregion // ExpansionIndicatorCellControl Properties

				#region SortingCancellableEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SortingCancellableEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PreviousSortDirection",
					new DescriptionAttribute(SR.GetString("SortingCancellableEventArgs_PreviousSortDirection_Property")),
				    new DisplayNameAttribute("PreviousSortDirection")				);


				tableBuilder.AddCustomAttributes(t, "NewSortDirection",
					new DescriptionAttribute(SR.GetString("SortingCancellableEventArgs_NewSortDirection_Property")),
				    new DisplayNameAttribute("NewSortDirection")				);

				#endregion // SortingCancellableEventArgs Properties

				#region CancellableColumnEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CancellableColumnEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("CancellableColumnEventArgs_Column_Property")),
				    new DisplayNameAttribute("Column")				);

				#endregion // CancellableColumnEventArgs Properties

				#region SortedColumnEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SortedColumnEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PreviousSortDirection",
					new DescriptionAttribute(SR.GetString("SortedColumnEventArgs_PreviousSortDirection_Property")),
				    new DisplayNameAttribute("PreviousSortDirection")				);

				#endregion // SortedColumnEventArgs Properties

				#region ColumnEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("ColumnEventArgs_Column_Property")),
				    new DisplayNameAttribute("Column")				);

				#endregion // ColumnEventArgs Properties

				#region ActiveCellChangingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ActiveCellChangingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PreviousActiveCell",
					new DescriptionAttribute(SR.GetString("ActiveCellChangingEventArgs_PreviousActiveCell_Property")),
				    new DisplayNameAttribute("PreviousActiveCell")				);


				tableBuilder.AddCustomAttributes(t, "NewActiveCell",
					new DescriptionAttribute(SR.GetString("ActiveCellChangingEventArgs_NewActiveCell_Property")),
				    new DisplayNameAttribute("NewActiveCell")				);

				#endregion // ActiveCellChangingEventArgs Properties

				#region SelectionCollectionChangedEventArgs`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SelectionCollectionChangedEventArgs`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PreviouslySelectedItems",
					new DescriptionAttribute(SR.GetString("SelectionCollectionChangedEventArgs`1_PreviouslySelectedItems_Property")),
				    new DisplayNameAttribute("PreviouslySelectedItems")				);


				tableBuilder.AddCustomAttributes(t, "NewSelectedItems",
					new DescriptionAttribute(SR.GetString("SelectionCollectionChangedEventArgs`1_NewSelectedItems_Property")),
				    new DisplayNameAttribute("NewSelectedItems")				);

				#endregion // SelectionCollectionChangedEventArgs`1 Properties

				#region CellControlAttachedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CellControlAttachedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("CellControlAttachedEventArgs_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "IsDirty",
					new DescriptionAttribute(SR.GetString("CellControlAttachedEventArgs_IsDirty_Property")),
				    new DisplayNameAttribute("IsDirty")				);

				#endregion // CellControlAttachedEventArgs Properties

				#region InitializeRowEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.InitializeRowEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // InitializeRowEventArgs Properties

				#region RowEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("RowEventArgs_Row_Property")),
				    new DisplayNameAttribute("Row")				);

				#endregion // RowEventArgs Properties

				#region RowSelectorClickedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowSelectorClickedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("RowSelectorClickedEventArgs_Row_Property")),
				    new DisplayNameAttribute("Row")				);

				#endregion // RowSelectorClickedEventArgs Properties

				#region CellClickedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CellClickedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("CellClickedEventArgs_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);

				#endregion // CellClickedEventArgs Properties

				#region ColumnFixedStateEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnFixedStateEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PreviousFixedState",
					new DescriptionAttribute(SR.GetString("ColumnFixedStateEventArgs_PreviousFixedState_Property")),
				    new DisplayNameAttribute("PreviousFixedState")				);

				#endregion // ColumnFixedStateEventArgs Properties

				#region CancellableColumnFixedStateEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CancellableColumnFixedStateEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FixedState",
					new DescriptionAttribute(SR.GetString("CancellableColumnFixedStateEventArgs_FixedState_Property")),
				    new DisplayNameAttribute("FixedState")				);

				#endregion // CancellableColumnFixedStateEventArgs Properties

				#region ColumnLayoutAssignedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnLayoutAssignedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("ColumnLayoutAssignedEventArgs_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);


				tableBuilder.AddCustomAttributes(t, "Level",
					new DescriptionAttribute(SR.GetString("ColumnLayoutAssignedEventArgs_Level_Property")),
				    new DisplayNameAttribute("Level")				);


				tableBuilder.AddCustomAttributes(t, "Key",
					new DescriptionAttribute(SR.GetString("ColumnLayoutAssignedEventArgs_Key_Property")),
				    new DisplayNameAttribute("Key")				);


				tableBuilder.AddCustomAttributes(t, "DataType",
					new DescriptionAttribute(SR.GetString("ColumnLayoutAssignedEventArgs_DataType_Property")),
				    new DisplayNameAttribute("DataType")				);


				tableBuilder.AddCustomAttributes(t, "Rows",
					new DescriptionAttribute(SR.GetString("ColumnLayoutAssignedEventArgs_Rows_Property")),
				    new DisplayNameAttribute("Rows")				);

				#endregion // ColumnLayoutAssignedEventArgs Properties

				#region CancellableRowExpansionChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CancellableRowExpansionChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("CancellableRowExpansionChangedEventArgs_Row_Property")),
				    new DisplayNameAttribute("Row")				);

				#endregion // CancellableRowExpansionChangedEventArgs Properties

				#region RowExpansionChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowExpansionChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("RowExpansionChangedEventArgs_Row_Property")),
				    new DisplayNameAttribute("Row")				);

				#endregion // RowExpansionChangedEventArgs Properties

				#region ColumnDragStartEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnDragStartEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnDragStartEventArgs Properties

				#region ColumnMovingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnMovingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DraggingHeader",
					new DescriptionAttribute(SR.GetString("ColumnMovingEventArgs_DraggingHeader_Property")),
				    new DisplayNameAttribute("DraggingHeader")				);

				#endregion // ColumnMovingEventArgs Properties

				#region ColumnDroppedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnDroppedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PreviousIndex",
					new DescriptionAttribute(SR.GetString("ColumnDroppedEventArgs_PreviousIndex_Property")),
				    new DisplayNameAttribute("PreviousIndex")				);


				tableBuilder.AddCustomAttributes(t, "NewIndex",
					new DescriptionAttribute(SR.GetString("ColumnDroppedEventArgs_NewIndex_Property")),
				    new DisplayNameAttribute("NewIndex")				);


				tableBuilder.AddCustomAttributes(t, "DropType",
					new DescriptionAttribute(SR.GetString("ColumnDroppedEventArgs_DropType_Property")),
				    new DisplayNameAttribute("DropType")				);

				#endregion // ColumnDroppedEventArgs Properties

				#region ColumnDragCanceledEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnDragCanceledEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CancelType",
					new DescriptionAttribute(SR.GetString("ColumnDragCanceledEventArgs_CancelType_Property")),
				    new DisplayNameAttribute("CancelType")				);

				#endregion // ColumnDragCanceledEventArgs Properties

				#region ColumnDragEndedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnDragEndedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnDragEndedEventArgs Properties

				#region BeginEditingCellEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.BeginEditingCellEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("BeginEditingCellEventArgs_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);

				#endregion // BeginEditingCellEventArgs Properties

				#region EditingCellEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EditingCellEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("EditingCellEventArgs_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "Editor",
					new DescriptionAttribute(SR.GetString("EditingCellEventArgs_Editor_Property")),
				    new DisplayNameAttribute("Editor")				);

				#endregion // EditingCellEventArgs Properties

				#region CellExitedEditingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CellExitedEditingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("CellExitedEditingEventArgs_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);

				#endregion // CellExitedEditingEventArgs Properties

				#region ExitEditingCellEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ExitEditingCellEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("ExitEditingCellEventArgs_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "NewValue",
					new DescriptionAttribute(SR.GetString("ExitEditingCellEventArgs_NewValue_Property")),
				    new DisplayNameAttribute("NewValue")				);


				tableBuilder.AddCustomAttributes(t, "EditingCanceled",
					new DescriptionAttribute(SR.GetString("ExitEditingCellEventArgs_EditingCanceled_Property")),
				    new DisplayNameAttribute("EditingCanceled")				);


				tableBuilder.AddCustomAttributes(t, "Editor",
					new DescriptionAttribute(SR.GetString("ExitEditingCellEventArgs_Editor_Property")),
				    new DisplayNameAttribute("Editor")				);

				#endregion // ExitEditingCellEventArgs Properties

				#region BeginEditingRowEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.BeginEditingRowEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // BeginEditingRowEventArgs Properties

				#region CancellableRowEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CancellableRowEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("CancellableRowEventArgs_Row_Property")),
				    new DisplayNameAttribute("Row")				);

				#endregion // CancellableRowEventArgs Properties

				#region EditingRowEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EditingRowEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // EditingRowEventArgs Properties

				#region ExitEditingRowEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ExitEditingRowEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("ExitEditingRowEventArgs_Row_Property")),
				    new DisplayNameAttribute("Row")				);


				tableBuilder.AddCustomAttributes(t, "EditingCanceled",
					new DescriptionAttribute(SR.GetString("ExitEditingRowEventArgs_EditingCanceled_Property")),
				    new DisplayNameAttribute("EditingCanceled")				);


				tableBuilder.AddCustomAttributes(t, "NewCellValues",
					new DescriptionAttribute(SR.GetString("ExitEditingRowEventArgs_NewCellValues_Property")),
				    new DisplayNameAttribute("NewCellValues")				);


				tableBuilder.AddCustomAttributes(t, "OriginalCellValues",
					new DescriptionAttribute(SR.GetString("ExitEditingRowEventArgs_OriginalCellValues_Property")),
				    new DisplayNameAttribute("OriginalCellValues")				);

				#endregion // ExitEditingRowEventArgs Properties

				#region CancellablePageChangingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CancellablePageChangingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NextPageIndex",
					new DescriptionAttribute(SR.GetString("CancellablePageChangingEventArgs_NextPageIndex_Property")),
				    new DisplayNameAttribute("NextPageIndex")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("CancellablePageChangingEventArgs_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);


				tableBuilder.AddCustomAttributes(t, "Level",
					new DescriptionAttribute(SR.GetString("CancellablePageChangingEventArgs_Level_Property")),
				    new DisplayNameAttribute("Level")				);


				tableBuilder.AddCustomAttributes(t, "Rows",
					new DescriptionAttribute(SR.GetString("CancellablePageChangingEventArgs_Rows_Property")),
				    new DisplayNameAttribute("Rows")				);

				#endregion // CancellablePageChangingEventArgs Properties

				#region PageChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.PageChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OldPageIndex",
					new DescriptionAttribute(SR.GetString("PageChangedEventArgs_OldPageIndex_Property")),
				    new DisplayNameAttribute("OldPageIndex")				);

				#endregion // PageChangedEventArgs Properties

				#region ColumnLayoutEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnLayoutEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("ColumnLayoutEventArgs_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);

				#endregion // ColumnLayoutEventArgs Properties

				#region CancellableColumnResizingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CancellableColumnResizingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Columns",
					new DescriptionAttribute(SR.GetString("CancellableColumnResizingEventArgs_Columns_Property")),
				    new DisplayNameAttribute("Columns")				);


				tableBuilder.AddCustomAttributes(t, "Width",
					new DescriptionAttribute(SR.GetString("CancellableColumnResizingEventArgs_Width_Property")),
				    new DisplayNameAttribute("Width")				);

				#endregion // CancellableColumnResizingEventArgs Properties

				#region ColumnResizedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnResizedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Columns",
					new DescriptionAttribute(SR.GetString("ColumnResizedEventArgs_Columns_Property")),
				    new DisplayNameAttribute("Columns")				);

				#endregion // ColumnResizedEventArgs Properties

				#region CancellableRowAddingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CancellableRowAddingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InsertionIndex",
					new DescriptionAttribute(SR.GetString("CancellableRowAddingEventArgs_InsertionIndex_Property")),
				    new DisplayNameAttribute("InsertionIndex")				);

				#endregion // CancellableRowAddingEventArgs Properties

				#region GroupByCollectionChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GroupByCollectionChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PreviousGroupedColumns",
					new DescriptionAttribute(SR.GetString("GroupByCollectionChangedEventArgs_PreviousGroupedColumns_Property")),
				    new DisplayNameAttribute("PreviousGroupedColumns")				);


				tableBuilder.AddCustomAttributes(t, "NewGroupedColumns",
					new DescriptionAttribute(SR.GetString("GroupByCollectionChangedEventArgs_NewGroupedColumns_Property")),
				    new DisplayNameAttribute("NewGroupedColumns")				);

				#endregion // GroupByCollectionChangedEventArgs Properties

				#region CellValidationErrorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CellValidationErrorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ValidationErrorEventArgs",
					new DescriptionAttribute(SR.GetString("CellValidationErrorEventArgs_ValidationErrorEventArgs_Property")),
				    new DisplayNameAttribute("ValidationErrorEventArgs")				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("CellValidationErrorEventArgs_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "Handled",
					new DescriptionAttribute(SR.GetString("CellValidationErrorEventArgs_Handled_Property")),
				    new DisplayNameAttribute("Handled")				);

				#endregion // CellValidationErrorEventArgs Properties

				#region DataObjectCreationEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DataObjectCreationEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NewObject",
					new DescriptionAttribute(SR.GetString("DataObjectCreationEventArgs_NewObject_Property")),
				    new DisplayNameAttribute("NewObject")				);


				tableBuilder.AddCustomAttributes(t, "ObjectType",
					new DescriptionAttribute(SR.GetString("DataObjectCreationEventArgs_ObjectType_Property")),
				    new DisplayNameAttribute("ObjectType")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("DataObjectCreationEventArgs_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);


				tableBuilder.AddCustomAttributes(t, "ParentRow",
					new DescriptionAttribute(SR.GetString("DataObjectCreationEventArgs_ParentRow_Property")),
				    new DisplayNameAttribute("ParentRow")				);


				tableBuilder.AddCustomAttributes(t, "CollectionType",
					new DescriptionAttribute(SR.GetString("DataObjectCreationEventArgs_CollectionType_Property")),
				    new DisplayNameAttribute("CollectionType")				);


				tableBuilder.AddCustomAttributes(t, "RowTypeCreated",
					new DescriptionAttribute(SR.GetString("DataObjectCreationEventArgs_RowTypeCreated_Property")),
				    new DisplayNameAttribute("RowTypeCreated")				);

				#endregion // DataObjectCreationEventArgs Properties

				#region PopulatingFiltersEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.PopulatingFiltersEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FilterOperands",
					new DescriptionAttribute(SR.GetString("PopulatingFiltersEventArgs_FilterOperands_Property")),
				    new DisplayNameAttribute("FilterOperands")				);

				#endregion // PopulatingFiltersEventArgs Properties

				#region DataLimitingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DataLimitingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DataSource",
					new DescriptionAttribute(SR.GetString("DataLimitingEventArgs_DataSource_Property")),
				    new DisplayNameAttribute("DataSource")				);


				tableBuilder.AddCustomAttributes(t, "EnablePaging",
					new DescriptionAttribute(SR.GetString("DataLimitingEventArgs_EnablePaging_Property")),
				    new DisplayNameAttribute("EnablePaging")				);


				tableBuilder.AddCustomAttributes(t, "PageSize",
					new DescriptionAttribute(SR.GetString("DataLimitingEventArgs_PageSize_Property")),
				    new DisplayNameAttribute("PageSize")				);


				tableBuilder.AddCustomAttributes(t, "CurrentPage",
					new DescriptionAttribute(SR.GetString("DataLimitingEventArgs_CurrentPage_Property")),
				    new DisplayNameAttribute("CurrentPage")				);


				tableBuilder.AddCustomAttributes(t, "CurrentSort",
					new DescriptionAttribute(SR.GetString("DataLimitingEventArgs_CurrentSort_Property")),
				    new DisplayNameAttribute("CurrentSort")				);


				tableBuilder.AddCustomAttributes(t, "GroupByContext",
					new DescriptionAttribute(SR.GetString("DataLimitingEventArgs_GroupByContext_Property")),
				    new DisplayNameAttribute("GroupByContext")				);


				tableBuilder.AddCustomAttributes(t, "Filters",
					new DescriptionAttribute(SR.GetString("DataLimitingEventArgs_Filters_Property")),
				    new DisplayNameAttribute("Filters")				);


				tableBuilder.AddCustomAttributes(t, "ParentRow",
					new DescriptionAttribute(SR.GetString("DataLimitingEventArgs_ParentRow_Property")),
				    new DisplayNameAttribute("ParentRow")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("DataLimitingEventArgs_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);

				#endregion // DataLimitingEventArgs Properties

				#region CancellableFilteringEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CancellableFilteringEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowFiltersCollection",
					new DescriptionAttribute(SR.GetString("CancellableFilteringEventArgs_RowFiltersCollection_Property")),
				    new DisplayNameAttribute("RowFiltersCollection")				);


				tableBuilder.AddCustomAttributes(t, "FilterValue",
					new DescriptionAttribute(SR.GetString("CancellableFilteringEventArgs_FilterValue_Property")),
				    new DisplayNameAttribute("FilterValue")				);


				tableBuilder.AddCustomAttributes(t, "FilteringOperand",
					new DescriptionAttribute(SR.GetString("CancellableFilteringEventArgs_FilteringOperand_Property")),
				    new DisplayNameAttribute("FilteringOperand")				);

				#endregion // CancellableFilteringEventArgs Properties

				#region FilteredEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FilteredEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowFiltersCollection",
					new DescriptionAttribute(SR.GetString("FilteredEventArgs_RowFiltersCollection_Property")),
				    new DisplayNameAttribute("RowFiltersCollection")				);

				#endregion // FilteredEventArgs Properties

				#region ClipboardCopyingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ClipboardCopyingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedItems",
					new DescriptionAttribute(SR.GetString("ClipboardCopyingEventArgs_SelectedItems_Property")),
				    new DisplayNameAttribute("SelectedItems")				);


				tableBuilder.AddCustomAttributes(t, "ClipboardValue",
					new DescriptionAttribute(SR.GetString("ClipboardCopyingEventArgs_ClipboardValue_Property")),
				    new DisplayNameAttribute("ClipboardValue")				);

				#endregion // ClipboardCopyingEventArgs Properties

				#region ClipboardCopyingItemEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ClipboardCopyingItemEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("ClipboardCopyingItemEventArgs_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "ClipboardValue",
					new DescriptionAttribute(SR.GetString("ClipboardCopyingItemEventArgs_ClipboardValue_Property")),
				    new DisplayNameAttribute("ClipboardValue")				);

				#endregion // ClipboardCopyingItemEventArgs Properties

				#region ClipboardPastingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ClipboardPastingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Values",
					new DescriptionAttribute(SR.GetString("ClipboardPastingEventArgs_Values_Property")),
				    new DisplayNameAttribute("Values")				);


				tableBuilder.AddCustomAttributes(t, "ClipboardValue",
					new DescriptionAttribute(SR.GetString("ClipboardPastingEventArgs_ClipboardValue_Property")),
				    new DisplayNameAttribute("ClipboardValue")				);

				#endregion // ClipboardPastingEventArgs Properties

				#region MaximumSummaryOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.MaximumSummaryOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SummaryCalculator",
					new DescriptionAttribute(SR.GetString("MaximumSummaryOperand_SummaryCalculator_Property")),
				    new DisplayNameAttribute("SummaryCalculator")				);


				tableBuilder.AddCustomAttributes(t, "LinqSummaryOperator",
					new DescriptionAttribute(SR.GetString("MaximumSummaryOperand_LinqSummaryOperator_Property")),
				    new DisplayNameAttribute("LinqSummaryOperator")				);

				#endregion // MaximumSummaryOperand Properties

				#region MinimumSummaryOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.MinimumSummaryOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SummaryCalculator",
					new DescriptionAttribute(SR.GetString("MinimumSummaryOperand_SummaryCalculator_Property")),
				    new DisplayNameAttribute("SummaryCalculator")				);


				tableBuilder.AddCustomAttributes(t, "LinqSummaryOperator",
					new DescriptionAttribute(SR.GetString("MinimumSummaryOperand_LinqSummaryOperator_Property")),
				    new DisplayNameAttribute("LinqSummaryOperator")				);

				#endregion // MinimumSummaryOperand Properties

				#region CountSummaryOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CountSummaryOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SummaryCalculator",
					new DescriptionAttribute(SR.GetString("CountSummaryOperand_SummaryCalculator_Property")),
				    new DisplayNameAttribute("SummaryCalculator")				);


				tableBuilder.AddCustomAttributes(t, "LinqSummaryOperator",
					new DescriptionAttribute(SR.GetString("CountSummaryOperand_LinqSummaryOperator_Property")),
				    new DisplayNameAttribute("LinqSummaryOperator")				);

				#endregion // CountSummaryOperand Properties

				#region SumSummaryOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SumSummaryOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SummaryCalculator",
					new DescriptionAttribute(SR.GetString("SumSummaryOperand_SummaryCalculator_Property")),
				    new DisplayNameAttribute("SummaryCalculator")				);


				tableBuilder.AddCustomAttributes(t, "LinqSummaryOperator",
					new DescriptionAttribute(SR.GetString("SumSummaryOperand_LinqSummaryOperator_Property")),
				    new DisplayNameAttribute("LinqSummaryOperator")				);

				#endregion // SumSummaryOperand Properties

				#region AverageSummaryOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.AverageSummaryOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SummaryCalculator",
					new DescriptionAttribute(SR.GetString("AverageSummaryOperand_SummaryCalculator_Property")),
				    new DisplayNameAttribute("SummaryCalculator")				);


				tableBuilder.AddCustomAttributes(t, "LinqSummaryOperator",
					new DescriptionAttribute(SR.GetString("AverageSummaryOperand_LinqSummaryOperator_Property")),
				    new DisplayNameAttribute("LinqSummaryOperator")				);

				#endregion // AverageSummaryOperand Properties

				#region RowSelectorSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowSelectorSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "EnableRowNumbering",
					new DescriptionAttribute(SR.GetString("RowSelectorSettings_EnableRowNumbering_Property")),
				    new DisplayNameAttribute("EnableRowNumbering")				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyle",
					new DescriptionAttribute(SR.GetString("RowSelectorSettings_HeaderStyle_Property")),
				    new DisplayNameAttribute("HeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyle",
					new DescriptionAttribute(SR.GetString("RowSelectorSettings_FooterStyle_Property")),
				    new DisplayNameAttribute("FooterStyle")				);


				tableBuilder.AddCustomAttributes(t, "RowNumberingSeed",
					new DescriptionAttribute(SR.GetString("RowSelectorSettings_RowNumberingSeed_Property")),
				    new DisplayNameAttribute("RowNumberingSeed")				);

				#endregion // RowSelectorSettings Properties

				#region FilteringSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FilteringSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowFiltering",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_AllowFiltering_Property")),
				    new DisplayNameAttribute("AllowFiltering")				);


				tableBuilder.AddCustomAttributes(t, "AllowFilteringResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_AllowFilteringResolved_Property")),
				    new DisplayNameAttribute("AllowFilteringResolved")				);


				tableBuilder.AddCustomAttributes(t, "AllowFilterRow",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_AllowFilterRow_Property")),
				    new DisplayNameAttribute("AllowFilterRow")				);


				tableBuilder.AddCustomAttributes(t, "AllowFilterRowResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_AllowFilterRowResolved_Property")),
				    new DisplayNameAttribute("AllowFilterRowResolved")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorStyle",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_ExpansionIndicatorStyle_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorStyleResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_ExpansionIndicatorStyleResolved_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "StyleResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_StyleResolved_Property")),
				    new DisplayNameAttribute("StyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FilterSelectionControlStyle",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_FilterSelectionControlStyle_Property")),
				    new DisplayNameAttribute("FilterSelectionControlStyle")				);


				tableBuilder.AddCustomAttributes(t, "FilterSelectionControlStyleResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_FilterSelectionControlStyleResolved_Property")),
				    new DisplayNameAttribute("FilterSelectionControlStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "CustomFilterDialogStyle",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_CustomFilterDialogStyle_Property")),
				    new DisplayNameAttribute("CustomFilterDialogStyle")				);


				tableBuilder.AddCustomAttributes(t, "CustomFilterDialogStyleResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_CustomFilterDialogStyleResolved_Property")),
				    new DisplayNameAttribute("CustomFilterDialogStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorStyle",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_RowSelectorStyle_Property")),
				    new DisplayNameAttribute("RowSelectorStyle")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorStyleResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_RowSelectorStyleResolved_Property")),
				    new DisplayNameAttribute("RowSelectorStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FilteringScope",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_FilteringScope_Property")),
				    new DisplayNameAttribute("FilteringScope")				);


				tableBuilder.AddCustomAttributes(t, "FilteringScopeResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_FilteringScopeResolved_Property")),
				    new DisplayNameAttribute("FilteringScopeResolved")				);


				tableBuilder.AddCustomAttributes(t, "RowFiltersCollection",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_RowFiltersCollection_Property")),
				    new DisplayNameAttribute("RowFiltersCollection")				);


				tableBuilder.AddCustomAttributes(t, "FilterRowHeight",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_FilterRowHeight_Property")),
				    new DisplayNameAttribute("FilterRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "FilterRowHeightResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_FilterRowHeightResolved_Property")),
				    new DisplayNameAttribute("FilterRowHeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuClearFiltersString",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_FilterMenuClearFiltersString_Property")),
				    new DisplayNameAttribute("FilterMenuClearFiltersString")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuClearFiltersStringResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_FilterMenuClearFiltersStringResolved_Property")),
				    new DisplayNameAttribute("FilterMenuClearFiltersStringResolved")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuCustomFilteringButtonVisibility",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_FilterMenuCustomFilteringButtonVisibility_Property")),
				    new DisplayNameAttribute("FilterMenuCustomFilteringButtonVisibility")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuCustomFilteringButtonVisibilityResolved",
					new DescriptionAttribute(SR.GetString("FilteringSettingsOverride_FilterMenuCustomFilteringButtonVisibilityResolved_Property")),
				    new DisplayNameAttribute("FilterMenuCustomFilteringButtonVisibilityResolved")				);

				#endregion // FilteringSettingsOverride Properties

				#region EditingSettingsBaseOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EditingSettingsBaseOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsF2EditingEnabled",
					new DescriptionAttribute(SR.GetString("EditingSettingsBaseOverride_IsF2EditingEnabled_Property")),
				    new DisplayNameAttribute("IsF2EditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsF2EditingEnabledResolved",
					new DescriptionAttribute(SR.GetString("EditingSettingsBaseOverride_IsF2EditingEnabledResolved_Property")),
				    new DisplayNameAttribute("IsF2EditingEnabledResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsEnterKeyEditingEnabled",
					new DescriptionAttribute(SR.GetString("EditingSettingsBaseOverride_IsEnterKeyEditingEnabled_Property")),
				    new DisplayNameAttribute("IsEnterKeyEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsEnterKeyEditingEnabledResolved",
					new DescriptionAttribute(SR.GetString("EditingSettingsBaseOverride_IsEnterKeyEditingEnabledResolved_Property")),
				    new DisplayNameAttribute("IsEnterKeyEditingEnabledResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsMouseActionEditingEnabled",
					new DescriptionAttribute(SR.GetString("EditingSettingsBaseOverride_IsMouseActionEditingEnabled_Property")),
				    new DisplayNameAttribute("IsMouseActionEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsMouseActionEditingEnabledResolved",
					new DescriptionAttribute(SR.GetString("EditingSettingsBaseOverride_IsMouseActionEditingEnabledResolved_Property")),
				    new DisplayNameAttribute("IsMouseActionEditingEnabledResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsOnCellActiveEditingEnabled",
					new DescriptionAttribute(SR.GetString("EditingSettingsBaseOverride_IsOnCellActiveEditingEnabled_Property")),
				    new DisplayNameAttribute("IsOnCellActiveEditingEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsOnCellActiveEditingEnabledResolved",
					new DescriptionAttribute(SR.GetString("EditingSettingsBaseOverride_IsOnCellActiveEditingEnabledResolved_Property")),
				    new DisplayNameAttribute("IsOnCellActiveEditingEnabledResolved")				);

				#endregion // EditingSettingsBaseOverride Properties

				#region NotEqualToConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.NotEqualToConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NotEqualToConditionalFormatRule Properties

				#region EqualToConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EqualToConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("EqualToConditionalFormatRule_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // EqualToConditionalFormatRule Properties

				#region DiscreetRuleBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DiscreetRuleBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "StyleToApply",
					new DescriptionAttribute(SR.GetString("DiscreetRuleBase_StyleToApply_Property")),
				    new DisplayNameAttribute("StyleToApply")				);

				#endregion // DiscreetRuleBase Properties

				#region ConditionalFormattingRuleBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ConditionalFormattingRuleBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingRuleBase_Column_Property")),
				    new DisplayNameAttribute("Column")				);


				tableBuilder.AddCustomAttributes(t, "IsTerminalRule",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingRuleBase_IsTerminalRule_Property")),
				    new DisplayNameAttribute("IsTerminalRule")				);


				tableBuilder.AddCustomAttributes(t, "StyleScope",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingRuleBase_StyleScope_Property")),
				    new DisplayNameAttribute("StyleScope")				);


				tableBuilder.AddCustomAttributes(t, "ShouldRefreshOnDataChange",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingRuleBase_ShouldRefreshOnDataChange_Property")),
				    new DisplayNameAttribute("ShouldRefreshOnDataChange")				);


				tableBuilder.AddCustomAttributes(t, "CellValueVisibility",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingRuleBase_CellValueVisibility_Property")),
				    new DisplayNameAttribute("CellValueVisibility")				);


				tableBuilder.AddCustomAttributes(t, "RuleExecution",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingRuleBase_RuleExecution_Property")),
				    new DisplayNameAttribute("RuleExecution")				);

				#endregion // ConditionalFormattingRuleBase Properties

				#region NotEqualToConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.NotEqualToConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NotEqualToConditionalFormatRuleProxy Properties

				#region EqualToConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EqualToConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("EqualToConditionalFormatRuleProxy_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // EqualToConditionalFormatRuleProxy Properties

				#region DiscreetRuleBaseProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DiscreetRuleBaseProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DiscreetRuleBaseProxy Properties

				#region ConditionalFormattingRuleBaseProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ConditionalFormattingRuleBaseProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RuleExecution",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingRuleBaseProxy_RuleExecution_Property")),
				    new DisplayNameAttribute("RuleExecution")				);

				#endregion // ConditionalFormattingRuleBaseProxy Properties

				#region GreaterThanOrEqualToConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanOrEqualToConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GreaterThanOrEqualToConditionalFormatRule Properties

				#region GreaterThanOrEqualToConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanOrEqualToConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GreaterThanOrEqualToConditionalFormatRuleProxy Properties

				#region ContainingConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ContainingConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ContainingConditionalFormatRule Properties

				#region StringConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.StringConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("StringConditionalFormatRule_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // StringConditionalFormatRule Properties

				#region ContainingConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ContainingConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ContainingConditionalFormatRuleProxy Properties

				#region StringConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.StringConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StringConditionalFormatRuleProxy Properties

				#region ColumnChooserDialog Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ColumnChooserDialog");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsOpen",
					new DescriptionAttribute(SR.GetString("ColumnChooserDialog_IsOpen_Property")),
				    new DisplayNameAttribute("IsOpen")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("ColumnChooserDialog_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("ColumnChooserDialog_Column_Property")),
				    new DisplayNameAttribute("Column")				);


				tableBuilder.AddCustomAttributes(t, "Columns",
					new DescriptionAttribute(SR.GetString("ColumnChooserDialog_Columns_Property")),
				    new DisplayNameAttribute("Columns")				);


				tableBuilder.AddCustomAttributes(t, "ViewLabelText",
					new DescriptionAttribute(SR.GetString("ColumnChooserDialog_ViewLabelText_Property")),
				    new DisplayNameAttribute("ViewLabelText")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLabelText",
					new DescriptionAttribute(SR.GetString("ColumnChooserDialog_ColumnLabelText_Property")),
				    new DisplayNameAttribute("ColumnLabelText")				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("ColumnChooserDialog_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);

				#endregion // ColumnChooserDialog Properties

				#region RowHeightTypeConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowHeightTypeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowHeightTypeConverter Properties

				#region GroupByRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "GroupByData",
					new DescriptionAttribute(SR.GetString("GroupByRow_GroupByData_Property")),
				    new DisplayNameAttribute("GroupByData")				);


				tableBuilder.AddCustomAttributes(t, "CurrentPageIndex",
					new DescriptionAttribute(SR.GetString("GroupByRow_CurrentPageIndex_Property")),
				    new DisplayNameAttribute("CurrentPageIndex")				);


				tableBuilder.AddCustomAttributes(t, "RowFiltersCollection",
					new DescriptionAttribute(SR.GetString("GroupByRow_RowFiltersCollection_Property")),
				    new DisplayNameAttribute("RowFiltersCollection")				);


				tableBuilder.AddCustomAttributes(t, "Rows",
					new DescriptionAttribute(SR.GetString("GroupByRow_Rows_Property")),
				    new DisplayNameAttribute("Rows")				);


				tableBuilder.AddCustomAttributes(t, "HasChildren",
					new DescriptionAttribute(SR.GetString("GroupByRow_HasChildren_Property")),
				    new DisplayNameAttribute("HasChildren")				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("GroupByRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "Cells",
					new DescriptionAttribute(SR.GetString("GroupByRow_Cells_Property")),
				    new DisplayNameAttribute("Cells")				);


				tableBuilder.AddCustomAttributes(t, "ChildBands",
					new DescriptionAttribute(SR.GetString("GroupByRow_ChildBands_Property")),
				    new DisplayNameAttribute("ChildBands")				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("GroupByRow_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);

				#endregion // GroupByRow Properties

				#region UnboundColumnDataContext Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.UnboundColumnDataContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowData",
					new DescriptionAttribute(SR.GetString("UnboundColumnDataContext_RowData_Property")),
				    new DisplayNameAttribute("RowData")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("UnboundColumnDataContext_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "ColumnKey",
					new DescriptionAttribute(SR.GetString("UnboundColumnDataContext_ColumnKey_Property")),
				    new DisplayNameAttribute("ColumnKey")				);

				#endregion // UnboundColumnDataContext Properties

				#region UnboundColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.UnboundColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RemovePaddingDuringEditing",
					new DescriptionAttribute(SR.GetString("UnboundColumnContentProvider_RemovePaddingDuringEditing_Property")),
				    new DisplayNameAttribute("RemovePaddingDuringEditing")				);

				#endregion // UnboundColumnContentProvider Properties

				#region DateColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateColumnContentProvider Properties

				#region ColumnLayout Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnLayout");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TargetTypeName",
					new DescriptionAttribute(SR.GetString("ColumnLayout_TargetTypeName_Property")),
				    new DisplayNameAttribute("TargetTypeName")				);


				tableBuilder.AddCustomAttributes(t, "Columns",
					new DescriptionAttribute(SR.GetString("ColumnLayout_Columns_Property")),
				    new DisplayNameAttribute("Columns")				);


				tableBuilder.AddCustomAttributes(t, "Grid",
					new DescriptionAttribute(SR.GetString("ColumnLayout_Grid_Property")),
				    new DisplayNameAttribute("Grid")				);


				tableBuilder.AddCustomAttributes(t, "HeaderVisibility",
					new DescriptionAttribute(SR.GetString("ColumnLayout_HeaderVisibility_Property")),
				    new DisplayNameAttribute("HeaderVisibility")				);


				tableBuilder.AddCustomAttributes(t, "HeaderVisibilityResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_HeaderVisibilityResolved_Property")),
				    new DisplayNameAttribute("HeaderVisibilityResolved")				);


				tableBuilder.AddCustomAttributes(t, "FooterVisibility",
					new DescriptionAttribute(SR.GetString("ColumnLayout_FooterVisibility_Property")),
				    new DisplayNameAttribute("FooterVisibility")				);


				tableBuilder.AddCustomAttributes(t, "FooterVisibilityResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_FooterVisibilityResolved_Property")),
				    new DisplayNameAttribute("FooterVisibilityResolved")				);


				tableBuilder.AddCustomAttributes(t, "AutoGenerateColumns",
					new DescriptionAttribute(SR.GetString("ColumnLayout_AutoGenerateColumns_Property")),
				    new DisplayNameAttribute("AutoGenerateColumns")				);


				tableBuilder.AddCustomAttributes(t, "AutoGenerateColumnsResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_AutoGenerateColumnsResolved_Property")),
				    new DisplayNameAttribute("AutoGenerateColumnsResolved")				);


				tableBuilder.AddCustomAttributes(t, "Indentation",
					new DescriptionAttribute(SR.GetString("ColumnLayout_Indentation_Property")),
				    new DisplayNameAttribute("Indentation")				);


				tableBuilder.AddCustomAttributes(t, "IndentationResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_IndentationResolved_Property")),
				    new DisplayNameAttribute("IndentationResolved")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayoutHeaderVisibility",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnLayoutHeaderVisibility_Property")),
				    new DisplayNameAttribute("ColumnLayoutHeaderVisibility")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayoutHeaderVisibilityResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnLayoutHeaderVisibilityResolved_Property")),
				    new DisplayNameAttribute("ColumnLayoutHeaderVisibilityResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsAlternateRowsEnabled",
					new DescriptionAttribute(SR.GetString("ColumnLayout_IsAlternateRowsEnabled_Property")),
				    new DisplayNameAttribute("IsAlternateRowsEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsAlternateRowsEnabledResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_IsAlternateRowsEnabledResolved_Property")),
				    new DisplayNameAttribute("IsAlternateRowsEnabledResolved")				);


				tableBuilder.AddCustomAttributes(t, "ChildBandHeaderStyle",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ChildBandHeaderStyle_Property")),
				    new DisplayNameAttribute("ChildBandHeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "ChildBandHeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ChildBandHeaderStyleResolved_Property")),
				    new DisplayNameAttribute("ChildBandHeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "ColumnsHeaderTemplate",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnsHeaderTemplate_Property")),
				    new DisplayNameAttribute("ColumnsHeaderTemplate")				);


				tableBuilder.AddCustomAttributes(t, "ColumnsHeaderTemplateResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnsHeaderTemplateResolved_Property")),
				    new DisplayNameAttribute("ColumnsHeaderTemplateResolved")				);


				tableBuilder.AddCustomAttributes(t, "ColumnsFooterTemplate",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnsFooterTemplate_Property")),
				    new DisplayNameAttribute("ColumnsFooterTemplate")				);


				tableBuilder.AddCustomAttributes(t, "ColumnsFooterTemplateResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnsFooterTemplateResolved_Property")),
				    new DisplayNameAttribute("ColumnsFooterTemplateResolved")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_RowSelectorSettings_Property")),
				    new DisplayNameAttribute("RowSelectorSettings")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ExpansionIndicatorSettings_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorSettings")				);


				tableBuilder.AddCustomAttributes(t, "ColumnMovingSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnMovingSettings_Property")),
				    new DisplayNameAttribute("ColumnMovingSettings")				);


				tableBuilder.AddCustomAttributes(t, "EditingSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_EditingSettings_Property")),
				    new DisplayNameAttribute("EditingSettings")				);


				tableBuilder.AddCustomAttributes(t, "SortingSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_SortingSettings_Property")),
				    new DisplayNameAttribute("SortingSettings")				);


				tableBuilder.AddCustomAttributes(t, "GroupBySettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_GroupBySettings_Property")),
				    new DisplayNameAttribute("GroupBySettings")				);


				tableBuilder.AddCustomAttributes(t, "DeferredScrollingSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_DeferredScrollingSettings_Property")),
				    new DisplayNameAttribute("DeferredScrollingSettings")				);


				tableBuilder.AddCustomAttributes(t, "PagerSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_PagerSettings_Property")),
				    new DisplayNameAttribute("PagerSettings")				);


				tableBuilder.AddCustomAttributes(t, "CellStyleResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_CellStyleResolved_Property")),
				    new DisplayNameAttribute("CellStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_HeaderStyleResolved_Property")),
				    new DisplayNameAttribute("HeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyleResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_FooterStyleResolved_Property")),
				    new DisplayNameAttribute("FooterStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FixedColumnSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_FixedColumnSettings_Property")),
				    new DisplayNameAttribute("FixedColumnSettings")				);


				tableBuilder.AddCustomAttributes(t, "ColumnResizingSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnResizingSettings_Property")),
				    new DisplayNameAttribute("ColumnResizingSettings")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_AddNewRowSettings_Property")),
				    new DisplayNameAttribute("AddNewRowSettings")				);


				tableBuilder.AddCustomAttributes(t, "SummaryRowSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_SummaryRowSettings_Property")),
				    new DisplayNameAttribute("SummaryRowSettings")				);


				tableBuilder.AddCustomAttributes(t, "FilteringSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_FilteringSettings_Property")),
				    new DisplayNameAttribute("FilteringSettings")				);


				tableBuilder.AddCustomAttributes(t, "RowHeight",
					new DescriptionAttribute(SR.GetString("ColumnLayout_RowHeight_Property")),
				    new DisplayNameAttribute("RowHeight")				);


				tableBuilder.AddCustomAttributes(t, "RowHeightResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_RowHeightResolved_Property")),
				    new DisplayNameAttribute("RowHeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "MinimumRowHeight",
					new DescriptionAttribute(SR.GetString("ColumnLayout_MinimumRowHeight_Property")),
				    new DisplayNameAttribute("MinimumRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "MinimumRowHeightResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_MinimumRowHeightResolved_Property")),
				    new DisplayNameAttribute("MinimumRowHeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "DeleteKeyAction",
					new DescriptionAttribute(SR.GetString("ColumnLayout_DeleteKeyAction_Property")),
				    new DisplayNameAttribute("DeleteKeyAction")				);


				tableBuilder.AddCustomAttributes(t, "DeleteKeyActionResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_DeleteKeyActionResolved_Property")),
				    new DisplayNameAttribute("DeleteKeyActionResolved")				);


				tableBuilder.AddCustomAttributes(t, "HeaderRowHeight",
					new DescriptionAttribute(SR.GetString("ColumnLayout_HeaderRowHeight_Property")),
				    new DisplayNameAttribute("HeaderRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "HeaderRowHeightResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_HeaderRowHeightResolved_Property")),
				    new DisplayNameAttribute("HeaderRowHeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "FooterRowHeight",
					new DescriptionAttribute(SR.GetString("ColumnLayout_FooterRowHeight_Property")),
				    new DisplayNameAttribute("FooterRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "FooterRowHeightResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_FooterRowHeightResolved_Property")),
				    new DisplayNameAttribute("FooterRowHeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "ChildBandHeaderHeight",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ChildBandHeaderHeight_Property")),
				    new DisplayNameAttribute("ChildBandHeaderHeight")				);


				tableBuilder.AddCustomAttributes(t, "ChildBandHeaderHeightResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ChildBandHeaderHeightResolved_Property")),
				    new DisplayNameAttribute("ChildBandHeaderHeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "ColumnWidth",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnWidth_Property")),
				    new DisplayNameAttribute("ColumnWidth")				);


				tableBuilder.AddCustomAttributes(t, "ColumnWidthResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnWidthResolved_Property")),
				    new DisplayNameAttribute("ColumnWidthResolved")				);


				tableBuilder.AddCustomAttributes(t, "FillerColumnSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_FillerColumnSettings_Property")),
				    new DisplayNameAttribute("FillerColumnSettings")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextHorizontalAlignment",
					new DescriptionAttribute(SR.GetString("ColumnLayout_HeaderTextHorizontalAlignment_Property")),
				    new DisplayNameAttribute("HeaderTextHorizontalAlignment")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextHorizontalAlignmentResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_HeaderTextHorizontalAlignmentResolved_Property")),
				    new DisplayNameAttribute("HeaderTextHorizontalAlignmentResolved")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextVerticalAlignment",
					new DescriptionAttribute(SR.GetString("ColumnLayout_HeaderTextVerticalAlignment_Property")),
				    new DisplayNameAttribute("HeaderTextVerticalAlignment")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextVerticalAlignmentResolved",
					new DescriptionAttribute(SR.GetString("ColumnLayout_HeaderTextVerticalAlignmentResolved_Property")),
				    new DisplayNameAttribute("HeaderTextVerticalAlignmentResolved")				);


				tableBuilder.AddCustomAttributes(t, "ColumnChooserSettings",
					new DescriptionAttribute(SR.GetString("ColumnLayout_ColumnChooserSettings_Property")),
				    new DisplayNameAttribute("ColumnChooserSettings")				);

				#endregion // ColumnLayout Properties

				#region XamGridColumnCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridColumnCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamGridColumnCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamGridColumnCommandSource_Properties"))
				);

				#endregion // XamGridColumnCommandSource Properties

				#region FilterRowExpansionIndicatorCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterRowExpansionIndicatorCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FilterRowExpansionIndicatorCell Properties

				#region FilterRowCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterRowCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // FilterRowCellControl Properties

				#region CellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentProvider",
					new DescriptionAttribute(SR.GetString("CellControl_ContentProvider_Property")),
				    new DisplayNameAttribute("ContentProvider")				);

				#endregion // CellControl Properties

				#region ColumnLayoutTemplateCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ColumnLayoutTemplateCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnLayoutTemplateCell Properties

				#region InvalidColumnKeyException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.InvalidColumnKeyException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // InvalidColumnKeyException Properties

				#region EmptyColumnKeyException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EmptyColumnKeyException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // EmptyColumnKeyException Properties

				#region DuplicateColumnKeyException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DuplicateColumnKeyException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DuplicateColumnKeyException Properties

				#region TypeResolutionException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.TypeResolutionException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TypeResolutionException Properties

				#region InvalidColumnTypeMappingException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.InvalidColumnTypeMappingException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // InvalidColumnTypeMappingException Properties

				#region InvalidPageIndexException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.InvalidPageIndexException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // InvalidPageIndexException Properties

				#region InvalidActiveCellException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.InvalidActiveCellException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // InvalidActiveCellException Properties

				#region ResizingColumnCannotBeRemovedException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ResizingColumnCannotBeRemovedException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ResizingColumnCannotBeRemovedException Properties

				#region NullDataException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.NullDataException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NullDataException Properties

				#region DataTypeMismatchException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DataTypeMismatchException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DataTypeMismatchException Properties

				#region ColumnLayoutException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnLayoutException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnLayoutException Properties

				#region ChildColumnIsSelectedAccessDeniedException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ChildColumnIsSelectedAccessDeniedException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ChildColumnIsSelectedAccessDeniedException Properties

				#region ChildColumnIsGroupByAccessDeniedException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ChildColumnIsGroupByAccessDeniedException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ChildColumnIsGroupByAccessDeniedException Properties

				#region InvalidRowIndexException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.InvalidRowIndexException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // InvalidRowIndexException Properties

				#region NullConditionalFormatEvaluationValueException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.NullConditionalFormatEvaluationValueException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NullConditionalFormatEvaluationValueException Properties

				#region MaximumSummaryCalculator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.MaximumSummaryCalculator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MaximumSummaryCalculator Properties

				#region MinimumSummaryCalculator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.MinimumSummaryCalculator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MinimumSummaryCalculator Properties

				#region CountSummaryCalculator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CountSummaryCalculator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CountSummaryCalculator Properties

				#region SumSummaryCalculator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SumSummaryCalculator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SumSummaryCalculator Properties

				#region AverageSummaryCalculator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.AverageSummaryCalculator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AverageSummaryCalculator Properties

				#region PagerSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.PagerSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowPaging",
					new DescriptionAttribute(SR.GetString("PagerSettingsOverride_AllowPaging_Property")),
				    new DisplayNameAttribute("AllowPaging")				);


				tableBuilder.AddCustomAttributes(t, "AllowPagingResolved",
					new DescriptionAttribute(SR.GetString("PagerSettingsOverride_AllowPagingResolved_Property")),
				    new DisplayNameAttribute("AllowPagingResolved")				);


				tableBuilder.AddCustomAttributes(t, "PageSize",
					new DescriptionAttribute(SR.GetString("PagerSettingsOverride_PageSize_Property")),
				    new DisplayNameAttribute("PageSize")				);


				tableBuilder.AddCustomAttributes(t, "PageSizeResolved",
					new DescriptionAttribute(SR.GetString("PagerSettingsOverride_PageSizeResolved_Property")),
				    new DisplayNameAttribute("PageSizeResolved")				);


				tableBuilder.AddCustomAttributes(t, "PagerRowHeight",
					new DescriptionAttribute(SR.GetString("PagerSettingsOverride_PagerRowHeight_Property")),
				    new DisplayNameAttribute("PagerRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "PagerRowHeightResolved",
					new DescriptionAttribute(SR.GetString("PagerSettingsOverride_PagerRowHeightResolved_Property")),
				    new DisplayNameAttribute("PagerRowHeightResolved")				);

				#endregion // PagerSettingsOverride Properties

				#region StyleSettingsOverrideBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.StyleSettingsOverrideBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("StyleSettingsOverrideBase_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "StyleResolved",
					new DescriptionAttribute(SR.GetString("StyleSettingsOverrideBase_StyleResolved_Property")),
				    new DisplayNameAttribute("StyleResolved")				);

				#endregion // StyleSettingsOverrideBase Properties

				#region PagerControlBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagerControlBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentPageIndex",
					new DescriptionAttribute(SR.GetString("PagerControlBase_CurrentPageIndex_Property")),
				    new DisplayNameAttribute("CurrentPageIndex")				);


				tableBuilder.AddCustomAttributes(t, "TotalPages",
					new DescriptionAttribute(SR.GetString("PagerControlBase_TotalPages_Property")),
				    new DisplayNameAttribute("TotalPages")				);

				#endregion // PagerControlBase Properties

				#region PagerControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagerControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MaximumAvailablePagerValues",
					new DescriptionAttribute(SR.GetString("PagerControl_MaximumAvailablePagerValues_Property")),
				    new DisplayNameAttribute("MaximumAvailablePagerValues")				);

				#endregion // PagerControl Properties

				#region PagingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NextPage",
					new DescriptionAttribute(SR.GetString("PagingEventArgs_NextPage_Property")),
				    new DisplayNameAttribute("NextPage")				);

				#endregion // PagingEventArgs Properties

				#region FixColumnCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixColumnCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FixColumnCommand Properties

				#region FixColumnLeftCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixColumnLeftCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FixColumnLeftCommand Properties

				#region FixColumnRightCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixColumnRightCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FixColumnRightCommand Properties

				#region UnfixColumnCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.UnfixColumnCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // UnfixColumnCommand Properties

				#region AverageValueConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.AverageValueConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FormattingTarget",
					new DescriptionAttribute(SR.GetString("AverageValueConditionalFormatRule_FormattingTarget_Property")),
				    new DisplayNameAttribute("FormattingTarget")				);

				#endregion // AverageValueConditionalFormatRule Properties

				#region AverageValueConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.AverageValueConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AverageValueConditionalFormatRuleProxy Properties

				#region ColumnResizingSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnResizingSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IndicatorStyle",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettings_IndicatorStyle_Property")),
				    new DisplayNameAttribute("IndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "AllowColumnResizing",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettings_AllowColumnResizing_Property")),
				    new DisplayNameAttribute("AllowColumnResizing")				);


				tableBuilder.AddCustomAttributes(t, "AllowDoubleClickToSize",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettings_AllowDoubleClickToSize_Property")),
				    new DisplayNameAttribute("AllowDoubleClickToSize")				);


				tableBuilder.AddCustomAttributes(t, "AllowMultipleColumnResize",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettings_AllowMultipleColumnResize_Property")),
				    new DisplayNameAttribute("AllowMultipleColumnResize")				);


				tableBuilder.AddCustomAttributes(t, "AllowCellAreaResizing",
					new DescriptionAttribute(SR.GetString("ColumnResizingSettings_AllowCellAreaResizing_Property")),
				    new DisplayNameAttribute("AllowCellAreaResizing")				);

				#endregion // ColumnResizingSettings Properties

				#region CellsPanelAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.CellsPanelAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CanSelectMultiple",
					new DescriptionAttribute(SR.GetString("CellsPanelAutomationPeer_CanSelectMultiple_Property")),
				    new DisplayNameAttribute("CanSelectMultiple")				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionRequired",
					new DescriptionAttribute(SR.GetString("CellsPanelAutomationPeer_IsSelectionRequired_Property")),
				    new DisplayNameAttribute("IsSelectionRequired")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("CellsPanelAutomationPeer_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "SelectionContainer",
					new DescriptionAttribute(SR.GetString("CellsPanelAutomationPeer_SelectionContainer_Property")),
				    new DisplayNameAttribute("SelectionContainer")				);


				tableBuilder.AddCustomAttributes(t, "ExpandCollapseState",
					new DescriptionAttribute(SR.GetString("CellsPanelAutomationPeer_ExpandCollapseState_Property")),
				    new DisplayNameAttribute("ExpandCollapseState")				);

				#endregion // CellsPanelAutomationPeer Properties

				#region XamGrid Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGrid");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamGridAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "Columns",
					new DescriptionAttribute(SR.GetString("XamGrid_Columns_Property")),
				    new DisplayNameAttribute("Columns"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Rows",
					new DescriptionAttribute(SR.GetString("XamGrid_Rows_Property")),
				    new DisplayNameAttribute("Rows"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("XamGrid_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayouts",
					new DescriptionAttribute(SR.GetString("XamGrid_ColumnLayouts_Property")),
				    new DisplayNameAttribute("ColumnLayouts"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxDepth",
					new DescriptionAttribute(SR.GetString("XamGrid_MaxDepth_Property")),
				    new DisplayNameAttribute("MaxDepth"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderVisibility",
					new DescriptionAttribute(SR.GetString("XamGrid_HeaderVisibility_Property")),
				    new DisplayNameAttribute("HeaderVisibility"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FooterVisibility",
					new DescriptionAttribute(SR.GetString("XamGrid_FooterVisibility_Property")),
				    new DisplayNameAttribute("FooterVisibility"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AutoGenerateColumns",
					new DescriptionAttribute(SR.GetString("XamGrid_AutoGenerateColumns_Property")),
				    new DisplayNameAttribute("AutoGenerateColumns"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Indentation",
					new DescriptionAttribute(SR.GetString("XamGrid_Indentation_Property")),
				    new DisplayNameAttribute("Indentation"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayoutHeaderVisibility",
					new DescriptionAttribute(SR.GetString("XamGrid_ColumnLayoutHeaderVisibility_Property")),
				    new DisplayNameAttribute("ColumnLayoutHeaderVisibility"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsAlternateRowsEnabled",
					new DescriptionAttribute(SR.GetString("XamGrid_IsAlternateRowsEnabled_Property")),
				    new DisplayNameAttribute("IsAlternateRowsEnabled"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CellStyle",
					new DescriptionAttribute(SR.GetString("XamGrid_CellStyle_Property")),
				    new DisplayNameAttribute("CellStyle"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyle",
					new DescriptionAttribute(SR.GetString("XamGrid_HeaderStyle_Property")),
				    new DisplayNameAttribute("HeaderStyle"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FooterStyle",
					new DescriptionAttribute(SR.GetString("XamGrid_FooterStyle_Property")),
				    new DisplayNameAttribute("FooterStyle"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FixedRowSeparatorStyle",
					new DescriptionAttribute(SR.GetString("XamGrid_FixedRowSeparatorStyle_Property")),
				    new DisplayNameAttribute("FixedRowSeparatorStyle"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ChildBandHeaderStyle",
					new DescriptionAttribute(SR.GetString("XamGrid_ChildBandHeaderStyle_Property")),
				    new DisplayNameAttribute("ChildBandHeaderStyle"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnsHeaderTemplate",
					new DescriptionAttribute(SR.GetString("XamGrid_ColumnsHeaderTemplate_Property")),
				    new DisplayNameAttribute("ColumnsHeaderTemplate"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnsFooterTemplate",
					new DescriptionAttribute(SR.GetString("XamGrid_ColumnsFooterTemplate_Property")),
				    new DisplayNameAttribute("ColumnsFooterTemplate"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_RowSelectorSettings_Property")),
				    new DisplayNameAttribute("RowSelectorSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_ExpansionIndicatorSettings_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_AddNewRowSettings_Property")),
				    new DisplayNameAttribute("AddNewRowSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SummaryRowSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_SummaryRowSettings_Property")),
				    new DisplayNameAttribute("SummaryRowSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FilteringSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_FilteringSettings_Property")),
				    new DisplayNameAttribute("FilteringSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnMovingSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_ColumnMovingSettings_Property")),
				    new DisplayNameAttribute("ColumnMovingSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EditingSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_EditingSettings_Property")),
				    new DisplayNameAttribute("EditingSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FixedColumnSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_FixedColumnSettings_Property")),
				    new DisplayNameAttribute("FixedColumnSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SortingSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_SortingSettings_Property")),
				    new DisplayNameAttribute("SortingSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "GroupBySettings",
					new DescriptionAttribute(SR.GetString("XamGrid_GroupBySettings_Property")),
				    new DisplayNameAttribute("GroupBySettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PagerSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_PagerSettings_Property")),
				    new DisplayNameAttribute("PagerSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveCell",
					new DescriptionAttribute(SR.GetString("XamGrid_ActiveCell_Property")),
				    new DisplayNameAttribute("ActiveCell"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "KeyboardNavigation",
					new DescriptionAttribute(SR.GetString("XamGrid_KeyboardNavigation_Property")),
				    new DisplayNameAttribute("KeyboardNavigation"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectionSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_SelectionSettings_Property")),
				    new DisplayNameAttribute("SelectionSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DeferredScrollingSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_DeferredScrollingSettings_Property")),
				    new DisplayNameAttribute("DeferredScrollingSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnTypeMappings",
					new DescriptionAttribute(SR.GetString("XamGrid_ColumnTypeMappings_Property")),
				    new DisplayNameAttribute("ColumnTypeMappings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnResizingSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_ColumnResizingSettings_Property")),
				    new DisplayNameAttribute("ColumnResizingSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RowHeight",
					new DescriptionAttribute(SR.GetString("XamGrid_RowHeight_Property")),
				    new DisplayNameAttribute("RowHeight"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinimumRowHeight",
					new DescriptionAttribute(SR.GetString("XamGrid_MinimumRowHeight_Property")),
				    new DisplayNameAttribute("MinimumRowHeight"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DeleteKeyAction",
					new DescriptionAttribute(SR.GetString("XamGrid_DeleteKeyAction_Property")),
				    new DisplayNameAttribute("DeleteKeyAction"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderRowHeight",
					new DescriptionAttribute(SR.GetString("XamGrid_HeaderRowHeight_Property")),
				    new DisplayNameAttribute("HeaderRowHeight"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FooterRowHeight",
					new DescriptionAttribute(SR.GetString("XamGrid_FooterRowHeight_Property")),
				    new DisplayNameAttribute("FooterRowHeight"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ChildBandHeaderHeight",
					new DescriptionAttribute(SR.GetString("XamGrid_ChildBandHeaderHeight_Property")),
				    new DisplayNameAttribute("ChildBandHeaderHeight"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnWidth",
					new DescriptionAttribute(SR.GetString("XamGrid_ColumnWidth_Property")),
				    new DisplayNameAttribute("ColumnWidth"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RowHover",
					new DescriptionAttribute(SR.GetString("XamGrid_RowHover_Property")),
				    new DisplayNameAttribute("RowHover"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FillerColumnSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_FillerColumnSettings_Property")),
				    new DisplayNameAttribute("FillerColumnSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextHorizontalAlignment",
					new DescriptionAttribute(SR.GetString("XamGrid_HeaderTextHorizontalAlignment_Property")),
				    new DisplayNameAttribute("HeaderTextHorizontalAlignment"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextVerticalAlignment",
					new DescriptionAttribute(SR.GetString("XamGrid_HeaderTextVerticalAlignment_Property")),
				    new DisplayNameAttribute("HeaderTextVerticalAlignment"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ConditionalFormattingSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_ConditionalFormattingSettings_Property")),
				    new DisplayNameAttribute("ConditionalFormattingSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnChooserSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_ColumnChooserSettings_Property")),
				    new DisplayNameAttribute("ColumnChooserSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ClipboardSettings",
					new DescriptionAttribute(SR.GetString("XamGrid_ClipboardSettings_Property")),
				    new DisplayNameAttribute("ClipboardSettings"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveItem",
					new DescriptionAttribute(SR.GetString("XamGrid_ActiveItem_Property")),
				    new DisplayNameAttribute("ActiveItem"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DataManagerProvider",
					new DescriptionAttribute(SR.GetString("XamGrid_DataManagerProvider_Property")),
				    new DisplayNameAttribute("DataManagerProvider"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsTouchSupportEnabled",
					new DescriptionAttribute(SR.GetString("XamGrid_IsTouchSupportEnabled_Property")),
				    new DisplayNameAttribute("IsTouchSupportEnabled"),
					new CategoryAttribute(SR.GetString("XamGrid_Properties"))
				);

				#endregion // XamGrid Properties

				#region RowCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowCommandBase Properties

				#region XamGridRowCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridRowCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamGridRowCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamGridRowCommandSource_Properties"))
				);

				#endregion // XamGridRowCommandSource Properties

				#region UnboundColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.UnboundColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplate",
					new DescriptionAttribute(SR.GetString("UnboundColumn_ItemTemplate_Property")),
				    new DisplayNameAttribute("ItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "EditorTemplate",
					new DescriptionAttribute(SR.GetString("UnboundColumn_EditorTemplate_Property")),
				    new DisplayNameAttribute("EditorTemplate")				);


				tableBuilder.AddCustomAttributes(t, "FilterItemTemplate",
					new DescriptionAttribute(SR.GetString("UnboundColumn_FilterItemTemplate_Property")),
				    new DisplayNameAttribute("FilterItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "FilterEditorTemplate",
					new DescriptionAttribute(SR.GetString("UnboundColumn_FilterEditorTemplate_Property")),
				    new DisplayNameAttribute("FilterEditorTemplate")				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("UnboundColumn_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);

				#endregion // UnboundColumn Properties

				#region TextColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.TextColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RemovePaddingDuringEditing",
					new DescriptionAttribute(SR.GetString("TextColumnContentProvider_RemovePaddingDuringEditing_Property")),
				    new DisplayNameAttribute("RemovePaddingDuringEditing")				);

				#endregion // TextColumnContentProvider Properties

				#region ImageColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ImageColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsFilterable",
					new DescriptionAttribute(SR.GetString("ImageColumn_IsFilterable_Property")),
				    new DisplayNameAttribute("IsFilterable")				);


				tableBuilder.AddCustomAttributes(t, "DataType",
					new DescriptionAttribute(SR.GetString("ImageColumn_DataType_Property")),
				    new DisplayNameAttribute("DataType")				);


				tableBuilder.AddCustomAttributes(t, "ImageStyle",
					new DescriptionAttribute(SR.GetString("ImageColumn_ImageStyle_Property")),
				    new DisplayNameAttribute("ImageStyle")				);


				tableBuilder.AddCustomAttributes(t, "ImageHeight",
					new DescriptionAttribute(SR.GetString("ImageColumn_ImageHeight_Property")),
				    new DisplayNameAttribute("ImageHeight")				);


				tableBuilder.AddCustomAttributes(t, "ImageWidth",
					new DescriptionAttribute(SR.GetString("ImageColumn_ImageWidth_Property")),
				    new DisplayNameAttribute("ImageWidth")				);

				#endregion // ImageColumn Properties

				#region ColumnBaseCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnBaseCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayout",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_ColumnLayout_Property")),
				    new DisplayNameAttribute("ColumnLayout")				);


				tableBuilder.AddCustomAttributes(t, "ColumnLayouts",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_ColumnLayouts_Property")),
				    new DisplayNameAttribute("ColumnLayouts")				);


				tableBuilder.AddCustomAttributes(t, "FixedAdornerColumns",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_FixedAdornerColumns_Property")),
				    new DisplayNameAttribute("FixedAdornerColumns")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorColumn",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_ExpansionIndicatorColumn_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorColumn")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorColumn",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_RowSelectorColumn_Property")),
				    new DisplayNameAttribute("RowSelectorColumn")				);


				tableBuilder.AddCustomAttributes(t, "DataColumns",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_DataColumns_Property")),
				    new DisplayNameAttribute("DataColumns")				);


				tableBuilder.AddCustomAttributes(t, "FillerColumn",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_FillerColumn_Property")),
				    new DisplayNameAttribute("FillerColumn")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderColumnLeft",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_FixedBorderColumnLeft_Property")),
				    new DisplayNameAttribute("FixedBorderColumnLeft")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderColumnRight",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_FixedBorderColumnRight_Property")),
				    new DisplayNameAttribute("FixedBorderColumnRight")				);


				tableBuilder.AddCustomAttributes(t, "FixedColumnsLeft",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_FixedColumnsLeft_Property")),
				    new DisplayNameAttribute("FixedColumnsLeft")				);


				tableBuilder.AddCustomAttributes(t, "GroupByColumns",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_GroupByColumns_Property")),
				    new DisplayNameAttribute("GroupByColumns")				);


				tableBuilder.AddCustomAttributes(t, "SelectedColumns",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_SelectedColumns_Property")),
				    new DisplayNameAttribute("SelectedColumns")				);


				tableBuilder.AddCustomAttributes(t, "FixedColumnsRight",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_FixedColumnsRight_Property")),
				    new DisplayNameAttribute("FixedColumnsRight")				);


				tableBuilder.AddCustomAttributes(t, "SortedColumns",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_SortedColumns_Property")),
				    new DisplayNameAttribute("SortedColumns")				);


				tableBuilder.AddCustomAttributes(t, "AllColumns",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_AllColumns_Property")),
				    new DisplayNameAttribute("AllColumns")				);


				tableBuilder.AddCustomAttributes(t, "AllVisibleColumns",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_AllVisibleColumns_Property")),
				    new DisplayNameAttribute("AllVisibleColumns")				);


				tableBuilder.AddCustomAttributes(t, "AllVisibleChildColumns",
					new DescriptionAttribute(SR.GetString("ColumnBaseCollection_AllVisibleChildColumns_Property")),
				    new DisplayNameAttribute("AllVisibleChildColumns")				);

				#endregion // ColumnBaseCollection Properties

				#region SummaryRowSelectorCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SummaryRowSelectorCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // SummaryRowSelectorCellControl Properties

				#region FixedBorderHeaderCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixedBorderHeaderCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // FixedBorderHeaderCellControl Properties

				#region ExpansionIndicatorHeaderCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ExpansionIndicatorHeaderCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // ExpansionIndicatorHeaderCell Properties

				#region AddNewRowSelectorCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.AddNewRowSelectorCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // AddNewRowSelectorCellControl Properties

				#region SelectedCellsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SelectedCellsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Grid",
					new DescriptionAttribute(SR.GetString("SelectedCollectionBase`1_Grid_Property")),
				    new DisplayNameAttribute("Grid")				);

				#endregion // SelectedCellsCollection Properties

				#region FixedColumnsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FixedColumnsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FixedColumnsCollection Properties

				#region ExpansionIndicatorSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ExpansionIndicatorSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyle",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorSettingsOverride_HeaderStyle_Property")),
				    new DisplayNameAttribute("HeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorSettingsOverride_HeaderStyleResolved_Property")),
				    new DisplayNameAttribute("HeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyle",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorSettingsOverride_FooterStyle_Property")),
				    new DisplayNameAttribute("FooterStyle")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyleResolved",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorSettingsOverride_FooterStyleResolved_Property")),
				    new DisplayNameAttribute("FooterStyleResolved")				);

				#endregion // ExpansionIndicatorSettingsOverride Properties

				#region VisualSettingsOverrideBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.VisualSettingsOverrideBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Visibility",
					new DescriptionAttribute(SR.GetString("VisualSettingsOverrideBase_Visibility_Property")),
				    new DisplayNameAttribute("Visibility")				);


				tableBuilder.AddCustomAttributes(t, "VisibilityResolved",
					new DescriptionAttribute(SR.GetString("VisualSettingsOverrideBase_VisibilityResolved_Property")),
				    new DisplayNameAttribute("VisibilityResolved")				);

				#endregion // VisualSettingsOverrideBase Properties

				#region NotContainingConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.NotContainingConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NotContainingConditionalFormatRule Properties

				#region NotContainingConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.NotContainingConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NotContainingConditionalFormatRuleProxy Properties

				#region ConditionalFormatIcon Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ConditionalFormatIcon");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Icon",
					new DescriptionAttribute(SR.GetString("ConditionalFormatIcon_Icon_Property")),
				    new DisplayNameAttribute("Icon")				);


				tableBuilder.AddCustomAttributes(t, "Operator",
					new DescriptionAttribute(SR.GetString("ConditionalFormatIcon_Operator_Property")),
				    new DisplayNameAttribute("Operator")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("ConditionalFormatIcon_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "ValueType",
					new DescriptionAttribute(SR.GetString("ConditionalFormatIcon_ValueType_Property")),
				    new DisplayNameAttribute("ValueType")				);

				#endregion // ConditionalFormatIcon Properties

				#region DataBarControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.DataBarControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DataBarDirection",
					new DescriptionAttribute(SR.GetString("DataBarControl_DataBarDirection_Property")),
				    new DisplayNameAttribute("DataBarDirection")				);


				tableBuilder.AddCustomAttributes(t, "NegativeValueBarBrush",
					new DescriptionAttribute(SR.GetString("DataBarControl_NegativeValueBarBrush_Property")),
				    new DisplayNameAttribute("NegativeValueBarBrush")				);


				tableBuilder.AddCustomAttributes(t, "PositiveValueBarBrush",
					new DescriptionAttribute(SR.GetString("DataBarControl_PositiveValueBarBrush_Property")),
				    new DisplayNameAttribute("PositiveValueBarBrush")				);


				tableBuilder.AddCustomAttributes(t, "BarHeight",
					new DescriptionAttribute(SR.GetString("DataBarControl_BarHeight_Property")),
				    new DisplayNameAttribute("BarHeight")				);


				tableBuilder.AddCustomAttributes(t, "BarPercentage",
					new DescriptionAttribute(SR.GetString("DataBarControl_BarPercentage_Property")),
				    new DisplayNameAttribute("BarPercentage")				);


				tableBuilder.AddCustomAttributes(t, "DataBarPositiveNegative",
					new DescriptionAttribute(SR.GetString("DataBarControl_DataBarPositiveNegative_Property")),
				    new DisplayNameAttribute("DataBarPositiveNegative")				);


				tableBuilder.AddCustomAttributes(t, "UnidirectionalDataBarBrush",
					new DescriptionAttribute(SR.GetString("DataBarControl_UnidirectionalDataBarBrush_Property")),
				    new DisplayNameAttribute("UnidirectionalDataBarBrush")				);

				#endregion // DataBarControl Properties

				#region PagerRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagerRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("PagerRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "Cells",
					new DescriptionAttribute(SR.GetString("PagerRow_Cells_Property")),
				    new DisplayNameAttribute("Cells")				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("PagerRow_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);

				#endregion // PagerRow Properties

				#region RowSelectorFooterCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowSelectorFooterCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // RowSelectorFooterCellControl Properties

				#region FooterCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FooterCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // FooterCellControl Properties

				#region PagerCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagerCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PagerCell Properties

				#region MergedSummaryCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.MergedSummaryCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // MergedSummaryCellControl Properties

				#region SummaryRowCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SummaryRowCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "SummaryDisplayTemplate",
					new DescriptionAttribute(SR.GetString("SummaryRowCellControl_SummaryDisplayTemplate_Property")),
				    new DisplayNameAttribute("SummaryDisplayTemplate")				);

				#endregion // SummaryRowCellControl Properties

				#region GroupHeaderCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupHeaderCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupHeaderCell Properties

				#region HeaderCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.HeaderCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnsHiddenToLeft",
					new DescriptionAttribute(SR.GetString("HeaderCell_ColumnsHiddenToLeft_Property")),
				    new DisplayNameAttribute("ColumnsHiddenToLeft")				);


				tableBuilder.AddCustomAttributes(t, "ColumnsHiddenToRight",
					new DescriptionAttribute(SR.GetString("HeaderCell_ColumnsHiddenToRight_Property")),
				    new DisplayNameAttribute("ColumnsHiddenToRight")				);

				#endregion // HeaderCell Properties

				#region SummaryOperandCollection Properties
				t = controlAssembly.GetType("Infragistics.SummaryOperandCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SummaryOperandCollection Properties

				#region HeaderDropDownControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.HeaderDropDownControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsOpen",
					new DescriptionAttribute(SR.GetString("HeaderDropDownControl_IsOpen_Property")),
				    new DisplayNameAttribute("IsOpen")				);


				tableBuilder.AddCustomAttributes(t, "OpenButtonContent",
					new DescriptionAttribute(SR.GetString("HeaderDropDownControl_OpenButtonContent_Property")),
				    new DisplayNameAttribute("OpenButtonContent")				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("HeaderDropDownControl_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "AllowResizing",
					new DescriptionAttribute(SR.GetString("HeaderDropDownControl_AllowResizing_Property")),
				    new DisplayNameAttribute("AllowResizing")				);

				#endregion // HeaderDropDownControl Properties

				#region XamGridPopupCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.XamGridPopupCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamGridPopupCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamGridPopupCommandSource_Properties"))
				);

				#endregion // XamGridPopupCommandSource Properties

				#region PopupCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PopupCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PopupCommandBase Properties

				#region OpenPopupCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.OpenPopupCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // OpenPopupCommand Properties

				#region TogglePopupCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.TogglePopupCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TogglePopupCommand Properties

				#region ClosePopupCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ClosePopupCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ClosePopupCommand Properties

				#region FilteringDataContext Properties
				t = controlAssembly.GetType("Infragistics.FilteringDataContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("FilteringDataContext_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "ColumnKey",
					new DescriptionAttribute(SR.GetString("FilteringDataContext_ColumnKey_Property")),
				    new DisplayNameAttribute("ColumnKey")				);


				tableBuilder.AddCustomAttributes(t, "RowData",
					new DescriptionAttribute(SR.GetString("FilteringDataContext_RowData_Property")),
				    new DisplayNameAttribute("RowData")				);

				#endregion // FilteringDataContext Properties

				#region ColumnFilterDialogControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ColumnFilterDialogControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("ColumnFilterDialogControl_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "CancelButtonText",
					new DescriptionAttribute(SR.GetString("ColumnFilterDialogControl_CancelButtonText_Property")),
				    new DisplayNameAttribute("CancelButtonText")				);


				tableBuilder.AddCustomAttributes(t, "OKButtonText",
					new DescriptionAttribute(SR.GetString("ColumnFilterDialogControl_OKButtonText_Property")),
				    new DisplayNameAttribute("OKButtonText")				);


				tableBuilder.AddCustomAttributes(t, "AndRadioButtonText",
					new DescriptionAttribute(SR.GetString("ColumnFilterDialogControl_AndRadioButtonText_Property")),
				    new DisplayNameAttribute("AndRadioButtonText")				);


				tableBuilder.AddCustomAttributes(t, "OrRadioButtonText",
					new DescriptionAttribute(SR.GetString("ColumnFilterDialogControl_OrRadioButtonText_Property")),
				    new DisplayNameAttribute("OrRadioButtonText")				);

				#endregion // ColumnFilterDialogControl Properties

				#region ProxyValueContainer Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ProxyValueContainer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ProxyValue1",
					new DescriptionAttribute(SR.GetString("ProxyValueContainer_ProxyValue1_Property")),
				    new DisplayNameAttribute("ProxyValue1")				);


				tableBuilder.AddCustomAttributes(t, "ProxyValue2",
					new DescriptionAttribute(SR.GetString("ProxyValueContainer_ProxyValue2_Property")),
				    new DisplayNameAttribute("ProxyValue2")				);

				#endregion // ProxyValueContainer Properties

				#region CustomFilterDialogFilteringDataContext Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CustomFilterDialogFilteringDataContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CustomFilterDialogFilteringDataContext Properties

				#region EndingWithConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EndingWithConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // EndingWithConditionalFormatRule Properties

				#region EndingWithConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EndingWithConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // EndingWithConditionalFormatRuleProxy Properties

				#region BeginningWithConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.BeginningWithConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // BeginningWithConditionalFormatRule Properties

				#region BeginningWithConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.BeginningWithConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // BeginningWithConditionalFormatRuleProxy Properties

				#region ChildBandRowsManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ChildBandRowsManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("ChildBandRowsManager_Column_Property")),
				    new DisplayNameAttribute("Column")				);


				tableBuilder.AddCustomAttributes(t, "Rows",
					new DescriptionAttribute(SR.GetString("ChildBandRowsManager_Rows_Property")),
				    new DisplayNameAttribute("Rows")				);

				#endregion // ChildBandRowsManager Properties

				#region ChildBandCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ChildBandCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ChildBandCollection Properties

				#region SummaryRowSelectorCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SummaryRowSelectorCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SummaryRowSelectorCell Properties

				#region RowSelectorCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowSelectorCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowSelectorCell Properties

				#region MergedSummaryCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.MergedSummaryCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MergedSummaryCell Properties

				#region ExpansionIndicatorFooterCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ExpansionIndicatorFooterCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // ExpansionIndicatorFooterCellControl Properties

				#region AddNewRowExpansionIndicatorCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.AddNewRowExpansionIndicatorCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AddNewRowExpansionIndicatorCell Properties

				#region RowsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Grid",
					new DescriptionAttribute(SR.GetString("RowsPanel_Grid_Property")),
				    new DisplayNameAttribute("Grid")				);


				tableBuilder.AddCustomAttributes(t, "VisibleRows",
					new DescriptionAttribute(SR.GetString("RowsPanel_VisibleRows_Property")),
				    new DisplayNameAttribute("VisibleRows")				);


				tableBuilder.AddCustomAttributes(t, "VisibleManagers",
					new DescriptionAttribute(SR.GetString("RowsPanel_VisibleManagers_Property")),
				    new DisplayNameAttribute("VisibleManagers")				);


				tableBuilder.AddCustomAttributes(t, "FixedRowsBottom",
					new DescriptionAttribute(SR.GetString("RowsPanel_FixedRowsBottom_Property")),
				    new DisplayNameAttribute("FixedRowsBottom")				);


				tableBuilder.AddCustomAttributes(t, "FixedRowsTop",
					new DescriptionAttribute(SR.GetString("RowsPanel_FixedRowsTop_Property")),
				    new DisplayNameAttribute("FixedRowsTop")				);


				tableBuilder.AddCustomAttributes(t, "CustomFilterDialogControl",
					new DescriptionAttribute(SR.GetString("RowsPanel_CustomFilterDialogControl_Property")),
				    new DisplayNameAttribute("CustomFilterDialogControl")				);

				#endregion // RowsPanel Properties

				#region SummaryColumnSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SummaryColumnSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SummaryOperands",
					new DescriptionAttribute(SR.GetString("SummaryColumnSettings_SummaryOperands_Property")),
				    new DisplayNameAttribute("SummaryOperands")				);


				tableBuilder.AddCustomAttributes(t, "SummaryRowCellStyle",
					new DescriptionAttribute(SR.GetString("SummaryColumnSettings_SummaryRowCellStyle_Property")),
				    new DisplayNameAttribute("SummaryRowCellStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupBySummaryDefinitions",
					new DescriptionAttribute(SR.GetString("SummaryColumnSettings_GroupBySummaryDefinitions_Property")),
				    new DisplayNameAttribute("GroupBySummaryDefinitions")				);

				#endregion // SummaryColumnSettings Properties

				#region GroupByAreaPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByAreaPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupByAreaPanel Properties

				#region ColumnChooserSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnChooserSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowHiddenColumnIndicator",
					new DescriptionAttribute(SR.GetString("ColumnChooserSettings_AllowHiddenColumnIndicator_Property")),
				    new DisplayNameAttribute("AllowHiddenColumnIndicator")				);


				tableBuilder.AddCustomAttributes(t, "AllowHideColumnIcon",
					new DescriptionAttribute(SR.GetString("ColumnChooserSettings_AllowHideColumnIcon_Property")),
				    new DisplayNameAttribute("AllowHideColumnIcon")				);


				tableBuilder.AddCustomAttributes(t, "HiddenColumnIndicatorToolTipText",
					new DescriptionAttribute(SR.GetString("ColumnChooserSettings_HiddenColumnIndicatorToolTipText_Property")),
				    new DisplayNameAttribute("HiddenColumnIndicatorToolTipText")				);


				tableBuilder.AddCustomAttributes(t, "ColumnChooserDisplayText",
					new DescriptionAttribute(SR.GetString("ColumnChooserSettings_ColumnChooserDisplayText_Property")),
				    new DisplayNameAttribute("ColumnChooserDisplayText")				);


				tableBuilder.AddCustomAttributes(t, "InitialLocation",
					new DescriptionAttribute(SR.GetString("ColumnChooserSettings_InitialLocation_Property")),
				    new DisplayNameAttribute("InitialLocation")				);


				tableBuilder.AddCustomAttributes(t, "AllowColumnMoving",
					new DescriptionAttribute(SR.GetString("ColumnChooserSettings_AllowColumnMoving_Property")),
				    new DisplayNameAttribute("AllowColumnMoving")				);

				#endregion // ColumnChooserSettings Properties

				#region ClipboardSortColumnComparer Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ClipboardSortColumnComparer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ClipboardSortColumnComparer Properties

				#region ColumnWidthTypeConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ColumnWidthTypeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnWidthTypeConverter Properties

				#region MergedSummaryRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.MergedSummaryRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("MergedSummaryRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("MergedSummaryRow_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "MergedColumnInfo",
					new DescriptionAttribute(SR.GetString("MergedSummaryRow_MergedColumnInfo_Property")),
				    new DisplayNameAttribute("MergedColumnInfo")				);

				#endregion // MergedSummaryRow Properties

				#region GroupFooterCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupFooterCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // GroupFooterCellControl Properties

				#region FilterRowCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterRowCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsEditable",
					new DescriptionAttribute(SR.GetString("FilterRowCell_IsEditable_Property")),
				    new DisplayNameAttribute("IsEditable")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("FilterRowCell_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "FilteringOperand",
					new DescriptionAttribute(SR.GetString("FilterRowCell_FilteringOperand_Property")),
				    new DisplayNameAttribute("FilteringOperand")				);


				tableBuilder.AddCustomAttributes(t, "FilteringOperandResolved",
					new DescriptionAttribute(SR.GetString("FilterRowCell_FilteringOperandResolved_Property")),
				    new DisplayNameAttribute("FilteringOperandResolved")				);


				tableBuilder.AddCustomAttributes(t, "FilterCellValue",
					new DescriptionAttribute(SR.GetString("FilterRowCell_FilterCellValue_Property")),
				    new DisplayNameAttribute("FilterCellValue")				);


				tableBuilder.AddCustomAttributes(t, "FilterCellValueResolved",
					new DescriptionAttribute(SR.GetString("FilterRowCell_FilterCellValueResolved_Property")),
				    new DisplayNameAttribute("FilterCellValueResolved")				);

				#endregion // FilterRowCell Properties

				#region SummaryBaseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SummaryBaseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SummaryBaseCommand Properties

				#region GroupByColumnsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GroupByColumnsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Grid",
					new DescriptionAttribute(SR.GetString("GroupByColumnsCollection_Grid_Property")),
				    new DisplayNameAttribute("Grid")				);

				#endregion // GroupByColumnsCollection Properties

				#region ScrollTipInfo Properties
				t = controlAssembly.GetType("Infragistics.ScrollTipInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("ScrollTipInfo_Row_Property")),
				    new DisplayNameAttribute("Row")				);


				tableBuilder.AddCustomAttributes(t, "FirstColumnValue",
					new DescriptionAttribute(SR.GetString("ScrollTipInfo_FirstColumnValue_Property")),
				    new DisplayNameAttribute("FirstColumnValue")				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("ScrollTipInfo_Column_Property")),
				    new DisplayNameAttribute("Column")				);

				#endregion // ScrollTipInfo Properties

				#region ThreeColorScaleConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ThreeColorScaleConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MedianValueType",
					new DescriptionAttribute(SR.GetString("ThreeColorScaleConditionalFormatRule_MedianValueType_Property")),
				    new DisplayNameAttribute("MedianValueType")				);


				tableBuilder.AddCustomAttributes(t, "MedianValue",
					new DescriptionAttribute(SR.GetString("ThreeColorScaleConditionalFormatRule_MedianValue_Property")),
				    new DisplayNameAttribute("MedianValue")				);


				tableBuilder.AddCustomAttributes(t, "MedianColor",
					new DescriptionAttribute(SR.GetString("ThreeColorScaleConditionalFormatRule_MedianColor_Property")),
				    new DisplayNameAttribute("MedianColor")				);

				#endregion // ThreeColorScaleConditionalFormatRule Properties

				#region TwoColorScaleConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.TwoColorScaleConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MinimumColor",
					new DescriptionAttribute(SR.GetString("TwoColorScaleConditionalFormatRule_MinimumColor_Property")),
				    new DisplayNameAttribute("MinimumColor")				);


				tableBuilder.AddCustomAttributes(t, "MaximumColor",
					new DescriptionAttribute(SR.GetString("TwoColorScaleConditionalFormatRule_MaximumColor_Property")),
				    new DisplayNameAttribute("MaximumColor")				);

				#endregion // TwoColorScaleConditionalFormatRule Properties

				#region TwoInputConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.TwoInputConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MinimumValue",
					new DescriptionAttribute(SR.GetString("TwoInputConditionalFormatRule_MinimumValue_Property")),
				    new DisplayNameAttribute("MinimumValue")				);


				tableBuilder.AddCustomAttributes(t, "MaximumValue",
					new DescriptionAttribute(SR.GetString("TwoInputConditionalFormatRule_MaximumValue_Property")),
				    new DisplayNameAttribute("MaximumValue")				);


				tableBuilder.AddCustomAttributes(t, "MinimumValueType",
					new DescriptionAttribute(SR.GetString("TwoInputConditionalFormatRule_MinimumValueType_Property")),
				    new DisplayNameAttribute("MinimumValueType")				);


				tableBuilder.AddCustomAttributes(t, "MaximumValueType",
					new DescriptionAttribute(SR.GetString("TwoInputConditionalFormatRule_MaximumValueType_Property")),
				    new DisplayNameAttribute("MaximumValueType")				);

				#endregion // TwoInputConditionalFormatRule Properties

				#region ThreeColorScaleConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ThreeColorScaleConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ThreeColorScaleConditionalFormatRuleProxy Properties

				#region TwoColorScaleConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.TwoColorScaleConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TwoColorScaleConditionalFormatRuleProxy Properties

				#region TwoInputConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.TwoInputConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TwoInputConditionalFormatRuleProxy Properties

				#region Animation Properties
				t = controlAssembly.GetType("Infragistics.Animation");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Duration",
					new DescriptionAttribute(SR.GetString("Animation_Duration_Property")),
				    new DisplayNameAttribute("Duration")				);


				tableBuilder.AddCustomAttributes(t, "IsPlaying",
					new DescriptionAttribute(SR.GetString("Animation_IsPlaying_Property")),
				    new DisplayNameAttribute("IsPlaying")				);


				tableBuilder.AddCustomAttributes(t, "Time",
					new DescriptionAttribute(SR.GetString("Animation_Time_Property")),
				    new DisplayNameAttribute("Time")				);


				tableBuilder.AddCustomAttributes(t, "EasingFunction",
					new DescriptionAttribute(SR.GetString("Animation_EasingFunction_Property")),
				    new DisplayNameAttribute("EasingFunction")				);

				#endregion // Animation Properties

				#region AnimationEventArgs Properties
				t = controlAssembly.GetType("Infragistics.AnimationEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("AnimationEventArgs_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // AnimationEventArgs Properties

				#region XamGridRowsManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridRowsManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "GroupByAreaRow",
					new DescriptionAttribute(SR.GetString("XamGridRowsManager_GroupByAreaRow_Property")),
				    new DisplayNameAttribute("GroupByAreaRow"),
					new CategoryAttribute(SR.GetString("XamGridRowsManager_Properties"))
				);

				#endregion // XamGridRowsManager Properties

				#region RowExpandCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowExpandCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowExpandCommand Properties

				#region RowCollapseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowCollapseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowCollapseCommand Properties

				#region RowDeleteCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowDeleteCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowDeleteCommand Properties

				#region RowEditCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowEditCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowEditCommand Properties

				#region GroupColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GroupColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsFilterable",
					new DescriptionAttribute(SR.GetString("GroupColumn_IsFilterable_Property")),
				    new DisplayNameAttribute("IsFilterable")				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("GroupColumn_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);


				tableBuilder.AddCustomAttributes(t, "IsSummable",
					new DescriptionAttribute(SR.GetString("GroupColumn_IsSummable_Property")),
				    new DisplayNameAttribute("IsSummable")				);


				tableBuilder.AddCustomAttributes(t, "IsGroupable",
					new DescriptionAttribute(SR.GetString("GroupColumn_IsGroupable_Property")),
				    new DisplayNameAttribute("IsGroupable")				);


				tableBuilder.AddCustomAttributes(t, "AllColumns",
					new DescriptionAttribute(SR.GetString("GroupColumn_AllColumns_Property")),
				    new DisplayNameAttribute("AllColumns")				);


				tableBuilder.AddCustomAttributes(t, "AllVisibleChildColumns",
					new DescriptionAttribute(SR.GetString("GroupColumn_AllVisibleChildColumns_Property")),
				    new DisplayNameAttribute("AllVisibleChildColumns")				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("GroupColumn_HeaderStyleResolved_Property")),
				    new DisplayNameAttribute("HeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "Columns",
					new DescriptionAttribute(SR.GetString("GroupColumn_Columns_Property")),
				    new DisplayNameAttribute("Columns")				);

				#endregion // GroupColumn Properties

				#region FillerColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FillerColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "WidthResolved",
					new DescriptionAttribute(SR.GetString("FillerColumn_WidthResolved_Property")),
				    new DisplayNameAttribute("WidthResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("FillerColumn_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);


				tableBuilder.AddCustomAttributes(t, "IsFixable",
					new DescriptionAttribute(SR.GetString("FillerColumn_IsFixable_Property")),
				    new DisplayNameAttribute("IsFixable")				);


				tableBuilder.AddCustomAttributes(t, "IsFilterable",
					new DescriptionAttribute(SR.GetString("FillerColumn_IsFilterable_Property")),
				    new DisplayNameAttribute("IsFilterable")				);


				tableBuilder.AddCustomAttributes(t, "IsMovable",
					new DescriptionAttribute(SR.GetString("FillerColumn_IsMovable_Property")),
				    new DisplayNameAttribute("IsMovable")				);


				tableBuilder.AddCustomAttributes(t, "IsResizable",
					new DescriptionAttribute(SR.GetString("FillerColumn_IsResizable_Property")),
				    new DisplayNameAttribute("IsResizable")				);


				tableBuilder.AddCustomAttributes(t, "IsSummable",
					new DescriptionAttribute(SR.GetString("FillerColumn_IsSummable_Property")),
				    new DisplayNameAttribute("IsSummable")				);


				tableBuilder.AddCustomAttributes(t, "CellStyleResolved",
					new DescriptionAttribute(SR.GetString("FillerColumn_CellStyleResolved_Property")),
				    new DisplayNameAttribute("CellStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("FillerColumn_HeaderStyleResolved_Property")),
				    new DisplayNameAttribute("HeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyleResolved",
					new DescriptionAttribute(SR.GetString("FillerColumn_FooterStyleResolved_Property")),
				    new DisplayNameAttribute("FooterStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsGroupable",
					new DescriptionAttribute(SR.GetString("FillerColumn_IsGroupable_Property")),
				    new DisplayNameAttribute("IsGroupable")				);


				tableBuilder.AddCustomAttributes(t, "IsHideable",
					new DescriptionAttribute(SR.GetString("FillerColumn_IsHideable_Property")),
				    new DisplayNameAttribute("IsHideable")				);

				#endregion // FillerColumn Properties

				#region ExpansionIndicatorColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ExpansionIndicatorColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorColumn_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);


				tableBuilder.AddCustomAttributes(t, "IsFixable",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorColumn_IsFixable_Property")),
				    new DisplayNameAttribute("IsFixable")				);


				tableBuilder.AddCustomAttributes(t, "IsMovable",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorColumn_IsMovable_Property")),
				    new DisplayNameAttribute("IsMovable")				);


				tableBuilder.AddCustomAttributes(t, "IsResizable",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorColumn_IsResizable_Property")),
				    new DisplayNameAttribute("IsResizable")				);


				tableBuilder.AddCustomAttributes(t, "WidthResolved",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorColumn_WidthResolved_Property")),
				    new DisplayNameAttribute("WidthResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsSummable",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorColumn_IsSummable_Property")),
				    new DisplayNameAttribute("IsSummable")				);


				tableBuilder.AddCustomAttributes(t, "IsGroupable",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorColumn_IsGroupable_Property")),
				    new DisplayNameAttribute("IsGroupable")				);

				#endregion // ExpansionIndicatorColumn Properties

				#region GroupByColumnLayoutHeaderCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByColumnLayoutHeaderCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupByColumnLayoutHeaderCell Properties

				#region GroupByHeaderCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByHeaderCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupByHeaderCell Properties

				#region FixedBorderHeaderCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixedBorderHeaderCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FixedBorderHeaderCell Properties

				#region SummaryResultFormatStringValueConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SummaryResultFormatStringValueConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SummaryResultFormatStringValueConverter Properties

				#region SortingSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SortingSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowSorting",
					new DescriptionAttribute(SR.GetString("SortingSettings_AllowSorting_Property")),
				    new DisplayNameAttribute("AllowSorting")				);


				tableBuilder.AddCustomAttributes(t, "ShowSortIndicator",
					new DescriptionAttribute(SR.GetString("SortingSettings_ShowSortIndicator_Property")),
				    new DisplayNameAttribute("ShowSortIndicator")				);


				tableBuilder.AddCustomAttributes(t, "AllowMultipleColumnSorting",
					new DescriptionAttribute(SR.GetString("SortingSettings_AllowMultipleColumnSorting_Property")),
				    new DisplayNameAttribute("AllowMultipleColumnSorting")				);


				tableBuilder.AddCustomAttributes(t, "SortedColumns",
					new DescriptionAttribute(SR.GetString("SortingSettings_SortedColumns_Property")),
				    new DisplayNameAttribute("SortedColumns")				);


				tableBuilder.AddCustomAttributes(t, "MultiSortingKey",
					new DescriptionAttribute(SR.GetString("SortingSettings_MultiSortingKey_Property")),
				    new DisplayNameAttribute("MultiSortingKey")				);


				tableBuilder.AddCustomAttributes(t, "FirstSortDirection",
					new DescriptionAttribute(SR.GetString("SortingSettings_FirstSortDirection_Property")),
				    new DisplayNameAttribute("FirstSortDirection")				);

				#endregion // SortingSettings Properties

				#region GroupByMovingIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByMovingIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupByMovingIndicator Properties

				#region MovingIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.MovingIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalOffset",
					new DescriptionAttribute(SR.GetString("MovingIndicator_HorizontalOffset_Property")),
				    new DisplayNameAttribute("HorizontalOffset")				);

				#endregion // MovingIndicator Properties

				#region FixedColumnSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FixedColumnSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowFixedColumns",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_AllowFixedColumns_Property")),
				    new DisplayNameAttribute("AllowFixedColumns")				);


				tableBuilder.AddCustomAttributes(t, "AllowFixedColumnsResolved",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_AllowFixedColumnsResolved_Property")),
				    new DisplayNameAttribute("AllowFixedColumnsResolved")				);


				tableBuilder.AddCustomAttributes(t, "FixedIndicatorDirection",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedIndicatorDirection_Property")),
				    new DisplayNameAttribute("FixedIndicatorDirection")				);


				tableBuilder.AddCustomAttributes(t, "FixedIndicatorDirectionResolved",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedIndicatorDirectionResolved_Property")),
				    new DisplayNameAttribute("FixedIndicatorDirectionResolved")				);


				tableBuilder.AddCustomAttributes(t, "FixedDropAreaLocation",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedDropAreaLocation_Property")),
				    new DisplayNameAttribute("FixedDropAreaLocation")				);


				tableBuilder.AddCustomAttributes(t, "FixedDropAreaLocationResolved",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedDropAreaLocationResolved_Property")),
				    new DisplayNameAttribute("FixedDropAreaLocationResolved")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderStyle",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedBorderStyle_Property")),
				    new DisplayNameAttribute("FixedBorderStyle")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderStyleResolved",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedBorderStyleResolved_Property")),
				    new DisplayNameAttribute("FixedBorderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderHeaderStyle",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedBorderHeaderStyle_Property")),
				    new DisplayNameAttribute("FixedBorderHeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderHeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedBorderHeaderStyleResolved_Property")),
				    new DisplayNameAttribute("FixedBorderHeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderFooterStyle",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedBorderFooterStyle_Property")),
				    new DisplayNameAttribute("FixedBorderFooterStyle")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderFooterStyleResolved",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedBorderFooterStyleResolved_Property")),
				    new DisplayNameAttribute("FixedBorderFooterStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FixedDropAreaLeftStyle",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedDropAreaLeftStyle_Property")),
				    new DisplayNameAttribute("FixedDropAreaLeftStyle")				);


				tableBuilder.AddCustomAttributes(t, "FixedDropAreaLeftStyleResolved",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedDropAreaLeftStyleResolved_Property")),
				    new DisplayNameAttribute("FixedDropAreaLeftStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FixedDropAreaRightStyle",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedDropAreaRightStyle_Property")),
				    new DisplayNameAttribute("FixedDropAreaRightStyle")				);


				tableBuilder.AddCustomAttributes(t, "FixedDropAreaRightStyleResolved",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedDropAreaRightStyleResolved_Property")),
				    new DisplayNameAttribute("FixedDropAreaRightStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FixedColumnsLeft",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedColumnsLeft_Property")),
				    new DisplayNameAttribute("FixedColumnsLeft")				);


				tableBuilder.AddCustomAttributes(t, "FixedColumnsRight",
					new DescriptionAttribute(SR.GetString("FixedColumnSettingsOverride_FixedColumnsRight_Property")),
				    new DisplayNameAttribute("FixedColumnsRight")				);

				#endregion // FixedColumnSettingsOverride Properties

				#region FilterSelectorControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterSelectorControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("FilterSelectorControl_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);

				#endregion // FilterSelectorControl Properties

				#region FilterOperandCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FilterOperandCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FilterOperandCollection Properties

				#region DateFilterSelectionControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.DateFilterSelectionControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DateFilterHierarchicalDataTemplate",
					new DescriptionAttribute(SR.GetString("DateFilterSelectionControl_DateFilterHierarchicalDataTemplate_Property")),
				    new DisplayNameAttribute("DateFilterHierarchicalDataTemplate")				);


				tableBuilder.AddCustomAttributes(t, "DateFilterObjectTypeItem",
					new DescriptionAttribute(SR.GetString("DateFilterSelectionControl_DateFilterObjectTypeItem_Property")),
				    new DisplayNameAttribute("DateFilterObjectTypeItem")				);


				tableBuilder.AddCustomAttributes(t, "DateFilterTypeList",
					new DescriptionAttribute(SR.GetString("DateFilterSelectionControl_DateFilterTypeList_Property")),
				    new DisplayNameAttribute("DateFilterTypeList")				);


				tableBuilder.AddCustomAttributes(t, "FilterableUniqueValues",
					new DescriptionAttribute(SR.GetString("DateFilterSelectionControl_FilterableUniqueValues_Property")),
				    new DisplayNameAttribute("FilterableUniqueValues")				);

				#endregion // DateFilterSelectionControl Properties

				#region FilterSelectionControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterSelectionControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ClearFiltersText",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_ClearFiltersText_Property")),
				    new DisplayNameAttribute("ClearFiltersText")				);


				tableBuilder.AddCustomAttributes(t, "TypeSpecificFiltersText",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_TypeSpecificFiltersText_Property")),
				    new DisplayNameAttribute("TypeSpecificFiltersText")				);


				tableBuilder.AddCustomAttributes(t, "CancelButtonText",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_CancelButtonText_Property")),
				    new DisplayNameAttribute("CancelButtonText")				);


				tableBuilder.AddCustomAttributes(t, "OKButtonText",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_OKButtonText_Property")),
				    new DisplayNameAttribute("OKButtonText")				);


				tableBuilder.AddCustomAttributes(t, "UniqueValues",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_UniqueValues_Property")),
				    new DisplayNameAttribute("UniqueValues")				);


				tableBuilder.AddCustomAttributes(t, "ClearFiltersTextResolved",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_ClearFiltersTextResolved_Property")),
				    new DisplayNameAttribute("ClearFiltersTextResolved")				);


				tableBuilder.AddCustomAttributes(t, "TypeSpecificFiltersTextResolved",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_TypeSpecificFiltersTextResolved_Property")),
				    new DisplayNameAttribute("TypeSpecificFiltersTextResolved")				);


				tableBuilder.AddCustomAttributes(t, "FilterableUniqueValues",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_FilterableUniqueValues_Property")),
				    new DisplayNameAttribute("FilterableUniqueValues")				);


				tableBuilder.AddCustomAttributes(t, "FilterText",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_FilterText_Property")),
				    new DisplayNameAttribute("FilterText")				);


				tableBuilder.AddCustomAttributes(t, "FilterBoxNoDataAvailable",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_FilterBoxNoDataAvailable_Property")),
				    new DisplayNameAttribute("FilterBoxNoDataAvailable")				);


				tableBuilder.AddCustomAttributes(t, "AppendFilterText",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_AppendFilterText_Property")),
				    new DisplayNameAttribute("AppendFilterText")				);


				tableBuilder.AddCustomAttributes(t, "AppendFilters",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_AppendFilters_Property")),
				    new DisplayNameAttribute("AppendFilters")				);


				tableBuilder.AddCustomAttributes(t, "ItemSourceSet",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_ItemSourceSet_Property")),
				    new DisplayNameAttribute("ItemSourceSet")				);


				tableBuilder.AddCustomAttributes(t, "HasFilters",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_HasFilters_Property")),
				    new DisplayNameAttribute("HasFilters")				);


				tableBuilder.AddCustomAttributes(t, "HasCheckedItems",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_HasCheckedItems_Property")),
				    new DisplayNameAttribute("HasCheckedItems")				);


				tableBuilder.AddCustomAttributes(t, "BusyText",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_BusyText_Property")),
				    new DisplayNameAttribute("BusyText")				);


				tableBuilder.AddCustomAttributes(t, "NotAllItemsShowingText",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_NotAllItemsShowingText_Property")),
				    new DisplayNameAttribute("NotAllItemsShowingText")				);


				tableBuilder.AddCustomAttributes(t, "NotAllItemsShowingTooltipText",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_NotAllItemsShowingTooltipText_Property")),
				    new DisplayNameAttribute("NotAllItemsShowingTooltipText")				);


				tableBuilder.AddCustomAttributes(t, "CanFilter",
					new DescriptionAttribute(SR.GetString("FilterSelectionControl_CanFilter_Property")),
				    new DisplayNameAttribute("CanFilter")				);

				#endregion // FilterSelectionControl Properties

				#region SelectionControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SelectionControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("SelectionControl_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxStyle",
					new DescriptionAttribute(SR.GetString("SelectionControl_CheckBoxStyle_Property")),
				    new DisplayNameAttribute("CheckBoxStyle")				);

				#endregion // SelectionControl Properties

				#region FilterItemCollection`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterItemCollection`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("FilterItemCollection`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);

				#endregion // FilterItemCollection`1 Properties

				#region FilterItemCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterItemCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("FilterItemCollection_Count_Property")),
				    new DisplayNameAttribute("Count")				);

				#endregion // FilterItemCollection Properties

				#region DateFilterTreeView Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.DateFilterTreeView");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateFilterTreeView Properties

				#region DateFilterHierarchicalDataTemplate Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.DateFilterHierarchicalDataTemplate");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateFilterHierarchicalDataTemplate Properties

				#region DeferredScrollingSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DeferredScrollingSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DeferredScrollTemplate",
					new DescriptionAttribute(SR.GetString("DeferredScrollingSettingsOverride_DeferredScrollTemplate_Property")),
				    new DisplayNameAttribute("DeferredScrollTemplate")				);


				tableBuilder.AddCustomAttributes(t, "DeferredScrollTemplateResolved",
					new DescriptionAttribute(SR.GetString("DeferredScrollingSettingsOverride_DeferredScrollTemplateResolved_Property")),
				    new DisplayNameAttribute("DeferredScrollTemplateResolved")				);


				tableBuilder.AddCustomAttributes(t, "DefaultColumnKey",
					new DescriptionAttribute(SR.GetString("DeferredScrollingSettingsOverride_DefaultColumnKey_Property")),
				    new DisplayNameAttribute("DefaultColumnKey")				);


				tableBuilder.AddCustomAttributes(t, "DefaultColumnKeyResolved",
					new DescriptionAttribute(SR.GetString("DeferredScrollingSettingsOverride_DefaultColumnKeyResolved_Property")),
				    new DisplayNameAttribute("DefaultColumnKeyResolved")				);


				tableBuilder.AddCustomAttributes(t, "GroupByDeferredScrollTemplate",
					new DescriptionAttribute(SR.GetString("DeferredScrollingSettingsOverride_GroupByDeferredScrollTemplate_Property")),
				    new DisplayNameAttribute("GroupByDeferredScrollTemplate")				);


				tableBuilder.AddCustomAttributes(t, "GroupByDeferredScrollTemplateResolved",
					new DescriptionAttribute(SR.GetString("DeferredScrollingSettingsOverride_GroupByDeferredScrollTemplateResolved_Property")),
				    new DisplayNameAttribute("GroupByDeferredScrollTemplateResolved")				);

				#endregion // DeferredScrollingSettingsOverride Properties

				#region ConditionalFormattingSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ConditionalFormattingSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ConditionalFormattingSettingsOverride Properties

				#region ClipboardSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ClipboardSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowCopy",
					new DescriptionAttribute(SR.GetString("ClipboardSettings_AllowCopy_Property")),
				    new DisplayNameAttribute("AllowCopy")				);


				tableBuilder.AddCustomAttributes(t, "CopyOptions",
					new DescriptionAttribute(SR.GetString("ClipboardSettings_CopyOptions_Property")),
				    new DisplayNameAttribute("CopyOptions")				);


				tableBuilder.AddCustomAttributes(t, "CopyType",
					new DescriptionAttribute(SR.GetString("ClipboardSettings_CopyType_Property")),
				    new DisplayNameAttribute("CopyType")				);


				tableBuilder.AddCustomAttributes(t, "AllowPaste",
					new DescriptionAttribute(SR.GetString("ClipboardSettings_AllowPaste_Property")),
				    new DisplayNameAttribute("AllowPaste")				);

				#endregion // ClipboardSettings Properties

				#region TextColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.TextColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TextBlockStyle",
					new DescriptionAttribute(SR.GetString("TextColumn_TextBlockStyle_Property")),
				    new DisplayNameAttribute("TextBlockStyle")				);


				tableBuilder.AddCustomAttributes(t, "TextWrapping",
					new DescriptionAttribute(SR.GetString("TextColumn_TextWrapping_Property")),
				    new DisplayNameAttribute("TextWrapping")				);


				tableBuilder.AddCustomAttributes(t, "FormatString",
					new DescriptionAttribute(SR.GetString("TextColumn_FormatString_Property")),
				    new DisplayNameAttribute("FormatString")				);

				#endregion // TextColumn Properties

				#region SingleCellBaseCollection`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SingleCellBaseCollection`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SingleCellBaseCollection`1 Properties

				#region CellBaseCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CellBaseCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CellBaseCollection Properties

				#region PagerCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagerCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // PagerCellControl Properties

				#region ExpansionIndicatorFooterCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ExpansionIndicatorFooterCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ExpansionIndicatorFooterCell Properties

				#region ConditionalFormattingUnboundCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ConditionalFormattingUnboundCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ConditionalFormattingUnboundCell Properties

				#region UnboundCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.UnboundCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("UnboundCell_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // UnboundCell Properties

				#region AddNewRowExpansionIndicatorCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.AddNewRowExpansionIndicatorCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // AddNewRowExpansionIndicatorCellControl Properties

				#region VisibilityToBoolValueConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.VisibilityToBoolValueConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // VisibilityToBoolValueConverter Properties

				#region SortedColumnsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SortedColumnsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SortedColumnsCollection Properties

				#region GroupByAreaCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByAreaCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupByAreaCommandBase Properties

				#region ToggleGroupByAreaCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ToggleGroupByAreaCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ToggleGroupByAreaCommand Properties

				#region ExpandGroupByAreaCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ExpandGroupByAreaCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ExpandGroupByAreaCommand Properties

				#region CollapseGroupByAreaCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CollapseGroupByAreaCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CollapseGroupByAreaCommand Properties

				#region DropAreaIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.DropAreaIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentState",
					new DescriptionAttribute(SR.GetString("DropAreaIndicator_CurrentState_Property")),
				    new DisplayNameAttribute("CurrentState")				);

				#endregion // DropAreaIndicator Properties

				#region LessThanConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.LessThanConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LessThanConditionalFormatRule Properties

				#region LessThanConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.LessThanConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LessThanConditionalFormatRuleProxy Properties

				#region BetweenXandYConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.BetweenXandYConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LowerBound",
					new DescriptionAttribute(SR.GetString("BetweenXandYConditionalFormatRule_LowerBound_Property")),
				    new DisplayNameAttribute("LowerBound")				);


				tableBuilder.AddCustomAttributes(t, "UpperBound",
					new DescriptionAttribute(SR.GetString("BetweenXandYConditionalFormatRule_UpperBound_Property")),
				    new DisplayNameAttribute("UpperBound")				);


				tableBuilder.AddCustomAttributes(t, "IsInclusive",
					new DescriptionAttribute(SR.GetString("BetweenXandYConditionalFormatRule_IsInclusive_Property")),
				    new DisplayNameAttribute("IsInclusive")				);

				#endregion // BetweenXandYConditionalFormatRule Properties

				#region BetweenXandYConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.BetweenXandYConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // BetweenXandYConditionalFormatRuleProxy Properties

				#region ColumnChooserCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnChooserCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnChooserCommandBase Properties

				#region ColumnChooserCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnChooserCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnChooserCloseCommand Properties

				#region ColumnChooserOpenCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnChooserOpenCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnChooserOpenCommand Properties

				#region XamGridColumnChooserCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridColumnChooserCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamGridColumnChooserCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamGridColumnChooserCommandSource_Properties"))
				);

				#endregion // XamGridColumnChooserCommandSource Properties

				#region HideColumnCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.HideColumnCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HideColumnCommand Properties

				#region ShowColumnCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ShowColumnCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ShowColumnCommand Properties

				#region RowSelectorColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowSelectorColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("RowSelectorColumn_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);


				tableBuilder.AddCustomAttributes(t, "IsFixable",
					new DescriptionAttribute(SR.GetString("RowSelectorColumn_IsFixable_Property")),
				    new DisplayNameAttribute("IsFixable")				);


				tableBuilder.AddCustomAttributes(t, "IsMovable",
					new DescriptionAttribute(SR.GetString("RowSelectorColumn_IsMovable_Property")),
				    new DisplayNameAttribute("IsMovable")				);


				tableBuilder.AddCustomAttributes(t, "IsResizable",
					new DescriptionAttribute(SR.GetString("RowSelectorColumn_IsResizable_Property")),
				    new DisplayNameAttribute("IsResizable")				);


				tableBuilder.AddCustomAttributes(t, "WidthResolved",
					new DescriptionAttribute(SR.GetString("RowSelectorColumn_WidthResolved_Property")),
				    new DisplayNameAttribute("WidthResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsSummable",
					new DescriptionAttribute(SR.GetString("RowSelectorColumn_IsSummable_Property")),
				    new DisplayNameAttribute("IsSummable")				);


				tableBuilder.AddCustomAttributes(t, "IsGroupable",
					new DescriptionAttribute(SR.GetString("RowSelectorColumn_IsGroupable_Property")),
				    new DisplayNameAttribute("IsGroupable")				);

				#endregion // RowSelectorColumn Properties

				#region HyperlinkColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.HyperlinkColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HyperlinkColumnContentProvider Properties

				#region HyperlinkColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.HyperlinkColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Content",
					new DescriptionAttribute(SR.GetString("HyperlinkColumn_Content_Property")),
				    new DisplayNameAttribute("Content")				);


				tableBuilder.AddCustomAttributes(t, "ContentBinding",
					new DescriptionAttribute(SR.GetString("HyperlinkColumn_ContentBinding_Property")),
				    new DisplayNameAttribute("ContentBinding")				);


				tableBuilder.AddCustomAttributes(t, "TargetName",
					new DescriptionAttribute(SR.GetString("HyperlinkColumn_TargetName_Property")),
				    new DisplayNameAttribute("TargetName")				);


				tableBuilder.AddCustomAttributes(t, "HyperlinkButtonStyle",
					new DescriptionAttribute(SR.GetString("HyperlinkColumn_HyperlinkButtonStyle_Property")),
				    new DisplayNameAttribute("HyperlinkButtonStyle")				);


				tableBuilder.AddCustomAttributes(t, "DataType",
					new DescriptionAttribute(SR.GetString("HyperlinkColumn_DataType_Property")),
				    new DisplayNameAttribute("DataType")				);

				#endregion // HyperlinkColumn Properties

				#region GroupFooterCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupFooterCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupFooterCell Properties

				#region FooterCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FooterCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FooterCell Properties

				#region GroupCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupCell Properties

				#region GroupByCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupByCell Properties

				#region ColumnLayoutTemplateCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ColumnLayoutTemplateCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // ColumnLayoutTemplateCellControl Properties

				#region XamGridRenderAdorner Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.XamGridRenderAdorner");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "XamGrid",
					new DescriptionAttribute(SR.GetString("XamGridRenderAdorner_XamGrid_Property")),
				    new DisplayNameAttribute("XamGrid"),
					new CategoryAttribute(SR.GetString("XamGridRenderAdorner_Properties"))
				);

				#endregion // XamGridRenderAdorner Properties

				#region SortingSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SortingSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowSorting",
					new DescriptionAttribute(SR.GetString("SortingSettingsOverride_AllowSorting_Property")),
				    new DisplayNameAttribute("AllowSorting")				);


				tableBuilder.AddCustomAttributes(t, "AllowSortingResolved",
					new DescriptionAttribute(SR.GetString("SortingSettingsOverride_AllowSortingResolved_Property")),
				    new DisplayNameAttribute("AllowSortingResolved")				);


				tableBuilder.AddCustomAttributes(t, "ShowSortIndicator",
					new DescriptionAttribute(SR.GetString("SortingSettingsOverride_ShowSortIndicator_Property")),
				    new DisplayNameAttribute("ShowSortIndicator")				);


				tableBuilder.AddCustomAttributes(t, "ShowSortIndicatorResolved",
					new DescriptionAttribute(SR.GetString("SortingSettingsOverride_ShowSortIndicatorResolved_Property")),
				    new DisplayNameAttribute("ShowSortIndicatorResolved")				);


				tableBuilder.AddCustomAttributes(t, "AllowMultipleColumnSorting",
					new DescriptionAttribute(SR.GetString("SortingSettingsOverride_AllowMultipleColumnSorting_Property")),
				    new DisplayNameAttribute("AllowMultipleColumnSorting")				);


				tableBuilder.AddCustomAttributes(t, "AllowMultipleColumnSortingResolved",
					new DescriptionAttribute(SR.GetString("SortingSettingsOverride_AllowMultipleColumnSortingResolved_Property")),
				    new DisplayNameAttribute("AllowMultipleColumnSortingResolved")				);


				tableBuilder.AddCustomAttributes(t, "SortedColumns",
					new DescriptionAttribute(SR.GetString("SortingSettingsOverride_SortedColumns_Property")),
				    new DisplayNameAttribute("SortedColumns")				);


				tableBuilder.AddCustomAttributes(t, "FirstSortDirection",
					new DescriptionAttribute(SR.GetString("SortingSettingsOverride_FirstSortDirection_Property")),
				    new DisplayNameAttribute("FirstSortDirection")				);


				tableBuilder.AddCustomAttributes(t, "FirstSortDirectionResolved",
					new DescriptionAttribute(SR.GetString("SortingSettingsOverride_FirstSortDirectionResolved_Property")),
				    new DisplayNameAttribute("FirstSortDirectionResolved")				);

				#endregion // SortingSettingsOverride Properties

				#region FillerColumnSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FillerColumnSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyle",
					new DescriptionAttribute(SR.GetString("FillerColumnSettingsOverride_HeaderStyle_Property")),
				    new DisplayNameAttribute("HeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("FillerColumnSettingsOverride_HeaderStyleResolved_Property")),
				    new DisplayNameAttribute("HeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyle",
					new DescriptionAttribute(SR.GetString("FillerColumnSettingsOverride_FooterStyle_Property")),
				    new DisplayNameAttribute("FooterStyle")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyleResolved",
					new DescriptionAttribute(SR.GetString("FillerColumnSettingsOverride_FooterStyleResolved_Property")),
				    new DisplayNameAttribute("FooterStyleResolved")				);

				#endregion // FillerColumnSettingsOverride Properties

				#region LessThanOrEqualToConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.LessThanOrEqualToConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LessThanOrEqualToConditionalFormatRule Properties

				#region LessThanOrEqualToConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.LessThanOrEqualToConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LessThanOrEqualToConditionalFormatRuleProxy Properties

				#region ColumnMovingSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnMovingSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IndicatorStyle",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettings_IndicatorStyle_Property")),
				    new DisplayNameAttribute("IndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "AllowColumnMoving",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettings_AllowColumnMoving_Property")),
				    new DisplayNameAttribute("AllowColumnMoving")				);


				tableBuilder.AddCustomAttributes(t, "EasingFunction",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettings_EasingFunction_Property")),
				    new DisplayNameAttribute("EasingFunction")				);


				tableBuilder.AddCustomAttributes(t, "AnimationDuration",
					new DescriptionAttribute(SR.GetString("ColumnMovingSettings_AnimationDuration_Property")),
				    new DisplayNameAttribute("AnimationDuration")				);

				#endregion // ColumnMovingSettings Properties

				#region ColumnLayoutTemplateRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ColumnLayoutTemplateRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cells",
					new DescriptionAttribute(SR.GetString("ColumnLayoutTemplateRow_Cells_Property")),
				    new DisplayNameAttribute("Cells")				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("ColumnLayoutTemplateRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);

				#endregion // ColumnLayoutTemplateRow Properties

				#region ReadOnlyKeyedColumnBaseCollection`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ReadOnlyKeyedColumnBaseCollection`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ReadOnlyKeyedColumnBaseCollection`1 Properties

				#region RowSelectorHeaderCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowSelectorHeaderCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowSelectorHeaderCell Properties

				#region AddNewRowCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.AddNewRowCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // AddNewRowCellControl Properties

				#region GridSortableListBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GridSortableListBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("GridSortableListBox_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);

				#endregion // GridSortableListBox Properties

				#region GridSortableListBoxItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GridSortableListBoxItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsMovable",
					new DescriptionAttribute(SR.GetString("GridSortableListBoxItem_IsMovable_Property")),
				    new DisplayNameAttribute("IsMovable")				);

				#endregion // GridSortableListBoxItem Properties

				#region SummarySelectionControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SummarySelectionControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OKCaption",
					new DescriptionAttribute(SR.GetString("SummarySelectionControl_OKCaption_Property")),
				    new DisplayNameAttribute("OKCaption")				);


				tableBuilder.AddCustomAttributes(t, "CancelCaption",
					new DescriptionAttribute(SR.GetString("SummarySelectionControl_CancelCaption_Property")),
				    new DisplayNameAttribute("CancelCaption")				);

				#endregion // SummarySelectionControl Properties

				#region SummarySelectionControlCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SummarySelectionControlCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("SummarySelectionControlCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // SummarySelectionControlCommandSource Properties

				#region AcceptChangesCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.AcceptChangesCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AcceptChangesCommand Properties

				#region RowSelectorSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowSelectorSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyle",
					new DescriptionAttribute(SR.GetString("RowSelectorSettingsOverride_HeaderStyle_Property")),
				    new DisplayNameAttribute("HeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("RowSelectorSettingsOverride_HeaderStyleResolved_Property")),
				    new DisplayNameAttribute("HeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyle",
					new DescriptionAttribute(SR.GetString("RowSelectorSettingsOverride_FooterStyle_Property")),
				    new DisplayNameAttribute("FooterStyle")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyleResolved",
					new DescriptionAttribute(SR.GetString("RowSelectorSettingsOverride_FooterStyleResolved_Property")),
				    new DisplayNameAttribute("FooterStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "RowNumberingSeed",
					new DescriptionAttribute(SR.GetString("RowSelectorSettingsOverride_RowNumberingSeed_Property")),
				    new DisplayNameAttribute("RowNumberingSeed")				);


				tableBuilder.AddCustomAttributes(t, "RowNumberingSeedResolved",
					new DescriptionAttribute(SR.GetString("RowSelectorSettingsOverride_RowNumberingSeedResolved_Property")),
				    new DisplayNameAttribute("RowNumberingSeedResolved")				);


				tableBuilder.AddCustomAttributes(t, "EnableRowNumbering",
					new DescriptionAttribute(SR.GetString("RowSelectorSettingsOverride_EnableRowNumbering_Property")),
				    new DisplayNameAttribute("EnableRowNumbering")				);


				tableBuilder.AddCustomAttributes(t, "EnableRowNumberingResolved",
					new DescriptionAttribute(SR.GetString("RowSelectorSettingsOverride_EnableRowNumberingResolved_Property")),
				    new DisplayNameAttribute("EnableRowNumberingResolved")				);

				#endregion // RowSelectorSettingsOverride Properties

				#region MergedCellsRenderAdorner Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.MergedCellsRenderAdorner");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MergedCellsRenderAdorner Properties

				#region XamGroupByAreaCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGroupByAreaCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamGroupByAreaCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamGroupByAreaCommandSource_Properties"))
				);

				#endregion // XamGroupByAreaCommandSource Properties

				#region FixedColumnSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FixedColumnSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowFixedColumns",
					new DescriptionAttribute(SR.GetString("FixedColumnSettings_AllowFixedColumns_Property")),
				    new DisplayNameAttribute("AllowFixedColumns")				);


				tableBuilder.AddCustomAttributes(t, "FixedIndicatorDirection",
					new DescriptionAttribute(SR.GetString("FixedColumnSettings_FixedIndicatorDirection_Property")),
				    new DisplayNameAttribute("FixedIndicatorDirection")				);


				tableBuilder.AddCustomAttributes(t, "FixedDropAreaLocation",
					new DescriptionAttribute(SR.GetString("FixedColumnSettings_FixedDropAreaLocation_Property")),
				    new DisplayNameAttribute("FixedDropAreaLocation")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderStyle",
					new DescriptionAttribute(SR.GetString("FixedColumnSettings_FixedBorderStyle_Property")),
				    new DisplayNameAttribute("FixedBorderStyle")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderHeaderStyle",
					new DescriptionAttribute(SR.GetString("FixedColumnSettings_FixedBorderHeaderStyle_Property")),
				    new DisplayNameAttribute("FixedBorderHeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "FixedBorderFooterStyle",
					new DescriptionAttribute(SR.GetString("FixedColumnSettings_FixedBorderFooterStyle_Property")),
				    new DisplayNameAttribute("FixedBorderFooterStyle")				);


				tableBuilder.AddCustomAttributes(t, "FixedDropAreaLeftStyle",
					new DescriptionAttribute(SR.GetString("FixedColumnSettings_FixedDropAreaLeftStyle_Property")),
				    new DisplayNameAttribute("FixedDropAreaLeftStyle")				);


				tableBuilder.AddCustomAttributes(t, "FixedDropAreaRightStyle",
					new DescriptionAttribute(SR.GetString("FixedColumnSettings_FixedDropAreaRightStyle_Property")),
				    new DisplayNameAttribute("FixedDropAreaRightStyle")				);


				tableBuilder.AddCustomAttributes(t, "FixedColumnsLeft",
					new DescriptionAttribute(SR.GetString("FixedColumnSettings_FixedColumnsLeft_Property")),
				    new DisplayNameAttribute("FixedColumnsLeft")				);


				tableBuilder.AddCustomAttributes(t, "FixedColumnsRight",
					new DescriptionAttribute(SR.GetString("FixedColumnSettings_FixedColumnsRight_Property")),
				    new DisplayNameAttribute("FixedColumnsRight")				);

				#endregion // FixedColumnSettings Properties

				#region DateTimeNoInputBaseFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNoInputBaseFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RequiresFilteringInput",
					new DescriptionAttribute(SR.GetString("DateTimeNoInputBaseFilterOperand_RequiresFilteringInput_Property")),
				    new DisplayNameAttribute("RequiresFilteringInput")				);

				#endregion // DateTimeNoInputBaseFilterOperand Properties

				#region FilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayName",
					new DescriptionAttribute(SR.GetString("FilterOperand_DisplayName_Property")),
				    new DisplayNameAttribute("DisplayName")				);


				tableBuilder.AddCustomAttributes(t, "Icon",
					new DescriptionAttribute(SR.GetString("FilterOperand_Icon_Property")),
				    new DisplayNameAttribute("Icon")				);


				tableBuilder.AddCustomAttributes(t, "IconResolved",
					new DescriptionAttribute(SR.GetString("FilterOperand_IconResolved_Property")),
				    new DisplayNameAttribute("IconResolved")				);


				tableBuilder.AddCustomAttributes(t, "RequiresFilteringInput",
					new DescriptionAttribute(SR.GetString("FilterOperand_RequiresFilteringInput_Property")),
				    new DisplayNameAttribute("RequiresFilteringInput")				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("FilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // FilterOperand Properties

				#region DateTimeAfterFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeAfterFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeAfterFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeAfterFilterOperand Properties

				#region DateTimeBeforeFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeBeforeFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeBeforeFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeBeforeFilterOperand Properties

				#region DateTimeTodayFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeTodayFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeTodayFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeTodayFilterOperand Properties

				#region DateTimeTomorrowFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeTomorrowFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeTomorrowFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeTomorrowFilterOperand Properties

				#region DateTimeYesterdayFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeYesterdayFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeYesterdayFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeYesterdayFilterOperand Properties

				#region DateTimeNextWeekFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextWeekFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeNextWeekFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeNextWeekFilterOperand Properties

				#region DateTimeThisWeekFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisWeekFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeThisWeekFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeThisWeekFilterOperand Properties

				#region DateTimeLastWeekFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastWeekFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeLastWeekFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeLastWeekFilterOperand Properties

				#region DateTimeNextMonthFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextMonthFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeNextMonthFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeNextMonthFilterOperand Properties

				#region DateTimeThisMonthFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisMonthFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeThisMonthFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeThisMonthFilterOperand Properties

				#region DateTimeLastMonthFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastMonthFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeLastMonthFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeLastMonthFilterOperand Properties

				#region DateTimeNextQuarterFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextQuarterFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeNextQuarterFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeNextQuarterFilterOperand Properties

				#region DateTimeThisQuarterFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisQuarterFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeThisQuarterFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeThisQuarterFilterOperand Properties

				#region DateTimeLastQuarterFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastQuarterFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeLastQuarterFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeLastQuarterFilterOperand Properties

				#region DateTimeNextYearFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNextYearFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeNextYearFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeNextYearFilterOperand Properties

				#region DateTimeThisYearFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeThisYearFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeThisYearFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeThisYearFilterOperand Properties

				#region DateTimeLastYearFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeLastYearFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeLastYearFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeLastYearFilterOperand Properties

				#region DateTimeYearToDateFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeYearToDateFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeYearToDateFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeYearToDateFilterOperand Properties

				#region MinimumValueConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.MinimumValueConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MinimumValueConditionalFormatRule Properties

				#region MinimumValueConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.MinimumValueConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MinimumValueConditionalFormatRuleProxy Properties

				#region IconConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.IconConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Rules",
					new DescriptionAttribute(SR.GetString("IconConditionalFormatRule_Rules_Property")),
				    new DisplayNameAttribute("Rules")				);

				#endregion // IconConditionalFormatRule Properties

				#region IconConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.IconConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // IconConditionalFormatRuleProxy Properties

				#region DataBarConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DataBarConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DataBarDirection",
					new DescriptionAttribute(SR.GetString("DataBarConditionalFormatRule_DataBarDirection_Property")),
				    new DisplayNameAttribute("DataBarDirection")				);


				tableBuilder.AddCustomAttributes(t, "DataBrush",
					new DescriptionAttribute(SR.GetString("DataBarConditionalFormatRule_DataBrush_Property")),
				    new DisplayNameAttribute("DataBrush")				);


				tableBuilder.AddCustomAttributes(t, "NegativeDataBrush",
					new DescriptionAttribute(SR.GetString("DataBarConditionalFormatRule_NegativeDataBrush_Property")),
				    new DisplayNameAttribute("NegativeDataBrush")				);


				tableBuilder.AddCustomAttributes(t, "UseNegativeDataBar",
					new DescriptionAttribute(SR.GetString("DataBarConditionalFormatRule_UseNegativeDataBar_Property")),
				    new DisplayNameAttribute("UseNegativeDataBar")				);

				#endregion // DataBarConditionalFormatRule Properties

				#region DataBarConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DataBarConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DataBarConditionalFormatRuleProxy Properties

				#region NullableIntConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.NullableIntConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NullableIntConverter Properties

				#region NullableDoubleConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.NullableDoubleConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NullableDoubleConverter Properties

				#region PagerCellsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagerCellsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PagerCellsPanel Properties

				#region CellsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CellsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("CellsPanel_Row_Property")),
				    new DisplayNameAttribute("Row")				);


				tableBuilder.AddCustomAttributes(t, "Owner",
					new DescriptionAttribute(SR.GetString("CellsPanel_Owner_Property")),
				    new DisplayNameAttribute("Owner")				);

				#endregion // CellsPanel Properties

				#region HeaderRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.HeaderRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("HeaderRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("HeaderRow_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);

				#endregion // HeaderRow Properties

				#region GroupByAreaRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByAreaRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("GroupByAreaRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "Cells",
					new DescriptionAttribute(SR.GetString("GroupByAreaRow_Cells_Property")),
				    new DisplayNameAttribute("Cells")				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("GroupByAreaRow_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);

				#endregion // GroupByAreaRow Properties

				#region TemplateColumnLayout Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.TemplateColumnLayout");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Template",
					new DescriptionAttribute(SR.GetString("TemplateColumnLayout_Template_Property")),
				    new DisplayNameAttribute("Template")				);

				#endregion // TemplateColumnLayout Properties

				#region ComboBoxColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ComboBoxColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ComboBoxColumnContentProvider Properties

				#region ColumnTypeMapping Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnTypeMapping");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnType",
					new DescriptionAttribute(SR.GetString("ColumnTypeMapping_ColumnType_Property")),
				    new DisplayNameAttribute("ColumnType")				);


				tableBuilder.AddCustomAttributes(t, "DataType",
					new DescriptionAttribute(SR.GetString("ColumnTypeMapping_DataType_Property")),
				    new DisplayNameAttribute("DataType")				);

				#endregion // ColumnTypeMapping Properties

				#region ConditionalFormattingCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ConditionalFormattingCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "Icon",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingCellControl_Icon_Property")),
				    new DisplayNameAttribute("Icon")				);


				tableBuilder.AddCustomAttributes(t, "BarDirection",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingCellControl_BarDirection_Property")),
				    new DisplayNameAttribute("BarDirection")				);


				tableBuilder.AddCustomAttributes(t, "BarPercentage",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingCellControl_BarPercentage_Property")),
				    new DisplayNameAttribute("BarPercentage")				);


				tableBuilder.AddCustomAttributes(t, "BarPositiveOrNegative",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingCellControl_BarPositiveOrNegative_Property")),
				    new DisplayNameAttribute("BarPositiveOrNegative")				);


				tableBuilder.AddCustomAttributes(t, "BarBrush",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingCellControl_BarBrush_Property")),
				    new DisplayNameAttribute("BarBrush")				);


				tableBuilder.AddCustomAttributes(t, "NegativeBarBrush",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingCellControl_NegativeBarBrush_Property")),
				    new DisplayNameAttribute("NegativeBarBrush")				);


				tableBuilder.AddCustomAttributes(t, "AltBackground",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingCellControl_AltBackground_Property")),
				    new DisplayNameAttribute("AltBackground")				);

				#endregion // ConditionalFormattingCellControl Properties

				#region ChildBandCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ChildBandCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // ChildBandCellControl Properties

				#region CellBaseCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CellBaseCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CellBaseCommandBase Properties

				#region FooterRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FooterRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("FooterRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("FooterRow_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);

				#endregion // FooterRow Properties

				#region ChildBandCellsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ChildBandCellsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ChildBandCellsPanel Properties

				#region FixedBorderColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixedBorderColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("FixedBorderColumn_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);


				tableBuilder.AddCustomAttributes(t, "IsFixable",
					new DescriptionAttribute(SR.GetString("FixedBorderColumn_IsFixable_Property")),
				    new DisplayNameAttribute("IsFixable")				);


				tableBuilder.AddCustomAttributes(t, "IsGroupable",
					new DescriptionAttribute(SR.GetString("FixedBorderColumn_IsGroupable_Property")),
				    new DisplayNameAttribute("IsGroupable")				);


				tableBuilder.AddCustomAttributes(t, "IsMovable",
					new DescriptionAttribute(SR.GetString("FixedBorderColumn_IsMovable_Property")),
				    new DisplayNameAttribute("IsMovable")				);


				tableBuilder.AddCustomAttributes(t, "IsResizable",
					new DescriptionAttribute(SR.GetString("FixedBorderColumn_IsResizable_Property")),
				    new DisplayNameAttribute("IsResizable")				);


				tableBuilder.AddCustomAttributes(t, "MinimumWidth",
					new DescriptionAttribute(SR.GetString("FixedBorderColumn_MinimumWidth_Property")),
				    new DisplayNameAttribute("MinimumWidth")				);


				tableBuilder.AddCustomAttributes(t, "WidthResolved",
					new DescriptionAttribute(SR.GetString("FixedBorderColumn_WidthResolved_Property")),
				    new DisplayNameAttribute("WidthResolved")				);

				#endregion // FixedBorderColumn Properties

				#region ColumnTypeMappingsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnTypeMappingsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnTypeMappingsCollection Properties

				#region SummaryRowExpansionIndicatorCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SummaryRowExpansionIndicatorCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // SummaryRowExpansionIndicatorCellControl Properties

				#region RowSelectorHeaderCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowSelectorHeaderCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // RowSelectorHeaderCellControl Properties

				#region ChildBandCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ChildBandCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ChildBandCell Properties

				#region SelectionSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SelectionSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CellSelection",
					new DescriptionAttribute(SR.GetString("SelectionSettings_CellSelection_Property")),
				    new DisplayNameAttribute("CellSelection")				);


				tableBuilder.AddCustomAttributes(t, "RowSelection",
					new DescriptionAttribute(SR.GetString("SelectionSettings_RowSelection_Property")),
				    new DisplayNameAttribute("RowSelection")				);


				tableBuilder.AddCustomAttributes(t, "ColumnSelection",
					new DescriptionAttribute(SR.GetString("SelectionSettings_ColumnSelection_Property")),
				    new DisplayNameAttribute("ColumnSelection")				);


				tableBuilder.AddCustomAttributes(t, "CellClickAction",
					new DescriptionAttribute(SR.GetString("SelectionSettings_CellClickAction_Property")),
				    new DisplayNameAttribute("CellClickAction")				);


				tableBuilder.AddCustomAttributes(t, "SelectedRows",
					new DescriptionAttribute(SR.GetString("SelectionSettings_SelectedRows_Property")),
				    new DisplayNameAttribute("SelectedRows")				);


				tableBuilder.AddCustomAttributes(t, "SelectedCells",
					new DescriptionAttribute(SR.GetString("SelectionSettings_SelectedCells_Property")),
				    new DisplayNameAttribute("SelectedCells")				);


				tableBuilder.AddCustomAttributes(t, "SelectedColumns",
					new DescriptionAttribute(SR.GetString("SelectionSettings_SelectedColumns_Property")),
				    new DisplayNameAttribute("SelectedColumns")				);

				#endregion // SelectionSettings Properties

				#region MergedContentControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.MergedContentControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MergedContentControl Properties

				#region GroupBySettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GroupBySettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsGroupable",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_IsGroupable_Property")),
				    new DisplayNameAttribute("IsGroupable")				);


				tableBuilder.AddCustomAttributes(t, "IsGroupableResolved",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_IsGroupableResolved_Property")),
				    new DisplayNameAttribute("IsGroupableResolved")				);


				tableBuilder.AddCustomAttributes(t, "GroupByMovingIndicatorStyle",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByMovingIndicatorStyle_Property")),
				    new DisplayNameAttribute("GroupByMovingIndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupByMovingIndicatorStyleResolved",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByMovingIndicatorStyleResolved_Property")),
				    new DisplayNameAttribute("GroupByMovingIndicatorStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "GroupByHeaderStyle",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByHeaderStyle_Property")),
				    new DisplayNameAttribute("GroupByHeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupByHeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByHeaderStyleResolved_Property")),
				    new DisplayNameAttribute("GroupByHeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "GroupByColumnLayoutHeaderStyle",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByColumnLayoutHeaderStyle_Property")),
				    new DisplayNameAttribute("GroupByColumnLayoutHeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupByColumnLayoutHeaderStyleResolved",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByColumnLayoutHeaderStyleResolved_Property")),
				    new DisplayNameAttribute("GroupByColumnLayoutHeaderStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "GroupByRowStyle",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByRowStyle_Property")),
				    new DisplayNameAttribute("GroupByRowStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupByRowStyleResolved",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByRowStyleResolved_Property")),
				    new DisplayNameAttribute("GroupByRowStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "GroupByRowHeight",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByRowHeight_Property")),
				    new DisplayNameAttribute("GroupByRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "GroupByRowHeightResolved",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByRowHeightResolved_Property")),
				    new DisplayNameAttribute("GroupByRowHeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "GroupByAreaRowHeight",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByAreaRowHeight_Property")),
				    new DisplayNameAttribute("GroupByAreaRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "GroupByAreaRowHeightResolved",
					new DescriptionAttribute(SR.GetString("GroupBySettingsOverride_GroupByAreaRowHeightResolved_Property")),
				    new DisplayNameAttribute("GroupByAreaRowHeightResolved")				);

				#endregion // GroupBySettingsOverride Properties

				#region FilterControlBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterControlBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("FilterControlBase_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);

				#endregion // FilterControlBase Properties

				#region FilterControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("FilterControl_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "SelectedItemIcon",
					new DescriptionAttribute(SR.GetString("FilterControl_SelectedItemIcon_Property")),
				    new DisplayNameAttribute("SelectedItemIcon")				);

				#endregion // FilterControl Properties

				#region DeferredScrollingSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DeferredScrollingSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowDeferredScrolling",
					new DescriptionAttribute(SR.GetString("DeferredScrollingSettings_AllowDeferredScrolling_Property")),
				    new DisplayNameAttribute("AllowDeferredScrolling")				);


				tableBuilder.AddCustomAttributes(t, "DeferredScrollTemplate",
					new DescriptionAttribute(SR.GetString("DeferredScrollingSettings_DeferredScrollTemplate_Property")),
				    new DisplayNameAttribute("DeferredScrollTemplate")				);


				tableBuilder.AddCustomAttributes(t, "DefaultColumnKey",
					new DescriptionAttribute(SR.GetString("DeferredScrollingSettings_DefaultColumnKey_Property")),
				    new DisplayNameAttribute("DefaultColumnKey")				);


				tableBuilder.AddCustomAttributes(t, "GroupByDeferredScrollTemplate",
					new DescriptionAttribute(SR.GetString("DeferredScrollingSettings_GroupByDeferredScrollTemplate_Property")),
				    new DisplayNameAttribute("GroupByDeferredScrollTemplate")				);

				#endregion // DeferredScrollingSettings Properties

				#region PercentData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.PercentData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Range",
					new DescriptionAttribute(SR.GetString("PercentData_Range_Property")),
				    new DisplayNameAttribute("Range")				);

				#endregion // PercentData Properties

				#region AddNewRowSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.AddNewRowSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowAddNewRow",
					new DescriptionAttribute(SR.GetString("AddNewRowSettings_AllowAddNewRow_Property")),
				    new DisplayNameAttribute("AllowAddNewRow")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("AddNewRowSettings_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorStyle",
					new DescriptionAttribute(SR.GetString("AddNewRowSettings_ExpansionIndicatorStyle_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorStyle",
					new DescriptionAttribute(SR.GetString("AddNewRowSettings_RowSelectorStyle_Property")),
				    new DisplayNameAttribute("RowSelectorStyle")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowHeight",
					new DescriptionAttribute(SR.GetString("AddNewRowSettings_AddNewRowHeight_Property")),
				    new DisplayNameAttribute("AddNewRowHeight")				);

				#endregion // AddNewRowSettings Properties

				#region IComparableTypeConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.IComparableTypeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // IComparableTypeConverter Properties

				#region RowCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowCollection Properties

				#region GroupByAreaRowCellsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByAreaRowCellsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupByAreaRowCellsPanel Properties

				#region FilterRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("FilterRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "HeightResolved",
					new DescriptionAttribute(SR.GetString("FilterRow_HeightResolved_Property")),
				    new DisplayNameAttribute("HeightResolved")				);


				tableBuilder.AddCustomAttributes(t, "HasChildren",
					new DescriptionAttribute(SR.GetString("FilterRow_HasChildren_Property")),
				    new DisplayNameAttribute("HasChildren")				);

				#endregion // FilterRow Properties

				#region ImageColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ImageColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ImageColumnContentProvider Properties

				#region ChildBandColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ChildBandColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "WidthResolved",
					new DescriptionAttribute(SR.GetString("ChildBandColumn_WidthResolved_Property")),
				    new DisplayNameAttribute("WidthResolved")				);

				#endregion // ChildBandColumn Properties

				#region SummaryRowCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SummaryRowCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsEditable",
					new DescriptionAttribute(SR.GetString("SummaryRowCell_IsEditable_Property")),
				    new DisplayNameAttribute("IsEditable")				);

				#endregion // SummaryRowCell Properties

				#region GroupHeaderCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupHeaderCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // GroupHeaderCellControl Properties

				#region FixedBorderFooterCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixedBorderFooterCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // FixedBorderFooterCellControl Properties

				#region FixedBorderFooterCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixedBorderFooterCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FixedBorderFooterCell Properties

				#region FilterRowSelectorCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterRowSelectorCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FilterRowSelectorCell Properties

				#region ExpansionIndicatorHeaderCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ExpansionIndicatorHeaderCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // ExpansionIndicatorHeaderCellControl Properties

				#region AddNewRowCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.AddNewRowCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsEditable",
					new DescriptionAttribute(SR.GetString("AddNewRowCell_IsEditable_Property")),
				    new DisplayNameAttribute("IsEditable")				);

				#endregion // AddNewRowCell Properties

				#region SimpleClickableContainer Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SimpleClickableContainer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Child",
					new DescriptionAttribute(SR.GetString("SimpleClickableContainer_Child_Property")),
				    new DisplayNameAttribute("Child")				);

				#endregion // SimpleClickableContainer Properties

				#region XamGridFilteringCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridFilteringCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamGridFilteringCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamGridFilteringCommandSource_Properties"))
				);

				#endregion // XamGridFilteringCommandSource Properties

				#region RowFilteringCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowFilteringCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowFilteringCommandBase Properties

				#region ClearFilters Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ClearFilters");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ClearFilters Properties

				#region GreaterThanConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GreaterThanConditionalFormatRule Properties

				#region GreaterThanConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GreaterThanConditionalFormatRuleProxy Properties

				#region ConditionalFormattingSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ConditionalFormattingSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowConditionalFormatting",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingSettings_AllowConditionalFormatting_Property")),
				    new DisplayNameAttribute("AllowConditionalFormatting")				);

				#endregion // ConditionalFormattingSettings Properties

				#region ConditionalFormatCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ConditionalFormatCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("ConditionalFormatCollection_Column_Property")),
				    new DisplayNameAttribute("Column")				);

				#endregion // ConditionalFormatCollection Properties

				#region AddNewRowSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.AddNewRowSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowAddNewRow",
					new DescriptionAttribute(SR.GetString("AddNewRowSettingsOverride_AllowAddNewRow_Property")),
				    new DisplayNameAttribute("AllowAddNewRow")				);


				tableBuilder.AddCustomAttributes(t, "AllowAddNewRowResolved",
					new DescriptionAttribute(SR.GetString("AddNewRowSettingsOverride_AllowAddNewRowResolved_Property")),
				    new DisplayNameAttribute("AllowAddNewRowResolved")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorStyle",
					new DescriptionAttribute(SR.GetString("AddNewRowSettingsOverride_ExpansionIndicatorStyle_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorStyleResolved",
					new DescriptionAttribute(SR.GetString("AddNewRowSettingsOverride_ExpansionIndicatorStyleResolved_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("AddNewRowSettingsOverride_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "StyleResolved",
					new DescriptionAttribute(SR.GetString("AddNewRowSettingsOverride_StyleResolved_Property")),
				    new DisplayNameAttribute("StyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorStyle",
					new DescriptionAttribute(SR.GetString("AddNewRowSettingsOverride_RowSelectorStyle_Property")),
				    new DisplayNameAttribute("RowSelectorStyle")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorStyleResolved",
					new DescriptionAttribute(SR.GetString("AddNewRowSettingsOverride_RowSelectorStyleResolved_Property")),
				    new DisplayNameAttribute("RowSelectorStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowHeight",
					new DescriptionAttribute(SR.GetString("AddNewRowSettingsOverride_AddNewRowHeight_Property")),
				    new DisplayNameAttribute("AddNewRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "AddNewRowHeightResolved",
					new DescriptionAttribute(SR.GetString("AddNewRowSettingsOverride_AddNewRowHeightResolved_Property")),
				    new DisplayNameAttribute("AddNewRowHeightResolved")				);

				#endregion // AddNewRowSettingsOverride Properties

				#region ComboBoxColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ComboBoxColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("ComboBoxColumn_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource")				);


				tableBuilder.AddCustomAttributes(t, "DisplayMemberPath",
					new DescriptionAttribute(SR.GetString("ComboBoxColumn_DisplayMemberPath_Property")),
				    new DisplayNameAttribute("DisplayMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "SelectedValuePath",
					new DescriptionAttribute(SR.GetString("ComboBoxColumn_SelectedValuePath_Property")),
				    new DisplayNameAttribute("SelectedValuePath")				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplate",
					new DescriptionAttribute(SR.GetString("ComboBoxColumn_ItemTemplate_Property")),
				    new DisplayNameAttribute("ItemTemplate")				);

				#endregion // ComboBoxColumn Properties

				#region ColumnLayoutCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnLayoutCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnLayoutCollection Properties

				#region ConditionalFormattingCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ConditionalFormattingCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("ConditionalFormattingCell_Style_Property")),
				    new DisplayNameAttribute("Style")				);

				#endregion // ConditionalFormattingCell Properties

				#region SummaryRowSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SummaryRowSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowSummaryRow",
					new DescriptionAttribute(SR.GetString("SummaryRowSettings_AllowSummaryRow_Property")),
				    new DisplayNameAttribute("AllowSummaryRow")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("SummaryRowSettings_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorStyle",
					new DescriptionAttribute(SR.GetString("SummaryRowSettings_ExpansionIndicatorStyle_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorStyle",
					new DescriptionAttribute(SR.GetString("SummaryRowSettings_RowSelectorStyle_Property")),
				    new DisplayNameAttribute("RowSelectorStyle")				);


				tableBuilder.AddCustomAttributes(t, "SummaryScope",
					new DescriptionAttribute(SR.GetString("SummaryRowSettings_SummaryScope_Property")),
				    new DisplayNameAttribute("SummaryScope")				);


				tableBuilder.AddCustomAttributes(t, "SummaryExecution",
					new DescriptionAttribute(SR.GetString("SummaryRowSettings_SummaryExecution_Property")),
				    new DisplayNameAttribute("SummaryExecution")				);


				tableBuilder.AddCustomAttributes(t, "SummaryDefinitionCollection",
					new DescriptionAttribute(SR.GetString("SummaryRowSettings_SummaryDefinitionCollection_Property")),
				    new DisplayNameAttribute("SummaryDefinitionCollection")				);


				tableBuilder.AddCustomAttributes(t, "SummaryResultCollection",
					new DescriptionAttribute(SR.GetString("SummaryRowSettings_SummaryResultCollection_Property")),
				    new DisplayNameAttribute("SummaryResultCollection")				);

				#endregion // SummaryRowSettings Properties

				#region MergeDataContext Properties
				t = controlAssembly.GetType("Infragistics.MergeDataContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("MergeDataContext_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "Records",
					new DescriptionAttribute(SR.GetString("MergeDataContext_Records_Property")),
				    new DisplayNameAttribute("Records")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("MergeDataContext_Count_Property")),
				    new DisplayNameAttribute("Count")				);

				#endregion // MergeDataContext Properties

				#region FilterSelectionControlCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FilterSelectionControlCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("FilterSelectionControlCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // FilterSelectionControlCommandSource Properties

				#region ClearSelectedItemsCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ClearSelectedItemsCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ClearSelectedItemsCommand Properties

				#region CustomFilteringDialogCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CustomFilteringDialogCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("CustomFilteringDialogCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // CustomFilteringDialogCommandSource Properties

				#region AcceptCustomFilterDialogChangesCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.AcceptCustomFilterDialogChangesCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AcceptCustomFilterDialogChangesCommand Properties

				#region CloseCustomFilterDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CloseCustomFilterDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CloseCustomFilterDialogCommand Properties

				#region CustomFilteringDialogControlCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CustomFilteringDialogControlCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("CustomFilteringDialogControlCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // CustomFilteringDialogControlCommandSource Properties

				#region CustomFilterDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CustomFilterDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CustomFilterDialogCommand Properties

				#region ShowCustomFilterDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ShowCustomFilterDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ShowCustomFilterDialogCommand Properties

				#region FilterValueProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterValueProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("FilterValueProxy_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "ContentString",
					new DescriptionAttribute(SR.GetString("FilterValueProxy_ContentString_Property")),
				    new DisplayNameAttribute("ContentString")				);


				tableBuilder.AddCustomAttributes(t, "IsChecked",
					new DescriptionAttribute(SR.GetString("FilterValueProxy_IsChecked_Property")),
				    new DisplayNameAttribute("IsChecked")				);

				#endregion // FilterValueProxy Properties

				#region FilterValueProxyCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterValueProxyCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("FilterItemCollection`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);

				#endregion // FilterValueProxyCollection Properties

				#region FillerColumnSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FillerColumnSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyle",
					new DescriptionAttribute(SR.GetString("FillerColumnSettings_HeaderStyle_Property")),
				    new DisplayNameAttribute("HeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyle",
					new DescriptionAttribute(SR.GetString("FillerColumnSettings_FooterStyle_Property")),
				    new DisplayNameAttribute("FooterStyle")				);

				#endregion // FillerColumnSettings Properties

				#region NotBetweenXandYConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.NotBetweenXandYConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NotBetweenXandYConditionalFormatRule Properties

				#region NotBetweenXandYConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.NotBetweenXandYConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NotBetweenXandYConditionalFormatRuleProxy Properties

				#region CellControlBaseAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.CellControlBaseAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("CellControlBaseAutomationPeer_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("CellControlBaseAutomationPeer_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("CellControlBaseAutomationPeer_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "SelectionContainer",
					new DescriptionAttribute(SR.GetString("CellControlBaseAutomationPeer_SelectionContainer_Property")),
				    new DisplayNameAttribute("SelectionContainer")				);

				#endregion // CellControlBaseAutomationPeer Properties

				#region ColumnLayoutTemplateRowCellsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ColumnLayoutTemplateRowCellsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColumnLayoutTemplateRowCellsPanel Properties

				#region TemplateColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.TemplateColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RemovePaddingDuringEditing",
					new DescriptionAttribute(SR.GetString("TemplateColumnContentProvider_RemovePaddingDuringEditing_Property")),
				    new DisplayNameAttribute("RemovePaddingDuringEditing")				);

				#endregion // TemplateColumnContentProvider Properties

				#region GroupColumnPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupColumnPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("GroupColumnPanel_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("GroupColumnPanel_Row_Property")),
				    new DisplayNameAttribute("Row")				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("GroupColumnPanel_Column_Property")),
				    new DisplayNameAttribute("Column")				);

				#endregion // GroupColumnPanel Properties

				#region DateColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedDateFormat",
					new DescriptionAttribute(SR.GetString("DateColumn_SelectedDateFormat_Property")),
				    new DisplayNameAttribute("SelectedDateFormat")				);

				#endregion // DateColumn Properties

				#region RowSelectorFooterCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RowSelectorFooterCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowSelectorFooterCell Properties

				#region CultureValueConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CultureValueConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CultureValueConverter Properties

				#region SummaryRowSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SummaryRowSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowSummaryRow",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_AllowSummaryRow_Property")),
				    new DisplayNameAttribute("AllowSummaryRow")				);


				tableBuilder.AddCustomAttributes(t, "AllowSummaryRowResolved",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_AllowSummaryRowResolved_Property")),
				    new DisplayNameAttribute("AllowSummaryRowResolved")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorStyle",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_ExpansionIndicatorStyle_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorStyleResolved",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_ExpansionIndicatorStyleResolved_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "StyleResolved",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_StyleResolved_Property")),
				    new DisplayNameAttribute("StyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorStyle",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_RowSelectorStyle_Property")),
				    new DisplayNameAttribute("RowSelectorStyle")				);


				tableBuilder.AddCustomAttributes(t, "RowSelectorStyleResolved",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_RowSelectorStyleResolved_Property")),
				    new DisplayNameAttribute("RowSelectorStyleResolved")				);


				tableBuilder.AddCustomAttributes(t, "SummaryDefinitionCollection",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_SummaryDefinitionCollection_Property")),
				    new DisplayNameAttribute("SummaryDefinitionCollection")				);


				tableBuilder.AddCustomAttributes(t, "SummaryScope",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_SummaryScope_Property")),
				    new DisplayNameAttribute("SummaryScope")				);


				tableBuilder.AddCustomAttributes(t, "SummaryScopeResolved",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_SummaryScopeResolved_Property")),
				    new DisplayNameAttribute("SummaryScopeResolved")				);


				tableBuilder.AddCustomAttributes(t, "SummaryExecution",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_SummaryExecution_Property")),
				    new DisplayNameAttribute("SummaryExecution")				);


				tableBuilder.AddCustomAttributes(t, "SummaryExecutionResolved",
					new DescriptionAttribute(SR.GetString("SummaryRowSettingsOverride_SummaryExecutionResolved_Property")),
				    new DisplayNameAttribute("SummaryExecutionResolved")				);

				#endregion // SummaryRowSettingsOverride Properties

				#region SelectColumnCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.SelectColumnCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SelectColumnCommand Properties

				#region UnselectColumnCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.UnselectColumnCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // UnselectColumnCommand Properties

				#region XamGridPagingCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridPagingCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamGridPagingCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamGridPagingCommandSource_Properties"))
				);

				#endregion // XamGridPagingCommandSource Properties

				#region XamGridPagingControlsCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridPagingControlsCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamGridPagingControlsCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamGridPagingControlsCommandSource_Properties"))
				);

				#endregion // XamGridPagingControlsCommandSource Properties

				#region PagingBaseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagingBaseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PagingBaseCommand Properties

				#region FirstPageCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FirstPageCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FirstPageCommand Properties

				#region PreviousPageCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PreviousPageCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PreviousPageCommand Properties

				#region LastPageCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.LastPageCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LastPageCommand Properties

				#region NextPageCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.NextPageCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NextPageCommand Properties

				#region GoToPageCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GoToPageCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GoToPageCommand Properties

				#region PagingControlCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagingControlCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PagingControlCommandBase Properties

				#region FirstPageControlCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FirstPageControlCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FirstPageControlCommand Properties

				#region PreviousPageControlCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PreviousPageControlCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PreviousPageControlCommand Properties

				#region NextPageControlCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.NextPageControlCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NextPageControlCommand Properties

				#region LastPageControlCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.LastPageControlCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LastPageControlCommand Properties

				#region GoToPageControlCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GoToPageControlCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GoToPageControlCommand Properties

				#region PagerSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.PagerSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowPaging",
					new DescriptionAttribute(SR.GetString("PagerSettings_AllowPaging_Property")),
				    new DisplayNameAttribute("AllowPaging")				);


				tableBuilder.AddCustomAttributes(t, "DisplayPagerWhenOnlyOnePage",
					new DescriptionAttribute(SR.GetString("PagerSettings_DisplayPagerWhenOnlyOnePage_Property")),
				    new DisplayNameAttribute("DisplayPagerWhenOnlyOnePage")				);


				tableBuilder.AddCustomAttributes(t, "PageSize",
					new DescriptionAttribute(SR.GetString("PagerSettings_PageSize_Property")),
				    new DisplayNameAttribute("PageSize")				);


				tableBuilder.AddCustomAttributes(t, "CurrentPageIndex",
					new DescriptionAttribute(SR.GetString("PagerSettings_CurrentPageIndex_Property")),
				    new DisplayNameAttribute("CurrentPageIndex")				);


				tableBuilder.AddCustomAttributes(t, "PagerRowHeight",
					new DescriptionAttribute(SR.GetString("PagerSettings_PagerRowHeight_Property")),
				    new DisplayNameAttribute("PagerRowHeight")				);

				#endregion // PagerSettings Properties

				#region RowFiltersCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.RowFiltersCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowFiltersCollection Properties

				#region ExpansionIndicatorSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ExpansionIndicatorSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyle",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorSettings_HeaderStyle_Property")),
				    new DisplayNameAttribute("HeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "FooterStyle",
					new DescriptionAttribute(SR.GetString("ExpansionIndicatorSettings_FooterStyle_Property")),
				    new DisplayNameAttribute("FooterStyle")				);

				#endregion // ExpansionIndicatorSettings Properties

				#region EditingSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EditingSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowEditing",
					new DescriptionAttribute(SR.GetString("EditingSettingsOverride_AllowEditing_Property")),
				    new DisplayNameAttribute("AllowEditing")				);


				tableBuilder.AddCustomAttributes(t, "AllowEditingResolved",
					new DescriptionAttribute(SR.GetString("EditingSettingsOverride_AllowEditingResolved_Property")),
				    new DisplayNameAttribute("AllowEditingResolved")				);

				#endregion // EditingSettingsOverride Properties

				#region MaximumValueConditionalFormatRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.MaximumValueConditionalFormatRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MaximumValueConditionalFormatRule Properties

				#region MaximumValueConditionalFormatRuleProxy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.MaximumValueConditionalFormatRuleProxy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MaximumValueConditionalFormatRuleProxy Properties

				#region PercentileData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.PercentileData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PercentileData Properties

				#region PercentileData`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.PercentileData`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PercentileData`1 Properties

				#region PercentileData`2 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.PercentileData`2");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PercentileData`2 Properties

				#region GroupByRowCellsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByRowCellsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupByRowCellsPanel Properties

				#region CheckBoxColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CheckBoxColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CheckBoxColumn Properties

				#region GroupByAreaCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByAreaCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupByAreaCell Properties

				#region FixedBorderCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixedBorderCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamGridSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamGridSupportingControlsAssetLibrary"))
				);

				#endregion // FixedBorderCellControl Properties

				#region AddNewRowSelectorCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.AddNewRowSelectorCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AddNewRowSelectorCell Properties

				#region FixedRowSeparator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FixedRowSeparator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FixedRowSeparator Properties

				#region SelectedColumnsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.SelectedColumnsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Grid",
					new DescriptionAttribute(SR.GetString("SelectedCollectionBase`1_Grid_Property")),
				    new DisplayNameAttribute("Grid")				);

				#endregion // SelectedColumnsCollection Properties

				#region GroupBySettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GroupBySettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowGroupByArea",
					new DescriptionAttribute(SR.GetString("GroupBySettings_AllowGroupByArea_Property")),
				    new DisplayNameAttribute("AllowGroupByArea")				);


				tableBuilder.AddCustomAttributes(t, "IsGroupable",
					new DescriptionAttribute(SR.GetString("GroupBySettings_IsGroupable_Property")),
				    new DisplayNameAttribute("IsGroupable")				);


				tableBuilder.AddCustomAttributes(t, "GroupByColumns",
					new DescriptionAttribute(SR.GetString("GroupBySettings_GroupByColumns_Property")),
				    new DisplayNameAttribute("GroupByColumns")				);


				tableBuilder.AddCustomAttributes(t, "EmptyGroupByAreaContent",
					new DescriptionAttribute(SR.GetString("GroupBySettings_EmptyGroupByAreaContent_Property")),
				    new DisplayNameAttribute("EmptyGroupByAreaContent")				);


				tableBuilder.AddCustomAttributes(t, "GroupByMovingIndicatorStyle",
					new DescriptionAttribute(SR.GetString("GroupBySettings_GroupByMovingIndicatorStyle_Property")),
				    new DisplayNameAttribute("GroupByMovingIndicatorStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupByAreaStyle",
					new DescriptionAttribute(SR.GetString("GroupBySettings_GroupByAreaStyle_Property")),
				    new DisplayNameAttribute("GroupByAreaStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupByHeaderStyle",
					new DescriptionAttribute(SR.GetString("GroupBySettings_GroupByHeaderStyle_Property")),
				    new DisplayNameAttribute("GroupByHeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupByColumnLayoutHeaderStyle",
					new DescriptionAttribute(SR.GetString("GroupBySettings_GroupByColumnLayoutHeaderStyle_Property")),
				    new DisplayNameAttribute("GroupByColumnLayoutHeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "GroupByRowStyle",
					new DescriptionAttribute(SR.GetString("GroupBySettings_GroupByRowStyle_Property")),
				    new DisplayNameAttribute("GroupByRowStyle")				);


				tableBuilder.AddCustomAttributes(t, "IsGroupByAreaExpanded",
					new DescriptionAttribute(SR.GetString("GroupBySettings_IsGroupByAreaExpanded_Property")),
				    new DisplayNameAttribute("IsGroupByAreaExpanded")				);


				tableBuilder.AddCustomAttributes(t, "ExpansionIndicatorVisibility",
					new DescriptionAttribute(SR.GetString("GroupBySettings_ExpansionIndicatorVisibility_Property")),
				    new DisplayNameAttribute("ExpansionIndicatorVisibility")				);


				tableBuilder.AddCustomAttributes(t, "GroupByRowHeight",
					new DescriptionAttribute(SR.GetString("GroupBySettings_GroupByRowHeight_Property")),
				    new DisplayNameAttribute("GroupByRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "GroupByAreaRowHeight",
					new DescriptionAttribute(SR.GetString("GroupBySettings_GroupByAreaRowHeight_Property")),
				    new DisplayNameAttribute("GroupByAreaRowHeight")				);


				tableBuilder.AddCustomAttributes(t, "GroupByOperation",
					new DescriptionAttribute(SR.GetString("GroupBySettings_GroupByOperation_Property")),
				    new DisplayNameAttribute("GroupByOperation")				);


				tableBuilder.AddCustomAttributes(t, "DisplayCountOnGroupedRow",
					new DescriptionAttribute(SR.GetString("GroupBySettings_DisplayCountOnGroupedRow_Property")),
				    new DisplayNameAttribute("DisplayCountOnGroupedRow")				);

				#endregion // GroupBySettings Properties

				#region GroupByColumnCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupByColumnCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupByColumnCommand Properties

				#region RemoveGroupByColumnCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.RemoveGroupByColumnCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RemoveGroupByColumnCommand Properties

				#region EqualsOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EqualsOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("EqualsOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // EqualsOperand Properties

				#region NotEqualsOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.NotEqualsOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("NotEqualsOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // NotEqualsOperand Properties

				#region GreaterThanOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("GreaterThanOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // GreaterThanOperand Properties

				#region GreaterThanOrEqualOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GreaterThanOrEqualOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("GreaterThanOrEqualOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // GreaterThanOrEqualOperand Properties

				#region LessThanOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.LessThanOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("LessThanOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // LessThanOperand Properties

				#region LessThanOrEqualOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.LessThanOrEqualOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("LessThanOrEqualOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // LessThanOrEqualOperand Properties

				#region StartsWithOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.StartsWithOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("StartsWithOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // StartsWithOperand Properties

				#region EndsWithOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EndsWithOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("EndsWithOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // EndsWithOperand Properties

				#region ContainsOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ContainsOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("ContainsOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // ContainsOperand Properties

				#region DoesNotContainOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DoesNotContainOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DoesNotContainOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DoesNotContainOperand Properties

				#region DoesNotStartWithOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DoesNotStartWithOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DoesNotStartWithOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DoesNotStartWithOperand Properties

				#region DoesNotEndWithOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DoesNotEndWithOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DoesNotEndWithOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DoesNotEndWithOperand Properties

				#region FilterColumnSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FilterColumnSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FilteringOperand",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilteringOperand_Property")),
				    new DisplayNameAttribute("FilteringOperand")				);


				tableBuilder.AddCustomAttributes(t, "FilteringOperandResolved",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilteringOperandResolved_Property")),
				    new DisplayNameAttribute("FilteringOperandResolved")				);


				tableBuilder.AddCustomAttributes(t, "FilterCaseSensitive",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterCaseSensitive_Property")),
				    new DisplayNameAttribute("FilterCaseSensitive")				);


				tableBuilder.AddCustomAttributes(t, "FilterCellValue",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterCellValue_Property")),
				    new DisplayNameAttribute("FilterCellValue")				);


				tableBuilder.AddCustomAttributes(t, "RowFilterOperands",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_RowFilterOperands_Property")),
				    new DisplayNameAttribute("RowFilterOperands")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuClearFiltersString",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterMenuClearFiltersString_Property")),
				    new DisplayNameAttribute("FilterMenuClearFiltersString")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuClearFiltersStringResolved",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterMenuClearFiltersStringResolved_Property")),
				    new DisplayNameAttribute("FilterMenuClearFiltersStringResolved")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuTypeSpecificFiltersString",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterMenuTypeSpecificFiltersString_Property")),
				    new DisplayNameAttribute("FilterMenuTypeSpecificFiltersString")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuCustomFilteringButtonVisibility",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterMenuCustomFilteringButtonVisibility_Property")),
				    new DisplayNameAttribute("FilterMenuCustomFilteringButtonVisibility")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuCustomFilteringButtonVisibilityResolved",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterMenuCustomFilteringButtonVisibilityResolved_Property")),
				    new DisplayNameAttribute("FilterMenuCustomFilteringButtonVisibilityResolved")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuFormatString",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterMenuFormatString_Property")),
				    new DisplayNameAttribute("FilterMenuFormatString")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuFormatStringResolved",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterMenuFormatStringResolved_Property")),
				    new DisplayNameAttribute("FilterMenuFormatStringResolved")				);


				tableBuilder.AddCustomAttributes(t, "FilterRowCellStyle",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterRowCellStyle_Property")),
				    new DisplayNameAttribute("FilterRowCellStyle")				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_Column_Property")),
				    new DisplayNameAttribute("Column")				);


				tableBuilder.AddCustomAttributes(t, "FilterMenuOperands",
					new DescriptionAttribute(SR.GetString("FilterColumnSettings_FilterMenuOperands_Property")),
				    new DisplayNameAttribute("FilterMenuOperands")				);

				#endregion // FilterColumnSettings Properties

				#region EditingSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.EditingSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowEditing",
					new DescriptionAttribute(SR.GetString("EditingSettings_AllowEditing_Property")),
				    new DisplayNameAttribute("AllowEditing")				);

				#endregion // EditingSettings Properties

				#region ConditionalFormatProxyCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ConditionalFormatProxyCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ConditionalFormatProxyCollection Properties

				#region ColumnChooserSettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnChooserSettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowHiddenColumnIndicator",
					new DescriptionAttribute(SR.GetString("ColumnChooserSettingsOverride_AllowHiddenColumnIndicator_Property")),
				    new DisplayNameAttribute("AllowHiddenColumnIndicator")				);


				tableBuilder.AddCustomAttributes(t, "AllowHiddenColumnIndicatorResolved",
					new DescriptionAttribute(SR.GetString("ColumnChooserSettingsOverride_AllowHiddenColumnIndicatorResolved_Property")),
				    new DisplayNameAttribute("AllowHiddenColumnIndicatorResolved")				);


				tableBuilder.AddCustomAttributes(t, "AllowHideColumnIcon",
					new DescriptionAttribute(SR.GetString("ColumnChooserSettingsOverride_AllowHideColumnIcon_Property")),
				    new DisplayNameAttribute("AllowHideColumnIcon")				);


				tableBuilder.AddCustomAttributes(t, "AllowHideColumnIconResolved",
					new DescriptionAttribute(SR.GetString("ColumnChooserSettingsOverride_AllowHideColumnIconResolved_Property")),
				    new DisplayNameAttribute("AllowHideColumnIconResolved")				);

				#endregion // ColumnChooserSettingsOverride Properties

				#region XamGridAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamGridAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontallyScrollable",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_HorizontallyScrollable_Property")),
				    new DisplayNameAttribute("HorizontallyScrollable"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticallyScrollable",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_VerticallyScrollable_Property")),
				    new DisplayNameAttribute("VerticallyScrollable"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalScrollPercent",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_HorizontalScrollPercent_Property")),
				    new DisplayNameAttribute("HorizontalScrollPercent"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalViewSize",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_HorizontalViewSize_Property")),
				    new DisplayNameAttribute("HorizontalViewSize"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalScrollPercent",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_VerticalScrollPercent_Property")),
				    new DisplayNameAttribute("VerticalScrollPercent"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalViewSize",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_VerticalViewSize_Property")),
				    new DisplayNameAttribute("VerticalViewSize"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CanSelectMultiple",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_CanSelectMultiple_Property")),
				    new DisplayNameAttribute("CanSelectMultiple"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionRequired",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_IsSelectionRequired_Property")),
				    new DisplayNameAttribute("IsSelectionRequired"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnCount",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_ColumnCount_Property")),
				    new DisplayNameAttribute("ColumnCount"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RowCount",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_RowCount_Property")),
				    new DisplayNameAttribute("RowCount"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RowOrColumnMajor",
					new DescriptionAttribute(SR.GetString("XamGridAutomationPeer_RowOrColumnMajor_Property")),
				    new DisplayNameAttribute("RowOrColumnMajor"),
					new CategoryAttribute(SR.GetString("XamGridAutomationPeer_Properties"))
				);

				#endregion // XamGridAutomationPeer Properties

				#region ColumnAutoGeneratedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnAutoGeneratedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("ColumnAutoGeneratedEventArgs_Column_Property")),
				    new DisplayNameAttribute("Column")				);

				#endregion // ColumnAutoGeneratedEventArgs Properties

				#region ClosePopupConditionalCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ClosePopupConditionalCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ClosePopupConditionalCommand Properties

				#region ShowCustomFilterDialogFilterMenuConditionalCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ShowCustomFilterDialogFilterMenuConditionalCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ShowCustomFilterDialogFilterMenuConditionalCommand Properties

				#region DateTimeJanuaryFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeJanuaryFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeJanuaryFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeJanuaryFilterOperand Properties

				#region DateTimeFebruaryFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeFebruaryFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeFebruaryFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeFebruaryFilterOperand Properties

				#region DateTimeMarchFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeMarchFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeMarchFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeMarchFilterOperand Properties

				#region DateTimeAprilFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeAprilFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeAprilFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeAprilFilterOperand Properties

				#region DateTimeMayFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeMayFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeMayFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeMayFilterOperand Properties

				#region DateTimeJuneFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeJuneFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeJuneFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeJuneFilterOperand Properties

				#region DateTimeJulyFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeJulyFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeJulyFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeJulyFilterOperand Properties

				#region DateTimeAugustFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeAugustFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeAugustFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeAugustFilterOperand Properties

				#region DateTimeSeptemberFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeSeptemberFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeSeptemberFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeSeptemberFilterOperand Properties

				#region DateTimeOctoberFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeOctoberFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeOctoberFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeOctoberFilterOperand Properties

				#region DateTimeNovemberFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeNovemberFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeNovemberFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeNovemberFilterOperand Properties

				#region DateTimeDecemberFilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeDecemberFilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeDecemberFilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeDecemberFilterOperand Properties

				#region DateTimeQuarter1FilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter1FilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeQuarter1FilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeQuarter1FilterOperand Properties

				#region DateTimeQuarter2FilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter2FilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeQuarter2FilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeQuarter2FilterOperand Properties

				#region DateTimeQuarter3FilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter3FilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeQuarter3FilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeQuarter3FilterOperand Properties

				#region DateTimeQuarter4FilterOperand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateTimeQuarter4FilterOperand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComparisonOperatorValue",
					new DescriptionAttribute(SR.GetString("DateTimeQuarter4FilterOperand_ComparisonOperatorValue_Property")),
				    new DisplayNameAttribute("ComparisonOperatorValue")				);

				#endregion // DateTimeQuarter4FilterOperand Properties

				#region CustomDisplayEditableColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.CustomDisplayEditableColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "EditorDisplayBehavior",
					new DescriptionAttribute(SR.GetString("CustomDisplayEditableColumn_EditorDisplayBehavior_Property")),
				    new DisplayNameAttribute("EditorDisplayBehavior")				);

				#endregion // CustomDisplayEditableColumn Properties

				#region TrackIsChecked Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.TrackIsChecked");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsChecked",
					new DescriptionAttribute(SR.GetString("TrackIsChecked_IsChecked_Property")),
				    new DisplayNameAttribute("IsChecked")				);


				tableBuilder.AddCustomAttributes(t, "Parent",
					new DescriptionAttribute(SR.GetString("TrackIsChecked_Parent_Property")),
				    new DisplayNameAttribute("Parent")				);

				#endregion // TrackIsChecked Properties

				#region TrackIsCheckWithChildren`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.TrackIsCheckWithChildren`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Children",
					new DescriptionAttribute(SR.GetString("TrackIsCheckWithChildren`1_Children_Property")),
				    new DisplayNameAttribute("Children")				);

				#endregion // TrackIsCheckWithChildren`1 Properties

				#region XamGridFilterDate Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridFilterDate");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Date",
					new DescriptionAttribute(SR.GetString("XamGridFilterDate_Date_Property")),
				    new DisplayNameAttribute("Date"),
					new CategoryAttribute(SR.GetString("XamGridFilterDate_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DateFilterObjectType",
					new DescriptionAttribute(SR.GetString("XamGridFilterDate_DateFilterObjectType_Property")),
				    new DisplayNameAttribute("DateFilterObjectType"),
					new CategoryAttribute(SR.GetString("XamGridFilterDate_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentString",
					new DescriptionAttribute(SR.GetString("XamGridFilterDate_ContentString_Property")),
				    new DisplayNameAttribute("ContentString"),
					new CategoryAttribute(SR.GetString("XamGridFilterDate_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentStringMonth",
					new DescriptionAttribute(SR.GetString("XamGridFilterDate_ContentStringMonth_Property")),
				    new DisplayNameAttribute("ContentStringMonth"),
					new CategoryAttribute(SR.GetString("XamGridFilterDate_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NullDate",
					new DescriptionAttribute(SR.GetString("XamGridFilterDate_NullDate_Property")),
				    new DisplayNameAttribute("NullDate"),
					new CategoryAttribute(SR.GetString("XamGridFilterDate_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsExpanded",
					new DescriptionAttribute(SR.GetString("XamGridFilterDate_IsExpanded_Property")),
				    new DisplayNameAttribute("IsExpanded"),
					new CategoryAttribute(SR.GetString("XamGridFilterDate_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Children",
					new DescriptionAttribute(SR.GetString("TrackIsCheckWithChildren`1_Children_Property")),
				    new DisplayNameAttribute("Children")				);

				#endregion // XamGridFilterDate Properties

				#region XamGridFilterYearCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridFilterYearCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("FilterItemCollection`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);

				#endregion // XamGridFilterYearCollection Properties

				#region XamGridPopupFilterMenuConditionalCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.XamGridPopupFilterMenuConditionalCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ConditionalCommandParameter",
					new DescriptionAttribute(SR.GetString("XamGridPopupFilterMenuConditionalCommandSource_ConditionalCommandParameter_Property")),
				    new DisplayNameAttribute("ConditionalCommandParameter"),
					new CategoryAttribute(SR.GetString("XamGridPopupFilterMenuConditionalCommandSource_Properties"))
				);

				#endregion // XamGridPopupFilterMenuConditionalCommandSource Properties

				#region CustomFilteringDialogFilterMenuCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CustomFilteringDialogFilterMenuCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ConditionalCommandParameter",
					new DescriptionAttribute(SR.GetString("CustomFilteringDialogFilterMenuCommandSource_ConditionalCommandParameter_Property")),
				    new DisplayNameAttribute("ConditionalCommandParameter")				);

				#endregion // CustomFilteringDialogFilterMenuCommandSource Properties

				#region DateFilterListDisplayObject Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.DateFilterListDisplayObject");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DateFilterObjectType",
					new DescriptionAttribute(SR.GetString("DateFilterListDisplayObject_DateFilterObjectType_Property")),
				    new DisplayNameAttribute("DateFilterObjectType")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("DateFilterListDisplayObject_Name_Property")),
				    new DisplayNameAttribute("Name")				);

				#endregion // DateFilterListDisplayObject Properties

				#region FilterValueProxyRowsFilter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FilterValueProxyRowsFilter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FieldName",
					new DescriptionAttribute(SR.GetString("FilterValueProxyRowsFilter_FieldName_Property")),
				    new DisplayNameAttribute("FieldName")				);


				tableBuilder.AddCustomAttributes(t, "FieldType",
					new DescriptionAttribute(SR.GetString("FilterValueProxyRowsFilter_FieldType_Property")),
				    new DisplayNameAttribute("FieldType")				);

				#endregion // FilterValueProxyRowsFilter Properties

				#region FilterMenuTrackingObject Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FilterMenuTrackingObject");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FilterOperands",
					new DescriptionAttribute(SR.GetString("FilterMenuTrackingObject_FilterOperands_Property")),
				    new DisplayNameAttribute("FilterOperands")				);


				tableBuilder.AddCustomAttributes(t, "IsSeparator",
					new DescriptionAttribute(SR.GetString("FilterMenuTrackingObject_IsSeparator_Property")),
				    new DisplayNameAttribute("IsSeparator")				);


				tableBuilder.AddCustomAttributes(t, "IsCheckable",
					new DescriptionAttribute(SR.GetString("FilterMenuTrackingObject_IsCheckable_Property")),
				    new DisplayNameAttribute("IsCheckable")				);


				tableBuilder.AddCustomAttributes(t, "Label",
					new DescriptionAttribute(SR.GetString("FilterMenuTrackingObject_Label_Property")),
				    new DisplayNameAttribute("Label")				);


				tableBuilder.AddCustomAttributes(t, "IsChecked",
					new DescriptionAttribute(SR.GetString("FilterMenuTrackingObject_IsChecked_Property")),
				    new DisplayNameAttribute("IsChecked")				);


				tableBuilder.AddCustomAttributes(t, "Children",
					new DescriptionAttribute(SR.GetString("FilterMenuTrackingObject_Children_Property")),
				    new DisplayNameAttribute("Children")				);


				tableBuilder.AddCustomAttributes(t, "FilterColumnSettings",
					new DescriptionAttribute(SR.GetString("FilterMenuTrackingObject_FilterColumnSettings_Property")),
				    new DisplayNameAttribute("FilterColumnSettings")				);

				#endregion // FilterMenuTrackingObject Properties

				#region DateFilterObjectStringFilter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateFilterObjectStringFilter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DateFilterObjectType",
					new DescriptionAttribute(SR.GetString("DateFilterObjectStringFilter_DateFilterObjectType_Property")),
				    new DisplayNameAttribute("DateFilterObjectType")				);

				#endregion // DateFilterObjectStringFilter Properties

				#region FilterValueProxyStringFilter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.FilterValueProxyStringFilter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FilterValueProxyStringFilter Properties

				#region XamGridFilterMenuCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridFilterMenuCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamGridFilterMenuCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamGridFilterMenuCommandSource_Properties"))
				);

				#endregion // XamGridFilterMenuCommandSource Properties

				#region XamGridFilterMenuFilterTextBoxCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridFilterMenuFilterTextBoxCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamGridFilterMenuFilterTextBoxCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamGridFilterMenuFilterTextBoxCommandSource_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FilterSelectionControl",
					new DescriptionAttribute(SR.GetString("XamGridFilterMenuFilterTextBoxCommandSource_FilterSelectionControl_Property")),
				    new DisplayNameAttribute("FilterSelectionControl"),
					new CategoryAttribute(SR.GetString("XamGridFilterMenuFilterTextBoxCommandSource_Properties"))
				);

				#endregion // XamGridFilterMenuFilterTextBoxCommandSource Properties

				#region FilterMenuCommands Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterMenuCommands");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FilterMenuCommands Properties

				#region FilterMenuFilterTextBoxCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterMenuFilterTextBoxCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FilterMenuFilterTextBoxCommand Properties

				#region FilterTextBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterTextBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FilterTextBox Properties

				#region FilterTextBoxWatermarked Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterTextBoxWatermarked");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FilterSelectionControl",
					new DescriptionAttribute(SR.GetString("FilterTextBoxWatermarked_FilterSelectionControl_Property")),
				    new DisplayNameAttribute("FilterSelectionControl")				);


				tableBuilder.AddCustomAttributes(t, "Watermark",
					new DescriptionAttribute(SR.GetString("FilterTextBoxWatermarked_Watermark_Property")),
				    new DisplayNameAttribute("Watermark")				);

				#endregion // FilterTextBoxWatermarked Properties

				#region DateFilterTypeConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.DateFilterTypeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateFilterTypeConverter Properties

				#region FilterMenuXamMenuItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterMenuXamMenuItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsSeparator",
					new DescriptionAttribute(SR.GetString("FilterMenuXamMenuItem_IsSeparator_Property")),
				    new DisplayNameAttribute("IsSeparator")				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("FilterMenuXamMenuItem_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "ParentMenu",
					new DescriptionAttribute(SR.GetString("FilterMenuXamMenuItem_ParentMenu_Property")),
				    new DisplayNameAttribute("ParentMenu")				);

				#endregion // FilterMenuXamMenuItem Properties

				#region DateFilterTreeViewItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.DateFilterTreeViewItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateFilterTreeViewItem Properties

				#region CustomFilterDialogContentControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CustomFilterDialogContentControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CustomFilterDialogContentControl Properties

				#region FilterMenuFilterTextBoxFilterTextCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterMenuFilterTextBoxFilterTextCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FilterMenuFilterTextBoxFilterTextCommand Properties

				#region FilterMenuFilterTextBoxClearFilterTextCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.FilterMenuFilterTextBoxClearFilterTextCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FilterMenuFilterTextBoxClearFilterTextCommand Properties

				#region DateColumnBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.DateColumnBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateColumnBase Properties

				#region ProxyColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ProxyColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ProxyColumnContentProvider Properties

				#region CompoundFilteringDialogFilterMenuCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CompoundFilteringDialogFilterMenuCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ConditionalCommandParameter",
					new DescriptionAttribute(SR.GetString("CompoundFilteringDialogFilterMenuCommandSource_ConditionalCommandParameter_Property")),
				    new DisplayNameAttribute("ConditionalCommandParameter")				);

				#endregion // CompoundFilteringDialogFilterMenuCommandSource Properties

				#region CompoundFilteringDialogCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CompoundFilteringDialogCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("CompoundFilteringDialogCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // CompoundFilteringDialogCommandSource Properties

				#region ShowCompoundFilterDialogFilterMenuConditionalCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ShowCompoundFilterDialogFilterMenuConditionalCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ShowCompoundFilterDialogFilterMenuConditionalCommand Properties

				#region GroupDisplayColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GroupDisplayColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsResizable",
					new DescriptionAttribute(SR.GetString("GroupDisplayColumn_IsResizable_Property")),
				    new DisplayNameAttribute("IsResizable")				);


				tableBuilder.AddCustomAttributes(t, "IsSortable",
					new DescriptionAttribute(SR.GetString("GroupDisplayColumn_IsSortable_Property")),
				    new DisplayNameAttribute("IsSortable")				);


				tableBuilder.AddCustomAttributes(t, "IsFilterable",
					new DescriptionAttribute(SR.GetString("GroupDisplayColumn_IsFilterable_Property")),
				    new DisplayNameAttribute("IsFilterable")				);


				tableBuilder.AddCustomAttributes(t, "AndColorBrush",
					new DescriptionAttribute(SR.GetString("GroupDisplayColumn_AndColorBrush_Property")),
				    new DisplayNameAttribute("AndColorBrush")				);


				tableBuilder.AddCustomAttributes(t, "OrColorBrush",
					new DescriptionAttribute(SR.GetString("GroupDisplayColumn_OrColorBrush_Property")),
				    new DisplayNameAttribute("OrColorBrush")				);

				#endregion // GroupDisplayColumn Properties

				#region XamGridConditionInfoGroup Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridConditionInfoGroup");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Level",
					new DescriptionAttribute(SR.GetString("XamGridConditionInfoGroup_Level_Property")),
				    new DisplayNameAttribute("Level"),
					new CategoryAttribute(SR.GetString("XamGridConditionInfoGroup_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Operator",
					new DescriptionAttribute(SR.GetString("XamGridConditionInfoGroup_Operator_Property")),
				    new DisplayNameAttribute("Operator"),
					new CategoryAttribute(SR.GetString("XamGridConditionInfoGroup_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LevelName",
					new DescriptionAttribute(SR.GetString("XamGridConditionInfoGroup_LevelName_Property")),
				    new DisplayNameAttribute("LevelName"),
					new CategoryAttribute(SR.GetString("XamGridConditionInfoGroup_Properties"))
				);

				#endregion // XamGridConditionInfoGroup Properties

				#region CompoundFilterDialogControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CompoundFilterDialogControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);


				tableBuilder.AddCustomAttributes(t, "CancelButtonText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_CancelButtonText_Property")),
				    new DisplayNameAttribute("CancelButtonText")				);


				tableBuilder.AddCustomAttributes(t, "OKButtonText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_OKButtonText_Property")),
				    new DisplayNameAttribute("OKButtonText")				);


				tableBuilder.AddCustomAttributes(t, "AndButtonText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_AndButtonText_Property")),
				    new DisplayNameAttribute("AndButtonText")				);


				tableBuilder.AddCustomAttributes(t, "OrButtonText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_OrButtonText_Property")),
				    new DisplayNameAttribute("OrButtonText")				);


				tableBuilder.AddCustomAttributes(t, "AddConditionButtonText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_AddConditionButtonText_Property")),
				    new DisplayNameAttribute("AddConditionButtonText")				);


				tableBuilder.AddCustomAttributes(t, "RemoveConditionButtonText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_RemoveConditionButtonText_Property")),
				    new DisplayNameAttribute("RemoveConditionButtonText")				);


				tableBuilder.AddCustomAttributes(t, "ToggleButtonText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_ToggleButtonText_Property")),
				    new DisplayNameAttribute("ToggleButtonText")				);


				tableBuilder.AddCustomAttributes(t, "UngroupButtonText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_UngroupButtonText_Property")),
				    new DisplayNameAttribute("UngroupButtonText")				);


				tableBuilder.AddCustomAttributes(t, "AndColorLabelText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_AndColorLabelText_Property")),
				    new DisplayNameAttribute("AndColorLabelText")				);


				tableBuilder.AddCustomAttributes(t, "OrColorLabelText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_OrColorLabelText_Property")),
				    new DisplayNameAttribute("OrColorLabelText")				);


				tableBuilder.AddCustomAttributes(t, "SelectedGroupText",
					new DescriptionAttribute(SR.GetString("CompoundFilterDialogControl_SelectedGroupText_Property")),
				    new DisplayNameAttribute("SelectedGroupText")				);

				#endregion // CompoundFilterDialogControl Properties

				#region CompoundFilteringDialogControlCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CompoundFilteringDialogControlCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("CompoundFilteringDialogControlCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // CompoundFilteringDialogControlCommandSource Properties

				#region CompoundFilterDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CompoundFilterDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CompoundFilterDialogCommand Properties

				#region ShowCompoundFilterDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.ShowCompoundFilterDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ShowCompoundFilterDialogCommand Properties

				#region AcceptCompoundFilterDialogChangesCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.AcceptCompoundFilterDialogChangesCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AcceptCompoundFilterDialogChangesCommand Properties

				#region CloseCompoundFilterDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.CloseCompoundFilterDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CloseCompoundFilterDialogCommand Properties

				#region GroupDisplayCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupDisplayCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupDisplayCell Properties

				#region XamGridConditionInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.XamGridConditionInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Group",
					new DescriptionAttribute(SR.GetString("XamGridConditionInfo_Group_Property")),
				    new DisplayNameAttribute("Group"),
					new CategoryAttribute(SR.GetString("XamGridConditionInfo_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FilterOperand",
					new DescriptionAttribute(SR.GetString("XamGridConditionInfo_FilterOperand_Property")),
				    new DisplayNameAttribute("FilterOperand"),
					new CategoryAttribute(SR.GetString("XamGridConditionInfo_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FilterValue",
					new DescriptionAttribute(SR.GetString("XamGridConditionInfo_FilterValue_Property")),
				    new DisplayNameAttribute("FilterValue"),
					new CategoryAttribute(SR.GetString("XamGridConditionInfo_Properties"))
				);

				#endregion // XamGridConditionInfo Properties

				#region ConditionGroup Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ConditionGroup");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Conditions",
					new DescriptionAttribute(SR.GetString("ConditionGroup_Conditions_Property")),
				    new DisplayNameAttribute("Conditions")				);

				#endregion // ConditionGroup Properties

				#region GroupDisplayCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.GroupDisplayCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InnerControlMargin",
					new DescriptionAttribute(SR.GetString("GroupDisplayCellControl_InnerControlMargin_Property")),
				    new DisplayNameAttribute("InnerControlMargin")				);

				#endregion // GroupDisplayCellControl Properties

				#region ClipboardPasteErrorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ClipboardPasteErrorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ErrorType",
					new DescriptionAttribute(SR.GetString("ClipboardPasteErrorEventArgs_ErrorType_Property")),
				    new DisplayNameAttribute("ErrorType")				);


				tableBuilder.AddCustomAttributes(t, "IsRecoverable",
					new DescriptionAttribute(SR.GetString("ClipboardPasteErrorEventArgs_IsRecoverable_Property")),
				    new DisplayNameAttribute("IsRecoverable")				);


				tableBuilder.AddCustomAttributes(t, "AttemptRecover",
					new DescriptionAttribute(SR.GetString("ClipboardPasteErrorEventArgs_AttemptRecover_Property")),
				    new DisplayNameAttribute("AttemptRecover")				);

				#endregion // ClipboardPasteErrorEventArgs Properties

				#region TemplateColumnEditorContentPresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.TemplateColumnEditorContentPresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TemplateColumnEditorContentPresenter Properties

				#region GroupDisplayColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.GroupDisplayColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GroupDisplayColumnContentProvider Properties

				#region PagerItemsControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagerItemsControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PagerItemsControl Properties

				#region PagerItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.PagerItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PagerItem Properties

				#region HeaderDropDownContentRootPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.HeaderDropDownContentRootPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HeaderDropDownContentRootPanel Properties

				#region NullableColumnWidthTypeConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.Primitives.NullableColumnWidthTypeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NullableColumnWidthTypeConverter Properties

				#region ColumnVisibilityChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Grids.ColumnVisibilityChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("ColumnVisibilityChangedEventArgs_Column_Property")),
				    new DisplayNameAttribute("Column")				);

				#endregion // ColumnVisibilityChangedEventArgs Properties
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