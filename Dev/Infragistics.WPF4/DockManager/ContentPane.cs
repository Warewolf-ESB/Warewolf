using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows.Media;
using System.Windows.Input;
using Infragistics.Shared;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.DockManager.Events;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DockManager.Dragging;
using Infragistics.Windows.Commands;
using System.Windows.Interop;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.DockManager;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Represents a single pane in a <see cref="XamDockManager"/>. The <see cref="ContentControl.Content"/> should be the 
	/// control/element that should be hosted in the dockable window and the <see cref="HeaderedContentControl.Header"/> is 
	/// used to display in the caption for the pane.
	/// </summary>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,        GroupName = VisualStateUtilities.GroupCommon )]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,          GroupName = VisualStateUtilities.GroupCommon )]

    [TemplateVisualState(Name = VisualStateUtilities.StateDockedBottom,    GroupName = VisualStateUtilities.GroupPaneLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateDockedLeft,      GroupName = VisualStateUtilities.GroupPaneLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateDockedRight,     GroupName = VisualStateUtilities.GroupPaneLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateDockedTop,       GroupName = VisualStateUtilities.GroupPaneLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateDockedBottom,    GroupName = VisualStateUtilities.GroupPaneLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateDocument,        GroupName = VisualStateUtilities.GroupPaneLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateFloating,        GroupName = VisualStateUtilities.GroupPaneLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnpinned,        GroupName = VisualStateUtilities.GroupPaneLocation )]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateActive,          GroupName = VisualStateUtilities.GroupActive )]
    [TemplateVisualState(Name = VisualStateUtilities.StateActiveDocument,  GroupName = VisualStateUtilities.GroupActive )]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,        GroupName = VisualStateUtilities.GroupActive )]


    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class ContentPane : HeaderedContentControl
		, ICommandHost
        // AS 3/30/09 TFS16355 - WinForms Interop
        , IHwndHostInfoOwner
	{
		#region Member Variables

		private readonly PanePlacementInfo _placementInfo;
		private FocusWatcher _mouseDownFocusWatcher;
		private Size _lastFloatingSize = Size.Empty;
		private Point? _lastFloatingLocation = null;
		private Rect _lastFloatingWindowRect = Rect.Empty;
		private DateTime _lastActivatedTime = DateTime.MinValue;
		private ContentPaneCommands _commands;
		private bool _suppressFlyoutDuringActivate;

        // AS 3/30/09 TFS16355 - WinForms Interop
        // Added anti-recursion to the Activate method since that could lead
        // to an exception within the Focus call.
        //
        private bool _isActivating;
        
        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        // Keep track of the paneHeader so we can let it know to Update its vsiual state when the
        // pane's IsActive or IsActiveDocument properties change
        private WeakReference _paneHeaderPresenter;


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


        #endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ContentPane"/>
		/// </summary>
		public ContentPane()
		{
			this._placementInfo = new PanePlacementInfo();
			this._commands = ContentPaneCommands.Instance;

            // AS 3/30/09 TFS16355 - WinForms Interop
            //// AS 10/13/08 TFS6032
            //this.SetValue(HwndHostProperty, new HwndHostInfo(this));
            HwndHostInfo.SetHwndHost(this, new HwndHostInfo(this));
		}

		static ContentPane()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(typeof(ContentPane)));
			ContentControl.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			ContentControl.VerticalContentAlignmentProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox));

			EventManager.RegisterClassHandler(typeof(ContentPane), Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnPreviousLostKeyboardFocus));

			EventManager.RegisterClassHandler(typeof(ContentPane), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
			EventManager.RegisterClassHandler(typeof(ContentPane), Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown), false);

			KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
			FocusManager.IsFocusScopeProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

			// provide a default template selector for the caption and tab item. since we need ellipses, etc.
			ContentPane.HeaderTemplateSelectorProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(ContentPaneTemplateSelector.Instance));

			// raise an event when a content pane is shown/hidden
			UIElement.VisibilityProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisibilityChanged)));

			// AS 9/9/09 TFS21111
			// There is a bug in the control-tab navigation logic in the wpf framework. Essentially during its processing of 
			// the control tab navigation, it calls the KeyboardNavigation.GetFirstChild method. This method has a bug where 
			// it can return the element itself instead of a descendant if the element is a focus scope and its 
			// focusedelement is itself. The wpf framework has a couple of issues that cause it do this. First, when you 
			// Focus an element, it sets the FocusedElement of the FocusScope to itself even if that element is the FocusScope.
			// So when the GetFirstChild method calls into the FocusedElement method, it finds that it has a FocusedElement 
			// (which in this case happens to be the element itself because of the wpf logic I just mentioned). Then it calls 
			// IsDescendantOf on the element passed in to ensure that the focused element is still a visual descendant of the 
			// passed in element. Unfortunately the WPF IsDescendantOf implementation will return true if you pass an element 
			// into its own IsDescendantOf method so FocusedElement ends up returning the element itself (the ContentPane in 
			// this case). The GetFirstChild then ends up returning that element. The control tab navigation logic then gets 
			// into an endless loop where it thinks its processing the child of the contentpane but because its GetFirstChild 
			// returns the contentpane itself, it just keeps processing the contentpane. To workaround this, we're going to 
			// prevent the FocusedElement of the ContentPane from being set to itself.
			// I submitted this as a bug in the wpf framework as well:
			// https://connect.microsoft.com/WPF/feedback/ViewFeedback.aspx?FeedbackID=488851
			//
			FocusManager.FocusedElementProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceFocusedElement)));

			// AS 9/8/09 TFS21921
			// When the KeyboardDevice.ReevaluateFocusCallback method determines that focus is within a different presentationsource
			// they shift focus to the rootvisual of the active presentation source without asking the old focused element (i.e. 
			// without raising the PreviewLostKeyboardFocus on the old element). So we have to watch for a focus to the root visual 
			// when we are within the element losing focus so we can potentially cancel it.
			// 
			EventManager.RegisterClassHandler(typeof(FrameworkElement), Keyboard.PreviewGotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnPreviousRootGotKeyboardFocus));

			// AS 3/26/10 TFS30153 - PaneLocation Optimization
			XamDockManager.PaneLocationPropertyKey.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPaneLocationChanged)));

			// AS 3/26/10 TFS30153 - DockManager Optimization
			XamDockManager.DockManagerPropertyKey.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDockManagerChanged)));

            // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));
            ActivePaneManager.IsActivePaneProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), ActivePaneManager.IsActivePanePropertyKey);
            ActivePaneManager.IsActiveDocumentProperty.OverrideMetadata(typeof(ContentPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), ActivePaneManager.IsActiveDocumentPropertyKey);

        } 
		#endregion //Constructor

		// AS 3/26/10 TFS30153 - DockManager Optimization
		#region OnDockManagerChanged
		private static void OnDockManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ContentPane pane = d as ContentPane;

			// this came up in the ribbon and could come up. when something is in the logical
			// tree but gets removed from teh visual tree, we could get an invalid property
			// change notification. see the ribbon's OnRibbonChanged for more details.
			if (XamDockManager.GetDockManager(pane) != e.NewValue)
				return;

			XamDockManager oldDockManager = e.OldValue as XamDockManager;
			XamDockManager newDockManager = e.NewValue as XamDockManager;

			pane.VerifyIsPinnedState();

			if (null != oldDockManager)
			{
				oldDockManager.ActivePaneManager.OnPaneRemoved(pane);

				// AS 3/30/09 TFS16355 - WinForms Interop
				oldDockManager.DirtyHasHwndHosts();
			}

			if (null != newDockManager)
			{
				ContentPane ancestorPane = Utilities.GetAncestorFromType(pane, typeof(ContentPane), true, newDockManager) as ContentPane;

				// AS 5/15/08 BR32035
				// Make sure that a content pane cannot be positioned within another
				// content pane unless it is within another dockmanager. Otherwise every single
				// place that we deal with content pane must be evaluated and check to see that
				// it is not within a pane.
				//
				if (ancestorPane != null && ancestorPane != pane)
					throw new InvalidOperationException(XamDockManager.GetString("LE_CannotNestContentPanes", pane, ancestorPane));

				newDockManager.ActivePaneManager.OnPaneAdded(pane);

				// AS 3/30/09 TFS16355 - WinForms Interop
				if (pane.HasHwndHost && false.Equals(newDockManager.HasHwndHostNoVerify))
					newDockManager.DirtyHasHwndHosts();
			}
		}

		#endregion //OnDockManagerChanged

		#region Base Class Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Invoked when the template for the control has been changed.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

        #endregion //OnApplyTemplate

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns <see cref="ContentPane"/> Automation Peer Class <see cref="ContentPaneAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ContentPaneAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer

        // AS 2/11/09 TFS12849
        #region OnHeaderChanged
        /// <summary>
        /// Invoked when the Header property has been changed.
        /// </summary>
        /// <param name="oldHeader">The old Header property value</param>
        /// <param name="newHeader">The new Header property value</param>
        protected override void OnHeaderChanged(object oldHeader, object newHeader)
        {
            base.OnHeaderChanged(oldHeader, newHeader);

            PaneToolWindow tw = PaneToolWindow.GetSinglePaneToolWindow(this);

            if (null != tw)
                tw.RefreshSinglePaneTitle();
        } 
        #endregion //OnHeaderChanged

		#region OnKeyDown

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			// Pass this key along to our commands class which will check to see if a command
			// needs to be executed.  If so, the commands class will execute the command and
			// return true.
			if (e.Handled == false
				&& this.Commands != null
				&& this.Commands.ProcessKeyboardInput(e, this) == true)
				e.Handled = true;
		}
		#endregion //OnKeyDown

		#region OnMouseEnter
		/// <summary>
		/// Invoked when the mouse enters the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (this.IsInUnpinnedArea)
				UnpinnedTabFlyout.ShowFlyoutOnMouseEnter(this);
		}
		#endregion //OnMouseEnter

		#region OnMouseLeave
		/// <summary>
		/// Invoked when the mouse leaves the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (this.IsInUnpinnedArea)
				UnpinnedTabFlyout.HideFlyoutOnMouseLeave(this);
		}
		#endregion //OnMouseLeave

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#region AllowDocking

		/// <summary>
		/// Identifies the <see cref="AllowDocking"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowDockingProperty = DependencyProperty.Register("AllowDocking",
			typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.
		/// </summary>
		/// <seealso cref="AllowDockingProperty"/>
		//[Description("Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public bool AllowDocking
		{
			get
			{
				return (bool)this.GetValue(ContentPane.AllowDockingProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowDockingProperty, value);
			}
		}

		#endregion //AllowDocking

		#region AllowDockingLeft

		/// <summary>
		/// Identifies the <see cref="AllowDockingLeft"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowDockingLeftProperty = DependencyProperty.Register("AllowDockingLeft",
			typeof(bool?), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.
		/// </summary>
		/// <seealso cref="AllowDockingLeftProperty"/>
		//[Description("Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? AllowDockingLeft
		{
			get
			{
				return (bool?)this.GetValue(ContentPane.AllowDockingLeftProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowDockingLeftProperty, value);
			}
		}

		#endregion //AllowDockingLeft

		#region AllowDockingTop

		/// <summary>
		/// Identifies the <see cref="AllowDockingTop"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowDockingTopProperty = DependencyProperty.Register("AllowDockingTop",
			typeof(bool?), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.
		/// </summary>
		/// <seealso cref="AllowDockingTopProperty"/>
		//[Description("Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? AllowDockingTop
		{
			get
			{
				return (bool?)this.GetValue(ContentPane.AllowDockingTopProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowDockingTopProperty, value);
			}
		}

		#endregion //AllowDockingTop

		#region AllowDockingRight

		/// <summary>
		/// Identifies the <see cref="AllowDockingRight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowDockingRightProperty = DependencyProperty.Register("AllowDockingRight",
			typeof(bool?), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.
		/// </summary>
		/// <seealso cref="AllowDockingRightProperty"/>
		//[Description("Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? AllowDockingRight
		{
			get
			{
				return (bool?)this.GetValue(ContentPane.AllowDockingRightProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowDockingRightProperty, value);
			}
		}

		#endregion //AllowDockingRight

		#region AllowDockingBottom

		/// <summary>
		/// Identifies the <see cref="AllowDockingBottom"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowDockingBottomProperty = DependencyProperty.Register("AllowDockingBottom",
			typeof(bool?), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.
		/// </summary>
		/// <seealso cref="AllowDockingBottomProperty"/>
		//[Description("Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? AllowDockingBottom
		{
			get
			{
				return (bool?)this.GetValue(ContentPane.AllowDockingBottomProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowDockingBottomProperty, value);
			}
		}

		#endregion //AllowDockingBottom

		#region AllowDockingFloating

		/// <summary>
		/// Identifies the <see cref="AllowDockingFloating"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowDockingFloatingProperty = DependencyProperty.Register("AllowDockingFloating",
			typeof(bool?), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.
		/// </summary>
		/// <seealso cref="AllowDockingFloatingProperty"/>
		//[Description("Returns/sets a boolean indicating whether the pane may be docked within the XamDockManager or within a floating window.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? AllowDockingFloating
		{
			get
			{
				return (bool?)this.GetValue(ContentPane.AllowDockingFloatingProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowDockingFloatingProperty, value);
			}
		}

		#endregion //AllowDockingFloating

		#region AllowInDocumentHost

		/// <summary>
		/// Identifies the <see cref="AllowInDocumentHost"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowInDocumentHostProperty = DependencyProperty.Register("AllowInDocumentHost",
			typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Indicates if the pane may be added to the content area of the <see cref="XamDockManager"/> when the XamDockManager’s content is a <see cref="DocumentContentHost"/>.
		/// </summary>
		/// <seealso cref="AllowInDocumentHostProperty"/>
		/// <seealso cref="DocumentContentHost"/>
		/// <seealso cref="AllowFloatingOnly"/>
		/// <seealso cref="AllowPinning"/>
		/// <seealso cref="AllowClose"/>
		/// <seealso cref="DockableAreas"/>
		//[Description("Indicates if the pane may be added to the content area of the XamDockManager when the XamDockManager’s content is a DocumentContentHost.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public bool AllowInDocumentHost
		{
			get
			{
				return (bool)this.GetValue(ContentPane.AllowInDocumentHostProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowInDocumentHostProperty, value);
			}
		}

		#endregion //AllowInDocumentHost

		#region AllowFloatingOnly

		/// <summary>
		/// Identifies the <see cref="AllowFloatingOnly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowFloatingOnlyProperty = DependencyProperty.Register("AllowFloatingOnly",
			typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Indicates if the pane can be displayed in a non-dockable floating window.
		/// </summary>
		/// <seealso cref="AllowFloatingOnlyProperty"/>
		/// <seealso cref="AllowInDocumentHost"/>
		/// <seealso cref="AllowPinning"/>
		/// <seealso cref="AllowClose"/>
		/// <seealso cref="DockableAreas"/>
		//[Description("Indicates if the pane can be displayed in a non-dockable floating window.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public bool AllowFloatingOnly
		{
			get
			{
				return (bool)this.GetValue(ContentPane.AllowFloatingOnlyProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowFloatingOnlyProperty, value);
			}
		}

		#endregion //AllowFloatingOnly

		#region AllowPinning

		/// <summary>
		/// Identifies the <see cref="AllowPinning"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowPinningProperty = DependencyProperty.Register("AllowPinning",
			typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Indicates if the pane can be unpinned by the end user and displayed within the <see cref="UnpinnedTabArea"/>.
		/// </summary>
		/// <seealso cref="AllowPinningProperty"/>
		/// <seealso cref="AllowInDocumentHost"/>
		/// <seealso cref="AllowFloatingOnly"/>
		/// <seealso cref="AllowClose"/>
		/// <seealso cref="DockableAreas"/>
		//[Description("Indicates if the pane can be unpinned by the end user and displayed within the UnpinnedTabArea.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public bool AllowPinning
		{
			get
			{
				return (bool)this.GetValue(ContentPane.AllowPinningProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowPinningProperty, value);
			}
		}

		#endregion //AllowPinning

		#region AllowClose

		/// <summary>
		/// Identifies the <see cref="AllowClose"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowCloseProperty = DependencyProperty.Register("AllowClose",
			typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnAllowCloseChanged)));

		private static void OnAllowCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PaneToolWindow window = ToolWindow.GetToolWindow(d) as PaneToolWindow;

			if (null != window)
			{
				window.RefreshToolWindowState();
			}
		}

		/// <summary>
		/// Indicates if the pane may be hidden using the UI.
		/// </summary>
		/// <seealso cref="AllowCloseProperty"/>
		/// <seealso cref="CloseAction"/>
		/// <seealso cref="AllowInDocumentHost"/>
		/// <seealso cref="AllowFloatingOnly"/>
		/// <seealso cref="AllowPinning"/>
		/// <seealso cref="DockableAreas"/>
		//[Description("Indicates if the pane may be hidden using the UI.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public bool AllowClose
		{
			get
			{
				return (bool)this.GetValue(ContentPane.AllowCloseProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowCloseProperty, value);
			}
		}

		#endregion //AllowClose

		#region AllowDockingInTabGroup

		/// <summary>
		/// Identifies the <see cref="AllowDockingInTabGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowDockingInTabGroupProperty = DependencyProperty.Register("AllowDockingInTabGroup",
			typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Indicates if the pane may be docked in a TabGroupPane within another pane. When false, a pane cannot be dragged into this pane and this pane cannot be dragged onto another such that it would create a tab group.
		/// </summary>
		/// <seealso cref="AllowDockingInTabGroupProperty"/>
		/// <seealso cref="DockableAreas"/>
		//[Description("Indicates if the pane may be docked in a TabGroupPane within another pane. When false, a pane cannot be dragged into this pane and this pane cannot be dragged onto another such that it would create a tab group.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public bool AllowDockingInTabGroup
		{
			get
			{
				return (bool)this.GetValue(ContentPane.AllowDockingInTabGroupProperty);
			}
			set
			{
				this.SetValue(ContentPane.AllowDockingInTabGroupProperty, value);
			}
		}

		#endregion //AllowDockingInTabGroup

		#region CloseAction

		/// <summary>
		/// Identifies the <see cref="CloseAction"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CloseActionProperty = DependencyProperty.Register("CloseAction",
			typeof(PaneCloseAction), typeof(ContentPane), new FrameworkPropertyMetadata(PaneCloseAction.HidePane));

		/// <summary>
		/// Determines what happens to the pane when it is closed.
		/// </summary>
		/// <seealso cref="CloseActionProperty"/>
		/// <seealso cref="AllowClose"/>
		//[Description("Determines what happens to the pane when it is closed.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public PaneCloseAction CloseAction
		{
			get
			{
				return (PaneCloseAction)this.GetValue(ContentPane.CloseActionProperty);
			}
			set
			{
				this.SetValue(ContentPane.CloseActionProperty, value);
			}
		}

		#endregion //CloseAction

		// AS 10/5/09 NA 2010.1 - Caption Button Visibility
		#region CloseButtonVisibility

		/// <summary>
		/// Identifies the <see cref="CloseButtonVisibility"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public static readonly DependencyProperty CloseButtonVisibilityProperty = DependencyProperty.Register("CloseButtonVisibility",
			typeof(Visibility), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

		/// <summary>
		/// Returns or sets the visibility of the close button within the pane's caption area.
		/// </summary>
		/// <seealso cref="CloseButtonVisibilityProperty"/>
		//[Description("Returns or sets the visibility of the close button within the pane's caption area.")]
		//[Category("DockManager Properties")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public Visibility CloseButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ContentPane.CloseButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(ContentPane.CloseButtonVisibilityProperty, value);
			}
		}

		#endregion //CloseButtonVisibility

        // AS 10/13/08 TFS6032
        // We need to be able to hide the content of the pane when it is unpinned
        // and contains hwndhosts. We could also use this in the future to hide the 
        // content area for other reasons.
        //
        #region ContentVisibility

        private static readonly DependencyPropertyKey ContentVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("ContentVisibility",
            typeof(Visibility), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

        /// <summary>
        /// Identifies the <see cref="ContentVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ContentVisibilityProperty =
            ContentVisibilityPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns a value indicating the preferred visibility for the content of the pane.
        /// </summary>
        /// <seealso cref="ContentVisibilityProperty"/>
        //[Description("Returns a value indicating the preferred visibility for the content of the pane.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public Visibility ContentVisibility
        {
            get
            {
                return (Visibility)this.GetValue(ContentPane.ContentVisibilityProperty);
            }
        }

        #endregion //ContentVisibility

		#region HasImage

		private static readonly DependencyPropertyKey HasImagePropertyKey =
			DependencyProperty.RegisterReadOnly("HasImage",
			typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasImage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasImageProperty =
			HasImagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the <see cref="Image"/> property has been set.
		/// </summary>
		/// <seealso cref="HasImageProperty"/>
		/// <seealso cref="Image"/>
		//[Description("Returns a boolean indicating if the Image property has been set.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool HasImage
		{
			get
			{
				return (bool)this.GetValue(ContentPane.HasImageProperty);
			}
		}

		#endregion //HasImage

		#region HeaderVisibility

		private static readonly DependencyPropertyKey HeaderVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("HeaderVisibility",
			typeof(Visibility), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

		/// <summary>
		/// Identifies the <see cref="HeaderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderVisibilityProperty =
			HeaderVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the preferred visibility for the pane based on its current location.
		/// </summary>
		/// <seealso cref="HeaderVisibilityProperty"/>
		//[Description("Returns the preferred visibility for the pane based on its current location.")]
		//[Category("DockManager Properties")] // Appearance
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility HeaderVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ContentPane.HeaderVisibilityProperty);
			}
		}

		#endregion //HeaderVisibility

		#region Image

		/// <summary>
		/// Identifies the <see cref="Image"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image",
			typeof(ImageSource), typeof(ContentPane), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnImageChanged)));

		private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.SetValue(ContentPane.HasImagePropertyKey, e.NewValue == null ? DependencyProperty.UnsetValue : KnownBoxes.TrueBox);
		}

		/// <summary>
		/// The image used to represent the pane in its tab item as well as in the <see cref="PaneNavigator"/>.
		/// </summary>
		/// <seealso cref="ImageProperty"/>
		/// <seealso cref="HasImage"/>
		//[Description("The image used to represent the pane in its tab item as well as in the 'PaneNavigator'.")]
		//[Category("DockManager Properties")] // Appearance
		[Bindable(true)]
		public ImageSource Image
		{
			get
			{
				return (ImageSource)this.GetValue(ContentPane.ImageProperty);
			}
			set
			{
				this.SetValue(ContentPane.ImageProperty, value);
			}
		}

		#endregion //Image

		#region IsActivePane

		/// <summary>
		/// Identifies the <see cref="IsActivePane"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActivePaneProperty = ActivePaneManager.IsActivePaneProperty.AddOwner(typeof(ContentPane));

		/// <summary>
		/// Returns a boolean indicating whether the pane is the <see cref="XamDockManager.ActivePane"/> of the XamDockManager.
		/// </summary>
		/// <seealso cref="IsActivePaneProperty"/>
		//[Description("Returns a boolean indicating whether the pane is the active pane of the XamDockManager.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsActivePane
		{
			get
			{
				return (bool)this.GetValue(ContentPane.IsActivePaneProperty);
			}
		}

		#endregion //IsActivePane

		#region IsActiveDocument

		//internal static readonly DependencyPropertyKey IsActiveDocumentPropertyKey =
		//    DependencyProperty.RegisterReadOnly("IsActiveDocument",
		//    typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsActiveDocument"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveDocumentProperty = ActivePaneManager.IsActiveDocumentProperty.AddOwner(typeof(ContentPane));
			//IsActiveDocumentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the pane is the <see cref="DocumentContentHost.ActiveDocument"/>
		/// </summary>
		/// <seealso cref="IsActiveDocumentProperty"/>
		//[Description("Returns a boolean indicating whether the pane is the active document of the DocumentContentHost within a XamDockManager.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsActiveDocument
		{
			get
			{
				return (bool)this.GetValue(ContentPane.IsActiveDocumentProperty);
			}
		}

		#endregion //IsActiveDocument

		#region IsPinned

		/// <summary>
		/// Identifies the <see cref="IsPinned"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsPinnedProperty = DependencyProperty.Register("IsPinned",
			typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnIsPinnedChanged)));

		private static void OnIsPinnedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            ((ContentPane)d).VerifyIsPinnedState();
		}

		/// <summary>
		/// Returns/sets a boolean indicating whether the pane should be displayed within an <see cref="UnpinnedTabArea"/>
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, <b>IsPinned</b> is true meaning that the pane will be displayed based on its current location. When set to false, 
		/// the pane will be removed from its current location and displayed within the <see cref="UnpinnedTabArea"/> of the containing 
		/// <see cref="XamDockManager"/>.</p>
		/// </remarks>
		/// <seealso cref="IsPinnedProperty"/>
		//[Description("Returns/sets a boolean indicating whether the pane should be displayed within an 'UnpinnedTabArea'")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public bool IsPinned
		{
			get
			{
				return (bool)this.GetValue(ContentPane.IsPinnedProperty);
			}
			set
			{
				this.SetValue(ContentPane.IsPinnedProperty, value);
			}
		}

		#endregion //IsPinned

		// AS 6/24/11 FloatingWindowCaptionSource
		#region IsProvidingFloatingWindowCaption

		private static readonly DependencyPropertyKey IsProvidingFloatingWindowCaptionPropertyKey =
			DependencyProperty.RegisterReadOnly("IsProvidingFloatingWindowCaption",
			typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsProvidingFloatingWindowCaption"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsProvidingFloatingWindowCaptionProperty =
			IsProvidingFloatingWindowCaptionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the pane is providing the caption of the floating PaneToolWindow
		/// </summary>
		/// <seealso cref="IsProvidingFloatingWindowCaptionProperty"/>
		[Description("Returns or sets a boolean indicating if the pane is providing the caption of the floating PaneToolWindow")]
		[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsProvidingFloatingWindowCaption
		{
			get
			{
				return (bool)this.GetValue(ContentPane.IsProvidingFloatingWindowCaptionProperty);
			}
		}

		#endregion //IsProvidingFloatingWindowCaption

		#region NavigatorDescription

		/// <summary>
		/// Identifies the <see cref="NavigatorDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NavigatorDescriptionProperty = DependencyProperty.Register("NavigatorDescription",
			typeof(object), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets a description of the contents. The description is used within the <see cref="PaneNavigator"/> to provide more information about the pane.
		/// </summary>
		/// <seealso cref="NavigatorDescriptionProperty"/>
		/// <seealso cref="NavigatorTitle"/>
		/// <seealso cref="NavigatorDescriptionTemplate"/>
		/// <seealso cref="NavigatorDescriptionTemplateSelector"/>
		//[Description("Returns/sets a description of the contents. The description is used within the 'PaneNavigator' to provide more information about the pane.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		public object NavigatorDescription
		{
			get
			{
				return (object)this.GetValue(ContentPane.NavigatorDescriptionProperty);
			}
			set
			{
				this.SetValue(ContentPane.NavigatorDescriptionProperty, value);
			}
		}

		#endregion //NavigatorDescription

		#region NavigatorDescriptionTemplate

		/// <summary>
		/// Identifies the <see cref="NavigatorDescriptionTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NavigatorDescriptionTemplateProperty = DependencyProperty.Register("NavigatorDescriptionTemplate",
			typeof(DataTemplate), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// The template used to display the content of the description in the <see cref="PaneNavigator"/>.
		/// </summary>
		/// <seealso cref="NavigatorDescriptionTemplateProperty"/>
		/// <seealso cref="NavigatorDescription"/>
		/// <seealso cref="NavigatorDescriptionTemplateSelector"/>
		//[Description("The template used to display the content of the description in the 'PaneNavigator'.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		public DataTemplate NavigatorDescriptionTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(ContentPane.NavigatorDescriptionTemplateProperty);
			}
			set
			{
				this.SetValue(ContentPane.NavigatorDescriptionTemplateProperty, value);
			}
		}

		#endregion //NavigatorDescriptionTemplate

		#region NavigatorDescriptionTemplateSelector

		/// <summary>
		/// Identifies the <see cref="NavigatorDescriptionTemplateSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NavigatorDescriptionTemplateSelectorProperty = DependencyProperty.Register("NavigatorDescriptionTemplateSelector",
			typeof(DataTemplateSelector), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Used to provide custom logic for choosing the template used to display the pane’s description in the <see cref="PaneNavigator"/>.
		/// </summary>
		/// <seealso cref="NavigatorDescriptionTemplateSelectorProperty"/>
		/// <seealso cref="NavigatorDescription"/>
		/// <seealso cref="NavigatorDescriptionTemplate"/>
		//[Description("Used to provide custom logic for choosing the template used to display the pane’s description in the 'PaneNavigator'.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		public DataTemplateSelector NavigatorDescriptionTemplateSelector
		{
			get
			{
				return (DataTemplateSelector)this.GetValue(ContentPane.NavigatorDescriptionTemplateSelectorProperty);
			}
			set
			{
				this.SetValue(ContentPane.NavigatorDescriptionTemplateSelectorProperty, value);
			}
		}

		#endregion //NavigatorDescriptionTemplateSelector

		#region NavigatorTitle

		/// <summary>
		/// Identifies the <see cref="NavigatorTitle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NavigatorTitleProperty = DependencyProperty.Register("NavigatorTitle",
			typeof(object), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the title for the pane. The title is used within the <see cref="PaneNavigator"/> to provide more information about the pane.
		/// </summary>
		/// <seealso cref="NavigatorTitleProperty"/>
		/// <seealso cref="NavigatorDescription"/>
		/// <seealso cref="NavigatorTitleTemplate"/>
		/// <seealso cref="NavigatorTitleTemplateSelector"/>
		//[Description("Returns/sets the title for the pane. The title is used within the 'PaneNavigator' to provide more information about the pane.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		public object NavigatorTitle
		{
			get
			{
				return (object)this.GetValue(ContentPane.NavigatorTitleProperty);
			}
			set
			{
				this.SetValue(ContentPane.NavigatorTitleProperty, value);
			}
		}

		#endregion //NavigatorTitle

		#region NavigatorTitleTemplate

		/// <summary>
		/// Identifies the <see cref="NavigatorTitleTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NavigatorTitleTemplateProperty = DependencyProperty.Register("NavigatorTitleTemplate",
			typeof(DataTemplate), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// The template used to display the content of the title in the <see cref="PaneNavigator"/>.
		/// </summary>
		/// <seealso cref="NavigatorTitleTemplateProperty"/>
		/// <seealso cref="NavigatorTitle"/>
		/// <seealso cref="NavigatorTitleTemplateSelector"/>
		//[Description("The template used to display the content of the title in the 'PaneNavigator'.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		public DataTemplate NavigatorTitleTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(ContentPane.NavigatorTitleTemplateProperty);
			}
			set
			{
				this.SetValue(ContentPane.NavigatorTitleTemplateProperty, value);
			}
		}

		#endregion //NavigatorTitleTemplate

		#region NavigatorTitleTemplateSelector

		/// <summary>
		/// Identifies the <see cref="NavigatorTitleTemplateSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NavigatorTitleTemplateSelectorProperty = DependencyProperty.Register("NavigatorTitleTemplateSelector",
			typeof(DataTemplateSelector), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Used to provide custom logic for choosing the template used to display the pane’s title in the <see cref="PaneNavigator"/>.
		/// </summary>
		/// <seealso cref="NavigatorTitleTemplateSelectorProperty"/>
		/// <seealso cref="NavigatorTitle"/>
		/// <seealso cref="NavigatorTitleTemplate"/>
		//[Description("Used to provide custom logic for choosing the template used to display the pane’s title in the 'PaneNavigator'.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		public DataTemplateSelector NavigatorTitleTemplateSelector
		{
			get
			{
				return (DataTemplateSelector)this.GetValue(ContentPane.NavigatorTitleTemplateSelectorProperty);
			}
			set
			{
				this.SetValue(ContentPane.NavigatorTitleTemplateSelectorProperty, value);
			}
		}

		#endregion //NavigatorTitleTemplateSelector

		#region PaneLocation

		/// <summary>
		/// Identifies the <see cref="PaneLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PaneLocationProperty = XamDockManager.PaneLocationProperty.AddOwner(typeof(ContentPane));

		/// <summary>
		/// Returns the current location of the pane.
		/// </summary>
		/// <seealso cref="PaneLocationProperty"/>
		//[Description("Description")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public PaneLocation PaneLocation
		{
			get
			{
				return (PaneLocation)this.GetValue(ContentPane.PaneLocationProperty);
			}
		}
		#endregion //PaneLocation

		// AS 10/5/09 NA 2010.1 - Caption Button Visibility
		#region PinButtonVisibility

		/// <summary>
		/// Identifies the <see cref="PinButtonVisibility"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public static readonly DependencyProperty PinButtonVisibilityProperty = DependencyProperty.Register("PinButtonVisibility",
			typeof(Visibility), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

		/// <summary>
		/// Returns or sets the visibility of the pin/unpin button within the pane's caption area.
		/// </summary>
		/// <seealso cref="PinButtonVisibilityProperty"/>
		//[Description("Returns or sets the visibility of the pin/unpin button within the pane's caption area.")]
		//[Category("DockManager Properties")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public Visibility PinButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ContentPane.PinButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(ContentPane.PinButtonVisibilityProperty, value);
			}
		}

		#endregion //PinButtonVisibility

		#region SaveInLayout

		/// <summary>
		/// Identifies the <see cref="SaveInLayout"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SaveInLayoutProperty = DependencyProperty.Register("SaveInLayout",
			typeof(bool), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns or sets a boolean indicating if the pane should be included in a saved layout.
		/// </summary>
		/// <seealso cref="SaveInLayoutProperty"/>
		//[Description("Indicates if the pane should be included in a saved layout.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public bool SaveInLayout
		{
			get
			{
				return (bool)this.GetValue(ContentPane.SaveInLayoutProperty);
			}
			set
			{
				this.SetValue(ContentPane.SaveInLayoutProperty, value);
			}
		}

		#endregion //SaveInLayout

		#region SerializationId

		/// <summary>
		/// Identifies the <see cref="SerializationId"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SerializationIdProperty = DependencyProperty.Register("SerializationId",
			typeof(string), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets a string that is stored with the pane when the layout is saved.
		/// </summary>
		/// <remarks>
		/// <p class="body">The SerializationId property is not used by the ContentPane itself. Instead, the value of the 
		/// property is saved along with the layout when using the <see cref="XamDockManager.SaveLayout(System.IO.Stream)"/> method. 
		/// This property can be used to save information that you can use to create a ContentPane when the layout is loaded if 
		/// the pane was not already loaded. If a ContentPane is referenced within a loaded layout and a pane with the saved 
		/// name does not exist within the layout, the <see cref="XamDockManager.InitializePaneContent"/> event is raised 
		/// to allow you to create the content for the pane. The SerializationId can be used to identify what type of content 
		/// that you want to create. For example, this can be set to the name of the file so that you can load the file when 
		/// the layout is loaded.</p>
		/// </remarks>
		/// <seealso cref="SerializationIdProperty"/>
		//[Description("A string that is stored with the pane when the layout is saved.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		public string SerializationId
		{
			get
			{
				return (string)this.GetValue(ContentPane.SerializationIdProperty);
			}
			set
			{
				this.SetValue(ContentPane.SerializationIdProperty, value);
			}
		}

		#endregion //SerializationId

		#region TabHeader

		/// <summary>
		/// Identifies the <see cref="TabHeader"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabHeaderProperty = DependencyProperty.Register("TabHeader",
			typeof(object), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// The data used for the header of the pane when displayed within a tab item or when the pane is unpinned.
		/// </summary>
		/// <seealso cref="TabHeaderProperty"/>
		/// <seealso cref="TabHeaderTemplate"/>
		/// <seealso cref="TabHeaderTemplateSelector"/>
		//[Description("The data used for the header of the pane when displayed within a tab item or when the pane is unpinned.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		public object TabHeader
		{
			get
			{
				return (object)this.GetValue(ContentPane.TabHeaderProperty);
			}
			set
			{
				this.SetValue(ContentPane.TabHeaderProperty, value);
			}
		}

		#endregion //TabHeader

		#region TabHeaderTemplate

		/// <summary>
		/// Identifies the <see cref="TabHeaderTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabHeaderTemplateProperty = DependencyProperty.Register("TabHeaderTemplate",
			typeof(DataTemplate), typeof(ContentPane), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// The template used to display the content of the <see cref="TabHeader"/>.
		/// </summary>
		/// <seealso cref="TabHeaderTemplateProperty"/>
		/// <seealso cref="TabHeader"/>
		/// <seealso cref="TabHeaderTemplateSelector"/>
		//[Description("The template used to display the content of the 'TabHeader'.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		public DataTemplate TabHeaderTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(ContentPane.TabHeaderTemplateProperty);
			}
			set
			{
				this.SetValue(ContentPane.TabHeaderTemplateProperty, value);
			}
		}

		#endregion //TabHeaderTemplate

		#region TabHeaderTemplateSelector

		/// <summary>
		/// Identifies the <see cref="TabHeaderTemplateSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabHeaderTemplateSelectorProperty = DependencyProperty.Register("TabHeaderTemplateSelector",
			typeof(DataTemplateSelector), typeof(ContentPane), new FrameworkPropertyMetadata(ContentPaneTemplateSelector.Instance));

		/// <summary>
		/// Used to provide custom logic for choosing the template used to display the <see cref="TabHeader"/>.
		/// </summary>
		/// <seealso cref="TabHeaderTemplateSelectorProperty"/>
		/// <seealso cref="TabHeader"/>
		/// <seealso cref="TabHeaderTemplate"/>
		//[Description("Used to provide custom logic for choosing the template used to display the 'TabHeader'.")]
		//[Category("DockManager Properties")] // Data
		[Bindable(true)]
		public DataTemplateSelector TabHeaderTemplateSelector
		{
			get
			{
				return (DataTemplateSelector)this.GetValue(ContentPane.TabHeaderTemplateSelectorProperty);
			}
			set
			{
				this.SetValue(ContentPane.TabHeaderTemplateSelectorProperty, value);
			}
		}

		#endregion //TabHeaderTemplateSelector

		// AS 10/5/09 NA 2010.1 - Caption Button Visibility
		#region WindowPositionMenuVisibility

		/// <summary>
		/// Identifies the <see cref="WindowPositionMenuVisibility"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public static readonly DependencyProperty WindowPositionMenuVisibilityProperty = DependencyProperty.Register("WindowPositionMenuVisibility",
			typeof(Visibility), typeof(ContentPane), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

		/// <summary>
		/// Returns or sets the visibility of the window position menu within the pane's caption area.
		/// </summary>
		/// <seealso cref="WindowPositionMenuVisibilityProperty"/>
		//[Description("Returns or sets the visibility of the window position menu within the pane's caption area.")]
		//[Category("DockManager Properties")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public Visibility WindowPositionMenuVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ContentPane.WindowPositionMenuVisibilityProperty);
			}
			set
			{
				this.SetValue(ContentPane.WindowPositionMenuVisibilityProperty, value);
			}
		}

		#endregion //WindowPositionMenuVisibility

		#endregion //Public Properties 

		#region Internal Properties

        // AS 10/13/08 TFS6032
        #region AllowAnimations
        internal bool AllowAnimations
        {
            get
            {
                HwndHostInfo hhi = HwndHostInfo.GetHwndHost(this);

                return null == hhi || hhi.HasHwndHost == false;
            }
        } 
        #endregion //AllowAnimations

		#region AllowedDropLocations
		internal AllowedDropLocations AllowedDropLocations
		{
			get
			{
				AllowedDropLocations location = (AllowedDropLocations)this.DockableAreas;

				if (this.AllowInDocumentHost)
					location |= AllowedDropLocations.Document;

				return location;
			}
		} 
		#endregion //AllowedDropLocations

		#region CanActivate
		internal bool CanActivate
		{
			get { return this.Visibility == Visibility.Visible && this.IsEnabled; }
		} 
		#endregion //CanActivate

		#region CanCloseAllButThis
		internal bool CanCloseAllButThis
		{
			get
			{
				// AS 4/29/08 BR31977
				// Rewritten to deal with all documents in the document host and not just
				// those of the active group.
				//
				//return this._placementInfo.CurrentContainer != null &&
				//	this._placementInfo.CurrentContainer.CanCloseAllButThis(this);
				if (this.PaneLocation == PaneLocation.Document)
				{
					XamDockManager dm = XamDockManager.GetDockManager(this);
					DocumentContentHost dch = dm != null ? dm.DocumentContentHost : null;

					if (null != dch)
					{
						ContentPane pane = DockManagerUtilities.GetFirstLastPane(dch, true, PaneFilterFlags.AllVisible);

						while (null != pane)
						{
							if (pane != this && pane.AllowClose)
								return true;

							pane = DockManagerUtilities.GetNextPreviousPane(pane, false, true, dch);
						}
					}
				}

				return false;
			}
		}
		#endregion //CanCloseAllButThis

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region CanShowInPopup
        /// <summary>
        /// Indicates if the content pane has content that should not be hosted in a Popup
        /// </summary>
        internal bool CanShowInPopup
        {
            get
            {
                HwndHostInfo hhi = HwndHostInfo.GetHwndHost(this);

                if (null != hhi && hhi.HasHwndHost)
                {
                    // the webbrowser has an explicit check to see if it 
                    // is hosted within a popup and throws an exception 
                    // if it is so we cannot allow the pane to be in a popup
                    if (HwndHostInfo.WebBrowserType != null 
                        && !DockManagerUtilities.CanHostBrowserInPopup 
                        && hhi.HasWebBrowser)
                        return false;
                }

                return true;
            }
        } 
        #endregion //CanShowInPopup

		#region CanToggleDockedState
		internal bool CanToggleDockedState
		{
			get
			{
				switch(XamDockManager.GetPaneLocation(this))
				{
					case PaneLocation.DockedBottom:
					case PaneLocation.DockedLeft:
					case PaneLocation.DockedRight:
					case PaneLocation.DockedTop:
						// only if it is allowed to float
						return this.IsDockingAllowed(PaneLocation.Floating);

					case PaneLocation.Floating:
						// as long as it includes something except floating
						return (this.DockableAreas & ~DockableAreas.Floating) != 0;

					default:
						return false;
				}
			}
		}
		#endregion //CanToggleDockedState

		#region CanTogglePinnedState
		internal bool CanTogglePinnedState
		{
			get
			{
				if (this.AllowPinning == false)
					return false;

				switch (XamDockManager.GetPaneLocation(this))
				{
					case PaneLocation.DockedBottom:
					case PaneLocation.DockedLeft:
					case PaneLocation.DockedRight:
					case PaneLocation.DockedTop:
					case PaneLocation.Unpinned:
						return true;
					default:
						return false;
				}
			}
		}
		#endregion //CanTogglePinnedState

		#region Commands
		internal CommandsBase Commands
		{
			get { return this._commands; }
		}
		#endregion //Commands

		#region DockableAreas
		internal DockableAreas DockableAreas
		{
			get
			{
				DockableAreas areas = 0;

				if (this.IsDockingAllowed(PaneLocation.DockedBottom))
					areas |= DockableAreas.Bottom;

				if (this.IsDockingAllowed(PaneLocation.DockedLeft))
					areas |= DockableAreas.Left;

				if (this.IsDockingAllowed(PaneLocation.DockedRight))
					areas |= DockableAreas.Right;

				if (this.IsDockingAllowed(PaneLocation.DockedTop))
					areas |= DockableAreas.Top;

				if (this.IsDockingAllowed(PaneLocation.Floating))
					areas |= DockableAreas.Floating;

				return areas;
			}
		} 
		#endregion //DockableAreas

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region HasHwndHost
        internal bool HasHwndHost
        {
            get
            {
                HwndHostInfo hhi = HwndHostInfo.GetHwndHost(this);
                return null != hhi && hhi.HasHwndHost;
            }
        }
        #endregion //HasHwndHost

        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        #region HeaderPresenter

        // Keep track of the paneHeader so we can let it know to Update its vsiual state when the
        // pane's IsActive or IsActiveDocument properties change
        internal PaneHeaderPresenter HeaderPresenter
        {
            get
            {
                if (this._paneHeaderPresenter == null)
                    return null;

                return Utilities.GetWeakReferenceTargetSafe(this._paneHeaderPresenter) as PaneHeaderPresenter;
            }
        }

        #endregion //HeaderPresenter	
    
		// AS 7/14/09 TFS18400
		#region IsActivating
		internal bool IsActivating
		{
			get { return _isActivating; }
		} 
		#endregion //IsActivating

        #region IsKeyboardFocusWithinEx
        // AS 1/30/09 TFS12993
        // When the outer content pane received a MouseDown it tried to activate 
        // itself. Within the Activate method, we do not do anything if the pane 
        // already has focus since we don't want to move focus. However we were 
        // using the FrameworkElement's IsKeyboardFocusWithin. When you clicked 
        // the first time focus was within the Button in the pane so 
        // IsKeyboardFocusWithin returned true and we did nothing. The second time 
        // though focus was within the checkbox within the popup. At this point 
        // the IsKeyboardFocusWithin returned false so we tried to activate the 
        // pane. This pulled focus out of the checkbox which cleared its IsPressed 
        // state which caused it to not raise the Click event and therefore not 
        // toggle its checked state. I had a helper property named 
        // IsKeyboardFocusWithinEx that more accurately reflects the 
        // iskeyboardwithin state so I used this in addition. I also decided to 
        // add a check for whether focus was within an hwnd host within the pane 
        // since for that too he IsKeyboardFocusWithin would return false.
        //
        internal bool IsKeyboardFocusWithinEx
        {
            get
            {
                // IsKeyboardFocusWithin may return false if focus is within a popup 
                // associated with an element within the pane. To get around this 
                // the IsKeyboardFocusWithinEx of the ActivePaneManager will be checked 
                // as well. This monitor's the (Got|Lost)KeyboardFocus routed events 
                // for the ContentPane class.
                return this.IsKeyboardFocusWithin ||
                    ActivePaneManager.GetIsKeyboardFocusWithinEx(this) ||
                    this.HasFocusInHwndHost();
            }
        } 
        #endregion //IsKeyboardFocusWithinEx

        // AS 10/13/08 TFS6032
        // We need to maintain a list of the hwndhosts within the pane. When we have one 
        // or more then the content needs to be hidden when the pane is unpinned and not 
        // displayed within the flyout.
        //
        #region HwndHost
        
