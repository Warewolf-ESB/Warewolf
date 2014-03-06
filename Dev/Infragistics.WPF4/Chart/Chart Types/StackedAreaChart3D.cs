
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Shapes;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class creates 3D stacked area chart. This class is also 
    /// responsible for 3D stacked area chart animation.
    /// </summary>
    internal class StackedAreaChart3D : AreaChart3D
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
        /// Draws data points for different 3D chart types using 3D models. Creates data points 
        /// as 3D shapes for all series which have selected chart type. Creates hit test functionality 
        /// and tooltips for data points.
        /// </summary>
        /// <param name="model3DGroup">Model 3D group which keeps all column 3D objects.</param>
        /// <param name="rotateTransform3D">3D Rotation angles of the scene.</param>
        protected override void Draw3D(Model3DGroup model3DGroup, RotateTransform3D rotateTransform3D)
        {
            SeriesCollection seriesList = GetSeries();

            int seriesNum = seriesList.Count;

            int numOfPoints = GetStackedNumberOfPoints(ChartType.StackedArea, seriesList);

            bool positiveValues = false;
            bool negativeValues = false;

            //Add the geometry model to the model group.
            int columnIndx = 0;
            for (int pointIndx = 0; pointIndx < numOfPoints - 1; pointIndx++)
            {
                double stackedStart = 0;
                double stackedEnd = 0;

                for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
                {
                    if (seriesList[seriesIndx].ChartType != ChartType.StackedArea)
                    {
                        continue;
                    }

                    if (seriesList[seriesIndx].DataPoints.Count <= pointIndx + 1)
                    {
                        // The Data Series of a stacked chart has different number of data points.
                        continue;
                    }

                    // Skip point if out of range
                    if (IsXOutOfRange(pointIndx + 1) || IsXOutOfRange(pointIndx + 2))
                    {
                        continue;
                    }

                    DataPoint point1 = seriesList[seriesIndx].DataPoints[pointIndx];
                    DataPoint point2 = seriesList[seriesIndx].DataPoints[pointIndx + 1];

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

                    double x = AxisX.GetPosition(pointIndx + 1);

                    double upStart, upEnd, downStart, downEnd;
                    if (positiveValues)
                    {
                        upStart = AxisY.GetLineValue(stackedStart + point1.Value);
                        upEnd = AxisY.GetLineValue(stackedEnd + point2.Value);
                        downStart = AxisY.GetLineValue(stackedStart);
                        downEnd = AxisY.GetLineValue(stackedEnd);
                    }
                    else
                    {
                        downStart = AxisY.GetLineValue(stackedStart + point1.Value);
                        downEnd = AxisY.GetLineValue(stackedEnd + point2.Value);
                        upStart = AxisY.GetLineValue(stackedStart);
                        upEnd = AxisY.GetLineValue(stackedEnd);
                    }

                    stackedStart += point1.Value;
                    stackedEnd += point2.Value;

                    double z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);
                    double width = AxisX.GetSize(1);

                    // Get point depth from chart parameter
                    double depth;
                    GetPointWidth(out depth, AxisX, AxisZ, seriesList[seriesIndx]);

                    // Get brush from point
                    Brush brush = GetBrush(point1, seriesIndx, pointIndx);

                    columnIndx++;

                    GeometryModel3D geometry = Create(columnIndx, new Point3D(x, 0, z), upStart, upEnd, downStart, downEnd, width, depth, brush, rotateTransform3D, point1, pointIndx, true);

                    SetHitTest3D(geometry, point1);

                    model3DGroup.Children.Add(geometry);
                }

            }

            // Draw point labels
            for (int pointIndx = 0; pointIndx < numOfPoints; pointIndx++)
            {
                // Draw point labels
                double stacked = 0;
                double prevStacked = 0;
                for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
                {
                    if (seriesList[seriesIndx].ChartType != ChartType.StackedArea)
                    {
                        continue;
                    }

                    if (seriesList[seriesIndx].DataPoints.Count <= pointIndx)
                    {
                        // The Data Series of a stacked chart has different number of data points.
                        continue;
                    }

                    // Skip point if out of range
                    if (IsXOutOfRange(pointIndx + 1) || IsXOutOfRange(pointIndx + 2))
                    {
                        continue;
                    }

                    DataPoint point = seriesList[seriesIndx].DataPoints[pointIndx];

                    if (point.GetMarker() != null)
                    {
                        stacked += point.Value;

                        double x = AxisX.GetPosition(pointIndx + 1);
                        double y = (AxisY.GetLineValue(stacked) + AxisY.GetLineValue(prevStacked)) / 2.0;
                        double z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                        double width = AxisX.GetSize(0.5);
                        double height = Math.Abs(AxisY.GetLineValue(stacked) - AxisY.GetLineValue(prevStacked));

                        prevStacked = stacked;

                        // Get point depth from chart parameter
                        double depth;
                        GetPointWidth(out depth, AxisX, AxisZ, seriesList[seriesIndx]);

                        // Get brush from point
                        Brush brush = GetBrush(point, seriesIndx, pointIndx);

                        columnIndx++;

                        GeometryModel3D label = CreateAreaLabel(columnIndx, new Point3D(x, y, z), width, height, depth, brush, rotateTransform3D, point, pointIndx, 0.01);

                        //Add the label model to the model group.
                        model3DGroup.Children.Add(label);
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