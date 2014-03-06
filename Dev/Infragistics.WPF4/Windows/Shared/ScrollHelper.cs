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
using System.Diagnostics;

namespace Infragistics.Controls
{
    #region ISupportScrollHelper

    /// <summary>
    /// An interface that will be used by the <see cref="TouchScrollHelper"/> class.
    /// </summary>
    internal interface ISupportScrollHelper
    {

        #region GetFirstItemHeight
        /// <summary>
        /// Gets the height of the first visually available element in the scroll area.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// When using ScrollType.Item the acceleration calculation uses this value to evaluate.
        /// </remarks>
        double GetFirstItemHeight();
        #endregion // GetFirstItemHeight

        #region GetFirstItemWidth
        /// <summary>
        /// Gets the width of the first visually available element in the scroll area.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// When using ScrollType.Item the acceleration calculation uses this value to evaluate.
        /// </remarks>
        double GetFirstItemWidth();
        #endregion // GetFirstItemWidth
		
		#region GetScrollModeFromPoint
		/// <summary>
		/// Method called to determine which scroll dimensions are valid for a specific point.
		/// </summary>
		TouchScrollMode GetScrollModeFromPoint(Point point, UIElement elementDirectlyOver);
		#endregion // GetScrollModeFromPoint

        #region HorizontalMax
        /// <summary>
        /// The maximum value for the horizontal scroll bar.
        /// </summary>
        double HorizontalMax { get; }
        #endregion // HorizontalMax

        #region HorizontalScrollType
        /// <summary>
        /// Gets the <see cref="ScrollType"/> that control the will use to control how horizontal scrolling for the control.
        /// </summary>
        ScrollType HorizontalScrollType { get; }
        #endregion // HorizontalScrollType

        #region HorizontalValue
        /// <summary>
        /// The current value of the horizontal scroll bar
        /// </summary>
        double HorizontalValue { get; set; }
        #endregion // HorizontalValue

        #region InvalidateScrollLayout

        /// <summary>
        /// This method will be called by the <see cref="TouchScrollHelper"/> when it wants to notify the <see cref="ISupportScrollHelper"/> implementer that it needs to redraw.
        /// </summary>
        void InvalidateScrollLayout();

        #endregion // InvalidateScrollLayout

		#region OnPanComplete
		/// <summary>
		/// Method called when a pan completes.
		/// </summary>
		void OnPanComplete();
		#endregion // OnPanComplete

		#region OnStateChanged
		/// <summary>
        /// Method called when a flick action finishes.
        /// </summary>
        void OnStateChanged(TouchState newState, TouchState oldState);
		#endregion // OnStateChanged

        #region VerticalMax
        /// <summary>
        /// The maximum value for the vertical scroll bar.
        /// </summary>
        double VerticalMax { get; }
        #endregion // VerticalMax 

		#region VerticalScrollType
        /// <summary>
        /// Gets the <see cref="ScrollType"/> that control the will use to control how vertical scrolling for the control.
        /// </summary>
        ScrollType VerticalScrollType { get; }
        #endregion // VerticalScrollType

        #region VerticalValue
        /// <summary>
        /// The current value of the vertical scroll bar
        /// </summary>
        double VerticalValue { get; set; }
        #endregion // VerticalValue
    }

    #endregion // ISupportScrollHelper

    #region ScrollType

    /// <summary>
    /// Enum that describes how touch scrolling will be controlled.
    /// </summary>
    internal enum ScrollType
    {
        /// <summary>
        /// A control using ScrollType.Pixel will accelerate based on how far the touch was moved across the screen and the size of the control.  
        /// </summary>
        Pixel,

        /// <summary>
        /// The control will provide information which will control how the acceleration will be calculated.
        /// </summary>
        /// <remarks>
		/// When using Item based scrolling, the <see cref="ISupportScrollHelper"/> implementer will provide sizing information which will affect how acceleration will be calculation.
        /// </remarks>
        Item
    }

    #endregion // ScrollType

    #region TouchScrollHelper

    /// <summary>
    /// This class supports the <see cref="TouchHelper"/> class for Silverlight and WPF touch screen support.
    /// </summary>
	internal class TouchScrollHelper : IProvideTouchInfo
    {
        #region Members

