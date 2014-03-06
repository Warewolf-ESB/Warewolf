using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;
using System.ComponentModel;
using Microsoft.Windows.Design.PropertyEditing;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.Design.Chart
{

	// JM 01-06-10 VS2010 Designer Support
	#region DesignMetadataHelper Static Class

	internal static class DesignMetadataHelper
	{
		internal static AttributeTableBuilder GetAttributeTableBuilder()
		{
			AttributeTableBuilder builder = new AttributeTableBuilder();

			#region Description/Category Attributes
			// Infragistics.Windows.Chart.XamChart
			// ===================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.XamChart), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes(CreateDescription("LD_XamChart"));

				// JM 05-04-11 TFS70940 Add ToolboxCategoryAttribute and ToolboxBrowsableAttribute.
                // [DN October 21 2011] remove XamChart from toolbox because it is being retired.
				callbackBuilder.AddCustomAttributes(ToolboxBrowsableAttribute.No);

				// callbackBuilder.AddCustomAttributes(new ToolboxCategoryAttribute(SR.GetString("XamChartAssetLibrary")));


				callbackBuilder.AddCustomAttributes("Series", CreateCategory("LC_Chart Data"), CreateDescription("LD_XamChart_P_Series"));
				callbackBuilder.AddCustomAttributes("Axes", CreateCategory("LC_Chart Data"), CreateDescription("LD_XamChart_P_Axes"));
				callbackBuilder.AddCustomAttributes("Transform3D", CreateCategory("LC_Chart Data"), CreateDescription("LD_XamChart_P_Transform3D"));
				callbackBuilder.AddCustomAttributes("Caption", CreateCategory("LC_Chart Data"), CreateDescription("LD_XamChart_P_Caption"));
				callbackBuilder.AddCustomAttributes("Scene", CreateCategory("LC_Chart Data"), CreateDescription("LD_XamChart_P_Scene"));
				callbackBuilder.AddCustomAttributes("Legend", CreateCategory("LC_Chart Data"), CreateDescription("LD_XamChart_P_Legend"));
				callbackBuilder.AddCustomAttributes("View3D", CreateCategory("LC_Chart Data"), CreateDescription("LD_XamChart_P_View3D"));
				callbackBuilder.AddCustomAttributes("RefreshEnabled", CreateDescription("LD_XamChart_P_RefreshEnabled"));
				callbackBuilder.AddCustomAttributes("Lights", CreateCategory("LC_Chart Data"), CreateDescription("LD_XamChart_P_Lights"));
				callbackBuilder.AddCustomAttributes("StartPaletteBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_XamChart_P_StartPaletteBrush"));
				callbackBuilder.AddCustomAttributes("EndPaletteBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_XamChart_P_EndPaletteBrush"));
				callbackBuilder.AddCustomAttributes("Theme", CreateCategory("LC_Chart Data"), CreateDescription("LD_XamChart_P_Theme"));
				callbackBuilder.AddCustomAttributes("DrawException", CreateDescription("LD_XamChart_P_DrawException"));
				callbackBuilder.AddCustomAttributes("DataBind", CreateCategory("LC_Behavior"), CreateDescription("LD_XamChart_E_DataBind"));
				callbackBuilder.AddCustomAttributes("ChartRendering", CreateCategory("LC_Behavior"), CreateDescription("LD_XamChart_E_ChartRendering"));
				callbackBuilder.AddCustomAttributes("ChartRendered", CreateCategory("LC_Behavior"), CreateDescription("LD_XamChart_E_ChartRendered"));
			});

			// Infragistics.Windows.Chart.Animation
			// ====================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.Animation), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("BeginTime", CreateDescription("LD_Animation_P_BeginTime"));
				callbackBuilder.AddCustomAttributes("Duration", CreateDescription("LD_Animation_P_Duration"));
				callbackBuilder.AddCustomAttributes("AccelerationRatio", CreateDescription("LD_Animation_P_AccelerationRatio"));
				callbackBuilder.AddCustomAttributes("DecelerationRatio", CreateDescription("LD_Animation_P_DecelerationRatio"));
				callbackBuilder.AddCustomAttributes("RepeatBehavior", CreateDescription("LD_Animation_P_RepeatBehavior"));
				callbackBuilder.AddCustomAttributes("Sequential", CreateDescription("LD_Animation_P_Sequential"));
			});

			// Infragistics.Windows.Chart.Axis
			// ===============================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.Axis), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("AxisType", CreateCategory("LC_Scale"), CreateDescription("LD_Axis_P_AxisType"));
				callbackBuilder.AddCustomAttributes("Maximum", CreateCategory("LC_Range"), CreateDescription("LD_Axis_P_Maximum"));
				callbackBuilder.AddCustomAttributes("Minimum", CreateCategory("LC_Range"), CreateDescription("LD_Axis_P_Minimum"));
				callbackBuilder.AddCustomAttributes("Unit", CreateCategory("LC_Range"), CreateDescription("LD_Axis_P_Unit"));
				callbackBuilder.AddCustomAttributes("AutoRange", CreateCategory("LC_Range"), CreateDescription("LD_Axis_P_AutoRange"));
				callbackBuilder.AddCustomAttributes("RangeFromZero", CreateCategory("LC_Range"), CreateDescription("LD_Axis_P_RangeFromZero"));
				callbackBuilder.AddCustomAttributes("Logarithmic", CreateCategory("LC_Scale"), CreateDescription("LD_Axis_P_Logarithmic"));
				callbackBuilder.AddCustomAttributes("LogarithmicBase", CreateCategory("LC_Scale"), CreateDescription("LD_Axis_P_LogarithmicBase"));
				callbackBuilder.AddCustomAttributes("Crossing", CreateCategory("LC_Scale"), CreateDescription("LD_Axis_P_Crossing"));
				callbackBuilder.AddCustomAttributes("Stroke", CreateCategory("LC_Brushes"), CreateDescription("LD_Axis_P_Stroke"));
				callbackBuilder.AddCustomAttributes("StrokeThickness", CreateCategory("LC_Appearance"), CreateDescription("LD_Axis_P_StrokeThickness"));
				callbackBuilder.AddCustomAttributes("Animation", CreateDescription("LD_Axis_P_Animation"));
				callbackBuilder.AddCustomAttributes("MajorGridline", CreateCategory("LC_Marks"), CreateDescription("LD_Axis_P_MajorGridline"));
				callbackBuilder.AddCustomAttributes("MinorGridline", CreateCategory("LC_Marks"), CreateDescription("LD_Axis_P_MinorGridline"));
				callbackBuilder.AddCustomAttributes("MajorTickMark", CreateCategory("LC_Marks"), CreateDescription("LD_Axis_P_MajorTickMark"));
				callbackBuilder.AddCustomAttributes("MinorTickMark", CreateCategory("LC_Marks"), CreateDescription("LD_Axis_P_MinorTickMark"));
				callbackBuilder.AddCustomAttributes("Stripes", CreateCategory("LC_Data"), CreateDescription("LD_Axis_P_Stripes"));
				callbackBuilder.AddCustomAttributes("Label", CreateCategory("LC_Marks"), CreateDescription("LD_Axis_P_Label"));
				callbackBuilder.AddCustomAttributes("Visible", CreateCategory("LC_Behavior"), CreateDescription("LD_Axis_P_Visible"));
				callbackBuilder.AddCustomAttributes("Name", CreateDescription("LD_Axis_P_Name"));
			});

			// Infragistics.Windows.Chart.Caption
			// ==================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.Caption), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Text", CreateCategory("LC_Data"), CreateDescription("LD_Caption_P_Text"));
				callbackBuilder.AddCustomAttributes("Margin", CreateCategory("LC_Layout"), CreateDescription("LD_Caption_P_Margin"));
				callbackBuilder.AddCustomAttributes("MarginType", CreateCategory("LC_Layout"), CreateDescription("LD_Caption_P_MarginType"));
				callbackBuilder.AddCustomAttributes("FontFamily", CreateCategory("LC_Font"), CreateDescription("LD_Caption_P_FontFamily"));
				callbackBuilder.AddCustomAttributes("FontSize", CreateCategory("LC_Font"), CreateDescription("LD_Caption_P_FontSize"));
				callbackBuilder.AddCustomAttributes("FontStyle", CreateCategory("LC_Font"), CreateDescription("LD_Caption_P_FontStyle"));
				callbackBuilder.AddCustomAttributes("FontWeight", CreateCategory("LC_Font"), CreateDescription("LD_Caption_P_FontWeight"));
				callbackBuilder.AddCustomAttributes("FontStretch", CreateCategory("LC_Font"), CreateDescription("LD_Caption_P_FontStretch"));
				callbackBuilder.AddCustomAttributes("Foreground", CreateCategory("LC_Brushes"), CreateDescription("LD_Caption_P_Foreground"));
				callbackBuilder.AddCustomAttributes("Background", CreateCategory("LC_Brushes"), CreateDescription("LD_Caption_P_Background"));
				callbackBuilder.AddCustomAttributes("BorderBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_Caption_P_BorderBrush"));
				callbackBuilder.AddCustomAttributes("BorderThickness", CreateDescription("LD_Caption_P_BorderThickness"));
			});

			// Infragistics.Windows.Chart.ChartParameter
			// =========================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.ChartParameter), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Value", CreateCategory("LC_Data"), CreateDescription("LD_ChartParameter_P_Value"));
				callbackBuilder.AddCustomAttributes("Type", CreateCategory("LC_Data"), CreateDescription("LD_ChartParameter_P_Type"));
			});

			// Infragistics.Windows.Chart.DataPointTemplate
			// ============================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.DataPointTemplate), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Fill", CreateDescription("LD_DataPointTemplate_P_Fill"));
				callbackBuilder.AddCustomAttributes("Stroke", CreateDescription("LD_DataPointTemplate_P_Stroke"));
				callbackBuilder.AddCustomAttributes("ToolTip", CreateDescription("LD_DataPointTemplate_P_ToolTip"));
				callbackBuilder.AddCustomAttributes("StrokeThickness", CreateDescription("LD_DataPointTemplate_P_StrokeThickness"));
			});

			// Infragistics.Windows.Chart.ColumnChartTemplate
			// ==============================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.ColumnChartTemplate), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsNegative", CreateDescription("LD_ColumnChartTemplate_P_IsNegative"));
				callbackBuilder.AddCustomAttributes("RectangleRounding", CreateDescription("LD_ColumnChartTemplate_P_RectangleRounding"));
				callbackBuilder.AddCustomAttributes("CornerRadius", CreateDescription("LD_ColumnChartTemplate_P_CornerRadius"));
				callbackBuilder.AddCustomAttributes("BorderThickness", CreateDescription("LD_ColumnChartTemplate_P_BorderThickness"));
			});

			// Infragistics.Windows.Chart.BarChartTemplate
			// ===========================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.BarChartTemplate), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("IsNegative", CreateDescription("LD_BarChartTemplate_P_IsNegative"));
				callbackBuilder.AddCustomAttributes("RectangleRounding", CreateDescription("LD_BarChartTemplate_P_RectangleRounding"));
				callbackBuilder.AddCustomAttributes("CornerRadius", CreateDescription("LD_BarChartTemplate_P_CornerRadius"));
				callbackBuilder.AddCustomAttributes("BorderThickness", CreateDescription("LD_BarChartTemplate_P_BorderThickness"));
			});

			// Infragistics.Windows.Chart.GridArea
			// ===================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.GridArea), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("RefreshPointsOnly", CreateDescription("LD_GridArea_P_RefreshPointsOnly"));
				callbackBuilder.AddCustomAttributes("Margin", CreateCategory("LC_Layout"), CreateDescription("LD_GridArea_P_Margin"));
				callbackBuilder.AddCustomAttributes("MarginType", CreateCategory("LC_Layout"), CreateDescription("LD_GridArea_P_MarginType"));
			});

			// Infragistics.Windows.Chart.Label
			// ================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.Label), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FontFamily", CreateCategory("LC_Font"), CreateDescription("LD_Label_P_FontFamily"));
				callbackBuilder.AddCustomAttributes("FontSize", CreateCategory("LC_Font"), CreateDescription("LD_Label_P_FontSize"));
				callbackBuilder.AddCustomAttributes("Angle", CreateCategory("LC_Font"), CreateDescription("LD_Label_P_Angle"));
				callbackBuilder.AddCustomAttributes("FontStyle", CreateCategory("LC_Font"), CreateDescription("LD_Label_P_FontStyle"));
				callbackBuilder.AddCustomAttributes("FontWeight", CreateCategory("LC_Font"), CreateDescription("LD_Label_P_FontWeight"));
				callbackBuilder.AddCustomAttributes("FontStretch", CreateCategory("LC_Font"), CreateDescription("LD_Label_P_FontStretch"));
				callbackBuilder.AddCustomAttributes("Foreground", CreateCategory("LC_Brushes"), CreateDescription("LD_Label_P_Foreground"));
				callbackBuilder.AddCustomAttributes("Format", CreateCategory("LC_Miscellaneous"), CreateDescription("LD_Label_P_Format"));
				callbackBuilder.AddCustomAttributes("AutoResize", CreateCategory("LC_Behavior"), CreateDescription("LD_Label_P_AutoResize"));
				callbackBuilder.AddCustomAttributes("DistanceFromAxis", CreateCategory("LC_Appearance"), CreateDescription("LD_Label_P_DistanceFromAxis"));
				callbackBuilder.AddCustomAttributes("Visible", CreateCategory("LC_Behavior"), CreateDescription("LD_Label_P_Visible"));
			});

			// Infragistics.Windows.Chart.LegendItem
			// =====================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.LegendItem), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Fill", CreateCategory("LC_Brushes"), CreateDescription("LD_LegendItem_P_Fill"));
				callbackBuilder.AddCustomAttributes("Stroke", CreateCategory("LC_Brushes"), CreateDescription("LD_LegendItem_P_Stroke"));
				callbackBuilder.AddCustomAttributes("StrokeThickness", CreateCategory("LC_Appearance"), CreateDescription("LD_LegendItem_P_StrokeThickness"));
				callbackBuilder.AddCustomAttributes("Text", CreateCategory("LC_Data"), CreateDescription("LD_LegendItem_P_Text"));
			});

			// Infragistics.Windows.Chart.LegendItemTemplate
			// =============================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.LegendItemTemplate), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Fill", CreateDescription("LD_LegendItemTemplate_P_Fill"));
				callbackBuilder.AddCustomAttributes("Stroke", CreateDescription("LD_LegendItemTemplate_P_Stroke"));
				callbackBuilder.AddCustomAttributes("StrokeThickness", CreateDescription("LD_LegendItemTemplate_P_StrokeThickness"));
				callbackBuilder.AddCustomAttributes("Text", CreateDescription("LD_LegendItemTemplate_P_Text"));
				callbackBuilder.AddCustomAttributes("Width", CreateDescription("LD_LegendItemTemplate_P_Width"));
				callbackBuilder.AddCustomAttributes("Height", CreateDescription("LD_LegendItemTemplate_P_Height"));
			});

			// Infragistics.Windows.Chart.Scene
			// ================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.Scene), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("GridArea", CreateCategory("LC_Chart Data"), CreateDescription("LD_Scene_P_GridArea"));
				callbackBuilder.AddCustomAttributes("Scene3DThickness", CreateCategory("LC_Chart Data"), CreateDescription("LD_Scene_P_Scene3DThickness"));
				callbackBuilder.AddCustomAttributes("Margin", CreateCategory("LC_Layout"), CreateDescription("LD_Scene_P_Margin"));
				callbackBuilder.AddCustomAttributes("MarginType", CreateCategory("LC_Layout"), CreateDescription("LD_Scene_P_MarginType"));
				callbackBuilder.AddCustomAttributes("Perspective", CreateCategory("LC_Chart Data"), CreateDescription("LD_Scene_P_Perspective"));
				callbackBuilder.AddCustomAttributes("Scene3DBrush", CreateCategory("LC_Brushes"), CreateDescription("LD_Scene_P_Scene3DBrush"));
				callbackBuilder.AddCustomAttributes("DataFilter", CreateDescription("LD_Scene_P_DataFilter"));
			});

			// Infragistics.Windows.Chart.Stripe
			// =================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.Stripe), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Animation", CreateDescription("LD_Stripe_P_Animation"));
				callbackBuilder.AddCustomAttributes("Stroke", CreateCategory("LC_Brushes"), CreateDescription("LD_Stripe_P_Stroke"));
				callbackBuilder.AddCustomAttributes("StrokeThickness", CreateCategory("LC_Appearance"), CreateDescription("LD_Stripe_P_StrokeThickness"));
				callbackBuilder.AddCustomAttributes("Unit", CreateCategory("LC_Range"), CreateDescription("LD_Stripe_P_Unit"));
				callbackBuilder.AddCustomAttributes("Fill", CreateCategory("LC_Brushes"), CreateDescription("LD_Stripe_P_Fill"));
				callbackBuilder.AddCustomAttributes("Width", CreateCategory("LC_Range"), CreateDescription("LD_Stripe_P_Width"));
			});

			// Infragistics.Windows.Chart.DataPoint
			// ====================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.DataPoint), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("ChartParameters", CreateCategory("LC_Data"), CreateDescription("LD_DataPoint_P_ChartParameters"));
				callbackBuilder.AddCustomAttributes("Value", CreateCategory("LC_Data"), CreateDescription("LD_DataPoint_P_Value"));
				callbackBuilder.AddCustomAttributes("Label", CreateCategory("LC_Data"), CreateDescription("LD_DataPoint_P_Label"));
				callbackBuilder.AddCustomAttributes("ExtraParameters", CreateDescription("LD_DataPoint_P_ExtraParameters"));
				callbackBuilder.AddCustomAttributes("Fill", CreateCategory("LC_Brushes"), CreateDescription("LD_DataPoint_P_Fill"));
				callbackBuilder.AddCustomAttributes("Stroke", CreateCategory("LC_Brushes"), CreateDescription("LD_DataPoint_P_Stroke"));
				callbackBuilder.AddCustomAttributes("StrokeThickness", CreateCategory("LC_Appearance"), CreateDescription("LD_DataPoint_P_StrokeThickness"));
				callbackBuilder.AddCustomAttributes("Animation", CreateCategory("LC_Appearance"), CreateDescription("LD_DataPoint_P_Animation"));
				callbackBuilder.AddCustomAttributes("Marker", CreateCategory("LC_Data"), CreateDescription("LD_DataPoint_P_Marker"));
				callbackBuilder.AddCustomAttributes("ToolTip", CreateCategory("LC_Data"), CreateDescription("LD_DataPoint_P_ToolTip"));
				callbackBuilder.AddCustomAttributes("NullValue", CreateCategory("LC_Data"), CreateDescription("LD_DataPoint_P_NullValue"));
			});

			// Infragistics.Windows.Chart.Legend
			// =================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.Legend), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Visible", CreateCategory("LC_Behavior"), CreateDescription("LD_Legend_P_Visible"));
				callbackBuilder.AddCustomAttributes("UseDataTemplate", CreateCategory("LC_Behavior"), CreateDescription("LD_Legend_P_UseDataTemplate"));
				callbackBuilder.AddCustomAttributes("Margin", CreateCategory("LC_Layout"), CreateDescription("LD_Legend_P_Margin"));
				callbackBuilder.AddCustomAttributes("MarginType", CreateCategory("LC_Layout"), CreateDescription("LD_Legend_P_MarginType"));
				callbackBuilder.AddCustomAttributes("Items", CreateCategory("LC_Data"), CreateDescription("LD_Legend_P_Items"));
			});

			// Infragistics.Windows.Chart.Mark
			// ===============================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.Mark), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Animation", CreateDescription("LD_Mark_P_Animation"));
				callbackBuilder.AddCustomAttributes("Stroke", CreateCategory("LC_Brushes"), CreateDescription("LD_Mark_P_Stroke"));
				callbackBuilder.AddCustomAttributes("Visible", CreateCategory("LC_Behavior"), CreateDescription("LD_Mark_P_Visible"));
				callbackBuilder.AddCustomAttributes("DashStyle", CreateCategory("LC_Appearance"), CreateDescription("LD_Mark_P_DashStyle"));
				callbackBuilder.AddCustomAttributes("StrokeThickness", CreateCategory("LC_Appearance"), CreateDescription("LD_Mark_P_StrokeThickness"));
				callbackBuilder.AddCustomAttributes("TickMarkSize", CreateCategory("LC_Appearance"), CreateDescription("LD_Mark_P_TickMarkSize"));
				callbackBuilder.AddCustomAttributes("Unit", CreateCategory("LC_Range"), CreateDescription("LD_Mark_P_Unit"));
			});

			// Infragistics.Windows.Chart.Marker
			// =================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.Marker), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("Type", CreateCategory("LC_Appearance"), CreateDescription("LD_Marker_P_Type"));
				callbackBuilder.AddCustomAttributes("FontFamily", CreateCategory("LC_Font"), CreateDescription("LD_Marker_P_FontFamily"));
				callbackBuilder.AddCustomAttributes("FontSize", CreateCategory("LC_Font"), CreateDescription("LD_Marker_P_FontSize"));
				callbackBuilder.AddCustomAttributes("FontStyle", CreateCategory("LC_Font"), CreateDescription("LD_Marker_P_FontStyle"));
				callbackBuilder.AddCustomAttributes("FontWeight", CreateCategory("LC_Font"), CreateDescription("LD_Marker_P_FontWeight"));
				callbackBuilder.AddCustomAttributes("FontStretch", CreateCategory("LC_Font"), CreateDescription("LD_Marker_P_FontStretch"));
				callbackBuilder.AddCustomAttributes("Foreground", CreateCategory("LC_Brushes"), CreateDescription("LD_Marker_P_Foreground"));
				callbackBuilder.AddCustomAttributes("Format", CreateCategory("LC_Miscellaneous"), CreateDescription("LD_Marker_P_Format"));
				callbackBuilder.AddCustomAttributes("MarkerSize", CreateCategory("LC_Appearance"), CreateDescription("LD_Marker_P_MarkerSize"));
				callbackBuilder.AddCustomAttributes("LabelDistance", CreateCategory("LC_Appearance"), CreateDescription("LD_Marker_P_LabelDistance"));
				callbackBuilder.AddCustomAttributes("Fill", CreateCategory("LC_Brushes"), CreateDescription("LD_Marker_P_Fill"));
				callbackBuilder.AddCustomAttributes("Stroke", CreateCategory("LC_Brushes"), CreateDescription("LD_Marker_P_Stroke"));
				callbackBuilder.AddCustomAttributes("StrokeThickness", CreateCategory("LC_Appearance"), CreateDescription("LD_Marker_P_StrokeThickness"));
				callbackBuilder.AddCustomAttributes("UseDataTemplate", CreateCategory("LC_Behavior"), CreateDescription("LD_Marker_P_UseDataTemplate"));
                callbackBuilder.AddCustomAttributes("DataTemplate", CreateCategory("LC_Behavior"), CreateDescription("LD_Marker_P_DataTemplate"));
                callbackBuilder.AddCustomAttributes("LabelOverflow", CreateCategory("LC_Behavior"), CreateDescription("LD_Marker_P_LabelOverflow"));
			});

			// Infragistics.Windows.Chart.Series
			// =================================
			builder.AddCallback(typeof(Infragistics.Windows.Chart.Series), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("DataPoints", CreateCategory("LC_Data"), CreateDescription("LD_Series_P_DataPoints"));
				callbackBuilder.AddCustomAttributes("ChartParameters", CreateCategory("LC_Data"), CreateDescription("LD_Series_P_ChartParameters"));
				callbackBuilder.AddCustomAttributes("ExtraParameters", CreateDescription("LD_Series_P_ExtraParameters"));
				callbackBuilder.AddCustomAttributes("ChartType", CreateCategory("LC_Appearance"), CreateDescription("LD_Series_P_ChartType"));
				callbackBuilder.AddCustomAttributes("DataPointColor", CreateCategory("LC_Appearance"), CreateDescription("LD_Series_P_DataPointColor"));
				callbackBuilder.AddCustomAttributes("DataMapping", CreateCategory("LC_Data"), CreateDescription("LD_Series_P_DataMapping"));
				callbackBuilder.AddCustomAttributes("DataSource", CreateCategory("LC_Data"), CreateDescription("LD_Series_P_DataSource"));
				callbackBuilder.AddCustomAttributes("Fill", CreateCategory("LC_Brushes"), CreateDescription("LD_Series_P_Fill"));
				callbackBuilder.AddCustomAttributes("Stroke", CreateCategory("LC_Brushes"), CreateDescription("LD_Series_P_Stroke"));
				callbackBuilder.AddCustomAttributes("StrokeThickness", CreateCategory("LC_Appearance"), CreateDescription("LD_Series_P_StrokeThickness"));
				callbackBuilder.AddCustomAttributes("Animation", CreateCategory("LC_Appearance"), CreateDescription("LD_Series_P_Animation"));
				callbackBuilder.AddCustomAttributes("Label", CreateCategory("LC_Data"), CreateDescription("LD_Series_P_Label"));
				callbackBuilder.AddCustomAttributes("Marker", CreateDescription("LD_Series_P_Marker"));
				callbackBuilder.AddCustomAttributes("AxisX", CreateDescription("LD_Series_P_AxisX"));
				callbackBuilder.AddCustomAttributes("AxisY", CreateDescription("LD_Series_P_AxisY"));
				callbackBuilder.AddCustomAttributes("UseDataTemplate", CreateCategory("LC_Behavior"), CreateDescription("LD_Series_P_UseDataTemplate"));
				callbackBuilder.AddCustomAttributes("ToolTip", CreateCategory("LC_Data"), CreateDescription("LD_Series_P_ToolTip"));
			});
			#endregion //Description/Category Attributes

			#region ToolboxBrowsableAttribute

			// ToolboxBrowsableAttribute
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Chart.ChartContentControl), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Chart.GridArea), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Chart.Legend), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Chart.Scene), ToolboxBrowsableAttribute.No);
			builder.AddCustomAttributes(typeof(Infragistics.Windows.Chart.CrosshairLine), ToolboxBrowsableAttribute.No);

			#endregion //ToolboxBrowsableAttribute

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