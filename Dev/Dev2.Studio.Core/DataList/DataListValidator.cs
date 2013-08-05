using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces.DataList;
using System.Xml;
using System.IO;

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

            //if (!string.IsNullOrEmpty(itemToMove.LastIndexedName))

            IndexedDataList.TryGetValue(itemToMove.Name, out matchingList);


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

            // Sashen.Naidoo
            // BUG 8556: The current datalist item was never being checked for validation after 
            //           it was added to the datalistValidator, this is merely to check if the current item
            //           has any duplicates and ammend the error status accordingly.

            UpdateValidationErrorsOnEntry(itemToMove);

            if (itemToMove.IsField)
                ValidateRecordSetChildren(itemToMove.Parent);
            else if (itemToMove.IsRecordset)
                ValidateRecordSetChildren(itemToMove);
        }

        public void Add(IDataListItemModel itemToAdd)
        {
            if (itemToAdd == null || String.IsNullOrWhiteSpace(itemToAdd.DisplayName)) return;

            //2013.04.10: Ashley Lewis - Bug 9168 Moved child validation to before parent validation (recordsets lose their errors during child validation)
            if(itemToAdd.IsField)
            {
                ValidateRecordSetChildren(itemToAdd.Parent);
            }
            else if(itemToAdd.IsRecordset)
            {
                ValidateRecordSetChildren(itemToAdd);
            }

            IList<IDataListItemModel> matchingList = null;

            IndexedDataList.TryGetValue(itemToAdd.Name, out matchingList);
            if (matchingList == null)
            {
                IndexedDataList.Add(itemToAdd.Name, new List<IDataListItemModel> {itemToAdd});
            }
            else
            {
                if (matchingList.Any(c => itemToAdd.IsRecordset == !c.IsRecordset))
                {
                    //2013.04.10: Ashley Lewis - Bug 9168 Not only are there duplicates detected but at least one of them is a different type to itemToAdd
                    matchingList.Add(itemToAdd);
                    foreach (IDataListItemModel item in matchingList)
                    {
                        item.SetError(item.IsRecordset ? StringResources.ErrorMessageDuplicateVariable : StringResources.ErrorMessageDuplicateRecordset);
                    }
                }
                else
                {
                    matchingList.Add(itemToAdd);
                    foreach (IDataListItemModel item in matchingList)
                    {
                        item.SetError(StringResources.ErrorMessageDuplicateValue);
                    }
                }
            }
        }

        public void Remove(IDataListItemModel itemToRemove)
        {
            if (itemToRemove == null) return;

            IList<IDataListItemModel> matchingList = null;

            IndexedDataList.TryGetValue(itemToRemove.LastIndexedName, out matchingList);

            if (matchingList != null)
            {
                matchingList.Remove(itemToRemove);
                ValidateDuplicats(matchingList);
                if (matchingList.Count == 0)
                {
                    IndexedDataList.Remove(itemToRemove.LastIndexedName);
                }
                UpdateValidationErrorsOnEntry(itemToRemove);
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
                    else if (item.HasError && !item.ErrorMessage.Equals(StringResources.ErrorMessageDuplicateValue))
                    {
                        item.RemoveError();
                    }
                    //item.DisplayName = item.DisplayName;
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
                if(parent.ErrorMessage != null && parent.ErrorMessage.Equals(StringResources.ErrorMessageEmptyRecordSet))
                {
                parent.RemoveError();
            }
        }
        }

        // Sashen.Naidoo
        // BUG 8556: The current datalist item was never being checked for validation after 
        //           it was added to the datalistValidator, this is merely to check if the current item
        //           has any duplicates and ammend the error status accordingly.
        private void UpdateValidationErrorsOnEntry(IDataListItemModel candidateEntry)
        {
            IList<IDataListItemModel> matches;
            IndexedDataList.TryGetValue(candidateEntry.Name, out matches);
            if (matches != null)
            {
                ValidateDuplicats(matches);
            }
            else if (candidateEntry.HasError && candidateEntry.ErrorMessage.Equals(StringResources.ErrorMessageDuplicateValue))
            {
                candidateEntry.RemoveError();
            }
            if (!string.IsNullOrEmpty(candidateEntry.Name))
            {
            try
            {
                if(!string.IsNullOrEmpty(candidateEntry.Name))
                {
                XmlConvert.VerifyName(candidateEntry.Name);
            }
                
            }
            catch (XmlException xex)
            {
                candidateEntry.SetError(StringResources.ErrorMessageInvalidChar);
            }
        }
        }
        #endregion Private Methods
    }
}