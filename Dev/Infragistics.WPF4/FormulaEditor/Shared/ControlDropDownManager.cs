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
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Diagnostics;

using Infragistics.Windows; 


namespace Infragistics.Controls.Primitives
{
	internal class ControlDropDownManager
	{
		#region Private Members

		private Control _owningControl;
		private Popup _popup;
		private ToggleButton _button;
		private Action _openAction;
		private Action _closeAction;



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


		#endregion //Private Members	
    
		#region Constructor

		internal ControlDropDownManager(Control owningControl, Popup popup, ToggleButton button, Action openAction, Action closeAction

			, Decorator chrome

			) : this(owningControl, popup, button, false, openAction, closeAction

			, chrome

			)
		{
		}

		internal ControlDropDownManager(Control owningControl, Popup popup, ToggleButton button, bool staysOpen, Action openAction, Action closeAction

			, Decorator chrome

)
		{
			CoreUtilities.ValidateNotNull(owningControl, "owningControl");
			CoreUtilities.ValidateNotNull(popup, "popup");

			_owningControl		= owningControl;
			_popup				= popup;
			_popup.Opened		+= new EventHandler(OnPopupOpened);
			_popup.Closed		+= new EventHandler(OnPopupClosed);
			_button				= button;
			_openAction			= openAction;
			_closeAction		= closeAction;


			_popup.StaysOpen = staysOpen;

			if (_button != null)
				Infragistics.Windows.Controls.PopupHelper.SetDropDownButton(_popup, _button);

			ControlDropDownManager.InitializeIfNotDefault(_popup, Popup.AllowsTransparencyProperty, KnownBoxes.TrueBox);

			if (SystemParameters.ComboBoxAnimation == true)
				ControlDropDownManager.InitializeIfNotDefault(_popup, Popup.PopupAnimationProperty, SystemParameters.ComboBoxPopupAnimation);

			if (chrome != null)
			{
				UIElement child = _popup.Child;

				// since we couldn't wrap the popup's content in a chrome element in xaml (because SL doesn't support it)
				// create it now
				if (child != null && child.GetType().Name != "SystemDropShadowChrome")
				{
					_popup.Child = chrome;
					chrome.Child = child;
				}
			}


#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)


			this.Placement = DropDownPlacementMode.Bottom;
		}

		#endregion //Constructor	
 
		#region Properties



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		#region HorizontalOffset

		internal double HorizontalOffset
		{
			get
			{



				return _popup.HorizontalOffset;

			}
			set
			{



				_popup.HorizontalOffset = value;

			}
		}

		#endregion  // HorizontalOffset

		#region IsOpen

		internal bool IsOpen
		{
			get { return _popup != null && _popup.IsOpen; }
			set
			{
				Debug.Assert(_popup != null, "Popup not initialized ");
				if (_popup == null)
					return;

				if (_popup.IsOpen != value)
				{




					_popup.IsOpen = value;
				}
			}
		}

		#endregion //IsOpen	

		#region OriginalPopupChild

		internal FrameworkElement OriginalPopupChild
		{
			get
			{



				return _popup.Child as FrameworkElement;

			}
		}

		#endregion  // OriginalPopupChild

		#region Placement

		internal DropDownPlacementMode Placement
		{
			get
			{



				return (DropDownPlacementMode)_popup.Placement;

			}
			set
			{



				_popup.Placement = (PlacementMode)value;

			}
		}

		#endregion  // Placement

		#region StaysOpen

		internal bool StaysOpen
		{
			get 
			{



				return _popup.StaysOpen;

			}
			set 
			{
				if (this.StaysOpen == value)
					return;



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

				_popup.StaysOpen = value;

			}
		}

		#endregion  // StaysOpen

		#region VerticalOffset

		internal double VerticalOffset
		{
			get
			{



				return _popup.VerticalOffset;

			}
			set
			{



				_popup.VerticalOffset = value;

			}
		}

		#endregion  // VerticalOffset

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region Dispose

		internal void Dispose()
		{
			if (_owningControl == null)
				return;



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


			_popup.Opened -= new EventHandler(OnPopupOpened);
			_popup.Closed -= new EventHandler(OnPopupClosed);









			_popup = null;
			_owningControl = null;
			_button = null;




		}

		#endregion //Dispose	



#region Infragistics Source Cleanup (Region)







































































































































































































































































#endregion // Infragistics Source Cleanup (Region)



		#region InitializeIfNotDefault

		internal static void InitializeIfNotDefault(DependencyObject target, DependencyProperty property, object value)
		{
			ValueSource source = DependencyPropertyHelper.GetValueSource(target, property);

			if (source.BaseValueSource == BaseValueSource.Default)
				target.SetValue(property, value);
		}

		#endregion  // InitializeIfNotDefault


		#endregion //Internal Methods

		#region Private Methods

		#region InitializeOutsidePopupOverlay



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


		#endregion  // InitializeOutsidePopupOverlay

		#region ResetSizingConstraints



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


		#endregion  // ResetSizingConstraints

		#endregion  // Private Methods

		#region Event Handlers


#region Infragistics Source Cleanup (Region)




















































#endregion // Infragistics Source Cleanup (Region)

		private void OnPopupClosed(object sender, EventArgs e)
		{
			if (_closeAction != null)
				_closeAction();







		}

		private void OnPopupOpened(object sender, EventArgs e)
		{
			if (_openAction != null)
				_openAction();



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion //Event Handlers	
    
		#endregion //Methods
	}

	internal enum DropDownPlacementMode
	{
		Relative = 1,
		Bottom = 2,
		Right = 4,
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