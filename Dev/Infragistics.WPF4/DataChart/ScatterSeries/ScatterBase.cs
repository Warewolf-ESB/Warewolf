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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class for all XamDataChart scatter series
    /// </summary>
    [WidgetModuleParent("ScatterChart")]
    public abstract class ScatterBase : MarkerSeries, ISupportsErrorBars
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new ScatterBaseView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            ScatterView = (ScatterBaseView)view;
        }
        internal ScatterBaseView ScatterView { get; set; }

        #region constructor and initialisation
        /// <summary>
        /// ScatterBase constructor.
        /// </summary>
        public ScatterBase()
        {
            _cachedWindowRect = Rect.Empty;
            _cachedViewportRect = Rect.Empty;
        }

        internal ScatterAxisInfoCache AxisInfoCache { get; set; }

        //internal RenderManager RenderManager { get; set; }
        #endregion

        #region XAxis Dependency Property
        /// <summary>
        /// Gets or sets the effective x-axis for the current object.
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
        /// Identifies the XAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(XAxisPropertyName, typeof(NumericXAxis), typeof(ScatterBase),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as ScatterBase).RaisePropertyChanged(XAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region YAxis Depedency Property
        /// <summary>
        /// Gets or sets the effective y-axis for the current object.
        /// <para>
        /// This is a dependency property.
        /// </para>
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
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(YAxisPropertyName, typeof(NumericYAxis), typeof(ScatterBase),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as ScatterBase).RaisePropertyChanged(YAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region XMemberPath Dependency Property and XColumn Property
        /// <summary>
        /// Gets or sets the value mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string XMemberPath
        {
            get
            {
                return (string)GetValue(XMemberPathProperty);
            }
            set
            {
                SetValue(XMemberPathProperty, value);
            }
        }

        internal const string XMemberPathPropertyName = "XMemberPath";

        /// <summary>
        /// Identifies the XMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty XMemberPathProperty = DependencyProperty.Register(XMemberPathPropertyName, typeof(string), typeof(ScatterBase), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ScatterBase).RaisePropertyChanged(XMemberPathPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// The IFastItemColumn containing X values.
        /// </summary>
        protected internal IFastItemColumn<double> XColumn
        {
            get { return xColumn; }
            private set
            {
                if (xColumn != value)
                {
                    IFastItemColumn<double> oldXColumn = XColumn;

                    xColumn = value;
                    RaisePropertyChanged(XColumnPropertyName, oldXColumn, XColumn);
                }
            }
        }
        private IFastItemColumn<double> xColumn;
        internal const string XColumnPropertyName = "XColumn";
        #endregion

        #region YMemberPath Dependency Property and YColumn Property
        /// <summary>
        /// Gets or sets the value mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string YMemberPath
        {
            get
            {
                return (string)GetValue(YMemberPathProperty);
            }
            set
            {
                SetValue(YMemberPathProperty, value);
            }
        }

        internal const string YMemberPathPropertyName = "YMemberPath";

        /// <summary>
        /// Identifies the YMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty YMemberPathProperty = DependencyProperty.Register(YMemberPathPropertyName, typeof(string), typeof(ScatterBase), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ScatterBase).RaisePropertyChanged(YMemberPathPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// The IFastItemColumn containing Y values.
        /// </summary>
        protected internal IFastItemColumn<double> YColumn
        {
            get { return yColumn; }
            private set
            {
                if (yColumn != value)
                {
                    IFastItemColumn<double> oldYColumn = YColumn;

                    yColumn = value;
                    RaisePropertyChanged(YColumnPropertyName, oldYColumn, YColumn);
                }
            }
        }
        private IFastItemColumn<double> yColumn;
        internal const string YColumnPropertyName = "YColumn";
        #endregion

        #region TrendLineType Dependency Property
        /// <summary>
        /// Gets or sets the trend type for the current scatter series.
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
        public static readonly DependencyProperty TrendLineTypeProperty = DependencyProperty.Register(TrendLineTypePropertyName, typeof(TrendLineType), typeof(ScatterBase),
            new PropertyMetadata(TrendLineType.None, (sender, e) =>
            {
                (sender as ScatterBase).RaisePropertyChanged(TrendLineTypePropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty TrendLineBrushProperty = DependencyProperty.Register(TrendLineBrushPropertyName, typeof(Brush), typeof(ScatterBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as ScatterBase).RaisePropertyChanged(TrendLineBrushPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty ActualTrendLineBrushProperty = DependencyProperty.Register(TrendLineActualBrushPropertyName, typeof(Brush), typeof(ScatterBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as ScatterBase).RaisePropertyChanged(TrendLineActualBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineThickness Property
        /// <summary>
        /// Gets or sets the thickness of the current scatter series object's trend line.
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
        public static readonly DependencyProperty TrendLineThicknessProperty = DependencyProperty.Register(TrendLineThicknessPropertyName, typeof(double), typeof(ScatterBase),
            new PropertyMetadata(1.5, (sender, e) =>
            {
                (sender as ScatterBase).RaisePropertyChanged(TrendLineThicknessPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineDashCap Property
        /// <summary>
        /// Gets or sets the PenLineCap enumeration value that specifies how the current
        /// scatter series object's trend line dash ends are drawn. 
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
        public static readonly DependencyProperty TrendLineDashCapProperty = DependencyProperty.Register(TrendLineDashCapPropertyName, typeof(PenLineCap), typeof(ScatterBase),
            new PropertyMetadata((sender, e) =>
            {
                (sender as ScatterBase).RaisePropertyChanged(TrendLineDashCapPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineDashArray Property
        /// <summary>
        /// Gets or sets a collection of double values that indicate the pattern of dashes and gaps that
        /// is used to draw the trend line for the current scatter series object. 
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
        public static readonly DependencyProperty TrendLineDashArrayProperty = DependencyProperty.Register(TrendLineDashArrayPropertyName, typeof(DoubleCollection), typeof(ScatterBase),
            new PropertyMetadata((sender, e) =>
            {
                (sender as ScatterBase).RaisePropertyChanged(TrendLineDashArrayPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLinePeriod Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current scatter series object.
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
        public static readonly DependencyProperty TrendLinePeriodProperty = DependencyProperty.Register(TrendLinePeriodPropertyName, typeof(int), typeof(ScatterBase),
            new PropertyMetadata(7, (sender, e) =>
            {
                (sender as ScatterBase).RaisePropertyChanged(TrendLinePeriodPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region MarkerCollisionAvoidance Dependency Property
        /// <summary>
        /// The desired behavior for markers in this series which are placed too close together for the current view, resulting in a collision.
        /// </summary>
        public CollisionAvoidanceType MarkerCollisionAvoidance
        {
            get
            {
                return (CollisionAvoidanceType)GetValue(MarkerCollisionAvoidanceProperty);
            }
            set
            {
                SetValue(MarkerCollisionAvoidanceProperty, value);
            }
        }

        internal const string MarkerCollisionAvoidancePropertyName = "MarkerCollisionAvoidance";

        /// <summary>
        /// Identifies the MarkerCollisionAvoidance dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerCollisionAvoidanceProperty =
            DependencyProperty.Register(MarkerCollisionAvoidancePropertyName, typeof(CollisionAvoidanceType), typeof(ScatterBase),
            new PropertyMetadata(CollisionAvoidanceType.None, (sender, e) =>
            {
                (sender as ScatterBase).RaisePropertyChanged(MarkerCollisionAvoidancePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ISupportsErrorBars Implementation


        bool ISupportsErrorBars.ShouldDisplayErrorBars()
        {
            if (this.ErrorBarSettings == null ||
                !View.Ready())
            {
                return false;
            }

            return true;
        }

        bool ISupportsErrorBars.ShouldSyncErrorBarsWithMarkers()
        {
            return this.ShouldDisplayMarkers();
        }

        ErrorBarSettingsBase ISupportsErrorBars.ErrorBarSettings
        {
            get
            {
                return this.ErrorBarSettings;
            }
        }




#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

        Axis ISupportsErrorBars.XAxis
        {
            get
            {
                return this.XAxis;
            }
        }

        Axis ISupportsErrorBars.YAxis
        {
            get
            {
                return this.YAxis;
            }
        }


        #endregion ISupportsErrorBars Implementation

        /// <summary>
        /// Identifies the TrendLineZIndex dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineZIndexProperty = DependencyProperty.Register(TrendLineZIndexPropertyName, typeof(int), typeof(ScatterBase), new PropertyMetadata(1001, (sender, e) =>
        {
            (sender as ScatterBase).RaisePropertyChanged(TrendLineZIndexPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// Gets or sets the Z-Index of the trend line.  Values greater than 1000 will result in the trend line being rendered in front of the series data.
        /// </summary>
        [WidgetDefaultNumber(1001)]
        public int TrendLineZIndex
        {
            get
            {
                return (int)this.GetValue(TrendLineZIndexProperty);
            }
            set
            {
                this.SetValue(TrendLineZIndexProperty, value);
            }
        }

        #region MaximumMarkers Dependency Property
        /// <summary>
        /// Gets or sets the maximum number of markerItems displayed by the current series.
        /// <para>If more than the specified number of markerItems are visible, the series will automatically
        /// choose a representative set.</para>
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultNumber(400)]
        public int MaximumMarkers
        {
            get
            {
                return (int)GetValue(MaximumMarkersProperty);
            }
            set
            {
                SetValue(MaximumMarkersProperty, value);
            }
        }

        internal const string MaximumMarkersPropertyName = "MaximumMarkers";

        /// <summary>
        /// Identifies the MaximumMarkers dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumMarkersProperty = DependencyProperty.Register(MaximumMarkersPropertyName, typeof(int), typeof(ScatterBase),
            new PropertyMetadata(400, (sender, e) =>
            {
                (sender as ScatterBase).RaisePropertyChanged(MaximumMarkersPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        /// <summary>
        /// Invalidates the axes associated with the series.
        /// </summary>
        protected internal override void InvalidateAxes()
        {
            base.InvalidateAxes();
            if (this.XAxis != null)
            {
                this.XAxis.RenderAxis(false);
            }
            if (this.YAxis != null)
            {
                this.YAxis.RenderAxis(false);
            }
        }

        #region ErrorBarSettings

        /// <summary>
        /// The error bar settings for the series.
        /// </summary>
        public ScatterErrorBarSettings ErrorBarSettings
        {
            get { return (ScatterErrorBarSettings)GetValue(ErrorBarSettingsProperty); }
            set { SetValue(ErrorBarSettingsProperty, value); }
        }

        private const string ErrorBarSettingsPropertyName = "ErrorBarSettings";

        /// <summary>
        /// Identifies the ErrorBarSettings dependency property.
        /// </summary>
        public static readonly DependencyProperty ErrorBarSettingsProperty =
            DependencyProperty.Register(ErrorBarSettingsPropertyName, typeof(ScatterErrorBarSettings), typeof(ScatterBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as ScatterBase).RaisePropertyChanged(ErrorBarSettingsPropertyName, e.OldValue, e.NewValue);
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
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            if (this.ScatterView.TrendLineManager.PropertyUpdated(sender, propertyName, oldValue, newValue,
              TrendLineDashArray))
            {
                RenderSeries(false);
                this.NotifyThumbnailAppearanceChanged();
            }

            switch (propertyName)
            {
                case FastItemsSourcePropertyName:
                    if (oldValue as FastItemsSource != null)
                    {
                        (oldValue as FastItemsSource).DeregisterColumn(XColumn);
                        (oldValue as FastItemsSource).DeregisterColumn(YColumn);
                        XColumn = null;
                        YColumn = null;
                    }

                    if (newValue as FastItemsSource != null)
                    {
                        XColumn = RegisterDoubleColumn(XMemberPath);
                        YColumn = RegisterDoubleColumn(YMemberPath);
                    }

                    if ((YAxis != null && !YAxis.UpdateRange()) ||
                        (XAxis != null && !XAxis.UpdateRange()))
                    {
                        RenderSeries(false);
                    }

                    break;

                case XAxisPropertyName:
                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }

                    if ((XAxis != null && !XAxis.UpdateRange()) ||
                        (newValue == null && oldValue != null))
                    {
                        RenderSeries(false);
                    }
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case YAxisPropertyName:
                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }
                    if ((YAxis != null && !YAxis.UpdateRange()) ||
                        (newValue == null && oldValue != null))
                    {
                        RenderSeries(false);
                    }
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                #region X Mapping and Column
                case XMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(XColumn);
                        XColumn = RegisterDoubleColumn(XMemberPath);
                    }

                    break;

                case XColumnPropertyName:
                    this.ScatterView.TrendLineManager.Reset();

                    if (XAxis != null && !XAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }
                    break;
                #endregion

                #region Y Mapping and Column
                case YMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(YColumn);
                        YColumn = RegisterDoubleColumn(YMemberPath);
                    }

                    break;

                case YColumnPropertyName:
                    this.ScatterView.TrendLineManager.Reset();

                    if (YAxis != null && !YAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }
                    break;
                #endregion

                case MarkerCollisionAvoidancePropertyName:
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case MaximumMarkersPropertyName:
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case TransitionProgressPropertyName:
                    TransitionFrame.Interpolate((float)TransitionProgress, PreviousFrame, CurrentFrame);

                    CacheViewInfo();
                    try
                    {
                        if (ClearAndAbortIfInvalid(View))
                        {
                            return;
                        }

                        if ((Math.Round(TransitionProgress * 100000) / 100000.0) == 1.0)
                        {
                            RenderFrame(CurrentFrame, this.ScatterView);
                        }
                        else
                        {
                            RenderFrame(TransitionFrame, this.ScatterView);
                        }
                    }
                    finally
                    {
                        UnCacheViewInfo();
                    }
                    break;
                case TrendLineBrushPropertyName:
                    this.UpdateIndexedProperties();
                    break;
                case ErrorBarSettingsPropertyName:
                    if (this.ErrorBarSettings != null)
                    {
                        this.ErrorBarSettings.Series = this;
                    }
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
                case TrendLineTypePropertyName:
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }

        private void UnCacheViewInfo()
        {
            _cachedViewportRect = Rect.Empty;
            _cachedWindowRect = Rect.Empty;
        }

        private Rect _cachedViewportRect;
        private Rect _cachedWindowRect;

        private void CacheViewInfo()
        {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            GetViewInfo(out _cachedViewportRect, out _cachedWindowRect);

        }

        /// <summary>
        /// Returns true if the series should react to a data change.
        /// </summary>
        /// <param name="propertyName">The name of the property being changed.</param>
        /// <param name="action">The action on the items source.</param>
        /// <returns>True if the series should react.</returns>
        protected virtual bool MustReact(string propertyName, FastItemsSourceEventAction action)
        {
            if (action != FastItemsSourceEventAction.Change)
            {
                return true;
            }

            if (propertyName == null)
            {
                return true;
            }

            if (XMemberPath == propertyName ||
                YMemberPath == propertyName)
            {
                return true;
            }

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
            bool refresh = false;

            if (!MustReact(propertyName, action))
            {
                return;
            }
            //UpdateCachedPoints(action, position, count, propertyName);
            this.ScatterView.TrendLineManager.DataUpdated(action, position, count, propertyName);

            if (XAxis != null && !XAxis.UpdateRange())
            {
                refresh = true;
            }

            if (YAxis != null && !YAxis.UpdateRange())
            {
                refresh = true;
            }

            if (refresh)
            {
                RenderSeries(true);
            }
        }

        internal void PrepLinePoints(ScatterFrame frame)
        {
            PrepLinePoints(frame, null);
        }

        internal void PrepLinePoints(ScatterFrame frame, Clipper clipper)
        {
            int xCount = XColumn != null ? XColumn.Count : 0;
            int yCount = YColumn != null ? YColumn.Count : 0;

            int count = Math.Min(xCount, yCount);
            if (count <= MaximumMarkers)
            {
                frame.Points.Clear();



                List<OwnedPoint> linePoints = new List<OwnedPoint>(frame.LinePoints.Values.Count);

                foreach (OwnedPoint point in frame.LinePoints.Values)
                {
                    linePoints.Add(point);
                }
                FastItemsSource fastItemsSource = FastItemsSource;
                linePoints.Sort(
                    (p1, p2) => {
                        var index1 = fastItemsSource.IndexOf(p1.OwnerItem);
                        var index2 = fastItemsSource.IndexOf(p2.OwnerItem);
                        if (index1 < index2)
                        {
                            return -1;
                        }
                        if (index1 > index2)
                        {
                            return 1;
                        }
                        return 0;
                    });

                if (clipper != null)
                {
                    clipper.Target = frame.Points;
                }

                foreach (OwnedPoint point in linePoints)
                {
                    if (fastItemsSource.IndexOf(point.OwnerItem) >= 0)
                    {
                        if (clipper != null)
                        {
                            clipper.Add(point.Point);
                        }
                        else
                        {
                            frame.Points.Add(new Point(point.Point.X, point.Point.Y));
                        }
                    }
                }
            }
        }

        //internal UnfilteredOwnedPoint GetNewPoint(object item)
        //{
        //    return GetNewPoint(FastItemsSource[item]);
        //}

        //internal UnfilteredOwnedPoint GetNewPoint(int index)
        //{
        //    Rect viewportRect;
        //    Rect windowRect;
        //    GetViewInfo(out viewportRect, out windowRect);

        //    return new UnfilteredOwnedPoint()
        //    {
        //        ColumnValues = new Point(XColumn[index], YColumn[index]),
        //        OwnerItem = FastItemsSource[index],
        //        Point = new Point(XAxis.GetScaledValue(XColumn[index], windowRect, viewportRect),
        //            YAxis.GetScaledValue(YColumn[index], windowRect, viewportRect))
        //    };
        //}

        //private void UpdateCachedPoints(FastItemsSourceEventAction action, int position, int count, string propertyName)
        //{
        //    if (ClearAndAbortIfInvalid())
        //    {
        //        return;
        //    }

        //    switch (action)
        //    {
        //        case FastItemsSourceEventAction.Change:
        //            if (!CurrentFrame.ChangedPoints.ContainsKey(FastItemsSource[position]))
        //            {
        //                CurrentFrame.ChangedPoints.Add(
        //                    FastItemsSource[position], 
        //                    CurrentFrame.CachedPoints[position]);
        //            }
        //            break;
        //        case FastItemsSourceEventAction.Insert:
        //            for (int i = position; i < count; i++)
        //            {
        //                CurrentFrame.CachedPoints.Insert(i,
        //                    GetNewPoint(i));
        //            }
        //            break;
        //        case FastItemsSourceEventAction.Remove:
        //            for (int i = position; i < count; i++)
        //            {
        //                CurrentFrame.ChangedPoints.Add(
        //                    FastItemsSource[position],
        //                    CurrentFrame.CachedPoints[position]);
        //                CurrentFrame.CachedPoints.RemoveAt(position);
        //            }
        //            break;
        //        case FastItemsSourceEventAction.Replace:
        //            for (int i = position; i < count; i++)
        //            {
        //                CurrentFrame.ChangedPoints.Add(
        //                    FastItemsSource[i],
        //                    CurrentFrame.CachedPoints[i]);
        //                CurrentFrame.CachedPoints[i] = 
        //                    GetNewPoint(i);
        //            }
        //            break;
        //        case FastItemsSourceEventAction.Reset:
        //            CurrentFrame.CachedPoints.Clear();
        //            break;
        //    }
        //}

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (axis != null && axis == XAxis && XColumn != null)
            {
                return new AxisRange(XColumn.Minimum, XColumn.Maximum);
            }

            if (axis != null && axis == YAxis && YColumn != null)
            {
                return new AxisRange(YColumn.Minimum, YColumn.Maximum);
            }

            return null;
        }

        /// <summary>
        /// Gets the item that is the best match for the specified world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The item that is the best match.</returns>
        protected override object GetItem(Point world)
        {
            return null;
        }

        /// <summary>
        /// Gets the index of the item that resides at the provided world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates of the requested item.</param>
        /// <returns>The requested item's index.</returns>
        protected override int GetItemIndex(Point world)
        {
            return -1;
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
            Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            Rect viewportRect = SeriesViewer != null ? SeriesViewer.ViewportRect : Rect.Empty;
            Rect unitRect = new Rect(0, 0, 1, 1);
            ScalerParams xParams = new ScalerParams(unitRect, unitRect, XAxis.IsInverted);
            ScalerParams yParams = new ScalerParams(unitRect, unitRect, YAxis.IsInverted);

            int index = !windowRect.IsEmpty && !viewportRect.IsEmpty && FastItemsSource != null ? FastItemsSource.IndexOf(item) : -1;
            double cx = XAxis != null && XColumn != null && index < XColumn.Count ? XAxis.GetScaledValue(XColumn[index], xParams) : double.NaN;
            double cy = YAxis != null && YColumn != null && index < YColumn.Count ? YAxis.GetScaledValue(YColumn[index], yParams) : double.NaN;

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
        /// Overridden in derived classes when they want to respond to the viewport changing.
        /// </summary>
        /// <param name="oldViewportRect">The old viewport rectangle.</param>
        /// <param name="newViewportRect">The new viewport rectangle.</param>
        protected override void ViewportRectChangedOverride(Rect oldViewportRect, Rect newViewportRect)
        {
            RenderSeries(false);
        }

        /// <summary>
        /// Overridden in derived classes when they want to respond to the chart's window changing.
        /// </summary>
        /// <param name="oldWindowRect">The old window rectangle of the chart.</param>
        /// <param name="newWindowRect">The new window rectangle of the chart.</param>
        protected override void WindowRectChangedOverride(Rect oldWindowRect, Rect newWindowRect)
        {
            RenderSeries(false);
        }

        internal ScatterFrame PreviousFrame;
        internal ScatterFrame TransitionFrame;
        internal ScatterFrame CurrentFrame;

        internal virtual void CalculateCachedPoints(ScatterFrame frame, int count, Rect windowRect, Rect viewportRect)
        {
            frame.CachedPoints = new Dictionary<object, OwnedPoint>(count);

            FastItemsSource itemsSource = FastItemsSource;
            double x;
            double y;
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, this.AxisInfoCache.XAxisIsInverted)
            {
                EffectiveViewportRect = this.SeriesViewer.EffectiveViewport
            };
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, this.AxisInfoCache.YAxisIsInverted)
            {
                EffectiveViewportRect = this.SeriesViewer.EffectiveViewport
            };

            for (int i = 0; i < count; i++)
            {
                x = XColumn[i];
                y = YColumn[i];
                Point point = new Point(
                            AxisInfoCache.XAxis.GetScaledValue(x, xParams),
                            AxisInfoCache.YAxis.GetScaledValue(y, yParams));

                if (!double.IsInfinity(point.X) &&
                    !double.IsInfinity(point.Y))
                {
                    object item = itemsSource[i];

                    if (!frame.CachedPoints.ContainsKey(item))
                    {
                        var columnValues = new Point(x, y);
                        var p = new Point(point.X, point.Y);
                        frame.CachedPoints.Add(item,
                                               new OwnedPoint()
                                               {
                                                   OwnerItem = item,
                                                   ColumnValues = columnValues,
                                                   Point = p
                                               });
                    }
                }
            }
        }

        internal virtual void PrepareFrame(ScatterFrame frame, ScatterBaseView view)
        {
            frame.Markers.Clear();
            frame.TrendLine.Clear();
            frame.HorizontalErrorBars.Clear();
            frame.VerticalErrorBars.Clear();
            frame.HorizontalErrorBarWidths.Clear();
            frame.VerticalErrorBarHeights.Clear();

            int count =
                    Math.Min(XColumn != null ? XColumn.Count : 0, YColumn != null ? YColumn.Count : 0);

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);
            xParams.EffectiveViewportRect = this.SeriesViewer.EffectiveViewport;

            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, YAxis.IsInverted);
            yParams.EffectiveViewportRect = this.SeriesViewer.EffectiveViewport;

            if (count < 1)
            {
                return;
            }

            AxisInfoCache = new ScatterAxisInfoCache()
            {
                XAxis = XAxis,
                YAxis = YAxis,
                XAxisIsInverted = XAxis.IsInverted,
                YAxisIsInverted = YAxis.IsInverted,




                FastItemsSource = FastItemsSource
            };

            //currently only try for accurate animations if we arent winnowing.
            if (count <= MaximumMarkers)
            {
                CalculateCachedPoints(frame, count, windowRect, viewportRect);
            }

            if (ShouldDisplayMarkers())
            {
                view.MarkerManager.WinnowMarkers(frame.Markers, MaximumMarkers,
                    windowRect, viewportRect, Resolution);
            }

            Clipper clipper = new Clipper(viewportRect, false) { Target = frame.TrendLine };
            double xmin = XAxis.GetUnscaledValue(viewportRect.Left, xParams);
            double xmax = XAxis.GetUnscaledValue(viewportRect.Right, xParams);

            view.TrendLineManager.PrepareLine(frame.TrendLine, TrendLineType, XColumn, YColumn,
                   TrendLinePeriod, (x) => XAxis.GetScaledValue(x, xParams),
                   (y) => YAxis.GetScaledValue(y, yParams),
                   new TrendResolutionParams()
                   {
                       Resolution = Resolution,
                       Viewport = viewportRect,
                       Window = windowRect
                   }, clipper, xmin, xmax);

            PrepareErrorBars(frame, view);

        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var scatterView = (ScatterBaseView)view;
            scatterView.ClearRendering(wipeClean);
        }

        internal virtual void RenderFrame(ScatterFrame frame, ScatterBaseView view)
        {
            Rect viewportRect = view.Viewport;
            
            AxisInfoCache = new ScatterAxisInfoCache()
            {
                XAxis = XAxis,
                YAxis = YAxis,
                XAxisIsInverted = XAxis.IsInverted,
                YAxisIsInverted = YAxis.IsInverted,




            };

            if (ShouldDisplayMarkers())
            {
                view.MarkerManager.Render(frame.Markers, UseLightweightMarkers);
            }
            
            view.RenderMarkers();

            Clipper clipper = new Clipper(double.NaN, viewportRect.Bottom, double.NaN, viewportRect.Top, false)
            {
                Target = view.TrendLineManager.TrendPolyline.Points
            };

            
            view.TrendLineManager.RasterizeTrendLine(frame.TrendLine, clipper);

            RenderErrorBars(frame, view);
        }

        internal void PrepareErrorBars(ScatterFrame frame, ScatterBaseView view)
        {
            ErrorBarsHelper errorBarsHelper = new ErrorBarsHelper(this, view);

            if (this.ErrorBarSettings == null)
            {
                return;
            }

            IErrorBarCalculator horizontalCalculator = this.ErrorBarSettings.HorizontalCalculator;
            IErrorBarCalculator verticalCalculator = this.ErrorBarSettings.VerticalCalculator;

            double errorBarPositionX = 0.0;
            double errorBarPositionY = 0.0;
            double errorBarWidth = 0.0;
            double errorBarHeight = 0.0;

            if (horizontalCalculator != null && errorBarsHelper.IsCalculatorIndependent(horizontalCalculator))
            {
                errorBarsHelper.CalculateIndependentErrorBarPosition(horizontalCalculator, ref errorBarPositionX);
                errorBarsHelper.CalculateIndependentErrorBarSize(horizontalCalculator, this.AxisInfoCache.XAxis, ref errorBarWidth);
            }

            if (verticalCalculator != null && errorBarsHelper.IsCalculatorIndependent(verticalCalculator))
            {
                errorBarsHelper.CalculateIndependentErrorBarPosition(verticalCalculator, ref errorBarPositionY);
                errorBarsHelper.CalculateIndependentErrorBarSize(verticalCalculator, this.AxisInfoCache.YAxis, ref errorBarHeight);
            }

            // compute & save error bar positions to frame
            foreach (object key in frame.Markers.Keys)
            {
                OwnedPoint point = frame.Markers[key];
                #region compute horizontal
                if (horizontalCalculator != null)
                {
                    if (horizontalCalculator.GetCalculatorType() == ErrorBarCalculatorType.Percentage)
                    {
                        double value;
                        NumericAxisBase refAxis, targetAxis;
                        targetAxis = this.AxisInfoCache.XAxis;
                        if (this.ErrorBarSettings.HorizontalCalculatorReference == ErrorBarCalculatorReference.X)
                        {
                            value = point.Point.X;
                            refAxis = this.AxisInfoCache.XAxis;
                        }
                        else
                        {
                            value = point.Point.Y;
                            refAxis = this.AxisInfoCache.YAxis;
                        }
                        errorBarsHelper.CalculateDependentErrorBarSize(value, horizontalCalculator, refAxis, targetAxis, ref errorBarWidth);
                    }
                    else if (horizontalCalculator.GetCalculatorType() == ErrorBarCalculatorType.Data)
                    {
                        IFastItemColumn<double> ErrorXColumn = horizontalCalculator.GetItemColumn();
                        int index = FastItemsSource.IndexOf(key);
                        if (ErrorXColumn != null && index < ErrorXColumn.Count)
                        {
                            double unscaledValue = ErrorXColumn[FastItemsSource.IndexOf(key)];
                            errorBarsHelper.CalculateErrorBarSize(unscaledValue, this.AxisInfoCache.XAxis, ref errorBarWidth);
                        }
                        else
                        {
                            errorBarWidth = double.NaN;
                        }
                    }
                    OwnedPoint p = new OwnedPoint();
                    Point centerHorizontal = errorBarsHelper.CalculateErrorBarCenterHorizontal(horizontalCalculator, this.AxisInfoCache.XAxis, point.Point, errorBarPositionX);
                    p.Point = centerHorizontal;
                    p.OwnerItem = point.OwnerItem;

                    frame.HorizontalErrorBars.Add(key, p);
                    frame.HorizontalErrorBarWidths.Add(key, errorBarWidth);
                }
                #endregion // compute horizontal
                #region compute vertical
                if (verticalCalculator != null)
                {
                    if (verticalCalculator.GetCalculatorType() == ErrorBarCalculatorType.Percentage)
                    {
                        double value;
                        NumericAxisBase refAxis, targetAxis;
                        targetAxis = this.AxisInfoCache.YAxis;
                        if (this.ErrorBarSettings.VerticalCalculatorReference == ErrorBarCalculatorReference.X)
                        {
                            value = point.Point.X;
                            refAxis = this.AxisInfoCache.XAxis;
                        }
                        else
                        {
                            value = point.Point.Y;
                            refAxis = this.AxisInfoCache.YAxis;
                        }
                        errorBarsHelper.CalculateDependentErrorBarSize(value, verticalCalculator, refAxis, targetAxis, ref errorBarHeight);
                    }
                    else if (verticalCalculator.GetCalculatorType() == ErrorBarCalculatorType.Data)
                    {
                        IFastItemColumn<double> ErrorYColumn = verticalCalculator.GetItemColumn();
                        int index = FastItemsSource.IndexOf(key);
                        if (ErrorYColumn != null && index < ErrorYColumn.Count)
                        {
                            double unscaledValue = ErrorYColumn[FastItemsSource.IndexOf(key)];
                            errorBarsHelper.CalculateErrorBarSize(unscaledValue, this.AxisInfoCache.YAxis, ref errorBarHeight);
                        }
                        else
                        {
                            errorBarHeight = double.NaN;
                        }

                    }
                    OwnedPoint p = new OwnedPoint();
                    Point centerVertical = errorBarsHelper.CalculateErrorBarCenterVertical(verticalCalculator, this.AxisInfoCache.YAxis, point.Point, errorBarPositionY);
                    p.Point = centerVertical;
                    p.OwnerItem = point.OwnerItem;

                    frame.VerticalErrorBars.Add(key, p);
                    frame.VerticalErrorBarHeights.Add(key, errorBarHeight);
                }
                #endregion // compute vertical
            }
        }

        internal void RenderErrorBars(ScatterFrame frame, ScatterBaseView view)
        {
            if (!view.HasSurface() || this.ErrorBarSettings == null)
            {
                view.HideErrorBars();
                return;
            }

            RenderErrorBarsHorizontal(frame, view);
            RenderErrorBarsVertical(frame, view);
        }

        private void RenderErrorBarsHorizontal(ScatterFrame frame, ScatterBaseView view)
        {
            view.AttachHorizontalErrorBars();

            ErrorBarsHelper errorBarsHelper = new ErrorBarsHelper(this, view);

            PathGeometry horizontalErrorBarsGeometry = new PathGeometry();

            IErrorBarCalculator horizontalCalculator = this.ErrorBarSettings.HorizontalCalculator;

            foreach (object key in frame.Markers.Keys)
            {
                if (horizontalCalculator != null &&
                    frame.HorizontalErrorBarWidths.ContainsKey(key))
                {
                    double errorBarWidth = frame.HorizontalErrorBarWidths[key];
                    if (!double.IsNaN(errorBarWidth))
                    {
                        Point centerHorizontal = frame.HorizontalErrorBars[key].Point;
                        if (this.ErrorBarSettings.EnableErrorBarsHorizontal == EnableErrorBars.Both ||
                            this.ErrorBarSettings.EnableErrorBarsHorizontal == EnableErrorBars.Positive)
                        {
                            errorBarsHelper.AddErrorBarHorizontal(horizontalErrorBarsGeometry, centerHorizontal, errorBarWidth, true);
                        }
                        if (this.ErrorBarSettings.EnableErrorBarsHorizontal == EnableErrorBars.Both ||
                            this.ErrorBarSettings.EnableErrorBarsHorizontal == EnableErrorBars.Negative)
                        {
                            errorBarsHelper.AddErrorBarHorizontal(horizontalErrorBarsGeometry, centerHorizontal, errorBarWidth, false);
                        }
                    }
                }
            }
            view.ProvideHorizontalErrorBarGeometry(horizontalErrorBarsGeometry);
        }


        private void RenderErrorBarsVertical(ScatterFrame frame, ScatterBaseView view)
        {
            view.AttachVerticalErrorBars();

            ErrorBarsHelper errorBarsHelper = new ErrorBarsHelper(this, view);

            PathGeometry verticalErrorBarsGeometry = new PathGeometry();

            IErrorBarCalculator verticalCalculator = this.ErrorBarSettings.VerticalCalculator;

            foreach (object key in frame.Markers.Keys)
            {
                if (verticalCalculator != null &&
                    frame.VerticalErrorBarHeights.ContainsKey(key))
                {
                    double errorBarHeight = frame.VerticalErrorBarHeights[key];
                    if (!double.IsNaN(errorBarHeight))
                    {
                        Point centerVertical = frame.VerticalErrorBars[key].Point;
                        if (this.ErrorBarSettings.EnableErrorBarsVertical == EnableErrorBars.Both ||
                            this.ErrorBarSettings.EnableErrorBarsVertical == EnableErrorBars.Positive)
                        {
                            errorBarsHelper.AddErrorBarVertical(verticalErrorBarsGeometry, centerVertical, errorBarHeight, true);
                        }
                        if (this.ErrorBarSettings.EnableErrorBarsVertical == EnableErrorBars.Both ||
                            this.ErrorBarSettings.EnableErrorBarsVertical == EnableErrorBars.Negative)
                        {
                            errorBarsHelper.AddErrorBarVertical(verticalErrorBarsGeometry, centerVertical, errorBarHeight, false);
                        }
                    }
                }
            }

            view.ProvideVerticalErrorBarGeometry(verticalErrorBarsGeometry);

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
            bool isValid = true;

            if (!base.ValidateSeries(viewportRect, windowRect, view)
                || windowRect.IsEmpty
                || viewportRect.IsEmpty
                || XAxis == null
                || YAxis == null
                || XAxis.SeriesViewer == null
                || YAxis.SeriesViewer == null
                || XColumn == null
                || YColumn == null
                || XColumn.Count == 0
                || YColumn.Count == 0
                || FastItemsSource == null
                || FastItemsSource.Count != XColumn.Count
                || FastItemsSource.Count != YColumn.Count
                || XAxis.SeriesViewer == null
                || YAxis.SeriesViewer == null
                || XAxis.ActualMinimumValue == XAxis.ActualMaximumValue
                || YAxis.ActualMinimumValue == YAxis.ActualMaximumValue)
            {
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Gets the view info for the series.
        /// </summary>
        /// <param name="viewportRect">The viewport to use.</param>
        /// <param name="windowRect">The window to use.</param>
        protected internal override void GetViewInfo(out Rect viewportRect, out Rect windowRect)
        {
            if (!_cachedViewportRect.IsEmpty &&
               !_cachedWindowRect.IsEmpty)
            {
                viewportRect = _cachedViewportRect;
                windowRect = _cachedWindowRect;
                return;
            }

            viewportRect = this.View.Viewport;
            windowRect = this.View.WindowRect;

            //windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            //viewportRect = View.Viewport;
        }

        /// <summary>
        /// Renders the series.
        /// </summary>
        /// <param name="animate">True if the change should be animated.</param>
        protected internal override void RenderSeriesOverride(bool animate)
        {
            CacheViewInfo();
            try
            {
                if (ClearAndAbortIfInvalid(View))
                {
                    return;
                }

                if (FastItemsSource != null &&
                    FastItemsSource.Count > MaximumMarkers)
                {
                    animate = false;
                }

                if (ShouldAnimate(animate))
                {
                    ScatterFrame previousFrame = PreviousFrame;

                    if (AnimationActive())
                    {
                        PreviousFrame = TransitionFrame;
                        TransitionFrame = previousFrame;
                    }
                    else
                    {
                        PreviousFrame = CurrentFrame;
                        CurrentFrame = previousFrame;
                    }

                    PrepareFrame(CurrentFrame, this.ScatterView);
                    StartAnimation();
                }
                else
                {
                    PrepareFrame(CurrentFrame, this.ScatterView);
                    RenderFrame(CurrentFrame, this.ScatterView);
                    //RenderErrorBars(CurrentFrame);
                }
            }
            finally
            {
                UnCacheViewInfo();
            }
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
            ScatterView.UpdateTrendlineBrush();
            #endregion
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
            if (ClearAndAbortIfInvalid(ThumbnailView))
            {
                return;
            }

            ScatterBaseView thumbnailView = this.ThumbnailView as ScatterBaseView;


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


            ScatterFrame frame = new ScatterFrame();

            this.PrepareFrame(frame, thumbnailView);
            this.RenderFrame(frame, thumbnailView);







            ThumbnailDirty = false;
        }

        internal virtual void RemoveUnusedMarkers(
            IDictionary<object, OwnedPoint> list, 
            HashPool<object, Marker> markers)
        {
            List<object> remove = new List<object>();
            foreach (object key in markers.ActiveKeys)
            {
                if (!list.ContainsKey(key))
                {
                    remove.Add(key);
                }
            }
            foreach (object key in remove)
            {

                markers.Remove(key);
            }
        }

        internal virtual Point[] GetMarkerLocations(HashPool<object, Marker> markers, Point[] locations, Rect windowRect, Rect viewportRect)
        {
            if (locations == null ||
                      locations.Length != AxisInfoCache.FastItemsSource.Count)
            {
                locations = new Point[AxisInfoCache.FastItemsSource.Count];






            }
            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted)
            {
                EffectiveViewportRect = SeriesViewer.EffectiveViewport
            };
            ScalerParams yParams = new ScalerParams(windowRect, viewportRect, YAxis.IsInverted)
            {
                EffectiveViewportRect = SeriesViewer.EffectiveViewport
            };
            double minX = AxisInfoCache.XAxis.GetUnscaledValue(
                viewportRect.Left,
                xParams);
            double maxX = AxisInfoCache.XAxis.GetUnscaledValue(
                viewportRect.Right,
                xParams);

            double minY = AxisInfoCache.YAxis.GetUnscaledValue(
                viewportRect.Bottom,
                yParams);
            double maxY = AxisInfoCache.YAxis.GetUnscaledValue(
                viewportRect.Top,
                yParams);

            if (AxisInfoCache.XAxisIsInverted)
            {
                double swap = minX;
                minX = maxX;
                maxX = swap;
            }
            if (AxisInfoCache.YAxisIsInverted)
            {
                double swap = minY;
                minY = maxY;
                maxY = swap;
            }

            ScatterAxisInfoCache cache = AxisInfoCache;




            NumericXAxis xAxis = cache.XAxis;
            NumericYAxis yAxis = cache.YAxis;
            double x;
            double y;
            var xColumn = XColumn;
            var yColumn = YColumn;

            for (int i = 0; i < AxisInfoCache.FastItemsSource.Count; i++)
            {
                x = xColumn[i];
                y = yColumn[i];

                if (x >= minX && x <= maxX &&
                    y >= minY && y <= maxY)
                {
                    locations[i].X = xAxis.GetScaledValue(x, xParams);
                    locations[i].Y = yAxis.GetScaledValue(y, yParams);
                }
                else
                {
                    locations[i].X = double.NaN;
                    locations[i].Y = double.NaN;
                }
            }
            return locations;
        }

        internal int[] GetActiveIndexes(HashPool<object, Marker> markers, int[] indexes)
        {
            if (indexes == null ||
                  indexes.Length != markers.ActiveCount)
            {
                indexes = new int[markers.ActiveCount];
            }

            int i = 0;
            FastItemsSource source = FastItemsSource;
            foreach (object key in markers.ActiveKeys)
            {
                indexes[i] = source.IndexOf(key);
                i++;
            }
            return indexes;
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
                ScatterView.TrendLineManager.TrendPolyline);

            trendShape.Tags.Add("Trend");
            svd.Shapes.Add(trendShape);
        }
    }

    internal class ScatterAxisInfoCache
    {
        public NumericXAxis XAxis { get; set; }
        public NumericYAxis YAxis { get; set; }




        public bool XAxisIsInverted { get; set; }
        public bool YAxisIsInverted { get; set; }
        public FastItemsSource FastItemsSource { get; set; }
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