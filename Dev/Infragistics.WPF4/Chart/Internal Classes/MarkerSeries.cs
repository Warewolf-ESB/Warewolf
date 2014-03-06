#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Media.Animation;
using System.Windows.Input;

#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class MarkerSeries : ChartSeries
    {
        #region Fields

        private ChartSeries _chartSeries;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets chart series
        /// </summary>
        internal ChartSeries ChartSeries
        {
            get
            {
                return _chartSeries;
            }
            set
            {
                _chartSeries = value;
            }
        }

        #endregion Properties

        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

        #endregion ChartTypeparameters

        #region Properties

        #endregion Properties

        #region Methods

        /// <summary>
        /// Draw markers using shape UI elements. 
        /// </summary>
        protected override void Draw2D()
        {
            int seriesNum = SeriesList.Count;

            double xVal;

            if (AxisX == null || AxisY == null)
            {
                return;
            }

            // Used to create cumulative values for stacked chart types
            CreateStackedData();

            // Data series loop
            int clusteredIndex = 0;
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {
                // Do not draw markers for pie and doughnut
                if (SeriesList[seriesIndx].Marker == null && SeriesList[seriesIndx].ChartType == ChartType.Pie || SeriesList[seriesIndx].ChartType == ChartType.Doughnut)
                {
                    continue;
                }

                // Do not draw markers for stock chart types
                if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Stock, SeriesList[seriesIndx]))
                {
                    continue;
                }

                // Do not draw markers 100% stacked charts
                if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Stacked100, SeriesList[seriesIndx]))
                {
                    continue;
                }

                // Clustered chart types
                bool clustered = ChartSeries.GetChartAttribute(ChartTypeAttribute.Clustered, SeriesList[seriesIndx]);

                // Stacked chart types
                bool stacked = ChartSeries.GetChartAttribute(ChartTypeAttribute.Stacked, SeriesList[seriesIndx]);

                // The number of data points
                int pointNum = SeriesList[seriesIndx].DataPoints.Count;

                // Data points loop
                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = SeriesList[seriesIndx].DataPoints[pointIndx];

                    Marker marker = GetMarker(point);
                    bool markerShapeDisabled = false;

                    // Data point X and Y position
                    if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Scatter, SeriesList[seriesIndx]))
                    {
                        xVal = point.GetParameterValueDouble(ChartParameterType.ValueX);
                    }
                    else
                    {
                        xVal = pointIndx + 1;
                    }

                    Point center = new Point();
                    double actualXValue;
                    double actualYValue;
                    bool isBarChart = false;

                    if (Chart.ChartCreator.IsSceneType(SceneType.Bar))
                    {
                        isBarChart = true;

                        if (stacked)
                        {
                            double val = GetStackedValue(seriesIndx, pointIndx);

                            actualXValue = val;
                            actualYValue = xVal;
                        }
                        else if (clustered)
                        {
                            // Search series for point width. Because of cluster and stacked chart types 
                            // all series have to have same point width.
                            double width = ColumnChart2D.FindClusterPointWidth(ChartType.Bar, ChartType.CylinderBar, SeriesList, 0.8);
                            int numOfBarSeries = BarChart2D.GetNumberOfBarSeries(SeriesList);
                            double columnWidth = width / numOfBarSeries;

                            actualXValue = point.Value;
                            actualYValue = xVal + clusteredIndex * columnWidth - width / 2 + columnWidth / 2;
                        }
                        else
                        {
                            actualXValue = point.Value;
                            actualYValue = xVal;
                        }
                    }
                    else if (Chart.ChartCreator.IsSceneType(SceneType.Scatter))
                    {
                        // Data point X and y position
                        xVal = point.GetParameterValueDouble(ChartParameterType.ValueX);
                        double yVal = point.GetParameterValueDouble(ChartParameterType.ValueY);

                        actualXValue = xVal;
                        actualYValue = yVal;
                    }
                    else
                    {
                        if (stacked)
                        {
                            double val = GetStackedValue(seriesIndx, pointIndx);

                            actualXValue = xVal;
                            actualYValue = val;
                        }
                        else if (clustered)
                        {
                            // Search series for point width. Because of cluster and stacked chart types 
                            // all series have to have same point width.
                            double width = ColumnChart2D.FindClusterPointWidth(ChartType.Column, ChartType.Cylinder, SeriesList, 0.8);
                            int numOfColumnSeries = ColumnChart2D.GetNumberOfColumnSeries(SeriesList);
                            double columnWidth = width / numOfColumnSeries;

                            actualXValue = xVal + clusteredIndex * columnWidth - width / 2 + columnWidth / 2;
                            actualYValue = point.Value;
                        }
                        else
                        {
                            actualXValue = xVal;
                            actualYValue = point.Value;
                        }
                    }

                    if (isBarChart == false)
                    {
                        if (AxisX.IsValueVisible(actualXValue) == false ||
                            AxisY.IsValueVisible(actualYValue) == false)
                        {
                            continue;
                        }

                        center.X = AxisX.GetPositionLogarithmic(actualXValue);
                        center.Y = AxisY.GetPositionLogarithmic(actualYValue);
                    }
                    else
                    {
                        if (AxisY.IsValueVisible(actualXValue) == false ||
                            AxisX.IsValueVisible(actualYValue) == false)
                        {
                            continue;
                        }

                        center.X = AxisY.GetPositionLogarithmic(actualXValue);
                        center.Y = AxisX.GetPositionLogarithmic(actualYValue);
                    }

                    if (Chart.ChartCreator.IsSceneType(SceneType.Bubble))
                    {
                        markerShapeDisabled = true;
                    }

                    if (marker != null)
                    {
                        // point.Marker = marker;
                        double markerSize;
                        if (Chart.ChartCreator.IsSceneType(SceneType.Scatter))
                        {
                            markerSize = AxisX.GetSize(Math.Abs(AxisX.RoundedMaximum - AxisX.RoundedMinimum) / (double)7);
                        }
                        else
                        {
                            markerSize = AxisX.GetSize(1);
                        }
                        markerSize *= marker.MarkerSize / 10.0;

                        // Set minimum maker size and maintain proportional size
                        markerSize += 2.0;

                        //Skip Null values
                        if (point.NullValue == true)
                        {
                            continue;
                        }

                        if (!markerShapeDisabled)
                        {
                            if (marker.UseDataTemplate || marker.DataTemplate != null)
                            {
                                AddTemplate(center, AxisX.GetSize(markerSize), point, pointIndx, seriesIndx);
                            }
                            else
                            {
                                switch (marker.Type)
                                {
                                    case MarkerType.Circle:
                                        markerSize *= 1.15;
                                        AddCircleShape(center, markerSize, point, seriesIndx, pointIndx);
                                        break;
                                    case MarkerType.Rectangle:
                                        markerSize *= 1.15;
                                        AddRectangleShape(center, markerSize, point, seriesIndx, pointIndx);
                                        break;
                                    case MarkerType.Triangle:
                                        AddTriangleShape(center, markerSize, point, seriesIndx, pointIndx);
                                        break;
                                    case MarkerType.Hexagon:
                                        AddDiamondShape(center, markerSize, point, seriesIndx, pointIndx);
                                        break;
                                    case MarkerType.Star5:
                                        markerSize *= 2;
                                        AddStarShape(center, markerSize, point, seriesIndx, pointIndx, 5);
                                        break;
                                    case MarkerType.Star6:
                                        markerSize *= 2;
                                        AddStarShape(center, markerSize, point, seriesIndx, pointIndx, 6);
                                        break;
                                    case MarkerType.Star7:
                                        markerSize *= 2;
                                        AddStarShape(center, markerSize, point, seriesIndx, pointIndx, 7);
                                        break;
                                    case MarkerType.Star8:
                                        markerSize *= 2;
                                        AddStarShape(center, markerSize, point, seriesIndx, pointIndx, 8);
                                        break;
                                }
                            }
                        }

                        AddLabel(center, point, markerSize);
                    }
                }
                if (clustered)
                {
                    clusteredIndex++;
                }
            }
        }

        /// <summary>
        /// This method draws data points as shapes.
        /// </summary>
        /// <param name="center">Bubble center.</param>
        /// <param name="radius">The radius of bubble chart.</param>
        /// <param name="point">Coresponding data point.</param>
        /// <param name="pointIndex">Current data point index.</param>
        /// <param name="seriesIndex">Current series index.</param>
        protected void AddTemplate(Point center, double radius, DataPoint point, int pointIndex, int seriesIndex)
        {
            MarkerSeries.AddTemplateInternal(center, radius, point, pointIndex, seriesIndex, this.Chart, this._elements);
        }

        internal static void AddTemplateInternal(Point center, double radius, DataPoint point, int pointIndex, int seriesIndex, Chart chart, UIElementCollection elements)
        {
            ContentPresenter content = new ContentPresenter();
            Marker marker = GetMarker(point);
            
            if (marker.DataTemplate != null) //use the DataTemplate and ignore the MarkerTemplate logic
            {
                content.ContentTemplate = marker.DataTemplate;
                content.Content = point.DataContext;
                content.ToolTip = point.GetToolTip();
            }
            else //use the MarkerTemplate logic - backward compatibility
            {
                MarkerTemplate template = new MarkerTemplate();
                content.Content = template;

                point.UpdateActualFill(seriesIndex, pointIndex);
                template.Fill = point.ActualFill;

                point.UpdateActualStroke(seriesIndex, pointIndex);
                template.Stroke = point.ActualStroke;

                template.StrokeThickness = point.GetStrokeThickness(seriesIndex, pointIndex);
                template.ToolTip = point.GetToolTip();

                if (marker != null && marker.Fill != null)
                {
                    template.Fill = marker.Fill;
                }

                if (marker != null && marker.Stroke != null)
                {
                    template.Stroke = marker.Stroke;
                }

                if (marker != null)
                {
                    template.StrokeThickness = marker.StrokeThickness;
                }
            }

            Canvas.SetLeft(content, center.X - radius / 2);
            Canvas.SetTop(content, center.Y - radius / 2);

            content.Width = radius;
            content.Height = radius;

            DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);
            if (ChartSeries.IsAnimationEnabledInternal(animation, chart))
            {
                ScaleTransform scaleTransform = new ScaleTransform(0, 0);
                animation.From = 0;
                animation.To = 1;

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

                content.RenderTransform = scaleTransform;
                content.RenderTransformOrigin = new Point(0.5, 0.5);
            }
            elements.Add(content);
        }

        /// <summary>
        /// Add a Label to the marker
        /// </summary>
        /// <param name="center">The center of the marker</param>
        /// <param name="point">Data point used for marker text</param>
        /// <param name="markerSize">The marker size</param>
        private void AddLabel(Point center, DataPoint point, double markerSize)
        {
            TextBlock label = new TextBlock();
            label.IsHitTestVisible = false;

            Marker marker = GetMarker(point);

            label.Text = CreateMarkerText(Chart.ChartCreator, point);

            Size size = GetTextSize(label, marker, _elements);

            markerSize *= marker.LabelDistance;

            Point loc = new Point();

            if (Chart.ChartCreator.IsSceneType(SceneType.Bar))
            {
                if (AxisY.GetCrossingPosition() > center.X)
                {
                    loc.X = center.X - markerSize - size.Width;
                }
                else
                {
                    loc.X = center.X + markerSize;
                }

                loc.Y = center.Y - size.Height / 2;
            }
            else if (Chart.ChartCreator.IsSceneType(SceneType.Bubble))
            {
                loc.X = center.X - size.Width / 2;
                loc.Y = center.Y - size.Height / 2;
            }
            else
            {
                loc.X = center.X - size.Width / 2;

                if (AxisY.GetCrossingPosition() >= center.Y)
                {
                    double max = AxisY.GetPosition(AxisY.RoundedMaximum);
                    loc.Y = center.Y - markerSize - size.Height;

                    if (loc.Y <= max)
                    {
                        loc.Y = center.Y + markerSize;
                    }
                }
                else
                {
                    double min = AxisY.GetPosition(AxisY.RoundedMinimum);

                    if (center.Y + size.Height + markerSize >= min)
                    {
                        loc.Y = center.Y - markerSize - size.Height;
                    }
                    else
                    {
                        loc.Y = center.Y + markerSize;
                    }
                }
            }

            switch (marker.LabelOverflow)
            {
                case LabelOverflow.FitInsideGridArea:
                    loc = CorrectLabelPosition(loc, size);
                    break;
            }

            Canvas.SetTop(label, loc.Y);
            Canvas.SetLeft(label, loc.X);

            _elements.Add(label);
        }

        private Point CorrectLabelPosition(Point pt, Size size)
        {
            if (pt.X + size.Width > this.Size.Width)
            {
                pt.X = this.Size.Width - size.Width;
            }

            pt.X = Math.Max(0, pt.X);

            if (pt.Y + size.Height > this.Size.Height)
            {
                pt.Y = this.Size.Height - size.Height;
            }

            pt.Y = Math.Max(0, pt.Y);

            return pt;
        }

        /// <summary>
        /// Creates formated text for marker
        /// </summary>
        /// <param name="chartCreator">Chart Creator</param>
        /// <param name="point">Data point used for marker text</param>
        /// <returns>Formated Marker text</returns>
        static internal string CreateMarkerText(ChartCreator chartCreator, DataPoint point)
        {
            string label;
            Marker marker = GetMarker(point);

            double defaultValue;
            double value;
            double valueX;
            double valueY;
            double valueZ;
            double radius;
            double high;
            double low;
            double open;
            double close;

            if (chartCreator.IsSceneType(SceneType.Bubble))
            {
                defaultValue = point.GetParameterValueDouble(ChartParameterType.Radius);
            }
            else if (chartCreator.IsSceneType(SceneType.Scatter))
            {
                defaultValue = point.GetParameterValueDouble(ChartParameterType.ValueY);
            }
            else
            {
                defaultValue = point.Value;
            }

            value = point.Value;
            bool isDateTime = point.IsDateTimeType(ChartParameterType.ValueX);
            valueX = point.GetParameterValueDouble(ChartParameterType.ValueX);
            valueY = point.GetParameterValueDouble(ChartParameterType.ValueY);
            valueZ = point.GetParameterValueDouble(ChartParameterType.ValueZ);
            radius = point.GetParameterValueDouble(ChartParameterType.Radius);
            high = point.GetParameterValueDouble(ChartParameterType.High);
            low = point.GetParameterValueDouble(ChartParameterType.Low);
            open = point.GetParameterValueDouble(ChartParameterType.Open);
            close = point.GetParameterValueDouble(ChartParameterType.Close);

            CultureInfo cultureToUse = CultureInformation.CultureToUse;

            if (!String.IsNullOrEmpty(marker.Format))
            {
                string format = CreateFormat(marker.Format);
                try
                {
                    if (isDateTime)
                    {
                        DateTime dateTime = DateTime.FromOADate(valueX);
                        label = string.Format(cultureToUse, format, value, dateTime, valueY, valueZ, radius, high, low, open, close);
                    }
                    else
                    {
                        label = string.Format(cultureToUse, format, value, valueX, valueY, valueZ, radius, high, low, open, close);
                    }
                }
                catch (Exception e)
                {
                    // Invalid marker formatting string.
                    throw new InvalidOperationException(ErrorString.Exc55, e);
                }
            }
            else
            {
                label = defaultValue.ToString(cultureToUse);
            }

            return label;
        }

        /// <summary>
        /// Create string format changing property names for values with syntax used for .net string format.
        /// </summary>
        /// <param name="format">Input format string</param>
        /// <returns>.Net format string syntax</returns>
        static private string CreateFormat(string format)
        {
            string newFormat = format;

            newFormat = newFormat.Replace("{ValueX", "{1");
            newFormat = newFormat.Replace("{ValueY", "{2");
            newFormat = newFormat.Replace("{ValueZ", "{3");
            newFormat = newFormat.Replace("{Radius", "{4");
            newFormat = newFormat.Replace("{High", "{5");
            newFormat = newFormat.Replace("{Low", "{6");
            newFormat = newFormat.Replace("{Open", "{7");
            newFormat = newFormat.Replace("{Close", "{8");
            newFormat = newFormat.Replace("{Value", "{0");

            return newFormat;
        }

        static internal Size GetTextSize(TextBlock label, Marker marker, UIElementCollection elements)
        {
            if (marker != null)
            {
                // Take appearance from font properties
                if (marker.ReadLocalValue(Marker.FontFamilyProperty) != DependencyProperty.UnsetValue || ((FontFamily)Marker.FontFamilyProperty.DefaultMetadata.DefaultValue) != marker.FontFamily)
                {
                    label.FontFamily = marker.FontFamily;
                }

                if (marker.ReadLocalValue(Marker.FontSizeProperty) != DependencyProperty.UnsetValue || (double)Marker.FontSizeProperty.DefaultMetadata.DefaultValue != marker.FontSize)
                {
                    label.FontSize = marker.FontSize;
                }

                if (marker.ReadLocalValue(Marker.FontStretchProperty) != DependencyProperty.UnsetValue || (FontStretch)Marker.FontStretchProperty.DefaultMetadata.DefaultValue != marker.FontStretch)
                {
                    label.FontStretch = marker.FontStretch;
                }

                if (marker.ReadLocalValue(Marker.FontStyleProperty) != DependencyProperty.UnsetValue || (FontStyle)Marker.FontStyleProperty.DefaultMetadata.DefaultValue != marker.FontStyle)
                {
                    label.FontStyle = marker.FontStyle;
                }

                if (marker.ReadLocalValue(Marker.FontWeightProperty) != DependencyProperty.UnsetValue || (FontWeight)Marker.FontWeightProperty.DefaultMetadata.DefaultValue != marker.FontWeight)
                {
                    label.FontWeight = marker.FontWeight;
                }

                if (marker.ReadLocalValue(Marker.ForegroundProperty) != DependencyProperty.UnsetValue || (Brush)Marker.ForegroundProperty.DefaultMetadata.DefaultValue != marker.Foreground)
                {
                    SolidColorBrush solidBrush = marker.Foreground as SolidColorBrush;
                    if (solidBrush == null || solidBrush.Color != Color.FromArgb(0, 0, 0, 0))
                    {
                        label.Foreground = marker.Foreground;
                    }
                }

                if (marker.ReadLocalValue(Marker.TextWrappingProperty) != DependencyProperty.UnsetValue || (TextWrapping)Marker.TextWrappingProperty.DefaultMetadata.DefaultValue != marker.TextWrapping)
                {
                    label.TextWrapping = marker.TextWrapping;
                }
            }

            elements.Add(label);
            Typeface typeface = new Typeface(label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch);

            CultureInfo cultureToUse = CultureInformation.CultureToUse;

            FormattedText formattedText = new FormattedText(label.Text, cultureToUse, FlowDirection.LeftToRight, typeface, label.FontSize, Brushes.Black);
            Size size = new Size(formattedText.Width, formattedText.Height);
            elements.Remove(label);
            return size;
        }

        private void AddCircleShape(Point center, double radius, DataPoint point, int seriesIndex, int pointIndex)
        {
            // If fast rendering mode is active use GDI+ rendering
            if (Graphics != null)
            {
                Graphics.SetGdiPen(seriesIndex, pointIndex, point);
                Graphics.SetGdiBrush(seriesIndex, pointIndex, point);
                Graphics.DrawEllipse(center.X - radius / 2, center.Y - radius / 2, radius, radius);
                Graphics.AddHitTestRegionEllipse(center.X - radius / 2, center.Y - radius / 2, radius, radius, seriesIndex, pointIndex);
            }
            else // Use WPF rendering
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = radius;
                ellipse.Height = radius;
                Canvas.SetLeft(ellipse, center.X - radius / 2);
                Canvas.SetTop(ellipse, center.Y - radius / 2);

                SetMarkerParameters(ellipse, point, seriesIndex, pointIndex);
                SetHitTest2D(ellipse, point);
                _elements.Add(ellipse);
            }
        }

        private void AddRectangleShape(Point center, double radius, DataPoint point, int seriesIndex, int pointIndex)
        {
            // If fast rendering mode is active use GDI+ rendering
            if (Graphics != null)
            {
                Graphics.SetGdiPen(seriesIndex, pointIndex, point);
                Graphics.SetGdiBrush(seriesIndex, pointIndex, point);
                Graphics.DrawRectangle(center.X - radius / 2, center.Y - radius / 2, radius, radius);
                Graphics.AddHitTestRegionRectangle(center.X - radius / 2, center.Y - radius / 2, radius, radius, seriesIndex, pointIndex);
            }
            else // Use WPF rendering
            {
                Rectangle rectangle = new Rectangle();
                rectangle.Width = radius;
                rectangle.Height = radius;
                Canvas.SetLeft(rectangle, center.X - radius / 2);
                Canvas.SetTop(rectangle, center.Y - radius / 2);

                SetMarkerParameters(rectangle, point, seriesIndex, pointIndex);
                SetHitTest2D(rectangle, point);
                _elements.Add(rectangle);
            }
        }

        private void AddStarShape(Point center, double radius, DataPoint point, int seriesIndex, int pointIndex, int sides)
        {
            Point[] starPoints = CreateStarPoints(sides, radius / 2, GetStartAngle(sides));
            AddPolygonShape(center, radius, point, seriesIndex, pointIndex, starPoints);
        }

        static internal double GetStartAngle(int sides)
        {
            if (sides == 5)
            {
                return -Math.PI / 20;
            }
            else if (sides == 6)
            {
                return -Math.PI / 12;
            }
            else if (sides == 7)
            {
                return +Math.PI / 28;
            }

            return 0;
        }

        private void AddTriangleShape(Point center, double radius, DataPoint point, int seriesIndex, int pointIndex)
        {
            radius *= 0.7;
            Point[] trianglePoints = new Point[3];
            trianglePoints[0] = new Point(-radius, radius);
            trianglePoints[1] = new Point(radius, radius);
            trianglePoints[2] = new Point(0, -radius);

            AddPolygonShape(center, radius, point, seriesIndex, pointIndex, trianglePoints);
        }

        private void AddDiamondShape(Point center, double radius, DataPoint point, int seriesIndex, int pointIndex)
        {
            radius *= 0.7;
            Point[] starPoints = CreateStarPoints(6, radius, -Math.PI / 12);
            Point[] diamondPoints = new Point[6];
            diamondPoints[0] = new Point(starPoints[0].X, starPoints[0].Y);
            diamondPoints[1] = new Point(starPoints[2].X, starPoints[2].Y);
            diamondPoints[2] = new Point(starPoints[4].X, starPoints[4].Y);
            diamondPoints[3] = new Point(starPoints[6].X, starPoints[6].Y);
            diamondPoints[4] = new Point(starPoints[8].X, starPoints[8].Y);
            diamondPoints[5] = new Point(starPoints[10].X, starPoints[10].Y);

            AddPolygonShape(center, radius, point, seriesIndex, pointIndex, diamondPoints);
        }

        private void AddPolygonShape(Point center, double radius, DataPoint point, int seriesIndex, int pointIndex, Point[] points)
        {
            // If fast rendering mode is active use GDI+ rendering
            if (Graphics != null)
            {
                for (int index = 0; index < points.Length; index++)
                {
                    points[index].Offset(center.X, center.Y);
                }

                Graphics.SetGdiPen(seriesIndex, pointIndex, point);
                Graphics.SetGdiBrush(seriesIndex, pointIndex, point);
                Graphics.DrawPolygon(points);
                Graphics.AddHitTestRegionPolygon(points, seriesIndex, pointIndex);
            }
            else // Use WPF rendering
            {
                Polygon poly = new Polygon();
                foreach (Point pt in points)
                {
                    pt.Offset(radius, radius);
                    poly.Points.Add(pt);
                }
                Point pt0 = points[0];
                pt0.Offset(radius, radius);
                poly.Points.Add(pt0);
                poly.Width = radius * 2;
                poly.Height = radius * 2;
                Canvas.SetLeft(poly, center.X - radius);
                Canvas.SetTop(poly, center.Y - radius);

                SetMarkerParameters(poly, point, seriesIndex, pointIndex);
                SetHitTest2D(poly, point);
                _elements.Add(poly);
            }
        }

        private void SetMarkerParameters(Shape shape, DataPoint point, int seriesIndx, int pointIndx)
        {
            SetShapeparameters(shape, point, seriesIndx, pointIndx);

            Marker marker = GetMarker(point);

            if (marker != null && marker.Fill != null)
            {
                shape.Fill = marker.Fill;
            }

            if (marker != null && marker.Stroke != null)
            {
                shape.Stroke = marker.Stroke;
            }

            if (marker != null)
            {
                shape.StrokeThickness = marker.StrokeThickness;
            }

            #region Events
            shape.MouseDown += new MouseButtonEventHandler(marker.RaiseEvent);
            shape.MouseEnter += new MouseEventHandler(marker.RaiseEvent);
            shape.MouseLeave += new MouseEventHandler(marker.RaiseEvent);
            shape.MouseLeftButtonDown += new MouseButtonEventHandler(marker.RaiseEvent);
            shape.MouseLeftButtonUp += new MouseButtonEventHandler(marker.RaiseEvent);
            shape.MouseMove += new MouseEventHandler(marker.RaiseEvent);
            shape.MouseRightButtonDown += new MouseButtonEventHandler(marker.RaiseEvent);
            shape.MouseRightButtonUp += new MouseButtonEventHandler(marker.RaiseEvent);
            shape.MouseUp += new MouseButtonEventHandler(marker.RaiseEvent);
            shape.MouseWheel += new MouseWheelEventHandler(marker.RaiseEvent);
            shape.ToolTipOpening += new ToolTipEventHandler(marker.RaiseEvent);
            shape.ToolTipClosing += new ToolTipEventHandler(marker.RaiseEvent);
            #endregion
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