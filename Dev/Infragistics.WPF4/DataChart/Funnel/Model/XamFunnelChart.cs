using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Messaging;
using System.Runtime.CompilerServices;





namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a funnel chart.
    /// </summary>
    [TemplatePart(Name = "PART_VIEW", Type = typeof(XamFunnelView))]

	
    




    [StyleTypedProperty(Property = "SelectedSliceStyle", StyleTargetType = typeof(XamFunnelSlice))]
    [StyleTypedProperty(Property = "UnselectedSliceStyle", StyleTargetType = typeof(XamFunnelSlice))]
    [Widget("FunnelChart")]
    public class XamFunnelChart : Control, IItemProvider
    {
        /// <summary>
        /// Creates a funnel chart instance.
        /// </summary>
        public XamFunnelChart()
        {

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                // [DN Nov 4 2011 : 93211] it is necessary to reference code from the common assembly so that the reference will be added at design time.  I feel so dirty right now.
                Infragistics.CommandBase cmd = new CommandBase();
            }

            _selectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            MessageHandler = new MessageHandler();
            MessageHandler.AddHandler(typeof(SliceClickedMessage), SliceClickedMessageReceived);
            MessageHandler.AddHandler(typeof(SelectedItemsChangedMessage), SelectedItemsChangedMessageReceived);

            ConfigurationMessages = new MessageChannel();
            FastItemsSource_Event = (o, e) => { DataUpdated(e.Action, e.Position, e.Count, e.PropertyName); };
            SendDefaults();

            this.DefaultStyleKey = typeof(XamFunnelChart);


			Infragistics.Windows.Utilities.ValidateLicense(typeof(XamDataChart), this);


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        }

        private MessageHandler _messageHandler;
        /// <summary>
        /// Handles communication message distribution.
        /// </summary>
        internal MessageHandler MessageHandler
        {
            get { return _messageHandler; }
            set { _messageHandler = value; }
        }

        /// <summary>
        /// Send the default values of some properties that have non default() values.
        /// </summary>
        private void SendDefaults()
        {
            OnPropertyChanged(
                BottomEdgeWidthPropertyName, 
                0.0, 
                BottomEdgeWidth);
            OnPropertyChanged(
                InnerLabelVisibilityPropertyName, 
                InnerLabelVisibility, 
                InnerLabelVisibility);
            OnPropertyChanged(
                OuterLabelAlignmentPropertyName,
                OuterLabelAlignment,
                OuterLabelAlignment);
            OnPropertyChanged(
                FunnelSliceDisplayPropertyName,
                FunnelSliceDisplay,
                FunnelSliceDisplay);
            OnPropertyChanged(
                UpperBezierControlPointPropertyName,
                UpperBezierControlPoint,
                UpperBezierControlPoint);
            OnPropertyChanged(
                LowerBezierControlPointPropertyName,
                LowerBezierControlPoint,
                LowerBezierControlPoint);

            SendItemProvider();
        }

        private void OnViewChanged()
        {
            OnPropertyChanged(
                InnerLabelTemplatePropertyName,
                InnerLabelTemplate,
                InnerLabelTemplate);
            OnPropertyChanged(
               OuterLabelTemplatePropertyName,
               OuterLabelTemplate,
               OuterLabelTemplate);
            OnPropertyChanged(
               ToolTipPropertyName,
               ToolTip,
               ToolTip);
        }

        /// <summary>
        /// Send our item provider implementation to the controller.
        /// </summary>
        private void SendItemProvider()
        {
            PropertyChangedMessage pm = new PropertyChangedMessage();
            pm.PropertyName = "ItemProvider";
            pm.OldValue = null;
            pm.NewValue = this;
            ConfigurationMessages.SendMessage(pm);
        }

        private ServiceProvider _serviceProvider;
        /// <summary>
        /// Provides the necessary services for the funnel chart.
        /// </summary>
        internal ServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            set
            {
                ServiceProvider oldValue = _serviceProvider;
                _serviceProvider = value;
                OnServiceProviderChanged(oldValue, _serviceProvider);
            }
        }

        /// <summary>
        /// Communication channel to the controller.
        /// </summary>
        internal MessageChannel ConfigurationMessages { get; set; }

        /// <summary>
        /// Connects this up do the controller and the view.
        /// </summary>
        internal XamFunnelConnector Connector { get; set; }

        /// <summary>
        /// Called when the service provider changes.
        /// </summary>
        /// <param name="oldValue">The old service provider.</param>
        /// <param name="newValue">The new service provider.</param>
        private void OnServiceProviderChanged(ServiceProvider oldValue, ServiceProvider newValue)
        {
            if (oldValue != null)
            {
                MessageChannel channel =
                    oldValue.GetService("ModelUpdateMessages") as MessageChannel;
                if (channel != null)
                {
                    channel.DetachTarget(MessageReceived);
                }

                ConfigurationMessages.DetachFromNext();
            }
            if (newValue != null)
            {
                MessageChannel channel =
                   newValue.GetService("ModelUpdateMessages") as MessageChannel;
                if (channel != null)
                {
                    channel.AttachTarget(MessageReceived);
                }

                MessageChannel rendering =
                    newValue.GetService("ConfigurationMessages") as MessageChannel;
                ConfigurationMessages.ConnectTo(rendering);
            }
        }

        /// <summary>
        /// Called when a message is received from the controller.
        /// </summary>
        /// <param name="m"></param>
        private void MessageReceived(Message m)
        {
            MessageHandler.MessageReceived(m);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Connector != null)
            {
                Connector.DisconnectView();
            }

            XamFunnelView view = GetTemplateChild("PART_VIEW") as XamFunnelView;
            if (view != null && Connector != null)
            {
                Connector.ReconnectView(view);
                OnViewChanged();
            }
            else if (view != null)
            {
                Connector = new XamFunnelConnector(view, this);
            }
        }

        #region Properties

        #region ItemsSource
        /// <summary>
        /// Gets or sets the ItemsSource for the funnel chart.
        /// <para>
        /// This is a dependency property.
        /// </para>
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
        internal const string ItemsSourcePropertyName = "ItemsSource";

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
            ItemsSourcePropertyName,
            typeof(IEnumerable),
            typeof(XamFunnelChart),
            new PropertyMetadata(null,
            (o, e) => (o as XamFunnelChart).OnPropertyChanged(
                ItemsSourcePropertyName, e.OldValue, e.NewValue)));

        #endregion

        #region FastItemsSource and FastItemsSource Event Handlers
        /// <summary>
        /// Gets or sets the FastItemsSource for the funnel chart.
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

        internal const string FastItemsSourcePropertyName = "FastItemsSource";

        /// <summary>
        /// Identifies the FastItemsSource dependency property.
        /// </summary>
        internal static readonly DependencyProperty FastItemsSourceProperty =
            DependencyProperty.Register(
            FastItemsSourcePropertyName,
            typeof(FastItemsSource),
            typeof(XamFunnelChart),
            new PropertyMetadata(
            (o, e) =>
                (o as XamFunnelChart).
                    OnPropertyChanged(
                    FastItemsSourcePropertyName,
                    e.OldValue,
                    e.NewValue)));

        private EventHandler<FastItemsSourceEventArgs> FastItemsSource_Event;

        /// <summary>
        /// When overridden in a derived class, DataChangedOverride is called whenever a change is made to
        /// the series data.
        /// </summary>
        /// <param name="action">The action performed on the data</param>
        /// <param name="position">The index of the first item involved in the update.</param>
        /// <param name="count">The number of items in the update.</param>
        /// <param name="propertyName">The name of the updated property.</param>
        protected void DataUpdated(
            FastItemsSourceEventAction action,
            int position,
            int count,
            string propertyName)
        {
            DataUpdatedMessage m = new DataUpdatedMessage()
            {
                Position = position,
                Count = count,
                PropertyName = propertyName
            };
            switch (action)
            {
                case FastItemsSourceEventAction.Change:
                    m.Action = Util.ItemsSourceAction.Change;
                    break;
                case FastItemsSourceEventAction.Insert:
                    m.Action = Util.ItemsSourceAction.Insert;
                    break;
                case FastItemsSourceEventAction.Remove:
                    m.Action = Util.ItemsSourceAction.Remove;
                    break;
                case FastItemsSourceEventAction.Replace:
                    m.Action = Util.ItemsSourceAction.Replace;
                    break;
                case FastItemsSourceEventAction.Reset:
                    m.Action = Util.ItemsSourceAction.Reset;
                    break;
            }

            OnPropertyChanged(ValueColumnPropertyName, ValueColumn, ValueColumn);
            OnPropertyChanged(InnerLabelColumnPropertyName, InnerLabelColumn, InnerLabelColumn);
            OnPropertyChanged(OuterLabelColumnPropertyName, OuterLabelColumn, OuterLabelColumn);
            ConfigurationMessages.SendMessage(m);
        }

        #endregion

        #region ValueMemberPath Dependency Property
        /// <summary>
        /// Gets or sets the value member path for the funnel chart.
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
        /// Identifies the ValueMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register(
            ValueMemberPathPropertyName,
            typeof(string),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    ValueMemberPathPropertyName,
                    e.OldValue,
                    e.NewValue)));

        /// <summary>
        /// Gets the FastItemColumn representing the mapped values in the items source.
        /// </summary>
        protected internal IFastItemColumn<double> ValueColumn
        {
            get { return valueColumn; }
            private set
            {
                if (valueColumn != value)
                {
                    IFastItemColumn<double> oldValueColumn = valueColumn;
                    valueColumn = value;
                    OnPropertyChanged(
                        ValueColumnPropertyName,
                        oldValueColumn,
                        valueColumn);
                }
            }
        }
        private IFastItemColumn<double> valueColumn;
        internal const string ValueColumnPropertyName = "ValueColumn";
        #endregion

        #region Brushes
        internal const string BrushesPropertyName = "Brushes";

        /// <summary>
        /// Identifies the Brushes dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushesProperty =
            DependencyProperty.Register(
            BrushesPropertyName,
            typeof(BrushCollection),
            typeof(XamFunnelChart),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as XamFunnelChart).OnPropertyChanged(
                    BrushesPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Brushes property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The brushes property defines the palette from which automatically assigned brushes are selected.
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
        #endregion

        #region Outlines
        internal const string OutlinesPropertyName = "Outlines";

        /// <summary>
        /// Identifies the Outlines dependency property.
        /// </summary>
        public static readonly DependencyProperty OutlinesProperty =
            DependencyProperty.Register(
            OutlinesPropertyName,
            typeof(BrushCollection),
            typeof(XamFunnelChart),
            new PropertyMetadata(null, (sender, e) =>
            {
                (sender as XamFunnelChart).OnPropertyChanged(
                    OutlinesPropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the Outlines property.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The Outlines property defines the palette from which automatically assigned Outlines are selected.
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
        #endregion

        #region BottomEdgeWidth Dependency Property
        /// <summary>
        /// Gets or sets the percentage (from near 0 to 1) of space the bottom edge of the funnel should take.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double BottomEdgeWidth
        {
            get
            {
                return (double)GetValue(BottomEdgeWidthProperty);
            }
            set
            {
                SetValue(BottomEdgeWidthProperty, value);
            }
        }

        internal const string BottomEdgeWidthPropertyName = "BottomEdgeWidth";

        /// <summary>
        /// Identifies the BottomEdgeWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty BottomEdgeWidthProperty =
            DependencyProperty.Register(
            BottomEdgeWidthPropertyName,
            typeof(double),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                .35,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    BottomEdgeWidthPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region InnerLabelMemberPath Dependency Property
        /// <summary>
        /// Gets or sets the InnerLabel mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string InnerLabelMemberPath
        {
            get
            {
                return (string)GetValue(InnerLabelMemberPathProperty);
            }
            set
            {
                SetValue(InnerLabelMemberPathProperty, value);
            }
        }

        internal const string InnerLabelMemberPathPropertyName = "InnerLabelMemberPath";

        /// <summary>
        /// Identifies the InnerLabelMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerLabelMemberPathProperty =
            DependencyProperty.Register(
            InnerLabelMemberPathPropertyName,
            typeof(string),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    InnerLabelMemberPathPropertyName,
                    e.OldValue,
                    e.NewValue)));

        /// <summary>
        /// Gets the FastItemColumn representing the mapped InnerLabels in the items source.
        /// </summary>
        protected internal IFastItemColumn<object> InnerLabelColumn
        {
            get { return _innerLabelColumn; }
            private set
            {
                if (_innerLabelColumn != value)
                {
                    IFastItemColumn<object> oldInnerLabelColumn = _innerLabelColumn;
                    _innerLabelColumn = value;
                    OnPropertyChanged(
                        InnerLabelColumnPropertyName,
                        oldInnerLabelColumn,
                        InnerLabelColumn);
                }
            }
        }
        private IFastItemColumn<object> _innerLabelColumn;
        internal const string InnerLabelColumnPropertyName = "InnerLabelColumn";
        #endregion

        #region OuterLabelMemberPath Dependency Property
        /// <summary>
        /// Gets or sets the OuterLabel mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string OuterLabelMemberPath
        {
            get
            {
                return (string)GetValue(OuterLabelMemberPathProperty);
            }
            set
            {
                SetValue(OuterLabelMemberPathProperty, value);
            }
        }

        internal const string OuterLabelMemberPathPropertyName = "OuterLabelMemberPath";

        /// <summary>
        /// Identifies the OuterLabelMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty OuterLabelMemberPathProperty =
            DependencyProperty.Register(
            OuterLabelMemberPathPropertyName,
            typeof(string),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    OuterLabelMemberPathPropertyName,
                    e.OldValue,
                    e.NewValue)));

        /// <summary>
        /// Gets the FastItemColumn representing the mapped OuterLabels in the items source.
        /// </summary>
        protected internal IFastItemColumn<object> OuterLabelColumn
        {
            get { return _outerLabelColumn; }
            private set
            {
                if (_outerLabelColumn != value)
                {
                    IFastItemColumn<object> oldOuterLabelColumn = _outerLabelColumn;
                    _outerLabelColumn = value;
                    OnPropertyChanged(
                        OuterLabelColumnPropertyName,
                        oldOuterLabelColumn,
                        OuterLabelColumn);
                }
            }
        }
        private IFastItemColumn<object> _outerLabelColumn;
        internal const string OuterLabelColumnPropertyName = "OuterLabelColumn";
        #endregion

        #region InnerLabelVisibility Dependency Property
        /// <summary>
        /// Gets or sets whether the inner labels are visible.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Visibility InnerLabelVisibility
        {
            get
            {
                return (Visibility)GetValue(InnerLabelVisibilityProperty);
            }
            set
            {
                SetValue(InnerLabelVisibilityProperty, value);
            }
        }

        internal const string InnerLabelVisibilityPropertyName = "InnerLabelVisibility";

        /// <summary>
        /// Identifies the InnerLabelVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerLabelVisibilityProperty =
            DependencyProperty.Register(
            InnerLabelVisibilityPropertyName,
            typeof(Visibility),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                Visibility.Visible,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    InnerLabelVisibilityPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region OuterLabelVisibility Dependency Property
        /// <summary>
        /// Gets or sets whether the outer labels are visible.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Visibility OuterLabelVisibility
        {
            get
            {
                return (Visibility)GetValue(OuterLabelVisibilityProperty);
            }
            set
            {
                SetValue(OuterLabelVisibilityProperty, value);
            }
        }

        internal const string OuterLabelVisibilityPropertyName = "OuterLabelVisibility";

        /// <summary>
        /// Identifies the OuterLabelVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty OuterLabelVisibilityProperty =
            DependencyProperty.Register(
            OuterLabelVisibilityPropertyName,
            typeof(Visibility),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                Visibility.Collapsed,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    OuterLabelVisibilityPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region OuterLabelAlignment Dependency Property
        /// <summary>
        /// Gets or sets which side of the chart the outer labels should appear.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public OuterLabelAlignment OuterLabelAlignment
        {
            get
            {
                return (OuterLabelAlignment)GetValue(OuterLabelAlignmentProperty);
            }
            set
            {
                SetValue(OuterLabelAlignmentProperty, value);
            }
        }

        internal const string OuterLabelAlignmentPropertyName = "OuterLabelAlignment";

        /// <summary>
        /// Identifies the OuterLabelAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty OuterLabelAlignmentProperty =
            DependencyProperty.Register(
            OuterLabelAlignmentPropertyName,
            typeof(OuterLabelAlignment),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                OuterLabelAlignment.Left,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    OuterLabelAlignmentPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region FunnelSliceDisplay Dependency Property
        /// <summary>
        /// Gets or sets the how the heights of the funnel slices should be configured.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public FunnelSliceDisplay FunnelSliceDisplay
        {
            get
            {
                return (FunnelSliceDisplay)GetValue(FunnelSliceDisplayProperty);
            }
            set
            {
                SetValue(FunnelSliceDisplayProperty, value);
            }
        }

        internal const string FunnelSliceDisplayPropertyName = "FunnelSliceDisplay";

        /// <summary>
        /// Identifies the FunnelSliceDisplay dependency property.
        /// </summary>
        public static readonly DependencyProperty FunnelSliceDisplayProperty =
            DependencyProperty.Register(
            FunnelSliceDisplayPropertyName,
            typeof(FunnelSliceDisplay),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                FunnelSliceDisplay.Uniform,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    FunnelSliceDisplayPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region InnerLabelTemplate Dependency Property
        /// <summary>
        /// Gets or sets the DataTemplate to use when displaying the inner labels.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public DataTemplate InnerLabelTemplate
        {
            get
            {
                return (DataTemplate)GetValue(InnerLabelTemplateProperty);
            }
            set
            {
                SetValue(InnerLabelTemplateProperty, value);
            }
        }

        internal const string InnerLabelTemplatePropertyName = "InnerLabelTemplate";

        /// <summary>
        /// Identifies the InnerLabelTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerLabelTemplateProperty =
            DependencyProperty.Register(
            InnerLabelTemplatePropertyName,
            typeof(DataTemplate),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    InnerLabelTemplatePropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region OuterLabelTemplate Dependency Property
        /// <summary>
        /// Gets or sets the DataTemplate to use when displaying the outer labels.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public DataTemplate OuterLabelTemplate
        {
            get
            {
                return (DataTemplate)GetValue(OuterLabelTemplateProperty);
            }
            set
            {
                SetValue(OuterLabelTemplateProperty, value);
            }
        }

        internal const string OuterLabelTemplatePropertyName = "OuterLabelTemplate";

        /// <summary>
        /// Identifies the OuterLabelTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty OuterLabelTemplateProperty =
            DependencyProperty.Register(
            OuterLabelTemplatePropertyName,
            typeof(DataTemplate),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    OuterLabelTemplatePropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region TransitionDuration Dependency Property
        /// <summary>
        /// Gets or sets how long the animations should take to run.
        /// <para>
        /// This is a dependency property.
        /// </para>
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

        internal const string TransitionDurationPropertyName = "TransitionDuration";

        /// <summary>
        /// Identifies the TransitionDuration dependency property.
        /// </summary>
        public static readonly DependencyProperty TransitionDurationProperty =
            DependencyProperty.Register(
            TransitionDurationPropertyName,
            typeof(TimeSpan),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                TimeSpan.FromSeconds(0),
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    TransitionDurationPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region IsInverted Dependency Property
        /// <summary>
        /// Gets or sets if the funnel should be rendered inverted.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
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

        internal const string IsInvertedPropertyName = "IsInverted";

        /// <summary>
        /// Identifies the IsInverted dependency property.
        /// </summary>
        public static readonly DependencyProperty IsInvertedProperty =
            DependencyProperty.Register(
            IsInvertedPropertyName,
            typeof(bool),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                false,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    IsInvertedPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region UpperBezierControlPoint Dependency Property
        /// <summary>
        /// Gets or sets the upper control point if a bezier curve is used to define the funnel.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Point UpperBezierControlPoint
        {
            get
            {
                return (Point)GetValue(UpperBezierControlPointProperty);
            }
            set
            {
                SetValue(UpperBezierControlPointProperty, value);
            }
        }

        internal const string UpperBezierControlPointPropertyName = "UpperBezierControlPoint";

        /// <summary>
        /// Identifies the UpperBezierControlPoint dependency property.
        /// </summary>
        public static readonly DependencyProperty UpperBezierControlPointProperty =
            DependencyProperty.Register(
            UpperBezierControlPointPropertyName,
            typeof(Point),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                new Point(.5, 0.0),
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    UpperBezierControlPointPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region LowerBezierControlPoint Dependency Property
        /// <summary>
        /// Gets or sets the lower control point if a bezier curve is used to define the funnel.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Point LowerBezierControlPoint
        {
            get
            {
                return (Point)GetValue(LowerBezierControlPointProperty);
            }
            set
            {
                SetValue(LowerBezierControlPointProperty, value);
            }
        }

        internal const string LowerBezierControlPointPropertyName = "LowerBezierControlPoint";

        /// <summary>
        /// Identifies the LowerBezierControlPoint dependency property.
        /// </summary>
        public static readonly DependencyProperty LowerBezierControlPointProperty =
            DependencyProperty.Register(
            LowerBezierControlPointPropertyName,
            typeof(Point),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                new Point(.5,1),
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    LowerBezierControlPointPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region UseBezierCurve Dependency Property
        /// <summary>
        /// Gets or sets whether to use a bezier curve to define the funnel.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public bool UseBezierCurve
        {
            get
            {
                return (bool)GetValue(UseBezierCurveProperty);
            }
            set
            {
                SetValue(UseBezierCurveProperty, value);
            }
        }

        internal const string UseBezierCurvePropertyName = "UseBezierCurve";

        /// <summary>
        /// Identifies the UseBezierCurve dependency property.
        /// </summary>
        public static readonly DependencyProperty UseBezierCurveProperty =
            DependencyProperty.Register(
            UseBezierCurvePropertyName,
            typeof(bool),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                false,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    UseBezierCurvePropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region AllowSliceSelection Dependency Property
        /// <summary>
        /// Gets or sets whether to allow slices to be selected.
        /// <para>
        /// This is a dependency property.
        /// </para>
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

        internal const string AllowSliceSelectionPropertyName = "AllowSliceSelection";

        /// <summary>
        /// Identifies the AllowSliceSelection dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowSliceSelectionProperty =
            DependencyProperty.Register(
            AllowSliceSelectionPropertyName,
            typeof(bool),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                false,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    AllowSliceSelectionPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region UseUnselectedStyle Dependency Property
        /// <summary>
        /// Gets or sets whether to use the unselected style on unselected slices.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public bool UseUnselectedStyle
        {
            get
            {
                return (bool)GetValue(UseUnselectedStyleProperty);
            }
            set
            {
                SetValue(UseUnselectedStyleProperty, value);
            }
        }

        internal const string UseUnselectedStylePropertyName = "UseUnselectedStyle";

        /// <summary>
        /// Identifies the UseUnselectedStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty UseUnselectedStyleProperty =
            DependencyProperty.Register(
            UseUnselectedStylePropertyName,
            typeof(bool),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                false,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    UseUnselectedStylePropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region SelectedSliceStyle Dependency Property
        /// <summary>
        /// Gets or sets the style to use for selected slices.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Style SelectedSliceStyle
        {
            get
            {
                return (Style)GetValue(SelectedSliceStyleProperty);
            }
            set
            {
                SetValue(SelectedSliceStyleProperty, value);
            }
        }

        internal const string SelectedSliceStylePropertyName = "SelectedSliceStyle";

        /// <summary>
        /// Identifies the SelectedSliceStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedSliceStyleProperty =
            DependencyProperty.Register(
            SelectedSliceStylePropertyName,
            typeof(Style),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    SelectedSliceStylePropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region UnselectedSliceStyle Dependency Property
        /// <summary>
        /// Gets or sets the style to use for unselected slices.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Style UnselectedSliceStyle
        {
            get
            {
                return (Style)GetValue(UnselectedSliceStyleProperty);
            }
            set
            {
                SetValue(UnselectedSliceStyleProperty, value);
            }
        }

        internal const string UnselectedSliceStylePropertyName = "UnselectedSliceStyle";

        /// <summary>
        /// Identifies the UnselectedSliceStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty UnselectedSliceStyleProperty =
            DependencyProperty.Register(
            UnselectedSliceStylePropertyName,
            typeof(Style),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    UnselectedSliceStylePropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region ToolTip Dependency Property
        /// <summary>
        /// Gets or sets the ToolTip to display over the slices.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public object ToolTip
        {
            get
            {
                return GetValue(ToolTipProperty);
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
        public static readonly DependencyProperty ToolTipProperty =
            DependencyProperty.Register(
            ToolTipPropertyName,
            typeof(object),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    ToolTipPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region SelectedItems Property
        private ObservableCollection<object> _selectedItems = new ObservableCollection<object>();
        private Dictionary<object, object> _selectedItemsDict = new Dictionary<object, object>();
        /// <summary>
        /// Represents the current selected items.
        /// </summary>
        public IList SelectedItems
        {
            get { return _selectedItems; }
        }
        #endregion

        #region Legend Dependency Property
        /// <summary>
        /// Gets or sets the Legend to display for the chart.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public ItemLegend Legend
        {
            get
            {
                return (ItemLegend)GetValue(LegendProperty);
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
        public static readonly DependencyProperty LegendProperty =
            DependencyProperty.Register(
            LegendPropertyName,
            typeof(ItemLegend),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    LegendPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region LegendItemTemplate Dependency Property
        /// <summary>
        /// Gets or sets the LegendItemTemplate to use for the legend items.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
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
        public static readonly DependencyProperty LegendItemTemplateProperty =
            DependencyProperty.Register(
            LegendItemTemplatePropertyName,
            typeof(DataTemplate),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    LegendItemTemplatePropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region LegendItemBadgeTemplate Dependency Property
        /// <summary>
        /// Gets or sets the LegendItemBadgeTemplate to use for the legend items.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
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
        public static readonly DependencyProperty LegendItemBadgeTemplateProperty =
            DependencyProperty.Register(
            LegendItemBadgeTemplatePropertyName,
            typeof(DataTemplate),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                null,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    LegendItemBadgeTemplatePropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion

        #region UseOuterLabelsForLegend Dependency Property
        /// <summary>
        /// Gets or sets whether to use the outer labels to identify the legend items.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public bool UseOuterLabelsForLegend
        {
            get
            {
                return (bool)GetValue(UseOuterLabelsForLegendProperty);
            }
            set
            {
                SetValue(UseOuterLabelsForLegendProperty, value);
            }
        }

        internal const string UseOuterLabelsForLegendPropertyName = "UseOuterLabelsForLegend";

        /// <summary>
        /// Identifies the UseOuterLabelsForLegend dependency property.
        /// </summary>
        public static readonly DependencyProperty UseOuterLabelsForLegendProperty =
            DependencyProperty.Register(
            UseOuterLabelsForLegendPropertyName,
            typeof(bool),
            typeof(XamFunnelChart),
            new PropertyMetadata(
                false,
                (o, e) => (o as XamFunnelChart)
                    .OnPropertyChanged(
                    UseOuterLabelsForLegendPropertyName,
                    e.OldValue,
                    e.NewValue)));
        #endregion



#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)


        #endregion

        /// <summary>
        /// Called when a property value changes.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        private void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (HandleItemsSourceChanges(propertyName, oldValue, newValue) ||
                HandleBrushChanges(propertyName, oldValue, newValue) ||
                HandleVisibilityChanges(propertyName, oldValue, newValue) ||
                HandleTimespans(propertyName, oldValue, newValue))
            {
                return;
            }

            ConfigurationMessages.SendMessage(
                new PropertyChangedMessage()
                {
                    PropertyName = propertyName,
                    OldValue = oldValue,
                    NewValue = newValue
                });
        }

        /// <summary>
        /// Coerce brush changes into the appropriate format for the controller.
        /// </summary>
        /// <param name="propertyName">The property that changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <returns>If the property change was handled.</returns>
        private bool HandleBrushChanges(string propertyName, object oldValue, object newValue)
        {
            if (propertyName == BrushesPropertyName ||
                propertyName == OutlinesPropertyName)
            {
                    ConfigurationMessages.SendMessage(
                        new PropertyChangedMessage()
                        {
                            PropertyName = propertyName,
                            OldValue = oldValue == null ? null : (oldValue as BrushCollection).ToArray(),
                            NewValue = newValue == null ? null : (newValue as BrushCollection).ToArray()
                        });

                    return true;
            }

            return false;
        }

        /// <summary>
        /// Handle changes to the various items source properties.
        /// </summary>
        /// <param name="propertyName">The property name that changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <returns>True if the property change was handled.</returns>
        private bool HandleItemsSourceChanges(string propertyName, object oldValue, object newValue)
        {
            if (propertyName == ItemsSourcePropertyName)
            {
                if (oldValue != null)
                {
                    FastItemsSource = null;
                }
                if (newValue != null)
                {
                    FastItemsSource = new FastItemsSource() { ItemsSource = ItemsSource };
                }

                return true;
            }
            else if (propertyName == FastItemsSourcePropertyName)
            {
                if (oldValue != null)
                {
                    (oldValue as FastItemsSource).Event -= FastItemsSource_Event;
                }
                if (newValue != null)
                {
                    (newValue as FastItemsSource).Event += FastItemsSource_Event;
                }
                HandleValueMemberPathChange(propertyName, oldValue, newValue);

                return true;
            }
            else if (propertyName == ValueMemberPathPropertyName ||
                propertyName == InnerLabelMemberPathPropertyName ||
                propertyName == OuterLabelMemberPathPropertyName)
            {
                HandleValueMemberPathChange(propertyName, oldValue, newValue);
                return false;
            }

            return false;
        }

        /// <summary>
        /// Handles changes to the member paths.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        private void HandleValueMemberPathChange(string propertyName, object oldValue, object newValue)
        {
            if (FastItemsSource == null)
            {
                var oldSource = oldValue as FastItemsSource;
                if (oldSource != null)
                {
                    if (ValueColumn != null)
                    {
                        oldSource.DeregisterColumn(ValueColumn);
                    }
                    if (InnerLabelColumn != null)
                    {
                        oldSource.DeregisterColumn(InnerLabelColumn);
                    }
                    if (OuterLabelColumn != null)
                    {
                        oldSource.DeregisterColumn(OuterLabelColumn);
                    }
                    ValueColumn = null;
                    InnerLabelColumn = null;
                    OuterLabelColumn = null;
                }
                return;
            }
            
            if (oldValue != null && oldValue is string)
            {
                switch (propertyName)
                {
                    case ValueMemberPathPropertyName:
                        FastItemsSource.DeregisterColumn(ValueColumn);
                        ValueColumn = null;
                        break;
                    case InnerLabelMemberPathPropertyName:
                        FastItemsSource.DeregisterColumn(InnerLabelColumn);
                        InnerLabelColumn = null;
                        break;
                    case OuterLabelMemberPathPropertyName:
                        FastItemsSource.DeregisterColumn(OuterLabelColumn);
                        OuterLabelColumn = null;
                        break;
                }
            }
            if (newValue != null && newValue is FastItemsSource)
            {
                if (ValueMemberPath != null)
                {
                    ValueColumn = (newValue as FastItemsSource).RegisterColumn(ValueMemberPath);
                }
                if (InnerLabelMemberPath != null)
                {
                    InnerLabelColumn = (newValue as FastItemsSource).RegisterColumnObject(InnerLabelMemberPath);
                }
                if (OuterLabelMemberPath != null)
                {
                    OuterLabelColumn = (newValue as FastItemsSource).RegisterColumnObject(OuterLabelMemberPath);
                }
            }
            if (newValue != null && newValue is string)
            {
                switch (propertyName)
                {
                    case ValueMemberPathPropertyName:
                        ValueColumn = FastItemsSource.RegisterColumn(ValueMemberPath);
                        break;
                    case InnerLabelMemberPathPropertyName:
                        InnerLabelColumn = FastItemsSource.RegisterColumnObject(InnerLabelMemberPath);
                        break;
                    case OuterLabelMemberPathPropertyName:
                        OuterLabelColumn = FastItemsSource.RegisterColumnObject(OuterLabelMemberPath);
                        break;
                }
            }
        }

        /// <summary>
        /// Coerce visibility changes into the appropariate format for the controller.
        /// </summary>
        /// <param name="propertyName">The property that changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <returns></returns>
        private bool HandleVisibilityChanges(string propertyName, object oldValue, object newValue)
        {
            if (propertyName == InnerLabelVisibilityPropertyName ||
                propertyName == OuterLabelVisibilityPropertyName)
            {
                bool old = false;
                if (oldValue != null && oldValue is Visibility
                    && (Visibility)oldValue == Visibility.Visible)
                {
                    old = true;
                }
                bool newVal = false;
                if (newValue != null && newValue is Visibility
                    && (Visibility)newValue == Visibility.Visible)
                {
                    newVal = true;
                }

                ConfigurationMessages.SendMessage(
                    new PropertyChangedMessage()
                    {
                        PropertyName = propertyName,
                        NewValue = newVal,
                        OldValue = old
                    });
                return true;
            }

            return false;
        }

        /// <summary>
        /// Coerce timespans into the appropriate format for the controller.
        /// </summary>
        /// <param name="propertyName">The property that changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <returns>True if the property change was handled.</returns>
        private bool HandleTimespans(string propertyName, object oldValue, object newValue)
        {
            if (propertyName == TransitionDurationPropertyName)
            {
                PropertyChangedMessage pm = new PropertyChangedMessage();
                pm.PropertyName = propertyName;
                if (oldValue != null)
                {
                    pm.OldValue = ((TimeSpan)oldValue).TotalMilliseconds;
                }
                if (newValue != null)
                {
                    pm.NewValue = ((TimeSpan)newValue).TotalMilliseconds;
                }
                ConfigurationMessages.SendMessage(pm);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when the controller indicates that a slice was clicked.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void SliceClickedMessageReceived(Message m)
        {
            SliceClickedMessage scm = (SliceClickedMessage)m;

            if (SliceClicked != null)
            {
                FunnelSliceClickedEventArgs args = new FunnelSliceClickedEventArgs();
                args.Index = scm.Index;
                if (FastItemsSource.Count > scm.Index &&
                    scm.Index >= 0)
                {
                    args.Item = FastItemsSource[scm.Index];
                }
                SliceClicked(this, args);
            }
        }

        /// <summary>
        /// Raised when a slice is clicked.
        /// </summary>
        public event FunnelSliceClickedEventHandler SliceClicked;

        /// <summary>
        /// Called when the controller indicates that the selected items have been updated.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void SelectedItemsChangedMessageReceived(Message m)
        {
            SelectedItemsChangedMessage sm = (SelectedItemsChangedMessage)m;

            Dictionary<object, object> newItems = new Dictionary<object, object>();
            var items = (from index in sm.SelectedItems
                        where index >= 0 && index < FastItemsSource.Count
                        select FastItemsSource[index]).ToList();
            items.ForEach((item) => {if (!newItems.ContainsKey(item)) { newItems.Add(item, item); }});

            MergeItems(_selectedItems, _selectedItemsDict, items, newItems);
        }

        /// <summary>
        /// Called if the user has updated the selected items.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The parameters of the event.</param>
        private void SelectedItems_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UserSelectedItemsChangedMessage m = new UserSelectedItemsChangedMessage();
            m.SelectedItems = (from item in _selectedItems.Distinct()
                               select FastItemsSource[item]).ToArray();
            ConfigurationMessages.SendMessage(m);
        }

        /// <summary>
        /// Merge the selected items lists.
        /// </summary>
        /// <param name="listA"></param>
        /// <param name="dictA"></param>
        /// <param name="listB"></param>
        /// <param name="dictB"></param>
        private void MergeItems(
            IList<object> listA, 
            Dictionary<object, object> dictA, 
            IList<object> listB, 
            Dictionary<object, object> dictB)
        {
            var toRemove = new List<object>();
            foreach (var item in listA)
            {
                if (!dictB.ContainsKey(item))
                {
                    toRemove.Add(item);
                }
            }
            foreach (var item in toRemove)
            {
                listA.Remove(item);
                dictA.Remove(item);
            }

            foreach (var item in listB)
            {
                if (!dictA.ContainsKey(item))
                {
                    listA.Add(item);
                    dictA.Add(item,item);
                }
            }
        }

        /// <summary>
        /// Provides the item for the specified index.
        /// </summary>
        /// <param name="index">The index for which to provide the item.</param>
        /// <returns>The indicated item.</returns>
        object IItemProvider.GetItem(int index)
        {
            return FastItemsSource[index];
        }

        /// <summary>
        /// Gets the count of the items bound.
        /// </summary>
        int IItemProvider.Count
        {
            get { return FastItemsSource != null ? FastItemsSource.Count : 0; }
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