        private ISupportScrollHelper _owner;

        private FrameworkElement _element;






 		private bool _animateToIntegralItemOnEndPan;
		private DoubleValueAnimator _panningEndAnimatorX;
		private DoubleValueAnimator _panningEndAnimatorY;

		private bool _isEnabled = true;

        private TouchHelper _touchHandler;

        #endregion // Members

		#region Constants

		private const double VELOCITY_EXTENT_FACTOR = 2.0;

		#endregion //Constants	
    
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchScrollHelper"/> class.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> which touches will be listened for.</param>
        /// <param name="owner">The <see cref="ISupportScrollHelper"/> which contains properties which will control the touch behavior.</param>
        public TouchScrollHelper(FrameworkElement element, ISupportScrollHelper owner)
        {
            this._owner = owner;

            this._element = element;

            this._touchHandler = new TouchHelper(this);

            this.ReconnectEvents();
        }

        #endregion // Constructor

        #region Properties

		#region AnimateToIntegralItemOnEndPan

		public bool AnimateToIntegralItemOnEndPan
		{
			get { return _animateToIntegralItemOnEndPan; }

			set
			{
				if (value != _animateToIntegralItemOnEndPan)
				{
					_animateToIntegralItemOnEndPan = value;

					if (!_animateToIntegralItemOnEndPan)
						this.StopPanningEndAnimation();
				}
			}
		}

		#endregion //AnimateToIntegralItemOnEndPan	
    		
		#region CurrentState

		public TouchState CurrentState
		{
			get { return _touchHandler.CurrentState; }
		}

		#endregion //CurrentState	
     		
		// JJD 03/14/12 - TFS100150 - Added 
		#region IsEnabled

		public bool IsEnabled
		{
			get { return _isEnabled; }
			set 
			{
				if (value != _isEnabled)
				{
					_isEnabled = value;

					if (_isEnabled)
						this.ReconnectEvents();
					else
						this.DisconnectEvents();
				}
			}
		}

		#endregion //CurrentState	
        
		#endregion // Properties

        #region Methods

        #region Public

        public void CancelTouchInteraction()
        {
            if (this._touchHandler != null)
                this._touchHandler.CancelTouchInteraction();
        }

        #endregion // Public

        #region Private

		#region CreateAnimation

		private Timeline CreateAnimation(int durationInMilliseconds, double fromValue, double toValue, IEasingFunction easing)
		{
			DoubleAnimation animation = new DoubleAnimation();
			animation.Duration = TimeSpan.FromMilliseconds(durationInMilliseconds);
			animation.AutoReverse = false;
			animation.FillBehavior = FillBehavior.Stop;

			animation.From = fromValue;
			animation.To = toValue;
			animation.EasingFunction = easing;





			return animation;
		}

		#endregion //CreateAnimation	
    
		#region CreateInertialAnimator



#region Infragistics Source Cleanup (Region)


























































#endregion // Infragistics Source Cleanup (Region)

		#endregion //CreateInertialAnimator

		#region CreatePanningEndAnimator

		private DoubleValueAnimator CreatePanningEndAnimator(double fromValue, double toValue, double elementExtent)
		{
			int duration = Math.Max(Math.Min((int)(Math.Abs(toValue - fromValue) * elementExtent * 4), 750), 200);

			return DoubleValueAnimator.Create(new Action<DoubleValueAnimator>(this.OnPanningEndAnimationValueChanged), 
											new Action<DoubleValueAnimator>(this.OnPanningEndAnimationComplete),
											this.CreateAnimation(duration, fromValue, toValue,  new QuadraticEase() { EasingMode = EasingMode.EaseOut}));
		}

		#endregion //CreatePanningEndAnimator

		#region DisconnectEvents

		private void DisconnectEvents()
		{
			this._element.ClearValue(TouchHelperBase.TouchHelperProperty);

			if (this._touchHandler != null)
			{
				this._touchHandler.TouchStarted -= TouchHandler_TouchStarted;
				this._touchHandler.Pan -= TouchHandler_Pan;



				this._touchHandler.TouchCompleted -= TouchHandler_TouchCompleted;
				this._touchHandler.PanComplete -= TouchHandler_PanComplete;
				this._touchHandler.StateChanged -= new EventHandler<StateChangedEventArgs>(TouchHandler_StateChanged);
			}
		}

