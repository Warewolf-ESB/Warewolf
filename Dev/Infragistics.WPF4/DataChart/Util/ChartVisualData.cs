using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Text;
using System.Windows.Controls;
using System.Collections;

namespace Infragistics.Controls.Charts.VisualData
{
    /// <summary>
    /// Represents visual data extracted from a chart for use by external tools and functionality.
    /// </summary>
    [DontObfuscate]
    public class ChartVisualData
    {
        /// <summary>
        /// Constructs a ChartVisualData
        /// </summary>
        public ChartVisualData()
        {
            Axes = new AxisVisualDataList();
            Series = new SeriesVisualDataList();
        }

        /// <summary>
        /// Data about the axes of the chart.
        /// </summary>
        public AxisVisualDataList Axes { get; set; }

        /// <summary>
        /// Data about the series of the chart.
        /// </summary>
        public SeriesVisualDataList Series { get; set; }

        /// <summary>
        /// The name of the chart.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Has the data been scaled against a viewport for normalization.
        /// </summary>
        public bool IsViewportScaled { get; set; }

        /// <summary>
        /// Scales the chart data against a viewport to normalize the data for comparison.
        /// </summary>
        public void ScaleByViewport()
        {
            foreach (var axis in Axes)
            {
                axis.ScaleByViewport();
            }
            foreach (var series in Series)
            {
                series.ScaleByViewport();
            }
        }
    }

    /// <summary>
    /// Represents a list of series visual data.
    /// </summary>
    [DontObfuscate]
    public class SeriesVisualDataList
        : List<SeriesVisualData>
    {

    }

    /// <summary>
    /// Represents the extracted visual data of a series.
    /// </summary>
    [DontObfuscate]
    public class SeriesVisualData
    {
        /// <summary>
        /// Constructs a SeriesVisualData.
        /// </summary>
        public SeriesVisualData()
        {
            Shapes = new PrimitiveVisualDataList();
            MarkerShapes = new MarkerVisualDataList();
        }

        /// <summary>
        /// The name of the series.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the series.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The viewport of the series.
        /// </summary>
        public Rect Viewport { get; set; }

        /// <summary>
        /// The visual shape data of the series.
        /// </summary>
        public PrimitiveVisualDataList Shapes { get; set; }

        /// <summary>
        /// The visual marker shape data of the series.
        /// </summary>
        public MarkerVisualDataList MarkerShapes { get; set; }

        /// <summary>
        /// Normalizes the shape data of the series by its viewport to enable comparison.
        /// </summary>
        internal void ScaleByViewport()
        {
            foreach (var shape in Shapes)
            {
                shape.ScaleByViewport(Viewport);
            }
            foreach (var markerShape in MarkerShapes)
            {
                markerShape.ScaleByViewport(Viewport);
            }
        }
    }

    /// <summary>
    /// Represents visual information about series markers.
    /// </summary>
    [DontObfuscate]
    public class MarkerVisualDataList
        : List<MarkerVisualData>
    {

    }

    /// <summary>
    /// Represents visual data about a series marker.
    /// </summary>
    [DontObfuscate]
    public class MarkerVisualData
    {
        /// <summary>
        /// The x position of the marker.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The y position of the marker.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// The index of the marker data.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The template applied to the marker.
        /// </summary>
        public DataTemplate ContentTemplate { get; set; }

        /// <summary>
        /// The visibility of the marker.
        /// </summary>
        public Visibility Visibility { get; set; }

        internal void ScaleByViewport(Rect Viewport)
        {
            X = (X - Viewport.Left) / Viewport.Width;
            Y = (Y - Viewport.Top) / Viewport.Height;
        }
    }

    /// <summary>
    /// Represents a list of axis visual data.
    /// </summary>
    [DontObfuscate]
    public class AxisVisualDataList
        : List<AxisVisualData>
    {

    }

    /// <summary>
    /// Represents visual data for an axis.
    /// </summary>
    [DontObfuscate]
    public class AxisVisualData
    {
        /// <summary>
        /// Constructs an AxisVisualData.
        /// </summary>
        public AxisVisualData()
        {
            Labels = new AxisLabelVisualDataList();
        }

        /// <summary>
        /// The name of the axis.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the axis.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The viewport of the axis.
        /// </summary>
        public Rect Viewport { get; set; }

        /// <summary>
        /// Visual information about the labels of the axis.
        /// </summary>
        public AxisLabelVisualDataList Labels { get; set; }

        /// <summary>
        /// Visual information about the axis line.
        /// </summary>
        public PrimitiveVisualData AxisLine { get; set; }

