using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using System.Data;
using Infragistics.Shared;
using Infragistics.Windows.Helpers;

using System.Runtime.CompilerServices;
using System.Reflection;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;

namespace Infragistics.Windows.Internal
{
	#region ThrottleTimer Class

	// SSP 8/2/09 - Summary Recalc Optimizations
	// Added ThrottleTimer class.
	// 
	internal class ThrottleTimer : DependencyObject
	{
		#region Data Structures

		#region IAction Interface

		internal interface IAction
		{
			void Perform( );
		}

		#endregion // IAction Interface

		#region RegisteredCallback Class

		private class RegisteredCallback
		{
			#region Members

			private WeakReference _action;
			internal int _numberOfTimes;
			internal long _lastCallTime = -1;

			#endregion // Members

			#region Constructor

			internal RegisteredCallback( IAction action )
			{
				CoreUtilities.ValidateNotNull( action, "action" );
				_action = new WeakReference( action );
			}

			#endregion // Constructor

			#region Action

			internal IAction Action
			{
				get
				{
					return null != _action ? (IAction)CoreUtilities.GetWeakReferenceTargetSafe( _action ) : null;
				}
			}

			#endregion // Action
		}

		#endregion // RegisteredCallback Class

		#endregion // Data Structures

		#region Member Vars

		private Timer _timer;
		private WeakList<RegisteredCallback> _callbacksList = new WeakList<RegisteredCallback>( );
		private WeakDictionary<IAction, RegisteredCallback> _callbacks = new WeakDictionary<IAction, RegisteredCallback>( true, false );

		private int _indecesCounter;
		private long _timerInterval;
		private bool _inOnTimerTick;
		private bool _isTimerActive;
		private int _lastCompactCount;

		// SSP 4/13/12 TFS108549 - Optimizations
		// 
		private long _throttleInterval;
		private long _maxTickProcessDuration;

		// SSP 7/20/10
		// 
		private int _lastTickCount;
		private long _wrapCounter;

		[ThreadStatic]
		private static Dictionary<long, ThrottleTimer> g_instances;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public ThrottleTimer( long throttleInterval )
		{
			// SSP 4/13/12 TFS108549 - Optimizations
			// Added _throttleInterval member var.
			// 
			//_timerInterval = throttleInterval;
			_throttleInterval = throttleInterval;
			_timerInterval = throttleInterval / 10;
			_maxTickProcessDuration = _timerInterval / 4;
		}

		#endregion // Constructor

		#region Properties

		#region Private/Internal Properties

		#region CurrentTime

		private long CurrentTime
		{
			get
			{
				// SSP 7/20/10
				// Handle TickCount wrapping around back to 0.
				// 
				//return Environment.TickCount;
				int tickCount = Environment.TickCount;

				if ( tickCount < _lastTickCount )
					_wrapCounter += int.MaxValue;

				_lastTickCount = tickCount;

				return _wrapCounter + tickCount;
			}
		}

		#endregion // CurrentTime

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region DoThrottledInvoke

		// SSP 4/10/12 TFS108549 - Optimizations
		// Added 'throttleFirstInvoke' parameter.
		// 
		//public void DoThrottledInvoke( IAction action )
		public void DoThrottledInvoke( IAction action, bool throttleFirstInvoke )
		{
			RegisteredCallback c;
			if ( !_callbacks.TryGetValue( action, out c ) )
				c = this.RegisterCallback( action, 0 );

			// If the amount of time elapsed since last invoke is greater than throttleInterval then
			// invoke right now. Otherwise let the timer invoke it.
			// 
			long currentTime = this.CurrentTime;

			// SSP 4/10/12 TFS108549 - Optimizations
			// -1 last call time indicates that it was never invoked before.
			// 
			if ( throttleFirstInvoke && -1 == c._lastCallTime )
				c._lastCallTime = currentTime;

			if ( currentTime - c._lastCallTime > _timerInterval )
			{
				c._lastCallTime = currentTime;

				IAction tmpAction = c.Action;
				if ( null != tmpAction )
				{
					if ( c._numberOfTimes > 0 )
						c._numberOfTimes--;

					tmpAction.Perform( );
				}
			}
			else
			{
				c._numberOfTimes = 1;
				this.EnsureTimerActive( );
			}
		}

		#endregion // DoThrottledInvoke

		#region GetInstance