#region Infragistics Source Cleanup (Region)



































#endregion // Infragistics Source Cleanup (Region)

        #endregion //HwndHost

		#region IsCurrentFlyoutPane
		/// <summary>
		/// Returns a boolean indicating if the pane is the one currently displayed within the unpinned flyout.
		/// </summary>
		internal bool IsCurrentFlyoutPane
		{
			get
			{
				XamDockManager dockManager = XamDockManager.GetDockManager(this);

				return null != dockManager && dockManager.CurrentFlyoutPane == this;
			}
		} 
		#endregion //IsCurrentFlyoutPane

		#region IsDockable
		/// <summary>
		/// Indicates if the pane is currently in a dockable state - docked or floating.
		/// </summary>
		internal bool IsDockable
		{
			get
			{
				return DockManagerUtilities.IsDockable(XamDockManager.GetPaneLocation(this));
			}
		}
		#endregion //IsDockable

		#region IsDocument
		internal bool IsDocument
		{
			get { return XamDockManager.GetPaneLocation(this) == PaneLocation.Document; }
		} 
		#endregion //IsDocument

		#region IsInUnpinnedArea
		internal bool IsInUnpinnedArea
		{
			get
			{
				return XamDockManager.GetPaneLocation(this) == PaneLocation.Unpinned;
			}
		} 
		#endregion //IsInUnpinnedArea

		#region LastActivatedTime
		internal DateTime LastActivatedTime
		{
			get { return this._lastActivatedTime; }
			set { this._lastActivatedTime = value; }
		} 
		#endregion //LastActivatedTime

		// AS 4/25/08
		// We need to track where the top/left of the pane is in screen coordinates
		// when it is in a floating pane in case we need to create a floating dockable
		// or floating only pane.
		//
		#region LastFloatingLocation
		internal Point? LastFloatingLocation
		{
			get { return this._lastFloatingLocation; }
			set { this._lastFloatingLocation = value; }
		} 
		#endregion //LastFloatingLocation

		#region LastFloatingSize
		/// <summary>
		/// Returns the size at which the pane was last arranged when it was floating.
		/// </summary>
		internal Size LastFloatingSize
		{
			get { return this._lastFloatingSize; }
			set { this._lastFloatingSize = value; }
		} 
		#endregion //LastFloatingSize

		#region LastFloatingWindowRect
		/// <summary>
		/// Returns the size and position of the floating window at which the pane was last arranged when it was floating.
		/// </summary>
		internal Rect LastFloatingWindowRect
		{
			get { return this._lastFloatingWindowRect; }
			set { this._lastFloatingWindowRect = value; }
		}
		#endregion //LastFloatingWindowRect

		#region PlacementInfo
		/// <summary>
		/// Returns information about where the pane has been placed within the <see cref="XamDockManager"/>
		/// </summary>
		internal PanePlacementInfo PlacementInfo
		{
			get { return this._placementInfo; }
		} 
		#endregion //PlacementInfo

		// AS 5/14/08 BR32037
		#region SuppressFlyoutDuringActivate
		internal bool SuppressFlyoutDuringActivate
		{
			get { return this._suppressFlyoutDuringActivate; }
			set { this._suppressFlyoutDuringActivate = value; }
		} 
		#endregion //SuppressFlyoutDuringActivate

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region Activate
		/// <summary>
		/// Puts focus into the content pane making it the <see cref="XamDockManager.ActivePane"/> of the owning <see cref="XamDockManager"/>
		/// </summary>
		// AS 8/21/09 TFS21171
		// Changed return type of public method to return a boolean as we do for the internal overloads.
		//
		//public void Activate()
		public bool Activate()
		{
			return this.ActivateInternal();
		}
		#endregion //Activate

		#region ExecuteCommand

		/// <summary>
		/// Executes the specified RoutedCommand.
		/// </summary>
		/// <param name="command">The RoutedCommand to execute.</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="ContentPaneCommands"/>
		public bool ExecuteCommand(RoutedCommand command)
		{
			return this.ExecuteCommandImpl(new ExecuteCommandInfo(command));
		}

		#endregion //ExecuteCommand

		#endregion //Public Methods
        
        #region Protected Methods

        #region VisualState... Methods


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            // set Common states
            if ( this.IsEnabled )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);


            // set active states
            if (this.IsActivePane)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateActive, useTransitions);
            else
            {
                if (this.IsActiveDocument)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateActiveDocument, VisualStateUtilities.StateInactive);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateInactive, useTransitions);
            }

            string state = null;
 
            // set PaneLocation states
            switch (this.PaneLocation)
            {
                case PaneLocation.DockedBottom:
                    state = VisualStateUtilities.StateDockedBottom;
                    break;
                case PaneLocation.DockedLeft:
                    state = VisualStateUtilities.StateDockedLeft;
                    break;
                case PaneLocation.DockedRight:
                    state = VisualStateUtilities.StateDockedRight;
                    break;
                case PaneLocation.DockedTop:
                    state = VisualStateUtilities.StateDockedTop;
                    break;
                case PaneLocation.Document:
                    state = VisualStateUtilities.StateDocument;
                    break;
                case PaneLocation.Floating:
                    state = VisualStateUtilities.StateFloating;
                    break;
                case PaneLocation.Unpinned:
                    state = VisualStateUtilities.StateUnpinned;
                    break;
            }
            
            if ( state != null )
                VisualStateManager.GoToState(this, state, useTransitions);

        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ContentPane pane = target as ContentPane;

            pane.UpdateVisualStates();
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
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

            PaneHeaderPresenter php = this.HeaderPresenter;

            if (php != null)
                php.UpdateVisualStates(useTransitions);
        }



        #endregion //VisualState... Methods	

        #endregion //Protected Methods

        #region Internal Methods

        #region Activate
        /// <summary>
		/// Puts focus into the pane.
		/// </summary>
		/// <returns>True if it was able to move focus into the pane; false if the pane already had focus or couldn't get focus</returns>
		internal bool ActivateInternal()
		{
			return this.ActivateInternal(false);
		}

		/// <summary>
		/// Puts focus into the pane.
		/// </summary>
		/// <param name="force">True if we should force the element to get focus regardless of whether it thinks it has focus</param>
		/// <returns>True if it was able to move focus into the pane; false if the pane already had focus or couldn't get focus</returns>
		internal bool ActivateInternal(bool force)
		{
			return this.ActivateInternal(force, true);
		}

		internal bool ActivateInternal(bool force, bool bringIntoView)
		{
			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			return this.ActivateInternal(force, bringIntoView, false);
		}

        /// <summary>
		/// Puts focus into the pane.
		/// </summary>
		/// <param name="force">True if we should force the element to get focus regardless of whether it thinks it has focus</param>
		/// <param name="bringIntoView">True if the pane should request to be brought into view.</param>
		/// <param name="restoreMinimizedToolWindow">True if the containing ToolWindow should be restored from the minimized state before activating</param>
		/// <returns>True if it was able to move focus into the pane; false if the pane already had focus or couldn't get focus</returns>
		internal bool ActivateInternal(bool force, bool bringIntoView, bool restoreMinimizedToolWindow )
		{
            if (_isActivating)
                return this.IsKeyboardFocusWithinEx;

            try
            {
                _isActivating = true;
                return ActivateInternalImpl(force, bringIntoView, restoreMinimizedToolWindow);
            }
            finally
            {
                _isActivating = false;
            }
        }

        // AS 3/30/09 TFS16355 - WinForms Interop
        private bool ActivateInternalImpl(bool force, bool bringIntoView, bool restoreMinimizedToolWindow )
        {
            // AS 11/26/08 TFS8265
            // Only get this once.
            //
            bool focusable = this.Focusable;

			if (// AS 5/15/08 BR32753
				// Allow trying to activate even if there is no content as long as 
				// the pane is focusable.
				//
				//null != this.Content 	// there is an element in the pane
                (null != this.Content || focusable)
				// AS 5/15/08 BR32762
				// Instead of checking IsEnabled and the Visibility here, we should
				// just use the CanActivate property.
				//
				//&& this.IsEnabled 		// the element must be enabled
				&& this.CanActivate
				// AS 4/7/08
				//&& false == this.IsKeyboardFocusWithin
                // AS 1/30/09 TFS12993
				//&& (force || false == this.IsKeyboardFocusWithin) // focus isn't already within the pane
				&& (force || false == this.IsKeyboardFocusWithinEx) // focus isn't already within the pane
                )
			{
				// AS 5/15/08 BR32751
				// Moved this block up from below because activating a floating window
				// will reactivate its active pane which will bring it into view. So
				// our BringIntoView call that was done before the call to activating
				// the window will be trumped.
				//
				Window window = Window.GetWindow(this);

				// AS 10/14/10 TFS36740
				bool activated = false;

				// make sure the containing window is activated if possible
				if (null != window && BrowserInteropHelper.IsBrowserHosted == false)
				{
					try
					{
						// AS 10/20/09 TFS23992
						// Do not try to activate the window while the window is not shown yet or that 
						// may trigger another series of measures.
						//
						//if (window.IsActive == false)
						// AS 6/28/10 TFS32978
						//if (window.IsActive == false && window.IsVisible && window.IsLoaded)
						//    window.Activate();
						// AS 10/14/10 TFS36740
						// Changed to return a boolean as to whether the window is active.
						//
						activated = DockManagerUtilities.Activate(window);
					}
					catch
					{
					}
				}
				else
				{
					ToolWindow toolWindow = ToolWindow.GetToolWindow(this);

					if (null != toolWindow)
					{
						toolWindow.Activate();
						activated = true; // AS 10/14/10 TFS36740
					}
				}

				// AS 10/14/10 TFS36740
				// WPF has a bug where an HwndHost (e.g. WindowsFormsHost) within a Window has 
				// focus, you cannot focus an element within a popup. In our case, the flyout 
				// uses a Popup when you have an HwndHost otherwise the flyout would be under 
				// the HwndHost (since that is in a separate hwnd that would be above the normal 
				// wpf content). I submitted an issue in MS'Connect but we'll try to workaround 
				// this somewhat. Since we don't want to interfere with the focus process, we can 
				// only really assume that we can manipulate focus within the Activate method
				// itself. So if we were able to activate the owning window and there is still 
				// no focused element then we'll force focus in the dockmanager which should pull 
				// focus out of the hwndhost. Then when we call Focus below, the WPF framework 
				// will actually allow the element to get the input focus.
				//
				if (activated)
				{
					XamDockManager dm = XamDockManager.GetDockManager(this);

					// AS 11/30/11 TFS96843
					// The fix for 36740 was meant to workaround an issue where the pane was in the unpinned 
					// state so we'll reduce the scope of the change to only doing this for an unpinned pane.
					//
					//if (null != dm && Keyboard.FocusedElement == null)
					if (null != dm && Keyboard.FocusedElement == null && this.PaneLocation == PaneLocation.Unpinned)
					{
						dm.ForceFocus();
					}
				}

				// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
				if (restoreMinimizedToolWindow)
				{
					DockManagerUtilities.RestoreMinimizedToolWindow(this);

					// AS 3/31/11 TFS66115
					// if the pane is hosted within the dockmanager (i.e. not floating) then we'll also try 
					// to restore its containing window. Normally I would prefer not to manipulate the state 
					// of an element/object outside our elements but this makes a more consistent experience 
					// for an end user.
					//
					if (ToolWindow.GetToolWindow(this) == null &&
						this.PaneLocation != DockManager.PaneLocation.Unknown &&
						false == DockManagerUtilities.IsFloating(this.PaneLocation))
					{
						DockManagerUtilities.RestoreMinimizedWindow(Window.GetWindow(this));
					}
				}

				// make sure the pane is in view
				// AS 5/28/08
				// Added param to allow the pane to be activated without requesting to be brought into
				// view. This is really necessary when activating an unpinned pane.
				//
				Debug.Assert(bringIntoView || this.IsPinned == false);

				if (bringIntoView)
				{
					this.BringIntoView();

					// force an update layout so we can be brought into view
					this.UpdateLayout();
				}

				// first see if there is an element with focus within the focusscope. it could be
				// that another focusscope has the keyboard focus but we have an element within
				// this pane that is focused in which case we can return focus to that.

				DependencyObject focusScope = FocusManager.GetFocusScope(this);
				Debug.Assert(this == focusScope);
				IInputElement focusedElement = FocusManager.GetFocusedElement(focusScope ?? this);

                #region Commented out
				
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

                #endregion // Commented out

				// AS 12/1/11 TFS96843
				// See the comment on ClearDragHelperFocusedElement for details.
				//
				if (window != null)
				{
					var toolWindow = ToolWindow.GetToolWindow(this) as PaneToolWindow;

					if (toolWindow != null && toolWindow.Host != null && toolWindow.Host.IsWindow)
					{
						toolWindow.ClearDragHelperFocusedElement();
					}
				}

				// AS 6/12/12 TFS113795
				// When the pane is on a window that is disabled (e.g. it is on a modal window) we will 
				// not be able to shift focus so we should assume that we can focus it and move the pane 
				// to the front of the list.
				//
				var oldFocus = Keyboard.FocusedElement;

				// if there is no focused element within our focusscope or its not within us
				// then shift focus to the first child
				if (focusedElement == null
					|| focusedElement is DependencyObject == false
					|| false == Utilities.IsDescendantOf(this, (DependencyObject)focusedElement))
				{
                    // AS 11/26/08 TFS8265
                    // Previously we were first trying to give focus to an element within the 
                    // pane but that could be an hwndhost in which case focus gets shifted into 
                    // the control within that. This can cause the wpf framework to cache the 
                    // current focused element as the element to return focus to when the window 
                    // hwnd regains focus. To avoid them caching the last pane/element, we will 
                    // now first focus the content pane (assuming it is focusable which it is by 
                    // default) and then if it got focus or if its not focusable then we'll shift
                    // focus within the pane.
                    //
                    //TraversalRequest request = new TraversalRequest(FocusNavigationDirection.First);
                    //if (this.MoveFocus(request))
                    //    return true;
                    //
                    //return this.Focus();
					// AS 6/14/12 TFS99504
                    //bool focused = focusable && this.Focus();
                    bool focused = focusable && this.FocusElementDuringActivate(this);

                    if (focusable == false || focused)
                    {
                        // then assuming we were able to give focus to the cp or it was not focusable
                        // shift focus into the pane
                        TraversalRequest request = new TraversalRequest(FocusNavigationDirection.First);
                        if (this.MoveFocus(request))
                            return true;

                        return focused;
                    }
				}
				else
				{
                    // AS 11/26/08 TFS10656
                    // We may not be able to focus the last focused element. It may have been 
                    // collapsed so if it fails and fails because the element was not visible
                    // then try to activate the next element.
                    //
					//return focusedElement.Focus();
					// AS 6/14/12 TFS99504
                    //if (focusedElement.Focus())
					if (this.FocusElementDuringActivate(focusedElement))
                        return true;

                    // if it failed and the element wasn't visible...
                    DependencyObject focusedObject = focusedElement as DependencyObject;

					// AS 9/9/09 TFS21028
					// We also want to shift focus if the element we wanted to focus was disabled.
					//
                    //if (null != focusedObject && false.Equals(focusedObject.GetValue(UIElement.IsVisibleProperty)))
					bool shiftFocus = false;

					if (null != focusedObject)
					{
						if (false.Equals(focusedObject.GetValue(UIElement.IsVisibleProperty)) ||
							false.Equals(focusedObject.GetValue(UIElement.IsEnabledProperty)))
							shiftFocus = true;
					}

                    if (shiftFocus)
                    {
                        // try to focus another sibling element after the previously focused one
                        if (DockManagerUtilities.MoveFocus(focusedObject, new TraversalRequest(FocusNavigationDirection.Next)))
                        {
                            // there seems to be some strange logic in wpf's movefocus for next
                            // so if it shifts focus outside then try to take it back
                            // AS 1/30/09 TFS12993
                            // This isn't part of this specific issue but just as 
                            // IsKeyboardFocusWithin could have been false causing
                            // us to get into this routine, IsKeyboardFocusWithin 
                            // may be false even though focus is within the pane
                            // but within a popup or within an hwnd host.
                            //
                            //if (this.IsKeyboardFocusWithin)
                            if (this.IsKeyboardFocusWithinEx)
                                return true;
                        }

                        // if that fails then focus the pane itself
						// AS 6/12/12 TFS113795
						// Let it continue on if focus returns false so we can do the processing below.
						//
						//return this.Focus();
						// AS 6/14/12 TFS99504
						//if (this.Focus())
						if (this.FocusElementDuringActivate(this))
							return true;
                    }
				}

				// AS 6/12/12 TFS113795
				// When the pane is on a window that is disabled (e.g. it is on a modal window) we will 
				// not be able to shift focus so we should assume that we can focus it and move the pane 
				// to the front of the list.
				//
				var currentFocus = Keyboard.FocusedElement;

				if (oldFocus == currentFocus && this.IsVisible && this.IsEnabled && DockManagerUtilities.IsInDisabledWindow(window))
				{
					var dm = XamDockManager.GetDockManager(this);

					if (null != dm && dm.ActivePane == null)
					{
						// move the pane to the head of the active pane list
						dm.ActivePaneManager.MoveToFront(this);

						// also if the pane is within the DCH then it wouldn't have become the active document
						// since focus hadn't changed so we should handle that
						if (this.PaneLocation == DockManager.PaneLocation.Document)
							dm.ActivePaneManager.SetActiveDocument(this);
					}
				}
			}

			// AS 8/21/09 TFS21171
			// This should have been updated with the changes for TFS12993 to ensure 
			// we return true when we contain an HwndHost that contains focus.
			//
			//return this.IsKeyboardFocused || this.IsKeyboardFocusWithin;
			return this.IsKeyboardFocusWithinEx;
		} 
		#endregion //Activate

		#region ChangeToDockable
		internal bool ChangeToDockable()
		{
			IContentPaneContainer currentContainer = this.PlacementInfo.CurrentContainer;
			IContentPaneContainer newContainer = this.PlacementInfo.DockableContainer;
			SplitPane newSplitPane = null;

			if (newContainer == currentContainer)
				return false;

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null == dockManager)
				return false;

			// AS 5/15/08 BR32636
			// If its already dockable then don't do anything.
			//
			//Debug.Assert(DockManagerUtilities.IsDockable(currentContainer.PaneLocation) == false);
			if (DockManagerUtilities.IsDockable(this.PaneLocation))
				return false;

			// AS 6/6/08
			// If the pane is floatingonly and the last dockable container was docked
			// then try to make it floating - either using the last floating container
			// if it had one (yes this differs slightly from vs but I think its more
			// consistent to keep it where it was when it was floating last) or put it 
			// into a new floating containing if it supports being a floating dockable
			// container.
			//
			if (DockManagerUtilities.IsFloating(this.PaneLocation) &&
				newContainer != null &&
				DockManagerUtilities.IsDocked(newContainer.PaneLocation))
			{
				if (this.PlacementInfo.FloatingDockablePlaceholder != null)
				{
					newContainer = this.PlacementInfo.FloatingDockablePlaceholder.Container;
				}
				else if (this.IsDockingAllowed(PaneLocation.Floating))
				{
					newContainer = null;
				}
			}

			// AS 7/7/11 TFS80889
			// If we have a previous dockable container but we cannot go to that position because 
			// the result would be hidden then use the other container if allowed.
			//
			if (newContainer != null && !dockManager.IsPaneLocationVisible(newContainer.PaneLocation))
			{
				if (newContainer.PaneLocation == PaneLocation.Floating)
				{
					// use the docked container
					newContainer = this.PlacementInfo.DockedContainer;

					// if the other location isn't allowed either then don't do anything
					if (newContainer != null && !dockManager.IsPaneLocationVisible(newContainer.PaneLocation))
						return false;
				}
				else
				{
					if (!dockManager.IsPaneLocationVisible(PaneLocation.Floating))
						return false;

					if (this.PlacementInfo.FloatingDockablePlaceholder != null)
						newContainer = this.PlacementInfo.FloatingDockablePlaceholder.Container;
					else
						newContainer = null;
				}
			}

			if (newContainer == null)
			{
				// AS 4/29/08 BR32013
				// I guess we will need to make the decision where to put the pane. If it allows floating
				// then the decision is simple but if it doesn't then we will just choose one of the allowed
				// dockable locations.
				// // we decide that rather than arbitrarily choosing to create a split pane and either
				// // float it (which may not be allowed by the pane) or docked it (in which case we have
				// // to choose where to position the split pane), we would just ignore this operation
				// return false;

				// if the pane hasn't been floating then we need to create a new 
				// tool window to host the floating only pane
				newSplitPane = DockManagerUtilities.CreateSplitPane(dockManager);
				InitialPaneLocation initialLocation = InitialPaneLocation.DockableFloating;

				if (this.IsDockingAllowed(PaneLocation.Floating))
				{
					// AS 4/25/08
					// If possible try to size the floating pane based on where it was on
					// the screen last and how large it was in that floating window.
					//
					// AS 5/2/08 BR32056
					// Moved to a helper routine - also allows us to use the current size
					// as the basic for the floating size.
					//
					//if (false == this.LastFloatingSize.IsEmpty)
					//	XamDockManager.SetFloatingSize(newSplitPane, this.LastFloatingSize);
					XamDockManager.SetFloatingSize(newSplitPane, this.GetSizeForFloating());

					if (null != this.LastFloatingLocation)
						XamDockManager.SetFloatingLocation(newSplitPane, this.LastFloatingLocation);
				}
				else
				{
					// find an allowed starting location...
					if (this.IsDockingAllowed(PaneLocation.DockedLeft))
						initialLocation = InitialPaneLocation.DockedLeft;
					else if (this.IsDockingAllowed(PaneLocation.DockedBottom))
						initialLocation = InitialPaneLocation.DockedBottom;
					else if (this.IsDockingAllowed(PaneLocation.DockedRight))
						initialLocation = InitialPaneLocation.DockedRight;
					else if (this.IsDockingAllowed(PaneLocation.DockedTop))
						initialLocation = InitialPaneLocation.DockedTop;
					else
						return false;
				}

				XamDockManager.SetInitialLocation(newSplitPane, initialLocation);

				newContainer = newSplitPane;
			}

			// AS 4/28/11 TFS73532
			// Reorganized so the splitpane is in the tree and if it is to be floating the 
			// pane is already visible if possible.
			//
			//// AS 10/15/08 TFS8068
			//// If we're moving the panes into a new split, we need the split to carry 
			//// the data context temporarily.
			////
			//IDisposable replacement = null;
			//
			//if (null != newSplitPane)
			//    replacement = DockManagerUtilities.CreateMoveReplacement(newSplitPane, this);
			//
			//DockManagerUtilities.MovePane(this, newContainer, null, newContainer.PaneLocation);
			//
			//// if this is a new floating window then we can add it to the dockmanager once we have
			//// the pane inside it
			//if (null != newSplitPane)
			//    dockManager.Panes.Add(newSplitPane);
			//
			//// AS 10/15/08 TFS8068
			//if (null != replacement)
			//    replacement.Dispose();
			using (MovePaneHelper moveHelper = new MovePaneHelper(dockManager, newContainer, this))
			{
				// first if we created a split pane then add it to the dockmanager so it can be 
				// put into the tree - possibly creating a toolwindow
				if (null != newSplitPane)
					dockManager.Panes.Add(newSplitPane);

				// lastly we can move the pane into the new container
				DockManagerUtilities.MovePane(this, newContainer, null, newContainer.PaneLocation, moveHelper);
			}

            // AS 5/21/08
            DockManagerUtilities.RemoveContainerIfNeeded(currentContainer);

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			//this.ActivateInternal(true);
			this.ActivateInternal(true, true, true);
			return true;
		}
		#endregion //ChangeToDockable

		#region ChangeToDocument
		internal bool ChangeToDocument()
		{
			// AS 5/15/08 BR32636
			// Using the menu item this is possible so don't bother with the assert.
			//
			//Debug.Assert(this.IsDocument == false);

			if (this.IsDocument)
				return false;

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null == dockManager)
				return false;

			DocumentContentHost host = dockManager.DocumentContentHost;
			IContentPaneContainer currentContainer = this.PlacementInfo.CurrentContainer;
			IContentPaneContainer newContainer = host.GetContainerForPane(this, true);

			if (null == newContainer)
				return false;

			Debug.Assert(newContainer.PaneLocation == PaneLocation.Document);
			DockManagerUtilities.MovePane(this, newContainer, 0, newContainer.PaneLocation);
			// AS 5/21/08
			DockManagerUtilities.RemoveContainerIfNeeded(currentContainer);
			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			//this.ActivateInternal(true);
			this.ActivateInternal(true, true, true);
			return true;
		}
		#endregion //ChangeToDocument

		#region ChangeToFloatingOnly
		internal bool ChangeToFloatingOnly()
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null == dockManager)
				return false;

			// AS 5/15/08 BR32636
			if (this.PaneLocation == PaneLocation.FloatingOnly)
				return false;

			ContentPane.PanePlacementInfo placement = this.PlacementInfo;
			IContentPaneContainer currentContainer = placement.CurrentContainer;
			ContentPanePlaceholder floatingPlaceholder = placement.FloatingOnlyPlaceholder;
			IContentPaneContainer newContainer = null;
			SplitPane newSplitPane = null;

			if (null != floatingPlaceholder)
				newContainer = floatingPlaceholder.Container;
			else
			{
				// if the pane hasn't been floating then we need to create a new 
				// tool window to host the floating only pane
				newSplitPane = DockManagerUtilities.CreateSplitPane(dockManager);
				XamDockManager.SetInitialLocation(newSplitPane, InitialPaneLocation.FloatingOnly);

				// AS 4/25/08
				// If possible try to size the floating pane based on where it was on
				// the screen last and how large it was in that floating window.
				//
				// AS 5/2/08 BR32290
				// We should choose a reasonable default for the floating size. I'm going to 
				// use the same default size we use when dragging an element into the floating
				// position.
				//
				//if (false == this.LastFloatingSize.IsEmpty)
				//	XamDockManager.SetFloatingSize(newSplitPane, this.LastFloatingSize);
				// AS 5/2/08 BR32056
				// Moved to a helper routine.
				//
				Size floatingSize = this.GetSizeForFloating();

				XamDockManager.SetFloatingSize(newSplitPane, floatingSize);

				if (null != this.LastFloatingLocation)
					XamDockManager.SetFloatingLocation(newSplitPane, this.LastFloatingLocation);

				newContainer = newSplitPane;
			}

			if (null == newContainer)
				return false;

			// AS 4/28/11 TFS73532
			// Reorganized so the splitpane is in the tree and if it is to be floating the 
			// pane is already visible if possible.
			//
			//// AS 10/15/08 TFS8068
			//// If we're moving the panes into a new split, we need the split to carry 
			//// the data context temporarily.
			////
			//IDisposable replacement = null;
			//
			//if (null != newSplitPane)
			//    replacement = DockManagerUtilities.CreateMoveReplacement(newSplitPane, this);
			//
			//DockManagerUtilities.MovePane(this, newContainer, null, newContainer.PaneLocation);
			//
			//// if this is a new floating window then we can add it to the dockmanager once we have
			//// the pane inside it
			//if (null != newSplitPane)
			//    dockManager.Panes.Add(newSplitPane);
			//
			//// AS 10/15/08 TFS8068
			//if (null != replacement)
			//    replacement.Dispose();
			using (MovePaneHelper moveHelper = new MovePaneHelper(dockManager, newContainer, this))
			{
				if (null != newSplitPane)
					dockManager.Panes.Add(newSplitPane);

				DockManagerUtilities.MovePane(this, newContainer, null, newContainer.PaneLocation, moveHelper);
			}

            // AS 5/21/08
            DockManagerUtilities.RemoveContainerIfNeeded(currentContainer);

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			//this.ActivateInternal(true);
			this.ActivateInternal(true, true, true);
			return true;
		}
		#endregion //ChangeToFloatingOnly

        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        #region ClearHeader

        // Keep track of the paneHeader so we can let it know to Update its vsiual state when the
        // pane's IsActive or IsActiveDocument properties change
        internal void ClearHeader(PaneHeaderPresenter header)
        {
            if ( this.Header == header )
                this._paneHeaderPresenter = null;
        }

        #endregion //ClearHeader

		#region GetSizeForFloating
		// AS 5/2/08 BR32056
		// Moved this to a helper routine since there are several cases where we need this size.
		//
		internal Size GetSizeForFloating()
		{
			Size floatingSize = this.LastFloatingSize;

			if (floatingSize.IsEmpty)
				floatingSize = new Size(this.ActualWidth, this.ActualHeight);

			return floatingSize;
		}
		#endregion //GetSizeForFloating

        // AS 2/9/09 TFS13375
        #region GetFocusedHwndHost
        internal FrameworkElement GetFocusedHwndHost()
        {
            HwndHostInfo hhi = HwndHostInfo.GetHwndHost(this);

            return null != hhi ? hhi.GetFocusedHwndHost() : null;
        } 
        #endregion //GetFocusedHwndHost

        // AS 11/25/08 TFS8265
        #region HasFocusInHwndHost
        internal bool HasFocusInHwndHost()
        {
            HwndHostInfo hhi = HwndHostInfo.GetHwndHost(this);

            return null != hhi && hhi.HasFocusWithin();
        }
        #endregion //HasFocusInHwndHost

        // AS 10/13/08 TFS6032
        #region HideAllHwndHosts
        internal void HideAllHwndHosts()
        {
            HwndHostInfo hhi = HwndHostInfo.GetHwndHost(this);

            if (null != hhi)
                hhi.HideAll();
        } 
        #endregion //HideAllHwndHosts

		#region IsDockingAllowed
		internal bool IsDockingAllowed(PaneLocation location)
		{
			// the current location is valid regardless
			if (location == XamDockManager.GetPaneLocation(this))
				return true;

			var dm = XamDockManager.GetDockManager(this);

			// AS 7/7/11 TFS80889
			// Just like we suppress dragging into a floating position when the floating windows 
			// are collapsed/hidden we should do the same when toggling the state.
			//
			if (dm != null && !dm.IsPaneLocationVisible(location))
				return false;

			DependencyProperty prop = null;

			switch (location)
			{
				case PaneLocation.DockedBottom:
					prop = AllowDockingBottomProperty;
					break;
				case PaneLocation.DockedLeft:
					prop = AllowDockingLeftProperty;
					break;
				case PaneLocation.DockedRight:
					prop = AllowDockingRightProperty;
					break;
				case PaneLocation.DockedTop:
					prop = AllowDockingTopProperty;
					break;
				case PaneLocation.Floating:
					prop = AllowDockingFloatingProperty;
					break;
				default:
					Debug.Fail("Invalid PaneLocation");
					return false;
			}

			return (bool?)this.GetValue(prop) ?? this.AllowDocking;
		} 
		#endregion //IsDockingAllowed

        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        #region InitializeHeader

        // Keep track of the paneHeader so we can let it know to Update its vsiual state when the
        // pane's IsActive or IsActiveDocument properties change
        internal void InitializeHeader(PaneHeaderPresenter header)
        {
            this._paneHeaderPresenter = new WeakReference(header);
        }

        #endregion //InitializeHeader

        #region InitializeMenu
        /// <summary>
		/// Helper method for populating a ContextMenu or MenuItem's Items for the pane based on its current state.
		/// </summary>
		/// <param name="items">The collection to populate</param>
		internal void InitializeMenu(ItemCollection items)
		{
			bool isDocument = this.IsDocument;
			int preDocumentCount = items.Count;

			if (isDocument)
			{
				this.AddMenuItem(PaneMenuItem.Close, items);
				this.AddMenuItem(PaneMenuItem.CloseAllButThis, items);
			}

			int postDocumentCount = items.Count;

			this.AddMenuItem(PaneMenuItem.FloatingOnly, items);
			this.AddMenuItem(PaneMenuItem.Dockable, items);
			this.AddMenuItem(PaneMenuItem.Document, items);
			this.AddMenuItem(PaneMenuItem.AutoHide, items);
			this.AddMenuItem(PaneMenuItem.Hide, items);

			int postPaneItemCount = items.Count;

			if (isDocument)
			{
				this.AddMenuItem(PaneMenuItem.NewHorizontalTabGroup, items);
				this.AddMenuItem(PaneMenuItem.NewVerticalTabGroup, items);

				// i would have thought previous would be before next but this is what vs does
				this.AddMenuItem(PaneMenuItem.NextTabGroup, items);
				this.AddMenuItem(PaneMenuItem.PreviousTabGroup, items);

				int postNewGroupCount = items.Count;

				// if we added document items after the pane items add another separator
				// note: add the second one first since inserting the other first will cause
				// these indexes to be wrong
				if (postDocumentCount != postPaneItemCount && postPaneItemCount != postNewGroupCount)
					this.InsertMenuItem(PaneMenuItem.Separator, postPaneItemCount, items);

				// if we added document items and pane items put a separator between them
				// AS 5/29/08
				// A separator wasn't added if there were no "middle" items.
				//
				//if (preDocumentCount != postDocumentCount && postDocumentCount != postPaneItemCount)
				if (preDocumentCount != postDocumentCount && postDocumentCount != postNewGroupCount)
					this.InsertMenuItem(PaneMenuItem.Separator, postDocumentCount, items);
			}

			// raise an event to let the programmer add to/remove items
			this.RaiseOptionsMenuOpening(new PaneOptionsMenuOpeningEventArgs(items));
		} 
		#endregion //InitializeMenu

        #region OnPaneContextMenuOpening

        internal static void OnPaneContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			FrameworkElement source = sender as FrameworkElement;
			ContentPane pane;

			if (source is PaneTabItem)
			{
				pane = ((PaneTabItem)source).Pane;

				// AS 5/2/08 BR32007
				// We don't want a content menu for the pane tab item.
				//
				if (pane.PaneLocation == PaneLocation.Unpinned)
					return;
			}
			else if (source is PaneHeaderPresenter)
				pane = ((PaneHeaderPresenter)source).Pane;
			else
				pane = null;

			Debug.Assert(null != pane);

			if (null != source
				&& null != pane
				&& source.ContextMenu == null
				// AS 7/16/09 TFS19263
				// It is not possible for the customer to provide a local value for the PaneHeaderPresenter
				// or the PaneTabItem since those are generated either by the TabGroupPane or in the template 
				// for the ContentPane.
				//
				//&& source.ReadLocalValue(ContextMenuProperty) != null
				&& DependencyPropertyHelper.GetValueSource(source, ContextMenuProperty).BaseValueSource < BaseValueSource.DefaultStyle
				// 5/2/08 BR32007
				// We do want a context menu for the unpinned pane caption.
				//
				//&& XamDockManager.GetPaneLocation(pane) != PaneLocation.Unpinned
				)
			{
				ContextMenu menu = new ContextMenu();

				menu.SetValue(DefaultStyleKeyProperty, XamDockManager.ContextMenuStyleKey);
				menu.PlacementTarget = source;
				menu.Placement = PlacementMode.RelativePoint;

				Point pt = new Point();

				DependencyObject relativeElement = e.OriginalSource as DependencyObject;

				while (relativeElement is ContentElement)
					relativeElement = LogicalTreeHelper.GetParent(relativeElement);

				if (relativeElement is UIElement)
					pt = source.TranslatePoint(pt, (UIElement)relativeElement);

				menu.HorizontalOffset = e.CursorLeft - pt.X;
				menu.VerticalOffset = e.CursorTop - pt.Y;

				pane.InitializeMenu(menu.Items);

				if (menu.Items.Count > 0)
				{
					// AS 5/23/11 TFS73513
					// See the notes in the ToolWindow->ShowContextMenu for details.
					//
					menu.Unloaded += delegate(object sender2, RoutedEventArgs e2)
					{
						((ContextMenu)sender2).PlacementTarget = null;
					};

					menu.IsOpen = true;
					e.Handled = true;
				}
			}
		}

		#endregion //OnPaneContextMenuOpening

        // AS 10/13/08 TFS6032
        #region VerifyContentVisibility
        internal void VerifyContentVisibility()
        {
            HwndHostInfo hhi = HwndHostInfo.GetHwndHost(this);

            if (null != hhi &&
                hhi.HasHwndHost &&
                DockManagerPanel.UnpinnedPaneContainer.GetIsHiddenUnpinned(this))
            {
                this.SetValue(ContentVisibilityPropertyKey, KnownBoxes.VisibilityCollapsedBox);
            }
            else
                this.ClearValue(ContentVisibilityPropertyKey);
        }
        #endregion //VerifyContentVisibility

        #region VerifyHeaderVisibility
		/// <summary>
		/// Updates the value of the <see cref="HeaderVisibilityProperty"/> based on the current state
		/// </summary>
		internal void VerifyHeaderVisibility()
		{
			object visibility = DependencyProperty.UnsetValue;
			object isProvidingFloatingCaption = DependencyProperty.UnsetValue; // AS 6/24/11 FloatingWindowCaptionSource

			if (this.IsDocument)
			{
				// do not show the caption area when in the document area - the tab items only are shown
				visibility = KnownBoxes.VisibilityCollapsedBox;
			}
			// AS 6/24/11 FloatingWindowCaptionSource
			//else if (PaneToolWindow.GetSinglePaneToolWindow(this) != null)
			else
			{
				var tw = PaneToolWindow.GetSinglePaneToolWindow(this);

				if (tw != null)
				{
					// do not show the pane header if it is the only visible pane 
					// the tool window. in that case, the title of the toolwindow
					// contains the caption
					if (tw.FloatingWindowCaptionSource == FloatingWindowCaptionSource.UseToolWindowTitle)
						visibility = KnownBoxes.VisibilityCollapsedBox;
					else
						isProvidingFloatingCaption = KnownBoxes.TrueBox;
				}
			}

			this.SetValue(HeaderVisibilityPropertyKey, visibility);
			this.SetValue(IsProvidingFloatingWindowCaptionPropertyKey, isProvidingFloatingCaption); // AS 6/24/11 FloatingWindowCaptionSource
		}
		#endregion //VerifyHeaderVisibility

		#endregion //Internal Methods

		#region Private Methods

		#region AddMenuItem
		private void AddMenuItem(PaneMenuItem menuInfo, ItemCollection items)
		{
			this.InsertMenuItem(menuInfo, items.Count, items);
		}
		#endregion //AddMenuItem

		// AS 9/9/09 TFS21111
		#region CoerceFocusedElement
		private static object CoerceFocusedElement(DependencyObject d, object newValue)
		{
			if (newValue == d)
				return null;

			return newValue;
		}
		#endregion //CoerceFocusedElement

		#region CreateMenuItem

		private MenuItem CreateMenuItem(RoutedCommand command, string resourceName, bool isCheckable, bool isChecked)
		{
			MenuItem mi = new MenuItem();

			if (isCheckable)
			{
				mi.IsCheckable = isCheckable;
				mi.IsChecked = isChecked;
			}

			mi.Command = command;
			mi.CommandTarget = this;
			// AS 3/25/08
			// See notes in TabGroupPane.OnSubmenuOpened
			//
			mi.CommandParameter = this;
			mi.Header = XamDockManager.GetString(resourceName);
			mi.SetValue(DefaultStyleKeyProperty, XamDockManager.MenuItemStyleKey);

			return mi;
		}

		#endregion //CreateMenuItem

		#region DisposeMouseDownFocusWatcher
		private void DisposeMouseDownFocusWatcher()
		{
			if (null != this._mouseDownFocusWatcher)
			{
				this._mouseDownFocusWatcher.FocusedElementChanged -= new RoutedPropertyChangedEventHandler<IInputElement>(this.OnMouseDownFocusChanged);
				this._mouseDownFocusWatcher.Dispose();
				this._mouseDownFocusWatcher = null;
			}
		}
		#endregion // DisposeMouseDownFocusWatcher

		#region ExecuteCommandImpl

		private bool ExecuteCommandImpl(ExecuteCommandInfo commandInfo)
		{
			RoutedCommand command = commandInfo.RoutedCommand;

			// Make sure we have a command to execute.
			DockManagerUtilities.ThrowIfNull(command, "command");

			// Make sure the minimal control state exists to execute the command.
			if (ContentPaneCommands.IsMinimumStatePresentForCommand(this as ICommandHost, command) == false)
				return false;

			// make sure the command can be executed
			if (false == ((ICommandHost)this).CanExecute(commandInfo))
				return false;

			// AS 5/2/08 BR31999
			if (this.PaneLocation == PaneLocation.Unknown)
				return false;

			// Fire the 'before executed' cancelable event.
			ExecutingCommandEventArgs beforeArgs = new ExecutingCommandEventArgs(command);
			bool proceed = this.RaiseExecutingCommand(beforeArgs);

			if (proceed == false)
            {
                // JJD 06/02/10 - TFS33112
                // Return the inverse of ContinueKeyRouting so that the developer can prevent
                // the original key message from bubbling
                //return false;
                return !beforeArgs.ContinueKeyRouting;
            }


			// Setup some info needed by more than 1 command.
			bool shiftKeyDown = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
			bool ctlKeyDown = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
			bool tabKeyDown = Keyboard.IsKeyDown(Key.Tab);



			// =========================================================================================
			// Determine which of our supported commands should be executed and do the associated action.
			bool handled = false;

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null != dockManager)
			{
				if (command == ContentPaneCommands.ActivatePane)
				{
					handled = this.ActivateInternal();
				}
				else if (command == ContentPaneCommands.TogglePinnedState)
				{
					if (this.CanTogglePinnedState)
					{
						dockManager.UnpinPane(this, true);
						handled = true;
					}
				}
				else if (command == ContentPaneCommands.ChangeToDockable)
				{
					handled = this.ChangeToDockable();
				}
				else if (command == ContentPaneCommands.ChangeToDocument)
				{
					handled = this.ChangeToDocument();
				}
				else if (command == ContentPaneCommands.ChangeToFloatingOnly)
				{
					handled = this.ChangeToFloatingOnly();
				}
				else if (command == ContentPaneCommands.Close)
				{
					dockManager.ClosePane(this, true);
					handled = true;
				}
				else if (command == ContentPaneCommands.CloseAllButThis)
				{
					dockManager.CloseAllButThis(this);
					handled = true;
				}
				else if (command == ContentPaneCommands.MoveToNewHorizontalGroup ||
					command == ContentPaneCommands.MoveToNewVerticalGroup)
				{
					Orientation newGroupOrientation = command == ContentPaneCommands.MoveToNewHorizontalGroup
						? Orientation.Horizontal
						: Orientation.Vertical;

					handled = XamDockManager.MoveToNewGroup(this, newGroupOrientation);
				}
				else if (command == ContentPaneCommands.MoveToNextGroup ||
					command == ContentPaneCommands.MoveToPreviousGroup)
				{
					handled = dockManager.MoveToNextGroup(this, command == ContentPaneCommands.MoveToNextGroup);
				}
				else if (command == ContentPaneCommands.ToggleDockedState)
				{
					handled = dockManager.ToggleDockedState(this);
				}
				else if (command == ContentPaneCommands.FlyIn)
				{
					dockManager.HideFlyout(this, false, true, true, false);
					handled = true;
				}
				else if (command == ContentPaneCommands.Flyout)
				{
					dockManager.ShowFlyout(this, false, false);
					handled = true;
				}
				else
				{
					Debug.Fail("Unexpected command:" + command.Name);
				}
			}

			// =========================================================================================

			//PostExecute:
			// If the command was executed, fire the 'after executed' event.
			if (handled == true)
				this.RaiseExecutedCommand(new ExecutedCommandEventArgs(command));


			return handled;
		}

		#endregion //ExecuteCommandImpl

		// AS 6/14/12 TFS99504
		#region FocusElementDuringActivate
		private bool FocusElementDuringActivate(IInputElement element)
		{
			if (null == element)
				return false;

			var dm = XamDockManager.GetDockManager(this);
			bool wasActivating = dm != null ? dm.IsActivatingPane : true;

			if (!wasActivating)
				dm.IsActivatingPane = true;

			try
			{
				return element.Focus();
			}
			finally
			{
				if (!wasActivating)
					dm.IsActivatingPane = false;
			}
		}
		#endregion //FocusElementDuringActivate

		#region InsertMenuItem
		private void InsertMenuItem(PaneMenuItem menuInfo, int index, ItemCollection items)
		{
			// AS 5/29/08
			// Refactored this method so that the basic criteria for whether a menu item should
			// be included is in a separate method since some menu items will not show if others
			// are not visible. E.g. Document would not be Shown if Dockable, Floating and AutoHide
			// are not.
			//
			if (false == this.ShouldIncludeMenuItem(menuInfo))
				return;

			object mi = null;

			switch (menuInfo)
			{
				case PaneMenuItem.AutoHide:
				{
					mi = CreateMenuItem(ContentPaneCommands.TogglePinnedState, "PinPaneMenuItem", true, XamDockManager.GetPaneLocation(this) == PaneLocation.Unpinned);
					break;
				}
				case PaneMenuItem.Hide:
				{
					// AS 5/29/08
					// if this allows close but is a document that doesn't allow any other state (dockable, floating only, etc.)
					// then do not include the hide menu item - the close will be sufficient
					if (this.IsDocument == false 
						|| this.ShouldIncludeMenuItem(PaneMenuItem.Dockable) 
						|| this.ShouldIncludeMenuItem(PaneMenuItem.FloatingOnly) 
						|| this.ShouldIncludeMenuItem(PaneMenuItem.AutoHide))
						mi = CreateMenuItem(ContentPaneCommands.Close, "HidePaneMenuItem", false, false);
					break;
				}
				case PaneMenuItem.FloatingOnly:
				{
					// AS 5/29/08
					// only if it supports one of the other states
					if (this.ShouldIncludeMenuItem(PaneMenuItem.Dockable) 
						|| this.ShouldIncludeMenuItem(PaneMenuItem.Document) 
						|| this.ShouldIncludeMenuItem(PaneMenuItem.AutoHide))
						mi = CreateMenuItem(ContentPaneCommands.ChangeToFloatingOnly, "FloatingPaneMenuItem", true, this.PaneLocation == PaneLocation.FloatingOnly);
					break;
				}
				case PaneMenuItem.Dockable:
				{
					// only if it supports one of the other states
					// only if it supports one of the other states
					if (this.ShouldIncludeMenuItem(PaneMenuItem.FloatingOnly)
						|| this.ShouldIncludeMenuItem(PaneMenuItem.Document)
						|| this.ShouldIncludeMenuItem(PaneMenuItem.AutoHide))
						mi = CreateMenuItem(ContentPaneCommands.ChangeToDockable, "DockablePaneMenuItem", true, this.IsDockable);

					break;
				}
				case PaneMenuItem.Document:
				{
					// only if it supports one of the other states
					// only if it supports one of the other states
					if (this.ShouldIncludeMenuItem(PaneMenuItem.FloatingOnly)
						|| this.ShouldIncludeMenuItem(PaneMenuItem.Dockable)
						|| this.ShouldIncludeMenuItem(PaneMenuItem.AutoHide))
						mi = CreateMenuItem(ContentPaneCommands.ChangeToDocument, "DocumentPaneMenuItem", true, this.IsDocument);
					break;
				}
				case PaneMenuItem.Close:
				{
					mi = CreateMenuItem(ContentPaneCommands.Close, "ClosePaneMenuItem", false, false);
					break;
				}
				case PaneMenuItem.CloseAllButThis:
				{
					// get the parent container and see if it has any closable panes. this differs from
					// vs in that they always show the option but disable it if there is only 1 document
					// we can't really do that since without other panes, we do not know if there would
					// ever be any that have their AllowClose set to true.
					mi = CreateMenuItem(ContentPaneCommands.CloseAllButThis, "CloseAllButThisPaneMenuItem", false, false);
					break;
				}
				case PaneMenuItem.Separator:
				{
					Separator sep = new Separator();
					sep.SetValue(DefaultStyleKeyProperty, XamDockManager.MenuItemSeparatorStyleKey);
					sep.SetResourceReference(StyleProperty, XamDockManager.MenuItemSeparatorStyleKey);
					mi = sep;
					break;
				}
				case PaneMenuItem.NewVerticalTabGroup:
				{
					mi = CreateMenuItem(ContentPaneCommands.MoveToNewVerticalGroup, "MoveToNewVerticalGroupPaneMenuItem", false, false);
					break;
				}
				case PaneMenuItem.NewHorizontalTabGroup:
				{
					mi = CreateMenuItem(ContentPaneCommands.MoveToNewHorizontalGroup, "MoveToNewHorizontalGroupPaneMenuItem", false, false);
					break;
				}
				case PaneMenuItem.NextTabGroup:
				{
					mi = CreateMenuItem(ContentPaneCommands.MoveToNextGroup, "MoveToNextGroupPaneMenuItem", false, false);
					break;
				}
				case PaneMenuItem.PreviousTabGroup:
				{
					mi = CreateMenuItem(ContentPaneCommands.MoveToPreviousGroup, "MoveToPreviousGroupPaneMenuItem", false, false);
					break;
				}
				default:
					Debug.Fail("Unrecognized menu item:" + menuInfo.ToString());
					break;
			}

			if (null != mi)
				items.Insert(index, mi);
		} 
		#endregion //InsertMenuItem

		// AS 9/8/09 TFS21921
		// Extracted helper method from OnPreviousLostKeyboardFocus.
		//
		#region IsLosingFocusToRootVisual
		private static bool IsLosingFocusToRootVisual(KeyboardFocusChangedEventArgs e)
		{
			bool isLosingFocusToRootVisual = false;

			Visual oldFocus = e.OldFocus as Visual;

			if (oldFocus != null)
			{
				DependencyObject newFocus = e.NewFocus as DependencyObject;

				if (newFocus is Visual || newFocus is System.Windows.Media.Media3D.Visual3D)
				{
					if (VisualTreeHelper.GetParent(newFocus) == null)
					{
						if (newFocus is Window && Window.GetWindow(oldFocus) == newFocus)
							isLosingFocusToRootVisual = true;
						else if (oldFocus.IsDescendantOf(newFocus))
							isLosingFocusToRootVisual = true;
					}
				}
			}

			return isLosingFocusToRootVisual;
		}
		#endregion //IsLosingFocusToRootVisual

		#region OnMouseDown
		private static void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			ContentPane pane = sender as ContentPane;

			// if we created a focus watcher in the preview mouse down and we still have it
			// then focus has not changed in the interim and we can release that and activate
			// the pane
			if (pane._mouseDownFocusWatcher != null)
			{
				pane.DisposeMouseDownFocusWatcher();
				pane.ActivateInternal();
			}
		}
		#endregion //OnMouseDown

		#region OnMouseDownFocusChanged
		private void OnMouseDownFocusChanged(object sender, RoutedPropertyChangedEventArgs<IInputElement> e)
		{
			this.DisposeMouseDownFocusWatcher();
		}
		#endregion //OnMouseDownFocusChanged

		// AS 3/26/10 TFS30153 - PaneLocation Optimization
		#region OnPaneLocationChanged
		private static void OnPaneLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ContentPane pane = d as ContentPane;

			pane.VerifyHeaderVisibility();
			pane.VerifyIsPinnedState();

			XamDockManager dockManager = XamDockManager.GetDockManager(pane);

			if (null != dockManager)
				dockManager.ActivePaneManager.OnPaneLocationChanged(pane, (PaneLocation)e.OldValue, (PaneLocation)e.NewValue);


            // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
            pane.UpdateVisualStates();

        }
		#endregion //OnPaneLocationChanged

		#region OnPreviousLostKeyboardFocus
		private static void OnPreviousLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			ContentPane pane = sender as ContentPane;

			// when elements (like a menu item) call Keyboard.Focus(null) to try and put
			// focus back to the default element, they are causing focus to go to the main
			// browser window when in an xbap. really what should happen is that the last
			// focused element in that window should get focus. well in the case of an xbap,
			// this is the containing window and therefore we should take the focus and 
			// put it into our focused element.
            // AS 2/19/09 TFS13715
            // Do not manipulate the focus if it is another window that is getting focus.
            //
			//if (e.NewFocus is Window && VisualTreeHelper.GetParent((Window)e.NewFocus) == null)
            // AS 3/30/09 TFS16355 - WinForms Interop
            // This needs to be altered somewhat. It seems that they are giving focus to the 
            // root visual of the presentation source which usually is a window but since we 
            // are now using popups could be that. I refactored this to take the original 
            // criteria as well as the new criteria into account.
            //
            //if (e.NewFocus is Window &&
            //    VisualTreeHelper.GetParent((Window)e.NewFocus) == null &&
            //    Window.GetWindow(pane) == e.NewFocus)
			//
			
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

			bool isLosingFocusToRootVisual = IsLosingFocusToRootVisual(e);

            if (isLosingFocusToRootVisual)
			{
				e.Handled = true;

				DependencyObject focusScope = FocusManager.GetFocusScope(pane);
				Debug.Assert(focusScope == pane);

				if (pane == focusScope)
				{
					IInputElement focusedElement = FocusManager.GetFocusedElement(pane);

					if (focusedElement != e.OldFocus)
					{
						if (null != focusedElement)
							focusedElement.Focus();
						else
							pane.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

						pane.InvalidateVisual();
					}
				}
			}
		}
		#endregion //OnPreviousLostKeyboardFocus

		#region OnPreviewMouseDown
		private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			ContentPane pane = sender as ContentPane;

			if (pane._mouseDownFocusWatcher == null && pane.IsKeyboardFocusWithin == false && pane.IsKeyboardFocused == false)
			{
				pane._mouseDownFocusWatcher = new FocusWatcher();
				pane._mouseDownFocusWatcher.FocusedElementChanged += new RoutedPropertyChangedEventHandler<IInputElement>(pane.OnMouseDownFocusChanged);
			}
		}
		#endregion //OnPreviewMouseDown

		// AS 9/8/09 TFS21921
		#region OnPreviousRootGotKeyboardFocus
		private static void OnPreviousRootGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			// the scenario we are trying to handle is when the framework is focusing the root visual 
			// because it thinks the focus is within a different presentation source. the issue though 
			// is they don't consider the fact that the focused element can be within a popup that is 
			// owned by the active presentation source.
			// 
			if (e.OriginalSource != e.Source)
				return;

			OnPreviousRootGotKeyboardFocusImpl(sender, e);
		}

		private static void OnPreviousRootGotKeyboardFocusImpl(object sender, KeyboardFocusChangedEventArgs e)
		{
			DependencyObject oldObject = e.OldFocus as DependencyObject;

			// we only care about the case where focus is shifted to the root visual so 
			// if focus is cleared or the old element isn't something that is part of the 
			// dockmanager we can ignore the change
			if (oldObject == null || XamDockManager.GetDockManager(oldObject) == null)
				return;

			// since we have an inherited property that the ContentPane sets on itself 
			// we can use that to quickly get to the containing content pane
			HwndHostInfo hhi = HwndHostInfo.GetHwndHost(oldObject);
			ContentPane containingPane = hhi != null ? hhi.Owner as ContentPane : null;

			if (containingPane == null)
				return;

			// we can skip the request if they are explicitly focusing something other than 
			// the root visual
			if (!IsLosingFocusToRootVisual(e))
				return;

			// lastly do not try to keep focus if we cannot actually be focused
			if (containingPane.ShouldPreventLostFocus(e.NewFocus as DependencyObject))
			{
				e.Handled = true;
			}
		} 
		#endregion //OnPreviousRootGotKeyboardFocus

		#region OnVisibilityChanged
		private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DockManagerUtilities.RaiseVisibilityChanged(d, e);
		} 
		#endregion //OnVisibilityChanged

		#region ShouldIncludeMenuItem
		// AS 5/29/08
		// Moved the basic criteria for whether to include a menu item here from 
		// the InsertMenuItem method since we needed to be able to check the 
		// criteria of other menu items.
		//
		private bool ShouldIncludeMenuItem(PaneMenuItem menuItem)
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null == dockManager)
				return false;

			switch (menuItem)
			{
				case PaneMenuItem.AutoHide:
					return this.AllowPinning;
				case PaneMenuItem.Close:
					return this.AllowClose;
				case PaneMenuItem.CloseAllButThis:
					return this.CanCloseAllButThis;
				case PaneMenuItem.Dockable:
					return this.IsDockable || this.DockableAreas != DockableAreas.None;
				case PaneMenuItem.Document:
					return dockManager.HasDocumentContentHost && (this.IsDocument || this.AllowInDocumentHost);
				case PaneMenuItem.FloatingOnly:
					return this.AllowFloatingOnly || this.PaneLocation == PaneLocation.FloatingOnly;
				case PaneMenuItem.Hide:
					return this.AllowClose;
				case PaneMenuItem.NewHorizontalTabGroup:
					return XamDockManager.CanMoveToNewGroup(this, Orientation.Horizontal);
				case PaneMenuItem.NewVerticalTabGroup:
					return XamDockManager.CanMoveToNewGroup(this, Orientation.Vertical);
				case PaneMenuItem.NextTabGroup:
					return XamDockManager.CanMoveToNextPreviousGroup(this, true);
				case PaneMenuItem.PreviousTabGroup:
					return XamDockManager.CanMoveToNextPreviousGroup(this, false);
				case PaneMenuItem.Separator:
					return true;
				default:
					Debug.Fail("Unexpected menu item:" + menuItem.ToString());
					return true;
			}
		}
		#endregion //ShouldIncludeMenuItem

		// AS 9/8/09 TFS21921
		#region ShouldPreventLostFocus
		private bool ShouldPreventLostFocus(DependencyObject newFocus)
		{
			if (!this.IsVisible || !this.IsEnabled)
				return false;

			return true;
		}
		#endregion //ShouldPreventLostFocus

		#region VerifyIsPinnedState
		/// <summary>
		/// Tells the associated <see cref="XamDockManager"/> to verify the placement of the pane based on its unpinned/pinned state
		/// </summary>
		internal void VerifyIsPinnedState()
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null != dockManager)
			{
				this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new System.Threading.ContextCallback(this.VerifyIsPinnedStateSynchronous), dockManager.IsLoaded);
			}
		}

		private void VerifyIsPinnedStateSynchronous(object allowShowFlyout)
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null != dockManager)
				dockManager.ProcessPinnedState(this, this.PaneLocation, true.Equals(allowShowFlyout));
		}
		#endregion //VerifyIsPinnedState

		#endregion //Private Methods

		#endregion //Methods

		#region Events

		#region Closing

		/// <summary>
		/// Event ID for the <see cref="Closing"/> routed event
		/// </summary>
		/// <seealso cref="Closing"/>
		/// <seealso cref="OnClosing"/>
		/// <seealso cref="PaneClosingEventArgs"/>
		public static readonly RoutedEvent ClosingEvent =
			EventManager.RegisterRoutedEvent("Closing", RoutingStrategy.Bubble, typeof(EventHandler<PaneClosingEventArgs>), typeof(ContentPane));

		/// <summary>
		/// Occurs when the <see cref="ContentPane"/> is about to be closed
		/// </summary>
		/// <seealso cref="Closing"/>
		/// <seealso cref="ClosingEvent"/>
		/// <seealso cref="PaneClosingEventArgs"/>
		protected virtual void OnClosing(PaneClosingEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseClosing(PaneClosingEventArgs args)
		{
			args.RoutedEvent = ContentPane.ClosingEvent;
			args.Source = this;
			this.OnClosing(args);
		}

		/// <summary>
		/// Occurs when the <see cref="ContentPane"/> is about to be closed
		/// </summary>
		/// <seealso cref="OnClosing"/>
		/// <seealso cref="ClosingEvent"/>
		/// <seealso cref="PaneClosingEventArgs"/>
		//[Description("Occurs when the 'ContentPane' is about to be closed")]
		//[Category("DockManager Events")] // Behavior
		public event EventHandler<PaneClosingEventArgs> Closing
		{
			add
			{
				base.AddHandler(ContentPane.ClosingEvent, value);
			}
			remove
			{
				base.RemoveHandler(ContentPane.ClosingEvent, value);
			}
		}

		#endregion //Closing

		#region Closed

		/// <summary>
		/// Event ID for the <see cref="Closed"/> routed event
		/// </summary>
		/// <seealso cref="Closed"/>
		/// <seealso cref="OnClosed"/>
		/// <seealso cref="PaneClosedEventArgs"/>
		public static readonly RoutedEvent ClosedEvent =
			EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(EventHandler<PaneClosedEventArgs>), typeof(ContentPane));

		/// <summary>
		/// Occurs when the <see cref="ContentPane"/> has been closed
		/// </summary>
		/// <seealso cref="Closed"/>
		/// <seealso cref="ClosedEvent"/>
		/// <seealso cref="PaneClosedEventArgs"/>
		protected virtual void OnClosed(PaneClosedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseClosed(PaneClosedEventArgs args)
		{
			args.RoutedEvent = ContentPane.ClosedEvent;
			args.Source = this;
			this.OnClosed(args);
		}

		/// <summary>
		/// Occurs when the <see cref="ContentPane"/> has been closed
		/// </summary>
		/// <seealso cref="OnClosed"/>
		/// <seealso cref="ClosedEvent"/>
		/// <seealso cref="PaneClosedEventArgs"/>
		//[Description("Occurs when the 'ContentPane' has been closed")]
		//[Category("DockManager Events")] // Behavior
		public event EventHandler<PaneClosedEventArgs> Closed
		{
			add
			{
				base.AddHandler(ContentPane.ClosedEvent, value);
			}
			remove
			{
				base.RemoveHandler(ContentPane.ClosedEvent, value);
			}
		}

		#endregion //Closed

		#region ExecutingCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutingCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		public static readonly RoutedEvent ExecutingCommandEvent =
			EventManager.RegisterRoutedEvent("ExecutingCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutingCommandEventArgs>), typeof(ContentPane));

		/// <summary>
		/// Occurs before a command is executed.
		/// </summary>
		/// <remarks><para class="body">This event is cancellable.</para></remarks>
		/// <seealso cref="ExecuteCommand(RoutedCommand)"/>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		protected virtual void OnExecutingCommand(ExecutingCommandEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal bool RaiseExecutingCommand(ExecutingCommandEventArgs args)
		{
			args.RoutedEvent = ContentPane.ExecutingCommandEvent;
			args.Source = this;
			this.OnExecutingCommand(args);

			return args.Cancel == false;
		}

		/// <summary>
		/// Occurs before a command is executed
		/// </summary>
		/// <remarks><para class="body">This event is cancellable.</para></remarks>
		/// <seealso cref="ExecuteCommand(RoutedCommand)"/>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		//[Description("Occurs before a command is performed")]
		//[Category("DockManager Events")] // Action
		public event EventHandler<ExecutingCommandEventArgs> ExecutingCommand
		{
			add
			{
				base.AddHandler(ContentPane.ExecutingCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(ContentPane.ExecutingCommandEvent, value);
			}
		}

		#endregion //ExecutingCommand

		#region ExecutedCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutedCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecuteCommand(RoutedCommand)"/>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		public static readonly RoutedEvent ExecutedCommandEvent =
			EventManager.RegisterRoutedEvent("ExecutedCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutedCommandEventArgs>), typeof(ContentPane));

		/// <summary>
		/// Occurs after a command is executed
		/// </summary>
		/// <seealso cref="ExecuteCommand(RoutedCommand)"/>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		protected virtual void OnExecutedCommand(ExecutedCommandEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseExecutedCommand(ExecutedCommandEventArgs args)
		{
			args.RoutedEvent = ContentPane.ExecutedCommandEvent;
			args.Source = this;
			this.OnExecutedCommand(args);
		}

		/// <summary>
		/// Occurs after a command is executed
		/// </summary>
		/// <seealso cref="ExecuteCommand(RoutedCommand)"/>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		//[Description("Occurs after a command is performed")]
		//[Category("DockManager Events")] // Action
		public event EventHandler<ExecutedCommandEventArgs> ExecutedCommand
		{
			add
			{
				base.AddHandler(ContentPane.ExecutedCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(ContentPane.ExecutedCommandEvent, value);
			}
		}

		#endregion //ExecutedCommand

		#region OptionsMenuOpening

		/// <summary>
		/// Event ID for the <see cref="OptionsMenuOpening"/> routed event
		/// </summary>
		/// <seealso cref="OptionsMenuOpening"/>
		/// <seealso cref="OnOptionsMenuOpening"/>
		/// <seealso cref="PaneOptionsMenuOpeningEventArgs"/>
		public static readonly RoutedEvent OptionsMenuOpeningEvent =
			EventManager.RegisterRoutedEvent("OptionsMenuOpening", RoutingStrategy.Bubble, typeof(EventHandler<PaneOptionsMenuOpeningEventArgs>), typeof(ContentPane));

		/// <summary>
		/// Occurs when the menu or context menu for the <see cref="ContentPane"/> is about to be displayed
		/// </summary>
		/// <seealso cref="OptionsMenuOpening"/>
		/// <seealso cref="OptionsMenuOpeningEvent"/>
		/// <seealso cref="PaneOptionsMenuOpeningEventArgs"/>
		protected virtual void OnOptionsMenuOpening(PaneOptionsMenuOpeningEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseOptionsMenuOpening(PaneOptionsMenuOpeningEventArgs args)
		{
			args.RoutedEvent = ContentPane.OptionsMenuOpeningEvent;
			args.Source = this;
			this.OnOptionsMenuOpening(args);
		}

		/// <summary>
		/// Occurs when the menu or context menu for the <see cref="ContentPane"/> is about to be displayed
		/// </summary>
		/// <seealso cref="OnOptionsMenuOpening"/>
		/// <seealso cref="OptionsMenuOpeningEvent"/>
		/// <seealso cref="PaneOptionsMenuOpeningEventArgs"/>
		//[Description("Occurs when the menu or context menu for the 'ContentPane' is about to be displayed")]
		//[Category("DockManager Events")] // Behavior
		public event EventHandler<PaneOptionsMenuOpeningEventArgs> OptionsMenuOpening
		{
			add
			{
				base.AddHandler(ContentPane.OptionsMenuOpeningEvent, value);
			}
			remove
			{
				base.RemoveHandler(ContentPane.OptionsMenuOpeningEvent, value);
			}
		}

		#endregion //OptionsMenuOpening

		#endregion //Events	
		
		#region ICommandHost Members

		bool ICommandHost.CanExecute(ExecuteCommandInfo commandInfo)
		{
			RoutedCommand command = commandInfo.RoutedCommand;

			if (null == command)
				return false;

            // AS 2/12/09 TFS12819
            // Since ContentPanes can be nested, this pane may consider that the command 
            // cannot execute but we don't want the command event to bubble up to the nesting 
            // ContentPane and have it execute that command.
            //
            if (command.OwnerType == typeof(ContentPaneCommands))
                commandInfo.ForceHandled = true;

			// AS 5/2/08 BR31999
			if (this.PaneLocation == PaneLocation.Unknown)
				return false;

			if (command == ContentPaneCommands.ActivatePane)
				return this.CanActivate;

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (command == ContentPaneCommands.Close)
				return this.AllowClose;
			else if (command == ContentPaneCommands.ChangeToDockable)
			{
				if (this.IsPinned == false)
					return false;

				return this.IsDockable || this.DockableAreas != 0;
			}
			else if (command == ContentPaneCommands.ChangeToDocument)
			{
				if (this.IsPinned == false)
					return false;

				// AS 6/23/11 TFS73499
				//return null != dockManager &&
				//    dockManager.HasDocumentContentHost &&
				//    (this.AllowInDocumentHost || this.IsDocument);
				if (dockManager == null || !dockManager.HasDocumentContentHost)
					return false;

				if (this.IsDocument)
					return true;

				if (!dockManager.IsPaneLocationVisible(PaneLocation.Document))
					return false;

				return this.AllowInDocumentHost;
			}
			else if (command == ContentPaneCommands.ChangeToFloatingOnly)
			{
				if (this.IsPinned == false)
					return false;

				// AS 6/23/11 TFS73499
				// Don't allow a window to go to floating only while the visibility of the 
				// floating windows would be collapsed/hidden.
				//
				//return this.AllowFloatingOnly || this.PaneLocation == PaneLocation.FloatingOnly;
				if (this.PaneLocation == PaneLocation.FloatingOnly)
					return true;

				if (dockManager != null && !dockManager.IsPaneLocationVisible(PaneLocation.FloatingOnly))
					return false;

				return this.AllowFloatingOnly;
			}
			else if (command == ContentPaneCommands.CloseAllButThis)
				return this.CanCloseAllButThis;

			else if (command == ContentPaneCommands.MoveToNewHorizontalGroup)
				return XamDockManager.CanMoveToNewGroup(this, Orientation.Horizontal);

			else if (command == ContentPaneCommands.MoveToNewVerticalGroup)
				return XamDockManager.CanMoveToNewGroup(this, Orientation.Vertical);

			else if (command == ContentPaneCommands.MoveToNextGroup)
				return XamDockManager.CanMoveToNextPreviousGroup(this, true);

			else if (command == ContentPaneCommands.MoveToPreviousGroup)
				return XamDockManager.CanMoveToNextPreviousGroup(this, false);

			else if (command == ContentPaneCommands.ToggleDockedState)
				return this.CanToggleDockedState;

			else if (command == ContentPaneCommands.TogglePinnedState)
				return this.CanTogglePinnedState;

			else if (command == ContentPaneCommands.FlyIn)
				return this.PaneLocation == PaneLocation.Unpinned && dockManager != null && dockManager.CurrentFlyoutPane == this;

			else if (command == ContentPaneCommands.Flyout)
				return this.Visibility == Visibility.Visible && this.PaneLocation == PaneLocation.Unpinned && dockManager != null;

			return false;
		}

		// SSP 3/18/10 TFS29783 - Optimizations
		// 
		//long ICommandHost.CurrentState
		long ICommandHost.GetCurrentState( long statesToQuery )
		{
			ContentPaneStates state = 0;

			if ( this.IsActiveDocument )
				state |= ContentPaneStates.IsActiveDocument;

			if ( this.IsActivePane )
				state |= ContentPaneStates.IsActivePane;

			if ( this.AllowClose )
				state |= ContentPaneStates.AllowClose;

			switch ( this.PaneLocation )
			{
				case PaneLocation.DockedBottom:
				case PaneLocation.DockedLeft:
				case PaneLocation.DockedRight:
				case PaneLocation.DockedTop:
					state |= ContentPaneStates.IsDocked;
					break;
				case PaneLocation.Document:
					state |= ContentPaneStates.IsDocument;
					break;
				case PaneLocation.Floating:
					state |= ContentPaneStates.IsFloating;
					state |= ContentPaneStates.IsFloatingDockable;
					break;
				case PaneLocation.FloatingOnly:
					state |= ContentPaneStates.IsFloating;
					state |= ContentPaneStates.IsFloatingOnly;
					break;
				case PaneLocation.Unpinned:
					state |= ContentPaneStates.IsUnpinned;
					break;
			}

			return (long)state & statesToQuery;
		}

		bool ICommandHost.Execute(ExecuteCommandInfo commandInfo)
		{
			RoutedCommand command = commandInfo.RoutedCommand;
			return null != command && this.ExecuteCommandImpl(commandInfo);
		}

		#endregion //ICommandHost Members

		#region ContentPaneTemplateSelector class
		/// <summary>
		/// Custom <see cref="DataTemplateSelector"/> used to choose the default datatemplate for the various content properties of the <see cref="ContentPane"/>
		/// </summary>
		internal class ContentPaneTemplateSelector : DataTemplateSelector
		{
			#region Members

			static internal readonly DataTemplate PaneHeaderDataTemplateForString;
			static internal readonly DataTemplate PaneTabItemDataTemplateForString;
			static internal readonly DataTemplate PaneTabItemDataTemplateForAccessText;
			static internal readonly DataTemplateSelector Instance = new ContentPaneTemplateSelector();

			#endregion //Members

			#region Constructors

			private ContentPaneTemplateSelector()
			{
			}

			static ContentPaneTemplateSelector()
			{
				PaneHeaderDataTemplateForString = DockManagerUtilities.CreateStringTemplate(typeof(PaneHeaderPresenter), false);
				PaneTabItemDataTemplateForString = DockManagerUtilities.CreateStringTemplate(typeof(PaneTabItem), false);
				PaneTabItemDataTemplateForAccessText = DockManagerUtilities.CreateStringTemplate(typeof(PaneTabItem), true);
			}

			#endregion //Constructors

			#region SelectTemplate
			public override DataTemplate SelectTemplate(object item, DependencyObject container)
			{
				ContentPresenter cp = container as ContentPresenter;

				if (null != cp && item is string)
				{
					if (cp.TemplatedParent is PaneHeaderPresenter && cp.RecognizesAccessKey == false)
						return PaneHeaderDataTemplateForString;
					else if (cp.TemplatedParent is PaneTabItem)
					{
						if (cp.RecognizesAccessKey)
							return PaneTabItemDataTemplateForAccessText;
						else
							return PaneTabItemDataTemplateForString;
					}
				}

				return null;
			}
			#endregion //SelectTemplate
		}
		#endregion //ContentPaneTemplateSelector class

		#region PanePlacementInfo class
		/// <summary>
		/// Class to manage the position/placement information for a <see cref="ContentPane"/>
		/// </summary>
		internal class PanePlacementInfo
		{
			#region Member Variables

			private ContentPanePlaceholder _dockedEdgePlaceholder;
			private ContentPanePlaceholder _floatingDockablePlaceholder;
			private ContentPanePlaceholder _floatingOnlyPlaceholder;
			private IContentPaneContainer _currentContainer;
			
			private IContentPaneContainer _lastDockableContainer;

			#endregion //Member Variables

			#region Constructor
			internal PanePlacementInfo()
			{
			} 
			#endregion //Constructor

			#region Properties

			#region CurrentContainer
			internal IContentPaneContainer CurrentContainer
			{
				get { return this._currentContainer; }
				set 
				{ 
					this._currentContainer = value;

					// store a reference to the last dockable container in case we 
					// move to a non-dockable container
					this.VerifyLastDockableContainer();







				}
			}
			#endregion //CurrentContainer

			#region DockableContainer
			/// <summary>
			/// Returns the last (or current dockable container for the pane.
			/// </summary>
			internal IContentPaneContainer DockableContainer
			{
				get
				{
					Debug.Assert(this._lastDockableContainer == null || DockManagerUtilities.IsDockable(this._lastDockableContainer.PaneLocation));

					return this._lastDockableContainer;
				}
			} 
			#endregion //DockableContainer

			#region DockedContainer
			internal IContentPaneContainer DockedContainer
			{
				get
				{
					if (this._dockedEdgePlaceholder != null)
						return this._dockedEdgePlaceholder.Container;

					if (this._currentContainer != null && DockManagerUtilities.IsDocked(this._currentContainer.PaneLocation))
						return this._currentContainer;

					return null;
				}
			} 
			#endregion //DockedContainer

			#region DockedEdgePlaceholder
			/// <summary>
			/// Returns the placeholder that maintains the position of the content pane 
			/// within a docked split pane.
			/// </summary>
			internal ContentPanePlaceholder DockedEdgePlaceholder
			{
				get { return this._dockedEdgePlaceholder; }
			}
			#endregion //DockedEdgePlaceholder

			#region FloatingDockablePlaceholder
			/// <summary>
			/// Returns the placeholder that maintains the position of the content pane 
			/// within a floating window.
			/// </summary>
			internal ContentPanePlaceholder FloatingDockablePlaceholder
			{
				get { return this._floatingDockablePlaceholder; }
			}
			#endregion //FloatingDockablePlaceholder

			#region FloatingOnlyPlaceholder
			/// <summary>
			/// Returns the placeholder that maintains the position of the content pane 
			/// within a floating window.
			/// </summary>
			internal ContentPanePlaceholder FloatingOnlyPlaceholder
			{
				get { return this._floatingOnlyPlaceholder; }
			}
			#endregion //FloatingOnlyPlaceholder

			#endregion //Properties

			#region Methods

			#region DeletePlaceholder
			private void DeletePlaceholder(ref ContentPanePlaceholder placeholder)
			{
				ContentPanePlaceholder oldPlaceholder = placeholder;

				placeholder = null;

				if (null != oldPlaceholder)
				{
					IContentPaneContainer contentContainer = oldPlaceholder.Container;
					IPaneContainer paneContainer = contentContainer as IPaneContainer;

					Debug.Assert(null != paneContainer);

					if (null != paneContainer)
					{
						int index = paneContainer.Panes.IndexOf(oldPlaceholder);

						if (index >= 0)
						{
							contentContainer.RemoveContentPane(oldPlaceholder.Pane, false);
							DockManagerUtilities.RemoveContainerIfNeeded(contentContainer);
						}
					}
				}
			}
			#endregion //DeletePlaceholder

			#region IsInSameDockedContainer
			internal bool IsInSameDockedContainer(ContentPane pane)
			{
				IContentPaneContainer dockedContainer = this.DockedContainer;
				IContentPaneContainer otherDockedContainer = pane.PlacementInfo.DockedContainer;

				if (dockedContainer == null || otherDockedContainer == null)
				{
					Debug.Fail("One of the panes doesn't have a docked container!");
					return false;
				}

				return dockedContainer == otherDockedContainer;
			}
			#endregion //IsInSameDockedContainer

			#region RemovePlaceholder
			internal void RemovePlaceholder(ContentPanePlaceholder placeholder)
			{
				if (placeholder == this._dockedEdgePlaceholder)
					this.RemovePlaceholderImpl(ref this._dockedEdgePlaceholder);
				else if (placeholder == this._floatingDockablePlaceholder)
					this.RemovePlaceholderImpl(ref this._floatingDockablePlaceholder);
				else if (placeholder == this._floatingOnlyPlaceholder)
					this.RemovePlaceholderImpl(ref this._floatingOnlyPlaceholder);

				// if we don't have a pane in this location....
				this.VerifyLastDockableContainer();
			}

			private void RemovePlaceholderImpl(ref ContentPanePlaceholder placeholder)
			{
				ContentPanePlaceholder oldPlaceholder = placeholder;

				if (null != oldPlaceholder)
				{
					// clear the reference to it
					placeholder = null;

					// make sure its container doesn't reference it any more. this could happen if
					// the pane was docked on the right, floated and then docked on the left for example
					if (null != oldPlaceholder.Container)
					{
						IPaneContainer container = oldPlaceholder.Container as IPaneContainer;
						int index = container != null ? container.Panes.IndexOf(oldPlaceholder) : -1;

						Debug.Assert(index < 0 || oldPlaceholder.Container.PaneLocation == PaneLocation.Unknown);
					}
				}
			}
			#endregion //RemovePlaceholder

			#region RemoveReplacedPlaceholder
			internal void RemoveReplacedPlaceholder()
			{
				if (this._currentContainer != null)
				{
					switch (this._currentContainer.PaneLocation)
					{
						case PaneLocation.DockedBottom:
						case PaneLocation.DockedLeft:
						case PaneLocation.DockedRight:
						case PaneLocation.DockedTop:
							this.DeletePlaceholder(ref this._dockedEdgePlaceholder);
							break;
						case PaneLocation.Floating:
							this.DeletePlaceholder(ref this._floatingDockablePlaceholder);
							break;
						case PaneLocation.FloatingOnly:
							this.DeletePlaceholder(ref this._floatingOnlyPlaceholder);
							break;
					}
				}
			} 
			#endregion //RemoveReplacedPlaceholder

			#region StorePlaceholder
			internal void StorePlaceholder(ContentPanePlaceholder placeholder)
			{
				PaneLocation location = XamDockManager.GetPaneLocation(placeholder);
				StorePlaceholder(placeholder, location);
			}

			internal void StorePlaceholder(ContentPanePlaceholder placeholder, PaneLocation location)
			{
				Debug.Assert(location == XamDockManager.GetPaneLocation(placeholder) ||
					XamDockManager.GetPaneLocation(placeholder) == PaneLocation.Unknown);

				if (DockManagerUtilities.IsDocked(location))
				{
					this.StorePlaceholderImpl(ref this._dockedEdgePlaceholder, placeholder);
					this.VerifyLastDockableContainer(DockableState.Docked);
				}
				else if (location == PaneLocation.Floating)
				{
					this.StorePlaceholderImpl(ref this._floatingDockablePlaceholder, placeholder);
					this.VerifyLastDockableContainer(DockableState.Floating);
				}
				else if (location == PaneLocation.FloatingOnly)
					this.StorePlaceholderImpl(ref this._floatingOnlyPlaceholder, placeholder);
				else
					Debug.Fail("Unexpected placeholder for location:" + location.ToString());
			}

			private void StorePlaceholderImpl(ref ContentPanePlaceholder placeholder, ContentPanePlaceholder newPlaceholder)
			{
				Debug.Assert(placeholder == null || newPlaceholder == placeholder);
				placeholder = newPlaceholder;
			}
			#endregion //StorePlaceholder 

			#region VerifyLastDockableContainer
			private void VerifyLastDockableContainer()
			{
				DockableState state = this._lastDockableContainer != null && DockManagerUtilities.IsFloating(this._lastDockableContainer.PaneLocation)
					? DockableState.Floating : DockableState.Docked;

				this.VerifyLastDockableContainer(state);
			}

			internal void VerifyLastDockableContainer(DockableState preferredState)
			{
				IContentPaneContainer lastDockableContainer;

				if (this._currentContainer != null && DockManagerUtilities.IsDockable(this._currentContainer.PaneLocation))
					lastDockableContainer = this._currentContainer;
				else if (preferredState == DockableState.Docked && this._dockedEdgePlaceholder != null)
					lastDockableContainer = this._dockedEdgePlaceholder.Container;
				else if (this._floatingDockablePlaceholder != null)
					lastDockableContainer = this._floatingDockablePlaceholder.Container;
				else
					lastDockableContainer = null;

				this._lastDockableContainer = lastDockableContainer;
			}
			#endregion //VerifyLastDockableContainer

			#region VerifyPlaceholderRemoved


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			#endregion //VerifyPlaceholderRemoved

			#endregion //Methods
		}
		#endregion //PanePlacementInfo class

        // AS 10/13/08 TFS6032
        // Helper class to maintain a list of the hwndhosts within the pane. We 
        // can also use this to get around a bug in the hwndhost whereby they 
        // delay hiding the window when the isvisible changes to false. A subsequent
        // reparenting of the WindowsFormsHost before the window is actually hidden 
        // causes a flicker as the Control repositions itself.
        //
        #region HwndHostInfo class
        
#region Infragistics Source Cleanup (Region)





































































































































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        #endregion //HwndHostInfo class

        #region IHwndHostInfoOwner Members

        void IHwndHostInfoOwner.OnHasHostsChanged()
        {
            this.VerifyContentVisibility();

            // AS 3/30/09 TFS16355 - WinForms Interop
            XamDockManager dm = XamDockManager.GetDockManager(this);

            if (null != dm)
			{
				// AS 9/11/09 TFS21329
				if (this == dm.CurrentFlyoutPane && null != dm.DockPanel)
					dm.DockPanel.FlyoutPanel.VerifyMouseOverTimer();

				// AS 9/11/09 TFS21330
				// When hosted in a floating window, we may need to show the window if its 
				// container has AllowsTransparency set to true because an HwndHost won't be 
				// displayed in that situation.
				//
				if (this.HasHwndHost)
				{
					PaneToolWindow paneToolWindow = ToolWindow.GetToolWindow(this) as PaneToolWindow;

					if (null != paneToolWindow)
						paneToolWindow.VerifyPreventAllowsTransparency();
				}

				if (!dm.HasHwndHost)
	                dm.DirtyHasHwndHosts();
			}
        }

        #endregion //IHwndHostInfoOwner Members
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