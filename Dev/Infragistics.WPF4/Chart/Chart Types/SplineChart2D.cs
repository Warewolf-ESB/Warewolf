
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class creates 2D spline chart. This class is also 
    /// responsible for 2D spline chart animation.
    /// </summary>
    internal class SplineChart2D : LineChart2D
    {
        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

        #endregion ChartTypeparameters

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        internal SplineChart2D()
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
                       
            // Data series loop
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {
                Series series = this.SeriesList[seriesIndx];
                if (series.ChartType != ChartType.Spline || series.DataPoints.Count == 0)
                {
                    continue;
                }

                // The number of data points
                int pointNum = series.DataPoints.Count;
                Point[] pointList = new Point[pointNum];
                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = SeriesList[seriesIndx].DataPoints[pointIndx];

                    double x = AxisX.GetPosition(pointIndx + 1);
                    double y = AxisY.GetLineValue(point.Value);

                    pointList[pointIndx].X = x;
                    pointList[pointIndx].Y = y;
                }

                if (pointList.Length < 2)
                {
                    continue;
                }

                int[] pointIndexes;
                Point[] spline = SystemDrawing.CreateSplinePoints(pointList, out pointIndexes, false);

                
                // Data points loop
                if (spline.Length == 2)
                {
                    DataPoint point = series.DataPoints[pointIndexes[0]];
                    DataPoint point2 = series.DataPoints[pointIndexes[1]];
                    if (!point.NullValue && !point2.NullValue)
                    {
                        this.AddShape(new Point[] { spline[0], spline[1] }, point, seriesIndx, pointIndexes[0], 0.0, 0.0, spline[1].X - spline[0].X);
                    }
                }
                else
                {
                    for (int pointIndx = 0; pointIndx < spline.Length - 1; pointIndx++)
                    {
                        // skip if we have reached the final data points and we do not have a valid value for point2
                        if (pointIndexes[pointIndx] + 1 >= series.DataPoints.Count)
                        {
                            continue;
                        }

                        DataPoint point2 = series.DataPoints[pointIndexes[pointIndx] + 1];
                        DataPoint point = series.DataPoints[pointIndexes[pointIndx]];

                        //Skip Null values
                        if (point.NullValue == true || point2.NullValue == true)
                        {
                            continue;
                        }

                        // Data point X and Y position
                        Point[] points = new Point[2];

                        points[0].X = spline[pointIndx].X;
                        points[0].Y = spline[pointIndx].Y;
                        points[1].X = spline[pointIndx + 1].X;
                        points[1].Y = spline[pointIndx + 1].Y;

                        // Draw a data point
                        this.AddShape(points, point, seriesIndx, pointIndexes[pointIndx], spline[spline.Length - 1].X - spline[0].X, spline[pointIndx].X - spline[0].X, spline[pointIndx + 1].X - spline[0].X);

                    }
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