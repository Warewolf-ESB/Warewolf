using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Infragistics.Collections;
using System.Windows.Controls.Primitives;

namespace Infragistics.Controls
{
	#region IProvideTouchInfo

	/// <summary>
	/// An interface that will be used by the <see cref="TouchHelper"/> class.
	/// </summary>
	internal interface IProvideTouchInfo
	{

		#region GetScrollModeFromPoint
		/// <summary>
		/// Method called to determine which scroll dimensions are valid for a specific point.
		/// </summary>
		TouchScrollMode GetScrollModeFromPoint(Point point, UIElement elementDirectlyOver);
		#endregion // GetScrollModeFromPoint

	}

	#endregion // IProvideTouchInfo

	#region TouchState

	/// <summary>
	/// Enum that describes the current touch state.
	/// </summary>
	internal enum TouchState
	{
		/// <summary>
		/// No touch operation is pending  
		/// </summary>
		NotDown,

		/// <summary>
		/// The screen has been touched but the move delta or time threshholds have not yet been reached.
		/// </summary>
		Pending,

		/// <summary>
		/// The move delta threshhold has been reached and the control is in a scroll operation.
		/// </summary>
		Scrolling,

		/// <summary>
		/// The touch and hold time threshhold has been reached and the control is in a hold state.
		/// </summary>
		Holding,
	}

	#endregion // TouchState

	#region TouchScrollMode

	/// <summary>
	/// Enum that detrmines the valid dimensions for a touch scroll operation.
	/// </summary>
	internal enum TouchScrollMode
	{
		/// <summary>
		/// Scrolling is not supported in either dimension  
		/// </summary>
		None,
		
		/// <summary>
		/// Scrolling is only supported horizontally
		/// </summary>
		Horizontal,
		
		/// <summary>
		/// Scrolling is only supported vertically
		/// </summary>
		Vertical,
		
		/// <summary>
		/// Scrolling is supported vertically and horizontally
		/// </summary>
		Both,
	}

	#endregion // TouchScrollMode

	#region HandleableEventArgs

	internal abstract class TouchHandleableEventArgs : EventArgs
    {
        public bool Handled
        {
            get;
            set;
        }
    }

    #endregion //HandleableEventArgs

    #region FlickEventArgs



#region Infragistics Source Cleanup (Region)



























































#endregion // Infragistics Source Cleanup (Region)

    #endregion // FlickEventArgs

    #region PanEventArgs

    /// <summary>
    /// An <see cref="EventArgs"/> object for the <see cref="TouchHelperBase.Pan"/> event.
    /// </summary>
    internal class PanEventArgs : TouchHandleableEventArgs
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PanEventArgs"/> class.
        /// </summary>        
        public PanEventArgs(Point p)
        {
            this.DeltaPoint = p;
        }
        #endregion // constructor

        #region DeltaPoint

        /// <summary>
        /// Gets the point from the screen.
        /// </summary>
        public Point DeltaPoint
        {
            get;
            protected set;
        }

        #endregion // DeltaPoint
    }

    #endregion // PanEventArgs

	#region StateChangedEventArgs

	internal class StateChangedEventArgs : EventArgs
    {
		public StateChangedEventArgs(TouchState newState, TouchState oldState)
        {
			this.NewState = newState;
            this.OldState = oldState;
        }

        public TouchState NewState
        {
            get;
            private set;
        }

        public TouchState OldState
        {
            get;
            private set;
        }

    }

	#endregion // StateChangedEventArgs

	#region TapEventArgs

	internal class TapEventArgs : TouchHandleableEventArgs
    {
        public TapEventArgs(Point p, UIElement element)
        {
            this.Point = p;
            this.Element = element;
        }

        public Point Point
        {
            get;
            protected set;
        }

        public UIElement Element
        {
            get;
            protected set;
        }

    }

    #endregion // TapEventArgs



