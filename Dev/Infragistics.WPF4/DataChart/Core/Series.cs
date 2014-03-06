using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Infragistics.Controls.Charts.Util;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;



namespace Infragistics.Controls.Charts
{


    /// <summary>
    /// Represents the base class for all XamDataChart series.
    /// </summary>
    [TemplatePart(Name = Series.RootCanvasName, Type = typeof(Canvas))]

    [TemplateVisualState(Name = Series.MouseOverVisualStateName, GroupName = "CommonStates")]

    [TemplateVisualState(Name = Series.NormalVisualStateName, GroupName = "CommonStates")]

    [DesignTimeVisible(false)]

    [Widget]
    [WidgetModule("ChartCore")]
    [WidgetIgnoreDepends("XamDataChart")]
    public abstract class Series : Control, INotifyPropertyChanged, IProvidesViewport
    {

        internal const string MouseOverVisualStateName = "MouseOver";

        internal const string NormalVisualStateName = "Normal";

        /// <summary>
        /// Gets or sets the view associated with the series.
        /// </summary>
        [SuppressWidgetMember]

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        public SeriesView View { get; set; }

        #region Constructor and Template Parts
        /// <summary>
        /// Initializes a new instance of the Series class. 
        /// </summary>
        internal protected Series()
        {
            ThumbnailDirty = true;

            View = CreateView();
            View.Viewport = Rect.Empty;
            
            OnViewCreated(View);
            View.OnInit();

            DefaultStyleKey = typeof(Series);

            SeriesViewer_WindowRectChanged = (o, e) => { WindowRectChangedOverride(e.OldRect, e.NewRect); };
            SeriesViewer_PropertyUpdated = (o, e) => { PropertyUpdatedOverride(o, e.PropertyName, e.OldValue, e.NewValue); };
            //Chart_SeriesCollectionChanged = (o, e) => { Index = XamDataChart.FindSeriesIndex(this); };
            FastItemsSource_Event = (o, e) => 
            {
                if (SeriesViewer != null)
                {
                    ThumbnailDirty = true;
                    SeriesViewer.NotifyThumbnailDataChanged();
                }
                DataUpdatedOverride(e.Action, e.Position, e.Count, e.PropertyName); 
            };

            PropertyUpdated += (o, e) => { PropertyUpdatedOverride(o, e.PropertyName, e.OldValue, e.NewValue); };






            // [DN February 14, 2012 : 98613] we really need to know when Visibility changes, so here's this hack...
            this.SetBinding(Series.VisibilityProxyProperty, new Binding("Visibility") { Source = this });


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        }

        /// </summary>
        protected const string VisibilityProxyPropertyName = "VisibilityProxy";

        /// <summary>
        /// Identifies the VisibilityProxy dependency property.
        /// </summary>
        private static readonly DependencyProperty VisibilityProxyProperty = DependencyProperty.Register(VisibilityProxyPropertyName, typeof(Visibility), typeof(Series), new PropertyMetadata((sender, e) =>
        {
            ((Series)sender).RaisePropertyChanged(VisibilityProxyPropertyName, e.OldValue, e.NewValue);
        }));












        
        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal virtual void OnViewCreated(SeriesView view)
        {

        }

        internal void OnSeriesAttached()
        {
            AssertLegendItems(ActualLegend, ActualLegend);
            if (FastItemsSource == null &&
                SyncLink != null &&
                ItemsSource != null)
            {
                FastItemsSource = (SyncLink as IFastItemsSourceProvider)
                    .GetFastItemsSource(ItemsSource);
            }
        }

        internal void OnSeriesDetached()
        {
            ClearLegendItems();
            if (FastItemsSource != null &&
                SyncLink != null &&
                ItemsSource != null)
            {
                FastItemsSource = (SyncLink as IFastItemsSourceProvider)
                    .ReleaseFastItemsSource(ItemsSource);
            }
            View.OnSeriesDetached();
        }

        //internal bool ItemsSourceReady
        //{
        //    get
        //    {
        //        return FastItemsSource != null
        //            && SyncLink != null
        //            && ItemsSource != null;
        //    }
        //}

       
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected virtual SeriesView CreateView()
        {
            return new SeriesView(this);
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes
        /// call ApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RootCanvas = GetTemplateChild(RootCanvasName) as Canvas;
            View.OnTemplateProvided();
        }


        private const string RootCanvasName = "RootCanvas";

        /// <summary>
        /// Gets the root canvas for the current series object.
        /// </summary>
        public Canvas RootCanvas { get ; private set; }
        
        /// <summary>
        /// Determines if the current series renders its markers to the MarkerCanvas of the parent series. 
        /// Ensures that StackedFragmentSeries don't reparent the MarkerCanvas.
        /// </summary>
        /// <returns>Whether or not to use parent series marker canvas.</returns>
        protected internal virtual bool UseParentMarkerCanvas()
        {
            return false;
        }

        #endregion

        /// <summary>
        /// Gets the item item index associated with the specified world position
        /// </summary>
        /// <returns>Item index or -1 if no item is assocated with the specified world position.</returns>
        protected virtual int GetItemIndex(Point world)
        {
            return -1;
        }

        /// <summary>
        /// Gets the item that is the best match for the specified world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The item that is the best match.</returns>
        protected virtual object GetItem(Point world)
        {
            return null;
        }

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Occurs when a property (including "effective" and non-dependency property) value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when a property (including "effective" and non-dependency property) value changes.
        /// </summary>
        public event PropertyUpdatedEventHandler PropertyUpdated;

        /// <summary>
        /// Raises PropertyChanged and/or PropertyUpdated events if any listeners have been registered.
        /// </summary>
        /// <param name="propertyName">Name of property whos value changed.</param>
        /// <param name="oldValue">Property value before change.</param>
        /// <param name="newValue">Property value after change.</param>
        protected void RaisePropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            if (PropertyUpdated != null)
            {
                PropertyUpdated(this, new PropertyUpdatedEventArgs(propertyName, oldValue, newValue));
            }
        }
        #endregion

        /// <summary>
        /// When overridden in a derived class gives the opportunity to define how the data source item
        /// for a given set of mouse coordinates is fetched.
        /// </summary>
        /// <param name="sender">The element the mouse is over.</param>
        /// <param name="point">The mouse coordinates for which to fetch the item.</param>
        /// <returns>The retrieved item.</returns>
        protected internal virtual object Item(object sender, Point point)
        {
            DataContext dataContext = View.GetDataContextFromSender(sender);
            object item = dataContext != null ? dataContext.Item : null;

            if (item == null)
            {
                Rect viewportRect = this.View.Viewport;
                Rect windowRect = this.View.WindowRect;
                Point world = new Point(
                    windowRect.Left + windowRect.Width * (point.X - viewportRect.Left) / viewportRect.Width,
                    windowRect.Top + windowRect.Height * (point.Y - viewportRect.Top) / viewportRect.Height);

                item = GetItem(world);
            }

            return item;
        }

        #region Overridable functions to handle and do the various things that categorySeries handle and do

        /// <summary>
        /// Requests that the provided item should be brought into view if possible.
        /// </summary>
        /// <param name="item">The item to attempt to bring into view.</param>
        public virtual bool ScrollIntoView(object item)
        {
            return false;
        }

        /// <summary>
        /// Returns the range requirement of this series on the given axis.
        /// </summary>
        /// <param name="axis">The axis for which to provide the range requirement.</param>
        /// <returns>The axis range needed for this series to fully display, or null, if there is no range requirement.</returns>
        protected internal virtual AxisRange GetRange(Axis axis)
        {
            return new AxisRange(double.NaN, double.NaN);
        }

        /// <summary>
        /// Renders the series.
        /// </summary>
        /// <param name="animate">True if the change should be animated.</param>
        public virtual void RenderSeries(bool animate)
        {
            if (SeriesViewer != null)
            {
                SeriesViewer.ChartContentManager.Refresh(
                    ChartContentType.Series,
                    this,
                    ContentInfo,
                    animate);
            }
        }

