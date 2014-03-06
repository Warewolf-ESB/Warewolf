
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
    /// This class creates 2D stacked area chart. This class is also 
    /// responsible for 2D stacked area chart animation.
    /// </summary>
    internal class StackedAreaChart2D : AreaChart2D
    {
        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type is stacked chart type
        /// </summary>
        override internal bool IsStacked { get { return true; } }

        #endregion ChartTypeparameters

        #region Methods

        /// <summary>
        /// Draws data points for different 2D chart types using Shapes. Draws data points for 
        /// all series which have selected chart type. Creates hit test functionality, 
        /// mouse events and tooltips for data points.
        /// </summary>
        protected override void Draw2D()
        {
            int seriesNum = SeriesList.Count;

            int numOfPoints = GetStackedNumberOfPoints(ChartType.StackedArea, SeriesList);

            bool positiveValues = false;
            bool negativeValues = false;

            double xVal1;
            double xVal2;

            // Data series loop
            for (int pointIndx = 0; pointIndx < numOfPoints - 1; pointIndx++)
            {
                double stackedStart = 0;
                double stackedEnd = 0;

                for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
                {
                    if (SeriesList[seriesIndx].ChartType != ChartType.StackedArea)
                    {
                        continue;
                    }

                    if (SeriesList[seriesIndx].DataPoints.Count <= pointIndx + 1)
                    {
                        // The Data Series of stacked area chart has different number of data points.
                        continue;
                    }

                    DataPoint point1 = SeriesList[seriesIndx].DataPoints[pointIndx];
                    DataPoint point2 = SeriesList[seriesIndx].DataPoints[pointIndx + 1];

                    if (point1.Value > 0 || point2.Value > 0)
                    {
                        positiveValues = true;
                    }

                    if (point1.Value < 0 || point2.Value < 0)
                    {
                        negativeValues = true;
                    }

                    if (positiveValues && negativeValues)
                    {
                        // Stacked area chart can�t have positive and negative data point values together.
                        throw new InvalidOperationException(ErrorString.Exc2);
                    }

                    // Data point X and y position
                    xVal1 = pointIndx + 1;
                    xVal2 = pointIndx + 2;

                    //double maxY = Math.Min(yPos1, yPos2);
                    //double maxHeight = Math.Max(Math.Abs(yPos1 - yPos0), Math.Abs(yPos2 - yPos0));

                    Point[] points = new Point[4];

                    double x1 = this.AxisX.GetPosition(xVal1) - 0.5;
                    double x2 = this.AxisX.GetPosition(xVal2) + 0.5;
                    points[0].X = x1;
                    points[0].Y = AxisY.GetLineValue(stackedStart + point1.Value);
                    points[1].X = x2;
                    points[1].Y = AxisY.GetLineValue(stackedEnd + point2.Value);
                    points[2].X = x2;
                    points[2].Y = AxisY.GetLineValue(stackedEnd);
                    points[3].X = x1;
                    points[3].Y = AxisY.GetLineValue(stackedStart);

                    DrawPoint(seriesIndx, pointIndx, points, point1);
                    
                    stackedStart += point1.Value;
                    stackedEnd += point2.Value;
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