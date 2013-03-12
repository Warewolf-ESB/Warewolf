using Caliburn.Micro;
using Dev2.DataList.Contract;
using System.Collections.Generic;

namespace Dev2.Studio.Core.Interfaces.DataList
{
    public interface IDataListViewModel : IScreen
    {
        IResourceModel Resource { get; }

        OptomizedObservableCollection<IDataListItemModel> ScalarCollection { get; set; }

        OptomizedObservableCollection<IDataListItemModel> RecsetCollection { get; set; }

        OptomizedObservableCollection<IDataListItemModel> DataList { get; }

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
        /// <author>Massimo.Guerrera</author>
        /// <date>2/20/2013</date>
        void SetUnusedDataListItems(IList<IDataListVerifyPart> parts);

        /// <summary>
        /// Initializes the data list view model.
        /// </summary>
        /// <param name="resourceModel">The resource model.</param>
        void InitializeDataListViewModel(IResourceModel resourceModel);

        /// <summary>
        /// Filters the list.
        /// </summary>    
        void FilterItems();

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
        /// Checks the name.
        /// </summary>
        /// <param name="item">The item.</param>
        void CheckName(IDataListItemModel item);

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
        /// <param name="parts">The parts.</param>
        void RemoveUnusedDataListItems(IList<IDataListVerifyPart> parts);

        /// <summary>
        /// Validates the names.
        /// </summary>
        /// <param name="item">The item.</param>
        void ValidateNames(IDataListItemModel item);
    }
}
