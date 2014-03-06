using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;

namespace Infragistics.Controls.Schedules
{
	internal class DetermineBodyFormatHelper
	{
		#region Member Variables

		private ActivityBase _activity;
		private ExchangeScheduleDataConnector _connector;
		private Action<DescriptionFormat> _onDetermineBodyFormatCompleted;
		private ErrorCallback _onError;

		#endregion  // Member Variables

		#region Constructor

		private DetermineBodyFormatHelper(ExchangeScheduleDataConnector connector, ActivityBase activity)
		{
			_activity = activity;
			_connector = connector;

			_onDetermineBodyFormatCompleted = this.OnDetermineBodyFormatCompleted;
			_onError = this.OnError;
		} 

		#endregion  // Constructor

		#region Methods

		#region Execute

		public static void Execute(ExchangeScheduleDataConnector connector, ActivityBase activity)
		{
			ItemType item = activity.DataItem as ItemType;
			if (item == null)
			{
				ExchangeConnectorUtilities.DebugFail("The activity should have an item associated.");
				return;
			}

			activity.OriginalDescriptionFormat = DescriptionFormat.Unknown;

			DetermineBodyFormatHelper helper = new DetermineBodyFormatHelper(connector, activity);
			ExchangeFolder folder = connector.GetOwningExchangeFolder(activity);
			folder.Service.DetermineBodyFormat(item, folder, helper._onDetermineBodyFormatCompleted, helper._onError);
		}

		#endregion  // Execute

		#region OnDetermineBodyFormatCompleted

		private void OnDetermineBodyFormatCompleted(DescriptionFormat format)
		{
			_activity.OriginalDescriptionFormat = format;
		}

		#endregion  // OnDetermineBodyFormatCompleted

		#region OnError

		private bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			// If the item can't be found, it may be because we moved the activity to a different calendar and the move operation
			// completed before the call to determine the body format is processed (the server will not process calls in the 
			// order they are received). In that case, we will get an error from the server, but it is correct, so just reset 
			// the OriginalDescriptionFormat.
			if (serverResponseCode == ResponseCodeType.ErrorItemNotFound)
			{
				_activity.OriginalDescriptionFormat = DescriptionFormat.Default;
				return true;
			}

			_connector.DefaultErrorCallback(reason, serverResponseCode, error);
			return false;
		}

		#endregion  // OnError 

		#endregion  // Methods
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