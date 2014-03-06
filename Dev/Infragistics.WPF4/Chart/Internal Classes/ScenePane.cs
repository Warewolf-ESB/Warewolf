using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using System.Globalization;

namespace Infragistics.Windows.Chart
{
    internal class ScenePane : ChartCanvas
    {

        #region Fields

        // Private fields
        private Chart _chart;
        private Size _finalSize;
        private PlottingPane3D _pane3D;
        private PlottingPane _pane2D;
        private bool _is3D = false;
        private AxisLabelsPane _labelsHorizontal1;
        private AxisLabelsPane _labelsVertical1;
        private AxisLabelsPane _labelsHorizontal2;
        private AxisLabelsPane _labelsVertical2;
        private SceneType _sceneType = 0;
        private GridArea _gridArea;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets Chart creator
        /// </summary>
        private ChartCreator ChartCreator
        {
            get { return _chart.ChartCreator; }
        }

        /// <summary>
        /// Gets primary X axis
        /// </summary>
        private AxisValue AxisX
        {
            get { return ChartCreator.AxisX; }
            set { ChartCreator.AxisX = value; }
        }

        /// <summary>
        /// Gets primary Y axis
        /// </summary>
        private AxisValue AxisY
        {
            get { return ChartCreator.AxisY; }
            set { ChartCreator.AxisY = value; }
        }

        /// <summary>
        /// Gets primary Z axis
        /// </summary>
        private AxisValue AxisZ
        {
            get { return ChartCreator.AxisZ; }
            set { ChartCreator.AxisZ = value; }
        }

        /// <summary>
        /// Gets secondary X axis
        /// </summary>
        private AxisValue AxisX2
        {
            get { return ChartCreator.AxisX2; }
            set { ChartCreator.AxisX2 = value; }
        }

        /// <summary>
        /// Gets secondary Y axis
        /// </summary>
        private AxisValue AxisY2
        {
            get { return ChartCreator.AxisY2; }
            set { ChartCreator.AxisY2 = value; }
        }

        /// <summary>
        /// Returns final size
        /// </summary>
        internal Size FinalSize
        {
            get { return _finalSize; }
            set { _finalSize = value; }
        }

        /// <summary>
        /// Gets the Canvas which is used to plot 3D charts
        /// </summary>
        internal PlottingPane3D PlottingPane3D
        {
            get { return _pane3D; }
        }

