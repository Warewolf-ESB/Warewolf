using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Infragistics.Controls
{
	internal class KeyEventTracker
	{
		#region Member Variables

		private DependencyObject _focusedElement;

		#endregion // Member Variables

		#region Constructor
		internal KeyEventTracker()
		{
		}
		#endregion // Constructor

		#region Properties

		#region FocusedElement
		internal DependencyObject FocusedElement
		{
			get { return _focusedElement; }
		}
		#endregion // FocusedElement

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region Activate
		internal void Activate()
		{
			if ( _focusedElement != null )
				return;

			this.Hook(PresentationUtilities.GetElementWithKeyboardFocus());
		}
		#endregion // Activate

		#region Deactivate
		internal void Deactivate()
		{
			if ( _focusedElement == null )
				return;

			this.Unhook(_focusedElement, true);
		}
		#endregion // Deactivate

		#endregion // Internal Methods

		#region Private Methods

		#region Hook
		private void Hook( DependencyObject element )
		{
			if ( element == _focusedElement )
				return;

			if ( null != _focusedElement )
				this.Unhook(_focusedElement, false);



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


			IInputElement inputElement = element as IInputElement;

			if ( null != inputElement )
			{
				_focusedElement = element;

				inputElement.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(OnElementLostKeyboardFocus);
				inputElement.KeyDown += new KeyEventHandler(OnElementKey);
				inputElement.KeyUp += new KeyEventHandler(OnElementKey);
				inputElement.PreviewKeyDown += new KeyEventHandler(OnElementKey);
				inputElement.PreviewKeyUp += new KeyEventHandler(OnElementKey);
			}


			this.RaiseFocusedElementChange();
		}
		#endregion // Hook

		#region OnElementKey
		private void OnElementKey( object sender, KeyEventArgs e )
		{
			var handler = this.KeyEvent;

			if ( null != handler )
				handler(this, e);
		}
		#endregion // OnElementKey

		#region OnElementLostKeyboardFocus

		private void OnElementLostKeyboardFocus( object sender, KeyboardFocusChangedEventArgs e )
		{
			if ( e.OriginalSource == _focusedElement && e.OldFocus == _focusedElement )
			{
				// unhook the old and hook the new
				this.Hook(e.NewFocus as DependencyObject);
			}
		}

		#endregion // OnElementLostKeyboardFocus

		#region OnElementLostFocus






		#endregion // OnElementLostFocus

		#region RaiseFocusedElementChange
		private void RaiseFocusedElementChange()
		{
			var handler = this.FocusedElementChanged;

			if ( null != handler )
				handler(this, EventArgs.Empty);
		}
		#endregion // RaiseFocusedElementChange

		#region Unhook
		private void Unhook( DependencyObject element, bool raiseEvents )
		{
			if ( element != _focusedElement || element == null )
				return;

			_focusedElement = null;



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


			IInputElement inputElement = element as IInputElement;

			if ( null != inputElement )
			{
				inputElement.LostKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnElementLostKeyboardFocus);
				inputElement.KeyDown -= new KeyEventHandler(OnElementKey);
				inputElement.KeyUp += new KeyEventHandler(OnElementKey);
				inputElement.PreviewKeyDown -= new KeyEventHandler(OnElementKey);
				inputElement.PreviewKeyUp -= new KeyEventHandler(OnElementKey);
			}


			if ( raiseEvents )
				this.RaiseFocusedElementChange();
		}
		#endregion // Unhook

		#endregion // Private Methods

		#endregion // Methods

		#region Events

		internal event EventHandler FocusedElementChanged;
		internal event KeyEventHandler KeyEvent;

		#endregion // Events
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