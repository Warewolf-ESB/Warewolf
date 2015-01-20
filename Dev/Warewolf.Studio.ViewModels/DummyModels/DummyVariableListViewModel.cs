//using System.Collections.Generic;
//using Dev2.Common.Interfaces.DataList.DatalistView;
//using Microsoft.Practices.Prism.Commands;
//using Warewolf.Studio.ViewModels.VariableList;

//namespace Warewolf.Studio.ViewModels.DummyModels
//{
//    public class DummyVariableListViewModel:IVariableListViewModel
//    {
//        IList<IVariableListViewScalarViewModel> _scalars;
//        IList<IVariablelistViewRecordSetViewModel> _recordSets;
//        string _filterExpression;
//        bool _enabled;
//        DelegateCommand _filterCommand;
//        DelegateCommand _sortCommand;
//        DelegateCommand _deleteUnusedCommand;

//        #region Implementation of IDisposable

//        public DummyVariableListViewModel()
//        {
//            _scalars = new IVariableListViewScalarViewModel[] { new VariableListItemViewScalarViewModel("bob", this, new List<IVariableListViewScalarViewModel>()), new VariableListItemViewScalarViewModel("mom", this, new List<IVariableListViewScalarViewModel>()) };
//            _recordSets = new IVariablelistViewRecordSetViewModel[] { new VariableListViewRecordSetViewModel("dave", new IVariableListViewColumnViewModel[0], this, new List<IVariableListItemViewModel>()), new VariableListViewRecordSetViewModel("gggg", new IVariableListViewColumn[0], this, new List<IVariableListItemViewModel>()) };
//        }

//        /// <summary>
//        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
//        /// </summary>
//        public void Dispose()
//        {
//        }

//        #endregion

//        #region Implementation of IVariableListViewModel

//        /// <summary>
//        /// The list of scalars visible in the designer variable list window
//        /// </summary>
//        public ICollection<IVariableListViewScalarViewModel> Scalars
//        {
//            get
//            {
//                return _scalars;
//            }
//        }
//        /// <summary>
//        /// The list of record sets visible in the studio variable list
//        /// </summary>
//        public ICollection<IVariablelistViewRecordSetViewModel> RecordSets
//        {
//            get
//            {
//                return _recordSets;
//            }
//        }
//        /// <summary>
//        /// The Expression that is currently filtering the variable list
//        /// </summary>
//        public string FilterExpression
//        {
//            get
//            {
//                return _filterExpression;
//            }
//            set
//            {
//                _filterExpression = value;
//            }
//        }
//        /// <summary>
//        /// Enabled. This is disable if not connected or if the user is not editing a workflow.
//        /// </summary>
//        public bool Enabled
//        {
//            get
//            {
//                return _enabled;
//            }
//            set
//            {
//                _enabled = value;
//            }
//        }

//        /// <summary>
//        /// Remove Unused variables
//        /// </summary>
//        public void ClearUnused(IList<IDataExpression> expressions)
//        {
//        }

//        /// <summary>
//        /// Sorts the list. The view model can be in a sorted asc, sorted desc or unsorted state
//        /// </summary>
//        /// 
//        public void Sort()
//        {
//        }

//        /// <summary>
//        /// Filter by name. The variable contains the search expression 
//        /// </summary>
//        /// <param name="searchExpression"></param>
//        public void Filter(string searchExpression)
//        {
//        }

//        /// <summary>
//        /// Add a scalar 
//        /// </summary>
//        /// <param name="scalar"></param>
//        public void AddScalar(IVariableListViewScalarViewModel scalar)
//        {
//        }

//        /// <summary>
//        /// Add a recordset
//        /// </summary>
//        /// <param name="recset"></param>
//        public void AddRecordSet(IVariablelistViewRecordSetViewModel recset)
//        {
//        }

//        /// <summary>
//        /// refresh from the selected workflow.
//        /// </summary>
//        /// <param name="expressions"></param>
//        public void Refresh(IList<IDataExpression> expressions)
//        {
//        }

//        /// <summary>
//        /// Add a column to the variable list. This should add the respective record set if it does not exist
//        /// </summary>
//        /// <param name="variableListViewColumn"></param>
//        public void AddColumn(IVariableListViewColumn variableListViewColumn)
//        {
//        }

//        /// <summary>
//        /// Delete an Item
//        /// </summary>
//        /// <param name="item"></param>
//        public void Delete(IVariableListViewItem item)
//        {
//        }

//        /// <summary>
//        /// View calls this command to Filter
//        /// </summary>
//        public DelegateCommand FilterCommand
//        {
//            get
//            {
//                return _filterCommand;
//            }
//        }
//        /// <summary>
//        /// View calls this command to Sort
//        /// </summary>
//        public DelegateCommand SortCommand
//        {
//            get
//            {
//                return _sortCommand;
//            }
//        }
//        /// <summary>
//        /// View calls this command to Delete Unused.
//        /// </summary>
//        public DelegateCommand DeleteUnusedCommand
//        {
//            get
//            {
//                return _deleteUnusedCommand;
//            }
//        }

//        #endregion
//    }
//}
