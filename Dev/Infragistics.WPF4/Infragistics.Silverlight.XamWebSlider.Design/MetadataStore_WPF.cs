using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Editors.XamSlider.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Editors.XamSlider.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Editors.FrequencyType);
				Assembly controlAssembly = t.Assembly;

				#region SliderTickMarks`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.SliderTickMarks`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Owner",
					new DescriptionAttribute(SR.GetString("SliderTickMarks`1_Owner_Property")),
				    new DisplayNameAttribute("Owner")				);


				tableBuilder.AddCustomAttributes(t, "TickMarksFrequency",
					new DescriptionAttribute(SR.GetString("SliderTickMarks`1_TickMarksFrequency_Property")),
				    new DisplayNameAttribute("TickMarksFrequency")				);


				tableBuilder.AddCustomAttributes(t, "TickMarksValues",
					new DescriptionAttribute(SR.GetString("SliderTickMarks`1_TickMarksValues_Property")),
				    new DisplayNameAttribute("TickMarksValues")				);

				#endregion // SliderTickMarks`1 Properties

				#region SliderTickMarksBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.SliderTickMarksBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalTickMarksTemplate",
					new DescriptionAttribute(SR.GetString("SliderTickMarksBase_HorizontalTickMarksTemplate_Property")),
				    new DisplayNameAttribute("HorizontalTickMarksTemplate")				);


				tableBuilder.AddCustomAttributes(t, "IncludeSliderEnds",
					new DescriptionAttribute(SR.GetString("SliderTickMarksBase_IncludeSliderEnds_Property")),
				    new DisplayNameAttribute("IncludeSliderEnds")				);


				tableBuilder.AddCustomAttributes(t, "NumberOfTickMarks",
					new DescriptionAttribute(SR.GetString("SliderTickMarksBase_NumberOfTickMarks_Property")),
				    new DisplayNameAttribute("NumberOfTickMarks")				);


				tableBuilder.AddCustomAttributes(t, "UseFrequency",
					new DescriptionAttribute(SR.GetString("SliderTickMarksBase_UseFrequency_Property")),
				    new DisplayNameAttribute("UseFrequency")				);


				tableBuilder.AddCustomAttributes(t, "VerticalTickMarksTemplate",
					new DescriptionAttribute(SR.GetString("SliderTickMarksBase_VerticalTickMarksTemplate_Property")),
				    new DisplayNameAttribute("VerticalTickMarksTemplate")				);

				#endregion // SliderTickMarksBase Properties

				#region SliderTickMarks Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.SliderTickMarks");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarksValues",
					new DescriptionAttribute(SR.GetString("SliderTickMarks_TickMarksValues_Property")),
				    new DisplayNameAttribute("TickMarksValues")				);


				tableBuilder.AddCustomAttributes(t, "Owner",
					new DescriptionAttribute(SR.GetString("SliderTickMarks`1_Owner_Property")),
				    new DisplayNameAttribute("Owner")				);


				tableBuilder.AddCustomAttributes(t, "TickMarksFrequency",
					new DescriptionAttribute(SR.GetString("SliderTickMarks`1_TickMarksFrequency_Property")),
				    new DisplayNameAttribute("TickMarksFrequency")				);


				tableBuilder.AddCustomAttributes(t, "TickMarksValues",
					new DescriptionAttribute(SR.GetString("SliderTickMarks`1_TickMarksValues_Property")),
				    new DisplayNameAttribute("TickMarksValues")				);

				#endregion // SliderTickMarks Properties

				#region ThumbValueChangedEventArgs`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ThumbValueChangedEventArgs`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OldValue",
					new DescriptionAttribute(SR.GetString("ThumbValueChangedEventArgs`1_OldValue_Property")),
				    new DisplayNameAttribute("OldValue")				);


				tableBuilder.AddCustomAttributes(t, "NewValue",
					new DescriptionAttribute(SR.GetString("ThumbValueChangedEventArgs`1_NewValue_Property")),
				    new DisplayNameAttribute("NewValue")				);


				tableBuilder.AddCustomAttributes(t, "Thumb",
					new DescriptionAttribute(SR.GetString("ThumbValueChangedEventArgs`1_Thumb_Property")),
				    new DisplayNameAttribute("Thumb")				);

				#endregion // ThumbValueChangedEventArgs`1 Properties

				#region TrackClickEventArgs`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.TrackClickEventArgs`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("TrackClickEventArgs`1_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // TrackClickEventArgs`1 Properties

				#region DateConverter Properties
				t = controlAssembly.GetType("Infragistics.DateConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateConverter Properties

				#region ThumbComparer`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.ThumbComparer`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ThumbComparer`1 Properties

				#region DateTimeSliderTickMarks Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.DateTimeSliderTickMarks");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FrequencyType",
					new DescriptionAttribute(SR.GetString("DateTimeSliderTickMarks_FrequencyType_Property")),
				    new DisplayNameAttribute("FrequencyType")				);


				tableBuilder.AddCustomAttributes(t, "TickMarksValues",
					new DescriptionAttribute(SR.GetString("DateTimeSliderTickMarks_TickMarksValues_Property")),
				    new DisplayNameAttribute("TickMarksValues")				);


				tableBuilder.AddCustomAttributes(t, "Owner",
					new DescriptionAttribute(SR.GetString("SliderTickMarks`1_Owner_Property")),
				    new DisplayNameAttribute("Owner")				);


				tableBuilder.AddCustomAttributes(t, "TickMarksFrequency",
					new DescriptionAttribute(SR.GetString("SliderTickMarks`1_TickMarksFrequency_Property")),
				    new DisplayNameAttribute("TickMarksFrequency")				);


				tableBuilder.AddCustomAttributes(t, "TickMarksValues",
					new DescriptionAttribute(SR.GetString("SliderTickMarks`1_TickMarksValues_Property")),
				    new DisplayNameAttribute("TickMarksValues")				);

				#endregion // DateTimeSliderTickMarks Properties

				#region DateTimeConverter Properties
				t = controlAssembly.GetType("Infragistics.DateTimeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateTimeConverter Properties

				#region TickMarksPanel`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.TickMarksPanel`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Owner",
					new DescriptionAttribute(SR.GetString("TickMarksPanel`1_Owner_Property")),
				    new DisplayNameAttribute("Owner")				);

				#endregion // TickMarksPanel`1 Properties

				#region StringToDateConverter Properties
				t = controlAssembly.GetType("Infragistics.StringToDateConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StringToDateConverter Properties

				#region XamDateTimeRangeSlider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamDateTimeRangeSlider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamSliderAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamSliderAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamDateTimeRangeSlider_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamDateTimeRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamDateTimeRangeSlider_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamDateTimeRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamDateTimeRangeSlider_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamDateTimeRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamDateTimeRangeSlider_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamDateTimeRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamDateTimeRangeSlider_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamDateTimeRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Thumbs",
					new DescriptionAttribute(SR.GetString("XamDateTimeRangeSlider_Thumbs_Property")),
				    new DisplayNameAttribute("Thumbs"),
					new CategoryAttribute(SR.GetString("XamDateTimeRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveThumb",
					new DescriptionAttribute(SR.GetString("XamDateTimeRangeSlider_ActiveThumb_Property")),
				    new DisplayNameAttribute("ActiveThumb"),
					new CategoryAttribute(SR.GetString("XamDateTimeRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionRangeEnabled",
					new DescriptionAttribute(SR.GetString("XamDateTimeRangeSlider_IsSelectionRangeEnabled_Property")),
				    new DisplayNameAttribute("IsSelectionRangeEnabled"),
					new CategoryAttribute(SR.GetString("XamDateTimeRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChangeType",
					new DescriptionAttribute(SR.GetString("XamDateTimeRangeSlider_LargeChangeType_Property")),
				    new DisplayNameAttribute("LargeChangeType"),
					new CategoryAttribute(SR.GetString("XamDateTimeRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChangeType",
					new DescriptionAttribute(SR.GetString("XamDateTimeRangeSlider_SmallChangeType_Property")),
				    new DisplayNameAttribute("SmallChangeType"),
					new CategoryAttribute(SR.GetString("XamDateTimeRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveThumb",
					new DescriptionAttribute(SR.GetString("XamRangeSlider`1_ActiveThumb_Property")),
				    new DisplayNameAttribute("ActiveThumb"),
					new CategoryAttribute(SR.GetString("XamRangeSlider`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionRangeEnabled",
					new DescriptionAttribute(SR.GetString("XamRangeSlider`1_IsSelectionRangeEnabled_Property")),
				    new DisplayNameAttribute("IsSelectionRangeEnabled"),
					new CategoryAttribute(SR.GetString("XamRangeSlider`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Thumbs",
					new DescriptionAttribute(SR.GetString("XamRangeSlider`1_Thumbs_Property")),
				    new DisplayNameAttribute("Thumbs"),
					new CategoryAttribute(SR.GetString("XamRangeSlider`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);

				#endregion // XamDateTimeRangeSlider Properties

				#region XamRangeSlider`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamRangeSlider`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveThumb",
					new DescriptionAttribute(SR.GetString("XamRangeSlider`1_ActiveThumb_Property")),
				    new DisplayNameAttribute("ActiveThumb"),
					new CategoryAttribute(SR.GetString("XamRangeSlider`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionRangeEnabled",
					new DescriptionAttribute(SR.GetString("XamRangeSlider`1_IsSelectionRangeEnabled_Property")),
				    new DisplayNameAttribute("IsSelectionRangeEnabled"),
					new CategoryAttribute(SR.GetString("XamRangeSlider`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Thumbs",
					new DescriptionAttribute(SR.GetString("XamRangeSlider`1_Thumbs_Property")),
				    new DisplayNameAttribute("Thumbs"),
					new CategoryAttribute(SR.GetString("XamRangeSlider`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);

				#endregion // XamRangeSlider`1 Properties

				#region XamSliderBase`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamSliderBase`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);

				#endregion // XamSliderBase`1 Properties

				#region XamSliderBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamSliderBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DecreaseButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamSliderBase_DecreaseButtonVisibility_Property")),
				    new DisplayNameAttribute("DecreaseButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EnableKeyboardNavigation",
					new DescriptionAttribute(SR.GetString("XamSliderBase_EnableKeyboardNavigation_Property")),
				    new DisplayNameAttribute("EnableKeyboardNavigation"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalTickMarksTemplate",
					new DescriptionAttribute(SR.GetString("XamSliderBase_HorizontalTickMarksTemplate_Property")),
				    new DisplayNameAttribute("HorizontalTickMarksTemplate"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IncreaseButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamSliderBase_IncreaseButtonVisibility_Property")),
				    new DisplayNameAttribute("IncreaseButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDirectionReversed",
					new DescriptionAttribute(SR.GetString("XamSliderBase_IsDirectionReversed_Property")),
				    new DisplayNameAttribute("IsDirectionReversed"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsMouseWheelEnabled",
					new DescriptionAttribute(SR.GetString("XamSliderBase_IsMouseWheelEnabled_Property")),
				    new DisplayNameAttribute("IsMouseWheelEnabled"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("XamSliderBase_Orientation_Property")),
				    new DisplayNameAttribute("Orientation"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ThumbStyle",
					new DescriptionAttribute(SR.GetString("XamSliderBase_ThumbStyle_Property")),
				    new DisplayNameAttribute("ThumbStyle"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrackFillBrush",
					new DescriptionAttribute(SR.GetString("XamSliderBase_TrackFillBrush_Property")),
				    new DisplayNameAttribute("TrackFillBrush"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrackFillStyle",
					new DescriptionAttribute(SR.GetString("XamSliderBase_TrackFillStyle_Property")),
				    new DisplayNameAttribute("TrackFillStyle"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrackClickAction",
					new DescriptionAttribute(SR.GetString("XamSliderBase_TrackClickAction_Property")),
				    new DisplayNameAttribute("TrackClickAction"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalTickMarksTemplate",
					new DescriptionAttribute(SR.GetString("XamSliderBase_VerticalTickMarksTemplate_Property")),
				    new DisplayNameAttribute("VerticalTickMarksTemplate"),
					new CategoryAttribute(SR.GetString("XamSliderBase_Properties"))
				);

				#endregion // XamSliderBase Properties

				#region XamDateTimeSlider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamDateTimeSlider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamSliderAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamSliderAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamDateTimeSlider_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamDateTimeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamDateTimeSlider_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamDateTimeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamDateTimeSlider_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamDateTimeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamDateTimeSlider_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamDateTimeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamDateTimeSlider_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamDateTimeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamDateTimeSlider_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamDateTimeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Thumb",
					new DescriptionAttribute(SR.GetString("XamDateTimeSlider_Thumb_Property")),
				    new DisplayNameAttribute("Thumb"),
					new CategoryAttribute(SR.GetString("XamDateTimeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChangeType",
					new DescriptionAttribute(SR.GetString("XamDateTimeSlider_LargeChangeType_Property")),
				    new DisplayNameAttribute("LargeChangeType"),
					new CategoryAttribute(SR.GetString("XamDateTimeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChangeType",
					new DescriptionAttribute(SR.GetString("XamDateTimeSlider_SmallChangeType_Property")),
				    new DisplayNameAttribute("SmallChangeType"),
					new CategoryAttribute(SR.GetString("XamDateTimeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Thumb",
					new DescriptionAttribute(SR.GetString("XamSimpleSliderBase`1_Thumb_Property")),
				    new DisplayNameAttribute("Thumb"),
					new CategoryAttribute(SR.GetString("XamSimpleSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamSimpleSliderBase`1_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSimpleSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);

				#endregion // XamDateTimeSlider Properties

				#region XamSimpleSliderBase`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamSimpleSliderBase`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Thumb",
					new DescriptionAttribute(SR.GetString("XamSimpleSliderBase`1_Thumb_Property")),
				    new DisplayNameAttribute("Thumb"),
					new CategoryAttribute(SR.GetString("XamSimpleSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamSimpleSliderBase`1_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSimpleSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);

				#endregion // XamSimpleSliderBase`1 Properties

				#region XamNumericRangeSlider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamNumericRangeSlider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamSliderAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamSliderAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveThumb",
					new DescriptionAttribute(SR.GetString("XamNumericRangeSlider_ActiveThumb_Property")),
				    new DisplayNameAttribute("ActiveThumb"),
					new CategoryAttribute(SR.GetString("XamNumericRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionRangeEnabled",
					new DescriptionAttribute(SR.GetString("XamNumericRangeSlider_IsSelectionRangeEnabled_Property")),
				    new DisplayNameAttribute("IsSelectionRangeEnabled"),
					new CategoryAttribute(SR.GetString("XamNumericRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamNumericRangeSlider_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamNumericRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Thumbs",
					new DescriptionAttribute(SR.GetString("XamNumericRangeSlider_Thumbs_Property")),
				    new DisplayNameAttribute("Thumbs"),
					new CategoryAttribute(SR.GetString("XamNumericRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamNumericRangeSlider_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamNumericRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamNumericRangeSlider_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamNumericRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamNumericRangeSlider_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamNumericRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamNumericRangeSlider_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamNumericRangeSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveThumb",
					new DescriptionAttribute(SR.GetString("XamRangeSlider`1_ActiveThumb_Property")),
				    new DisplayNameAttribute("ActiveThumb"),
					new CategoryAttribute(SR.GetString("XamRangeSlider`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionRangeEnabled",
					new DescriptionAttribute(SR.GetString("XamRangeSlider`1_IsSelectionRangeEnabled_Property")),
				    new DisplayNameAttribute("IsSelectionRangeEnabled"),
					new CategoryAttribute(SR.GetString("XamRangeSlider`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Thumbs",
					new DescriptionAttribute(SR.GetString("XamRangeSlider`1_Thumbs_Property")),
				    new DisplayNameAttribute("Thumbs"),
					new CategoryAttribute(SR.GetString("XamRangeSlider`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);

				#endregion // XamNumericRangeSlider Properties

				#region XamNumericSlider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamNumericSlider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamSliderAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamSliderAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamNumericSlider_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamNumericSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamNumericSlider_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamNumericSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamNumericSlider_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamNumericSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamNumericSlider_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamNumericSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamNumericSlider_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamNumericSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamNumericSlider_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamNumericSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Thumb",
					new DescriptionAttribute(SR.GetString("XamNumericSlider_Thumb_Property")),
				    new DisplayNameAttribute("Thumb"),
					new CategoryAttribute(SR.GetString("XamNumericSlider_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Thumb",
					new DescriptionAttribute(SR.GetString("XamSimpleSliderBase`1_Thumb_Property")),
				    new DisplayNameAttribute("Thumb"),
					new CategoryAttribute(SR.GetString("XamSimpleSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamSimpleSliderBase`1_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSimpleSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickMarks",
					new DescriptionAttribute(SR.GetString("XamSliderBase`1_TickMarks_Property")),
				    new DisplayNameAttribute("TickMarks"),
					new CategoryAttribute(SR.GetString("XamSliderBase`1_Properties"))
				);

				#endregion // XamNumericSlider Properties

				#region LargeDecreaseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.LargeDecreaseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LargeDecreaseCommand Properties

				#region XamSliderBaseCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.XamSliderBaseCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamSliderBaseCommandBase Properties

				#region LargeIncreaseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.LargeIncreaseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LargeIncreaseCommand Properties

				#region SmallDecreaseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.SmallDecreaseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SmallDecreaseCommand Properties

				#region SmallIncreaseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.SmallIncreaseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SmallIncreaseCommand Properties

				#region XamSliderBaseCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamSliderBaseCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamSliderBaseCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamSliderBaseCommandSource_Properties"))
				);

				#endregion // XamSliderBaseCommandSource Properties

				#region XamSliderDateTimeThumb Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamSliderDateTimeThumb");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamSliderSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamSliderSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamSliderDateTimeThumb_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSliderDateTimeThumb_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSnapToTickEnabled",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_IsSnapToTickEnabled_Property")),
				    new DisplayNameAttribute("IsSnapToTickEnabled"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsTrackFillVisible",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_IsTrackFillVisible_Property")),
				    new DisplayNameAttribute("IsTrackFillVisible"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrackFillBrush",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_TrackFillBrush_Property")),
				    new DisplayNameAttribute("TrackFillBrush"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrackFillStyle",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_TrackFillStyle_Property")),
				    new DisplayNameAttribute("TrackFillStyle"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);

				#endregion // XamSliderDateTimeThumb Properties

				#region XamSliderThumb`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamSliderThumb`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsSnapToTickEnabled",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_IsSnapToTickEnabled_Property")),
				    new DisplayNameAttribute("IsSnapToTickEnabled"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsTrackFillVisible",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_IsTrackFillVisible_Property")),
				    new DisplayNameAttribute("IsTrackFillVisible"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrackFillBrush",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_TrackFillBrush_Property")),
				    new DisplayNameAttribute("TrackFillBrush"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrackFillStyle",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_TrackFillStyle_Property")),
				    new DisplayNameAttribute("TrackFillStyle"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);

				#endregion // XamSliderThumb`1 Properties

				#region XamSliderThumbBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamSliderThumbBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InteractionMode",
					new DescriptionAttribute(SR.GetString("XamSliderThumbBase_InteractionMode_Property")),
				    new DisplayNameAttribute("InteractionMode"),
					new CategoryAttribute(SR.GetString("XamSliderThumbBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsActive",
					new DescriptionAttribute(SR.GetString("XamSliderThumbBase_IsActive_Property")),
				    new DisplayNameAttribute("IsActive"),
					new CategoryAttribute(SR.GetString("XamSliderThumbBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Owner",
					new DescriptionAttribute(SR.GetString("XamSliderThumbBase_Owner_Property")),
				    new DisplayNameAttribute("Owner"),
					new CategoryAttribute(SR.GetString("XamSliderThumbBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDragEnabled",
					new DescriptionAttribute(SR.GetString("XamSliderThumbBase_IsDragEnabled_Property")),
				    new DisplayNameAttribute("IsDragEnabled"),
					new CategoryAttribute(SR.GetString("XamSliderThumbBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ToolTipTemplate",
					new DescriptionAttribute(SR.GetString("XamSliderThumbBase_ToolTipTemplate_Property")),
				    new DisplayNameAttribute("ToolTipTemplate"),
					new CategoryAttribute(SR.GetString("XamSliderThumbBase_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ToolTipVisibility",
					new DescriptionAttribute(SR.GetString("XamSliderThumbBase_ToolTipVisibility_Property")),
				    new DisplayNameAttribute("ToolTipVisibility"),
					new CategoryAttribute(SR.GetString("XamSliderThumbBase_Properties"))
				);

				#endregion // XamSliderThumbBase Properties

				#region XamSliderNumericThumb Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamSliderNumericThumb");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(SR.GetString("XamSliderSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamSliderSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamSliderNumericThumb_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSliderNumericThumb_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSnapToTickEnabled",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_IsSnapToTickEnabled_Property")),
				    new DisplayNameAttribute("IsSnapToTickEnabled"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsTrackFillVisible",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_IsTrackFillVisible_Property")),
				    new DisplayNameAttribute("IsTrackFillVisible"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrackFillBrush",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_TrackFillBrush_Property")),
				    new DisplayNameAttribute("TrackFillBrush"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrackFillStyle",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_TrackFillStyle_Property")),
				    new DisplayNameAttribute("TrackFillStyle"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamSliderThumb`1_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSliderThumb`1_Properties"))
				);

				#endregion // XamSliderNumericThumb Properties

				#region TrackFill Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.TrackFill");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TrackFill Properties

				#region XamSliderBaseAutomationPeer`1 Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamSliderBaseAutomationPeer`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CanSelectMultiple",
					new DescriptionAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_CanSelectMultiple_Property")),
				    new DisplayNameAttribute("CanSelectMultiple"),
					new CategoryAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionRequired",
					new DescriptionAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_IsSelectionRequired_Property")),
				    new DisplayNameAttribute("IsSelectionRequired"),
					new CategoryAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly"),
					new CategoryAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LargeChange",
					new DescriptionAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_LargeChange_Property")),
				    new DisplayNameAttribute("LargeChange"),
					new CategoryAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Maximum",
					new DescriptionAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Maximum_Property")),
				    new DisplayNameAttribute("Maximum"),
					new CategoryAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Minimum",
					new DescriptionAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Minimum_Property")),
				    new DisplayNameAttribute("Minimum"),
					new CategoryAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SmallChange",
					new DescriptionAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_SmallChange_Property")),
				    new DisplayNameAttribute("SmallChange"),
					new CategoryAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSliderBaseAutomationPeer`1_Properties"))
				);

				#endregion // XamSliderBaseAutomationPeer`1 Properties

				#region XamSliderThumbAutomationPeer`1 Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamSliderThumbAutomationPeer`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("XamSliderThumbAutomationPeer`1_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected"),
					new CategoryAttribute(SR.GetString("XamSliderThumbAutomationPeer`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectionContainer",
					new DescriptionAttribute(SR.GetString("XamSliderThumbAutomationPeer`1_SelectionContainer_Property")),
				    new DisplayNameAttribute("SelectionContainer"),
					new CategoryAttribute(SR.GetString("XamSliderThumbAutomationPeer`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("XamSliderThumbAutomationPeer`1_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly"),
					new CategoryAttribute(SR.GetString("XamSliderThumbAutomationPeer`1_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("XamSliderThumbAutomationPeer`1_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSliderThumbAutomationPeer`1_Properties"))
				);

				#endregion // XamSliderThumbAutomationPeer`1 Properties

				#region TrackFillChangedEventArgs`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.TrackFillChangedEventArgs`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LesserThumb",
					new DescriptionAttribute(SR.GetString("TrackFillChangedEventArgs`1_LesserThumb_Property")),
				    new DisplayNameAttribute("LesserThumb")				);


				tableBuilder.AddCustomAttributes(t, "GreaterThumb",
					new DescriptionAttribute(SR.GetString("TrackFillChangedEventArgs`1_GreaterThumb_Property")),
				    new DisplayNameAttribute("GreaterThumb")				);

				#endregion // TrackFillChangedEventArgs`1 Properties
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