
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
    /// This class creates 3D scatter line chart. This class is also 
    /// responsible for 3D scatter line chart animation.
    /// </summary>
    class ScatterLineChart3D : LineChart3D
    {
        #region Fields

        //protected double _edge = 0.01;
        private double _smooth = 90;

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

            int columnIndx = 0;
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {                
                if (seriesList[seriesIndx].ChartType != ChartType.ScatterLine)
                {
                    continue;
                }

                // The number of data points
                int pointNum = seriesList[seriesIndx].DataPoints.Count;

                for (int pointIndx = 0; pointIndx < pointNum - 1; pointIndx++)
                {
                    DataPoint point1 = seriesList[seriesIndx].DataPoints[pointIndx];
                    DataPoint point2 = seriesList[seriesIndx].DataPoints[pointIndx + 1];

                    //Skip Null values
                    if (point1.NullValue == true || point2.NullValue == true)
                    {
                        continue;
                    }

                    // Data point X, Y and Z position
                    double x1 = AxisX.GetPositionLogarithmic(point1.GetParameterValueDouble(ChartParameterType.ValueX));
                    double y1 = AxisY.GetPositionLogarithmic(point1.GetParameterValueDouble(ChartParameterType.ValueY));
                    double z1 = AxisZ.GetPositionLogarithmic(point1.GetParameterValueDouble(ChartParameterType.ValueZ));

                    double x2 = AxisX.GetPositionLogarithmic(point2.GetParameterValueDouble(ChartParameterType.ValueX));
                    double y2 = AxisY.GetPositionLogarithmic(point2.GetParameterValueDouble(ChartParameterType.ValueY));
                    double z2 = AxisZ.GetPositionLogarithmic(point2.GetParameterValueDouble(ChartParameterType.ValueZ));

                    // Get brush from point
                    Brush brush = GetBrush(point1, seriesIndx, pointIndx);

                    columnIndx++;

                    GeometryModel3D geometry = Create(columnIndx, x1, y1, z1, x2, y2, z2, brush, rotateTransform3D, point1, pointIndx);

                    SetHitTest3D(geometry, point1);

                    model3DGroup.Children.Add(geometry);
                }
            }
        }


        private GeometryModel3D Create(int index, double x1, double y1, double z1, double x2, double y2, double z2, Brush brush, RotateTransform3D rotateTransform3D, DataPoint point, int pointIndex)
        {
            GeometryModel3D geometryModel = new GeometryModel3D();
            
            // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet 
            // is created.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            double size = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) + (z2 - z1) * (z2 - z1));
            double edge = 0.01 * point.GetParameterValueDouble(ChartParameterType.EdgeSize3D);
            CreateColumnCylinder(points, normals, textures, indices, index, new Point3D(x1, y1, z1), edge, size, brush, rotateTransform3D, true, point, pointIndex);
            
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
                DoubleAnimation doubleAnimation = point.GetDoubleAnimation(pointIndex);
                DoubleAnimation doubleAnimationY = point.GetDoubleAnimation(pointIndex);
                doubleAnimationY.Duration = new Duration(new TimeSpan(0));
                               
                ScaleTransform3D scale = new ScaleTransform3D();
                scale.ScaleX = 0;
                scale.ScaleY = 0;

                scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimationY);

                transformGroup.Children.Add(scale);
            }

            double angle = Vector3D.AngleBetween(new Vector3D(1, 0, 0), new Vector3D(x2 - x1, y2 - y1, z2 - z1));

            Vector3D vv = Vector3D.CrossProduct(new Vector3D(1, 0, 0), new Vector3D(x2 - x1, y2 - y1, z2 - z1));

            //Define a rotation
            RotateTransform3D myRotateTransform = new RotateTransform3D(new AxisAngleRotation3D(vv, angle));

            transformGroup.Children.Add(myRotateTransform);
            
            TranslateTransform3D translate = new TranslateTransform3D(x1, y1, z1);

            
            transformGroup.Children.Add(translate);

            transformGroup.Children.Add(rotateTransform3D);

            geometryModel.Transform = transformGroup;


            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;


            return geometryModel;
        }

        internal void CreateColumnCylinder(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, int index, Point3D center, double radius, double height, Brush brush, RotateTransform3D rotateTransform3D, bool negativeValue, DataPoint point, int pointIndex)
        {
            CylinderCurve(points, normals, texturePoints, indices, radius, height, 0);
            CylinderTopBottom(points, normals, texturePoints, indices, radius, height, true, 0);
            CylinderTopBottom(points, normals, texturePoints, indices, radius, height, false, 0);
        }

        /// <summary>
        /// Create 3D cylinder curve
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="radius">A radius of the cylinder.</param>
        /// <param name="height">A height of the cylinder.</param>
        /// <param name="edge">The size of the cylinder edge.</param>
        private void CylinderCurve(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double radius, double height, double edge)
        {
            int indicesNum = points.Count;
            double currentAngle = 0;
            double numSegments = _smooth;

            double smoothStep = 2 * Math.PI / numSegments;
            double textureStep = 1.0 / numSegments;

            for (int segment = 0; segment <= numSegments; segment++)
            {
                // Points
                points.Add(FindRadialPointX(currentAngle, radius, edge));
                points.Add(FindRadialPointX(currentAngle, radius, height - edge));

                // Normals
                normals.Add(FindRadialVectorX(currentAngle, radius, 0));
                normals.Add(FindRadialVectorX(currentAngle, radius, 0));

                // Textures
                texturePoints.Add(new Point(0, textureStep * ((double)segment)));
                texturePoints.Add(new Point(1, textureStep * ((double)segment)));

                currentAngle += smoothStep;
            }
            for (int segment = 0; segment < numSegments * 2; segment += 2)
            {
                // Indices
                indices.Add(segment + 2 + indicesNum);
                indices.Add(segment + 1 + indicesNum);
                indices.Add(segment + 0 + indicesNum);
                indices.Add(segment + 2 + indicesNum);
                indices.Add(segment + 3 + indicesNum);
                indices.Add(segment + 1 + indicesNum);
            }
        }

        /// <summary>
        /// This 3D element presents the top or the bottom of the cylinder.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="radius">A radius of the cylinder.</param>
        /// <param name="height">A height of the cylinder.</param>
        /// <param name="top">True if this is the top of the cylinder, false for bottom.</param>
        /// <param name="edge">The size of the cylinder edge.</param>
        private void CylinderTopBottom(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double radius, double height, bool top, double edge)
        {
            int indicesNum = points.Count;
            double currentAngle = 0;
            double numSegments = _smooth;

            double smoothStep = 2 * Math.PI / numSegments;

            if (top)
            {
                points.Add(new Point3D(height, 0, 0));
                normals.Add(new Vector3D(1, 0, 0));
                texturePoints.Add(new Point(0.5, 0.5));
            }
            else
            {
                points.Add(new Point3D(0, 0, 0));
                normals.Add(new Vector3D(-1, 0, 0));
                texturePoints.Add(new Point(0.5, 0.5));
            }

            for (int segment = 0; segment <= numSegments; segment++)
            {
                if (top)
                {
                    points.Add(FindRadialPointX(currentAngle, radius - edge, height));
                    normals.Add(new Vector3D(1, 0, 0));
                }
                else
                {
                    points.Add(FindRadialPointX(currentAngle, radius - edge, 0));
                    normals.Add(new Vector3D(-1, 0, 0));
                }

                currentAngle += smoothStep;

            }

            for (int segment = 0; segment < numSegments; segment++)
            {
                if (top)
                {
                    indices.Add(0 + indicesNum);
                    indices.Add(segment + 1 + indicesNum);
                    indices.Add(segment + 2 + indicesNum);

                }
                else
                {
                    indices.Add(0 + indicesNum);
                    indices.Add(segment + 2 + indicesNum);
                    indices.Add(segment + 1 + indicesNum);
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