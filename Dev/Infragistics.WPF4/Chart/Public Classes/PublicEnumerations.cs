
#region Using

using System;
using System.Collections.Generic;
using System.Text;

#endregion Using

namespace Infragistics.Windows.Chart
{
    #region Enum

    /// <summary>
    /// Used to set the way on which data points get colors. By default colors for data points depend on 
    /// chart type (for example pie chart have different colors for every data point and column chart have 
    /// same color for all points).  
    /// </summary>
    public enum DataPointColor
    {
        /// <summary>
        /// Colors for data points depend on chart type
        /// </summary>
        Auto,

        /// <summary>
        /// Data Points have same color for all chart types.
        /// </summary>
        Same,

        /// <summary>
        /// Data Points have different colors for all chart types.
        /// </summary>
        Different

    }

    /// <summary>
    /// The Predefine look for the control.
    /// </summary>
    [Obsolete("The 'Theme' enumeration is no longer used. The theme property is of type string to allow custom themes to be registered.")]
	public enum Theme
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Theme Aquarius
        /// </summary>
        Aquarius,

        /// <summary>
        /// Theme Default
        /// </summary>
        Default,

        /// <summary>
        /// Theme Lucid Dream
        /// </summary>
        LucidDream,

        /// <summary>
        /// Theme Luminol
        /// </summary>
        Luminol,

        /// <summary>
        /// Theme Nautilus
        /// </summary>
        Nautilus,

        /// <summary>
        /// Theme Neon
        /// </summary>
        Neon,

        /// <summary>
        /// Theme Peach
        /// </summary>
        Peach,

        /// <summary>
        /// Theme Pumpkin
        /// </summary>
        Pumpkin,

        /// <summary>
        /// Theme Red Planet
        /// </summary>
        RedPlanet,

        /// <summary>
        /// Theme Royale Velvet
        /// </summary>
        RoyaleVelvet,

