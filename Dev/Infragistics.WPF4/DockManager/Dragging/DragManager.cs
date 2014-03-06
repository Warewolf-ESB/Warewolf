//#define DEBUG_DRAGGING




using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Infragistics.Windows.Controls;
using System.Windows.Controls;
using Infragistics.Windows.DockManager.Events;
using System.Collections.ObjectModel;
using System.Windows.Interop;
using System.Collections.Specialized;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Themes;
using System.Collections;
using Infragistics.Shared;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Security;

namespace Infragistics.Windows.DockManager.Dragging
{
	/// <summary>
	/// Helper class for managing a drag operation within a <see cref="XamDockManager"/>
	/// </summary>
	internal class DragManager
	{
		#region Member Variables

		private XamDockManager _dockManager;
		private DragState _dragState;
		private Point? _mouseDownLocation;
		private FrameworkContentElement _mouseCaptureHelper;
		private ToolWindow[] _indicatorWindows;
        // AS 3/30/09 TFS16355 - WinForms Interop
		//private static readonly ControlTemplate IndicatorToolWindowTemplate;
        private static readonly ControlTemplate IndicatorToolWindowTemplate;
        private static readonly ControlTemplate IndicatorToolWindowTemplateWhite;

		private DockingIndicator _lastHotTrackIndicator;
		private ReadOnlyCollection<ContentPane> _panesBeingDragged;
		private KeyEventFocusWatcher _keyFocusWatcher;
		private AllowedDropLocations _allowedLocations;
		private bool _isDraggingDocuments;
		private DropInfo _dropInfo;

		private PaneToolWindow _windowBeingDragged;
		private UIElement _initialDragElement;
		private FrameworkElement _rootDragElement;
		private PaneToolWindow _rootDragElementToolWindow; // AS 9/20/11 TFS88634
		private ToolWindow _dropPreviewWindow;

		private Dictionary<FrameworkElement, IndicatorEnabledState> _indicatorStates;
		private IndicatorEnabledState _globalIndicatorState;
		private Dictionary<PaneDragAction, DragResult> _dragResults;

		// AS 5/17/08 BR32810
		private IInputElement _lastFocusedElement;
		private ContentPane _lastActivePane;

		// AS 5/28/08 RaisePaneDragOverForInvalidLocations
		private Cursor _defaultValidCursor;
		private Cursor _defaultInvalidCursor;
		private bool _raisePaneDragOverForInvalidLocations;

		// AS 5/29/08 BR33471
		private bool _bringWindowToFront;

        // AS 3/13/09 FloatingWindowDragMode
        private bool _dragFullWindows;
        private ToolWindow _floatingPreviewWindow;
        private bool _canRaiseDragEvents;
        private bool _forceFloating;

		// AS 11/12/09 TFS24789 - TabItemDragBehavior
		private TabItemDragBehavior _tabItemDragBehavior;
		private ToolWindow _tabItemDropPreviewWindow;

		private const bool DebugTabItemPreview = false;

		// AS 10/13/11 TFS91945
		private bool RaisePaneDragOverInternal;

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		private bool _isWindowDragMode;
		private bool _deferFloatingWindowPositioning;
		private System.Threading.Timer _windowDragModeKeyTimer;
		private DragMoveCache _dragMoveCache;

		// AS 5/7/11 TFS106302
		private HashSet _windowsPendingClose;

		#endregion //Member Variables

		#region Constructor
		static DragManager()
		{
			// create the ToolWindow template without chrome for the drop indicators
            FrameworkElementFactory fefRoot = new FrameworkElementFactory(typeof(ContentPresenter));
            fefRoot.Name = "PART_Content";
            ControlTemplate template = new ControlTemplate(typeof(ToolWindow));
            template.VisualTree = fefRoot;
            template.Seal();
            IndicatorToolWindowTemplate = template;

            // AS 3/30/09 TFS16355 - WinForms Interop
            // When a Popup is displayed as a child control instead of a top level window - 
            // which happens when used in a browser - the Popup does not support transparency.
            // So we need to provide a background element in that case that has a backcolor 
            // or else you will see the unrendered/uninitialized areas of the popup's hwnd
            // as black (or corrupted).
            //
            FrameworkElementFactory fefRoot2 = new FrameworkElementFactory(typeof(Grid));
            FrameworkElementFactory fef2 = new FrameworkElementFactory(typeof(ContentPresenter));
            fef2.Name = "PART_Content";
            fefRoot2.SetValue(Control.BackgroundProperty, System.Windows.Media.Brushes.White);
            fefRoot2.AppendChild(fef2);
            ControlTemplate template2 = new ControlTemplate(typeof(ToolWindow));
            template2.VisualTree = fefRoot2;
            template2.Seal();
            IndicatorToolWindowTemplateWhite = template2;
		}

		internal DragManager(XamDockManager dockManager)
		{
			DockManagerUtilities.ThrowIfNull(dockManager, "dockManager");
			_dockManager = dockManager;
		} 
		#endregion //Constructor

		#region Properties

		#region CurrentDragPaneSize
		internal Size CurrentDragPaneSize
		{
			get
			{
				FrameworkElement relativeElement = this._windowBeingDragged ?? this._rootDragElement;
				Debug.Assert(null != relativeElement);
				// AS 5/2/08 BR32290
				// We need to include the non-client size since that's what VS does.
				//
				//return new Size(relativeElement.ActualWidth, relativeElement.ActualHeight);
				Size size = new Size(relativeElement.ActualWidth, relativeElement.ActualHeight);

				// AS 6/12/12 TFS114254
				// This Size is used for a new root pane. If we add in the non-client size then that could 
				// make it bigger than what it was when it was docked since the floating size didn't 
				// include the non-client size.
				//
				//if (null != this._windowBeingDragged)
				//	size = this._windowBeingDragged.AddNonClientSize(size);

				return size;
			}
		} 
		#endregion //CurrentDragPaneSize

		#region DockManager
		internal XamDockManager DockManager
		{
			get { return this._dockManager; }
		} 
		#endregion //DockManager

		#region DragState
		internal DragState DragState
		{
			get { return this._dragState; }
		} 
		#endregion //DragState

		#region DropPreviewWindow
		private ToolWindow DropPreviewWindow
		{
			get
			{
				if (this._dropPreviewWindow == null)
				{
					Debug.Assert(this._dragState == DragState.Dragging);

                    // AS 3/13/09 FloatingWindowDragMode
                    // Created helper method to remove duplicate code when 
                    // creating a DragToolWindow.
                    //
                    //ToolWindow window = new DragToolWindow();
                    ToolWindow window = DragToolWindow.Create(this._dockManager);

					// AS 4/23/08
					// the preview doesn't need to be topmost. making it so actually makes
					// things more complicated because then we need to try to bring the indicators
					// to the front. the preview should end up being above the floating windows since
					// it is shown after they are shown
					//window.Topmost = true;

                    // AS 3/13/09 FloatingWindowDragMode
                    //window.ResizeMode = ResizeMode.NoResize;
					//window.UseOSNonClientArea = false;
					//window.Template = IndicatorToolWindowTemplate;
					//window.SetValue(XamDockManager.DockManagerPropertyKey, this._dockManager);
					window.HorizontalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;
					window.VerticalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;
					window.IsHitTestVisible = false;
                    // AS 3/13/09 FloatingWindowDragMode
                    //// AS 5/21/08
                    //// Instead of binding the theme property, we will add the resources of the 
                    //// dockmanager to the window's resources. That will pick up the theme property
                    //// resources and any other resources they put in.
                    ////
                    ////window.SetBinding(ToolWindow.ThemeProperty, Utilities.CreateBindingObject(XamDockManager.ThemeProperty, System.Windows.Data.BindingMode.OneWay, this._dockManager));
                    //window.Resources.MergedDictionaries.Add(this._dockManager.Resources);

					Control dropPreview = new Control();
					dropPreview.HorizontalAlignment = HorizontalAlignment.Stretch;
					dropPreview.VerticalAlignment = VerticalAlignment.Stretch;
					dropPreview.SetResourceReference(Control.StyleProperty, XamDockManager.DropPreviewStyleKey);
					
					// AS 5/29/08 BR33471
					// Since we are doing a bring to front on the window being dragged, it
					// can end up going above the drop preview. Also, in vs, the drop preview
					// when over the dm can still be above the floating panes so this probably
					// makes sense anyway. So to handle these, we will make this topmost.
					// We'll have to bring the indicators to the top though when the drop 
					// preview is shown.
					//
					window.Topmost = true;

					window.Content = dropPreview;

					this._dropPreviewWindow = window;
				}

				return this._dropPreviewWindow;
			}
		} 
		#endregion //DropPreviewWindow

        // AS 3/13/09 FloatingWindowDragMode
        #region FloatingWindow
        internal ToolWindow FloatingWindow
        {
            get
            {
                if (this.DeferFloatingWindowPositioning)
                    return _floatingPreviewWindow;

                return this.WindowBeingDragged;
            }
        } 
        #endregion //FloatingWindow

        // AS 3/13/09 FloatingWindowDragMode
        #region ForceFloating
		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		//private bool ForceFloating
		//{
		//    get { return _forceFloating || IsControlKeyDown; }
		//} 
        #endregion //ForceFloating

		#region GlobalIndicatorsElement
		internal FrameworkElement GlobalIndicatorsElement
		{
			get
			{
				if (this._isDraggingDocuments)
				{
					return this._dockManager.DocumentContentHost;
				}

				return this._dockManager.DockPanel ?? (FrameworkElement)this._dockManager;
			}
		}
		#endregion //GlobalIndicatorsElement

		#region IsControlKeyDown
		private static bool IsControlKeyDown
		{
			get { return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control; }
		}
		#endregion //IsControlKeyDown

		#region IsDragging
		/// <summary>
		/// Indicates if a pane is currently being dragged.
		/// </summary>
		internal bool IsDragging
		{
			get { return this._dragState == DragState.Dragging; }
		}
		#endregion //IsDragging

		#region IsDragPending
		/// <summary>
		/// Indicates if the manager is awaiting the start of a drag operation.
		/// </summary>
		internal bool IsDragPending
		{
			get { return this._dragState == DragState.Pending; }
		}
		#endregion //IsDragPending

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region IsInWindowDragMode
		internal bool IsInWindowDragMode
		{
			get { return _isWindowDragMode; }
			set
			{
				if (value != _isWindowDragMode)
				{
					_isWindowDragMode = value;

					if (value && _windowDragModeKeyTimer == null && !_forceFloating)
					{
						_windowDragModeKeyTimer = new System.Threading.Timer(new System.Threading.TimerCallback(OnWindowDragModeKeyTimerTick), null, 0, 250);
					}
					else if (!value && _windowDragModeKeyTimer != null)
					{
						_windowDragModeKeyTimer.Dispose();
						_windowDragModeKeyTimer = null;
					}
				}
			}
		}
		#endregion //IsInWindowDragMode

		#region OriginalRootContainer
		internal FrameworkElement OriginalRootContainer
		{
			get { return this._rootDragElement; }
		}
		#endregion //OriginalRootContainer

		#region PanesBeingDragged
		internal ReadOnlyCollection<ContentPane> PanesBeingDragged
		{
			get { return this._panesBeingDragged; }
		} 
		#endregion //PanesBeingDragged

		// AS 11/12/09 TFS24789 - TabItemDragBehavior
		#region TabItemDragBehavior
		internal TabItemDragBehavior TabItemDragBehavior
		{
			get
			{
				return _tabItemDragBehavior;
			}
		} 
		#endregion //TabItemDragBehavior

		// AS 11/12/09 TFS24789 - TabItemDragBehavior
		// Use a separate toolwindow that contains a DropIndicator to provide the insertion bar 
		// between tab items.
		#region TabItemDropPreviewWindow
		private ToolWindow TabItemDropPreviewWindow
		{
			get
			{
				if (this._tabItemDropPreviewWindow == null)
				{
					Debug.Assert(this._dragState == DragState.Dragging);

					ToolWindow window = DragToolWindow.Create(this._dockManager);

					window.HorizontalAlignmentMode = ToolWindowAlignmentMode.Manual;
					window.VerticalAlignmentMode = ToolWindowAlignmentMode.Manual;
					window.IsHitTestVisible = false;

					Control dropPreview = new DropIndicator();
					dropPreview.HorizontalAlignment = HorizontalAlignment.Stretch;
					dropPreview.VerticalAlignment = VerticalAlignment.Stretch;

					// AS 5/29/08 BR33471
					// Since we are doing a bring to front on the window being dragged, it
					// can end up going above the drop preview. Also, in vs, the drop preview
					// when over the dm can still be above the floating panes so this probably
					// makes sense anyway. So to handle these, we will make this topmost.
					// We'll have to bring the indicators to the top though when the drop 
					// preview is shown.
					//
					window.Topmost = true;

					window.Content = dropPreview;

					Debug.Assert(_tabItemDropPreviewWindow == null);
					_tabItemDropPreviewWindow = window;

					DockManagerUtilities.ApplyTemplateRecursively(window);
				}

				return this._tabItemDropPreviewWindow;
			}
		}
		#endregion //TabItemDropPreviewWindow

		// AS 3/13/09 FloatingWindowDragMode
        #region DeferFloatingWindowPositioning
        internal bool DeferFloatingWindowPositioning
        {
			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			//get { return !_dragFullWindows; }
            get { return _deferFloatingWindowPositioning; }
        } 
        #endregion //DeferFloatingWindowPositioning

		#region WindowBeingDragged
		internal PaneToolWindow WindowBeingDragged
		{
			get { return this._windowBeingDragged; }
		} 
		#endregion //WindowBeingDragged

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region CloneRootDragElement
        // AS 10/15/08 TFS8068
        // We need to maintain the datacontext on the elements being reparented until
        // the new split is put into the dockmanager's panes collection.
        //
        //internal SplitPane CloneRootDragElement(PaneLocation newLocation)
		// AS 4/28/11 TFS73532
		// Instead of deferring adding the split that is created to the caller we will do it but allow 
		// the caller to provide a delegate that will be invoked when the split pane is created.
		//
		//internal IDisposable CloneRootDragElement(PaneLocation newLocation, out SplitPane newSplit)
        internal SplitPane CloneRootDragElement(PaneLocation newLocation, Action<SplitPane> initializeNewSplit)
		{
			
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


			// AS 4/28/11 TFS73532
			// Reorganized to better deal with the top-down building that must occur.
			//
			#region Refactored
			//// AS 10/15/08 TFS8068
			//IDisposable replacement = null;
			//
			//if (this._windowBeingDragged != null)
			//{
			//    // AS 10/15/08 TFS8068
			//    //return DockManagerUtilities.Clone(this._windowBeingDragged.Content as SplitPane, newLocation);
			//    replacement = DockManagerUtilities.Clone(this._windowBeingDragged.Content as SplitPane, newLocation, out newSplit);
			//}
			//else if (this._rootDragElement is ContentPane)
			//{
			//    // AS 10/15/08 TFS8068
			//    //SplitPane split = DockManagerUtilities.CreateSplitPane();
			//    //DockManagerUtilities.MovePane((ContentPane)this._rootDragElement, split, null, newLocation);
			//    //return split;
			//    newSplit = DockManagerUtilities.CreateSplitPane(_dockManager);
			//    replacement = DockManagerUtilities.CreateMoveReplacement(newSplit, this._rootDragElement);
			//    DockManagerUtilities.MovePane((ContentPane)this._rootDragElement, newSplit, null, newLocation);
			//}
			//else if (this._rootDragElement is TabGroupPane)
			//{
			//    // AS 10/15/08 TFS8068
			//    //SplitPane split = DockManagerUtilities.CreateSplitPane();
			//    //TabGroupPane tabGroup = DockManagerUtilities.Clone((TabGroupPane)this._rootDragElement, newLocation);
			//    //split.Panes.Add(tabGroup);
			//    //return split;
			//    newSplit = DockManagerUtilities.CreateSplitPane(_dockManager);
			//    TabGroupPane tabGroup;
			//    replacement = DockManagerUtilities.Clone((TabGroupPane)this._rootDragElement, newLocation, out tabGroup);
			//    newSplit.Panes.Add(tabGroup);
			//}
			//else if (this._rootDragElement is SplitPane)
			//{
			//    // AS 10/15/08 TFS8068
			//    //return DockManagerUtilities.Clone((SplitPane)this._rootDragElement, newLocation);
			//    replacement = DockManagerUtilities.Clone((SplitPane)this._rootDragElement, newLocation, out newSplit);
			//}
			//else
			//{
			//    Debug.Fail("Unrecognized root element!");
			//    // AS 10/15/08 TFS8068
			//    //return null;
			//    throw new InvalidOperationException();
			//}
			//
			//// AS 10/15/08 TFS8068
			//Debug.Assert(null != replacement);
			//return replacement; 
			#endregion //Refactored

			SplitPane newSplit = DockManagerUtilities.CreateSplitPane(_dockManager);

			using (MovePaneHelper moveHelper = new MovePaneHelper(_dockManager, newSplit))
			{
				if (null != initializeNewSplit)
					initializeNewSplit(newSplit);

				if (_rootDragElement is TabGroupPane || _rootDragElement is ContentPane)
				{
					if (_rootDragElement is ContentPane)
					{
						DockManagerUtilities.MovePane((ContentPane)this._rootDragElement, newSplit, null, newLocation, moveHelper);
					}
					else
					{
						TabGroupPane clone = DockManagerUtilities.CreateTabGroup(_dockManager);
						newSplit.Panes.Add(clone);
						DockManagerUtilities.Clone(_rootDragElement as TabGroupPane, newLocation, clone, moveHelper);
					}
				}
				else
				{
					SplitPane sourceSplit;

					if (this._windowBeingDragged != null)
						sourceSplit = _windowBeingDragged.Content as SplitPane;
					else
						sourceSplit = _rootDragElement as SplitPane;

					Debug.Assert(null != sourceSplit);

					if (null == sourceSplit)
						return null;

					DockManagerUtilities.Clone(sourceSplit, newLocation, newSplit, moveHelper);
				}
			}

			return newSplit;
		} 
		#endregion //CloneRootDragElement

        // AS 3/13/09 FloatingWindowDragMode
        #region CreateFloatingPreviewWindow
        internal void CreateFloatingPreviewWindow(Point point, Size size)
        {
            Debug.Assert(this._floatingPreviewWindow == null);
            Debug.Assert(this.DeferFloatingWindowPositioning);

            if (null == _floatingPreviewWindow)
            {
                _floatingPreviewWindow = DragToolWindow.Create(this._dockManager);
                _floatingPreviewWindow.IsHitTestVisible = false;

                // position/size
                _floatingPreviewWindow.Left = point.X;
                _floatingPreviewWindow.Top = point.Y;
                _floatingPreviewWindow.Width = size.Width;
                _floatingPreviewWindow.Height = size.Height;

                // content
                Control previewControl = new Control();
                previewControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                previewControl.VerticalAlignment = VerticalAlignment.Stretch;
                previewControl.SetResourceReference(Control.StyleProperty, XamDockManager.FloatingWindowPreviewStyleKey);
                _floatingPreviewWindow.Content = previewControl;

                Debug.Assert(null != _initialDragElement && XamDockManager.GetPaneLocation(_initialDragElement) != PaneLocation.Unknown);
                PaneLocation paneLocation = null != _initialDragElement ? XamDockManager.GetPaneLocation(this._initialDragElement) : PaneLocation.Unknown;
                previewControl.SetValue(XamDockManager.PaneLocationPropertyKey, DockManagerKnownBoxes.FromValue(paneLocation));

                // show
                // AS 3/30/09 TFS16355 - WinForms Interop
                //_floatingPreviewWindow.Show(this._dockManager, false);
                DockManagerUtilities.ShowToolWindow(_floatingPreviewWindow, _dockManager, false);
                _floatingPreviewWindow.BringToFront();
            }
            else
            {
                _floatingPreviewWindow.Width = size.Width;
                _floatingPreviewWindow.Height = size.Height;
                _floatingPreviewWindow.Top = point.Y;
                _floatingPreviewWindow.Left = point.X;
            }
        }
        #endregion //CreateFloatingPreviewWindow

