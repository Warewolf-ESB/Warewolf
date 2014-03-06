using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules
{
	#region QueryOccurrencesHelper class

	internal class QueryOccurrencesHelper
	{
		#region Member Variables

		private ExchangeScheduleDataConnector _connector;
		private DataErrorInfo _errorInfo;
		private Action _onCompleted;
		private ErrorCallback _onError;
		private Action<ExchangeFolder, IList<ItemType>> _onItemsReceivedFromServer;
		private Action _onRemoteCallProcessed;
		private int _pendingCallCount;

		#endregion  // Member Variables

		#region Constructor

		private QueryOccurrencesHelper(ExchangeScheduleDataConnector connector, Action onCompleted)
		{
			_connector = connector;
			_onCompleted = onCompleted;

			_onError = this.OnError;
			_onItemsReceivedFromServer = this.OnItemsReceivedFromServer;
			_onRemoteCallProcessed = this.OnRemoteCallProcessed;
		}

		#endregion  // Constructor

		#region Methods

		#region Execute

		public static void Execute(
			ExchangeScheduleDataConnector connector,
			List<ActivityBase> rootActivities,
			Action onCompleted,
			ErrorCallback onError)
		{
			QueryOccurrencesHelper helper = new QueryOccurrencesHelper(connector, onCompleted);

			helper._pendingCallCount++;
			connector.OnRecurringItemsAddedOrModified(
				rootActivities,
				helper._onItemsReceivedFromServer,
				helper._onRemoteCallProcessed,
				onError);
		}

		#endregion  // Execute

		#region OnError

		private bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			DataErrorInfo errorInfo = _connector.GetDataErrorInfo(reason, error);

			if (_errorInfo == null)
				_errorInfo = errorInfo;
			else if (_errorInfo.ErrorList != null)
				_errorInfo.ErrorList.Add(errorInfo);
			else
				_errorInfo = DataErrorInfo.CreateFromList(new List<DataErrorInfo>() { _errorInfo, errorInfo });

			this.OnRemoteCallProcessed();
			return false;
		}

		#endregion  // OnError

		#region OnItemsReceivedFromServer

		public void OnItemsReceivedFromServer(ExchangeFolder folder, IList<ItemType> items)
		{
			List<ActivityBase> recurringActivityRoots;
			_connector.OnItemsReceivedFromServer(
				folder, items,
				ref _pendingCallCount, out recurringActivityRoots,
				_onRemoteCallProcessed, _onError);
		}

		#endregion  // OnItemsReceivedFromServer

		#region OnRemoteCallProcessed

		private void OnRemoteCallProcessed()
		{
			_pendingCallCount--;
			Debug.Assert(_pendingCallCount >= 0, "The pending call count should never be less than 0");

			if (_pendingCallCount <= 0)
			{
				if (_errorInfo != null)
					_connector.RaiseError(_errorInfo);
				else if (_onCompleted != null)
					_onCompleted();
			}
		}

		#endregion  // OnRemoteCallProcessed

		#endregion  // Methods
	}

	#endregion  // QueryOccurrencesHelper class
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