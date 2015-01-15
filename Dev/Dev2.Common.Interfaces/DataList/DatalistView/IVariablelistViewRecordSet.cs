using System.Collections.Generic;
using Microsoft.Practices.Prism.Commands;

namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IVariablelistViewRecordSet : IVariableListViewItem
    {
        IList<IVariableListViewColumn> Columns { get; }
        void AddColumn(IVariableListViewColumn variableListViewColumn);
        void RemoveColumn(IVariableListViewColumn variableListViewColumn);
    }

    public interface IVariableListViewRecodSetViewModel : IVariableListViewItem
    {
        /// <summary>
        /// Command to Edit Noted
        /// </summary>
        DelegateCommand EditNotes { get; }
        /// <summary>
        /// Command to delete
        /// </summary>
        DelegateCommand Delete { get; }
        /// <summary>
        /// Is Delete Visible
        /// </summary>
        bool DeleteVisible { get;set; }
        /// <summary>
        /// Isinput chackbox visible
        /// </summary>
        bool InputVisible { get; set; }
        /// <summary>
        /// Its output chackbox visible
        /// </summary>
        bool OutputVisible { get; set; }
        /// <summary>
        /// Tooltip will be note or the validation message
        /// </summary>
        string ToolTip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        bool IsValid { get; set; }
    }
}