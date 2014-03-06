using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Windows.Data;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base functionality for a XamDataChart financial indicator series.
    /// </summary>
    [WidgetModuleParent("FinancialChart")]
    public abstract class FinancialIndicator : FinancialSeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new FinancialIndicatorView(this);
        }
        
        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            IndicatorView = (FinancialIndicatorView)view;
        }
        internal FinancialIndicatorView IndicatorView { get; set; }

        #region Constructor and Initialisation
        /// <summary>
        /// Initializes a new instance of the FinancialIndicator class. 
        /// </summary>
        internal FinancialIndicator()
        {
            PreviousFrame = new CategoryFrame(3);
            TransitionFrame = new CategoryFrame(3);
            CurrentFrame = new CategoryFrame(3);

            IndicatorColumn = new List<double>();
            IndicatorRange = new AxisRange(-100, 100);
        }
        
        #endregion

        internal void EnsureYRangeThenRender(bool animate)
        {
            if (YAxis != null && !YAxis.UpdateRange())
            {
                RenderSeries(true);
            }
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
            if (XAxis != null && XAxis is ISortingAxis)
            {
                (XAxis as ISortingAxis).NotifyDataChanged();
            }

            this.IndicatorView.TrendLineManager.DataUpdated(action, position, count, propertyName);

            if (propertyName != null && mappingToColumnName.ContainsKey(propertyName))
            {
                mappingToColumnName.TryGetValue(propertyName, out propertyName);
            }

            if (XAxis != null && XAxis is ISortingAxis)
            {
                
                //to update if the original indices are different than the sorted.
                action = FastItemsSourceEventAction.Reset;
                position = 0;
                count = FastItemsSource.Count;
            }

            switch (action)
            {
                case FastItemsSourceEventAction.Change:
                    if (ShouldUpdateIndicator(position, count, propertyName))
                    {
                        UpdateIndicator(position, count, propertyName);
                        EnsureYRangeThenRender(true);
                    }
                    break;

                case FastItemsSourceEventAction.Replace:
                    if (ShouldUpdateIndicator(position, FastItemsSource.Count - position, propertyName))
                    {
                        UpdateIndicator(position, FastItemsSource.Count - position, propertyName);
                        EnsureYRangeThenRender(true);
                    }
                    break;

                case FastItemsSourceEventAction.Insert:
                    if (ShouldUpdateIndicator(position, FastItemsSource.Count - position, propertyName))
                    {
                        IndicatorColumn.InsertRange(position, new double[count]);
                        UpdateIndicator(position, FastItemsSource.Count - position, propertyName);
                        EnsureYRangeThenRender(true);
                    }
                    break;

                case FastItemsSourceEventAction.Remove:
                    if (ShouldUpdateIndicator(position, FastItemsSource.Count - position, propertyName))
                    {
                        IndicatorColumn.RemoveRange(position, count);
                        UpdateIndicator(position, FastItemsSource.Count - position, propertyName);
                        EnsureYRangeThenRender(true);
                    }
                    break;

                case FastItemsSourceEventAction.Reset:
                    if (ShouldUpdateIndicator(position, FastItemsSource.Count - position, propertyName))
                    {
                        IndicatorColumn = new List<double>(FastItemsSource.Count);
                        IndicatorColumn.InsertRange(0, new double[count]);
                        UpdateIndicator(position, FastItemsSource.Count - position, propertyName);
                        EnsureYRangeThenRender(true);
                    }
                    break;
            }
        }

        #region DisplayType Dependency Property
        /// <summary>
        /// Gets or sets the display for the current FinancialIndicator object.
        /// <para/>
        /// This is a dependency property.
        /// </summary>
        [SuppressWidgetMemberCopy]
        public IndicatorDisplayType DisplayType
        {
            get
            {
                return (IndicatorDisplayType)GetValue(DisplayTypeProperty);
            }
            set
            {
                SetValue(DisplayTypeProperty, value);
            }
        }

        internal const string DisplayTypePropertyName = "DisplayType";

        /// <summary>
        /// Identifies the DisplayType dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayTypeProperty = DependencyProperty.Register(DisplayTypePropertyName, typeof(IndicatorDisplayType), typeof(FinancialIndicator),
            new PropertyMetadata(IndicatorDisplayType.Line, (sender, e) =>
            {
                (sender as FinancialIndicator).RaisePropertyChanged(DisplayTypePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region IgnoreFirst Depedency Property
        /// <summary>
        /// Gets or sets the number of values to hide at the beginning of the indicator.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public int IgnoreFirst
        {
            get
            {
                return (int)GetValue(IgnoreFirstProperty);
            }
            set
            {
                SetValue(IgnoreFirstProperty, value);
            }
        }

        internal const string IgnoreFirstPropertyName = "IgnoreFirst";

        /// <summary>
        /// Identifies the IgnoreFirst dependency property.
        /// </summary>
        public static readonly DependencyProperty IgnoreFirstProperty = DependencyProperty.Register(IgnoreFirstPropertyName, typeof(int), typeof(FinancialSeries),
            new PropertyMetadata(0, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as FinancialIndicator).RaisePropertyChanged(IgnoreFirstPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLine related

        #region TrendLineType Dependency Property
        /// <summary>
        /// Gets or sets the trend type for the current indicator series.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public TrendLineType TrendLineType
        {
            get
            {
                return (TrendLineType)GetValue(TrendLineTypeProperty);
            }
            set
            {
                SetValue(TrendLineTypeProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TrendLineType dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineTypeProperty = DependencyProperty.Register(TrendLineTypePropertyName, typeof(TrendLineType), typeof(FinancialIndicator),
            new PropertyMetadata(TrendLineType.None, (sender, e) =>
            {
                (sender as FinancialIndicator).RaisePropertyChanged(TrendLineTypePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion

        #region TrendLineBrush Dependency Property
        /// <summary>
        /// Gets or sets the brush to use to draw the trend line.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush TrendLineBrush
        {
            get
            {
                return (Brush)GetValue(TrendLineBrushProperty);
            }
            set
            {
                SetValue(TrendLineBrushProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TrendLineBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineBrushProperty = DependencyProperty.Register(TrendLineBrushPropertyName, typeof(Brush), typeof(FinancialIndicator),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as FinancialIndicator).RaisePropertyChanged(TrendLineBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion
        #region ActualTrendLineBrush Dependency Property
        /// <summary>
        /// Gets the effective TrendLineBrush for this indicator.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush ActualTrendLineBrush
        {
            get
            {
                return (Brush)GetValue(ActualTrendLineBrushProperty);
            }
            internal set
            {
                SetValue(ActualTrendLineBrushProperty, value);
            }
        }

        /// <summary>
        /// Identifies the ActualTrendLineBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualTrendLineBrushProperty = DependencyProperty.Register(TrendLineActualBrushPropertyName, typeof(Brush), typeof(FinancialIndicator),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as FinancialIndicator).RaisePropertyChanged(TrendLineActualBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion
        #region TrendLineThickness Property
        /// <summary>
        /// Gets or sets the thickness of the current indicator object's trend line.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double TrendLineThickness
        {
            get
            {
                return (double)GetValue(TrendLineThicknessProperty);
            }
            set
            {
                SetValue(TrendLineThicknessProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TrendLineThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineThicknessProperty = DependencyProperty.Register(TrendLineThicknessPropertyName, typeof(double), typeof(FinancialIndicator),
            new PropertyMetadata(1.5, (sender, e) =>
            {
                (sender as FinancialIndicator).RaisePropertyChanged(TrendLineThicknessPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineDashCap Property
        /// <summary>
        /// Gets or sets the PenLineCap enumeration value that specifies how the current
        /// indicator object's trend line dash ends are drawn. 
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public PenLineCap TrendLineDashCap
        {
            get
            {
                return (PenLineCap)GetValue(TrendLineDashCapProperty);
            }
            set
            {
                SetValue(TrendLineDashCapProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TrendLineDashCap dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineDashCapProperty = DependencyProperty.Register(TrendLineDashCapPropertyName, typeof(PenLineCap), typeof(FinancialIndicator),
            new PropertyMetadata((sender, e) =>
            {
                (sender as FinancialIndicator).RaisePropertyChanged(TrendLineDashCapPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineDashArray Property
        /// <summary>
        /// Gets or sets a collection of double values that indicate the pattern of dashes and gaps that
        /// is used to draw the trend line for the current indicator object. 
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public DoubleCollection TrendLineDashArray
        {
            get
            {
                return (DoubleCollection)GetValue(TrendLineDashArrayProperty);
            }
            set
            {
                SetValue(TrendLineDashArrayProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TrendLineDashArray dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineDashArrayProperty = DependencyProperty.Register(TrendLineDashArrayPropertyName, typeof(DoubleCollection), typeof(FinancialIndicator),
            new PropertyMetadata((sender, e) =>
            {
                (sender as FinancialIndicator).RaisePropertyChanged(TrendLineDashArrayPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLinePeriod Dependency Property
        /// <summary>
        /// Gets or sets the trend line period for the current series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The typical, and initial, value for trend line period is 7.
        /// </remarks>
        public int TrendLinePeriod
        {
            get
            {
                return (int)GetValue(TrendLinePeriodProperty);
            }
            set
            {
                SetValue(TrendLinePeriodProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TrendLinePeriod dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLinePeriodProperty = DependencyProperty.Register(TrendLinePeriodPropertyName, typeof(int), typeof(FinancialIndicator),
            new PropertyMetadata(7, (sender, e) =>
            {
                (sender as FinancialIndicator).RaisePropertyChanged(TrendLinePeriodPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Should be overridden in derived classes to specify a value 
        /// to use for the trendlineperiod regardless of what is set on the 
        /// series.
        /// </summary>
        /// <returns>The to use for the period.</returns>
        protected virtual int TrendPeriodOverride()
        {
            return -1;
        }
        #endregion

        #region TrendLineZIndex Dependency Property
        /// <summary>
        /// Identifies the TrendLineZIndex dependency property
        /// </summary>
        public static readonly DependencyProperty TrendLineZIndexProperty =
            DependencyProperty.Register(TrendLineZIndexPropertyName,
            typeof(int), typeof(FinancialIndicator), new PropertyMetadata(1001, (sender, e) =>
            {
                (sender as FinancialIndicator).RaisePropertyChanged(
                    TrendLineZIndexPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the trend line z index for the current series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The trend line renders over the series and markers by default, lower this value to shift it to the background.
        /// </remarks>
        /// </summary>
        public int TrendLineZIndex
        {
            get
            {
                return (int)this.GetValue(FinancialIndicator.TrendLineZIndexProperty);
            }
            set
            {
                this.SetValue(FinancialIndicator.TrendLineZIndexProperty, value);
            }
        }
        #endregion

        #region IndicatorColumn PseudoColumn Property
        /// <summary>
        /// The calculated indicator values
        /// </summary>
        protected internal List<double> IndicatorColumn { get; private set; }
        //protected bool IndicatorColumnValid { get; private set; }
        /// <summary>
        /// The Y range defined by the Indicator values.
        /// </summary>
        protected AxisRange IndicatorRange { get; set; }

        /// <summary>
        /// Overridden in derived classes to calculate the value of the indicator based on the input columns.
        /// </summary>
        /// <param name="position">The starting position to calculate from.</param>
        /// <param name="count">The number of positions to calculate.</param>
        /// <returns>Whether updates are required.</returns>
        protected abstract bool IndicatorOverride(int position, int count);

        /// <summary>
        /// Overridden in derived classes to provide a list of column names on which the calculation depends.
        /// </summary>
        /// <param name="position">The starting position of the calculation.</param>
        /// <param name="count">The number of positions required to be calculated.</param>
        /// <returns>The list of columns on which the calculation depends.</returns>
        protected abstract IList<string> BasedOn(int position, int count);

        #endregion
        #endregion

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
            switch (propertyName)
            {
                case XAxisPropertyName:
                    this.IndicatorView.TrendLineManager = CategoryTrendLineManagerBase.SelectManager(
                        this.IndicatorView.TrendLineManager, XAxis, RootCanvas, this);
                    break;
            }

            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            if (this.IndicatorView.TrendLineManager.PropertyUpdated(sender, propertyName, oldValue, newValue,
                TrendLineDashArray))
            {
                RenderSeries(false);
                this.NotifyThumbnailAppearanceChanged();
            }

            switch (propertyName)
            {
                case FastItemsSourcePropertyName:
                    if (FastItemsSource != null)
                    {
                        IndicatorColumn = new List<double>(FastItemsSource.Count);
                        IndicatorColumn.InsertRange(0, new double[FastItemsSource.Count]);
                        UpdateIndicator(0, FastItemsSource.Count, null);

                        if (YAxis != null && !YAxis.UpdateRange())
                        {
                            this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                            RenderSeries(false);
                        }
                    }
                    break;

                case TrendLineBrushPropertyName:
                    this.UpdateIndexedProperties();
                    break;

                case DisplayTypePropertyName:
                    ClearRendering(true, View);
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case OpenColumnPropertyName:
                case HighColumnPropertyName:
                case LowColumnPropertyName:
                case CloseColumnPropertyName:
                case VolumeColumnPropertyName:
                    if (FastItemsSource != null)
                    {
                        if (IndicatorColumn.Count != FastItemsSource.Count)
                        {
                            IndicatorColumn = new List<double>(FastItemsSource.Count);
                            IndicatorColumn.InsertRange(0, new double[FastItemsSource.Count]);
                        }
                        if (ShouldUpdateIndicator(0, FastItemsSource.Count - 1, propertyName))
                        {
                            FullIndicatorRefresh();
                        }
                    }
                    break;
                case XAxisPropertyName:
                    if (XAxis != null &&
                        (XAxis is ISortingAxis || oldValue is ISortingAxis))
                    {
                        FullIndicatorRefresh();
                    }
                    break;
                case YAxisPropertyName:
                case IgnoreFirstPropertyName:
                    FullIndicatorRefresh();
                    this.NotifyThumbnailAppearanceChanged();
                    break;
                case TrendLineTypePropertyName:
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }

        private bool ShouldUpdateIndicator(int position, int count, string updatedPropertyName)
        {
            if (updatedPropertyName == null)
            {
                return true;
            }
            if (BasedOn(position, count).Contains(updatedPropertyName))
            {
                return true;
            }

            return false;
        }

        private void UpdateIndicator(int position, int count, string updatedPropertyName)
        {
            IndicatorOverride(position, count);
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (FastItemsSource == null)
            {
                return null;
            }

            if (axis != null && axis == YAxis)
            {
                return IndicatorRange;
            }

            return null;
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
            //Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            //Rect viewportRect = windowRect != null ? View.Viewport : Rect.Empty;
            
            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;

            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, YAxis.IsInverted);

            if (index >= 0 && windowRect != null && viewportRect != null)
            {
                if (XAxis != null)
                {
                    double cx = XAxis.GetScaledValue(index, xParams);

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

                if (YAxis != null && IndicatorColumn != null && index < IndicatorColumn.Count)
                {
                    double cy = YAxis.GetScaledValue(IndicatorColumn[index], yParams);

                    if (cy < windowRect.Top + 0.1 * windowRect.Height)
                    {
                        cy = cy + 0.4 * windowRect.Height;
                    }

                    if (cy > windowRect.Bottom - 0.1 * windowRect.Height)
                    {
                        cy = cy - 0.4 * windowRect.Height;
                    }

                    windowRect.Y = cy - 0.5 * windowRect.Height;
                }

                SyncLink.WindowNotify(SeriesViewer, windowRect);
            }

            return index >= 0;
        }

        /// <summary>
        /// Checks if the series is valid to be rendered.
        /// </summary>
        /// <param name="viewportRect">The current viewport, a rectangle with bounds equivalent to the screen size of the series.</param>
        /// <param name="windowRect">The current window, a rectangle bounded between 0 and 1 representing the pan and zoom position.</param>
        /// <param name="view">The SeriesView in context.</param>
        /// <returns>True if the series is valid to be rendered, otherwise false.</returns>
        protected internal override bool ValidateSeries(Rect viewportRect, Rect windowRect, SeriesView view)
        {
            bool isValid = base.ValidateSeries(viewportRect, windowRect, view);
            if (IndicatorColumn == null || IndicatorColumn.Count == 0)
            {
                isValid = false;
            }
            return isValid;
        }

        internal override void PrepareFrame(CategoryFrame frame, FinancialSeriesView view)
        {
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            
            CategoryAxisBase xaxis = XAxis;
            NumericYAxis yaxis = YAxis;
            
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, xaxis.IsInverted);
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, yaxis.IsInverted);

            frame.Buckets.Clear();
            frame.Markers.Clear();
            frame.Trend.Clear();

            double offset = 0.0;            // offset (pels) to the center of this categorySeries

            ISortingAxis sortingXAxis = XAxis as ISortingAxis;
            if (sortingXAxis != null && sortingXAxis.SortedIndices.Count != FastItemsSource.Count)
            {
                //mismatch in series and axis data sources.
                return;
            }

            #region work out the category mode and offset
            CategoryMode categoryMode = XAxis.CategoryMode;

            switch (categoryMode)
            {
                case CategoryMode.Mode0:    // use bucket.X as-is
                    offset = 0.0;
                    break;

                case CategoryMode.Mode1:    // offset x by half category width
                    offset = 0.5 * XAxis.GetCategorySize(windowRect, viewportRect);
                    break;

                case CategoryMode.Mode2:    // offset x by the appropriate amount for this categorySeries
                    offset = XAxis.GetGroupCenter(Index, windowRect, viewportRect);
                    break;
            }
            #endregion

            int _trendPeriod = TrendPeriodOverride();
            if (_trendPeriod == -1)
            {
                _trendPeriod = TrendLinePeriod;
            }
            FinancialIndicatorView indicatorView = view as FinancialIndicatorView;
            indicatorView.TrendLineManager.PrepareLine(frame.Trend, TrendLineType, IndicatorColumn,
                _trendPeriod, (x) => XAxis.GetScaledValue(x, xParams),
                (y) => YAxis.GetScaledValue(y, yParams),
                new TrendResolutionParams()
                {
                    BucketSize = view.BucketCalculator.BucketSize,
                    FirstBucket = view.BucketCalculator.FirstBucket,
                    LastBucket = view.BucketCalculator.LastBucket,
                    Offset = offset,
                    Resolution = Resolution,
                    Viewport = viewportRect,
                    Window = windowRect
                });

            #region bucketize data
            float singlePixelSpan = ConvertToSingle(this.XAxis.GetUnscaledValue(2.0, xParams) - this.XAxis.GetUnscaledValue(1.0, xParams));

            for (int i = view.BucketCalculator.FirstBucket; i <= view.BucketCalculator.LastBucket; ++i)
            {
                float[] bucket;
                if (sortingXAxis == null)
                {
                    //index based bucketing
                    bucket = view.BucketCalculator.GetBucket(i);
                }
                else
                {
                    // SortedAxis based bucketing (for CategoryDateTimeXAxis)
                    int index = sortingXAxis.SortedIndices[i];
                    double bucketX = sortingXAxis.GetUnscaledValueAt(index);
                    float bucketY0 = ConvertToSingle(this.IndicatorColumn[i]);
                    float bucketY1 = bucketY0;
                    double currentX = bucketX;

                    while (i < view.BucketCalculator.LastBucket)
                    {
                        index = sortingXAxis.SortedIndices[i + 1];
                        currentX = sortingXAxis.GetUnscaledValueAt(index);
                        if (currentX - bucketX > singlePixelSpan)
                        {
                            // next item does not belong in this bucket
                            break;
                        }

                        // add next item to this bucket
                        i++;
                        float y = ConvertToSingle(this.IndicatorColumn[i]);
                        bucketY0 = Math.Min(bucketY0, y);
                        bucketY1 = Math.Max(bucketY1, y);

                    }

                    double xVal = double.NaN;
                    if (!double.IsNaN(bucketX))
                    {
                        xVal = this.XAxis.GetScaledValue(bucketX, xParams);
                    }

                    bucket = new float[] { ConvertToSingle(xVal), bucketY0, bucketY1 };
                }

                if (!float.IsNaN(bucket[0]))
                {
                    if (XAxis != null && XAxis is ISortingAxis)
                    {
                        bucket[0] = (float)(bucket[0] + offset);
                    }
                    else
                    {
                        bucket[0] = (float)(xaxis.GetScaledValue(bucket[0], xParams) + offset);
                    }
                    bucket[1] = (float)yaxis.GetScaledValue(bucket[1], yParams);

                    if (view.BucketCalculator.BucketSize > 1 || sortingXAxis != null)
                    {
                        bucket[2] = (float)yaxis.GetScaledValue(bucket[2], yParams);
                    }
                    else
                    {
                        bucket[2] = bucket[1];
                    }

                    frame.Buckets.Add(bucket);
                }
            }
            #endregion

            return;
        }

        private float ConvertToSingle(double p)
        {



            return Convert.ToSingle(p);

        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var indicatorView = (FinancialIndicatorView)view;
            indicatorView.ClearIndicatorVisual(wipeClean);
        }



        internal override void RenderFrame(CategoryFrame frame, FinancialSeriesView view)
        {
            //ClearRendering(false);
            FinancialIndicatorView indicatorView = view as FinancialIndicatorView;
            indicatorView.ClearIndicatorVisual(false);

            Func<int, double> x0 = delegate(int i) { return frame.Buckets[i][0]; };
            Func<int, double> y0 = delegate(int i) { return frame.Buckets[i][1]; };
            Func<int, double> x1 = delegate(int i) { return frame.Buckets[i][0]; };
            Func<int, double> y1 = delegate(int i) { return frame.Buckets[i][2]; };

            
            indicatorView.TrendLineManager.RasterizeTrendLine(frame.Trend);

            Rect viewportRect = view.Viewport;

            double zero = 0.0;

            if (frame.Buckets.Count > 0)
            {
                switch (DisplayType)
                {
                    case IndicatorDisplayType.Line:
                        indicatorView.RasterizeLine(frame.Buckets.Count, x0, y0, x1, y1, true);
                        break;

                    case IndicatorDisplayType.Area:
                        if (YAxis != null)
                        {
                            zero = GetWorldZeroValue(view);
                        }
                        else
                        {
                            zero = 0.5 * (viewportRect.Top + viewportRect.Bottom);
                        }

                        indicatorView.RasterizeArea(frame.Buckets.Count, x0, y0, x1, y1, true, zero);
                        break;

                    case IndicatorDisplayType.Column:
                        zero = GetWorldZeroValue(view);
                        indicatorView.RasterizeColumns(frame.Buckets.Count, x0, y0, x1, y1, true, zero);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            indicatorView.UpdateHitTests();
        }

        

        internal virtual double GetWorldZeroValue(FinancialSeriesView view)
        {
            double value = 0.0;

            //Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            //Rect viewportRect = View.Viewport;

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            if (!windowRect.IsEmpty && !viewportRect.IsEmpty && YAxis != null)
            {
                ScalerParams yParams = new ScalerParams(windowRect, viewportRect, YAxis.IsInverted);
                value = YAxis.GetScaledValue(YAxis.ReferenceValue, yParams);
            }

            return value;
        }       

        /// <summary>
        /// Updates properties that are based on the index of the series in the series collection.
        /// </summary>
        protected override void UpdateIndexedProperties()
        {
            base.UpdateIndexedProperties();

            if (Index < 0)
            {
                return;
            }

            #region ActualTrendLineBrush
            IndicatorView.UpdateTrendlineBrush();
            #endregion
        }

        /// <summary>
        /// Refreshes all the indicator values.
        /// </summary>
        protected void FullIndicatorRefresh()
        {
            this.IndicatorView.TrendLineManager.Reset();
            IndicatorOverride(0, IndicatorColumn.Count);
            if (YAxis != null && !YAxis.UpdateRange())
            {
                RenderSeries(false);
            }
        }

        /// <summary>
        /// Exports visual information about the series for use by external tools and functionality.
        /// </summary>
        /// <param name="svd">The data container.</param>
        protected override void ExportVisualDataOverride(SeriesVisualData svd)
        {
            base.ExportVisualDataOverride(svd);

            var trendShape = new PolyLineVisualData(
                "trendLine",
                IndicatorView.TrendLineManager.TrendPolyline);

            trendShape.Tags.Add("Trend");
            svd.Shapes.Add(trendShape);
        }
    }

    internal class FinancialIndicatorBucketCalculator
        : FinancialBucketCalculator
    {
        internal FinancialIndicatorBucketCalculator(FinancialSeriesView view)
            : base(view)
        {
            IndicatorView = (FinancialIndicatorView)view;
        }

        protected FinancialIndicatorView IndicatorView { get; set; }

        public override float[] GetBucket(int index)
        {
            int i0 = index * this.BucketSize;
            int i1 = Math.Min(i0 + this.BucketSize - 1, IndicatorView.IndicatorModel.IndicatorColumn.Count - 1);

            double min = double.NaN;
            double max = double.NaN;

            for (int i = i0; i <= i1; ++i)
            {
                double y = IndicatorView.IndicatorModel.IndicatorColumn[i];

                if (!double.IsNaN(min))
                {
                    if (!double.IsNaN(y))
                    {
                        min = Math.Min(min, y);
                        max = Math.Max(max, y);
                    }
                }
                else
                {
                    min = y;
                    max = y;
                }
            }

            if (!double.IsNaN(min))
            {
                return new float[] { (float)(0.5 * (i0 + i1)), (float)min, (float)max };
            }

            return new float[] { float.NaN, float.NaN, float.NaN };
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