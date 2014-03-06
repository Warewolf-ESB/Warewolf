
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
    /// This class creates 3D area chart. This class is also 
    /// responsible for 3D area chart animation.
    /// </summary>
    class AreaChart3D : Primitives3D
    {
        #region Fields

        private Point3D _lineCenter;
        private double _pointShift;
        private double _edge = 0.005;

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
                if (seriesList[seriesIndx].ChartType != ChartType.Area)
                {
                    continue;
                }

                // The number of data points
                int pointNum = seriesList[seriesIndx].DataPoints.Count;

                for (int pointIndx = 0; pointIndx < pointNum - 1; pointIndx++)
                {
                    DataPoint point1 = seriesList[seriesIndx].DataPoints[pointIndx];
                    DataPoint point2 = seriesList[seriesIndx].DataPoints[pointIndx + 1];

                    // Skip point if out of range
                    if (IsXOutOfRange(pointIndx + 1) || IsXOutOfRange(pointIndx + 2))
                    {
                        continue;
                    }

                    // Skip point if out of range
                    if (IsYOutOfRange(point1.Value) || IsYOutOfRange(point2.Value))
                    {
                        continue;
                    }

                    //Skip Null values
                    if (point1.NullValue == true || point2.NullValue == true)
                    {
                        continue;
                    }
                    
                    double x = AxisX.GetPosition(pointIndx + 1);
                    double upStart = AxisY.GetPositionLogarithmic(point1.Value);
                    double upEnd = AxisY.GetPositionLogarithmic(point2.Value);
                    double downStart = AxisY.GetAreaZeroValue();
                    double downEnd = AxisY.GetAreaZeroValue();
                    double z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                    double width = AxisX.GetSize(1);

                    // Get point depth from chart parameter
                    double depth;
                    GetPointWidth(out depth, AxisX, AxisZ, seriesList[seriesIndx]);

                    // Get brush from point
                    Brush brush = GetBrush(point1, seriesIndx, pointIndx);
                    
                    columnIndx++;

                    GeometryModel3D geometry = Create(columnIndx, new Point3D(x, 0, z), upStart, upEnd, downStart, downEnd, width, depth, brush, rotateTransform3D, point1, pointIndx, false);
                    SetHitTest3D(geometry, point1);

                    model3DGroup.Children.Add(geometry);
                }

                // Draw point labels
                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = seriesList[seriesIndx].DataPoints[pointIndx];

                    // Skip point if out of range
                    if (IsXOutOfRange(pointIndx + 1) || IsXOutOfRange(pointIndx + 2))
                    {
                        continue;
                    }

                    // Skip point if out of range
                    if (IsYOutOfRange(point.Value) || IsYOutOfRange(AxisY.Crossing))
                    {
                        continue;
                    }

                    //Skip Null values
                    if (point.NullValue == true)
                    {
                        continue;
                    }

                    if (point.GetMarker() != null)
                    {

                        double x = AxisX.GetPosition(pointIndx + 1);
                        double y = (AxisY.GetPositionLogarithmic(point.Value) + AxisY.GetPositionLogarithmic(AxisY.Crossing)) / 2.0;
                        double z = AxisZ.GetPosition(GetZSeriesPosition(seriesIndx, seriesList) - 0.5);

                        double width = AxisX.GetSize(0.5);
                        double height = Math.Abs(AxisY.GetPositionLogarithmic(point.Value) - AxisY.GetPositionLogarithmic(AxisY.Crossing));

                        // Get point depth from chart parameter
                        double depth;
                        GetPointWidth(out depth, AxisX, AxisZ, seriesList[seriesIndx]);

                        // Get brush from point
                        Brush brush = GetBrush(point, seriesIndx, pointIndx);

                        columnIndx++;

                        GeometryModel3D label = CreateAreaLabel(columnIndx, new Point3D(x, y, z), width, height, depth, brush, rotateTransform3D, point, pointIndx, _edge);

                        //Add the label model to the model group.
                        model3DGroup.Children.Add(label);
                    }
                }
            }
        }


        internal GeometryModel3D Create(int index, Point3D center, double upStart, double upEnd, double downStart, double downEnd, double width, double depth, Brush brush, RotateTransform3D rotateTransform3D, DataPoint point, int pointIndex, bool stacked)
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

            _edge = 0.005 * point.GetParameterValueDouble(ChartParameterType.EdgeSize3D);

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
                DoubleAnimation doubleAnimation = point.GetDoubleAnimation(pointIndex);

                DoubleAnimation doubleAnimationY = doubleAnimation.Clone();
                doubleAnimationY.Duration = new Duration(TimeSpan.FromMilliseconds(0));
                
                ScaleTransform3D scale = new ScaleTransform3D();
                scale.ScaleX = 0;
                scale.ScaleY = 0;

                scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimationY);
                
                TranslateTransform3D translateAnim = new TranslateTransform3D();
                TranslateTransform3D translateAnim2 = new TranslateTransform3D();

                translateAnim.OffsetY = upStart;
                translateAnim2.OffsetY = -upStart;

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

        protected GeometryModel3D CreateAreaLabel(int index, Point3D center, double width, double height, double depth, Brush brush, RotateTransform3D rotateTransform3D, DataPoint point, int pointIndex, double edge)
        {

            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D plane.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            FormattedText text = GetLabelStyle(point);
            double maxCoef = 0.006 / Math.Max(text.Width, text.Extent) * text.Extent;

            height *= 0.8;
            width *= 0.6;

            if (text.Width * maxCoef < width)
            {
                width = text.Width * maxCoef;
            }

            if (text.Extent * maxCoef < height)
            {
                height = text.Extent * maxCoef;
            }

            double textWidth = width;
            double textHeight = height;

            // Check if the point is first or last point
            int numOfPoints = point.GetSeries().DataPoints.Count;

            bool firstPoint = false;
            bool lastPoint = false;
            if (pointIndex == 0)
            {
                firstPoint = true;
            }

            if (pointIndex == numOfPoints - 1)
            {
                lastPoint = true;
            }


            CreateLabel(points, normals, textures, indices, textWidth, textHeight / 2, depth / 2, 1.1);

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

            double xPos = center.X;

            if (firstPoint)
            {
                xPos += width + edge;
            }

            if (lastPoint)
            {
                xPos -= width + edge;
            }

            TranslateTransform3D translate;
            translate = new TranslateTransform3D(xPos, center.Y, center.Z);

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

            // Create animation
            if (IsAnimationEnabled(point.GetDoubleAnimation(pointIndex)))
            {
                // Create double animations for scaling animation
                DoubleAnimation doubleAnimation = point.GetDoubleAnimation(pointIndex);

                ScaleTransform3D scale = new ScaleTransform3D();
                scale.ScaleX = 0;
                scale.ScaleY = 0;

                // Scaled by X and Y axis.
                scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimation);

                // Create animated transformation group
                transformGroup.Children.Add(scale);
            }

            transformGroup.Children.Add(translate);

            // transformGroup.Children.Add(rotateTransform3D);
            geometryModel.Transform = transformGroup;


            return geometryModel;
        }

        
        protected void AreaSides(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double upStart, double upEnd, double downStart, double downEnd, double width, double depth)
        {
            if (upStart > downEnd && upEnd < downEnd)
            {
                double width1 = width / (upStart - upEnd) * upStart;
                AreaSideElements(points, normals, texturePoints, indices, upStart, downEnd, downStart, downEnd, width1, depth, _edge);
                _pointShift += width1;
                AreaSideElements(points, normals, texturePoints, indices, downStart, downEnd, downStart, upEnd, width - width1, depth, _edge);
                _pointShift = 0; 
                return;
            }
            else if (upStart < downEnd && upEnd > downEnd)
            {
                double width1 = width / (upEnd - upStart) * (-upStart);
                AreaSideElements(points, normals, texturePoints, indices, downStart, downEnd, upStart, downEnd, width1, depth, _edge);
                _pointShift += width1;
                AreaSideElements(points, normals, texturePoints, indices, downStart, upEnd, downStart, downEnd, width - width1, depth, _edge);
                _pointShift = 0;
                return;
            }
            else if (upStart >= downEnd && upEnd >= downEnd)
            {
                AreaSideElements(points, normals, texturePoints, indices, upStart, upEnd, downStart, downEnd, width, depth, _edge);
            }
            else if (upStart <= downEnd && upEnd <= downEnd)
            {
                AreaSideElements(points, normals, texturePoints, indices, downStart, downEnd, upStart, upEnd, width, depth, _edge);
            }
        }

        protected void AreaSideElements(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double upStart, double upEnd, double downStart, double downEnd, double width, double depth, double edge)
        {
            if (Math.Abs(upStart - downStart) <= 2 * edge)
            {
                edge = Math.Abs(downStart - upStart) / 2.1;
            }

            if (Math.Abs(upEnd - downEnd) <= 2 * edge)
            {
                edge = Math.Abs(downEnd - upEnd) / 2.1;
            }

            // Avoid Triangle for Front and back sides
            if (downEnd == upEnd)
            {
                upEnd += 0.001;
            }

            if (downStart == upStart)
            {
                upStart += 0.001;
            }

            // ********************************
            // Sides
            // ********************************
            // Front Side
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, downStart + edge, depth / 2), CreatePoint(width, downEnd + edge, depth / 2), CreatePoint(width, upEnd - edge, depth / 2), CreatePoint(0, upStart - edge, depth / 2));

            // Top Side
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, upStart, depth / 2 - edge), CreatePoint(width, upEnd, depth / 2 - edge), CreatePoint(width, upEnd, -depth / 2 + edge), CreatePoint(0, upStart, -depth / 2 + edge));

            // Bottom Side
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, downStart, depth / 2 - edge), CreatePoint(0, downStart, -depth / 2 + edge), CreatePoint(width, downEnd, -depth / 2 + edge), CreatePoint(width, downEnd, depth / 2 - edge));

            // Back Side
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, downStart + edge, -depth / 2), CreatePoint(0, upStart - edge, -depth / 2), CreatePoint(width, upEnd - edge, -depth / 2), CreatePoint(width, downEnd + edge, -depth / 2));

            // Right Side
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(width, downEnd, depth / 2 - edge), CreatePoint(width, downEnd, -depth / 2 + edge), CreatePoint(width, upEnd, -depth / 2 + edge), CreatePoint(width, upEnd, depth / 2 - edge));

            // Right Side Edge Cover Front
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(width, downEnd + edge, depth / 2), CreatePoint(width, downEnd, depth / 2 - edge), CreatePoint(width, upEnd, depth / 2 - edge), CreatePoint(width, upEnd - edge, depth / 2));

            // Right Side Edge Cover Back
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(width, downEnd, -depth / 2 + edge), CreatePoint(width, downEnd + edge, -depth / 2), CreatePoint(width, upEnd - edge, -depth / 2), CreatePoint(width, upEnd, -depth / 2 + edge));

            // Left Side
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, upStart, depth / 2 - edge), CreatePoint(0, upStart, -depth / 2 + edge), CreatePoint(0, downStart, -depth / 2 + edge), CreatePoint(0, downStart, depth / 2 - edge));

            // Left Side Edge Cover Front
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, upStart - edge, depth / 2), CreatePoint(0, upStart, depth / 2 - edge), CreatePoint(0, downStart, depth / 2 - edge), CreatePoint(0, downStart + edge, depth / 2));

            // Left Side Edge Cover Back
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, upStart, -depth / 2 + edge), CreatePoint(0, upStart - edge, -depth / 2), CreatePoint(0, downStart + edge, -depth / 2), CreatePoint(0, downStart, -depth / 2 + edge));


            // ********************************
            // Edges
            // ********************************
            // Top Front Edge
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, upStart - edge, depth / 2), CreatePoint(width, upEnd - edge, depth / 2), CreatePoint(width, upEnd, depth / 2 - edge), CreatePoint(0, upStart, depth / 2 - edge));

            // Top Back Edge
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, upStart - edge, -depth / 2), CreatePoint(0, upStart, -depth / 2 + edge), CreatePoint(width, upEnd, -depth / 2 + edge), CreatePoint(width, upEnd - edge, -depth / 2));

            // Bottom Front Edge
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, downStart + edge, depth / 2), CreatePoint(0, downStart, depth / 2 - edge), CreatePoint(width, downEnd, depth / 2 - edge), CreatePoint(width, downEnd + edge, depth / 2));

            // Bottom Front Edge
            this.AddPolygon(normals, points, texturePoints, indices, CreatePoint(0, downStart + edge, -depth / 2), CreatePoint(width, downEnd + edge, -depth / 2), CreatePoint(width, downEnd, -depth / 2 + edge), CreatePoint(0, downStart, -depth / 2 + edge));
            
        }

        
        private Point3D CreatePoint(double x, double y, double z)
        {
            return new Point3D(x + _pointShift, y, z);
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