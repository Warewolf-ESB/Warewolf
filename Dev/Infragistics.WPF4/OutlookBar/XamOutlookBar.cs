using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Infragistics.Windows.Controls;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.OutlookBar.Events;
using Infragistics.Windows.Themes;
using System.Windows.Resources;
using System.Windows.Interop;
using Infragistics.Shared;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.OutlookBar;
using System.Windows.Media.Animation;
using System.Collections;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Internal;
using Infragistics.Collections;		// JM 10-10-08 TFS8903

namespace Infragistics.Windows.OutlookBar
{
    /// <summary>
    /// The XamOutlookBar is a WPF Component that provides the UI and functionality 
    /// of the navigation bar/strip in the applications /Like MS Outlook 2003 or later versions/  
    /// </summary>
    /// <remarks> XamOutlookBar is a WPF version of the existing Infragistics WinExplorerBar element. The WPF  version of the <see cref="XamOutlookBar"/>   supports only the OutlookNavigationPane style. It is not backward-compatible with the WinExplorerBar control.</remarks>
          
                     

    // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateMinimized,            GroupName = VisualStateUtilities.GroupMinimized)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotMinimized,         GroupName = VisualStateUtilities.GroupMinimized)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateLeft,                 GroupName = VisualStateUtilities.GroupSplitterLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateRight,                GroupName = VisualStateUtilities.GroupSplitterLocation)]

    [TemplatePart(Name = "PART_SelectedGroupHeader", Type = typeof(SelectedGroupHeader))]
    [TemplatePart(Name = "PART_SelectedGroupContent", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_Splitter", Type = typeof(GroupAreaSplitter))]
    [TemplatePart(Name = "PART_NavigationArea", Type = typeof(GroupsPresenter))]
    [TemplatePart(Name = "PART_OverflowArea", Type = typeof(GroupOverflowArea))]
    [TemplatePart(Name = "PART_VerticalSplitter", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_Grid", Type = typeof(Grid))]
    [ContentProperty("Groups")] 
    public class XamOutlookBar : Control
    {
        #region Member Variables

        // Parts of XamOutlookBar in order of appearance
        SelectedGroupHeader _selectedGroupHeader;   // XamOutlookBar Header
        SelectedGroupContent _selectedGroupContent; // XamOutlookBar Selected Group Content
        GroupAreaSplitter _splitter;                // XamOutlookBar Splitter
        GroupsPresenter _navigationAreaIC;          // XamOutlookBar Navigation Area Items
        GroupOverflowArea _overflowArea;            // Overflow Items - groups and menu
        Thumb _verticalSplitter = null;             // Width changing

        Grid _grid;                                 // the grid
        private bool _expanding = false;            // This flag is set to true when expanding the xambar
        private double _lastExpandedWidth = 100;    // Width before minimize
        private double _minimizedWidth = 40;        // calculates, see getLargestGroupImage
        private bool _selectionInitialized= false;  // selected group is initialized

        private List<DependencyObject> _logicalChildren;    // LogicalChildren
        private SplitterPreviewAdorner _adorner;    // show preview when SplitterResizeMode.Deferred

        // OutlookBar groups ... 
        private OutlookBarGroupCollection _groups;
        internal OutlookBarGroupCollection _contextMenuGroups;
        internal OutlookBarGroupCollection _navigationAreaGroups;
        internal OutlookBarGroupCollection _overflowAreaGroups;
        private ReadOnlyOutlookBarGroupCollection _navigationAreaGroupsReadOnly;
        private ReadOnlyOutlookBarGroupCollection _contextMenuGroupsReadOnly;
        private ReadOnlyOutlookBarGroupCollection _overflowAreaGroupsReadOnly;

        // Width
        private Size _lastExpandedSelectedGroupSize = new Size(100, 100);
        private bool _mustCoerceWidth;              // this is for design time problem when IsMinimized is true

        // Animations
        private Storyboard _sbMinimize;                     // minmizing
        private Storyboard _sbExpand;                       // expanding
        private bool _animationInProcess = false;           // true when XOB plays animations
        private bool _useAnimations = true;                 // enable/disable animations

        private UltraLicense _license = null; 

        private XamOutlookBar _xamOutlookbar;       // private member - used for unit testing //MM 2008.07.23

        private GroupsItemsSource _icGroupsSource;  // items control - GroupsSource implementation

		// JM 10-23-08
		private bool _groupsCollectionChangedBeforeControlLoaded;

		// JM 10-22-09 TFS23110, TFS20587, TFS24169
		private bool _inferMinimizedStateFromWidth;


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		// JM 03-10-11 TFS67362
		private int _previouslySelectedGroupIndex = -1;

        #endregion //Member Variables

        #region Base Class Overrides

        #region ArrangeOverride
        /// <summary>
        /// Positions child elements and determines a size for a <see cref="XamOutlookBar"/>.
        /// </summary>
        /// <param name="arrangeBounds">The size available to this element for arranging its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size sz = base.ArrangeOverride(arrangeBounds);

            ArrangeGroups(true);

            return sz;
        }

        #endregion //ArrangeOverride	

        #region Automation
        /// <summary>
        /// Returns <see cref="XamOutlookBar"/> Automation Peer Class <see cref="XamOutlookBarAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new XamOutlookBarAutomationPeer(this);
        }
        #endregion

        #region LogicalChildren
        /// <summary> 
        /// Returns enumerator to logical children
        /// </summary>
        /// <returns>The <see cref="System.Collections.IEnumerator"/> used to iterate its children.</returns>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (this._logicalChildren == null || this._logicalChildren.Count == 0)
                    return base.LogicalChildren;

                if (base.LogicalChildren != null)
                    return new MultiSourceEnumerator(base.LogicalChildren,
                        this._logicalChildren.GetEnumerator());

                return _logicalChildren.GetEnumerator();
            }
        }

        #endregion //LogicalChildren	
    
        #region MeasureOverride
        /// <summary>
        /// Invoked to measure the element and its children.
        /// </summary>
        /// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size sz = base.MeasureOverride(constraint);

			// JM 09-09-09 TFS21655 - If the minimized width has changed, invalidate the Width property to force a coercion of the Width.
			double oldMinimizedWidth = this._minimizedWidth;
            this._minimizedWidth = GetMinimizedWidth();
			if (this._minimizedWidth != oldMinimizedWidth)
				this.InvalidateProperty(WidthProperty);

            return sz;
        }
        
        #endregion //MeasureOverride	
    
        #region OnApplyTemplate
        /// <summary>
        /// Invoked when the template for the element has been applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_verticalSplitter != null)
            {
                _verticalSplitter.DragDelta -= VerticalSplitter_DragDelta;
                _verticalSplitter.DragCompleted -= VerticalSplitter_DragCompleted;
                _verticalSplitter.DragStarted -= VerticalSplitter_DragStarted;
            }

            if(_grid!=null)
                _grid.SizeChanged -= Grid_SizeChanged;

			_navigationAreaIC = this.GetTemplateChild("PART_NavigationArea") as GroupsPresenter;

			_overflowArea = this.GetTemplateChild("PART_OverflowArea") as GroupOverflowArea;
			_splitter = this.GetTemplateChild("PART_Splitter") as GroupAreaSplitter;
            
			_selectedGroupContent = this.GetTemplateChild("PART_SelectedGroupContent") as SelectedGroupContent;
			_selectedGroupHeader = this.GetTemplateChild("PART_SelectedGroupHeader") as SelectedGroupHeader;
			_verticalSplitter = this.GetTemplateChild("PART_VerticalSplitter") as Thumb;
            
            if (_verticalSplitter != null)
            {
                _verticalSplitter.DragDelta += new DragDeltaEventHandler(VerticalSplitter_DragDelta);
                _verticalSplitter.DragCompleted += new DragCompletedEventHandler(VerticalSplitter_DragCompleted);
                _verticalSplitter.DragStarted += new DragStartedEventHandler(VerticalSplitter_DragStarted);
            }

            InitializeGroups(true, false);
            if (this.IsLoaded)
            {
                _splitter.OnApplyTemplate();
                _splitter.InvalidateMeasure();
                GetMaxGroupsInTheNavigationArea(NavigationAreaMaxGroupsResolved, true);
            }
			// JM 01-13-11 TFS61501
			//else
            //   this.Loaded += new RoutedEventHandler(XamOutlookBar_Loaded);

            UpdateSelectedGroupContent();
			_grid = this.GetTemplateChild("PART_Grid") as Grid;
            
