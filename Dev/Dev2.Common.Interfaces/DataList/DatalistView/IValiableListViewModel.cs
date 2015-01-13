using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IVariableListViewModel:IDisposable
    {
        /// <summary>
        /// The list of scalars visible in the designer variable list window
        /// </summary>
        IList<IDataListViewScalar> Scalars { get; }
        /// <summary>
        /// The list of record sets visible in the studio variable list
        /// </summary>
        IList<IDatalistViewRecordSet> RecordSets { get; }
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
        void AddScalar(IDataListViewScalar scalar);
        /// <summary>
        /// Add a recordset
        /// </summary>
        /// <param name="recset"></param>
        void AddRecordSet(IDatalistViewRecordSet recset);
        /// <summary>
        /// refresh from the selected workflow.
        /// </summary>
        /// <param name="expressions"></param>
        void Refresh(IList<IDataExpression> expressions);

        /// <summary>
        /// Add a column to the variable list. This should add the respective record set if it does not exist
        /// </summary>
        /// <param name="dataListViewColumn"></param>
        void AddColumn(IDataListViewColumn dataListViewColumn);

        /// <summary>
        /// Mark an Item as an input
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isInput"></param>
        void MarkAsInput(IDataListViewItem item,bool isInput);


        /// <summary>
        /// Mark an Item as an Output
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isInput"></param>
        void MarkAsOutput(IDataListViewItem item, bool isInput);


    }
}