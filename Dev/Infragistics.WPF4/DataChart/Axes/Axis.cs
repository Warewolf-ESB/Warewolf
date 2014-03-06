using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;



using System.Linq;


using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class from which all axis types are derived.
    /// </summary>
    [TemplatePart(Name = Axis.RootCanvasName, Type = typeof(Canvas))]

    [DesignTimeVisible(false)]






    public abstract class Axis : Control, INotifyPropertyChanged
    {
        internal virtual AxisView CreateView()
        {
            return new AxisView(this);
        }
        internal virtual AxisView View { get; set; }
        internal virtual void OnViewCreated(AxisView view)
        {

        }

        #region constructor and initialisation

        /// <summary>
        /// Constructs a new Axis instance.
        /// </summary>
        protected Axis()
        {
            View = CreateView();
            OnViewCreated(View);
            View.OnInit();
            TextBlocks =
                new Pool<TextBlock>()
                {
                    Create = View.TextBlockCreate,
                    Activate = View.TextBlockActivate,
                    Disactivate = View.TextBlockDisactivate,
                    Destroy = View.TextBlockDestroy
                };

            ViewportOverride = Rect.Empty;

            LabelDataContext = new List<object>();
            LabelPositions = new List<LabelPosition>();
            LabelPanel = CreateLabelPanel();
            LabelPanel.Axis = this;



            Series = new List<Series>();


            DefaultStyleKey = typeof(Axis);

            PropertyUpdated += (o, e) => PropertyUpdatedOverride(o, e.PropertyName, e.OldValue, e.NewValue);
            SeriesViewer_WindowRectChanged = (o, e) => WindowRectChangedOverride(e.OldRect, e.NewRect);

            View.BindLabelPanelStyle();
        }



#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


        internal void HandleRectChanged(Rect oldRect, Rect newRect)
        {



            if (oldRect != newRect)

            {
                ViewportChangedOverride(oldRect, newRect);
            }
        }

        internal abstract AxisLabelPanelBase CreateLabelPanel();

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            RootCanvas = GetTemplateChild(RootCanvasName) as Canvas;

            View.OnTemplateProvided();
        }

        /// <summary>
        /// Gets the current Axis object's root canvas.
        /// </summary>
        public Canvas RootCanvas { get; private set; }

        private const string RootCanvasName = "RootCanvas";

        #endregion constructor and initialisation

        #region FastItemsSourceProvider property

        internal IFastItemsSourceProvider FastItemsSourceProvider
        {
            get { return fastItemsSourceProvider; }
            set
            {
                if (FastItemsSourceProvider != value)
                {
                    IFastItemsSourceProvider oldChart = FastItemsSourceProvider;

                    fastItemsSourceProvider = value;
                    RaisePropertyChanged(FastItemsSourceProviderPropertyName, oldChart, fastItemsSourceProvider);
                }
            }
        }

        private IFastItemsSourceProvider fastItemsSourceProvider;
        internal const string FastItemsSourceProviderPropertyName = "FastItemsSourceProvider";

        #endregion FastItemsSourceProvider property

        #region Owner Chart Property

        internal const string SeriesViewerPropertyName = "SeriesViewer";

        /// <summary>
        /// Gets the ChartArea for the current Axis object.
        /// </summary>
        /// <remarks>
        /// This property is maintained by the chart control and is not guaranteed to be
        /// valid until at least after the owner chart has been loaded.
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
        /// Gets or sets the Chart control reference.
        /// </summary>

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Obsolete("Please use the SeriesViewer property.")]

        [SuppressWidgetMember]
        public XamDataChart Chart { get { return this.SeriesViewer as XamDataChart; } set { this.SeriesViewer = value; } }

        /// <summary>
        /// Listener for the owner chart area's window rect changed
        /// </summary>
        internal readonly RectChangedEventHandler SeriesViewer_WindowRectChanged;

        #endregion Owner Chart Property



#region Infragistics Source Cleanup (Region)




























































