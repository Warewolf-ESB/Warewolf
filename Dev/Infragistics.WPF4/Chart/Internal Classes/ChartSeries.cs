
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Windows.Data;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Base class for all chart type series
    /// </summary>
    internal class ChartSeries
    {
        #region Fields

        // Private fields
        private Chart _chart;
        private AxisValue _axisX;
        private AxisValue _axisY;
        private AxisValue _axisZ;
        protected UIElementCollection _elements;
        private GdiGraphics _graphics = null;
        private Size _size;
        protected PlottingPane3D _plottingPane3D;
        private bool _zSeriesPositions;
        private List<Series> _seriesList;
        private double[,] _stackedData;

        #endregion Fields

        #region ChartTypeparameters

        /// <summary>
        /// Gets a Boolean value that specifies whether every data point of 
        /// the series have different color for this chart type.
        /// </summary>
        virtual internal bool IsColorFromPoint { get { return false; } }
        virtual internal bool IsStrokeMainColor { get { return false; } }
        virtual internal bool IsBar { get { return false; } }
        virtual internal bool IsStacked { get { return false; } }
        virtual internal bool IsScene { get { return true; } }
        virtual internal bool IsScatter { get { return false; } }
        virtual internal bool IsStacked100 { get { return false; } }
        virtual internal bool IsMarkerPoint { get { return false; } }
        virtual internal bool IsStock { get { return false; } }
        virtual internal bool IsBubble { get { return false; } }
        virtual internal bool IsClustered { get { return false; } }

        /// <summary>
        /// This method checks weather a chart type from specified series has a chart parameter set to true.
        /// </summary>
        /// <param name="parameter">Chart parameter</param>
        /// <param name="series">Data Series</param>
        /// <returns>True if chart type from specified series has a chart parameter set to true.</returns>
        static internal bool GetChartAttribute(ChartTypeAttribute parameter, Series series)
        {
            return GetChartAttribute(parameter, series.ChartType, series.GetIChart().View3D);
        }

        /// <summary>
        /// This method checks weather specified chart type has a chart parameter set to true.
        /// </summary>
        /// <param name="parameter">Chart parameter</param>
        /// <param name="type">Chart type</param>
        /// <param name="view3D">3D chart type</param>
        /// <returns>True if specified chart type has a chart parameter set to true.</returns>
        static internal bool GetChartAttribute(ChartTypeAttribute parameter, ChartType type, bool view3D)
        {
            ChartSeries chartSeries = CreateChartSeries(type, view3D);

            if (parameter == ChartTypeAttribute.ColorFromPoint)
            {
                return chartSeries.IsColorFromPoint;
            }
            else if (parameter == ChartTypeAttribute.StrokeMainColor)
            {
                return chartSeries.IsStrokeMainColor;
            }
            else if (parameter == ChartTypeAttribute.Bar)
            {
                return chartSeries.IsBar;
            }
            else if (parameter == ChartTypeAttribute.Stacked)
            {
                return chartSeries.IsStacked;
            }
            else if (parameter == ChartTypeAttribute.Scatter)
            {
                return chartSeries.IsScatter;
            }
            else if (parameter == ChartTypeAttribute.Scene)
            {
                return chartSeries.IsScene;
            }
            else if (parameter == ChartTypeAttribute.Stock)
            {
                return chartSeries.IsStock;
            }
            else if (parameter == ChartTypeAttribute.Stacked100)
            {
                return chartSeries.IsStacked100;
            }
            else if (parameter == ChartTypeAttribute.MarkerPoint)
            {
                return chartSeries.IsMarkerPoint;
            }
            else if (parameter == ChartTypeAttribute.Bubble)
            {
                return chartSeries.IsBubble;
            }
            else if (parameter == ChartTypeAttribute.Clustered)
            {
                return chartSeries.IsClustered;
            }

            return false;
        }

        #endregion ChartTypeparameters

        #region Properties

        /// <summary>
        /// Gets or sets Data Series
        /// </summary>
        internal List<Series> SeriesList
        {
            get { return _seriesList; }
            set { _seriesList = value; }
        }

        /// <summary>
        /// Gets or sets GDI Graphics
        /// </summary>
        protected GdiGraphics Graphics
        {
            get { return _graphics; }
            set { _graphics = value; }
        }

        /// <summary>
        /// Gets or sets Chart reference
        /// </summary>
        internal Chart Chart
        {
            get { return _chart; }
            set { _chart = value; }
        }

        /// <summary>
        /// Gets or sets PlottingPane3D reference
        /// </summary>
        internal PlottingPane3D PlottingPane3D
        {
            get { return _plottingPane3D; }
            set { _plottingPane3D = value; }
        }

        /// <summary>
        /// Gets or sets X axis reference
        /// </summary>
        internal AxisValue AxisX
        {
            get { return _axisX; }
            set { _axisX = value; }
        }

        /// <summary>
        /// Gets or sets Y Axis reference
        /// </summary>
        internal AxisValue AxisY
        {
            get { return _axisY; }
            set { _axisY = value; }
        }

        /// <summary>
        /// Gets or sets Z Axis reference
        /// </summary>
        internal AxisValue AxisZ
        {
            get { return _axisZ; }
            set { _axisZ = value; }
        }

        /// <summary>
        /// Gets or sets Plotting pane size
        /// </summary>
        internal Size Size
        {
            get { return _size; }
            set { _size = value; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// This method draws chart series using Shape UI Elements. Used for 2D charts
        /// </summary>
        /// <param name="elements">Element collection used to keep data point shapes</param>
        internal void Draw(UIElementCollection elements)
        {
            _elements = elements;
            Draw2D();
        }

        /// <summary>
        /// This method draws chart series using GDI Graphics. Used for fast performance
        /// </summary>
        /// <param name="graphics">Gdi Graphics object</param>
        internal void Draw(GdiGraphics graphics)
        {
            _graphics = graphics;
            Draw2D();
        }

        /// <summary>
        /// This method draws chart series using GDI Graphics. Used for fast performance
        /// </summary>
        /// <param name="graphics">Gdi Graphics object</param>
        /// <param name="elements">Element collection used to keep data point shapes</param>
        internal void Draw(GdiGraphics graphics, UIElementCollection elements)
        {
            _elements = elements;
            _graphics = graphics;
            Draw2D();
        }

        /// <summary>
        /// Draw data points using Drawing context or shape 
        /// UI elements. Also used for hit test functionality.
        /// </summary>
        protected virtual void Draw2D()
        {
        }

        /// <summary>
        /// Draw chart type series
        /// </summary>
        internal void Draw(Model3DGroup model3DGroup, RotateTransform3D rotateTransform3D)
        {
            Draw3D(model3DGroup, rotateTransform3D);
        }

        /// <summary>
        /// Draw chart type series
        /// </summary>
        protected virtual void Draw3D(Model3DGroup model3DGroup, RotateTransform3D rotateTransform3D)
        {
        }

        protected SeriesCollection GetSeries()
        {
            IChart chart = _chart.GetContainer();
            return chart.Series;
        }

        internal bool IsAnimationEnabled(object animation)
        {
            return IsAnimationEnabled() && animation != null;
        }

        protected bool IsAnimationEnabled()
        {
            return ChartSeries.IsAnimationEnabledInternal(this._chart);
        }
        internal static bool IsAnimationEnabledInternal(object animation, Chart chart)
        {
            return ChartSeries.IsAnimationEnabledInternal(chart) && animation != null;
        }
        internal static bool IsAnimationEnabledInternal(Chart chart)
        {
            return XamChart.GetControl(chart).IsAnimationEnabled;
        }
        /// <summary>
        /// Find max number of data points for stacked chart types
        /// </summary>
        /// <param name="type"></param>
        /// <param name="seriesList"></param>
        /// <returns></returns>
        protected int GetStackedNumberOfPoints(ChartType type, SeriesCollection seriesList)
        {
            List<Series> series = new List<Series>(seriesList);

            return GetStackedNumberOfPoints(type, series);
        }

        /// <summary>
        /// Find max number of data points for stacked chart types
        /// </summary>
        /// <param name="type"></param>
        /// <param name="seriesList"></param>
        /// <returns></returns>
        protected int GetStackedNumberOfPoints(ChartType type, List<Series> seriesList)
        {
            int numOfPoints = int.MinValue;
            foreach (Series series in seriesList)
            {
                if (series.ChartType == type)
                {
                    if (numOfPoints < series.DataPoints.Count)
                    {
                        numOfPoints = series.DataPoints.Count;
                    }
                }
            }

            if (numOfPoints == int.MinValue)
            {
                return 0;
            }

            return numOfPoints;
        }

        /// <summary>
        /// Search Chart type enumeration and find all chart types which 
        /// are stacked. Uses chart type parameters.
        /// </summary>
        /// <returns>Array of stacked chart types</returns>
        internal static ArrayList GetStackedChartTypes()
        {
            Array chartTypes = Enum.GetValues(typeof(ChartType));
            ArrayList stackedList = new ArrayList();

            // Chart types loop
            for (int index = 0; index < chartTypes.Length; index++)
            {
                if (GetChartAttribute(ChartTypeAttribute.Stacked, (ChartType)chartTypes.GetValue(index), true))
                {
                    stackedList.Add((ChartType)chartTypes.GetValue(index));
                }
            }

            return stackedList;
        }

        /// <summary>
        /// Calculates Z position for 3D charts. Stacked chart types are grouped together by Z value.
        /// </summary>
        /// <param name="index">The series index.</param>
        /// <param name="seriesList">Array of series.</param>
        /// <returns>The Z position of the series with specified index.</returns>
        protected int GetZSeriesPosition(int index, SeriesCollection seriesList)
        {
            // Find all stacked chart types
            ArrayList stackedTypes = GetStackedChartTypes();

            // Already calculated (buffer).
            if (_zSeriesPositions)
            {
                return seriesList[index].RealIndex + 1;
            }

            _zSeriesPositions = true;

            // The number of series which belongs to particular 
            // stacked chart type.
            int[] stackedIndexes = new int[stackedTypes.Count];

            for (int stackedIndex = 0; stackedIndex < stackedIndexes.Length; stackedIndex++)
            {
                stackedIndexes[stackedIndex] = -1;
            }

            int realIndex = -1;
            int seriesIndex = 0;

            // Series loop
            foreach (Series series in seriesList)
            {
                // Find the Z index for the first series which belongs to the group of stacked chart 
                // types, and set Z position to all series which belongs to this stacked chart type.
                for (int stackedIndex = 0; stackedIndex < stackedTypes.Count; stackedIndex++)
                {
                    if (stackedIndexes[stackedIndex] == -1 && series.ChartType == (ChartType)stackedTypes[stackedIndex])
                    {
                        realIndex++;
                        stackedIndexes[stackedIndex] = realIndex;
                    }
                }

                // The series is not staked.
                bool stacked = false;
                for (int stackedIndex = 0; stackedIndex < stackedTypes.Count; stackedIndex++)
                {
                    if (series.ChartType == (ChartType)stackedTypes[stackedIndex])
                    {
                        stacked = true;
                    }
                }

                if (!stacked)
                {
                    realIndex++;
                }

                // Set Z position for non stacked chart types.
                seriesList[seriesIndex].RealIndex = realIndex;

                // Set Z position for stacked chart types.
                for (int stackedIndex = 0; stackedIndex < stackedTypes.Count; stackedIndex++)
                {
                    if (series.ChartType == (ChartType)stackedTypes[stackedIndex])
                    {
                        seriesList[seriesIndex].RealIndex = stackedIndexes[stackedIndex];
                    }
                }

                seriesIndex++;
            }

            return seriesList[index].RealIndex + 1;
        }

        /// <summary>
        /// Create chart series from chart types.
        /// </summary>
        /// <param name="type">Chart type</param>
        /// <param name="view3D">2D or 3D charts</param>
        /// <returns>New chart series</returns>
        static internal ChartSeries CreateChartSeries(ChartType type, bool view3D)
        {
            ChartSeries chartSeries;

            // 2D Charts
            if (!view3D)
            {
                switch (type)
                {
                    case ChartType.Pie:
                        chartSeries = new PieChart2D();
                        break;
                    case ChartType.Line:
                        chartSeries = new LineChart2D();
                        break;
                    case ChartType.Area:
                        chartSeries = new AreaChart2D();
                        break;
                    case ChartType.Doughnut:
                        chartSeries = new DoughnutChart2D();
                        break;
                    case ChartType.Column:
                        chartSeries = new ColumnChart2D();
                        break;
                    case ChartType.Bar:
                        chartSeries = new BarChart2D();
                        break;
                    case ChartType.Bubble:
                        chartSeries = new BubbleChart2D();
                        break;
                    case ChartType.CylinderBar:
                        chartSeries = new BarChart2D();
                        break;
                    case ChartType.Cylinder:
                        chartSeries = new ColumnChart2D();
                        break;
                    case ChartType.StackedColumn:
                        chartSeries = new StackedColumnChart2D();
                        break;
                    case ChartType.StackedBar:
                        chartSeries = new StackedBarChart2D();
                        break;
                    case ChartType.StackedCylinderBar:
                        chartSeries = new StackedBarChart2D();
                        break;
                    case ChartType.StackedArea:
                        chartSeries = new StackedAreaChart2D();
                        break;
                    case ChartType.StackedCylinder:
                        chartSeries = new StackedColumnChart2D();
                        break;
                    case ChartType.ScatterLine:
                        chartSeries = new ScatterLineChart2D();
                        break;
                    case ChartType.Scatter:
                        chartSeries = new ScatterChart2D();
                        break;
                    case ChartType.Spline:
                        chartSeries = new SplineChart2D();
                        break;
                    case ChartType.Stacked100Column:
                        chartSeries = new Stacked100ColumnChart2D();
                        break;
                    case ChartType.Stacked100Cylinder:
                        chartSeries = new Stacked100ColumnChart2D();
                        break;
                    case ChartType.Stacked100Bar:
                        chartSeries = new Stacked100BarChart2D();
                        break;
                    case ChartType.Stacked100CylinderBar:
                        chartSeries = new Stacked100BarChart2D();
                        break;
                    case ChartType.Stacked100Area:
                        chartSeries = new Stacked100AreaChart2D();
                        break;
                    case ChartType.Stock:
                        chartSeries = new StockChart2D();
                        break;
                    case ChartType.Candlestick:
                        chartSeries = new CandlestickChart2D();
                        break;
                    case ChartType.Point:
                        chartSeries = new PointChart2D();
                        break;
                    default:
                        chartSeries = new ColumnChart2D();
                        break;
                }
            }
            else // 3D Charts
            {
                switch (type)
                {
                    case ChartType.Pie:
                        chartSeries = new PieChart3D();
                        break;
                    case ChartType.Doughnut:
                        chartSeries = new DoughnutChart3D();
                        break;
                    case ChartType.Line:
                        chartSeries = new LineChart3D();
                        break;
                    case ChartType.Area:
                        chartSeries = new AreaChart3D();
                        break;
                    case ChartType.Column:
                        chartSeries = new ColumnChart3D();
                        break;
                    case ChartType.Bubble:
                        chartSeries = new BubbleChart3D();
                        break;
                    case ChartType.Bar:
                        chartSeries = new BarChart3D();
                        break;
                    case ChartType.CylinderBar:
                        chartSeries = new CylinderBarChart3D();
                        break;
                    case ChartType.Cylinder:
                        chartSeries = new CylinderChart3D();
                        break;
                    case ChartType.StackedColumn:
                        chartSeries = new StackedColumnChart3D();
                        break;
                    case ChartType.StackedBar:
                        chartSeries = new StackedBarChart3D();
                        break;
                    case ChartType.StackedCylinder:
                        chartSeries = new StackedCylinderChart3D();
                        break;
                    case ChartType.StackedCylinderBar:
                        chartSeries = new StackedCylinderBarChart3D();
                        break;
                    case ChartType.StackedArea:
                        chartSeries = new StackedAreaChart3D();
                        break;
                    case ChartType.Spline:
                        chartSeries = new SplineChart3D();
                        break;
                    case ChartType.ScatterLine:
                        chartSeries = new ScatterLineChart3D();
                        break;
                    case ChartType.Scatter:
                        chartSeries = new ScatterChart3D();
                        break;
                    case ChartType.Stacked100Column:
                        chartSeries = new Stacked100ColumnChart3D();
                        break;
                    case ChartType.Stacked100Cylinder:
                        chartSeries = new Stacked100CylinderChart3D();
                        break;
                    case ChartType.Stacked100Bar:
                        chartSeries = new Stacked100BarChart3D();
                        break;
                    case ChartType.Stacked100CylinderBar:
                        chartSeries = new Stacked100CylinderBarChart3D();
                        break;
                    case ChartType.Stacked100Area:
                        chartSeries = new Stacked100AreaChart3D();
                        break;
                    case ChartType.Stock:
                        chartSeries = new StockChart3D();
                        break;
                    case ChartType.Candlestick:
                        chartSeries = new CandlestickChart3D();
                        break;
                    case ChartType.Point:
                        chartSeries = new PointChart3D();
                        break;
                    default:
                        chartSeries = new ColumnChart3D();
                        break;
                }
            }

            return chartSeries;
        }

        protected void SetHitTest3D(GeometryModel3D geometry, DataPoint point)
        {
            Series series = point.GetSeries();

            this.SetHitTest3D(geometry, this.FindSeriesIndex(series), series.DataPoints.IndexOf(point));
        }
        protected void SetHitTest2D(UIElement element, DataPoint point)
        {
            Series series = point.GetSeries();

            this.SetHitTest2D(element, this.FindSeriesIndex(series), series.DataPoints.IndexOf(point));
        }
        private void SetHitTest3D(GeometryModel3D geometry, int seriesIndex, int pointIndex)
        {
            HitTestInfo hitTestInfo = new HitTestInfo();
            hitTestInfo.GeometryModel3D = geometry;
            hitTestInfo.DataPointIndex = pointIndex;
            hitTestInfo.SeriesIndex = seriesIndex;
            _chart.HitTestInfoArray.Add(hitTestInfo);
        }

        /// <summary>
        /// Creates hit test info data.
        /// </summary>
        /// <param name="element">A shape which hit test data has to be set</param>
        /// <param name="seriesIndex">A data series index, parent of the data point</param>
        /// <param name="pointIndex">Data point index</param>
        private void SetHitTest2D(UIElement element, int seriesIndex, int pointIndex)
        {
            HitTestInfo hitTestInfo = new HitTestInfo();
            hitTestInfo.UIElement = element;
            hitTestInfo.DataPointIndex = pointIndex;
            hitTestInfo.SeriesIndex = seriesIndex;
            _chart.HitTestInfoArray.Add(hitTestInfo);
        }

        protected Brush GetBrush(DataPoint point, int seriesIndx, int pointIndx)
        {
            point.UpdateActualFill(seriesIndx, pointIndx);

            return point.ActualFill;
        }

        private int FindSeriesIndex(Series series)
        {
            int index = 0;
            foreach (Series ser in _chart.GetContainer().Series)
            {
                if (ser == series)
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        /// <summary>
        /// Set appearance parameters and event handlers for shapes.
        /// </summary>
        /// <param name="shape">A shape which appearance parameters has to be set.</param>
        /// <param name="point">A data point which is presented with the shape.</param>
        /// <param name="srsIndx">Series index from chart series group</param>
        /// <param name="pointIndx">Data point index</param>
        protected void SetShapeparameters(Shape shape, DataPoint point, int srsIndx, int pointIndx)
        {
            Series series = point.GetSeries();
            int seriesIndx = FindSeriesIndex(series);
            if (seriesIndx == -1)
            {
                seriesIndx = srsIndx;
            }
            shape.ToolTip = point.GetToolTip();

            point.UpdateActualFill(seriesIndx, pointIndx);
            shape.SetBinding(Shape.FillProperty, new Binding(DataPoint.ActualFillPropertyName) { Source = point });

            point.UpdateActualStroke(seriesIndx, pointIndx);
            shape.SetBinding(Shape.StrokeProperty, new Binding(DataPoint.ActualStrokePropertyName) { Source = point }); 

            shape.StrokeThickness = point.GetStrokeThickness(seriesIndx, pointIndx);

            Rectangle rectangle = shape as Rectangle;
            if (rectangle != null)
            {
                rectangle.RadiusX = point.GetParameterValueDouble(ChartParameterType.RectangleRounding);
                rectangle.RadiusY = point.GetParameterValueDouble(ChartParameterType.RectangleRounding);
            }

            #region Events
            #region DataPoint Events
            shape.MouseDown += new MouseButtonEventHandler(point.RaiseEvent);
            shape.MouseEnter += new MouseEventHandler(point.RaiseEvent);
            shape.MouseLeave += new MouseEventHandler(point.RaiseEvent);
            shape.MouseLeftButtonDown += new MouseButtonEventHandler(point.RaiseEvent);
            shape.MouseLeftButtonUp += new MouseButtonEventHandler(point.RaiseEvent);
            shape.MouseMove += new MouseEventHandler(point.RaiseEvent);
            shape.MouseRightButtonDown += new MouseButtonEventHandler(point.RaiseEvent);
            shape.MouseRightButtonUp += new MouseButtonEventHandler(point.RaiseEvent);
            shape.MouseUp += new MouseButtonEventHandler(point.RaiseEvent);
            shape.MouseWheel += new MouseWheelEventHandler(point.RaiseEvent);
            shape.ToolTipOpening += new ToolTipEventHandler(point.RaiseEvent);
            shape.ToolTipClosing += new ToolTipEventHandler(point.RaiseEvent);
            #endregion
            #region Series Events
            shape.MouseDown += new MouseButtonEventHandler(series.RaiseEvent);
            shape.MouseEnter += new MouseEventHandler(series.RaiseEvent);
            shape.MouseLeave += new MouseEventHandler(series.RaiseEvent);
            shape.MouseLeftButtonDown += new MouseButtonEventHandler(series.RaiseEvent);
            shape.MouseLeftButtonUp += new MouseButtonEventHandler(series.RaiseEvent);
            shape.MouseMove += new MouseEventHandler(series.RaiseEvent);
            shape.MouseRightButtonDown += new MouseButtonEventHandler(series.RaiseEvent);
            shape.MouseRightButtonUp += new MouseButtonEventHandler(series.RaiseEvent);
            shape.MouseUp += new MouseButtonEventHandler(series.RaiseEvent);
            shape.MouseWheel += new MouseWheelEventHandler(series.RaiseEvent);
            shape.ToolTipOpening += new ToolTipEventHandler(series.RaiseEvent);
            shape.ToolTipClosing += new ToolTipEventHandler(series.RaiseEvent);
            #endregion
            #endregion

        }

        /// <summary>
        /// Set event handlers for content.
        /// </summary>
        /// <param name="content">A content which appearance parameters has to be set.</param>
        /// <param name="point">A data point which is presented with the shape.</param>
        protected void SetContentEvents(ContentControl content, DataPoint point)
        {
            // Events
            content.MouseDown += new MouseButtonEventHandler(point.RaiseEvent);
            content.MouseEnter += new MouseEventHandler(point.RaiseEvent);
            content.MouseLeave += new MouseEventHandler(point.RaiseEvent);
            content.MouseLeftButtonDown += new MouseButtonEventHandler(point.RaiseEvent);
            content.MouseLeftButtonUp += new MouseButtonEventHandler(point.RaiseEvent);
            content.MouseMove += new MouseEventHandler(point.RaiseEvent);
            content.MouseRightButtonDown += new MouseButtonEventHandler(point.RaiseEvent);
            content.MouseRightButtonUp += new MouseButtonEventHandler(point.RaiseEvent);
            content.MouseUp += new MouseButtonEventHandler(point.RaiseEvent);
            content.MouseWheel += new MouseWheelEventHandler(point.RaiseEvent);
            content.ToolTipOpening += new ToolTipEventHandler(point.RaiseEvent);
            content.ToolTipClosing += new ToolTipEventHandler(point.RaiseEvent);
        }

        protected bool IsOutOfRange(double x, double y)
        {
            if (IsXOutOfRange(x) || IsYOutOfRange(y))
            {
                return true;
            }

            return false;
        }

        protected bool IsXOutOfRange(double x)
        {
            if (AxisX.Logarithmic)
            {
                x = Math.Log(x, AxisX.LogarithmicBase);
            }

            if (x < AxisX.RoundedMinimum)
            {
                return true;
            }

            if (x > AxisX.RoundedMaximum)
            {
                return true;
            }

            return false;
        }

        protected bool IsYOutOfRange(double y)
        {
            if (AxisY.Logarithmic)
            {
                y = Math.Log(y, AxisY.LogarithmicBase);
            }

            if (y < AxisY.RoundedMinimum)
            {
                return true;
            }

            if (y > AxisY.RoundedMaximum)
            {
                return true;
            }

            return false;
        }

        protected Point[] CreateStarPoints(int segments, double radius, double startAngle)
        {
            Point[] points = new Point[segments * 2];
            double angle = 2.0 * Math.PI / ((double)(segments));
            for (int index = 0; index < segments; index++)
            {
                double y = Math.Round(radius * Math.Cos(angle * index + Math.PI / 4.0 + startAngle), 5);
                double x = Math.Round(radius * Math.Sin(angle * index + Math.PI / 4.0 + startAngle), 5);
                points[index * 2] = new Point(x, y);
            }

            double shift = angle / 2.0;
            for (int index = 0; index < segments; index++)
            {
                double y = Math.Round(radius / 2 * Math.Cos(shift + angle * index + Math.PI / 4.0 + startAngle), 5);
                double x = Math.Round(radius / 2 * Math.Sin(shift + angle * index + Math.PI / 4.0 + startAngle), 5);
                points[index * 2 + 1] = new Point(x, y);
            }
            return points;
        }

        /// <summary>
        /// Create points for the profile of 3D Column (Top or bottom side). The profile could 
        /// be: Square, triangle, pentagon, octagon, etc.
        /// </summary>
        /// <param name="segments">The number of sides of the profile.</param>
        /// <param name="radius">The radius of the profile</param>
        /// <returns>Array of points in 2D space (Profile)</returns>
        protected Point[] CreatePoints(int segments, double radius)
        {
            Point[] points = new Point[segments];
            double angle = 2.0 * Math.PI / ((double)(segments));

            // Segments loop
            for (int index = 0; index < segments; index++)
            {
                // X and Y positions
                double y = Math.Round(radius * Math.Cos(angle * index + Math.PI / 4.0), 5);
                double x = Math.Round(radius * Math.Sin(angle * index + Math.PI / 4.0), 5);
                points[index] = new Point(x, y);
            }
            return points;
        }

        /// <summary>
        /// Create Label Brush
        /// </summary>
        /// <param name="point">Data Point as a source for label.</param>
        /// <returns>The drawing brush with a label</returns>
        protected DrawingBrush CreateLabelBrush(DataPoint point)
        {
            // Create a Drawing group and context for texture
            DrawingGroup drawing = new DrawingGroup();
            DrawingContext context = drawing.Open();

            FormattedText text = GetLabelStyle(point);

            // don't add shadow if the foreground is transparent
            Marker marker = GetMarker(point);
            if (marker == null || marker.Foreground != Brushes.Transparent)
            {
                FormattedText shadowText = GetLabelStyle(point);
                shadowText.SetForegroundBrush(Brushes.Black);

                context.DrawText(shadowText, new Point(3 / text.Width, 3 / text.Extent));
            }

            context.DrawText(text, new Point(0, 0));
            context.Close();

            // Create drawing brush with lables
            DrawingBrush brush = new DrawingBrush(drawing);

            return brush;
        }

        static protected Marker GetMarker(DataPoint point)
        {
            Marker marker = point.GetMarker();

            if (marker == null && ChartSeries.GetChartAttribute(ChartTypeAttribute.MarkerPoint, point.GetSeries()))
            {
                marker = new Marker();
                marker.Foreground = Brushes.Transparent;
            }
            if (marker != null)
            {
                marker.DataContext = point.DataContext;
            }

            return marker;
        }

        protected FormattedText GetLabelStyle(DataPoint point)
        {
            Marker marker = GetMarker(point);
            string textValue;

            textValue = MarkerSeries.CreateMarkerText(_chart.ChartCreator, point);

            CultureInfo cultureToUse = CultureInformation.CultureToUse;

            FormattedText formattedText = new FormattedText(textValue, cultureToUse, FlowDirection.LeftToRight, new Typeface("Tahoma"), 11, Brushes.Black);

            TextBlock textBlock = new TextBlock();
            _chart.Children.Add(textBlock);
            formattedText.SetFontFamily(textBlock.FontFamily);
            formattedText.SetFontSize(textBlock.FontSize);
            formattedText.SetFontStretch(textBlock.FontStretch);
            formattedText.SetFontWeight(textBlock.FontWeight);
            formattedText.SetForegroundBrush(textBlock.Foreground);
            _chart.Children.Remove(textBlock);

            formattedText.TextAlignment = TextAlignment.Center;

            // Take appearance from font properties
            if (marker != null)
            {
                if (marker.ReadLocalValue(Marker.FontFamilyProperty) != DependencyProperty.UnsetValue || Marker.FontFamilyProperty.DefaultMetadata.DefaultValue != marker.FontFamily)
                {
                    formattedText.SetFontFamily(marker.FontFamily);
                }

                if (marker.ReadLocalValue(Marker.FontSizeProperty) != DependencyProperty.UnsetValue || (double)Marker.FontSizeProperty.DefaultMetadata.DefaultValue != marker.FontSize)
                {
                    formattedText.SetFontSize(marker.FontSize);
                }

                if (marker.ReadLocalValue(Marker.FontStretchProperty) != DependencyProperty.UnsetValue || (FontStretch)Marker.FontStretchProperty.DefaultMetadata.DefaultValue != marker.FontStretch)
                {
                    formattedText.SetFontStretch(marker.FontStretch);
                }

                if (marker.ReadLocalValue(Marker.FontStyleProperty) != DependencyProperty.UnsetValue || (FontStyle)Marker.FontStyleProperty.DefaultMetadata.DefaultValue != marker.FontStyle)
                {
                    formattedText.SetFontStyle(marker.FontStyle);
                }

                if (marker.ReadLocalValue(Marker.FontWeightProperty) != DependencyProperty.UnsetValue || (FontWeight)Marker.FontWeightProperty.DefaultMetadata.DefaultValue != marker.FontWeight)
                {
                    formattedText.SetFontWeight(marker.FontWeight);
                }

                if (marker.ReadLocalValue(Marker.ForegroundProperty) != DependencyProperty.UnsetValue || Marker.ForegroundProperty.DefaultMetadata.DefaultValue != marker.Foreground)
                {
                    formattedText.SetForegroundBrush(marker.Foreground);
                }
            }

            return formattedText;
        }


        /// <summary>
        /// Calculates Max radius value from all data points.
        /// </summary>
        /// <returns>Maximum radius value</returns>
        protected double FindMaxRadius(List<Series> seriesList)
        {
            double maxRadius = double.MinValue;

            int seriesNum = seriesList.Count;

            // Data series loop
            for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
            {
                if (seriesList[seriesIndx].ChartType != ChartType.Bubble)
                {
                    continue;
                }

                // The number of data points
                int pointNum = seriesList[seriesIndx].DataPoints.Count;

                // Data points loop
                for (int pointIndx = 0; pointIndx < pointNum; pointIndx++)
                {
                    DataPoint point = seriesList[seriesIndx].DataPoints[pointIndx];
                    double radius = point.GetParameterValueDouble(ChartParameterType.Radius);
                    if (radius > maxRadius)
                    {
                        maxRadius = radius;
                    }
                }
            }

            return maxRadius;
        }

        /// <summary>
        /// Used to create cumulative values for stacked chart types
        /// </summary>
        protected void CreateStackedData()
        {
            int seriesNum = SeriesList.Count;
            if (seriesNum == 0)
            {
                return;
            }

            // Tha max number of data points for all series
            int maxNumOfPoints = GetStackedNumberOfPoints(SeriesList[0].ChartType, SeriesList);

            // The cumulative stacked numbers
            _stackedData = new double[seriesNum, maxNumOfPoints];

            // Data point loop
            for (int pointIndx = 0; pointIndx < maxNumOfPoints; pointIndx++)
            {
                // Sum of stacked data point values
                double stackedPositiveY = 0;
                double stackedNegativeY = 0;

                // Series loop
                for (int seriesIndx = 0; seriesIndx < seriesNum; seriesIndx++)
                {
                    if (!ChartSeries.GetChartAttribute(ChartTypeAttribute.Stacked, SeriesList[seriesIndx]))
                    {
                        continue;
                    }

                    // Skeep point index if less points then the index value
                    if (SeriesList[seriesIndx].DataPoints.Count <= pointIndx)
                    {
                        continue;
                    }

                    // Current data point value
                    DataPoint point = SeriesList[seriesIndx].DataPoints[pointIndx];

                    //Skip Null values
                    if (point.NullValue == true)
                    {
                        continue;
                    }

                    double val = point.Value;

                    if (val > 0)
                    {
                        stackedPositiveY += val;

                        // Cumulative data point value
                        _stackedData[seriesIndx, pointIndx] = stackedPositiveY;
                    }
                    else
                    {
                        stackedNegativeY += val;

                        // Cumulative data point value
                        _stackedData[seriesIndx, pointIndx] = stackedNegativeY;
                    }
                }
            }
        }

        /// <summary>
        /// This method checks if sum for 100% stacked chart is 0 to avoid division by zero.
        /// </summary>
        /// <param name="point">data point which contain the real value</param>
        /// <param name="sum">The sum of data point values</param>
        /// <returns>The relative value for 100% stacked chart which cannot be null.</returns>
        protected double GetStacked100Value(DataPoint point, double sum)
        {
            double value = 0;

            if (sum != 0)
            {
                value = point.Value / sum * 100;
            }

            return value;
        }

        /// <summary>
        /// Returns cumulative data point value.
        /// </summary>
        /// <param name="seriesIndex">Series index</param>
        /// <param name="pointIndex">Data point index</param>
        /// <returns>Stacked data point value</returns>
        protected double GetStackedValue(int seriesIndex, int pointIndex)
        {
            return _stackedData[seriesIndex, pointIndex];
        }

        /// <summary>
        /// This algorithm replaces a group of data points with one and increase performance. This 
        /// algorithm is activated only if there are more than 1000 data points in the series. 
        /// </summary>
        /// <param name="series">Series to be optimized</param>
        /// <param name="scatter">True if the chart types is scatter</param>
        /// <returns>Data points which are visible are set to true</returns>
        protected bool[] FastLine(Series series, bool scatter)
        {
            bool[] fastFlags = new bool[series.DataPoints.Count];

            // The number of data points
            int pointNum = series.DataPoints.Count;

            double[] x = new double[pointNum];
            double[] y = new double[pointNum];

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            double pixelCoeff = 100;

            // Creates array for X and Y values.
            for (int pointIndex = 0; pointIndex < pointNum; pointIndex++)
            {
                if (scatter)
                {
                    x[pointIndex] = series.DataPoints[pointIndex].GetParameterValueDouble(ChartParameterType.ValueX);
                    y[pointIndex] = series.DataPoints[pointIndex].GetParameterValueDouble(ChartParameterType.ValueY);
                }
                else
                {
                    x[pointIndex] = pointIndex + 1;
                    y[pointIndex] = series.DataPoints[pointIndex].Value;
                }

                if (minX > x[pointIndex])
                {
                    minX = x[pointIndex];
                }

                if (maxX < x[pointIndex])
                {
                    maxX = x[pointIndex];
                }

                if (minY > y[pointIndex])
                {
                    minY = y[pointIndex];
                }

                if (maxY < y[pointIndex])
                {
                    maxY = y[pointIndex];
                }
            }

            double distX = Math.Abs(maxX - minX) / pixelCoeff;
            double distY = Math.Abs(maxY - minY) / pixelCoeff;

            double cumX = 0;
            double cumY = 0;
            for (int pointIndex = 0; pointIndex < pointNum - 1; pointIndex++)
            {
                // Finds cumulative distance for small group of data points which are 
                // close to each other. Cumulative distance shouldn�t be longer than 
                // distX or distY if points are not visible.
                cumX += Math.Abs(x[pointIndex] - x[pointIndex + 1]);
                cumY += Math.Abs(y[pointIndex] - y[pointIndex + 1]);
                if (cumX > distX || cumY > distY)
                {
                    fastFlags[pointIndex] = true;
                    fastFlags[pointIndex + 1] = true;
                    cumX = 0;
                    cumY = 0;
                }

                // Finds a distance between two neighbouring data points. If the distance is longer 
                // than �distX/distY� these data points are visible and they cannot be hidden.
                if (Math.Abs(x[pointIndex] - x[pointIndex + 1]) > distX / 5 || Math.Abs(y[pointIndex] - y[pointIndex + 1]) > distY / 5)
                {
                    fastFlags[pointIndex] = true;
                    fastFlags[pointIndex + 1] = true;
                }
            }

            // The first and the last points always visible.
            fastFlags[0] = true;
            fastFlags[pointNum - 1] = true;

            return fastFlags;
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