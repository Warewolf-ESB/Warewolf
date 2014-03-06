using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Editors.XamColorPicker.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Editors.XamColorPicker.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Editors.ColorSliderView);
				Assembly controlAssembly = t.Assembly;

				#region ColorPickerDialog Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ColorPickerDialog");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsOpen",
					new DescriptionAttribute(SR.GetString("ColorPickerDialog_IsOpen_Property")),
				    new DisplayNameAttribute("IsOpen")				);


				tableBuilder.AddCustomAttributes(t, "ColorPicker",
					new DescriptionAttribute(SR.GetString("ColorPickerDialog_ColorPicker_Property")),
				    new DisplayNameAttribute("ColorPicker")				);


				tableBuilder.AddCustomAttributes(t, "DialogCaption",
					new DescriptionAttribute(SR.GetString("ColorPickerDialog_DialogCaption_Property")),
				    new DisplayNameAttribute("DialogCaption")				);

				#endregion // ColorPickerDialog Properties

				#region ColorPickerDialogCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ColorPickerDialogCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColorPickerDialogCommandBase Properties

				#region ColorPickerDialogCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ColorPickerDialogCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColorPickerDialogCloseCommand Properties

				#region ColorPickerDialogOpenCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ColorPickerDialogOpenCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColorPickerDialogOpenCommand Properties

				#region ColorStripsCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ColorStripsCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("ColorStripsCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // ColorStripsCommandSource Properties

				#region XamPickerCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamPickerCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamPickerCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamPickerCommandSource_Properties"))
				);

				#endregion // XamPickerCommandSource Properties

				#region ColorPalette Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ColorPalette");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Colors",
					new DescriptionAttribute(SR.GetString("ColorPalette_Colors_Property")),
				    new DisplayNameAttribute("Colors"),
					new CategoryAttribute(SR.GetString("ColorPalette_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DefaultColor",
					new DescriptionAttribute(SR.GetString("ColorPalette_DefaultColor_Property")),
				    new DisplayNameAttribute("DefaultColor"),
					new CategoryAttribute(SR.GetString("ColorPalette_Properties"))
				);

				#endregion // ColorPalette Properties

				#region ColorPatch Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ColorPatch");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Color",
					new DescriptionAttribute(SR.GetString("ColorPatch_Color_Property")),
				    new DisplayNameAttribute("Color"),
					new CategoryAttribute(SR.GetString("ColorPatch_Properties"))
				);

				#endregion // ColorPatch Properties

				#region ColorPaletteCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ColorPaletteCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColorPaletteCollection Properties

				#region ColorPatchCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ColorPatchCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColorPatchCollection Properties

				#region ColorPickerDialogCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ColorPickerDialogCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("ColorPickerDialogCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // ColorPickerDialogCommandSource Properties

				#region ColorSlidersCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ColorSlidersCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColorSlidersCommandBase Properties

				#region RGBCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.RGBCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RGBCommand Properties

				#region HSLCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.HSLCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HSLCommand Properties

				#region CMYKCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CMYKCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CMYKCommand Properties

				#region AcceptColorCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.AcceptColorCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AcceptColorCommand Properties

				#region PickerCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.PickerCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PickerCommandBase Properties

				#region PickerToggleCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.PickerToggleCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PickerToggleCommand Properties

				#region PickerCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.PickerCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PickerCloseCommand Properties

				#region PreviousPaletteCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.PreviousPaletteCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PreviousPaletteCommand Properties

				#region NextPaletteCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.NextPaletteCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NextPaletteCommand Properties

				#region OpenAdvanceEditorCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.OpenAdvanceEditorCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // OpenAdvanceEditorCommand Properties

				#region XamColorPicker Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamColorPicker");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamColorPickerAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamColorPickerAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviouslySelectedColor",
					new DescriptionAttribute(SR.GetString("XamColorPicker_PreviouslySelectedColor_Property")),
				    new DisplayNameAttribute("PreviouslySelectedColor"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedColor",
					new DescriptionAttribute(SR.GetString("XamColorPicker_SelectedColor_Property")),
				    new DisplayNameAttribute("SelectedColor"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DerivedPalettesCount",
					new DescriptionAttribute(SR.GetString("XamColorPicker_DerivedPalettesCount_Property")),
				    new DisplayNameAttribute("DerivedPalettesCount"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentPalette",
					new DescriptionAttribute(SR.GetString("XamColorPicker_CurrentPalette_Property")),
				    new DisplayNameAttribute("CurrentPalette"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColorPalettes",
					new DescriptionAttribute(SR.GetString("XamColorPicker_ColorPalettes_Property")),
				    new DisplayNameAttribute("ColorPalettes"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDropDownOpen",
					new DescriptionAttribute(SR.GetString("XamColorPicker_IsDropDownOpen_Property")),
				    new DisplayNameAttribute("IsDropDownOpen"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowAdvancedEditorButton",
					new DescriptionAttribute(SR.GetString("XamColorPicker_ShowAdvancedEditorButton_Property")),
				    new DisplayNameAttribute("ShowAdvancedEditorButton"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowRecentColorsPalette",
					new DescriptionAttribute(SR.GetString("XamColorPicker_ShowRecentColorsPalette_Property")),
				    new DisplayNameAttribute("ShowRecentColorsPalette"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowDerivedColorPalettes",
					new DescriptionAttribute(SR.GetString("XamColorPicker_ShowDerivedColorPalettes_Property")),
				    new DisplayNameAttribute("ShowDerivedColorPalettes"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentPaletteCaption",
					new DescriptionAttribute(SR.GetString("XamColorPicker_CurrentPaletteCaption_Property")),
				    new DisplayNameAttribute("CurrentPaletteCaption"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RecentColorPaletteCaption",
					new DescriptionAttribute(SR.GetString("XamColorPicker_RecentColorPaletteCaption_Property")),
				    new DisplayNameAttribute("RecentColorPaletteCaption"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DerivedColorPalettesCaption",
					new DescriptionAttribute(SR.GetString("XamColorPicker_DerivedColorPalettesCaption_Property")),
				    new DisplayNameAttribute("DerivedColorPalettesCaption"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AdvancedButtonCaption",
					new DescriptionAttribute(SR.GetString("XamColorPicker_AdvancedButtonCaption_Property")),
				    new DisplayNameAttribute("AdvancedButtonCaption"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentColorCaption",
					new DescriptionAttribute(SR.GetString("XamColorPicker_CurrentColorCaption_Property")),
				    new DisplayNameAttribute("CurrentColorCaption"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DerivedPaletteColorItemBoxStyle",
					new DescriptionAttribute(SR.GetString("XamColorPicker_DerivedPaletteColorItemBoxStyle_Property")),
				    new DisplayNameAttribute("DerivedPaletteColorItemBoxStyle"),
					new CategoryAttribute(SR.GetString("XamColorPicker_Properties"))
				);

				#endregion // XamColorPicker Properties

				#region ColorStripManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ColorStripManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ColorStrips",
					new DescriptionAttribute(SR.GetString("ColorStripManager_ColorStrips_Property")),
				    new DisplayNameAttribute("ColorStrips")				);

				#endregion // ColorStripManager Properties

				#region ColorItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ColorItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Parent",
					new DescriptionAttribute(SR.GetString("ColorItem_Parent_Property")),
				    new DisplayNameAttribute("Parent")				);


				tableBuilder.AddCustomAttributes(t, "Color",
					new DescriptionAttribute(SR.GetString("ColorItem_Color_Property")),
				    new DisplayNameAttribute("Color")				);

				#endregion // ColorItem Properties

				#region SpecializedEditorsBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.SpecializedEditorsBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SpecializedEditorsBase Properties

				#region ByteTextBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ByteTextBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ByteTextBox Properties

				#region DegreeTextBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.DegreeTextBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DegreeTextBox Properties

				#region PercentTextBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.PercentTextBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PercentTextBox Properties

				#region ColorStrip Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ColorStrip");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "VisibleItemCount",
					new DescriptionAttribute(SR.GetString("ColorStrip_VisibleItemCount_Property")),
				    new DisplayNameAttribute("VisibleItemCount")				);


				tableBuilder.AddCustomAttributes(t, "ColorPalette",
					new DescriptionAttribute(SR.GetString("ColorStrip_ColorPalette_Property")),
				    new DisplayNameAttribute("ColorPalette")				);


				tableBuilder.AddCustomAttributes(t, "SelectedColorItem",
					new DescriptionAttribute(SR.GetString("ColorStrip_SelectedColorItem_Property")),
				    new DisplayNameAttribute("SelectedColorItem")				);


				tableBuilder.AddCustomAttributes(t, "DarknessShift",
					new DescriptionAttribute(SR.GetString("ColorStrip_DarknessShift_Property")),
				    new DisplayNameAttribute("DarknessShift")				);


				tableBuilder.AddCustomAttributes(t, "ColorItemBoxStyle",
					new DescriptionAttribute(SR.GetString("ColorStrip_ColorItemBoxStyle_Property")),
				    new DisplayNameAttribute("ColorItemBoxStyle")				);

				#endregion // ColorStrip Properties

				#region AdvancedColorShadePicker Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.AdvancedColorShadePicker");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentColor",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_CurrentColor_Property")),
				    new DisplayNameAttribute("CurrentColor")				);


				tableBuilder.AddCustomAttributes(t, "SelectedColor",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_SelectedColor_Property")),
				    new DisplayNameAttribute("SelectedColor")				);


				tableBuilder.AddCustomAttributes(t, "Alpha",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_Alpha_Property")),
				    new DisplayNameAttribute("Alpha")				);


				tableBuilder.AddCustomAttributes(t, "Red",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_Red_Property")),
				    new DisplayNameAttribute("Red")				);


				tableBuilder.AddCustomAttributes(t, "Blue",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_Blue_Property")),
				    new DisplayNameAttribute("Blue")				);


				tableBuilder.AddCustomAttributes(t, "Green",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_Green_Property")),
				    new DisplayNameAttribute("Green")				);


				tableBuilder.AddCustomAttributes(t, "H",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_H_Property")),
				    new DisplayNameAttribute("H")				);


				tableBuilder.AddCustomAttributes(t, "S",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_S_Property")),
				    new DisplayNameAttribute("S")				);


				tableBuilder.AddCustomAttributes(t, "L",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_L_Property")),
				    new DisplayNameAttribute("L")				);


				tableBuilder.AddCustomAttributes(t, "C",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_C_Property")),
				    new DisplayNameAttribute("C")				);


				tableBuilder.AddCustomAttributes(t, "M",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_M_Property")),
				    new DisplayNameAttribute("M")				);


				tableBuilder.AddCustomAttributes(t, "Y",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_Y_Property")),
				    new DisplayNameAttribute("Y")				);


				tableBuilder.AddCustomAttributes(t, "K",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_K_Property")),
				    new DisplayNameAttribute("K")				);


				tableBuilder.AddCustomAttributes(t, "AlphaCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_AlphaCaption_Property")),
				    new DisplayNameAttribute("AlphaCaption")				);


				tableBuilder.AddCustomAttributes(t, "RedCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_RedCaption_Property")),
				    new DisplayNameAttribute("RedCaption")				);


				tableBuilder.AddCustomAttributes(t, "BlueCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_BlueCaption_Property")),
				    new DisplayNameAttribute("BlueCaption")				);


				tableBuilder.AddCustomAttributes(t, "GreenCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_GreenCaption_Property")),
				    new DisplayNameAttribute("GreenCaption")				);


				tableBuilder.AddCustomAttributes(t, "HueCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_HueCaption_Property")),
				    new DisplayNameAttribute("HueCaption")				);


				tableBuilder.AddCustomAttributes(t, "SaturationCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_SaturationCaption_Property")),
				    new DisplayNameAttribute("SaturationCaption")				);


				tableBuilder.AddCustomAttributes(t, "LightnessCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_LightnessCaption_Property")),
				    new DisplayNameAttribute("LightnessCaption")				);


				tableBuilder.AddCustomAttributes(t, "CyanCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_CyanCaption_Property")),
				    new DisplayNameAttribute("CyanCaption")				);


				tableBuilder.AddCustomAttributes(t, "YellowCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_YellowCaption_Property")),
				    new DisplayNameAttribute("YellowCaption")				);


				tableBuilder.AddCustomAttributes(t, "MagentaCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_MagentaCaption_Property")),
				    new DisplayNameAttribute("MagentaCaption")				);


				tableBuilder.AddCustomAttributes(t, "BlackCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_BlackCaption_Property")),
				    new DisplayNameAttribute("BlackCaption")				);


				tableBuilder.AddCustomAttributes(t, "OKCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_OKCaption_Property")),
				    new DisplayNameAttribute("OKCaption")				);


				tableBuilder.AddCustomAttributes(t, "CancelCaption",
					new DescriptionAttribute(SR.GetString("AdvancedColorShadePicker_CancelCaption_Property")),
				    new DisplayNameAttribute("CancelCaption")				);

				#endregion // AdvancedColorShadePicker Properties

				#region ColorItemBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ColorItemBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ColorItemBrush",
					new DescriptionAttribute(SR.GetString("ColorItemBox_ColorItemBrush_Property")),
				    new DisplayNameAttribute("ColorItemBrush")				);


				tableBuilder.AddCustomAttributes(t, "ColorItem",
					new DescriptionAttribute(SR.GetString("ColorItemBox_ColorItem_Property")),
				    new DisplayNameAttribute("ColorItem")				);

				#endregion // ColorItemBox Properties

				#region ByteSlider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ByteSlider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("ByteSlider_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue")				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("ByteSlider_MinValue_Property")),
				    new DisplayNameAttribute("MinValue")				);

				#endregion // ByteSlider Properties

				#region SelectedColorItemChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.SelectedColorItemChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OriginalColorItem",
					new DescriptionAttribute(SR.GetString("SelectedColorItemChangedEventArgs_OriginalColorItem_Property")),
				    new DisplayNameAttribute("OriginalColorItem")				);


				tableBuilder.AddCustomAttributes(t, "NewColorItem",
					new DescriptionAttribute(SR.GetString("SelectedColorItemChangedEventArgs_NewColorItem_Property")),
				    new DisplayNameAttribute("NewColorItem")				);

				#endregion // SelectedColorItemChangedEventArgs Properties

				#region SelectedColorChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.SelectedColorChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OriginalColor",
					new DescriptionAttribute(SR.GetString("SelectedColorChangedEventArgs_OriginalColor_Property")),
				    new DisplayNameAttribute("OriginalColor")				);


				tableBuilder.AddCustomAttributes(t, "NewColor",
					new DescriptionAttribute(SR.GetString("SelectedColorChangedEventArgs_NewColor_Property")),
				    new DisplayNameAttribute("NewColor")				);

				#endregion // SelectedColorChangedEventArgs Properties

				#region XamColorPickerAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamColorPickerAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ExpandCollapseState",
					new DescriptionAttribute(SR.GetString("XamColorPickerAutomationPeer_ExpandCollapseState_Property")),
				    new DisplayNameAttribute("ExpandCollapseState"),
					new CategoryAttribute(SR.GetString("XamColorPickerAutomationPeer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("XamColorPickerAutomationPeer_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly"),
					new CategoryAttribute(SR.GetString("XamColorPickerAutomationPeer_Properties"))
				);

				#endregion // XamColorPickerAutomationPeer Properties

				#region CancelColorCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CancelColorCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CancelColorCommand Properties

				#region NullableColorConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.NullableColorConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NullableColorConverter Properties

				#region ColorItemBoxAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.ColorItemBoxAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ColorItemBoxAutomationPeer Properties

				#region ByteSliderThumb Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.ByteSliderThumb");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ByteSliderThumb Properties

				#region SpecializedTextBoxValueConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.SpecializedTextBoxValueConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SpecializedTextBoxValueConverter Properties

				#region TransparencyBackgroundControl Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.TransparencyBackgroundControl");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Color1",
					new DescriptionAttribute(SR.GetString("TransparencyBackgroundControl_Color1_Property")),
				    new DisplayNameAttribute("Color1")				);


				tableBuilder.AddCustomAttributes(t, "Color2",
					new DescriptionAttribute(SR.GetString("TransparencyBackgroundControl_Color2_Property")),
				    new DisplayNameAttribute("Color2")				);


				tableBuilder.AddCustomAttributes(t, "SquareSize",
					new DescriptionAttribute(SR.GetString("TransparencyBackgroundControl_SquareSize_Property")),
				    new DisplayNameAttribute("SquareSize")				);

				#endregion // TransparencyBackgroundControl Properties
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