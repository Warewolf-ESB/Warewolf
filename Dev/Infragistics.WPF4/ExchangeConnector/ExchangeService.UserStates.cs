using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Windows.Media;

namespace Infragistics.Controls.Schedules
{
	partial class ExchangeService
	{
		#region UserState class

		private class UserState
		{
			protected ErrorCallback _onError;
			protected readonly ExchangeService _service;

			public UserState(ExchangeService service, ErrorCallback onError)
			{
				_onError = onError;
				_service = service;
			}

			#region OnCallCancelled

			public void OnCallCancelled()
			{
				if (_onError != null)
					_onError(RemoteCallErrorReason.Cancelled, ResponseCodeType.NoError, null);
			}

			#endregion  // OnCallCancelled

			#region OnErrorOccurred

			public bool OnErrorOccurred(Exception error)
			{
				if (_onError != null)
					return _onError(RemoteCallErrorReason.Error, ResponseCodeType.NoError, error);

				return false;
			}

			#endregion  // OnErrorOccurred

			#region PreprocessServerResponse

			protected bool PreprocessServerResponse(AsyncCompletedEventArgs e)
			{
				if (e.Error != null)
				{
					this.OnErrorOccurred(e.Error);

					return false;
				}

				if (e.Cancelled)
				{
					this.OnCallCancelled();
					return false;
				}

				return true;
			}

			#endregion  // PreprocessServerResponse

			#region ProcessResponseCodeFromServer

			protected bool ProcessResponseCodeFromServer(ResponseMessageType responseMessage, out bool wasError)
			{
				wasError = false;

				if (responseMessage.ResponseCodeSpecified == false ||
					responseMessage.ResponseCode == ResponseCodeType.NoError)
				{
					return true;
				}

				wasError = true;

				bool ignoreError = _onError(RemoteCallErrorReason.Error, responseMessage.ResponseCode, new InvalidOperationException(responseMessage.MessageText));
				Debug.Assert(ignoreError, "Error from server: " + responseMessage.MessageText);
				return ignoreError;
			}

			#endregion  // ProcessResponseCodeFromServer

			#region ProcessServerResponse

			protected bool ProcessServerResponse<T, TMessage>(T response, out IList<TMessage> responseMessages)
				where T : BaseResponseMessageType
				where TMessage : ResponseMessageType
			{
				responseMessages = null;

				List<TMessage> responseMessageList = new List<TMessage>();

				foreach (ResponseMessageType baseResponseMessage in response.ResponseMessages.Items)
				{
					bool wasError;
					if (this.ProcessResponseCodeFromServer(baseResponseMessage, out wasError) == false)
						return false;

					// If there was an error with this response message, but the error handler wants to continue execution, 
					// just move to the next response message.
					if (wasError)
						continue;

					TMessage responseMessage = baseResponseMessage as TMessage;

					if (responseMessage == null)
					{
						ExchangeConnectorUtilities.DebugFail("Incorrect response message received.");
						return false;
					}

					responseMessageList.Add(responseMessage);
				}

				responseMessages = responseMessageList;
				return true;
			}

			#endregion  // ProcessServerResponse
		}

		#endregion  // UserState class


		#region CreateItemUserState class

		private class CreateItemUserState : UserState
		{
			private ItemType _item;
			private Action<ItemType> _onCreateItemsCompleted;

			public CreateItemUserState(ExchangeService service, ItemType item, Action<ItemType> onCreateItemsCompleted, ErrorCallback onError)
				: base(service, onError)
			{
				_item = item;
				_onCreateItemsCompleted = onCreateItemsCompleted;
			}

			public void ProcessResponse(CreateItemCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<ItemInfoResponseMessageType> itemInfoResponseMessages;
				if (this.ProcessServerResponse(e.Result, out itemInfoResponseMessages) == false)
					return;

				Debug.Assert(itemInfoResponseMessages.Count == 1, "Incorrect number of responses.");
				if (itemInfoResponseMessages.Count == 0)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorCreatingActivity")));
					return;
				}

				ItemInfoResponseMessageType itemInfo = itemInfoResponseMessages[0];

				ItemType[] committeditems = itemInfo.Items.Items;
				Debug.Assert(committeditems.Length == 1, "Incorrect number of items created responses.");
				if (committeditems.Length == 0)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorCreatingActivity")));
					return;
				}

				ItemType createdItem = _item;
				createdItem.ItemId = committeditems[0].ItemId;

