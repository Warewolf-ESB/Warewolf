using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.DataList
{
    public class DataListValidator : IDataListValidator
    {
        #region Ctor

        public DataListValidator()
        {
            IndexedDataList = new Dictionary<string, IList<IDataListItemModel>>();
        }

        #endregion Ctor

        #region Properties

        private Dictionary<string, IList<IDataListItemModel>> IndexedDataList { get; set; }

        #endregion Properties

        #region Methods

        public void Move(IDataListItemModel itemToMove)
        {
            if (itemToMove == null)
                return;

            IList<IDataListItemModel> matchingList = null;

            IndexedDataList.TryGetValue(itemToMove.LastIndexedName, out matchingList);

            if (matchingList != null)
            {
                matchingList.Remove(itemToMove);
                ValidateDuplicats(matchingList);
                Add(itemToMove);
                itemToMove.LastIndexedName = itemToMove.Name;
            }
            else
            {
                Add(itemToMove);
                itemToMove.LastIndexedName = itemToMove.Name;
            }

            if (itemToMove.IsField)
                ValidateRecordSetChildren(itemToMove.Parent);
            else if (itemToMove.IsRecordset)
                ValidateRecordSetChildren(itemToMove);
        }

        public void Add(IDataListItemModel itemToAdd)
        {
            if (itemToAdd == null || String.IsNullOrWhiteSpace(itemToAdd.Name)) return;


            IList<IDataListItemModel> matchingList = null;

            IndexedDataList.TryGetValue(itemToAdd.Name, out matchingList);
            if (matchingList == null)
            {
                IndexedDataList.Add(itemToAdd.Name, new List<IDataListItemModel> {itemToAdd});
            }
            else
            {
                matchingList.Add(itemToAdd);
                ValidateDuplicats(matchingList);
            }

            if (itemToAdd.IsField)
                ValidateRecordSetChildren(itemToAdd.Parent);
            else if (itemToAdd.IsRecordset)
                ValidateRecordSetChildren(itemToAdd);
        }

        public void Remove(IDataListItemModel itemToRemove)
        {
            if (itemToRemove == null) return;

            IList<IDataListItemModel> matchingList = null;

            IndexedDataList.TryGetValue(itemToRemove.Name, out matchingList);

            if (matchingList != null)
            {
                matchingList.Remove(itemToRemove);
                ValidateDuplicats(matchingList);
                if (matchingList.Count == 0)
                {
                    IndexedDataList.Remove(itemToRemove.Name);
                }
            }

            if (itemToRemove.IsField)
                ValidateRecordSetChildren(itemToRemove.Parent);
        }

        #endregion Methods

        #region Private Methods

        private void ValidateDuplicats(IList<IDataListItemModel> itemsToValidate)
        {
            if (itemsToValidate != null)
            {
                if (itemsToValidate.Count > 1)
                {
                    foreach (IDataListItemModel item in itemsToValidate)
                    {
                        item.SetError(StringResources.ErrorMessageDuplicateValue);
                    }
                }
                else if (itemsToValidate.Count == 1)
                {
                    IDataListItemModel item = itemsToValidate[0];
                    if (item.HasError && !item.ErrorMessage.Equals(StringResources.ErrorMessageInvalidChar))
                    {
                        item.RemoveError();
                    }
                    item.DisplayName = item.DisplayName;
                }
            }
        }

        /// <summary>
        ///     Validates the record set children.
        /// </summary>
        public void ValidateRecordSetChildren(IDataListItemModel parent)
        {
            //Only applicable for recordsets
            if (!parent.IsRecordset) return;

            //If DisplayName is empty do not validate
            if (String.IsNullOrWhiteSpace(parent.DisplayName)) return;

            //If no children - show error (This situation should not occur, there is always at least one empty one
            if (parent.Children.Count == 0 || parent.Children == null)
            {
                parent.SetError(StringResources.ErrorMessageEmptyRecordSet);
            }
                //Check that there is at least one child
            else if (parent.Children.Count == 1 && String.IsNullOrWhiteSpace(parent.Children.First().DisplayName))
            {
                parent.SetError(StringResources.ErrorMessageEmptyRecordSet);
            }
            else
            {
                parent.RemoveError();
            }
        }
        #endregion Private Methods
    }
}