using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules
{
	internal class EndEditForActivitiesHelper
	{
		#region Member Variables

		private ActivityBase _activity;
		private Action _completeEditOperation;
		private ExchangeScheduleDataConnector _connector;
		private ItemType _originalItem;
		private ErrorCallback _onError;
		private Action<ItemType> _onItemCreated;
		private Action<ItemType> _onItemCreatedInNewUsersFolder;
		private Action<ExchangeFolder, ItemType> _onItemMovedToNewFolder;
		private Action<ItemType, bool> _onItemUpdatedOrCreated;
		private Action<ExchangeFolder, IList<ItemType>> _onReceivedItemsFromServer;
		private ActivityOperationResult _result;
		private ItemType _updateItem;

		#endregion  // Member Variables

		#region Constructor

		private EndEditForActivitiesHelper(ExchangeScheduleDataConnector connector, ActivityBase activity)
		{
			_activity = activity;
			_connector = connector;

			_completeEditOperation = this.CompleteEditOperation;
			_onError = this.OnError;
			_onItemCreated = this.OnItemCreated;
			_onItemCreatedInNewUsersFolder = this.OnItemCreatedInNewUsersFolder;
			_onItemMovedToNewFolder = this.OnItemMovedToNewFolder;
			_onItemUpdatedOrCreated = this.OnItemUpdatedOrCreated;
			_onReceivedItemsFromServer = this.OnReceivedItemsFromServer;
		}

		#endregion  // Constructor

		#region Methods

		#region AddActivityToFolder

		private void AddActivityToFolder(ExchangeFolder owningFolder)
		{
			ItemType item = EWSUtilities.ItemFromActivity(_activity, _connector);

			_activity.DataItem = item;

			owningFolder.CreateItem(item, _onItemCreated, _onError);
		}

		#endregion  // AddActivityToFolder

		#region CompleteEditOperation

		private void CompleteEditOperation()
		{
			this.CompleteEditOperation(true);
		}

		private void CompleteEditOperation(bool hadChanges)
		{
			bool isAddNew = _activity.IsAddNew;

			// If the item was updated (not added), and it is an occurrence, it is now a variance.
			if (hadChanges && isAddNew == false && _activity.IsOccurrence)
				_activity.IsVariance = true;

			bool wasRecurringMaster = false;
			if (_activity.BeginEditData != null)
				wasRecurringMaster = ExchangeConnectorUtilities.IsRecurringMaster(_activity.BeginEditData);

			_activity.ClearBeginEditData();
			_result.InitializeResult(null, true);

			// Either it's an add or an update. The value should never be set to False.
			bool? isAdd = isAddNew ? true : (bool?)null;
			_connector.VerifyItemsInQueryResults(_activity, isAdd, false, wasRecurringMaster, true);
		}

		#endregion  // CompleteEditOperation

		#region EndEdit

		private ActivityOperationResult EndEdit()
		{
			_result = new ActivityOperationResult(_activity);

			bool activityIsAddNew = _activity.IsAddNew;
			ActivityOperation operation = activityIsAddNew ? ActivityOperation.Add : ActivityOperation.Edit;

			if (ListScheduleDataConnectorUtilities<ActivityBase>.CheckItemOperationAllowed(_connector, _result, operation) == false)
				return _result;

			ExchangeFolder owningFolder = _connector.GetOwningExchangeFolder(_activity);

			if (activityIsAddNew)
			{
				this.AddActivityToFolder(owningFolder);
			}
			else
			{
				_updateItem = (ItemType)_activity.DataItem;
				EWSUtilities.UpdateItem(_updateItem, _activity, _connector);

				_originalItem = (ItemType)_activity.BeginEditData.DataItem;
				ExchangeFolder originalOwningFolder = _connector.GetOwningExchangeFolder(_activity.BeginEditData);

				// Update the query results before sending to the server. 
				// Otherwise, activities may jump around when dragging them (especially between days or calendars).
				_connector.VerifyItemsInQueryResults(_activity, null, false);

				if (owningFolder == originalOwningFolder)
				{
					owningFolder.Service.UpdateItem(owningFolder, _updateItem, _originalItem, _onItemUpdatedOrCreated, _onError);
				}
				else
				{
					if (owningFolder.Service == originalOwningFolder.Service)
						owningFolder.Service.MoveItem(owningFolder, _updateItem, _onItemMovedToNewFolder, _onError);
					else
						owningFolder.CreateItem(_updateItem, _onItemCreatedInNewUsersFolder, _onError);
				}
			}

			return _result;
		}	

		#endregion  // EndEdit

		#region Execute

		public static ActivityOperationResult Execute(ExchangeScheduleDataConnector connector, ActivityBase activity)
		{
			EndEditForActivitiesHelper helper = new EndEditForActivitiesHelper(connector, activity);
			return helper.EndEdit();
		} 

		#endregion  // Execute

		#region OnError

		private bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			if (_updateItem != null && serverResponseCode == ResponseCodeType.ErrorIrresolvableConflict)
			{
				error = new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_NewerVersionOfActivityOnServer"));
				ExchangeFolder owningFolder = _connector.GetOwningExchangeFolder(_activity);
				owningFolder.GetItems(new BaseItemIdType[] { _updateItem.ItemId }, _onReceivedItemsFromServer, null);
			}

			// MD 2/24/11 - TFS66914
			// If there was an error when committing the change to the server, cancel the edit so we 
			// revert back to the original data item.
			DataErrorInfo errorInfo1;
			_connector.CancelEdit(_activity, out errorInfo1);

			// MD 3/9/11 - TFS66914
			// Once we cancel the edit operation, the start and end times could be modified, so re-verify it in all query result.
			_connector.VerifyItemsInQueryResults(_activity, null, false);

			DataErrorInfo errorInfo2 = _connector.GetDataErrorInfo(reason, error, _activity);

			if (errorInfo1 != null)
				errorInfo2 = DataErrorInfo.CreateFromList(new List<DataErrorInfo>() { errorInfo1, errorInfo2 });

			_result.InitializeResult(errorInfo2, true);
			return false;
		}

		#endregion  // OnError

		#region OnItemCreated

		private void OnItemCreated(ItemType item)
		{
			this.OnItemUpdatedOrCreated(item, true);
		}

		#endregion  // OnItemCreated

		#region OnItemCreatedInNewUsersFolder

		private void OnItemCreatedInNewUsersFolder(ItemType createdItem)
		{
			ExchangeFolder originalOwningFolder = _connector.GetOwningExchangeFolder(_activity.BeginEditData);
			bool deleteAll = ExchangeConnectorUtilities.ShouldDeleteAllTaskOccurrences(_activity);

			originalOwningFolder.Service.DeleteItem(_originalItem, deleteAll,
				() => this.OnItemCreated(createdItem),
				_onError);
		} 

		#endregion  // OnItemCreatedInNewUsersFolder

		#region OnItemMovedToNewFolder

		private void OnItemMovedToNewFolder(ExchangeFolder folder, ItemType movedItem)
		{
			// The moved item will have a new id, so copy the id over to the updated item before trying to update it.
			_updateItem.ItemId = movedItem.ItemId;

			// In this case, the moved item acts as the original item because after the move, it still has all the 
			// properties of the original item.
			folder.Service.UpdateItem(folder, _updateItem, movedItem, _onItemUpdatedOrCreated, _onError);
		}

		#endregion  // OnItemMovedToNewFolder

		#region OnItemUpdatedOrCreated

		private void OnItemUpdatedOrCreated(ItemType item, bool hadChanges)
		{
			if (_activity.Id != null && _activity.Id != item.ItemId.Id)
				_connector.CachedActvities.Remove(_activity.Id);

			_activity.DataItem = item;
			_activity.Id = item.ItemId.Id;

			// Now that we have the ID, add it to the cached activities collection.
			_connector.CachedActvities[_activity.Id] = _activity;

			if (hadChanges && _activity.IsRecurrenceRoot)
			{
				QueryOccurrencesHelper.Execute(_connector, new List<ActivityBase>() { _activity },
					_completeEditOperation,
					_onError);
			}
			// MD 11/4/11 - TFS81088
			// When an occurrence is updated, the recurrence root will have a new change key in its Id on the server, 
			// so we should re-get the Id to make sure subsequent updates to the root activity do not cause an error.
			else if (hadChanges && _activity.RootActivity != null)
			{
				ExchangeFolder owningFolder = _connector.GetOwningExchangeFolder(_activity);
				ItemType rootActivityItem = _activity.RootActivity.DataItem as ItemType;

				if (rootActivityItem != null)
				{
					owningFolder.Service.RefreshItemId(
						owningFolder, 
						rootActivityItem, 
						(folder, items) => this.CompleteEditOperation(hadChanges), 
						_onError);
				}
				else
				{
					ExchangeConnectorUtilities.DebugFail("The root activity should have an item here.");
					this.CompleteEditOperation(hadChanges);
				}
			}
			// ------------- End of TFS81088 fix -----------
			else
			{
				this.CompleteEditOperation(hadChanges);
			}
		}

		#endregion  // OnItemUpdatedOrCreated

		#region OnReceivedItemsFromServer

		private void OnReceivedItemsFromServer(ExchangeFolder folder, IList<ItemType> items)
		{
			int temp = 0;
			List<ActivityBase> recurringActivityRoots;
			_connector.OnItemsReceivedFromServer(folder, items, ref temp, out recurringActivityRoots);

			_connector.OnRecurringItemsAddedOrModified(recurringActivityRoots, _onReceivedItemsFromServer, null, null);
		} 

		#endregion // OnReceivedItemsFromServer

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