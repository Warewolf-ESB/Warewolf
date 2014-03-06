using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.Selection
{
	#region Class SelectionStrategyBase

	/// <summary>
    /// Abstract base class for selection strategies
    /// </summary>
    public abstract class SelectionStrategyBase
    {
        #region Private Members

		private ISelectionHost				_selectionHost;

		private DispatcherTimer				_verticalAutoScrollTimer;
		private DispatcherTimer				_horizontalAutoScrollTimer;
		private bool						_inDragMode;
		private bool						_ignoreNextMouseMove;
		private bool						_isReleasingMouseCapture;
		private double						_initialXPos;
		private double						_initialYPos;

		private SelectionState				_currentSelectionState = SelectionState.SingleSelect;

        // JJD 10/10/14
        // Changed to a weak reference so we don't root the item
		//private ISelectableItem				_currentSelectableItem;
		private WeakReference				_currentSelectableItemRef;

		private AutoScrollManager			_autoScrollManagerVertical;
		private AutoScrollManager			_autoScrollManagerHorizontal;

		// AS 4/12/11 TFS62951
		// If the mouse is moving while the mouse is outside the area containing the 
		// selectable items and the interval changes (which it pretty much would 
		// based on the delta from the scrollable area) then we end up stopping 
		// and restarting the timer a lot with the new interval losing whatever time 
		// had transpired since we started the timer initially even though we never 
		// really meant to stop it. To try and retain the amount of time that we 
		// had waited we'll start a stopwatch when we start the timer initially. 
		// I'm using a stopwatch since it provides better accuracy than tickcount 
		// or now/utcnow was providing.
		//
		private Stopwatch					_autoScrollStopwatchHorizontal;
		private Stopwatch					_autoScrollStopwatchVertical;

        #endregion //Private Members	
            
        #region Constructor

        /// <summary>
		/// Initializes a new instance of the <see cref="SelectionStrategyBase"/> class
        /// </summary>
        /// <param name="selectionHost">The selection host for which the selection strategy will interact</param>
        protected SelectionStrategyBase(ISelectionHost selectionHost)
        {
			if (selectionHost == null)
				throw new ArgumentNullException( "selectionHost", SR.GetString( "LE_ArgumentNullException_2" ) );

			this._selectionHost = selectionHost;
        }

        #endregion //Constructor

		#region Enumerations

			#region SelectionState

		/// <summary>
		/// The potential selection states of the strategy.
		/// </summary>
		protected enum SelectionState
		{
			/// <summary>
			/// Currently in single selection state.
			/// </summary>
			SingleSelect = 0,

			/// <summary>
			/// Currently in pre-drag state.
			/// </summary>
			PreDrag = 1,

			/// <summary>
			/// Currently in pre-edit state.
			/// </summary>
			PreEdit = 2,

			/// <summary>
			/// Currently in potential pre-drag state.
			/// </summary>
			PotentialPreDrag = 3,

			/// <summary>
			/// Currently in extSelect state.
			/// </summary>
			ExtSelect = 4,

			/// <summary>
			/// Currently in ControlClick state.
			/// </summary>
			ControlClick = 5
		};

			#endregion //SelectionState

		#endregion //Enumerations

		#region Properties

			#region Public Properties

				#region IsMultiSelect

		/// <summary>
        /// True if this strategy supports selecting more than one 
        /// item at a time (read-only).
        /// </summary>
        public virtual bool IsMultiSelect { get { return false; } }

                #endregion //IsMultiSelect	
    
                #region IsSingleSelect

        /// <summary>
        /// True if this strategy supports selecting only one 
        /// item at a time (read-only).
        /// </summary>
        public virtual bool IsSingleSelect { get { return false; } }

                #endregion //IsSingleSelect	
    
				#region IsReleasingMouseCapture

		/// <summary>
        /// True while the mouse capture is being toggled off (read-only).
        /// </summary>
		public bool IsReleasingMouseCapture { get { return this._isReleasingMouseCapture; } }

                #endregion //IsReleasingMouseCapture	

            #endregion //Public Properties

			#region Protected Properties

				#region AutoScrollHorizontalIntervalMax

		/// <summary>
		/// Returns the number of milliseconds between consecutive horizontal auto scrolls when
		/// auto scrolling at the slowest rate.
		/// </summary>
		protected virtual int AutoScrollHorizontalIntervalMax
		{
			get { return 300; }
		}

				#endregion //AutoScrollHorizontalIntervalMax

				#region AutoScrollHorizontalIntervalMin

		/// <summary>
		/// Returns the number of milliseconds between consecutive horizontal auto scrolls when
		/// auto scrolling at the fastest rate.
		/// </summary>
		protected virtual int AutoScrollHorizontalIntervalMin
		{
			get { return 10; }
		}

				#endregion //AutoScrollHorizontalIntervalMin

				#region AutoScrollVerticalIntervalMax

		/// <summary>
		/// Returns the number of milliseconds between consecutive vertical auto scrolls when
		/// auto scrolling at the slowest rate.
		/// </summary>
		protected virtual int AutoScrollVerticalIntervalMax
		{
			get { return 300; }
		}

				#endregion //AutoScrollVerticalIntervalMax

				#region AutoScrollVerticalIntervalMin

		/// <summary>
		/// Returns the number of milliseconds between consecutive vertical auto scrolls when
		/// auto scrolling at the fastest rate.
		/// </summary>
		protected virtual int AutoScrollVerticalIntervalMin
		{
			get { return 10; }
		}

				#endregion //AutoScrollVerticalIntervalMin

				#region AutoScrollTimerDispatcherPriority

		/// <summary>
		/// Returns the dispatcher priority for the autoscroll timers
		/// </summary>
		protected virtual DispatcherPriority AutoScrollTimerDispatcherPriority
		{
			get { return DispatcherPriority.Input; }
		}

				#endregion //AutoScrollTimerDispatcherPriority

				#region CurrentSelectableItem

		/// <summary>
		/// Returns/sets the current selectable item for the strategy.
		/// </summary>
		protected ISelectableItem CurrentSelectableItem
		{
			get 
            {
                
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

                return null != _currentSelectableItemRef ? Utilities.GetWeakReferenceTargetSafe(_currentSelectableItemRef) as ISelectableItem : null;
            }
			set 
            {
                // JJD 10/10/14
                // Changed to a weak reference so we don't root the item
                //this._currentSelectableItem = value; 
                if (value == null)
                    this._currentSelectableItemRef = null;
                else
                    this._currentSelectableItemRef = new WeakReference(value); 
            }
		}

				#endregion //CurrentSelectableItem

				#region CurrentSelectionState

		/// <summary>
		/// Returns/sets the current state of the selection for the strategy.
		/// </summary>
		protected SelectionState CurrentSelectionState
		{
			get { return this._currentSelectionState; }
			set { this._currentSelectionState = value; }
		}

				#endregion //CurrentSelectionState

				#region DragThreshold

		/// <summary>
		/// Returns a value which represents the amount that the mouse must move from the inital MouseDown
		/// position before dragging will start.
		/// </summary>
		protected virtual int DragThreshold
		{
			get { return 3; }
		}

				#endregion //DragThreshold

				#region IgnoreNextMouseMove

		/// <summary>
		/// Returns/sets whether the next mouse move should be ignored.
		/// </summary>
		// AS 8/11/08   +   JM 09-10-08 TFS5972 BR32616
		//protected bool IgnoreNextMouseMove
		internal protected bool IgnoreNextMouseMove
		{
			get { return this._ignoreNextMouseMove; }
			set { this._ignoreNextMouseMove = value; }
		}

				#endregion IgnoreNextMouseMove

				#region InDragMode

		/// <summary>
		/// Returns/sets whether the strategy is currently in drag mode.
		/// </summary>
		protected bool InDragMode
		{
			get { return this._inDragMode; }
			set { this._inDragMode = value; }
		}

				#endregion //InDragMode

				#region InitialXPos

		/// <summary>
		/// Returns/sets the X coordinate of the mouse when the initial MouseDown was received.
		/// </summary>
		protected double InitialXPos
		{
			get { return this._initialXPos; }
			set { this._initialXPos = value; }
		}

				#endregion //InitialXPos

				#region InitialYPos

		/// <summary>
		/// Returns/sets the Y coordinate of the mouse when the initial MouseDown was received.
		/// </summary>
		protected double InitialYPos
		{
			get { return this._initialYPos; }
			set { this._initialYPos = value; }
		}

				#endregion //InitialYPos

				#region IsAnyMouseButtonDown

		/// <summary>
		/// Returns true if any mouse button is currently pressed.
		/// </summary>
		protected bool IsAnyMouseButtonDown
		{
			get { return	Mouse.LeftButton	== MouseButtonState.Pressed  ||
							Mouse.MiddleButton	== MouseButtonState.Pressed  ||
							Mouse.RightButton	== MouseButtonState.Pressed; }
		}

				#endregion //IsAnyMouseButtonDown

				#region IsCtrlKeyPressed

		/// <summary>
		/// Returns true if the CTRK key is currently pressed.
		/// </summary>
		protected bool IsCtrlKeyPressed
		{
			get { return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl); }
		}

				#endregion //IsCtrlKeyPressed

				#region IsLeftMouseButtonDown

		/// <summary>
		/// Returns true if the left mouse button is currently pressed
		/// </summary>
		protected bool IsLeftMouseButtonDown
		{
			get { return Mouse.LeftButton == MouseButtonState.Pressed; }
		}

				#endregion //IsLeftMouseButtonDown

				#region IsShiftKeyPressed

		/// <summary>
		/// Returns true if the shift key is currently pressed.
		/// </summary>
		protected bool IsShiftKeyPressed
		{
			get { return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift); }
		}

				#endregion //IsShiftKeyPressed

				#region PixelThresholdForHorizontalAutoScroll

		/// <summary>
		/// Returns the number of pixels inside the left or right edge of the ScrollArea within which
		/// auto scrolling will take place
		/// </summary>
		protected virtual int PixelThresholdForHorizontalAutoScroll
		{
			get { return 20; }
		}

				#endregion //PixelThresholdForHorizontalAutoScroll

				#region PixelThresholdForVerticalAutoScroll

		/// <summary>
		/// Returns the number of pixels inside the top or bottom edge of the ScrollArea within which
		/// auto scrolling will take place
		/// </summary>
		protected virtual int PixelThresholdForVerticalAutoScroll
		{
			get { return 20; }
		}

				#endregion //PixelThresholdForVerticalAutoScroll

				#region SelectionHost

		/// <summary>
		/// Returns the SelectionHost (usually the control) for the strategy.
		/// </summary>
		protected ISelectionHost SelectionHost
		{
			get { return this._selectionHost; }
		}

				#endregion //SelectionHost

			#endregion //Protected Properties

		#endregion //Properties

		#region Methods

			#region Public Methods

				#region CancelPendingOperations

		/// <summary>
        /// Called to cancel any pending mouse drag operation
        /// </summary>
        public virtual void CancelPendingOperations()
        {
			// SSP 5/21/09 TFS17816
			// 
			// ----------------------------------------------------------------------------
			if ( this.InDragMode )
			{
				if ( null != this.SelectionHost )
					this.SelectionHost.OnDragEnd( true );

				this.InDragMode = false;
			}

			// cancel the timers 
			this.StopHorizontalAutoScrollTimer( );
			this.StopVerticalAutoScrollTimer( );
			// ----------------------------------------------------------------------------
        }

                #endregion //CancelPendingOperations	
    
                #region CanItemBeNavigatedTo

        /// <summary>
        /// Determines if a <see cref="ISelectableItem"/> can be navigated to.
        /// </summary>
        /// <param name="item">The selectable item</param>
        /// <param name="shiftKeyDown"><b>true</b> if shift key is depressed.</param>
		/// <param name="controlKeyDown"><b>true</b> if Ctrl key is depressed.</param>
        /// <returns>True if the itme can be navigated to.</returns>
        public virtual bool CanItemBeNavigatedTo(ISelectableItem item,
                                                bool shiftKeyDown,
                                                bool controlKeyDown)
        {
            return item != null && item.IsTabStop;
        }

                #endregion //CanItemBeNavigatedTo	
    
				// AS 4/20/07
				// We discussed this and decided that there should be a central helper method so we don't have to change
				// each controls implementation whenever the enum is updated.
				//
				#region GetSelectionStrategy
		/// <summary>
		/// Returns one of the built in <see cref="SelectionStrategyBase"/> derived classes based on the specified <see cref="SelectionType"/>
		/// </summary>
		/// <param name="type">An enum indicating the type of selection strategy that should be returned.</param>
		/// <param name="host">The <see cref="ISelectionHost"/> for which the selection strategy is being created</param>
		/// <param name="oldStrategy">The previous strategy that was used by the host or null if the host was not associated with a selection strategy</param>
		/// <returns>If the <paramref name="oldStrategy"/> is of the appropriate type and its <see cref="SelectionHost"/> is <paramref name="host"/>, that instance will be returned. Otherwise, a new <see cref="SelectionStrategyBase"/> derived class will be created based on the <paramref name="type"/> and using the specified <paramref name="host"/> as its <b>SelectionHost</b>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="host"/> is a null reference (<b>Nothing</b> in Visual Basic)</exception>
		public static SelectionStrategyBase GetSelectionStrategy(SelectionType type, ISelectionHost host, SelectionStrategyBase oldStrategy)
		{
			if (host == null)
				throw new ArgumentNullException("host");

			// use the old one by default as long as its for the same host
			SelectionStrategyBase newStrategy = oldStrategy != null && oldStrategy._selectionHost == host ? oldStrategy : null;

			// AS 6/4/09
			// Since we have several types that derive from SelectionStrategyExtended 
			// and SelectionStrategySingle, I made a helper method that returns 
			// the SelectionType for a given strategy in the derived type order 
			// so we just do the check here to see if the existing one is valid 
			// and if so return it.
			//
			if (null != newStrategy && GetSelectionType(newStrategy) == type)
				return newStrategy;

			switch (type)
			{
				default:
				case SelectionType.Default:
				case SelectionType.None:
					Debug.Assert(type != SelectionType.Default, "We should be handed a resolved selection type!");
					Debug.Assert(type == SelectionType.Default || type == SelectionType.None, "Unexpected selection type - " + type.ToString());

					// AS 6/4/09
					//if (newStrategy is SelectionStrategyNone == false)
						newStrategy = new SelectionStrategyNone(host);
					break;
				case SelectionType.Extended:
					// AS 6/4/09
					//// AS 2/19/09 TFS11612
					////if (newStrategy is SelectionStrategyExtended == false)
					//if (newStrategy is SelectionStrategyExtended == false ||
					//    newStrategy is SelectionStrategyContiguous)
						newStrategy = new SelectionStrategyExtended(host);
					break;
				case SelectionType.Single:
					// AS 6/4/09
					//if (newStrategy is SelectionStrategySingle == false)
						newStrategy = new SelectionStrategySingle(host);
					break;
                // AS 7/16/08 NA 2008 Vol 2
                case SelectionType.Range:
					// AS 6/4/09
					//if (newStrategy is SelectionStrategyContiguous == false)
                        newStrategy = new SelectionStrategyContiguous(host);
                    break;
				// AS 6/4/09
				case SelectionType.ExtendedAutoDrag:
					newStrategy = new SelectionStrategyExtendedAutoDrag(host);
					break;
				case SelectionType.SingleAutoDrag:
					newStrategy = new SelectionStrategySingleAutoDrag(host);
					break;
			}

			return newStrategy;
		} 
				#endregion //GetSelectionStrategy

                #region IsMultiSelectStrategy
        /// <summary>
        /// Returns a boolean indicating if the default <see cref="SelectionStrategyBase"/> for a given <see cref="SelectionType"/> allows multiple selection.
        /// </summary>
        /// <param name="selectionType">The selection type to evaluate</param>
        /// <returns>True for selection types such as <b>Extended</b> and <b>Range</b>; false for types such as <b>Single</b> and <b>None</b>.</returns>
        public static bool IsMultiSelectStrategy(SelectionType selectionType)
        {
            switch (selectionType)
            {
                case SelectionType.Extended:
                case SelectionType.Range:
				case SelectionType.ExtendedAutoDrag:
                    return true;
                case SelectionType.None:
                case SelectionType.Single:
				case SelectionType.SingleAutoDrag:
                    return false;
                default:
                case SelectionType.Default:
                    Debug.Fail("Unexpected selection type");
                    return false;
            }
        }   
                #endregion //IsMultiSelectStrategy

                #region IsSingleSelectStrategy
        /// <summary>
        /// Returns a boolean indicating if the default <see cref="SelectionStrategyBase"/> for a given <see cref="SelectionType"/> is used to perform single item selection.
        /// </summary>
        /// <param name="selectionType">The selection type to evaluate</param>
        /// <returns>True for selection types such as <b>Single</b>; false for types such as <b>Extended</b> and <b>None</b>.</returns>
        public static bool IsSingleSelectStrategy(SelectionType selectionType)
        {
            switch (selectionType)
            {
                case SelectionType.Single:
				case SelectionType.SingleAutoDrag:
                    return true;
                case SelectionType.Extended:
                case SelectionType.Range:
                case SelectionType.None:
				case SelectionType.ExtendedAutoDrag:
                    return false;
                default:
                case SelectionType.Default:
                    Debug.Fail("Unexpected selection type");
                    return false;
            }
        }   
                #endregion //IsSingleSelectStrategy

                #region OnMouseLeftButtonDown

        /// <summary>
        /// Called when a left mouse down is received. 
        /// </summary>
        /// <param name="item">The selectable item that the mouse is over (may be null)</param>
        /// <param name="e">The mouse event args</param>
        /// <returns>Returning true on a mouse down message will cause the mouse to be captured</returns>
        /// <remarks>
        /// Note: The <see cref="OnMouseMove"/> and <see cref="OnMouseLeftButtonUp"/> 
        /// methods are called only if this method returns true and thereby captured the mouse.
        /// </remarks>
        public abstract bool OnMouseLeftButtonDown(ISelectableItem item, System.Windows.Input.MouseEventArgs e);

                #endregion //OnMouseLeftButtonDown

                #region OnMouseMove

        /// <summary>
        /// Called when mouse move message is received. 
        /// </summary>
        /// <param name="item">The selectable item that the mouse is over (may be null)</param>
        /// <param name="e">The mouse event args</param>
        /// <returns>Returning false on a mouse down message will cause the mouse to be released.</returns>
        public abstract bool OnMouseMove(ISelectableItem item, System.Windows.Input.MouseEventArgs e);

                #endregion //OnMouseMove

                #region OnMouseLeftButtonUp

        /// <summary>
        /// Called when a left mouse up is received while the mouse is captured. 
        /// </summary>
        /// <param name="item">The selectable item that the mouse is over (may be null)</param>
        /// <param name="e">The mouse event args</param>
        public abstract void OnMouseLeftButtonUp(ISelectableItem item, System.Windows.Input.MouseEventArgs e);

                #endregion //OnMouseLeftButtonUp	
    
                #region SelectItemViaKeyboard

       /// <summary>
       /// Called to select an item as a result of a key press
       /// </summary>
       /// <param name="item">The item to select</param>
       /// <param name="shiftKeyDown">True if the shift key is pressed</param>
       /// <param name="controlKeyDown">True if the control key is pressed.</param>
		/// <param name="forceToggle">Toggle the existing selection.</param>
       /// <returns>True is successful.</returns>
		public abstract bool SelectItemViaKeyboard(ISelectableItem item, bool shiftKeyDown, bool controlKeyDown, bool forceToggle);

                #endregion //SelectItemViaKeyboard	
    
            #endregion //Public Methods	

			#region Protected Methods

				#region DragMove

		/// <summary>
        /// Used to verify the drag timers and call the <see cref="ISelectionHost.OnDragMove(MouseEventArgs)"/> 
		/// </summary>
		protected void DragMove(System.Windows.Input.MouseEventArgs e)
		{
			this.ManageTimers(e);
			this.SelectionHost.OnDragMove(e);
		}

				#endregion //DragMove

				#region DragStart

		/// <summary>
		/// Called when dragging is to be started.
		/// It calls OnDragStart off the SelectionManager.
		/// </summary>
		protected void DragStart(System.Windows.Input.MouseEventArgs mouseEventArgs)
		{
			if (null != this.SelectionHost && null != this.CurrentSelectableItem)
			{
				bool okToDrag = this.SelectionHost.OnDragStart(this.CurrentSelectableItem, mouseEventArgs);

				// Just in case.
				okToDrag = okToDrag && this.IsAnyMouseButtonDown;

				if (okToDrag)
					this.InDragMode = true;
				else
				{
					// Don't release capture here. Call OnDragStartCancelled method which
					// is virtual and default implementation releases the capture. For Extended
					// and ExtendedAutoDrag we want to go into extended selection mode and thus
					// not release the capture.
					this.OnDragStartCanceled();
				}
			}
		}

				#endregion //DragStart

				#region GetMousePointInRootElementCoordinates

		/// <summary>
		/// Returns a point in RootElement coordinates that corresponds to the mouse position contained in mouseEventArgs.
		/// </summary>
		/// <param name="mouseEventArgs"></param>
		/// <returns></returns>
		protected Point GetMousePointInRootElementCoordinates(System.Windows.Input.MouseEventArgs mouseEventArgs)
		{
			return mouseEventArgs.GetPosition(this.SelectionHost.RootElement as IInputElement);
		}

				#endregion GetMousePointInRootElementCoordinates

				#region ManageTimers

		/// <summary>
		/// Determines during a drag operation whether timers
		/// are required to generate horizontal or vertical scrolling.
		/// </summary>
		/// <param name="mouseEventArgs">The mouse event args</param>
		protected virtual void ManageTimers(System.Windows.Input.MouseEventArgs mouseEventArgs)
		{
			AutoScrollInfo	autoScrollInfo	= this.SelectionHost.GetAutoScrollInfo(this.CurrentSelectableItem);


		    // Manage Horizontal Dragging
			this._autoScrollManagerHorizontal = new AutoScrollManager(this, autoScrollInfo, mouseEventArgs, false);
			if (this._autoScrollManagerHorizontal.IsScrollingRequired == true)
				this.SetupHorizontalAutoScrollTimer(this._autoScrollManagerHorizontal.TimerInterval);
			else
				this.StopHorizontalAutoScrollTimer();



		    // Manage Vertical Dragging
			this._autoScrollManagerVertical = new AutoScrollManager(this, autoScrollInfo, mouseEventArgs, true);
			if (this._autoScrollManagerVertical.IsScrollingRequired == true)
				this.SetupVerticalAutoScrollTimer(this._autoScrollManagerVertical.TimerInterval);
			else
				this.StopVerticalAutoScrollTimer();

		}

				#endregion //ManageTimers

				#region OnDragStartCanceled

		/// <summary>
		/// This method is called when the selection manager cancels OnDragStart. Default implementation releases the capture.
		/// </summary>
		protected virtual void OnDragStartCanceled()
		{
			//	If the control has capture, but no mouse buttons are pressed,
			//	the captre was probably ripped away by (for example)
			if (this.SelectionHost.RootElement.IsMouseCaptured && this.IsAnyMouseButtonDown == false)
			{
				if (this.SelectionHost != null)
					this.SelectionHost.OnDragEnd(true);
			}


			if (this.SelectionHost.RootElement.IsMouseCaptured)
				this.SelectionHost.RootElement.ReleaseMouseCapture();
		}

				#endregion OnDragStartCanceled

				#region ShouldStartDrag

		/// <summary>
		/// Returns true if dragging should start (when we're in preDrag mode).
		/// The default implementation checks to see if the x or y coords exceed
		/// the drag threshold.
		/// </summary>
		/// <param name="x">x position</param>
		/// <param name="y">y position</param>
		/// <returns><b>True</b> if should start drag, false otherwise.</returns>
		protected virtual bool ShouldStartDrag(double x, double y)
		{
			if (Math.Abs(x - this.InitialXPos) > this.DragThreshold ||
				Math.Abs(y - this.InitialYPos) > this.DragThreshold)
				return true;
			else
				return false;
		}

				#endregion ShouldStartDrag

				#region StopHorizontalAutoScrollTimer

		/// <summary>
		/// Stops the timer used for horizontal auto scrolling.
		/// </summary>
		protected void StopHorizontalAutoScrollTimer()
		{
			// AS 4/12/11 TFS62951
			if (_autoScrollStopwatchHorizontal != null)
			{
				_autoScrollStopwatchHorizontal.Stop();
				_autoScrollStopwatchHorizontal = null;
			}

			if (this._horizontalAutoScrollTimer != null && this._horizontalAutoScrollTimer.IsEnabled)
				this._horizontalAutoScrollTimer.Stop();
		}

				#endregion //StopHorizontalAutoScrollTimer

				#region StopVerticalAutoScrollTimer

		/// <summary>
		/// Stops the timer used for vertical auto scrolling.
		/// </summary>
		protected void StopVerticalAutoScrollTimer()
		{
			// AS 4/12/11 TFS62951
			if (_autoScrollStopwatchVertical != null)
			{
				_autoScrollStopwatchVertical.Stop();
				_autoScrollStopwatchVertical = null;
			}

			if (this._verticalAutoScrollTimer != null && this._verticalAutoScrollTimer.IsEnabled)
				this._verticalAutoScrollTimer.Stop();
		}

				#endregion //StopVerticalAutoScrollTimer

			#endregion //Protected Methods

			#region Private Methods

				#region ForceSynchronousMouseMove

		private void ForceSynchronousMouseMove()
		{
			IInputElement captured = Mouse.Captured;

			// release and recapture the mouse
			if (captured != null)
			{
				// set a flag so we can ignore the release capture notification
				// which would normally cancel pending operations
				this._isReleasingMouseCapture = true;
				try
				{
					Mouse.Capture(null);
					Mouse.Capture(captured, CaptureMode.Element);
				}
				finally
				{
					// reset the flag
					this._isReleasingMouseCapture = false;
				}
			}
		}

				#endregion //ForceSynchronousMouseMove	
    
				// AS 6/4/09
				// To make it easier to see if the same strategy can be returned from 
				// GetSelectionStrategy, I added this method which does the type checks
				// from most derived to least.
				//
				#region GetSelectionType
		private static SelectionType? GetSelectionType(SelectionStrategyBase strategy)
		{
			if (strategy is SelectionStrategyContiguous)
				return SelectionType.Range;

			if (strategy is SelectionStrategySingleAutoDrag)
				return SelectionType.SingleAutoDrag;

			if (strategy is SelectionStrategySingle)
				return SelectionType.Single;

			if (strategy is SelectionStrategyExtendedAutoDrag)
				return SelectionType.ExtendedAutoDrag;

			if (strategy is SelectionStrategyExtended)
				return SelectionType.Extended;

			if (strategy is SelectionStrategyNone)
				return SelectionType.None;

			Debug.Fail("Unrecognized selection type:" + (strategy != null ? strategy.ToString() : string.Empty));
			return null;
		} 
				#endregion //GetSelectionType

				#region OnHorizontalAutoScrollTimerTick

		void OnHorizontalAutoScrollTimerTick(object sender, EventArgs e)
		{
			this.StopHorizontalAutoScrollTimer();

			if (Mouse.LeftButton == MouseButtonState.Released)
				return;

            // JJD 10/10/14
            // Changed to a weak reference so we don't root the item
			//this._selectionHost.DoAutoScrollHorizontal(this._currentSelectableItem, this._autoScrollManagerHorizontal.ScrollDirection, this._autoScrollManagerHorizontal.ScrollSpeed);
            ISelectableItem item = this.CurrentSelectableItem;

            if (item != null)
            {
                this._selectionHost.DoAutoScrollHorizontal(item, this._autoScrollManagerHorizontal.ScrollDirection, this._autoScrollManagerHorizontal.ScrollSpeed);

                this.ForceSynchronousMouseMove();
            }
		}

				#endregion //OnHorizontalAutoScrollTimerTick	
    
				#region OnVerticalAutoScrollTimerTick

		private void OnVerticalAutoScrollTimerTick(object sender, EventArgs e)
		{
			this.StopVerticalAutoScrollTimer();

			if (Mouse.LeftButton == MouseButtonState.Released)
				return;

            // JJD 10/10/14
            // Changed to a weak reference so we don't root the item
            //this._selectionHost.DoAutoScrollHorizontal(this._currentSelectableItem, this._autoScrollManagerHorizontal.ScrollDirection, this._autoScrollManagerHorizontal.ScrollSpeed);
            ISelectableItem item = this.CurrentSelectableItem;

            if (item != null)
            {
                this._selectionHost.DoAutoScrollVertical(item, this._autoScrollManagerVertical.ScrollDirection, this._autoScrollManagerVertical.ScrollSpeed);

                this.ForceSynchronousMouseMove();
            }
		}

				#endregion //OnVerticalAutoScrollTimerTick	
    
				#region SetupHorizontalAutoScrollTimer

		private void SetupHorizontalAutoScrollTimer(double interval)
		{
			if (this._horizontalAutoScrollTimer == null)
			{
				this._horizontalAutoScrollTimer			 = new DispatcherTimer(this.AutoScrollTimerDispatcherPriority);
				this._horizontalAutoScrollTimer.Tick	+= new EventHandler(OnHorizontalAutoScrollTimerTick);
			}

			// AS 4/12/11 TFS62951
			//if (interval != Convert.ToDouble(this._horizontalAutoScrollTimer.Interval.Milliseconds) ||
			TimeSpan timeSpanInterval = TimeSpan.FromMilliseconds(interval);

			if (timeSpanInterval != this._horizontalAutoScrollTimer.Interval ||
				this._horizontalAutoScrollTimer.IsEnabled == false)
			{
				// AS 4/12/11 TFS62951
				Stopwatch sw = _autoScrollStopwatchHorizontal ?? Stopwatch.StartNew();
				if (_autoScrollStopwatchHorizontal != null)
				{
					timeSpanInterval = TimeSpan.FromTicks(Math.Max(timeSpanInterval.Ticks - _autoScrollStopwatchHorizontal.ElapsedTicks, 0));
					_autoScrollStopwatchHorizontal = null;
				}

				//this.StopHorizontalAutoScrollTimer();

				// AS 4/12/11 TFS62951
				//this._horizontalAutoScrollTimer.Interval = TimeSpan.FromMilliseconds(interval);
				this._horizontalAutoScrollTimer.Interval = timeSpanInterval;
				this._autoScrollStopwatchHorizontal = sw;

				this._horizontalAutoScrollTimer.Start();
			}
		}

				#endregion //SetupHorizontalAutoScrollTimer

				#region SetupVerticalAutoScrollTimer

		private void SetupVerticalAutoScrollTimer(double interval)
		{
			if (this._verticalAutoScrollTimer == null)
			{
				this._verticalAutoScrollTimer = new DispatcherTimer(this.AutoScrollTimerDispatcherPriority);
				this._verticalAutoScrollTimer.Tick += new EventHandler(OnVerticalAutoScrollTimerTick);
			}


			// AS 4/12/11 TFS62951
			//if (interval != Convert.ToDouble(this._verticalAutoScrollTimer.Interval.Milliseconds) ||
			TimeSpan timeSpanInterval = TimeSpan.FromMilliseconds(interval);

			if (timeSpanInterval != this._verticalAutoScrollTimer.Interval ||
				this._verticalAutoScrollTimer.IsEnabled == false)
			{
				// AS 4/12/11 TFS62951
				Stopwatch sw = _autoScrollStopwatchVertical ?? Stopwatch.StartNew();
				if (_autoScrollStopwatchVertical != null)
				{
					timeSpanInterval = TimeSpan.FromTicks(Math.Max(timeSpanInterval.Ticks - _autoScrollStopwatchVertical.ElapsedTicks, 0));
					_autoScrollStopwatchVertical = null;
				}

				//this.StopVerticalAutoScrollTimer();

				// AS 4/12/11 TFS62951
				//this._verticalAutoScrollTimer.Interval = TimeSpan.FromMilliseconds(interval);
				this._verticalAutoScrollTimer.Interval = timeSpanInterval;
				this._autoScrollStopwatchVertical = sw;

				this._verticalAutoScrollTimer.Start();
			}
		}

				#endregion //SetupVerticalAutoScrollTimer

			#endregion //Private Methods

		#endregion //Methods

		#region AutoScrollManager Nested Class

		/// <summary>
		/// Class that calcualtes and manages info about autoscrolling.
		/// </summary>
		protected class AutoScrollManager
		{
			#region Member Variables

			private SelectionStrategyBase					_selectionStrategy;
			private AutoScrollInfo							_autoScrollInfo;
			private System.Windows.Input.MouseEventArgs		_mouseEventArgs;
			private bool									_directionIsVertical;

			private bool									_isScrollingRequired;
			private ScrollDirection							_scrollDirection;
			private ScrollSpeed								_scrollSpeed;
			private double									_timerInterval;

			private Rect									_scrollAreaRect;
			private Rect									_scrollAreaInnerRect;
			private Point									_mousePosition;

			#endregion Member Variables

			#region Constructor

			/// <summary>
			/// Initializes a new instance of the <see cref="AutoScrollManager"/> class
			/// </summary>
			/// <param name="selectionStrategy">The associated selection strategy</param>
			/// <param name="autoScrollInfo">Provides information about what type of scrolling is possible</param>
			/// <param name="mouseEventArgs">The associated mouse event arguments from the mouse event for which the <b>AutoScrollManager</b> is being created</param>
			/// <param name="directionIsVertical">True if the manager will be dealing with vertical scrolling; otherwise false for horizontal scrolling.</param>
			public AutoScrollManager(SelectionStrategyBase selectionStrategy, AutoScrollInfo autoScrollInfo, System.Windows.Input.MouseEventArgs mouseEventArgs, bool directionIsVertical)
			{
				if (selectionStrategy == null)
					throw new ArgumentNullException("selectionStrategy", SR.GetString( "LE_ArgumentNullException_3" ) );
				if (mouseEventArgs == null)
					throw new ArgumentNullException( "mouseEventArgs", SR.GetString( "LE_ArgumentNullException_4" ) );

				this._selectionStrategy		= selectionStrategy;
				this._autoScrollInfo		= autoScrollInfo;
				this._mouseEventArgs		= mouseEventArgs;
				this._directionIsVertical	= directionIsVertical;

				this.Initialize();
			}

			#endregion Constructor

			#region Methods

				#region GetAutoScrollTimerInterval

			private double GetAutoScrollTimerInterval(int mouseThresholdAreaPenetration)
			{

				//	This is the function that MS Excel applies to their Mouse panning curve
				//	
				//	Maximum exponent		= log n(Max Time / Min Time)
				//	Exponent for pixel P	= (threshholdAreaPenetration/thresholdAreaSize)*Maximum Exponent
				//	Current Time			= Max Time / n^Exponent 

				// AS 4/12/11 TFS62951
				// Moved to helper properties to avoid duplicating this code.
				//
				//int maxTime = this._directionIsVertical ? this._selectionStrategy.AutoScrollVerticalIntervalMax :
				//                                            this._selectionStrategy.AutoScrollHorizontalIntervalMax;
				//int minTime = this._directionIsVertical ?	this._selectionStrategy.AutoScrollVerticalIntervalMin :
				//                                            this._selectionStrategy.AutoScrollHorizontalIntervalMin;
				int maxTime = this.MaxTimerInterval;
				int minTime = this.MinTimerInterval;

				double exp = System.Math.Log((double)(maxTime / minTime));
				double pix = (double)mouseThresholdAreaPenetration 
					/ (this._directionIsVertical ?	((double)SystemParameters.PrimaryScreenHeight * .4d) :
													((double)SystemParameters.PrimaryScreenWidth * .4d));

				//Debug.WriteLine("penetration: " + mouseThresholdAreaPenetration.ToString());

				//Debug.WriteLine("exp: " + exp.ToString() + 
				//                 " pix: " + pix.ToString() +
				//                 " mintime: " + minTime.ToString() +
				//                 " maxtime: " + maxTime.ToString());
				pix = System.Math.Abs(pix * exp);

				//Debug.WriteLine("Pow: " + System.Math.Pow(System.Math.E, pix).ToString());

				// Note that we use Euler's number here for the natural logarithm, however
				// any number theoretically yields the same result.
				double calculatedInterval = ((double)maxTime) / System.Math.Pow(System.Math.E, pix);

				//Debug.WriteLine("calculatedInterval: " + calculatedInterval.ToString() + 
				//                 " pix: " + pix.ToString());

				//Debug.WriteLine("");

				// Make sure the return value is within range
				calculatedInterval = Math.Min(calculatedInterval, maxTime);
				calculatedInterval = Math.Max(calculatedInterval, minTime);

				return calculatedInterval;
			}

				#endregion //GetAutoScrollTimerInterval

				#region GetMouseThresholdAreaPenetration

            private int GetMouseThresholdAreaPenetration()
            {
                int penetration = this.GetMouseThresholdAreaPenetration(this._directionIsVertical);

                // AS 8/28/08 NA 2008 Vol 2 ScrollOrientation
                // if we can scroll and we don't necessarily scroll in both directions...
                if (0 != penetration && this._autoScrollInfo.ScrollOrientation != ScrollOrientation.Both)
                {
                    int otherPenetration = this.GetMouseThresholdAreaPenetration(!this._directionIsVertical);

                    // if we can scroll in the other orientation as well
                    if (0 != otherPenetration)
                    {
                        switch (this._autoScrollInfo.ScrollOrientation)
                        {
                            case ScrollOrientation.Horizontal:
                                // if we should prefer horizontal then do not scroll vertically
                                if (this._directionIsVertical)
                                    penetration = 0;
                                    break;
                            case ScrollOrientation.Vertical:
                                // if we should prefer horizontal then do not scroll vertically
                                if (false == this._directionIsVertical)
                                    penetration = 0;
                                break;
                            case ScrollOrientation.BasedOnDistance:
                                int comparison = Math.Abs(penetration).CompareTo(Math.Abs(otherPenetration));

                                // if the mouse is further from the other orientation or they're equal
                                // and this represents horizontal scrolling, then do not scroll
                                if (comparison > 0 || (comparison == 0 && false == this._directionIsVertical))
                                    penetration = 0;
                                break;
                        }
                    }
                }

                return penetration;
            }

            // AS 8/28/08 NA 2008 Vol 2 ScrollOrientation
            // Added an overload so we could check the other orientation.
            //
            private int GetMouseThresholdAreaPenetration(bool vertical)
			{
				// Default to zero.
				int threshholdAreaPenetration = 0;


                // AS 8/28/08 NA 2008 Vol 2 ScrollOrientation
                //if (this._directionIsVertical)
                if (vertical)
				{
					if (this._mousePosition.Y < this._scrollAreaInnerRect.Top)
					{
						if (this._autoScrollInfo.CanScrollUp)
							threshholdAreaPenetration = (int)(this._mousePosition.Y - this._scrollAreaInnerRect.Top);
					}
					else
					{
						if (this._mousePosition.Y > this._scrollAreaInnerRect.Bottom)
						{
							if (this._autoScrollInfo.CanScrollDown)
								threshholdAreaPenetration = (int)(this._mousePosition.Y - this._scrollAreaInnerRect.Bottom);
						}
					}
				}
				else
				{
					if (this._mousePosition.X < this._scrollAreaInnerRect.Left)
					{
						if (this._autoScrollInfo.CanScrollLeft)
							threshholdAreaPenetration = (int)(this._mousePosition.X - this._scrollAreaInnerRect.Left);
					}
					else
					{
						if (this._mousePosition.X > this._scrollAreaInnerRect.Right)
						{
							if (this._autoScrollInfo.CanScrollRight)
								threshholdAreaPenetration = (int)(this._mousePosition.X - this._scrollAreaInnerRect.Right);
						}
					}
				}


				return threshholdAreaPenetration;
			}

				#endregion //GetMouseThresholdAreaPenetration

				#region GetScrollSpeedFromTimerInterval

			/// <summary>
			/// Returns a ScrollSpeed enum value based on the supplied timer interval and the min and max timer intervals.
			/// </summary>
			/// <param name="timerInterval"></param>
			/// <returns></returns>
			private ScrollSpeed GetScrollSpeedFromTimerInterval(double timerInterval)
			{
				// AS 4/12/11 TFS62951
				// Moved to helper properties to avoid duplicating this code.
				//
				//double minInterval, maxInterval;
				//
				//if (this._directionIsVertical)
				//{
				//    minInterval = this._selectionStrategy.AutoScrollVerticalIntervalMin;
				//    maxInterval = this._selectionStrategy.AutoScrollVerticalIntervalMax;
				//}
				//else
				//{
				//    minInterval = this._selectionStrategy.AutoScrollHorizontalIntervalMin;
				//    maxInterval = this._selectionStrategy.AutoScrollHorizontalIntervalMax;
				//}
				double minInterval = this.MinTimerInterval;
				double maxInterval = this.MaxTimerInterval;

				// Take care of some simple cases
				if (timerInterval <= minInterval)
				{
					// AS 1/18/11 TFS62951
					//return ScrollSpeed.Slowest;
					return ScrollSpeed.Fastest;
				}

				if (timerInterval >= maxInterval)
				{
					// AS 1/18/11 TFS62951
					//return ScrollSpeed.Fastest;
					return ScrollSpeed.Slowest;
				}

				double speedAsPercentage = (timerInterval - minInterval) / (maxInterval - minInterval);

				if (speedAsPercentage < .14)
					return ScrollSpeed.Fastest;
				if (speedAsPercentage < .28)
					return ScrollSpeed.Faster;
				if (speedAsPercentage < .42)
					return ScrollSpeed.Fast;
				if (speedAsPercentage < .56)
					return ScrollSpeed.Medium;
				if (speedAsPercentage < .70)
					return ScrollSpeed.Slow;
				if (speedAsPercentage < .84)
					return ScrollSpeed.Slower;

				return ScrollSpeed.Slowest;
			}

				#endregion GetScrollSpeedFromTimerInterval

				#region Initialize

			private void Initialize()
			{
				// First check to see if the control is capable of scrolling in the direction we need.  If not,
				// set IsScrollingRequired = false.
				if (this._directionIsVertical)
				{
					if (this._autoScrollInfo.CanScrollUp == false && this._autoScrollInfo.CanScrollDown == false)
					{
						this._isScrollingRequired = false;
						return;
					}

				}
				else if (this._autoScrollInfo.CanScrollLeft == false && this._autoScrollInfo.CanScrollRight == false)
				{
					this._isScrollingRequired = false;
					return;
				}


				// Since the control is capable of scrolling in the direction we need, check to see if the current mouse position
				// requires an AutoScroll.
				//
				// Get the scroll area rect.
				this._scrollAreaRect		= new Rect(this._autoScrollInfo.ScrollArea.TranslatePoint(new Point(0, 0), this._selectionStrategy.SelectionHost.RootElement),
													   new Size(this._autoScrollInfo.ScrollArea.ActualWidth, this._autoScrollInfo.ScrollArea.ActualHeight));

				// Get the inner rect that represents the scroll area's rect minus the auto scroll pixel threshhold.
				this._scrollAreaInnerRect	= this._scrollAreaRect;
				this._scrollAreaInnerRect.Inflate(-this._selectionStrategy.PixelThresholdForHorizontalAutoScroll,
												  -this._selectionStrategy.PixelThresholdForVerticalAutoScroll);

				// Get the mouse position in 'root element' coordinates.
				this._mousePosition			= this._selectionStrategy.GetMousePointInRootElementCoordinates(this._mouseEventArgs);

				// AS 4/12/11 TFS62951
				
#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)

				// assume we cannot scroll
				this._isScrollingRequired = false;

				// If the mouse is within the inner rect (i.e. within the ScrollArea but not within 'threshold # of pixels' of the
				// edge), then no scrolling is required.
				if (!this._scrollAreaInnerRect.Contains(this._mousePosition))
				{
					// Compute the autoscroll timer interval based on the mouse's position with respect to the
					// threshhold area.
					this._timerInterval = -1;
					int mouseThresholdAreaPenetration = this.GetMouseThresholdAreaPenetration();
					if (mouseThresholdAreaPenetration != 0)
						this._timerInterval = this.GetAutoScrollTimerInterval(Math.Abs(mouseThresholdAreaPenetration));


					// If we have a good timer interval, set IsScrollingRequired to true and set other important variables.
					if (this._timerInterval != -1)
					{
						this._isScrollingRequired = true;
						this._scrollDirection = mouseThresholdAreaPenetration < 0 
							? ScrollDirection.Decrement 
							: ScrollDirection.Increment;
						// AS 4/12/11 TFS62951
						// Moved this below the callback.
						//
						//this._scrollSpeed = this.GetScrollSpeedFromTimerInterval(this._timerInterval);
					}
				}

				// AS 4/12/11 TFS62951
				// Whether we think we should scroll or not, give the source a chance to adjust the 
				// the scroll direction and speed.
				//
				if (_autoScrollInfo.AdjustScrollInfoCallback != null)
				{
					ScrollDirection? direction = _isScrollingRequired ? _scrollDirection : (ScrollDirection?)null;
					double interval = _isScrollingRequired ? _timerInterval : this.MaxTimerInterval;

					_autoScrollInfo.AdjustScrollInfoCallback(!_directionIsVertical, _mouseEventArgs, _selectionStrategy.CurrentSelectableItem, ref direction, ref interval);

					_isScrollingRequired = direction != null;
					_scrollDirection = direction ?? ScrollDirection.Decrement;
					_timerInterval = Math.Max(Math.Min(interval, this.MaxTimerInterval), this.MinTimerInterval);
				}

				if (_isScrollingRequired)
					_scrollSpeed = this.GetScrollSpeedFromTimerInterval(_timerInterval);
			}

				#endregion Initialize

			#endregion Methods

			#region Properties

				#region IsScrollingRequired

			/// <summary>
			/// Returns true if scrolling is required.
			/// </summary>
			public bool IsScrollingRequired
			{
				get { return this._isScrollingRequired; }
			}

				#endregion IsScrollingRequired

				// AS 4/12/11 TFS62951
				#region MaxTimerInterval
			private int MaxTimerInterval
			{
				get
				{
					return _directionIsVertical
						? _selectionStrategy.AutoScrollVerticalIntervalMax
						: _selectionStrategy.AutoScrollHorizontalIntervalMax;
				}
			} 
				#endregion //MaxTimerInterval

				// AS 4/12/11 TFS62951
				#region MinTimerInterval
			private int MinTimerInterval
			{
				get
				{
					return _directionIsVertical
						? _selectionStrategy.AutoScrollVerticalIntervalMin
						: _selectionStrategy.AutoScrollHorizontalIntervalMin;
				}
			} 
				#endregion //MinTimerInterval

				#region ScrollDirection

			/// <summary>
			/// Returns the direction for the AutoScroll.
			/// </summary>
			public ScrollDirection ScrollDirection { get { return this._scrollDirection; } }

				#endregion //ScrollDirection

				#region ScrollSpeed

			/// <summary>
			/// Returns the speed of the AutoScroll
			/// </summary>
			public ScrollSpeed ScrollSpeed { get { return this._scrollSpeed; } }

				#endregion //ScrollSpeed

				#region TimerInterval

			/// <summary>
			/// Returns the interval to use for the AutoScroll timer (in milliseconds).
			/// </summary>
			public double TimerInterval { get { return this._timerInterval; } }

				#endregion //TimerInterval

			#endregion Properties
		}

		#endregion AutoScrollManager Nested Class
	}

	#endregion //Class SelectionStrategyBase

	#region Class SelectionStrategySingle

	/// <summary>
	/// Selection strategy for selecting one item at a time.
	/// </summary>
	public class SelectionStrategySingle : SelectionStrategyBase
	{
		#region Private Members

		// JJD 12/1/11 - TFS96941
		private bool _allowToggle = true;

		#endregion //Private Members	
    
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectionStrategySingle"/> class
		/// </summary>
		/// <param name="selectionHost">The selection host for which the selection strategy will interact</param>
		public SelectionStrategySingle(ISelectionHost selectionHost)
			: base(selectionHost)
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region Properties

				#region IsSingleSelect

		/// <summary>
		/// True if this strategy supports selecting only one 
		/// item at a time (read-only).
		/// </summary>
		public override bool IsSingleSelect { get { return true; } }

				#endregion //IsSingleSelect	

			#endregion //Properties

			#region Methods

				#region Public Methods

					#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when a left mouse down is received. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		/// <returns>Returning true on a mouse down message will cause the mouse to be captured</returns>
		/// <remarks>
		/// Note: The <see cref="OnMouseMove"/> and <see cref="OnMouseLeftButtonUp"/> 
		/// methods are called only if this method returns true and thereby captured the mouse.
		/// </remarks>
		public override bool OnMouseLeftButtonDown(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
			// Call ProcessMouseLeftButtonDown below with false as forceDrag parameter because we
			// don't want force a drag - only SelectionStrategySingleAutoDrag forces a drag.
			return this.ProcessMouseLeftButtonDown(item, e, false);
		}

					#endregion //OnMouseLeftButtonDown

					#region OnMouseMove

		/// <summary>
		/// Called when mouse move message is received. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		/// <returns>Returning false on a mouse down message will cause the mouse to be released.</returns>
		public override bool OnMouseMove(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
			if (this.InDragMode)
			{
				this.DragMove(e);

				return true;
			}


			Point currentMousePoint = this.GetMousePointInRootElementCoordinates(e);
			if (this.CurrentSelectionState == SelectionState.PreDrag)
			{
				if (this.ShouldStartDrag(currentMousePoint.X, currentMousePoint.Y))
				{
					this.DragStart(e);

					return true;
				}
			}

			this.ManageTimers(e);

			// SelectionStrategySingleAutoDrag shares this implementation, since
			// they are similar strategies, but we don't always want to change selection
			// during a drag, so check the virtual 'ShouldSelectOnDrag' property
			// before continuing.
			if (this.ShouldSelectOnDrag)
			{
				// JM 03-13-09 TFS 15307 - Activate the item.
                // JJD 7/14/09 - TFS18784
                // Added preventScrollItemIntoView flag
                //if (item != null && this.SelectionHost.ActivateItem(item) == false)
                if (item != null && this.SelectionHost.ActivateItem(item, false) == false)
                    return false;

				this.SelectItemAtPoint(e);
			}

			return true;
		}

				#endregion //OnMouseMove

					#region OnMouseLeftButtonUp

		/// <summary>
		/// Called when a left mouse up is received while the mouse is captured. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		public override void OnMouseLeftButtonUp(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
			this.StopHorizontalAutoScrollTimer();
			this.StopVerticalAutoScrollTimer();


			if (this.InDragMode)
			{
				this.SelectionHost.OnDragEnd(false);
				this.InDragMode = false;
			}


			this.SelectionHost.RootElement.ReleaseMouseCapture();


			// Notify the selection host that the mouse has been released to provide 
			// the opportunity to do something (enter editMode, etc.)
			this.SelectionHost.OnMouseUp(e);
		}

					#endregion //OnMouseLeftButtonUp

					#region SelectItemViaKeyboard

		/// <summary>
		/// Called to select an item as a result of a key press
		/// </summary>
		/// <param name="item">The item to select</param>
		/// <param name="shiftKeyDown">True if the shift key is pressed</param>
		/// <param name="controlKeyDown">True if the control key is pressed.</param>
		/// <param name="forceToggle">Toggle the existing selection.</param>
		/// <returns>True is successful.</returns>
		public override bool SelectItemViaKeyboard(ISelectableItem item, bool shiftKeyDown, bool controlKeyDown, bool forceToggle)
		{
			// 'Force toggle' in SelectionStrategySingle is ignored.
			if (forceToggle == true)
			{
				// JJD 12/1/11 - TFS96941
				// Only return if AllowTggle is false or the Ctrl key is not pressed since
				// we should support toggling if it is
				if (_allowToggle == false || controlKeyDown == false)
					return false;
			}


			// Save a reference to the item and activate it.
			this.CurrentSelectableItem = item;

            // JJD 7/14/09 - TFS18784
            // Added preventScrollItemIntoView flag
            //if (this.SelectionHost.ActivateItem(item) == false)
            if (this.SelectionHost.ActivateItem(item, false) == false)
				return false;


			// Update the selection state.
			this.CurrentSelectionState = SelectionState.SingleSelect;


			// If the item is not yet selected, select it.
			if (this.CurrentSelectableItem.IsSelected == false)
				this.SelectionHost.SelectItem(item, true);
			else
			{
				// JJD 12/1/11 - TFS96941
				// if forceToggle is true then deselect the item
				if (forceToggle && _allowToggle)
					this.SelectionHost.DeselectItem(this.CurrentSelectableItem);
			}


			return true;
		}

				#endregion //SelectItemViaKeyboard	

				#endregion //Public Methods

			#endregion //Methods

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				// JJD 12/1/11 - TFS96941 - added
				#region AllowToggle

		/// <summary>
		/// Gets/sets whether toggling of the selected state of an item is allowed
		/// </summary>
		/// <remarks>
		/// <para class="body">If AllowToggle is to true then holding down the 'Ctrl' key while clicking on a selectable item will toggle its selection state. The same behavior applies when holding the 'Ctrl' key down and using the keyboard to select/deselect the item.</para>
		/// <para class="body">For example, holding the 'Ctrl' key down and pressing the space bar while focus is within a <b>XamDataGrid</b> (with its <b>FieldLayoutSettings.SelectionTypeRecord</b> set to 'Single') will toggle the selection state of its ActiveRecord assuming there is no ActiveCell.</para>
		/// </remarks>
		/// <value>True if toggling is allowed, otherwise false. The default is true.</value>
		public bool AllowToggle
		{
			get { return _allowToggle; }
			set { _allowToggle = value; }
		}

				#endregion //AllowToggle

			#endregion //Public Properties	
    
		#endregion //Properties	
        
		#region Methods

			#region Protected Methods

				#region ProcessMouseLeftButtonDown

		/// <summary>
		/// OnMouseDown handler that takes a third parameter that specifies whether
		/// we are forcing a drag or not.  SelectionStrategySingleAutoDrag passes
		/// true to this.
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null) <see cref="ISelectableItem"/></param>
		/// <param name="e">The mouse event args</param>
		/// <param name="forceDrag"><b>true</b> if a drag should be forced</param>
		/// <returns></returns>
		protected virtual bool ProcessMouseLeftButtonDown(ISelectableItem item, System.Windows.Input.MouseEventArgs e, bool forceDrag)
		{
			// Save the initial cursor position.
			Point p				= this.GetMousePointInRootElementCoordinates(e);
			this.InitialXPos	= p.X;
			this.InitialYPos	= p.Y;


			// Save the current selectable item.
			this.CurrentSelectableItem = item;


			// Activate the SelectableItem
            // JJD 7/14/09 - TFS18784
            // Added preventScrollItemIntoView flag
            //bool returnCode = this.SelectionHost.ActivateItem(item);
            bool returnCode = this.SelectionHost.ActivateItem(item, true);

			if (returnCode == false)
				return false;

			if (this.CurrentSelectableItem.IsSelected)
			{
				// JJD 12/1/11 - TFS96941
				// if AllowToggle is true and the Ctrl key is pressed then deselect the item
				if (_allowToggle == true && this.IsCtrlKeyPressed == true)
				{
					this.SelectionHost.DeselectItem(this.CurrentSelectableItem);

					// return false so that the mouse doesn't get captured
					return false;
				}

				if (this.CurrentSelectableItem.IsDraggable)
					this.CurrentSelectionState = SelectionState.PreDrag;
				else
					this.CurrentSelectionState = SelectionState.PreEdit;


				// Since the selectable item is already set we can exit.
				return true;
			}
			else
				this.CurrentSelectionState = SelectionState.SingleSelect;


			// Tell the SelectionHost to select the item.
			this.SelectionHost.SelectItem(this.CurrentSelectableItem, true);


			// If forceDrag param is true (SingleAutoDrag), force the PreDrag state.
			if (forceDrag && this.CurrentSelectableItem.IsDraggable)
				this.CurrentSelectionState = SelectionState.PreDrag;


			return true;
		}

				#endregion //ProcessMouseLeftButtonDown

				#region SelectItemAtPoint

		/// <summary>
		/// Selects the item at or nearest the mouse location
		/// </summary>
		/// <param name="mouseEventArgs">The MouseEventArgs from the most recent mouse event</param>
		protected virtual void SelectItemAtPoint(System.Windows.Input.MouseEventArgs mouseEventArgs)
		{
			// Check if the left button is down. If it isn't kill the timer and exit
			if (this.IgnoreNextMouseMove || this.IsLeftMouseButtonDown == false)
			{
				this.IgnoreNextMouseMove = false;
				this.StopHorizontalAutoScrollTimer();
				this.StopVerticalAutoScrollTimer();

				return;
			}

			ISelectableItem selectedItem = this.SelectionHost.GetNearestCompatibleItem(this.CurrentSelectableItem, mouseEventArgs);

			// no new selectedItem or selectedItem is of different type, bail
			if (selectedItem == null ||
                // AS 8/28/08
                // The item could have been recycled in which case it may not be selected
                // anymore. This happened with the XamMonthCalendar. You could mouse down
                // on an item to select it and then drag the mouse outside the control
                // causing it to scroll. At that point the item was recycled and represents
                // a different date in which case its no longer selected so make sure
                // the current selectable item is selected.
                //
				//selectedItem == this.CurrentSelectableItem ||
				(selectedItem == this.CurrentSelectableItem && selectedItem.IsSelected) ||
                // AS 8/28/08
                // This is unlikely to happen but it could - basically the CurrentSelectableItem
                // could be null in which case the GetType check would fail.
                //
				//selectedItem.GetType() != this.CurrentSelectableItem.GetType())
                (this.CurrentSelectableItem != null && selectedItem.GetType() != this.CurrentSelectableItem.GetType()))
			{
				return;
			}


			// If the action wasn't canceled, the item becomes the current item.
			bool success = this.SelectionHost.SelectItem(selectedItem, true);
			if (success == true)
				this.CurrentSelectableItem = selectedItem;
		}

				#endregion //SelectItemAtPoint

				#region ShouldSelectOnDrag

		/// <summary>
		/// Returns whether selection should be modified when the cursor passes over
		/// a different item than the selected one. The default implementation returns true.
		/// </summary>
		protected virtual bool ShouldSelectOnDrag { get { return true; } }

				#endregion //ShouldSelectOnDrag

			#endregion //Protected methods

		#endregion //Methods
	}

	#endregion //Class SelectionStrategySingle

	#region Class SelectionStrategyNone

	/// <summary>
	/// Selection strategy for preventing selection of items in the associated selection host.
	/// </summary>
	public class SelectionStrategyNone : SelectionStrategyBase
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectionStrategyNone"/> class
		/// </summary>
		/// <param name="selectionHost">The selection host for which the selection strategy will interact</param>
		public SelectionStrategyNone(ISelectionHost selectionHost)
			: base(selectionHost)
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when a left mouse down is received. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		/// <returns>Returning true on a mouse down message will cause the mouse to be captured</returns>
		/// <remarks>
		/// Note: The <see cref="OnMouseMove"/> and <see cref="OnMouseLeftButtonUp"/> 
		/// methods are called only if this method returns true and thereby captured the mouse.
		/// </remarks>
		public override bool OnMouseLeftButtonDown(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
			// Save the initial cursor position.
			Point p				= this.GetMousePointInRootElementCoordinates(e);
			this.InitialXPos	= p.X;
			this.InitialYPos	= p.Y;


			if (this.IsLeftMouseButtonDown)
			{
				this.IgnoreNextMouseMove = false;
			}


            // JJD 7/14/09 - TFS18784
            // Added preventScrollItemIntoView flag
            //if (this.SelectionHost.ActivateItem(item) == false)
            if (this.SelectionHost.ActivateItem(item, false) == false)
				return false;

			this.CurrentSelectableItem = item;

			return true;		
		}

			#endregion //OnMouseLeftButtonDown

			#region OnMouseMove

		/// <summary>
		/// Called when mouse move message is received. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		/// <returns>Returning false on a mouse down message will cause the mouse to be released.</returns>
		public override bool OnMouseMove(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
			if (this.InDragMode)
			{
				this.DragMove(e);
				return true;
			}


			Point mousePosition = this.GetMousePointInRootElementCoordinates(e);
			if (this.ShouldStartDrag(mousePosition.X, mousePosition.Y))
				this.DragStart(e);

			return true;
		}

			#endregion //OnMouseMove

			#region OnMouseLeftButtonUp

		/// <summary>
		/// Called when a left mouse up is received while the mouse is captured. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		public override void OnMouseLeftButtonUp(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
			this.StopHorizontalAutoScrollTimer();
			this.StopVerticalAutoScrollTimer();

			this.SelectionHost.OnDragEnd(false);
			this.InDragMode = false;
			this.SelectionHost.RootElement.ReleaseMouseCapture();


			// Notify the selection manager that the mouse has been released to provide
			// the opportunity to do something (enter editMode, etc.)
			this.SelectionHost.OnMouseUp(e);
		}

			#endregion //OnMouseLeftButtonUp

			#region SelectItemViaKeyboard

		/// <summary>
		/// Called to select an item as a result of a key press
		/// </summary>
		/// <param name="item">The item to select</param>
		/// <param name="shiftKeyDown">True if the shift key is pressed</param>
		/// <param name="controlKeyDown">True if the control key is pressed.</param>
		/// <param name="forceToggle">Toggle the existing selection.</param>
		/// <returns>True is successful.</returns>
		public override bool SelectItemViaKeyboard(ISelectableItem item, bool shiftKeyDown, bool controlKeyDown, bool forceToggle)
		{
			// 'Force toggle' in SelectionStrategyNone is ignored.
			if (forceToggle == true)
				return false;


			// Try to activate the item.
            // JJD 7/14/09 - TFS18784
            // Added preventScrollItemIntoView flag
            //if (this.SelectionHost.ActivateItem(item) == false)
            if (this.SelectionHost.ActivateItem(item, false) == false)
				return false;


			// Save a reference to the item.
			this.CurrentSelectableItem = item;


			return true;
		}

			#endregion //SelectItemViaKeyboard	

		#endregion //Base Class Overrides
	}

	#endregion //Class SelectionStrategyNone

	#region Class SelectionStrategyExtended

	/// <summary>
	/// Selection strategy used for selecting multiple items. The selection does not have to be contiguous.
	/// </summary>
	public class SelectionStrategyExtended : SelectionStrategyBase
	{
		#region Member Variables

		private bool									_isInManualDragMode;
		private bool									_shouldSelect = true;
		private bool									_selectionDragCanceled;
		private DispatcherTimer							_dragDelayTimer;

		private System.Windows.Input.MouseEventArgs		_initialMouseDownEventArgs;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectionStrategyExtended"/> class
		/// </summary>
		/// <param name="selectionHost">The selection host for which the selection strategy will interact</param>
		public SelectionStrategyExtended(ISelectionHost selectionHost)
			: base(selectionHost)
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region Properties

				#region Public Properties

					#region IsMultiSelect

		/// <summary>
		/// Returns true if only a single item can be selected 
		/// at any given time 
		/// </summary>
		public override bool IsMultiSelect
		{
			get
			{
				return true;
			}
		}

				#endregion //IsMultiSelect

					#region IsDiscontiguousAllowed

		/// <summary>
		/// Returns true if discontinuous selection is allowed 
		/// </summary>
		public virtual bool IsDiscontiguousAllowed
		{
			get
			{
				return true;
			}
		}

				#endregion //IsDiscontiguousAllowed

				#endregion //PublicProperties

			#endregion //Properties

			#region Methods

				#region CanItemBeNavigatedTo

		/// <summary>
		/// Returns true if item can be selected.
		/// </summary>
		/// <param name="item"><see cref="ISelectableItem"/>Selected item</param>
		/// <param name="shiftKeyDown"><b>true</b> if shift key is down</param>
		/// <param name="controlKeyDown"><b>true</b> if shift key is down</param>
		/// <returns><b>true</b> if item can be selected</returns>
		public override bool CanItemBeNavigatedTo(ISelectableItem item, bool shiftKeyDown, bool controlKeyDown)
		{
			if (false == base.CanItemBeNavigatedTo(item, shiftKeyDown, controlKeyDown))
				return false;


			// If the shift or ctl keys are down we need to see if the item is
			// compatiable with the existing selection since we don't allow
			// cross band selection or mixed type selections
			if (shiftKeyDown || controlKeyDown)
				return this.SelectionHost.IsItemSelectableWithCurrentSelection(item);

			return true;
		}

				#endregion //CanItemBeNavigatedTo

				#region OnDragStartCanceled

		/// <summary>
		/// This method is called when the selection manager cancels OnDragStart. The default implementation
		/// releases the capture.
		/// </summary>
		protected override void OnDragStartCanceled()
		{
			// Do nothing for extended and extended auto drag. If the user cancels SelectionDrag event,
			// then we should go into extended selection mode.
		}

				#endregion //OnDragStartCanceled

				#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when a left mouse down is received. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		/// <returns>Returning true on a mouse down message will cause the mouse to be captured</returns>
		/// <remarks>
		/// Note: The <see cref="OnMouseMove"/> and <see cref="OnMouseLeftButtonUp"/> 
		/// methods are called only if this method returns true and thereby captured the mouse.
		/// </remarks>
		public override bool OnMouseLeftButtonDown(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
			return this.ProcessMouseLeftButtonDown(item, e, false);
		}

				#endregion //OnMouseLeftButtonDown

				#region OnMouseMove

		/// <summary>
		/// Called when mouse move message is received. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		/// <returns>Returning false on a mouse down message will cause the mouse to be released.</returns>
		public override bool OnMouseMove(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
			// If we are in drag mode, pass along the mouse info to the DragMove method. 
			if (this.InDragMode)
			{
				this.DragMove(e);

				return true;
			}



			if (this.CurrentSelectionState == SelectionState.PotentialPreDrag)
			{
				// If we're manually dragging (ie timer went off and event not cancelled), 
				// there is nothing left to do

				if (this._isInManualDragMode)
				{
					this.SelectionHost.RootElement.ReleaseMouseCapture();

					this.CurrentSelectionState = SelectionState.ExtSelect;

					return true;
				}
			}


			// Determine if we need to start a drag operation.
			bool	timerKilled	= false;
			Point	mousePoint	= this.GetMousePointInRootElementCoordinates(e);
			if (this.CurrentSelectionState == SelectionState.PreDrag || this.CurrentSelectionState == SelectionState.PotentialPreDrag)
			{
				if (this.ShouldStartDrag(mousePoint.X, mousePoint.Y))
				{
					if (this.CurrentSelectionState == SelectionState.PreDrag)
					{
						this.DragStart(e);
						this.CurrentSelectionState = SelectionState.ExtSelect;

						return true;
					}
					else
					{
						timerKilled = true;
						this.StopDragDelayTimer();
					}
				}

			}


			// Deal with the timers.
			if (false == this.IsLeftMouseButtonDown)
			{
				this.StopHorizontalAutoScrollTimer();
				this.StopVerticalAutoScrollTimer();
			}
			else
				this.ManageTimers(e);


			// If the left button isn't down or the ignore flag is set then exit.
			if (this.IgnoreNextMouseMove || false == this.IsLeftMouseButtonDown)
			{
				this.IgnoreNextMouseMove = false;
				return true;
			}


			// Establish the current selectable item;
			item = this.SelectionHost.GetNearestCompatibleItem(this.CurrentSelectableItem, e);

			// JJD 1/24/07 
			// If we don't have an item simply return
			if (item == null )
				return true;

			item = this.SelectionHost.TranslateItem(item);

			// JJD 1/24/07 
			// If we don't have an item simply return
			if (item == null )
				return true;

			// If the 'OnSelectionDrag' event was cancelled, we want to select 
			// the item and continue with normal selection - so don't return 
			// in that case
			if (this.CurrentSelectionState == SelectionState.PotentialPreDrag &&
				(this._selectionDragCanceled || timerKilled))
			{
				this.CurrentSelectionState = SelectionState.ExtSelect;
			}
			else
			{
				// JJD 1/24/07 
				// item null check not needed since we are doing that above
				// If we don't have an item, or the item is same as last item, simply return
				//if (item == null || item == this.CurrentSelectableItem)
				if (item == this.CurrentSelectableItem)
					return true;
			}


			this.CurrentSelectableItem = item;


			// Activate the item.
            // JJD 7/14/09 - TFS18784
            // Added preventScrollItemIntoView flag
            //if (this.SelectionHost.ActivateItem(item) == false)
            if (this.SelectionHost.ActivateItem(item, false) == false)
				return false;


			// The control key specifies whether or not leave the existing selection
			// and simply toggle the specified item. Don't leave the existing selection
			// if discontiguous is not allowed.
			bool controlKeyDown = this.IsCtrlKeyPressed && this.IsDiscontiguousAllowed;
			if (controlKeyDown)
			{
				// Use the 'selected' flag set in the mouse down instead of
				// checking the state of the current pivot item (which could
				// have changed during the drag)
				if (this._shouldSelect)
					this.SelectionHost.SelectRange(item, false);
				else
					this.SelectionHost.DeselectRange(item);
			}
			else
				this.SelectionHost.SelectRange(item, true);


			// Set the flag so we ignore the next mouse move in case we ended up
			// scrolling the above item into view which might change the item
			// under the cursor.
			this.IgnoreNextMouseMove = true;


			return true;
		}

				#endregion //OnMouseMove

				#region OnMouseLeftButtonUp

		/// <summary>
		/// Called when a left mouse up is received while the mouse is captured. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		public override void OnMouseLeftButtonUp(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
            // JJD 7/14/09 - TFS18784
            // wrap code in try finally
            try
            {
                // stop the timers
                this.StopDragDelayTimer();
                this.StopHorizontalAutoScrollTimer();
                this.StopVerticalAutoScrollTimer();

                if (this._isInManualDragMode)
                {
                    this.SelectionHost.OnDragEnd(false);
                    this._isInManualDragMode = false;
                }

                this.IgnoreNextMouseMove = false;

                if (this.CurrentSelectionState != SelectionState.PreDrag &&
                     this.CurrentSelectionState != SelectionState.PotentialPreDrag)
                {
                    this.CurrentSelectionState = SelectionState.ExtSelect;
                    return;
                }

                item = this.SelectionHost.TranslateItem(item);

                if (item == null ||
                     this.CurrentSelectableItem == null ||
                     item.GetType() != this.CurrentSelectableItem.GetType())
                    return;

                if (this.CurrentSelectionState == SelectionState.PreDrag &&
                     this.CurrentSelectionState == SelectionState.PotentialPreDrag)
                {
                    if (this.IsShiftKeyPressed)
                        return;
                }

                this.SelectionHost.SetPivotItem(item, false);

                // the control key specifies whether or not leave the existing selection
                // and simply toggle the specified item
                bool toggleItem = false;
                if (this.IsCtrlKeyPressed &&
                     this.IsDiscontiguousAllowed)
                    toggleItem = true;

                // see if we need to select or unselect and whether to clear
                // existing selection
                if (toggleItem)
                {
                    if (this.ShouldSelect)
                        this.SelectionHost.SelectItem(item, false);
                    else
                        this.SelectionHost.DeselectItem(item);
                }
                else
                    // select range and clear existing selection
                    this.SelectionHost.SelectItem(item, true);
            }
            finally
            {
                // JJD 7/14/09 - TFS18784
                // Notify the selection manager that the mouse has been released to provide
                // the opportunity to do something (enter editMode, etc.)
                this.SelectionHost.OnMouseUp(e);
            }

		}

				#endregion //OnMouseLeftButtonUp

				#region SelectItemViaKeyboard

		/// <summary>
		/// Called to select an item as a result of a key press
		/// </summary>
		/// <param name="item">The item to select</param>
		/// <param name="shiftKeyDown">True if the shift key is pressed</param>
		/// <param name="controlKeyDown">True if the control key is pressed.</param>
		/// <param name="forceToggle">Toggle the existing selection.</param>
		/// <returns>True is successful.</returns>
		public override bool SelectItemViaKeyboard(ISelectableItem item, bool shiftKeyDown, bool controlKeyDown, bool forceToggle)
		{
			// The shift key determines if we are doing range selection.
			bool isRangeSelect = shiftKeyDown;


			// The control key specifies whether or not leave the existing selection
			// and simply toggle the specified item
			//
			// Also set toggleItem true if the forceToggle param is true
			// This is to prevent the existing selection from being cleared
			// when the space key is pressed (see below). 
			//
			// Don't toggle if discontiguous is not allowed.
			bool toggleItem = (controlKeyDown || forceToggle) && this.IsDiscontiguousAllowed;


			// If we're shift-clicking or ctl-clicking on an item of a different type, simply return.
			if (isRangeSelect && false == this.SelectionHost.IsItemSelectableWithCurrentSelection(item))
				return false;


			// If we can't activate the item, return.
            // JJD 7/14/09 - TFS18784
            // Added preventScrollItemIntoView flag
            //if (false == this.SelectionHost.ActivateItem(item))
			if (false == this.SelectionHost.ActivateItem(item, false))
				return false;


			//	Don't return if we are forcing a toggle.  When the space key is pressed and the CTRL key
			//	is not pressed, the control maintains the selected state of previously selected items. 
			if (false	== isRangeSelect	&&
				true	== toggleItem		&& 
				false	== forceToggle)
				return true;


			// Save a reference to the item.
			this.CurrentSelectableItem = item;


			// Default our select/unselect flag to 'select'.
			this._shouldSelect = true;


			// Set the selection state to ExtSelect.
			this.CurrentSelectionState = SelectionState.ExtSelect;


			// Set the pivot item.
			this.SelectionHost.SetPivotItem(item, isRangeSelect);


			// Evaluate toggling taking into account whether we are forcing a drag and/or range selecting.
			if (false	== toggleItem		&&
				true	== item.IsSelected	&&
				true	== item.IsDraggable)
			{
				// Don't return, unless we are toggling the selected state;
				// otherwise, we won't clear the existing selection
				if (forceToggle)
					return false;
			}


			// Determine whether we should select or unselect.
			if (toggleItem)
			{
				this.CurrentSelectionState = SelectionState.ControlClick;

				// If we're Ctl-Shift clicking, don't toggle
				if (false == isRangeSelect)
					this._shouldSelect = !item.IsSelected;
				else
					// Instead use pivot item's selected state
					this._shouldSelect = this.SelectionHost.GetPivotItem(item).IsSelected;
			}


			// If we're selecting a single item, make sure we don't exceed the max selected items.
			if (this._shouldSelect && toggleItem)
			{
				if (this.SelectionHost.IsMaxSelectedItemsReached(item))
					return false;
			}


			// Do the actual selection.  First clear the initialSelection.
			if (false == toggleItem)
				this.SelectionHost.ClearSnapshot();


			// Update the items in the initial snapshot if we are toggling.
			if (toggleItem && false == isRangeSelect)
				this.SelectionHost.SnapshotSelection(item);


			if (isRangeSelect)
			{
				// See if we need to select or unselect and whether to clear existing selection
				if (toggleItem)
				{
					// Both control and shift are down, so snaking is a go.
					this.SelectionHost.EnterSnakingMode(item);

					if (this._shouldSelect)
						this.SelectionHost.SelectRange(item, false);
					else
						this.SelectionHost.DeselectRange(item);
				}
				else
					// Select a range and clear the existing selection.
					this.SelectionHost.SelectRange(item, true);
			}
			else
			{
				// See if we need to select or unselect and whether we should clear
				// existing selection.
				if (toggleItem)
				{
					if (this._shouldSelect)
						this.SelectionHost.SelectItem(item, false);
					else
						this.SelectionHost.DeselectItem(item);
				}
				else
					// Select a range and clear the existing selection.
					this.SelectionHost.SelectItem(item, true);
			}


			// If we're selecting a single item, make sure we don't exceed max selected items.
			if (this._shouldSelect && this.SelectionHost.IsMaxSelectedItemsReached(item))
				return false;


			return true;
		}

				#endregion //SelectItemViaKeyboard	

			#endregion //Methods
		
		#endregion Base Class Overrides

		#region Properties

			#region Protected Properties

				#region DragDelayTimerDispatcherPriority

		/// <summary>
		/// Returns the dispatcher priority for the drag delay timer
		/// </summary>
		protected virtual DispatcherPriority DragDelayTimerDispatcherPriority
		{
			get { return DispatcherPriority.Normal; }
		}

				#endregion //DragDelayTimerDispatcherPriority

				#region DragDelayTimerInterval

		/// <summary>
		/// Returns the interval for the drag delay timer in milliseconds.
		/// </summary>
		protected virtual double DragDelayTimerInterval
		{
			get { return 750; }
		}

				#endregion //DragDelayTimerInterval

				#region ShouldSelect

		/// <summary>
		/// Returns true if the strategy should select, or false if it should unselect.
		/// </summary>
		protected bool ShouldSelect
		{
			get { return this._shouldSelect; }
		}

				#endregion //ShouldSelect

			#endregion //Protected Properties

		#endregion //Properties

		#region Methods

			#region Protected Methods

				#region ProcessMouseLeftButtonDown

		/// <summary>
		/// OnMouseDown handler that takes a third parameter that specifies whether
		/// we are forcing a drag or not.  SelectionStrategySingleAutoDrag passes
		/// true to this.
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null) <see cref="ISelectableItem"/></param>
		/// <param name="e">The mouse event args</param>
		/// <param name="forceDrag"><b>true</b> if a drag should be forced</param>
		/// <returns></returns>
		protected virtual bool ProcessMouseLeftButtonDown(ISelectableItem item, System.Windows.Input.MouseEventArgs e, bool forceDrag)
		{
			// Reset flag
			this._isInManualDragMode = false;


			// The shift key specifies range selection
			bool isRangeSelect = this.IsShiftKeyPressed;


			// The control key specifies whether or not leave the existing selection
			// and simply toggle the specified item.  Only toggle if discontiguous 
			// is allowed
			bool toggleItem = false;
			if (this.IsCtrlKeyPressed == true && this.IsDiscontiguousAllowed)
				toggleItem = true;


			// If we're shift-clicking or ctl-clicking on an item of a different type,
			// simply return.
			if ((isRangeSelect || toggleItem) && this.SelectionHost.IsItemSelectableWithCurrentSelection(item) == false)
				return false;


			// Save the initial cursor position.
			Point p				= this.GetMousePointInRootElementCoordinates(e);
			this.InitialXPos	= p.X;
			this.InitialYPos	= p.Y;


			// Save the mouse event args - we need these to call OnDragStart when the drag timer fires.
			this._initialMouseDownEventArgs = e;


			// Save a reference to the item and initialize our select/unselect flag to 'select'.
			this.CurrentSelectableItem	= item;
			this._shouldSelect			= true;


			// Activate the item and return if unsuccessful.
            // JJD 7/14/09 - TFS18784
            // Added preventScrollItemIntoView flag
            //if (this.SelectionHost.ActivateItem(item) == false)
            if (this.SelectionHost.ActivateItem(item, true) == false)
				return false;


			// Determine the current selection state.
			if (item.IsSelected || item.IsSelectable == false || forceDrag)
			{
				if (item.IsDraggable)
				{
					this.CurrentSelectionState = SelectionState.PreDrag;
				}
				else
				// If we click on a selected item and drag isn't auto, we start a timer
				// that if fired, puts us in manual drag mode.
				if (toggleItem == false)
				{
					this.CurrentSelectionState = SelectionState.PotentialPreDrag;

					// Start a timer and fire OnSelectionDrag when the timer is fired
					this._selectionDragCanceled = false;
					this.StartDragDelayTimer();
				}
				else
					this.CurrentSelectionState = SelectionState.ExtSelect;



				if (isRangeSelect && (this.CurrentSelectionState == SelectionState.PreDrag ||
									  this.CurrentSelectionState == SelectionState.PotentialPreDrag))
				{
					// First clear the initial snapshot.
					this.SelectionHost.ClearSnapshot();


					// See if we need to select or unselect and whether to clear
					// existing selection
					if (toggleItem)
					{
						// Update items in the initial snapshot if toggling.
						this.SelectionHost.SnapshotSelection(item);


						// Use the pivot item's selected state to determine whether we are selecting or unselecting
						this._shouldSelect = this.SelectionHost.GetPivotItem(item).IsSelected;

						if (this._shouldSelect)
							this.SelectionHost.SelectRange(item, false);
						else 
							this.SelectionHost.DeselectRange(item);
					}
					else
						// Select range and clear the existing selection.
						this.SelectionHost.SelectRange(item, true);

					return false;
				}
			}
			else
				this.CurrentSelectionState = SelectionState.ExtSelect;


			// Set the pivot item.
			this.SelectionHost.SetPivotItem(item, isRangeSelect);


			// Handle the dragging case.
			if (item.IsSelected && item.IsDraggable)
			{
				// Set the 'should select' flag to false, in case we don't drag and simply 
				// want to unselect (since we don't reach code below)
				if (toggleItem)
					this._shouldSelect = false;

				// SSP 5/20/09 TFS17816
				// Returning false here causes the selection control to null out active strategy
				// and it won't handle move and up messages. It basically takes no further actions.
				// However we do want it to start drag mode in the mouse move and if that doesn't
				// happen then start drage selection mode. In the data presenter, Fields return
				// true from IsDraggable. However if multiple fields are selected, it won't go
				// into drag mode. In that case the selection strategy should fallback to drag
				// selecting fields. An example scenario is that you select multiple fields.
				// Then start dragging the mouse from one of the selected fields. It shouldn't
				// go into drag mode since multiple fields can't be dragged at the same time,
				// however it should go into drag selection mode where it starts drag selecting
				// from that field.
				// 
				//return false;
				return true;
			}


			// Evaluate toggling taking into account whether we are forcing a drag and/or range selecting.
			if (toggleItem)
			{
				// Don't set state to CtlClick if we're forcing drag (ie SelectionStrategyExtendedAutoDrag)
				if (!forceDrag)
					this.CurrentSelectionState = SelectionState.ControlClick;


				// If we're Ctl-Shift clicking, don't toggle.
				if (!isRangeSelect)
					this._shouldSelect = !item.IsSelected;
				else
					this._shouldSelect = this.SelectionHost.GetPivotItem(item).IsSelected;
			}
			else
			if (item.IsSelected)
				return true;



			// Do the actual selection.  First clear the initialSelection.
			this.SelectionHost.ClearSnapshot();


			// Update items in the initial selection if toggling.
			if (toggleItem)
				this.SelectionHost.SnapshotSelection(item);


			// Do the right thing depending on whether we are range selecting or not.
			if (isRangeSelect)
			{
				// See if we need to select or unselect and whether we need to clear the existing selection.
				if (toggleItem)
				{
					if (this._shouldSelect)
						this.SelectionHost.SelectRange(item, false);
					else 
						this.SelectionHost.DeselectRange(item);
				}
				else
					// Select a range and clear the existing selection.
					this.SelectionHost.SelectRange(item, true);
			}
			else
			{
				// See if we need to select or unselect and whether we need to clear the existing selection.
				if (toggleItem)
				{
					if (this._shouldSelect)
						this.SelectionHost.SelectItem(item, false);
					else
						this.SelectionHost.DeselectItem(item);
				}
				else
					// Select a range and clear the existing selection.
					this.SelectionHost.SelectItem(item, true);
			}


			// If we're selecting a single item, make sure we don't the exceed max selected items.
			if (this._shouldSelect  &&  this.SelectionHost.IsMaxSelectedItemsReached(item))
				return false;


			return true;
		}

				#endregion //ProcessMouseLeftButtonDown

				#region StartDragDelayTimer

		/// <summary>
		/// Starts the timer used to ensure a delay period before dragging starts.
		/// </summary>
		protected void StartDragDelayTimer()
		{
			this.StopDragDelayTimer();

			this._dragDelayTimer			= new DispatcherTimer(this.DragDelayTimerDispatcherPriority);
			this._dragDelayTimer.Interval	= TimeSpan.FromMilliseconds(this.DragDelayTimerInterval);
			this._dragDelayTimer.Tick		+= new EventHandler(OnDragDelayTimerTick);
			this._dragDelayTimer.Start();
		}

		void OnDragDelayTimerTick(object sender, EventArgs e)
		{
			if (false == this.SelectionHost.OnDragStart(this.CurrentSelectableItem, this._initialMouseDownEventArgs))
				this._selectionDragCanceled = true;
			else
				this._isInManualDragMode = true;


			this.StopDragDelayTimer();
		}

				#endregion //StartDragDelayTimer

				#region StopDragDelayTimer

		/// <summary>
		/// Stops the timer used to ensure a delay period before dragging starts.
		/// </summary>
		protected void StopDragDelayTimer()
		{
			if (this._dragDelayTimer == null)
				return;

			this._dragDelayTimer.Stop();
			this._dragDelayTimer.Tick	-= new EventHandler(this.OnDragDelayTimerTick);
			this._dragDelayTimer		= null;
		}

				#endregion //StopDragDelayTimer

			#endregion Protected Methods

		#endregion //Methods
	}

	#endregion Class SelectionStrategyExtended

    // AS 7/16/08 NA 2008 Vol 2
    #region Class SelectionStrategyContiguous

    /// <summary>
	/// Selection strategy used for selecting multiple items where the selection should not be contiguous.
    /// </summary>
    public class SelectionStrategyContiguous : SelectionStrategyExtended
    {
        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="SelectionStrategyContiguous"/>
        /// </summary>
		/// <param name="selectionHost">The selection host for which the selection strategy will interact</param>
        public SelectionStrategyContiguous(ISelectionHost selectionHost) : base(selectionHost)
        {
        }

        #endregion //Constructor

        #region Base Class Overrides

            #region IsDiscontiguousAllowed

        /// <summary>
        /// Returns true if discontinuous selection is allowed 
        /// </summary>
        public override bool IsDiscontiguousAllowed
        {
            get
            {
                return false;
            }
        }

            #endregion //IsDiscontiguousAllowed

        #endregion //Base Class Overrides
    }

    #endregion //Class SelectionStrategyContiguous

	// AS 6/4/09
	// Exposed these selection strategies so we can use them within the xamDataPresenter.
	//
	#region Class SelectionStrategyExtendedAutoDrag

	/// <summary>
	/// Selection strategy for selecting multiple items that can initiate a drag on the mouse down.
	/// </summary>
	public class SelectionStrategyExtendedAutoDrag : SelectionStrategyExtended
	{
		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="SelectionStrategyExtendedAutoDrag"/>
		/// </summary>
		/// <param name="selectionHost">The selection host for which the selection strategy will interact</param>
		public SelectionStrategyExtendedAutoDrag(ISelectionHost selectionHost)
			: base(selectionHost)
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when a left mouse down is received. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		/// <returns>Returning true on a mouse down message will cause the mouse to be captured</returns>
		public override bool OnMouseLeftButtonDown(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
			return this.ProcessMouseLeftButtonDown(item, e, true);
		}

		#endregion //OnMouseLeftButtonDown

		#endregion //Base Class Overrides
	}

	#endregion //Class SelectionStrategyExtendedAutoDrag

	#region Class SelectionStrategySingleAutoDrag

	/// <summary>
	/// Selection strategy for selecting a single item that can initiate a drag on the mouse down.
	/// </summary>
	public class SelectionStrategySingleAutoDrag : SelectionStrategySingle
	{
		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="SelectionStrategySingleAutoDrag"/>
		/// </summary>
		/// <param name="selectionHost">The selection host for which the selection strategy will interact</param>
		public SelectionStrategySingleAutoDrag(ISelectionHost selectionHost)
			: base(selectionHost)
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when a left mouse down is received. 
		/// </summary>
		/// <param name="item">The selectable item that the mouse is over (may be null)</param>
		/// <param name="e">The mouse event args</param>
		/// <returns>Returning true on a mouse down message will cause the mouse to be captured</returns>
		public override bool OnMouseLeftButtonDown(ISelectableItem item, System.Windows.Input.MouseEventArgs e)
		{
			// Simply call SelectionStrategySingle::ProcessMouseDown with true as the forceDrag 
			// parameter because we want to force a drag.
			return base.ProcessMouseLeftButtonDown(item, e, true);
		}

		#endregion //OnMouseLeftButtonDown

		#region ShouldSelectOnAutoDrag

		/// <summary>
		/// Returns whether selection should be modified when the cursor passes over
		/// a different item than the selected one.
		/// </summary>
		protected override bool ShouldSelectOnDrag { get { return false; } }

		#endregion //ShouldSelectOnAutoDrag

		#endregion //Base Class Overrides
	}

	#endregion //Class SelectionStrategySingleAutoDrag
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