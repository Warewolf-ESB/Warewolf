//#define DEBUG_ACTIVATION




using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Windows.Data;
using Infragistics.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Interop;
using System.Security.Permissions;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Helper class for managing the active pane
	/// </summary>
	internal class ActivePaneManager : DependencyObject
	{
		
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

		#region Member Variables

		// maintains an ordered list of the panes in the order in which they have been activated
		// regardless of the dockmanager's navigation mode
		private LinkedList<ContentPane> _panes;

		private XamDockManager _dockManager;
		private bool _isInNavigationMode;
		private ContentPane _activePane;
		private ContentPane _activeDocument;
		private int _navigationModeVersion;

		// list of the panes pending removal
		private List<ContentPane> _panesToRemove;

		// AS 7/17/09 TFS18453/TFS19568
		private List<ContentPanePlaceholder> _placeholdersToRemove;

		// focus watchers used to track events of the focused element at various points
		private KeyEventFocusWatcher _navigationModeFocusWatcher;
		private FocusWatcher _activePaneFocusWatcher;

		private int _suspendVerificationCount;

		private WeakReference _lastActivePane;

        // AS 4/17/09 TFS16807
        //// AS 11/25/08 TFS8265
        //private WeakPreprocessMessage _preprocessHandler;
        private IDisposable _preprocessHandler;
        private static bool _canCreatePreprocessHandler = true;

		// AS 9/8/09 TFS21921
		// See SetActivePane(ContentPane) for notes.
		//
		private DispatcherOperation _pendingClearActivePane;

		#endregion //Member Variables

		#region Constructor
		static ActivePaneManager()
		{
			// register event handlers so we can manage the active pane
			EventManager.RegisterClassHandler(typeof(ContentPane), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnContentPaneGotKeyboardFocus), true);
            // AS 3/3/09 TFS10656
            //EventManager.RegisterClassHandler(typeof(ContentPane), Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnContentPaneLostKeyboardFocus), true);

			EventManager.RegisterClassHandler(typeof(ContentPane), FocusManager.GotFocusEvent, new RoutedEventHandler(OnPaneGotOrLostFocus), true);
			EventManager.RegisterClassHandler(typeof(ContentPane), FocusManager.LostFocusEvent, new RoutedEventHandler(OnPaneGotOrLostFocus), true);
		}

		internal ActivePaneManager(XamDockManager dockManager)
		{
			this._dockManager = dockManager;
			this._panes = new LinkedList<ContentPane>();
			this._panesToRemove = new List<ContentPane>();

			// AS 7/17/09 TFS18453/TFS19568
			this._placeholdersToRemove = new List<ContentPanePlaceholder>();

			RoutedEventHandler loadHandler = new RoutedEventHandler(OnDockManagerLoadStateChanged);
			this._dockManager.Loaded += loadHandler;
			this._dockManager.Unloaded += loadHandler;
        }

		#endregion //Constructor

		#region Properties

		#region ActiveDocument

		private static readonly DependencyPropertyKey ActiveDocumentPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("ActiveDocument",
			typeof(ContentPane), typeof(ActivePaneManager), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ActiveDocumentProperty =
			ActiveDocumentPropertyKey.DependencyProperty;

		#endregion //ActiveDocument

		#region ActivePane

		private static readonly DependencyPropertyKey ActivePanePropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("ActivePane",
			typeof(ContentPane), typeof(ActivePaneManager), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ActivePaneProperty =
			ActivePanePropertyKey.DependencyProperty;

		#endregion //ActivePane

		#region CanNavigatePanes
		/// <summary>
		/// Indicates if there are more than one non-document panes in the associated <see cref="DocumentContentHost"/> of the owning <see cref="XamDockManager"/>.
		/// </summary>
		public bool CanNavigatePanes
		{
			get
			{
				// we want to return true if there is at least 1 content pane. otherwise keys like
				// alt-f6 will just shift between owned windows. also, we may not have an active pane
				// so we should in that case activate the last pane that was activated
				return null != GetPaneInActivationOrder(null, true, true, PaneFilterFlags.AllVisibleExceptDocument, true);
			}
		}
		#endregion // CanNavigatePanes

		#region CanNavigateDocuments
		/// <summary>
		/// Indicates if there are more than one documents in the associated <see cref="DocumentContentHost"/> of the owning <see cref="XamDockManager"/>.
		/// </summary>
		public bool CanNavigateDocuments
		{
			get
			{
				ContentPane activePane = this.GetActiveDocument();

				if (null != activePane)
				{
					return null != GetPaneInActivationOrder(activePane, false, true, PaneFilterFlags.Document, true)
						|| activePane != GetPaneInActivationOrder(null, true, true, PaneFilterFlags.Document, true);
				}

				return false;
			}
		}
		#endregion // CanNavigateDocuments

		#region HasActivatablePanes
		/// <summary>
		/// Indicates if there is at least one pane that can be activated.
		/// </summary>
		internal bool HasActivatablePanes
		{
			get { return null != this.GetPaneInActivationOrder(null, true, true, PaneFilterFlags.AllVisible, true); }
		} 
		#endregion //HasActivatablePanes

		#region IsActiveDocument

		internal static readonly DependencyPropertyKey IsActiveDocumentPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsActiveDocument",
			typeof(bool), typeof(ActivePaneManager), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the "IsActiveDocument" attached readonly dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveDocumentProperty =
			IsActiveDocumentPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the value of the 'IsActiveDocument' attached readonly property
		/// </summary>
		/// <seealso cref="IsActiveDocumentProperty"/>
		public static bool GetIsActiveDocument(DependencyObject d)
		{
			return (bool)d.GetValue(ActivePaneManager.IsActiveDocumentProperty);
		}

		#endregion //IsActiveDocument

		#region IsActivePane

		internal static readonly DependencyPropertyKey IsActivePanePropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsActivePane",
			typeof(bool), typeof(ActivePaneManager), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the "IsActivePane" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsActivePane"/>
		public static readonly DependencyProperty IsActivePaneProperty =
			IsActivePanePropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the value of the 'IsActivePane' attached readonly property
		/// </summary>
		/// <seealso cref="IsActivePaneProperty"/>
		public static bool GetIsActivePane(DependencyObject d)
		{
			return (bool)d.GetValue(ActivePaneManager.IsActivePaneProperty);
		}

		#endregion //IsActivePane

		#region IsDockManagerWindowActive

		/// <summary>
		/// Identifies the <see cref="IsDockManagerWindowActive"/> dependency property
		/// </summary>
		private static readonly DependencyProperty IsDockManagerWindowActiveProperty =
			DependencyProperty.Register("IsDockManagerWindowActive",
			typeof(bool), typeof(ActivePaneManager), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsDockManagerWindowActiveChanged)));

		private static void OnIsDockManagerWindowActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Output(e.NewValue, "OnIsDockManagerWindowActiveChanged");

			((ActivePaneManager)d).OnWindowActivationChanged();
		}

		/// <summary>
		/// Returns a boolean indicating whether the window containing the XamDockManager is active.
		/// </summary>
		/// <seealso cref="IsDockManagerWindowActiveProperty"/>
		public bool IsDockManagerWindowActive
		{
			get
			{
				return (bool)this.GetValue(ActivePaneManager.IsDockManagerWindowActiveProperty);
			}
		}

		#endregion //IsDockManagerWindowActive

        // AS 3/3/09 TFS10656
        #region FocusWatcher

        public static readonly DependencyProperty FocusWatcherProperty =
            DependencyProperty.RegisterAttached("FocusWatcher", typeof(FocusWatcher), typeof(ActivePaneManager),
                new FrameworkPropertyMetadata(null));

        #endregion //FocusWatcher

		#region IsKeyboardFocusWithinEx

		private static readonly DependencyPropertyKey IsKeyboardFocusWithinExPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsKeyboardFocusWithinEx",
			typeof(bool), typeof(ActivePaneManager), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsKeyboardFocusWithinExChanged)));

		private static void OnIsKeyboardFocusWithinExChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Debug.Assert(d is ContentPane);

			ContentPane pane = d as ContentPane;
			XamDockManager dockManager = XamDockManager.GetDockManager(pane);

            // AS 3/3/09 TFS10656
            // We were relying on the Keyboard.LostKeyboardFocusEvent for the ContentPane in order to clear 
            // the IsKeyboardFocusWithinEx. The problem is that if the element with keyboard focus is disconnected 
            // while it has focus then we won't get that notification. Instead, we will create a FocusWatcher that 
            // will track the FocusedElement while the IsKeyboardFocusWithinEx is true. When it detects that 
            // keyboard focus has left the pane it will clear this flag at which point we will dispose the 
            // watcher.
            //
            //if (null != dockManager)
            //    dockManager.ActivePaneManager.OnContentPaneStateChanged(pane);
            bool isFocusWithin = true.Equals(e.NewValue) && null != dockManager;
            FocusWatcher watcher = (FocusWatcher)pane.GetValue(FocusWatcherProperty);

            // if we need a watcher and don't have one yet then create one
            if (null == watcher && isFocusWithin && null != dockManager)
            {
                watcher = new PaneFocusWatcher(pane);
                pane.SetValue(FocusWatcherProperty, watcher);
            }
            else if (null != watcher && (null == dockManager  || !isFocusWithin))
            {
                // if we have one but don't need it any more then release it
                watcher.Dispose();
                pane.ClearValue(FocusWatcherProperty);
            }

            if (null != dockManager)
            {
                ActivePaneManager activeManager = dockManager.ActivePaneManager;

                // if we have already shifted focus to another pane then 
                // do not process this change
                if (!isFocusWithin && activeManager._activePane != pane)
                    return;

                activeManager.OnContentPaneStateChanged(pane);
            }
		}

		/// <summary>
		/// Identifies the IsKeyboardFocusWithinEx" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsKeyboardFocusWithinEx"/>
		public static readonly DependencyProperty IsKeyboardFocusWithinExProperty =
			IsKeyboardFocusWithinExPropertyKey.DependencyProperty;


		/// <summary>
		/// Gets the value of the 'IsKeyboardFocusWithinEx' attached readonly property
		/// </summary>
		/// <seealso cref="IsKeyboardFocusWithinExProperty"/>
		public static bool GetIsKeyboardFocusWithinEx(DependencyObject d)
		{
			return (bool)d.GetValue(ActivePaneManager.IsKeyboardFocusWithinExProperty);
		}

		#endregion //IsKeyboardFocusWithinEx

		#region IsInNavigationMode
		/// <summary>
		/// Returns/sets a boolean indicating whether the dockmanager is in a cycle of processing ctrl/tab or alt/tab.
		/// </summary>
		public bool IsInNavigationMode
		{
			get
			{
				return this._isInNavigationMode;
			}
			set
			{
				if (value != this._isInNavigationMode)
				{
					this._isInNavigationMode = value;

					if (value)
					{
						// we keep a version number so classes like TabGroupPane can know when
						// they need to rebuild a snapshot
						this._navigationModeVersion++;

						// added a way to know when the alt/ctrl is released regardless of who has focus
						this.CreateNavigationModeFocusWatcher();
					}
					else
					{
						this.DisposeNavigationModeFocusWatcher();

						// move the active item to the head of the list
						if (null != this._activePane)
							this.MovePaneToHead(this._activePane);
					}
				}
			}
		}

		#endregion //IsInNavigationMode

		#region IsVerificationSuspended
		internal bool IsVerificationSuspended
		{
			get { return this._suspendVerificationCount > 0; }
		} 
		#endregion //IsVerificationSuspended

		#region NavigationModeVersion
		/// <summary>
		/// Returns an integer indicating the id of the current navigation mode version if the manager is in navigation mode.
		/// </summary>
		public int NavigationModeVersion
		{
			get { return this._navigationModeVersion; }
		}
		#endregion // NavigationModeVersion

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region GetActiveDocument
		/// <summary>
		/// Returns the current active document pane. Note, this may not be the current active pane.
		/// </summary>
		public ContentPane GetActiveDocument()
		{
			return this._activeDocument;
		}
		#endregion //GetActiveDocument

		#region GetActivePane
		/// <summary>
		/// Returns the current active non-document pane. Note, this may be a Document
		/// </summary>
		public ContentPane GetActivePane()
		{
			return this._activePane;
		}
		#endregion //GetActivePane

		#endregion //Public Methods

		#region Internal Methods

		#region ActivateNextDocument
		/// <summary>
		/// Activates the next/previous document pane.
		/// </summary>
		/// <param name="pane">The starting point for the navigation</param>
		/// <param name="next">True to move to the next pane; false to move to the previous</param>
		/// <returns>True if the navigation was performed</returns>
		internal bool ActivateNextDocument(ContentPane pane, bool next)
		{
			return this.ActivateNextItem(pane, next, true);
		}
		#endregion // ActivateNextDocument

		#region ActivateNextPane
		/// <summary>
		/// Activates the next/previous non-document pane.
		/// </summary>
		/// <param name="pane">The starting point for the navigation</param>
		/// <param name="next">True to move to the next pane; false to move to the previous</param>
		/// <returns>True if the navigation was performed</returns>
		internal bool ActivateNextPane(ContentPane pane, bool next)
		{
			return this.ActivateNextItem(pane, next, false);
		}
		#endregion // ActivateNextPane

		#region GetInitialPaneNavigatorItem
		/// <summary>
		/// Helper method for determining which pane should be selected in the pane navigator based on 
		/// which pane is active, whether we are navigating the documents or panes initially and whether 
		/// to navigate to the next/previous item.
		/// </summary>
		/// <param name="startWithDocuments">True if we are navigating the documents; false to navigate the panes list. If there are no activatable panes of the specified type then the other type could be returned.</param>
		/// <param name="offset">1 to navigate to the next item after the active one; -1 to navigate to the item before the active one and 0 to use the active one</param>
		/// <returns>The pane to start with</returns>
		internal ContentPane GetInitialPaneNavigatorItem(bool startWithDocuments, int offset)
		{
			ContentPane initialPane = startWithDocuments ? this._activeDocument : this._activePane;

			if (false == startWithDocuments && (null == initialPane || initialPane.IsDocument))
			{
				// if the active pane is a document but we're supposed to start with panes then 
				// just get the last active pane - we need to specify the navigation order just 
				// in case when we do navigate we would do so through the visible order but we 
				// still need to start with the last active pane
				initialPane = GetNextActivePane(null, true, PaneFilterFlags.AllVisibleExceptDocument, PaneNavigationOrder.ActivationOrder, false, true);

				// if we don't have any activatable panes then just start with the first document
				if (initialPane == null)
					initialPane = this._activeDocument;
			}
			// AS 9/9/09 TFS21110
			// This isn't directly related but we're not starting off with the correct pane in the 
			// navigator when focus is within an hwndhost within the contentpane.
			//
			//else if (startWithDocuments && null != initialPane && false == GetIsKeyboardFocusWithinEx(initialPane))
			else if (startWithDocuments && null != initialPane && !initialPane.IsKeyboardFocusWithinEx)
			{
				// if we are supposed to start with documents but it wasn't active then just start with that - 
				// i.e. do not honor the offset
			}
			else if (initialPane != null)
			{
				// we found a pane to start with and its of the right type - process the offset
				if (offset != 0)
					initialPane = GetNextActivePane(initialPane, offset > 0, startWithDocuments ? PaneFilterFlags.Document : PaneFilterFlags.AllVisibleExceptDocument);
			}
			else
			{
				// we didn't find a pane of the specified type so just get a pane of the other type - here too
				// we'll start with the current active item
				PaneFilterFlags oppositefilter = startWithDocuments ? PaneFilterFlags.AllVisibleExceptDocument : PaneFilterFlags.Document;
				initialPane = GetNextActivePane(null, true, oppositefilter, PaneNavigationOrder.ActivationOrder, false, true);
			}

			return initialPane;
		} 
		#endregion //GetInitialPaneNavigatorItem

		#region GetLastActivePane
		internal ContentPane GetLastActivePane()
		{
			if (this._activePane == null)
				return this.GetNextActivePane(null, true, PaneFilterFlags.AllVisible, PaneNavigationOrder.ActivationOrder, false, true);

			return this._activePane;
		} 
		#endregion //GetLastActivePane

		#region GetPanes
		/// <summary>
		/// Returns an enumerable list of panes.
		/// </summary>
		/// <param name="filter">A flagged enum used to indicate which panes should be returned.</param>
		/// <param name="order">The order in which the panes should be iterated</param>
		/// <param name="ensureActivatable">True if only activatable panes should be considered</param>
		/// <returns>An enumerable list of panes</returns>
		internal IEnumerable<ContentPane> GetPanes(PaneFilterFlags filter, PaneNavigationOrder order, bool ensureActivatable)
		{
			ContentPane pane = null;

			while (null != (pane = GetNextActivePane(pane, true, filter, order, false, ensureActivatable)))
				yield return pane;

			yield break;
		}
		#endregion //GetPanes

		// AS 1/12/11 TFS61435
		#region MoveToFront
		internal void MoveToFront(ContentPane pane)
		{
			Debug.Assert(pane != null, "Cannot move a null");
			Debug.Assert(_activePane == null, "This method is meant to be used when we don't have an active pane. Otherwise you should activate this pane.");

			if (_activePane != null || pane == null)
				return;

			Debug.Assert(_panes.Contains(pane), "Specified pane doesn't exist in the list!");

			Output(pane, "** Move To Front Of Activation List **");
			if (this._panes.Remove(pane))
			{
				this._panes.AddFirst(pane);
			}
		}
		#endregion //MoveToFront

		#region OnPaneAdded
		internal void OnPaneAdded(ContentPane pane)
		{
			int removalIndex = this._panesToRemove.IndexOf(pane);

			if (removalIndex >= 0)
			{
				this._panesToRemove.RemoveAt(removalIndex);
				return;
			}

			// AS 7/17/09 TFS19569
			// Since we may skip the removal, it is possible that we will get an add for 
			// a pane that we already have in the list in which case we will ignore the 
			// request.
			//
			//Debug.Assert(this._panes.Contains(pane) == false);
			if (this._panes.Contains(pane))
				return;


			this._panes.AddLast(pane);

			if (pane.PaneLocation != PaneLocation.Unknown)
				this.VerifyActiveDocument(false);
		}
		#endregion //OnPaneAdded

		// AS 7/17/09 TFS18453/TFS19568
		#region OnPlaceholderAdded
		internal void OnPlaceholderAdded(ContentPanePlaceholder placeholder)
		{
			int removeIndex = _placeholdersToRemove.IndexOf(placeholder);

			if (removeIndex >= 0)
			{
				_placeholdersToRemove.RemoveAt(removeIndex);
			}
		}
		#endregion //OnPlaceholderRemoved

		#region OnPaneRemoved
		internal void OnPaneRemoved(ContentPane pane)
		{
			// put the pane in a pending remove list and start a begin invoke
			Debug.Assert(this._panesToRemove.Contains(pane) == false);

			this._panesToRemove.Add(pane);

			// AS 7/15/09 TFS18453/Optimization
			// Only dispatch 1 message - when we get the first pane to remove.
			//
			if (_panesToRemove.Count == 1)
				// AS 8/26/09 TFS21358
				//this._dockManager.Dispatcher.BeginInvoke(DispatcherPriority.Send, new DockManagerUtilities.MethodInvoker(ProcessPaneRemoval));
				this._dockManager.Dispatcher.BeginInvoke(DispatcherPriority.Render, new DockManagerUtilities.MethodInvoker(ProcessPaneRemoval));
		}
		#endregion //OnPaneRemoved

		// AS 7/17/09 TFS18453/TFS19568
		#region OnPlaceholderRemoved
		internal void OnPlaceholderRemoved(ContentPanePlaceholder placeholder)
		{
			// put the pane in a pending remove list and start a begin invoke
			Debug.Assert(_placeholdersToRemove.Contains(placeholder) == false);

			this._placeholdersToRemove.Add(placeholder);

			if (_placeholdersToRemove.Count == 1)
				// AS 8/26/09 TFS21358
				//this._dockManager.Dispatcher.BeginInvoke(DispatcherPriority.Send, new DockManagerUtilities.MethodInvoker(ProcessPaneRemoval));
				this._dockManager.Dispatcher.BeginInvoke(DispatcherPriority.Render, new DockManagerUtilities.MethodInvoker(ProcessPaneRemoval));
		} 
		#endregion //OnPlaceholderRemoved

		// AS 10/14/10 TFS57352
		#region ProcessPendingOperations
		internal void ProcessPendingOperations()
		{
			this.ProcessPaneRemoval();
		}
		#endregion //ProcessPendingOperations

		#region ProcessPaneRemoval
		private void ProcessPaneRemoval()
		{
			// AS 7/16/09 TFS18452
			// While debugging this I found that we may get in here while we are waiting for
			// the panes to be reloaded (e.g. when we get the onapplytemplate) so do not 
			// do anything if we are not yet loaded. With the fix for TFS18453, we would 
			// end up removing the placeholders, etc.
			//
			if (_dockManager.IsLoaded == false)
				return;

			// AS 7/17/09 TFS19569
			// If the template of the xamDockManager has changed since it was previously applied but the new one 
			// has not been applied then the panes may still be visual children of the old dockmanager panel. When 
			// that template was released we got the change notification for the DockManager property even though 
			// it was still in the logical tree of the XDM. We will force the new template to be applied so the 
			// panes can be moved from the old dockmanagerpanel to the new one which should trigger the DockManager
			// property change and remove the panes from the remove list.
			//
			if (_dockManager.DockPanel != null)
				_dockManager.ApplyTemplate();

			// AS 7/17/09 TFS18453/TFS19568 [Start]
			ContentPanePlaceholder[] placeholders = _placeholdersToRemove.ToArray();
			_placeholdersToRemove.Clear();

			if (placeholders.Length > 0)
			{
				for (int i = 0; i < placeholders.Length; i++)
				{
					ContentPanePlaceholder placeholder = placeholders[i];

					Debug.Assert(XamDockManager.GetDockManager(placeholder) != _dockManager);
					Debug.Assert(!DockManagerUtilities.IsValidDockManagerElement(_dockManager, placeholder), "Placeholder is still a descendant");

					ContentPane cp = placeholder.Pane;

					if (null != cp)
					{
						if (DockManagerUtilities.IsValidDockManagerElement(_dockManager, placeholder))
						{
							Debug.Assert(XamDockManager.GetDockManager(placeholder) == null);
							continue;
						}

						Output(placeholder, "Placeholder Removed");

						// AS 2/2/10 TFS6504
						// if the pane is unpinned and this is the dockedcontainer for it then 
						// remove the pane from the unpinned area (i.e. reinsert into the parent 
						// container)
						if (placeholder == cp.PlacementInfo.DockedEdgePlaceholder && cp.IsPinned == false)
						{
							_dockManager.MovePaneToPinnedContainer(cp, true);
							continue;
						}

						cp.PlacementInfo.RemovePlaceholder(placeholder);
					}
				}
			}
			// AS 7/17/09 TFS18453/TFS19568 [End]

			// AS 7/15/09 TFS18453
			// Since we're going to be doing more than just clearing the active panes list
			// we should empty the list and work with a finite set.
			//
			ContentPane[] panesToRemove = _panesToRemove.ToArray();
			_panesToRemove.Clear();

			if (panesToRemove.Length > 0)
			{
				for (int i = 0, count = panesToRemove.Length; i < count; i++)
				{
					ContentPane pane = panesToRemove[i];

					Debug.Assert(XamDockManager.GetDockManager(pane) != _dockManager);
					Debug.Assert(!DockManagerUtilities.IsValidDockManagerElement(_dockManager, pane), "Pane is still a descendant");

					// AS 7/17/09 TFS19569
					// Just in case we get here for a pane that is still in the logical tree, we will 
					// assume that it is still valid and skip the removal.
					//
					if (DockManagerUtilities.IsValidDockManagerElement(_dockManager, pane))
					{
						Debug.Assert(XamDockManager.GetDockManager(pane) == null);
						continue;
					}

					Output(pane, "Pane Removed from ActivePane List");

					this._panes.Remove(pane);

					// AS 7/15/09 TFS18453
					// We need to make sure we clean up the pane (e.g. remove any 
					// placeholders that were created for this pane).
					//
					// AS 2/2/10 TFS25608
					// Added 4th parameter to indicate that we should not remove the pane from 
					// its current container. The reason we should get here is because the pane 
					// is disconnected from the dockmanager. If it wasn't explicitly pulled out 
					// by the customer then we shouldn't pull it out implicitly either.
					//
					_dockManager.ClosePaneHelper(pane, PaneCloseAction.RemovePane, false, false);
				}

				// AS 7/15/09 TFS18453
				// This is now cleared above. We're only working with a local array.
				//
				//this._panesToRemove.Clear();

				this.VerifyActivePane(true );
			}
		}
		#endregion //ProcessPaneRemoval

		#region RebuildPaneList
		/// <summary>
		/// Creates the default active pane list based on the visual order
		/// </summary>
		internal void RebuildPaneList()
		{
			this._panes.Clear();

			PaneFilterFlags filter = PaneFilterFlags.All;

			ContentPane pane = DockManagerUtilities.GetFirstLastPane(this._dockManager, true, filter);

			if (null != pane)
			{
				LinkedListNode<ContentPane> node = this._panes.AddFirst(pane);

				while (null != (pane = DockManagerUtilities.GetNextPreviousPane(pane, false, true, null, filter)))
				{
					node = this._panes.AddAfter(node, pane);
				}
			}

            
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

            ContentPane focusedPane = GetKeyboardFocusedPane();

			if (null != focusedPane)
				this.OnContentPaneStateChanged(focusedPane);
			else
			{
				
			}

			this.VerifyActiveDocument(true);
		}
		#endregion //RebuildPaneList

		#region RefreshActivePaneSortOrder
		internal void RefreshActivePaneSortOrder()
		{
			List<ContentPane> panes = new List<ContentPane>(this._panes.Count);

			foreach (ContentPane pane in this._panes)
				panes.Add(pane);

			// clear the tree
			this._panes.Clear();

			// sort by the datetime
			Utilities.SortMergeGeneric(panes, LastActivatedTimeComparer.Instance);

			// rebuild the list
			foreach (ContentPane pane in panes)
			{
				// AS 5/14/08 BR32587
				// The panes are sorted such that those with the latest activation
				// times are at the end so we need to add each to the head since
				// each one was activated after the previous one and our linked list
				// is managed with the most recently activated at the head.
				//
				//this._panes.AddLast(pane);
				this._panes.AddFirst(pane);
			}
		}
		#endregion //RefreshActivePaneSortOrder

		// AS 5/14/08 BR32587
		#region ResetActivePanesAfterLoad
		internal void ResetActivePanesAfterLoad(bool canChangeActivePane)
		{
			// after a layout is loaded, we need to find the first set of activatable
			// panes that are in view to be the new active document and active pane
			// without disturbing the selecteditem of a containing tabgroup
			ContentPane newActivePane = null;
			ContentPane newActiveDocument = null;

			foreach (ContentPane pane in this.GetPanes(PaneFilterFlags.AllVisible, PaneNavigationOrder.ActivationOrder, true))
			{
				if (pane.PaneLocation != PaneLocation.Unpinned)
				{
					TabGroupPane owningTabGroup = LogicalTreeHelper.GetParent(pane) as TabGroupPane;

					// if the pane is not in a tabgroup or its the selected item
					if (null == owningTabGroup || pane == owningTabGroup.SelectedItem)
					{
						if (newActivePane == null)
							newActivePane = pane;

						if (pane.PaneLocation == PaneLocation.Document)
							newActiveDocument = pane;

						// if we have an active document then we'd have an active pane
						// but the reverse is not true so once we have both, exit
						if (newActiveDocument != null)
							break;
					}
				}
			}

			// if we're not allowed to change the active pane use the current active pane
			if (canChangeActivePane == false)
			{
				newActivePane = this._activePane;

				// if that pane can't be the active pane any more then find the next active pane
				if (null != this._activePane && this._activePane.CanActivate == false)
					newActivePane = this.GetNextActivePane(null, true, PaneFilterFlags.AllVisible, PaneNavigationOrder.ActivationOrder, false, true);
			}
			else if (newActivePane == null)
			{
				// if we're allowed to change the active pane but we didn't find
				// one then use the last active pane
				newActivePane = this.GetLastActivePane();
			}

			this.SetActivePaneImpl(newActivePane, newActiveDocument);
		}
		#endregion //ResetActivePanesAfterLoad

		#region ResumeVerification
		internal void ResumeVerification()
		{
			Debug.Assert(this._suspendVerificationCount > 0);
			this._suspendVerificationCount--;

			if (this._suspendVerificationCount == 0)
				this.VerifyActivePane();
		} 
		#endregion //ResumeVerification

		// AS 7/14/09 TFS18400
		// Added a helper method for changing the activedocument without changing the active pane.
		// This is really only meant for the case that we know the old active document was invalid 
		// and we know the new active document.
		//
		#region SetActiveDocument
		internal void SetActiveDocument(ContentPane cp)
		{
			Debug.Assert(null != cp && cp.IsDocument && XamDockManager.GetDockManager(cp) == _dockManager);
			this.SetActivePaneImpl(_activePane, cp);
		}
		#endregion //SetActiveDocument

		#region SuspendVerification
		internal void SuspendVerification()
		{
			this._suspendVerificationCount++;
		} 
		#endregion //SuspendVerification

		#region VerifyActivePane
		/// <summary>
		/// Helper method for ensuring that the current ActivePane and ActiveDocument panes are 
		/// still associated with the dockmanager
		/// </summary>
		internal void VerifyActivePane()
		{
			// AS 6/10/11 TFS74484
			this.VerifyActivePane(false);
		}

		// AS 6/10/11 TFS74484
		private void VerifyActivePane(bool verifySelectedTab)
		{
			if (this.IsVerificationSuspended)
				return;

			// AS 1/10/12 TFS89803
			// In this case what was happening is that we were calling UpdateLayout 
			// while we were in the middle of moving an item in the tabgrouppane. 
			// Really though we should not be changing the active pane while 
			// we are loading the layout. Instead, we should verify the active 
			// pane after the layout load is complete which should happen when 
			// the ResetActivePanesAfterLoad method is invoked.
			//
			if (_dockManager.IsLoadingLayout)
				return;

			ContentPane oldActivePane = this._activePane;
			ContentPane oldActiveDocument = this._activeDocument;
			ContentPane paneToActivate = this._activePane;
			ContentPane documentToActivate = this._activeDocument;

            if (oldActivePane != null)
			{
				PaneFilterFlags filter = oldActivePane == oldActiveDocument ? PaneFilterFlags.Document : PaneFilterFlags.AllVisibleExceptDocument;

				// if the pane was removed from the dockmanager then use the active pane list
				// to get the pane to activate
				if (XamDockManager.GetDockManager(oldActivePane) != this._dockManager)
					paneToActivate = GetPaneInActivationOrder(oldActivePane, false, true, filter, true);
                // AS 3/6/09 TFS15044
                // Use oldActivePane for consistency.
                //
				//else if (this._activePane.Visibility == Visibility.Collapsed)
                else if (oldActivePane.Visibility == Visibility.Collapsed)
					paneToActivate = GetNextActivePane(oldActivePane, true, filter);
				else
					paneToActivate = oldActivePane;

				// if we can't find one of the same type then just find any pane
				if (paneToActivate == null)
					paneToActivate = GetPaneInActivationOrder(null, true, true, PaneFilterFlags.AllVisible, true);

				if (paneToActivate != null && XamDockManager.GetDockManager(paneToActivate) != this._dockManager)
					paneToActivate = null;

                // AS 3/6/09 TFS15044
                // If we're about to focus a pane and keyboard focus is not within 
                // any of our panes then make sure we're not going to steal focus
                // from another control
                if (paneToActivate != null && this.GetKeyboardFocusedPane() == null)
                {
                    DependencyObject focusScope = FocusManager.GetFocusScope(this._dockManager);

                    if (null != focusScope)
                    {
                        DependencyObject focusedObject = FocusManager.GetFocusedElement(focusScope) as DependencyObject;

                        // if there is a focused object that is not the xamdockmanager
                        // and is not within the xamdockmanager then we should not steal
                        // focus from it
                        if (focusedObject != null &&                        // something has logical focus
                            focusedObject is XamDockManager == false &&     // but its not the dockmanager itself (in which case it would be ok to steal focus)
                            GetDockManagerPane(focusedObject, this._dockManager) == null &&     // and its not within any of our panes
                            Utilities.IsDescendantOf(focusScope, focusedObject, true))          // and that focused object is valid (i.e. its a child of the focus scope and not itself orphaned)
                        {
                            paneToActivate = null;
                        }
                    }
                }
			}

			// verify the active document as well
			if (oldActiveDocument != null)
			{
				// if the pane was removed from the dockmanager then use the active pane list
				// to get the pane to activate
				if (XamDockManager.GetDockManager(oldActiveDocument) != this._dockManager)
					documentToActivate = GetPaneInActivationOrder(oldActiveDocument, false, true, PaneFilterFlags.Document, true);
				else if (this._activeDocument.Visibility == Visibility.Collapsed || PaneLocation.Document != this._activeDocument.PaneLocation)
					documentToActivate = GetNextActivePane(oldActiveDocument, true, PaneFilterFlags.Document);
				else
					documentToActivate = oldActiveDocument;

				if (documentToActivate != null && XamDockManager.GetDockManager(documentToActivate) != this._dockManager)
					documentToActivate = null;
			}

			if (oldActivePane != paneToActivate || oldActiveDocument != documentToActivate)
			{
				OutputIf(oldActivePane != paneToActivate, string.Format("Changing Pane: Old:{0} New:{1}", oldActivePane, paneToActivate), "VerifyActivePane");
				OutputIf(oldActiveDocument != documentToActivate, string.Format("Changing Document: Old:{0} New:{1}", oldActiveDocument, documentToActivate), "VerifyActivePane");

				// AS 5/14/08 BR32037
				bool oldSuppress = false;
				if (null != paneToActivate)
				{
					oldSuppress = paneToActivate.SuppressFlyoutDuringActivate;
					paneToActivate.SuppressFlyoutDuringActivate = true;
				}

				try
				{
					this.SetActivePaneImpl(paneToActivate, documentToActivate);

					// AS 5/14/08
					// Added extra check in case the active pane changes while raising the events.
					//
					if (null != paneToActivate && paneToActivate == this._activePane)
					{
						// AS 9/8/09 TFS20496
						// We need to check the IsKeyboardFocusWithinEx because focus could be within 
						// the HwndHost within the old pane.
						//
						//if (oldActivePane.IsKeyboardFocusWithin)
						// AS 10/16/09 TFS23012
						// If we consider a pane active and we don't have another pane with keyboard focus
						// we're going to activate the pane and take focus.
						//
						//if (oldActivePane.IsKeyboardFocusWithinEx)
						if (oldActivePane.IsKeyboardFocusWithinEx || 
							(Keyboard.FocusedElement == null && this.GetKeyboardFocusedPane() == null))
						{
							// AS 5/28/08
							// When focus is shifted from the old pane to the new pane and the new pane
							// is unpinned, we should not force the flyout to be displayed.
							//
							//paneToActivate.Activate();
							paneToActivate.ActivateInternal(false, paneToActivate.PaneLocation != PaneLocation.Unpinned);
						}
						// AS 5/14/08 BR32037
						// Added if clause since we don't want to show the flyout as a result of verification.
						//
						else if (paneToActivate.PaneLocation != PaneLocation.Unpinned)
						{
							paneToActivate.BringIntoView();
							paneToActivate.UpdateLayout();
						}
					}
				}
				finally
				{
					if (null != paneToActivate)
						paneToActivate.SuppressFlyoutDuringActivate = oldSuppress;
				}

				// AS 5/14/08
				// Added extra check in case the active pane changes while raising the events.
				//
				if (null != documentToActivate && documentToActivate != paneToActivate && documentToActivate == this._activeDocument)
				{
					documentToActivate.BringIntoView();
					documentToActivate.UpdateLayout();
				}
			}

			this.VerifyActiveDocument(false);

			// AS 6/10/11 TFS74484
			if (verifySelectedTab && !_dockManager.IsClosingPanes)
			{
				this.VerifyTabIsSelected(this._activePane);
				this.VerifyTabIsSelected(this._activeDocument);
			}
		}
		#endregion //VerifyActivePane

		// AS 4/17/09 TFS16807/Optimization
		// Moved the creation/destruction of the preprocess handler from the 
		// OnDockManagerLoadStateChanged to a common helper routine so we can 
		// ensure we only create it if we have an HwndHost.
		//
		#region VerifyHasPreprocessHandler
		internal void VerifyHasPreprocessHandler()
		{
			if (_dockManager.IsLoaded && _dockManager.HasHwndHost)
			{
				//// AS 11/25/08 TFS8265
				//if (null == this._preprocessHandler)
				if (null == this._preprocessHandler && _canCreatePreprocessHandler)
				{
					// AS 4/17/09 TFS16807
					//this._preprocessHandler = WeakPreprocessMessage.Create(this);
					try
					{
						this._preprocessHandler = this.CreatePreprocessHandler();
					}
					catch (System.Security.SecurityException)
					{
						_canCreatePreprocessHandler = false;
					}
				}
			}
			else
			{
				// AS 11/25/08 TFS8265
				if (null != this._preprocessHandler)
					this._preprocessHandler.Dispose();
			}
		}
		#endregion //VerifyHasPreprocessHandler

		#endregion //Internal Methods

		#region Private Methods

		#region ActivateNextItem
		/// <summary>
		/// Helper method to activate the next document/non-document in the list based on the dockmanager's NavigationOrder property
		/// </summary>
		/// <param name="pane">The pane to start with</param>
		/// <param name="next">Next to move forward; otherwise false to move to the previous item</param>
		/// <param name="document">True if we are navigating documents; false if we are navigating non-documents</param>
		/// <returns>True if the pane was activated; otherwise false</returns>
		private bool ActivateNextItem(ContentPane pane, bool next, bool document)
		{
			Output(string.Format("Pane: {0}, Next={1}, Document={2}", pane, next, document), "ActivateNextItem");

			ContentPane paneToActivate = null;
			PaneFilterFlags filter = document ? PaneFilterFlags.Document : PaneFilterFlags.AllVisibleExceptDocument;
			Debug.Assert(null == pane || this._dockManager == XamDockManager.GetDockManager(pane));

			if (null == pane || document != pane.IsDocument)
			{
				paneToActivate = document ? this.GetActiveDocument() : this.GetActivePane();

				// there are 2 possibilities that may necessitate a new search - the active pane is 
				// a document and we didn't want a document or we didn't get anything because we
				// have no documents or are not using them and focus is not within a pane. in either
				// case we want to find the last activated activatable pane.
				if (false == document && (null == paneToActivate || paneToActivate.IsDocument))
				{
					paneToActivate = GetPaneInActivationOrder(null, true, true, filter, true);
				}
			}
			else
				paneToActivate = GetNextActivePane(pane, next, filter);

			if (null != paneToActivate)
			{
				return paneToActivate.ActivateInternal();
			}

			return false;
		}
		#endregion // ActivateNextItem

		#region ClearActivePane
		private void ClearActivePane()
		{
			this.SetActivePane(null);
		}
		#endregion // ClearActivePane

		#region CreateNavigationModeFocusWatcher
		private void CreateNavigationModeFocusWatcher()
		{
			Debug.Assert(this._navigationModeFocusWatcher == null);
			this.DisposeNavigationModeFocusWatcher();

			Output("Created", "NavigationModeFocusWatcher");
			this._navigationModeFocusWatcher = new KeyEventFocusWatcher();
			this._navigationModeFocusWatcher.KeyEvent += new KeyEventHandler(OnNavigationModeFocusKeyEvent);

			// AS 5/8/09 TFS17021
			// We're not getting the key up for the alt/ctrl because focus is shifting elsewhere so watch
			// when focus shifts to null. When it does we will do a delayed check and if we don't have an 
			// active pane then we can consider ourselves out of navigation mode. It was this focuswatcher 
			// instance that was sticking around when the application lost focus. I made changes in the 
			// focuswatcher to more lazily find out when the focus has been changed just in case this 
			// should happen in a different situation.
			//
			this._navigationModeFocusWatcher.FocusedElementChanged += new RoutedPropertyChangedEventHandler<IInputElement>(OnNavigationModeFocusChanged);
		}

		#endregion //CreateNavigationModeFocusWatcher

		#region CreateActivePaneFocusWatcher
		private void CreateActivePaneFocusWatcher()
		{
			Debug.Assert(this._activePaneFocusWatcher == null);
			this.DisposeActivePaneFocusWatcher();

			Output("Created", "ActivePaneFocusWatcher");
			this._activePaneFocusWatcher = new FocusWatcher();
			this._activePaneFocusWatcher.FocusedElementChanged += new RoutedPropertyChangedEventHandler<IInputElement>(OnKeyboardFocusedElementChanged);
		}
		#endregion //CreateActivePaneFocusWatcher

		#region DisposeActivePaneFocusWatcher
		private void DisposeActivePaneFocusWatcher()
		{
			if (null != this._activePaneFocusWatcher)
			{
				Output("Disposed", "ActivePaneFocusWatcher");
				this._activePaneFocusWatcher.FocusedElementChanged -= new RoutedPropertyChangedEventHandler<IInputElement>(OnKeyboardFocusedElementChanged);
				this._activePaneFocusWatcher.Dispose();
				this._activePaneFocusWatcher = null;
			}
		}
		#endregion //DisposeActivePaneFocusWatcher

		#region DisposeNavigationModeFocusWatcher
		private void DisposeNavigationModeFocusWatcher()
		{
			if (null != this._navigationModeFocusWatcher)
			{
				Output("Disposed", "NavigationModeFocusWatcher");
				this._navigationModeFocusWatcher.KeyEvent -= new KeyEventHandler(OnNavigationModeFocusKeyEvent);

				// AS 5/8/09 TFS17021
				this._navigationModeFocusWatcher.FocusedElementChanged -= new RoutedPropertyChangedEventHandler<IInputElement>(OnNavigationModeFocusChanged);

				this._navigationModeFocusWatcher.Dispose();
				this._navigationModeFocusWatcher = null;
			}
		}

		#endregion //DisposeNavigationModeFocusWatcher

        // AS 1/28/09 TFS11956
        // Refactored from the GetFocusedPane method.
        //
        #region GetDockManagerPane

        // AS 3/3/09 TFS10656
        // Added some extra overloads.
        //
        private static ContentPane GetDockManagerPane(IInputElement element, XamDockManager dockManager)
        {
            return GetDockManagerPane(element as DependencyObject, dockManager);
        }

        private static ContentPane GetDockManagerPane(DependencyObject d, XamDockManager dockManager)
        {
            if (null != d)
                return GetDockManagerPane(d as ContentPane ?? (ContentPane)Utilities.GetAncestorFromType(d, typeof(ContentPane), true, dockManager), dockManager);

            return null;
        }

        private static ContentPane GetDockManagerPane(ContentPane pane, XamDockManager dockManager)
        {
            Debug.Assert(null != dockManager);

            // AS 11/5/08 TFS9956
            // Ensure this is from our dockmanager and not from a parent or nested dockmanager.
            //
            while (pane != null && XamDockManager.GetDockManager(pane) != dockManager)
            {
                // AS 11/13/08 TFS9956
                // GetAncestorFromType will return the descendant if its of the specified type so
                // we need to get the parent.
                //
                //paneFromFocusScope = Utilities.GetAncestorFromType((DependencyObject)focusedElement, typeof(ContentPane), true, this._dockManager) as ContentPane;
                DependencyObject ancestor = Utilities.GetParent(pane);

                if (null == ancestor)
                    return null;

                pane = Utilities.GetAncestorFromType(ancestor, typeof(ContentPane), true, dockManager) as ContentPane;
            }

            return pane;
        }
        #endregion //GetDockManagerPane

        #region GetFocusedPane
        private ContentPane GetFocusedPane(DependencyObject focusScopeChild)
		{
			ContentPane paneFromFocusScope = null;

			if (null != focusScopeChild)
			{
				DependencyObject focusScope = FocusManager.GetFocusScope(focusScopeChild);
				IInputElement focusedElement = null != focusScope ? FocusManager.GetFocusedElement(focusScope) : null;

                
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)




                paneFromFocusScope = GetDockManagerPane(focusedElement, this._dockManager);
			}

			return paneFromFocusScope;
		}
		#endregion //GetFocusedPane

        // AS 2/9/09 TFS13375
        #region GetKeyboardFocusedPane
        private ContentPane GetKeyboardFocusedPane()
        {
            DependencyObject focusedObject = Keyboard.FocusedElement as DependencyObject;
            ContentPane focusedPane = null;

            if (null != focusedObject)
            {
                
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

                focusedPane = GetDockManagerPane(focusedObject, this._dockManager);
            }
            else
            {
                // if the FocusedElement is null then its possible that an hwnd host has the input
                // focus
                foreach (ContentPane cp in this._panes)
                {
                    if (cp.HasFocusInHwndHost())
                    {
						// AS 9/8/09 TFS20496
						// We need to skip closed/hidden panes even if they contain the keyboard focus
						// because those really can't be the focused pane.
						//
						if (cp.Visibility != Visibility.Visible)
							continue;

                        focusedPane = cp;
                        break;
                    }
                }
            }

            return focusedPane;
        }
        #endregion //GetKeyboardFocusedPane

		#region GetNextActivePane
		/// <summary>
		/// Helper method for determining what pane would/should be activated after the specified pane honoring the 
		/// dockmanager's NavigationOrder
		/// </summary>
		/// <param name="pane">The pane to start with (or null to start at the beginning)</param>
		/// <param name="next">True to get the next item; false to get the previous item</param>
		/// <param name="filter">The filter used to restrict which panes to locate</param>
		/// <returns>The next/previous pane to activate</returns>
		private ContentPane GetNextActivePane(ContentPane pane, bool next, PaneFilterFlags filter)
		{
			return this.GetNextActivePane(pane, next, filter, this._dockManager.NavigationOrder, true, true);
		}

		/// <summary>
		/// Helper method for determining what pane would/should be activated after the specified pane with a specific 
		/// navigation order
		/// </summary>
		/// <param name="pane">The pane to start with (or null to start at the beginning)</param>
		/// <param name="next">True to get the next item; false to get the previous item</param>
		/// <param name="filter">The filter used to restrict which panes to locate</param>
		/// <param name="wrap">True to allow wrapping around to the beginning of the list; false to not wrap</param>
		/// <param name="order">Indicates how the next/previous item should be found - based on its visible position or based on the order in which it has been activated.</param>
		/// <param name="ensureActivatable">True to only return panes that can be activated; false to include all.</param>
		/// <returns>The next/previous pane to activate</returns>
		private ContentPane GetNextActivePane(ContentPane pane, bool next, PaneFilterFlags filter, PaneNavigationOrder order, bool wrap, bool ensureActivatable)
		{
			ContentPane nextPane = null;

			if (order == PaneNavigationOrder.VisibleOrder)
			{
				// start with the specified pane
				nextPane = pane;
				ContentPane firstPane = null;

				while (null != (nextPane = DockManagerUtilities.GetNextPreviousPane(nextPane, wrap, next, this._dockManager, filter)))
				{
					// this is a safety net in case we didn't start out with a pane but there are
					// no activatable panes
					if (firstPane == nextPane)
					{
						nextPane = null;
						break;
					}
					else if (firstPane == null)
						firstPane = nextPane;

					// find an activatable pane
					if (XamDockManager.GetDockManager(nextPane) == this._dockManager)
					{
						if (ensureActivatable == false || nextPane.CanActivate)
							break;
					}
				}
			}
			else
			{
				nextPane = GetPaneInActivationOrder(pane, pane == null, next, filter, ensureActivatable);

				// wrap if needed
				if (wrap && null == nextPane && null != pane)
					nextPane = GetPaneInActivationOrder(null, true, next, filter, ensureActivatable);
			}

			return nextPane;
		}
		#endregion //GetNextActivePane

		#region GetActivationOrderNode
		/// <summary>
		/// Helper method to walk forward/backward through the linked list evaluating all nodes including the starting node.
		/// </summary>
		/// <param name="node">The starting node or null to start with the first/last</param>
		/// <param name="startWithNode">True to start evaluating the filter with the specified node. False to start with the next/previous based on the <paramref name="next"/> parameter.</param>
		/// <param name="next">True to move forward in the list; otherwise false to move backward through the list</param>
		/// <param name="filter">The panelocation that the pane must conform to in order to be included</param>
		/// <param name="ensureActivatable">True to only include panes that can be activated; false otherwise.</param>
		/// <returns>The next/previous node or null if there is none</returns>
		private LinkedListNode<ContentPane> GetActivationOrderNode(LinkedListNode<ContentPane> node, bool startWithNode, bool next, PaneFilterFlags filter, bool ensureActivatable)
		{
            if (node == null)
            {
                node = next ? this._panes.First : this._panes.Last;

                // AS 3/6/09 TFS15044
                // If we didn't have a node passed in then we do want to start with 
                // the first one we found.
                //
                startWithNode = true;
            }

			if (node != null && startWithNode == false)
				node = next ? node.Next : node.Previous;

			ContentPane firstFoundPane = null;

			while (null != node)
			{
				ContentPane pane = node.Value;

				// safety net in case we don't have any activatable panes
				if (firstFoundPane == pane)
					return null;
				else if (firstFoundPane == null)
					firstFoundPane = pane;

				// AS 6/4/08
				// This could happen while adding the children to the visual tree/
				//
				// AS 7/17/09 TFS18453/TFS19568
				//Debug.Assert(VisualTreeHelper.GetParent(pane) == null || XamDockManager.GetDockManager(pane) == this._dockManager);
				Debug.Assert(
					VisualTreeHelper.GetParent(pane) == null || 
					XamDockManager.GetDockManager(pane) == this._dockManager || 
					DockManagerUtilities.IsValidDockManagerElement(_dockManager, pane) ||
					(_dockManager.DragManager.DragState != Infragistics.Windows.DockManager.Dragging.DragState.None && _dockManager.DragManager.PanesBeingDragged.Contains(pane)),
					"Pane in list is not a valid dockmanager pane?");

				if (XamDockManager.GetDockManager(pane) == this._dockManager
					&& (ensureActivatable == false || pane.CanActivate)
					&& DockManagerUtilities.MeetsCriteria(pane, filter))
					return node;

				node = next ? node.Next : node.Previous;
			}

			return null;
		}
		#endregion //GetActivationOrderNode

		#region GetPaneInActivationOrder
		/// <summary>
		/// Helper method for finding a ContentPane in the list.
		/// </summary>
		/// <param name="pane">The pane to use as the starting point or null to based on the first/last node.</param>
		/// <param name="startWithPane">True to start evaluating the filter with the specified node. False to start with the next/previous based on the <paramref name="next"/> parameter.</param>
		/// <param name="next">True to move forward in the list; otherwise false to move backward through the list</param>
		/// <param name="filter">The panelocation that the pane must conform to in order to be included</param>
		/// <param name="ensureActivatable">True to ensure that the pane can be activated; false to include all panes</param>
		/// <returns>The next/previous pane or null if there is none</returns>
		private ContentPane GetPaneInActivationOrder(ContentPane pane, bool startWithPane, bool next, PaneFilterFlags filter, bool ensureActivatable)
		{
			Debug.Assert(pane == null || startWithPane == false || DockManagerUtilities.MeetsCriteria(pane, filter), "The specified pane doesn't meet the filter criteria! This may not be a valid starting point.");
			Debug.Assert(pane == null || startWithPane == false || XamDockManager.GetDockManager(pane) == this._dockManager);

			LinkedListNode<ContentPane> node = pane != null ? this._panes.Find(pane) : null;
			LinkedListNode<ContentPane> nextPrevious = GetActivationOrderNode(node, startWithPane, next, filter, ensureActivatable);

			return null != nextPrevious ? nextPrevious.Value : null;
		}
		#endregion //GetPaneInActivationOrder

		#region IsKeyboardFocusWithinNestedParentFocusScope
		private bool IsKeyboardFocusWithinNestedParentFocusScope(ContentPane pane)
		{
			// get the focused element of the focus scope containing the pane
			DependencyObject parent = Utilities.GetParent(pane, true);
			DependencyObject parentFocusScope = null != parent ? FocusManager.GetFocusScope(parent) : null;

			// get the focusscope of the keyboard focused element
			IInputElement keyFocusedElement = Keyboard.FocusedElement;
			DependencyObject keyFocusedObject = keyFocusedElement as DependencyObject;
			DependencyObject keyFocusScope = null != keyFocusedObject ? FocusManager.GetFocusScope(keyFocusedObject) : null;

			// assuming they're not in the same focus scope
			if (keyFocusScope != parentFocusScope)
			{
				while (keyFocusScope != null)
				{
					// AS 2/17/12 TFS101330
					// If focus is within a dockmanager pane then we know it's not in a menu/toolbar/ribbon
					// and so we can allow the pane to be deactivated unless it actually contains the element 
					// with the keyboard focus.
					//
					// AS 2/17/12 TFS100637
					// Added additional parameter since in this case the focused element was not in the logical/visual tree. 
					// It was in a popup (of the contextmenu) and to retain the old behavior we should consider that a reason 
					// to stay active.
					//
					if (keyFocusScope is ContentPane && !Utilities.IsDescendantOf(pane, keyFocusedObject, true, true ))
						return false;

					keyFocusScope = Utilities.GetParent(keyFocusScope);

					if (null != keyFocusScope)
					{
						keyFocusScope = FocusManager.GetFocusScope(keyFocusScope);

						if (keyFocusScope == parentFocusScope)
							return true;
					}
				}
			}

			return false;
		}
		#endregion //IsKeyboardFocusWithinNestedParentFocusScope

		#region MovePaneToHead
		private void MovePaneToHead(ContentPane pane)
		{
			if (this._isInNavigationMode == false)
			{
				Output(pane, "** Move To Front Of Activation List **");
				this._panes.Remove(pane);
				this._panes.AddFirst(pane);

				pane.LastActivatedTime = DateTime.Now;
			}
			else
			{
				Output("In Navigation mode - Skipping", "MovePaneToHead");

				//Debug.Assert(this._isInNavigationMode == false
				//    || Keyboard.IsKeyDown(Key.LeftCtrl)
				//    || Keyboard.IsKeyDown(Key.RightCtrl)
				//    || Keyboard.IsKeyDown(Key.LeftAlt)
				//    || Keyboard.IsKeyDown(Key.RightCtrl), "We are still in navigation mode but none of the modifier keys are down!");

				if (Keyboard.IsKeyUp(Key.LeftCtrl) &&
					Keyboard.IsKeyUp(Key.RightCtrl) &&
					Keyboard.IsKeyUp(Key.LeftAlt) &&
					Keyboard.IsKeyUp(Key.RightCtrl))
				{
					this.IsInNavigationMode = false;
				}
			}
		}
		#endregion // MovePaneToHead

		#region OnContentPaneGotKeyboardFocus
		private static void OnContentPaneGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			ContentPane pane = sender as ContentPane;

			OutputIf(GetIsKeyboardFocusWithinEx(pane) == false, pane, "GotKeyboardFocus");

			PaneToolWindow toolWindow = ToolWindow.GetToolWindow(pane) as PaneToolWindow;

			if (null != toolWindow)
				toolWindow.LastActivePane = pane;

			// AS 5/8/09 TFS15993
			// In this specific case the window was minimized so there was no focused element.
			// So when the window was restored, WPF shifted focus back to the last focused 
			// element which was the pane. This was causing the flyout to be displayed because 
			// the unpinned pane was being activated. However this activation was implicit 
			// so we really don't want the pane to flyout. I could have tried to check for 
			// the WindowState of the window but that won't help an xbap case and it also 
			// won't help if you were to reactivate the window in some other way - e.g. 
			// using Alt-Tab.
			//
			bool suppressFlyout = e.OldFocus == null;
			bool wasFlyoutSuppressed = pane.SuppressFlyoutDuringActivate;

			try
			{
				if (suppressFlyout)
					pane.SuppressFlyoutDuringActivate = true;

				// keep our own flag that focus is or is not within the pane. the iskeyboardfocuswithin 
				// doesn't work when there's a break in the tree
				pane.SetValue(IsKeyboardFocusWithinExPropertyKey, KnownBoxes.TrueBox);
			}
			finally
			{
				if (suppressFlyout)
					pane.SuppressFlyoutDuringActivate = wasFlyoutSuppressed;
			}

			// the content pane is its own focus scope. when it got keyboard focus i think it should
			// have become the focused element of its focus scope but the framework is not doing that
			// so we have to. this is needed so we know when the pane is not the focused element of
			// its containing focusscope
			DependencyObject parent = VisualTreeHelper.GetParent(pane);
			DependencyObject focusScope = null != parent ? FocusManager.GetFocusScope(parent) : parent;

			// AS 3/25/08
			// We can't do this. For more, see the notes in the OnContentPaneStateChanged.
			//
			//if (null != focusScope)
			//    FocusManager.SetFocusedElement(focusScope, pane);
		}

		#endregion //OnContentPaneGotKeyboardFocus

		#region OnContentPaneLostKeyboardFocus
        
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

		#endregion //OnContentPaneLostKeyboardFocus

		#region OnContentPaneStateChanged
		private void OnContentPaneStateChanged(ContentPane pane)
		{
			Debug.Assert(null != pane);

			if (null == pane)
				return;

			bool activatePane = false;
			bool useActivePaneWatcher = false;

            // AS 1/28/09 TFS11956
            // As a safety valve, we should not accept a pane that is from another dockmanager.
            //
            Debug.Assert(_dockManager == XamDockManager.GetDockManager(pane) || XamDockManager.GetDockManager(pane) == null);

            if (_dockManager != XamDockManager.GetDockManager(pane))
            {
                activatePane = false;
            }
            else
			// if the pane has keyboard focus then it must be active
            // AS 11/25/08 TFS8265
            // Consider it active if it contains an hwndhost which has the input focus.
            //
            //if (GetIsKeyboardFocusWithinEx(pane))
			if (GetIsKeyboardFocusWithinEx(pane) || pane.HasFocusInHwndHost())
			{
				Output("Activating pane - IsKeyboardFocusWithinEx is true:" + pane.ToString(), "OnContentPaneStateChanged");
				activatePane = true;
			}
			else if (pane.Visibility != Visibility.Visible)
			{
				// AS 5/14/09 BR32006
				// When I implemented the fix for 44909, I think I reintroduced (or introduced 
				// a new way to reproduce BR32006). When the IsKeyboardFocusWithinEx was set to 
				// false, we were getting back into this routine with that pane but since we
				// never evaluated its visibility, we assumed we should treat it as the active
				// pane and ended up showing the flyout. We should not have tried to activate
				// the pane if it was hidden.
				//
				Output("The pane is hidden so it won't be activated:" + pane.ToString(), "OnContentPaneStateChanged");
				activatePane = false;
			}
			// AS 3/24/08
			// I can't use the FocusWithinManager because that is setting the IsFocusWithin
			// based on when my element contains a focused element and not when it is the 
			// focused element.
			//
			//else if (true.Equals(pane.GetValue(FocusWithinManager.IsFocusWithinProperty))				// has logical focus (i.e. focus within its focus scope)
			// AS 3/25/08
			// Using IsFocused only works if the content pane can become the focused element of 
			// its focus scope. There are two reasons we cannot/shouldn't do this. First, there is
			// a "behavior" in the CommandManager that will alter the OriginalSource of the CanExecute
			// and Execute events as they bubble up once they hit a focusscope and they end up transferring
			// the event to the focusedelement of that focusscope. In this case that means it transfers to
			// the active pane of the dockmanager since we set the focuselement to the active pane. The 
			// second issue is that we are altering the focus in the "body" of the app. If the customer is
			// not using a DocumentContentHost then they will want their control to continue to be the
			// focusedelement so when the window receives keyboard focus it goes to their control within.
			// Instead we need to consider ourselves active as long as the focus scope of the window
			//else if (pane.IsFocused																		// has logical focus (i.e. focus within its focus scope)
			//	&& (this._activePane == null || false == GetIsKeyboardFocusWithinEx(this._activePane))) // we don't have an active pane or it doesn't have the input focus (i.e. the active pane was within a floating window)
			else if (this.IsKeyboardFocusWithinNestedParentFocusScope(pane)									// has logical focus (i.e. focus within its focus scope)
				&& (this._activePane == null || false == GetIsKeyboardFocusWithinEx(this._activePane))) // we don't have an active pane or it doesn't have the input focus (i.e. the active pane was within a floating window)
			{
				// the reason for all of these checks is because we want to consider a pane to be
				// active even if it doesn't have the keyboard focus if it the focused element
				// and its containing window (the dockmanager's owning window) is active. this
				// is to cover the situation where you are within a wpf menu, ribbon, etc. which
				// are in their own focus scope.

				
				Output("Activating pane - Logical Focus in the Active Window:" + pane.ToString(), "OnContentPaneStateChanged");

				// its docked (i.e. within the xamdockamanger and therefore its window)
				if (DockManagerUtilities.IsFloating(XamDockManager.GetPaneLocation(pane)))
				{
					PaneToolWindow toolWindow = PaneToolWindow.GetToolWindow(pane) as PaneToolWindow;

					if (toolWindow.IsActive)
					{
						activatePane = true;
						useActivePaneWatcher = true;
					}
				}
				else // docked - i.e. within the xamdockmanager and therefore within its window
				{
					// and the containing window is active
					// AS 2/17/12 TFS100637
					// If we are not hosted within a window (e.g. using ElementHost) then IsDockManagerWindowActive 
					// would have been false. In that case we can assume that if the 
					// IsKeyboardFocusWithinNestedParentFocusScope call above was true that keyboard focus is within 
					// a related focus scope and so we can treat the pane as active.
					//
					//if (true == this.IsDockManagerWindowActive)
					if (true == this.IsDockManagerWindowActive || Window.GetWindow(pane) == null)
					{
						activatePane = true;
						useActivePaneWatcher = true;
					}
				}
			}

			if (useActivePaneWatcher && this._activePaneFocusWatcher == null)
				this.CreateActivePaneFocusWatcher();
			else if (false == useActivePaneWatcher && this._activePaneFocusWatcher != null)
				this.DisposeActivePaneFocusWatcher();

			if (activatePane == true)
			{
				this.SetActivePane(pane);
			}
			else if (pane == this._activePane)
			{
				Output("Clearing Active Pane", "OnContentPaneStateChanged");
				this.ClearActivePane();
			}
		}
		#endregion // OnContentPaneStateChanged

		#region OnDockManagerLoadStateChanged
		private void OnDockManagerLoadStateChanged(object sender, RoutedEventArgs e)
		{
			this.VerifyDockManagerWindowBindings();

            if (this._dockManager.IsLoaded)
            {
				// AS 7/16/09 TFS18452
				// If we bailed out of the ProcessPaneRemoval method because the 
				// dockmanager wasn't loaded yet then we should clean those up now.
				//
				// AS 9/24/09 TFS22613
				// We don't need to synchronously process this list.
				//
				//this.ProcessPaneRemoval();
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Infragistics.Windows.DockManager.DockManagerUtilities.MethodInvoker(this.ProcessPaneRemoval));

				
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

				this.VerifyHasPreprocessHandler();

                this.VerifyActiveDocument(false);
            }
            else
            {
                // just in case the dm gets unloaded while the watchers are enabled - dispose them now
                this.DisposeActivePaneFocusWatcher();
                this.DisposeNavigationModeFocusWatcher();

				// AS 9/11/09 TFS21329/Optimization
				// Use a common helper method to create the preprocess handler.
				//
				//// AS 11/25/08 TFS8265
				//if (null != this._preprocessHandler)
				//    this._preprocessHandler.Dispose();
				this.VerifyHasPreprocessHandler();
            }
		}

        // AS 4/17/09 TFS16807
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private IDisposable CreatePreprocessHandler()
        {
            return new WeakPreprocessMessage(this);
        }
		#endregion // OnDockManagerLoadStateChanged

		// AS 5/8/09 TFS17021
		#region OnEvaluateEndNavigationMode
		private object OnEvaluateEndNavigationMode(object param)
		{
			// if the navigation mode is still enabled for this watcher...
			if (param == _navigationModeFocusWatcher)
			{
				// if we were in navigation mode and focus went to null we would 
				// have gotten here after a delay. if after that delay we don't 
				// have an active pane then we can assume that we should get out
				// of navigation mode
				if (this._activePane == null)
				{
					this.IsInNavigationMode = false;
				}
			}
			return null;
		} 
		#endregion //OnEvaluateEndNavigationMode

        // AS 11/25/08 TFS8265
        #region OnFocusHwndChanged
        private void OnFocusHwndChanged()
        {
            foreach (ContentPane cp in this._panes)
            {
                if (cp.HasFocusInHwndHost())
                {
                    this.OnContentPaneStateChanged(cp);
                    return;
                }
            }

            // AS 2/9/09 TFS13375
            // The focus may have left the dockmanager in which case the active 
            // pane should be cleared.
            //
            //this.VerifyActivePane();
            if (null != _activePane)
                this.OnContentPaneStateChanged(_activePane);
            else
                this.VerifyActivePane();
        }
        #endregion //OnFocusHwndChanged

		#region OnKeyboardFocusedElementChanged
		private void OnKeyboardFocusedElementChanged(object sender, RoutedPropertyChangedEventArgs<IInputElement> e)
		{
			if (null != this._activePane)
				this.OnContentPaneStateChanged(this._activePane);
		}
		#endregion //OnKeyboardFocusedElementChanged

		// AS 5/8/09 TFS17021
		#region OnNavigationModeFocusChanged
		private void OnNavigationModeFocusChanged(object sender, RoutedPropertyChangedEventArgs<IInputElement> e)
		{
			if (e.NewValue == null)
			{
				this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, 
					new DispatcherOperationCallback(OnEvaluateEndNavigationMode), sender);
			}
		} 
		#endregion //OnNavigationModeFocusChanged

		#region OnNavigationModeFocusKeyEvent
		private void OnNavigationModeFocusKeyEvent(object sender, KeyEventArgs e)
		{
			if (e.RoutedEvent == Keyboard.PreviewKeyUpEvent &&
				e.KeyboardDevice.GetKeyStates(Key.LeftAlt) == KeyStates.None &&
				e.KeyboardDevice.GetKeyStates(Key.LeftCtrl) == KeyStates.None &&
				e.KeyboardDevice.GetKeyStates(Key.RightAlt) == KeyStates.None &&
				e.KeyboardDevice.GetKeyStates(Key.RightCtrl) == KeyStates.None)
			{
				this.IsInNavigationMode = false;
			}
		}
		#endregion //OnNavigationModeFocusKeyEvent

		#region OnPaneGotOrLostFocus
		private static void OnPaneGotOrLostFocus(object sender, RoutedEventArgs e)
		{
			if (e.OriginalSource == sender)
			{
				ContentPane pane = sender as ContentPane;
				XamDockManager dockManager = XamDockManager.GetDockManager(pane);

				if (null != dockManager)
					dockManager.ActivePaneManager.OnContentPaneStateChanged(pane);
			}
		}
		#endregion //OnPaneGotOrLostFocus

		#region OnPaneLocationChanged
		internal void OnPaneLocationChanged(ContentPane pane, PaneLocation oldLocation, PaneLocation newLocation)
		{
			// when a pane becomes/leaves the document area then we need to update the active document


			if (pane == this._activeDocument && newLocation != PaneLocation.Document)
			{
				// the active document is always the active document regardless of keyboard focus
				// the only time you won't have an active document is if there are no documents
				// when the panelocation of a contentpane has changed

				// if the pane is the active pane and it was a document then change the active
				// document to the next document
				
				this.VerifyActivePane();
			}
			else if (pane == this._activePane && newLocation == PaneLocation.Document)
			{
				// update activedocument
				this.SetActivePaneImpl(this._activePane, this._activePane);
			}
			else if (this._activeDocument == null && newLocation == PaneLocation.Document)
			{
				this.VerifyActiveDocument(false);
			}
		}
		#endregion // OnPaneLocationChanged

		#region OnToolWindowActivationChanged
        // AS 3/30/09 TFS16355 - WinForms Interop
        //internal void OnToolWindowActivationChanged(PaneToolWindow window)
        internal void OnToolWindowActivationChanged(ToolWindow window)
        {
			this.OnWindowActivationChanged();
		}
		#endregion //OnToolWindowActivationChanged

		#region OnWindowActivationChanged
		private void OnWindowActivationChanged()
		{
			this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new DockManagerUtilities.MethodInvoker(this.OnWindowActivationChangedImpl));
		}

		private void OnWindowActivationChangedImpl()
		{
			// when the main window loses focus...
			if (this.IsDockManagerWindowActive == false)
			{
				if (null != this._activePane)
					this.OnContentPaneStateChanged(this._activePane);
			}
			else // the main window has gained the input focus
			{
				// if we get the input focus in the window and we don't have an active pane
				// then try to ascertain one based on the logical focus of the dm's 
				// focusscope
				if (null == this._activePane)
				{
					ContentPane pane = this.GetFocusedPane(this._dockManager);

					if (null != pane)
						this.OnContentPaneStateChanged(pane);
				}
			}
		}
		#endregion // OnWindowActivationChanged

		#region Output
		[Conditional("DEBUG_ACTIVATION")]
		private static void Output(object value, string category)
		{
			Debug.WriteLine(value, category);
		}

		[Conditional("DEBUG_ACTIVATION")]
		private static void OutputIf(bool condition, object value, string category)
		{
			if (condition)
				Output(value, category);
		}
		#endregion //Output

		#region SetActivePane
		private void SetActivePane(ContentPane newActivePane)
		{
			if (newActivePane == null && this._activePane != null)
			{
				// we want to wait temporarily in case another pane is about to be activated
				// AS 9/8/09 TFS21921
				// Track the dispatcher operation so we can abort it when the active pane is changed 
				// or if someone tries to activate the currently active pane in the interim.
				//
				//this._dockManager.Dispatcher.BeginInvoke(DispatcherPriority.Send, new DockManagerUtilities.PaneMethodInvoker(this.DelayedClearActivePane), this._activePane);
				if (null == _pendingClearActivePane || _pendingClearActivePane.Status == DispatcherOperationStatus.Completed)
					_pendingClearActivePane = this._dockManager.Dispatcher.BeginInvoke(DispatcherPriority.Send, new DockManagerUtilities.PaneMethodInvoker(this.DelayedClearActivePane), this._activePane);

				return;
			}

			this.SetActivePaneImpl(newActivePane, newActivePane != null && newActivePane.IsDocument ? newActivePane : this._activeDocument);
		}

		private void DelayedClearActivePane(ContentPane oldActivePane)
		{
			// if the pane to be deactivated is still active...
			// AS 3/26/08
			// Use the new helper method to verify the active pane is valid.
			//
			//if (oldActivePane == this._activePane)
			//	this.SetActivePaneImpl(null);
			// AS 3/28/08
			// Actually we do want to clear the active pane if it doesn't have the keyboard focus. Otherwise
			// we will not deactivate the pane when focus really has gone to the root focusscope.
			//this.VerifyActivePane();
			if (oldActivePane == this._activePane
				&& this._activePane != null
				&& XamDockManager.GetDockManager(oldActivePane) == this._dockManager
				&& GetIsKeyboardFocusWithinEx(oldActivePane) == false)
			{
				Debug.Assert(!oldActivePane.HasFocusInHwndHost());

				this.SetActivePaneImpl(null, this._activeDocument);
			}
			else
				this.VerifyActivePane();
		}


		private void SetActivePaneImpl(ContentPane newActivePane, ContentPane newActiveDocument)
		{
			#region Setup

			// AS 9/8/09 TFS21921
			// If we were going to clear the active pane but were waiting to see if another 
			// pane would be active first, cancel that pending operation now since we have 
			// an explicit activation.
			//
			if (_pendingClearActivePane != null)
			{
				_pendingClearActivePane.Abort();
				_pendingClearActivePane = null;
			}

			Debug.Assert(newActivePane == null || this._dockManager == XamDockManager.GetDockManager(newActivePane));

			ContentPane oldActivePane = this._activePane;
			ContentPane oldActiveDocument = this._activeDocument;
			DocumentContentHost contentHost = this._dockManager.DocumentContentHost;

			bool isNewActivePane = newActivePane != oldActivePane;
			bool isNewActiveDocument = newActiveDocument != oldActiveDocument;

			if (false == isNewActivePane && false == isNewActiveDocument)
				return;

			#endregion //Setup

			#region Update Local Members

			Output(string.Format("Old: {0} New:{1}", oldActivePane, newActivePane), "ActivePane Changed");
			OutputIf(isNewActiveDocument, string.Format("Old: {0} New:{1}", oldActiveDocument, newActiveDocument), "ActiveDocument Changed");

			// update the local members first
			this._activePane = newActivePane;

			if (isNewActiveDocument)
				this._activeDocument = newActiveDocument;

			#endregion //Update Local Members

			#region Move the pane in the ActiveList

			if (null != newActivePane && isNewActivePane)
				this.MovePaneToHead(newActivePane);
			// AS 1/12/11 TFS61435
			// if we are activating a document while we have no active pane and the previous active 
			// document was the last active pane then we need to make this document the head of the 
			// list so it will get activated when the dm next gets focus
			//
			else if (newActiveDocument != null && oldActivePane == null && newActivePane == null && _panes.Count > 0 && oldActiveDocument == _panes.First.Value)
			{
				this.MoveToFront(newActiveDocument);
			}

			#endregion //Move the pane in the ActiveList

			#region Update Local Dep. Props

			// update local dep props
			this.SetValue(ActivePanePropertyKey, newActivePane);

			if (isNewActiveDocument)
				this.SetValue(ActiveDocumentPropertyKey, newActiveDocument);

			#endregion //Update Local Dep. Props

			#region Initialize IsActive Pane|Document properties
			// update the state of the panes
			if (isNewActiveDocument)
			{
				if (oldActiveDocument != null)
					oldActiveDocument.ClearValue(IsActiveDocumentPropertyKey);

				if (null != newActiveDocument)
					newActiveDocument.SetValue(IsActiveDocumentPropertyKey, KnownBoxes.TrueBox);
			}

			if (isNewActivePane)
			{
				if (oldActivePane != null)
					oldActivePane.ClearValue(IsActivePanePropertyKey);

				if (newActivePane != null)
					newActivePane.SetValue(IsActivePanePropertyKey, KnownBoxes.TrueBox);
			}

			#endregion //Initialize IsActive Pane|Document properties

			#region Set Control Properties

			// update properties on the controls
			if (isNewActiveDocument && null != contentHost)
				contentHost.SetValue(ActiveDocumentPropertyKey, newActiveDocument);

			this._dockManager.SetValue(ActivePanePropertyKey, newActivePane);

			#endregion //Set Control Properties

			#region Clear Old ActivePane

			// AS 2/2/10 TFS26919
			// The old activepane's IsKeyboardFocusWithinExPropertyKey may still be true (and it 
			// may even have our panewatcher associated with it). Since we know its no longer focused 
			// we can clear its state. Since the IsKeyboardFocusWithinEx is still true we may skip 
			// a call to Activate because we think its already active.
			//
			if (isNewActivePane && null != oldActivePane)
				oldActivePane.ClearValue(IsKeyboardFocusWithinExPropertyKey); 

			#endregion //Clear Old ActivePane

			#region Update UI For New/Old ActivePane

			if (isNewActivePane)
			{
				ContentPane flyoutPaneToHide = oldActivePane;

				var currentFlyout = _dockManager.CurrentFlyoutPane;

				// AS 11/14/11 TFS96013
				// It is possible that focus went outside the app (or to an hwndhost outside 
				// the XDM) and then when we get an active pane later we should make sure to 
				// close the current flyout if it is for a different pane.
				// 
				if (flyoutPaneToHide == null &&
					currentFlyout != null &&
					currentFlyout != _dockManager.ActivePane)
				{
					flyoutPaneToHide = _dockManager.CurrentFlyoutPane;
				}

				// AS 11/14/11 TFS96013
				// Keyboard.FocusedElement will be null if focus is within an HwndHost so we'll 
				// verify this by checking our GetKeyboardFocusedPane which contains the HwndHosts 
				// within our ContentPanes.
				//
				//if (Keyboard.FocusedElement == null)
				if (Keyboard.FocusedElement == null && this.GetKeyboardFocusedPane() == null)
				{
					// focus is not within the app so keep the pane open
					flyoutPaneToHide = null;
				}
				else
				{
					// if we don't have an old pane, see if the one we cached is still the active flyout pane
					if (flyoutPaneToHide == null && this._lastActivePane != null)
					{
						flyoutPaneToHide = Utilities.GetWeakReferenceTargetSafe(this._lastActivePane) as ContentPane;

						// if the pane got activated again then don't hide the flyout
						if (flyoutPaneToHide == newActivePane)
							flyoutPaneToHide = null;

						this._lastActivePane = null;
					}
				}

				if (null != flyoutPaneToHide)
				{
					this._dockManager.HideFlyout(flyoutPaneToHide, false, true, true, false);
					this._lastActivePane = null;
				}

				if (null != oldActivePane && null == newActivePane && Keyboard.FocusedElement == null)
					this._lastActivePane = new WeakReference(oldActivePane);

				if (newActivePane != null && newActivePane == this._activePane)
				{
					switch (XamDockManager.GetPaneLocation(newActivePane))
					{
						case PaneLocation.Unpinned:
							// AS 5/14/08 BR32037
							// Depending on how the pane was activated, we don't necessarily want to show
							// the flyout. For example, if another pane was active and we made this the 
							// active pane because it received keyboard focus, we don't want to show the
							// flyout since VS doesn't.
							//
							// AS 5/5/10 TFS29178
							//if (false == newActivePane.SuppressFlyoutDuringActivate)
							if (false == newActivePane.SuppressFlyoutDuringActivate && !_dockManager.IsShowFlyoutSuspended)
								this._dockManager.ShowFlyout(newActivePane, false, false);
							break;
						case PaneLocation.Floating:
						case PaneLocation.FloatingOnly:
							PaneToolWindow toolWindow = ToolWindow.GetToolWindow(newActivePane) as PaneToolWindow;

							if (null != toolWindow)
								toolWindow.LastActivePane = newActivePane;
							break;
					}
				}
			}
			#endregion //Update UI For New ActivePane

            // AS 2/9/09 TFS13375
            // Since a ContentPane is a FocusScope by default, we "transfer" command canexecute and 
            // execute events to the focused element within the pane. However we can only do this if 
            // the dockmanager receives the events. It will only receive the events if its in the 
            // tree of the FocusedElement. The DM sets the FocusedElement of its containing focus 
            // scope if the element receiving the keyboard focus is within a different focus scope 
            // but it did this within the OnGotKeyboardFocus handler. Unfortunately this isn't called 
            // when an HwndHost gets focus because the Keyboard.FocusedElement goes to null because 
            // another window besides the wpf window has the os input focus. Therefore when we make 
            // a pane active and that pane has a focused hwndhost we will do an additional verification 
            // that the DM is the focused element of its focus scope.
            //
            if (isNewActivePane && null != newActivePane)
            {
                FrameworkElement focusedHwndHost = newActivePane.GetFocusedHwndHost();

				// AS 12/1/11 TFS96843
				// Found while debugging/fixing this issue. Basically we should pass along an element 
				// from a floating pane.
				//
				if (null != focusedHwndHost && Utilities.IsDescendantOf(_dockManager, focusedHwndHost, true))
                    _dockManager.EnsureIsFocusedElement(focusedHwndHost);
            }

			// AS 12/1/11 TFS96843
			// This isn't absolutely necessary for this fix but I think its better to ensure that 
			// we don't get into that situation again in case the pane became active because an 
			// element was explicitly given focus instead of going through our activate method.
			//
			if (isNewActivePane && 
				_activePane == newActivePane && 
				newActivePane != null && 
				DockManagerUtilities.IsFloating(newActivePane.PaneLocation))
			{
				var paneToolWindow = ToolWindow.GetToolWindow(newActivePane) as PaneToolWindow;

				if (paneToolWindow != null && paneToolWindow.IsActive)
				{
					paneToolWindow.ClearDragHelperFocusedElement();
				}
			}

			#region Raise Events

			// raise events
			if (isNewActiveDocument && null != contentHost)
				contentHost.RaiseActiveDocumentChanged(new RoutedPropertyChangedEventArgs<ContentPane>(oldActiveDocument, newActiveDocument));

			if (isNewActivePane)
				this._dockManager.RaiseActivePaneChanged(new RoutedPropertyChangedEventArgs<ContentPane>(oldActivePane, newActivePane));

			#endregion //Raise Events
		}
		#endregion // SetActivePane

		#region VerifyDockManagerWindowBindings
		private void VerifyDockManagerWindowBindings()
		{
			Window window = Window.GetWindow(this._dockManager);

			if (window == null)
				BindingOperations.ClearBinding(this, IsDockManagerWindowActiveProperty);
			else
				BindingOperations.SetBinding(this, IsDockManagerWindowActiveProperty, Utilities.CreateBindingObject(Window.IsActiveProperty, BindingMode.OneWay, window));
		}
		#endregion // VerifyDockManagerWindowBindings

		#region VerifyActiveDocument
		internal void VerifyActiveDocument(bool force)
		{
			if (this._dockManager.IsLoaded || force)
			{
				ContentPane newActiveDocument = this._activeDocument;
				DocumentContentHost host = this._dockManager.DocumentContentHost;

				if (null != host)
				{
					// this will be true when the form is closing
					//Debug.Assert(this._activeDocument == null || this._activeDocument.IsVisible);

					// if there is no currently active document...
					if (newActiveDocument == null)
					{
						ContentPane focusedElement = this.GetFocusedPane(host);

						// if there is a focused element within the focus scope use that
						if (focusedElement != null && focusedElement.IsDocument)
							newActiveDocument = focusedElement;
						else
						{
							// otherwise find the first selected tab
							ContentPane firstPane = null;

							while (null != (newActiveDocument = this.GetPaneInActivationOrder(newActiveDocument, firstPane == null, true, PaneFilterFlags.Document, true)))
							{
								// if we wrapped around then we don't have one
								if (newActiveDocument == firstPane)
								{
									newActiveDocument = null;
									break;
								}

								if (null == firstPane)
									firstPane = newActiveDocument;

								if (newActiveDocument.IsVisible == false)
									continue;

								DependencyObject container = LogicalTreeHelper.GetParent(newActiveDocument);

								if (container is TabGroupPane)
								{
									TabGroupPane group = container as TabGroupPane;

									TabItem tab = group.ItemContainerGenerator.ContainerFromItem(newActiveDocument) as TabItem;

									if (tab != null && tab.IsSelected)
										break;
								}
								else
								{
									Debug.Assert(container is SplitPane);
									break;
								}
							};
						}
					}
				}
				else
				{
					Debug.Assert(this._activeDocument == null);
				}

				if (this._activeDocument != newActiveDocument)
				{
					this.SetActivePaneImpl(this._activePane, newActiveDocument);
				}
			}
		}
		#endregion //VerifyActiveDocument

		// AS 6/10/11 TFS74484
		#region VerifyTabIsSelected
		private void VerifyTabIsSelected(ContentPane pane)
		{
			if (null == pane)
				return;

			TabGroupPane tgp = LogicalTreeHelper.GetParent(pane) as TabGroupPane;

			if (null == tgp)
				return;

			int index = tgp.Items.IndexOf(pane);

			Debug.Assert(index >= 0, "Pane returns group as logical parent but not in the collection?");

			tgp.SelectedIndex = index;
		}
		#endregion //VerifyTabIsSelected

		#endregion //Private Methods

		#endregion //Methods

		#region PaneEnumerator class
		private class PaneEnumerator : IEnumerator<ContentPane>
		{
			#region Member Variables

			private ContentPane _current;
			private ActivePaneManager _manager;
			private bool? _isComplete;
			private PaneFilterFlags _filter;
			private PaneNavigationOrder _order;
			private bool _ensureActivatable;

			#endregion //Member Variables

			#region Constructor
			internal PaneEnumerator(ActivePaneManager manager)
				: this(manager, PaneFilterFlags.AllVisible)
			{
			}

			internal PaneEnumerator(ActivePaneManager manager, PaneFilterFlags filter)
				: this(manager, filter, manager._dockManager.NavigationOrder)
			{
			}

			internal PaneEnumerator(ActivePaneManager manager, PaneFilterFlags filter, PaneNavigationOrder navigationOrder) 
				: this(manager, filter, navigationOrder, true)
			{
			}

			internal PaneEnumerator(ActivePaneManager manager, PaneFilterFlags filter, PaneNavigationOrder navigationOrder, bool ensureActivatable)
			{
				DockManagerUtilities.ThrowIfNull(manager, "manager");

				this._manager = manager;
				this._filter = filter;
				this._order = navigationOrder;
				this._ensureActivatable = ensureActivatable;
			}
			#endregion //Constructor

			#region IEnumerator<ContentPane> Members
			public ContentPane Current
			{
				get
				{
					if (this._isComplete == null)
						throw new InvalidOperationException(); // not started

					if (this._isComplete.Value)
						throw new InvalidOperationException(); // reached end

					return this._current;
				}
			}
			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				this._current = null;
			}

			#endregion //IDisposable

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get { return this.Current; }
			}

			public bool MoveNext()
			{
				if (false == this._isComplete.HasValue || this._isComplete.Value == false)
				{
					this._current = this._manager.GetNextActivePane(this._current, true, this._filter, this._order, false, this._ensureActivatable);
					this._isComplete = this._current == null;
				}

				return this._isComplete.Value == false;
			}

			public void Reset()
			{
				this._isComplete = null;
				this._current = null;
			}

			#endregion // IEnumerator Members
		} 
		#endregion //PaneEnumerator class

		#region LastActivatedTimeComparer
		private class LastActivatedTimeComparer : IComparer<ContentPane>
		{
			#region Member Variables

			internal static readonly IComparer<ContentPane> Instance = new LastActivatedTimeComparer();

			#endregion //Member Variables

			#region Constructor
			private LastActivatedTimeComparer()
			{
			}
			#endregion //Constructor

			#region IComparer<ContentPane> Members

			public int Compare(ContentPane x, ContentPane y)
			{
				if (x == y && x == null)
					return 0;
				else if (x == null)
					return -1;
				else if (y == null)
					return 1;

				return x.LastActivatedTime.CompareTo(y.LastActivatedTime);
			}

			#endregion // IComparer<ContentPane> Members
		} 
		#endregion //LastActivatedTimeComparer

        // AS 11/25/08 TFS8265
        #region WeakPreprocessMessage
        private class WeakPreprocessMessage : WeakReference
            // AS 4/17/09 TFS16807
            , IDisposable
        {
            #region Member Variables

            private IntPtr _focusHwnd;
            //AS 4/17/09 TFS16807
            //private static bool _canSinkEvent = true;

            #endregion //Member Variables

            #region Constructor
            //AS 4/17/09 TFS16807
            //private WeakPreprocessMessage(ActivePaneManager manager)
            internal WeakPreprocessMessage(ActivePaneManager manager)
                : base(manager)
            {
                ComponentDispatcher.ThreadPreprocessMessage += new ThreadMessageEventHandler(OnThreadPreprocessMessage);
            }
            #endregion //Constructor

            #region Methods

            [System.Security.SuppressUnmanagedCodeSecurity]
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern IntPtr GetFocus();

            #region Create
            
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

            #endregion //Create

            #region OnThreadPreprocessMessage
            private void OnThreadPreprocessMessage(ref System.Windows.Interop.MSG msg, ref bool handled)
            {
                IntPtr focus = GetFocus();

                if (focus != _focusHwnd)
                {
                    // AS 3/12/09
                    //ActivePaneManager apm = this.Target as ActivePaneManager;
                    ActivePaneManager apm = Utilities.GetWeakReferenceTargetSafe(this) as ActivePaneManager;

                    if (null != apm)
                    {
                        _focusHwnd = focus;
                        apm.Dispatcher.BeginInvoke(DispatcherPriority.Send, new DockManagerUtilities.MethodInvoker(apm.OnFocusHwndChanged));
                    }
                    else
                    {
                        this.Dispose();
                    }
                }
            }
            #endregion //OnThreadPreprocessMessage

            #region Dispose
            public void Dispose()
            {
                
                
                
                
                
                
                
                
                try
                {
                    UIPermission perm = new UIPermission(PermissionState.Unrestricted);

					try
					{
						perm.Assert();
					}
					catch (InvalidOperationException)
					{
					}

                    ComponentDispatcher.ThreadPreprocessMessage -= new ThreadMessageEventHandler(OnThreadPreprocessMessage);
                }
                catch (System.Security.SecurityException)
                {
                }
            }
            #endregion //Dispose

            #endregion //Methods
        }
        #endregion //WeakPreprocessMessage

        // AS 3/3/09 TFS10656
        // Previously we were relying on the Keyboard.LostKeyboardFocusEvent bubbling up to the 
        // ContentPane and then based on where focus was shifted, clearing the IsKeyboardFocusWithinEx
        // property. However, if the element with focus (or something between it and the CP) is removed 
        // from the tree then we weren't getting the notification and that flag was left set to true. 
        // This caused problems since the ActivePaneManager (and the ContentPane's IsKeyboardFocusWithinEx 
        // property) were then led to believe that it still had focus.
        //
        #region PaneFocusWatcher
        private class PaneFocusWatcher : FocusWatcher
        {
            #region Member Variables

            private ContentPane _pane; 

            #endregion //Member Variables

            #region Constructor
            internal PaneFocusWatcher(ContentPane pane)
            {
                DockManagerUtilities.ThrowIfNull(pane, "pane");
                _pane = pane;
            } 
            #endregion //Constructor

            #region Base class overrides
            protected override void OnFocusElementChanged(IInputElement oldElement, IInputElement newElement)
            {
                XamDockManager newDockManager = newElement is DependencyObject ? XamDockManager.GetDockManager((DependencyObject)newElement) : null;
                XamDockManager oldDockManager = XamDockManager.GetDockManager(_pane);

                bool hasLostFocus = true;

                // if focus is not within a dockmanager then we know the pane can't be focused any more
                if (newDockManager != null)
                {
                    // likewise if the old pane is no longer associated with a dockmanager assume it lost focus
                    if (null != oldDockManager)
                    {
                        ContentPane newContentPane = ActivePaneManager.GetDockManagerPane(newElement, oldDockManager);
                        hasLostFocus = newContentPane != _pane;
                    }
                }

                if (hasLostFocus)
                {
                    Output(_pane, "LostKeyboardFocus");

                    _pane.ClearValue(IsKeyboardFocusWithinExPropertyKey);
                }

                base.OnFocusElementChanged(oldElement, newElement);
            }
            #endregion //Base class overrides
        }
        #endregion //PaneFocusWatcher
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