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

namespace Infragistics.Windows.Design.Editors
{

	// JM 01-06-10 VS2010 Designer Support
	#region DesignMetadataHelper Static Class

	internal static class DesignMetadataHelper
	{
		internal static AttributeTableBuilder GetAttributeTableBuilder()
		{
			AttributeTableBuilder builder = new AttributeTableBuilder();

			#region Description/Category


			// Infragistics.Windows.Editors.SectionPresenter
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.SectionPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_SectionPresenter"));
			});

			// Infragistics.Windows.Editors.DisplayCharacterPresenter
			// ======================================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.DisplayCharacterPresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_DisplayCharacterPresenter"));
			});

			// Infragistics.Windows.Editors.SectionsList
			// =========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.SectionsList), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_SectionsList"));
			});

			// Infragistics.Windows.Editors.DisplayCharactersList
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.DisplayCharactersList), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_DisplayCharactersList"));
			});

			// Infragistics.Windows.Editors.CaretElement
			// =========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CaretElement), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_CaretElement"));

				callbackBuilder.AddCustomAttributes("BlinkVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_CaretElement_P_BlinkVisibility"));
			});


			// Infragistics.Windows.Editors.ValueEditor
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.ValueEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_ValueEditor"));

				// JM 01-06-10 - The new VS2010 AttributeTableBuilder does not have an override that takes a 
				// so use the overload that takes the member name as a string.
				// JM 9/19/08 [BR29911 TFS5830]
				//callbackBuilder.AddCustomAttributes(Infragistics.Windows.Editors.ValueEditor.ValueProperty, new TypeConverterAttribute(typeof(StringConverter)));
				callbackBuilder.AddCustomAttributes("Value", new TypeConverterAttribute(typeof(StringConverter)));

				callbackBuilder.AddCustomAttributes("Value", CreateCategory("LC_Data"), CreateDescription("LD_ValueEditor_P_Value"));
				callbackBuilder.AddCustomAttributes("Text", CreateCategory("LC_Data"), CreateDescription("LD_ValueEditor_P_Text"));
				callbackBuilder.AddCustomAttributes("ValueType", CreateCategory("LC_Data"), CreateDescription("LD_ValueEditor_P_ValueType"));
				callbackBuilder.AddCustomAttributes("ValueConstraint", CreateCategory("LC_Data"), CreateDescription("LD_ValueEditor_P_ValueConstraint"));
				callbackBuilder.AddCustomAttributes("InvalidValueBehavior", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_InvalidValueBehavior"));
				callbackBuilder.AddCustomAttributes("ValueToTextConverter", CreateCategory("LC_Data"), CreateDescription("LD_ValueEditor_P_ValueToTextConverter"));
				callbackBuilder.AddCustomAttributes("IsValueValid", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_IsValueValid"));
				callbackBuilder.AddCustomAttributes("EditTemplate", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_EditTemplate"));
				callbackBuilder.AddCustomAttributes("IsNullable", CreateCategory("LC_Data"), CreateDescription("LD_ValueEditor_P_IsNullable"));
				callbackBuilder.AddCustomAttributes("ReadOnly", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_ReadOnly"));
				callbackBuilder.AddCustomAttributes("IsAlwaysInEditMode", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_IsAlwaysInEditMode"));
				callbackBuilder.AddCustomAttributes("IsEmbedded", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_IsEmbedded"));
				callbackBuilder.AddCustomAttributes("IsInEditMode", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_IsInEditMode"));
				callbackBuilder.AddCustomAttributes("OriginalValue", CreateCategory("LC_Data"), CreateDescription("LD_ValueEditor_P_OriginalValue"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_Appearance"), CreateDescription("LD_ValueEditor_P_Theme"));
				callbackBuilder.AddCustomAttributes("FormatProvider", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_FormatProvider"));
				callbackBuilder.AddCustomAttributes("Format", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_Format"));
				callbackBuilder.AddCustomAttributes("EditModeEnded", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_E_EditModeEnded"));
				callbackBuilder.AddCustomAttributes("EditModeEnding", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_E_EditModeEnding"));
				callbackBuilder.AddCustomAttributes("EditModeStarted", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_E_EditModeStarted"));
				callbackBuilder.AddCustomAttributes("EditModeStarting", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_E_EditModeStarting"));
				callbackBuilder.AddCustomAttributes("EditModeValidationError", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_E_EditModeValidationError"));
				callbackBuilder.AddCustomAttributes("ValueChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_E_ValueChanged"));
				callbackBuilder.AddCustomAttributes("TextChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_E_TextChanged"));
				callbackBuilder.AddCustomAttributes("EditTemplateChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_E_EditTemplateChanged"));
				callbackBuilder.AddCustomAttributes("IsReadOnly", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_IsReadOnly"));
				callbackBuilder.AddCustomAttributes("AlwaysValidate", CreateCategory("LC_Behavior"), CreateDescription("LD_ValueEditor_P_AlwaysValidate"));
            });

			// Infragistics.Windows.Editors.TextEditorBase
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.TextEditorBase), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DisplayText", CreateCategory("LC_Behavior"), CreateDescription("LD_TextEditorBase_P_DisplayText"));
				callbackBuilder.AddCustomAttributes("NullText", CreateCategory("LC_Behavior"), CreateDescription("LD_TextEditorBase_P_NullText"));
				callbackBuilder.AddCustomAttributes("ValueToDisplayTextConverter", CreateCategory("LC_Data"), CreateDescription("LD_TextEditorBase_P_ValueToDisplayTextConverter"));
			});


			// Infragistics.Windows.Editors.XamMaskedEditor
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamMaskedEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxBrowsable and ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamMaskedEditor"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamMaskedEditorAssetLibrary")));


				callbackBuilder.AddCustomAttributes("InsertMode", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_InsertMode"));
				callbackBuilder.AddCustomAttributes("Mask", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_Mask"));
				callbackBuilder.AddCustomAttributes("DataMode", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_DataMode"));
				callbackBuilder.AddCustomAttributes("ClipMode", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_ClipMode"));
				callbackBuilder.AddCustomAttributes("DisplayMode", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_DisplayMode"));
				callbackBuilder.AddCustomAttributes("PadChar", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_PadChar"));
				callbackBuilder.AddCustomAttributes("PromptChar", CreateCategory("LC_Display"), CreateDescription("LD_XamMaskedEditor_P_PromptChar"));
				callbackBuilder.AddCustomAttributes("SelectionStart", CreateCategory("LC_Data"), CreateDescription("LD_XamMaskedEditor_P_SelectionStart"));
				callbackBuilder.AddCustomAttributes("SelectionLength", CreateCategory("LC_Data"), CreateDescription("LD_XamMaskedEditor_P_SelectionLength"));
				callbackBuilder.AddCustomAttributes("SelectedText", CreateCategory("LC_Data"), CreateDescription("LD_XamMaskedEditor_P_SelectedText"));
				callbackBuilder.AddCustomAttributes("TextLength", CreateCategory("LC_Data"), CreateDescription("LD_XamMaskedEditor_P_TextLength"));
				callbackBuilder.AddCustomAttributes("ExecutingCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_E_ExecutingCommand"));
				callbackBuilder.AddCustomAttributes("ExecutedCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_E_ExecutedCommand"));
				// SSP 8/7/07
				// 
				callbackBuilder.AddCustomAttributes("SelectAllBehavior", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_SelectAllBehavior"));

				// JM 05-18-10
				callbackBuilder.AddCustomAttributes("AllowShiftingAcrossSections", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_AllowShiftingAcrossSections"));
				callbackBuilder.AddCustomAttributes("SpinWrap", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_SpinWrap"));
				callbackBuilder.AddCustomAttributes("TabNavigation", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_TabNavigation"));
			});


			// Infragistics.Windows.Editors.XamDateTimeEditor
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamDateTimeEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxBrowsable and ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamDateTimeEditor"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamDateTimeEditorAssetLibrary")));

			});


			// Infragistics.Windows.Editors.XamCurrencyEditor
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamCurrencyEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxBrowsable and ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamCurrencyEditor"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamCurrencyEditorAssetLibrary")));

			});



			// Infragistics.Windows.Editors.XamCheckEditor
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamCheckEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxBrowsable and ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamCheckEditor"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamCheckEditorAssetLibrary")));


				callbackBuilder.AddCustomAttributes("IsChecked", CreateCategory("LC_Data"), CreateDescription("LD_XamCheckEditor_P_IsChecked"));
				callbackBuilder.AddCustomAttributes("IsThreeState", CreateCategory("LC_Behavior"), CreateDescription("LD_XamCheckEditor_P_IsThreeState"));
            });


			// Infragistics.Windows.Editors.ValueEditorCheckBox
			// ================================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.ValueEditorCheckBox), delegate(AttributeCallbackBuilder callbackBuilder)
			{
                // JJD 06/04/10 - TFS32695
                callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

                callbackBuilder.AddCustomAttributes(CreateDescription("LD_ValueEditorCheckBox"));
			});

			// Infragistics.Windows.Editors.XamTextEditor
			// ==========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamTextEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamTextEditor"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamTextEditorAssetLibrary")));


				callbackBuilder.AddCustomAttributes("HorizontalScrollBarVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTextEditor_P_HorizontalScrollBarVisibility"));
				callbackBuilder.AddCustomAttributes("TextWrapping", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTextEditor_P_TextWrapping"));
				callbackBuilder.AddCustomAttributes("TextAlignment", CreateCategory("LC_Appearance"), CreateDescription("LD_XamTextEditor_P_TextAlignment"));
				callbackBuilder.AddCustomAttributes("TextAlignmentResolved", CreateCategory("LC_Appearance"), CreateDescription("LD_XamTextEditor_P_TextAlignmentResolved"));
				callbackBuilder.AddCustomAttributes("VerticalScrollBarVisibility", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTextEditor_P_VerticalScrollBarVisibility"));
				callbackBuilder.AddCustomAttributes("SelectionStart", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTextEditor_P_SelectionStart"));
				callbackBuilder.AddCustomAttributes("SelectionLength", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTextEditor_P_SelectionLength"));
				callbackBuilder.AddCustomAttributes("TextLength", CreateCategory("LC_Data"), CreateDescription("LD_XamTextEditor_P_TextLength"));
				callbackBuilder.AddCustomAttributes("SelectedText", CreateCategory("LC_Data"), CreateDescription("LD_XamTextEditor_P_SelectedText"));
			});


			// Infragistics.Windows.Editors.XamNumericEditor
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamNumericEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamNumericEditor"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamNumericEditorAssetLibrary")));

			});


			// Infragistics.Windows.Editors.ValueConstraint
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.ValueConstraint), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Enumeration", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_Enumeration"));
				callbackBuilder.AddCustomAttributes("FixedValue", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_FixedValue"));
				callbackBuilder.AddCustomAttributes("MaxExclusive", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_MaxExclusive"));
				callbackBuilder.AddCustomAttributes("MaxInclusive", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_MaxInclusive"));
				callbackBuilder.AddCustomAttributes("MaxLength", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_MaxLength"));
				callbackBuilder.AddCustomAttributes("MinExclusive", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_MinExclusive"));
				callbackBuilder.AddCustomAttributes("MinInclusive", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_MinInclusive"));
				callbackBuilder.AddCustomAttributes("MinLength", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_MinLength"));
				callbackBuilder.AddCustomAttributes("Nullable", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_Nullable"));
				callbackBuilder.AddCustomAttributes("RegexPattern", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_RegexPattern"));
			});

			// SSP 2/27/12 TFS101030
			// 
			builder.AddCustomAttributes( typeof( Infragistics.Windows.Editors.ValueConstraint ), "ValidationRules",
				new Microsoft.Windows.Design.PropertyEditing.NewItemTypesAttribute(
					typeof( System.Windows.Controls.DataErrorValidationRule ),
					typeof( System.Windows.Controls.ExceptionValidationRule )
				)
			);

			// Infragistics.Windows.Editors.ValuePresenter
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.ValuePresenter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_ValuePresenter"));

				callbackBuilder.AddCustomAttributes("IsInEditMode", CreateCategory("LC_Behavior"), CreateDescription("LD_ValuePresenter_P_IsInEditMode"));
			});


			// Infragistics.Windows.Editors.XamComboEditor
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamComboEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamComboEditor"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamComboEditorAssetLibrary")));


				callbackBuilder.AddCustomAttributes("ComboBoxStyle", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_ComboBoxStyle"));
				callbackBuilder.AddCustomAttributes("DropDownButtonDisplayMode", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_DropDownButtonDisplayMode"));
				callbackBuilder.AddCustomAttributes("DropDownButtonStyle", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_DropDownButtonStyle"));
				callbackBuilder.AddCustomAttributes("DropDownButtonVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_XamComboEditor_P_DropDownButtonVisibility"));
				callbackBuilder.AddCustomAttributes("DropDownResizeMode", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_DropDownResizeMode"));
				callbackBuilder.AddCustomAttributes("IsDropDownOpen", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_IsDropDownOpen"));
				callbackBuilder.AddCustomAttributes("IsEditable", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_IsEditable"));
				callbackBuilder.AddCustomAttributes("ItemsProvider", CreateCategory("LC_Data"), CreateDescription("LD_XamComboEditor_P_ItemsProvider"));
				callbackBuilder.AddCustomAttributes("MaxDropDownHeight", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_MaxDropDownHeight"));
				callbackBuilder.AddCustomAttributes("MaxDropDownWidth", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_MaxDropDownWidth"));
				callbackBuilder.AddCustomAttributes("MinDropDownWidth", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_MinDropDownWidth"));
				callbackBuilder.AddCustomAttributes("SelectedIndex", CreateCategory("LC_Data"), CreateDescription("LD_XamComboEditor_P_SelectedIndex"));
				callbackBuilder.AddCustomAttributes("SelectedItem", CreateCategory("LC_Data"), CreateDescription("LD_XamComboEditor_P_SelectedItem"));
				callbackBuilder.AddCustomAttributes("SelectedText", CreateCategory("LC_Data"), CreateDescription("LD_XamComboEditor_P_SelectedText"));
				callbackBuilder.AddCustomAttributes("SelectionStart", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_SelectionStart"));
				callbackBuilder.AddCustomAttributes("SelectionLength", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_P_SelectionLength"));
				callbackBuilder.AddCustomAttributes("TextAlignmentResolved", CreateCategory("LC_Appearance"), CreateDescription("LD_XamComboEditor_P_TextAlignmentResolved"));
				callbackBuilder.AddCustomAttributes("TextLength", CreateCategory("LC_Data"), CreateDescription("LD_XamComboEditor_P_TextLength"));
				callbackBuilder.AddCustomAttributes("ExecutingCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_E_ExecutingCommand"));
				callbackBuilder.AddCustomAttributes("ExecutedCommand", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_E_ExecutedCommand"));
				callbackBuilder.AddCustomAttributes("DropDownOpened", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_E_DropDownOpened"));
				callbackBuilder.AddCustomAttributes("DropDownClosed", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_E_DropDownClosed"));
				callbackBuilder.AddCustomAttributes("SelectedItemChanged", CreateCategory("LC_Behavior"), CreateDescription("LD_XamComboEditor_E_SelectedItemChanged"));

				// AS 5/22/08
				callbackBuilder.AddCustomAttributes("ItemsSource", CreateCategory("LC_Data"), CreateDescription("LD_XamComboEditor_P_ItemsSource"));
				callbackBuilder.AddCustomAttributes("DisplayMemberPath", CreateCategory("LC_Data"), CreateDescription("LD_XamComboEditor_P_DisplayMemberPath"));
				callbackBuilder.AddCustomAttributes("ValuePath", CreateCategory("LC_Data"), CreateDescription("LD_XamComboEditor_P_ValuePath"));
            });


			// Infragistics.Windows.Editors.ComboBoxItemsProvider
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.ComboBoxItemsProvider), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DisplayMemberPath", CreateCategory("LC_Data"), CreateDescription("LD_ComboBoxItemsProvider_P_DisplayMemberPath"));
				callbackBuilder.AddCustomAttributes("DisplayTextComparer", CreateCategory("LC_Behavior"), CreateDescription("LD_ComboBoxItemsProvider_P_DisplayTextComparer"));
				callbackBuilder.AddCustomAttributes("Items", CreateCategory("LC_Data"), CreateDescription("LD_ComboBoxItemsProvider_P_Items"));
				callbackBuilder.AddCustomAttributes("ItemsSource", CreateCategory("LC_Data"), CreateDescription("LD_ComboBoxItemsProvider_P_ItemsSource"));
				callbackBuilder.AddCustomAttributes("ValueComparer", CreateCategory("LC_Behavior"), CreateDescription("LD_ComboBoxItemsProvider_P_ValueComparer"));
				callbackBuilder.AddCustomAttributes("ValuePath", CreateCategory("LC_Data"), CreateDescription("LD_ComboBoxItemsProvider_P_ValuePath"));
			});

			// Infragistics.Windows.Editors.ComboBoxDataItem
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.ComboBoxDataItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DisplayText", CreateDescription("LD_ComboBoxDataItem_P_DisplayText"));
				callbackBuilder.AddCustomAttributes("Image", CreateCategory("LC_Appearance"), CreateDescription("LD_ComboBoxDataItem_P_Image"));
				callbackBuilder.AddCustomAttributes("Tag", CreateCategory("LC_Data"), CreateDescription("LD_ComboBoxDataItem_P_Tag"));
				callbackBuilder.AddCustomAttributes("Value", CreateCategory("LC_Data"), CreateDescription("LD_ComboBoxDataItem_P_Value"));
			});

			#endregion //Description/Category

			#region NA 2008 Vol 2

			// Infragistics.Windows.Editors.CalendarItem
			// =========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CalendarItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("ContainsSelectedDates", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItem_P_ContainsSelectedDates"));
				callbackBuilder.AddCustomAttributes("ContainsToday", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItem_P_ContainsToday"));
				callbackBuilder.AddCustomAttributes("EndDate", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItem_P_EndDate"));
				callbackBuilder.AddCustomAttributes("IsActive", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItem_P_IsActive"));
				callbackBuilder.AddCustomAttributes("IsLeadingOrTrailingItem", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItem_P_IsLeadingOrTrailingItem"));
				callbackBuilder.AddCustomAttributes("IsSelected", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItem_P_IsSelected"));
				callbackBuilder.AddCustomAttributes("IsSelectionActive", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItem_P_IsSelectionActive"));
				callbackBuilder.AddCustomAttributes("IsToday", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItem_P_IsToday"));
				callbackBuilder.AddCustomAttributes("StartDate", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItem_P_StartDate"));
            });

			// Infragistics.Windows.Editors.CalendarItemGroupPanel
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CalendarItemGroupPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("HorizontalContentAlignment", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroupPanel_P_HorizontalContentAlignment"));
				callbackBuilder.AddCustomAttributes("GroupHeight", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroupPanel_P_GroupHeight"));
				callbackBuilder.AddCustomAttributes("GroupWidth", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroupPanel_P_GroupWidth"));
				callbackBuilder.AddCustomAttributes("MaxGroups", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroupPanel_P_MaxGroups"));
				callbackBuilder.AddCustomAttributes("VerticalContentAlignment", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroupPanel_P_VerticalContentAlignment"));
			});

			// Infragistics.Windows.Editors.CalendarItemGroup
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CalendarItemGroup), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("CurrentCalendarMode", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroup_P_CurrentCalendarMode"));
				callbackBuilder.AddCustomAttributes("ReferenceGroupOffset", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroup_P_ReferenceGroupOffset"));
				callbackBuilder.AddCustomAttributes("ShowLeadingDates", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroup_P_ShowLeadingDates"));
				callbackBuilder.AddCustomAttributes("ScrollNextButtonVisibility", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroup_P_ScrollNextButtonVisibility"));
				callbackBuilder.AddCustomAttributes("ScrollPreviousButtonVisibility", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroup_P_ScrollPreviousButtonVisibility"));
				callbackBuilder.AddCustomAttributes("ShowTrailingDates", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroup_P_ShowTrailingDates"));

				// AS 10/14/09 FR11859
				callbackBuilder.AddCustomAttributes("IsTrailingGroup", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroup_P_IsTrailingGroup"));
				callbackBuilder.AddCustomAttributes("IsLeadingGroup", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemGroup_P_IsLeadingGroup"));
            });

			// Infragistics.Windows.Editors.CalendarDayOfWeek
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CalendarDayOfWeek), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.Editors.CalendarItemArea
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CalendarItemArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("DayOfWeekHeaderVisibility", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemArea_P_DayOfWeekHeaderVisibility"));
				callbackBuilder.AddCustomAttributes("ItemColumns", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemArea_P_ItemColumns"));
				callbackBuilder.AddCustomAttributes("ItemRows", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemArea_P_ItemRows"));
				callbackBuilder.AddCustomAttributes("WeekNumbers", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemArea_P_WeekNumbers"));
				callbackBuilder.AddCustomAttributes("WeekNumberVisibility", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarItemArea_P_WeekNumberVisibility"));
			});

			// Infragistics.Windows.Editors.CalendarItemGroupTitle
			// ===================================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CalendarItemGroupTitle), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.Editors.XamDateTimeEditor
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamDateTimeEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AllowDropDown", CreateCategory("LC_Behavior"), CreateDescription("LD_XamDateTimeEditor_P_AllowDropDown"));
				callbackBuilder.AddCustomAttributes("ComputedMaxDate", CreateCategory("LC_Data"), CreateDescription("LD_XamDateTimeEditor_P_ComputedMaxDate"));
				callbackBuilder.AddCustomAttributes("ComputedMinCalendarMode", CreateCategory("LC_Data"), CreateDescription("LD_XamDateTimeEditor_P_ComputedMinCalendarMode"));
				callbackBuilder.AddCustomAttributes("ComputedMinDate", CreateCategory("LC_Data"), CreateDescription("LD_XamDateTimeEditor_P_ComputedMinDate"));
				callbackBuilder.AddCustomAttributes("DropDownButtonDisplayMode", CreateCategory("LC_Behavior"), CreateDescription("LD_XamDateTimeEditor_P_DropDownButtonDisplayMode"));
				callbackBuilder.AddCustomAttributes("DropDownButtonStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_XamDateTimeEditor_P_DropDownButtonStyle"));
				callbackBuilder.AddCustomAttributes("DropDownButtonVisibility", CreateCategory("LC_Appearance"), CreateDescription("LD_XamDateTimeEditor_P_DropDownButtonVisibility"));
				callbackBuilder.AddCustomAttributes("DropDownOpened", CreateCategory("LC_Behavior"), CreateDescription("LD_XamDateTimeEditor_E_DropDownOpened"));
				callbackBuilder.AddCustomAttributes("DropDownClosed", CreateCategory("LC_Behavior"), CreateDescription("LD_XamDateTimeEditor_E_DropDownClosed"));
			});

			// Infragistics.Windows.Editors.CalendarItemAreaPanel
			// ==================================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CalendarItemAreaPanel), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});

			// Infragistics.Windows.Editors.CalendarDay
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CalendarDay), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("IsWorkday", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_CalendarDay_P_IsWorkday"));
            });

			// Infragistics.Windows.Editors.XamMonthCalendar
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamMonthCalendar), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JM 05-04-11 TFS70940, TFS60790 Add ToolboxCategoryAttribute.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.Yes, CreateDescription("LD_XamMonthCalendar"));

				callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamMonthCalendarAssetLibrary")));


				callbackBuilder.AddCustomAttributes("ActiveDate", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_ActiveDate"));
				callbackBuilder.AddCustomAttributes("AutoAdjustCalendarDimensions", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_AutoAdjustCalendarDimensions"));
				callbackBuilder.AddCustomAttributes("CalendarDimensions", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_CalendarDimensions"));
				callbackBuilder.AddCustomAttributes("CalendarDayStyle", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_CalendarDayStyle"));
				callbackBuilder.AddCustomAttributes("CalendarDayStyleSelector", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_CalendarDayStyleSelector"));
				callbackBuilder.AddCustomAttributes("CalendarItemStyle", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_CalendarItemStyle"));
				callbackBuilder.AddCustomAttributes("CalendarItemStyleSelector", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_CalendarItemStyleSelector"));
				callbackBuilder.AddCustomAttributes("CurrentCalendarMode", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_CurrentCalendarMode"));
				callbackBuilder.AddCustomAttributes("DayOfWeekHeaderFormat", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_DayOfWeekHeaderFormat"));
				callbackBuilder.AddCustomAttributes("DayOfWeekHeaderVisibility", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_DayOfWeekHeaderVisibility"));

				// JJD 11/9/11 - TFS79598 - added entry for DisabledDates
				callbackBuilder.AddCustomAttributes("DisabledDates", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_DisabledDates"));
				
				callbackBuilder.AddCustomAttributes("DisabledDaysOfWeek", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_DisabledDaysOfWeek"));
				callbackBuilder.AddCustomAttributes("FirstDayOfWeek", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_FirstDayOfWeek"));
				callbackBuilder.AddCustomAttributes("MaxDate", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_MaxDate"));
				callbackBuilder.AddCustomAttributes("MaxSelectedDates", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_MaxSelectedDates"));
				callbackBuilder.AddCustomAttributes("MinDate", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_MinDate"));
				callbackBuilder.AddCustomAttributes("MinCalendarMode", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_MinCalendarMode"));
				callbackBuilder.AddCustomAttributes("ReferenceDate", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_ReferenceDate"));
				callbackBuilder.AddCustomAttributes("ScrollButtonVisibility", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_ScrollButtonVisibility"));
				callbackBuilder.AddCustomAttributes("SelectedDate", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_SelectedDate"));
				callbackBuilder.AddCustomAttributes("SelectionType", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_SelectionType"));
				callbackBuilder.AddCustomAttributes("ShowDisabledDaysOfWeek", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_ShowDisabledDaysOfWeek"));
				callbackBuilder.AddCustomAttributes("ShowLeadingAndTrailingDates", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_ShowLeadingAndTrailingDates"));
				callbackBuilder.AddCustomAttributes("TodayButtonVisibility", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_TodayButtonVisibility"));
				callbackBuilder.AddCustomAttributes("WeekNumberVisibility", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_WeekNumberVisibility"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_Theme"));
				callbackBuilder.AddCustomAttributes("WeekRule", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_WeekRule"));
				callbackBuilder.AddCustomAttributes("Workdays", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_Workdays"));
				callbackBuilder.AddCustomAttributes("ExecutingCommand", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_E_ExecutingCommand"));
				callbackBuilder.AddCustomAttributes("ExecutedCommand", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_E_ExecutedCommand"));
				callbackBuilder.AddCustomAttributes("SelectedDatesChanged", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_E_SelectedDatesChanged"));

				// AS 10/14/09 FR11859
				callbackBuilder.AddCustomAttributes("AllowLeadingAndTrailingGroupActivation", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_AllowLeadingAndTrailingGroupActivation"));

				// AS 3/23/10 TFS26461
				callbackBuilder.AddCustomAttributes("Today", CreateCategory("LC_MonthCalendar Properties"), CreateDescription("LD_XamMonthCalendar_P_Today"));
			});

			// Infragistics.Windows.Editors.CalendarWeekNumber
			// ===============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CalendarWeekNumber), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			});


			#endregion //NA 2008 Vol 2

			#region NA 2009 Vol 2

			// Infragistics.Windows.Editors.ValueEditor
			// ========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.ValueEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("InvalidValueErrorInfo", CreateCategory("LC_Data"), CreateDescription("LD_ValueEditor_P_InvalidValueErrorInfo"));
			});

			// Infragistics.Windows.Editors.XamComboEditor
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamComboEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DisplayValue", CreateCategory("LC_Data"), CreateDescription("LD_XamComboEditor_P_DisplayValue"));
				callbackBuilder.AddCustomAttributes("DisplayValueSource", CreateCategory("LC_Data"), CreateDescription("LD_XamComboEditor_P_DisplayValueSource"));
			});

			// Infragistics.Windows.Editors.XamDateTimeEditor
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamDateTimeEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DateValue", CreateCategory("LC_Data"), CreateDescription("LD_XamDateTimeEditor_P_DateValue"));
			});

			// Infragistics.Windows.Editors.ValueConstraint
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.ValueConstraint), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ValidateAsType", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_ValidateAsType"));
				callbackBuilder.AddCustomAttributes("ValidationRules", CreateCategory("LC_Data"), CreateDescription("LD_ValueConstraint_P_ValidationRules"));
			});

			#endregion //NA 2009 Vol 2

			#region ToolboxBrowsable
            // JJD 06/04/10 - TFS32695
            // Moved ToolboxBrowsableAttribute.No into callbacks

            // AS 1/8/08 ToolboxBrowsable

			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Editors.CaretElement), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Editors.DisplayCharacterPresenter), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Editors.DisplayCharactersList), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Editors.SectionPresenter), ToolboxBrowsableAttribute.No);
			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Editors.SectionsList), ToolboxBrowsableAttribute.No);

			//builder.AddCustomAttributes(typeof(Infragistics.Windows.Editors.ValueEditorCheckBox), ToolboxBrowsableAttribute.No);

			#endregion //ToolboxBrowsable

			#region NA 2010 Vol 1

			// Infragistics.Windows.Editors.CalendarItemGroup
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.CalendarItemGroup), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				callbackBuilder.AddCustomAttributes("IsLeadingGroup", CreateCategory("LC_Behavior"), CreateDescription("LD_CalendarItemGroup_P_IsLeadingGroup"));
			});

			// Infragistics.Windows.Editors.XamMaskedEditor
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamMaskedEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("SpinButtonDisplayMode", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_SpinButtonDisplayMode"));
				callbackBuilder.AddCustomAttributes("SpinButtonStyle", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_SpinButtonStyle"));
				callbackBuilder.AddCustomAttributes("SpinButtonVisibilityResolved", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_SpinButtonVisibilityResolved"));
				callbackBuilder.AddCustomAttributes("SpinIncrement", CreateCategory("LC_Data"), CreateDescription("LD_XamMaskedEditor_P_SpinIncrement"));
				callbackBuilder.AddCustomAttributes("AutoFillDate", CreateCategory("LC_Behavior"), CreateDescription("LD_XamMaskedEditor_P_AutoFillDate"));
			});

			// Infragistics.Windows.Editors.XamTextEditor
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.XamTextEditor), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				// JJD 11/29/10 - TFS58984 - Added
				callbackBuilder.AddCustomAttributes("AcceptsReturn", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTextEditor_P_AcceptsReturn"));
				callbackBuilder.AddCustomAttributes("AcceptsTab", CreateCategory("LC_Behavior"), CreateDescription("LD_XamTextEditor_P_AcceptsTab"));
			});

			#endregion

			#region Other

			// AS 8/25/11 TFS83698
			// Infragistics.Windows.Editors.VirtualizingStackPanelEx
			// ======================================================
			builder.AddCallback(typeof(Infragistics.Windows.Editors.VirtualizingStackPanelEx), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);
			}); 

			#endregion //Other

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