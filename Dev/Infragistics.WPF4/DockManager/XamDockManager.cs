using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using System.Windows;
using System.Collections.Specialized;
using System.ComponentModel;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Controls.Events;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections;
using Infragistics.Windows.Licensing;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DockManager.Events;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.DockManager.Dragging;
using Infragistics.Shared;
using System.Windows.Data;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.DockManager;
using System.Windows.Interop;
using Infragistics.Collections;


namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// A custom control that arranges elements as panes similar to that of Microsoft Visual Studio.
	/// </summary>
	[TemplatePart(Name = "PART_UnpinnedTabAreaTop", Type = typeof(UnpinnedTabArea))]
	[TemplatePart(Name = "PART_UnpinnedTabAreaBottom", Type = typeof(UnpinnedTabArea))]
	[TemplatePart(Name = "PART_UnpinnedTabAreaLeft", Type = typeof(UnpinnedTabArea))]
	[TemplatePart(Name = "PART_UnpinnedTabAreaRight", Type = typeof(UnpinnedTabArea))]
	[TemplatePart(Name = "PART_Panel", Type = typeof(DockManagerPanel))]
	
	
	public class XamDockManager : ContentControl
		, ICommandHost
		, IPaneContainer
        // AS 3/30/09 TFS16355 - WinForms Interop
        , IHwndHostContainer
		, IHwndHostInfoOwner // AS 9/8/09 TFS21746
	{
		#region Member Variables

		private UnpinnedTabAreaInfo[] _tabAreaInfos = new UnpinnedTabAreaInfo[4];
		private DockManagerPanel _dockPanel;
		private PanesCollection _panes;
		private UltraLicense _license;
		private ObservableCollection<PaneToolWindow> _toolWindows;
		private ReadOnlyObservableCollection<PaneToolWindow> _readOnlyWindows;
		private RootPaneList _rootPaneList;
		private DockManagerCommands _commands;
		private ActivePaneManager _activePaneManager;
		private DragManager _dragManager;
		private List<DependencyObject> _logicalChildren;
		private bool _isLoadingLayout;
		private int _suspendShowFlyoutCount;

		// AS 6/4/08 BR33654
		private RoutedCommand _transferCanExecuteCommand;
		private RoutedCommand _transferExecuteCommand;

        // AS 10/15/08 TFS6271
        private bool _forcingFocus;

        // AS 4/8/09 TFS16492
        // First a little background. There is a problem in the WPF framework whereby the 
        // HwndKeyboardInputProvider associated with an HwndSource assumes that it should 
        // store whatever is in the Keyboard.FocusedElement when it gets a WM_KILLFOCUS, 
        // even if that is not a descendant of the window. See the following for details:
        // https://connect.microsoft.com/feedback/ViewFeedback.aspx?FeedbackID=431388&SiteID=212
        //
        // Now, when dragging, the DragManager used to create a FrameworkContentElement to 
        // which it could assign focus and mouse capture. This FCE was given focus when the 
        // drag operation starts and was released when the drag operation was over. The 
        // problem though is that if the HwndKeyboardInputProvider had cached a reference 
        // to this element (e.g. when a floating window was shown and the main window 
        // containing the dockmanager was deactivated), then when the main window was 
        // reactivated, the HwndKeyboardInputProvider would call focus on the FCE but 
        // that wouldn't do anything because the element was released and isn't in the 
        // logical tree anymore. That in and of itself isn't necessarily a problem. 
        // However, as a result the Keyboard.FocusedElement was not changed so it could be,
        // and in this case was, an element from a different window. So if focus was not 
        // changed before the window lost focus, the HwndKeyboardInputProvider would cache 
        // the Keyboard.FocusedElement, which was an element from a different window. When 
        // the window was subsequently reactivated and the HwndKeyboardInputProvider handled 
        // the WM_SETFOCUS, it would blindly set focus to that element which since it was 
        // in a different window would activate the other window preventing you from activating 
        // the main window.
        //
        // So to at least get around the case where they cached a reference to the FCE used 
        // by the DragManager and therefore the Keyboard.FocusedElement wasn't changed when 
        // the window regained focus initially, we will now leave the FCE in the logical tree
        // and therefore it should be able to be given focus which will allow the FocusedElement
        // to change to something within the window. This bug in the WPF framework could 
        // manifest itself in other ways though so this only works around one scenario.
        //
        private FrameworkContentElement _dragHelper;

		// AS 7/14/09 TFS18400
		private bool _isClosingPanes;

		// AS 7/16/09 TFS18452
		private Dictionary<ContentPane, bool> _deferredPinnedPanes;

		// AS 9/16/09 TFS22219
		private LogicalContainer _unpinnedPaneHolder;

        // AS 3/30/09 TFS16355 - WinForms Interop
        private bool? _hasHwndHost;

		// AS 3/17/10 TFS27618
		private bool _isSettingFocusedElement;

		// AS 10/14/10 TFS36772 Cut/Copy
		private static readonly Type _secureCommandType;

		// AS 10/5/09 NA 2010.1 - LayoutMode
		private WeakReference _fillPane;

		// AS 6/23/11 TFS73499
		private HwndSourceHelper _hwndSourceHelper;
		private static bool _avoidHwndSourceHelper;

		// AS 4/28/11 TFS73532
		private bool _hasAppliedTemplate;

		// AS 6/14/12 TFS99504
		private bool _isActivatingPane;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="XamDockManager"/>
		/// </summary>
		public XamDockManager()
		{
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamDockManager), this) as Infragistics.Windows.Licensing.UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }

			for (int i = 0; i < this._tabAreaInfos.Length; i++)
				this._tabAreaInfos[i] = new UnpinnedTabAreaInfo(this);

			this._panes = new PanesCollection(this);
			this._toolWindows = new ObservableCollection<PaneToolWindow>();
			this._readOnlyWindows = new ReadOnlyObservableCollection<PaneToolWindow>(this._toolWindows);
			this._rootPaneList = new RootPaneList(this);

			this._commands = DockManagerCommands.Instance;
			this._activePaneManager = new ActivePaneManager(this);
			this._dragManager = new DragManager(this);

			// we don't want to process add/removes until the control has been
			// initialized. otherwise property values like the initial location
			// may not get picked up if the items are added before the property
			// settings are deserialized. we'll call end update in the OnInitialized
			//
			if (this.IsInitialized == false)
				this._panes.BeginUpdate();

			this.SetValue(XamDockManager.DockManagerPropertyKey, this);

            // AS 10/9/08 TFS6846
            // If this dockmanager is within another dockmanager, we don't want to pick up
            // its pane location for the elements within the dockmanager.
            //
            this.SetValue(XamDockManager.PaneLocationPropertyKey, DockManagerKnownBoxes.PaneLocationUnknownBox);

			// AS 10/5/09 NA 2010.1 - LayoutMode
			// In case the dockmanager is nested, we don't want the property value of the ancestor 
			// split pane to affect the panes within this dockmanager.
			//
			this.SetValue(SplitPane.IsInFillPanePropertyKey, KnownBoxes.FalseBox);

			// AS 4/30/08
			// We need to hook the loaded event so that we can use that as a trigger for showing 
			// the floating windows as well as triggering the ToolWindowLoaded event.
			//
			Debug.Assert(this.IsLoaded == false);
			this.Loaded += new RoutedEventHandler(OnDockManagerLoaded);
		}

		static XamDockManager()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamDockManager), new FrameworkPropertyMetadata(typeof(XamDockManager)));

			// GH 5/13/08
			// register the groupings that should be applied when the theme property is changed
			ThemeManager.RegisterGroupings(typeof(XamDockManager), new string[] { PrimitivesGeneric.Location.Grouping, DockManagerGeneric.Location.Grouping });
	
			// AS 4/24/08
			// The xdm should be part of the default focus scope - i.e. the fs of the window - but the content
			// panes will be part of a different focus scope. We want to be able to allow the programmer to 
			// start off with focus within the main pane. To do that we have to allow the dockmanager to receive
			// focus but it will in turn shift focus to its active document. In case we have nothing to activate
			// make sure we don't show a focus rect.
			//
			//UIElement.FocusableProperty.OverrideMetadata(typeof(XamDockManager), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(XamDockManager), new FrameworkPropertyMetadata(null));

			// AS 7/17/12 TFS116580
			// This was somehow causing the IsInDesignModeProperty to no longer work for any class.
			//
			//// AS 5/20/08 BR31679
			//DesignerProperties.IsInDesignModeProperty.OverrideMetadata(typeof(XamDockManager), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsInDesignModeChanged)));

			// AS 6/4/08 BR33654
			// When a button in a toolbar/ribbon or menu item in a menu invokes the canexecute of the associated 
			// command, that bubbles up the element tree. When it gets to the focus scope (the toolbar/ribbon/menu), 
			// the command manager transfers that to start with the focusedelement of the parent focus scope. The 
			// parent focus scope is the window and its focused element was null. Even if it wasn't null - it would 
			// be something in its focus scope - at best the dockmanager - but not the content panes since they are 
			// in a different focusscope. In order to get around this, we have 2 options - not have the contentpanes 
			// be a focus scope (which we want to have so that each can maintain its own focused element as vs does) 
			// or help pass along the canexecute/execute of commands to the active pane's focused element. This 
			// workaround deals with the latter although it shouldn't prevent the other option. Anyway, first the 
			// DM has to make itself the focused element when something within it gets focus (assuming something 
			// within it but within the same focus scope doesn't already have focus. This way we get the 
			// canexecute/execute raised to the dm when its in the root focus scope. In the canexecute/execute, 
			// we raise the canexecute/execute of the routed command (can only do it with that because that's the 
			// only one that takes a target) using the focused element of the active content pane (assuming that 
			// pane is not floating).
			//
			EventManager.RegisterClassHandler(typeof(XamDockManager), CommandManager.CanExecuteEvent, new CanExecuteRoutedEventHandler(OnTransferCanExecuteCommand));
			EventManager.RegisterClassHandler(typeof(XamDockManager), CommandManager.ExecutedEvent, new ExecutedRoutedEventHandler(OnTransferExecuteCommand));

			// AS 10/14/10 TFS36772 Cut/Copy
			// This isn't specific to this issue but I noticed it while debugging. Previously only the 
			// Paste command was a security command but in CLR 4, Cut and Copy are also SecureCommands 
			// so we'll check for the command type instead of looking for just that one command.
			//
			//// AS 6/4/08 BR33654
			//// We need to special case the paste command so the permissions asserted for use of the clipboard
			//// are propogated in the transfer. as a result we need to have the OnTransferCanExecuteCommand and 
			//// OnTransferExecuteCommand ignore these commands.
			////
			//CommandManager.RegisterClassCommandBinding(typeof(XamDockManager), new CommandBinding(ApplicationCommands.Paste, new ExecutedRoutedEventHandler(OnTransferBindingExecute), new CanExecuteRoutedEventHandler(OnTransferBindingCanExecute)));
			_secureCommandType = ApplicationCommands.Paste.GetType();

			CanExecuteRoutedEventHandler canHandler = new CanExecuteRoutedEventHandler(OnTransferBindingCanExecute);
			ExecutedRoutedEventHandler exeHandler = new ExecutedRoutedEventHandler(OnTransferBindingExecute);

			foreach (System.Reflection.PropertyInfo prop in typeof(ApplicationCommands).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
			{
				if (typeof(ICommand).IsAssignableFrom(prop.PropertyType))
				{
					ICommand cmd = prop.GetValue(null, null) as ICommand;

					if (cmd != null && cmd.GetType() == _secureCommandType)
						CommandManager.RegisterClassCommandBinding(typeof(XamDockManager), new CommandBinding(cmd, exeHandler, canHandler));
				}
			}
		}

		#endregion //Constructor

		#region Base class overrides

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns <see cref="XamDockManager"/> Automation Peer Class <see cref="XamDockManagerAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new XamDockManagerAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer

        #region LogicalChildren
        /// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

				if (null == this._logicalChildren || this._logicalChildren.Count == 0)
					return base.LogicalChildren;

				return new MultiSourceEnumerator(base.LogicalChildren,
					this._logicalChildren.GetEnumerator());
			}
		} 
		#endregion //LogicalChildren

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been changed.
		/// </summary>
		public override void OnApplyTemplate()
		{
			// remove the docked panes from the old panel
			if (this._dockPanel != null)
			{
				// AS 7/1/10 TFS34460
				_dockPanel.Loaded -= new RoutedEventHandler(OnDockPanelLoaded);

				this._dockPanel.Panes.Clear();

				// make sure to remove all the content panes from the visual tree
				// of the dockpanel so the elements can be placed into the new tree
				this._dockPanel.RemoveAllUnpinnedPanes();
			}

			// AS 5/9/08
			// We need to remove and readd the panes to the logical tree so
			// that the attached DockManager property will get propogated to
			// the panes. Otherwise, when the template has been changed, the 
			// docked SplitPanes (and their descendants) will not have their
			// XamRibbon property set.
			//
			IList<SplitPane> logicalPanes = new List<SplitPane>();

			// AS 4/28/11 TFS73532
			// Don't remove from the logical tree the first time the template is applied.
			//
			//if (null != this._logicalChildren)
			if (null != this._logicalChildren && _hasAppliedTemplate)
			{
				foreach (object lc in this._logicalChildren)
				{
					SplitPane split = lc as SplitPane;

					if (null != split)
						logicalPanes.Add(split);
				}
			}
            // AS 10/15/08 TFS8068
            // Maintain the DataContext during the move.
            //
            using (DockManagerUtilities.CreateMoveReplacement(logicalPanes))
            {
                foreach (SplitPane split in logicalPanes)
                    this.RemoveLogicalChildInternal(split);

                base.OnApplyTemplate();

                // get the unpinned tab areas
                for (int i = 0; i < 4; i++)
                    this._tabAreaInfos[i].TabArea = this.GetTemplateChild("PART_UnpinnedTabArea" + ((Dock)i).ToString()) as UnpinnedTabArea;

                this._dockPanel = this.GetTemplateChild("PART_Panel") as DockManagerPanel;

                // AS 5/9/08
                foreach (SplitPane split in logicalPanes)
                    this.AddLogicalChildInternal(split);
            }

			if (this._dockPanel != null)
			{
				if (this.Panes.Count > 0)
				{
					// AS 7/1/10 TFS34460
					// There seems to be a bug in the framework that can cause the 
					// IsLoaded state of elements to be incorrect. This seems to 
					// manifest itself when the elements are loaded and are the logical 
					// children of an element that is loaded but are made visual children 
					// of an element that is not loaded. However it only happens in 
					// some situations. It seems as though this is related to some 
					// seemingly fragile code in FrameworkElement where the IsLoaded state 
					// may be based upon some cached value depending on whether something 
					// is hooked into the loaded/unloaded events. To avoid this I'm 
					// deferring making the splits into visual children of the dockpanel 
					// until the dockpanel is loaded if any of the docked splits are loaded.
					//
					//this._dockPanel.Panes.ReInitialize(this._panes.DockedPanes);
					bool hasLoadedSplits = false;

					foreach (SplitPane split in _panes.DockedPanes)
					{
						if (split.IsLoaded)
						{
							hasLoadedSplits = true;
							break;
						}
					}

					if (_dockPanel.IsLoaded || !hasLoadedSplits)
						this._dockPanel.Panes.ReInitialize(this._panes.DockedPanes);
					else
					{
						_dockPanel.Loaded += new RoutedEventHandler(OnDockPanelLoaded);
					}
				}

				foreach (ContentPane pane in DockManagerUtilities.GetAllPanes(this, PaneFilterFlags.Unpinned))
					this._dockPanel.AddUnpinnedPane(pane);
			}

			this._rootPaneList.RefreshTabAreas();

			// AS 4/28/11 TFS73532
			_hasAppliedTemplate = true;
		}
		#endregion //OnApplyTemplate

		#region OnContentChanged
		/// <summary>
		/// Invoked when the <see cref="ContentControl.Content"/> property has been changed.
		/// </summary>
		/// <param name="oldContent">The previous value of the Content property</param>
		/// <param name="newContent">The new value of the Content property</param>
		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);

			DocumentContentHost newContentHost = newContent as DocumentContentHost;
			DocumentContentHost oldContentHost = oldContent as DocumentContentHost;

			// set or clear the HasDocumentContentHost based on the content
			this.SetValue(HasDocumentContentHostPropertyKey, null != newContentHost ? KnownBoxes.TrueBox : DependencyProperty.UnsetValue);

			if (oldContentHost != null)
				oldContentHost.ClearValue(PaneLocationPropertyKey);

			if (newContentHost != null)
				newContentHost.SetValue(PaneLocationPropertyKey, DockManagerKnownBoxes.PaneLocationDocumentBox);

			// AS 9/8/09 TFS21746
			// The Content of a ContentControl such as xamDockManager is the logical child of the 
			// contentcontrol. Since wpf prefers the logical tree when propogating inherited properties 
			// that means that the HwndHostInfo property of the DockManagerPanel will not propogate to 
			// the Content of the XDM. So if that content is a FrameworkElement and the FE contains an 
			// HwndHost we won't know about it so we have to set our inherited property directly on the 
			// root content if its an element.
			//
			FrameworkElement newFeContent = newContent as FrameworkElement;
			FrameworkElement oldFeContent = oldContent as FrameworkElement;

			if (oldFeContent != null)
				oldFeContent.ClearValue(HwndHostInfo.HwndHostProperty);

			if (newFeContent != null)
				HwndHostInfo.SetHwndHost(newFeContent, new HwndHostInfo(this));

			// AS 10/5/09 NA 2010.1 - LayoutMode
			this.CoerceValue(LayoutModeProperty);
		}
		#endregion //OnContentChanged

		// AS 3/17/10 TFS27618
		// Changed to overriding the OnPreviewGotKeyboardFocus instead of OnGotKeyboardFocus.
		//
		// AS 4/24/08
		// See comment in static ctor regarding focusable.
		//
		#region OnPreviewGotKeyboardFocus
		// AS 3/17/10 TFS27618
		///// <summary>
		///// Invoked when the element receives keyboard focus.
		///// </summary>
		///// <param name="e">Provides data about the event</param>
		//protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		//{
		//    base.OnGotKeyboardFocus(e);
		/// <summary>
		/// Invoked when the element is about to receive keyboard focus.
		/// </summary>
		/// <param name="e">Provides data about the event</param>
		protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnPreviewGotKeyboardFocus(e);

			// AS 3/17/10 TFS27618
			// Since we are now handling PreviewGotKeyboardFocus, we could get in here while 
			// we are initializing the FocusManager.FocusedElement of the FocusScope containing 
			// the XamDockManager.
			//
			if (_isSettingFocusedElement)
				return;

            // AS 10/15/08 TFS6271
            // If we're giving focus specifically to the dockmanager then 
            // do not try to retake focus.
            //
            if (this._forcingFocus)
                return;

			// AS 6/24/11 FloatingWindowCaptionSource
			// I don't think this is specific to this feature change but part of the fact that 
			// the panetoolwindow's draghelper may have had focus. In any case if the active pane 
			// manager isn't support to be changing the active pane then we shouldn't be doing 
			// that either while we are in the middle of processing a drop.
			//
			if (this.ActivePaneManager.IsVerificationSuspended)
				return;

			// AS 6/4/08 BR33654
			// We want to make the DM be the focused element of its 
			// focus scope if the focus should go into a pane within the dockmanager.
			//
			DependencyObject dep = e.NewFocus as DependencyObject;

            #region Refactored
            
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

            #endregion //Refactored
            this.EnsureIsFocusedElement(dep);

			// AS 6/14/12 TFS99504
			if (this._isActivatingPane)
				return;

            // AS 10/8/08 TFS8629
            // When you call SetFocus on an element, the KeyboardDevice's TryChangeFocus
            // will try to ensure that focus is within the hwnd. If it wasn't then they 
            // try to restore the cached last focused element, which could be a xamDockManager.
            // So if focus was within a WindowsFormsHost within a pane, that pane would be the
            // xdm's activepane. When something (including our own code) tries to focus an 
            // element in another pane, that acquire focus to shift focus from the wfh hwnd
            // to the hwnd of the wpf window would restore focus to the dockmanager before
            // it tried to focus the element whose SetFocus was invoked. If focus was changed 
            // during that acquire focus handling then they wouldn't try to focus the original
            // element whose SetFocus was invoked. To avoid this we'll not shift focus into
            // a pane if the old focused element was null unless the xdm is not loaded (which 
            // would likely mean that someone was trying to put focus within the xdm when the 
            // window was first shown).
            //
            //if (e.NewFocus == this)
			// AS 7/14/09
			// When the fix for TFS16492 was implemented, the unit test for 13715 was broken 
			// although not the issue that was reported. Essentially now that the DragHelper 
			// remains a logical, focusable child of the xdm, it can and will get focus. So 
			// just as we shift focus out of the xdm when it explicitly gets focus, likewise 
			// we have to do that when the drag helper gets focus (unless we're explicitly 
			// focusing it.
			//
            //if (e.NewFocus == this && (e.OldFocus != null || false == this.IsLoaded))
            if ((e.NewFocus == this || (_dragHelper != null && e.NewFocus == _dragHelper)) 
				&& (e.OldFocus != null || false == this.IsLoaded))
			{
                // make sure we have an active document if possible
				this.ActivePaneManager.VerifyActiveDocument(true);

				// activate the active pane if we have one otherwise the active document if we have one
				// we were originally just going to use the active document but that ends up causing problems
				// for example if you were to execute the ActivateNextDocument from a menu item, when the menu
				// closed (before it triggers the command) it would do a focus(null) which would focus the 
				// dockmanager. then we woudl process the command and move to the subsequent document when
				// it should have activated the last active document when a non-document pane was active
				ContentPane paneToActivate = this.ActivePane ?? this.ActivePaneManager.GetActiveDocument();

				
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

				if (null == paneToActivate)
				{
					if (this.HasDocumentContentHost == false &&
						this.DockPanel != null &&
						this.DockPanel.Child != null)
					{
						// AS 12/1/09 TFS25295
						// By invoking movefocus on the dockmanager panel itself we are potentially 
						// activating the first contentpane when we meant to try to focus something in 
						// the content area. Later we will try to find the last active pane and focus 
						// that if we couldn't focus something in the content area.
						//
						//e.Handled = this.DockPanel.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
						e.Handled = this.DockPanel.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
					}
				}

				if (e.Handled == false)
				{
					// if we haven't found a pane yet or the active pane is in a different window, 
					// search through the active panes for the first activatable pane
					Window thisWindow = Window.GetWindow(this);

					if (paneToActivate != null && Window.GetWindow(paneToActivate) != thisWindow)
						paneToActivate = null;

					if (null == paneToActivate)
					{
						PaneFilterFlags filter = PaneFilterFlags.AllVisible;
						foreach (ContentPane pane in this.ActivePaneManager.GetPanes(filter, PaneNavigationOrder.ActivationOrder, true))
						{
							if (Window.GetWindow(pane) == thisWindow)
							{
								paneToActivate = pane;
								break;
							}
						}
					}

					if (null != paneToActivate)
					{
						this.SuspendShowFlyout();

						try
						{
							e.Handled = paneToActivate.ActivateInternal();
						}
						finally
						{
							this.ResumeShowFlyout();
						}
					}
				}
			}
		}

		#endregion //OnGotKeyboardFocus

		#region OnInitialized
		/// <summary>
		/// Invoked after the element has been initialized.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnInitialized(EventArgs e)
		{
			// AS 6/23/11 TFS73499
			// We need to know the visibility of the root visual (as well as knowing 
			// when we have been removed from the visual tree). To do that we'll use 
			// a helper object that watches the HwndSource of the xamDockManager.
			//
			if (!_avoidHwndSourceHelper)
			{
				try
				{
					_hwndSourceHelper = new HwndSourceHelper(this);
				}
				catch (System.Security.SecurityException)
				{
					_avoidHwndSourceHelper = true;
				}
			}

			// we deferred updates on the collection until the element is first
			// initialized
			this._panes.EndUpdate();

			// AS 10/5/09 NA 2010.1 - LayoutMode
			this.VerifyFillPane();

			base.OnInitialized(e);

			this._activePaneManager.RebuildPaneList();
		} 
		#endregion //OnInitialized

		#region OnKeyDown

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Handled == true)
				return;

			DependencyObject dep = e.OriginalSource as DependencyObject;

			// if the element is within a toolwindow then do not process the message
			// since we would have tried already when the panetoolwindow got the key message
			if (null != dep && ToolWindow.GetToolWindow(dep) != ToolWindow.GetToolWindow(this))
				return;

			this.ProcessKeyDown(e);
		}
		#endregion //OnKeyDown

		#region OnKeyUp

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			if (e.Handled == true)
				return;

			DependencyObject dep = e.OriginalSource as DependencyObject;

			// if the element is within a toolwindow then do not process the message
			// since we would have tried already when the panetoolwindow got the key message
			if (null != dep && ToolWindow.GetToolWindow(dep) != ToolWindow.GetToolWindow(this))
				return;

			this.ProcessKeyUp(e);
		}
		#endregion //OnKeyUp

		#endregion //Base class overrides

		#region Resource Keys

		#region DropPreviewStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for the <see cref="Control"/> instance used during a drag operation to represent where a pane will be positioned when it is dropped.
		/// </summary>
		public static readonly ResourceKey DropPreviewStyleKey = new StaticPropertyResourceKey(typeof(XamDockManager), "DropPreviewStyleKey");

		#endregion //DropPreviewStyleKey

		#region ContextMenuStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for the <see cref="ContextMenu"/> instances created by the <see cref="XamDockManager"/>.
		/// </summary>
		public static readonly ResourceKey ContextMenuStyleKey = new StaticPropertyResourceKey(typeof(XamDockManager), "ContextMenuStyleKey");

		#endregion //ContextMenuStyleKey

        // AS 3/13/09 FloatingWindowDragMode
        #region FloatingWindowPreviewStyleKey

        /// <summary>
		/// The key used to identify the <see cref="Style"/> for the <see cref="Control"/> instance used during a drag operation to represent where a floating window will be positioned when it is dropped.
		/// </summary>
        public static readonly ResourceKey FloatingWindowPreviewStyleKey = new StaticPropertyResourceKey(typeof(XamDockManager), "FloatingWindowPreviewStyleKey");

        #endregion //FloatingWindowPreviewStyleKey

        #region MenuItemSeparatorStyleKey

        /// <summary>
		/// The key used to identify the <see cref="Style"/> for <see cref="Separator"/> instances that are submenu items within the <see cref="XamDockManager"/>.
		/// </summary>
		public static readonly ResourceKey MenuItemSeparatorStyleKey = new StaticPropertyResourceKey(typeof(XamDockManager), "MenuItemSeparatorStyleKey");

		#endregion //MenuItemSeparatorStyleKey

		#region MenuItemStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for <see cref="MenuItem"/> instances that are submenu items within the <see cref="XamDockManager"/>.
		/// </summary>
		public static readonly ResourceKey MenuItemStyleKey = new StaticPropertyResourceKey(typeof(XamDockManager), "MenuItemStyleKey");

		#endregion //MenuItemStyleKey

		#region ToolTipStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for <see cref="ToolTip"/> instances used in the XamDockManager.
		/// </summary>
		public static readonly ResourceKey ToolTipStyleKey = new StaticPropertyResourceKey(typeof(XamDockManager), "ToolTipStyleKey");

		#endregion //ToolTipStyleKey

		#region SubMenuItemTemplateKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for <see cref="ToolTip"/> instances used in the XamDockManager.
		/// </summary>
		public static readonly ResourceKey SubMenuItemTemplateKey = new StaticPropertyResourceKey(typeof(XamDockManager), "SubMenuItemTemplateKey");

		#endregion //SubMenuItemTemplateKey

		#endregion //Resource Keys

		#region Properties

		#region Public Properties

		#region ActivePane

		/// <summary>
		/// Identifies the <see cref="ActivePane"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivePaneProperty = ActivePaneManager.ActivePaneProperty.AddOwner(typeof(XamDockManager));

		/// <summary>
		/// Returns the current active ContentPane.
		/// </summary>
		/// <seealso cref="ActivePaneProperty"/>
		//[Description("Returns the current active ContentPane.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public ContentPane ActivePane
		{
			get
			{
				return (ContentPane)this.GetValue(XamDockManager.ActivePaneProperty);
			}
		}

		#endregion //ActivePane

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region AllowMinimizeFloatingWindows

		/// <summary>
		/// Identifies the <see cref="AllowMinimizeFloatingWindows"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowMinimizeFloatingWindowsProperty = DependencyProperty.Register("AllowMinimizeFloatingWindows",
			typeof(bool), typeof(XamDockManager), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a value indicating whether the window used to contain the floating panes may be minimized.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default the floating windows may not be maximized or minimized. This property may be used to enable the end user to 
		/// be able to minimize and restore up (i.e. unminimize) the floating window. Typically when one enables this feature, one would also want 
		/// to set the <see cref="ShowFloatingWindowsInTaskbar"/> to true so that the floating windows can be restored within the taskbar.</p>
		/// <p class="note"><b>Note:</b> This property is only supported when the PaneToolWindow is hosted in a WPF Window.</p>
		/// </remarks>
		/// <seealso cref="AllowMinimizeFloatingWindowsProperty"/>
		/// <seealso cref="AllowMaximizeFloatingWindows"/>
		//[Description("Returns/sets a value indicating whether the window used to contain the floating panes may be minimized.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool AllowMinimizeFloatingWindows
		{
			get
			{
				return (bool)this.GetValue(XamDockManager.AllowMinimizeFloatingWindowsProperty);
			}
			set
			{
				this.SetValue(XamDockManager.AllowMinimizeFloatingWindowsProperty, value);
			}
		}

		#endregion //AllowMinimizeFloatingWindows

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region AllowMaximizeFloatingWindows

		/// <summary>
		/// Identifies the <see cref="AllowMaximizeFloatingWindows"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowMaximizeFloatingWindowsProperty = DependencyProperty.Register("AllowMaximizeFloatingWindows",
			typeof(bool), typeof(XamDockManager), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a value indicating whether the window used to contain the floating panes may be maximized.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default the floating windows may not be maximized or minimized. This property may be used to enable the end user to 
		/// be able to maximize and restore down (i.e. unmaximize) the floating window.</p>
		/// <p class="note"><b>Note:</b> Some operating systems such as Windows 7 allow a window to be dragged into a maximized state. If this property 
		/// is set to true and you want to enable this capability then you must set the <see cref="FloatingWindowDragMode"/> to <b>UseSystemWindowDrag</b>. 
		/// Please be aware that there are some limitations when using this mode such as setting the 
		/// <see cref="Infragistics.Windows.DockManager.Dragging.MoveWindowAction.NewLocation"/> will not be supported.</p>
		/// <p class="note"><b>Note:</b> This property is only supported when the PaneToolWindow is hosted in a WPF Window.</p>
		/// </remarks>
		/// <seealso cref="AllowMaximizeFloatingWindowsProperty"/>
		/// <seealso cref="AllowMinimizeFloatingWindows"/>
		//[Description("Returns/sets a value indicating whether the window used to contain the floating panes may be maximized.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool AllowMaximizeFloatingWindows
		{
			get
			{
				return (bool)this.GetValue(XamDockManager.AllowMaximizeFloatingWindowsProperty);
			}
			set
			{
				this.SetValue(XamDockManager.AllowMaximizeFloatingWindowsProperty, value);
			}
		}

		#endregion //AllowMaximizeFloatingWindows

		#region CloseBehavior

		/// <summary>
		/// Identifies the <see cref="CloseBehavior"/> dependency property
		/// </summary>
		/// <seealso cref="CloseBehavior"/>
		public static readonly DependencyProperty CloseBehaviorProperty = DependencyProperty.Register("CloseBehavior",
			typeof(PaneActionBehavior), typeof(XamDockManager), new FrameworkPropertyMetadata(PaneActionBehavior.ActivePane));

		/// <summary>
		/// Determines whether the active pane or all panes are closed when clicking the close button (or using the Close context menu item) of a pane within a dockable TabGroup.
		/// </summary>
		/// <seealso cref="CloseBehaviorProperty"/>
		/// <seealso cref="PaneActionBehavior"/>
		/// <seealso cref="PinBehavior"/>
		//[Description("Determines whether the active pane or all panes are closed when clicking the close button (or using the Close context menu item) of a pane within a dockable TabGroup.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public PaneActionBehavior CloseBehavior
		{
			get
			{
				return (PaneActionBehavior)this.GetValue(XamDockManager.CloseBehaviorProperty);
			}
			set
			{
				this.SetValue(XamDockManager.CloseBehaviorProperty, value);
			}
		}

		#endregion //CloseBehavior

		#region CurrentFlyoutPane
		/// <summary>
		/// Returns the <see cref="ContentPane"/> currently within the <see cref="UnpinnedTabFlyout"/> or null if the flyout is not shown.
		/// </summary>
		public ContentPane CurrentFlyoutPane
		{
			get
			{
				return this._dockPanel != null && this._dockPanel.HasFlyoutPanel
					? this._dockPanel.FlyoutPanel.Pane
					: null;
			}
		}
		#endregion //CurrentFlyoutPane

		#region DockManager

		// note: this is only internal to allow internal types to override the metadata to get prop change notifications
		internal static readonly DependencyPropertyKey DockManagerPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("DockManager",
			// AS 3/26/10 TFS30153 - DockManager Optimization
			//typeof(XamDockManager), typeof(XamDockManager), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior, new PropertyChangedCallback(OnDockManagerChanged)));
			typeof(XamDockManager), typeof(XamDockManager), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior));

		
