
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using System.Collections;
using System.Resources;
using System.Reflection;
using System.Globalization;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Chart contains basic chart elements: Grids, Axes, Labels, Data Points, 
    /// Legends, Titles, etc. The Chart control can have many Charts 
    /// as a nested elements.
    /// </summary>
    /// <remarks>
    /// The xamChart control contains a collection of charts. Each individual chart has many of 
    /// the same properties as xamChart itself: Series, Axes, Legend, Caption, Lights, Transform3D, etc. 
    /// In addition, each individual chart contains its own chart collection. Because each chart contain 
    /// a collection of charts, you can create nested charts where each chart can have multiple parent 
    /// charts inside its boundaries. When you use a chart as a parent (that is, it has children inside the 
    /// chart collection), it doesn't display chart data; it is only used as a container of other charts.
    /// 
    /// Important - The nested chart functionality is disabled for the first release. Public properties from this class are not used.
    /// 
    /// </remarks>
    //[Microsoft.Windows.Design.ToolboxBrowsable(false)] - // This attribute need reference to C:\Program Files\Microsoft Expression\Blend 1.0\Microsoft.Windows.Design.dll
    internal class Chart : ChartContentControl, IChart
    {
        #region Fields

        private bool isLoaded = false;

        private Size _finalSize;
        private bool _nestedChart = true;
        private object _chartParent;
        private ArrayList _hitTestInfoArray = new ArrayList();
        private bool _fakeData;
        private ChartCreator _chartCreator;
        private ChartPane _chartPane;
        private Series _fakeSeries1;
        private Series _fakeSeries2;
        private bool _skipRefresh = false;
        private bool _updateDataSource = false;
        private bool _fastRenderingMode = false;

        protected bool _errorDisplaying = false;
        #endregion Fields

        #region Delegates

        protected delegate void DeferredError();

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets array used to keep pairs of Geometry model 3D or Shape and 
        /// corresponding data point object.
        /// </summary>
        internal ArrayList HitTestInfoArray
        {
            get { return _hitTestInfoArray; }
        }

        /// <summary>
        /// Returns Chart Creator
        /// </summary>
        internal ChartCreator ChartCreator
        {
            get { return _chartCreator; }
        }

        /// <summary>
        /// Gets collection of UIElements
        /// </summary>
        internal UIElementCollection Children
        {
            get
            {
                if (_chartPane != null)
                {
                    return _chartPane.Children;
                }
                else
                {
                    // If called before ChartPane is created.
                    ChartPane tempPane = new ChartPane(this);
                    return tempPane.Children;
                }
            }
        }

        /// <summary>
        /// Gets or sets value which indicates that data source should be updated.
        /// </summary>
        internal bool UpdateDataSource
        {
            get { return _updateDataSource; }
            set { _updateDataSource = value; }
        }

        /// <summary>
        /// Gets or sets fast rendering mode.
        /// </summary>
        internal bool FastRenderingMode
        {
            get { return _fastRenderingMode; }
            set { _fastRenderingMode = value; }
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
        /// True if this chart is nested into another chart or false if this 
        /// chart belongs to the chart control.
        /// </summary>
        internal bool NestedChart
        {
            get
            {
                return _nestedChart;
            }
            set
            {
                _nestedChart = value;
            }
        }

        /// <summary>
        /// The parent object
        /// </summary>
        internal object ChartParent
        {
            get
            {
                return _chartParent;
            }
            set
            {
                _chartParent = value;
            }
        }

        #endregion Internal Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the Chart class. 
        /// </summary>
        public Chart()
            : base()
        {
        }

        /// <summary>
        /// Raises the Initialized event. This method is invoked whenever IsInitialized is set to true internally. 
        /// </summary>
        /// <param name="e">Arguments of the event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            if (this._chartCreator == null)
            {
                this._chartCreator = new ChartCreator(this);
            }
            if (this._chartPane == null)
            {
                this._chartPane = new ChartPane(this);
            }
            this.Content = _chartPane;

            // [TT 12/06/2010:31139] - init the chart in the OnRenderSizeChanged, because 
            // in the reporting the Loaded event is never called
            // this.Loaded += new RoutedEventHandler(Chart_Loaded);
            // InitChart();

            base.OnInitialized(e);
        }

        /// <summary>
        /// When overriden in a class specifies the current logical children for that object.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                List<object> children = new List<object>();
                if (Content != null)
                {
                    children.Add(Content);
                }

                XamChart control = XamChart.GetControl(this);
                if (control != null && !_nestedChart)
                {
                    if (control.DefaultAxisX != null)
                    {
                        children.Add(control.DefaultAxisX);
                    }
                    if (control.DefaultAxisY != null)
                    {
                        children.Add(control.DefaultAxisY);
                    }
                    if (control.DefaultAxisZ != null)
                    {
                        children.Add(control.DefaultAxisZ);
                    }
                    var container = GetContainer();
                    if (container != null)
                    {
                        var caption = container.Caption;
                        if (caption != null)
                        {
                            children.Add(caption);
                        }
                        foreach (var child in container.Series)
                        {
                            children.Add(child);
                        }
                        foreach (var child in container.Axes)
                        {
                            children.Add(child);
                        }
                    }
                }

                return children.GetEnumerator();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.SizeChanged"/> event, using the specified information as part of the eventual event data.
        /// </summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            InitChart();
        }

        internal void CreateLogicalTree()
        {
            XamChart control = XamChart.GetControl(this);
            if (!control.LogicalTreeMode && !_nestedChart)
            {
                control.LogicalTreeMode = true;

                control.DefaultAxisY.AxisType = AxisType.PrimaryY;
                control.DefaultAxisZ.AxisType = AxisType.PrimaryZ;
                AddLogChild(control.DefaultAxisX);
                AddLogChild(control.DefaultAxisY);
                AddLogChild(control.DefaultAxisZ);
                control.DefaultAxisX.AddChildren();
                control.DefaultAxisY.AddChildren();
                control.DefaultAxisZ.AddChildren();

                AddLogChild(GetContainer().Caption);

                foreach (Series series in GetContainer().Series)
                {
                    this.AddLogChild(series);
                    series.AddChildren();
                }

                foreach (Axis axis in GetContainer().Axes)
                {
                    this.AddLogChild(axis);
                    axis.AddChildren();
                }

                control.LogicalTreeMode = false;
            }
        }

        /// <summary>
        /// Adds the provided element as a child of this element. This method has to be 
        /// used to avoid error which appears when the control template is edited 
        /// in design time in Blend.
        /// </summary>
        /// <param name="child">Child element to be added.</param>
        private void AddLogChild(object child)
        {
            ChartFrameworkContentElement element = child as ChartFrameworkContentElement;
            if (element != null)
            {
                if (element.Parent == null)
                {
                    this.AddLogicalChild(child);
                }
                else
                {
                    bool themeChanging = XamChart.GetControl(this).IsThemeChanging;
                    if (themeChanging)
                    {
                        DependencyObject parent = element.Parent;
                        Chart chart = parent as Chart;
                        if (chart != null)
                        {
                            chart.RemoveLogicalChild(element);
                            this.AddLogicalChild(child);
                        }
                    }
                }
            }
        }

        static internal void AddVisualChild(UIElementCollection children, UIElement child)
        {
            // ********************************************************
            // This code takes style from visual tree for Controls to 
            // avoid cyclic reference error.
            // ********************************************************
            if (child is Legend)
            {
                Legend legend = child as Legend;
                if (legend.ReadLocalValue(Legend.StyleProperty) == DependencyProperty.UnsetValue || legend.StyleSetFromControl)
                {
                    // Take style from visual tree.
                    Legend newLegend = new Legend();
                    children.Add(newLegend);
                    Style style = newLegend.Style;
                    legend.Style = style;
                    children.Remove(newLegend);
                    legend.StyleSetFromControl = true;
                }
            }

            if (child is Scene)
            {
                Scene scene = child as Scene;
                if (scene.ReadLocalValue(Scene.StyleProperty) == DependencyProperty.UnsetValue || scene.StyleSetFromControl)
                {
                    // Take style from visual tree.
                    Scene newScene = new Scene();
                    children.Add(newScene);
                    Style style = newScene.Style;
                    scene.Style = style;
                    children.Remove(newScene);
                    scene.StyleSetFromControl = true;

                }
            }

            if (child is GridArea)
            {
                GridArea gridArea = child as GridArea;
                if (gridArea.ReadLocalValue(GridArea.StyleProperty) == DependencyProperty.UnsetValue || gridArea.StyleSetFromControl)
                {
                    // Take style from visual tree.
                    GridArea newGridArea = new GridArea();
                    children.Add(newGridArea);
                    Style style = newGridArea.Style;
                    gridArea.Style = style;
                    children.Remove(newGridArea);
                    gridArea.StyleSetFromControl = true;
                }
            }

            if (child is FrameworkElement)
            {
                DependencyObject parent = ((FrameworkElement)child).Parent;
                if (parent != null)
                {
                    if (parent is ChartCanvas)
                    {
                        ChartCanvas chartPane = parent as ChartCanvas;

                        chartPane.Children.Remove(child);
                    }
                    else if (parent is ChartContentControl)
                    {
                        ChartContentControl chartContentControl = parent as ChartContentControl;
                        chartContentControl.Content = null;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            children.Add(child);
        }

        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamChart control = XamChart.GetControl(d);
            if (control != null && e.NewValue != e.OldValue)
            {
                control.RefreshProperty();
            }
        }

        internal void CreateFakeData()
        {
            if (XamChart.GetControl(this).Series.Count == 0)
            {
                _fakeData = true;
                _fakeSeries1 = new Series();
                AddFakePoint(_fakeSeries1, 4);
                AddFakePoint(_fakeSeries1, 7);
                AddFakePoint(_fakeSeries1, 15);
                AddFakePoint(_fakeSeries1, 6);

                _fakeSeries2 = new Series();
                AddFakePoint(_fakeSeries2, 8);
                AddFakePoint(_fakeSeries2, 5);
                AddFakePoint(_fakeSeries2, 12);
                AddFakePoint(_fakeSeries2, 8);

                XamChart.GetControl(this).Series.EnableRefresh = false;

                AddLogChild(_fakeSeries1);
                AddLogChild(_fakeSeries2);

                XamChart.GetControl(this).Series.Add(_fakeSeries1);
                XamChart.GetControl(this).Series.Add(_fakeSeries2);
                XamChart.GetControl(this).Series.EnableRefresh = true;
            }
        }

        private void AddFakePoint(Series series, double value)
        {
            DataPoint point = new DataPoint();
            point.Value = value;
            point.ChartParameters.Add(ChartParameterType.ValueX, value);
            point.ChartParameters.Add(ChartParameterType.ValueY, value);
            series.DataPoints.Add(point);
        }

        internal void RemoveFakeData()
        {
            if (_fakeData)
            {
                XamChart.GetControl(this).Series.EnableRefresh = false;
                XamChart.GetControl(this).Series.Clear();
                XamChart.GetControl(this).Series.EnableRefresh = true;
                _fakeData = false;
            }
        }

        /// <summary>
        /// Reconciles problems with the chart internal state that could have arisen
        /// due to an exception bubbling through the stack.
        /// </summary>
        private void ReconcileChartState()
        {
            //if an exception has bubbled past layers of the chart, the chart may
            //have been left in IsRefreshingMode
            XamChart containingChart = XamChart.GetControl(this);
            if (containingChart != null)
            {
                containingChart.IsRefreshingMode = false;
            }
        }

        private bool ErrorMessageHelper(Exception e, bool deferred)
        {
            this.Children.Clear();

            Border border = new Border();
            border.BorderBrush = Brushes.Red;
            border.Background = Brushes.White;
            border.CornerRadius = new CornerRadius(6);
            border.BorderThickness = new Thickness(1);
            ChartCanvas errorCanvas = new ChartCanvas();

            errorCanvas.Background = Brushes.Yellow;

            TextBlock error = new TextBlock();
            error.TextWrapping = TextWrapping.Wrap;
            error.Width = this.ActualWidth;
            error.Height = this.ActualHeight;
            error.Foreground = Brushes.Black;
            error.FontWeight = FontWeights.Bold;
            error.FontSize = 14;
            error.Background = Brushes.White;
            error.Text = ErrorString.Str1 + e.Message;
            border.Child = error;
            errorCanvas.Children.Add(border);
            this.Children.Add(errorCanvas);

            _errorDisplaying = false;

            if (deferred)
            {
                //if we are called deferred, it is the result of an exception
                //that has bubbled through some layers of the chart. 
                //it may have left the chart in an unwanted state.
                ReconcileChartState();
            }

            return true;
        }

        /// <summary>
        /// Defers a call to setup the error panel until after the current operations
        /// against the logical tree have concluded. Thus making sure we can properly display 
        /// the error message.
        /// </summary>
        /// <param name="e"></param>
        private void DeferErrorMessage(Exception e)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                           new DeferredError(delegate() { ErrorMessageHelper(e, true); }));
        }

        internal void ErrorMessage(Exception e)
        {
            //we have re-entered this method because an error has been reported while
            //we are trying to clear the children of the chart. Bubble this exception
            //because it will be caught by the outer call to this method.
            if (_errorDisplaying)
            {
                throw e;
            }

            try
            {
                //track re-entrance into this method.
                _errorDisplaying = true;
                ErrorMessageHelper(e, false);
                _errorDisplaying = false;
            }
            catch
            {
                //if we have any issue at all here, it could be because the logical
                //tree is not currently in a state that would allow for the 
                //display of the error message. If this is the case, enqueue
                //the error display on the dispatcher, to run after WPF is done
                //modifying the logical tree.
                DeferErrorMessage(e);
            }
        }

        /// <summary>
        /// Retrieve the value of the string resource by name. The resource manager 
        /// will retrieve the value of the localized resource using the caller's current 
        /// culture setting.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string GetRSString(string name)
        {
            ResourceManager rm = new ResourceManager("Infragistics.Windows.Chart.Properties.Resources", Assembly.GetExecutingAssembly());

            return rm.GetString(name);
        }

        internal bool IsErrorDrawingEnabled()
        {
            if (XamChart.GetControl(this) != null && XamChart.GetControl(this).DrawException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method called when invalid data is received.        
        /// </summary>
        internal void InvalidDataReceived(Exception exception)
        {
            XamChart chart = XamChart.GetControl(this);
            bool userHandled = false;

            if (chart != null)
            {
                userHandled = chart.RaiseInvalidDataReceived(exception);
            }

            if (!userHandled)
            {
                if (IsErrorDrawingEnabled())
                {
                    ErrorMessage(exception);
                    _skipRefresh = false;
                }
                else
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Repaints the chart
        /// </summary>
        public bool Refresh()
        {
            if (_skipRefresh)
            {
                return false;
            }

            try
            {
                RefreshWithException();
            }
            catch (InvalidOperationException e)
            {
                InvalidDataReceived(e);

                return false;
            }
            catch (ArgumentException e)
            {
                InvalidDataReceived(e);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Repaints the chart
        /// </summary>
        public void RefreshWithException()
        {
            XamChart.GetControl(this).IsRefreshingMode = true;

            if (UpdateDataSource)
            {
                BindData();
            }

            this.Children.Clear();

            // Raise Before Render event
            XamChart.GetControl(this).RaiseChartRendering();

            CreateLogicalTree();

            CreateFakeData();

            _chartCreator.RemoveChildren();
            HitTestInfoArray.Clear();

            // Nested charts do not exist.
            //if (XamChart.GetControl(this).Charts.Count == 0 || _nestedChart)
            {
                // Draws 2D or 3D charts
                if (!GetContainer().View3D)
                {
                    _chartCreator.Create2DChart();
                }
                else
                {
                    _chartCreator.Create3DChart();
                }
            }
            





            RemoveFakeData();

            // Disable focus in design mode for chart elements
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Rectangle topRectangle = new Rectangle();
                topRectangle.Fill = Brushes.Transparent;
                this.Children.Add(topRectangle);
            }
            if (UpdateDataSource)
            {
                UpdateDataSource = false;
            }
            XamChart.GetControl(this).IsRefreshingMode = false;
        }


        /// <summary>
        /// When implemented in a derived class, participates in rendering operations 
        /// that are directed by the layout system. The rendering instructions for this 
        /// element are not used directly when this method is invoked, and are instead 
        /// preserved for later asynchronous use by layout and drawing. 
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (_chartCreator.ScenePane != null)
            {
                _chartCreator.ScenePane.InvalidateVisual();
            }
        }



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


        private void InitChart()
        {
            // load the control only one
            if (this.isLoaded == false)
            {
                BindData();

                // Redraw the chart
                Refresh();

                this.isLoaded = true;
            }
        }

        /// <summary>
        /// Perform data binding to the series from this chart.
        /// </summary>
        internal void BindData()
        {
            _skipRefresh = true;
            SeriesCollection series = GetContainer().Series;
            for (int seriesIndx = 0; seriesIndx < series.Count; seriesIndx++)
            {
                series[seriesIndx].DataBind();
            }

            if (XamChart.GetControl(this) != null)
            {
                XamChart.GetControl(this).RaiseDataBind();
            }

            _skipRefresh = false;
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
        /// Creates an array of chart types from series collection.
        /// </summary>
        /// <returns>Array of chart types</returns>
        internal ChartType[] GetChartTypes()
        {
            SeriesCollection seriesCollection = GetContainer().Series;

            List<ChartType> chartTypes = new List<ChartType>();

            foreach (Series series in seriesCollection)
            {
                bool exist = false;
                foreach (ChartType type in chartTypes)
                {
                    if (type == series.ChartType)
                    {
                        exist = true;
                    }
                }

                if (!exist)
                {
                    chartTypes.Add(series.ChartType);
                }
            }

            ChartType[] types = new ChartType[chartTypes.Count];

            int index = 0;
            foreach (ChartType type in chartTypes)
            {
                types[index] = type;
                index++;
            }

            return types;
        }

        /// <summary>
        /// Create an array of data values from data points.
        /// </summary>
        /// <returns>Data values</returns>
        internal double[][] GetData()
        {
            SeriesCollection seriesCollection = GetContainer().Series;
            double[][] data = new double[seriesCollection.Count][];

            // Series loop
            for (int seriesIndx = 0; seriesIndx < seriesCollection.Count; seriesIndx++)
            {
                data[seriesIndx] = new double[seriesCollection[seriesIndx].DataPoints.Count];

                // Data points loop
                for (int pointIndx = 0; pointIndx < seriesCollection[seriesIndx].DataPoints.Count; pointIndx++)
                {
                    data[seriesIndx][pointIndx] = seriesCollection[seriesIndx].DataPoints[pointIndx].Value;
                }
            }

            return data;
        }

        /// <summary>
        /// This method gets an interface which contains public properties 
        /// for the chart. If the chart is the first parent properties 
        /// are taken from the chart control.
        /// </summary>
        /// <returns>Interface which contains public properties for the chart</returns>
        internal IChart GetContainer()
        {
            if (_nestedChart)
            {
                return this;
            }
            else
            {
                return XamChart.GetControl(this);
            }
        }

        
#region Infragistics Source Cleanup (Region)
























































#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Returns the topmost data point of a hit test.
        /// </summary>
        /// <param name="e">Provides data for Mouse related events.</param>
        /// <returns>Provides data with information about selected data point.</returns>
        public HitTestArgs HitTest(MouseEventArgs e)
        {
            Point position = new Point();
            if (!GetContainer().View3D)
            {

                if (_chartCreator != null && _chartCreator.Scene != null && _chartCreator.Scene.GridArea != null)
                {
                    position = e.GetPosition(_chartCreator.Scene.GridArea);
                }
                else if (_chartCreator != null && _chartCreator.ScenePane != null && _chartCreator.ScenePane.PlottingPane2D != null)
                {
                    position = e.GetPosition(_chartCreator.ScenePane.PlottingPane2D);
                }

            }
            else
            {
                position = e.GetPosition(_chartCreator.ScenePane.PlottingPane3D.Viewport3D);
            }

            return HitTest(position.X, position.Y);
        }

        /// <summary>
        /// Returns the topmost data point of a hit test.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// Provides data with information about selected data point.
        /// </returns>
        public HitTestArgs HitTest(double x, double y)
        {
            // Nested charts exist.
            
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


            if (!GetContainer().View3D)
            {
                HitTestArgs hitTestArgs = new HitTestArgs();
                foreach (HitTestInfo hitTestInfo in HitTestInfoArray)
                {
                    if (this.FastRenderingMode)
                    {
                        if (hitTestInfo.Path != null && GetContainer().Series.Count != 0)
                        {
                            if (hitTestInfo.Path.IsVisible((float)x, (float)y))
                            {
                                hitTestArgs.PointIndex = hitTestInfo.DataPointIndex;
                                hitTestArgs.SeriesIndex = hitTestInfo.SeriesIndex;
                                hitTestArgs.SelectedObject = GetContainer().Series[hitTestArgs.SeriesIndex].DataPoints[hitTestArgs.PointIndex];
                                hitTestArgs.Chart = this;
                            }
                        }
                    }
                    else
                    {
                        if (hitTestInfo.UIElement != null && hitTestInfo.UIElement.IsMouseOver && this.GetContainer().Series.Count != 0)
                        {
                            hitTestArgs.PointIndex = hitTestInfo.DataPointIndex;
                            hitTestArgs.SeriesIndex = hitTestInfo.SeriesIndex;
                            hitTestArgs.SelectedObject = GetContainer().Series[hitTestArgs.SeriesIndex].DataPoints[hitTestArgs.PointIndex];
                            hitTestArgs.Chart = this;
                        }
                    }
                }

                // Axis value position from pixel position.
                if (_chartCreator != null && _chartCreator.Scene != null && _chartCreator.Scene.GridArea != null)
                {
                    bool isBarChart = this.ChartCreator.ScenePane.IsSceneType(SceneType.Bar);

                    if (ChartCreator.AxisX != null)
                    {
                        if (isBarChart)
                        {
                            hitTestArgs.YValue = ChartCreator.AxisX.GetPixelValue(y);
                        }
                        else
                        {
                            hitTestArgs.XValue = ChartCreator.AxisX.GetPixelValue(x);
                        }
                    }

                    if (ChartCreator.AxisY != null)
                    {
                        if (isBarChart)
                        {
                            hitTestArgs.XValue = ChartCreator.AxisY.GetPixelValue(x);
                        }
                        else
                        {
                            hitTestArgs.YValue = ChartCreator.AxisY.GetPixelValue(y);
                        }
                    }

                    if (ChartCreator.AxisX2 != null)
                    {
                        if (isBarChart)
                        {
                            hitTestArgs.YValueSecondary = ChartCreator.AxisX2.GetPixelValue(y);
                        }
                        else
                        {
                            hitTestArgs.XValueSecondary = ChartCreator.AxisX2.GetPixelValue(x);
                        }
                    }

                    if (ChartCreator.AxisY2 != null)
                    {
                        if (isBarChart)
                        {
                            hitTestArgs.XValueSecondary = ChartCreator.AxisY2.GetPixelValue(x);
                        }
                        else
                        {
                            hitTestArgs.YValueSecondary = ChartCreator.AxisY2.GetPixelValue(y);
                        }
                    }
                }

                return hitTestArgs;
            }
            else
            {

                HitTestArgs hitTestArgs = new HitTestArgs();
                Point3D testpoint3D = new Point3D(x, y, 0);

                Vector3D testdirection = new Vector3D(x, y, 10);

                HitTestResult result = VisualTreeHelper.HitTest(_chartCreator.ScenePane.PlottingPane3D.Viewport3D, new Point(x, y));

                if (result != null)
                {
                    RayHitTestResult rayResult = result as RayHitTestResult;

                    foreach (HitTestInfo hitTestInfo in HitTestInfoArray)
                    {
                        if (hitTestInfo.GeometryModel3D == rayResult.ModelHit)
                        {
                            hitTestArgs.PointIndex = hitTestInfo.DataPointIndex;
                            hitTestArgs.SeriesIndex = hitTestInfo.SeriesIndex;
                            hitTestArgs.SelectedObject = GetContainer().Series[hitTestArgs.SeriesIndex].DataPoints[hitTestArgs.PointIndex];
                            hitTestArgs.Chart = this;
                        }
                    }
                }

                return hitTestArgs;
            }
        }

        #endregion Methods

        #region Public Properties

        #region Series

        private SeriesCollection _series = new SeriesCollection();

        /// <summary>
        /// The collection of data series which are used to provide data to the chart.
        /// </summary>
        /// <remarks>
        /// When a chart is deployed on the page, a default column chart appears with two series. 
        /// This default chart does not contain any data, but when the chart is placed into a visual 
        /// design surface, the chart automatically creates demo data to give it some appearance will 
        /// it is being worked with, or until you supply your data. As soon as you add actual data to 
        /// the control, the demo data disappears from the chart. You cannot change the sample data 
        /// because it does not exist in the Series collection. 
        /// </remarks>
        [Description("The collection of data series which are used to provide data to the chart.")]
        [Category("Chart Data")]
        public SeriesCollection Series
        {
            get
            {
                if (_series.ChartParent == null)
                {
                    _series.ChartParent = this;
                }
                return _series;
            }
        }

        #endregion Series

        #region Lights

        private LightCollection _lights = new LightCollection();

        /// <summary>
        /// Gets Collection with light objects that represent lighting applied to a 3-D scene.
        /// </summary>
        /// <remarks>
        /// For 3D charts, by default, a light effect is created with default values (one DirectionalLight object, 
        /// and one PointLight object). However, if you want to change the Light effect for the 3D scene, you need 
        /// to create a Light effect and add it to the Lights collection. When you add a Light effect to the collection, 
        /// the default Light effect disappears, and only newly created Light effects can be used. 
        /// </remarks>
        [Description("Gets Collection with light objects that represent lighting applied to a 3-D scene.")]
        [Category("Chart Data")]
        public LightCollection Lights
        {
            get
            {
                if (_lights.ChartParent == null)
                {
                    _lights.ChartParent = this;
                }
                return _lights;
            }
        }

        #endregion Lights

        #region Axes

        private AxisCollection _axes = new AxisCollection();

        /// <summary>
        /// Gets axis collection. Every axis has an AxisType which could be X, Y or Z.
        /// </summary>
        /// <remarks>
        /// By default, the chart's axis does not exist in the Axes collection; however, 
        /// internally, default axes are created. If you don't want to modify the appearance 
        /// or range of the axes, grid lines, or axis labels, you can simply use the default 
        /// values of the axes. If you want to change the the default axis values, you need 
        /// to create an axis and add it to the Axes collection.
        /// </remarks>
        [Description("Gets axis collection. Every axis has an AxisType which could be X, Y or Z.")]
        [Category("Chart Data")]
        public AxisCollection Axes
        {
            get
            {
                if (_axes.ChartParent == null)
                {
                    _axes.ChartParent = this;
                }
                return _axes;
            }
        }

        #endregion Axes

        #region Transform3D

        /// <summary>
        /// Identifies the <see cref="Transform3D"/> dependency property
        /// </summary>
        public static readonly DependencyProperty Transform3DProperty = DependencyProperty.Register("Transform3D",
            typeof(Transform3D), typeof(Chart), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets Transformation Matrix for 3D charts that provides all three-dimensional transformations, including translation, rotation, and scale transformations.
        /// </summary>
        /// <seealso cref="Transform3DProperty"/>
        [Description("Gets or sets Transformation Matrix for 3D charts that provides all three-dimensional transformations, including translation, rotation, and scale transformations.")]
        [Category("Chart Data")]
        public Transform3D Transform3D
        {
            get
            {
                return (Transform3D)this.GetValue(Chart.Transform3DProperty);
            }
            set
            {
                this.SetValue(Chart.Transform3DProperty, value);
            }
        }

        #endregion Transform3D

        #region View3D

        /// <summary>
        /// Identifies the <see cref="View3D"/> dependency property
        /// </summary>
        public static readonly DependencyProperty View3DProperty = DependencyProperty.Register("View3D",
            typeof(bool), typeof(Chart), new FrameworkPropertyMetadata((bool)false, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether the chart is rendered in 3D.
        /// </summary>
        /// <seealso cref="View3DProperty"/>
        [Description("Gets or sets a value that indicates whether the chart is rendered in 3D.")]
        [Category("Chart Data")]
        public bool View3D
        {
            get
            {
                return (bool)this.GetValue(Chart.View3DProperty);
            }
            set
            {
                this.SetValue(Chart.View3DProperty, value);
            }
        }


        #endregion View3D

        #region Caption

        /// <summary>
        /// Identifies the <see cref="Caption"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption",
            typeof(Caption), typeof(Chart), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the chart caption. Keeps information about text value, font, color and position for a Chart title.
        /// </summary>
        /// <seealso cref="CaptionProperty"/>
        [Description("Gets or sets the chart caption. Keeps information about text value, font, color and position for a Chart title.")]
        [Category("Chart Data")]
        public Caption Caption
        {
            get
            {
                Caption obj = (Caption)this.GetValue(Chart.CaptionProperty);
                if (obj != null)
                {
                    obj.ChartParent = this;
                }

                return obj;
            }
            set
            {
                this.SetValue(Chart.CaptionProperty, value);
            }
        }

        #endregion Caption

        #region Scene

        Scene _scene = new Scene();

        /// <summary>
        /// Identifies the <see cref="Scene"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SceneProperty = DependencyProperty.Register("Scene",
            typeof(Scene), typeof(Chart), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the scene which contains information related to chart scene appearance. The scene 
        /// is different for 2D and 3D chart. For 2D chart it is used for Background color 
        /// and position. For 3D chart it also containes thicknes of the 3D scene.
        /// </summary>
        /// <seealso cref="SceneProperty"/>
        [Description("Gets or sets the chart scene which contains information related to scene appearance. The chart scene is different for 2D and 3D chart. For 2D chart it is used for Background color and position. For 3D chart it also containes thicknes of the 3D scene.")]
        [Category("Chart Data")]
        public Scene Scene
        {
            get
            {
                Scene obj = (Scene)this.GetValue(Chart.SceneProperty);
                if (obj == null)
                {
                    obj = _scene;
                }
                obj.ChartParent = this;
                return obj;
            }
            set
            {
                this.SetValue(Chart.SceneProperty, value);
            }
        }

        #endregion Scene

        #region Legend

        Legend _legend = new Legend();

        /// <summary>
        /// Identifies the <see cref="Legend"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LegendProperty = DependencyProperty.Register("Legend",
            typeof(Legend), typeof(Chart), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the chart legend. The chart legend box appears alongside 
        /// the chart border. It is used to give text description for every data 
        /// point or series appearance in the chart. Many qualities of the legend 
        /// are dependent upon ChartType.
        /// </summary>
        /// <seealso cref="LegendProperty"/>
        [Description("Gets or sets the chart legend. The chart legend box appears alongside the chart border. It is used to give text description for every data point or series appearance in the chart. Many qualities of the legend are dependent upon ChartType.")]
        [Category("Chart Data")]
        public Legend Legend
        {
            get
            {
                Legend obj = (Legend)this.GetValue(Chart.LegendProperty);
                if (obj == null)
                {
                    obj = _legend;
                }
                obj.ChartParent = this;
                return obj;
            }
            set
            {
                this.SetValue(Chart.LegendProperty, value);
            }
        }

        #endregion Legend

        #region Position

        /// <summary>
        /// Identifies the <see cref="Position"/> dependency property
        /// </summary>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
            typeof(Rect), typeof(Chart), new FrameworkPropertyMetadata(Rect.Empty, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnPositionValidate));

        /// <summary>
        /// Gets or sets relative position of the chart, ranging in value from 0 to 100.
        /// </summary>
        /// <remarks>
        /// The property uses percentage values as the unit, which means that the values for the left, right, top, and bottom of the rectangle could be values between 0 and 100. This allows you to easily resize the chart and all elements inside the chart.
        /// </remarks>
        /// <seealso cref="PositionProperty"/>
        [Description("Gets or sets relative position of the chart, ranging in value from 0 to 100.")]
        [Category("Layout")]
        public Rect Position
        {
            get
            {
                return (Rect)this.GetValue(Chart.PositionProperty);
            }
            set
            {
                this.SetValue(Chart.PositionProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnPositionValidate(object value)
        {
            Rect newValue = (Rect)value;
            return (newValue.IsEmpty || newValue.X >= 0 && newValue.Y >= 0 && newValue.X <= 100 && newValue.Y <= 100 &&
                newValue.Width >= 0 && newValue.Height >= 0 && newValue.Width <= 100 && newValue.Height <= 100 &&
                newValue.X + newValue.Width <= 100 && newValue.Y + newValue.Height <= 100);

        }

        #endregion Position

        #endregion Public Properties

        #region Hidden Public Properties

        /// <summary>
        /// Gets or sets the maximum height constraint of the element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double MaxHeight
        {
            get
            {
                return base.MaxHeight;
            }
            set
            {
                base.MaxHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum width constraint of the element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double MaxWidth
        {
            get
            {
                return base.MaxWidth;
            }
            set
            {
                base.MaxWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum height constraint of the element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double MinHeight
        {
            get
            {
                return base.MinHeight;
            }
            set
            {
                base.MinHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum width constraint of the element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double MinWidth
        {
            get
            {
                return base.MinWidth;
            }
            set
            {
                base.MinWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment characteristics applied 
        /// to this element when it is composed within a parent element, 
        /// such as a panel or items control. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return base.HorizontalAlignment;
            }
            set
            {
                base.HorizontalAlignment = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment characteristics applied to 
        /// this element when it is composed within a parent element such as 
        /// a panel or items control. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new VerticalAlignment VerticalAlignment
        {
            get
            {
                return base.VerticalAlignment;
            }
            set
            {
                base.VerticalAlignment = value;
            }
        }

        #endregion Hidden Public Properties
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