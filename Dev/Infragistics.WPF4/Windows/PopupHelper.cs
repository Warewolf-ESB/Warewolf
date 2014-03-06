using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
    /// <summary>
    /// Class that provides attached behaviors for dealing with a <see cref="Popup"/>
    /// </summary>
    public static class PopupHelper
    {
        #region Member Variables

        // AS 3/30/09 TFS16355 - WinForms Interop
        [ThreadStatic]
        private static bool? _popupAllowsTransparency;

        #endregion //Member Variables

        #region Constructor
        static PopupHelper()
        {
            EventManager.RegisterClassHandler(typeof(Popup), Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(OnPopupPreviewMouseUp));
            EventManager.RegisterClassHandler(typeof(Popup), Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPopupPreviewMouseDown));
        }
        #endregion //Constructor

        #region Properties

        #region DropDownButton

        /// <summary>
        /// DropDownButton Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty DropDownButtonProperty =
            DependencyProperty.RegisterAttached("DropDownButton", typeof(ButtonBase), typeof(PopupHelper),
                new FrameworkPropertyMetadata((ButtonBase)null, new PropertyChangedCallback(OnDropDownButtonChanged)));

        private static void OnDropDownButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Popup p = d as Popup;

            if (null != p)
            {
                if (e.NewValue == null)
                    p.Opened -= new EventHandler(OnPopupOpened);
                else
                    p.Opened += new EventHandler(OnPopupOpened);
            }
        }

        /// <summary>
        /// Gets the button that acts as the dropdown button for the popup.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(Popup))]
        public static ButtonBase GetDropDownButton(DependencyObject d)
        {
            return (ButtonBase)d.GetValue(DropDownButtonProperty);
        }

        /// <summary>
        /// Sets the button that acts as the dropdown button for the popup.
        /// </summary>
        public static void SetDropDownButton(DependencyObject d, ButtonBase value)
        {
            d.SetValue(DropDownButtonProperty, value);
        }

        #endregion //DropDownButton

		// AS 9/23/09 TFS22369
		#region HandleMouseDownOnClose

		/// <summary>
		/// HandleMouseDownOnClose Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty HandleMouseDownOnCloseProperty =
			DependencyProperty.RegisterAttached("HandleMouseDownOnClose", typeof(bool), typeof(PopupHelper),
				new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnHandleMouseDownOnCloseChanged)));

		private static void OnHandleMouseDownOnCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Popup p = d as Popup;

			if (null != p)
			{
				// note i'm using an instance handler here because I want to make sure the popup has 
				// already processed its handling of the previewmousedown
				if (true.Equals(e.NewValue))
					p.PreviewMouseDown += new MouseButtonEventHandler(OnPopupPreviewMouseDownInstance);
				else
					p.PreviewMouseDown -= new MouseButtonEventHandler(OnPopupPreviewMouseDownInstance);
			}
		}

		/// <summary>
		/// Returns a boolean indicating if the PreviewMouseDown of the Popup should be marked as handled when the Popup is closed.
		/// </summary>
		/// <seealso cref="SetHandleMouseDownOnClose"/>
		public static bool GetHandleMouseDownOnClose(DependencyObject d)
		{
			return (bool)d.GetValue(HandleMouseDownOnCloseProperty);
		}

		/// <summary>
		/// Returns a boolean indicating if the PreviewMouseDown of the Popup should be marked as handled when the Popup is closed.
		/// </summary>
		/// <remarks>
		/// <p class="body">The current implementation of a <see cref="Popup"/> is to close itself during the PreviewMouseDown tunnel event 
		/// and release capture. However, it does not mark the PreviewMouseDown as handled and so the element on which you clicked will 
		/// still receive a MouseDown event. In certain cases, it is preferrable to have the mouse down that caused the click to be 
		/// intercepted and not affect the element on which it was clicked until the subsequent mouse click. Setting this property to 
		/// true will ensure that if the Popup's PreviewMouseDown is invoked and not handled even though the Popup was closed that the 
		/// PreviewMouseDown is handled and therefore the element on which the mouse was clicked will not receive the MouseDown.</p>
		/// <p class="note"><b>Note:</b> This property will not affect a popup type implementation whereby the popup was closed during 
		/// the PreviewMouseDownOutsideCapturedElement.</p>
		/// </remarks>
		/// <seealso cref="GetHandleMouseDownOnClose"/>
		public static void SetHandleMouseDownOnClose(DependencyObject d, bool value)
		{
			d.SetValue(HandleMouseDownOnCloseProperty, value);
		}
		#endregion //HandleMouseDownOnClose

		// AS 3/30/09 TFS16355 - WinForms Interop
        #region IsPopupInChildWindow
        internal static bool IsPopupInChildWindow
        {
            get
            {
				
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				return !PopupAllowsTransparency;
            }
        }
        #endregion //IsPopupInChildWindow

		// AS 9/11/09 TFS21330
		#region PopupAllowsTransparency
		internal static bool PopupAllowsTransparency
		{
			get
			{
				if (_popupAllowsTransparency == null)
				{
					System.Windows.Controls.Primitives.Popup p = new System.Windows.Controls.Primitives.Popup();
					p.AllowsTransparency = true;
					p.CoerceValue(System.Windows.Controls.Primitives.Popup.AllowsTransparencyProperty);
					_popupAllowsTransparency = p.AllowsTransparency;
				}

				// AS 10/16/09 TFS22903
				// When this was refactored from IsPopupInChildWindow the value was inverted.
				//
				//return !_popupAllowsTransparency.Value;
				return _popupAllowsTransparency.Value;
			}
		} 
		#endregion //PopupAllowsTransparency

        #endregion //Properties

        #region Methods

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region BringToFront
        /// <summary>
        /// Method used to try and bring a 
        /// </summary>
        /// <param name="popup">The popup that should be brought to the front of the z-order.</param>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This method requires unmanaged code rights in order to function but will not throw an unhandled exception if 
        /// the rights are not available.</p>
        /// </remarks>
        public static void BringToFront(Popup popup)
        {
            Utilities.ThrowIfNull(popup, "popup");

            if (popup.IsOpen == false)
                return;

            try
            {
                PopupController.BringToFront(popup);
            }
            catch (System.Security.SecurityException)
            {
            }
        } 
        #endregion //BringToFront

        #region OnPopupOpened
        private static void OnPopupOpened(object sender, EventArgs e)
        {
            Popup p = (Popup)sender;
            ButtonBase btn = GetDropDownButton(p);
            Debug.Assert(null != btn);

            // if the button that caused the popup to open has the mouse
            // capture then we need to take capture away from it and have
            // the popup capture the mouse
            if (null != btn && btn.IsMouseCaptureWithin && p.StaysOpen == false)
            {
                // force the button to relinquish capture
                btn.ReleaseMouseCapture();

                // then toggle the stays open. when stays open goes back to false
                // while its open, the popup will retake the capture
                p.StaysOpen = true;
                p.StaysOpen = false;
            }
        }
        #endregion //OnPopupOpened

        #region OnPopupPreviewMouseUp
        private static void OnPopupPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Popup p = sender as Popup;
            ButtonBase btn = GetDropDownButton(p);

            if (null != btn
                && p.IsOpen
                && p.StaysOpen == false
                && btn.ClickMode == System.Windows.Controls.ClickMode.Press)
            {
                HitTestResult result = VisualTreeHelper.HitTest(btn, e.GetPosition(btn));

                // since the popup should have capture, releasing the mouse button
                // over the dropdown button would cause the popup to think the 
                // mouse was clicked outside and that it should close so eat the mouse
                // message so it doesn't close
                if (null != result && null != result.VisualHit && btn.IsAncestorOf(result.VisualHit))
                    e.Handled = true;
            }
        }

        #endregion //OnPopupPreviewMouseUp

        #region OnPopupPreviewMouseDown
        private static void OnPopupPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Popup p = sender as Popup;
            ButtonBase btn = GetDropDownButton(p);

            if (null != btn
                && p.IsOpen
                && p.StaysOpen == false
                && btn.ClickMode != System.Windows.Controls.ClickMode.Hover)
            {
                HitTestResult result = VisualTreeHelper.HitTest(btn, e.GetPosition(btn));

                // if the mouse is pressed down on the dropdown button...
                if (null != result && null != result.VisualHit && btn.IsAncestorOf(result.VisualHit))
                {
                    // if the click mode is press then we want to close the popup
                    if (btn.ClickMode == System.Windows.Controls.ClickMode.Press)
                        p.IsOpen = false;

                    // in either case (if its release or press) we want to eat the mouse. for release
                    // we want to eay it because we don't want the popup to close until the mouse up
                    // and for press, we don't want the button getting the mouse down and showing
                    // the popup again when it was closed as a result of the same mouse down
                    e.Handled = true;
                }
            }
        }

		// AS 9/23/09 TFS22369
		private static void OnPopupPreviewMouseDownInstance(object sender, MouseButtonEventArgs e)
		{
			Popup p = sender as Popup;

			if (!p.IsOpen && GetHandleMouseDownOnClose(p))
				e.Handled = true;
		}
        #endregion //OnPopupPreviewMouseDown

        #endregion //Methods
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