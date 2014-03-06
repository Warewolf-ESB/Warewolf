
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
    /// This class creates 3D pie chart. This class is also 
    /// responsible for 3D pie chart animation.
    /// </summary>
    internal class PieChart3D : Primitives3D
    {
        #region Fields

        // Private fields
        private double _zCoef = 1.05;
        private double _smooth = 90;
        private double _edgeAngle = 1.0 / 180.0 * Math.PI;
        private double _innerRadiusCoef = 0.98;
        private double _innerRadiusEdge = 0.03;
        private double _pieRounding = 2;

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

            // Find the sum of data points
            for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
            {
                DataPoint point = seriesList[0].DataPoints[pointIndx];

                // Skip Null and 0 values
                if (point.NullValue == true || point.Value == 0)
                {
                    continue;
                }

                sumPoints += Math.Abs(point.Value);
            }

            if (seriesList[0].ChartType != ChartType.Pie)
            {
                return;
            }

            double angleCoeff = 360 / sumPoints;

            double startAngle = 0;
            double endAngle = 0;
            for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
            {
                DataPoint point = seriesList[0].DataPoints[pointIndx];

                // Skip Null and 0 values
                if (point.NullValue == true || point.Value == 0)
                {
                    continue;
                }

                // Get brush from point
                Brush brush = GetBrush(point, 0, pointIndx);

                endAngle += Math.Abs(point.Value) * angleCoeff;

                GeometryModel3D geometry = CreatePieSlice(pointIndx, startAngle, endAngle, 0.8, 0.1, brush, rotateTransform3D, point, pointIndx);

                SetHitTest3D(geometry, point);

                //Add the geometry model to the model group.
                model3DGroup.Children.Add(geometry);

                startAngle = endAngle;
            }

            // add the markers separately
            startAngle = 0;
            endAngle = 0;
            for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
            {
                DataPoint point = seriesList[0].DataPoints[pointIndx];

                // Skip Null and 0 values
                if (point.NullValue == true || point.Value == 0)
                {
                    continue;
                }

                // Get brush from point
                Brush brush = GetBrush(point, 0, pointIndx);

                endAngle += Math.Abs(point.Value) * angleCoeff;
                if (point.GetMarker() != null)
                {
                    GeometryModel3D label = CreatePieSliceLabel(pointIndx, startAngle, endAngle, 0.8, 0.1, brush, rotateTransform3D, point, pointIndx, false);

                    //Add the label model to the model group.
                    model3DGroup.Children.Add(label);
                }

                startAngle = endAngle;
            }
        }

        private GeometryModel3D CreatePieSlice(int index, double startAngle, double endAngle, double radius, double z, Brush brush, RotateTransform3D rotateTransform3D, DataPoint point, int pointIndex)
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

            double edgeSize3D = point.GetParameterValueDouble(ChartParameterType.EdgeSize3D) / 2.0;
            _zCoef = 1 + 0.2 * edgeSize3D;
            _edgeAngle = 1.5 / 180.0 * Math.PI * edgeSize3D;
            _innerRadiusCoef = 1 - 0.05 * edgeSize3D;
            _innerRadiusEdge = 0.04 * edgeSize3D;
            _pieRounding = point.GetParameterValueDouble(ChartParameterType.Pie3DRounding);

            CreateMainCurve(points, normals, textures, indices, startAngle, endAngle, radius, z);
            CreateTop(points, normals, textures, indices, startAngle, endAngle, radius, z, false);
            CreateTop(points, normals, textures, indices, startAngle, endAngle, radius, -z, true);
            CreateCurveEdge(points, normals, textures, indices, startAngle, endAngle, radius, z, false);
            CreateCurveEdge(points, normals, textures, indices, startAngle, endAngle, radius, -z, true);
            CreateSideEdge(points, normals, textures, indices, startAngle, endAngle, radius, -z, true);
            CreateSideEdge(points, normals, textures, indices, startAngle, endAngle, radius, z, false);
            CreateSides(points, normals, textures, indices, startAngle, endAngle, radius, z);

            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TriangleIndices = indices;
            meshGeometry3D.TextureCoordinates = textures;

            MaterialGroup materials = new MaterialGroup();

            DiffuseMaterial colorMaterial = new DiffuseMaterial(brush);
            SpecularMaterial specularMaterial = new SpecularMaterial(Brushes.White, 32);

            materials.Children.Add(colorMaterial);
            materials.Children.Add(specularMaterial);
            geometryModel.Material = materials;

            Transform3DGroup transformGroup = new Transform3DGroup();

            // Exploded 
            TranslateTransform3D explodedTransform = new TranslateTransform3D();
            Point3D explodedCenter = new Point3D();

            // Chart parameters
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

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

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
            geometryModel.Transform = transformGroup;


            return geometryModel;
        }

        protected GeometryModel3D CreatePieSliceLabel(int index, double startAngle, double endAngle, double radius, double z, Brush brush, RotateTransform3D rotateTransform3D, DataPoint point, int pointIndex, bool doughnut)
        {

            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D plane.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();


            FormattedText text = GetLabelStyle(point);
            double maxCoef = 0.008 / Math.Max(text.Width, text.Extent) * text.Extent;
            double width = text.Width * maxCoef;
            double height = text.Extent * maxCoef;

            if (Math.Abs(endAngle - startAngle) < 180.0) // [DN 4/15/2008:BR32066] the following routine of reducing the text width should not be a concern for slices 180 degrees or greater.  and in fact the distance formula in there was inverting in slices over 180 degrees and shrinking the labels (approaching zero width for a 360 degree slice).
            {
                // Find max text width (avoid intersection with next labels).
                Point3D startLabel = FindRadialPointZ(startAngle / 180 * Math.PI, radius * 0.5, z);
                Point3D endLabel = FindRadialPointZ(endAngle / 180 * Math.PI, radius * 0.5, z);

                double maxWidth = Math.Sqrt(Math.Pow(startLabel.X - endLabel.X, 2) + Math.Pow(startLabel.Y - endLabel.Y, 2));

                if (width * 2.0 > maxWidth)
                {
                    width = maxWidth / 2.0;
                }
            }
            CreateLabel(points, normals, textures, indices, width, height, z);

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

            double explodedRadius = 0;
            if (doughnut)
            {
                if (point.GetParameterValueBool(ChartParameterType.Exploded))
                {
                    explodedRadius = radius * point.GetParameterValueDouble(ChartParameterType.ExplodedRadius) + (radius - z);
                }
                radius = radius - z;
            }
            else
            {
                if (point.GetParameterValueBool(ChartParameterType.Exploded))
                {
                    explodedRadius = radius * point.GetParameterValueDouble(ChartParameterType.ExplodedRadius) + radius * 0.6;
                }
                radius = radius * 0.6;
            }

            TranslateTransform3D translate;

            // Exploded
            Animation exAnim = point.GetParameterValueAnimation(ChartParameterType.ExplodedAnimation);
            DoubleAnimation explodedAnimation = null;

            if (exAnim != null)
            {
                explodedAnimation = exAnim.GetDoubleAnimation();
            }

            if (IsAnimationEnabled(explodedAnimation) && point.GetParameterValueBool(ChartParameterType.Exploded))
            {
                explodedAnimation.From = radius;

                explodedAnimation.To = explodedRadius;

                translate = new TranslateTransform3D(0, radius, 0);

                translate.BeginAnimation(TranslateTransform3D.OffsetYProperty, explodedAnimation);
            }
            else
            {
                if (point.GetParameterValueBool(ChartParameterType.Exploded))
                {
                    translate = new TranslateTransform3D(0, explodedRadius, 0);
                }
                else
                {
                    translate = new TranslateTransform3D(0, radius, 0);
                }
            }

            RotateTransform3D rotateTransform = new RotateTransform3D();
            AxisAngleRotation3D rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), -(startAngle + endAngle) / 2.0);
            rotateTransform.Rotation = rotation;


            transformGroup.Children.Add(translate);
            transformGroup.Children.Add(rotateTransform);

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

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
            geometryModel.Transform = transformGroup;

            return geometryModel;
        }

        /// <summary>
        /// Creates Main curve of the pie slice.
        /// </summary>
        /// <param name="points">Mash points</param>
        /// <param name="normals">Mash normals</param>
        /// <param name="texturePoints">Mash texture positions</param>
        /// <param name="indices">Mash indices</param>
        /// <param name="startAngle">The start angle of the pie slice</param>
        /// <param name="endAngle">The end angle of the pie slice</param>
        /// <param name="radius">The pie radius</param>
        /// <param name="z">The half of the pie width</param>
        private void CreateMainCurve(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double startAngle, double endAngle, double radius, double z)
        {
            Point3D centerPoint = new Point3D();
            double centerX, centerZ;
            FindCenterPointFromRadius(radius, z, radius, -z, _pieRounding * z, 0, 0, out centerX, out centerZ);
            centerPoint.X = centerX;
            centerPoint.Z = centerZ;
            Point3DCollection pointsArc = GenerateArcPoints(new Point3D(radius, 0, z), new Point3D(radius, 0, -z), centerPoint, 5);
            CreateMainCurve(centerPoint, pointsArc, points, normals, texturePoints, indices, startAngle, endAngle, radius, z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <param name="pointsArc"></param>
        /// <param name="points">Mash points</param>
        /// <param name="normals">Mash normals</param>
        /// <param name="texturePoints">Mash texture positions</param>
        /// <param name="indices">Mash indices</param>
        /// <param name="startAngle">The start angle of the pie slice</param>
        /// <param name="endAngle">The end angle of the pie slice</param>
        /// <param name="radius">The pie radius</param>
        /// <param name="z">The half of the pie width</param>
        private void CreateMainCurve(Point3D centerPoint, Point3DCollection pointsArc, Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double startAngle, double endAngle, double radius, double z)
        {
            int indicesNum = points.Count;

            // Smooth unit is used to define the smoothes of the pie slice curve. It depends of the pie slice size.
            double numSegments = GetNumberOfSegments(startAngle, endAngle, _smooth);
            double smoothStep = (endAngle - startAngle) / numSegments;

            int numArcs = pointsArc.Count;
            double currentAngle = startAngle;
            double txtWidth = 1.0 / numSegments;
            double txtHeight = 1.0 / numArcs * 0.3;
            for (int segment = 0; segment <= numSegments; segment++)
            {
                for (int arcIndex = 0; arcIndex < numArcs; arcIndex++)
                {
                    // Points
                    points.Add(FindRadialPointZ(currentAngle, pointsArc[arcIndex].X, pointsArc[arcIndex].Z));

                    // Textures
                    texturePoints.Add(new Point(txtWidth * segment, txtHeight * arcIndex));

                    // Normals
                    normals.Add(FindRadialPointZ(currentAngle, pointsArc[arcIndex].X, pointsArc[arcIndex].Z) - FindRadialPointZ(currentAngle, centerPoint.X, centerPoint.Z));
                }
                currentAngle += smoothStep;
            }
            for (int segment = 0; segment < numSegments; segment++)
            {
                for (int arcIndex = 0; arcIndex < numArcs - 1; arcIndex++)
                {
                    int indx = segment * numArcs + arcIndex;
                    // Indices
                    indices.Add(indx + numArcs + indicesNum);
                    indices.Add(indx + 1 + indicesNum);
                    indices.Add(indx + 0 + indicesNum);
                    indices.Add(indx + numArcs + indicesNum);
                    indices.Add(indx + numArcs + 1 + indicesNum);
                    indices.Add(indx + 1 + indicesNum);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="points">Mash points</param>
        /// <param name="normals">Mash normals</param>
        /// <param name="texturePoints">Mash texture positions</param>
        /// <param name="indices">Mash indices</param>
        /// <param name="startAngle">The start angle of the pie slice</param>
        /// <param name="endAngle">The end angle of the pie slice</param>
        /// <param name="radius">The pie radius</param>
        /// <param name="z">The half of the pie width</param>
        /// <param name="inverseNormal"></param>
        internal void CreateTop(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double startAngle, double endAngle, double radius, double z, bool inverseNormal)
        {
            int indicesNum = points.Count;

            double innerRadius = radius * _innerRadiusCoef;
            double innerZ = z * _zCoef;

            double sweepAngle = endAngle - startAngle;
            if (_edgeAngle > sweepAngle / 4)
            {
                _edgeAngle = sweepAngle / 4;
            }

            double edgeStartAngle = startAngle + _edgeAngle;
            double edgeEndAngle = endAngle - _edgeAngle;

            // Smooth unit is used to define the smoothes of the pie slice curve. It depends of the pie slice size.
            double numSegments = GetNumberOfSegments(startAngle, endAngle, _smooth);
            double smoothStep = (endAngle - startAngle) / numSegments;
            double edgeSmoothStep = (edgeEndAngle - edgeStartAngle) / numSegments;

            // Smooth unit is used to define the smoothes of the pie slice curve. It depends of the pie slice size.
            Point3D shift = FindRadialPointZ((startAngle + endAngle) / 2, innerRadius * _innerRadiusEdge, innerZ);

            //shift = new Point3D(0, 0, innerZ);

            double currentAngle = startAngle;
            double edgeCurrentAngle = edgeStartAngle;
            points.Add(shift);
            // Texture
            texturePoints.Add(new Point(0.5, 0.5));

            if (inverseNormal)
            {
                normals.Add(new Vector3D(0, 0, -1));
            }
            else
            {
                normals.Add(new Vector3D(0, 0, 1));
            }

            for (int segment = 0; segment <= numSegments; segment++)
            {
                points.Add(FindRadialPointZ(edgeCurrentAngle, innerRadius, innerZ));
                // Textures
                double textStep = 1.0 / numSegments;
                texturePoints.Add(new Point(0, textStep * segment));

                if (inverseNormal)
                {
                    normals.Add(new Vector3D(0, 0, -1));
                }
                else
                {
                    normals.Add(new Vector3D(0, 0, 1));
                }

                currentAngle += smoothStep;
                edgeCurrentAngle += edgeSmoothStep;

            }

            for (int segment = 0; segment < numSegments; segment++)
            {
                if (inverseNormal)
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
        /// 
        /// </summary>
        /// <param name="points">Mash points</param>
        /// <param name="normals">Mash normals</param>
        /// <param name="texturePoints">Mash texture positions</param>
        /// <param name="indices">Mash indices</param>
        /// <param name="startAngle">The start angle of the pie slice</param>
        /// <param name="endAngle">The end angle of the pie slice</param>
        /// <param name="radius">The pie radius</param>
        /// <param name="z">The half of the pie width</param>
        /// <param name="inverseNormal"></param>
        internal void CreateSideEdge(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double startAngle, double endAngle, double radius, double z, bool inverseNormal)
        {
            double innerRadius = radius * _innerRadiusCoef;
            double innerZ = z * _zCoef;

            double sweepAngle = endAngle - startAngle;
            if (_edgeAngle > sweepAngle / 4)
            {
                _edgeAngle = sweepAngle / 4;
            }

            double edgeStartAngle = startAngle + _edgeAngle;
            double edgeEndAngle = endAngle - _edgeAngle;

            Point3D shift;
            shift = FindRadialPointZ((startAngle + endAngle) / 2.0, innerRadius * _innerRadiusEdge, innerZ);
            //shift = new Point3D(0, 0, innerZ);
            if (inverseNormal)
            {
                this.AddPolygon(normals, points, texturePoints, indices, new Point3D(0, 0, z), FindRadialPointZ(startAngle, radius, z), FindRadialPointZ(edgeStartAngle, innerRadius, innerZ), shift);
                this.AddPolygon(normals, points, texturePoints, indices, new Point3D(0, 0, z), shift, FindRadialPointZ(edgeEndAngle, innerRadius, innerZ), FindRadialPointZ(endAngle, radius, z));

            }
            else
            {
                this.AddPolygon(normals, points, texturePoints, indices, new Point3D(0, 0, z), shift, FindRadialPointZ(edgeStartAngle, innerRadius, innerZ), FindRadialPointZ(startAngle, radius, z));
                this.AddPolygon(normals, points, texturePoints, indices, new Point3D(0, 0, z), FindRadialPointZ(endAngle, radius, z), FindRadialPointZ(edgeEndAngle, innerRadius, innerZ), shift);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points">Mash points</param>
        /// <param name="normals">Mash normals</param>
        /// <param name="texturePoints">Mash texture positions</param>
        /// <param name="indices">Mash indices</param>
        /// <param name="startAngle">The start angle of the pie slice</param>
        /// <param name="endAngle">The end angle of the pie slice</param>
        /// <param name="radius">The pie radius</param>
        /// <param name="z">The half of the pie width</param>
        /// <param name="inverseNormal"></param>
        private void CreateCurveEdge(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double startAngle, double endAngle, double radius, double z, bool inverseNormal)
        {
            int indicesNum = points.Count;

            double innerRadius = radius * _innerRadiusCoef;
            double innerZ = z * _zCoef;

            double sweepAngle = endAngle - startAngle;
            if (_edgeAngle > sweepAngle / 4)
            {
                _edgeAngle = sweepAngle / 4;
            }

            double edgeStartAngle = startAngle + _edgeAngle;
            double edgeEndAngle = endAngle - _edgeAngle;


            // Smooth unit is used to define the smoothes of the pie slice curve. It depends of the pie slice size.
            double numSegments = GetNumberOfSegments(startAngle, endAngle, _smooth);
            double smoothStep = (endAngle - startAngle) / numSegments;
            double edgeSmoothStep = (edgeEndAngle - edgeStartAngle) / numSegments;

            double currentAngle = startAngle;
            double edgeCurrentAngle = edgeStartAngle;
            double txtStep = 1.0 / (numSegments + 1);
            for (int segment = 0; segment < numSegments; segment++)
            {
                Point3D pt1 = FindRadialPointZ(currentAngle, radius, z);
                Point3D pt2 = FindRadialPointZ(edgeCurrentAngle, innerRadius, innerZ);
                Point3D pt3 = FindRadialPointZ(edgeCurrentAngle + edgeSmoothStep, innerRadius, innerZ);
                Point3D pt4 = FindRadialPointZ(currentAngle + smoothStep, radius, z);


                points.Add(pt1);
                points.Add(pt2);
                points.Add(pt3);
                points.Add(pt4);

                // Textures
                texturePoints.Add(new Point(txtStep * segment, 0));
                texturePoints.Add(new Point(txtStep * segment, 1));
                texturePoints.Add(new Point(txtStep * segment, 1));
                texturePoints.Add(new Point(txtStep * segment, 0));

                for (int normalIndex = 0; normalIndex <= 3; normalIndex++)
                {
                    if (inverseNormal)
                    {
                        normals.Add(NormalVector(pt2, pt3, pt1, pt2));
                    }
                    else
                    {
                        normals.Add(NormalVector(pt1, pt2, pt2, pt3));
                    }
                }

                if (inverseNormal)
                {
                    indices.Add(segment * 4 + 2 + indicesNum);
                    indices.Add(segment * 4 + 1 + indicesNum);
                    indices.Add(segment * 4 + 0 + indicesNum);
                    indices.Add(segment * 4 + 3 + indicesNum);
                    indices.Add(segment * 4 + 2 + indicesNum);
                    indices.Add(segment * 4 + 0 + indicesNum);
                }
                else
                {
                    indices.Add(segment * 4 + 0 + indicesNum);
                    indices.Add(segment * 4 + 1 + indicesNum);
                    indices.Add(segment * 4 + 2 + indicesNum);
                    indices.Add(segment * 4 + 0 + indicesNum);
                    indices.Add(segment * 4 + 2 + indicesNum);
                    indices.Add(segment * 4 + 3 + indicesNum);
                }

                currentAngle += smoothStep;
                edgeCurrentAngle += edgeSmoothStep;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points">Mash points</param>
        /// <param name="normals">Mash normals</param>
        /// <param name="texturePoints">Mash texture positions</param>
        /// <param name="indices">Mash indices</param>
        /// <param name="startAngle">The start angle of the pie slice</param>
        /// <param name="endAngle">The end angle of the pie slice</param>
        /// <param name="radius">The pie radius</param>
        /// <param name="z">The half of the pie width</param>
        internal void CreateSides(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double startAngle, double endAngle, double radius, double z)
        {
            this.AddPolygon(normals, points, texturePoints, indices, new Point3D(0, 0, z), FindRadialPointZ(startAngle, radius, z), FindRadialPointZ(startAngle, radius, -z), new Point3D(0, 0, -z));
            this.AddPolygon(normals, points, texturePoints, indices, new Point3D(0, 0, z), new Point3D(0, 0, -z), FindRadialPointZ(endAngle, radius, -z), FindRadialPointZ(endAngle, radius, z));

            CreateCurveSides(points, normals, texturePoints, indices, startAngle, endAngle, radius, z);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points">Mash points</param>
        /// <param name="normals">Mash normals</param>
        /// <param name="texturePoints">Mash texture positions</param>
        /// <param name="indices">Mash indices</param>
        /// <param name="startAngle">The start angle of the pie slice</param>
        /// <param name="endAngle">The end angle of the pie slice</param>
        /// <param name="radius">The pie radius</param>
        /// <param name="z">The half of the pie width</param>
        internal protected void CreateCurveSides(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double startAngle, double endAngle, double radius, double z)
        {
            Point3D centerPoint = new Point3D();
            double centerX, centerZ;
            FindCenterPointFromRadius(radius, z, radius, -z, _pieRounding * z, 0, 0, out centerX, out centerZ);
            centerPoint.X = centerX;
            centerPoint.Z = centerZ;
            Point3DCollection pointsArc = GenerateArcPoints(new Point3D(radius, 0, z), new Point3D(radius, 0, -z), centerPoint, 5);

            for (int index = 0; index < pointsArc.Count - 1; index++)
            {
                AddTriangle(normals, points, texturePoints, indices, FindRadialPointZ(startAngle, radius, 0), FindRadialPointZ(startAngle, pointsArc[index].X, pointsArc[index].Z), FindRadialPointZ(startAngle, pointsArc[index + 1].X, pointsArc[index + 1].Z));
                AddTriangle(normals, points, texturePoints, indices, FindRadialPointZ(endAngle, radius, 0), FindRadialPointZ(endAngle, pointsArc[index + 1].X, pointsArc[index + 1].Z), FindRadialPointZ(endAngle, pointsArc[index].X, pointsArc[index].Z));
            }
        }

        /// <summary>
        /// Calculates the number of segments used to create a curve. 
        /// </summary>
        /// <param name="startAngle">The start angle of the shape.</param>
        /// <param name="endAngle">The end angle of the shape.</param>
        /// <param name="smooth">Coefficient of the curve smoothness.</param>
        /// <returns>The number of segments as double</returns>
        static internal double GetNumberOfSegments(double startAngle, double endAngle, double smooth)
        {
            // Calculate the number of segments
            double numSegments = Math.Floor((endAngle - startAngle) * smooth / Math.PI);

            // The number of segments must be minimum 1.
            if (numSegments == 0)
            {
                numSegments = 1;
            }

            return numSegments;
        }

        private void CreateLabel(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double width, double height, double z)
        {
            z *= _zCoef * 1.1;

            AddPolygon(normals, points, texturePoints, indices, new Point3D(-width, -height, -z), new Point3D(-width, height, -z), new Point3D(width, height, -z), new Point3D(width, -height, -z));
            AddPolygon(normals, points, texturePoints, indices, new Point3D(width, -height, z), new Point3D(width, height, z), new Point3D(-width, height, z), new Point3D(-width, -height, z));
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