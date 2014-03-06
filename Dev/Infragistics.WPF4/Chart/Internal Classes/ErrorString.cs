
#region Using

using System;
using System.Collections.Generic;
using System.Text;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class is temporary! All error messages should be located in resource file. Because of 
    /// problem with obfuscation error messages are temporary located in this class.
    /// </summary>
    internal class ErrorString
    {
        internal static string Exc1 { get { return "Position - The rectangle values must be between 0 and 100."; } }
        internal static string Exc10 { get { return "Axis Value � Axis type not defined."; } }
        internal static string Exc11 { get { return "Axis interval cannot be negative value or zero."; } }
        internal static string Exc12 { get { return "Mark Unit value too small. Increase Unit value for Axis or remove axis labels."; } }
        internal static string Exc13 { get { return "Type not supported for tooltips. Use any other UIElement or text instead."; } }
        internal static string Exc14 { get { return "Error Generating Arc points (Invalid Radius): The center point doesn�t have same distance from the start and the end point."; } }
        internal static string Exc15 { get { return "The radius is smaler then the distance between two points. (Primitives3D)"; } }
        internal static string Exc16 { get { return "Axis Value � Axis type not defined."; } }
        internal static string Exc17 { get { return "Stripe Unit value too small."; } }
        internal static string Exc18 { get { return "Mark Unit value too small."; } }
        internal static string Exc19 { get { return "The Chart cannot be placed outside the chart control."; } }
        internal static string Exc2 { get { return "Stacked area chart can�t have positive and negative data point values together."; } }
        internal static string Exc21 { get { return "The value for �Column3DNumberOfSides� chart attribute has to be between 3 and 8."; } }
        internal static string Exc22 { get { return "The value for �Column3DStarNumberOfSides� chart attribute has to be between 4 and 8."; } }
        internal static string Exc23 { get { return "The value for �ExplodedRadius� chart attribute has to be between 0 and 1."; } }
        internal static string Exc24 { get { return "The value for �EdgeSize3D� chart attribute has to be between 0 and 5. Default value is 1."; } }
        internal static string Exc25 { get { return "The value for �Pie3DRounding� chart attribute has to be between 1 and 3. Default value is 2."; } }
        internal static string Exc26 { get { return "The value for �Radius� chart attribute has to be Greater or equal to 0. Default value is 0."; } }
        internal static string Exc27 { get { return "Value from a Chart attribute cannot be converted to Brush."; } }
        internal static string Exc28 { get { return "Value from a Chart attribute cannot be converted to integer value."; } }
        internal static string Exc29 { get { return "Value from a Chart attribute cannot be converted to boolean value."; } }
        internal static string Exc3 { get { return "Invalid inner and outer radius for 3D doughnut chart."; } }
        internal static string Exc30 { get { return "Value from a Chart attribute cannot be converted to double value."; } }
        internal static string Exc31 { get { return "Series does not have a parent."; } }
        internal static string Exc32 { get { return "Invalid Data Type from Data Source."; } }
        internal static string Exc33 { get { return "Data Binding Error - Wrong Data Point value."; } }
        internal static string Exc34{ get { return "The Chart Grid Area cannot be placed outside the chart control."; } }
        internal static string Exc35 { get { return "The Chart Legend cannot be placed outside the chart control."; } }
        internal static string Exc36 { get { return "The Chart Scene cannot be placed outside the chart control."; } }
        internal static string Exc37 { get { return "Bad format for DataMapping property."; } }
        internal static string Exc38 { get { return "Palette color position can be value between 0 and 1."; } }
        internal static string Exc39 { get { return "The scatter chart types cannot be used together with other non scatter chart types."; } }
        internal static string Exc4 { get { return "The axis of the labels not set."; } }
        internal static string Exc40 { get { return "The pie and doughnut chart types cannot be used together with other chart types."; } }
        internal static string Exc401 { get { return "The pie and doughnut chart types cannot be used together with other chart types."; } }
        internal static string Exc41 { get { return "The bar chart types cannot be used together with other non bar chart types."; } }
        internal static string Exc42 { get { return "Invalid values for StartPaletteBrush and EndPaletteBrush. The palette could be created using SolidColorBrush and LinearGradientBrush."; } }
        internal static string Exc43 { get { return "The stock chart needs High, Low, Open and Close values. The values have to be set for every data point using ChartParameters collection."; } }
        internal static string Exc44 { get { return "The candlestick chart needs High, Low, Open and Close values. The values have to be set for every data point using ChartParameters collection."; } }
        internal static string Exc45 { get { return "Stock chart type is not supported in 3D."; } }
        internal static string Exc46 { get { return "Candlestick chart type is not supported in 3D."; } }
        internal static string Exc47 { get { return "The scatter chart needs ValueX and ValueY. The values have to be set for every data point using ChartParameters collection."; } }
        internal static string Exc48 { get { return "The scatter chart needs ValueX, ValueY and ValueZ. The values have to be set for every data point using ChartParameters collection."; } }
        internal static string Exc49 { get { return "The bubble chart needs ValueX, ValueY and Radius. The values have to be set for every data point using ChartParameters collection."; } }
        internal static string Exc5 { get { return "The chart not set."; } }
        internal static string Exc50 { get { return "The bubble chart needs ValueX, ValueY, ValueZ and Radius. The values have to be set for every data point using ChartParameters collection."; } }
        internal static string Exc51 { get { return "Left, Right, Top and Bottom Margin values have to be between 0 and 100 if MarginType is set to Percent."; } }
        internal static string Exc52 { get { return "Margin values cause negative Width or Height."; } }
        internal static string Exc53 { get { return "The value for �RectangleRounding� chart parameter has to be greater or equal to 0. Default value is 0."; } }
        internal static string Exc54 { get { return "The value for �PointWidth� chart parameter has to be between 0 and 2. Default value is 1."; } }
        internal static string Exc55 { get { return "Invalid marker formatting string. Data Label format uses .NET Composite Formatting specification. The �{Value}� text is replaced with data point value. For Bubble, scatter and stock chart types {ValueX}, {ValueY}, {Radius}, {High}, {Low}, etc. formats are used. For example, to create a label which displays a text �Data point value is: 3� where 3 is data point value, we have to create a following format string �Data point value is: {Value}�. Few more examples: 1. �Product price is: $3.00 dollars� -> �Product price is: {Value:C} dollars�. 2. �Calculated value is: 03.00� -> �Calculated value is: {Value:00.00}�. For more information about format property see �Composite Formatting� in .NET Framework Developer�s Guide."; } }
        internal static string Exc56 { get { return "The Data Series of stacked area chart has different number of data points."; } }
        internal static string Exc57 { get { return "Data Binding Error - Invalid binding property name. Check DataMapping format string."; } }
        internal static string Exc58 { get { return "GetPosition method has to be used with 2D charts only."; } }
        internal static string Exc59 { get { return "The category X and Z axes cannot have Logarithmic scale."; } }
        internal static string Exc6 { get { return "Mark Unit value too small."; } }
        internal static string Exc60 { get { return "Candlestick chart does not support different data point colors. The Series.DataPointColor cannot be set to DataPointColor.Different for this chart type."; } }
        internal static string Exc61 { get { return "100% Chart types do not support logarithmic axes."; } }
        internal static string Exc62 { get { return "If the AutoRange is set to false, Minimum and Maximum have to be set and Unit value must be greater than zero."; } }
        internal static string Exc63 { get { return "There is more than one Axis which has axis type: "; } }
        internal static string Exc64 { get { return "Crossing cannot be used on logarithmic scale."; } }
        internal static string Exc65 { get { return "100% stacked chart types cannot be used with other chart types."; } }
        internal static string Exc66 { get { return "Axis Unit value too small."; } }
        internal static string Exc67 { get { return "This brush type is not supported with this rendering mode. Please, use Full rendering mode."; } }
        internal static string Exc68 { get { return "Format specifier was invalid."; } }
        internal static string Exc7 { get { return "Invalid minimum and maximum values. Minimum cannot be greater than the maximum."; } }
        internal static string Exc8 { get { return "The Unit value for this axis is too small for specified Minimum and Maximum values. Increase the Unit value."; } }
        internal static string Exc9 { get { return "Logarithmic axes cannot show negative or zero values."; } }
        internal static string Str1 { get { return "XamChart Warning:"; } }        
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