#region Infragistics Source Cleanup (Region)
























































































































































































































































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)



	#region TouchHelper
	/// <summary>
    /// A class that will listen for touch events from touch screens to allow interaction with controls.
    /// </summary>
    internal class TouchHelper : TouchHelperBase
    {
        #region Members
        bool _tapValid;
        TimeSpan _touchAndHoldThreshold = TimeSpan.FromSeconds(2);
        TimeSpan _tapThreshold = TimeSpan.FromSeconds(2);
        DispatcherTimer _touchAndHoldTimer, _tapTimer;
        int _startedTimeStamp;

        UIElement _initalTouchElement;
        Point _initialTouchPoint;

		// JJD 04/05/12 - TFS107218
		// Added cached time member so we can calculate the time between ManipulationStarting events.
		// If this is less than the double click time we need to cancel the
		// the manipulation. Otherwise,  the MouseDoubleClick events never get raised
		int _lastManipulationStarting;

        bool _pannedData;

		[ThreadStatic]
		static Infragistics.Windows.HwndSourceHelper _hwndSourceHelper;
		static bool _canCreateHwndSource = true;
		
		[ThreadStatic]
		static int? _TickCountOfLastQuerySystemGestureStatus;

        #endregion // Members

        #region Constructor

        public TouchHelper() : this(null)
        {

        }

		public TouchHelper(IProvideTouchInfo touchInfoProvider) : base(touchInfoProvider)
        {

        }

        #endregion // Constructor

		#region Base class overrides

		#region OnAttached

		protected override void OnAttached()
		{
			UIElement element = this.Element;

			if (element != null)
			{
				ManageHwndSource(element, true);
				
				//element.ManipulationBoundaryFeedback	+= this._element_ManipulationBoundaryFeedback;
				element.ManipulationCompleted			+= this._element_ManipulationCompleted;
				element.ManipulationInertiaStarting		+= this._element_ManipulationInertiaStarting;
				element.ManipulationStarted				+= this._element_ManipulationStarted;
				element.ManipulationStarting			+= this._element_ManipulationStarting;
				element.ManipulationDelta				+= this._element_ManipulationDelta;
			}

			this._touchAndHoldTimer = new DispatcherTimer() { Interval = this.TouchAndHoldThreshold };
			this._touchAndHoldTimer.Tick += new EventHandler(this.TouchAndHoldTimer_Tick);

			this._tapTimer = new DispatcherTimer() { Interval = this.TapThreshold };
			this._tapTimer.Tick += new EventHandler(this.TapTimer_Tick);

		}

		#endregion //OnAttached

		#region OnDetached

		protected override void OnDetached()
		{
			UIElement element = this.Element;

			if (element != null)
			{
				ManageHwndSource(element, true);
				
				element.ManipulationBoundaryFeedback	-= this._element_ManipulationBoundaryFeedback;
				element.ManipulationCompleted			-= this._element_ManipulationCompleted;
				element.ManipulationInertiaStarting		-= this._element_ManipulationInertiaStarting;
				element.ManipulationStarted				-= this._element_ManipulationStarted;
				element.ManipulationStarting			-= this._element_ManipulationStarting;
				element.ManipulationDelta				-= this._element_ManipulationDelta;
			}

			if (_touchAndHoldTimer != null)
			{
				_touchAndHoldTimer.Stop();
				_touchAndHoldTimer.Tick -= new EventHandler(this.TouchAndHoldTimer_Tick);
				_touchAndHoldTimer = null;
			}

			if (_tapTimer != null)
			{
				_tapTimer.Stop();
				_tapTimer.Tick -= new EventHandler(this.TouchAndHoldTimer_Tick);
				_tapTimer = null;
			}
		}


		#endregion //OnDetached	

		#region OnStateChanged

		protected override void OnStateChanged(TouchState newState, TouchState oldState)
		{
			if (newState != TouchState.Pending)
			{
				_touchAndHoldTimer.Stop();
				_tapTimer.Stop();
			}

			base.OnStateChanged(newState, oldState);
		}

		#endregion //OnStateChanged

		#endregion //Base class overrides	
        
        #region Properties

        #region Public

        #region TouchAndHoldThreshold

        public TimeSpan TouchAndHoldThreshold
        {
            get
            {
                return this._touchAndHoldThreshold;
            }
            set
            {
                if (value != this._touchAndHoldThreshold)
                {
                    this._touchAndHoldThreshold = value;
                    this._touchAndHoldTimer.Interval = this._touchAndHoldThreshold;
                }
            }
        }

        #endregion // TouchAndHoldThreshold

        #region TapThreshold

        public TimeSpan TapThreshold
        {
            get
            {
                return this._tapThreshold;
            }
            set
            {
                if (value != this._tapThreshold)
                {
                    this._tapThreshold = value;
                    this._tapTimer.Interval = this._tapThreshold;
                }
            }
        }

        #endregion // TapThreshold

        #endregion // Public

        #endregion // Properties

        #region Methods
    
		#region HasLastQueryTimeExpired

		private bool HasLastQueryTimeExpired()
		{
			bool expired = false;

			if (_TickCountOfLastQuerySystemGestureStatus.HasValue)
			{
				expired = (Math.Abs(Environment.TickCount - _TickCountOfLastQuerySystemGestureStatus.Value) > TouchHelperBase.TOUCH_HOLD_TIME);

				_TickCountOfLastQuerySystemGestureStatus = null;
			}

			return expired;
		}

		#endregion //HasLastQueryTimeExpired	
    
		#region ManageHwndSource
		[System.Security.SecuritySafeCritical]
		private static void ManageHwndSource(UIElement element, bool isAdding)
		{
			CoreUtilities.ValidateNotNull(element);

			if (_hwndSourceHelper == null)
			{
				if (!isAdding || !_canCreateHwndSource)
				{
					Debug.Assert(isAdding || !_canCreateHwndSource, "Removing but never created?");
					return;
				}

				if (_canCreateHwndSource)
				{
					try
					{
						_hwndSourceHelper = new Windows.HwndSourceHelper(new System.Windows.Interop.HwndSourceHook(WndProc));
					}
					catch (System.Security.SecurityException ex)
					{
						_canCreateHwndSource = false;
					}
				}

				if (_hwndSourceHelper == null)
					return;
			}

			if (isAdding)
				_hwndSourceHelper.AddElement(element);
			else
				_hwndSourceHelper.RemoveElement(element);
		}
		#endregion //ManageHwndSource

		#region WndProc
		private static IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (message == Infragistics.Windows.Helpers.NativeWindowMethods.WM_TABLET_QUERYSYSTEMGESTURESTATUS)
			{
				_TickCountOfLastQuerySystemGestureStatus = Environment.TickCount;
			}

			return IntPtr.Zero;
		}
		#endregion //WndProc

        #endregion // Methods

        #region EventHandlers

        void _element_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
			this._touchAndHoldTimer.Stop();

			if (e.ManipulationContainer != this.Element)
				return;

			// if the user touches down again while we are scrolling then end the current scroll operation
			if (this.CurrentState == TouchState.Scrolling && _TickCountOfLastQuerySystemGestureStatus.HasValue)
			{
				e.Complete();
				e.Handled = true;
				this.CurrentState = TouchState.NotDown;
				return;
			}

			if (this.HasLastQueryTimeExpired())
			{
				this.CurrentState = TouchState.Holding;
				this.CurrentState = TouchState.NotDown;
				e.Cancel();
				e.Handled = true;
				return;
			}

			if (this.CurrentState == TouchState.Pending)
			{
				int currentTicks = Environment.TickCount;

				if (Math.Abs(currentTicks - this._startedTimeStamp) <= TOUCH_HOLD_TIME)
				{
					Vector translation = e.CumulativeManipulation.Translation;
					if (Math.Abs(translation.X) <= TOUCH_MOVE_THRESHHOLD &&
						Math.Abs(translation.Y) <= TOUCH_MOVE_THRESHHOLD)
					{
						e.Handled = true;
						return;
					}

					this.CurrentState = TouchState.Scrolling;
				}
				else
				{
					this.CurrentState = TouchState.Holding;
				}
			}

            this._tapValid = false;

			if (this.CurrentState != TouchState.Scrolling)
			{
				this.CurrentState = TouchState.NotDown;

				e.Cancel();
				e.Handled = true;

				return;
			}

            this.OnPan(e);
            this._pannedData = true;

			e.Handled = true;
        }

        void _element_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
			if (e.ManipulationContainer != this.Element)
				return;

			// JJD 04/05/12 - TFS107218
			// Calculate the time delta from the last ManipulationStarting.
			// If this is less than the double click time we should cancel the
			// the manipulation. Otherwise, the MouseDoubleClick events
			// will never get raised
			int oldTickCount = _lastManipulationStarting;
			_lastManipulationStarting = Environment.TickCount;
			int delta = Math.Abs(_lastManipulationStarting - oldTickCount);

			if ( delta < Infragistics.Windows.Utilities.SystemDoubleClickTime )
			{
				this.CurrentState = TouchState.NotDown;
				e.Cancel();
				e.Handled = true;
				// reset the _lastManipulationStarting so we don't interfere with triple clicks
				_lastManipulationStarting = 0;
				return;
			}

			if (this.HasLastQueryTimeExpired() )
			{
				this.CurrentState = TouchState.NotDown;
				e.Cancel();
				e.Handled = true;
				return;
			}

			Point point = new Point(-1000, -1000);
			UIElement elementDirctlyOver = null;
			foreach (IManipulator manipulator in e.Manipulators)
			{
				TouchDevice td = manipulator as TouchDevice;

				point = manipulator.GetPosition(this.Element);

				var elem = this.Element.InputHitTest(point) as DependencyObject;

				while (elem is ContentElement )
					elem = LogicalTreeHelper.GetParent(elem);

				elementDirctlyOver = elem as UIElement;

				break;
			}

			this.SetScrollModeFromPoint(point, elementDirctlyOver);

			switch (this.ScrollMode )
			{
				case TouchScrollMode.None:
					return;

				case TouchScrollMode.Horizontal:
					e.Mode = ManipulationModes.TranslateX;
					break;

				case TouchScrollMode.Vertical:
					e.Mode = ManipulationModes.TranslateY;
					break;

				case TouchScrollMode.Both:
				default:
					e.Mode = ManipulationModes.Translate;
					break;
			}

			e.Handled = true;
        }	

        void _element_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
			if (e.ManipulationContainer != this.Element)
				return;

            this._initalTouchElement = e.ManipulationContainer as UIElement;
            this._initialTouchPoint = e.ManipulationOrigin;

			if (this.ScrollMode == TouchScrollMode.None)
			{
				this.CurrentState = TouchState.NotDown;
				e.Cancel();
				e.Handled = true;
				return;
			}

			this.CurrentState = TouchState.Pending;
            this._tapValid = true;

            this._tapTimer.Start();
            this._touchAndHoldTimer.Start();

            int currentTicks = Environment.TickCount;

            if (Math.Abs((long)((long)currentTicks - (long)this._startedTimeStamp)) <= DOUBLE_CLICK_TIME)
            {
                this.OnDoubleTap(this._initialTouchPoint, this._initalTouchElement);
            }

            this._startedTimeStamp = Environment.TickCount;

			e.Handled = true;
        }

        void _element_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
			if (e.ManipulationContainer != this.Element)
				return;
			
			ManipulationVelocities velocities = e.InitialVelocities;

			double absX = Math.Abs(velocities.LinearVelocity.X);
			double absY = Math.Abs(velocities.LinearVelocity.Y);

			if (this.CurrentState == TouchState.Pending)
			{
				if (absX < .001 &&
					absY < .001)
				{
					e.Cancel();
					e.Handled = true;

					this.CurrentState = TouchState.NotDown;
					return;
				}
			}
			else
			{
				if (this.CurrentState != TouchState.Scrolling)
				{
					this.CurrentState = TouchState.NotDown;
					e.Cancel();
					e.Handled = true;
					return;
				}
			}

			InertiaTranslationBehavior behavior = e.TranslationBehavior;

			if (behavior != null)
			{
				// if the velocity is primarily in one direction
				// then only go in that direction
				if (absX * FLICK_DIRECTION_RATIO_THRESHHOLD > absY)
					behavior.InitialVelocity = new Vector(velocities.LinearVelocity.X, 0);
				else
				if (absY * FLICK_DIRECTION_RATIO_THRESHHOLD > absX)
					behavior.InitialVelocity = new Vector(0, velocities.LinearVelocity.Y);
			}

            if (!e.Handled)
            {






				e.TranslationBehavior.DesiredDeceleration = .001;

			
				e.Handled = true;
            }
        }

        void _element_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            this._touchAndHoldTimer.Stop();

			if (e.ManipulationContainer != this.Element)
				return;

            if (this._tapValid)
            {
                this.OnTap(e.ManipulationOrigin, e.ManipulationContainer as UIElement);
            }

            this._initalTouchElement = null;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			if (this._pannedData)
			{
				// reset the flag
				_pannedData = false;
				
				this.OnPanComplete();
			}

            this.OnTouchCompleted(e.ManipulationOrigin, e.ManipulationContainer as UIElement);

			switch( this.CurrentState )
			{
				case TouchState.Pending:
				case TouchState.Holding:
					e.Cancel();
					break;
			}

			this.CurrentState = TouchState.NotDown;

			e.Handled = true;
        }

        void _element_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {

        }


	#region TapTimer_Tick

        void TapTimer_Tick(object sender, EventArgs e)
        {
            this._tapValid = false;
            this._tapTimer.Stop();

        }
		#endregion // TapTimer_Tick

	#region TouchAndHoldTimer_Tick

        void TouchAndHoldTimer_Tick(object sender, EventArgs e)
        {
            this._touchAndHoldTimer.Stop();

			if (this.CurrentState == TouchState.Pending)
			{
				this.CurrentState = TouchState.Holding;

				this.OnTapAndHold(this._initialTouchPoint, this._initalTouchElement);

				this.CurrentState = TouchState.NotDown;
			}
        }
		#endregion // TouchAndHoldTimer_Tick

		#endregion // EventHandlers
    }
	#endregion // TouchHelper


	#region TouchHelperBase
	internal abstract class TouchHelperBase : DependencyObject
    {
		#region Private Members

		private IProvideTouchInfo _touchInfoProvider;
		private TouchScrollMode _scrollMode;
		private TouchState _currentState = TouchState.NotDown;
		private UIElement _element;

		#endregion //Private Members

		#region Constants

		protected const int TOUCH_MOVE_THRESHHOLD = 3;
		protected const int DOUBLE_CLICK_TIME = 500;
		protected const int TOUCH_HOLD_TIME = 400;
		protected const double FLICK_DIRECTION_RATIO_THRESHHOLD = 4;

		#endregion //Constants	
    
		protected TouchHelperBase(IProvideTouchInfo touchInfoProvider)
		{
			_touchInfoProvider = touchInfoProvider;
		}

        #region Properties

		#region CurrentState

		public TouchState CurrentState
		{
			get { return _currentState; }
			protected set
			{
				if (value != _currentState)
				{
					TouchState oldState = _currentState;

					_currentState = value;

					this.OnStateChanged(_currentState, oldState);
				}
			}
		}

		#endregion //CurrentState	
    
		#region Element

		public UIElement Element 
		{
			get { return _element; }
		}

		#endregion //Element	
		
		#region ScrollMode

		internal TouchScrollMode ScrollMode { get { return _scrollMode; } }

   		#endregion //ScrollMode	
    		
		#region TouchInfoProvider

		internal IProvideTouchInfo TouchInfoProvider { get { return _touchInfoProvider; } }

		#endregion //TouchInfoProvider	

		#region TouchHelper

        /// <summary>
        /// An attached property that Gets/Sets the <see cref="TouchHelper"/> that should be attached to a <see cref="UIElement"/>
        /// </summary>
        public static readonly DependencyProperty TouchHelperProperty = DependencyProperty.RegisterAttached("TouchHelper", typeof(TouchHelperBase), typeof(TouchHelperBase), new PropertyMetadata(null, OnTouchHelperChanged));

        /// <summary>
        /// Gets the <see cref="TouchHelper"/> attached to a specified <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static TouchHelper GetTouchHelper(UIElement element)
        {
            return (TouchHelper)element.GetValue(TouchHelperProperty);
        }

        /// <summary>
        /// Sets the <see cref="TouchHelper"/> that should be attached to the specified <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="helper"></param>
        public static void SetTouchHelper(UIElement element, TouchHelper helper)
        {
			element.SetValue(TouchHelperProperty, helper);
		}

		static void OnTouchHelperChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			UIElement element = target as UIElement;

			TouchHelperBase oldHelper = e.OldValue as TouchHelperBase;

			if (oldHelper != null)
			{
				// call the virtual OnDetached method before clearing the element member 
				oldHelper.OnDetached();

				// clear the elment member
				oldHelper._element = null;
			}

			TouchHelperBase newHelper = e.NewValue as TouchHelperBase;

			if (newHelper != null)
			{
				// initialize the elment member
				newHelper._element = element;

				// call the vrtual OnAttached method
				newHelper.OnAttached();
			}

		}

		#endregion // TouchHelper
       
        #endregion // Properties

		#region Methods

		#region CancelTouchInteraction

		protected internal virtual void CancelTouchInteraction()
		{
		}

		#endregion //CancelTouchInteraction

		#region OnAttached

		protected abstract void OnAttached();

		#endregion //OnAttached

		#region OnDetached

		protected abstract void OnDetached();

		#endregion //OnDetached	
    
		#region SetScrollModeFromPoint

		internal void SetScrollModeFromPoint(Point point, UIElement elementDirectlyOver)
		{
			if ( _touchInfoProvider == null )
				_scrollMode = TouchScrollMode.Both;
			else
				_scrollMode = _touchInfoProvider.GetScrollModeFromPoint(point, elementDirectlyOver);
		}

		#endregion //SetScrollModeFromPoint

		#endregion //Methods

		#region Events

		#region OnTouchStarted

		public event EventHandler<TapEventArgs> TouchStarted;

        protected virtual void OnTouchStarted(Point p, UIElement element)
        {
            if (this.TouchStarted != null)
            {
                this.TouchStarted(this.Element, new TapEventArgs(p, element));
            }
        }

        #endregion // OnTouchStarted

        #region OnTouchCompleted

        public event EventHandler<TapEventArgs> TouchCompleted;

        protected virtual void OnTouchCompleted(Point p, UIElement element)
        {
            if (this.TouchCompleted != null)
            {
                this.TouchCompleted(this.Element, new TapEventArgs(p, element));
            }
        }

        #endregion // OnTouchCompleted

        #region OnTap

        public event EventHandler<TapEventArgs> Tap;

        protected virtual void OnTap(Point p, UIElement element)
        {
            if (this.Tap != null)
            {
                this.Tap(this.Element, new TapEventArgs(p, element));
            }
        }

        #endregion // OnTap

        #region OnDoubleTap

        public event EventHandler<TapEventArgs> DoubleTap;

        protected virtual void OnDoubleTap(Point p, UIElement element)
        {
            if (this.DoubleTap != null)
            {
                this.DoubleTap(this.Element, new TapEventArgs(p, element));
            }
        }

        #endregion // OnDoubleTap

        #region OnTapAndHold

        public event EventHandler<TapEventArgs> TapAndHold;

        protected virtual void OnTapAndHold(Point p, UIElement element)
        {
            if (this.TapAndHold != null)
            {
                this.TapAndHold(this.Element, new TapEventArgs(p, element));
            }
        }

        #endregion // OnTapAndHold

        #region OnPan

        public event EventHandler<PanEventArgs> Pan;

        protected virtual bool OnPan(ManipulationDeltaEventArgs e)
        {
            if (this.Pan != null)
            {
                Point p = new Point(e.DeltaManipulation.Translation.X, e.DeltaManipulation.Translation.Y);
                PanEventArgs args = new PanEventArgs(p);
                this.Pan(this.Element, args);
                return args.Handled;
            }

            return false;
        }

        protected virtual bool OnPan(Point p)
        {
            if (this.Pan != null)
            {
                PanEventArgs args = new PanEventArgs(p);
                this.Pan(this.Element, args);
                return args.Handled;
            }

            return false;
        }

        #endregion // OnPan

        #region OnFlick



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


        #endregion // OnFlick

        #region OnPanComplete

        /// <summary>
        /// Event raised when you hold and drag down with the finger.
        /// </summary>
        public event EventHandler<EventArgs> PanComplete;

        /// <summary>
        /// Raises the <see cref="PanComplete"/> event.
        /// </summary>
        protected virtual void OnPanComplete()
        {
            if (this.PanComplete != null)
            {
                this.PanComplete(this.Element, EventArgs.Empty);
            }
        }

        #endregion // OnPanComplete

		#region OnStateChanged

		public event EventHandler<StateChangedEventArgs> StateChanged;

        protected virtual void OnStateChanged(TouchState newState, TouchState oldState)
        {
            if (this.StateChanged != null)
            {
                this.StateChanged(this.Element, new StateChangedEventArgs(newState, oldState));
            }
		}

		#endregion // OnStateChanged

		#endregion // Events
	}
    #endregion // TouchHelperBase
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