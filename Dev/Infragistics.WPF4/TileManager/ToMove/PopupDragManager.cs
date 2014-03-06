using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

using Infragistics.Windows.Controls;






using System.Windows.Controls;




namespace Infragistics.Controls.Primitives
{
	internal class PopupDragManager
	{
		#region Private Members

		private ContentControl _contentControl;
		private Popup _popup;


		private PopupController _controller;




		#endregion //Private Members	
 
		#region Constructor

		internal PopupDragManager()
		{









		}

		#endregion //Constructor	

		#region Properties

		#region Internal Properties

		#region Popup

		internal Popup Popup { get { return this._popup; } }

		#endregion //Popup





		internal double ZoomFactor { get { return 1; } }


		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region Close

		internal void Close()
		{
			if (_popup == null)
				return;

			_popup.IsOpen = false;
			_popup = null;


			_controller = null;


			if (_contentControl != null)
			{
				_contentControl.ClearValue(ContentControl.ContentProperty);
				_contentControl = null;
			}
		}

		#endregion //Close
    
		#region Open

		internal bool Open(MouseEventArgs mouseArgs, Point offset, FrameworkElement owner, object content, DataTemplate contentTemplate)
		{
			CoreUtilities.ValidateNotNull(mouseArgs);
			CoreUtilities.ValidateNotNull(owner);

			Point pt = mouseArgs.GetPosition(null);

			pt.X += offset.X;
			pt.Y += offset.Y;

			return this.Open(pt, owner, content, contentTemplate);
		}

		internal bool Open(Point ptinScreenCoords, FrameworkElement owner, object content, DataTemplate contentTemplate)
		{
			CoreUtilities.ValidateNotNull(owner);

			if (_popup != null)
				this.Close();

			_contentControl = new ContentControl();
			_contentControl.Content = content;
			
			if ( contentTemplate != null )
				_contentControl.ContentTemplate = contentTemplate;
			
			_popup = new Popup();
			_popup.FlowDirection = owner.FlowDirection;
			_popup.Child = _contentControl;

			_popup.Placement = PlacementMode.Absolute;
			_popup.AllowsTransparency = true;

			_controller = PopupController.Create(_popup, owner);

			// JJD 03/28/12 - TFS106951
			// If no PopupController was created it means we are in an XBAP application
			// and we don't have security permissions to create a top level window.
			// If this is the case we need to set the PlacementTarget.
			// Otherwise setting the Popop's IsOpen property to true will throw
			// a secuirty exception
			_popup.PlacementTarget = owner;

			if (_controller != null)
				_controller.TopMost = true;


			this.PositionPopup(ptinScreenCoords);

			_popup.IsOpen = true;

			return true;
		}

		#endregion //Open

		#region PositionPopup

		internal bool PositionPopup(Point ptInScreenCoords)
		{
			if (_popup == null)
				return false;



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


			_popup.HorizontalOffset = ptInScreenCoords.X /= this.ZoomFactor;
			_popup.VerticalOffset = ptInScreenCoords.Y /= this.ZoomFactor;

			return true;
		}

		internal bool PositionPopup(MouseEventArgs mouseArgs, Point offset)
		{
			CoreUtilities.ValidateNotNull(mouseArgs);

			Point pt = mouseArgs.GetPosition(null);

			pt.X += offset.X;
			pt.Y += offset.Y;

			return this.PositionPopup(pt);
		}

		#endregion //PositionPopup	

		#endregion //Internal Methods

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