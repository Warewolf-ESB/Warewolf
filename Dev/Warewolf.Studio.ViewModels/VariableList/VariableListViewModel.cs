using System;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.VariableList
{
    public class VariableListViewModel:BindableBase, IVariableListViewModel
    {
        IList<IVariableListViewScalar> _scalars;
        IList<IVariablelistViewRecordSet> _recordSets;
        IList<IDataExpression> _workflowExpressions;
        readonly IDatalistViewExpressionConvertor _convertor;
        bool _enabled;
        string _filterExpression;
        bool _asc = true;

        public VariableListViewModel(IList<IDataExpression> workflowExpressions, IDatalistViewExpressionConvertor convertor)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "workflowExpressions", workflowExpressions }, { "convertor", convertor } });
            _workflowExpressions = workflowExpressions;
            _convertor = convertor;
            RecordSets = new List<IVariablelistViewRecordSet>();
            Scalars = new List<IVariableListViewScalar>();
            CreateItemsToBindToFromToolExpressions();
            FilterCommand = new DelegateCommand(() => Filter(_filterExpression));
            DeleteUnusedCommand = new DelegateCommand(()=>ClearUnused(_workflowExpressions));
            SortCommand = new DelegateCommand(Sort);
        }

        void CreateItemsToBindToFromToolExpressions()
        {
            var expressions = _workflowExpressions.Select(_convertor.Create);
            foreach(var dataListViewItem in expressions)
            {
                if(dataListViewItem != null)
                {
                    if (dataListViewItem is IVariablelistViewRecordSet)
                    {
                        // ReSharper disable SuspiciousTypeConversion.Global
                        AddRecordSet(dataListViewItem as IVariablelistViewRecordSet);
                        
                    }
                    else if (dataListViewItem is IVariableListViewScalar)
                    {
                        AddScalar(dataListViewItem as IVariableListViewScalar);

                    }
                    else if (dataListViewItem is IVariableListViewColumn)
                    {
                        AddColumn(dataListViewItem as IVariableListViewColumn);
                        // ReSharper restore SuspiciousTypeConversion.Global
                    }
                    else
                    {
                        throw new WarewolfInvalidTypeException("An invalid type was passed to the Datalist view as a part of the datalist",null);
                    }
                }
            }

        }

        public void AddColumn(IVariableListViewColumn variableListViewColumn)
        {
            var recset = new DataListViewRecodSetViewModel(variableListViewColumn.RecordsetName, new List<IVariableListViewColumn> { variableListViewColumn },this);
            if(RecordSets.Any(a=>a.Name==recset.Name))
            {
                // ReSharper disable CSharpWarnings::CS0252
                RecordSets.First(a => a.Name == recset.Name).AddColumn(variableListViewColumn);
                // ReSharper restore CSharpWarnings::CS0252

            }
            else
            {
                RecordSets.Add(recset);
            }
        }



        /// <summary>
        /// Mark an Item as an Output
        /// </summary>
        /// <param name="item"></param>
        public void Delete(IVariableListViewItem item)
        {
            
        }

        public DelegateCommand FilterCommand { get; private set; }
        public DelegateCommand SortCommand { get; set; }
        public DelegateCommand DeleteUnusedCommand { get; set; }

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
        public IList<IVariableListViewScalar> Scalars
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
        public IList<IVariablelistViewRecordSet> RecordSets
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
        /// The Expression that is currently filtering the variable list
        /// </summary>
        public string FilterExpression
        {
            get
            {
                return _filterExpression;
            }
            set
            {
                OnPropertyChanged(()=>FilterExpression);
                _filterExpression = value;
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

            var allExpressions = RecordSets.SelectMany(a => a.Columns ?? new List<IVariableListViewColumn>()) as IEnumerable<IVariableListViewItem>;
            allExpressions = allExpressions.Union(Scalars);
            allExpressions = allExpressions.Union(RecordSets);
            var unusedviewExpressions =allExpressions.Except( expressions.Select(_convertor.Create)).ToArray();
            foreach (var dataListViewItem in unusedviewExpressions)
            {
                RemoveExpression(dataListViewItem);
            }
            _workflowExpressions = expressions;
            CreateItemsToBindToFromToolExpressions();
        }

        void RemoveExpression(IVariableListViewItem variableListViewItem)
        {
            if (Scalars.Contains(variableListViewItem))
            {
                Scalars.Remove(variableListViewItem as IVariableListViewScalar);
                OnPropertyChanged("Scalars");
            }
            if (variableListViewItem is IVariablelistViewRecordSet)
            {
                RecordSets.Remove(variableListViewItem as IVariablelistViewRecordSet);
                OnPropertyChanged("RecordSets");
            }
            if (variableListViewItem is IVariableListViewColumn)
            {
                var datalistViewRecordSet = RecordSets.FirstOrDefault(a => a.Name == variableListViewItem.Name);
                if(datalistViewRecordSet != null)
                {
                    datalistViewRecordSet.RemoveColumn(variableListViewItem as IVariableListViewColumn);
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
            if (_asc)
            {
                RecordSets = RecordSets.OrderBy(a => a.Name).ToList();
                Scalars = Scalars.OrderBy(a => a.Name).ToList();
            }
            else
            {
                RecordSets = RecordSets.OrderBy(a => a.Name).Reverse().ToList();
                Scalars = Scalars.OrderBy(a => a.Name).Reverse().ToList();
            }
            _asc = !_asc;
        }

        /// <summary>
        /// Filter by name. The variable contains the search expression 
        /// </summary>
        /// <param name="searchExpression"></param>
        public void Filter(string searchExpression)
        {
            RecordSets.ForEach(a => a.Visible = true);
            Scalars.ForEach(a => a.Visible = true);
            RecordSets.Where(a=>!a.Name.Contains(searchExpression)).ForEach(a=>a.Visible=false);
            Scalars.Where(a => !a.Name.Contains(searchExpression)).ForEach(a => a.Visible = false);
        }

        /// <summary>
        /// Add a scalar 
        /// </summary>
        /// <param name="scalar"></param>
        public void AddScalar(IVariableListViewScalar scalar)
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
        public void AddRecordSet(IVariablelistViewRecordSet recset)
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
