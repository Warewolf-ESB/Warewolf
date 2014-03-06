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
using System.Windows.Data;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;




using System.Linq;



namespace Infragistics.Controls.Charts
{
    internal interface IHasTrendline
    {
        TrendLineType TrendLineType { get; }
    }

    internal interface IHasCategoryTrendline
        : IHasTrendline
    {
        IPreparesCategoryTrendline TrendlinePreparer { get; }
        int TrendLinePeriod { get; }
    }

    internal class DefaultCategoryTrendlineHost
        : IHasCategoryTrendline
    {
        public DefaultCategoryTrendlineHost()
        {
            _trendLinePreparer = new DefaultCategoryTrendlinePreparer();
        }

        private IPreparesCategoryTrendline _trendLinePreparer;
        public IPreparesCategoryTrendline TrendlinePreparer
        {
            get { return _trendLinePreparer; }
        }

        public TrendLineType TrendLineType
        {
            get { return TrendLineType.None; }
        }

        public int TrendLinePeriod
        {
            get { return 1; }
        }
    }

    internal interface IPreparesCategoryTrendline
    {
        void PrepareLine(List<Point> flattenedPoints, TrendLineType trendLineType,
            IList<double> valueColumn, int period,
            Func<double, double> getScaledXValue,
            Func<double, double> getScaledYValue,
            TrendResolutionParams trendResolutionParams);
    }

    internal class DefaultCategoryTrendlinePreparer
        : IPreparesCategoryTrendline
    {
        public void PrepareLine(
            List<Point> flattenedPoints,
            TrendLineType trendLineType,
            IList<double> valueColumn,
            int period,
            Func<double, double> getScaledXValue,
            Func<double, double> getScaledYValue,
            TrendResolutionParams trendResolutionParams)
        {
        }
    }

    internal class TrendResolutionParams
    {
        public int FirstBucket { get; set; }
        public int LastBucket { get; set; }
        public int BucketSize { get; set; }
        public Rect Viewport { get; set; }
        public Rect Window { get; set; }
        public double Resolution { get; set; }
        public double Offset { get; set; }
    }

    //[WidgetModule("Required")]
    //internal class TrendLinePropertyNames
    //{
    //    public const string DashArray = "TrendLineDashArray";
    //    public const string Type = "TrendLineType";
    //    public const string Period = "TrendLinePeriod";
    //    public const string Brush = "TrendLineBrush";
    //    public const string ActualBrush = "ActualTrendLineBrush";
    //    public const string Thickness = "TrendLineThickness";
    //    public const string DashCap = "TrendLineDashCap";
    //    public const string ZIndex = "TrendLineZIndex";
    //}

