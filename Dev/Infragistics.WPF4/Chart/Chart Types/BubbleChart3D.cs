
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
    /// This class creates 3D Bubble chart. This class is also 
    /// responsible for 3D Bubble chart animation.
    /// </summary>
    internal class BubbleChart3D : Primitives3D
    {
        #region Fields

        // The number of segments used to create a sphere. More segments mean better 
        // quality of sphere, but slow performance of the rendering and animation.
        private int _segments = 60;

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

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type 
        /// is Bubble.
        /// </summary>
        override internal bool IsBubble { get { return true; } }

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

            // Bubble max radius in pixels
            double pixelRadius = Math.Min(
                AxisX.GetSize(Math.Abs(AxisX.RoundedMaximum - AxisX.RoundedMinimum) / 3),
                AxisY.GetSize(Math.Abs(AxisY.RoundedMaximum - AxisY.RoundedMinimum) / 3)
            );

            pixelRadius = Math.Min(pixelRadius, AxisZ.GetSize(Math.Abs(AxisZ.RoundedMaximum - AxisZ.RoundedMinimum) / 3));

            // Calculates Max radius value from all data points.
            double maxRadius = FindMaxRadius(new List<Series>(seriesList));

            //Add the geometry model to the model group.
            int bubbleIndx = 0;
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {                
                // Series is not bubble chart
                if (seriesList[seriesIndx].ChartType != ChartType.Bubble)
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
                    radius = pixelRadius / maxRadius * point.GetParameterValueDouble(ChartParameterType.Radius);

                    if (double.IsNaN(radius))
                    {
                        radius = pixelRadius / 2.0;
                    }

                    if (radius < 0)
                    {
                        radius = 0;
                    }
                                     
                    // Get brush from point
                    Brush brush = GetBrush(point, seriesIndx, pointIndx);

                    bubbleIndx++;

                    // Create a bubble for a data point
                    GeometryModel3D geometry = CreateBubble(new Point3D(x, y, z), radius, brush, rotateTransform3D, point, pointIndx, _segments,this);

                    // Create hit test info for this bubble
                    SetHitTest3D(geometry, point);

                    // Add bubble to the group
                    model3DGroup.Children.Add(geometry);

                    // Create labels
                    if (point.GetMarker() != null)
                    {
                        // Create surface with brush which contain a label.
                        GeometryModel3D label = CreateBubbleLabel(new Point3D(x, y, z), radius, brush, point, pointIndx);

                        //Add the label to the model group.
                        model3DGroup.Children.Add(label);
                    }
                }
            }
        }

        /// <summary>
        /// This method creates a rectangle in the 3D space and fills that 
        /// rectangle with a label using drawing brush.
        /// </summary>
        /// <param name="center">The center of the bubble</param>
        /// <param name="radius">The radius of the bubble</param>
        /// <param name="brush">The brush used for bubble</param>
        /// <param name="point">The data point which represents this bubble.</param>
        /// <param name="pointIndex">An index of the data point</param>
        /// <returns>Geometry model of the 3D surface with a label as Drawing brush</returns>
        protected GeometryModel3D CreateBubbleLabel(Point3D center, double radius, Brush brush, DataPoint point, int pointIndex)
        {

            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D plane.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            // Create a text for the label
            FormattedText text = GetLabelStyle(point);
            double maxCoef = 0.003 / Math.Max(text.Width, text.Extent) * text.Extent;
            double textWidth = text.Width * maxCoef;
            double textHeight = text.Extent * maxCoef;

            if (textHeight > radius / 2)
            {
                textHeight = radius / 2;
            }

            // Creates geometry 3D model with rectangle in the 3D space 
            // which will be container of the label.
            CreateLabel(points, normals, textures, indices, textWidth, textHeight, radius, 1.0);
            
            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TriangleIndices = indices;
            meshGeometry3D.TextureCoordinates = textures;

            // Create Label's Drawing Brush
            brush = CreateLabelBrush(point);

            // Create materials
            MaterialGroup materials = new MaterialGroup();
            DiffuseMaterial colorMaterial = new DiffuseMaterial(brush);
            materials.Children.Add(colorMaterial);
            geometryModel.Material = materials;

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

            // Create transformations
            Transform3DGroup transformGroup = new Transform3DGroup();
            TranslateTransform3D translate = new TranslateTransform3D(center.X, center.Y, center.Z);

            // Create animation
            if (IsAnimationEnabled(point.GetDoubleAnimation(pointIndex)))
            {
                // Create double animations for scaling animation
                DoubleAnimation doubleAnimation = point.GetDoubleAnimation(pointIndex);
                
                ScaleTransform3D scale = new ScaleTransform3D();
                scale.ScaleX = 0;
                scale.ScaleY = 0;
                scale.ScaleZ = 0;

                // Bubble scaled by X, Y and Z axis.
                scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleZProperty, doubleAnimation);

                // Create animated transformation group
                transformGroup.Children.Add(scale);
            }

            // Set position of the sphere on the scene.
            transformGroup.Children.Add(translate);
            geometryModel.Transform = transformGroup;

            return geometryModel;
        }

        /// <summary>
        /// Creates one sphere which represents a data point. This method animates the 
        /// sphere and creates material and transformation matrix.
        /// </summary>
        /// <param name="center">The center of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="brush">The brush used for the sphere</param>
        /// <param name="rotateTransform3D">3D Rotation angles of the scene</param>
        /// <param name="point">The data point which represents this bubble</param>
        /// <param name="pointIndex">An index of the data point</param>
        /// <param name="segments">The number of segments used to create a sphere. More segments mean better quality of sphere, but slow performance of the rendering and animation.</param>
        /// <param name="chartSeries">Chart series which produce Bubble</param>
        /// <returns>Geometry model of the 3D sphere</returns>
        static internal GeometryModel3D CreateBubble(Point3D center, double radius, Brush brush, RotateTransform3D rotateTransform3D, DataPoint point, int pointIndex, int segments, ChartSeries chartSeries)
        {
            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D Bubble.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

            // Normal vectors, mesh points and indices.
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            // Create a Sphere which represents 3D Bubble
            AddSphere(points, normals, textures, indices, radius, segments);

            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TriangleIndices = indices;
            meshGeometry3D.TextureCoordinates = textures;

            // Create matirials
            MaterialGroup materials = new MaterialGroup();
            DiffuseMaterial colorMaterial = new DiffuseMaterial(brush);
            SpecularMaterial specularMaterial = new SpecularMaterial(Brushes.White, 16);

            materials.Children.Add(colorMaterial);
            materials.Children.Add(specularMaterial);
            geometryModel.Material = materials;

            // Create transformations
            Transform3DGroup transformGroup = new Transform3DGroup();
            TranslateTransform3D translate = new TranslateTransform3D(center.X, center.Y, center.Z);

            // Create animation
            if (chartSeries.IsAnimationEnabled(point.GetDoubleAnimation(pointIndex)))
            {
                // Create double animations for scaling animation
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

                // Bubble scaled by X, Y and Z axis.
                scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleZProperty, doubleAnimation);
                                
                // Create animated transformation group
                transformGroup.Children.Add(scale);
            }

            // Set position of the sphere on the scene.
            transformGroup.Children.Add(translate);
            transformGroup.Children.Add(rotateTransform3D);
            geometryModel.Transform = transformGroup;

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

            return geometryModel;

        }

        /// <summary>
        /// Create mesh for a sphere.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="radius">The sphere radius</param>
        /// <param name="segments">The number of segments used to create a sphere. More segments mean better quality of sphere, but slow performance of the rendering and animation.</param>
        static private void AddSphere(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double radius, int segments)
        {
            double stepY = 2.0 * radius / (double)segments;
            double stepAlpha = 2.0 * Math.PI / (double)segments;

            // Y Position loop
            for (int yIndex = 0; yIndex <= segments; yIndex++)
            {
                double y = -radius + yIndex * stepY;

                // The Angle loop. Alpha angle belongs to XZ plane.
                for (int alphaIndex = 0; alphaIndex <= segments; alphaIndex++)
                {
                    // Create Points, normals and texture points.
                    double alpha = alphaIndex * stepAlpha;

                    points.Add(SpherePosition(alpha, y, radius));
                    normals.Add((Vector3D)SpherePosition(alpha, y, radius));
                    texturePoints.Add(SphereTexturePosition(alpha, y, radius));

                    // Do not set indices for the last indexes.
                    if (alphaIndex == segments || yIndex == segments)
                    {
                        continue;
                    }

                    // Create indices for mesh points
                    int ptLT = alphaIndex + yIndex * segments + yIndex;
                    int ptRT = alphaIndex + 1 + yIndex * segments + yIndex;
                    int ptLB = alphaIndex + (yIndex + 1) * (segments + 1);
                    int ptRB = alphaIndex + 1 + (yIndex + 1) * (segments + 1);

                    // First triangle
                    indices.Add(ptLT);
                    indices.Add(ptLB);
                    indices.Add(ptRT);

                    // Second triangle
                    indices.Add(ptRT);
                    indices.Add(ptLB);
                    indices.Add(ptRB);
                }
            }
        }
        
        /// <summary>
        /// Get position of sphere mesh points using y coordinate 
        /// and alpha angle.
        /// </summary>
        /// <param name="alpha">The angle in XZ plane</param>
        /// <param name="y">Y position</param>
        /// <param name="radius">The sphere radius</param>
        /// <returns>The point position</returns>
        static internal Point3D SpherePosition(double alpha, double y, double radius)
        {
            // Radius of the circle in XZ plane (Radius of the sphere projection on XZ).
            double rProjection = Math.Sqrt(radius * radius - y * y);
           
            return new Point3D(rProjection * Math.Cos(alpha), y, rProjection * Math.Sin(alpha));
        }

        /// <summary>
        /// Finds the position of a 2D point mapped to 
        /// a texture from 3D space.
        /// </summary>
        /// <param name="alpha">The position of the 3D point using angle in XZ plane.</param>
        /// <param name="y">The position of the 3D point using Y coordinate.</param>
        /// <param name="radius">The sphere radius</param>
        /// <returns>Mapped position on the texture.</returns>
        static private Point SphereTexturePosition(double alpha, double y, double radius)
        {
            Point txtPosition = new Point();
            txtPosition.X = 1 - alpha / (2 * Math.PI);
            txtPosition.Y = 1 - (radius + y) / (2 * radius);
            
            return txtPosition;
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