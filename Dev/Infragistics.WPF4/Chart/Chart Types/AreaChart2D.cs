
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class creates 2D area chart. This class is also 
    /// responsible for 2D area chart animation.
    /// </summary>
    internal class AreaChart2D : ChartSeries
    {
        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

        #endregion ChartTypeparameters

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        internal AreaChart2D()
        {
        }

        /// <summary>
        /// Draws data points for different 2D chart types using Shapes. Draws data points for 
        /// all series which have selected chart type. Creates hit test functionality, 
        /// mouse events and tooltips for data points.
        /// </summary>
        protected override void Draw2D()
        {
            double xVal2, xVal1, yVal2, yVal1;

            int seriesNum = SeriesList.Count;

            // Data series loop
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {
                if (SeriesList[seriesIndx].ChartType != ChartType.Area)
                {
                    continue;
                }

                // The number of data points
                int pointNum = SeriesList[seriesIndx].DataPoints.Count;

                // Activate fast algorithm when series have more than 1000 data points.
                bool fastLine = false;
                bool[] fastFlags = new bool[pointNum];
                if (pointNum > 1000 && !AxisX.Logarithmic && !AxisY.Logarithmic && Chart != null && Chart.GetContainer().Scene != null && Chart.GetContainer().Scene.DataFilter)
                {
                    fastFlags = FastLine(SeriesList[seriesIndx], false);
                    fastLine = true;
                }

                // Data points loop
                int firstPointIndex = 0;
                for (int pointIndx = 0; pointIndx < pointNum - 1; pointIndx++)
                {
                    DataPoint point1;
                    DataPoint point2;

                    if (fastLine)
                    {
                        if (fastFlags[pointIndx + 1] || pointIndx == 0)
                        {
                            point1 = SeriesList[seriesIndx].DataPoints[firstPointIndex];
                            point2 = SeriesList[seriesIndx].DataPoints[pointIndx + 1];

                            // Data point X and y position
                            xVal1 = firstPointIndex + 1;
                            xVal2 = pointIndx + 2;
                            yVal1 = point1.Value;
                            yVal2 = point2.Value;

                            firstPointIndex = pointIndx + 1;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        point1 = SeriesList[seriesIndx].DataPoints[pointIndx];
                        point2 = SeriesList[seriesIndx].DataPoints[pointIndx + 1];

                        // Data point X and y position
                        xVal1 = pointIndx + 1;
                        xVal2 = pointIndx + 2;
                        yVal1 = point1.Value;
                        yVal2 = point2.Value;
                    }

                    // Skip point if out of range
                    if (IsXOutOfRange(xVal1) || IsXOutOfRange(xVal2))
                    {
                        continue;
                    }

                    //Skip Null values
                    if (point1.NullValue == true || point2.NullValue == true)
                    {
                        continue;
                    }

                    Point[] points = new Point[4];

                    double x1 = this.AxisX.GetPosition(xVal1) - 0.5;
                    double x2 = this.AxisX.GetPosition(xVal2) + 0.5;
                    points[0].X = x1;
                    points[0].Y = AxisY.GetPositionLogarithmic(yVal1);
                    points[1].X = x2;
                    points[1].Y = AxisY.GetPositionLogarithmic(yVal2);
                    points[2].X = x2;
                    points[2].Y = AxisY.GetAreaZeroValue();
                    points[3].X = x1;
                    points[3].Y = AxisY.GetAreaZeroValue();

                    Point crossingPoint;
                    if (GetCrossingPoint(out crossingPoint, points[0], points[1], AxisY.GetAreaZeroValue()))
                    {
                        // The first segment
                        Point oldPoint1 = points[1];
                        Point oldPoint2 = points[2];
                        points[1] = crossingPoint;
                        points[2] = crossingPoint;
                        DrawPoint(seriesIndx, pointIndx, points, point1);

                        // The second segment
                        points[1] = oldPoint1;
                        points[2] = oldPoint2;
                        points[0] = crossingPoint;
                        points[3] = crossingPoint;
                        DrawPoint(seriesIndx, pointIndx, points, point1);
                    }
                    else
                    {
                        DrawPoint(seriesIndx, pointIndx, points, point1);
                    }
                }
            }
        }

        protected void DrawPoint(int seriesIndx, int pointIndx, Point[] points, DataPoint dataPoint)
        {
            Rect rect = FindBoundingRect(points);

            // Draw or hit test a data point
            AddShape(rect, points, dataPoint, pointIndx, seriesIndx);
        }

        private Rect FindBoundingRect(Point[] points)
        {
            double left = double.MaxValue;
            double right = double.MinValue;
            double top = double.MaxValue;
            double bottom = double.MinValue;
            for (int pointIndx = 0; pointIndx < 4; pointIndx++)
            {
                if (left > points[pointIndx].X)
                {
                    left = points[pointIndx].X;
                }

                if (right < points[pointIndx].X)
                {
                    right = points[pointIndx].X;
                }

                if (top > points[pointIndx].Y)
                {
                    top = points[pointIndx].Y;
                }

                if (bottom < points[pointIndx].Y)
                {
                    bottom = points[pointIndx].Y;
                }
            }

            return new Rect(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// This method draws data points as shapes.
        /// </summary>
        /// <param name="rect">Bounding rectangle of a data point shape.</param>
        /// <param name="points">Area shape of a data point</param>
        /// <param name="point">Data point</param>
        /// <param name="pointIndex">Data point index</param>
        /// <param name="seriesIndex">Series index</param>
        private void AddShape(Rect rect, Point[] points, DataPoint point, int pointIndex, int seriesIndex)
        {
            // If fast rendering mode is active use GDI+ rendering
            if (Graphics != null)
            {
                Graphics.SetGdiPen(seriesIndex, pointIndex, point);
                Graphics.SetGdiBrush(seriesIndex, pointIndex, point);
                Graphics.DrawPolygon(points);
                Graphics.AddHitTestRegionPolygon(points, seriesIndex, pointIndex);
            }
            else if (point.GetSeries().UseDataTemplate)
            {
                ContentControl content = this.AddTemplate(rect, points, point, pointIndex, seriesIndex);
                this.SetHitTest2D(content, point);
            }
            else
            {
                Polygon polygon = new Polygon();

                for (int pointIndx = 0; pointIndx < 4; pointIndx++)
                {
                    points[pointIndx].X -= rect.X;
                    points[pointIndx].Y -= rect.Y;
                }

                Canvas.SetLeft(polygon, rect.Left);
                Canvas.SetTop(polygon, rect.Top);
                polygon.Width = rect.Width;
                polygon.Height = rect.Height;

                polygon.Points = new PointCollection();
                polygon.Points.Add(points[0]);
                polygon.Points.Add(points[1]);
                polygon.Points.Add(points[2]);
                polygon.Points.Add(points[3]);

                SetShapeparameters(polygon, point, seriesIndex, pointIndex);

                DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);
                if (IsAnimationEnabled(animation))
                {
                    ScaleTransform scaleTransform = new ScaleTransform(0, 1);
                    animation.From = 0;
                    animation.To = 1;

                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                    polygon.RenderTransform = scaleTransform;
                }
                SetHitTest2D(polygon, point);
                _elements.Add(polygon);
            }
        }

        /// <summary>
        /// This method draws data points as shapes using data templates.
        /// </summary>
        /// <param name="rect">Column rectangle which represent a data point.</param>
        /// <param name="points">Area shape of a data point</param>
        /// <param name="point">Coresponding data point.</param>
        /// <param name="pointIndex">Current data point index.</param>
        /// <param name="seriesIndex">Current series index.</param>
        protected ContentControl AddTemplate(Rect rect, Point[] points,DataPoint point, int pointIndex, int seriesIndex)
        {
            AreaChartTemplate template = new AreaChartTemplate();
            ContentControl content = new ContentControl();

            point.DataPointTemplate = template;

            content.Content = template;

            point.UpdateActualFill(seriesIndex, pointIndex);
            template.Fill = point.ActualFill;

            point.UpdateActualStroke(seriesIndex, pointIndex);
            template.Stroke = point.ActualStroke;

            template.StrokeThickness = point.GetStrokeThickness(seriesIndex, pointIndex);
            template.ToolTip = point.GetToolTip();

            Polygon polygon = new Polygon();

            for (int pointIndx = 0; pointIndx < 4; pointIndx++)
            {
                points[pointIndx].X -= rect.X;
                points[pointIndx].Y -= rect.Y;
            }

            Canvas.SetLeft(content, rect.Left);
            Canvas.SetTop(content, rect.Top);
            content.Width = rect.Width;
            content.Height = rect.Height;

            template.Point1 = points[0];
            template.Point2 = points[1];
            template.Point3 = points[2];
            template.Point4 = points[3];

            SetContentEvents(content, point);

            DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);
            if (IsAnimationEnabled(animation))
            {
                ScaleTransform scaleTransform = new ScaleTransform(0, 1);
                animation.From = 0;
                animation.To = 1;

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                content.RenderTransform = scaleTransform;
            }

            _elements.Add(content);
            return content;
        }


        internal bool GetCrossingPoint(out Point pointCross, Point point1, Point point2, double crossing)
        {
            pointCross = new Point(0, 0);

            if ((point1.Y > crossing && point2.Y < crossing) || (point1.Y < crossing && point2.Y > crossing))
            {
                double diff = (point1.Y - point2.Y) / (point1.Y - crossing);

                pointCross.X = point1.X + (point2.X - point1.X) / diff;
                pointCross.Y = crossing;
                return true;
            }

            return false;
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