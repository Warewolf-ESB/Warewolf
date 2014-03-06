using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Windows.Media.Effects;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Base class for stacked series with a category x-axis and a numeric y-axis.
    /// </summary>
    public abstract class HorizontalStackedSeriesBase : StackedSeriesBase
    {
        #region XAxis Dependency Property
        /// <summary>
        /// Gets or sets the effective x-axis for this series.
        /// </summary>
        public CategoryAxisBase XAxis
        {
            get
            {
                return (CategoryAxisBase)GetValue(XAxisProperty);
            }
            set
            {
                SetValue(XAxisProperty, value);
            }
        }

        internal const string XAxisPropertyName = "XAxis";

        /// <summary>
        /// Identifies the XAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(XAxisPropertyName, typeof(CategoryAxisBase), typeof(HorizontalStackedSeriesBase),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as HorizontalStackedSeriesBase).RaisePropertyChanged(XAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region YAxis Depedency Property
        /// <summary>
        /// Gets or sets the effective y-axis for this series.
        /// </summary>
        public NumericYAxis YAxis
        {
            get
            {
                return (NumericYAxis)GetValue(YAxisProperty);
            }
            set
            {
                SetValue(YAxisProperty, value);
            }
        }

        internal const string YAxisPropertyName = "YAxis";

        /// <summary>
        /// Identifies the YAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(YAxisPropertyName, typeof(NumericYAxis), typeof(HorizontalStackedSeriesBase),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as HorizontalStackedSeriesBase).RaisePropertyChanged(YAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        internal override Axis GetXAxis()
        {
            return this.XAxis;
        }
        internal override Axis GetYAxis()
        {
            return this.YAxis;
        }
        internal override bool UpdateNumericAxisRange()
        {
            return this.YAxis != null && this.YAxis.UpdateRange();
        }
        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);
            switch (propertyName)
            {
                case XAxisPropertyName:
                    DeregisterForAxis(oldValue as Axis);
                    RegisterForAxis(newValue as Axis);

                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case YAxisPropertyName:
                    DeregisterForAxis(oldValue as Axis);
                    RegisterForAxis(newValue as Axis);

                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);

                    this.UpdateNumericAxisRange();

                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }
    }
    /// <summary>
    /// Base class for stacked series with a numeric x-axis and a category y-axis.
    /// </summary>
    public abstract class VerticalStackedSeriesBase : StackedSeriesBase
    {
        #region XAxis Dependency Property
        /// <summary>
        /// Gets or sets the effective x-axis for the current CategorySeries object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public NumericXAxis XAxis
        {
            get
            {
                return (NumericXAxis)GetValue(XAxisProperty);
            }
            set
            {
                SetValue(XAxisProperty, value);
            }
        }

        internal const string XAxisPropertyName = "XAxis";

        /// <summary>
        /// Identifies the ActualXAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(XAxisPropertyName, typeof(NumericXAxis), typeof(VerticalStackedSeriesBase),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as VerticalStackedSeriesBase).RaisePropertyChanged(XAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region YAxis Depedency Property
        /// <summary>
        /// Gets or sets the effective y-axis for the current CategorySeries object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public CategoryYAxis YAxis
        {
            get
            {
                return (CategoryYAxis)GetValue(YAxisProperty);
            }
            set
            {
                SetValue(YAxisProperty, value);
            }
        }

        internal const string YAxisPropertyName = "YAxis";

        /// <summary>
        /// Identifies the ActualYAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(YAxisPropertyName, typeof(CategoryYAxis), typeof(VerticalStackedSeriesBase),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as VerticalStackedSeriesBase).RaisePropertyChanged(YAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        internal override Axis GetXAxis()
        {
            return this.XAxis;
        }
        internal override Axis GetYAxis()
        {
            return this.YAxis;
        }
        internal override bool UpdateNumericAxisRange()
        {
            return this.XAxis != null && this.XAxis.UpdateRange();
        }
        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);
            switch (propertyName)
            {
                case XAxisPropertyName:
                    DeregisterForAxis(oldValue as Axis);
                    RegisterForAxis(newValue as Axis);
                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case YAxisPropertyName:
                    DeregisterForAxis(oldValue as Axis);
                    RegisterForAxis(newValue as Axis);
                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                    this.UpdateNumericAxisRange();
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }
    }
    
    internal interface IStacked100Series { }

    /// <summary>
    /// Represents a base class for stacked series.
    /// </summary>
    public abstract class StackedSeriesBase : CategorySeries, IIsCategoryBased
    {
        #region C'tor and Initialization
        /// <summary>
        /// Creates a new instance of a stacked series.
        /// </summary>
        protected StackedSeriesBase()
        {
            Series = new StackedSeriesCollection();
            Series.CollectionResetting += Series_CollectionResetting;
            Series.CollectionChanged += Series_CollectionChanged;
            FramePreparer = new StackedSeriesFramePreparer(this, this.StackedView, this, this, this.StackedView.BucketCalculator);
            StackedSeriesManager = new StackedSeriesManager(this);

            AutoGeneratedSeries = new StackedSeriesCollection();
            AutoGeneratedSeries.CollectionChanged += Series_CollectionChanged;
            AutoGeneratedSeries.CollectionResetting += AutoGeneratedSeries_CollectionResetting;

            //empty legend template for the parent series.
            LegendItemTemplate = new DataTemplate();

            //set up bindings for UIElement properties to keep track of their changes.
            SetBinding(SeriesVisibilityProperty, new Binding("Visibility") { Source = this });


            SetBinding(SeriesCursorProperty, new Binding("Cursor") { Source = this });
            SetBinding(SeriesEffectProperty, new Binding("Effect") { Source = this });

            Loaded += (o, e) => 
            {
                GenerateSeries();
            };
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (RootCanvas != null)
            {
                if (!RootCanvas.Children.Contains(PlotArea))
                {
                    RootCanvas.Children.Add(PlotArea);
                }

                if (!RootCanvas.Children.Contains(SeriesPanel))
                {
                    RootCanvas.Children.Add(SeriesPanel);
                }

                RootCanvas.SizeChanged += (o, e) => 
                {
                    SeriesPanel.Width = e.NewSize.Width;
                    SeriesPanel.Height = e.NewSize.Height;
                };
            }
        }
        #endregion

        #region View-related
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new StackedSeriesView(this);
        }
        
        internal StackedSeriesView StackedView { get; set; }
        
        /// <summary>
        /// Called when the view has been created.
        /// </summary>
        /// <param name="view">View class for the current model</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            StackedView = (StackedSeriesView)view;
        }
        #endregion

        #region Public Properties
        #region Series
        /// <summary>
        /// Contains one or more stacked fragments.
        /// </summary>
        public StackedSeriesCollection Series 
        { 
            get; 
            private set; 
        }
        #endregion

        #region AutoGenerateSeries Dependency Property
        internal const string AutoGenerateSeriesPropertyName = "AutoGenerateSeries";

        /// <summary>
        /// Identifies the AutoGenerateSeries dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoGenerateSeriesProperty = DependencyProperty.Register(AutoGenerateSeriesPropertyName, typeof(bool), typeof(StackedSeriesBase),
            new PropertyMetadata(false, (o, e) => 
            {
                (o as StackedSeriesBase).RaisePropertyChanged(AutoGenerateSeriesPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets whether series should be automatically generated. Reqiures the use of GroupBy as the ItemsSource.
        /// </summary>
        public bool AutoGenerateSeries
        {
            get { return (bool)GetValue(AutoGenerateSeriesProperty); }
            set { SetValue(AutoGenerateSeriesProperty, value); }
        }
        #endregion

        #region ReverseLegendOrder Dependency Property
        internal const string ReverseLegendOrderPropertyName = "ReverseLegendOrder";

        /// <summary>
        /// Identifies the ReverseLegendOrder dependency property.
        /// </summary>
        public static readonly DependencyProperty ReverseLegendOrderProperty = DependencyProperty.Register(ReverseLegendOrderPropertyName, typeof(bool), typeof(StackedSeriesBase),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as StackedSeriesBase).RaisePropertyChanged(ReverseLegendOrderPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets whether the order of the fragment series should be reversed in the legend.
        /// </summary>
        public bool ReverseLegendOrder
        {
            get
            {
                return (bool)GetValue(ReverseLegendOrderProperty);
            }
            set
            {
                SetValue(ReverseLegendOrderProperty, value);
            }
        }
        #endregion

        #endregion

        #region Events
        /// <summary>
        /// Occurs when a new fragment series is automatically generated.
        /// </summary>
        public event StackedSeriesCreatedEventHandler SeriesCreated;
        #endregion

        #region Non-public Properties
        #region SeriesVisibility Dependency Property
        internal const string SeriesVisibilityPropertyName = "SeriesVisibility";
        internal static readonly DependencyProperty SeriesVisibilityProperty = DependencyProperty.Register(SeriesVisibilityPropertyName, typeof(Visibility), typeof(StackedSeriesBase),
            new PropertyMetadata((o, e) => 
            {
                (o as StackedSeriesBase).RaisePropertyChanged(SeriesVisibilityPropertyName, e.OldValue, e.NewValue);
            }));
        internal Visibility SeriesVisibility
        {
            get { return (Visibility)GetValue(SeriesVisibilityProperty); }
            set { SetValue(SeriesVisibilityProperty, value); }
        }
        #endregion


        #region SeriesCursor Dependency Property
        internal const string SeriesCursorPropertyName = "SeriesCursor";
        internal static readonly DependencyProperty SeriesCursorProperty = DependencyProperty.Register(SeriesCursorPropertyName, typeof(Cursor), typeof(StackedSeriesBase),
            new PropertyMetadata((o, e) =>
            {
                (o as StackedSeriesBase).RaisePropertyChanged(SeriesCursorPropertyName, e.OldValue, e.NewValue);
            }));
        internal Cursor SeriesCursor
        {
            get { return (Cursor)GetValue(SeriesCursorProperty); }
            set { SetValue(SeriesCursorProperty, value); }
        }
        #endregion

        #region SeriesEffect Dependency Property
        internal const string SeriesEffectPropertyName = "SeriesEffect";
        internal static readonly DependencyProperty SeriesEffectProperty = DependencyProperty.Register(SeriesEffectPropertyName, typeof(Effect), typeof(StackedSeriesBase),
            new PropertyMetadata((o, e) =>
            {
                (o as StackedSeriesBase).RaisePropertyChanged(SeriesEffectPropertyName, e.OldValue, e.NewValue);
            }));
        internal Effect SeriesEffect
        {
            get { return (Effect)GetValue(SeriesEffectProperty); }
            set { SetValue(SeriesEffectProperty, value); }
        }
        #endregion


        private readonly Canvas _plotArea = new Canvas();

        /// <summary>
        /// Gets the plotting canvas.
        /// </summary>
        protected internal Canvas PlotArea
        {
            get
            {
                return _plotArea;
            }
        }

        private readonly Grid _seriesPanel = new Grid();

        /// <summary>
        /// Gets the grid contraining the child series
        /// </summary>
        protected internal Grid SeriesPanel
        {
            get
            {
                return _seriesPanel;
            }
        }

        internal double Minimum { get; set; }
        internal double Maximum { get; set; }
        internal double[] Highs { get; set; }
        internal double[] Lows { get; set; }
        
        internal StackedSeriesFramePreparer FramePreparer { get; set; }
        internal StackedSeriesManager StackedSeriesManager { get; set; }
        internal StackedSeriesCollection AutoGeneratedSeries { get; set; }
        
        internal StackedSeriesCollection ActualSeries
        {
            get
            {
                return AutoGenerateSeries ? AutoGeneratedSeries : Series;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles collection resetting for the AutoGeneratedSeries collection.
        /// </summary>
        private void AutoGeneratedSeries_CollectionResetting(object sender, EventArgs e)
        {
            foreach (StackedFragmentSeries series in AutoGeneratedSeries)
            {
                if (StackedSeriesManager != null && StackedSeriesManager.SeriesLogical.Contains(series))
                {
                    series.ParentSeries = null;
                    StackedSeriesManager.SeriesLogical.Remove(series);
                }
            }
        }

        /// <summary>
        /// Handles collection resetting for the public Series collection.
        /// </summary>
        private void Series_CollectionResetting(object sender, EventArgs e)
        {
            foreach (StackedFragmentSeries series in Series)
            {
                if (StackedSeriesManager != null && StackedSeriesManager.SeriesLogical.Contains(series))
                {
                    series.ParentSeries = null;
                    StackedSeriesManager.SeriesLogical.Remove(series);
                }
            }
        }

        /// <summary>
        /// Handles collection changed for the public Series collection.
        /// </summary>
        /// <remarks>
        /// Does nothing if AutoGenerateSeries is true. 
        /// Otherwise, just adds the series to the series manager.
        /// </remarks>
        void Series_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (StackedFragmentSeries series in e.OldItems)
                {
                    if (StackedSeriesManager != null && StackedSeriesManager.SeriesLogical.Contains(series))
                    {
                        series.ParentSeries = null;
                        StackedSeriesManager.SeriesLogical.Remove(series);
                    }
                }
            }

            if (e.NewItems != null)
            {
                int counter = e.NewStartingIndex;
                foreach (StackedFragmentSeries series in e.NewItems)
                {
                    if (StackedSeriesManager != null && !StackedSeriesManager.SeriesLogical.Contains(series))
                    {
                        series.ParentSeries = this;

                        series.UpdateCursor();
                        series.UpdateEffect();

                        series.UpdateBrush();
                        series.UpdateDashArray();
                        series.UpdateDashCap();
                        series.UpdateEndCap();
                        series.UpdateIsHitTestVisible();
                        series.UpdateLegendItemBadgeTemplate();
                        series.UpdateLegendItemTemplate();
                        series.UpdateMarkerTemplate();
                        series.UpdateMarkerType();
                        series.UpdateMarkerBrush();
                        series.UpdateMarkerOutline();
                        series.UpdateMarkerStyle();
                        series.UpdateMarkerTemplate();
                        series.UpdateOpacity();
                        series.UpdateOpacityMask();
                        series.UpdateOutline();
                        series.UpdateRadiusX();
                        series.UpdateRadiusY();
                        series.UpdateStartCap();
                        series.UpdateThickness();
                        series.UpdateToolTip();




                        series.UpdateUseLightweightMarkers();
                        series.UpdateVisibility();

                        if (!AutoGenerateSeries)
                        {
                            StackedSeriesManager.SeriesLogical.Insert(counter, series);
                            counter++;
                        }
                    }
                }
            }

            if (!AutoGenerateSeries)
            {
                UpdateAxisRanges();
            }
        }
        #endregion

        #region Member Overrides
        internal override CategoryFramePreparer GetFramePreparer(CategorySeriesView view)
        {

            if (view == this.ThumbnailView)
            {
                CategorySeriesView thumbnailView = this.ThumbnailView as CategorySeriesView;
                return new StackedSeriesFramePreparer(this,
                    thumbnailView as ISupportsMarkers,
                    this.SeriesViewer.View.OverviewPlusDetailViewportHost,
                    this,
                    thumbnailView.BucketCalculator);

            }
            else

            {
                return this.FramePreparer;
            }
        }
        internal override void PrepareFrame(CategoryFrame frame, CategorySeriesView view)
        {
            this.PrepareData();       
            this.GetFramePreparer(view).PrepareFrame(frame, view);
        }

        /// <summary>
        /// Clears all visuals that belong to this series.
        /// </summary>
        /// <param name="wipeClean">WipeClean</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            foreach (var series in ActualSeries)
            {
                if (series.VisualSeriesLink != null)
                {
                    series.VisualSeriesLink.ClearRendering(wipeClean, series.VisualSeriesLink.View);
                }
            }
        }

        internal void CalculateStackedValues()
        {
            PrepareData();
        }

        internal virtual void UpdateAxisRanges()
        {
            Axis xAxis = this.GetXAxis();
            if (xAxis != null)
            {
                xAxis.UpdateRange(true);
            }
            Axis yAxis = this.GetYAxis();
            if (yAxis != null)
            {
                yAxis.UpdateRange(true);
            }
        }

        /// <summary>
        /// Scrolls the series to display the item for the specified data item.
        /// </summary>
        /// <remarks>
        /// The series is scrolled by the minimum amount required to place the specified data item within
        /// the central 80% of the visible axis.
        /// </remarks>
        /// <param name="item">The data item (item) to scroll to.</param>
        /// <returns>True if the specified item could be displayed.</returns>
        public override bool ScrollIntoView(object item)
        {
            int index = FastItemsSource != null ? FastItemsSource[item] : -1;
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = View.Viewport;

            if (index >= 0 && windowRect != null && viewportRect != null)
            {
                Axis xAxis = this.GetXAxis();
                if (xAxis != null)
                {
                    ScalerParams xParams = new ScalerParams(windowRect, viewportRect, xAxis.IsInverted);
                    double cx = xAxis.GetScaledValue(index, xParams);

                    if (cx < windowRect.Left + 0.1 * windowRect.Width)
                    {
                        cx = cx + 0.4 * windowRect.Width;
                    }

                    if (cx > windowRect.Right - 0.1 * windowRect.Width)
                    {
                        cx = cx - 0.4 * windowRect.Width;
                    }

                    windowRect.X = cx - 0.5 * windowRect.Width;
                }
                Axis yAxis = this.GetYAxis();
                if (yAxis != null && Highs != null && index < Highs.Length)
                {
                    ScalerParams yParams = new ScalerParams(windowRect, viewportRect, yAxis.IsInverted);
                    double high = yAxis.GetScaledValue(Highs[index], yParams);
                    double low = yAxis.GetScaledValue(Lows[index], yParams);
                    if (!double.IsNaN(high) && !double.IsNaN(low))
                    {
                        double height = Math.Abs(low - high);
                        if (windowRect.Height < height)
                        {
                            windowRect.Height = height;
                            windowRect.Y = Math.Min(low, high);
                        }
                        else
                        {
                            if (low < windowRect.Top + 0.1 * windowRect.Height)
                            {
                                low = low + 0.4 * windowRect.Height;
                            }

                            if (low > windowRect.Bottom - 0.1 * windowRect.Height)
                            {
                                low = low - 0.4 * windowRect.Height;
                            }

                            windowRect.Y = low - 0.5 * windowRect.Height;
                        }
                    }
                }

                SyncLink.WindowNotify(SeriesViewer, windowRect);
            }

            return index >= 0;
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (Lows == null || Lows.Length == 0
                || Highs == null || Highs.Length == 0)
            {
                return null;
            }

            if (axis == this.GetXAxis())
            {
                int max = Math.Min(Lows.Length, Highs.Length);

                return new AxisRange(0, max - 1);
            }

            if (axis == this.GetYAxis())
            {
                return new AxisRange(Minimum, Maximum);
            }

            return null;
        }

        /// <summary>
        /// Determines if the series should display markers.
        /// </summary>
        /// <returns></returns>
        protected internal override bool ShouldDisplayMarkers()
        {
            return false;
        }

        /// <summary>
        /// When overridden in a derived class, DataChangedOverride is called whenever a change is made to
        /// the series data.
        /// </summary>
        /// <param name="action">The action performed on the data</param>
        /// <param name="position">The index of the first item involved in the update.</param>
        /// <param name="count">The number of items in the update.</param>
        /// <param name="propertyName">The name of the updated property.</param>
        protected override void DataUpdatedOverride(FastItemsSourceEventAction action, int position, int count, string propertyName)
        {
            this.UpdateNumericAxisRange();
            RenderSeries(false);
        }

        #endregion

        #region Non-public Methods

        /// <summary>
        /// Returns the series view associated with the current series model.
        /// </summary>
        internal virtual CategorySeriesView GetSeriesView()
        {
            return StackedView;
        }

        /// <summary>
        /// Returns the scaled zero value based on the axis reference value.
        /// </summary>
        internal virtual double GetScaledWorldZeroValue()
        {
            double value = 0.0;

            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;
            NumericYAxis yAxis = this.GetYAxis() as NumericYAxis;

            if (!windowRect.IsEmpty && !viewportRect.IsEmpty && yAxis != null)
            {
                ScalerParams yParams = new ScalerParams(windowRect, viewportRect, yAxis.IsInverted);
                value = yAxis.GetScaledValue(yAxis.ReferenceValue, yParams);
            }

            return value;
        }

        /// <summary>
        /// Returns the unscaled zero value based on the axis reference value.
        /// </summary>
        internal virtual double GetUnscaledWorldZeroValue()
        {
            NumericYAxis yAxis = this.GetYAxis() as NumericYAxis;
            if (yAxis != null)
            {
                return yAxis.ReferenceValue;
            }

            return 0.0;
        }

        /// <summary>
        /// Returns the index of the current fragment series based on the index of the parent series.
        /// </summary>
        internal virtual int GetFragmentSeriesIndex(StackedFragmentSeries series)
        {
            return Index < 0 || ActualSeries == null || ActualSeries.Count == 0 ?
                -1 : Index + ActualSeries.IndexOf(series);
        }

        /// <summary>
        /// Returns the positoin of the item in the data source.
        /// </summary>
        /// <param name="world">The point used to find the item.</param>
        internal virtual int GetFragmentItemIndex(Point world)
        {
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = View.Viewport;

            int rowIndex = -1;
            CategoryAxisBase xAxis = this.GetXAxis() as CategoryAxisBase;
            if (xAxis != null && !windowRect.IsEmpty && !viewportRect.IsEmpty)
            {
                double left = xAxis.GetUnscaledValue(viewportRect.Left, windowRect, viewportRect, xAxis.CategoryMode);
                double right = xAxis.GetUnscaledValue(viewportRect.Right, windowRect, viewportRect, xAxis.CategoryMode);

                double windowX = (world.X - windowRect.Left) / windowRect.Width;
                double bucket = left + (windowX * (right - left));
                if (xAxis.CategoryMode != CategoryMode.Mode0)
                {
                    bucket -= .5;
                }
                int bucketNumber = (int)Math.Round(bucket);

                //the row index is the bucket number at this point. It doesn't depend on the bucket size, because 
                //GetUnscaledValue uses total items count
                //rowIndex = bucketNumber * BucketSize;

                rowIndex = bucketNumber;
            }
            return rowIndex;
        }

        /// <summary>
        /// Validates the Fragment visual.
        /// </summary>
        internal virtual bool ValidateFragmentSeries(AnchoredCategorySeries series, Rect viewportRect, Rect windowRect, SeriesView view)
        {
            bool isValid = true;
            CategoryAxisBase xAxis = this.GetXAxis() as CategoryAxisBase;
            NumericYAxis yAxis = this.GetYAxis() as NumericYAxis;
            if (!view.HasSurface()
                || windowRect.IsEmpty
                || viewportRect.IsEmpty
                || xAxis == null
                || xAxis.ItemsSource == null
                || yAxis == null
                || FastItemsSource == null
                || xAxis.SeriesViewer == null
                || yAxis.SeriesViewer == null
                || yAxis.ActualMinimumValue == yAxis.ActualMaximumValue)
            {
                isValid = false;
            }

            var categoryView = (CategorySeriesView)view;
            int bucketSize = categoryView.BucketCalculator.BucketSize;
            if (series.ValueColumn == null
                || series.ValueColumn.Count == 0
                || bucketSize < 1
                || series.Visibility != Visibility.Visible)
            {
                isValid = false;
            }
            return isValid;
        }

        /// <summary>
        /// Creates a list of series if the data source is a GroupBy.
        /// </summary>
        private void GenerateSeries()
        {
            if (SeriesViewer == null || StackedSeriesManager == null) return;
            
            //clearing autogenerated series should also clear out the visuals.
            AutoGeneratedSeries.Clear();

            if (!AutoGenerateSeries)
            {
                //There might be series in the Series collection that need to be re-added.
                foreach (var series in Series)
                {
                    if (!StackedSeriesManager.SeriesLogical.Contains(series))
                    {
                        StackedSeriesManager.SeriesLogical.Add(series);
                    }
                }
                UpdateAxisRanges();
                return;
            }


            GroupBy groupBy = ItemsSource as GroupBy;

            if (groupBy == null || string.IsNullOrEmpty(groupBy.ValueMemberPath)) return;

            for (int i = 0; i < groupBy.NumberOfSeries; i++)
            {
                string valueMemberPath = groupBy.SeriesKeys[i] + "_" + groupBy.ValueMemberPath;
                StackedFragmentSeries series = new StackedFragmentSeries
                {
                    ParentSeries = this,
                    Chart = SeriesViewer,
                    ValueMemberPath = valueMemberPath,
                };

                AutoGeneratedSeries.Add(series);

                //set index after the series is added to AutoGeneratedSeries, or it will be -1
                series.Index = GetFragmentSeriesIndex(series);

                StackedSeriesManager.SeriesLogical.Add(series);

                StackedSeriesCreatedEventArgs seriesCreatedEventArgs = new StackedSeriesCreatedEventArgs(series);
                if (SeriesCreated != null)
                {
                    SeriesCreated(series, seriesCreatedEventArgs);
                }
            }

            RenderSeries(false);

        }

        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);
            StackedSeriesManager.RenderSeries();
        }

        internal virtual void RenderFragment(AnchoredCategorySeries series, CategoryFrame frame, CategorySeriesView view)
        {

        }

        /// <summary>
        /// Calculates the value column and min and max values.
        /// </summary>
        protected virtual void PrepareData()
        {
            if (FastItemsSource == null)
                return;

            Highs = new double[FastItemsSource.Count];
            Lows = new double[FastItemsSource.Count];

            Minimum = double.PositiveInfinity;
            Maximum = double.NegativeInfinity;

            //double zero = GetUnscaledWorldZeroValue();
            double zero = 0.0;

            foreach (var series in ActualSeries)
            {
                FastItemsSource.DeregisterColumn(series.ValueColumn);
                series.ValueColumn = FastItemsSource.RegisterColumn(series.ValueMemberPath);
                series.Positive = true;

                if (series.ValueColumn != null)
                {
                    series.HighValues.Clear();
                    series.LowValues.Clear();

                    for (int i = 0; i < series.ValueColumn.Count; i++)
                    {
                        double value = series.ValueColumn[i];

                        if (value < zero)
                        {
                            series.HighValues.Add(zero);
                            series.LowValues.Add(Lows[i]);
                            Lows[i] += value;
                            if (series.Positive) series.Positive = false;
                        }
                        else if (value >= zero)
                        {
                            series.HighValues.Add(Highs[i]);
                            series.LowValues.Add(zero);
                            Highs[i] += value;
                        }
                        else if (double.IsNaN(value) || double.IsInfinity(value))
                        {
                            series.HighValues.Add(Highs[i]);
                            series.LowValues.Add(Lows[i]);
                        }
                    }
                }
            }

            for (int i = 0; i < FastItemsSource.Count; i++)
            {
                Minimum = Math.Min(Minimum, Lows[i]);
                Maximum = Math.Max(Maximum, Highs[i]);
            }
        }

        #endregion

        #region Property Updates
        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case CategorySeries.SeriesViewerPropertyName:
                    if (ActualSeries != null)
                    {
                        foreach (var series in ActualSeries)
                        {
                            series.Chart = SeriesViewer;
                        }
                        StackedSeriesManager.RenderSeries();
                    }
                    break;

                case CategorySeries.FastItemsSourcePropertyName:
                    if (AutoGenerateSeries)
                    {
                        GenerateSeries();
                    }

                    if (!this.UpdateNumericAxisRange())
                    {
                        StackedView.BucketCalculator.CalculateBuckets(Resolution);
                    }

                    RenderSeries(false);
                    break;

                case AutoGenerateSeriesPropertyName:
                    GenerateSeries();
                    break;

                case ReverseLegendOrderPropertyName:
                    if (SeriesViewer != null) SeriesViewer.OnLegendSortChanged(StackedSeriesManager.SeriesVisual);
                    break;

                case SeriesVisibilityPropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateVisibility();
                    }
                    break;

                case BrushPropertyName:
                    foreach (var series in ActualSeries) series.UpdateBrush();
                    break;

                case DashArrayPropertyName:
                    foreach (var series in ActualSeries) series.UpdateDashArray();
                    break;

                case DashCapPropertyName:
                    foreach (var series in ActualSeries) series.UpdateDashCap();
                    break;


                case SeriesCursorPropertyName:
                    foreach (var series in ActualSeries) series.UpdateCursor();
                    break;

                case SeriesEffectPropertyName:
                    foreach (var series in ActualSeries) series.UpdateEffect();
                    break;


                case EndCapPropertyName:
                    foreach (var series in ActualSeries) series.UpdateEndCap();
                    break;

                case "IsHitTestVisible":
                    foreach (var series in ActualSeries) series.UpdateIsHitTestVisible();
                    break;

                case LegendItemBadgeTemplatePropertyName:
                    foreach (var series in ActualSeries) series.UpdateLegendItemBadgeTemplate();
                    break;

                case LegendItemTemplatePropertyName:
                    foreach (var series in ActualSeries) series.UpdateLegendItemTemplate();
                    break;

                case LegendItemVisibilityPropertyName:
                    foreach (var series in ActualSeries) series.UpdateLegendItemVisibility();
                    break;

                case MarkerTemplatePropertyName:
                    foreach (var series in ActualSeries) series.UpdateMarkerTemplate();
                    break;

                case MarkerTypePropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateMarkerType();
                    }
                    break;

                case MarkerBrushPropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateMarkerBrush();
                    }
                    break;

                case MarkerOutlinePropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateMarkerOutline();
                    }
                    break;

                case MarkerStylePropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateMarkerStyle();
                    }
                    break;

                case "Opacity":
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateOpacity();
                    }
                    break;

                case "OpacityMask":
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateOpacityMask();
                    }
                    break;

                case OutlinePropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateOutline();
                    }
                    break;

                case StartCapPropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateStartCap();
                    }
                    break;

                case ThicknessPropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateThickness();
                    }
                    break;

                case ToolTipPropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateToolTip();
                    }
                    break;


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

                case UseLightweightMarkersPropertyName:
                    foreach (var series in ActualSeries)
                    {
                        series.UpdateUseLightweightMarkers();
                    }
                    break;
            }
        }

        #endregion

        #region IISCategoryBased implementation
        CategoryMode IIsCategoryBased.CurrentCategoryMode
        {
            get { return PreferredCategoryMode(this.GetXAxis() as CategoryAxisBase); }
        }

        ICategoryScaler IIsCategoryBased.Scaler
        {
            get { return this.GetXAxis() as ICategoryScaler; }
        }

        IScaler IIsCategoryBased.YScaler
        {
            get { return this.GetYAxis() as IScaler; }
        }

        IBucketizer IIsCategoryBased.Bucketizer
        {
            get { return this.CategoryView.BucketCalculator; }
        }

        int IIsCategoryBased.CurrentMode2Index
        {
            get { return GetMode2Index(); }
        }

        IDetectsCollisions IIsCategoryBased.ProvideCollisionDetector()
        {
            return new CollisionAvoider();
        }
        #endregion

        /// <summary>
        /// Renders the thumbnail image for the OPD pane.
        /// </summary>
        /// <param name="viewportRect">The viewport to use.</param>
        /// <param name="surface">The render target.</param>
        protected internal override void RenderThumbnail(Rect viewportRect, RenderSurface surface)
        {
            var dirty = ThumbnailDirty;

            base.RenderThumbnail(viewportRect, surface);

            if (!dirty)
            {
                View.PrepSurface(surface);
                return;
            }

            View.PrepSurface(surface);
            if (ClearAndAbortIfInvalid(ThumbnailView))
            {
                return;
            }

            foreach (StackedFragmentSeries fragment in this.Series)
            {
                fragment.VisualSeriesLink.RenderThumbnail(viewportRect, surface);
            }

            ThumbnailDirty = false;
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