				if (_onCreateItemsCompleted != null)
					_onCreateItemsCompleted(createdItem);
			}
		}

		#endregion  // CreateItemUserState class

		#region DeleteItemUserState class

		private class DeleteItemUserState : UserState
		{
			private Action _onDeleteItemsCompleted;

			public DeleteItemUserState(ExchangeService service, Action onDeleteItemsCompleted, ErrorCallback onError)
				: base(service, onError)
			{
				_onDeleteItemsCompleted = onDeleteItemsCompleted;
			}

			public void ProcessResponse(DeleteItemCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<ResponseMessageType> responseMessages;
				if (this.ProcessServerResponse(e.Result, out responseMessages) == false)
					return;

				if (_onDeleteItemsCompleted != null)
					_onDeleteItemsCompleted();
			}
		}

		#endregion  // DeleteItemUserState class

		#region DetermineBodyFormatUserState

		private class DetermineBodyFormatUserState : GetItemUserState
		{
			private Action<DescriptionFormat> _onBodyFormatDetermined;

			public DetermineBodyFormatUserState(ExchangeService service, ExchangeFolder folder, Action<DescriptionFormat> onBodyFormatDetermined, ErrorCallback onError)
				: base(service, folder, onError)
			{
				_onBodyFormatDetermined = onBodyFormatDetermined;
			}

			protected override void OnGetItemsCompleted(ExchangeFolder folder, IList<ItemType> items)
			{
				Debug.Assert(items.Count == 1, "There should be exactly one item returned from this call.");

				if (items.Count == 0)
				{
					if (_onError != null)
						_onError(RemoteCallErrorReason.Warning, ResponseCodeType.NoError, new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_CouldNotDetermineBodyFormat")));

					return;
				}

				BodyType body = items[0].Body;

				DescriptionFormat originalFormat;
				switch (body.BodyType1)
				{
					case BodyTypeType.HTML:
						originalFormat = DescriptionFormat.HTML;
						break;

					case BodyTypeType.Text:
						originalFormat = DescriptionFormat.Text;
						break;

					default:
						ExchangeConnectorUtilities.DebugFail("Unknown BodyTypeType: " + body.BodyType1);
						originalFormat = DescriptionFormat.HTML;
						break;
				}

				if (_onBodyFormatDetermined != null)
					_onBodyFormatDetermined(originalFormat);
			}
		}

		#endregion  // DetermineBodyFormatUserState

		#region FilterItemIdsByExpectedTypeUserState

		private class FilterItemIdsByExpectedTypeUserState : GetItemUserState
		{
			private Action<ExchangeFolder, IList<ItemType>> _onFilterItemIdsByExpectedTypeCompleted;

			public FilterItemIdsByExpectedTypeUserState(ExchangeService service, ExchangeFolder folder, Action<ExchangeFolder, IList<ItemType>> onFilterItemIdsByExpectedTypeCompleted, ErrorCallback onError)
				: base(service, folder, onError)
			{
				_onFilterItemIdsByExpectedTypeCompleted = onFilterItemIdsByExpectedTypeCompleted;
			}

			protected override void OnGetItemsCompleted(ExchangeFolder folder, IList<ItemType> items)
			{
				List<ItemType> filteredItemIds = new List<ItemType>();

				for (int i = 0; i < items.Count; i++)
				{
					ItemType item = items[i];

					if (folder.ShouldIncludeItem(item.ItemClass))
						filteredItemIds.Add(item);
				}

				if (_onFilterItemIdsByExpectedTypeCompleted != null)
					_onFilterItemIdsByExpectedTypeCompleted(folder, filteredItemIds);
			}
		}

		#endregion  // FilterItemIdsByExpectedTypeUserState

		#region FindFolderUserState class

		private abstract class FindFolderUserState : UserState
		{
			private bool _getAncestorChains;

			public FindFolderUserState(ExchangeService service, bool getAncestorChains, ErrorCallback onError)
				: base(service, onError)
			{
				_getAncestorChains = getAncestorChains;
			}

			protected abstract void OnFindFolderCompleted(List<BaseFolderType> folders, Dictionary<BaseFolderType, List<FolderIdType>> ancestorChains);

			public void ProcessResponse(FindFolderCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<FindFolderResponseMessageType> findFolderResponseMessages;
				if (this.ProcessServerResponse(e.Result, out findFolderResponseMessages) == false)
					return;

				foreach (FindFolderResponseMessageType findFolderResponseMessage in findFolderResponseMessages)
				{
					FindFolderParentType findFolderParent = findFolderResponseMessage.RootFolder;

					List<BaseFolderType> folders = new List<BaseFolderType>();
					for (int i = 0; i < findFolderParent.Folders.Length; i++)
					{
						BaseFolderType folder = findFolderParent.Folders[i];
						if (this.ShouldIncludeFolder(folder))
							folders.Add(folder);
					}

					Dictionary<BaseFolderType, List<FolderIdType>> ancestorChains = new Dictionary<BaseFolderType, List<FolderIdType>>();

					if (_getAncestorChains && folders.Count > 0)
						GetAncestorChainsHelper.Execute(this, folders, ancestorChains);
					else
						this.OnFindFolderCompleted(folders, ancestorChains);
				}
			}

			protected virtual bool ShouldIncludeFolder(BaseFolderType folder)
			{
				return true;
			}

			#region GetAncestorChainsHelper class

			private class GetAncestorChainsHelper
			{
				private Dictionary<BaseFolderType, List<FolderIdType>> _ancestorChains;
				private List<BaseFolderType> _folders;
				private ErrorCallback _onError;
				private Action<List<FolderAncestorInfo>> _onFindNextAncestorsComleted;
				private FindFolderUserState _owner;

				private GetAncestorChainsHelper(FindFolderUserState owner, List<BaseFolderType> folders, Dictionary<BaseFolderType, List<FolderIdType>> ancestorChains)
				{
					_ancestorChains = ancestorChains;
					_folders = folders;
					_owner = owner;

					_onError = this.OnError;
					_onFindNextAncestorsComleted = this.OnFindNextAncestorsComleted;
				}

				public static void Execute(FindFolderUserState owner, List<BaseFolderType> folders, Dictionary<BaseFolderType, List<FolderIdType>> ancestorChains)
				{
					GetAncestorChainsHelper helper = new GetAncestorChainsHelper(owner, folders, ancestorChains);
					helper.GetAncestorChains();
				}

				private void GetAncestorChains()
				{
					List<FolderAncestorInfo> folderAncestorInfos = new List<FolderAncestorInfo>();

					for (int i = 0; i < _folders.Count; i++)
					{
						BaseFolderType folder = _folders[i];
						FolderIdType highestAncestorId;
						if (this.HasFullAncestorChain(folder, out highestAncestorId) == false)
							folderAncestorInfos.Add(new FolderAncestorInfo(folder, highestAncestorId));
					}

					if (folderAncestorInfos.Count > 0)
						_owner._service.FindNextAncestors(folderAncestorInfos, _onFindNextAncestorsComleted, _onError);
					else
						_owner.OnFindFolderCompleted(_folders, _ancestorChains);
				}

				private FolderIdType GetHighestAncestor(BaseFolderType folder)
				{
					List<FolderIdType> ancestorChain;
					if (_ancestorChains.TryGetValue(folder, out ancestorChain) && ancestorChain.Count > 0)
						return ancestorChain[ancestorChain.Count - 1];

					return folder.ParentFolderId;
				}

				private bool HasFullAncestorChain(BaseFolderType folder, out FolderIdType highestAncestorId)
				{
					highestAncestorId = this.GetHighestAncestor(folder);
					return highestAncestorId.Id == _owner._service._msgFolderRootFolder.FolderId.Id;
				}

				private bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
				{
					if (serverResponseCode == ResponseCodeType.ErrorFolderNotFound)
						return true;

					// MD 10/21/11
					// Found while fixing TFS87807
					// The _onError member points back to this method, so this was causing a StackOverflowException.
					// Instead, we should be calling into the owner's error callback.
					//return _onError(reason, serverResponseCode, error);
					return _owner._onError(reason, serverResponseCode, error);
				}

				private void OnFindNextAncestorsComleted(List<FolderAncestorInfo> folderAncestorInfos)
				{
					bool addedAnyAncestor = false;
					for (int i = 0; i < folderAncestorInfos.Count; i++)
					{
						FolderAncestorInfo info = folderAncestorInfos[i];
						if (info.NextAncestorId == null)
						{
							_ancestorChains.Remove(info.Folder);
							continue;
						}

						List<FolderIdType> ancestorChain;
						if (_ancestorChains.TryGetValue(info.Folder, out ancestorChain) == false)
						{
							ancestorChain = new List<FolderIdType>();
							ancestorChain.Add(info.CurrentAncestorId);
							_ancestorChains[info.Folder] = ancestorChain;
						}

						if (info.NextAncestorId == null)
							continue;

						ancestorChain.Add(info.NextAncestorId);
						addedAnyAncestor = true;
					}

					if (addedAnyAncestor)
						this.GetAncestorChains();
					else
						_owner.OnFindFolderCompleted(_folders, _ancestorChains);
				}
			}

			#endregion  // GetAncestorChainsHelper class
		}

		#endregion  // FindFolderUserState class

		#region FindItemUserState class

		private abstract class FindItemUserState : UserState
		{
			protected FindItemUserState(ExchangeService service, ErrorCallback onError)
				: base(service, onError) { }

			protected abstract void OnFindItemCompleted(IList<ItemType> items);

			public void ProcessResponse(FindItemCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<FindItemResponseMessageType> findItemResponseMessages;
				if (this.ProcessServerResponse(e.Result, out findItemResponseMessages) == false)
					return;

				foreach (FindItemResponseMessageType findItemResponseMessage in findItemResponseMessages)
				{
					FindItemParentType findItemParent = findItemResponseMessage.RootFolder;
					ArrayOfRealItemsType itemsArray = findItemParent.Item as ArrayOfRealItemsType;

					if (itemsArray == null)
					{
						ExchangeConnectorUtilities.DebugFail("We could not find the array of found items.");
						continue;
					}

					ItemType[] items = itemsArray.Items;
					if (items == null)
						items = new ItemType[0];

					this.OnFindItemCompleted(items);
				}
			}
		}

		#endregion  // FindItemUserState class

		#region FindNextAncestorsUserState

		private class FindNextAncestorsUserState : GetFolderUserState
		{
			private List<FolderAncestorInfo> _folderAncestorInfos;
			private Action<List<FolderAncestorInfo>> _onFindNextAncestorsCompleted;

			public FindNextAncestorsUserState(
				ExchangeService service,
				List<FolderAncestorInfo> folderAncestorInfos,
				Action<List<FolderAncestorInfo>> onFindNextAncestorsCompleted,
				ErrorCallback onError)
				: base(service, onError)
			{
				_folderAncestorInfos = folderAncestorInfos;
				_onFindNextAncestorsCompleted = onFindNextAncestorsCompleted;
			}

			protected override void ProcessResponseMessages(IList<FolderInfoResponseMessageType> folderInfoResponseMessages)
			{
				for (int responseIndex = 0; responseIndex < folderInfoResponseMessages.Count; responseIndex++)
				{
					FolderInfoResponseMessageType response = folderInfoResponseMessages[responseIndex];

					Debug.Assert(response.Folders.Length == 1, "There should be one folder in the response.");
					if (response.Folders.Length == 0)
						continue;

					BaseFolderType folder = response.Folders[0];

					for (int pairIndex = 0; pairIndex < _folderAncestorInfos.Count; pairIndex++)
					{
						FolderAncestorInfo info = _folderAncestorInfos[pairIndex];
						if (info.CurrentAncestorId.Id == folder.FolderId.Id)
							info.NextAncestorId = folder.ParentFolderId;
					}
				}

				if (_onFindNextAncestorsCompleted != null)
					_onFindNextAncestorsCompleted(_folderAncestorInfos);
			}
		}

		#endregion  // FindNextAncestorsUserState

		#region FindRemindersFolderUserState

		private class FindRemindersFolderUserState : FindFolderUserState
		{
			public FindRemindersFolderUserState(ExchangeService service, ErrorCallback onError)
				: base(service, false, onError) { }

			protected override void OnFindFolderCompleted(List<BaseFolderType> folders, Dictionary<BaseFolderType, List<FolderIdType>> ancestorChains)
			{
				if (folders.Count == 0)
				{
					ExchangeConnectorUtilities.DebugFail("Could not find the reminders folder.");
					return;
				}

				SearchFolderType searchFolder = folders[0] as SearchFolderType;

				if (searchFolder == null)
				{
					ExchangeConnectorUtilities.DebugFail("Could not find the reminders folder.");
					return;
				}

				_service._remindersFolder = new ExchangeRemindersFolder(_service, searchFolder);

				List<Action> actions = _service._onFindRemindersFolderCompletedActions;

				if (actions != null)
				{
					_service._onFindRemindersFolderCompletedActions = null;

					for (int i = 0; i < actions.Count; i++)
						actions[i]();
				}
			}
		}

		#endregion  // FindRemindersFolderUserState

		#region FindRemindersUserState

		private class FindRemindersUserState : FindItemUserState
		{
			private List<ItemType> _allReminders;
			private bool _errorOccurred;
			private ErrorCallback _onErrorFromCaller;
			private Action<ExchangeFolder, IList<ItemType>> _onFindRemindersCompleted;
			private FindRemindersUserState[] _otherPendingCalls;
			private bool _receivedServerResponse;

			public FindRemindersUserState(
				ExchangeService service,
				List<ItemType> allReminders,
				Action<ExchangeFolder, IList<ItemType>> onFindRemindersCompleted,
				ErrorCallback onError)
				: base(service, null)
			{
				_allReminders = allReminders;
				_onErrorFromCaller = onError;
				_onFindRemindersCompleted = onFindRemindersCompleted;

				_onError = this.OnErrorCallback;
			}

			private bool DoOtherCallsHaveErrors()
			{
				if (_otherPendingCalls == null)
					return false;

				for (int i = 0; i < _otherPendingCalls.Length; i++)
				{
					if (_otherPendingCalls[i]._errorOccurred)
						return true;
				}

				return false;
			}

			private bool HaveAllCallsCompleted()
			{
				if (_receivedServerResponse == false)
					return false;

				if (_otherPendingCalls == null)
					return true;

				for (int i = 0; i < _otherPendingCalls.Length; i++)
				{
					if (_otherPendingCalls[i]._receivedServerResponse == false)
						return false;
				}

				return true;
			}

			private bool OnErrorCallback(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
			{
				_receivedServerResponse = true;

				if (this.DoOtherCallsHaveErrors())
					return false;

				_errorOccurred = true;

				if (_onErrorFromCaller != null)
					return _onErrorFromCaller(reason, serverResponseCode, error);

				return false;
			}

			protected override void OnFindItemCompleted(IList<ItemType> items)
			{
				_receivedServerResponse = true;

				if (this.DoOtherCallsHaveErrors())
					return;

				_allReminders.AddRange(items);

				if (this.HaveAllCallsCompleted())
					RemindersQueryHelper.FilterReminderItems(_service, _allReminders, _onFindRemindersCompleted, _onError);
			}

			public FindRemindersUserState[] OtherPendingCalls
			{
				get { return _otherPendingCalls; }
				set { _otherPendingCalls = value; }
			}
		}

		#endregion  // FindRemindersUserState

		#region GetAppointmentsInRangeUserState

		private class GetAppointmentsInRangeUserState : FindItemUserState
		{
			private List<ItemType> _existingItems;
			private ExchangeFolder _folder;
			private Action<ExchangeFolder, IList<ItemType>> _onGetAppointmentsInRangeCompleted;
			private Action<ExchangeFolder, IList<ItemType>> _onGetFullItemsCompleted;

			public GetAppointmentsInRangeUserState(ExchangeService service, ExchangeFolder folder, Action<ExchangeFolder, IList<ItemType>> onGetAppointmentsInRangeCompleted, ErrorCallback onError)
				: base(service, onError)
			{
				_folder = folder;
				_onGetAppointmentsInRangeCompleted = onGetAppointmentsInRangeCompleted;

				_onGetFullItemsCompleted = this.OnGetFullItemsCompleted;
			}

			protected override void OnFindItemCompleted(IList<ItemType> items)
			{
				List<BaseItemIdType> itemIds = new List<BaseItemIdType>();
				foreach (ItemType item in items)
				{
					ItemIdType itemId = item.ItemId;

					ActivityBase activity;
					if (_service.Connector.CachedActvities.TryGetValue(itemId.Id, out activity))
					{
						// If we already have the data item in memory and it has the same change key, we don't need to download it from the server again, 
						// because it will have the same data we have on the client.
						ItemType originalItem = activity.DataItem as ItemType;
						if (originalItem != null && originalItem.ItemId.ChangeKey == itemId.ChangeKey)
						{
							if (_existingItems == null)
								_existingItems = new List<ItemType>();

							_existingItems.Add(originalItem);
							continue;
						}
					}

					itemIds.Add(itemId);
				}

				if (itemIds.Count == 0)
				{
					if (_onGetFullItemsCompleted != null)
						_onGetFullItemsCompleted(_folder, new List<ItemType>());

					return;
				}

				_service.GetItems(_folder, itemIds, _onGetFullItemsCompleted, _onError);
			}

			private void OnGetFullItemsCompleted(ExchangeFolder folder, IList<ItemType> items)
			{
				IList<ItemType> allItems;
				if (_existingItems != null)
				{
					_existingItems.AddRange(items);
					allItems = _existingItems;
				}
				else
				{
					allItems = items;
				}

				if (_onGetAppointmentsInRangeCompleted != null)
					_onGetAppointmentsInRangeCompleted(folder, allItems);
			}
		}

		#endregion  // GetAppointmentsInRangeUserState

		#region GetCategoriesUserState

		private class GetCategoriesUserState : GetUserConfigurationUserState
		{
			// MD 6/8/11
			// Found while fixing TFS75934
			private ErrorCallback _onErrorFromCaller;

			private Action _onGetCategoriesCompleted;

			public GetCategoriesUserState(ExchangeService service, Action onGetCategoriesCompleted, ErrorCallback onError)
				// MD 6/8/11
				// Found while fixing TFS75934
				//: base(service, onError)
				: base(service, null)
			{
				_onGetCategoriesCompleted = onGetCategoriesCompleted;

				// MD 6/8/11
				// Found while fixing TFS75934
				_onErrorFromCaller = onError;
				_onError = this.ErrorCallback;
			}

			// MD 6/8/11
			// Found while fixing TFS75934
			// Some mailboxes may not have categories.
			private bool ErrorCallback(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
			{
				// Some resources may not have categories.
				if (serverResponseCode == ResponseCodeType.ErrorItemNotFound)
				{
					if (_onGetCategoriesCompleted != null)
						_onGetCategoriesCompleted();

					return true;
				}

				// MD 4/5/12 - TFS101459
				// This causes a StackOverflowException because we are calling into the same method here. We should be calling into
				// the error callback provided by the called.
				//return _onError(reason, serverResponseCode, error);
				return _onErrorFromCaller(reason, serverResponseCode, error);
			}

			protected override void ProcessResponseMessages(IList<GetUserConfigurationResponseMessageType> getUserConfigurationResponseMessages)
			{
				// MD 6/8/11
				// Found while fixing TFS75934
				// Some mailboxes may not have categories.
				//Debug.Assert(getUserConfigurationResponseMessages.Count == 1, "There should only be one response.");
				//if (getUserConfigurationResponseMessages.Count == 0)
				//{
				//    this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorGettingActivityCategories")));
				//    return;
				//}
				if (getUserConfigurationResponseMessages.Count == 0)
					return;

				UserConfigurationType userConfiguration = getUserConfigurationResponseMessages[0].UserConfiguration;
				byte[] xmlDataRaw = userConfiguration == null ? null : userConfiguration.XmlData;

				if (xmlDataRaw == null)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorGettingActivityCategories")));
					return;
				}

				Resource resource = _service._resource;

				List<ActivityCategory> existingCategories = new List<ActivityCategory>();
				if (resource.CustomActivityCategories != null)
				{
					existingCategories.AddRange(resource.CustomActivityCategories);
					resource.CustomActivityCategories.Clear();
				}

				using (MemoryStream stream = new MemoryStream(xmlDataRaw))
				using (XmlReader xmlReader = XmlReader.Create(stream))
				{
					_service._categoryList = CategoryList.Load(xmlReader);

					for (int i = 0; i < _service._categoryList.Categories.Count; i++)
					{
						Category exchangeCategory = _service._categoryList.Categories[i];

						ActivityCategory category = null;
						for (int j = 0; j < existingCategories.Count; j++)
						{
							ActivityCategory existingCategory = existingCategories[j];
							Category existingExchangeCategory = existingCategory.DataItem as Category;
							if (existingExchangeCategory == null)
								continue;

							if (exchangeCategory.Id == existingExchangeCategory.Id)
							{
								category = existingCategory;
								existingCategories.RemoveAt(j);
								break;
							}
						}

						if (category == null)
							category = new ActivityCategory();

						category.CategoryName = exchangeCategory.Name;

						if (exchangeCategory.ColorIndex < 0)
						{
							category.Color = null;
						}
						else if (ExchangeScheduleDataConnector.ExchangeCategoryColors.Length <= exchangeCategory.ColorIndex)
						{
							ExchangeConnectorUtilities.DebugFail("The color index is out of range.");
							category.Color = null;
						}
						else
						{
							category.Color = ExchangeScheduleDataConnector.ExchangeCategoryColors[exchangeCategory.ColorIndex];
						}

						category.DataItem = exchangeCategory;

						if (resource.CustomActivityCategories == null)
							resource.CustomActivityCategories = new ActivityCategoryCollection();

						resource.CustomActivityCategories.Add(category);
					}
				}

				if (_onGetCategoriesCompleted != null)
					_onGetCategoriesCompleted();
			}
		}

		#endregion //GetCategoriesUserState

		#region GetDefaultFoldersUserState

		private class GetDefaultFoldersUserState : GetFolderUserState
		{
			private Action<IList<ExchangeFolder>> _onGetNonDefaultFoldersCompleted;

			public GetDefaultFoldersUserState(ExchangeService service, ErrorCallback onError)
				: base(service, onError)
			{
				_onGetNonDefaultFoldersCompleted = this.OnGetNonDefaultFoldersCompleted;
			}

			protected override void ProcessResponseMessages(IList<FolderInfoResponseMessageType> folderInfoResponseMessages)
			{
				Debug.Assert(folderInfoResponseMessages.Count == 5, "We should have received three responses when getting the default folders.");
				if (folderInfoResponseMessages.Count == 0)
					return;

				for (int i = 0; i < folderInfoResponseMessages.Count; i++)
				{
					FolderInfoResponseMessageType response = folderInfoResponseMessages[i];

					switch (i)
					{
						case 0:
							CalendarFolderType defaultCalendarFolder = ExchangeService.GetFolderFromResponse<CalendarFolderType>(response);
							if (defaultCalendarFolder != null)
								_service._calendarFolder = new ExchangeCalendarFolder(_service, defaultCalendarFolder);
							break;

						case 1:
							_service._deletedItemsFolder = ExchangeService.GetFolderFromResponse<FolderType>(response);
							break;

						case 2:
							FolderType journalFolder = ExchangeService.GetFolderFromResponse<FolderType>(response);
							if (journalFolder != null)
								_service._journalFolder = new ExchangeJournalFolder(_service, journalFolder);
							break;

						case 3:
							_service._msgFolderRootFolder = ExchangeService.GetFolderFromResponse<FolderType>(response);
							break;

						case 4:
							TasksFolderType tasksFolder = ExchangeService.GetFolderFromResponse<TasksFolderType>(response);
							if (tasksFolder != null)
								_service._tasksFolder = new ExchangeTasksFolder(_service, tasksFolder);
							break;
					}
				}

				_service.GetNonDefaultFolders(_onGetNonDefaultFoldersCompleted, _onError);
			}

			private void OnGetNonDefaultFoldersCompleted(IList<ExchangeFolder> childFolders)
			{
				// Once we have loaded all non default calendars, load the reminders folder. This needs to be done after getting 
				// all the other folders so reminder items can be linked up with the object representing their parent folders.
				_service.FindRemindersFolder(_onError);

				List<Action<ExchangeService>> actions = _service._onGetDefaultFoldersCompletedActions;

				if (actions != null)
				{
					_service._onGetDefaultFoldersCompletedActions = null;

					for (int i = 0; i < actions.Count; i++)
						actions[i](_service);
				}
			}
		}

		#endregion  // GetDefaultFoldersUserState

		#region GetEventsUserState class

		private class GetEventsUserState : UserState
		{
			private Action<ServerChangeType, ExchangeFolder, ItemIdType> _onChangeDetected;
			protected Action _onServerResponseCompleted;

			public GetEventsUserState(ExchangeService service, Action<ServerChangeType, ExchangeFolder, ItemIdType> onChangeDetected, Action onServerResponseCompleted, ErrorCallback onError)
				: base(service, onError)
			{
				_onChangeDetected = onChangeDetected;
				_onServerResponseCompleted = onServerResponseCompleted;
			}

			public void ProcessResponse(GetEventsCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<GetEventsResponseMessageType> getEventsResponseMessages;
				if (this.ProcessServerResponse(e.Result, out getEventsResponseMessages) == false)
					return;

				bool hasMoreEvents = false;
				foreach (GetEventsResponseMessageType getEventsResponseMessage in getEventsResponseMessages)
				{
					NotificationType notification = getEventsResponseMessage.Notification;

					Debug.Assert(
						notification.SubscriptionId == _service._pullNotificationInfo.SubscriptionId,
						"This notification is for the wrong subscription type.");

					foreach (BaseNotificationEventType notificationEvent in notification.Items)
					{
						try
						{
							BaseObjectChangedEventType objectChangedNotificationEvent = notificationEvent as BaseObjectChangedEventType;

							if (objectChangedNotificationEvent == null)
								continue;

							// If a folder has changed, download the folder ID from the server again because it may have a new change key
							// but we can't use the ID here because the change key appears to be junk and causes an error when we try to do
							// operations with the folder later.
							FolderIdType folderId = objectChangedNotificationEvent.Item as FolderIdType;
							if (folderId != null)
							{
								_service.RefreshFolderId(folderId);
								continue;
							}

							ItemIdType itemId = objectChangedNotificationEvent.Item as ItemIdType;

							if (itemId == null)
								continue;

							FolderIdType parentFolderId = objectChangedNotificationEvent.ParentFolderId;
							ExchangeFolder folder = _service.FindFolderWithId(parentFolderId);

							CreatedEventType createdEvent = objectChangedNotificationEvent as CreatedEventType;
							if (createdEvent != null)
							{
								if (folder != null)
									_onChangeDetected(ServerChangeType.ItemAdded, folder, itemId);

								continue;
							}

							DeletedEventType deletedEvent = objectChangedNotificationEvent as DeletedEventType;
							if (deletedEvent != null)
							{
								if (folder != null)
									_onChangeDetected(ServerChangeType.ItemDeleted, folder, itemId);

								continue;
							}

							ModifiedEventType modifiedEvent = objectChangedNotificationEvent as ModifiedEventType;
							if (modifiedEvent != null)
							{
								if (folder != null)
									_onChangeDetected(ServerChangeType.ItemModified, folder, itemId);

								continue;
							}

							MovedEventType movedEvent = objectChangedNotificationEvent as MovedEventType;
							if (movedEvent != null)
							{
								Debug.Assert((folder is ExchangeRemindersFolder) == false, "Nothing should be moved to the reminder folder.");

								if (folder != null)
									_onChangeDetected(ServerChangeType.ItemAdded, folder, itemId);

								ItemIdType oldItemId = (ItemIdType)movedEvent.Item1;
								ExchangeFolder oldFolder = _service.FindFolderWithId(movedEvent.OldParentFolderId);

								if (oldFolder != null)
									_onChangeDetected(ServerChangeType.ItemDeleted, oldFolder, oldItemId);

								continue;
							}

							CopiedEventType copiedEvent = objectChangedNotificationEvent as CopiedEventType;
							if (createdEvent != null)
							{
								Debug.Assert((folder is ExchangeRemindersFolder) == false, "Nothing should be copied to the reminder folder.");

								if (folder != null)
									_onChangeDetected(ServerChangeType.ItemAdded, folder, itemId);

								continue;
							}

							ExchangeConnectorUtilities.DebugFail("Unknown notification event type.");
						}
						finally
						{
							_service._pullNotificationInfo.Watermark = notificationEvent.Watermark;
						}
					}

					hasMoreEvents |= notification.MoreEvents;
				}

				if (hasMoreEvents)
					_service.PollForChanges(_onChangeDetected, _onServerResponseCompleted, _onError);
				else if (_onServerResponseCompleted != null)
					_onServerResponseCompleted();
			}
		}

		#endregion  // GetEventsUserState class

		#region GetFolderUserState class

		private abstract class GetFolderUserState : UserState
		{
			protected GetFolderUserState(ExchangeService service, ErrorCallback onError)
				: base(service, onError) { }

			public void ProcessResponse(GetFolderCompletedEventArgs e)
			{
				_service._isGettingDefaultFolders = false;

				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<FolderInfoResponseMessageType> folderInfoResponseMessages;
				if (this.ProcessServerResponse(e.Result, out folderInfoResponseMessages) == false)
					return;

				this.ProcessResponseMessages(folderInfoResponseMessages);
			}

			protected abstract void ProcessResponseMessages(IList<FolderInfoResponseMessageType> folderInfoResponseMessages);
		}

		#endregion  // GetFolderUserState class

		#region GetItemUserState class

		private class GetItemUserState : UserState
		{
			private ExchangeFolder _folder;
			protected Action<ExchangeFolder, IList<ItemType>> _onGetItemsCompleted;

			protected GetItemUserState(ExchangeService service, ExchangeFolder folder, ErrorCallback onError)
				: base(service, onError)
			{
				_folder = folder;
			}

			public GetItemUserState(ExchangeService service, ExchangeFolder folder, Action<ExchangeFolder, IList<ItemType>> onGetItemsCompleted, ErrorCallback onError)
				: this(service, folder, onError)
			{
				_onGetItemsCompleted = onGetItemsCompleted;
			}

			protected virtual void OnGetItemsCompleted(ExchangeFolder folder, IList<ItemType> items)
			{
				if (_onGetItemsCompleted != null)
					_onGetItemsCompleted(_folder, items);
			}

			public void ProcessResponse(GetItemCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<ItemInfoResponseMessageType> itemInfoResponseMessages;
				if (this.ProcessServerResponse(e.Result, out itemInfoResponseMessages) == false)
					return;

				List<ItemType> allItems = new List<ItemType>();

				foreach (ItemInfoResponseMessageType itemInfoResponseMessage in itemInfoResponseMessages)
				{
					ItemType[] items = itemInfoResponseMessage.Items.Items;

					if (items == null)
						continue;

					allItems.AddRange(items);
				}

				this.OnGetItemsCompleted(_folder, allItems);
			}
		}

		#endregion  // GetItemUserState class

		#region GetNonDefaultFoldersUserState

		private class GetNonDefaultFoldersUserState : FindFolderUserState
		{
			private Action<IList<ExchangeFolder>> _onGetNonDefaultFoldersCompleted;

			public GetNonDefaultFoldersUserState(ExchangeService service, Action<IList<ExchangeFolder>> onGetNonDefaultFoldersCompleted, ErrorCallback onError)
				: base(service, true, onError)
			{
				_onGetNonDefaultFoldersCompleted = onGetNonDefaultFoldersCompleted;
			}

			private bool IsUnderDeletedItemsFolder(BaseFolderType folder, Dictionary<BaseFolderType, List<FolderIdType>> ancestorChains)
			{
				if (_service._deletedItemsFolder == null)
					return false;

				List<FolderIdType> ancestorChain;
				if (ancestorChains.TryGetValue(folder, out ancestorChain) == false)
					return false;

				for (int i = 0; i < ancestorChain.Count; i++)
				{
					FolderIdType ancestorId = ancestorChain[i];
					if (ancestorId.Id == _service._deletedItemsFolder.FolderId.Id)
						return true;
				}

				return false;
			}

			protected override void OnFindFolderCompleted(List<BaseFolderType> folders, Dictionary<BaseFolderType, List<FolderIdType>> ancestorChains)
			{
				for (int i = 0; i < folders.Count; i++)
				{
					BaseFolderType folder = folders[i];

					// If the folder was deleted, it will be a child of the deleted items folder and we should skip it.
					if (this.IsUnderDeletedItemsFolder(folder, ancestorChains))
						continue;

					ExchangeFolder exchangeFolder = EWSUtilities.CreateExchangeFolder(_service, folder);

					if (exchangeFolder != null)
					{
						if (_service._nonDefaultFolders == null)
							_service._nonDefaultFolders = new List<ExchangeFolder>();

						_service._nonDefaultFolders.Add(exchangeFolder);
					}
				}

				_service.SubscribeToPullNotifications(_onError);

				if (_onGetNonDefaultFoldersCompleted != null)
					_onGetNonDefaultFoldersCompleted(_service._nonDefaultFolders);
			}

			protected override bool ShouldIncludeFolder(BaseFolderType folder)
			{
				if (folder.FolderId.Id == _service.CalendarFolder.FolderId.Id ||
					folder.FolderId.Id == _service.JournalFolder.FolderId.Id ||
					folder.FolderId.Id == _service.TasksFolder.FolderId.Id)
				{
					return false;
				}

				if (folder is SearchFolderType)
					return false;

				return true;
			}
		}

		#endregion  // GetNonDefaultFoldersUserState

		#region GetUserAvailabilityUserState

		private class GetUserAvailabilityUserState : UserState
		{
			private Action<WorkingHours> _onGetUserAvailabilityCompleted;

			public GetUserAvailabilityUserState(ExchangeService service, Action<WorkingHours> onGetUserAvailabilityCompleted, ErrorCallback onError)
				: base(service, onError)
			{
				_onGetUserAvailabilityCompleted = onGetUserAvailabilityCompleted;
			}

			public void ProcessResponse(GetUserAvailabilityCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				List<WorkingHours> responseMessageList = new List<WorkingHours>();
				foreach (FreeBusyResponseType freeBusyResponse in e.Result.FreeBusyResponseArray)
				{
					bool wasError;
					if (this.ProcessResponseCodeFromServer(freeBusyResponse.ResponseMessage, out wasError) == false)
						return;

					// If there was an error with this response message, but the error handler wants to continue execution, 
					// just move to the next response message.
					if (wasError)
						continue;

					responseMessageList.Add(freeBusyResponse.FreeBusyView.WorkingHours);
				}

				Debug.Assert(responseMessageList.Count == 1, "There were too many free busy responses.");
				if (responseMessageList.Count == 0)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorGettingWorkingHours")));
					return;
				}

				if (_onGetUserAvailabilityCompleted != null)
					_onGetUserAvailabilityCompleted(responseMessageList[0]);
			}
		}

		#endregion  // GetUserAvailabilityUserState

		#region GetUserConfigurationUserState

		private abstract class GetUserConfigurationUserState : UserState
		{
			public GetUserConfigurationUserState(ExchangeService service, ErrorCallback onError)
				: base(service, onError) { }

			public void ProcessResponse(GetUserConfigurationCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<GetUserConfigurationResponseMessageType> getUserConfigurationResponseMessages;
				if (this.ProcessServerResponse(e.Result, out getUserConfigurationResponseMessages) == false)
					return;

				this.ProcessResponseMessages(getUserConfigurationResponseMessages);
			}

			protected abstract void ProcessResponseMessages(IList<GetUserConfigurationResponseMessageType> getUserConfigurationResponseMessages);
		}

		#endregion //GetUserConfigurationUserState

		#region MoveItemUserState class

		private class MoveItemUserState : UserState
		{
			private Action<ExchangeFolder, ItemType> _onMoveItemCompleted;
			private ExchangeFolder _toFolder;

			public MoveItemUserState(ExchangeService service, ExchangeFolder toFolder, Action<ExchangeFolder, ItemType> onMoveItemCompleted, ErrorCallback onError)
				: base(service, onError)
			{
				_onMoveItemCompleted = onMoveItemCompleted;
				_toFolder = toFolder;
			}

			private void OnGetMovedItemsCompleted(ExchangeFolder folder, IList<ItemType> items)
			{
				Debug.Assert(items.Count == 1, "There should only be one item here");
				if (items.Count == 0)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorMovingActivity")));
					return;
				}

				if (_onMoveItemCompleted != null)
					_onMoveItemCompleted(folder, items[0]);
			}

			public void ProcessResponse(MoveItemCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<ItemInfoResponseMessageType> moveItemResponseMessages;
				if (this.ProcessServerResponse(e.Result, out moveItemResponseMessages) == false)
					return;

				Debug.Assert(moveItemResponseMessages.Count == 1, "Incorrect number of responses.");
				if (moveItemResponseMessages.Count == 0)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorMovingActivity")));
					return;
				}

				ItemInfoResponseMessageType moveItemResponseMessage = moveItemResponseMessages[0];

				Debug.Assert(moveItemResponseMessage.Items.Items.Length == 1, "Incorrect number of items returned from the move item operation.");
				if (moveItemResponseMessage.Items.Items.Length == 0)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorMovingActivity")));
					return;
				}

				ItemType returnedItem = moveItemResponseMessage.Items.Items[0];

				_service.GetItems(_toFolder, new BaseItemIdType[] { returnedItem.ItemId },
					this.OnGetMovedItemsCompleted,
					_onError);
			}
		}

		#endregion  // MoveItemUserState class

		#region PollForChangesUserState

		private class PollForChangesUserState : GetEventsUserState
		{
			private ErrorCallback _onErrorFromCaller;

			public PollForChangesUserState(ExchangeService service, Action<ServerChangeType, ExchangeFolder, ItemIdType> onChangeDetected, Action onServerResponseCompleted, ErrorCallback onError)
				: base(service, onChangeDetected, onServerResponseCompleted, null)
			{
				_onErrorFromCaller = onError;

				_onError = this.OnErrorCallback;
			}

			private bool OnErrorCallback(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
			{
				// If the subscription id expired, re-subscribe and requery everything so we get any changes 
				// that occurred since the last polling.
				if (serverResponseCode == ResponseCodeType.ErrorExpiredSubscription ||
					serverResponseCode == ResponseCodeType.ErrorSubscriptionNotFound)
				{
					_service._connector.RequeryAll();
					_service.SubscribeToPullNotifications(_onError);

					if (_onServerResponseCompleted != null)
						_onServerResponseCompleted();

					return false;
				}

				if (_onErrorFromCaller == null)
					return false;

				return _onErrorFromCaller(reason, serverResponseCode, error);
			}
		}

		#endregion  // PollForChangesUserState

		#region RefreshFolderIdUserState

		private class RefreshFolderIdUserState : GetFolderUserState
		{
			private ExchangeFolder _folder;

			public RefreshFolderIdUserState(ExchangeService service, ExchangeFolder folder)
				: base(service, null)
			{
				_folder = folder;
			}

			protected override void ProcessResponseMessages(IList<FolderInfoResponseMessageType> folderInfoResponseMessages)
			{
				if (folderInfoResponseMessages.Count == 0)
					return;

				FolderInfoResponseMessageType response = folderInfoResponseMessages[0];
				if (response.Folders.Length == 0)
					return;

				_folder.Folder.FolderId = response.Folders[0].FolderId;
			}
		}

		#endregion //RefreshFolderIdUserState

		// MD 11/4/11 - TFS81088
		#region RefreshItemIdUserState

		private class RefreshItemIdUserState : GetItemUserState
		{
			private ItemType _item;

			public RefreshItemIdUserState(ExchangeService service, 
				ExchangeFolder folder, 
				ItemType item,
				Action<ExchangeFolder, IList<ItemType>> onGetItemsCompleted,
				ErrorCallback onError)
				: base(service, folder, onGetItemsCompleted, onError)
			{
				_item = item;
			}

			protected override void OnGetItemsCompleted(ExchangeFolder folder, IList<ItemType> items)
			{
				Debug.Assert(items.Count == 1, "There should be exactly one item returned from this call.");

				if (items.Count == 0)
				{
					if (_onError != null)
						_onError(RemoteCallErrorReason.Warning, ResponseCodeType.NoError, new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_CouldNotDetermineBodyFormat")));

					return;
				}

				Debug.Assert(_item.ItemId.Id == items[0].ItemId.Id, "The Id should not have changed. Only the ChangeKey can change.");
				_item.ItemId = items[0].ItemId;

				base.OnGetItemsCompleted(folder, items);
			}
		}

		#endregion //RefreshItemIdUserState

		#region ResolveNamesUserState class

		private abstract class ResolveNamesUserState : UserState
		{
			public ResolveNamesUserState(ExchangeService service, ErrorCallback onError)
				: base(service, onError) { }

			protected abstract void OnResolveNamesCompleted(ResolutionType[] resolutions);

			public void ProcessResponse(ResolveNamesCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<ResolveNamesResponseMessageType> resolveNamesResponseMessages;
				if (this.ProcessServerResponse(e.Result, out resolveNamesResponseMessages) == false)
					return;

				Debug.Assert(resolveNamesResponseMessages.Count == 1, "Incorrect number of responses.");
				if (resolveNamesResponseMessages.Count == 0)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorGettingUserInfo")));
					return;
				}

				ResolveNamesResponseMessageType resolveNamesResponseMessage = resolveNamesResponseMessages[0];
				Debug.Assert(resolveNamesResponseMessage.ResolutionSet.Resolution.Length == 1, "Incorrect number of resolutions.");
				if (resolveNamesResponseMessage.ResolutionSet.Resolution.Length == 0)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorGettingUserInfo")));
					return;
				}

				this.OnResolveNamesCompleted(resolveNamesResponseMessage.ResolutionSet.Resolution);
			}
		}

		#endregion  // ResolveNamesUserState class

		#region ResolveUserInfoUserState

		private class ResolveUserInfoUserState : ResolveNamesUserState
		{
			private ErrorCallback _onErrorFromCaller;
			private Action<string, string> _onResolveUserInfoCompleted;

			public ResolveUserInfoUserState(ExchangeService service, Action<string, string> onResolveUserInfoCompleted, ErrorCallback onError)
				: base(service, null)
			{
				_onErrorFromCaller = onError;
				_onResolveUserInfoCompleted = onResolveUserInfoCompleted;

				_onError = this.OnErrorCallback;
			}

			private bool OnErrorCallback(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
			{
				if (serverResponseCode == ResponseCodeType.ErrorNameResolutionNoResults)
				{
					if (_onResolveUserInfoCompleted != null)
						_onResolveUserInfoCompleted(null, null);

					return false;
				}

				if (_onErrorFromCaller == null)
					return false;

				return _onErrorFromCaller(reason, serverResponseCode, error);
			}

			protected override void OnResolveNamesCompleted(ResolutionType[] resolutions)
			{
				ResolutionType resolution = resolutions[0];
				if (resolution.Contact == null)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorGettingUserInfo")));
					return;
				}

				if (_onResolveUserInfoCompleted != null)
				{
					_onResolveUserInfoCompleted(
						resolution.Contact.DisplayName,
						resolution.Mailbox.EmailAddress);
				}
			}
		}

		#endregion  // ResolveUserInfoUserState

		#region SubscribeUserState class

		private class SubscribeUserState : UserState
		{
			public SubscribeUserState(ExchangeService service, ErrorCallback onError)
				: base(service, onError) { }

			public void ProcessResponse(SubscribeCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<SubscribeResponseMessageType> subscribeResponseMessages;
				if (this.ProcessServerResponse(e.Result, out subscribeResponseMessages) == false)
					return;

				Debug.Assert(subscribeResponseMessages.Count == 1, "We should have only received one response when subscribing to reminders.");
				if (subscribeResponseMessages.Count == 0)
					return;

				SubscribeResponseMessageType subscribeResponseMessage = subscribeResponseMessages[0];

				_service._pullNotificationInfo = new PullNotificationInfo(
					subscribeResponseMessage.SubscriptionId,
					subscribeResponseMessage.Watermark);
			}
		}

		#endregion  // SubscribeUserState class

		#region UnsubscribeUserState class

		private class UnsubscribeUserState : UserState
		{
			public UnsubscribeUserState(ExchangeService service, ErrorCallback onError)
				: base(service, onError) { }

			public void ProcessResponse(UnsubscribeCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<ResponseMessageType> unsubscribeResponseMessages;
				if (this.ProcessServerResponse(e.Result, out unsubscribeResponseMessages) == false)
					return;

				Debug.Assert(unsubscribeResponseMessages.Count == 1, "We should have only received one response when unsubscribing from reminders.");
				_service._pullNotificationInfo = null;
			}
		}

		#endregion  // UnsubscribeUserState class

		#region UpdateCategoriesUserState

		private class UpdateCategoriesUserState : UpdateUserConfigurationUserState
		{
			public Action _onUpdateCategoriesUserState;

			public UpdateCategoriesUserState(ExchangeService service, Action onUpdateCategoriesUserState, ErrorCallback onError)
				: base(service, onError) 
			{
				_onUpdateCategoriesUserState = onUpdateCategoriesUserState;
			}

			protected override void ProcessResponseMessages(IList<ResponseMessageType> responseMessages)
			{
				if (_onUpdateCategoriesUserState != null)
					_onUpdateCategoriesUserState();
			}
		} 

		#endregion //UpdateCategoriesUserState

		#region UpdateItemsUserState class

		private abstract class UpdateItemsUserState : UserState
		{
			private ItemType[] _items;

			public UpdateItemsUserState(ExchangeService service, ItemType[] items, ErrorCallback onError)
				: base(service, onError)
			{
				_items = items;
			}

			protected abstract void OnUpdateItemsCompleted(ItemType[] items);

			public void ProcessResponse(UpdateItemCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<UpdateItemResponseMessageType> updateItemResponseMessages;
				if (this.ProcessServerResponse(e.Result, out updateItemResponseMessages) == false)
					return;

				foreach (UpdateItemResponseMessageType updateItemResponseMessage in updateItemResponseMessages)
				{
					ItemType[] items = updateItemResponseMessage.Items.Items;
					ItemType[] updatedItems = _items;

					for (int i = 0; i < items.Length; i++)
						updatedItems[i].ItemId = items[i].ItemId;

					this.OnUpdateItemsCompleted(updatedItems);
				}
			}
		}

		#endregion  // UpdateItemsUserState class

		#region UpdateItemUserState

		private class UpdateItemUserState : UpdateItemsUserState
		{
			private Action<ItemType, bool> _onUpdateCalendarItemCompleted;

			public UpdateItemUserState(ExchangeService service, ItemType item, Action<ItemType, bool> onUpdateCalendarItemCompleted, ErrorCallback onError)
				: base(service, new ItemType[] { item }, onError)
			{
				_onUpdateCalendarItemCompleted = onUpdateCalendarItemCompleted;
			}

			protected override void OnUpdateItemsCompleted(ItemType[] items)
			{
				Debug.Assert(items.Length == 1, "Only one item should have been updated.");

				if (items.Length == 0)
				{
					this.OnErrorOccurred(new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorUpdatingActivity")));
					return;
				}

				ItemType updatedItem = items[0];

				if (_onUpdateCalendarItemCompleted != null)
					_onUpdateCalendarItemCompleted(updatedItem, true);
			}
		}

		#endregion  // UpdateItemUserState

		#region UpdateUserConfigurationUserState

		private abstract class UpdateUserConfigurationUserState : UserState
		{
			public UpdateUserConfigurationUserState(ExchangeService service, ErrorCallback onError)
				: base(service, onError) { }

			public void ProcessResponse(UpdateUserConfigurationCompletedEventArgs e)
			{
				if (this.PreprocessServerResponse(e) == false)
					return;

				IList<ResponseMessageType> responseMessages;
				if (this.ProcessServerResponse(e.Result, out responseMessages) == false)
					return;

				this.ProcessResponseMessages(responseMessages);
			}

			protected abstract void ProcessResponseMessages(IList<ResponseMessageType> responseMessages);
		} 

		#endregion //UpdateUserConfigurationUserState
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