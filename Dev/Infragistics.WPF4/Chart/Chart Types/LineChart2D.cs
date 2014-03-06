
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
    /// This class creates 2D line chart. This class is also 
    /// responsible for 2D line chart animation.
    /// </summary>
    internal class LineChart2D : ChartSeries
    {
        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        override internal bool IsColorFromPoint { get { return false; } }

        /// <summary>
        /// Gets a Boolean value that specifies whether this chart type uses only stroke brush 
        /// and doesn�t use fill brush.
        /// </summary>
        override internal bool IsStrokeMainColor { get { return true; } }

        #endregion ChartTypeparameters

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        internal LineChart2D()
        {
        }

        /// <summary>
        /// Draws data points for different 2D chart types using Shapes. Draws data points for 
        /// all series which have selected chart type. Creates hit test functionality, 
        /// mouse events and tooltips for data points.
        /// </summary>
        protected override void Draw2D()
        {
            int seriesNum = SeriesList.Count;

            double xVal1;
            double yVal1;
            double xVal2;
            double yVal2;

            // Data series loop
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {
                if (SeriesList[seriesIndx].ChartType != ChartType.Line)
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



                    Point[] points = new Point[2];

                    points[0].X = AxisX.GetPosition(xVal1);
                    points[0].Y = AxisY.GetPositionLogarithmic(yVal1);
                    points[1].X = AxisX.GetPosition(xVal2);
                    points[1].Y = AxisY.GetPositionLogarithmic(yVal2);

                    // Skip pointa if out of range
                    if (IsXOutOfRange(xVal1) || IsXOutOfRange(xVal2))
                    {
                        continue;
                    }

                    //Skip Null values
                    if (point1.NullValue == true || point2.NullValue == true)
                    {
                        continue;
                    }

                    AddShape(points, point1, seriesIndx, pointIndx, 0, 0, 0);
                }
            }
        }

        /// <summary>
        /// This method draws data points as shapes.
        /// </summary>
        /// <param name="points">Shape points.</param>
        /// <param name="point">Data points.</param>
        /// <param name="seriesIndex">Index of the current series.</param>
        /// <param name="pointIndex">A data point index.</param>
        /// <param name="splineMax">Spline length in X axis units.</param>
        /// <param name="splineStart">X value where spline starts</param>
        /// <param name="splineEnd">X value where spline ends</param>
        protected void AddShape(Point[] points, DataPoint point, int seriesIndex, int pointIndex, double splineMax, double splineStart, double splineEnd)
        {
            // If fast rendering mode is active use GDI+ rendering
            if (Graphics != null)
            {
                Graphics.SetGdiLinePen(seriesIndex, pointIndex, point);
                Graphics.DrawLine(points[0].X, points[0].Y, points[1].X, points[1].Y);
                Graphics.AddHitTestRegionLine(points[0].X, points[0].Y, points[1].X, points[1].Y, seriesIndex, pointIndex);
            }
            else // Use WPF rendering
            {
                DoubleAnimation animationX;
                DoubleAnimation animationY;

                // Spline chart
                if (splineMax != 0)
                {
                    animationX = point.GetDoubleAnimationSpline(pointIndex, splineMax, splineStart, splineEnd);
                    animationY = point.GetDoubleAnimationSpline(pointIndex, splineMax, splineStart, splineEnd);
                }
                else // Non spline chart
                {
                    animationX = point.GetDoubleAnimation(pointIndex);
                    animationY = point.GetDoubleAnimation(pointIndex);
                }

                Line line = new Line();
                line.X1 = points[0].X;
                line.Y1 = points[0].Y;
                line.X2 = points[1].X;
                line.Y2 = points[1].Y;

                SetShapeparameters(line, point, seriesIndex, pointIndex);

                if (IsAnimationEnabled(animationX))
                {
                    line.X2 = points[0].X;
                    line.Y2 = points[0].Y;
                    animationX.From = points[0].X;
                    animationX.To = points[1].X;

                    animationY.From = points[0].Y;
                    animationY.To = points[1].Y;

                    line.BeginAnimation(Line.X2Property, animationX);
                    line.BeginAnimation(Line.Y2Property, animationY);
                }

                SetHitTest2D(line, point);
                _elements.Add(line);
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
        protected void AddTemplate(Rect rect, Point[] points, DataPoint point, int pointIndex, int seriesIndex)
        {
            LineChartTemplate template = new LineChartTemplate();
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

            Canvas.SetLeft(content, rect.Left);
            Canvas.SetTop(content, rect.Top);

            for (int pointIndx = 0; pointIndx < 2; pointIndx++)
            {
                points[pointIndx].X -= rect.X;
                points[pointIndx].Y -= rect.Y;
            }

            content.Width = rect.Width;
            content.Height = rect.Height;

            template.Point1 = points[0];
            template.Point2 = points[1];

            template.RegionPoint1 = new Point(points[0].X, points[0].Y + template.StrokeThickness);
            template.RegionPoint2 = new Point(points[1].X, points[1].Y + template.StrokeThickness);
            template.RegionPoint3 = new Point(points[1].X, points[1].Y - template.StrokeThickness);
            template.RegionPoint4 = new Point(points[0].X, points[0].Y - template.StrokeThickness);

            DoubleAnimation animation = point.GetDoubleAnimation(pointIndex);

            SetContentEvents(content, point);

            if (IsAnimationEnabled(animation))
            {
                content.Width = 0;
                animation.From = 0;
                animation.To = rect.Width;

                content.BeginAnimation(ContentControl.WidthProperty, animation);
            }

            _elements.Add(content);
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