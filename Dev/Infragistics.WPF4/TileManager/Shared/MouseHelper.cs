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
	internal class MouseHelper
	{
		#region Member Variables

		internal const double DoubleClickSizeX = 4;
		internal const double DoubleClickSizeY = 4;
		internal const long DoubleClickTime = 500 * TimeSpan.TicksPerMillisecond;

		// AS 3/13/12
		// Using the default values stored in the registry under 
 		// HKEY_CURRENT_USER\Software\Microsoft\Wisp\Pen\SysEventParameters for the 
		// DblTime and DblDist.
		//
		internal const double DoubleTapSize = 15;
		internal const long DoubleTapTime = 800 * TimeSpan.TicksPerMillisecond;

		// AS 3/7/12
		// When the touch manipulations are promoted they don't seem to initialize the 
		// click count so it's always 1. Therefore we need to calculate if it is a 
		// double click. Also since SL5 added a ClickCount we can remove this for SL.
		//
		private long _lastClickTicks;
		private Point _lastClickPos;

		private UIElement _element;
		private int _clickCount;

		#endregion // Member Variables

		#region Constructor
		internal MouseHelper(UIElement element)
		{
			_element = element;
		}
		#endregion // Constructor

		#region Properties

		#region ClickCount
		public int ClickCount
		{
			get { return _clickCount; }
		}
		#endregion // ClickCount

		#endregion // Properties

		#region Methods

		#region OnMouseLeftButtonDown
		public bool OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{

			// AS 3/7/12
			// See notes in member variables
			//
			if (e.StylusDevice == null || e.ClickCount != 1
				// AS 4/6/12 TFS108390



				)
			{
				_clickCount = e.ClickCount;
				return true;
			}

			long nowTicks = DateTime.Now.Ticks;
			long delta = nowTicks - _lastClickTicks;
			bool incrementClick = false;
			Point pos = e.GetPosition(_element);

			if (delta <= DoubleTapTime)
			{
				if (Math.Abs(pos.X - _lastClickPos.X) <= DoubleTapSize &&
					Math.Abs(pos.Y - _lastClickPos.Y) <= DoubleTapSize)
				{
					incrementClick = true;
				}
			}

			if (incrementClick)
				_clickCount++;
			else
				_clickCount = 1;

			if (_clickCount == 1)
			{
				_lastClickPos = pos;
				_lastClickTicks = nowTicks;
			}

			return true;
		}
		#endregion // OnMouseLeftButtonDown

		#endregion // Methods
	}

	/// <summary>
	/// Helper class for invoking an action on the left mouse up.
	/// </summary>
	internal class ClickHelper
	{
		#region Member Variables

		[ThreadStatic]
		private static ClickHelper _instance;

		private FrameworkElement _element;
		private Point _mouseDownPoint;
		private bool _leftDoubleClickRange;
		private bool _onlyWithinDoubleClickRange;
		private Action _clickAction;

		#endregion // Member Variables

		#region Constructor
		private ClickHelper()
		{
		}
		#endregion // Constructor

		#region Properties
		private static ClickHelper Instance
		{
			get
			{
				if (_instance == null)
					_instance = new ClickHelper();

				return _instance;
			}
		}
		#endregion // Properties

		#region Static Methods
		internal static void OnMouseLeftButtonDown(FrameworkElement element, MouseButtonEventArgs e, Action clickAction, bool onlyWithinDoubleClickRange)
		{
			Instance.Initialize(element, e, clickAction, onlyWithinDoubleClickRange);
		}

		internal static void CancelCurrent()
		{
			if (_instance != null)
				_instance.End(true);
		}
		#endregion // Static Methods

		#region Instance Methods

		#region HookElement
		private void HookElement(FrameworkElement element, bool hook)
		{
			if (hook)
			{
				element.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseLeftButtonUp);
				element.LostMouseCapture += new MouseEventHandler(this.OnLostMouseCapture);

				element.MouseRightButtonDown += new MouseButtonEventHandler(this.OnMouseRightButtonDown);

                element.MouseMove += new MouseEventHandler(this.OnMouseMove);
			}
			else
			{
				element.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnMouseLeftButtonUp);

				element.MouseRightButtonDown -= new MouseButtonEventHandler(this.OnMouseRightButtonDown);

                element.LostMouseCapture -= new MouseEventHandler(this.OnLostMouseCapture);
				element.MouseMove -= new MouseEventHandler(this.OnMouseMove);
			}
		}
		#endregion // HookElement

		#region Initialize
		private void Initialize(FrameworkElement element, MouseButtonEventArgs e, Action clickAction, bool onlyWithinDoubleClickRange)
		{
			this.End(true);

			Debug.Assert(null == _element);

			try
			{
				_mouseDownPoint = e.GetPosition(element);
			}
			catch
			{
				return;
			}

			_element = element;
			_leftDoubleClickRange = false;
			_onlyWithinDoubleClickRange = onlyWithinDoubleClickRange;
			_clickAction = clickAction;

			// hook first in case we get capture (in which case capturemouse will return true) 
			// but we lose capture before it returns
			this.HookElement(element, true);

			// try to capture the mouse. if that failed then unhook
			if (!element.CaptureMouse())
				this.HookElement(element, false);

			if (_element == element)
				e.Handled = true;
		}
		#endregion // Initialize

		#region End
		private void End(bool cancel)
		{
			if (_element == null)
				return;

			var element = _element;
			Action clickAction;

			if (cancel || (_onlyWithinDoubleClickRange && _leftDoubleClickRange))
				clickAction = null;
			else
				clickAction = _clickAction;

			// clear out the members before we perform any operation
			_element = null;
			_clickAction = null;

			HookElement(element, false);
			element.ReleaseMouseCapture();

			if (null != clickAction)
				clickAction();
		}
		#endregion // End

		#region OnLostMouseCapture
		private void OnLostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
		{
			this.End(true);
		}
		#endregion // OnLostMouseCapture

		#region OnMouseLeftButtonUp
		private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			bool cancel = true;
			var element = _element;

			try
			{
				Point pt = e.GetPosition(_element);

				Rect rect = new Rect(new Point(), element.RenderSize);

				if (rect.Contains(pt))
					cancel = false;
			}
			catch
			{
			}


			this.End(cancel);
		}
		#endregion // OnMouseLeftButtonUp

		#region OnMouseMove
		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (_onlyWithinDoubleClickRange && !_leftDoubleClickRange)
			{
				try
				{
					Point pt = e.GetPosition(_element);

					if (Math.Abs(pt.X - _mouseDownPoint.X) > MouseHelper.DoubleClickSizeX ||
						Math.Abs(pt.Y - _mouseDownPoint.Y) > MouseHelper.DoubleClickSizeY)
					{
						_leftDoubleClickRange = true;
					}

				}
				catch
				{
				}
			}
		}
		#endregion // OnMouseMove

		#region OnMouseRightButtonDown
		private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.End(true);
		}
		#endregion // OnMouseRightButtonDown

		#endregion // Instance Methods
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