		#endregion //DisconnectEvents	

		#region OnInertiaAnimationValueChanged



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


		#endregion //OnInertiaAnimationValueChanged	
    
		#region OnInertiaAnimationComplete



#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


		#endregion //OnInertiaAnimationComplete

		#region OnPanningEndAnimationValueChanged

		private void OnPanningEndAnimationValueChanged(DoubleValueAnimator animator)
		{
			if (animator == _panningEndAnimatorX)
			{
				_owner.HorizontalValue = animator.Value;
				_owner.InvalidateScrollLayout();
			}
			else if (animator == _panningEndAnimatorY)
			{
				_owner.VerticalValue = animator.Value;
				_owner.InvalidateScrollLayout();
			}
		}

		#endregion //OnPanningEndAnimationValueChanged	
    
		#region OnPanningEndAnimationComplete

		private void OnPanningEndAnimationComplete(DoubleValueAnimator animator)
		{
			bool allPanAnimationsComplete = false;

			if (animator == _panningEndAnimatorX)
			{
				_owner.HorizontalValue = CoreUtilities.RoundToIntegralValue(_owner.HorizontalValue);
				_panningEndAnimatorX = null;
				allPanAnimationsComplete = _panningEndAnimatorY == null;
			}
			else if (animator == _panningEndAnimatorY)
			{
				_owner.VerticalValue = CoreUtilities.RoundToIntegralValue(_owner.VerticalValue);
				_panningEndAnimatorY = null;
				allPanAnimationsComplete = _panningEndAnimatorX == null;
			}

			if (allPanAnimationsComplete && this._owner != null)
			{
				this._owner.OnPanComplete();
			}
		}

		#endregion //OnPanningEndAnimationComplete	
        
		#region ReconnectEvents

		private void ReconnectEvents()
		{
			this.DisconnectEvents();

			TouchHelperBase.SetTouchHelper(_element, _touchHandler);

			if (this._touchHandler != null)
			{
				this._touchHandler.TouchStarted += TouchHandler_TouchStarted;
				this._touchHandler.Pan += TouchHandler_Pan;



				this._touchHandler.TouchCompleted += TouchHandler_TouchCompleted;
				this._touchHandler.PanComplete += TouchHandler_PanComplete;
				this._touchHandler.StateChanged += new EventHandler<StateChangedEventArgs>(TouchHandler_StateChanged);
			}
		}

		#endregion //ReconnectEvents	

		#region StopPanningEndAnimation

		private void StopPanningEndAnimation()
        {
			_panningEndAnimatorY = null;
			_panningEndAnimatorY = null;
        }

		#endregion // StopPanningEndAnimation

        #endregion // Private

        #endregion // Methods

        #region EventHandlers

		#region TouchHandler_TouchStarted

		void TouchHandler_TouchStarted(object sender, EventArgs e)
		{


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			this.OnTouchStarted();
		}

		#endregion //TouchHandler_TouchStarted	
    
		#region TouchHandler_Pan

		void TouchHandler_Pan(object sender, PanEventArgs e)
		{
			this.StopPanningEndAnimation();

			bool invalidate = false;

			Point delta = e.DeltaPoint;

			#region Process panning in the Y dimension

			double vertMax = this._owner.VerticalMax;

			if (vertMax > 0)
			{
				double val = delta.Y;

				if (this._owner.VerticalScrollType == ScrollType.Item)
				{
					double firstVisibleItemHeight = this._owner.GetFirstItemHeight();

					val = val / firstVisibleItemHeight;
				}

				if (val != 0)
				{
					invalidate = true;

					if (val < 0)
						this._owner.VerticalValue = Math.Min(vertMax, this._owner.VerticalValue - val);
					else
						this._owner.VerticalValue = Math.Max(0, this._owner.VerticalValue - val);
				}
			}

			#endregion //Process panning in the Y dimension	
    
			#region Process panning in the X dimension

			double horzMax = this._owner.HorizontalMax;

			if (horzMax > 0)
			{
				double val = delta.X;

				if (this._owner.HorizontalScrollType == ScrollType.Item)
				{
					double firstVisibleItemWidth = this._owner.GetFirstItemWidth();

					val = val / firstVisibleItemWidth;
				}

				if (val != 0)
				{
					invalidate = true;

					if (val < 0)
						this._owner.HorizontalValue = Math.Min(horzMax, this._owner.HorizontalValue - val);
					else
						this._owner.HorizontalValue = Math.Max(0, this._owner.HorizontalValue - val);
				}
			}

			#endregion //Process panning in the X dimension	
    
			if (invalidate)
			{
				this._owner.InvalidateScrollLayout();
			}

			e.Handled = true;
		}

