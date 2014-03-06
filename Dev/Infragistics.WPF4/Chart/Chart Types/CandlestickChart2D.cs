
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class creates 2D Candlestick chart. This class is also 
    /// responsible for 2D Candlestick chart animation.
    /// </summary>
    internal class CandlestickChart2D : ChartSeries
    {
        #region Fields

        // Private fields
        protected double _width = 0.4;

        #endregion fields

        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type is stock type (High, Low, Open, Close)
        /// </summary>
        override internal bool IsStock { get { return true; } }

        #endregion ChartTypeparameters

        #region Methods

        /// <summary>
        /// Draws data points for different 2D chart types using Shapes. Draws data points for 
        /// all series which have selected chart type. Creates hit test functionality, 
        /// mouse events and tooltips for data points.
        /// </summary>
        protected override void Draw2D()
        {            
            int numOfColumnSeries = 0;
            foreach (Series series in SeriesList)
            {
                if (series.ChartType == ChartType.Candlestick)
                {
                    numOfColumnSeries++;
                }
            }

            int seriesNum = SeriesList.Count;

            // Search series for point width. Because of cluster and stacked chart types 
            // all series have to have same point width.
            double widthDef = ColumnChart2D.FindClusterPointWidth(ChartType.Candlestick, SeriesList, _width);

            double xVal;

            // Data series loop
            int columnIndex = 0;
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {                
                if (SeriesList[seriesIndx].ChartType != ChartType.Candlestick)
                {
                    continue;
                }

                if (SeriesList[seriesIndx].DataPointColor == DataPointColor.Different)
                {
                    // Candlestick chart does not support different data point colors. The Series.DataPointColor cannot be set to DataPointColor.Different for this chart type.
                    throw new InvalidOperationException(ErrorString.Exc60);
                }


                // The number of data points
                int pointNum = SeriesList[seriesIndx].DataPoints.Count;

                // Data points loop
                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = SeriesList[seriesIndx].DataPoints[pointIndx];

                    //Skip Null values
                    if (point.NullValue == true)
                    {
                        continue;
                    }

                    // Get negative color
                    Brush negativeBrush = point.GetParameterValueBrush(ChartParameterType.CandlestickNegativeFill);
                    Brush negativeStrokeBrush = point.GetParameterValueBrush(ChartParameterType.CandlestickNegativeStroke);

                    // Get prices from chart parameters
                    double high = point.GetParameterValueDouble(ChartParameterType.High);
                    double low = point.GetParameterValueDouble(ChartParameterType.Low);
                    double open = point.GetParameterValueDouble(ChartParameterType.Open);
                    double close = point.GetParameterValueDouble(ChartParameterType.Close);

                    // Data point X position
                    xVal = pointIndx + 1;

                    double highPixel = AxisY.GetColumnValue(high, false);
                    double lowPixel = AxisY.GetColumnValue(low, false);
                    double openPixel = AxisY.GetColumnValue(open, false);
                    double closePixel = AxisY.GetColumnValue(close, false);

                    double width = AxisX.GetSize(widthDef);
                    double x = AxisX.GetPosition(xVal);

                    // Draw or hit test a data point
                    AddShape(highPixel, lowPixel, openPixel, closePixel, x, width, point, pointIndx, seriesIndx, negativeBrush, negativeStrokeBrush);
                }
                columnIndex++;
            }
        }

        /// <summary>
        /// This method draws data points as shapes.
        /// </summary>
        /// <param name="high">Pixel y position of High price</param>
        /// <param name="low">Pixel y position of Low price</param>
        /// <param name="open">Pixel y position of Open price</param>
        /// <param name="close">Pixel y position of Close price</param>
        /// <param name="x">Pixel x position for all prices</param>
        /// <param name="width">The width of data point space</param>
        /// <param name="point">Coresponding data point.</param>
        /// <param name="pointIndex">Current data point index.</param>
        /// <param name="seriesIndex">Current series index.</param>
        /// <param name="negativeBrush">The brush used to fill the candlestick Open � Close rectangle if difference between Open price and Close price is negative.</param>
        /// <param name="negativeStrokeBrush">The stroke brush used for candlestick Open � Close rectangle if difference between Open price and Close price is negative.</param>
        protected void AddShape(double high, double low, double open, double close, double x, double width, DataPoint point, int pointIndex, int seriesIndex, Brush negativeBrush, Brush negativeStrokeBrush)
        {
            DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);

            // High Low Line
            Line lineHighLow = new Line();
            lineHighLow.X1 = x;
            lineHighLow.Y1 = low;
            lineHighLow.X2 = x;
            lineHighLow.Y2 = high;

            Rectangle rectOpenClose = new Rectangle();
            rectOpenClose.Height = Math.Abs(open - close);
            rectOpenClose.Width = 2 * width;
            Canvas.SetLeft(rectOpenClose, x - width);
            Canvas.SetTop(rectOpenClose, Math.Min(open, close));

            // Set appearance parameters and event handlers for shapes.
            SetShapeparameters(lineHighLow, point, seriesIndex, pointIndex);
            SetShapeparameters(rectOpenClose, point, seriesIndex, pointIndex);

            if (open < close)
            {
                rectOpenClose.Fill = negativeBrush;
                rectOpenClose.Stroke = negativeStrokeBrush;
                lineHighLow.Stroke = negativeStrokeBrush;
            }
            
            // Animation enabled
            if (IsAnimationEnabled(animation))
            {
                // High Low Line
                lineHighLow.Y2 = low;
                animation.From = low;
                animation.To = high;
                lineHighLow.BeginAnimation(Line.Y2Property, animation);

                rectOpenClose.Height = 0;
                animation.From = 0;
                animation.To = Math.Abs(open - close);
                rectOpenClose.BeginAnimation(Rectangle.HeightProperty, animation);
            }

            // Creates hit test info data.
            SetHitTest2D(lineHighLow, point);
            SetHitTest2D(rectOpenClose, point);

            // Add shapes to UI elements collection
            _elements.Add(lineHighLow);
            _elements.Add(rectOpenClose);
        }
      
        #endregion Methods
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