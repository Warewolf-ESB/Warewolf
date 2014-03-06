
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class creates 2D stacked column chart. This class is also 
    /// responsible for 2D stacked column chart animation.
    /// </summary>
    internal class StackedColumnChart2D : ColumnChart2D
    {
        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

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

            double xVal;
            
            // Find max number of data points for column stacked chart type 
            int numOfPoints = Math.Max(GetStackedNumberOfPoints(ChartType.StackedColumn, SeriesList), GetStackedNumberOfPoints(ChartType.StackedCylinder, SeriesList));

            // Search series for point width. Because of cluster and stacked chart types 
            // all series have to have same point width.
            double width = ColumnChart2D.FindClusterPointWidth(ChartType.StackedColumn, ChartType.StackedCylinder, SeriesList, _width);

            // Data point loop
            for (int pointIndx = 0; pointIndx < numOfPoints; pointIndx++)
            {
                // Sum of stacked data point values
                double stackedPositiveY = 0;
                double stackedNegativeY = 0;

                // Data series loop
                for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
                {
                    // Skip all series which don�t have stacked column or stacked cylinder chart type.
                    if ((SeriesList[seriesIndx].ChartType != ChartType.StackedColumn && SeriesList[seriesIndx].ChartType != ChartType.StackedCylinder) || SeriesList[seriesIndx].DataPoints.Count <= pointIndx)
                    {
                        continue;
                    }

                    // Current data point
                    DataPoint point = SeriesList[seriesIndx].DataPoints[pointIndx];

                    //Skip Null values
                    if (point.NullValue == true)
                    {
                        continue;
                    }

                    // Data point X position
                    xVal = pointIndx + 1;

                    double columnWidth = width / 2.0;
                    Rect rect;

                    // Calculate rectangle for positive or negative data point value.
                    if (point.Value > 0)
                    {
                        stackedPositiveY += point.Value;
                        rect = new Rect(
                        AxisX.GetPosition(xVal - columnWidth),
                        AxisY.GetStackedColumnValue(point.Value, stackedPositiveY),
                        AxisX.GetSize(columnWidth * 2),
                        AxisY.GetStackedColumnHeight(point.Value, stackedPositiveY)
                        );
                    }
                    else
                    {
                        stackedNegativeY += point.Value;
                        rect = new Rect(
                        AxisX.GetPosition(xVal - columnWidth),
                        AxisY.GetStackedColumnValue(point.Value, stackedNegativeY),
                        AxisX.GetSize(columnWidth * 2),
                        AxisY.GetStackedColumnHeight(point.Value, stackedNegativeY)
                        );
                    }

                    // Draw or hit test a data point
                    AddShape(rect, point.Value > 0, point, pointIndx, seriesIndx);
                    
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