		#endregion //TouchHandler_Pan	
    
		#region TouchHandler_Flick



#region Infragistics Source Cleanup (Region)








































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //TouchHandler_Flick

		#region TouchHandler_StateChanged

		void TouchHandler_StateChanged(object sender, StateChangedEventArgs e)
		{
			if (_owner != null)
				_owner.OnStateChanged(e.NewState, e.OldState);
		}

		#endregion //TouchHandler_StateChanged	
    
		#region TouchHandler_TouchCompleted

		void TouchHandler_TouchCompleted(object sender, EventArgs e)
		{
			this.OnTouchCompleted();
		}

		#endregion //TouchHandler_TouchCompleted	
    
		#region TouchHandler_PanComplete

		void TouchHandler_PanComplete(object sender, EventArgs e)
		{
			this.ProcessPanComplete();
		}

		#endregion //TouchHandler_PanComplete	
    
		#region ProcessPanComplete

		private void ProcessPanComplete()
		{
			// stop any old animation
			this.StopPanningEndAnimation();
			
			if (this._owner == null)
				return;


			// check whether we need to start an animation based on the AnimateToIntegralItemOnEndPan property
			if (!this.AnimateToIntegralItemOnEndPan)
			{
				this._owner.OnPanComplete();
				return;
			}

			#region Vertical Pan end processing

			if (_owner.VerticalScrollType == ScrollType.Item)
			{
				// get the item extent
				double itemExtent = _owner.GetFirstItemHeight();

				// use thte current value as the 'from' value
				double fromValue = _owner.VerticalValue;

				// uround to the nearest intgral value to get the 'to' value
				double toValue = CoreUtilities.RoundToIntegralValue(fromValue);

				// calculate the delta number of pixels between the old and new values
				double deltaPixels =  itemExtent * (fromValue - toValue);

				// if the delta pixels is greater than 5 animate bto the 'to' value.
				// Otherwise, just step the value directly
				if (Math.Abs(deltaPixels) > 5)
					_panningEndAnimatorY = this.CreatePanningEndAnimator(fromValue, toValue, itemExtent);
				else
					_owner.VerticalValue = toValue;

			}

			#endregion //Vertical Pan end processing	
    
			#region Horizontal pan end processing

			if (_owner.HorizontalScrollType == ScrollType.Item)
			{
				// get the item extent
				double itemExtent = _owner.GetFirstItemWidth();

				// use thte current value as the 'from' value
				double fromValue = _owner.HorizontalValue;

				// uround to the nearest intgral value to get the 'to' value
				double toValue = CoreUtilities.RoundToIntegralValue(fromValue);

				// calculate the delta number of pixels between the old and new values
				double deltaPixels = itemExtent * (fromValue - toValue);

				// if the delta pixels is greater than 5 animate bto the 'to' value.
				// Otherwise, just step the value directly
				if (Math.Abs(deltaPixels) > 5)
					_panningEndAnimatorX = this.CreatePanningEndAnimator(fromValue, toValue, itemExtent);
				else
					_owner.HorizontalValue = toValue;
			}

			#endregion //Horizontal pan end processing	
    
			if (_panningEndAnimatorX == null && _panningEndAnimatorY == null)
				this._owner.OnPanComplete();
			
		}

		#endregion //ProcessPanComplete	
    
        #endregion // EventHandlers

        #region Events

        #region OnTouchStarted

        /// <summary>
        /// Event raised when a finger touches the screen.
        /// </summary>
        public event EventHandler<EventArgs> TouchStarted;

        /// <summary>
        /// Raises the <see cref="TouchStarted"/> event.
        /// </summary>
        protected virtual void OnTouchStarted()
        {
            if (this.TouchStarted != null)
            {
                this.TouchStarted(this._element, EventArgs.Empty);
            }
        }

