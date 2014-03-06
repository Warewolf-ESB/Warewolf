
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
    /// This class creates 3D line chart. This class is also 
    /// responsible for 3D line chart animation.
    /// </summary>
    class LineChart3D : AreaChart3D
    {
        #region Fields

        private double _edge = 0.01;
        private Point3D _lineCenter;

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

            
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {
                int columnIndx = 0;

                if (seriesList[seriesIndx].ChartType != ChartType.Line)
                {
                    continue;
                }

                // The number of data points
                int pointNum = seriesList[seriesIndx].DataPoints.Count;

                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint curPt = seriesList[seriesIndx].DataPoints[pointIndx];
                    _edge = 0.005 * curPt.GetParameterValueDouble(ChartParameterType.EdgeSize3D);
                    double width = AxisX.GetSize(0.5);
                    
                    if (curPt.NullValue == true || IsOutOfRange(pointIndx + 1, curPt.Value))
                    {
                        continue;
                    }

                    double curY = AxisY.GetPositionLogarithmic(curPt.Value);

                    // Get point depth from chart parameter
                    double depth;
                    GetPointWidth(out depth, AxisX, AxisZ, seriesList[seriesIndx]);

                    // Get brush from point
                    Brush brush = GetBrush(curPt, seriesIndx, pointIndx);

                    columnIndx++;

                    // draw the half of line before the data point except for the first
                    if (pointIndx != 0)
                    {
                        DataPoint prevPt = seriesList[seriesIndx].DataPoints[pointIndx - 1];
                        if (prevPt.NullValue == false && !IsOutOfRange(columnIndx - 0.5, prevPt.Value))
                        {
                            double x = AxisX.GetPosition(columnIndx - 0.5);
                            double prevY = AxisY.GetPositionLogarithmic(prevPt.Value);
                            double diffY = (curY - prevY) / 2;

                            double y = prevY + diffY;

                            double upStart = _edge;
                            double upEnd = diffY + _edge;
                            double downStart = -_edge;
                            double downEnd = diffY - _edge;
                            double z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                            GeometryModel3D geometry = Create(columnIndx, new Point3D(x, y, z), upStart, upEnd, downStart, downEnd, width, depth, brush, rotateTransform3D, curPt, pointIndx, true, 0, 0, 0);
                            SetHitTest3D(geometry, curPt);

                            model3DGroup.Children.Add(geometry);
                        }
                    }
                    // draw the half of line after the data point except for the last
                    if (pointIndx != pointNum - 1)
                    {
                        DataPoint nextPt = seriesList[seriesIndx].DataPoints[pointIndx + 1];
                        if (nextPt.NullValue == false && !IsOutOfRange(columnIndx + 0.5, nextPt.Value))
                        {
                            double x = AxisX.GetPosition(columnIndx);
                            double nextY = AxisY.GetPositionLogarithmic(nextPt.Value);

                            double diffY = (nextY - curY) / 2;

                            double y = curY;

                            double upStart = _edge;
                            double upEnd = diffY + _edge;
                            double downStart = -_edge;
                            double downEnd = diffY - _edge;
                            double z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                            GeometryModel3D geometry = Create(columnIndx, new Point3D(x, y, z), upStart, upEnd, downStart, downEnd, width, depth, brush, rotateTransform3D, curPt, pointIndx, true, 0, 0, 0);
                            SetHitTest3D(geometry, curPt);

                            model3DGroup.Children.Add(geometry);
                        }
                    }
                    
                }
            }
        }

        
        internal GeometryModel3D Create(int index, Point3D center, double upStart, double upEnd, double downStart, double downEnd, double width, double depth, Brush brush, RotateTransform3D rotateTransform3D, DataPoint point, int pointIndex, bool stacked, double splineMax, double splineStart, double splineEnd)
        {
            GeometryModel3D geometryModel = new GeometryModel3D();
            _lineCenter = center;

            // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet 
            // is created.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            if (stacked)
            {
                AreaSideElements(points, normals, textures, indices, upStart, upEnd, downStart, downEnd, width, depth, _edge);
            }
            else
            {
                AreaSides(points, normals, textures, indices, upStart, upEnd, downStart, downEnd, width, depth);
            }

            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TriangleIndices = indices;
            meshGeometry3D.TextureCoordinates = textures;

            MaterialGroup materials = new MaterialGroup();

            DiffuseMaterial colorMaterial = new DiffuseMaterial(brush);
            SpecularMaterial specularMaterial = new SpecularMaterial(brush, 10);


            materials.Children.Add(colorMaterial);
            materials.Children.Add(specularMaterial);
            geometryModel.Material = materials;

            Transform3DGroup transformGroup = new Transform3DGroup();

            if (IsAnimationEnabled(point.GetDoubleAnimation(pointIndex)))
            {
                DoubleAnimation doubleAnimation;
                // Spline chart
                if (splineMax != 0)
                {
                    doubleAnimation = point.GetDoubleAnimationSpline(pointIndex, splineMax, splineStart, splineEnd);
                }
                else // Non spline chart
                {
                    doubleAnimation = point.GetDoubleAnimation(pointIndex);
                }
                               
                ScaleTransform3D scale = new ScaleTransform3D();
                scale.ScaleX = 0;
                scale.ScaleY = 0;

                scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimation);

                transformGroup.Children.Add(scale);
            }



            TranslateTransform3D translate = new TranslateTransform3D(_lineCenter.X, _lineCenter.Y, _lineCenter.Z);

            transformGroup.Children.Add(translate);

            transformGroup.Children.Add(rotateTransform3D);

            geometryModel.Transform = transformGroup;


            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;


            return geometryModel;
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