#region Infragistics Source Cleanup (Region)















































































#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Identifies the DockManager attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetDockManager"/>
		public static readonly DependencyProperty DockManagerProperty =
			DockManagerPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns the <see cref="XamDockManager"/> that the object is within or null if not used within a XamDockManager.
		/// </summary>
		/// <seealso cref="DockManagerProperty"/>
		public static XamDockManager GetDockManager(DependencyObject d)
		{
			return (XamDockManager)d.GetValue(XamDockManager.DockManagerProperty);
		}

		#endregion //DockManager

		#region DropPreviewTabLocation

		internal static readonly DependencyPropertyKey DropPreviewTabLocationPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("DropPreviewTabLocation",
			typeof(DropPreviewTabLocation), typeof(XamDockManager), new FrameworkPropertyMetadata(DropPreviewTabLocation.None));

		/// <summary>
		/// Identifies the 'DropPreviewTabLocation' attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetDropPreviewTabLocation"/>
		public static readonly DependencyProperty DropPreviewTabLocationProperty =
			DropPreviewTabLocationPropertyKey.DependencyProperty;


		/// <summary>
		/// Used within the template for the <see cref="XamDockManager.DropPreviewStyleKey"/> to determine where the tab item should be positioned within the drop preview.
		/// </summary>
		/// <seealso cref="DropPreviewTabLocationProperty"/>
		public static DropPreviewTabLocation GetDropPreviewTabLocation(DependencyObject d)
		{
			return (DropPreviewTabLocation)d.GetValue(XamDockManager.DropPreviewTabLocationProperty);
		}

		#endregion //DropPreviewTabLocation

		#region FloatingLocation

		/// <summary>
		/// Identifies the FloatingLocation attached dependency property
		/// </summary>
		/// <seealso cref="GetFloatingLocation"/>
		/// <seealso cref="SetFloatingLocation"/>
		public static readonly DependencyProperty FloatingLocationProperty = DependencyProperty.RegisterAttached("FloatingLocation",
			typeof(Point?), typeof(XamDockManager), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the location in which a floating <see cref="SplitPane"/> in the 
		/// <see cref="Panes"/> collection will be arranged. If the XamDockManager will be used 
		/// within an XBAP, these coordinates should be relative to the root visual containing 
		/// the XamDockManager.
		/// </summary>
		/// <seealso cref="FloatingLocationProperty"/>
		/// <seealso cref="SetFloatingLocation"/>
		//[Description("Returns/sets the location in which a floating SplitPane in the Panes collection will be arranged. If the XamDockManager will be used within an XBAP, these coordinates should be relative to the root visual containing the XamDockManager.")]
		//[Category("DockManager Properties")] // Layout
		[AttachedPropertyBrowsableForChildren(IncludeDescendants=false)]
		[AttachedPropertyBrowsableForType(typeof(SplitPane))]
		public static Point? GetFloatingLocation(DependencyObject d)
		{
			return (Point?)d.GetValue(XamDockManager.FloatingLocationProperty);
		}

		/// <summary>
		/// Sets the value of the 'FloatingLocation' attached property
		/// </summary>
		/// <seealso cref="FloatingLocationProperty"/>
		/// <seealso cref="GetFloatingLocation"/>
		public static void SetFloatingLocation(DependencyObject d, Point? value)
		{
			d.SetValue(XamDockManager.FloatingLocationProperty, value);
		}

		#endregion //FloatingLocation

		#region FloatingSize

		/// <summary>
		/// Identifies the FloatingSize attached dependency property
		/// </summary>
		/// <seealso cref="GetFloatingSize"/>
		/// <seealso cref="SetFloatingSize"/>
		public static readonly DependencyProperty FloatingSizeProperty = DependencyProperty.RegisterAttached("FloatingSize",
			typeof(Size), typeof(XamDockManager), new FrameworkPropertyMetadata(Size.Empty));

		/// <summary>
		/// Returns/sets the size of the <see cref="PaneToolWindow"/> that will contain a floating <see cref="SplitPane"/> in the <see cref="Panes"/> collection.
		/// </summary>
		/// <seealso cref="FloatingSizeProperty"/>
		/// <seealso cref="SetFloatingSize"/>
		//[Description("Returns/sets the size of the 'PaneToolWindow' that will contain a floating SplitPane in the Panes collection.")]
		//[Category("DockManager Properties")] // Layout
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(SplitPane))]
		public static Size GetFloatingSize(DependencyObject d)
		{
			return (Size)d.GetValue(XamDockManager.FloatingSizeProperty);
		}

		/// <summary>
		/// Sets the value of the 'FloatingSize' attached property
		/// </summary>
		/// <seealso cref="FloatingSizeProperty"/>
		/// <seealso cref="GetFloatingSize"/>
		public static void SetFloatingSize(DependencyObject d, Size value)
		{
			d.SetValue(XamDockManager.FloatingSizeProperty, value);
		}

		#endregion //FloatingSize

		// AS 6/24/11 FloatingWindowCaptionSource
		#region FloatingWindowCaptionSource

		/// <summary>
		/// Identifies the <see cref="FloatingWindowCaptionSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FloatingWindowCaptionSourceProperty = DependencyProperty.Register("FloatingWindowCaptionSource",
			typeof(FloatingWindowCaptionSource), typeof(XamDockManager), new FrameworkPropertyMetadata(FloatingWindowCaptionSource.UseToolWindowTitle));

		/// <summary>
		/// Returns or sets a value that determines what provides the caption area of a floating 'PaneToolWindow'
		/// </summary>
		/// <seealso cref="FloatingWindowCaptionSourceProperty"/>
		[Bindable(true)]
		public FloatingWindowCaptionSource FloatingWindowCaptionSource
		{
			get
			{
				return (FloatingWindowCaptionSource)this.GetValue(XamDockManager.FloatingWindowCaptionSourceProperty);
			}
			set
			{
				this.SetValue(XamDockManager.FloatingWindowCaptionSourceProperty, value);
			}
		}

		#endregion //FloatingWindowCaptionSource

		// AS 6/9/11 TFS76337
		#region FloatingWindowDoubleClickAction

		/// <summary>
		/// Identifies the <see cref="FloatingWindowDoubleClickAction"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FloatingWindowDoubleClickActionProperty = DependencyProperty.Register("FloatingWindowDoubleClickAction",
			typeof(FloatingWindowDoubleClickAction?), typeof(XamDockManager), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the action that should be taken when double clicking on the caption area of a <see cref="PaneToolWindow"/>
		/// </summary>
		/// <remarks>
		/// <p class="body">By default this property is set to null. When left set to null, the actual action taken will be based upon the 
		/// state of the ToolWindow. If the ToolWindow supports being maximized, then double clicking the caption of the PaneToolWindow 
		/// will toggle between it's 'Maximized' and 'Normal' states. Otherwise if the PaneToolWindow's PaneLocation is DockableFloating then 
		/// any ContentPane instances within it that were previously docked will be restored to their docked positiones.</p>
		/// </remarks>
		/// <seealso cref="FloatingWindowDoubleClickActionProperty"/>
		//[Description("Returns or sets the action that should be taken when double clicking on the caption area of a 'PaneToolWindow'")]
		//[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<FloatingWindowDoubleClickAction>))]
		public FloatingWindowDoubleClickAction? FloatingWindowDoubleClickAction
		{
			get
			{
				return (FloatingWindowDoubleClickAction?)this.GetValue(XamDockManager.FloatingWindowDoubleClickActionProperty);
			}
			set
			{
				this.SetValue(XamDockManager.FloatingWindowDoubleClickActionProperty, value);
			}
		}
		#endregion //FloatingWindowDoubleClickAction

        // AS 3/13/09 FloatingWindowDragMode
        #region FloatingWindowDragMode

        /// <summary>
        /// Identifies the <see cref="FloatingWindowDragMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FloatingWindowDragModeProperty = DependencyProperty.Register("FloatingWindowDragMode",
            typeof(FloatingWindowDragMode), typeof(XamDockManager), new FrameworkPropertyMetadata(FloatingWindowDragMode.Immediate));

        /// <summary>
        /// Returns/sets a value that indicates how a floating pane should be repositioned during a drag operation.
        /// </summary>
        /// <seealso cref="FloatingWindowDragModeProperty"/>
        //[Description("Returns/sets a value that indicates how a floating pane should be repositioned during a drag operation.")]
        //[Category("Behavior")]
        [Bindable(true)]
        public FloatingWindowDragMode FloatingWindowDragMode
        {
            get
            {
                return (FloatingWindowDragMode)this.GetValue(XamDockManager.FloatingWindowDragModeProperty);
            }
            set
            {
                this.SetValue(XamDockManager.FloatingWindowDragModeProperty, value);
            }
        }

        #endregion //FloatingWindowDragMode

		// AS 6/23/11 TFS73499
		#region FloatingWindowVisibility

		/// <summary>
		/// Identifies the <see cref="FloatingWindowVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FloatingWindowVisibilityProperty = DependencyProperty.Register("FloatingWindowVisibility",
			typeof(FloatingWindowVisibility), typeof(XamDockManager), new FrameworkPropertyMetadata(FloatingWindowVisibility.Visible, new PropertyChangedCallback(OnFloatingWindowVisibilityChanged)));

		private static void OnFloatingWindowVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var dm = d as XamDockManager;
			dm.VerifyFloatingWindowVisibility();
		}

		/// <summary>
		/// Returns or sets a value indicating that is used to determine the Visibility of the floating windows.
		/// </summary>
		/// <seealso cref="FloatingWindowVisibilityProperty"/>
		[Bindable(true)]
		public FloatingWindowVisibility FloatingWindowVisibility
		{
			get
			{
				return (FloatingWindowVisibility)this.GetValue(XamDockManager.FloatingWindowVisibilityProperty);
			}
			set
			{
				this.SetValue(XamDockManager.FloatingWindowVisibilityProperty, value);
			}
		}

		#endregion //FloatingWindowVisibility

		#region FlyoutAnimation

		/// <summary>
		/// Identifies the <see cref="FlyoutAnimation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FlyoutAnimationProperty = DependencyProperty.Register("FlyoutAnimation",
			typeof(PaneFlyoutAnimation), typeof(XamDockManager), new FrameworkPropertyMetadata(PaneFlyoutAnimation.Slide, new PropertyChangedCallback(OnFlyoutAnimationChanged)));

		private static void OnFlyoutAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>
		/// Determines the type of animation, if any, that occurs when the flyout pane is displayed or hidden.
		/// </summary>
		/// <seealso cref="FlyoutAnimationProperty"/>
		//[Description("Determines the type of animation, if any, that occurs when the flyout pane is displayed or hidden.")]
		//[Category("DockManager Properties")] // Layout
		[Bindable(true)]
		public PaneFlyoutAnimation FlyoutAnimation
		{
			get
			{
				return (PaneFlyoutAnimation)this.GetValue(XamDockManager.FlyoutAnimationProperty);
			}
			set
			{
				this.SetValue(XamDockManager.FlyoutAnimationProperty, value);
			}
		}

		#endregion //FlyoutAnimation

        #region HasDocumentContentHost

		private static readonly DependencyPropertyKey HasDocumentContentHostPropertyKey =
			DependencyProperty.RegisterReadOnly("HasDocumentContentHost",
			typeof(bool), typeof(XamDockManager), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasDocumentContentHost"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasDocumentContentHostProperty =
			HasDocumentContentHostPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the <see cref="ContentControl.Content"/> property has been set to a <see cref="DocumentContentHost"/> that provides the tabbed document behavior for the content.
		/// </summary>
		/// <seealso cref="HasDocumentContentHostProperty"/>
		/// <seealso cref="DocumentContentHost"/>
		/// <seealso cref="ContentControl.Content"/>
		//[Description("Returns a boolean indicating whether the Content property has been set to a DocumentContentHost that provides the tabbed document behavior for the content.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool HasDocumentContentHost
		{
			get
			{
				return (bool)this.GetValue(XamDockManager.HasDocumentContentHostProperty);
			}
		}

		#endregion //HasDocumentContentHost

		#region InitialLocation

		/// <summary>
		/// Identifies the InitialLocation attached dependency property
		/// </summary>
		/// <seealso cref="GetInitialLocation"/>
		/// <seealso cref="SetInitialLocation"/>
		public static readonly DependencyProperty InitialLocationProperty = DependencyProperty.RegisterAttached("InitialLocation",
			typeof(InitialPaneLocation), typeof(XamDockManager), new FrameworkPropertyMetadata(InitialPaneLocation.DockedLeft, new PropertyChangedCallback(OnInitialLocationChanged)));

		private static void OnInitialLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SplitPane split = d as SplitPane;

			if (null == split)
				throw new InvalidOperationException(GetString("LE_InitialLocationForSplitOnly"));

			XamDockManager dockManager = XamDockManager.GetDockManager(d);

			if (dockManager != null)
			{
				// AS 5/20/08 BR31679
				// In VS 2008, changing a property doesn't cause the object to be recreated
				// and in the case of the InitialLocation, we do want to allow that to be changed
				// so we'll allow it in design mode. Since we do different processing based on
				// the initial location, we'll process this as if it was removed and readded
				// in the same spot.
				//
				if (DesignerProperties.GetIsInDesignMode(dockManager))
				{
					int index = dockManager.Panes.IndexOf(split);

					if (index >= 0)
					{
						dockManager.Panes.RemoveAt(index);
						dockManager.Panes.Insert(index, split);
						return;
					}
				}

				throw new InvalidOperationException(GetString("LE_CannotChangeInitialLocation"));
			}
		}

		/// <summary>
		/// Returns/sets where the <see cref="SplitPane"/> in the <see cref="Panes"/> collection will be displayed when the XamDockManager is initially displayed. Once the pane has been initialized, this value is no longer used.
		/// </summary>
		/// <seealso cref="InitialLocationProperty"/>
		/// <seealso cref="SetInitialLocation"/>
		//[Description("Returns/sets where the pane in the Panes collection will be displayed when the XamDockManager is initially displayed.")]
		//[Category("DockManager Properties")] // Layout
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(SplitPane))]
		public static InitialPaneLocation GetInitialLocation(DependencyObject d)
		{
			return (InitialPaneLocation)d.GetValue(XamDockManager.InitialLocationProperty);
		}

		/// <summary>
		/// Sets the value of the 'InitialLocation' attached property
		/// </summary>
		/// <seealso cref="InitialLocationProperty"/>
		/// <seealso cref="GetInitialLocation"/>
		public static void SetInitialLocation(DependencyObject d, InitialPaneLocation value)
		{
			d.SetValue(XamDockManager.InitialLocationProperty, DockManagerKnownBoxes.FromValue(value));
		}

		#endregion //InitialLocation

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region LayoutMode

		/// <summary>
		/// Identifies the <see cref="LayoutMode"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public static readonly DependencyProperty LayoutModeProperty = DependencyProperty.Register("LayoutMode",
			typeof(DockedPaneLayoutMode), typeof(XamDockManager), new FrameworkPropertyMetadata(DockedPaneLayoutMode.Standard, new PropertyChangedCallback(OnLayoutModeChanged), new CoerceValueCallback(CoerceLayoutMode)));

		private static object CoerceLayoutMode(DependencyObject d, object newValue)
		{
			if (false == DockedPaneLayoutMode.Standard.Equals(newValue) &&
				((XamDockManager)d).HasContent)
			{
				return DockedPaneLayoutMode.Standard;
			}

			return newValue;
		}

		private static void OnLayoutModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDockManager dm = (XamDockManager)d;

			if (null != dm._dockPanel)
			{
				dm._dockPanel.InvalidateMeasure();
				dm._dockPanel.InvalidateArrange();
			}

			dm.VerifyFillPane();
		}

		/// <summary>
		/// Returns or sets a value which determines how to position the docked <see cref="SplitPane"/> instances 
		/// in the <see cref="XamDockManager.Panes"/> collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">When set to <b>Standard</b>, which is the default value, the docked 
		/// <see cref="SplitPane"/> instances are positioned along the edges of the DockManagerPanel 
		/// around the <see cref="ContentControl.Content"/>.</p> 
		/// <p class="body">When set to <b>FillContainer</b>, the innermost <b>SplitPane</b>, the 
		/// last visible SplitPane in the <see cref="Panes"/> collection whose PaneLocation is 
		/// one of the docked values (e.g DockedLeft, DockedRight, DockedTop or DockedBottom), fills the 
		/// remaining space. This means that the SplitPane set to fill the available area may change as 
		/// the ContentPanes are docked/undocked, closed and pinned/unpinned.</p>
		/// <p class="note">Note: If the <see cref="ContentControl.Content"/> property is set to a non-null value, the 
		/// property will be coerced to Standard.</p>
		/// </remarks>
		/// <seealso cref="DockedPaneLayoutMode"/>
		//[Description("Determines how the docked SplitPanes are arranged within the XamDockManager")]
		//[Category("DockManager Properties")] //Layout
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public DockedPaneLayoutMode LayoutMode
		{
			get
			{
				return (DockedPaneLayoutMode)this.GetValue(XamDockManager.LayoutModeProperty);
			}
			set
			{
				this.SetValue(XamDockManager.LayoutModeProperty, value);
			}
		}

		#endregion //LayoutMode

		#region NavigationOrder

		/// <summary>
		/// Identifies the <see cref="NavigationOrder"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NavigationOrderProperty = DependencyProperty.Register("NavigationOrder",
			typeof(PaneNavigationOrder), typeof(XamDockManager), new FrameworkPropertyMetadata(PaneNavigationOrder.ActivationOrder));

		/// <summary>
		/// Returns/sets an enumeration indicating the order in which panes are navigated using the keyboard or the PaneNavigator.
		/// </summary>
		/// <seealso cref="NavigationOrderProperty"/>
		//[Description("Returns/sets an enumeration indicating the order in which panes are navigated using the keyboard or the PaneNavigator.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public PaneNavigationOrder NavigationOrder
		{
			get
			{
				return (PaneNavigationOrder)this.GetValue(XamDockManager.NavigationOrderProperty);
			}
			set
			{
				this.SetValue(XamDockManager.NavigationOrderProperty, value);
			}
		}

		#endregion //NavigationOrder

		#region Panes
		/// <summary>
		/// Returns a collection of the root level panes that are displayed with the <see cref="XamDockManager"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Panes collection represents the root <see cref="SplitPane"/> instances that are docked to one 
		/// of the edges of the XamDockManager or floating above it (whether it is dockable or not). To initialize the default 
		/// location, you can use the attached <see cref="InitialLocationProperty"/>. For a floating pane, you can use the 
		/// attached <see cref="FloatingLocationProperty"/> to control where the floating pane is position.</p>
		/// </remarks>
		//[Description("The collection of split panes displayed within the XamDockManager representing the root floating and docked panes.")]
		//[Category("DockManager Properties")] // Data
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ObservableCollectionExtended<SplitPane> Panes
		{
			get { return this._panes; }
		}
		#endregion //Panes

		#region PaneLocation

		internal static readonly DependencyPropertyKey PaneLocationPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("PaneLocation",
			// AS 3/26/10 TFS30153 - PaneLocation Optimization
			//typeof(PaneLocation), typeof(XamDockManager), new FrameworkPropertyMetadata(PaneLocation.Unknown, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior, new PropertyChangedCallback(OnPaneLocationChanged)));
			typeof(PaneLocation), typeof(XamDockManager), new FrameworkPropertyMetadata(PaneLocation.Unknown, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior));

		
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Identifies the PaneLocation attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetPaneLocation"/>
		public static readonly DependencyProperty PaneLocationProperty =
			PaneLocationPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns the current location indicating where the element exists within the XamDockManager.
		/// </summary>
		/// <seealso cref="PaneLocationProperty"/>
		public static PaneLocation GetPaneLocation(DependencyObject d)
		{
			return (PaneLocation)d.GetValue(XamDockManager.PaneLocationProperty);
		}

		#endregion //PaneLocation

		#region PaneNavigatorButtonDisplayMode

		/// <summary>
		/// Identifies the <see cref="PaneNavigatorButtonDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PaneNavigatorButtonDisplayModeProperty = DependencyProperty.Register("PaneNavigatorButtonDisplayMode",
			typeof(PaneNavigatorButtonDisplayMode), typeof(XamDockManager), new FrameworkPropertyMetadata(PaneNavigatorButtonDisplayMode.WhenHostedInBrowser));

		/// <summary>
		/// Determines when a button that can be used to display the 'PaneNavigator' is displayed within the XamDockManager.
		/// </summary>
		/// <seealso cref="PaneNavigatorButtonDisplayModeProperty"/>
		//[Description("Determines when a button that can be used to display the 'PaneNavigator' is displayed within the XamDockManager.")]
		//[Category("DockManager Properties")] // Appearance
		[Bindable(true)]
		public PaneNavigatorButtonDisplayMode PaneNavigatorButtonDisplayMode
		{
			get
			{
				return (PaneNavigatorButtonDisplayMode)this.GetValue(XamDockManager.PaneNavigatorButtonDisplayModeProperty);
			}
			set
			{
				this.SetValue(XamDockManager.PaneNavigatorButtonDisplayModeProperty, value);
			}
		}

		#endregion //PaneNavigatorButtonDisplayMode

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region ShowFloatingWindowsInTaskbar

		/// <summary>
		/// Identifies the <see cref="ShowFloatingWindowsInTaskbar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowFloatingWindowsInTaskbarProperty = DependencyProperty.Register("ShowFloatingWindowsInTaskbar",
			typeof(bool), typeof(XamDockManager), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a value indicating whether the window used to contain the floating panes will be displayed within the OS taskbar when a WPF window is used to host the floating panes.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default the floating windows do not show up in the OS taskbar. Setting this property to true will cause them to be shown in the taskbar. This is particularly 
		/// useful when the floating windows are allowed to be minimized but also when the floating windows are not owned windows since the window may then be behind the main window.</p>
		/// <p class="note"><b>Note:</b> This property only has an effect when the PaneToolWindow is hosted in a WPF Window.</p>
		/// </remarks>
		/// <seealso cref="ShowFloatingWindowsInTaskbarProperty"/>
		//[Description("Returns/sets a value indicating whether the window used to contain the floating panes will be displayed within the OS taskbar when a WPF window is used to host the floating panes.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool ShowFloatingWindowsInTaskbar
		{
			get
			{
				return (bool)this.GetValue(XamDockManager.ShowFloatingWindowsInTaskbarProperty);
			}
			set
			{
				this.SetValue(XamDockManager.ShowFloatingWindowsInTaskbarProperty, value);
			}
		}

		#endregion //ShowFloatingWindowsInTaskbar

		// AS 11/12/09 TFS24789 - TabItemDragBehavior
		#region TabItemDragBehavior

		/// <summary>
		/// Identifies the <see cref="TabItemDragBehavior"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabItemDragBehaviorProperty = DependencyProperty.Register("TabItemDragBehavior",
			typeof(TabItemDragBehavior), typeof(XamDockManager), new FrameworkPropertyMetadata(TabItemDragBehavior.DisplayTabPreview));

		/// <summary>
		/// Returns or sets a value that determines the type of visual indication provides during a drag operation when over the tab item area of a TabGroupPane.
		/// </summary>
		/// <seealso cref="TabItemDragBehaviorProperty"/>
		//[Description("Returns or sets a value that determines the type of visual indication provides during a drag operation when over the tab item area of a TabGroupPane.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public TabItemDragBehavior TabItemDragBehavior
		{
			get
			{
				return (TabItemDragBehavior)this.GetValue(XamDockManager.TabItemDragBehaviorProperty);
			}
			set
			{
				this.SetValue(XamDockManager.TabItemDragBehaviorProperty, value);
			}
		}

		#endregion //TabItemDragBehavior

		#region Theme

		/// <summary>
		/// Identifies the 'Theme' dependency property
		/// </summary>
		public static readonly DependencyProperty ThemeProperty = ThemeManager.ThemeProperty.AddOwner(typeof(XamDockManager), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThemeChanged)));

		/// <summary>
		/// Routed event used to notify when the <see cref="Theme"/> property has been changed.
		/// </summary>
		public static readonly RoutedEvent ThemeChangedEvent = ThemeManager.ThemeChangedEvent.AddOwner(typeof(XamDockManager));

		private static void OnThemeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamDockManager control = target as XamDockManager;

			if (control.IsInitialized)
			{
				control.InvalidateMeasure();
				control.UpdateLayout();
			}

			control.OnThemeChanged((string)(e.OldValue), (string)(e.NewValue));
		}

		/// <summary>
		/// Gets/sets the default look for the control.
		/// </summary>
		/// <remarks>
		/// <para class="body">If left set to null then the default 'Generic' theme will be used. 
		/// This property can be set to the name of any registered theme (see <see cref="Infragistics.Windows.Themes.ThemeManager.Register(string, string, ResourceDictionary)"/> and <see cref="Infragistics.Windows.Themes.ThemeManager.GetThemes()"/> methods).</para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.Themes.ThemeManager"/>
		/// <seealso cref="ThemeProperty"/>
		//[Description("Gets/sets the general look of the XamDockManager and its elements.")]
		//[Category("DockManager Properties")] // Appearance
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Themes.Internal.DockManagerThemeTypeConverter))]
		public string Theme
		{
			get
			{
				return (string)this.GetValue(XamDockManager.ThemeProperty);
			}
			set
			{
				this.SetValue(XamDockManager.ThemeProperty, value);
			}
		}

		/// <summary>
		/// Used to raise the <see cref="ThemeChanged"/> event.
		/// </summary>
		protected virtual void OnThemeChanged(string previousValue, string currentValue)
		{
			RoutedPropertyChangedEventArgs<string> newEvent = new RoutedPropertyChangedEventArgs<string>(previousValue, currentValue);
			newEvent.RoutedEvent = XamDockManager.ThemeChangedEvent;
			newEvent.Source = this;
			this.RaiseEvent(newEvent);
		}

		/// <summary>
		/// Occurs when the <see cref="Theme"/> property has been changed.
		/// </summary>
		//[Description("Occurs when the 'Theme' property has been changed.")]
		//[Category("DockManager Events")] // Behavior
		public event RoutedPropertyChangedEventHandler<string> ThemeChanged
		{
			add
			{
				base.AddHandler(XamDockManager.ThemeChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamDockManager.ThemeChangedEvent, value);
			}
		}

		#endregion //Theme

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region UseOwnedFloatingWindows

		/// <summary>
		/// Identifies the <see cref="UseOwnedFloatingWindows"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UseOwnedFloatingWindowsProperty = DependencyProperty.Register("UseOwnedFloatingWindows",
			typeof(bool), typeof(XamDockManager), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets a value indicating whether the window used to contain the floating panes will be an owned window of the Window containing the xamDockManager
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, the floating windows are owned windows. As such the floating windows will always stay above the owner window and will be hidden 
		/// when the owning window is minimized. Setting this property to false indicates the floating window should not be owned. Therefore it may go behind the main window 
		/// with regards to z-order.</p>
		/// <p class="note"><b>Note:</b> This property only has an effect when the PaneToolWindow is hosted in a WPF Window.</p>
		/// </remarks>
		/// <seealso cref="UseOwnedFloatingWindowsProperty"/>
		//[Description("Returns/sets a value indicating whether the window used to contain the floating panes will be an owned window of the Window containing the xamDockManager")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool UseOwnedFloatingWindows
		{
			get
			{
				return (bool)this.GetValue(XamDockManager.UseOwnedFloatingWindowsProperty);
			}
			set
			{
				this.SetValue(XamDockManager.UseOwnedFloatingWindowsProperty, value);
			}
		}

		#endregion //UseOwnedFloatingWindows

		#region PinBehavior

		/// <summary>
		/// Identifies the <see cref="PinBehavior"/> dependency property
		/// </summary>
		/// <seealso cref="PinBehavior"/>
		public static readonly DependencyProperty PinBehaviorProperty = DependencyProperty.Register("PinBehavior",
			typeof(PaneActionBehavior), typeof(XamDockManager), new FrameworkPropertyMetadata(PaneActionBehavior.AllPanes));

		/// <summary>
		/// Determines whether the active pane or all panes are unpinned when clicking the auto hide button (or using the Auto Hide context menu item) of a pane within a dockable TabGroup.
		/// </summary>
		/// <seealso cref="PinBehaviorProperty"/>
		/// <seealso cref="PaneActionBehavior"/>
		/// <seealso cref="PinBehavior"/>
		//[Description("Determines whether the active pane or all panes are unpinned when clicking the auto hide button (or using the Auto Hide context menu item) of a pane within a dockable TabGroup.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public PaneActionBehavior PinBehavior
		{
			get
			{
				return (PaneActionBehavior)this.GetValue(XamDockManager.PinBehaviorProperty);
			}
			set
			{
				this.SetValue(XamDockManager.PinBehaviorProperty, value);
			}
		}

		#endregion //PinBehavior

		// AS 9/29/09 NA 2010.1 - UnpinnedTabHoverAction
		#region UnpinnedTabHoverAction

		/// <summary>
		/// Identifies the <see cref="UnpinnedTabHoverAction"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public static readonly DependencyProperty UnpinnedTabHoverActionProperty = DependencyProperty.Register("UnpinnedTabHoverAction",
				typeof(UnpinnedTabHoverAction), typeof(XamDockManager), new FrameworkPropertyMetadata(UnpinnedTabHoverAction.Flyout));

		/// <summary>
		/// Returns or sets what action should occur when the mouse is hovered over an unpinned tab item.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, when the mouse hovers over an unpinned tab, the flyout for the ContentPane associated 
		/// with that tab will be displayed.</p>
		/// </remarks>
		/// <seealso cref="UnpinnedTabHoverActionProperty"/>
		//[Description("Returns or sets what action should occur when the mouse is hovered over an unpinned tab item.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public UnpinnedTabHoverAction UnpinnedTabHoverAction
		{
			get
			{
				return (UnpinnedTabHoverAction)this.GetValue(XamDockManager.UnpinnedTabHoverActionProperty);
			}
			set
			{
				this.SetValue(XamDockManager.UnpinnedTabHoverActionProperty, value);
			}
		}

		#endregion //UnpinnedTabHoverAction

		#endregion //Public Properties

		#region Internal Properties

		#region ActivePaneManager
		/// <summary>
		/// Returns the object that manages the current active pane
		/// </summary>
		internal ActivePaneManager ActivePaneManager
		{
			get { return this._activePaneManager; }
		} 
		#endregion // ActivePaneManager

		#region Commands
		internal CommandsBase Commands
		{
			get { return this._commands; }
		}
		#endregion //Commands

		// AS 6/23/11 TFS73499
		#region ComputedFloatingWindowVisibility

		private static readonly DependencyPropertyKey ComputedFloatingWindowVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("ComputedFloatingWindowVisibility",
			typeof(Visibility), typeof(XamDockManager), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, new PropertyChangedCallback(OnComputedFloatingWindowVisibilityChanged)));

		private static void OnComputedFloatingWindowVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var dm = d as XamDockManager;

			if (dm._toolWindows != null)
			{
				foreach (var tw in dm._toolWindows)
					tw.CoerceValue(VisibilityProperty);
			}
		}

		/// <summary>
		/// Identifies the <see cref="ComputedFloatingWindowVisibility"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty ComputedFloatingWindowVisibilityProperty =
			ComputedFloatingWindowVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the calculated visibility for the floating tool windows
		/// </summary>
		/// <seealso cref="ComputedFloatingWindowVisibilityProperty"/>
		internal Visibility ComputedFloatingWindowVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamDockManager.ComputedFloatingWindowVisibilityProperty);
			}
		}

		#endregion //ComputedFloatingWindowVisibility

		#region DockPanel
		/// <summary>
		/// Returns the element that contains the root split panes.
		/// </summary>
		internal DockManagerPanel DockPanel
		{
			get { return this._dockPanel; }
		} 
		#endregion //DockPanel

		#region DocumentContentHost
		internal DocumentContentHost DocumentContentHost
		{
			get { return this.Content as DocumentContentHost; }
		} 
		#endregion //DocumentContentHost

        // AS 3/13/09 FloatingWindowDragMode
        #region DragFullWindows
        internal bool DragFullWindows
        {
            get
            {
				return this.GetDragFullWindows(this.FloatingWindowDragMode);
            }
        }

		// AS 2/22/12 TFS101038
		// Moved here since we may not be honoring the drag mode.
		//
		internal bool GetDragFullWindows(FloatingWindowDragMode dragMode)
		{
            switch (dragMode)
            {
                case FloatingWindowDragMode.Deferred:
                    return false;
                default:
                case FloatingWindowDragMode.Immediate:
                    Debug.Assert(this.FloatingWindowDragMode == FloatingWindowDragMode.Immediate);
                    return true;
				case FloatingWindowDragMode.UseSystemWindowDrag: // AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
                case FloatingWindowDragMode.UseDragFullWindowsSystemSetting:
                    object value = this.TryFindResource(SystemParameters.DragFullWindowsKey);

                    if (value is bool)
                        return (bool)value;

                    return true;
            }
		}
        #endregion //DragFullWindows

        // AS 4/8/09 TFS16492
        #region DragHelper
        internal FrameworkContentElement DragHelper
        {
            get
            {
                if (_dragHelper == null)
                {
                    _dragHelper = new FrameworkContentElement();

					// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
					// Moved from DragManager
					//
					// AS 5/17/08
					// Set the name to make it easier to identify when debugging, bug reports, etc.
					_dragHelper.Name = "DockManagerDragHelper";

					// AS 4/25/08
					// We may need to put focus into this element temporarily.
					//
					_dragHelper.SetValue(FrameworkContentElement.FocusableProperty, KnownBoxes.TrueBox);

                    this.AddLogicalChildInternal(_dragHelper);
                }

                return _dragHelper;
            }
        }
        #endregion //DragHelper

		#region DragManager
		/// <summary>
		/// Returns the object that manages pane dragging.
		/// </summary>
		internal DragManager DragManager
		{
			get { return this._dragManager; }
		} 
		#endregion //DragManager

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region FillPane
		internal SplitPane FillPane
		{
			get
			{
				return null != this._fillPane
					? Utilities.GetWeakReferenceTargetSafe(_fillPane) as SplitPane
					: null;
			}
		}
		#endregion //FillPane

		#region FlyoutState
		/// <summary>
		/// Returns the current state of the flyout panel
		/// </summary>
		internal UnpinnedFlyoutState FlyoutState
		{
			get
			{
				if (null != this._dockPanel && this._dockPanel.HasFlyoutPanel)
					return this._dockPanel.FlyoutPanel.CurrentState;

				return UnpinnedFlyoutState.Closed;
			}
		}
		#endregion //FlyoutState

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region HasHwndHost
        internal bool HasHwndHost
        {
            get
            {
                if (null == _hasHwndHost)
                {
                    _hasHwndHost = false;

                    if (this._dockPanel != null && _dockPanel.HasHwndHost)
                        _hasHwndHost = true;
					// AS 9/8/09 TFS21746
					// The foreach block below used to be in this else block. We only need 
					// to check the contentpane if the content itself doesn't have an hwndhost.
					//
                    //else
					else if (this.Content is FrameworkElement)
					{
						HwndHostInfo hhi = HwndHostInfo.GetHwndHost((DependencyObject)this.Content);

						if (null != hhi)
							_hasHwndHost = hhi.HasHwndHost;
					}

					if (_hasHwndHost == false)
					{
                        foreach (ContentPane cp in this.GetPanes(PaneNavigationOrder.VisibleOrder))
                        {
                            if (cp.HasHwndHost)
                            {
                                _hasHwndHost = true;
                                break;
                            }
                        }
                    }

					// AS 4/17/09 TFS16807/Optimization
					// This is an optimization but we should only create this 
					// handler when we know we have an HwndHost.
					//
					this._activePaneManager.VerifyHasPreprocessHandler();

                    HwndHostInfo.OnHasHwndHostChanged(this);
                }

                return _hasHwndHost.Value;
            }
        }

		// AS 3/26/10 TFS30153 - DockManager Optimization
		internal bool? HasHwndHostNoVerify
		{
			get { return _hasHwndHost; }
		}
        #endregion //HasHwndHost

		// AS 6/14/12 TFS99504
		#region IsActivatingPane
		internal bool IsActivatingPane
		{
			get { return _isActivatingPane; }
			set { _isActivatingPane = value; }
		} 
		#endregion //IsActivatingPane

		// AS 7/14/09 TFS18400
		#region IsClosingPanes
		internal bool IsClosingPanes
		{
			get
			{
				return _isClosingPanes;
			}
		}
		#endregion //IsClosingPanes

		#region IsLoadingLayout
		internal bool IsLoadingLayout
		{
			get { return this._isLoadingLayout; }
			set 
			{
				if (value != this._isLoadingLayout)
				{
					this._isLoadingLayout = value;

					if (value == false)
					{
						// when we come out of loading a layout we need to send an async message
						// and until we receive that we should not show the flyout. the reason this
						// is needed is because the content panes asynchronously verify their
						// pinned state so when they get their async callback the xdm will try to
						// show the flyout
						this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Infragistics.Windows.DockManager.DockManagerUtilities.MethodInvoker(this.ResumeShowFlyout));
					}
					else
						this.SuspendShowFlyout();
				}
			}
		} 
		#endregion //IsLoadingLayout

		#region IsShowFlyoutSuspended
		internal bool IsShowFlyoutSuspended
		{
			get { return this._suspendShowFlyoutCount > 0; }
		} 
		#endregion //IsShowFlyoutSuspended

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region ShowFlyoutInToolWindow
        internal bool ShowFlyoutInToolWindow
        {
            get
            {
                // if we have any hwndhosts then we need to show the flyout
                // in a toolwindow so that it is displayed above the hwnds
                // of those hwndhost controls
                if (this.HasHwndHost)
                {
                    if (BrowserInteropHelper.IsBrowserHosted)
                        return DockManagerUtilities.HasUnmanagedCodeRights;

                    return true;
                }

                return false;
            }
        }
        #endregion //ShowFlyoutInToolWindow

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region ShowSplitterInPopup
        internal bool ShowSplitterInPopup
        {
            // as with the flyout, we want the splitter to show the preview in a popup
            // if there are any hwnd hosts instead of using the adorner layer
            get { return this.ShowFlyoutInToolWindow; }
        } 
        #endregion //ShowSplitterInPopup

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region ShowToolWindowsInPopup
        internal bool ShowToolWindowsInPopup
        {
            get
            {
                // if we have hwnd hosts within the dm then we may want to 
                // use a popup since that is a separate hwnd. otherwise the 
                // floating windows could be hosted within the adorner layer
                // which means they will be behind any hwndhosts
                if (this.HasHwndHost)
                {
                    // if we're hosted in a browser then we only want to 
                    // do this if we have unmanaged code rights. basically the 
                    // popup class does not manage its own z-order so the popup
                    // would fall behind hwndhosts which means its no better than
                    // putting it in the adorner layer. and having it in a popup 
                    // when its in the browser has the downside that the popup 
                    // doesn't support transparency when its a child window - which 
                    // it is when in a browser. when we have unmanaged code rights 
                    // though we can use apis to bring the popup to the front ourselves 
                    // so we can use it
                    if (BrowserInteropHelper.IsBrowserHosted)
                        return DockManagerUtilities.HasUnmanagedCodeRights;
                }

                return false;
            }
        } 
        #endregion //ShowToolWindowsInPopup

        #region ToolWindows
		internal ReadOnlyObservableCollection<PaneToolWindow> ToolWindows
		{
			get { return this._readOnlyWindows; }
		} 
		#endregion // ToolWindows

		// AS 9/16/09 TFS22219
		#region UnpinnedPaneHolder
		/// <summary>
		/// Element used to act as a logical parent for the unpinned panes while we do not have an UnpinnedTabArea for the UnpinnedTabAreaInfo.
		/// </summary>
		internal LogicalContainer UnpinnedPaneHolder
		{
			get
			{
				if (_unpinnedPaneHolder == null)
				{
					_unpinnedPaneHolder = new LogicalContainer();
					_unpinnedPaneHolder.SetValue(XamDockManager.PaneLocationPropertyKey, DockManagerKnownBoxes.PaneLocationUnpinnedBox);
					this.AddLogicalChildInternal(_unpinnedPaneHolder);
				}

				return _unpinnedPaneHolder;
			}
		}
		#endregion //UnpinnedPaneHolder

		#endregion //Internal Properties

		#region Private Properties

		#region AllowToolWindowLoaded
		// AS 4/30/08
		// We had to add the IsVisible check here because if the dm is on a tabcontrol but is
		// not in the selected tab, it will still get its Loaded event fired when the form
		// is shown but we really don't want to show the floaters at that point. We will get
		// another loaded event (strangely without an unloaded between) when the tab containing
		// the dm is selected and at that point we want to show the floaters. The only distinguishing
		// property state seems to be the IsVisible state. Note, we're not listening to the 
		// IsVisibleChanged because that is documented as not being directly tied to the IsVisible
		// property.
		//
		/// <summary>
		/// Indicates where the <see cref="PaneToolWindow.IsWindowLoaded"/> may be set to true.
		/// </summary>
		private bool AllowToolWindowLoaded
		{
			// AS 5/19/08 Reuse Group/Split
			//get { return this.IsLoaded && this.IsVisible; }
			// AS 6/23/11 TFS73499
			// We may want to show the windows when the hosting window is hidden.
			//
			//get { return this.IsLoaded
			//    && this.IsVisible
			//    && this.IsLoadingLayout == false
			//    ; }
			get
			{
				if (this.IsLoadingLayout || !this.IsLoaded)
					return false;

				// we don't try to create the hwndsourcehelper until we're initialized.
				if (!this.IsInitialized)
					return false;

				if (_hwndSourceHelper == null)
					return this.IsVisible;

				// if we're not associated with a presentation source then we're not in the 
				// visual tree so we shouldn't show the floating windows
				if (_hwndSourceHelper.CurrentSource == null)
					return false;

				// AS 2/21/12 TFS99925
				// We shouldn't be evaluating the RootVisualVisibility if the FloatingWindowVisibility
				// is not SyncWithDockManagerWindow.
				//
				switch (this.FloatingWindowVisibility)
				{
					case FloatingWindowVisibility.Collapsed:
						return false;
					case FloatingWindowVisibility.Hidden:
					case FloatingWindowVisibility.Visible:
						return true;
				}

				// show it as long as they would be visible or hidden
				return _hwndSourceHelper.RootVisualVisibility != Visibility.Collapsed;
			}
		}
		#endregion //AllowToolWindowLoaded

		// AS 6/23/11 TFS73499
		#region IsHwndSourceRootVisualNonVisible
		private bool IsHwndSourceRootVisualNonVisible
		{
			get
			{
				return _hwndSourceHelper != null &&
					_hwndSourceHelper.CurrentSource != null &&
					_hwndSourceHelper.RootVisualVisibility != Visibility.Visible;
			}
		}
		#endregion //IsHwndSourceRootVisualNonVisible

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region AddDocument
		/// <summary>
		/// Adds a <see cref="ContentPane"/> to the associated <see cref="DocumentContentHost"/>
		/// </summary>
		/// <param name="header">The value for the <see cref="HeaderedContentControl.Header"/> property of the new <see cref="ContentPane"/></param>
		/// <param name="content">The value for the <see cref="ContentControl.Content"/> property of the new <see cref="ContentPane"/></param>
		/// <exception cref="InvalidOperationException">The Content property must be set to a DocumentContentHost.</exception>
		public ContentPane AddDocument(object header, object content)
		{
			if (false == this.HasDocumentContentHost)
				throw new InvalidOperationException(GetString("LE_NeedDocumentContentHost"));

			ContentPane cp = new ContentPane();
			cp.Header = header;
			cp.Content = content;

			IContentPaneContainer newContainer = this.DocumentContentHost.GetContainerForPane(cp, true);
			newContainer.InsertContentPane(0, cp);

			return cp;
		} 
		#endregion //AddDocument

		#region GetPanes
		/// <summary>
		/// Returns an enumerator that can be used to find all the <see cref="ContentPane"/> instances within the <see cref="XamDockManager"/>
		/// </summary>
		/// <param name="order">The order in which the panes should be returned.</param>
		/// <returns></returns>
		public IEnumerable<ContentPane> GetPanes(PaneNavigationOrder order)
		{
			return this.ActivePaneManager.GetPanes(PaneFilterFlags.All, order, false);
		} 
		#endregion //GetPanes

		#region ExecuteCommand

		/// <summary>
		/// Executes the specified RoutedCommand.
		/// </summary>
		/// <param name="command">The RoutedCommand to execute.</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="DockManagerCommands"/>
		public bool ExecuteCommand(RoutedCommand command)
		{
			return this.ExecuteCommandImpl(new ExecuteCommandInfo(command));
		}

		#endregion //ExecuteCommand

		#region LoadLayout
		/// <summary>
		/// Loads a layout that was saved with the <see cref="SaveLayout(Stream)"/> method.
		/// </summary>
		/// <param name="stream">The stream containing the saved layout.</param>
		public void LoadLayout(Stream stream)
		{
			LayoutManager.LoadLayout(this, stream);
		}

		/// <summary>
		/// Loads a layout saved with the <see cref="SaveLayout()"/> method.
		/// </summary>
		/// <param name="layout">The string containing the saved layout</param>
		public void LoadLayout(string layout)
		{
			LayoutManager.LoadLayout(this, layout);
		}
		#endregion //LoadLayout

		#region SaveLayout
		/// <summary>
		/// Saves information about the current arrangement of the panes to the specified stream.
		/// </summary>
		/// <exception cref="InvalidOperationException">The 'Name' property of a <see cref="ContentPane"/> whose <see cref="ContentPane.SaveInLayout"/> is true has not been set or is not unique with respect to the other content panes being saved.</exception>
		/// <param name="stream">The stream to which the layout should be saved.</param>
		public void SaveLayout(Stream stream)
		{
			LayoutManager.SaveLayout(this, stream);
		}

		/// <summary>
		/// Saves information about the current arrangement of the panes and returns that as a string.
		/// </summary>
		/// <exception cref="InvalidOperationException">The 'Name' property of a <see cref="ContentPane"/> whose <see cref="ContentPane.SaveInLayout"/> is true has not been set or is not unique with respect to the other content panes being saved.</exception>
		///	<returns>A string that contains the layout information.</returns>
		public string SaveLayout()
		{
			return LayoutManager.SaveLayout(this);
		}
		#endregion //SaveLayout

		#endregion //Public Methods

		#region Internal Methods

		#region AddLogicalChildInternal
		/// <summary>
		/// Helper method for adding a child to the dockmanager's logical tree
		/// </summary>
		/// <param name="logicalChild">The child to add to the logical tree</param>
		internal void AddLogicalChildInternal(DependencyObject logicalChild)
		{
			if (this._logicalChildren == null)
				this._logicalChildren = new List<DependencyObject>();

			this._logicalChildren.Add(logicalChild);
			this.AddLogicalChild(logicalChild);
		} 
		#endregion //AddLogicalChildInternal

		// AS 6/23/11 TFS73499
		#region AdjustAllowedDropLocations
		internal AllowedDropLocations AdjustAllowedDropLocations(AllowedDropLocations allowedLocations)
		{
			var floatingVisibility = this.FloatingWindowVisibility;

			switch (floatingVisibility)
			{
				case FloatingWindowVisibility.Collapsed:
				case FloatingWindowVisibility.Hidden:
					allowedLocations &= ~AllowedDropLocations.Floating;
					break;
			}

			if (this.IsHwndSourceRootVisualNonVisible)
			{
				// if the main window is not visible then don't show any docking indicators
				allowedLocations &= ~(AllowedDropLocations.Docked | AllowedDropLocations.Document);

				if (floatingVisibility == FloatingWindowVisibility.SyncWithDockManagerWindow)
					allowedLocations &= ~AllowedDropLocations.Floating;
			}

			return allowedLocations;
		}
		#endregion //AdjustAllowedDropLocations

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region BringToolWindowsToFront
        internal void BringToolWindowsToFront()
        {
            ToolWindow[] windows = ToolWindow.GetToolWindows(this);

            for (int i = windows.Length - 1; i >= 0; i--)
            {
                PaneToolWindow window = windows[i] as PaneToolWindow;

                if (null != window && window.IsVisible)
                {
                    window.BringToFront();
                }
            }
        }
        #endregion //BringToolWindowsToFront

		#region CanMoveToNewGroup
		/// <summary>
		/// Helper method to determine if a pane can be moved to a new vertical/horizontal group
		/// </summary>
		/// <param name="pane">The pane that would be moved</param>
		/// <param name="orientation">The orientation for the new group</param>
		/// <returns>True for document panes that are in a group with multiple visible panes</returns>
		internal static bool CanMoveToNewGroup(ContentPane pane, Orientation orientation)
		{
			if (null != pane && pane.IsDocument)
			{
				IContentPaneContainer container = pane.PlacementInfo.CurrentContainer;
				Debug.Assert(container is TabGroupPane);

				return null != container && container.GetVisiblePanes().Count > 1;
			}

			return false;
		}
		#endregion //CanMoveToNewGroup

		#region CanMoveToNextPreviousGroup
		/// <summary>
		/// Helper method to determine if there is a next/previous group to which the specified pane can be moved.
		/// </summary>
		/// <param name="pane">The pane to evaluate for a move</param>
		/// <param name="next">True to move to the next group; false to move to the previous</param>
		/// <returns>True if there is a previous group; otherwise false</returns>
		internal static bool CanMoveToNextPreviousGroup(ContentPane pane, bool next)
		{
			return null != GetNextPreviousGroup(pane, next);
		}
		#endregion //CanMoveToNextPreviousGroup

		#region CloseAllButThis
		/// <summary>
		/// Helper method to close all the panes in the group except the current
		/// </summary>
		/// <param name="pane">The pane whose siblings are to be closed</param>
		internal void CloseAllButThis(ContentPane pane)
		{
			
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

			if (pane.PaneLocation != PaneLocation.Document)
				return;

			IList<ContentPane> closablePanes = new List<ContentPane>();

			foreach (ContentPane child in DockManagerUtilities.GetAllPanes(this.DocumentContentHost, PaneFilterFlags.AllVisible))
			{
				if (child != pane && child.AllowClose)
					closablePanes.Add(child);
			}

			ClosePanes(closablePanes);
		}
		#endregion // CloseAllButThis

		#region ClosePane

		/// <summary>
		/// Helper method for closing all the panes within a group
		/// </summary>
		/// <param name="pane">Provides the context about what pane(s) should be closed</param>
		/// <param name="includeSiblingsIfAllowed">If true all sibling panes will be closed if the close behavior supports it</param>
		internal void ClosePane(ContentPane pane, bool includeSiblingsIfAllowed)
		{
			IList<ContentPane> panesToClose = GetPanesToProcess(pane, includeSiblingsIfAllowed, this.CloseBehavior);

			ClosePanes(panesToClose);
		}

		#endregion //ClosePane

		#region ClosePanes
		/// <summary>
		/// Closes one or more ContentPane instances
		/// </summary>
		/// <param name="panes">A list of panes to close</param>
		internal void ClosePanes(IList<ContentPane> panes)
		{
			for (int i = 0, count = panes.Count; i < count; i++)
			{
				ContentPane visiblePane = panes[i];

				if (visiblePane.AllowClose)
				{
					ClosePaneHelper(visiblePane, visiblePane.CloseAction, true);
				}
			}

			// after a short delay change the active pane
			this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new DockManagerUtilities.MethodInvoker(this.ActivePaneManager.VerifyActivePane));
		}
		#endregion //ClosePanes

		#region ClosePaneHelper
		internal void ClosePaneHelper(ContentPane pane, PaneCloseAction closeAction, bool raiseEvents)
		{
			this.ClosePaneHelper(pane, closeAction, raiseEvents, true);
		}

		// AS 2/2/10 TFS25608
		// Added overload that can be used to remove just the placeholders.
		//
		internal void ClosePaneHelper(ContentPane pane, PaneCloseAction closeAction, bool raiseEvents, bool removeFromCurrentContainer)
		{
			// AS 7/14/09 TFS18400
			// Keep track of whether panes are being closed so the TabGroupPane
			// can know to skip verifying the selected pane.
			//
			bool wasClosing = _isClosingPanes;
			_isClosingPanes = true;

			try
			{
				this.ClosePaneHelperImpl(pane, closeAction, raiseEvents, removeFromCurrentContainer);
			}
			finally
			{
				this._isClosingPanes = wasClosing;
			}
		}

		private void ClosePaneHelperImpl(ContentPane pane, PaneCloseAction closeAction, bool raiseEvents, bool removeFromCurrentContainer )
		{
			if (raiseEvents)
			{
				PaneClosingEventArgs beforeArgs = new PaneClosingEventArgs();
				pane.RaiseClosing(beforeArgs);

				if (beforeArgs.Cancel)
					return;
			}

			if (closeAction == PaneCloseAction.RemovePane)
			{
				ContentPane.PanePlacementInfo placementInfo = pane.PlacementInfo;

				// remove the placeholders
				if (placementInfo.DockedEdgePlaceholder != null)
					DestroyPaneElement(placementInfo.DockedEdgePlaceholder.Container, pane);

				if (placementInfo.FloatingDockablePlaceholder != null)
					DestroyPaneElement(placementInfo.FloatingDockablePlaceholder.Container, pane);

				if (placementInfo.FloatingOnlyPlaceholder != null)
					DestroyPaneElement(placementInfo.FloatingOnlyPlaceholder.Container, pane);

				// AS 2/2/10 TFS25608
				// We do not want to remove the pane from its current container unless it 
				// was explicitly removed (or implicitly as part of the layout load).
				//
				if (removeFromCurrentContainer)
				{
					// remove the pane itself
					DestroyPaneElement(placementInfo.CurrentContainer, pane);
				}
			}
			else
			{
				pane.Visibility = Visibility.Collapsed;
			}

			if (raiseEvents)
				pane.RaiseClosed(new PaneClosedEventArgs());
		}
		#endregion //ClosePaneHelper

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region DirtyHasHwndHosts
        internal void DirtyHasHwndHosts()
        {
            _hasHwndHost = null;
        }
        #endregion //DirtyHasHwndHosts

        #region EnsureIsFocusedElement
        // AS 2/9/09 TFS13375
        // This routine was used within the OnGotKeyboardFocus handler. However that is not called 
        // when an HwndHost within a pane is getting focus so I refactored this part of it into a 
        // helper routine that we can use from the ActivePaneManager when it 
        /// <summary>
        /// Helper method to set the FocusedElement of the DM's FocusScope to itself should the new
        /// focused element exist within a different focus scope.
        /// </summary>
        /// <param name="newNestedFocusedElement">The element getting the keyboard focus</param>
        internal void EnsureIsFocusedElement(DependencyObject newNestedFocusedElement)
        {
            if (null != newNestedFocusedElement)
            {
                // AS 3/30/09 TFS16355 - WinForms Interop
                //Debug.Assert(this == newNestedFocusedElement || Utilities.IsDescendantOf(this, newNestedFocusedElement));
                Debug.Assert(this == newNestedFocusedElement || Utilities.IsDescendantOf(this, newNestedFocusedElement, true));

                DependencyObject newFocusScope = FocusManager.GetFocusScope(newNestedFocusedElement);
                DependencyObject parent = Utilities.GetParent(this);
                DependencyObject focusScope = null != parent ? FocusManager.GetFocusScope(parent) : null;
                DependencyObject focusScopeFocused = null != focusScope ? FocusManager.GetFocusedElement(focusScope) as DependencyObject : null;

				// AS 3/26/10 TFS30153
				// Optimization. If we're already the focused element of our focus scope then there is nothing to do.
				//
				if (focusScopeFocused == this)
					return;

                // update the focused element of our focus scope to be within us
                if (focusScope != null					// we're in a focus scope
                    && newFocusScope != focusScope		// the element being focused is in a different one
                    && (focusScopeFocused == null		// the focused element of our focus scope isn't a child of ours
                        ||
						// AS 7/14/09
						// See the notes in OnGotKeyboardFocus. Essentially since the drag helper is in the tree, we 
						// need to ignore that element when considering whether to shift the focused element.
						//
						focusScopeFocused == _dragHelper // the focused element is our drag helper
						||
                        // AS 3/30/09 TFS16355 - WinForms Interop
                        //false == Utilities.IsDescendantOf(this, focusScopeFocused))
                        false == Utilities.IsDescendantOf(this, focusScopeFocused, true))
                    )
                {
					
#region Infragistics Source Cleanup (Region)











































#endregion // Infragistics Source Cleanup (Region)

					bool wasSettingFocusedElement = _isSettingFocusedElement;
					_isSettingFocusedElement = true;

					try
					{
						DockManagerUtilities.SetFocusedElement(focusScope, this);
					}
					finally
					{
						_isSettingFocusedElement = wasSettingFocusedElement;
					}
				}
            }
        }
        #endregion //EnsureIsFocusedElement

        // AS 10/15/08 TFS6271
        #region ForceFocus
        internal void ForceFocus()
        {
            this.ForceFocus(this);
        }

        // AS 2/26/09 TFS14668
        // Added an overload so we can use this with any input element
        // and have the xdm skip its focus redirection.
        //
        internal void ForceFocus(IInputElement element)
        {
            Debug.Assert(null != element);

            bool wasForcing = this._forcingFocus;
            this._forcingFocus = true;

            try
            {
                element.Focus();
            }
            finally
            {
                this._forcingFocus = wasForcing;
            }
        }
        #endregion //ForceFocus

		#region GetDockedSide
		/// <summary>
		/// Returns the side to which the pane would be or is docked.
		/// </summary>
		/// <param name="pane">The pane to evaluate</param>
		/// <returns>The side to which the pane should be docked/unpinned</returns>
		internal Dock GetDockedSide(ContentPane pane)
		{
			PaneLocation location = PaneLocation.DockedLeft;

			if (pane.PlacementInfo.DockedEdgePlaceholder != null)
				location = XamDockManager.GetPaneLocation(pane.PlacementInfo.DockedEdgePlaceholder);
			else if (location == PaneLocation.Unpinned)
			{
				// if there is no placeholder and its unpinned then something must be wrong
				Debug.Fail("The pane is unpinned but there is no placeholder!");
				UnpinnedTabAreaInfo info = pane.PlacementInfo.CurrentContainer as UnpinnedTabAreaInfo;

				if (null != info)
					return (Dock)Array.IndexOf(this._tabAreaInfos, info);
				else
					Debug.Fail("Its not even associated with a 'UnpinnedTabAreaInfo'");
			}
			else if (DockManagerUtilities.IsDocked(location))
				location = XamDockManager.GetPaneLocation(pane);
			else
				Debug.Fail("Unable to determine docked side!");

			return DockManagerUtilities.GetDockedSide(location);
		} 
		#endregion //GetDockedSide

		#region GetNextPreviousGroup
		internal static IContentPaneContainer GetNextPreviousGroup(ContentPane pane, bool next)
		{
			if (null != pane && pane.IsDocument)
			{
				XamDockManager dockManager = XamDockManager.GetDockManager(pane);

				if (null != dockManager && dockManager.HasDocumentContentHost)
				{
					DocumentContentHost rootContainer = dockManager.DocumentContentHost;
					ContentPane nextPane = pane;

					while (null != (nextPane = DockManagerUtilities.GetNextPreviousPane(nextPane, false, next, rootContainer)))
					{
						// if there is a group before/after it
						if (nextPane.PlacementInfo.CurrentContainer != pane.PlacementInfo.CurrentContainer)
							return nextPane.PlacementInfo.CurrentContainer;
					}
				}
			}

			return null;
		}
		#endregion //GetNextPreviousGroup

		#region GetPaneForCommand
		internal static ContentPane GetPaneForCommand(object parameter, object originalSource, ContentPane defaultPane)
		{
			// use the following info to locate the content pane on which to act:

			// 1) use the parameter
			ContentPane pane = parameter as ContentPane;

			// 2) use the source (if its a contentpane)
			if (null == pane)
				pane = originalSource as ContentPane;

			if (null == pane)
			{
				DependencyObject originalSourceObject = originalSource as DependencyObject;

				// 3) use an ancestor of the source (if its within a content pane)
				// AS 9/10/09 TFS19267
				// I found this while testing this issue. The original source could be an XDM in which 
				// case we could get an ancestor pane if the XDM is nested because GetAncestorFromType 
				// doesn't check the object passed in and starts with the ancestor.
				//
				//if (null != originalSourceObject)
				if (null != originalSourceObject && originalSourceObject is XamDockManager == false)
					pane = Utilities.GetAncestorFromType(originalSourceObject, typeof(ContentPane), true, null, typeof(XamDockManager)) as ContentPane;
			}

			// 4) use the active pane
			if (null == pane)
				pane = defaultPane;

			return pane;
		}
		#endregion //GetPaneForCommand

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

		#region GetUnpinnedTabArea
		internal UnpinnedTabAreaInfo GetUnpinnedTabAreaInfo(Dock side)
		{
			return this._tabAreaInfos[(int)side];
		}

		internal UnpinnedTabArea GetUnpinnedTabArea(Dock side)
		{
			return this.GetUnpinnedTabAreaInfo(side).TabArea;
		}
		#endregion //GetUnpinnedTabArea

		#region HideFlyout
		/// <summary>
		/// Helper method to hide the flyout if a particular pane is showing.
		/// </summary>
		/// <param name="pane">Null to force the flyout to be hidden regardless of what it is showing or a pane that should be hidden if it is shown in the flyout.</param>
		/// <param name="checkMouseOver">True if the flyout can be allowed to remain over if the mouse is over it.</param>
		/// <param name="allowAnimation">True if the flyout close can be animated; false to immediately close it.</param>
		/// <param name="ignoreMouseWhileClosing">True if the mouse should not be evaluated as a means to keep the flyout opened</param>
		/// <param name="allowAutoHideDelay">True if there should be a slight delay before starting the hide operation</param>
		internal void HideFlyout(ContentPane pane, bool checkMouseOver, bool allowAnimation, bool ignoreMouseWhileClosing, bool allowAutoHideDelay)
		{
			if (this._dockPanel != null && this._dockPanel.HasFlyoutPanel)
				this._dockPanel.FlyoutPanel.HideFlyout(pane, checkMouseOver, allowAnimation, ignoreMouseWhileClosing, allowAutoHideDelay);
		} 
		#endregion //HideFlyout

		// AS 6/23/11 TFS73499
		#region IsPaneLocationVisible
		internal bool IsPaneLocationVisible(PaneLocation paneLocation)
		{
			switch (paneLocation)
			{
				case PaneLocation.Floating:
				case PaneLocation.FloatingOnly:
					{
						switch (this.FloatingWindowVisibility)
						{
							case FloatingWindowVisibility.Hidden:
							case FloatingWindowVisibility.Collapsed:
								return false;
							case FloatingWindowVisibility.SyncWithDockManagerWindow:
								if (this.IsHwndSourceRootVisualNonVisible)
								{
									return false;
								}
								break;
						}
						break;
					}
				default:
					{
						if (this.IsHwndSourceRootVisualNonVisible)
							return false;

						break;
					}
			}

			return true;
		}
		#endregion //IsPaneLocationVisible

		// AS 6/23/11 TFS73499
		#region MeetsVisibleCriteria
		internal bool MeetsVisibleCriteria(FrameworkElement element)
		{
			if (element == null)
				return false;

			var paneLocation = GetPaneLocation(element);

			// this block is really here to make sure that when navigating or building the list
			// for the pane navigator that we not include panes that are in the root dm if the 
			// root visual is not visible and not return floating panes if the 
			// floatingwindowvisibility is collapsed/hidden
			if (DockManagerUtilities.IsFloating(paneLocation))
			{
				switch (this.FloatingWindowVisibility)
				{
					case FloatingWindowVisibility.Collapsed:
					case FloatingWindowVisibility.Hidden:
						return false;
					case FloatingWindowVisibility.Visible:
						return true;
					case FloatingWindowVisibility.SyncWithDockManagerWindow:
						if (this.IsHwndSourceRootVisualNonVisible)
						{
							return false;
						}
						break;
				}
			}
			else
			{
				// if the element is within the docked edge or document and the visual containing the 
				// xamdockmanager is not visible then we shouldn't include the panes within it
				if (this.IsHwndSourceRootVisualNonVisible)
				{
					return false;
				}
			}

			return true;
		}
		#endregion //MeetsVisibleCriteria

		// AS 2/2/10 TFS6504
		// Moved this block from within the ProcessPinnedState to a helper routine that could 
		// be used by the ActivePaneManager when we detect that the docked container for an unpinned 
		// pane was removed. In this way if you remove/clear a splitpane that contains the docked 
		// state of a contentpane from the dockmanager, the content pane will be removed too.
		//
		#region MovePaneToPinnedContainer
		internal void MovePaneToPinnedContainer(ContentPane pane, bool isRemovingPane)
		{
			// the pane must be in the unpinned area but should be pinned
			// so we need to reposition back into its old dock location
			ContentPanePlaceholder dockedPlaceholder = pane.PlacementInfo.DockedEdgePlaceholder;

			Debug.Assert(dockedPlaceholder != null, "The pane is currently unpinned and being pinned but there is no docked placeholder to determine its position!");
			Debug.Assert(dockedPlaceholder == null || isRemovingPane || XamDockManager.GetDockManager(dockedPlaceholder) == this, "The docked placeholder is no longer associated with the dockmanager!");
			Debug.Assert(dockedPlaceholder == null || isRemovingPane || DockManagerUtilities.IsDocked(dockedPlaceholder), "The docked placedholder is no longer docked!");

			// we have a placeholder to know where to reposition the pane
			if (null != dockedPlaceholder &&
				(isRemovingPane ||
					(XamDockManager.GetDockManager(dockedPlaceholder) == this &&
					DockManagerUtilities.IsDocked(dockedPlaceholder) == true)
				))
			{
				IContentPaneContainer dockedContainer = dockedPlaceholder.Container;
				IContentPaneContainer unpinnedContainer = pane.PlacementInfo.CurrentContainer;

				// AS 2/11/10 TFS6504
				// If the docked placeholder of a contentpane is removed then remove the content pane 
				// from the dockmanager.
				//
				if (null == dockedContainer && isRemovingPane)
				{
					Debug.Assert(pane.IsPinned == false);

					// remove the docked placeholder and remove the pane from the dockmanager
					pane.PlacementInfo.RemovePlaceholder(dockedPlaceholder);

					this.ClosePaneHelper(pane, PaneCloseAction.RemovePane, false);
					return;
				}

				Debug.Assert(null != dockedContainer, "The docked placeholder does not have a reference to its container!");
				Debug.Assert(null == dockedContainer || isRemovingPane || DockManagerUtilities.IsDocked(dockedContainer.PaneLocation), "The container for the docked placeholder is not docked!");

				Debug.Assert(unpinnedContainer != null, "The unpinned pane does not have a current container!");
				Debug.Assert(unpinnedContainer == null || unpinnedContainer.PaneLocation == PaneLocation.Unpinned, "The current container is not unpinned!");

				if (null != dockedContainer && null != unpinnedContainer)
				{
					// AS 10/15/08 TFS8068
					using (DockManagerUtilities.CreateMoveReplacement(pane))
					{
						// permanently remove the pane from the unpinned area
						unpinnedContainer.RemoveContentPane(pane, false);

						// and place it in the docked location
						dockedContainer.InsertContentPane(null, pane);
					}

					if (pane.IsActivePane)
						pane.BringIntoView();
				}
			}
		}
		#endregion //MovePaneToPinnedContainer

		#region MoveToNewGroup
		/// <summary>
		/// Moves a single ContentPane within the DocumentContentHost from one group to a new adjacent group
		/// </summary>
		/// <param name="pane">The pane to be moved</param>
		/// <param name="newGroupOrientation">The orientation for the new group that will contain the pane</param>
		/// <returns>True if the pane was moved to a new group</returns>
		internal static bool MoveToNewGroup(ContentPane pane, Orientation newGroupOrientation)
		{
			Debug.Assert(null != pane && pane.IsDocument);

			if (null == pane || false == pane.IsDocument)
				return false;

			IContentPaneContainer currentContainer = pane.PlacementInfo.CurrentContainer;
			Debug.Assert(currentContainer is TabGroupPane);

			IPaneContainer container = DockManagerUtilities.GetParentPane(currentContainer);

			Debug.Assert(container is SplitPane);
			SplitPane splitPane = container as SplitPane;

			if (null != splitPane)
			{
				//using (FocusHelper fh = new FocusHelper(XamDockManager.GetDockManager(pane)))
				//{

                // AS 10/15/08 TFS8068
                using (GroupTempValueReplacement replacements = new GroupTempValueReplacement())
                {
					// AS 9/11/09 TFS21330
					//// AS 10/15/08 TFS8068
					//replacements.Add(DockManagerUtilities.CreateMoveReplacement(pane));
					DockManagerUtilities.AddMoveReplacement(replacements, pane);

                    // remove the pane from the tab group
                    currentContainer.RemoveContentPane(pane, false);
                    FrameworkElement containerElement = (FrameworkElement)currentContainer;

                    // find out where the tab group is in the split pane
                    int containerIndex = splitPane.Panes.IndexOf(containerElement);
					TabGroupPane newTabGroup = DockManagerUtilities.CreateTabGroup(XamDockManager.GetDockManager(splitPane));
                    newTabGroup.Items.Add(pane);

                    // move the pane to a new tab group in the current split pane if possible (i.e. if the orientations match)
                    if (splitPane.SplitterOrientation == newGroupOrientation)
                    {
                        splitPane.Panes.Insert(containerIndex + 1, newTabGroup);
                    }
                    else
                    {
                        // create a new split to contain the tab group that contained the pane
                        // and the new tab group containing the moved pane
						SplitPane newSplit = DockManagerUtilities.CreateSplitPane(XamDockManager.GetDockManager(splitPane));
                        newSplit.SplitterOrientation = newGroupOrientation;

						// AS 9/11/09 TFS21330
						//// AS 10/15/08 TFS8068
						//replacements.Add(DockManagerUtilities.CreateMoveReplacement(containerElement));
						DockManagerUtilities.AddMoveReplacement(replacements, containerElement);

                        // remove the tab group from the split
                        splitPane.Panes.Remove(containerElement);

                        newSplit.Panes.Add(containerElement);
                        newSplit.Panes.Add(newTabGroup);
                        splitPane.Panes.Insert(containerIndex, newSplit);

                    }

                    // AS 5/21/08
                    // remove the old container if its no longer needed
                    DockManagerUtilities.RemoveContainerIfNeeded(currentContainer);

                    return true;
                }
				//}
			}

			return false;
		}
		#endregion // MoveToNewGroup

		#region MoveToNextGroup
		/// <summary>
		/// Moves a single ContentPane in the DocumentContentHost from one tab group to an adjacent group - either before or after the current containing group.
		/// </summary>
		/// <param name="pane">The pane to be moved</param>
		/// <param name="next">True to move to the next group; false to move to the previous group</param>
		/// <returns>True if the pane was moved; otherwise false</returns>
		internal bool MoveToNextGroup(ContentPane pane, bool next)
		{
			Debug.Assert(null != pane);

			if (pane == null)
				return false;

			IContentPaneContainer newContainer = GetNextPreviousGroup(pane, next);
			IContentPaneContainer currentContainer = pane.PlacementInfo.CurrentContainer;

			Debug.Assert(newContainer is TabGroupPane);
			Debug.Assert(pane.IsDocument);

			if (pane.IsDocument && newContainer != null && currentContainer != null && newContainer != currentContainer)
			{
				//using (FocusHelper fh = new FocusHelper(this))
				//{

                // AS 10/15/08 TFS8068
                using (DockManagerUtilities.CreateMoveReplacement(pane))
                {
                    currentContainer.RemoveContentPane(pane, false);
                    newContainer.InsertContentPane(0, pane);
                    pane.BringIntoView();

                    // remove the container if its now empty
                    DockManagerUtilities.RemoveContainerIfNeeded(currentContainer);

                    return true;
                }
				//}
			}

			return false;
		}
		#endregion // MoveToNextGroup

		// AS 5/17/08 Reuse Group/Split
		#region OnAfterLoadLayout
		internal void OnAfterLoadLayout()
		{
			// since we can reuse groups (and thereby the panetoolwindow that could 
			// contain that group), we should have the windows verify their state
			foreach (PaneToolWindow toolWindow in this._toolWindows)
			{
				toolWindow.RefreshToolWindowState();
			}

			// AS 6/23/11 TFS73499
			//if (this.AllowToolWindowLoaded)
			//	this.SetIsWindowLoaded(true);
			this.VerifyFloatingWindowVisibility();
		}
		#endregion //OnAfterLoadLayout

		// AS 5/17/08 Reuse Group/Split
		// This isn't really specific to this change but the dm should know when the layout is 
		// about to be loaded so it can perform any needed preparations. In theory we could
		// use the setter of the IsLoadingLayout but there is some processing we do after
		// that before we actually manipulate the dockmanager so I want do this later.
		//
		#region OnBeforeLoadLayout
		internal void OnBeforeLoadLayout()
		{
			
		}
		#endregion //OnBeforeLoadLayout

		// AS 10/14/10 TFS57352
		#region OnBeforeLoadLayoutProcess
		internal void OnBeforeLoadLayoutProcess()
		{
			this.ActivePaneManager.ProcessPendingOperations();
		}
		#endregion //OnBeforeLoadLayoutProcess

		// AS 2/3/10 TFS27137
		#region OnBeforeSaveLayout
		internal void OnBeforeSaveLayout()
		{
			// make sure we've gotten a process pinned state for each pane
			foreach (ContentPane cp in this.GetPanes(PaneNavigationOrder.VisibleOrder))
			{
				this.ProcessPinnedState(cp, cp.PaneLocation, false);
			}

			this.ProcessDeferredUnpinnedPanes();
		}
		#endregion //OnBeforeSaveLayout

		#region ProcessKeyDown
		/// <summary>
		/// Helper method for processing a keydown for the dockmanager or one of its tool windows.
		/// </summary>
		/// <param name="e">Provides information about the keyboard event</param>
		internal void ProcessKeyDown(KeyEventArgs e)
		{
			if (e.Handled == true)
				return;

			if (this._activePaneManager.IsInNavigationMode == false 
				&& e.IsRepeat == false)
			{
				Key key = e.Key == Key.System ? e.SystemKey : e.Key;

				switch (key)
				{
					case Key.LeftAlt:
					case Key.RightAlt:
					case Key.LeftCtrl:
					case Key.RightCtrl:
						this.ActivePaneManager.IsInNavigationMode = true;
						break;
				}
			}

			PaneNavigator.ProcessKeyDown(this, e);

			// Pass this key along to our commands class which will check to see if a command
			// needs to be executed.  If so, the commands class will execute the command and
			// return true.
			if (e.Handled == false 
				&& this.Commands != null 
				&& this.Commands.ProcessKeyboardInput(e, this) == true)
				e.Handled = true;
		}
		#endregion //ProcessKeyDown

		#region ProcessKeyUp
		/// <summary>
		/// Helper method for processing a keydown for the dockmanager or one of its tool windows.
		/// </summary>
		/// <param name="e">Provides information about the keyboard event</param>
		internal void ProcessKeyUp(KeyEventArgs e)
		{
			if (e.Handled == true)
				return;

			if (this._activePaneManager.IsInNavigationMode == true) 
			{
				Key key = e.Key == Key.System ? e.SystemKey : e.Key;

				switch (key)
				{
					case Key.LeftAlt:
					case Key.RightAlt:
					case Key.LeftCtrl:
					case Key.RightCtrl:
						this.ActivePaneManager.IsInNavigationMode = false;
						break;
				}
			}
		}
		#endregion //ProcessKeyUp

		#region ProcessPaneHeaderDoubleClick
		internal bool ProcessPaneHeaderDoubleClick(ContentPane pane)
		{
			// AS 6/24/11 FloatingWindowCaptionSource
			if (pane.IsProvidingFloatingWindowCaption)
			{
				var tw = ToolWindow.GetToolWindow(pane) as PaneToolWindow;

				if (null != tw)
				{
					tw.OnDoubleClickCaption(new Point(), Mouse.PrimaryDevice);
					return true;
				}
			}

			TabGroupPane tabGroup = DockManagerUtilities.GetParentPane(pane) as TabGroupPane;

			// if the pane is not in a group then just toggle this pane
			// AS 5/27/08
			// We also want to perform a normal toggle if the tab group only has 1 visible pane.
			//
			//if (null == tabGroup)
			if (null == tabGroup || ((IContentPaneContainer)tabGroup).GetVisiblePanes().Count < 2)
				return pane.ExecuteCommand(ContentPaneCommands.ToggleDockedState);
			else
			{
				// get all the visible panes
				List<ContentPane> groupPanes = DockManagerUtilities.GetAllPanes(tabGroup, PaneFilterFlags.AllVisible);

                // AS 2/19/09 TFS10930
                if (groupPanes.Count == 1)
                    return groupPanes[0].ExecuteCommand(ContentPaneCommands.ToggleDockedState);

				switch (pane.PaneLocation)
				{
					case PaneLocation.DockedBottom:
					case PaneLocation.DockedLeft:
					case PaneLocation.DockedRight:
					case PaneLocation.DockedTop:
						{
							#region Docked to Floating As A Group

							// we're going to handle this as a single atomic operation - either all
							// panes can be floated together or none of them can be floated.
							#region Setup

							// first make sure all support toggling
							foreach (ContentPane child in groupPanes)
							{
								// if one cannot then cancel the operation
								if (child.CanToggleDockedState == false)
									return false;
							}

							// before we fire the executing, let's make sure we have enough info to
							// process the request

							// we need the selected pane since we will be using its last floating size/location
							ContentPane selectedPane = tabGroup.SelectedItem as ContentPane ?? DockManagerUtilities.GetFirstLastPane(tabGroup, true);

							if (selectedPane == null)
								return false;

							// AS 5/2/08 BR32056
							// We're not going to require that the pane was previously floating.
							//
							// // get its floating or floating only placeholder
							//ContentPanePlaceholder floatingPlaceholder = selectedPane.PlacementInfo.FloatingDockablePlaceholder ?? selectedPane.PlacementInfo.FloatingOnlyPlaceholder;
							//
							//if (null == floatingPlaceholder)
							//	return false;
							//
							//Debug.Assert(selectedPane.LastFloatingWindowRect.IsEmpty == false);

							// next raise the command executing event for these
							foreach (ContentPane child in groupPanes)
							{
								ExecutingCommandEventArgs beforeArgs = new ExecutingCommandEventArgs(ContentPaneCommands.ToggleDockedState);
								child.RaiseExecutingCommand(beforeArgs);

								// if any cancel then cancel the operation
								if (beforeArgs.Cancel)
									return false;
							}
							#endregion //Setup

							// store the active pane to ensure it has been activated after the move
							ContentPane activePane = this.ActivePane;

							IList<IContentPaneContainer> oldContainers = DockManagerUtilities.GetPaneContainers(groupPanes); // AS 3/17/11 TFS67321

							// create the new floating split pane
							SplitPane split = DockManagerUtilities.CreateSplitPane(this);

							// AS 4/28/11 TFS73532
							// Refactored to allow top-down element creation/move.
							//
							#region Refactored
							//// AS 10/16/08 TFS8068
							////TabGroupPane newTabGroup = DockManagerUtilities.Clone(tabGroup, PaneLocation.Floating);
							//TabGroupPane newTabGroup;
							//using (DockManagerUtilities.Clone(tabGroup, PaneLocation.Floating, out newTabGroup))
							//{
							//    split.Panes.Add(newTabGroup);
							//
							//    // initialize its state and add it to the dockmanager - this will also
							//    // show the floating window
							//    XamDockManager.SetInitialLocation(split, InitialPaneLocation.DockableFloating);

							//    if (selectedPane.LastFloatingWindowRect.IsEmpty == false)
							//    {
							//        XamDockManager.SetFloatingLocation(split, selectedPane.LastFloatingWindowRect.Location);
							//        XamDockManager.SetFloatingSize(split, selectedPane.LastFloatingWindowRect.Size);
							//    }
							//    else
							//    {
							//        // AS 5/2/08 BR32056
							//        // If we didn't have a floating rect then use last size or current size
							//        //
							//        XamDockManager.SetFloatingSize(split, selectedPane.GetSizeForFloating());
							//    }
							//
							//    this.Panes.Add(split);
							//}
							#endregion //Refactored

							using (MovePaneHelper moveHelper = new MovePaneHelper(this, split))
							{
								// initialize its state and add it to the dockmanager - this will also show the floating window
								XamDockManager.SetInitialLocation(split, InitialPaneLocation.DockableFloating);

								if (selectedPane.LastFloatingWindowRect.IsEmpty == false)
								{
									XamDockManager.SetFloatingLocation(split, selectedPane.LastFloatingWindowRect.Location);
									XamDockManager.SetFloatingSize(split, selectedPane.LastFloatingWindowRect.Size);
								}
								else
								{
									// AS 5/2/08 BR32056
									// If we didn't have a floating rect then use last size or current size
									//
									XamDockManager.SetFloatingSize(split, selectedPane.GetSizeForFloating());
								}

								this.Panes.Add(split);

								TabGroupPane newTabGroup = DockManagerUtilities.CreateTabGroup(this);
								split.Panes.Add(newTabGroup);

								DockManagerUtilities.Clone(tabGroup, PaneLocation.Floating, newTabGroup, moveHelper);
							}

							// some of these panes may have had floating placeholders before so fix that up now
							DockManagerUtilities.FixPlaceholders(groupPanes);

							if (null != activePane)
								activePane.ActivateInternal(true);

							DockManagerUtilities.RemoveContainersIfNeeded(oldContainers); // AS 3/17/11 TFS67321

							// lastly raise the after event
							foreach (ContentPane child in groupPanes)
							{
								ExecutedCommandEventArgs args = new ExecutedCommandEventArgs(ContentPaneCommands.ToggleDockedState);
								child.RaiseExecutedCommand(args);
							}

							return true;

							#endregion //Docked to Floating As A Group
						}
					case PaneLocation.Floating:
						// just position all panes back in their last docked states
						return this.ToggleDockedState(groupPanes, true );
					default:
					case PaneLocation.Document:
					case PaneLocation.FloatingOnly:
					case PaneLocation.Unknown:
					case PaneLocation.Unpinned:
						return false;
				}
			}
		} 
		#endregion //ProcessPaneHeaderDoubleClick

		#region ProcessPinnedState
		internal void ProcessPinnedState(ContentPane pane, PaneLocation location, bool allowShowFlyout)
		{
			// get the current container for the pane
			IContentPaneContainer container = pane.PlacementInfo.CurrentContainer;

			Debug.Assert(pane.PaneLocation == PaneLocation.Unknown || VisualTreeHelper.GetParent(pane) == null || container != null, "No current container is available!");
			Debug.Assert(container == null || container.PaneLocation == location, "The current container is in a different location!");

			if (null == container)
				return;

			switch (location)
			{
				case PaneLocation.Unpinned:
					{
						#region Unpin pane

						// the pane is in the unpinned area and is unpinned then there is nothing to do
						if (pane.IsPinned == false)
						{
							Debug.Assert(pane.PlacementInfo.DockedEdgePlaceholder == null ||
								pane.PlacementInfo.CurrentContainer == null ||
								(Dock)Array.IndexOf(this._tabAreaInfos, pane.PlacementInfo.CurrentContainer) == DockManagerUtilities.GetDockedSide(XamDockManager.GetPaneLocation(pane.PlacementInfo.DockedEdgePlaceholder)));
							return;
						}

						// AS 2/2/10 TFS6504
						#region Moved
						//// the pane must be in the unpinned area but should be pinned
						//// so we need to reposition back into its old dock location
						//ContentPanePlaceholder dockedPlaceholder = pane.PlacementInfo.DockedEdgePlaceholder;
						//
						//Debug.Assert(dockedPlaceholder != null, "The pane is currently unpinned and being pinned but there is no docked placeholder to determine its position!");
						//Debug.Assert(dockedPlaceholder == null || XamDockManager.GetDockManager(dockedPlaceholder) == this, "The docked placeholder is no longer associated with the dockmanager!");
						//Debug.Assert(dockedPlaceholder == null || DockManagerUtilities.IsDocked(dockedPlaceholder), "The docked placedholder is no longer docked!");
						//
						//// we have a placeholder to know where to reposition the pane
						//if (null != dockedPlaceholder &&
						//    XamDockManager.GetDockManager(dockedPlaceholder) == this &&
						//    DockManagerUtilities.IsDocked(dockedPlaceholder) == true)
						//{
						//    IContentPaneContainer dockedContainer = dockedPlaceholder.Container;
						//
						//    Debug.Assert(null != dockedContainer, "The docked placeholder does not have a reference to its container!");
						//    Debug.Assert(null == dockedContainer || DockManagerUtilities.IsDocked(dockedContainer.PaneLocation), "The container for the docked placeholder is not docked!");
						//
						//    IContentPaneContainer unpinnedContainer = pane.PlacementInfo.CurrentContainer;
						//    Debug.Assert(unpinnedContainer != null, "The unpinned pane does not have a current container!");
						//    Debug.Assert(unpinnedContainer == null || unpinnedContainer.PaneLocation == PaneLocation.Unpinned, "The current container is not unpinned!");
						//
						//    if (null != dockedContainer && null != unpinnedContainer)
						//    {
						//        // AS 10/15/08 TFS8068
						//        using (DockManagerUtilities.CreateMoveReplacement(pane))
						//        {
						//            // permanently remove the pane from the unpinned area
						//            unpinnedContainer.RemoveContentPane(pane, false);
						//
						//            // and place it in the docked location
						//            dockedContainer.InsertContentPane(null, pane);
						//        }
						//
						//        if (pane.IsActivePane)
						//            pane.BringIntoView();
						//    }
						//} 
						#endregion //Moved 
						this.MovePaneToPinnedContainer(pane, false);
						break;
						#endregion //Unpin pane
					}
				case PaneLocation.DockedBottom:
				case PaneLocation.DockedLeft:
				case PaneLocation.DockedRight:
				case PaneLocation.DockedTop:
					{
						// if the pane is docked and its docked there is nothing to do
						if (pane.IsPinned == true)
							return;

						// the pane is in the docked area but should be unpinned

						// we want to initialize the flyout extent based on the extent of the 
						// split pane that contains it plus the splitter extent
						// AS 7/7/08 BR34615
						// It is slightly inconsistent that when the pane is pinned it will be 
						// sized based on its contents if the containing root split pane has not 
						// been resized so I have changed the unpinned handling such that if a 
						// contentpane is unpinned and its containing root split pane has not 
						// been given an explicit extent, that we will not give the flyout an 
						// explicit extent.
						//
						//if (container.ContainerElement != null && container.ContainerElement.IsLoaded)
						//{
						//	bool isVert = location == PaneLocation.DockedLeft || location == PaneLocation.DockedRight;
						// AS 7/16/09 TFS18452
						// We used to skip the initialization of the flyout extent if the container wasn't loaded. However, 
						// you would then get different behavior with initially setting the IsPinned to false in xaml vs 
						// waiting for the window to load and then unpinning (without changing the extent of the split).
						//
						//bool initializeFlyoutExtent = container.ContainerElement != null && container.ContainerElement.IsLoaded;
						// AS 3/25/10 TFS29399
						// While the layout is loading, we do not want to calculate the unpinned extent.
						//
						//bool initializeFlyoutExtent = true;
						bool initializeFlyoutExtent = !this.IsLoadingLayout;

						bool isVert = location == PaneLocation.DockedLeft || location == PaneLocation.DockedRight;

						if (initializeFlyoutExtent)
						{
							SplitPane rootSplit = DockManagerUtilities.GetRootSplitPane(pane);
							Debug.Assert(null != rootSplit);
							// AS 7/16/09 TFS18452
							// If there was no root split we should assume there is no extent.
							//
							//double extent = rootSplit == null ? double.PositiveInfinity : (isVert ? rootSplit.Width : rootSplit.Height);
							double extent = rootSplit == null ? double.NaN : (isVert ? rootSplit.Width : rootSplit.Height);

							if (double.IsNaN(extent))
								initializeFlyoutExtent = false;
						}

						// AS 7/16/09 TFS18452
						// If we had an explicit width and we cannot get the extent yet then we have to wait and try again later.
						// Instead of checking the loaded (which will still be false when we call this from layoutupdated), we 
						// will rely on whether the measure of the pane is valid. I specifically didn't check that of the container 
						// since that could be invalidated if there are multiple panes within the parent that are being unpinned.
						//
						if (initializeFlyoutExtent)
						{
							Debug.Assert(container.ContainerElement != null);

							if (container.ContainerElement == null)
								initializeFlyoutExtent = false;
							else if (!pane.IsMeasureValid)
							{
								this.DeferProcessPinnedState(pane, allowShowFlyout);
								return;
							}
						}

						if (initializeFlyoutExtent)
						{
							FrameworkElement elementWithExtent = container is TabGroupPane ? container as FrameworkElement : pane;
							double extent = isVert ? elementWithExtent.ActualWidth : elementWithExtent.ActualHeight;

							// AS 3/25/10 TFS29399
							// This shouldn't happen but just in case we should not set the flyout 
							// to a 0 extent or the end user will not be able to see the flyout.
							//
							Debug.Assert(extent > 0);

							if (extent > 0)
							{
								// find the splitter of the root split pane and add in its width/height
								IPaneContainer parentContainer = container.ContainerElement as IPaneContainer;
								IPaneContainer previous = null;

								while (parentContainer != null && parentContainer is XamDockManager == false)
								{
									previous = parentContainer;
									parentContainer = DockManagerUtilities.GetParentPane(parentContainer);
								}

								if (previous is SplitPane)
								{
									PaneSplitter splitter = PaneSplitter.GetSplitter(previous as SplitPane);

									if (null != splitter)
										extent += isVert ? splitter.ActualWidth : splitter.ActualHeight;
								}

								UnpinnedTabFlyout.SetFlyoutExtent(pane, extent);
							}
						}

						// get the edge on which the pane is docked so we know what unpinned area to add it to
						Dock unpinnedSide = DockManagerUtilities.GetDockedSide(location);

                        // AS 10/15/08 TFS8068
                        using (DockManagerUtilities.CreateMoveReplacement(pane))
                        {
                            // remove the pane and be sure it stores a placeholder for it
                            container.RemoveContentPane(pane, true);

                            IContentPaneContainer unpinnedContainer = this._tabAreaInfos[(int)unpinnedSide];

                            // add it to the appropriate unpinned side
                            unpinnedContainer.InsertContentPane(null, pane);
                        }

						// animate hiding the pane (unless we are loading a layout)
						// AS 5/28/08
						// I found this while writing another unit test. We don't want to bother showing the
						// flyout only to hide it, if the flyout animation is none.
						//
						//if (allowShowFlyout && this.IsLoaded && this.IsShowFlyoutSuspended == false)
						if (allowShowFlyout 
							&& this.IsLoaded 
							&& this.IsShowFlyoutSuspended == false
							&& this.FlyoutAnimation != PaneFlyoutAnimation.None
							)
							this.ShowAndHideFlyout(pane);

						break;
					}
				case PaneLocation.Floating:
				case PaneLocation.FloatingOnly:
				case PaneLocation.Document:
					break;
				default:
				case PaneLocation.Unknown:
					Debug.Assert(location == PaneLocation.Unknown);
					break;
			}
		}

		#endregion //ProcessPinnedState

		#region RemoveLogicalChildInternal
		/// <summary>
		/// Helper method for removing a child from the dockmanager's logical tree
		/// </summary>
		/// <param name="logicalChild">The child to remove from the logical tree</param>
		internal void RemoveLogicalChildInternal(DependencyObject logicalChild)
		{
			Debug.Assert(this._logicalChildren != null);

			if (null != this._logicalChildren)
			{
				Debug.Assert(this._logicalChildren.Contains(logicalChild));

				this._logicalChildren.Remove(logicalChild);
				this.RemoveLogicalChild(logicalChild);
			}
		} 
		#endregion //RemoveLogicalChildInternal

		#region ShowAndHideFlyout
		/// <summary>
		/// Helper method for showing a pane in the flyout and animating its hiding ignoring the mouse over state.
		/// </summary>
		/// <param name="contentPane">The pane to display in the flyout</param>
		private void ShowAndHideFlyout(ContentPane contentPane)
		{
			Debug.Assert(contentPane != null, "We need a pane!");
			Debug.Assert(contentPane == null || XamDockManager.GetPaneLocation(contentPane) == PaneLocation.Unpinned, "The pane is not unpinned!");
			Debug.Assert(this._dockPanel != null, "We need the dockpanel since it contains the flyout panel!");

			if (this._dockPanel != null && null != contentPane)
				this._dockPanel.FlyoutPanel.ShowAndHideFlyout(contentPane);
		} 
		#endregion //ShowAndHideFlyout 

		#region ShowFlyout
		/// <summary>
		/// Displays the flyout for the specified pane.
		/// </summary>
		/// <param name="contentPane">The pane being displayed in the unpinned flyout</param>
		/// <param name="delay">True if the show can be delayed</param>
		/// <param name="basedOnMouseOver">True if the pane is being shown due to a mouse over notification</param>
		internal void ShowFlyout(ContentPane contentPane, bool delay, bool basedOnMouseOver)
		{
			Debug.Assert(contentPane != null, "We need a pane!");
			Debug.Assert(contentPane == null || XamDockManager.GetPaneLocation(contentPane) == PaneLocation.Unpinned, "The pane is not unpinned!");
			Debug.Assert(this._dockPanel != null, "We need the dockpanel since it contains the flyout panel!");

			if (this._dockPanel != null && null != contentPane)
				this._dockPanel.FlyoutPanel.ShowFlyout(contentPane, delay, basedOnMouseOver, true);
		}
		#endregion //ShowFlyout

		#region ToggleDockedState
		/// <summary>
		/// Changes the docked state from floating to docked or vice versa. This overload assumes that the executing/executed events have been raised.
		/// </summary>
		/// <param name="pane">Pane whose state is to be toggled</param>
		/// <returns></returns>
		internal bool ToggleDockedState(ContentPane pane)
		{
			// AS 10/27/10 TFS58542
			//return ToggleDockedState(new ContentPane[] { pane });
			List<ContentPane> panes = new List<ContentPane>();
			panes.Add(pane);
			return ToggleDockedState(panes, false);
		}

		private bool ToggleDockedStateImpl(ContentPane pane)
		{
			// AS 5/2/08 BR32056
			// This method is sometimes called directly - e.g. when dbl clicking a pane header
			// or floating window caption so we should make sure the pane is allowed to toggle
			// its state.
			//
			if (pane.CanToggleDockedState == false)
				return false;

			ContentPane.PanePlacementInfo placement = pane.PlacementInfo;
			IContentPaneContainer currentContainer = placement.CurrentContainer;
			IContentPaneContainer newContainer = null;
			
			// AS 5/2/08 BR32056
			// Allow for a new container to be created.
			//
			SplitPane newSplitPane = null;

			Debug.Assert(DockManagerUtilities.IsDockable(currentContainer.PaneLocation));

			if (DockManagerUtilities.IsDocked(currentContainer.PaneLocation))
			{
				// use the floating container if its not floating only
				ContentPanePlaceholder floatingPlaceholder = placement.FloatingDockablePlaceholder;

				if (floatingPlaceholder != null)
					newContainer = floatingPlaceholder.Container;
				else
				{
					// AS 5/2/08 BR32056
					// Even if the pane is not floating then we will support toggling to the
					// floating state. We will follow VS though and still not allow toggling
					// to a docked state even if it was never docked.
					//
					newSplitPane = DockManagerUtilities.CreateSplitPane(this);
					XamDockManager.SetInitialLocation(newSplitPane, InitialPaneLocation.DockableFloating);
					XamDockManager.SetFloatingSize(newSplitPane, pane.GetSizeForFloating());

					if (null != pane.LastFloatingLocation)
						XamDockManager.SetFloatingLocation(newSplitPane, pane.LastFloatingLocation);

					newContainer = newSplitPane;
				}
			}
			else
			{
				// get the docked placeholder
				newContainer = placement.DockedContainer;
			}

			if (newContainer == null)
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
			//DockManagerUtilities.MovePane(pane, newContainer, null, newContainer.PaneLocation);
			//
			//// AS 5/2/08 BR32056
			//// If this is a new floating window then we can add it to the dockmanager once we have the pane inside it
			//if (null != newSplitPane)
			//    this.Panes.Add(newSplitPane);
			//
			//// AS 10/15/08 TFS8068
			//if (null != replacement)
			//    replacement.Dispose();
			using (MovePaneHelper moveHelper = new MovePaneHelper(this, newContainer, pane))
			{
				// If this is a new floating window then we can add it to the dockmanager
				if (null != newSplitPane)
					this.Panes.Add(newSplitPane);

				DockManagerUtilities.MovePane(pane, newContainer, null, newContainer.PaneLocation, moveHelper);
			}

            return true;
		}

		// AS 10/27/10 TFS58542
		//internal bool ToggleDockedState(IList<ContentPane> panes)
		internal bool ToggleDockedState(List<ContentPane> panes, bool raiseEvents)
		{
			ContentPane activePane = this.ActivePane;
			bool result = false;

			// AS 10/27/10 TFS58542
			// Before we try to raise events, filter out any panes that 
			// cannot be toggled so we don't raise any events for them.
			//
			for (int i = panes.Count - 1; i >= 0; i--)
			{
				ContentPane child = panes[i];

				if (!child.CanToggleDockedState)
					panes.RemoveAt(i);
			}

			// AS 10/27/10 TFS58542
			// If the caller did not raise the executing then we need to raise 
			// it now. To maintain the existing behavior where only those that 
			// can be toggled will be toggled, we will just remove those that 
			// cannot be toggled and continue with those that can.
			//
			if (raiseEvents)
			{
				// next raise the command executing event for these
				for (int i = panes.Count - 1; i >= 0; i--)
				{
					ContentPane child = panes[i];

					ExecutingCommandEventArgs beforeArgs = new ExecutingCommandEventArgs(ContentPaneCommands.ToggleDockedState);
					child.RaiseExecutingCommand(beforeArgs);

					// if any cancel then cancel the operation
					if (beforeArgs.Cancel)
					{
						panes.RemoveAt(i);
					}
				}
			}

			// AS 10/27/10 TFS58542
			// If everything was filtered out then skip the operation.
			//
			if (panes.Count == 0)
				return false;

			IList<IContentPaneContainer> oldContainers = DockManagerUtilities.GetPaneContainers(panes); // AS 3/17/11 TFS67321

			foreach (ContentPane pane in panes)
			{
				if (ToggleDockedStateImpl(pane))
					result = true;
			}

			DockManagerUtilities.RemoveContainersIfNeeded(oldContainers); // AS 3/17/11 TFS67321

			if (result && null != activePane && activePane.CanActivate)
			{
				// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
				//activePane.ActivateInternal(true);
				activePane.ActivateInternal(true, true, panes.Contains(activePane));
			}

			// AS 10/27/10 TFS58542
			// If we raised the before event then we should raise the after as well.
			//
			if (raiseEvents)
			{
				// lastly raise the after event
				foreach (ContentPane child in panes)
				{
					ExecutedCommandEventArgs args = new ExecutedCommandEventArgs(ContentPaneCommands.ToggleDockedState);
					child.RaiseExecutedCommand(args);
				}
			}

			return result;
		}

		#endregion //ToggleDockedState

		// AS 9/9/09 TFS21110
		#region TransferFocusOutOfHwndHost
		internal void TransferFocusOutOfHwndHost()
		{
			foreach (HwndHostInfo hhi in this.GetHwndHostInfos())
			{
				DependencyObject d = hhi.GetFocusedHwndHost();

				if (null != d)
				{
					while (d != null)
					{
						IInputElement inputElement = d as IInputElement;

						// walk up the ancestor chain until we find an element that is focusable
						if (null != inputElement &&
							d is HwndHost == false &&
							inputElement.Focusable &&
							inputElement.IsEnabled &&
							true.Equals(d.GetValue(UIElement.IsVisibleProperty)))
						{
							XamDockManager dm = XamDockManager.GetDockManager(d);

							if (null != dm)
							{
								dm.ForceFocus(inputElement);
								break;
							}
						}

						d = Utilities.GetParent(d, true);
					}
					break;
				}
			}
		}
		#endregion //TransferFocusOutOfHwndHost

		#region UnpinPane

		internal void UnpinPane(ContentPane pane, bool includeSiblingsIfAllowed)
		{
			IList<ContentPane> panesToUnpin = GetPanesToProcess(pane, includeSiblingsIfAllowed, this.PinBehavior);

			for (int i = 0, count = panesToUnpin.Count; i < count; i++)
			{
				ContentPane visiblePane = panesToUnpin[i];

				if (visiblePane.AllowPinning)
					panesToUnpin[i].IsPinned = !panesToUnpin[i].IsPinned;
			}
		}
		#endregion //UnpinPane

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region VerifyFillPane
		internal void VerifyFillPane()
		{
			SplitPane currentFill = this.FillPane;
			SplitPane newFill = null;

			if (this.LayoutMode == DockedPaneLayoutMode.FillContainer)
			{
				for (int i = this.Panes.Count - 1; i >= 0; i--)
				{
					SplitPane rootPane = this._panes[i];

					// we only care about the innermost visible docked splitpane
					if (rootPane.Visibility == Visibility.Collapsed ||
						false == DockManagerUtilities.IsDocked(rootPane))
						continue;

					newFill = rootPane;
					break;
				}
			}

			// in case we have a weak reference but we don't have a fill clear the wr
			if (currentFill == null)
				this._fillPane = null;

			if (newFill != currentFill)
			{
				if (null != currentFill)
					currentFill.ClearValue(SplitPane.IsFillPanePropertyKey);

				if (null == newFill)
					this._fillPane = null;
				else
				{
					this._fillPane = new WeakReference(newFill);
					newFill.SetValue(SplitPane.IsFillPanePropertyKey, KnownBoxes.TrueBox);
				}
			}
		}
		#endregion //VerifyFillPane

		#endregion //Internal Methods

		#region Protected methods

		#region CreatePaneToolWindow
		/// <summary>
		/// Factory method used to create a new <see cref="PaneToolWindow"/> to be used within the xamDockManager.
		/// </summary>
		/// <returns>A new <see cref="PaneToolWindow"/>instance</returns>
		internal protected virtual PaneToolWindow CreatePaneToolWindow()
		{
			PaneToolWindow window = new PaneToolWindow();
			return window;
		}
		#endregion //CreatePaneToolWindow

		#region CreateSplitPane
		/// <summary>
		/// Factory method used to create a new <see cref="SplitPane"/> to be used within the xamDockManager.
		/// </summary>
		/// <returns>A new <see cref="SplitPane"/>instance</returns>
		internal protected virtual SplitPane CreateSplitPane()
		{
			SplitPane split = new SplitPane();
			split.Name = DockManagerUtilities.AutoGeneratedName();
			return split;
		}
		#endregion //CreateSplitPane

		#region CreateTabGroupPane
		/// <summary>
		/// Factory method used to create a new <see cref="TabGroupPane"/> to be used within the xamDockManager.
		/// </summary>
		/// <returns>A new <see cref="TabGroupPane"/>instance</returns>
		internal protected virtual TabGroupPane CreateTabGroupPane()
		{
			TabGroupPane group = new TabGroupPane();
			group.Name = DockManagerUtilities.AutoGeneratedName();
			return group;
		}
		#endregion //CreateTabGroupPane

		#endregion //Protected methods

		#region Private Methods

		// AS 7/16/09 TFS18452
		#region DeferProcessPinnedState

		private void DeferProcessPinnedState(ContentPane pane, bool allowShowFlyout)
		{
			Debug.Assert(null != pane);

			if (_deferredPinnedPanes == null)
			{
				_deferredPinnedPanes = new Dictionary<ContentPane,bool>();

				EventHandler handler = new EventHandler(OnLayoutUpdated);
				this.LayoutUpdated -= handler;
				this.LayoutUpdated += handler;
			}

			_deferredPinnedPanes[pane] = allowShowFlyout;
		}
		#endregion //DeferProcessPinnedState

		#region DestroyPaneElement
		private void DestroyPaneElement(IContentPaneContainer container, ContentPane pane)
		{
			Debug.Assert(null != container || (VisualTreeHelper.GetParent(pane) == null && LogicalTreeHelper.GetParent(pane) == null));

			if (null != container)
			{
				container.RemoveContentPane(pane, false);

				DockManagerUtilities.RemoveContainerIfNeeded(container);
			}
		} 
		#endregion //DestroyPaneElement

		#region ExecuteCommandImpl

		private bool ExecuteCommandImpl(ExecuteCommandInfo commandInfo)
		{
			RoutedCommand command = commandInfo.RoutedCommand;

			// Make sure we have a command to execute.
			DockManagerUtilities.ThrowIfNull(command, "command");

			// Make sure the minimal control state exists to execute the command.
			if (DockManagerCommands.IsMinimumStatePresentForCommand(this as ICommandHost, command) == false)
				return false;

			// make sure the command can be executed
			if (false == ((ICommandHost)this).CanExecute(commandInfo))
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

			ContentPane pane = GetPaneForCommand(commandInfo);

			Debug.Assert(pane == null || XamDockManager.GetDockManager(pane) == this);

			if (command == DockManagerCommands.ActivateNextPane ||
				command == DockManagerCommands.ActivatePreviousPane)
			{
				handled = this._activePaneManager.ActivateNextPane(pane, command == DockManagerCommands.ActivateNextPane);
			}
			else if (command == DockManagerCommands.ActivateNextDocument ||
				command == DockManagerCommands.ActivatePreviousDocument)
			{
				handled = this._activePaneManager.ActivateNextDocument(pane, command == DockManagerCommands.ActivateNextDocument);
			}
			else if (command == DockManagerCommands.CloseActiveDocument)
			{
				ContentPane activeDocument = this.ActivePaneManager.GetActiveDocument();

				if (null != activeDocument && activeDocument.AllowClose)
					this.ClosePane(activeDocument, false);
			}
			else if (command == DockManagerCommands.ShowPaneNavigator)
			{
				PaneNavigatorStartInfo startInfo = commandInfo.Parameter as PaneNavigatorStartInfo ?? new PaneNavigatorStartInfo();
				// AS 9/9/09 TFS21110
				// If we could not show the navigator then we should not mark the event as handled.
				//
				//PaneNavigator.Show(this, startInfo);
				//handled = true;
				handled = PaneNavigator.Show(this, startInfo);
			}


// =========================================================================================

//PostExecute:
			// If the command was executed, fire the 'after executed' event.
			if (handled == true)
				this.RaiseExecutedCommand(new ExecutedCommandEventArgs(command));


			return handled;
		}

		#endregion //ExecuteCommandImpl

		// AS 9/9/09 TFS21110
		// Refactored from IHwndHostContainer.GetHosts impl.
		//
		#region GetHwndHostInfos
		private IEnumerable<HwndHostInfo> GetHwndHostInfos()
		{
			if (null != this._dockPanel)
			{
				HwndHostInfo hhi = HwndHostInfo.GetHwndHost(_dockPanel);

				if (null != hhi)
					yield return hhi;
			}

			// AS 9/8/09 TFS21746
			if (this.Content is FrameworkElement)
			{
				HwndHostInfo hhi = HwndHostInfo.GetHwndHost((DependencyObject)this.Content);

				if (null != hhi)
					yield return hhi;
			}

			foreach (ContentPane cp in this.GetPanes(PaneNavigationOrder.ActivationOrder))
			{
				HwndHostInfo hhi = HwndHostInfo.GetHwndHost(cp);

				if (null != hhi)
					yield return hhi;
			}
		} 
		#endregion //GetHwndHostInfos

		#region GetPaneForCommand
		private ContentPane GetPaneForCommand(ExecuteCommandInfo commandInfo)
		{
			return GetPaneForCommand(commandInfo.Parameter, commandInfo.OriginalSource, this.ActivePane);
		}
		#endregion //GetPaneForCommand

		#region GetPanesToProcess
		private IList<ContentPane> GetPanesToProcess(ContentPane pane, bool includeSiblingsIfAllowed, PaneActionBehavior behavior)
		{
			IList<ContentPane> panesToProcess;
			IContentPaneContainer container = pane.PlacementInfo.CurrentContainer;
			Debug.Assert(container != null);

			// AS 4/7/08 BR31793
			// We don't honor the pane action when the panes come from a split pane.
			//
			// AS 5/21/08 BR33191
			// We only want to look at the docked container if the pane is unpinned.
			//
			//if (pane.PlacementInfo.DockedContainer is TabGroupPane == false)
			if (pane.PaneLocation == PaneLocation.Unpinned && pane.PlacementInfo.DockedContainer is TabGroupPane == false)
				includeSiblingsIfAllowed = false;

			if (null != container 
				&& includeSiblingsIfAllowed
				&& behavior == PaneActionBehavior.AllPanes)
			{
				panesToProcess = container.GetAllPanesForPaneAction(pane);
			}
			else
				panesToProcess = new ContentPane[] { pane };

			return panesToProcess;
		}
		#endregion //GetPanesToProcess

		#region GetTransferCommandTarget
		// AS 10/14/10 TFS36772
		// The bulk of this is the logic that was done in the transfer(can)execute routine 
		// to figure out what element to transfer to. Now though this will delve into that 
		// object if the target was to be a dockmanager.
		//
		private IInputElement GetTransferCommandTarget()
		{
			ContentPane cp = this.ActivePane;

			// AS 3/15/11 TFS63842
			// If keyboard focus shifts out of the dockmanager then the ActivePane 
			// will return null but if the command is directed into the DM then 
			// we should delegate to the last active pane (assuming logical focus 
			// isn't within some other element in the dockmanager).
			//
			DependencyObject thisFocusScope = FocusManager.GetFocusScope(this);

			if (cp == null && !this.IsKeyboardFocusWithin)
			{
				if (thisFocusScope != null && FocusManager.GetFocusedElement(thisFocusScope) == this)
					cp = this.ActivePaneManager.GetLastActivePane();
			}

			DependencyObject cpFocusScope = null != cp ? FocusManager.GetFocusScope(cp) : null;

			if (cp != null												// we have an active pane
				&& false == DesignerProperties.GetIsInDesignMode(this)	// not in design time
				// AS 3/15/11 TFS63842
				//&& FocusManager.GetFocusScope(this) != cpFocusScope		// different focusscope - if it was same it should have gotten called directly
				&& thisFocusScope != cpFocusScope						// different focusscope - if it was same it should have gotten called directly
				// AS 3/30/09 TFS16355 - WinForms Interop
				//&& Utilities.IsDescendantOf(this, cp))					// within the this - i.e. not a floating pane
				&& Utilities.IsDescendantOf(this, cp, true))            // within the this - i.e. not a floating pane
			{
				IInputElement focusedElement = FocusManager.GetFocusedElement(cpFocusScope);

				DependencyObject focusedObject = focusedElement as DependencyObject;
				XamDockManager innerDm = focusedObject as XamDockManager;

				if (innerDm == null && focusedObject != null)
				{
					XamDockManager tempDm = XamDockManager.GetDockManager(focusedObject);

					// if the focused object is the drag helper of the dockmanager then consider
					// the dockmanager to be focused
					if (tempDm != null && tempDm._dragHelper == focusedObject)
						innerDm = tempDm;
				}

				if (innerDm != null && innerDm != this)
					return innerDm.GetTransferCommandTarget();

				return focusedElement;
			}

			return null;
		}
		#endregion //GetTransferCommandTarget

		// AS 7/1/10 TFS34460
		// See OnApplyTemplate for details.
		//
		#region OnDockPanelLoaded
		private void OnDockPanelLoaded(object sender, RoutedEventArgs e)
		{
			if (null != _dockPanel)
			{
				_dockPanel.Panes.ReInitialize(this._panes.DockedPanes);
			}
		}
		#endregion //OnDockPanelLoaded

		#region OnDockManagerLoaded
		// AS 4/30/08
		private void OnDockManagerLoaded(object sender, RoutedEventArgs e)
		{
			// if we are on a tabcontrol but not on the selected tab, then we still
			// get a loaded when the form is first displayed so ignore that if we
			// are not visible yet. when the tab that contains us is selected, we will
			// get another loaded event.
			if (this.IsVisible == false)
			{
				// AS 6/23/11 TFS73499
				// The floating windows may need to be shown anyway depending on the FloatingWindowVisibility.
				//
				this.VerifyFloatingWindowVisibility();

				return;
			}

			// hook the unloaded event and unhook the loaded event. we only want to be
			// hooked into one since its possible to get 2 loadeds without an unloaded
			// between them - e.g. be on a tab control in the selected tab, select another
			// tab (unloaded fires) then close the window (unloaded fires again).
			//
			this.Unloaded += new RoutedEventHandler(OnDockManagerUnloaded);
			this.Loaded -= new RoutedEventHandler(OnDockManagerLoaded);

			// AS 6/23/11 TFS73499
			//this.SetIsWindowLoaded(true);
			this.VerifyFloatingWindowVisibility();

			// AS 1/6/10 TFS25507
			this.ProcessDeferredUnpinnedPanes();

            // AS 3/30/09 TFS16355 - WinForms Interop
            // If the floaters are shown in a top level window then we should bring 
            // those above any HwndHosts when it first comes up.
            //
            if (this.ShowToolWindowsInPopup)
                this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new DockManagerUtilities.MethodInvoker(this.BringToolWindowsToFront));
        }
		#endregion //OnDockManagerLoaded

        #region OnDockManagerUnloaded
        // AS 4/30/08
        private void OnDockManagerUnloaded(object sender, RoutedEventArgs e)
		{
			// unhook the unloaded event and hook the loaded event. this works
			// around a bug in the wpf framework where the unloaded could fire
			// multiple times - more times than the loaded. i saw this when the dm
			// was in a tab control. it fired when a different tab was selected
			// and again when the form was closed
			this.Unloaded -= new RoutedEventHandler(OnDockManagerUnloaded);
			this.Loaded += new RoutedEventHandler(OnDockManagerLoaded);

			// AS 6/23/11 TFS73499
			//this.SetIsWindowLoaded(false);
			this.VerifyFloatingWindowVisibility();
		}
		#endregion //OnDockManagerUnloaded

		// AS 5/20/08 BR31679
		#region OnIsInDesignModeChanged
		// AS 7/17/12 TFS116580
		//private static void OnIsInDesignModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		//{
		//    ((XamDockManager)d).OnIsInDesignModeChanged((bool)e.OldValue, (bool)e.NewValue);
		//}
		//
		//private void OnIsInDesignModeChanged(bool oldDesignMode, bool newDesignMode)
		//{
		//    foreach (SplitPane split in this.Panes)
		//    {
		//        if (DockManagerUtilities.IsFloating(XamDockManager.GetPaneLocation(split)))
		//        {
		//            this.OnFloatingPaneRemoved(split, oldDesignMode);
		//            this.OnFloatingPaneAdded(split, newDesignMode);
		//        }
		//    }
		//}
		#endregion //OnIsInDesignModeChanged

		#region OnLayoutUpdated
		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			// AS 1/6/10 TFS25507
			// Moved to a helper method so we could asynchronously process this 
			// action as well as invoke it elsewhere as needed. Also we were not 
			// unhooking the LayoutUpdated.
			//
			//// AS 7/16/09 TFS18452
			//// If there were unpinned panes when the dockmanager was initially created 
			//// then we would not have been able to get the initial size of the panes 
			//// that were unpinned to know what the flyout extent should be.
			////
			//Dictionary<ContentPane, bool> deferredPinned = _deferredPinnedPanes;
			//_deferredPinnedPanes = null;
			//
			//if (null != deferredPinned)
			//{
			//    foreach (KeyValuePair<ContentPane, bool> pair in deferredPinned)
			//    {
			//        if (XamDockManager.GetDockManager(pair.Key) != this)
			//            continue;
			//
			//        this.ProcessPinnedState(pair.Key, pair.Key.PaneLocation, pair.Value);
			//    }
			//}
			this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);
		
			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, 
				new DockManagerUtilities.MethodInvoker(ProcessDeferredUnpinnedPanes));
		}
		#endregion //OnLayoutUpdated

		// AS 6/4/08 BR33654
		#region OnTransferXXX methods

		#region OnTransferBindingExecute
		private static void OnTransferBindingExecute(object sender, ExecutedRoutedEventArgs e)
		{
			XamDockManager dm = (XamDockManager)sender;
			Debug.Assert(e.Command is RoutedCommand, "This method is supposed to be used for specific routed commands only.");

			// AS 10/14/10 TFS36762
			if (!dm.ShouldTransferCommands())
				return;

			dm.TransferExecute(e.Command as RoutedCommand, e);
		}
		#endregion //OnTransferBindingExecute

		#region OnTransferBindingCanExecute
		private static void OnTransferBindingCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			XamDockManager dm = (XamDockManager)sender;
			Debug.Assert(e.Command is RoutedCommand, "This method is supposed to be used for specific routed commands only.");

			// AS 10/14/10 TFS36762
			// Created a helper method to find out when we should transfer commands.
			//
			//// AS 3/26/10 TFS30153
			//// See the notes in OnTransferCanExecuteCommand
			////
			//ContentPane parentPane = FocusManager.GetFocusScope(dm) as ContentPane;
			//
			//if (null != parentPane)
			//{
			//    XamDockManager dmOuter = GetDockManager(parentPane);
			//
			//    if (dmOuter != null && dmOuter._transferCanExecuteCommand == null)
			//        return;
			//}
			if (!dm.ShouldTransferCommands())
			{
				// AS 6/7/11 TFS76560
				// Since this may get invoked as a result of a keyboard gesture we need to mark 
				// the event such that it will continue routing.
				e.ContinueRouting = true;

				return;
			}

			// AS 6/8/09 TFS18259
			// The keyboard shortcut for the paste command is bubbling up to the XDM but 
			// nothing inside it has used it. If its not handled then we want it to continue
			// routing up.
			//
			bool updateContinueRouting = dm._transferCanExecuteCommand != e.Command && false == e.Handled && false == e.ContinueRouting;

			if (updateContinueRouting)
				e.ContinueRouting = true;

			dm.TransferCanExecute(e.Command as RoutedCommand, e);

			// AS 6/8/09 TFS18259
			// If we manipulated the continue routing state and the event was handled then 
			// don't let the command continue routing.
			//
			if (updateContinueRouting && e.Handled)
				e.ContinueRouting = false;
		}
		#endregion //OnTransferBindingCanExecute

		#region OnTransferCanExecuteCommand
		private static void OnTransferCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			XamDockManager dm = (XamDockManager)sender;
			RoutedCommand rc = e.Command as RoutedCommand;

			// note: we need to special case paste. that will be handled via a command
			// binding to preserve the uipermission asserted by the framework
			// AS 10/14/10 TFS36772 Cut/Copy
			// In CLR4, Copy and Paste are also securecommands.
			//
			//if (null != rc && rc != ApplicationCommands.Paste)
			if (null != rc && rc.GetType() != _secureCommandType)
			{
				// AS 3/26/10 TFS30153
				// There was a bug reported a while back whereby a commandhost that was in 
				// a focusscope outside the dockmanager (e.g. a button in a toolbar that is 
				// sibling to a dockmanager) invoked the canexecute on itself (because no
				// commandtarget was specified) but that never reached the control within 
				// the pane. The reason was because when the canexecute bubbled up from the 
				// toolbar button to the toolbar, the WPF commandmanager class caught the 
				// event and re-raised it for the focused element of the focus scope that 
				// contained the toolbar. That was the dockmanager. So to get around that 
				// limitation/issue in commandmanager, we would transfer the canexecute 
				// to the focusedelement of its activepane. However, there is a problem 
				// when the commandhost that initiates the canexecute is within the dm.
				// In that case what happens is that the event bubbles up to the inner 
				// most dm. It doesn't do anything but when it bubbles up to the pane 
				// that contains that inner dm, the commandmanager sees this as another 
				// focus scope change and reinvokes it on the focusedelement of its 
				// containing focus scope - that pane's dm. It then transfer the event 
				// into its active pane (which just received the event). Then it 
				// continues bubbling up until it hits that dm's containing dm. So 
				// ultimately what you get is the canexecute being invoked once for 
				// each nested dockmanager. Since we cannot detect whether the command 
				// is being invoked for an external or internal commandhost (because 
				// when the commandmanager transfers the event it does so with a new 
				// event args and uses that focusedelement as the original source) we 
				// cannot prevent all the transferring. However we can avoid transferring 
				// until we reach the outermost dm by not transferring if the dm 
				// is within a contentpane and it is not transferring the command into us.
				//
				// AS 10/14/10 TFS36762
				//ContentPane parentPane = FocusManager.GetFocusScope(dm) as ContentPane;
				//
				//if (null != parentPane)
				//{
				//    XamDockManager dmOuter = GetDockManager(parentPane);
				//
				//    if (dmOuter != null && dmOuter._transferCanExecuteCommand == null)
				//        return;
				//}
				if (!dm.ShouldTransferCommands())
					return;

				dm.TransferCanExecute(rc, e);
			}
		}
		#endregion //OnTransferCanExecuteCommand

		#region OnTransferExecuteCommand
		private static void OnTransferExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			XamDockManager dm = (XamDockManager)sender;
			RoutedCommand rc = e.Command as RoutedCommand;

			// AS 10/14/10 TFS36762
			if (!dm.ShouldTransferCommands())
				return;

			// note: we need to special case paste. that will be handled via a command
			// binding to preserve the uipermission asserted by the framework
			// AS 10/14/10 TFS36772 Cut/Copy
			// In CLR4, Copy and Paste are also securecommands.
			//
			//if (null != rc && rc != ApplicationCommands.Paste)
			if (null != rc && rc.GetType() != _secureCommandType)
			{
				dm.TransferExecute(rc, e);
			}
		}
		#endregion //OnTransferExecuteCommand

		#endregion //OnTransferXXX methods

		// AS 1/6/10 TFS25507
		// Moved here from the OnLayoutUpdated.
		//
		#region ProcessDeferredUnpinnedPanes
		private void ProcessDeferredUnpinnedPanes()
		{
			// AS 7/16/09 TFS18452
			// If there were unpinned panes when the dockmanager was initially created 
			// then we would not have been able to get the initial size of the panes 
			// that were unpinned to know what the flyout extent should be.
			//
			Dictionary<ContentPane, bool> deferredPinned = _deferredPinnedPanes;
			_deferredPinnedPanes = null;

			if (null != deferredPinned)
			{
				foreach (KeyValuePair<ContentPane, bool> pair in deferredPinned)
				{
					if (XamDockManager.GetDockManager(pair.Key) != this)
						continue;

					this.ProcessPinnedState(pair.Key, pair.Key.PaneLocation, pair.Value);
				}
			}
		}
		#endregion //ProcessDeferredUnpinnedPanes

		#region ResumeShowFlyout
		internal void ResumeShowFlyout()
		{
			Debug.Assert(this._suspendShowFlyoutCount > 0);
			this._suspendShowFlyoutCount--;
		}
		#endregion //ResumeShowFlyout

		#region SetIsWindowLoaded
		// AS 4/30/08
		private void SetIsWindowLoaded(bool loaded)
		{
			Debug.Assert(loaded == false || this.AllowToolWindowLoaded);

			PaneToolWindow[] windows = new PaneToolWindow[this._toolWindows.Count];
			this._toolWindows.CopyTo(windows, 0);

			// when the dockmanager is loaded (or reloaded) then reshow the floating windows
			foreach (PaneToolWindow window in windows)
			{
				// AS 2/21/12 TFS99925
				//window.IsWindowLoaded = loaded;
				SetIsWindowLoaded(window, loaded);
			}
		}

		// AS 2/21/12 TFS99925
		private void SetIsWindowLoaded(PaneToolWindow window, bool loaded)
		{
			if (loaded)
			{
				// we're going to selectively suppress the loading of the toolwindow.
				// if the window has been shown then we'll leave it alone since the 
				// OS should have hidden the window. this should also reduce the chance 
				// of introducing an issue. if we don't have an hwndsource (i.e. we 
				// don't have the rights) then likely we're not using windows and 
				// we'll just let the window show. if the window associated with the 
				// hwnd is normal/maximized or if the rootvisual isn't a window (e.g.
				// an elementhost) then we'll let the window be shown. for the latter 
				// we really don't want to be trying to track who the ancestor hwnd 
				// is and subclass it to find out when its window state changes.
				// lastly, we will limit this to only when the SynWithDockManagerWindow 
				// setting since like the visibility this really isn't something 
				// that is part of the owner/owned window contract or the OS would 
				// have handled it to begin with.
				if (!window.IsWindowLoaded &&
					this.FloatingWindowVisibility == FloatingWindowVisibility.SyncWithDockManagerWindow &&
					this.UseOwnedFloatingWindows && // this really only applies to owned windows since unowned windows would not be hidden by the os when the main window is minimized
					_hwndSourceHelper != null &&
					_hwndSourceHelper.RootVisualWindowState == WindowState.Minimized)
				{
					loaded = false;
				}
			}

			window.IsWindowLoaded = loaded;
		}
		#endregion //SetIsWindowLoaded

		// AS 10/14/10 TFS36772
		#region ShouldTransferCommands
		private bool ShouldTransferCommands()
		{
			var containingPane = FocusManager.GetFocusScope(this) as ContentPane;
			XamDockManager parentDm = null;

			if (null != containingPane)
			{
				parentDm = GetDockManager(containingPane);
			}
			else
			{
				DependencyObject parent = VisualTreeHelper.GetParent(this);

				if (null != parent)
					parentDm = GetDockManager(parent);
			}

			// if there is an ancestor dockmanager then let it transfer the command
			if (null != parentDm)
				return false;

			return true;
		}
		#endregion //ShouldTransferCommands

		#region SuspendShowFlyout
		internal void SuspendShowFlyout()
		{
			this._suspendShowFlyoutCount++;
		}
		#endregion //SuspendShowFlyout

		// AS 5/20/08 BR31679
		#region OnFloatingPaneAdded
		private void OnFloatingPaneAdded(SplitPane pane)
		{
			this.OnFloatingPaneAdded(pane, DesignerProperties.GetIsInDesignMode(pane));
		}

		private void OnFloatingPaneAdded(SplitPane pane, bool isDesignMode)
		{
			PaneLocation paneLocation = DockManagerUtilities.ToPaneLocation(XamDockManager.GetInitialLocation(pane));

			if (isDesignMode)
			{
				// AS 5/20/08 BR31679
				// Just keep the element as a logical child of the xdm so that we
				// can still have the attached properties exposed for the elements.
				//
				pane.SetValue(XamDockManager.PaneLocationPropertyKey, DockManagerKnownBoxes.FromValue(paneLocation));
				this.AddLogicalChildInternal(pane);
			}
			else
			{
				PaneToolWindow window = this.CreatePaneToolWindow();
				window.SetValue(XamDockManager.DockManagerPropertyKey, this);
				window.SetValue(XamDockManager.PaneLocationPropertyKey, DockManagerKnownBoxes.FromValue(paneLocation));

                // AS 10/15/08 TFS8068
                // The floating windows are not part of the logical/visual tree of the 
                // dockmanager but they should reflect the same datacontext/right to left/etc.
                //
                window.SetBinding(FrameworkElement.DataContextProperty, Utilities.CreateBindingObject(FrameworkElement.DataContextProperty, BindingMode.OneWay, this));
                window.SetBinding(FrameworkElement.FlowDirectionProperty, Utilities.CreateBindingObject(FrameworkElement.FlowDirectionProperty, BindingMode.OneWay, this));

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				window.SetBinding(ToolWindow.AllowMaximizeProperty, Utilities.CreateBindingObject(XamDockManager.AllowMaximizeFloatingWindowsProperty, BindingMode.OneWay, this));
				window.SetBinding(ToolWindow.AllowMinimizeProperty, Utilities.CreateBindingObject(XamDockManager.AllowMinimizeFloatingWindowsProperty, BindingMode.OneWay, this));
				window.SetBinding(ToolWindow.ShowInTaskbarProperty, Utilities.CreateBindingObject(XamDockManager.ShowFloatingWindowsInTaskbarProperty, BindingMode.OneWay, this));
				window.SetBinding(ToolWindow.IsOwnedWindowProperty, Utilities.CreateBindingObject(XamDockManager.UseOwnedFloatingWindowsProperty, BindingMode.OneWay, this));

				// AS 6/24/11 FloatingWindowCaptionSource
				window.SetBinding(PaneToolWindow.FloatingWindowCaptionSourceProperty, Utilities.CreateBindingObject(XamDockManager.FloatingWindowCaptionSourceProperty, BindingMode.OneWay, this));

				window.Content = pane;
				this._toolWindows.Add(window);

				// AS 4/30/08
				// Instead of raising the tool window event when the pane is first added, we're
				// going to manage it around when the dm is loaded/unloaded. This helps in 2 cases.
				// First, if the dm is on a page/tab and you select another page/tab, this 
				// will give the programmer a chance to unhook any events on the pane tool window.
				// Second, just being on the window and closing the window, the events will now
				// fire where as previously they would not have.
				//
				if (this.AllowToolWindowLoaded)
				{
					// AS 2/21/12 TFS99925
					//window.IsWindowLoaded = true;
					SetIsWindowLoaded(window, true);
				}
			}
		} 
		#endregion //OnFloatingPaneAdded

		// AS 5/20/08 BR31679
		#region OnFloatingPaneRemoved
		private void OnFloatingPaneRemoved(SplitPane pane)
		{
			this.OnFloatingPaneRemoved(pane, DesignerProperties.GetIsInDesignMode(pane));
		}

		private void OnFloatingPaneRemoved(SplitPane pane, bool isDesignMode)
		{
			if (isDesignMode)
			{
				// AS 5/20/08 BR31679
				// Just keep the element as a logical child of the xdm so that we
				// can still have the attached properties exposed for the elements.
				//
				this.RemoveLogicalChildInternal(pane);
				pane.ClearValue(XamDockManager.PaneLocationPropertyKey);
			}
			else
			{
				pane.ClearValue(XamDockManager.PaneLocationPropertyKey);

				PaneToolWindow window = ToolWindow.GetToolWindow(pane) as PaneToolWindow;

				Debug.Assert(null != window);

				if (null != window)
					this._toolWindows.Remove(window);

				window.IsWindowLoaded = false;

				window.ClearValue(XamDockManager.DockManagerPropertyKey);
				window.ClearValue(XamDockManager.PaneLocationPropertyKey);
				window.Content = null;

				// AS 5/19/08 Reuse Group/Split
				// The window may be open so make sure its been closed so it
				// can be cleaned up properly.
				//
				// AS 5/22/08 BR33278
				// Do not close the window if we are in the Closing event.
				//
				if (window.IsClosing == false)
					window.Close();
			}
		} 
		#endregion //OnFloatingPaneRemoved

		// AS 6/23/11 TFS73499
		#region OnRootVisualVisibilityChanged
		private void OnRootVisualVisibilityChanged()
		{
			// this isn't necessarily required but i'm not fond of synchronously showing 
			// toolwindows while we are being hooked up to or detached from an hwndsource
			// so we'll process it async
			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DockManagerUtilities.MethodInvoker(VerifyFloatingWindowVisibility));
		}
		#endregion //OnRootVisualVisibilityChanged

		// AS 6/4/08 BR33654
		#region TransferExecute
		private void TransferExecute(RoutedCommand rc, ExecutedRoutedEventArgs e)
		{
			// note: we need to special case paste. that will be handled via a command
			// binding to preserve the uipermission asserted by the framework
			if (null != rc)
			{
				// if this is not the command we are transferring...
				if (rc != this._transferExecuteCommand)
				{
					RoutedCommand oldTransferCommand = this._transferExecuteCommand;
					this._transferExecuteCommand = rc;

					try
					{
						// AS 10/14/10 TFS36772
						// Previously we were handling the CanExecute and Execute by transferring 
						// these to the focused element of the active pane. The problem is that the 
						// CanExecute for that element would return false because we don't propogate
						// the canexecute for the command we are in the process of transfering the 
						// execute for (see fix for BR33775). Well that means if we have nested dock
						// managers then we don't propogate the execute to the innermost element. 
						// In thinking about this, what we were doing was not effecient because we 
						// would call execute/canexecute multiple times. While we stopped the bubble 
						// event from percolating above the dockmanager we still would have caused 
						// a series of previews from the root to the target for each level. Really what 
						// we should have been doing was just getting the innermost element that we 
						// would have routed to and have the outermost dockmanager route it directly 
						// to that. So now we won't get in here unless ShouldTransferCommands is true 
						// (so only for the outermost dm) and instead of raising execute/canexecute on 
						// the element within the active pane, we'll delve into that if the target 
						// was a dockmanager because that is what would have happened when we transferred 
						// the event.
						// 
						// Instead of transferring to the direct focused element of the active pane, 
						// I've created a helper method that will find the deepest descendant that will 
						// be the ul
						//ContentPane cp = this.ActivePane;
						//DependencyObject cpFocusScope = null != cp ? FocusManager.GetFocusScope(cp) : null;
						//
						//if (cp != null												// we have an active pane
						//    && false == DesignerProperties.GetIsInDesignMode(this)	// not in design time
						//    && FocusManager.GetFocusScope(this) != cpFocusScope		// different focusscope - if it was same it should have gotten called directly
						//    // AS 3/30/09 TFS16355 - WinForms Interop
						//    //&& Utilities.IsDescendantOf(this, cp))					// within the this - i.e. not a floating pane
						//    && Utilities.IsDescendantOf(this, cp, true))            // within the this - i.e. not a floating pane
						//{
						//    IInputElement focusedElement = FocusManager.GetFocusedElement(cpFocusScope);
						//
						//    if (null != focusedElement && rc.CanExecute(e.Parameter, focusedElement))
						//    {
						//        try
						//        {
						//            rc.Execute(e.Parameter, focusedElement);
						//        }
						//        finally
						//        {
						//            e.Handled = true;
						//        }
						//    }
						//}
						IInputElement focusedElement = this.GetTransferCommandTarget();

						if (null != focusedElement && rc.CanExecute(e.Parameter, focusedElement))
						{
							try
							{
								rc.Execute(e.Parameter, focusedElement);
							}
							finally
							{
								e.Handled = true;
							}
						}
					}
					finally
					{
						this._transferExecuteCommand = oldTransferCommand;
					}
				}
				else
				{
					// AS 6/10/08 BR33775
					// Actually we should not mark it handled. If we're getting here then
					// the command that we called execute on has bubbled up to us and either
					// the dockmanager or another ancestor element may need to handle this
					// command.
					//
					//// if we're transferring the event then stop the event here because
					//// we will let the original canexecute bubble up
					//e.Handled = true;
				}
			}
		}
		#endregion //TransferExecute

		// AS 6/4/08 BR33654
		#region TransferCanExecute
		private void TransferCanExecute(RoutedCommand rc, CanExecuteRoutedEventArgs e)
		{
			// note: we need to special case paste. that will be handled via a command
			// binding to preserve the uipermission asserted by the framework
			if (null != rc)
			{
				// AS 6/10/08 BR33775
				// If we're calling CanExecute then we shouldn't try to transfer
				// the request since it has already been routed to the focused
				// element of the active pane.
				//
				if (rc == this._transferExecuteCommand)
					return;

				// if this is not the command we are transferring...
				if (rc != this._transferCanExecuteCommand)
				{
					RoutedCommand oldTransferCommand = this._transferCanExecuteCommand;
					this._transferCanExecuteCommand = rc;

					try
					{
						// AS 10/14/10 TFS36772
						// See notes in TransferExecute for details. Basically have the outermost 
						// dm directly invoke the canexecute on the ultimate innermost target.
						//
						//ContentPane cp = this.ActivePane;
						//DependencyObject cpFocusScope = null != cp ? FocusManager.GetFocusScope(cp) : null;
						//
						//if (cp != null												// we have an active pane
						//    && false == DesignerProperties.GetIsInDesignMode(this)	// not in design time
						//    && FocusManager.GetFocusScope(this) != cpFocusScope		// different focusscope - if it was same it should have gotten called directly
						//    // AS 3/30/09 TFS16355 - WinForms Interop
						//    //&& Utilities.IsDescendantOf(this, cp))					// within the this - i.e. not a floating pane
						//    && Utilities.IsDescendantOf(this, cp, true))            // within the this - i.e. not a floating pane
						//{
						//    IInputElement focusedElement = FocusManager.GetFocusedElement(cpFocusScope);
						//
						//    if (null != focusedElement && rc.CanExecute(e.Parameter, focusedElement))
						//    {
						//        e.CanExecute = true;
						//        e.Handled = true;
						//    }
						//}
						IInputElement focusedElement = this.GetTransferCommandTarget();

						if (null != focusedElement && rc.CanExecute(e.Parameter, focusedElement))
						{
							e.CanExecute = true;
							e.Handled = true;
						}
					}
					finally
					{
						this._transferCanExecuteCommand = oldTransferCommand;
					}
				}
				else
				{
					// if we're transferring the event then stop the event here because
					// we will let the original canexecute bubble up
					e.ContinueRouting = false;
					e.Handled = true;
				}
			}
		}
		#endregion //TransferCanExecute

		// AS 6/23/11 TFS73499
		#region VerifyFloatingWindowVisibility
		private void VerifyFloatingWindowVisibility()
		{
			object visibility;

			if (!this.AllowToolWindowLoaded)
			{
				// if windows aren't allowed to be shown then hide them all
				visibility = KnownBoxes.VisibilityCollapsedBox;
			}
			else
			{
				switch (this.FloatingWindowVisibility)
				{
					case FloatingWindowVisibility.Collapsed:
						visibility = KnownBoxes.VisibilityCollapsedBox;
						break;
					case FloatingWindowVisibility.Hidden:
						visibility = KnownBoxes.VisibilityHiddenBox;
						break;
					default:
					case FloatingWindowVisibility.Visible:
						visibility = KnownBoxes.VisibilityVisibleBox;
						break;
					case FloatingWindowVisibility.SyncWithDockManagerWindow:
						if (_hwndSourceHelper == null)
							visibility = KnownBoxes.VisibilityVisibleBox;
						else
							visibility = _hwndSourceHelper.RootVisualVisibility;
						break;
				}
			}

			Debug.Assert(visibility is Visibility);
			this.SetValue(ComputedFloatingWindowVisibilityPropertyKey, visibility);

			// now make sure that the IsWindowLoaded is up to date which may cause the 
			// toolwindow to be shown
			this.SetIsWindowLoaded(this.AllowToolWindowLoaded);
		}
		#endregion //VerifyFloatingWindowVisibility

		#endregion //Private Methods

		#endregion //Methods

		#region Events

		#region ActivePaneChanged

		/// <summary>
		/// Event ID for the <see cref="ActivePaneChanged"/> routed event
		/// </summary>
		/// <seealso cref="ActivePane"/>
		/// <seealso cref="ActivePaneChanged"/>
		/// <seealso cref="OnActivePaneChanged"/>
		public static readonly RoutedEvent ActivePaneChangedEvent =
			EventManager.RegisterRoutedEvent("ActivePaneChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<ContentPane>), typeof(XamDockManager));

		/// <summary>
		/// Occurs when the <see cref="ActivePane"/> property has been changed
		/// </summary>
		/// <seealso cref="ActivePane"/>
		/// <seealso cref="ActivePaneChanged"/>
		/// <seealso cref="ActivePaneChangedEvent"/>
		protected virtual void OnActivePaneChanged(RoutedPropertyChangedEventArgs<ContentPane> args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseActivePaneChanged(RoutedPropertyChangedEventArgs<ContentPane> args)
		{
			args.RoutedEvent = XamDockManager.ActivePaneChangedEvent;
			args.Source = this;
			this.OnActivePaneChanged(args);
		}

		/// <summary>
		/// Occurs when the <see cref="ActivePane"/> property has been changed
		/// </summary>
		/// <seealso cref="OnActivePaneChanged"/>
		/// <seealso cref="ActivePaneChangedEvent"/>
		//[Description("Occurs when the 'ActivePane' property has been changed")]
		//[Category("DockManager Events")] // Behavior
		public event RoutedPropertyChangedEventHandler<ContentPane> ActivePaneChanged
		{
			add
			{
				base.AddHandler(XamDockManager.ActivePaneChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamDockManager.ActivePaneChangedEvent, value);
			}
		}

		#endregion //ActivePaneChanged

		#region ExecutingCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutingCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		public static readonly RoutedEvent ExecutingCommandEvent =
			EventManager.RegisterRoutedEvent("ExecutingCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutingCommandEventArgs>), typeof(XamDockManager));

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
			args.RoutedEvent = XamDockManager.ExecutingCommandEvent;
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
				base.AddHandler(XamDockManager.ExecutingCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamDockManager.ExecutingCommandEvent, value);
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
			EventManager.RegisterRoutedEvent("ExecutedCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutedCommandEventArgs>), typeof(XamDockManager));

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
			args.RoutedEvent = XamDockManager.ExecutedCommandEvent;
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
				base.AddHandler(XamDockManager.ExecutedCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamDockManager.ExecutedCommandEvent, value);
			}
		}

		#endregion //ExecutedCommand

		// AS 12/15/11 TFS98010
		#region FlyoutOpening

		// kept internal since the event is internal
		internal virtual void OnFlyoutOpening(EventArgs e)
		{
			var handler = this.FlyoutOpening;

			if (null != handler)
				handler(this, e);
		}

		internal void RaiseFlyoutOpening(EventArgs e)
		{
			this.OnFlyoutOpening(e);
		}

		/// <summary>
		/// Invoked when the <see cref="UnpinnedTabFlyout"/> is about to be opened.
		/// </summary>
		internal event EventHandler FlyoutOpening;  //FlyoutOpening

		#endregion // FlyoutOpening

		#region InitializePaneContent

		/// <summary>
		/// Event ID for the <see cref="InitializePaneContent"/> routed event
		/// </summary>
		/// <seealso cref="InitializePaneContent"/>
		/// <seealso cref="OnInitializePaneContent"/>
		/// <seealso cref="InitializePaneContentEventArgs"/>
		public static readonly RoutedEvent InitializePaneContentEvent =
			EventManager.RegisterRoutedEvent("InitializePaneContent", RoutingStrategy.Bubble, typeof(EventHandler<InitializePaneContentEventArgs>), typeof(XamDockManager));

		/// <summary>
		/// Occurs during a call to <see cref="LoadLayout(Stream)"/> when a content pane is referenced in the layout but does not exist within the <see cref="XamDockManager"/>.
		/// </summary>
		/// <seealso cref="InitializePaneContent"/>
		/// <seealso cref="InitializePaneContentEvent"/>
		/// <seealso cref="InitializePaneContentEventArgs"/>
		protected virtual void OnInitializePaneContent(InitializePaneContentEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseInitializePaneContent(InitializePaneContentEventArgs args)
		{
			args.RoutedEvent = XamDockManager.InitializePaneContentEvent;
			args.Source = this;
			this.OnInitializePaneContent(args);
		}

		/// <summary>
		/// Occurs during a call to <see cref="LoadLayout(Stream)"/> when a content pane is referenced in the layout but does not exist within the <see cref="XamDockManager"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The InitializePaneContent event is raised when a layout is loaded that references a 
		/// <see cref="ContentPane"/> that does not exist within the <see cref="XamDockManager"/> when the layout 
		/// is being loaded. The <see cref="ContentControl.Content"/> property of the <see cref="InitializePaneContentEventArgs.NewPane"/> 
		/// must be set. If it is not set then the pane will be removed from the layout. The <see cref="ContentPane.SerializationId"/> 
		/// may be used to store information in the layout that may be used to determine how to initialize the content of the pane. For 
		/// example, the SerializationId could be set to the name of the file that was loaded when the layout was saved.</p>
        /// <p class="note">The <see cref="InitializePaneContentEventArgs.NewPane"/> may also be set to a new <see cref="ContentPane"/>. If set to 
        /// a new pane, the pane will always be used in the layout.</p>
		/// </remarks>
		/// <seealso cref="OnInitializePaneContent"/>
		/// <seealso cref="InitializePaneContentEvent"/>
		/// <seealso cref="InitializePaneContentEventArgs"/>
		//[Description("Occurs during a call to 'LoadLayout' when a content pane is referenced in the layout but does not exist within the 'XamDockManager'.")]
		//[Category("DockManager Events")] // Behavior
		public event EventHandler<InitializePaneContentEventArgs> InitializePaneContent
		{
			add
			{
				base.AddHandler(XamDockManager.InitializePaneContentEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamDockManager.InitializePaneContentEvent, value);
			}
		}

		#endregion //InitializePaneContent

		#region PaneDragEnded

		/// <summary>
		/// Event ID for the <see cref="PaneDragEnded"/> routed event
		/// </summary>
		/// <seealso cref="PaneDragEnded"/>
		/// <seealso cref="OnPaneDragEnded"/>
		/// <seealso cref="PaneDragEndedEventArgs"/>
		public static readonly RoutedEvent PaneDragEndedEvent =
			EventManager.RegisterRoutedEvent("PaneDragEnded", RoutingStrategy.Bubble, typeof(EventHandler<PaneDragEndedEventArgs>), typeof(XamDockManager));

		/// <summary>
		/// Occurs when a pane drag operation has ended.
		/// </summary>
		/// <seealso cref="PaneDragEnded"/>
		/// <seealso cref="PaneDragEndedEvent"/>
		/// <seealso cref="PaneDragEndedEventArgs"/>
		protected virtual void OnPaneDragEnded(PaneDragEndedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaisePaneDragEnded(PaneDragEndedEventArgs args)
		{
			args.RoutedEvent = XamDockManager.PaneDragEndedEvent;
			args.Source = this;
			this.OnPaneDragEnded(args);
		}

		/// <summary>
		/// Occurs when a pane drag operation has ended.
		/// </summary>
		/// <seealso cref="OnPaneDragEnded"/>
		/// <seealso cref="PaneDragEndedEvent"/>
		/// <seealso cref="PaneDragEndedEventArgs"/>
		//[Description("Occurs when a pane drag operation has ended.")]
		//[Category("DockManager Events")] // Behavior
		public event EventHandler<PaneDragEndedEventArgs> PaneDragEnded
		{
			add
			{
				base.AddHandler(XamDockManager.PaneDragEndedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamDockManager.PaneDragEndedEvent, value);
			}
		}

		#endregion //PaneDragEnded

		#region PaneDragOver

		/// <summary>
		/// Event ID for the <see cref="PaneDragOver"/> routed event
		/// </summary>
		/// <seealso cref="PaneDragOver"/>
		/// <seealso cref="OnPaneDragOver"/>
		/// <seealso cref="PaneDragOverEventArgs"/>
		public static readonly RoutedEvent PaneDragOverEvent =
			EventManager.RegisterRoutedEvent("PaneDragOver", RoutingStrategy.Bubble, typeof(EventHandler<PaneDragOverEventArgs>), typeof(XamDockManager));

		/// <summary>
		/// Occurs during a pane drag operation when a new drop location is encountered
		/// </summary>
		/// <seealso cref="PaneDragOver"/>
		/// <seealso cref="PaneDragOverEvent"/>
		/// <seealso cref="PaneDragOverEventArgs"/>
		protected virtual void OnPaneDragOver(PaneDragOverEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaisePaneDragOver(PaneDragOverEventArgs args)
		{
			args.RoutedEvent = XamDockManager.PaneDragOverEvent;
			args.Source = this;
			this.OnPaneDragOver(args);
		}

		/// <summary>
		/// Occurs during a pane drag operation when a new drop location is encountered
		/// </summary>
		/// <seealso cref="OnPaneDragOver"/>
		/// <seealso cref="PaneDragOverEvent"/>
		/// <seealso cref="PaneDragOverEventArgs"/>
		//[Description("Occurs during a pane drag operation when a new drop location is encountered")]
		//[Category("DockManager Events")] // Behavior
		public event EventHandler<PaneDragOverEventArgs> PaneDragOver
		{
			add
			{
				base.AddHandler(XamDockManager.PaneDragOverEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamDockManager.PaneDragOverEvent, value);
			}
		}

		#endregion //PaneDragOver

		// AS 10/13/11 TFS91945
		#region PageDragOverInternal

		internal void OnPageDragOverInternal(PaneDragOverEventArgs args)
		{
			var handler = this.PaneDragOverInternal;

			if (null != handler)
				handler(this, args);
		}

		internal bool HasPaneDragOverInternalListeners
		{
			get { return this.PaneDragOverInternal != null; }
		}

		internal event EventHandler<PaneDragOverEventArgs> PaneDragOverInternal;

		#endregion //PageDragOverInternal

		#region PaneDragStarting

		/// <summary>
		/// Event ID for the <see cref="PaneDragStarting"/> routed event
		/// </summary>
		/// <seealso cref="PaneDragStarting"/>
		/// <seealso cref="OnPaneDragStarting"/>
		/// <seealso cref="PaneDragStartingEventArgs"/>
		public static readonly RoutedEvent PaneDragStartingEvent =
			EventManager.RegisterRoutedEvent("PaneDragStarting", RoutingStrategy.Bubble, typeof(EventHandler<PaneDragStartingEventArgs>), typeof(XamDockManager));

		/// <summary>
		/// Occurs when one or more <see cref="ContentPane"/> instances are being dragged
		/// </summary>
		/// <seealso cref="PaneDragStarting"/>
		/// <seealso cref="PaneDragStartingEvent"/>
		/// <seealso cref="PaneDragStartingEventArgs"/>
		protected virtual void OnPaneDragStarting(PaneDragStartingEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaisePaneDragStarting(PaneDragStartingEventArgs args)
		{
			args.RoutedEvent = XamDockManager.PaneDragStartingEvent;
			args.Source = this;
			this.OnPaneDragStarting(args);
		}

		/// <summary>
		/// Occurs when one or more <see cref="ContentPane"/> instances are being dragged
		/// </summary>
		/// <seealso cref="OnPaneDragStarting"/>
		/// <seealso cref="PaneDragStartingEvent"/>
		/// <seealso cref="PaneDragStartingEventArgs"/>
		//[Description("Occurs when one or more 'ContentPane' instances are being dragged")]
		//[Category("DockManager Events")] // Behavior
		public event EventHandler<PaneDragStartingEventArgs> PaneDragStarting
		{
			add
			{
				base.AddHandler(XamDockManager.PaneDragStartingEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamDockManager.PaneDragStartingEvent, value);
			}
		}

		#endregion //PaneDragStarting

		#region ToolWindowLoaded

		/// <summary>
		/// Event ID for the <see cref="ToolWindowLoaded"/> routed event
		/// </summary>
		/// <seealso cref="ToolWindowLoaded"/>
		/// <seealso cref="OnToolWindowLoaded"/>
		/// <seealso cref="PaneToolWindowEventArgs"/>
		public static readonly RoutedEvent ToolWindowLoadedEvent =
			EventManager.RegisterRoutedEvent("ToolWindowLoaded", RoutingStrategy.Bubble, typeof(EventHandler<PaneToolWindowEventArgs>), typeof(XamDockManager));

		/// <summary>
		/// Occurs when a <see cref="PaneToolWindow"/> is created and about to be shown for the first time.
		/// </summary>
		/// <seealso cref="ToolWindowLoaded"/>
		/// <seealso cref="ToolWindowLoadedEvent"/>
		/// <seealso cref="PaneToolWindowEventArgs"/>
		protected virtual void OnToolWindowLoaded(PaneToolWindowEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseToolWindowLoaded(PaneToolWindowEventArgs args)
		{
			args.RoutedEvent = XamDockManager.ToolWindowLoadedEvent;
			args.Source = this;
			this.OnToolWindowLoaded(args);
		}

		/// <summary>
		/// Occurs when a <see cref="PaneToolWindow"/> is created and about to be shown for the first time.
		/// </summary>
		/// <remarks>
		/// <p class="body">This event is fired when a new <see cref="PaneToolWindow"/> is created and is about to be displayed. This event will 
		/// not be raised if the window is hidden and then reshown.</p>
		/// </remarks>
		/// <seealso cref="OnToolWindowLoaded"/>
		/// <seealso cref="ToolWindowLoadedEvent"/>
		/// <seealso cref="PaneToolWindowEventArgs"/>
		//[Description("Occurs when a 'PaneToolWindow' is created and about to be shown for the first time.")]
		//[Category("DockManager Events")] // Behavior
		public event EventHandler<PaneToolWindowEventArgs> ToolWindowLoaded
		{
			add
			{
				base.AddHandler(XamDockManager.ToolWindowLoadedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamDockManager.ToolWindowLoadedEvent, value);
			}
		}

		#endregion //ToolWindowLoaded

		#region ToolWindowUnloaded

		/// <summary>
		/// Event ID for the <see cref="ToolWindowUnloaded"/> routed event
		/// </summary>
		/// <seealso cref="ToolWindowUnloaded"/>
		/// <seealso cref="OnToolWindowUnloaded"/>
		/// <seealso cref="PaneToolWindowEventArgs"/>
		public static readonly RoutedEvent ToolWindowUnloadedEvent =
			EventManager.RegisterRoutedEvent("ToolWindowUnloaded", RoutingStrategy.Bubble, typeof(EventHandler<PaneToolWindowEventArgs>), typeof(XamDockManager));

		/// <summary>
		/// Occurs when a <see cref="PaneToolWindow"/> is created and about to be shown for the first time.
		/// </summary>
		/// <seealso cref="ToolWindowUnloaded"/>
		/// <seealso cref="ToolWindowUnloadedEvent"/>
		/// <seealso cref="PaneToolWindowEventArgs"/>
		protected virtual void OnToolWindowUnloaded(PaneToolWindowEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseToolWindowUnloaded(PaneToolWindowEventArgs args)
		{
			args.RoutedEvent = XamDockManager.ToolWindowUnloadedEvent;
			args.Source = this;
			this.OnToolWindowUnloaded(args);
		}

		/// <summary>
		/// Occurs when a <see cref="PaneToolWindow"/> is about to be removed from the <see cref="XamDockManager"/>
		/// </summary>
		/// <remarks>
		/// <p class="body">The event is fired when all the panes have been removed from the <see cref="PaneToolWindow"/> and the window is about 
		/// to be removed from the XamDockManager. It is not fired just because the tool window is hidden because all of the panes it contains have been hidden.</p>
		/// </remarks>
		/// <seealso cref="OnToolWindowUnloaded"/>
		/// <seealso cref="ToolWindowUnloadedEvent"/>
		/// <seealso cref="PaneToolWindowEventArgs"/>
		//[Description("Occurs when a 'PaneToolWindow' is about to be removed from the 'XamDockManager'.")]
		//[Category("DockManager Events")] // Behavior
		public event EventHandler<PaneToolWindowEventArgs> ToolWindowUnloaded
		{
			add
			{
				base.AddHandler(XamDockManager.ToolWindowUnloadedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamDockManager.ToolWindowUnloadedEvent, value);
			}
		}

		#endregion //ToolWindowUnloaded

		#endregion //Events

		#region ICommandHost Members

		bool ICommandHost.CanExecute(ExecuteCommandInfo commandInfo)
		{
			RoutedCommand command = commandInfo.RoutedCommand;

			if (null != command)
			{
				// AS 9/10/09 TFS19267
				// Since the XDM needs to transfer execution of a command to a nested control because of 
				// how WPF handles routing commands, the inner XDM was getting the canexecute for the 
				// showpanenavigator command (in this case raised by clicking the button within the outer 
				// xdm). Since there are no state requirements on the command and no way to identify the 
				// source of the canexecute, the inner xdm returned true that it could execute the command.
				// Since the CanExecute returned true, the outer xdm tried to executed the command to the 
				// inner element. The customer was handling the ExecutingCommand to prevent the inner xdm from 
				// showing its pane navigator and so the execute did nothing. So we needed a way to identify 
				// the source for the command request so when we use the DockManagerCommands, we'll use a 
				// parameter of the XDM. In this way the inner xdm can ignore the request to execute for 
				// the outer xdm's raising of the showpanenavigator.
				//
				if (commandInfo.Parameter is XamDockManager && commandInfo.Parameter != this)
					return false;

                // AS 2/12/09 TFS12819
                if (command.OwnerType == typeof(DockManagerCommands))
                    commandInfo.ForceHandled = true;

				if (command == DockManagerCommands.ActivateNextDocument ||
					command == DockManagerCommands.ActivatePreviousDocument)
					return this._activePaneManager.CanNavigateDocuments;

				if (command == DockManagerCommands.ActivateNextPane ||
					command == DockManagerCommands.ActivatePreviousPane)
					return this._activePaneManager.CanNavigatePanes;

				if (command == DockManagerCommands.CloseActiveDocument)
				{
					ContentPane activeDocument = this.ActivePaneManager.GetActiveDocument();

					return null != activeDocument && activeDocument.AllowClose;
				}

				if (command == DockManagerCommands.ShowPaneNavigator)
				{
					return true;
				}
			}

			return false;
		}

		// SSP 3/18/10 TFS29783 - Optimizations
		// 
		//long ICommandHost.CurrentState
		long ICommandHost.GetCurrentState( long statesToQuery )
		{
			DockManagerStates state = 0;

			if ( this.ActivePane != null )
				state |= DockManagerStates.ActivePane;

			if ( this.HasDocumentContentHost )
			{
				state |= DockManagerStates.HasDocumentContentHost;

				if ( null != this.ActivePaneManager.GetActiveDocument( ) )
					state |= DockManagerStates.ActiveDocument;
			}

			return (long)state & statesToQuery;
		}

		bool ICommandHost.Execute(ExecuteCommandInfo commandInfo)
		{
			RoutedCommand command = commandInfo.RoutedCommand;
			return null != command && this.ExecuteCommandImpl(commandInfo);
		}

		#endregion

		#region IPaneContainer Members

		IList IPaneContainer.Panes
		{
			get { return this._rootPaneList; }
		}

		bool IPaneContainer.RemovePane(object pane)
		{
			if (pane is PaneToolWindow)
			{
				PaneToolWindow window = (PaneToolWindow)pane;

				int index = this._toolWindows.IndexOf(window);

				Debug.Assert(window.Content == null || this.Panes.Contains((SplitPane)window.Content) == false);
				Debug.Assert(index < 0);
				return false;
			}
			else if (pane is SplitPane)
			{
				SplitPane split = (SplitPane)pane;
				int index = this.Panes.IndexOf(split);

				if (index < 0)
					return false;

				this.Panes.RemoveAt(index);

				return true;
			}
			else
			{
				Debug.Fail("Unexpected pane!");
				return false;
			}
		}

		bool IPaneContainer.CanBeRemoved
		{
			get { return false; }
		}

		#endregion //IPaneContainer Members

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region IHwndHostContainer Members

        bool IHwndHostContainer.HasHwndHost
        {
            get { return this.HasHwndHost; }
        }

        IEnumerable<HwndHost> IHwndHostContainer.GetHosts()
        {
			
#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)

			foreach (HwndHostInfo hhi in GetHwndHostInfos())
			{
				foreach (HwndHost host in hhi.GetHosts())
					yield return host;
			}
        }

        #endregion

		// AS 9/8/09 TFS21746
		#region IHwndHostInfoOwner Members

		void IHwndHostInfoOwner.OnHasHostsChanged()
		{
			this.DirtyHasHwndHosts();
		}

		#endregion //IHwndHostInfoOwner

		#region PanesCollection class
		internal class PanesCollection : ObservableCollectionExtended<SplitPane>
		{
			#region Member Variables

			private QueuedObservableCollection<SplitPane> _dockedPanes;
			private QueuedObservableCollection<SplitPane> _floatingPanes;
			private XamDockManager _dockManager;
			private ReadOnlyObservableCollection<SplitPane> _readOnlyDockedPanes;
			private ReadOnlyObservableCollection<SplitPane> _readOnlyFloatingPanes;

			#endregion //Member Variables

			#region Constructor
			internal PanesCollection(XamDockManager dockManager)
			{
				this._dockManager = dockManager;

				this._dockedPanes = new QueuedObservableCollection<SplitPane>();
				this._dockedPanes.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnDockedPanesChanged);
				this._floatingPanes = new QueuedObservableCollection<SplitPane>();
				this._floatingPanes.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnFloatingPanesChanged);

				this._readOnlyDockedPanes = new ReadOnlyObservableCollection<SplitPane>(this._dockedPanes);
				this._readOnlyFloatingPanes = new ReadOnlyObservableCollection<SplitPane>(this._floatingPanes);
			}
			#endregion //Constructor

			#region Base class overrides

			#region OnCollectionChanged
			protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
			{
				// process the changes whether they be reordering, adding panes or removing
				SplitPane[] floatingPanes = FindAll(this, new Predicate<SplitPane>(IsFloating));
				SplitPane[] dockedPanes = FindAll(this, new Predicate<SplitPane>(IsDocked));

				this._dockedPanes.ReInitialize(dockedPanes);
				this._floatingPanes.ReInitialize(floatingPanes);

				base.OnCollectionChanged(e);
			}
			#endregion //OnCollectionChanged

			#endregion //Base class overrides

			#region Properties

			public ReadOnlyObservableCollection<SplitPane> DockedPanes
			{
				get { return this._readOnlyDockedPanes; }
			}

			public ReadOnlyObservableCollection<SplitPane> FloatingPanes
			{
				get { return this._readOnlyFloatingPanes; }
			}
			
			#endregion //Properties

			#region Methods

			#region IsFloating
			private static bool IsFloating(SplitPane pane)
			{
				InitialPaneLocation location = XamDockManager.GetInitialLocation(pane);

				return location == InitialPaneLocation.DockableFloating ||
					location == InitialPaneLocation.FloatingOnly;
			}
			#endregion //IsFloating

			#region IsDocked
			private static bool IsDocked(SplitPane pane)
			{
				return IsFloating(pane) == false;
			}
			#endregion //IsDocked

			#region FindAll<T>
			private static T[] FindAll<T>(IList<T> list, Predicate<T> match)
			{
				List<T> matches = new List<T>();

				for (int i = 0, count = list.Count; i < count; i++)
				{
					T item = list[i];

					if (match(item))
						matches.Add(item);
				}

				return matches.ToArray();
			}
			#endregion //FindAll<T>

			#region OnDockedPanesChanged
			void OnDockedPanesChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				Debug.Assert(e.Action == NotifyCollectionChangedAction.Reset);
				Debug.Assert(e is QueuedObservableCollection<SplitPane>.QueuedNotifyCollectionChangedEventArgs);

				QueuedObservableCollection<SplitPane>.QueuedNotifyCollectionChangedEventArgs args = e as QueuedObservableCollection<SplitPane>.QueuedNotifyCollectionChangedEventArgs;

				// deal with items added/removed
				for (int i = 0, count = args.ItemsAdded.Count; i < count; i++)
				{
					SplitPane pane = args.ItemsAdded[i];

					if (null != pane)
					{
						PaneLocation paneLocation = DockManagerUtilities.ToPaneLocation(XamDockManager.GetInitialLocation(pane));
						pane.SetValue(XamDockManager.PaneLocationPropertyKey, DockManagerKnownBoxes.FromValue(paneLocation));

						// include the docked panes in the logical tree of the dockmanager
						// AS 5/9/08
						//this._dockManager.AddLogicalChild(pane);
						this._dockManager.AddLogicalChildInternal(pane);
					}
				}

				for (int i = 0, count = args.ItemsRemoved.Count; i < count; i++)
				{
					SplitPane pane = args.ItemsRemoved[i];

					if (null != pane)
					{
						// include the docked panes in the logical tree of the dockmanager
						// AS 5/9/08
						//this._dockManager.RemoveLogicalChild(pane);
						this._dockManager.RemoveLogicalChildInternal(pane);

						pane.ClearValue(XamDockManager.PaneLocationPropertyKey);
					}
				}

				if (this._dockManager._dockPanel != null)
					this._dockManager._dockPanel.Panes.ReInitialize(this._dockedPanes);

				// AS 10/5/09 NA 2010.1 - LayoutMode
				this._dockManager.VerifyFillPane();
			}
			#endregion //OnDockedPanesChanged

			#region OnFloatingPanesChanged
			void OnFloatingPanesChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				Debug.Assert(e.Action == NotifyCollectionChangedAction.Reset);
				Debug.Assert(e is QueuedObservableCollection<SplitPane>.QueuedNotifyCollectionChangedEventArgs);

				QueuedObservableCollection<SplitPane>.QueuedNotifyCollectionChangedEventArgs args = e as QueuedObservableCollection<SplitPane>.QueuedNotifyCollectionChangedEventArgs;

				// deal with items added/removed
				for (int i = 0, count = args.ItemsAdded.Count; i < count; i++)
				{
					SplitPane pane = args.ItemsAdded[i];

					if (null != pane)
					{
						
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

						this._dockManager.OnFloatingPaneAdded(pane);
					}
				}

				for (int i = 0, count = args.ItemsRemoved.Count; i < count; i++)
				{
					SplitPane pane = args.ItemsRemoved[i];

					if (null != pane)
					{
						
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

						this._dockManager.OnFloatingPaneRemoved(pane);
					}
				}

			}
			#endregion //OnFloatingPanesChanged

			#endregion //Methods
		} 
		#endregion //PanesCollection class

		#region RootPaneList class
		/// <summary>
		/// Helper class for managing the list of root level <see cref="IPaneContainer"/> instances
		/// </summary>
		private class RootPaneList : IList
		{
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			#region Member Variables

			private XamDockManager _dockManager;
			private List<UnpinnedTabArea> _unpinnedTabAreas;

			#endregion //Member Variables

			#region Constructor
			internal RootPaneList(XamDockManager dockManager)
			{
				this._dockManager = dockManager;
				this._unpinnedTabAreas = new List<UnpinnedTabArea>(4);

				this.RefreshTabAreas();
			}
			#endregion //Constructor

			#region Properties

			#region DockedPanes
			private IList DockedPanes
			{
				get { return ((PanesCollection)this._dockManager.Panes).DockedPanes; }
			}
			#endregion //DockedPanes

			#region DocumentHost
			private DocumentContentHost DocumentHost
			{
				get
				{
					return this._dockManager.HasDocumentContentHost
						? (DocumentContentHost)this._dockManager.Content
						: null;
				}
			}
			#endregion //DocumentHost

			#region ToolWindows
			private IList ToolWindows
			{
				get { return this._dockManager._toolWindows; }
			}
			#endregion //ToolWindows

			#region UnpinnedTabAreas
			private IList UnpinnedTabAreas
			{
				get { return this._unpinnedTabAreas; }
			}
			#endregion //UnpinnedTabAreas

			#endregion //Properties

			#region Methods
			internal void RefreshTabAreas()
			{
				this._unpinnedTabAreas.Clear();

				foreach (UnpinnedTabAreaInfo tabAreaInfo in this._dockManager._tabAreaInfos)
				{
					if (null != tabAreaInfo.TabArea)
						this._unpinnedTabAreas.Add(tabAreaInfo.TabArea);
				}
			}
			#endregion //Methods

			#region IList Members

			public int Add(object value)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains(object value)
			{
				return this.IndexOf(value) >= 0;
			}

			public int IndexOf(object value)
			{
				// keep this in sync with the GetEnumerator, CopyTo and indexer
				int index = 0;
				int tempIndex = -1;

				// 1 - unpinned
				if (value is UnpinnedTabArea)
					tempIndex = this.UnpinnedTabAreas.IndexOf(value);

				if (tempIndex < 0)
				{
					index += this.UnpinnedTabAreas.Count;

					// 2 - docked panes
					tempIndex = this.DockedPanes.IndexOf(value);

					if (tempIndex < 0)
					{
						index += this.DockedPanes.Count;

						// 3 - documenthost
						if (this.DocumentHost != null)
						{
							if (value is DocumentContentHost)
								tempIndex = 0;
							else
								index++;
						}

						// 4 - floating
						if (tempIndex < 0)
							tempIndex = this.ToolWindows.IndexOf(value);
					}
				}

				if (tempIndex >= 0)
					tempIndex += index;

				return tempIndex;
			}

			public void Insert(int index, object value)
			{
				throw new NotSupportedException();
			}

			public bool IsFixedSize
			{
				get { return false; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public void Remove(object value)
			{
				throw new NotSupportedException();
			}

			public void RemoveAt(int index)
			{
				throw new NotSupportedException();
			}

			public object this[int index]
			{
				get
				{
					// keep this in sync with the IndexOf, CopyTo and GetEnumerator

					// 1 - unpinned
					if (index < this._unpinnedTabAreas.Count)
						return this._unpinnedTabAreas[index];

					index -= this._unpinnedTabAreas.Count;

					// 2 - docked
					if (index < this.DockedPanes.Count)
						return this.DockedPanes[index];

					index -= this.DockedPanes.Count;

					// 3 - documenthost
					if (this.DocumentHost != null)
					{
						if (index == 0)
							return this.DocumentHost;

						index--;
					}

					Debug.Assert(index < this._dockManager._toolWindows.Count);

					// 4 - floating
					return this._dockManager._toolWindows[index];
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			#endregion

			#region ICollection Members

			public void CopyTo(Array array, int index)
			{
				DockManagerUtilities.ThrowIfNull(array, "array");

				if (index < 0)
					throw new IndexOutOfRangeException();

				// keep this in sync with the IndexOf, CopyTo and GetEnumerator

				// 1 - unpinned
				this.UnpinnedTabAreas.CopyTo(array, index);
				index += this._unpinnedTabAreas.Count;

				// 2 - docked
				this.DockedPanes.CopyTo(array, index);
				index += this.DockedPanes.Count;

				// 3 - documenthost
				if (null != this.DocumentHost)
				{
					array.SetValue(this.DocumentHost, index);
					index++;
				}

				// 4 - floating
				this.ToolWindows.CopyTo(array, index);
			}

			public int Count
			{
				get 
				{ 
					int count = this.UnpinnedTabAreas.Count + this.DockedPanes.Count + this.ToolWindows.Count;

					if (this.DocumentHost != null)
						count++;

					return count;
				}
			}

			public bool IsSynchronized
			{
				get { return false; }
			}

			public object SyncRoot
			{
				get { return typeof(RootPaneList); }
			}

			#endregion

			#region IEnumerable Members

			public IEnumerator GetEnumerator()
			{
				return new MultiSourceEnumerator(
					// keep this in sync with the IndexOf, CopyTo and indexer
					this.UnpinnedTabAreas.GetEnumerator(),			// 1 - unpinned
					this.DockedPanes.GetEnumerator(),				// 2 - docked
					new SingleItemEnumerator(this.DocumentHost),	// 3 - documenthost
					this.ToolWindows.GetEnumerator());				// 4 - floating
			}

			#endregion
		}
		#endregion //RootPaneList class

		// AS 6/23/11 TFS73499
		#region HwndSourceHelper class
		private class HwndSourceHelper : DependencyObject
		{
			#region Member Variables

			private const Visibility DefaultRootVisualVisibility = Visibility.Collapsed;
			private WeakReference _element;
			private PresentationSource _currentSource;
			private Visibility _cachedRootVisualVisibility = DefaultRootVisualVisibility;
			private WindowState? _cachedRootVisualWindowState; // AS 2/21/12 TFS99925

			#endregion //Member Variables

			#region Constructor
			internal HwndSourceHelper(XamDockManager element)
			{
				_element = new WeakReference(element);
				// AS 8/22/11 TFS84326
				// Do not hook the event until after the FromVisual call. Otherwise we'll hook the event 
				// and get an exception when the OnSourceChanged is invoked because we won't have the rights 
				// to access the NewSource.
				//
				//HwndSource.AddSourceChangedHandler(element, new SourceChangedEventHandler(this.OnSourceChanged));
				this.CurrentSource = HwndSource.FromVisual(element);
				HwndSource.AddSourceChangedHandler(element, new SourceChangedEventHandler(this.OnSourceChanged));
			}
			#endregion //Constructor

			#region Properties

			#region CurrentSource
			public PresentationSource CurrentSource
			{
				get { return _currentSource; }
				private set
				{
					if (value != _currentSource)
					{
						var oldSource = _currentSource;
						_currentSource = value;

						HookUnhookSource(oldSource, false);
						HookUnhookSource(value, true);

						this.UpdateRootVisibilityBinding();
					}
				}
			}
			#endregion //CurrentSource

			#region RootVisualVisibility

			/// <summary>
			/// Identifies the <see cref="RootVisualVisibility"/> dependency property
			/// </summary>
			private static readonly DependencyProperty RootVisualVisibilityProperty = DependencyProperty.Register("RootVisualVisibility",
				typeof(Visibility), typeof(HwndSourceHelper), new FrameworkPropertyMetadata(DefaultRootVisualVisibility, new PropertyChangedCallback(OnRootVisualVisibilityChanged)));

			private static void OnRootVisualVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				var instance = d as HwndSourceHelper;

				instance.RootVisualVisibility = (Visibility)e.NewValue;
			}

			/// <summary>
			/// Returns or sets the visibility of the root visual
			/// </summary>
			/// <seealso cref="RootVisualVisibilityProperty"/>
			public Visibility RootVisualVisibility
			{
				get
				{
					return _cachedRootVisualVisibility;
				}
				private set
				{
					if (value != _cachedRootVisualVisibility)
					{
						_cachedRootVisualVisibility = value;

						var dm = Utilities.GetWeakReferenceTargetSafe(_element) as XamDockManager;

						if (null != dm)
							dm.OnRootVisualVisibilityChanged();
					}
				}
			}

			#endregion //RootVisualVisibility

			// AS 2/21/12 TFS99925
			#region RootVisualWindowState

			/// <summary>
			/// Identifies the <see cref="RootVisualWindowState"/> dependency property
			/// </summary>
			private static readonly DependencyProperty RootVisualWindowStateProperty = DependencyProperty.Register("RootVisualWindowState",
				typeof(WindowState?), typeof(HwndSourceHelper), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnRootVisualWindowStateChanged)));

			private static void OnRootVisualWindowStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				var instance = d as HwndSourceHelper;

				instance.RootVisualWindowState = (WindowState?)e.NewValue;
			}

			/// <summary>
			/// Returns or sets the WindowState of the root visual
			/// </summary>
			/// <seealso cref="RootVisualWindowStateProperty"/>
			public WindowState? RootVisualWindowState
			{
				get
				{
					return _cachedRootVisualWindowState;
				}
				private set
				{
					if (value != _cachedRootVisualWindowState)
					{
						_cachedRootVisualWindowState = value;

						var dm = Utilities.GetWeakReferenceTargetSafe(_element) as XamDockManager;

						if (null != dm)
							dm.OnRootVisualVisibilityChanged();
					}
				}
			}

			#endregion //RootVisualWindowState

			#endregion //Properties

			#region Methods

			#region HookUnhookSource
			private void HookUnhookSource(PresentationSource ps, bool hook)
			{
				if (ps == null)
					return;

				HwndSource hs = ps as HwndSource;

				if (hook)
				{
					ps.ContentRendered += new EventHandler(OnPresentationSourceContentRendered);

					if (null != hs)
					{
						hs.Disposed += new EventHandler(OnHwndSourceDisposed);
					}
				}
				else
				{
					ps.ContentRendered -= new EventHandler(OnPresentationSourceContentRendered);

					if (null != hs)
					{
						hs.Disposed -= new EventHandler(OnHwndSourceDisposed);
					}
				}
			}
			#endregion //HookUnhookSource

			#region OnHwndSourceDisposed
			private void OnHwndSourceDisposed(object sender, EventArgs e)
			{
				// todo - do we need to do anything here?
			}
			#endregion //OnHwndSourceDisposed

			#region OnPresentationSourceContentRendered
			private void OnPresentationSourceContentRendered(object sender, EventArgs e)
			{
				// if the RootVisual changes the ContentRendered will be invoked again
				this.UpdateRootVisibilityBinding();
			}
			#endregion //OnPresentationSourceContentRendered

			#region OnSourceChanged
			private void OnSourceChanged(object sender, SourceChangedEventArgs e)
			{
				this.CurrentSource = e.NewSource;
			}
			#endregion //OnSourceChanged

			#region UpdateRootVisibilityBinding
			private void UpdateRootVisibilityBinding()
			{
				var rootVisual = _currentSource != null ? _currentSource.RootVisual : null;

				if (rootVisual == null)
				{
					BindingOperations.ClearBinding(this, RootVisualVisibilityProperty);
					this.RootVisualVisibility = DefaultRootVisualVisibility;
				}
				else
				{
					BindingOperations.SetBinding(this, RootVisualVisibilityProperty, Utilities.CreateBindingObject(UIElement.VisibilityProperty, BindingMode.OneWay, rootVisual));
					this.RootVisualVisibility = (Visibility)rootVisual.GetValue(UIElement.VisibilityProperty);
				}

				// AS 2/21/12 TFS99925
				Window window = rootVisual as Window;

				if (null == window)
				{
					BindingOperations.ClearBinding(this, RootVisualWindowStateProperty);
					this.RootVisualWindowState = null;
				}
				else
				{
					BindingOperations.SetBinding(this, RootVisualWindowStateProperty, Utilities.CreateBindingObject(Window.WindowStateProperty, BindingMode.OneWay, rootVisual));
					this.RootVisualWindowState = (WindowState)rootVisual.GetValue(Window.WindowStateProperty);
				}

			}
			#endregion //UpdateRootVisibilityBinding

			#endregion //Methods
		} 
		#endregion //HwndSourceHelper class
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