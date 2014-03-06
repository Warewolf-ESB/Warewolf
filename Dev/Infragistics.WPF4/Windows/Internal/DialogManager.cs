using System;
using System.Linq;
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
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Interop;
using System.Runtime.CompilerServices;




using Infragistics.Windows.Controls;


namespace Infragistics.Controls
{

	/// <summary>
	/// A helper class for displaying dialogs that abstracts away the differences between displaying dialogs in Silverlight and WPF.
	/// For Silverlight the class uses <b>XamDialogWindow</b> and for WPF the class uses Infragistics.Windows.Controls.ToolWindow/>.
	/// </summary>






	internal static class DialogManager
	{
		#region Methods

		#region ActivateDialog
		internal static void ActivateDialog(FrameworkElement dialog)
		{


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

			if (dialog is Window)
				((Window)dialog).Activate();
			else
			if (dialog is Infragistics.Windows.Controls.ToolWindow)
				((Infragistics.Windows.Controls.ToolWindow)dialog).Activate();

		}
		#endregion //ActivateDialog

		// AS 10/27/10 TFS36504
		// Refactored the close into a helper method.
		//
		#region CloseDialog
		internal static void CloseDialog( FrameworkElement dialogElement )
		{






			Window window = dialogElement as Window;

			if ( window != null )
			{
				window.Close();
			}
			else
			{
				var toolWindow = dialogElement as Infragistics.Windows.Controls.ToolWindow;

				if ( null != toolWindow )
					toolWindow.Close();
			}

		}

		#endregion // CloseDialog

		#region DisplayDialog

		// JM 04-12-11 TFS69396 Added an overload that takes a resizeSilverlightWindowToFitInBrowser parameter



#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

		internal static FrameworkElement DisplayDialog(FrameworkElement container, FrameworkElement dialogContents, Size dialogSize, bool resizable, object header, bool showModally, ResourceDictionary resources, Func<bool> closingCallback, Action<bool?> closedCallback)
		{
			return DisplayDialog(container, dialogContents, dialogSize, resizable, header, showModally, resources, closingCallback, closedCallback, false);
		}



#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

