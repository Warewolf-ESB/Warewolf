
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
    /// This class creates 2D stock chart. This class is also 
    /// responsible for 2D stock chart animation.
    /// </summary>
    internal class StockChart2D : ChartSeries
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

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type uses only stroke brush 
        /// and doesn�t use fill brush.
        /// </summary>
        override internal bool IsStrokeMainColor { get { return true; } }

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
                if (series.ChartType == ChartType.Stock)
                {
                    numOfColumnSeries++;
                }
            }

            int seriesNum = SeriesList.Count;

            double xVal;

            // Search series for point width. Because of cluster and stacked chart types 
            // all series have to have same point width.
            double widthDef = ColumnChart2D.FindClusterPointWidth(ChartType.Stock, SeriesList, _width);

            // Data series loop
            int columnIndex = 0;
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {                
                if (SeriesList[seriesIndx].ChartType != ChartType.Stock)
                {
                    continue;
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

                    // Get prices from chart parameters
                    double high = point.GetParameterValueDouble(ChartParameterType.High);
                    double low = point.GetParameterValueDouble(ChartParameterType.Low);
                    double open = point.GetParameterValueDouble(ChartParameterType.Open);
                    double close = point.GetParameterValueDouble(ChartParameterType.Close);
                    
                    // Data point X position
                    xVal = pointIndx + 1;

                    double highPixel = AxisY.GetColumnValue(high);
                    double lowPixel = AxisY.GetColumnValue(low);
                    double openPixel = AxisY.GetColumnValue(open);
                    double closePixel = AxisY.GetColumnValue(close);

                    double width = AxisX.GetSize(widthDef);
                    double x = AxisX.GetPosition(xVal);

                    // Draw or hit test a data point
                    AddShape(highPixel, lowPixel, openPixel, closePixel, x, width, point, pointIndx, seriesIndx);
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
        protected void AddShape(double high, double low, double open, double close, double x, double width, DataPoint point, int pointIndex, int seriesIndex)
        {
            DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);

            // High Low Line
            Line lineHighLow = new Line();
            lineHighLow.X1 = x;
            lineHighLow.Y1 = low;
            lineHighLow.X2 = x;
            lineHighLow.Y2 = high;

            // Open Line
            Line lineOpen = new Line();
            lineOpen.X1 = x;
            lineOpen.Y1 = open;
            lineOpen.X2 = x - width;
            lineOpen.Y2 = open;

            // Close Line
            Line lineClose = new Line();
            lineClose.X1 = x;
            lineClose.Y1 = close;
            lineClose.X2 = x + width;
            lineClose.Y2 = close;

            // Set appearance parameters and event handlers for shapes.
            SetShapeparameters(lineHighLow, point, seriesIndex, pointIndex);
            SetShapeparameters(lineOpen, point, seriesIndex, pointIndex);
            SetShapeparameters(lineClose, point, seriesIndex, pointIndex);
            
            // Animation enabled
            if (IsAnimationEnabled(animation))
            {
                // High Low Line
                lineHighLow.Y2 = low;
                animation.From = low;
                animation.To = high;
                lineHighLow.BeginAnimation(Line.Y2Property, animation);

                // Open Line
                lineOpen.X2 = x;
                animation.From = x;
                animation.To = x - width;
                lineOpen.BeginAnimation(Line.X2Property, animation);

                // Close Line
                lineClose.X2 = x;
                animation.From = x;
                animation.To = x + width;
                lineClose.BeginAnimation(Line.X2Property, animation);
            }

            // Creates hit test info data.
            SetHitTest2D(lineHighLow, point);
            SetHitTest2D(lineOpen, point);
            SetHitTest2D(lineClose, point);

            // Add shapes to UI elements collection
            _elements.Add(lineHighLow);
            _elements.Add(lineOpen);
            _elements.Add(lineClose);
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