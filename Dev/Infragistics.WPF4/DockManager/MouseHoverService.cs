using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Threading;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Static class for raising a MouseHover event for an element.
	/// </summary>
	internal static class MouseHoverService
	{
		#region Constructor
		static MouseHoverService()
		{
		} 
		#endregion //Constructor

		#region Properties

		#region MouseHoverInfo

		/// <summary>
		/// Identifies the <see cref="MouseHoverInfo"/> dependency property
		/// </summary>
		private static readonly DependencyPropertyKey MouseHoverInfoPropertyKey = DependencyProperty.RegisterAttachedReadOnly("MouseHoverInfo",
			typeof(MouseHoverInfo), typeof(MouseHoverService), new FrameworkPropertyMetadata(null));

		private static readonly DependencyProperty MouseHoverInfoProperty = MouseHoverInfoPropertyKey.DependencyProperty;

		#endregion //MouseHoverInfo

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region Register
		/// <summary>
		/// Registers a class to be able to receive <see cref="MouseHoverEvent"/> notifications
		/// </summary>
		/// <param name="elementType">The type of element to process</param>
		public static void Register(Type elementType)
		{
			Debug.Assert(typeof(DependencyObject).IsAssignableFrom(elementType));
			Debug.Assert(typeof(IInputElement).IsAssignableFrom(elementType));

			if (false == typeof(DependencyObject).IsAssignableFrom(elementType))
				throw new InvalidOperationException();

			if (false == typeof(IInputElement).IsAssignableFrom(elementType))
				throw new InvalidOperationException();

			EventManager.RegisterClassHandler(elementType, Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMove), true);
			EventManager.RegisterClassHandler(elementType, Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
			EventManager.RegisterClassHandler(elementType, Mouse.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp), true);
			EventManager.RegisterClassHandler(elementType, Mouse.MouseLeaveEvent, new MouseEventHandler(OnMouseLeave), true);
		}
		#endregion //Register

		#endregion //Public Methods

		#region Private Methods

		#region OnMouseXXX
		private static void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			ProcessMouseEvent(sender, e, true);
		}

		private static void OnMouseLeave(object sender, MouseEventArgs e)
		{
			ProcessMouseEvent(sender, e, true);
		}

		private static void OnMouseMove(object sender, MouseEventArgs e)
		{
			ProcessMouseEvent(sender, e, false);
		}

		private static void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			ProcessMouseEvent(sender, e, false);
		}
		#endregion //OnMouseXXX

		#region ProcessMouseEvent
		private static void ProcessMouseEvent(object sender, MouseEventArgs e, bool forceCancel)
		{
			DependencyObject dep = (DependencyObject)sender;

			// reset the element as a hover cannot occur while the mouse is down
			MouseHoverInfo hoverInfo = (MouseHoverInfo)dep.GetValue(MouseHoverInfoProperty);

			if (forceCancel == false &&
				e.LeftButton == MouseButtonState.Released &&
				e.RightButton == MouseButtonState.Released &&
				e.MiddleButton == MouseButtonState.Released &&
				e.XButton1 == MouseButtonState.Released &&
				e.XButton2 == MouseButtonState.Released)
			{
				// the mouse is not down so either capture 
				if (hoverInfo == null)
				{
					hoverInfo = new MouseHoverInfo((IInputElement)sender);
					dep.SetValue(MouseHoverInfoPropertyKey, hoverInfo);
				}

				hoverInfo.Initialize(e);
			}
			else if (null != hoverInfo)
			{
				hoverInfo.Cancel();
				dep.ClearValue(MouseHoverInfoPropertyKey);
			}
		}
		#endregion //ProcessMouseEvent

		#endregion //Private Methods

		#endregion //Methods

		#region Events

		// FUTURE it might be nice to genericize this for all elements as bubble/tunnel like
		// other mouse events but we want to be careful not to add overhead
		//public static readonly RoutedEvent MouseHoverEvent = EventManager.RegisterRoutedEvent("MouseHover", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(MouseHoverService));
		//public static readonly RoutedEvent PreviewMouseHoverEvent = EventManager.RegisterRoutedEvent("PreviewMouseHover", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(MouseHoverService));
		public static readonly RoutedEvent MouseHoverEvent = EventManager.RegisterRoutedEvent("MouseHover", RoutingStrategy.Direct, typeof(MouseEventHandler), typeof(MouseHoverService));

		#endregion //Events

		#region MouseHoverInfo class
		private class MouseHoverInfo
		{
			#region Member Variables

			private IInputElement _element;
			private Point? _lastMousePoint;
			private DispatcherTimer _timer;

			#endregion //Member Variables

			#region Constructor
			internal MouseHoverInfo(IInputElement element)
			{
				DockManagerUtilities.ThrowIfNull(element, "element");

				this._element = element;
			}
			#endregion //Constructor

			#region Methods

			#region Internal Methods

			#region Cancel
			internal void Cancel()
			{
				this._lastMousePoint = null;
				this.StopTimer();
			}
			#endregion //Cancel

			#region InitializeTimer
			internal void Initialize(MouseEventArgs e)
			{
				Point pt = e.GetPosition(this._element);

				// if we had a point see if we went outside the distance
				if (this._lastMousePoint != null)
				{
					Size size = new Size(SystemParameters.MouseHoverWidth, SystemParameters.MouseHoverHeight);
					Rect rect = new Rect(this._lastMousePoint.Value, size);
					rect.Offset(-size.Width / 2, -size.Height / 2);

					// if we're within the hover threshold ignore the point
					if (rect.Contains(pt))
						return;
				}

				// if we're here then we need to restart the timer and cache
				// the new point
				this._lastMousePoint = pt;
				this.StartTimer();
			}
			#endregion //InitializeTimer

			#endregion //Internal Methods

			#region Private Methods

			#region OnTimerTick
			private void OnTimerTick(object sender, EventArgs e)
			{
				this.Cancel();

				// AS 6/4/08 BR33653
				// InputManager.Current requires permissions.
				//
				//MouseEventArgs mouseArgs = new MouseEventArgs(InputManager.Current.PrimaryMouseDevice, Environment.TickCount);
				MouseEventArgs mouseArgs = new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount);
				mouseArgs.RoutedEvent = MouseHoverService.MouseHoverEvent;
				this._element.RaiseEvent(mouseArgs);
			}
			#endregion //OnTimerTick

			#region StartTimer
			private void StartTimer()
			{
				if (null == this._timer)
				{
					this._timer = new DispatcherTimer(DispatcherPriority.Input);
					this._timer.Tick += new EventHandler(OnTimerTick);
				}
				else
					this._timer.Stop();

				this._timer.Interval = SystemParameters.MouseHoverTime;
				this._timer.Start();
			}
			#endregion //StartTimer

			#region StopTimer
			private void StopTimer()
			{
				if (null != this._timer)
					this._timer.Stop();
			}
			#endregion //StopTimer

			#endregion //Private Methods

			#endregion //Methods
		} 
		#endregion //MouseHoverInfo class
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