		internal static FrameworkElement DisplayDialog( FrameworkElement container, FrameworkElement dialogContents, Size dialogSize, bool resizable, object header, bool showModally, ResourceDictionary resources, Func<bool> closingCallback, Action<bool?> closedCallback, bool resizeSilverlightWindowToFitInBrowser)
		{


#region Infragistics Source Cleanup (Region)



























































































































#endregion // Infragistics Source Cleanup (Region)




			Window owningWindow = container as Window;
			if (owningWindow == null &&  false == System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted)
				owningWindow = PresentationUtilities.GetVisualAncestor<Window>(container, null);

			if (dialogContents is Window)
			{
				Window window					= dialogContents as Window;
				window.Title					= header == null ? string.Empty : header.ToString();
				window.ResizeMode				= resizable ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;
				window.Owner					= owningWindow;

				// AS 6/8/11 TFS73965
				// If we're given a window and we don't have an owningWindow then try to use the owning hwnd.
				//
				if (owningWindow == null)
				{
					IntPtr ownerHwnd = GetOwnerHwnd(container);

					if (IntPtr.Zero != ownerHwnd)
						SetWindowOwner(dialogContents as Window, ownerHwnd);
				}

				// JM 02-25-11 TFS 67024
				if (false == resizable)
					window.WindowStyle			= WindowStyle.ToolWindow;

				// JM 03-24-11 TFS68833
				Infragistics.Windows.Utilities.EnableModelessKeyboardInteropForWindow(window);

				// AS 11/2/10 TFS49402/TFS49912/TFS51985
				// To try and be more consistent with outlook we should only center dialogs.
				//
				if ( showModally )
					window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

				if (dialogSize != Size.Empty)
				{
					dialogContents.Width		= dialogSize.Width;
					dialogContents.Height		= dialogSize.Height;
				}
				else
					window.SizeToContent		= SizeToContent.WidthAndHeight;

				if (resources != null)
					dialogContents.Resources.MergedDictionaries.Add(resources);

				// Focus the first element
				window.Loaded += delegate(object sender, RoutedEventArgs e)
				{
					DispatcherOperationCallback callback = delegate(object param)
					{
						((Window)param).MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
						return null;
					};

					// Loaded is too early so delay setting focus to the first element.
					((Window)sender).Dispatcher.BeginInvoke(DispatcherPriority.Loaded, callback, sender);
				};

				// If the caller has specified callbacks, listen to WindowStateChanged and/or WindowStateChanging and call the callbacks.
				// AS 10/27/10 TFS36504
				// Always hook the closing so we can clear the owner before the window is closed 
				// since there is an os/framework bug where the owner will get deactivated when the 
				// owned window is closed if a modal window was shown while the owned window was 
				// displayed. 
				//
				window.Closing += delegate( object sender, CancelEventArgs e )
				{
					Window w = sender as Window;

					// JM 11-29-10 TFS60642 - Should be checking for closingCallback == null
					//if (null != closedCallback)
					if (null != closingCallback)
					{
						bool cancel = closingCallback();
						if ( cancel )
							e.Cancel = true;
					}

					if ( !showModally && e.Cancel == false )
						w.Owner = null;
				};

				if (closedCallback != null)
				{
					window.Closed += delegate(object sender, EventArgs e)
					{
						closedCallback(window.DialogResult);
					};
				}

				if (showModally)
					((Window)dialogContents).ShowDialog();
				else
					((Window)dialogContents).Show();

				return dialogContents;
			}
			else
			{
				Infragistics.Windows.Controls.ToolWindow toolWindow 
										= new Infragistics.Windows.Controls.ToolWindow();
				toolWindow.Content		= dialogContents;
				toolWindow.ResizeMode	= resizable ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;
				toolWindow.Title		= header == null ? string.Empty : header.ToString();
				if (dialogSize != Size.Empty)
				{
					toolWindow.Width	= dialogSize.Width;
					toolWindow.Height	= dialogSize.Height;
				}

				// JM 02-25-11 TFS 67024
				if (false == resizable)
					toolWindow.AllowMaximize = toolWindow.AllowMinimize = false;

				
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

				if ( showModally )
				{
					toolWindow.WindowStartupLocation = Infragistics.Windows.Controls.ToolWindowStartupLocation.CenterOwnerWindow;
				}

				if (resources != null)
					toolWindow.Resources.MergedDictionaries.Add(resources);

				// Focus the first element
				toolWindow.Loaded += delegate(object sender, RoutedEventArgs e)
				{
					DispatcherOperationCallback callback = delegate(object param)
					{
						((Infragistics.Windows.Controls.ToolWindow)param).MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
						return null;
					};

					// Loaded is too early so delay setting focus to the first element.
					((Infragistics.Windows.Controls.ToolWindow)sender).Dispatcher.BeginInvoke(DispatcherPriority.Loaded, callback, sender);
				};

				// If the caller has specified callbacks, listen to WindowStateChanged and/or WindowStateChanging and call the callbacks.
				if (closingCallback != null)
				{
					toolWindow.Closing += delegate(object sender, CancelEventArgs e)
					{
						bool cancel = closingCallback();
						if (cancel)
							e.Cancel = true;
					};
				}

				if (closedCallback != null)
				{
					toolWindow.Closed += delegate(object sender, EventArgs e)
					{
						closedCallback(toolWindow.DialogResult);
					};
				}

				FrameworkElement owner = container ?? owningWindow;

				if (showModally)
					toolWindow.ShowDialog(owner, null);
				else
					toolWindow.Show(owner);

				return toolWindow;
			}

		}
		#endregion //DisplayDialog

		// AS 6/8/11 TFS73965
		#region GetOwnerHwnd

		private static bool _getOwnerHwndFailed = false;