        /// <summary>
        /// Theme Theme Park
        /// </summary>
        ThemePark,
    }

    /// <summary>
    /// Margin types used for positioning of chart elements
    /// </summary>
    public enum MarginType
    {
        /// <summary>
        /// Automatic chart element positioning
        /// </summary>
        Auto,

        /// <summary>
        /// Manual chart element positioning
        /// </summary>
        Percent,
    }

    /// <summary>
    /// Rendering mode is used to improve rendering performance for a big number 
    /// of data points.  If the value is �Auto� Rendering mode depends on number of data 
    /// points. If the number of points is greater than 100 the performance mode is active. 
    /// When Performance mode is active, some wpf features are disabled (animation, backgrounds, 
    /// themes, data templates, etc). Auto is default value.
    /// </summary>
    public enum RenderingMode
    {
        /// <summary>
        /// If Auto mode is active the rendering mode depends on the number of data points. If the number of points is greater than 100 the performance mode is active.
        /// </summary>
        Auto,

        /// <summary>
        /// Used for high quality rendering. All wpf features are enabled (animation, backgrounds, themes, data templates, etc). 
        /// </summary>
        Full,

        /// <summary>
        /// Used for fast rendering of data points. When Performance mode is active, some wpf features are disabled (animation, backgrounds, themes, data templates, etc).
        /// </summary>
        Performance
    }

    /// <summary>
    /// Chart types used both for 2D and 3D
    /// </summary>
    public enum ChartType
    {
        /// <summary>
        /// A pie chart is a circular chart divided into sectors proportional to the percentages of the whole. 
        /// A pie chart always shows a single data series, and is useful for determining which item or items in the series are most significant.
        /// </summary>
        Pie,

        /// <summary>
        /// Line chart has data points plotted in series using evenly-spaced intervals and connected with a line to 
        /// emphasize the relationships between the points.
        /// </summary>
        Line,

        /// <summary>
        /// An area chart displays series as a set of points connected by a line with the area below the line filled in. 
        /// Values are represented on the y-axis and categories are displayed on the x-axis.
        /// </summary>
        Area,

        /// <summary>
        /// A column chart shows data points as rectangles which starts at the crossing value and ends at 
        /// the value which corresponds to the data point value.
        /// </summary>
        Column,

        /// <summary>
        /// A bubble chart shows data points using the first value for position end second value for relative radius.
        /// </summary>
        Bubble,

        /// <summary>
        /// A bar chart shows data points as rectangles which starts at the crossing value and ends at 
        /// the value which corresponds to the data point value. Similar to the column chart but it has horizontal orientation.
        /// </summary>
        Bar,

        /// <summary>
        /// Like a pie chart, a doughnut chart shows the size of items that make up a data series proportional 
        /// to the total of the items in the series.
        /// </summary>
        Doughnut,

        /// <summary>
        /// Cylinder chart looks like round column chart in 3D space. In 2D, cylinder chart is the same as column chart.
        /// </summary>
        Cylinder,

        /// <summary>
        /// Cylinder Bar chart looks like round bar chart in 3D space. In 2D, cylinder bar chart is the same as bar chart.
        /// </summary>
        CylinderBar,

        /// <summary>
        /// Spline chart has similar functionality as line chart. Line chart has data points connected with 
        /// the strait lines and spline chart has data points connected with the smooth curve.
        /// </summary>
        Spline,

        /// <summary>
        /// A stacked column chart is used to show cumulative values of data points which belong to 
        /// different data series and have same index or position inside parent series.
        /// </summary>
        StackedColumn,

        /// <summary>
        /// A stacked bar chart is used to show cumulative values of data points which belong to different 
        /// data series and have same index or position inside parent series.
        /// </summary>
        StackedBar,

        /// <summary>
        /// A stacked area chart is used to show cumulative values of data points which belong to different 
        /// data series and have same index or position inside parent series.
        /// </summary>
        StackedArea,

        /// <summary>
        /// Stacked cylinder chart looks like round stacked column chart in 3D space. In 2D, stacked cylinder 
        /// chart is the same as stacked column chart.
        /// </summary>
        StackedCylinder,

        /// <summary>
        /// Stacked cylinder bar chart looks like round stacked bar chart in 3D space. In 2D, stacked 
        /// cylinder bar chart is the same as stacked bar chart.
        /// </summary>
        StackedCylinderBar,

        /// <summary>
        /// A stacked 100% column chart is used to show relative cumulative values of data points (in percentages) which 
        /// belong to different data series and have same index or position inside parent series.
        /// </summary>
        Stacked100Column,

        /// <summary>
        /// Stacked 100% cylinder chart looks like rounded stacked 100% column chart in 3D space. In 2D, stacked 
        /// 100 % cylinder chart is the same as stacked 100% column chart.
        /// </summary>
        Stacked100Cylinder,

        /// <summary>
        /// Stacked 100% cylinder bar chart looks like rounded stacked 100% bar chart in 3D space. In 2D, stacked 
        /// 100 % cylinder bar chart is the same as stacked 100% bar chart.
        /// </summary>
        Stacked100CylinderBar,

        /// <summary>
        /// A stacked 100% bar chart is used to show relative cumulative values of data points (in percentages) which 
        /// belong to different data series and have same index or position inside parent series.
        /// </summary>
        Stacked100Bar,

        /// <summary>
        /// A stacked 100% area chart is used to show relative cumulative values of data points (in percentages) which 
        /// belong to different data series and have same index or position inside parent series.
        /// </summary>
        Stacked100Area,

        /// <summary>
        /// Scatter chart type is a point chart which uses two value axis (or 3 value axis in 3D). 
        /// Defined with X, Y and Z values from Chart parameters. This chart type does not 
        /// use Value property from data point!
        /// </summary>
        Scatter,

        /// <summary>
        /// Scatter line chart type is a line chart which uses two value axis 
        /// (or 3 value axis in 3D). Defined with X, Y and Z values from Chart 
        /// parameters. This chart type does not use Value property from data point!
        /// </summary>
        ScatterLine,

        /// <summary>
        /// Stock chart type is used to show stock market prices during the period of time. 
        /// Every data point shows High, Low, Open and Close prices. High and Low prices 
        /// are connected with vertical line, Open and Close prices are presented as horizontal 
        /// lines from left or right side of the main vertical line. This chart type does not use 
        /// Value property from data point! Instead, Chart parameters (High, Low, Open and Close) 
        /// are used as a data point values.
        /// </summary>
        Stock,

        /// <summary>
        /// Candlestick chart type is used to show stock market prices during the period of time. 
        /// Every data point shows High, Low, Open and Close prices. Open and Close prices are 
        /// connected with a rectangle, which fill color depends on negative or positive difference 
        /// between Open and Close price. This chart type does not use Value property from data point! 
        /// Insted, Chart parameters (High, Low, Open and Close) are used as a data point values.
        /// </summary>
        Candlestick,

        /// <summary>
        /// Point chart type uses category X axis and numeric Y axis (plus category Z axis for 3D chart). 
        /// Point chart is a chart type which has only markers placed at a position which corresponds to 
        /// the data point value.
        /// </summary>
        Point
    }

    /// <summary>
    /// Data Point marker types. Markers are various shapes used to show exact data point values. 
    /// Used together with different chart types. 
    /// </summary>
    public enum MarkerType
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Circle
        /// </summary>
        Circle,

        /// <summary>
        /// Rectangle
        /// </summary>
        Rectangle,

        /// <summary>
        /// Triangle
        /// </summary>
        Triangle,

        /// <summary>
        /// Hexagon
        /// </summary>
        Hexagon,

        /// <summary>
        /// Star 5
        /// </summary>
        Star5,

        /// <summary>
        /// Star 6
        /// </summary>
        Star6,

        /// <summary>
        /// Star 7
        /// </summary>
        Star7,

        /// <summary>
        /// Star 8
        /// </summary>
        Star8
    }

    /// <summary>
    /// Axis Types
    /// </summary>
    public enum AxisType
    {
        /// <summary>
        /// Primary X Axis
        /// </summary>
        PrimaryX,

        /// <summary>
        /// Primary Y Axis
        /// </summary>
        PrimaryY,

        /// <summary>
        /// Primary Z Axis
        /// </summary>
        PrimaryZ,

        /// <summary>
        /// Secondary X Axis
        /// </summary>
        SecondaryX,

        /// <summary>
        /// Secondary Y Axis
        /// </summary>
        SecondaryY,
    }

    /// <summary>
    /// Different types of chart parameters. Chart parameters provides a base set of parameters for different chart 
    /// types.
    /// </summary>
    public enum ChartParameterType
    {
        /// <summary>
        /// Data point Value used for scatter chart types.
        /// </summary>
        ValueX,

        /// <summary>
        /// Data point Value used for scatter chart types.
        /// </summary>
        ValueY,

        /// <summary>
        /// Data point Value used for scatter chart types.
        /// </summary>
        ValueZ,

        /// <summary>
        /// Data point Value used for bubble chart.
        /// </summary>
        Radius,

        /// <summary>
        /// Data point value used for stock chart types.
        /// </summary>
        High,

        /// <summary>
        /// Data point value used for stock chart types.
        /// </summary>
        Low,

        /// <summary>
        /// Data point value used for stock chart types.
        /// </summary>
        Open,

        /// <summary>
        /// Data point value used for stock chart types.
        /// </summary>
        Close,

        /// <summary>
        /// Used to define Point Width for category axis.  Value 1 for PointWidth means default width for data points, 
        /// 0.5 is twice smaller than default width and value 2 is twice bigger than default width. Default value for 
        /// this parameter is 1;
        /// </summary>
        PointWidth,

        /// <summary>
        /// Edge Size of the 3D charts. The value for �EdgeSize3D� chart parameter has to be between 0 and 1. Default value is 0.5.
        /// </summary>
        EdgeSize3D,

        /// <summary>
        /// Rounding Size of the 3D pie chart. The value for �Pie3DRounding� chart parameter has to be between 1 and 3. Default value is 2.
        /// </summary>
        Pie3DRounding,

        /// <summary>
        /// Used to create exploded pie or doughnut slice.
        /// </summary>
        Exploded,

        /// <summary>
        /// Radius of exploded pie or doughnut chart. The value for �ExplodedRadius� chart parameter has to be between 0 and 1.
        /// </summary>
        ExplodedRadius,

        /// <summary>
        /// The number of sides for 3D column chart. The value for �Column3DNumberOfSides� chart parameter has to be between 3 and 8.
        /// </summary>
        Column3DNumberOfSides,

        /// <summary>
        /// Column 3D chart has a shape of the star.
        /// </summary>
        Column3DStar,

        /// <summary>
        /// The number of sides for 3D column chart with star shape. The value for �Column3DStar� chart parameter has to be between 4 and 8.
        /// </summary>
        Column3DStarNumberOfSides,

        /// <summary>
        /// The brush used to fill the candlestick Open � Close rectangle if difference 
        /// between Open price and Close price is negative (If this value is positive Fill 
        /// brush from series or data points is used). Default color is black.
        /// </summary>
        CandlestickNegativeFill,

        /// <summary>
        /// The stroke brush used for candlestick Open � Close rectangle if difference 
        /// between Open price and Close price is negative (If this value is positive Fill 
        /// brush from series or data points is used). Default color is gray.
        /// </summary>
        CandlestickNegativeStroke,

        /// <summary>
        /// Used to define radius of the ellipse that is used to round the corners of the 
        /// rectangle. Used for 2D chart which data point shape is rectangle (Column, Bar, 
        /// Stacked Column, Candlestick, etc.)
        /// </summary>
        RectangleRounding,

        /// <summary>
        /// Used to define animation for exploded slice for pie and doughnut chart.
        /// </summary>
        ExplodedAnimation
    }

    /// <summary>
    /// Describes the marker labels behaviour 
    /// when there isn't enough space for them.
    /// </summary>
    public enum LabelOverflow
    {
        /// <summary>
        /// Clip within the grid area.
        /// </summary>
        ClipToGridArea,
        /// <summary>
        /// Fit inside the grid area.
        /// </summary>
        FitInsideGridArea
    }

    #endregion Enum
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