        private void DoRenderSeries(bool animate)
        {
            RenderSeriesOverride(animate);
        }
        /// <summary>
        /// Renders the series.
        /// </summary>
        /// <param name="animate">Whether or not to transition smoothly from the previous state of the series.</param>
        /// <remarks>
        /// The animate parameter is relevant only for series which implement transitions.  Not all series in the DataChart support this behavior.
        /// </remarks>
        protected internal virtual void RenderSeriesOverride(bool animate)
        {
            
        }

        internal SeriesView ThumbnailView { get; private set; }
        
        /// <summary>
        /// Renders the thumbnail for the OPD pane.
        /// </summary>
        /// <param name="viewportRect">The viewport to use.</param>
        /// <param name="surface">The render target.</param>
        protected internal virtual void RenderThumbnail(Rect viewportRect, RenderSurface surface)
        {
            if (this.ThumbnailView == null)
            {
                this.ThumbnailView = this.CreateView();
                this.ThumbnailView.IsThumbnailView = true;






                this.ThumbnailView.OnInit();
            }
            this.ThumbnailView.Viewport = viewportRect;
        }

        /// <summary>
        /// Gets the view info for the series.
        /// </summary>
        /// <param name="viewportRect">The viewport to use.</param>
        /// <param name="windowRect">The window to use.</param>
        protected internal virtual void GetViewInfo(out Rect viewportRect, out Rect windowRect)
        {
            viewportRect = View.Viewport;
            windowRect = View.WindowRect;

            // fix - check this
            //if (SeriesViewer != null)
            //{
            //    windowRect = SeriesViewer.ActualWindowRect;
            //}
            //else
            //{
            //    windowRect = Rect.Empty;
            //}
        }


        /// <summary>
        /// Checks if the series is valid to be rendered.
        /// </summary>
        /// <param name="viewportRect">The current viewport, a rectangle with bounds equivalent to the screen size of the series.</param>
        /// <param name="windowRect">The current window, a rectangle bounded between 0 and 1 representing the pan and zoom position.</param>
        /// <returns>True if the series is valid to be rendered, otherwise false.</returns>
        protected internal virtual bool ValidateSeries(Rect viewportRect, Rect windowRect)
        {
            return ValidateSeries(viewportRect, windowRect, View);
        }