		internal static IntPtr GetOwnerHwnd(FrameworkElement owner)
		{
			if (_getOwnerHwndFailed)
				return IntPtr.Zero;

			// don't try to use a wpf window in an xbap
			if (BrowserInteropHelper.IsBrowserHosted)
				return IntPtr.Zero;

			try
			{
				return GetOwnerHwndImpl(owner);
			}
			catch (System.Security.SecurityException)
			{
				_getOwnerHwndFailed = true;
				return IntPtr.Zero;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static IntPtr GetOwnerHwndImpl(FrameworkElement owner)
		{
			var hs = HwndSource.FromVisual(owner) as HwndSource;

			if (hs == null)
				return IntPtr.Zero;

			return hs.Handle;
		}

		#endregion //GetOwnerHwnd

		#region ResolveRootPanel


#region Infragistics Source Cleanup (Region)














































#endregion // Infragistics Source Cleanup (Region)

		#endregion // ResolveRootPanel

		// AS 6/8/11 TFS73965
		#region SetWindowOwner

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void SetWindowOwner(Window window, IntPtr ownerHwnd)
		{
			WindowInteropHelper wih = new WindowInteropHelper(window);
			wih.Owner = ownerHwnd;
		}

		#endregion //SetWindowOwner

		#endregion //Methods

		#region CloseDialogHelper class
		internal class CloseDialogHelper<T>
		{
			private T _context;
			private Action<bool?, T> _closeAction;

			internal CloseDialogHelper(Action<bool?, T> closeAction, T closeContext)
			{
				_context = closeContext;
				_closeAction = closeAction;
			}

			public void OnClosed(bool? dialogResult)
			{
				_closeAction(dialogResult, _context);
			}
		}
		#endregion // CloseDialogHelper class
	}

	#region Internal DialogElementProxy Class


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

	internal class DialogElementProxy :
		IDisposable // MD 8/10/11
	{
		#region Member Variables

		private bool _isInitialized;
		private FrameworkElement _childElement;
		private FrameworkElement _dialogElement;

		#endregion //Member Variables

		#region Constructor


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal DialogElementProxy(FrameworkElement childElement)
		{
			CoreUtilities.ValidateNotNull(childElement, "childElement");
			if (false == (childElement is IDialogElementProxyHost))
			{
#pragma warning disable 436
				throw new ArgumentException(SR.GetString("LE_DoesNotImplementInterface", "IDialogElementProxyHost"), "childElement"); // "childElement does not implement IDialogElementProxyHost!"
#pragma warning restore 436
			}

			this._childElement = childElement;
		}
		#endregion //Constructor

		#region Interfaces

		#region IDisposable Members

		public void Dispose()
		{
			if (_childElement != null)
			{
				if (_dialogElement != null)
				{



					if (System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted)
					{
						((ToolWindow)_dialogElement).Closing -= new System.ComponentModel.CancelEventHandler(OnWindowClosing);
					}
					else
					{
						if (_dialogElement is Window)
							((Window)_dialogElement).Closing -= new System.ComponentModel.CancelEventHandler(OnWindowClosing);
						else if (_dialogElement is ToolWindow)
							((ToolWindow)_dialogElement).Closing -= new System.ComponentModel.CancelEventHandler(OnWindowClosing);
					}


					_dialogElement = null;
				}

				_isInitialized = false;
				_childElement = null;
			}

		}

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region Internal Methods

		#region Close





		internal void Close()
		{
			DialogManager.CloseDialog(this.DialogElement);
		}
		#endregion //Close

		#region Initialize





		internal void Initialize()
		{
			if (this._isInitialized == false && this.DialogElement != null)
			{




				if (System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted)
				{
					if (this.DialogElement is ToolWindow)
						((ToolWindow)this.DialogElement).Closing += new System.ComponentModel.CancelEventHandler(OnWindowClosing);
				}
				else
				{
					if (this.DialogElement is Window)
						((Window)this.DialogElement).Closing += new System.ComponentModel.CancelEventHandler(OnWindowClosing);
					else if (this.DialogElement is ToolWindow)
						((ToolWindow)this.DialogElement).Closing += new System.ComponentModel.CancelEventHandler(OnWindowClosing);
				}


				this._isInitialized = true;
			}
		}
		#endregion //Initialize

		// AS 10/27/10 TFS36504
		// Refactored the close into a helper method.
		//
		#region SetDialogResult
		internal void SetDialogResult(bool result)
		{

			FrameworkElement dialogElement = this.DialogElement;

			if ( dialogElement is Window )
				((Window)dialogElement).DialogResult = result;
			else
			{
				var toolWindow = dialogElement as ToolWindow;

				if (null != toolWindow)
				{
					if (toolWindow.IsModal)
						toolWindow.DialogResult = result;
					else
						toolWindow.Close();
				}
			}

		}
		#endregion //SetDialogResult

		#endregion //Internal Methods

		#region Private Methods

		#region OnWindowClosing

		void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs args)
		{
			// Check the args.Cancel property to ensure that if the closing has already been canceled 
			// inside a callback supplied to the dialog manager, we don't allow that to be undone here.
			if (args.Cancel != true)
				args.Cancel = ((IDialogElementProxyHost)this._childElement).OnClosing();
		}

		#endregion //OnWindowClosing

		#region OnXamDialogWindowClosing


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		#endregion //OnXamDialogWindowClosing

		#endregion //Private Methods

		#endregion //Methods

		#region Properties

		#region DialogElement
		internal FrameworkElement DialogElement
		{
			get
			{
				if (this._dialogElement == null)
				{



					this._dialogElement = ToolWindow.GetToolWindow(this._childElement);
					if (null == this._dialogElement)
						this._dialogElement = PresentationUtilities.GetVisualAncestor<Window>(this._childElement, null);

				}

				return this._dialogElement;
			}
		}
		#endregion //DialogElement

		#endregion //Properties
	}
	#endregion //Internal DialogElementProxy Class

	#region Internal IDialogElementProxyHost Interface
	internal interface IDialogElementProxyHost
	{






		bool OnClosing();
	}
	#endregion //Internal IDialogElementProxyHost Interface
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