        /// <summary>
        /// Visual information about the major lines of the axis.
        /// </summary>
        public PrimitiveVisualData MajorLines { get; set; }

        /// <summary>
        /// Visual information about the minor lines of the axis.
        /// </summary>
        public PrimitiveVisualData MinorLines { get; set; }

        internal void ScaleByViewport()
        {
            bool isHorizontal = true;
            if (Type == "NumericYAxis" ||
                Type == "CategoryYAxis")
            {
                isHorizontal = false;
            }

            foreach (var label in Labels)
            {
                label.ScaleByViewport(Viewport, isHorizontal);
            }
            AxisLine.ScaleByViewport(Viewport);
        }
    }

    /// <summary>
    /// Provides information about primitive visual elements.
    /// </summary>
    [DontObfuscate]
    public class PrimitiveVisualDataList
        : List<PrimitiveVisualData>
    {
        /// <summary>
        /// Returns the items matching the categorical tag.
        /// </summary>
        /// <param name="tag">The tag to match.</param>
        /// <returns>The returned visual data.</returns>
        public PrimitiveVisualDataList ContainingTag(string tag)
        {
            PrimitiveVisualDataList ret = new PrimitiveVisualDataList();
            for (int i = 0; i < Count; i++)
            {
                PrimitiveVisualData curr = this[i];
                for (int j = 0; j < curr.Tags.Count; j++)
                {
                    if (curr.Tags[j] == tag)
                    {
                        ret.Add(curr);
                        break;
                    }
                }
            }
            return ret;
        }
    }

    /// <summary>
    /// Provides information about axis label elements.
    /// </summary>
    [DontObfuscate]
    public class AxisLabelVisualDataList
        : List<AxisLabelVisualData>
    {
        
    }

    /// <summary>
    /// Provides information about an axis label visual element.
    /// </summary>
    [DontObfuscate]
    public class AxisLabelVisualData
    {
        /// <summary>
        /// The value of the label.
        /// </summary>
        public object LabelValue { get; set; }

        /// <summary>
        /// The position of the label along the axis.
        /// </summary>
        public double LabelPosition { get; set; }

        /// <summary>
        /// The appearance of the label
        /// </summary>
        public LabelAppearanceData Appearance { get; set; }

        internal void ScaleByViewport(Rect Viewport, bool isHorizontal)
        {
            if (isHorizontal)
            {
                LabelPosition = (LabelPosition - Viewport.Left) / Viewport.Width;
            }
            else
            {
                LabelPosition = (LabelPosition - Viewport.Top) / Viewport.Height;
            }
        }
    }

    /// <summary>
    /// Describes appearance information about a label.
    /// </summary>
    [DontObfuscate]
    public class LabelAppearanceData
    {
        /// <summary>
        /// The Label Text.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// Describes appearance information about a primitive element.
    /// </summary>
    [DontObfuscate]
    public class PrimitiveAppearanceData
    {
        /// <summary>
        /// The stroke color of the pimitive.
        /// </summary>
        public Color Stroke { get; set; }

        /// <summary>
        /// The fill color of the primitive.
        /// </summary>
        public Color Fill { get; set; }
        
        /// <summary>
        /// The thickness of the stroke of the primitive.
        /// </summary>
        public double StrokeThickness { get; set; }

        /// <summary>
        /// Whether the primitive is visible.
        /// </summary>
        public Visibility Visibility { get; set; }

        /// <summary>
        /// The opacity of teh primitive.
        /// </summary>
        public double Opacity { get; set; }

        /// <summary>
        /// The left position of the primitive.
        /// </summary>
        public double CanvasLeft { get; set; }

        /// <summary>
        /// The top position of the primitive.
        /// </summary>
        public double CanvasTop { get; set; }

        /// <summary>
        /// The z index of the primitive.
        /// </summary>
        public int CanvaZIndex { get; set; }

        /// <summary>
        /// Stroke dash array of the primitive.
        /// </summary>



        public IEnumerable DashArray { get; set; }

        /// <summary>
        /// The value of the line dash cap.
        /// </summary>
        public int DashCap { get; set; }

        internal void ScaleByViewport(Rect viewport)
        {
            CanvasLeft = (CanvasLeft - viewport.Left) / viewport.Width;
            CanvasTop = (CanvasTop - viewport.Top) / viewport.Height;
        }
    }

    /// <summary>
    /// Describes how point saliency is determined.
    /// </summary>
    [DontObfuscate]
    public class GetPointsSettings
    {
        /// <summary>
        /// Describes whether the start element of shapes should be ignored due to duplicate points.
        /// </summary>
        public bool IgnoreFigureStartPoint { get; set; }
    }

