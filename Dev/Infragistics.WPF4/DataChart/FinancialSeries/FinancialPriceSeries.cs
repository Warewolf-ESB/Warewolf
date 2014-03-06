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
using Infragistics.Controls.Charts.Util;
using System.Windows.Data;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart financial price series that renders as Candlestick or OHLC representations.
    /// </summary>
    /// <remarks>
    /// Default required members: Open, Low, High, Close
    /// </remarks>
    [WidgetModuleParent("FinancialChart")]
    public class FinancialPriceSeries : FinancialSeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new FinancialPriceSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            FinancialPriceView = (FinancialPriceSeriesView)view;
        }
        internal FinancialPriceSeriesView FinancialPriceView { get; set; }

        #region Constructor and Initialisation
        /// <summary>
        /// Initializes a new instance of the FinancialIndicator class. 
        /// </summary>
        public FinancialPriceSeries()
        {
            DefaultStyleKey = typeof(FinancialPriceSeries);

            PreviousFrame = new CategoryFrame(5);
            TransitionFrame = new CategoryFrame(5);
            CurrentFrame = new CategoryFrame(5);
        }

        #endregion

        #region Trendline related

        #region TrendLineType Dependency Property
        /// <summary>
        /// Gets or sets the trend type for the current financial series.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultString("none")]
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
        public static readonly DependencyProperty TrendLineTypeProperty = DependencyProperty.Register(TrendLineTypePropertyName, typeof(TrendLineType), typeof(FinancialPriceSeries),
            new PropertyMetadata(TrendLineType.None, (sender, e) =>
            {
                (sender as FinancialPriceSeries).RaisePropertyChanged(TrendLineTypePropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty TrendLineBrushProperty = DependencyProperty.Register(TrendLineBrushPropertyName, typeof(Brush), typeof(FinancialPriceSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as FinancialPriceSeries).RaisePropertyChanged(TrendLineBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion
        #region ActualTrendLineBrush Dependency Property
        /// <summary>
        /// Gets the effective TrendLineBrush for this FinancialPriceSeries.
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
        public static readonly DependencyProperty ActualTrendLineBrushProperty = DependencyProperty.Register(TrendLineActualBrushPropertyName, typeof(Brush), typeof(FinancialPriceSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as FinancialPriceSeries).RaisePropertyChanged(TrendLineActualBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion
        #region TrendLineThickness Property
        /// <summary>
        /// Gets or sets the thickness of the current FinancialPriceSeries object's trend line.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [WidgetDefaultNumber(1.5)]
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
        public static readonly DependencyProperty TrendLineThicknessProperty = DependencyProperty.Register(TrendLineThicknessPropertyName, typeof(double), typeof(FinancialPriceSeries),
            new PropertyMetadata(1.5, (sender, e) =>
            {
                (sender as FinancialPriceSeries).RaisePropertyChanged(TrendLineThicknessPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineDashCap Property
        /// <summary>
        /// Gets or sets the PenLineCap enumeration value that specifies how the current
        /// FinancialPriceSeries object's trend line dash ends are drawn. 
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
        public static readonly DependencyProperty TrendLineDashCapProperty = DependencyProperty.Register(TrendLineDashCapPropertyName, typeof(PenLineCap), typeof(FinancialPriceSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as FinancialPriceSeries).RaisePropertyChanged(TrendLineDashCapPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineDashArray Property
        /// <summary>
        /// Gets or sets a collection of double values that indicate the pattern of dashes and gaps that
        /// is used to draw the trend line for the current FinancialPriceSeries object. 
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
        public static readonly DependencyProperty TrendLineDashArrayProperty = DependencyProperty.Register(TrendLineDashArrayPropertyName, typeof(DoubleCollection), typeof(FinancialPriceSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as FinancialPriceSeries).RaisePropertyChanged(TrendLineDashArrayPropertyName, e.OldValue, e.NewValue);
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
        [WidgetDefaultNumber(7)]
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
        public static readonly DependencyProperty TrendLinePeriodProperty = DependencyProperty.Register(TrendLinePeriodPropertyName, typeof(int), typeof(FinancialPriceSeries),
            new PropertyMetadata(7, (sender, e) =>
            {
                (sender as FinancialPriceSeries).RaisePropertyChanged(TrendLinePeriodPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineZIndex Dependency Property
        /// <summary>
        /// Identifies the TrendLineZIndex dependency property
        /// </summary>
        public static readonly DependencyProperty TrendLineZIndexProperty = DependencyProperty.Register(TrendLineZIndexPropertyName, typeof(int), typeof(FinancialPriceSeries), new PropertyMetadata(1001, (sender, e) =>
        {
            (sender as FinancialPriceSeries).RaisePropertyChanged(TrendLineZIndexPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the trend line Z index for the current series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The trend line renders over the series and markers by default, lower this value to shift it to the background.
        /// </remarks>
        /// </summary>
        [WidgetDefaultNumber(1001)]
        public int TrendLineZIndex
        {
            get
            {
                return (int)this.GetValue(FinancialPriceSeries.TrendLineZIndexProperty);
            }
            set
            {
                this.SetValue(FinancialPriceSeries.TrendLineZIndexProperty, value);
            }
        }
        #endregion

        #endregion

        #region Data
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

            this.FinancialPriceView.BucketCalculator.CalculateBuckets(Resolution);

            this.FinancialPriceView.TrendLineManager.DataUpdated(action, position, count, propertyName);

            if (YAxis != null)
            {
                YAxis.UpdateRange();
            }

            RenderSeries(true);
        }
        #endregion

        #region DisplayType Dependency Property
        /// <summary>
        /// Gets or sets the display type for the current FinancialPriceSeries object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [WidgetDefaultString("candlestick")]
        [SuppressWidgetMemberCopy]
        public PriceDisplayType DisplayType
        {
            get
            {
                return (PriceDisplayType)GetValue(DisplayTypeProperty);
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
        public static readonly DependencyProperty DisplayTypeProperty = DependencyProperty.Register(DisplayTypePropertyName, typeof(PriceDisplayType), typeof(FinancialPriceSeries),
            new PropertyMetadata(PriceDisplayType.Candlestick, (sender, e) =>
            {
                (sender as FinancialPriceSeries).RaisePropertyChanged(DisplayTypePropertyName, e.OldValue, e.NewValue);
            }));
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
                    this.FinancialPriceView.TrendLineManager = CategoryTrendLineManagerBase.SelectManager(
                        this.FinancialPriceView.TrendLineManager, XAxis, RootCanvas, this);
                    break;
            }

            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            if (this.FinancialPriceView.TrendLineManager != null && this.FinancialPriceView.TrendLineManager.PropertyUpdated(sender, propertyName, oldValue, newValue, this.TrendLineDashArray))
            {
                RenderSeries(false);
                this.NotifyThumbnailAppearanceChanged();
            }

            switch (propertyName)
            {
                case DisplayTypePropertyName:
                    if (RootCanvas != null)
                    {
                        UpdatePathBrushes();
                        RenderFrame(CurrentFrame, this.FinancialPriceView);
                    }
                    this.NotifyThumbnailAppearanceChanged();
                    break;
                case TrendLineBrushPropertyName:
                    this.UpdateIndexedProperties();
                    break;
                case TrendLineTypePropertyName:
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }

        private void UpdatePathBrushes()
        {
            FinancialPriceView.UpdatePathBrushes();
            if (ThumbnailView != null)
            {
                ((FinancialPriceSeriesView)ThumbnailView).UpdatePathBrushes();
            }
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (axis != null && axis == YAxis && LowColumn != null && !double.IsNaN(LowColumn.Minimum) && HighColumn != null && !double.IsNaN(HighColumn.Maximum))
            {
                return new AxisRange(LowColumn.Minimum, HighColumn.Maximum);
            }

            return null;
        }

        /// <summary>
        /// Scrolls the series to display the item for the specified data item.
        /// </summary>
        /// <remarks>
        /// The categorySeries is scrolled by the minimum amount required to place the specified data item within
        /// the central 80% of the visible axis.
        /// </remarks>
        /// <param name="item">The data item (item) to scroll to.</param>
        /// <returns>True if the specified item could be displayed.</returns>
        public override bool ScrollIntoView(object item)
        {
            int index = FastItemsSource != null ? FastItemsSource[item] : -1;
            //Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            //Rect viewportRect = !windowRect.IsEmpty ? View.Viewport : Rect.Empty;

            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;
            Rect unitRect = new Rect(0, 0, 1, 1);

            ScalerParams yParams = new ScalerParams(unitRect, unitRect, YAxis.IsInverted);

            if (index >= 0 && windowRect != null && viewportRect != null)
            {
                if (XAxis != null)
                {
                    ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);
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

                if (YAxis != null && LowColumn != null && HighColumn != null && index < LowColumn.Count && index < HighColumn.Count)
                {
                    // scroll so that low and high are both in range
                    double low = YAxis.GetScaledValue(LowColumn[index], yParams);
                    double high = YAxis.GetScaledValue(HighColumn[index], yParams);
                    if (!double.IsNaN(low) && !double.IsNaN(high))
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
        /// Checks if the series is valid to be rendered.
        /// </summary>
        /// <param name="viewportRect">The current viewport, a rectangle with bounds equivalent to the screen size of the series.</param>
        /// <param name="windowRect">The current window, a rectangle bounded between 0 and 1 representing the pan and zoom position.</param>
        /// <param name="view">The SeriesView in context.</param>
        /// <returns>True if the series is valid to be rendered, otherwise false.</returns>
        protected internal override bool ValidateSeries(Rect viewportRect, Rect windowRect, SeriesView view)
        {
            bool isValid = base.ValidateSeries(viewportRect, windowRect, view);
            if (OpenColumn == null ||
                CloseColumn == null ||
                HighColumn == null ||
                LowColumn == null)
            {
                isValid = false;
            }
            return isValid;
        }

        private float ConvertToSingle(double value)
        {



            return Convert.ToSingle(value);

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

            FinancialPriceSeriesView priceSeriesView = view as FinancialPriceSeriesView;

            #region validate and bucketize TrendLine

            if (TrendLineType != TrendLineType.None)
            {
                double[] typical = new double[FastItemsSource.Count];
                int typicalIndex = 0;
                foreach (double typicalPrice in TypicalColumn)
                {
                    typical[typicalIndex] = typicalPrice;
                    typicalIndex++;
                }

                priceSeriesView.TrendLineManager.PrepareLine(frame.Trend, TrendLineType, typical,
                   TrendLinePeriod, (x) => XAxis.GetScaledValue(x, xParams),
                   (y) => YAxis.GetScaledValue(y, yParams),
                   new TrendResolutionParams()
                   {
                       BucketSize = view.BucketCalculator.BucketSize,
                       FirstBucket = view.BucketCalculator.FirstBucket,
                       LastBucket = view.BucketCalculator.LastBucket,
                       Offset = offset,
                       Resolution = Resolution,
                       Viewport = viewportRect
                   });
            }

            #endregion

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
                    float bucketOpen = ConvertToSingle(OpenColumn[index]);
                    float bucketHigh = ConvertToSingle(HighColumn[index]);
                    float bucketLow = ConvertToSingle(LowColumn[index]);
                    float bucketClose = ConvertToSingle(CloseColumn[index]);

                    float currentOpen = bucketOpen;
                    float currentHigh = bucketHigh;
                    float currentLow = bucketLow;
                    float currentClose = bucketClose;

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

                        //in a cluster of points, when bucketing, 
                        //we want to keep the Open value from the first point,
                        //Close value from the last point, 
                        //the smallest Low and the largest High values among all points in the cluster
                        currentHigh = Math.Max(bucketHigh, ConvertToSingle(HighColumn[index]));
                        currentLow = Math.Min(bucketLow, ConvertToSingle(LowColumn[index]));
                        currentClose = ConvertToSingle(CloseColumn[index]);
                    }

                    double xVal = double.NaN;
                    if (!double.IsNaN(bucketX))
                    {
                        xVal = this.XAxis.GetScaledValue(bucketX, xParams);
                    }

                    bucket = new float[] { ConvertToSingle(xVal), currentOpen, currentHigh, currentLow, currentClose };
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
                    bucket[2] = (float)yaxis.GetScaledValue(bucket[2], yParams);
                    bucket[3] = (float)yaxis.GetScaledValue(bucket[3], yParams);
                    bucket[4] = (float)yaxis.GetScaledValue(bucket[4], yParams);

                    frame.Buckets.Add(bucket);
                }
            }
            #endregion

            return;
        }
        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);
            FinancialPriceSeriesView financialPriceView = view as FinancialPriceSeriesView;
            financialPriceView.ClearPriceSymbols();

            if (financialPriceView.TrendLineManager != null)
            {
                financialPriceView.TrendLineManager.ClearPoints();
            }
        }

        private const double MIN_WIDTH = 3.0; // actually more like HALF_MIN_WIDTH

        internal override void RenderFrame(CategoryFrame frame, FinancialSeriesView view)
        {
            if (XAxis == null || YAxis == null)
            {
                return;
            }

            //ClearRendering(false);
            FinancialPriceSeriesView priceSeriesView = view as FinancialPriceSeriesView;
            priceSeriesView.ClearPriceSymbols();
            if (priceSeriesView.TrendLineManager != null)
            {
                priceSeriesView.TrendLineManager.ClearPoints();
            }

            PriceDisplayType displayType = DisplayType;

            



            double width = this.XAxis.GetGroupSize(view.WindowRect, view.Viewport) / 2.0;
            width = Math.Max(width, FinancialPriceSeries.MIN_WIDTH);
            FinancialPriceSeriesView priceView = view as FinancialPriceSeriesView;

            PathGeometry positiveGroup = priceView.GetPositiveGroup();
            PathGeometry negativeGroup = priceView.GetNegativeGroup();




            
            if (TrendLineType != TrendLineType.None)
            {
                priceSeriesView.TrendLineManager.RasterizeTrendLine(frame.Trend);
            }

            var buckets = frame.Buckets;
            for (int i = 0; i < buckets.Count; ++i)
            {
                double left = buckets[i][0] - width;
                double center = buckets[i][0];
                double right = buckets[i][0] + width;

                double open = buckets[i][1];
                double high = buckets[i][2];
                double low = buckets[i][3];
                double close = buckets[i][4];
                if (double.IsNaN(open) || double.IsNaN(high) || double.IsNaN(low) || double.IsNaN(close))
                {
                    continue;
                }
                bool negative = open < close;

                PathGeometry group = negative ? negativeGroup : positiveGroup;




                switch (displayType)
                {
                    case PriceDisplayType.Candlestick:
                        if (negative)
                        {
                            double tmp = open;
                            open = close;
                            close = tmp;
                        }

                        //in WPF Pathfigures are much faster.

                        PathFigure pf1 = new PathFigure();
                        pf1.StartPoint = new Point(center, low);
                        pf1.Segments.Add(new LineSegment() { Point = new Point(center, open) });

                        PathFigure pf2 = new PathFigure();
                        pf2.StartPoint = new Point(left, close);
                        pf2.Segments.Add(new LineSegment() { Point = new Point(right, close) });
                        pf2.Segments.Add(new LineSegment() { Point = new Point(right, open) });
                        pf2.Segments.Add(new LineSegment() { Point = new Point(left, open) });
                        pf2.Segments.Add(new LineSegment() { Point = new Point(left, close) });

                        PathFigure pf3 = new PathFigure();
                        pf3.StartPoint = new Point(center, close);
                        pf3.Segments.Add(new LineSegment() { Point = new Point(center, high) });

                        group.Figures.Add(pf1);
                        group.Figures.Add(pf2);
                        group.Figures.Add(pf3);





                        break;

                    case PriceDisplayType.OHLC:

                        PathFigure pf4 = new PathFigure();
                        pf4.StartPoint = new Point(left, open);
                        pf4.Segments.Add(new LineSegment() { Point = new Point(center, open) });

                        PathFigure pf5 = new PathFigure();
                        pf5.StartPoint = new Point(center, low);
                        pf5.Segments.Add(new LineSegment() { Point = new Point(center, high) });

                        PathFigure pf6 = new PathFigure();
                        pf6.StartPoint = new Point(center, close);
                        pf6.Segments.Add(new LineSegment() { Point = new Point(right, close) });

                        group.Figures.Add(pf4);
                        group.Figures.Add(pf5);
                        group.Figures.Add(pf6);





                        break;
                }
            }






        }

        /// <summary>
        /// Returns the item that is found at the given point.
        /// </summary>
        /// <param name="sender">The origin of the mouse interaction.</param>
        /// <param name="point">The point to search for the item near.</param>
        /// <returns>The found item.</returns>
        protected internal override object Item(object sender, Point point)
        {
            if (sender == this.FinancialPriceView.TrendLineManager.TrendPolyline)
            {
                // this is not a data item.
                return null;
            }

            return base.Item(sender, point);
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
            FinancialPriceView.UpdateTrendlineBrush();
            #endregion
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
                FinancialPriceView.TrendLineManager.TrendPolyline);

            trendShape.Tags.Add("Trend");
            svd.Shapes.Add(trendShape);
        }
    }

    internal class FinancialPriceBucketCalculator
        : FinancialBucketCalculator
    {
        public FinancialPriceBucketCalculator(FinancialSeriesView view)
            : base(view)
        {

        }

        public override float[] GetBucket(int index)
        {
            int i0 = index * BucketSize;
            int i1 = Math.Min(i0 + BucketSize - 1, View.FinancialModel.FastItemsSource.Count - 1);

            if (i0 <= i1 && i0 >= 0 && i1 >= 0)
            {
                double open = View.FinancialModel.OpenColumn[i0];
                double high = double.NegativeInfinity;
                double low = double.PositiveInfinity;
                double close = View.FinancialModel.CloseColumn[i1];

                for (int i = i0; i <= i1; ++i)
                {
                    high = Math.Max(high, View.FinancialModel.HighColumn[i]);
                    low = Math.Min(low, View.FinancialModel.LowColumn[i]);
                }

                low = Math.Min(open, low);
                high = Math.Max(close, high);

                return new float[] { (float)(0.5 * (i0 + i1)), (float)open, (float)high, (float)low, (float)close };
            }

            return new float[] { float.NaN, float.NaN, float.NaN, float.NaN, float.NaN };
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