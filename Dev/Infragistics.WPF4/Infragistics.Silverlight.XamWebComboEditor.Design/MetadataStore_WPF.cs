using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Editors.XamComboEditor.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Editors.XamComboEditor.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Editors.XamComboEditor);
				Assembly controlAssembly = t.Assembly;

				#region ItemsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ItemsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComboEditor",
					new DescriptionAttribute(SR.GetString("ItemsPanel_ComboEditor_Property")),
				    new DisplayNameAttribute("ComboEditor")				);


				tableBuilder.AddCustomAttributes(t, "ComboEditor",
					new DescriptionAttribute(SR.GetString("ItemsPanelBase`2_ComboEditor_Property")),
				    new DisplayNameAttribute("ComboEditor")				);


				tableBuilder.AddCustomAttributes(t, "FixedRowsTop",
					new DescriptionAttribute(SR.GetString("ItemsPanelBase`2_FixedRowsTop_Property")),
				    new DisplayNameAttribute("FixedRowsTop")				);

				#endregion // ItemsPanel Properties

				#region ComboEditorItemControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboEditorItemControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamComboEditorSupportAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamComboEditorSupportAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ComboEditorItemControl_Item_Property")),
				    new DisplayNameAttribute("Item")				);

				#endregion // ComboEditorItemControl Properties

				#region XamComboEditor Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamComboEditor");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamComboEditorAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamComboEditorAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowFiltering",
					new DescriptionAttribute(SR.GetString("XamComboEditor_AllowFiltering_Property")),
				    new DisplayNameAttribute("AllowFiltering"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsEditable",
					new DescriptionAttribute(SR.GetString("XamComboEditor_IsEditable_Property")),
				    new DisplayNameAttribute("IsEditable"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AutoComplete",
					new DescriptionAttribute(SR.GetString("XamComboEditor_AutoComplete_Property")),
				    new DisplayNameAttribute("AutoComplete"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OpenDropDownOnTyping",
					new DescriptionAttribute(SR.GetString("XamComboEditor_OpenDropDownOnTyping_Property")),
				    new DisplayNameAttribute("OpenDropDownOnTyping"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDropDownOpen",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_IsDropDownOpen_Property")),
				    new DisplayNameAttribute("IsDropDownOpen"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemContainerStyle",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_ItemContainerStyle_Property")),
				    new DisplayNameAttribute("ItemContainerStyle"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxDropDownHeight",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_MaxDropDownHeight_Property")),
				    new DisplayNameAttribute("MaxDropDownHeight"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedIndex",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_SelectedIndex_Property")),
				    new DisplayNameAttribute("SelectedIndex"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedItem",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_SelectedItem_Property")),
				    new DisplayNameAttribute("SelectedItem"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedItems",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_SelectedItems_Property")),
				    new DisplayNameAttribute("SelectedItems"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CustomItemsFilter",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_CustomItemsFilter_Property")),
				    new DisplayNameAttribute("CustomItemsFilter"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplate",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_ItemTemplate_Property")),
				    new DisplayNameAttribute("ItemTemplate"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayMemberPath",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_DisplayMemberPath_Property")),
				    new DisplayNameAttribute("DisplayMemberPath"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Items",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_Items_Property")),
				    new DisplayNameAttribute("Items"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowMultipleSelection",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_AllowMultipleSelection_Property")),
				    new DisplayNameAttribute("AllowMultipleSelection"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxVisibility",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_CheckBoxVisibility_Property")),
				    new DisplayNameAttribute("CheckBoxVisibility"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MultiSelectValueDelimiter",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_MultiSelectValueDelimiter_Property")),
				    new DisplayNameAttribute("MultiSelectValueDelimiter"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AutoCompleteDelay",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_AutoCompleteDelay_Property")),
				    new DisplayNameAttribute("AutoCompleteDelay"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CustomValueEnteredAction",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_CustomValueEnteredAction_Property")),
				    new DisplayNameAttribute("CustomValueEnteredAction"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EmptyText",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_EmptyText_Property")),
				    new DisplayNameAttribute("EmptyText"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EditAreaBackground",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_EditAreaBackground_Property")),
				    new DisplayNameAttribute("EditAreaBackground")				);


				tableBuilder.AddCustomAttributes(t, "IsTouchSupportEnabled",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_IsTouchSupportEnabled_Property")),
				    new DisplayNameAttribute("IsTouchSupportEnabled")				);

				#endregion // XamComboEditor Properties

				#region ComboEditorItemCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboEditorItemCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ComboEditorItemCollection Properties

				#region StringToCharTypeConverter Properties
				t = controlAssembly.GetType("Infragistics.StringToCharTypeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StringToCharTypeConverter Properties

				#region ComboEditorItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboEditorItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComboEditor",
					new DescriptionAttribute(SR.GetString("ComboEditorItem_ComboEditor_Property")),
				    new DisplayNameAttribute("ComboEditor")				);


				tableBuilder.AddCustomAttributes(t, "Data",
					new DescriptionAttribute(SR.GetString("ComboEditorItem_Data_Property")),
				    new DisplayNameAttribute("Data")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("ComboEditorItem_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "IsFocused",
					new DescriptionAttribute(SR.GetString("ComboEditorItem_IsFocused_Property")),
				    new DisplayNameAttribute("IsFocused")				);


				tableBuilder.AddCustomAttributes(t, "Control",
					new DescriptionAttribute(SR.GetString("ComboEditorItem_Control_Property")),
				    new DisplayNameAttribute("Control")				);


				tableBuilder.AddCustomAttributes(t, "IsEnabled",
					new DescriptionAttribute(SR.GetString("ComboEditorItem_IsEnabled_Property")),
				    new DisplayNameAttribute("IsEnabled")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("ComboEditorItem_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "Data",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_Data_Property")),
				    new DisplayNameAttribute("Data")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "IsFocused",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_IsFocused_Property")),
				    new DisplayNameAttribute("IsFocused")				);


				tableBuilder.AddCustomAttributes(t, "Control",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_Control_Property")),
				    new DisplayNameAttribute("Control")				);


				tableBuilder.AddCustomAttributes(t, "IsEnabled",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_IsEnabled_Property")),
				    new DisplayNameAttribute("IsEnabled")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_Style_Property")),
				    new DisplayNameAttribute("Style")				);

				#endregion // ComboEditorItem Properties

				#region ItemsFilter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ItemsFilter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Conditions",
					new DescriptionAttribute(SR.GetString("ItemsFilter_Conditions_Property")),
				    new DisplayNameAttribute("Conditions")				);


				tableBuilder.AddCustomAttributes(t, "ObjectType",
					new DescriptionAttribute(SR.GetString("ItemsFilter_ObjectType_Property")),
				    new DisplayNameAttribute("ObjectType")				);


				tableBuilder.AddCustomAttributes(t, "FieldName",
					new DescriptionAttribute(SR.GetString("ItemsFilter_FieldName_Property")),
				    new DisplayNameAttribute("FieldName")				);


				tableBuilder.AddCustomAttributes(t, "FieldType",
					new DescriptionAttribute(SR.GetString("ItemsFilter_FieldType_Property")),
				    new DisplayNameAttribute("FieldType")				);


				tableBuilder.AddCustomAttributes(t, "ObjectTypedInfo",
					new DescriptionAttribute(SR.GetString("ItemsFilter_ObjectTypedInfo_Property")),
				    new DisplayNameAttribute("ObjectTypedInfo")				);

				#endregion // ItemsFilter Properties

				#region SpecializedTextBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.SpecializedTextBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SpecializedTextBox Properties

				#region CheckboxComboColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CheckboxComboColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CheckboxComboColumnContentProvider Properties

				#region ComboColumnContentProviderBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ComboColumnContentProviderBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ComboColumnContentProviderBase Properties

				#region HighlightingTextBlock Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.HighlightingTextBlock");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Text",
					new DescriptionAttribute(SR.GetString("HighlightingTextBlock_Text_Property")),
				    new DisplayNameAttribute("Text")				);

				#endregion // HighlightingTextBlock Properties

				#region ComboColumnCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboColumnCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FixedAdornerColumns",
					new DescriptionAttribute(SR.GetString("ComboColumnCollection_FixedAdornerColumns_Property")),
				    new DisplayNameAttribute("FixedAdornerColumns")				);


				tableBuilder.AddCustomAttributes(t, "FillerColumn",
					new DescriptionAttribute(SR.GetString("ComboColumnCollection_FillerColumn_Property")),
				    new DisplayNameAttribute("FillerColumn")				);

				#endregion // ComboColumnCollection Properties

				#region ComboCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ContentProvider",
					new DescriptionAttribute(SR.GetString("ComboCellControl_ContentProvider_Property")),
				    new DisplayNameAttribute("ContentProvider")				);

				#endregion // ComboCellControl Properties

				#region ComboCellControlBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboCellControlBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ContentProvider",
					new DescriptionAttribute(SR.GetString("ComboCellControlBase_ContentProvider_Property")),
				    new DisplayNameAttribute("ContentProvider")				);


				tableBuilder.AddCustomAttributes(t, "Cell",
					new DescriptionAttribute(SR.GetString("ComboCellControlBase_Cell_Property")),
				    new DisplayNameAttribute("Cell")				);

				#endregion // ComboCellControlBase Properties

				#region TextComboColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.TextComboColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TextComboColumnContentProvider Properties

				#region ItemsPanelBase`2 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ItemsPanelBase`2");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComboEditor",
					new DescriptionAttribute(SR.GetString("ItemsPanelBase`2_ComboEditor_Property")),
				    new DisplayNameAttribute("ComboEditor")				);


				tableBuilder.AddCustomAttributes(t, "FixedRowsTop",
					new DescriptionAttribute(SR.GetString("ItemsPanelBase`2_FixedRowsTop_Property")),
				    new DisplayNameAttribute("FixedRowsTop")				);

				#endregion // ItemsPanelBase`2 Properties

				#region ComboEditorItemBase`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboEditorItemBase`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Data",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_Data_Property")),
				    new DisplayNameAttribute("Data")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "IsFocused",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_IsFocused_Property")),
				    new DisplayNameAttribute("IsFocused")				);


				tableBuilder.AddCustomAttributes(t, "Control",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_Control_Property")),
				    new DisplayNameAttribute("Control")				);


				tableBuilder.AddCustomAttributes(t, "IsEnabled",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_IsEnabled_Property")),
				    new DisplayNameAttribute("IsEnabled")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_Style_Property")),
				    new DisplayNameAttribute("Style")				);

				#endregion // ComboEditorItemBase`1 Properties

				#region MultiColumnComboItemsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.MultiColumnComboItemsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FixedRowsTop",
					new DescriptionAttribute(SR.GetString("MultiColumnComboItemsPanel_FixedRowsTop_Property")),
				    new DisplayNameAttribute("FixedRowsTop")				);


				tableBuilder.AddCustomAttributes(t, "ComboEditor",
					new DescriptionAttribute(SR.GetString("ItemsPanelBase`2_ComboEditor_Property")),
				    new DisplayNameAttribute("ComboEditor")				);


				tableBuilder.AddCustomAttributes(t, "FixedRowsTop",
					new DescriptionAttribute(SR.GetString("ItemsPanelBase`2_FixedRowsTop_Property")),
				    new DisplayNameAttribute("FixedRowsTop")				);

				#endregion // MultiColumnComboItemsPanel Properties

				#region TextComboColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.TextComboColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TextBlockStyle",
					new DescriptionAttribute(SR.GetString("TextComboColumn_TextBlockStyle_Property")),
				    new DisplayNameAttribute("TextBlockStyle")				);


				tableBuilder.AddCustomAttributes(t, "TextWrapping",
					new DescriptionAttribute(SR.GetString("TextComboColumn_TextWrapping_Property")),
				    new DisplayNameAttribute("TextWrapping")				);


				tableBuilder.AddCustomAttributes(t, "FormatString",
					new DescriptionAttribute(SR.GetString("TextComboColumn_FormatString_Property")),
				    new DisplayNameAttribute("FormatString")				);

				#endregion // TextComboColumn Properties

				#region ComboColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Key",
					new DescriptionAttribute(SR.GetString("ComboColumn_Key_Property")),
				    new DisplayNameAttribute("Key")				);


				tableBuilder.AddCustomAttributes(t, "DataType",
					new DescriptionAttribute(SR.GetString("ComboColumn_DataType_Property")),
				    new DisplayNameAttribute("DataType")				);


				tableBuilder.AddCustomAttributes(t, "IsAutoGenerated",
					new DescriptionAttribute(SR.GetString("ComboColumn_IsAutoGenerated_Property")),
				    new DisplayNameAttribute("IsAutoGenerated")				);


				tableBuilder.AddCustomAttributes(t, "ValueConverter",
					new DescriptionAttribute(SR.GetString("ComboColumn_ValueConverter_Property")),
				    new DisplayNameAttribute("ValueConverter")				);


				tableBuilder.AddCustomAttributes(t, "ValueConverterParameter",
					new DescriptionAttribute(SR.GetString("ComboColumn_ValueConverterParameter_Property")),
				    new DisplayNameAttribute("ValueConverterParameter")				);


				tableBuilder.AddCustomAttributes(t, "CellStyle",
					new DescriptionAttribute(SR.GetString("ComboColumn_CellStyle_Property")),
				    new DisplayNameAttribute("CellStyle")				);


				tableBuilder.AddCustomAttributes(t, "ActualWidth",
					new DescriptionAttribute(SR.GetString("ComboColumn_ActualWidth_Property")),
				    new DisplayNameAttribute("ActualWidth")				);


				tableBuilder.AddCustomAttributes(t, "MinimumWidth",
					new DescriptionAttribute(SR.GetString("ComboColumn_MinimumWidth_Property")),
				    new DisplayNameAttribute("MinimumWidth")				);


				tableBuilder.AddCustomAttributes(t, "MaximumWidth",
					new DescriptionAttribute(SR.GetString("ComboColumn_MaximumWidth_Property")),
				    new DisplayNameAttribute("MaximumWidth")				);


				tableBuilder.AddCustomAttributes(t, "HeaderText",
					new DescriptionAttribute(SR.GetString("ComboColumn_HeaderText_Property")),
				    new DisplayNameAttribute("HeaderText")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTextResolved",
					new DescriptionAttribute(SR.GetString("ComboColumn_HeaderTextResolved_Property")),
				    new DisplayNameAttribute("HeaderTextResolved")				);


				tableBuilder.AddCustomAttributes(t, "HeaderTemplate",
					new DescriptionAttribute(SR.GetString("ComboColumn_HeaderTemplate_Property")),
				    new DisplayNameAttribute("HeaderTemplate")				);


				tableBuilder.AddCustomAttributes(t, "HeaderStyle",
					new DescriptionAttribute(SR.GetString("ComboColumn_HeaderStyle_Property")),
				    new DisplayNameAttribute("HeaderStyle")				);


				tableBuilder.AddCustomAttributes(t, "Visibility",
					new DescriptionAttribute(SR.GetString("ComboColumn_Visibility_Property")),
				    new DisplayNameAttribute("Visibility")				);


				tableBuilder.AddCustomAttributes(t, "Width",
					new DescriptionAttribute(SR.GetString("ComboColumn_Width_Property")),
				    new DisplayNameAttribute("Width")				);

				#endregion // ComboColumn Properties

				#region RowSelectionCheckBoxColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.RowSelectionCheckBoxColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowSelectionCheckBoxColumn Properties

				#region ComboCellBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboCellBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("ComboCellBase_Row_Property")),
				    new DisplayNameAttribute("Row")				);


				tableBuilder.AddCustomAttributes(t, "Column",
					new DescriptionAttribute(SR.GetString("ComboCellBase_Column_Property")),
				    new DisplayNameAttribute("Column")				);


				tableBuilder.AddCustomAttributes(t, "Control",
					new DescriptionAttribute(SR.GetString("ComboCellBase_Control_Property")),
				    new DisplayNameAttribute("Control")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("ComboCellBase_Style_Property")),
				    new DisplayNameAttribute("Style")				);

				#endregion // ComboCellBase Properties

				#region ComboItemAddingEventArgs`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboItemAddingEventArgs`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ComboItemAddingEventArgs`1_Item_Property")),
				    new DisplayNameAttribute("Item")				);

				#endregion // ComboItemAddingEventArgs`1 Properties

				#region ComboItemAddedEventArgs`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboItemAddedEventArgs`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ComboItemAddedEventArgs`1_Item_Property")),
				    new DisplayNameAttribute("Item")				);

				#endregion // ComboItemAddedEventArgs`1 Properties

				#region XamComboEditorAutomationPeer`2 Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamComboEditorAutomationPeer`2");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CanSelectMultiple",
					new DescriptionAttribute(SR.GetString("XamComboEditorAutomationPeer`2_CanSelectMultiple_Property")),
				    new DisplayNameAttribute("CanSelectMultiple"),
					new CategoryAttribute(SR.GetString("XamComboEditorAutomationPeer`2_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionRequired",
					new DescriptionAttribute(SR.GetString("XamComboEditorAutomationPeer`2_IsSelectionRequired_Property")),
				    new DisplayNameAttribute("IsSelectionRequired"),
					new CategoryAttribute(SR.GetString("XamComboEditorAutomationPeer`2_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ExpandCollapseState",
					new DescriptionAttribute(SR.GetString("XamComboEditorAutomationPeer`2_ExpandCollapseState_Property")),
				    new DisplayNameAttribute("ExpandCollapseState"),
					new CategoryAttribute(SR.GetString("XamComboEditorAutomationPeer`2_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("XamComboEditorAutomationPeer`2_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly"),
					new CategoryAttribute(SR.GetString("XamComboEditorAutomationPeer`2_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamComboEditorAutomationPeer`2_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamComboEditorAutomationPeer`2_Properties"))
				);

				#endregion // XamComboEditorAutomationPeer`2 Properties

				#region CheckboxComboColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.CheckboxComboColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CheckboxComboColumn Properties

				#region MultiColumnComboEditorCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.MultiColumnComboEditorCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MultiColumnComboEditorCommandBase Properties

				#region MultiColumnComboEditorCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.MultiColumnComboEditorCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("MultiColumnComboEditorCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // MultiColumnComboEditorCommandSource Properties

				#region MultiColumnComboEditorClearSelectionCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.MultiColumnComboEditorClearSelectionCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MultiColumnComboEditorClearSelectionCommand Properties

				#region DateComboColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.DateComboColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateComboColumn Properties

				#region RowSelectionCheckBoxCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.RowSelectionCheckBoxCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowSelectionCheckBoxCellControl Properties

				#region RowSelectionCheckBoxCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.RowSelectionCheckBoxCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowSelectionCheckBoxCell Properties

				#region ComboCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ComboCell Properties

				#region ComboCellsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ComboCellsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ComboCellsCollection Properties

				#region ComboEditorBase`2 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboEditorBase`2");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsDropDownOpen",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_IsDropDownOpen_Property")),
				    new DisplayNameAttribute("IsDropDownOpen"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemContainerStyle",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_ItemContainerStyle_Property")),
				    new DisplayNameAttribute("ItemContainerStyle"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxDropDownHeight",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_MaxDropDownHeight_Property")),
				    new DisplayNameAttribute("MaxDropDownHeight"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedIndex",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_SelectedIndex_Property")),
				    new DisplayNameAttribute("SelectedIndex"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedItem",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_SelectedItem_Property")),
				    new DisplayNameAttribute("SelectedItem"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedItems",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_SelectedItems_Property")),
				    new DisplayNameAttribute("SelectedItems"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CustomItemsFilter",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_CustomItemsFilter_Property")),
				    new DisplayNameAttribute("CustomItemsFilter"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplate",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_ItemTemplate_Property")),
				    new DisplayNameAttribute("ItemTemplate"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayMemberPath",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_DisplayMemberPath_Property")),
				    new DisplayNameAttribute("DisplayMemberPath"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Items",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_Items_Property")),
				    new DisplayNameAttribute("Items"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowMultipleSelection",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_AllowMultipleSelection_Property")),
				    new DisplayNameAttribute("AllowMultipleSelection"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxVisibility",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_CheckBoxVisibility_Property")),
				    new DisplayNameAttribute("CheckBoxVisibility"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MultiSelectValueDelimiter",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_MultiSelectValueDelimiter_Property")),
				    new DisplayNameAttribute("MultiSelectValueDelimiter"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AutoCompleteDelay",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_AutoCompleteDelay_Property")),
				    new DisplayNameAttribute("AutoCompleteDelay"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CustomValueEnteredAction",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_CustomValueEnteredAction_Property")),
				    new DisplayNameAttribute("CustomValueEnteredAction"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EmptyText",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_EmptyText_Property")),
				    new DisplayNameAttribute("EmptyText"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EditAreaBackground",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_EditAreaBackground_Property")),
				    new DisplayNameAttribute("EditAreaBackground")				);


				tableBuilder.AddCustomAttributes(t, "IsTouchSupportEnabled",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_IsTouchSupportEnabled_Property")),
				    new DisplayNameAttribute("IsTouchSupportEnabled")				);

				#endregion // ComboEditorBase`2 Properties

				#region ReadOnlyKeyedComboColumnCollection`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ReadOnlyKeyedComboColumnCollection`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ReadOnlyKeyedComboColumnCollection`1 Properties

				#region XamMultiColumnComboEditor Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamMultiColumnComboEditor");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamComboEditorAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamComboEditorAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowDropDownResizing",
					new DescriptionAttribute(SR.GetString("XamMultiColumnComboEditor_AllowDropDownResizing_Property")),
				    new DisplayNameAttribute("AllowDropDownResizing"),
					new CategoryAttribute(SR.GetString("XamMultiColumnComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AutoGenerateColumns",
					new DescriptionAttribute(SR.GetString("XamMultiColumnComboEditor_AutoGenerateColumns_Property")),
				    new DisplayNameAttribute("AutoGenerateColumns"),
					new CategoryAttribute(SR.GetString("XamMultiColumnComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Columns",
					new DescriptionAttribute(SR.GetString("XamMultiColumnComboEditor_Columns_Property")),
				    new DisplayNameAttribute("Columns"),
					new CategoryAttribute(SR.GetString("XamMultiColumnComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnTypeMappings",
					new DescriptionAttribute(SR.GetString("XamMultiColumnComboEditor_ColumnTypeMappings_Property")),
				    new DisplayNameAttribute("ColumnTypeMappings"),
					new CategoryAttribute(SR.GetString("XamMultiColumnComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FilterMode",
					new DescriptionAttribute(SR.GetString("XamMultiColumnComboEditor_FilterMode_Property")),
				    new DisplayNameAttribute("FilterMode"),
					new CategoryAttribute(SR.GetString("XamMultiColumnComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Footer",
					new DescriptionAttribute(SR.GetString("XamMultiColumnComboEditor_Footer_Property")),
				    new DisplayNameAttribute("Footer"),
					new CategoryAttribute(SR.GetString("XamMultiColumnComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FooterTemplate",
					new DescriptionAttribute(SR.GetString("XamMultiColumnComboEditor_FooterTemplate_Property")),
				    new DisplayNameAttribute("FooterTemplate"),
					new CategoryAttribute(SR.GetString("XamMultiColumnComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("XamMultiColumnComboEditor_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamMultiColumnComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedItemsResetButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamMultiColumnComboEditor_SelectedItemsResetButtonVisibility_Property")),
				    new DisplayNameAttribute("SelectedItemsResetButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamMultiColumnComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDropDownOpen",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_IsDropDownOpen_Property")),
				    new DisplayNameAttribute("IsDropDownOpen"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemContainerStyle",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_ItemContainerStyle_Property")),
				    new DisplayNameAttribute("ItemContainerStyle"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxDropDownHeight",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_MaxDropDownHeight_Property")),
				    new DisplayNameAttribute("MaxDropDownHeight"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedIndex",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_SelectedIndex_Property")),
				    new DisplayNameAttribute("SelectedIndex"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedItem",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_SelectedItem_Property")),
				    new DisplayNameAttribute("SelectedItem"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedItems",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_SelectedItems_Property")),
				    new DisplayNameAttribute("SelectedItems"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CustomItemsFilter",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_CustomItemsFilter_Property")),
				    new DisplayNameAttribute("CustomItemsFilter"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemTemplate",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_ItemTemplate_Property")),
				    new DisplayNameAttribute("ItemTemplate"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayMemberPath",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_DisplayMemberPath_Property")),
				    new DisplayNameAttribute("DisplayMemberPath"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Items",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_Items_Property")),
				    new DisplayNameAttribute("Items"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowMultipleSelection",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_AllowMultipleSelection_Property")),
				    new DisplayNameAttribute("AllowMultipleSelection"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CheckBoxVisibility",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_CheckBoxVisibility_Property")),
				    new DisplayNameAttribute("CheckBoxVisibility"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MultiSelectValueDelimiter",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_MultiSelectValueDelimiter_Property")),
				    new DisplayNameAttribute("MultiSelectValueDelimiter"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AutoCompleteDelay",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_AutoCompleteDelay_Property")),
				    new DisplayNameAttribute("AutoCompleteDelay"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CustomValueEnteredAction",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_CustomValueEnteredAction_Property")),
				    new DisplayNameAttribute("CustomValueEnteredAction"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EmptyText",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_EmptyText_Property")),
				    new DisplayNameAttribute("EmptyText"),
					new CategoryAttribute(SR.GetString("XamComboEditor_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EditAreaBackground",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_EditAreaBackground_Property")),
				    new DisplayNameAttribute("EditAreaBackground")				);


				tableBuilder.AddCustomAttributes(t, "IsTouchSupportEnabled",
					new DescriptionAttribute(SR.GetString("ComboEditorBase`2_IsTouchSupportEnabled_Property")),
				    new DisplayNameAttribute("IsTouchSupportEnabled")				);

				#endregion // XamMultiColumnComboEditor Properties

				#region ComboCellsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ComboCellsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Row",
					new DescriptionAttribute(SR.GetString("ComboCellsPanel_Row_Property")),
				    new DisplayNameAttribute("Row")				);


				tableBuilder.AddCustomAttributes(t, "Owner",
					new DescriptionAttribute(SR.GetString("ComboCellsPanel_Owner_Property")),
				    new DisplayNameAttribute("Owner")				);

				#endregion // ComboCellsPanel Properties

				#region ComboColumnTypeMapping Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboColumnTypeMapping");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ColumnType",
					new DescriptionAttribute(SR.GetString("ComboColumnTypeMapping_ColumnType_Property")),
				    new DisplayNameAttribute("ColumnType")				);


				tableBuilder.AddCustomAttributes(t, "DataType",
					new DescriptionAttribute(SR.GetString("ComboColumnTypeMapping_DataType_Property")),
				    new DisplayNameAttribute("DataType")				);

				#endregion // ComboColumnTypeMapping Properties

				#region ComboHeaderCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboHeaderCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ComboHeaderCell Properties

				#region ComboEditorItemControlAutomationPeer`2 Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.ComboEditorItemControlAutomationPeer`2");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("ComboEditorItemControlAutomationPeer`2_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("ComboEditorItemControlAutomationPeer`2_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("ComboEditorItemControlAutomationPeer`2_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "SelectionContainer",
					new DescriptionAttribute(SR.GetString("ComboEditorItemControlAutomationPeer`2_SelectionContainer_Property")),
				    new DisplayNameAttribute("SelectionContainer")				);

				#endregion // ComboEditorItemControlAutomationPeer`2 Properties

				#region ComboRowBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboRowBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ActualHeight",
					new DescriptionAttribute(SR.GetString("ComboRowBase_ActualHeight_Property")),
				    new DisplayNameAttribute("ActualHeight")				);


				tableBuilder.AddCustomAttributes(t, "Cells",
					new DescriptionAttribute(SR.GetString("ComboRowBase_Cells_Property")),
				    new DisplayNameAttribute("Cells")				);


				tableBuilder.AddCustomAttributes(t, "ComboEditor",
					new DescriptionAttribute(SR.GetString("ComboRowBase_ComboEditor_Property")),
				    new DisplayNameAttribute("ComboEditor")				);


				tableBuilder.AddCustomAttributes(t, "CellStyle",
					new DescriptionAttribute(SR.GetString("ComboRowBase_CellStyle_Property")),
				    new DisplayNameAttribute("CellStyle")				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("ComboRowBase_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);


				tableBuilder.AddCustomAttributes(t, "IsMouseOver",
					new DescriptionAttribute(SR.GetString("ComboRowBase_IsMouseOver_Property")),
				    new DisplayNameAttribute("IsMouseOver")				);


				tableBuilder.AddCustomAttributes(t, "Data",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_Data_Property")),
				    new DisplayNameAttribute("Data")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "IsFocused",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_IsFocused_Property")),
				    new DisplayNameAttribute("IsFocused")				);


				tableBuilder.AddCustomAttributes(t, "Control",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_Control_Property")),
				    new DisplayNameAttribute("Control")				);


				tableBuilder.AddCustomAttributes(t, "IsEnabled",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_IsEnabled_Property")),
				    new DisplayNameAttribute("IsEnabled")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("ComboEditorItemBase`1_Style_Property")),
				    new DisplayNameAttribute("Style")				);

				#endregion // ComboRowBase Properties

				#region ImageComboColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ImageComboColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ImageComboColumnContentProvider Properties

				#region DateComboColumnContentProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.DateComboColumnContentProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateComboColumnContentProvider Properties

				#region FixedComboColumnsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.FixedComboColumnsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FixedComboColumnsCollection Properties

				#region ComboHeaderRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboHeaderRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("ComboHeaderRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);

				#endregion // ComboHeaderRow Properties

				#region ImageComboColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ImageComboColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ImageStyle",
					new DescriptionAttribute(SR.GetString("ImageComboColumn_ImageStyle_Property")),
				    new DisplayNameAttribute("ImageStyle")				);


				tableBuilder.AddCustomAttributes(t, "ImageHeight",
					new DescriptionAttribute(SR.GetString("ImageComboColumn_ImageHeight_Property")),
				    new DisplayNameAttribute("ImageHeight")				);


				tableBuilder.AddCustomAttributes(t, "ImageWidth",
					new DescriptionAttribute(SR.GetString("ImageComboColumn_ImageWidth_Property")),
				    new DisplayNameAttribute("ImageWidth")				);

				#endregion // ImageComboColumn Properties

				#region RowSelectionCheckBoxHeaderCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.RowSelectionCheckBoxHeaderCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowSelectionCheckBoxHeaderCellControl Properties

				#region ComboRow Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboRow");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RowType",
					new DescriptionAttribute(SR.GetString("ComboRow_RowType_Property")),
				    new DisplayNameAttribute("RowType")				);

				#endregion // ComboRow Properties

				#region FillerComboColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.FillerComboColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FillerComboColumn Properties

				#region ComboColumnTypeMappingsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboColumnTypeMappingsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ComboColumnTypeMappingsCollection Properties

				#region RowSelectionCheckBoxHeaderCell Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.RowSelectionCheckBoxHeaderCell");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RowSelectionCheckBoxHeaderCell Properties

				#region ComboHeaderCellControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ComboHeaderCellControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ComboHeaderCellControl Properties

				#region ComboCellsPanelAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.ComboCellsPanelAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("ComboCellsPanelAutomationPeer_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("ComboCellsPanelAutomationPeer_Value_Property")),
				    new DisplayNameAttribute("Value")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("ComboCellsPanelAutomationPeer_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "SelectionContainer",
					new DescriptionAttribute(SR.GetString("ComboCellsPanelAutomationPeer_SelectionContainer_Property")),
				    new DisplayNameAttribute("SelectionContainer")				);

				#endregion // ComboCellsPanelAutomationPeer Properties

				#region ComboColumnWidthTypeConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ComboColumnWidthTypeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ComboColumnWidthTypeConverter Properties
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