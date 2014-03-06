using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;



using System.Linq;

using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;




namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the base class for the pie chart.
    /// </summary>

    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenter))]

    [StyleTypedProperty(Property = LeaderLineStylePropertyName, StyleTargetType = typeof(Line))]
    [StyleTypedProperty(Property = ToolTipStylePropertyName, StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = OthersCategoryStylePropertyName, StyleTargetType = typeof(Slice))]
    [StyleTypedProperty(Property = SelectedStylePropertyName, StyleTargetType = typeof(Slice))]
    [Widget("PieChart")]
    public abstract class PieChartBase : Control, INotifyPropertyChanged
    {
        internal virtual PieChartBaseView CreateView()
        {
            return new PieChartBaseView(this);
        }
        internal virtual void OnViewCreated(PieChartBaseView view)
        {
            View = (PieChartBaseView)view;
        }
        internal PieChartBaseView View { get; set; }

        #region C'tor and Initialization

        /// <summary>
        /// Creates a new instance of PieChartBase
        /// </summary>
        public PieChartBase()
        {
            PieChartBaseView view = CreateView();
            OnViewCreated(view);
            view.OnInit();

            DefaultStyleKey = typeof(PieChartBase);

            ValueIndices = new List<int>();
            OthersValueIndices = new List<int>();
            Others = new List<object>();

            ToolTipManager = new PieChartToolTipManager(this);
            View.InitToolTipManager(ToolTipManager);


            _propertyUpdatedOverride = (o, e) => { PropertyUpdatedOverride(o, e.PropertyName, e.OldValue, e.NewValue); };

            _brushesChangedOverride = (o, e) =>
            {
                RenderSlices();
                RenderLegendItems();
            };

            _explodedIndicesChangedOverride = (o, e) =>
            {
                if (AllowSliceExplosion)
                {
                    PrepareSlices();
                    PrepareLabels();
                    RenderSlices();
                    RenderLabels();
                }
            };

            _selectedIndicesChangedOverride = (o, e) =>
            {
                if (AllowSliceSelection)
                {
                    PrepareSlices();
                    RenderSlices();
                    RenderLegendItems();
                }
            };

            FastItemsSource_Event = (o, e) =>
            {
                DataUpdatedOverride(e.Action, e.Position, e.Count, e.PropertyName);
            };

            PropertyUpdated += _propertyUpdatedOverride;
            SelectedSlices.CollectionChanged += _selectedIndicesChangedOverride;
            _selectedAttached = true;
            ExplodedSlices.CollectionChanged += _explodedIndicesChangedOverride;
            _explodedAttached = true;

            Slices = new Pool<Slice>
            {
                Create = View.SliceCreate,
                Activate = View.SliceActivate,
                Disactivate = View.SliceDisactivate,
                Destroy = View.SliceDestroy
            };
            Labels = new Pool<PieLabel>
            {
                Create = View.LabelCreate,
                Activate = View.LabelActivate,
                Disactivate = View.LabelDisactivate,
                Destroy = View.LabelDestroy
            };


            Loaded += PieChartBase_Loaded;
            Unloaded += PieChartBase_Unloaded;
            
            // [DN May 23, 2012 : 91722] hack to get a notification when FontSize changes
            this.SetBinding(PieChartBase.FontSizeProxyProperty, new Binding("FontSize") { Source = this });


        }

        private bool _brushesAttached = false;
        private bool _outlinesAttached = false;
        private bool _fastItemsSourceAttached = false;
        private bool _selectedAttached = false;
        private bool _explodedAttached = false;


        void PieChartBase_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Brushes != null && _brushesAttached)
            {
                Brushes.CollectionChanged -= _brushesChangedOverride;
                _brushesAttached = false;
            }
            if (Outlines != null && _outlinesAttached)
            {
                Outlines.CollectionChanged -= _brushesChangedOverride;
                _outlinesAttached = false;
            }
            if (FastItemsSource != null && _fastItemsSourceAttached)
            {
                FastItemsSource.Event -= FastItemsSource_Event;
                _fastItemsSourceAttached = false;
            }
            if (ExplodedSlices != null && _explodedAttached)
            {
                ExplodedSlices.CollectionChanged -= _explodedIndicesChangedOverride;
                _explodedAttached = false;
            }
            if (SelectedSlices != null && _selectedAttached)
            {
                SelectedSlices.CollectionChanged -= _selectedIndicesChangedOverride;
                _selectedAttached = false;
            }
        }


        void PieChartBase_Loaded(object sender, RoutedEventArgs e)
        {
            bool needsRender = false;

            if (Brushes != null && !_brushesAttached)
            {
                Brushes.CollectionChanged += _brushesChangedOverride;
                _brushesAttached = true;
                needsRender = true;
            }
            if (Outlines != null && !_outlinesAttached)
            {
                Outlines.CollectionChanged += _brushesChangedOverride;
                _outlinesAttached = true;
                needsRender = true;
            }
            if (FastItemsSource != null && !_fastItemsSourceAttached)
            {
                FastItemsSource.Event += FastItemsSource_Event;
                _fastItemsSourceAttached = true;
                needsRender = true;
            }
            if (ExplodedSlices != null && !_explodedAttached)
            {
                ExplodedSlices.CollectionChanged += _explodedIndicesChangedOverride;
                _explodedAttached = true;
                needsRender = true;
            }
            if (SelectedSlices != null && !_selectedAttached)
            {
                SelectedSlices.CollectionChanged += _selectedIndicesChangedOverride;
                _selectedAttached = true;
                needsRender = true;
            }

            if (needsRender)
            {
                RenderChart();
            }
        }


        static PieChartBase()
        {
            ToolTipService.IsEnabledProperty.OverrideMetadata(typeof(PieChartBase), new FrameworkPropertyMetadata(false));
        }



        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ContentPresenter = GetTemplateChild(ContentPresenterName) as ContentPresenter;

            if (ContentPresenter == null)
                return;

            View.OnTemplateProvided();
        }


        #endregion C'tor and Initialization

        #region Non-public members

        private readonly PropertyUpdatedEventHandler _propertyUpdatedOverride;
        private readonly NotifyCollectionChangedEventHandler _brushesChangedOverride;
        private readonly NotifyCollectionChangedEventHandler _selectedIndicesChangedOverride;
        private readonly NotifyCollectionChangedEventHandler _explodedIndicesChangedOverride;

        #region ContentPresenter

        private const string ContentPresenterName = "ContentPresenter";


        private ContentPresenter _contentPresenter;

        /// <summary>
        /// Gets the current Chart's root ContentPresenter.
        /// </summary>
        public ContentPresenter ContentPresenter
        {
            get
            {
                return _contentPresenter;
            }
            private set
            {
                if (ContentPresenter != null)
                {
                    ContentPresenter.Content = null;
                }

                _contentPresenter = value;

                if (ContentPresenter != null)
                {
                    View.OnContentPresenterProvided();
                }
            }
        }


        #endregion ContentPresenter

        #region FastItemsSource and FastItemsSource Event Handlers

        private EventHandler<FastItemsSourceEventArgs> FastItemsSource_Event;
        internal const string FastItemsSourcePropertyName = "FastItemsSource";

        /// <summary>
        /// Identifies the FastItemsSource dependency property.
        /// </summary>
        internal static readonly DependencyProperty FastItemsSourceProperty = DependencyProperty.Register(FastItemsSourcePropertyName, typeof(FastItemsSource), typeof(PieChartBase),
            new PropertyMetadata((sender, e) =>
            {
                (sender as PieChartBase).RaisePropertyChanged(FastItemsSourcePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the ItemsSource property for the pie chart.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <para>
        /// The FastItemsSource is a proxy which handles caching, conversion and
        /// weak event listeners.
        /// </para>
        internal FastItemsSource FastItemsSource
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

        #endregion FastItemsSource and FastItemsSource Event Handlers

        #region ValueColumn

        private IFastItemColumn<double> _valueColumn;

        /// <summary>
        /// Gets the FastItemColumn representing the mapped values in the items source.
        /// </summary>
        protected internal IFastItemColumn<double> ValueColumn
        {
            get { return _valueColumn; }
            private set
            {
                if (_valueColumn != value)
                {
                    IFastItemColumn<double> oldValueColumn = _valueColumn;

                    _valueColumn = value;
                    RaisePropertyChanged(ValueColumnPropertyName, oldValueColumn, _valueColumn);
                }
            }
        }

        internal const string ValueColumnPropertyName = "ValueColumn";

        #endregion ValueColumn

        internal double OthersTotal { get; set; }

        internal double Total { get; set; }

        internal List<int> ValueIndices { get; set; }

        internal List<int> OthersValueIndices { get; set; }

        internal List<object> Others { get; set; }

        internal double ActualStartAngle { get; set; }

        internal List<UIElement> LegendItems { get; set; }

        internal double ChartInnerExtent
        {
            get
            {
                return 0;
            }
        }
        

        internal PieChartToolTipManager ToolTipManager { get; set; }


        #endregion Non-public members

        #region Public properties

        #region ItemsSource

        internal const string ItemsSourcePropertyName = "ItemsSource";

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(ItemsSourcePropertyName, typeof(IEnumerable), typeof(PieChartBase),
            new PropertyMetadata(null, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(ItemsSourcePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the data source for the chart.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get
            {
                return (IEnumerable)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        #endregion ItemsSource

        #region ValueMemberPath

        internal const string ValueMemberPathPropertyName = "ValueMemberPath";

        /// <summary>
        /// Identifies the ValueMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(ValueMemberPathPropertyName, typeof(string), typeof(PieChartBase),
            new PropertyMetadata(null, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(ValueMemberPathPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or Sets the property name that contains the values.
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

        #endregion ValueMemberPath

        #region LabelMemberPath Dependency Property

        internal const string LabelMemberPathPropertyName = "LabelMemberPath";

        /// <summary>
        /// Identifies the LabelMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelMemberPathProperty = DependencyProperty.Register(LabelMemberPathPropertyName, typeof(string), typeof(PieChartBase),
            new PropertyMetadata((o, e) =>
        {
            (o as PieChartBase).RaisePropertyChanged(LabelMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the property name that contains the labels.
        /// </summary>
        public string LabelMemberPath
        {
            get
            {
                return (string)GetValue(LabelMemberPathProperty);
            }
            set
            {
                SetValue(LabelMemberPathProperty, value);
            }
        }

        #endregion LabelMemberPath Dependency Property

        #region LabelColumn internal Dependency Property

        internal const string LabelColumnPropertyName = "LabelColumn";
        private IFastItemColumn<object> _labelColumn;

        /// <summary>
        /// Gets the data column used for labels.
        /// </summary>
        protected internal IFastItemColumn<object> LabelColumn
        {
            get { return _labelColumn; }
            private set
            {
                if (_labelColumn != value)
                {
                    IFastItemColumn<object> oldColumn = LabelColumn;

                    _labelColumn = value;
                    RaisePropertyChanged(LabelColumnPropertyName, oldColumn, LabelColumn);
                }
            }
        }

        #endregion LabelColumn internal Dependency Property

        #region LabelPosition

        internal const string LabelsPositionPropertyName = "LabelsPosition";

        /// <summary>
        /// Identifies the LabelsPosition dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelsPositionProperty = DependencyProperty.Register(LabelsPositionPropertyName, typeof(LabelsPosition), typeof(PieChartBase),
            new PropertyMetadata(LabelsPosition.Center, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(LabelsPositionPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the position of chart labels.
        /// </summary>
        [WidgetDefaultString("center")]
        public LabelsPosition LabelsPosition
        {
            get
            {
                return (LabelsPosition)GetValue(LabelsPositionProperty);
            }
            set
            {
                SetValue(LabelsPositionProperty, value);
            }
        }

        #endregion LabelPosition

        #region LeaderLineVisibility

        internal const string LeaderLineVisibilityPropertyName = "LeaderLineVisibility";

        /// <summary>
        /// Identifies the LeaderLineVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty LeaderLineVisibilityProperty = DependencyProperty.Register(LeaderLineVisibilityPropertyName, typeof(Visibility), typeof(PieChartBase),
            new PropertyMetadata(Visibility.Visible, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(LeaderLineVisibilityPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets whether the leader lines are visible.
        /// </summary>
        [WidgetDefaultString("visible")]
        public Visibility LeaderLineVisibility
        {
            get
            {
                return (Visibility)GetValue(LeaderLineVisibilityProperty);
            }
            set
            {
                SetValue(LeaderLineVisibilityProperty, value);
            }
        }

        #endregion LeaderLineVisibility

        #region LeaderLineStyle

        internal const string LeaderLineStylePropertyName = "LeaderLineStyle";

        /// <summary>
        /// Identifies the LeaderLineStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty LeaderLineStyleProperty = DependencyProperty.Register(LeaderLineStylePropertyName, typeof(Style), typeof(PieChartBase),
            new PropertyMetadata(null, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(LeaderLineStylePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the style for the leader lines.
        /// </summary>
        [SuppressWidgetMember]
        public Style LeaderLineStyle
        {
            get
            {
                return (Style)GetValue(LeaderLineStyleProperty);
            }
            set
            {
                SetValue(LeaderLineStyleProperty, value);
            }
        }

        #endregion LeaderLineStyle

        #region ToolTip

        internal const string ToolTipPropertyName = "ToolTip";

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == FrameworkElement.ToolTipProperty)
            {
                this.PropertyUpdatedOverride(this, ToolTipPropertyName, e.OldValue, e.NewValue);
            }
        }


#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)


        #endregion ToolTip

        #region OthersCategoryThreshold

        internal const string OthersCategoryThresholdPropertyName = "OthersCategoryThreshold";

        /// <summary>
        /// Identifies the OthersCategoryThreshold dependency property.
        /// </summary>
        public static readonly DependencyProperty OthersCategoryThresholdProperty = DependencyProperty.Register(OthersCategoryThresholdPropertyName, typeof(double), typeof(PieChartBase),
            new PropertyMetadata(3.0, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(OthersCategoryThresholdPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the threshold value that determines if slices are grouped into the Others slice.
        /// </summary>
        [WidgetDefaultNumber(3.0)]
        public double OthersCategoryThreshold
        {
            get
            {
                return (double)GetValue(OthersCategoryThresholdProperty);
            }
            set
            {
                SetValue(OthersCategoryThresholdProperty, value);
            }
        }

        #endregion OthersCategoryThreshold

        #region OthersCategoryType

        internal const string OthersCategoryTypePropertyName = "OthersCategoryType";

        /// <summary>
        /// Identifies the OthersCategoryType dependency property.
        /// </summary>
        public static readonly DependencyProperty OthersCategoryTypeProperty = DependencyProperty.Register(OthersCategoryTypePropertyName, typeof(OthersCategoryType), typeof(PieChartBase),
            new PropertyMetadata(OthersCategoryType.Percent, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(OthersCategoryTypePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets whether to use numeric or percent-based threshold value.
        /// </summary>
        [WidgetDefaultString("percent")]
        public OthersCategoryType OthersCategoryType
        {
            get
            {
                return (OthersCategoryType)GetValue(OthersCategoryTypeProperty);
            }
            set
            {
                SetValue(OthersCategoryTypeProperty, value);
            }
        }

        #endregion OthersCategoryType

        #region OthersCategoryText

        internal const string OthersCategoryTextPropertyName = "OthersCategoryText";

        /// <summary>
        /// Identifies the OthersCateogryText dependency property.
        /// </summary>
        public static readonly DependencyProperty OthersCategoryTextProperty = DependencyProperty.Register(OthersCategoryTextPropertyName, typeof(string), typeof(PieChartBase),
            new PropertyMetadata("Others", (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(OthersCategoryTextPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the label of the Others slice.
        /// </summary>
        [WidgetDefaultString("Others")]
        public string OthersCategoryText
        {
            get
            {
                return (string)GetValue(OthersCategoryTextProperty);
            }
            set
            {
                SetValue(OthersCategoryTextProperty, value);
            }
        }

        #endregion OthersCategoryText

        #region ExplodedRadius

        internal const string ExplodedRadiusPropertyName = "ExplodedRadius";

        /// <summary>
        /// Identifies the ExplodedRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty ExplodedRadiusProperty = DependencyProperty.Register(ExplodedRadiusPropertyName, typeof(double), typeof(PieChartBase),
            new PropertyMetadata(0.2, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(ExplodedRadiusPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Determines how much the exploded slice is offset from the center. Value between 0 and 1.
        /// </summary>
        [WidgetDefaultNumber(0.2)]
        public double ExplodedRadius
        {
            get
            {
                return (double)GetValue(ExplodedRadiusProperty);
            }
            set
            {
                SetValue(ExplodedRadiusProperty, value);
            }
        }

        /// <summary>
        /// Gets a coerced ExplodedRadius property value between 0 and 1.
        /// </summary>
        internal double ActualExplodedRadius
        {
            get
            {
                double radius = ExplodedRadius;
                if (double.IsNaN(radius) || double.IsInfinity(radius) || radius < 0) return 0;
                if (radius > 1) return 1;
                return radius;
            }
        }

        #endregion ExplodedRadius

        #region RadiusFactor

        internal const string RadiusFactorPropertyName = "RadiusFactor";

        /// <summary>
        /// Identifies the RadiusFactor dependency property.
        /// </summary>        
        public static DependencyProperty RadiusFactorProperty = DependencyProperty.Register(RadiusFactorPropertyName, typeof(double), typeof(PieChartBase),
            new PropertyMetadata(.9, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(RadiusFactorPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the scaling factor of the chart's radius. Value between 0 and 1.
        /// </summary>
        [WidgetDefaultNumber(.9)]
        public double RadiusFactor
        {
            get
            {
                return (double)GetValue(RadiusFactorProperty);
            }
            set
            {
                SetValue(RadiusFactorProperty, value);
            }
        }

        /// <summary>
        /// Gets a coerced version of RadiusFactor property between 0 and 1.
        /// </summary>
        internal double ActualRadiusFactor
        {
            get
            {
                double radiusFactor = RadiusFactor;
                if (double.IsNaN(radiusFactor) || double.IsInfinity(radiusFactor) || radiusFactor < 0) return 0;
                if (radiusFactor > 1) return 1;
                return radiusFactor;
            }
        }

        #endregion RadiusFactor

        #region AllowSliceSelection

        internal const string AllowSliceSelectionPropertyName = "AllowSliceSelection";

        /// <summary>
        /// Identifies the AllowSliceSelection dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowSliceSelectionProperty = DependencyProperty.Register(AllowSliceSelectionPropertyName, typeof(bool), typeof(PieChartBase),
            new PropertyMetadata(true, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(AllowSliceSelectionPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets whether the slices can be selected.
        /// </summary>
        public bool AllowSliceSelection
        {
            get
            {
                return (bool)GetValue(AllowSliceSelectionProperty);
            }
            set
            {
                SetValue(AllowSliceSelectionProperty, value);
            }
        }

        #endregion AllowSliceSelection

        #region AllowSliceExplosion

        internal const string AllowSliceExplosionPropertyName = "AllowSliceExplosion";

        /// <summary>
        /// Identifies the AllowSliceExplosion dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowSliceExplosionProperty = DependencyProperty.Register(AllowSliceExplosionPropertyName, typeof(bool), typeof(PieChartBase),
            new PropertyMetadata(true, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(AllowSliceExplosionPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets whether the slices can be exploded.
        /// </summary>
        public bool AllowSliceExplosion
        {
            get
            {
                return (bool)GetValue(AllowSliceExplosionProperty);
            }
            set
            {
                SetValue(AllowSliceExplosionProperty, value);
            }
        }

        #endregion AllowSliceExplosion

        #region ExplodedSlices

        /// <summary>
        /// Gets or sets the collection of exploded slice indices.
        /// </summary>
        [TypeConverter(typeof(IndexCollectionTypeConverter))]
        [SuppressWidgetMemberCopy]
        public IndexCollection ExplodedSlices
        {
            get
            {
                return _explodedSlices;
            }
            set
            {
                _explodedSlices.CollectionChanged -= _explodedIndicesChangedOverride;
                _explodedAttached = false;
                _explodedSlices = value;
                if (_explodedSlices != null)
                {
                    _explodedSlices.CollectionChanged += _explodedIndicesChangedOverride;
                    _explodedAttached = true;
                }

                if (AllowSliceExplosion)
                {
                    PrepareSlices();
                    PrepareLabels();
                    RenderSlices();
                    RenderLabels();
                }
            }
        }

        private IndexCollection _explodedSlices = new IndexCollection();

        #endregion ExplodedSlices

        #region Legend

        internal const string LegendPropertyName = "Legend";

        /// <summary>
        /// Identifies the Legend dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendProperty = DependencyProperty.Register(LegendPropertyName, typeof(LegendBase), typeof(PieChartBase),
            new PropertyMetadata(null, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(LegendPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the legend used for the current chart.
        /// </summary>
        [SuppressWidgetMemberCopy]
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

        #endregion Legend

        #region Label Extent

        internal const string LabelExtentPropertyName = "LabelExtent";

        /// <summary>
        /// Identifies the LabelExtent dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelExtentProperty = DependencyProperty.Register(LabelExtentPropertyName, typeof(double), typeof(PieChartBase),
            new PropertyMetadata(10.0, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(LabelExtentPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the pixel amount, by which the labels are offset from the edge of the slices.
        /// </summary>
        [WidgetDefaultNumber(10.0)]
        public double LabelExtent
        {
            get
            {
                return (double)GetValue(LabelExtentProperty);
            }
            set
            {
                SetValue(LabelExtentProperty, value);
            }
        }

        #endregion Label Extent

        #region StartAngle

        internal const string StartAnglePropertyName = "StartAngle";

        /// <summary>
        /// Identifies the StartAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty = DependencyProperty.Register(StartAnglePropertyName, typeof(double), typeof(PieChartBase),
            new PropertyMetadata(0.0, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(StartAnglePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the starting angle of the chart.
        /// </summary>
        /// <remarks>
        /// The default zero value is equivalent to 3 o'clock.
        /// </remarks>
        [WidgetDefaultNumber(0.0)]
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set
            {
                SetValue(StartAngleProperty, value);
            }
        }

        #endregion StartAngle

        #region SweepDirection

        internal const string SweepDirectionPropertyName = "SweepDirection";

        /// <summary>
        /// Identifies the SweepDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty SweepDirectionProperty = DependencyProperty.Register(SweepDirectionPropertyName, typeof(SweepDirection), typeof(PieChartBase),
            new PropertyMetadata(SweepDirection.Clockwise, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(SweepDirectionPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the rotational direction of the chart.
        /// </summary>
        [WidgetDefaultString("clockwise")]
        public SweepDirection SweepDirection
        {
            get { return (SweepDirection)GetValue(SweepDirectionProperty); }
            set { SetValue(SweepDirectionProperty, value); }
        }

        #endregion SweepDirection

        #region SelectedSlices

        /// <summary>
        /// Gets or sets the collection of selected slice indices.
        /// </summary>
        [TypeConverter(typeof(IndexCollectionTypeConverter))]
        [SuppressWidgetMember]
        public IndexCollection SelectedSlices
        {
            get
            {
                return _selectedSlices;
            }
            set
            {
                _selectedSlices.CollectionChanged -= _selectedIndicesChangedOverride;
                _selectedAttached = false;
                _selectedSlices = value;
                if (_selectedSlices != null)
                {
                    _selectedSlices.CollectionChanged += _selectedIndicesChangedOverride;
                    _selectedAttached = true;
                }

                if (AllowSliceSelection)
                {
                    PrepareSlices();
                    RenderSlices();
                    RenderLegendItems();
                }
            }
        }

        private IndexCollection _selectedSlices = new IndexCollection();

        #endregion SelectedSlices

        #region OthersCategoryStyle

        internal const string OthersCategoryStylePropertyName = "OthersCategoryStyle";

        /// <summary>
        /// Identifies the OthersCategoryStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty OthersCategoryStyleProperty = DependencyProperty.Register(OthersCategoryStylePropertyName, typeof(Style), typeof(PieChartBase),
            new PropertyMetadata(null, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(OthersCategoryStylePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the style used for the Others slice.
        /// </summary>
        [SuppressWidgetMember]
        public Style OthersCategoryStyle
        {
            get { return (Style)GetValue(OthersCategoryStyleProperty); }
            set { SetValue(OthersCategoryStyleProperty, value); }
        }

        #endregion OthersCategoryStyle

        #region SelectedStyle

        internal const string SelectedStylePropertyName = "SelectedStyle";

        /// <summary>
        /// Identifies the SelectedStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedStyleProperty = DependencyProperty.Register(SelectedStylePropertyName, typeof(Style), typeof(PieChartBase),
            new PropertyMetadata(null, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(SelectedStylePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the style used when a slice is selected.
        /// </summary>
        public Style SelectedStyle
        {
            get { return (Style)GetValue(SelectedStyleProperty); }
            set { SetValue(SelectedStyleProperty, value); }
        }

        #endregion SelectedStyle

        #region ToolTipStyle

        internal const string ToolTipStylePropertyName = "ToolTipStyle";

        /// <summary>
        /// Identifies the ToolTipStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolTipStyleProperty = DependencyProperty.Register(ToolTipStylePropertyName, typeof(Style), typeof(PieChartBase),
            new PropertyMetadata(null, (o, e) =>
            {
                (o as PieChartBase).RaisePropertyChanged(ToolTipStylePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the style used for the tooltip.
        /// </summary>
        [SuppressWidgetMember]
        public Style ToolTipStyle
        {
            get { return (Style)GetValue(ToolTipStyleProperty); }
            set { SetValue(ToolTipStyleProperty, value); }
        }

        #endregion ToolTipStyle

        #region Brushes

        internal const string BrushesPropertyName = "Brushes";

        /// <summary>
        /// Identifies the Brushes dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushesProperty = DependencyProperty.Register(BrushesPropertyName, typeof(BrushCollection), typeof(PieChartBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as PieChartBase).RaisePropertyChanged(BrushesPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Brushes property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The brushes property defines the palette from which automatically assigned slice brushes are selected.
        /// </remarks>
        public BrushCollection Brushes
        {
            get
            {
                return (BrushCollection)GetValue(BrushesProperty);
            }
            set
            {
                SetValue(BrushesProperty, value);
            }
        }

        #endregion Brushes

        #region Outlines

        internal const string OutlinesPropertyName = "Outlines";

        /// <summary>
        /// Identifies the Outlines dependency property.
        /// </summary>
        public static readonly DependencyProperty OutlinesProperty = DependencyProperty.Register(OutlinesPropertyName, typeof(BrushCollection), typeof(PieChartBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as PieChartBase).RaisePropertyChanged(OutlinesPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Outlines property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The Outlines property defines the palette from which automatically assigned slice outlines are selected.
        /// </remarks>
        public BrushCollection Outlines
        {
            get
            {
                return (BrushCollection)GetValue(OutlinesProperty);
            }
            set
            {
                SetValue(OutlinesProperty, value);
            }
        }

        #endregion Outlines

        #region LegendItemTemplate Dependency Property

        /// <summary>
        /// Gets or sets the LegendItemTemplate property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The legend item control content is created according to the LegendItemTemplate on-demand by
        /// the chart object itself.
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
        public static readonly DependencyProperty LegendItemTemplateProperty = DependencyProperty.Register(LegendItemTemplatePropertyName, typeof(DataTemplate), typeof(PieChartBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as PieChartBase).RaisePropertyChanged(LegendItemTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion LegendItemTemplate Dependency Property

        #region LegendItemBadgeTemplate Dependency Property

        /// <summary>
        /// Gets or sets the LegendItemBadgeTemplate property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The legend item badge is created according to the LegendItemBadgeTemplate on-demand by
        /// the chart object itself.
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
        public static readonly DependencyProperty LegendItemBadgeTemplateProperty = DependencyProperty.Register(LegendItemBadgeTemplatePropertyName, typeof(DataTemplate), typeof(PieChartBase),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as PieChartBase).RaisePropertyChanged(LegendItemBadgeTemplatePropertyName, e.OldValue, e.NewValue);
            }));

        #endregion LegendItemBadgeTemplate Dependency Property

        #region LabelTemplate Dependency Property
        private const string LabelTemplatePropertyName = "LabelTemplate";
        /// <summary>
        /// Identifies the LabelTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelTemplateProperty = DependencyProperty.Register(LabelTemplatePropertyName, typeof(DataTemplate), typeof(PieChartBase), new PropertyMetadata(null, (sender, e) =>
            {
                (sender as PieChartBase).RaisePropertyChanged(LabelTemplatePropertyName, e.OldValue, e.NewValue);
            }));
        /// <summary>
        /// The DataTemplate to use when creating pie chart labels.
        /// </summary>
        [SuppressWidgetMember]
        public DataTemplate LabelTemplate
        {
            get
            {
                return this.GetValue(LabelTemplateProperty) as DataTemplate;
            }
            set
            {
                this.SetValue(LabelTemplateProperty, value);
            }
        }
        #endregion

        #endregion Public properties

        #region Events

        #region SliceClick

        /// <summary>
        /// Raised when the slice is clicked.
        /// </summary>
        public event SliceClickEventHandler SliceClick;

        private void OnSliceClick(object sender, SliceClickEventArgs e)
        {
            if (SliceClick != null)
            {
                SliceClick(sender, e);
            }
        }

        #endregion SliceClick

        #endregion Events

        #region Slice Pool Management

        internal Pool<Slice> Slices;

        #endregion Slice Pool Management

        #region Label Pool Management

        internal Pool<PieLabel> Labels;

        #endregion Label Pool Management

        #region Methods

        #region Helpers

        internal void ExplodeSlice(Slice slice, bool explode)
        {
            if (!ExplodedSlices.Contains(slice.Index) && explode)
            {
                ExplodedSlices.Add(slice.Index);
            }

            if (ExplodedSlices.Contains(slice.Index) && !explode)
            {
                ExplodedSlices.Remove(slice.Index);
            }
        }

        internal void SelectSlice(Slice slice, bool shouldSelect)
        {
            if (!SelectedSlices.Contains(slice.Index) && shouldSelect)
            {
                SelectedSlices.Add(slice.Index);
            }

            if (SelectedSlices.Contains(slice.Index) && !shouldSelect)
            {
                SelectedSlices.Remove(slice.Index);
            }
        }

        internal virtual void SetSliceAppearance(Slice slice)
        {
            View.SetSliceAppearance(slice);
        }

        /// <summary>
        /// Gets the label for a data item.
        /// </summary>
        /// <param name="slice">The data item to get the label for.</param>
        /// <returns>The requested label.</returns>
        protected internal virtual object GetLabel(Slice slice)
        {
            return View.GetLabel(slice);
        }

        internal Rect GetSliceInnerBounds(Slice slice, LabelsPosition position)
        {
            Rect bounds = new Rect(0,0,0,0);
            return bounds;
        }

        internal bool FitsInsideBounds(PieLabel label, Point center)
        {
            Slice slice = label.Slice;
            if (slice == null) return false;
            Point origin = slice.GetSliceOrigin();
            double startAngle = SweepDirection == SweepDirection.Clockwise ? slice.StartAngle : slice.EndAngle;
            double endAngle = SweepDirection == SweepDirection.Clockwise ? slice.EndAngle : slice.StartAngle;
            bool useAngleOffset = false;
            bool isCircle = false;

            Point startPoint = GeometryUtil.FindRadialPoint(origin, startAngle, slice.Radius);
            Point endPoint = GeometryUtil.FindRadialPoint(origin, endAngle, slice.Radius);

            startAngle = FindAngle(startPoint.X, origin.X, startPoint.Y, origin.Y);
            endAngle = FindAngle(endPoint.X, origin.X, endPoint.Y, origin.Y);

            if (IsCircle(slice))
            {
                //this is a complete circle.
                isCircle = true;
            }

            double labelRadius;
            labelRadius = MathUtil.Hypot(label.Bounds.Right - origin.X, label.Bounds.Top - origin.Y);
            if (labelRadius > slice.Radius) return false;
            labelRadius = MathUtil.Hypot(label.Bounds.Right - origin.X, label.Bounds.Bottom - origin.Y);
            if (labelRadius > slice.Radius) return false;
            labelRadius = MathUtil.Hypot(label.Bounds.Left - origin.X, label.Bounds.Top - origin.Y);
            if (labelRadius > slice.Radius) return false;
            labelRadius = MathUtil.Hypot(label.Bounds.Left - origin.X, label.Bounds.Bottom - origin.Y);
            if (labelRadius > slice.Radius) return false;

            if (isCircle) return true;

            if (startAngle > endAngle)
            {
                //the wedge intersects the 0 degrees line.
                startAngle = startAngle - 360;
                useAngleOffset = true;
            }

            double labelAngle;

            //examine the top right point
            labelAngle = FindAngle(label.Bounds.Right, origin.X, label.Bounds.Top, origin.Y);
            if (useAngleOffset && labelAngle > 180 && labelAngle < 360) labelAngle = labelAngle - 360;
            if (labelAngle < startAngle || labelAngle > endAngle) return false;

            //examine the bottom right point
            labelAngle = FindAngle(label.Bounds.Right, origin.X, label.Bounds.Bottom, origin.Y);
            if (useAngleOffset && labelAngle > 180 && labelAngle < 360) labelAngle = labelAngle - 360;
            if (labelAngle < startAngle || labelAngle > endAngle) return false;

            //examine the top left point
            labelAngle = FindAngle(label.Bounds.Left, origin.X, label.Bounds.Top, origin.Y);
            if (useAngleOffset && labelAngle > 180 && labelAngle < 360) labelAngle = labelAngle - 360;
            if (labelAngle < startAngle || labelAngle > endAngle) return false;

            //examine the bottom left point
            labelAngle = FindAngle(label.Bounds.Left, origin.X, label.Bounds.Bottom, origin.Y);
            if (useAngleOffset && labelAngle > 180 && labelAngle < 360) labelAngle = labelAngle - 360;
            if (labelAngle < startAngle || labelAngle > endAngle) return false;

            return true;
        }

        internal static double RoundAngle(double angle)
        {




            return Math.Round(Math.Abs(angle), 5);

        }

        private bool IsCircle(Slice slice)
        {
            return RoundAngle(Math.Abs(slice.EndAngle - slice.StartAngle)) == 360;
        }

        /// <summary>
        /// Calculates the angle in degrees on the unit circle.
        /// </summary>
        private static double FindAngle(double x, double centerX, double y, double centerY)
        {
            double h = MathUtil.Hypot(x - centerX, y - centerY);
            double angle = Math.Asin((y - centerY) / h) * 180 / Math.PI;

            if (x < centerX) angle = 180 - angle;
            if (x > centerX) angle = 360 + angle;

            if (angle == 360) angle = 0;

            return GeometryUtil.SimplifyAngle(angle);
        }

        private static void SortLabels(List<PieLabel> labels)
        {
            for (int i = 0; i < labels.Count; i++)
            {
                for (int j = i + 1; j < labels.Count; j++)
                {
                    if (labels[i].Bounds.Top > labels[j].Bounds.Top)
                    {
                        //swap
                        PieLabel temp = labels[i];
                        labels[i] = labels[j];
                        labels[j] = temp;
                    }
                }
            }
        }

        private void ResolveCollisions(List<PieLabel> labels)
        {
            if (labels.Count == 0) return;

            double renderWidth = RenderSize.Width;
            double renderHeight = RenderSize.Height;




            int count = labels.Count;
            double radius = labels[0].Slice.Radius;
            Point center = labels[0].Slice.GetSliceOrigin();
            bool hasEnoughSpace = true;
            int collisions = 0;
            double minHeight = double.PositiveInfinity;
            double maxHeight = double.NegativeInfinity;

            //check for collisions
            for (int i = 0; i < count - 1; i++)
            {
                PieLabel currentLabel = labels[i];
                PieLabel nextLabel = labels[i + 1];

                if (currentLabel.Bounds.IntersectsWith(nextLabel.Bounds))
                {
                    collisions++;
                }
            }

            double totalHeight = 0;
            foreach (var label in labels)
            {
                minHeight = Math.Min(minHeight, label.Bounds.Height);
                maxHeight = Math.Max(maxHeight, label.Bounds.Height);
                totalHeight += label.Bounds.Height;
            }

            if (totalHeight > renderHeight)
            {
                hasEnoughSpace = false;
            }

            if (hasEnoughSpace && collisions > 0)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        PieLabel currentLabel = labels[i];
                        PieLabel nextLabel = labels[j];

                        if (currentLabel.Bounds.IntersectsWith(nextLabel.Bounds))
                        {
                            Rect bounds = nextLabel.Bounds;
                            bounds.Y = Math.Min(currentLabel.Bounds.Bottom + 0.01, renderHeight - minHeight);

                            double c = LabelExtent + radius;
                            double b = Math.Abs(center.Y - (bounds.Y + minHeight / 2));
                            double x = Math.Sqrt(Math.Abs(c * c - b * b));

                            double angle = GeometryUtil.SimplifyAngle(nextLabel.Angle);
                            if (angle > 90 && angle < 270)
                            {
                                x = (bounds.Width + x) * -1;
                            }

                            bounds.X = center.X + x;
                            nextLabel.Bounds = bounds;
                        }
                    }
                }

                //sweep backwards
                for (int i = count - 1; i > 0; i--)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        PieLabel currentLabel = labels[i];
                        PieLabel nextLabel = labels[j];

                        if (currentLabel.Bounds.IntersectsWith(nextLabel.Bounds))
                        {
                            Rect bounds = nextLabel.Bounds;
                            bounds.Y = Math.Max(currentLabel.Bounds.Top - minHeight - 0.01, 0);

                            double c = LabelExtent + radius;
                            double b = Math.Abs(center.Y - (bounds.Y + minHeight / 2));
                            double x = Math.Sqrt(Math.Abs(c * c - b * b));

                            double angle = GeometryUtil.SimplifyAngle(nextLabel.Angle);
                            if (angle > 90 && angle < 270)
                            {
                                x = (bounds.Width + x) * -1;
                            }

                            bounds.X = center.X + x;
                            nextLabel.Bounds = bounds;
                        }
                    }
                }
            }

            foreach (var label in labels)
            {
                Rect bounds = label.Bounds;


                if (bounds.Left > renderWidth || bounds.Right < 0)
                {
                    label.Visibility = System.Windows.Visibility.Collapsed;
                    label.LeaderLine.Visibility = System.Windows.Visibility.Collapsed;
                }

                else if (bounds.Left < 0)
                {
                    double offset = Math.Abs(bounds.X);
                    bounds.X = 0;

                    if (offset > bounds.Width)
                    {
                        bounds.Width = 0;
                    }
                    else
                    {
                        bounds.Width -= offset;
                    }

                    label.Bounds = bounds;

                    label.Measure(new Size(bounds.Width, bounds.Height));

                }

                else if (bounds.Right > renderWidth)
                {
                    double offset = bounds.Right - renderWidth;

                    if (offset > bounds.Width)
                    {
                        bounds.Width = 0;
                    }
                    else
                    {
                        bounds.Width -= offset;
                    }

                    label.Bounds = bounds;

                    label.Measure(new Size(bounds.Width, bounds.Height));

                }

            }
        }

        /// <summary>
        /// Called whenever a change is made to the data in the ItemsSource.
        /// </summary>
        /// <param name="action">The action performed on the data</param>
        /// <param name="position">The index of the first item involved in the update.</param>
        /// <param name="count">The number of items in the update.</param>
        /// <param name="propertyName">The name of the updated property.</param>
        protected virtual void DataUpdatedOverride(FastItemsSourceEventAction action, int position, int count, string propertyName)
        {
            RenderChart();
        }

        #endregion Helpers

        /// <summary>
        /// Renders the piechart.
        /// </summary>
        protected internal virtual void RenderChart()
        {
            PrepareData();
            PrepareSlices();
            PrepareLabels();
            RenderSlices();
            RenderLabels();
            RenderLegendItems();
            View.UpdateView();
        }

        #region Preparers

        /// <summary>
        /// Extracts data from the data source.
        /// </summary>
        protected internal virtual void PrepareData()
        {
            Total = 0;
            OthersTotal = 0;
            ValueIndices.Clear();
            OthersValueIndices.Clear();
            Others.Clear();

            if (ItemsSource == null || FastItemsSource == null)
            {
                return;
            }

            if (ValueColumn == null || ValueColumn.Count == 0)
            {
                return;
            }

            foreach (var value in ValueColumn)
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                {
                    continue;
                }

                Total = Total + value;
            }

            for (int i = 0; i < ValueColumn.Count; i++)
            {
                double value = ValueColumn[i];

                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0) continue;

                double calculatedValue = OthersCategoryType == OthersCategoryType.Percent ? value / Total : value;
                double calculatedThreshold = OthersCategoryType == OthersCategoryType.Percent
                                                 ? OthersCategoryThreshold / 100
                                                 : OthersCategoryThreshold;

                if (calculatedValue <= calculatedThreshold)
                {
                    OthersTotal = OthersTotal + value;
                    OthersValueIndices.Add(i);
                    Others.Add(FastItemsSource[i]);
                }
                else
                {
                    ValueIndices.Add(i);
                }
            }
        }

        /// <summary>
        /// Prepares data needed to create pie slices.
        /// </summary>
        protected internal virtual void PrepareSlices()
        {
            if (ItemsSource == null || FastItemsSource == null)
            {
                Slices.Count = 0;
                return;
            }

            int totalSliceCount = ValueIndices.Count;
            bool hasOtherSlice = OthersValueIndices.Count > 0;
            double startAngle = RoundAngle(ActualStartAngle);
            double endAngle = RoundAngle(ActualStartAngle);

            if (hasOtherSlice)
            {
                //include the Others slice in the total.
                totalSliceCount++;
            }

            for (int i = 0; i < totalSliceCount; i++)
            {
                bool isOtherSlice = false;
                double value;

                if (i == totalSliceCount - 1 && hasOtherSlice)
                {
                    value = OthersTotal;
                    isOtherSlice = true;
                }
                else
                {
                    value = ValueColumn[ValueIndices[i]];
                }

                if (SweepDirection == SweepDirection.Clockwise)
                {
                    endAngle += RoundAngle(Math.Abs(value) * 360 / Total);
                }
                else
                {
                    endAngle -= RoundAngle(Math.Abs(value) * 360 / Total);
                }

                Slice slice = Slices[i];
                slice.StartAngle = startAngle;
                slice.EndAngle = endAngle;
                slice.InnerExtentStart = slice.InnerExtentEnd = ChartInnerExtent;
                slice.IsOthersSlice = isOtherSlice;
                slice.ExplodedRadius = ActualExplodedRadius;
                slice.Index = i;
                slice.DataContext = isOtherSlice ? Others : FastItemsSource[ValueIndices[i]];

                slice.IsExploded = ExplodedSlices.Contains(i);
                slice.IsSelected = SelectedSlices.Contains(i);

                startAngle = endAngle;
            }

            Slices.Count = totalSliceCount;
        }

        /// <summary>
        /// Prepares data needed to create piechart labels.
        /// </summary>
        protected internal virtual void PrepareLabels()
        {
            if (ItemsSource == null || FastItemsSource == null)
            {
                Labels.Count = 0;
                return;
            }

            if (LabelColumn == null || LabelColumn.Count == 0 || LabelsPosition == LabelsPosition.None)
            {
                Labels.Count = 0;
                return;
            }

            int totalLabelCount = ValueIndices.Count;
            //TextBlock sampleText = new TextBlock { Text = OthersCategoryText};

            if (OthersValueIndices.Count > 0)
            {
                totalLabelCount++;
            }

            View.LabelPreMeasure();

            for (int i = 0; i < totalLabelCount; i++)
            {
                string labelString;
                bool isOthersLabel = false;

                object labelFromLabelColumn;
                if (i == totalLabelCount - 1 && OthersValueIndices.Count > 0)
                {
                    //this is the Others label
                    labelFromLabelColumn = null;
                    isOthersLabel = true;
                    labelString = OthersCategoryText;
                }
                else
                {
                    labelFromLabelColumn = LabelColumn[ValueIndices[i]];



                    labelString = Convert.ToString(labelFromLabelColumn);

                }

                PieLabel label = Labels[i];

                label.ClearValue(WidthProperty);
                label.ClearValue(HeightProperty);


                Slice slice = Slices[i];
                slice.Label = label;

                label.Angle = GeometryUtil.SimplifyAngle((slice.StartAngle + slice.EndAngle) / 2);
                label.Slice = slice;

                label.Label = new TextBlock { Text = labelString };

                if (this.LabelTemplate == null)
                {
                    label.DataContext = isOthersLabel ? Others : FastItemsSource[ValueIndices[i]];
                    label.CreateContent();
                }
                else
                {

                    object itemLabel = this.GetLabel(slice);
                    label.Content = new PieSliceDataContext()
                    {
                        Series = this,
                        Slice = slice,
                        Item = isOthersLabel ? Others : FastItemsSource[ValueIndices[i]],
                        ItemLabel = itemLabel != null ? itemLabel.ToString() : null,
                        ItemBrush = slice.Background,
                        PercentValue = this.GetPercentValue(slice)
                    };
                    label.ContentTemplate = this.LabelTemplate;

                }
                label.Visibility = System.Windows.Visibility.Visible;
                
                // measure at infinite size to get string size
                label.Bounds = View.GetLabelBounds(label);
            }

            Labels.Count = totalLabelCount;
        }

        #endregion Preparers

        #region Renderers

        /// <summary>
        /// Gets or sets the chart's viewport.
        /// </summary>
        protected internal Rect Viewport { get; set; }

        /// <summary>
        /// Renders pie slices.
        /// </summary>
        protected internal virtual void RenderSlices()
        {

            if (RenderSize.Height == 0 || RenderSize.Width == 0)
            {
                return;
            }


            if (ItemsSource == null || FastItemsSource == null)
            {
                return;
            }

            Size pieCanvasSize = View.UpdatePieViewport();
            Viewport = new Rect(0, 0, pieCanvasSize.Width, pieCanvasSize.Height);

            Point center = new Point(pieCanvasSize.Width / 2, pieCanvasSize.Height / 2);
            double radius = Math.Min(pieCanvasSize.Height / 2, pieCanvasSize.Width / 2) * ActualRadiusFactor;

            foreach (Slice slice in Slices.Active)
            {
                Point explodedCenter = GeometryUtil.FindCenter(
                    pieCanvasSize.Width, pieCanvasSize.Height, 
                    true, (slice.StartAngle + slice.EndAngle) / 2, 
                    radius * ActualExplodedRadius);

                slice.InnerExtentStart = slice.InnerExtentEnd = ChartInnerExtent;
                slice.Radius = radius;
                slice.ExplodedRadius = ActualExplodedRadius;
                slice.Origin = center;
                slice.ExplodedOrigin = explodedCenter;

                SetSliceAppearance(slice);

                slice.CreateShape();
            }

            View.UpdateView();
        }

        /// <summary>
        /// Renders pie labels.
        /// </summary>
        protected internal virtual void RenderLabels()
        {

            double renderWidth = RenderSize.Width;
            double renderHeight = RenderSize.Height;




            if (renderHeight == 0 || renderWidth == 0)
            {
                return;
            }

            if (Labels.Active.Count == 0 || LabelsPosition == LabelsPosition.None)
            {
                Labels.Count = 0;
                return;
            }

            List<PieLabel> rightLabels = new List<PieLabel>();
            List<PieLabel> leftLabels = new List<PieLabel>();
            List<PieLabel> centerLabels = new List<PieLabel>();
            List<PieLabel> insideEndLabels = new List<PieLabel>();

            foreach (var label in Labels.Active)
            {
                Slice slice = label.Slice;
                if (slice == null) continue;

                Point center = slice.GetSliceOrigin();

                double width = label.Bounds.Width;
                double height = label.Bounds.Height;

                //these labels are placed in the center of the slices.
                if (LabelsPosition == LabelsPosition.Center || LabelsPosition == LabelsPosition.BestFit)
                {
                    Point labelCenter = GeometryUtil.FindRadialPoint(center, label.Angle, slice.Radius / 2);
                    label.Bounds = new Rect(labelCenter.X - width / 2, labelCenter.Y - height / 2, width, height);
                    bool fitsInCenter = FitsInsideBounds(label, labelCenter);

                    if (fitsInCenter || LabelsPosition == LabelsPosition.Center)
                    {
                        centerLabels.Add(label);
                        label.LeaderLine.Visibility = Visibility.Collapsed;

                        if (!fitsInCenter && LabelsPosition == LabelsPosition.Center)
                        {
                            label.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            label.Visibility = Visibility.Visible;
                        }
                        continue;
                    }
                }

                //these labels are inside the the slices.
                if (LabelsPosition == LabelsPosition.InsideEnd || LabelsPosition == LabelsPosition.BestFit)
                {
                    double labelOffset = 5.0; //offset inside labels by 5px from the edge.
                    double labelAngleRadians = label.Angle * Math.PI / 180;
                    labelOffset += Math.Abs(label.Bounds.Width / 2 * Math.Cos(labelAngleRadians)) + Math.Abs(label.Bounds.Height / 2 * Math.Sin(labelAngleRadians));
                    Point labelCenter = GeometryUtil.FindRadialPoint(center, label.Angle, slice.Radius - labelOffset);
                    label.Bounds = new Rect(labelCenter.X - width / 2, labelCenter.Y - height / 2, width, height);
                    bool fitsOnInside = FitsInsideBounds(label, labelCenter);

                    if (fitsOnInside || LabelsPosition == LabelsPosition.InsideEnd)
                    {
                        insideEndLabels.Add(label);
                        label.LeaderLine.Visibility = Visibility.Collapsed;

                        if (!fitsOnInside && LabelsPosition == LabelsPosition.InsideEnd)
                        {
                            label.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            label.Visibility = Visibility.Visible;
                        }

                        continue;
                    }
                }

                //these labels are outside the slices.
                Point leaderLineStartPoint = GeometryUtil.FindRadialPoint(center, label.Angle, slice.Radius);
                Point labelPoint = GeometryUtil.FindRadialPoint(center, label.Angle, slice.Radius + LabelExtent);

                label.LeaderLine.X1 = leaderLineStartPoint.X;
                label.LeaderLine.Y1 = leaderLineStartPoint.Y;
                label.LeaderLine.Visibility = LeaderLineVisibility;
                label.Visibility = Visibility.Visible;

                label.UpdateCascadingLeaderLineStroke();

                if (label.Angle < 90 && label.Angle >= 0)
                {
                    label.Bounds = new Rect(labelPoint.X, labelPoint.Y, width, height);
                    rightLabels.Add(label);
                }
                else if (label.Angle < 180 && label.Angle >= 90)
                {
                    label.Bounds = new Rect(labelPoint.X - width, labelPoint.Y, width, height);
                    leftLabels.Add(label);
                }
                else if (label.Angle < 270 && label.Angle >= 180)
                {
                    label.Bounds = new Rect(labelPoint.X - width, labelPoint.Y - height, width, height);
                    leftLabels.Add(label);
                }
                else
                {
                    label.Bounds = new Rect(labelPoint.X, labelPoint.Y - height, width, height);
                    rightLabels.Add(label);
                }

                //restrict labels to the pie chart's bounds.
                //if (label.Bounds.Right > renderWidth)
                //{
                //    label.Bounds = new Rect(renderWidth - label.Bounds.Width, label.Bounds.Y, label.Bounds.Width, label.Bounds.Height);
                //}
                //if (label.Bounds.X < 0)
                //{
                //    label.Bounds = new Rect(0, label.Bounds.Y, label.Bounds.Width, label.Bounds.Height);
                //}
                if (label.Bounds.Y < 0)
                {
                    label.Bounds = new Rect(label.Bounds.X, 0, label.Bounds.Width, label.Bounds.Height);
                }
                if (label.Bounds.Bottom > renderHeight)
                {
                    label.Bounds = new Rect(label.Bounds.X, renderHeight - label.Bounds.Height, label.Bounds.Width, label.Bounds.Height);
                }
            }

            SortLabels(rightLabels);
            ResolveCollisions(rightLabels);

            SortLabels(leftLabels);
            ResolveCollisions(leftLabels);

            foreach (var label in centerLabels)
            {
                View.UpdateLabelPosition(label, label.Bounds.X, label.Bounds.Y);
            }

            foreach (var label in insideEndLabels)
            {
                View.UpdateLabelPosition(label, label.Bounds.X, label.Bounds.Y);
            }

            foreach (var label in rightLabels)
            {
                View.UpdateLabelPosition(label, label.Bounds.X, label.Bounds.Y);
                label.MoveLeaderLineEndpoint(label.Bounds.Left, (label.Bounds.Top + label.Bounds.Bottom) / 2);
            }
            foreach (var label in leftLabels)
            {
                View.UpdateLabelPosition(label, label.Bounds.X, label.Bounds.Y);
                label.MoveLeaderLineEndpoint(label.Bounds.Right, (label.Bounds.Top + label.Bounds.Bottom) / 2);
            }

            View.UpdateView();
        }

        /// <summary>
        /// Renders legend items.
        /// </summary>
        protected internal virtual void RenderLegendItems()
        {
//#if !TINYCLR
            ItemLegend itemLegend = Legend as ItemLegend;
            if (itemLegend == null) return;

            if (LabelColumn == null || LabelColumn.Count == 0)
            {
                itemLegend.ClearLegendItems(this);
                return;
            }

            LegendItems = new List<UIElement>();

            foreach (var slice in Slices.Active)
            {
                ContentControl item = new ContentControl();

                item.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = itemLegend });


                object itemLabel = this.GetLabel(slice);
                Brush itemBrush = slice.Background;
                item.Content = new PieSliceDataContext
                { 
                    Series = this, 
                    Slice = slice, 
                    Item = slice.DataContext, 
                    ItemBrush = itemBrush, 
                    ItemLabel = itemLabel != null ? itemLabel.ToString() : null,
                    PercentValue = this.GetPercentValue(slice)
                };
                item.ContentTemplate = LegendItemTemplate;

                LegendItems.Add(item);
            }

            itemLegend.CreateLegendItems(LegendItems, this);
//#endif
        }

        #endregion Renderers

        #endregion Methods
        /// <summary>
        /// Registers a numeric column in the FastItemsSource.
        /// </summary>
        /// <param name="memberPath">The name on the property on each data item to get a value from.</param>
        /// <returns>A FastItemColumn containing the numeric values retrieved the given memberPath of each item in the FastItemsSource.</returns>
        protected IFastItemColumn<double> RegisterDoubleColumn(string memberPath)
        {




            return FastItemsSource.RegisterColumn(memberPath);

        }
        /// <summary>
        /// Registers a column in the FastItemsSource.
        /// </summary>
        /// <param name="memberPath">The name on the property on each data item to get a value from.</param>
        /// <returns>A FastItemColumn containing the values retrieved the given memberPath of each item in the FastItemsSource.</returns>
        protected IFastItemColumn<object> RegisterObjectColumn(string memberPath)
        {




            return FastItemsSource.RegisterColumnObject(memberPath);

        }

        #region Property updates

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
                case ItemsSourcePropertyName:
                    FastItemsSource = new FastItemsSource { ItemsSource = (



                                                                    IEnumerable 

                        )newValue};
                    break;

                case FastItemsSourcePropertyName:
                    if (oldValue as FastItemsSource != null)
                    {
                        (oldValue as FastItemsSource).Event -= FastItemsSource_Event;
                        _fastItemsSourceAttached = false;
                        (oldValue as FastItemsSource).DeregisterColumn(ValueColumn);
                        (oldValue as FastItemsSource).DeregisterColumn(LabelColumn);
                        ValueColumn = null;
                        LabelColumn = null;
                    }

                    if (newValue as FastItemsSource != null)
                    {
                        (newValue as FastItemsSource).Event += FastItemsSource_Event;
                        _fastItemsSourceAttached = true;
                        ValueColumn = RegisterDoubleColumn(ValueMemberPath);
                        LabelColumn = RegisterObjectColumn(LabelMemberPath);
                    }

                    RenderChart();
                    break;

                case ValueMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(ValueColumn);
                        ValueColumn = RegisterDoubleColumn(ValueMemberPath);
                    }
                    break;

                case LabelMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(LabelColumn);
                        LabelColumn = RegisterObjectColumn(LabelMemberPath);
                        PrepareLabels();
                        RenderLabels();
                        RenderLegendItems();
                    }
                    break;

                case StartAnglePropertyName:
                    ActualStartAngle = (double)newValue;
                    PrepareSlices();
                    PrepareLabels();
                    RenderSlices();
                    RenderLabels();
                    break;

                case ToolTipPropertyName:
                    View.UpdateToolTipContent(ToolTip);
                    break;

                case LegendPropertyName:
                    ItemLegend oldLegend = oldValue as ItemLegend;
                    if (oldLegend != null)
                    {
                        oldLegend.ClearLegendItems(this);
                    }
                    RenderLegendItems();
                    break;

                case LegendItemBadgeTemplatePropertyName:
                case LegendItemTemplatePropertyName:
                    RenderLegendItems();
                    break;

                case RadiusFactorPropertyName:
                case ExplodedRadiusPropertyName:
                case SweepDirectionPropertyName:
                    PrepareSlices();
                    PrepareLabels();
                    RenderSlices();
                    RenderLabels();
                    break;

                case OthersCategoryStylePropertyName:
                case SelectedStylePropertyName:
                    RenderSlices();
                    RenderLegendItems();
                    break;

                case BrushesPropertyName:
                    if (oldValue != null)
                    {
                        var ov = (BrushCollection)oldValue;
                        ov.CollectionChanged -= _brushesChangedOverride;
                        _brushesAttached = false;
                    }
                    if (newValue != null)
                    {
                        var bc = (BrushCollection)newValue;
                        bc.CollectionChanged += _brushesChangedOverride;
                        _brushesAttached = true;
                    }

                    RenderSlices();
                    RenderLegendItems();
                    break;
                case OutlinesPropertyName:
                    if (oldValue != null)
                    {
                        var ov = (BrushCollection)oldValue;
                        ov.CollectionChanged -= _brushesChangedOverride;
                        _outlinesAttached = false;
                    }
                    if (newValue != null)
                    {
                        var bc = (BrushCollection)newValue;
                        bc.CollectionChanged += _brushesChangedOverride;
                        _outlinesAttached = true;
                    }

                    RenderSlices();
                    RenderLegendItems();
                    break;

                case ValueColumnPropertyName:
                case OthersCategoryThresholdPropertyName:
                case OthersCategoryTypePropertyName:
                case AllowSliceExplosionPropertyName:
                case AllowSliceSelectionPropertyName:
                    RenderChart();
                    break;

                case LabelsPositionPropertyName:
                case LabelExtentPropertyName:
                    PrepareLabels();
                    RenderLabels();
                    break;

                case OthersCategoryTextPropertyName:
                    PrepareLabels();
                    RenderLabels();
                    RenderLegendItems();
                    break;

                case LeaderLineVisibilityPropertyName:
                    RenderLabels();
                    break;
                case LeaderLineStylePropertyName:
                    // [DN Mar-30-2011:70644] must route this through the dispatcher to give the leaderlines' bindings to the LeaderLineStyle property a chance to take effect.

                    Dispatcher.BeginInvoke((Action)(() =>
                        {
                            foreach (PieLabel label in Labels.Active)
                            {
                                label.UpdateCascadingLeaderLineStroke();
                            }
                        }));

                    break;





                case LabelTemplatePropertyName:
                    PrepareLabels();
                    RenderLabels();
                    break;
            }
        }

        #endregion Property updates

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

        private void RaisePropertyChanged(string name, object oldValue, object newValue)
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

        internal void SliceClicked(Slice slice, MouseEventArgs args)
        {



            SliceClickEventArgs sliceClickEventArgs = new SliceClickEventArgs(slice);

            OnSliceClick(this, sliceClickEventArgs);

            View.UpdateToolTip(slice, args);
        }

        internal void ItemEntered(object item, object args)
        {

            ToolTipManager.UpdateToolTip((MouseEventArgs)args, (FrameworkElement)item);



        }

        internal void ItemMouseMoved(object item, object args)
        {

            ToolTipManager.UpdateToolTip((MouseEventArgs)args, (FrameworkElement)item);



        }

        internal void ItemMouseLeft(object o, MouseEventArgs e)
        {
            View.CloseToolTip();
        }

        internal void OnSizeUpdated()
        {
            RenderChart();
        }



#region Infragistics Source Cleanup (Region)





























































#endregion // Infragistics Source Cleanup (Region)



        private static readonly DependencyProperty FontSizeProxyProperty = DependencyProperty.Register("FontSizeProxy", typeof(double), typeof(PieChartBase), new PropertyMetadata(11.0, (sender, e) =>
        {
            PieChartBase pieChart = sender as PieChartBase;
            // [DN May 23 2012 : 91722] using the dispatcher here to allow a moment for the fontsize to cascade down the visual tree to the actual labels

            pieChart.Dispatcher.BeginInvoke(new Action(() =>
            {
                pieChart.PrepareLabels();
                pieChart.RenderLabels();
            }));

        }));

        internal double GetPercentValue(Slice slice)
        {
            if (slice == null || this.ValueColumn == null || this.ValueIndices == null)
            {
                return double.NaN;
            }
            if (slice.IsOthersSlice)
            {
                return this.OthersTotal / this.Total * 100.0;
            }
            else
            {
                return this.ValueColumn[this.ValueIndices[slice.Index]] / this.Total * 100.0;
            }
        }



#region Infragistics Source Cleanup (Region)

























































































#endregion // Infragistics Source Cleanup (Region)




#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


    }




    internal class PieLabel : ContentPresenter

    {
        internal PieLabel()
        {

            SizeChanged += (o, e) =>
                {
                    if (e.NewSize.Width == 0 || e.NewSize.Height == 0)
                        return;

                    if (Slice != null && Slice.Owner != null)
                    {
                        Slice.Owner.PrepareLabels();
                        Slice.Owner.RenderLabels();
                    }
                };

        }

        internal Slice Slice { get; set; }

        internal Rect Bounds { get; set; }

        internal double Angle { get; set; }

        internal object Label { get; set; }

        internal Line LeaderLine { get; set; }

        internal void CreateContent()
        {

            TextBlock text = Label as TextBlock;
            if (text != null)
            {
                if (Slice != null)
                {
                    text.SetBinding(TextBlock.ForegroundProperty, new Binding("Foreground") { Source = Slice });
                    text.SetBinding(TextBlock.FontFamilyProperty, new Binding("FontFamily") { Source = Slice });
                    text.SetBinding(TextBlock.FontSizeProperty, new Binding("FontSize") { Source = Slice });
                    text.SetBinding(TextBlock.FontStretchProperty, new Binding("FontStretch") { Source = Slice });
                    text.SetBinding(TextBlock.FontStyleProperty, new Binding("FontStyle") { Source = Slice });
                    text.SetBinding(TextBlock.FontWeightProperty, new Binding("FontWeight") { Source = Slice });
                    text.TextTrimming = TextTrimming.WordEllipsis;
                }
            }
            Content = text;

        }

        internal void UpdateCascadingLeaderLineStroke()
        {

            // [DN Mar-30-2011:70644] if PieChart.LeaderLineStyle is null, then use the slice background for the stroke of each leaderline.  otherwise, use the LeaderLineStyle.
            if (this.LeaderLine == null)
            {
                return;
            }
            if (this.LeaderLine.Style == null)
            {
                this.LeaderLine.SetBinding(Line.StrokeProperty, new Binding("Background") { Source = this.Slice });
            }
            else
            {
                this.LeaderLine.ClearValue(Line.StrokeProperty);
            }

        }

        internal void MoveLeaderLineEndpoint(double x, double y)
        {
            LeaderLine.X2 = x;
            LeaderLine.Y2 = y;
        }


        /// <summary>
        /// Provides the behavior for the Measure pass of Silverlight layout. Classes can override this method to define their own Measure pass behavior.
        /// </summary>
        /// <param name="constraint">The available size that this object can give to child objects. Infinity (<see cref="F:System.Double.PositiveInfinity"/>) can be specified as a value to indicate that the object will size to whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based on its calculations of the allocated sizes for child objects; or based on other considerations, such as a fixed container size.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            if (!double.IsNaN(constraint.Width) && !double.IsInfinity(constraint.Width))
            {
                this.Width = constraint.Width;
            }

            if (!double.IsNaN(constraint.Height) && !double.IsInfinity(constraint.Height))
            {
                this.Height = constraint.Height;
            }

            return base.MeasureOverride(constraint);
        }

    }


    /// <summary>
    /// Represents a collection of distinct integers.
    /// </summary>
    public class IndexCollection : ObservableCollection<int>
    {
        /// <summary>
        /// Inserts an integer into the collection.
        /// </summary>
        /// <param name="index">Index of the new value</param>
        /// <param name="item">Item to be inserted</param>
        protected override void InsertItem(int index, int item)
        {
            if (!Contains(item)) base.InsertItem(index, item);
        }

        /// <summary>
        /// Replaces an item at a given index with a new item.
        /// </summary>
        /// <param name="index">Index of the value to be replaced</param>
        /// <param name="item">Value to be added</param>
        protected override void SetItem(int index, int item)
        {
            if (Contains(item))
            {
                Remove(item);
                InsertItem(index, item);
            }
            else
                base.SetItem(index, item);
        }
    }

    /// <summary>
    /// Converts XAML-based string to a collection of integers.
    /// </summary>
    public class IndexCollectionTypeConverter : TypeConverter
    {

        /// <summary>
        /// Determines if a conversion can be made from a given object.
        /// </summary>
        /// <param name="context">Source data</param>
        /// <param name="sourceType">Source data type</param>
        /// <returns>True if conversion is possible, otherwise false</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Convert an object into an IndexCollection.
        /// </summary>
        /// <param name="context">Source datacontext</param>
        /// <param name="culture">Current culture</param>
        /// <param name="value">Source value</param>
        /// <returns>New index collection</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            IndexCollection values = new IndexCollection();
            char[] separators = { ' ', ',' };

            foreach (var val in value.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                values.Add(Convert.ToInt32(val.Trim()));
            }

            return values;
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