		public static ThrottleTimer GetInstance( long interval )
		{
			if ( null == g_instances )
				g_instances = new Dictionary<long, ThrottleTimer>( );

			Dictionary<long, ThrottleTimer> instances = g_instances;
			ThrottleTimer timer;
			if ( !instances.TryGetValue( interval, out timer ) )
			{
				timer = new ThrottleTimer( interval );
				instances[interval] = timer;
			}

			return timer;
		}

		#endregion // GetInstance

		#endregion // Public Methods

		#region Private/Internal Methods

		#region CompactStructures

		private void CompactStructures( )
		{
			if ( _callbacksList.Count > 2 * _lastCompactCount )
			{
				_callbacksList.Compact( );
				_callbacks.Compact( true );
				_lastCompactCount = _callbacksList.Count;
			}
		}

		#endregion // CompactStructures

		#region DeactivateTimer

		private void DeactivateTimer( )
		{
			if ( _isTimerActive )
			{
				_isTimerActive = false;
				_timer.Change( Timeout.Infinite, Timeout.Infinite );
			}
		}

		#endregion // DeactivateTimer

		#region EnsureTimerActive

		private void EnsureTimerActive( )
		{
			if ( !_isTimerActive )
			{
				if ( null == _timer )
					_timer = new Timer( new TimerCallback( this.OnTimerTick ), null, _timerInterval, _timerInterval );
				else
					_timer.Change( _timerInterval, _timerInterval );

				_isTimerActive = true;
			}
		}

		#endregion // EnsureTimerActive

		#region OnTimerTick

		private void OnTimerTick( object parameter )
		{
			if (!_inOnTimerTick)
			{

				this.Dispatcher.Invoke(DispatcherPriority.Normal, new PropertyValueTracker.MethodInvoker(this.OnTimerTick_Sync));




			}
		}

		#endregion // OnTimerTick

		#region OnTimerTick_Sync








		private void OnTimerTick_Sync( )
		{
			if ( _inOnTimerTick )
			{
				Debug.WriteLine( "************************** OnTimerTick is getting called recursively." );
				return;
			}

			_inOnTimerTick = true;
			try
			{
				WeakList<RegisteredCallback> list = _callbacksList;
				if ( null != list )
				{
					this.CompactStructures( );

					long startTime = this.CurrentTime;

					for ( int j = 0; j < list.Count; j++ )
					{
						int index = _indecesCounter++ % list.Count;
						RegisteredCallback callback = list[index];

						if ( null != callback && callback._numberOfTimes > 0 )
						{
							Debug.WriteLine( "Processing item at index = " + index );

							long currentTime = this.CurrentTime;

							// SSP 4/13/12 TFS108549 - Optimizations
							// Enclosed the existing code in the if block.
							// 
							if ( currentTime - callback._lastCallTime >= _throttleInterval )
							{
								callback._numberOfTimes--;
								callback._lastCallTime = currentTime;
								IAction action = callback.Action;

								// SSP 3/22/10 TFS27718
								// Only null out the entry when action has been garbage collected.
								// 
								//if ( 0 == callback._numberOfTimes || null == action )
								if ( null == action )
									list[index] = null;

								if ( null != action )
								{
									action.Perform( );

									// SSP 4/13/12 TFS108549 - Optimizations
									// 
									//if ( currentTime - startTime > _timerInterval / 4 )
									if ( currentTime - startTime > _maxTickProcessDuration )
										break;
								}
							}
						}
					}
				}
			}
			finally
			{
				_inOnTimerTick = false;
			}
		}

		#endregion // OnTimerTick_Sync

		#region RegisterCallback

		private RegisteredCallback RegisterCallback( IAction action, int numberOfTimes )
		{
			if ( numberOfTimes < 0 )
				throw new ArgumentOutOfRangeException( );

			RegisteredCallback c;
			bool newlyCreated = false;
			if ( !_callbacks.TryGetValue( action, out c ) )
			{
				c = new RegisteredCallback( action );
				newlyCreated = true;
			}
			
			c._numberOfTimes = numberOfTimes;

			if ( newlyCreated )
			{
				_callbacks[action] = c;
				_callbacksList.Add( c );
				
				if ( c._numberOfTimes > 0 )
					this.EnsureTimerActive( );
			}

			return c;
		}

		#endregion // RegisterCallback

		#region Unregister

		internal void Unregister( IAction action )
		{
			RegisteredCallback c;
			if ( _callbacks.TryGetValue( action, out c ) )
			{
				_callbacks.Remove( action );
				_callbacksList.Remove( c );

				if ( 0 == _callbacks.Count )
					this.DeactivateTimer( );
			}
		}

