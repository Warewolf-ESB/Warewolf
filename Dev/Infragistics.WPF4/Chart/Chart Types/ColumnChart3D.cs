
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
    /// This class creates 3D column chart. This class is also 
    /// responsible for 3D column chart animation.
    /// </summary>
    internal class ColumnChart3D : ColumnBar3DShapes
    {
        #region Fields

        private bool _star;

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
                // Series is not column chart
                if (seriesList[seriesIndx].ChartType != ChartType.Column)
                {
                    continue;
                }

                // The number of data points
                int pointNum = seriesList[seriesIndx].DataPoints.Count;

                // Data points loop
                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = seriesList[seriesIndx].DataPoints[pointIndx];

                    double x, y, z;
                    double width, depth, height;

                    // A column position
                    x = AxisX.GetPosition(pointIndx + 1);
                    y = AxisY.GetColumnValue(point.Value);
                    z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                    // Get point width and depth from chart parameter
                    GetPointWidth(out width, out depth, AxisX, AxisZ, seriesList[seriesIndx]);

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

                    height = AxisY.GetColumnHeight(point.Value);

                    // Find edge size which is proportional to the 
                    // width and depth of the column.
                    double size = GetPointSize(width, depth);
                    double edge = GetEdgeSize(size, point);

                    // Get brush from point
                    Brush brush = GetBrush(point, seriesIndx, pointIndx);

                    columnIndx++;

                    // Create a column for a data point
                    GeometryModel3D geometry = CreateColumn(columnIndx, new Point3D(x, y, z), size, height, size, brush, rotateTransform3D, point.Value < AxisY.Crossing, point, pointIndx, edge);

                    // Create hit test info for this column
                    SetHitTest3D(geometry, point);

                    // Add column to the group
                    model3DGroup.Children.Add(geometry);

                    // Create labels
                    if (point.GetMarker() != null)
                    {
                        // Create surface with brush which contain a label.
                        GeometryModel3D label = CreateColumnLabel(new Point3D(x, y, z), size, height, size, brush, point, pointIndx, edge);

                        //Add the label to the model group.
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
            return Math.Min(depth, width);
        }

        /// <summary>
        /// This method creates 3D column which represents one data point. Column can 
        /// have different shape profiles (square, triangle, octagon, star, etc). 3D Column 
        /// has an edge by default.
        /// </summary>
        /// <param name="index">The column index</param>
        /// <param name="center">The center of the column</param>
        /// <param name="width">The width of the column</param>
        /// <param name="height">The height of the column</param>
        /// <param name="depth">The depth of the column</param>
        /// <param name="brush">The brush used for column</param>
        /// <param name="rotateTransform3D">3D Rotation angles of the scene.</param>
        /// <param name="negativeValue">true if the value of the data point is negative. Used for animation.</param>
        /// <param name="point">The data point which represents this column.</param>
        /// <param name="pointIndex">An index of the data point</param>
        /// <param name="edge">edge size</param>
        /// <returns>Geometry model of the 3D cube</returns>
        protected GeometryModel3D CreateColumn(int index, Point3D center, double width, double height, double depth, Brush brush, RotateTransform3D rotateTransform3D, bool negativeValue, DataPoint point, int pointIndex, double edge)
        {
            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D column.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

            // Normal vectors, mesh points and indices.
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            // Do not draw Column if the height is 0.
            if (height == 0)
            {
                return geometryModel;
            }

            // Get chart parameters
            int segments = point.GetParameterValueInt(ChartParameterType.Column3DNumberOfSides);
            _star = point.GetParameterValueBool(ChartParameterType.Column3DStar);
            int starSegments = point.GetParameterValueInt(ChartParameterType.Column3DStarNumberOfSides);

            Point[] shapePoints;
            if (_star)
            {
                shapePoints = CreateStarPoints(starSegments, width / 2, MarkerSeries.GetStartAngle(starSegments));
            }
            else
            {
                shapePoints = CreatePoints(segments, width / 2);
            }

            // Create sides of the 3D Column
            AddSides(points, normals, textures, indices, shapePoints, height, width, edge, segments);

            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TriangleIndices = indices;
            meshGeometry3D.TextureCoordinates = textures;

            // Create matirials
            MaterialGroup materials = new MaterialGroup();
            DiffuseMaterial colorMaterial = new DiffuseMaterial(brush);
            SpecularMaterial specularMaterial = new SpecularMaterial(Brushes.White, 32);

            materials.Children.Add(colorMaterial);
            materials.Children.Add(specularMaterial);
            geometryModel.Material = materials;

            // Create transformations
            Transform3DGroup transformGroup = new Transform3DGroup();
            TranslateTransform3D translate = new TranslateTransform3D(center.X, center.Y, center.Z);

            // Create animation
            if (IsAnimationEnabled(point.GetDoubleAnimation(pointIndex)))
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

                // Column scaled by Y axis only.
                scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, doubleAnimationStart);
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleZProperty, doubleAnimationStart);

                TranslateTransform3D translateAnim = new TranslateTransform3D();
                TranslateTransform3D translateAnim2 = new TranslateTransform3D();

                // Change animation for negative columns
                if (negativeValue)
                {
                    translateAnim.OffsetY = -height;
                    translateAnim2.OffsetY = height;
                }

                // Create animated transformation group
                transformGroup.Children.Add(translateAnim);
                transformGroup.Children.Add(scale);
                transformGroup.Children.Add(translateAnim2);
            }

            // Set position of the cube on the scene.
            transformGroup.Children.Add(translate);
            transformGroup.Children.Add(rotateTransform3D);
            geometryModel.Transform = transformGroup;

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

            return geometryModel;
        }

        /// <summary>
        /// This method creates a rectangle in the 3D space and fills that 
        /// rectangle with a label using drawing brush.
        /// </summary>
        /// <param name="center">The center of the column</param>
        /// <param name="width">The width of the column</param>
        /// <param name="height">The height of the column</param>
        /// <param name="depth">The depth of the column</param>
        /// <param name="brush">The brush used for column</param>
        /// <param name="point">The data point which represents this column.</param>
        /// <param name="pointIndex">An index of the data point</param>
        /// <param name="edge">The edge size of the column</param>
        /// <returns>Geometry model of the 3D surface with a label as Drawing brush</returns>
        protected GeometryModel3D CreateColumnLabel(Point3D center, double width, double height, double depth, Brush brush, DataPoint point, int pointIndex, double edge)
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

            if (textHeight > height / 4)
            {
                textHeight = height / 4;
            }

            // Get chart parameters
            int segments = point.GetParameterValueInt(ChartParameterType.Column3DNumberOfSides);

            // Creates geometry 3D model with rectangle in the 3D space 
            // which will be container of the label.
            if (segments == 4 && !_star)
            {
                CreateLabel(points, normals, textures, indices, textWidth, textHeight, depth / 2 / Math.Sqrt(2), 1.1);
            }
            else
            {
                CreateLabel(points, normals, textures, indices, textWidth, textHeight, depth / 2, 1.1);
            }

            // Very small data points
            if (textHeight < edge)
            {
                edge = textHeight / 4;
            }

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

            // Create transformations
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
        /// Creates a side for the column with edges for different column profiles.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="height">The height of the column</param>
        /// <param name="edge">The edge size of the column</param>
        /// <param name="start">The start point of the profile segment.</param>
        /// <param name="end">The end point of the profile segment.</param>
        /// <param name="nextEnd">The end point of the next segment of the profile.</param>
        /// <param name="segments">The number of sides of the column profile.</param>
        protected override void AddSide(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double height, double edge, Point start, Point end, Point nextEnd, int segments)
        {
            // The radius of the curved edge.
            double distanceOfCurve = Math.Sqrt(2) / 7.0 * edge;

            // The center of the radius for a curved edge.
            double centerOfCurve = Math.Sqrt(2) * edge - distanceOfCurve * 5.0 + distanceOfCurve * segments;

            // Column shape (4 sides).
            if (!_star && segments == 4)
            {
                centerOfCurve = Math.Sqrt(2) * edge;
            }
                        
            // Side edge size.
            double edgeRight = edge;
            if (_star)
            {
                edgeRight = 0;
            }

            // Front
            this.AddPolygon(normals, points, texturePoints, indices, GetPosition(start, end, height - edge, true, edgeRight), GetPosition(start, end, edge, true, edgeRight), GetPosition(start, end, edge, false, edgeRight), GetPosition(start, end, height - edge, false, edgeRight));

            // Edge Top
            this.AddPolygon(normals, points, texturePoints, indices, GetPosition(new Point(0, 0), start, height, false, edge), GetPosition(start, end, height - edge, true, edgeRight), GetPosition(start, end, height - edge, false, edgeRight), GetPosition(new Point(0, 0), end, height, false, edge), FindNormalCenter(start, end, height - edge, edge, new Vector3D(0,-1,0)), Math.Abs(start.X - end.X), edge);


            // Edge Right
            if (!_star)
            {
                this.AddPolygon(normals, points, texturePoints, indices, GetPosition(end, nextEnd, height - edge, true, edge), GetPosition(start, end, height - edge, false, edge), GetPosition(start, end, edge, false, edge), GetPosition(end, nextEnd, edge, true, edge), GetPosition(new Point(0, 0), end, height / 2.0, false, centerOfCurve), edge, Math.Abs(start.Y - end.Y));
            }

            // Front Right Top Triangle
            if (!_star)
            {
                this.AddTriangle(normals, points, texturePoints, indices, GetPosition(new Point(0, 0), end, height, false, edge), GetPosition(start, end, height - edge, false, edge), GetPosition(end, nextEnd, height - edge, true, edge), GetPosition(new Point(0, 0), end, height - edge, false, edge * 1.4));
            }


            // Edge Bottom
            this.AddPolygon(normals, points, texturePoints, indices, GetPosition(start, end, edge, true, edgeRight), GetPosition(new Point(0, 0), start, 0, false, edge), GetPosition(new Point(0, 0), end, 0, false, edge), GetPosition(start, end, edge, false, edgeRight), FindNormalCenter(start, end, edge, edge, new Vector3D(0, -1, 0)), Math.Abs(start.X - end.X), edge);


            // Front Right Bottom Triangle
            if (!_star)
            {
                this.AddTriangle(normals, points, texturePoints, indices, GetPosition(new Point(0, 0), end, 0, false, edge), GetPosition(end, nextEnd, edge, true, edge), GetPosition(start, end, edge, false, edge), GetPosition(new Point(0, 0), end, edge, false, edge * 1.4));
            }
        }

        private Point Mid(Point start, Point end)
        {
            return new Point((start.X + end.X) / 2.0, (start.Y + end.Y) / 2.0);
        }

        /// <summary>
        /// Finds the center point of the curve used as profile for an edge. This point is used to 
        /// find normal vectors of the edge control points and to create a curved edge effect.
        /// </summary>
        /// <param name="start">The start point of an edge line.</param>
        /// <param name="end">The end point of an edge line</param>
        /// <param name="y">y position of an edge</param>
        /// <param name="edge">the edge size</param>
        /// <param name="normal">The normal vector of the top</param>
        /// <returns>The center point</returns>
        private Point3D FindNormalCenter(Point start, Point end, double y, double edge, Vector3D normal)
        {
            Point mid = Mid(start, end);
            Point3D start3D = new Point3D(start.X, y, start.Y);
            Point3D end3D = new Point3D(end.X, y, end.Y);
            Point3D mid3D = new Point3D(mid.X, y, mid.Y);
            Vector3D startEnd = start3D - end3D;
            Vector3D result = Vector3D.CrossProduct(normal, startEnd);
            result.Normalize();
            result *= edge;


            return mid3D + result;
        }

        /// <summary>
        /// Creates 3D point from 2D position of the column profile.
        /// </summary>
        /// <param name="start">The start point of the profile segment</param>
        /// <param name="end">The end point of the profile segment</param>
        /// <param name="y">Y position of the 3D point.</param>
        /// <param name="startEdge">True if the edge is next to the start position, false if the edge is next to the end position</param>
        /// <param name="edge">The size of the edge</param>
        /// <returns>Point 3D created from 2D profile.</returns>
        private Point3D GetPosition(Point start, Point end, double y, bool startEdge, double edge)
        {
            if (startEdge)
            {
                // The edge is next to the start position.
                Point position = GetEdgePosition(start, end, true, edge);
                return new Point3D(position.X, y, position.Y);
            }
            else
            {
                // The edge is next to the end position.
                Point position = GetEdgePosition(start, end, false, edge);
                return new Point3D(position.X, y, position.Y);
            }
        }


        /// <summary>
        /// Creates the top for the column with edges for different column profiles.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="shapePoints">The shape points of the column profile.</param>
        /// <param name="height">The height of the column</param>
        /// <param name="width">The width of the column/bar</param>
        /// <param name="edge">The edge size of the column</param>
        protected override void Top(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, Point[] shapePoints, double height, double width, double edge)
        {
            int indicesNum = points.Count;

            // Set Y position for 3D points and normals 
            points.Add(new Point3D(0, height, 0));
            normals.Add(new Vector3D(0, 1, 0));
            texturePoints.Add(new Point(0.5, 0.5));

            // Set X and Z positions for 3D points and normals from the profile shape.
            for (int segment = 0; segment < shapePoints.Length; segment++)
            {
                points.Add(GetPosition(new Point(0, 0), shapePoints[segment], height, false, edge));
                texturePoints.Add(new Point(shapePoints[segment].X / width + 0.5, shapePoints[segment].Y / width + 0.5));
                normals.Add(new Vector3D(0, 1, 0));
            }

            // Set indices
            for (int segment = 0; segment < shapePoints.Length - 1; segment++)
            {
                indices.Add(0 + indicesNum);
                indices.Add(segment + 1 + indicesNum);
                indices.Add(segment + 2 + indicesNum);
            }

            indices.Add(0 + indicesNum);
            indices.Add(shapePoints.Length + indicesNum);
            indices.Add(1 + indicesNum);
        }

        /// <summary>
        /// Creates the bottom for the column with edges for different column profiles.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="shapePoints">The shape points of the column profile.</param>
        /// <param name="height">The height of the column</param>
        /// <param name="width">The width of the column/bar</param>
        /// <param name="edge">The edge size of the column</param>
        protected override void Bottom(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, Point[] shapePoints, double height, double width, double edge)
        {
            int indicesNum = points.Count;

            // Set Y position for 3D points and normals 
            points.Add(new Point3D(0, 0, 0));
            normals.Add(new Vector3D(0, -1, 0));
            texturePoints.Add(new Point(0.5, 0.5));

            // Set X and Z positions for 3D points and normals from the profile shape.
            for (int segment = 0; segment < shapePoints.Length; segment++)
            {
                points.Add(GetPosition(new Point(0, 0), shapePoints[segment], 0, false, edge));
                texturePoints.Add(new Point(shapePoints[segment].X / width + 0.5, shapePoints[segment].Y / width + 0.5));
                normals.Add(new Vector3D(0, -1, 0));
            }

            // Set indices
            for (int segment = 0; segment < shapePoints.Length - 1; segment++)
            {
                indices.Add(0 + indicesNum);
                indices.Add(segment + 2 + indicesNum);
                indices.Add(segment + 1 + indicesNum);

            }

            indices.Add(0 + indicesNum);
            indices.Add(1 + indicesNum);
            indices.Add(shapePoints.Length + indicesNum);

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