#endregion // Infragistics Source Cleanup (Region)


        #region Stroke Dependency Property

        /// <summary>
        /// Gets or sets the Stroke property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Brush Stroke
        {
            get
            {
                return (Brush)GetValue(StrokeProperty);
            }
            set
            {
                SetValue(StrokeProperty, value);
            }
        }

        internal const string StrokePropertyName = "Stroke";

        /// <summary>
        /// Identifies the Stroke dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(StrokePropertyName, typeof(Brush), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(StrokePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion Stroke Dependency Property

        #region StrokeThickness Dependency Property

        /// <summary>
        /// Gets or sets the StrokeThickness property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultNumber(1.0)]
        public double StrokeThickness
        {
            get
            {
                return (double)GetValue(StrokeThicknessProperty);
            }
            set
            {
                SetValue(StrokeThicknessProperty, value);
            }
        }

        internal const string StrokeThicknessPropertyName = "StrokeThickness";

        /// <summary>
        /// Identifies the StrokeThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(StrokeThicknessPropertyName, typeof(double), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(StrokeThicknessPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion StrokeThickness Dependency Property

        #region StrokeDashArray Dependency Property

        /// <summary>
        /// Gets or sets the StrokeDashArray property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [SuppressWidgetMember]
        public DoubleCollection StrokeDashArray
        {
            get
            {
                return (DoubleCollection)GetValue(StrokeDashArrayProperty);
            }
            set
            {
                SetValue(StrokeDashArrayProperty, value);
            }
        }

        internal const string StrokeDashArrayPropertyName = "StrokeDashArray";

        /// <summary>
        /// Identifies the StrokeDashArray dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(StrokeDashArrayPropertyName, typeof(DoubleCollection), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(StrokeDashArrayPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion StrokeDashArray Dependency Property

        #region Strip Dependency Property

        /// <summary>
        /// Gets or sets the Strip property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Brush Strip
        {
            get
            {
                return (Brush)GetValue(StripProperty);
            }
            set
            {
                SetValue(StripProperty, value);
            }
        }

        internal const string StripPropertyName = "Strip";

        /// <summary>
        /// Identifies the Strip dependency property.
        /// </summary>
        public static readonly DependencyProperty StripProperty = DependencyProperty.Register(StripPropertyName, typeof(Brush), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(StripPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion Strip Dependency Property

        #region MajorStroke Dependency Property

        /// <summary>
        /// Gets or sets the MajorStroke property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Brush MajorStroke
        {
            get
            {
                return (Brush)GetValue(MajorStrokeProperty);
            }
            set
            {
                SetValue(MajorStrokeProperty, value);
            }
        }

        internal const string MajorStrokePropertyName = "MajorStroke";

        /// <summary>
        /// Identifies the MajorStroke dependency property.
        /// </summary>
        public static readonly DependencyProperty MajorStrokeProperty = DependencyProperty.Register(MajorStrokePropertyName, typeof(Brush), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(MajorStrokePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion MajorStroke Dependency Property

        #region MajorStrokeThickness Dependency Property

        /// <summary>
        /// Gets or sets the MajorStrokeThickness property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultNumber(1.0)]
        public double MajorStrokeThickness
        {
            get
            {
                return (double)GetValue(MajorStrokeThicknessProperty);
            }
            set
            {
                SetValue(MajorStrokeThicknessProperty, value);
            }
        }

        internal const string MajorStrokeThicknessPropertyName = "MajorStrokeThickness";

        /// <summary>
        /// Identifies the MajorStrokeThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty MajorStrokeThicknessProperty = DependencyProperty.Register(MajorStrokeThicknessPropertyName, typeof(double), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(MajorStrokeThicknessPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion MajorStrokeThickness Dependency Property

        #region MajorStrokeDashArray Dependency Property

        /// <summary>
        /// Gets or sets the MajorStrokeDashArray property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [SuppressWidgetMember]
        public DoubleCollection MajorStrokeDashArray
        {
            get
            {
                return (DoubleCollection)GetValue(MajorStrokeDashArrayProperty);
            }
            set
            {
                SetValue(MajorStrokeDashArrayProperty, value);
            }
        }

        internal const string MajorStrokeDashArrayPropertyName = "MajorStrokeDashArray";

        /// <summary>
        /// Identifies the MajorStrokeDashArray dependency property.
        /// </summary>
        public static readonly DependencyProperty MajorStrokeDashArrayProperty = DependencyProperty.Register(MajorStrokeDashArrayPropertyName, typeof(DoubleCollection), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(MajorStrokeDashArrayPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion MajorStrokeDashArray Dependency Property

        #region MinorStroke Dependency Property

        /// <summary>
        /// Gets or sets the MinorStroke property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public Brush MinorStroke
        {
            get
            {
                return (Brush)GetValue(MinorStrokeProperty);
            }
            set
            {
                SetValue(MinorStrokeProperty, value);
            }
        }

        internal const string MinorStrokePropertyName = "MinorStroke";

        /// <summary>
        /// Identifies the MinorStroke dependency property.
        /// </summary>
        public static readonly DependencyProperty MinorStrokeProperty = DependencyProperty.Register(MinorStrokePropertyName, typeof(Brush), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(MinorStrokePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion MinorStroke Dependency Property

        #region MinorStrokeThickness Dependency Property

        /// <summary>
        /// Gets or sets the MinorStrokeThickness property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultNumber(1.0)]
        public double MinorStrokeThickness
        {
            get
            {
                return (double)GetValue(MinorStrokeThicknessProperty);
            }
            set
            {
                SetValue(MinorStrokeThicknessProperty, value);
            }
        }

        internal const string MinorStrokeThicknessPropertyName = "MinorStrokeThickness";

        /// <summary>
        /// Identifies the MinorStrokeThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty MinorStrokeThicknessProperty = DependencyProperty.Register(MinorStrokeThicknessPropertyName, typeof(double), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(MinorStrokeThicknessPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion MinorStrokeThickness Dependency Property

        #region MinorStrokeDashArray Dependency Property

        /// <summary>
        /// Gets or sets the MinorStrokeDashArray property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [SuppressWidgetMember]
        public DoubleCollection MinorStrokeDashArray
        {
            get
            {
                return (DoubleCollection)GetValue(MinorStrokeDashArrayProperty);
            }
            set
            {
                SetValue(MinorStrokeDashArrayProperty, value);
            }
        }

        internal const string MinorStrokeDashArrayPropertyName = "MinorStrokeDashArray";

        /// <summary>
        /// Identifies the MinorStrokeDashArray dependency property.
        /// </summary>
        public static readonly DependencyProperty MinorStrokeDashArrayProperty = DependencyProperty.Register(MinorStrokeDashArrayPropertyName, typeof(DoubleCollection), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(MinorStrokeDashArrayPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion MinorStrokeDashArray Dependency Property

        #region IsDisabled Dependency Property

        /// <summary>
        /// Gets or sets the IsDisabled property. If true, the axis will not be rendered.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultBoolean(false)]
        public bool IsDisabled
        {
            get
            {
                return (bool)GetValue(IsDisabledProperty);
            }
            set
            {
                SetValue(IsDisabledProperty, value);
            }
        }

        internal const string IsDisabledPropertyName = "IsDisabled";

        /// <summary>
        /// Identifies the IsDisabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDisabledProperty = DependencyProperty.Register(IsDisabledPropertyName, typeof(bool), typeof(Axis),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(IsDisabledPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion IsDisabled Dependency Property

        #region IsInverted Dependency Property

        /// <summary>
        /// Gets or sets the IsInverted property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultBoolean(false)]
        public bool IsInverted
        {
            get
            {
                return (bool)GetValue(IsInvertedProperty);
            }
            set
            {
                SetValue(IsInvertedProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets whether the axis is inverted.
        /// </summary>
        protected internal bool IsInvertedCached { get; set; }

        internal const string IsInvertedPropertyName = "IsInverted";

        /// <summary>
        /// Identifies the IsInverted dependency property.
        /// </summary>
        public static readonly DependencyProperty IsInvertedProperty = DependencyProperty.Register(IsInvertedPropertyName, typeof(bool), typeof(Axis),
            new PropertyMetadata(false, (sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(IsInvertedPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion IsInverted Dependency Property

        #region LabelSettings Dependency Property

        internal const string LabelSettingsPropertyName = "LabelSettings";

        /// <summary>
        /// Identifies the LabelSettings dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelSettingsProperty = DependencyProperty.Register(LabelSettingsPropertyName, typeof(AxisLabelSettings), typeof(Axis),
            new PropertyMetadata(null, (sender, e) =>
                {
                    (sender as Axis).RaisePropertyChanged(LabelSettingsPropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets or sets the settings for the axis labels.
        /// </summary>
        [SuppressWidgetMember]
        public AxisLabelSettings LabelSettings
        {
            get
            {
                return (AxisLabelSettings)this.GetValue(LabelSettingsProperty);
            }
            set
            {
                this.SetValue(LabelSettingsProperty, value);
            }
        }

        #endregion LabelSettings Dependency Property

        #region LabelPanelStyle Dependency Property

        internal const string LabelPanelStylePropertyName = "LabelPanelStyle";

        /// <summary>
        /// Identifies the LabelPanelStyle Dependency Property.
        /// </summary>
        public static readonly DependencyProperty LabelPanelStyleProperty = DependencyProperty.Register(LabelPanelStylePropertyName, typeof(Style), typeof(Axis),
            new PropertyMetadata(null, (sender, e) =>
                {
                    (sender as Axis).RaisePropertyChanged(LabelPanelStylePropertyName, e.OldValue, e.NewValue);
                }));

        /// <summary>
        /// Gets or sets the LabelPanelStyle property.
        /// </summary>
        [SuppressWidgetMember]
        public Style LabelPanelStyle
        {
            get
            {
                return (Style)GetValue(LabelPanelStyleProperty);
            }
            set
            {
                SetValue(LabelPanelStyleProperty, value);
            }
        }

        #endregion LabelPanelStyle Dependency Property

        #region CrossingAxis Dependency Property

        /// <summary>
        /// Gets or sets the CrossingAxis property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        [SuppressWidgetMemberCopy]
        public Axis CrossingAxis
        {
            get
            {
                return (Axis)this.GetValue(CrossingAxisProperty);
            }
            set
            {
                this.SetValue(CrossingAxisProperty, value);
            }
        }

        internal const string CrossingAxisPropertyName = "CrossingAxis";

        /// <summary>
        /// Identifies the CrossingAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty CrossingAxisProperty = DependencyProperty.Register(CrossingAxisPropertyName, typeof(Axis), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(CrossingAxisPropertyName, e.OldValue, e.NewValue);
            }));

        #endregion CrossingAxis Dependency Property

        #region CrossingValue Dependency Property

        /// <summary>
        /// Gets or sets the CrossingValue property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>

        [TypeConverter(typeof(ObjectConverter))]

        public object CrossingValue
        {
            get
            {
                return this.GetValue(CrossingValueProperty);
            }
            set
            {
                this.SetValue(CrossingValueProperty, value);
            }
        }

        internal const string CrossingValuePropertyName = "CrossingValue";

        /// <summary>
        /// Identifies the CrossingValue dependency property.
        /// </summary>
        public static readonly DependencyProperty CrossingValueProperty = DependencyProperty.Register(CrossingValuePropertyName, typeof(object), typeof(Axis),
            new PropertyMetadata((sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(CrossingValuePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion CrossingValue Dependency Property

        #region RangeChanged event

        /// <summary>
        /// Occurs when the axis range changes.
        /// </summary>
        [SuppressWidgetMember]
        public event AxisRangeChangedEventHandler RangeChanged;

        /// <summary>
        /// Raises the RangeChanged event.
        /// </summary>
        /// <param name="ea">The AxisRangeChangedEventArgs for the event.</param>
        protected void RaiseRangeChanged(AxisRangeChangedEventArgs ea)
        {
            if (RangeChanged != null)
            {
                RangeChanged(this, ea);
            }
        }

        #endregion RangeChanged event

        private Rect _viewport = Rect.Empty;
        /// <summary>
        /// Gets the viewport of the axis.
        /// </summary>
        internal Rect ViewportRect
        {
            get
            {
                if (ViewportOverride.IsEmpty)
                {
                    return _viewport;
                }
                return ViewportOverride;
            }
            set
            {
                _viewport = value;
            }
        }

        internal Rect ViewportOverride { get; set; }

        internal void UpdateLineVisibility()
        {
            Visibility visible = Visibility.Visible;
            XamDataChart dataChart = this.SeriesViewer as XamDataChart;
            if (dataChart != null && dataChart.GridMode == GridMode.None)
            {
                visible = Visibility.Collapsed;
            }

            View.UpdateLineVisibility(visible);
        }

        /// <summary>
        /// Gets the label data contexts.
        /// </summary>
        protected internal List<object> LabelDataContext { get; internal set; }

        internal List<LabelPosition> LabelPositions { get; set; }
        internal AxisLabelPanelBase LabelPanel { get; set; }

        /// <summary>
        /// Overridden in derived classes when they want to respond to the viewport changing.
        /// </summary>
        /// <param name="oldRect">The old viewport rectangle.</param>
        /// <param name="newRect">The new viewport rectangle.</param>
        protected virtual void ViewportChangedOverride(Rect oldRect, Rect newRect)
        {
            ViewportRect = newRect;
            MustInvalidateLabels = true;
            UpdateRange();
            RenderAxis(false);

            if (SeriesViewer != null)
            {
                SeriesViewer.ChartContentManager.ViewportChanged(
                    ChartContentType.Axis,
                    this,
                    ContentInfo,
                    newRect);
            }
        }

        private bool _mustInvalidateLabels = false;

        internal bool MustInvalidateLabels
        {
            get
            {
                return _mustInvalidateLabels;
            }
            set
            {
                _mustInvalidateLabels = value;
            }
        }

        /// <summary>
        /// Overridden in derived classes when they want to respond to the chart's window changing.
        /// </summary>
        /// <param name="oldRect">The old window rectangle.</param>
        /// <param name="newRect">The new window rectangle.</param>
        protected virtual void WindowRectChangedOverride(Rect oldRect, Rect newRect)
        {
            MustInvalidateLabels = true;
            RenderAxis(true);
        }

        internal void Refresh()
        {
            RenderAxis(false);
        }

        [Weak]
        internal ContentInfo ContentInfo { get; set; }

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the axis. Gives the axis a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected virtual void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            switch (propertyName)
            {
                case Axis.SeriesViewerPropertyName:
                    SeriesViewer oldSeriesViewer = oldValue as SeriesViewer;
                    if (oldSeriesViewer != null)
                    {
                        oldSeriesViewer.WindowRectChanged += SeriesViewer_WindowRectChanged;
                        oldSeriesViewer.ChartContentManager.Unsubscribe(ChartContentType.Axis, this);
                        View.DetachFromChart(oldSeriesViewer);
                    }
                    SeriesViewer newSeriesViewer = newValue as SeriesViewer;
                    if (newSeriesViewer != null)
                    {
                        newSeriesViewer.WindowRectChanged += SeriesViewer_WindowRectChanged;
                        ContentInfo = newSeriesViewer.ChartContentManager.Subscribe(ChartContentType.Axis, this, DoRenderAxis);
                        View.AttachToChart(newSeriesViewer);
                        if (RangeDirty && !ContentInfo.RangeDirty)
                        {
                            RangeDirty = false;
                            UpdateRange();
                        }
                    }
                    break;
                case IsDisabledPropertyName:
                    RenderAxis(false);
                    break;
                case Axis.IsInvertedPropertyName:
                    IsInvertedCached = IsInverted;
                    AxisRangeChangedEventArgs rangeChangedEventArgs = new AxisRangeChangedEventArgs(0, 0, 1, 1);
                    DoRaiseRangeChanged(rangeChangedEventArgs);






                    foreach (var series in Series)
                    {

                        series.InvalidateAxes();
                        if (series.SeriesViewer != null)
                        {
                            series.NotifyThumbnailAppearanceChanged();
                        }
                    }
                    break;
                case Axis.LabelPropertyName:
                    if (newValue is DataTemplate)
                    {
                        UsingTemplate = true;
                    }
                    else
                    {
                        UsingTemplate = false;
                    }
                    MustInvalidateLabels = true;
                    ResetLabelPanel();
                    RenderAxis(false);
                    break;

                case Axis.MajorStrokeThicknessPropertyName:
                case Axis.MinorStrokeThicknessPropertyName:
                case Axis.StrokeThicknessPropertyName:
                    RenderAxis(false);
                    break;

                case Axis.LabelPanelStylePropertyName:
                    View.OnLabelPanelStyleChanged(newValue);
                    RenderAxis(false);
                    break;
                case Axis.LabelSettingsPropertyName:
                    AxisLabelSettings labelSettings = newValue as AxisLabelSettings;
                    if (labelSettings != null)
                    {
                        labelSettings.Axis = this;
                        _currentLabelSettings = labelSettings;
                    }
                    else
                    {
                        _currentLabelSettings = new AxisLabelSettings();
                    }
                    if (View.Ready())
                    {
                        View.ChangeLabelSettings(_currentLabelSettings);
                    }
                    break;
                case Axis.MinorStrokePropertyName:
                    if (newValue != null)
                    {
                        ShouldRenderMinorLines = true;
                        RenderAxis(false);
                    }
                    else
                    {
                        ShouldRenderMinorLines = false;
                    }
                    break;


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            }
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Gets or sets whether the minor gridliens should be displayed.
        /// </summary>
        protected internal bool ShouldRenderMinorLines { get; set; }

        private void ResetLabelPanel()
        {
            TextBlocks.Count = 0;
            if (View.Ready())
            {
                View.ResetLabelPanel();
            }
        }

        internal bool UsingTemplate { get; set; }

        /// <summary>
        /// Generates the visual for a horizontal strip.
        /// </summary>
        /// <param name="geometry">The geometry into which to generate the visual.</param>
        /// <param name="y0">The starting y value for the strip.</param>
        /// <param name="y1">The ending y value for the strip.</param>
        /// <param name="viewportRect">The viewport of the axis.</param>
        protected void HorizontalStrip(GeometryCollection geometry, double y0, double y1, Rect viewportRect)
        {
            double ymin = Math.Min(y0, y1);
            double ymax = Math.Max(y0, y1);

            if (ymin < viewportRect.Bottom && ymax > viewportRect.Top)
            {
                RectangleGeometry strip = new RectangleGeometry();

                strip.Rect = new Rect(viewportRect.Left, ymin, viewportRect.Width, ymax - ymin);
                geometry.Add(strip);
            }
        }

        /// <summary>
        /// Generates the visual for a horizontal line.
        /// </summary>
        /// <param name="geometry">The geometry into which to generate the visual.</param>
        /// <param name="y">The y value for the horizontal line.</param>
        /// <param name="viewportRect">The viewport of the axis.</param>
        protected void HorizontalLine(GeometryCollection geometry, double y, Rect viewportRect)
        {
            if (y <= viewportRect.Bottom && y >= viewportRect.Top)
            {
                LineGeometry line = new LineGeometry();

                line.StartPoint = new Point(viewportRect.Left, y);
                line.EndPoint = new Point(viewportRect.Right, y);
                geometry.Add(line);
            }
        }

        /// <summary>
        /// Generates the visual for a vertical strip.
        /// </summary>
        /// <param name="geometry">The geometry into which to generated the visual.</param>
        /// <param name="x0">The starting x value for the strip.</param>
        /// <param name="x1">The ending x value for the strip.</param>
        /// <param name="viewportRect">The viewport of the axis.</param>
        protected void VerticalStrip(GeometryCollection geometry, double x0, double x1, Rect viewportRect)
        {
            double xmin = Math.Min(x0, x1);
            double xmax = Math.Max(x0, x1);

            if (xmax > viewportRect.Left && xmin < viewportRect.Right)
            {
                RectangleGeometry strip = new RectangleGeometry();

                strip.Rect = new Rect(xmin, viewportRect.Top, xmax - xmin, viewportRect.Height);
                geometry.Add(strip);
            }
        }

        /// <summary>
        /// Generates the visual for a vertical line.
        /// </summary>
        /// <param name="geometry">The geometry into which to generate the visual.</param>
        /// <param name="x">The x value for the line.</param>
        /// <param name="viewportRect">The viewport of the axis.</param>
        protected void VerticalLine(GeometryCollection geometry, double x, Rect viewportRect)
        {
            if (x >= viewportRect.Left && x <= viewportRect.Right)
            {
                LineGeometry majorLine = new LineGeometry();

                majorLine.StartPoint = new Point(x, viewportRect.Top);
                majorLine.EndPoint = new Point(x, viewportRect.Bottom);
                geometry.Add(majorLine);
            }
        }



#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Clears the marks from a geometry.
        /// </summary>
        /// <param name="geometry">The geometry to clear the marks from.</param>
        protected internal void ClearMarks(GeometryCollection geometry)
        {
            View.ClearMarks(geometry);
        }

        /// <summary>
        /// Clears all marks from the axis.
        /// </summary>
        protected void ClearAllMarks()
        {
            TextBlocks.Count = 0;
            View.ClearAllMarks();

            LabelDataContext.Clear();
            LabelPositions.Clear();
        }

        /// <summary>
        /// Renders the axis with no animation.
        /// </summary>
        public void RenderAxis()
        {
            RenderAxis(false);
        }

        internal void RenderAxis(bool animate)
        {
            if (SeriesViewer != null)
            {
                SeriesViewer.ChartContentManager.Refresh(
                    ChartContentType.Axis,
                    this,
                    ContentInfo,
                    animate);
            }
        }

        internal void DoRenderAxis(bool animate)
        {
            if (View.IsDisabled())
            {
                return;
            }

            double lastPos = 0.0;
            if (LabelPositions != null &&
                LabelPositions.Count > 0)
            {
                lastPos = LabelPositions.Last().Value;
            }
            RenderAxisOverride(animate);
            double currPos = 1.0;
            if (LabelPositions != null &&
                LabelPositions.Count > 0)
            {
                currPos = LabelPositions.Last().Value;
            }
            if (currPos != lastPos || MustInvalidateLabels)
            {
                MustInvalidateLabels = false;
                if (View.Ready())
                {
                    View.LabelNeedRearrange();
                }
            }

            View.EnsureRender();
            EnsureExtentUpdated();
        }

        private void EnsureExtentUpdated()
        {
            if (!View.Ready())
            {
                return;
            }
            if (!HasUserExtent())
            {
                View.EnsureAutoExtent();
            }
        }

        internal bool RangeDirty { get; set; }

        /// <summary>
        /// Updates the axis range.
        /// </summary>
        public bool UpdateRange()
        {
            return this.UpdateRange(false);
        }
        /// <summary>
        /// Updates the axis range.
        /// </summary>
        /// <param name="immediate">True if the change should be made immediately, or False if it can be deferred to the next refresh.</param>
        /// <returns>True if the axis range has changed.</returns>
        public bool UpdateRange(bool immediate)
        {
            if (immediate || this.SeriesViewer == null)
            {
                bool ret = UpdateRangeOverride();
                if (ret)
                {
                    MustInvalidateLabels = true;
                }
                RangeDirty = false;
                return ret;
            }

            if (!RangeDirty)
            {
                RangeDirty = true;
                SeriesViewer.ChartContentManager.RangeDirty(this, ContentInfo);
            }

            return false;
        }

        internal virtual bool UpdateRangeOverride()
        {
            return false;
        }

        /// <summary>
        /// Creates or updates the visuals for the axis.
        /// </summary>
        /// <param name="animate">Whether the updates to the visuals should be animated.</param>
        protected virtual void RenderAxisOverride(bool animate)
        {
           
        }

        #region INotifyPropertyChanged implementation

        /// <summary>
        /// Occurs when a property (including "effective" and non-dependency property) value changes.
        /// </summary>
        [SuppressWidgetMember]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when a property (including "effective" and non-dependency property) value changes.
        /// </summary>
        [SuppressWidgetMember]
        public event PropertyUpdatedEventHandler PropertyUpdated;

        /// <summary>
        /// Raises the property changed and updated events.
        /// </summary>
        /// <param name="name">The name of the property being changed.</param>
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

        #endregion INotifyPropertyChanged implementation

        /// <summary>
        /// The series that are registered to this axis.
        /// </summary>
        /// 



        protected internal List<Series> Series { get; set; }


        /// <summary>
        /// Registers a series that uses an axis with the axis.
        /// </summary>
        /// <param name="series">The series to register.</param>
        /// <returns>If the registration was a success.</returns>
        public virtual bool RegisterSeries(Series series)
        {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            bool present = Series.Contains(series);
            if (!present)
            {
                Series.Add(series);
            }

            return !present;
        }

        /// <summary>
        /// Deregisters a series that uses an axis from the axis.
        /// </summary>
        /// <param name="series">The series to deregister.</param>
        /// <returns>If the deregistration was a success.</returns>
        public virtual bool DeregisterSeries(Series series)
        {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            bool present = Series.Contains(series);
            if (present)
            {
                Series.Remove(series);
            }

            return present;
        }

        internal const string LabelPropertyName = "Label";

        /// <summary>
        /// Identifies the Label dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(LabelPropertyName, typeof(object), typeof(Axis), new PropertyMetadata(null,
            (sender, e) =>
            {
                (sender as Axis).RaisePropertyChanged(LabelPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the axis label format string.
        /// </summary>

        [TypeConverter(typeof(ObjectConverter))]

        public object Label
        {
            get
            {
                return this.GetValue(LabelProperty);
            }
            set
            {
                this.SetValue(LabelProperty, value);
            }
        }

        /// <summary>
        /// Gets the label for a data item.
        /// </summary>
        /// <param name="dataItem">The data item to get the label for.</param>
        /// <returns>The requested label.</returns>
        protected internal virtual object GetLabel(object dataItem)
        {
            return View.GetLabelValue(dataItem);
        }

        /// <summary>
        /// Gets the scaled viewport value from an unscaled axis value.
        /// </summary>
        /// <param name="unscaledValue">The unscaled axis value.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns>The scaled viewport value.</returns>
        public virtual double GetScaledValue(double unscaledValue, ScalerParams p)
        {
            return 0;
        }

        /// <summary>
        /// Gets the scaled viewport value from an unscaled axis value.
        /// </summary>
        /// <param name="unscaledValue">The unscaled axis value.</param>
        /// <param name="windowRect">The current window of the chart.</param>
        /// <param name="viewportRect">The current viewport.</param>
        /// <returns>The scaled viewport value.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is deprecated. Use GetScaledValue(double unscaledValue, ScalerParams p) instead.")]
        public virtual double GetScaledValue(double unscaledValue, Rect windowRect, Rect viewportRect)
        {
            ScalerParams p = new ScalerParams(windowRect, viewportRect, IsInverted);
            return GetScaledValue(unscaledValue, p);
        }

        /// <summary>
        /// Get a list of scaled viewport values from a list of unscaled axis values.
        /// </summary>
        /// <param name="unscaledValues">The list of unscaled axis values.</param>
        /// <param name="p">Scaler parameters</param>
        public virtual void GetScaledValueList(IList<double> unscaledValues, ScalerParams p)
        {
            
        }

        /// <summary>
        /// Gets the unscaled axis value from an scaled viewport value.
        /// </summary>
        /// <param name="scaledValue">The scaled viewport value.</param>
        /// <param name="p">Scaler parameters</param>
        /// <returns>The unscaled axis value.</returns>
        public virtual double GetUnscaledValue(double scaledValue, ScalerParams p)
        {
            return 0.0;
        }

        /// <summary>
        /// Gets the unscaled axis value from an scaled viewport value.
        /// </summary>
        /// <param name="scaledValue">The scaled viewport value.</param>
        /// <param name="windowRect">The current window of the chart.</param>
        /// <param name="viewportRect">The current viewport.</param>
        /// <returns>The unscaled axis value.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is deprecated. Use GetUnscaledValue(double unscaledValue, ScalerParams p) instead.")]
        public virtual double GetUnscaledValue(double scaledValue, Rect windowRect, Rect viewportRect)
        {
            ScalerParams p = new ScalerParams(windowRect, viewportRect, IsInverted);
            return GetUnscaledValue(scaledValue, p);
        }

        /// <summary>
        /// Gets a list of unscaled axis values from a list of scaled viewport values.
        /// </summary>
        /// <param name="scaledValues">A list containing the scaled viewport values to unscale.</param>
        /// <param name="p">Scaler parameters</param>
        public virtual void GetUnscaledValueList(IList<double> scaledValues, ScalerParams p)
        {

        }
        /// <summary>
        /// Scales a value from axis space into screen space.
        /// </summary>
        /// <param name="unscaledValue">The unscaled axis value to scale.</param>
        /// <returns>The scaled value in screen coordinates.</returns>
        public double ScaleValue(double unscaledValue)
        {
            ScalerParams p = new ScalerParams(this.SeriesViewer.WindowRect, this.ViewportRect, IsInverted);
            p.EffectiveViewportRect = this.SeriesViewer.EffectiveViewport;
            return GetScaledValue(unscaledValue, p);
        }

        /// <summary>
        /// Gets the axis orientation.
        /// </summary>
        protected internal abstract AxisOrientation Orientation { get; }

        internal Size MeasuredSize { get; set; }


        /// <summary>
        /// Provides the behavior for the Measure pass of Silverlight layout. Classes can override this method to define their own Measure pass behavior.
        /// </summary>
        /// <param name="availableSize">The available size that this object can give to child objects. Infinity (<see cref="F:System.Double.PositiveInfinity"/>) can be specified as a value to indicate that the object will size to whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based on its calculations of the allocated sizes for child objects; or based on other considerations, such as a fixed container size.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            MeasuredSize = availableSize;
            return base.MeasureOverride(availableSize);
        }


        internal void OverrideViewport()
        {
            ViewportOverride = new Rect(0, 0, MeasuredSize.Width, MeasuredSize.Height);
        }
    
        internal AxisComponentsForView _axisComponentsForView = new AxisComponentsForView();
        internal AxisComponentsForView GetAxisComponentsForView()
        {

            _axisComponentsForView.RootCanvas = RootCanvas;
            _axisComponentsForView.Axis = this;

            _axisComponentsForView.LabelPanel = LabelPanel;

 	        return _axisComponentsForView;
        }

        internal bool HasUserExtent()
        {


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

            return LabelSettings != null && LabelSettings.HasUserExtent();

        }

        internal void DoRaiseRangeChanged(AxisRangeChangedEventArgs rangeChangedEventArgs)
        {
            RaiseRangeChanged(rangeChangedEventArgs);
        }

        internal Pool<TextBlock> TextBlocks { get; set; }

        /// <summary>
        /// Gets the visuals representing the Axis line.
        /// </summary>
        protected internal Path AxisLines 
        {
            get
            {
                return GetAxisComponentsFromView().AxisLines;
            }
        }
        /// <summary>
        /// Gets the visuals representing the major lines.
        /// </summary>
        protected internal Path MajorLines 
        {
            get
            {
                return GetAxisComponentsFromView().MajorLines;
            }
        }
        /// <summary>
        /// Gets the visuals representing the strips.
        /// </summary>
        protected internal Path Strips
        {
            get
            {
                return View.GetAxisComponentsFromView().Strips;
            }
        }

        /// <summary>
        /// Gets the visuals representing the minor lines.
        /// </summary>
        protected internal Path MinorLines 
        {
            get
            {
                return GetAxisComponentsFromView().MinorLines;
            }
        }

        internal SyncSettings GetSyncSettings()
        {
            return SyncManager.GetSyncSettings(_seriesViewer);
        }

        internal AxisComponentsFromView GetAxisComponentsFromView()
        {
            return View.GetAxisComponentsFromView();
        }

        /// <summary>
        /// Reference to the axis label settings class.
        /// </summary>
        protected AxisLabelSettings _currentLabelSettings = new AxisLabelSettings();
        internal AxisLabelSettings CurrentLabelSettings
        {
            get
            {
                return _currentLabelSettings;
            }
        }



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


        internal bool HasCrossingValue()
        {



            return ReadLocalValue(Axis.CrossingValueProperty) != DependencyProperty.UnsetValue;

        }

        /// <summary>
        /// Exports visual information about the series for use by external tools and functionality.
        /// </summary>
        public AxisVisualData ExportVisualData()
        {
            AxisVisualData avd = new AxisVisualData();
            avd.Viewport = ViewportRect;
            avd.Type = this.GetType().Name;
            avd.Name = Name;
            avd.AxisLine = new PathVisualData("axisLine", View.AxisLines);
            avd.MinorLines = new PathVisualData("minorLines", View.MinorLines);
            avd.MajorLines = new PathVisualData("majorLines", View.MajorLines);

            for (var i = 0; i < LabelPanel.LabelPositions.Count; i++)
            {
                var labelPosition = LabelPanel.LabelPositions[i];
                var labelContext = LabelPanel.LabelDataContext[i];

                var newLabelData = new AxisLabelVisualData()
                {
                    LabelPosition = labelPosition.Value,
                    LabelValue = labelContext
                };
                newLabelData.Appearance = AppearanceHelper
                    .FromTextElement(LabelPanel.TextBlocks[i]);

                avd.Labels.Add(newLabelData);
            }

            return avd;
        }

        protected internal IEnumerable<Series> DirectSeries()
        {





            for (var i = 0; i < Series.Count; i++)
            {
                var currentSeries = Series[i];

                yield return currentSeries;
            }
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