            if (_grid != null)
                _grid.SizeChanged += new SizeChangedEventHandler(Grid_SizeChanged);


            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);


			// JM 04-25-11 TFS22746.  Discovered this while fixing 22746 - the toggle button in the 
			// selected group header was not being updated to reflect the minimized status of the control
			// when a new Theme is set.
			if (null != this._selectedGroupHeader)
				this._selectedGroupHeader.SetIsMinimized(this.IsMinimized);
        }
        #endregion //OnApplyTemplate	

        #region OnRenderSizeChanged
        /// <summary>       
        /// Raises the <see cref="System.Windows.FrameworkElement.SizeChanged"/> event, using the specified
        /// information as part of the eventual event data.
        /// </summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            SetNavigationAreaSize(sizeInfo.NewSize.Height- sizeInfo.PreviousSize.Height);
            _selectionInitialized = Groups.Count == 0;
            if (sizeInfo.WidthChanged && _selectionInitialized)
                setMinimizedByWidth(sizeInfo.NewSize.Width);
        }

        #endregion //OnRenderSizeChanged	
        
        #endregion //Base Class Overrides

        #region Constructors

        #region Static Constructor
        static XamOutlookBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(XamOutlookBar),
                new FrameworkPropertyMetadata(typeof(XamOutlookBar)));

			// JM 10-22-09 TFS23110 - Undo the change below made for TFS20587 where we set the default value of the Width property to 144
			// JM 09-17-09 TFS20587 - We are setting the default size of the XamOutlookBar in a DefaultInitializer in the
			// Visual Studio design assembly.  However, since DefaultInitializers are not used by the current version of
			// Blend, this default width doesn't get set when a XamOutlookBar is created on the Blend design surface.
			WidthProperty.AddOwner(typeof(XamOutlookBar),
				new FrameworkPropertyMetadata(double.NaN,
					new PropertyChangedCallback(OnWidthChanged),
					new CoerceValueCallback(OnCoerceWidth)));
			//WidthProperty.AddOwner(typeof(XamOutlookBar),
			//    new FrameworkPropertyMetadata((double)144,
			//        new PropertyChangedCallback(OnWidthChanged),
			//        new CoerceValueCallback(OnCoerceWidth)));

            ThemeManager.RegisterGroupings(typeof(XamOutlookBar),
                new string[] { PrimitivesGeneric.Location.Grouping, 
                    OutlookBarGeneric.Location.Grouping});

            #region ShowPopupCommand

            CommandBinding cbPopup = new CommandBinding(OutlookBarCommands.ShowPopupCommand,
				XamOutlookBar.ExecuteCommandGlobalHandler, XamOutlookBar.CanExecuteCommand);

            CommandManager.RegisterClassCommandBinding(typeof(XamOutlookBar), cbPopup);

            #endregion //ShowSelectedGroupPopup command	

            #region ShowOptionsCommand

            CommandBinding cbShoOpt = new CommandBinding(OutlookBarCommands.ShowOptionsCommand,
				XamOutlookBar.ExecuteCommandGlobalHandler, XamOutlookBar.CanExecuteCommand);

            CommandManager.RegisterClassCommandBinding(typeof(XamOutlookBar), cbShoOpt);

            #endregion //ShowOptionsCommand

            #region ShowFewerButtonsCommand

            CommandBinding cbShrink = new CommandBinding(OutlookBarCommands.ShowFewerButtonsCommand,
				XamOutlookBar.ExecuteCommandGlobalHandler, XamOutlookBar.CanExecuteCommand);

            CommandManager.RegisterClassCommandBinding(typeof(XamOutlookBar), cbShrink);

            #endregion //ShowFewerButtonsCommand	
    
            #region ShowMoreButtonsCommand

            CommandBinding cbExpand = new CommandBinding(OutlookBarCommands.ShowMoreButtonsCommand,
				XamOutlookBar.ExecuteCommandGlobalHandler, XamOutlookBar.CanExecuteCommand);

            CommandManager.RegisterClassCommandBinding(typeof(XamOutlookBar), cbExpand);

            #endregion //ShowMoreButtonsCommand	
    
            #region GroupMoveUpCommand

            CommandBinding cbUp = new CommandBinding(OutlookBarCommands.GroupMoveUpCommand,
				XamOutlookBar.ExecuteCommandGlobalHandler, XamOutlookBar.CanExecuteCommand);

			CommandManager.RegisterClassCommandBinding(typeof(XamOutlookBar), cbUp);

            #endregion //GroupMoveUpCommand	
    
            #region GroupMoveDownCommand

            CommandBinding cbDn = new CommandBinding(OutlookBarCommands.GroupMoveDownCommand,
				XamOutlookBar.ExecuteCommandGlobalHandler, XamOutlookBar.CanExecuteCommand);

			CommandManager.RegisterClassCommandBinding(typeof(XamOutlookBar), cbDn);

            #endregion //GroupMoveDownCommand

            #region SelectGroupCommand

            CommandBinding cbSelect = new CommandBinding(OutlookBarCommands.SelectGroupCommand,
				XamOutlookBar.ExecuteCommandGlobalHandler, XamOutlookBar.CanExecuteCommand);

            CommandManager.RegisterClassCommandBinding(typeof(XamOutlookBar), cbSelect);

            #endregion //SelectGroupCommand
        }
        #endregion // Static Constructor

        #region Public Constructor
        /// <summary>
        /// Initializes a new instance of <see cref="XamOutlookBar"/>
        /// </summary>
        public XamOutlookBar()
        {
            // Verify and cache the license
            try
            {
                // We need to pass our type into the method since we do not want to pass in 
                // the derived type.
                this._license = LicenseManager.Validate(typeof(XamOutlookBar), this) as UltraLicense;
            }
            catch (System.IO.FileNotFoundException) { }
        
            _contextMenuGroups = new OutlookBarGroupCollection(null);
            _navigationAreaGroups = new OutlookBarGroupCollection(null);
            _overflowAreaGroups = new OutlookBarGroupCollection(null);

            XamOutlookBar.SetOutlookBar(this, this);
            this.SetGroups(new OutlookBarGroupCollection(this));

            this.SetResourceReference(DefaultSmallImageProperty, XamOutlookBar.DefaultSmallImageKey);

            this._xamOutlookbar = this;

			// JM 01-11-11 TFS61189
			this.Unloaded += new RoutedEventHandler(XamOutlookBar_Unloaded);

			// JM 01-13-11 TFS61501
			this.Loaded += new RoutedEventHandler(XamOutlookBar_Loaded);
		}

        #endregion //Public Constructor

        #endregion //Constructors

        #region Properties

        #region Private Properties
        




        private double MinimizedWidthResolved
        {
            get { return double.IsNaN(MinimizedWidth) ? _minimizedWidth : MinimizedWidth; }
        }
        




        private double MinimizedStateThresholdResolved
        {
            get { return double.IsNaN(MinimizedStateThreshold) ? MinimizedWidthResolved : MinimizedStateThreshold; }
        }
        




        private double minExpandedWidth
        {
            get { return MinimizedWidthResolved + MinimizedStateThresholdResolved; }
        }

        #endregion //Private Properties	
    
        #region Internal Properties

        #region DefaultSmallImage

        




        internal static readonly DependencyProperty DefaultSmallImageProperty = DependencyProperty.Register("DefaultSmallImage",
            typeof(ImageSource), typeof(XamOutlookBar), new FrameworkPropertyMetadata());
        #endregion //DefaultSmallImage

        #region NavigationAreaMaxGroups
        internal int NavigationAreaMaxGroupsResolved
        {
            get { return NavigationAreaMaxGroups < 0 ? int.MaxValue : NavigationAreaMaxGroups; }
        }
        #endregion //NavigationAreaMaxGroups

        #region VisibleGroupsCount
        internal int VisibleGroupsCount
        {
            get { return _navigationAreaGroups.Count + _overflowAreaGroups.Count + _contextMenuGroups.Count; }
        }
        #endregion //VisibleGroupsCount

        #endregion //Internal Properties

        #region Public Properties

        #region AllowMinimized
        /// <summary>
        /// Identifies the <see cref="AllowMinimized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowMinimizedProperty = DependencyProperty.Register(
            "AllowMinimized", typeof(bool), typeof(XamOutlookBar),
            new PropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnAllowMinimizedChanged)));
        /// <summary>
        /// Returns/sets the possibility of the <see cref="XamOutlookBar"/> to be minimized. This is a dependency property. 
        /// </summary>
        /// <remarks>
        /// <p class="body">By default, <b>AllowMinimized</b> is true. Set this property to false 
        /// to prevent minimizing of <see cref="XamOutlookBar"/>.</p>
        /// </remarks>
        /// <seealso cref="IsMinimized"/>
        //[Description("Returns/sets the possibility of the XamOutlookBar to be minimized")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public bool AllowMinimized
        {
            get { return (bool)this.GetValue(XamOutlookBar.AllowMinimizedProperty); }
            set { this.SetValue(XamOutlookBar.AllowMinimizedProperty, value); }
        }

        private static void OnAllowMinimizedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamOutlookBar xamBar = target as XamOutlookBar;
            xamBar.CoerceValue(XamOutlookBar.IsMinimizedProperty);
        }

        #endregion //AllowMinimized

        #region ContextMenuGroups
        /// <summary>
        /// Returns a read only collection of <see cref="OutlookBarGroup"/> items in the overflow context menu of XamOutlookBar
        /// </summary>
        /// <remarks>
        /// <p class="body">The groups in the <b>ContextMenuGroups</b> collection will be displayed inside overflow context menu of XamOutlookBar. 
        /// The <see cref="OutlookBarGroup.LocationProperty"/> of each group in this collection is set to <b>OutlookBarGroupLocation.OverflowContextMenu</b>.</p>
        /// </remarks>
        /// <seealso cref="OutlookBarGroupLocation"/>
        /// <seealso cref="OutlookBarGroup"/>
        /// <seealso cref="Groups"/>
        //[Description("Returns a read only collection of OutlookBarGroup items in the overflow context menu of XamOutlookBar")]
        //[Category("OutlookBar Properties")] // Behavior
        [Browsable(false)]
        public ReadOnlyOutlookBarGroupCollection ContextMenuGroups
        {
            get {
                if (_contextMenuGroupsReadOnly == null)
                    _contextMenuGroupsReadOnly = new ReadOnlyOutlookBarGroupCollection(_contextMenuGroups);
                return _contextMenuGroupsReadOnly;
            }
        }
        #endregion //ContextMenuGroups

        #region DefaultSmallImageKey

        /// <summary>
        /// The key used to identify the default small image for <see cref="OutlookBarGroup"/>.
        /// </summary>
        /// <remarks>
        /// <p class="body">This is the default value of <see cref="OutlookBarGroup.SmallImageProperty"/>.</p>
        /// </remarks>
        /// <seealso cref="OutlookBarGroup.SmallImage"/>
        public static readonly ResourceKey DefaultSmallImageKey =
            new StaticPropertyResourceKey(typeof(XamOutlookBar), "DefaultSmallImageKey");

        #endregion //DefaultSmallImageKey	

        #region ExpandStoryboard

        /// <summary>
        /// Identifies the <see cref="ExpandStoryboard"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ExpandStoryboardProperty = DependencyProperty.Register("ExpandStoryboard",
            typeof(Storyboard), typeof(XamOutlookBar), new FrameworkPropertyMetadata());

        /// <summary>
        /// 
        /// Returns/set the <see cref="System.Windows.Media.Animation.Storyboard"/> that <see cref="XamOutlookBar"/> plays when it goes to the expanded state. This is a dependency property.
        /// </summary>
        /// <seealso cref="ExpandStoryboardProperty"/>
        /// <seealso cref="IsMinimized"/>
        //[Description("Returns/set the Storyboard that XamOutlookBar plays when it goes to the expanded state.")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public Storyboard ExpandStoryboard
        {
            get
            {
                return (Storyboard)this.GetValue(XamOutlookBar.ExpandStoryboardProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.ExpandStoryboardProperty, value);
            }
        }

        #endregion //ExpandStoryboard

        #region Groups 
        /// <summary>
        /// Returns a modifiable collection of <see cref="OutlookBarGroup"/> items.
        /// </summary>
        /// <remarks>
        /// <p class="body"> The developer can use only one of <see cref="Groups"/> or <see cref="GroupsSource"/> properties to add groups to the control.
        /// Each <see cref="OutlookBarGroup"/> in the <b>Groups</b> belongs to one of readonly collectons:
        /// <see cref="NavigationAreaGroups"/>
        /// <see cref="OverflowAreaGroups"/>
        /// <see cref="ContextMenuGroups"/>
        /// </p>
        /// </remarks>
        /// <seealso cref="GroupsSource"/>
        //[Description("Returns a modifiable collection of OutlookBarGroup items")]
        //[Category("OutlookBar Properties")] // Behavior
        public OutlookBarGroupCollection Groups
        {
            get { return _groups; }
        }

        #endregion //Groups DependencyProperty
        
        #region GroupsSource

        /// <summary>
        /// Identifies the <see cref="GroupsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GroupsSourceProperty = DependencyProperty.Register("GroupsSource",
            typeof(IEnumerable), typeof(XamOutlookBar), 
            new FrameworkPropertyMetadata(null, OnGroupsSourceChanged));

        /// <summary>
        /// Gets or sets a collection used to generate the content of the <see cref="XamOutlookBar"/>. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// <p class="body"> The developer can use only one of <see cref="Groups"/> or <see cref="GroupsSource"/> properties to add groups to the control.
        /// This allows the Groups collection to be populated via a binding on the GroupsSource property.
        /// When <b>GroupsSource</b> is in use, setting this property to null will remove
        /// the collection and restore use to <b>Groups</b> (which will be an empty collection).</p>
        /// </remarks>
        /// <seealso cref="GroupsSourceProperty"/>
        /// <seealso cref="Groups"/>
        //[Description("Gets or sets a collection used to generate the content of the XamOutlookBar")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public IEnumerable GroupsSource
        {
            get
            {
                return (IEnumerable)this.GetValue(XamOutlookBar.GroupsSourceProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.GroupsSourceProperty, value);
            }
        }

        private static void OnGroupsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamOutlookBar xamBar = d as XamOutlookBar;
            if (xamBar == null)
                return;
            
            if (xamBar.Groups.Count > 0 &&  e.OldValue==null && e.NewValue!=null )
                throw new InvalidOperationException(GetString("LE_CannotSetGroupSource"));

            xamBar.Groups.Clear();
            xamBar._icGroupsSource.SetValue(ItemsControl.ItemsSourceProperty, e.NewValue);
        }
        #endregion //GroupsSource
        
        #region HorizontalSplitterStyleKey
        /// <summary>
        /// The key used to identify the style used for a <see cref="Thumb"/> used to represent a horizontal splitter.
        /// </summary>
        public static readonly ResourceKey HorizontalSplitterStyleKey =
            new StaticPropertyResourceKey(typeof(XamOutlookBar), "HorizontalSplitterStyleKey");

        #endregion //HorizontalSplitterStyleKey (static property)

        #region IsMinimized
        /// <summary>
        /// Identifies the <see cref="IsMinimizedProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsMinimizedProperty = DependencyProperty.Register(
            "IsMinimized", typeof(bool), typeof(XamOutlookBar), 
            new PropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsMinimizedChanged), new CoerceValueCallback(OnCoerceIsMinimized)));

        /// <summary>
        /// Returns/sets whether the <see cref="XamOutlookBar"/> is in minimized state. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// <p class="body">By default <see cref="IsMinimized"/> is false. The minimized state of XamOutlookBar depends also on <b>AllowMinimized</b> and <b>MinimizedStateThreshold</b> properties.</p>
        /// </remarks>
        /// <seealso cref="IsMinimizedProperty"/>
        /// <seealso cref="AllowMinimized"/>
        /// <seealso cref="MinimizedStateThreshold"/>
        //[Description("Returns/sets whether the XamOutlokBar is in minimized state")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public bool IsMinimized
        {
            get { 
                bool ismin=(bool)this.GetValue(IsMinimizedProperty);
                return ismin;
            }
            set { this.SetValue(IsMinimizedProperty, value); }
        }

        private static object OnCoerceIsMinimized(DependencyObject target, object value)
        {
            XamOutlookBar xamBar = target as XamOutlookBar;

            if (!xamBar.AllowMinimized)
                value = false; ;
            
            xamBar._expanding = xamBar.IsMinimized && !(bool)value;

            OutlookBarCancelableRoutedEventArgs args = new OutlookBarCancelableRoutedEventArgs();
            if ((bool)value)
            {
                if (!xamBar.IsMinimized)
                {
                    if (!xamBar._animationInProcess)
                    {
                        xamBar.RaiseNavigationPaneMinimizing(args);
                        value = !args.Cancel;
                    }

                    if ((bool)value)
                    {
                        if (!xamBar._animationInProcess)
                        {
                            if (xamBar._selectedGroupContent != null)
                            {
                                xamBar._lastExpandedSelectedGroupSize =
                                    new Size(xamBar._selectedGroupContent.ActualWidth,
                                        xamBar._selectedGroupContent.ActualHeight);
                            }//end if- 
                            if (double.IsNaN(xamBar.Width) || xamBar.ActualWidth > xamBar.minExpandedWidth)
                                xamBar._lastExpandedWidth = xamBar.IsLoaded ? xamBar.ActualWidth : xamBar.Width;

                            if (xamBar.CanAnimate(true))
                            {
                                if (xamBar.ActualWidth > xamBar.minExpandedWidth)
                                {
                                    xamBar.AnimateMinimizing();
                                    return false;
                                }
                            }
                        }
                    }//end if- we can minimize the XOB
                }// end if- expanded->minimized
            }//end if- minimizing
            else if (xamBar.IsMinimized)
            {
                xamBar.RaiseNavigationPaneExpanding(args);
                value= args.Cancel;
            }//end else - minnimized->expanded
            if (args.Cancel)
                xamBar._selectedGroupHeader.SetIsMinimized((bool)value);    // toggle button

            return value;
        }

        private static void OnIsMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamOutlookBar xamBar= d as XamOutlookBar;

            if(xamBar==null)
                return;
            
            if (!xamBar.IsInitialized)
            {
                xamBar._mustCoerceWidth = true;
            }
            if (!xamBar._animationInProcess)
            {
                xamBar.InvalidateProperty(WidthProperty);
				// JM 10-23-09 TFS24169 - The Coerce is redundant after the Invalidate
                //xamBar.CoerceValue(WidthProperty);
            }

            if (xamBar.IsMinimized)
            {
                xamBar.RaiseNavigationPaneMinimized(new RoutedEventArgs());
            }
            else
            {
                if (double.IsNaN(xamBar.Width))
                    xamBar.InitializeGroups(true, true);

                if (xamBar._expanding && xamBar.CanAnimate(false) && !(bool)e.NewValue)
                    xamBar.AnimateExpanding();
                else
                    xamBar.RaiseNavigationPaneExpanded(new RoutedEventArgs());
            }
            
            xamBar._expanding = false;

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            xamBar.UpdateVisualStates();
            xamBar.UpdateAllGroupsVisualState();

        }

        #endregion //IsMinimized

        #region IsVerticalSplitterVisible

        /// <summary>
        /// Identifies the <see cref="IsVerticalSplitterVisible"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsVerticalSplitterVisibleProperty = DependencyProperty.Register("IsVerticalSplitterVisible",
            typeof(bool), typeof(XamOutlookBar), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

        /// <summary>
        /// Returns/sets a value indicating whether the vertical splitter is visible in the user interface. This is a dependency property.
        /// </summary>
        /// <seealso cref="IsVerticalSplitterVisibleProperty"/>
        /// <seealso cref="VerticalSplitterLocation"/>
        /// <seealso cref="VerticalSplitterPreviewStyleKey"/>
        //[Description("Returns/sets a value indicating whether the vertical splitter is visible in the user interface")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public bool IsVerticalSplitterVisible
        {
            get
            {
                return (bool)this.GetValue(XamOutlookBar.IsVerticalSplitterVisibleProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.IsVerticalSplitterVisibleProperty, value);
            }
        }

        #endregion //IsVerticalSplitterVisible
        
        #region MinimizedStateThreshold
        /// <summary>
        /// Identifies the <see cref="MinimizedStateThreshold"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimizedStateThresholdProperty = DependencyProperty.Register("MinimizedStateThreshold",
            typeof(double), typeof(XamOutlookBar), new FrameworkPropertyMetadata(double.NaN, OnMinimizedStateThresholdChanged));

        /// <summary>
        /// Returns/sets the number of pixels over the minimized size where the controls snaps to its minimized state. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// <para class="body">When the property value has not been set the <see cref="XamOutlookBar"/> resolves the threshold to a calculated value that is the width in minimized state.</para>
        /// </remarks>
        /// <seealso cref="MinimizedStateThresholdProperty"/>
        /// <seealso cref="MinimizedWidth"/>
        /// <seealso cref="IsMinimized"/>
        //[Description("Returns/sets the number of pixels over the minimized size where the controls snaps to its minimized state.")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public double MinimizedStateThreshold
        {
            get
            {
                return (double)this.GetValue(XamOutlookBar.MinimizedStateThresholdProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.MinimizedStateThresholdProperty, value);
            }
        }

        private static void OnMinimizedStateThresholdChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            XamOutlookBar xamBar = o as XamOutlookBar;

            if (xamBar != null)
            {
                double widthExpanded = xamBar.IsMinimized ? xamBar._lastExpandedWidth : xamBar.ActualWidth;

				// JM 10-23-09 TFS24169 - We don't need to do this if we are already minimized.  Also, set then reset the 
				// _inferMinimizedStateFromWidth flag to ensure that the setMinimizedByWidth method does its thing.
				if (xamBar.IsMinimized == false || 
					(double.IsNaN((double)e.NewValue) || (double)e.NewValue < (double)e.OldValue))
				{
					xamBar._inferMinimizedStateFromWidth = true;
					xamBar.setMinimizedByWidth(widthExpanded);
					xamBar._inferMinimizedStateFromWidth = false;
				}

				// JM 10-23-09 TFS24169 - If we are not minimized, we need to re-evaluate our Width since MinimizedStateThreshold
				// can affect our un-minimized Width.
				if (xamBar.IsMinimized == false)
					xamBar.InvalidateProperty(WidthProperty);
			}
        }
        #endregion //MinimizedStateThreshold

        #region MinimizedWidth
        /// <summary>
        /// Identifies the <see cref="MinimizedWidth"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinimizedWidthProperty = DependencyProperty.Register("MinimizedWidth",
            typeof(double), typeof(XamOutlookBar), 
            new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnMinimizedWidthChanged)))
            ;
        /// <summary>
        /// Returns/set the width of <see cref="XamOutlookBar"/> in minimized state. This is a dependency property.
        /// </summary>
        /// <seealso cref="MinimizedWidthProperty"/>
        /// <seealso cref="IsMinimized"/>
        //[Description("Returns/set the width of XamOutlookBar in minimized state")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public double MinimizedWidth
        {
            get
            {
                return (double)this.GetValue(XamOutlookBar.MinimizedWidthProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.MinimizedWidthProperty, value);
            }
        }

        private static void OnMinimizedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            XamOutlookBar xamBar = o as XamOutlookBar;

            if (xamBar != null)
            {
				// JM 10-23-09 TFS24169 - We only need to do this if we are minimized.  Also, set then reset the _inferMinimizedStateFromWidth flag to ensure that the 
				// setMinimizedByWidth method does its thing.
				if (xamBar.IsMinimized)
				{
					xamBar._inferMinimizedStateFromWidth = true;
					// JM 10-23-09
					//xamBar.setMinimizedByWidth(xamBar.ActualWidth);
					xamBar.setMinimizedByWidth((double)e.NewValue);
					xamBar._inferMinimizedStateFromWidth = false;
				}

				xamBar.InvalidateProperty(WidthProperty);
			}
        }
        #endregion //MinimizedWidth
        
        #region MinimizeStoryboard

        /// <summary>
        /// Identifies the <see cref="MinimizeStoryboard"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinimizeStoryboardProperty = DependencyProperty.Register("MinimizeStoryboard",
            typeof(Storyboard), typeof(XamOutlookBar), new FrameworkPropertyMetadata());

        /// <summary>
        /// Returns/set the <see cref="Storyboard"/> that XamOutlookBar plays when it goes to the minimized state.
        /// </summary>
        /// <seealso cref="MinimizeStoryboardProperty"/>
        /// <seealso cref="IsMinimized"/>
        //[Description("Returns/set the Storyboard that XamOutlookBar plays when it goes to the minimized state")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public Storyboard MinimizeStoryboard
        {
            get
            {
                return (Storyboard)this.GetValue(XamOutlookBar.MinimizeStoryboardProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.MinimizeStoryboardProperty, value);
            }
        }

        #endregion //MinimizeStoryboard

        #region MinimizeToggleButtonStyleKey
        /// <summary>
        /// The key used to identify the style used by the minimize toggle button in the <see cref="XamOutlookBar"/>.
        /// </summary>
        public static readonly ResourceKey MinimizeToggleButtonStyleKey =
            new StaticPropertyResourceKey(typeof(XamOutlookBar), "MinimizeToggleButtonStyleKey");

        #endregion //MinimizeToggleButtonStyleKey

        #region NavigationPaneToggleButtonStyleKey
        /// <summary>
        /// The key used to identify the style used for a <see cref="System.Windows.Controls.Primitives.ToggleButton"/> used to show popup with content of the selected <see cref="OutlookBarGroup"/>.
        /// </summary>
        public static readonly ResourceKey NavigationPaneToggleButtonStyleKey =
           new StaticPropertyResourceKey(typeof(XamOutlookBar), "NavigationPaneToggleButtonStyleKey");

        #endregion //NavigationPaneToggleButtonStyleKey

        #region NavigationAreaGroups
        /// <summary>
        /// Returns a readonly collection of <see cref="OutlookBarGroup"/> items in the navigation area of the XamOutlookBar
        /// </summary>
        /// <remarks>
        /// <p class="body">The groups in the <b>NavigationAreaGroups</b> collection are displayed inside navigation area of <see cref="XamOutlookBar"/>. 
		/// The <see cref="OutlookBarGroup.LocationProperty"/> of each group in this collection is set to <see cref="OutlookBarGroupLocation"/>.NavigationGroupArea.</p>
        /// </remarks>
        /// <seealso cref="OutlookBarGroupLocation"/>
        /// <seealso cref="OutlookBarGroup"/>
        /// <seealso cref="Groups"/>
        //[Description("Returns a readonly collection of OutlookBarGroup items in the navigation area of the XamOutlookBar")]
        //[Category("OutlookBar Properties")] // Behavior
        [Browsable(false)]
        public ReadOnlyOutlookBarGroupCollection NavigationAreaGroups
        {
            get
            {
                if (_navigationAreaGroupsReadOnly == null)
                    _navigationAreaGroupsReadOnly = new ReadOnlyOutlookBarGroupCollection(_navigationAreaGroups);
                return _navigationAreaGroupsReadOnly;
            }
        }
        #endregion //NavigationAreaGroups

        #region NavigationAreaMaxGroups
        /// <summary>
        /// Identifies the <see cref="NavigationAreaMaxGroups"/> dependency property
        /// </summary>
        public static readonly DependencyProperty NavigationAreaMaxGroupsProperty = DependencyProperty.Register("NavigationAreaMaxGroups",
            typeof(int), typeof(XamOutlookBar), 
            new FrameworkPropertyMetadata(-1, new PropertyChangedCallback(OnNavigationAreaMaxGroupsChanged)));

        /// <summary>
        /// Returns/set desired number of groups in the navigation area of the <see cref="XamOutlookBar"/>. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// <p class="body">By default, <b>NavigationAreaMaxGroups</b> is -1 which meaning that XamOutlookBar will put as much as possible groups in the navigation area depending on available space.</p>
        /// </remarks>
        /// <seealso cref="NavigationAreaMaxGroupsProperty"/>
        //[Description("Returns/set desired number of groups in the navigation area of the XamOutlookBar")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public int NavigationAreaMaxGroups
        {
            get
            {
                return (int)this.GetValue(XamOutlookBar.NavigationAreaMaxGroupsProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.NavigationAreaMaxGroupsProperty, value);
            }
        }
        
        private static void OnNavigationAreaMaxGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamOutlookBar xamBar = d as XamOutlookBar;
            if (xamBar == null)
                return;
            if (xamBar._navigationAreaIC != null)
                xamBar.GetMaxGroupsInTheNavigationArea(xamBar.NavigationAreaMaxGroupsResolved, false);
        }

        #endregion //NavigationAreaMaxGroups

        #region NavigationPaneOptionsControlStyle

        /// <summary>
        /// Identifies the <see cref="NavigationPaneOptionsControlStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty NavigationPaneOptionsControlStyleProperty = DependencyProperty.Register("NavigationPaneOptionsControlStyle",
            typeof(Style), typeof(XamOutlookBar), new FrameworkPropertyMetadata());

        /// <summary>
        /// Returns/set the style of <see cref="NavigationPaneOptionsControl"/>. This is a dependency property.
        /// </summary>
        /// <p class="body">The <b>NavigationPaneOptionsControl</b> is used to change the display order and visibility of the groups</p> 
        /// <seealso cref="NavigationPaneOptionsControlStyleProperty"/>
        //[Description("Returns/set the style of NavigationPaneOptionsControl")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public Style NavigationPaneOptionsControlStyle
        {
            get
            {
                return (Style)this.GetValue(XamOutlookBar.NavigationPaneOptionsControlStyleProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.NavigationPaneOptionsControlStyleProperty, value);
            }
        }

        #endregion //NavigationPaneOptionsControlStyle

        #region OutlookBar 
        internal static readonly DependencyPropertyKey OutlookBarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "OutlookBar", typeof(XamOutlookBar), typeof(XamOutlookBar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior, OnOutlookBarChanged));
        
        /// <summary>
        /// Identifies the OutlookBar attached dependency property
        /// </summary>
        public static readonly DependencyProperty OutlookBarProperty =
            OutlookBarPropertyKey.DependencyProperty;

        internal static void SetOutlookBar(DependencyObject element, XamOutlookBar OutlookBar)
        {
            element.SetValue(OutlookBarPropertyKey, OutlookBar);
        }

        /// <summary>
        /// Returns the value of XamOutlookBar attached property
        /// </summary>
        /// <param name="element">UIElement</param>
        public static XamOutlookBar GetOutlookBar(DependencyObject element)
        {
            return (XamOutlookBar)element.GetValue(OutlookBarProperty);
        }

        private static void OnOutlookBarChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            OutlookBarGroup gr= o as OutlookBarGroup;
            if(gr!=null)
            {
                gr.ResolveSmallImage();
            }
        }
        #endregion //OutlookBar

        #region OverflowAreaGroups
        /// <summary>
        /// Returns a readonly collection of <see cref="OutlookBarGroup"/> items in the XamOutlookBar overflow area
        /// </summary>
        /// <remarks>
        /// <p class="body">The groups in the <b>OverflowAreaGroups</b> collection are displayed inside overflow area of XamOutlookBar. 
        /// The <see cref="OutlookBarGroup.LocationProperty"/> of each group in this collection is set to <b>OutlookBarGroupLocation.OverflowArea</b>.</p>
        /// </remarks>
        /// <seealso cref="OutlookBarGroupLocation"/>
        /// <seealso cref="OutlookBarGroup"/>
        /// <seealso cref="Groups"/>
        //[Description("Returns a readonly collection of OutlookBarGroup items in the XamOutlookBar overflow area")]
        //[Category("OutlookBar Properties")] // Behavior
        [Browsable(false)]
        public ReadOnlyOutlookBarGroupCollection OverflowAreaGroups
        {
            get {
                if (_overflowAreaGroupsReadOnly == null)
                    _overflowAreaGroupsReadOnly = new ReadOnlyOutlookBarGroupCollection(_overflowAreaGroups);
                return _overflowAreaGroupsReadOnly;
            }
        }
        #endregion //OverflowAreaGroups

        #region OverflowMenuButtonStyleKey

        /// <summary>
        /// The key used to identify the style used for a <see cref="MenuItem"/> used to represent a header of overflow menu.
        /// </summary>
        public static readonly ResourceKey OverflowMenuButtonStyleKey =
            new StaticPropertyResourceKey(typeof(XamOutlookBar), "OverflowMenuButtonStyleKey");

        #endregion //OverflowMenuButtonStyleKey

        #region OverflowMenuItemStyleKey
        /// <summary>
        /// The key used to identify the style used for a <see cref="MenuItem"/> used to represent an overflow menu item.
        /// </summary>
        public static readonly ResourceKey OverflowMenuItemStyleKey =
            new StaticPropertyResourceKey(typeof(XamOutlookBar), "OverflowMenuItemStyleKey");

        #endregion //OverflowMenuItemStyleKey	

        #region OverflowMenuSeparatorStyleKey
        /// <summary>
        /// The key used to identify the style used for a <see cref="Separator"/> inside the overflow menu.
        /// </summary>
        public static readonly ResourceKey OverflowMenuSeparatorStyleKey =
            new StaticPropertyResourceKey(typeof(XamOutlookBar), "OverflowMenuSeparatorStyleKey");

        #endregion //OverflowMenuSeparatorStyleKey

        #region ReserveSpaceForLargeImage 

        /// <summary>
        /// Identifies the <see cref="ReserveSpaceForLargeImage "/> dependency property
        /// </summary>
        public static readonly DependencyProperty ReserveSpaceForLargeImageProperty =
            DependencyProperty.Register("ReserveSpaceForLargeImage",
            typeof(bool), typeof(XamOutlookBar), new PropertyMetadata(KnownBoxes.TrueBox));

        /// <summary>
        /// Returns/sets whether the space exists for the LargeImage of an <see cref="OutlookBarGroup"/>. This is a dependency property.
        /// </summary>
        /// <seealso cref="ReserveSpaceForLargeImageProperty"/>
        /// <seealso cref="OutlookBarGroup"/>
        //[Description("Returns/sets whether there should be a reserved space for LargeImage of the OutlookBarGroup in the navigation area")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public bool ReserveSpaceForLargeImage 
        {
            get
            {
                return (bool)this.GetValue(XamOutlookBar.ReserveSpaceForLargeImageProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.ReserveSpaceForLargeImageProperty, value);
            }
        }

        #endregion //ReserveSpaceForLargeImage 

        #region SelectedAreaMinHeight
        /// <summary>
        /// Identifies the <see cref="SelectedAreaMinHeight"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedAreaMinHeightProperty = DependencyProperty.Register(
            "SelectedAreaMinHeight", typeof(double), typeof(XamOutlookBar),
            new FrameworkPropertyMetadata(64.0, new PropertyChangedCallback(OnSelectedAreaMinHeightChanged)));

        private bool _selectedAreaMinHeightChanged;
        private static void OnSelectedAreaMinHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamOutlookBar xamBar = d as XamOutlookBar;
            if (xamBar != null)
            {
                if (xamBar._navigationAreaIC != null)
                {
                    xamBar._selectedAreaMinHeightChanged = true;
                    xamBar._navigationAreaIC.InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Returns/sets the the minimum height of the selected group area, in device-independent units. This is a dependency property.
        /// </summary>
        /// <seealso cref="SelectedAreaMinHeightProperty"/>
        /// <seealso cref="SelectedGroup"/>
        //[Description("Returns/sets the The minimum height of the selected group area, in device-independent units.")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public double SelectedAreaMinHeight
        {
            get { return (double)this.GetValue(XamOutlookBar.SelectedAreaMinHeightProperty); }
            set { this.SetValue(XamOutlookBar.SelectedAreaMinHeightProperty, value); }
        }
        #endregion //SelectedAreaMinHeight

        #region SelectedGroup 
        /// <summary>
        /// Identifies the <see cref="SelectedGroup"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedGroupProperty = DependencyProperty.Register(
            "SelectedGroup", typeof(OutlookBarGroup), typeof(XamOutlookBar),
            new FrameworkPropertyMetadata (null, OnSelectedGroupChangedCallback, OnCoerceSelectedGroupCallback));

        private static void OnSelectedGroupChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            XamOutlookBar xamOutlookBar = (XamOutlookBar)obj;

            if (xamOutlookBar.SelectedGroup != null)
                xamOutlookBar.SelectedGroup.IsSelected = true;
            xamOutlookBar.UpdateSelectedGroupContent();

            OutlookBarGroup oldSelected = (OutlookBarGroup)args.OldValue;
            if (oldSelected != null)
                oldSelected.IsSelected = false;

            SelectedGroupChangedEventArgs selArgs =
                new SelectedGroupChangedEventArgs((OutlookBarGroup)args.OldValue, (OutlookBarGroup)args.NewValue);
            xamOutlookBar.RaiseSelectedGroupChanged(selArgs);

            // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
            xamOutlookBar.UpdateSelectedGroupVisualState();

        }

        private static object OnCoerceSelectedGroupCallback(DependencyObject obj, object value)
        {
            XamOutlookBar xamOutlookBar = (XamOutlookBar)obj;
            OutlookBarGroup gr = (OutlookBarGroup)value;

            if (!xamOutlookBar.ApproveNewSelection(gr))
                return xamOutlookBar.SelectedGroup; // new value is not approved-return current value

            return gr;
        }

        // this is for OnCoerceIsSelectedCallback only
        internal bool SelectGroup(OutlookBarGroup gr, bool isSelectedNewValue)
        {
            OutlookBarGroup newSelection = isSelectedNewValue ? gr : null;

            if ((this.SelectedGroup == gr) && !isSelectedNewValue)
            {
                this.SelectedGroup = null;
                return false;
            }//clear curren selection

            if ((this.SelectedGroup == gr)||((newSelection==null)))
                return isSelectedNewValue;  // no changes

            return SelectionApproved(gr);
        }

        private bool SelectionApproved(OutlookBarGroup gr)
        {
            OutlookBarGroup old = this.SelectedGroup;
            this.SelectedGroup = gr;
            bool ok= this.SelectedGroup == gr;
            if (ok && old != null && old != this.SelectedGroup)
                old.IsSelected = false;
            return ok;
        }

        private bool ApproveNewSelection(OutlookBarGroup newSelection)
        {
            // check for approval
            if (this.SelectedGroup == newSelection)
                return true;
            SelectedGroupChangingEventArgs args =
                new SelectedGroupChangingEventArgs(this.SelectedGroup, newSelection);
            RaiseSelectedGroupChanging(args);
            return !args.Cancel;
        }

        /// <summary>
        /// Returns/sets the currently selected group in the <see cref="XamOutlookBar"/>. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// <p class="body">The content of <b>SelectedGroup</b> is displayed in the selected group content area of <b>XamOutlookBar</b>. The amount of space allocated is determined by the <b>SelectedAreaMinHeight</b> property.</p>
        /// </remarks>
        /// <seealso cref="OutlookBarGroup"/>
        /// <seealso cref="SelectedAreaMinHeight"/>
        //[Description("Returns/sets the currently selected group in the XamOutlookBar")]
        //[Category("OutlookBar Properties")] // Behavior
        public OutlookBarGroup SelectedGroup
        {
            get { return (OutlookBarGroup)this.GetValue(XamOutlookBar.SelectedGroupProperty); }
            set { this.SetValue(XamOutlookBar.SelectedGroupProperty, value); }
        }
        #endregion //SelectedGroup

        #region SelectedGroupContent

        private static readonly DependencyPropertyKey SelectedGroupContentPropertyKey =
        DependencyProperty.RegisterReadOnly("SelectedGroupContent", typeof(object),
        typeof(XamOutlookBar), new FrameworkPropertyMetadata((object)null));

        /// <summary>
        /// Identifies the <see cref="SelectedGroupContent"/> dependency property
        /// </summary> 
        public static readonly DependencyProperty SelectedGroupContentProperty =
            SelectedGroupContentPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the Content of current <see cref="SelectedGroup"/>. This is a dependency property.
        /// </summary> 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedGroupContent
        {
            get
            {
                return GetValue(SelectedGroupContentProperty);
            }
            internal set
            {
                SetValue(SelectedGroupContentPropertyKey, value);
            }
        }

        #endregion //SelectedGroupContent	
    
        #region SelectedGroupContentTemplate

        private static readonly DependencyPropertyKey SelectedGroupContentTemplatePropertyKey = 
            DependencyProperty.RegisterReadOnly(
            "SelectedGroupContentTemplate", typeof(DataTemplate), 
            typeof(XamOutlookBar), new FrameworkPropertyMetadata((DataTemplate)null));

        /// <summary> 
        /// Identifies the <see cref="SelectedGroupContentTemplate"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedGroupContentTemplateProperty = 
            SelectedGroupContentTemplatePropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the <see cref="ContentControl.ContentTemplate"/> of current <see cref="SelectedGroup"/>. This is a dependency property.
        /// </summary> 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTemplate SelectedGroupContentTemplate
        {
            get
            {
                return (DataTemplate)GetValue(SelectedGroupContentTemplateProperty);
            }
            internal set
            {
                SetValue(SelectedGroupContentTemplatePropertyKey, value);
            }
        }

        #endregion //SelectedGroupContentTemplate	
    
        #region SelectedGroupContentTemplateSelector

        private static readonly DependencyPropertyKey SelectedGroupContentTemplateSelectorPropertyKey = DependencyProperty.RegisterReadOnly("SelectedGroupContentTemplateSelector", typeof(DataTemplateSelector), typeof(XamOutlookBar), new FrameworkPropertyMetadata((DataTemplateSelector)null));

        /// <summary> 
        /// Identifies the <see cref="SelectedGroupContentTemplateSelector"/>
        /// </summary>
        public static readonly DependencyProperty SelectedGroupContentTemplateSelectorProperty = SelectedGroupContentTemplateSelectorPropertyKey.DependencyProperty;

        /// <summary> 
        ///  Returns <see cref="DataTemplateSelector"/> which allows the app writer to provide custom style selection logic. This is a dependency property.
        /// </summary> 
        /// <seealso cref="SelectedGroup"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTemplateSelector SelectedGroupContentTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)GetValue(SelectedGroupContentTemplateSelectorProperty);
            }
            internal set
            {
                SetValue(SelectedGroupContentTemplateSelectorPropertyKey, value);
            }
        }

        #endregion //SelectedGroupContentTemplateSelector

        #region ShowGroupHeaderAsToolTip
        /// <summary>
        /// Identifies the <see cref="ShowGroupHeaderAsToolTip"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ShowGroupHeaderAsToolTipProperty = DependencyProperty.Register(
            "ShowGroupHeaderAsToolTip", typeof(bool), typeof(XamOutlookBar), new PropertyMetadata(KnownBoxes.TrueBox));

        /// <summary>
        /// Returns/sets the possibility to show OutlookBarGroup header in the overflow area as ToolTip. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <b>OutlookBarGroup</b> fills the tooltip with <b>Header</b> only when it has no tooltip property set and <b>ShowGroupHeaderAsToolTip</b> is true.</p> 
        /// </remarks>
        /// <seealso cref="OutlookBarGroup"/>
        //[Description("Returns/sets the possibility to show OutlookBarGroup header in the overflow area as ToolTip.")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public bool ShowGroupHeaderAsToolTip
        {
            get { return (bool)this.GetValue(ShowGroupHeaderAsToolTipProperty); }
            set { this.SetValue(ShowGroupHeaderAsToolTipProperty, value); }
        }
        #endregion //ShowGroupHeaderAsToolTip

        #region ShowFewerButtonsImageKey

        /// <summary>
        /// The key used to identify the icon for the "Show Fewer Buttons" menu option
        /// </summary>
        public static readonly ResourceKey ShowFewerButtonsImageKey =
            new StaticPropertyResourceKey(typeof(XamOutlookBar), "ShowFewerButtonsImageKey");

        #endregion //ShowFewerButtonsImageKey

        #region ShowMoreButtonsImageKey

        /// <summary>
        /// The key used to identify the icon for the "Show More Buttons" menu option
        /// </summary>
        public static readonly ResourceKey ShowMoreButtonsImageKey =
            new StaticPropertyResourceKey(typeof(XamOutlookBar), "ShowMoreButtonsImageKey");

        #endregion //ShowMoreButtonsImageKey

        #region ShowToolTips
        /// <summary>
        /// Identifies the <see cref="ShowToolTips"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ShowToolTipsProperty = DependencyProperty.Register(
            "ShowToolTips", typeof(bool), typeof(XamOutlookBar), new PropertyMetadata(KnownBoxes.TrueBox));

        /// <summary>
        /// Returns/sets the possibility of the tooltip appearance for the group in the overflow area. This is a dependency property.
        /// </summary>
        /// <seealso cref="ShowGroupHeaderAsToolTip"/>
        //[Description("Returns/sets the possibility of the tooltip appearance for the group in the overflow area")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public bool ShowToolTips
        {
            get { return (bool)this.GetValue(ShowToolTipsProperty); }
            set { this.SetValue(ShowToolTipsProperty, value); }
        }
        #endregion //ShowToolTips

        #region Theme

        /// <summary>
        /// Identifies the 'Theme' dependency property
        /// </summary>
        public static readonly DependencyProperty ThemeProperty = 
            ThemeManager.ThemeProperty.AddOwner(typeof(XamOutlookBar),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThemeChanged)));

        /// <summary>
        /// Gets/sets the default look for the control. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// <para class="body">If left set to null then the default 'Generic' theme will be used. 
        /// This property can be set to the name of any registered theme (see <see cref="Infragistics.Windows.Themes.ThemeManager.Register(string, string, ResourceDictionary)"/> and <see cref="Infragistics.Windows.Themes.ThemeManager.GetThemes()"/> methods).</para>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.Themes.ThemeManager"/>
        /// <seealso cref="ThemeProperty"/>
        //[Description("Gets/sets the general look of the XamOutlookBar and its elements.")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        [TypeConverter(typeof(Infragistics.Windows.Themes.Internal.OutlookBarThemeTypeConverter))]
        public string Theme
        {
            get
            {
                return (string)this.GetValue(XamOutlookBar.ThemeProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.ThemeProperty, value);
            }
        }

        #endregion //Theme

        #region VerticalSplitterLocation
        /// <summary>
        /// Identifies the <see cref="VerticalSplitterLocation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalSplitterLocationProperty = DependencyProperty.Register("VerticalSplitterLocation",
            typeof(VerticalSplitterLocation), typeof(XamOutlookBar),
            new FrameworkPropertyMetadata(VerticalSplitterLocation.Right, OnVerticalSplitterLocationChanged));

        private static void OnVerticalSplitterLocationChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            XamOutlookBar xamBar = target as XamOutlookBar;

            if (xamBar != null)
            {
                xamBar.UpdateVisualStates();
                xamBar.UpdateAllGroupsVisualState();
            }


        }

        /// <summary>
        /// Returns/sets the location of vertical splitter of the XamOutlookBar. This is a dependency property.
        /// </summary>
        /// <seealso cref="VerticalSplitterLocationProperty"/>
        //[Description("Specifies location of vertical splitter of the XamOutlookBar")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public VerticalSplitterLocation VerticalSplitterLocation
        {
            get
            {
                return (VerticalSplitterLocation)this.GetValue(XamOutlookBar.VerticalSplitterLocationProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.VerticalSplitterLocationProperty, value);
            }
        }

        #endregion //VerticalSplitterLocation

        #region VerticalSplitterResizeMode
        /// <summary>
        /// Returns/sets the VerticalSplitterResizeMode of the XamOutlookBar.
        /// </summary>
        /// <seealso cref="SplitterResizeMode"/>
        //[Description("Returns/sets the VerticalSplitterResizeMode of the XamOutlookBar")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public SplitterResizeMode VerticalSplitterResizeMode
        {
            get { return _verticalSplitterResizeMode; }
            set { _verticalSplitterResizeMode = value; }
        }
        private SplitterResizeMode _verticalSplitterResizeMode = SplitterResizeMode.Deferred;

        #endregion //VerticalSplitterResizeMode

        #region VerticalSplitterPreviewStyleKey
        /// <summary>
        /// The key used to identify the style used for a VerticalSplitterPreview control that is used during a drag operation to represent the preview of where the splitter will be positioned.
        /// </summary>
        public static readonly ResourceKey VerticalSplitterPreviewStyleKey =
            new StaticPropertyResourceKey(typeof(XamOutlookBar), "VerticalSplitterPreviewStyleKey");

        #endregion //MinimizeToggleButtonStyleKey

        #region VerticalSplitterStyleKey
        /// <summary>
        /// The key used to identify the style used for a <see cref="System.Windows.Controls.Primitives.Thumb"/> used to represent a vertcal splitter.
        /// </summary>
        public static readonly ResourceKey VerticalSplitterStyleKey =
            new StaticPropertyResourceKey(typeof(XamOutlookBar), "VerticalSplitterStyleKey");

        #endregion //MinimizeToggleButtonStyleKey

        #region VerticalSplitterWidth

        /// <summary>
        /// Identifies the <see cref="VerticalSplitterWidth"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalSplitterWidthProperty = DependencyProperty.Register("VerticalSplitterWidth",
            typeof(double), typeof(XamOutlookBar), new FrameworkPropertyMetadata(2.0));

        /// <summary>
        /// Returns/sets the width of vertical splitter. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// <p class="body">This property allows the developer to set the Width of the splitter without having to specify a Style (via the VerticalSplitterStyleKey property).</p> 
        /// </remarks>
        /// <seealso cref="VerticalSplitterWidthProperty"/>
        /// <seealso cref="VerticalSplitterStyleKey"/>
        //[Description("Returns/sets the width of vertical splitter")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public double VerticalSplitterWidth
        {
            get
            {
                return (double)this.GetValue(XamOutlookBar.VerticalSplitterWidthProperty);
            }
            set
            {
                this.SetValue(XamOutlookBar.VerticalSplitterWidthProperty, value);
            }
        }

        #endregion //VerticalSplitterWidth

        #endregion // Public Properties

        #endregion // Properties

        #region Methods

        #region Public Methods

		// JM 10-10-08 TFS8903
		#region ExecuteCommand

		/// <summary>
		/// Executes the specified RoutedCommand.
		/// </summary>
		/// <param name="command">The RoutedCommand to execute.</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="OutlookBarCommands"/>
		public bool ExecuteCommand(RoutedCommand command)
		{
			if (command.CanExecute(null, this))
				return this.ExecuteCommandImpl(command, null, null, null);
			else
				return false;
		}

		#endregion //ExecuteCommand

		#region GroupMoveDown

		/// <summary>
        /// Moves specified <see cref="OutlookBarGroup"/> backward into the Groups collection.
        /// </summary>
        /// <param name="group">An <see cref="OutlookBarGroup"/> which changes its position</param>
        public void GroupMoveDown(OutlookBarGroup group)
        {
            int oldIndex = Groups.IndexOf(group);

            int newIndex;
            for (newIndex = oldIndex + 1; newIndex < Groups.Count; newIndex++)
                if (Groups[newIndex].Visibility != Visibility.Collapsed)
                    break;

            if (newIndex > 0 && newIndex < Groups.Count)
            {
                if (group.Visibility != Visibility.Collapsed)
                {
                    int areaIndexOld, areaIndexNew;
                    OutlookBarGroup group2 = Groups[newIndex];
                    OutlookBarGroupCollection srcCollection = GetGroupsFromLocation(group);
                    OutlookBarGroupCollection destCollection = GetGroupsFromLocation(group2);
                    if (group2.Location == group.Location)
                    {
                        areaIndexOld = srcCollection.IndexOf(group);
                        areaIndexNew = destCollection.IndexOf(group2);
                        srcCollection.Move(areaIndexOld, areaIndexNew);
                    }//end if- the new location is on the same area
                    else
                    {
                        OutlookBarGroupLocation location2 = group2.Location;
                        OutlookBarGroupLocation location = group.Location;

                        srcCollection.Remove(group);    // this will shift up group2

                        bool noSpaceFor2nd = destCollection.Remove(group2);  // 

                        group.SetLocation(location2);

                        destCollection.Insert(0, group);

                        if (noSpaceFor2nd)
                        {
                            group2.SetLocation(location2);
                            destCollection.Insert(0, group2);
                        }// end if- cannot shift up automatic; shift manual
                    }//end else- the new location is on a new area
                }
                Groups.Move(oldIndex, newIndex);
            }
        }

        #endregion //GroupMoveDown	
            
        #region GroupMoveUp

        /// <summary>
        /// Moves specified <see cref="OutlookBarGroup"/> forward into the Groups collection.
        /// </summary>
        /// <param name="group">An <see cref="OutlookBarGroup"/> which changes its position</param>
        public void GroupMoveUp(OutlookBarGroup group)
        {
            int oldIndex = Groups.IndexOf(group);

            int newIndex;
            for (newIndex = oldIndex - 1; newIndex >= 0; newIndex--)
                if (Groups[newIndex].Visibility != Visibility.Collapsed)
                    break;

            if (newIndex >= 0 && newIndex < Groups.Count)
            {
                OutlookBarGroup group2 = Groups[newIndex];
                GroupMoveDown(group2);
            }
        }

        #endregion //GroupMoveUp	
    
        #endregion //Public Methods

        #region Protected Methods

        #region VisualState... Methods


        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            // set minimized state
            if (this.IsMinimized)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateMinimized, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNotMinimized, useTransitions);

            // set splitter location state
            if (this.VerticalSplitterLocation == VerticalSplitterLocation.Left)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateLeft, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateRight, useTransitions);

        }

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamOutlookBar xamBar = target as XamOutlookBar;

            if (xamBar != null)
                xamBar.UpdateVisualStates();
        }

        // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
        #region UpdateSelectedGroupVisualState

        private void UpdateAllGroupsVisualState()
        {
            foreach (OutlookBarGroup group in this.Groups)
                group.UpdateVisualStates();

            this.UpdateSelectedGroupVisualState();
        }

        #endregion //UpdateSelectedGroupVisualState

        // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
        #region UpdateSelectedGroupVisualState

        private void UpdateSelectedGroupVisualState()
        {
            SelectedGroupHeader header = Utilities.GetDescendantFromType<SelectedGroupHeader>(this, true, null);

            if (header != null)
                header.UpdateVisualStates();

            SelectedGroupContent content = Utilities.GetDescendantFromType<SelectedGroupContent>(this, true, null);

            if (content != null)
                content.UpdateVisualStates();
        }

        #endregion //UpdateSelectedGroupVisualState	

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



        #endregion //VisualState... Methods

        #endregion //Protected Methods

        #region Internal Methods

		#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
		#endregion // GetString

        internal void SetGroups(OutlookBarGroupCollection newGroups)
        {
			// JM 02-17-09 TFS 13997 - Refactor this code to replace the contents of the XamOutlookBar's Groups collection with the new
			//						   set of Groups rather than simply setting the XamOutlookBar.Groups to a new collection instance.  
			//if (_groups != null)
			//{
			//    foreach (OutlookBarGroup gr in _groups)
			//    {
			//        RemoveFromLogicalTree(gr);
			//    }
			//}

			//_icGroupsSource = new GroupsItemsSource(groups); // items control - GroupsSource implementation
			//_groups = groups;
			//this._groups.CollectionChanged += new NotifyCollectionChangedEventHandler(Groups_CollectionChanged);
			//this._groups.ItemPropertyChanged += new EventHandler<ItemPropertyChangedEventArgs>(Groups_ItemPropertyChanged);
			//_groups.Refresh();
			//UpdateSelectedGroupContent();


			// If we already have a Groups collection, remove each Group from the LogicalTree before proceeding
			// JM 08-17-10 TFS23338 - If we are bound, rebuild the groups.
			if (this.GroupsSource != null)
				this._icGroupsSource.RebuildAllGroups();
			else
			if (this._groups != null)
			{
				foreach (OutlookBarGroup gr in this._groups)
				{
					RemoveFromLogicalTree(gr);
				}
			}
			else	// Create an empty Groups collection and listen for changes.
			{
				// Create and initialize a new Groups collection and hook into change events.
				this._groups						= new OutlookBarGroupCollection(this);
				this._groups.CollectionChanged		+= new NotifyCollectionChangedEventHandler(Groups_CollectionChanged);
				this._groups.ItemPropertyChanged	+= new EventHandler<ItemPropertyChangedEventArgs>(Groups_ItemPropertyChanged);


				// Create a new GroupItemsSource instance and initialize it with the new groups.
				_icGroupsSource						= new GroupsItemsSource(this._groups); // items control - GroupsSource implementation
			}


			// Re-initialize the Groups collection with the new Groups by calling the ReInitalize.  This method will clear the collection, suspend change
			// notifications, copy over the new Groups and resume change notifications and send a Reset notification.
			this._groups.ReInitialize(newGroups as IList<OutlookBarGroup>);
			UpdateSelectedGroupContent();
		}

        internal void SetSplitterIncrement()
        {
            int lastVisibleIndex = GetLastVisibleGroupIndex();
            if (_splitter != null)
            {
                if (lastVisibleIndex < 0)
                    _splitter.DragIngrementDn = 0;
                else
                    _splitter.DragIngrementDn = _navigationAreaGroups[lastVisibleIndex].ActualHeight;
                if (lastVisibleIndex + 1 < Groups.Count && _navigationAreaIC != null)
                    _splitter.DragIngrementUp = _navigationAreaIC.NextAreaItemHeight;
                else
                    _splitter.DragIngrementUp = 0;
            }
        }

        internal void SaveGroupOrderAndVisibility()
        {
            int lastPosition=0;

            foreach (OutlookBarGroup gr in Groups)
                if (gr.InitialOrder != int.MaxValue && lastPosition < gr.InitialOrder)
                    lastPosition = gr.InitialOrder;

            for (int i = 0; i < Groups.Count; i++)
            {
                OutlookBarGroup gr = Groups[i];
                if (gr.InitialOrder == int.MaxValue)
                {
                    gr.InitialOrder = ++lastPosition;
                    gr.IsVisibleOnStart = gr.Visibility != Visibility.Collapsed;
                }
            }
        }

        internal void RefreshNavigationArea(bool reloadGroups)
        {
            if (_navigationAreaIC != null)
                GetMaxGroupsInTheNavigationArea(NavigationAreaMaxGroupsResolved, reloadGroups);
        }
        #endregion //Internal Methods	
    
        #region Private Methods

        #region Eventhandlers

        void XamOutlookBar_Loaded(object sender, RoutedEventArgs e)
        {
			// JM 03-10-09 - While fixing TFS 11436, noticed that this Loaded handler which is set in OnApplyTemplate, is never unhooked.  Since OnApplyTemplate
			//				 can get called multiple times, we would be setting this handler multiple times.  Adding code here to unhook since we should only need 
			//				 to respond to Loaded once.
			this.Loaded -= new RoutedEventHandler(XamOutlookBar_Loaded);


			// 10-23-08 - If the groups collection was changed before we were loaded, initialize the groups now.
			if (this._groupsCollectionChangedBeforeControlLoaded)
			{
				this.InitializeGroups(true, false);
				this._groupsCollectionChangedBeforeControlLoaded = false;
			}

            GetMaxGroupsInTheNavigationArea(NavigationAreaMaxGroupsResolved, true);
            
            VerifySelectedGroup(true);      // have to select some group
            
            if (_mustCoerceWidth)
            {
                this.CoerceValue(WidthProperty);
                this.InvalidateMeasure();
            }// end if- outlookbar starts minimized; there is a problem at designtime 
        }

        void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
			// JM 07-31-08 - e.Height is sometimes true when the height hasn't actually changed.
			//				 Change the check to also require that the New height does not equal
			//				 the Previous height.
			bool heightChanged = e.HeightChanged && e.PreviousSize.Height != e.NewSize.Height;
			if (this.IsLoaded && heightChanged)
			{
                double deltaH = e.PreviousSize.Height - e.NewSize.Height;
                if ((NavigationAreaMaxGroupsResolved != _navigationAreaGroups.Count)
                    || (deltaH + 0.1 > _splitter.DragIngrementDn)
                    || (-deltaH > _splitter.DragIngrementUp - 0.1)
                    || _selectedAreaMinHeightChanged)
                {
                    GetMaxGroupsInTheNavigationArea(NavigationAreaMaxGroupsResolved, true);
                    _selectedAreaMinHeightChanged = false;
                }//end if- this is for fast changes of height - maximize/restore
            }//end if- try to keep desired number of groups on the navigation area
        }

		// JM 01-11-11 TFS61189 Added.
		#region XamOutlookBar_Unloaded
		void XamOutlookBar_Unloaded(object sender, RoutedEventArgs e)
		{
			this.Unloaded	-= new RoutedEventHandler(XamOutlookBar_Unloaded);
			this.Loaded		+=new RoutedEventHandler(XamOutlookBar_ReLoaded);

			// JM 03-10-11 TFS67362.  Save the index of the currently selected group so we can restore the selected
			// group if/when we are reloaded.
			if (null != this.SelectedGroup &&
				null != this._groups)
				this._previouslySelectedGroupIndex = this._groups.IndexOf(this.SelectedGroup);
			else
				this._previouslySelectedGroupIndex = -1;
		}
		#endregion //XamOutlookBar_Unloaded

		// JM 01-11-11 TFS61189 Added.
		#region XamOutlookBar_ReLoaded
		void XamOutlookBar_ReLoaded(object sender, RoutedEventArgs e)
		{
			this.Unloaded	+= new RoutedEventHandler(XamOutlookBar_Unloaded);
			this.Loaded		-= new RoutedEventHandler(XamOutlookBar_ReLoaded);

			if (this.GroupsSource != null)
			{
				if (this._groups != null)
					this._groups.CanEdit = true; // enables direct editing of groups

				this._icGroupsSource.RebuildAllGroups();

				// JM 03-25-11 TFS61189, TFS67362
				this.InitializeGroups(true, true);

				if (this._groups != null)
					this._groups.CanEdit = false; // enables direct editing of groups
			}

			// JM 03-10-11 TFS67362.  Try to select the group that was selected when we were unloaded.
			if (this._previouslySelectedGroupIndex > -1 &&
				null != this._groups &&
				this._groups.Count > 0)
			{
				if (this._previouslySelectedGroupIndex < this._groups.Count)
					this.SelectedGroup = this._groups[this._previouslySelectedGroupIndex];
				else
					this.SelectedGroup = this._groups[this._groups.Count - 1];

				this._previouslySelectedGroupIndex = -1;
			}
		}
		#endregion //XamOutlookBar_ReLoaded

        #endregion //Eventhandlers

        #region Width Coerece 

        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {
            // we should return a specific value when XamOutlookBar is minimized
            XamOutlookBar xamBar = o as XamOutlookBar;
            ValueSource vs = DependencyPropertyHelper.GetValueSource(xamBar, WidthProperty);

            if (xamBar._animationInProcess)
            {
                return baseValue;
            }
            
            if (xamBar != null )
            {
                double minWidthCoerced=(double)baseValue;
                if (xamBar.IsMinimized)
                {
					// JM 04-27-11 TFS73871
					xamBar._minimizedWidth = xamBar.GetMinimizedWidth();

                    minWidthCoerced = xamBar.MinimizedWidthResolved; // the minimized width
					// JM 10-22-09 TFS23110, TFS20587
					//if ((double)baseValue > xamBar.minExpandedWidth)
					if (double.IsNaN((double)baseValue) || (double)baseValue > xamBar.minExpandedWidth)
							xamBar._lastExpandedWidth = (double)baseValue;
                }
                else if (xamBar._expanding)
                {
                    minWidthCoerced = double.NaN;

					// JM 02-04-09 TFS11748 - Only set an explicit width if we have a 'last expanded width' that is not NaN
					if (double.IsNaN(xamBar._lastExpandedWidth) == false)
						// JM 10-23-09 TFS 24169
						//minWidthCoerced = xamBar._lastExpandedWidth > xamBar.minExpandedWidth ?
						//    xamBar._lastExpandedWidth : 2 * xamBar.minExpandedWidth;
						minWidthCoerced = xamBar._lastExpandedWidth > xamBar.minExpandedWidth ?
							xamBar._lastExpandedWidth : xamBar.minExpandedWidth;
				}
                else
                    return baseValue;
                return minWidthCoerced;
            }
            return baseValue;
        }

        private static void OnWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            XamOutlookBar xamBar = o as XamOutlookBar;

			// JM 10-023-09
			//xamBar.setMinimizedByWidth((double)e.NewValue);
			if (xamBar.IsLoaded && xamBar.IsInitialized)
			{
				xamBar._inferMinimizedStateFromWidth = true;
				xamBar.setMinimizedByWidth((double)e.NewValue);
				xamBar._inferMinimizedStateFromWidth = false;
			}
        }

        bool setMinimizedByWidth(double newWidth)
        {
			// JM 10-22-09 TFS20587 - Only infer the minimized state from our width when the Width is begin changed by dragging the Vertical splitter.
			if (this._inferMinimizedStateFromWidth == false)
				return false;

            if (this._animationInProcess)
                return false;

            bool oldValueIsMinimized = this.IsMinimized;

			// JM 10-23-09
			//if (newWidth > this.minExpandedWidth && this.IsMinimized)
			if ((double.IsNaN(newWidth) || newWidth > this.minExpandedWidth) && this.IsMinimized)
                this.IsMinimized = false;
            else if (newWidth < this.minExpandedWidth && !this.IsMinimized)
                this.IsMinimized = newWidth > 0;
            if (!this.IsMinimized)
                this._lastExpandedWidth = newWidth;
            return oldValueIsMinimized != this.IsMinimized;
        }


        #endregion //Width Corce, Changed	
    
        #region Vertical splitter methods

        private void AddAdorner()
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_verticalSplitter);
            if (null != layer)
            {
                this._adorner = new SplitterPreviewAdorner(_verticalSplitter);

				// JM 02-09-09 TFS12731
				layer.FlowDirection			= this.FlowDirection;
				this._adorner.FlowDirection = this.FlowDirection;

                layer.Add(this._adorner);
                this.AddToLogicalTree(_adorner);
            }
        }
        private void RemoveAdorner()
        {
            if (this._adorner != null)
            {
                AdornerLayer layer = VisualTreeHelper.GetParent(this._adorner) as AdornerLayer;
                if (null != layer)
                    layer.Remove(this._adorner);

                RemoveFromLogicalTree(_adorner);
                this._adorner = null;
            }
        }

        void VerticalSplitter_DragStarted(object sender, DragStartedEventArgs e)
        {
            if ((null == this._adorner) &&
                (this.VerticalSplitterResizeMode == SplitterResizeMode.Deferred))
            {
                RemoveAdorner();
                AddAdorner();
                if (!this.IsMinimized)
                    this._lastExpandedWidth = this.ActualWidth;
            }
            else
                RemoveAdorner();

			// JM 10-22-09 TFS23110, TFS20587
			this._inferMinimizedStateFromWidth = true;
		}

        void VerticalSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
			// JM 02-09-09 TFS12731
			//double delta = this.VerticalSplitterLocation == VerticalSplitterLocation.Right ?
			//	e.HorizontalChange : -e.HorizontalChange;
			double delta = this.VerticalSplitterLocation == VerticalSplitterLocation.Right && this.FlowDirection == FlowDirection.LeftToRight ?
			    e.HorizontalChange : -e.HorizontalChange;
            double w = this.ActualWidth + delta;
            if (this.IsMinimized && (w >= this.minExpandedWidth))
            {
                bool useAnimations = this._useAnimations;
                this._useAnimations = false || (this.VerticalSplitterResizeMode == SplitterResizeMode.Deferred);
                this._lastExpandedWidth = w;
                this.IsMinimized = false;
                this._useAnimations = useAnimations;
            }
            if ((w >= 1) && (this.VerticalSplitterResizeMode == SplitterResizeMode.Deferred))
            {
                if (!_animationInProcess)
                    this.BeginAnimation(WidthProperty, null);

                if (!this.IsMinimized && (w < this.minExpandedWidth))
                    this.IsMinimized = true;
                else
                    this.Width = w;
            }

            RemoveAdorner();

			// JM 10-22-09 TFS23110, TFS20587
			this._inferMinimizedStateFromWidth = false;
		}
        void VerticalSplitter_DragDelta(object sender, DragDeltaEventArgs e)
        {
			// JM 02-09-09 TFS12731
			//double delta = this.VerticalSplitterLocation == VerticalSplitterLocation.Right ?
			//	e.HorizontalChange : -e.HorizontalChange;
			double delta = this.VerticalSplitterLocation == VerticalSplitterLocation.Right && this.FlowDirection == FlowDirection.LeftToRight ?
			    e.HorizontalChange : -e.HorizontalChange;

            double w = this.ActualWidth + delta;

            if (this.VerticalSplitterResizeMode == SplitterResizeMode.Immediate)
            {
                if (this.IsMinimized && (w > this.minExpandedWidth))
                {
                    bool useAnimations = this._useAnimations;
                    this._useAnimations = false;
                    this._lastExpandedWidth = w;
                    this.IsMinimized = false;
                    this._useAnimations = useAnimations;
                }
                if (w >= 1)
                {
                    this.BeginAnimation(WidthProperty, null);
                    this.Width = w;
                }
            }
            else
            {
                double splitterDelta;
                double minW = double.IsNaN(this.MinimizedWidth) ? GetMinimizedWidth() : this.MinimizedWidth;
                if (w < this.minExpandedWidth)
                    splitterDelta = minW - this.ActualWidth;  // snap to MinimizedWidth
                else
                    splitterDelta = delta;

				// JM 02-09-09 TFS12731
				//if (this.VerticalSplitterLocation == VerticalSplitterLocation.Left)
				if (this.VerticalSplitterLocation == VerticalSplitterLocation.Left || this.FlowDirection == FlowDirection.RightToLeft)
                    splitterDelta *= -1;

                if (this._adorner != null)
                    this._adorner.OffsetX = splitterDelta;

            }//end else - show preview
        }

        #endregion //Vertical splitter methods	

        #region Minimized width calculation

        private double GetLargestGroupImage()
        {
            //// This searchs for largest image to set size when XamOutlookBar is minimized
            double minWidth = this.MinWidth, width;

            // check all the groups
            double parentMarginsWidth = 0;
            foreach (OutlookBarGroup gr in Groups)
            {
                if ((gr.Visibility == Visibility.Visible))
                {
                    width = 0;
                    if (gr.Image != null)
                        parentMarginsWidth = GetParentMargins(gr.Image);
                    if (gr.LargeImage != null)
                        width = gr.LargeImage.Width + parentMarginsWidth;
                    else if (gr.Template != null) 
                    {
                        ColumnDefinition imgCol = gr.Template.FindName("ImageColumn", gr) as ColumnDefinition;
                        if (imgCol != null)
                            width = imgCol.ActualWidth + parentMarginsWidth;
                    }
                    if (minWidth < width)
                        minWidth = width;
                }// end if- check visible groups only
            }// end for - get width of widest group image in the navigation area

            return minWidth;
        }

        private double GetParentMargins(FrameworkElement child)
        {
            double w = 0;
            DependencyObject o = child;
            for (o = child; o != null; o = VisualTreeHelper.GetParent(o))
            {
                if (child is OutlookBarGroup)
                    break;
                child = o as FrameworkElement;
                if (child == null)
                    continue;
                w += Math.Max(child.Margin.Left, child.Margin.Right);
            }
            return 2 * w;
        }

        private double GetMinimizedWidth()
        {
			// JM 09-03-09 TFS21655 - Just need to look at the first group in the navigation area.
            //double minWidth = GetLargestGroupImage();
			double minWidth = this.GetFirstNavigationGroupImageWidth();
            if (minWidth < this.MinWidth)
                minWidth = this.MinWidth;
            double dw = 2;
			// JM 04-27-11 TFS73871
			//if (_verticalSplitter != null)
			//    if (_verticalSplitter.IsVisible || this.IsVerticalSplitterVisible)
			//        dw += _verticalSplitter.ActualWidth;
			if (this.IsVerticalSplitterVisible)
				dw += this.VerticalSplitterWidth;

			return Math.Max(minWidth + dw, 32);
        }

		// JM 09-03-09 TFS21655 - Added.
		// Gets the Width of the image in the first group in the navigation area.
		private double GetFirstNavigationGroupImageWidth()
		{
			// JM 04-27-11 TFS73871 - Add the left/right margins of the PART_Image element
			// Initialize the width to 24 (got this value from the MaxWidth of the PART_Image element in the OutlookBarGroup template).
			//double width = 24;
			double width = 26;

			OutlookBarGroup firstGroup = this.NavigationAreaGroups.Count > 0 ? this.NavigationAreaGroups[0] : null;
			if (firstGroup != null)
			{
				// Add up the widths of all the margins from the image element up to the OutlookBarGroup element.
				double parentMarginsWidth = 0;
				if (firstGroup.Image != null)
					parentMarginsWidth = GetParentMargins(firstGroup.Image);
				
				// Try to use the ActualWidth of the Grid Column containing the Image element as the base width.
				if (firstGroup.Template != null) 
                {
                    ColumnDefinition imgCol = firstGroup.Template.FindName("ImageColumn", firstGroup) as ColumnDefinition;
					// JM 04-27-11 TFS73871
					//if (imgCol != null)
					if (imgCol				!= null && 
						false				== double.IsNaN(imgCol.ActualWidth) &&
						imgCol.ActualWidth	> 0)
						width = imgCol.ActualWidth + parentMarginsWidth;
                }
				else
				// Fall back to the Width of the Image element.
                if (firstGroup.LargeImage != null)
					width = firstGroup.Image.Width + parentMarginsWidth;
			}

			return width;
		}

        #endregion //Minimized width calculation	
    
        #region Splitter

        private bool ShrinkNavigationArea()
        {
            if (_splitter != null)
                return _splitter.ShrinkNavigationArea();
            return false;
        }

        private bool ExtendNavigationArea()
        {
            if (_splitter != null)
                return _splitter.ExtendNavigationArea();
            return false;
        }

        private int GetLastVisibleGroupIndex()
        {
            int i;
            for (i = _navigationAreaGroups.Count - 1; i >= 0; i--)
                if (_navigationAreaGroups[i].Visibility != Visibility.Collapsed)
                    break;
            return i;
        }

        #endregion //Splitter	

        #region Groups Eventhandlers

        void Groups_ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            OutlookBarGroup changedGroup = e.Item as OutlookBarGroup;

            if (e.PropertyName == "Visibility")
            {
                if (changedGroup.Visibility == Visibility.Collapsed)
                {
                    //changedGroup.IsSelected = false;
                    switch (changedGroup.Location)
                    {
                        case OutlookBarGroupLocation.NavigationGroupArea:
                            _navigationAreaGroups.Remove(changedGroup);
                            break;
                        case OutlookBarGroupLocation.OverflowArea:
                            _overflowAreaGroups.Remove(changedGroup);
                            break;
                        case OutlookBarGroupLocation.OverflowContextMenu:
                            _contextMenuGroups.Remove(changedGroup);
                            break;
                    }
                    changedGroup.SetLocation(OutlookBarGroupLocation.None);
                }// end if- group is removed from UI area
                else
                {
                    InitializeGroups(true, false);
                    GetMaxGroupsInTheNavigationArea(this.NavigationAreaMaxGroupsResolved, true);
                }// end else- group is added to UI area
            }//end if- show/hide a group
            if (e.PropertyName == "ToolTip")
            {
                if (changedGroup.Image != null)
                {
                    if (changedGroup.ToolTip != null)
                        changedGroup.Image.SetValue(ToolTipService.IsEnabledProperty, false);
                }
            }//end if- resolve tooltips

            if (changedGroup.OutlookBar == null)
                XamOutlookBar.SetOutlookBar(changedGroup, this);
        }
        void Groups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OutlookBarGroup changedGroup;
            AddRemoveLogicalChildren(e);
            UpdateSelectedGroupContent();

			if (!this.IsLoaded)
			{
				// JM 10-23-08 - Set a flag that tells us the groups collection changed before we were loaded.
				this._groupsCollectionChangedBeforeControlLoaded = true;

				return;
			}

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    InitializeGroups(true, true);
                    break;
                case NotifyCollectionChangedAction.Add:
                    changedGroup = e.NewItems[0] as OutlookBarGroup;
                    if (this.SelectedGroup != changedGroup && IsInitialized)
                        changedGroup.IsSelected = false; 

                    int i;
                    for (i = Groups.IndexOf(changedGroup) - 1; i >= 0; i--)
                        if (Groups[i].Visibility != Visibility.Collapsed)
                            break;

                    if (i >= 0)
                        changedGroup.SetLocation(Groups[i].Location); // like prev visible group
                    else
                        changedGroup.SetLocation(OutlookBarGroupLocation.NavigationGroupArea);

                    switch (changedGroup.Location)
                    {
                        case OutlookBarGroupLocation.NavigationGroupArea:
                            InitializeGroups(true, true);
                            break;
                        case OutlookBarGroupLocation.OverflowArea:
                            InitAreaGroup(_overflowAreaGroups, changedGroup.Location);
                            break;
                        case OutlookBarGroupLocation.OverflowContextMenu:
                            InitAreaGroup(_contextMenuGroups, changedGroup.Location);
                            break;
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    changedGroup = e.OldItems[0] as OutlookBarGroup;
                    changedGroup.IsSelected = false;
                    switch (changedGroup.Location)
                    {
                        case OutlookBarGroupLocation.NavigationGroupArea:
                            _navigationAreaGroups.Remove(changedGroup);
                            break;
                        case OutlookBarGroupLocation.OverflowArea:
                            _overflowAreaGroups.Remove(changedGroup);
                            break;
                        case OutlookBarGroupLocation.OverflowContextMenu:
                            _contextMenuGroups.Remove(changedGroup);
                            break;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    this.UpdateLayout();
                    GetMaxGroupsInTheNavigationArea(NavigationAreaMaxGroupsResolved, false);
                    break;
            }
            if (this.IsMinimized)
                this.CoerceValue(WidthProperty);    // set new width if it is set to auto

        }


        #endregion //Groups Eventhandles	
    
        #region LocalTree methods

        private void AddRemoveLogicalChildren(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Reset:

                    if (e.OldItems != null)
                    {
                        foreach (OutlookBarGroup gr in e.OldItems)
                        {
                            RemoveFromLogicalTree(gr);
                        }// remove OldItems from the logical tree
                    }

                    if (e.NewItems != null)
                    {
                        foreach (OutlookBarGroup gr in e.NewItems)
                        {
                            AddToLogicalTree(gr);
                        }// add NewItems to the logical tree
                    }
                    break;
            }
        }

        private DependencyObject FindRootInLogicalTree(DependencyObject obj)
        {
            DependencyObject root;
            for (root = obj; obj != null; obj = LogicalTreeHelper.GetParent(obj))
            {
                root = obj;
                if (root == this)
                    return this;
            }

            return root;
        }

        private void AddToLogicalTree(DependencyObject obj)
        {
            obj = FindRootInLogicalTree(obj);
            if (obj == null || obj == this)
                return;
            if (_logicalChildren == null)
                _logicalChildren = new List<DependencyObject>();
            else if (_logicalChildren.IndexOf(obj) >= 0)
                return;

            AddLogicalChild(obj);
            _logicalChildren.Add(obj);
        }

        private void RemoveFromLogicalTree(DependencyObject obj)
        {
            if (obj == null)
                return;

            if (_logicalChildren != null)
                _logicalChildren.Remove(obj);
            RemoveLogicalChild(obj);
        }

        #endregion //LocalTree methods	

        #region Groups location

        // AS 3/12/09 TFS11010
        // While implementing unit tests for verifying the logical tree I found that
        // sometimes InitializeGroups is called recursively. I was a bit leary of putting 
        // in standard anti-recursion since the recursive calls may have been relying on 
        // the InitializeGroups processing being completed. Instead I added a version 
        // number so the outer call could know that a recursive call had occurred and could
        // bail out.
        //
        private int _initializeGroupVersion = 0;

        private void InitializeGroups(bool moveToNavigationArea, bool fillNavAreaWithDesiredNumberOfGroups)
        {
            // AS 3/12/09 TFS11010
            _initializeGroupVersion++;
            int origVersion = _initializeGroupVersion;

            ClearUIcollections();
            this.UpdateLayout();

            // AS 3/12/09 TFS11010
            if (origVersion != _initializeGroupVersion)
                return;

            if (moveToNavigationArea)
            {
                foreach (OutlookBarGroup gr in Groups)
                    gr.SetLocation(OutlookBarGroupLocation.NavigationGroupArea);
            }

            GetGroups(_navigationAreaGroups, Groups.Count, Groups, 0,
                OutlookBarGroupLocation.NavigationGroupArea);
            
            if (fillNavAreaWithDesiredNumberOfGroups)
            {
                this.UpdateLayout();

                // AS 3/12/09 TFS11010
                if (origVersion != _initializeGroupVersion)
                    return;

                GetMaxGroupsInTheNavigationArea(NavigationAreaMaxGroupsResolved, false);
            }// end if- rearrange navigation area
        }

        private int GetGroups(OutlookBarGroupCollection dest, int maxItemsInDest,
            OutlookBarGroupCollection source, int startIndex, OutlookBarGroupLocation loc)
        {
            OutlookBarGroupCollection tempColl = new OutlookBarGroupCollection(null);
            int i, n = 0;
            for (i = startIndex; (i < source.Count) && (dest.Count < maxItemsInDest); i++, n++)
            {
                OutlookBarGroup gr = source[i];
                if (gr.Visibility != Visibility.Collapsed)
                {
                    gr.SetLocation(loc);
                    tempColl.Add(gr);
                }
                else
                    gr.SetLocation(OutlookBarGroupLocation.None);
            }
            dest.AddRange(tempColl);

            return i;
        }

        private int InitAreaGroup(OutlookBarGroupCollection dest, OutlookBarGroupLocation loc)
        {
            dest.Clear();
            foreach (OutlookBarGroup gr in Groups)
            {
                if ((gr.Location == loc) && (gr.Visibility!=Visibility.Collapsed))
                    dest.Add(gr);
            }
            return dest.Count;
        }

        private void ArrangeGroups(bool reset)
        {
            if (this.Groups.Count == 0)
            {
                ClearUIcollections();
                return;
            }
            else
                SetSplitterIncrement();
        }

        private void ClearUIcollections()
        {
            this._contextMenuGroups.Clear();
            this._overflowAreaGroups.Clear();
            this._navigationAreaGroups.Clear();
        }

        #endregion //Groups location	
    
        #region Storyboard, animations ...

        bool CanAnimate(bool minimizing)
        {
            // returns value that indicates 'can XOB start animations'
            if (minimizing)
            {
                if (MinimizeStoryboard == null)
                    return false;
            }
            else if (ExpandStoryboard == null)
                return false;

            return _useAnimations && !DesignerProperties.GetIsInDesignMode(this);
        }

        void AnimateMinimizing()
        {
            // animates minimizing of XOB
            if (!CanAnimate(true))
                return;

            if (_animationInProcess)
                return;

            if (this.ActualWidth < minExpandedWidth)
                return;

            _sbMinimize = MinimizeStoryboard.IsFrozen ? MinimizeStoryboard.Clone() : MinimizeStoryboard;

            DoubleAnimation da = GetWidthAnimation(_sbMinimize);
            if (da != null)
            {
                da.From = this.ActualWidth;
                da.To = this._minimizedWidth;
                da.FillBehavior = FillBehavior.HoldEnd;
            }

            _animationInProcess = true;
            _sbMinimize.Completed += new EventHandler(MinimizeStoryboard_Completed);
            _sbMinimize.Begin(this);
        }

        void MinimizeStoryboard_Completed(object sender, EventArgs e)
        {
            this.IsMinimized = true;
            _animationInProcess = false;
            this.CoerceValue(WidthProperty);
            _sbMinimize.Completed -= MinimizeStoryboard_Completed;
            CommandManager.InvalidateRequerySuggested();
        }

        void AnimateExpanding()
        {
            // animates expanding of XOB
            if (!CanAnimate(false))
                return;
            if (_animationInProcess)
                return;
            _sbExpand = ExpandStoryboard.IsFrozen ? ExpandStoryboard.Clone() : ExpandStoryboard;
            DoubleAnimation da = GetWidthAnimation(_sbExpand);
            if (da != null)
            {
                if (!double.IsNaN(_lastExpandedWidth))
                    da.To = _lastExpandedWidth;
                else
                    da.To = 2 * minExpandedWidth;

                da.FillBehavior = FillBehavior.HoldEnd;// FillBehavior.Stop;
                da.From = ActualWidth; ;
            }
            else
                this.CoerceValue(WidthProperty);

            _animationInProcess = true;

            _sbExpand.Completed += new EventHandler(ExpandStoryboard_Completed);
            _sbExpand.Begin(this);
        }

        void ExpandStoryboard_Completed(object sender, EventArgs e)
        {
            _animationInProcess = false;
            this.IsMinimized = false;
            _sbExpand.Completed -= ExpandStoryboard_Completed;
            this.RaiseNavigationPaneExpanded(new RoutedEventArgs());
        }

        DoubleAnimation GetWidthAnimation(Storyboard storyboard)
        {
            if (storyboard == null)
                return null;

            // finds animation of width; 
            // we must set correct From, To and FillBehavor properties

            DoubleAnimation anim = null;
            foreach (Timeline timeline in storyboard.Children)
            {
                anim = timeline as DoubleAnimation;
                if (anim == null)
                    continue;

                string targetName = Storyboard.GetTargetName(anim);
                if (!String.IsNullOrEmpty(targetName) && targetName != this.Name)
                    continue;

                PropertyPath propPath = Storyboard.GetTargetProperty(anim);
                if (propPath == null)
                    continue;

                if (propPath.Path == "Width")
                    return anim;
            }
            return null;
        }

        #endregion //Storyboard, animations ...	
    
        #region Groups methods

        private int GetMaxGroupsInTheNavigationArea(int desiredCount, bool clearGroupsBefore)
        {

            if (clearGroupsBefore && desiredCount > 0)
                GetMaxGroupsInTheNavigationArea(0, false);

            int i, n;

            n = desiredCount < 0 ?
                 Groups.Count : desiredCount - _navigationAreaGroups.Count;


            if (this.IsLoaded && _splitter != null)//&& !DesignerProperties.GetIsInDesignMode(this)
            {
                int j, cnt = desiredCount < 0 ? this.Groups.Count : Math.Min(desiredCount, this.Groups.Count);
                int nmbGroups = this.Groups.Count;

                double goodh = 0, h = 0;
                for (j = 0; j < nmbGroups && cnt > 0; j++)
                {
                    if (this.Groups[j].Visibility == Visibility.Collapsed)
                        continue;
                    cnt--;
                    h += this.Groups[j].DesiredNavigationHeight;

                    if (_splitter.CanSetNavigationAreaSize(h))
                        goodh = h;
                    else
                        break;
                }
                if (_splitter.SetNavigationAreaSize(goodh))
                {
                    SetSplitterIncrement();
                    return desiredCount;
                }
                return _navigationAreaGroups.Count;
            }

            if (n > 0)
            {
                for (i = 0; i < n; i++)
                {
                    if (!ExtendNavigationArea())
                        break;
                    this.UpdateLayout();
                    SetSplitterIncrement();
                }//end for - 
            }//end if - must extend navigation area
            else if (n < 0)
            {
                n *= -1;
                for (i = 0; i < n; i++)
                {
                    if (!ShrinkNavigationArea())
                        break;
                    this.UpdateLayout();
                    SetSplitterIncrement();
                    if (_navigationAreaGroups.Count == desiredCount)
                        break;// 
                }//end for- 
            }//end if - must shrink navigation area

            SetSplitterIncrement();

            return _navigationAreaGroups.Count;
        }

        private OutlookBarGroupCollection GetGroupsFromLocation(OutlookBarGroup group)
        {
            switch (group.Location)
            {
                case OutlookBarGroupLocation.NavigationGroupArea:
                    return _navigationAreaGroups;
                case OutlookBarGroupLocation.OverflowArea:
                    return _overflowAreaGroups;
                case OutlookBarGroupLocation.OverflowContextMenu:
                    return _contextMenuGroups;
            }
            return null;
        }

        private void SetNavigationAreaSize(double direction)
        {
            if (_navigationAreaIC == null || _grid == null)
                return;

            int r = (int)_navigationAreaIC.GetValue(Grid.RowProperty);
            RowDefinition rdSelectedGroupContent = _grid.RowDefinitions[r - 2]; // content area row          
            RowDefinition rdNavigationArea = _grid.RowDefinitions[r];            // navigation area row          

            if (Math.Abs(_navigationAreaIC.ActualHeight - rdNavigationArea.ActualHeight) > 0.1)
                _navigationAreaIC.Height = rdNavigationArea.ActualHeight;       // fit to row height

            int deltaGroups = NavigationAreaMaxGroupsResolved - _navigationAreaGroups.Count;
            if (rdSelectedGroupContent.ActualHeight < rdSelectedGroupContent.MinHeight + 0.001)
            {
                if ((direction < 0) && (_navigationAreaGroups.Count > 0))
                    ShrinkNavigationArea();
            }// end if- smaller size
            else if (direction > 0)
            {
                if ((deltaGroups > 0) && (VisibleGroupsCount < Groups.Count))
                    ExtendNavigationArea();
            }// end else- larger size
            if (deltaGroups == 0 && rdNavigationArea.Height.IsStar && _navigationAreaIC.ActualHeight > 0)
            {
                GridLength l = new GridLength(_navigationAreaIC.ActualHeight);
                rdNavigationArea.Height = l;
            }// end if- set grid size
        }

		// JM 10-6-10 Make this internal so we can call it from NavigationPaneOptionsControl.UpdateXamOutlookBar
        //private void VerifySelectedGroup(bool forceSelectedGroup)
        internal void VerifySelectedGroup(bool forceSelectedGroup)
        {
            bool hasSelectedGroup = false;
            foreach(OutlookBarGroup gr in Groups)
            {
                if (gr.IsSelected)
                {
                    if (this.SelectedGroup == null)
                        this.SelectedGroup = gr;
                    else if (gr != this.SelectedGroup)
                        gr.IsSelected = false;
                    hasSelectedGroup = true;
                }//end if- check selected groups to prevent multiple selection
            }
            if (forceSelectedGroup && !hasSelectedGroup && Groups.Count > 0)
                Groups[0].IsSelected = true;
            _selectionInitialized = true;
        }

		// JM 05-05-09 TFS 17313 - Make this internal so we can call it from OutlookBarGroup.OnContentChanged, OutlookBarGroup.OnContentTemplateChanged
		//						   and OutlookBarGroup.OnContentTemplateSelectorChanged.
        //private void UpdateSelectedGroupContent()
        internal void UpdateSelectedGroupContent()
        {
			// JM 03-28-11 TFS70376 - Uncomment the following 2 lines.
			// JM 03-25-11 TFS61189, TFS67362.  Don't do this here.
			if (SelectedGroup != null && _groups.IndexOf(this.SelectedGroup) < 0)
				SelectedGroup = null;

            if (this.SelectedGroup == null)
            {
                SelectedGroupContent = null;
                SelectedGroupContentTemplate = null;
                SelectedGroupContentTemplateSelector = null;
            }
            else
            {
                SelectedGroupContent = SelectedGroup.Content;
                SelectedGroupContentTemplate = SelectedGroup.ContentTemplate;
                SelectedGroupContentTemplateSelector = SelectedGroup.ContentTemplateSelector;
            }
        }

        #endregion //Groups methods	
        
        #region Commands

		// JM 10-10-08 TFS8903
		#region ExecuteCommandGlobalHandler

		private static void ExecuteCommandGlobalHandler(object sender, ExecutedRoutedEventArgs e)
		{
			XamOutlookBar xamOutlookBar = sender as XamOutlookBar;
			if (xamOutlookBar != null)
				xamOutlookBar.ExecuteCommandImpl(e.Command as RoutedCommand, e.Parameter, e.Source, e.OriginalSource);
		}

		#endregion //ExecuteCommandGlobalHandler

		#region ExecuteCommandImpl

		private bool ExecuteCommandImpl(RoutedCommand command, object parameter, object source, object originalSource)
		{
			// Make sure we have a command to execute.
			if (null == command)
				throw new ArgumentNullException("command");

			// Fire the 'before executed' cancelable event.
			ExecutingCommandEventArgs beforeArgs = new ExecutingCommandEventArgs(command);
			if (false == this.RaiseExecutingCommand(beforeArgs))
            {
                // JJD 06/02/10 - TFS33112
                // Return the inverse of ContinueKeyRouting so that the developer can prevent
                // the original key message from bubbling
                //return false;
                return !beforeArgs.ContinueKeyRouting;
            }

			bool handled = false;

			if (command == OutlookBarCommands.GroupMoveDownCommand)
				handled = this.ExecuteGroupMoveDownCommand(source, originalSource, parameter);
			else
			if (command == OutlookBarCommands.GroupMoveUpCommand)
				handled = this.ExecuteGroupMoveUpCommand(source, originalSource, parameter);
			else
			if (command == OutlookBarCommands.SelectGroupCommand)
				handled = this.ExecuteSelectGroupCommand(source, originalSource, parameter);
			else
			if (command == OutlookBarCommands.ShowFewerButtonsCommand)
				handled = this.ExecuteShowFewerButtonsCommand(source, originalSource, parameter);
			else
			if (command == OutlookBarCommands.ShowMoreButtonsCommand)
				handled = this.ExecuteShowMoreButtonsCommand(source, originalSource, parameter);
			else
			if (command == OutlookBarCommands.ShowOptionsCommand)
				handled = this.ExecuteShowOptionsCommand(source, originalSource, parameter);
			else
			if (command == OutlookBarCommands.ShowPopupCommand)
				handled = this.ExecuteShowPopupCommand(source, originalSource, parameter);


			// If the command was executed, fire the 'after executed' event.
			if (handled == true)
				this.RaiseExecutedCommand(new ExecutedCommandEventArgs(command));

			return handled;
		}

		#endregion //ExecuteCommandImpl

		#region ExecutedXXXCommand

		private bool ExecuteGroupMoveDownCommand(object source, object originalSource, object parameter)
        {
			OutlookBarGroup gr = XamOutlookBar.GetGroupForCommand(source, originalSource, parameter);

			if (gr != null && gr.OutlookBar != null)
			{
				gr.OutlookBar.GroupMoveDown(gr);
				return true;
			}
			else
				return false;
        }

		private bool ExecuteGroupMoveUpCommand(object source, object originalSource, object parameter)
        {
			OutlookBarGroup gr = XamOutlookBar.GetGroupForCommand(source, originalSource, parameter);

			if (gr != null && gr.OutlookBar != null)
			{
				gr.OutlookBar.GroupMoveUp(gr);
				return true;
			}
			else
				return false;
        }

		private bool ExecuteShowPopupCommand(object source, object originalSource, object parameter)
        {
            if (this._selectedGroupContent == null)
                return false;

            OutlookBarCancelableRoutedEventArgs args = new OutlookBarCancelableRoutedEventArgs();
            this.RaiseSelectedGroupPopupOpening(args);
            if (!args.Cancel)
            {
                //Popup popup = xamOutlookBar._popUp;

                Popup popup = this._selectedGroupContent.GetTemplateChildHelper("PART_Popup") as Popup;

                if (popup != null)
                {
                    if (popup.IsOpen || this.SelectedGroup==null)
                    {
                        popup.IsOpen = false;
                        return false;
                    }

                    PopupResizerDecorator popDecorator =
                        this._selectedGroupContent.GetTemplateChildHelper("popDec") as PopupResizerDecorator;
					// JM 03-10-09 TFS 11436 - We have changed the name of the 'popDec' element to 'PART_PopupResizerDecorator'
					//						   and added a TemplatePart attribute for this name to the SelectedGroupContent class.  For backward compatibility
					//						   in customer styles we still look for the old name (in the preceeding line), but if null we now look for the new name.
					if (popDecorator == null)
						popDecorator = this._selectedGroupContent.GetTemplateChildHelper("PART_PopupResizerDecorator") as PopupResizerDecorator;

                    FrameworkElement popElement =
                        this._selectedGroupContent.GetTemplateChildHelper("SelectedGroupContentPresenter") as FrameworkElement;
					// JM 03-10-09 TFS 11436 - We have changed the name of the 'SelectedGroupContentPresenter' element to 'PART_SelectedGroupContentPresenter'
					//						   and added a TemplatePart attribute for this name to the SelectedGroupContent class.  For backward compatibility
					//						   in customer styles we still look for the old name (in the preceeding line), but if null we now look for the new name.
					if (popElement == null)
						popElement = this._selectedGroupContent.GetTemplateChildHelper("PART_SelectedGroupContentPresenter") as FrameworkElement;


                    if(popElement==null)
                        return false;

					// JM 03-10-09 TFS 11436 - We don't require that the PopupResizerDecorator be in the template, so enclose the following logic in a null check.
					if (popDecorator != null)
					{
						double popupMaxHeight = this._selectedGroupContent.ActualHeight;
						if (this._navigationAreaIC != null)
							popupMaxHeight += this._navigationAreaIC.ActualHeight;
						if (this._splitter != null)
							popupMaxHeight += this._splitter.ActualHeight;

						popDecorator.MaxHeight = popupMaxHeight;

						// JM 04-21-11 TFS67144 If we don't have a _lastExpandedWidth then measure the _selectedGroupContent and use that Width
						// JM 08-17-10 TFS36556 - Use _lastExpandedWidth if set, otherwise _lastExpandedSelectedGroupSize.Width
						//popDecorator.Width = this._lastExpandedSelectedGroupSize.Width;
						//popDecorator.Width = (false == double.IsNaN(this._lastExpandedWidth)) ? this._lastExpandedWidth 
						//                                                                      : this._lastExpandedSelectedGroupSize.Width;
						double width = this._lastExpandedWidth;
						if (double.IsNaN(width))
						{
							this._selectedGroupContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
							width = this._selectedGroupContent.DesiredSize.Width;
						}
						popDecorator.Width = (false == double.IsNaN(width)) ? this._lastExpandedWidth
																			: this._lastExpandedSelectedGroupSize.Width;

						PopupResizerDecorator.Constraints constraints =
							PopupResizerDecorator.GetResizeConstraints(popElement);
						if (constraints == null)
							constraints = new PopupResizerDecorator.Constraints();

						// horizontal only
						constraints.MinimumHeight = Math.Min(this._selectedGroupContent.ActualHeight, popDecorator.MaxHeight);
						constraints.MaximumHeight = popDecorator.MaxHeight;

						// width
						constraints.MinimumWidth = 2 * this.minExpandedWidth;
						constraints.MaximumWidth = this.MaxWidth;

						PopupResizerDecorator.SetResizeConstraints(popElement, constraints);
					}

                    popup.Closed += new EventHandler(this.Popup_Closed);

					// AS 11/16/11 TFS79544
					// Temporarily remove the Content as a logical child of the selected group. What is happening 
					// is that the logical parent is within a tree that is loaded so it's IsLoaded remains true 
					// but it is being reparented into a visual tree where the visual parent's IsLoaded is false. 
					// For some reason when the IsLoaded of the ancestor is changed to true, the IsLoaded of the 
					// content of the group is being changed to false. To avoid this we'll temporarily pull the 
					// content of the group out of the logical tree while the popup is opening.
					//
					var gr = this.SelectedGroup;

					try
					{
						if (null != gr)
							gr.SuppressLogicalContent = true;

						popup.IsOpen = true;
					}
					finally
					{
						if (null != gr)
							gr.SuppressLogicalContent = false;
					}

                    this.RaiseSelectedGroupPopupOpened(new RoutedEventArgs());

					return true;
                }
            }//end if - popup opening is not canceled

            return false;
        }

        void Popup_Closed(object sender, EventArgs e)
        {
            Popup popup = sender as Popup;
            if (popup != null)
                popup.Closed -= Popup_Closed;

            OutlookBarGroup gr = this.SelectedGroup;

            if (gr == null)
                return;

            object o = gr.Content;
            gr.Content = null;
            UpdateSelectedGroupContent();

            gr.Content = o;
            UpdateSelectedGroupContent();

            RaiseSelectedGroupPopupClosed(new RoutedEventArgs());
        }

		private bool ExecuteShowFewerButtonsCommand(object source, object originalSource, object parameter)
        {
			if (this.NavigationAreaGroups.Count > 0)
			{
				this.NavigationAreaMaxGroups = this.NavigationAreaGroups.Count - 1;
				return true;
			}
			else
				return false;
        }

		private bool ExecuteShowMoreButtonsCommand(object source, object originalSource, object parameter)
        {
            this.NavigationAreaMaxGroups = this.NavigationAreaGroups.Count + 1;
			return true;
        }

		private bool ExecuteShowOptionsCommand(object source, object originalSource, object parameter)
        {
			ToolWindow						dlg				= new ToolWindow();
			FrameworkElement				feOwner			= this;
			NavigationPaneOptionsControl	npoc			= new NavigationPaneOptionsControl();

			npoc.OutlookBar = this;
			dlg.Content		= npoc;

			if (BrowserInteropHelper.IsBrowserHosted == false)
			{
				Window				owner = null;
				DependencyObject	o;

				for (o = VisualTreeHelper.GetParent(this); o != null; o = VisualTreeHelper.GetParent(o))
				{
					if (o is Window)
						break;
				}
				owner = o as Window;

				if (owner != null)
					feOwner = owner;
			}

			dlg.VerticalAlignmentMode	= ToolWindowAlignmentMode.UseAlignment;
			dlg.VerticalAlignment		= VerticalAlignment.Center;
			dlg.HorizontalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;
			dlg.HorizontalAlignment		= HorizontalAlignment.Center;
			dlg.Title					= GetString("NavigationPaneOptionsDialogTitle");

			// JM 04-26-11 TFS64509
			dlg.ResizeMode				= ResizeMode.NoResize;

			// AS 12/10/10
			// Add in the control's resources so any theming will affect the options window.
			//
			//dlg.ShowDialog(feOwner, null);
			dlg.Resources.MergedDictionaries.Add(this.Resources);
			dlg.ShowDialog(feOwner, this.ShowDialogCallback);

			return true;
        }

		// AS 12/10/10
		private void ShowDialogCallback( ToolWindow window, bool? dialogResult )
		{
			window.Resources.MergedDictionaries.Remove(this.Resources);
		}

		private bool ExecuteSelectGroupCommand(object source, object originalSource, object parameter)
        {
			OutlookBarGroup gr = XamOutlookBar.GetGroupForCommand(source, originalSource, parameter);
			gr.IsSelected = true;
			return true;
        }

        #endregion //ExecutedXXXCommand	

        #region CanExecuteCommand

        private static void CanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            XamOutlookBar xamOutlookBar = sender as XamOutlookBar;

			e.CanExecute = false;

            if (xamOutlookBar != null)
            {
                if (e.Command == OutlookBarCommands.ShowMoreButtonsCommand)
                {
                    e.CanExecute = (xamOutlookBar._navigationAreaGroups.Count < xamOutlookBar.VisibleGroupsCount)
                        && xamOutlookBar._splitter != null;
                }
				else
                if (e.Command == OutlookBarCommands.ShowFewerButtonsCommand)
                {
                    e.CanExecute = xamOutlookBar._navigationAreaGroups.Count > 0
                        && xamOutlookBar._splitter != null;
                }
				else
				if (e.Command == OutlookBarCommands.ShowPopupCommand && xamOutlookBar._selectedGroupContent != null)
                {
                    Popup popup = xamOutlookBar._selectedGroupContent.GetTemplateChildHelper("PART_Popup") as Popup;
                    if (popup != null)
                    {
                        e.CanExecute = 
                            xamOutlookBar.IsMinimized &&
                            !popup.IsOpen &&
                            xamOutlookBar.SelectedGroup != null;
                    }
					else
						e.CanExecute = false;
                }
				else
				if (e.Command == OutlookBarCommands.ShowOptionsCommand)
                {
                    e.CanExecute = true;
                }
				else
				if (e.Command == OutlookBarCommands.GroupMoveDownCommand)
                {
					OutlookBarGroup gr = XamOutlookBar.GetGroupForCommand(e.Source, e.OriginalSource, e.Parameter);
					if (gr == null)
                        e.CanExecute = false;
                    else
                        e.CanExecute = xamOutlookBar.Groups.IndexOf(gr) < xamOutlookBar.Groups.Count - 1;
                }
				else
				if (e.Command == OutlookBarCommands.GroupMoveUpCommand)
                {
					OutlookBarGroup gr = XamOutlookBar.GetGroupForCommand(e.Source, e.OriginalSource, e.Parameter);
					if (gr == null)
                        e.CanExecute = false;
                    else
                        e.CanExecute = xamOutlookBar.Groups.IndexOf(gr) > 0;
                }
				else
				if (e.Command == OutlookBarCommands.SelectGroupCommand)
				{
					OutlookBarGroup gr = XamOutlookBar.GetGroupForCommand(e.Source, e.OriginalSource, e.Parameter);
					e.CanExecute = (gr != null && gr.IsSelected == false);
						
				}
			}//end if - sender is XamOutlookBar
        }

        #endregion //CanExecuteCommand	
        
		// JM 10-10-08 TFS8903
		#region GetGroupForCommand
		private static OutlookBarGroup GetGroupForCommand(object source, object originalSource, object parameter)
		{
            OutlookBarGroup gr = parameter as OutlookBarGroup;
			if (gr == null)
			{
				gr = source as OutlookBarGroup;
				if (gr == null)
					gr = originalSource as OutlookBarGroup;
			}

			return gr;
		}
		#endregion //GetGroupForCommand

        #endregion //Commands	
    
        #endregion //Private Methods

        #endregion //Methods

        #region Events

        #region Public Events

		// JM 10-10-08 TFS8903
		#region ExecutingCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutingCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		public static readonly RoutedEvent ExecutingCommandEvent =
			EventManager.RegisterRoutedEvent("ExecutingCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutingCommandEventArgs>), typeof(XamOutlookBar));

		/// <summary>
		/// Occurs before a command is performed
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		protected virtual void OnExecutingCommand(ExecutingCommandEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal bool RaiseExecutingCommand(ExecutingCommandEventArgs args)
		{
			args.RoutedEvent = XamOutlookBar.ExecutingCommandEvent;
			args.Source = this;
			this.OnExecutingCommand(args);

			return args.Cancel == false;
		}

		/// <summary>
		/// Occurs before a command is performed
		/// </summary>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		//[Description("Occurs before a command is performed")]
		//[Category("OutlookBar Properties")] // Behavior
		public event EventHandler<ExecutingCommandEventArgs> ExecutingCommand
		{
			add
			{
				base.AddHandler(XamOutlookBar.ExecutingCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamOutlookBar.ExecutingCommandEvent, value);
			}
		}

		#endregion //ExecutingCommand

		// JM 10-10-08 TFS8903
		#region ExecutedCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutedCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		public static readonly RoutedEvent ExecutedCommandEvent =
			EventManager.RegisterRoutedEvent("ExecutedCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutedCommandEventArgs>), typeof(XamOutlookBar));

		/// <summary>
		/// Occurs after a command is performed
		/// </summary>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		protected virtual void OnExecutedCommand(ExecutedCommandEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseExecutedCommand(ExecutedCommandEventArgs args)
		{
			args.RoutedEvent = XamOutlookBar.ExecutedCommandEvent;
			args.Source = this;
			this.OnExecutedCommand(args);
		}

		/// <summary>
		/// Occurs after a command is performed
		/// </summary>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		//[Description("Occurs after a command is performed")]
		//[Category("OutlookBar Properties")] // Behavior
		public event EventHandler<ExecutedCommandEventArgs> ExecutedCommand
		{
			add
			{
				base.AddHandler(XamOutlookBar.ExecutedCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamOutlookBar.ExecutedCommandEvent, value);
			}
		}

		#endregion //ExecutedCommand

        #region GroupsReset

        /// <summary>
        /// Event ID for the <see cref="GroupsReset"/> routed event
        /// </summary>
        /// <seealso cref="GroupsReset"/>
        /// <seealso cref="OnGroupsReset"/>
        public static readonly RoutedEvent GroupsResetEvent =
            EventManager.RegisterRoutedEvent("GroupsReset", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamOutlookBar));

        /// <summary>
        /// Raises the <seealso cref="GroupsResetEvent"/>
        /// </summary>
        /// <param name="args">The RoutedEventArgs that contains the event data</param>
        /// <seealso cref="GroupsReset"/>
        /// <seealso cref="GroupsResetEvent"/>
        protected virtual void OnGroupsReset(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseGroupsReset(RoutedEventArgs args)
        {
            args.RoutedEvent = XamOutlookBar.GroupsResetEvent;
            args.Source = this;
            this.OnGroupsReset(args);
        }

        /// <summary>
        /// Occurs after reseting the visibility and the order of the groups in their initial state by the <see cref="NavigationPaneOptionsControl"/>
        /// </summary>
        /// <seealso cref="OnGroupsReset"/>
        /// <seealso cref="GroupsResetEvent"/>
        //[Description("Occurs after reseting the visibility and the order of the groups in their initial state by the NavigationPaneOptionsControl")]
        //[Category("OutlookBar Properties")] // Behavior
        public event RoutedEventHandler GroupsReset
        {
            add
            {
                base.AddHandler(XamOutlookBar.GroupsResetEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.GroupsResetEvent, value);
            }
        }

        #endregion //GroupsReset

        #region NavigationPaneExpanding
        /// <summary>
        /// Event ID for the <see cref="NavigationPaneExpanding"/> routed event
        /// </summary>
        /// <seealso cref="NavigationPaneExpanding"/>
        /// <seealso cref="OnNavigationPaneExpanding"/>
        public static readonly RoutedEvent NavigationPaneExpandingEvent =
            EventManager.RegisterRoutedEvent("NavigationPaneExpanding", RoutingStrategy.Bubble,
            typeof(EventHandler<OutlookBarCancelableRoutedEventArgs>), typeof(XamOutlookBar));
        
        /// <summary>
        /// Occurs before <see cref="XamOutlookBar"/> has been restored to a normal(not minimized) state
        /// </summary>
        /// <param name="args">The OutlookBarCancelableRoutedEventArgs that contains the event data</param>
        protected virtual void OnNavigationPaneExpanding(OutlookBarCancelableRoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseNavigationPaneExpanding(OutlookBarCancelableRoutedEventArgs args)
        {
            args.RoutedEvent = XamOutlookBar.NavigationPaneExpandingEvent;
            args.Source = this;
            this.OnNavigationPaneExpanding(args);
        }

        /// <summary>
        /// Raises before completing the <see cref="XamOutlookBar"/> restoration to normal (not minimized) state
        /// </summary>
        //[Description("Raises before completing the XamOutlookBar restoration to normal (not minimized) state")]
        //[Category("OutlookBar Properties")] // Behavior
        public event EventHandler<OutlookBarCancelableRoutedEventArgs> NavigationPaneExpanding
        {
            add
            {
                base.AddHandler(XamOutlookBar.NavigationPaneExpandingEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.NavigationPaneExpandingEvent, value);
            }
        }

        #endregion //NavigationPaneExpanding

        #region NavigationPaneExpanded

        /// <summary>
        /// Event ID for the <see cref="NavigationPaneExpanded"/> routed event
        /// </summary>
        /// <seealso cref="NavigationPaneExpanded"/>
        /// <seealso cref="OnNavigationPaneExpanded"/>
        public static readonly RoutedEvent NavigationPaneExpandedEvent =
            EventManager.RegisterRoutedEvent("NavigationPaneExpanded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamOutlookBar));

        /// <summary>
        /// Raises the <see cref="NavigationPaneExpandedEvent"/>
        /// </summary>
        /// <param name="args">The RoutedEventArgs that contains the event data</param>
        /// <seealso cref="NavigationPaneExpanded"/>
        /// <seealso cref="NavigationPaneExpandedEvent"/>
        protected virtual void OnNavigationPaneExpanded(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseNavigationPaneExpanded(RoutedEventArgs args)
        {
            args.RoutedEvent = XamOutlookBar.NavigationPaneExpandedEvent;
            args.Source = this;
            this.OnNavigationPaneExpanded(args);
        }

        /// <summary>
        /// Raises after completing the XamOutlookBar expansion.
        /// </summary>
        /// <seealso cref="OnNavigationPaneExpanded"/>
        /// <seealso cref="NavigationPaneExpandedEvent"/>
        //[Description("Raises after completing the XamOutlookBar expansion")]
        //[Category("OutlookBar Properties")] // Behavior
        public event RoutedEventHandler NavigationPaneExpanded
        {
            add
            {
                base.AddHandler(XamOutlookBar.NavigationPaneExpandedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.NavigationPaneExpandedEvent, value);
            }
        }

        #endregion //NavigationPaneExpanded

        #region NavigationPaneMinimizing
        /// <summary>
        /// Event ID for the <see cref="NavigationPaneMinimizing"/> routed event
        /// </summary>
        /// <seealso cref="NavigationPaneMinimizing"/>
        /// <seealso cref="OnNavigationPaneMinimizing"/>
        public static readonly RoutedEvent NavigationPaneMinimizingEvent =
            EventManager.RegisterRoutedEvent("NavigationPaneMinimizing", RoutingStrategy.Bubble,
            typeof(EventHandler<OutlookBarCancelableRoutedEventArgs>), typeof(XamOutlookBar));

        /// <summary>
        /// Raises the <see cref="NavigationPaneMinimizingEvent"/>
        /// </summary>
        /// <param name="args">The OutlookBarCancelableRoutedEventArgs that contains the event data</param>
        protected virtual void OnNavigationPaneMinimizing(OutlookBarCancelableRoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseNavigationPaneMinimizing(OutlookBarCancelableRoutedEventArgs args)
        {
            args.RoutedEvent = XamOutlookBar.NavigationPaneMinimizingEvent;
            args.Source = this;
            this.OnNavigationPaneMinimizing(args);
        }

        /// <summary>
        /// Raises before completing the <see cref="XamOutlookBar"/> minimization 
        /// </summary>
        //[Description("Raises before completing the XamOutlookBar minimization ")]
        //[Category("OutlookBar Properties")] // Behavior
        public event EventHandler<OutlookBarCancelableRoutedEventArgs> NavigationPaneMinimizing
        {
            add
            {
                base.AddHandler(XamOutlookBar.NavigationPaneMinimizingEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.NavigationPaneMinimizingEvent, value);
            }
        }

        #endregion //NavigationPaneMinimizing

        #region NavigationPaneMinimized

        /// <summary>
        /// Event ID for the <see cref="NavigationPaneMinimized"/> routed event
        /// </summary>
        /// <seealso cref="NavigationPaneMinimized"/>
        /// <seealso cref="OnNavigationPaneMinimized"/>
       public static readonly RoutedEvent NavigationPaneMinimizedEvent =
            EventManager.RegisterRoutedEvent("NavigationPaneMinimized", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamOutlookBar));

        /// <summary>
        /// Used to raise the <see cref="NavigationPaneMinimizedEvent"/> event
        /// </summary>
        /// <seealso cref="NavigationPaneMinimized"/>
        /// <seealso cref="NavigationPaneMinimizedEvent"/>
         protected virtual void OnNavigationPaneMinimized(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseNavigationPaneMinimized(RoutedEventArgs args)
        {
            args.RoutedEvent = XamOutlookBar.NavigationPaneMinimizedEvent;
            args.Source = this;
            this.OnNavigationPaneMinimized(args);
        }

        /// <summary>
        /// Occurs after <see cref="XamOutlookBar"/> has been minimized
        /// </summary>
        /// <seealso cref="OnNavigationPaneMinimized"/>
        /// <seealso cref="NavigationPaneMinimizedEvent"/>
        //[Description("Occurs after XamOutlookBar has been minimized")]
        //[Category("OutlookBar Properties")] // Behavior
        public event RoutedEventHandler NavigationPaneMinimized
        {
            add
            {
                base.AddHandler(XamOutlookBar.NavigationPaneMinimizedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.NavigationPaneMinimizedEvent, value);
            }
        }

        #endregion //NavigationPaneMinimized

        #region SelectedGroupChanging

        /// <summary>
        /// Event ID for the <see cref="SelectedGroupChanging"/> routed event
        /// </summary>
        /// <seealso cref="SelectedGroupChanging"/>
        /// <seealso cref="OnSelectedGroupChanging"/>
        public static readonly RoutedEvent SelectedGroupChangingEvent =
            EventManager.RegisterRoutedEvent("SelectedGroupChanging", RoutingStrategy.Bubble,
            typeof(EventHandler<SelectedGroupChangingEventArgs>), typeof(XamOutlookBar));

        /// <summary>
        /// Used to raise the <see cref="SelectedGroupChangingEvent"/> event
        /// </summary>
        /// <param name="args">The SelectedGroupChangingEventArgs that contains the event data</param>
        protected virtual void OnSelectedGroupChanging(SelectedGroupChangingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseSelectedGroupChanging(SelectedGroupChangingEventArgs args)
        {
            args.RoutedEvent = XamOutlookBar.SelectedGroupChangingEvent;
            args.Source = this;
            this.OnSelectedGroupChanging(args);
        }

        /// <summary>
        /// Raises before completing the change of the <see cref="SelectedGroup"/> in the XamOutlookBar.
        /// </summary>
        //[Description("Raises before completing the change of the SelectedGroup in the XamOutlookBar")]
        //[Category("OutlookBar Properties")] // Behavior
        public event EventHandler<SelectedGroupChangingEventArgs> SelectedGroupChanging
        {
            add
            {
                base.AddHandler(XamOutlookBar.SelectedGroupChangingEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.SelectedGroupChangingEvent, value);
            }
        }

        #endregion //SelectedGroupChanging

        #region SelectedGroupChanged

        /// <summary>
        /// Event ID for the <see cref="SelectedGroupChanged"/> routed event
        /// </summary>
        /// <seealso cref="SelectedGroupChanged"/>
        /// <seealso cref="OnSelectedGroupChanged"/>
        public static readonly RoutedEvent SelectedGroupChangedEvent =
            EventManager.RegisterRoutedEvent("SelectedGroupChanged", RoutingStrategy.Bubble,
            typeof(EventHandler<SelectedGroupChangedEventArgs>), typeof(XamOutlookBar));

        /// <summary>
        /// Used to raise the <see cref="SelectedGroupChangedEvent"/> event
        /// </summary>
        /// <param name="args">The SelectedGroupChangedEventArgs that contains the event data</param>
        protected virtual void OnSelectedGroupChanged(SelectedGroupChangedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseSelectedGroupChanged(SelectedGroupChangedEventArgs args)
        {
            args.RoutedEvent = XamOutlookBar.SelectedGroupChangedEvent;
            args.Source = this;
            this.OnSelectedGroupChanged(args);
        }

        /// <summary>
        /// Occurs after <see cref="SelectedGroup"/> has been changed
        /// </summary>
        //[Description("Occurs after SelectedGroup has been changed")]
        //[Category("OutlookBar Properties")] // Behavior
        public event RoutedEventHandler SelectedGroupChanged
        {
            add
            {
                base.AddHandler(XamOutlookBar.SelectedGroupChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.SelectedGroupChangedEvent, value);
            }
        }

        #endregion //SelectedGroupChanged

        #region SelectedGroupPopupClosed

        /// <summary>
        /// Event ID for the <see cref="SelectedGroupPopupClosed"/> routed event
        /// </summary>
        /// <seealso cref="SelectedGroupPopupClosed"/>
        /// <seealso cref="OnSelectedGroupPopupClosed"/>
        public static readonly RoutedEvent SelectedGroupPopupClosedEvent =
            EventManager.RegisterRoutedEvent("SelectedGroupPopupClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamOutlookBar));

        /// <summary>
        /// Used to raise the <see cref="SelectedGroupPopupClosed"/> event
        /// </summary>
        /// <seealso cref="SelectedGroupPopupClosed"/>
        /// <seealso cref="SelectedGroupPopupClosedEvent"/>
        protected virtual void OnSelectedGroupPopupClosed(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseSelectedGroupPopupClosed(RoutedEventArgs args)
        {
            args.RoutedEvent = XamOutlookBar.SelectedGroupPopupClosedEvent;
            args.Source = this;
            this.OnSelectedGroupPopupClosed(args);
        }

        /// <summary>
        /// Occurs when the <see cref="IsMinimized"/> property of the <see cref="XamOutlookBar"/> is true and the popup for the <see cref="SelectedGroup"/> has closed up.
        /// </summary>
        /// <seealso cref="OnSelectedGroupPopupClosed"/>
        /// <seealso cref="SelectedGroupPopupClosedEvent"/>
        //[Description("Occurs when the IsMinimized property of the XamOutlookBar is true and the popup for the SelectedGroup has closed up.")]
        //[Category("OutlookBar Properties")] // Behavior
        public event RoutedEventHandler SelectedGroupPopupClosed
        {
            add
            {
                base.AddHandler(XamOutlookBar.SelectedGroupPopupClosedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.SelectedGroupPopupClosedEvent, value);
            }
        }

        #endregion //SelectedGroupPopupClosed
        
        #region SelectedGroupPopupOpened

        /// <summary>
        /// Event ID for the <see cref="SelectedGroupPopupOpened"/> routed event
        /// </summary>
        /// <seealso cref="SelectedGroupPopupOpened"/>
        /// <seealso cref="OnSelectedGroupPopupOpened"/>
        public static readonly RoutedEvent SelectedGroupPopupOpenedEvent =
            EventManager.RegisterRoutedEvent("SelectedGroupPopupOpened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamOutlookBar));

        /// <summary>
        /// Used to raise the <see cref="SelectedGroupPopupOpened"/> event
        /// </summary>
        /// <seealso cref="SelectedGroupPopupOpened"/>
        /// <seealso cref="SelectedGroupPopupOpenedEvent"/>
        protected virtual void OnSelectedGroupPopupOpened(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseSelectedGroupPopupOpened(RoutedEventArgs args)
        {
            args.RoutedEvent = XamOutlookBar.SelectedGroupPopupOpenedEvent;
            args.Source = this;
            this.OnSelectedGroupPopupOpened(args);
        }

        /// <summary>
        /// Occurs when the <see cref="IsMinimized"/> property of the <see cref="XamOutlookBar"/> is true and the popup for the <see cref="SelectedGroup"/> has been opened.
        /// </summary>
        /// <seealso cref="OnSelectedGroupPopupOpened"/>
        /// <seealso cref="SelectedGroupPopupOpenedEvent"/>
        //[Description("Occurs when the IsMinimized property of the XamOutlookBar is true and the popup for the SelectedGroup has been opened.")]
        //[Category("OutlookBar Properties")] // Behavior
        public event RoutedEventHandler SelectedGroupPopupOpened
        {
            add
            {
                base.AddHandler(XamOutlookBar.SelectedGroupPopupOpenedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.SelectedGroupPopupOpenedEvent, value);
            }
        }

        #endregion //SelectedGroupPopupOpened

        #region SelectedGroupPopupOpening

        /// <summary>
        /// Event ID for the <see cref="SelectedGroupPopupOpening"/> routed event
        /// </summary>
        /// <seealso cref="SelectedGroupPopupOpening"/>
        /// <seealso cref="OnSelectedGroupPopupOpening"/>
        public static readonly RoutedEvent SelectedGroupPopupOpeningEvent =
            EventManager.RegisterRoutedEvent("SelectedGroupPopupOpening", RoutingStrategy.Bubble, 
            typeof(EventHandler<OutlookBarCancelableRoutedEventArgs>), typeof(XamOutlookBar));

        /// <summary>
        /// Used to raise the <see cref="SelectedGroupPopupOpening"/> event
        /// </summary>
        /// <seealso cref="SelectedGroupPopupOpening"/>
        /// <seealso cref="SelectedGroupPopupOpeningEvent"/>
        protected virtual void OnSelectedGroupPopupOpening(OutlookBarCancelableRoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseSelectedGroupPopupOpening(OutlookBarCancelableRoutedEventArgs args)
        {
            args.RoutedEvent = XamOutlookBar.SelectedGroupPopupOpeningEvent;
            args.Source = this;
            this.OnSelectedGroupPopupOpening(args);
        }

        /// <summary>
        /// Occurs when the popup is about to be displayed to the end user.
        /// </summary>
        /// <seealso cref="OnSelectedGroupPopupOpening"/>
        /// <seealso cref="SelectedGroupPopupOpeningEvent"/>
        //[Description("Occurs when the popup is about to be displayed to the end user.")]
        //[Category("OutlookBar Properties")] // Behavior
        public event EventHandler<OutlookBarCancelableRoutedEventArgs> SelectedGroupPopupOpening
        {
            add
            {
                base.AddHandler(XamOutlookBar.SelectedGroupPopupOpeningEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.SelectedGroupPopupOpeningEvent, value);
            }
        }

        #endregion //SelectedGroupPopupOpening

        #region ThemeChanged


        /// <summary>
        /// Routed event used to notify when the <see cref="Theme"/> property has been changed.
        /// </summary>
        public static readonly RoutedEvent ThemeChangedEvent = 
            ThemeManager.ThemeChangedEvent.AddOwner(typeof(XamOutlookBar));

		// AS 8/16/10 TFS35781
		private bool ShouldUpdateLayoutOnThemeChange()
		{
			DependencyObject descendant = this;
			DependencyObject previousDescendant = null;

			while (descendant != null)
			{
				if (descendant is Page)
					return true;

				if (descendant is Window)
					return true;

				previousDescendant = descendant;
				descendant = Utilities.GetParent(descendant, false) ?? Utilities.GetParent(descendant, true);
			}

			var window = Window.GetWindow(this);

			// if we walked up the visual tree and didn't hit a window
			// but this had a window associated with it then it must be 
			// out of the visual tree
			if (window != null)
				return false;

			return true;
		}

		private static void OnThemeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamOutlookBar control = target as XamOutlookBar;
            if (control.IsInitialized)
            {
                control.InvalidateMeasure();

				// AS 8/16/10 TFS35781
				// Added if block. Calling UpdateLayout is causing the containing 
				// ancestor contentpresenter (the PART_SelectedContentHost of the 
				// ancestor TabGroupPane) to add the Content as a visual child 
				// during the ApplyTemplate call resulting from the UpdateLayout 
				// call but then the OnApplyTemplate impl of ContentPresenter tries 
				// to remove the old TemplateChild as its visual child and add the 
				// the Content as its visual child. That blows up because the CP 
				// already did this erroneously.
				//
				if (control.ShouldUpdateLayoutOnThemeChange())
					control.UpdateLayout();
            }
            control.OnThemeChanged((string)(e.OldValue), (string)(e.NewValue));
        }
        /// <summary>
        /// Used to raise the <see cref="ThemeChanged"/> event.
        /// </summary>
        protected virtual void OnThemeChanged(string previousValue, string currentValue)
        {
            RoutedPropertyChangedEventArgs<string> newEvent = new RoutedPropertyChangedEventArgs<string>(previousValue, currentValue);
            newEvent.RoutedEvent = XamOutlookBar.ThemeChangedEvent;
            newEvent.Source = this;
            this.RaiseEvent(newEvent);
        }

        /// <summary>
        /// Occurs when the <see cref="Theme"/> property has been changed.
        /// </summary>
        //[Description("Occurs when the 'Theme' property has been changed.")]
        //[Category("OutlookBar Properties")] // Behavior
        public event RoutedPropertyChangedEventHandler<string> ThemeChanged
        {
            add
            {
                base.AddHandler(XamOutlookBar.ThemeChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamOutlookBar.ThemeChangedEvent, value);
            }
        }

        #endregion //ThemeChanged

        #endregion //Public Events

        #endregion //Events

        #region Commands
        // see OutlookBarCommands
        #endregion //Commands

        #region SplitterPreviewAdorner Class
        private class SplitterPreviewAdorner : Adorner
        {
            #region Member Variables

            private UIElement _element;
            private TranslateTransform _transform;

            #endregion //Member Variables

            #region Constructor
            public SplitterPreviewAdorner(Thumb splitter)
                : base(splitter)
            {
                // create the control that will render the preview

                VerticalSplitterPreview ctrl = new VerticalSplitterPreview();
                ctrl.SetResourceReference(FrameworkElement.StyleProperty, XamOutlookBar.VerticalSplitterPreviewStyleKey);
                ctrl.IsEnabled = false;

                Decorator decorator = new Decorator();
                decorator.Child = ctrl;
                this._element = decorator;

                this._transform = new TranslateTransform();
                this._element.RenderTransform = this._transform;

                this.AddVisualChild(this._element);
            }
            #endregion //Constructor

            #region Properties
            public double OffsetX
            {
                get { return this._transform.X; }
                set { this._transform.X = value; }
            }

            public double OffsetY
            {
                get { return this._transform.Y; }
                set { this._transform.Y = value; }
            }
            #endregion //Properties

            #region Base class overrides
            protected override int VisualChildrenCount
            {
                get
                {
                    return 1;
                }
            }

            protected override Visual GetVisualChild(int index)
            {
                if (index == 0)
                    return this._element;

                return base.GetVisualChild(index);
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                this._element.Arrange(new Rect(finalSize));
                return finalSize;
            }
            #endregion //Base class overrides
        }
        #endregion //SplitterPreviewAdorner

        #region GroupsItemsSource Class

        private class GroupsItemsSource : ItemsControl
        {
            #region Members

            private OutlookBarGroupCollection _groups;

            #endregion //Members	
    
            #region Constructors

            public GroupsItemsSource(OutlookBarGroupCollection groups)
            {
                _groups = groups;
            }

            #endregion //Constructors	
    
            #region Base Class Overrides

            #region OnItemsChanged

            protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
            {
                try
                {
                    _groups.CanEdit = true; // enables direct editing of groups
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            for (int i = 0; i < e.NewItems.Count; i++)
                            {
                                object o = e.NewItems[i];
								if (o != null)
								{
									_groups.Insert(i + e.NewStartingIndex, CreateOutlookBarGroup(o));

									// JM 01-11-11 TFS61189
									RebuildAllGroups();
								}
							}
                            break;
                        case NotifyCollectionChangedAction.Move:
                            RebuildAllGroups();
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                OutlookBarGroup gr = e.OldItems[i] as OutlookBarGroup;
                                if (gr == null)
                                {
                                    string key = (i + e.OldStartingIndex).ToString();
                                    gr = _groups[key];
                                }
                                if (gr != null)
                                    _groups.Remove(gr);

								// JM 06-29-10 TFS32783
								RebuildAllGroups();
							}
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            RebuildAllGroups();
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            RebuildAllGroups();
                            break;
                    }
                }
                finally
                {
                    _groups.CanEdit = false;
                }

                base.OnItemsChanged(e);
            }

            #endregion //OnItemsChanged	
    
            #region OnItemsSourceChanged

            protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
            {
                base.OnItemsSourceChanged(oldValue, newValue);
            }

            #endregion //OnItemsSourceChanged	
    
            #endregion //Base Class Overrides

            #region Methods

            #region Private Methods

            internal void RebuildAllGroups()
            {
				// JM 03-25-11 TFS61189, TFS67362.  Only rebuild if necessary
				bool rebuildNeeded = false;

				if (_groups.Count != this.Items.Count)
					rebuildNeeded = true;
				else
				{
					for (int i = 0; i < this.Items.Count; i++)
					{
						if (this.Items[i] != _groups[i].DataContext)
						{
							rebuildNeeded = true;
							break;
						}
					}
				}

				if (rebuildNeeded)
				{
					_groups.Clear();
					foreach (object o in this.Items)
					{
						if (o != null)
							_groups.Add(CreateOutlookBarGroup(o));
					}
				}
            }

            private OutlookBarGroup CreateOutlookBarGroup(object o)
            {
                if (o is OutlookBarGroup)
                    return o as OutlookBarGroup;

                OutlookBarGroup gr = new OutlookBarGroup();

                gr.DataContext = o;

                gr.Key = Items.IndexOf(o).ToString();
                
                return gr;
            }

            #endregion //Private Methods	
    
            #endregion //Methods
        }

        #endregion //GroupsItemsSource class
	
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