		#region CreateWindowBeingDragged
		internal void CreateWindowBeingDragged(Point location, Size size)
		{
			Debug.Assert(this._windowBeingDragged == null);

			if (null == this._windowBeingDragged)
			{
				Debug.Assert(this._rootDragElement is ContentPane || this._rootDragElement is TabGroupPane);

				if (this._rootDragElement is ContentPane || this._rootDragElement is TabGroupPane)
				{
					// AS 4/25/08
					// If the element we're going to reparent has the keyboard focus, which it should,
					// then we need to temporarily shift focus out. If we don't then the framework
					// gets into a bad state. They don't clear the IsKeyboardFocusWithin, etc. of the 
					// parent chain. To get around this we will temporarily move focus out of the pane
					// while we are reparenting the elements.
					//
					IInputElement focusedElement = Keyboard.FocusedElement;
					bool shiftFocus = focusedElement is DependencyObject &&
						// AS 5/2/08 BR32272
						// We also want to shift focus if the root drag element itself has focus.
						//
						(focusedElement == this._rootDragElement || Utilities.IsDescendantOf(this._rootDragElement, focusedElement as DependencyObject));

					if (shiftFocus)
					{
						Debug.Assert(_mouseCaptureHelper != null, "We're trying to shift focus while creating the window but we don't have a capture helper");

						if (_mouseCaptureHelper != null)
							Keyboard.Focus(this._mouseCaptureHelper);
					}

					// AS 5/21/08
					// get the current containers so we can remove them if needed after the move
					IList<IContentPaneContainer> oldContainers = this.GetDragPaneContainers();

					// AS 4/28/11 TFS73532
					// Changed CloneRootDragElement so it will take a callback so it initializes/creates the 
					// parent before adding the children so we do a top-down build of the tree.
					//
					//// AS 10/15/08 TFS8068
					////SplitPane rootSplit = this.CloneRootDragElement(PaneLocation.Floating);
					//SplitPane rootSplit;
					//using (this.CloneRootDragElement(PaneLocation.Floating, out rootSplit))
					//{
					//    XamDockManager.SetInitialLocation(rootSplit, InitialPaneLocation.DockableFloating);
					//    XamDockManager.SetFloatingLocation(rootSplit, location);
					//    XamDockManager.SetFloatingSize(rootSplit, size);
					//    this._dockManager.Panes.Add(rootSplit);
					//}
					Action<SplitPane> initializeCallback = delegate(SplitPane split)
					{
						XamDockManager.SetInitialLocation(split, InitialPaneLocation.DockableFloating);
						XamDockManager.SetFloatingLocation(split, location);
						XamDockManager.SetFloatingSize(split, size);
						this._dockManager.Panes.Add(split);
					};
					SplitPane rootSplit = this.CloneRootDragElement(PaneLocation.Floating, initializeCallback);

					this._windowBeingDragged = ToolWindow.GetToolWindow(rootSplit) as PaneToolWindow;
					Debug.Assert(null != this._windowBeingDragged);

					// AS 5/28/08 BR33432
					// Make sure to initialize the last active pane with the pane that was
					// active when we did the drag if it is now within the floating window.
					//
					Debug.Assert(this._lastActivePane != null, "We don't have an active pane that we can use to initialize the active pane of the new floating window?");

					if (null != this._lastActivePane)
					{
						PaneToolWindow toolWindow = ToolWindow.GetToolWindow(this._lastActivePane) as PaneToolWindow;

						if (null != toolWindow && toolWindow == this._windowBeingDragged)
							toolWindow.LastActivePane = this._lastActivePane;
					}

					// some of these panes may have had floating placeholders before so fix that up now
					this.FixPlaceholders();

					// AS 5/21/08
					DockManagerUtilities.RemoveContainersIfNeeded(oldContainers);

					// AS 4/25/08
					// See comment above.
					//
					if (shiftFocus)
					{
						// AS 5/2/08 BR32272
						// If the element is or is within a content pane then we want to 
						// activate the pane first since that will activate the floating window, etc.
						//
						ContentPane cp = focusedElement as ContentPane ?? Utilities.GetAncestorFromType((DependencyObject)focusedElement, typeof(ContentPane), true, this._rootDragElement) as ContentPane;

						if (null != cp)
							cp.ActivateInternal(true);

						if (cp != focusedElement)
							focusedElement.Focus();
					}

					// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
					if (_dragState != Dragging.DragState.ProcessingDrop && // AS 3/31/11 TFS65912
						_windowBeingDragged != null &&
						_windowBeingDragged.Host != null &&
						_windowBeingDragged.Host.IsWindow)
					{
						if (_dockManager.FloatingWindowDragMode == FloatingWindowDragMode.UseSystemWindowDrag)
							this.SwitchToWindowDrag();
						else
						{
							FrameworkContentElement dragHelper = _windowBeingDragged.DragHelper;

							if (Keyboard.FocusedElement == _mouseCaptureHelper)
								dragHelper.Focus();

							// AS 9/19/11 TFS88123 - Added if check
							// In this situation the Focus call above is causing a WPF Menu to be given focus. When 
							// that happens it captures the mouse. As a result our captured element gets a lost 
							// mouse capture event and we end the drag operation during the Focus call. While we 
							// could try unhooking the events before calling focus other things could happen 
							// (like the mouse button released, etc.) that could end the drag so we'll just ensure 
							// that we're in a drag operation still.
							//
							if (_dragState == DragState.Dragging)
							{
								if (_mouseCaptureHelper != null)
									this.SetDragCaptureHelper(null);

								dragHelper.CaptureMouse();
								Debug.Assert(dragHelper.IsMouseCaptured, "DragHelper of toolwindow doesn't have mouse capture");

								if (_dragState == DragState.Dragging)
									this.SetDragCaptureHelper(dragHelper);
							}
						}
					}
				}
			}
		} 
		#endregion //CreateWindowBeingDragged

		#region GetDropLocation
		internal static AllowedDropLocations GetDropLocation(PaneLocation location)
		{
			switch (location)
			{
				default:
				case PaneLocation.FloatingOnly:
				case PaneLocation.Unpinned:
				case PaneLocation.Unknown:
					Debug.Fail("This method should not be used with these types");
					return 0;
				case PaneLocation.Document:
					return AllowedDropLocations.Document;
				case PaneLocation.DockedBottom:
					return AllowedDropLocations.Bottom;
				case PaneLocation.DockedLeft:
					return AllowedDropLocations.Left;
				case PaneLocation.DockedTop:
					return AllowedDropLocations.Top;
				case PaneLocation.DockedRight:
					return AllowedDropLocations.Right;
				case PaneLocation.Floating:
					return AllowedDropLocations.Floating;
			}
		}
		#endregion //GetDropLocation

		#region GetMoveToSplitPaneRelativeSize
		internal Size GetMoveToSplitPaneRelativeSize(FrameworkElement newGroup)
		{
			FrameworkElement relativeElement = null;

			if (this._panesBeingDragged.Count == 1)
				relativeElement = this._panesBeingDragged[0];
            else if (this._rootDragElement is TabGroupPane)
                relativeElement = this._rootDragElement;
            else
            {
                // AS 3/13/09 FloatingWindowDragMode
                // I found this while implementing this feature.
                // The ToolWindow would never have a relative size but 
                // its root panel in theory could so get it from that.
                //
                //relativeElement = this._windowBeingDragged;
                Debug.Assert(null != _windowBeingDragged);

                if (_windowBeingDragged == null)
                    return new Size(100d, 100d);

                relativeElement = (FrameworkElement)_windowBeingDragged.Pane ?? _windowBeingDragged;
            }

			Debug.Assert(null != relativeElement);

			return SplitPane.GetRelativeSize(relativeElement);
		}
		#endregion //GetMoveToSplitPaneRelativeSize

		#region GetDragPaneContainers
		// AS 5/21/08
		// Helper method to get a list of the unique containers currently containing
		// the content panes. This is needed by the various pane drag actions to 
		// ensure we clean up as needed.
		//
		internal IList<IContentPaneContainer> GetDragPaneContainers()
		{
			// AS 3/17/11 TFS67321
			// Moved impl to a helper method.
			//
			//List<IContentPaneContainer> containers = new List<IContentPaneContainer>();
			//IContentPaneContainer lastContainer = null;
			//
			//foreach (ContentPane cp in this._panesBeingDragged)
			//{
			//    IContentPaneContainer container = cp.PlacementInfo.CurrentContainer;
			//
			//    if (lastContainer == container)
			//        continue;
			//
			//    lastContainer = container;
			//
			//    if (false == containers.Contains(container))
			//        containers.Add(container);
			//}
			//
			//return containers;
			return DockManagerUtilities.GetPaneContainers(_panesBeingDragged);
		} 
		#endregion //GetDragPaneContainers

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region GetIndicatorToolWindowTemplate
        internal static ControlTemplate GetIndicatorToolWindowTemplate(XamDockManager dm)
        {
            // when hosted in a browser, the popup will not be shown in a top level window
            // and therefore will not be allowed to support transparency
            return dm != null && dm.ShowToolWindowsInPopup && DockManagerUtilities.IsPopupInChildWindow
                ? IndicatorToolWindowTemplateWhite
                : IndicatorToolWindowTemplate;
        } 
        #endregion //GetIndicatorToolWindowTemplate

		#region GetPanesForDragElement
		internal static IList<ContentPane> GetPanesForDragElement(FrameworkElement rootPane, out FrameworkElement rootContainerElement)
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(rootPane);

			if (null == dockManager)
				throw new ArgumentException(XamDockManager.GetString("LE_ElementNotInDockManager"), "rootPane");

			IList<ContentPane> panes;

			// if the root pane is a content pane then just consider that to be the pane being dragged
			ContentPane pane = rootPane as ContentPane;

			if (null == pane)
			{
				// otherwise see if its a tab item
				PaneTabItem tab = rootPane as PaneTabItem ?? Utilities.GetAncestorFromType(rootPane, typeof(PaneTabItem), true, dockManager, typeof(IPaneContainer)) as PaneTabItem;

				if (null != tab)
				{
					pane = tab.Pane;
				}
				else // see if its a child of a content pane
				{
					pane = Utilities.GetAncestorFromType(rootPane, typeof(ContentPane), true, dockManager, typeof(IPaneContainer)) as ContentPane;

					if (null != pane)
					{
						// if the pane is within a tab group and we're not dragging the tab item 
						// then drag the entire group
						if (DockManagerUtilities.GetParentPane(pane) is TabGroupPane)
						{
							Debug.Assert(rootPane is PaneHeaderPresenter || null != Utilities.GetAncestorFromType(rootPane, typeof(PaneHeaderPresenter), true, pane), "A pane is being dragged but not by using its header or tab item");
							pane = null;
						}
					}
				}
			}

			if (null != pane)
			{
				// copy/drag the associated element
				rootContainerElement = pane;

				Debug.Assert(XamDockManager.GetDockManager(pane) == dockManager);
				panes = new ContentPane[] { pane };
			}
			else
			{
				panes = new List<ContentPane>();
				IPaneContainer container = rootPane as IPaneContainer ?? DockManagerUtilities.GetParentPane(rootPane);

				if (null == container)
					throw new ArgumentException(XamDockManager.GetString("LE_InvalidRootDragElement"));

				// copy/drag the associated element
				rootContainerElement = container as FrameworkElement;

				pane = DockManagerUtilities.GetFirstLastPane(container as UIElement, true, PaneFilterFlags.AllVisible);

				while (null != pane)
				{
					panes.Add(pane);
					pane = DockManagerUtilities.GetNextPreviousPane(pane, false, true, container);
				}
			}

			return panes;
		} 
		#endregion //GetPanesForDragElement

		#region HideDropPreview
		internal void HideDropPreview()
		{
			// AS 11/12/09 TFS24789 - TabItemDragBehavior
			// Moved to a helper method since we have multiple preview windows.
			//
			//if (this._dropPreviewWindow != null && this._dropPreviewWindow.IsVisible)
			//    this._dropPreviewWindow.Close();
			HideToolWindow(_dropPreviewWindow);
			HideToolWindow(_tabItemDropPreviewWindow);
		}
		#endregion //HideDropPreview

		// AS 11/12/09 TFS24789 - TabItemDragBehavior
		// Helper funtion based on the previous HideDropPreview impl.
		//
		#region HideToolWindow
		private static void HideToolWindow(ToolWindow toolWindow)
		{
			if (toolWindow != null && toolWindow.IsVisible)
				toolWindow.Close();
		}
		#endregion //HideToolWindow

		#region IsBeingDragged
		internal bool IsBeingDragged(ContentPane pane)
		{
			foreach (ContentPane dragPane in this._panesBeingDragged)
			{
				if (dragPane == pane)
					return true;
			}

			return false;
		}
		#endregion //IsBeingDragged

		#region IsDropLocationAllowed
		internal bool IsDropLocationAllowed(PaneLocation location)
		{
			return this.IsDropLocationAllowed(GetDropLocation(location));
		}

		internal bool IsDropLocationAllowed(AllowedDropLocations location)
		{
			// AS 5/28/08 RaisePaneDragOverForInvalidLocations
			//return location == (this._allowedLocations & location);
			return 0 != (this._allowedLocations & location);
		}
		#endregion //IsDropLocationAllowed

		// AS 9/20/11 TFS88634
		#region OnToolWindowClosing
		internal void OnToolWindowClosing(PaneToolWindow toolWindow)
		{
			// AS 5/7/11 TFS106302
			// We need to defer closing the toolwindow until after the panes 
			// have been moved into the their intended destinations and not 
			// during the processing of the dragaction itself.
			//
			if (_dragState == Dragging.DragState.ProcessingDrop)
			{
				this.AddForceRemainOpen(toolWindow);
				return;
			}

			// we only need to do this for the window that contained the root element 
			// so we can avoid the OS issue where capture is no longer transferred 
			// to the capture HWND
			if (toolWindow == null || toolWindow != _rootDragElementToolWindow)
				return;

			// if the drag operation is not in progress then we don't need to do anything
			if (_dragState != Dragging.DragState.Dragging)
				return;

			var oldHelper = _mouseCaptureHelper;

			// if we didn't capture the mouse for this window or already transferred then 
			// we don't have to do anything further.
			if (oldHelper == null || LogicalTreeHelper.GetParent(oldHelper) != toolWindow)
				return;

			// we only want to intervene when we are closing the window as a result of 
			// removing/closing the last pane
			if (toolWindow.Pane != null && toolWindow.Pane.Visibility != Visibility.Collapsed)
				return;

			// if we're in the process of reshowing the window then we shouldn't interfere with the close
			if (toolWindow.IsReshowing)
				return;

			if (toolWindow.Host != null && toolWindow.Host.IsWindow)
			{
				// make sure the window cannot be closed or else capture will be interrupted (not through events 
				// or even the OS' idea of what hwnd has capture) but because it just won't route the messages 
				// to the appropriate window
				// AS 5/7/11 TFS106302
				// Use a new helper method that will hold one or more panes to be processed in this way.
				//
				//_rootDragElementToolWindow.ForceRemainOpen = true;
				this.AddForceRemainOpen(_rootDragElementToolWindow);
			}

			var dragHelper = _dockManager.DragHelper;

			// we're going to capture the mouse for the dm which means the current helper will 
			// lose capture so clear that out before we capture so we don't cancel the drag
			this.SetDragCaptureHelper(null);

			oldHelper.ReleaseMouseCapture();

			dragHelper.CaptureMouse();

			if (_dragState != Dragging.DragState.Dragging)
				return;

			if (dragHelper.IsMouseCaptured)
				this.SetDragCaptureHelper(dragHelper);

			_dockManager.ForceFocus(_dockManager.DragHelper);
		}
		#endregion //OnToolWindowClosing

		#region MoveToNewRootSplitPane
		internal void MoveToNewRootSplitPane(Dock side, bool outerEdge, double thickness)
		{
			IList<IContentPaneContainer> oldContainers = this.GetDragPaneContainers(); // AS 3/17/11 TFS67321

			PaneLocation newLocation = DockManagerUtilities.GetDockedLocation(side);

			// AS 4/28/11 TFS73532
			//SplitPane newSplit = null;
			//// AS 10/16/08 TFS8068
			////this.MoveToSplitPane(ref newSplit, 0, newLocation);
			//FrameworkElement movedElement;
			//using (this.MoveToSplitPane(ref newSplit, 0, newLocation, out movedElement))
			//{
			//    int newIndex = outerEdge ? 0 : this._dockManager.Panes.Count;
			//    InitialPaneLocation initialLocation = DockManagerUtilities.GetInitialLocation(side);
			//    XamDockManager.SetInitialLocation(newSplit, initialLocation);
			//
			//    if (side == Dock.Left || side == Dock.Right)
			//        newSplit.Width = thickness;
			//    else
			//        newSplit.Height = thickness;
			//
			//    this._dockManager.Panes.Insert(newIndex, newSplit);
			//}
			SplitPane newSplit = DockManagerUtilities.CreateSplitPane(_dockManager);

			using (MovePaneHelper moveHelper = new MovePaneHelper(_dockManager, newSplit))
			{
				int newIndex = outerEdge ? 0 : this._dockManager.Panes.Count;
				InitialPaneLocation initialLocation = DockManagerUtilities.GetInitialLocation(side);
				XamDockManager.SetInitialLocation(newSplit, initialLocation);

				if (side == Dock.Left || side == Dock.Right)
					newSplit.Width = thickness;
				else
					newSplit.Height = thickness;

				this._dockManager.Panes.Insert(newIndex, newSplit);

				this.MoveToSplitPane(newSplit, 0, newLocation, moveHelper);
			}

			DockManagerUtilities.RemoveContainersIfNeeded(oldContainers); // AS 3/17/11 TFS67321
		} 
		#endregion //MoveToNewRootSplitPane

		#region MoveToSplitPane
		/// <summary>
		/// Moves the panes being dragged into the specified split pane at the specified index.
		/// </summary>
		/// <param name="splitPane">The target split pane to which the panes being dragged will be moved</param>
		/// <param name="index">The index in the split pane to which the panes should be moved</param>
		/// <returns>Returns the element that was moved.</returns>
		internal FrameworkElement MoveToSplitPane(SplitPane splitPane, int index)
		{
			Debug.Assert(null != splitPane);
			PaneLocation paneLocation = XamDockManager.GetPaneLocation(splitPane);
			Debug.Assert(paneLocation != PaneLocation.Unknown);

			// AS 4/28/11 TFS73532
			//// AS 10/16/08 TFS8068
			////return this.MoveToSplitPane(ref splitPane, index, paneLocation);
			//FrameworkElement movedElement;
			//using (this.MoveToSplitPane(ref splitPane, index, paneLocation, out movedElement))
			//    return movedElement;
			using (MovePaneHelper moveHelper = new MovePaneHelper(_dockManager, splitPane))
			{
				return this.MoveToSplitPane(splitPane, index, paneLocation, moveHelper);
			}
		}
		#endregion //MoveToSplitPane

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnWindowBeforeDrag
		internal bool OnWindowBeforeDrag(PaneToolWindow paneToolWindow, Point pt, MouseDevice mouse)
		{
			// if we're transferring the drag to window we are dragging then just return true 
			if (this.DragState == DragState.Dragging &&
				paneToolWindow == this.FloatingWindow)
			{
				return true;
			}

			Debug.Assert(this.DragState == DragState.None);

			// AS 6/8/11 TFS76337
			// If this was the result of dragging the caption then the mouse point 
			// may not be over the ToolWindow itself but is likely over the non-client 
			// area of the window. We need to adjust the rect we pass off as the original 
			// position.
			//
			//return this.StartDrag(paneToolWindow, pt, new ScreenInputDeviceInfo(paneToolWindow.PointToScreen(pt)), false, true);
			// AS 8/22/11 TFS84326
			//Point screenPoint = paneToolWindow.PointToScreen(pt);
			Point screenPoint = Utilities.PointToScreenSafe(paneToolWindow,pt);
			Point relativePoint = pt;
			AdjustDragPositionForToolWindow(paneToolWindow, screenPoint, ref relativePoint);

			return this.StartDrag(paneToolWindow, relativePoint, new ScreenInputDeviceInfo(screenPoint), false, true);
		}
		#endregion //OnWindowBeforeDrag

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnWindowDragStart
		internal void OnWindowDragStart(ToolWindow paneToolWindow)
		{
			Debug.Assert(paneToolWindow == this.FloatingWindow, "Different window is starting a drag?");
		}
		#endregion //OnWindowDragStart

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnWindowDragEnd
		// AS 5/8/12 TFS107054
		// Added a boolean return value to know if the window did anything.
		//
		internal bool OnWindowDragEnd(ToolWindow toolWindow)
		{
			bool result = false;

			if (toolWindow == this.FloatingWindow)
			{
				if (_dragState == DragState.Dragging)
					result = true;

				this.EndDrag(false);
			}

			return result;
		}
		#endregion //OnWindowDragEnd

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnWindowDragging
		internal void OnWindowDragging(ToolWindow toolWindow, Point pt, MouseDevice mouse, Rect logicalScreenRect)
		{
			if (toolWindow == this.FloatingWindow)
			{
				// AS 8/22/11 TFS84326
				//this.ProcessDragMove(new ScreenInputDeviceInfo(toolWindow.PointToScreen(pt)), logicalScreenRect);
				this.ProcessDragMove(new ScreenInputDeviceInfo(Utilities.PointToScreenSafe(toolWindow,pt)), logicalScreenRect);
			}
		}
		#endregion //OnWindowDragging

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnWindowMoveCancelled
		internal void OnWindowMoveCancelled(PaneToolWindow paneToolWindow)
		{
			if (paneToolWindow == this.FloatingWindow)
				this.EndDrag(true);
		}
		#endregion //OnWindowMoveCancelled

		#region ProcessMouseDown
		/// <summary>
		/// Helper method to initiate a drag operation.
		/// </summary>
		/// <param name="element">The element for which the mouse down event is occuring</param>
		/// <param name="e">The event arguments for the mouse down event</param>
		internal static void ProcessMouseDown(FrameworkElement element, MouseButtonEventArgs e)
		{
			// we're expecting a mouse down event args
			Debug.Assert(e.RoutedEvent == Mouse.MouseDownEvent);

			// AS 6/8/11 TFS76337
			if (ProcessMouseDown(element, new MouseArgsInputDeviceInfo(e)))
				e.Handled = true;
		}

