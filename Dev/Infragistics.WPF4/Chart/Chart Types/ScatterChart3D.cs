
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
    /// This class creates 3D scatter chart. This class is also 
    /// responsible for 3D scatter chart animation.
    /// </summary>
    class ScatterChart3D : PointChart3D
    {
        #region Fields

        #endregion Fields

        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type 
        /// uses all value axes (scatter chart).
        /// </summary>
        override internal bool IsScatter { get { return true; } }

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

            double x;
            double y;
            double z;
            double radius;
            double minRadius = Math.Min(
                AxisX.GetSize(Math.Abs(AxisX.RoundedMaximum - AxisX.RoundedMinimum) / 10),
                AxisY.GetSize(Math.Abs(AxisY.RoundedMaximum - AxisY.RoundedMinimum) / 10)
            );

            double pixelRadius = Math.Min(AxisX.GetSize(AxisX.RoundedInterval), AxisY.GetSize(AxisY.RoundedInterval));
            pixelRadius = Math.Min(pixelRadius, AxisZ.GetSize(AxisZ.RoundedInterval));

            if (pixelRadius < minRadius)
            {
                pixelRadius = minRadius;
            }

            //Add the geometry model to the model group.
            int markerIndx = 0;
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {                
                // Series is not Scatter chart
                if (seriesList[seriesIndx].ChartType != ChartType.Scatter)
                {
                    continue;
                }

                // The number of data points
                int pointNum = seriesList[seriesIndx].DataPoints.Count;

                // Data points loop
                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = seriesList[seriesIndx].DataPoints[pointIndx];

                    //Skip Null values
                    if (point.NullValue == true)
                    {
                        continue;
                    }

                    // Data point X, Y and Z position
                    x = AxisX.GetPositionLogarithmic(point.GetParameterValueDouble(ChartParameterType.ValueX));
                    y = AxisY.GetPositionLogarithmic(point.GetParameterValueDouble(ChartParameterType.ValueY));
                    z = AxisZ.GetPositionLogarithmic(point.GetParameterValueDouble(ChartParameterType.ValueZ));

                    // A bubble radius
                    radius = pixelRadius * 0.5;

                    double maxRadius = AxisX.GetSize(Math.Abs(AxisX.RoundedMaximum - AxisX.RoundedMinimum) / 10);
                    if (radius > maxRadius)
                    {
                        radius = maxRadius;
                    }
                                      
                    // Get brush from point
                    Brush brush = GetBrush(point, seriesIndx, pointIndx);

                    markerIndx++;

                    // Create a marker for a data point
                    GeometryModel3D geometry;
                    if (point.GetMarker() != null)
                    {
                        radius *= point.GetMarker().MarkerSize / 2;
                    }

                    if (point.GetMarker() == null || point.GetMarker().Type == MarkerType.Circle)
                    {
                        geometry = BubbleChart3D.CreateBubble(new Point3D(x, y, z), radius / 2.0, brush, rotateTransform3D, point, pointIndx, 30, this);
                    }
                    else
                    {
                        geometry = CreateMarkerPoint(markerIndx, new Point3D(x, y, z), radius, radius, radius, brush, rotateTransform3D, point, pointIndx);
                    }

                    // Create hit test info for this bubble
                    SetHitTest3D(geometry, point);

                    // Add bubble to the group
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