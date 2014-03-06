using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Windows.Controls;
using System.Diagnostics;



using System.Linq;

using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Base class for anchored category series with a category x-axis and a numeric y-axis.
    /// </summary>
    public abstract class HorizontalAnchoredCategorySeries : AnchoredCategorySeries
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
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(XAxisPropertyName, typeof(CategoryAxisBase), typeof(HorizontalAnchoredCategorySeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as HorizontalAnchoredCategorySeries).RaisePropertyChanged(XAxisPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(YAxisPropertyName, typeof(NumericYAxis), typeof(HorizontalAnchoredCategorySeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as HorizontalAnchoredCategorySeries).RaisePropertyChanged(YAxisPropertyName, e.OldValue, e.NewValue);
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
        internal override void SetXAxis(Axis xAxis)
        {
            this.XAxis = xAxis as CategoryAxisBase;
        }
        internal override void SetYAxis(Axis yAxis)
        {
            this.YAxis = yAxis as NumericYAxis;
        }
        internal override bool UpdateNumericAxisRange()
        {
            return this.YAxis != null && this.YAxis.UpdateRange();
        }
        /// <summary>
        /// A cached value for the x axis.
        /// </summary>
        protected CategoryAxisBase CachedXAxis { get; set; }
        /// <summary>
        /// A cached value for the y axis.
        /// </summary>
        protected NumericYAxis CachedYAxis { get; set; }
        internal void CacheXAxis(CategoryAxisBase xAxis)
        {
            CachedXAxis = xAxis;
        }

        internal void CacheYAxis(NumericYAxis yAxis)
        {
            CachedYAxis = yAxis;
        }
        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated on the series or owning chart. Gives the series a chance to respond to the various property updates.
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
                    this.AnchoredView.TrendLineManager = CategoryTrendLineManagerBase.SelectManager(
                        this.AnchoredView.TrendLineManager, XAxis, RootCanvas, this);
                    break;
            }

            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);
            switch (propertyName)
            {
                case XAxisPropertyName:
                    DeregisterForAxis(oldValue as Axis);
                    RegisterForAxis(newValue as Axis);
                    CachedXAxis = XAxis;
                    CacheXAxis(XAxis);
                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case YAxisPropertyName:
                    DeregisterForAxis(oldValue as Axis);
                    RegisterForAxis(newValue as Axis);
                    CachedYAxis = YAxis;
                    CacheYAxis(YAxis);
                    this.CategoryView.BucketCalculator.CalculateBuckets(Resolution);
                    this.UpdateNumericAxisRange();
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }

        /// <summary>
        /// Called when the data assigned to the series has been updated.
        /// </summary>
        /// <param name="action">The action that was performed on the data.</param>
        /// <param name="position">The position that was updated in the data.</param>
        /// <param name="count">The number of items that were affected by the data change.</param>
        /// <param name="propertyName">The name of the property that has been changed on a data item.</param>
        protected override void DataUpdatedOverride(FastItemsSourceEventAction action, int position, int count, string propertyName)
        {
            base.DataUpdatedOverride(action, position, count, propertyName);
            if (CachedXAxis != null && CachedXAxis is ISortingAxis)
            {
                (CachedXAxis as ISortingAxis).NotifyDataChanged();
            }
            switch (action)
            {
                case FastItemsSourceEventAction.Reset:
                    if (CachedXAxis != null)
                    {
                        CachedXAxis.UpdateRange();
                    }

                    if (CachedYAxis != null && !CachedYAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }

                    break;

                case FastItemsSourceEventAction.Insert:
                    if (CachedXAxis != null)
                    {
                        CachedXAxis.UpdateRange();
                    }

                    if (CachedYAxis != null && !CachedYAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }

                    break;

                case FastItemsSourceEventAction.Remove:
                    if (CachedXAxis != null)
                    {
                        CachedXAxis.UpdateRange();
                    }

                    if (CachedYAxis != null && !CachedYAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }

                    break;

                case FastItemsSourceEventAction.Replace:
                    if (CachedValueMemberPath != null && this.AnchoredView.BucketCalculator.BucketSize > 0
                        && CachedYAxis != null && !CachedYAxis.UpdateRange())
                    {
                        RenderSeries(true);
                    }
                    break;

                case FastItemsSourceEventAction.Change:
                    if (propertyName == CachedValueMemberPath)
                    {
                        if (XAxis != null)
                        {
                            XAxis.UpdateRange();
                        }

                        if (YAxis != null && !YAxis.UpdateRange())
                        {
                            RenderSeries(true);
                        }
                    }

                    break;
            }
        }
    }
    /// <summary>
    /// Represents a vertically laid out category based series.
    /// </summary>
    public abstract class VerticalAnchoredCategorySeries : AnchoredCategorySeries
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
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(XAxisPropertyName, typeof(NumericXAxis), typeof(VerticalAnchoredCategorySeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as VerticalAnchoredCategorySeries).RaisePropertyChanged(XAxisPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(YAxisPropertyName, typeof(CategoryYAxis), typeof(VerticalAnchoredCategorySeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as VerticalAnchoredCategorySeries).RaisePropertyChanged(YAxisPropertyName, e.OldValue, e.NewValue);
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
        internal override void SetXAxis(Axis xAxis)
        {
            this.XAxis = xAxis as NumericXAxis;
        }
        internal override void SetYAxis(Axis yAxis)
        {
            this.YAxis = yAxis as CategoryYAxis;
        }
        internal override bool UpdateNumericAxisRange()
        {
            return this.XAxis != null && this.XAxis.UpdateRange();
        }
        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated on the series or owning chart. Gives the series a chance to respond to the various property updates.
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
    /// Represents the base class for all XamDataChart anchored category/value series.
    /// </summary>
    [WidgetModuleParent("CategoryChart")]
    public abstract class AnchoredCategorySeries
        : CategorySeries, IIsCategoryBased, IHasSingleValueCategory, IHasCategoryTrendline
    {
        internal AnchoredCategorySeriesView AnchoredView { get; set; }
        
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            var view = new AnchoredCategorySeriesView(this);
            return view;
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            AnchoredView = (AnchoredCategorySeriesView)view;
           
        }

        #region Constructor and Initialisation
        /// <summary>
        /// Initializes a new instance of the AnchoredCategorySeries class. 
        /// </summary>
        internal protected AnchoredCategorySeries()
        {
            LineRasterizer = new CategoryLineRasterizer();
            FramePreparer = new CategoryFramePreparer(this, this.CategoryView, this, this, this.CategoryView.BucketCalculator);
        }
        
        internal CategoryLineRasterizer LineRasterizer { get; set; }
        

        #endregion

        #region ValueMemberPath Dependency Property
        /// <summary>
        /// Gets or sets the value mapping property for the current series object.
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
        /// String path to the value column.
        /// </summary>
        protected string _valueMemberPath;

        /// <summary>
        /// Identifies the ValueMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(ValueMemberPathPropertyName, typeof(string), typeof(AnchoredCategorySeries), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as AnchoredCategorySeries).RaisePropertyChanged(ValueMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets the FastItemColumn representing the mapped values in the items source.
        /// </summary>
        protected internal IFastItemColumn<double> ValueColumn
        {
            get { return valueColumn; }
            set
            {
                if (valueColumn != value)
                {
                    IFastItemColumn<double> oldValueColumn = valueColumn;

                    valueColumn = value;
                    RaisePropertyChanged(ValueColumnPropertyName, oldValueColumn, valueColumn);
                }
            }
        }
        private IFastItemColumn<double> valueColumn;
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
        public static readonly DependencyProperty TrendLineTypeProperty = DependencyProperty.Register(TrendLineTypePropertyName, typeof(TrendLineType), typeof(AnchoredCategorySeries),
            new PropertyMetadata(TrendLineType.None, (sender, e) =>
            {
                (sender as AnchoredCategorySeries).RaisePropertyChanged(TrendLineTypePropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty TrendLineBrushProperty = DependencyProperty.Register(TrendLineBrushPropertyName, typeof(Brush), typeof(AnchoredCategorySeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as AnchoredCategorySeries).RaisePropertyChanged(TrendLineBrushPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty ActualTrendLineBrushProperty = DependencyProperty.Register(TrendLineActualBrushPropertyName, typeof(Brush), typeof(AnchoredCategorySeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as AnchoredCategorySeries).RaisePropertyChanged(TrendLineActualBrushPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty TrendLineThicknessProperty = DependencyProperty.Register(TrendLineThicknessPropertyName, typeof(double), typeof(AnchoredCategorySeries),
            new PropertyMetadata(1.5, (sender, e) =>
            {
                (sender as AnchoredCategorySeries).RaisePropertyChanged(TrendLineThicknessPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineDashCap Property
        /// <summary>
        /// Gets or sets the PenLineCap enumeration value that specifies how the current
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
        public static readonly DependencyProperty TrendLineDashCapProperty = DependencyProperty.Register(TrendLineDashCapPropertyName, typeof(PenLineCap), typeof(AnchoredCategorySeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as AnchoredCategorySeries).RaisePropertyChanged(TrendLineDashCapPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty TrendLineDashArrayProperty = DependencyProperty.Register(TrendLineDashArrayPropertyName, typeof(DoubleCollection), typeof(AnchoredCategorySeries),
            new PropertyMetadata((sender, e) =>
            {
                (sender as AnchoredCategorySeries).RaisePropertyChanged(TrendLineDashArrayPropertyName, e.OldValue, e.NewValue);
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
        public static readonly DependencyProperty TrendLinePeriodProperty = DependencyProperty.Register(TrendLinePeriodPropertyName, typeof(int), typeof(AnchoredCategorySeries),
            new PropertyMetadata(7, (sender, e) =>
            {
                (sender as AnchoredCategorySeries).RaisePropertyChanged(TrendLinePeriodPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region TrendLineZIndex Dependency Property
        /// <summary>
        /// Identifies the TrendLineZIndex dependency property
        /// </summary>
        public static readonly DependencyProperty TrendLineZIndexProperty = DependencyProperty.Register(TrendLineZIndexPropertyName, typeof(int), typeof(AnchoredCategorySeries), new PropertyMetadata(1001, (sender, e) =>
        {
            (sender as AnchoredCategorySeries).RaisePropertyChanged(TrendLineZIndexPropertyName, e.OldValue, e.NewValue);
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
                return (int)this.GetValue(AnchoredCategorySeries.TrendLineZIndexProperty);
            }
            set
            {
                this.SetValue(AnchoredCategorySeries.TrendLineZIndexProperty, value);
            }
        }
        #endregion

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
            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;
            Rect unitRect = new Rect(0, 0, 1, 1);

            int index =
                !windowRect.IsEmpty
                && !viewportRect.IsEmpty
                && FastItemsSource != null ? FastItemsSource.IndexOf(item) : -1;

            Axis xAxis = this.GetXAxis();

            ScalerParams xParams = new ScalerParams(unitRect, unitRect, xAxis.IsInverted);

            double cx = xAxis != null ? xAxis.GetScaledValue(index, xParams) : double.NaN;
            double offset = xAxis != null ? FramePreparer.GetOffset(xAxis as ICategoryScaler, windowRect, viewportRect) : 0.0;
            cx += offset;

            Axis yAxis = this.GetYAxis();
            double cy =
                yAxis != null
                && ValueColumn != null
                && index < ValueColumn.Count ? yAxis.GetScaledValue(ValueColumn[index], xParams) : double.NaN;

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
        /// When overridden in a derived class, gets called whenever a property value is updated on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            NumericAxisBase yaxis = this.GetYAxis() as NumericAxisBase;

            if (this.AnchoredView.TrendLineManager.PropertyUpdated(
                sender, propertyName, oldValue, newValue,
              TrendLineDashArray))
            {
                RenderSeries(false);
                this.NotifyThumbnailAppearanceChanged();
            }

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
                    }

                    if (yaxis != null && !yaxis.UpdateRange())
                    {
                        this.AnchoredView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }

                    break;

                case AnchoredCategorySeries.ValueMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(ValueColumn);
                        ValueColumn = RegisterDoubleColumn(ValueMemberPath);
                    }
                    CachedValueMemberPath = ValueMemberPath;
                    break;

                case AnchoredCategorySeries.ValueColumnPropertyName:
                    this.AnchoredView.TrendLineManager.Reset();

                    if (yaxis != null && !yaxis.UpdateRange())
                    {
                        this.AnchoredView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;

                case TrendLineBrushPropertyName:
                    this.UpdateIndexedProperties();
                    break;

                case AnchoredCategorySeries.ErrorBarSettingsPropertyName:
                    RenderSeries(false);
                    break;
                case TrendLineTypePropertyName:
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (ValueColumn == null || ValueColumn.Count == 0)
            {
                return null;
            }

            if (axis == this.GetXAxis())
            {
                return new AxisRange(0, ValueColumn.Count - 1);
            }

            if (axis == this.GetYAxis())
            {
                return new AxisRange(ValueColumn.Minimum, ValueColumn.Maximum);
            }

            return null;
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
                    this.AnchoredView.BucketCalculator.CalculateBuckets(Resolution);
                    break;
            }



            this.AnchoredView.TrendLineManager.DataUpdated(action, position, count, propertyName);

            
        }     

        internal virtual double GetWorldZeroValue(CategorySeriesView view)
        {
            double value = 0.0;

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;
            NumericYAxis yAxis = this.GetYAxis() as NumericYAxis;
            if (!windowRect.IsEmpty && !viewportRect.IsEmpty && yAxis != null)
            {
                ScalerParams p = new ScalerParams(windowRect, viewportRect, yAxis.IsInverted);
                value = yAxis.GetScaledValue(yAxis.ReferenceValue, p);
            }

            return value;
        }



#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


        internal void TerminatePolygon(PointCollection polygon, int count, CategorySeriesView view)
        {
            double worldZeroValue = GetWorldZeroValue(view);
            if (polygon.Count > 0)
            {
                double zero = worldZeroValue;

                polygon.Add(new Point(polygon.Last().X, zero));
                polygon.Add(new Point(polygon.First().X, zero));
            }
        }



#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

        internal Clipper GetLineClipper(Func<int, double> x0, int endIndex, Rect viewportRect, Rect windowRect)
        {
            Clipper clipper = null;
            
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
        /// Checks if the series is valid to be rendered.
        /// </summary>
        /// <param name="viewportRect">The current viewport, a rectangle with bounds equivalent to the screen size of the series.</param>
        /// <param name="windowRect">The current window, a rectangle bounded between 0 and 1 representing the pan and zoom position.</param>
        /// <param name="view">The SeriesView in context.</param>
        /// <returns>True if the series is valid to be rendered, otherwise false.</returns>
        protected internal override bool ValidateSeries(Rect viewportRect, Rect windowRect, SeriesView view)
        {
            bool isValid = base.ValidateSeries(viewportRect, windowRect, view);
            var anchoredView = (AnchoredCategorySeriesView)view;
            if (ValueColumn == null
                || ValueColumn.Count == 0
                || anchoredView.BucketCalculator.BucketSize < 1)
            {
                isValid = false;
            }
            return isValid;
        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);
            var catView = (AnchoredCategorySeriesView)view;
            catView.HideErrorBars();
            catView.TrendLineManager.ClearPoints();
        }

        internal override void PrepareFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.PrepareFrame(frame, view);
            this.GetFramePreparer(view).PrepareFrame(frame, view);
        }

        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);

            AnchoredCategorySeriesView anchoredView  = view as AnchoredCategorySeriesView;
            anchoredView.TrendLineManager.RasterizeTrendLine(frame.Trend);
            CategoryMarkerManager.RasterizeMarkers(this, frame.Markers, view.Markers, UseLightweightMarkers);
            view.RenderMarkers();
            RenderErrorBars(frame, view);
        }

        internal void RenderErrorBars(CategoryFrame frame, CategorySeriesView view)
        {

            if (frame == null ||
                !((ISupportsErrorBars)this).ShouldDisplayErrorBars() ||
                this.ErrorBarSettings.EnableErrorBars == EnableErrorBars.None)
            {
                view.HideErrorBars();
                return;
            }

            view.ShowErrorBars();

            PathGeometry errorBarsGeometry = new PathGeometry();
            ErrorBarsHelper errorBarsHelper = new ErrorBarsHelper(this, view);
            double errorBarSize = 0.0;

            NumericAxisBase axis;
            Action<PathGeometry, Point, double, bool> AddErrorBarAction;
            if (this is ColumnSeries ||
                this is SplineSeries ||
                this is LineSeries ||
                this is AreaSeries ||
                this is StepLineSeries ||
                this is StepAreaSeries ||
                this is SplineAreaSeries)
            {
                axis = this.GetYAxis() as NumericAxisBase;
                AddErrorBarAction = errorBarsHelper.AddErrorBarVertical;
            }
            else if (this is BarSeries)
            {
                axis = ((BarSeries)this).XAxis;
                AddErrorBarAction = errorBarsHelper.AddErrorBarHorizontal;
            }
            else
            {
                return;
            }

            for (int i = 0; i < frame.ErrorBars.Count && i < frame.ErrorBarSizes.Count; i++)
            {
                Point position = frame.ErrorBars[i];
                errorBarSize = frame.ErrorBarSizes[i];

                if (this.ErrorBarSettings.EnableErrorBars == EnableErrorBars.Both || this.ErrorBarSettings.EnableErrorBars == EnableErrorBars.Positive)
                {
                    AddErrorBarAction(errorBarsGeometry, position, errorBarSize, true);
                }

                if (this.ErrorBarSettings.EnableErrorBars == EnableErrorBars.Both || this.ErrorBarSettings.EnableErrorBars == EnableErrorBars.Negative)
                {
                    AddErrorBarAction(errorBarsGeometry, position, errorBarSize, false);
                }
            }

            view.UpdateErrorBars(errorBarsGeometry);

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
            AnchoredView.ResetTrendlineBrush();

            if (TrendLineBrush != null)
            {
                AnchoredView.BindTrendlineBrushToActualTrendlineBrush();
            }
            else
            {
                AnchoredView.BindTrendlineBrushToActualBrush();
            }
            #endregion
        }

        /// <summary>
        /// Returns the item that is found at the given point.
        /// </summary>
        /// <param name="sender">The origin of the mouse interaction.</param>
        /// <param name="point">The point to search for the item near.</param>
        /// <returns>The found item.</returns>
        protected internal override object Item(object sender, Point point)
        {
            if (sender == this.AnchoredView.TrendLineManager.TrendPolyline)
            {
                // this is not a data item.
                return null;
            }
            return base.Item(sender, point);
        }

        IList<double> IHasSingleValueCategory.ValueColumn
        {
            get { return ValueColumn; }
        }

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
            get { return this.AnchoredView.BucketCalculator; }
        }

        int IIsCategoryBased.CurrentMode2Index
        {
            get { return GetMode2Index(); }
        }

        IDetectsCollisions IIsCategoryBased.ProvideCollisionDetector()
        {
            return new CollisionAvoider();
        }

        IPreparesCategoryTrendline IHasCategoryTrendline.TrendlinePreparer
        {
            get { return this.AnchoredView.TrendLineManager; }
        }
      
        /// <summary>
        /// A cached version of ValueMemberPath.
        /// </summary>
        protected string CachedValueMemberPath { get; set; }
        
        internal abstract void SetXAxis(Axis xAxis);
        internal abstract void SetYAxis(Axis yAxis);

        /// <summary>
        /// Exports visual information about the series for use by external tools and functionality.
        /// </summary>
        /// <param name="svd">The data container.</param>
        protected override void ExportVisualDataOverride(SeriesVisualData svd)
        {
            base.ExportVisualDataOverride(svd);

            var trendShape = new PolyLineVisualData(
                "trendLine", 
                AnchoredView.TrendLineManager.TrendPolyline);

            trendShape.Tags.Add("Trend");
            svd.Shapes.Add(trendShape);
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