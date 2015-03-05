
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Data.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Interfaces.DataList
{
    public interface IDataListViewModel : IDisposable
    {
        IResourceDefinition Resource { get; }
        RelayCommand FindUnusedAndMissingCommand { get; }

        ObservableCollection<IDataListItemModel> ScalarCollection { get; }

        ObservableCollection<IDataListItemModel> RecsetCollection { get; }

        ObservableCollection<IDataListItemModel> DataList { get; }
        bool HasErrors { get; }
        string DataListErrorMessage { get; }

        /// <summary>
        /// Removes the data list item.
        /// </summary>
        /// <param name="itemToRemove">The item to remove.</param>
        /// <author>Massimo.Guerrera</author>
        /// <date>2/21/2013</date>
        void RemoveDataListItem(IDataListItemModel itemToRemove);

        /// <summary>
        /// Sets the unused data list items.
        /// </summary>
        /// <param name="parts">The parts.</param>
        /// <param name="isUsed"></param>
        /// <author>Massimo.Guerrera</author>
        /// <date>2/20/2013</date>
        void SetIsUsedDataListItems(IList<IDataListVerifyPart> parts, bool isUsed);

        /// <summary>
        /// Initializes the data list view model.
        /// </summary>
        /// <param name="resourceModel">The resource model.</param>
        void InitializeDataListViewModel(IResourceModel resourceModel);

        void InitializeDataListViewModel();

        /// <summary>
        /// Adds the blank row.
        /// </summary>
        /// <param name="item">The item.</param>
        void AddBlankRow(IDataListItemModel item);

        /// <summary>
        /// Removes the blank rows.
        /// </summary>
        /// <param name="item">The item.</param>
        void RemoveBlankRows(IDataListItemModel item);

        /// <summary>
        /// Writes the data list to the resource model.
        /// </summary>
        string WriteToResourceModel();


        /// <summary>
        /// Adds the recordset names if missing.
        /// </summary>
        void AddRecordsetNamesIfMissing();

        /// <summary>
        /// Adds the missing data list items.
        /// </summary>
        /// <param name="parts">The parts.</param>
        void AddMissingDataListItems(IList<IDataListVerifyPart> parts);

        /// <summary>
        /// Removes the unused data list items.
        /// </summary>     
        void RemoveUnusedDataListItems();

        /// <summary>
        /// Validates the names.
        /// </summary>
        /// <param name="item">The item.</param>
        void ValidateNames(IDataListItemModel item);

        /// <summary>
        /// Finds the missing workflow data regions.
        /// </summary>
        /// <param name="partsToVerify">The parts to verify.</param>
        /// <param name="excludeUnusedItems"></param>
        /// <returns></returns>
        List<IDataListVerifyPart> MissingWorkflowItems(IList<IDataListVerifyPart> partsToVerify, bool excludeUnusedItems = false);

        List<IDataListVerifyPart> MissingDataListParts(IList<IDataListVerifyPart> partsToVerify);
        List<IDataListVerifyPart> UpdateDataListItems(IResourceModel contextualResourceModel, IList<IDataListVerifyPart> workflowFields);

        /// <summary>
        ///     Creates the list of data list item view model to bind to.
        /// </summary>
        /// <param name="errorString">The error string.</param>
        /// <returns></returns>
        void CreateListsOfIDataListItemModelToBindTo(out string errorString);

        void ClearCollections();
    }
}
