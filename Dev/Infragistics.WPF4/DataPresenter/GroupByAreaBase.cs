using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using Infragistics.Windows.Themes;
using Infragistics.Windows.DataPresenter.Events;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.DataPresenter
{
    #region GroupByAreaBase Class

    /// <summary>
    /// The abstract base class for <see cref="GroupByArea"/> and <see cref="GroupByAreaMulti"/> used by the <see cref="XamDataPresenter"/>, <see cref="XamDataGrid"/> and <see cref="XamDataCarousel"/> 
    /// for managing and displaying grouped by fields. 
    /// </summary>
    /// <seealso cref="DataPresenterBase.DefaultFieldLayout"/>
    /// <seealso cref="Field"/>
    /// <seealso cref="XamDataPresenter"/>
    /// <seealso cref="XamDataGrid"/>
    /// <seealso cref="XamDataCarousel"/>
    /// <seealso cref="DataPresenterBase.GroupByArea"/>
    /// <seealso cref="DataPresenterBase.GroupByAreaMulti"/>
    /// <seealso cref="DataPresenterBase.GroupByAreaMode"/>
    [TemplatePart(Name = "PART_InsertionPoint", Type = typeof(FrameworkElement))]
    //[Description("A control used by the XamDataPresenter, XamDataGrid and XamDataCarousel that manages and displays a list of fields that are available for grouping and a list of fields that are already grouped.  It also provides support for dragging and dropping fields between the lists.")]
    public abstract class GroupByAreaBase : Control
    {
        #region Member Variables

        private DataPresenterBase _dataPresenter;
        private FrameworkElement _insertionPoint;

        private Rectangle _currentFieldLabelDragElement;
        private TranslateTransform _currentFieldLabelDragElementTransform;
        private bool _arePropertiesHookedUp;
        private bool _isDragInsertionPointHighlighted;
        private Vector _lastDragInsertionPointOffset = new Vector(double.PositiveInfinity, double.PositiveInfinity);

        private bool _isThemeChangedEventWired;
		private bool				_showPromptsInOnLoaded;
		private bool				_hidePromptsInOnLoaded;

		// AS 2/25/11 TFS67071
		// Instead of setting the Command of the expanderbar we'll hook the click 
		// since setting the command will mean the CanExecute will get invoked 
		// whenever the mouse or key is pressed and for us this command always 
		// is available.
		//
		private List<ExpanderBar> _expanderBars;

        #endregion Member Variables

        #region Constructors

        static GroupByAreaBase()
        {
            // Register commands.
            //
            // ToggleExpandedState
            GroupByAreaBase.ToggleExpandedState
                = new RoutedCommand("ToggleExpandedState", typeof(GroupByAreaBase));

            CommandManager.RegisterClassCommandBinding(typeof(GroupByAreaBase),
                new CommandBinding(GroupByAreaBase.ToggleExpandedState,
                                     new ExecutedRoutedEventHandler(GroupByAreaBase.OnToggleExpandedState),
                                     new CanExecuteRoutedEventHandler(GroupByAreaBase.OnQueryToggleExpandedState)));

        }

        /// <summary>
        /// Constructor provided to allow creation in design tools for template and style editing.
        /// </summary>
        public GroupByAreaBase()
            : this(null)
        {
            this.IsExpanded = true;
        }

        internal GroupByAreaBase(DataPresenterBase dataPresenter)
        {
            // JJD 4/14/09 - NA 2009 vol 2 - Cross band grouping
            // call InitializeDataPresenter
            
            this.InitializeDataPresenter(dataPresenter);

            
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


            // we might might to add another event that would get raised at this point that is
            // named something else for clarity (e.g. FieldLayoutTemplatesGenerated)
            // [BR20144] JM 2-26-07 - Listen to FieldLayoutInitialized instead of RecordContentAreaInitialized.
            //if (this._dataPresenter != null)
            //    this._dataPresenter.FieldLayoutInitialized += new EventHandler<FieldLayoutInitializedEventArgs>(OnFieldLayoutInitialized);
        }

        #endregion //Constructors

        #region Commands

        /// <summary>
        /// Toggles the GroupByAreaBase's <see cref="IsExpanded"/> property.
        /// </summary>
        /// <seealso cref="IsExpanded"/>
        public static readonly RoutedCommand ToggleExpandedState;

        #endregion Commands

        #region Base Class Overrides

        #region GetVisualChild

        /// <summary>
        /// Gets the visual child at a specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the specific child visual.</param>
        /// <returns>The visual child at the specified index.</returns>
        protected override Visual GetVisualChild(int index)
        {
            if (index > 0 &&
                this.FieldLabelDragInProgress &&
                this._currentFieldLabelDragElement != null)
                return this._currentFieldLabelDragElement as Visual;

            return base.GetVisualChild(index);
        }

        #endregion //GetVisualChild

        #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
			// AS 2/25/11 TFS67071
			// Unhook from any expander bars in case we get retemplated.
			//
			RoutedEventHandler expanderBarClickHandler = new RoutedEventHandler(OnExpanderBarClick);
			if (_expanderBars == null)
				_expanderBars = new List<ExpanderBar>();
			else
			{
				foreach (ExpanderBar bar in _expanderBars)
					bar.Click -= expanderBarClickHandler;

				_expanderBars.Clear();
			}

            base.OnApplyTemplate();

			// AS 2/25/11 TFS67071
			// Get all the expander bars in the template - there can be one on top and bottom so 
			// we'll collect them all - and hook their click.
			//
			Utilities.DependencyObjectSearchCallback<ExpanderBar> callback = delegate(ExpanderBar bar)
			{
				// JM 03-28-11 TFS70359 . Only do this for expander bars that do not have a command attached.
				if (bar.Command == null)
					_expanderBars.Add(bar);

				return false;
			};
			Utilities.GetTemplateChild<ExpanderBar>(this, callback);

			foreach (ExpanderBar bar in _expanderBars)
				bar.Click += expanderBarClickHandler;
			

            // JJD 3/08/07
            // If the prompts weren't set then initialize them now to the defaults
            this.InitializePrompts();

            this.VerifyInsertionPoint();

            // Call OnFieldLayoutInitialized directly here in case the FieldLayoutInitialized event was fired by
            // the DataPresenter before we hooked it in our constructor.
            this.OnFieldLayoutInitialized();

            if (this._arePropertiesHookedUp == false && this._dataPresenter != null)
            {
                this._arePropertiesHookedUp = true;
                this.SetBinding(GroupByAreaBase.IsExpandedProperty, Utilities.CreateBindingObject(DataPresenterBase.IsGroupByAreaExpandedProperty, BindingMode.TwoWay, this._dataPresenter));
            }
        }
        #endregion //OnApplyTemplate

        #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="GroupByAreaBase"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.GroupByAreaBaseAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new GroupByAreaBaseAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

        #region OnPropertyChanged

        /// <summary>
        /// Called when a DependencyProperty has changed
        /// </summary>
        /// <param name="e">An instance of DependencyPropertyChangedEventArgs that contains information about the property that changed.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DefaultPrompt1Property ||
                e.Property == DefaultPrompt2Property)
            {
                // JJD 3/08/07
                // If we have already been initialized then sync up the prompts if they 
                // were left to the default
                if (this.IsInitialized)
                {
                    DependencyProperty correspondingPromptProperty;

                    if (e.Property == DefaultPrompt1Property)
                        correspondingPromptProperty = Prompt1Property;
                    else
                        correspondingPromptProperty = Prompt2Property;

                    string existingPrompt = this.GetValue(correspondingPromptProperty) as string;

                    if (existingPrompt == null ||
                        existingPrompt == (string)e.OldValue)
                        this.SetValue(correspondingPromptProperty, e.NewValue);
                }
            }
            else
                if (e.Property == Prompt1Property ||
                    e.Property == Prompt2Property)
                {
                    // JJD 3/08/07
                    // If we have already been initialized then sync up the prompt with
                    // the default string if the new value is null
                    if (this.IsInitialized && e.NewValue == null)
                        this.InitializePrompts();
                }
        }

        #endregion //OnPropertyChanged

        #region VisualChildrenCount

        /// <summary>
        /// Returns the total numder of visual children (read-only).
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                if (this.FieldLabelDragInProgress && this._currentFieldLabelDragElement != null)
                    return 2;
                else
                    return base.VisualChildrenCount;
            }
        }

        #endregion //VisualChildrenCount

        #endregion //Base Class Overrides

        #region Properties

        #region Public Properties

        #region FieldLabelDragInProgress

        internal static readonly DependencyPropertyKey FieldLabelDragInProgressPropertyKey =
            DependencyProperty.RegisterReadOnly("FieldLabelDragInProgress",
            typeof(bool), typeof(GroupByAreaBase), new FrameworkPropertyMetadata());

        /// <summary>
        /// Identifies the 'FieldLabelDragInProgress' dependency property
        /// </summary>
        public static readonly DependencyProperty FieldLabelDragInProgressProperty =
            FieldLabelDragInProgressPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns true if a <see cref="GroupByAreaFieldLabel"/> is currently being dragged.
        /// </summary>
        /// <seealso cref="GroupByAreaFieldLabel"/>
        //[Description("Returns true if a GroupByAreaFieldLabel is currently being dragged")]
        //[Category("Behavior")]
        public bool FieldLabelDragInProgress
        {
            get
            {
                return (bool)this.GetValue(GroupByAreaBase.FieldLabelDragInProgressProperty);
            }
        }

        #endregion //FieldLabelDragInProgress

        #region IsExpanded

        /// <summary>
        /// Identifies the 'IsExpanded' dependency property
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
                  typeof(bool), typeof(GroupByAreaBase), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnIsExpandedChanged)));

        private static void OnIsExpandedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            GroupByAreaBase control = target as GroupByAreaBase;

            if (control != null)
            {
                AutomationPeer peer = UIElementAutomationPeer.FromElement(control);

                if (null != peer)
                {
                    bool isExpanded = (bool)e.NewValue;

                    peer.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                        !isExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
                        isExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
                }

                if ((bool)(e.NewValue) == true)
                    control.OnExpanded();
                else
                    control.OnCollapsed();
            }
        }

        /// <summary>
        /// Returns/sets whether the <see cref="GroupByAreaBase"/> is expanded or collapsed.
        /// </summary>
        /// <remarks>
        /// <p class="body">The GroupByAreaBase keeps track of its expanded and collapsed state and exposes this property that styles can trigger of off to collapse the elements in the style's Template.
        /// The default style for the GroupByAreaBase provides contains a Template that exposes a UI for expanding and collapsing the control.</p>
        /// </remarks>
        //[Description("Returns/sets whether the GroupByAreaBase is expanded or collapsed.")]
        //[Category("Behavior")]
        public bool IsExpanded
        {
            get
            {
                return (bool)this.GetValue(GroupByAreaBase.IsExpandedProperty);
            }
            set
            {
                this.SetValue(GroupByAreaBase.IsExpandedProperty, KnownBoxes.FromValue(value));
            }
        }

        #endregion //IsExpanded

        #region Prompt1

        /// <summary>
        /// Identifies the 'Prompt1' dependency property
        /// </summary>
        public static readonly DependencyProperty Prompt1Property =
            DependencyProperty.Register("Prompt1",
            typeof(string), typeof(GroupByAreaBase), new FrameworkPropertyMetadata());

        /// <summary>
        /// Returns/sets the first of 2 instructional prompts displayed in the <see cref="GroupByAreaBase"/>.
        /// </summary>
        /// <remarks>
        /// <p class="body">The default value for this prompt is 'group by area'.</p>
        /// </remarks>
        //[Description("Returns/sets the first of 2 instructional prompts displayed in the GroupByAreaBase")]
        //[Category("Appearance")]
        public string Prompt1
        {
            get
            {
                return (string)this.GetValue(GroupByAreaBase.Prompt1Property);
            }
            set
            {
                this.SetValue(GroupByAreaBase.Prompt1Property, value);
            }
        }

        #endregion //Prompt1

		#region Prompt1Template

		/// <summary>
		/// Identifies the <see cref="Prompt1Template"/> dependency property
		/// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty Prompt1TemplateProperty = DependencyProperty.Register("Prompt1Template",
			typeof(DataTemplate), typeof(GroupByAreaBase), new FrameworkPropertyMetadata((DataTemplate)null));

		/// <summary>
		/// Returns/sets the DataTemplate to use for the first prompt in the <see cref="GroupByArea"/> and <see cref="GroupByAreaMulti"/>.
		/// </summary>
		/// <seealso cref="Prompt1TemplateProperty"/>
		/// <seealso cref="Prompt1"/>
		/// <seealso cref="Prompt2"/>
		/// <seealso cref="Prompt2Template"/>
		/// <seealso cref="ShowPromptsEvent"/>
		/// <seealso cref="HidePromptsEvent"/>
		//[Description("Returns/sets the DataTemplate to use for the first prompt in the GroupByArea and GroupByAreaMulti.")]
		//[Category("Appearance")]
		[Bindable(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public DataTemplate Prompt1Template
		{
			get
			{
				return (DataTemplate)this.GetValue(GroupByAreaBase.Prompt1TemplateProperty);
			}
			set
			{
				this.SetValue(GroupByAreaBase.Prompt1TemplateProperty, value);
			}
		}

		#endregion //Prompt1Template

        #region Prompt2

        /// <summary>
        /// Identifies the 'Prompt2' dependency property
        /// </summary>
        public static readonly DependencyProperty Prompt2Property =
            DependencyProperty.Register("Prompt2",
            typeof(string), typeof(GroupByAreaBase), new FrameworkPropertyMetadata());

        /// <summary>
        /// Returns/sets the second of 2 instructional prompts displayed in the <see cref="GroupByAreaBase"/>
        /// </summary>
        /// <remarks>
        /// <p class="body">The default value for this prompt is 'Drag a field here to group by that field'.</p>
        /// </remarks>
        //[Description("Returns/sets the second of 2 instructional prompts displayed in the GroupByAreaBase")]
        //[Category("Appearance")]
        public string Prompt2
        {
            get
            {
                return (string)this.GetValue(GroupByAreaBase.Prompt2Property);
            }
            set
            {
                this.SetValue(GroupByAreaBase.Prompt2Property, value);
            }
        }

        #endregion //Prompt2

		#region Prompt2Template

		/// <summary>
		/// Identifies the <see cref="Prompt2Template"/> dependency property
		/// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty Prompt2TemplateProperty = DependencyProperty.Register("Prompt2Template",
			typeof(DataTemplate), typeof(GroupByAreaBase), new FrameworkPropertyMetadata((DataTemplate)null));

		/// <summary>
		/// Returns/sets the DataTemplate to use for the second prompt in the <see cref="GroupByArea"/> and <see cref="GroupByAreaMulti"/>.
		/// </summary>
		/// <seealso cref="Prompt2TemplateProperty"/>
		/// <seealso cref="Prompt2"/>
		/// <seealso cref="Prompt1"/>
		/// <seealso cref="Prompt1Template"/>
		/// <seealso cref="ShowPromptsEvent"/>
		/// <seealso cref="HidePromptsEvent"/>
		//[Description("Returns/sets the DataTemplate to use for the second prompt in the GroupByArea and GroupByAreaMulti.")]
		//[Category("Appearance")]
		[Bindable(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public DataTemplate Prompt2Template
		{
			get
			{
				return (DataTemplate)this.GetValue(GroupByAreaBase.Prompt2TemplateProperty);
			}
			set
			{
				this.SetValue(GroupByAreaBase.Prompt2TemplateProperty, value);
			}
		}

		#endregion //Prompt2Template

        #endregion Public Properties

        #region Internal Properties

        #region CurrentFieldLabelDragElement

        internal Rectangle CurrentFieldLabelDragElement
        {
            get
            {
                if (this._currentFieldLabelDragElement == null)
                {
                    this._currentFieldLabelDragElement = new Rectangle();
                    this._currentFieldLabelDragElement.Visibility = Visibility.Hidden;

                    this._currentFieldLabelDragElementTransform = new TranslateTransform(0, 0);
                    this._currentFieldLabelDragElement.RenderTransform = this._currentFieldLabelDragElementTransform;

                    this.AddVisualChild(this._currentFieldLabelDragElement);
                }

                return this._currentFieldLabelDragElement;
            }
            set
            {
                this._currentFieldLabelDragElement = value;
            }
        }

        internal TranslateTransform CurrentFieldLabelDragElementTransform { get { return this._currentFieldLabelDragElementTransform; } }

        #endregion CurrentFieldLabelDragElement

        #region DataPresenterBase

        internal DataPresenterBase DataPresenter
        {
            get { return this._dataPresenter; }
        }

        #endregion //DataPresenterBase

        #region DefaultPrompt1

        private static readonly DependencyProperty DefaultPrompt1Property =
            DependencyProperty.Register("DefaultPrompt1",
            typeof(string), typeof(GroupByAreaBase), new FrameworkPropertyMetadata());

        private string DefaultPrompt1
        {
            get
            {
                return (string)this.GetValue(GroupByAreaBase.DefaultPrompt1Property);
            }
        }

        #endregion //DefaultPrompt1

        #region DefaultPrompt2

        private static readonly DependencyProperty DefaultPrompt2Property =
            DependencyProperty.Register("DefaultPrompt2",
            typeof(string), typeof(GroupByAreaBase), new FrameworkPropertyMetadata());

        private string DefaultPrompt2
        {
            get
            {
                return (string)this.GetValue(GroupByAreaBase.DefaultPrompt2Property);
            }
        }

        #endregion //DefaultPrompt2

        #region InsertionPoint

        internal FrameworkElement InsertionPoint { get { return this._insertionPoint; } }

        #endregion //InsertionPoint	
        
        #region IsDragInsertionPointHighlighted

        internal bool IsDragInsertionPointHighlighted 
        { 
            get { return this._isDragInsertionPointHighlighted; }
            set { this._isDragInsertionPointHighlighted = value; }
        }

        #endregion //IsDragInsertionPointHighlighted	

        #region LastDragInsertionPointOffset

        internal Vector LastDragInsertionPointOffset 
        { 
            get { return this._lastDragInsertionPointOffset; }
            set { this._lastDragInsertionPointOffset = value; }
        }

        #endregion //LastDragInsertionPointOffset	
    
        #region StyleVersionNumber

        internal static readonly DependencyProperty StyleVersionNumberProperty = DependencyProperty.Register("StyleVersionNumber",
            typeof(int), typeof(GroupByAreaBase), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnStyleVersionNumberPropertyChanged)));

        private static void OnStyleVersionNumberPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            GroupByAreaBase groupByArea = target as GroupByAreaBase;
            if (groupByArea != null)
            {
                groupByArea.OnStyleVersionNumberChanged();
                return;
            }

            if (target != null)
                throw new InvalidOperationException(DataPresenterBase.GetString("LE_InvalidOperationException_12"));
        }

        internal virtual void OnStyleVersionNumberChanged()
        {
        }

        internal int StyleVersionNumber
        {
            get { return (int)this.GetValue(GroupByAreaBase.StyleVersionNumberProperty); }
            set { this.SetValue(GroupByAreaBase.StyleVersionNumberProperty, value); }
        }

        #endregion //StyleVersionNumber

        #endregion Internal Properties

        #region Private Properties

        #endregion Private Properties

        #endregion Properties

        #region Methods

        #region Internal Methods

        // JJD 4/14/09 - NA 2009 vol 2 - Cross band grouping
        #region InitializeDataPresenter

        internal virtual void InitializeDataPresenter(DataPresenterBase dataPresenter)
        {
            // unwire events on the old datapresenter
            if (this._dataPresenter != null)
                this._dataPresenter.FieldLayoutInitialized -= new EventHandler<FieldLayoutInitializedEventArgs>(OnFieldLayoutInitialized);

            this._dataPresenter = dataPresenter;

            // we might might to add another event that would get raised at this point that is
            // named something else for clarity (e.g. FieldLayoutTemplatesGenerated)
            // [BR20144] JM 2-26-07 - Listen to FieldLayoutInitialized instead of RecordContentAreaInitialized.
            if (this._dataPresenter != null)
                this._dataPresenter.FieldLayoutInitialized += new EventHandler<FieldLayoutInitializedEventArgs>(OnFieldLayoutInitialized);
        }

        #endregion //InitializeDataPresenter

        #endregion //Internal Methods	
        
        #region Private Methods

        #region InitializePrompts

        private void InitializePrompts()
        {
            // JJD 3/08/07
            // If the prompts weren't set then initialize them now to the defaults
            string prompt = this.Prompt1;
            if (prompt == null)
                this.Prompt1 = this.DefaultPrompt1;

            prompt = this.Prompt2;
            if (prompt == null)
                this.Prompt2 = this.DefaultPrompt2;
        }

        #endregion //InitializePrompts

		// AS 2/25/11 TFS67071
		#region OnExpanderBarClick
		private void OnExpanderBarClick(object sender, RoutedEventArgs e)
		{
			ToggleExpandedState.Execute(null, this);
		}
		#endregion //OnExpanderBarClick

		// JM 05-05-09 Added.
		#region OnLoaded

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.Loaded -= new RoutedEventHandler(OnLoaded);

			if (this._showPromptsInOnLoaded)
			{
				this.OnShowPrompts();
				this._showPromptsInOnLoaded = false;
			}
			else
			if (this._hidePromptsInOnLoaded)
			{
				this.OnHidePrompts();
				this._hidePromptsInOnLoaded = false;
			}
		}

		#endregion //OnLoaded

		#region OnThemeChanged

		internal virtual void OnThemeChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
        }

        #endregion //OnThemeChanged

        #region OnOnFieldLayoutInitialized

        void OnFieldLayoutInitialized(object sender, FieldLayoutInitializedEventArgs e)
        {
            this.OnFieldLayoutInitialized();
        }

        internal virtual void OnFieldLayoutInitialized()
        {
            // Make sure that a default field layout has been established.  This may not have happened yet
            // depending on timing.
            if (this.DataPresenter == null || this.DataPresenter.DefaultFieldLayout == null)
                return;

            // JJD  2/24/07
            // Only hook the event the 1st time thru
            // Listen to the theme changed event
            if (this._isThemeChangedEventWired == false)
            {
                this._isThemeChangedEventWired = true;
                this.DataPresenter.ThemeChanged += new RoutedPropertyChangedEventHandler<string>(OnThemeChanged);
            }
        }

        #endregion //OnFieldLayoutInitialized

        #region OnExpanderBarAreaClick

        private void OnExpanderBarAreaMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.IsExpanded = !this.IsExpanded;
        }

        #endregion //OnExpanderBarAreaClick

        #region VerifyInsertionPointArea

        private void VerifyInsertionPoint()
        {
            DependencyObject area = base.GetTemplateChild("PART_InsertionPoint");

            if (area == null)
                return;

            FrameworkElement fe = area as FrameworkElement;
            if (fe == null)
                throw new NotSupportedException(DataPresenterBase.GetString("LE_NotSupportedException_4", "FrameworkElement", "PART_InsertionPoint", "GroupByArea", area.GetType().Name));

            this._insertionPoint = fe;
        }

        #endregion //VerifyInsertionPointArea	

        #endregion Private Methods

        #region Static Methods

        #region OnToggleExpandedState

        private static void OnToggleExpandedState(object target, ExecutedRoutedEventArgs args)
        {
            GroupByAreaBase groupByArea = target as GroupByAreaBase;
            if (groupByArea != null)
                groupByArea.IsExpanded = !groupByArea.IsExpanded;
            else
                throw new InvalidOperationException(DataPresenterBase.GetString("LE_InvalidOperationException_13"));
        }

        #endregion //OnToggleExpandedState

        #region OnQueryToggleExpandedState

        private static void OnQueryToggleExpandedState(object target, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

        #endregion //OnQueryToggleExpandedState

        #endregion Static Methods

        #endregion Methods

        #region Events

        #region Collapsed

        /// <summary>
        /// Event ID for the 'Collapsed' routed event
        /// </summary>
        public static readonly RoutedEvent CollapsedEvent =
            Expander.CollapsedEvent.AddOwner(typeof(GroupByAreaBase));

        /// <summary>
        /// Occurs after the <see cref="GroupByAreaBase"/> is collapsed.
        /// </summary>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="IsExpanded"/>
        internal protected virtual void OnCollapsed()
        {
            RoutedEventArgs args = new RoutedEventArgs(GroupByAreaBase.CollapsedEvent, this);
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Occurs after the <see cref="GroupByAreaBase"/> is collapsed.
        /// </summary>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="IsExpanded"/>
        //[Description("Occurs after the GroupByAreaBase is collapsed")]
        //[Category("Behavior")]
        public event RoutedEventHandler Collapsed
        {
            add
            {
                base.AddHandler(GroupByAreaBase.CollapsedEvent, value);
            }
            remove
            {
                base.RemoveHandler(GroupByAreaBase.CollapsedEvent, value);
            }
        }

        #endregion //Collapsed

        #region Expanded

        /// <summary>
        /// Event ID for the 'Expanded' routed event
        /// </summary>
        public static readonly RoutedEvent ExpandedEvent =
            Expander.ExpandedEvent.AddOwner(typeof(GroupByAreaBase));

        /// <summary>
        /// Occurs after the <see cref="GroupByAreaBase"/> is expanded
        /// </summary>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="IsExpanded"/>
        internal protected virtual void OnExpanded()
        {
            RoutedEventArgs args = new RoutedEventArgs(GroupByAreaBase.ExpandedEvent, this);
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Occurs after the <see cref="GroupByAreaBase"/> is expanded.
        /// </summary>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="IsExpanded"/>
        //[Description("Occurs after the GroupByAreaBase is expanded")]
        //[Category("Behavior")]
        public event RoutedEventHandler Expanded
        {
            add
            {
                base.AddHandler(GroupByAreaBase.ExpandedEvent, value);
            }
            remove
            {
                base.RemoveHandler(GroupByAreaBase.ExpandedEvent, value);
            }
        }

        #endregion //Expanded

        #region HideInsertionPoint

        /// <summary>
        /// Event ID for the 'HideInsertionPoint' routed event
        /// </summary>
        public static readonly RoutedEvent HideInsertionPointEvent =
            EventManager.RegisterRoutedEvent("HideInsertionPoint", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GroupByAreaBase));

        /// <summary>
        /// Occurs when the <see cref="GroupByAreaBase"/> should hide its insertion point
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>The element that represents the insertion point is defined by the <see cref="GroupByAreaBase"/> as a TemplatePart 
        /// with the name 'PART_InsertionPoint'.  If you replace the Template of the <see cref="GroupByAreaBase"/>, you will probably want to include an
        /// element with that name.  The <see cref="GroupByAreaBase"/> will automatically move the insertion point as needed when the user drags 
        /// <see cref="GroupByAreaFieldLabel"/>s.  It will also fire this event when the insertion point should be hidden.  You should include an 
        /// EventTrigger in your replacement Template that listens for this event and hides the insertion point element.</p>
        /// </remarks>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="GroupByAreaFieldLabel"/>
        internal protected virtual void OnHideInsertionPoint()
        {
            RoutedEventArgs args = new RoutedEventArgs(GroupByAreaBase.HideInsertionPointEvent, this);
            this.RaiseEvent(args);

            this._isDragInsertionPointHighlighted = false;
        }

        /// <summary>
        /// Occurs when the <see cref="GroupByAreaBase"/> should hide its insertion point
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>The element that represents the insertion point is defined by the <see cref="GroupByAreaBase"/> as a TemplatePart 
        /// with the name 'PART_InsertionPoint'.  If you replace the Template of the <see cref="GroupByAreaBase"/>, you will probably want to include an
        /// element with that name.  The <see cref="GroupByAreaBase"/> will automatically move the insertion point as needed when the user 
        /// drags <see cref="GroupByAreaFieldLabel"/>s.  It will also fire this event when the insertion point should be hidden.  You should include an 
        /// EventTrigger in your replacement Template that listenes for this event and hides the insertion point element.</p>
        /// </remarks>
        /// <seealso cref="GroupByAreaBase"/>
        //[Description("Occurs when the GroupByAreaBase should hide its insertion point")]
        //[Category("Behavior")]
        public event RoutedEventHandler HideInsertionPoint
        {
            add
            {
                base.AddHandler(GroupByAreaBase.HideInsertionPointEvent, value);
            }
            remove
            {
                base.RemoveHandler(GroupByAreaBase.HideInsertionPointEvent, value);
            }
        }

        #endregion //HideInsertionPoint

        #region HidePrompts

        /// <summary>
        /// Event ID for the 'HidePromptsEvent' routed event
        /// </summary>
        public static readonly RoutedEvent HidePromptsEvent =
            EventManager.RegisterRoutedEvent("HidePrompts", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GroupByAreaBase));

        /// <summary>
        /// Occurs when the <see cref="GroupByAreaBase"/> should hide its prompts.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>If you replace the Template of the <see cref="GroupByAreaBase"/>, you should include an
        /// EventTrigger in your replacement Template that listens for this event and hides the elements that are displaying <see cref="Prompt1"/> and <see cref="Prompt2"/>.
        /// The <see cref="GroupByAreaBase"/> will fire this event when the number of Fields that are gropued changes from greater than zero to zero.</p>
        /// </remarks>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="Prompt1"/>
        /// <seealso cref="Prompt2"/>
        internal protected virtual void OnHidePrompts()
        {
			if (this.IsLoaded == false)
			{
				this.Loaded					+= new RoutedEventHandler(OnLoaded);
				this._hidePromptsInOnLoaded = true;
				this._showPromptsInOnLoaded = false;
				return;
			}

			DependencyObject o = VisualTreeHelper.GetParent(this);
            if (o != null && VisualTreeHelper.GetChildrenCount(this) > 0)
            {
				RoutedEventArgs args = new RoutedEventArgs(GroupByAreaBase.HidePromptsEvent, this);
				this.RaiseEvent(args);
			}
        }

        /// <summary>
        /// Occurs when the <see cref="GroupByAreaBase"/> should hide its prompts.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>If you replace the Template of the <see cref="GroupByAreaBase"/>, you should include an
        /// EventTrigger in your replacement Template that listens for this event and hides the elements that are displaying <see cref="Prompt1"/> and <see cref="Prompt2"/>.
        /// The <see cref="GroupByAreaBase"/> will fire this event when the number of groupd Fields changes from greater than zero to zero.</p>
        /// </remarks>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="Prompt1"/>
        /// <seealso cref="Prompt2"/>
        //[Description("Occurs when the GroupByAreaBase should hide its prompts")]
        //[Category("Behavior")]
        public event RoutedEventHandler HidePrompts
        {
            add
            {
                base.AddHandler(GroupByAreaBase.HidePromptsEvent, value);
            }
            remove
            {
                base.RemoveHandler(GroupByAreaBase.HidePromptsEvent, value);
            }
        }

        #endregion //HidePrompts

        #region ShowInsertionPoint

        /// <summary>
        /// Event ID for the 'ShowInsertionPoint' routed event
        /// </summary>
        public static readonly RoutedEvent ShowInsertionPointEvent =
            EventManager.RegisterRoutedEvent("ShowInsertionPoint", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GroupByAreaBase));

        /// <summary>
        /// Occurs when the <see cref="GroupByAreaBase"/> should display its insertion point
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>The element that represents the insertion point is defined by the <see cref="GroupByAreaBase"/> as a TemplatePart 
        /// with the name 'PART_InsertionPoint'.  If you replace the Template of the <see cref="GroupByAreaBase"/>, you will probably want to include an
        /// element with that name.  The <see cref="GroupByAreaBase"/> will automatically move the insertion point as needed when the user 
        /// drags <see cref="GroupByAreaFieldLabel"/>s.  It will also fire this event when the insertion point should be shown.  You should include an 
        /// EventTrigger in your replacement Template that listens for this event and shows the insertion point element.</p>
        /// </remarks>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="GroupByAreaFieldLabel"/>
        internal protected virtual void OnShowInsertionPoint()
        {
            RoutedEventArgs args = new RoutedEventArgs(GroupByAreaBase.ShowInsertionPointEvent, this);
            this.RaiseEvent(args);

            this._isDragInsertionPointHighlighted = true;
        }

        /// <summary>
        /// Occurs when the <see cref="GroupByAreaBase"/> should display its insertion point
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>The element that represents the insertion point is defined by the <see cref="GroupByAreaBase"/> as a TemplatePart 
        /// with the name 'PART_InsertionPoint'.  If you replace the Template of the <see cref="GroupByAreaBase"/>, you will probably want to include an
        /// element with that name.  The <see cref="GroupByAreaBase"/> will automatically move the insertion point as needed when the user 
        /// drags <see cref="GroupByAreaFieldLabel"/>s.  It will also fire this event when the insertion point should be shown.  You should include an 
        /// EventTrigger in your replacement Template that listens for this event and shows the insertion point element.</p>
        /// </remarks>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="GroupByAreaFieldLabel"/>
        //[Description("Occurs when the GroupByAreaBase should display its insertion point")]
        //[Category("Behavior")]
        public event RoutedEventHandler ShowInsertionPoint
        {
            add
            {
                base.AddHandler(GroupByAreaBase.ShowInsertionPointEvent, value);
            }
            remove
            {
                base.RemoveHandler(GroupByAreaBase.ShowInsertionPointEvent, value);
            }
        }

        #endregion //ShowInsertionPoint

        #region ShowPrompts

        /// <summary>
        /// Event ID for the 'ShowPrompts' routed event
        /// </summary>
        public static readonly RoutedEvent ShowPromptsEvent =
            EventManager.RegisterRoutedEvent("ShowPrompts", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GroupByAreaBase));

        /// <summary>
        /// Occurs when the <see cref="GroupByAreaBase"/> should display its prompts
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>If you replace the Template of the <see cref="GroupByAreaBase"/>, you should include an
        /// EventTrigger in your replacement Template that listens for this event and shows the elements that are displaying <see cref="Prompt1"/> and <see cref="Prompt2"/>.
        /// The <see cref="GroupByAreaBase"/> will fire this event when the number of grouped Fields changes from zero to greater than zero.</p>
        /// </remarks>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="Prompt1"/>
        /// <seealso cref="Prompt2"/>
        internal protected virtual void OnShowPrompts()
        {
			if (this.IsLoaded == false)
			{
				this.Loaded					+= new RoutedEventHandler(OnLoaded);
				this._showPromptsInOnLoaded = true;
				this._hidePromptsInOnLoaded = false;
				return;
			}


			DependencyObject o = VisualTreeHelper.GetParent(this);
			if (o != null && VisualTreeHelper.GetChildrenCount(this) > 0)
			{
				RoutedEventArgs args = new RoutedEventArgs(GroupByAreaBase.ShowPromptsEvent, this);
				this.RaiseEvent(args);
			}
        }

        /// <summary>
        /// Occurs when the <see cref="GroupByAreaBase"/> should display its prompts
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>If you replace the Template of the <see cref="GroupByAreaBase"/>, you should include an
        /// EventTrigger in your replacement Template that listens for this event and shows the elements that are displaying <see cref="Prompt1"/> and <see cref="Prompt2"/>.
        /// The <see cref="GroupByAreaBase"/> will fire this event when the number of grouped Fields changes from zero to greater than zero.</p>
        /// </remarks>
        /// <seealso cref="GroupByAreaBase"/>
        /// <seealso cref="Prompt1"/>
        /// <seealso cref="Prompt2"/>
        //[Description("Occurs when the GroupByAreaBase should display its prompts")]
        //[Category("Behavior")]
        public event RoutedEventHandler ShowPrompts
        {
            add
            {
                base.AddHandler(GroupByAreaBase.ShowPromptsEvent, value);
            }
            remove
            {
                base.RemoveHandler(GroupByAreaBase.ShowPromptsEvent, value);
            }
        }

        #endregion //ShowPrompts

        #endregion //Events
    }

    #endregion //GroupByAreaBase Class

	#region GroupingHelper Internal Class

	internal class GroupingHelper
	{
		#region Member Variables

		// AS 6/1/09 NA 2009.2 Undo/Redo
		// The GroupByAreaBase is only used to raise the Grouping/Grouped events. This class
		// could raise the events itself without worrying about what group by area was being 
		// used. This allows me to reuse this for the undo/redo.
		//
		//private GroupByAreaBase					_groupByArea;
		private FieldLayout						_fieldLayout;
		private FieldSortDescriptionCollection	_sortedFields;
		private List<FieldSortDescription>		_groups;

		#endregion //Member Variables

		#region Constructor

		internal GroupingHelper(FieldLayout fieldLayout)
		{
			this._fieldLayout	= fieldLayout;
			this._sortedFields	= fieldLayout.SortedFields;


			// Initialize a the list of existing groupby fields used to support raising the grouping events
			this._groups		= new List<FieldSortDescription>(this._sortedFields.CountOfGroupByFields + 1);
			for (int i = 0; i < this._sortedFields.CountOfGroupByFields; i++)
			{
				if (this._sortedFields[i].IsGroupBy)
					this._groups.Add(this._sortedFields[i]);
			}
		}

		#endregion //Constructor

		#region Properties

			#region FieldLayout

		internal FieldLayout FieldLayout
		{
			get { return this._fieldLayout; }
		}

			#endregion //FieldLayout	
 
			#region Groups

		internal List<FieldSortDescription> Groups
		{
			get { return this._groups; }
		}

			#endregion //Groups	
    
			#region SortedFields

		internal FieldSortDescriptionCollection SortedFields
		{
			get { return this._sortedFields; }
		}

			#endregion //SortedFields	
    
		#endregion Properties

		#region Methods

			// AS 6/1/09 NA 2009.2 Undo/Redo
			#region AddUndoAction
		private void AddUndoAction(Field field)
		{
			DataPresenterBase dp = this.FieldLayout.DataPresenter;

			if (null != dp && dp.IsUndoEnabled)
				dp.History.AddUndoActionInternal(new GroupByAction(field));
		} 
			#endregion //AddUndoAction

			#region GetFieldSortDescriptionForField

		internal FieldSortDescription GetFieldSortDescriptionForField(Field field, IList<FieldSortDescription> sortedFields)
		{
			foreach (FieldSortDescription fieldSortDescription in sortedFields)
			{
				// JM 05-30-08 BR33244 - Compare the Field instead of the Field.Name since the Field.Name property might not
				// be set.
				//if (fieldSortDescription.Field.Name == field.Name)
				if (fieldSortDescription.Field == field)
					return fieldSortDescription;
			}

			return null;
		}

			#endregion //GetFieldSortDescriptionForField

			#region ProcessApplyReGroup

		internal void ProcessApplyReGroup(Field field, int insertAtIndex)
		{
			this.ProcessApplyReGroup(field, insertAtIndex, true);
		}

		internal void ProcessApplyReGroup(Field field, int insertAtIndex, bool addToUndo)
		{
			// AS 6/1/09 NA 2009.2 Undo/Redo
			if (addToUndo)
				this.AddUndoAction(field);

			// JJD 1/29/09
			// Since we will be making multiple updates call BeginUpdate
			this._sortedFields.BeginUpdate();

			try
			{
				// Change the field's position in the list of sorted fields to trigger the actual grouping.
				foreach (FieldSortDescription fieldSortDescription in this._sortedFields)
				{
					if (fieldSortDescription.Field == field)
					{
						// Remove from old position.
						this._sortedFields.Remove(fieldSortDescription);

						// Insert at new position.
						this._sortedFields.Insert(insertAtIndex, fieldSortDescription);
						break;
					}
				}
			}
			finally
			{
				// JJD 1/29/09
				// Call EndUpdate since we called BeginUpdate above
				this._sortedFields.EndUpdate();
			}
		}

			#endregion //ProcessApplyReGroup

			#region ProcessApplyGrouping

		internal void ProcessApplyGrouping(Field field, int insertAtIndex)
		{
			this.ProcessApplyGrouping(field, insertAtIndex, true);
		}

		internal void ProcessApplyGrouping(Field field, int insertAtIndex, bool addToUndo)
		{
			// AS 6/1/09 NA 2009.2 Undo/Redo
			if (addToUndo)
				this.AddUndoAction(field);

			// Add the field to the list of sorted fields to trigger the actual grouping.  (If it is
			// already in the sorted fields collection remove it first then re-add it)
			FieldSortDescription oldfieldSortDescription = this.GetFieldSortDescriptionForField(field, this._sortedFields);
			if (oldfieldSortDescription != null)
			{
				// JJD 1/29/09
				// Since we will be making multiple updates call BeginUpdate
				this._sortedFields.BeginUpdate();

				this._sortedFields.Remove(oldfieldSortDescription);
			}

			try
			{
				// Find the FieldSortDescription in our list
				FieldSortDescription fieldSortDescription = this.GetFieldSortDescriptionForField(field, this._groups);


				this._sortedFields.Insert(insertAtIndex, fieldSortDescription);
			}
			finally
			{
				// JJD 1/29/09
				// If we called BeginUpdate above call EndUpdate
				if (oldfieldSortDescription != null)
					this._sortedFields.EndUpdate();
			}
		}

			#endregion //ProcessApplyGrouping

			#region ProcessApplyUnGroup

		internal void ProcessApplyUnGroup(Field field)
		{
			this.ProcessApplyUnGroup(field, true);
		}

		internal void ProcessApplyUnGroup(Field field, bool addToUndo)
		{
			// AS 6/1/09 NA 2009.2 Undo/Redo
			if (addToUndo)
				this.AddUndoAction(field);

			// JM 05-04-09 - Call BeginUpdate/EndUpdate in this routine instead of relying on the caller to do it.
			this._sortedFields.BeginUpdate();

			try
			{
				// Remove the field from the list of sorted fields to trigger the actual grouping.
				foreach (FieldSortDescription fieldSortDescription in this._sortedFields)
				{
					if (fieldSortDescription.Field == field)
					{
						this._sortedFields.Remove(fieldSortDescription);
						break;
					}
				}
			}
			finally
			{
				this._sortedFields.EndUpdate();
			}
		}

			#endregion //ProcessApplyUnGroup

			#region ProcessTryReGroup

		internal bool ProcessTryReGroup(Field field, int insertAtIndex)
		{
			// JJD 3/2/07
			// adjust the list of existing groupby fields used to support raising the grouping events
			foreach (FieldSortDescription fieldSortDescription in this._groups)
			{
				if (fieldSortDescription.Field == field)
				{
					// Remove from old position.
					this._groups.Remove(fieldSortDescription);

					Debug.Assert(insertAtIndex >= 0 && insertAtIndex <= _groups.Count);

					// Insert at new position.
					this._groups.Insert(insertAtIndex, fieldSortDescription);
					break;
				}
			}

			// JJD 3/2/07
			// raise the before grouping event
			// if canceled then return
			if (!this.RaiseGroupingEventHelper())
				return false;

			return true;
		}

			#endregion //ProcessTryReGroup

			#region ProcessTryGrouping

		internal bool ProcessTryGrouping(Field field, int insertAtIndex)
		{
			// Add the field to the list of sorted fields to trigger the actual grouping.  (If it is
			// already in the sorted fields collection remove it first then re-add it)
			FieldSortDescription oldfieldSortDescription = this.GetFieldSortDescriptionForField(field, this._groups);
			if (oldfieldSortDescription != null)
				this._groups.Remove(oldfieldSortDescription);

			FieldSortDescription fieldSortDescription	= new FieldSortDescription();
			fieldSortDescription.Field					= field;
			fieldSortDescription.Direction				= ListSortDirection.Ascending;
			// JM 10-16-08 [BR35076  TFS6401] Preserve the sort direction specified on the Field (if any)
			if (this._sortedFields.Contains(fieldSortDescription.Field))
				fieldSortDescription.Direction = this._sortedFields[fieldSortDescription.Field].Direction;
			fieldSortDescription.IsGroupBy = true;

			Debug.Assert(insertAtIndex >= 0 && insertAtIndex <= _groups.Count);

			this._groups.Insert(insertAtIndex, fieldSortDescription);

			// JJD 3/2/07
			// raise the before grouping event
			// if canceled then return
			if (!this.RaiseGroupingEventHelper())
				return false;

			return true;
		}

			#endregion //ProcessTryGrouping

			#region ProcessTryUnGroup

		internal bool ProcessTryUnGroup(Field field)
		{
			// JJD 3/2/07
			// Get the field from the list
			FieldSortDescription oldfieldSortDescription = this.GetFieldSortDescriptionForField(field, this._groups);

			Debug.Assert(oldfieldSortDescription != null);

			if (oldfieldSortDescription == null)
				return false;

			// JJD 3/2/07
			// Remove the field from the list
			this._groups.Remove(oldfieldSortDescription);

			// JJD 3/2/07
			// raise the before grouping event
			// if canceled then return
			if (!this.RaiseGroupingEventHelper())
				return false;

			return true;
		}

			#endregion //ProcessTryUnGroup

			// AS 6/1/09 NA 2009.2 Undo/Redo
			// Moved from the GroupByAreaBase and changed to reference the DataPresenter
			// from the FieldLayout being manipulated. Also we don't need to pass in the 
			// field layout or groups since this class has that info.
			//
			#region RaiseGroupingEventHelper

        internal bool RaiseGroupingEventHelper()
        {
            FieldSortDescription[] newGroupsArray = new FieldSortDescription[_groups.Count];
            _groups.CopyTo(newGroupsArray);

            GroupingEventArgs args = new GroupingEventArgs(_fieldLayout, newGroupsArray);

            // JJD 3/2/07
            // raise the before grouping event
            this.FieldLayout.DataPresenter.RaiseGrouping(args);

            return !args.Cancel;
        }

			#endregion //RaiseGroupingEventHelper

			// AS 6/1/09 NA 2009.2 Undo/Redo
			// Moved from the GroupByAreaBase and changed to reference the DataPresenter
			// from the FieldLayout being manipulated. Also we don't need to pass in the 
			// field layout or groups since this class has that info.
			//
			// JJD 5/27/09 - TFS17941 - added
			#region RaiseGroupedEventHelper

        internal void RaiseGroupedEventHelper()
        {
            FieldSortDescription[] newGroupsArray = new FieldSortDescription[_groups.Count];
            _groups.CopyTo(newGroupsArray);

            GroupedEventArgs args = new GroupedEventArgs(_fieldLayout, newGroupsArray);

            this.FieldLayout.DataPresenter.RaiseGrouped(args);
        }

	        #endregion //RaiseGroupedEventHelper

		#endregion //Methods
	}

	#endregion //GroupingHelper Internal Class
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