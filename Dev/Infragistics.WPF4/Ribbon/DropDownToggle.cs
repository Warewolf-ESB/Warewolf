using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;
using System.Windows.Automation.Provider;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// An element used the toggle the drop down state of another element.
	/// </summary>
	/// <remarks>
	/// <para class="body">It functions like a ToggleButton whose ClickMode is set to 'Press' but it doesn't take mouse capture (which can interfere with a Popup whose StaysOpen property is set to 'False').</para>
	/// </remarks>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DropDownToggle : ContentControl
	{
		#region Private Members

		// AS 1/17/08 BR29775
		[ThreadStatic()]
		private static DropDownToggle s_toggleWithLastMousePress;

		#endregion //Private Members	
    
		#region Construstors

		/// <summary>
		/// Initializes a new instance of a <see cref="DropDownToggle"/> class.
		/// </summary>
		public DropDownToggle()
		{
		}

		static DropDownToggle()
		{
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(DropDownToggle), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			EventManager.RegisterClassHandler(typeof(UIElement), Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown_ClassHandler), true);
			EventManager.RegisterClassHandler(typeof(UIElement), Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(OnPreviewMouseUp_ClassHandler), true);
			EventManager.RegisterClassHandler(typeof(UIElement), Mouse.GotMouseCaptureEvent, new MouseEventHandler(OnGotMouseCapture_ClassHandler), true);
		}

		#endregion //Construstors

		#region Base Class Overrides

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="DropDownToggle"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.DropDownToggleAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new DropDownToggleAutomationPeer(this);
        }
            #endregion

			#region OnMouseEnter

		/// <summary>
		/// Called when the mouse enters the element.
		/// </summary>
		/// <param name="e">The mouse event arguments</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (this == s_toggleWithLastMousePress)
				this.SetValue(IsPressedPropertyKey, KnownBoxes.TrueBox);
		}

			#endregion //OnMouseEnter	

			#region OnMouseLeave

		/// <summary>
		/// Called when the mouse leaves the element.
		/// </summary>
		/// <param name="e">The mouse event arguments</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			this.SetValue(IsPressedPropertyKey, KnownBoxes.FalseBox);
		}

			#endregion //OnMouseLeave	

			#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when the left mouse button is pressed
		/// </summary>
		/// <param name="e">The mouse event arguments</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			if (e.Handled == true)
				return;

			this.ProcessMouseDown(e);
		}

			#endregion //OnMouseLeftButtonDown	

			#region OnMouseLeftButtonUp

		/// <summary>
		/// Called when the left mouse button is released
		/// </summary>
		/// <param name="e">The mouse event arguments</param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);

			if (Mouse.Captured == this)
			{
				this.ReleaseMouseCapture();

				this.SetValue(IsPressedPropertyKey, KnownBoxes.FalseBox);

				ResetToggleWithLastMousePress();

				e.Handled = true;
			}
		}

			#endregion //OnMouseLeftButtonUp

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region IsDroppedDown

		/// <summary>
		/// Identifies the <see cref="IsDroppedDown"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDroppedDownProperty = DependencyProperty.Register("IsDroppedDown",
			typeof(bool), typeof(DropDownToggle), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsDroppedDownChanged)));

		private static void OnIsDroppedDownChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			DropDownToggle toggle = target as DropDownToggle;

            if (toggle != null)
            {
                toggle.SetValue(IsPressedPropertyKey, KnownBoxes.FalseBox);
                // TK Raise property change event
                bool? newValue = (bool?)e.NewValue;
                bool? oldValue = (bool?)e.OldValue;
                toggle.RaiseAutomationToggleStatePropertyChanged(oldValue, newValue);
            }
		}

		/// <summary>
		/// Gets/sest the drop down state.
		/// </summary>
		/// <remarks>
		/// <para class="body">Toggles its state on a left mouse down.</para>
		/// </remarks>
		/// <seealso cref="IsDroppedDownProperty"/>
		//[Description("Gets/sest the drop down state.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsDroppedDown
		{
			get
			{
				return (bool)this.GetValue(DropDownToggle.IsDroppedDownProperty);
			}
			set
			{
				this.SetValue(DropDownToggle.IsDroppedDownProperty, value);
			}
		}

				#endregion //IsDroppedDown

				#region IsPressed

		private static readonly DependencyPropertyKey IsPressedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsPressed",
			typeof(bool), typeof(DropDownToggle), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsPressed"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsPressedProperty =
			IsPressedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns whether the element is being pressed (read-only).
		/// </summary>
		/// <seealso cref="IsPressedProperty"/>
		//[Description("Returns whether the element is being pressed (read-only).")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsPressed
		{
			get
			{
				return (bool)this.GetValue(DropDownToggle.IsPressedProperty);
			}
		}

				#endregion //IsPressed

			#endregion //Public Properties	
    
		#endregion //Properties	
  
		#region Methods

			#region Private Methods

				#region GetDropDownToggleUnderMouse

		private static DropDownToggle GetDropDownToggleUnderMouse(object sender, MouseButtonEventArgs e)
		{
			Popup popup = sender as Popup;

			if (popup == null)
			{
				FrameworkElement fe = e.OriginalSource as FrameworkElement;

				if (fe == null)
					return null;

				if (VisualTreeHelper.GetParent(fe) != null)
					return null;

				popup = fe.Parent as Popup;

				if (popup == null || popup.StaysOpen == true)
					return null;
			}

			FrameworkElement tparent = popup.TemplatedParent as FrameworkElement;

			if (tparent == null)
				tparent = popup.PlacementTarget as FrameworkElement;

			// JM 11-26-07 Workitem #1228 - Remove the following uneeded assertion.
			//Debug.Assert(tparent != null);

			if (tparent == null)
				return null;

			DependencyObject elementUnderMouse = tparent.InputHitTest(e.GetPosition(tparent)) as DependencyObject;

			if (elementUnderMouse == null)
				return null;

			DropDownToggle ddt = elementUnderMouse as DropDownToggle;

			if (ddt == null)
				ddt = Utilities.GetAncestorFromType(elementUnderMouse, typeof(DropDownToggle), true) as DropDownToggle;

			return ddt;
		}

				#endregion //GetDropDownToggleUnderMouse	
        
				#region OnGotMouseCapture_ClassHandler

		private static void OnGotMouseCapture_ClassHandler(object sender, MouseEventArgs e)
		{
			DropDownToggle toggle = Mouse.Captured as DropDownToggle;

			if (toggle == null)
			{
				ResetToggleWithLastMousePress();
			}
		}

				#endregion //OnGotMouseCapture_ClassHandler	

				#region OnPreviewMouseDown_ClassHandler

		private static void OnPreviewMouseDown_ClassHandler(object sender, MouseButtonEventArgs e)
		{
			ResetToggleWithLastMousePress();

			DropDownToggle ddt = GetDropDownToggleUnderMouse(sender, e);

			if (ddt != null && ddt.IsDroppedDown)
				ddt.ProcessMouseDown(e);
		}

				#endregion //OnPreviewMouseDown_ClassHandler	

				#region OnPreviewMouseUp_ClassHandler

		private static void OnPreviewMouseUp_ClassHandler(object sender, MouseButtonEventArgs e)
		{
			ResetToggleWithLastMousePress();

			DropDownToggle ddt = GetDropDownToggleUnderMouse(sender, e);

			if (ddt != null && ddt.IsDroppedDown)
				e.Handled = true;
			
		}

				#endregion //OnPreviewMouseUp_ClassHandler	
	
				#region ProcessMouseDown

		private void ProcessMouseDown(MouseButtonEventArgs e)
		{

			// toggle the dropdown state
			bool isDroppedDown = !this.IsDroppedDown;

			this.IsDroppedDown = isDroppedDown;

			if (isDroppedDown == false)
				this.CaptureMouse();

			if (Mouse.LeftButton == MouseButtonState.Pressed &&
				Mouse.Captured == null &&
				this.IsMouseOver)
			{
				this.SetValue(IsPressedPropertyKey, KnownBoxes.TrueBox);
				s_toggleWithLastMousePress = this;
			}

			e.Handled = true;
		}

				#endregion //ProcessMouseDown	
    
                #region RaiseAutomationExpandCollapseStateChanged

        private void RaiseAutomationToggleStatePropertyChanged(bool? oldValue, bool? newValue)
        {
            DropDownToggleAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as DropDownToggleAutomationPeer;

            if (null != peer)
                peer.RaiseToggleStatePropertyChangedEvent(oldValue, newValue);
        }

                #endregion //RaiseAutomationExpandCollapseStateChanged

                #region ResetToggleWithLastMousePress

		private static void ResetToggleWithLastMousePress()
		{
			DropDownToggle toggle = Mouse.Captured as DropDownToggle;

			if (toggle != null)
			{
				//toggle.UpdateLayout();
				toggle.ReleaseMouseCapture();
			}

			if (s_toggleWithLastMousePress != null)
			{
				s_toggleWithLastMousePress.SetValue(IsPressedPropertyKey, KnownBoxes.FalseBox);
				s_toggleWithLastMousePress = null;
			}
		}

				#endregion //ResetToggleWithLastMousePress	
    
			#endregion //Private Methods	
    
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