#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Studio.Interfaces.DataList
{
    public interface IDataListViewModel : IScreen, IChild, IDisposable, IEquatable<IDataListViewModel>
    {
        IResourceModel Resource { get; }
        IRelayCommand FindUnusedAndMissingCommand { get; }

        ObservableCollection<IScalarItemModel> ScalarCollection { get; }
        void Add(IScalarItemModel item);
        void Remove(IScalarItemModel item);
        int ScalarCollectionCount { get; }

        ObservableCollection<IRecordSetItemModel> RecsetCollection { get; }
        void Add(IRecordSetItemModel item);
        void Remove(IRecordSetItemModel item);
        int RecsetCollectionCount { get; }

        ObservableCollection<IComplexObjectItemModel> ComplexObjectCollection { get; }
        void Add(IComplexObjectItemModel item);
        void Remove(IComplexObjectItemModel item);
        int ComplexObjectCollectionCount { get; }

        ObservableCollection<IDataListItemModel> DataList { get; }
        bool HasErrors { get; }
        string DataListErrorMessage { get; }
        bool IsSorting { get; set; }
        ISuggestionProvider Provider { get; set; }
        bool ViewSortDelete { get; set; }

        /// <summary>
        /// Removes the data list item.
        /// </summary>
        /// <param name="itemToRemove">The item to remove.</param>
        /// <author>Massimo.Guerrera</author>
        /// <date>2/21/2013</date>
        void RemoveDataListItem(IDataListItemModel itemToRemove);
        
        void SetIsUsedDataListItems(IList<IDataListVerifyPart> parts, bool isUsed);
        
        void InitializeDataListViewModel(IResourceModel resourceModel);
        
        void AddBlankRow(IDataListItemModel item);
        
        void RemoveBlankRows(IDataListItemModel item);
        
        string WriteToResourceModel();
        
        void AddRecordsetNamesIfMissing();
        
        void AddMissingDataListItems(IList<IDataListVerifyPart> parts);
        
        void RemoveUnusedDataListItems();
        
        void ValidateVariableNamesForUI(IDataListItemModel item);

        List<IDataListVerifyPart> MissingWorkflowItems(IList<IDataListVerifyPart> partsToVerify);

        List<IDataListVerifyPart> MissingDataListParts(IList<IDataListVerifyPart> partsToVerify);

        List<IDataListVerifyPart> UpdateDataListItems(IResourceModel contextualResourceModel, IList<IDataListVerifyPart> workflowFields);
        
        void CreateListsOfIDataListItemModelToBindTo(out string errorString);

        void ClearCollections();

        void UpdateHelpDescriptor(string helpText);

        void GenerateComplexObjectFromJson(string parentObjectName, string json);
        void LogCustomTrackerEvent(string eventCategory, string eventName, string text);
    }
}