    /// <summary>
    /// Describes information about a primitive visual element.
    /// </summary>
    [DontObfuscate]
    public abstract class PrimitiveVisualData
    {
        /// <summary>
        /// Constructs a primitive visual element.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        public PrimitiveVisualData(string name)
        {
            Name = name;
            Tags = new ShapeTags();
            Appearance = new PrimitiveAppearanceData();
        }

        /// <summary>
        /// The visual appearance of the element.
        /// </summary>
        public PrimitiveAppearanceData Appearance { get; set; }

        /// <summary>
        /// Information tags that categorize the intent of the element.
        /// </summary>
        public ShapeTags Tags { get; set; }

        /// <summary>
        /// The type of the visual element.
        /// </summary>
        public abstract string Type
        {
            get;
        }

        /// <summary>
        /// The name of the visual element.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="viewport"></param>
        public virtual void ScaleByViewport(Rect viewport)
        {
            Appearance.ScaleByViewport(viewport);
        }

        /// <summary>
        /// Gets the salient points associated with this visual element.
        /// </summary>
        /// <param name="settings">Describes how point saliency should be determined.</param>
        /// <returns>Groupings of points associated with the element.</returns>
        public List<List<Point>> GetPoints(GetPointsSettings settings)
        {
            var points = new List<List<Point>>();
            
            GetPointsOverride(points, settings);

            return points;
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public abstract void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings);
    }

    /// <summary>
    /// Describes visual information for a rectangle visual.
    /// </summary>
    [DontObfuscate]
    public class RectangleVisualData
        : PrimitiveVisualData
    {
        /// <summary>
        /// Constructs a RectangleVisualData.
        /// </summary>
        public RectangleVisualData()
            : base("rect1")
        {

        }

        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Constructs a RectangleVisualData based on a source Rectangle.
        /// </summary>
        /// <param name="name">The name of the visual data.</param>
        /// <param name="rect">The source Rectangle.</param>
        public RectangleVisualData(string name, Rectangle rect)
            : base(name)
        {
            Width = rect.Width;
            Height = rect.Height;
            AppearanceHelper.GetShapeAppearance(Appearance, rect);
        }

        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get { return "Rectangle"; }
        }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="viewport"></param>
        public override void ScaleByViewport(Rect viewport)
        {
            base.ScaleByViewport(viewport);
            Width = Width / viewport.Width;
            Height = Height / viewport.Height;
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings)
        {
            List<Point> current = new List<Point>();
            points.Add(current);
            current.Add(new Point(Appearance.CanvasLeft, Appearance.CanvasTop));
            current.Add(new Point(Appearance.CanvasLeft + Width, Appearance.CanvasTop));
            current.Add(new Point(Appearance.CanvasLeft + Width, Appearance.CanvasTop + Height));
            current.Add(new Point(Appearance.CanvasLeft, Appearance.CanvasTop + Height));
        }
    }

    /// <summary>
    /// Represents categorical information to provide context for a visual element.
    /// </summary>
    [DontObfuscate]
    public class ShapeTags
        : List<string>
    {
        
    }

    /// <summary>
    /// Describes visual information for a line visual.
    /// </summary>
    [DontObfuscate]
    public class LineVisualData
        : PrimitiveVisualData
    {
        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get { return "Line"; }
        }

        /// <summary>
        /// Constructs a LineVisualData.
        /// </summary>
        public LineVisualData()
            : base("line1")
        {

        }

        /// <summary>
        /// Constructs a LineVisualData based on a source Line.
        /// </summary>
        /// <param name="name">The name of the visual data.</param>
        /// <param name="line">The source Line.</param>
        public LineVisualData(string name, Line line)
            : base(name)
        {
            X1 = line.X1;
            Y1 = line.Y1;
            X2 = line.X2;
            Y2 = line.Y2;

            AppearanceHelper.GetShapeAppearance(Appearance, line);
        }

        /// <summary>
        /// The x position of the end of the line.
        /// </summary>
        public double X1 { get; set; }
        /// <summary>
        /// The y position of the end of the line.
        /// </summary>
        public double Y1 { get; set; }
        /// <summary>
        /// The x position of the end of the line.
        /// </summary>
        public double X2 { get; set; }
        /// <summary>
        /// The y position of the end of the line.
        /// </summary>
        public double Y2 { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="viewport"></param>
        public override void ScaleByViewport(Rect viewport)
        {
            base.ScaleByViewport(viewport);

            X1 = (X1 - viewport.Left) / viewport.Width;
            Y1 = (Y1 - viewport.Top) / viewport.Height;
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings)
        {
            var current = new List<Point>();
            points.Add(current);
            current.Add(new Point(X1, Y1));
            current.Add(new Point(X2, Y2));
        }
    }