		#endregion // Unregister

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // ThrottleTimer Class

	#region PropertyValueTracker Class

	/// <summary>
	/// Class used for tracking value of a property (also a property path can be specified) of a source object.
	/// Specified callback is called whenever the property's value changes.
	/// </summary>

	public 



	class PropertyValueTracker : DependencyObject, ThrottleTimer.IAction

	{
		#region Members

		/// <summary>
		/// Callback delegate for 'handler' parameter to the constructor.
		/// </summary>
		public delegate void PropertyValueChangedHandler( );
		
		
		
		//private delegate void MethodInvoker( );
		internal delegate void MethodInvoker( );

		private PropertyValueChangedHandler _handler;
		private bool _callAsynchronously;
		private bool _hasPendingAsynchronousCallback;
		private object _tag;

		private DispatcherPriority _asynchronousDispatcherPriority = DispatcherPriority.Render;


		// SSP 8/2/09 - Summary Recalc Optimizations
		// Added ThrottleTime property.
		// 
		/// <summary>
		/// Throttle time in milliseconds. 0 means no throttling.
		/// </summary>
		private int _throttleTime;
		private ThrottleTimer _cachedThrottleTimer;

		// SSP 4/10/12 TFS108549 - Optimizations
		// 
		private bool _throttleFirstInvoke;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source">Source object that the property path applies to.</param>
		/// <param name="propertyPath">Property path.</param>
		/// <param name="handler">Handler to call whenever the property specified by property path changes.</param>
		public PropertyValueTracker( object source, string propertyPath, PropertyValueChangedHandler handler )
			: this( source, propertyPath, handler, false )
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source">Source object that the property path applies to.</param>
		/// <param name="propertyPath">Property path.</param>
		/// <param name="handler">Handler to call whenever the property specified by property path changes.</param>
		/// <param name="callAsynchronously">Specifies whether to call the callback asynchronously using dispatcher.</param>
		public PropertyValueTracker( object source, string propertyPath, PropertyValueChangedHandler handler, bool callAsynchronously )
		{

			Binding binding = Utilities.CreateBindingObject( propertyPath, BindingMode.OneWay, source );





			this.Initialize( binding, handler, callAsynchronously );
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source">Source object that the property belongs to.</param>
		/// <param name="dependencyProperty">Property whose value to track.</param>
		/// <param name="handler">Handler to call whenever the specified property's value changes.</param>
		public PropertyValueTracker( object source, DependencyProperty dependencyProperty, PropertyValueChangedHandler handler )
			: this( source, dependencyProperty, handler, false )
		{
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source">Source object that the property belongs to.</param>
		/// <param name="dependencyProperty">Property whose value to track.</param>
		/// <param name="handler">Handler to call whenever the specified property's value changes.</param>
		/// <param name="callAsynchronously">Specifies whether to call the callback asynchronously using dispatcher.</param>
		public PropertyValueTracker( object source, DependencyProperty dependencyProperty, PropertyValueChangedHandler handler, bool callAsynchronously )
		{
			Binding binding = Utilities.CreateBindingObject( dependencyProperty, BindingMode.OneWay, source );

			this.Initialize( binding, handler, callAsynchronously );
		}


		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region AsynchronousDispatcherPriority


		/// <summary>
		/// Gets or sets the dispatcher priority to use to call asynchronously if 
		/// <see cref="CallAsynchronously"/> is set to true.
		/// </summary>
		public DispatcherPriority AsynchronousDispatcherPriority
		{
			get
			{
				return _asynchronousDispatcherPriority;
			}
			set
			{
				_asynchronousDispatcherPriority = value;
			}
		}

		#endregion // AsynchronousDispatcherPriority

		#region CallAsynchronously

		/// <summary>
		/// Specifies whether to call the handler asyncrhonously using Dispatcher.BeginInvoke.
		/// </summary>
		public bool CallAsynchronously
		{
			get
			{
				return _callAsynchronously;
			}
			set
			{
				_callAsynchronously = value;
			}
		}

		#endregion // CallAsynchronously

		#region Tag

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		#endregion // Tag

		#region ThrottleTime

		// SSP 8/2/09 - Summary Recalc Optimizations
		// Added ThrottleTime property.
		// 
		/// <summary>
		/// Throttle time in milliseconds. Default is 0 which means there's no throttling.
		/// </summary>
		public int ThrottleTime
		{
			get
			{
				return _throttleTime;
			}
			set
			{
				if ( _throttleTime != value )
				{
					if ( value < 0 )
						throw new ArgumentOutOfRangeException( "Throttle time can't be less than 0." );

					_throttleTime = value;
				}
			}
		}

		#endregion // ThrottleTime

		#region ThrottleFirstInvoke

		// SSP 4/10/12 TFS108549 - Optimizations
		// 
		/// <summary>
		/// If <see cref="ThrottleTime"/> is set to a positive value then <b>ThrottleFirstInvoke</b> property indicates
		/// whether to invoke the callback for the first property value change after the delay specified by 
		/// <i>ThrottleTime</i>, or right away. By default the first callback is invoked right away and successive
		/// callbacks are throttled. Setting this to <i>True</i> will cause the first callback to be delayed by the
		/// throttle time as well as the successive callbacks.
		/// </summary>
		public bool ThrottleFirstInvoke
		{
			get
			{
				return _throttleFirstInvoke;
			}
			set
			{
				_throttleFirstInvoke = value;
			}
		} 

		#endregion // ThrottleFirstInvoke

		#endregion // Public Properties

		#region Private/Internal Properties

		#region Target

		internal static readonly DependencyProperty TargetProperty = DependencyPropertyUtilities.Register("Target",
			typeof(object), typeof(PropertyValueTracker),
			null, new PropertyChangedCallback(OnTargetChanged)
			);

		private static void OnTargetChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			PropertyValueTracker tracker = (PropertyValueTracker)dependencyObject;
			tracker.ProcessTargetChanged( );
		}

		// SSP 9/9/11 - Calc
		// 
		/// <summary>
		/// Gets the value of the target property that's the target of the binding.
		/// </summary>
		internal object Target
		{
			get
			{
				return this.GetValue( TargetProperty );
			}
		}

		#endregion // Target

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		#region Initialize






		private void Initialize( Binding binding, PropertyValueChangedHandler handler, bool callAsynchronously )
		{
			BindingOperations.SetBinding( this, TargetProperty, binding );
			_handler = handler;
			_callAsynchronously = callAsynchronously;



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion // Initialize

		#region DelayedInvokeHanlder

		private void DelayedInvokeHanlder( )
		{
			_hasPendingAsynchronousCallback = false;

			this.InvokeHandler( );
		}

		#endregion // DelayedInvokeHanlder

		#region InvokeHandler

		private void InvokeHandler( )
		{
			if ( null != _handler )
				_handler.Invoke( );
		}

		#endregion // InvokeHandler

		#region ProcessTargetChanged

		private void ProcessTargetChanged( )
		{
			if ( null != _handler )
			{
				// SSP 8/2/09 - Summary Recalc Optimizations
				// Added the if block.
				// 
				if ( _throttleTime > 0 )
				{
					if ( !_hasPendingAsynchronousCallback )
					{
						if ( null == _cachedThrottleTimer )
							_cachedThrottleTimer = ThrottleTimer.GetInstance( _throttleTime );

						_hasPendingAsynchronousCallback = true;

						// SSP 4/10/12 TFS108549 - Optimizations
						// Added ThrottleFirstInvoke property.
						// 
						//_cachedThrottleTimer.DoThrottledInvoke( this );
						_cachedThrottleTimer.DoThrottledInvoke( this, this.ThrottleFirstInvoke );
					}
				}
				else if ( _callAsynchronously )
				{
					if ( !_hasPendingAsynchronousCallback )
					{
						Dispatcher dispatcher = this.Dispatcher;
						if ( null != dispatcher )
						{

							dispatcher.BeginInvoke( this.AsynchronousDispatcherPriority, new MethodInvoker( this.DelayedInvokeHanlder ) );



							_hasPendingAsynchronousCallback = true;
						}
						else
							this.InvokeHandler( );
					}
				}
				else
					this.InvokeHandler( );
			}
		}

		#endregion // ProcessTargetChanged

		#endregion // Private/Internal Methods

		#region Deactivate

		// SSP 4/13/12 TFS108549 - Optimizations
		// 
		/// <summary>
		/// Stops listening to the path.
		/// </summary>
		public void Deactivate( )
		{
			_handler = null;
			this.ClearValue( TargetProperty );

			if ( null != _cachedThrottleTimer )
				_cachedThrottleTimer.Unregister( this );
		}

		#endregion // Deactivate

		#endregion // Methods

		#region IAction Members




		void ThrottleTimer.IAction.Perform( )
		{




			this._hasPendingAsynchronousCallback = false;
			this.InvokeHandler( );
		}

		#endregion
	}

	#endregion // PropertyValueTracker Class

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