
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using gdiDrawing = System.Drawing;
using gdiImaging = System.Drawing.Imaging;
using WpfImaging = System.Windows.Media.Imaging;
using WpfMedia = System.Windows.Media;
using gdiDrawing2D = System.Drawing.Drawing2D;
using System.Drawing.Drawing2D;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Collections;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class is used for fast 2D rendering. It encapsulates GDI+ Graphics. 
    /// If data points are drawn using WPF rendering engine (FrameworkElements or 
    /// DrawingContext), drawing is very slow, especially for big number of data points. 
    /// So the solution is to use GDI+ drawing for big number of data points. Disadvantage 
    /// of this approach is that some of WPF features will not work (Animation, DataTemplates, etc.) 
    /// This rendering is used only for GridArea and all other chart elements (Axis Labels, Legend, etc) 
    /// are drawn using WPF rendering. 
    /// </summary>
    internal class GdiGraphics
    {
        #region Fields

        private gdiDrawing::Graphics _g;
        private gdiDrawing::Bitmap _bitmap;
        private Chart _chart;
        private gdiDrawing::Pen _pen;
        private gdiDrawing::Brush _brush;
        private Hashtable _brushHash = new Hashtable();
        private Hashtable _penHash = new Hashtable();
        private int _realSeriesIndex;
        internal bool AllowDataPointBrush = false;
        internal bool ForceSolidBrush = true;
        private Brush _gradientBrush;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Sets the rendering quality for this Graphics.
        /// </summary>
        /// <param name="smoothing">True if Anti Alias is on</param>
        internal void SetSmoothingMode(bool smoothing)
        {
            if (smoothing)
            {
                _g.SmoothingMode = SmoothingMode.AntiAlias;
            }
            else
            {
                _g.SmoothingMode = SmoothingMode.Default;
            }
        }

        /// <summary>
        /// Creates GDI+ Graphics object from a bitmap.
        /// </summary>
        /// <param name="width">The bitmap width</param>
        /// <param name="height">The bitmap height</param>
        /// <param name="brush">The background brush</param>
        /// <param name="chart">The Chart</param>
        internal GdiGraphics(double width, double height, Brush brush, Chart chart)
        {
            _chart = chart;
            int bitmapWidth = (int)Math.Floor(width + 1);
            int bitmapHeight = (int)Math.Floor(height + 1);

            // Create a bitmap
            _bitmap = new gdiDrawing::Bitmap(bitmapWidth, bitmapHeight);
            _g = gdiDrawing::Graphics.FromImage(_bitmap);
           
            // Draw Background
            gdiDrawing::Rectangle rect = new gdiDrawing::Rectangle(0, 0, bitmapWidth, bitmapHeight);
            _g.FillRectangle(GetBrush(brush, rect), rect);
        }

        /// <summary>
        /// Creates WPF image brush from GDI+ bitmap which is used for Graphics drawing.
        /// </summary>
        /// <returns>WPF image brush.</returns>
        internal ImageBrush CreateImageBrush()
        {
            MemoryStream stream = new MemoryStream();
            _bitmap.Save(stream, gdiImaging::ImageFormat.Bmp);
            WpfImaging::BitmapImage wpfBitmapImage = new WpfImaging::BitmapImage();
            wpfBitmapImage.BeginInit();
            wpfBitmapImage.StreamSource = stream;
            wpfBitmapImage.EndInit();
            return new ImageBrush(wpfBitmapImage);
        }

        /// <summary>
        /// Because of performance with gdi+ drawing, the Pen per series can be created only once.
        /// </summary>
        /// <param name="srsIndx">Series index</param>
        /// <param name="pointIndex">Data Point index</param>
        /// <param name="point">Data point</param>
        internal void SetGdiPen(int srsIndx, int pointIndex, DataPoint point)
        {
            Series series = point.GetSeries();
            if (_penHash.Contains(series))
            {
                _pen = _penHash[series] as gdiDrawing::Pen;
            }
            else
            {
                int seriesIndex = FindSeriesIndex(series);
                if (seriesIndex == -1)
                {
                    seriesIndex = srsIndx;
                }

                Pen pen = new Pen(point.GetSeriesStroke(seriesIndex), point.GetSeriesStrokeThickness(seriesIndex));
                gdiDrawing::Pen gdiPen = GetPen(pen);
                _penHash.Add(series, gdiPen);
                _pen = gdiPen;
            }
        }

        /// <summary>
        /// Because of performance with gdi+ drawing, the Pen per series can be created only once.
        /// </summary>
        /// <param name="srsIndx">Series index</param>
        /// <param name="pointIndex">Data Point index</param>
        /// <param name="point">Data point</param>
        internal void SetGdiLinePen(int srsIndx, int pointIndex, DataPoint point)
        {
            Series series = point.GetSeries();

            if (_penHash.Contains(series))
            {
                _pen = _penHash[series] as gdiDrawing::Pen;
            }
            else
            {
                int seriesIndex = FindSeriesIndex(series);
                if (seriesIndex == -1)
                {
                    seriesIndex = srsIndx;
                }

                Pen pen = new Pen(point.GetSeriesFill(seriesIndex), point.GetSeriesStrokeThickness(seriesIndex));
                gdiDrawing::Pen gdiPen = GetPen(pen);
                _penHash.Add(series, gdiPen);
                _pen = gdiPen;
            }
        }

        /// <summary>
        /// Because of performance with gdi+ drawing, the Brush per series can be created only once.
        /// </summary>
        /// <param name="srsIndx">Series index</param>
        /// <param name="pointIndex">Data Point index</param>
        /// <param name="point">Data point</param>
        internal void SetGdiBrush(int srsIndx, int pointIndex, DataPoint point)
        {
            Series series = point.GetSeries();

            if (!ForceSolidBrush)
            {
                int seriesIndex = FindSeriesIndex(series);
                point.UpdateActualFill(seriesIndex, pointIndex);
                _gradientBrush = point.ActualFill;
               
               return;
            }

            if (AllowDataPointBrush)
            {
                int seriesIndex = FindSeriesIndex(series);

                point.UpdateActualFill(seriesIndex, pointIndex);
                Brush brush = point.ActualFill;

                gdiDrawing::Brush gdiBrush = GetBrush(brush);
               _brush = gdiBrush;
               return;
            }

            if (_brushHash.Contains(series))
            {
                _brush = _brushHash[series] as gdiDrawing::Brush;
            }
            else
            {
                int seriesIndex = FindSeriesIndex(series);
                if (seriesIndex == -1)
                {
                    seriesIndex = srsIndx;
                }

                Brush brush = point.GetSeriesFill(seriesIndex);
                gdiDrawing::Brush gdiBrush = GetBrush(brush);
                _brushHash.Add(series, gdiBrush);
                _brush = gdiBrush;
            }
        }

        /// <summary>
        /// Find index from series
        /// </summary>
        /// <param name="series">Data Series</param>
        /// <returns>Series Index</returns>
        private int FindSeriesIndex(Series series)
        {
            int index = 0;
            _realSeriesIndex = 0;
            XamChart control = XamChart.GetControl(series);
            if (control == null)
            {
                return 0;
            }

            foreach (Series ser in control.Series)
            {
                if (ser == series)
                {
                    _realSeriesIndex = index;
                    return index;
                }
                index++;
            }

            return -1;
        }

        /// <summary>
        /// Draws a line connecting two Point structures.
        /// </summary>
        /// <param name="pen">Pen that determines the color, width, and style of the line.</param>
        /// <param name="point1">Point structure that represents the first point to connect</param>
        /// <param name="point2">Point structure that represents the second point to connect.</param>
        internal void DrawLine(Pen pen, Point point1, Point point2)
        {
            _g.DrawLine(GetPen(pen), (float)point1.X, (float)point1.Y, (float)point2.X, (float)point2.Y);
        }

        /// <summary>
        /// Draws a line connecting the two points specified by the coordinate pairs.
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        internal void DrawLine(double x1, double y1, double x2, double y2)
        {
            _g.DrawLine(_pen, (float)x1, (float)y1, (float)x2, (float)y2);
        }

        /// <summary>
        /// Draws a line connecting two Point structures.
        /// </summary>
        /// <param name="point1">Point structure that represents the first point to connect</param>
        /// <param name="point2">Point structure that represents the second point to connect.</param>
        internal void DrawLine(Point point1, Point point2)
        {
            _g.DrawLine(_pen, (float)point1.X, (float)point1.Y, (float)point2.X, (float)point2.Y);
        }

        /// <summary>
        /// Draws a rectangle with the specified Brush and Pen.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width">Width of the rectangle to draw.</param>
        /// <param name="height">Height of the rectangle to draw.</param>
        internal void DrawRectangle(double x, double y, double width, double height)
        {
            if (!ForceSolidBrush)
            {
                gdiDrawing::RectangleF gdiRect = new gdiDrawing::RectangleF((float)x, (float)y, (float)width, (float)height);
                _brush = GetBrush(_gradientBrush, gdiRect);
            }

            _g.FillRectangle(_brush, (float)x, (float)y, (float)width, (float)height);
            _g.DrawRectangle(_pen, (float)x, (float)y, (float)width, (float)height);

            if (!ForceSolidBrush)
            {
                _brush.Dispose();
            }
        }

        /// <summary>
        /// Draws an ellipse with the specified Brush and Pen.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle to draw.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle to draw.</param>
        /// <param name="width">Width of the bounding rectangle to draw.</param>
        /// <param name="height">Height of the bounding rectangle to draw.</param>
        internal void DrawEllipse(double x, double y, double width, double height)
        {
            if (!ForceSolidBrush)
            {
                gdiDrawing::Rectangle gdiRect = new gdiDrawing::Rectangle((int)x, (int)y, (int)width, (int)height);
                _brush = GetBrush(_gradientBrush, gdiRect);
            }

            _g.FillEllipse(_brush, (float)x, (float)y, (float)width, (float)height);
            _g.DrawEllipse(_pen, (float)x, (float)y, (float)width, (float)height);

            if (!ForceSolidBrush)
            {
                _brush.Dispose();
            }
        }

        /// <summary>
        /// Draws and fill a polygon defined by an array of Point structures.
        /// </summary>
        /// <param name="points">Array of Point structures that represent the vertices of the polygon.</param>
        internal void DrawPolygon(Point[] points)
        {
            if (!ForceSolidBrush)
            {
                gdiDrawing::RectangleF gdiRect = GetBoundingRect(points);
                _brush = GetBrush(_gradientBrush, gdiRect);
            }

            _g.FillPolygon(_brush, GetPoints(points));
            _g.DrawPolygon(_pen, GetPoints(points));

            if (!ForceSolidBrush)
            {
                _brush.Dispose();
            }
        }

        /// <summary>
        /// Gets the bounding rectangle from polygon points.
        /// </summary>
        /// <param name="points">Polygon points</param>
        /// <returns>Bounding rectangle</returns>
        private gdiDrawing::RectangleF GetBoundingRect(Point[] points)
        {
            gdiDrawing::RectangleF rect = new gdiDrawing::RectangleF();
            gdiDrawing.PointF[] pointsF = GetPoints(points);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (gdiDrawing::PointF point in pointsF)
            {
                if (minX > point.X)
                {
                    minX = point.X;
                }

                if (maxX < point.X)
                {
                    maxX = point.X;
                }

                if (minY > point.Y)
                {
                    minY = point.Y;
                }

                if (maxY < point.Y)
                {
                    maxY = point.Y;
                }
            }

            rect.X = minX;
            rect.Y = minY;
            rect.Width = maxX - minX;
            rect.Height = maxY - minY;
            return rect;
        }

        /// <summary>
        /// Draws a rectangle with the specified Brush and Pen.
        /// </summary>
        /// <param name="brush">The brush with which to fill the rectangle.</param>
        /// <param name="pen">The pen with which to stroke the rectangle.</param>
        /// <param name="rect">The rectangle to draw.</param>
        internal void DrawRectangle(Brush brush, Pen pen, Rect rect)
        {
            gdiDrawing::Rectangle gdiRect = new gdiDrawing::Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            _g.FillRectangle(GetBrush(brush, gdiRect), (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
            _g.DrawRectangle(GetPen(pen), (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
        }

        /// <summary>
        /// Converts WPF Point structures to System.Drawing PointF structures.
        /// </summary>
        /// <param name="points">An array of WPF Point structures</param>
        /// <returns>An array of System.Drawing PointF structures</returns>
        private gdiDrawing::PointF[] GetPoints(Point[] points)
        {
            gdiDrawing::PointF[] pointsF = new gdiDrawing::PointF[points.Length];

            int index = 0;
            foreach (Point point in points)
            {
                pointsF[index].X = (float)points[index].X;
                pointsF[index].Y = (float)points[index].Y;
                index++;
            }

            return pointsF;
        }

        /// <summary>
        /// Creates a graphics path as a hit test region from polygon.
        /// </summary>
        /// <param name="points">Array of Point structures that represent the vertices of the polygon.</param>
        /// <param name="seriesIndex">Data series index</param>
        /// <param name="dataPointIndex">Data point index</param>
        internal void AddHitTestRegionPolygon(Point[] points, int seriesIndex, int dataPointIndex)
        {
            double size = 5;
            float sizeF = (float)size;
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(GetPoints(points));

            HitTestInfo hitTestInfo = new HitTestInfo();
            hitTestInfo.Path = path;
            hitTestInfo.SeriesIndex = _realSeriesIndex;
            hitTestInfo.DataPointIndex = dataPointIndex;

            _chart.HitTestInfoArray.Add(hitTestInfo);
        }

        /// <summary>
        /// Creates a graphics path as a hit test region from rectangle.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width">Width of the rectangle to draw.</param>
        /// <param name="height">Height of the rectangle to draw.</param>
        /// <param name="seriesIndex">Data series index</param>
        /// <param name="dataPointIndex">Data point index</param>
        internal void AddHitTestRegionRectangle(double x, double y, double width, double height, int seriesIndex, int dataPointIndex)
        {
            // Minimum width and height size for hit test
            if (width < 2)
            {
                width = 2;
                x = x - 1;
            }

            if (height < 2)
            {
                height = 2;
                y = y - 1;
            }

            GraphicsPath path = new GraphicsPath();
            gdiDrawing::RectangleF rect = new gdiDrawing::RectangleF((float)x, (float)y, (float)width, (float)height);
            path.AddRectangle(rect);

            HitTestInfo hitTestInfo = new HitTestInfo();
            hitTestInfo.Path = path;
            hitTestInfo.SeriesIndex = _realSeriesIndex;
            hitTestInfo.DataPointIndex = dataPointIndex;

            _chart.HitTestInfoArray.Add(hitTestInfo);
        }

        /// <summary>
        /// Creates a graphics path as a hit test region from ellipse.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
        /// <param name="seriesIndex">Data series index</param>
        /// <param name="dataPointIndex">Data point index</param>
        internal void AddHitTestRegionEllipse(double x, double y, double width, double height, int seriesIndex, int dataPointIndex)
        {
            // Minimum width and height size for hit test
            if (width < 2)
            {
                width = 2;
                x = x - 1;
            }

            if (height < 2)
            {
                height = 2;
                y = y - 1;
            }

            double size = 5;
            float sizeF = (float)size;
            GraphicsPath path = new GraphicsPath();
            gdiDrawing::RectangleF rect = new gdiDrawing::RectangleF((float)x, (float)y, (float)width, (float)height);
            path.AddEllipse(rect);

            HitTestInfo hitTestInfo = new HitTestInfo();
            hitTestInfo.Path = path;
            hitTestInfo.SeriesIndex = _realSeriesIndex;
            hitTestInfo.DataPointIndex = dataPointIndex;

            _chart.HitTestInfoArray.Add(hitTestInfo);
        }

        /// <summary>
        /// Creates a graphics path as a hit test region from line.
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        /// <param name="seriesIndex">Data series index</param>
        /// <param name="dataPointIndex">Data point index</param>
        internal void AddHitTestRegionLine(double x1, double y1, double x2, double y2, int seriesIndex, int dataPointIndex)
        {
            double size = 5;
            float sizeF = (float)size;
            GraphicsPath path = new GraphicsPath();

            // Vertical line
            if (Math.Abs(x2 - x1) < 2)
            {
                path.AddLine((float)x1 - sizeF, (float)y1, (float)x1 - sizeF, (float)y2);
                path.AddLine((float)x1 - sizeF, (float)y2, (float)x2 + sizeF, (float)y2);
                path.AddLine((float)x2 + sizeF, (float)y2, (float)x2 + sizeF, (float)y1);
            }
            else
            {
                path.AddLine((float)x1, (float)y1 - sizeF, (float)x1, (float)y1 + sizeF);
                path.AddLine((float)x1, (float)y1 + sizeF, (float)x2, (float)y2 + sizeF);
                path.AddLine((float)x2, (float)y2 + sizeF, (float)x2, (float)y2 - sizeF);
            }
            path.CloseFigure();

            HitTestInfo hitTestInfo = new HitTestInfo();
            hitTestInfo.Path = path;
            hitTestInfo.SeriesIndex = _realSeriesIndex;
            hitTestInfo.DataPointIndex = dataPointIndex;

            _chart.HitTestInfoArray.Add(hitTestInfo);
        }

        /// <summary>
        /// Converts WPF Pen to GDI+ Pen.
        /// </summary>
        /// <param name="pen">WPF Pen</param>
        /// <returns>GDI+ Pen</returns>
        private gdiDrawing::Pen GetPen(Pen pen)
        {
            gdiDrawing::Brush brush = GetBrush(pen.Brush);
            if (brush != null && pen != null)
            {
                return new gdiDrawing::Pen(brush, (float)pen.Thickness);
            }

            return new gdiDrawing::Pen(gdiDrawing::Color.Black, 1);
        }

        /// <summary>
        /// Converts WPF Brush to GDI+ Brush.
        /// </summary>
        /// <param name="brush">WPF Brush</param>
        /// <returns>GDI+ Brush</returns>
        private gdiDrawing::Brush GetBrush(Brush brush)
        {
            SolidColorBrush solidBrush = brush as SolidColorBrush;
            if (solidBrush != null)
            {
                return new gdiDrawing::SolidBrush(GetColor(solidBrush.Color));
            }

            return new gdiDrawing::SolidBrush(gdiDrawing::Color.White);
        }

      
        /// <summary>
        /// Converts WPF Brush to GDI+ Brush.
        /// </summary>
        /// <param name="brush">WPF Brush</param>
        /// <param name="rect">Rectangle of the area which should be filled with brush</param>
        /// <returns>GDI+ Brush</returns>
        private gdiDrawing::Brush GetBrush(Brush brush, gdiDrawing::RectangleF rect)
        {
            if (brush is SolidColorBrush)
            {
                SolidColorBrush solidBrush = brush as SolidColorBrush;
                if (solidBrush != null)
                {
                    return new gdiDrawing::SolidBrush(GetColor(solidBrush.Color));
                }
            }
            else if (brush is WpfMedia::LinearGradientBrush)
            {
                if (ForceSolidBrush)
                {
                    WpfMedia::LinearGradientBrush linearBrush = brush as WpfMedia::LinearGradientBrush;
                    return new gdiDrawing::SolidBrush(GetColor(linearBrush.GradientStops[0].Color));
                }
                else
                {
                    WpfMedia::LinearGradientBrush linearBrush = brush as WpfMedia::LinearGradientBrush;
                    if (linearBrush != null && linearBrush.GradientStops.Count >= 2)
                    {
                        gdiDrawing2D::LinearGradientBrush gdiLinearBrush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, GetColor(linearBrush.GradientStops[0].Color), GetColor(linearBrush.GradientStops[1].Color), GetGradientAngle(linearBrush));
                        ColorBlend colorBlend = new ColorBlend(linearBrush.GradientStops.Count);
                        int index = 0;
                        foreach (GradientStop stop in linearBrush.GradientStops)
                        {
                            colorBlend.Colors[index] = GetColor(stop.Color);
                            colorBlend.Positions[index] = (float)stop.Offset;
                            index++;
                        }

                        SortColorBlend(colorBlend);
                        gdiLinearBrush.InterpolationColors = colorBlend;
                        return gdiLinearBrush;
                    }
                }
            }
            else
            {
                // This brush type is not supported with this rendering mode. Please, use Full rendering mode.
                //throw new InvalidOperationException(ErrorString.Exc67);
            }

            return new gdiDrawing::SolidBrush(gdiDrawing::Color.White);
        }

        /// <summary>
        /// Calculate angle for linear brush from start and end points.
        /// </summary>
        /// <param name="brush">WPF brush</param>
        /// <returns>Angle</returns>
        private float GetGradientAngle(WpfMedia::LinearGradientBrush brush)
        {
            Point startPoint = brush.StartPoint;
            Point endPoint = brush.EndPoint;

            float angle = (float)(Math.Atan((endPoint.Y - startPoint.Y) / (endPoint.X - startPoint.X)) / Math.PI * 180.0);

            if (endPoint.X - startPoint.X < 0)
            {
                angle += 180;
            }

            return angle;
        }

        /// <summary>
        /// Sort positions because of error in Interpolation colors and non compatible brushes between GDI+ and wpf.
        /// </summary>
        /// <param name="blend">Color Blend</param>
        private void SortColorBlend(ColorBlend blend)
        {
            float iPos, jPos,tempPos;
            gdiDrawing::Color tempColor;
            for (int i = 0; i < blend.Positions.Length; i++)
            {
                iPos = blend.Positions[i];
                for (int j = 0; j < blend.Positions.Length; j++)
                {
                    jPos = blend.Positions[j];
                    if (iPos < jPos)
                    {
                        tempPos = blend.Positions[j];
                        blend.Positions[j] = blend.Positions[i];
                        blend.Positions[i] = tempPos;

                        tempColor = blend.Colors[j];
                        blend.Colors[j] = blend.Colors[i];
                        blend.Colors[i] = tempColor;
                    }
                }
            }
        }


        /// <summary>
        /// Returns gdi point from wpf point
        /// </summary>
        /// <param name="point">wpf point</param>
        /// <returns>gdi point</returns>
        private System.Drawing.Point GetPoint(System.Windows.Point point)
        {
            System.Drawing.Point gdiPoint = new System.Drawing.Point();
            gdiPoint.X = (int)point.X;
            gdiPoint.Y = (int)point.Y;

            return gdiPoint;
        }

        /// <summary>
        /// Converts WPF Color to GDI+ Color.
        /// </summary>
        /// <param name="color">WPF Color</param>
        /// <returns>GDI+ Color</returns>
        private gdiDrawing::Color GetColor(Color color)
        {
            return gdiDrawing::Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Releases all resources used by this GdiGraphics.
        /// </summary>
        internal void Dispose()
        {
            _g.Dispose();
            _bitmap.Dispose();
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