    /// <summary>
    /// Describes visual information for a polyline visual.
    /// </summary>
    [DontObfuscate]
    public class PolyLineVisualData
        : PrimitiveVisualData
    {
        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get { return "Polyline"; }
        }

        /// <summary>
        /// Constructs a PolyLineVisualData.
        /// </summary>
        public PolyLineVisualData()
            : base("polyLine1")
        {

        }

        /// <summary>
        /// Constructs a PolyLineVisualData based on a source Polyline.
        /// </summary>
        /// <param name="name">The name of the visual data.</param>
        /// <param name="line">The source Polyline.</param>
        public PolyLineVisualData(string name, Polyline line)
            : base(name)
        {
            Points = line.Points;

            AppearanceHelper.GetShapeAppearance(Appearance, line);
        }

        /// <summary>
        /// The points in the polyline.
        /// </summary>
        public PointCollection Points { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="viewport"></param>
        public override void ScaleByViewport(Rect viewport)
        {
            base.ScaleByViewport(viewport);

            for (var i = 0; i < Points.Count; i++)
            {
                Points[i] = new Point(
                (Points[i].X - viewport.Left) / viewport.Width,
                (Points[i].Y - viewport.Top) / viewport.Height);
            }
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings)
        {
            var current = new List<Point>();
            points.Add(current);
            for (var i = 0; i < Points.Count; i++)
            {
                current.Add(Points[i]);
            }
        }
    }

    /// <summary>
    /// Describes visual information for a polygon visual.
    /// </summary>
    [DontObfuscate]
    public class PolygonVisualData
        : PrimitiveVisualData
    {
        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get { return "Polygon"; }
        }

        /// <summary>
        /// Constructs a PolygonVisualData.
        /// </summary>
        public PolygonVisualData()
            : base("polygon1")
        {

        }

        /// <summary>
        /// Constructs a PolygonVisualData based on a source Polygon.
        /// </summary>
        /// <param name="name">The name of the visual data.</param>
        /// <param name="line">The source Polygon.</param>
        public PolygonVisualData(string name, Polygon polygon)
            : base(name)
        {
            Points = polygon.Points;

            AppearanceHelper.GetShapeAppearance(Appearance, polygon);
        }

        /// <summary>
        /// The points in the polygon.
        /// </summary>
        public PointCollection Points { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="viewport"></param>
        public override void ScaleByViewport(Rect viewport)
        {
            base.ScaleByViewport(viewport);

            for (var i = 0; i < Points.Count; i++)
            {
                Points[i] = new Point(
                (Points[i].X - viewport.Left) / viewport.Width,
                (Points[i].Y - viewport.Top) / viewport.Height);
            }
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings)
        {
            var current = new List<Point>();
            points.Add(current);
            for (var i = 0; i < Points.Count; i++)
            {
                current.Add(Points[i]);
            }
        }
    }

    /// <summary>
    /// Describes visual information for a path visual.
    /// </summary>
    [DontObfuscate]
    public class PathVisualData
        : PrimitiveVisualData
    {
        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get { return "Path"; }
        }

        /// <summary>
        /// Constructs a PathVisualData.
        /// </summary>
        public PathVisualData()
            : base("path1")
        {
            Data = new List<GeometryData>();
        }

        /// <summary>
        /// Constructs a PathVisualData based on a source Path.
        /// </summary>
        /// <param name="name">The name of the visual data.</param>
        /// <param name="path">The source Path.</param>
        public PathVisualData(string name, Path path)
            : base(name)
        {
            Data = AppearanceHelper.FromGeometry(path.Data);

            AppearanceHelper.GetShapeAppearance(Appearance, path);
        }

        /// <summary>
        /// The data in the path.
        /// </summary>
        public List<GeometryData> Data { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="viewport"></param>
        public override void ScaleByViewport(Rect viewport)
        {
            base.ScaleByViewport(viewport);
            foreach (var data in Data)
            {
                data.ScaleByViewport(viewport);
            }
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings)
        {
            for (var i = 0; i < Data.Count; i++)
            {
                var data = Data[i];
                data.GetPointsOverride(points, settings);
            }
        }
    }

