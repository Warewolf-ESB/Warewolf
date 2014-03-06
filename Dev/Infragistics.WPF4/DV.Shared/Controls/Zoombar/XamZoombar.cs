using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls
{


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

    /// <summary>
    /// The Zoombar control class
    /// </summary>    
    [TemplatePart(Name = XamZoombar.HorizontalRootCanvasElementName, Type = typeof(Canvas))]
    [TemplatePart(Name = XamZoombar.HorizontalPreviewElementName, Type = typeof(Panel))]
    [TemplatePart(Name = XamZoombar.HorizontalScrollbarElementName, Type = typeof(Grid))]
    [TemplatePart(Name = XamZoombar.HorizontalTrackElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamZoombar.HorizontalScrollLeftElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamZoombar.HorizontalScrollRightElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamZoombar.HorizontalThumbElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamZoombar.HorizontalScaleLeftElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamZoombar.HorizontalScaleRightElementName, Type = typeof(FrameworkElement))]

    [TemplatePart(Name = XamZoombar.VerticalRootCanvasElementName, Type = typeof(Canvas))]
    [TemplatePart(Name = XamZoombar.VerticalPreviewElementName, Type = typeof(Panel))]
    [TemplatePart(Name = XamZoombar.VerticalScrollbarElementName, Type = typeof(Grid))]
    [TemplatePart(Name = XamZoombar.VerticalTrackElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamZoombar.VerticalScrollTopElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamZoombar.VerticalScrollBottomElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamZoombar.VerticalThumbElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamZoombar.VerticalScaleTopElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = XamZoombar.VerticalScaleBottomElementName, Type = typeof(FrameworkElement))]

    [StyleTypedProperty(Property = "HorizontalScaleLeftStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "HorizontalScaleRightStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "HorizontalThumbStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "HorizontalScrollLeftStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "HorizontalScrollRightStyle", StyleTargetType = typeof(ContentControl))]

    [StyleTypedProperty(Property = "VerticalScaleTopStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "VerticalScaleBottomStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "VerticalThumbStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "VerticalScrollTopStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "VerticalScrollBottomStyle", StyleTargetType = typeof(ContentControl))]

	
	

    public class XamZoombar : InteractiveControl
    {
        #region Constants

        // horizontal
        private const string HorizontalRootCanvasElementName = "HorizontalRootCanvasElement";

        private const string HorizontalPreviewElementName = "HorizontalPreviewElement";

        private const string HorizontalScrollbarElementName = "HorizontalScrollbarElement";
        private const string HorizontalTrackElementName = "HorizontalTrackElement";

        private const string HorizontalScrollLeftElementName = "HorizontalScrollLeftElement";
        private const string HorizontalScrollRightElementName = "HorizontalScrollRightElement";

        private const string HorizontalThumbElementName = "HorizontalThumbElement";

        private const string HorizontalScaleLeftElementName = "HorizontalScaleLeftElement";
        private const string HorizontalScaleRightElementName = "HorizontalScaleRightElement";

        // vertical
        private const string VerticalRootCanvasElementName = "VerticalRootCanvasElement";

        private const string VerticalPreviewElementName = "VerticalPreviewElement";

        private const string VerticalScrollbarElementName = "VerticalScrollbarElement";
        private const string VerticalTrackElementName = "VerticalTrackElement";

        private const string VerticalScrollTopElementName = "VerticalScrollTopElement";
        private const string VerticalScrollBottomElementName = "VerticalScrollBottomElement";

        private const string VerticalThumbElementName = "VerticalThumbElement";

        private const string VerticalScaleTopElementName = "VerticalScaleTopElement";
        private const string VerticalScaleBottomElementName = "VerticalScaleBottomElement";

        private const string FormatString = "{0} Minimum:{1} Maximum:{2} Range.Minimum:{3} Range.Maximum:{4}";

        #endregion

        #region Fields

        // horizontal
        private Canvas horizontalRootCanvas;

        private ShapeElement horizontalThumbShape;

        private ShapeElement horizontalScaleLeftShape;
        private ShapeElement horizontalScaleRightShape;

        private FrameworkElement horizontalScrollLeftElement;
        private FrameworkElement horizontalScrollRightElement;

        private Grid horizontalScrollbarElement;
        private Panel horizontalPreviewPanel;

        private ScrollShape horizontalScrollLeftShape;
        private ScrollShape horizontalScrollRightShape;

        private HorizontalThumbNode horizontalThumbNode;
        private TrackShape horizontalTrackShape;

        private List<VisualElement> horizontalVisualElements;

        // vertical
        private Canvas verticalRootCanvas;

        private ShapeElement verticalThumbShape;

        private ShapeElement verticalScaleTopShape;
        private ShapeElement verticalScaleBottomShape;

        private FrameworkElement verticalScrollTopElement;
        private FrameworkElement verticalScrollBottomElement;

        private Grid verticalScrollbarElement;
        private Panel verticalPreviewPanel;

        private ScrollShape verticalScrollTopShape;
        private ScrollShape verticalScrollBottomShape;

        private VerticalThumbNode verticalThumbNode;
        private TrackShape verticalTrackShape;

        private List<VisualElement> verticalVisualElements;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="XamZoombar"/> class.
        /// </summary>
        public XamZoombar()
        {
            this.DefaultStyleKey = typeof(XamZoombar);

            this.SetBinding(NewStyleProperty, new Binding("Style") { Source = this });

            // init thumb nodes
            this.horizontalThumbNode = new HorizontalThumbNode();
            this.VisualElements.Add(this.horizontalThumbNode);

            this.verticalThumbNode = new VerticalThumbNode();
            this.VisualElements.Add(this.verticalThumbNode);

            // init timer
            this.Timer = new DispatcherTimer();
            this.Timer.Tick += new EventHandler(timer_Tick);

            this.Timer.Interval = TimeSpan.FromMilliseconds(250);

            // init tools
            this.MouseDownTools.Add(new TrackbarTool(this));
            this.MouseDownTools.Add(new ResizingTool(this));
            this.MouseDownTools.Add(new DraggingTool(this));

            // init collections
            this.horizontalVisualElements = new List<VisualElement>();
            this.verticalVisualElements = new List<VisualElement>();

            this.Initialize = true;

            this.Range = new Range();

            this.TempRange = new Range();
            this.TempRange.Zoombar = this;
            this.TempRange.Initialize = true;

            this.SizeChanged += new SizeChangedEventHandler(XamZoombar_SizeChanged);
            this.LayoutUpdated += new EventHandler(XamZoombar_LayoutUpdated);


			Infragistics.Windows.Utilities.ValidateLicense(typeof(XamZoombar), this);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        }

        /// <summary>
        /// Occurs when zoom is changed.
        /// </summary>
        public event EventHandler<ZoomChangedEventArgs> ZoomChanged;
        /// <summary>
        /// Occurs when zoom is changing.
        /// </summary>
        public event EventHandler<ZoomChangeEventArgs> ZoomChanging;

        /// <summary>
        /// Gets the size of the preview content.
        /// </summary>
        /// <value>The size of the preview content.</value>
        public Size PreviewContentSize
        {
            get
            {
                Panel panel;

                if (this.Orientation == Orientation.Horizontal)
                {
                    panel = this.horizontalPreviewPanel;
                }
                else
                {
                    panel = this.verticalPreviewPanel;
                }

                double width = panel.ActualWidth;
                double height = panel.ActualHeight;

                return new Size(width, height);
            }
        }

        #region HorizontalPreviewContent

        /// <summary>
        /// Gets or sets the content of the horizontal preview.
        /// </summary>
        /// <value>The content of the horizontal preview.</value>
        public object HorizontalPreviewContent
        {
            get { return (object)GetValue(HorizontalPreviewContentProperty); }
            set { SetValue(HorizontalPreviewContentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalPreviewContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalPreviewContentProperty =
            DependencyProperty.Register("HorizontalPreviewContent", typeof(object), typeof(XamZoombar), null);

        #endregion

        #region VerticalPreviewContent

        /// <summary>
        /// Gets or sets the content of the vertical preview.
        /// </summary>
        /// <value>The content of the vertical preview.</value>
        public object VerticalPreviewContent
        {
            get { return (object)GetValue(VerticalPreviewContentProperty); }
            set { SetValue(VerticalPreviewContentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalPreviewContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalPreviewContentProperty =
            DependencyProperty.Register("VerticalPreviewContent", typeof(object), typeof(XamZoombar), null);

        #endregion


        #region HorizontalScaleLeftStyle

        /// <summary>
        /// Gets or sets the horizontal scale left element style.
        /// </summary>
        /// <value>The horizontal scale left element style.</value>
        public Style HorizontalScaleLeftStyle
        {
            get { return (Style)GetValue(HorizontalScaleLeftStyleProperty); }
            set { SetValue(HorizontalScaleLeftStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalScaleLeftStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalScaleLeftStyleProperty =
            DependencyProperty.Register("HorizontalScaleLeftStyle", typeof(Style), typeof(XamZoombar), null);

        #endregion

        #region HorizontalScaleRightStyle

        /// <summary>
        /// Gets or sets the horizontal scale right element style.
        /// </summary>
        /// <value>The horizontal scale right element style.</value>
        public Style HorizontalScaleRightStyle
        {
            get { return (Style)GetValue(HorizontalScaleRightStyleProperty); }
            set { SetValue(HorizontalScaleRightStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalScaleRightStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalScaleRightStyleProperty =
            DependencyProperty.Register("HorizontalScaleRightStyle", typeof(Style), typeof(XamZoombar), null);

        #endregion

        #region HorizontalThumbStyle

        /// <summary>
        /// Gets or sets the horizontal thumb element style.
        /// </summary>
        /// <value>The horizontal thumb element style.</value>
        public Style HorizontalThumbStyle
        {
            get { return (Style)GetValue(HorizontalThumbStyleProperty); }
            set { SetValue(HorizontalThumbStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalThumbStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalThumbStyleProperty =
            DependencyProperty.Register("HorizontalThumbStyle", typeof(Style), typeof(XamZoombar), null);

        #endregion


        #region HorizontalScrollLeftStyle

        /// <summary>
        /// Gets or sets the horizontal scroll left style.
        /// </summary>
        /// <value>The horizontal scroll left style.</value>
        public Style HorizontalScrollLeftStyle
        {
            get { return (Style)GetValue(HorizontalScrollLeftStyleProperty); }
            set { SetValue(HorizontalScrollLeftStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalScrollLeftStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollLeftStyleProperty =
            DependencyProperty.Register("HorizontalScrollLeftStyle", typeof(Style), typeof(XamZoombar),
              new PropertyMetadata(null));

        #endregion

        #region HorizontalScrollRightStyle

        /// <summary>
        /// Gets or sets the horizontal scroll right style.
        /// </summary>
        /// <value>The horizontal scroll right style.</value>
        public Style HorizontalScrollRightStyle
        {
            get { return (Style)GetValue(HorizontalScrollRightStyleProperty); }
            set { SetValue(HorizontalScrollRightStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalScrollRightStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollRightStyleProperty =
            DependencyProperty.Register("HorizontalScrollRightStyle", typeof(Style), typeof(XamZoombar),
              new PropertyMetadata(null));

        #endregion


        #region VerticalScaleTopStyle

        /// <summary>
        /// Gets or sets the vertical scale top element style.
        /// </summary>
        /// <value>The vertical scale top element style.</value>
        public Style VerticalScaleTopStyle
        {
            get { return (Style)GetValue(VerticalScaleTopStyleProperty); }
            set { SetValue(VerticalScaleTopStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalScaleTopStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalScaleTopStyleProperty =
            DependencyProperty.Register("VerticalScaleTopStyle", typeof(Style), typeof(XamZoombar), null);

        #endregion

        #region VerticalScaleBottomStyle

        /// <summary>
        /// Gets or sets the vertical scale bottom element style.
        /// </summary>
        /// <value>The vertical scale bottom element style.</value>
        public Style VerticalScaleBottomStyle
        {
            get { return (Style)GetValue(VerticalScaleBottomStyleProperty); }
            set { SetValue(VerticalScaleBottomStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalScaleBottomStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalScaleBottomStyleProperty =
            DependencyProperty.Register("VerticalScaleBottomStyle", typeof(Style), typeof(XamZoombar), null);

        #endregion

        #region VerticalThumbStyle

        /// <summary>
        /// Gets or sets the vertical thumb element style.
        /// </summary>
        /// <value>The vertical thumb element style.</value>
        public Style VerticalThumbStyle
        {
            get { return (Style)GetValue(VerticalThumbStyleProperty); }
            set { SetValue(VerticalThumbStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalThumbStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalThumbStyleProperty =
            DependencyProperty.Register("VerticalThumbStyle", typeof(Style), typeof(XamZoombar), null);

        #endregion


        #region VerticalScrollTopStyle

        /// <summary>
        /// Gets or sets the vertical scroll top style.
        /// </summary>
        /// <value>The vertical scroll top style.</value>
        public Style VerticalScrollTopStyle
        {
            get { return (Style)GetValue(VerticalScrollTopStyleProperty); }
            set { SetValue(VerticalScrollTopStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalScrollTopStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalScrollTopStyleProperty =
            DependencyProperty.Register("VerticalScrollTopStyle", typeof(Style), typeof(XamZoombar),
              new PropertyMetadata(null));

        #endregion

        #region VerticalScrollBottomStyle

        /// <summary>
        /// Gets or sets the vertical scroll bottom style.
        /// </summary>
        /// <value>The vertical scroll bottom style.</value>
        public Style VerticalScrollBottomStyle
        {
            get { return (Style)GetValue(VerticalScrollBottomStyleProperty); }
            set { SetValue(VerticalScrollBottomStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalScrollBottomStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBottomStyleProperty =
            DependencyProperty.Register("VerticalScrollBottomStyle", typeof(Style), typeof(XamZoombar),
              new PropertyMetadata(null));

        #endregion


        #region Minimum

        /// <summary>
        /// Gets or sets the minimum possible values of the Range element.
        /// </summary>
        /// <value>The minimum value. The default is 0.</value>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Minimum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(XamZoombar), new PropertyMetadata(0.0, new PropertyChangedCallback(OnMinimumChanged)));

        private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamZoombar zoombar = d as XamZoombar;

            zoombar.OnMinimumChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// MinimumProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMinimumChanged(double oldValue, double newValue)
        {
            if (this.Maximum < this.Minimum)
            {
                this.Maximum = this.Minimum;
                return;
            }

            this.UpdateThumb();
        }

        #endregion

        #region Maximum

        /// <summary>
        /// Gets or sets the maximum possible values of the Range element.
        /// </summary>        
        /// <value>The maximum value. The default is 1.</value>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Maximum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(XamZoombar), new PropertyMetadata(1.0, new PropertyChangedCallback(OnMaximumChanged)));

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamZoombar zoombar = d as XamZoombar;

            zoombar.OnMaximumChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// MinimumProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMaximumChanged(double oldValue, double newValue)
        {
            if (this.Minimum > this.Maximum)
            {
                this.Minimum = this.Maximum;
                return;
            }

            this.UpdateThumb();
        }

        #endregion

        #region Range

        /// <summary>
        /// Gets or sets the current Range element.
        /// </summary>
        /// <value>The range.</value>
        public Range Range
        {
            get { return (Range)GetValue(RangeProperty); }
            set { SetValue(RangeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Range"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RangeProperty =
            DependencyProperty.Register("Range", typeof(Range), typeof(XamZoombar), new PropertyMetadata(new PropertyChangedCallback(OnRangeChanged)));

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamZoombar zoombar = d as XamZoombar;
            zoombar.OnRangeChanged((Range)e.OldValue, (Range)e.NewValue);
        }

        internal void ChangeRange(Range oldRange, Range newRange)
        {
            OnRangeChanged(oldRange, newRange);
        }

        /// <summary>
        /// This code will run whenever Range changes, to change the template
        /// being used to display this control.
        /// </summary>
        /// <param name="oldRange">The old range.</param>
        /// <param name="newRange">The new range.</param>
        protected virtual void OnRangeChanged(Range oldRange, Range newRange)
        {
            if (newRange == null)
            {
                return;
            }

            newRange.Zoombar = this;
            this.UpdateThumb();

            if (this.ZoomCanceled)
            {
                this.ZoomCanceled = false;
                return;
            }

            if (this.Initialize == false)
            {
                ZoomChangeEventArgs eventArgs = new ZoomChangeEventArgs(newRange);
                OnZoomChanging(eventArgs);
                if (eventArgs.Cancel)
                {
                    this.ZoomCanceled = true;
                    this.Range = oldRange;
                    return;
                }
            }

            ZoomChangedEventArgs zoomChangedEventArgs = new ZoomChangedEventArgs(newRange);
            OnZoomChanged(zoomChangedEventArgs);

            this.ZoomCanceled = false;
        }

        private bool _zoomCanceled;
        private bool ZoomCanceled
        {
            get { return _zoomCanceled; }
            set { _zoomCanceled = value; }
        }

        #endregion

        #region SmallChange

        /// <summary>
        /// Gets or sets a value to be added to or subtracted from the values of the Range element.
        /// </summary>
        /// <remarks>
        /// When the user clicks the scroll elements of the Zoombar control, the Range element values
        /// would increase or decrease by the value of SmallChange.
        /// </remarks>
        /// <value>The small change value. The default is 0.1.</value>
        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="SmallChange"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register("SmallChange", typeof(double), typeof(XamZoombar), new PropertyMetadata(0.1, OnSmallChangeChanged));

        private static void OnSmallChangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamZoombar)d).OnSmallChangeChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// SmallChangeProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnSmallChangeChanged(double oldValue, double newValue)
        {
            if (!IsValidChange(newValue))
            {
                throw new ArgumentException("Zoombar_InvalidChangeValue", SmallChangeProperty.ToString());
            }
        }

        #endregion

        #region LargeChange

        /// <summary>
        /// Gets or sets a value to be added to or subtracted from the values of the Range element.
        /// </summary>
        /// <remarks>
        /// When the user clicks the track element of the Zoombar control, the Range element values
        /// would increase or decrease by the value of LargeChange.
        /// </remarks>
        /// <value>The large change value. The default is 0.2.</value>
        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="LargeChange"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register("LargeChange", typeof(double), typeof(XamZoombar), new PropertyMetadata(0.2, OnLargeChangeChanged));

        private static void OnLargeChangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamZoombar)d).OnLargeChangeChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// LargeChangeProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnLargeChangeChanged(double oldValue, double newValue)
        {
            if (!IsValidChange(newValue))
            {
                throw new ArgumentException("Zoombar_InvalidChangeValue", LargeChangeProperty.ToString());
            }
        }

        #endregion

        #region Orientation

        /// <summary> 
        /// Gets or sets whether the Zoombar has an orientation of vertical or horizontal. 
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(XamZoombar),
                new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamZoombar)d).OnOrientationChanged((Orientation)e.OldValue, (Orientation)e.NewValue);
        }

        /// <summary>
        /// OrientationProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnOrientationChanged(Orientation oldValue, Orientation newValue)
        {
            if (this.Initialize)
            {
                return;
            }

            if (this.verticalRootCanvas == null ||
                this.horizontalRootCanvas == null)
            {
                return;
            }

            if (this.Orientation == Orientation.Horizontal)
            {
                this.verticalRootCanvas.Visibility = Visibility.Collapsed;
                SetClipMode(verticalRootCanvas, true);
                SetClipMode(horizontalRootCanvas, false);
                this.horizontalRootCanvas.Visibility = Visibility.Visible;
                this.SetHorizontalVisualElements();
            }
            else
            {
                this.horizontalRootCanvas.Visibility = Visibility.Collapsed;
                SetClipMode(horizontalRootCanvas, true);
                SetClipMode(verticalRootCanvas, false);
                this.verticalRootCanvas.Visibility = Visibility.Visible;
                this.SetVerticalVisualElements();
            }


            this.UpdateThumb();
        }

        #endregion Orientation

        #region Overrides

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.horizontalThumbNode != null)
            {
                this.horizontalThumbNode.ClearChildren();
            }

            if (this.verticalThumbNode != null)
            {
                this.verticalThumbNode.ClearChildren();
            }

            InitHorionatalTemplate();
            InitVerticalTemplate();

            if (this.Orientation == Orientation.Horizontal)
            {
                SetClipMode(verticalRootCanvas, true);
                SetHorizontalVisualElements();
            }
            else
            {
                SetClipMode(horizontalRootCanvas, true);
                SetVerticalVisualElements();
            }
        }

        private void SetClipMode(FrameworkElement ele, bool clip)
        {

            if (ele != null)
            {
                ele.ClipToBounds = clip;
            }

        }

        /// <summary>
        /// Method invoked when the left mouse button is pressed over this control.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (!e.Handled)
            {
                e.Handled = true;



            }
        }

        /// <summary>
        /// Method invoked when the left mouse button is released over this control.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (!e.Handled)
            {
                e.Handled = true;
            }
        }


        /// <summary>
        /// Method invoked when a key is pressed while this control has focus.
        /// </summary>
        /// <param name="e">The KeyEventArgs in context.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (this.Orientation == Orientation.Vertical)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        MoveTo(this.Minimum, true);
                        break;
                    case Key.Down:
                        MoveTo(this.Maximum, true);
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Left:
                        MoveTo(this.Minimum, true);
                        break;
                    case Key.Right:
                        MoveTo(this.Maximum, true);
                        break;
                }
            }
        }
        /// <summary>
        /// Method invoked when a key is released while this control has focus.
        /// </summary>
        /// <param name="e">The KeyEventArgs in context.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    this.StopMove();
                    break;
            }
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
            Size size = base.MeasureOverride(availableSize);

            if (double.IsInfinity(availableSize.Width))
            {
                availableSize.Width = size.Width;
            }
            if (double.IsInfinity(availableSize.Height))
            {
                availableSize.Height = size.Height;
            }

            return availableSize;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            if (this.Range != null)
            {
                return string.Format(CultureInfo.InvariantCulture, FormatString, new object[] { base.ToString(), this.Minimum, this.Maximum, this.Range.Minimum, this.Range.Maximum });
            }

            return string.Format(CultureInfo.InvariantCulture, FormatString, new object[] { base.ToString(), this.Minimum, this.Maximum, this.Minimum, this.Maximum });
        }


        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the Silverlight automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new XamZoombarAutomationPeer(this);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Raises the <see cref="E:ZoomChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Infragistics.Controls.ZoomChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnZoomChanged(ZoomChangedEventArgs e)
        {
            if (this.ZoomChanged != null)
            {
                this.ZoomChanged(this, e);
            }
        }
        /// <summary>
        /// Raises the <see cref="E:ZoomChanging"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Infragistics.Controls.ZoomChangeEventArgs"/> instance containing the event data.</param>
        protected virtual void OnZoomChanging(ZoomChangeEventArgs e)
        {
            if (this.ZoomChanging != null)
            {
                this.ZoomChanging(this, e);
            }
        }

        #endregion

        #region Internal Operations

        #region NewStyle

        /// <summary>
        /// Used to handle the Style property changes.
        /// </summary>
        /// <value>The new style.</value>
        private Style NewStyle
        {
            get { return (Style)GetValue(NewStyleProperty); }
            set { SetValue(NewStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="NewStyle"/> dependency property.
        /// </summary>
        private static readonly DependencyProperty NewStyleProperty =
            DependencyProperty.Register("NewStyle", typeof(Style), typeof(XamZoombar),
            new PropertyMetadata(null, OnNewStyleChanged));

        private static void OnNewStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamZoombar)d).OnNewStyleChanged((Style)e.OldValue, (Style)e.NewValue);
        }

        /// <summary>
        /// NewStyleProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnNewStyleChanged(Style oldValue, Style newValue)
        {
            if (this.horizontalPreviewPanel != null &&
                this.horizontalPreviewPanel.Children.Count > 0)
            {
                ContentPresenter cp = this.horizontalPreviewPanel.Children[0] as ContentPresenter;
                if (cp != null && cp.Content != null)
                {
                    cp.Content = null;
                }
            }

            if (this.verticalPreviewPanel != null &&
                this.verticalPreviewPanel.Children.Count > 0)
            {
                ContentPresenter cp = this.verticalPreviewPanel.Children[0] as ContentPresenter;
                if (cp != null && cp.Content != null)
                {
                    cp.Content = null;
                }
            }
        }

        #endregion

        private bool _isSizeChanged;
        internal bool IsSizeChanged
        {
            get { return _isSizeChanged; }
            set { _isSizeChanged = value; }
        }

        private Range _tempRange;
        internal Range TempRange
        {
            get { return _tempRange; }
            set { _tempRange = value; }
        }

        internal double CalculateMinScale()
        {
            double minScale;

            if (this.Orientation == Orientation.Horizontal)
            {
                minScale = (this.LeftScaleWidth + this.RightScaleWidth) / this.ScrollAreaWidth;
            }
            else
            {
                minScale = (this.TopScaleHeight + this.BottomScaleHeight) / this.ScrollAreaHeight;
            }

            return minScale;
        }

        //internal double CalculateMinRangeDistance()
        //{
        //    double minScale = CalculateMinScale();

        //    double minRange = (this.Maximum - this.Minimum) * minScale;

        //    return minRange;
        //}
        internal double CalculateValue(Point pt)
        {
            double scroll;

            if (this.Orientation == Orientation.Horizontal)
            {
                double x = pt.X - this.LeftOffset;
                double width = this.ScrollAreaWidth;

                scroll = x / width;
            }
            else
            {
                double y = pt.Y - this.TopOffset;
                double height = this.ScrollAreaHeight;

                scroll = y / height;
            }

            double fullRange = this.Maximum - this.Minimum;
            double value = this.Minimum + fullRange * scroll;

            return value;
        }

        internal void RaiseZoomChangingEvent()
        {
            ZoomChangeEventArgs eventArgs = new ZoomChangeEventArgs(this.TempRange);
            OnZoomChanging(eventArgs);

            if (eventArgs.Cancel)
            {
                ICancelableTool tool = this.CurrentTool as ICancelableTool;

                if (tool != null)
                {
                    tool.IsCanceled = true;
                    tool.StopTool();
                }
            }
        }

        // horizontal
        private double leftScrollWidth;
        internal double LeftScrollWidth
        {
            get { return this.leftScrollWidth; }
            set { this.leftScrollWidth = value; }
        }

        private double rightScrollWidth;
        internal double RightScrollWidth
        {
            get { return this.rightScrollWidth; }
            set { this.rightScrollWidth = value; }
        }

        private double leftScaleWidth;
        internal double LeftScaleWidth
        {
            get { return this.leftScaleWidth; }
            set { this.leftScaleWidth = value; }
        }

        private double rightScaleWidth;
        internal double RightScaleWidth
        {
            get { return this.rightScaleWidth; }
            set { this.rightScaleWidth = value; }
        }

        private double scrollAreaWidth;
        internal double ScrollAreaWidth
        {
            get { return this.scrollAreaWidth; }
            set { this.scrollAreaWidth = value; }
        }

        internal double LeftOffset
        {
            get
            {
                return this.LeftScrollWidth;// -this.LeftScaleWidth / 2.0;
            }
        }
        internal double RightOffset
        {
            get
            {
                return this.RightScrollWidth;// -this.RightScaleWidth / 2.0;
            }
        }

        // vertical
        private double topScrollHeight;
        internal double TopScrollHeight
        {
            get { return this.topScrollHeight; }
            set { this.topScrollHeight = value; }
        }

        private double bottomScrollHeight;
        internal double BottomScrollHeight
        {
            get { return this.bottomScrollHeight; }
            set { this.bottomScrollHeight = value; }
        }

        private double topScaleHeight;
        internal double TopScaleHeight
        {
            get { return this.topScaleHeight; }
            set { this.topScaleHeight = value; }
        }

        private double bottomScaleHeight;
        internal double BottomScaleHeight
        {
            get { return this.bottomScaleHeight; }
            set { this.bottomScaleHeight = value; }
        }

        private double scrollAreaHeight;
        internal double ScrollAreaHeight
        {
            get { return this.scrollAreaHeight; }
            set { this.scrollAreaHeight = value; }
        }

        internal double TopOffset
        {
            get
            {
                return this.TopScrollHeight;// -this.TopScaleHeight / 2.0;
            }
        }
        internal double BottomOffset
        {
            get
            {
                return this.BottomScrollHeight;// -this.BottomScaleHeight / 2.0;
            }
        }

        private bool IsMoved { get; set; }

        #endregion

        #region Private Operations

        private void SizeUpdated()
        {
            this.Initialize = false;

            if (this.ActualWidth == 0 || this.ActualHeight == 0)
            {
                return;
            }

            UpdateElementPositions();
        }

        private static bool IsValidChange(double value)
        {
            return IsValidDoubleValue(value) && value >= 0.0;
        }

        private static bool IsValidDoubleValue(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        #endregion

        #region Initialization

        private bool _initialize;
        private bool Initialize
        {
            get { return _initialize; }
            set { _initialize = value; }
        }

        private void InitHorionatalTemplate()
        {
            if (this.horizontalRootCanvas != null)
            {
                this.horizontalRootCanvas.SizeChanged -= HorizontalRootCanvas_SizeChanged;
                this.horizontalRootCanvas.Children.Clear();
            }

            this.horizontalRootCanvas = this.GetTemplateChild(XamZoombar.HorizontalRootCanvasElementName) as Canvas;
            if (this.horizontalRootCanvas != null)
            {
                this.horizontalRootCanvas.SizeChanged += HorizontalRootCanvas_SizeChanged;
            }

            if (this.horizontalPreviewPanel != null)
            {
                // this.horizontalPreviewPanel.Unloaded -= HorizontalPreviewPanel_Unloaded;
                this.horizontalPreviewPanel.Children.Clear();
            }

            this.horizontalPreviewPanel = this.GetTemplateChild(XamZoombar.HorizontalPreviewElementName) as Panel;
            // [92398] in a tab control this will remove the preview
            // this.horizontalPreviewPanel.Unloaded += HorizontalPreviewPanel_Unloaded;

            this.horizontalScrollbarElement = this.GetTemplateChild(XamZoombar.HorizontalScrollbarElementName) as Grid;

            FrameworkElement horizontalTrackElement = this.GetTemplateChild(XamZoombar.HorizontalTrackElementName) as FrameworkElement;
            if (horizontalTrackElement != null)
            {
                this.horizontalTrackShape = new TrackShape(horizontalTrackElement);
                this.horizontalVisualElements.Add(this.horizontalTrackShape);
            }

            this.horizontalScrollLeftElement = this.GetTemplateChild(XamZoombar.HorizontalScrollLeftElementName) as FrameworkElement;
            if (this.horizontalScrollLeftElement != null)
            {
                this.horizontalScrollLeftShape = new ScrollShape(this.horizontalScrollLeftElement);
                this.horizontalVisualElements.Add(this.horizontalScrollLeftShape);
            }

            this.horizontalScrollRightElement = this.GetTemplateChild(XamZoombar.HorizontalScrollRightElementName) as FrameworkElement;
            if (this.horizontalScrollRightElement != null)
            {
                this.horizontalScrollRightShape = new ScrollShape(this.horizontalScrollRightElement);
                this.horizontalVisualElements.Add(this.horizontalScrollRightShape);
            }

            FrameworkElement horizontalThumbElement = this.GetTemplateChild(XamZoombar.HorizontalThumbElementName) as FrameworkElement;
            if (horizontalThumbElement != null)
            {
                this.horizontalThumbShape = new ShapeElement();
                this.horizontalThumbShape.FrameworkElements.Add(horizontalThumbElement);
            }

            FrameworkElement horizontalScaleLeftElement = this.GetTemplateChild(XamZoombar.HorizontalScaleLeftElementName) as FrameworkElement;
            if (horizontalScaleLeftElement != null)
            {
                this.horizontalScaleLeftShape = new ShapeElement();
                this.horizontalScaleLeftShape.FrameworkElements.Add(horizontalScaleLeftElement);
            }

            FrameworkElement horizontalScaleRightElement = this.GetTemplateChild(XamZoombar.HorizontalScaleRightElementName) as FrameworkElement;
            if (horizontalScaleRightElement != null)
            {
                this.horizontalScaleRightShape = new ShapeElement();
                this.horizontalScaleRightShape.FrameworkElements.Add(horizontalScaleRightElement);
            }

            this.horizontalThumbNode = CreateHorizontalThumbNode();
            this.horizontalVisualElements.Add(this.horizontalThumbNode);
        }

        private void InitVerticalTemplate()
        {
            if (this.verticalRootCanvas != null)
            {
                this.verticalRootCanvas.SizeChanged -= VerticalRootCanvas_SizeChanged;
                this.verticalRootCanvas.Children.Clear();
            }

            this.verticalRootCanvas = this.GetTemplateChild(XamZoombar.VerticalRootCanvasElementName) as Canvas;
            if (this.verticalRootCanvas != null)
            {
                this.verticalRootCanvas.SizeChanged += VerticalRootCanvas_SizeChanged;
            }

            if (this.verticalPreviewPanel != null)
            {
                // this.verticalPreviewPanel.Unloaded -= VerticalPreviewPanel_Unloaded;
                this.verticalPreviewPanel.Children.Clear();
            }

            this.verticalPreviewPanel = this.GetTemplateChild(XamZoombar.VerticalPreviewElementName) as Panel;
            // [92398] in a tab control this will remove the preview
            // this.verticalPreviewPanel.Unloaded += VerticalPreviewPanel_Unloaded;

            this.verticalScrollbarElement = this.GetTemplateChild(XamZoombar.VerticalScrollbarElementName) as Grid;

            FrameworkElement verticalTrackElement = this.GetTemplateChild(XamZoombar.VerticalTrackElementName) as FrameworkElement;
            if (verticalTrackElement != null)
            {
                this.verticalTrackShape = new TrackShape(verticalTrackElement);
                this.verticalVisualElements.Add(this.verticalTrackShape);
            }

            this.verticalScrollTopElement = this.GetTemplateChild(XamZoombar.VerticalScrollTopElementName) as FrameworkElement;
            if (this.verticalScrollTopElement != null)
            {
                this.verticalScrollTopShape = new ScrollShape(this.verticalScrollTopElement);
                this.verticalVisualElements.Add(this.verticalScrollTopShape);
            }

            this.verticalScrollBottomElement = this.GetTemplateChild(XamZoombar.VerticalScrollBottomElementName) as FrameworkElement;
            if (this.verticalScrollBottomElement != null)
            {
                this.verticalScrollBottomShape = new ScrollShape(this.verticalScrollBottomElement);
                this.verticalVisualElements.Add(this.verticalScrollBottomShape);
            }

            FrameworkElement verticalThumbElement = this.GetTemplateChild(XamZoombar.VerticalThumbElementName) as FrameworkElement;
            if (verticalThumbElement != null)
            {
                this.verticalThumbShape = new ShapeElement();
                this.verticalThumbShape.FrameworkElements.Add(verticalThumbElement);
            }

            FrameworkElement verticalScaleTopElement = this.GetTemplateChild(XamZoombar.VerticalScaleTopElementName) as FrameworkElement;
            if (verticalScaleTopElement != null)
            {
                this.verticalScaleTopShape = new ShapeElement();
                this.verticalScaleTopShape.FrameworkElements.Add(verticalScaleTopElement);
            }

            FrameworkElement verticalScaleBottomElement = this.GetTemplateChild(XamZoombar.VerticalScaleBottomElementName) as FrameworkElement;
            if (verticalScaleBottomElement != null)
            {
                this.verticalScaleBottomShape = new ShapeElement();
                this.verticalScaleBottomShape.FrameworkElements.Add(verticalScaleBottomElement);
            }

            this.verticalThumbNode = CreateVerticalThumbNode();
            this.verticalVisualElements.Add(this.verticalThumbNode);
        }

        private void RemoveVisualElements()
        {
            if (this.Initialize)
            {
                return;
            }

            if (this.Canvas != null)
            {
                for (int index = this.VisualElements.Count - 1; index >= 0; index--)
                {
                    this.VisualElements.RemoveAt(index);
                }
            }
        }

        private void SetHorizontalVisualElements()
        {
            this.RemoveVisualElements();

            this.Canvas = this.horizontalRootCanvas;

            foreach (VisualElement visualElement in this.horizontalVisualElements)
            {
                this.VisualElements.Add(visualElement);
            }
        }
        private void SetVerticalVisualElements()
        {
            this.RemoveVisualElements();

            this.Canvas = this.verticalRootCanvas;

            foreach (VisualElement visualElement in this.verticalVisualElements)
            {
                this.VisualElements.Add(visualElement);
            }
        }

        private void HorizontalRootCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth > 0 && this.ActualHeight > 0)
            {
                this.horizontalScrollbarElement.Width = this.ActualWidth;
                this.horizontalScrollbarElement.Height = this.ActualHeight;
            }

            UpdateElementPositions();
        }
        private void VerticalRootCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth > 0 && this.ActualHeight > 0)
            {
                this.verticalScrollbarElement.Width = this.ActualWidth;
                this.verticalScrollbarElement.Height = this.ActualHeight;
            }

            UpdateElementPositions();
        }

        private void XamZoombar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.IsSizeChanged = true;
        }

        private void XamZoombar_LayoutUpdated(object sender, EventArgs e)
        {
            if (this.IsSizeChanged == false)
            {
                return;
            }

            SizeUpdated();

            this.IsSizeChanged = false;
        }

        private void HorizontalPreviewPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.horizontalPreviewPanel != null &&
                this.horizontalPreviewPanel.Children.Count > 0)
            {
                ContentPresenter cp = this.horizontalPreviewPanel.Children[0] as ContentPresenter;
                if (cp != null && cp.Content != null)
                {
                    cp.Content = null;
                }
            }
        }

        private void VerticalPreviewPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.verticalPreviewPanel != null &&
                this.verticalPreviewPanel.Children.Count > 0)
            {
                ContentPresenter cp = this.verticalPreviewPanel.Children[0] as ContentPresenter;
                if (cp != null && cp.Content != null)
                {
                    cp.Content = null;
                }
            }
        }

        #endregion

        #region Positioning

        private void UpdateElementPositions()
        {
            if (this.ActualWidth == 0 || this.ActualHeight == 0)
            {
                return;
            }

            if (this.Orientation == Orientation.Horizontal)
            {
                UpdateVerticalLayout();

                if (this.verticalRootCanvas != null)
                {
                    this.verticalRootCanvas.Visibility = Visibility.Collapsed;
                    this.horizontalRootCanvas.Visibility = Visibility.Visible;
                    this.horizontalRootCanvas.UpdateLayout();
                }

                UpdateHorizontalLayout();
            }
            else
            {
                UpdateHorizontalLayout();

                if (this.horizontalRootCanvas != null)
                {
                    this.horizontalRootCanvas.Visibility = Visibility.Collapsed;
                    this.verticalRootCanvas.Visibility = Visibility.Visible;
                    this.verticalRootCanvas.UpdateLayout();
                }

                UpdateVerticalLayout();
            }

            UpdateThumb();
        }

        private void UpdateHorizontalLayout()
        {
            if (this.horizontalRootCanvas == null)
            {
                return;
            }

            Size size = new Size(this.ActualWidth, this.ActualHeight);

            this.LeftScrollWidth = this.horizontalScrollLeftShape.FrameworkElement.ActualWidth;
            this.RightScrollWidth = this.horizontalScrollRightShape.FrameworkElement.ActualWidth;

            this.LeftScaleWidth = this.horizontalScaleLeftShape.Width;
            this.RightScaleWidth = this.horizontalScaleRightShape.Width;

            double left = this.LeftOffset;
            double right = this.RightOffset;

            this.ScrollAreaWidth = size.Width - left - right;

            double x = size.Width / 2.0 - this.horizontalThumbShape.Width / 2.0;
            double width = this.horizontalThumbShape.Width;

            this.horizontalThumbNode.Bounds = new Rect(x, 0, width, size.Height);
            this.horizontalScrollLeftShape.MoveLeft = true;
        }
        private void UpdateVerticalLayout()
        {
            if (this.verticalRootCanvas == null)
            {
                return;
            }

            Size size = new Size(this.ActualWidth, this.ActualHeight);

            this.TopScrollHeight = this.verticalScrollTopShape.FrameworkElement.ActualHeight;
            this.BottomScrollHeight = this.verticalScrollBottomShape.FrameworkElement.ActualHeight;

            this.TopScaleHeight = this.verticalScaleTopShape.Height;
            this.BottomScaleHeight = this.verticalScaleBottomShape.Height;

            double top = this.TopOffset;
            double bottom = this.BottomOffset;

            this.ScrollAreaHeight = size.Height - top - bottom;

            double y = size.Height / 2.0 - this.verticalThumbShape.Height / 2.0;
            double height = this.verticalThumbShape.Height;

            this.verticalThumbNode.Bounds = new Rect(0, y, size.Width, height);
            this.verticalScrollTopShape.MoveLeft = true;
        }

        #endregion

        #region Thumb Methods

        private HorizontalThumbNode CreateHorizontalThumbNode()
        {
            if (this.horizontalThumbShape != null)
            {
                this.horizontalThumbNode.Children.Add(this.horizontalThumbShape);
                this.horizontalThumbNode.ThumbBackground = this.horizontalThumbShape;
            }

            if (this.horizontalScaleLeftShape != null)
            {
                this.horizontalThumbNode.Children.Add(this.horizontalScaleLeftShape);
                this.horizontalThumbNode.ScaleLeftElement = this.horizontalScaleLeftShape;
            }

            if (this.horizontalScaleRightShape != null)
            {
                this.horizontalThumbNode.Children.Add(this.horizontalScaleRightShape);
                this.horizontalThumbNode.ScaleRightElement = this.horizontalScaleRightShape;
            }

            return this.horizontalThumbNode;
        }
        private VerticalThumbNode CreateVerticalThumbNode()
        {
            if (this.verticalThumbShape != null)
            {
                this.verticalThumbNode.Children.Add(this.verticalThumbShape);
                this.verticalThumbNode.ThumbBackground = this.verticalThumbShape;
            }

            if (this.verticalScaleTopShape != null)
            {
                this.verticalThumbNode.Children.Add(this.verticalScaleTopShape);
                this.verticalThumbNode.ScaleTopElement = this.verticalScaleTopShape;
            }

            if (this.verticalScaleBottomShape != null)
            {
                this.verticalThumbNode.Children.Add(this.verticalScaleBottomShape);
                this.verticalThumbNode.ScaleBottomElement = this.verticalScaleBottomShape;
            }

            return this.verticalThumbNode;
        }

        internal void UpdateThumb()
        {
           if (this.Initialize || this.Range == null)
            {
                return;
            }

            UpdateThumb(this.Range);
        }

        internal void UpdateTempThumb()
        {
            UpdateThumb(this.TempRange);
        }

        private void UpdateThumb(Range range)
        {
            if (this.Initialize)
            {
                return;
            }

            if (this.Orientation == Orientation.Horizontal)
            {
                Rect bounds = this.horizontalThumbNode.Bounds;

                bounds.Width = System.Math.Max(0.0, this.ScrollAreaWidth * range.Scale);
                double width = this.ScrollAreaWidth - bounds.Width;
                bounds.X = this.LeftOffset + width * range.Scroll;
                bounds = CalculatePossibleBounds(bounds);

                this.horizontalThumbNode.Bounds = bounds;
            }
            else
            {
                Rect bounds = this.verticalThumbNode.Bounds;

                bounds.Height = System.Math.Max(0.0, this.ScrollAreaHeight * range.Scale);
                double height = this.ScrollAreaHeight - bounds.Height;
                bounds.Y = this.TopOffset + height * range.Scroll;
                bounds = CalculatePossibleBounds(bounds);

                this.verticalThumbNode.Bounds = bounds;
            }
        }

        private Rect CalculatePossibleBounds(Rect bounds)
        {
            if (this.Orientation == Orientation.Horizontal)
            {
                double left = this.LeftOffset;
                double right = this.ActualWidth - this.RightOffset;

                double maxWidth = System.Math.Max(0.0, right - left);
                if (bounds.Width > maxWidth)
                {
                    bounds.Width = maxWidth;
                }

                double minWidth = this.LeftScaleWidth + this.RightScaleWidth;
                if (bounds.Width < minWidth)
                {
                    bounds.X -= (minWidth - bounds.Width) / 2.0;
                    bounds.Width = minWidth;
                }

                if (bounds.Right > right)
                {
                    bounds.X = right - bounds.Width;
                }

                if (bounds.X < left)
                {
                    bounds.X = left;
                }
            }
            else
            {
                double top = this.TopOffset;
                double bottom = this.ActualHeight - this.BottomOffset;

                double maxHeight = System.Math.Max(0.0, bottom - top);
                if (bounds.Height > maxHeight)
                {
                    bounds.Height = maxHeight;
                }

                double minHeight = this.TopScaleHeight + this.BottomScaleHeight;
                if (bounds.Height < minHeight)
                {
                    bounds.Y -= (minHeight - bounds.Height) / 2.0;
                    bounds.Height = minHeight;
                }

                if (bounds.Bottom > bottom)
                {
                    bounds.Y = bottom - bounds.Height;
                }

                if (bounds.Y < top)
                {
                    bounds.Y = top;
                }
            }

            return bounds;
        }

        #endregion

        #region Move Animation

        private DispatcherTimer _timer;
        private DispatcherTimer Timer
        {
            get { return _timer; }
            set { _timer = value; }
        }

        private double _newValue;
        private double NewValue
        {
            get { return _newValue; }
            set { _newValue = value; }
        }

        private bool _isSmallChange;
        private bool IsSmallChange
        {
            get { return _isSmallChange; }
            set { _isSmallChange = value; }
        }

        internal void MoveTo(double newValue, bool isSmallChange)
        {
            this.IsMoved = false;

            this.NewValue = newValue;
            this.IsSmallChange = isSmallChange;
            this.Timer.Start();
        }

        internal void StopMove()
        {
            this.Timer.Stop();

            if (this.IsMoved == false)
            {
                DoMove();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DoMove();
        }

        private void DoMove()
        {
            this.IsMoved = true;

            double center = (this.Range.Minimum + this.Range.Maximum) / 2.0;
            double change = IsSmallChange ? this.SmallChange : this.LargeChange;

            Debug.WriteLine("Move");

            if (center == NewValue)
            {
                StopMove();
                return;
            }

            Range range = new Range();
            range.Initialize = true;
            range.Minimum = this.Range.Minimum - change;
            range.Maximum = this.Range.Maximum - change;
            range.Initialize = false;

            if (this.NewValue < center)
            {
                if (range.Minimum < this.Minimum)
                {
                    double diff = this.Minimum - range.Minimum;

                    range.Initialize = true;
                    range.Minimum = this.Minimum;
                    range.Maximum += diff;
                    range.Initialize = false;

                    this.Range = range;
                    this.StopMove();
                }
                else if (this.NewValue > center - change)
                {
                    this.Range = range;
                    this.StopMove();
                }
                else
                {
                    this.Range = range;
                }

                return;
            }

            range.Initialize = true;
            range.Minimum = this.Range.Minimum + change;
            range.Maximum = this.Range.Maximum + change;
            range.Initialize = false;

            if (this.NewValue > center)
            {
                if (range.Maximum > this.Maximum)
                {
                    double diff = range.Maximum - this.Maximum;

                    range.Initialize = true;
                    range.Minimum -= diff;
                    range.Maximum = this.Maximum;
                    range.Initialize = false;

                    this.Range = range;
                    this.StopMove();
                }
                else if (this.NewValue < center + change)
                {
                    this.Range = range;
                    this.StopMove();
                }
                else
                {
                    this.Range = range;
                }

                return;
            }
        }

        #endregion

        #region RegisterResources

        /// <summary>
        /// Adds an additonal Resx file in which the control will pull its resources from.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
        /// <param name="assembly">The assembly in which the resx file is embedded.</param>
        /// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
        public static void RegisterResources(string name, System.Reflection.Assembly assembly)
        {
#pragma warning disable 436
            SR.AddResource(name, assembly);
#pragma warning restore 436
        }

        #endregion // RegisterResources

        #region UnregisterResources

        /// <summary>
        /// Removes a previously registered resx file.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that was used for registration.</param>
        /// <remarks>
        /// Note: this won't have any effect on controls that are already in view and are already displaying strings.
        /// It will only affect any new controls created.
        /// </remarks>
        public static void UnregisterResources(string name)
        {
#pragma warning disable 436
            SR.RemoveResource(name);
#pragma warning restore 436
        }

        #endregion // UnregisterResources
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