		// AS 6/8/11 TFS76337
		// Moved the implementation from the overload that takes a MouseButtonEventArgs to one that takes a
		// IInputDeviceInfo and returns a boolean indicating if the event should be handled or not.
		//
		private static bool ProcessMouseDown(FrameworkElement element, IInputDeviceInfo e)
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(element);
			DragManager dragManager = null != dockManager ? dockManager.DragManager : null;
			DockManagerUtilities.ThrowIfNull(element, "element");
			Debug.Assert(null != dragManager);

			switch (XamDockManager.GetPaneLocation(element))
			{
				case PaneLocation.Unknown:
				case PaneLocation.Unpinned:
					return false;
			}

			if (null != dragManager)
			{
				// AS 6/8/11 TFS76337
				//if (e.ChangedButton == MouseButton.Left)
				if ((e.ChangedButton ?? MouseButton.Left) == MouseButton.Left)
				{
					FrameworkElement rootContainer;
					IList<ContentPane> panes = GetPanesForDragElement(element, out rootContainer);

					Debug.Assert(null != panes && panes.Count > 0);

					if (null == panes || panes.Count == 0)
						return false;

					// check the panes and don't start a drag if nothing can happen
					AllowedDropLocations locations = dragManager.GetAllowedDropLocations(panes);

					if (locations == 0)
						return false;

					// AS 6/24/11 FloatingWindowCaptionSource
					// When the mouse is pressed down on the PaneHeaderPresenter and we're letting the os 
					// do the drag then start it now as would happen when the mouse is pressed down on the 
					// caption when the title of the panetoolwindow is shown.
					//
					var paneHeader = element as PaneHeaderPresenter;

					if (null != paneHeader && null != paneHeader.Pane)
					{
						var tw = PaneToolWindow.GetSinglePaneToolWindow(paneHeader.Pane);

						if (tw != null && dockManager.FloatingWindowDragMode == FloatingWindowDragMode.UseSystemWindowDrag)
						{
							if (dragManager.StartDrag(tw, e.GetPosition(tw), e, false, true))
							{
								if (dragManager.DragState == DragState.Dragging)
								{
									dragManager.SwitchToWindowDrag();
									return true;
								}
							}

							return false;
						}
					}

					Debug.Assert(dragManager.DragState == DragState.None);

					if (element.CaptureMouse())
					{
						dragManager.HookElementPendingDrag(element);
						dragManager._dragState = DragState.Pending;
						// when a document is selected, its size/position are changing
						// so therefore the position of the mouse relative to it has
						// changed and the first mouse move could result in starting
						// a drag. i could have tried to force a layout but that seems
						// dangerous. instead, we'll store the location in screen coords
						// relative to the element
						//dragManager._mouseDownLocation = e.GetPosition(element);
						dragManager._mouseDownLocation = Utilities.PointToScreenSafe(element, e.GetPosition(element));
						// AS 6/8/11 TFS76337
						//e.Handled = true;
						return true;
					}
				}
				else if (dragManager.DragState != DragState.None)
				{
					// if we're dragging or waiting to start one then eat the message
					// AS 6/8/11 TFS76337
					//e.Handled = true;
					return true;
				}
			}