    /// <summary>
    /// Describes visual information for a geometry visual.
    /// </summary>
    [DontObfuscate]
    public abstract class GeometryData
    {
        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="Viewport"></param>
        public abstract void ScaleByViewport(Rect Viewport);

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public abstract void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings);
    }

    /// <summary>
    /// Describes visual information for a path geometry.
    /// </summary>
    [DontObfuscate]
    public class PathGeometryData
        : GeometryData
    {
        /// <summary>
        /// Constructs a PathGeometryData.
        /// </summary>
        public PathGeometryData()
        {
            Figures = new List<PathFigureData>();
        }

        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get
            {
                return "Path";
            }
        }

        /// <summary>
        /// The figures in the path.
        /// </summary>
        public List<PathFigureData> Figures { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="Viewport"></param>
        public override void ScaleByViewport(Rect Viewport)
        {
            foreach (var figure in Figures)
            {
                figure.ScaleByViewport(Viewport);
            }
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings)
        {
            for (var i = 0; i < Figures.Count; i++)
            {
                var figure = Figures[i];
                figure.GetPointsOverride(points, settings);
            }
        }
    }

    /// <summary>
    /// Describes visual information for a line geometry.
    /// </summary>
    [DontObfuscate]
    public class LineGeometryData
        : GeometryData
    {
        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get { return "Line"; }
        }

        /// <summary>
        /// The x position of the start of the line.
        /// </summary>
        public double X1 { get; set; }
        /// <summary>
        /// The y position of the start of the line.
        /// </summary>
        public double Y1 { get; set; }
        /// <summary>
        /// The x position of the end of the line.
        /// </summary>
        public double X2 { get; set; }
        /// <summary>
        /// The y position of the end of the line.
        /// </summary>
        public double Y2 { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="Viewport"></param>
        public override void ScaleByViewport(Rect Viewport)
        {
            X1 = (X1 - Viewport.Left) / Viewport.Width;
            Y1 = (Y1 - Viewport.Top) / Viewport.Height;
            X2 = (X2 - Viewport.Left) / Viewport.Width;
            Y2 = (Y2 - Viewport.Top) / Viewport.Height;
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings)
        {
            var current = new List<Point>();
            points.Add(current);
            current.Add(new Point(X1, Y1));
            current.Add(new Point(X2, Y2));
        }
    }

    /// <summary>
    /// Describes visual information for a rectangle geometry.
    /// </summary>
    [DontObfuscate]
    public class RectangleGeometryData
        : GeometryData
    {
        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get { return "Rectangle"; }
        }

        /// <summary>
        /// The x position of the rectangle.
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// The y position of the rectangle.
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="Viewport"></param>
        public override void ScaleByViewport(Rect Viewport)
        {
            X = (X - Viewport.Left) / Viewport.Width;
            Y = (Y - Viewport.Top) / Viewport.Height;
            Width = Width / Viewport.Width;
            Height = Height / Viewport.Height;
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings)
        {
            var current = new List<Point>();
            points.Add(current);
            current.Add(new Point(X, Y));
            current.Add(new Point(X + Width, Y));
            current.Add(new Point(X + Width, Y + Height));
            current.Add(new Point(X, Y + Height));
        }
    }

    /// <summary>
    /// Describes visual information for an ellipse geometry.
    /// </summary>
    [DontObfuscate]
    public class EllipseGeometryData
        : GeometryData
    {
        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get { return "Ellipse"; }
        }

        /// <summary>
        /// The x coordinate of the center of the ellipse.
        /// </summary>
        public double CenterX { get; set; }
        /// <summary>
        /// The y coordinate of the center of the ellipse.
        /// </summary>
        public double CenterY { get; set; }
        /// <summary>
        /// The x radius of the ellipse.
        /// </summary>
        public double RadiusX { get; set; }
        /// <summary>
        /// The y radius of the ellipse.
        /// </summary>
        public double RadiusY { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="Viewport"></param>
        public override void ScaleByViewport(Rect Viewport)
        {
            CenterX = (CenterX - Viewport.Left) / Viewport.Width;
            CenterX = (CenterY - Viewport.Top) / Viewport.Height;
            RadiusX = RadiusX / Viewport.Width;
            RadiusY = RadiusY / Viewport.Height;
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings)
        {
            var current = new List<Point>();
            points.Add(current);
            current.Add(new Point(CenterX, CenterY));
        }
    }

    /// <summary>
    /// Describes visual information for a path figure.
    /// </summary>
    [DontObfuscate]
    public class PathFigureData
    {
        /// <summary>
        /// Constructs a PathFigureData.
        /// </summary>
        public PathFigureData()
        {
            Segments = new List<SegmentData>();
            StartPoint = new Point();
        }

        /// <summary>
        /// Constructs a PathFigureData based on a source PathFigure.
        /// </summary>
        /// <param name="fig">The source PathFigure.</param>
        public PathFigureData(PathFigure fig)
        {
            Segments = new List<SegmentData>();
            StartPoint = fig.StartPoint;
            for (var i = 0; i < fig.Segments.Count; i++)
            {
                var seg = fig.Segments[i];
                SegmentData newData = null;


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

                if (seg is LineSegment)
                {
                    newData = new LineSegmentData((LineSegment)seg);
                }
                else if (seg is PolyLineSegment)
                {
                    newData = new PolylineSegmentData((PolyLineSegment)seg);
                }
                else if (seg is ArcSegment)
                {
                    newData = new ArcSegmentData((ArcSegment)seg);
                }

                Segments.Add(newData);
            }
        }

        /// <summary>
        /// The start point of the path.
        /// </summary>
        public Point StartPoint { get; set; }

        /// <summary>
        /// The segments in the path.
        /// </summary>
        public List<SegmentData> Segments { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="Viewport"></param>
        internal void ScaleByViewport(Rect Viewport)
        {
            StartPoint = new Point(
            (StartPoint.X - Viewport.Left) / Viewport.Width,
            (StartPoint.Y - Viewport.Top) / Viewport.Height);

            for (var i = 0; i < Segments.Count; i++)
            {
                Segments[i].ScaleByViewport(Viewport);
            }
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="points">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public void GetPointsOverride(List<List<Point>> points, GetPointsSettings settings)
        {
            var current = new List<Point>();
            points.Add(current);
            if (!settings.IgnoreFigureStartPoint)
            {
                current.Add(new Point(StartPoint.X, StartPoint.Y));
            }

            for (var i = 0; i < Segments.Count; i++)
            {
                Segments[i].GetPointsOverride(current, settings);
            }
        }
    }

    /// <summary>
    /// Describes visual information for a segment.
    /// </summary>
    [DontObfuscate]
    public abstract class SegmentData
    {
        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="Viewport"></param>
        public abstract void ScaleByViewport(Rect Viewport);

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="current">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public abstract void GetPointsOverride(List<Point> current, GetPointsSettings settings);
    }

    /// <summary>
    /// Describes visual information for a line segment.
    /// </summary>
    [DontObfuscate]
    public class LineSegmentData
        : SegmentData
    {
        /// <summary>
        /// Constructs a LineSegmentData.
        /// </summary>
        public LineSegmentData()
        {
            Point = new Point();
        }

        /// <summary>
        /// Constructs a LineSegmentData based on a source LineSegment.
        /// </summary>
        /// <param name="seg">The source LineSegment.</param>
        public LineSegmentData(LineSegment seg)
        {
            Point = seg.Point;
        }
        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get { return "Line"; }
        }

        /// <summary>
        /// The end point of the line segment.
        /// </summary>
        public Point Point { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="Viewport"></param>
        public override void ScaleByViewport(Rect Viewport)
        {
            Point = new Point(
            (Point.X - Viewport.Left) / Viewport.Width,
            (Point.Y - Viewport.Top) / Viewport.Height);
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="current">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<Point> current, GetPointsSettings settings)
        {
            current.Add(new Point(Point.X, Point.Y));
        }
    }

    /// <summary>
    /// Describes visual information for a polyline segment.
    /// </summary>
    [DontObfuscate]
    public class PolylineSegmentData
        : SegmentData
    {
        /// <summary>
        /// Constructs a PolylineSegmentData.
        /// </summary>
        public PolylineSegmentData()
        {
            Points = new PointCollection();
        }

        /// <summary>
        /// Constructs a PolylineSegmentData based on a source PolyLineSegment.
        /// </summary>
        /// <param name="poly">The source PolylineSegment.</param>
        public PolylineSegmentData(PolyLineSegment poly)
        {
            Points = poly.Points;
        }

        /// <summary>
        /// Returns the type name of the visual data.
        /// </summary>
        public override string Type
        {
            get { return "Polyline"; }
        }

        /// <summary>
        /// The points in the segment.
        /// </summary>
        public PointCollection Points { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="Viewport"></param>
        public override void ScaleByViewport(Rect Viewport)
        {
            for (var i = 0; i < Points.Count; i++)
            {
                Points[i] = new Point(
                (Points[i].X - Viewport.Left) / Viewport.Width,
                (Points[i].Y - Viewport.Top) / Viewport.Height);
            }
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="current">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<Point> current, GetPointsSettings settings)
        {
            for (var i = 0; i < Points.Count; i++)
            {
                current.Add(new Point(Points[i].X, Points[i].Y));
            }
        }
    }

    /// <summary>
    /// Information data about an arc segment.
    /// </summary>
    [DontObfuscate]
    public class ArcSegmentData
        : SegmentData
    {
        /// <summary>
        /// Constructs an ArcSegmentData
        /// </summary>
        public ArcSegmentData()
        {
            Point = new Point();
            IsLargeArc = false;
            IsCounterClockwise = true;
            RotationAngle = 0;
        }

        /// <summary>
        /// Constructs an ArcSegmentData based on a source arc.
        /// </summary>
        /// <param name="arc">The arc to extract data from.</param>
        public ArcSegmentData(ArcSegment arc)
        {
            Point = arc.Point;
            IsLargeArc = arc.IsLargeArc;
            IsCounterClockwise = arc.SweepDirection == SweepDirection.Counterclockwise;
            SizeX = arc.Size.Width;
            SizeY = arc.Size.Height;
            RotationAngle = arc.RotationAngle;
        }

        /// <summary>
        /// The type of this data.
        /// </summary>
        public override string Type
        {
            get { return "Arc"; }
        }

        /// <summary>
        /// The center point of the arc.
        /// </summary>
        public Point Point { get; set; }
        /// <summary>
        /// Whether the arc is a large arc.
        /// </summary>
        public bool IsLargeArc { get; set; }
        /// <summary>
        /// The sweep direction of the arc.
        /// </summary>
        public bool IsCounterClockwise { get; set; }
        /// <summary>
        /// The x size os the arg.
        /// </summary>
        public double SizeX { get; set; }
        /// <summary>
        /// The y size of the arc.
        /// </summary>
        public double SizeY { get; set; }
        /// <summary>
        /// The rotation angle of the arc.
        /// </summary>
        public double RotationAngle { get; set; }

        /// <summary>
        /// Called to scale this data by a viewport for normalization.
        /// </summary>
        /// <param name="Viewport"></param>
        public override void ScaleByViewport(Rect Viewport)
        {
            Point = new Point(
            (Point.X - Viewport.Left) / Viewport.Width,
            (Point.Y - Viewport.Top) / Viewport.Height);
            SizeX = SizeX / Viewport.Width;
            SizeY = SizeY / Viewport.Height;
        }

        /// <summary>
        /// Called to extract the salient points from this data.
        /// </summary>
        /// <param name="current">The points container.</param>
        /// <param name="settings">Information about how point saliency is determined.</param>
        public override void GetPointsOverride(List<Point> current, GetPointsSettings settings)
        {
            current.Add(new Point(Point.X, Point.Y));
        }
    }

    /// <summary>
    /// Assists in storing information about chart visual appearance.
    /// </summary>
    [DontObfuscate]
    public class AppearanceHelper
    {
        /// <summary>
        /// Returns the color from a brush.
        /// </summary>
        /// <param name="b">The brush to extract the color from.</param>
        /// <returns>The returned color.</returns>
        public static Color FromBrush(Brush b)
        {
            if (b == null)
            {
                return Color.FromArgb(0, 0, 0, 0); 
            }



            Color toRet = Color.FromArgb(0,0,0,0);
            if (b is SolidColorBrush)
            {
                toRet = ((SolidColorBrush)b).Color;
            }
            else if (b is LinearGradientBrush)
            {
                toRet = ((LinearGradientBrush)b).GradientStops[0].Color;
            }
            else if (b is RadialGradientBrush)
            {
                toRet = ((RadialGradientBrush)b).GradientStops[0].Color;
            }

            return toRet;

        }

        /// <summary>
        /// Gets the left position of an element.
        /// </summary>
        /// <param name="line">The element to get the left position of.</param>
        /// <returns>The left position.</returns>
        public static double GetCanvasLeft(FrameworkElement line)
        {



            return Canvas.GetLeft(line);

        }

        /// <summary>
        /// Gets the top position of an element.
        /// </summary>
        /// <param name="line">The element to get the top position of.</param>
        /// <returns>The top position.</returns>
        public static double GetCanvasTop(FrameworkElement line)
        {



            return Canvas.GetTop(line);

        }

        /// <summary>
        /// Gets the z-index of an element.
        /// </summary>
        /// <param name="line">The element to get the z index of</param>
        /// <returns>The z index of the element.</returns>
        public static int GetCanvasZIndex(FrameworkElement line)
        {



            return Canvas.GetZIndex(line);

        }

        /// <summary>
        /// Gets the geometry data for a path.
        /// </summary>
        /// <param name="path">The path to get data for.</param>
        /// <returns>The geometry data for a path.</returns>
        public static List<GeometryData> FromPathData(Path path)
        {
            return FromGeometry(path.Data);
        }

        /// <summary>
        /// Gets data about the provided geometry.
        /// </summary>
        /// <param name="data">The geometry to get data for.</param>
        /// <returns>The list of geometry data extracted.</returns>
        public static List<GeometryData> FromGeometry(Geometry data)
        {
            if (data == null)
            {
                return new List<GeometryData>();
            }

            if (data is GeometryGroup)
            {
                List<GeometryData> ret = new List<GeometryData>();
                var group = (GeometryGroup)data;
                for (var i = 0; i < group.Children.Count; i++)
                {
                    var items = FromGeometry(group.Children[i]);
                    for (var j = 0; j < items.Count; j++)
                    {
                        ret.Add(items[j]);
                    }
                }
                return ret;
            }
            else if (data is PathGeometry)
            {
                return FromPathGeometry((PathGeometry)data);
            }
            else if (data is LineGeometry)
            {
                return FromLineGeometry((LineGeometry)data);
            }
            else if (data is RectangleGeometry)
            {
                return FromRectangleGeometry((RectangleGeometry)data);
            }
            else if (data is EllipseGeometry)
            {
                return FromEllipseGeometry((EllipseGeometry)data);
            }
            else
            {
                throw new Exception("not supported");
            }
        }

        private static List<GeometryData> FromEllipseGeometry(EllipseGeometry ellipseGeometry)
        {
            List<GeometryData> ret = new List<GeometryData>();
            EllipseGeometryData newData = new EllipseGeometryData();
            ret.Add(newData);
            newData.CenterX = ellipseGeometry.Center.X;
            newData.CenterY = ellipseGeometry.Center.Y;
            newData.RadiusX = ellipseGeometry.RadiusX;
            newData.RadiusY = ellipseGeometry.RadiusY;

            return ret;
        }

        private static List<GeometryData> FromRectangleGeometry(RectangleGeometry rectangleGeometry)
        {
            List<GeometryData> ret = new List<GeometryData>();
            RectangleGeometryData newData = new RectangleGeometryData();
            ret.Add(newData);
            newData.X = rectangleGeometry.Rect.X;
            newData.Y = rectangleGeometry.Rect.Y;
            newData.Width = rectangleGeometry.Rect.Width;
            newData.Height = rectangleGeometry.Rect.Height;

            return ret;
        }

        private static List<GeometryData> FromLineGeometry(LineGeometry lineGeometry)
        {
            List<GeometryData> ret = new List<GeometryData>();
            LineGeometryData newData = new LineGeometryData();
            ret.Add(newData);
            newData.X1 = lineGeometry.StartPoint.X;
            newData.Y1 = lineGeometry.StartPoint.Y;
            newData.X2 = lineGeometry.EndPoint.X;
            newData.Y2 = lineGeometry.EndPoint.Y;
            
            return ret;
        }

        private static List<GeometryData> FromPathGeometry(PathGeometry pathGeometry)
        {
            List<GeometryData> ret = new List<GeometryData>();
            PathGeometryData newData = new PathGeometryData();
            ret.Add(newData);
            for (var i = 0; i < pathGeometry.Figures.Count; i++)
            {
                var figure = pathGeometry.Figures[i];
                PathFigureData f = new PathFigureData(figure);
                newData.Figures.Add(f);
            }

            return ret;
        }

        /// <summary>
        /// Gets appearance information for a shape.
        /// </summary>
        /// <param name="appearance">The appearance information to populate.</param>
        /// <param name="path">The shape from which to get information.</param>
        public static void GetShapeAppearance(PrimitiveAppearanceData appearance, Shape path)
        {
            appearance.Stroke = AppearanceHelper.FromBrush(path.Stroke);
            appearance.Fill = AppearanceHelper.FromBrush(path.Fill);
            appearance.StrokeThickness = path.StrokeThickness;



            appearance.DashArray = path.StrokeDashArray;

            appearance.DashCap = (int)path.StrokeDashCap;
            appearance.Visibility = path.Visibility;
            appearance.Opacity = path.Opacity;
            appearance.CanvasLeft = AppearanceHelper.GetCanvasLeft(path);
            appearance.CanvasTop = AppearanceHelper.GetCanvasTop(path);
            appearance.CanvaZIndex = AppearanceHelper.GetCanvasZIndex(path);
        }

        /// <summary>
        /// Gets appearance information from a text element.
        /// </summary>
        /// <param name="frameworkElement">The text element to examine.</param>
        /// <returns>The label appearance information obtained.</returns>
        public static LabelAppearanceData FromTextElement(FrameworkElement frameworkElement)
        {
            var lad = new LabelAppearanceData();






            return lad;
        }
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