        /// <summary>
        /// Checks if the series is valid to be rendered.
        /// </summary>
        /// <param name="viewportRect">The current viewport, a rectangle with bounds equivalent to the screen size of the series.</param>
        /// <param name="windowRect">The current window, a rectangle bounded between 0 and 1 representing the pan and zoom position.</param>
        /// <param name="view">The SeriesView in context.</param>
        /// <returns>True if the series is valid to be rendered, otherwise false.</returns>
        protected internal virtual bool ValidateSeries(Rect viewportRect, Rect windowRect, SeriesView view)
        {
            //Collapsed Visibility shouldn't affect fragment series and should be equivalent of setting a transparent brush

            if (this is SplineFragmentBase || this is FragmentBase)
            {
                return true;
            }


            return this.Visibility == Visibility.Visible;
        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        protected internal virtual void ClearRendering(bool wipeClean)
        {
            this.ClearRendering(wipeClean, this.View);
        }
        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal virtual void ClearRendering(bool wipeClean, SeriesView view)
        {
            if (wipeClean)
            {
                ThumbnailDirty = true;
            }
        }

        /// <summary>
        /// Clears the series and aborts rendering if the series is not valid.
        /// </summary>
        /// <returns>True if rendering should be aborted.</returns>
        protected virtual bool ClearAndAbortIfInvalid()
        {
            return ClearAndAbortIfInvalid(View);
        }

        /// <summary>
        /// Clears the series and aborts rendering if the series is not valid.
        /// </summary>
        /// <returns>True if rendering should be aborted.</returns>
        protected virtual bool ClearAndAbortIfInvalid(SeriesView view)
        {
            Rect viewportRect = view.Viewport;
            Rect windowRect = view.WindowRect;
            if (!ValidateSeries(viewportRect, windowRect, view))
            {
                ClearRendering(true, view);
                return true;
            }
            return false;
        }

        internal void OnViewportChanged(Rect oldViewportRect, Rect newViewportRect)
        {
            View.Viewport = newViewportRect;
            ViewportRectChangedOverride(oldViewportRect, newViewportRect);
            if (SeriesViewer != null)
            {
                SeriesViewer.ChartContentManager.ViewportChanged(
                    ChartContentType.Series,
                    this,
                    ContentInfo,
                    newViewportRect);
            }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever the series window rectangle 
        /// is changed.
        /// </summary>
        /// <param name="oldWindowRect">Old window rectangle in normalised world coordinates.</param>
        /// <param name="newWindowRect">New window rectangle in normalised world coordinates.</param>
        protected virtual void WindowRectChangedOverride(Rect oldWindowRect, Rect newWindowRect)
        {
            
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever the series viewport rectangle 
        /// is changed.
        /// </summary>
        /// <param name="oldViewportRect">Old viewport rectangle in device coordinates.</param>
        /// <param name="newViewportRect">New viewport rectangle in device coordinates.</param>
        protected virtual void ViewportRectChangedOverride(Rect oldViewportRect, Rect newViewportRect)
        {
           
        }

        [Weak]
        internal ContentInfo ContentInfo { get; set; }

        private bool _thumbnailDirty = true;
        protected internal bool ThumbnailDirty
        {
            get
            {
                return _thumbnailDirty;
            }
            set
            {
                _thumbnailDirty = value;
            }
        }

        /// <summary>
        /// Called to notify that the data has changed and the thumbnail may need to refresh.
        /// </summary>
        protected void NotifyThumbnailDataChanged()
        {
            ThumbnailDirty = true;
            if (SeriesViewer != null)
            {
                SeriesViewer.NotifyThumbnailDataChanged();
            }
        }

        /// <summary>
        /// Called to notify that the appearance has changed and the thumbnail may need to refresh
        /// </summary>
        protected internal void NotifyThumbnailAppearanceChanged()
        {
            ThumbnailDirty = true;
            if (SeriesViewer != null)
            {
                SeriesViewer.NotifyThumbnailAppearanceChanged();
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected virtual void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            switch (propertyName)
            {
                #region SyncManager Properties
                case Series.SyncLinkPropertyName:
                    if (oldValue as SyncLink != null)
                    {
                        (oldValue as SyncLink).PropertyUpdated -= SeriesViewer_PropertyUpdated;
                        ReleaseItemsSource(oldValue as IFastItemsSourceProvider);
                    }

                    if (newValue as SyncLink != null)
                    {
                        (newValue as SyncLink).PropertyUpdated += SeriesViewer_PropertyUpdated;
                        RegisterItemsSource(newValue as IFastItemsSourceProvider);
                    }

                    if (Index == -1)
                    {
                        Index = XamDataChart.FindSeriesIndex(this);
                    }
                    break;

                case XamDataChart.BrushesPropertyName:
                case XamDataChart.MarkerOutlinesPropertyName:
                case XamDataChart.MarkerBrushesPropertyName:
                case XamDataChart.OutlinesPropertyName:
                    DoUpdateIndexedProperties();
                    break;
                #endregion

                #region Chart and stuff off chart area
                case Series.SeriesViewerPropertyName:
                    if (oldValue as SeriesViewer != null)
                    {
                        (oldValue as SeriesViewer).WindowRectChanged -= SeriesViewer_WindowRectChanged;
                        (oldValue as SeriesViewer).Series.CollectionChanged -= SeriesViewer_SeriesCollectionChanged;
                        (oldValue as SeriesViewer).PropertyUpdated -= SeriesViewer_PropertyUpdated;
                        (oldValue as SeriesViewer).ChartContentManager.Unsubscribe(ChartContentType.Series, this);
                        View.DetachFromChart((SeriesViewer)oldValue);
                    }

                    if (newValue as SeriesViewer != null)
                    {
                        (newValue as SeriesViewer).WindowRectChanged += SeriesViewer_WindowRectChanged;
                        (newValue as SeriesViewer).Series.CollectionChanged += SeriesViewer_SeriesCollectionChanged;
                        (newValue as SeriesViewer).PropertyUpdated += SeriesViewer_PropertyUpdated;
                        ContentInfo = (newValue as SeriesViewer).ChartContentManager.Subscribe(ChartContentType.Series, this, DoRenderSeries);
                        View.AttachToChart((SeriesViewer)newValue);
                    }

                    if (Index == -1 || newValue == null)
                    {
                        Index = XamDataChart.FindSeriesIndex(this);
                    }
                    ActualLegend = FindActualLegend();

                    AssertLegendItems(oldValue as XamDataChart, newValue as XamDataChart);

                    break;

                #endregion

                case Series.TitlePropertyName:
                    if (this.View != null)
                    {
                        this.View.OnTitlePropertyChanged();
                    }
                    break;



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


                case Series.ItemsSourcePropertyName:
                    if (SyncLink != null)
                    {
                        RegisterItemsSource(SyncLink);
                    }
                    break;

                case Series.FastItemsSourcePropertyName:
                    NotifyThumbnailDataChanged();
                    if (oldValue as FastItemsSource != null)
                    {
                        (oldValue as FastItemsSource).Event -= FastItemsSource_Event;
                    }

                    if (newValue as FastItemsSource != null)
                    {
                        (newValue as FastItemsSource).Event += FastItemsSource_Event;
                    }
                    break;

                case XamDataChart.LegendPropertyName:
                    if (newValue != null)
                    {
                        if (sender == this)
                        {
                            (newValue as LegendBase).SeriesOwner = this;
                        }
                        else if (sender is XamDataChart)
                        {
                            (newValue as LegendBase).ChartOwner = sender;
                        }
                    }
                    ActualLegend = FindActualLegend();
                    break;

                case Series.ActualLegendPropertyName:
                    AssertLegendItems(oldValue as LegendBase, newValue as LegendBase);
                    break;

                case Series.LegendItemPropertyName:
                    AssertLegendItems(oldValue as Control, newValue as Control);
                    break;

                case Series.LegendItemVisibilityPropertyName:
                    View.OnLegendItemVisibilityChanged();
                    break;

                case Series.ToolTipPropertyName:
                    View.UpdateToolTipValue(ToolTip);
                    break;
                

                case Series.IndexPropertyName:
                case Series.BrushPropertyName:
                case Series.OutlinePropertyName:
                    DoUpdateIndexedProperties();
                    break;

                case XamDataChart.CrosshairPointPropertyName:

                    if (SeriesViewer != null)
                    {
                        SeriesViewer.RaiseSeriesCursorMouseMove(
                            this, GetItem(SeriesViewer.CrosshairPoint));
                    }

                    break;
                case Series.ResolutionPropertyName:
                    this.RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
                case Series.ThicknessPropertyName:
                    
                    RenderSeries(false);
                    break;
                case Series.TransitionDurationPropertyName:
                case Series.TransitionEasingFunctionPropertyName:
                    FunctionOrDurationUpdated = true;



                    break;

                case Series.DiscreteLegendItemTemplatePropertyName:
                    RenderSeries(false);
                    break;
                case VisibilityProxyPropertyName:
                    if ((Visibility)oldValue != Visibility.Visible && (Visibility)newValue == Visibility.Visible)
                    {
                        this.RenderSeries(false);
                    }
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }
        #endregion

        #region SyncLink
        /// <summary>
        /// The name of the SyncLink property.
        /// </summary>
        protected internal const string SyncLinkPropertyName = "SyncLink";

        /// <summary>
        /// Sets or sets the synchronization link associated with the series.
        /// </summary>
        [SuppressWidgetMember]
        public SyncLink SyncLink
        {
            get { return syncLink; }
            set
            {
                if (SyncLink != value)
                {
                    SyncLink oldDataChart = SyncLink;

                    syncLink = value;
                    RaisePropertyChanged(SyncLinkPropertyName, oldDataChart, SyncLink);
                }
            }
        }
        private SyncLink syncLink;
        #endregion

        #region Chart Event Handlers
        /// <summary>
        /// The name of the SeriesViewer property.
        /// </summary>
        protected internal const string SeriesViewerPropertyName = "SeriesViewer";
        /// <summary>
        /// Gets the Chart for the current chart series object.
        /// </summary>
        /// <remarks>
        /// The chart property is maintained internally by the series and may 
        /// lag behind the visual hierarchy defined in xaml or code.
        /// </remarks>
        [SuppressWidgetMember]
        public SeriesViewer SeriesViewer
        {
            get { return _seriesViewer; }
            set
            {
                if (SeriesViewer != value)
                {
                    SeriesViewer oldChart = SeriesViewer;

                    _seriesViewer = value;
                    RaisePropertyChanged(SeriesViewerPropertyName, oldChart, _seriesViewer);



                }
            }
        }

        [Weak]
        private SeriesViewer _seriesViewer;

        /// <summary>
        /// Gets the chart associated with the series.
        /// </summary>

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Obsolete("Please use the SeriesViewer property.")]

        [SuppressWidgetMember]
        public XamDataChart Chart { get { return this.SeriesViewer as XamDataChart; } set { this.SeriesViewer = value; } }

        /// <summary>
        /// Listener for the owner chart's series collection.
        /// </summary>
        internal readonly NotifyCollectionChangedEventHandler SeriesViewer_SeriesCollectionChanged;

        /// <summary>
        /// Listener for the owner chart's property updated.
        /// </summary>
        internal readonly PropertyUpdatedEventHandler SeriesViewer_PropertyUpdated;

        /// <summary>
        /// Listener for the owner chart's window rect changed
        /// </summary>
        internal readonly RectChangedEventHandler SeriesViewer_WindowRectChanged;
        #endregion

        #region ItemsSource
        /// <summary>
        /// Gets or sets the ItemsSource property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        public



            System.Collections.IEnumerable 

            ItemsSource
        {
            get
            {
                return (
                    


            System.Collections.IEnumerable

                     )GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        /// <summary>
        /// The name of the ItemsSource property.
        /// </summary>
        protected internal const string ItemsSourcePropertyName = "ItemsSource";

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(ItemsSourcePropertyName, typeof(System.Collections.IEnumerable), typeof(Series), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Series series = sender as Series;

            if (series.SyncLink != null)
            {
                (series.SyncLink as IFastItemsSourceProvider)
                    .ReleaseFastItemsSource(



                    (IEnumerable) 


                    e.OldValue
                    );
            }

            (sender as Series).RaisePropertyChanged(ItemsSourcePropertyName, e.OldValue, e.NewValue);
        }));

        #endregion

        #region FastItemsSource and FastItemsSource Event Handlers
        /// <summary>
        /// Gets or sets the ItemsSource property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <para>
        /// The FastItemsSource is a proxy which handles caching, conversion and
        /// weak event listeners.
        /// </para>
        protected internal FastItemsSource FastItemsSource
        {
            get
            {
                return (FastItemsSource)GetValue(FastItemsSourceProperty);
            }
            set
            {
                SetValue(FastItemsSourceProperty, value);
            }
        }

        /// <summary>
        /// The name of the FastItemsSource property.
        /// </summary>
        protected internal const string FastItemsSourcePropertyName = "FastItemsSource";

        /// <summary>
        /// Identifies the FastItemsSource dependency property.
        /// </summary>
        protected internal static readonly DependencyProperty FastItemsSourceProperty = DependencyProperty.Register(FastItemsSourcePropertyName, typeof(FastItemsSource), typeof(Series),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(FastItemsSourcePropertyName, e.OldValue, e.NewValue);
            }));

        internal EventHandler<FastItemsSourceEventArgs> FastItemsSource_Event;

       

        /// <summary>
        /// Method called whenever a change is made to the series data.
        /// </summary>
        /// <param name="action">The action performed on the data</param>
        /// <param name="position">The index of the first item involved in the update.</param>
        /// <param name="count">The number of items in the update.</param>
        /// <param name="propertyName">The name of the updated property.</param>
        protected virtual void DataUpdatedOverride(FastItemsSourceEventAction action, int position, int count, string propertyName)
        {
           
        }

        #endregion

        #region Legend, ActualLegend and LegendItem
        /// <summary>
        /// Gets or sets a legend for the current series object.
        /// </summary>
        /// <remarks>
        /// If the Legend property is not set, the series will use the setting according to the parent XamDataChart's Legend property.
        /// <para>
        /// This is generally expressed as an element name binding, as the Legend must exist at some other position in the layout. 
        /// This property only indicates which Legend to use and will not alone cause the legend to be visible.
        /// </para>
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

        /// <summary>
        /// Identifies the Legend dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendProperty = DependencyProperty.Register("Legend", typeof(LegendBase), typeof(Series),
            new PropertyMetadata(null, (sender, e) =>
            {
                Series series = sender as Series;

                series.RaisePropertyChanged(XamDataChart.LegendPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets the legend item associated with the current series object.
        /// </summary>
        /// <remarks>
        /// The legend item control is created according to the LegendItemTemplate on-demand by 
        /// the series object itself.
        /// </remarks>
        public Control LegendItem
        {
            get
            {
                return legendItem;
            }
            private set
            {
                if (LegendItem != value)
                {
                    Control oldLegendItem = LegendItem;

                    legendItem = value;

                    RaisePropertyChanged(LegendItemPropertyName, oldLegendItem, legendItem);
                }
            }
        }
        private Control legendItem;
        internal const string LegendItemPropertyName = "LegendItem";

        internal object ProvideLegendItem()
        {
            return GetLegendItem();
        }

        /// <summary>
        /// Gets the legend item control to use.
        /// </summary>
        /// <returns>The legend item control to use.</returns>
        protected virtual Control GetLegendItem()
        {
            ContentControl legendItem = LegendItem as ContentControl;
            DataTemplate dataTemplate = LegendItemTemplate;

            if (dataTemplate != null)
            {
                if (legendItem == null)
                {
                    legendItem = new ContentControl();

                    legendItem.SetBinding(ContentControl.VisibilityProperty, new Binding("LegendItemVisibility") { Source = this });
                    // [DN 3/18/2010] todo: see if binding the foreground property is still necessary in Silverlight 4.  this should be automatic, but isn't, so we'll hook it up right now.
                    legendItem.SetBinding(ContentControl.ForegroundProperty, new Binding("ActualLegend.Foreground") { Source = this });

                }

                legendItem.Content = new DataContext() { Series = this, Item = null };
                legendItem.ContentTemplate = dataTemplate;
                //legendItem.Content = dataTemplate.LoadContent();
            }
            else
            {
                legendItem = null;
            }

            return legendItem;
        }

        internal const string ActualLegendPropertyName = "ActualLegend";

        /// <summary>
        /// Gets the effective legend associated with the current series object.
        /// </summary>



        public LegendBase ActualLegend
        {
            get { return actualLegend; }
            internal set
            {
                if (ActualLegend != value)
                {
                    LegendBase oldActualLegend = actualLegend;

                    actualLegend = value;

                    RaisePropertyChanged(ActualLegendPropertyName, oldActualLegend, actualLegend);
                }
            }
        }
        private LegendBase actualLegend;

        #endregion

        #region LegendItemVisibility Dependency Property
        /// <summary>
        /// Gets or sets the legend item visibility for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [WidgetDefaultString("visible")]
        public Visibility LegendItemVisibility
        {
            get
            {
                return (Visibility)GetValue(LegendItemVisibilityProperty);
            }
            set
            {
                SetValue(LegendItemVisibilityProperty, value);
            }
        }

        internal const string LegendItemVisibilityPropertyName = "LegendItemVisibility";

        /// <summary>
        /// Identifies the LegendItemVisibility Dependency Property.
        /// </summary>
        public static readonly DependencyProperty LegendItemVisibilityProperty = DependencyProperty.Register(LegendItemVisibilityPropertyName, typeof(Visibility), typeof(Series),
            new PropertyMetadata(Visibility.Visible, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(LegendItemVisibilityPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion

        #region LegendItemBadgeTemplate Dependency Property
        /// <summary>
        /// Gets or sets the LegendItemBadgeTemplate property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The legend item badge is created according to the LegendItemBadgeTemplate on-demand by 
        /// the series object itself.
        /// </remarks>
        public DataTemplate LegendItemBadgeTemplate
        {
            get
            {
                return (DataTemplate)GetValue(LegendItemBadgeTemplateProperty);
            }
            set
            {
                SetValue(LegendItemBadgeTemplateProperty, value);
            }
        }

        internal const string LegendItemBadgeTemplatePropertyName = "LegendItemBadgeTemplate";

        /// <summary>
        /// Identifies the LegendItemBadgeTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendItemBadgeTemplateProperty = DependencyProperty.Register(LegendItemBadgeTemplatePropertyName, typeof(DataTemplate), typeof(Series),
            new PropertyMetadata(null, (sender, e) =>
            {
                Series series = sender as Series;

                series.RaisePropertyChanged(LegendItemBadgeTemplatePropertyName, e.OldValue, e.NewValue);
                series.LegendItem = series.GetLegendItem();
            }));
        #endregion

        #region LegendItemTemplate Dependency Property
        /// <summary>
        /// Gets or sets the LegendItemTemplate property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The legend item control content is created according to the LegendItemTemplate on-demand by 
        /// the series object itself.
        /// </remarks>
        public DataTemplate LegendItemTemplate
        {
            get
            {
                return (DataTemplate)GetValue(LegendItemTemplateProperty);
            }
            set
            {
                SetValue(LegendItemTemplateProperty, value);
            }
        }

        internal const string LegendItemTemplatePropertyName = "LegendItemTemplate";

        /// <summary>
        /// Identifies the LegendItemTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendItemTemplateProperty = DependencyProperty.Register(LegendItemTemplatePropertyName, typeof(DataTemplate), typeof(Series),
            new PropertyMetadata(null, (sender, e) =>
            {
                Series series = sender as Series;

                series.RaisePropertyChanged(LegendItemTemplatePropertyName, e.OldValue, e.NewValue);
                series.LegendItem = series.GetLegendItem();
            }));
        #endregion

        #region DiscreteLegendItemTemplate Dependency Property
        /// <summary>
        /// Gets or sets the DiscreteLegendItemTemplate property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The legend item control content is created according to the DiscreteLegendItemTemplate on-demand by 
        /// the series object itself.
        /// </remarks>
        public DataTemplate DiscreteLegendItemTemplate
        {
            get
            {
                return (DataTemplate)GetValue(DiscreteLegendItemTemplateProperty);
            }
            set
            {
                SetValue(DiscreteLegendItemTemplateProperty, value);
            }
        }

        internal const string DiscreteLegendItemTemplatePropertyName = "DiscreteLegendItemTemplate";

        /// <summary>
        /// Identifies the DiscreteLegendItemTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty DiscreteLegendItemTemplateProperty = DependencyProperty.Register(DiscreteLegendItemTemplatePropertyName, typeof(DataTemplate), typeof(Series),
            new PropertyMetadata(null, (sender, e) =>
            {
                Series series = sender as Series;

                series.RaisePropertyChanged(DiscreteLegendItemTemplatePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region Index and Indexed Properties
        /// <summary>
        /// Gets the Index property.
        /// </summary>
        /// <remarks>
        /// This property is used to support the XamDataChart infrastructure, and is not intended to be set by application code.
        /// </remarks>
        [SuppressWidgetMember]
        public int Index
        {
            get
            {
                return (int)GetValue(IndexProperty);
            }
            set
            {
                SetValue(IndexProperty, value);
            }
        }

        /// <summary>
        /// The name of the Index property.
        /// </summary>
        protected internal const string IndexPropertyName = "Index";

        /// <summary>
        /// Identifies the Index dependency property.
        /// </summary>
        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(IndexPropertyName, typeof(int), typeof(Series),
            new PropertyMetadata(-1, (sender, e) =>
            {
                Series series = sender as Series;

                series.RaisePropertyChanged(IndexPropertyName, e.OldValue, e.NewValue);
                series.LegendItem = series.GetLegendItem();
            }));

        internal void UpdateSeriesIndexedPropertiesInternal()
        {
            UpdateIndexedProperties();
        }

        /// <summary>
        /// Makes sure the indexed properties are updated.
        /// </summary>
        protected internal void DoUpdateIndexedProperties()
        {
            UpdateIndexedProperties();
        }

        /// <summary>
        /// Updates properties that are based on the index of the series in the series collection.
        /// </summary>
        protected virtual void UpdateIndexedProperties()
        {
            NotifyThumbnailAppearanceChanged();
            if (Index < 0)
            {
                return;
            }

            #region ActualBrush
            if (Brush != null)
            {
                View.ResetActualBrush();
                View.BindActualToUserBrush();
            }
            else
            {
                this.ActualBrush = this.SeriesViewer == null ? null : this.SeriesViewer.GetBrushByIndex(this.Index);
            }
            #endregion

            #region ActualOutline
            //          if (ReadLocalValue(OutlineProperty) != DependencyProperty.UnsetValue) // doesn't detect values set in a style
            if (Outline != null)
            {
                View.ResetActualOutlineBrush();
                View.BindActualToUserOutline();
            }
            else
            {
                this.ActualOutline = this.SeriesViewer == null ? null : this.SeriesViewer.GetOutlineByIndex(this.Index);
            }
            #endregion
        }

        #endregion

        #region TransitionEasingFunction, TransitionDuration and TransitionProgress Dependency Properties

        /// <summary>
        /// Gets or sets the EasingFunction used to morph the current series.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public EasingFunctionBase TransitionEasingFunction
        {
            get
            {
                return (EasingFunctionBase)GetValue(TransitionEasingFunctionProperty);
            }
            set
            {
                SetValue(TransitionEasingFunctionProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TransitionEasingFunction dependency property.
        /// </summary>
        public static readonly DependencyProperty TransitionEasingFunctionProperty = DependencyProperty.Register(TransitionEasingFunctionPropertyName, typeof(EasingFunctionBase), typeof(Series),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(TransitionEasingFunctionPropertyName, e.OldValue, e.NewValue);
            }));


        internal const string TransitionEasingFunctionPropertyName = "TransitionEasingFunction";
        internal bool FunctionOrDurationUpdated { get; set; }



#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Gets or sets the duration of the current series's morph.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public TimeSpan TransitionDuration
        {
            get
            {
                return (TimeSpan)GetValue(TransitionDurationProperty);
            }
            set
            {
                SetValue(TransitionDurationProperty, value);
            }
        }

        protected internal const string TransitionDurationPropertyName = "TransitionDuration";

        /// <summary>
        /// Identifies the TransitionDuration dependency property.
        /// </summary>
        public static readonly DependencyProperty TransitionDurationProperty = DependencyProperty.Register(TransitionDurationPropertyName, typeof(TimeSpan), typeof(Series),
            new PropertyMetadata(TimeSpan.FromMilliseconds(0), (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(TransitionDurationPropertyName, e.OldValue, e.NewValue);
            }));


        internal double TransitionProgress
        {
            get
            {
                return (double)GetValue(TransitionProgressProperty);
            }
            set
            {
                SetValue(TransitionProgressProperty, value);
            }
        }
        internal const string TransitionProgressPropertyName = "TransitionProgress";
        internal static readonly DependencyProperty TransitionProgressProperty = DependencyProperty.Register(TransitionProgressPropertyName, typeof(double), typeof(Series),
            new PropertyMetadata(0.0, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(TransitionProgressPropertyName, e.OldValue, e.NewValue);
            }));


        internal static ClockState GetClockState(Storyboard transitionStoryboard)
        {

            try
            {
                return transitionStoryboard.GetCurrentState();
            }
            catch
            {
                return ClockState.Stopped;
            }



        }

        internal Storyboard TransitionStoryboard
        {
            get
            {
                bool wasNull = false;
                if (transitionStoryboard == null)
                {
                    wasNull = true;
                    DoubleAnimation animation = new DoubleAnimation();
                    animation.From = 0.0;
                    animation.To = 1.0;

                    transitionStoryboard = new Storyboard();
                    transitionStoryboard.Children.Add(animation);

                    Storyboard.SetTarget(animation, this);
                    Storyboard.SetTargetProperty(animation, new PropertyPath(Series.TransitionProgressProperty));
                }

                if (wasNull || (GetClockState(transitionStoryboard) == ClockState.Stopped && FunctionOrDurationUpdated))
                {
                    FunctionOrDurationUpdated = false;
                    DoubleAnimation animation = transitionStoryboard.Children[0] as DoubleAnimation;

                    animation.EasingFunction = TransitionEasingFunction;
                    animation.Duration = TransitionDuration;
                }


                if (wasNull)
                {
                    transitionStoryboard.Begin(this, true);
                    transitionStoryboard.Stop(this);
                    transitionStoryboard.Seek(TimeSpan.FromMilliseconds(0));
                }


                return transitionStoryboard;
            }
        }
        private Storyboard transitionStoryboard;

        private DispatcherTimer _animationTimer = new DispatcherTimer();
        private const int AnimationTickMilliseconds = 10;
        private const bool UseDispatcherTimer = false;

        internal void StartupStoryBoard()
        {


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

            {
                Storyboard board = TransitionStoryboard;



                board.Stop(this);

                board.Seek(TimeSpan.FromMilliseconds(0));



                board.Begin(this, true);

            }
        }


#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)



        internal bool AnimationActive()
        {





#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            {
                return GetClockState(TransitionStoryboard) == ClockState.Active;
            }

        }

        internal void StartAnimation()
        {



            StartupStoryBoard();

        }

        #endregion

        #region Resolution Dependency Property
        /// <summary>
        /// Gets or sets the current series object's rendering resolution.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [WidgetDefaultNumber(1.0)]
        public double Resolution
        {
            get
            {
                return (double)GetValue(ResolutionProperty);
            }
            set
            {
                SetValue(ResolutionProperty, value);
            }
        }
        /// <summary>
        /// Property name for the Resolution property.
        /// </summary>
        protected internal const string ResolutionPropertyName = "Resolution";

        /// <summary>
        /// Identifies the Resolution dependency property.
        /// </summary>
        public static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register(ResolutionPropertyName, typeof(double), typeof(Series), new PropertyMetadata(1.0,
            (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(ResolutionPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region Title Dependency Property
        /// <summary>
        /// Gets or sets the Title property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The legend item control is created according to the Title on-demand by 
        /// the series object itself.
        /// </remarks>

        [TypeConverter(typeof(ObjectConverter))]

        public object Title
        {
            get
            {
                return (object)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        internal const string TitlePropertyName = "Title";

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(TitlePropertyName, typeof(object), typeof(Series),
            new PropertyMetadata("Series Title", (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(TitlePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region Brush and ActualBrush Dependency Properties
        /// <summary>
        /// Gets or sets the brush to use for the series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush Brush
        {
            get
            {
                return (Brush)GetValue(BrushProperty);
            }
            set
            {
                SetValue(BrushProperty, value);
            }
        }

        internal const string BrushPropertyName = "Brush";

        /// <summary>
        /// Identifies the Brush dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(BrushPropertyName, typeof(Brush), typeof(Series),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(BrushPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets the effective brush for the current series object.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Brush ActualBrush
        {
            get
            {
                return (Brush)GetValue(ActualBrushProperty);
            }
            internal set
            {
                SetValue(ActualBrushProperty, value);
            }
        }
        /// <summary>
        /// Property name for the ActualBrush property.
        /// </summary>
        protected internal const string ActualBrushPropertyName = "ActualBrush";

        /// <summary>
        /// Identifies the ActualBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualBrushProperty = DependencyProperty.Register(ActualBrushPropertyName, typeof(Brush), typeof(Series),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(ActualBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region Outline and ActualOutline Dependency Properties
        /// <summary>
        /// Gets or sets the brush to use for the outline of the series.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>Some series types, such as LineSeries, do not display outlines.  Therefore, this property does not affect some charts.</remarks>
        public Brush Outline
        {
            get
            {
                return (Brush)GetValue(OutlineProperty);
            }
            set
            {
                SetValue(OutlineProperty, value);
            }
        }

        internal const string OutlinePropertyName = "Outline";
        /// <summary>
        /// Identifies the Outline dependency property.
        /// </summary>
        public static readonly DependencyProperty OutlineProperty = DependencyProperty.Register(OutlinePropertyName, typeof(Brush), typeof(Series),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(OutlinePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets the effective outline for the current series object.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Brush ActualOutline
        {
            get
            {
                return (Brush)GetValue(ActualOutlineProperty);
            }
            internal set
            {
                SetValue(ActualOutlineProperty, value);
            }
        }

        internal const string ActualOutlinePropertyName = "ActualOutline";

        /// <summary>
        /// Identifies the ActualOutline dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualOutlineProperty = DependencyProperty.Register(ActualOutlinePropertyName, typeof(Brush), typeof(Series),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(ActualOutlinePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region LineJoin Property

        /// <summary>
        /// Gets or sets the brush that specifies current series object's line join style.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public PenLineJoin LineJoin
        {
            get
            {
                return (PenLineJoin)GetValue(LineJoinProperty);
            }
            set
            {
                SetValue(LineJoinProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Stroke dependency property.
        /// </summary>
        public static readonly DependencyProperty LineJoinProperty = DependencyProperty.Register(LineJoinPropertyName, typeof(PenLineJoin), typeof(Series),
            new PropertyMetadata(PenLineJoin.Bevel, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(LineJoinPropertyName, e.OldValue, e.NewValue);
            }));

        internal const string LineJoinPropertyName = "LineJoin";
        #endregion

        #region MiterLimit Property
        /// <summary>
        /// Gets or sets the current series object's outline miter limit.
        /// </summary>
        [SuppressWidgetMember]
        public double MiterLimit
        {
            get
            {
                return (double)GetValue(MiterLimitProperty);
            }
            set
            {
                SetValue(MiterLimitProperty, value);
            }
        }

        internal const string MiterLimitPropertyName = "MiterLimit";
        /// <summary>
        /// Identifies the Stroke dependency property.
        /// </summary>
        public static readonly DependencyProperty MiterLimitProperty = DependencyProperty.Register(MiterLimitPropertyName, typeof(double), typeof(Series),
            new PropertyMetadata(0.0, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(MiterLimitPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region Thickness Property
        /// <summary>
        /// Gets or sets the width of the current series object's line thickness.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double Thickness
        {
            get
            {
                return (double)GetValue(ThicknessProperty);
            }
            set
            {
                SetValue(ThicknessProperty, value);
            }
        }

        internal const string ThicknessPropertyName = "Thickness";

        /// <summary>
        /// Identifies the Thickness dependency property.
        /// </summary>
        /// <remarks>
        /// There is a problematic behavior in Silverlight 3 where changing the StrokeThickness property of many shapes is not reflected at runtime.  If changing this property seems to have no effect, please use the workaround of making another change to the UI to force a refresh.
        /// <code>
        /// theChart.RenderTransform = new RotateTransform() { Angle = 0.01 };
        /// Dispatcher.BeginInvoke( () => theChart.RenderTransform = null);
        /// </code>
        /// </remarks>
        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(ThicknessPropertyName, typeof(double), typeof(Series),
            new PropertyMetadata(1.5, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(ThicknessPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region DashCap Property
        /// <summary>
        /// Gets or sets the PenLineCap enumeration value that specifies how the current series object's dash ends are drawn. 
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public PenLineCap DashCap
        {
            get
            {
                return (PenLineCap)GetValue(DashCapProperty);
            }
            set
            {
                SetValue(DashCapProperty, value);
            }
        }

        internal const string DashCapPropertyName = "DashCap";

        /// <summary>
        /// Identifies the DashCap dependency property.
        /// </summary>
        public static readonly DependencyProperty DashCapProperty = DependencyProperty.Register(DashCapPropertyName, typeof(PenLineCap), typeof(Series),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(DashCapPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region DashArray Property
        /// <summary>
        /// Gets or sets a collection of Double values that indicate the pattern of dashes and gaps that
        /// is used to outline the current series object. 
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMember]
        public DoubleCollection DashArray
        {
            get
            {
                return (DoubleCollection)GetValue(DashArrayProperty);
            }
            set
            {
                SetValue(DashArrayProperty, value);
            }
        }

        internal const string DashArrayPropertyName = "DashArray";

        /// <summary>
        /// Identifies the StrokeDashArray dependency property.
        /// </summary>
        public static readonly DependencyProperty DashArrayProperty = DependencyProperty.Register(DashArrayPropertyName, typeof(DoubleCollection), typeof(Series),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(DashArrayPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region ToolTip Dependency Property
        /// <summary>
        /// Gets or sets the ToolTip for the current series object.
        /// <para>This is a dependency property.</para>
        /// </summary>

        [TypeConverter(typeof(ObjectConverter))]

        [SuppressWidgetMember]
        public object ToolTip
        {
            get
            {
                return (object)GetValue(ToolTipProperty);
            }
            set
            {
                SetValue(ToolTipProperty, value);
            }
        }

        internal const string ToolTipPropertyName = "ToolTip";

        /// <summary>
        /// Identifies the ToolTip dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register(ToolTipPropertyName, typeof(object), typeof(Series),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(ToolTipPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        /// <summary>
        /// Method called when the series has changed in a way that will invalidate the range and rendering of its axes.
        /// </summary>
        protected internal virtual void InvalidateAxes() {  }

        /// <summary>
        /// Calculates the weighted moving average.
        /// </summary>
        /// <remarks>
        /// The first period values are calculated by accumulation and may be considered invalid.
        /// </remarks>
        /// <param name="sequence">Sequence to average.</param>
        /// <param name="period">Average period.</param>
        /// <returns>The weighted moving average.</returns>
        public static IEnumerable<double> WMA(IEnumerable<double> sequence, int period)
        {
            return TrendCalculators.WMA(sequence, period);
        }

        /// <summary>
        /// Calculates the exponential moving average.
        /// </summary>
        /// <remarks>
        /// The first period values are calculated by accumulation and may be considered invalid.
        /// </remarks>
        /// <param name="sequence">Sequence to average.</param>
        /// <param name="period">Average period.</param>
        /// <returns>The exponential moving average.</returns>
        public static IEnumerable<double> EMA(IEnumerable<double> sequence, int period)
        {
            return TrendCalculators.EMA(sequence, period);
        }

        /// <summary>
        /// Calculates the modified moving average.
        /// </summary>
        /// <remarks>
        /// The first period values are calculated by accumulation and may be considered invalid.
        /// </remarks>
        /// <param name="sequence">Sequence to average.</param>
        /// <param name="period">Average period.</param>
        /// <returns>The modified moving average.</returns>
        public static IEnumerable<double> MMA(IEnumerable<double> sequence, int period)
        {
            return TrendCalculators.MMA(sequence, period);
        }

        /// <summary>
        /// Calculates the cumulative moving average.
        /// </summary>
        /// <param name="sequence">Sequence to average.</param>
        /// <returns>The cumulative moving average.</returns>
        public static IEnumerable<double> CMA(IEnumerable<double> sequence)
        {
            return TrendCalculators.CMA(sequence);
        }

        /// <summary>
        /// Calculates the simple moving average.
        /// </summary>
        /// <remarks>
        /// The first period values are calculated by accumulation and may be considered invalid.
        /// </remarks>
        /// <param name="sequence">Sequence to average.</param>
        /// <param name="period">Average period.</param>
        /// <returns>The simple moving average.</returns>
        public static IEnumerable<double> SMA(IEnumerable<double> sequence, int period)
        {
            return TrendCalculators.SMA(sequence, period);
        }

        /// <summary>
        /// Calculates a moving sum over a sequence with a given period.
        /// </summary>
        /// <param name="sequence">The sequence for which to calculate the moving sum.</param>
        /// <param name="period">The period to use for the calculation.</param>
        /// <returns>The moving sum values.</returns>
        public static IEnumerable<double> MovingSum(IEnumerable<double> sequence, int period)
        {
            return TrendCalculators.MovingSum(sequence, period);
        }

        /// <summary>
        /// Calculates the standard deviation of a sequence with a given period.
        /// </summary>
        /// <param name="sequence">The sequence for which to calculate the standard deviation values.</param>
        /// <param name="period">The period to use for the calculation.</param>
        /// <returns>The sequence of calculated standard deviaton values.</returns>
        public static IEnumerable<double> STDEV(IEnumerable<double> sequence, int period)
        {
            return TrendCalculators.STDEV(sequence, period);
        }

        /// <summary>
        /// Evaluates the given epression over a range of indexes.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="start">The starting index.</param>
        /// <param name="length">The ending index.</param>
        /// <returns>The stream of values.</returns>
        public static IEnumerable<double> ToEnumerableRange(Func<int, double> expression, int start, int length)
        {
            for (int i = start; i < length; i++)
            {
                yield return expression(i);
            }
        }

        /// <summary>
        /// Evaluates the given expression over a range of indexes starting at 0.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="length">The number of items to evaluate.</param>
        /// <returns>The stream of values.</returns>
        public static IEnumerable<double> ToEnumerable(Func<int, double> expression, int length)
        {
            return ToEnumerableRange(expression, 0, length);
        }



#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)


        internal const string StartCapPropertyName = "StartCap";
        /// <summary>
        /// Identifies the StartCap dependency property.
        /// </summary>
        public static readonly DependencyProperty StartCapProperty = DependencyProperty.Register(StartCapPropertyName, typeof(PenLineCap), typeof(Series),
            new PropertyMetadata(PenLineCap.Round, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(StartCapPropertyName, e.OldValue, e.NewValue);
            }));
        /// <summary>
        /// Gets or sets the style of the starting point of any lines or polylines representing this series.
        /// </summary>
        /// <remarks>
        /// Not every series type has a line at which it would be appropriate to display a start cap, so this property does not affect every series type.  LineSeries, for example, is affected by StartCap, but ColumnSeries is not.
        /// </remarks>
        [SuppressWidgetMember]
        public PenLineCap StartCap
        {
            get
            {
                return (PenLineCap)this.GetValue(StartCapProperty);
            }
            set
            {
                this.SetValue(StartCapProperty, value);
            }
        }
        internal const string EndCapPropertyName = "EndCap";
        /// <summary>
        /// Identifies the EndCap dependency property.
        /// </summary>
        public static readonly DependencyProperty EndCapProperty = DependencyProperty.Register(EndCapPropertyName, typeof(PenLineCap), typeof(Series),
            new PropertyMetadata(PenLineCap.Round, (sender, e) =>
            {
                (sender as Series).RaisePropertyChanged(EndCapPropertyName, e.OldValue, e.NewValue);
            }));
        /// <summary>
        /// Gets or sets the style of the end point of any lines or polylines representing this series.
        /// </summary>
        /// <remarks>
        /// Not every series type has a line at which it would be appropriate to display an end cap, so this property does not affect every series type.  LineSeries, for example, is affected by EndCap, but ColumnSeries is not.
        /// </remarks>
        [SuppressWidgetMember]
        public PenLineCap EndCap
        {
            get
            {
                return (PenLineCap)this.GetValue(EndCapProperty);
            }
            set
            {
                this.SetValue(EndCapProperty, value);
            }
        }

        void IProvidesViewport.GetViewInfo(out Rect viewportRect, out Rect windowRect)
        {
            this.GetViewInfo(out viewportRect, out windowRect);
        }

        private SeriesComponentsForView _seriesComponentsForView = new SeriesComponentsForView();
        internal virtual SeriesComponentsForView GetSeriesComponentsForView()
        {

            _seriesComponentsForView.Series = this;
            _seriesComponentsForView.RootCanvas = RootCanvas;

            return _seriesComponentsForView;
        }


        internal Canvas MarkerCanvas
        {
            get
            {
                return GetSeriesComponentsFromView().MarkerCanvas;
            }
        }


        /// <summary>
        /// Gets the tooltip formatter associated with the series.
        /// </summary>
        public StringFormatter ToolTipFormatter 
        {
            get
            {
                return GetSeriesComponentsFromView().ToolTipFormatter;
            }
        }

        internal DataContext GetDataContextFromSender(object sender)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            return frameworkElement != null ? frameworkElement.DataContext as DataContext : null;
        }


        internal void SetMarkerCanvas(Canvas canvas)
        {
            UpdateMarkerCanvas(canvas);
        }

        internal double GetTotalMilliseconds()
        {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            return TransitionDuration.TotalMilliseconds;

        }

        /// <summary>
        /// Keeps track of the most recent item that was hovered over. Used for the mouse leave event.
        /// </summary>
        internal object LastHoverItem { get; set; }

        internal bool MouseIsOver { get; set; }
        private bool Pressed { get; set; }

        internal virtual void OnMouseEnter(Point pt, object source, object data)
        {

            View.GoToMouseOverState();

            MouseIsOver = true;
            object item = Item(source, pt);
            LastHoverItem = item;

            if (SeriesViewer != null && !ContentInfo.IsDirty)
            {
                View.UpdateToolTip(pt, item, data);
            }

            if (SeriesViewer != null)
            {
                SeriesViewer.OnSeriesMouseEnter(this, item, data);
            }
        }

        internal virtual void OnMouseMove(Point point, object source, object data)
        {



            object item = Item(source, point);

            View.UpdateToolTip(point, item, data);

            if (SeriesViewer != null)
            {
                SeriesViewer.OnSeriesMouseMove(this, item, data);
            }
        }

        internal virtual void OnMouseLeave(Point pt, object p, object data)
        {
            View.GoToNormalState();

            MouseIsOver = false;
            //the current point is not valid for getting an item, because it lies outside the item.
            //object item = Item(e.OriginalSource, point);
            object item = LastHoverItem;

            View.HideTooltip();

            if (SeriesViewer != null)
            {
                SeriesViewer.OnSeriesMouseLeave(this, item, data);
            }
        }

        internal virtual void OnLeftButtonDown(Point pt, object source, object data)
        {
            Pressed = true;
            if (SeriesViewer != null)
            {
                SeriesViewer.OnSeriesMouseLeftButtonDown(
                    this, Item(source, pt), data);
            }
        }

        internal virtual void OnMouseLeftButtonUp(Point pt, object source, object data)
        {
            Pressed = false;
            if (SeriesViewer != null && MouseIsOver)
            {
                SeriesViewer.OnSeriesMouseLeftButtonUp(
                    this, Item(source, pt), data);
            }
        }

        internal virtual void OnLostMouseCapture(Point pt, object source, object data)
        {
            if (Pressed)
            {
                OnMouseLeftButtonUp(pt, source, data);
            }
            if (MouseIsOver)
            {
                OnMouseLeave(pt, source, data);
            }
        }

        internal virtual void OnRightButtonDown(Point pt, object source, object data)
        {
            if (SeriesViewer != null)
            {
                SeriesViewer.OnSeriesMouseRightButtonDown(
                    this, Item(source, pt), data);
            }
        }

        internal virtual void OnRightButtonUp(Point pt, object source, object data)
        {
            if (SeriesViewer != null)
            {
                SeriesViewer.OnSeriesMouseRightButtonUp(
                    this, Item(source, pt), data);
            }
        }

        private void AssertLegendItems(XamDataChart oldChart, XamDataChart newChart)
        {
            if (ActualLegend != null && LegendItem != null)
            {
                if (newChart == null && ActualLegend.Children.Contains(LegendItem))
                {
                    ActualLegend.Children.Remove(LegendItem);
                }

                if (newChart != null && !ActualLegend.Children.Contains(LegendItem)



                    )
                {
                    ActualLegend.AddChildInOrder(LegendItem, this);
                }
            }
        }

        private void ClearLegendItems()
        {
            if (ActualLegend == null || LegendItem == null)
            {
                return;
            }

            ActualLegend.Children.Remove(LegendItem);
        }

        internal void AssertLegendItems(LegendBase oldLegend, LegendBase newLegend)
        {
            if (LegendItem != null)
            {
                if (oldLegend != null && oldLegend.Children.Contains(LegendItem))
                {
                    oldLegend.Children.Remove(LegendItem);
                }

                if (SeriesViewer != null && newLegend != null && !newLegend.Children.Contains(this.LegendItem)



                    )
                {
                    newLegend.AddChildInOrder(LegendItem, this);
                }
            }
        }

        private void AssertLegendItems(Control oldItem, Control newItem)
        {
            if (ActualLegend != null && oldItem != null && ActualLegend.Children.Contains(oldItem))
            {
                ActualLegend.Children.Remove(oldItem);
            }

            if (SeriesViewer != null && ActualLegend != null && newItem != null



                )
            {
                ActualLegend.AddChildInOrder(newItem, this);
            }
        }

        internal virtual void ReleaseItemsSource(IFastItemsSourceProvider provider)
        {
            FastItemsSource = provider
                            .ReleaseFastItemsSource(ItemsSource);
        }

        internal virtual void RegisterItemsSource(IFastItemsSourceProvider provider)
        {
            FastItemsSource = provider
                            .GetFastItemsSource(ItemsSource);
        }

        /// <summary>
        /// Finds the legend to use for this series.
        /// </summary>
        /// <returns>The legend that should be used.</returns>
        protected virtual LegendBase FindActualLegend()
        {
            if (Legend != null)
            {
                return Legend;
            }

            if (SeriesViewer != null && SeriesViewer.Legend != null)
            {
                return SeriesViewer.Legend;
            }
            return null;
        }

       
        internal virtual bool ShouldAnimate(bool animate)
        {
            return (animate && GetTotalMilliseconds() > 0) 






                ;
        }



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Deregisters this series with an axis.
        /// </summary>
        /// <param name="axis">The axis to deregister from.</param>
        protected void DeregisterForAxis(Axis axis)
        {
            if (axis != null)
            {
                axis.DeregisterSeries(this);
            }
        }

        /// <summary>
        /// Registers this series with an axis.
        /// </summary>
        /// <param name="axis">The axis to register with.</param>
        protected void RegisterForAxis(Axis axis)
        {
            if (axis != null)
            {
                axis.RegisterSeries(this);
            }
        }

        internal SeriesComponentsFromView GetSeriesComponentsFromView()
        {
            return View.GetSeriesComponentsFromView();
        }

        internal void UpdateMarkerCanvas(Canvas canvas)
        {
            View.SetMarkerCanvas(canvas);
        }










#region Infragistics Source Cleanup (Region)




















































#endregion // Infragistics Source Cleanup (Region)




#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Registers a double column for the assigned ItemsSource.
        /// </summary>
        /// <param name="memberPath">The property path from which to retrieve the column values.</param>
        /// <returns>The column reference.</returns>
        protected virtual IFastItemColumn<double> RegisterDoubleColumn(string memberPath)
        {


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

            return FastItemsSource.RegisterColumn(memberPath);

        }

        /// <summary>
        /// Registers an int column for the assigned ItemsSource.
        /// </summary>
        /// <param name="memberPath">The property path from which to retrieve the column values.</param>
        /// <returns>The column reference.</returns>
        protected virtual IFastItemColumn<int> RegisterIntColumn(string memberPath)
        {


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

            return FastItemsSource.RegisterColumnInt(memberPath);

        }

        /// <summary>
        /// Registers an object column for the assigned ItemsSource.
        /// </summary>
        /// <param name="memberPath">The property path from which to retrieve the column values.</param>
        /// <returns>The column reference.</returns>
        protected virtual IFastItemColumn<object> RegisterObjectColumn(string memberPath)
        {


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

            return FastItemsSource.RegisterColumnObject(memberPath);

        }

        /// <summary>
        /// Gets the viewport associated with the series.
        /// </summary>
        protected Rect Viewport { get { return this.View != null ? this.View.Viewport : Rect.Empty; } }

        /// <summary>
        /// Exports visual information about the series for use by external tools and functionality.
        /// </summary>
        public SeriesVisualData ExportVisualData()
        {
            SeriesVisualData svd = new SeriesVisualData();
            svd.Viewport = Viewport;
            svd.Type = this.GetType().Name;
            svd.Name = Name;

            ExportVisualDataOverride(svd);
            View.ExportViewShapes(svd);

            return svd;
        }

        /// <summary>
        /// Called when the series should provide visual data for export.
        /// </summary>
        /// <param name="svd">The container for the visual data to export.</param>
        protected virtual void ExportVisualDataOverride(SeriesVisualData svd)
        {
            
        }
        /// <summary>
        /// Property name for the TrendLineDashArray property.
        /// </summary>
        protected internal const string TrendLineDashArrayPropertyName = "TrendLineDashArray";
        /// <summary>
        /// Property name for the TrendLineType property.
        /// </summary>
        protected internal const string TrendLineTypePropertyName = "TrendLineType";
        /// <summary>
        /// Property name for the TrendLinePeriod property.
        /// </summary>
        protected internal const string TrendLinePeriodPropertyName = "TrendLinePeriod";
        /// <summary>
        /// Property name for the TrendLineBrush property.
        /// </summary>
        protected internal const string TrendLineBrushPropertyName = "TrendLineBrush";
        /// <summary>
        /// Property name for the TrendLineActualBrush property.
        /// </summary>
        protected internal const string TrendLineActualBrushPropertyName = "ActualTrendLineBrush";
        /// <summary>
        /// Property name for the TrendLineThickness property.
        /// </summary>
        protected internal const string TrendLineThicknessPropertyName = "TrendLineThickness";
        /// <summary>
        /// Property name for the TrendLineDashCap property.
        /// </summary>
        protected internal const string TrendLineDashCapPropertyName = "TrendLineDashCap";
        /// <summary>
        /// Property name for the TrendLineZIndex property.
        /// </summary>
        protected internal const string TrendLineZIndexPropertyName = "TrendLineZIndex";
    }

    /// <summary>
    /// Represents a target for a render operation.
    /// </summary>
    public class RenderSurface
    {



        /// <summary>
        /// The surface on which to render.
        /// </summary>
        public Canvas Surface { get; set; }

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