    internal class TrendFitCalculator
    {
        public static double[] CalculateFit(List<Point> trend, TrendLineType trendLineType, TrendResolutionParams trendResolutionParams,
            double[] trendCoefficients, int count,
            Func<int, double> GetUnscaledX, Func<int, double> GetUnscaledY,
            Func<double, double> GetScaledXValue, Func<double, double> GetScaledYValue,
            double xmin, double xmax)
        {
            if (trendCoefficients == null)
            {
                //(i) => i + 1;
                switch (trendLineType)
                {
                    case TrendLineType.LinearFit: trendCoefficients = LeastSquaresFit.LinearFit(count, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.QuadraticFit: trendCoefficients = LeastSquaresFit.QuadraticFit(count, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.CubicFit: trendCoefficients = LeastSquaresFit.CubicFit(count, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.QuarticFit: trendCoefficients = LeastSquaresFit.QuarticFit(count, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.QuinticFit: trendCoefficients = LeastSquaresFit.QuinticFit(count, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.ExponentialFit: trendCoefficients = LeastSquaresFit.ExponentialFit(count, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.LogarithmicFit: trendCoefficients = LeastSquaresFit.LogarithmicFit(count, GetUnscaledX, GetUnscaledY); break;
                    case TrendLineType.PowerLawFit: trendCoefficients = LeastSquaresFit.PowerLawFit(count, GetUnscaledX, GetUnscaledY); break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (trendCoefficients == null)
            {
                return null;
            }

            for (int i = 0; i < trendResolutionParams.Viewport.Width; i += 2)
            {
                double p = i / (trendResolutionParams.Viewport.Width - 1);
                double xi = xmin + p * (xmax - xmin);
                double yi = double.NaN;

                switch (trendLineType)
                {
                    case TrendLineType.LinearFit: yi = LeastSquaresFit.LinearEvaluate(trendCoefficients, xi); break;
                    case TrendLineType.QuadraticFit: yi = LeastSquaresFit.QuadraticEvaluate(trendCoefficients, xi); break;
                    case TrendLineType.CubicFit: yi = LeastSquaresFit.CubicEvaluate(trendCoefficients, xi); break;
                    case TrendLineType.QuarticFit: yi = LeastSquaresFit.QuarticEvaluate(trendCoefficients, xi); break;
                    case TrendLineType.QuinticFit: yi = LeastSquaresFit.QuinticEvaluate(trendCoefficients, xi); break;
                    case TrendLineType.ExponentialFit: yi = LeastSquaresFit.ExponentialEvaluate(trendCoefficients, xi); break;
                    case TrendLineType.LogarithmicFit: yi = LeastSquaresFit.LogarithmicEvaluate(trendCoefficients, xi); break;
                    case TrendLineType.PowerLawFit: yi = LeastSquaresFit.PowerLawEvaluate(trendCoefficients, xi); break;
                    default:
                        throw new NotImplementedException();
                }

                // convert xi and yi to pels

                xi = GetScaledXValue(xi);
                yi = GetScaledYValue(yi);

                if (!double.IsNaN(yi) && !double.IsInfinity(yi))
                {
                    trend.Add(new Point(xi + trendResolutionParams.Offset, yi));
                }
            }

            return trendCoefficients;
        }
    }

    internal class TrendAverageCalculator
    {
        private static IEnumerable<double> GetAverage(TrendLineType trendLineType, IEnumerable<double> sourceColumn,
            int period)
        {
            IEnumerable<double> average;

            switch (trendLineType)
            {
                case TrendLineType.SimpleAverage: 
                case TrendLineType.ExponentialAverage: 
                case TrendLineType.ModifiedAverage:
                case TrendLineType.WeightedAverage:
                    if (period < 1)
                    {
                        //clamp period to 1, to avoid div by 0 errors.
                        period = 1;
                    }
                    break;
            }

            switch (trendLineType)
            {
                case TrendLineType.SimpleAverage: average = Series.SMA(sourceColumn, period); break;
                case TrendLineType.ExponentialAverage: average = Series.EMA(sourceColumn, period); break;
                case TrendLineType.ModifiedAverage: average = Series.MMA(sourceColumn, period); break;
                case TrendLineType.CumulativeAverage: average = Series.CMA(sourceColumn); break;
                case TrendLineType.WeightedAverage: average = Series.WMA(sourceColumn, period); break;
                default:
                    throw new NotImplementedException();
            }

            return average;
        }

        public static void CalculateSingleValueAverage(TrendLineType trendLineType, IList<double> trendColumn,
            IList<double> valueColumn, int period)
        {
            if (trendColumn.Count == 0)
            {
                IEnumerable<double> average = GetAverage(trendLineType, valueColumn, period);

                foreach (double d in average)
                {
                    trendColumn.Add(d);
                }
            }
        }

        public static void CalculateXYAverage(TrendLineType trendLineType, IList<Point> trendColumn,
            IEnumerable<double> XColumn, IEnumerable<double> YColumn, int period)
        {
            if (trendColumn.Count == 0)
            {
                IEnumerator<double> xAverage = GetAverage(trendLineType, XColumn, period).GetEnumerator();
                IEnumerator<double> yAverage = GetAverage(trendLineType, YColumn, period).GetEnumerator();

                while (xAverage.MoveNext() && yAverage.MoveNext())
                {
                    trendColumn.Add(new Point(xAverage.Current, yAverage.Current));
                }
            }
        }
    }

    internal abstract class TrendLineManagerBase<TTrendColumn>
    {
        public List<TTrendColumn> TrendColumn { get; set; } // pseudo-column for averages
        protected double[] TrendCoefficients { get; set; }   // coefficients for fit

        public TrendLineManagerBase()
        {
            TrendColumn = new List<TTrendColumn>();
        }

        public Polyline TrendPolyline
        {
            get { return trendPolyline; }
        }
        public readonly Polyline trendPolyline = new Polyline() { IsHitTestVisible = false };

        public virtual void RasterizeTrendLine(List<Point> trendPoints)
        {
            RasterizeTrendLine(trendPoints, null);
        }

        protected bool IsFit(TrendLineType type)
        {
            return  type == TrendLineType.LinearFit ||
                    type == TrendLineType.QuadraticFit ||
                    type == TrendLineType.CubicFit ||
                    type == TrendLineType.QuarticFit ||
                    type == TrendLineType.QuinticFit ||
                    type == TrendLineType.LogarithmicFit ||
                    type == TrendLineType.ExponentialFit ||
                    type == TrendLineType.PowerLawFit;
        }

        protected bool IsAverage(TrendLineType type)
        {
            return type == TrendLineType.SimpleAverage ||
                    type == TrendLineType.ExponentialAverage ||
                    type == TrendLineType.ModifiedAverage ||
                    type == TrendLineType.CumulativeAverage ||
                    type == TrendLineType.WeightedAverage;
        }

        public virtual void RasterizeTrendLine(List<Point> trendPoints, Clipper clipper)
        {
            TrendPolyline.Points.Clear();

            if (clipper != null)
            {
                clipper.Target = TrendPolyline.Points;
            }

            if (trendPoints != null)
            {
                foreach (Point point in trendPoints)
                {
                    if (!double.IsNaN(point.X) && !double.IsNaN(point.Y))
                    {
                        if (clipper != null)
                        {
                            clipper.Add(point);
                        }
                        else
                        {
                            TrendPolyline.Points.Add(point);
                        }
                    }
                }
            }

            TrendPolyline.IsHitTestVisible = TrendPolyline.Points.Count > 0;
        }

        protected virtual void FlattenTrendLine(IList<Point> trend,
            TrendResolutionParams trendResolutionParams, List<Point> flattenedPoints)
        {
            FlattenTrendLine(trend, trendResolutionParams, flattenedPoints, null);
        }

        protected virtual void FlattenTrendLine(IList<Point> trend,
            TrendResolutionParams trendResolutionParams, List<Point> flattenedPoints, Clipper clipper)
        {
            if (clipper != null)
            {
                clipper.Target = flattenedPoints;
            }

            foreach (int i in Flattener.Flatten(trend.Count,
                (i) => trend[i].X, (i) => trend[i].Y,
                trendResolutionParams.Resolution))
            {
                if (clipper != null)
                {
                    clipper.Add(trend[i]);
                }
                else
                {
                    flattenedPoints.Add(trend[i]);
                }
            }
        }

        public void AttachPolyLine(Canvas rootCanvas, Series owner)
        {
            if (rootCanvas == null || owner == null)
            {
                return;
            }
            if (TrendPolyline.Parent != null)
            {
                Detach();
            }
            rootCanvas.Children.Add(TrendPolyline);


            TrendPolyline.SetBinding(Shape.StrokeProperty, new Binding(TrendLineActualBrushPropertyName) { Source = owner });
            TrendPolyline.SetBinding(Shape.StrokeThicknessProperty, new Binding(TrendLineThicknessPropertyName) { Source = owner });
            TrendPolyline.SetBinding(Shape.StrokeDashCapProperty, new Binding(TrendLineDashCapPropertyName) { Source = owner });
            TrendPolyline.SetBinding(Canvas.ZIndexProperty, new Binding(TrendLineZIndexPropertyName) { Source = owner });
            TrendPolyline.SetBinding(Shape.StrokeDashArrayProperty, new Binding(TrendLineDashArrayPropertyName) 
            { 
                Source = owner, 
                Converter = new DoubleCollectionDuplicator() 
            });
            VisualInformationManager.SetIsTrendLineVisual(TrendPolyline, true);

        }

        public void Detach()
        {
            if (TrendPolyline == null)
            {
                return;
            }
            Panel parent = TrendPolyline.Parent as Panel;
            if (parent != null)
            {
                parent.Children.Remove(TrendPolyline);
            }

            TrendPolyline.ClearValue(Shape.StrokeProperty);
            TrendPolyline.ClearValue(Shape.StrokeThicknessProperty);
            TrendPolyline.ClearValue(Shape.StrokeDashCapProperty);
            TrendPolyline.ClearValue(Canvas.ZIndexProperty);
            TrendPolyline.ClearValue(Shape.StrokeDashArrayProperty);

        }

        public void ClearPoints()
        {
            TrendPolyline.Points.Clear();
        }

        public void Reset()
        {
            TrendCoefficients = null;
            TrendColumn.Clear();
        }

        public void DataUpdated(FastItemsSourceEventAction action, int position, int count, string propertyName)
        {
            switch (action)
            {
                case FastItemsSourceEventAction.Change:
                case FastItemsSourceEventAction.Replace:
                case FastItemsSourceEventAction.Insert:
                case FastItemsSourceEventAction.Remove:
                case FastItemsSourceEventAction.Reset:
                    Reset();
                    break;
            }
        }

        public const string TrendLineDashArrayPropertyName = "TrendLineDashArray";
        public const string TrendLineTypePropertyName = "TrendLineType";
        public const string TrendLinePeriodPropertyName = "TrendLinePeriod";
        public const string TrendLineBrushPropertyName = "TrendLineBrush";
        public const string TrendLineActualBrushPropertyName = "ActualTrendLineBrush";
        public const string TrendLineThicknessPropertyName = "TrendLineThickness";
        public const string TrendLineDashCapPropertyName = "TrendLineDashCap";
        public const string TrendLineZIndexPropertyName = "TrendLineZIndex";

        public bool PropertyUpdated(object sender, string propertyName, object oldValue,
            object newValue, DoubleCollection trendLineDashArray)
        {
            bool requiresRender = false;

            switch (propertyName)
            {
                case TrendLineTypePropertyName:
                case TrendLinePeriodPropertyName:
                    Reset();
                    requiresRender = true;
                    break;
                case TrendLineThicknessPropertyName:
                    requiresRender = true;
                    break;
                case Series.FastItemsSourcePropertyName:
                    requiresRender = true;
                    Reset();
                    break;
            }

            return requiresRender;
        }
    }
    
//#if !TINYCLR
    internal class PolarTrendLineManager
        : TrendLineManagerBase<Point>
    {
        public void PrepareLine(List<Point> flattenedPoints, TrendLineType trendLineType,
           IEnumerable<double> angleColumn, 
           IEnumerable<double> radiusColumn,
           int period,
           Func<double, double> getScaledAngleValue,
           Func<double, double> getScaledRadiusValue,
           TrendResolutionParams trendResolutionParams, Clipper clipper, double min, double max)
        {
            double xmin = min;
            double xmax = max;
            List<Point> trend = new List<Point>();

            if (!trendResolutionParams.Window.IsEmpty && !trendResolutionParams.Viewport.IsEmpty)
            {
                IList<double> angleList = null;
                IList<double> radiusList = null;
                if (angleColumn != null)
                {
                    angleList = angleColumn.ToList();
                }
                if (radiusColumn != null)
                {
                    radiusList = radiusColumn.ToList();
                }

                int count = 0;
                if (angleList != null)
                {
                    count = angleList.Count;
                }
                if (radiusList != null)
                {
                    count = Math.Min(count, radiusList.Count);
                }

                if (trendLineType == TrendLineType.None)
                {
                    TrendCoefficients = null;
                    TrendColumn.Clear();
                    return;
                }

                if (IsFit(trendLineType))
                {
                    TrendColumn.Clear();

                    TrendCoefficients =
                        TrendFitCalculator.CalculateFit(trend, trendLineType, trendResolutionParams,
                        TrendCoefficients, count, (i) => angleList[i], (i) => radiusList[i],
                        getScaledAngleValue, getScaledRadiusValue, xmin, xmax);
                }

                if (IsAverage(trendLineType))
                {
                    TrendCoefficients = null;

                    TrendAverageCalculator.CalculateXYAverage(trendLineType, TrendColumn, angleColumn, radiusColumn, period);

                    // trend column is valid.

                    foreach (Point point in TrendColumn)
                    {
                        double xi = getScaledAngleValue(point.X);
                        double yi = getScaledRadiusValue(point.Y);

                        if (!double.IsNaN(xi) && !double.IsNaN(yi))
                        {
                            trend.Add(new Point(xi, yi));
                        }
                    }
                }

                if (trend.Count > 0)
                {
                    FlattenTrendLine(trend, trendResolutionParams, flattenedPoints, clipper);
                }
            }
        }

        public bool UseCartesianInterpolation { get; set; }
        public UnknownValuePlotting UnknownValuePlotting { get; set; }
        public double RadiusExtentScale { get; set; }
        public double InnerRadiusExtentScale { get; set; }
        public Func<double, double, double> ProjectX { get; set; }
        public Func<double, double, double> ProjectY { get; set; }

        protected override void FlattenTrendLine(IList<Point> trend,
            TrendResolutionParams trendResolutionParams, List<Point> flattenedPoints, Clipper clipper)
        {
            if (clipper != null)
            {
                clipper.Target = flattenedPoints;
            }

            PolarLinePlanner planner = new PolarLinePlanner
            {
                AngleProvider = (i) => trend[i].X,
                RadiusProvider = (i) => trend[i].Y,
                Clipper = clipper,
                Count = trend.Count,
                Resolution = trendResolutionParams.Resolution,
                TransformedXProvider = (i) => ProjectX(trend[i].X, trend[i].Y),
                TransformedYProvider = (i) => ProjectY(trend[i].X, trend[i].Y),
                UseCartesianInterpolation = UseCartesianInterpolation,
                UnknownValuePlotting = UnknownValuePlotting,
                Viewport = trendResolutionParams.Viewport,
                Window = trendResolutionParams.Window
            };

            planner.PrepareLine();

        }
    }



    internal class RadialTrendLineManager
        : TrendLineManagerBase<double>
    {
        public void PrepareLine(List<Point> flattenedPoints, TrendLineType trendLineType,
           IList<double> valueColumn, int period,
           Func<double, double> GetScaledAngleValue,
           Func<double, double> GetScaledRadiusValue,
           TrendResolutionParams trendResolutionParams, Clipper clipper)
        {
            double xmin = trendResolutionParams.FirstBucket * trendResolutionParams.BucketSize;
            double xmax = trendResolutionParams.LastBucket * trendResolutionParams.BucketSize;
            List<Point> trend = new List<Point>();

            if (!trendResolutionParams.Window.IsEmpty && !trendResolutionParams.Viewport.IsEmpty)
            {
                if (trendLineType == TrendLineType.None)
                {
                    TrendCoefficients = null;
                    TrendColumn.Clear();
                    return;
                }

                if (IsFit(trendLineType))
                {
                    TrendColumn.Clear();

                    TrendCoefficients =
                        TrendFitCalculator.CalculateFit(trend, trendLineType, trendResolutionParams,
                        TrendCoefficients, valueColumn.Count, (i) => i + 1, (i) => valueColumn[i],
                        GetScaledAngleValue, GetScaledRadiusValue, xmin, xmax);
                }

                if (IsAverage(trendLineType))
                {
                    TrendCoefficients = null;

                    TrendAverageCalculator.CalculateSingleValueAverage(trendLineType, TrendColumn, valueColumn, period);

                    // trend column is valid.

                    for (int i = trendResolutionParams.FirstBucket; i <= trendResolutionParams.LastBucket; i += 1)
                    {
                        int itemIndex = (i % valueColumn.Count) * trendResolutionParams.BucketSize;

                        if (itemIndex >= 0 && itemIndex < TrendColumn.Count)
                        {
                            double xi = GetScaledAngleValue(itemIndex);
                            double yi = GetScaledRadiusValue(TrendColumn[itemIndex]);

                            if (!double.IsNaN(xi) && !double.IsNaN(yi))
                            {
                                trend.Add(new Point(xi + trendResolutionParams.Offset, yi));
                            }
                        }
                    }
                }

                if (trend.Count > 0)
                {
                    FlattenTrendLine(trend, trendResolutionParams, flattenedPoints, clipper);
                }
            }
        }

        public double RadiusExtentScale { get; set; }
        public double InnerRadiusExtentScale { get; set; }
        public Func<double, double, double> ProjectX { get; set; }
        public Func<double, double, double> ProjectY { get; set; }

        protected override void FlattenTrendLine(IList<Point> trend,
            TrendResolutionParams trendResolutionParams, List<Point> flattenedPoints, Clipper clipper)
        {
            if (clipper != null)
            {
                clipper.Target = flattenedPoints;
            }

            PolarLinePlanner planner = new PolarLinePlanner
            {
                AngleProvider = (i) => trend[i].X,
                RadiusProvider = (i) => trend[i].Y,
                Clipper = clipper,
                Count = trend.Count,
                Resolution = trendResolutionParams.Resolution,
                TransformedXProvider = (i) => ProjectX(trend[i].X, trend[i].Y),
                TransformedYProvider = (i) => ProjectY(trend[i].X, trend[i].Y),
                UseCartesianInterpolation = true,
                Viewport = trendResolutionParams.Viewport,
                Window = trendResolutionParams.Window
            };

            planner.PrepareLine();

        }
    }
//#endif

    internal class ScatterTrendLineManager
        : TrendLineManagerBase<Point>
    {
        public void PrepareLine(List<Point> flattenedPoints, TrendLineType trendLineType,
           IList<double> XColumn, IList<double> YColumn, int period,
           Func<double, double> GetScaledXValue,
           Func<double, double> GetScaledYValue,
           TrendResolutionParams trendResolutionParams, Clipper clipper, double min, double max)
        {
            double xmin = min;
            double xmax = max;
            List<Point> trend = new List<Point>();

            int count = 0;
            if (XColumn != null)
            {
                count = XColumn.Count;
            }
            if (YColumn != null)
            {
                count = Math.Min(count, YColumn.Count);
            }

            if (!trendResolutionParams.Window.IsEmpty && !trendResolutionParams.Viewport.IsEmpty)
            {
                if (trendLineType == TrendLineType.None)
                {
                    TrendCoefficients = null;
                    TrendColumn.Clear();
                }
                else if (IsFit(trendLineType))
                {
                    TrendColumn.Clear();

                    TrendCoefficients =
                        TrendFitCalculator.CalculateFit(trend, trendLineType, trendResolutionParams,
                        TrendCoefficients, count, (i) => XColumn[i], (i) => YColumn[i],
                        GetScaledXValue, GetScaledYValue, xmin, xmax);
                }
                else if (IsAverage(trendLineType))
                {
                    TrendCoefficients = null;
                    TrendColumn.Clear();

                    TrendAverageCalculator.CalculateXYAverage(trendLineType, TrendColumn, XColumn, YColumn, period);

                    // trend column is valid.

                    foreach (Point point in TrendColumn)
                    {
                        double xi = GetScaledXValue(point.X);
                        double yi = GetScaledYValue(point.Y);

                        if (!double.IsNaN(xi) && !double.IsNaN(yi))
                        {
                            trend.Add(new Point(xi, yi));
                        }
                    }
                }

                FlattenTrendLine(trend, trendResolutionParams, flattenedPoints, clipper);
            }
        }
    }

    internal abstract class CategoryTrendLineManagerBase
        : TrendLineManagerBase<double>, IPreparesCategoryTrendline
    {
        public abstract void PrepareLine(List<Point> flattenedPoints, TrendLineType trendLineType,
            IList<double> valueColumn, int period,
            Func<double, double> GetScaledXValue,
            Func<double, double> GetScaledYValue,
            TrendResolutionParams trendResolutionParams);

        internal static CategoryTrendLineManagerBase SelectManager(
            CategoryTrendLineManagerBase trendLineManager, 
            CategoryAxisBase xAxis, Canvas rootCanvas, Series series)
        {
            if (xAxis != null && xAxis is ISortingAxis)
            {
                if (trendLineManager != null)
                {
                    trendLineManager.Detach();
                }

                CategoryTrendLineManagerBase newManager =
                    new SortingTrendLineManager(
                        (x) =>
                        {
                            int sortedIndex = x;
                            ISortingAxis axis = xAxis as ISortingAxis;
                            if (axis != null)
                            {
                                x = Math.Min(x, axis.SortedIndices.Count - 1);
                                sortedIndex = axis.SortedIndices[x];
                            }
                            return axis.GetUnscaledValueAt(sortedIndex);
                        },
                        (x, viewport, window) =>
                        {
                            ScalerParams xParams = new ScalerParams(window, viewport, xAxis.IsInverted);
                            return xAxis.GetUnscaledValue(x, xParams);
                        });

                newManager.AttachPolyLine(rootCanvas, series);
                return newManager;
            }
            else if (!(trendLineManager is CategoryTrendLineManager))
            {
                if (trendLineManager != null)
                {
                    trendLineManager.Detach();
                }

                CategoryTrendLineManagerBase newManager =
                    new CategoryTrendLineManager();

                newManager.AttachPolyLine(rootCanvas, series);
                return newManager;
            }

            return trendLineManager;
        }
    }

    internal class CategoryTrendLineManager
        : CategoryTrendLineManagerBase
    {
        public override void PrepareLine(List<Point> flattenedPoints, TrendLineType trendLineType,
            IList<double> valueColumn, int period,
            Func<double, double> GetScaledXValue,
            Func<double, double> GetScaledYValue,
            TrendResolutionParams trendResolutionParams)
        {
            #region validate and bucketize TrendLine

            double xmin = trendResolutionParams.FirstBucket * trendResolutionParams.BucketSize;
            double xmax = trendResolutionParams.LastBucket * trendResolutionParams.BucketSize;
            List<Point> trend = new List<Point>();

            if (trendLineType == TrendLineType.None)
            {
                TrendCoefficients = null;
                TrendColumn.Clear();
                return;
            }

            if (IsFit(trendLineType))
            {
                TrendColumn.Clear();

                TrendCoefficients =
                    TrendFitCalculator.CalculateFit(trend, trendLineType, trendResolutionParams,
                    TrendCoefficients, valueColumn.Count, (i) => i + 1, (i) => valueColumn[i],
                    GetScaledXValue, GetScaledYValue, xmin, xmax);
            }

            if (IsAverage(trendLineType))
            {
                TrendCoefficients = null;

                TrendAverageCalculator.CalculateSingleValueAverage(trendLineType, TrendColumn, valueColumn, period);

                for (int i = trendResolutionParams.FirstBucket; i <= trendResolutionParams.LastBucket; i += 1)
                {
                    int itemIndex = i * trendResolutionParams.BucketSize;

                    if (itemIndex >= 0 && itemIndex < TrendColumn.Count)
                    {
                        double xi = GetScaledXValue(itemIndex);
                        double yi = GetScaledYValue(TrendColumn[itemIndex]);

                        trend.Add(new Point(xi + trendResolutionParams.Offset, yi));
                    }
                }
            }

            FlattenTrendLine(trend, trendResolutionParams, flattenedPoints);

            #endregion
        }

    }

    internal class SortingTrendLineManager
        : CategoryTrendLineManagerBase
    {
        public SortingTrendLineManager(Func<int, double> getUnscaledXValueFromUnsortedIndex,
            Func<double, Rect, Rect, double> getUnscaledXValue)
        {
            GetUnscaledValueFromUnsortedIndex = getUnscaledXValueFromUnsortedIndex;
            GetUnscaledXValue = getUnscaledXValue;
        }

        public Func<int, double> GetUnscaledValueFromUnsortedIndex { get; set; }
        public Func<double, Rect, Rect, double> GetUnscaledXValue { get; set; }
        //public Func<float> GetSinglePixelSpan { get; set; }

        public override void PrepareLine(List<Point> flattenedPoints, TrendLineType trendLineType,
            IList<double> valueColumn, int period,
            Func<double, double> GetScaledXValue,
            Func<double, double> GetScaledYValue,
            TrendResolutionParams trendResolutionParams)
        {
            #region validate and bucketize TrendLine

            double xmin = trendResolutionParams.FirstBucket * trendResolutionParams.BucketSize;
            double xmax = trendResolutionParams.LastBucket * trendResolutionParams.BucketSize;
            List<Point> trend = new List<Point>();

            if (trendLineType == TrendLineType.None)
            {
                TrendCoefficients = null;
                TrendColumn.Clear();
                return;
            }

            if (IsFit(trendLineType))
            {
                TrendColumn.Clear();

                TrendCoefficients =
                    TrendFitCalculator.CalculateFit(trend, trendLineType, trendResolutionParams,
                    TrendCoefficients, valueColumn.Count, 
                    (x) => x + 1, (i) => valueColumn[i],
                    (x) =>
                        {
                            int floor = (int)Math.Floor(x);
                            int ceil = (int)Math.Ceiling(x);
                            double p = x - floor;
                            double unscaled;

                            if (ceil <= xmax)
                            {
                                unscaled = GetUnscaledValueFromUnsortedIndex(floor) +
                                       p * (GetUnscaledValueFromUnsortedIndex(ceil) -
                                          GetUnscaledValueFromUnsortedIndex(floor));
                            }
                            else
                            {
                                unscaled =  GetUnscaledValueFromUnsortedIndex(floor) +
                                       p * (GetUnscaledValueFromUnsortedIndex((int)xmax) -
                                          GetUnscaledValueFromUnsortedIndex(floor));
                            }
                            return GetScaledXValue(unscaled);
                        },
                    GetScaledYValue, xmin, xmax);
            }

            if (IsAverage(trendLineType))
            {
                TrendCoefficients = null;

                TrendAverageCalculator.CalculateSingleValueAverage(trendLineType, TrendColumn, valueColumn, period);

                //float singlePixelSpan = GetSinglePixelSpan();

                for (int i = trendResolutionParams.FirstBucket; i <= trendResolutionParams.LastBucket; i += 1)
                {
                    int itemIndex = i * trendResolutionParams.BucketSize;
                    double unscaledX = GetUnscaledValueFromUnsortedIndex(itemIndex);

                    if (itemIndex >= 0 && itemIndex < TrendColumn.Count)
                    {
                        double xi = GetScaledXValue(unscaledX);
                        double yi = GetScaledYValue(TrendColumn[itemIndex]);

                        trend.Add(new Point(xi + trendResolutionParams.Offset, yi));
                    }
                }
            }

            FlattenTrendLine(trend, trendResolutionParams, flattenedPoints);

            #endregion
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