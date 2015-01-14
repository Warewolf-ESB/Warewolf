using System;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.VariableList
{
    public class VariableListViewModel:BindableBase, IVariableListViewModel
    {
        IList<IDataListViewScalar> _scalars;
        IList<IDatalistViewRecordSet> _recordSets;
        readonly IList<IDataExpression> _workflowExpressions;
        readonly IDatalistViewExpressionConvertor _convertor;
        bool _enabled;

        public VariableListViewModel(IList<IDataExpression> workflowExpressions, IDatalistViewExpressionConvertor convertor)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "workflowExpressions", workflowExpressions }, { "convertor", convertor } });
            _workflowExpressions = workflowExpressions;
            _convertor = convertor;
            CreateItemsToBindToFromToolExpressions();
            
        }

        void CreateItemsToBindToFromToolExpressions()
        {
            var expressions = _workflowExpressions.Select(_convertor.Create);
            foreach(var dataListViewItem in expressions)
            {
                if(dataListViewItem != null)
                {
                    var itemType = dataListViewItem.GetType();
                    if (itemType == typeof(IDatalistViewRecordSet))
                    {
                        // ReSharper disable SuspiciousTypeConversion.Global
                        AddRecordSet(itemType as IDatalistViewRecordSet);
                        
                    }
                    else if ((itemType == typeof(IDataListViewScalar)))
                    {
                        AddScalar(itemType as IDataListViewScalar);

                    }
                    else if ((itemType == typeof(IDataListViewColumn)))
                    {
                        AddColumn(itemType as IDataListViewColumn);
                        // ReSharper restore SuspiciousTypeConversion.Global
                    }
                    else
                    {
                        throw new WarewolfInvalidTypeException("An invalid type was passed to the Datalist view as a part of the datalist",null);
                    }
                }
            }

        }

        public void AddColumn(IDataListViewColumn dataListViewColumn)
        {
            var recset = new DataListViewRecordSetViewModel(dataListViewColumn.ColumnName, new List<IDataListViewColumn> { dataListViewColumn });
            if(RecordSets.Contains(recset))
            {
                RecordSets.First(a => a == recset).AddColumn(dataListViewColumn);

            }
            else
            {
                RecordSets.Add(recset);
            }
        }

        /// <summary>
        /// Mark an Item as an input
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isInput"></param>
        public void MarkAsInput(IDataListViewItem item, bool isInput)
        {
            item.Input = true;
        }

        /// <summary>
        /// Mark an Item as an Output
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isInput"></param>
        public void MarkAsOutput(IDataListViewItem item, bool isInput)
        {
            item.Output = true;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool b)
        {
        }

        #endregion

        #region Implementation of IVariableListViewModel

        /// <summary>
        /// The list of scalars visible in the designer variable list window
        /// </summary>
        public IList<IDataListViewScalar> Scalars
        {
            get
            {
                return _scalars;
            }
            private set
            {
                _scalars = value;
                OnPropertyChanged("Scalars");
            }
        }
        /// <summary>
        /// The list of record sets visible in the studio variable list
        /// </summary>
        public IList<IDatalistViewRecordSet> RecordSets
        {
            get
            {
                return _recordSets;
            }
            private set
            {
                
                _recordSets = value;
                OnPropertyChanged("RecordSets");
            }
        }
        /// <summary>
        /// Enabled. This is disable if not connected or if the user is not editing a workflow.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                OnPropertyChanged("Enabled");
                _enabled = value;
            }
        }

        /// <summary>
        /// Remove Unused variables
        /// </summary>
        public void ClearUnused(IList<IDataExpression> expressions)
        {

            var allExpressions = RecordSets.SelectMany(a => a.Columns) as IEnumerable<IDataListViewItem>;
            allExpressions = allExpressions.Union(Scalars);
            allExpressions = allExpressions.Union(RecordSets);
            var unusedviewExpressions =allExpressions.Except( expressions.Select(_convertor.Create));
            foreach (var dataListViewItem in unusedviewExpressions)
            {
                RemoveExpression(dataListViewItem);
            }
            CreateItemsToBindToFromToolExpressions();
        }

        void RemoveExpression(IDataListViewItem dataListViewItem)
        {
            if (Scalars.Contains(dataListViewItem))
            {
                Scalars.Remove(dataListViewItem as IDataListViewScalar);
                OnPropertyChanged("Scalars");
            }
            if (dataListViewItem is IDatalistViewRecordSet)
            {
                RecordSets.Remove(dataListViewItem as IDatalistViewRecordSet);
                OnPropertyChanged("RecordSets");
            }
            if (dataListViewItem is IDataListViewColumn)
            {
                var datalistViewRecordSet = RecordSets.FirstOrDefault(a => a.Name == dataListViewItem.Name);
                if(datalistViewRecordSet != null)
                {
                    datalistViewRecordSet.RemoveColumn(dataListViewItem as IDataListViewColumn);
                    OnPropertyChanged("RecordSets");
                }
            }
        }

        /// <summary>
        /// Sorts the list. The view model can be in a sorted asc, sorted desc or unsorted state
        /// </summary>
        /// 
        public void Sort()
        {
            RecordSets = RecordSets.OrderBy(a => a.Name).ToList();
            Scalars = Scalars.OrderBy(a => a.Name).ToList();
        }

        /// <summary>
        /// Filter by name. The variable contains the search expression 
        /// </summary>
        /// <param name="searchExpression"></param>
        public void Filter(string searchExpression)
        {
            RecordSets.Where(a=>!a.Name.Contains(searchExpression)).ForEach(a=>a.Visible=false);
        }

        /// <summary>
        /// Add a scalar 
        /// </summary>
        /// <param name="scalar"></param>
        public void AddScalar(IDataListViewScalar scalar)
        {
            if (!Scalars.Contains(scalar))
            {
                Scalars.Add(scalar);
                OnPropertyChanged("Scalars");
            }
        }
        /// <summary>
        /// Add a recordset
        /// </summary>
        /// <param name="recset"></param>
        public void AddRecordSet(IDatalistViewRecordSet recset)
        {
            if(!RecordSets.Contains(recset))
            {
                RecordSets.Add(recset);
                OnPropertyChanged("RecordSets");
            }
        }

        /// <summary>
        /// refresh from the selected workflow.
        /// </summary>
        /// <param name="expressions"></param>
        public void Refresh(IList<IDataExpression> expressions)
        {

        }

        #endregion
    }
}
