
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class creates 3D spline chart. This class is also 
    /// responsible for 3D spline chart animation.
    /// </summary>
    internal class SplineChart3D : LineChart3D
    {
        #region Fields

        private double _edge = 0.01;

        #endregion Fields

        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

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

            //Add the geometry model to the model group.
            int columnIndx = 0;
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {
                if (seriesList[seriesIndx].ChartType != ChartType.Spline)
                {
                    continue;
                }

                // The number of data points
                int pointNum = seriesList[seriesIndx].DataPoints.Count;

                if (pointNum < 2)
                {
                    continue;
                }

                Point[] pointList = new Point[pointNum];
                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = seriesList[seriesIndx].DataPoints[pointIndx];

                    double x = AxisX.GetPosition(pointIndx + 1);
                    double y = AxisY.GetLineValue(point.Value);

                    pointList[pointIndx].X = x;
                    pointList[pointIndx].Y = y;
                }

                int[] pointIndexes;
                Point[] spline = SystemDrawing.CreateSplinePoints(pointList, out pointIndexes, true);

                for (int pointIndx = 0; pointIndx < spline.Length - 1; pointIndx++)
                {
                    DataPoint point2 = seriesList[seriesIndx].DataPoints[pointIndexes[pointIndx] + 1];
                    DataPoint point = seriesList[seriesIndx].DataPoints[pointIndexes[pointIndx]];

                    //Skip Null values
                    if (point.NullValue == true || point2.NullValue == true)
                    {
                        continue;
                    }

                    _edge = 0.005 * point.GetParameterValueDouble(ChartParameterType.EdgeSize3D);

                    double x = spline[pointIndx].X;
                    double y = spline[pointIndx].Y;
                    double upStart = _edge;
                    double upEnd = spline[pointIndx+1].Y - spline[pointIndx].Y + _edge;
                    double downStart = -_edge;
                    double downEnd = spline[pointIndx + 1].Y - spline[pointIndx].Y - _edge;
                    double z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                    double width = spline[pointIndx + 1].X - spline[pointIndx].X;

                    // Get point depth from chart parameter
                    double depth;
                    GetPointWidth(out depth, AxisX, AxisZ, seriesList[seriesIndx]);

                    // Get brush from point
                    Brush brush = GetBrush(point, seriesIndx, pointIndexes[pointIndx]);

                    columnIndx++;

                    GeometryModel3D geometry = Create(columnIndx, new Point3D(x, y, z), upStart, upEnd, downStart, downEnd, width, depth, brush, rotateTransform3D, point, pointIndexes[pointIndx], true, spline[spline.Length - 1].X - spline[0].X, spline[pointIndx].X - spline[0].X, spline[pointIndx + 1].X - spline[0].X);

                    SetHitTest3D(geometry, point);

                    model3DGroup.Children.Add(geometry);
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