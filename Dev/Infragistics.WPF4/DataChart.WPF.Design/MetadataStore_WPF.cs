using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Charts.XamDataChart.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Charts.XamDataChart.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Charts.XamDataChart);
				Assembly controlAssembly = t.Assembly;

				#region ChartCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ChartCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ChartCollection Properties

				#region AxisCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AxisCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AxisCollection Properties

				#region IndicatorCalculationStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.IndicatorCalculationStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // IndicatorCalculationStrategy Properties

				#region StreamingIndicatorCalculationStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StreamingIndicatorCalculationStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StreamingIndicatorCalculationStrategy Properties

				#region StochRSIIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.StochRSIIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StochRSIIndicatorStrategy Properties

				#region MassIndexIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.MassIndexIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MassIndexIndicatorStrategy Properties

				#region AverageTrueRangeIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.AverageTrueRangeIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AverageTrueRangeIndicatorStrategy Properties

				#region PercentagePriceOscillatorIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.PercentagePriceOscillatorIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PercentagePriceOscillatorIndicatorStrategy Properties

				#region PositiveVolumeIndexIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.PositiveVolumeIndexIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PositiveVolumeIndexIndicatorStrategy Properties

				#region CustomIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CustomIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CustomIndicatorStrategy Properties

				#region XamDataChart Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.XamDataChart");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDataChartAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDataChartAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentPresenter",
					new DescriptionAttribute(SR.GetString("XamDataChart_ContentPresenter_Property")),
				    new DisplayNameAttribute("ContentPresenter"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalZoombar",
					new DescriptionAttribute(SR.GetString("XamDataChart_HorizontalZoombar_Property")),
				    new DisplayNameAttribute("HorizontalZoombar"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalZoombar",
					new DescriptionAttribute(SR.GetString("XamDataChart_VerticalZoombar_Property")),
				    new DisplayNameAttribute("VerticalZoombar"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviewPathStyle",
					new DescriptionAttribute(SR.GetString("XamDataChart_PreviewPathStyle_Property")),
				    new DisplayNameAttribute("PreviewPathStyle"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalZoomable",
					new DescriptionAttribute(SR.GetString("XamDataChart_HorizontalZoomable_Property")),
				    new DisplayNameAttribute("HorizontalZoomable"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalZoombarVisibility",
					new DescriptionAttribute(SR.GetString("XamDataChart_HorizontalZoombarVisibility_Property")),
				    new DisplayNameAttribute("HorizontalZoombarVisibility"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalZoomable",
					new DescriptionAttribute(SR.GetString("XamDataChart_VerticalZoomable_Property")),
				    new DisplayNameAttribute("VerticalZoomable"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalZoombarVisibility",
					new DescriptionAttribute(SR.GetString("XamDataChart_VerticalZoombarVisibility_Property")),
				    new DisplayNameAttribute("VerticalZoombarVisibility"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSquare",
					new DescriptionAttribute(SR.GetString("XamDataChart_IsSquare_Property")),
				    new DisplayNameAttribute("IsSquare"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ZoombarStyle",
					new DescriptionAttribute(SR.GetString("XamDataChart_ZoombarStyle_Property")),
				    new DisplayNameAttribute("ZoombarStyle"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "GridMode",
					new DescriptionAttribute(SR.GetString("XamDataChart_GridMode_Property")),
				    new DisplayNameAttribute("GridMode"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaBorderBrush",
					new DescriptionAttribute(SR.GetString("XamDataChart_PlotAreaBorderBrush_Property")),
				    new DisplayNameAttribute("PlotAreaBorderBrush"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaBorderThickness",
					new DescriptionAttribute(SR.GetString("XamDataChart_PlotAreaBorderThickness_Property")),
				    new DisplayNameAttribute("PlotAreaBorderThickness"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaBackground",
					new DescriptionAttribute(SR.GetString("XamDataChart_PlotAreaBackground_Property")),
				    new DisplayNameAttribute("PlotAreaBackground"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaBackgroundContent",
					new DescriptionAttribute(SR.GetString("XamDataChart_PlotAreaBackgroundContent_Property")),
				    new DisplayNameAttribute("PlotAreaBackgroundContent"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaMinWidth",
					new DescriptionAttribute(SR.GetString("XamDataChart_PlotAreaMinWidth_Property")),
				    new DisplayNameAttribute("PlotAreaMinWidth"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaMinHeight",
					new DescriptionAttribute(SR.GetString("XamDataChart_PlotAreaMinHeight_Property")),
				    new DisplayNameAttribute("PlotAreaMinHeight"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Legend",
					new DescriptionAttribute(SR.GetString("XamDataChart_Legend_Property")),
				    new DisplayNameAttribute("Legend"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CrosshairLineStyle",
					new DescriptionAttribute(SR.GetString("XamDataChart_CrosshairLineStyle_Property")),
				    new DisplayNameAttribute("CrosshairLineStyle"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DefaultInteraction",
					new DescriptionAttribute(SR.GetString("XamDataChart_DefaultInteraction_Property")),
				    new DisplayNameAttribute("DefaultInteraction"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ToolTipStyle",
					new DescriptionAttribute(SR.GetString("XamDataChart_ToolTipStyle_Property")),
				    new DisplayNameAttribute("ToolTipStyle"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Brushes",
					new DescriptionAttribute(SR.GetString("XamDataChart_Brushes_Property")),
				    new DisplayNameAttribute("Brushes"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MarkerBrushes",
					new DescriptionAttribute(SR.GetString("XamDataChart_MarkerBrushes_Property")),
				    new DisplayNameAttribute("MarkerBrushes"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Outlines",
					new DescriptionAttribute(SR.GetString("XamDataChart_Outlines_Property")),
				    new DisplayNameAttribute("Outlines"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MarkerOutlines",
					new DescriptionAttribute(SR.GetString("XamDataChart_MarkerOutlines_Property")),
				    new DisplayNameAttribute("MarkerOutlines"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CrosshairVisibility",
					new DescriptionAttribute(SR.GetString("XamDataChart_CrosshairVisibility_Property")),
				    new DisplayNameAttribute("CrosshairVisibility"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DragModifier",
					new DescriptionAttribute(SR.GetString("XamDataChart_DragModifier_Property")),
				    new DisplayNameAttribute("DragModifier"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PanModifier",
					new DescriptionAttribute(SR.GetString("XamDataChart_PanModifier_Property")),
				    new DisplayNameAttribute("PanModifier"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CircleMarkerTemplate",
					new DescriptionAttribute(SR.GetString("XamDataChart_CircleMarkerTemplate_Property")),
				    new DisplayNameAttribute("CircleMarkerTemplate"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TriangleMarkerTemplate",
					new DescriptionAttribute(SR.GetString("XamDataChart_TriangleMarkerTemplate_Property")),
				    new DisplayNameAttribute("TriangleMarkerTemplate"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PyramidMarkerTemplate",
					new DescriptionAttribute(SR.GetString("XamDataChart_PyramidMarkerTemplate_Property")),
				    new DisplayNameAttribute("PyramidMarkerTemplate"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SquareMarkerTemplate",
					new DescriptionAttribute(SR.GetString("XamDataChart_SquareMarkerTemplate_Property")),
				    new DisplayNameAttribute("SquareMarkerTemplate"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DiamondMarkerTemplate",
					new DescriptionAttribute(SR.GetString("XamDataChart_DiamondMarkerTemplate_Property")),
				    new DisplayNameAttribute("DiamondMarkerTemplate"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PentagonMarkerTemplate",
					new DescriptionAttribute(SR.GetString("XamDataChart_PentagonMarkerTemplate_Property")),
				    new DisplayNameAttribute("PentagonMarkerTemplate"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HexagonMarkerTemplate",
					new DescriptionAttribute(SR.GetString("XamDataChart_HexagonMarkerTemplate_Property")),
				    new DisplayNameAttribute("HexagonMarkerTemplate"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TetragramMarkerTemplate",
					new DescriptionAttribute(SR.GetString("XamDataChart_TetragramMarkerTemplate_Property")),
				    new DisplayNameAttribute("TetragramMarkerTemplate"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PentagramMarkerTemplate",
					new DescriptionAttribute(SR.GetString("XamDataChart_PentagramMarkerTemplate_Property")),
				    new DisplayNameAttribute("PentagramMarkerTemplate"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HexagramMarkerTemplate",
					new DescriptionAttribute(SR.GetString("XamDataChart_HexagramMarkerTemplate_Property")),
				    new DisplayNameAttribute("HexagramMarkerTemplate"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WindowRect",
					new DescriptionAttribute(SR.GetString("XamDataChart_WindowRect_Property")),
				    new DisplayNameAttribute("WindowRect"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WindowPositionHorizontal",
					new DescriptionAttribute(SR.GetString("XamDataChart_WindowPositionHorizontal_Property")),
				    new DisplayNameAttribute("WindowPositionHorizontal"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WindowPositionVertical",
					new DescriptionAttribute(SR.GetString("XamDataChart_WindowPositionVertical_Property")),
				    new DisplayNameAttribute("WindowPositionVertical"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WindowScaleHorizontal",
					new DescriptionAttribute(SR.GetString("XamDataChart_WindowScaleHorizontal_Property")),
				    new DisplayNameAttribute("WindowScaleHorizontal"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WindowScaleVertical",
					new DescriptionAttribute(SR.GetString("XamDataChart_WindowScaleVertical_Property")),
				    new DisplayNameAttribute("WindowScaleVertical"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Axes",
					new DescriptionAttribute(SR.GetString("XamDataChart_Axes_Property")),
				    new DisplayNameAttribute("Axes"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Series",
					new DescriptionAttribute(SR.GetString("XamDataChart_Series_Property")),
				    new DisplayNameAttribute("Series"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "GridZIndex",
					new DescriptionAttribute(SR.GetString("XamDataChart_GridZIndex_Property")),
				    new DisplayNameAttribute("GridZIndex"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SeriesZIndex",
					new DescriptionAttribute(SR.GetString("XamDataChart_SeriesZIndex_Property")),
				    new DisplayNameAttribute("SeriesZIndex"),
					new CategoryAttribute(SR.GetString("XamDataChart_Properties"))
				);

				#endregion // XamDataChart Properties

				#region NumericYAxis Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.NumericYAxis");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NumericYAxis Properties

				#region NumericAxisBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.NumericAxisBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MinimumValue",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_MinimumValue_Property")),
				    new DisplayNameAttribute("MinimumValue")				);


				tableBuilder.AddCustomAttributes(t, "ActualMinimumValue",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_ActualMinimumValue_Property")),
				    new DisplayNameAttribute("ActualMinimumValue")				);


				tableBuilder.AddCustomAttributes(t, "MaximumValue",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_MaximumValue_Property")),
				    new DisplayNameAttribute("MaximumValue")				);


				tableBuilder.AddCustomAttributes(t, "ActualMaximumValue",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_ActualMaximumValue_Property")),
				    new DisplayNameAttribute("ActualMaximumValue")				);


				tableBuilder.AddCustomAttributes(t, "Interval",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_Interval_Property")),
				    new DisplayNameAttribute("Interval")				);


				tableBuilder.AddCustomAttributes(t, "ReferenceValue",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_ReferenceValue_Property")),
				    new DisplayNameAttribute("ReferenceValue")				);


				tableBuilder.AddCustomAttributes(t, "IsLogarithmic",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_IsLogarithmic_Property")),
				    new DisplayNameAttribute("IsLogarithmic")				);


				tableBuilder.AddCustomAttributes(t, "LogarithmBase",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_LogarithmBase_Property")),
				    new DisplayNameAttribute("LogarithmBase")				);


				tableBuilder.AddCustomAttributes(t, "ActualIsLogarithmic",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_ActualIsLogarithmic_Property")),
				    new DisplayNameAttribute("ActualIsLogarithmic")				);


				tableBuilder.AddCustomAttributes(t, "HasUserMinimum",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_HasUserMinimum_Property")),
				    new DisplayNameAttribute("HasUserMinimum")				);


				tableBuilder.AddCustomAttributes(t, "HasUserMaximum",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_HasUserMaximum_Property")),
				    new DisplayNameAttribute("HasUserMaximum")				);


				tableBuilder.AddCustomAttributes(t, "TickmarkValues",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_TickmarkValues_Property")),
				    new DisplayNameAttribute("TickmarkValues")				);


				tableBuilder.AddCustomAttributes(t, "ActualTickmarkValues",
					new DescriptionAttribute(SR.GetString("NumericAxisBase_ActualTickmarkValues_Property")),
				    new DisplayNameAttribute("ActualTickmarkValues")				);

				#endregion // NumericAxisBase Properties

				#region Axis Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Axis");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RootCanvas",
					new DescriptionAttribute(SR.GetString("Axis_RootCanvas_Property")),
				    new DisplayNameAttribute("RootCanvas")				);


				tableBuilder.AddCustomAttributes(t, "Chart",
					new DescriptionAttribute(SR.GetString("Axis_Chart_Property")),
				    new DisplayNameAttribute("Chart")				);


				tableBuilder.AddCustomAttributes(t, "Stroke",
					new DescriptionAttribute(SR.GetString("Axis_Stroke_Property")),
				    new DisplayNameAttribute("Stroke")				);


				tableBuilder.AddCustomAttributes(t, "StrokeThickness",
					new DescriptionAttribute(SR.GetString("Axis_StrokeThickness_Property")),
				    new DisplayNameAttribute("StrokeThickness")				);


				tableBuilder.AddCustomAttributes(t, "StrokeDashArray",
					new DescriptionAttribute(SR.GetString("Axis_StrokeDashArray_Property")),
				    new DisplayNameAttribute("StrokeDashArray")				);


				tableBuilder.AddCustomAttributes(t, "Strip",
					new DescriptionAttribute(SR.GetString("Axis_Strip_Property")),
				    new DisplayNameAttribute("Strip")				);


				tableBuilder.AddCustomAttributes(t, "MajorStroke",
					new DescriptionAttribute(SR.GetString("Axis_MajorStroke_Property")),
				    new DisplayNameAttribute("MajorStroke")				);


				tableBuilder.AddCustomAttributes(t, "MajorStrokeThickness",
					new DescriptionAttribute(SR.GetString("Axis_MajorStrokeThickness_Property")),
				    new DisplayNameAttribute("MajorStrokeThickness")				);


				tableBuilder.AddCustomAttributes(t, "MajorStrokeDashArray",
					new DescriptionAttribute(SR.GetString("Axis_MajorStrokeDashArray_Property")),
				    new DisplayNameAttribute("MajorStrokeDashArray")				);


				tableBuilder.AddCustomAttributes(t, "MinorStroke",
					new DescriptionAttribute(SR.GetString("Axis_MinorStroke_Property")),
				    new DisplayNameAttribute("MinorStroke")				);


				tableBuilder.AddCustomAttributes(t, "MinorStrokeThickness",
					new DescriptionAttribute(SR.GetString("Axis_MinorStrokeThickness_Property")),
				    new DisplayNameAttribute("MinorStrokeThickness")				);


				tableBuilder.AddCustomAttributes(t, "MinorStrokeDashArray",
					new DescriptionAttribute(SR.GetString("Axis_MinorStrokeDashArray_Property")),
				    new DisplayNameAttribute("MinorStrokeDashArray")				);


				tableBuilder.AddCustomAttributes(t, "IsInverted",
					new DescriptionAttribute(SR.GetString("Axis_IsInverted_Property")),
				    new DisplayNameAttribute("IsInverted")				);


				tableBuilder.AddCustomAttributes(t, "LabelSettings",
					new DescriptionAttribute(SR.GetString("Axis_LabelSettings_Property")),
				    new DisplayNameAttribute("LabelSettings")				);


				tableBuilder.AddCustomAttributes(t, "CrossingAxis",
					new DescriptionAttribute(SR.GetString("Axis_CrossingAxis_Property")),
				    new DisplayNameAttribute("CrossingAxis")				);


				tableBuilder.AddCustomAttributes(t, "CrossingValue",
					new DescriptionAttribute(SR.GetString("Axis_CrossingValue_Property")),
				    new DisplayNameAttribute("CrossingValue"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "Label",
					new DescriptionAttribute(SR.GetString("Axis_Label_Property")),
				    new DisplayNameAttribute("Label"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "LabelPanelStyle",
					new DescriptionAttribute(SR.GetString("Axis_LabelPanelStyle_Property")),
				    new DisplayNameAttribute("LabelPanelStyle")				);


				tableBuilder.AddCustomAttributes(t, "SeriesViewer",
					new DescriptionAttribute(SR.GetString("Axis_SeriesViewer_Property")),
				    new DisplayNameAttribute("SeriesViewer")				);


				tableBuilder.AddCustomAttributes(t, "IsDisabled",
					new DescriptionAttribute(SR.GetString("Axis_IsDisabled_Property")),
				    new DisplayNameAttribute("IsDisabled")				);

				#endregion // Axis Properties

				#region TRIXIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.TRIXIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TRIXIndicatorStrategy Properties

				#region SlowStochasticOscillatorIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.SlowStochasticOscillatorIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SlowStochasticOscillatorIndicatorStrategy Properties

				#region NegativeVolumeIndexIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.NegativeVolumeIndexIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NegativeVolumeIndexIndicatorStrategy Properties

				#region MedianPriceIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.MedianPriceIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MedianPriceIndicatorStrategy Properties

				#region ItemwiseIndicatorCalculationStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ItemwiseIndicatorCalculationStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ItemwiseIndicatorCalculationStrategy Properties

				#region ForceIndexIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.ForceIndexIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ForceIndexIndicatorStrategy Properties

				#region CategoryDateTimeXAxis Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CategoryDateTimeXAxis");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayType",
					new DescriptionAttribute(SR.GetString("CategoryDateTimeXAxis_DisplayType_Property")),
				    new DisplayNameAttribute("DisplayType")				);


				tableBuilder.AddCustomAttributes(t, "DateTimeMemberPath",
					new DescriptionAttribute(SR.GetString("CategoryDateTimeXAxis_DateTimeMemberPath_Property")),
				    new DisplayNameAttribute("DateTimeMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "MinimumValue",
					new DescriptionAttribute(SR.GetString("CategoryDateTimeXAxis_MinimumValue_Property")),
				    new DisplayNameAttribute("MinimumValue")				);


				tableBuilder.AddCustomAttributes(t, "MaximumValue",
					new DescriptionAttribute(SR.GetString("CategoryDateTimeXAxis_MaximumValue_Property")),
				    new DisplayNameAttribute("MaximumValue")				);


				tableBuilder.AddCustomAttributes(t, "Interval",
					new DescriptionAttribute(SR.GetString("CategoryDateTimeXAxis_Interval_Property")),
				    new DisplayNameAttribute("Interval")				);


				tableBuilder.AddCustomAttributes(t, "ActualMinimumValue",
					new DescriptionAttribute(SR.GetString("CategoryDateTimeXAxis_ActualMinimumValue_Property")),
				    new DisplayNameAttribute("ActualMinimumValue")				);


				tableBuilder.AddCustomAttributes(t, "ActualMaximumValue",
					new DescriptionAttribute(SR.GetString("CategoryDateTimeXAxis_ActualMaximumValue_Property")),
				    new DisplayNameAttribute("ActualMaximumValue")				);

				#endregion // CategoryDateTimeXAxis Properties

				#region CategoryAxisBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CategoryAxisBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("CategoryAxisBase_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource")				);


				tableBuilder.AddCustomAttributes(t, "ItemsCount",
					new DescriptionAttribute(SR.GetString("CategoryAxisBase_ItemsCount_Property")),
				    new DisplayNameAttribute("ItemsCount")				);


				tableBuilder.AddCustomAttributes(t, "Gap",
					new DescriptionAttribute(SR.GetString("CategoryAxisBase_Gap_Property")),
				    new DisplayNameAttribute("Gap")				);


				tableBuilder.AddCustomAttributes(t, "Overlap",
					new DescriptionAttribute(SR.GetString("CategoryAxisBase_Overlap_Property")),
				    new DisplayNameAttribute("Overlap")				);

				#endregion // CategoryAxisBase Properties

				#region WeightedCloseIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.WeightedCloseIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // WeightedCloseIndicatorStrategy Properties

				#region RelativeStrengthIndexIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.RelativeStrengthIndexIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RelativeStrengthIndexIndicatorStrategy Properties

				#region RateOfChangeAndMomentumIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.RateOfChangeAndMomentumIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RateOfChangeAndMomentumIndicatorStrategy Properties

				#region PriceVolumeTrendIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.PriceVolumeTrendIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PriceVolumeTrendIndicatorStrategy Properties

				#region OnBalanceVolumeIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.OnBalanceVolumeIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // OnBalanceVolumeIndicatorStrategy Properties

				#region FastStochasticOscillatorIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.FastStochasticOscillatorIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FastStochasticOscillatorIndicatorStrategy Properties

				#region DetrendedPriceOscillatorIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.DetrendedPriceOscillatorIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DetrendedPriceOscillatorIndicatorStrategy Properties

				#region ChaikinVolatilityIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.ChaikinVolatilityIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ChaikinVolatilityIndicatorStrategy Properties

				#region Partitioner Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.Partitioner");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Partitioner Properties

				#region StandardDeviationIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.StandardDeviationIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StandardDeviationIndicatorStrategy Properties

				#region EaseOfMovementIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.EaseOfMovementIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // EaseOfMovementIndicatorStrategy Properties

				#region CategoryXAxis Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CategoryXAxis");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Interval",
					new DescriptionAttribute(SR.GetString("CategoryXAxis_Interval_Property")),
				    new DisplayNameAttribute("Interval")				);

				#endregion // CategoryXAxis Properties

				#region ChartMouseEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ChartMouseEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "StylusDevice",
					new DescriptionAttribute(SR.GetString("ChartMouseEventArgs_StylusDevice_Property")),
				    new DisplayNameAttribute("StylusDevice")				);


				tableBuilder.AddCustomAttributes(t, "OriginalSource",
					new DescriptionAttribute(SR.GetString("ChartMouseEventArgs_OriginalSource_Property")),
				    new DisplayNameAttribute("OriginalSource"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ChartMouseEventArgs_Item_Property")),
				    new DisplayNameAttribute("Item"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "Series",
					new DescriptionAttribute(SR.GetString("ChartMouseEventArgs_Series_Property")),
				    new DisplayNameAttribute("Series")				);


				tableBuilder.AddCustomAttributes(t, "Chart",
					new DescriptionAttribute(SR.GetString("ChartMouseEventArgs_Chart_Property")),
				    new DisplayNameAttribute("Chart")				);

				#endregion // ChartMouseEventArgs Properties

				#region DataChartMouseEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DataChartMouseEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DataChartMouseEventHandler Properties

				#region ChartLegendMouseEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ChartLegendMouseEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LegendItem",
					new DescriptionAttribute(SR.GetString("ChartLegendMouseEventArgs_LegendItem_Property")),
				    new DisplayNameAttribute("LegendItem"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);

				#endregion // ChartLegendMouseEventArgs Properties

				#region DataChartLegendMouseEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DataChartLegendMouseEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DataChartLegendMouseEventHandler Properties

				#region PercentageVolumeOscillatorIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.PercentageVolumeOscillatorIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PercentageVolumeOscillatorIndicatorStrategy Properties

				#region DataChartMouseButtonEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DataChartMouseButtonEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Handled",
					new DescriptionAttribute(SR.GetString("DataChartMouseButtonEventArgs_Handled_Property")),
				    new DisplayNameAttribute("Handled")				);


				tableBuilder.AddCustomAttributes(t, "StylusDevice",
					new DescriptionAttribute(SR.GetString("DataChartMouseButtonEventArgs_StylusDevice_Property")),
				    new DisplayNameAttribute("StylusDevice")				);


				tableBuilder.AddCustomAttributes(t, "OriginalSource",
					new DescriptionAttribute(SR.GetString("DataChartMouseButtonEventArgs_OriginalSource_Property")),
				    new DisplayNameAttribute("OriginalSource"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("DataChartMouseButtonEventArgs_Item_Property")),
				    new DisplayNameAttribute("Item"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "Series",
					new DescriptionAttribute(SR.GetString("DataChartMouseButtonEventArgs_Series_Property")),
				    new DisplayNameAttribute("Series")				);


				tableBuilder.AddCustomAttributes(t, "Chart",
					new DescriptionAttribute(SR.GetString("DataChartMouseButtonEventArgs_Chart_Property")),
				    new DisplayNameAttribute("Chart")				);

				#endregion // DataChartMouseButtonEventArgs Properties

				#region DataChartMouseButtonEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DataChartMouseButtonEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DataChartMouseButtonEventHandler Properties

				#region DataChartLegendMouseButtonEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DataChartLegendMouseButtonEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LegendItem",
					new DescriptionAttribute(SR.GetString("DataChartLegendMouseButtonEventArgs_LegendItem_Property")),
				    new DisplayNameAttribute("LegendItem"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);

				#endregion // DataChartLegendMouseButtonEventArgs Properties

				#region DataChartLegendMouseButtonEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DataChartLegendMouseButtonEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DataChartLegendMouseButtonEventHandler Properties

				#region UltimateOscillatorIndicatorCalculationStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.UltimateOscillatorIndicatorCalculationStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // UltimateOscillatorIndicatorCalculationStrategy Properties

				#region MovingAverageConvergenceDivergenceIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.MovingAverageConvergenceDivergenceIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MovingAverageConvergenceDivergenceIndicatorStrategy Properties

				#region FinancialCalculationDataSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FinancialCalculationDataSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OpenColumn",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_OpenColumn_Property")),
				    new DisplayNameAttribute("OpenColumn")				);


				tableBuilder.AddCustomAttributes(t, "CloseColumn",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_CloseColumn_Property")),
				    new DisplayNameAttribute("CloseColumn")				);


				tableBuilder.AddCustomAttributes(t, "HighColumn",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_HighColumn_Property")),
				    new DisplayNameAttribute("HighColumn")				);


				tableBuilder.AddCustomAttributes(t, "LowColumn",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_LowColumn_Property")),
				    new DisplayNameAttribute("LowColumn")				);


				tableBuilder.AddCustomAttributes(t, "VolumeColumn",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_VolumeColumn_Property")),
				    new DisplayNameAttribute("VolumeColumn")				);


				tableBuilder.AddCustomAttributes(t, "IndicatorColumn",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_IndicatorColumn_Property")),
				    new DisplayNameAttribute("IndicatorColumn")				);


				tableBuilder.AddCustomAttributes(t, "TypicalColumn",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_TypicalColumn_Property")),
				    new DisplayNameAttribute("TypicalColumn")				);


				tableBuilder.AddCustomAttributes(t, "TrueRange",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_TrueRange_Property")),
				    new DisplayNameAttribute("TrueRange")				);


				tableBuilder.AddCustomAttributes(t, "TrueLow",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_TrueLow_Property")),
				    new DisplayNameAttribute("TrueLow")				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_Period_Property")),
				    new DisplayNameAttribute("Period")				);


				tableBuilder.AddCustomAttributes(t, "ShortPeriod",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_ShortPeriod_Property")),
				    new DisplayNameAttribute("ShortPeriod")				);


				tableBuilder.AddCustomAttributes(t, "LongPeriod",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_LongPeriod_Property")),
				    new DisplayNameAttribute("LongPeriod")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "CalculateFrom",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_CalculateFrom_Property")),
				    new DisplayNameAttribute("CalculateFrom")				);


				tableBuilder.AddCustomAttributes(t, "CalculateCount",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_CalculateCount_Property")),
				    new DisplayNameAttribute("CalculateCount")				);


				tableBuilder.AddCustomAttributes(t, "Multiplier",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_Multiplier_Property")),
				    new DisplayNameAttribute("Multiplier")				);


				tableBuilder.AddCustomAttributes(t, "MinimumValue",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_MinimumValue_Property")),
				    new DisplayNameAttribute("MinimumValue")				);


				tableBuilder.AddCustomAttributes(t, "MaximumValue",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_MaximumValue_Property")),
				    new DisplayNameAttribute("MaximumValue")				);


				tableBuilder.AddCustomAttributes(t, "SpecifiesRange",
					new DescriptionAttribute(SR.GetString("FinancialCalculationDataSource_SpecifiesRange_Property")),
				    new DisplayNameAttribute("SpecifiesRange")				);

				#endregion // FinancialCalculationDataSource Properties

				#region FinancialCalculationSupportingCalculations Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FinancialCalculationSupportingCalculations");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "EMA",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_EMA_Property")),
				    new DisplayNameAttribute("EMA")				);


				tableBuilder.AddCustomAttributes(t, "SMA",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_SMA_Property")),
				    new DisplayNameAttribute("SMA")				);


				tableBuilder.AddCustomAttributes(t, "STDEV",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_STDEV_Property")),
				    new DisplayNameAttribute("STDEV")				);


				tableBuilder.AddCustomAttributes(t, "MovingSum",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_MovingSum_Property")),
				    new DisplayNameAttribute("MovingSum")				);


				tableBuilder.AddCustomAttributes(t, "ShortVolumeOscillatorAverage",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_ShortVolumeOscillatorAverage_Property")),
				    new DisplayNameAttribute("ShortVolumeOscillatorAverage")				);


				tableBuilder.AddCustomAttributes(t, "LongVolumeOscillatorAverage",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_LongVolumeOscillatorAverage_Property")),
				    new DisplayNameAttribute("LongVolumeOscillatorAverage")				);


				tableBuilder.AddCustomAttributes(t, "ShortPriceOscillatorAverage",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_ShortPriceOscillatorAverage_Property")),
				    new DisplayNameAttribute("ShortPriceOscillatorAverage")				);


				tableBuilder.AddCustomAttributes(t, "LongPriceOscillatorAverage",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_LongPriceOscillatorAverage_Property")),
				    new DisplayNameAttribute("LongPriceOscillatorAverage")				);


				tableBuilder.AddCustomAttributes(t, "ToEnumerableRange",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_ToEnumerableRange_Property")),
				    new DisplayNameAttribute("ToEnumerableRange")				);


				tableBuilder.AddCustomAttributes(t, "ToEnumerable",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_ToEnumerable_Property")),
				    new DisplayNameAttribute("ToEnumerable")				);


				tableBuilder.AddCustomAttributes(t, "MakeSafe",
					new DescriptionAttribute(SR.GetString("FinancialCalculationSupportingCalculations_MakeSafe_Property")),
				    new DisplayNameAttribute("MakeSafe")				);

				#endregion // FinancialCalculationSupportingCalculations Properties

				#region SupportingCalculation`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SupportingCalculation`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Strategy",
					new DescriptionAttribute(SR.GetString("SupportingCalculation`1_Strategy_Property")),
				    new DisplayNameAttribute("Strategy")				);


				tableBuilder.AddCustomAttributes(t, "BasedOn",
					new DescriptionAttribute(SR.GetString("SupportingCalculation`1_BasedOn_Property")),
				    new DisplayNameAttribute("BasedOn")				);

				#endregion // SupportingCalculation`1 Properties

				#region ColumnSupportingCalculation Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ColumnSupportingCalculation");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Strategy",
					new DescriptionAttribute(SR.GetString("SupportingCalculation`1_Strategy_Property")),
				    new DisplayNameAttribute("Strategy")				);


				tableBuilder.AddCustomAttributes(t, "BasedOn",
					new DescriptionAttribute(SR.GetString("SupportingCalculation`1_BasedOn_Property")),
				    new DisplayNameAttribute("BasedOn")				);

				#endregion // ColumnSupportingCalculation Properties

				#region DataSourceSupportingCalculation Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DataSourceSupportingCalculation");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Strategy",
					new DescriptionAttribute(SR.GetString("SupportingCalculation`1_Strategy_Property")),
				    new DisplayNameAttribute("Strategy")				);


				tableBuilder.AddCustomAttributes(t, "BasedOn",
					new DescriptionAttribute(SR.GetString("SupportingCalculation`1_BasedOn_Property")),
				    new DisplayNameAttribute("BasedOn")				);

				#endregion // DataSourceSupportingCalculation Properties

				#region CalculatedColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculatedColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "BasedOn",
					new DescriptionAttribute(SR.GetString("CalculatedColumn_BasedOn_Property")),
				    new DisplayNameAttribute("BasedOn")				);

				#endregion // CalculatedColumn Properties

				#region FinancialEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FinancialEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Position",
					new DescriptionAttribute(SR.GetString("FinancialEventArgs_Position_Property")),
				    new DisplayNameAttribute("Position")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("FinancialEventArgs_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "DataSource",
					new DescriptionAttribute(SR.GetString("FinancialEventArgs_DataSource_Property")),
				    new DisplayNameAttribute("DataSource")				);


				tableBuilder.AddCustomAttributes(t, "SupportingCalculations",
					new DescriptionAttribute(SR.GetString("FinancialEventArgs_SupportingCalculations_Property")),
				    new DisplayNameAttribute("SupportingCalculations")				);


				tableBuilder.AddCustomAttributes(t, "BasedOn",
					new DescriptionAttribute(SR.GetString("FinancialEventArgs_BasedOn_Property")),
				    new DisplayNameAttribute("BasedOn")				);

				#endregion // FinancialEventArgs Properties

				#region FinancialEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FinancialEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FinancialEventHandler Properties

				#region SyncSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SyncSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SyncChannel",
					new DescriptionAttribute(SR.GetString("SyncSettings_SyncChannel_Property")),
				    new DisplayNameAttribute("SyncChannel")				);


				tableBuilder.AddCustomAttributes(t, "SynchronizeVertically",
					new DescriptionAttribute(SR.GetString("SyncSettings_SynchronizeVertically_Property")),
				    new DisplayNameAttribute("SynchronizeVertically")				);


				tableBuilder.AddCustomAttributes(t, "SynchronizeHorizontally",
					new DescriptionAttribute(SR.GetString("SyncSettings_SynchronizeHorizontally_Property")),
				    new DisplayNameAttribute("SynchronizeHorizontally")				);

				#endregion // SyncSettings Properties

				#region MoneyFlowIndexIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.MoneyFlowIndexIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MoneyFlowIndexIndicatorStrategy Properties

				#region CommodityChannelIndexIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.CommodityChannelIndexIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CommodityChannelIndexIndicatorStrategy Properties

				#region ChaikinOscillatorIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.ChaikinOscillatorIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ChaikinOscillatorIndicatorStrategy Properties

				#region AbsoluteVolumeOscillatorIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.AbsoluteVolumeOscillatorIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AbsoluteVolumeOscillatorIndicatorStrategy Properties

				#region PropertyUpdatedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PropertyUpdatedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PropertyName",
					new DescriptionAttribute(SR.GetString("PropertyUpdatedEventArgs_PropertyName_Property")),
				    new DisplayNameAttribute("PropertyName")				);


				tableBuilder.AddCustomAttributes(t, "OldValue",
					new DescriptionAttribute(SR.GetString("PropertyUpdatedEventArgs_OldValue_Property")),
				    new DisplayNameAttribute("OldValue"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "NewValue",
					new DescriptionAttribute(SR.GetString("PropertyUpdatedEventArgs_NewValue_Property")),
				    new DisplayNameAttribute("NewValue"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);

				#endregion // PropertyUpdatedEventArgs Properties

				#region PropertyUpdatedEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PropertyUpdatedEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PropertyUpdatedEventHandler Properties

				#region AxisRangeChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AxisRangeChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OldMinimumValue",
					new DescriptionAttribute(SR.GetString("AxisRangeChangedEventArgs_OldMinimumValue_Property")),
				    new DisplayNameAttribute("OldMinimumValue")				);


				tableBuilder.AddCustomAttributes(t, "MinimumValue",
					new DescriptionAttribute(SR.GetString("AxisRangeChangedEventArgs_MinimumValue_Property")),
				    new DisplayNameAttribute("MinimumValue")				);


				tableBuilder.AddCustomAttributes(t, "OldMaximumValue",
					new DescriptionAttribute(SR.GetString("AxisRangeChangedEventArgs_OldMaximumValue_Property")),
				    new DisplayNameAttribute("OldMaximumValue")				);


				tableBuilder.AddCustomAttributes(t, "MaximumValue",
					new DescriptionAttribute(SR.GetString("AxisRangeChangedEventArgs_MaximumValue_Property")),
				    new DisplayNameAttribute("MaximumValue")				);

				#endregion // AxisRangeChangedEventArgs Properties

				#region AxisRangeChangedEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AxisRangeChangedEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AxisRangeChangedEventHandler Properties

				#region AxisLabelSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AxisLabelSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Effect",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_Effect_Property")),
				    new DisplayNameAttribute("Effect")				);


				tableBuilder.AddCustomAttributes(t, "Foreground",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_Foreground_Property")),
				    new DisplayNameAttribute("Foreground")				);


				tableBuilder.AddCustomAttributes(t, "FontFamily",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_FontFamily_Property")),
				    new DisplayNameAttribute("FontFamily")				);


				tableBuilder.AddCustomAttributes(t, "FontSize",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_FontSize_Property")),
				    new DisplayNameAttribute("FontSize")				);


				tableBuilder.AddCustomAttributes(t, "FontStretch",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_FontStretch_Property")),
				    new DisplayNameAttribute("FontStretch")				);


				tableBuilder.AddCustomAttributes(t, "FontStyle",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_FontStyle_Property")),
				    new DisplayNameAttribute("FontStyle")				);


				tableBuilder.AddCustomAttributes(t, "FontWeight",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_FontWeight_Property")),
				    new DisplayNameAttribute("FontWeight")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalAlignment",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_HorizontalAlignment_Property")),
				    new DisplayNameAttribute("HorizontalAlignment")				);


				tableBuilder.AddCustomAttributes(t, "VerticalAlignment",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_VerticalAlignment_Property")),
				    new DisplayNameAttribute("VerticalAlignment")				);


				tableBuilder.AddCustomAttributes(t, "IsHitTestVisible",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_IsHitTestVisible_Property")),
				    new DisplayNameAttribute("IsHitTestVisible")				);


				tableBuilder.AddCustomAttributes(t, "OpacityMask",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_OpacityMask_Property")),
				    new DisplayNameAttribute("OpacityMask")				);


				tableBuilder.AddCustomAttributes(t, "Opacity",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_Opacity_Property")),
				    new DisplayNameAttribute("Opacity")				);


				tableBuilder.AddCustomAttributes(t, "Padding",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_Padding_Property")),
				    new DisplayNameAttribute("Padding")				);


				tableBuilder.AddCustomAttributes(t, "TextDecorations",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_TextDecorations_Property")),
				    new DisplayNameAttribute("TextDecorations")				);


				tableBuilder.AddCustomAttributes(t, "TextWrapping",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_TextWrapping_Property")),
				    new DisplayNameAttribute("TextWrapping")				);


				tableBuilder.AddCustomAttributes(t, "Visibility",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_Visibility_Property")),
				    new DisplayNameAttribute("Visibility")				);


				tableBuilder.AddCustomAttributes(t, "Extent",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_Extent_Property")),
				    new DisplayNameAttribute("Extent")				);


				tableBuilder.AddCustomAttributes(t, "Angle",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_Angle_Property")),
				    new DisplayNameAttribute("Angle")				);


				tableBuilder.AddCustomAttributes(t, "Location",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_Location_Property")),
				    new DisplayNameAttribute("Location")				);


				tableBuilder.AddCustomAttributes(t, "TextAlignment",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_TextAlignment_Property")),
				    new DisplayNameAttribute("TextAlignment")				);


				tableBuilder.AddCustomAttributes(t, "Axis",
					new DescriptionAttribute(SR.GetString("AxisLabelSettings_Axis_Property")),
				    new DisplayNameAttribute("Axis")				);

				#endregion // AxisLabelSettings Properties

				#region DataContext Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DataContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Series",
					new DescriptionAttribute(SR.GetString("DataContext_Series_Property")),
				    new DisplayNameAttribute("Series")				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("DataContext_Item_Property")),
				    new DisplayNameAttribute("Item"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "ItemBrush",
					new DescriptionAttribute(SR.GetString("DataContext_ItemBrush_Property")),
				    new DisplayNameAttribute("ItemBrush")				);


				tableBuilder.AddCustomAttributes(t, "ActualItemBrush",
					new DescriptionAttribute(SR.GetString("DataContext_ActualItemBrush_Property")),
				    new DisplayNameAttribute("ActualItemBrush")				);


				tableBuilder.AddCustomAttributes(t, "ItemLabel",
					new DescriptionAttribute(SR.GetString("DataContext_ItemLabel_Property")),
				    new DisplayNameAttribute("ItemLabel")				);

				#endregion // DataContext Properties

				#region AxisItemDataContext Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AxisItemDataContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("AxisItemDataContext_Item_Property")),
				    new DisplayNameAttribute("Item"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "Axis",
					new DescriptionAttribute(SR.GetString("AxisItemDataContext_Axis_Property")),
				    new DisplayNameAttribute("Axis")				);

				#endregion // AxisItemDataContext Properties

				#region Pool`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.Pool`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Create",
					new DescriptionAttribute(SR.GetString("Pool`1_Create_Property")),
				    new DisplayNameAttribute("Create")				);


				tableBuilder.AddCustomAttributes(t, "Disactivate",
					new DescriptionAttribute(SR.GetString("Pool`1_Disactivate_Property")),
				    new DisplayNameAttribute("Disactivate")				);


				tableBuilder.AddCustomAttributes(t, "Activate",
					new DescriptionAttribute(SR.GetString("Pool`1_Activate_Property")),
				    new DisplayNameAttribute("Activate")				);


				tableBuilder.AddCustomAttributes(t, "Destroy",
					new DescriptionAttribute(SR.GetString("Pool`1_Destroy_Property")),
				    new DisplayNameAttribute("Destroy")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("Pool`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "Active",
					new DescriptionAttribute(SR.GetString("Pool`1_Active_Property")),
				    new DisplayNameAttribute("Active")				);


				tableBuilder.AddCustomAttributes(t, "Inactive",
					new DescriptionAttribute(SR.GetString("Pool`1_Inactive_Property")),
				    new DisplayNameAttribute("Inactive")				);

				#endregion // Pool`1 Properties

				#region HashPool`2 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.HashPool`2");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Create",
					new DescriptionAttribute(SR.GetString("HashPool`2_Create_Property")),
				    new DisplayNameAttribute("Create")				);


				tableBuilder.AddCustomAttributes(t, "Disactivate",
					new DescriptionAttribute(SR.GetString("HashPool`2_Disactivate_Property")),
				    new DisplayNameAttribute("Disactivate")				);


				tableBuilder.AddCustomAttributes(t, "Activate",
					new DescriptionAttribute(SR.GetString("HashPool`2_Activate_Property")),
				    new DisplayNameAttribute("Activate")				);


				tableBuilder.AddCustomAttributes(t, "Destroy",
					new DescriptionAttribute(SR.GetString("HashPool`2_Destroy_Property")),
				    new DisplayNameAttribute("Destroy")				);


				tableBuilder.AddCustomAttributes(t, "ActiveKeys",
					new DescriptionAttribute(SR.GetString("HashPool`2_ActiveKeys_Property")),
				    new DisplayNameAttribute("ActiveKeys")				);


				tableBuilder.AddCustomAttributes(t, "ActiveCount",
					new DescriptionAttribute(SR.GetString("HashPool`2_ActiveCount_Property")),
				    new DisplayNameAttribute("ActiveCount")				);

				#endregion // HashPool`2 Properties

				#region BollingerBandWidthIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.BollingerBandWidthIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // BollingerBandWidthIndicatorStrategy Properties

				#region Legend Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Legend");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDataChartSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDataChartSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentPresenter",
					new DescriptionAttribute(SR.GetString("Legend_ContentPresenter_Property")),
				    new DisplayNameAttribute("ContentPresenter")				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("Legend_Orientation_Property")),
				    new DisplayNameAttribute("Orientation")				);


				tableBuilder.AddCustomAttributes(t, "Children",
					new DescriptionAttribute(SR.GetString("Legend_Children_Property")),
				    new DisplayNameAttribute("Children")				);

				#endregion // Legend Properties

				#region ItemwiseStrategyCalculationStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ItemwiseStrategyCalculationStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemwiseStrategy",
					new DescriptionAttribute(SR.GetString("ItemwiseStrategyCalculationStrategy_ItemwiseStrategy_Property")),
				    new DisplayNameAttribute("ItemwiseStrategy")				);

				#endregion // ItemwiseStrategyCalculationStrategy Properties

				#region FullStochasticOscillatorIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.FullStochasticOscillatorIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FullStochasticOscillatorIndicatorStrategy Properties

				#region PercentKCalculationStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.PercentKCalculationStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PercentKCalculationStrategy Properties

				#region AverageDirectionalIndexIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.AverageDirectionalIndexIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AverageDirectionalIndexIndicatorStrategy Properties

				#region NumericXAxis Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.NumericXAxis");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NumericXAxis Properties

				#region TypicalPriceIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.TypicalPriceIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TypicalPriceIndicatorStrategy Properties

				#region Marker Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Marker");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Marker Properties

				#region AxisRange Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AxisRange");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Minimum",
					new DescriptionAttribute(SR.GetString("AxisRange_Minimum_Property")),
				    new DisplayNameAttribute("Minimum")				);


				tableBuilder.AddCustomAttributes(t, "Maximum",
					new DescriptionAttribute(SR.GetString("AxisRange_Maximum_Property")),
				    new DisplayNameAttribute("Maximum")				);

				#endregion // AxisRange Properties

				#region WilliamsPercentRIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.WilliamsPercentRIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // WilliamsPercentRIndicatorStrategy Properties

				#region MarketFacilitationIndexIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.MarketFacilitationIndexIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MarketFacilitationIndexIndicatorStrategy Properties

				#region AccumulationDistributionIndicatorStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CalculationStrategies.AccumulationDistributionIndicatorStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AccumulationDistributionIndicatorStrategy Properties

				#region ChartCursorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ChartCursorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ChartCursorEventArgs_Item_Property")),
				    new DisplayNameAttribute("Item"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "Series",
					new DescriptionAttribute(SR.GetString("ChartCursorEventArgs_Series_Property")),
				    new DisplayNameAttribute("Series")				);


				tableBuilder.AddCustomAttributes(t, "Chart",
					new DescriptionAttribute(SR.GetString("ChartCursorEventArgs_Chart_Property")),
				    new DisplayNameAttribute("Chart")				);


				tableBuilder.AddCustomAttributes(t, "SeriesViewer",
					new DescriptionAttribute(SR.GetString("ChartCursorEventArgs_SeriesViewer_Property")),
				    new DisplayNameAttribute("SeriesViewer")				);

				#endregion // ChartCursorEventArgs Properties

				#region DataChartCursorEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DataChartCursorEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DataChartCursorEventHandler Properties

				#region ScatterSplineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ScatterSplineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Stiffness",
					new DescriptionAttribute(SR.GetString("ScatterSplineSeries_Stiffness_Property")),
				    new DisplayNameAttribute("Stiffness")				);

				#endregion // ScatterSplineSeries Properties

				#region ScatterBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ScatterBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "XAxis",
					new DescriptionAttribute(SR.GetString("ScatterBase_XAxis_Property")),
				    new DisplayNameAttribute("XAxis")				);


				tableBuilder.AddCustomAttributes(t, "YAxis",
					new DescriptionAttribute(SR.GetString("ScatterBase_YAxis_Property")),
				    new DisplayNameAttribute("YAxis")				);


				tableBuilder.AddCustomAttributes(t, "XMemberPath",
					new DescriptionAttribute(SR.GetString("ScatterBase_XMemberPath_Property")),
				    new DisplayNameAttribute("XMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "YMemberPath",
					new DescriptionAttribute(SR.GetString("ScatterBase_YMemberPath_Property")),
				    new DisplayNameAttribute("YMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineType",
					new DescriptionAttribute(SR.GetString("ScatterBase_TrendLineType_Property")),
				    new DisplayNameAttribute("TrendLineType")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineBrush",
					new DescriptionAttribute(SR.GetString("ScatterBase_TrendLineBrush_Property")),
				    new DisplayNameAttribute("TrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "ActualTrendLineBrush",
					new DescriptionAttribute(SR.GetString("ScatterBase_ActualTrendLineBrush_Property")),
				    new DisplayNameAttribute("ActualTrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineThickness",
					new DescriptionAttribute(SR.GetString("ScatterBase_TrendLineThickness_Property")),
				    new DisplayNameAttribute("TrendLineThickness")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashCap",
					new DescriptionAttribute(SR.GetString("ScatterBase_TrendLineDashCap_Property")),
				    new DisplayNameAttribute("TrendLineDashCap")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashArray",
					new DescriptionAttribute(SR.GetString("ScatterBase_TrendLineDashArray_Property")),
				    new DisplayNameAttribute("TrendLineDashArray")				);


				tableBuilder.AddCustomAttributes(t, "TrendLinePeriod",
					new DescriptionAttribute(SR.GetString("ScatterBase_TrendLinePeriod_Property")),
				    new DisplayNameAttribute("TrendLinePeriod")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineZIndex",
					new DescriptionAttribute(SR.GetString("ScatterBase_TrendLineZIndex_Property")),
				    new DisplayNameAttribute("TrendLineZIndex")				);


				tableBuilder.AddCustomAttributes(t, "MaximumMarkers",
					new DescriptionAttribute(SR.GetString("ScatterBase_MaximumMarkers_Property")),
				    new DisplayNameAttribute("MaximumMarkers")				);


				tableBuilder.AddCustomAttributes(t, "ErrorBarSettings",
					new DescriptionAttribute(SR.GetString("ScatterBase_ErrorBarSettings_Property")),
				    new DisplayNameAttribute("ErrorBarSettings")				);


				tableBuilder.AddCustomAttributes(t, "MarkerCollisionAvoidance",
					new DescriptionAttribute(SR.GetString("ScatterBase_MarkerCollisionAvoidance_Property")),
				    new DisplayNameAttribute("MarkerCollisionAvoidance")				);

				#endregion // ScatterBase Properties

				#region MarkerSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.MarkerSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MarkerType",
					new DescriptionAttribute(SR.GetString("MarkerSeries_MarkerType_Property")),
				    new DisplayNameAttribute("MarkerType")				);


				tableBuilder.AddCustomAttributes(t, "MarkerTemplate",
					new DescriptionAttribute(SR.GetString("MarkerSeries_MarkerTemplate_Property")),
				    new DisplayNameAttribute("MarkerTemplate")				);


				tableBuilder.AddCustomAttributes(t, "ActualMarkerTemplate",
					new DescriptionAttribute(SR.GetString("MarkerSeries_ActualMarkerTemplate_Property")),
				    new DisplayNameAttribute("ActualMarkerTemplate")				);


				tableBuilder.AddCustomAttributes(t, "MarkerBrush",
					new DescriptionAttribute(SR.GetString("MarkerSeries_MarkerBrush_Property")),
				    new DisplayNameAttribute("MarkerBrush")				);


				tableBuilder.AddCustomAttributes(t, "ActualMarkerBrush",
					new DescriptionAttribute(SR.GetString("MarkerSeries_ActualMarkerBrush_Property")),
				    new DisplayNameAttribute("ActualMarkerBrush")				);


				tableBuilder.AddCustomAttributes(t, "MarkerOutline",
					new DescriptionAttribute(SR.GetString("MarkerSeries_MarkerOutline_Property")),
				    new DisplayNameAttribute("MarkerOutline")				);


				tableBuilder.AddCustomAttributes(t, "ActualMarkerOutline",
					new DescriptionAttribute(SR.GetString("MarkerSeries_ActualMarkerOutline_Property")),
				    new DisplayNameAttribute("ActualMarkerOutline")				);


				tableBuilder.AddCustomAttributes(t, "MarkerStyle",
					new DescriptionAttribute(SR.GetString("MarkerSeries_MarkerStyle_Property")),
				    new DisplayNameAttribute("MarkerStyle")				);


				tableBuilder.AddCustomAttributes(t, "UseLightweightMarkers",
					new DescriptionAttribute(SR.GetString("MarkerSeries_UseLightweightMarkers_Property")),
				    new DisplayNameAttribute("UseLightweightMarkers")				);

				#endregion // MarkerSeries Properties

				#region Series Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Series");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RootCanvas",
					new DescriptionAttribute(SR.GetString("Series_RootCanvas_Property")),
				    new DisplayNameAttribute("RootCanvas")				);


				tableBuilder.AddCustomAttributes(t, "Chart",
					new DescriptionAttribute(SR.GetString("Series_Chart_Property")),
				    new DisplayNameAttribute("Chart"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("Series_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource")				);


				tableBuilder.AddCustomAttributes(t, "Legend",
					new DescriptionAttribute(SR.GetString("Series_Legend_Property")),
				    new DisplayNameAttribute("Legend")				);


				tableBuilder.AddCustomAttributes(t, "LegendItem",
					new DescriptionAttribute(SR.GetString("Series_LegendItem_Property")),
				    new DisplayNameAttribute("LegendItem")				);


				tableBuilder.AddCustomAttributes(t, "ActualLegend",
					new DescriptionAttribute(SR.GetString("Series_ActualLegend_Property")),
				    new DisplayNameAttribute("ActualLegend")				);


				tableBuilder.AddCustomAttributes(t, "LegendItemVisibility",
					new DescriptionAttribute(SR.GetString("Series_LegendItemVisibility_Property")),
				    new DisplayNameAttribute("LegendItemVisibility")				);


				tableBuilder.AddCustomAttributes(t, "LegendItemBadgeTemplate",
					new DescriptionAttribute(SR.GetString("Series_LegendItemBadgeTemplate_Property")),
				    new DisplayNameAttribute("LegendItemBadgeTemplate")				);


				tableBuilder.AddCustomAttributes(t, "LegendItemTemplate",
					new DescriptionAttribute(SR.GetString("Series_LegendItemTemplate_Property")),
				    new DisplayNameAttribute("LegendItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "Index",
					new DescriptionAttribute(SR.GetString("Series_Index_Property")),
				    new DisplayNameAttribute("Index")				);


				tableBuilder.AddCustomAttributes(t, "TransitionEasingFunction",
					new DescriptionAttribute(SR.GetString("Series_TransitionEasingFunction_Property")),
				    new DisplayNameAttribute("TransitionEasingFunction")				);


				tableBuilder.AddCustomAttributes(t, "TransitionDuration",
					new DescriptionAttribute(SR.GetString("Series_TransitionDuration_Property")),
				    new DisplayNameAttribute("TransitionDuration")				);


				tableBuilder.AddCustomAttributes(t, "Resolution",
					new DescriptionAttribute(SR.GetString("Series_Resolution_Property")),
				    new DisplayNameAttribute("Resolution")				);


				tableBuilder.AddCustomAttributes(t, "Title",
					new DescriptionAttribute(SR.GetString("Series_Title_Property")),
				    new DisplayNameAttribute("Title"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "Brush",
					new DescriptionAttribute(SR.GetString("Series_Brush_Property")),
				    new DisplayNameAttribute("Brush")				);


				tableBuilder.AddCustomAttributes(t, "ActualBrush",
					new DescriptionAttribute(SR.GetString("Series_ActualBrush_Property")),
				    new DisplayNameAttribute("ActualBrush")				);


				tableBuilder.AddCustomAttributes(t, "Outline",
					new DescriptionAttribute(SR.GetString("Series_Outline_Property")),
				    new DisplayNameAttribute("Outline")				);


				tableBuilder.AddCustomAttributes(t, "ActualOutline",
					new DescriptionAttribute(SR.GetString("Series_ActualOutline_Property")),
				    new DisplayNameAttribute("ActualOutline")				);


				tableBuilder.AddCustomAttributes(t, "LineJoin",
					new DescriptionAttribute(SR.GetString("Series_LineJoin_Property")),
				    new DisplayNameAttribute("LineJoin")				);


				tableBuilder.AddCustomAttributes(t, "MiterLimit",
					new DescriptionAttribute(SR.GetString("Series_MiterLimit_Property")),
				    new DisplayNameAttribute("MiterLimit")				);


				tableBuilder.AddCustomAttributes(t, "Thickness",
					new DescriptionAttribute(SR.GetString("Series_Thickness_Property")),
				    new DisplayNameAttribute("Thickness")				);


				tableBuilder.AddCustomAttributes(t, "DashCap",
					new DescriptionAttribute(SR.GetString("Series_DashCap_Property")),
				    new DisplayNameAttribute("DashCap")				);


				tableBuilder.AddCustomAttributes(t, "DashArray",
					new DescriptionAttribute(SR.GetString("Series_DashArray_Property")),
				    new DisplayNameAttribute("DashArray")				);


				tableBuilder.AddCustomAttributes(t, "ToolTip",
					new DescriptionAttribute(SR.GetString("Series_ToolTip_Property")),
				    new DisplayNameAttribute("ToolTip"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "StartCap",
					new DescriptionAttribute(SR.GetString("Series_StartCap_Property")),
				    new DisplayNameAttribute("StartCap")				);


				tableBuilder.AddCustomAttributes(t, "EndCap",
					new DescriptionAttribute(SR.GetString("Series_EndCap_Property")),
				    new DisplayNameAttribute("EndCap")				);


				tableBuilder.AddCustomAttributes(t, "DiscreteLegendItemTemplate",
					new DescriptionAttribute(SR.GetString("Series_DiscreteLegendItemTemplate_Property")),
				    new DisplayNameAttribute("DiscreteLegendItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "SeriesViewer",
					new DescriptionAttribute(SR.GetString("Series_SeriesViewer_Property")),
				    new DisplayNameAttribute("SeriesViewer")				);


				tableBuilder.AddCustomAttributes(t, "View",
					new DescriptionAttribute(SR.GetString("Series_View_Property")),
				    new DisplayNameAttribute("View")				);


				tableBuilder.AddCustomAttributes(t, "SyncLink",
					new DescriptionAttribute(SR.GetString("Series_SyncLink_Property")),
				    new DisplayNameAttribute("SyncLink")				);


				tableBuilder.AddCustomAttributes(t, "ToolTipFormatter",
					new DescriptionAttribute(SR.GetString("Series_ToolTipFormatter_Property")),
				    new DisplayNameAttribute("ToolTipFormatter")				);

				#endregion // Series Properties

				#region StrategyBasedIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StrategyBasedIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StrategyBasedIndicator Properties

				#region FinancialIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FinancialIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayType",
					new DescriptionAttribute(SR.GetString("FinancialIndicator_DisplayType_Property")),
				    new DisplayNameAttribute("DisplayType")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineType",
					new DescriptionAttribute(SR.GetString("FinancialIndicator_TrendLineType_Property")),
				    new DisplayNameAttribute("TrendLineType")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineBrush",
					new DescriptionAttribute(SR.GetString("FinancialIndicator_TrendLineBrush_Property")),
				    new DisplayNameAttribute("TrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "ActualTrendLineBrush",
					new DescriptionAttribute(SR.GetString("FinancialIndicator_ActualTrendLineBrush_Property")),
				    new DisplayNameAttribute("ActualTrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineThickness",
					new DescriptionAttribute(SR.GetString("FinancialIndicator_TrendLineThickness_Property")),
				    new DisplayNameAttribute("TrendLineThickness")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashCap",
					new DescriptionAttribute(SR.GetString("FinancialIndicator_TrendLineDashCap_Property")),
				    new DisplayNameAttribute("TrendLineDashCap")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashArray",
					new DescriptionAttribute(SR.GetString("FinancialIndicator_TrendLineDashArray_Property")),
				    new DisplayNameAttribute("TrendLineDashArray")				);


				tableBuilder.AddCustomAttributes(t, "TrendLinePeriod",
					new DescriptionAttribute(SR.GetString("FinancialIndicator_TrendLinePeriod_Property")),
				    new DisplayNameAttribute("TrendLinePeriod")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineZIndex",
					new DescriptionAttribute(SR.GetString("FinancialIndicator_TrendLineZIndex_Property")),
				    new DisplayNameAttribute("TrendLineZIndex")				);


				tableBuilder.AddCustomAttributes(t, "IgnoreFirst",
					new DescriptionAttribute(SR.GetString("FinancialIndicator_IgnoreFirst_Property")),
				    new DisplayNameAttribute("IgnoreFirst")				);

				#endregion // FinancialIndicator Properties

				#region FinancialSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FinancialSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NegativeBrush",
					new DescriptionAttribute(SR.GetString("FinancialSeries_NegativeBrush_Property")),
				    new DisplayNameAttribute("NegativeBrush")				);


				tableBuilder.AddCustomAttributes(t, "XAxis",
					new DescriptionAttribute(SR.GetString("FinancialSeries_XAxis_Property")),
				    new DisplayNameAttribute("XAxis")				);


				tableBuilder.AddCustomAttributes(t, "YAxis",
					new DescriptionAttribute(SR.GetString("FinancialSeries_YAxis_Property")),
				    new DisplayNameAttribute("YAxis")				);


				tableBuilder.AddCustomAttributes(t, "OpenMemberPath",
					new DescriptionAttribute(SR.GetString("FinancialSeries_OpenMemberPath_Property")),
				    new DisplayNameAttribute("OpenMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "HighMemberPath",
					new DescriptionAttribute(SR.GetString("FinancialSeries_HighMemberPath_Property")),
				    new DisplayNameAttribute("HighMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "LowMemberPath",
					new DescriptionAttribute(SR.GetString("FinancialSeries_LowMemberPath_Property")),
				    new DisplayNameAttribute("LowMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "CloseMemberPath",
					new DescriptionAttribute(SR.GetString("FinancialSeries_CloseMemberPath_Property")),
				    new DisplayNameAttribute("CloseMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "VolumeMemberPath",
					new DescriptionAttribute(SR.GetString("FinancialSeries_VolumeMemberPath_Property")),
				    new DisplayNameAttribute("VolumeMemberPath")				);

				#endregion // FinancialSeries Properties

				#region StochRSIIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StochRSIIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("StochRSIIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // StochRSIIndicator Properties

				#region FinancialPriceSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FinancialPriceSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TrendLineType",
					new DescriptionAttribute(SR.GetString("FinancialPriceSeries_TrendLineType_Property")),
				    new DisplayNameAttribute("TrendLineType")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineBrush",
					new DescriptionAttribute(SR.GetString("FinancialPriceSeries_TrendLineBrush_Property")),
				    new DisplayNameAttribute("TrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "ActualTrendLineBrush",
					new DescriptionAttribute(SR.GetString("FinancialPriceSeries_ActualTrendLineBrush_Property")),
				    new DisplayNameAttribute("ActualTrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineThickness",
					new DescriptionAttribute(SR.GetString("FinancialPriceSeries_TrendLineThickness_Property")),
				    new DisplayNameAttribute("TrendLineThickness")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashCap",
					new DescriptionAttribute(SR.GetString("FinancialPriceSeries_TrendLineDashCap_Property")),
				    new DisplayNameAttribute("TrendLineDashCap")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashArray",
					new DescriptionAttribute(SR.GetString("FinancialPriceSeries_TrendLineDashArray_Property")),
				    new DisplayNameAttribute("TrendLineDashArray")				);


				tableBuilder.AddCustomAttributes(t, "TrendLinePeriod",
					new DescriptionAttribute(SR.GetString("FinancialPriceSeries_TrendLinePeriod_Property")),
				    new DisplayNameAttribute("TrendLinePeriod")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineZIndex",
					new DescriptionAttribute(SR.GetString("FinancialPriceSeries_TrendLineZIndex_Property")),
				    new DisplayNameAttribute("TrendLineZIndex")				);


				tableBuilder.AddCustomAttributes(t, "DisplayType",
					new DescriptionAttribute(SR.GetString("FinancialPriceSeries_DisplayType_Property")),
				    new DisplayNameAttribute("DisplayType")				);

				#endregion // FinancialPriceSeries Properties

				#region StepLineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StepLineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StepLineSeries Properties

				#region AnchoredCategorySeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AnchoredCategorySeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ValueMemberPath",
					new DescriptionAttribute(SR.GetString("AnchoredCategorySeries_ValueMemberPath_Property")),
				    new DisplayNameAttribute("ValueMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineType",
					new DescriptionAttribute(SR.GetString("AnchoredCategorySeries_TrendLineType_Property")),
				    new DisplayNameAttribute("TrendLineType")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineBrush",
					new DescriptionAttribute(SR.GetString("AnchoredCategorySeries_TrendLineBrush_Property")),
				    new DisplayNameAttribute("TrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "ActualTrendLineBrush",
					new DescriptionAttribute(SR.GetString("AnchoredCategorySeries_ActualTrendLineBrush_Property")),
				    new DisplayNameAttribute("ActualTrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineThickness",
					new DescriptionAttribute(SR.GetString("AnchoredCategorySeries_TrendLineThickness_Property")),
				    new DisplayNameAttribute("TrendLineThickness")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashCap",
					new DescriptionAttribute(SR.GetString("AnchoredCategorySeries_TrendLineDashCap_Property")),
				    new DisplayNameAttribute("TrendLineDashCap")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashArray",
					new DescriptionAttribute(SR.GetString("AnchoredCategorySeries_TrendLineDashArray_Property")),
				    new DisplayNameAttribute("TrendLineDashArray")				);


				tableBuilder.AddCustomAttributes(t, "TrendLinePeriod",
					new DescriptionAttribute(SR.GetString("AnchoredCategorySeries_TrendLinePeriod_Property")),
				    new DisplayNameAttribute("TrendLinePeriod")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineZIndex",
					new DescriptionAttribute(SR.GetString("AnchoredCategorySeries_TrendLineZIndex_Property")),
				    new DisplayNameAttribute("TrendLineZIndex")				);

				#endregion // AnchoredCategorySeries Properties

				#region CategorySeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CategorySeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ErrorBarSettings",
					new DescriptionAttribute(SR.GetString("CategorySeries_ErrorBarSettings_Property")),
				    new DisplayNameAttribute("ErrorBarSettings")				);

				#endregion // CategorySeries Properties

				#region ColumnSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ColumnSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("ColumnSeries_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("ColumnSeries_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);

				#endregion // ColumnSeries Properties

				#region MassIndexIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.MassIndexIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MassIndexIndicator Properties

				#region FinancialOverlay Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FinancialOverlay");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IgnoreFirst",
					new DescriptionAttribute(SR.GetString("FinancialOverlay_IgnoreFirst_Property")),
				    new DisplayNameAttribute("IgnoreFirst")				);

				#endregion // FinancialOverlay Properties

				#region AverageTrueRangeIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AverageTrueRangeIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("AverageTrueRangeIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // AverageTrueRangeIndicator Properties

				#region AreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "UnknownValuePlotting",
					new DescriptionAttribute(SR.GetString("AreaSeries_UnknownValuePlotting_Property")),
				    new DisplayNameAttribute("UnknownValuePlotting")				);

				#endregion // AreaSeries Properties

				#region ScatterLineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ScatterLineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "UnknownValuePlotting",
					new DescriptionAttribute(SR.GetString("ScatterLineSeries_UnknownValuePlotting_Property")),
				    new DisplayNameAttribute("UnknownValuePlotting")				);

				#endregion // ScatterLineSeries Properties

				#region PositiveVolumeIndexIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PositiveVolumeIndexIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PositiveVolumeIndexIndicator Properties

				#region PercentagePriceOscillatorIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PercentagePriceOscillatorIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ShortPeriod",
					new DescriptionAttribute(SR.GetString("PercentagePriceOscillatorIndicator_ShortPeriod_Property")),
				    new DisplayNameAttribute("ShortPeriod")				);


				tableBuilder.AddCustomAttributes(t, "LongPeriod",
					new DescriptionAttribute(SR.GetString("PercentagePriceOscillatorIndicator_LongPeriod_Property")),
				    new DisplayNameAttribute("LongPeriod")				);

				#endregion // PercentagePriceOscillatorIndicator Properties

				#region CustomIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CustomIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CustomIndicator Properties

				#region RangeColumnSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RangeColumnSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("RangeColumnSeries_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("RangeColumnSeries_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);

				#endregion // RangeColumnSeries Properties

				#region RangeCategorySeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RangeCategorySeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LowMemberPath",
					new DescriptionAttribute(SR.GetString("RangeCategorySeries_LowMemberPath_Property")),
				    new DisplayNameAttribute("LowMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "HighMemberPath",
					new DescriptionAttribute(SR.GetString("RangeCategorySeries_HighMemberPath_Property")),
				    new DisplayNameAttribute("HighMemberPath")				);

				#endregion // RangeCategorySeries Properties

				#region RangeAreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RangeAreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RangeAreaSeries Properties

				#region TRIXIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.TRIXIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("TRIXIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // TRIXIndicator Properties

				#region SlowStochasticOscillatorIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SlowStochasticOscillatorIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("SlowStochasticOscillatorIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // SlowStochasticOscillatorIndicator Properties

				#region NegativeVolumeIndexIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.NegativeVolumeIndexIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NegativeVolumeIndexIndicator Properties

				#region MedianPriceIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.MedianPriceIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MedianPriceIndicator Properties

				#region ItemwiseStrategyBasedIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ItemwiseStrategyBasedIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ItemwiseStrategyBasedIndicator Properties

				#region ForceIndexIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ForceIndexIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("ForceIndexIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // ForceIndexIndicator Properties

				#region SplineSeriesBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SplineSeriesBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SplineType",
					new DescriptionAttribute(SR.GetString("SplineSeriesBase_SplineType_Property")),
				    new DisplayNameAttribute("SplineType")				);

				#endregion // SplineSeriesBase Properties

				#region WeightedCloseIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.WeightedCloseIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // WeightedCloseIndicator Properties

				#region RelativeStrengthIndexIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RelativeStrengthIndexIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("RelativeStrengthIndexIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // RelativeStrengthIndexIndicator Properties

				#region RateOfChangeAndMomentumIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RateOfChangeAndMomentumIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("RateOfChangeAndMomentumIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // RateOfChangeAndMomentumIndicator Properties

				#region PriceVolumeTrendIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PriceVolumeTrendIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PriceVolumeTrendIndicator Properties

				#region OnBalanceVolumeIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.OnBalanceVolumeIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // OnBalanceVolumeIndicator Properties

				#region FastStochasticOscillatorIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FastStochasticOscillatorIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("FastStochasticOscillatorIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // FastStochasticOscillatorIndicator Properties

				#region DetrendedPriceOscillatorIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DetrendedPriceOscillatorIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("DetrendedPriceOscillatorIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // DetrendedPriceOscillatorIndicator Properties

				#region ChaikinVolatilityIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ChaikinVolatilityIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("ChaikinVolatilityIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // ChaikinVolatilityIndicator Properties

				#region ScatterSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ScatterSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScatterSeries Properties

				#region StandardDeviationIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StandardDeviationIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("StandardDeviationIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // StandardDeviationIndicator Properties

				#region EaseOfMovementIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.EaseOfMovementIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // EaseOfMovementIndicator Properties

				#region PercentageVolumeOscillatorIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PercentageVolumeOscillatorIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ShortPeriod",
					new DescriptionAttribute(SR.GetString("PercentageVolumeOscillatorIndicator_ShortPeriod_Property")),
				    new DisplayNameAttribute("ShortPeriod")				);


				tableBuilder.AddCustomAttributes(t, "LongPeriod",
					new DescriptionAttribute(SR.GetString("PercentageVolumeOscillatorIndicator_LongPeriod_Property")),
				    new DisplayNameAttribute("LongPeriod")				);

				#endregion // PercentageVolumeOscillatorIndicator Properties

				#region BollingerBandsOverlay Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.BollingerBandsOverlay");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("BollingerBandsOverlay_Period_Property")),
				    new DisplayNameAttribute("Period")				);


				tableBuilder.AddCustomAttributes(t, "Multiplier",
					new DescriptionAttribute(SR.GetString("BollingerBandsOverlay_Multiplier_Property")),
				    new DisplayNameAttribute("Multiplier")				);

				#endregion // BollingerBandsOverlay Properties

				#region UltimateOscillatorIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.UltimateOscillatorIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // UltimateOscillatorIndicator Properties

				#region MovingAverageConvergenceDivergenceIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.MovingAverageConvergenceDivergenceIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ShortPeriod",
					new DescriptionAttribute(SR.GetString("MovingAverageConvergenceDivergenceIndicator_ShortPeriod_Property")),
				    new DisplayNameAttribute("ShortPeriod")				);


				tableBuilder.AddCustomAttributes(t, "LongPeriod",
					new DescriptionAttribute(SR.GetString("MovingAverageConvergenceDivergenceIndicator_LongPeriod_Property")),
				    new DisplayNameAttribute("LongPeriod")				);


				tableBuilder.AddCustomAttributes(t, "SignalPeriod",
					new DescriptionAttribute(SR.GetString("MovingAverageConvergenceDivergenceIndicator_SignalPeriod_Property")),
				    new DisplayNameAttribute("SignalPeriod")				);

				#endregion // MovingAverageConvergenceDivergenceIndicator Properties

				#region SplineAreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SplineAreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SplineAreaSeries Properties

				#region LineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.LineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "UnknownValuePlotting",
					new DescriptionAttribute(SR.GetString("LineSeries_UnknownValuePlotting_Property")),
				    new DisplayNameAttribute("UnknownValuePlotting")				);

				#endregion // LineSeries Properties

				#region MoneyFlowIndexIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.MoneyFlowIndexIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("MoneyFlowIndexIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // MoneyFlowIndexIndicator Properties

				#region CommodityChannelIndexIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CommodityChannelIndexIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("CommodityChannelIndexIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // CommodityChannelIndexIndicator Properties

				#region ChaikinOscillatorIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ChaikinOscillatorIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ShortPeriod",
					new DescriptionAttribute(SR.GetString("ChaikinOscillatorIndicator_ShortPeriod_Property")),
				    new DisplayNameAttribute("ShortPeriod")				);


				tableBuilder.AddCustomAttributes(t, "LongPeriod",
					new DescriptionAttribute(SR.GetString("ChaikinOscillatorIndicator_LongPeriod_Property")),
				    new DisplayNameAttribute("LongPeriod")				);

				#endregion // ChaikinOscillatorIndicator Properties

				#region AbsoluteVolumeOscillatorIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AbsoluteVolumeOscillatorIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ShortPeriod",
					new DescriptionAttribute(SR.GetString("AbsoluteVolumeOscillatorIndicator_ShortPeriod_Property")),
				    new DisplayNameAttribute("ShortPeriod")				);


				tableBuilder.AddCustomAttributes(t, "LongPeriod",
					new DescriptionAttribute(SR.GetString("AbsoluteVolumeOscillatorIndicator_LongPeriod_Property")),
				    new DisplayNameAttribute("LongPeriod")				);

				#endregion // AbsoluteVolumeOscillatorIndicator Properties

				#region SeriesCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SeriesCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SeriesCollection Properties

				#region StepAreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StepAreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StepAreaSeries Properties

				#region SplineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SplineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SplineSeries Properties

				#region PriceChannelOverlay Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PriceChannelOverlay");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("PriceChannelOverlay_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // PriceChannelOverlay Properties

				#region BollingerBandWidthIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.BollingerBandWidthIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("BollingerBandWidthIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);


				tableBuilder.AddCustomAttributes(t, "Multiplier",
					new DescriptionAttribute(SR.GetString("BollingerBandWidthIndicator_Multiplier_Property")),
				    new DisplayNameAttribute("Multiplier")				);

				#endregion // BollingerBandWidthIndicator Properties

				#region FullStochasticOscillatorIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FullStochasticOscillatorIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("FullStochasticOscillatorIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);


				tableBuilder.AddCustomAttributes(t, "SmoothingPeriod",
					new DescriptionAttribute(SR.GetString("FullStochasticOscillatorIndicator_SmoothingPeriod_Property")),
				    new DisplayNameAttribute("SmoothingPeriod")				);


				tableBuilder.AddCustomAttributes(t, "TriggerPeriod",
					new DescriptionAttribute(SR.GetString("FullStochasticOscillatorIndicator_TriggerPeriod_Property")),
				    new DisplayNameAttribute("TriggerPeriod")				);

				#endregion // FullStochasticOscillatorIndicator Properties

				#region AverageDirectionalIndexIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AverageDirectionalIndexIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("AverageDirectionalIndexIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // AverageDirectionalIndexIndicator Properties

				#region TypicalPriceIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.TypicalPriceIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TypicalPriceIndicator Properties

				#region WaterfallSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.WaterfallSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NegativeBrush",
					new DescriptionAttribute(SR.GetString("WaterfallSeries_NegativeBrush_Property")),
				    new DisplayNameAttribute("NegativeBrush")				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("WaterfallSeries_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("WaterfallSeries_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);

				#endregion // WaterfallSeries Properties

				#region WilliamsPercentRIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.WilliamsPercentRIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Period",
					new DescriptionAttribute(SR.GetString("WilliamsPercentRIndicator_Period_Property")),
				    new DisplayNameAttribute("Period")				);

				#endregion // WilliamsPercentRIndicator Properties

				#region MarketFacilitationIndexIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.MarketFacilitationIndexIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MarketFacilitationIndexIndicator Properties

				#region AccumulationDistributionIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AccumulationDistributionIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AccumulationDistributionIndicator Properties

				#region SafeReadOnlyDoubleCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.SafeReadOnlyDoubleCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("SafeReadOnlyDoubleCollection_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("SafeReadOnlyDoubleCollection_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);

				#endregion // SafeReadOnlyDoubleCollection Properties

				#region SafeSortedReadOnlyDoubleCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.SafeSortedReadOnlyDoubleCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("SafeSortedReadOnlyDoubleCollection_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("SafeSortedReadOnlyDoubleCollection_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);

				#endregion // SafeSortedReadOnlyDoubleCollection Properties

				#region VisualInformationManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualInformationManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // VisualInformationManager Properties

				#region SyncSettingsConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SyncSettingsConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SyncSettingsConverter Properties

				#region SyncManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SyncManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SyncManager Properties

				#region NumericAngleAxis Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.NumericAngleAxis");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "StartAngleOffset",
					new DescriptionAttribute(SR.GetString("NumericAngleAxis_StartAngleOffset_Property")),
				    new DisplayNameAttribute("StartAngleOffset")				);

				#endregion // NumericAngleAxis Properties

				#region NumericRadiusAxis Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.NumericRadiusAxis");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusExtentScale",
					new DescriptionAttribute(SR.GetString("NumericRadiusAxis_RadiusExtentScale_Property")),
				    new DisplayNameAttribute("RadiusExtentScale")				);


				tableBuilder.AddCustomAttributes(t, "InnerRadiusExtentScale",
					new DescriptionAttribute(SR.GetString("NumericRadiusAxis_InnerRadiusExtentScale_Property")),
				    new DisplayNameAttribute("InnerRadiusExtentScale")				);

				#endregion // NumericRadiusAxis Properties

				#region PolarAxes Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PolarAxes");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusAxis",
					new DescriptionAttribute(SR.GetString("PolarAxes_RadiusAxis_Property")),
				    new DisplayNameAttribute("RadiusAxis")				);


				tableBuilder.AddCustomAttributes(t, "AngleAxis",
					new DescriptionAttribute(SR.GetString("PolarAxes_AngleAxis_Property")),
				    new DisplayNameAttribute("AngleAxis")				);

				#endregion // PolarAxes Properties

				#region PolarBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PolarBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AngleMemberPath",
					new DescriptionAttribute(SR.GetString("PolarBase_AngleMemberPath_Property")),
				    new DisplayNameAttribute("AngleMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "RadiusMemberPath",
					new DescriptionAttribute(SR.GetString("PolarBase_RadiusMemberPath_Property")),
				    new DisplayNameAttribute("RadiusMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "AngleAxis",
					new DescriptionAttribute(SR.GetString("PolarBase_AngleAxis_Property")),
				    new DisplayNameAttribute("AngleAxis")				);


				tableBuilder.AddCustomAttributes(t, "RadiusAxis",
					new DescriptionAttribute(SR.GetString("PolarBase_RadiusAxis_Property")),
				    new DisplayNameAttribute("RadiusAxis")				);


				tableBuilder.AddCustomAttributes(t, "UseCartesianInterpolation",
					new DescriptionAttribute(SR.GetString("PolarBase_UseCartesianInterpolation_Property")),
				    new DisplayNameAttribute("UseCartesianInterpolation")				);


				tableBuilder.AddCustomAttributes(t, "MaximumMarkers",
					new DescriptionAttribute(SR.GetString("PolarBase_MaximumMarkers_Property")),
				    new DisplayNameAttribute("MaximumMarkers")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineType",
					new DescriptionAttribute(SR.GetString("PolarBase_TrendLineType_Property")),
				    new DisplayNameAttribute("TrendLineType")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineBrush",
					new DescriptionAttribute(SR.GetString("PolarBase_TrendLineBrush_Property")),
				    new DisplayNameAttribute("TrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "ActualTrendLineBrush",
					new DescriptionAttribute(SR.GetString("PolarBase_ActualTrendLineBrush_Property")),
				    new DisplayNameAttribute("ActualTrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineThickness",
					new DescriptionAttribute(SR.GetString("PolarBase_TrendLineThickness_Property")),
				    new DisplayNameAttribute("TrendLineThickness")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashCap",
					new DescriptionAttribute(SR.GetString("PolarBase_TrendLineDashCap_Property")),
				    new DisplayNameAttribute("TrendLineDashCap")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashArray",
					new DescriptionAttribute(SR.GetString("PolarBase_TrendLineDashArray_Property")),
				    new DisplayNameAttribute("TrendLineDashArray")				);


				tableBuilder.AddCustomAttributes(t, "TrendLinePeriod",
					new DescriptionAttribute(SR.GetString("PolarBase_TrendLinePeriod_Property")),
				    new DisplayNameAttribute("TrendLinePeriod")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineZIndex",
					new DescriptionAttribute(SR.GetString("PolarBase_TrendLineZIndex_Property")),
				    new DisplayNameAttribute("TrendLineZIndex")				);


				tableBuilder.AddCustomAttributes(t, "ClipSeriesToBounds",
					new DescriptionAttribute(SR.GetString("PolarBase_ClipSeriesToBounds_Property")),
				    new DisplayNameAttribute("ClipSeriesToBounds")				);

				#endregion // PolarBase Properties

				#region PolarSplineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PolarSplineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Stiffness",
					new DescriptionAttribute(SR.GetString("PolarSplineSeries_Stiffness_Property")),
				    new DisplayNameAttribute("Stiffness")				);

				#endregion // PolarSplineSeries Properties

				#region PolarLineSeriesBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PolarLineSeriesBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PolarLineSeriesBase Properties

				#region PolarScatterSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PolarScatterSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PolarScatterSeries Properties

				#region PolarLineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PolarLineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "UnknownValuePlotting",
					new DescriptionAttribute(SR.GetString("PolarLineSeries_UnknownValuePlotting_Property")),
				    new DisplayNameAttribute("UnknownValuePlotting")				);

				#endregion // PolarLineSeries Properties

				#region CategoryAngleAxis Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CategoryAngleAxis");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "StartAngleOffset",
					new DescriptionAttribute(SR.GetString("CategoryAngleAxis_StartAngleOffset_Property")),
				    new DisplayNameAttribute("StartAngleOffset")				);


				tableBuilder.AddCustomAttributes(t, "Interval",
					new DescriptionAttribute(SR.GetString("CategoryAngleAxis_Interval_Property")),
				    new DisplayNameAttribute("Interval")				);

				#endregion // CategoryAngleAxis Properties

				#region RadialAxes Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RadialAxes");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusAxis",
					new DescriptionAttribute(SR.GetString("RadialAxes_RadiusAxis_Property")),
				    new DisplayNameAttribute("RadiusAxis")				);


				tableBuilder.AddCustomAttributes(t, "AngleAxis",
					new DescriptionAttribute(SR.GetString("RadialAxes_AngleAxis_Property")),
				    new DisplayNameAttribute("AngleAxis")				);

				#endregion // RadialAxes Properties

				#region AnchoredRadialSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AnchoredRadialSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ValueMemberPath",
					new DescriptionAttribute(SR.GetString("AnchoredRadialSeries_ValueMemberPath_Property")),
				    new DisplayNameAttribute("ValueMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineType",
					new DescriptionAttribute(SR.GetString("AnchoredRadialSeries_TrendLineType_Property")),
				    new DisplayNameAttribute("TrendLineType")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineBrush",
					new DescriptionAttribute(SR.GetString("AnchoredRadialSeries_TrendLineBrush_Property")),
				    new DisplayNameAttribute("TrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "ActualTrendLineBrush",
					new DescriptionAttribute(SR.GetString("AnchoredRadialSeries_ActualTrendLineBrush_Property")),
				    new DisplayNameAttribute("ActualTrendLineBrush")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineThickness",
					new DescriptionAttribute(SR.GetString("AnchoredRadialSeries_TrendLineThickness_Property")),
				    new DisplayNameAttribute("TrendLineThickness")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashCap",
					new DescriptionAttribute(SR.GetString("AnchoredRadialSeries_TrendLineDashCap_Property")),
				    new DisplayNameAttribute("TrendLineDashCap")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineDashArray",
					new DescriptionAttribute(SR.GetString("AnchoredRadialSeries_TrendLineDashArray_Property")),
				    new DisplayNameAttribute("TrendLineDashArray")				);


				tableBuilder.AddCustomAttributes(t, "TrendLinePeriod",
					new DescriptionAttribute(SR.GetString("AnchoredRadialSeries_TrendLinePeriod_Property")),
				    new DisplayNameAttribute("TrendLinePeriod")				);


				tableBuilder.AddCustomAttributes(t, "TrendLineZIndex",
					new DescriptionAttribute(SR.GetString("AnchoredRadialSeries_TrendLineZIndex_Property")),
				    new DisplayNameAttribute("TrendLineZIndex")				);

				#endregion // AnchoredRadialSeries Properties

				#region RadialBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RadialBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AngleAxis",
					new DescriptionAttribute(SR.GetString("RadialBase_AngleAxis_Property")),
				    new DisplayNameAttribute("AngleAxis")				);


				tableBuilder.AddCustomAttributes(t, "ValueAxis",
					new DescriptionAttribute(SR.GetString("RadialBase_ValueAxis_Property")),
				    new DisplayNameAttribute("ValueAxis")				);


				tableBuilder.AddCustomAttributes(t, "ClipSeriesToBounds",
					new DescriptionAttribute(SR.GetString("RadialBase_ClipSeriesToBounds_Property")),
				    new DisplayNameAttribute("ClipSeriesToBounds")				);

				#endregion // RadialBase Properties

				#region RadialAreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RadialAreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "UnknownValuePlotting",
					new DescriptionAttribute(SR.GetString("RadialAreaSeries_UnknownValuePlotting_Property")),
				    new DisplayNameAttribute("UnknownValuePlotting")				);

				#endregion // RadialAreaSeries Properties

				#region RadialColumnSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RadialColumnSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("RadialColumnSeries_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("RadialColumnSeries_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);

				#endregion // RadialColumnSeries Properties

				#region RadialLineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RadialLineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "UnknownValuePlotting",
					new DescriptionAttribute(SR.GetString("RadialLineSeries_UnknownValuePlotting_Property")),
				    new DisplayNameAttribute("UnknownValuePlotting")				);

				#endregion // RadialLineSeries Properties

				#region RadialPieSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RadialPieSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("RadialPieSeries_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("RadialPieSeries_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);

				#endregion // RadialPieSeries Properties

				#region PolarAreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PolarAreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "UnknownValuePlotting",
					new DescriptionAttribute(SR.GetString("PolarAreaSeries_UnknownValuePlotting_Property")),
				    new DisplayNameAttribute("UnknownValuePlotting")				);

				#endregion // PolarAreaSeries Properties

				#region PolarSplineAreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PolarSplineAreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Stiffness",
					new DescriptionAttribute(SR.GetString("PolarSplineAreaSeries_Stiffness_Property")),
				    new DisplayNameAttribute("Stiffness")				);

				#endregion // PolarSplineAreaSeries Properties

				#region BubbleSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.BubbleSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusMemberPath",
					new DescriptionAttribute(SR.GetString("BubbleSeries_RadiusMemberPath_Property")),
				    new DisplayNameAttribute("RadiusMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "RadiusScale",
					new DescriptionAttribute(SR.GetString("BubbleSeries_RadiusScale_Property")),
				    new DisplayNameAttribute("RadiusScale")				);


				tableBuilder.AddCustomAttributes(t, "LabelMemberPath",
					new DescriptionAttribute(SR.GetString("BubbleSeries_LabelMemberPath_Property")),
				    new DisplayNameAttribute("LabelMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "FillMemberPath",
					new DescriptionAttribute(SR.GetString("BubbleSeries_FillMemberPath_Property")),
				    new DisplayNameAttribute("FillMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "FillScale",
					new DescriptionAttribute(SR.GetString("BubbleSeries_FillScale_Property")),
				    new DisplayNameAttribute("FillScale")				);

				#endregion // BubbleSeries Properties

				#region StackedFragmentSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedFragmentSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);


				tableBuilder.AddCustomAttributes(t, "Brush",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_Brush_Property")),
				    new DisplayNameAttribute("Brush")				);


				tableBuilder.AddCustomAttributes(t, "Cursor",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_Cursor_Property")),
				    new DisplayNameAttribute("Cursor")				);


				tableBuilder.AddCustomAttributes(t, "DashArray",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_DashArray_Property")),
				    new DisplayNameAttribute("DashArray")				);


				tableBuilder.AddCustomAttributes(t, "DashCap",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_DashCap_Property")),
				    new DisplayNameAttribute("DashCap")				);


				tableBuilder.AddCustomAttributes(t, "Effect",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_Effect_Property")),
				    new DisplayNameAttribute("Effect")				);


				tableBuilder.AddCustomAttributes(t, "EndCap",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_EndCap_Property")),
				    new DisplayNameAttribute("EndCap")				);


				tableBuilder.AddCustomAttributes(t, "IsHitTestVisible",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_IsHitTestVisible_Property")),
				    new DisplayNameAttribute("IsHitTestVisible")				);


				tableBuilder.AddCustomAttributes(t, "LegendItemBadgeTemplate",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_LegendItemBadgeTemplate_Property")),
				    new DisplayNameAttribute("LegendItemBadgeTemplate")				);


				tableBuilder.AddCustomAttributes(t, "LegendItemTemplate",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_LegendItemTemplate_Property")),
				    new DisplayNameAttribute("LegendItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "LegendItemVisibility",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_LegendItemVisibility_Property")),
				    new DisplayNameAttribute("LegendItemVisibility")				);


				tableBuilder.AddCustomAttributes(t, "MarkerBrush",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_MarkerBrush_Property")),
				    new DisplayNameAttribute("MarkerBrush")				);


				tableBuilder.AddCustomAttributes(t, "MarkerOutline",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_MarkerOutline_Property")),
				    new DisplayNameAttribute("MarkerOutline")				);


				tableBuilder.AddCustomAttributes(t, "MarkerStyle",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_MarkerStyle_Property")),
				    new DisplayNameAttribute("MarkerStyle")				);


				tableBuilder.AddCustomAttributes(t, "MarkerTemplate",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_MarkerTemplate_Property")),
				    new DisplayNameAttribute("MarkerTemplate")				);


				tableBuilder.AddCustomAttributes(t, "MarkerType",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_MarkerType_Property")),
				    new DisplayNameAttribute("MarkerType")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "Opacity",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_Opacity_Property")),
				    new DisplayNameAttribute("Opacity")				);


				tableBuilder.AddCustomAttributes(t, "OpacityMask",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_OpacityMask_Property")),
				    new DisplayNameAttribute("OpacityMask")				);


				tableBuilder.AddCustomAttributes(t, "Outline",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_Outline_Property")),
				    new DisplayNameAttribute("Outline")				);


				tableBuilder.AddCustomAttributes(t, "StartCap",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_StartCap_Property")),
				    new DisplayNameAttribute("StartCap")				);


				tableBuilder.AddCustomAttributes(t, "Thickness",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_Thickness_Property")),
				    new DisplayNameAttribute("Thickness")				);


				tableBuilder.AddCustomAttributes(t, "Title",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_Title_Property")),
				    new DisplayNameAttribute("Title")				);


				tableBuilder.AddCustomAttributes(t, "ToolTip",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ToolTip_Property")),
				    new DisplayNameAttribute("ToolTip")				);


				tableBuilder.AddCustomAttributes(t, "UseLightweightMarkers",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_UseLightweightMarkers_Property")),
				    new DisplayNameAttribute("UseLightweightMarkers")				);


				tableBuilder.AddCustomAttributes(t, "ValueMemberPath",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ValueMemberPath_Property")),
				    new DisplayNameAttribute("ValueMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "Visibility",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_Visibility_Property")),
				    new DisplayNameAttribute("Visibility")				);


				tableBuilder.AddCustomAttributes(t, "ActualBrush",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualBrush_Property")),
				    new DisplayNameAttribute("ActualBrush")				);


				tableBuilder.AddCustomAttributes(t, "ActualCursor",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualCursor_Property")),
				    new DisplayNameAttribute("ActualCursor")				);


				tableBuilder.AddCustomAttributes(t, "ActualDashArray",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualDashArray_Property")),
				    new DisplayNameAttribute("ActualDashArray")				);


				tableBuilder.AddCustomAttributes(t, "ActualDashCap",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualDashCap_Property")),
				    new DisplayNameAttribute("ActualDashCap")				);


				tableBuilder.AddCustomAttributes(t, "ActualEffect",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualEffect_Property")),
				    new DisplayNameAttribute("ActualEffect")				);


				tableBuilder.AddCustomAttributes(t, "ActualEndCap",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualEndCap_Property")),
				    new DisplayNameAttribute("ActualEndCap")				);


				tableBuilder.AddCustomAttributes(t, "ActualIsHitTestVisible",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualIsHitTestVisible_Property")),
				    new DisplayNameAttribute("ActualIsHitTestVisible")				);


				tableBuilder.AddCustomAttributes(t, "ActualLegendItemBadgeTemplate",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualLegendItemBadgeTemplate_Property")),
				    new DisplayNameAttribute("ActualLegendItemBadgeTemplate")				);


				tableBuilder.AddCustomAttributes(t, "ActualLegendItemTemplate",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualLegendItemTemplate_Property")),
				    new DisplayNameAttribute("ActualLegendItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "ActualLegendItemVisibility",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualLegendItemVisibility_Property")),
				    new DisplayNameAttribute("ActualLegendItemVisibility")				);


				tableBuilder.AddCustomAttributes(t, "ActualMarkerBrush",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualMarkerBrush_Property")),
				    new DisplayNameAttribute("ActualMarkerBrush")				);


				tableBuilder.AddCustomAttributes(t, "ActualMarkerOutline",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualMarkerOutline_Property")),
				    new DisplayNameAttribute("ActualMarkerOutline")				);


				tableBuilder.AddCustomAttributes(t, "ActualMarkerStyle",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualMarkerStyle_Property")),
				    new DisplayNameAttribute("ActualMarkerStyle")				);


				tableBuilder.AddCustomAttributes(t, "ActualMarkerTemplate",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualMarkerTemplate_Property")),
				    new DisplayNameAttribute("ActualMarkerTemplate")				);


				tableBuilder.AddCustomAttributes(t, "ActualMarkerType",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualMarkerType_Property")),
				    new DisplayNameAttribute("ActualMarkerType")				);


				tableBuilder.AddCustomAttributes(t, "ActualOpacity",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualOpacity_Property")),
				    new DisplayNameAttribute("ActualOpacity")				);


				tableBuilder.AddCustomAttributes(t, "ActualOpacityMask",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualOpacityMask_Property")),
				    new DisplayNameAttribute("ActualOpacityMask")				);


				tableBuilder.AddCustomAttributes(t, "ActualOutline",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualOutline_Property")),
				    new DisplayNameAttribute("ActualOutline")				);


				tableBuilder.AddCustomAttributes(t, "ActualRadiusX",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualRadiusX_Property")),
				    new DisplayNameAttribute("ActualRadiusX")				);


				tableBuilder.AddCustomAttributes(t, "ActualRadiusY",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualRadiusY_Property")),
				    new DisplayNameAttribute("ActualRadiusY")				);


				tableBuilder.AddCustomAttributes(t, "ActualStartCap",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualStartCap_Property")),
				    new DisplayNameAttribute("ActualStartCap")				);


				tableBuilder.AddCustomAttributes(t, "ActualToolTip",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualToolTip_Property")),
				    new DisplayNameAttribute("ActualToolTip")				);


				tableBuilder.AddCustomAttributes(t, "ActualUseLightweightMarkers",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualUseLightweightMarkers_Property")),
				    new DisplayNameAttribute("ActualUseLightweightMarkers")				);


				tableBuilder.AddCustomAttributes(t, "ActualVisibility",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualVisibility_Property")),
				    new DisplayNameAttribute("ActualVisibility")				);


				tableBuilder.AddCustomAttributes(t, "ActualThickness",
					new DescriptionAttribute(SR.GetString("StackedFragmentSeries_ActualThickness_Property")),
				    new DisplayNameAttribute("ActualThickness")				);

				#endregion // StackedFragmentSeries Properties

				#region Slice Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Slice");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "StartAngle",
					new DescriptionAttribute(SR.GetString("Slice_StartAngle_Property")),
				    new DisplayNameAttribute("StartAngle")				);


				tableBuilder.AddCustomAttributes(t, "EndAngle",
					new DescriptionAttribute(SR.GetString("Slice_EndAngle_Property")),
				    new DisplayNameAttribute("EndAngle")				);


				tableBuilder.AddCustomAttributes(t, "InnerExtentStart",
					new DescriptionAttribute(SR.GetString("Slice_InnerExtentStart_Property")),
				    new DisplayNameAttribute("InnerExtentStart")				);


				tableBuilder.AddCustomAttributes(t, "InnerExtentEnd",
					new DescriptionAttribute(SR.GetString("Slice_InnerExtentEnd_Property")),
				    new DisplayNameAttribute("InnerExtentEnd")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("Slice_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "IsExploded",
					new DescriptionAttribute(SR.GetString("Slice_IsExploded_Property")),
				    new DisplayNameAttribute("IsExploded")				);


				tableBuilder.AddCustomAttributes(t, "IsOthersSlice",
					new DescriptionAttribute(SR.GetString("Slice_IsOthersSlice_Property")),
				    new DisplayNameAttribute("IsOthersSlice")				);


				tableBuilder.AddCustomAttributes(t, "Origin",
					new DescriptionAttribute(SR.GetString("Slice_Origin_Property")),
				    new DisplayNameAttribute("Origin")				);


				tableBuilder.AddCustomAttributes(t, "ExplodedOrigin",
					new DescriptionAttribute(SR.GetString("Slice_ExplodedOrigin_Property")),
				    new DisplayNameAttribute("ExplodedOrigin")				);


				tableBuilder.AddCustomAttributes(t, "Radius",
					new DescriptionAttribute(SR.GetString("Slice_Radius_Property")),
				    new DisplayNameAttribute("Radius")				);


				tableBuilder.AddCustomAttributes(t, "ExplodedRadius",
					new DescriptionAttribute(SR.GetString("Slice_ExplodedRadius_Property")),
				    new DisplayNameAttribute("ExplodedRadius")				);


				tableBuilder.AddCustomAttributes(t, "Index",
					new DescriptionAttribute(SR.GetString("Slice_Index_Property")),
				    new DisplayNameAttribute("Index")				);


				tableBuilder.AddCustomAttributes(t, "StrokeThickness",
					new DescriptionAttribute(SR.GetString("Slice_StrokeThickness_Property")),
				    new DisplayNameAttribute("StrokeThickness")				);

				#endregion // Slice Properties

				#region SizeScale Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SizeScale");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MinimumValue",
					new DescriptionAttribute(SR.GetString("SizeScale_MinimumValue_Property")),
				    new DisplayNameAttribute("MinimumValue")				);


				tableBuilder.AddCustomAttributes(t, "MaximumValue",
					new DescriptionAttribute(SR.GetString("SizeScale_MaximumValue_Property")),
				    new DisplayNameAttribute("MaximumValue")				);


				tableBuilder.AddCustomAttributes(t, "IsLogarithmic",
					new DescriptionAttribute(SR.GetString("SizeScale_IsLogarithmic_Property")),
				    new DisplayNameAttribute("IsLogarithmic")				);


				tableBuilder.AddCustomAttributes(t, "LogarithmBase",
					new DescriptionAttribute(SR.GetString("SizeScale_LogarithmBase_Property")),
				    new DisplayNameAttribute("LogarithmBase")				);

				#endregion // SizeScale Properties

				#region ValueOverlay Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ValueOverlay");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Axis",
					new DescriptionAttribute(SR.GetString("ValueOverlay_Axis_Property")),
				    new DisplayNameAttribute("Axis")				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("ValueOverlay_Value_Property")),
				    new DisplayNameAttribute("Value")				);

				#endregion // ValueOverlay Properties

				#region PieChartBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PieChartBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ContentPresenter",
					new DescriptionAttribute(SR.GetString("PieChartBase_ContentPresenter_Property")),
				    new DisplayNameAttribute("ContentPresenter"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ToolTip",
					new DescriptionAttribute(SR.GetString("PieChartBase_ToolTip_Property")),
				    new DisplayNameAttribute("ToolTip"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ExplodedRadius",
					new DescriptionAttribute(SR.GetString("PieChartBase_ExplodedRadius_Property")),
				    new DisplayNameAttribute("ExplodedRadius"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Brushes",
					new DescriptionAttribute(SR.GetString("PieChartBase_Brushes_Property")),
				    new DisplayNameAttribute("Brushes"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("PieChartBase_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueMemberPath",
					new DescriptionAttribute(SR.GetString("PieChartBase_ValueMemberPath_Property")),
				    new DisplayNameAttribute("ValueMemberPath"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LabelMemberPath",
					new DescriptionAttribute(SR.GetString("PieChartBase_LabelMemberPath_Property")),
				    new DisplayNameAttribute("LabelMemberPath"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LabelsPosition",
					new DescriptionAttribute(SR.GetString("PieChartBase_LabelsPosition_Property")),
				    new DisplayNameAttribute("LabelsPosition"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LeaderLineVisibility",
					new DescriptionAttribute(SR.GetString("PieChartBase_LeaderLineVisibility_Property")),
				    new DisplayNameAttribute("LeaderLineVisibility"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LeaderLineStyle",
					new DescriptionAttribute(SR.GetString("PieChartBase_LeaderLineStyle_Property")),
				    new DisplayNameAttribute("LeaderLineStyle"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OthersCategoryThreshold",
					new DescriptionAttribute(SR.GetString("PieChartBase_OthersCategoryThreshold_Property")),
				    new DisplayNameAttribute("OthersCategoryThreshold"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OthersCategoryType",
					new DescriptionAttribute(SR.GetString("PieChartBase_OthersCategoryType_Property")),
				    new DisplayNameAttribute("OthersCategoryType"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OthersCategoryText",
					new DescriptionAttribute(SR.GetString("PieChartBase_OthersCategoryText_Property")),
				    new DisplayNameAttribute("OthersCategoryText"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusFactor",
					new DescriptionAttribute(SR.GetString("PieChartBase_RadiusFactor_Property")),
				    new DisplayNameAttribute("RadiusFactor"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowSliceSelection",
					new DescriptionAttribute(SR.GetString("PieChartBase_AllowSliceSelection_Property")),
				    new DisplayNameAttribute("AllowSliceSelection"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowSliceExplosion",
					new DescriptionAttribute(SR.GetString("PieChartBase_AllowSliceExplosion_Property")),
				    new DisplayNameAttribute("AllowSliceExplosion"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ExplodedSlices",
					new DescriptionAttribute(SR.GetString("PieChartBase_ExplodedSlices_Property")),
				    new DisplayNameAttribute("ExplodedSlices"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Legend",
					new DescriptionAttribute(SR.GetString("PieChartBase_Legend_Property")),
				    new DisplayNameAttribute("Legend"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LabelExtent",
					new DescriptionAttribute(SR.GetString("PieChartBase_LabelExtent_Property")),
				    new DisplayNameAttribute("LabelExtent"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "StartAngle",
					new DescriptionAttribute(SR.GetString("PieChartBase_StartAngle_Property")),
				    new DisplayNameAttribute("StartAngle"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SweepDirection",
					new DescriptionAttribute(SR.GetString("PieChartBase_SweepDirection_Property")),
				    new DisplayNameAttribute("SweepDirection"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedSlices",
					new DescriptionAttribute(SR.GetString("PieChartBase_SelectedSlices_Property")),
				    new DisplayNameAttribute("SelectedSlices"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OthersCategoryStyle",
					new DescriptionAttribute(SR.GetString("PieChartBase_OthersCategoryStyle_Property")),
				    new DisplayNameAttribute("OthersCategoryStyle"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedStyle",
					new DescriptionAttribute(SR.GetString("PieChartBase_SelectedStyle_Property")),
				    new DisplayNameAttribute("SelectedStyle"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ToolTipStyle",
					new DescriptionAttribute(SR.GetString("PieChartBase_ToolTipStyle_Property")),
				    new DisplayNameAttribute("ToolTipStyle"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Outlines",
					new DescriptionAttribute(SR.GetString("PieChartBase_Outlines_Property")),
				    new DisplayNameAttribute("Outlines"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LegendItemTemplate",
					new DescriptionAttribute(SR.GetString("PieChartBase_LegendItemTemplate_Property")),
				    new DisplayNameAttribute("LegendItemTemplate"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LegendItemBadgeTemplate",
					new DescriptionAttribute(SR.GetString("PieChartBase_LegendItemBadgeTemplate_Property")),
				    new DisplayNameAttribute("LegendItemBadgeTemplate"),
					new CategoryAttribute(SR.GetString("XamPieChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LabelTemplate",
					new DescriptionAttribute(SR.GetString("PieChartBase_LabelTemplate_Property")),
				    new DisplayNameAttribute("LabelTemplate")				);

				#endregion // PieChartBase Properties

				#region StackedColumnSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedColumnSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("StackedColumnSeries_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("StackedColumnSeries_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);

				#endregion // StackedColumnSeries Properties

				#region StackedSeriesBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedSeriesBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Series",
					new DescriptionAttribute(SR.GetString("StackedSeriesBase_Series_Property")),
				    new DisplayNameAttribute("Series")				);


				tableBuilder.AddCustomAttributes(t, "AutoGenerateSeries",
					new DescriptionAttribute(SR.GetString("StackedSeriesBase_AutoGenerateSeries_Property")),
				    new DisplayNameAttribute("AutoGenerateSeries")				);


				tableBuilder.AddCustomAttributes(t, "ReverseLegendOrder",
					new DescriptionAttribute(SR.GetString("StackedSeriesBase_ReverseLegendOrder_Property")),
				    new DisplayNameAttribute("ReverseLegendOrder")				);

				#endregion // StackedSeriesBase Properties

				#region CustomPaletteBrushScale Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CustomPaletteBrushScale");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "BrushSelectionMode",
					new DescriptionAttribute(SR.GetString("CustomPaletteBrushScale_BrushSelectionMode_Property")),
				    new DisplayNameAttribute("BrushSelectionMode")				);

				#endregion // CustomPaletteBrushScale Properties

				#region BrushScale Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.BrushScale");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Brushes",
					new DescriptionAttribute(SR.GetString("BrushScale_Brushes_Property")),
				    new DisplayNameAttribute("Brushes")				);

				#endregion // BrushScale Properties

				#region HorizontalAxisLabelPanelBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.HorizontalAxisLabelPanelBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HorizontalAxisLabelPanelBase Properties

				#region AxisLabelPanelBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AxisLabelPanelBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AxisLabelPanelBase Properties

				#region HorizontalAxisLabelPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.HorizontalAxisLabelPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HorizontalAxisLabelPanel Properties

				#region RadialAxisLabelPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RadialAxisLabelPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RotationCenter",
					new DescriptionAttribute(SR.GetString("RadialAxisLabelPanel_RotationCenter_Property")),
				    new DisplayNameAttribute("RotationCenter")				);


				tableBuilder.AddCustomAttributes(t, "CrossingAngle",
					new DescriptionAttribute(SR.GetString("RadialAxisLabelPanel_CrossingAngle_Property")),
				    new DisplayNameAttribute("CrossingAngle")				);

				#endregion // RadialAxisLabelPanel Properties

				#region Stacked100BarSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Stacked100BarSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Stacked100BarSeries Properties

				#region StackedBarSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedBarSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("StackedBarSeries_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("StackedBarSeries_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);

				#endregion // StackedBarSeries Properties

				#region ScaleLegend Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ScaleLegend");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDataChartSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDataChartSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentPresenter",
					new DescriptionAttribute(SR.GetString("ScaleLegend_ContentPresenter_Property")),
				    new DisplayNameAttribute("ContentPresenter")				);


				tableBuilder.AddCustomAttributes(t, "LegendScaleElement",
					new DescriptionAttribute(SR.GetString("ScaleLegend_LegendScaleElement_Property")),
				    new DisplayNameAttribute("LegendScaleElement")				);


				tableBuilder.AddCustomAttributes(t, "MinText",
					new DescriptionAttribute(SR.GetString("ScaleLegend_MinText_Property")),
				    new DisplayNameAttribute("MinText")				);


				tableBuilder.AddCustomAttributes(t, "MaxText",
					new DescriptionAttribute(SR.GetString("ScaleLegend_MaxText_Property")),
				    new DisplayNameAttribute("MaxText")				);

				#endregion // ScaleLegend Properties

				#region LegendBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.LegendBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Children",
					new DescriptionAttribute(SR.GetString("LegendBase_Children_Property")),
				    new DisplayNameAttribute("Children")				);


				tableBuilder.AddCustomAttributes(t, "ContentPresenter",
					new DescriptionAttribute(SR.GetString("LegendBase_ContentPresenter_Property")),
				    new DisplayNameAttribute("ContentPresenter")				);

				#endregion // LegendBase Properties

				#region ValueBrushScale Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ValueBrushScale");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MinimumValue",
					new DescriptionAttribute(SR.GetString("ValueBrushScale_MinimumValue_Property")),
				    new DisplayNameAttribute("MinimumValue")				);


				tableBuilder.AddCustomAttributes(t, "MaximumValue",
					new DescriptionAttribute(SR.GetString("ValueBrushScale_MaximumValue_Property")),
				    new DisplayNameAttribute("MaximumValue")				);


				tableBuilder.AddCustomAttributes(t, "IsLogarithmic",
					new DescriptionAttribute(SR.GetString("ValueBrushScale_IsLogarithmic_Property")),
				    new DisplayNameAttribute("IsLogarithmic")				);


				tableBuilder.AddCustomAttributes(t, "LogarithmBase",
					new DescriptionAttribute(SR.GetString("ValueBrushScale_LogarithmBase_Property")),
				    new DisplayNameAttribute("LogarithmBase")				);

				#endregion // ValueBrushScale Properties

				#region LegendAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AutomationPeers.LegendAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LegendAutomationPeer Properties

				#region StackedSeriesCreatedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedSeriesCreatedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Brush",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_Brush_Property")),
				    new DisplayNameAttribute("Brush")				);


				tableBuilder.AddCustomAttributes(t, "LegendItemTemplate",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_LegendItemTemplate_Property")),
				    new DisplayNameAttribute("LegendItemTemplate")				);


				tableBuilder.AddCustomAttributes(t, "LegendItemBadgeTemplate",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_LegendItemBadgeTemplate_Property")),
				    new DisplayNameAttribute("LegendItemBadgeTemplate")				);


				tableBuilder.AddCustomAttributes(t, "LegendItemVisibility",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_LegendItemVisibility_Property")),
				    new DisplayNameAttribute("LegendItemVisibility")				);


				tableBuilder.AddCustomAttributes(t, "Outline",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_Outline_Property")),
				    new DisplayNameAttribute("Outline")				);


				tableBuilder.AddCustomAttributes(t, "DashArray",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_DashArray_Property")),
				    new DisplayNameAttribute("DashArray")				);


				tableBuilder.AddCustomAttributes(t, "DashCap",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_DashCap_Property")),
				    new DisplayNameAttribute("DashCap")				);


				tableBuilder.AddCustomAttributes(t, "Thickness",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_Thickness_Property")),
				    new DisplayNameAttribute("Thickness")				);


				tableBuilder.AddCustomAttributes(t, "Title",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_Title_Property")),
				    new DisplayNameAttribute("Title")				);


				tableBuilder.AddCustomAttributes(t, "ToolTip",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_ToolTip_Property")),
				    new DisplayNameAttribute("ToolTip")				);


				tableBuilder.AddCustomAttributes(t, "MarkerBrush",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_MarkerBrush_Property")),
				    new DisplayNameAttribute("MarkerBrush")				);


				tableBuilder.AddCustomAttributes(t, "MarkerOutline",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_MarkerOutline_Property")),
				    new DisplayNameAttribute("MarkerOutline")				);


				tableBuilder.AddCustomAttributes(t, "MarkerStyle",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_MarkerStyle_Property")),
				    new DisplayNameAttribute("MarkerStyle")				);


				tableBuilder.AddCustomAttributes(t, "MarkerTemplate",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_MarkerTemplate_Property")),
				    new DisplayNameAttribute("MarkerTemplate")				);


				tableBuilder.AddCustomAttributes(t, "MarkerType",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_MarkerType_Property")),
				    new DisplayNameAttribute("MarkerType")				);


				tableBuilder.AddCustomAttributes(t, "Index",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_Index_Property")),
				    new DisplayNameAttribute("Index")				);


				tableBuilder.AddCustomAttributes(t, "StartCap",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_StartCap_Property")),
				    new DisplayNameAttribute("StartCap")				);


				tableBuilder.AddCustomAttributes(t, "EndCap",
					new DescriptionAttribute(SR.GetString("StackedSeriesCreatedEventArgs_EndCap_Property")),
				    new DisplayNameAttribute("EndCap")				);

				#endregion // StackedSeriesCreatedEventArgs Properties

				#region CategoryYAxis Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CategoryYAxis");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Interval",
					new DescriptionAttribute(SR.GetString("CategoryYAxis_Interval_Property")),
				    new DisplayNameAttribute("Interval")				);

				#endregion // CategoryYAxis Properties

				#region Stacked100ColumnSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Stacked100ColumnSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Stacked100ColumnSeries Properties

				#region ItemLegend Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ItemLegend");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDataChartSupportingControlsAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDataChartSupportingControlsAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentPresenter",
					new DescriptionAttribute(SR.GetString("ItemLegend_ContentPresenter_Property")),
				    new DisplayNameAttribute("ContentPresenter")				);

				#endregion // ItemLegend Properties

				#region VerticalAxisLabelPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VerticalAxisLabelPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // VerticalAxisLabelPanel Properties

				#region AngleAxisLabelPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AngleAxisLabelPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ClipLabelsToBounds",
					new DescriptionAttribute(SR.GetString("AngleAxisLabelPanel_ClipLabelsToBounds_Property")),
				    new DisplayNameAttribute("ClipLabelsToBounds")				);


				tableBuilder.AddCustomAttributes(t, "GetPoint",
					new DescriptionAttribute(SR.GetString("AngleAxisLabelPanel_GetPoint_Property")),
				    new DisplayNameAttribute("GetPoint")				);

				#endregion // AngleAxisLabelPanel Properties

				#region BarSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.BarSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("BarSeries_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("BarSeries_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);

				#endregion // BarSeries Properties

				#region XamDataChartAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AutomationPeers.XamDataChartAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamDataChartAutomationPeer Properties

				#region SliceAppearance Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SliceAppearance");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Offset",
					new DescriptionAttribute(SR.GetString("SliceAppearance_Offset_Property")),
				    new DisplayNameAttribute("Offset")				);


				tableBuilder.AddCustomAttributes(t, "UpperLeft",
					new DescriptionAttribute(SR.GetString("SliceAppearance_UpperLeft_Property")),
				    new DisplayNameAttribute("UpperLeft")				);


				tableBuilder.AddCustomAttributes(t, "BezierPoints",
					new DescriptionAttribute(SR.GetString("SliceAppearance_BezierPoints_Property")),
				    new DisplayNameAttribute("BezierPoints")				);


				tableBuilder.AddCustomAttributes(t, "RightBezierPoints",
					new DescriptionAttribute(SR.GetString("SliceAppearance_RightBezierPoints_Property")),
				    new DisplayNameAttribute("RightBezierPoints")				);


				tableBuilder.AddCustomAttributes(t, "Points",
					new DescriptionAttribute(SR.GetString("SliceAppearance_Points_Property")),
				    new DisplayNameAttribute("Points")				);


				tableBuilder.AddCustomAttributes(t, "UpperRight",
					new DescriptionAttribute(SR.GetString("SliceAppearance_UpperRight_Property")),
				    new DisplayNameAttribute("UpperRight")				);


				tableBuilder.AddCustomAttributes(t, "LowerRight",
					new DescriptionAttribute(SR.GetString("SliceAppearance_LowerRight_Property")),
				    new DisplayNameAttribute("LowerRight")				);


				tableBuilder.AddCustomAttributes(t, "LowerLeft",
					new DescriptionAttribute(SR.GetString("SliceAppearance_LowerLeft_Property")),
				    new DisplayNameAttribute("LowerLeft")				);


				tableBuilder.AddCustomAttributes(t, "Style",
					new DescriptionAttribute(SR.GetString("SliceAppearance_Style_Property")),
				    new DisplayNameAttribute("Style")				);


				tableBuilder.AddCustomAttributes(t, "Fill",
					new DescriptionAttribute(SR.GetString("SliceAppearance_Fill_Property")),
				    new DisplayNameAttribute("Fill")				);


				tableBuilder.AddCustomAttributes(t, "Outline",
					new DescriptionAttribute(SR.GetString("SliceAppearance_Outline_Property")),
				    new DisplayNameAttribute("Outline")				);


				tableBuilder.AddCustomAttributes(t, "InnerLabel",
					new DescriptionAttribute(SR.GetString("SliceAppearance_InnerLabel_Property")),
				    new DisplayNameAttribute("InnerLabel")				);


				tableBuilder.AddCustomAttributes(t, "InnerLabelPosition",
					new DescriptionAttribute(SR.GetString("SliceAppearance_InnerLabelPosition_Property")),
				    new DisplayNameAttribute("InnerLabelPosition")				);


				tableBuilder.AddCustomAttributes(t, "HasInnerLabel",
					new DescriptionAttribute(SR.GetString("SliceAppearance_HasInnerLabel_Property")),
				    new DisplayNameAttribute("HasInnerLabel")				);


				tableBuilder.AddCustomAttributes(t, "InnerLabelTemplate",
					new DescriptionAttribute(SR.GetString("SliceAppearance_InnerLabelTemplate_Property")),
				    new DisplayNameAttribute("InnerLabelTemplate")				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("SliceAppearance_Item_Property")),
				    new DisplayNameAttribute("Item")				);


				tableBuilder.AddCustomAttributes(t, "Index",
					new DescriptionAttribute(SR.GetString("SliceAppearance_Index_Property")),
				    new DisplayNameAttribute("Index")				);

				#endregion // SliceAppearance Properties

				#region XamFunnelView Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.XamFunnelView");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ToolTip",
					new DescriptionAttribute(SR.GetString("XamFunnelView_ToolTip_Property")),
				    new DisplayNameAttribute("ToolTip"),
					new CategoryAttribute(SR.GetString("XamFunnelView_Properties"))
				);

				#endregion // XamFunnelView Properties

				#region ScatterErrorBarSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ScatterErrorBarSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "EnableErrorBarsHorizontal",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_EnableErrorBarsHorizontal_Property")),
				    new DisplayNameAttribute("EnableErrorBarsHorizontal")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalCalculator",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_HorizontalCalculator_Property")),
				    new DisplayNameAttribute("HorizontalCalculator")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalStroke",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_HorizontalStroke_Property")),
				    new DisplayNameAttribute("HorizontalStroke")				);


				tableBuilder.AddCustomAttributes(t, "EnableErrorBarsVertical",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_EnableErrorBarsVertical_Property")),
				    new DisplayNameAttribute("EnableErrorBarsVertical")				);


				tableBuilder.AddCustomAttributes(t, "VerticalCalculator",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_VerticalCalculator_Property")),
				    new DisplayNameAttribute("VerticalCalculator")				);


				tableBuilder.AddCustomAttributes(t, "VerticalStroke",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_VerticalStroke_Property")),
				    new DisplayNameAttribute("VerticalStroke")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalStrokeThickness",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_HorizontalStrokeThickness_Property")),
				    new DisplayNameAttribute("HorizontalStrokeThickness")				);


				tableBuilder.AddCustomAttributes(t, "VerticalStrokeThickness",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_VerticalStrokeThickness_Property")),
				    new DisplayNameAttribute("VerticalStrokeThickness")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalCalculatorReference",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_HorizontalCalculatorReference_Property")),
				    new DisplayNameAttribute("HorizontalCalculatorReference")				);


				tableBuilder.AddCustomAttributes(t, "VerticalCalculatorReference",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_VerticalCalculatorReference_Property")),
				    new DisplayNameAttribute("VerticalCalculatorReference")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalErrorBarStyle",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_HorizontalErrorBarStyle_Property")),
				    new DisplayNameAttribute("HorizontalErrorBarStyle")				);


				tableBuilder.AddCustomAttributes(t, "VerticalErrorBarStyle",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_VerticalErrorBarStyle_Property")),
				    new DisplayNameAttribute("VerticalErrorBarStyle")				);


				tableBuilder.AddCustomAttributes(t, "HorizontalErrorBarCapLength",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_HorizontalErrorBarCapLength_Property")),
				    new DisplayNameAttribute("HorizontalErrorBarCapLength")				);


				tableBuilder.AddCustomAttributes(t, "VerticalErrorBarCapLength",
					new DescriptionAttribute(SR.GetString("ScatterErrorBarSettings_VerticalErrorBarCapLength_Property")),
				    new DisplayNameAttribute("VerticalErrorBarCapLength")				);

				#endregion // ScatterErrorBarSettings Properties

				#region XamFunnelSlice Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.XamFunnelSlice");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "StrokeThickness",
					new DescriptionAttribute(SR.GetString("XamFunnelSlice_StrokeThickness_Property")),
				    new DisplayNameAttribute("StrokeThickness"),
					new CategoryAttribute(SR.GetString("XamFunnelSlice_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Fill",
					new DescriptionAttribute(SR.GetString("XamFunnelSlice_Fill_Property")),
				    new DisplayNameAttribute("Fill"),
					new CategoryAttribute(SR.GetString("XamFunnelSlice_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Outline",
					new DescriptionAttribute(SR.GetString("XamFunnelSlice_Outline_Property")),
				    new DisplayNameAttribute("Outline"),
					new CategoryAttribute(SR.GetString("XamFunnelSlice_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActualFill",
					new DescriptionAttribute(SR.GetString("XamFunnelSlice_ActualFill_Property")),
				    new DisplayNameAttribute("ActualFill"),
					new CategoryAttribute(SR.GetString("XamFunnelSlice_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActualOutline",
					new DescriptionAttribute(SR.GetString("XamFunnelSlice_ActualOutline_Property")),
				    new DisplayNameAttribute("ActualOutline"),
					new CategoryAttribute(SR.GetString("XamFunnelSlice_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LabelVisibility",
					new DescriptionAttribute(SR.GetString("XamFunnelSlice_LabelVisibility_Property")),
				    new DisplayNameAttribute("LabelVisibility"),
					new CategoryAttribute(SR.GetString("XamFunnelSlice_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Owner",
					new DescriptionAttribute(SR.GetString("XamFunnelSlice_Owner_Property")),
				    new DisplayNameAttribute("Owner"),
					new CategoryAttribute(SR.GetString("XamFunnelSlice_Properties"))
				);

				#endregion // XamFunnelSlice Properties

				#region FunnelSliceClickedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FunnelSliceClickedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Index",
					new DescriptionAttribute(SR.GetString("FunnelSliceClickedEventArgs_Index_Property")),
				    new DisplayNameAttribute("Index")				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("FunnelSliceClickedEventArgs_Item_Property")),
				    new DisplayNameAttribute("Item")				);

				#endregion // FunnelSliceClickedEventArgs Properties

				#region FunnelSliceClickedEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FunnelSliceClickedEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FunnelSliceClickedEventHandler Properties

				#region SortedListView`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.SortedListView`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsFixedSize",
					new DescriptionAttribute(SR.GetString("SortedListView`1_IsFixedSize_Property")),
				    new DisplayNameAttribute("IsFixedSize")				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("SortedListView`1_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("SortedListView`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "IsSynchronized",
					new DescriptionAttribute(SR.GetString("SortedListView`1_IsSynchronized_Property")),
				    new DisplayNameAttribute("IsSynchronized")				);


				tableBuilder.AddCustomAttributes(t, "SyncRoot",
					new DescriptionAttribute(SR.GetString("SortedListView`1_SyncRoot_Property")),
				    new DisplayNameAttribute("SyncRoot")				);

				#endregion // SortedListView`1 Properties

				#region FunnelDataContext Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FunnelDataContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("FunnelDataContext_Item_Property")),
				    new DisplayNameAttribute("Item")				);


				tableBuilder.AddCustomAttributes(t, "Index",
					new DescriptionAttribute(SR.GetString("FunnelDataContext_Index_Property")),
				    new DisplayNameAttribute("Index")				);

				#endregion // FunnelDataContext Properties

				#region SliceClickEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SliceClickEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("SliceClickEventArgs_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "IsExploded",
					new DescriptionAttribute(SR.GetString("SliceClickEventArgs_IsExploded_Property")),
				    new DisplayNameAttribute("IsExploded")				);


				tableBuilder.AddCustomAttributes(t, "IsOthersSlice",
					new DescriptionAttribute(SR.GetString("SliceClickEventArgs_IsOthersSlice_Property")),
				    new DisplayNameAttribute("IsOthersSlice")				);


				tableBuilder.AddCustomAttributes(t, "DataContext",
					new DescriptionAttribute(SR.GetString("SliceClickEventArgs_DataContext_Property")),
				    new DisplayNameAttribute("DataContext")				);

				#endregion // SliceClickEventArgs Properties

				#region XamPieChart Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.XamPieChart");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDataChartAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDataChartAssetLibrary"))
				);

				#endregion // XamPieChart Properties

				#region XamFunnelChart Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.XamFunnelChart");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDataChartAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDataChartAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemsSource",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_ItemsSource_Property")),
				    new DisplayNameAttribute("ItemsSource"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ValueMemberPath",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_ValueMemberPath_Property")),
				    new DisplayNameAttribute("ValueMemberPath"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Brushes",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_Brushes_Property")),
				    new DisplayNameAttribute("Brushes"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Outlines",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_Outlines_Property")),
				    new DisplayNameAttribute("Outlines"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "BottomEdgeWidth",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_BottomEdgeWidth_Property")),
				    new DisplayNameAttribute("BottomEdgeWidth"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InnerLabelMemberPath",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_InnerLabelMemberPath_Property")),
				    new DisplayNameAttribute("InnerLabelMemberPath"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OuterLabelMemberPath",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_OuterLabelMemberPath_Property")),
				    new DisplayNameAttribute("OuterLabelMemberPath"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InnerLabelVisibility",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_InnerLabelVisibility_Property")),
				    new DisplayNameAttribute("InnerLabelVisibility"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OuterLabelVisibility",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_OuterLabelVisibility_Property")),
				    new DisplayNameAttribute("OuterLabelVisibility"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OuterLabelAlignment",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_OuterLabelAlignment_Property")),
				    new DisplayNameAttribute("OuterLabelAlignment"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FunnelSliceDisplay",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_FunnelSliceDisplay_Property")),
				    new DisplayNameAttribute("FunnelSliceDisplay"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "InnerLabelTemplate",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_InnerLabelTemplate_Property")),
				    new DisplayNameAttribute("InnerLabelTemplate"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OuterLabelTemplate",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_OuterLabelTemplate_Property")),
				    new DisplayNameAttribute("OuterLabelTemplate"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TransitionDuration",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_TransitionDuration_Property")),
				    new DisplayNameAttribute("TransitionDuration"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsInverted",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_IsInverted_Property")),
				    new DisplayNameAttribute("IsInverted"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UpperBezierControlPoint",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_UpperBezierControlPoint_Property")),
				    new DisplayNameAttribute("UpperBezierControlPoint"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LowerBezierControlPoint",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_LowerBezierControlPoint_Property")),
				    new DisplayNameAttribute("LowerBezierControlPoint"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UseBezierCurve",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_UseBezierCurve_Property")),
				    new DisplayNameAttribute("UseBezierCurve"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowSliceSelection",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_AllowSliceSelection_Property")),
				    new DisplayNameAttribute("AllowSliceSelection"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UseUnselectedStyle",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_UseUnselectedStyle_Property")),
				    new DisplayNameAttribute("UseUnselectedStyle"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedSliceStyle",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_SelectedSliceStyle_Property")),
				    new DisplayNameAttribute("SelectedSliceStyle"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UnselectedSliceStyle",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_UnselectedSliceStyle_Property")),
				    new DisplayNameAttribute("UnselectedSliceStyle"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ToolTip",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_ToolTip_Property")),
				    new DisplayNameAttribute("ToolTip"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedItems",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_SelectedItems_Property")),
				    new DisplayNameAttribute("SelectedItems"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Legend",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_Legend_Property")),
				    new DisplayNameAttribute("Legend"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LegendItemTemplate",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_LegendItemTemplate_Property")),
				    new DisplayNameAttribute("LegendItemTemplate"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LegendItemBadgeTemplate",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_LegendItemBadgeTemplate_Property")),
				    new DisplayNameAttribute("LegendItemBadgeTemplate"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UseOuterLabelsForLegend",
					new DescriptionAttribute(SR.GetString("XamFunnelChart_UseOuterLabelsForLegend_Property")),
				    new DisplayNameAttribute("UseOuterLabelsForLegend"),
					new CategoryAttribute(SR.GetString("XamFunnelChart_Properties"))
				);

				#endregion // XamFunnelChart Properties

				#region SyncLink Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SyncLink");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Charts",
					new DescriptionAttribute(SR.GetString("SyncLink_Charts_Property")),
				    new DisplayNameAttribute("Charts")				);


				tableBuilder.AddCustomAttributes(t, "SyncChannel",
					new DescriptionAttribute(SR.GetString("SyncLink_SyncChannel_Property")),
				    new DisplayNameAttribute("SyncChannel")				);

				#endregion // SyncLink Properties

				#region SliceClickEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SliceClickEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SliceClickEventHandler Properties

				#region PieSliceDataContext Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PieSliceDataContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Slice",
					new DescriptionAttribute(SR.GetString("PieSliceDataContext_Slice_Property")),
				    new DisplayNameAttribute("Slice")				);


				tableBuilder.AddCustomAttributes(t, "PercentValue",
					new DescriptionAttribute(SR.GetString("PieSliceDataContext_PercentValue_Property")),
				    new DisplayNameAttribute("PercentValue")				);

				#endregion // PieSliceDataContext Properties

				#region IndexCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.IndexCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // IndexCollection Properties

				#region IndexCollectionTypeConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.IndexCollectionTypeConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // IndexCollectionTypeConverter Properties

				#region SliceInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SliceInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SliceInfo Properties

				#region StackedSeriesCreatedEventHandler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedSeriesCreatedEventHandler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StackedSeriesCreatedEventHandler Properties

				#region SeriesView Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SeriesView");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SeriesView Properties

				#region SeriesViewer Properties
				t = controlAssembly.GetType("Infragistics.Controls.SeriesViewer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Series",
					new DescriptionAttribute(SR.GetString("SeriesViewer_Series_Property")),
				    new DisplayNameAttribute("Series"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CrosshairPoint",
					new DescriptionAttribute(SR.GetString("SeriesViewer_CrosshairPoint_Property")),
				    new DisplayNameAttribute("CrosshairPoint"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "Legend",
					new DescriptionAttribute(SR.GetString("SeriesViewer_Legend_Property")),
				    new DisplayNameAttribute("Legend"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalZoombar",
					new DescriptionAttribute(SR.GetString("SeriesViewer_HorizontalZoombar_Property")),
				    new DisplayNameAttribute("HorizontalZoombar"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "VerticalZoombar",
					new DescriptionAttribute(SR.GetString("SeriesViewer_VerticalZoombar_Property")),
				    new DisplayNameAttribute("VerticalZoombar"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "WindowRect",
					new DescriptionAttribute(SR.GetString("SeriesViewer_WindowRect_Property")),
				    new DisplayNameAttribute("WindowRect"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalZoomable",
					new DescriptionAttribute(SR.GetString("SeriesViewer_HorizontalZoomable_Property")),
				    new DisplayNameAttribute("HorizontalZoomable"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalZoombarVisibility",
					new DescriptionAttribute(SR.GetString("SeriesViewer_HorizontalZoombarVisibility_Property")),
				    new DisplayNameAttribute("HorizontalZoombarVisibility"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalZoomable",
					new DescriptionAttribute(SR.GetString("SeriesViewer_VerticalZoomable_Property")),
				    new DisplayNameAttribute("VerticalZoomable"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalZoombarVisibility",
					new DescriptionAttribute(SR.GetString("SeriesViewer_VerticalZoombarVisibility_Property")),
				    new DisplayNameAttribute("VerticalZoombarVisibility"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActualSyncLink",
					new DescriptionAttribute(SR.GetString("SeriesViewer_ActualSyncLink_Property")),
				    new DisplayNameAttribute("ActualSyncLink"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "CrosshairVisibility",
					new DescriptionAttribute(SR.GetString("SeriesViewer_CrosshairVisibility_Property")),
				    new DisplayNameAttribute("CrosshairVisibility"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaBorderBrush",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PlotAreaBorderBrush_Property")),
				    new DisplayNameAttribute("PlotAreaBorderBrush"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaBorderThickness",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PlotAreaBorderThickness_Property")),
				    new DisplayNameAttribute("PlotAreaBorderThickness"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaBackground",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PlotAreaBackground_Property")),
				    new DisplayNameAttribute("PlotAreaBackground"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaMinWidth",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PlotAreaMinWidth_Property")),
				    new DisplayNameAttribute("PlotAreaMinWidth"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaMinHeight",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PlotAreaMinHeight_Property")),
				    new DisplayNameAttribute("PlotAreaMinHeight"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DefaultInteraction",
					new DescriptionAttribute(SR.GetString("SeriesViewer_DefaultInteraction_Property")),
				    new DisplayNameAttribute("DefaultInteraction"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DragModifier",
					new DescriptionAttribute(SR.GetString("SeriesViewer_DragModifier_Property")),
				    new DisplayNameAttribute("DragModifier"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PanModifier",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PanModifier_Property")),
				    new DisplayNameAttribute("PanModifier"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviewRect",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PreviewRect_Property")),
				    new DisplayNameAttribute("PreviewRect"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "WindowPositionHorizontal",
					new DescriptionAttribute(SR.GetString("SeriesViewer_WindowPositionHorizontal_Property")),
				    new DisplayNameAttribute("WindowPositionHorizontal"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "WindowPositionVertical",
					new DescriptionAttribute(SR.GetString("SeriesViewer_WindowPositionVertical_Property")),
				    new DisplayNameAttribute("WindowPositionVertical"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "WindowScaleHorizontal",
					new DescriptionAttribute(SR.GetString("SeriesViewer_WindowScaleHorizontal_Property")),
				    new DisplayNameAttribute("WindowScaleHorizontal"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "WindowScaleVertical",
					new DescriptionAttribute(SR.GetString("SeriesViewer_WindowScaleVertical_Property")),
				    new DisplayNameAttribute("WindowScaleVertical"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "ContentPresenter",
					new DescriptionAttribute(SR.GetString("SeriesViewer_ContentPresenter_Property")),
				    new DisplayNameAttribute("ContentPresenter"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "CrosshairLineStyle",
					new DescriptionAttribute(SR.GetString("SeriesViewer_CrosshairLineStyle_Property")),
				    new DisplayNameAttribute("CrosshairLineStyle"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreviewPathStyle",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PreviewPathStyle_Property")),
				    new DisplayNameAttribute("PreviewPathStyle"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ToolTipStyle",
					new DescriptionAttribute(SR.GetString("SeriesViewer_ToolTipStyle_Property")),
				    new DisplayNameAttribute("ToolTipStyle"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ZoombarStyle",
					new DescriptionAttribute(SR.GetString("SeriesViewer_ZoombarStyle_Property")),
				    new DisplayNameAttribute("ZoombarStyle"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CircleMarkerTemplate",
					new DescriptionAttribute(SR.GetString("SeriesViewer_CircleMarkerTemplate_Property")),
				    new DisplayNameAttribute("CircleMarkerTemplate")				);


				tableBuilder.AddCustomAttributes(t, "TriangleMarkerTemplate",
					new DescriptionAttribute(SR.GetString("SeriesViewer_TriangleMarkerTemplate_Property")),
				    new DisplayNameAttribute("TriangleMarkerTemplate"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "PyramidMarkerTemplate",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PyramidMarkerTemplate_Property")),
				    new DisplayNameAttribute("PyramidMarkerTemplate"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "SquareMarkerTemplate",
					new DescriptionAttribute(SR.GetString("SeriesViewer_SquareMarkerTemplate_Property")),
				    new DisplayNameAttribute("SquareMarkerTemplate"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "DiamondMarkerTemplate",
					new DescriptionAttribute(SR.GetString("SeriesViewer_DiamondMarkerTemplate_Property")),
				    new DisplayNameAttribute("DiamondMarkerTemplate"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "PentagonMarkerTemplate",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PentagonMarkerTemplate_Property")),
				    new DisplayNameAttribute("PentagonMarkerTemplate"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "HexagonMarkerTemplate",
					new DescriptionAttribute(SR.GetString("SeriesViewer_HexagonMarkerTemplate_Property")),
				    new DisplayNameAttribute("HexagonMarkerTemplate"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "TetragramMarkerTemplate",
					new DescriptionAttribute(SR.GetString("SeriesViewer_TetragramMarkerTemplate_Property")),
				    new DisplayNameAttribute("TetragramMarkerTemplate"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "PentagramMarkerTemplate",
					new DescriptionAttribute(SR.GetString("SeriesViewer_PentagramMarkerTemplate_Property")),
				    new DisplayNameAttribute("PentagramMarkerTemplate"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "HexagramMarkerTemplate",
					new DescriptionAttribute(SR.GetString("SeriesViewer_HexagramMarkerTemplate_Property")),
				    new DisplayNameAttribute("HexagramMarkerTemplate"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "WindowRectMinWidth",
					new DescriptionAttribute(SR.GetString("SeriesViewer_WindowRectMinWidth_Property")),
				    new DisplayNameAttribute("WindowRectMinWidth"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "ActualWindowPositionHorizontal",
					new DescriptionAttribute(SR.GetString("SeriesViewer_ActualWindowPositionHorizontal_Property")),
				    new DisplayNameAttribute("ActualWindowPositionHorizontal"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "ActualWindowPositionVertical",
					new DescriptionAttribute(SR.GetString("SeriesViewer_ActualWindowPositionVertical_Property")),
				    new DisplayNameAttribute("ActualWindowPositionVertical"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "ActualWindowScaleHorizontal",
					new DescriptionAttribute(SR.GetString("SeriesViewer_ActualWindowScaleHorizontal_Property")),
				    new DisplayNameAttribute("ActualWindowScaleHorizontal"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "ActualWindowScaleVertical",
					new DescriptionAttribute(SR.GetString("SeriesViewer_ActualWindowScaleVertical_Property")),
				    new DisplayNameAttribute("ActualWindowScaleVertical"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "ViewportRect",
					new DescriptionAttribute(SR.GetString("SeriesViewer_ViewportRect_Property")),
				    new DisplayNameAttribute("ViewportRect"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "WindowResponse",
					new DescriptionAttribute(SR.GetString("SeriesViewer_WindowResponse_Property")),
				    new DisplayNameAttribute("WindowResponse"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SyncChannel",
					new DescriptionAttribute(SR.GetString("SeriesViewer_SyncChannel_Property")),
				    new DisplayNameAttribute("SyncChannel"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OverviewPlusDetailPaneVisibility",
					new DescriptionAttribute(SR.GetString("SeriesViewer_OverviewPlusDetailPaneVisibility_Property")),
				    new DisplayNameAttribute("OverviewPlusDetailPaneVisibility"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OverviewPlusDetailPaneStyle",
					new DescriptionAttribute(SR.GetString("SeriesViewer_OverviewPlusDetailPaneStyle_Property")),
				    new DisplayNameAttribute("OverviewPlusDetailPaneStyle"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OverviewPlusDetailPaneHorizontalAlignment",
					new DescriptionAttribute(SR.GetString("SeriesViewer_OverviewPlusDetailPaneHorizontalAlignment_Property")),
				    new DisplayNameAttribute("OverviewPlusDetailPaneHorizontalAlignment"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OverviewPlusDetailPaneVerticalAlignment",
					new DescriptionAttribute(SR.GetString("SeriesViewer_OverviewPlusDetailPaneVerticalAlignment_Property")),
				    new DisplayNameAttribute("OverviewPlusDetailPaneVerticalAlignment"),
					new CategoryAttribute(SR.GetString("SeriesViewer_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActualWindowRect",
					new DescriptionAttribute(SR.GetString("SeriesViewer_ActualWindowRect_Property")),
				    new DisplayNameAttribute("ActualWindowRect"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "EffectiveViewport",
					new DescriptionAttribute(SR.GetString("SeriesViewer_EffectiveViewport_Property")),
				    new DisplayNameAttribute("EffectiveViewport"),
				    BrowsableAttribute.No				);

				#endregion // SeriesViewer Properties

				#region ErrorBarSettingsBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ErrorBarSettingsBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ErrorBarSettingsBase Properties

				#region StackedSeriesCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedSeriesCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StackedSeriesCollection Properties

				#region SeriesViewerView Properties
				t = controlAssembly.GetType("Infragistics.Controls.SeriesViewerView");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalZoombar",
					new DescriptionAttribute(SR.GetString("SeriesViewerView_HorizontalZoombar_Property")),
				    new DisplayNameAttribute("HorizontalZoombar")				);


				tableBuilder.AddCustomAttributes(t, "VerticalZoombar",
					new DescriptionAttribute(SR.GetString("SeriesViewerView_VerticalZoombar_Property")),
				    new DisplayNameAttribute("VerticalZoombar")				);


				tableBuilder.AddCustomAttributes(t, "PlotAreaViewport",
					new DescriptionAttribute(SR.GetString("SeriesViewerView_PlotAreaViewport_Property")),
				    new DisplayNameAttribute("PlotAreaViewport")				);


				tableBuilder.AddCustomAttributes(t, "CurrentModifiers",
					new DescriptionAttribute(SR.GetString("SeriesViewerView_CurrentModifiers_Property")),
				    new DisplayNameAttribute("CurrentModifiers")				);

				#endregion // SeriesViewerView Properties

				#region PointSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PointSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PointSeries Properties

				#region CategoryErrorBarSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.CategoryErrorBarSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "EnableErrorBars",
					new DescriptionAttribute(SR.GetString("CategoryErrorBarSettings_EnableErrorBars_Property")),
				    new DisplayNameAttribute("EnableErrorBars")				);


				tableBuilder.AddCustomAttributes(t, "Calculator",
					new DescriptionAttribute(SR.GetString("CategoryErrorBarSettings_Calculator_Property")),
				    new DisplayNameAttribute("Calculator")				);


				tableBuilder.AddCustomAttributes(t, "Stroke",
					new DescriptionAttribute(SR.GetString("CategoryErrorBarSettings_Stroke_Property")),
				    new DisplayNameAttribute("Stroke")				);


				tableBuilder.AddCustomAttributes(t, "StrokeThickness",
					new DescriptionAttribute(SR.GetString("CategoryErrorBarSettings_StrokeThickness_Property")),
				    new DisplayNameAttribute("StrokeThickness")				);


				tableBuilder.AddCustomAttributes(t, "ErrorBarStyle",
					new DescriptionAttribute(SR.GetString("CategoryErrorBarSettings_ErrorBarStyle_Property")),
				    new DisplayNameAttribute("ErrorBarStyle")				);


				tableBuilder.AddCustomAttributes(t, "ErrorBarCapLength",
					new DescriptionAttribute(SR.GetString("CategoryErrorBarSettings_ErrorBarCapLength_Property")),
				    new DisplayNameAttribute("ErrorBarCapLength")				);

				#endregion // CategoryErrorBarSettings Properties

				#region PointList Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.PointList");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PointList Properties

				#region StackedSplineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedSplineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StackedSplineSeries Properties

				#region Stacked100LineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Stacked100LineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Stacked100LineSeries Properties

				#region SupportingCalculationStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SupportingCalculationStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SupportingCalculationStrategy Properties

				#region ProvideColumnValuesStrategy Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ProvideColumnValuesStrategy");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ProvideColumnValuesStrategy Properties

				#region DoubleColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.DoubleColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Values",
					new DescriptionAttribute(SR.GetString("DoubleColumn_Values_Property")),
				    new DisplayNameAttribute("Values")				);

				#endregion // DoubleColumn Properties

				#region StringColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.StringColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Values",
					new DescriptionAttribute(SR.GetString("StringColumn_Values_Property")),
				    new DisplayNameAttribute("Values")				);

				#endregion // StringColumn Properties

				#region ObjectColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.ObjectColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Values",
					new DescriptionAttribute(SR.GetString("ObjectColumn_Values_Property")),
				    new DisplayNameAttribute("Values")				);

				#endregion // ObjectColumn Properties

				#region IntColumn Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.IntColumn");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Values",
					new DescriptionAttribute(SR.GetString("IntColumn_Values_Property")),
				    new DisplayNameAttribute("Values")				);

				#endregion // IntColumn Properties

				#region IntColumnComparison Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Util.IntColumnComparison");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // IntColumnComparison Properties

				#region StackedSplineAreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedSplineAreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StackedSplineAreaSeries Properties

				#region StackedAreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedAreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StackedAreaSeries Properties

				#region Stacked100SplineAreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Stacked100SplineAreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Stacked100SplineAreaSeries Properties

				#region RenderSurface Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.RenderSurface");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Surface",
					new DescriptionAttribute(SR.GetString("RenderSurface_Surface_Property")),
				    new DisplayNameAttribute("Surface")				);

				#endregion // RenderSurface Properties

				#region Stacked100SplineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Stacked100SplineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Stacked100SplineSeries Properties

				#region Stacked100AreaSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.Stacked100AreaSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // Stacked100AreaSeries Properties

				#region HorizontalAnchoredCategorySeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.HorizontalAnchoredCategorySeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "XAxis",
					new DescriptionAttribute(SR.GetString("HorizontalAnchoredCategorySeries_XAxis_Property")),
				    new DisplayNameAttribute("XAxis")				);


				tableBuilder.AddCustomAttributes(t, "YAxis",
					new DescriptionAttribute(SR.GetString("HorizontalAnchoredCategorySeries_YAxis_Property")),
				    new DisplayNameAttribute("YAxis")				);

				#endregion // HorizontalAnchoredCategorySeries Properties

				#region LogarithmicTickmarkValues Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.LogarithmicTickmarkValues");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LogarithmBase",
					new DescriptionAttribute(SR.GetString("LogarithmicTickmarkValues_LogarithmBase_Property")),
				    new DisplayNameAttribute("LogarithmBase")				);

				#endregion // LogarithmicTickmarkValues Properties

				#region TickmarkValues Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.TickmarkValues");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TickmarkValues Properties

				#region LinearTickmarkValues Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.LinearTickmarkValues");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LinearTickmarkValues Properties

				#region HorizontalLinearScaler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.HorizontalLinearScaler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HorizontalLinearScaler Properties

				#region LinearScaler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.LinearScaler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LinearScaler Properties

				#region NumericScaler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.NumericScaler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NumericScaler Properties

				#region StackedLineSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StackedLineSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // StackedLineSeries Properties

				#region HorizontalStackedSeriesBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.HorizontalStackedSeriesBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "XAxis",
					new DescriptionAttribute(SR.GetString("HorizontalStackedSeriesBase_XAxis_Property")),
				    new DisplayNameAttribute("XAxis")				);


				tableBuilder.AddCustomAttributes(t, "YAxis",
					new DescriptionAttribute(SR.GetString("HorizontalStackedSeriesBase_YAxis_Property")),
				    new DisplayNameAttribute("YAxis")				);

				#endregion // HorizontalStackedSeriesBase Properties

				#region ColumnFragment Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ColumnFragment");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("ColumnFragment_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("ColumnFragment_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);


				tableBuilder.AddCustomAttributes(t, "XAxis",
					new DescriptionAttribute(SR.GetString("ColumnFragment_XAxis_Property")),
				    new DisplayNameAttribute("XAxis")				);


				tableBuilder.AddCustomAttributes(t, "YAxis",
					new DescriptionAttribute(SR.GetString("ColumnFragment_YAxis_Property")),
				    new DisplayNameAttribute("YAxis")				);

				#endregion // ColumnFragment Properties

				#region ViewportCalculator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ViewportCalculator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ViewportCalculator Properties

				#region VerticalAnchoredCategorySeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VerticalAnchoredCategorySeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "XAxis",
					new DescriptionAttribute(SR.GetString("VerticalAnchoredCategorySeries_XAxis_Property")),
				    new DisplayNameAttribute("XAxis")				);


				tableBuilder.AddCustomAttributes(t, "YAxis",
					new DescriptionAttribute(SR.GetString("VerticalAnchoredCategorySeries_YAxis_Property")),
				    new DisplayNameAttribute("YAxis")				);

				#endregion // VerticalAnchoredCategorySeries Properties

				#region HorizontalLogarithmicScaler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.HorizontalLogarithmicScaler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HorizontalLogarithmicScaler Properties

				#region LogarithmicScaler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.LogarithmicScaler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LogarithmicScaler Properties

				#region AxisLabelPanelBaseView Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AxisLabelPanelBaseView");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AxisLabelPanelBaseView Properties

				#region DoubleCollectionDuplicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DoubleCollectionDuplicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DoubleCollectionDuplicator Properties

				#region HorizontalRangeCategorySeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.HorizontalRangeCategorySeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "XAxis",
					new DescriptionAttribute(SR.GetString("HorizontalRangeCategorySeries_XAxis_Property")),
				    new DisplayNameAttribute("XAxis")				);


				tableBuilder.AddCustomAttributes(t, "YAxis",
					new DescriptionAttribute(SR.GetString("HorizontalRangeCategorySeries_YAxis_Property")),
				    new DisplayNameAttribute("YAxis")				);

				#endregion // HorizontalRangeCategorySeries Properties

				#region VerticalStackedSeriesBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VerticalStackedSeriesBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "XAxis",
					new DescriptionAttribute(SR.GetString("VerticalStackedSeriesBase_XAxis_Property")),
				    new DisplayNameAttribute("XAxis")				);


				tableBuilder.AddCustomAttributes(t, "YAxis",
					new DescriptionAttribute(SR.GetString("VerticalStackedSeriesBase_YAxis_Property")),
				    new DisplayNameAttribute("YAxis")				);

				#endregion // VerticalStackedSeriesBase Properties

				#region StraightNumericAxisBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.StraightNumericAxisBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ScaleMode",
					new DescriptionAttribute(SR.GetString("StraightNumericAxisBase_ScaleMode_Property")),
				    new DisplayNameAttribute("ScaleMode")				);


				tableBuilder.AddCustomAttributes(t, "Scaler",
					new DescriptionAttribute(SR.GetString("StraightNumericAxisBase_Scaler_Property")),
				    new DisplayNameAttribute("Scaler")				);

				#endregion // StraightNumericAxisBase Properties

				#region VerticalLinearScaler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VerticalLinearScaler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // VerticalLinearScaler Properties

				#region DefaultFlattener Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.DefaultFlattener");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DefaultFlattener Properties

				#region VerticalLogarithmicScaler Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VerticalLogarithmicScaler");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // VerticalLogarithmicScaler Properties

				#region TickmarkValuesInitializationParameters Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.TickmarkValuesInitializationParameters");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "VisibleMinimum",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_VisibleMinimum_Property")),
				    new DisplayNameAttribute("VisibleMinimum")				);


				tableBuilder.AddCustomAttributes(t, "VisibleMaximum",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_VisibleMaximum_Property")),
				    new DisplayNameAttribute("VisibleMaximum")				);


				tableBuilder.AddCustomAttributes(t, "ActualMinimum",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_ActualMinimum_Property")),
				    new DisplayNameAttribute("ActualMinimum")				);


				tableBuilder.AddCustomAttributes(t, "ActualMaximum",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_ActualMaximum_Property")),
				    new DisplayNameAttribute("ActualMaximum")				);


				tableBuilder.AddCustomAttributes(t, "Resolution",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_Resolution_Property")),
				    new DisplayNameAttribute("Resolution")				);


				tableBuilder.AddCustomAttributes(t, "HasUserInterval",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_HasUserInterval_Property")),
				    new DisplayNameAttribute("HasUserInterval")				);


				tableBuilder.AddCustomAttributes(t, "UserInterval",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_UserInterval_Property")),
				    new DisplayNameAttribute("UserInterval")				);


				tableBuilder.AddCustomAttributes(t, "IntervalOverride",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_IntervalOverride_Property")),
				    new DisplayNameAttribute("IntervalOverride")				);


				tableBuilder.AddCustomAttributes(t, "MinorCountOverride",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_MinorCountOverride_Property")),
				    new DisplayNameAttribute("MinorCountOverride")				);


				tableBuilder.AddCustomAttributes(t, "Mode2GroupCount",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_Mode2GroupCount_Property")),
				    new DisplayNameAttribute("Mode2GroupCount")				);


				tableBuilder.AddCustomAttributes(t, "Viewport",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_Viewport_Property")),
				    new DisplayNameAttribute("Viewport")				);


				tableBuilder.AddCustomAttributes(t, "Window",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_Window_Property")),
				    new DisplayNameAttribute("Window")				);


				tableBuilder.AddCustomAttributes(t, "IsInverted",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_IsInverted_Property")),
				    new DisplayNameAttribute("IsInverted")				);


				tableBuilder.AddCustomAttributes(t, "GetScaledValue",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_GetScaledValue_Property")),
				    new DisplayNameAttribute("GetScaledValue")				);


				tableBuilder.AddCustomAttributes(t, "GetGroupCenter",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_GetGroupCenter_Property")),
				    new DisplayNameAttribute("GetGroupCenter")				);


				tableBuilder.AddCustomAttributes(t, "GetUnscaledGroupCenter",
					new DescriptionAttribute(SR.GetString("TickmarkValuesInitializationParameters_GetUnscaledGroupCenter_Property")),
				    new DisplayNameAttribute("GetUnscaledGroupCenter")				);

				#endregion // TickmarkValuesInitializationParameters Properties

				#region FunnelSliceDataContext Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FunnelSliceDataContext");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ItemOutline",
					new DescriptionAttribute(SR.GetString("FunnelSliceDataContext_ItemOutline_Property")),
				    new DisplayNameAttribute("ItemOutline")				);

				#endregion // FunnelSliceDataContext Properties

				#region ScalerParams Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ScalerParams");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "WindowRect",
					new DescriptionAttribute(SR.GetString("ScalerParams_WindowRect_Property")),
				    new DisplayNameAttribute("WindowRect")				);


				tableBuilder.AddCustomAttributes(t, "ViewportRect",
					new DescriptionAttribute(SR.GetString("ScalerParams_ViewportRect_Property")),
				    new DisplayNameAttribute("ViewportRect")				);


				tableBuilder.AddCustomAttributes(t, "EffectiveViewportRect",
					new DescriptionAttribute(SR.GetString("ScalerParams_EffectiveViewportRect_Property")),
				    new DisplayNameAttribute("EffectiveViewportRect")				);


				tableBuilder.AddCustomAttributes(t, "IsInverted",
					new DescriptionAttribute(SR.GetString("ScalerParams_IsInverted_Property")),
				    new DisplayNameAttribute("IsInverted")				);

				#endregion // ScalerParams Properties

				#region HighDensityScatterSeries Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.HighDensityScatterSeries");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "XAxis",
					new DescriptionAttribute(SR.GetString("HighDensityScatterSeries_XAxis_Property")),
				    new DisplayNameAttribute("XAxis")				);


				tableBuilder.AddCustomAttributes(t, "YAxis",
					new DescriptionAttribute(SR.GetString("HighDensityScatterSeries_YAxis_Property")),
				    new DisplayNameAttribute("YAxis")				);


				tableBuilder.AddCustomAttributes(t, "XMemberPath",
					new DescriptionAttribute(SR.GetString("HighDensityScatterSeries_XMemberPath_Property")),
				    new DisplayNameAttribute("XMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "YMemberPath",
					new DescriptionAttribute(SR.GetString("HighDensityScatterSeries_YMemberPath_Property")),
				    new DisplayNameAttribute("YMemberPath")				);


				tableBuilder.AddCustomAttributes(t, "UseBruteForce",
					new DescriptionAttribute(SR.GetString("HighDensityScatterSeries_UseBruteForce_Property")),
				    new DisplayNameAttribute("UseBruteForce")				);


				tableBuilder.AddCustomAttributes(t, "ProgressiveLoad",
					new DescriptionAttribute(SR.GetString("HighDensityScatterSeries_ProgressiveLoad_Property")),
				    new DisplayNameAttribute("ProgressiveLoad")				);


				tableBuilder.AddCustomAttributes(t, "MouseOverEnabled",
					new DescriptionAttribute(SR.GetString("HighDensityScatterSeries_MouseOverEnabled_Property")),
				    new DisplayNameAttribute("MouseOverEnabled")				);


				tableBuilder.AddCustomAttributes(t, "UseSquareCutoffStyle",
					new DescriptionAttribute(SR.GetString("HighDensityScatterSeries_UseSquareCutoffStyle_Property")),
				    new DisplayNameAttribute("UseSquareCutoffStyle")				);


				tableBuilder.AddCustomAttributes(t, "HeatMinimum",
					new DescriptionAttribute(SR.GetString("HighDensityScatterSeries_HeatMinimum_Property")),
				    new DisplayNameAttribute("HeatMinimum")				);


				tableBuilder.AddCustomAttributes(t, "HeatMaximum",
					new DescriptionAttribute(SR.GetString("HighDensityScatterSeries_HeatMaximum_Property")),
				    new DisplayNameAttribute("HeatMaximum")				);

				#endregion // HighDensityScatterSeries Properties

				#region BarFragment Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.BarFragment");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // BarFragment Properties

				#region FragmentBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.FragmentBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // FragmentBase Properties

				#region SplineFragment Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SplineFragment");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SplineFragment Properties

				#region SplineFragmentBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SplineFragmentBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SplineFragmentBase Properties

				#region AreaFragment Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.AreaFragment");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AreaFragment Properties

				#region LineFragment Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.LineFragment");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // LineFragment Properties

				#region OwnedPoint Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.OwnedPoint");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Point",
					new DescriptionAttribute(SR.GetString("OwnedPoint_Point_Property")),
				    new DisplayNameAttribute("Point")				);


				tableBuilder.AddCustomAttributes(t, "OwnerItem",
					new DescriptionAttribute(SR.GetString("OwnedPoint_OwnerItem_Property")),
				    new DisplayNameAttribute("OwnerItem")				);


				tableBuilder.AddCustomAttributes(t, "ColumnValues",
					new DescriptionAttribute(SR.GetString("OwnedPoint_ColumnValues_Property")),
				    new DisplayNameAttribute("ColumnValues")				);

				#endregion // OwnedPoint Properties

				#region NumericMarkerManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.NumericMarkerManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "PopulateColumnValues",
					new DescriptionAttribute(SR.GetString("NumericMarkerManager_PopulateColumnValues_Property")),
				    new DisplayNameAttribute("PopulateColumnValues")				);


				tableBuilder.AddCustomAttributes(t, "GetColumnValues",
					new DescriptionAttribute(SR.GetString("NumericMarkerManager_GetColumnValues_Property")),
				    new DisplayNameAttribute("GetColumnValues")				);

				#endregion // NumericMarkerManager Properties

				#region MarkerManagerBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.MarkerManagerBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "GetItemLocationsStrategy",
					new DescriptionAttribute(SR.GetString("MarkerManagerBase_GetItemLocationsStrategy_Property")),
				    new DisplayNameAttribute("GetItemLocationsStrategy")				);


				tableBuilder.AddCustomAttributes(t, "ProvideMarkerStrategy",
					new DescriptionAttribute(SR.GetString("MarkerManagerBase_ProvideMarkerStrategy_Property")),
				    new DisplayNameAttribute("ProvideMarkerStrategy")				);


				tableBuilder.AddCustomAttributes(t, "RemoveUnusedMarkers",
					new DescriptionAttribute(SR.GetString("MarkerManagerBase_RemoveUnusedMarkers_Property")),
				    new DisplayNameAttribute("RemoveUnusedMarkers")				);


				tableBuilder.AddCustomAttributes(t, "ProvideItemStrategy",
					new DescriptionAttribute(SR.GetString("MarkerManagerBase_ProvideItemStrategy_Property")),
				    new DisplayNameAttribute("ProvideItemStrategy")				);


				tableBuilder.AddCustomAttributes(t, "ActiveMarkerIndexesStrategy",
					new DescriptionAttribute(SR.GetString("MarkerManagerBase_ActiveMarkerIndexesStrategy_Property")),
				    new DisplayNameAttribute("ActiveMarkerIndexesStrategy")				);

				#endregion // MarkerManagerBase Properties

				#region MouseMoveThunk Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.MouseMoveThunk");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Position",
					new DescriptionAttribute(SR.GetString("MouseMoveThunk_Position_Property")),
				    new DisplayNameAttribute("Position")				);


				tableBuilder.AddCustomAttributes(t, "AxisPosition",
					new DescriptionAttribute(SR.GetString("MouseMoveThunk_AxisPosition_Property")),
				    new DisplayNameAttribute("AxisPosition")				);


				tableBuilder.AddCustomAttributes(t, "DesiredNeighborCount",
					new DescriptionAttribute(SR.GetString("MouseMoveThunk_DesiredNeighborCount_Property")),
				    new DisplayNameAttribute("DesiredNeighborCount")				);


				tableBuilder.AddCustomAttributes(t, "ScalerParamsX",
					new DescriptionAttribute(SR.GetString("MouseMoveThunk_ScalerParamsX_Property")),
				    new DisplayNameAttribute("ScalerParamsX")				);

				#endregion // MouseMoveThunk Properties

				#region ScatterMouseOverEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ScatterMouseOverEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AxisPosition",
					new DescriptionAttribute(SR.GetString("ScatterMouseOverEventArgs_AxisPosition_Property")),
				    new DisplayNameAttribute("AxisPosition")				);


				tableBuilder.AddCustomAttributes(t, "MousePosition",
					new DescriptionAttribute(SR.GetString("ScatterMouseOverEventArgs_MousePosition_Property")),
				    new DisplayNameAttribute("MousePosition")				);


				tableBuilder.AddCustomAttributes(t, "NearestItems",
					new DescriptionAttribute(SR.GetString("ScatterMouseOverEventArgs_NearestItems_Property")),
				    new DisplayNameAttribute("NearestItems")				);

				#endregion // ScatterMouseOverEventArgs Properties

				#region HighDensityScatterSeriesView Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.HighDensityScatterSeriesView");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HighDensityScatterSeriesView Properties

				#region ProgressiveLoadStatusEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.ProgressiveLoadStatusEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentStatus",
					new DescriptionAttribute(SR.GetString("ProgressiveLoadStatusEventArgs_CurrentStatus_Property")),
				    new DisplayNameAttribute("CurrentStatus")				);

				#endregion // ProgressiveLoadStatusEventArgs Properties

				#region ChartVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.ChartVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Axes",
					new DescriptionAttribute(SR.GetString("ChartVisualData_Axes_Property")),
				    new DisplayNameAttribute("Axes")				);


				tableBuilder.AddCustomAttributes(t, "Series",
					new DescriptionAttribute(SR.GetString("ChartVisualData_Series_Property")),
				    new DisplayNameAttribute("Series")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("ChartVisualData_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "IsViewportScaled",
					new DescriptionAttribute(SR.GetString("ChartVisualData_IsViewportScaled_Property")),
				    new DisplayNameAttribute("IsViewportScaled")				);

				#endregion // ChartVisualData Properties

				#region SeriesVisualDataList Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.SeriesVisualDataList");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SeriesVisualDataList Properties

				#region SeriesVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.SeriesVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("SeriesVisualData_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("SeriesVisualData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Viewport",
					new DescriptionAttribute(SR.GetString("SeriesVisualData_Viewport_Property")),
				    new DisplayNameAttribute("Viewport")				);


				tableBuilder.AddCustomAttributes(t, "Shapes",
					new DescriptionAttribute(SR.GetString("SeriesVisualData_Shapes_Property")),
				    new DisplayNameAttribute("Shapes")				);


				tableBuilder.AddCustomAttributes(t, "MarkerShapes",
					new DescriptionAttribute(SR.GetString("SeriesVisualData_MarkerShapes_Property")),
				    new DisplayNameAttribute("MarkerShapes")				);

				#endregion // SeriesVisualData Properties

				#region MarkerVisualDataList Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.MarkerVisualDataList");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MarkerVisualDataList Properties

				#region MarkerVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.MarkerVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "X",
					new DescriptionAttribute(SR.GetString("MarkerVisualData_X_Property")),
				    new DisplayNameAttribute("X")				);


				tableBuilder.AddCustomAttributes(t, "Y",
					new DescriptionAttribute(SR.GetString("MarkerVisualData_Y_Property")),
				    new DisplayNameAttribute("Y")				);


				tableBuilder.AddCustomAttributes(t, "Index",
					new DescriptionAttribute(SR.GetString("MarkerVisualData_Index_Property")),
				    new DisplayNameAttribute("Index")				);


				tableBuilder.AddCustomAttributes(t, "ContentTemplate",
					new DescriptionAttribute(SR.GetString("MarkerVisualData_ContentTemplate_Property")),
				    new DisplayNameAttribute("ContentTemplate")				);


				tableBuilder.AddCustomAttributes(t, "Visibility",
					new DescriptionAttribute(SR.GetString("MarkerVisualData_Visibility_Property")),
				    new DisplayNameAttribute("Visibility")				);

				#endregion // MarkerVisualData Properties

				#region AxisVisualDataList Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.AxisVisualDataList");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AxisVisualDataList Properties

				#region AxisVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.AxisVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("AxisVisualData_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("AxisVisualData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Viewport",
					new DescriptionAttribute(SR.GetString("AxisVisualData_Viewport_Property")),
				    new DisplayNameAttribute("Viewport")				);


				tableBuilder.AddCustomAttributes(t, "Labels",
					new DescriptionAttribute(SR.GetString("AxisVisualData_Labels_Property")),
				    new DisplayNameAttribute("Labels")				);


				tableBuilder.AddCustomAttributes(t, "AxisLine",
					new DescriptionAttribute(SR.GetString("AxisVisualData_AxisLine_Property")),
				    new DisplayNameAttribute("AxisLine")				);


				tableBuilder.AddCustomAttributes(t, "MajorLines",
					new DescriptionAttribute(SR.GetString("AxisVisualData_MajorLines_Property")),
				    new DisplayNameAttribute("MajorLines")				);


				tableBuilder.AddCustomAttributes(t, "MinorLines",
					new DescriptionAttribute(SR.GetString("AxisVisualData_MinorLines_Property")),
				    new DisplayNameAttribute("MinorLines")				);

				#endregion // AxisVisualData Properties

				#region PrimitiveVisualDataList Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.PrimitiveVisualDataList");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // PrimitiveVisualDataList Properties

				#region AxisLabelVisualDataList Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.AxisLabelVisualDataList");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AxisLabelVisualDataList Properties

				#region AxisLabelVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.AxisLabelVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LabelValue",
					new DescriptionAttribute(SR.GetString("AxisLabelVisualData_LabelValue_Property")),
				    new DisplayNameAttribute("LabelValue")				);


				tableBuilder.AddCustomAttributes(t, "LabelPosition",
					new DescriptionAttribute(SR.GetString("AxisLabelVisualData_LabelPosition_Property")),
				    new DisplayNameAttribute("LabelPosition")				);


				tableBuilder.AddCustomAttributes(t, "Appearance",
					new DescriptionAttribute(SR.GetString("AxisLabelVisualData_Appearance_Property")),
				    new DisplayNameAttribute("Appearance")				);

				#endregion // AxisLabelVisualData Properties

				#region LabelAppearanceData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.LabelAppearanceData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Text",
					new DescriptionAttribute(SR.GetString("LabelAppearanceData_Text_Property")),
				    new DisplayNameAttribute("Text")				);

				#endregion // LabelAppearanceData Properties

				#region PrimitiveAppearanceData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.PrimitiveAppearanceData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Stroke",
					new DescriptionAttribute(SR.GetString("PrimitiveAppearanceData_Stroke_Property")),
				    new DisplayNameAttribute("Stroke")				);


				tableBuilder.AddCustomAttributes(t, "Fill",
					new DescriptionAttribute(SR.GetString("PrimitiveAppearanceData_Fill_Property")),
				    new DisplayNameAttribute("Fill")				);


				tableBuilder.AddCustomAttributes(t, "StrokeThickness",
					new DescriptionAttribute(SR.GetString("PrimitiveAppearanceData_StrokeThickness_Property")),
				    new DisplayNameAttribute("StrokeThickness")				);


				tableBuilder.AddCustomAttributes(t, "Visibility",
					new DescriptionAttribute(SR.GetString("PrimitiveAppearanceData_Visibility_Property")),
				    new DisplayNameAttribute("Visibility")				);


				tableBuilder.AddCustomAttributes(t, "Opacity",
					new DescriptionAttribute(SR.GetString("PrimitiveAppearanceData_Opacity_Property")),
				    new DisplayNameAttribute("Opacity")				);


				tableBuilder.AddCustomAttributes(t, "CanvasLeft",
					new DescriptionAttribute(SR.GetString("PrimitiveAppearanceData_CanvasLeft_Property")),
				    new DisplayNameAttribute("CanvasLeft")				);


				tableBuilder.AddCustomAttributes(t, "CanvasTop",
					new DescriptionAttribute(SR.GetString("PrimitiveAppearanceData_CanvasTop_Property")),
				    new DisplayNameAttribute("CanvasTop")				);


				tableBuilder.AddCustomAttributes(t, "CanvaZIndex",
					new DescriptionAttribute(SR.GetString("PrimitiveAppearanceData_CanvaZIndex_Property")),
				    new DisplayNameAttribute("CanvaZIndex")				);


				tableBuilder.AddCustomAttributes(t, "DashArray",
					new DescriptionAttribute(SR.GetString("PrimitiveAppearanceData_DashArray_Property")),
				    new DisplayNameAttribute("DashArray")				);


				tableBuilder.AddCustomAttributes(t, "DashCap",
					new DescriptionAttribute(SR.GetString("PrimitiveAppearanceData_DashCap_Property")),
				    new DisplayNameAttribute("DashCap")				);

				#endregion // PrimitiveAppearanceData Properties

				#region GetPointsSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.GetPointsSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IgnoreFigureStartPoint",
					new DescriptionAttribute(SR.GetString("GetPointsSettings_IgnoreFigureStartPoint_Property")),
				    new DisplayNameAttribute("IgnoreFigureStartPoint")				);

				#endregion // GetPointsSettings Properties

				#region PrimitiveVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.PrimitiveVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Appearance",
					new DescriptionAttribute(SR.GetString("PrimitiveVisualData_Appearance_Property")),
				    new DisplayNameAttribute("Appearance")				);


				tableBuilder.AddCustomAttributes(t, "Tags",
					new DescriptionAttribute(SR.GetString("PrimitiveVisualData_Tags_Property")),
				    new DisplayNameAttribute("Tags")				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("PrimitiveVisualData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("PrimitiveVisualData_Name_Property")),
				    new DisplayNameAttribute("Name")				);

				#endregion // PrimitiveVisualData Properties

				#region RectangleVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.RectangleVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Width",
					new DescriptionAttribute(SR.GetString("RectangleVisualData_Width_Property")),
				    new DisplayNameAttribute("Width")				);


				tableBuilder.AddCustomAttributes(t, "Height",
					new DescriptionAttribute(SR.GetString("RectangleVisualData_Height_Property")),
				    new DisplayNameAttribute("Height")				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("RectangleVisualData_Type_Property")),
				    new DisplayNameAttribute("Type")				);

				#endregion // RectangleVisualData Properties

				#region ShapeTags Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.ShapeTags");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ShapeTags Properties

				#region LineVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.LineVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("LineVisualData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "X1",
					new DescriptionAttribute(SR.GetString("LineVisualData_X1_Property")),
				    new DisplayNameAttribute("X1")				);


				tableBuilder.AddCustomAttributes(t, "Y1",
					new DescriptionAttribute(SR.GetString("LineVisualData_Y1_Property")),
				    new DisplayNameAttribute("Y1")				);


				tableBuilder.AddCustomAttributes(t, "X2",
					new DescriptionAttribute(SR.GetString("LineVisualData_X2_Property")),
				    new DisplayNameAttribute("X2")				);


				tableBuilder.AddCustomAttributes(t, "Y2",
					new DescriptionAttribute(SR.GetString("LineVisualData_Y2_Property")),
				    new DisplayNameAttribute("Y2")				);

				#endregion // LineVisualData Properties

				#region PolyLineVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.PolyLineVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("PolyLineVisualData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Points",
					new DescriptionAttribute(SR.GetString("PolyLineVisualData_Points_Property")),
				    new DisplayNameAttribute("Points")				);

				#endregion // PolyLineVisualData Properties

				#region PolygonVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.PolygonVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("PolygonVisualData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Points",
					new DescriptionAttribute(SR.GetString("PolygonVisualData_Points_Property")),
				    new DisplayNameAttribute("Points")				);

				#endregion // PolygonVisualData Properties

				#region PathVisualData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.PathVisualData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("PathVisualData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Data",
					new DescriptionAttribute(SR.GetString("PathVisualData_Data_Property")),
				    new DisplayNameAttribute("Data")				);

				#endregion // PathVisualData Properties

				#region GeometryData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.GeometryData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("GeometryData_Type_Property")),
				    new DisplayNameAttribute("Type")				);

				#endregion // GeometryData Properties

				#region PathGeometryData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.PathGeometryData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("PathGeometryData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Figures",
					new DescriptionAttribute(SR.GetString("PathGeometryData_Figures_Property")),
				    new DisplayNameAttribute("Figures")				);

				#endregion // PathGeometryData Properties

				#region LineGeometryData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.LineGeometryData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("LineGeometryData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "X1",
					new DescriptionAttribute(SR.GetString("LineGeometryData_X1_Property")),
				    new DisplayNameAttribute("X1")				);


				tableBuilder.AddCustomAttributes(t, "Y1",
					new DescriptionAttribute(SR.GetString("LineGeometryData_Y1_Property")),
				    new DisplayNameAttribute("Y1")				);


				tableBuilder.AddCustomAttributes(t, "X2",
					new DescriptionAttribute(SR.GetString("LineGeometryData_X2_Property")),
				    new DisplayNameAttribute("X2")				);


				tableBuilder.AddCustomAttributes(t, "Y2",
					new DescriptionAttribute(SR.GetString("LineGeometryData_Y2_Property")),
				    new DisplayNameAttribute("Y2")				);

				#endregion // LineGeometryData Properties

				#region RectangleGeometryData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.RectangleGeometryData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("RectangleGeometryData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "X",
					new DescriptionAttribute(SR.GetString("RectangleGeometryData_X_Property")),
				    new DisplayNameAttribute("X")				);


				tableBuilder.AddCustomAttributes(t, "Y",
					new DescriptionAttribute(SR.GetString("RectangleGeometryData_Y_Property")),
				    new DisplayNameAttribute("Y")				);


				tableBuilder.AddCustomAttributes(t, "Width",
					new DescriptionAttribute(SR.GetString("RectangleGeometryData_Width_Property")),
				    new DisplayNameAttribute("Width")				);


				tableBuilder.AddCustomAttributes(t, "Height",
					new DescriptionAttribute(SR.GetString("RectangleGeometryData_Height_Property")),
				    new DisplayNameAttribute("Height")				);

				#endregion // RectangleGeometryData Properties

				#region EllipseGeometryData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.EllipseGeometryData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("EllipseGeometryData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "CenterX",
					new DescriptionAttribute(SR.GetString("EllipseGeometryData_CenterX_Property")),
				    new DisplayNameAttribute("CenterX")				);


				tableBuilder.AddCustomAttributes(t, "CenterY",
					new DescriptionAttribute(SR.GetString("EllipseGeometryData_CenterY_Property")),
				    new DisplayNameAttribute("CenterY")				);


				tableBuilder.AddCustomAttributes(t, "RadiusX",
					new DescriptionAttribute(SR.GetString("EllipseGeometryData_RadiusX_Property")),
				    new DisplayNameAttribute("RadiusX")				);


				tableBuilder.AddCustomAttributes(t, "RadiusY",
					new DescriptionAttribute(SR.GetString("EllipseGeometryData_RadiusY_Property")),
				    new DisplayNameAttribute("RadiusY")				);

				#endregion // EllipseGeometryData Properties

				#region PathFigureData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.PathFigureData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "StartPoint",
					new DescriptionAttribute(SR.GetString("PathFigureData_StartPoint_Property")),
				    new DisplayNameAttribute("StartPoint")				);


				tableBuilder.AddCustomAttributes(t, "Segments",
					new DescriptionAttribute(SR.GetString("PathFigureData_Segments_Property")),
				    new DisplayNameAttribute("Segments")				);

				#endregion // PathFigureData Properties

				#region SegmentData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.SegmentData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("SegmentData_Type_Property")),
				    new DisplayNameAttribute("Type")				);

				#endregion // SegmentData Properties

				#region LineSegmentData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.LineSegmentData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("LineSegmentData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Point",
					new DescriptionAttribute(SR.GetString("LineSegmentData_Point_Property")),
				    new DisplayNameAttribute("Point")				);

				#endregion // LineSegmentData Properties

				#region PolylineSegmentData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.PolylineSegmentData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("PolylineSegmentData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Points",
					new DescriptionAttribute(SR.GetString("PolylineSegmentData_Points_Property")),
				    new DisplayNameAttribute("Points")				);

				#endregion // PolylineSegmentData Properties

				#region ArcSegmentData Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.ArcSegmentData");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("ArcSegmentData_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Point",
					new DescriptionAttribute(SR.GetString("ArcSegmentData_Point_Property")),
				    new DisplayNameAttribute("Point")				);


				tableBuilder.AddCustomAttributes(t, "IsLargeArc",
					new DescriptionAttribute(SR.GetString("ArcSegmentData_IsLargeArc_Property")),
				    new DisplayNameAttribute("IsLargeArc")				);


				tableBuilder.AddCustomAttributes(t, "IsCounterClockwise",
					new DescriptionAttribute(SR.GetString("ArcSegmentData_IsCounterClockwise_Property")),
				    new DisplayNameAttribute("IsCounterClockwise")				);


				tableBuilder.AddCustomAttributes(t, "SizeX",
					new DescriptionAttribute(SR.GetString("ArcSegmentData_SizeX_Property")),
				    new DisplayNameAttribute("SizeX")				);


				tableBuilder.AddCustomAttributes(t, "SizeY",
					new DescriptionAttribute(SR.GetString("ArcSegmentData_SizeY_Property")),
				    new DisplayNameAttribute("SizeY")				);


				tableBuilder.AddCustomAttributes(t, "RotationAngle",
					new DescriptionAttribute(SR.GetString("ArcSegmentData_RotationAngle_Property")),
				    new DisplayNameAttribute("RotationAngle")				);

				#endregion // ArcSegmentData Properties

				#region AppearanceHelper Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.VisualData.AppearanceHelper");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AppearanceHelper Properties

				#region SplineAreaFragment Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.SplineAreaFragment");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SplineAreaFragment Properties

				#region MarkerManagerBucket Properties
				t = controlAssembly.GetType("Infragistics.Controls.Charts.MarkerManagerBucket");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Items",
					new DescriptionAttribute(SR.GetString("MarkerManagerBucket_Items_Property")),
				    new DisplayNameAttribute("Items")				);


				tableBuilder.AddCustomAttributes(t, "PriorityItems",
					new DescriptionAttribute(SR.GetString("MarkerManagerBucket_PriorityItems_Property")),
				    new DisplayNameAttribute("PriorityItems")				);


				tableBuilder.AddCustomAttributes(t, "IsEmpty",
					new DescriptionAttribute(SR.GetString("MarkerManagerBucket_IsEmpty_Property")),
				    new DisplayNameAttribute("IsEmpty")				);

				#endregion // MarkerManagerBucket Properties
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