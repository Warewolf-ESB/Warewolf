using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;

namespace Infragistics.Controls.Schedules
{
	internal class RemindersQueryHelper
	{
		private List<ItemType> _fullReminderItems;
		private int _numberOfGetItemsCallsPending;
		private ErrorCallback _onError;
		private Action<ExchangeFolder, IList<ItemType>> _onFilterItemsCompleted;
		private Action<ExchangeFolder, IList<ItemType>> _onFindRemindersCompleted;
		private Action<ExchangeFolder, IList<ItemType>> _onItemsReceived;
		private ExchangeService _service;

		private RemindersQueryHelper(ExchangeService service,
			Action<ExchangeFolder, IList<ItemType>> onFindRemindersCompleted,
			ErrorCallback onError)
		{
			_fullReminderItems = new List<ItemType>();
			_onError = onError;
			_onFindRemindersCompleted = onFindRemindersCompleted;
			_service = service;

			_onFilterItemsCompleted = this.OnFilterItemsCompleted;
			_onItemsReceived = this.OnItemsReceived;
		}

		public static void FilterReminderItems(ExchangeService service,
			IList<ItemType> reminderItems,
			Action<ExchangeFolder, IList<ItemType>> onFindRemindersCompleted,
			ErrorCallback onError)
		{
			Dictionary<ExchangeFolder, List<BaseItemIdType>> itemsToGetByFolder = new Dictionary<ExchangeFolder, List<BaseItemIdType>>();
			foreach (ItemType reminderItem in reminderItems)
			{
				ExchangeFolder folder = service.FindFolderWithId(reminderItem.ParentFolderId);

				if (folder == null)
					continue;

				List<BaseItemIdType> itemsToGet;
				if (itemsToGetByFolder.TryGetValue(folder, out itemsToGet) == false)
				{
					itemsToGet = new List<BaseItemIdType>();
					itemsToGetByFolder.Add(folder, itemsToGet);
				}

				if (itemsToGet.Contains(reminderItem.ItemId) == false)
					itemsToGet.Add(reminderItem.ItemId);
			}

			if (itemsToGetByFolder.Count == 0)
			{
				if (onFindRemindersCompleted != null)
					onFindRemindersCompleted(service.RemindersFolder, new List<ItemType>());

				return;
			}

			RemindersQueryHelper helper = new RemindersQueryHelper(service, onFindRemindersCompleted, onError);
			helper._numberOfGetItemsCallsPending = itemsToGetByFolder.Count;

			foreach (KeyValuePair<ExchangeFolder, List<BaseItemIdType>> pair in itemsToGetByFolder)
			{
				service.FilterItemIdsByExpectedType(pair.Key, pair.Value,
					helper._onFilterItemsCompleted,
					onError);
			}
		}

		private void OnFilterItemsCompleted(ExchangeFolder folder, IList<ItemType> filteredList)
		{
			folder.GetItems(EWSUtilities.ConvertToItemIdList(filteredList),
				_onItemsReceived,
				_onError);
		}

		private void OnItemsReceived(ExchangeFolder folder, IList<ItemType> items)
		{
			_fullReminderItems.AddRange(items);
			_numberOfGetItemsCallsPending--;

			if (_numberOfGetItemsCallsPending == 0)
			{
				if (_onFindRemindersCompleted != null)
					_onFindRemindersCompleted(_service.RemindersFolder, _fullReminderItems);
			}
		}
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