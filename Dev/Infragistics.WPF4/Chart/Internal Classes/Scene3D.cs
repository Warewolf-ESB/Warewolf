
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Scene 3D contains methods and properties responsible for 
    /// drawing 3D Scene. The 3D scene contains 3 areas XY, XZ and YZ. 
    /// All this areas have background, gridlines and labels.
    /// </summary>
    internal class Scene3D : Primitives3D
    {
        #region Enumeration


        #endregion Enumeration

        #region Fields

        // Private fields
        private double _wallSize = 0.02;
        private double _width = 1;
        private double _height = 1;
        private double _depth = 1;
        private double _labelAreaShiftX = 0.014;
        private double _labelAreaShiftY = 0.01;
        private double _labelAreaShiftZ = 0.01;
        private double _thickness3D = 0.005;
        private Chart _chart;
        private double _defaultFontSize = 250;
        
        #endregion Fields

        #region Properties
        
        
        #endregion Properties

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="chart">Chart container for this scene</param>
        /// <param name="wallSize">The scene wall size</param>
        /// <param name="width">The scene width</param>
        /// <param name="height">The scene height</param>
        /// <param name="depth">The scene depth</param>
        internal Scene3D(Chart chart, double wallSize, double width, double height, double depth)
        {
            _wallSize = wallSize;
            _width = width;
            _height = height;
            _depth = depth;
            _chart = chart;
        }

        /// <summary>
        /// Gets Scene type. 
        /// </summary>
        internal bool IsSceneType(SceneType type)
        {
            return _chart.ChartCreator.IsSceneType(type);
        }

        /// <summary>
        /// Draws the 3D scene without data points. The scene has 3 sides, grids, labels etc. 
        /// </summary>
        /// <param name="model3DGroup">Collection of 3-D models.</param>
        /// <param name="rotateTransform3D">Specifies a rotation transformation.</param>
        protected override void Draw3D(Model3DGroup model3DGroup, RotateTransform3D rotateTransform3D)
        {
            // Set Scene depth
            _depth = Math.Abs(Chart.ChartCreator.AxisZ.RoundedMaximum - Chart.ChartCreator.AxisZ.RoundedMinimum) / Math.Abs(Chart.ChartCreator.AxisX.RoundedMaximum - Chart.ChartCreator.AxisX.RoundedMinimum);
            if (_depth > 0.5)
            {
                _depth = 0.5;
            }

            if (_depth < 0.1)
            {
                _depth = 0.1;
            }

            // Set Scene height and width
            _height = 0.5;
            _width = 1;
            
            if (IsSceneType(SceneType.Bar))
            {
                _width *= 0.8;
                _height *= 1.2;
                Chart.ChartCreator.AxisZ.Scene3DSize = _depth;
                Chart.ChartCreator.AxisX.Scene3DSize = _height;
                Chart.ChartCreator.AxisY.Scene3DSize = _width;
            }
            else if(IsSceneType(SceneType.Scatter))
            {
                _depth = 0.4;
                Chart.ChartCreator.AxisZ.Scene3DSize = _depth;
                Chart.ChartCreator.AxisY.Scene3DSize = _height;
                Chart.ChartCreator.AxisX.Scene3DSize = _width;
            }
            else
            {
                Chart.ChartCreator.AxisZ.Scene3DSize = _depth;
                Chart.ChartCreator.AxisX.Scene3DSize = _width;
                Chart.ChartCreator.AxisY.Scene3DSize = _height;
            }

            Brush sceneFill = Brushes.White;

            if (Chart.GetContainer().Scene != null)
            {
                _wallSize = Chart.GetContainer().Scene.Scene3DThickness / 100;
                if (Chart.GetContainer().Scene.GridArea != null)
                {
                    sceneFill = Chart.GetContainer().Scene.Scene3DBrush;
                }
            }

            // Create 3D sides
            model3DGroup.Children.Add(Create(sceneFill, rotateTransform3D));

            // Create grids
            CreateSceneSide(rotateTransform3D, model3DGroup.Children, Space3DPlane.YZ, sceneFill);
            CreateSceneSide(rotateTransform3D, model3DGroup.Children, Space3DPlane.XZ, sceneFill);
            CreateSceneSide(rotateTransform3D, model3DGroup.Children, Space3DPlane.XY, sceneFill);

            // Create labels
            CreateLabels(rotateTransform3D, model3DGroup.Children, Space3DAxis.Y);
            CreateLabels(rotateTransform3D, model3DGroup.Children, Space3DAxis.Z);
            CreateLabels(rotateTransform3D, model3DGroup.Children, Space3DAxis.X);
        }

        /// <summary>
        /// Create 3D sides of the scene: XY, XZ and YZ
        /// </summary>
        /// <param name="brush">Brush of the scene sides.</param>
        /// <param name="rotateTransform3D">Specifies a rotation transformation.</param>
        /// <returns>A 3-D model comprised of a MeshGeometry3D and a Material.</returns>
        private GeometryModel3D Create(Brush brush, RotateTransform3D rotateTransform3D)
        {
            // Create a 3-D model
            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D plane.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            // Create 3 scene sides
            Cube(points, normals, textures, indices, new Point3D(-_width / 2 - _wallSize / 2, -_wallSize / 2, -_wallSize / 2), _wallSize, _height + _wallSize, _depth + _wallSize, Space3DPlane.YZ);
            Cube(points, normals, textures, indices, new Point3D(-_wallSize / 2, -_height / 2 - _wallSize / 2, -_wallSize / 2), _width + _wallSize, _wallSize, _depth + _wallSize, Space3DPlane.XZ);
            Cube(points, normals, textures, indices, new Point3D(-_wallSize / 2, -_wallSize / 2, -_depth / 2 - _wallSize / 2), _width + _wallSize, _height + _wallSize, _wallSize, Space3DPlane.XY);

            // Set Vectors
            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TriangleIndices = indices;

            // Create material for the scene
            MaterialGroup materials = new MaterialGroup();

            DiffuseMaterial colorMaterial = new DiffuseMaterial(brush);
            SpecularMaterial specularMaterial = new SpecularMaterial(brush, 32);

            materials.Children.Add(colorMaterial);
            materials.Children.Add(specularMaterial);
            geometryModel.Material = materials;

            // Set transformation matrix
            TranslateTransform3D translate = new TranslateTransform3D(0, 0, 0);

            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(rotateTransform3D);
            transformGroup.Children.Add(translate);

            geometryModel.Transform = transformGroup;

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

            return geometryModel;
        }

        /// <summary>
        /// Creates textures with gridlines for 3D scene.
        /// </summary>
        /// <param name="rotateTransform3D">Specifies a rotation transformation.</param>
        /// <param name="modelCollection">Represents an ordered collection of Model3D objects.</param>
        /// <param name="plane">A 3D plane</param>
        /// <param name="brush">Brush of the scene sides.</param>
        private void CreateSceneSide(RotateTransform3D rotateTransform3D, Model3DCollection modelCollection, Space3DPlane plane, Brush brush)
        {
            // Create a 3-D model
            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D plane.
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

            // Create 3D vectors and points
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();

            // Create 3D rectangle surface which will keep gridlines texture
            switch (plane)
            {
                case Space3DPlane.YZ:
                    AddPolygon(normals, points, textures, indices, new Point3D(-_width / 2, -_height / 2, -_depth / 2), new Point3D(-_width / 2, _height / 2, -_depth / 2), new Point3D(-_width / 2, _height / 2, _depth / 2), new Point3D(-_width / 2, -_height / 2, _depth / 2));
                    break;
                case Space3DPlane.XZ:
                    AddPolygon(normals, points, textures, indices, new Point3D(-_width / 2, -_height / 2, -_depth / 2), new Point3D(-_width / 2, -_height / 2, _depth / 2), new Point3D(_width / 2, -_height / 2, _depth / 2), new Point3D(_width / 2, -_height / 2, -_depth / 2));
                    break;
                default:
                    AddPolygon(normals, points, textures, indices, new Point3D(-_width / 2, _height / 2, -_depth / 2), new Point3D(-_width / 2, -_height / 2, -_depth / 2), new Point3D(_width / 2, -_height / 2, -_depth / 2), new Point3D(_width / 2, _height / 2, -_depth / 2));
                    break;
            }

            // Set vectors
            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TextureCoordinates = textures;
            meshGeometry3D.TriangleIndices = indices;

            // Create materials
            MaterialGroup materials = new MaterialGroup();

            // Create a Drawing group and context for texture
            DrawingGroup drawing = new DrawingGroup();
            DrawingContext context = drawing.Open();

            // Set horizontal and vertical axes for grids
            AxisType axisHorizontal = AxisType.PrimaryX;
            AxisType axisVertical = AxisType.PrimaryY;
            if (IsSceneType(SceneType.Bar))
            {
                axisHorizontal = AxisType.PrimaryY;
                axisVertical = AxisType.PrimaryX;
            }

            // Draw gridlines
            switch (plane)
            {
                case Space3DPlane.YZ:
                    // Background
                    context.DrawRectangle(brush, new Pen(Brushes.Yellow, 0), new Rect(-_depth / 2, -_height / 2, _depth, _height));
                    
                    // Draw Stripes
                    DrawStripes(context, AxisType.PrimaryZ, axisVertical);

                    // Draw gridlines
                    DrawGridArea(context, AxisType.PrimaryZ, axisVertical, false);
                    DrawGridArea(context, AxisType.PrimaryZ, axisVertical, true);
                    break;
                case Space3DPlane.XZ:
                    // Background
                    context.DrawRectangle(brush, new Pen(Brushes.Yellow, 0), new Rect(-_width / 2, -_depth / 2, _width, _depth));

                    // Draw Stripes
                    DrawStripes(context, axisHorizontal, AxisType.PrimaryZ);

                    // Draw gridlines
                    DrawGridArea(context, axisHorizontal, AxisType.PrimaryZ, false);
                    DrawGridArea(context, axisHorizontal, AxisType.PrimaryZ, true);
                    break;
                default:
                    // Background
                    context.DrawRectangle(brush, new Pen(Brushes.Yellow, 0), new Rect(-_width / 2, -_height / 2, _width, _height));

                    // Draw Stripes
                    DrawStripes(context, axisHorizontal, axisVertical);

                    // Draw gridlines
                    DrawGridArea(context, axisHorizontal, axisVertical, false);
                    DrawGridArea(context, axisHorizontal, axisVertical, true);
                    break;
            }

            // Close drawing context
            context.Close();

            // Create brush from drawing group
            DrawingBrush textureBrush = new DrawingBrush(drawing);

            // Create texture for 3D surface
            DiffuseMaterial colorMaterial = new DiffuseMaterial(textureBrush);
            SpecularMaterial specularMaterial = new SpecularMaterial(textureBrush, 32);

            materials.Children.Add(colorMaterial);
            materials.Children.Add(specularMaterial);
            geometryModel.Material = materials;

            // Set default transformation matrix for model
            TranslateTransform3D translate = new TranslateTransform3D(0, 0, 0);

            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(rotateTransform3D);
            transformGroup.Children.Add(translate);

            geometryModel.Transform = transformGroup;

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

            // Add model
            modelCollection.Add(geometryModel);
        }

        /// <summary>
        /// Draw and animates stripes on the scene surfaces
        /// </summary>
        /// <param name="context">Drawing context from the drawing brush used for scene surface.</param>
        /// <param name="axisTypeV">Axis type for vertical stripes.</param>
        /// <param name="axisTypeH">Axis type for horizontal stripes.</param>
        private void DrawStripes(DrawingContext context, AxisType axisTypeV, AxisType axisTypeH)
        {
            Axis axisH = Chart.GetContainer().Axes.GetAxis(axisTypeH);
            Axis axisV = Chart.GetContainer().Axes.GetAxis(axisTypeV);

            // Draw Vertical stripes
            foreach (Stripe stripe in axisV.Stripes)
            {
                DrawStripeArea(context, axisTypeV, axisTypeH, stripe, true);
            }

            // Draw Horizontal stripes
            foreach (Stripe stripe in axisH.Stripes)
            {
                DrawStripeArea(context, axisTypeV, axisTypeH, stripe, false);
            }
        }

        /// <summary>
        /// Draw and animates grid lines on the scene surfaces
        /// </summary>
        /// <param name="context">Drawing context from the drawing brush used for scene surface.</param>
        /// <param name="axisType1">Axis type of the first axis.</param>
        /// <param name="axisType2">Axis type of the second axis.</param>
        /// <param name="stripe">Stripe to draw</param>
        /// <param name="vertical">True for vertical strips drawing</param>
        private void DrawStripeArea(DrawingContext context, AxisType axisType1, AxisType axisType2, Stripe stripe, bool vertical)
        {
            int index = 0;
            long tickDuration = 0;
            long beginTime = 0;
            AxisValue axisA;
            AxisValue axisB;

            if (axisType2 == AxisType.PrimaryX)
            {
                axisA = AxisX;
            }
            else if (axisType2 == AxisType.PrimaryY)
            {
                axisA = AxisY;
            }
            else
            {
                axisA = AxisZ;
            }

            if (axisType1 == AxisType.PrimaryX)
            {
                axisB = AxisX;
            }
            else if (axisType1 == AxisType.PrimaryY)
            {
                axisB = AxisY;
            }
            else
            {
                axisB = AxisZ;
            }

            double intervalH = axisA.RoundedInterval * 2;
            if (stripe != null && stripe.Unit != 0)
            {
                intervalH = stripe.Unit;
            }

            double intervalV = axisB.RoundedInterval * 2;
            if (stripe != null && stripe.Unit != 0)
            {
                intervalV = stripe.Unit;
            }

            double stripeWidthH = axisA.RoundedInterval;
            if (stripe.Width != 0)
            {
                stripeWidthH = stripe.Width;
            }

            double stripeWidthV = axisB.RoundedInterval;
            if (stripe.Width != 0)
            {
                stripeWidthV = stripe.Width;
            }

            if (vertical)
            {
                // Interval validation
                if ((axisB.RoundedMaximum - axisB.RoundedMinimum) / intervalV > 50)
                {
                    // Stripe Unit value too small.
                    throw new InvalidOperationException(ErrorString.Exc17);
                }
            }
            else
            {
                if ((axisA.RoundedMaximum - axisA.RoundedMinimum) / intervalH > 50)
                {
                    // Stripe Unit value too small.
                    throw new InvalidOperationException(ErrorString.Exc17);
                }
            }

            Pen pen = new Pen(Brushes.Gray, 1);
            Brush fill = Brushes.LightGray;

            // Set Stroke 
            if (stripe.Stroke != null)
            {
                pen.Brush = stripe.Stroke;
            }

            // Set Fill
            if (stripe.Fill != null)
            {
                fill = stripe.Fill;
            }

            pen.Thickness = _thickness3D * stripe.StrokeThickness;

            RectAnimation rectAnimation;
            if (vertical)
            {
                // Vertical stripes on texture
                rectAnimation = GetRectAnimation(axisType1, ref tickDuration, ref beginTime, ref index, stripe);
                for (double position = axisB.RoundedMinimum; position + stripeWidthV <= axisB.RoundedMaximum; position += intervalV)
                {
                    Rect from = new Rect(new Point(axisB.GetPosition(position), axisA.GetPosition(axisA.RoundedMaximum)), new Point(axisB.GetPosition(position + stripeWidthV), axisA.GetPosition(axisA.RoundedMaximum)));
                    Rect to = new Rect(new Point(axisB.GetPosition(position), axisA.GetPosition(axisA.RoundedMaximum)), new Point(axisB.GetPosition(position + stripeWidthV), axisA.GetPosition(axisA.RoundedMinimum)));

                    DrawRectangle(context, pen, fill, rectAnimation, from, to, ref index, tickDuration, beginTime);
                }
            }
            else
            {
                // Horizontal stripes on texture
                rectAnimation = GetRectAnimation(axisType2, ref tickDuration, ref beginTime, ref index, stripe);
                for (double position = axisA.RoundedMinimum; position + stripeWidthH <= axisA.RoundedMaximum; position += intervalH)
                {
                    Rect from;
                    Rect to;
                    // Special case for YZ plane!
                    if (axisA.AxisType == AxisType.PrimaryY && axisB.AxisType == AxisType.PrimaryX)
                    {
                        from = new Rect(new Point(axisB.GetPosition(axisB.RoundedMinimum), axisA.GetPosition(axisA.RoundedMaximum + axisA.RoundedMinimum - position)), new Point(axisB.GetPosition(axisB.RoundedMinimum), axisA.GetPosition(axisA.RoundedMaximum + axisA.RoundedMinimum - position - stripeWidthH)));
                        to = new Rect(new Point(axisB.GetPosition(axisB.RoundedMinimum), axisA.GetPosition(axisA.RoundedMaximum + axisA.RoundedMinimum - position)), new Point(axisB.GetPosition(axisB.RoundedMaximum), axisA.GetPosition(axisA.RoundedMaximum + axisA.RoundedMinimum - position - stripeWidthH)));
                    }
                    else
                    {
                        from = new Rect(new Point(axisB.GetPosition(axisB.RoundedMinimum), axisA.GetPosition(position)), new Point(axisB.GetPosition(axisB.RoundedMinimum), axisA.GetPosition(position + stripeWidthH)));
                        to = new Rect(new Point(axisB.GetPosition(axisB.RoundedMinimum), axisA.GetPosition(position)), new Point(axisB.GetPosition(axisB.RoundedMaximum), axisA.GetPosition(position + stripeWidthH)));
                    }
                    
                    DrawRectangle(context, pen, fill, rectAnimation, from, to, ref index, tickDuration, beginTime);
                }
            }
        }

        /// <summary>
        /// Draw and animates grid lines on the scene surfaces
        /// </summary>
        /// <param name="context">Drawing context from the drawing brush used for scene surface.</param>
        /// <param name="axisType1">Axis type of the first axis.</param>
        /// <param name="axisType2">Axis type of the second axis.</param>
        /// <param name="isMajor">True if major gridlines are drawn.</param>
        private void DrawGridArea(DrawingContext context, AxisType axisType1, AxisType axisType2, bool isMajor)
        {
            int index = 0;
            long tickDuration = 0;
            long beginTime = 0;
            AxisValue axisH;
            AxisValue axisV;
            double intervalV = 0;
            double intervalH = 0;

            Mark markV = Chart.GetContainer().Axes.GetMark(axisType1, true, isMajor);
            Mark markH = Chart.GetContainer().Axes.GetMark(axisType2, true, isMajor);

            // Minor Gridlines disabled
            if (markV != null && !isMajor && markV.Unit == 0)
            {
                return;
            }

            if (markH != null && !isMajor && markH.Unit == 0)
            {
                return;
            }

            if (axisType2 == AxisType.PrimaryX)
            {
                axisH = AxisX;
            }
            else if (axisType2 == AxisType.PrimaryY)
            {
                axisH = AxisY;
            }
            else
            {
                axisH = AxisZ;
            }

            if (axisType1 == AxisType.PrimaryX)
            {
                axisV = AxisX;
            }
            else if (axisType1 == AxisType.PrimaryY)
            {
                axisV = AxisY;
            }
            else
            {
                axisV = AxisZ;
            }

            if (markV != null && markV.Unit != 0)
            {
                intervalV = markV.Unit;
            }
            else
            {
                intervalV = axisV.RoundedInterval;
            }

            if (markH != null && markH.Unit != 0)
            {
                intervalH = markH.Unit;
            }
            else
            {
                intervalH = axisH.RoundedInterval;
            }

            // Interval validation
            if ((axisV.RoundedMaximum - axisV.RoundedMinimum) / intervalV > 50)
            {
                // Mark Unit value too small.
                throw new InvalidOperationException(ErrorString.Exc18);
            }

            if ((axisH.RoundedMaximum - axisH.RoundedMinimum) / intervalH > 50)
            {
                // Mark Unit value too small.
                throw new InvalidOperationException(ErrorString.Exc18);
            }

            Pen pen;
            PointAnimation pointAnimation;

            // Vertical gridlines on texture
            if (markV == null || markV.Visible)
            {
                pen = GetPen(axisType1, isMajor);
                pointAnimation = GetPointAnimation(axisType1, ref tickDuration, ref beginTime, ref index, isMajor);
                for (double position = axisV.RoundedMinimum; position <= axisV.RoundedMaximum; position += intervalV)
                {
                    Point from = new Point(axisV.GetPosition(position), axisH.GetPosition(axisH.RoundedMaximum));
                    Point to = new Point(axisV.GetPosition(position), axisH.GetPosition(axisH.RoundedMinimum));
                    DrawLine(context, pen, pointAnimation, from, to, ref index, tickDuration, beginTime);
                }
            }

            // Horizontal gridlines on texture
            if (markH == null || markH.Visible)
            {
                pen = GetPen(axisType2, isMajor);
                pointAnimation = GetPointAnimation(axisType2, ref tickDuration, ref beginTime, ref index, isMajor);
                for (double position = axisH.RoundedMinimum; position <= axisH.RoundedMaximum; position += intervalH)
                {
                    Point from;
                    Point to;
                    // Special case for YZ plane!
                    if (axisH.AxisType == AxisType.PrimaryY && axisV.AxisType == AxisType.PrimaryX)
                    {
                        from = new Point(axisV.GetPosition(axisV.RoundedMinimum), axisH.GetPosition(axisH.RoundedMaximum) + axisH.GetPosition(axisH.RoundedMinimum) - axisH.GetPosition(position));
                        to = new Point(axisV.GetPosition(axisV.RoundedMaximum), axisH.GetPosition(axisH.RoundedMaximum) + axisH.GetPosition(axisH.RoundedMinimum) - axisH.GetPosition(position));
                    }
                    else
                    {
                        from = new Point(axisV.GetPosition(axisV.RoundedMinimum), axisH.GetPosition(position));
                        to = new Point(axisV.GetPosition(axisV.RoundedMaximum), axisH.GetPosition(position));
                    }
                    DrawLine(context, pen, pointAnimation, from, to, ref index, tickDuration, beginTime);
                }
            }
        }


        private PointAnimation GetPointAnimation(AxisType axisType, ref long tickDuration, ref long beginTime, ref int index, bool isMajor)
        {
            // Fill point animation from chart animation
            PointAnimation pointAnimation = null;
            AxisValue axisValue;
            Animation animation = null;
            Mark mark = Chart.GetContainer().Axes.GetMark(axisType, true, isMajor);

            index = 0;

            if (mark != null)
            {
                animation = mark.Animation;
            }

            if (axisType == AxisType.PrimaryX)
            {
                axisValue = AxisX;
            }
            else if (axisType == AxisType.PrimaryY)
            {
                axisValue = AxisY;
            }
            else
            {
                axisValue = AxisZ;
            }

            double interval = axisValue.RoundedInterval;
            if (mark != null && mark.Unit != 0)
            {
                interval = mark.Unit;
            }

            if (animation != null)
            {
                pointAnimation = new PointAnimation();
                pointAnimation.BeginTime = animation.BeginTime;
                pointAnimation.Duration = animation.Duration;
                pointAnimation.AccelerationRatio = animation.AccelerationRatio;
                pointAnimation.DecelerationRatio = animation.DecelerationRatio;
                pointAnimation.RepeatBehavior = animation.RepeatBehavior;

                // Calculate begin time and duration for sequential animation.
                if (animation.Sequential)
                {
                    Duration duration = Duration.Automatic;

                    int numOfLines = PlottingGridsArea.GetNumOfTicks(axisValue, interval);

                    if (animation.Duration.HasTimeSpan)
                    {
                        tickDuration = animation.Duration.TimeSpan.Ticks / (numOfLines * 2);
                        duration = new Duration(new TimeSpan(tickDuration));
                    }

                    pointAnimation.Duration = duration;
                    beginTime = animation.BeginTime.Ticks;
                }
            }

            return pointAnimation;
        }

        private void DrawLine(DrawingContext dc, Pen pen, PointAnimation pointAnimation, Point from, Point to, ref int index, long tickDuration, long beginTime)
        {
            AnimationClock clock = null;
            if (pointAnimation != null)
            {
                pointAnimation.From = from;
                pointAnimation.To = to;

                pointAnimation.BeginTime = new TimeSpan(beginTime + tickDuration * index);

                // Create a clock for the animation.
                clock = pointAnimation.CreateClock();
                AnimationClock emptyClock = new PointAnimation().CreateClock();

                dc.DrawLine(pen, from, emptyClock, from, clock);
            }
            else
            {
                dc.DrawLine(pen, from, to);
            }

            index++;
        }

        private RectAnimation GetRectAnimation(AxisType axisType, ref long tickDuration, ref long beginTime, ref int index, Stripe stripe)
        {
            // Fill point animation from chart animation
            RectAnimation rectAnimation = null;
            AxisValue axisValue;
            Animation animation = null;
            
            index = 0;

            animation = stripe.Animation;
            
            if (axisType == AxisType.PrimaryX)
            {
                axisValue = AxisX;
            }
            else if (axisType == AxisType.PrimaryY)
            {
                axisValue = AxisY;
            }
            else
            {
                axisValue = AxisZ;
            }

            double interval = axisValue.RoundedInterval;
            if (stripe.Unit != 0)
            {
                interval = stripe.Unit;
            }

            if (animation != null)
            {
                rectAnimation = new RectAnimation();
                rectAnimation.BeginTime = animation.BeginTime;
                rectAnimation.Duration = animation.Duration;
                rectAnimation.AccelerationRatio = animation.AccelerationRatio;
                rectAnimation.DecelerationRatio = animation.DecelerationRatio;
                rectAnimation.RepeatBehavior = animation.RepeatBehavior;

                // Calculate begin time and duration for sequential animation.
                if (animation.Sequential)
                {
                    Duration duration = Duration.Automatic;

                    int numOfLines = PlottingGridsArea.GetNumOfTicks(axisValue, interval);

                    if (animation.Duration.HasTimeSpan)
                    {
                        tickDuration = animation.Duration.TimeSpan.Ticks / (numOfLines);
                        duration = new Duration(new TimeSpan(tickDuration));
                    }

                    rectAnimation.Duration = duration;
                    beginTime = animation.BeginTime.Ticks;
                }
            }

            return rectAnimation;
        }

        private void DrawRectangle(DrawingContext dc, Pen pen, Brush fill, RectAnimation rectAnimation, Rect from, Rect to, ref int index, long tickDuration, long beginTime)
        {
            AnimationClock clock = null;
            if (rectAnimation != null)
            {
                rectAnimation.From = from;
                rectAnimation.To = to;

                rectAnimation.BeginTime = new TimeSpan(beginTime + tickDuration * index);
                
                // Create a clock for the animation.
                clock = rectAnimation.CreateClock();

                dc.DrawRectangle(fill, pen, from, clock);
            }
            else
            {
                dc.DrawRectangle(fill, pen, to);
            }

            index++;
        }

        /// <summary>
        /// Create area with labels. Labels are created as a texture.
        /// </summary>
        /// <param name="rotateTransform3D">Specifies a rotation transformation.</param>
        /// <param name="modelCollection">Represents an ordered collection of Model3D objects.</param>
        /// <param name="spaceAxis">A 3D space axis</param>
        private void CreateLabels(RotateTransform3D rotateTransform3D, Model3DCollection modelCollection, Space3DAxis spaceAxis)
        {
            double fontAreaSize = 0.2;
            double labelMargine = 1.1;
            double labelMargineY = 1.4;
            
            // Create geometry model
            GeometryModel3D geometryModel = new GeometryModel3D();

            // The geometry specifes the shape of the 3D plane. 
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection points = new Point3DCollection();
            PointCollection textures = new PointCollection();
            Int32Collection indices = new Int32Collection();
                        
            // Create label 3D surface as rectangle
            DrawingBrush brush;
            switch (spaceAxis)
            {
                case Space3DAxis.X:
                    if (!IsAxisVisible(AxisType.PrimaryX))
                    {
                        return;
                    }
                    this.AddPolygon(normals, points, textures, indices, new Point3D(-_width / 2 * labelMargine, -_height / 2, _depth / 2 + _labelAreaShiftX), new Point3D(-_width / 2 * labelMargine, -_height / 2 - fontAreaSize, _depth / 2 + _labelAreaShiftX), new Point3D(_width / 2 * labelMargine, -_height / 2 - fontAreaSize, _depth / 2 + _labelAreaShiftX), new Point3D(_width / 2 * labelMargine, -_height / 2, _depth / 2 + _labelAreaShiftX));
                    brush = CreateLabelsBrushX(_width * labelMargine, fontAreaSize, labelMargine);
                    break;
                case Space3DAxis.Y:
                    if (!IsAxisVisible(AxisType.PrimaryY))
                    {
                        return;
                    }
                    this.AddPolygon(normals, points, textures, indices, new Point3D(-_width / 2 - fontAreaSize, _height / 2 * labelMargineY, _depth / 2 + _labelAreaShiftY), new Point3D(-_width / 2 - fontAreaSize, -_height / 2 * labelMargineY, _depth / 2 + _labelAreaShiftY), new Point3D(-_width / 2 - _wallSize, -_height / 2 * labelMargineY, _depth / 2 + _labelAreaShiftY), new Point3D(-_width / 2 - _wallSize, _height / 2 * labelMargineY, _depth / 2 + _labelAreaShiftY));
                    brush = CreateLabelsBrushY(fontAreaSize, _height * labelMargineY, labelMargineY);
                    break;
                default:
                    if (!IsAxisVisible(AxisType.PrimaryZ))
                    {
                        return;
                    }
                    this.AddPolygon(normals, points, textures, indices, new Point3D(_width / 2 + _labelAreaShiftZ, -_height / 2, _depth / 2 * labelMargine), new Point3D(_width / 2 + _labelAreaShiftZ, -_height / 2 - fontAreaSize, _depth / 2 * labelMargine), new Point3D(_width / 2 + _labelAreaShiftZ, -_height / 2 - fontAreaSize, -_depth / 2 * labelMargine), new Point3D(_width / 2 + _labelAreaShiftZ, -_height / 2, -_depth / 2 * labelMargine));
                    brush = CreateLabelsBrushZ(_depth * labelMargine, fontAreaSize, labelMargine);
                    break;
            }

            // Set vectors
            meshGeometry3D.Positions = points;
            meshGeometry3D.Normals = normals;
            meshGeometry3D.TextureCoordinates = textures;
            meshGeometry3D.TriangleIndices = indices;

            // Create texture for 3D surface
            MaterialGroup materials = new MaterialGroup();
            DiffuseMaterial colorMaterial = new DiffuseMaterial(brush);
            SpecularMaterial specularMaterial = new SpecularMaterial(brush, 32);

            materials.Children.Add(colorMaterial);
            materials.Children.Add(specularMaterial);
            geometryModel.Material = materials;

            // Set default transformation matrix for model
            TranslateTransform3D translate = new TranslateTransform3D(0, 0, 0);
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(rotateTransform3D);
            transformGroup.Children.Add(translate);
            geometryModel.Transform = transformGroup;

            // Apply the mesh to the geometry model.
            geometryModel.Geometry = meshGeometry3D;

            // Add model
            modelCollection.Add(geometryModel);
        }

        /// <summary>
        /// Checks is axis are visible
        /// </summary>
        /// <param name="type">Axis type</param>
        /// <returns>True if visible</returns>
        private bool IsAxisVisible(AxisType type)
        {
            if (Chart.GetContainer().Axes.IsAxisVisible(type))
            {
                if (Chart.GetContainer().Axes.GetAxis(type).Label != null && !Chart.GetContainer().Axes.GetAxis(type).Label.Visible)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Create Label Brush for X Axis.
        /// </summary>
        /// <param name="relWidth">The width of a label area in 3D coordinate system.</param>
        /// <param name="relHeight">The height of a label area in 3D coordinate system.</param>
        /// <param name="labelMargine">The relative size of margin.</param>
        /// <returns>The drawing brush with axis labels.</returns>
        private DrawingBrush CreateLabelsBrushX(double relWidth, double relHeight, double labelMargine)
        {
            // Label width in relative coordinates
            double labelWidth = _defaultFontSize * relWidth;

            // Create axis labels
            AxisLabelsHorizontal labelsHorizontal = new AxisLabelsHorizontal();
            labelsHorizontal.Width = labelWidth;
            labelsHorizontal.Height = _defaultFontSize * relHeight;
            labelsHorizontal.TopAxisLabels = true;
            labelsHorizontal.MinimumFontSize = 2;

            AxisValue copyAxis;
            if (IsSceneType(SceneType.Bar))
            {
                labelsHorizontal.SetLabelStyle(Chart, AxisType.PrimaryY);

                // Performs a shallow copy 
                copyAxis = AxisY.Clone();
            }
            else
            {
                labelsHorizontal.SetLabelStyle(Chart, AxisType.PrimaryX);

                // Performs a shallow copy 
                copyAxis = AxisX.Clone();
            }

            copyAxis.SizeInPixels = _defaultFontSize;

            labelsHorizontal.Shift = labelWidth * (labelMargine - 1.0) / 2.0;
            labelsHorizontal.Axis = copyAxis;
            labelsHorizontal.Chart = Chart;
            labelsHorizontal.Is3D = true;
            
            // Fill labels
            labelsHorizontal.FillLabels();

            // Create a Drawing group and context for texture
            DrawingGroup drawing = new DrawingGroup();
            DrawingContext context = drawing.Open();

            labelsHorizontal.Draw(context);
            context.Close();

            // Create drawing brush with lables
            DrawingBrush brush = new DrawingBrush(drawing);

            return brush;
        }

        /// <summary>
        /// Create Label Brush for Y Axis.
        /// </summary>
        /// <param name="relWidth">The width of a label area in 3D coordinate system.</param>
        /// <param name="relHeight">The height of a label area in 3D coordinate system.</param>
        /// <param name="labelMargine">The relative size of margin.</param>
        /// <returns>The drawing brush with axis labels.</returns>
        private DrawingBrush CreateLabelsBrushY(double relWidth, double relHeight, double labelMargine)
        {
            // Label height in relative coordinates
            double labelHeight = _defaultFontSize * relHeight;

            // Create axis labels
            AxisLabelsVertical labelsVertical = new AxisLabelsVertical();
            labelsVertical.Width = _defaultFontSize * relWidth;
            labelsVertical.Height = labelHeight;
            labelsVertical.LabelDistance = 2;
            labelsVertical.TopAxisLabels = true;

            labelsVertical.MinimumFontSize = 6;

            AxisValue copyAxis;
            if (IsSceneType(SceneType.Bar))
            {
                labelsVertical.SetLabelStyle(Chart, AxisType.PrimaryX);

                // Performs a shallow copy 
                copyAxis = AxisX.Clone();
            }
            else
            {
                labelsVertical.SetLabelStyle(Chart, AxisType.PrimaryY);

                // Performs a shallow copy 
                copyAxis = AxisY.Clone();
            }

            copyAxis.SizeInPixels = _defaultFontSize;
            copyAxis.PixelOrientationNegative = true;

            labelsVertical.Axis = copyAxis;
            labelsVertical.Chart = Chart;
            labelsVertical.Is3D = true;
            labelsVertical.Shift = (labelHeight - labelHeight / labelMargine) / 2.0;

            // Fill labels
            labelsVertical.FillLabels();

            // Create a Drawing group and context for texture
            DrawingGroup drawing = new DrawingGroup();
            DrawingContext context = drawing.Open();

            labelsVertical.Draw(context);
            context.Close();

            // Create drawing brush with lables
            DrawingBrush brush = new DrawingBrush(drawing);

            return brush;
        }

        /// <summary>
        /// Create Label Brush for Z Axis.
        /// </summary>
        /// <param name="relWidth">The width of a label area in 3D coordinate system.</param>
        /// <param name="relHeight">The height of a label area in 3D coordinate system.</param>
        /// <param name="labelMargine">The relative size of margin.</param>
        /// <returns>The drawing brush with axis labels.</returns>
        private DrawingBrush CreateLabelsBrushZ(double relWidth, double relHeight, double labelMargine)
        {
            // Label width in relative coordinates
            double labelWidth = _defaultFontSize * relWidth;

            // Create axis labels
            AxisLabelsHorizontal labelsHorizontal = new AxisLabelsHorizontal();
            labelsHorizontal.Width = labelWidth;
            labelsHorizontal.Height = _defaultFontSize * relHeight;
            labelsHorizontal.TopAxisLabels = true;
            labelsHorizontal.MinimumFontSize = 6;
            labelsHorizontal.SetLabelStyle(Chart, AxisType.PrimaryZ);
            labelsHorizontal.Is3D = true;

            // Performs a shallow copy 
            AxisValue copyAxis = AxisZ.Clone();

            copyAxis.SizeInPixels = labelWidth / labelMargine;
            copyAxis.Is3D = false;

            labelsHorizontal.Axis = copyAxis;
            labelsHorizontal.Chart = Chart;
            labelsHorizontal.Shift = labelWidth * (labelMargine - 1.0) / 2.0;

            // Fill labels
            labelsHorizontal.FillLabels();

            // Create a Drawing group and context for texture
            DrawingGroup drawing = new DrawingGroup();
            DrawingContext context = drawing.Open();

            labelsHorizontal.Draw(context);
            context.Close();

            // Create drawing brush with lables
            DrawingBrush brush = new DrawingBrush(drawing);

            return brush;
        }


        /// <summary>
        /// Creates pen for horizontal and vertical gridlines.
        /// </summary>
        /// <param name="axisType">Axis type</param>
        /// <param name="major">True if gridlines are major</param>
        /// <returns>A pen used for gridlines</returns>
        private Pen GetPen(AxisType axisType, bool major)
        {
            Pen pen = new Pen(Brushes.Gray, _thickness3D);
            Mark mark = Chart.GetContainer().Axes.GetMark(axisType, true, major);

            if (mark != null && mark.Stroke != null)
            {
                pen.Brush = mark.Stroke;
                pen.Thickness = _thickness3D * mark.StrokeThickness;
            }

            return pen;
        }

        /// <summary>
        /// Create a cube which represents a side of the 3D scene.
        /// </summary>
        /// <param name="points">A collection of vertex positions for a MeshGeometry3D</param>
        /// <param name="normals">A collection of normal vectors for the MeshGeometry3D</param>
        /// <param name="texturePoints">A collection of texture coordinates for the MeshGeometry3D</param>
        /// <param name="indices">A collection of triangle indices for the MeshGeometry3D</param>
        /// <param name="center">The center of the cube</param>
        /// <param name="width">The width of the cube</param>
        /// <param name="height">The height of the cube</param>
        /// <param name="depth">The depth of the cube</param>
        /// <param name="plane">The side of the scene: XY, YZ or XZ</param>
        private void Cube(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, Point3D center, double width, double height, double depth, Space3DPlane plane)
        {
            // Left Side
            this.AddPolygon(normals, points, texturePoints, indices, new Point3D(center.X - width / 2, center.Y - height / 2, center.Z - depth / 2), new Point3D(center.X - width / 2, center.Y - height / 2, center.Z + depth / 2), new Point3D(center.X - width / 2, center.Y + height / 2, center.Z + depth / 2), new Point3D(center.X - width / 2, center.Y + height / 2, center.Z - depth / 2));

            // Right Side
            if (plane != Space3DPlane.YZ)
            {
                this.AddPolygon(normals, points, texturePoints, indices, new Point3D(center.X + width / 2, center.Y - height / 2, center.Z - depth / 2), new Point3D(center.X + width / 2, center.Y + height / 2, center.Z - depth / 2), new Point3D(center.X + width / 2, center.Y + height / 2, center.Z + depth / 2), new Point3D(center.X + width / 2, center.Y - height / 2, center.Z + depth / 2));
            }

            // Front Side
            if (plane != Space3DPlane.XY)
            {
                this.AddPolygon(normals, points, texturePoints, indices, new Point3D(center.X - width / 2, center.Y - height / 2, center.Z + depth / 2), new Point3D(center.X + width / 2, center.Y - height / 2, center.Z + depth / 2), new Point3D(center.X + width / 2, center.Y + height / 2, center.Z + depth / 2), new Point3D(center.X - width / 2, center.Y + height / 2, center.Z + depth / 2));
            }

            // Back Side
            this.AddPolygon(normals, points, texturePoints, indices, new Point3D(center.X - width / 2, center.Y - height / 2, center.Z - depth / 2), new Point3D(center.X - width / 2, center.Y + height / 2, center.Z - depth / 2), new Point3D(center.X + width / 2, center.Y + height / 2, center.Z - depth / 2), new Point3D(center.X + width / 2, center.Y - height / 2, center.Z - depth / 2));

            // Top Side
            if (plane != Space3DPlane.XZ)
            {
                this.AddPolygon(normals, points, texturePoints, indices, new Point3D(center.X - width / 2, center.Y + height / 2, center.Z - depth / 2), new Point3D(center.X - width / 2, center.Y + height / 2, center.Z + depth / 2), new Point3D(center.X + width / 2, center.Y + height / 2, center.Z + depth / 2), new Point3D(center.X + width / 2, center.Y + height / 2, center.Z - depth / 2));
            }

            // Bottom Side
            this.AddPolygon(normals, points, texturePoints, indices, new Point3D(center.X - width / 2, center.Y - height / 2, center.Z - depth / 2), new Point3D(center.X + width / 2, center.Y - height / 2, center.Z - depth / 2), new Point3D(center.X + width / 2, center.Y - height / 2, center.Z + depth / 2), new Point3D(center.X - width / 2, center.Y - height / 2, center.Z + depth / 2));

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