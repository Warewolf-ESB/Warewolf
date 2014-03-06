
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
    /// This class creates 2D scatter line chart. This class is also 
    /// responsible for 2D scatter line chart animation.
    /// </summary>
    internal class ScatterLineChart2D : LineChart2D
    {
        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type uses only stroke brush 
        /// and doesn�t use fill brush.
        /// </summary>
        override internal bool IsStrokeMainColor { get { return true; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type 
        /// uses all value axes (scatter chart).
        /// </summary>
        override internal bool IsScatter { get { return true; } }

        #endregion ChartTypeparameters

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        internal ScatterLineChart2D()
        {
        }

        /// <summary>
        /// Draws data points for different 2D chart types using Shapes. Draws data points for 
        /// all series which have selected chart type. Creates hit test functionality, 
        /// mouse events and tooltips for data points.
        /// </summary>
        protected override void Draw2D()
        {           
            int seriesNum = SeriesList.Count;

            double xVal1;
            double yVal1;
            double xVal2;
            double yVal2;

            // Data series loop
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {                
                if (SeriesList[seriesIndx].ChartType != ChartType.ScatterLine)
                {
                    continue;
                }

                // The number of data points
                int pointNum = SeriesList[seriesIndx].DataPoints.Count;

                // Activate fast algorithm when series have more than 1000 data points.
                bool fastLine = false;
                bool[] fastFlags = new bool[pointNum];
                if (pointNum > 1000 && !AxisX.Logarithmic && !AxisY.Logarithmic && Chart != null && Chart.GetContainer().Scene != null && Chart.GetContainer().Scene.DataFilter)
                {
                    fastFlags = FastLine(SeriesList[seriesIndx],true);
                    fastLine = true;
                }

                // Data points loop
                int firstPointIndex = 0;
                for (int pointIndx = 0; pointIndx < pointNum - 1; pointIndx++)
                {
                    DataPoint point1;
                    DataPoint point2;

                    if (fastLine)
                    {
                        if (fastFlags[pointIndx + 1] || pointIndx==0)
                        {
                            point1 = SeriesList[seriesIndx].DataPoints[firstPointIndex];
                            point2 = SeriesList[seriesIndx].DataPoints[pointIndx + 1];
                            firstPointIndex = pointIndx + 1;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        point1 = SeriesList[seriesIndx].DataPoints[pointIndx];
                        point2 = SeriesList[seriesIndx].DataPoints[pointIndx + 1];
                    }

                    // Data point X and y position
                    xVal1 = point1.GetParameterValueDouble(ChartParameterType.ValueX);
                    xVal2 = point2.GetParameterValueDouble(ChartParameterType.ValueX);
                    yVal1 = point1.GetParameterValueDouble(ChartParameterType.ValueY);
                    yVal2 = point2.GetParameterValueDouble(ChartParameterType.ValueY);
                                        
                    Point[] points = new Point[2];

                    // Skip pointa if out of range
                    if (IsXOutOfRange(xVal1) || IsXOutOfRange(xVal2))
                    {
                        continue;
                    }

                    //Skip Null values
                    if (point1.NullValue == true || point2.NullValue == true)
                    {
                        continue;
                    }

                    points[0].X = AxisX.GetPositionLogarithmic(xVal1);
                    points[0].Y = AxisY.GetPositionLogarithmic(yVal1);
                    points[1].X = AxisX.GetPositionLogarithmic(xVal2);
                    points[1].Y = AxisY.GetPositionLogarithmic(yVal2);
                    
                    // Draw a data point
                    AddShape(points, point1, seriesIndx, pointIndx, 0, 0, 0);
                    
                }
            }
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