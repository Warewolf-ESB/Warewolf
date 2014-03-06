using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Infragistics.Controls
{
    internal enum AdjustmentPriority
    {
        None,
        Biggest,
        Smallest,
        Width,
        Height,
        Best,
        Worst
    }

    /// <summary>
    /// Represents a surface viewer control.
    /// </summary>
    [TemplatePart(Name = SurfaceViewer.ThumbnailName, Type = typeof(XamOverviewPlusDetailPane))]
    [TemplatePart(Name = SurfaceViewer.ContentPresenterName, Type = typeof(Canvas))]
    [TemplatePart(Name = SurfaceViewer.OverlayName, Type = typeof(Canvas))]
    public abstract class SurfaceViewer : ContentControl, IOverviewPlusDetailControl, INotifyPropertyChanged
    {
        #region Constants

        private const double MinScale = 0.00001;
        private const double ScaleChange = 0.1;

        private const string ContentPresenterName = "ContentPresenter";
        private const string OverlayName = "Overlay";
        private const string ThumbnailName = "Thumbnail";

        private readonly Path PreviewPath = new Path() { Data = new GeometryGroup(), IsHitTestVisible = false, Visibility = Visibility.Collapsed };
        private readonly Path DragPath = new Path() { Data = new RectangleGeometry(), IsHitTestVisible = false, Visibility = Visibility.Collapsed };
        
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SurfaceViewer"/> class.
        /// </summary>
        protected SurfaceViewer()
        {
            this.DefaultStyleKey = typeof(SurfaceViewer);

            (this.PreviewPath.Data as GeometryGroup).Children.Add(new RectangleGeometry());
            (this.PreviewPath.Data as GeometryGroup).Children.Add(new RectangleGeometry());
        }

        #region Events

        /// <summary>
        /// Occurs when the drag rect is changing.
        /// </summary>
        public event RectChangedEventHandler DragRectChanging;
        /// <summary>
        /// Raises the <see cref="E:DragRectChanging"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Infragistics.RectChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnDragRectChanging(RectChangedEventArgs e)
        {
            if (DragRectChanging != null)
            {
                DragRectChanging(this, e);
            }

            if (WorldToViewport(e.NewRect).GetArea() > 16)
            {
                this.PreviewRect = Adjust(e.NewRect, AdjustmentPriority.Best);
            }
            else
            {
                this.PreviewRect = Rect.Empty;
            }
        }

        /// <summary>
        /// Occurs when the drag rect is changed.
        /// </summary>
        public event RectChangedEventHandler DragRectChanged;
        /// <summary>
        /// Raises the <see cref="E:DragRectChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Infragistics.RectChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnDragRectChanged(RectChangedEventArgs e)
        {
            if (DragRectChanged != null)
            {
                DragRectChanged(this, e);
            }

            this.PreviewRect = Rect.Empty;

            if (WorldToViewport(e.NewRect).GetArea() > 16)
            {
                this.WindowRect = Adjust(e.NewRect, AdjustmentPriority.Best);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }

        #endregion

        #region Dependency Properties

        #region DragStroke Dependency Property
        /// <summary>
        /// Sets or gets the DragStroke property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Brush DragStroke
        {
            get
            {
                return (Brush)GetValue(DragStrokeProperty);
            }
            set
            {
                SetValue(DragStrokeProperty, value);
            }
        }

        private const string DragStrokePropertyName = "DragStroke";

        /// <summary>
        /// Identifies the DragStroke dependency property.
        /// </summary>
        public static readonly DependencyProperty DragStrokeProperty = DependencyProperty.Register(DragStrokePropertyName, typeof(Brush), typeof(SurfaceViewer),
            new PropertyMetadata(new SolidColorBrush(Colors.Black), (sender, e) =>
            {
                (sender as SurfaceViewer).OnPropertyUpdated(DragStrokePropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region DragStrokeThickness Dependency Property
        /// <summary>
        /// Sets or gets the DragStrokeThickness property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public double DragStrokeThickness
        {
            get
            {
                return (double)GetValue(DragStrokeThicknessProperty);
            }
            set
            {
                SetValue(DragStrokeThicknessProperty, value);
            }
        }

        private const string DragStrokeThicknessPropertyName = "DragStrokeThickness";

        /// <summary>
        /// Identifies the DragStrokeThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty DragStrokeThicknessProperty = DependencyProperty.Register(DragStrokeThicknessPropertyName, typeof(double), typeof(SurfaceViewer),
            new PropertyMetadata(1.0, (sender, e) =>
            {
                (sender as SurfaceViewer).OnPropertyUpdated(DragStrokeThicknessPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region DragStrokeDashArray Dependency Property
        /// <summary>
        /// Sets or gets the DragStrokeDashArray property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public DoubleCollection DragStrokeDashArray
        {
            get
            {
                return (DoubleCollection)GetValue(DragStrokeDashArrayProperty);
            }
            set
            {
                SetValue(DragStrokeDashArrayProperty, value);
            }
        }

        private const string DragStrokeDashArrayPropertyName = "DragStrokeDashArray";

        /// <summary>
        /// Identifies the DragStrokeDashArray dependency property.
        /// </summary>
        public static readonly DependencyProperty DragStrokeDashArrayProperty = DependencyProperty.Register(DragStrokeDashArrayPropertyName, typeof(DoubleCollection), typeof(SurfaceViewer),
            new PropertyMetadata((sender, e) =>
            {
                (sender as SurfaceViewer).OnPropertyUpdated(DragStrokeDashArrayPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region PreviewBrush Dependency Property
        /// <summary>
        /// Sets or gets the PreviewBrush property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Brush PreviewBrush
        {
            get
            {
                return (Brush)GetValue(PreviewBrushProperty);
            }
            set
            {
                SetValue(PreviewBrushProperty, value);
            }
        }

        private const string PreviewBrushPropertyName = "PreviewBrush";

        /// <summary>
        /// Identifies the PreviewBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty PreviewBrushProperty = DependencyProperty.Register(PreviewBrushPropertyName, typeof(Brush), typeof(SurfaceViewer),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x80, 0x80, 0x80, 0x80)), (sender, e) =>
            {
                (sender as SurfaceViewer).OnPropertyUpdated(PreviewBrushPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region NavigationSettings

        private NavigationSettings _navigationSettings;

        /// <summary>
        /// Gets or sets the navigation settings.
        /// </summary>
        /// <value>The navigation settings.</value>
        public NavigationSettings NavigationSettings
        {
            get
            {
                if (this._navigationSettings == null)
                {
                    this._navigationSettings = new NavigationSettings();
                }

                return this._navigationSettings;
            }
            set
            {
                if (value != this._navigationSettings)
                {
                    OnPropertyUpdated("NavigationSettings", this._navigationSettings, value);
                    this._navigationSettings = value;
                }
            }
        }
        #endregion

        #region MinimumZoomLevel

        /// <summary>
        /// Gets or sets the minimum scale.
        /// </summary>
        /// <value>The minimum scale.</value>
        public double MinimumZoomLevel
        {
            get { return (double)GetValue(MinimumZoomLevelProperty); }
            set { SetValue(MinimumZoomLevelProperty, value); }
        }

        private const string MinimumZoomLevelPropertyName = "MinimumZoomLevel";

        /// <summary>
        /// Identifies the <see cref="MinimumZoomLevel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumZoomLevelProperty =
            DependencyProperty.Register(MinimumZoomLevelPropertyName, typeof(double), typeof(SurfaceViewer),
            new PropertyMetadata(double.NaN, OnMinimumZoomLevelChanged));

        private static void OnMinimumZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SurfaceViewer)d).OnMinimumZoomLevelChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// MinimumZoomLevelProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMinimumZoomLevelChanged(double oldValue, double newValue)
        {
            OnPropertyUpdated(MinimumZoomLevelPropertyName, oldValue, newValue);

            // force rescaling
            this.ZoomLevel = this.ZoomLevel;
        }

        #endregion

        #region MaximumZoomLevel

        /// <summary>
        /// Gets or sets the maximum scale.
        /// </summary>
        /// <value>The maximum scale.</value>
        public double MaximumZoomLevel
        {
            get { return (double)GetValue(MaximumZoomLevelProperty); }
            set { SetValue(MaximumZoomLevelProperty, value); }
        }

        private const string MaximumZoomLevelPropertyName = "MaximumZoomLevel";

        /// <summary>
        /// Identifies the <see cref="MaximumZoomLevel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumZoomLevelProperty =
            DependencyProperty.Register(MaximumZoomLevelPropertyName, typeof(double), typeof(SurfaceViewer),
            new PropertyMetadata(double.NaN, OnMaximumZoomLevelChanged));

        private static void OnMaximumZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SurfaceViewer)d).OnMaximumZoomLevelChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// MaximumZoomLevelProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMaximumZoomLevelChanged(double oldValue, double newValue)
        {
            OnPropertyUpdated(MaximumZoomLevelPropertyName, oldValue, newValue);

            // force rescaling
            this.ZoomLevel = this.ZoomLevel;
        }

        #endregion

        #region WorldRect

        /// <summary>
        /// Gets or sets the world rect.
        /// </summary>
        /// <value>The world rect.</value>
        public Rect WorldRect
        {
            get { return (Rect)GetValue(WorldRectProperty); }
            set { SetValue(WorldRectProperty, value); }
        }

        private const string WorldRectPropertyName = "WorldRect";

        /// <summary>
        /// Identifies the <see cref="WorldRect"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WorldRectProperty =
            DependencyProperty.Register(WorldRectPropertyName, typeof(Rect), typeof(SurfaceViewer),
            new PropertyMetadata(Rect.Empty, OnWorldRectChanged));

        private static void OnWorldRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SurfaceViewer)d).OnWorldRectChanged((Rect)e.OldValue, (Rect)e.NewValue);
        }

        /// <summary>
        /// WorldRectProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnWorldRectChanged(Rect oldValue, Rect newValue)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(WorldRectPropertyName));

            UpdateNavigators();
        }

        #endregion

        #region WindowRect

        /// <summary>
        /// Gets or sets the window rect.
        /// </summary>
        /// <value>The window rect.</value>
        public Rect WindowRect
        {
            get { return (Rect)GetValue(WindowRectProperty); }
            set { SetValue(WindowRectProperty, value); }
        }

        private const string WindowRectPropertyName = "WindowRect";

        /// <summary>
        /// Identifies the <see cref="WindowRect"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowRectProperty =
            DependencyProperty.Register(WindowRectPropertyName, typeof(Rect), typeof(SurfaceViewer),
            new PropertyMetadata(Rect.Empty, OnWindowRectChanged));

        private static void OnWindowRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SurfaceViewer)d).OnWindowRectChanged((Rect)e.OldValue, (Rect)e.NewValue);
        }

        /// <summary>
        /// WindowRectProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnWindowRectChanged(Rect oldValue, Rect newValue)
        {
            if (newValue.IsEmpty)
            {
                return;
            }

            double oldScale = 1;

            if (oldValue.IsEmpty == false)
            {
                oldScale = this.ViewportRect.Width / oldValue.Width;
            }

            double newScale = this.ViewportRect.Width / newValue.Width;

            if (oldScale != newScale || ZoomLevelDisplayText == null)
            {
                OnPropertyUpdated(ZoomLevelPropertyName, oldScale, newScale);
            }

            OnPropertyChanged(new PropertyChangedEventArgs(WindowRectPropertyName));

            UpdateNavigators();
        }

        #endregion

        #region PreviewRect

        /// <summary>
        /// Gets or sets the preview rect.
        /// </summary>
        /// <value>The preview rect.</value>
        public Rect PreviewRect
        {
            get { return (Rect)GetValue(PreviewRectProperty); }
            set { SetValue(PreviewRectProperty, value); }
        }

        private const string PreviewRectPropertyName = "PreviewRect";

        /// <summary>
        /// Identifies the <see cref="PreviewRect"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PreviewRectProperty =
            DependencyProperty.Register(PreviewRectPropertyName, typeof(Rect), typeof(SurfaceViewer),
            new PropertyMetadata(Rect.Empty, OnPreviewRectChanged));

        private static void OnPreviewRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SurfaceViewer)d).OnPreviewRectChanged((Rect)e.OldValue, (Rect)e.NewValue);
        }

        /// <summary>
        /// PreviewRectProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnPreviewRectChanged(Rect oldValue, Rect newValue)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(PreviewRectPropertyName));

            this.PreviewPath.Visibility = PreviewRect.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
            UpdateNavigators();
        }

        #endregion

        #region ViewportRect Property

        /// <summary>
        /// Gets the ViewportRect property.
        /// </summary>
        public Rect ViewportRect
        {
            get { return _viewportRect; }
            private set
            {
                if (this.ViewportRect != value)
                {
                    Rect oldViewportRect = this.ViewportRect;
                    _viewportRect = value;

                    OnViewportRectChanged(oldViewportRect, this.ViewportRect);
                }
            }
        }
        private Rect _viewportRect = Rect.Empty;
        private const string ViewportRectPropertyName = "ViewportRect";

        /// <summary>
        /// ViewportRectProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnViewportRectChanged(Rect oldValue, Rect newValue)
        {
            if (newValue.IsEmpty == false)
            {
                if (oldValue.IsEmpty == false && this.WindowRect.IsEmpty == false)
                {
                    double oldZoom = Math.Min(oldValue.Width / this.WindowRect.Width, oldValue.Height / this.WindowRect.Height);
                    double newZoom = Math.Min(newValue.Width / this.WindowRect.Width, newValue.Height / this.WindowRect.Height);

                    double dScale = newZoom / oldZoom;

                    Rect window = this.WindowRect;

                    window.Width *= dScale;
                    window.Height *= dScale;

                    this.WindowRect = window;

                    if (this.OverviewPlusDetailPane != null)
                    {
                        this.OverviewPlusDetailPane.Window = window;
                    }
                }


                ((PreviewPath.Data as GeometryGroup).Children[0] as RectangleGeometry).Rect = newValue;
                (Overlay.Clip as RectangleGeometry).Rect = newValue;
            }

            UpdateNavigators();
        }

        #endregion


        #region OverviewPlusDetailPaneStyle

        /// <summary>
        /// Gets or sets the overview plus detail pane style.
        /// </summary>
        /// <value>The overview plus detail pane style.</value>
        public Style OverviewPlusDetailPaneStyle
        {
            get { return (Style)GetValue(OverviewPlusDetailPaneStyleProperty); }
            set { SetValue(OverviewPlusDetailPaneStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="OverviewPlusDetailPaneStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OverviewPlusDetailPaneStyleProperty =
            DependencyProperty.Register("OverviewPlusDetailPaneStyle", typeof(Style), typeof(SurfaceViewer),
            new PropertyMetadata(null, OnOverviewPlusDetailPaneStyleChanged));

        private static void OnOverviewPlusDetailPaneStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SurfaceViewer)d).OnOverviewPlusDetailPaneStyleChanged((Style)e.OldValue, (Style)e.NewValue);
        }

        /// <summary>
        /// OverviewPlusDetailPaneStyleProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnOverviewPlusDetailPaneStyleChanged(Style oldValue, Style newValue)
        {
            if (this.OverviewPlusDetailPane != null)
            {
                if (newValue != null)
                {
                    this.OverviewPlusDetailPane.Style = newValue;
                }
                else
                {
                    this.OverviewPlusDetailPane.ClearValue(Control.StyleProperty);
                }
            }
        }

        #endregion

        #region HorizontalOverviewPlusDetailPaneAlignment

        /// <summary>
        /// Gets or sets the horizontal overview plus detail pane alignment.
        /// </summary>
        /// <value>The horizontal overview plus detail pane alignment.</value>
        public HorizontalAlignment HorizontalOverviewPlusDetailPaneAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalOverviewPlusDetailPaneAlignmentProperty); }
            set { SetValue(HorizontalOverviewPlusDetailPaneAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalOverviewPlusDetailPaneAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalOverviewPlusDetailPaneAlignmentProperty =
            DependencyProperty.Register("HorizontalOverviewPlusDetailPaneAlignment", typeof(HorizontalAlignment), typeof(SurfaceViewer),
              new PropertyMetadata(HorizontalAlignment.Right));

        #endregion

        #region VerticalOverviewPlusDetailPaneAlignment

        /// <summary>
        /// Gets or sets the vertical overview plus detail pane alignment.
        /// </summary>
        /// <value>The vertical overview plus detail pane alignment.</value>
        public VerticalAlignment VerticalOverviewPlusDetailPaneAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalOverviewPlusDetailPaneAlignmentProperty); }
            set { SetValue(VerticalOverviewPlusDetailPaneAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalOverviewPlusDetailPaneAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalOverviewPlusDetailPaneAlignmentProperty =
            DependencyProperty.Register("VerticalOverviewPlusDetailPaneAlignment", typeof(VerticalAlignment), typeof(SurfaceViewer),
              new PropertyMetadata(VerticalAlignment.Bottom));

        #endregion

        #region OverviewPlusDetailPaneVisibility

        /// <summary>
        /// Gets or sets the overview plus detail pane visibility.
        /// </summary>
        /// <value>The overview plus detail pane visibility.</value>
        public Visibility OverviewPlusDetailPaneVisibility
        {
            get { return (Visibility)GetValue(OverviewPlusDetailPaneVisibilityProperty); }
            set { SetValue(OverviewPlusDetailPaneVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="OverviewPlusDetailPaneVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OverviewPlusDetailPaneVisibilityProperty =
            DependencyProperty.Register("OverviewPlusDetailPaneVisibility", typeof(Visibility), typeof(SurfaceViewer),
              new PropertyMetadata(Visibility.Visible));

        #endregion


        #endregion

        #region Properties

        #region Public

        private double _initialZoomLevel = double.NaN;

        private const string ZoomLevelPropertyName = "ZoomLevel";

        /// <summary>
        /// Gets or sets the zoom level.
        /// </summary>
        /// <value>The zoom level.</value>
        public double ZoomLevel
        {
            get
            {
                if (double.IsNaN(_initialZoomLevel) == false)
                {
                    return _initialZoomLevel;
                }

                if (this.WindowRect.IsEmpty || this.ViewportRect.IsEmpty)
                {
                    return 1;
                }

                double zoom = Math.Min(this.ViewportRect.Width / this.WindowRect.Width, this.ViewportRect.Height / this.WindowRect.Height);

                if (double.IsNaN(zoom) || double.IsInfinity(zoom))
                {
                    zoom = 1;
                }

                return zoom;
            }
            set
            {
                if (this.ViewportRect.IsEmpty || this.WorldRect.IsEmpty)
                {
                    _initialZoomLevel = value;
                    return;
                }

                _initialZoomLevel = double.NaN;

                double oldZoom = this.ZoomLevel;
                double newZoom = ClampZoomLevel(value);

                double dZoom = oldZoom / newZoom;

                Point center = ViewportToWorld(new Point(this.ViewportRect.Width / 2, this.ViewportRect.Height / 2));

                double left = dZoom * (this.WindowRect.Left - center.X) + center.X;
                double bottom = dZoom * (this.WindowRect.Bottom - center.Y) + center.Y;
                double right = dZoom * (this.WindowRect.Right - center.X) + center.X;
                double top = dZoom * (this.WindowRect.Top - center.Y) + center.Y;

                this.WindowRect = new Rect(left, top, right - left, bottom - top);
            }
        }

        /// <summary>
        /// Gets the overview plus detail pane.
        /// </summary>
        /// <value>The overview plus detail pane.</value>
        public XamOverviewPlusDetailPane OverviewPlusDetailPane { get; internal set; }

        private const string ZoomLevelDisplayTextPropertyName = "ZoomLevelDisplayText";
        private string _zoomLevelDisplayText;

        // this text is displayed in the callout above the zoom level slider.
        /// <summary>
        /// The text to display in the callout above the zoom level slider.
        /// </summary>
        /// <remarks>
        /// This text is expected to change whenever the zoom level is changed.
        /// </remarks>
        public string ZoomLevelDisplayText
        {
            get
            {
                return this._zoomLevelDisplayText;
            }
            set
            {
                bool changed = this.ZoomLevelDisplayText != value;
                if (changed)
                {
                    object oldValue = this.ZoomLevelDisplayText;
                    this._zoomLevelDisplayText = value;
                    this.OnPropertyUpdated(ZoomLevelDisplayTextPropertyName, oldValue, value);
                }
            }
        }

        #endregion

        #region Private

        private Point Anchor { get; set; }
        private bool IsDragging { get; set; }
        private bool IsPanning { get; set; }

        #endregion

        #region Internal

        /// <summary>
        /// The ContentPresenter element from the Template of this SurfaceViewer.
        /// </summary>
        internal ContentPresenter ContentPresenter { get; private set; }

        /// <summary>
        /// The Overlay element from the Template of this SurfaceViewer.
        /// </summary>
        internal Canvas Overlay { get; private set; }

        #endregion

        #endregion

        #region Overrides

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) 
        /// call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>. In simplest terms, this means the method is called 
        /// just before a UI element displays in an application. For more information, see Remarks.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.OverviewPlusDetailPane != null)
            {
                this.OverviewPlusDetailPane.WindowChanging -= thumbnail_WindowRectChanging;
                this.OverviewPlusDetailPane.WindowChanged -= thumbnail_WindowRectChanged;
            }
            this.OverviewPlusDetailPane = GetTemplateChild(ThumbnailName) as XamOverviewPlusDetailPane;
            if (this.OverviewPlusDetailPane != null)
            {
                this.OverviewPlusDetailPane.SurfaceViewer = this;
                this.OverviewPlusDetailPane.WindowChanging += thumbnail_WindowRectChanging;
                this.OverviewPlusDetailPane.WindowChanged += thumbnail_WindowRectChanged;

                if (this.OverviewPlusDetailPaneStyle != null)
                {
                    this.OverviewPlusDetailPane.Style = this.OverviewPlusDetailPaneStyle;
                }
            }

            if (this.ContentPresenter != null)
            {
                this.ContentPresenter.SizeChanged -= ContentPresenter_SizeChanged;
            }

            this.ContentPresenter = GetTemplateChild(ContentPresenterName) as ContentPresenter;

            if (this.ContentPresenter != null)
            {
                this.ContentPresenter.SizeChanged += ContentPresenter_SizeChanged;
            }

            if (this.Overlay != null)
            {
                this.Overlay.Children.Remove(PreviewPath);
                this.Overlay.Children.Remove(DragPath);
            }

            this.Overlay = GetTemplateChild(OverlayName) as Canvas;

            if (this.Overlay != null)
            {
                this.Overlay.Clip = new RectangleGeometry();
                this.Overlay.Children.Add(PreviewPath);
                this.Overlay.Children.Add(DragPath);
            }

            this.DragPath.SetBinding(Shape.StrokeProperty, new Binding(DragStrokePropertyName) { Source = this });
            this.DragPath.SetBinding(Shape.StrokeThicknessProperty, new Binding(DragStrokeThicknessPropertyName) { Source = this });
            this.DragPath.SetBinding(Shape.StrokeDashArrayProperty, new Binding(DragStrokeDashArrayPropertyName) { Source = this });

            this.PreviewPath.SetBinding(Shape.FillProperty, new Binding(PreviewBrushPropertyName) { Source = this });

            this.ViewportRect = this.ContentPresenter != null && this.ContentPresenter.ActualWidth > 0.0 && this.ContentPresenter.ActualHeight > 0.0 ? new Rect(0, 0, this.ContentPresenter.ActualWidth, this.ContentPresenter.ActualHeight) : Rect.Empty;
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (this.CanStartNavigation(e) == false)
            {
                return;
            }

            this.Focus();

            Point pt = e.GetPosition(Overlay);

            if (this.ViewportRect.Contains(pt) == false)
            {
                return;
            }

            bool isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

            if (isCtrlPressed)
            {
                if (this.NavigationSettings.AllowZoom &&
                    this.NavigationSettings.AllowPan)
                {
                    this.IsDragging = CaptureMouse();
                }
            }
            else
            {
                if (this.NavigationSettings.AllowPan)
                {
                    this.IsPanning = CaptureMouse();
                }
            }

            if (this.IsDragging)
            {
                this.Anchor = ViewportToWorld(pt);

                Rect dragRect = new Rect(this.Anchor, ViewportToWorld(e.GetPosition(Overlay)));

                (this.DragPath.Data as RectangleGeometry).Rect = WorldToViewport(dragRect);
                this.DragPath.Visibility = Visibility.Visible;

                OnDragRectChanging(new RectChangedEventArgs(dragRect, dragRect));

                e.Handled = true;
            }
            else if (this.IsPanning)
            {
                this.Anchor = pt;
                this.Cursor = Cursors.Hand;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseMove"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.IsDragging)
            {
                Rect dragRect = new Rect(this.Anchor, ViewportToWorld(e.GetPosition(Overlay)));

                (this.DragPath.Data as RectangleGeometry).Rect = WorldToViewport(dragRect);

                OnDragRectChanging(new RectChangedEventArgs(dragRect, dragRect));
            }
            else if (this.IsPanning)
            {
                Point pt = e.GetPosition(Overlay);

                double dx = this.Anchor.X - pt.X;
                double dy = this.Anchor.Y - pt.Y;

                double scale = this.ZoomLevel;

                dx /= scale;
                dy /= scale;

                if (WindowRect != Rect.Empty)
                {
                    Rect newWindowRect = WindowRect;
                    newWindowRect.X += dx;
                    newWindowRect.Y += dy;
                    this.WindowRect = newWindowRect;
                }

                this.Anchor = pt;
            }

            base.OnMouseMove(e);
        }
        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (this.IsDragging)
            {
                Rect dragRect = new Rect(this.Anchor, ViewportToWorld(e.GetPosition(Overlay)));

                ReleaseMouseCapture();
                this.DragPath.Visibility = Visibility.Collapsed;
                this.IsDragging = false;

                OnDragRectChanged(new RectChangedEventArgs(dragRect, dragRect));
                e.Handled = true;
            }
            else if (this.IsPanning)
            {
                this.Cursor = Cursors.Arrow;
                this.IsPanning = false;
                ReleaseMouseCapture();
                e.Handled = true;
            }

            base.OnMouseLeftButtonUp(e);
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseWheel"/> event occurs to provide handling for the event in a derived class without attaching a delegate.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.MouseWheelEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (this.NavigationSettings.AllowZoom)
            {
                double scale = 1 + Math.Min(0.5, Math.Max(-0.5, e.Delta / 1200.0));
                this.ZoomLevel *= scale;

                e.Handled = true;
            }
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.KeyDown"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            Key key = e.Key;
            bool ctrlKey = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

            switch (e.Key)
            {
                case Key.Escape:
                    if (this.IsDragging)
                    {
                        ReleaseMouseCapture();
                        OnDragRectChanged(new RectChangedEventArgs(Rect.Empty, Rect.Empty));
                        this.DragPath.Visibility = Visibility.Collapsed;
                        this.IsDragging = false;
                        e.Handled = true;
                    }
                    break;
                case Key.Add:
                    if (ctrlKey && this.NavigationSettings.AllowZoom)
                    {
                        ZoomIn();
                        e.Handled = true;
                    }
                    break;
                case Key.Subtract:
                    if (ctrlKey && this.NavigationSettings.AllowZoom)
                    {
                        ZoomOut();
                        e.Handled = true;
                    }
                    break;
            }
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Renders the preview.
        /// </summary>
        public virtual void RenderPreview()
        {

        }

        /// <summary>
        /// Adjusts the specified rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns></returns>
        public Rect Adjust(Rect rect)
        {
            return Adjust(rect, AdjustmentPriority.Biggest);
        }

        /// <summary>
        /// Scales to fit.
        /// </summary>
        public void ScaleToFit()
        {
            Rect viewport = this.ViewportRect;

            if (viewport.IsEmpty || this.WorldRect.IsEmpty)
            {
                return;
            }

            Rect size = this.WorldRect;

            double width, height;

            double x = 0;
            double y = 0;

            if (size.Width >= viewport.Width)
            {
                width = size.Width;
            }
            else
            {
                width = viewport.Width;
                x = (size.Width - width) / 2;
            }

            if (size.Height >= viewport.Height)
            {
                height = size.Height;
            }
            else
            {
                height = viewport.Height;
                y = (size.Height - height) / 2;
            }

            Rect rect = new Rect(x, y, width, height);

            this.WindowRect = Adjust(rect, AdjustmentPriority.Worst);

            if (double.IsNaN(_initialZoomLevel) == false)
            {
                this.ZoomLevel = _initialZoomLevel;
            }
        }

        /// <summary>
        /// Zooms to 100%.
        /// </summary>
        public void ZoomTo100()
        {
            ScaleToFit();
            this.ZoomLevel = 1;
        }

        /// <summary>
        /// Increase the scale.
        /// </summary>
        public void ZoomIn()
        {
            this.ZoomLevel *= (1 + ScaleChange);
        }

        /// <summary>
        /// Decrease the scale.
        /// </summary>
        public void ZoomOut()
        {
            this.ZoomLevel *= (1 - ScaleChange);
        }

        /// <summary>
        /// Gets or sets the default interaction state.
        /// </summary>
        /// <value>The default interaction.</value>
        public InteractionState DefaultInteraction { get; set; }

        #endregion

        #region Internal

        internal Rect Adjust(Rect rect, AdjustmentPriority adjustmentPriority)
        {
            Rect viewportRect = this.ViewportRect;

            if (!viewportRect.IsEmpty)
            {
                double cx = rect.Left + 0.5 * rect.Width;
                double cy = rect.Top + 0.5 * rect.Height;

                double width = rect.Width;
                double height = rect.Height;

                switch (adjustmentPriority)
                {
                    case AdjustmentPriority.Biggest:
                        return Adjust(rect, rect.Width > rect.Height ? AdjustmentPriority.Width : AdjustmentPriority.Height);

                    case AdjustmentPriority.Smallest:
                        return Adjust(rect, rect.Width > rect.Height ? AdjustmentPriority.Height : AdjustmentPriority.Width);

                    case AdjustmentPriority.Best:
                        return Adjust(rect, (viewportRect.Width * height) / (viewportRect.Height * width) > 1.0 ? AdjustmentPriority.Width : AdjustmentPriority.Height);

                    case AdjustmentPriority.Worst:
                        return Adjust(rect, (viewportRect.Width * height) / (viewportRect.Height * width) > 1.0 ? AdjustmentPriority.Height : AdjustmentPriority.Width);

                    case AdjustmentPriority.Width:

                        height = width * viewportRect.Height / viewportRect.Width;
                        rect.Height = height;

                        break;

                    case AdjustmentPriority.Height:
                        width = height * viewportRect.Width / viewportRect.Height;
                        break;
                }

                rect = new Rect(cx - 0.5 * width, cy - 0.5 * height, width, height);
            }

            return FitRectInZoom(rect);
        }

        internal Point ViewportToWorld(Point point)
        {
            Rect viewportRect = ViewportRect;
            Rect windowRect = WindowRect;

            if (viewportRect.IsEmpty || windowRect.IsEmpty)
            {
                return new Point(double.NaN, double.NaN);
            }

            double x = windowRect.Left + windowRect.Width * (point.X - viewportRect.Left) / viewportRect.Width;
            double y = windowRect.Top + windowRect.Height * (point.Y - viewportRect.Top) / viewportRect.Height;

            return new Point(x, y);
        }
        internal Rect WorldToViewport(Rect rect)
        {
            Rect viewportRect = ViewportRect;
            Rect windowRect = WindowRect;

            if (viewportRect.IsEmpty || windowRect.IsEmpty || rect.IsEmpty)
            {
                return Rect.Empty;
            }

            double left = viewportRect.Left + viewportRect.Width * (rect.Left - windowRect.Left) / windowRect.Width;
            double top = viewportRect.Top + viewportRect.Height * (rect.Top - windowRect.Top) / windowRect.Height;
            double right = viewportRect.Left + viewportRect.Width * (rect.Right - windowRect.Left) / windowRect.Width;
            double bottom = viewportRect.Top + viewportRect.Height * (rect.Bottom - windowRect.Top) / windowRect.Height;

            return new Rect(left, top, right - left, bottom - top);
        }

        #endregion

        #region Protected

        /// <summary>
        /// Called when a property is updated.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnPropertyUpdated(string propertyName, object oldValue, object newValue)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

            switch (propertyName)
            {
                case ZoomLevelPropertyName:
                    this.UpdateZoomLevelDisplayText();
                    break;
            }
        }

        /// <summary>
        /// Determines whether this instance can start navigation.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can start navigation; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool CanStartNavigation(MouseButtonEventArgs e)
        {
            return true;
        }

        #endregion

        #region Private

        private void UpdateNavigators()
        {
            if (this.OverviewPlusDetailPane != null)
            {
                this.OverviewPlusDetailPane.World = this.WorldRect;
                this.OverviewPlusDetailPane.Window = this.WindowRect;
                this.OverviewPlusDetailPane.Preview = this.PreviewRect;
            }

            //(DragPath.Data as RectangleGeometry).Rect = WorldToViewport(WindowRect);
            ((this.PreviewPath.Data as GeometryGroup).Children[1] as RectangleGeometry).Rect = WorldToViewport(this.PreviewRect);
        }

        private double ClampZoomLevel(double scale)
        {
            if (double.IsNaN(this.MinimumZoomLevel) == false)
            {
                scale = Math.Max(this.MinimumZoomLevel, scale);
            }

            if (double.IsNaN(this.MaximumZoomLevel) == false)
            {
                scale = Math.Min(this.MaximumZoomLevel, scale);
            }

            scale = Math.Max(scale, MinScale);

            return scale;
        }

        private Rect FitRectInZoom(Rect rect)
        {
            double scale = this.ViewportRect.Width / rect.Width;

            double newScale = ClampZoomLevel(scale);

            if (scale == newScale)
            {
                return rect;
            }

            double dScale = scale / newScale;

            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            double left = dScale * (rect.Left - center.X) + center.X;
            double bottom = dScale * (rect.Bottom - center.Y) + center.Y;
            double right = dScale * (rect.Right - center.X) + center.X;
            double top = dScale * (rect.Top - center.Y) + center.Y;

            rect = new Rect(left, top, right - left, bottom - top);

            return rect;
        }

        private void ContentPresenter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Rect newRect = this.ContentPresenter != null && this.ContentPresenter.ActualWidth > 0.0 && this.ContentPresenter.ActualHeight > 0.0 ? new Rect(0, 0, this.ContentPresenter.ActualWidth, this.ContentPresenter.ActualHeight) : Rect.Empty;

            if (this.ViewportRect != newRect)
            {
                this.ViewportRect = newRect;
            }
            else
            {
                // force the control to rearrange
                 //this.ViewportRect = Rect.Empty;
                 //this.ViewportRect = newRect;
            }
        }

        private void thumbnail_WindowRectChanging(object sender, PropertyChangedEventArgs<Rect> e)
        {
            this.WindowRect = Adjust(e.NewValue);
        }
        private void thumbnail_WindowRectChanged(object sender, PropertyChangedEventArgs<Rect> e)
        {
            this.WindowRect = Adjust(e.NewValue);
        }

        private void UpdateZoomLevelDisplayText()
        {
            this.ZoomLevelDisplayText = Math.Round(100 * this.ZoomLevel).ToString();
        }

        #endregion

        #endregion



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