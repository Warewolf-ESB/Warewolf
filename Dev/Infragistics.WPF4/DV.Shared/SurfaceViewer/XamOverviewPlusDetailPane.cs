using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls
{

    /// <summary>
    /// The XamOverviewPlusDetailPane control.
    /// </summary>

    [StyleTypedProperty(Property = WindowStylePropertyName, StyleTargetType = typeof(Path))]
    [StyleTypedProperty(Property = PreviewStylePropertyName, StyleTargetType = typeof(Path))]
    [TemplatePart(Name = ContentPresenterElementName, Type = typeof(ContentPresenter))]

    [TemplateVisualState(Name = StateFull, GroupName = SizeStates)]
    [TemplateVisualState(Name = StateMinimal, GroupName = SizeStates)]
    [TemplateVisualState(Name = StateZoomEnabled, GroupName = ZoomableStates)]
    [TemplateVisualState(Name = StateZoomDisabled, GroupName = ZoomableStates)]

    public class XamOverviewPlusDetailPane : Control, INotifyPropertyChanged
    {



        internal XamOverviewPlusDetailPaneView View { get; set; }


        private const double MaxZoom = 2;

        internal const string SizeStates = "SizeStates";
        internal const string StateFull = "Full";
        internal const string StateMinimal = "Minimal";
        internal const string ZoomableStates = "ZoomableStates";
        internal const string StateZoomEnabled = "ZoomEnabled";
        internal const string StateZoomDisabled = "ZoomDisabled";


        internal const string RootElementName = "Root";

        internal const string ContentPresenterElementName = "ContentPresenter";

        internal const string ZoomOutName = "ZoomOut";
        internal const string ZoomLevelName = "ZoomLevel";
        internal const string ZoomInName = "ZoomIn";
        internal const string ZoomTo100Name = "ZoomTo100";
        internal const string ScaleToFitName = "ScaleToFit";

        internal const string ButtonCursorName = "ButtonCursor";
        internal const string DragPanName = "DragPan";
        internal const string DragZoomName = "DragZoom";

        internal const string ButtonsGridName = "ButtonsGrid";

        /// <summary>
        /// Initializes a new instance of the <see cref="XamOverviewPlusDetailPane"/> class.
        /// </summary>
        public XamOverviewPlusDetailPane()
        {
            View = new XamOverviewPlusDetailPaneView(this);
            View.OnInit();

            this.DefaultStyleKey = typeof(XamOverviewPlusDetailPane);



        }

        #region Events

        /// <summary>
        /// Occurs when window is changing.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs<Rect>> WindowChanging;

        /// <summary>
        /// Raises the <see cref="E:WindowChanging"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Infragistics.Controls.PropertyChangedEventArgs&lt;T&gt;"/> instance containing the event data.</param>
        protected virtual void OnWindowChanging(PropertyChangedEventArgs<Rect> e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (this.Immediate)
            {
                this.Window = e.NewValue;
            }

            if (this.WindowChanging != null)
            {
                this.WindowChanging(this, e);
            }
        }

        /// <summary>
        /// Occurs when window is changed.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs<Rect>> WindowChanged;

        /// <summary>
        /// Raises the <see cref="E:WindowChanged"/> event.
        /// </summary>
        /// <param name="a">The <see cref="Infragistics.Controls.PropertyChangedEventArgs&lt;T&gt;"/> instance containing the event data.</param>
        protected internal virtual void OnWindowChanged(PropertyChangedEventArgs<Rect> a)
        {
            if (this.WindowChanged != null)
            {
                this.WindowChanged(this, a);
            }
        }

        /// <summary>
        /// Occurs when the actual size of the thumbnail changes.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs<Size>> ThumbnailSizeChanged;
        /// <summary>
        /// Method called when the OverviewPlusDetailPane thumbnail size is changed.
        /// </summary>
        /// <param name="e">The EventArgs in context.</param>
        protected internal virtual void OnThumbnailSizeChanged(PropertyChangedEventArgs<Size> e)
        {
            if (ThumbnailSizeChanged != null)
            {
                ThumbnailSizeChanged(this, e);
            }
        }

        #endregion Events

        #region Properties

        #region Dependencies

        #region Immediate Dependency Property

        /// <summary>
        /// Sets or gets the Immediate property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        /// <remarks>
        /// In immediate mode, the thumbnail directly modifies its own Window rectangle.
        /// </remarks>
        public bool Immediate
        {
            get
            {
                return (bool)GetValue(ImmediateProperty);
            }
            set
            {
                SetValue(ImmediateProperty, value);
            }
        }

        private const string ImmediatePropertyName = "Immediate";

        /// <summary>
        /// Identifies the Immediate dependency property.
        /// </summary>
        public static readonly DependencyProperty ImmediateProperty = DependencyProperty.Register(ImmediatePropertyName, typeof(bool), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata(true, (sender, e) =>
            {
                (sender as XamOverviewPlusDetailPane).OnPropertyChanged(new PropertyChangedEventArgs<bool>(ImmediatePropertyName, (bool)e.OldValue, (bool)e.NewValue));
            }));

        #endregion Immediate Dependency Property

        #region World Dependency Property

        /// <summary>
        /// Sets or gets the World property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        /// <remarks>
        /// The world rectangle defines the base coordinate system for the Window property, and
        /// is also controls the shape of the visible world rectangle in the thumbnail control (adjusted to
        /// fit whilst maintaining aspect ratio)
        /// </remarks>
        public Rect World
        {
            get
            {
                return (Rect)GetValue(WorldProperty);
            }
            set
            {
                SetValue(WorldProperty, value);
            }
        }

        private const string WorldPropertyName = "World";

        /// <summary>
        /// Identifies the World dependency property.
        /// </summary>
        public static readonly DependencyProperty WorldProperty = DependencyProperty.Register(WorldPropertyName, typeof(Rect), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata(Rect.Empty, (sender, e) =>
            {
                (sender as XamOverviewPlusDetailPane).OnPropertyChanged(new PropertyChangedEventArgs<Rect>(WorldPropertyName, (Rect)e.OldValue, (Rect)e.NewValue));
            }));

        #endregion World Dependency Property

        #region WorldStyle Dependency Property

        /// <summary>
        /// Sets or gets the WorldStyle property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        /// <remarks>
        /// The world rectangle defines the base coordinate system for the Window property, and
        /// is also controls the shape of the visible world rectangle in the thumbnail control (adjusted to
        /// fit whilst maintaining aspect ratio)
        /// </remarks>
        public Style WorldStyle
        {
            get
            {
                return (Style)GetValue(WorldStyleProperty);
            }
            set
            {
                SetValue(WorldStyleProperty, value);
            }
        }

        internal const string WorldStylePropertyName = "WorldStyle";

        /// <summary>
        /// Identifies the WorldStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty WorldStyleProperty = DependencyProperty.Register(WorldStylePropertyName, typeof(Style), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata((sender, e) =>
            {
                (sender as XamOverviewPlusDetailPane).OnPropertyChanged(new PropertyChangedEventArgs<Style>(WorldStylePropertyName, e.OldValue as Style, e.NewValue as Style));
            }));

        #endregion WorldStyle Dependency Property

        #region Window Dependency Property

        /// <summary>
        /// Sets or gets the Window property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Rect Window
        {
            get
            {
                return (Rect)GetValue(WindowProperty);
            }
            set
            {
                SetValue(WindowProperty, value);
            }
        }

        private const string WindowPropertyName = "Window";

        /// <summary>
        /// Identifies the Window dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowProperty = DependencyProperty.Register(WindowPropertyName, typeof(Rect), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata(Rect.Empty, (sender, e) =>
            {
                (sender as XamOverviewPlusDetailPane).OnPropertyChanged(new PropertyChangedEventArgs<Rect>(WindowPropertyName, (Rect)e.OldValue, (Rect)e.NewValue));
            }));

        #endregion Window Dependency Property

        #region WindowStyle Dependency Property

        /// <summary>
        /// Sets or gets the WindowStyle property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        /// <remarks>
        /// The world rectangle defines the base coordinate system for the Window property, and
        /// is also controls the shape of the visible world rectangle in the thumbnail control (adjusted to
        /// fit whilst maintaining aspect ratio)
        /// </remarks>
        public Style WindowStyle
        {
            get
            {
                return (Style)GetValue(WindowStyleProperty);
            }
            set
            {
                SetValue(WindowStyleProperty, value);
            }
        }

        internal const string WindowStylePropertyName = "WindowStyle";

        /// <summary>
        /// Identifies the WindowStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowStyleProperty = DependencyProperty.Register(WindowStylePropertyName, typeof(Style), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata((sender, e) =>
            {
                (sender as XamOverviewPlusDetailPane).OnPropertyChanged(new PropertyChangedEventArgs<Style>(WindowStylePropertyName, e.OldValue as Style, e.NewValue as Style));
            }));

        #endregion WindowStyle Dependency Property

        #region Preview Dependency Property

        /// <summary>
        /// Sets or gets the Preview property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Rect Preview
        {
            get
            {
                return (Rect)GetValue(PreviewProperty);
            }
            set
            {
                SetValue(PreviewProperty, value);
            }
        }

        private const string PreviewPropertyName = "Preview";

        /// <summary>
        /// Identifies the Preview dependency property.
        /// </summary>
        public static readonly DependencyProperty PreviewProperty = DependencyProperty.Register(PreviewPropertyName, typeof(Rect), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata(Rect.Empty, (sender, e) =>
            {
                (sender as XamOverviewPlusDetailPane).OnPropertyChanged(new PropertyChangedEventArgs<Rect>(PreviewPropertyName, (Rect)e.OldValue, (Rect)e.NewValue));
            }));

        #endregion Preview Dependency Property

        #region PreviewStyle Dependency Property

        /// <summary>
        /// Sets or gets the PreviewStyle property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        /// <remarks>
        /// The world rectangle defines the base coordinate system for the Window property, and
        /// is also controls the shape of the visible world rectangle in the thumbnail control (adjusted to
        /// fit whilst maintaining aspect ratio)
        /// </remarks>
        public Style PreviewStyle
        {
            get
            {
                return (Style)GetValue(PreviewStyleProperty);
            }
            set
            {
                SetValue(PreviewStyleProperty, value);
            }
        }

        internal const string PreviewStylePropertyName = "PreviewStyle";

        /// <summary>
        /// Identifies the PreviewStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty PreviewStyleProperty = DependencyProperty.Register(PreviewStylePropertyName, typeof(Style), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata((sender, e) =>
            {
                (sender as XamOverviewPlusDetailPane).OnPropertyChanged(new PropertyChangedEventArgs<Style>(PreviewStylePropertyName, e.OldValue as Style, e.NewValue as Style));
            }));

        #endregion PreviewStyle Dependency Property

        #region ShrinkToThumbnail

        /// <summary>
        /// Gets or sets a value indicating whether is shrinking to thumbnail.
        /// </summary>
        /// <value><c>true</c> if is shrinking to thumbnail; otherwise, <c>false</c>.</value>
        public bool ShrinkToThumbnail
        {
            get { return (bool)GetValue(ShrinkToThumbnailProperty); }
            set { SetValue(ShrinkToThumbnailProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ShrinkToThumbnail"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShrinkToThumbnailProperty =
            DependencyProperty.Register("ShrinkToThumbnail", typeof(bool), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata(true, OnShrinkToThumbnailChanged));

        private static void OnShrinkToThumbnailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamOverviewPlusDetailPane)d).OnShrinkToThumbnailChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// ShrinkToThumbnailProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnShrinkToThumbnailChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                this.GoToState(OverviewPlusDetailPaneMode.Minimal);
            }else
            {
                this.GoToState(OverviewPlusDetailPaneMode.Compact);
            }
        }

        #endregion ShrinkToThumbnail

        #region Mode

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        public OverviewPlusDetailPaneMode Mode
        {
            get { return (OverviewPlusDetailPaneMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Mode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(OverviewPlusDetailPaneMode), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata(OverviewPlusDetailPaneMode.Minimal, OnModeChanged));

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamOverviewPlusDetailPane)d).OnModeChanged((OverviewPlusDetailPaneMode)e.OldValue, (OverviewPlusDetailPaneMode)e.NewValue);
        }

        /// <summary>
        /// ModeProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnModeChanged(OverviewPlusDetailPaneMode oldValue, OverviewPlusDetailPaneMode newValue)
        {
            GoToState(newValue);
        }

        internal void GoToState(OverviewPlusDetailPaneMode mode)
        {
            switch (mode)
            {
                case OverviewPlusDetailPaneMode.Full:
                    View.GoToFullState();
                    if (this.IsZoomable)
                    {
                        View.GoToZoomEnabledState();
                    }
                    else
                    {
                        View.GoToZoomDisabledState();
                    }
                    break;
                case OverviewPlusDetailPaneMode.Compact:
                    View.GoToMinimalState();
                    if (this.IsZoomable)
                    {
                        View.GoToZoomEnabledState();
                    }
                    else
                    {
                        View.GoToZoomDisabledState();
                    }
                    break;
                case OverviewPlusDetailPaneMode.Minimal:
                    View.GoToMinimalState();
                    View.GoToZoomDisabledState();
                    break;
            }
        }

        private void UpdateVisualState()
        {
            if (this.ShrinkToThumbnail)
            {
                this.GoToState(OverviewPlusDetailPaneMode.Minimal);
            }else
            {
                this.GoToState(OverviewPlusDetailPaneMode.Compact);
            }
        }

        #endregion Mode

        #endregion Dependencies

        #region Public

        private IOverviewPlusDetailControl _surfaceViewer;
        /// <summary>
        /// Gets the surface viewer.
        /// </summary>
        /// <value>The surface viewer.</value>
        public IOverviewPlusDetailControl SurfaceViewer
        {
            get
            {
                return _surfaceViewer;
            }



            internal set

            {
                _surfaceViewer = value;

                View.SetSurfaceBindings();
            }
        }


        /// <summary>
        /// Gets the preview canvas.
        /// </summary>
        /// <value>The preview canvas.</value>
        public Canvas PreviewCanvas
        {
            get { return View.PreviewCanvas; }
        }


        /// <summary>
        /// Gets the preview viewportd rect.
        /// </summary>
        /// <value>The preview viewportd rect.</value>
        public Rect PreviewViewportdRect
        {
            get
            {
                Rect rect = this.WorldToCanvas(this.World);

                return new Rect(0, 0, rect.Width, rect.Height);
            }
        }

        #region ZoomTo100ButtonVisibility

        /// <summary>
        /// Gets or sets the zoom to 100% button visibility.
        /// </summary>
        /// <value>The zoom to 100% button visibility.</value>
        public Visibility ZoomTo100ButtonVisibility
        {
            get { return (Visibility)GetValue(ZoomTo100ButtonVisibilityProperty); }
            set { SetValue(ZoomTo100ButtonVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ZoomTo100ButtonVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomTo100ButtonVisibilityProperty =
            DependencyProperty.Register("ZoomTo100ButtonVisibility", typeof(Visibility), typeof(XamOverviewPlusDetailPane),
              new PropertyMetadata(Visibility.Visible, (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated("ZoomTo100ButtonVisibility", e.OldValue, e.NewValue)));

        #endregion

        #region ScaleToFitButtonVisibility

        /// <summary>
        /// Gets or sets the scale to fit button visibility.
        /// </summary>
        /// <value>The scale to fit button visibility.</value>
        public Visibility ScaleToFitButtonVisibility
        {
            get { return (Visibility)GetValue(ScaleToFitButtonVisibilityProperty); }
            set { SetValue(ScaleToFitButtonVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ScaleToFitButtonVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleToFitButtonVisibilityProperty =
            DependencyProperty.Register("ScaleToFitButtonVisibility", typeof(Visibility), typeof(XamOverviewPlusDetailPane),
              new PropertyMetadata(Visibility.Visible, (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated("ScaleToFitButtonVisibility", e.OldValue, e.NewValue)));

        #endregion

        #region InteractionStatesToolVisibility

        /// <summary>
        /// Gets or sets the interaction states tool visibility.
        /// </summary>
        /// <value>The interaction states tool visibility.</value>
        public Visibility InteractionStatesToolVisibility
        {
            get { return (Visibility)GetValue(InteractionStatesToolVisibilityProperty); }
            set { SetValue(InteractionStatesToolVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="InteractionStatesToolVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InteractionStatesToolVisibilityProperty =
            DependencyProperty.Register("InteractionStatesToolVisibility", typeof(Visibility), typeof(XamOverviewPlusDetailPane),
              new PropertyMetadata(Visibility.Visible, (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated("InteractionStatesToolVisibility", e.OldValue, e.NewValue)));

        #endregion


        #region DragPanButtonToolTip

        /// <summary>
        /// Gets or sets the zoom to 100% button tooltip.
        /// </summary>
        /// <value>The zoom to 100% button tooltip.</value>
        public object DragPanButtonToolTip
        {
            get { return (object)GetValue(DragPanButtonToolTipProperty); }
            set { SetValue(DragPanButtonToolTipProperty, value); }
        }
#pragma warning disable 436
        /// <summary>
        /// Identifies the <see cref="DragPanButtonToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DragPanButtonToolTipProperty =
            DependencyProperty.Register("DragPanButtonToolTip", typeof(object), typeof(XamOverviewPlusDetailPane),
              new PropertyMetadata(SR.GetString("OPD_DragPan"), 
                  (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated("DragPanButtonToolTip", e.OldValue, e.NewValue)));
#pragma warning restore 436
        #endregion

        #region DragZoomButtonToolTip

        /// <summary>
        /// Gets or sets the zoom to 100% button tooltip.
        /// </summary>
        /// <value>The zoom to 100% button tooltip.</value>
        public object DragZoomButtonToolTip
        {
            get { return (object)GetValue(DragZoomButtonToolTipProperty); }
            set { SetValue(DragZoomButtonToolTipProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DragZoomButtonToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DragZoomButtonToolTipProperty =
            DependencyProperty.Register("DragZoomButtonToolTip", typeof(object), typeof(XamOverviewPlusDetailPane),
              new PropertyMetadata(SR.GetString("OPD_DragZoom"), 
                  (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated("DragZoomButtonToolTip", e.OldValue, e.NewValue)));

        #endregion

        #region ZoomInButtonToolTip

        /// <summary>
        /// Gets or sets the zoom to 100% button tooltip.
        /// </summary>
        /// <value>The zoom to 100% button tooltip.</value>
        public object ZoomInButtonToolTip
        {
            get { return (object)GetValue(ZoomInButtonToolTipProperty); }
            set { SetValue(ZoomInButtonToolTipProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ZoomInButtonToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomInButtonToolTipProperty =
            DependencyProperty.Register("ZoomInButtonToolTip", typeof(object), typeof(XamOverviewPlusDetailPane),
              new PropertyMetadata(SR.GetString("OPD_ZoomIn"), 
                  (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated("ZoomInButtonToolTip", e.OldValue, e.NewValue)));

        #endregion

        #region ZoomOutButtonToolTip

        /// <summary>
        /// Gets or sets the zoom to 100% button tooltip.
        /// </summary>
        /// <value>The zoom to 100% button tooltip.</value>
        public object ZoomOutButtonToolTip
        {
            get { return (object)GetValue(ZoomOutButtonToolTipProperty); }
            set { SetValue(ZoomOutButtonToolTipProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ZoomOutButtonToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomOutButtonToolTipProperty =
            DependencyProperty.Register("ZoomOutButtonToolTip", typeof(object), typeof(XamOverviewPlusDetailPane),
              new PropertyMetadata(SR.GetString("OPD_ZoomOut"), 
                  (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated("ZoomOutButtonToolTip", e.OldValue, e.NewValue)));

        #endregion

        #region ZoomTo100ButtonToolTip

        /// <summary>
        /// Gets or sets the zoom to 100% button tooltip.
        /// </summary>
        /// <value>The zoom to 100% button tooltip.</value>
        public object ZoomTo100ButtonToolTip
        {
            get { return (object)GetValue(ZoomTo100ButtonToolTipProperty); }
            set { SetValue(ZoomTo100ButtonToolTipProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ZoomTo100ButtonToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomTo100ButtonToolTipProperty =
            DependencyProperty.Register("ZoomTo100ButtonToolTip", typeof(object), typeof(XamOverviewPlusDetailPane),
              new PropertyMetadata(SR.GetString("OPD_ZoomTo100"), 
                  (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated("ZoomTo100ButtonToolTip", e.OldValue, e.NewValue)));

        #endregion

        #region ScaleToFitButtonToolTip

        /// <summary>
        /// Gets or sets the scale to fit button tooltip.
        /// </summary>
        /// <value>The scale to fit button tooltip.</value>
        public object ScaleToFitButtonToolTip
        {
            get { return (object)GetValue(ScaleToFitButtonToolTipProperty); }
            set { SetValue(ScaleToFitButtonToolTipProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ScaleToFitButtonToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleToFitButtonToolTipProperty =
            DependencyProperty.Register("ScaleToFitButtonToolTip", typeof(object), typeof(XamOverviewPlusDetailPane),
              new PropertyMetadata(SR.GetString("OPD_ScaleToFit"),
                  (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated("ScaleToFitButtonToolTip", e.OldValue, e.NewValue)));

        #endregion

        #region DefaultInteractionButtonToolTip

        private const string DefaultInteractionButtonToolTipPropertyName="DefaultInteractionButtonToolTip";

        /// <summary>
        /// Identifies the <see cref="DefaultInteractionButtonToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultInteractionButtonToolTipProperty =
            DependencyProperty.Register(DefaultInteractionButtonToolTipPropertyName, typeof(object), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata(SR.GetString("OPD_DefaultInteraction"),
                (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated(DefaultInteractionButtonToolTipPropertyName, e.OldValue, e.NewValue)));
        /// <summary>
        /// The content to display as the tooltip over the button which changes the default interaction of the host control.
        /// </summary>
        public object DefaultInteractionButtonToolTip
        {
            get { return GetValue(DefaultInteractionButtonToolTipProperty); }
            set { this.SetValue(DefaultInteractionButtonToolTipProperty, value); }
        }


        #endregion


        #region ZoomLevelLargeChange
        private const string ZoomLevelLargeChangePropertyName = "ZoomLevelLargeChange";
        /// <summary>
        /// Identifies the ZoomLevelLargeChange dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomLevelLargeChangeProperty = 
            DependencyProperty.Register(ZoomLevelLargeChangePropertyName, typeof(double), typeof(XamOverviewPlusDetailPane), 
            new PropertyMetadata(1.0, (o, e) => ((XamOverviewPlusDetailPane)o).OnPropertyUpdated(ZoomLevelLargeChangePropertyName, e.OldValue, e.NewValue)));
        /// <summary>
        /// The amount to increment the zoom level by when an interaction by the end user calls for a large change.
        /// </summary>
        public double ZoomLevelLargeChange
        {
            get
            {
                return (double)this.GetValue(ZoomLevelLargeChangeProperty);
            }
            set
            {
                this.SetValue(ZoomLevelLargeChangeProperty, value);
            }
        }
        #endregion

        #endregion Public

        #region Private


        #region InnerHorizontalAlignment

        private HorizontalAlignment InnerHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(InnerHorizontalAlignmentProperty); }
            set { SetValue(InnerHorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="InnerHorizontalAlignment"/> dependency property.
        /// </summary>
        internal static readonly DependencyProperty InnerHorizontalAlignmentProperty =
            DependencyProperty.Register("InnerHorizontalAlignment", typeof(HorizontalAlignment), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata(HorizontalAlignment.Stretch, OnInnerHorizontalAlignmentChanged));

        private static void OnInnerHorizontalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamOverviewPlusDetailPane)d).OnInnerHorizontalAlignmentChanged((HorizontalAlignment)e.OldValue, (HorizontalAlignment)e.NewValue);
        }

        /// <summary>
        /// InnerHorizontalAlignmentProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnInnerHorizontalAlignmentChanged(HorizontalAlignment oldValue, HorizontalAlignment newValue)
        {
            SetRenderOrigin();
        }

        #endregion

        #region InnerVerticalAlignment

        private VerticalAlignment InnerVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(InnerVerticalAlignmentProperty); }
            set { SetValue(InnerVerticalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="InnerVerticalAlignment"/> dependency property.
        /// </summary>
        internal static readonly DependencyProperty InnerVerticalAlignmentProperty =
            DependencyProperty.Register("InnerVerticalAlignment", typeof(VerticalAlignment), typeof(XamOverviewPlusDetailPane),
            new PropertyMetadata(VerticalAlignment.Stretch, OnInnerVerticalAlignmentChanged));

        private static void OnInnerVerticalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamOverviewPlusDetailPane)d).OnInnerVerticalAlignmentChanged((VerticalAlignment)e.OldValue, (VerticalAlignment)e.NewValue);
        }

        /// <summary>
        /// InnerVerticalAlignmentProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnInnerVerticalAlignmentChanged(VerticalAlignment oldValue, VerticalAlignment newValue)
        {
            SetRenderOrigin();
        }

        #endregion

        #region IsZoomable
        private bool _isZoomable;
        /// <summary>
        /// Boolean indicating whether or not zooming is enabled on the OverviewPlusDetailPane.
        /// </summary>
        public bool IsZoomable
        {
            get
            {
                return _isZoomable;
            }
            set { 
                _isZoomable = value;
                this.UpdateVisualState();
            }
        }
        #endregion

        /// <summary>
        /// Gets the viewport.
        /// </summary>
        /// <value>The viewport.</value>
        public Rect Viewport
        {
            get
            {
                return View.Viewport;
            }
        }

        private bool RefreshPending { get; set; }

        #endregion Private

        #endregion Properties

        #region Overrides


        internal DependencyObject FetchChildByName(string name)
        {
            return GetTemplateChild(name);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass)
        /// call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// In simplest terms, this means the method is called just before a
        /// UI element displays in an application. For more information, see Remarks.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            View.OnTemplateProvided();

            this.UpdateVisualState();
        }

        /// <summary>
        /// Provides the behavior for the Measure pass of Silverlight layout. Classes can override this method to define their own Measure pass behavior.
        /// </summary>
        /// <param name="availableSize">The available size that this object can give to child objects. Infinity (<see cref="F:System.Double.PositiveInfinity"/>) can be specified as a value to indicate that the object will size to whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based on its calculations of the allocated sizes for child objects; or based on other considerations, such as a fixed container size.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            base.MeasureOverride(availableSize);
            Size desiredSize = GetDesiredSize(availableSize);
            return desiredSize;
        }





        internal Size GetDesiredSize(Size availableSize)

        {
            // the idea is to try to get a size which matches the aspect ratio of the world
            // as closely as possible.

            double width = availableSize.Width;
            double height = availableSize.Height;
            double aspect = !World.IsEmpty ? World.Width / World.Height : 1.0;

            if (double.IsInfinity(height))
            {
                if (double.IsInfinity(width))
                {
                    width = 128.0;
                }

                height = width / aspect;
            }
            else
            {
                if (double.IsInfinity(width))
                {
                    width = height * aspect;
                }
            }

            return new Size(width, height);
        }

        private bool IsPanning { get; set; }
        private Point Anchor { get; set; }

        internal void OnMouseEnter()
        {
            if (this.ShrinkToThumbnail)
            {
                this.GoToState(OverviewPlusDetailPaneMode.Full);
            }
        }

        internal void OnMouseLeave()
        {
            if (this.ShrinkToThumbnail)
            {
                this.GoToState(OverviewPlusDetailPaneMode.Minimal);
            }
        }

        internal bool OnKeyDown(Key key)
        {
            switch (key)
            {
                case Key.Escape:
                    if (this.IsPanning)
                    {
                        View.CancelMouseOperations();
                        this.IsPanning = false;

                        //Rect viewport = Viewport;
                        //Rect world = !viewport.IsEmpty ? World : Rect.Empty;
                        //Rect window = !world.IsEmpty ? Window : Rect.Empty;
                        //Rect preview = new Rect(this.Anchor.X - 0.5 * window.Width, this.Anchor.Y - 0.5 * window.Height, window.Width, window.Height);

                        // generate a window changing event (Empty)
                        Preview = Rect.Empty;
                        return true;
                    }
                    break;
            }
            return false;
        }

        internal void OnMouseLeftButtonDown(Point p)
        {
            if (!_pinching)
            {
                _ignoreContactUp = false;
            }

            _lastPosition = p;
            Rect canvasToWorld = WorldToCanvas(World);

            if (canvasToWorld.Contains(p) == false)
            {
                return;
            }

            Rect viewport = Viewport;
            Rect world = !viewport.IsEmpty ? World : Rect.Empty;
            Rect window = !world.IsEmpty ? Window : Rect.Empty;

            this.Anchor = CanvasToWorld(p);

            if (window.IsEmpty == false && View.CaptureMouse())
            {
                this.IsPanning = true;
                
                window = new Rect(this.Anchor.X - 0.5 * window.Width, this.Anchor.Y - 0.5 * window.Height, window.Width, window.Height);




                    OnWindowChanging(new PropertyChangedEventArgs<Rect>(WindowPropertyName, Window, window));



            }
        }

        Point _lastPosition;

        private const double DRAG_DISTANCE = 10.0;
        private const double DRAG_DISTANCE_NEAR = 2.0;

        internal void OnMouseMove(Point p, bool onMouseMove, bool isFinger)
        {
            if (double.IsNaN(Anchor.X) || double.IsNaN(Anchor.Y))
            {
                this.Anchor = CanvasToWorld(p);
            }

            double distance = DRAG_DISTANCE_NEAR;
            if (isFinger)
            {
                distance = DRAG_DISTANCE;
            }
            bool farFromAnchor = false;
            var a = WorldPointToCanvas(this.Anchor);
            Rect rect = new Rect(a, p);

            if (rect.Width > distance && rect.Height > distance)
            {
                farFromAnchor = true;
            }

            if (!_pinching)
            {
                if (farFromAnchor)
                {
                    _ignoreContactUp = false;
                }
            }

            _lastPosition = p;
            Rect viewport = Viewport;
            Rect world = !viewport.IsEmpty ? World : Rect.Empty;
            Rect window = !world.IsEmpty ? Window : Rect.Empty;

            if (this.IsPanning)
            {
                Point center = CanvasToWorld(p);

                window = new Rect(center.X - 0.5 * window.Width, center.Y - 0.5 * window.Height, window.Width, window.Height);

                OnWindowChanging(new PropertyChangedEventArgs<Rect>(WindowPropertyName, Window, window));
            }
        }

        internal bool OnMouseLeftButtonUp(Point p)
        {
            _lastPosition = p;
            Rect viewport = Viewport;
            Rect world = !viewport.IsEmpty ? World : Rect.Empty;
            Rect window = !world.IsEmpty ? Window : Rect.Empty;

            if (this.IsPanning && !_ignoreContactUp)
            {
                View.CancelMouseOperations();
                this.IsPanning = false;

                Point center = CanvasToWorld(p);
                window = new Rect(center.X - 0.5 * window.Width, center.Y - 0.5 * window.Height, window.Width, window.Height);




                    OnWindowChanged(new PropertyChangedEventArgs<Rect>(WindowPropertyName, Window, window));



                return true;
            }

            return false;
        }

        internal bool OnMouseWheel(double delta)
        {
            Point center = this.Window.GetCenter();

            double scale = 1.0 - MathUtil.Clamp(delta, -0.5, 0.5);
            double left = center.X - scale * (center.X - Window.Left);
            double bottom = center.Y + scale * (Window.Bottom - center.Y);
            double right = center.X + scale * (Window.Right - center.X);
            double top = center.Y - scale * (center.Y - Window.Top);

            Rect window = new Rect(left, top, right - left, bottom - top);
            OnWindowChanged(new PropertyChangedEventArgs<Rect>(WindowPropertyName, Window, window));

            return true;
        }

        private bool _ignoreContactUp = false;



#region Infragistics Source Cleanup (Region)





































































#endregion // Infragistics Source Cleanup (Region)

        #endregion Overrides

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Method invoked whenever a property value is changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        /// <param name="oldValue">The previous value of the changed property.</param>
        /// <param name="newValue">The new value of the changed property.</param>
        protected virtual void OnPropertyUpdated(string propertyName, object oldValue, object newValue)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }

            switch (e.PropertyName)
            {
                case WorldPropertyName:

                    UpdateLayout();

                    Refresh(false);
                    break;

                case WindowPropertyName:
                    Refresh(false);
                    break;

                case PreviewPropertyName:
                    Refresh(false);
                    break;
            }
        }

        #endregion INotifyPropertyChanged

        #region Methods

        private void SetRenderOrigin()
        {
            View.SetRenderOrigin();
        }
       
        private Point CanvasToWorld(Point point)
        {
            Rect viewport = Viewport;
            Rect world = !viewport.IsEmpty ? World : Rect.Empty;

            if (!world.IsEmpty)
            {
                double s = viewport.Width / viewport.Height > world.Width / world.Height ? viewport.Height / world.Height : viewport.Width / world.Width;
                double tx = 0.5 * ((viewport.Left + viewport.Right) - (world.Left + world.Right) * s);
                double ty = 0.5 * ((viewport.Top + viewport.Bottom) - (world.Top + world.Bottom) * s);

                return new Point((point.X - tx) / s, (point.Y - ty) / s);
            }

            return new Point(double.NaN, double.NaN);
        }

        private Point WorldPointToCanvas(Point pt)
        {
            Rect viewport = this.Viewport;
            Rect world = !viewport.IsEmpty ? World : Rect.Empty;

            if (!world.IsEmpty)
            {
                double s = viewport.Width / viewport.Height > world.Width / world.Height ? viewport.Height / world.Height : viewport.Width / world.Width;
                double tx = 0.5 * ((viewport.Left + viewport.Right) - (world.Left + world.Right) * s);
                double ty = 0.5 * ((viewport.Top + viewport.Bottom) - (world.Top + world.Bottom) * s);

                return new Point(pt.X * s + tx, pt.Y * s + ty);
            }

            return new Point(double.NaN, double.NaN);
        }

        private Rect WorldToCanvas(Rect rect)
        {
            Rect viewport = this.Viewport;
            Rect world = !viewport.IsEmpty ? World : Rect.Empty;

            if (!world.IsEmpty && !rect.IsEmpty)
            {
                double s = viewport.Width / viewport.Height > world.Width / world.Height ? viewport.Height / world.Height : viewport.Width / world.Width;
                double tx = 0.5 * ((viewport.Left + viewport.Right) - (world.Left + world.Right) * s);
                double ty = 0.5 * ((viewport.Top + viewport.Bottom) - (world.Top + world.Bottom) * s);

                return new Rect(rect.Left * s + tx, rect.Top * s + ty, rect.Width * s, rect.Height * s);
            }

            return Rect.Empty;
        }




        internal void Refresh(bool immediate)

        {
            UpdateSliderRanges();

            if (!immediate)
            {
                if (!this.RefreshPending)
                {
                    this.RefreshPending = true;



                    this.Dispatcher.BeginInvoke(new Action<bool>(Refresh), true);

                }

                return;
            }
            this.RefreshPending = false;

            Rect world = WorldToCanvas(World);
            Rect windowRect = WorldToCanvas(Window);
            Rect preview = WorldToCanvas(Preview);

            if (this.Visibility != Visibility.Visible ||
                world.IsEmpty ||
                double.IsNaN(world.X) ||
                double.IsNaN(world.Y))
            {
                this.RefreshPending = false;

                return;
            }

            View.UpdateWorldRect(world);

            View.PositionPreview(world);
            
            View.UpdateWindowPath(world, windowRect);
            View.UpdatePreviewPath(world, preview);
            

            if (this.SurfaceViewer != null)
            {
                this.SurfaceViewer.RenderPreview();
            }



        }
        /// <summary>
        /// Boolean indicating whether or not the slider ranges are currently being updated.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool UpdatingSliderRanges { get; private set; }
        private void UpdateSliderRanges()
        {
            
            if (this.Visibility != Visibility.Visible ||
                this.SurfaceViewer == null ||
                !View.IsReady())
            {
                return;
            }

            Rect viewportRect = this.SurfaceViewer.ViewportRect;
            Rect worldRect = this.SurfaceViewer.WorldRect;

            if (viewportRect.IsEmpty || worldRect.IsEmpty)
            {
                return;
            }
            this.UpdatingSliderRanges = true;
            double currentValue = View.GetSliderValue();
            double min = this.SurfaceViewer.MinimumZoomLevel;
            double max = this.SurfaceViewer.MaximumZoomLevel;

            // slider minimum
            if (double.IsNaN(min))
            {
                min = Math.Min(viewportRect.Width / worldRect.Width, viewportRect.Height / worldRect.Height);
                min = Math.Min(min, 0.5);
                min = Math.Min(min, currentValue);
            }

            // slider maximum            
            if (double.IsNaN(max))
            {
                max = MaxZoom;
                max = Math.Max(max, currentValue);
            }

            View.SetSliderMin(min);
            View.SetSliderMax(max);
            this.UpdatingSliderRanges = false;
        }

        

        #endregion Methods

        internal void OnDefaultInteraction(InteractionState state)
        {
            if (this.SurfaceViewer != null)
            {
                this.SurfaceViewer.DefaultInteraction = state;
            }
        }

        internal void OnScaleToFit()
        {
            if (this.SurfaceViewer != null)
            {
                this.SurfaceViewer.ScaleToFit();
            }
        }

        internal void OnZoomTo100()
        {
            if (this.SurfaceViewer != null)
            {
                this.SurfaceViewer.ZoomTo100();
            }
        }

        internal void OnContainerSizeChanged()
        {
            Refresh(false);
        }



#region Infragistics Source Cleanup (Region)



















































#endregion // Infragistics Source Cleanup (Region)


        private bool _pinching;




        internal 

            bool Pinching { get { return _pinching; } }

        internal void OnGestureCompleted(Point pt)
        {
            _pinching = false;
            Anchor = new Point(double.NaN, double.NaN);
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