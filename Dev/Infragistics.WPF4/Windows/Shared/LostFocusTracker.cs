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

namespace Infragistics.Controls
{
	internal class LostFocusTracker
	{
		#region Private Members

		private Action _callback;
		private UIElement _element;



		private IInputElement _elementWithFocus;


		#endregion //Private Members	
	
		#region Constructor

		internal LostFocusTracker(UIElement element, Action callback)
		{
			this._element = element;
			this._callback = callback;

			this.VerfiyIsFocusWithin();

		}

		#endregion //Constructor	
	
		#region Properties
		
		#region ElementWithFocus
		internal DependencyObject ElementWithFocus
		{
			get { return _elementWithFocus as DependencyObject; }
		}
		#endregion // ElementWithFocus

		#region IsActive

		// SSP 9/20/11 TFS87057
		// 
		/// <summary>
		/// Indicates if the this LostFocusTracker is active. If there's no focused element
		/// that it's tracking then it's considered to be inactive.
		/// </summary>
		internal bool IsActive
		{
			get
			{
				return null != _elementWithFocus;
			}
		} 

		#endregion // IsActive

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region Deactivate

		internal void Deactivate(bool forceLostFocusBindingUpdates)
		{
			if (this._elementWithFocus != null)
			{
				// In SL the text property 2-way bindings don't update the source until the LostFocu event which
				// is raised asynchronously in SL. Therefore call ForceTextBindingUpdate which special cases
				// the text property off TextBox and the Password property off PasswordBox (which are the only
				// 2 properties in SL which have this behavior) and force the binding to push the value back to the
				// source.
				if (forceLostFocusBindingUpdates)
					PresentationUtilities.ForceLostFocusBindingUpdate(_element, _elementWithFocus as DependencyObject);




				this._elementWithFocus.LostKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnElementWithFocus_LostFocus);

				this._elementWithFocus = null;
			}

			this._element = null;
			this._callback = null;
		}

		#endregion //Deactivate

		#endregion //Internal Methods	
		
		#region Private Methods

		#region OnElementWithFocus_LostFocus



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		private void OnElementWithFocus_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (this._elementWithFocus != null)
			{
				this._elementWithFocus.LostKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnElementWithFocus_LostFocus);

				this.VerfiyIsFocusWithin();
			}
		}


		#endregion //OnElementWithFocus_LostFocus

		#region VerfiyIsFocusWithin

		private void VerfiyIsFocusWithin()
		{
			DependencyObject focusedObject = PresentationUtilities.GetElementWithKeyboardFocus();

			bool isFocusWithin = focusedObject != null ? PresentationUtilities.IsAncestorOf(this._element, focusedObject) : false;

			if (isFocusWithin)
			{


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

				this._elementWithFocus = focusedObject as IInputElement;

				if (this._elementWithFocus != null )
					this._elementWithFocus.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(OnElementWithFocus_LostFocus);
				else
					isFocusWithin = false;

			}

			if (!isFocusWithin)
			{
				if (this._callback != null)
				{
					Action callback = _callback;
					this._callback = null;
					callback();
				}

				this._element = null;
			}

		}

		#endregion //VerfiyIsFocusWithin

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