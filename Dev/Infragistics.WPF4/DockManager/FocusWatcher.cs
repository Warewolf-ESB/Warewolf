using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Threading;
using Infragistics.Windows.Helpers;
using Infragistics.Collections;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Helper class for watching the element with keyboard focus.
	/// </summary>
	internal class FocusWatcher : IDisposable
	{
		#region Member Variables

		private IInputElement _focusedElement;
		// AS 5/8/09 TFS17021
		// Rather than using a BeginInvoke to periodically check if focus is restored within 
		// the application we will use a handler for the GotKeyboardFocus event for the types 
		// that implement IInputElement and have the watcher register (via a weak list) to get
		// the notification when something gets focus.
		//
		//private DispatcherOperation _focusCheckOperation;
		private static object GlobalLock = new object();
		private static WeakList<FocusWatcher> _gotFocusList;

        // AS 1/28/09 TFS12456
        private bool _isDisposed;

		#endregion //Member Variables

		#region Constructor
		static FocusWatcher()
		{
			// AS 5/8/09 TFS17021
			_gotFocusList = new WeakList<FocusWatcher>();

			KeyboardFocusChangedEventHandler handler = new KeyboardFocusChangedEventHandler(OnGlobalGotFocus);

			EventManager.RegisterClassHandler(typeof(UIElement), Keyboard.GotKeyboardFocusEvent, handler, true);
			EventManager.RegisterClassHandler(typeof(ContentElement), Keyboard.GotKeyboardFocusEvent, handler, true);

			// conditionally deal with UIElement3D since it didn't exist in 3.0
			Type element3d = Type.GetType("System.Windows.UIElement3D, " + typeof(UIElement).Assembly.FullName, false);

			if (element3d != null)
				EventManager.RegisterClassHandler(element3d, Keyboard.GotKeyboardFocusEvent, handler, true);
		}

		internal FocusWatcher()
		{
			this.HookElementImpl(Keyboard.FocusedElement);
		} 
		#endregion //Constructor

		#region Properties
		public IInputElement FocusedElement
		{
			get { return this._focusedElement; }
		}
		#endregion //Properties

		#region Methods

		#region CheckFocusedElement
		private void CheckFocusedElement()
		{
			// AS 5/8/09 TFS17021
			//this._focusCheckOperation = null;
			UnregisterGlobalGotFocus(this);

			this.HookElementImpl(Keyboard.FocusedElement);
		}
		#endregion //CheckFocusedElement

		#region HookElement
		private void HookElementImpl(IInputElement element)
		{
			this.UnhookElementImpl();

            // AS 1/28/09 TFS12456
            if (_isDisposed)
                return;

			if (null != element)
			{
				// AS 5/8/09 TFS17021
				//if (null != this._focusCheckOperation && this._focusCheckOperation.Status == DispatcherOperationStatus.Pending)
				//	this._focusCheckOperation.Abort();
				//
				//this._focusCheckOperation = null;
				UnregisterGlobalGotFocus(this);

				this._focusedElement = element;
				this._focusedElement.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(OnFocusedElementLostFocus);
				this.HookElement(element);
			}
			else
			{
				// AS 5/8/09 TFS17021
				//if (null == this._focusCheckOperation)
				//	this._focusCheckOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Infragistics.Windows.DockManager.DockManagerUtilities.MethodInvoker(CheckFocusedElement));
				RegisterGlobalGotFocus(this);
			}
		}

		/// <summary>
		/// Invoked to allow a derived class to hook events on the new focused element.
		/// </summary>
		/// <param name="element">The new element</param>
		protected virtual void HookElement(IInputElement element)
		{
		}
		#endregion //HookElement

		#region OnFocusElementChanged
		/// <summary>
		/// Invoked when the focused element has changed.
		/// </summary>
		/// <param name="oldElement">The old element</param>
		/// <param name="newElement">The new element</param>
		protected virtual void OnFocusElementChanged(IInputElement oldElement, IInputElement newElement)
		{
			if (null != this.FocusedElementChanged)
			{
				// AS 8/12/09
				// We should have been passing in the new element as well.
				//this.FocusedElementChanged(this, new RoutedPropertyChangedEventArgs<IInputElement>(oldElement, null));
				this.FocusedElementChanged(this, new RoutedPropertyChangedEventArgs<IInputElement>(oldElement, newElement));
			}
		} 
		#endregion //OnFocusElementChanged

		#region OnFocusedElementLostFocus
		private void OnFocusedElementLostFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			// AS 8/12/09
			// The focused element may have changed so just use our cached element as the old focus
			// so we can properly unhook from its events.
			//
			//Debug.Assert(this._focusedElement == e.OldFocus);
			IInputElement oldFocus = _focusedElement;

			this.HookElementImpl(e.NewFocus);

			// AS 8/12/09
			//this.OnFocusElementChanged(e.OldFocus, e.NewFocus);
			this.OnFocusElementChanged(oldFocus, e.NewFocus);
		}
		#endregion //OnFocusedElementLostFocus

		// AS 5/8/09 TFS17021
		#region OnGlobalGotFocus
		private static void OnGlobalGotFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (e.OriginalSource == sender && e.OldFocus == null && e.NewFocus != null)
				ProcessGlobalGotFocus(e.NewFocus);
		}
		#endregion //OnGlobalGotFocus

		// AS 5/8/09 TFS17021
		#region ProcessGlobalGotFocus
		private static void ProcessGlobalGotFocus(IInputElement newFocus)
		{
			lock (GlobalLock)
			{
				int count = _gotFocusList.Count;

				if (count > 0)
				{
					FocusWatcher[] watchers = new FocusWatcher[count];
					_gotFocusList.CopyTo(watchers, 0);
					_gotFocusList.Clear();

					foreach (FocusWatcher watcher in watchers)
					{
						if (null != watcher && !watcher._isDisposed)
							watcher.HookElement(newFocus);
					}
				}
			}
		}
		#endregion //ProcessGlobalGotFocus

		// AS 5/8/09 TFS17021
		#region RegisterGlobalGotFocus
		private static void RegisterGlobalGotFocus(FocusWatcher watcher)
		{
			lock (GlobalLock)
			{
				Debug.Assert(!_gotFocusList.Contains(watcher));
				_gotFocusList.Add(watcher);
			}
		}
		#endregion //RegisterGlobalGotFocus

		#region UnhookElement
		private void UnhookElementImpl()
		{
			if (null != this._focusedElement)
			{
				IInputElement oldElement = this._focusedElement;
				this._focusedElement = null;
				oldElement.LostKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnFocusedElementLostFocus);
				this.UnhookElement(oldElement);
			}
		}

		/// <summary>
		/// Invoked to allow a derived class to hook events on the new focused element.
		/// </summary>
		/// <param name="element">The new element</param>
		protected virtual void UnhookElement(IInputElement element)
		{
		}
		#endregion //UnhookElement

		// AS 5/8/09 TFS17021
		#region UnregisterGlobalGotFocus
		private static void UnregisterGlobalGotFocus(FocusWatcher watcher)
		{
			lock (GlobalLock)
			{
				_gotFocusList.Remove(watcher);
			}
		}
		#endregion //UnregisterGlobalGotFocus

		#endregion //Methods

		#region Events

		public event RoutedPropertyChangedEventHandler<IInputElement> FocusedElementChanged;

		#endregion //Events

		#region IDisposable Members

		public void Dispose()
		{
            // AS 1/28/09 TFS12456
            // Just in case we'll keep a flag to know we're disposed but the main fix
            // was to move the abort call out of the if block below.
            //
            _isDisposed = true;

			// AS 5/8/09 TFS17021
			//if (null != this._focusCheckOperation)
            //    this._focusCheckOperation.Abort();
			UnregisterGlobalGotFocus(this);

            if (this._focusedElement != null)
			{
                // AS 1/28/09 TFS12456
                // The dispatcheroperation will only be non-null if we don't have a focused element so 
                // we need to abort it outside the _focusedElement check.
                //
				//if (null != this._focusCheckOperation)
				//	this._focusCheckOperation.Abort();

				IInputElement oldElement = this._focusedElement;

				// AS 6/8/11 TFS78439
				// We should be calling the UnhookElementImpl otherwise the derived class(es) wouldn't be able to unhook.
				//
				//this._focusedElement.LostKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnFocusedElementLostFocus);
				//this._focusedElement = null;
				this.UnhookElementImpl();

				this.OnFocusElementChanged(oldElement, null);
			}
		}

		#endregion
	}

	internal class KeyEventFocusWatcher : FocusWatcher
	{
		#region Constructor
		internal KeyEventFocusWatcher()
		{
		} 
		#endregion //Constructor

		#region Base class overrides
		protected override void HookElement(IInputElement element)
		{
			base.HookElement(element);

			KeyEventHandler keyHandler = new KeyEventHandler(OnElementKeyEvent);
			element.PreviewKeyDown += keyHandler;
			element.PreviewKeyUp += keyHandler;
			element.KeyDown += keyHandler;
			element.KeyUp += keyHandler;
		}

		protected override void UnhookElement(IInputElement element)
		{
			base.UnhookElement(element);

			KeyEventHandler keyHandler = new KeyEventHandler(OnElementKeyEvent);
			element.PreviewKeyDown -= keyHandler;
			element.PreviewKeyUp -= keyHandler;
			element.KeyDown -= keyHandler;
			element.KeyUp -= keyHandler;
		} 
		#endregion //Base class overrides

		#region Methods
		void OnElementKeyEvent(object sender, KeyEventArgs e)
		{
			if (null != this.KeyEvent)
				this.KeyEvent(this, e);
		} 
		#endregion //Methods

		#region Events

		public event KeyEventHandler KeyEvent;

		#endregion //Events	
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