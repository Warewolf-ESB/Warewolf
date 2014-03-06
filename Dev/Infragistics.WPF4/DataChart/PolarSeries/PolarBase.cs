using System;



using System.Linq;

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
using System.Diagnostics;
using System.Windows.Data;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class from which all XamDataChart polar series are derived.
    /// </summary>
    [WidgetModuleParent("PolarChart")]
    public abstract class PolarBase
        : MarkerSeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new PolarBaseView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            PolarView = (PolarBaseView)view;
        }
        internal PolarBaseView PolarView { get; set; }

        private PolarFrame _operatingFrame;

        /// <summary>
        /// Initializes a new instance of the PolarBase class.
        /// </summary>
        public PolarBase()
        {
            TransitionFrame.Retransform = RetransformPoint;
            PreviousFrame.Retransform = RetransformPoint;
            CurrentFrame.Retransform = RetransformPoint;

            //ViewportCalculator = new PolarAxisBasedViewportCalculator();
            
            SeriesRenderer = new SeriesRenderer<PolarFrame, PolarBaseView>(
                PrepareFrame,
                RenderFrame, AnimationActive, StartAnimation);
        }

        private void DoGetScaledPoints()
        {
            PolarAxes.GetScaledPoints(_operatingFrame.Transformed, AngleColumn, RadiusColumn,
                                                 _operatingWindowRect, _operatingViewportRect,
                                                 (j, a) => Math.Cos(a), (j, a) => Math.Sin(a));
        }

        private Point[] Locations { get; set; }
        internal PolarAxisInfoCache AxisInfoCache { get; set; }
        

        private Rect _operatingWindowRect;
        private Rect _operatingViewportRect;

        internal SeriesRenderer<PolarFrame, PolarBaseView> SeriesRenderer { get; set; }

        #region AngleMemberPath Dependency Property
        /// <summary>
        /// Gets or sets the path to use to find the angle values for the series.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public string AngleMemberPath
        {
            get
            {
                return (string)GetValue(AngleMemberPathProperty);
            }
            set
            {
                SetValue(AngleMemberPathProperty, value);
            }
        }

        internal const string AngleMemberPathPropertyName = "AngleMemberPath";

        /// <summary>
        /// Identifies the AngleMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty AngleMemberPathProperty = DependencyProperty.Register(AngleMemberPathPropertyName, typeof(string), typeof(PolarBase),
            new PropertyMetadata((sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(AngleMemberPathPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region RadiusMemberPath Dependency Property
        /// <summary>
        /// Gets or sets the path to use to get the radius values for the series.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public string RadiusMemberPath
        {
            get
            {
                return (string)GetValue(RadiusMemberPathProperty);
            }
            set
            {
                SetValue(RadiusMemberPathProperty, value);
            }
        }

        internal const string RadiusMemberPathPropertyName = "RadiusMemberPath";

        /// <summary>
        /// Identifies the RadiusMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusMemberPathProperty = DependencyProperty.Register(RadiusMemberPathPropertyName, typeof(string), typeof(PolarBase),
            new PropertyMetadata((sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(RadiusMemberPathPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        /// <summary>
        /// The column of angle values.
        /// </summary>
        protected IFastItemColumn<double> AngleColumn
        {
            get { return _angleColumn; }
            private set
            {
                if (_angleColumn != value)
                {
                    IFastItemColumn<double> oldXColumn = AngleColumn;

                    _angleColumn = value;
                    RaisePropertyChanged(AngleColumnPropertyName, oldXColumn, AngleColumn);
                }
            }
        }
        private IFastItemColumn<double> _angleColumn;
        internal const string AngleColumnPropertyName = "AngleColumn";

        /// <summary>
        /// The column of radius values.
        /// </summary>
        protected IFastItemColumn<double> RadiusColumn
        {
            get { return _radiusColumn; }
            private set
            {
                if (_radiusColumn != value)
                {
                    IFastItemColumn<double> oldXColumn = RadiusColumn;

                    _radiusColumn = value;
                    RaisePropertyChanged(RadiusColumnPropertyName, oldXColumn, RadiusColumn);
                }
            }
        }
        private IFastItemColumn<double> _radiusColumn;
        internal const string RadiusColumnPropertyName = "RadiusColumn";

        #region AngleAxis Dependency Property
        /// <summary>
        /// Gets the effective angle axis for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public NumericAngleAxis AngleAxis
        {
            get
            {
                return (NumericAngleAxis)GetValue(AngleAxisProperty);
            }
            set
            {
                SetValue(AngleAxisProperty, value);
            }
        }

        internal const string AngleAxisPropertyName = "AngleAxis";

        /// <summary>
        /// Identifies the ActualXAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty AngleAxisProperty = DependencyProperty.Register(AngleAxisPropertyName, typeof(NumericAngleAxis), typeof(PolarBase),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as PolarBase).RaisePropertyChanged(AngleAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region RadiusAxis Depedency Property
        /// <summary>
        /// Gets the effective radius axis for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public NumericRadiusAxis RadiusAxis
        {
            get
            {
                return (NumericRadiusAxis)GetValue(RadiusAxisProperty);
            }
            set
            {
                SetValue(RadiusAxisProperty, value);
            }
        }

        internal const string RadiusAxisPropertyName = "RadiusAxis";

        /// <summary>
        /// Identifies the ActualXAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusAxisProperty = DependencyProperty.Register(RadiusAxisPropertyName, typeof(NumericRadiusAxis), typeof(PolarBase),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as PolarBase).RaisePropertyChanged(RadiusAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region UseCartesianInterpolation Depedency Property
        /// <summary>
        /// Gets or sets whether Cartesian Interpolation should be used rather than Archimedian 
        /// spiral based interpolation.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [WidgetDefaultBoolean(true)]
        public bool UseCartesianInterpolation
        {
            get
            {
                return (bool)GetValue(UseCartesianInterpolationProperty);
            }
            set
            {
                SetValue(UseCartesianInterpolationProperty, value);
            }
        }

        internal const string UseCartesianInterpolationPropertyName = "UseCartesianInterpolation";

        /// <summary>
        /// Identifies the UseCartesianInterpolation dependency property.
        /// </summary>
        public static readonly DependencyProperty UseCartesianInterpolationProperty =
            DependencyProperty.Register(UseCartesianInterpolationPropertyName, typeof(bool), typeof(PolarBase),
            new PropertyMetadata(true, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as PolarBase)
                    .RaisePropertyChanged(UseCartesianInterpolationPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region MaximumMarkers Dependency Property
        /// <summary>
        /// Gets or sets the maximum number of markers displayed by the current series.
        /// <para>If more than the specified number of markers are visible, the polar series will automatically
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
        public static readonly DependencyProperty MaximumMarkersProperty =
            DependencyProperty.Register(MaximumMarkersPropertyName, typeof(int), typeof(PolarBase),
            new PropertyMetadata(400, (sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(MaximumMarkersPropertyName, e.OldValue, e.NewValue);
            }));
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
        public static readonly DependencyProperty TrendLineTypeProperty = DependencyProperty.Register(TrendLineTypePropertyName, typeof(TrendLineType), typeof(PolarBase),
            new PropertyMetadata(TrendLineType.None, (sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(TrendLineTypePropertyName, e.OldValue, e.NewValue);
            }));

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
        public static readonly DependencyProperty TrendLineBrushProperty = DependencyProperty.Register(TrendLineBrushPropertyName, typeof(Brush), typeof(PolarBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(TrendLineBrushPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty ActualTrendLineBrushProperty = DependencyProperty.Register(TrendLineActualBrushPropertyName, typeof(Brush), typeof(PolarBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(TrendLineActualBrushPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty TrendLineThicknessProperty = DependencyProperty.Register(TrendLineThicknessPropertyName, typeof(double), typeof(PolarBase),
            new PropertyMetadata(1.5, (sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(TrendLineThicknessPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty TrendLineDashCapProperty = DependencyProperty.Register(TrendLineDashCapPropertyName, typeof(PenLineCap), typeof(PolarBase),
            new PropertyMetadata((sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(TrendLineDashCapPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty TrendLineDashArrayProperty = DependencyProperty.Register(TrendLineDashArrayPropertyName, typeof(DoubleCollection), typeof(PolarBase),
            new PropertyMetadata((sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(TrendLineDashArrayPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLinePeriod Dependency Property
        /// <summary>
        /// Gets or sets the moving average period for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
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
        public static readonly DependencyProperty TrendLinePeriodProperty = DependencyProperty.Register(TrendLinePeriodPropertyName, typeof(int), typeof(PolarBase),
            new PropertyMetadata(7, (sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(TrendLinePeriodPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        /// <summary>
        /// Identifies the TrendLineZIndex dependency property.
        /// </summary>
        public static readonly DependencyProperty TrendLineZIndexProperty = DependencyProperty.Register(TrendLineZIndexPropertyName, typeof(int), typeof(PolarBase), new PropertyMetadata(1, (sender, e) =>
        {
            (sender as PolarBase).RaisePropertyChanged(TrendLineZIndexPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Sets or Gets the Trendline Z index.
        /// </summary>
        [WidgetDefaultNumber(1)]
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

        #region ClipSeriesToBounds Dependency Property
        /// <summary>
        /// Gets or sets whether to clip the series to the bounds.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Setting this to true can effect performance.
        /// </remarks>
        public bool ClipSeriesToBounds
        {
            get
            {
                return (bool)GetValue(ClipSeriesToBoundsProperty);
            }
            set
            {
                SetValue(ClipSeriesToBoundsProperty, value);
            }
        }

        internal const string ClipSeriesToBoundsPropertyName = "ClipSeriesToBounds";

        /// <summary>
        /// Identifies the ClipSeriesToBounds dependency property.
        /// </summary>
        public static readonly DependencyProperty ClipSeriesToBoundsProperty = DependencyProperty.Register(ClipSeriesToBoundsPropertyName, typeof(bool), typeof(PolarBase),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as PolarBase).RaisePropertyChanged(ClipSeriesToBoundsPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        internal PolarFrame PreviousFrame = new PolarFrame();
        internal PolarFrame TransitionFrame = new PolarFrame();
        internal PolarFrame CurrentFrame = new PolarFrame();

        internal PolarAxes PolarAxes;

        /// <summary>
        /// Invalidates the axes associated with the series.
        /// </summary>
        protected internal override void InvalidateAxes()
        {
            base.InvalidateAxes();
            if (this.RadiusAxis != null)
            {
                this.RadiusAxis.RenderAxis(false);
            }
            if (this.AngleAxis != null)
            {
                this.AngleAxis.RenderAxis(false);
            }
        }

        /// <summary>
        /// Overridden in derived classes to respond to data updates.
        /// </summary>
        /// <param name="action">The action performed on the bound data.</param>
        /// <param name="position">The position at which the action was performed.</param>
        /// <param name="count">The count of affected positions.</param>
        /// <param name="propertyName">The property name changed.</param>
        protected override void DataUpdatedOverride(
            FastItemsSourceEventAction action,
            int position, int count,
            string propertyName)
        {
            bool refresh = false;

            this.PolarView.TrendLineManager.DataUpdated(action, position, count, propertyName);

            if (AngleAxis != null && !AngleAxis.UpdateRange())
            {
                refresh = true;
            }

            if (RadiusAxis != null && !RadiusAxis.UpdateRange())
            {
                refresh = true;
            }

            if (refresh)
            {
                RenderSeries(true);
            }
        }

        /// <summary>
        /// Overridden in derived classes to respond to the viewport changing.
        /// </summary>
        /// <param name="oldViewportRect">The old viewport dimensions.</param>
        /// <param name="newViewportRect">The new viewport dimensions.</param>
        protected override void ViewportRectChangedOverride(
            Rect oldViewportRect, Rect newViewportRect)
        {
            RenderSeries(false);
        }

        /// <summary>
        /// Overridden in derived classes to respond to the window changing.
        /// </summary>
        /// <param name="oldWindowRect">The old window.</param>
        /// <param name="newWindowRect">The new window.</param>
        protected override void WindowRectChangedOverride(
            Rect oldWindowRect, Rect newWindowRect)
        {
            RenderSeries(false);
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

            if (!base.ValidateSeries(viewportRect, windowRect, view) || !view.HasSurface()
                || windowRect.IsEmpty
                || viewportRect.IsEmpty
                || AngleAxis == null
                || RadiusAxis == null
                || AngleColumn == null
                || RadiusColumn == null
                || AngleColumn.Count == 0
                || RadiusColumn.Count == 0
                || FastItemsSource == null
                || FastItemsSource.Count != AngleColumn.Count
                || FastItemsSource.Count != RadiusColumn.Count
                || PolarAxes == null
                || AngleAxis.SeriesViewer == null
                || RadiusAxis.SeriesViewer == null
                || AngleAxis.ActualMinimumValue == AngleAxis.ActualMaximumValue
                || RadiusAxis.ActualMinimumValue == RadiusAxis.ActualMaximumValue)
            {
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Called to render the series.
        /// </summary>
        /// <param name="animate">True if the change should be animated.</param>
        protected internal override void RenderSeriesOverride(bool animate)
        {
            Rect windowRect;
            Rect viewportRect;

            GetViewInfo(out viewportRect, out windowRect);
            if (!ValidateSeries(viewportRect, windowRect, View))
            {
                ClearRendering(true, View);
                return;
            }
            _operatingWindowRect = windowRect;
            _operatingViewportRect = viewportRect;


            AxisInfoCache = new PolarAxisInfoCache(
                AngleAxis,
                RadiusAxis,
                FastItemsSource);

            var args =
                new SeriesRenderingArguments(this, viewportRect, windowRect, animate);



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

            SeriesRenderer.Render(args, ref PreviousFrame, ref CurrentFrame, ref TransitionFrame, this.PolarView);

        }

        private Point RetransformPoint(Point from)
        {
            Point newPoint = PolarAxes.GetScaledPoint(from.X, from.Y,
                                                 _operatingWindowRect,
                                                 _operatingViewportRect,
                                                 AxisInfoCache.AngleAxisIsLogarithmic,
                                                 AxisInfoCache.AngleAxisIsInverted,
                                                 AxisInfoCache.RadiusAxisIsLogarithmic,
                                                 AxisInfoCache.RadiusAxisIsInverted,
                                                 AxisInfoCache.RadiusExtentScale,
                                                 AxisInfoCache.InnerRadiusExtentScale);
            //System.Diagnostics.Debug.WriteLine("from: " + from + " to: " + newPoint);
            return newPoint;
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (axis != null && axis == AngleAxis && AngleColumn != null)
            {
                return new AxisRange(AngleColumn.Minimum, AngleColumn.Maximum);
            }
            if (axis != null && axis == RadiusAxis && RadiusColumn != null)
            {
                return new AxisRange(RadiusColumn.Minimum, RadiusColumn.Maximum);
            }

            return null;
        }

        /// <summary>
        /// Scrolls the requested item into view, if possible.
        /// </summary>
        /// <param name="item">The item to scroll into view.</param>
        /// <returns>True if it was possible to scroll the item into view.</returns>
        public override bool ScrollIntoView(object item)
        {
            Rect windowRect = View.WindowRect;
            Rect viewportRect = View.Viewport;
            int index = !windowRect.IsEmpty && !viewportRect.IsEmpty && FastItemsSource != null ? FastItemsSource[item] : -1;

            if (AngleAxis == null ||
                AngleColumn == null ||
                RadiusAxis == null ||
                RadiusColumn == null)
            {
                return false;
            }

            if (index < 0 ||
                index > AngleColumn.Count - 1 ||
                index > RadiusColumn.Count - 1)
            {
                return false;
            }

            double scaledAngle = AngleAxis.GetScaledAngle(AngleColumn[index]);
            double scaledRadius = RadiusAxis.GetScaledValue(RadiusColumn[index]);
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
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender,
            string propertyName,
            object oldValue,
            object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            if (this.PolarView.TrendLineManager.PropertyUpdated(sender, propertyName, oldValue, newValue,
                this.TrendLineDashArray))
            {
                RenderSeries(false);
                this.NotifyThumbnailAppearanceChanged();
            }

            switch (propertyName)
            {
                case FastItemsSourcePropertyName:
                    if (oldValue as FastItemsSource != null)
                    {
                        (oldValue as FastItemsSource).DeregisterColumn(AngleColumn);
                        (oldValue as FastItemsSource).DeregisterColumn(RadiusColumn);
                        AngleColumn = null;
                        RadiusColumn = null;
                    }

                    if (newValue as FastItemsSource != null)
                    {
                        AngleColumn = RegisterDoubleColumn(AngleMemberPath);
                        RadiusColumn = RegisterDoubleColumn(RadiusMemberPath);
                    }

                    if ((RadiusAxis != null && !RadiusAxis.UpdateRange()) ||
                        (AngleAxis != null && !AngleAxis.UpdateRange()))
                    {
                        RenderSeries(false);
                    }

                    break;

                case AngleAxisPropertyName:
                    if (AngleAxis != null && RadiusAxis != null)
                    {
                        PolarAxes = new PolarAxes(RadiusAxis, AngleAxis);
                    }

                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }

                    if (AngleAxis != null && !AngleAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }
                    else if (oldValue != null && newValue == null)
                    {
                        ClearRendering(true, View);
                    }
                    break;

                case RadiusAxisPropertyName:
                    if (AngleAxis != null && RadiusAxis != null)
                    {
                        PolarAxes = new PolarAxes(RadiusAxis, AngleAxis);
                    }

                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }

                    if (RadiusAxis != null && !RadiusAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }
                    else if (oldValue != null && newValue == null)
                    {
                        ClearRendering(true, View);
                    }

                    if (AngleAxis != null && !AngleAxis.UpdateRange())
                    {
                        AngleAxis.Refresh();
                    }
                    break;

                #region Angle Mapping and Column
                case AngleMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(AngleColumn);
                        AngleColumn = RegisterDoubleColumn(AngleMemberPath);
                    }

                    break;

                case AngleColumnPropertyName:
                    this.PolarView.TrendLineManager.Reset();

                    if (AngleAxis != null && !AngleAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }
                    break;
                #endregion

                #region Y Mapping and Column
                case RadiusMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(RadiusColumn);
                        RadiusColumn = RegisterDoubleColumn(RadiusMemberPath);
                    }

                    break;

                case RadiusColumnPropertyName:
                    this.PolarView.TrendLineManager.Reset();

                    if (RadiusAxis != null && !RadiusAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }
                    break;
                #endregion

                case UseCartesianInterpolationPropertyName:
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case MaximumMarkersPropertyName:
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case TransitionProgressPropertyName:
                    AxisInfoCache = new PolarAxisInfoCache(
                       AngleAxis,
                       RadiusAxis,
                       FastItemsSource);
                    _operatingWindowRect = View.WindowRect;
                    _operatingViewportRect = View.Viewport;
                    TransitionFrame.UseCartesianInterpolation = UseCartesianInterpolation;
                    TransitionFrame.Interpolate((float)TransitionProgress, PreviousFrame, CurrentFrame);

                    if (ClearAndAbortIfInvalid(View))
                    {
                        return;
                    }

                    if (TransitionProgress == 1.0)
                    {
                        RenderFrame(CurrentFrame, PolarView);
                    }
                    else
                    {
                        RenderFrame(TransitionFrame, PolarView);
                    }
                    break;

                case TrendLineBrushPropertyName:
                    this.UpdateIndexedProperties();
                    break;
                case ClipSeriesToBoundsPropertyName:
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
                case TrendLineTypePropertyName:
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var polarView = (PolarBaseView)view;
            if (wipeClean)
            {
                polarView.Markers.Clear();
            }
            polarView.TrendLineManager.ClearPoints();
        }

        private Clipper GetTrendlineClipper(Rect viewportRect, IList<Point> target)
        {
            double top = viewportRect.Top - 10;
            double bottom = viewportRect.Bottom + 10;
            double left = viewportRect.Left - 10;
            double right = viewportRect.Right + 10;

            var clipper = new Clipper(left, bottom, right, top, false)
            {
                Target = target
            };
            return clipper;
        }

        internal virtual void CalculateCachedPoints(PolarFrame frame, int count, Rect windowRect, Rect viewportRect)
        {
            frame.CachedPoints = new Dictionary<object, OwnedPoint>(count);

            FastItemsSource itemsSource = FastItemsSource;
            for (int i = 0; i < count; i++)
            {
                Point point = frame.Transformed[i];

                if (!double.IsInfinity(point.X) &&
                    !double.IsInfinity(point.Y))
                {
                    var columnValues = new Point(AngleColumn[i], RadiusColumn[i]);
                    var p = new Point(point.X, point.Y);
                    frame.CachedPoints.Add(itemsSource[i],
                                           new OwnedPoint()
                                           {
                                               OwnerItem = itemsSource[i],
                                               ColumnValues = columnValues,
                                               Point = p
                                           });
                }
            }
        }

        internal virtual void PrepareFrame(PolarFrame frame, PolarBaseView view)
        {
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            frame.Markers.Clear();
            frame.TrendLine.Clear();

            int count =
                    Math.Min(AngleColumn != null ? AngleColumn.Count : 0, RadiusColumn != null ? RadiusColumn.Count : 0);

            if (count < 1)
            {
                return;
            }

            AxisInfoCache = new PolarAxisInfoCache(
              AngleAxis,
              RadiusAxis,
              FastItemsSource);

            _operatingFrame = frame;
            _operatingViewportRect = viewportRect;
            _operatingWindowRect = windowRect;

            view.MarkerManager.WinnowMarkers(frame.Markers, MaximumMarkers,
                windowRect, viewportRect, Resolution);

            //currently only try for accurate animations if we arent winnowing.
            if (count <= MaximumMarkers)
            {
                CalculateCachedPoints(frame, count, windowRect, viewportRect);
            }

            Clipper clipper = GetTrendlineClipper(viewportRect, frame.TrendLine); //new Clipper(viewportRect, false) { Target = frame.TrendLine };
            double angleMin = Math.Min(AngleAxis.ActualMinimumValue, AngleAxis.ActualMaximumValue);
            double angleMax = Math.Max(AngleAxis.ActualMaximumValue, AngleAxis.ActualMinimumValue);

            view.TrendLineManager.UseCartesianInterpolation = UseCartesianInterpolation;
            view.TrendLineManager.UnknownValuePlotting = UnknownValuePlotting.LinearInterpolate;
            view.TrendLineManager.RadiusExtentScale = RadiusAxis.ActualRadiusExtentScale;
            view.TrendLineManager.InnerRadiusExtentScale = RadiusAxis.ActualInnerRadiusExtentScale;
            view.TrendLineManager.ProjectX =
                (angle, radius) =>
                {
                    return PolarAxes.GetXValue(angle, radius, windowRect, viewportRect, Math.Cos);
                };
            view.TrendLineManager.ProjectY =
                (angle, radius) =>
                {
                    return PolarAxes.GetYValue(angle, radius, windowRect, viewportRect, Math.Sin);
                };
            view.TrendLineManager.PrepareLine(
                frame.TrendLine,
                TrendLineType,
                from angle in AngleColumn where angle <= angleMax && angle >= angleMin select angle,
                RadiusColumn,
                   TrendLinePeriod,
                   AngleAxis.GetScaledAngle,
                   RadiusAxis.GetScaledValue,
                   new TrendResolutionParams()
                   {
                       Resolution = Resolution,
                       Viewport = viewportRect,
                       Window = windowRect
                   }, clipper, angleMin, angleMax);
        }

        internal virtual void RenderFrame(PolarFrame frame, PolarBaseView view)
        {
            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            AxisInfoCache = new PolarAxisInfoCache(
              AngleAxis,
              RadiusAxis,
              FastItemsSource);
            view.MarkerManager.Render(frame.Markers, UseLightweightMarkers);
            view.RenderMarkers();

            Clipper clipper = GetTrendlineClipper(viewportRect, view.TrendLineManager.TrendPolyline.Points);
            //new Clipper(double.NaN, ViewportRect.Bottom, double.NaN, ViewportRect.Top, false)
            //{
            //    Target = TrendLineManager.TrendPolyline.Points
            //};
            view.TrendLineManager.RasterizeTrendLine(frame.TrendLine, clipper);

            ApplyClipping(viewportRect, windowRect, view);
        }

        private void ApplyClipping(Rect viewportRect, Rect windowRect, PolarBaseView view)
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
            PolarView.UpdateTrendlineBrush();
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

            PolarBaseView thumbnailView = (PolarBaseView)this.ThumbnailView;
            //PolarBaseView originalView = this.PolarView;

            PolarFrame frame = new PolarFrame();

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

        internal void RemoveUnusedMarkers(IDictionary<object, OwnedPoint> list, HashPool<object, Marker> markers)
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

        internal Point[] GetMarkerLocations(HashPool<object, Marker> Markers, Rect WindowRect, Rect Viewport)
        {
            DoGetScaledPoints();
            return _operatingFrame.Transformed.ToArray();
        }

        internal List<int> GetActiveIndexes(HashPool<object, Marker> Markers)
        {
            List<int> indexes = new List<int>();
            FastItemsSource source = FastItemsSource;
            foreach (object key in this.PolarView.Markers.ActiveKeys)
            {
                indexes.Add(source.IndexOf(key));
            }
            return indexes;
        }

        internal Point GetColumnValues(int i)
        {
            return new Point(AngleColumn[i], RadiusColumn[i]);
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
                PolarView.TrendLineManager.TrendPolyline);

            trendShape.Tags.Add("Trend");
            svd.Shapes.Add(trendShape);
        }
    }

    internal class PolarAxisInfoCache
    {
        public NumericAngleAxis AngleAxis { get; set; }
        public NumericRadiusAxis RadiusAxis { get; set; }
        public bool AngleAxisIsLogarithmic { get; set; }
        public bool RadiusAxisIsLogarithmic { get; set; }
        public bool AngleAxisIsInverted { get; set; }
        public bool RadiusAxisIsInverted { get; set; }
        public FastItemsSource FastItemsSource { get; set; }
        public double RadiusExtentScale { get; set; }
        public double InnerRadiusExtentScale { get; set; }

        public PolarAxisInfoCache(NumericAngleAxis numAxis, NumericRadiusAxis radAxis, FastItemsSource itemsSource)
        {
            AngleAxis = numAxis;
            RadiusAxis = radAxis;
            AngleAxisIsLogarithmic = AngleAxis.IsReallyLogarithmic;
            AngleAxisIsInverted = AngleAxis.IsInverted;
            RadiusAxisIsLogarithmic = RadiusAxis.IsReallyLogarithmic;
            RadiusAxisIsInverted = RadiusAxis.IsInverted;
            RadiusExtentScale = RadiusAxis.ActualRadiusExtentScale;
            InnerRadiusExtentScale = RadiusAxis.ActualInnerRadiusExtentScale;
            FastItemsSource = itemsSource;
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