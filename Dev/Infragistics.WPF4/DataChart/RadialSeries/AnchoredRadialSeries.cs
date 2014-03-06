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
using System.Windows.Data;
using System.Collections.Generic;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class for all XamDataChart anchored radial category series.
    /// </summary>
    public abstract class AnchoredRadialSeries
        : RadialBase
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new AnchoredRadialSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            AnchoredRadialView = (AnchoredRadialSeriesView)view;
        }
        internal AnchoredRadialSeriesView AnchoredRadialView { get; set; }

        /// <summary>
        /// Initializes a new instance of the AnchoredRadialSeries
        /// </summary>
        public AnchoredRadialSeries()
        {
            LineRasterizer = new CategoryLineRasterizer();
        }

        internal CategoryLineRasterizer LineRasterizer { get; set; }
        

        #region ValueMapping Dependency Property and ValueColumn
        /// <summary>
        /// Gets or sets the item path that provides the values for the current series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string ValueMemberPath
        {
            get
            {
                return (string)GetValue(ValueMemberPathProperty);
            }
            set
            {
                SetValue(ValueMemberPathProperty, value);
            }
        }

        internal const string ValueMemberPathPropertyName = "ValueMemberPath";

        /// <summary>
        /// Identifies the ValueMapping dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register(ValueMemberPathPropertyName, typeof(string),
            typeof(AnchoredRadialSeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as AnchoredRadialSeries).RaisePropertyChanged(ValueMemberPathPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Holds the values for the series.
        /// </summary>
        protected internal IFastItemColumn<double> ValueColumn
        {
            get { return _valueColumn; }
            private set
            {
                if (_valueColumn != value)
                {
                    IFastItemColumn<double> oldValueColumn = _valueColumn;

                    _valueColumn = value;
                    RaisePropertyChanged(ValueColumnPropertyName, oldValueColumn, _valueColumn);
                }
            }
        }
        private IFastItemColumn<double> _valueColumn;
        internal const string ValueColumnPropertyName = "ValueColumn";
        #endregion

        #region TrendLineType Dependency Property
        /// <summary>
        /// Gets or sets the trend type for the current series object.
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
        public static readonly DependencyProperty TrendLineTypeProperty = DependencyProperty.Register(TrendLineTypePropertyName, typeof(TrendLineType), typeof(AnchoredRadialSeries),
            new PropertyMetadata(TrendLineType.None, (sender, e) =>
            {
                (sender as AnchoredRadialSeries).RaisePropertyChanged(TrendLineTypePropertyName, e.OldValue, e.NewValue);
            }));

        // properties for TrendStroke, TrendBrush etc
        #endregion

        #region TrendLineBrush Dependency Property
        /// <summary>
        /// Gets or sets the brush that specifies how to the current series
        /// object's Trend line is drawn.
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
        public static readonly DependencyProperty TrendLineBrushProperty = DependencyProperty.Register(TrendLineBrushPropertyName, typeof(Brush), typeof(AnchoredRadialSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as AnchoredRadialSeries).RaisePropertyChanged(TrendLineBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion
        #region ActualTrendLineBrush Dependency Property
        /// <summary>
        /// Gets the effective TrendLineBrush for this series.
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
        public static readonly DependencyProperty ActualTrendLineBrushProperty = DependencyProperty.Register(TrendLineActualBrushPropertyName, typeof(Brush), typeof(AnchoredRadialSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as AnchoredRadialSeries).RaisePropertyChanged(TrendLineActualBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion
        #region TrendLineThickness Property
        /// <summary>
        /// Gets or sets the thickness of the current series object's trend line.
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
        public static readonly DependencyProperty TrendLineThicknessProperty = DependencyProperty.Register(TrendLineThicknessPropertyName, typeof(double), typeof(AnchoredRadialSeries),
            new PropertyMetadata(1.5, (sender, e) =>
            {
                (sender as AnchoredRadialSeries).RaisePropertyChanged(TrendLineThicknessPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineDashCap Property
        /// <summary>
        /// Gets or sets the PenLineCap enumeration value that specifies how the the current
        /// series object's trend line dash ends are drawn. 
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
        public static readonly DependencyProperty TrendLineDashCapProperty = DependencyProperty.Register(TrendLineDashCapPropertyName, typeof(PenLineCap), typeof(AnchoredRadialSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as AnchoredRadialSeries).RaisePropertyChanged(TrendLineDashCapPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineDashArray Property
        /// <summary>
        /// Gets or sets a collection of double values that indicate the pattern of dashes and gaps that
        /// is used to draw the trend line for the current series object. 
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
        public static readonly DependencyProperty TrendLineDashArrayProperty = DependencyProperty.Register(TrendLineDashArrayPropertyName, typeof(DoubleCollection), typeof(AnchoredRadialSeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as AnchoredRadialSeries).RaisePropertyChanged(TrendLineDashArrayPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLinePeriod Dependency Property
        /// <summary>
        /// Gets or sets the trend line period for the current series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// <remarks>
        /// The typical, and initial, value for bollinger band periods is 20.
        /// </remarks>
        /// </summary>
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
        public static readonly DependencyProperty TrendLinePeriodProperty = DependencyProperty.Register(TrendLinePeriodPropertyName, typeof(int), typeof(AnchoredRadialSeries),
            new PropertyMetadata(7, (sender, e) =>
            {
                (sender as AnchoredRadialSeries).RaisePropertyChanged(TrendLinePeriodPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        /// <summary>
        /// Identifies the TrendLineZIndex Dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineZIndexProperty = DependencyProperty.Register(TrendLineZIndexPropertyName, typeof(int), typeof(AnchoredRadialSeries), new PropertyMetadata(1, (sender, e) =>
        {
            (sender as AnchoredRadialSeries).RaisePropertyChanged(TrendLineZIndexPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Sets or Gets the Z index of the trendline.
        /// </summary>
        [WidgetDefaultNumber(1)]
        public int TrendLineZIndex
        {
            get
            {
                return (int)this.GetValue(AnchoredRadialSeries.TrendLineZIndexProperty);
            }
            set
            {
                this.SetValue(AnchoredRadialSeries.TrendLineZIndexProperty, value);
            }
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (axis != null && axis == AngleAxis && ValueColumn != null && ValueColumn.Count > 0)
            {
                return new AxisRange(0, ValueColumn.Count - 1);
            }

            if (axis != null && axis == ValueAxis && ValueColumn != null && ValueColumn.Count > 0)
            {
                return new AxisRange(ValueColumn.Minimum, ValueColumn.Maximum);
            }
            return null;
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
            if (this.AnchoredRadialView.TrendLineManager.PropertyUpdated(sender, propertyName, oldValue, newValue,
              TrendLineDashArray))
            {
                RenderSeries(false);
                this.NotifyThumbnailAppearanceChanged();
            }

            NumericAxisBase valueAxis = ValueAxis as NumericAxisBase;

            switch (propertyName)
            {
                case Series.FastItemsSourcePropertyName:
                    if (oldValue as FastItemsSource != null)
                    {
                        (oldValue as FastItemsSource).DeregisterColumn(ValueColumn);
                        ValueColumn = null;
                    }

                    if (newValue as FastItemsSource != null)
                    {
                        ValueColumn = RegisterDoubleColumn(ValueMemberPath);
                        this.AnchoredRadialView.BucketCalculator.CalculateBuckets(Resolution);
                    }

                    if (valueAxis != null && !valueAxis.UpdateRange())
                    {
                        this.AnchoredRadialView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }

                    break;

                case ValueMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(ValueColumn);
                        ValueColumn = RegisterDoubleColumn(ValueMemberPath);
                    }

                    break;

                case ValueColumnPropertyName:
                    if (valueAxis != null && !valueAxis.UpdateRange())
                    {
                        this.AnchoredRadialView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;

                case TrendLineBrushPropertyName:
                    this.UpdateIndexedProperties();
                    break;
                case TrendLineTypePropertyName:
                    this.NotifyThumbnailAppearanceChanged();
                    break;
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
            switch (action)
            {
                case FastItemsSourceEventAction.Reset:
                case FastItemsSourceEventAction.Insert:
                case FastItemsSourceEventAction.Remove:
                    this.AnchoredRadialView.BucketCalculator.CalculateBuckets(Resolution);
                    break;
            }

            this.AnchoredRadialView.TrendLineManager.DataUpdated(action, position, count, propertyName);

            switch (action)
            {
                case FastItemsSourceEventAction.Reset:
                    if (ValueAxis != null && !ValueAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }

                    break;

                case FastItemsSourceEventAction.Insert:
                    if (ValueAxis != null && !ValueAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Remove:
                    if (ValueAxis != null && !ValueAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Replace:
                    if (ValueMemberPath != null && this.AnchoredRadialView.BucketCalculator.BucketSize > 0
                        && ValueAxis != null && !ValueAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Change:
                    if (propertyName == ValueMemberPath)
                    {
                        if (ValueAxis != null && !ValueAxis.UpdateRange())
                        {
                            RenderSeries(true);
                        }
                    }

                    break;
            }
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
            var anchoredView = (AnchoredRadialSeriesView)view;

            if (FastItemsSource == null ||
                FastItemsSource.Count == 0 ||
                AngleAxis == null ||
                ValueColumn == null ||
                AngleAxis.ItemsCount == 0 ||
                ValueAxis == null ||
                double.IsInfinity(ValueAxis.ActualMinimumValue) ||
                double.IsInfinity(ValueAxis.ActualMaximumValue) ||
                anchoredView.BucketCalculator.BucketSize < 1)
            {
                isValid = false;
            }
            return isValid;
        }

        /// <summary>
        /// Gets whether the shape of the series is a closed polygon.
        /// </summary>
        protected virtual bool IsClosed { get { return false; } }

        internal override void PrepareFrame(RadialFrame frame, RadialBaseView view)
        {
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            CategoryAngleAxis angleAxis = AngleAxis;
            NumericRadiusAxis valueAxis = ValueAxis;

            double minValue = valueAxis.ActualMinimumValue;
            double maxValue = valueAxis.ActualMaximumValue;

            frame.Buckets.Clear();
            frame.Markers.Clear();
            frame.Trend.Clear();

            bool markers = this.ShouldDisplayMarkers();
            int markerCount = 0;

            var anchoredRadialView = (AnchoredRadialSeriesView)view;

            CollisionAvoider collisionAvoider = new CollisionAvoider();
            double offset = 0.0;            // offset (pels) to the center of this categorySeries

            int lastBucket = view.BucketCalculator.LastBucket;

            #region work out the category mode and offset

            CategoryMode categoryMode = PreferredCategoryMode(angleAxis);

            if (categoryMode == CategoryMode.Mode0 && angleAxis.CategoryMode != CategoryMode.Mode0)
            {
                categoryMode = CategoryMode.Mode1;
            }

            switch (categoryMode)
            {
                case CategoryMode.Mode0:    // use bucket.X as-is
                    offset = 0.0;
                    break;

                case CategoryMode.Mode1:    // offset x by half category width
                    offset = 0.5 * angleAxis.GetCategorySize(windowRect, viewportRect);
                    break;

                case CategoryMode.Mode2:    // offset x by the appropriate amount for this categorySeries
                    offset = angleAxis.GetGroupCenter(GetMode2Index(), windowRect, viewportRect);
                    break;
            }

            if (angleAxis.IsInverted)
            {
                offset = -offset;
            }
            #endregion

            #region validate and bucketize TrendLine

            anchoredRadialView.TrendLineManager.RadiusExtentScale = ValueAxis.ActualRadiusExtentScale;
            anchoredRadialView.TrendLineManager.InnerRadiusExtentScale = ValueAxis.ActualInnerRadiusExtentScale;
            anchoredRadialView.TrendLineManager.ProjectX = (angle, radius) => ProjectX(angle, radius, windowRect, viewportRect);
            anchoredRadialView.TrendLineManager.ProjectY = (angle, radius) => ProjectY(angle, radius, windowRect, viewportRect);

            Clipper clipper = new Clipper(viewportRect, false) { Target = frame.Trend };
            var resParams = new TrendResolutionParams()
               {
                   BucketSize = view.BucketCalculator.BucketSize,
                   FirstBucket = view.BucketCalculator.FirstBucket,
                   LastBucket = lastBucket,
                   Offset = offset,
                   Resolution = Resolution,
                   Viewport = viewportRect,
                   Window = windowRect
               };

            anchoredRadialView.TrendLineManager.PrepareLine(frame.Trend, TrendLineType, ValueColumn,
               TrendLinePeriod, 
               (a) => AngleAxis.GetScaledAngle(a),
               (r) => ValueAxis.GetScaledValue(r),
               resParams,
               clipper);

            #endregion

            bool inNans = true;

            #region bucketize data and markerItems
            if (RepeatExists(view))
            {
                lastBucket--;
            }

            for (int i = view.BucketCalculator.FirstBucket; i <= lastBucket; ++i)
            {
                int index = i; // % AngleAxis.ItemsCount;
                if (index * view.BucketCalculator.BucketSize >= AngleAxis.ItemsCount)
                {
                    index -= (AngleAxis.ItemsCount) / view.BucketCalculator.BucketSize;
                }

                float[] bucket = view.BucketCalculator.GetBucket(index);

                if (!float.IsNaN(bucket[0]))
                {

                    bucket[0] = (float)(angleAxis.GetScaledAngle(bucket[0]) + offset);

                    if (bucket[1] < minValue || bucket[1] > maxValue)
                    {
                        continue;
                    }

                    bucket[1] = (float)valueAxis.GetScaledValue(bucket[1]);

                    if (view.BucketCalculator.BucketSize > 1)
                    {
                        if (bucket[2] < minValue || bucket[2] > maxValue)
                        {
                            continue;
                        }

                        bucket[2] = (float)valueAxis.GetScaledValue(bucket[2]);
                    }
                    else
                    {
                        bucket[2] = bucket[1];
                    }

                    if ((float.IsNaN(bucket[1]) ||
                        float.IsNaN(bucket[2])) &&
                        inNans && IsClosed && CenterVisible())
                    {
                        lastBucket++;
                    }
                    else
                    {
                        inNans = false;
                    }

                    frame.Buckets.Add(bucket);

                    if (markers)
                    {
                        int j = Math.Min(index * view.BucketCalculator.BucketSize, FastItemsSource.Count - 1);

                        double x = _axes.GetXValue(bucket[0], bucket[1], windowRect, viewportRect, (a) => Math.Cos(a));
                        double y = _axes.GetYValue(bucket[0], bucket[1], windowRect, viewportRect, (a) => Math.Sin(a));

                        Rect markerRect = new Rect(x - 5, y - 5, 11, 11);

                        
                        if (!double.IsNaN(x) && !double.IsNaN(y) &&
                            !double.IsInfinity(x) &&
                            !double.IsInfinity(y) &&
                            collisionAvoider.TryAdd(markerRect))
                        {
                            frame.Markers.Add(new Point(x, y));

                            Marker marker = view.Markers[markerCount];
                            (marker.Content as DataContext).Item = FastItemsSource[j];

                            ++markerCount;
                        }
                    }
                }
                else
                {
                    if (inNans && IsClosed && CenterVisible())
                    {
                        lastBucket++;
                    }
                }
            }
            #endregion


            view.Markers.Count = markerCount;
            return;
        }

        private double ProjectX(double angle, double radius, Rect windowRect, Rect viewportRect)
        {
            return _axes.GetXValue(angle, radius, windowRect, viewportRect, (x) => Math.Cos(x));
        }

        private double ProjectY(double angle, double radius, Rect windowRect, Rect viewportRect)
        {
            return _axes.GetYValue(angle, radius, windowRect, viewportRect, (x) => Math.Sin(x));
        }

        private bool RepeatExists(RadialBaseView view)
        {
            var anchoredView = (AnchoredRadialSeriesView)view;
                return !IsClosed
                    && anchoredView.BucketCalculator.FirstBucket == 0
                    && anchoredView.BucketCalculator.LastBucket == AngleAxis.ItemsCount;
        }



#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

        internal Clipper GetLineClipper(Func<int,double> x0, int endIndex, SeriesView view)
        {
            Clipper clipper = null;
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            if (endIndex > -1 && !windowRect.IsEmpty && !viewportRect.IsEmpty)
            {
                double left = x0(0) < viewportRect.Left - 2000 ? viewportRect.Left - 10 : double.NaN;
                double bottom = viewportRect.Bottom + 10;
                double right = x0(endIndex) > viewportRect.Right + 2000 ? viewportRect.Right + 10 : double.NaN;
                double top = viewportRect.Top - 10;
                clipper = new Clipper(left, bottom, right, top, false);
            }

            return clipper;
        }


        /// <summary>
        /// A point used to complete the polygon.
        /// </summary>
        protected Point _terminationPoint = new Point(0, 0);

        internal void TerminatePolygon(
            PointCollection polygon0,
            PointCollection line0,
            PointCollection polygon01,
            PointCollection line1, bool finished)
        {
            if (polygon0.Count > 0 && line1.Count > 0)
            {
                if (!finished || CenterNotVisible())
                {
                    polygon0.Add(_terminationPoint);
                    polygon0.Add(polygon0[0]);

                    line1.Add(_terminationPoint);
                    line1.Add(line1[0]);
                }
            }
        }

        private bool CenterVisible()
        {
            return !CenterNotVisible();
        }

        private bool CenterNotVisible()
        {
            var window = View.WindowRect;
            return !window.Contains(new Point(0.5, 0.5));
        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var radView = (AnchoredRadialSeriesView)view;
            if (wipeClean)
            {
                radView.Markers.Clear();
            }
            radView.TrendLineManager.ClearPoints();
        }

        internal override void RenderFrame(RadialFrame frame, RadialBaseView view)
        {
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            var anchoredView = (AnchoredRadialSeriesView)view;
            anchoredView.TrendLineManager.RasterizeTrendLine(frame.Trend);
            CategoryMarkerManager.RasterizeMarkers(this, frame.Markers, anchoredView.Markers, UseLightweightMarkers);
            anchoredView.RenderMarkers();
            ApplyClipping(viewportRect, windowRect, anchoredView);
        }

        private void ApplyClipping(Rect viewportRect, Rect windowRect, AnchoredRadialSeriesView view)
        {
            view.ApplyClipping(viewportRect, windowRect);
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
            AnchoredRadialView.UpdateTrendlineBrush();
            #endregion
        }

        /// <summary>
        /// Returns the item that is the best match for the provided point.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="point">The point associated with the event.</param>
        /// <returns>The best match for the point.</returns>
        protected internal override object Item(object sender, Point point)
        {
            if (sender == this.AnchoredRadialView.TrendLineManager.TrendPolyline)
            {
                // this is not a data item.
                return null;
            }
            return base.Item(sender, point);
        }

        /// <summary>
        /// Scrolls the specified item into the view.
        /// </summary>
        /// <param name="item">The item to scroll into view.</param>
        /// <returns>True if the item has been scrolled into view.</returns>
        public override bool ScrollIntoView(object item)
        {
            Rect windowRect = View.WindowRect;
            Rect viewportRect = View.Viewport;
            int index = !windowRect.IsEmpty && !viewportRect.IsEmpty && FastItemsSource != null ? FastItemsSource[item] : -1;

            if (AngleAxis == null ||
                ValueColumn == null ||
                ValueAxis == null)
            {
                return false;
            }

            if (index < 0 ||
                index > ValueColumn.Count - 1)
            {
                return false;
            }

            double scaledAngle = AngleAxis.GetScaledAngle(index);
            double scaledRadius = ValueAxis.GetScaledValue(ValueColumn[index]);

            if (double.IsNaN(scaledRadius))
            {
                scaledRadius = (ValueAxis.ActualInnerRadiusExtentScale + ValueAxis.ActualRadiusExtentScale) / 2.0;
            }

            double cx = 0.5 + (Math.Cos(scaledAngle) * scaledRadius);
            double cy = 0.5 + (Math.Sin(scaledAngle) * scaledRadius);

            if (!double.IsNaN(cx))
            {
                if (cx < windowRect.Left + 0.1 * windowRect.Width)
                {
                    cx = cx + 0.4 * windowRect.Width;
                    windowRect.X = cx - 0.5 * windowRect.Width;
                }

                if (cx > windowRect.Right - 0.1 * windowRect.Width)
                {
                    cx = cx - 0.4 * windowRect.Width;
                    windowRect.X = cx - 0.5 * windowRect.Width;
                }
            }

            if (!double.IsNaN(cy))
            {
                if (cy < windowRect.Top + 0.1 * windowRect.Height)
                {
                    cy = cy + 0.4 * windowRect.Height;
                    windowRect.Y = cy - 0.5 * windowRect.Height;
                }

                if (cy > windowRect.Bottom - 0.1 * windowRect.Height)
                {
                    cy = cy - 0.4 * windowRect.Height;
                    windowRect.Y = cy - 0.5 * windowRect.Height;
                }
            }

            if (SyncLink != null)
            {
                SyncLink.WindowNotify(SeriesViewer, windowRect);
            }

            return index >= 0;
        }

        /// <summary>
        /// Renders the thumbnail for the OPD.
        /// </summary>
        /// <param name="viewportRect">The viewport to use.</param>
        /// <param name="surface">The render target.</param>
        protected internal override void RenderThumbnail(Rect viewportRect, RenderSurface surface)
        {
            base.RenderThumbnail(viewportRect, surface);

            if (!ThumbnailDirty)
            {
                View.PrepSurface(surface);
                return;
            }

            this.View.PrepSurface(surface);
            AnchoredRadialSeriesView thumbnailView = this.ThumbnailView as AnchoredRadialSeriesView;
            thumbnailView.BucketCalculator.CalculateBuckets(this.Resolution);
            if (ClearAndAbortIfInvalid(ThumbnailView))
            {
                return;
            }

            //AnchoredRadialSeriesView originalView = this.AnchoredRadialView;

            RadialFrame frame = new RadialFrame(3);

            // set the thumbnail view
            //OnViewCreated(thumbnailView);
            //this.View = thumbnailView;
            this.PrepareFrame(frame, thumbnailView);
            this.RenderFrame(frame, thumbnailView);

            // return the old view
            //OnViewCreated(originalView);
            //this.View = originalView;

            ThumbnailDirty = false;
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
                AnchoredRadialView.TrendLineManager.TrendPolyline);

            trendShape.Tags.Add("Trend");
            svd.Shapes.Add(trendShape);
        }
    }

    internal class AnchoredRadialBucketCalculator : RadialBucketCalculator
    {
        private AnchoredRadialSeriesView AnchoredView { get; set; }
        public AnchoredRadialBucketCalculator(AnchoredRadialSeriesView view)
            : base(view)
        {
            this.AnchoredView = view;
        }
        public override float[] GetBucket(int bucket)
        {
            IFastItemColumn<double> valueColumn = this.AnchoredView.AnchoredRadialModel.ValueColumn;
            int i0 = Math.Min(bucket * BucketSize, valueColumn.Count - 1);
            int i1 = Math.Min(i0 + BucketSize - 1, valueColumn.Count - 1);

            double min = double.NaN;
            double max = double.NaN;

            for (int i = i0; i <= i1; ++i)
            {
                double y = valueColumn[i];

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

            return new float[] { (float)(0.5 * (i0 + i1)), float.NaN, float.NaN };
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