        #endregion // OnTouchStarted

        #region OnTouchCompleted

        /// <summary>
        /// Event raised when the finger is removed from the screen.
        /// </summary>
        public event EventHandler<EventArgs> TouchCompleted;

        /// <summary>
        /// Raises the <see cref="TouchCompleted"/> event.
        /// </summary>
        protected virtual void OnTouchCompleted()
        {
            if (this.TouchCompleted != null)
            {
                this.TouchCompleted(this._element, EventArgs.Empty);
            }
        }

        #endregion // OnTouchCompleted

        #endregion // Events

		#region IProvideTouchInfo Members

		TouchScrollMode IProvideTouchInfo.GetScrollModeFromPoint(Point point, UIElement elementDirectlyOver)
		{
			if (_owner != null)
				return _owner.GetScrollModeFromPoint(point, elementDirectlyOver);

			return TouchScrollMode.None;
		}

		#endregion

		#region DoubleValueAnimator nested private class

		private class DoubleValueAnimator : DependencyObject
		{
			#region Private Members

			private Action<DoubleValueAnimator> _onChangedAction;
			private Action<DoubleValueAnimator> _onAnimatedCompleteAction;
			private Timeline _animation;
			private Storyboard _storyboard;

			#endregion //Private Members

			#region Constructor

			private DoubleValueAnimator(Timeline animation)
			{
				_animation = animation;
			}

			#endregion //Constructor

			#region Static Create method

			internal static DoubleValueAnimator Create(Action<DoubleValueAnimator> onChangedAction, Action<DoubleValueAnimator> onAnimationCompleteAction, Timeline animation)
			{
				DoubleValueAnimator animator = new DoubleValueAnimator(animation);

				animator.Initialize(onChangedAction, onAnimationCompleteAction);

				return animator;
			}

			#endregion //Static Create method	
    
			#region Properties

			#region Public Properties

			#region Value

			public static readonly DependencyProperty ValueProperty = DependencyPropertyUtilities.Register("Value",
				typeof(double), typeof(DoubleValueAnimator),
				DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnValueChanged))
				);

			private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				DoubleValueAnimator instance = (DoubleValueAnimator)d;

				if (instance._onChangedAction != null && !double.IsNaN((double)e.NewValue))
					instance._onChangedAction(instance);
			}

			public double Value
			{
				get
				{
					return (double)this.GetValue(DoubleValueAnimator.ValueProperty);
				}
				set
				{
					this.SetValue(DoubleValueAnimator.ValueProperty, value);
				}
			}

			#endregion //Value

			#endregion //Public Properties

			#endregion //Properties

			#region Methods

			#region Private Methods

			private void Initialize(Action<DoubleValueAnimator> onChangedAction, Action<DoubleValueAnimator> onAnimationCompleteAction)
			{
				_onAnimatedCompleteAction = onAnimationCompleteAction;

				if (_animation != null)
				{
					if ((!(_animation is DoubleAnimation)) &&
						 (!(_animation is DoubleAnimationUsingKeyFrames)))
						throw new ArgumentException("animation must be DoubleAnimation or DoubleAnimationUsingKeyFrames");

					_storyboard = new Storyboard();
					_storyboard.Completed += new EventHandler(OnStoryboardCompleted);

					_storyboard.FillBehavior = FillBehavior.Stop;

					Storyboard.SetTargetProperty(_animation, new PropertyPath(ValueProperty));

					_storyboard.Children.Add(_animation);

					Storyboard.SetTarget(_animation, this);

					_storyboard.Begin();
				}

				_onChangedAction = onChangedAction;
			}

			#region OnStoryboardCompleted

			private void OnStoryboardCompleted(object sender, EventArgs e)
			{
				_onChangedAction = null;

				if (_onAnimatedCompleteAction != null)
				{
					_onAnimatedCompleteAction(this);
					_onAnimatedCompleteAction = null;
				}
			}

			#endregion //OnStoryboardCompleted

			#endregion //Private Methods

			#endregion //Methods

		}

		#endregion //DoubleValueAnimator nested private class
	}

    #endregion // TouchScrollHelper
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