        /// <summary>
        /// Gets the Canvas which is used to plot 2D charts
        /// </summary>
        internal PlottingPane PlottingPane2D
        {
            get { return _pane2D; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="chart">The chart</param>
        /// <param name="is3D">True if the scene contains 3D charts</param>
        internal ScenePane(Chart chart, bool is3D)
        {
            _chart = chart;
            _is3D = is3D;
        }

        /// <summary>
        /// Gets the XamChart or the Chart
        /// </summary>
        /// <returns>The IChart Interface</returns>
        private IChart GetContainer()
        {
            return _chart.GetContainer();
        }

        /// <summary>
        /// When overridden in a derived class, participates in rendering operations 
        /// that are directed by the layout system. This method is invoked after layout 
        /// update, and before rendering, if the element's RenderSize has changed as a 
        /// result of layout update. 
        /// </summary>
        /// <param name="sizeInfo">The packaged parameters (SizeChangedInfo), which includes old and new sizes, and which dimension actually changes.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (!_is3D)
            {
                ChartCreator.Scene.ContentControlPropertyChanging = true;
                Draw();
                ChartCreator.Scene.ContentControlPropertyChanging = false;
            }

            base.OnRenderSizeChanged(sizeInfo);
        }

        private void CleanDataPointTemplates()
        {
            if (_chart == null || _chart.Series == null)
            {
                return;
            }

            foreach (Series series in _chart.Series)
            {
                foreach (DataPoint dp in series.DataPoints)
                {
                    dp.DataPointTemplate = null;
                }
            }
        }

        /// <summary>
        /// Repaints the chart and draws exception as the text if an exception is raised.
        /// </summary>
        public bool Draw()
        {
            CleanDataPointTemplates();

            try
            {
                DrawWithException();
            }
            catch (InvalidOperationException e)
            {
                _chart.InvalidDataReceived(e);

                return false;
            }
            catch (ArgumentException e)
            {
                _chart.InvalidDataReceived(e);

                return false;
            }

            return true;
        }

        internal void ClearTooltip()
        {
            // [DN 7/31/2008:BR35080] clear tooltip in case it has been set on the 3d pane...
            if (this._pane3D != null)
            {
                this._pane3D.ClearTooltip();
            }
        }

        /// <summary>
        /// Draws the chart without drawing exception
        /// </summary>
        private void DrawWithException()
        {
            // Create fake data if data points are not set.
            _chart.CreateFakeData();

            _is3D = _chart.GetContainer().View3D;
            _pane2D = null;
            _pane3D = null;
            if (_gridArea != null)
            {
                _gridArea.Content = null;
                _gridArea.PlottingPane2D = null;
                _gridArea.PlottingPane3D = null;

            }

            this.Children.Clear();

            // Draw 2D or 3D scene
            if (_is3D)
            {
                Create3DScene();
            }
            else
            {

                Create2DScene();
            }

            _chart.RemoveFakeData();

            // Raise After Render event
            XamChart control = XamChart.GetControl(_chart);
            if (control != null)
            {
                control.RaiseChartRendered();
            }
            if (this.PlottingPane2D != null && control.Crosshairs != null)
            {
                control.Crosshairs.PlottingPane = this.PlottingPane2D;
            }
        }

        /// <summary>
        /// Creates the scene for 2D charts
        /// </summary>
        private void Create2DScene()
        {
            ChartType[] chartTypes = _chart.GetChartTypes();

            List<List<Series>> groupedSeries = GetSeriesGroups();

            // Find Scene Type from series and chart types.
            FindSceneType(chartTypes);

            CheckIncompatibleChartTypes();

            // Create 2D plotting pane.
            _pane2D = new PlottingPane(_chart);

            _gridArea = ChartCreator.Scene.GridArea;
            _gridArea.SetValue(GridArea.IsPieScenePropertyKey, ChartCreator.IsSceneType(SceneType.Pie));
            if (_gridArea == null)
            {
                _gridArea = new GridArea();
            }
            if (_gridArea.MarginType == MarginType.Auto)
            {
                GridAreaAutoPosition();
            }
            else
            {
                Thickness margin = _gridArea.Margin;
                ChartCreator.CheckMargin(margin);
                _gridArea.RelativePosition = new Rect(margin.Left, margin.Top, 100 - margin.Left - margin.Right, 100 - margin.Top - margin.Bottom);
            }
            _gridArea.PlottingPane2D = _pane2D;
            _gridArea.Content = _pane2D;

            if (GetContainer().Scene != null && GetContainer().Scene.MarginType != MarginType.Auto)
            {
                Thickness margin = GetContainer().Scene.Margin;
                ChartCreator.CheckMargin(margin);
                RelativePosition = new Rect(margin.Left, margin.Top, 100 - margin.Left - margin.Right, 100 - margin.Top - margin.Bottom);
            }
            else
            {
                RelativePosition = new Rect(0, 0, 100, 100);
            }

            // Draw 2D chart without scene. Used for pie and doughnut chart.
            if (IsSceneType(SceneType.Pie))
            {
                // Create chart series
                ChartSeries chartSeries;
                for (int seriesIndex = 0; seriesIndex < chartTypes.Length; seriesIndex++)
                {
                    if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Scene, chartTypes[0], true))
                    {
                        continue;
                    }

                    chartSeries = CreateChartSeries(chartTypes[seriesIndex]);
                    chartSeries.Chart = _chart;
                    chartSeries.SeriesList = groupedSeries[0];

                    // Add chart series
                    _pane2D.ChartSeriesList.Add(chartSeries);
                }

                AddVisualChild(_gridArea);
            }
            else
            {
                AxisX = new AxisValue();
                AxisY = new AxisValue();
                AxisX2 = new AxisValue();
                AxisY2 = new AxisValue();

                // Create axis and axis labels
                CreateAxisLabels2D();

                // Create chart series
                ChartSeries chartSeries;
                for (int seriesGroupIndex = 0; seriesGroupIndex < groupedSeries.Count; seriesGroupIndex++)
                {
                    if (!ChartSeries.GetChartAttribute(ChartTypeAttribute.Scene, groupedSeries[seriesGroupIndex][0].ChartType, true))
                    {
                        continue;
                    }

                    // Create chart series
                    chartSeries = CreateChartSeries(groupedSeries[seriesGroupIndex][0].ChartType);
                    chartSeries.Chart = _chart;
                    GetSeriesAxis(groupedSeries[seriesGroupIndex][0], chartSeries);
                    chartSeries.SeriesList = groupedSeries[seriesGroupIndex];

                    // Add chart series
                    _pane2D.ChartSeriesList.Add(chartSeries);
                }

                _pane2D.ClipToBounds = true;
                _gridArea.Content = _pane2D;

                // Add labels
                AddAxisLabels(_labelsVertical1);
                AddAxisLabels(_labelsHorizontal1);
                AddAxisLabels(_labelsVertical2);
                AddAxisLabels(_labelsHorizontal2);

                AddVisualChild(_gridArea);
            }
        }

