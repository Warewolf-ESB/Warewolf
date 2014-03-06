
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
    /// This class creates 3D point chart. This class is also 
    /// responsible for 3D point chart animation.
    /// </summary>
    internal class PointChart3D : ColumnBar3DShapes
    {
        #region Fields

        internal double _edge = 0.05;
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
            int markerIndx = 0;
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {
                // Series is not point chart
                if (seriesList[seriesIndx].ChartType != ChartType.Point)
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

                    double x, y, z;
                    double radius;

                    // Skip point if out of range
                    if (IsOutOfRange(pointIndx + 1 - 0.5, point.Value) || IsOutOfRange(pointIndx + 1 + 0.5, point.Value))
                    {
                        continue;
                    }

                    // A point position
                    x = AxisX.GetPosition(pointIndx + 1);
                    y = AxisY.GetPositionLogarithmic(point.Value);
                    z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                    // A radius of the point
                    radius = AxisX.GetSize(1) / 3.0;

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
                        geometry = BubbleChart3D.CreateBubble(new Point3D(x, y, z), radius, brush, rotateTransform3D, point, pointIndx, 30,this);
                    }
                    else
                    {
                        geometry = CreateMarkerPoint(markerIndx, new Point3D(x, y, z), radius, radius, radius, brush, rotateTransform3D, point, pointIndx);
                    }

                    // Create hit test info for this column
                    SetHitTest3D(geometry, point);

                    // Add column to the group
                    model3DGroup.Children.Add(geometry);
                }
            }
        }

        /// <summary>
        /// This method creates 3D Marker which represents one data point. Marker can 
        /// have different shape profiles (triangle, cube, star, etc). 3D Marker 
        /// has an edge by default.
        /// </summary>
        /// <param name="index">The marker index</param>
        /// <param name="center">The center of the marker</param>
        /// <param name="width">The width of the marker</param>
        /// <param name="height">The height of the marker</param>
        /// <param name="depth">The depth of the marker</param>
        /// <param name="brush">The brush used for marker</param>
        /// <param name="rotateTransform3D">3D Rotation angles of the scene.</param>
        /// <param name="point">The data point which represents this marker.</param>
        /// <param name="pointIndex">An index of the data point</param>
        /// <returns>Geometry model of the 3D cube</returns>
        internal GeometryModel3D CreateMarkerPoint(int index, Point3D center, double width, double height, double depth, Brush brush, RotateTransform3D rotateTransform3D, DataPoint point, int pointIndex)
        {
            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D marker.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

            // Normal vectors, mesh points and indices.
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            // Do not draw marker if the height is 0.
            if (height == 0)
            {
                return geometryModel;
            }

            int segments = 0;
            int starSegments = 0;
            switch(point.GetMarker().Type)
            {
                case MarkerType.Rectangle:
                    _star = false;
                    segments = 4;
                    break;
                case MarkerType.Triangle:
                    _star = false;
                    segments = 3;
                    break;
                case MarkerType.Hexagon:
                    _star = false;
                    segments = 6;
                    break;
                case MarkerType.Star5:
                    _star = true;
                    starSegments = 5;
                    break;
                case MarkerType.Star6:
                    _star = true;
                    starSegments = 6;
                    break;
                case MarkerType.Star7:
                    _star = true;
                    starSegments = 7;
                    break;
                case MarkerType.Star8:
                    _star = true;
                    starSegments = 8;
                    break;
            }
           
            _edge = 0;
            
            Point[] shapePoints;
            if (_star)
            {
                shapePoints = CreateStarPoints(starSegments, width / 2, -MarkerSeries.GetStartAngle(starSegments));
            }
            else
            {
                shapePoints = CreatePoints(segments, width / 2);
            }

            // Create sides of the 3D marker
            AddSides(points, normals, textures, indices, shapePoints, height, width, _edge, segments);

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
                
                ScaleTransform3D scale = new ScaleTransform3D();
                scale.ScaleX = 0;
                scale.ScaleY = 0;
                scale.ScaleZ = 0;

                // marker scaled by X, Y and Z axes.
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleZProperty, doubleAnimation);

                // Create animated transformation group
                transformGroup.Children.Add(scale);
            }

            // Set position of the marker on the scene.
            transformGroup.Children.Add(translate);
            transformGroup.Children.Add(rotateTransform3D);
            geometryModel.Transform = transformGroup;

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

            return geometryModel;
        }

        /// <summary>
        /// Creates a side for the marker with edges for different profiles.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="height">The height of the marker</param>
        /// <param name="edge">The edge size of the marker</param>
        /// <param name="start">The start point of the profile segment.</param>
        /// <param name="end">The end point of the profile segment.</param>
        /// <param name="nextEnd">The end point of the next segment of the profile.</param>
        /// <param name="segments">The number of sides of the bar profile.</param>
        protected override void AddSide(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double height, double edge, Point start, Point end, Point nextEnd, int segments)
        {
            // The radius of the curved edge.
            double distanceOfCurve = Math.Sqrt(2) / 7.0 * edge;

            // The center of the radius for a curved edge.
            double centerOfCurve = Math.Sqrt(2) * edge - distanceOfCurve * 5.0 + distanceOfCurve * segments;

            // marker shape (4 sides).
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
            this.AddPolygon(normals, points, texturePoints, indices, GetPosition(start, end, height - edge, true, edgeRight), GetPosition(start, end, height - edge, false, edgeRight), GetPosition(start, end, edge, false, edgeRight), GetPosition(start, end, edge, true, edgeRight));

            // Edge Top
            this.AddPolygon(normals, points, texturePoints, indices, GetPosition(new Point(0, 0), end, height, false, edge), GetPosition(start, end, height - edge, false, edgeRight), GetPosition(start, end, height - edge, true, edgeRight), GetPosition(new Point(0, 0), start, height, false, edge), FindNormalCenter(start, end, height - edge, edge, new Vector3D(0, 0, 1)), Math.Abs(start.X - end.X), edge);


            // Edge Right
            if (!_star)
            {
                this.AddPolygon(normals, points, texturePoints, indices, GetPosition(start, end, height - edge, false, edge), GetPosition(end, nextEnd, height - edge, true, edge), GetPosition(end, nextEnd, edge, true, edge), GetPosition(start, end, edge, false, edge), GetPosition(new Point(0, 0), end, height / 2.0, false, centerOfCurve), edge, Math.Abs(start.Y - end.Y));
            }

            // Front Right Top Triangle
            if (!_star)
            {
                this.AddTriangle(normals, points, texturePoints, indices, GetPosition(new Point(0, 0), end, height, false, edge), GetPosition(end, nextEnd, height - edge, true, edge), GetPosition(start, end, height - edge, false, edge), GetPosition(new Point(0, 0), end, height - edge, false, edge * 1.4));
            }

            // Edge Bottom
            this.AddPolygon(normals, points, texturePoints, indices, GetPosition(new Point(0, 0), start, 0, false, edge), GetPosition(start, end, edge, true, edgeRight), GetPosition(start, end, edge, false, edgeRight), GetPosition(new Point(0, 0), end, 0, false, edge), FindNormalCenter(start, end, edge, edge, new Vector3D(0, 0, 1)), Math.Abs(start.X - end.X), edge);

            // Front Right Bottom Triangle
            if (!_star)
            {
                this.AddTriangle(normals, points, texturePoints, indices, GetPosition(new Point(0, 0), end, 0, false, edge), GetPosition(start, end, edge, false, edge), GetPosition(end, nextEnd, edge, true, edge), GetPosition(new Point(0, 0), end, edge, false, edge * 1.4));
            }
        }

        /// <summary>
        /// Creates 3D point from 2D position of the marker profile.
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
                return new Point3D(position.X, position.Y, y);
            }
            else
            {
                // The edge is next to the end position.
                Point position = GetEdgePosition(start, end, false, edge);
                return new Point3D(position.X, position.Y, y );
            }
        }


        private Point3D FindNormalCenter(Point start, Point end, double y, double edge, Vector3D normal)
        {
            Point mid = Mid(start, end);
            Point3D start3D = new Point3D(start.X, start.Y, y);
            Point3D end3D = new Point3D(end.X, end.Y, y);
            Point3D mid3D = new Point3D(mid.X, mid.Y, y);
            Vector3D startEnd = start3D - end3D;
            Vector3D result = Vector3D.CrossProduct(normal, startEnd);
            result.Normalize();
            result *= edge;

            return mid3D + result;
        }

        /// <summary>
        /// Creates the top for the bar with edges for different column profiles.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="shapePoints">The shape points of the bar profile.</param>
        /// <param name="height">The height of the bar</param>
        /// <param name="width">The width of the column/bar</param>
        /// <param name="edge">The edge size of the bar</param>
        protected override void Top(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, Point[] shapePoints, double height, double width, double edge)
        {
            int indicesNum = points.Count;

            // Set Y position for 3D points and normals 
            points.Add(new Point3D(0, 0, height));
            normals.Add(new Vector3D(0, 0, 1));
            texturePoints.Add(new Point(0.5, 0.5));

            // Set X and Z positions for 3D points and normals from the profile shape.
            for (int segment = 0; segment < shapePoints.Length; segment++)
            {
                points.Add(GetPosition(new Point(0, 0), shapePoints[segment], height, false, edge));
                texturePoints.Add(new Point(shapePoints[segment].X / width + 0.5, shapePoints[segment].Y / width + 0.5));
                normals.Add(new Vector3D(0, 0, 1));
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

        /// <summary>
        /// Creates the bottom for the bar with edges for different column profiles.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D.</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D.</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D.</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D.</param>
        /// <param name="shapePoints">The shape points of the bar profile.</param>
        /// <param name="height">The height of the bar</param>
        /// <param name="width">The width of the column/bar</param>
        /// <param name="edge">The edge size of the bar</param>
        protected override void Bottom(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, Point[] shapePoints, double height, double width, double edge)
        {
            int indicesNum = points.Count;

            // Set Y position for 3D points and normals 
            points.Add(new Point3D(0, 0, 0));
            normals.Add(new Vector3D(0, 0, -1));
            texturePoints.Add(new Point(0.5, 0.5));

            // Set X and Z positions for 3D points and normals from the profile shape.
            for (int segment = 0; segment < shapePoints.Length; segment++)
            {
                points.Add(GetPosition(new Point(0, 0), shapePoints[segment], 0, false, edge));
                texturePoints.Add(new Point(shapePoints[segment].X / width + 0.5, shapePoints[segment].Y / width + 0.5));
                normals.Add(new Vector3D(0, 0, -1));
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

        private Point Mid(Point start, Point end)
        {
            return new Point((start.X + end.X) / 2.0, (start.Y + end.Y) / 2.0);
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