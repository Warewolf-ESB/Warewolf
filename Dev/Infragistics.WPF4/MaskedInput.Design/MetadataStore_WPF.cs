using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Editors.XamMaskedInput.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Editors.XamMaskedInput.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Editors.XamMaskedInput);
				Assembly controlAssembly = t.Assembly;

				#region SectionsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.SectionsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SectionsCollection Properties

				#region SectionBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.SectionBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Sections",
					new DescriptionAttribute(SR.GetString("SectionBase_Sections_Property")),
				    new DisplayNameAttribute("Sections")				);


				tableBuilder.AddCustomAttributes(t, "DisplayChars",
					new DescriptionAttribute(SR.GetString("SectionBase_DisplayChars_Property")),
				    new DisplayNameAttribute("DisplayChars")				);


				tableBuilder.AddCustomAttributes(t, "PreviousSection",
					new DescriptionAttribute(SR.GetString("SectionBase_PreviousSection_Property")),
				    new DisplayNameAttribute("PreviousSection")				);


				tableBuilder.AddCustomAttributes(t, "PreviousLiteralSection",
					new DescriptionAttribute(SR.GetString("SectionBase_PreviousLiteralSection_Property")),
				    new DisplayNameAttribute("PreviousLiteralSection")				);


				tableBuilder.AddCustomAttributes(t, "NextSection",
					new DescriptionAttribute(SR.GetString("SectionBase_NextSection_Property")),
				    new DisplayNameAttribute("NextSection")				);


				tableBuilder.AddCustomAttributes(t, "PreviousEditSection",
					new DescriptionAttribute(SR.GetString("SectionBase_PreviousEditSection_Property")),
				    new DisplayNameAttribute("PreviousEditSection")				);


				tableBuilder.AddCustomAttributes(t, "NextEditSection",
					new DescriptionAttribute(SR.GetString("SectionBase_NextEditSection_Property")),
				    new DisplayNameAttribute("NextEditSection")				);


				tableBuilder.AddCustomAttributes(t, "NextLiteralSection",
					new DescriptionAttribute(SR.GetString("SectionBase_NextLiteralSection_Property")),
				    new DisplayNameAttribute("NextLiteralSection")				);

				#endregion // SectionBase Properties

				#region LiteralSection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.LiteralSection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LiteralSection Properties

				#region EditSectionBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.EditSectionBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("EditSectionBase_Orientation_Property")),
				    new DisplayNameAttribute("Orientation")				);

				#endregion // EditSectionBase Properties

				#region DisplayCharsEditSection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.DisplayCharsEditSection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("DisplayCharsEditSection_Orientation_Property")),
				    new DisplayNameAttribute("Orientation")				);

				#endregion // DisplayCharsEditSection Properties

				#region NumberSection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.NumberSection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("NumberSection_MinValue_Property")),
				    new DisplayNameAttribute("MinValue")				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("NumberSection_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue")				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("NumberSection_Orientation_Property")),
				    new DisplayNameAttribute("Orientation")				);

				#endregion // NumberSection Properties

				#region MonthSection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.MonthSection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MonthSection Properties

				#region DaySection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.DaySection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DaySection Properties

				#region YearSection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.YearSection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // YearSection Properties

				#region HourSection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.HourSection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HourSection Properties

				#region AMPMSection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.AMPMSection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AMPMSection Properties

				#region InputCharBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.InputCharBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsEditable",
					new DescriptionAttribute(SR.GetString("InputCharBase_IsEditable_Property")),
				    new DisplayNameAttribute("IsEditable")				);


				tableBuilder.AddCustomAttributes(t, "Char",
					new DescriptionAttribute(SR.GetString("InputCharBase_Char_Property")),
				    new DisplayNameAttribute("Char")				);

				#endregion // InputCharBase Properties

				#region DisplayCharBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.DisplayCharBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IncludeMethod",
					new DescriptionAttribute(SR.GetString("DisplayCharBase_IncludeMethod_Property")),
				    new DisplayNameAttribute("IncludeMethod")				);


				tableBuilder.AddCustomAttributes(t, "DrawString",
					new DescriptionAttribute(SR.GetString("DisplayCharBase_DrawString_Property")),
				    new DisplayNameAttribute("DrawString")				);


				tableBuilder.AddCustomAttributes(t, "IsEditable",
					new DescriptionAttribute(SR.GetString("DisplayCharBase_IsEditable_Property")),
				    new DisplayNameAttribute("IsEditable")				);


				tableBuilder.AddCustomAttributes(t, "Char",
					new DescriptionAttribute(SR.GetString("DisplayCharBase_Char_Property")),
				    new DisplayNameAttribute("Char")				);

				#endregion // DisplayCharBase Properties

				#region MinuteSection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.MinuteSection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MinuteSection Properties

				#region SecondSection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.SecondSection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SecondSection Properties

				#region FractionPart Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.FractionPart");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NumberOfDigits",
					new DescriptionAttribute(SR.GetString("FractionPart_NumberOfDigits_Property")),
				    new DisplayNameAttribute("NumberOfDigits")				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("FractionPart_Orientation_Property")),
				    new DisplayNameAttribute("Orientation")				);

				#endregion // FractionPart Properties

				#region FractionPartContinuous Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.FractionPartContinuous");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("FractionPartContinuous_Orientation_Property")),
				    new DisplayNameAttribute("Orientation")				);

				#endregion // FractionPartContinuous Properties

				#region DigitChar Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.DigitChar");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DigitChar Properties

				#region AlphaChar Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.AlphaChar");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AlphaChar Properties

				#region AlphanumericChar Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.AlphanumericChar");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AlphanumericChar Properties

				#region CharacterSet Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.CharacterSet");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CharacterSet Properties

				#region HexDigitChar Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.HexDigitChar");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HexDigitChar Properties

				#region ParsedMask Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ParsedMask");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Mask",
					new DescriptionAttribute(SR.GetString("ParsedMask_Mask_Property")),
				    new DisplayNameAttribute("Mask")				);


				tableBuilder.AddCustomAttributes(t, "PromptCharacter",
					new DescriptionAttribute(SR.GetString("ParsedMask_PromptCharacter_Property")),
				    new DisplayNameAttribute("PromptCharacter")				);


				tableBuilder.AddCustomAttributes(t, "PadCharacter",
					new DescriptionAttribute(SR.GetString("ParsedMask_PadCharacter_Property")),
				    new DisplayNameAttribute("PadCharacter")				);

				#endregion // ParsedMask Properties

				#region ValueConstraint Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ValueConstraint");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Enumeration",
					new DescriptionAttribute(SR.GetString("ValueConstraint_Enumeration_Property")),
				    new DisplayNameAttribute("Enumeration"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasEnumeration",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasEnumeration_Property")),
				    new DisplayNameAttribute("HasEnumeration"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FixedValue",
					new DescriptionAttribute(SR.GetString("ValueConstraint_FixedValue_Property")),
				    new DisplayNameAttribute("FixedValue"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasFixedValue",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasFixedValue_Property")),
				    new DisplayNameAttribute("HasFixedValue"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxExclusive",
					new DescriptionAttribute(SR.GetString("ValueConstraint_MaxExclusive_Property")),
				    new DisplayNameAttribute("MaxExclusive"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasMaxExclusive",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasMaxExclusive_Property")),
				    new DisplayNameAttribute("HasMaxExclusive"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxInclusive",
					new DescriptionAttribute(SR.GetString("ValueConstraint_MaxInclusive_Property")),
				    new DisplayNameAttribute("MaxInclusive"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasMaxInclusive",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasMaxInclusive_Property")),
				    new DisplayNameAttribute("HasMaxInclusive"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxLength",
					new DescriptionAttribute(SR.GetString("ValueConstraint_MaxLength_Property")),
				    new DisplayNameAttribute("MaxLength"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasMaxLength",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasMaxLength_Property")),
				    new DisplayNameAttribute("HasMaxLength"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinExclusive",
					new DescriptionAttribute(SR.GetString("ValueConstraint_MinExclusive_Property")),
				    new DisplayNameAttribute("MinExclusive"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasMinExclusive",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasMinExclusive_Property")),
				    new DisplayNameAttribute("HasMinExclusive"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinInclusive",
					new DescriptionAttribute(SR.GetString("ValueConstraint_MinInclusive_Property")),
				    new DisplayNameAttribute("MinInclusive"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasMinInclusive",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasMinInclusive_Property")),
				    new DisplayNameAttribute("HasMinInclusive"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinLength",
					new DescriptionAttribute(SR.GetString("ValueConstraint_MinLength_Property")),
				    new DisplayNameAttribute("MinLength"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasMinLength",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasMinLength_Property")),
				    new DisplayNameAttribute("HasMinLength"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Nullable",
					new DescriptionAttribute(SR.GetString("ValueConstraint_Nullable_Property")),
				    new DisplayNameAttribute("Nullable"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasNullable",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasNullable_Property")),
				    new DisplayNameAttribute("HasNullable"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RegexPattern",
					new DescriptionAttribute(SR.GetString("ValueConstraint_RegexPattern_Property")),
				    new DisplayNameAttribute("RegexPattern"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasRegexPattern",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasRegexPattern_Property")),
				    new DisplayNameAttribute("HasRegexPattern"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValidateAsType",
					new DescriptionAttribute(SR.GetString("ValueConstraint_ValidateAsType_Property")),
				    new DisplayNameAttribute("ValidateAsType"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasAnyConstraints",
					new DescriptionAttribute(SR.GetString("ValueConstraint_HasAnyConstraints_Property")),
				    new DisplayNameAttribute("HasAnyConstraints"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);

				#endregion // ValueConstraint Properties

				#region ValidationErrorInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ValidationErrorInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ErrorMessage",
					new DescriptionAttribute(SR.GetString("ValidationErrorInfo_ErrorMessage_Property")),
				    new DisplayNameAttribute("ErrorMessage")				);


				tableBuilder.AddCustomAttributes(t, "Exception",
					new DescriptionAttribute(SR.GetString("ValidationErrorInfo_Exception_Property")),
				    new DisplayNameAttribute("Exception")				);

				#endregion // ValidationErrorInfo Properties

				#region HorizontalToTextAlignmentConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.HorizontalToTextAlignmentConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HorizontalToTextAlignmentConverter Properties

				#region DisplayCharsCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.DisplayCharsCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Section",
					new DescriptionAttribute(SR.GetString("DisplayCharsCollection_Section_Property")),
				    new DisplayNameAttribute("Section")				);

				#endregion // DisplayCharsCollection Properties

				#region MaskedInputCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.MaskedInputCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandId",
					new DescriptionAttribute(SR.GetString("MaskedInputCommand_CommandId_Property")),
				    new DisplayNameAttribute("CommandId")				);

				#endregion // MaskedInputCommand Properties

				#region MaskedInputCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.MaskedInputCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandId",
					new DescriptionAttribute(SR.GetString("MaskedInputCommandSource_CommandId_Property")),
				    new DisplayNameAttribute("CommandId")				);

				#endregion // MaskedInputCommandSource Properties

				#region XamMaskedInput Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamMaskedInput");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamMaskedInputAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamMaskedInputAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "SpinButtonDisplayMode",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_SpinButtonDisplayMode_Property")),
				    new DisplayNameAttribute("SpinButtonDisplayMode"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SpinButtonStyle",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_SpinButtonStyle_Property")),
				    new DisplayNameAttribute("SpinButtonStyle"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SpinButtonVisibilityResolved",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_SpinButtonVisibilityResolved_Property")),
				    new DisplayNameAttribute("SpinButtonVisibilityResolved"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SpinIncrement",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_SpinIncrement_Property")),
				    new DisplayNameAttribute("SpinIncrement"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SpinWrap",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_SpinWrap_Property")),
				    new DisplayNameAttribute("SpinWrap"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Sections",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_Sections_Property")),
				    new DisplayNameAttribute("Sections"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayChars",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_DisplayChars_Property")),
				    new DisplayNameAttribute("DisplayChars"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InsertMode",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_InsertMode_Property")),
				    new DisplayNameAttribute("InsertMode"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Mask",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_Mask_Property")),
				    new DisplayNameAttribute("Mask"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DataMode",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_DataMode_Property")),
				    new DisplayNameAttribute("DataMode"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ClipMode",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_ClipMode_Property")),
				    new DisplayNameAttribute("ClipMode"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayMode",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_DisplayMode_Property")),
				    new DisplayNameAttribute("DisplayMode"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PadChar",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_PadChar_Property")),
				    new DisplayNameAttribute("PadChar"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PromptChar",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_PromptChar_Property")),
				    new DisplayNameAttribute("PromptChar"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SectionTabNavigation",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_SectionTabNavigation_Property")),
				    new DisplayNameAttribute("SectionTabNavigation"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowShiftingAcrossSections",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_AllowShiftingAcrossSections_Property")),
				    new DisplayNameAttribute("AllowShiftingAcrossSections"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectionStart",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_SelectionStart_Property")),
				    new DisplayNameAttribute("SelectionStart"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectionLength",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_SelectionLength_Property")),
				    new DisplayNameAttribute("SelectionLength"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedText",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_SelectedText_Property")),
				    new DisplayNameAttribute("SelectedText"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TextLength",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_TextLength_Property")),
				    new DisplayNameAttribute("TextLength"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectAllBehavior",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_SelectAllBehavior_Property")),
				    new DisplayNameAttribute("SelectAllBehavior"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AutoFillDate",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_AutoFillDate_Property")),
				    new DisplayNameAttribute("AutoFillDate"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrimFractionalZeros",
					new DescriptionAttribute(SR.GetString("XamMaskedInput_TrimFractionalZeros_Property")),
				    new DisplayNameAttribute("TrimFractionalZeros"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);

				#endregion // XamMaskedInput Properties

				#region MaskedInputTextBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.MaskedInputTextBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MaskedInputTextBox Properties

				#region XamCurrencyInput Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamCurrencyInput");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamMaskedInputAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamMaskedInputAssetLibrary"))
				);

				#endregion // XamCurrencyInput Properties

				#region XamNumericInput Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamNumericInput");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamMaskedInputAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamMaskedInputAssetLibrary"))
				);

				#endregion // XamNumericInput Properties

				#region TextInputBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.TextInputBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayText",
					new DescriptionAttribute(SR.GetString("TextInputBase_DisplayText_Property")),
				    new DisplayNameAttribute("DisplayText"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NullText",
					new DescriptionAttribute(SR.GetString("TextInputBase_NullText_Property")),
				    new DisplayNameAttribute("NullText"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueToDisplayTextConverter",
					new DescriptionAttribute(SR.GetString("TextInputBase_ValueToDisplayTextConverter_Property")),
				    new DisplayNameAttribute("ValueToDisplayTextConverter"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueToDisplayTextConverterResolved",
					new DescriptionAttribute(SR.GetString("TextInputBase_ValueToDisplayTextConverterResolved_Property")),
				    new DisplayNameAttribute("ValueToDisplayTextConverterResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);

				#endregion // TextInputBase Properties

				#region ValueInput Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ValueInput");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AlwaysValidateResolved",
					new DescriptionAttribute(SR.GetString("ValueInput_AlwaysValidateResolved_Property")),
				    new DisplayNameAttribute("AlwaysValidateResolved"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FormatProviderResolved",
					new DescriptionAttribute(SR.GetString("ValueInput_FormatProviderResolved_Property")),
				    new DisplayNameAttribute("FormatProviderResolved"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueToTextConverterResolved",
					new DescriptionAttribute(SR.GetString("ValueInput_ValueToTextConverterResolved_Property")),
				    new DisplayNameAttribute("ValueToTextConverterResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AlwaysValidate",
					new DescriptionAttribute(SR.GetString("ValueInput_AlwaysValidate_Property")),
				    new DisplayNameAttribute("AlwaysValidate"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("ValueInput_Value_Property")),
				    new DisplayNameAttribute("Value"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Text",
					new DescriptionAttribute(SR.GetString("ValueInput_Text_Property")),
				    new DisplayNameAttribute("Text"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueType",
					new DescriptionAttribute(SR.GetString("ValueInput_ValueType_Property")),
				    new DisplayNameAttribute("ValueType"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueTypeName",
					new DescriptionAttribute(SR.GetString("ValueInput_ValueTypeName_Property")),
				    new DisplayNameAttribute("ValueTypeName"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueTypeResolved",
					new DescriptionAttribute(SR.GetString("ValueInput_ValueTypeResolved_Property")),
				    new DisplayNameAttribute("ValueTypeResolved"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueConstraint",
					new DescriptionAttribute(SR.GetString("ValueInput_ValueConstraint_Property")),
				    new DisplayNameAttribute("ValueConstraint"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InvalidValueBehavior",
					new DescriptionAttribute(SR.GetString("ValueInput_InvalidValueBehavior_Property")),
				    new DisplayNameAttribute("InvalidValueBehavior"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueToTextConverter",
					new DescriptionAttribute(SR.GetString("ValueInput_ValueToTextConverter_Property")),
				    new DisplayNameAttribute("ValueToTextConverter"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsValueValid",
					new DescriptionAttribute(SR.GetString("ValueInput_IsValueValid_Property")),
				    new DisplayNameAttribute("IsValueValid"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasValueChanged",
					new DescriptionAttribute(SR.GetString("ValueInput_HasValueChanged_Property")),
				    new DisplayNameAttribute("HasValueChanged"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InvalidValueErrorInfo",
					new DescriptionAttribute(SR.GetString("ValueInput_InvalidValueErrorInfo_Property")),
				    new DisplayNameAttribute("InvalidValueErrorInfo"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("ValueInput_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OriginalValue",
					new DescriptionAttribute(SR.GetString("ValueInput_OriginalValue_Property")),
				    new DisplayNameAttribute("OriginalValue"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FormatProvider",
					new DescriptionAttribute(SR.GetString("ValueInput_FormatProvider_Property")),
				    new DisplayNameAttribute("FormatProvider"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Format",
					new DescriptionAttribute(SR.GetString("ValueInput_Format_Property")),
				    new DisplayNameAttribute("Format"),
					new CategoryAttribute(SR.GetString("XamMaskedInput_Properties"))
				);

				#endregion // ValueInput Properties

				#region EditModeValidationErrorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.EditModeValidationErrorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Editor",
					new DescriptionAttribute(SR.GetString("EditModeValidationErrorEventArgs_Editor_Property")),
				    new DisplayNameAttribute("Editor")				);


				tableBuilder.AddCustomAttributes(t, "InvalidValueBehavior",
					new DescriptionAttribute(SR.GetString("EditModeValidationErrorEventArgs_InvalidValueBehavior_Property")),
				    new DisplayNameAttribute("InvalidValueBehavior")				);


				tableBuilder.AddCustomAttributes(t, "ErrorMessage",
					new DescriptionAttribute(SR.GetString("EditModeValidationErrorEventArgs_ErrorMessage_Property")),
				    new DisplayNameAttribute("ErrorMessage")				);


				tableBuilder.AddCustomAttributes(t, "Exception",
					new DescriptionAttribute(SR.GetString("EditModeValidationErrorEventArgs_Exception_Property")),
				    new DisplayNameAttribute("Exception")				);

				#endregion // EditModeValidationErrorEventArgs Properties

				#region InvalidCharEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.InvalidCharEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Char",
					new DescriptionAttribute(SR.GetString("InvalidCharEventArgs_Char_Property")),
				    new DisplayNameAttribute("Char")				);


				tableBuilder.AddCustomAttributes(t, "DisplayChar",
					new DescriptionAttribute(SR.GetString("InvalidCharEventArgs_DisplayChar_Property")),
				    new DisplayNameAttribute("DisplayChar")				);


				tableBuilder.AddCustomAttributes(t, "Beep",
					new DescriptionAttribute(SR.GetString("InvalidCharEventArgs_Beep_Property")),
				    new DisplayNameAttribute("Beep")				);

				#endregion // InvalidCharEventArgs Properties

				#region MaskCharConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.MaskCharConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MaskCharConverter Properties

				#region ValueInputAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.ValueInputAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ValueInputAutomationPeer Properties

				#region XamMaskedInputAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamMaskedInputAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamMaskedInputAutomationPeer Properties
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