			return false;
		}
		#endregion //ProcessMouseDown

		#region ShowDropPreview
        // AS 3/13/09 FloatingWindowDragMode
        // Added target PaneLocation to all overloads 
        // Pass along the target PaneLocation as well since we include this in the floating 
        // window preview.
        //
		internal void ShowDropPreview(FrameworkElement owner, DropPreviewTabLocation location, Size size, PaneLocation targetLocation)
		{
			ShowDropPreview(owner, location, null, size, 0d, 0d, targetLocation);
		}

        internal void ShowDropPreview(FrameworkElement owner, Dock side, Size size, double offset, PaneLocation targetLocation)
		{
			bool isLeftRight = side == Dock.Left || side == Dock.Right;
			double offsetX = isLeftRight ? offset : 0d;
			double offsetY = isLeftRight ? 0d : offset;

			this.ShowDropPreview(owner, DropPreviewTabLocation.None, side, size, offsetX, offsetY, targetLocation);
		}

        private void ShowDropPreview(FrameworkElement owner, DropPreviewTabLocation location, Dock? sideForNone, Size size, double offsetX, double offsetY, PaneLocation targetLocation)
		{
			HorizontalAlignment horzAlign = HorizontalAlignment.Stretch;
			VerticalAlignment vertAlign = VerticalAlignment.Stretch;

			#region Initialize Alignment
			if (location == DropPreviewTabLocation.None)
			{
				if (sideForNone != null)
				{
					switch (sideForNone.Value)
					{
						case Dock.Left:
						case Dock.Right:
							horzAlign = sideForNone == Dock.Left
								? HorizontalAlignment.Left
								: HorizontalAlignment.Right;
							break;
						case Dock.Bottom:
						case Dock.Top:
							vertAlign = sideForNone == Dock.Top
								? VerticalAlignment.Top
								: VerticalAlignment.Bottom;
							break;
					}
				}
			}
			#endregion //Initialize Alignment

			this.ShowDropPreview(owner, location, size, horzAlign, vertAlign, offsetX, offsetY, targetLocation);
		}

		internal void ShowDropPreview(FrameworkElement owner, DropPreviewTabLocation location, Size size,
            HorizontalAlignment horzAlign, VerticalAlignment vertAlign, double offsetX, double offsetY, PaneLocation targetLocation)
		{
			// AS 11/12/09 TFS24789 - TabItemDragBehavior
			// If we previously showed the tab item preview make sure that's hidden
			// since we don't show both at the same time.
			//
			HideToolWindow(_tabItemDropPreviewWindow);

			ToolWindow window = this.DropPreviewWindow;
			Control control = window.Content as Control;

			if (window.Owner != owner)
				window.Close();

			#region Constraint to min/max for preview
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			#endregion //Constraint to min/max for preview

			if (false == double.IsNaN(size.Width) && false == double.IsInfinity(size.Width))
				window.Width = size.Width;
			else
				window.ClearValue(FrameworkElement.WidthProperty);

			if (false == double.IsNaN(size.Height) && false == double.IsInfinity(size.Height))
				window.Height = size.Height;
			else
				window.ClearValue(FrameworkElement.HeightProperty);

			window.VerticalAlignment = vertAlign;
			window.HorizontalAlignment = horzAlign;
			window.VerticalAlignmentOffset = offsetY;
			window.HorizontalAlignmentOffset = offsetX;

			control.SetValue(XamDockManager.DropPreviewTabLocationPropertyKey, DockManagerKnownBoxes.FromValue(location));

            // AS 3/13/09 FloatingWindowDragMode
            control.SetValue(XamDockManager.PaneLocationPropertyKey, DockManagerKnownBoxes.FromValue(targetLocation));

			if (window.IsVisible == false)
			{
                // AS 3/30/09 TFS16355 - WinForms Interop
				//window.Show(owner, false);
                DockManagerUtilities.ShowToolWindow(window, owner, false);

				// AS 5/29/08 BR33471
				this._bringWindowToFront = true;

				// AS 5/29/08 BR33471
				// The drop preview is topmost so we need to make sure
				// that the indicator windows are above the drop preview.
				//
				foreach (ToolWindow tw in this._indicatorWindows)
				{
					if (null != tw && tw.IsVisible)
						tw.BringToFront();
				}
			}
		} 
		#endregion //ShowDropPreview

		// AS 11/12/09 TFS24789 - TabItemDragBehavior
		#region ShowTabItemPreview
		internal bool ShowTabItemPreview(TabGroupPane tabGroup, int index)
		{
			ToolWindow toolWindow = this.TabItemDropPreviewWindow;
			DropIndicator dropIndicator = toolWindow.Content as DropIndicator;
			Debug.Assert(null != dropIndicator);

			FlowDirection flowDirection = tabGroup.FlowDirection;
			bool isArrangedHorizontally = tabGroup.TabStripPlacement == Dock.Top ||
				tabGroup.TabStripPlacement == Dock.Bottom;

			TabItem tiBefore = DockManagerUtilities.GetItemContainer(tabGroup, index, true, false) as TabItem;
			TabItem tiAfter = DockManagerUtilities.GetItemContainer(tabGroup, index, false, true) as TabItem;

			bool isAfter = false;
			TabItem ti = null;

			if (tiAfter == null)
			{
				// if there is nothing after this point then we need to position after the before item
				isAfter = true;
				ti = tiBefore;
			}
			else
			{
				// default to showing the insertion bar before the "after" item
				ti = tiAfter;

				if (tiBefore != null)
				{
					// if we have a before and after and the before has a higher zindex
					// then use its edge
					int zBefore = Panel.GetZIndex(tiBefore);
					int zAfter = Panel.GetZIndex(tiAfter);

					if (zBefore > zAfter)
					{
						isAfter = true;
						ti = tiBefore;
					}
				}
				else
				{
					// if there is no before then show the insertion at the beginning of the 
					// after item
					ti = tiAfter;
				}
			}

			if (ti == null)
				return false;

			Debug.WriteLineIf(DebugTabItemPreview, string.Format("Group:{0} Index:{1}", tabGroup, index), "TabItemPreview Starting");

			FrameworkElement owner = tabGroup;
			DropLocation dropLocation;

			if (!isArrangedHorizontally)
				dropLocation = isAfter ? DropLocation.BelowTarget : DropLocation.AboveTarget;
			else
				dropLocation = isAfter ? DropLocation.RightOfTarget : DropLocation.LeftOfTarget;

			var tag = ti;

			if (toolWindow.Owner != owner || toolWindow.Tag != tag || dropLocation != dropIndicator.DropLocation)
			{
				// i'm clearing the drop location so that the animation is shown
				dropIndicator.DropLocation = null;
				toolWindow.Close();
			}

			toolWindow.Tag = tag;

			GeneralTransform transform = ti.TransformToAncestor(tabGroup);

			// the rect of the tab may be smaller than the rect of the contents as is the case 
			// with the default templates for the document panetabitem to produce the overlap so 
			// we want to use the combination of the rects
			Rect targetBounds = Rect.Union(new Rect(ti.RenderSize), VisualTreeHelper.GetDescendantBounds(ti));

			Debug.WriteLineIf(DebugTabItemPreview, targetBounds, "Original Bounds");

			// now transform the item bounds relative to the tab group so we're dealing with the tab group 
			// coordinates
			targetBounds = transform.TransformBounds(targetBounds);
			Debug.WriteLineIf(DebugTabItemPreview, targetBounds, "Transformed Size");

			Point topLeft = Utilities.PointToScreenSafe(tabGroup, targetBounds.TopLeft, null);
			Point bottomRight = Utilities.PointToScreenSafe(tabGroup, targetBounds.BottomRight, null);

			// now we've got the target rect in screen coordinates
			targetBounds = Utilities.RectFromPoints(topLeft, bottomRight);

			// setup the drop indicator as needed
			dropIndicator.DropTargetWidth = targetBounds.Width;
			dropIndicator.DropTargetHeight = targetBounds.Height;
			dropIndicator.DropLocation = dropLocation;

			Debug.WriteLineIf(DebugTabItemPreview, string.Format("Loc: {2} Size:{0}x{1}", targetBounds.Width, targetBounds.Height, dropLocation), "Initialized DropLocation");

			// try to ensure that the layout of the drop indicator is complete so we get the descendant
			toolWindow.UpdateLayout();

			UIElement elem = (UIElement)Utilities.GetDescendantFromName(dropIndicator, "PART_Offset");

			Point relativePoint = targetBounds.Location;

			if (null != elem)
			{
				if (toolWindow.IsVisible == false)
				{
					toolWindow.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
					toolWindow.Arrange(new Rect(toolWindow.DesiredSize));
				}

				Point offset = elem.TranslatePoint(new Point(0, 0), toolWindow);

				Debug.WriteLineIf(DebugTabItemPreview, offset, "Drop Offset");

				relativePoint.X -= offset.X;
				relativePoint.Y -= offset.Y;
			}

			if (DropLocation.RightOfTarget == dropLocation && flowDirection == FlowDirection.LeftToRight ||
                DropLocation.LeftOfTarget == dropLocation && flowDirection == FlowDirection.RightToLeft)
			{
				relativePoint.X += targetBounds.Width;

				Debug.WriteLineIf(DebugTabItemPreview, relativePoint, "Adjusting Offset for RightOfTarget");
			}
			else if (DropLocation.BelowTarget == dropLocation)
			{
				relativePoint.Y += targetBounds.Height;

				Debug.WriteLineIf(DebugTabItemPreview, relativePoint, "Adjusting Offset for BelowTarget");
			}

			// convert back to client coordinates relative to the owner
			relativePoint = Utilities.PointFromScreenSafe(owner, relativePoint);

			// then get the location with respect to what the owner needs
			relativePoint = ToolWindow.GetScreenPoint(owner, relativePoint, owner);

			Debug.WriteLineIf(DebugTabItemPreview, relativePoint, "Translated ScreenPoint");

			toolWindow.Left = relativePoint.X;
			toolWindow.Top = relativePoint.Y;

			// make sure the drop preview is hidden
			HideToolWindow(_dropPreviewWindow);

			if (!toolWindow.IsVisible)
				DockManagerUtilities.ShowToolWindow(toolWindow, owner, false);

			Debug.WriteLineIf(DebugTabItemPreview, string.Format("Group:{0} Index:{1}", tabGroup, index), "TabItemPreview Ending");

			return true;
		}
		#endregion //ShowTabItemPreview

		#region StartWindowDrag

		/// <summary>
		/// Helper method for initiating a drag operation of the specified tool window.
		/// </summary>
		/// <param name="paneToolWindow">The window to be dragged</param>
		/// <param name="pt">The point relative to the upper left of the window.</param>
		/// <param name="mouse">The mouse that started the drag</param>
		internal bool StartWindowDrag(PaneToolWindow paneToolWindow, Point pt, MouseDevice mouse)
		{
			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			//MouseButtonEventArgs args = new MouseButtonEventArgs(mouse, 0, MouseButton.Left);
			//args.RoutedEvent = Mouse.MouseDownEvent;
			//args.Source = paneToolWindow;
			//
			//return this.StartDrag(paneToolWindow, pt, args, false);
			// AS 6/8/11 TFS76337
			// Instead of immediately starting a drag operation we should capture the mouse and wait to see if the mouse 
			// is moved beyond the drag threshold.
			//
			//return this.StartDrag(paneToolWindow, pt, new ScreenInputDeviceInfo(paneToolWindow.PointToScreen(pt)), false, false);
			// AS 8/22/11 TFS84326
			//return ProcessMouseDown(paneToolWindow, new ScreenInputDeviceInfo(paneToolWindow.PointToScreen(pt)));
			return ProcessMouseDown(paneToolWindow, new ScreenInputDeviceInfo(Utilities.PointToScreenSafe(paneToolWindow,pt)));
		}
		#endregion //StartWindowDrag

		#endregion //Internal Methods

		#region Private Methods

		// AS 5/7/11 TFS106302
		#region AddForceRemainOpen
		private void AddForceRemainOpen(PaneToolWindow toolWindow)
		{
			if (toolWindow == null)
				return;

			if (_windowsPendingClose == null)
				_windowsPendingClose = new HashSet();

			_windowsPendingClose.Add(toolWindow);
			toolWindow.ForceRemainOpen = true;
		}
		#endregion //AddForceRemainOpen

		// AS 6/8/11 TFS76337
		#region AdjustDragPositionForToolWindow
		private static void AdjustDragPositionForToolWindow(ToolWindow toolWindow, Point screenPoint, ref Point relativePoint)
		{
			// the point passed in from the panetoolwindow is relative to the toolwindow itself but when we deal with 
			// the location/size of the floating toolwindow we are really containing the non-client area as well so we 
			// need to adjust the point to be relative to the window containing the toolwindow
			if (toolWindow != null && toolWindow.Host is Window)
			{
				Rect windowBounds = toolWindow.Host.GetWindowBounds();

				if (!windowBounds.IsEmpty)
				{
					Point logicalScreenPoint = Utilities.ConvertToLogicalPixels((int)screenPoint.X, (int)screenPoint.Y, toolWindow);
					logicalScreenPoint.X -= windowBounds.Left;
					logicalScreenPoint.Y -= windowBounds.Top;
					relativePoint = logicalScreenPoint;
				}
			}
		}
		#endregion //AdjustDragPositionForToolWindow

		#region CancelPendingDrag
		private void CancelPendingDrag(FrameworkElement element)
		{
			// clean up the state
			this._mouseDownLocation = null;
			this._dragState = DragState.None;

			// unhook the events so we don't process the lost capture we may cause below
			UnhookElementPendingDrag(element);

			// finally release the mouse capture
			element.ReleaseMouseCapture();
		}
		#endregion //CancelPendingDrag

		#region CreateAddToGroupAction
		// AS 11/12/09 TFS24789 - TabItemDragMode
		private AddToGroupActionBase CreateAddToGroupAction(FrameworkElement group, int index)
		{
			// AS 11/12/09 TFS24789 - TabItemDragMode
			// Added a new overload since one some ui interactions should utilize the new tab item insertion behavior.
			//
			//if (this.IsMoveInGroupAction(group))
			//    return new MoveInGroupAction(group, index);
			//else
			//    return new AddToGroupAction(group, index);
			return CreateAddToGroupAction(group, index, false);
		}

		// AS 11/12/09 TFS24789 - TabItemDragMode
		private AddToGroupActionBase CreateAddToGroupAction(FrameworkElement group, int index, bool allowTabItemInsertionBar)
		{
			AddToGroupActionBase action;

			if (this.IsMoveInGroupAction(group))
				action = new MoveInGroupAction(group, index);
			else
				action = new AddToGroupAction(group, index);

			if (allowTabItemInsertionBar)
			{
				action.TabItemDragBehavior = this.TabItemDragBehavior;
			}

			return action;
		}
		#endregion //CreateAddToGroupAction

		#region CreateIndicators
		private void CreateIndicators()
		{
			this._indicatorWindows = new ToolWindow[5];

			for (int i = 0; i < this._indicatorWindows.Length; i++)
			{
				this._indicatorWindows[i] = CreateIndicatorWindow((DockingIndicatorPosition)i);
			}

			Output("Created Indicators", "Dragging");
		}
		#endregion //CreateIndicators

		#region CreateIndicatorWindow
		private ToolWindow CreateIndicatorWindow(DockingIndicatorPosition position)
		{
			const int EdgePadding = 13;
			DockingIndicator indicator = new DockingIndicator();
			indicator.Position = position;

            // AS 3/13/09 FloatingWindowDragMode
            // Created helper method to remove duplicate code when 
            // creating a DragToolWindow.
            //
            //ToolWindow window = new DragToolWindow();
            //window.ResizeMode = ResizeMode.NoResize;
			//window.UseOSNonClientArea = false;
            ToolWindow window = DragToolWindow.Create(this._dockManager);
			window.Topmost = true;
			// moved this up from below so its available when the template is set and
			// before the content is set.
            // AS 3/13/09 FloatingWindowDragMode
            //window.SetValue(XamDockManager.DockManagerPropertyKey, this._dockManager);
			window.VerticalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;
			window.HorizontalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;
            // AS 3/13/09 FloatingWindowDragMode
            //window.Template = IndicatorToolWindowTemplate;
            //
            //// AS 5/21/08
            //// Instead of binding the theme property, we will add the resources of the 
            //// dockmanager to the window's resources. That will pick up the theme property
            //// resources and any other resources they put in.
            ////
            ////window.SetBinding(ToolWindow.ThemeProperty, Utilities.CreateBindingObject(XamDockManager.ThemeProperty, System.Windows.Data.BindingMode.OneWay, this._dockManager));
            //window.Resources.MergedDictionaries.Add(this._dockManager.Resources);

			#region Initialize Alignments
			switch (position)
			{
				case DockingIndicatorPosition.Left:
					window.HorizontalAlignment = HorizontalAlignment.Left;
					window.VerticalAlignment = VerticalAlignment.Center;
					window.HorizontalAlignmentOffset = EdgePadding;
					break;
				case DockingIndicatorPosition.Right:
					window.HorizontalAlignment = HorizontalAlignment.Right;
					window.VerticalAlignment = VerticalAlignment.Center;
					window.HorizontalAlignmentOffset = -EdgePadding;
					break;
				case DockingIndicatorPosition.Top:
					window.HorizontalAlignment = HorizontalAlignment.Center;
					window.VerticalAlignment = VerticalAlignment.Top;
					window.VerticalAlignmentOffset = EdgePadding;
					break;
				case DockingIndicatorPosition.Bottom:
					window.HorizontalAlignment = HorizontalAlignment.Center;
					window.VerticalAlignment = VerticalAlignment.Bottom;
					window.VerticalAlignmentOffset = -EdgePadding;
					break;
				case DockingIndicatorPosition.Center:
					window.HorizontalAlignment = HorizontalAlignment.Center;
					window.VerticalAlignment = VerticalAlignment.Center;
					break;
			} 
			#endregion //Initialize Alignments

			window.Content = indicator;

			return window;
		}
		#endregion //CreateIndicatorWindow

		#region EndDrag
		private void EndDrag(bool cancel)
		{
			Debug.Assert(this._dragState == DragState.Dragging);

			switch (this._dragState)
			{
				case DragState.None:
					return;
				case DragState.Pending:
					Debug.Fail("We're in a pending state. We should not have hit this code!");
					return;
				// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
				case DragState.ProcessingDrop:
					return;
			}

			Debug.Assert(this._dragState == DragState.Dragging);

			#region Clean Up

			// AS 9/20/11 TFS88634
			var originalToolWindow = _rootDragElementToolWindow;
			_rootDragElementToolWindow = null;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			_dragMoveCache = null;

			// unhook from the class that tracks key presses during the drag
			if (null != this._keyFocusWatcher)
			{
				this._keyFocusWatcher.KeyEvent -= new KeyEventHandler(OnKeyFocusWatcherKeyEvent);
				this._keyFocusWatcher.Dispose();
				this._keyFocusWatcher = null;
			}

			// first things first - unhook the events

			// AS 5/2/08 BR32272
			// We still want to release capture, etc. but we need to use this element still.
			// Plus we weren't removing it from the dockmanager on an end capture.
			//
			FrameworkContentElement oldMouseCaptureHelper = this._mouseCaptureHelper;
			Debug.Assert(null != oldMouseCaptureHelper || _isWindowDragMode);

			this.SetDragCaptureHelper(null);

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (_isWindowDragMode)
			{
				Mouse.OverrideCursor = null;
				this.IsInWindowDragMode = false;

				// AS 3/31/11 TFS66104
				// I'm letting the above code set the property so we stop the timer, etc. but going to 
				// reset the member variable until further below so we don't process move operations
				// during the drag.
				//
				_isWindowDragMode = true;
			}

			this._dragState = DragState.None;

			// clear the hottrack state
			if (null != this._lastHotTrackIndicator)
			{
				this._lastHotTrackIndicator.ClearValue(DockingIndicator.HotTrackPositionPropertyKey);
				this._lastHotTrackIndicator = null;
			}

			this.RemoveIndicators();

			// AS 11/12/09 TFS24789 - TabItemDragBehavior
			// Changed to a helper method so we can deal with the tab item drag window as well.
			//
			//if (this._dropPreviewWindow != null)
			//{
			//    this._dropPreviewWindow.Close();
			//
			//    // AS 5/21/08
			//    // To prevent rooting since we are not setting the theme property, we need
			//    // to clear the merged dictionaries.
			//    //
			//    this._dropPreviewWindow.Resources.MergedDictionaries.Clear();
			//
			//    this._dropPreviewWindow = null;
			//}
			ReleaseToolWindow(ref _dropPreviewWindow);
			ReleaseToolWindow(ref _tabItemDropPreviewWindow);

			this._globalIndicatorState = null;
			this._indicatorStates = null;

			// AS 5/28/08 RaisePaneDragOverForInvalidLocations
			this._defaultValidCursor = null;
			this._defaultInvalidCursor = null;

            // AS 3/13/09 FloatingWindowDragMode
            // Close the floating preview so its hidden but don't clear it out yet 
            // since an action may use it. We'll clear it out below.
            //
            if (null != _floatingPreviewWindow)
            {
                _floatingPreviewWindow.Close();

                // AS 5/21/08
                // To prevent rooting since we are not setting the theme property, we need
                // to clear the merged dictionaries.
                //
                _floatingPreviewWindow.Resources.MergedDictionaries.Clear();
            }

			#endregion //Clean Up

			if (false == cancel)
			{
				if (null != this._dropInfo && null != this._dropInfo.Action)
				{
					DragResult result;
					this._dragResults.TryGetValue(this._dropInfo.Action, out result);

					if (result != null && result.IsValid)
					{
						// AS 5/2/08 BR32272
						// This isn't directly related to this bug but now that we are shifting
						// focus into the floating window we should be careful to shift focus out
						// of the focused element before reparenting the elements.
						//
						// ContentPane activePane = this._dockManager.ActivePane;
						IInputElement focusedElement = Keyboard.FocusedElement;

						// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
						//if (null != focusedElement)
						//    Keyboard.Focus(oldMouseCaptureHelper);
						if (null != focusedElement)
						{
							if (oldMouseCaptureHelper == null)
								oldMouseCaptureHelper = _dockManager.DragHelper;

							Keyboard.Focus(oldMouseCaptureHelper);
						}

						try
						{
							// AS 12/9/09 TFS25268
							_dragState = DragState.ProcessingDrop;

							this._dockManager.ActivePaneManager.SuspendVerification();

							this._dropInfo.Action.PerformAction(this, false);

							this.FixPlaceholders();
						}
						finally
						{
							// AS 12/9/09 TFS25268
							_dragState = DragState.None;

							this._dockManager.ActivePaneManager.ResumeVerification();
						}

						// AS 5/2/08 BR32272
						// If the element is or is within a content pane then we want to 
						// activate the pane first since that will activate the floating window, etc.
						//
						//if (null != activePane)
						//	activePane.ActivateInternal(true);
						ContentPane cp = false == focusedElement is DependencyObject ? null : (focusedElement as ContentPane ?? Utilities.GetAncestorFromType((DependencyObject)focusedElement, typeof(ContentPane), true, this._rootDragElement) as ContentPane);

						if (null != cp)
							cp.ActivateInternal(true);

						if (cp != focusedElement && null != focusedElement)
							focusedElement.Focus();
					}
				}
                else if (this._windowBeingDragged != null && this._windowBeingDragged.IsVisible == false)
                {
                    // AS 10/15/08 TFS6271
                    //this._windowBeingDragged.Show(this._dockManager, true);
                    DockManagerUtilities.ShowToolWindow(this._windowBeingDragged, this._dockManager, true);
                }
			}

			// AS 3/31/11 TFS66104
			// Don't clear this until after the drop is processed.
			//
			this.IsInWindowDragMode = false;

			// AS 5/17/08 BR32810
			// We now put focus into the capture helper throughout the drag operation
			// so now that the drag is over - cancelled or not - if focus is within it
			// then we want to try and restore that to the last focused element if it 
			// still has the input focus.
			//
			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			//if (oldMouseCaptureHelper.IsKeyboardFocused)
			// AS 6/24/11 FloatingWindowCaptionSource
			// This isn't specific to this feature change but focus could be in the 
			// capture helper we had or that of the dockmanager. If it's in either then 
			// we shift the focus.
			//
			//if (oldMouseCaptureHelper != null && oldMouseCaptureHelper.IsKeyboardFocused)
			if ((oldMouseCaptureHelper != null && oldMouseCaptureHelper.IsKeyboardFocused)
				|| _dockManager.DragHelper.IsKeyboardFocused)
			{
				if (null != this._lastFocusedElement && this._lastFocusedElement.Focusable)
					Keyboard.Focus(this._lastFocusedElement);

				if (oldMouseCaptureHelper.IsKeyboardFocused || _dockManager.DragHelper.IsKeyboardFocused)
				{
					if (this._lastActivePane != null && this._lastActivePane.CanActivate)
						this._lastActivePane.ActivateInternal();

					if (oldMouseCaptureHelper.IsKeyboardFocused || _dockManager.DragHelper.IsKeyboardFocused)
						Keyboard.Focus(null);
				}
			}

			// make sure the floating window is above the others
			if (this._windowBeingDragged != null && this._windowBeingDragged.IsVisible)
				this._windowBeingDragged.BringToFront();

			// AS 5/17/08
			// Found this while testing out the reuse split/tabgroup change. If somehow focus is within
			// the helper object then shift it to the dockmanager.
			//
			DependencyObject focusScope = FocusManager.GetFocusScope(this._dockManager);

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			//if (null != focusScope && FocusManager.GetFocusedElement(focusScope) == oldMouseCaptureHelper)
			if (null != focusScope && oldMouseCaptureHelper != null && FocusManager.GetFocusedElement(focusScope) == oldMouseCaptureHelper)
			{
				FocusManager.SetFocusedElement(focusScope, this._dockManager);
			}

			#region Final Clean Up - Post Process

			Debug.Assert(_dragState == Dragging.DragState.None, "Started another drag?");

            // AS 4/8/09 TFS16492
            //// AS 5/2/08 BR32272
            //// We want to make sure that we remove the logical child we created when we
            //// started the drag operation.
            ////
            //this._dockManager.RemoveLogicalChildInternal(oldMouseCaptureHelper);

			// clear the reference to the window being dragged
			this._windowBeingDragged = null;
			this._initialDragElement = null;
			this._rootDragElement = null;
			this._dropInfo = null;
			this._dragResults = null;

            // AS 3/13/09 FloatingWindowDragMode
            this._floatingPreviewWindow = null;

			// AS 5/17/08 BR32810
			this._lastActivePane = null;
			this._lastFocusedElement = null;

			#endregion //Final Clean Up - Post Process

			// AS 5/7/11 TFS106302
			// We may have multiple windows that we suspended so now we'll use a helper 
			// method that will clean all of these up.
			//
			//// AS 9/20/11 TFS88634
			//if (originalToolWindow != null)
			//    originalToolWindow.ForceRemainOpen = false;
			this.ProcessPendingWindowClose();

			Output(DockManagerUtilities.DumpTree(this._dockManager), "EndDrag - Tree");

            // AS 3/13/09 FloatingWindowDragMode
            // Added if check since we will be using this for floating only panes 
            // as well and don't want to start raising the events for that.
            //
            if (_canRaiseDragEvents)
            {
                // lastly raise the drag ended event
                PaneDragEndedEventArgs args = new PaneDragEndedEventArgs(this._panesBeingDragged);
                this._panesBeingDragged = null;
                this._dockManager.RaisePaneDragEnded(args);
            }
		}
		#endregion //EndDrag

		#region FixPlaceholders
		private void FixPlaceholders()
		{
			DockManagerUtilities.FixPlaceholders(this._panesBeingDragged);
		}
		#endregion //FixPlaceholders

		#region GetAllowedDropLocations
		// AS 6/23/11 TFS73499
		// No longer static. We need to be able to get additional information from the dockmanager.
		//
		private AllowedDropLocations GetAllowedDropLocations(IList<ContentPane> panes)
		{
			if (panes.Count == 0)
				return 0;

			// assume we can drop everywhere
			AllowedDropLocations allowedLocations = AllowedDropLocations.All;

			// remove locations based on what the panes being dragged allow
			foreach (ContentPane pane in panes)
			{
				// find out where a pane is allowed
				AllowedDropLocations paneAllowedLocations = pane.AllowedDropLocations;

				// we only want to allow locations that all allow
				allowedLocations &= paneAllowedLocations;
			}

			// AS 6/23/11 TFS73499
			// We don't want to allow going to a floating position if the floating 
			// window can't be shown. Similarly we don't want to try and go to a docked 
			// position if the floating windows are shown but the main dockmanager 
			// window is not.
			//
			allowedLocations = _dockManager.AdjustAllowedDropLocations(allowedLocations);

			return allowedLocations;
		}
		#endregion //GetAllowedDropLocations

		#region GetHitTestIndicator
		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		//private DockingIndicator GetHitTestIndicator(MouseEventArgs e, out DockingIndicatorPosition? position)
		private DockingIndicator GetHitTestIndicator(IInputDeviceInfo e, out DockingIndicatorPosition? position)
		{
			// first find the indicator that the mouse is over
			for (int i = 0; i < this._indicatorWindows.Length; i++)
			{
				DockingIndicator indicator = this._indicatorWindows[i].Content as DockingIndicator;

				if (false == indicator.IsVisible)
					continue;

				InitializeIndicator(indicator);

				Point pt = e.GetPosition(indicator);

				position = indicator.GetPosition(pt);

				if (null != position)
					return indicator;
			}

			position = null;
			return null;
		} 
		#endregion //GetHitTestIndicator

		#region GetIndicatorDragAction
		private PaneDragAction GetIndicatorDragAction(DockingIndicator indicator, DockingIndicatorPosition positionInIndicator)
		{
			ToolWindow window = ToolWindow.GetToolWindow(indicator);

			Debug.Assert(null != window);

			if (null == window)
				return null;

			#region Over DocumentContentHost Indicator
			// we're over an indicator that is within the document content host so
			// we will be moving a pane within that area
			if (XamDockManager.GetPaneLocation(window.Owner) == PaneLocation.Document)
			{
				DocumentContentHost documentHost = this._dockManager.DocumentContentHost;

				if (window.Owner is DocumentContentHost)
				{
					#region Dragging Panes
					if (this._isDraggingDocuments == false)
					{
						switch (positionInIndicator)
						{
							// when dragging panes over the document content host we want to 
							// only add to the document content host when over the center. the 
							// edges will be used to create root level split panes adjacent
							// to the document content host
							case DockingIndicatorPosition.Center:
								{
									IContentPaneContainer container = documentHost.GetContainerForPane(this._panesBeingDragged[0], false);
									Debug.Assert(container == null || container is TabGroupPane);
									return new AddToDocumentHostAction(container as TabGroupPane, 0);
								}
							case DockingIndicatorPosition.Left:
							case DockingIndicatorPosition.Right:
							case DockingIndicatorPosition.Top:
							case DockingIndicatorPosition.Bottom:
								// if the indicator is for the dockmanager then create a new root split pane
								return new NewRootPaneAction(DockManagerUtilities.GetDockedSide(positionInIndicator), RootSplitPaneLocation.InnerDockManagerEdge);
							default:
								Debug.Fail("Unexpected position!");
								return null;
						}
					}
					#endregion //Dragging Panes

					#region Dragging Documents

					if (positionInIndicator == DockingIndicatorPosition.Center
						|| documentHost.ActiveDocument == null)
					{
						// this should only happen if there are no visible children
						IContentPaneContainer container = documentHost.GetContainerForPane(this._panesBeingDragged[0], false);
						Debug.Assert(container == null || container is TabGroupPane);
						return new AddToDocumentHostAction(container as TabGroupPane, 0);
					}

					// this means we are going to create a new root level split
					switch (positionInIndicator)
					{
						default:
							Debug.Fail("unrecognized location");
							return null;

						case DockingIndicatorPosition.Left:
						case DockingIndicatorPosition.Right:
						case DockingIndicatorPosition.Top:
						case DockingIndicatorPosition.Bottom:
							{
								// create a new split in the document host
								Dock side = DockManagerUtilities.GetDockedSide(positionInIndicator);
								return new NewRootPaneAction(side, RootSplitPaneLocation.DocumentContentHost);
							}
					}
					#endregion //Dragging Documents
				}

				#region Dragging a document over a contentpane within the documentcontenthost

				// this should only happen when dragging documents and over the center
				// indicator
				Debug.Assert(indicator.Position == DockingIndicatorPosition.Center && this._isDraggingDocuments);

				if (indicator.Position != DockingIndicatorPosition.Center || false == this._isDraggingDocuments)
					return null;

				#endregion //Dragging a document over a contentpane within the documentcontenthost
			} 
			#endregion //Over DocumentContentHost Indicator

			Debug.Assert(indicator.Position == DockingIndicatorPosition.Center
						|| window.Owner is DockManagerPanel
						|| window.Owner is XamDockManager);

			#region Center Indicator
			if (indicator.Position == DockingIndicatorPosition.Center)
			{
				#region Center Indicator When There Is No DocumentContentHost
				if (window.Owner is ContentPane == false)
				{
					Debug.Assert(XamDockManager.GetPaneLocation(window.Owner) == PaneLocation.Unknown);
					Debug.Assert(this._dockManager.DockPanel != null && this._dockManager.DockPanel.Child != null && (this._dockManager.DockPanel.Child == window.Owner || this._dockManager.DockPanel.Child.IsAncestorOf(window.Owner)));
					Debug.Assert(this._dockManager.HasDocumentContentHost == false);

					switch (positionInIndicator)
					{
						default:
							Debug.Fail("Unexpected indicator!");
							return null;

						case DockingIndicatorPosition.Center:
							// cannot do anything in this area
							return null;

						case DockingIndicatorPosition.Left:
						case DockingIndicatorPosition.Top:
						case DockingIndicatorPosition.Right:
						case DockingIndicatorPosition.Bottom:
						{
							// if the indicator is for the dockmanager then create a new root split pane
							return new NewRootPaneAction(DockManagerUtilities.GetDockedSide(positionInIndicator), RootSplitPaneLocation.InnerDockManagerEdge);
						}
					}
				} 
				#endregion //Center Indicator When There Is No DocumentContentHost

				Debug.Assert(window.Owner is ContentPane);

				ContentPane pane = window.Owner as ContentPane;

				if (null == pane)
					return null;

				IPaneContainer paneContainer = DockManagerUtilities.GetParentPane(pane);

				Debug.Assert(paneContainer is SplitPane || paneContainer is TabGroupPane);

				if (paneContainer is SplitPane == false && paneContainer is TabGroupPane == false)
					return null;

				// content panes should be in a tab group when in the document area
				if (paneContainer is SplitPane && XamDockManager.GetPaneLocation(window.Owner) == PaneLocation.Document)
					return null;

				// we will process the drop on either the tabgroup or the contentpane
				FrameworkElement targetPane = paneContainer as TabGroupPane ?? (FrameworkElement)pane;

				// this should only be the case when we are 
				switch (positionInIndicator)
				{
					default:
						Debug.Fail("Unexpected indicator!");
						return null;

					case DockingIndicatorPosition.Center:
						if (targetPane is TabGroupPane)
						{
							int index = 0;

							// AS 5/21/08 BR32779
							// if we're dragging a single pane and go over the center 
							// of the tab group that contains it then don't change its
							// position
							if (this._panesBeingDragged.Count == 1)
							{
								TabGroupPane tabGroup = (TabGroupPane)targetPane;

								int existingIndex = DockManagerUtilities.IndexOf(tabGroup.Items, this._panesBeingDragged[0], false);

								if (existingIndex >= 0)
									existingIndex = index;
							}

                            // AS 10/16/08
                            // While writing unit tests to verify TFS8068, I found that we should 
                            // have sometimes returned move in group.
                            //
                            //return new AddToGroupAction(targetPane, index);
							return this.CreateAddToGroupAction(targetPane, index);
						}
						else // create a new tab group around the pane
							return new NewTabGroupAction(pane);
					case DockingIndicatorPosition.Left:
					case DockingIndicatorPosition.Top:
					case DockingIndicatorPosition.Right:
					case DockingIndicatorPosition.Bottom:
					{
						Dock side = DockManagerUtilities.GetDockedSide(positionInIndicator);
						SplitPane splitPane = DockManagerUtilities.GetParentPane(targetPane) as SplitPane;

						Debug.Assert(splitPane != null);

						if (null == splitPane)
							return null;

						switch (side)
						{
							case Dock.Left:
							case Dock.Right:
								// the containing splitter is already in this orientation so just add to this group
                                if (splitPane.SplitterOrientation == Orientation.Vertical)
                                {
                                    // AS 10/16/08
                                    // While writing unit tests to verify TFS8068, I found that we should 
                                    // have sometimes returned move in group.
                                    //
                                    //return new AddToGroupAction(splitPane, splitPane.Panes.IndexOf(targetPane) + (side == Dock.Right ? 1 : 0));
                                    return this.CreateAddToGroupAction(splitPane, splitPane.Panes.IndexOf(targetPane) + (side == Dock.Right ? 1 : 0));
                                }
								break;
							case Dock.Top:
							case Dock.Bottom:
								// the containing splitter is already in this orientation so just add to this group
                                if (splitPane.SplitterOrientation == Orientation.Horizontal)
                                {
                                    // AS 10/16/08
                                    // While writing unit tests to verify TFS8068, I found that we should 
                                    // have sometimes returned move in group.
                                    //
                                    //return new AddToGroupAction(splitPane, splitPane.Panes.IndexOf(targetPane) + (side == Dock.Bottom ? 1 : 0));
                                    return this.CreateAddToGroupAction(splitPane, splitPane.Panes.IndexOf(targetPane) + (side == Dock.Bottom ? 1 : 0));
                                }
								break;
						}

						// otherwise create a new split pane
						return new NewSplitPaneAction(targetPane, DockManagerUtilities.GetDockedSide(positionInIndicator));
					}
				}
			} 
			#endregion //Center Indicator

			// if its a global indicator for the dockmanager then map the position to the docked side
			Debug.Assert(indicator.Position == positionInIndicator);

			#region Root Global Indicator

			if (indicator.Position == DockingIndicatorPosition.Center)
			{
				Debug.Fail("Unexpected position!");
				return null;
			}

			return new NewRootPaneAction(DockManagerUtilities.GetDockedSide(indicator.Position), RootSplitPaneLocation.OuterDockManagerEdge);

			#endregion //Root Global Indicator
		} 
		#endregion //GetIndicatorDragAction

		#region GetScreenPoint
		private Point GetScreenPoint(MouseEventArgs e)
		{
			return ToolWindow.GetScreenPoint(this._dockManager, e);
		}
		#endregion //GetScreenPoint

		#region HideIndicators
		private void HideIndicators(bool includeCenter, bool includeGlobal)
		{
			if (null != this._indicatorWindows)
			{
				Debug.Assert(this._indicatorWindows.Length == 5);

				for (int i = 0; i < this._indicatorWindows.Length; i++)
				{
					ToolWindow window = this._indicatorWindows[i];
					DockingIndicator indicator = window.Content as DockingIndicator;

					if (indicator.Position == DockingIndicatorPosition.Center)
					{
						if (false == includeCenter)
							continue;
					}
					else if (includeGlobal == false)
					{
						continue;
					}

					if (window.IsVisible)
					{
						Output(indicator.Position, "HideIndicators");

						this._indicatorWindows[i].Close();
					}
				}
			}
		}
		#endregion //HideIndicators

		#region HitTest
		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		//private IEnumerable<IInputElement> HitTest(MouseEventArgs e)
		private IEnumerable<IInputElement> HitTest(IInputDeviceInfo e)
		{
			// get the window containing the dm window so we can be sure
			// the tool window is in a separate top level window
			Window dmWindow = Window.GetWindow(this._dockManager);

			// get the toolwindows sorted by zindex
			int ownerIndex; // AS 1/10/12 TFS90890
			ToolWindow[] windows = ToolWindow.GetToolWindows(this._dockManager, out ownerIndex);

			// iterate the dm's floating windows
			// AS 1/10/12 TFS90890
			//foreach (ToolWindow item in windows)
			for (int i = 0; i < windows.Length; i++)
			{
				// AS 1/10/12 TFS90890
				// If the owner is in between the toolwindows then hit test it before getting 
				// to those floating windows.
				//
				if (i == ownerIndex)
				{
					// lastly check the dockmanager itself
					Point dmPoint = e.GetPosition(this._dockManager);
					IInputElement dmElement = this._dockManager.InputHitTest(dmPoint);

					if (null != dmElement)
						yield return dmElement;

					// if it didn't hit test in the dockmanager then we should skip it if 
					// its obscured by the containing window
					if (ToolWindow.IsMouseOverRoot(_dockManager, dmPoint))
						break;
				}

				PaneToolWindow window = windows[i] as PaneToolWindow;

				if (window == null || window.Content is SplitPane == false)
					continue;

				// skip hidden windows
				if (window.IsVisible == false)
					continue;

				// skip the drag window
				if (window == this._windowBeingDragged)
					continue;

                // AS 3/13/09 FloatingWindowDragMode
                if (window == this._floatingPreviewWindow)
                    continue;

				Point pt = e.GetPosition(window);
				IInputElement windowElement = window.InputHitTest(pt);

				if (null != windowElement)
				{
					yield return windowElement;
				}
				else if (window.SinglePane != null && window.IsUsingOSNonClientArea)
				{
					// see if we are over the non-client area of the floating window
					Window windowHost = Window.GetWindow(window);

					if (null != windowHost && windowHost != dmWindow)
					{
						// if we're in a window then we must have ui rights so find the screen point
						Point screenPt = window.PointToScreen(pt);
						IntPtr hwnd = new WindowInteropHelper(windowHost).Handle;
						IntPtr lParam = new IntPtr(((int)screenPt.Y) << 16 | ((int)screenPt.X));

						if (IntPtr.Zero != hwnd)
						{
							IntPtr hResult = NativeWindowMethods.SendMessageApi(hwnd, NativeWindowMethods.WM_NCHITTEST, IntPtr.Zero, lParam);

							if (NativeWindowMethods.HT_CAPTION == NativeWindowMethods.IntPtrToInt32(hResult))
							{
								yield return windowHost;
							}
						}
					}
				}
			}

			// AS 1/10/12 TFS90890
			// Added if check since we may have already hit test the owning window.
			//
			if (ownerIndex == windows.Length)
			{
				// lastly check the dockmanager itself
				Point dmPoint = e.GetPosition(this._dockManager);
				IInputElement dmElement = this._dockManager.InputHitTest(dmPoint);

				if (null != dmElement)
					yield return dmElement;
			}

			yield break;
		} 
		#endregion //HitTest

		#region HookElementPendingDrag
		private void HookElementPendingDrag(FrameworkElement element)
		{
			Debug.Assert(element.IsMouseCaptured);

			element.AddHandler(FrameworkElement.LostMouseCaptureEvent, new MouseEventHandler(this.OnPendingDragElementLostCapture), true);
			element.AddHandler(FrameworkElement.MouseUpEvent, new MouseButtonEventHandler(this.OnPendingDragElementMouseUp), true);
			element.AddHandler(FrameworkElement.MouseMoveEvent, new MouseEventHandler(this.OnPendingDragElementMouseMove), true);
		}
		#endregion //HookElementPendingDrag

		#region InitializeIndicator
		private void InitializeIndicator(DockingIndicator indicator)
		{
			switch (indicator.Position)
			{
				case DockingIndicatorPosition.Bottom:
					// AS 5/1/08
					// Do not disable element or it will not hittest and then we won't end up showing
					// the not cursor when back over the indicator again.
					//
					//indicator.IsEnabled = _globalIndicatorState.Bottom ?? true;
					indicator.SetValue(DockingIndicator.CanDockBottomPropertyKey, KnownBoxes.FromValue(_globalIndicatorState.Bottom ?? true));
					break;
				case DockingIndicatorPosition.Left:
					// AS 5/1/08
					//indicator.IsEnabled = _globalIndicatorState.Left ?? true;
					indicator.SetValue(DockingIndicator.CanDockLeftPropertyKey, KnownBoxes.FromValue(_globalIndicatorState.Left ?? true));
					break;
				case DockingIndicatorPosition.Right:
					// AS 5/1/08
					//indicator.IsEnabled = _globalIndicatorState.Right ?? true;
					indicator.SetValue(DockingIndicator.CanDockRightPropertyKey, KnownBoxes.FromValue(_globalIndicatorState.Right ?? true));
					break;
				case DockingIndicatorPosition.Top:
					// AS 5/1/08
					//indicator.IsEnabled = _globalIndicatorState.Top ?? true;
					indicator.SetValue(DockingIndicator.CanDockTopPropertyKey, KnownBoxes.FromValue(_globalIndicatorState.Top ?? true));
					break;
				case DockingIndicatorPosition.Center:
					{
						ToolWindow window = ToolWindow.GetToolWindow(indicator);
						Debug.Assert(null != window && null != window.Owner);

						IndicatorEnabledState enabledState;
						if (false == this._indicatorStates.TryGetValue(window.Owner, out enabledState))
						{
							enabledState = new IndicatorEnabledState();
							this._indicatorStates.Add(window.Owner, enabledState);
						}

						enabledState.Initialize(indicator);
						break;
					}
			}
		}
		#endregion //InitializeIndicator

		#region IsMoveInGroupAction
		private bool IsMoveInGroupAction(FrameworkElement group)
		{
			IPaneContainer container = group as IPaneContainer;

			if (null != container)
			{
				IList panes = container.Panes;

				foreach (ContentPane pane in this._panesBeingDragged)
				{
					// if the pane doesn't exist within the group then its not a reposition
					if (DockManagerUtilities.IndexOf(panes, pane, false) < 0)
						return false;
				}
			}

			return true;
		}
		#endregion //IsMoveInGroupAction

		#region MoveToSplitPane
		/// <summary>
		/// Helper method to move the panes being dragged to a split pane. If the split pane is null, one will be created.
		/// </summary>
		/// <param name="splitPane">The split pane to add to or null to create a new one.</param>
		/// <param name="index">The index to insert the panes if a <paramref name="splitPane"/> is provided</param>
		/// <param name="paneLocation">The location where the split pane will exist</param>
		/// <param name="moveHelper">Optional helper object used when reparenting the content panes</param>
        ///// <param name="movedElement">The element that was moved.</param>
        // AS 10/16/08 TFS8068
        //private FrameworkElement MoveToSplitPane(ref SplitPane splitPane, int index, PaneLocation paneLocation)
		// AS 4/28/11 TFS73532
		//private IDisposable MoveToSplitPane(ref SplitPane splitPane, int index, PaneLocation paneLocation, out FrameworkElement movedElement)
		private FrameworkElement MoveToSplitPane(SplitPane splitPane, int index, PaneLocation paneLocation, MovePaneHelper moveHelper)
		{
			// AS 4/28/11 TFS73532
			//// AS 10/16/08 TFS8068
			//IDisposable replacement;
			Debug.Assert(splitPane != null);
			FrameworkElement movedElement;

			if (this._panesBeingDragged.Count == 1)
			{
				// AS 4/28/11 TFS73532
				//if (splitPane == null)
				//    splitPane = DockManagerUtilities.CreateSplitPane(_dockManager);

				// AS 4/28/11 TFS73532
				//// AS 10/16/08 TFS8068
				//replacement = DockManagerUtilities.CreateMoveReplacement(this._panesBeingDragged[0]);

				// it doesn't matter if we've moved it to a floating window - just move it now to the specified pane
				// so we don't insert an extra unneeded layer
				DockManagerUtilities.MovePane(this._panesBeingDragged[0], splitPane, index, paneLocation, moveHelper );

                // AS 10/16/08 TFS8068
                //return this._panesBeingDragged[0];
                movedElement = this._panesBeingDragged[0];
			}
			else if (this._rootDragElement is TabGroupPane)
			{
				// AS 4/28/11 TFS73532
				//if (splitPane == null)
				//    splitPane = DockManagerUtilities.CreateSplitPane(_dockManager);

				// we started by dragging a tab group so find the tab group to clone
				TabGroupPane group;

				#region Find Source TabGroup
				// if we've dragged a window then we need to find the group within that
				if (null != this._windowBeingDragged)
				{
					SplitPane rootSplit = (SplitPane)this._windowBeingDragged.Content;

					ContentPane firstPane = DockManagerUtilities.GetFirstLastPane(rootSplit, true, PaneFilterFlags.AllVisible);

					Debug.Assert(null != firstPane);

					group = null != firstPane ? DockManagerUtilities.GetParentPane(firstPane) as TabGroupPane : null;

					Debug.Assert(null != group);
				}
				else
				{
					group = (TabGroupPane)this._rootDragElement;
				}
				#endregion //Find Source TabGroup

                if (null != group)
                {
					// AS 4/28/11 TFS73532
					//// clone the group and move it into the split pane
					//// AS 10/16/08 TFS8068
					////TabGroupPane newGroup = DockManagerUtilities.Clone(group, paneLocation);
					//TabGroupPane newGroup;
					//replacement = DockManagerUtilities.Clone(group, paneLocation, out newGroup);
					TabGroupPane newGroup = DockManagerUtilities.CreateTabGroup(_dockManager);

                    Debug.Assert(index <= splitPane.Panes.Count);
                    index = Math.Min(index, splitPane.Panes.Count);
                    splitPane.Panes.Insert(index, newGroup);

					// AS 4/28/11 TFS73532
					// Add/move the children after the tabgroup is in the tree.
					//
					DockManagerUtilities.Clone(group, paneLocation, newGroup, moveHelper);

                    // AS 10/16/08 TFS8068
                    //return newGroup;
                    movedElement = newGroup;
                }
                else
                {
					// AS 4/28/11 TFS73532
					//// AS 10/16/08 TFS8068
					//// Return an object that is disposable even though we're not
					//// doing anything in case a caller is expecting a return value.
					////
					//replacement = new GroupTempValueReplacement();
                    movedElement = null;
                }

                // AS 10/16/08 TFS8068
                //return null;
			}
			else
			{
				// we're dragging a window so we can just clone its contents - which must be a
				// split pane - and put that into the target split pane
				Debug.Assert(this._windowBeingDragged != null);

				SplitPane source = (SplitPane)this._windowBeingDragged.Content;

				// AS 4/28/11 TFS73532
				//// AS 10/16/08 TFS8068
				////SplitPane newSplit = DockManagerUtilities.Clone(source, paneLocation);
				//SplitPane newSplit;
				//replacement = DockManagerUtilities.Clone(source, paneLocation, out newSplit);
				SplitPane newSplit = DockManagerUtilities.CreateSplitPane(_dockManager);

				if (null != splitPane)
				{
					Debug.Assert(index <= splitPane.Panes.Count);
					splitPane.Panes.Insert(index, newSplit);
				}
				else
				{
					splitPane = newSplit;
				}

				// AS 4/28/11 TFS73532
				DockManagerUtilities.Clone(source, paneLocation, newSplit, moveHelper);

                // AS 10/16/08 TFS8068
                //return newSplit;
                movedElement = newSplit;
			}

			// AS 4/28/11 TFS73532
			//// AS 10/16/08 TFS8068
			//Debug.Assert(null != replacement);
			//return replacement;
			return movedElement;
		}
		#endregion //MoveToSplitPane

		#region OnDragElementXXX event sinks
		private void OnDragElementLostCapture(object sender, MouseEventArgs e)
		{
			Debug.Assert(this._dragState == DragState.Dragging);
			this.EndDrag(true);
			e.Handled = true;
		}

		private void OnDragElementMouseDown(object sender, MouseButtonEventArgs e)
		{
			// eat any new mouse down messages while dragging
			e.Handled = true;
		}

		private void OnDragElementMouseUp(object sender, MouseButtonEventArgs e)
		{
			Debug.Assert(this._dragState == DragState.Dragging);

			if (e.ChangedButton == MouseButton.Left)
				this.EndDrag(false);

			e.Handled = true;
		}

		private void OnDragElementMouseMove(object sender, MouseEventArgs e)
		{
			// AS 9/20/11 TFS88634
			// There was an issue in the OS (or maybe WPF) whereby if the original window 
			// that took capture was closed and capture was "transferred" the capture was 
			// lost (without raising any notifications) when the window was closed and we would 
			// only get mouse notifications when over one of the windows in this application. 
			// In case that manifests itself some other way we'll cancel the drag operation.
			//
			if (_dragState == Dragging.DragState.Dragging && e.LeftButton != MouseButtonState.Pressed)
			{
				this.EndDrag(true);
				return;
			}

			Debug.Assert(this._dragState == DragState.Dragging);
			this.ProcessDragMove(e);
		}
		#endregion //OnDragElementXXX event sinks

		#region OnKeyFocusWatcherKeyEvent
		private void OnKeyFocusWatcherKeyEvent(object sender, KeyEventArgs e)
		{
			if (e.RoutedEvent == Keyboard.PreviewKeyDownEvent)
			{
				Key key = e.Key == Key.System ? e.SystemKey : e.Key;

				if (key == Key.LeftCtrl || key == Key.RightCtrl)
				{
					// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
					// If the window was not yet shown (e.g. dragging a tab in a docked 
					// tab group) then this will not force the move to be processed. Also 
					// this won't raise the dragging events.
					//
					//// show the floating window if necessary
					//// AS 3/13/09 FloatingWindowDragMode
					//// We may be using a preview in which case we should reshow that
					//// instead of the window being dragged.
					////
					////if (this._windowBeingDragged != null && this._windowBeingDragged.IsVisible == false)
					//ToolWindow windowBeingDragged = this.FloatingWindow;
					//
					//if (windowBeingDragged != null && windowBeingDragged.IsVisible == false)
					//{
					//    // AS 10/15/08 TFS6271
					//    //this._windowBeingDragged.Show(this._dockManager, true);
					//    DockManagerUtilities.ShowToolWindow(windowBeingDragged, this._dockManager, true);
					//}
					//
					//// hide all indicators when the control key is pressed down. they
					//// should only be reshown if we get another mouse move and its up
					//this.HideIndicators(true, true);
					//
					//// the preview must also be hidden
					//this.HideDropPreview();
					this.VerifyControlKeyStateChanged();
				}
				else if (key == Key.Escape)
				{
					// cancel the drag when escape is pressed
					this.EndDrag(true);
				}
			}
			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// VS 2008 and prior didn't do this but VS 2010 does so we may 
			// as well.
			//
			else if (e.RoutedEvent == Keyboard.PreviewKeyUpEvent)
			{
				Key key = e.Key == Key.System ? e.SystemKey : e.Key;

				if (key == Key.LeftCtrl || key == Key.RightCtrl)
				{
					this.VerifyControlKeyStateChanged();
				}
			}

			// don't let any key events get through
			e.Handled = true;
		} 
		#endregion //OnKeyFocusWatcherKeyEvent

		#region OnPendingDragElementXXX event sinks
		private void OnPendingDragElementLostCapture(object sender, MouseEventArgs e)
		{
			// cancel the drag
			Debug.Assert(sender is FrameworkElement && this._dragState == DragState.Pending);
			this.CancelPendingDrag(sender as FrameworkElement);
		}

		private void OnPendingDragElementMouseUp(object sender, MouseButtonEventArgs e)
		{
			// cancel the drag
			Debug.Assert(sender is FrameworkElement && this._dragState == DragState.Pending);

			if (e.ChangedButton == MouseButton.Left)
			{
				this.CancelPendingDrag(sender as FrameworkElement);
			}
		}

		private void OnPendingDragElementMouseMove(object sender, MouseEventArgs e)
		{
			// potentially start the drag
			Debug.Assert(sender is FrameworkElement && this._dragState != DragState.Dragging);
			Debug.Assert(null != this._mouseDownLocation);

			FrameworkElement element = sender as FrameworkElement;

			if (element.IsVisible == false)
			{
				Debug.Fail("The element was removed from the visual tree!");
				this.CancelPendingDrag(element);
				return;
			}

			Point mousePos = e.GetPosition(element);
			Point originalPos = Utilities.PointFromScreenSafe(element, this._mouseDownLocation.Value);
			Point screenMousePos = Utilities.PointToScreenSafe(element, mousePos);

			if (XamDockManager.GetDockManager(element) == null)
			{
				Debug.Fail("The element isn't in the visual/logical tree!");
				this.CancelPendingDrag(element);
				return;
			}

			Rect dragRect = new Rect(this._mouseDownLocation.Value, new Size());
			// VS requires at least 6 pixels so either they don't honor the system setting or
			// they have a min. we'll use this as the min
			const double MinDragExtent = 6;
			dragRect.Inflate(Math.Min(MinDragExtent, SystemParameters.MinimumHorizontalDragDistance), Math.Min(MinDragExtent, SystemParameters.MinimumVerticalDragDistance));

			// see if the mouse has gone outside the drag rect
			if (false == dragRect.Contains(screenMousePos))
			{
				// AS 6/8/11 TFS76337
				// If this was the result of dragging the caption then the mouse point 
				// may not be over the ToolWindow itself but is likely over the non-client 
				// area of the window. We need to adjust the rect we pass off as the original 
				// position.
				//
				AdjustDragPositionForToolWindow(element as ToolWindow, _mouseDownLocation.Value, ref originalPos);

				// end the pending drag
				this.CancelPendingDrag(element);
				e.Handled = true;

				// AS 6/24/11 FloatingWindowCaptionSource
				// If the end user is dragging the caption of a pane and its caption 
				// is that of the floating window then pretend we are dragging the 
				// window itself.
				//
				bool isWindowDragMode = false;
				var header = element as PaneHeaderPresenter;

				if (null != header && header.Pane != null)
				{
					var tw = PaneToolWindow.GetSinglePaneToolWindow(header.Pane);

					if (tw != null)
					{
						originalPos = header.TransformToAncestor(tw).Transform(originalPos);
						element = tw;

						if (this.DockManager.FloatingWindowDragMode == FloatingWindowDragMode.UseSystemWindowDrag)
							isWindowDragMode = true;
					}
				}

				var elementAsWindow = element as ToolWindow;

				// AS 6/15/12 TFS114774
				// If the window is minimized then this is likely that the content pane's caption is being used 
				// in which case we should force using the os drag operations.
				//
				if (null != elementAsWindow && elementAsWindow.WindowState == WindowState.Minimized)
					isWindowDragMode = true;

				// start the pane dragging
				// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
				//this.StartDrag(element, originalPos, e, true);
				// AS 6/24/11 FloatingWindowCaptionSource
				// This could be a case where we wanted the os to do the drag even though they started by dragging our header.
				//
				//this.StartDrag(element, originalPos, new MouseArgsInputDeviceInfo(e), true, false);
				bool dragStarted = this.StartDrag(element, originalPos, new MouseArgsInputDeviceInfo(e), true, isWindowDragMode);

				// if we started a drag operation for a header that we shifted to the window...
				if (dragStarted && this.DragState == Dragging.DragState.Dragging && isWindowDragMode)
				{
					this.SwitchToWindowDrag();
				}
			}
		}
		#endregion //OnPendingDragElementXXX event sinks

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnWindowDragModeKeyTimerTick
		private void OnWindowDragModeKeyTimerTick(object state)
		{
			if (_dragState == DragState.Dragging)
			{
				_dockManager.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Infragistics.Windows.DockManager.DockManagerUtilities.MethodInvoker(VerifySystemControlKeyState));
			}
		}
		#endregion //OnWindowDragModeKeyTimerTick

		#region Output
		[Conditional("DEBUG_DRAGGING")]
		private static void Output(object value, string category)
		{
			Debug.WriteLine(value, category);
		}

		[Conditional("DEBUG_DRAGGING")]
		private static void OutputIf(bool condition, object value, string category)
		{
			if (condition)
				Output(value, category);
		}
		#endregion //Output

		#region PositionCenterIndicator
		private void PositionCenterIndicator(DependencyObject hitTestElement)
		{
			// we're making this assumption in lots of places
			Debug.Assert(hitTestElement is ContentPane 
				|| hitTestElement == null 
				|| hitTestElement is DocumentContentHost
				|| (XamDockManager.GetPaneLocation(hitTestElement) == PaneLocation.Unknown && this._dockManager.HasDocumentContentHost == false && this._dockManager.DockPanel != null && hitTestElement == this._dockManager.DockPanel.Child));

			ToolWindow centerWindow = this._indicatorWindows[(int)DockingIndicatorPosition.Center];
			DockingIndicator centerIndicator = centerWindow.Content as DockingIndicator;
			FrameworkElement newCenterOwner = hitTestElement as FrameworkElement;

			if (centerWindow.Owner != newCenterOwner)
			{
				// first we have to close the existing window
				if (centerWindow.Owner != null)
					centerWindow.Close();

				// then show it in the new place if we have a new place
				if (null != newCenterOwner)
				{
					Output(hitTestElement, "PositionCenterIndicator");

                    // AS 3/30/09 TFS16355 - WinForms Interop
					//centerWindow.Show(newCenterOwner, false);
                    DockManagerUtilities.ShowToolWindow(centerWindow, newCenterOwner, false);

					// AS 5/29/08 BR33471
					this._bringWindowToFront = true;
				}
			}
		} 
		#endregion //PositionCenterIndicator

		#region ProcessDragMove
		private void ProcessDragMove(MouseEventArgs e)
		{
			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			this.ProcessDragMove(new MouseArgsInputDeviceInfo(e), null);
		}

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// Since we may not have a MouseEventArgs that has its position information up to 
		// date, we need to use a helper class that provide the information we need from 
		// the args.
		//
		private void ProcessDragMove(IInputDeviceInfo e, Rect? windowRect)
		{
			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Cache the last information in case the control key state changes.
			//
			if (_dragMoveCache == null)
				_dragMoveCache = new DragMoveCache();

			_dragMoveCache.DeviceInfo = e;
			_dragMoveCache.WindowRect = windowRect;
			_dragMoveCache.WasControlKeyDown = IsControlKeyDown;
			bool forceFloating = _forceFloating || _dragMoveCache.WasControlKeyDown;

			DropInfo dropInfo = this._dropInfo;
			this._dropInfo.Reset();

            // AS 3/13/09 FloatingWindowDragMode
			//if (IsControlKeyDown)
			if (forceFloating)
			{
				#region Control Key Is Down - Floating Option Only

				// suppress all indicators and just show the floating window
				dropInfo.ShowGlobalIndicators = false;
				dropInfo.HideCenterIndicator = true;
				dropInfo.ShowFloatingWindow = true;
				dropInfo.HideDropPreview = true;

				// AS 5/28/08 RaisePaneDragOverForInvalidLocations
				dropInfo.HitTestLocation = PaneLocation.Floating;

				#endregion //Control Key Is Down - Floating Option Only
			}
			else
			{
				// we want the global indicators if the mouse is somewhere within the bounds of the 
				// dockmanager/documentcontenthost. we may choose to turn it off based on where the 
				// mouse is below
				FrameworkElement globalIndicatorElement = this.GlobalIndicatorsElement;
				Rect rect = new Rect(0, 0, globalIndicatorElement.ActualWidth, globalIndicatorElement.ActualHeight);
				dropInfo.ShowGlobalIndicators = rect.Contains(e.GetPosition(globalIndicatorElement));

				// AS 1/13/11 TFS61174
				// Moved down since the hittest logic may set ShowGlobalIndicators to true.
				//
				//// AS 5/28/08 RaisePaneDragOverForInvalidLocations
				//// Do not show the global indicators if the pane cannot be docked to any edge.
				////
				//if (false == this._raisePaneDragOverForInvalidLocations &&
				//    false == this.IsDropLocationAllowed(AllowedDropLocations.Docked))
				//{
				//    dropInfo.ShowGlobalIndicators = false;
				//}

				#region HitTest
				// see what element the mouse is over
				foreach (IInputElement inputElement in this.HitTest(e))
				{
					DependencyObject dep = inputElement as DependencyObject;

					Debug.Assert(dep != null);

					if (null == dep)
						break;

					// special case when over the non-client caption of a floating window
					if (dep is Window)
					{
						#region Non-Client Window Caption
						
						PaneToolWindow containedToolWindow = ((Window)dep).Content as PaneToolWindow;
						Debug.Assert(null != containedToolWindow && null != containedToolWindow.SinglePane);
						ContentPane cp = containedToolWindow.SinglePane;

						// ignore the window we are dragging
						if (containedToolWindow == this._windowBeingDragged)
							continue;

						PaneLocation toolWindowLocation = XamDockManager.GetPaneLocation(containedToolWindow);

						// skip the window if we're over a floating only pane
						if (toolWindowLocation == PaneLocation.FloatingOnly)
							continue;

						// AS 5/28/08 RaisePaneDragOverForInvalidLocations
						dropInfo.HitTestLocation = toolWindowLocation;

						IPaneContainer container = DockManagerUtilities.GetParentPane(cp);
						TabGroupPane group = container as TabGroupPane;

						// if the item is in a tabgroup then we will add to that tab group
						if (null != group)
						{
							Debug.Assert(false == this.IsMoveInGroupAction(group));
							dropInfo.Action = new AddToGroupAction(group, group.Items.Count);
						}
						else // otherwise create a new group
							dropInfo.Action = new NewTabGroupAction(cp);

						// the header of a pane, background of a tabgrouppane and caption
						// of a floating window with only 1 pane all have the same ui
						dropInfo.ShowGlobalIndicators = false;
						dropInfo.HideCenterIndicator = true;
						dropInfo.ShowFloatingWindow = true;
						dropInfo.HideDropPreview = false;
						break;

						#endregion // Non-Client Window Caption
					}

					#region Skip Invalid PaneLocations

					// AS 1/6/10 TFS25302
					// When there are nested dockmanagers the element we are over could 
					// be a nested dockmanager and hit a pane location of unknown in 
					// which case we won't end up with a center indicator. Instead we'll 
					// now walk up and use the first element within our dockmanager as the 
					// starting point.
					//
					while (dep != null)
					{
						// get the dockmanager of the hit test element
						DependencyObject temp = XamDockManager.GetDockManager(dep);

						// if we're not over a dockmanager or we're over our dockmanager we can stop
						if (temp == null || temp == _dockManager)
							break;

						// use the parent of the dockmanager as the starting point
						dep = System.Windows.Media.VisualTreeHelper.GetParent(temp);
					}

					if (dep == null)
						continue;

					PaneLocation location = XamDockManager.GetPaneLocation(dep);

					// AS 5/28/08 RaisePaneDragOverForInvalidLocations
					dropInfo.HitTestLocation = location;

					#region Over DockManager Content When Not Using DocumentContentHost
					if (location == PaneLocation.Unknown)
					{
						if (XamDockManager.GetDockManager(dep) == this._dockManager)
						{
							// if we don't have a document content host then make sure
							// that we have positioned the center indicator over the dockmanagerpanel
							// 
							if (this._dockManager.HasDocumentContentHost == false)
							{
								DockManagerPanel panel = this._dockManager.DockPanel;

								if (null != panel && panel.Child != null)
								{
									Point pt = e.GetPosition(panel.Child);

									if (new Rect(panel.Child.RenderSize).Contains(pt))
									{
										dropInfo.ShowGlobalIndicators = true;
										dropInfo.HideCenterIndicator = false;
										dropInfo.ShowFloatingWindow = true;
										dropInfo.HideDropPreview = false;

										// AS 5/28/08 RaisePaneDragOverForInvalidLocations
										// Delay showing the center indicator until we know it is possible
										// for it to contain a valid drop location.
										//
										//this.PositionCenterIndicator(panel.Child);
										dropInfo.CenterIndicatorElement = panel.Child;
										break;
									}
								}
							}
						}
					} 
					#endregion //Over DockManager Content When Not Using DocumentContentHost

					switch (location)
					{
						case PaneLocation.Unpinned:
						case PaneLocation.FloatingOnly:
							continue;
						case PaneLocation.Unknown:
							continue;
						
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

					}

					#endregion //Skip Invalid PaneLocations

					// get the containing tool window so we can tell if we go
					// over its caption (when its only hosting 1 pane)
					PaneToolWindow toolWindow = location == PaneLocation.Floating ? ToolWindow.GetToolWindow(dep) as PaneToolWindow : null;

					// if tab header area - hide all indicators, hide floating window, and show tabs in tab area
					// if caption,floating pane caption, or tabgroup (not header) - hide all indicators, hide floating window and show tab like drop rect
					// if contentpane/document content host - show center indicator and based drop rect based on hittest of indicators
					// if none then float and update floating position

					#region Walk Up Element Chain

					// walk up looking for a recognized drop target
					while (dep != null)
					{
						bool processLikeCaption = false;

						if (XamDockManager.GetDockManager(dep) != this._dockManager)
						{
							// in case we have a nested dockmanager, ignore its elements
						}
						else if (dep is PaneHeaderPresenter)
						{
							#region Pane Caption

							PaneHeaderPresenter header = (PaneHeaderPresenter)dep;

							if (false == this.IsBeingDragged(header.Pane) && null != header.Pane )
							{
								processLikeCaption = true;

								IPaneContainer container = DockManagerUtilities.GetParentPane(header.Pane);
								TabGroupPane group = container as TabGroupPane;

								// if the item is in a tabgroup then we will add to that tab group
								if (null != group)
									dropInfo.Action = CreateAddToGroupAction(group, group.Items.Count);
								else // otherwise create a new group
									dropInfo.Action = new NewTabGroupAction(header.Pane);
							}
							#endregion //Pane Caption
						}
						else if (toolWindow != null
							&& null != toolWindow.SinglePane
							&& toolWindow.CaptionElement == dep)
						{
							#region Floating Pane Caption

							if (false == this.IsBeingDragged(toolWindow.SinglePane))
							{
								// from a display perspective the behavior is the same as if we went over a pane
								processLikeCaption = true;

								IPaneContainer container = DockManagerUtilities.GetParentPane(toolWindow.SinglePane);
								TabGroupPane group = container as TabGroupPane;

								// if the item in the floating window is in a tabgroup then 
								// we will add to that tab group
								if (null != group)
									dropInfo.Action = CreateAddToGroupAction(group, group.Items.Count);
								else // otherwise create a new group
									dropInfo.Action = new NewTabGroupAction(toolWindow.SinglePane);
							}
							#endregion //Floating Pane Caption
						}
						else if (dep is ContentPane || dep is DocumentContentHost)
						{
							#region ContentPane or DocumentContentHost

							// if we're dragging panes and we're over a pane in the document
							// area then we need to ignore the pane. we need to show the 
							// center indicator over the document content host because we
							// need to be able to allow the user to create a root level split
							// pane docked just outside the document content host.
							//
							if (dep is ContentPane
								&& false == this._isDraggingDocuments
								&& PaneLocation.Document == XamDockManager.GetPaneLocation(dep))
							{
							}
							else if (dep is DocumentContentHost
								&& this._isDraggingDocuments)
							{
								// if we're dragging over the splitter within the document content host, etc.
								// do not show the center indicator
							}
							else
							{
								// skip when we are over the pane being dragged
								// AS 5/21/08 BR32779
								// When over a ContentPane in a tab group, we want to allow the center
								// indicator to be shown as long as there is one visible pane in that 
								// group that is not part of the drag operation. Otherwise, you won't be
								// able to move a pane from a tabgroup into a split with that tab group.
								//
								//if (false == dep is ContentPane || false == this.IsBeingDragged((ContentPane)dep))
								bool canShowCenterIndicator = false;

								if (false == dep is ContentPane)
									canShowCenterIndicator = true;
								else
								{
									IPaneContainer container = DockManagerUtilities.GetParentPane(dep);

									if (container is TabGroupPane)
									{
										foreach (object item in container.Panes)
										{
											ContentPane siblingPane = item as ContentPane;

											// if there is a visible item in the group that is not
											// part of the panes being dragged then we can show the 
											// center indicator
											if (null != siblingPane
												&& siblingPane.Visibility == Visibility.Visible
												&& 0 > DockManagerUtilities.IndexOf(this.PanesBeingDragged, siblingPane, false))
											{
												canShowCenterIndicator = true;
												break;
											}
										}
									}
									else
										canShowCenterIndicator = false == this.IsBeingDragged((ContentPane)dep);

									// if we're over a cp in a split or a tabgroup that only
									// contains the panes being dragged then ignore the hit test results
									// add see if we're over a global indicator
									if (false == canShowCenterIndicator)
										break;
								}

								if (canShowCenterIndicator)
								{
									dropInfo.ShowFloatingWindow = true;
									dropInfo.HideCenterIndicator = false;

									// in order to find out where this pane will go we will need to show the center indicator
									// AS 5/28/08 RaisePaneDragOverForInvalidLocations
									// Delay showing the center indicator until we know it is possible
									// for it to contain a valid drop location.
									//
									//this.PositionCenterIndicator(dep);
									dropInfo.CenterIndicatorElement = dep;
									break;
								}
							}
							#endregion //ContentPane or DocumentContentHost
						}
						else if (dep is PaneTabItem)
						{
							#region PaneTabItem

							PaneTabItem tab = (PaneTabItem)dep;
							TabGroupPane group = ItemsControl.ItemsControlFromItemContainer(dep) as TabGroupPane;

							if (group == null)
							{
								// this shouldn't happen
								Debug.Fail("The tab is not part of a group!");
							}
							else if (false == this._isDraggingDocuments && PaneLocation.Document == XamDockManager.GetPaneLocation(dep))
							{
								// if we're dragging panes and we're over a pane in the document
								// area then we need to ignore the pane. we need to show the 
								// center indicator over the document content host because we
								// need to be able to allow the user to create a root level split
								// pane docked just outside the document content host.
							}
                            // AS 10/16/08 TFS6254
                            // If we are dragging a single pane and we're over the pane itself
                            // we have to process it or the header area will process it and try
                            // to move it to the end of the list.
                            //
							//else if (this.IsBeingDragged(((PaneTabItem)dep).Pane))
                            else if (this._panesBeingDragged.Count > 1 && this.IsBeingDragged(tab.Pane))
							{
								// skip when we are over the pane being dragged
							}
							else
							{
								processLikeCaption = true;

								// insert the items before the specified tab
								int indexOfTab = group.ItemContainerGenerator.IndexFromContainer(dep);

								// AS 11/12/09 TFS24789 - TabItemDragBehavior
								#region TabItemDragBehavior.DisplayInsertionBar
								if (this.TabItemDragBehavior == TabItemDragBehavior.DisplayInsertionBar &&
									// when over the pane being dragged this would be a no-op so leave
									// the marker before the tab
									!this.IsBeingDragged(tab.Pane))
								{
									bool positionAfter = false;

									Rect itemRect = Rect.Union(new Rect(tab.RenderSize), VisualTreeHelper.GetDescendantBounds(tab));

									// since the item may have a transform on it get the rect relative to the group
									itemRect = tab.TransformToAncestor(group).TransformBounds(itemRect);

									Point relativePoint = e.GetPosition(group);

									bool isArrangedHorizontally = group.TabStripPlacement == Dock.Top ||
										group.TabStripPlacement == Dock.Bottom;

									if (isArrangedHorizontally)
									{
										positionAfter = relativePoint.X >= itemRect.Left + ((itemRect.Width + 1) / 2);
									}
									else
									{
										positionAfter = relativePoint.Y >= itemRect.Top + ((itemRect.Height + 1) / 2);
									}

									if (positionAfter)
										indexOfTab++;
									else
									{
										// if we're going to use the original index and position the item
										// before the tab we're over, we need to adjust the index if the item 
										// before it is the pane being dragged since the result will be a no-op
										// and the location should be consistent
										int previousIndex;
										PaneTabItem previousTab = DockManagerUtilities.GetItemContainer(group, indexOfTab, true, false, out previousIndex) as PaneTabItem;

										if (previousTab != null && this.IsBeingDragged(previousTab.Pane))
										{
											tab = previousTab;
											indexOfTab = previousIndex;
										}
									}
								}
								#endregion //TabItemDragBehavior.DisplayInsertionBar

								dropInfo.Action = CreateAddToGroupAction(group, indexOfTab, true );

								dropInfo.ShowFloatingWindow = false;
								dropInfo.HideDropPreview = false;
							}

							#endregion //PaneTabItem
						}
						else if (dep is TabGroupPane)
						{
							#region TabGroupPane

							if (false == this._isDraggingDocuments
								&& PaneLocation.Document == XamDockManager.GetPaneLocation(dep))
							{
								// if we're dragging panes and we're over a pane in the document
								// area then we need to ignore the pane. we need to show the 
								// center indicator over the document content host because we
								// need to be able to allow the user to create a root level split
								// pane docked just outside the document content host.
							}
							else if (dep == this._rootDragElement)
							{
								// if the tabgroup was the start of the drag then ignore it as part of the hittesting
							}
							else
							{
								TabGroupPane group = (TabGroupPane)dep;
								FrameworkElement headerArea = group.HeaderArea;

								processLikeCaption = true;

								// if we're over the header area then we want to behave a little differently
								if (null != headerArea && Utilities.IsDescendantOf(headerArea, inputElement as DependencyObject))
								{
									// the mouse is over the header area. the items need to be added to the group
									// and displayed - at least the tab items
									dropInfo.Action = CreateAddToGroupAction(group, group.Items.Count, true );
								}
								// AS 11/12/09 TFS24789 - TabItemDragBehavior
								// I'm not sure when inserting at 0 was expected but I don't want to change 
								// the default behavior just in case so when using the insertion bar we'll 
								// add the items to the end.
								//
								else if (this.TabItemDragBehavior == TabItemDragBehavior.DisplayInsertionBar)
								{
									dropInfo.Action = CreateAddToGroupAction(group, group.Items.Count, true);
								}
								else
								{
									// move the tabs to the beginning
									dropInfo.Action = CreateAddToGroupAction(group, 0);
								}
							}
							#endregion //TabGroupPane
						}
						else if (dep is XamDockManager)
						{
							// stop processing elements when we get to the dockmanager
							if (dep == this._dockManager)
								break;
						}

						#region processLikeCaption
						if (processLikeCaption)
						{
							Debug.Assert(null != dropInfo.Action);

							// the header of a pane, background of a tabgrouppane and caption
							// of a floating window with only 1 pane all have the same ui
							dropInfo.ShowGlobalIndicators = false;
							dropInfo.HideCenterIndicator = true;
							dropInfo.HideDropPreview = false;
							break;
						}
						#endregion //processLikeCaption

						dep = Utilities.GetParent(dep);
					}
					#endregion //Walk Up Element Chain

					break;
				} 
				#endregion //HitTest

				// AS 1/13/11 TFS61174
				// Moved down from above since the hittest logic may have set ShowGlobalIndicators to true.
				//
				// AS 5/28/08 RaisePaneDragOverForInvalidLocations
				// Do not show the global indicators if the pane cannot be docked to any edge.
				//
				if (false == this._raisePaneDragOverForInvalidLocations &&
					false == this.IsDropLocationAllowed(AllowedDropLocations.Docked))
				{
					dropInfo.ShowGlobalIndicators = false;
				}
			}

			// AS 5/28/08 RaisePaneDragOverForInvalidLocations
			#region RaisePaneDragOverForInvalidLocations
			if (false == this._raisePaneDragOverForInvalidLocations)
			{
				// if we want to perform an action then evaluate the location
				if (dropInfo.Action != null)
				{
					switch (dropInfo.HitTestLocation)
					{
						case PaneLocation.DockedBottom:
						case PaneLocation.DockedLeft:
						case PaneLocation.DockedRight:
						case PaneLocation.DockedTop:
						case PaneLocation.Document:
						case PaneLocation.Floating:
						case PaneLocation.Unknown:
							// make the judgement based on the drop location
							if (false == this.IsDropLocationAllowed(dropInfo.HitTestLocation))
								dropInfo.Action = InvalidDropLocation.Instance;

							break;
						case PaneLocation.Unpinned:
						case PaneLocation.FloatingOnly:
							break;
						default:
							Debug.Fail("Unexpected location:" + dropInfo.HitTestLocation.ToString());
							break;
					}
				}
				else if (dropInfo.CenterIndicatorElement != null)
				{
					AllowedDropLocations dropLocations;

					if (dropInfo.CenterIndicatorElement is DocumentContentHost)
					{
						// if over the center portion of the dockmanager, then the
						// ultimate location may be a number of locations so as long
						// as one is valid, we can accept it
						dropLocations = AllowedDropLocations.Docked | AllowedDropLocations.Document;
					}
					else if (dropInfo.HitTestLocation == PaneLocation.Unknown)
					{
						// similar to above except the center portion is not available
						dropLocations = AllowedDropLocations.Docked;
					}
					else
					{
						// for a center indicator within a specific location, just
						// use the location itself
						dropLocations = GetDropLocation(dropInfo.HitTestLocation);
					}

					if (false == this.IsDropLocationAllowed(dropLocations))
						dropInfo.Action = InvalidDropLocation.Instance;
				}
			} 
			#endregion //RaisePaneDragOverForInvalidLocations

			// AS 5/28/08 RaisePaneDragOverForInvalidLocations
			// Previously we showed the indicator in the hittest loop above but we need to wait 
			// until we can determine if any of the drop locations will be valid before showing
			// it. If we had an element that we were going to position the center indicator
			// over and the indicator could result in a valid location then show the indicator.
			//
			if (dropInfo.Action != InvalidDropLocation.Instance 
				&& dropInfo.HideCenterIndicator == false
				&& dropInfo.CenterIndicatorElement != null)
			{
				this.PositionCenterIndicator(dropInfo.CenterIndicatorElement);
			}

			#region Show/Hide Indicators

			// if we're allowed to show the global indicators then do so now
			if (dropInfo.ShowGlobalIndicators)
			{
				// hide the center if needed
				if (dropInfo.HideCenterIndicator)
					this.HideIndicators(true, false);

				this.ShowGlobalIndicators();
			}
			else
			{
				// hide the global indicators and possibly the center one
				this.HideIndicators(dropInfo.HideCenterIndicator, true);
			} 
			#endregion //Show/Hide Indicators

			DockingIndicatorPosition? position = null;
			DockingIndicator currentIndicator = null;

			#region HitTest Indicators

			// if we got to this point and don't have a drop action (and control is not down)...
            // AS 3/13/09 FloatingWindowDragMode
			//if (IsControlKeyDown == false && null == dropInfo.Action)
			if (forceFloating == false && null == dropInfo.Action)
			{
				// find out indicator we are over (and the position)
				currentIndicator = this.GetHitTestIndicator(e, out position);

				// get the drop action for the indicator we are over
				if (null != currentIndicator)
				{
					dropInfo.Action = this.GetIndicatorDragAction(currentIndicator, position.Value);

					// if we couldn't get a drag action then don't hot track the area
					if (null == dropInfo.Action)
					{
						dropInfo.HideDropPreview = true;

						currentIndicator = null;
						position = null;
					}
					else
						dropInfo.HideDropPreview = false;
				}
			} 
			#endregion //HitTest Indicators

			// AS 5/28/08 RaisePaneDragOverForInvalidLocations
			if (null != dropInfo.Action
				&& null == currentIndicator
				&& false == this._raisePaneDragOverForInvalidLocations
				&& false == dropInfo.Action.IsActionAllowed(this))
			{
				// If we were over an invalid location but we can show it as floating
				// then try to float or reposition the floating window. Otherwise do
				// not raise an event. Instead, use the singleton invalid drop location
				// which we have already stored as an invalid location.
				//
				if (this.IsDropLocationAllowed(AllowedDropLocations.Floating))
					dropInfo.Action = null;
				else
				{
					dropInfo.Action = InvalidDropLocation.Instance;
					dropInfo.HideDropPreview = true;
				}
			}

			#region Fallback to Floating If No Other Action

			// if we still don't have a drag action then the action will be to float
			if (null == dropInfo.Action)
			{
				dropInfo.HideDropPreview = true;

				// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
				//Point pt = this.GetScreenPoint(e);
				Point pt = e.GetScreenPoint(_dockManager);

				// AS 7/16/09 TFS18992
				// Store the actual screenpoint for comparison below.
				//
				Point screenPt = pt;

				// adjust for the original mouse down offset
				pt.X -= this._mouseDownLocation.Value.X;
				pt.Y -= this._mouseDownLocation.Value.Y;

                // AS 3/13/09 FloatingWindowDragMode
                // In deferred dragging, the _windowBeingDragged could be null 
                // even though we have already raised an event for the FloatPaneAction.
                // Basically we should have neither in order to raise a float action.
                //
                //if (this._windowBeingDragged == null)
                ToolWindow floatingWindow = this.FloatingWindow;

                if (floatingWindow == null && _windowBeingDragged == null)
				{
					Debug.Assert(this._rootDragElement is ContentPane || this._rootDragElement is TabGroupPane);

					ContentPane cp = this._rootDragElement as ContentPane ?? ((TabGroupPane)this._rootDragElement).SelectedItem as ContentPane;

					Size floatingSize = cp != null ? cp.LastFloatingSize : Size.Empty;

					if (floatingSize.IsEmpty)
						floatingSize = new Size(this._rootDragElement.ActualWidth, this._rootDragElement.ActualHeight);

					// AS 7/16/09 TFS18992
					// Ensure the mouse is over the floating window.
					//
					DockManagerUtilities.AdjustFloatingLocation(screenPt, ref pt, floatingSize);

					dropInfo.Action = new FloatPaneAction(pt, floatingSize);
				}
				else
				{
                    // AS 3/13/09 FloatingWindowDragMode
                    // In deferred drag mode, if the window was initially floating then 
                    // we want to get the location from that but once we have shown the 
                    // preview then we want to use that window's position. Also, in deferred 
                    // mode we want to be consistent and never provide the floating window
                    // even if it started off as floating since we will not be manipulating 
                    // its position during the drag.
                    //
                    //Point oldPoint = new Point(this._windowBeingDragged.Left, this._windowBeingDragged.Top);
                    //
                    //if (oldPoint != pt)
                    //	dropInfo.Action = new MoveWindowAction(this._windowBeingDragged, oldPoint, pt);
					// AS 7/16/09 TFS18992
					// Get the source window once.
					//
					//Point oldPoint = floatingWindow != null
					//    ? new Point(floatingWindow.Left, floatingWindow.Top)
					//    : new Point(_windowBeingDragged.Left, _windowBeingDragged.Top);
					ToolWindow srcWindow = floatingWindow != null ? floatingWindow : _windowBeingDragged;

					Point oldPoint = new Point(srcWindow.Left, srcWindow.Top);

					// AS 7/16/09 TFS18992
					// Ensure the mouse is over the floating window.
					//
					DockManagerUtilities.AdjustFloatingLocation(screenPt, ref pt, srcWindow.RenderSize);

					// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
					// If there is a specified rect (which would happen in an os drag mode), then 
					// we need to use that value.
					//
					if (null != windowRect)
						pt = windowRect.Value.Location;

                    if (oldPoint != pt)
                        dropInfo.Action = new MoveWindowAction(this.DeferFloatingWindowPositioning ? null : this._windowBeingDragged, oldPoint, pt);
				}
			} 
			#endregion //Fallback to Floating If No Other Action

			// if we're over a different indicator, clear the hottrack state of the last one
			if (null != this._lastHotTrackIndicator && (position == null || this._lastHotTrackIndicator != currentIndicator))
				this._lastHotTrackIndicator.ClearValue(DockingIndicator.HotTrackPositionPropertyKey);

            if (dropInfo.HideDropPreview)
            {
                this.HideDropPreview();
            }
            else if (_floatingPreviewWindow != null && _floatingPreviewWindow.IsVisible)
            {
                // AS 3/13/09 FloatingWindowDragMode
                // We only want to show 1 preview at a time so if we're not hiding 
                // the drop previews then we should hide the floating preview if we
                // have one.
                //
                _floatingPreviewWindow.Close();
            }

			if (null != dropInfo.Action)
			{
				DragResult result = null;

				// see whether we allowed the action before and use that info to allow/deny
				this._dragResults.TryGetValue(dropInfo.Action, out result);
				
				bool resultChanged = false;

				if (result == null)
				{
					#region New DropAction

					PaneDragOverEventArgs dragArgs = new PaneDragOverEventArgs(this._panesBeingDragged, dropInfo.Action);

					// initialize the state of the action based on whether its allowed
					dragArgs.IsValidDragAction = dropInfo.Action.IsActionAllowed(this);

                    // AS 3/13/09 FloatingWindowDragMode
                    // Added if check since we will be using this for floating only panes 
                    // as well and don't want to start raising the events for that.
                    //
                    if (_canRaiseDragEvents)
                    {
                        // AS 5/28/08 RaisePaneDragOverForInvalidLocations
                        // This should only happen if we went over an indicator whose state was invalid.
                        // We need to get into this block because we want to update the state of the indicator.
                        // In any case, we don't want to raise the panedragover if the state is invalid
                        // unless we're allowed to raise it for invalid locations.
                        //
						if (dragArgs.IsValidDragAction || this._raisePaneDragOverForInvalidLocations)
						{
							this._dockManager.RaisePaneDragOver(dragArgs);

							// AS 5/5/10 TFS30214
							// The developer could have done something in the PaneDragOver that cancelled 
							// the drag operation.
							//
							if (this._dragState != DragState.Dragging)
								return;
						}
                    }

					bool isValid = dragArgs.IsValidDragAction;

					// AS 5/28/08 RaisePaneDragOverForInvalidLocations
					// The default cursors are specified in the PaneDragStarting so use those instead. This
					// way if we are not raising the PaneDragOver for invalid locations, the programmer 
					// still has some control over the cursor when over an invalid location.
					//
					//Cursor cursor = dragArgs.Cursor ?? (isValid ? Cursors.Arrow : Cursors.No);
					Cursor cursor = dragArgs.Cursor ?? (isValid ? this._defaultValidCursor : this._defaultInvalidCursor);

					result = new DragResult(cursor, isValid);

					// AS 10/13/11 TFS91945
					// Moved out of this block since the internal event could change the state.
					//
					//if (null != currentIndicator && null != position)
					//    StoreIndicatorState(currentIndicator, position.Value, result.IsValid);

					//// store the result so we don't ask again
					//// AS 9/29/09 NA 2010.1 - PaneDragAction
					//// A drag action can be manipulated in the event such that its now the same as a previous one.
					////
					////this._dragResults.Add(dropInfo.Action, result);
					//this._dragResults[dropInfo.Action] = result;
					resultChanged = true;

					#endregion //New DropAction
				}

				// AS 10/13/11 TFS91945
				if (this.RaisePaneDragOverInternal)
				{
					var args = new PaneDragOverEventArgs(_panesBeingDragged, dropInfo.Action);
					args.IsValidDragAction = result.IsValid;
					args.Cursor = result.Cursor;

					_dockManager.OnPageDragOverInternal(args);

					// if we changed something ...
					if (args.IsValidDragAction != result.IsValid ||
						args.Cursor != result.Cursor)
					{
						result = new DragResult(args.Cursor, args.IsValidDragAction);
						resultChanged = true;
					}
				}

				// AS 10/13/11 TFS91945
				// Moved out from the if block above where we raise the PaneDragOver 
				// event so that we can update it in case the internal event modifies 
				// the state.
				//
				if (resultChanged)
				{
					if (null != currentIndicator && null != position)
						StoreIndicatorState(currentIndicator, position.Value, result.IsValid);

					// store the result so we don't ask again
					// AS 9/29/09 NA 2010.1 - PaneDragAction
					// A drag action can be manipulated in the event such that its now the same as a previous one.
					//
					//this._dragResults.Add(dropInfo.Action, result);
					this._dragResults[dropInfo.Action] = result;
				}

				if (false == result.IsValid)
				{
					// don't show the preview 
					this.HideDropPreview();

					// hide the hottracking of the indicator
					if (null != this._lastHotTrackIndicator)
						this._lastHotTrackIndicator.ClearValue(DockingIndicator.HotTrackPositionPropertyKey);

					// do not hot track the indicator if the drop position is invalid
					currentIndicator = null;
					position = null;
				}

				// update the cursor
				// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
				//this._mouseCaptureHelper.Cursor = result.Cursor;
				if (_mouseCaptureHelper != null)
					this._mouseCaptureHelper.Cursor = result.Cursor;
				else
					Mouse.OverrideCursor = result.Cursor;

				Output(dropInfo.Action, "Performing Preview Action");

				// let the action be performed
				if (result.IsValid)
				{
					dropInfo.Action.PerformAction(this, true);

					// AS 5/5/10 TFS30214
					// In theory a drag could end if the preview results in the window becoming floating, etc.
					//
					if (this._dragState != DragState.Dragging)
						return;
				}
			}

			// store the indicator
			this._lastHotTrackIndicator = currentIndicator;

			// if we're over an indicator...
			if (null != position)
			{
				Debug.Assert(null != currentIndicator);

				// and the position has changed...
				if (currentIndicator.HotTrackPosition != position.Value)
				{
					currentIndicator.SetValue(DockingIndicator.HotTrackPositionPropertyKey, position.Value);
				}
			}

			// AS 5/29/08 BR33471
			// The indicators could be an owned form of another top level form in which
			// case showing that owned form makes the owner come to the top of its zorder
			// which sometimes causes it to come over the window being dragged. Since we
			// can't prevent that we'll just call bringtofront on the window being dragged
			// after we've processed the move if we showed an indicator.
			//
            // AS 3/13/09 FloatingWindowDragMode
            // Changed to manipulate the zorder of the preview window if we're
            // using a preview otherwise the window being dragged.
            //
            ToolWindow windowBeingDragged = this.FloatingWindow;

			if (this._bringWindowToFront
				&& windowBeingDragged != null
				&& windowBeingDragged.IsVisible)
			{
				windowBeingDragged.BringToFront();
				this._bringWindowToFront = false;
			}
		}

		#endregion //ProcessDragMove 

		// AS 5/7/11 TFS106302
		#region ProcessPendingWindowClose
		private void ProcessPendingWindowClose()
		{
			if (_windowsPendingClose != null)
			{
				var windows = _windowsPendingClose.ToArray<PaneToolWindow>();
				_windowsPendingClose.Clear();

				foreach (var window in windows)
				{
					window.ForceRemainOpen = false;
				}
			}
		}
		#endregion //ProcessPendingWindowClose

		// AS 11/12/09 TFS24789 - TabItemDragBehavior
		#region ReleaseToolWindow
		private static void ReleaseToolWindow(ref ToolWindow toolWindow)
		{
			if (toolWindow != null)
			{
				toolWindow.Close();

				// AS 5/21/08
				// To prevent rooting since we are not setting the theme property, we need
				// to clear the merged dictionaries.
				//
				toolWindow.Resources.MergedDictionaries.Clear();

				toolWindow = null;
			}
		}
		#endregion //ReleaseToolWindow

		#region RemoveIndicators
		private void RemoveIndicators()
		{
			if (null != this._indicatorWindows)
			{
				this.HideIndicators(true, true);

				// AS 5/21/08
				// To prevent rooting since we are not setting the theme property, we need
				// to clear the merged dictionaries.
				//
				foreach (ToolWindow indicatorWindow in this._indicatorWindows)
				{
					if (null != indicatorWindow)
						indicatorWindow.Resources.MergedDictionaries.Clear();
				}

				this._indicatorWindows = null;
			}
		}
		#endregion //RemoveIndicators

		#region SetDragCaptureHelper
		private void SetDragCaptureHelper(FrameworkContentElement dragHelper)
		{
			if (this._mouseCaptureHelper != null)
			{
				this._mouseCaptureHelper.LostMouseCapture -= new MouseEventHandler(this.OnDragElementLostCapture);
				this._mouseCaptureHelper.MouseUp -= new MouseButtonEventHandler(this.OnDragElementMouseUp);
				this._mouseCaptureHelper.MouseMove -= new MouseEventHandler(this.OnDragElementMouseMove);
				this._mouseCaptureHelper.MouseDown -= new MouseButtonEventHandler(this.OnDragElementMouseDown);
				this._mouseCaptureHelper.ClearValue(FrameworkContentElement.CursorProperty); // AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
				this._mouseCaptureHelper.ReleaseMouseCapture();
			}

			this._mouseCaptureHelper = dragHelper;

			if (dragHelper != null)
			{
				Debug.Assert(dragHelper.IsMouseCaptured);
				Debug.Assert(this._dragState == DragState.Dragging);

				dragHelper.LostMouseCapture += new MouseEventHandler(this.OnDragElementLostCapture);
				dragHelper.MouseUp += new MouseButtonEventHandler(this.OnDragElementMouseUp);
				dragHelper.MouseMove += new MouseEventHandler(this.OnDragElementMouseMove);
				dragHelper.MouseDown += new MouseButtonEventHandler(this.OnDragElementMouseDown);
			}
		}
		#endregion //SetDragCaptureHelper

		#region ShowGlobalIndicators
		private void ShowGlobalIndicators()
		{
			Debug.Assert(this._indicatorWindows != null && this._indicatorWindows.Length == 5);

			for (int i = 0; i < this._indicatorWindows.Length; i++)
			{
				DockingIndicatorPosition position = (DockingIndicatorPosition)i;
				ToolWindow window = this._indicatorWindows[i];

				if (position != DockingIndicatorPosition.Center)
				{
					if (window.IsVisible == false && _dockManager.MeetsVisibleCriteria(this.GlobalIndicatorsElement) )
					{
						Output(position, "ShowGlobalIndicators");

                        // AS 3/30/09 TFS16355 - WinForms Interop
						//this._indicatorWindows[i].Show(this.GlobalIndicatorsElement, false);
                        DockManagerUtilities.ShowToolWindow(this._indicatorWindows[i], this.GlobalIndicatorsElement, false);

						// AS 5/29/08 BR33471
						this._bringWindowToFront = true;
					}
				}
			}
		} 
		#endregion //ShowGlobalIndicators

		#region StartDrag
		/// <summary>
		/// Starts a drag operation for the specified element.
		/// </summary>
		/// <param name="element">The element for which the drag is being started</param>
		/// <param name="mouseDownPosition">The location of the mouse in relation to the element for which the drag is being started</param>
		/// <param name="e">The event args for the mouse event that started the drag operation</param>
		/// <param name="processMove">True if we should process the first drag move now.</param>
		/// <param name="isWindowDragMode">True if the element is a toolwindow that will be moved by the OS</param>
		/// <returns>True if the drag was started</returns>
		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		//private bool StartDrag(FrameworkElement element, Point mouseDownPosition, MouseEventArgs e, bool processMove)
		private bool StartDrag(FrameworkElement element, Point mouseDownPosition, IInputDeviceInfo e, bool processMove, bool isWindowDragMode)
		{
			Debug.Assert(this._dragState == DragState.None);
            Debug.Assert(null != element);

            if (element == null)
                return false;

            // AS 3/13/09 FloatingWindowDragMode
            // Since we will now use this method with floating only panes, we 
            // want to continue the previous behavior and suppress the drag events.
            //
			PaneLocation elementPaneLocation = XamDockManager.GetPaneLocation(element);
            switch (elementPaneLocation)
            {
                case PaneLocation.FloatingOnly:
                    Debug.Assert(element is PaneToolWindow);
                    _canRaiseDragEvents = false;
                    _forceFloating = true;
                    break;
                default:
                    Debug.Assert(XamDockManager.GetPaneLocation(element) != PaneLocation.Unknown);
                    _forceFloating = false;
                    _canRaiseDragEvents = true;
                    break;
            }

            // AS 4/8/09 TFS16492
            //FrameworkContentElement mouseCaptureHelper = new FrameworkContentElement();
            FrameworkContentElement mouseCaptureHelper = _dockManager.DragHelper;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// If the window is floating then use the drag helper of the toolwindow
			// since we don't want to bring the main dockmanager window to the foreground. 
			// This was what VS 2008 and prior did so technically its right but this 
			// causes a problem when there are unowned windows.
			//
			if (DockManagerUtilities.IsFloating(elementPaneLocation))
			{
				PaneToolWindow tw = ToolWindow.GetToolWindow(element) as PaneToolWindow;

				if (null != tw)
				{
					mouseCaptureHelper = tw.DragHelper;

					// AS 6/15/12 TFS114774
					// We shouldn't be raising the events. Normally we wouldn't even get here but when 
					// the FloatingWindowCaptionSource is UseContentPaneCaption then our element is 
					// visible while minimized.
					//
					if (tw.WindowState == WindowState.Minimized)
					{
						Debug.Assert(isWindowDragMode, "Shouldn't we be letting the OS do the drag since the window is minimized?");
						_canRaiseDragEvents = false;
						_forceFloating = true;
					}
				}
			}

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Moved to DM
			//
			//// AS 5/17/08
			//// Set the name to make it easier to identify when debugging, bug reports, etc.
			//mouseCaptureHelper.Name = "DockManagerDragHelper";

			//// AS 4/25/08
			//// We may need to put focus into this element temporarily.
			////
			//mouseCaptureHelper.SetValue(FrameworkContentElement.FocusableProperty, KnownBoxes.TrueBox);

            // AS 4/8/09 TFS16492
            // Once created this will always be a logical child of the dockmanager.
            //
            //this._dockManager.AddLogicalChildInternal(mouseCaptureHelper);

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			PaneToolWindow paneToolWindow = element as PaneToolWindow;

			bool cancelled = false;

			if (false == cancelled)
			{
				PaneDragStartingEventArgs beforeArgs = new PaneDragStartingEventArgs(element);

				// AS 5/28/08 RaisePaneDragOverForInvalidLocations
				beforeArgs.InvalidDragActionCursor = Cursors.No;
				beforeArgs.ValidDragActionCursor = Cursors.Arrow;

				// AS 3/25/10 TFS29475
				// There was no test project or additional information other than a callstack provided.
				// The only place that I can see that may have led to an ArgumentOutOfRange on a List<T>
				// is down below where we call _panesBeingDragged[0]. That member is initialized to 
				// the Panes of these event arguments which could be a list<T> so if there are no panes 
				// in the window we'll consider the drag cancelled.
				//
				if (beforeArgs.Panes.Count == 0)
					cancelled = true;
				else
				{
					// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
					// If the tool window is maximized but cannot be dragged then default the 
					// drag operation such that it would be cancelled but let the developer 
					// decide if they want to continue anyway. Later down below we will be 
					// setting the WindowState to Normal.
					//
					if (!cancelled &&
						paneToolWindow != null &&
						paneToolWindow.WindowState == WindowState.Maximized &&
						!ToolWindow.RestoreCommand.CanExecute(null, paneToolWindow))
					{
						beforeArgs.Cancel = true;
					}

					// AS 3/13/09 FloatingWindowDragMode
					// Added if check since we will be using this for floating only panes 
					// as well and don't want to start raising the events for that.
					//
					if (_canRaiseDragEvents)
						this._dockManager.RaisePaneDragStarting(beforeArgs);

					cancelled = beforeArgs.Cancel;
				}

				// if it wasn't cancelled then cache the panes being dragged
				if (cancelled == false)
				{
					this._panesBeingDragged = beforeArgs.Panes;
					this._rootDragElement = beforeArgs.RootContainerElement;
					this._rootDragElementToolWindow = ToolWindow.GetToolWindow(_rootDragElement) as PaneToolWindow; // AS 9/20/11 TFS88634

					// AS 5/28/08 RaisePaneDragOverForInvalidLocations
					this._defaultInvalidCursor = beforeArgs.InvalidDragActionCursor;
					this._defaultValidCursor = beforeArgs.ValidDragActionCursor;
					this._raisePaneDragOverForInvalidLocations = beforeArgs.RaisePaneDragOverForInvalidLocations;
				}
			}

			// AS 2/22/12 TFS101038
			// In the case of a touch drag we are not honoring the setting so if we get in here for a 
			// window drag mode then use UseSystemWindowDrag to find out if the window will be moved.
			//
			bool dragFullWindows = _dockManager.GetDragFullWindows(isWindowDragMode ? FloatingWindowDragMode.UseSystemWindowDrag : _dockManager.FloatingWindowDragMode);

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (cancelled == false && 
				element is PaneToolWindow &&
				isWindowDragMode == false && // let the os do it in window drag mode
				dragFullWindows && // AS 6/8/11 TFS76337 - If the drag operation is deferred then defer the restore as well.
				paneToolWindow.WindowState == WindowState.Maximized)
			{
				// because the position of the window will be changed as a result of changing
				// its state, we will process a move right away. otherwise the window won't 
				// get repositioned relative to the mouse until the next move
				processMove = true;
				paneToolWindow.WindowState = WindowState.Normal;

				// if the operation fails or is in some other way interferred with 
				// then end the drag
				if (paneToolWindow.IsVisible == false ||
					paneToolWindow.WindowState != WindowState.Normal)
				{
					cancelled = true;
				}
			}

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Do not capture the mouse when the os will be dragging the window.
			//
			//if (cancelled == false)
			if ( cancelled == false && false == isWindowDragMode )
			{
				// try to establish capture in the helper element
				mouseCaptureHelper.CaptureMouse();

				cancelled = mouseCaptureHelper.IsMouseCaptured == false;
			}

			if (cancelled)
			{
				// AS 5/17/08 BR32810
				// This isn't specific to the bug but I noticed that we were setting these 
				// above but if we were canceling then we weren't clearing them
				this._panesBeingDragged = null;
				this._rootDragElement = null;

                // AS 4/8/09 TFS16492
                //// if for some reason we could not then remove the element and leave
				//this._dockManager.RemoveLogicalChildInternal(mouseCaptureHelper);
				return false;
			}

			// AS 10/13/11 TFS91945
			this.RaisePaneDragOverInternal = _canRaiseDragEvents && _dockManager.HasPaneDragOverInternalListeners;

			// AS 5/17/08 BR32810
			// Cache the pane that was last active and move focus to our helper class. The pane
			// being dragged is not supposed to be active. We'll also store the last focused
			// element and try to restore focus to that first when the drag ends.
			//
			this._lastActivePane = this._dockManager.ActivePane;
			this._lastFocusedElement = Keyboard.FocusedElement;
            // AS 2/26/09 TFS14668
            // If focus is within a floating window then calling focus on the FCE will
            // cause focus to shift to the DM first. When the DM gets focus it then tries 
            // to focus its active pane which causes the flyout to be shown.
            //
			//mouseCaptureHelper.Focus();

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Moving down
			//
			//_dockManager.ForceFocus(mouseCaptureHelper);

			Output(this._lastActivePane, "Cached ActivePane");
			Output(this._lastFocusedElement, "Cached FocusedElement");

			this._dropInfo = new DropInfo();

            // AS 3/13/09 FloatingWindowDragMode
            this._dragFullWindows = dragFullWindows;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Don't use the preview when the drag mode calls for letting the os do the drag
			// and the toolwindow will use a Window to host the content.
			//
			_deferFloatingWindowPositioning = !_dragFullWindows &&
				!isWindowDragMode && // AS 6/15/12 TFS114774 - Not if we're going to switch to letting the os do the drag.
				(!ToolWindow.WillHostInWindow(_dockManager) || _dockManager.FloatingWindowDragMode != FloatingWindowDragMode.UseSystemWindowDrag);

			// AS 11/12/09 TFS24789 - TabItemDragBehavior
			this._tabItemDragBehavior = _dockManager.TabItemDragBehavior;

			this._windowBeingDragged = element as PaneToolWindow;
			this._initialDragElement = element;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			this.IsInWindowDragMode = isWindowDragMode;

			// if this is a tab item store the point relative to the tab header area itself
			if (element is PaneTabItem)
			{
				TabGroupPane group = ItemsControl.ItemsControlFromItemContainer(element) as TabGroupPane;

				if (null != group && group.HeaderArea != null && element.IsDescendantOf(group.HeaderArea))
				{
					mouseDownPosition = element.TransformToAncestor(group.HeaderArea).Transform(mouseDownPosition);
				}
			}

			this._mouseDownLocation = mouseDownPosition;

			// store a reference to the frameworkelement
			this._dragState = DragState.Dragging;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Do not capture the mouse if the os is dragging the window.
			//
			if (isWindowDragMode == false)
				this.SetDragCaptureHelper(mouseCaptureHelper);

			// create a class to catch events when keys are pressed while in a drag operation
			this._keyFocusWatcher = new KeyEventFocusWatcher();
			this._keyFocusWatcher.KeyEvent += new KeyEventHandler(OnKeyFocusWatcherKeyEvent);

			this._globalIndicatorState = new IndicatorEnabledState();
			this._indicatorStates = new Dictionary<FrameworkElement, IndicatorEnabledState>();
			this._dragResults = new Dictionary<PaneDragAction, DragResult>(new DragActionComparer());

			// AS 5/28/08 RaisePaneDragOverForInvalidLocations
			// We never want to raise the panedragover with the internal InvalidDropLocation drag
			// action so add that result before starting the drag and use the default invalid
			// cursor as its cursor.
			//
			this._dragResults.Add(InvalidDropLocation.Instance, new DragResult(this._defaultInvalidCursor, false));

			this.CreateIndicators();

			// assume we can drop everywhere
			this._allowedLocations = this.GetAllowedDropLocations(this._panesBeingDragged);

			this._isDraggingDocuments = XamDockManager.GetPaneLocation(this._panesBeingDragged[0]) == PaneLocation.Document;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Moved the focus call down from above since this will trigger focus events 
			// which could cause us to do something when we haven't finished preparing 
			// for the drag. I'm also changing the impl since we don't want to take focus 
			// when the os will be moving the window. Also, if this is the capture helper 
			// from the toolwindow then focus it directly.
			//
			//_dockManager.ForceFocus(mouseCaptureHelper);
			if (isWindowDragMode == false && _mouseCaptureHelper == mouseCaptureHelper)
			{
				if (LogicalTreeHelper.GetParent(mouseCaptureHelper) is ToolWindow)
					mouseCaptureHelper.Focus();
				else
					_dockManager.ForceFocus(mouseCaptureHelper);

				Debug.Assert(mouseCaptureHelper.IsKeyboardFocused);
			}

			Output(string.Format("Element: {0}, Point={1}", element, mouseDownPosition), "Start Drag");

			// process the first move now as long as the mouse is different then the original mouse down point
			// this would happen you mouse down on a floating pane window caption
			if (processMove)
				this.ProcessDragMove(e, null);

			return true;
		}
		#endregion //StartDrag

		#region StoreIndicatorState
		private void StoreIndicatorState(DockingIndicator indicator, DockingIndicatorPosition position, bool isEnabled)
		{
			IndicatorEnabledState enabledState;

			if (indicator.Position == DockingIndicatorPosition.Center)
			{
				ToolWindow window = ToolWindow.GetToolWindow(indicator);
				Debug.Assert(null != window && null != window.Owner);
				if (false == this._indicatorStates.TryGetValue(window.Owner, out enabledState))
					return;
			}
			else
				enabledState = _globalIndicatorState;


			switch (position)
			{
				case DockingIndicatorPosition.Bottom:
					enabledState.Bottom = isEnabled;
					break;
				case DockingIndicatorPosition.Left:
					enabledState.Left = isEnabled;
					break;
				case DockingIndicatorPosition.Right:
					enabledState.Right = isEnabled;
					break;
				case DockingIndicatorPosition.Top:
					enabledState.Top = isEnabled;
					break;
				case DockingIndicatorPosition.Center:
					enabledState.Center = isEnabled;
					break;
			}
		}
		#endregion //StoreIndicatorState

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region SwitchToWindowDrag
		[MethodImpl(MethodImplOptions.NoInlining)]
		private void SwitchToWindowDrag()
		{
			// start it async since the operation is modal
			_dockManager.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Infragistics.Windows.DockManager.DockManagerUtilities.MethodInvoker(OnDelayedSwitchToWindowDrag));
		}

		private void OnDelayedSwitchToWindowDrag()
		{
			if (this.DragState != DragState.Dragging)
				return;

			ToolWindow toolWindow = this.FloatingWindow;

			if (null == toolWindow)
				return;

			Debug.Assert(toolWindow != null && toolWindow.Host != null && toolWindow.Host.IsWindow, "Can only do this for a toolwindow shown in a Window");
			
			// unhook the events of the drag helper
			this.SetDragCaptureHelper(null);
			
			// set the flag right away because the dragmove is a modal loop
			this.IsInWindowDragMode = true;

			MouseDevice mouse = Mouse.PrimaryDevice;

			// AS 6/24/11 FloatingWindowCaptionSource
			// There are cases where the button state indicates the button is release which can result 
			// in an exception if we try to start a drag move so its better to just skip the call.
			//
			if (mouse == null || mouse.LeftButton != MouseButtonState.Pressed)
			{
				this.EndDrag(true);
				return;
			}

			// start the drag move
			toolWindow.Host.DragMove(new MouseEventArgs(mouse, 0));
		}
		#endregion //SwitchToWindowDrag

		#region UnhookElementPendingDrag
		private void UnhookElementPendingDrag(FrameworkElement element)
		{
			element.RemoveHandler(FrameworkElement.LostMouseCaptureEvent, new MouseEventHandler(this.OnPendingDragElementLostCapture));
			element.RemoveHandler(FrameworkElement.MouseUpEvent, new MouseButtonEventHandler(this.OnPendingDragElementMouseUp));
			element.RemoveHandler(FrameworkElement.MouseMoveEvent, new MouseEventHandler(this.OnPendingDragElementMouseMove));
		}
		#endregion //UnhookElementPendingDrag

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region VerifyControlKeyStateChanged
		private void VerifyControlKeyStateChanged()
		{
			if (_dragMoveCache != null && _dragState == DragState.Dragging && !_forceFloating)
			{
				if (_dragMoveCache.WasControlKeyDown != IsControlKeyDown)
				{
					this.ProcessDragMove(_dragMoveCache.DeviceInfo, _dragMoveCache.WindowRect);
				}
			}
		}
		#endregion //VerifyControlKeyStateChanged

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region VerifySystemControlKeyState
		private void VerifySystemControlKeyState()
		{
			this.VerifyControlKeyStateChanged();
		}
		#endregion //VerifySystemControlKeyState

		#endregion //Private Methods

		#endregion //Methods

		#region DragResult class
		private class DragResult
		{
			private Cursor _cursor;
			private bool _isValid;

			internal DragResult(Cursor cursor, bool isValid)
			{
				this._isValid = isValid;
				this._cursor = cursor;
			}

			internal Cursor Cursor
			{
				get { return this._cursor; }
			}

			internal bool IsValid
			{
				get { return this._isValid; }
			}
		} 
		#endregion //DragResult class

		#region DragToolWindow class
		private class DragToolWindow : ToolWindow
		{
			static DragToolWindow()
			{
				// AS 5/9/08
				// register the groupings that should be applied when the theme property is changed
				ThemeManager.RegisterGroupings(typeof(DragToolWindow), new string[] { PrimitivesGeneric.Location.Grouping, DockManagerGeneric.Location.Grouping });
			}

            // AS 3/13/09 FloatingWindowDragMode
            // Refactored duplicate code into a helper method.
            //
            internal static DragToolWindow Create(XamDockManager dockManager)
            {
                Debug.Assert(null != dockManager);

                DragToolWindow window = new DragToolWindow();

                window.ResizeMode = ResizeMode.NoResize;
                window.UseOSNonClientArea = false;
                // AS 3/30/09 TFS16355 - WinForms Interop
                //window.Template = IndicatorToolWindowTemplate;
                window.Template = GetIndicatorToolWindowTemplate(dockManager);
                window.SetValue(XamDockManager.DockManagerPropertyKey, dockManager);

				// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
				window.IsOwnedWindow = false;

				// AS 7/14/09 TFS18424
				window.SetBinding(FrameworkElement.FlowDirectionProperty, Utilities.CreateBindingObject(FrameworkElement.FlowDirectionProperty, System.Windows.Data.BindingMode.OneWay, dockManager));

                // AS 5/21/08
                // Instead of binding the theme property, we will add the resources of the 
                // dockmanager to the window's resources. That will pick up the theme property
                // resources and any other resources they put in.
                //
                //window.SetBinding(ToolWindow.ThemeProperty, Utilities.CreateBindingObject(XamDockManager.ThemeProperty, System.Windows.Data.BindingMode.OneWay, this._dockManager));
                if (null != dockManager)
                    window.Resources.MergedDictionaries.Add(dockManager.Resources);

                return window;

            }

			#region Base class overrides

			// AS 8/4/11 TFS83465/TFS83469
			#region KeepOnScreen
			internal override bool KeepOnScreen
			{
				get
				{
					return false;
				}
			}
			#endregion //KeepOnScreen

			#endregion //Base class overrides
		} 
		#endregion // DragToolWindow class

		#region DropInfo class
		private class DropInfo
		{
			public PaneDragAction Action;
			public bool ShowGlobalIndicators;
			public bool ShowFloatingWindow;
			public bool HideCenterIndicator;
			public bool HideDropPreview;

			// AS 5/28/08 RaisePaneDragOverForInvalidLocations
			public DependencyObject CenterIndicatorElement;
			public PaneLocation HitTestLocation;

			internal DropInfo()
			{
				this.Reset();
			}

			internal void Reset()
			{
				this.Action = null;
				this.ShowFloatingWindow = true;
				this.ShowGlobalIndicators = false;
				this.HideCenterIndicator = true;
				this.HideDropPreview = true;

				// AS 5/28/08 RaisePaneDragOverForInvalidLocations
				this.CenterIndicatorElement = null;
				this.HitTestLocation = PaneLocation.Unknown;
			}
		}
		#endregion //DropInfo class

		#region IndicatorEnabledState class
		private class IndicatorEnabledState
		{
			#region Member Variables

			private static BitVector32.Section _leftSection = BitVector32.CreateSection(0xff);
			private static BitVector32.Section _rightSection = BitVector32.CreateSection(0xff, _leftSection);
			private static BitVector32.Section _topSection = BitVector32.CreateSection(0xff, _rightSection);
			private static BitVector32.Section _bottomSection = BitVector32.CreateSection(0xff, _topSection);
			private static BitVector32.Section _centerSection = BitVector32.CreateSection(0xfff);
			private const int Unset = 0x00;
			private const int Disabled = 0x01;
			private const int Enabled = 0x10;

			private BitVector32 _flags;
			private bool? _center;

			#endregion //Member Variables

			#region Properties
			internal bool? Center
			{
				get { return GetValue(_centerSection); }
				set { SetValue(_centerSection, value); }
			}

			internal bool? Left
			{
				get { return GetValue(_leftSection); }
				set { SetValue(_leftSection, value); }
			}

			internal bool? Right
			{
				get { return GetValue(_rightSection); }
				set { SetValue(_rightSection, value); }
			}

			internal bool? Top
			{
				get { return GetValue(_topSection); }
				set { SetValue(_topSection, value); }
			}

			internal bool? Bottom
			{
				get { return GetValue(_bottomSection); }
				set { SetValue(_bottomSection, value); }
			}
			#endregion //Properties

			#region Methods
			private bool? GetValue(BitVector32.Section section)
			{
				if (section == _centerSection)
					return _center;

				int value = this._flags[section];

				if (value == 0)
					return null;
				else
					return value == Enabled;
			}

			private void SetValue(BitVector32.Section section, bool? value)
			{
				if (section == _centerSection)
					_center = value;
				else
				{
					int intVal = value == null ? 0 : value.Value ? Enabled : Disabled;
					this._flags[section] = intVal;
				}
			}

			private static object GetDependencyValue(bool? value)
			{
				if (value == null)
					return DependencyProperty.UnsetValue;
				else if (value.Value)
					return KnownBoxes.TrueBox;
				else
					return KnownBoxes.FalseBox;
			}

			internal void Initialize(DockingIndicator indicator)
			{
				indicator.SetValue(DockingIndicator.CanDockCenterPropertyKey, GetDependencyValue(this.GetValue(_centerSection)));
				indicator.SetValue(DockingIndicator.CanDockLeftPropertyKey, GetDependencyValue(this.GetValue(_leftSection)));
				indicator.SetValue(DockingIndicator.CanDockRightPropertyKey, GetDependencyValue(this.GetValue(_rightSection)));
				indicator.SetValue(DockingIndicator.CanDockTopPropertyKey, GetDependencyValue(this.GetValue(_topSection)));
				indicator.SetValue(DockingIndicator.CanDockBottomPropertyKey, GetDependencyValue(this.GetValue(_bottomSection)));
			}
			#endregion //Methods
		}
		#endregion //IndicatorEnabledState class

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region IInputDeviceInfo interface
		private interface IInputDeviceInfo
		{
			Point GetPosition(IInputElement relativeTo);
			Point GetScreenPoint(FrameworkElement relativeElement);

			// AS 6/8/11 TFS76337
			// I added this since we are passing this interface into the ProcessMouseDown 
			// and that routine needed to know what mouse button was involved.
			//
			MouseButton? ChangedButton { get; }
		}
		#endregion //IInputDeviceInfo interface

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region MouseArgsInputDeviceInfo class
		private class MouseArgsInputDeviceInfo : IInputDeviceInfo
		{
			#region Member Variables

			private MouseEventArgs _args;

			#endregion //Member Variables

			#region Constructor
			internal MouseArgsInputDeviceInfo(MouseEventArgs args)
			{
				_args = args;
			}
			#endregion //Constructor

			#region IInputDeviceInfo Members

			Point IInputDeviceInfo.GetPosition(IInputElement relativeTo)
			{
				return _args.GetPosition(relativeTo);
			}

			Point IInputDeviceInfo.GetScreenPoint(FrameworkElement relativeElement)
			{
				return ToolWindow.GetScreenPoint(relativeElement, _args);
			}

			// AS 6/8/11 TFS76337
			MouseButton? IInputDeviceInfo.ChangedButton
			{
				get
				{
					MouseButtonEventArgs mbea = _args as MouseButtonEventArgs;
					return mbea == null ? (MouseButton?)null : mbea.ChangedButton;
				}
			}
			#endregion //IInputDeviceInfo Members
		} 
		#endregion //MouseArgsInputDeviceInfo class

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region ScreenInputDeviceInfo class
		private class ScreenInputDeviceInfo : IInputDeviceInfo
		{
			#region Member Variables

			private Point _screenPoint;

			#endregion //Member Variables

			#region Constructor
			internal ScreenInputDeviceInfo(Point screenPoint)
			{
				_screenPoint = screenPoint;
			}
			#endregion //Constructor

			#region IInputDeviceInfo Members

			Point IInputDeviceInfo.GetPosition(IInputElement relativeTo)
			{
				DependencyObject d = relativeTo as DependencyObject;

				while (d != null && d is Visual == false)
				{
					d = Utilities.GetParent(d, true);
				}

				Debug.Assert(d != null);

				if (d == null)
					return new Point();

				return Utilities.PointFromScreenSafe(d as Visual, _screenPoint);
			}

			Point IInputDeviceInfo.GetScreenPoint(FrameworkElement relativeElement)
			{
				Point relativePt = Utilities.PointFromScreenSafe(relativeElement, _screenPoint);
				// AS 8/22/11 TFS84326
				//return relativeElement.PointToScreen(relativePt); ;
				return Utilities.PointToScreenSafe(relativeElement,relativePt); ;
			}

			// AS 6/8/11 TFS76337
			MouseButton? IInputDeviceInfo.ChangedButton
			{
				get { return null; }
			}
			#endregion //IInputDeviceInfo Members
		} 
		#endregion //ScreenInputDeviceInfo class

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region DragMoveCache
		private class DragMoveCache
		{
			internal Rect? WindowRect;
			internal IInputDeviceInfo DeviceInfo;
			internal bool WasControlKeyDown;
		}
		#endregion //DragMoveCache
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