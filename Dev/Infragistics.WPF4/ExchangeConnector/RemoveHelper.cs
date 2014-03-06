using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Collections;
using Infragistics.Controls.Schedules.EWS;

namespace Infragistics.Controls.Schedules
{
	internal class RemoveHelper
	{
		#region Member Variables

		private ActivityBase _activity;
		private ExchangeScheduleDataConnector _connector;
		private Action _onDeleteItemCompleted;
		private ErrorCallback _onError;
		private ActivityOperationResult _result; 

		#endregion  // Member Variables

		#region Constructor

		private RemoveHelper(ExchangeScheduleDataConnector connector, ActivityBase activity, ActivityOperationResult result)
		{
			_activity = activity;
			_connector = connector;
			_result = result;

			_onDeleteItemCompleted = this.OnDeleteItemCompleted;
			_onError = this.OnError;
		} 

		#endregion  // Constructor

		#region Methods

		#region OnDeleteItemCompleted

		private void OnDeleteItemCompleted()
		{
			_connector.VerifyItemsInQueryResults(_activity, false, false);
			_activity.RaisePropertyChangedEvent("IsDeleted");
			if (_activity.IsRecurrenceRoot)
			{
				WeakList<ActivityBase> occurrenceList;
				if (_connector.OccurrencesByRoot.TryGetValue(_activity, out occurrenceList))
				{
					ActivityBase[] occurrences = occurrenceList.ToArray();
					for (int i = 0; i < occurrences.Length; i++)
					{
						ActivityBase occurrence = occurrences[i];
						if (occurrence != null)
							occurrence.RaisePropertyChangedEvent("IsDeleted");
					}
				}
			}
			// MD 11/4/11 - TFS81088
			// When an occurrence is deleted, the recurrence root will have a new change key in its Id on the server, 
			// so we should re-get the Id to make sure subsequent updates to the root activity do not cause an error.
			// Also, moved the call to InitializeResult to a helper method named OnOperationComplete so we can call
			// it from multiple places.
			//_result.InitializeResult(null, true);
			else if (_activity.RootActivity != null)
			{
				ExchangeFolder owningFolder = _connector.GetOwningExchangeFolder(_activity);
				ItemType rootActivityItem = _activity.RootActivity.DataItem as ItemType;

				if (rootActivityItem != null)
				{
					owningFolder.Service.RefreshItemId(
						owningFolder, 
						rootActivityItem, 
						(folder, items) => this.OnOperationComplete(), 
						_onError);
				}
				else
				{
					ExchangeConnectorUtilities.DebugFail("The root activity should have an item here.");
					this.OnOperationComplete();
				}
			}
			else
			{
				this.OnOperationComplete();
			}
		}

		#endregion  // OnDeleteItemCompleted

		#region OnError

		private bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			_result.InitializeResult(_connector.GetDataErrorInfo(reason, error, _activity), true);
			return false;
		}

		#endregion  // OnError

		// MD 11/4/11 - TFS81088
		#region OnOperationComplete

		private void OnOperationComplete()
		{
			_result.InitializeResult(null, true);
		}

		#endregion  // OnOperationComplete

		#region RemoveActivity

		public static ActivityOperationResult RemoveActivity(ExchangeScheduleDataConnector connector, ActivityBase activity)
		{
			bool shouldDeleteAllTaskOccurrences = ExchangeConnectorUtilities.ShouldDeleteAllTaskOccurrences(activity);
			if (activity.IsOccurrence && activity is Task)
				activity = activity.RootActivity;

			ActivityOperationResult result = new ActivityOperationResult(activity);

			if (ListScheduleDataConnectorUtilities<ActivityBase>.CheckItemOperationAllowed(connector, result, ActivityOperation.Remove) == false)
				return result;

			ItemType item = activity.DataItem as ItemType;
			if (item == null || item.ItemId == null)
			{
				result.InitializeResult(
					DataErrorInfo.CreateError(activity, ExchangeConnectorUtilities.GetString("LE_CannotRemoveActivityWithoutDataItem")), 
					true);

				return result;
			}

			RemoveHelper helper = new RemoveHelper(connector, activity, result);
			ExchangeFolder folder = connector.GetOwningExchangeFolder(activity);

			folder.Service.DeleteItem(item, shouldDeleteAllTaskOccurrences,
				helper._onDeleteItemCompleted,
				helper._onError);

			return result;
		}

		#endregion  // RemoveActivity

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