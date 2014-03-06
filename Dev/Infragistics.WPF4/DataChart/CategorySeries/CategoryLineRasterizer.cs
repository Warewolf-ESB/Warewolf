using System;



using System.Linq;

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





    internal class CategoryLineRasterizer
    {
        /// <summary>
        /// True, if the series uses a date time axis; false otherwise.
        /// </summary>
        internal bool IsSortingAxis { get; set; }

        private PointCollection _flattenedLinePoints = new PointCollection();
        internal PointCollection FlattenedLinePoints
        {
            get { return _flattenedLinePoints; }
            set { _flattenedLinePoints = value; }
        }



#region Infragistics Source Cleanup (Region)






























































































































































#endregion // Infragistics Source Cleanup (Region)

        public void RasterizePolylinePaths(Path polylines0, Path polygons01, Path polylines1, int count,
            Func<int, double> x0, Func<int, double> y0, Func<int, double> x1, Func<int, double> y1,
            UnknownValuePlotting unknownValuePlotting, Clipper clipper, int bucketSize, double resolution)
        {
            PathGeometry polylineData0 = new PathGeometry();
            PathGeometry polygonData01 = new PathGeometry();
            PathGeometry polylineData1 = new PathGeometry();

            polylines0.Data = polylineData0;
            polygons01.Data = polygonData01;
            polylines1.Data = polylineData1;

            polylineData0.Figures = new PathFigureCollection();
            polygonData01.Figures = new PathFigureCollection();
            polylineData1.Figures = new PathFigureCollection();

            List<PolyLineSegment> polylineSegments0 = new List<PolyLineSegment>();
            List<PolyLineSegment> polylineSegments1 = new List<PolyLineSegment>();
            List<PolyLineSegment> polygonSegments0 = new List<PolyLineSegment>();
            List<PolyLineSegment> polygonSegments1 = new List<PolyLineSegment>();

            if (unknownValuePlotting == UnknownValuePlotting.LinearInterpolate || unknownValuePlotting == UnknownValuePlotting.DontPlot)
            {
                Clipper incrementalClipper = unknownValuePlotting == UnknownValuePlotting.DontPlot ? clipper : null;

                int currentLineStartIndex = 0;
                for (int i = 0; i < count; i++)
                {
                    if (double.IsNaN(y0(i))) // if y0(i) is nanners, then point i is nanners.
                    {

                        int pointsInCurrentLine = i - currentLineStartIndex;
                        bool addPoints =
     (unknownValuePlotting == UnknownValuePlotting.LinearInterpolate && pointsInCurrentLine > 0) ||
     (unknownValuePlotting == UnknownValuePlotting.DontPlot && pointsInCurrentLine > 1);

                        if (addPoints)
                        {
                            if (unknownValuePlotting == UnknownValuePlotting.DontPlot || polylineSegments0.Count == 0)
                            {
                                // start a new segment if this is the first segment, or if we're in DontPlot mode
                                PolyLineSegment currentPolylineSegment0 = new PolyLineSegment();
                                PolyLineSegment currentPolylineSegment1 = new PolyLineSegment();
                                PolyLineSegment currentPolygonSegment0 = new PolyLineSegment();
                                PolyLineSegment currentPolygonSegment1 = new PolyLineSegment();

                                polylineSegments0.Add(currentPolylineSegment0);
                                polylineSegments1.Add(currentPolylineSegment1);
                                polygonSegments0.Add(currentPolygonSegment0);
                                polygonSegments1.Add(currentPolygonSegment1);
                            }
                            // call RasterizePolyline to populate the points in the current segment.
                            this.RasterizePolyline(
                                polylineSegments0[polylineSegments0.Count - 1].Points,
                                polylineSegments1[polylineSegments1.Count - 1].Points,
                                polygonSegments0[polygonSegments0.Count - 1].Points,
                                polygonSegments1[polygonSegments1.Count - 1].Points,
                                currentLineStartIndex,
                                i - 1, x0, y0, x1, y1, incrementalClipper, bucketSize, resolution);
                        }
                        currentLineStartIndex = i + 1;
                    }
                }
                if (unknownValuePlotting == UnknownValuePlotting.DontPlot || polylineSegments0.Count == 0)
                {
                    // start a new segment if this is the first segment, or if we're in DontPlot mode
                    PolyLineSegment lastPolylineSegment0 = new PolyLineSegment();
                    PolyLineSegment lastPolygonSegment0 = new PolyLineSegment();
                    PolyLineSegment lastPolygonSegment1 = new PolyLineSegment();
                    PolyLineSegment lastPolylineSegment1 = new PolyLineSegment();

                    polylineSegments0.Add(lastPolylineSegment0);
                    polylineSegments1.Add(lastPolylineSegment1);
                    polygonSegments0.Add(lastPolygonSegment0);
                    polygonSegments1.Add(lastPolygonSegment1);
                }
                this.RasterizePolyline(polylineSegments0[polylineSegments0.Count - 1].Points,
                    polylineSegments1[polylineSegments1.Count - 1].Points,
                    polygonSegments0[polygonSegments0.Count - 1].Points,
                    polygonSegments1[polygonSegments1.Count - 1].Points,
                    currentLineStartIndex, count - 1, x0, y0, x1, y1, incrementalClipper, bucketSize, resolution);

                if (incrementalClipper == null && polylineSegments0.Count == 1 && clipper != null)
                {
                    // linear interpolate ... clipping is deferred until we have the whole line, which is right now
                    this.ClipSegment(polylineSegments0[0], clipper);
                }
            }
            else
            {
                polylineSegments0.Add(new PolyLineSegment());
                polylineSegments1.Add(new PolyLineSegment());
                polygonSegments0.Add(new PolyLineSegment());
                polygonSegments1.Add(new PolyLineSegment());

                this.RasterizePolyline(polylineSegments0[0].Points,
                    polylineSegments1[0].Points,
                    polygonSegments0[0].Points,
                    polygonSegments1[0].Points, 
                    count, x0, y0, x1, y1, clipper, bucketSize, resolution);

            }
            for (int current = 0; current < polylineSegments0.Count; current++)
            {
                PolyLineSegment polylineSegment0 = polylineSegments0[current];
                PolyLineSegment polylineSegment1 = polylineSegments1[current];
                PolyLineSegment polygonSegment0 = polygonSegments0[current];
                PolyLineSegment polygonSegment1 = polygonSegments1[current];

                if (polylineSegment0.Points.Count > 0)
                {
                    PathFigure polylineFigure0 = new PathFigure() { StartPoint = polylineSegment0.Points[0] };
                    polylineFigure0.Segments.Add(polylineSegment0);
                    polylineData0.Figures.Add(polylineFigure0);
                }
                if (polylineSegment1.Points.Count > 0)
                {
                    PathFigure polylineFigure1 = new PathFigure() { StartPoint = polylineSegment1.Points[0] };
                    polylineFigure1.Segments.Add(polylineSegment1);
                    polylineData1.Figures.Add(polylineFigure1);
                }
                if (polygonSegment0.Points.Count > 0 && polygonSegment1.Points.Count > 0)
                {
                    // combine polygonSegment0 and polygonSegment1
                    PolyLineSegment polygonSegment01 = new PolyLineSegment();
                    if (clipper != null)
                    {
                        bool temp = clipper.IsClosed;
                        clipper.IsClosed = true;
                        clipper.Target = polygonSegment01.Points;
                        foreach (Point p in polygonSegment0.Points)
                        {
                            clipper.Add(p);
                        }
                        foreach (Point p in polygonSegment1.Points.Reverse())
                        {
                            clipper.Add(p);
                        }
                        clipper.Target = null;
                        clipper.IsClosed = temp;
                    }
                    else
                    {
                        foreach (Point p in polygonSegment0.Points)
                        {
                            polygonSegment01.Points.Add(p);
                        }
                        foreach (Point p in polygonSegment1.Points.Reverse())
                        {
                            polygonSegment01.Points.Add(p);
                        }
                    }

                    if (polygonSegment01.Points.Count > 0)
                    {
                        PathFigure polygonFigure01 = new PathFigure() { StartPoint = polygonSegment01.Points[0] };
                        polygonFigure01.Segments.Add(polygonSegment01);
                        polygonData01.Figures.Add(polygonFigure01);
                    }
                }
            }
        }


        private void ClipSegment(PolyLineSegment segment, Clipper clipper)
        {
            PointCollection points = segment.Points;
            clipper.Target = segment.Points = new PointCollection();
            foreach (Point p in points)
            {
                clipper.Add(p);
            }
            clipper.Target = null;
        }



#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Renders into a potentially chunky polyline
        /// </summary>
        /// <param name="polyline0">Bottom polyline</param>
        /// <param name="polyline1">Top polyline</param>
        /// <param name="polygon0">Bottom of center polygon</param>
        /// <param name="polygon1">Top of center polygon</param>
        /// <param name="count">Number of points</param>
        /// <param name="x0">Bottom points x coordinate indexed left to right</param>
        /// <param name="y0">Bottom points y coordinate indexed left to right</param>
        /// <param name="x1">Top points x coordinate indexed right to left</param>
        /// <param name="y1">Top points y coordinate indexed right to left</param>
        /// <param name="clipper">Clipper to use</param>
        /// <param name="bucketSize">Current bucketsize of the series</param>
        /// <param name="resolution">Current resolution of the chart</param>
        public void RasterizePolyline(Polyline polyline0,
                                            Polyline polyline1,
                                            Polyline polygon0,
                                            Polyline polygon1,
                                            int count,
                                            Func<int, double> x0, Func<int, double> y0,
                                            Func<int, double> x1, Func<int, double> y1,
                                            Clipper clipper, int bucketSize,
                                            double resolution)
        {
            polyline0.Points.Clear();
            polygon0.Points.Clear();
            polygon1.Points.Clear();
            polyline1.Points.Clear();

            this.RasterizePolyline(polyline0.Points,
                polyline1.Points,
                polygon0.Points,
                polygon1.Points,
                count, x0, y0, x1, y1, clipper, bucketSize, resolution);

            polyline0.IsHitTestVisible = polyline0.Points.Count > 0;
            polygon0.IsHitTestVisible = polygon0.Points.Count > 0;
            polygon1.IsHitTestVisible = polygon1.Points.Count > 0;
            polyline1.IsHitTestVisible = polyline1.Points.Count > 0;
        }




#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

        public void RasterizePolyline(PointCollection polylinePoints0,
                                        PointCollection polylinePoints1,
                                        PointCollection polygonPoints0,
                                        PointCollection polygonPoints1,
                                        int count,
                                        Func<int, double> x0, Func<int, double> y0,
                                        Func<int, double> x1, Func<int, double> y1,
                                        Clipper clipper, int bucketSize,
                                        double resolution)
        {
            this.RasterizePolyline(polylinePoints0, polylinePoints1, polygonPoints0, polygonPoints1,
                0, count - 1, x0, y0, x1, y1, clipper, bucketSize, resolution);
        }




#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)

        private void FlattenPoints(PointCollection points, int startIndex, int endIndex, Func<int, double> xDelegate, Func<int, double> yDelegate, double resolution)
        {
            var flattened = Flattener.Flatten(new List<int>(), xDelegate, yDelegate, startIndex, endIndex, resolution);
            int j = 0;
            for (int i = 0; i < flattened.Count; i++)
            {
                j = flattened[i];
                Point pointToAdd = new Point(xDelegate(j), yDelegate(j));
                points.Add(pointToAdd);
            }
        }


        private void ClipPoints(PointCollection points, PointCollection pointsToClip, Clipper clipper, double resolution)
        {
            clipper.Target = points;
            for (int i = 0; i < pointsToClip.Count; i++)
            {
                clipper.Add(pointsToClip[i]);
            }
            clipper.Target = null;
        }



#region Infragistics Source Cleanup (Region)


































































#endregion // Infragistics Source Cleanup (Region)

        public void RasterizePolyline(PointCollection polylinePoints0,
                                        PointCollection polylinePoints1,
                                        PointCollection polygonPoints0,
                                        PointCollection polygonPoints1,
                                        int startIndex, int endIndex,
                                        Func<int, double> x0, Func<int, double> y0,
                                        Func<int, double> x1, Func<int, double> y1,
                                        Clipper clipper, int bucketSize,
                                        double resolution)
        {
            if (endIndex > -1)
            {
                //data on DateTime axis should be treated as bucketized (bucketsize > 1)
                if (bucketSize == 1 && !IsSortingAxis)
                {
                    PointCollection polylinePoints0_new = new PointCollection();
                    this.FlattenPoints(polylinePoints0_new, startIndex, endIndex, x0, y0, resolution);

                    if (clipper != null)
                    {
                        this.ClipPoints(polylinePoints0, polylinePoints0_new, clipper, resolution);
                    }
                    else
                    {
                        foreach (Point p in polylinePoints0_new)
                        {
                            polylinePoints0.Add(p);
                        }
                    }
                }
                else
                {
                    PointCollection polylinePoints0_new = new PointCollection();
                    PointCollection polylinePoints1_new = new PointCollection();

                    // flatten polyline points
                    this.FlattenPoints(polylinePoints0_new, startIndex, endIndex, x0, y0, resolution);
                    this.FlattenPoints(polylinePoints1_new, startIndex, endIndex, x1, y1, resolution);

                    // prior to clipping, save flattened polyline points as polygon points
                    foreach (Point point in polylinePoints0_new)
                    {
                        polygonPoints0.Add(point);
                    }
                    foreach (Point point in polylinePoints1_new)
                    {
                        polygonPoints1.Add(point);
                    }

                    // perform clipping, if needed
                    if (clipper != null)
                    {
                        this.ClipPoints(polylinePoints0, polylinePoints0_new, clipper, resolution);
                        this.ClipPoints(polylinePoints1, polylinePoints1_new, clipper, resolution);
                    }
                    else
                    {
                        foreach (Point p in polylinePoints0_new)
                        {
                            polylinePoints0.Add(p);
                        }
                        foreach (Point p in polylinePoints1_new)
                        {
                            polylinePoints1.Add(p);
                        }
                    }
                }
            }
        }




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        public void RasterizePolygonPaths(Path polygons0, Path polylines0, Path polygons01, Path polylines1,
                                       int count,
                                       Func<int, double> x0, Func<int, double> y0,
                                       Func<int, double> x1, Func<int, double> y1,
                                       int bucketSize, double resolution, Action<PointCollection, PointCollection, PointCollection, PointCollection, bool> terminatePolygon, UnknownValuePlotting unknownValuePlotting)
        {

            PathGeometry polygonData0 = new PathGeometry();
            PathGeometry polylineData0 = new PathGeometry();
            PathGeometry polygonData01 = new PathGeometry();
            PathGeometry polylineData1 = new PathGeometry();

            polygons0.Data = polygonData0;
            polylines0.Data = polylineData0;
            polygons01.Data = polygonData01;
            polylines1.Data = polylineData1;

            polygonData0.Figures = new PathFigureCollection();
            polylineData0.Figures = new PathFigureCollection();
            polygonData01.Figures = new PathFigureCollection();
            polylineData1.Figures = new PathFigureCollection();

            List<PolyLineSegment> polygonSegments0 = new List<PolyLineSegment>();
            List<PolyLineSegment> polylineSegments0 = new List<PolyLineSegment>();
            List<PolyLineSegment> polygonSegments01 = new List<PolyLineSegment>();
            List<PolyLineSegment> polylineSegments1 = new List<PolyLineSegment>();

            if (unknownValuePlotting == UnknownValuePlotting.LinearInterpolate || unknownValuePlotting == UnknownValuePlotting.DontPlot)
            {
                int currentLineStartIndex = 0;
                for (int i = 0; i < count; i++)
                {



                    if (double.IsNaN(y0(i))) // if y0(i) is nanners, then point i is nanners.

                    {

                        int pointsInCurrentLine = i - currentLineStartIndex;
                        bool addPoints =
                            (unknownValuePlotting == UnknownValuePlotting.LinearInterpolate && pointsInCurrentLine > 0) ||
                            (unknownValuePlotting == UnknownValuePlotting.DontPlot && pointsInCurrentLine > 1);

                        if (addPoints)
                        {
                            if (unknownValuePlotting == UnknownValuePlotting.DontPlot || polylineSegments0.Count == 0)
                            {
                                // start a new segment if this is the first segment, or if we're in DontPlot mode
                                PolyLineSegment currentPolygonSegment0 = new PolyLineSegment();
                                PolyLineSegment currentPolylineSegment0 = new PolyLineSegment();
                                PolyLineSegment currentPolygonSegment01 = new PolyLineSegment();
                                PolyLineSegment currentPolylineSegment1 = new PolyLineSegment();

                                polygonSegments0.Add(currentPolygonSegment0);
                                polylineSegments0.Add(currentPolylineSegment0);
                                polygonSegments01.Add(currentPolygonSegment01);
                                polylineSegments1.Add(currentPolylineSegment1);
                            }
                            // call RasterizePolyline to populate the points in the current segment.

                            this.RasterizePolygon(polygonSegments0[polygonSegments0.Count - 1].Points, polylineSegments0[polylineSegments0.Count - 1].Points, polygonSegments01[polygonSegments01.Count - 1].Points, polylineSegments1[polylineSegments1.Count - 1].Points, currentLineStartIndex, i - 1, 




                                x0, y0, x1, y1, 

                                bucketSize, resolution);

                            if (unknownValuePlotting == UnknownValuePlotting.DontPlot)
                            {
                                terminatePolygon(polygonSegments0[polygonSegments0.Count - 1].Points,
                                                 polylineSegments0[polylineSegments0.Count - 1].Points, 
                                                 polygonSegments01[polygonSegments01.Count - 1].Points, 
                                                 polylineSegments1[polylineSegments1.Count - 1].Points,
                                                 false);
                            }
                        }
                        currentLineStartIndex = i + 1;
                    }
                }
                if (unknownValuePlotting == UnknownValuePlotting.DontPlot || polylineSegments0.Count == 0)
                {
                    PolyLineSegment lastPolygonSegment0 = new PolyLineSegment();
                    PolyLineSegment lastPolylineSegment0 = new PolyLineSegment();
                    PolyLineSegment lastPolygonSegment01 = new PolyLineSegment();
                    PolyLineSegment lastPolylineSegment1 = new PolyLineSegment();

                    polygonSegments0.Add(lastPolygonSegment0);
                    polylineSegments0.Add(lastPolylineSegment0);
                    polygonSegments01.Add(lastPolygonSegment01);
                    polylineSegments1.Add(lastPolylineSegment1);
                }
                this.RasterizePolygon(polygonSegments0[polygonSegments0.Count - 1].Points, polylineSegments0[polylineSegments0.Count - 1].Points, polygonSegments01[polygonSegments01.Count - 1].Points, polylineSegments1[polylineSegments1.Count - 1].Points, currentLineStartIndex, count - 1, 




                    x0, y0, x1, y1, 

                    bucketSize, resolution);
                terminatePolygon(polygonSegments0[polygonSegments0.Count - 1].Points,
                                 polylineSegments0[polylineSegments0.Count - 1].Points, 
                                 polygonSegments01[polygonSegments01.Count - 1].Points, 
                                 polylineSegments1[polylineSegments1.Count - 1].Points, true);
            }
            else
            {
                polygonSegments0.Add(new PolyLineSegment());
                polylineSegments0.Add(new PolyLineSegment());
                polygonSegments01.Add(new PolyLineSegment());
                polylineSegments1.Add(new PolyLineSegment());

                this.RasterizePolygon(polygonSegments0[0].Points, polylineSegments0[0].Points, polygonSegments01[0].Points, polylineSegments1[0].Points, 0, count - 1, 




                    x0, y0, x1, y1, 

                    bucketSize, resolution);
                terminatePolygon(polygonSegments0[0].Points,
                polylineSegments0[0].Points,
                polygonSegments01[0].Points,
                polylineSegments1[0].Points, true);
            }
            for (int current = 0; current < polylineSegments0.Count; current++)
            {
                PolyLineSegment polygonSegment0 = polygonSegments0[current];
                PolyLineSegment polylineSegment0 = polylineSegments0[current];
                PolyLineSegment polygonSegment01 = polygonSegments01[current];
                PolyLineSegment polylineSegment1 = polylineSegments1[current];

                if (polygonSegment0.Points.Count > 0)
                {
                    PathFigure polygonFigure0 = new PathFigure() { StartPoint = polygonSegment0.Points[0] };
                    polygonFigure0.Segments.Add(polygonSegment0);
                    polygonData0.Figures.Add(polygonFigure0);
                }
                if (polylineSegment0.Points.Count > 0)
                {
                    PathFigure polylineFigure0 = new PathFigure() { StartPoint = polylineSegment0.Points[0] };
                    polylineFigure0.Segments.Add(polylineSegment0);
                    polylineData0.Figures.Add(polylineFigure0);
                }
                if (polygonSegment01.Points.Count > 0)
                {
                    PathFigure polygonFigure01 = new PathFigure() { StartPoint = polygonSegment01.Points[0] };
                    polygonFigure01.Segments.Add(polygonSegment01);
                    polygonData01.Figures.Add(polygonFigure01);
                }
                if (polylineSegment1.Points.Count > 0)
                {
                    PathFigure polylineFigure1 = new PathFigure() { StartPoint = polylineSegment1.Points[0] };
                    polylineFigure1.Segments.Add(polylineSegment1);
                    polylineData1.Figures.Add(polylineFigure1);
                }
            }
        }

        // this one seems unused.
        ///// <summary>
        ///// Renders into a anchored polygon with a potentially chunky top edge
        ///// </summary>
        ///// <param name="polygon0">Bottom polygon</param>
        ///// <param name="polyline0">Bottom polyline</param>
        ///// <param name="polygon01">Top polygon</param>
        ///// <param name="polyline1">Top polyline</param>
        ///// <param name="count">Number of points</param>
        ///// <param name="x0">Bottom points x coordinate indexed left to right</param>
        ///// <param name="y0">Bottom points y coordinate indexed left to right</param>
        ///// <param name="x1">Top points x coordinate indexed right to left</param>
        ///// <param name="y1">Top points y coordinate indexed right to left</param>
        ///// <param name="bucketSize">The current bucketsize of the series.</param>
        ///// <param name="resolution">The current resolution of the series.</param>
        ///// <param name="terminatePolygon">Strategy to terminate the polygon.</param>
        //public void RasterizePolygon(Polygon polygon0, Polyline polyline0, Polygon polygon01, Polyline polyline1,
        //                                int count,
        //                                Func<int, double> x0, Func<int, double> y0,
        //                                Func<int, double> x1, Func<int, double> y1,
        //                                int bucketSize, double resolution, Action<PointCollection> terminatePolygon)
        //{
        //    polygon0.Points.Clear();
        //    polyline0.Points.Clear();
        //    polygon01.Points.Clear();
        //    polyline1.Points.Clear();

        //    this.RasterizePolygon(polygon0.Points, polyline0.Points, polygon01.Points, polyline1.Points, 0, count - 1, x0, y0, x1, y1, bucketSize, resolution);

        //    polygon0.IsHitTestVisible = polygon0.Points.Count > 0;
        //    polyline0.IsHitTestVisible = polyline0.Points.Count > 0;
        //    polygon01.IsHitTestVisible = polygon01.Points.Count > 0;
        //    polyline1.IsHitTestVisible = polyline1.Points.Count > 0;

        //    terminatePolygon(polygon0.Points);
        //}




#region Infragistics Source Cleanup (Region)






















































#endregion // Infragistics Source Cleanup (Region)

        public void RasterizePolygon(PointCollection polygonPoints0, PointCollection polylinePoints0, PointCollection polygonPoints01, PointCollection polylinePoints1,
                                        int startIndex, int endIndex,
                                        Func<int, double> x0, Func<int, double> y0,
                                        Func<int, double> x1, Func<int, double> y1,
                                        int bucketSize, double resolution)
        {
            FlattenedLinePoints.Clear();

            //data on DateTime axis should be treated as bucketized (bucketsize > 1)
            if (bucketSize == 1 && !IsSortingAxis)
            {
                foreach (int i in Flattener.Flatten(new List<int>(), x0, y0, startIndex, endIndex, resolution))
                {
                    polygonPoints0.Add(new Point(x0(i), y0(i)));
                    polylinePoints1.Add(new Point(x0(i), y0(i)));
                    FlattenedLinePoints.Add(new Point(x0(i), y0(i)));
                }
            }
            else
            {
                foreach (int i in Flattener.Flatten(new List<int>(), x0, y0, startIndex, endIndex, resolution))
                {
                    polygonPoints0.Add(new Point(x0(i), y0(i)));
                    polylinePoints0.Add(new Point(x0(i), y0(i)));
                    polygonPoints01.Add(new Point(x0(i), y0(i)));
                    FlattenedLinePoints.Add(new Point(x0(i), y0(i)));
                }

                //Flatten the polyline and add the resulting points in reverse order.
                //Reversing the order is required to handle null values properly. 
                //The first point in the reverse polyline/polygon should start at the same X value as the last point in the main polyline/polygon.
                foreach (int i in Flattener.Flatten(new List<int>(), x1, y1, startIndex, endIndex, resolution).Reverse())
                {
                    polylinePoints1.Add(new Point(x1(i), y1(i)));
                    polygonPoints01.Add(new Point(x1(i), y1(i)));
                }
            }
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