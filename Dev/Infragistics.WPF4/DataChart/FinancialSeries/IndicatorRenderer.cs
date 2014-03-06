using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    internal class IndicatorRenderer
    {
        private static PathFigure output(List<int> segments, Func<int, double> x0, Func<int, double> y0, double resolution)
        {
            PathFigure pathFigure = new PathFigure();

            IList<int> flattenedIndices = Flattener.Flatten(new List<int>(), segments, x0, y0, 0, segments.Count - 1, resolution);
            for (int j = 0; j < flattenedIndices.Count; j++)
            {
                int k = flattenedIndices[j];
                pathFigure.Segments.Add(new LineSegment() { Point = new Point(x0(k), y0(k)) });
            }

            pathFigure.StartPoint = (pathFigure.Segments[0] as LineSegment).Point;

            return pathFigure;
        }

        /// <summary>
        /// Renders into a potentially chunky polyline
        /// </summary>
        /// <param name="count">Number of points</param>
        /// <param name="x0">Bottom points x coordinate indexed left to right</param>
        /// <param name="y0">Bottom points y coordinate indexed left to right</param>
        /// <param name="x1">Top points x coordinate indexed left to right</param>
        /// <param name="y1">Top points y coordinate indexed left to right</param>
        /// <param name="colorByGradient">Whether to color by gradient.</param>
        /// <param name="windowRect">Chart's window</param>
        /// <param name="viewportRect">Chart's viewport</param>
        /// <param name="positivePath0">First positive path</param>
        /// <param name="positivePath01">Second positive path</param>
        /// <param name="positivePath1">Third positive path</param>
        /// <param name="negativePath0">First negative path</param>
        /// <param name="negativePath01">Second negative path</param>
        /// <param name="negativePath1">Third negative path</param>
        /// <param name="bucketSize">Bucket size</param>
        /// <param name="resolution">Resolution</param>
        public static void RasterizeLine(int count,
                                        Func<int, double> x0, Func<int, double> y0,
                                        Func<int, double> x1, Func<int, double> y1,
                                        bool colorByGradient,
                                        Rect windowRect,
                                        Rect viewportRect,
                                        Path positivePath0,
                                        Path positivePath01,
                                        Path positivePath1,
                                        Path negativePath0,
                                        Path negativePath01,
                                        Path negativePath1,
                                        double bucketSize,
                                        double resolution)
        {
            PathFigureCollection positiveFigures0 = (positivePath0.Data as PathGeometry).Figures;

            PathFigureCollection negativeFigures0 = (negativePath0.Data as PathGeometry).Figures;

            if (bucketSize == 1)
            {
                List<int> currentSegment = new List<int>();
                PathFigureCollection currentFigures0 = positiveFigures0;
                int currentType = 0;

                currentSegment.Add(0);

                for (int i = 0, j = 1; j < count; i = j++)
                {
                    int type = currentType;
                    double valueDelegateResult = y0(j) - y0(i);
                    if (colorByGradient && !double.IsNaN(valueDelegateResult))
                    {
                        type = Math.Sign(valueDelegateResult);
                    }

                    if (type != 0 && type != currentType)
                    {
                        currentFigures0.Add(output(currentSegment, x0, y0, resolution));

                        currentType = type;
                        currentFigures0 = currentType == 1 ? negativeFigures0 : positiveFigures0;
                        currentSegment.Clear();
                        currentSegment.Add(i);
                    }

                    currentSegment.Add(j);
                }

                currentFigures0.Add(output(currentSegment, x0, y0, resolution));
            }
            else
            {
                List<int> currentSegment = new List<int>();
                PathFigureCollection currentFigures0 = positiveFigures0;
                int currentType = 0;

                currentSegment.Add(0);

                for (int i = 0, j = 1; j < count; i = j++)
                {
                    int type = currentType;
                    double valueDelegateResult = y0(j) - y0(i);
                    if (colorByGradient && !double.IsNaN(valueDelegateResult))
                    {
                        type = Math.Sign(valueDelegateResult);
                    }

                    if (type != 0 && type != currentType)
                    {
                        if (currentSegment.Count > 0)
                        {
                            currentFigures0.Add(output(currentSegment, x0, y0, resolution));
                        }

                        currentType = type;
                        currentFigures0 = currentType == 1 ? negativeFigures0 : positiveFigures0;
                        currentSegment.Clear();
                        currentSegment.Add(i);
                    }

                    currentSegment.Add(j);
                }

                if (currentSegment.Count > 0)
                {
                    currentFigures0.Add(output(currentSegment, x0, y0, resolution));
                }

            }
        }

        /// <summary>
        /// Renders a polygon
        /// </summary>
        /// <param name="count">Number of points</param>
        /// <param name="x0">Bottom points x coordinate indexed left to right</param>
        /// <param name="y0">Bottom points y coordinate indexed left to right</param>
        /// <param name="x1">Top points x coordinate indexed left to right</param>
        /// <param name="y1">Top points y coordinate indexed left to right</param>
        /// <param name="colorByGradient">Whether to color by gradient.</param>
        /// <param name="windowRect">Chart's window</param>
        /// <param name="viewportRect">Chart's viewport</param>
        /// <param name="positivePath0">First positive path</param>
        /// <param name="positivePath01">Second positive path</param>
        /// <param name="positivePath1">Third positive path</param>
        /// <param name="negativePath0">First negative path</param>
        /// <param name="negativePath01">Second negative path</param>
        /// <param name="negativePath1">Third negative path</param>
        /// <param name="worldZero">Zero line</param>
        /// <param name="bucketSize">Bucket size</param>
        /// <param name="resolution">Resolution</param>
        public static void RasterizeArea(int count,
                                    Func<int, double> x0, Func<int, double> y0,
                                    Func<int, double> x1, Func<int, double> y1,
                                    bool colorByGradient,
                                    Rect windowRect,
                                    Rect viewportRect,
                                    Path positivePath0,
                                    Path positivePath01,
                                    Path positivePath1,
                                    Path negativePath0,
                                    Path negativePath01,
                                    Path negativePath1,
                                    double worldZero,
                                    double bucketSize,
                                    double resolution)
        {
            PathFigureCollection positiveFigures0 = (positivePath0.Data as PathGeometry).Figures;
            PathFigureCollection positiveFigures01 = (positivePath01.Data as PathGeometry).Figures;

            PathFigureCollection negativeFigures0 = (negativePath0.Data as PathGeometry).Figures;
            PathFigureCollection negativeFigures01 = (negativePath01.Data as PathGeometry).Figures;

            //double zero = 0.0;

            //if (YAxis != null)
            //{
            //    zero = GetWorldZeroValue();
            //}
            //else
            //{
            //    zero = 0.5 * (viewportRect.Top + viewportRect.Bottom);
            //}

            if (bucketSize == 1)
            {
                List<int> currentSegment = new List<int>();
                PathFigureCollection currentFigures0 = positiveFigures0;
                PathFigureCollection currentFigures01 = positiveFigures01;
                int currentType = 0;

                currentSegment.Add(0);

                for (int i = 0, j = 1; j < count; i = j++)
                {
                    int type = currentType;
                    double valueDelegateResult = y0(j) - y0(i);
                    if (colorByGradient && !double.IsNaN(valueDelegateResult))
                    {
                        type = Math.Sign(valueDelegateResult);
                    }

                    if (type != 0 && type != currentType)
                    {
                        if (currentSegment.Count > 0)
                        {
                            PathFigure figure0 = output(currentSegment, x0, y0, resolution);
                            PathFigure figure01 = figure0.Duplicate();

                            figure01.Segments.Add(new LineSegment() { Point = new Point((figure0.Segments[figure0.Segments.Count - 1] as LineSegment).Point.X, worldZero) });
                            figure01.Segments.Add(new LineSegment() { Point = new Point((figure0.Segments[0] as LineSegment).Point.X, worldZero) });

                            currentFigures0.Add(figure0);
                            currentFigures01.Add(figure01);
                        }

                        currentType = type;
                        currentFigures0 = currentType == 1 ? negativeFigures0 : positiveFigures0;
                        currentFigures01 = currentType == 1 ? negativeFigures01 : positiveFigures01;
                        currentSegment.Clear();
                        currentSegment.Add(i);
                    }

                    currentSegment.Add(j);
                }

                {
                    PathFigure figure0 = output(currentSegment, x0, y0, resolution);
                    PathFigure figure01 = figure0.Duplicate();

                    figure01.Segments.Add(new LineSegment() { Point = new Point((figure0.Segments[figure0.Segments.Count - 1] as LineSegment).Point.X, worldZero) });
                    figure01.Segments.Add(new LineSegment() { Point = new Point((figure0.Segments[0] as LineSegment).Point.X, worldZero) });

                    currentFigures0.Add(figure0);
                    currentFigures01.Add(figure01);
                }
            }
            else
            {
                List<int> currentSegment = new List<int>();
                PathFigureCollection currentFigures0 = positiveFigures0;
                PathFigureCollection currentFigures01 = positiveFigures01;
                int currentType = 0;

                currentSegment.Add(0);

                for (int i = 0, j = 1; j < count; i = j++)
                {
                    int type = currentType;
                    double valueDelegateResult = (y0(j) + y1(j)) - (y0(i) + y1(i));
                    if (colorByGradient && !double.IsNaN(valueDelegateResult))
                    {
                        type = Math.Sign(valueDelegateResult);
                    }

                    if (type != 0 && type != currentType)
                    {
                        if (currentSegment.Count > 0)
                        {
                            PathFigure figure0 = output(currentSegment, x0, y0, resolution);
                            PathFigure figure01 = figure0.Duplicate();

                            figure01.Segments.Add(new LineSegment() { Point = new Point((figure0.Segments[figure0.Segments.Count - 1] as LineSegment).Point.X, worldZero) });
                            figure01.Segments.Add(new LineSegment() { Point = new Point((figure0.Segments[0] as LineSegment).Point.X, worldZero) });

                            currentFigures0.Add(figure0);
                            currentFigures01.Add(figure01);
                        }

                        currentType = type;
                        currentFigures0 = currentType == 1 ? negativeFigures0 : positiveFigures0;
                        currentFigures01 = currentType == 1 ? negativeFigures01 : positiveFigures01;
                        currentSegment.Clear();
                        currentSegment.Add(i);
                    }

                    currentSegment.Add(j);
                }

                {
                    PathFigure figure0 = output(currentSegment, x0, y0, resolution);
                    PathFigure figure01 = figure0.Duplicate();

                    figure01.Segments.Add(new LineSegment() { Point = new Point((figure0.Segments[figure0.Segments.Count - 1] as LineSegment).Point.X, worldZero) });
                    figure01.Segments.Add(new LineSegment() { Point = new Point((figure0.Segments[0] as LineSegment).Point.X, worldZero) });

                    currentFigures0.Add(figure0);
                    currentFigures01.Add(figure01);
                }
            }

        }
        ///// <summary>
        ///// Renders into a anchored polygon with a potentially chunky top edge
        ///// </summary>
        ///// <param name="polygon0">Bottom polygon</param>
        ///// <param name="polyline0">Bottom polyline</param>
        ///// <param name="polygon01">Top polygon</param>
        ///// <param name="polyline0">Top polyline</param>
        ///// <param name="Count">Number of points</param>
        ///// <param name="x0">Bottom points x coordinate indexed left to right</param>
        ///// <param name="y0">Bottom points y coordinate indexed left to right</param>
        ///// <param name="x1">Top points x coordinate indexed right to left</param>
        ///// <param name="y1">Top points y coordinate indexed right to left</param>
        //protected void RasterizePolygon(Polygon polygon0, Polyline polyline0, Polygon polygon01, Polyline polyline1,
        //                                int count,
        //                                Func<int, double> x0, Func<int, double> y0,
        //                                Func<int, double> x1, Func<int, double> y1,
        //                                bool colorByGradient)
        //{
        //    polygon0.Points.Clear();
        //    polyline0.Points.Clear();
        //    polygon01.Points.Clear();
        //    polyline1.Points.Clear();

        //    if (BucketSize == 1)
        //    {
        //        foreach (int i in Flattener.Flatten(count, x0, y0, Resolution))
        //        {
        //            polygon0.Points.Add(new Point(x0(i), y0(i)));
        //            polyline1.Points.Add(new Point(x0(i), y0(i)));
        //        }
        //    }
        //    else
        //    {
        //        foreach (int i in Flattener.Flatten(count, x0, y0, Resolution))
        //        {
        //            polygon0.Points.Add(new Point(x0(i), y0(i)));
        //            polyline0.Points.Add(new Point(x0(i), y0(i)));
        //            polygon01.Points.Add(new Point(x0(i), y0(i)));
        //        }

        //        foreach (int i in Flattener.Flatten(count, x1, y1, Resolution))
        //        {
        //            polyline1.Points.Add(new Point(x1(i), y1(i)));
        //            polygon01.Points.Add(new Point(x1(i), y1(i)));
        //        }
        //    }

        //    if (polygon0.Points.Count > 0)
        //    {
        //        Rect windowRect = ChartArea != null ? ChartArea.WindowRect : Rect.Empty;
        //        Rect viewportRect = ViewportRect;
        //        double zero = YAxis.GetScaledValue(YAxis.ReferenceValue, windowRect, viewportRect); // convert to world coordinates

        //        polygon0.Points.Add(new Point(x0(count - 1), zero));
        //        polygon0.Points.Add(new Point(x0(0), zero));
        //    }

        //    polygon0.IsHitTestVisible = polygon0.Points.Count > 0;
        //    polyline0.IsHitTestVisible = polyline0.Points.Count > 0;
        //    polygon01.IsHitTestVisible = polygon01.Points.Count > 0;
        //    polyline1.IsHitTestVisible = polyline1.Points.Count > 0;
        //}

        /// <summary>
        /// Generated the visuals for a set of columns based on the data.
        /// </summary>
        /// <param name="count">Number of points</param>
        /// <param name="x0">Bottom points x coordinate indexed left to right</param>
        /// <param name="y0">Bottom points y coordinate indexed left to right</param>
        /// <param name="x1">Top points x coordinate indexed left to right</param>
        /// <param name="y1">Top points y coordinate indexed left to right</param>
        /// <param name="colorByGradient">Whether to color by gradient</param>
        /// <param name="worldZero">Zero line</param>
        /// <param name="Columns">Column pool</param>
        /// <param name="positiveColumns">Positive columns path</param>
        /// <param name="negativeColumns">Negative columns path</param>
        public static void RasterizeColumns(int count,
                                        Func<int, double> x0, Func<int, double> y0,
                                        Func<int, double> x1, Func<int, double> y1,
                                        bool colorByGradient,
                                        double worldZero,
                                        Pool<LineGeometry> Columns,
                                        Path positiveColumns,
                                        Path negativeColumns)
        {
            GeometryGroup positiveGeometryGroup = positiveColumns.Data as GeometryGroup;
            GeometryGroup negativeGeometryGroup = negativeColumns.Data as GeometryGroup;

            //if (BucketSize == 1)
            //{
            //double zero = GetWorldZeroValue();

            for (int i = 0; i < count; ++i)
            {
                LineGeometry column = Columns[i];

                column.StartPoint = new Point(x0(i), worldZero);
                column.EndPoint = new Point(x0(i), y0(i));

                bool pos = false;
                if (i > 0)
                {
                    if (y0(i) <= y0(i - 1))
                    {
                        pos = true;
                    }
                }
                else
                {
                    if (count > 1)
                    {
                        if (y0(i + 1) <= y0(i))
                        {
                            pos = true;
                        }
                    }
                }

                if (pos)
                {
                    positiveGeometryGroup.Children.Add(column);
                }
                else
                {
                    negativeGeometryGroup.Children.Add(column);
                }
            }

            Columns.Count = count;
            //}
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