        /// <summary>
        /// xamChart can combine different chart types on the same scene, but some 
        /// chart types cannot be combined. This method checks if chart types which 
        /// belong to this scene can be combined.
        /// </summary>
        private void CheckIncompatibleChartTypes()
        {
            SeriesCollection seriesList = _chart.GetContainer().Series;

            bool scatter = false;
            bool bar = false;
            bool column = false;
            bool nonScene = false;
            bool stacked100 = false;
            bool nonStacked100 = false;

            bool pie = false;
            bool doughnut = false;

            foreach (Series series in seriesList)
            {
                if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Bar, series))
                {
                    bar = true;
                }
                else if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Scatter, series))
                {
                    scatter = true;
                }
                else if (!ChartSeries.GetChartAttribute(ChartTypeAttribute.Scene, series))
                {
                    nonScene = true;
                    if (series.ChartType == ChartType.Pie)
                    {
                        pie = true;
                    }
                    if (series.ChartType == ChartType.Doughnut)
                    {
                        doughnut = true;
                    }
                }
                else
                {
                    column = true;
                }

                if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Stacked100, series))
                {
                    stacked100 = true;
                }
                else
                {
                    nonStacked100 = true;
                }
            }

            if (scatter && (bar || column || nonScene))
            {
                // The scatter chart types cannot be used together with other non scatter chart types.
                throw new InvalidOperationException(ErrorString.Exc39);
            }

            if (nonScene && (scatter || column || bar))
            {
                // The pie and doughnut chart types cannot be used together with other chart types.
                throw new InvalidOperationException(ErrorString.Exc40);
            }

            if (bar && (scatter || column || nonScene))
            {
                // The bar chart types cannot be used together with other non bar chart types.
                throw new InvalidOperationException(ErrorString.Exc41);
            }

            if (stacked100 && nonStacked100)
            {
                // 100% stacked chart types cannot be used with other chart types.
                throw new InvalidOperationException(ErrorString.Exc65);
            }

            if (nonScene && pie && doughnut)
            {
                //The pie and doughnut chart types cannot be used combined between each other.
                throw new InvalidOperationException(ErrorString.Exc401);
            }

        }
        private static bool DoesChartTypeRequireGrouping(ChartType chartType)
        {
            switch (chartType)
            {
                case ChartType.Area:
                case ChartType.Candlestick:
                case ChartType.Doughnut:
                case ChartType.Line:
                case ChartType.Pie:
                case ChartType.Point:
                case ChartType.Scatter:
                case ChartType.ScatterLine:
                case ChartType.Spline:
                case ChartType.Stock:
                    return false;

#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

                default:
                    return true;
            }
        }
        private List<List<Series>> GetSeriesGroups()
        {
            List<List<Series>> seriesGroups = new List<List<Series>>();
            SeriesCollection seriesList = _chart.GetContainer().Series;

            foreach (Series series in seriesList)
            {
                List<Series> collection = GetSeriesGroupItem(seriesGroups, series);
                if (collection == null || !ScenePane.DoesChartTypeRequireGrouping(series.ChartType))
                {
                    List<Series> newCollection = new List<Series>();
                    newCollection.Add(series);
                    seriesGroups.Add(newCollection);
                }
                else
                {
                    collection.Add(series);
                }
            }

            return seriesGroups;
        }

        private List<Series> GetSeriesGroupItem(List<List<Series>> group, Series series)
        {
            foreach (List<Series> collection in group)
            {
                if (collection[0].ChartType == series.ChartType)
                {
                    if (string.Compare(collection[0].AxisX, series.AxisX, true, CultureInfo.InvariantCulture) == 0 && string.Compare(collection[0].AxisY, series.AxisY, true, CultureInfo.InvariantCulture) == 0)
                    {
                        return collection;
                    }
                }
            }

            return null;
        }

        private void GetSeriesAxis(Series series, ChartSeries chartSeries)
        {
            AxisCollection axes = _chart.GetContainer().Axes;
            if (!string.IsNullOrEmpty(series.AxisX) && axes.GetAxis(AxisType.SecondaryX) != null && !string.IsNullOrEmpty(axes.GetAxis(AxisType.SecondaryX).Name) && string.Compare(axes.GetAxis(AxisType.SecondaryX).Name, series.AxisX, true, CultureInfo.InvariantCulture) == 0)
            {
                chartSeries.AxisX = AxisX2;
            }
            else
            {
                chartSeries.AxisX = AxisX;
            }

            if (!string.IsNullOrEmpty(series.AxisY) && axes.GetAxis(AxisType.SecondaryY) != null && !string.IsNullOrEmpty(axes.GetAxis(AxisType.SecondaryY).Name) && string.Compare(axes.GetAxis(AxisType.SecondaryY).Name, series.AxisY, true, CultureInfo.InvariantCulture) == 0)
            {
                chartSeries.AxisY = AxisY2;
            }
            else
            {
                chartSeries.AxisY = AxisY;
            }
        }

        private void AddAxisLabels(AxisLabelsPane labels)
        {
            if (labels.AxisLabelsBase.Visible)
            {
                AddVisualChild(labels);
            }
        }

        private void GridAreaAutoPosition()
        {
            AxisCollection axisList = _chart.GetContainer().Axes;

            double labelWidth = 90;
            double labelLeft = 5;
            double labelHeight = 90;
            double labelTop = 5;

            if (!IsSceneType(SceneType.Bar))
            {
                if (axisList.IsAxisVisible(AxisType.PrimaryY))
                {
                    labelLeft += 10;
                    labelWidth -= 10;
                }

                if (axisList.IsAxisVisible(AxisType.SecondaryY))
                {
                    labelWidth -= 10;
                }

                if (axisList.IsAxisVisible(AxisType.PrimaryX))
                {
                    labelHeight -= 10;
                }

                if (axisList.IsAxisVisible(AxisType.SecondaryX))
                {
                    labelTop += 10;
                    labelHeight -= 10;
                }
            }
            else
            {
                if (axisList.IsAxisVisible(AxisType.PrimaryX))
                {
                    labelLeft += 10;
                    labelWidth -= 10;
                }

                if (axisList.IsAxisVisible(AxisType.SecondaryX))
                {
                    labelWidth -= 10;
                }

                if (axisList.IsAxisVisible(AxisType.PrimaryY))
                {
                    labelHeight -= 10;
                }

                if (axisList.IsAxisVisible(AxisType.SecondaryY))
                {
                    labelTop += 10;
                    labelHeight -= 10;
                }
            }
            _gridArea.RelativePosition = new Rect(labelLeft, labelTop, labelWidth, labelHeight);
        }

        /// <summary>
        /// Create axis and axis labels for 2D charts 
        /// </summary>
        private void CreateAxisLabels2D()
        {
            AxisCollection axisList = _chart.GetContainer().Axes;
            // ********************************************
            // Create Horizontal Bottom labels
            // ********************************************
            _labelsHorizontal1 = new AxisLabelsPane();
            AxisLabelsHorizontal axisLabelsHorizontal1 = new AxisLabelsHorizontal();
            _labelsHorizontal1.RelativePosition = new Rect(0, _gridArea.RelativePosition.Bottom, 100, 100 - _gridArea.RelativePosition.Bottom);
            axisLabelsHorizontal1.Shift = _gridArea.RelativePosition.Left;

            // ********************************************
            // Create Horizontal Top labels
            // ********************************************
            _labelsHorizontal2 = new AxisLabelsPane();
            AxisLabelsHorizontal axisLabelsHorizontal2 = new AxisLabelsHorizontal();
            _labelsHorizontal2.RelativePosition = new Rect(0, 0, 100, _gridArea.RelativePosition.Top);
            axisLabelsHorizontal2.Shift = _gridArea.RelativePosition.Left;

            // ********************************************
            // Create Vertical Left labels
            // ********************************************
            _labelsVertical1 = new AxisLabelsPane();
            AxisLabelsVertical axisLabelsVertical1 = new AxisLabelsVertical();
            _labelsVertical1.RelativePosition = new Rect(0, 0, _gridArea.RelativePosition.Left, 100);
            axisLabelsVertical1.Shift = _gridArea.RelativePosition.Top;

            // ********************************************
            // Create Vertical top labels
            // ********************************************
            _labelsVertical2 = new AxisLabelsPane();
            AxisLabelsVertical axisLabelsVertical2 = new AxisLabelsVertical();
            _labelsVertical2.RelativePosition = new Rect(_gridArea.RelativePosition.Right, 0, 100 - _gridArea.RelativePosition.Right, 100);
            axisLabelsVertical2.Shift = _gridArea.RelativePosition.Top;

            if (!IsSceneType(SceneType.Bar))
            {
                // Set X axis
                AxisX.PixelOrientationNegative = false;
                AxisX.SizeInPixels = GetAbsoluteX(_gridArea.RelativePosition.Width);

                // Set Y axis
                AxisY.PixelOrientationNegative = true;
                AxisY.SizeInPixels = GetAbsoluteY(_gridArea.RelativePosition.Height);

                // Set X2 axis
                AxisX2.PixelOrientationNegative = false;
                AxisX2.SizeInPixels = GetAbsoluteX(_gridArea.RelativePosition.Width);

                // Set Y2 axis
                AxisY2.PixelOrientationNegative = true;
                AxisY2.SizeInPixels = GetAbsoluteY(_gridArea.RelativePosition.Height);

                // Set Horizontal Bottom Axis label values
                axisLabelsHorizontal1.SetLabelStyle(_chart, AxisType.PrimaryX);
                axisLabelsHorizontal1.Axis = AxisX;
                axisLabelsHorizontal1.Visible = axisList.IsAxisVisible(AxisType.PrimaryX);

                // Set Vertical Left Axis label values
                axisLabelsVertical1.SetLabelStyle(_chart, AxisType.PrimaryY);
                axisLabelsVertical1.Axis = AxisY;
                axisLabelsVertical1.Visible = axisList.IsAxisVisible(AxisType.PrimaryY);

                // Set Horizontal Right Axis label values
                axisLabelsHorizontal2.SetLabelStyle(_chart, AxisType.SecondaryX);
                axisLabelsHorizontal2.Axis = AxisX2;
                axisLabelsHorizontal2.TopAxisLabels = true;
                axisLabelsHorizontal2.Visible = axisList.IsAxisVisible(AxisType.SecondaryX);

                // Set Vertical Top Axis label values
                axisLabelsVertical2.SetLabelStyle(_chart, AxisType.SecondaryY);
                axisLabelsVertical2.Axis = AxisY2;
                axisLabelsVertical2.Visible = axisList.IsAxisVisible(AxisType.SecondaryY);

            }
            else
            {
                // Set X axis
                AxisX.PixelOrientationNegative = true;
                AxisX.SizeInPixels = GetAbsoluteY(_gridArea.RelativePosition.Height);

                // Set Y axis
                AxisY.SizeInPixels = GetAbsoluteX(_gridArea.RelativePosition.Width);

                // Set X2 axis
                AxisX2.PixelOrientationNegative = true;
                AxisX2.SizeInPixels = GetAbsoluteY(_gridArea.RelativePosition.Height);

                // Set Y2 axis
                AxisY2.SizeInPixels = GetAbsoluteX(_gridArea.RelativePosition.Width);

                // Set Horizontal Axis label values
                axisLabelsHorizontal1.SetLabelStyle(_chart, AxisType.PrimaryY);
                axisLabelsHorizontal1.Axis = AxisY;
                axisLabelsHorizontal1.Visible = axisList.IsAxisVisible(AxisType.PrimaryY);

                // Set Vertical Axis label values
                axisLabelsVertical1.SetLabelStyle(_chart, AxisType.PrimaryX);
                axisLabelsVertical1.Axis = AxisX;
                axisLabelsVertical1.Visible = axisList.IsAxisVisible(AxisType.PrimaryX);

                // Set Horizontal Right Axis label values
                axisLabelsHorizontal2.SetLabelStyle(_chart, AxisType.SecondaryY);
                axisLabelsHorizontal2.Axis = AxisY2;
                axisLabelsHorizontal2.Visible = axisList.IsAxisVisible(AxisType.SecondaryY);

                // Set Vertical Axis label values
                axisLabelsVertical2.SetLabelStyle(_chart, AxisType.SecondaryX);
                axisLabelsVertical2.Axis = AxisX2;
                axisLabelsVertical2.Visible = axisList.IsAxisVisible(AxisType.SecondaryX);

            }

            // Set X axis
            if (IsSceneType(SceneType.Scatter))
            {
                AxisX.ZeroIncluded = false;
            }

            if (!IsSceneType(SceneType.Scatter))
            {
                AxisX.Indexed = true;
            }
            AxisX.Chart = _chart;
            AxisX.AxisType = AxisType.PrimaryX;
            if (axisList.GetAxis(AxisType.PrimaryX) != null)
            {
                AxisX.Calculate();
            }

            // Set X2 axis
            if (IsSceneType(SceneType.Scatter))
            {
                AxisX2.ZeroIncluded = false;
            }

            if (!IsSceneType(SceneType.Scatter))
            {
                AxisX2.Indexed = true;
            }
            AxisX2.Chart = _chart;
            AxisX2.AxisType = AxisType.SecondaryX;
            if (axisList.GetAxis(AxisType.SecondaryX) != null)
            {
                AxisX2.Calculate();
            }

            // Set Y axis
            if (axisList.GetAxis(AxisType.PrimaryY) != null && axisList.GetAxis(AxisType.PrimaryY).ReadLocalValue(Axis.RangeFromZeroProperty) != DependencyProperty.UnsetValue)
            {
                AxisY.ZeroIncluded = axisList.GetAxis(AxisType.PrimaryY).RangeFromZero;
            }

            if (IsSceneType(SceneType.Scatter))
            {
                AxisY.ZeroIncluded = false;
            }

            AxisY.Chart = _chart;
            AxisY.AxisType = AxisType.PrimaryY;
            if (axisList.GetAxis(AxisType.PrimaryY) != null)
            {
                AxisY.Calculate();
            }

            // Set Y2 axis
            if (axisList.GetAxis(AxisType.SecondaryY) != null && axisList.GetAxis(AxisType.SecondaryY).ReadLocalValue(Axis.RangeFromZeroProperty) != DependencyProperty.UnsetValue)
            {
                AxisY2.ZeroIncluded = axisList.GetAxis(AxisType.SecondaryY).RangeFromZero;
            }

            if (IsSceneType(SceneType.Scatter))
            {
                AxisY2.ZeroIncluded = false;
            }

            AxisY2.Chart = _chart;
            AxisY2.AxisType = AxisType.SecondaryY;
            if (axisList.GetAxis(AxisType.SecondaryY) != null)
            {
                AxisY2.Calculate();
            }


            // Set Horizontal Left Axis label values
            _labelsHorizontal1.Background = Brushes.Transparent;
            axisLabelsHorizontal1.TopAxisLabels = true;
            axisLabelsHorizontal1.MinimumFontSize = 6;
            axisLabelsHorizontal1.Chart = _chart;
            axisLabelsHorizontal1.FillLabels();
            _labelsHorizontal1.AxisLabelsBase = axisLabelsHorizontal1;

            // Set Vertical Bottom Axis label values
            _labelsVertical1.Background = Brushes.Transparent;
            axisLabelsVertical1.TopAxisLabels = true;
            axisLabelsVertical1.MinimumFontSize = 6;
            axisLabelsVertical1.Chart = _chart;
            axisLabelsVertical1.FillLabels();
            _labelsVertical1.AxisLabelsBase = axisLabelsVertical1;

            // Set Horizontal Right Axis label values
            _labelsHorizontal2.Background = Brushes.Transparent;
            axisLabelsHorizontal2.TopAxisLabels = false;
            axisLabelsHorizontal2.MinimumFontSize = 6;
            axisLabelsHorizontal2.Chart = _chart;
            axisLabelsHorizontal2.FillLabels();
            _labelsHorizontal2.AxisLabelsBase = axisLabelsHorizontal2;

            // Set Vertical Top Axis label values
            _labelsVertical2.Background = Brushes.Transparent;
            axisLabelsVertical2.LeftAxisLabels = true;
            axisLabelsVertical2.MinimumFontSize = 6;
            axisLabelsVertical2.Chart = _chart;
            axisLabelsVertical2.FillLabels();
            _labelsVertical2.AxisLabelsBase = axisLabelsVertical2;
        }

        private void Create3DScene()
        {
            ChartType[] chartTypes = _chart.GetChartTypes();

            // Find Scene Type from series and chart types.
            FindSceneType(chartTypes);

            CheckIncompatibleChartTypes();

            // Draw 3D chart without scene. Used for pie and doughnut chart.
            if (IsSceneType(SceneType.Pie))
            {
                // Create 3D plotting pane.
                _pane3D = new PlottingPane3D(_chart, this);

                //_pane3D.RelativePosition = new Rect(0, 0, 100, 100);
                CreatePane3DSize();

                if (GetContainer().Scene != null && GetContainer().Scene.GridArea != null)
                {
                    GetContainer().Scene.GridArea.PlottingPane3D = _pane3D;
                }

                // Series loop
                ChartSeries chartSeries;
                for (int seriesIndex = 0; seriesIndex < chartTypes.Length; seriesIndex++)
                {
                    // Create chart series
                    chartSeries = CreateChartSeries(chartTypes[seriesIndex]);
                    chartSeries.Chart = _chart;

                    // Draw only pie chart
                    if (!(chartSeries is PieChart3D || chartSeries is DoughnutChart3D))
                    {
                        continue;
                    }

                    // Set axes for chart series
                    chartSeries.AxisX = AxisX;
                    chartSeries.AxisY = AxisY;
                    chartSeries.AxisZ = AxisZ;
                    chartSeries.PlottingPane3D = _pane3D;

                    // Add chart series
                    _pane3D.AddChartSeries(chartSeries);
                }

                // Draw pane
                _pane3D.Draw();
                AddVisualChild(_pane3D);
            }
            else // Draw 3D charts which have 3D scene.
            {
                // Create 3D pane
                _pane3D = new PlottingPane3D(_chart, this);
                AxisX = new AxisValue();
                AxisY = new AxisValue();
                AxisZ = new AxisValue();
                AxisCollection axisList = _chart.GetContainer().Axes;
                _pane3D.RelativePosition = new Rect(0, 0, 100, 100);

                if (GetContainer().Scene != null && GetContainer().Scene.GridArea != null)
                {
                    GetContainer().Scene.GridArea.PlottingPane3D = _pane3D;
                }

                // Set X Axis values
                if (IsSceneType(SceneType.Scatter))
                {
                    AxisX.ZeroIncluded = false;
                }

                if (!IsSceneType(SceneType.Scatter))
                {
                    AxisX.Indexed = true;
                }
                AxisX.PixelOrientationNegative = false;
                AxisX.SizeInPixels = 1;
                AxisX.Is3D = true;
                AxisX.Chart = _chart;
                AxisX.AxisType = AxisType.PrimaryX;
                if (axisList.GetAxis(AxisType.PrimaryX) != null)
                {
                    AxisX.Calculate();
                }

                // Set Y Axis values
                if (axisList.GetAxis(AxisType.PrimaryY) != null && axisList.GetAxis(AxisType.PrimaryY).ReadLocalValue(Axis.RangeFromZeroProperty) != DependencyProperty.UnsetValue)
                {
                    AxisY.ZeroIncluded = axisList.GetAxis(AxisType.PrimaryY).RangeFromZero;
                }

                if (IsSceneType(SceneType.Scatter))
                {
                    AxisY.ZeroIncluded = false;
                }

                AxisY.SizeInPixels = 1;
                AxisY.Chart = _chart;
                AxisY.Is3D = true;
                AxisY.AxisType = AxisType.PrimaryY;
                if (axisList.GetAxis(AxisType.PrimaryY) != null)
                {
                    AxisY.Calculate();
                }

                // Set Z Axis values
                if (IsSceneType(SceneType.Scatter))
                {
                    AxisZ.ZeroIncluded = false;
                }


                if (!IsSceneType(SceneType.Scatter))
                {
                    AxisZ.IndexedSeries = true;
                }

                AxisZ.PixelOrientationNegative = false;
                AxisZ.SizeInPixels = 1;
                AxisZ.Chart = _chart;
                AxisZ.Is3D = true;
                AxisZ.AxisType = AxisType.PrimaryZ;
                if (axisList.GetAxis(AxisType.PrimaryZ) != null)
                {
                    AxisZ.Calculate();
                }

                // Set plotting pane 3D axes and grids
                _pane3D.AxisX = AxisX;
                _pane3D.AxisY = AxisY;
                _pane3D.AxisZ = AxisZ;

                ChartSeries chartSeries;

                // Series loop
                for (int seriesIndex = 0; seriesIndex < chartTypes.Length; seriesIndex++)
                {
                    // Create chart series
                    chartSeries = CreateChartSeries(chartTypes[seriesIndex]);
                    chartSeries.Chart = _chart;
                    chartSeries.AxisX = AxisX;
                    chartSeries.AxisY = AxisY;
                    chartSeries.AxisZ = AxisZ;
                    chartSeries.PlottingPane3D = _pane3D;

                    // Add chart series
                    _pane3D.AddChartSeries(chartSeries);
                }

                // Draw pane
                _pane3D.Draw();
                AddVisualChild(_pane3D);
            }
        }

        private void CreatePane3DSize()
        {
            if (_chart != null && !double.IsNaN(_chart.ActualWidth) && !double.IsNaN(_chart.ActualHeight) && _chart.ActualWidth > 0 && _chart.ActualHeight > 0)
            {
                double width = _chart.ActualWidth * 0.7;
                double ratio = width / _chart.ActualHeight;
                if (ratio > 1)
                {
                    double x = (100.0 - 100.0 / ratio) / 2.0;
                    double rectWidth = 99 - x * 2;

                    _pane3D.RelativePosition = new Rect(x, 0, rectWidth, 100);
                }
                else
                {
                    _pane3D.RelativePosition = new Rect(0, 0, 100, 100);
                }
            }
            else
            {
                _pane3D.RelativePosition = new Rect(0, 0, 100, 100);
            }
        }

        /// <summary>
        /// Find Scene Type from series and chart types.
        /// </summary>
        /// <param name="chartTypes">A list of chart types used on the scene</param>
        private void FindSceneType(ChartType[] chartTypes)
        {
            _sceneType = 0;

            // Checks if chart have a scene.
            if (chartTypes.Length > 0)
            {
                if (!ChartSeries.GetChartAttribute(ChartTypeAttribute.Scene, chartTypes[0], true))
                {
                    SetSceneType(SceneType.Pie, true);
                    return;
                }

                // Checks if scene type is Scatter.
                if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Stacked100, chartTypes[0], true))
                {
                    SetSceneType(SceneType.Stacked100, true);
                }

                // Checks if scene type is bar.
                if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Bar, chartTypes[0], true))
                {
                    SetSceneType(SceneType.Bar, true);
                    return;
                }

                // Checks if scene type is Scatter.
                if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Bubble, chartTypes[0], true))
                {
                    SetSceneType(SceneType.Scatter, true);
                    SetSceneType(SceneType.Bubble, true);
                    return;
                }

                // Checks if scene type is Scatter.
                if (ChartSeries.GetChartAttribute(ChartTypeAttribute.Scatter, chartTypes[0], true))
                {
                    SetSceneType(SceneType.Scatter, true);
                    return;
                }
            }
        }

        private void SetSceneType(SceneType type, bool value)
        {
            if (value)
            {
                _sceneType |= type;
            }
            else
            {
                ulong valLong = (ulong)type;
                valLong = 0xffffffffffffffff ^ valLong;
                _sceneType &= (SceneType)valLong;
            }
        }

        internal bool IsSceneType(SceneType type)
        {
            if ((_sceneType & type) == type)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Create chart series from chart types.
        /// </summary>
        /// <param name="type">Chart type</param>
        /// <returns>New chart series</returns>
        private ChartSeries CreateChartSeries(ChartType type)
        {
            return ChartSeries.CreateChartSeries(type, GetContainer().View3D);
        }

        /// <summary>
        /// Returns absolute position of the chart. By default 
        /// relative values are used from 0 to 100.
        /// </summary>
        /// <param name="value">Relative X position</param>
        /// <returns>Absolute X position</returns>
        internal double GetAbsoluteX(double value)
        {
            return value / 100 * _finalSize.Width;
        }

        /// <summary>
        /// Returns absolute position of the chart. By default 
        /// relative values are used from 0 to 100.
        /// </summary>
        /// <param name="value">Relative Y position</param>
        /// <returns>Absolute Y position</returns>
        internal double GetAbsoluteY(double value)
        {
            return value / 100 * _finalSize.Height;
        }

        /// <summary>
        /// Measures the child elements of a Canvas in anticipation of arranging 
        /// them during the ArrangeOverride pass. 
        /// </summary>
        /// <param name="availableSize">An upper limit Size that should not be exceeded.</param>
        /// <returns>A Size that represents the size that is required to arrange child content.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size childSize = availableSize;
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(childSize);
            }
            return availableSize;
        }

        /// <summary>
        /// Arranges the content of a Canvas element.
        /// </summary>
        /// <param name="arrangeSize">The size that this Canvas element should use to arrange its child elements.</param>
        /// <returns>A Size that represents the arranged size of this Canvas element and its descendants.</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            FinalSize = arrangeSize;
            foreach (UIElement child in InternalChildren)
            {
                if (child is ChartCanvas)
                {
                    // Find absolute positions
                    ChartCanvas plottingPane = child as ChartCanvas;
                    double x = plottingPane.RelativePosition.X / 100 * FinalSize.Width;
                    double y = plottingPane.RelativePosition.Y / 100 * FinalSize.Height;
                    double width = plottingPane.RelativePosition.Width / 100 * FinalSize.Width;
                    double height = plottingPane.RelativePosition.Height / 100 * FinalSize.Height;

                    child.Arrange(new Rect(x, y, width, height));
                    plottingPane.Width = width;
                    plottingPane.Height = height;
                }
                else if (child is ChartContentControl)
                {
                    // Find absolute positions
                    ChartContentControl chartContentControl = child as ChartContentControl;
                    double x = chartContentControl.RelativePosition.X / 100 * FinalSize.Width;
                    double y = chartContentControl.RelativePosition.Y / 100 * FinalSize.Height;
                    double width = chartContentControl.RelativePosition.Width / 100 * FinalSize.Width;
                    double height = chartContentControl.RelativePosition.Height / 100 * FinalSize.Height;

                    child.Arrange(new Rect(x, y, width, height));
                    chartContentControl.Width = width;
                    chartContentControl.Height = height;
                }
            }
            return arrangeSize; // Returns the final Arranged size
        }

        internal void AddVisualChild(UIElement child)
        {
            Chart.AddVisualChild(Children, child);
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