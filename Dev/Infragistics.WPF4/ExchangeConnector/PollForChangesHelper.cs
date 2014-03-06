using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules
{
	internal class PollForChangesHelper
	{
		#region Member Variables

		private Dictionary<ExchangeFolder, List<BaseItemIdType>> _addedOrModifiedItemIdsByFolder;
		private ExchangeScheduleDataConnector _connector;
		private Action<ExchangeFolder, IList<ItemType>> _getAddedOrModifiedItems;
		private ErrorCallback _onError;
		private ErrorCallback _onErrorFilteringAddedOrModifiedItems;
		private ErrorCallback _onErrorMain;
		private Action<ServerChangeType, ExchangeFolder, ItemIdType> _onItemChangeDetected;
		private Action _onPollForChangesCompleted;
		private Action _onPollForChangesResponseCompleted;
		private Action<ExchangeFolder, IList<ItemType>> _onReceivedItemsFromServer;
		private Action<ExchangeFolder, IList<ItemType>> _onReceivedOccurrencesFromServer;
		private Action _onRemoteCallProcessed;
		private int _pendingCallCount;

		#endregion  // Member Variables

		#region Constructor

		private PollForChangesHelper(ExchangeScheduleDataConnector connector, Action onPollForChangesCompleted)
		{
			_addedOrModifiedItemIdsByFolder = new Dictionary<ExchangeFolder, List<BaseItemIdType>>();
			_connector = connector;
			_onPollForChangesCompleted = onPollForChangesCompleted;

			_getAddedOrModifiedItems = this.GetAddedOrModifiedItems;
			_onError = this.OnError;
			_onErrorFilteringAddedOrModifiedItems = this.OnErrorFilteringAddedOrModifiedItems;
			_onErrorMain = this.OnErrorMain;
			_onItemChangeDetected = this.OnItemChangeDetected;
			_onReceivedItemsFromServer = this.OnReceivedItemsFromServer;
			_onReceivedOccurrencesFromServer = this.OnReceivedOccurrencesFromServer;
			_onRemoteCallProcessed = this.OnRemoteCallProcessed;
			_onPollForChangesResponseCompleted = this.OnPollForChangesResponseCompleted;
		}

		#endregion  // Constructor

		#region Methods

		#region Execute

		public static void Execute(ExchangeScheduleDataConnector connector, Action onPollForChangesCompleted)
		{
			if (connector.ExchangeServices == null || connector.ExchangeServices.Count == 0)
			{
				// MD 2/24/11 - TFS66949
				// If there are no connections to poll for changed, we are done polling, so call the callback.
				if (onPollForChangesCompleted != null)
					onPollForChangesCompleted();

				return;
			}

			int helpersPendingCount = 0;
			Action individualPollForChangesCompleted = () =>
			{
				helpersPendingCount--;
				if (helpersPendingCount == 0 && onPollForChangesCompleted != null)
					onPollForChangesCompleted();
			};

			foreach (ExchangeService exchangeService in connector.ExchangeServices)
			{
				helpersPendingCount++;
				PollForChangesHelper helper = new PollForChangesHelper(connector, individualPollForChangesCompleted);

				helper._pendingCallCount++;
				exchangeService.PollForChanges(
					helper._onItemChangeDetected,
					helper._onPollForChangesResponseCompleted,
					helper._onErrorMain);
			}
		}

		#endregion  // Execute

		#region GetAddedOrModifiedItems

		private void GetAddedOrModifiedItems(ExchangeFolder folder, IList<ItemType> addedOrModifiedItems)
		{
			IList<BaseItemIdType> itemIds;
			if (folder is ExchangeCalendarFolder)
			{
				bool didCategoriesChange = false;
				List<BaseItemIdType> temp = new List<BaseItemIdType>();
				for (int i = 0; i < addedOrModifiedItems.Count; i++)
				{
					ItemType item = addedOrModifiedItems[i];

					switch (item.ItemClass)
					{
						case ExchangeCalendarFolder.CategoryListClass:
							didCategoriesChange = true;
							break;

						case ExchangeCalendarFolder.ItemClass:
							temp.Add(item.ItemId);
							break;

						default:
							break;
					}
				}

				itemIds = temp;

				if (didCategoriesChange)
				{
					_pendingCallCount++;
					folder.Service.GetCategories(_onRemoteCallProcessed, _onError);
				}
			}
			else
			{
				itemIds = EWSUtilities.ConvertToItemIdList(addedOrModifiedItems);
			}

			_pendingCallCount++;
			folder.GetItems(itemIds, _onReceivedItemsFromServer, _onError);

			this.OnRemoteCallProcessed();
		}

		#endregion  // GetAddedOrModifiedItems

		#region OnError

		private bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			if (serverResponseCode == ResponseCodeType.ErrorItemNotFound)
				return true;

			this.OnRemoteCallProcessed();
			return _connector.DefaultErrorCallback(reason, serverResponseCode, error);
		} 

		#endregion  // OnError

		#region OnErrorFilteringAddedOrModifiedItems

		private bool OnErrorFilteringAddedOrModifiedItems(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			// If we can't find the item, just ignore it and skip to the next one. If it was a created item,
			// it must have been deleted since we got the notification and if it was a modified item, we will
			// get the delete notification shortly, so just ignore this error.
			if (serverResponseCode == ResponseCodeType.ErrorItemNotFound)
				return true;

			if (_onError == null)
				return false;

			return _onError(reason, serverResponseCode, error);
		}

		#endregion  // OnErrorFilteringAddedOrModifiedItems

		#region OnErrorMain

		private bool OnErrorMain(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			this.OnRemoteCallProcessed();
			return _connector.DefaultErrorCallback(reason, serverResponseCode, error);
		} 

		#endregion  // OnErrorMain

		#region OnItemChangeDetected

		internal void OnItemChangeDetected(ServerChangeType changeType, ExchangeFolder folder, ItemIdType itemId)
		{
			List<BaseItemIdType> addedOrModifiedItemIds;
			_addedOrModifiedItemIdsByFolder.TryGetValue(folder, out addedOrModifiedItemIds);

			switch (changeType)
			{
				case ServerChangeType.ItemAdded:
				case ServerChangeType.ItemModified:
					{
						if (addedOrModifiedItemIds == null)
						{
							addedOrModifiedItemIds = new List<BaseItemIdType>();
							_addedOrModifiedItemIdsByFolder.Add(folder, addedOrModifiedItemIds);
						}

						if (EWSUtilities.ListContainsId(addedOrModifiedItemIds, itemId) == false)
							addedOrModifiedItemIds.Add(itemId);
					}
					break;
				case ServerChangeType.ItemDeleted:
					{
						ActivityBase activity;
						if (_connector.CachedActvities.TryGetValue(itemId.Id, out activity))
							_connector.VerifyItemsInQueryResults(activity, false, folder.IsRemindersFolder);

						// If the items were added and removed in the same polling interval, remove the item from the 
						// addedOrModifiedItemIds collection so we don't try to get it back from the server.
						if (addedOrModifiedItemIds != null)
						{
							for (int i = addedOrModifiedItemIds.Count - 1; i >= 0; i--)
							{
								ItemIdType addedOrModifiedItemId = (ItemIdType)addedOrModifiedItemIds[i];
								if (addedOrModifiedItemId.Id == itemId.Id)
									addedOrModifiedItemIds.RemoveAt(i);
							}
						}
					}
					break;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown ServerChangeType: " + changeType);
					break;
			}
		}

		#endregion  // OnItemChangeDetected

		#region OnPollForChangesResponseCompleted

		internal void OnPollForChangesResponseCompleted()
		{
			// When we have finished getting the item ids of the changed items, get the the full items from the server.
			foreach (KeyValuePair<ExchangeFolder, List<BaseItemIdType>> pair in _addedOrModifiedItemIdsByFolder)
			{
				ExchangeFolder folder = pair.Key;

				_pendingCallCount++;
				folder.Service.FilterItemIdsByExpectedType(folder, pair.Value,
					_getAddedOrModifiedItems,
					_onErrorFilteringAddedOrModifiedItems);
			}

			_addedOrModifiedItemIdsByFolder.Clear();
			this.OnRemoteCallProcessed();
		}

		#endregion  // OnPollForChangesResponseCompleted

		#region OnReceivedItemsFromServer

		private void OnReceivedItemsFromServer(ExchangeFolder folder, IList<ItemType> items)
		{
			this.OnReceivedItemsFromServerHelper(folder, items);
			this.OnRemoteCallProcessed();
		}

		#endregion  // OnReceivedItemsFromServer

		#region OnReceivedItemsFromServerHelper

		private void OnReceivedItemsFromServerHelper(ExchangeFolder folder, IList<ItemType> items)
		{
			List<ActivityBase> recurringActivityRoots;

			// MD 5/26/11 - TFS75790
			// If we need to query root items for occurrences, the _pendingCallCount will be incremented, so we need _onRemoteCallProcessed in the end
			// so the _pendingCallCount is decremented again. Also, if there is an error, we want to make sure the _pendingCallCount is decremented as well.
			//_connector.OnItemsReceivedFromServer(folder, items, ref _pendingCallCount, out recurringActivityRoots);
			_connector.OnItemsReceivedFromServer(folder, items, ref _pendingCallCount, out recurringActivityRoots, _onRemoteCallProcessed, _onError);

			// If any recurring items were added or modified, we will need to requery all date ranges which might have 
			// occurrences in them.
			_pendingCallCount++;

			// MD 5/26/11
			// Found while fixing TFS75790
			// If there is an error, we want to make sure the _pendingCallCount is decremented.
			//_connector.OnRecurringItemsAddedOrModified(recurringActivityRoots, _onReceivedOccurrencesFromServer, _onRemoteCallProcessed, null);
			_connector.OnRecurringItemsAddedOrModified(recurringActivityRoots, _onReceivedOccurrencesFromServer, _onRemoteCallProcessed, _onError);
		} 

		#endregion //OnReceivedItemsFromServerHelper

		#region OnReceivedOccurrencesFromServer

		private void OnReceivedOccurrencesFromServer(ExchangeFolder folder, IList<ItemType> items)
		{
			this.OnReceivedItemsFromServerHelper(folder, items);
		} 

		#endregion //OnReceivedOccurrencesFromServer

		#region OnRemoteCallProcessed

		private void OnRemoteCallProcessed()
		{
			_pendingCallCount--;
			Debug.Assert(_pendingCallCount >= 0, "The pending call count should never be less than 0");

			if (_pendingCallCount == 0)
			{
				if (_onPollForChangesCompleted != null)
					_onPollForChangesCompleted();
			}
		}

		#endregion  // OnRemoteCallProcessed

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