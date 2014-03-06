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
using Infragistics.Controls.Charts;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;







namespace Infragistics.Controls
{
    /// <summary>
    /// A control for viewing series.
    /// </summary>


    [StyleTypedProperty(Property = OverviewPlusDetailPaneStylePropertyName, StyleTargetType = typeof(XamOverviewPlusDetailPane))]

    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenter))]




    [Widget("Chart")]
    public abstract class SeriesViewer : Control, INotifyPropertyChanged



    {
        internal virtual SeriesViewerView View { get; set; }


        static SeriesViewer()
        {
            SeriesViewer.StandardRect = new Rect(0.0, 0.0, 1.0, 1.0);
        }






        /// <summary>
        /// Instantiates an instance of a SeriesViewer.
        /// </summary>
        protected SeriesViewer()
        {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)




            this.View = this.CreateView();
            OnViewCreated(View);
            this.View.OnInit();


            this.OverviewPlusDetailPane.ThumbnailSizeChanged += (o, e) =>
                {
                    ((SeriesViewerSurfaceViewer)OverviewPlusDetailPane.SurfaceViewer).IsDirty = true;
                    foreach (var series in Series)
                    {
                        series.ThumbnailDirty = true;
                    }
                };


            this.ActualSyncLink = new SyncLink();
            this.UpdateSyncLink(null, ActualSyncLink);

            this.ChartContentManager = new ChartContentManager(this);

            this.PropertyUpdated += (o, e) => { PropertyUpdatedOverride(o, e.PropertyName, e.OldValue, e.NewValue); };

            this.Series.CollectionChanged += new NotifyCollectionChangedEventHandler(Series_CollectionChanged);
            this.Series.CollectionResetting += new EventHandler<EventArgs>(Series_CollectionResetting);


            CentralArea = new ChartAreaPanel(this);


            AddLogicalChild(CentralArea);

            View.CreateLayout();

            this.InvalidateActualWindowRect();
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.ContentPresenter = this.GetTemplateChild(ContentPresenterName) as ContentPresenter;
            this.View.OnTemplateProvided();
            Debug.Assert(this.ContentPresenter != null, "SeriesViewer: Template applied without ContentPresenter");
        }


        #region Series

        /// <summary>
        /// Gets the current SeriesViewer object's child Series. 
        /// </summary>
        public SeriesCollection Series
        {
            get { return series; }
        }

        private SeriesCollection series = new SeriesCollection();

        private void Series_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Series series in e.OldItems)
                {
                    series.SyncLink = null;
                    series.SeriesViewer = null;
                    RemoveDataSource(series);

                    View.RemoveSeries(series);
                }
            }

            if (e.NewItems != null)
            {
                foreach (Series series in e.NewItems)
                {
                    if (!View.SeriesAttached(series))
                    {




                        View.AttachSeries(series);

                        series.SyncLink = ActualSyncLink;
                        series.SeriesViewer = this;
                        series.UpdateSeriesIndexedPropertiesInternal();
                    }
                }
            }
            this.NotifyThumbnailAppearanceChanged();
        }
        /// <summary>
        /// Removes the DataSource setting for the given argument.
        /// </summary>
        /// <param name="item">The object to remove the datasource from; typically a Series or Axis.</param>
        protected void RemoveDataSource(object item)
        {


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        }

        private void Series_CollectionResetting(object sender, EventArgs e)
        {
            foreach (Series series in Series)
            {
                series.SyncLink = null;
                series.SeriesViewer = null;
                RemoveDataSource(series);

                View.RemoveSeries(series);
            }
        }

        internal void MatchRatio(ref double width, ref double height,
           bool widthChanging, bool heightChanging)
        {
            Rect viewport = ViewportRect;
            double viewportWidth = viewport.Width;
            double viewportHeight = viewport.Height;

            double viewportRatio = viewportWidth / viewportHeight;
            double matchRatio = width / height;

            if (double.IsNaN(viewportRatio))
            {
                return;
            }

            if (double.IsNaN(matchRatio) || widthChanging || heightChanging ||
                Math.Abs(viewportRatio - matchRatio) > 0.0001)
            {
                if (widthChanging && heightChanging)
                {
                    if (width > height)
                    {
                        height = width / viewportRatio;
                    }
                    else
                    {
                        width = height * viewportRatio;
                    }
                }
                else if (widthChanging)
                {
                    height = width / viewportRatio;
                }
                else
                {
                    width = height * viewportRatio;
                }
            }

            if (height > 1.0)
            {
                double scale = 1.0 / height;
                height = 1.0;
                width = width * scale;
            }

            if (width > 1.0)
            {
                double scale = 1.0 / width;
                width = 1.0;
                height = height * scale;
            }
        }

        internal virtual bool EffectiveIsSquare()
        {
            return false;
        }



        #endregion Series

        /// <summary>
        /// Gets the appropriate color for a series based on index.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>The color to use.</returns>
        protected internal virtual Brush GetBrushByIndex(int index)
        {
            return null;
        }

        /// <summary>
        /// Gets the appropriate color for an outline based on index.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>The color to use.</returns>
        protected internal virtual Brush GetOutlineByIndex(int index)
        {
            return null;
        }

        /// <summary>
        /// Gets the appropriate color for a marker based on index.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>The color to use.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual Brush GetMarkerBrushByIndex(int index)
        {
            return null;
        }

        /// <summary>
        /// Gets the appropriate color for a marker outline based on index.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>The color to use.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual Brush GetMarkerOutlineByIndex(int index)
        {
            return null;
        }
        /// <summary>
        /// Helps manage the SeriesViewer content.
        /// </summary>
        internal ChartContentManager ChartContentManager { get; set; }

        #region CrosshairPoint

        internal const string CrosshairPointPropertyName = "CrosshairPoint";

        /// <summary>
        /// Gets or sets the cross hair point (in world coordinates)
        /// </summary>
        /// <remarks>
        /// Either or both of the crosshair point's X and Y may be set to double.NaN, in which
        /// case the relevant crosshair line is hidden.
        /// </remarks>
        public Point CrosshairPoint
        {
            get { return crosshairPoint; }
            set
            {
                if (crosshairPoint != value)
                {
                    Point oldCrosshairPoint = crosshairPoint;

                    crosshairPoint = value;
                    RaisePropertyChanged(CrosshairPointPropertyName, oldCrosshairPoint, crosshairPoint);
                }
            }
        }

        private Point crosshairPoint;

        #endregion CrosshairPoint

        internal bool IsInDragOperation
        {
            get
            {
                return State == InteractionState.DragZoom
                    || State == InteractionState.DragPan;
            }
        }

        /// <summary>
        /// Gets or sets which legend to use for all series in this SeriesViewer, unless otherwise specified by the Series.Legend property.
        /// </summary>
        /// <remarks>
        /// This is generally expressed as an element name binding, as the Legend must exist at some other position in the layout.
        /// This property only indicates which Legend to use and will not alone cause the legend to be visible.
        /// </remarks>
        [SuppressWidgetMember]
        public LegendBase Legend
        {
            get
            {
                return (LegendBase)GetValue(LegendProperty);
            }
            set
            {
                SetValue(LegendProperty, value);
            }
        }

        internal const string LegendPropertyName = "Legend";

        /// <summary>
        /// Identifies the Legend dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendProperty = DependencyProperty.Register(LegendPropertyName, typeof(LegendBase), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                SeriesViewer chart = sender as SeriesViewer;
                chart.RaisePropertyChanged(LegendPropertyName, e.OldValue, e.NewValue);
            }));
        
        
        /// <summary>
        /// Gets or sets whether the series animations should be allowed when a range change has been detected on an axis.
        /// </summary>
        [SuppressWidgetMember]

        internal



            bool AnimateSeriesWhenAxisRangeChanges
        {
            get
            {
                return (bool)GetValue(AnimateSeriesWhenAxisRangeChangesProperty);
            }
            set
            {
                SetValue(AnimateSeriesWhenAxisRangeChangesProperty, value);
            }
        }

        internal const string AnimateSeriesWhenAxisRangeChangesPropertyName = "AnimateSeriesWhenAxisRangeChanges";

        
        internal



            static readonly DependencyProperty AnimateSeriesWhenAxisRangeChangesProperty = DependencyProperty.Register(AnimateSeriesWhenAxisRangeChangesPropertyName, typeof(bool), typeof(SeriesViewer),
                new PropertyMetadata(false, (sender, e) =>
                {
                    SeriesViewer chart = sender as SeriesViewer;
                    chart.RaisePropertyChanged(AnimateSeriesWhenAxisRangeChangesPropertyName, e.OldValue, e.NewValue);
                }));


        #region INotifyPropertyChanged Members

        /// <summary>
        /// Notifies clients that a property value has changed.
        /// </summary>
        [SuppressWidgetMember]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when the value of a property is updated.
        /// </summary>
        [SuppressWidgetMember]
        public event PropertyUpdatedEventHandler PropertyUpdated;

        /// <summary>
        /// Raises the propertychanged event.
        /// </summary>
        /// <param name="name">The name of the property that has been changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected void RaisePropertyChanged(string name, object oldValue, object newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            if (PropertyUpdated != null)
            {
                PropertyUpdated(this, new PropertyUpdatedEventArgs(name, oldValue, newValue));
            }
        }

        #endregion INotifyPropertyChanged Members

        internal void OnLegendSortChanged()
        {
            OnLegendSortChanged(Series);
        }

        internal void OnLegendSortChanged(IEnumerable seriesCollection)
        {
            foreach (Series currSeries in seriesCollection)
            {
                LegendBase legend = currSeries.ActualLegend;
                if (legend == null)
                {
                    continue;
                }

                Control item = currSeries.LegendItem;
                if (item == null)
                {
                    continue;
                }

                if (legend.Children.Contains(item))
                {
                    legend.Children.Remove(item);
                }
                if (!legend.Children.Contains(item)



                    )
                {
                    legend.AddChildInOrder(item, currSeries);
                }
            }

        }
        internal void OnSeriesMouseEnter(Series series, object item, object data)
        {

            RaiseSeriesMouseEnter(series, item, data as MouseEventArgs);

        }

        internal void OnSeriesMouseMove(Series series, object item, object data)
        {

            RaiseSeriesMouseMove(series, item, data as MouseEventArgs);

        }

        internal void OnSeriesMouseLeave(Series series, object item, object data)
        {

            RaiseSeriesMouseLeave(series, item, data as MouseEventArgs);

        }

        internal void OnSeriesMouseLeftButtonDown(Series series, object item, object data)
        {
            RaiseSeriesMouseLeftButtonDown(series, item, data as MouseButtonEventArgs);
        }

        internal void OnSeriesMouseLeftButtonUp(Series series, object item, object data)
        {
            var args = data as MouseButtonEventArgs;
            if (args != null)
            {
                RaiseSeriesMouseLeftButtonUp(series, item, data as MouseButtonEventArgs);
            }
        }

        internal void OnSeriesMouseRightButtonDown(Series series, object item, object data)
        {

            RaiseSeriesMouseRightButtonDown(series, item, data as MouseButtonEventArgs);

        }

        internal void OnSeriesMouseRightButtonUp(Series series, object item, object data)
        {

            RaiseSeriesMouseRightButtonUp(series, item, data as MouseButtonEventArgs);

        }
        #region Events


        /// <summary>
        /// Occurs when the cursors are moved over a series in this SeriesViewer.
        /// </summary>
        public event DataChartCursorEventHandler SeriesCursorMouseMove;

        internal void RaiseSeriesCursorMouseMove(Series series, object item)
        {
            if (SeriesCursorMouseMove != null && CrosshairsVisible)
            {
                SeriesCursorMouseMove(this, new ChartCursorEventArgs(this, series, item));
            }
        }


        /// <summary>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over a Series.
        /// </summary>
        public event DataChartMouseButtonEventHandler SeriesMouseLeftButtonDown;

        internal void RaiseSeriesMouseLeftButtonDown(Series series, object item, MouseButtonEventArgs e)
        {
            if (SeriesMouseLeftButtonDown != null)
            {
                SeriesMouseLeftButtonDown(this, new DataChartMouseButtonEventArgs(this, series, item, e));
            }
        }

        /// <summary>
        /// Occurs when the left mouse button is released while the mouse pointer is over a Series.
        /// </summary>
        public event DataChartMouseButtonEventHandler SeriesMouseLeftButtonUp;

        internal void RaiseSeriesMouseLeftButtonUp(Series series, object item, MouseButtonEventArgs e)
        {
            if (SeriesMouseLeftButtonUp != null)
            {
                SeriesMouseLeftButtonUp(this, new DataChartMouseButtonEventArgs(this, series, item, e));
            }
        }


        /// <summary>
        /// Occurs when the right mouse button is pressed while the mouse pointer is over a Series.
        /// </summary>
        public event DataChartMouseButtonEventHandler SeriesMouseRightButtonDown;
        internal void RaiseSeriesMouseRightButtonDown(Series series, object item, MouseButtonEventArgs e)
        {
            if (SeriesMouseRightButtonDown != null)
            {
                SeriesMouseRightButtonDown(this, new DataChartMouseButtonEventArgs(this, series, item, e));
            }
        }

        /// <summary>
        /// Occurs when the right mouse button is released while the mouse pointer is over a Series.
        /// </summary>
        public event DataChartMouseButtonEventHandler SeriesMouseRightButtonUp;
        internal void RaiseSeriesMouseRightButtonUp(Series series, object item, MouseButtonEventArgs e)
        {
            if (SeriesMouseRightButtonUp != null)
            {
                SeriesMouseRightButtonUp(this, new DataChartMouseButtonEventArgs(this, series, item, e));
            }
        }



        /// <summary>
        /// Occurs when the mouse pointer moves while over a Series.
        /// </summary>
        public event DataChartMouseEventHandler SeriesMouseMove;

        internal void RaiseSeriesMouseMove(Series series, object item, MouseEventArgs e)
        {
            if (SeriesMouseMove != null)
            {
                SeriesMouseMove(this, new ChartMouseEventArgs(this, series, item, e));
            }
        }

        /// <summary>
        /// Occurs when the mouse pointer enters a Series.
        /// </summary>
        public event DataChartMouseEventHandler SeriesMouseEnter;

        internal void RaiseSeriesMouseEnter(Series series, object item, MouseEventArgs e)
        {
            if (SeriesMouseEnter != null)
            {
                SeriesMouseEnter(this, new ChartMouseEventArgs(this, series, item, e));
            }
        }

        /// <summary>
        /// Occurs when the mouse pointer leaves a Series.
        /// </summary>
        public event DataChartMouseEventHandler SeriesMouseLeave;

        internal void RaiseSeriesMouseLeave(Series series, object item, MouseEventArgs e)
        {
            if (SeriesMouseLeave != null)
            {
                SeriesMouseLeave(this, new ChartMouseEventArgs(this, series, item, e));
            }
        }


        #endregion Events


        /// <summary>
        /// The current SeriesViewer object's horizontal Zoombar
        /// </summary>
        public XamZoombar HorizontalZoombar
        {
            get { return View.GetComponentsFromView().HorizontalZoombar; }
        }

        /// <summary>
        /// The current SeriesViewer object's vertical Zoombar
        /// </summary>
        public XamZoombar VerticalZoombar
        {
            get
            {
                return View.GetComponentsFromView().VerticalZoombar;
            }
        }


        internal Border PlotAreaBorder
        {
            get
            {
                return View.GetComponentsFromView().PlotAreaBorder;
            }
        }

        internal Popup ToolTipPopup
        {
            get
            {
                return View.GetComponentsFromView().ToolTipPopup;
            }
        }

        private Rect _viewport = Rect.Empty;

        /// <summary>
        /// Gets the viewport rectangle associated with the SeriesViewer, the physical dimensions of the plot area.
        /// </summary>
        public virtual Rect ViewportRect
        {
            get
            {
                return _viewport;
            }
        }

        private Rect _effectiveViewport = Rect.Empty;

        /// <summary>
        /// Gets the EffectiveViewport rectangle, representing the effective viewport area after margins have been subtracted.
        /// </summary>
        public Rect EffectiveViewport
        {
            get
            {
                return _effectiveViewport;
            }
        }
        /// <summary>
        /// Method used to calculate the effective viewport bounds.
        /// </summary>
        /// <param name="viewportRect">The actual viewport bounds.</param>
        /// <returns>The effective viewport bounds.</returns>
        protected virtual Rect ComputeEffectiveViewport(Rect viewportRect)
        {
            return Rect.Empty;
        }

        /// <summary>
        /// Set the given Axis viewport equal to the SeriesViewer viewport.
        /// </summary>
        /// <param name="axis"></param>
        protected void UpdateAxisViewport(Axis axis)
        {
            axis.ViewportRect = this.ViewportRect;
        }

        /// <summary>
        /// The property name of the WindowRect property.
        /// </summary>
        protected internal const string WindowRectPropertyName = "WindowRect";
        /// <summary>
        /// Identifies the WindowRect dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowRectProperty = DependencyProperty.Register(WindowRectPropertyName, typeof(Rect), typeof(SeriesViewer), new PropertyMetadata(new Rect(0, 0, 1, 1),
            (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(WindowRectPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// A rectangle representing the portion of the SeriesViewer currently in view.
        /// </summary>
        /// <remarks>
        /// A rectangle at X=0, Y=0 with a Height and Width of 1 implies the entire plotting area is in view.  A Height and Width of .5 would imply that the view is halfway zoomed in.
        /// </remarks>
        public Rect WindowRect
        {
            get
            {
                return (Rect)this.GetValue(WindowRectProperty);
            }
            set
            {
                this.SetValue(WindowRectProperty, value);
            }
        }



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Called so that the viewer can reconcile a plot area size change.
        /// </summary>
        /// <param name="oldGridAreaRect">The old size of the plot area.</param>
        /// <param name="newGridAreaRect">The new size of the plot area.</param>
        protected internal virtual void ProcessPlotAreaSizeChanged(Rect oldGridAreaRect, Rect newGridAreaRect)
        {
            if (!newGridAreaRect.IsEmpty)
            {
                _viewport = newGridAreaRect;
                _effectiveViewport = ComputeEffectiveViewport(_viewport);
                View.ViewportChanged(_viewport);
            }

            if (EffectiveIsSquare())
            {
                if (ActualSyncLink != null)
                {
                    WindowNotify(this.ActualWindowRect);
                }
            }

            OnPlotAreaSizeChanged(oldGridAreaRect, newGridAreaRect);

            #region adjust thumbnail



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


            #endregion adjust thumbnail




        }
        private SeriesViewerComponentsForView _componentsForView = new SeriesViewerComponentsForView();
        internal SeriesViewerComponentsForView GetComponentsForView()
        {

            _componentsForView.SeriesViewer = this;
            _componentsForView.CentralArea = CentralArea;


            return _componentsForView;
        }
        #region HorizontalZoomable Dependency Property

        /// <summary>
        /// Gets or sets the current SeriesViewer's horizontal zoomability.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [WidgetDefaultBoolean(false)]
        public bool HorizontalZoomable
        {
            get
            {
                return (bool)GetValue(HorizontalZoomableProperty);
            }
            set
            {
                SetValue(HorizontalZoomableProperty, value);
            }
        }

        internal const string HorizontalZoomablePropertyName = "HorizontalZoomable";
        /// <summary>
        /// Identifies the HorizontalZoomable dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalZoomableProperty = DependencyProperty.Register(HorizontalZoomablePropertyName, typeof(bool), typeof(SeriesViewer),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(HorizontalZoomablePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion HorizontalZoomable Dependency Property



        #region HorizontalZoombarVisibility Property

        /// <summary>
        /// Gets or sets the current SeriesViewer object's horizontal zoom bar visibility.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public Visibility HorizontalZoombarVisibility
        {
            get
            {
                return (Visibility)GetValue(HorizontalZoombarVisibilityProperty);
            }
            set
            {
                SetValue(HorizontalZoombarVisibilityProperty, value);
            }
        }

        internal const string HorizontalZoombarVisibilityPropertyName = "HorizontalZoombarVisibility";

        /// <summary>
        /// Identifies the HorizontalZoombarVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalZoombarVisibilityProperty = DependencyProperty.Register(HorizontalZoombarVisibilityPropertyName, typeof(Visibility), typeof(SeriesViewer),
            new PropertyMetadata(Visibility.Collapsed, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(HorizontalZoombarVisibilityPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion HorizontalZoombarVisibility Property



        #region VerticalZoomable Dependency Property

        /// <summary>
        /// Gets or sets the current SeriesViewer's vertical zoomability.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [WidgetDefaultBoolean(false)]
        public bool VerticalZoomable
        {
            get
            {
                return (bool)GetValue(VerticalZoomableProperty);
            }
            set
            {
                SetValue(VerticalZoomableProperty, value);
            }
        }

        internal const string VerticalZoomablePropertyName = "VerticalZoomable";

        /// <summary>
        /// Identifies the VerticalWindowMode dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalZoomableProperty = DependencyProperty.Register(VerticalZoomablePropertyName, typeof(bool), typeof(SeriesViewer),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(VerticalZoomablePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion VerticalZoomable Dependency Property



        #region VerticalZoombarVisibility Property

        /// <summary>
        /// Gets or sets the current SeriesViewer object's vertical zoom bar visibility.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public Visibility VerticalZoombarVisibility
        {
            get
            {
                return (Visibility)GetValue(VerticalZoombarVisibilityProperty);
            }
            set
            {
                SetValue(VerticalZoombarVisibilityProperty, value);
            }
        }

        internal const string VerticalZoombarVisibilityPropertyName = "VerticalZoombarVisibility";

        /// <summary>
        /// Identifies the VerticalZoombarVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalZoombarVisibilityProperty = DependencyProperty.Register(VerticalZoombarVisibilityPropertyName, typeof(Visibility), typeof(SeriesViewer),
            new PropertyMetadata(Visibility.Collapsed, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(VerticalZoombarVisibilityPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion VerticalZoombarVisibility Property


        internal void OnDetachedFromUI()
        {
            SyncManager.SuspendSyncChannel(this);
        }

        internal void OnAttachedToUI()
        {
            SyncManager.EnsureSyncChannel(this);
        }
        internal InteractionState State
        {
            get { return state; }
            private set
            {
                if (State != value)
                {
                    state = value;

                    switch (state)
                    {
                        case InteractionState.None:
                            View.SetDefaultCursor();
                            RenderCrosshairs();  // crosshairs can be displayed
                            // tooltips can be displayed
                            View.GoToIdleState();
                            break;

                        case InteractionState.DragZoom:
                            View.SetHandCursor();
                            //_interactionHide = true;
                            View.HideTooltip();
                            // detail canvas becomes visible
                            RenderCrosshairs();  // crosshairs cannot be displayed
                            View.GoToDraggingVisualState();
                            break;

                        case InteractionState.DragPan:
                            View.SetHandCursor();
                            //_interactionHide = true;
                            View.HideTooltip();
                            // detail canvas becomes visible
                            RenderCrosshairs();  // crosshairs cannot be displayed
                            View.GoToPanningVisualState();
                            break;


#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

                    }
                }
            }
        }

        private bool _interactionHide = false;

        private InteractionState state = InteractionState.None;

        /// <summary>
        /// anchor (mouse down or multitouch) in grid area coordinates.
        /// </summary>
        private Rect Anchor { get; set; }

        internal void OnMouseEnter(Point pt)
        {
            CrosshairNotify(ToWorld(pt));
        }

        internal void OnMouseLeave(Point pt)
        {
            CrosshairNotify(new Point(double.NaN, double.NaN));
        }

        internal bool OnMouseWheel(Point pt, double delta)
        {
            double cx = ActualWindowRect.Left + ActualWindowRect.Width * pt.X / ViewportRect.Width;
            double cy = ActualWindowRect.Top + ActualWindowRect.Height * pt.Y / ViewportRect.Height;

            double scale = 1.0 - MathUtil.Clamp(delta, -0.5, 0.5);
            double left = Math.Max(0.0, cx - scale * (cx - ActualWindowRect.Left));
            double bottom = Math.Min(1.0, cy + scale * (ActualWindowRect.Bottom - cy));
            double right = Math.Min(1.0, cx + scale * (ActualWindowRect.Right - cx));
            double top = Math.Max(0.0, cy - scale * (cy - ActualWindowRect.Top));

            // notify the chart

            WindowNotify(new Rect(left, top, right - left, bottom - top));

            return HorizontalZoomable || VerticalZoomable;
        }

        internal void OnDoubleTap(Point pt)
        {
            double cx = ActualWindowRect.Left + ActualWindowRect.Width * pt.X / ViewportRect.Width;
            double cy = ActualWindowRect.Top + ActualWindowRect.Height * pt.Y / ViewportRect.Height;

            double scale = 0.7;
            double left = Math.Max(0.0, cx - scale * (cx - ActualWindowRect.Left));
            double bottom = Math.Min(1.0, cy + scale * (ActualWindowRect.Bottom - cy));
            double right = Math.Min(1.0, cx + scale * (ActualWindowRect.Right - cx));
            double top = Math.Max(0.0, cy - scale * (cy - ActualWindowRect.Top));

            // notify the chart

            WindowNotify(new Rect(left, top, right - left, bottom - top));
        }

        private Rect WindowRectAtStartOfWindowOperation { get; set; }

        /// <summary>
        /// Called when a key is pressed down.
        /// </summary>
        /// <param name="key">The key that has been pressed.</param>
        /// <returns>True if the event is handled by the series viewer.</returns>
        public bool OnKeyDown(Key key)
        {
            if (State == InteractionState.DragPan && key == Key.Escape)
            {
                View.CancelMouseInteractions();
                State = InteractionState.None;

                PreviewNotify(Rect.Empty);
                if (WindowResponse == WindowResponse.Immediate)
                {
                    WindowNotify(this.WindowRectAtStartOfWindowOperation);
                }
                return true;
            }

            if (State == InteractionState.DragZoom && key == Key.Escape)
            {
                View.CancelMouseInteractions();
                State = InteractionState.None;

                View.HideDragPath();

                PreviewNotify(Rect.Empty);
                return true;
            }

            Rect windowRect = Rect.Empty;
            bool handled = false;

            switch (key)
            {
                case Key.Home:
                    handled = true;
                    windowRect = new Rect(0, 0, 1, 1);
                    break;

                case Key.PageDown:
                    handled = true;
                    windowRect = new Rect(ActualWindowRect.Left - 0.1 * ActualWindowRect.Width, ActualWindowRect.Top - 0.1 * ActualWindowRect.Height, 1.2 * ActualWindowRect.Width, 1.2 * ActualWindowRect.Height);
                    break;

                case Key.PageUp:
                    handled = true;
                    windowRect = new Rect(ActualWindowRect.Left + 0.1 * ActualWindowRect.Width, ActualWindowRect.Top + 0.1 * ActualWindowRect.Height, 0.8 * ActualWindowRect.Width, 0.8 * ActualWindowRect.Height);
                    break;

                case Key.Left:
                    handled = true;
                    windowRect = new Rect(ActualWindowRect.Left - 0.1 * ActualWindowRect.Width, ActualWindowRect.Top, ActualWindowRect.Width, ActualWindowRect.Height);
                    windowRect.X = windowRect.X - Math.Min(windowRect.Left, 0.0);
                    break;

                case Key.Right:
                    handled = true;
                    windowRect = new Rect(ActualWindowRect.Left + 0.1 * ActualWindowRect.Width, ActualWindowRect.Top, ActualWindowRect.Width, ActualWindowRect.Height);
                    windowRect.X = windowRect.X - Math.Max(windowRect.Right - 1.0, 0.0);
                    break;

                case Key.Up:
                    handled = true;
                    windowRect = new Rect(ActualWindowRect.Left, ActualWindowRect.Top - 0.1 * ActualWindowRect.Height, ActualWindowRect.Width, ActualWindowRect.Height);
                    windowRect.Y = windowRect.Y - Math.Min(windowRect.Top, 0.0);
                    break;

                case Key.Down:
                    handled = true;
                    windowRect = new Rect(ActualWindowRect.Left, ActualWindowRect.Top + 0.1 * ActualWindowRect.Height, ActualWindowRect.Width, ActualWindowRect.Height);
                    windowRect.Y = windowRect.Y - Math.Max(windowRect.Bottom - 1.0, 0.0);
                    break;
            }

            if (!windowRect.IsEmpty && windowRect != ActualWindowRect)
            {
                WindowNotify(windowRect);
            }

            return handled;
        }

        private bool _pinching = false;
        protected internal bool CrosshairsVisible { get; set; }



#region Infragistics Source Cleanup (Region)




































































































































































































































#endregion // Infragistics Source Cleanup (Region)


        internal void OnZoomChanged(Rect rc)
        {
            PreviewNotify(Rect.Empty);
            WindowNotify(rc);
        }

        /// <summary>
        /// Called when a contact has started at a location.
        /// </summary>
        /// <param name="pt">The location of the contact.</param>
        protected internal void OnContactStarted(Point pt, bool isFinger)
        {
            if (!_pinching)
            {
                _ignoreContactUp = false;
            }

            StartWindowOperation(pt);
        }

        private InteractionState ResolveDefaultInteraction()
        {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            return DefaultInteraction;

        }

        protected void StartWindowOperation(Point pt)
        {
            InteractionState newState;
            var defaultInteraction = ResolveDefaultInteraction();
            if (View.CurrentModifiers == ModifierKeys.None)
            {
                switch (defaultInteraction)
                {
                    case InteractionState.DragZoom:
                        newState = HorizontalZoomable || VerticalZoomable ? defaultInteraction : InteractionState.None;
                        break;
                    default:
                        newState = defaultInteraction;
                        break;
                }
            }
            else
            {


#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

                if (View.CurrentModifiers == DragModifier)
                {
                    newState = InteractionState.DragZoom;
                }
                else if (View.CurrentModifiers == PanModifier)
                {
                    newState = InteractionState.DragPan;
                }
                else
                {
                    newState = InteractionState.None;
                }
            }

            switch (newState)
            {
                case InteractionState.DragZoom:
                case InteractionState.DragPan:




                    //e.Handled = true;
                    View.FocusChart();
                    View.PlotAreaCaptureMouse();
                    break;
            }

            State = newState;

            switch (State)
            {
                case InteractionState.DragZoom:
                    this.Anchor = new Rect(pt.X, pt.Y, 0, 0);

                    View.ShowDragPath();
                    View.UpdateDragPath(this.Anchor);

                    PreviewNotify(Rect.Empty);
                    break;
                case InteractionState.DragPan:
                    this.Anchor = new Rect(pt.X, pt.Y, 0, 0);



                    break;


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            }
            this.WindowRectAtStartOfWindowOperation = this.ActualWindowRect;
        }

        private const double DRAG_DISTANCE = 10.0;
        private const double DRAG_DISTANCE_NEAR = 2.0;

        /// <summary>
        /// Called when a contact is moved to a new position.
        /// </summary>
        /// <param name="pt">The new position of the contact.</param>
        protected internal void OnContactMoved(Point pt, bool isFinger)
        {
            if (Anchor.IsEmpty && isFinger)
            {
                StartWindowOperation(pt);
            }

            double distance = DRAG_DISTANCE_NEAR;
            if (isFinger)
            {
                distance = DRAG_DISTANCE;
            }
            bool farFromAnchor = false;
            Rect rect = new Rect(new Point(this.Anchor.X, this.Anchor.Y), pt);

            if (rect.Width > distance && rect.Height > distance)
            {
                farFromAnchor = true;
                //if (_interactionHide)
                //{
                //    _interactionHide = false;
                //    View.HideTooltip();
                //}
            }

            if (!_pinching)
            {
                if (farFromAnchor)
                {
                    _ignoreContactUp = false;
                }
            }

            if (!_pinching)
            {
                CrosshairNotify(ToWorld(pt));
            }

            if (State == InteractionState.DragZoom)
            {
                if (farFromAnchor)
                {
                    Rect rc = this.ToWorld(rect);

                    PreviewNotify(rc);
                }
                else
                {
                    PreviewNotify(Rect.Empty);
                }

                View.UpdateDragPath(rect);
            }

            if (State == InteractionState.DragPan)
            {
                Rect gridAreaRect = ViewportRect;

                double left = ActualWindowRect.Left + ActualWindowRect.Width * (this.Anchor.X - pt.X) / gridAreaRect.Width;
                double top = ActualWindowRect.Top + ActualWindowRect.Height * (this.Anchor.Y - pt.Y) / gridAreaRect.Height;

                Rect rc = new Rect(left, top, ActualWindowRect.Width, ActualWindowRect.Height);
                if (WindowResponse == WindowResponse.Immediate)
                {
                    PreviewNotify(Rect.Empty);
                    WindowNotify(rc);
                    this.Anchor = new Rect(pt, new Size(0, 0));


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

                }
                else
                {
                    PreviewNotify(rc);
                }
            }


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

            View.CheckInteractionCompleted(pt);
        }

        private bool _ignoreContactUp = false;

        /// <summary>
        /// Called when a contact is completed.
        /// </summary>
        /// <param name="pt">The point of contact.</param>
        protected internal void OnContactCompleted(Point pt, bool isFinger)
        {
            if (!_ignoreContactUp)
            {
                double distance = DRAG_DISTANCE_NEAR;
                if (isFinger)
                {
                    distance = DRAG_DISTANCE;
                }
                bool farFromAnchor = false;
                Rect rect = new Rect(new Point(this.Anchor.X, this.Anchor.Y), pt);

                if (rect.Width > distance && rect.Height > distance)
                {
                    farFromAnchor = true;
                }

                if (State == InteractionState.DragZoom)
                {
                    View.CompleteMouseCapture();
                    View.HideDragPath();
                    
                    if (farFromAnchor)
                    {
                        WindowNotify(ToWorld(rect));
                    }

                    PreviewNotify(Rect.Empty);
                }

                if (State == InteractionState.DragPan)
                {
                    View.CompleteMouseCapture();

                    Rect gridAreaRect = ViewportRect;
                    double left = ActualWindowRect.Left + ActualWindowRect.Width * (this.Anchor.X - pt.X) / gridAreaRect.Width;
                    double top = ActualWindowRect.Top + ActualWindowRect.Height * (this.Anchor.Y - pt.Y) / gridAreaRect.Height;

                    WindowNotify(new Rect(left, top, ActualWindowRect.Width, ActualWindowRect.Height));
                    PreviewNotify(Rect.Empty);
                }



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

            }

            State = InteractionState.None;






            if (isFinger)
            {
                View.HideTooltip();



            }
           
        }



        internal void UpdateSyncSettings(object sender, PropertyUpdatedEventArgs e)
        {
            if (e.PropertyName == SyncSettings.SyncChannelPropertyName)
            {
                SyncManager.ChangeSyncChannel(this, (string)e.OldValue, (string)e.NewValue);
            }
        }

        internal virtual void UpdateSyncLink(SyncLink oldLink,
            SyncLink newLink)
        {
            if (oldLink != null)
            {
                oldLink.ChartsInternal.Remove(this);
                oldLink.PropertyUpdated -= PropertyUpdated;
            }

            if (newLink != null)
            {
                newLink.ChartsInternal.Add(this);
                newLink.PropertyUpdated += PropertyUpdated;
            }

            foreach (Series series in Series)
            {
                series.SyncLink = ActualSyncLink;
                series.SeriesViewer = this;
            }
        }

        /// <summary>
        /// Called to notify of the crosshair point changing.
        /// </summary>
        /// <param name="notificationPoint">The new crosshair point.</param>
        protected internal virtual void CrosshairNotify(Point notificationPoint)
        {
            ActualSyncLink.CrosshairNotify(this, notificationPoint);
        }

        /// <summary>
        /// Called to notify of the window rectangle changing.
        /// </summary>
        /// <param name="windowRect">The new window rectangle.</param>
        protected internal virtual void WindowNotify(Rect windowRect)
        {
            ActualSyncLink.WindowNotify(this, windowRect);
        }

        /// <summary>
        /// Called to notify of the preview rectangle changing.
        /// </summary>
        /// <param name="previewRect">The new preview rectangle.</param>
        protected internal virtual void PreviewNotify(Rect previewRect)
        {
            ActualSyncLink.PreviewNotify(this, previewRect);
        }



        internal void OnZoomChanging(Rect rc)
        {
            if (WindowResponse == WindowResponse.Immediate)
            {
                PreviewNotify(Rect.Empty);
                WindowNotify(rc);
            }
            else
            {
                PreviewNotify(rc);
            }
        }

        internal bool IsSyncReady
        {
            get
            {
                return ActualSyncLink != null;
            }
        }

        internal IEnumerable<SeriesViewer> SynchronizedCharts()
        {
            if (ActualSyncLink == null)
            {
                yield break;
            }





            foreach (SeriesViewer chart in ActualSyncLink.Charts)
            {

                yield return chart;
            }
        }
        private Point ToWorld(Point pt)
        {
            Rect gridAreaRect = ViewportRect;

            double x = ActualWindowRect.Left + ActualWindowRect.Width * pt.X / gridAreaRect.Width;
            double y = ActualWindowRect.Top + ActualWindowRect.Height * pt.Y / gridAreaRect.Height;

            return new Point(x, y);
        }

        private Rect ToWorld(Rect rect)
        {
            Rect gridAreaRect = ViewportRect;

            double left = ActualWindowRect.Left + ActualWindowRect.Width * rect.Left / gridAreaRect.Width;
            double top = ActualWindowRect.Top + ActualWindowRect.Height * rect.Top / gridAreaRect.Height;
            double right = ActualWindowRect.Left + ActualWindowRect.Width * rect.Right / gridAreaRect.Width;
            double bottom = ActualWindowRect.Top + ActualWindowRect.Height * rect.Bottom / gridAreaRect.Height;

            return new Rect(left, top, right - left, bottom - top);
        }
        /// <summary>
        /// Occurs just after the current SeriesViewer's window rectangle is changed.
        /// </summary>
        public event RectChangedEventHandler WindowRectChanged;

        private void RaiseWindowRectChanged(Rect oldWindowRect, Rect newWindowRect)
        {



            if (WindowRectChanged != null && oldWindowRect != newWindowRect)
            {
                WindowRectChanged(this, new RectChangedEventArgs(oldWindowRect, newWindowRect));
            }



        }
        internal void OnWindowRectChanged(Rect oldRect, Rect newRect)
        {
            RaiseWindowRectChanged(oldRect, newRect);
        }



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Raised when the actual window rectangle of the SeriesViewer changes.
        /// </summary>
        [SuppressWidgetMember]
        public event RectChangedEventHandler ActualWindowRectChanged;
        private void RaiseActualWindowRectChanged(Rect oldRect, Rect newRect)
        {
            if (this.ActualWindowRectChanged != null



                && oldRect != newRect

)
            {
                this.ActualWindowRectChanged(this, new RectChangedEventArgs(oldRect, newRect));
            }
        }

        /// <summary>
        /// Resets the zoom level to default.
        /// </summary>
        public void ResetZoom()
        {
            View.ResetWindowRect();
        }

        /// <summary>
        /// Occurs just after the current SeriesViewer's grid area rectangle is changed.
        /// </summary>
        /// <remarks>
        /// The grid area may change as the result of the SeriesViewer being resized, or
        /// of an axis being added or changing size, possibly in another SeriesViewer.
        /// </remarks>
        public event RectChangedEventHandler GridAreaRectChanged;

        private void RaiseGridAreaRectChanged(Rect oldGridAreaRect, Rect newGridAreaRect)
        {
            if (GridAreaRectChanged != null && oldGridAreaRect != newGridAreaRect)
            {
                GridAreaRectChanged(this, new RectChangedEventArgs(oldGridAreaRect, newGridAreaRect));
            }
        }
        internal void OnPlotAreaSizeChanged(Rect oldGridAreaRect, Rect newGridAreaRect)
        {
            RaiseGridAreaRectChanged(oldGridAreaRect, newGridAreaRect);
            this.InvalidateActualWindowRect();
        }

        private const string WindowResponsePropertyName = "WindowResponse";
        /// <summary>
        /// Identifies the WindowResponse dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowResponseProperty = DependencyProperty.Register(WindowResponsePropertyName, typeof(WindowResponse), typeof(SeriesViewer), new PropertyMetadata(



            WindowResponse.Deferred, 

            (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(WindowResponsePropertyName, e.OldValue, e.NewValue);
            }));
        /// <summary>
        /// The response to user panning and zooming: whether to update the view immediately while the user action is happening, or to defer the update to after the user action is complete.  The user action will be an action such as a mouse drag which causes panning and/or zooming to occur.
        /// </summary>
        public WindowResponse WindowResponse
        {
            get
            {
                return (WindowResponse)this.GetValue(WindowResponseProperty);
            }
            set
            {
                this.SetValue(WindowResponseProperty, value);
            }
        }







        internal bool DontNotify { get; set; }

        internal const string WindowRectMinWidthPropertyName = "WindowRectMinWidth";

        /// <summary>
        /// Identifies the WindowRectMinWidth property.
        /// </summary>
        public static readonly DependencyProperty WindowRectMinWidthProperty = DependencyProperty.Register(
            WindowRectMinWidthPropertyName, typeof(double), typeof(SeriesViewer),
            new PropertyMetadata(0.0001, (o, e) => (o as SeriesViewer).RaisePropertyChanged(WindowRectMinWidthPropertyName, e.OldValue, e.NewValue)));

        /// <summary>
        /// Sets or gets the minimum width that the window rect is allowed to reach before being clamped.
        /// Decrease this value if you want to allow for further zooming into the viewer.
        /// If this value is lowered too much it can cause graphical corruption due to floating point arithmetic inaccuracy.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double WindowRectMinWidth
        {
            get
            {
                return (double)GetValue(WindowRectMinWidthProperty);
            }
            set
            {
                SetValue(WindowRectMinWidthProperty, value);
            }
        }

        //#if TINYCLR
        //        internal const double WINDOWRECT_MINIMUM_WIDTH = 4.94066e-324;
        //#else
        //        internal const double WINDOWRECT_MINIMUM_WIDTH = double.Epsilon;
        //#endif
        //        internal const double WINDOWRECT_MINIMUM_HEIGHT = WINDOWRECT_MINIMUM_WIDTH;

        /// <summary>
        /// Sets or gets the Synchronization channel to use for the SeriesViewer.
        /// </summary>
        [SuppressWidgetMember]
        public string SyncChannel { get; set; }
        private SyncLink _actualSyncLink;
        /// <summary>
        /// Gets or sets the sync manager used to synchronize SeriesViewers.
        /// </summary>
        [SuppressWidgetMember]
        public SyncLink ActualSyncLink
        {
            get
            {
                return _actualSyncLink;
            }
            set
            {
                var oldValue = _actualSyncLink;
                _actualSyncLink = value;
                RaisePropertyChanged(ActualSyncLinkPropertyName, oldValue, _actualSyncLink);
            }
        }
        internal const string ActualSyncLinkPropertyName = "ActualSyncLink";
        internal void InvalidatePanels()
        {

            CentralArea.InvalidateArrange();



        }

        internal const string OverviewPlusDetailPaneVisibilityPropertyName = "OverviewPlusDetailPaneVisibility";
        /// <summary>
        /// Identifies the OverviewPlusDetailPaneVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty OverviewPlusDetailPaneVisibilityProperty = DependencyProperty.Register(OverviewPlusDetailPaneVisibilityPropertyName, typeof(Visibility), typeof(SeriesViewer), new PropertyMetadata(Visibility.Collapsed, (sender, e) =>
        {
            (sender as SeriesViewer).RaisePropertyChanged(OverviewPlusDetailPaneVisibilityPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// The visibility of the OverviewPlusDetailPane.
        /// </summary>
        [WidgetDefaultString("collapsed")]
        public Visibility OverviewPlusDetailPaneVisibility
        {
            get
            {
                return (Visibility)this.GetValue(OverviewPlusDetailPaneVisibilityProperty);
            }
            set
            {
                this.SetValue(OverviewPlusDetailPaneVisibilityProperty, value);
            }
        }


        internal XamOverviewPlusDetailPane OverviewPlusDetailPane
        {
            get
            {
                return this.View.GetComponentsFromView().OverviewPlusDetailPane;
            }
        }



        internal const string OverviewPlusDetailPaneStylePropertyName = "OverviewPlusDetailPaneStyle";
        /// <summary>
        /// Identifies the OverviewPlusDetailPaneStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty OverviewPlusDetailPaneStyleProperty = DependencyProperty.Register(OverviewPlusDetailPaneStylePropertyName, typeof(Style), typeof(SeriesViewer), new PropertyMetadata(null, (sender, e) =>
        {
            (sender as SeriesViewer).RaisePropertyChanged(OverviewPlusDetailPaneStylePropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// The Style to apply to the OverviewPlusDetailPane.
        /// </summary>
        public Style OverviewPlusDetailPaneStyle
        {
            get
            {
                return this.GetValue(OverviewPlusDetailPaneStyleProperty) as Style;
            }
            set
            {
                this.SetValue(OverviewPlusDetailPaneStyleProperty, value);
            }
        }

        internal const string OverviewPlusDetailPaneHorizontalAlignmentPropertyName = "OverviewPlusDetailPaneHorizontalAlignment";
        /// <summary>
        /// Identifies the OverviewPlusDetailPaneHorizontalAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty OverviewPlusDetailPaneHorizontalAlignmentProperty = DependencyProperty.Register(OverviewPlusDetailPaneHorizontalAlignmentPropertyName, typeof(HorizontalAlignment), typeof(SeriesViewer), new PropertyMetadata(HorizontalAlignment.Right, (sender, e) =>
        {
            (sender as SeriesViewer).RaisePropertyChanged(OverviewPlusDetailPaneHorizontalAlignmentPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// The horizontal alignment of the OverviewPlusDetailPane.
        /// </summary>
        public HorizontalAlignment OverviewPlusDetailPaneHorizontalAlignment
        {
            get
            {
                return (HorizontalAlignment)this.GetValue(OverviewPlusDetailPaneHorizontalAlignmentProperty);
            }
            set
            {
                this.SetValue(OverviewPlusDetailPaneHorizontalAlignmentProperty, value);
            }
        }

        internal const string OverviewPlusDetailPaneVerticalAlignmentPropertyName = "OverviewPlusDetailPaneVerticalAlignment";
        /// <summary>
        /// Identifies the OverviewPlusDetailPaneVerticalAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty OverviewPlusDetailPaneVerticalAlignmentProperty = DependencyProperty.Register(OverviewPlusDetailPaneVerticalAlignmentPropertyName, typeof(VerticalAlignment), typeof(SeriesViewer), new PropertyMetadata(VerticalAlignment.Bottom, (sender, e) =>
        {
            (sender as SeriesViewer).RaisePropertyChanged(OverviewPlusDetailPaneVerticalAlignmentPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// The vertical alignment of the OverviewPlusDetailPane.
        /// </summary>
        public VerticalAlignment OverviewPlusDetailPaneVerticalAlignment
        {
            get
            {
                return (VerticalAlignment)this.GetValue(OverviewPlusDetailPaneVerticalAlignmentProperty);
            }
            set
            {
                this.SetValue(OverviewPlusDetailPaneVerticalAlignmentProperty, value);
            }
        }


        #region CrosshairVisibility Property

        /// <summary>
        /// Gets or sets the current SeriesViewer's crosshair visibility override.
        /// </summary>
        [WidgetDefaultString("collapsed")]
        public Visibility CrosshairVisibility
        {
            get
            {
                return (Visibility)GetValue(CrosshairVisibilityProperty);
            }
            set
            {
                SetValue(CrosshairVisibilityProperty, value);
            }
        }

        internal const string CrosshairVisibilityPropertyName = "CrosshairVisibility";

        /// <summary>
        /// Identifies the CrosshairVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty CrosshairVisibilityProperty = DependencyProperty.Register(CrosshairVisibilityPropertyName, typeof(Visibility), typeof(SeriesViewer),
            new PropertyMetadata(Visibility.Visible, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(CrosshairVisibilityPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion CrosshairVisibility Property

        /// <summary>
        /// The current SeriesViewer object's central area in its top-level panel.
        /// </summary>
        internal ChartAreaPanel CentralArea { get; private set; }

        #region PlotAreaBorderBrush Property

        /// <summary>
        /// Gets or sets the brush used as the border for the current SeriesViewer object's plot area.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public Brush PlotAreaBorderBrush
        {
            get
            {
                return (Brush)GetValue(PlotAreaBorderBrushProperty);
            }
            set
            {
                SetValue(PlotAreaBorderBrushProperty, value);
            }
        }

        internal const string PlotAreaBorderBrushPropertyName = "PlotAreaBorderBrush";

        /// <summary>
        /// Identifies the GridAreaBorderBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty PlotAreaBorderBrushProperty = DependencyProperty.Register(PlotAreaBorderBrushPropertyName, typeof(Brush), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(PlotAreaBorderBrushPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion PlotAreaBorderBrush Property

        #region PlotAreaBorderThickness Property


        /// <summary>
        /// Gets or sets the thickness of the border for the current SeriesViewer object's plot area.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Thickness PlotAreaBorderThickness
        {
            get
            {
                return (Thickness)GetValue(PlotAreaBorderThicknessProperty);
            }
            set
            {
                SetValue(PlotAreaBorderThicknessProperty, value);
            }
        }

        /// <summary>
        /// Identifies the GridAreaBorderThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty PlotAreaBorderThicknessProperty = DependencyProperty.Register(PlotAreaBorderThicknessPropertyName, typeof(Thickness), typeof(SeriesViewer),
            new PropertyMetadata((sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(PlotAreaBorderThicknessPropertyName, e.OldValue, e.NewValue);
            }));

        internal const string PlotAreaBorderThicknessPropertyName = "PlotAreaBorderThickness";

        #endregion PlotAreaBorderThickness Property

        #region PlotAreaBackground Property

        /// <summary>
        /// Gets or sets the brush used as the background for the current SeriesViewer object's plot area.
        /// </summary>
        public Brush PlotAreaBackground
        {
            get
            {
                return (Brush)GetValue(PlotAreaBackgroundProperty);
            }
            set
            {
                SetValue(PlotAreaBackgroundProperty, value);
            }
        }

        internal const string PlotAreaBackgroundPropertyName = "PlotAreaBackground";

        /// <summary>
        /// Identifies the PlotAreaBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty PlotAreaBackgroundProperty = DependencyProperty.Register(PlotAreaBackgroundPropertyName, typeof(Brush), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(PlotAreaBackgroundPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion PlotAreaBackground Property



        #region PlotAreaMinWidth Property

        /// <summary>
        /// Gets or sets the minimum width used to size the plot area.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public double PlotAreaMinWidth
        {
            get
            {
                return (double)GetValue(PlotAreaMinWidthProperty);
            }
            set
            {
                SetValue(PlotAreaMinWidthProperty, value);
            }
        }

        internal const string PlotAreaMinWidthPropertyName = "PlotAreaMinWidth";

        /// <summary>
        /// Identifies the PlotAreaMinWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty PlotAreaMinWidthProperty = DependencyProperty.Register(PlotAreaMinWidthPropertyName, typeof(double), typeof(SeriesViewer),
            new PropertyMetadata(50.0, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(PlotAreaMinWidthPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion PlotAreaMinWidth Property

        #region PlotAreaMinHeight Property

        /// <summary>
        /// Gets or sets the minimum Height used to size the plot area.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public double PlotAreaMinHeight
        {
            get
            {
                return (double)GetValue(PlotAreaMinHeightProperty);
            }
            set
            {
                SetValue(PlotAreaMinHeightProperty, value);
            }
        }

        internal const string PlotAreaMinHeightPropertyName = "PlotAreaMinHeight";

        /// <summary>
        /// Identifies the PlotAreaMinHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty PlotAreaMinHeightProperty = DependencyProperty.Register(PlotAreaMinHeightPropertyName, typeof(double), typeof(SeriesViewer),
            new PropertyMetadata(50.0, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(PlotAreaMinHeightPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion PlotAreaMinHeight Property
        private void RenderCrosshairs()
        {
            var crosshairPoint = CrosshairPoint;






            bool visible = State == InteractionState.None &&
                CrosshairsVisible &&
                !_pinching;


            Rect viewportRect = ViewportRect;

            if (visible && !double.IsNaN(crosshairPoint.X) && !viewportRect.IsEmpty)
            {
                double x = viewportRect.Width * (crosshairPoint.X - ActualWindowRect.Left) / ActualWindowRect.Width;

                View.UpdateVerticalCrosshair(viewportRect.Left + x, viewportRect.Top, viewportRect.Left + x, viewportRect.Bottom);
                View.ShowVerticalCrosshair();



            }
            else
            {
                View.HideVerticalCrosshair();
            }

            if (visible && !double.IsNaN(crosshairPoint.Y) && !viewportRect.IsEmpty)
            {
                double y = viewportRect.Height * (crosshairPoint.Y - ActualWindowRect.Top) / ActualWindowRect.Height;

                View.UpdateHorizontalCrosshair(viewportRect.Left, viewportRect.Top + y, viewportRect.Right, viewportRect.Top + y);
                View.ShowHorizontalCrosshair();
            }
            else
            {
                View.HideHorizontalCrosshair();
            }
        }
        private int _windowEventDepth = 0;

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning SeriesViewer. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected virtual void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            switch (propertyName)
            {
                case ActualSyncLinkPropertyName:
                    UpdateSyncLink(oldValue as SyncLink, newValue as SyncLink);
                    break;

                case SeriesViewer.CrosshairVisibilityPropertyName:
                    if (CrosshairVisibility != System.Windows.Visibility.Collapsed)
                    {
                        CrosshairsVisible = true;
                    }
                    else
                    {



                        CrosshairsVisible = false;

                    }
                    RenderCrosshairs();
                    break;

                case SeriesViewer.CrosshairPointPropertyName:
                    RenderCrosshairs();
                    break;



                case SeriesViewer.WindowRectPropertyName:
                    if (this.ActualSyncLink != null)
                    {
                        _windowEventDepth++;
                        WindowNotify((Rect)newValue);
                        _windowEventDepth--;
                    }
                    if (!WindowRect.Equals((Rect)oldValue) && _windowEventDepth == 0)
                    {
                        OnWindowRectChanged((Rect)oldValue, (Rect)WindowRect);
                    }
                    this.InvalidateActualWindowRect();
                    break;
                case SeriesViewer.WindowScaleHorizontalPropertyName:
                    ActualWindowScaleHorizontal = WindowScaleHorizontal;
                    break;
                case SeriesViewer.WindowScaleVerticalPropertyName:
                    ActualWindowScaleVertical = WindowScaleVertical;
                    break;
                case SeriesViewer.WindowPositionHorizontalPropertyName:
                    ActualWindowPositionHorizontal = WindowPositionHorizontal;
                    break;
                case SeriesViewer.WindowPositionVerticalPropertyName:
                    ActualWindowPositionVertical = WindowPositionVertical;
                    break;
                case SeriesViewer.ActualWindowScaleHorizontalPropertyName:
                case SeriesViewer.ActualWindowScaleVerticalPropertyName:
                case SeriesViewer.ActualWindowPositionVerticalPropertyName:
                case SeriesViewer.ActualWindowPositionHorizontalPropertyName:
                    if (!this.SuspendWindowRect)
                    {
                        double minWidth = WindowRectMinWidth;
                        this.WindowRect = new Rect(
                            ActualWindowPositionHorizontal,
                            ActualWindowPositionVertical,
                            MathUtil.Clamp(ActualWindowScaleHorizontal, minWidth, 1.0),
                            MathUtil.Clamp(ActualWindowScaleVertical, minWidth, 1.0));
                    }
                    break;

                case SeriesViewer.OverviewPlusDetailPaneVisibilityPropertyName:




                        _useOPD = OverviewPlusDetailPaneVisibility == Visibility.Visible;





                        View.OverviewPlusDetailPaneVisibilityChanged();
                        View.UpdateOverviewPlusDetailRects();
                        InvalidatePanels();


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

                    break;

                case SeriesViewer.ActualWindowRectPropertyName:
                    if (oldValue != newValue)
                    {
                        this.ActualWindowRectUpdated();
                    }
                    this.RaiseActualWindowRectChanged((Rect)oldValue, (Rect)newValue);
                    break;
                case SeriesViewer.PlotAreaBackgroundPropertyName:
                    View.OnPlotAreaBackgroundChanged((Brush)newValue);
                    break;

                case HorizontalZoomablePropertyName:
                case VerticalZoomablePropertyName:
                    this.UpdateOverviewPlusDetailPaneControlPanelVisibility();
                    break;

                case SeriesViewer.OverviewPlusDetailPaneHorizontalAlignmentPropertyName:
                case SeriesViewer.OverviewPlusDetailPaneVerticalAlignmentPropertyName:
                    InvalidatePanels();
                    break;
                case PlotAreaMinHeightPropertyName:
                case PlotAreaMinWidthPropertyName:
                    CentralArea.InvalidateMeasure();
                    break;


            }
        }


        internal void UpdateOverviewPlusDetailPaneControlPanelVisibility()
        {

            if (this.OverviewPlusDetailPane == null)
            {
                return;
            }
            this.OverviewPlusDetailPane.IsZoomable = this.HorizontalZoomable || this.VerticalZoomable;

        }

        private bool _useOPD = false;


        private bool SuspendWindowRect { get; set; }


        #region DefaultInteraction Dependency Property

        /// <summary>
        /// Gets or sets the DefaultInteraction property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The default interaction state defines the SeriesViewer's response to mouse events.
        /// </remarks>
        [WidgetDefaultString("dragZoom")]
        public InteractionState DefaultInteraction
        {
            get
            {
                return (InteractionState)GetValue(DefaultInteractionProperty);
            }
            set
            {
                SetValue(DefaultInteractionProperty, value);
            }
        }

        internal const string DefaultInteractionPropertyName = "DefaultInteraction";

        /// <summary>
        /// Identifies the DefaultInteractionState dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultInteractionProperty = DependencyProperty.Register(DefaultInteractionPropertyName,
            typeof(InteractionState), typeof(SeriesViewer),
            new PropertyMetadata(



                InteractionState.DragZoom, 

                (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(DefaultInteractionPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion DefaultInteraction Dependency Property






        #region DragModifier Property

        /// <summary>
        /// Gets or sets the current SeriesViewer's DragModifier property.
        /// </summary>
        [WidgetDefaultString("none")]
        public ModifierKeys DragModifier
        {
            get
            {
                return (ModifierKeys)GetValue(DragModifierProperty);
            }
            set
            {
                SetValue(DragModifierProperty, value);
            }
        }

        internal const string DragModifierPropertyName = "DragModifier";

        /// <summary>
        /// Identifies the DragModifier dependency property.
        /// </summary>
        public static readonly DependencyProperty DragModifierProperty = DependencyProperty.Register(DragModifierPropertyName, typeof(ModifierKeys), typeof(SeriesViewer),
            new PropertyMetadata(ModifierKeys.None, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(DragModifierPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion DragModifier Property

        #region PanModifier Property

        /// <summary>
        /// Gets or sets the current SeriesViewer's PanModifier property.
        /// </summary>
        [WidgetDefaultString("shift")]
        public ModifierKeys PanModifier
        {
            get
            {
                return (ModifierKeys)GetValue(PanModifierProperty);
            }
            set
            {
                SetValue(PanModifierProperty, value);
            }
        }

        internal const string PanModifierPropertyName = "PanModifier";

        /// <summary>
        /// Identifies the PanModifier dependency property.
        /// </summary>
        public static readonly DependencyProperty PanModifierProperty = DependencyProperty.Register(PanModifierPropertyName, typeof(ModifierKeys), typeof(SeriesViewer),
            new PropertyMetadata(ModifierKeys.Shift, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(PanModifierPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion PanModifier Property
        #region Inter-Chart interaction

        private Rect previewRect = Rect.Empty;

        /// <summary>
        /// Gets or sets the preview rectangle.
        /// </summary>
        /// <remarks>
        /// The preview rectangle may be set to Rect.Empty, in which case the visible preview
        /// strokePath is hidden.
        /// </remarks>
        public Rect PreviewRect
        {
            get { return previewRect; }
            set
            {
                var oldPreviewRect = previewRect;
                previewRect = value;

                if (previewRect.IsEmpty)
                {
                    View.HidePreviewPath();






                }
                else
                {
                    View.UpdatePreviewPath(ViewportRect, ToGridArea(PreviewRect));
                    View.ShowPreviewPath();
                    this.RaisePropertyChanged("PreviewRect", oldPreviewRect, previewRect);


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                }
            }
        }

        internal void ActualWindowRectUpdated()
        {
            bool suspendWindowRectStored = this.SuspendWindowRect;
            this.SuspendWindowRect = true;
            ActualWindowPositionHorizontal = ActualWindowRect.X;
            ActualWindowPositionVertical = ActualWindowRect.Y;
            ActualWindowScaleHorizontal = ActualWindowRect.Width;
            ActualWindowScaleVertical = ActualWindowRect.Height;
            this.SuspendWindowRect = suspendWindowRectStored;


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

            // the preview strokePath

            // the drag strokePath
            View.HideTooltip();


            View.UpdateZoombars(ActualWindowRect);



        }



        internal const string WindowPositionHorizontalPropertyName = "WindowPositionHorizontal";
        /// <summary>
        /// Identifies the WindowPositionHorizontal dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowPositionHorizontalProperty = DependencyProperty.Register(WindowPositionHorizontalPropertyName, typeof(double), typeof(SeriesViewer), new PropertyMetadata(double.NaN,
            (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(WindowPositionHorizontalPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// A number between 0 and 1 determining the position of the horizontal scroll.
        /// </summary>
        /// <remarks>
        /// This property is effectively a shortcut to the X position of the WindowRect property.
        /// </remarks>
        [WidgetDefaultNumber(0.0)]
        public double WindowPositionHorizontal
        {
            get
            {
                return (double)this.GetValue(WindowPositionHorizontalProperty);
            }
            set
            {
                this.SetValue(WindowPositionHorizontalProperty, value);
            }
        }

        internal const string WindowPositionVerticalPropertyName = "WindowPositionVertical";
        /// <summary>
        /// Identifies the WindowPositionVertical dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowPositionVerticalProperty = DependencyProperty.Register(WindowPositionVerticalPropertyName, typeof(double), typeof(SeriesViewer), new PropertyMetadata(double.NaN,
            (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(WindowPositionVerticalPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// A number between 0 and 1 determining the position of the vertical scroll.
        /// </summary>
        /// <remarks>
        /// This property is effectively a shortcut to the Y position of the WindowRect property.
        /// </remarks>
        [WidgetDefaultNumber(0.0)]
        public double WindowPositionVertical
        {
            get
            {
                return (double)this.GetValue(WindowPositionVerticalProperty);
            }
            set
            {
                this.SetValue(WindowPositionVerticalProperty, value);
            }
        }

        internal const string WindowScaleHorizontalPropertyName = "WindowScaleHorizontal";
        /// <summary>
        /// Identifies the WindowScaleHorizontal dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowScaleHorizontalProperty = DependencyProperty.Register(WindowScaleHorizontalPropertyName, typeof(double), typeof(SeriesViewer), new PropertyMetadata(double.NaN,
            (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(WindowScaleHorizontalPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// A number between 0 and 1 determining the scale of the horizontal zoom.
        /// </summary>
        /// <remarks>
        /// This property is effectively a shortcut to the Width of the WindowRect property.
        /// </remarks>
        [WidgetDefaultNumber(1.0)]
        public double WindowScaleHorizontal
        {
            get
            {
                return (double)this.GetValue(WindowScaleHorizontalProperty);
            }
            set
            {
                this.SetValue(WindowScaleHorizontalProperty, value);
            }
        }

        internal const string WindowScaleVerticalPropertyName = "WindowScaleVertical";
        /// <summary>
        /// Identifies the WindowScaleVertical dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowScaleVerticalProperty = DependencyProperty.Register(WindowScaleVerticalPropertyName, typeof(double), typeof(SeriesViewer), new PropertyMetadata(double.NaN,
            (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(WindowScaleVerticalPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// A number between 0 and 1 determining the scale of the vertical zoom.
        /// </summary>
        /// <remarks>
        /// This property is effectively a shortcut to the Height of the WindowRect property.
        /// </remarks>
        [WidgetDefaultNumber(1.0)]
        public double WindowScaleVertical
        {
            get
            {
                return (double)this.GetValue(WindowScaleVerticalProperty);
            }
            set
            {
                this.SetValue(WindowScaleVerticalProperty, value);
            }
        }

        #endregion Inter-Chart interaction
        #region Conversion between world and grid area coordinates



        private Rect ToGridArea(Rect rect)
        {
            if (ViewportRect.IsEmpty)
            {
                return Rect.Empty;
            }
            Rect gridAreaRect = ViewportRect;

            double left = gridAreaRect.Left + (gridAreaRect.Width * (rect.Left - ActualWindowRect.Left) / ActualWindowRect.Width);
            double top = gridAreaRect.Top + (gridAreaRect.Height * (rect.Top - ActualWindowRect.Top) / ActualWindowRect.Height);
            double right = gridAreaRect.Left + (gridAreaRect.Width * (rect.Right - ActualWindowRect.Left) / ActualWindowRect.Width);
            double bottom = gridAreaRect.Top + (gridAreaRect.Height * (rect.Bottom - ActualWindowRect.Top) / ActualWindowRect.Height);

            return new Rect(left, top, right - left, bottom - top);
        }

        #endregion Conversion between world and grid area coordinates

        private const string ContentPresenterName = "ContentPresenter";


        /// <summary>
        /// Gets the current SeriesViewer's root ContentPresenter.
        /// </summary>
        public ContentPresenter ContentPresenter
        {
            get { return contentPresenter; }
            private set
            {
                if (ContentPresenter != null)
                {
                    ContentPresenter.Content = null;
                }

                contentPresenter = value;

                if (ContentPresenter != null)
                {
                    ContentPresenter.Content = CentralArea;
                }
            }
        }

        private ContentPresenter contentPresenter;

        /// <summary>
        /// Raised when the SeriesViewer's processing for an update has completed.
        /// </summary>
        public event EventHandler<EventArgs> RefreshCompleted;

        internal void RaiseRefreshCompleted()
        {
            if (RefreshCompleted != null)
            {
                RefreshCompleted(this, new EventArgs());
            }
        }

        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected abstract SeriesViewerView CreateView();

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected virtual void OnViewCreated(SeriesViewerView view)
        {

            this.UpdateOverviewPlusDetailPaneControlPanelVisibility();

        }

        #region CrosshairLineStyle Dependency Property

        /// <summary>
        /// Gets or sets the style used to display the current SeriesViewer's crosshair lines.
        /// </summary>
        [SuppressWidgetMember]
        public Style CrosshairLineStyle
        {
            get
            {
                return (Style)GetValue(CrosshairLineStyleProperty);
            }
            set
            {
                SetValue(CrosshairLineStyleProperty, value);
            }
        }

        internal const string CrosshairLineStylePropertyName = "CrosshairLineStyle";

        /// <summary>
        /// Identifies the CrosshairLineStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty CrosshairLineStyleProperty = DependencyProperty.Register(CrosshairLineStylePropertyName, typeof(Style), typeof(SeriesViewer),
            new PropertyMetadata((sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(CrosshairLineStylePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion CrosshairLineStyle Dependency Property

        internal const string IdleVisualStateName = "Idle";
        internal const string DraggingVisualStateName = "Dragging";
        internal const string PanningVisualStateName = "Panning";
        internal const string InkingVisualStateName = "Inking";
        internal const string ErasingVisualStateName = "Erasing";
        #region PreviewPathStyle Property

        /// <summary>
        /// Gets or sets the style used to display the window preview shadow.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public Style PreviewPathStyle
        {
            get
            {
                return (Style)GetValue(PreviewPathStyleProperty);
            }
            set
            {
                SetValue(PreviewPathStyleProperty, value);
            }
        }

        internal const string PreviewPathStylePropertyName = "PreviewPathStyle";

        /// <summary>
        /// Identifies the PreviewPathStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty PreviewPathStyleProperty = DependencyProperty.Register(PreviewPathStylePropertyName, typeof(Style), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(PreviewPathStylePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion PreviewPathStyle Property



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal static readonly Rect StandardRect = Rect.Empty;

        #region ToolTipStyle Dependency Property


        /// <summary>
        /// Gets or sets the current SeriesViewer's ToolTipStyle property.
        /// </summary>
        /// <remarks>
        /// The ToolTipStyle property defines the marker template used for
        /// series with a MarkerStyleType of Circle.
        /// </remarks>
        [SuppressWidgetMember]
        public Style ToolTipStyle
        {
            get
            {
                return (Style)GetValue(ToolTipStyleProperty);
            }
            set
            {
                SetValue(ToolTipStyleProperty, value);
            }
        }

        internal const string ToolTipStylePropertyName = "ToolTipStyle";

        /// <summary>
        /// Identifies the ToolTipStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolTipStyleProperty = DependencyProperty.Register(ToolTipStylePropertyName, typeof(Style), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(ToolTipStylePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion ToolTipStyle Dependency Property
        #region ZoombarStyle Dependency Property

        /// <summary>
        /// Gets or sets the style used to display the current SeriesViewer object's zoom bars.
        /// </summary>
        [SuppressWidgetMember]
        public Style ZoombarStyle
        {
            get
            {
                return (Style)GetValue(ZoombarStyleProperty);
            }
            set
            {
                SetValue(ZoombarStyleProperty, value);
            }
        }

        internal const string ZoombarStylePropertyName = "ZoombarStyle";

        /// <summary>
        /// Identifies the ZoombarStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoombarStyleProperty = DependencyProperty.Register(ZoombarStylePropertyName, typeof(Style), typeof(SeriesViewer),
            new PropertyMetadata((sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(ZoombarStylePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion ZoombarStyle Dependency Property
        #region CircleMarkerTemplate  Dependency Property

        /// <summary>
        /// Gets or sets the template to use for circle markers on the SeriesViewer.
        /// </summary>
        /// <remarks>
        /// Defines the marker template used for
        /// series with a marker type of circle.
        /// </remarks>
        public DataTemplate CircleMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(CircleMarkerTemplateProperty);
            }
            set
            {
                SetValue(CircleMarkerTemplateProperty, value);
            }
        }

        internal const string CircleMarkerTemplatePropertyName = "CircleMarkerTemplate";

        /// <summary>
        /// Identifies the CircleMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty CircleMarkerTemplateProperty = DependencyProperty.Register(CircleMarkerTemplatePropertyName, typeof(DataTemplate), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(CircleMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion CircleMarkerTemplate  Dependency Property

        #region TriangleMarkerTemplate Dependency Property

        /// <summary>
        /// Gets or sets the template to use for triangle markers on the SeriesViewer.
        /// </summary>
        /// <remarks>
        /// Defines the marker template used for
        /// series with a marker type of triangle.
        /// </remarks>
        public DataTemplate TriangleMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(TriangleMarkerTemplateProperty);
            }
            set
            {
                SetValue(TriangleMarkerTemplateProperty, value);
            }
        }

        internal const string TriangleMarkerTemplatePropertyName = "TriangleMarkerTemplate";

        /// <summary>
        /// Identifies the TriangleMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty TriangleMarkerTemplateProperty = DependencyProperty.Register(TriangleMarkerTemplatePropertyName, typeof(DataTemplate), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(TriangleMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion TriangleMarkerTemplate Dependency Property

        #region PyramidMarkerTemplate Dependency Property

        /// <summary>
        /// Gets or sets the template to use for pyramid markers on the SeriesViewer.
        /// </summary>
        /// <remarks>
        /// Defines the marker template used for
        /// series with a marker type of pyramid.
        /// </remarks>
        public DataTemplate PyramidMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(PyramidMarkerTemplateProperty);
            }
            set
            {
                SetValue(PyramidMarkerTemplateProperty, value);
            }
        }

        internal const string PyramidMarkerTemplatePropertyName = "PyramidMarkerTemplate";

        /// <summary>
        /// Identifies the PyramidMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty PyramidMarkerTemplateProperty = DependencyProperty.Register(PyramidMarkerTemplatePropertyName, typeof(DataTemplate), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(PyramidMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion PyramidMarkerTemplate Dependency Property

        #region SquareMarkerTemplate Dependency Property

        /// <summary>
        /// Gets or sets the template to use for square markers on the SeriesViewer.
        /// </summary>
        /// <remarks>
        /// Defines the marker template used for
        /// series with a marker type of square.
        /// </remarks>
        public DataTemplate SquareMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(SquareMarkerTemplateProperty);
            }
            set
            {
                SetValue(SquareMarkerTemplateProperty, value);
            }
        }

        internal const string SquareMarkerTemplatePropertyName = "SquareMarkerTemplate";

        /// <summary>
        /// Identifies the SquareMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty SquareMarkerTemplateProperty = DependencyProperty.Register(SquareMarkerTemplatePropertyName, typeof(DataTemplate), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(SquareMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion SquareMarkerTemplate Dependency Property

        #region DiamondMarkerTemplate Dependency Property

        /// <summary>
        /// Gets or sets the template to use for diamond markers on the SeriesViewer.
        /// </summary>
        /// <remarks>
        /// Defines the marker template used for
        /// series with a marker type of diamond.
        /// </remarks>
        public DataTemplate DiamondMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(DiamondMarkerTemplateProperty);
            }
            set
            {
                SetValue(DiamondMarkerTemplateProperty, value);
            }
        }

        internal const string DiamondMarkerTemplatePropertyName = "DiamondMarkerTemplate";

        /// <summary>
        /// Identifies the DiamondMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty DiamondMarkerTemplateProperty = DependencyProperty.Register(DiamondMarkerTemplatePropertyName, typeof(DataTemplate), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(DiamondMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion DiamondMarkerTemplate Dependency Property

        #region PentagonMarkerTemplate Dependency Property

        /// <summary>
        /// Gets or sets the template to use for pentagon markers on the SeriesViewer.
        /// </summary>
        /// <remarks>
        /// Defines the marker template used for
        /// series with a marker type of pentagon.
        /// </remarks>
        public DataTemplate PentagonMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(PentagonMarkerTemplateProperty);
            }
            set
            {
                SetValue(PentagonMarkerTemplateProperty, value);
            }
        }

        internal const string PentagonMarkerTemplatePropertyName = "PentagonMarkerTemplate";

        /// <summary>
        /// Identifies the PentagonMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty PentagonMarkerTemplateProperty = DependencyProperty.Register(PentagonMarkerTemplatePropertyName, typeof(DataTemplate), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(PentagonMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion PentagonMarkerTemplate Dependency Property

        #region HexagonMarkerTemplate  Dependency Property

        /// <summary>
        /// Gets or sets the template to use for hexagon markers on the SeriesViewer.
        /// </summary>
        /// <remarks>
        /// Defines the marker template used for
        /// series with a marker type of hexagon.
        /// </remarks>
        public DataTemplate HexagonMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(HexagonMarkerTemplateProperty);
            }
            set
            {
                SetValue(HexagonMarkerTemplateProperty, value);
            }
        }

        internal const string HexagonMarkerTemplatePropertyName = "HexagonMarkerTemplate";

        /// <summary>
        /// Identifies the HexagonMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty HexagonMarkerTemplateProperty = DependencyProperty.Register(HexagonMarkerTemplatePropertyName, typeof(DataTemplate), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(HexagonMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion HexagonMarkerTemplate  Dependency Property

        #region TetragramMarkerTemplate Dependency Property

        /// <summary>
        /// Gets or sets the template to use for tetragram markers on the SeriesViewer.
        /// </summary>
        /// <remarks>
        /// Defines the marker template used for
        /// series with a marker type of tetragram.
        /// </remarks>
        public DataTemplate TetragramMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(TetragramMarkerTemplateProperty);
            }
            set
            {
                SetValue(TetragramMarkerTemplateProperty, value);
            }
        }

        internal const string TetragramMarkerTemplatePropertyName = "TetragramMarkerTemplate";

        /// <summary>
        /// Identifies the TetragramMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty TetragramMarkerTemplateProperty = DependencyProperty.Register(TetragramMarkerTemplatePropertyName, typeof(DataTemplate), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(TetragramMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion TetragramMarkerTemplate Dependency Property

        #region PentagramMarkerTemplate Dependency Property

        /// <summary>
        /// Gets or sets the template to use for pentragram markers on the SeriesViewer.
        /// </summary>
        /// <remarks>
        /// Defines the marker template used for
        /// series with a marker type of pentagram.
        /// </remarks>
        public DataTemplate PentagramMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(PentagramMarkerTemplateProperty);
            }
            set
            {
                SetValue(PentagramMarkerTemplateProperty, value);
            }
        }

        internal const string PentagramMarkerTemplatePropertyName = "PentagramMarkerTemplate";

        /// <summary>
        /// Identifies the PentagramMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty PentagramMarkerTemplateProperty = DependencyProperty.Register(PentagramMarkerTemplatePropertyName, typeof(DataTemplate), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(PentagramMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion PentagramMarkerTemplate Dependency Property

        #region HexagramMarkerTemplate Dependency Property

        /// <summary>
        /// Gets or sets the template to use for hexagram markers on the SeriesViewer.
        /// </summary>
        /// <remarks>
        /// Defines the marker template used for
        /// series with a marker type of hexagram.
        /// </remarks>
        public DataTemplate HexagramMarkerTemplate
        {
            get
            {
                return (DataTemplate)GetValue(HexagramMarkerTemplateProperty);
            }
            set
            {
                SetValue(HexagramMarkerTemplateProperty, value);
            }
        }

        internal const string HexagramMarkerTemplatePropertyName = "HexagramMarkerTemplate";

        /// <summary>
        /// Identifies the HexagramMarkerTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty HexagramMarkerTemplateProperty = DependencyProperty.Register(HexagramMarkerTemplatePropertyName, typeof(DataTemplate), typeof(SeriesViewer),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as SeriesViewer).RaisePropertyChanged(HexagramMarkerTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion HexagramMarkerTemplate Dependency Property



#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)




#region Infragistics Source Cleanup (Region)



















































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// The property name of the ActualWindowRect property.
        /// </summary>
        protected const string ActualWindowRectPropertyName = "ActualWindowRect";
        private Rect _actualWindowRect;

        /// <summary>
        /// Gets the actual value of the window rectangle, which represents the current zoom level.
        /// </summary>
        public Rect ActualWindowRect
        {
            get
            {
                return this._actualWindowRect;
            }
            protected set
            {
                bool changed = this._actualWindowRect != value;
                if (changed)
                {
                    object oldValue = this._actualWindowRect;
                    this._actualWindowRect = value;
                    this.RaisePropertyChanged(ActualWindowRectPropertyName, oldValue, value);
                }
            }
        }
        private void InvalidateActualWindowRect()
        {
            this.ActualWindowRect = this.CalculateActualWindowRect();

        }

        /// <summary>
        /// Called to calculate the actual window rectangle.
        /// </summary>
        /// <returns>The value which should be assigned to the ActualWindowRect property, based on a calculation on the WindowRect property.</returns>
        protected virtual Rect CalculateActualWindowRect()
        {
            return new Rect(
                Math.Min(1.0, Math.Max(0.0, this.WindowRect.Left)),
                Math.Min(1.0, Math.Max(0.0, this.WindowRect.Top)),
                Math.Min(1.0, Math.Max(0.0, this.WindowRect.Width)),
                Math.Min(1.0, Math.Max(0.0, this.WindowRect.Height)));
        }

        private double _actualWindowPositionHorizontal = 0.0;
        internal const string ActualWindowPositionHorizontalPropertyName = "ActualWindowPositionHorizontal";
        /// <summary>
        /// A number between 0 and 1 determining the position of the horizontal scroll.
        /// </summary>
        /// <remarks>
        /// This property is effectively a shortcut to the Left of the ActualWindowRect property.
        /// </remarks>
        public double ActualWindowPositionHorizontal
        {
            get
            {
                return _actualWindowPositionHorizontal;
            }
            private set
            {
                double oldValue = _actualWindowPositionHorizontal;
                _actualWindowPositionHorizontal = value;
                RaisePropertyChanged(ActualWindowPositionHorizontalPropertyName, oldValue, _actualWindowPositionHorizontal);
            }
        }

        private double _actualWindowPositionVertical = 0.0;
        internal const string ActualWindowPositionVerticalPropertyName = "ActualWindowPositionVertical";
        /// <summary>
        /// A number between 0 and 1 determining the position of the vertical scroll.
        /// </summary>
        /// <remarks>
        /// This property is effectively a shortcut to the Top of the ActualWindowRect property.
        /// </remarks>
        public double ActualWindowPositionVertical
        {
            get
            {
                return _actualWindowPositionVertical;
            }
            private set
            {
                double oldValue = _actualWindowPositionVertical;
                _actualWindowPositionVertical = value;
                RaisePropertyChanged(ActualWindowPositionVerticalPropertyName, oldValue, _actualWindowPositionVertical);
            }
        }

        private double _actualWindowScaleHorizontal = 1.0;
        internal const string ActualWindowScaleHorizontalPropertyName = "ActualWindowScaleHorizontal";
        /// <summary>
        /// A number between 0 and 1 determining the scale of the horizontal zoom.
        /// </summary>
        /// <remarks>
        /// This property is effectively a shortcut to the Width of the ActualWindowRect property.
        /// </remarks>
        public double ActualWindowScaleHorizontal
        {
            get
            {
                return _actualWindowScaleHorizontal;
            }
            private set
            {
                double oldValue = _actualWindowScaleHorizontal;
                _actualWindowScaleHorizontal = value;
                RaisePropertyChanged(ActualWindowScaleHorizontalPropertyName, oldValue, _actualWindowScaleHorizontal);
            }
        }

        private double _actualWindowScaleVertical = 1.0;
        internal const string ActualWindowScaleVerticalPropertyName = "ActualWindowScaleVertical";
        /// <summary>
        /// A number between 0 and 1 determining the scale of the vertical zoom.
        /// </summary>
        /// <remarks>
        /// This property is effectively a shortcut to the Height of the ActualWindowRect property.
        /// </remarks>
        public double ActualWindowScaleVertical
        {
            get
            {
                return _actualWindowScaleVertical;
            }
            private set
            {
                double oldValue = _actualWindowScaleVertical;
                _actualWindowScaleVertical = value;
                RaisePropertyChanged(ActualWindowScaleVerticalPropertyName, oldValue, _actualWindowScaleVertical);
            }
        }

        internal void NotifyThumbnailDataChanged()
        {


            if (_useOPD)
            {
                ((SeriesViewerSurfaceViewer)OverviewPlusDetailPane.SurfaceViewer).IsDirty = true;
                OverviewPlusDetailPane.Refresh(false);
            }


        }

        internal void NotifyThumbnailAppearanceChanged()
        {


            if (_useOPD)
            {
                ((SeriesViewerSurfaceViewer)OverviewPlusDetailPane.SurfaceViewer).IsDirty = true;
                OverviewPlusDetailPane.Refresh(false);
            }


        }

        /// <summary>
        /// Use to force the SeriesViewer to finish any deferred work before printing or evaluating its visual.
        /// </summary>
        /// <remarks>
        /// This should only be called if the visual of the SeriesViewer needs to be synchronously saved or evaluated. 
        /// Calling this method too often will hinder the performance of the SeriesViewer.
        /// </remarks>
        public void Flush()
        {
            ChartContentManager.Force();
            View.EnsurePanelsArranged();
            ChartContentManager.Force();



        }



#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Boolean method indicating whether or not this SeriesViewer should be zoomed only with a fixed aspect ratio.
        /// </summary>
        /// <returns>False.</returns>
        public virtual bool UseFixedAspectZoom()
        {
            return false;
        }

        private DependencyObject _background;

        [Weak]
        private ContentInfo _backgroundContentInfo;
        /// <summary>
        /// Registers the given DependencyObject as background content.
        /// </summary>
        /// <param name="content">The DependencyObject to register as background content.</param>
        /// <param name="refresh">An action to be invoked each time the background content is being refreshed.</param>
        protected void RegisterBackgroundContent(DependencyObject content, Action<bool> refresh)
        {
            if (_background != null)
            {
                UnRegisterBackgroundContent(_background);
                _background = null;
                _backgroundContentInfo = null;
            }
            _background = content;
            _backgroundContentInfo = ChartContentManager.Subscribe(ChartContentType.Background, content, refresh);
        }
        /// <summary>
        /// Unregisters the given DependencyObject as background content.
        /// </summary>
        /// <param name="content">The DependencyObject to unregister as background content.</param>
        protected void UnRegisterBackgroundContent(DependencyObject content)
        {
            ChartContentManager.Unsubscribe(ChartContentType.Background, content);
            _background = null;
            _backgroundContentInfo = null;
        }
        /// <summary>
        /// Calls for a deferred refresh to the SeriesViewer's background.
        /// </summary>
        protected void DeferBackgroundRefresh()
        {
            ChartContentManager.Refresh(ChartContentType.Background, _background, _backgroundContentInfo, false);
        }

        private List<FrameworkElement> _logicalChildrenInternal;

        private IEnumerator LogicalChildrenInternal()
        {
            if (_logicalChildrenInternal == null)
                _logicalChildrenInternal = new List<FrameworkElement> { CentralArea };
            return _logicalChildrenInternal.GetEnumerator();
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (CentralArea != null)
                {
                    return LogicalChildrenInternal();
                }

                return base.LogicalChildren;
            }
        }




#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

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