using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using Infragistics.Controls.Charts.Util;
using System.Windows.Media;



using System.Linq;

using System.Collections.ObjectModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class for all financial indicator and overlay series.
    /// </summary>
    public abstract class FinancialSeries : Series, IHasCategoryAxis
    {
        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            FinancialView = (FinancialSeriesView)view;
        }
        internal FinancialSeriesView FinancialView { get; set; }

        #region Constructor and Initialisation
        /// <summary>
        /// Initializes a default FinancialSeries object.
        /// </summary>
        internal protected FinancialSeries()
        {
            DefaultStyleKey = typeof(FinancialSeries);
        }
        #endregion

        #region NegativeBrush Dependency Property
        /// <summary>
        /// Gets or sets the brush to use for negative portions of the series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush NegativeBrush
        {
            get
            {
                return (Brush)GetValue(NegativeBrushProperty);
            }
            set
            {
                SetValue(NegativeBrushProperty, value);
            }
        }

        internal const string NegativeBrushPropertyName = "NegativeBrush";


        /// <summary>
        /// Identifies the Fill dependency property.
        /// </summary>
        public static readonly DependencyProperty NegativeBrushProperty = DependencyProperty.Register(NegativeBrushPropertyName, typeof(Brush), typeof(FinancialSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as FinancialSeries).RaisePropertyChanged(NegativeBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region XAxis Dependency Property
        /// <summary>
        /// Gets or sets the effective x-axis for the current FinancialSeries object.
        /// <para>
        /// This is a dependency property.
        /// </para>
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
        /// Identifies the ActualXAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(XAxisPropertyName, typeof(CategoryAxisBase), typeof(FinancialSeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as FinancialSeries).RaisePropertyChanged(XAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region YAxis Depedency Property
        /// <summary>
        /// Gets or sets the effective y-axis for the current FinancialSeries object.
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
        /// Identifies the ActualYAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(YAxisPropertyName, typeof(NumericYAxis), typeof(FinancialSeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as FinancialSeries).RaisePropertyChanged(YAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region OpenMemberPath Dependency Property and OpenColumn Property
        /// <summary>
        /// Gets or sets the open mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string OpenMemberPath
        {
            get
            {
                return (string)GetValue(OpenMemberPathProperty);
            }
            set
            {
                SetValue(OpenMemberPathProperty, value);
            }
        }

        internal const string OpenMemberPathPropertyName = "OpenMemberPath";

        /// <summary>
        /// Identifies the OpenMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty OpenMemberPathProperty = DependencyProperty.Register(OpenMemberPathPropertyName, typeof(string), typeof(FinancialSeries),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as FinancialSeries).RaisePropertyChanged(OpenMemberPathPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets the Open data values gathered from the data source.
        /// </summary>
        protected internal IFastItemColumn<double> OpenColumn
        {
            get { return openColumn; }
            private set
            {
                if (openColumn != value)
                {
                    IFastItemColumn<double> oldOpenColumn = openColumn;

                    openColumn = value;
                    RaisePropertyChanged(OpenColumnPropertyName, oldOpenColumn, openColumn);
                }
            }
        }
        private IFastItemColumn<double> openColumn;
        internal const string OpenColumnPropertyName = "OpenColumn";
        #endregion

        #region HighMemberPath Dependency Property and HighColumn Property
        /// <summary>
        /// Gets or sets the high mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string HighMemberPath
        {
            get
            {
                return (string)GetValue(HighMemberPathProperty);
            }
            set
            {
                SetValue(HighMemberPathProperty, value);
            }
        }

        internal const string HighMemberPathPropertyName = "HighMemberPath";

        /// <summary>
        /// Identifies the HighMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty HighMemberPathProperty = DependencyProperty.Register(HighMemberPathPropertyName, typeof(string), typeof(FinancialSeries), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as FinancialSeries).RaisePropertyChanged(HighMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets the high data values gathered from the data source.
        /// </summary>
        protected internal IFastItemColumn<double> HighColumn
        {
            get { return highColumn; }
            private set
            {
                if (highColumn != value)
                {
                    IFastItemColumn<double> oldHighColumn = highColumn;

                    highColumn = value;
                    RaisePropertyChanged(HighColumnPropertyName, oldHighColumn, highColumn);
                }
            }
        }
        private IFastItemColumn<double> highColumn;
        internal const string HighColumnPropertyName = "HighColumn";
        #endregion

        #region LowMemberPath Dependency Property and LowColumn Property
        /// <summary>
        /// Gets or sets the low mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string LowMemberPath
        {
            get
            {
                return (string)GetValue(LowMemberPathProperty);
            }
            set
            {
                SetValue(LowMemberPathProperty, value);
            }
        }

        internal const string LowMemberPathPropertyName = "LowMemberPath";

        /// <summary>
        /// Identifies the LowMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty LowMemberPathProperty = DependencyProperty.Register(LowMemberPathPropertyName, typeof(string), typeof(FinancialSeries), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as FinancialSeries).RaisePropertyChanged(LowMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets the low data values gathered from the data source.
        /// </summary>
        protected internal IFastItemColumn<double> LowColumn
        {
            get { return lowColumn; }
            private set
            {
                if (lowColumn != value)
                {
                    IFastItemColumn<double> oldLowColumn = lowColumn;

                    lowColumn = value;
                    RaisePropertyChanged(LowColumnPropertyName, oldLowColumn, lowColumn);
                }
            }
        }
        private IFastItemColumn<double> lowColumn;
        internal const string LowColumnPropertyName = "LowColumn";
        #endregion

        #region CloseMemberPath Dependency Property and CloseColumn Property
        /// <summary>
        /// Gets or sets the close mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string CloseMemberPath
        {
            get
            {
                return (string)GetValue(CloseMemberPathProperty);
            }
            set
            {
                SetValue(CloseMemberPathProperty, value);
            }
        }

        internal const string CloseMemberPathPropertyName = "CloseMemberPath";

        /// <summary>
        /// Identifies the CloseMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty CloseMemberPathProperty = DependencyProperty.Register(CloseMemberPathPropertyName, typeof(string), typeof(FinancialSeries), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as FinancialSeries).RaisePropertyChanged(CloseMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets the Close data values gathered from the data source.
        /// </summary>
        protected internal IFastItemColumn<double> CloseColumn
        {
            get { return closeColumn; }
            private set
            {
                if (closeColumn != value)
                {
                    IFastItemColumn<double> oldCloseColumn = closeColumn;

                    closeColumn = value;
                    RaisePropertyChanged(CloseColumnPropertyName, oldCloseColumn, closeColumn);
                }
            }
        }
        private IFastItemColumn<double> closeColumn;
        internal const string CloseColumnPropertyName = "CloseColumn";
        #endregion

        #region VolumeMemberPath Dependency Property and VolumnColumn Property
        /// <summary>
        /// Gets or sets the volume mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string VolumeMemberPath
        {
            get
            {
                return (string)GetValue(VolumeMemberPathProperty);
            }
            set
            {
                SetValue(VolumeMemberPathProperty, value);
            }
        }

        internal const string VolumeMemberPathPropertyName = "VolumeMemberPath";

        /// <summary>
        /// Identifies the VolumeMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty VolumeMemberPathProperty = DependencyProperty.Register(VolumeMemberPathPropertyName, typeof(string), typeof(FinancialSeries), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as FinancialSeries).RaisePropertyChanged(VolumeMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets the Volume data values gathered from the data source.
        /// </summary>
        protected internal IFastItemColumn<double> VolumeColumn
        {
            get { return volumeColumn; }
            private set
            {
                if (volumeColumn != value)
                {
                    IFastItemColumn<double> oldVolumeColumn = volumeColumn;

                    volumeColumn = value;
                    RaisePropertyChanged(VolumeColumnPropertyName, oldVolumeColumn, volumeColumn);
                }
            }
        }
        private IFastItemColumn<double> volumeColumn;
        internal const string VolumeColumnPropertyName = "VolumeColumn";
        #endregion

        /// <summary>
        /// Invalidates the associated axes.
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

        /// <summary>
        /// Overridden in derived classes when they want to respond to the chart's window being updated.
        /// </summary>
        /// <param name="oldWindowRect">The old window rectangle.</param>
        /// <param name="newWindowRect">The new window rectangle.</param>
        protected override void WindowRectChangedOverride(Rect oldWindowRect, Rect newWindowRect)
        {
            this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
            RenderSeries(false);
        }

        /// <summary>
        /// Overridden in derived classes when they want to respond to the viewport being updated.
        /// </summary>
        /// <param name="oldViewportRect">The old viewport rectangle.</param>
        /// <param name="newViewportRect">The new viewport rectangle.</param>
        protected override void ViewportRectChangedOverride(Rect oldViewportRect, Rect newViewportRect)
        {
            this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
            RenderSeries(false);
        }

        /// <summary>
        /// Maps from fast item columns to member paths.
        /// </summary>
        protected Dictionary<IFastItemColumn<double>, string> columnToMapping =
            new Dictionary<IFastItemColumn<double>, string>();
        /// <summary>
        /// Maps from member paths to column names.
        /// </summary>
        protected Dictionary<string, string> mappingToColumnName =
            new Dictionary<string, string>();

        private IFastItemColumn<double> RegisterColumn(FastItemsSource itemsSource, string mapping, string propertyName)
        {
            IFastItemColumn<double> column = RegisterDoubleColumn(mapping);
            columnToMapping.Add(column, mapping);
            mappingToColumnName.Add(mapping, propertyName);
            return column;
        }

        private void DeRegisterColumn(FastItemsSource itemsSource, IFastItemColumn<double> column)
        {
            if (column == null)
            {
                return;
            }
            itemsSource.DeregisterColumn(column);
            string mapping = columnToMapping[column];
            mappingToColumnName.Remove(mapping);
            columnToMapping.Remove(column);
        }

        private bool _ignoreColumnChanges = false;

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
                case Series.SyncLinkPropertyName:
                    if (SyncLink != null && SeriesViewer != null)
                    {
                        this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }

                    break;

                case SeriesViewerPropertyName:
                    //dont stay registered with axes while not in chart
                    if (oldValue != null && newValue == null)
                    {
                        if (XAxis != null)
                        {
                            XAxis.DeregisterSeries(this);
                        }
                        if (YAxis != null)
                        {
                            YAxis.DeregisterSeries(this);
                        }
                    }
                    if (oldValue == null && newValue != null)
                    {
                        if (XAxis != null)
                        {
                            XAxis.RegisterSeries(this);
                        }
                        if (YAxis != null)
                        {
                            YAxis.RegisterSeries(this);
                        }
                    }

                    this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);
                    break;

                case FastItemsSourcePropertyName:
                    _ignoreColumnChanges = true;
                    if (oldValue as FastItemsSource != null)
                    {
                        DeRegisterColumn(oldValue as FastItemsSource, OpenColumn);
                        DeRegisterColumn(oldValue as FastItemsSource, HighColumn);
                        DeRegisterColumn(oldValue as FastItemsSource, LowColumn);
                        DeRegisterColumn(oldValue as FastItemsSource, CloseColumn);
                        DeRegisterColumn(oldValue as FastItemsSource, VolumeColumn);
                        OpenColumn = null;
                        HighColumn = null;
                        LowColumn = null;
                        CloseColumn = null;
                        VolumeColumn = null;
                    }

                    if (newValue as FastItemsSource != null)
                    {
                        if (OpenMemberPath != null)
                        {
                            OpenColumn = RegisterColumn(newValue as FastItemsSource, OpenMemberPath, OpenColumnPropertyName);
                        }
                        if (HighMemberPath != null)
                        {
                            HighColumn = RegisterColumn(newValue as FastItemsSource, HighMemberPath, HighColumnPropertyName);
                        }
                        if (LowMemberPath != null)
                        {
                            LowColumn = RegisterColumn(newValue as FastItemsSource, LowMemberPath, LowColumnPropertyName);
                        }
                        if (CloseMemberPath != null)
                        {
                            CloseColumn = RegisterColumn(newValue as FastItemsSource, CloseMemberPath, CloseColumnPropertyName);
                        }
                        if (VolumeMemberPath != null)
                        {
                            VolumeColumn = RegisterColumn(newValue as FastItemsSource, VolumeMemberPath, VolumeColumnPropertyName);
                        }
                    }
                    _ignoreColumnChanges = false;

                    if (YAxis != null && !YAxis.UpdateRange())
                    {
                        this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;

                #region Open Mapping and Column
                case OpenMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        DeRegisterColumn(FastItemsSource, OpenColumn);
                        OpenColumn = RegisterColumn(FastItemsSource, OpenMemberPath, OpenColumnPropertyName);
                    }

                    break;

                case OpenColumnPropertyName:
                    if (YAxis != null && !YAxis.UpdateRange() && !_ignoreColumnChanges)
                    {
                        this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;
                #endregion

                #region High Mapping and Column
                case HighMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        DeRegisterColumn(FastItemsSource, HighColumn);
                        HighColumn = RegisterColumn(FastItemsSource, HighMemberPath, HighColumnPropertyName);
                    }

                    break;

                case HighColumnPropertyName:
                    if (YAxis != null && !YAxis.UpdateRange() && !_ignoreColumnChanges)
                    {
                        this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;
                #endregion

                #region Low Mapping and Column
                case LowMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        DeRegisterColumn(FastItemsSource, LowColumn);
                        LowColumn = RegisterColumn(FastItemsSource, LowMemberPath, LowColumnPropertyName);
                    }

                    break;

                case LowColumnPropertyName:
                    if (YAxis != null && !YAxis.UpdateRange() && !_ignoreColumnChanges)
                    {
                        this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;
                #endregion

                #region Close Mapping and Column
                case CloseMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        DeRegisterColumn(FastItemsSource, CloseColumn);
                        CloseColumn = RegisterColumn(FastItemsSource, CloseMemberPath, CloseColumnPropertyName);
                    }

                    break;

                case CloseColumnPropertyName:
                    if (YAxis != null && !YAxis.UpdateRange() && !_ignoreColumnChanges)
                    {
                        this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;
                #endregion

                #region Volume Mapping and Column
                case VolumeMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        DeRegisterColumn(FastItemsSource, VolumeColumn);
                        VolumeColumn = RegisterColumn(FastItemsSource, VolumeMemberPath, VolumeColumnPropertyName);
                    }

                    break;

                case VolumeColumnPropertyName:
                    if (YAxis != null && !YAxis.UpdateRange() && !_ignoreColumnChanges)
                    {
                        this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                        RenderSeries(false);
                    }
                    break;
                #endregion

                case XAxisPropertyName:
                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }
                    this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                    RenderSeries(false);
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
                    this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);

                    if (YAxis != null && !YAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }
                    this.NotifyThumbnailAppearanceChanged();
                    break;

                case TransitionProgressPropertyName:
                    TransitionFrame.Interpolate((float)TransitionProgress, PreviousFrame, CurrentFrame);

                    if (ClearAndAbortIfInvalid(View))
                    {
                        return;
                    }

                    if (TransitionProgress == 1.0)
                    {
                        RenderFrame(CurrentFrame, this.FinancialView);
                    }
                    else
                    {
                        RenderFrame(TransitionFrame, this.FinancialView);
                    }

                    break;

                case NegativeBrushPropertyName:
                    RenderSeries(false);
                    break;
            }
        }

        #region Data and Buckets

        /// <summary>
        /// Gets the index of the item that resides at the provided world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates of the requested item.</param>
        /// <returns>The requested item's index.</returns>
        protected override int GetItemIndex(Point world)
        {
            //Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            //Rect viewportRect = View.Viewport;

            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;

            int rowIndex = -1;

            if (!windowRect.IsEmpty && !viewportRect.IsEmpty)
            {
                double windowX = (world.X - windowRect.Left) / windowRect.Width;
                int bucketNumber = this.FinancialView.BucketCalculator.FirstBucket + (int)Math.Round((windowX * (this.FinancialView.BucketCalculator.LastBucket - this.FinancialView.BucketCalculator.FirstBucket)));

                rowIndex = bucketNumber * this.FinancialView.BucketCalculator.BucketSize;
            }

            return rowIndex;
        }

        /// <summary>
        /// Gets the item index based on world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to get the item for.</param>
        /// <returns>The index of the item.</returns>
        protected virtual int GetItemIndexSorted(Point world)
        {
            //Rect windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            //Rect viewportRect = View.Viewport;

            Rect windowRect = this.View.WindowRect;
            Rect viewportRect = this.View.Viewport;

            if (windowRect.IsEmpty || viewportRect.IsEmpty)
            {
                return -1;
            }

            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, XAxis.IsInverted);

            ISortingAxis sorting = XAxis as ISortingAxis;

            double left = XAxis.GetUnscaledValue(viewportRect.Left, xParams);
            double right = XAxis.GetUnscaledValue(viewportRect.Right, xParams);
            double windowX = (world.X - windowRect.Left) / windowRect.Width;
            double axisValue = left + ((right - left) * windowX);


            if ((long)axisValue <= DateTime.MinValue.Ticks || (long)axisValue >= DateTime.MaxValue.Ticks)
            {
                return -1;
            }

            int itemIndex = sorting.GetIndexClosestToUnscaledValue(axisValue);
            return itemIndex;
        }

        /// <summary>
        /// Gets the item that is the best match for the specified world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The item that is the best match.</returns>
        protected override object GetItem(Point world)
        {
            int index = 0;

            if (this.XAxis is ISortingAxis)
            {
                index = GetItemIndexSorted(world);
                if (index == -1)
                {
                    return null;
                }
            }
            else
            {
                index = GetItemIndex(world);
            }

            return index >= 0 && FastItemsSource != null && index < FastItemsSource.Count ? FastItemsSource[index] : null;
        }

        
        #endregion

        #region Transitions and Rendering
        internal CategoryFrame PreviousFrame;
        internal CategoryFrame TransitionFrame;
        internal CategoryFrame CurrentFrame;

        internal abstract void PrepareFrame(CategoryFrame frame, FinancialSeriesView view);
        internal abstract void RenderFrame(CategoryFrame frame, FinancialSeriesView view);

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
            var financialView = (FinancialSeriesView)view;

            if (FastItemsSource == null || FastItemsSource.Count == 0 ||
                !view.HasSurface() 
                || windowRect.IsEmpty
                || viewportRect.IsEmpty
                || XAxis == null
                || YAxis == null
                || financialView.BucketCalculator.BucketSize < 1
                || XAxis.SeriesViewer == null
                || YAxis.SeriesViewer == null
                || YAxis.ActualMinimumValue == YAxis.ActualMaximumValue)
            {
                financialView.BucketCalculator.BucketSize = 0;
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Gets the view info for the series.
        /// </summary>
        /// <param name="viewportRect">The viewport of the series.</param>
        /// <param name="windowRect">The window of the series.</param>
        protected internal override void GetViewInfo(out Rect viewportRect, out Rect windowRect)
        {
            //windowRect = SeriesViewer != null ? SeriesViewer.ActualWindowRect : Rect.Empty;
            //viewportRect = View.Viewport;

            windowRect = this.View.WindowRect;
            viewportRect = this.View.Viewport;
        }

        /// <summary>
        /// Renders the series.
        /// </summary>
        /// <param name="animate">True if the change should be animated.</param>
        protected internal override void RenderSeriesOverride(bool animate)
        {
            if (ClearAndAbortIfInvalid(View))
            {
                return;
            }

            if (ShouldAnimate(animate))
            {
                CategoryFrame previousFrame = PreviousFrame;

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

                this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                PrepareFrame(CurrentFrame, this.FinancialView);
                StartAnimation();
            }
            else
            {
                this.FinancialView.BucketCalculator.CalculateBuckets(Resolution);
                PrepareFrame(CurrentFrame, this.FinancialView);

                RenderFrame(CurrentFrame, this.FinancialView);
            }
        }
        #endregion

        #region commonly used enumerables

        /// <summary>
        /// Handle this event in order to perform a custom typical price calculation.
        /// </summary>
        public event FinancialEventHandler Typical;

        /// <summary>
        /// Handle this event in order to specify which columns the Typical price calculation is based on.
        /// </summary>
        public event FinancialEventHandler TypicalBasedOn;

        /// <summary>
        /// Validates the the required columns have been mapped for the required calculation.
        /// </summary>
        /// <param name="basedOn">The columns the calculation is based on.</param>
        /// <returns>Whether the required columns are mapped.</returns>
        protected bool ValidateBasedOn(IList<string> basedOn)
        {
            foreach (string col in basedOn)
            {
                switch (col)
                {
                    case "HighColumn":
                        if (HighColumn == null)
                        {
                            return false;
                        }
                        break;
                    case "LowColumn":
                        if (LowColumn == null)
                        {
                            return false;
                        }
                        break;
                    case "OpenColumn":
                        if (OpenColumn == null)
                        {
                            return false;
                        }
                        break;
                    case "CloseColumn":
                        if (CloseColumn == null)
                        {
                            return false;
                        }
                        break;
                    case "VolumeColumn":
                        if (VolumeColumn == null)
                        {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        internal bool XAxisSortRequired
        {
            get { return XAxis != null && XAxis is ISortingAxis; }
        }

        /// <summary>
        /// Typical column enumerable. The precise meaning of "typical" should be configurable
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<double> TypicalColumn
        {
            get
            {
                if (Typical != null)
                {
                    FinancialCalculationDataSource dataSource = ProvideDataSource(0, FastItemsSource.Count);
                    Typical(this, new FinancialEventArgs(0, FastItemsSource.Count,
                        dataSource, ProvideSupportingCalculations(dataSource)));

                    foreach (double value in dataSource.TypicalColumn)
                    {
                        yield return value;
                    }
                }
                else
                {
                    if (XAxisSortRequired && (XAxis as ISortingAxis).SortedIndices != null)
                    {
                        int count = FastItemsSource.Count;
                        IList<int> sorted = (XAxis as ISortingAxis).SortedIndices;

                        for (int i = 0; i < count; ++i)
                        {
                            yield return (HighColumn[sorted[i]] + LowColumn[sorted[i]] + CloseColumn[sorted[i]]) / 3.0;
                        }
                    }
                    else
                    {
                        int count = FastItemsSource.Count;

                        for (int i = 0; i < count; ++i)
                        {
                            yield return (HighColumn[i] + LowColumn[i] + CloseColumn[i]) / 3.0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// True range enumerable
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<double> TR
        {
            get
            {
                if (XAxisSortRequired && (XAxis as ISortingAxis).SortedIndices != null)
                {
                    int count = HighColumn.Count;
                    IList<int> sorted = (XAxis as ISortingAxis).SortedIndices;

                    if (count > 0)
                    {
                        yield return MakeSafe(HighColumn[sorted[0]] - LowColumn[sorted[0]]);
                    }

                    for (int i = 1; i < count; ++i)
                    {
                        yield return
                            Math.Max(MakeSafe(HighColumn[sorted[i]] - LowColumn[sorted[i]]),
                                     Math.Max(MakeSafe(Math.Abs(HighColumn[sorted[i]] - CloseColumn[sorted[i - 1]])),
                                              MakeSafe(Math.Abs(LowColumn[sorted[i]] - CloseColumn[sorted[i - 1]]))));
                    }
                }
                else
                {
                    int count = HighColumn.Count;

                    if (count > 0)
                    {
                        yield return MakeSafe(HighColumn[0] - LowColumn[0]);
                    }

                    for (int i = 1; i < count; ++i)
                    {
                        yield return
                            Math.Max(MakeSafe(HighColumn[i] - LowColumn[i]),
                                     Math.Max(MakeSafe(Math.Abs(HighColumn[i] - CloseColumn[i - 1])),
                                              MakeSafe(Math.Abs(LowColumn[i] - CloseColumn[i - 1]))));
                    }
                }
            }
        }

        /// <summary>
        /// True low enumerable
        /// </summary>
        protected IEnumerable<double> TL
        {
            get
            {
                if (XAxisSortRequired && (XAxis as ISortingAxis).SortedIndices != null)
                {
                    int count = LowColumn.Count;
                    IList<int> sorted = (XAxis as ISortingAxis).SortedIndices;

                    if (count > 0)
                    {
                        yield return MakeSafe(LowColumn[sorted[0]]);
                    }

                    for (int i = 1; i < count; i++)
                    {
                        yield return Math.Min(MakeSafe(LowColumn[sorted[i]]),
                            MakeSafe(CloseColumn[sorted[i - 1]]));
                    }
                }
                else
                {
                    int count = LowColumn.Count;

                    if (count > 0)
                    {
                        yield return MakeSafe(LowColumn[0]);
                    }

                    for (int i = 1; i < count; i++)
                    {
                        yield return Math.Min(MakeSafe(LowColumn[i]),
                            MakeSafe(CloseColumn[i - 1]));
                    }
                }
            }
        }

        internal IList<double> MakeReadOnlyAndEnsureSorted(IList<double> column)
        {
            if (column == null)
            {
                return null;
            }

            if (XAxisSortRequired && (XAxis as ISortingAxis).SortedIndices != null)
            {
                return new SafeSortedReadOnlyDoubleCollection(
                    column,
                    (XAxis as ISortingAxis).SortedIndices);
            }

            return new SafeReadOnlyDoubleCollection(column);
        }

        /// <summary>
        /// Gets the columns the current Typical Price calculation are based on.
        /// </summary>
        /// <returns>The list of column names the calculation is based on.</returns>
        protected IList<string> GetTypicalBasedOn()
        {
            IList<string> ret = new List<String>();
            ret.Add("HighColumn");
            ret.Add("LowColumn");
            ret.Add("CloseColumn");

            if (TypicalBasedOn != null)
            {
                FinancialCalculationDataSource dataSource = ProvideDataSource(0, FastItemsSource.Count);
                FinancialEventArgs args = new FinancialEventArgs(0, FastItemsSource.Count,
                    dataSource, ProvideSupportingCalculations(dataSource));

                TypicalBasedOn(this, args);
                return args.BasedOn;
            }

            return ret;
        }

        /// <summary>
        /// Provides the data requires by the data contract between the financial series
        /// and the calculation strategies.
        /// </summary>
        /// <returns>The data which the calculation strategies need.</returns>
        protected virtual FinancialCalculationDataSource ProvideDataSource(int position, int count)
        {
            IList<double> readOnlyOpenColumn = MakeReadOnlyAndEnsureSorted(this.OpenColumn);
            IList<double> readOnlyCloseColumn = MakeReadOnlyAndEnsureSorted(this.CloseColumn);
            IList<double> readOnlyHighColumn = MakeReadOnlyAndEnsureSorted(this.HighColumn);
            IList<double> readOnlyLowColumn = MakeReadOnlyAndEnsureSorted(this.LowColumn);
            IList<double> readOnlyVolumeColumn = MakeReadOnlyAndEnsureSorted(this.VolumeColumn);

            FinancialCalculationDataSource dataSource = new FinancialCalculationDataSource()
            {
                TypicalColumn = new CalculatedColumn(new SafeEnumerable(this.TypicalColumn),
                    GetTypicalBasedOn()),
                TrueRange = new CalculatedColumn(new SafeEnumerable(this.TR),
                    new List<string>() { "HighColumn", "LowColumn", "CloseColumn" }),
                TrueLow = new CalculatedColumn(new SafeEnumerable(this.TL),
                    new List<string>() { "LowColumn", "CloseColumn" }),
                //An indicator should not be allowed to change the underlying
                //financial data, so the data is exposed as read only.
                //note these are only wrappers, this does not represent a collection
                //copy.
                OpenColumn = readOnlyOpenColumn,
                CloseColumn = readOnlyCloseColumn,
                HighColumn = readOnlyHighColumn,
                LowColumn = readOnlyLowColumn,
                VolumeColumn = readOnlyVolumeColumn,
                CalculateFrom = position,
                CalculateCount = count,

                MinimumValue = double.NaN,
                MaximumValue = double.NaN,
                Count = this.FastItemsSource != null ? this.FastItemsSource.Count : 0
            };

            return dataSource;
        }

        internal double MakeSafe(double value)
        {
            if (double.IsInfinity(value) ||
                double.IsNaN(value))
            {
                return 0.0;
            }

            return value;
        }

        /// <summary>
        /// Provides the supporting calculation strategies so that the indicator 
        /// calculation strategy can perform its calculations.
        /// </summary>
        /// <returns>The supporting calculation strategies.</returns>
        protected virtual FinancialCalculationSupportingCalculations ProvideSupportingCalculations(FinancialCalculationDataSource dataSource)
        {
            return new FinancialCalculationSupportingCalculations()
            {
                EMA = new ColumnSupportingCalculation(Series.EMA, new List<string>()),
                SMA = new ColumnSupportingCalculation(Series.SMA, new List<string>()),
                STDEV = new ColumnSupportingCalculation(Series.STDEV, new List<string>()),
                MovingSum = new ColumnSupportingCalculation(Series.MovingSum, new List<string>()),
                ShortVolumeOscillatorAverage =
                new DataSourceSupportingCalculation((ds) => Series.EMA(ds.VolumeColumn, ds.ShortPeriod),
                    new List<string>() { FinancialSeries.VolumeColumnPropertyName }),
                LongVolumeOscillatorAverage =
                new DataSourceSupportingCalculation((ds) => Series.EMA(ds.VolumeColumn, ds.LongPeriod),
                    new List<string>() { FinancialSeries.VolumeColumnPropertyName }),
                ShortPriceOscillatorAverage =
                new DataSourceSupportingCalculation((ds) => Series.EMA(ds.TypicalColumn, ds.ShortPeriod),
                    dataSource.TypicalColumn.BasedOn),
                LongPriceOscillatorAverage =
                new DataSourceSupportingCalculation((ds) => Series.EMA(ds.TypicalColumn, ds.LongPeriod),
                    dataSource.TypicalColumn.BasedOn),
                ToEnumerableRange = Series.ToEnumerableRange,
                ToEnumerable = Series.ToEnumerable,
                MakeSafe = MakeSafe
            };
        }

        #endregion

        CategoryAxisBase IHasCategoryAxis.CategoryAxis
        {
            get { return XAxis; }
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

            FinancialSeriesView thumbnailView = this.ThumbnailView as FinancialSeriesView;
            this.View.PrepSurface(surface);
            thumbnailView.BucketCalculator.CalculateBuckets(this.Resolution);
            if (ClearAndAbortIfInvalid(ThumbnailView))
            {
                return;
            }
            
            //FinancialSeriesView originalView = this.FinancialView;
            // CategoryFrame originalFrame = this.CurrentFrame;

            CategoryFrame frame = new CategoryFrame(3);
            
            // this.CurrentFrame = frame;

            this.PrepareFrame(frame, thumbnailView);
            this.RenderFrame(frame, thumbnailView);

            ThumbnailDirty = false;
            // return the old view
            //OnViewCreated(originalView);
            //this.View = originalView;
            // this.CurrentFrame = originalFrame;
        }
    }

    internal class FinancialBucketCalculator
        : IBucketizer
    {
        protected FinancialSeriesView View { get; private set; }

        internal FinancialBucketCalculator(FinancialSeriesView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            this.View = view;

            FirstBucket = -1;
            LastBucket = LastBucket;
            BucketSize = 0;   
        }

        public virtual float[] GetBucket(int index)
        {
            return null; 
        }

        public virtual float GetErrorBucket(int index, IFastItemColumn<double> column)
        {
            return float.NaN;
        }

        /// <summary>
        /// Gets or sets the first visible bucket of the series.
        /// </summary>
        protected internal int FirstBucket { get; set; }
        /// <summary>
        /// Gets or sets the last visible bucket of the series.
        /// </summary>
        protected internal int LastBucket { get; set; }
        /// <summary>
        /// Gets or sets the bucket size of the series.
        /// </summary>
        protected internal int BucketSize { get; set; }

        public void GetBucketInfo(out int firstBucket, out int lastBucket, out int bucketSize, out double resolution)
        {
            firstBucket = this.FirstBucket;
            lastBucket = this.LastBucket;
            bucketSize = this.BucketSize;
            resolution = this.View.FinancialModel.Resolution;
        }

        protected internal virtual void CalculateBuckets(double resolution)
        {
            Rect windowRect = View.WindowRect;
            Rect viewportRect = View.Viewport;

            if (windowRect.IsEmpty || viewportRect.IsEmpty || View.FinancialModel.XAxis == null)
            {
                BucketSize = 0;
                return;
            }

            var xIsInverted = (View.FinancialModel.XAxis != null) ? View.FinancialModel.XAxis.IsInverted : false;

            ScalerParams xParams = new ScalerParams(windowRect, viewportRect, xIsInverted);

            ISortingAxis sortingXAxis = View.FinancialModel.XAxis as ISortingAxis;
            if (sortingXAxis == null || sortingXAxis.SortedIndices == null)
            {
                // index-based bucketing
                double x0 = Math.Floor(View.FinancialModel.XAxis.GetUnscaledValue(viewportRect.Left, xParams));
                double x1 = Math.Ceiling(View.FinancialModel.XAxis.GetUnscaledValue(viewportRect.Right, xParams));

                if (View.FinancialModel.XAxis.IsInverted)
                {
                    x1 = Math.Ceiling(View.FinancialModel.XAxis.GetUnscaledValue(viewportRect.Left, xParams));
                    x0 = Math.Floor(View.FinancialModel.XAxis.GetUnscaledValue(viewportRect.Right, xParams));
                }

                double c = Math.Floor((x1 - x0 + 1.0) * resolution / viewportRect.Width); // the number of rows per bucket

                BucketSize = (int)Math.Max(1.0, c);
                FirstBucket = (int)Math.Floor(x0 / BucketSize); // first visibile bucket
                LastBucket = (int)Math.Ceiling(x1 / BucketSize); // first invisible bucket
            }
            else
            {
                // SortedAxis based bucketing (for CategoryDateTimeXAxis)
                this.FirstBucket = sortingXAxis.GetFirstVisibleIndex(windowRect, viewportRect);
                this.LastBucket = sortingXAxis.GetLastVisibleIndex(windowRect, viewportRect);
                this.BucketSize = 1;
            }
        }



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

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