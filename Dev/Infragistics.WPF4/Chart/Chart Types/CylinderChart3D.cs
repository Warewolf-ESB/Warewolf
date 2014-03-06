
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
    /// This class creates 3D cylinder chart. This class is also 
    /// responsible for 3D cylinder chart animation.
    /// </summary>
    class CylinderChart3D : Primitives3D
    {
        #region Fields

        private double _smooth = 90;

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
                if (seriesList[seriesIndx].ChartType != ChartType.Cylinder)
                {
                    continue;
                }

                // The number of data points
                int pointNum = seriesList[seriesIndx].DataPoints.Count;

                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = seriesList[seriesIndx].DataPoints[pointIndx];

                    double x, y, z;
                    double width, depth, height;

                    x = AxisX.GetPosition(pointIndx + 1);
                    y = AxisY.GetColumnValue(point.Value);
                    z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                    // Skip point if out of range
                    if (IsXOutOfRange(pointIndx + 1 - 0.5) || IsXOutOfRange(pointIndx + 1 + 0.5))
                    {
                        continue;
                    }

                    //Skip Null values
                    if (point.NullValue == true)
                    {
                        continue;
                    }

                    // Get point width and depth from chart parameter
                    GetPointWidth(out width, out depth, AxisX, AxisZ, seriesList[seriesIndx]);
                    height = AxisY.GetColumnHeight(point.Value);

                    // Find edge size which is proportional to the 
                    // width and depth of the column.
                    double size = GetPointSize(width, depth);
                    double edge = GetEdgeSize(size, point);

                    // Get brush from point
                    Brush brush = GetBrush(point, seriesIndx, pointIndx);

                    GeometryModel3D geometry = CreateColumnCylinder(columnIndx, new Point3D(x, y, z), size, height, brush, rotateTransform3D, point.Value < AxisY.Crossing, point, pointIndx, edge);

                    columnIndx++;

                    SetHitTest3D(geometry, point);

                    model3DGroup.Children.Add(geometry);

                    if (point.GetMarker() != null)
                    {
                        GeometryModel3D label = CreateCylinderLabel(columnIndx, new Point3D(x, y, z), size, height, size, brush, rotateTransform3D, point.Value < AxisY.Crossing, point, pointIndx, edge);

                        //Add the label model to the model group.
                        model3DGroup.Children.Add(label);
                    }
                }
            }

        }

        /// <summary>
        /// Calculates the edge size from data point size
        /// </summary>
        /// <param name="size">Data point size</param>
        /// <param name="point">Current Data Point</param>
        /// <returns>The edge size</returns>
        protected double GetEdgeSize(double size, DataPoint point)
        {
            double edge = size * 0.05;

            // Create limit for edge size if small number of data points are used
            if (edge > 0.01)
            {
                edge = 0.01;
            }

            edge *= point.GetParameterValueDouble(ChartParameterType.EdgeSize3D);

            return edge;
        }

        /// <summary>
        /// Calculates the data point size
        /// </summary>
        /// <param name="width">width od the data points scene space</param>
        /// <param name="depth">depth od the data points scene space</param>
        /// <returns>Data point size</returns>
        protected double GetPointSize(double width, double depth)
        {
            return Math.Min(depth, width) / 2 * 0.8;
        }

        internal GeometryModel3D CreateColumnCylinder(int index, Point3D center, double radius, double height, Brush brush, RotateTransform3D rotateTransform3D, bool negativeValue, DataPoint point, int pointIndex, double edge)
        {
            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet 
            // is created.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            if (height == 0)
            {
                return geometryModel;
            }

            CylinderCurve(points, normals, textures, indices, radius, height, edge);
            CylinderTopBottom(points, normals, textures, indices, radius, height, true, edge);
            CylinderTopBottom(points, normals, textures, indices, radius, height, false, edge);
            CylinderEdgeRounded(points, normals, textures, indices, radius, height, true, edge);
            CylinderEdgeRounded(points, normals, textures, indices, radius, height, false, edge);



            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TextureCoordinates = textures;
            meshGeometry3D.TriangleIndices = indices;



            MaterialGroup materials = new MaterialGroup();

            DiffuseMaterial colorMaterial = new DiffuseMaterial(brush);
            SpecularMaterial specularMaterial = new SpecularMaterial(Brushes.White, 32);
          
            //materials.Children.Add(emissiveMaterial);
            materials.Children.Add(colorMaterial);
            // materials.Children.Add(emissiveMaterial);
            materials.Children.Add(specularMaterial);

            geometryModel.Material = materials;

            Transform3DGroup transformGroup = new Transform3DGroup();
            TranslateTransform3D translate = new TranslateTransform3D(center.X, center.Y, center.Z);

            if (IsAnimationEnabled(point.GetDoubleAnimation(pointIndex)))
            {
                DoubleAnimation doubleAnimation = point.GetDoubleAnimation(pointIndex);
                

                DoubleAnimation doubleAnimationStart = new DoubleAnimation();
                doubleAnimationStart.From = 0;
                doubleAnimationStart.To = 1;
                doubleAnimationStart.BeginTime = doubleAnimation.BeginTime;
                doubleAnimationStart.Duration = new Duration(TimeSpan.FromMilliseconds(0));


                ScaleTransform3D scale = new ScaleTransform3D();
                scale.ScaleX = 0;
                scale.ScaleY = 0;
                scale.ScaleZ = 0;

                scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, doubleAnimationStart);
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleZProperty, doubleAnimationStart);

                
                TranslateTransform3D translateAnim = new TranslateTransform3D();
                TranslateTransform3D translateAnim2 = new TranslateTransform3D();

                if (negativeValue)
                {
                    translateAnim.OffsetY = -height;
                    translateAnim2.OffsetY = height;
                }


                transformGroup.Children.Add(translateAnim);
                transformGroup.Children.Add(scale);
                transformGroup.Children.Add(translateAnim2);
            }

            transformGroup.Children.Add(translate);
            transformGroup.Children.Add(rotateTransform3D);
            geometryModel.Transform = transformGroup;
            
            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;


            return geometryModel;
        }


        protected GeometryModel3D CreateCylinderLabel(int index, Point3D center, double width, double height, double depth, Brush brush, RotateTransform3D rotateTransform3D, bool negativeValue, DataPoint point, int pointIndex, double edge)
        {

            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D plane.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            FormattedText text = GetLabelStyle(point);
            double maxCoef = 0.003 / Math.Max(text.Width, text.Extent) * text.Extent;
            double textWidth = text.Width * maxCoef;
            double textHeight = text.Extent * maxCoef;

            if (textHeight > height / 4)
            {
                textHeight = height / 4;
            }

            CreateLabel(points, normals, textures, indices, textWidth, textHeight, depth, 1.0 );

            // Very small data points
            if (textHeight < edge)
            {
                edge = textHeight / 4;
            }

            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TriangleIndices = indices;
            meshGeometry3D.TextureCoordinates = textures;

            // Create Label Brush
            brush = CreateLabelBrush(point);

            MaterialGroup materials = new MaterialGroup();

            DiffuseMaterial colorMaterial = new DiffuseMaterial(brush);

            materials.Children.Add(colorMaterial);
            geometryModel.Material = materials;

            Transform3DGroup transformGroup = new Transform3DGroup();

            TranslateTransform3D translate = new TranslateTransform3D(center.X, center.Y + height - textHeight - 2 * edge, center.Z);
            
            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

            // Create animation
            if (IsAnimationEnabled(point.GetDoubleAnimation(pointIndex)))
            {
                // Create double animations for scaling animation
                DoubleAnimation doubleAnimation = point.GetDoubleAnimation(pointIndex);

                DoubleAnimation translateAnimation = point.GetDoubleAnimation(pointIndex);
                if (point.Value < 0)
                {
                    translateAnimation.From = center.Y + height;
                }
                else
                {
                    translateAnimation.From = center.Y - textHeight;
                }

                translateAnimation.To = translate.OffsetY;
                translate.OffsetY = center.Y - textHeight;

                ScaleTransform3D scale = new ScaleTransform3D();
                scale.ScaleY = 0;

                // Bubble scaled by X, Y and Z axis.
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimation);

                // Column label translated by Y axis only.
                translate.BeginAnimation(TranslateTransform3D.OffsetYProperty, translateAnimation);

                // Create animated transformation group
                transformGroup.Children.Add(scale);
            }

            transformGroup.Children.Add(translate);

            geometryModel.Transform = transformGroup;


            return geometryModel;
        }

        /// <summary>
        /// 
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

            // Points
            for (int segment = 0; segment <= numSegments; segment++)
            {
                // Points
                points.Add(FindRadialPointY(currentAngle, radius, edge));
                points.Add(FindRadialPointY(currentAngle, radius, height - edge));

                // Normals
                normals.Add(FindRadialVectorY(currentAngle, radius, 0));
                normals.Add(FindRadialVectorY(currentAngle, radius, 0));

                // Textures
                texturePoints.Add(new Point(0, textureStep * ((double)segment)));
                texturePoints.Add(new Point(1, textureStep * ((double)segment)));

                currentAngle += smoothStep;
            }

            // Indices
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
                points.Add(new Point3D(0, height, 0));
                normals.Add(new Vector3D(0, 1, 0));
                texturePoints.Add(new Point(0.5, 0.5));
            }
            else
            {
                points.Add(new Point3D(0, 0, 0));
                normals.Add(new Vector3D(0, -1, 0));
                texturePoints.Add(new Point(0.5, 0.5));
            }

            for (int segment = 0; segment <= numSegments; segment++)
            {
                if (top)
                {
                    points.Add(FindRadialPointY(currentAngle, radius - edge, height));
                    normals.Add(new Vector3D(0, 1, 0));
                    texturePoints.Add(new Point(Math.Cos(currentAngle), Math.Sin(currentAngle)));
                }
                else
                {
                    points.Add(FindRadialPointY(currentAngle, radius - edge, 0));
                    normals.Add(new Vector3D(0, -1, 0));
                    texturePoints.Add(new Point(Math.Cos(currentAngle), Math.Sin(currentAngle)));
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

        /// <summary>
        /// This 3D element presents the rounded edge of the cylinder.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="radius">A radius of the cylinder.</param>
        /// <param name="height">A height of the cylinder.</param>
        /// <param name="top">True if the edge is at the top of the cylinder.</param>
        /// <param name="edge">The size of the cylinder edge.</param>
        private void CylinderEdgeRounded(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double radius, double height, bool top, double edge)
        {
            Point3D centerPoint = new Point3D();
            Point3DCollection pointsArc;

            if (top)
            {
                centerPoint.X = radius - edge;
                centerPoint.Z = height - edge;
                pointsArc = GenerateArcPoints(new Point3D(radius - edge, 0, height), new Point3D(radius, 0, height - edge), centerPoint, 2);
            }
            else
            {
                centerPoint.X = radius - edge;
                centerPoint.Z = edge;
                pointsArc = GenerateArcPoints(new Point3D(radius - edge, 0, 0), new Point3D(radius, 0, edge), centerPoint, 2);
            }

            for (int arcSegment = 0; arcSegment < pointsArc.Count - 1; arcSegment++)
            {
                int indicesNum = points.Count;
                double currentAngle = 0;
                double numSegments = _smooth;

                double smoothStep = 2 * Math.PI / numSegments;
                for (int segment = 0; segment <= numSegments; segment++)
                {
                    // Points
                    points.Add(FindRadialPointY(currentAngle, pointsArc[arcSegment].X, pointsArc[arcSegment].Z));
                    points.Add(FindRadialPointY(currentAngle, pointsArc[arcSegment + 1].X, pointsArc[arcSegment + 1].Z));

                    // Normals
                    Point3D pt1 = FindRadialPointY(currentAngle, pointsArc[arcSegment].X, pointsArc[arcSegment].Z);
                    Point3D pt2 = FindRadialPointY(currentAngle, pointsArc[arcSegment + 1].X, pointsArc[arcSegment + 1].Z);
                    Point3D center;
                    if (top)
                    {
                        center = FindRadialPointY(currentAngle, radius - edge, height - edge);
                    }
                    else
                    {
                        center = FindRadialPointY(currentAngle, radius - edge, edge);
                    }

                    normals.Add(Point3D.Subtract(pt1, center));
                    normals.Add(Point3D.Subtract(pt2, center));

                    currentAngle += smoothStep;
                }

                // Indices
                for (int segment = 0; segment <= numSegments; segment++)
                {
                    if (top)
                    {
                        indices.Add(segment * 2 + 0 + indicesNum);
                        indices.Add(segment * 2 + 1 + indicesNum);
                        indices.Add(segment * 2 + 3 + indicesNum);
                        indices.Add(segment * 2 + 0 + indicesNum);
                        indices.Add(segment * 2 + 3 + indicesNum);
                        indices.Add(segment * 2 + 2 + indicesNum);
                    }
                    else
                    {
                        indices.Add(segment * 2 + 0 + indicesNum);
                        indices.Add(segment * 2 + 3 + indicesNum);
                        indices.Add(segment * 2 + 1 + indicesNum);
                        indices.Add(segment * 2 + 0 + indicesNum);
                        indices.Add(segment * 2 + 2 + indicesNum);
                        indices.Add(segment * 2 + 3 + indicesNum);
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