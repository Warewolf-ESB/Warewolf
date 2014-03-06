
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class creates 3D doughnut chart. This class is also 
    /// responsible for 3D doughnut chart animation.
    /// </summary>
    internal class DoughnutChart3D : PieChart3D
    {
        #region Fields

        private int _contoursSegments = 100;
        private double _shapeSegments = 50;

        #endregion Fields

        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return true; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether scene exist 
        /// for this chart type.
        /// </summary>
        override internal bool IsScene { get { return false; } }

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
            double sumPoints = 0;
            SeriesCollection seriesList = GetSeries();

            // The number of data points
            int pointNum = seriesList[0].DataPoints.Count;

            for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
            {
                DataPoint point = seriesList[0].DataPoints[pointIndx];

                //Skip Null values
                if (point.NullValue == true)
                {
                    continue;
                }

                sumPoints += Math.Abs(point.Value);
            }

            if (seriesList[0].ChartType != ChartType.Doughnut)
            {
                return;
            }

            double angleCoeff = 360 / sumPoints;

            double startAngle = 0;
            double endAngle = 0;
            for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
            {
                DataPoint point = seriesList[0].DataPoints[pointIndx];

                //Skip Null values
                if (point.NullValue == true)
                {
                    continue;
                }

                // Get brush from point
                Brush brush = GetBrush(point, 0, pointIndx);
                
                endAngle += Math.Abs(point.Value) * angleCoeff;

                GeometryModel3D geometry = CreateDoughnutSlice(pointIndx, startAngle, endAngle, 0.8, brush, rotateTransform3D, point, pointIndx);

                SetHitTest3D(geometry, point);

                //Add the geometry model to the model group.
                model3DGroup.Children.Add(geometry);

                if (point.GetMarker() != null)
                {
                    GeometryModel3D label = CreatePieSliceLabel(pointIndx, startAngle, endAngle, 0.8, 0.16, brush, rotateTransform3D, point, pointIndx, true);

                    //Add the label model to the model group.
                    model3DGroup.Children.Add(label);
                }

                startAngle = endAngle;
            }
        }

        private GeometryModel3D CreateDoughnutSlice(int index, double startAngle, double endAngle, double radius, Brush brush, RotateTransform3D rotateTransform3D, DataPoint point, int pointIndex)
        {
            startAngle = startAngle / 180 * Math.PI;
            endAngle = endAngle / 180 * Math.PI;

            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D plane.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            DoughnutShape(points, normals, textures, indices, startAngle, endAngle, radius * 0.6, radius);
            Top(points, normals, textures, indices, startAngle, radius * 0.6, radius, true);
            Top(points, normals, textures, indices, endAngle, radius * 0.6, radius, false);
            
            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TextureCoordinates = textures;
            meshGeometry3D.TriangleIndices = indices;

            
            MaterialGroup materials = new MaterialGroup();

            DiffuseMaterial colorMaterial = new DiffuseMaterial(brush);
            SpecularMaterial specularMaterial = new SpecularMaterial(Brushes.LightGray, 100);
            
            materials.Children.Add(colorMaterial);
            materials.Children.Add(specularMaterial);
            geometryModel.Material = materials;

            Transform3DGroup transformGroup = new Transform3DGroup();

            // Exploded 
            TranslateTransform3D explodedTransform = new TranslateTransform3D();
            Point3D explodedCenter = new Point3D();
            if (point.GetParameterValueBool(ChartParameterType.Exploded))
            {
                explodedCenter = FindRadialPointY((startAngle + endAngle) / 2.0, radius * point.GetParameterValueDouble(ChartParameterType.ExplodedRadius), 0);
            }

            Animation exAnimX = point.GetParameterValueAnimation(ChartParameterType.ExplodedAnimation);
            Animation exAnimY = point.GetParameterValueAnimation(ChartParameterType.ExplodedAnimation);
            DoubleAnimation explodedAnimationX = null;
            DoubleAnimation explodedAnimationY = null;

            if (exAnimX != null && exAnimY != null)
            {
                explodedAnimationX = exAnimX.GetDoubleAnimation();
                explodedAnimationY = exAnimY.GetDoubleAnimation();
            }

            if (IsAnimationEnabled(explodedAnimationX) && IsAnimationEnabled(explodedAnimationY))
            {
                explodedAnimationX.From = 0;
                explodedAnimationY.From = 0;

                explodedAnimationX.To = explodedCenter.X;
                explodedAnimationY.To = explodedCenter.Z;

                explodedTransform.OffsetX = 0;
                explodedTransform.OffsetY = 0;

                explodedTransform.BeginAnimation(TranslateTransform3D.OffsetXProperty, explodedAnimationX);
                explodedTransform.BeginAnimation(TranslateTransform3D.OffsetYProperty, explodedAnimationY);

            }
            else
            {
                explodedTransform.OffsetX = explodedCenter.X;
                explodedTransform.OffsetY = explodedCenter.Z;
            }

            transformGroup.Children.Add(explodedTransform);

            if (IsAnimationEnabled(point.GetDoubleAnimation(pointIndex)))
            {
                DoubleAnimation doubleAnimation = point.GetDoubleAnimation(pointIndex);

                ScaleTransform3D scale = new ScaleTransform3D();
                scale.ScaleX = 0;
                scale.ScaleY = 0;
                scale.ScaleZ = 0;

                scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, doubleAnimation);
                scale.BeginAnimation(ScaleTransform3D.ScaleZProperty, doubleAnimation);

                transformGroup.Children.Add(scale);
            }


            transformGroup.Children.Add(rotateTransform3D);

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

            geometryModel.Transform = transformGroup;

            return geometryModel;
        }

        
        private void DoughnutShape(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double startAngle, double endAngle, double innerRadius, double outerRadius)
        {
            Point[] contour = GetContour(innerRadius, outerRadius);
            int indicesNum = points.Count;

            // Smooth unit is used to define the smoothes of the pie slice curve. It depends of the pie slice size.
            double numSegments = PieChart3D.GetNumberOfSegments(startAngle, endAngle, _shapeSegments);
            double smoothStep = (endAngle - startAngle) / numSegments;

            // Texture Coordinate step
            double xTexStep = 1.0 / (double)(numSegments);
            double yTexStep = 1.0 / (double)(contour.Length - 1);

            double currentAngle = startAngle;
            for (int segment = 0; segment <= numSegments; segment++)
            {
                for (int arcIndex = 0; arcIndex < contour.Length; arcIndex++)
                {
                    // Points
                    points.Add(FindRadialPointZ(currentAngle, contour[arcIndex].X, contour[arcIndex].Y));

                    // Textures
                    texturePoints.Add(new Point(xTexStep * segment, yTexStep * arcIndex));

                    // Normals
                    normals.Add( FindRadialPointZ(currentAngle, contour[arcIndex].X, contour[arcIndex].Y) - FindRadialPointZ(currentAngle, (outerRadius + innerRadius) / 2.0, 0));
                }
                currentAngle += smoothStep;
            }
            for (int segment = 0; segment < numSegments; segment++)
            {
                for (int arcIndex = 0; arcIndex < contour.Length; arcIndex++)
                {
                    int indx = segment * contour.Length + arcIndex;
                    // Indices
                    indices.Add(indx + contour.Length + indicesNum);
                    indices.Add(indx + 1 + indicesNum);
                    indices.Add(indx + 0 + indicesNum);
                    indices.Add(indx + contour.Length + indicesNum);
                    indices.Add(indx + contour.Length + 1 + indicesNum);
                    indices.Add(indx + 1 + indicesNum);
                }
            }
        }

        private void Top(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double angle, double innerRadius, double outerRadius, bool startAngle )
        {
            Point[] contour = GetContour(innerRadius, outerRadius);
            int indicesNum = points.Count;

            double xCenter = (innerRadius + outerRadius) / 2;
            double yCenter = 0;
            
            points.Add(FindRadialPointZ(angle, xCenter, yCenter));

            Vector3D vectorContour = new Vector3D(0, 0, 1);
            Vector3D vectorCenter = FindRadialPointZ(angle, xCenter, 0) - new Point3D(0, 0, 0);
            Vector3D normalVector;
            
            if (startAngle)
            {
                normalVector = Vector3D.CrossProduct(vectorContour, vectorCenter);
            }
            else
            {
                normalVector = Vector3D.CrossProduct(vectorCenter, vectorContour);
            }
            
            normals.Add(normalVector);

            for (int arcIndex = 0; arcIndex < contour.Length; arcIndex++)
            {
                // Points
                points.Add(FindRadialPointZ(angle, contour[arcIndex].X, contour[arcIndex].Y));

                // Textures
                //texturePoints.Add(new Point(xTexStep * segment, yTexStep * arcIndex));

                // Normals
                normals.Add(normalVector);
            }

            for (int arcIndex = 0; arcIndex < contour.Length - 1; arcIndex++)
            {
                // Indices
                indices.Add(indicesNum);

                if (startAngle)
                {
                    indices.Add(indicesNum + arcIndex + 1);
                    indices.Add(indicesNum + arcIndex + 2);
                }
                else
                {
                    indices.Add(indicesNum + arcIndex + 2);
                    indices.Add(indicesNum + arcIndex + 1);
                }
            }
        }


        private Point[] GetContour(double innerRadius, double outerRadius)
        {
            Point[] points = new Point[_contoursSegments + 1];

            if (outerRadius <= innerRadius)
            {
                // Invalid inner and outer radius for 3D doughnut chart.
                throw new InvalidOperationException(ErrorString.Exc3);
            }

            double xCenter = (innerRadius + outerRadius) / 2;
            double yCenter = 0;
            double radius = (outerRadius - innerRadius) / 2.0;

            double angle = 0;
            double stepAngle = 2 * Math.PI / _contoursSegments;
            for (int pointIndex = 0; pointIndex <= _contoursSegments; pointIndex++)
            {
                points[pointIndex].X = xCenter + radius * Math.Cos(angle);
                points[pointIndex].Y = yCenter - radius * Math.Sin(angle);

                angle += stepAngle;
            }

            return points;
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