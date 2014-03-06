using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Windows.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using Microsoft.Win32;
using Infragistics.Collections;

namespace Infragistics.Windows.Editors
{
	// AS 3/23/10 TFS26461 - added
	// I had written a version of this class for a customer when the original issue about the 
	// today button not being updated was reported. However another issue about the IsToday/ContainsToday
	// not being updated was reported and it was not possible to workaround that externally (without 
	// possibly scrolling the items to force them to be reinitialized) so I incorporated this class 
	// as an internal class and used it within the xamMonthCalendar. This class tries to hook the system's 
	// timechanged to handle the case where the user changes the date manually and based upon whether 
	// we could do that we also create a dispatchertimer either for the difference between the current 
	// time and the end of the day or a small interval in the case where we could not hook the event. 
	// In this way we can avoid having a short interval timer unless we need to.
	//
	/// <summary>
	/// Custom object for providing the current date.
	/// </summary>
	internal sealed class CurrentDate : DependencyObject
	{
		#region Member Variables

		private static WeakList<CurrentDate> _instances;
		private static object _currentDate;
		private static readonly object Lock = new object();
		private static DispatcherTimer _timer;
		private static bool _canHookSystemTime = true;

		#endregion //Member Variables

		#region Constructor
		static CurrentDate()
		{
			_currentDate = DateTime.Today;
			_instances = new WeakList<CurrentDate>();
		}

		/// <summary>
		/// Initializes a new <see cref="CurrentDate"/>
		/// </summary>
		public CurrentDate()
		{
			AddInstance(this);
		}
		#endregion //Constructor

		#region Properties

		#region Value

		private static readonly DependencyPropertyKey ValuePropertyKey =
			DependencyProperty.RegisterReadOnly("Value",
			typeof(DateTime), typeof(CurrentDate), new FrameworkPropertyMetadata(DateTime.Today, new PropertyChangedCallback(OnValueChanged)));

		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CurrentDate item = d as CurrentDate;

			EventHandler handler = item.ValueChanged;

			if (handler != null)
				handler(item, EventArgs.Empty);
		}

		/// <summary>
		/// Identifies the <see cref="Value"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ValueProperty =
			ValuePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a DateTime that represents the current day.
		/// </summary>
		/// <seealso cref="ValueProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public DateTime Value
		{
			get
			{
				return (DateTime)this.GetValue(CurrentDate.ValueProperty);
			}
		}

		#endregion //Value

		#endregion //Properties

		#region Methods

		#region AddInstance
		private static void AddInstance(CurrentDate item)
		{
			lock (Lock)
			{
				_instances.Add(item);
				item.SetValue(ValuePropertyKey, _currentDate);

				if (_instances.Count == 1)
				{
					StartDateWatch(item.Dispatcher);
				}
				else if (_instances.Count % 20 == 0)
				{
					Debug.Assert(null != _timer);

					_instances.Compact();

					// if everything is gone we can end the timer
					CurrentDate firstItem = _instances[0];

					if (firstItem == null)
						EndDateWatch();
					else if (firstItem.Dispatcher != _timer.Dispatcher)
					{
						// restart the timer with the new dispatcher
						EndDateWatch();
						StartDateWatch(firstItem.Dispatcher);
					}
				}
			}
		}

		#endregion //AddInstance

		#region CheckForDateChange
		private static void CheckForDateChange()
		{
			DateTime date = DateTime.Today;
			DateTime current = (DateTime)_currentDate;

			if (date != current)
			{
				lock (Lock)
				{
					// update the singleton boxed date value
					_currentDate = date;

					// update the members still alive
					foreach (CurrentDate item in _instances)
					{
						// AS 8/16/10 TFS36762
						// This isn't directly related to the bug but we had a potential
						// hole if there were instances of the controls on different threads.
						//
						//item.SetValue(ValuePropertyKey, _currentDate);
						if (item.CheckAccess())
							item.SetValue(ValuePropertyKey, _currentDate);
						else
							item.Dispatcher.BeginInvoke(DispatcherPriority.Send, new System.Threading.SendOrPostCallback(UpdateCurrentDate), item);
					}

					// collapse for any empty entries
					_instances.Compact();

					if (_instances.Count == 0)
						EndDateWatch();
				}
			}
		}
		#endregion //CheckForDateChange

		#region EndDateWatch
		private static void EndDateWatch()
		{
			// end the dispatch timer
			if (null != _timer)
			{
				DispatcherTimer timer = _timer;
				_timer = null;
				timer.Stop();

				UnhookSystemTimeChange();
			}
		}
		#endregion //EndDateWatch

		#region HookSystemTimeChange
		private static bool HookSystemTimeChange()
		{
			if (_canHookSystemTime)
			{
				try
				{
					HookSystemTimeChangeImpl();
					return true;
				}
				catch (SecurityException)
				{
					_canHookSystemTime = false;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void HookSystemTimeChangeImpl()
		{
			SystemEvents.TimeChanged += new EventHandler(OnSystemTimeChanged);
		}
		#endregion //HookSystemTimeChange

		#region OnSystemTimeChanged
		private static void OnSystemTimeChanged(object sender, EventArgs e)
		{
			CheckForDateChange();
		}
		#endregion //OnSystemTimeChanged

		#region OnTimerTick
		private static void OnTimerTick(object sender, EventArgs e)
		{
			if (sender == _timer)
				CheckForDateChange();
		}
		#endregion //OnTimerTick

		#region StartDateWatch
		private static void StartDateWatch(Dispatcher dispatcher)
		{
			Debug.Assert(_timer == null);
			bool useLongDelay = HookSystemTimeChange();

			TimeSpan delay;
			
			if (!useLongDelay)
				delay = TimeSpan.FromMinutes(.2);
			else
				delay = DateTime.Today.AddDays(1d).Subtract(DateTime.Now.Subtract(TimeSpan.FromMinutes(.5)));

			// AS 8/16/10 TFS36762
			// This too wasn't directly related to the bug but if we can use 
			// the systemtime changed event then we don't need the short timer
			// interval.
			//
			//_timer = new DispatcherTimer(TimeSpan.FromMinutes(.2), DispatcherPriority.ContextIdle, new EventHandler(OnTimerTick), dispatcher);
			_timer = new DispatcherTimer(delay, DispatcherPriority.ContextIdle, new EventHandler(OnTimerTick), dispatcher);
		}
		#endregion //StartDateWatch

		#region UnhookSystemTimeChange
		private static void UnhookSystemTimeChange()
		{
			if (_canHookSystemTime)
			{
				try
				{
					UnhookSystemTimeChangeImpl();
				}
				catch (SecurityException)
				{
					_canHookSystemTime = false;
				}
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void UnhookSystemTimeChangeImpl()
		{
			SystemEvents.TimeChanged -= new EventHandler(OnSystemTimeChanged);
		} 
		#endregion //UnhookSystemTimeChange

		// AS 8/16/10 TFS36762
		#region UpdateCurrentDate
		private static void UpdateCurrentDate(object param)
		{
			CurrentDate instance = param as CurrentDate;

			if (null != instance)
				instance.SetValue(ValuePropertyKey, _currentDate);
		}
		#endregion //UpdateCurrentDate

		#endregion //Methods

		#region Events

		/// <summary>
		/// Invoked when the Value has changed.
		/// </summary>
		public EventHandler ValueChanged;

		#endregion //Events

		#region Delegate

		private delegate void MethodInvoker();

		#endregion //Delegate
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