using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Commands;

namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IVariableListViewModel:IDisposable
    {
        /// <summary>
        /// The list of scalars visible in the designer variable list window
        /// </summary>
        ICollection<IVariableListViewScalarViewModel> Scalars { get; }
        /// <summary>
        /// The list of record sets visible in the studio variable list
        /// </summary>
        ICollection<IVariablelistViewRecordSetViewModel> RecordSets { get; }

        /// <summary>
        /// The Expression that is currently filtering the variable list
        /// </summary>
        string FilterExpression { get; set; }
        /// <summary>
        /// Enabled. This is disable if not connected or if the user is not editing a workflow.
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// Remove Unused variables
        /// </summary>
        void ClearUnused(IList<IDataExpression> expressions);
        /// <summary>
        /// Sorts the list. The view model can be in a sorted asc, sorted desc or unsorted state
        /// </summary>
        /// 
        void Sort();
        /// <summary>
        /// Filter by name. The variable contains the search expression 
        /// </summary>
        /// <param name="searchExpression"></param>
        void Filter(string searchExpression);
        /// <summary>
        /// Add a scalar 
        /// </summary>
        /// <param name="scalar"></param>
        void AddScalar(IVariableListViewScalarViewModel scalar);
        /// <summary>
        /// Add a recordset
        /// </summary>
        /// <param name="recset"></param>
        void AddRecordSet(IVariablelistViewRecordSetViewModel recset);
        /// <summary>
        /// refresh from the selected workflow.
        /// </summary>
        /// <param name="expressions"></param>
        void Refresh(IList<IDataExpression> expressions);

        /// <summary>
        /// Add a column to the variable list. This should add the respective record set if it does not exist
        /// </summary>
        /// <param name="variableListViewColumn"></param>
        void AddColumn(IVariableListViewColumnViewModel variableListViewColumn);



        /// <summary>
        /// Delete an Item
        /// </summary>
        /// <param name="item"></param>

        void Delete(IVariableListViewItem item);

        /// <summary>
        /// View calls this command to Filter
        /// </summary>
        DelegateCommand FilterCommand { get; }

        /// <summary>
        /// View calls this command to Sort
        /// </summary>
        DelegateCommand SortCommand { get; }

        /// <summary>
        /// View calls this command to Delete Unused.
        /// </summary>
        DelegateCommand DeleteUnusedCommand { get; }

    }
}