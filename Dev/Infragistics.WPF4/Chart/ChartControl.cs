
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using System.Globalization;
using Infragistics.Windows.Themes;
using System.Windows.Media.Animation;

#endregion Using

namespace Infragistics.Windows.Chart
{
    #region DataBindEventArgs class and delegate

    /// <summary>
    /// Event arguments for routed event <see cref="XamChart.DataBind"/>
    /// </summary>
    /// <seealso cref="XamChart.DataBind"/>
    /// <seealso cref="XamChart.DataBindEvent"/>
    public class DataBindEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the DataBindEventArgs class. 
        /// </summary>
        public DataBindEventArgs()
        {
        }
    }

    #endregion DataBindEventArgs class and delegate

    #region ChartRenderingEventArgs class and delegate

    /// <summary>
    /// Event arguments for routed event <see cref="XamChart.ChartRendering"/>
    /// </summary>
    /// <seealso cref="XamChart.ChartRendering"/>
    /// <seealso cref="XamChart.ChartRenderingEvent"/>
    public class ChartRenderingEventArgs : RoutedEventArgs
    {

        /// <summary>
        /// Initializes a new instance of the ChartRenderingEventArgs class. 
        /// </summary>
        public ChartRenderingEventArgs()
        {
        }
    }

    #endregion ChartRenderingEventArgs class and delegate

    #region ChartRenderedEventArgs class and delegate

    /// <summary>
    /// Event arguments for routed event <see cref="XamChart.ChartRendered"/>
    /// </summary>
    /// <seealso cref="XamChart.ChartRendered"/>
    /// <seealso cref="XamChart.ChartRenderedEvent"/>
    public class ChartRenderedEventArgs : RoutedEventArgs
    {

        /// <summary>
        /// Initializes a new instance of the ChartRenderedEventArgs class. 
        /// </summary>
        public ChartRenderedEventArgs()
        {
        }
    }

    #endregion ChartRenderedEventArgs class and delegate

    #region InvalidDataEventArgs

    /// <summary>
    /// Provides event arguments for the InvalidDataReceived routed event that is raised when an invalid data is bound to the XamChart.
    /// </summary>
    /// <seealso cref="XamChart.InvalidDataReceived"/>
    /// <seealso cref="XamChart.InvalidDataReceivedEvent"/>
    public class InvalidDataEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the invalid data exception
        /// </summary>
        public Exception Exception { get; private set; }

        ///<summary>
        /// Gets or sets a value that indicates wheather the invalid data exception has been handled. Set to true 
        /// if the invalid data exception is to be marked handled; otherwise false. The default value is false.       
        ///</summary>
        public bool ExceptionHandled { get; set; }

        /// <summary>
        /// Initializes a new instance of the InvalidDataEventArgs class, using the supplied invalid data exception.
        /// </summary>
        /// <param name="exception">The invalid data exception</param>
        public InvalidDataEventArgs(Exception exception)
        {   
            Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the InvalidDataEventArgs class, using the supplied invalid data exception and routed event identifier.
        /// </summary>
        /// <param name="exception">The invalid data exception</param>
        /// <param name="routedEvent">The routed event identifier for this instance of the InvalidDataEventArgs class.</param>
        public InvalidDataEventArgs(Exception exception, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the InvalidDataEventArgs class, using the supplied invalid data exception, routed event identifier, and providing the opportunity to declare a different source for the event.
        /// </summary>
        /// <param name="exception">The invalid data exception</param>
        /// <param name="routedEvent">The routed event identifier for this instance of the InvalidDataEventArgs class.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the InvalidDataEventArgs.Source property.</param>
        public InvalidDataEventArgs(Exception exception, RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            Exception = exception;
        }
    }

    #endregion InvalidDataEventArgs

    /// <summary>
    /// The xamChart control provides a framework for creating charting applications, or adding charting 
    /// capability to existing data or information-driven applications.
    /// </summary>
    /// <remarks>
    /// The xamChart control contains a collection of charts. Each individual chart has many of the same 
    /// properties as xamChart itself: Series, Axes, Legend, Caption, Lights, Transform3D, etc. In addition, 
    /// each individual chart contains its own chart collection. Because each chart contain a collection of 
    /// charts, you can create nested charts where each chart can have multiple parent charts inside its 
    /// boundaries. When you use a chart as a parent (that is, it has children inside the chart collection), 
    /// it doesn't display chart data; it is only used as a container of other charts. 
    /// </remarks>
    [TemplatePart(Name = "PART_Default_Chart", Type = typeof(Chart))]
    //[Description("The chart control.")]
		// AS 11/7/07 BR21903
		// AS 11/7/07 BR21903
    public class XamChart : Control, IChart
    {
        #region Fields

        // Private Fields
        private Chart _defaultChart;
        private Color[] _randomColors;
        private Random _random = new Random(1);
        private bool _logicalTreeMode;
        private Axis _defaultAxisX = new Axis();
        private Axis _defaultAxisY = new Axis();
        private Axis _defaultAxisZ = new Axis();
        private bool _isAnimationEnabled = true;
        private bool _isThemeChanging = false;
        private bool _isRefreshingMode = false;
        private bool _contentControlPropertyChanging = false;

        // AS 11/7/07 BR21903
        private Infragistics.Windows.Licensing.UltraLicense _license;

        #endregion Fields

        #region Properties

        /// <summary>
        /// On property changed called and active
        /// </summary>
        internal bool ContentControlPropertyChanging
        {
            get
            {
                return _contentControlPropertyChanging;
            }
            set
            {
                _contentControlPropertyChanging = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the themes are changing.
        /// </summary>
        internal bool IsThemeChanging
        {
            get
            {
                return _isThemeChanging;
            }
            set
            {
                _isThemeChanging = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether refreshing is in progress.
        /// </summary>
        internal bool IsRefreshingMode
        {
            get
            {
                return _isRefreshingMode;
            }
            set
            {
                _isRefreshingMode = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the animation is enabled.
        /// </summary>
        internal bool IsAnimationEnabled
        {
            get
            {
                return _isAnimationEnabled;
            }
            set
            {
                _isAnimationEnabled = value;
            }
        }

        internal bool IsCrosshairsSupported()
        {
            if (this.View3D)
            {
                return false;
            }

            if (this.Series.Count == 0)
            {
                return false;
            }

            if (this.Series[0].ChartType == ChartType.Pie ||
                this.Series[0].ChartType == ChartType.Doughnut)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deafult Chart
        /// </summary>
        internal Chart DefaultChart
        {
            get
            {
                return _defaultChart;
            }
            set
            {
                _defaultChart = value;
            }
        }

        /// <summary>
        /// Deafult axis X
        /// </summary>
        internal Axis DefaultAxisX
        {
            get
            {
                return _defaultAxisX;
            }
            set
            {
                _defaultAxisX = value;
            }
        }

        /// <summary>
        /// Deafult axis Y
        /// </summary>
        internal Axis DefaultAxisY
        {
            get
            {
                return _defaultAxisY;
            }
            set
            {
                _defaultAxisY = value;
            }
        }

        /// <summary>
        /// Deafult axis Z
        /// </summary>
        internal Axis DefaultAxisZ
        {
            get
            {
                return _defaultAxisZ;
            }
            set
            {
                _defaultAxisZ = value;
            }
        }

        /// <summary>
        /// When a UIElement is added to the logical tree, properties of the UIElement 
        /// are set from the tree. Every time when a property is set the refresh method 
        /// is called. This property is used to prevent refreshing when UIElements are 
        /// added to the visual tree.
        /// </summary>
        internal bool LogicalTreeMode
        {
            get
            {
                return _logicalTreeMode;
            }
            set
            {
                _logicalTreeMode = value;
            }
        }

        #endregion Properties

        #region Events

        #region DataBind

        /// <summary>
        /// Event ID for the <see cref="DataBind"/> routed event
        /// </summary>
        /// <seealso cref="DataBind"/>
        /// <seealso cref="OnDataBind"/>
        /// <seealso cref="DataBindEventArgs"/>
        public static readonly RoutedEvent DataBindEvent =
            EventManager.RegisterRoutedEvent("DataBind", RoutingStrategy.Bubble, typeof(EventHandler<DataBindEventArgs>), typeof(XamChart));

        /// <summary>
        /// Occurs when the chart is about to data bind series to the data source.
        /// </summary>
        /// <seealso cref="DataBind"/>
        /// <seealso cref="DataBindEvent"/>
        /// <seealso cref="DataBindEventArgs"/>
        protected virtual void OnDataBind(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }


        /// <summary>
        /// Raise Data Bind event
        /// </summary>
        internal void RaiseDataBind()
        {
            DataBindEventArgs args = new DataBindEventArgs();
            args.RoutedEvent = XamChart.DataBindEvent;
            args.Source = this;
            this.OnDataBind(args);
        }

        /// <summary>
        /// This event occurs immediately after data binding. This event is used to change color or appearance of data points after binding.
        /// </summary>
        /// <remarks>
        /// When data binding is used a new series without data points has to be created. Data binding process will create data points and set 
        /// Value property from data points using values from the data source.  To change color or other properties of data points we have 
        /// to use data bind event after data binding to specify new values. DataBind event occurs only ones after binding, for all series 
        /// from the chart.
        /// </remarks>
        /// <seealso cref="OnDataBind"/>
        /// <seealso cref="DataBindEvent"/>
        /// <seealso cref="DataBindEventArgs"/>
        //[Description("This event occurs immediately after data binding. This event is used to change color or appearance of data points after binding.")]
        //[Category("Behavior")]
        public event EventHandler<DataBindEventArgs> DataBind
        {
            add
            {
                base.AddHandler(XamChart.DataBindEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamChart.DataBindEvent, value);
            }
        }

        #endregion DataBind

        #region ChartRendering

        /// <summary>
        /// Event ID for the <see cref="ChartRendering"/> routed event
        /// </summary>
        /// <seealso cref="ChartRendering"/>
        /// <seealso cref="OnChartRendering"/>
        public static readonly RoutedEvent ChartRenderingEvent =
            EventManager.RegisterRoutedEvent("ChartRendering", RoutingStrategy.Bubble, typeof(EventHandler<ChartRenderingEventArgs>), typeof(XamChart));

        /// <summary>
        /// Occurs before the chart is about to be rendered.
        /// </summary>
        /// <seealso cref="ChartRendering"/>
        /// <seealso cref="ChartRenderingEvent"/>
        protected virtual void OnChartRendering(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Raise Chart rendering event.
        /// </summary>
        internal void RaiseChartRendering()
        {
            ChartRenderingEventArgs args = new ChartRenderingEventArgs();
            args.RoutedEvent = XamChart.ChartRenderingEvent;
            args.Source = this;
            this.OnChartRendering(args);
        }

        /// <summary>
        /// This event occurs before the chart is rendered.
        /// </summary>
        /// <seealso cref="OnChartRendering"/>
        /// <seealso cref="ChartRenderingEvent"/>
        /// <seealso cref="ChartRenderingEventArgs"/>
        //[Description("This event occurs before the chart is rendered.")]
        //[Category("Behavior")]
        public event EventHandler<ChartRenderingEventArgs> ChartRendering
        {
            add
            {
                base.AddHandler(XamChart.ChartRenderingEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamChart.ChartRenderingEvent, value);
            }
        }

        #endregion ChartRendering

        #region ChartRendered

        /// <summary>
        /// Event ID for the <see cref="ChartRendered"/> routed event
        /// </summary>
        /// <seealso cref="ChartRendered"/>
        /// <seealso cref="OnChartRendered"/>
        public static readonly RoutedEvent ChartRenderedEvent =
            EventManager.RegisterRoutedEvent("ChartRendered", RoutingStrategy.Bubble, typeof(EventHandler<ChartRenderedEventArgs>), typeof(XamChart));

        /// <summary>
        /// Occurs after the chart is rendered.
        /// </summary>
        /// <seealso cref="ChartRendered"/>
        /// <seealso cref="ChartRenderedEvent"/>
        protected virtual void OnChartRendered(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Raise chart rendered event
        /// </summary>
        internal void RaiseChartRendered()
        {
            ChartRenderedEventArgs args = new ChartRenderedEventArgs();
            args.RoutedEvent = XamChart.ChartRenderedEvent;
            args.Source = this;
            this.OnChartRendered(args);
        }

        /// <summary>
        /// This event occurs after the chart is rendered.
        /// </summary>
        /// <seealso cref="OnChartRendered"/>
        /// <seealso cref="ChartRenderedEvent"/>
        /// <seealso cref="ChartRenderedEventArgs"/>
        //[Description("This event occurs after the chart is rendered.")]
        //[Category("Behavior")]
        public event EventHandler<ChartRenderedEventArgs> ChartRendered
        {
            add
            {
                base.AddHandler(XamChart.ChartRenderedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamChart.ChartRenderedEvent, value);
            }
        }

        #endregion Rendered

        #region InvalidDataReceived

        /// <summary>
        /// Event ID for the <see cref="InvalidDataReceived"/> routed event
        /// </summary>
        /// <seealso cref="InvalidDataReceived"/>
        /// <seealso cref="OnInvalidDataReceived"/>
        public static RoutedEvent InvalidDataReceivedEvent = EventManager.RegisterRoutedEvent("InvalidDataReceived", RoutingStrategy.Bubble, typeof(EventHandler<InvalidDataEventArgs>), typeof(XamChart));

        /// <summary>
        /// Event is raised when invalid data is bound to the XamChart.
        /// </summary>
        /// <seealso cref="OnInvalidDataReceived"/>
        /// <seealso cref="InvalidDataReceivedEvent"/>
        /// <seealso cref="InvalidDataEventArgs"/>
        //[Description(Event raised when invalid data is bound to the XamChart.")]
        //[Category("Behavior")]
        public event EventHandler<InvalidDataEventArgs> InvalidDataReceived
        {
            add { this.AddHandler(XamChart.InvalidDataReceivedEvent, value); }
            remove { this.RemoveHandler(XamChart.InvalidDataReceivedEvent, value); }
        }

        /// <summary>
        /// Method invoked when invalid data is bound to the XamChart.
        /// </summary>
        /// <seealso cref="InvalidDataReceived"/>
        /// <seealso cref="InvalidDataReceivedEvent"/>
        protected virtual void OnInvalidDataReceived(RoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        /// <summary>
        /// Raise invalid data received event.
        /// <returns>Returns a bool value that indicates whether tha invalid data exception has been handled.</returns>
        /// </summary>
        internal bool RaiseInvalidDataReceived(Exception exception)
        {
            InvalidDataEventArgs args = new InvalidDataEventArgs(exception);
            args.RoutedEvent = XamChart.InvalidDataReceivedEvent;
            args.Source = this;
            this.OnInvalidDataReceived(args);

            return args.ExceptionHandled;
        }

        #endregion InvalidDataReceived

        #endregion Events

        #region Methods

		/// <summary>
		/// Initializes a new <see cref="XamChart"/>
		/// </summary>
		public XamChart()
		{
			// AS 11/7/07 BR21903
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamChart), this) as Infragistics.Windows.Licensing.UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }

            this.Crosshairs = new Crosshairs();
            this._defaultChart = new Chart();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        static XamChart()
        {
            // AS 5/9/08
            // register the groupings that should be applied when the theme property is changed
            ThemeManager.RegisterGroupings(typeof(XamChart), new string[] { Theme1.Location.Grouping });

            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(XamChart), new FrameworkPropertyMetadata(typeof(XamChart)));
        }

        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamChart control = XamChart.GetControl(d);

            bool themeProperty = e.Property.Name == "Theme";
            if (themeProperty)
            {
                control.IsThemeChanging = true;
                control.UpdateLayout(); // [DN 3/23/2009:14035] this is so the legend will get its new template applied.
            }

            if (control != null && e.NewValue != e.OldValue)
            {
                control.RefreshProperty();
            }

            if (themeProperty)
            {
                control.IsThemeChanging = false;
            }
        }

        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are directed by 
        /// the layout system. The rendering instructions for this element are not used directly when this 
        /// method is invoked, and are instead preserved for later asynchronous use by layout and drawing. 
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (DefaultChart != null && DefaultChart.ChartCreator != null && DefaultChart.ChartCreator.ScenePane != null)
            {
                if (!this.View3D)
                {
                    // BR30674: Clicking on the 2D PieChart xamChart (under ListBox, which resides under StackPanel, which resides under Grid) dissappears the chart 
                    // DefaultChart.ChartCreator.ScenePane.Children.Clear();
                }
            }

            base.OnRender(drawingContext);

        }

        // AS 5/9/08
        // Let the ThemeManager handle the resourcedictionary management when the theme
        // property changes.
        //
        //private ResourceDictionary _resourceSets = new ResourceDictionary();
        //private ResourceDictionary _rd;


        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this 
        /// FrameworkElement has been updated. The specific dependency property that changed 
        /// is reported in the arguments parameter. Overrides OnPropertyChanged. 
        /// </summary>
        /// <param name="e">Event arguments that describe the property that changed, as well as old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            ContentControlPropertyChanging = true;
            if (PropertyNeedRefresh(this, e))
            {
                RefreshProperty();
            }

            base.OnPropertyChanged(e);
            ContentControlPropertyChanging = false;
        }

        internal bool IsDesignMode
        {
            get
            {
                return DesignerProperties.GetIsInDesignMode(this);
            }
        }

        /// <summary>
        /// Used to check if property value needs refresh
        /// </summary>
        /// <param name="parent">The parent object</param>
        /// <param name="e">Event arguments that describe the property that changed, as well as old and new values.</param>
        /// <returns>True if property value needs refresh</returns>
        static internal bool PropertyNeedRefresh(DependencyObject parent, DependencyPropertyChangedEventArgs e)
        {
            if (
                e.Property == XamChart.FontFamilyProperty ||
                e.Property == XamChart.FontSizeProperty ||
                e.Property == XamChart.FontStyleProperty ||
                e.Property == XamChart.FontWeightProperty ||
                e.Property == XamChart.FontStretchProperty ||
                e.Property == XamChart.SnapsToDevicePixelsProperty
                )
            {
                if (DesignerProperties.GetIsInDesignMode(parent))
                {
                    return true;
                }
            }

            if (
                //e.Property == XamChart.ForegroundProperty ||
                //e.Property == XamChart.BackgroundProperty ||
                e.Property == XamChart.StartPaletteBrushProperty ||
                e.Property == XamChart.EndPaletteBrushProperty
                )
            {
                if (!Brush.Equals(e.NewValue, e.OldValue))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Create an array of random colors
        /// </summary>
        internal void FillRandomColors()
        {
            if (_randomColors != null)
            {
                return;
            }

            _randomColors = new Color[100];

            _random = new Random(1);
            for (int colorIndex = 0; colorIndex < 100; colorIndex++)
            {
                // Create random color
                _randomColors[colorIndex] = Color.FromRgb((Byte)_random.Next(255), (Byte)_random.Next(255), (Byte)_random.Next(255));
            }

            _randomColors[0] = Color.FromRgb(0x46, 0x82, 0xb4);
            _randomColors[1] = Color.FromRgb(0x41, 0x69, 0xe1);
            _randomColors[2] = Color.FromRgb(0x64, 0x95, 0xed);
            _randomColors[3] = Color.FromRgb(0xb0, 0xc4, 0xde);
            _randomColors[4] = Color.FromRgb(0x7b, 0x68, 0xee);
            _randomColors[5] = Color.FromRgb(0x6a, 0x5a, 0xcd);
            _randomColors[6] = Color.FromRgb(0x48, 0x3d, 0x8b);
            _randomColors[7] = Color.FromRgb(0x19, 0x19, 0x70);
        }

        /// <summary>
        /// Get a random color from generated array of colors.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Color GetRandomColor(int index)
        {
            FillRandomColors();
            index %= 100;
            return _randomColors[index];
        }

        /// <summary>
        /// Called when any dependency property from the chart 
        /// control and other classes is set.
        /// </summary>
        internal void RefreshProperty()
        {
            if (!IsRefreshingMode)
            {
                Refresh();
            }
        }

        /// <summary>
        /// Repaints chart control content.
        /// </summary>
        public void Refresh()
        {
            if (!RefreshEnabled)
            {
                return;
            }

            this.IsRefreshingMode = true;

            // Used for data points only refreshing and 2D charts only
            if (View3D == false && _defaultChart != null && _defaultChart.ChartCreator != null && this.Scene != null && this.Scene.GridArea != null && this.Scene.GridArea.RefreshPointsOnly == true && _defaultChart.ChartCreator.ScenePane != null && _defaultChart.ChartCreator.ScenePane.PlottingPane2D != null)
            {
                _defaultChart.ChartCreator.ScenePane.PlottingPane2D.Draw();
                return;
            }

            if (_defaultChart != null && _defaultChart.Parent != null)
            {
                // [TT 26/10/2010:58216] - don't clear the collection if the count is 0. 
                // This throws an exception in design time. 
                if (_defaultChart.Children.Count > 0)
                {
                    _defaultChart.Children.Clear();
                }

                _defaultChart.Refresh();
            }

            this.IsRefreshingMode = false;
        }

        /// <summary>
        /// Perform a data binding. The DataSource property of a series has to be set before this method is called.
        /// </summary>
        /// <remarks>
        /// This method should be called only if the data source cannot be refreshed automatically in run time. 
        /// A <see cref="Infragistics.Windows.Chart.DataPoint"/> is created for every record, and the <see cref="Infragistics.Windows.Chart.DataPoint.Value"/> property of 
        /// the data point will be filled with a value from the specified data source. The series will be filled with created data points. 
        /// If <see cref="Infragistics.Windows.Chart.Series.DataMapping"/> property is not specified, only the <see cref="Infragistics.Windows.Chart.DataPoint.Value"/> property from data point will be set. To fill <see cref="Infragistics.Windows.Chart.DataPoint.Label"/> or 
        /// <see cref="Infragistics.Windows.Chart.DataPoint.ChartParameters"/> (Chart parameters keep values for Stock or Scatter chart types), 
        /// the <see cref="Infragistics.Windows.Chart.Series.DataMapping"/> property from the series has to be used. 
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.Chart.Series.DataSource"/>
        /// <seealso cref="Infragistics.Windows.Chart.Series.DataMapping"/>
        public void DataSourceRefresh()
        {
            if (DefaultChart != null)
            {
                DefaultChart.BindData();
            }
        }

        /// <summary>
        /// Called when the template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DependencyObject content = base.GetTemplateChild("PART_Default_Chart");

            if (content == null)
                return;

            ContentPresenter contentPresenter = content as ContentPresenter;
            if (this._defaultChart == null)
            {
                this._defaultChart = new Chart();
            }
            _defaultChart.ChartParent = this;
            contentPresenter.Content = _defaultChart;
            _defaultChart.NestedChart = false;

            // The default chart has to be in logical tree even if it is in 
            // Visual tree. Otherwise Binding will not work.
            this.AddLogicalChild(_defaultChart);
            // [DN 12/10/2009:18125] adding call to refresh here.
            // [TT 29985] - don't call Refresh here, because of issue 29985 
            this.Refresh();
        }

        /// <summary>
        /// Returns the topmost data point of a hit test.
        /// </summary>
        /// <param name="e">Provides data for Mouse related events.</param>
        /// <returns>Provides data with information about selected data point.</returns>
        public HitTestArgs HitTest(MouseEventArgs e)
        {
            return _defaultChart.HitTest(e);
        }

        /// <summary>
        /// Returns the topmost data point of a hit test.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns></returns>
        public HitTestArgs HitTest(double x, double y)
        {
            return _defaultChart.HitTest(x, y);
        }

        /// <summary>
        /// Returns the topmost data point of a hit test.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public HitTestArgs HitTest(Point position)
        {
            return _defaultChart.HitTest(position.X, position.Y);
        }

        internal static XamChart GetControl(object child)
        {
            return FindXamChart(child) as XamChart;
        }

        private static object FindXamChart(object child)
        {
            if (child is XamChart)
            {
                return child;
            }
            else if (child is DataPoint)
            {
                return FindXamChart((child as DataPoint).ChartParent);
            }
            else if (child is DataPointCollection)
            {
                return FindXamChart((child as DataPointCollection).ChartParent);
            }
            else if (child is Series)
            {
                return FindXamChart((child as Series).ChartParent);
            }
            else if (child is SeriesCollection)
            {
                return FindXamChart((child as SeriesCollection).ChartParent);
            }
            else if (child is StripeCollection)
            {
                return FindXamChart((child as StripeCollection).ChartParent);
            }
            else if (child is Chart)
            {
                return FindXamChart((child as Chart).ChartParent);
            }
            else if (child is ChartCollection)
            {
                return FindXamChart((child as ChartCollection).ChartParent);
            }
            else if (child is Caption)
            {
                return FindXamChart((child as Caption).ChartParent);
            }
            else if (child is CaptionPane)
            {
                return FindXamChart((child as CaptionPane).ChartParent);
            }
            else if (child is Legend)
            {
                return FindXamChart((child as Legend).ChartParent);
            }
            else if (child is Mark)
            {
                return FindXamChart((child as Mark).ChartParent);
            }
            else if (child is Stripe)
            {
                return FindXamChart((child as Stripe).ChartParent);
            }
            else if (child is Label)
            {
                return FindXamChart((child as Label).ChartParent);
            }
            else if (child is Scene)
            {
                return FindXamChart((child as Scene).ChartParent);
            }
            else if (child is Animation)
            {
                return FindXamChart((child as Animation).ChartParent);
            }
            else if (child is Axis)
            {
                return FindXamChart((child as Axis).ChartParent);
            }
            else if (child is AxisCollection)
            {
                return FindXamChart((child as AxisCollection).ChartParent);
            }
            else if (child is ChartParameterCollection)
            {
                return FindXamChart((child as ChartParameterCollection).ChartParent);
            }
            else if (child is ChartParameter)
            {
                return FindXamChart((child as ChartParameter).ChartParent);
            }
            else if (child is LightCollection)
            {
                return FindXamChart((child as LightCollection).ChartParent);
            }
            else if (child is Marker)
            {
                return FindXamChart((child as Marker).ChartParent);
            }
            else if (child is GridArea)
            {
                return FindXamChart((child as GridArea).ChartParent);
            }
            else if (child is LegendItem)
            {
                return FindXamChart((child as LegendItem).ChartParent);
            }
            else if (child is LegendItemCollection)
            {
                return FindXamChart((child as LegendItemCollection).ChartParent);
            }
            else if (child is GridAreaRenderingOptions)
            {
                return FindXamChart((child as GridAreaRenderingOptions).ChartParent);
            }
            else if (child is GridAreaRenderingDetails)
            {
                return FindXamChart((child as GridAreaRenderingDetails).ChartParent);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns minimum, maximum and unit axis values.
        /// </summary>
        /// <param name="type">Axis Type</param>
        /// <param name="minimum">Minimum axis value</param>
        /// <param name="maximum">Maximum axis value</param>
        /// <param name="unit">Axis unit</param>
        public void GetAxisRange(AxisType type, out double minimum, out double maximum, out double unit)
        {
            minimum = double.NaN;
            maximum = double.NaN;
            unit = double.NaN;

            if (DefaultChart == null || DefaultChart.ChartCreator == null)
            {
                return;
            }

            ChartCreator creator = this.DefaultChart.ChartCreator;

            if (type == AxisType.PrimaryX && creator.AxisX != null)
            {
                minimum = creator.AxisX.RoundedMinimum;
                maximum = creator.AxisX.RoundedMaximum;
                unit = creator.AxisX.RoundedInterval;
            }
            else if (type == AxisType.PrimaryY && creator.AxisY != null)
            {
                minimum = creator.AxisY.RoundedMinimum;
                maximum = creator.AxisY.RoundedMaximum;
                unit = creator.AxisY.RoundedInterval;
            }
            else if (type == AxisType.PrimaryZ && creator.AxisZ != null)
            {
                minimum = creator.AxisZ.RoundedMinimum;
                maximum = creator.AxisZ.RoundedMaximum;
                unit = creator.AxisZ.RoundedInterval;
            }
            else if (type == AxisType.SecondaryX && creator.AxisX2 != null)
            {
                minimum = creator.AxisX2.RoundedMinimum;
                maximum = creator.AxisX2.RoundedMaximum;
                unit = creator.AxisX2.RoundedInterval;
            }
            else if (type == AxisType.SecondaryY && creator.AxisY2 != null)
            {
                minimum = creator.AxisY2.RoundedMinimum;
                maximum = creator.AxisY2.RoundedMaximum;
                unit = creator.AxisY2.RoundedInterval;
            }
        }

        /// <summary>
        /// Returns position in pixels from an axis value.
        /// </summary>
        /// <param name="type">Axis Type</param>
        /// <param name="value">Axis value</param>
        /// <returns>Pixel position</returns>
        /// <seealso cref="HitTest"/>
        public double GetPosition(AxisType type, double value)
        {
            double retVal = double.NaN;

            if (this.View3D)
            {
                // GetPosition method has to be used with 2D charts only.
                throw new InvalidOperationException(ErrorString.Exc58);
            }

            if (DefaultChart == null || DefaultChart.ChartCreator == null || DefaultChart.ChartCreator.Scene == null || DefaultChart.ChartCreator.Scene.GridArea == null)
            {
                return retVal;
            }

            double leftScene = DefaultChart.ChartCreator.ScenePane.GetAbsoluteX(DefaultChart.ChartCreator.Scene.GridArea.RelativePosition.Left);
            double leftChart = DefaultChart.GetAbsoluteX(DefaultChart.ChartCreator.Scene.RelativePosition.Left);
            double topScene = DefaultChart.ChartCreator.ScenePane.GetAbsoluteY(DefaultChart.ChartCreator.Scene.GridArea.RelativePosition.Top);
            double topChart = DefaultChart.GetAbsoluteY(DefaultChart.ChartCreator.Scene.RelativePosition.Top);



            ChartCreator creator = this.DefaultChart.ChartCreator;

            if (type == AxisType.PrimaryX && creator.AxisX != null)
            {
                retVal = leftChart + leftScene + creator.AxisX.GetPositionLogarithmic(value);
            }
            else if (type == AxisType.PrimaryY && creator.AxisY != null)
            {
                retVal = topChart + topScene + creator.AxisY.GetPositionLogarithmic(value);
            }

            else if (type == AxisType.SecondaryX && creator.AxisX2 != null)
            {
                retVal = leftChart + leftScene + creator.AxisX2.GetPositionLogarithmic(value);
            }

            else if (type == AxisType.SecondaryY && creator.AxisY2 != null)
            {
                retVal = topChart + topScene + creator.AxisY2.GetPositionLogarithmic(value);
            }

            return retVal;
        }

        internal bool IsPerformance()
        {
            bool isPerformance = false;

            if (this.Scene != null &&
                this.Scene.ScenePane != null &&
                this.Scene.ScenePane.PlottingPane2D != null)
            {
                isPerformance = this.Scene.ScenePane.PlottingPane2D.IsPerformance();
            }

            return isPerformance;
        }

        #endregion Methods

        #region Properties

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
        //[Description("The collection of data series which are used to provide data to the chart.")]
        //[Category("Chart Data")]
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

        
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)


        #region Axes

        private AxisCollection _axes = new AxisCollection();

        /// <summary>
        /// Gets the axis collection. Every axis has an AxisType which could be PrimaryX, PrimaryY, PrimaryZ, SecondaryX and SecondaryY.
        /// </summary>
        /// <remarks>
        /// By default, the chart's axes do not exist in the Axes collection; however, 
        /// internally, default axes are created. If you don't want to modify the appearance 
        /// or range of the axes, grid lines, or axis labels, you can simply use the default 
        /// values of the axes. If you want to change the default axis values, you need 
        /// to create an axis and add it to the Axes collection.
        /// </remarks>
        //[Description("Gets the axis collection. Every axis has an AxisType which could be PrimaryX, PrimaryY, PrimaryZ, SecondaryX and SecondaryY.")]
        //[Category("Chart Data")]
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
            typeof(Transform3D), typeof(XamChart), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets Transformation Matrix for 3D charts that provides all three-dimensional transformations, including translation, rotation, and scale transformations.
        /// </summary>
        /// <seealso cref="Transform3DProperty"/>
        //[Description("Gets or sets Transformation Matrix for 3D charts that provides all three-dimensional transformations, including translation, rotation, and scale transformations.")]
        //[Category("Chart Data")]
        public Transform3D Transform3D
        {
            get
            {
                return (Transform3D)this.GetValue(XamChart.Transform3DProperty);
            }
            set
            {
                this.SetValue(XamChart.Transform3DProperty, value);
            }
        }

        #endregion Transform3D

        #region Caption

        /// <summary>
        /// Identifies the <see cref="Caption"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption",
            typeof(Caption), typeof(XamChart), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the chart caption. Keeps information about text value, font, color and position for a Chart title.
        /// </summary>
        /// <seealso cref="CaptionProperty"/>
        //[Description("Gets or sets the chart caption. Keeps information about text value, font, color and position for a Chart title.")]
        //[Category("Chart Data")]
        public Caption Caption
        {
            get
            {
                Caption obj = (Caption)this.GetValue(XamChart.CaptionProperty);
                if (obj != null)
                {
                    obj.ChartParent = this;
                }

                return obj;
            }
            set
            {
                this.SetValue(XamChart.CaptionProperty, value);
            }
        }

        #endregion Caption

        #region Scene

        Scene _scene = new Scene();

        /// <summary>
        /// Identifies the <see cref="Scene"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SceneProperty = DependencyProperty.Register("Scene",
            typeof(Scene), typeof(XamChart), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the scene which contains information related to chart scene appearance. The scene 
        /// is different for 2D and 3D chart. For 2D chart it is used for Background color 
        /// and position. For 3D chart it also containes thicknes of the 3D scene.
        /// </summary>
        /// <seealso cref="SceneProperty"/>
        //[Description("Gets or sets the chart scene which contains information related to scene appearance. The chart scene is different for 2D and 3D chart. For 2D chart it is used for Background color and position. For 3D chart it also containes thicknes of the 3D scene.")]
        //[Category("Chart Data")]
        public Scene Scene
        {
            get
            {
                Scene obj = (Scene)this.GetValue(XamChart.SceneProperty);
                if (obj == null)
                {
                    obj = _scene;
                }
                obj.ChartParent = this;
                return obj;
            }
            set
            {
                this.SetValue(XamChart.SceneProperty, value);
            }
        }

        #endregion Scene

        #region Legend

        Legend _legend = new Legend();

        /// <summary>
        /// Identifies the <see cref="Legend"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LegendProperty = DependencyProperty.Register("Legend",
            typeof(Legend), typeof(XamChart), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the chart legend. The chart legend box appears alongside 
        /// the chart border. It is used to give text description for every data 
        /// point or series appearance in the chart. Many qualities of the legend 
        /// are dependent upon ChartType.
        /// </summary>
        /// <seealso cref="LegendProperty"/>
        //[Description("Gets or sets the chart legend. The chart legend box appears alongside the chart border. It is used to give text description for every data point or series appearance in the chart. Many qualities of the legend are dependent upon ChartType.")]
        //[Category("Chart Data")]
        public Legend Legend
        {
            get
            {
                Legend obj = (Legend)this.GetValue(XamChart.LegendProperty);
                if (obj == null)
                {
                    obj = _legend;
                }
                obj.ChartParent = this;
                return obj;
            }
            set
            {
                this.SetValue(XamChart.LegendProperty, value);
            }
        }

        #endregion Legend

        #region View3D

        /// <summary>
        /// Identifies the <see cref="View3D"/> dependency property
        /// </summary>
        public static readonly DependencyProperty View3DProperty = DependencyProperty.Register("View3D",
            typeof(bool), typeof(XamChart), new FrameworkPropertyMetadata((bool)false, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether the chart is rendered in 3D.
        /// </summary>
        /// <seealso cref="View3DProperty"/>
        //[Description("Gets or sets a value that indicates whether the chart is rendered in 3D.")]
        //[Category("Chart Data")]
        public bool View3D
        {
            get
            {
                return (bool)this.GetValue(XamChart.View3DProperty);
            }
            set
            {
                this.SetValue(XamChart.View3DProperty, value);
            }
        }


        #endregion View3D

        #region RefreshEnabled

        /// <summary>
        /// Identifies the <see cref="RefreshEnabled"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RefreshEnabledProperty = DependencyProperty.Register("RefreshEnabled",
            typeof(bool), typeof(XamChart), new FrameworkPropertyMetadata((bool)true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether refreshing of the chart is enabled.
        /// </summary>
        /// <remarks>
        /// This property has to be used every time when data are changed in runtime to improve performance 
        /// of the rendering. For example, if we have observable collection which is used as a <see cref="Infragistics.Windows.Chart.Series.DataSource"/> 
        /// and we change values in the collection, every time when a value is changed the chart will be drawn. 
        /// Similar case is changing data point values in run time. To stop refreshing the chart RefreshEnabled 
        /// property has to be set to false before we change data and back to true after data is changed. 
        /// </remarks>
        /// <seealso cref="RefreshEnabledProperty"/>
        //[Description("Gets or sets a value that indicates whether refreshing of the chart is enabled.")]
        public bool RefreshEnabled
        {
            get
            {
                return (bool)this.GetValue(XamChart.RefreshEnabledProperty);
            }
            set
            {
                this.SetValue(XamChart.RefreshEnabledProperty, value);
            }
        }


        #endregion RefreshEnabled

        #region Lights

        private LightCollection _lights = new LightCollection();

        /// <summary>
        /// Gets Collection with lights that represent lighting applied to a 3-D scene. Used for 3D charts only.
        /// </summary>
        /// <remarks>
        /// For 3D charts, by default, a light effect is created with default values (one DirectionalLight, 
        /// and one PointLight). However, if you want to change the Light effects for the 3D scene, you need 
        /// to create a Light effect and add it to the Lights collection. When you add a Light effect to the collection, 
        /// the default Light effects disappear, and only newly created Light effects are used. 
        /// </remarks>
        //[Description("Gets Collection with lights that represent lighting applied to a 3-D scene. Used for 3D charts only.")]
        //[Category("Chart Data")]
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

        #region StartPaletteBrush

        /// <summary>
        /// Identifies the <see cref="StartPaletteBrush"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StartPaletteBrushProperty = DependencyProperty.Register("StartPaletteBrush",
            typeof(Brush), typeof(XamChart), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0xE6, 0xBE, 0x02)), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the start Brush which is used to create a palette of colors for data points and series.
        /// </summary>
        /// <seealso cref="StartPaletteBrushProperty"/>
        //[Description("Gets or sets the end Brush which is used to create a palette of colors for data points and series.")]
        //[Category("Brushes")]
        public Brush StartPaletteBrush
        {
            get
            {
                return (Brush)this.GetValue(XamChart.StartPaletteBrushProperty);
            }
            set
            {
                this.SetValue(XamChart.StartPaletteBrushProperty, value);
            }
        }

        #endregion StartPaletteBrush

        #region EndPaletteBrush

        /// <summary>
        /// Identifies the <see cref="EndPaletteBrush"/> dependency property
        /// </summary>
        public static readonly DependencyProperty EndPaletteBrushProperty = DependencyProperty.Register("EndPaletteBrush",
            typeof(Brush), typeof(XamChart), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x07, 0x6C, 0xB0)), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the end Brush which is used to create a palette of colors for data points and series.
        /// </summary>
        /// <seealso cref="EndPaletteBrushProperty"/>
        //[Description("Gets or sets the end Brush which is used to create a palette of colors for data points and series.")]
        //[Category("Brushes")]
        public Brush EndPaletteBrush
        {
            get
            {
                return (Brush)this.GetValue(XamChart.EndPaletteBrushProperty);
            }
            set
            {
                this.SetValue(XamChart.EndPaletteBrushProperty, value);
            }
        }

        #endregion EndPaletteBrush


        #region Crosshairs

        /// <summary>
        /// Gets the crosshairs property. Crosshairs are lines which help the the end user 
        /// clearly see the relationship between points on the graph that are aligned on or 
        /// near the same axis values. 
        /// </summary>
        /// <value>The crosshairs.</value>
        public Crosshairs Crosshairs
        {
            get { return (Crosshairs)GetValue(CrosshairsProperty); }
            set { SetValue(CrosshairsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Crosshairs"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CrosshairsProperty =
            DependencyProperty.Register("Crosshairs", typeof(Crosshairs), typeof(XamChart),
            new PropertyMetadata(null, OnCrosshairsChanged));

        private static void OnCrosshairsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamChart)d).OnCrosshairsChanged((Crosshairs)e.OldValue, (Crosshairs)e.NewValue);
        }

        private void OnCrosshairsChanged(Crosshairs oldValue, Crosshairs newValue)
        {
            if (this.Scene != null && this.Scene.GridArea != null)
            {
                if (oldValue != null)
                {
                    oldValue.Hide();
                }

                PlottingPane plottingPane = this.Scene.GridArea.PlottingPane2D;
                if (plottingPane != null && newValue != null)
                {
                    if (oldValue != null)
                    {
                        newValue.Visible = oldValue.Visible;
                    }
                    newValue.PlottingPane = plottingPane;
                    newValue.SetupCrosshairSizes();
                    newValue.UpdateByCursor(new Point(oldValue.ScreenX, oldValue.ScreenY));
                    newValue.Show();

                }                
            }
        }

        #endregion


        #region Theme

        
#region Infragistics Source Cleanup (Region)





































#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Identifies the <see cref="Theme"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ThemeProperty = ThemeManager.ThemeProperty.AddOwner(typeof(XamChart), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the default look for the control.
        /// </summary>
        /// <remarks>
        /// <para class="body">If left set to null then the default 'Generic' theme will be used. 
        /// This property can be set to the name of any registered theme (see <see cref="Infragistics.Windows.Themes.ThemeManager.Register(string, string, ResourceDictionary)"/> and <see cref="Infragistics.Windows.Themes.ThemeManager.GetThemes()"/> methods).</para>
        /// <para></para>
        /// <para class="note"><b>Note: </b> The following themes are pre-registered by this assembly but additional themes can be registered as well.
        /// <ul>
        /// <li>"Generic" - the default theme.</li>
        /// </ul>
        /// </para>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.Themes.ThemeManager"/>
        /// <seealso cref="ThemeProperty"/>
        //[Description("Gets or sets the default look for the control.")]
        //[Category("Chart Data")]
        [Bindable(true)]
        [TypeConverter(typeof(Infragistics.Windows.Themes.Internal.ChartThemeTypeConverter))]
        public string Theme
        {
            get
            {
                return (string)this.GetValue(XamChart.ThemeProperty);
            }
            set
            {
                this.SetValue(XamChart.ThemeProperty, value);
            }
        }

        #endregion Theme

        #region DrawException

        /// <summary>
        /// Identifies the <see cref="DrawException"/> dependency property
        /// </summary>        
        public static readonly DependencyProperty DrawExceptionProperty = DependencyProperty.Register("DrawException",
            typeof(bool), typeof(XamChart), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether an exception is drawn as a string message on the screen. Otherwise, exception will be thrown.
        /// </summary>
        /// <seealso cref="DrawExceptionProperty"/>
        //[Description("Gets or sets a value that indicates whether an exception is drawn as a string message on the screen. Otherwise, exception will be thrown.")]        
        public bool DrawException
        {
            get
            {
                return (bool)this.GetValue(XamChart.DrawExceptionProperty);
            }
            set
            {
                this.SetValue(XamChart.DrawExceptionProperty, value);
            }
        }

        #endregion DrawException
        
        #endregion Properties
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