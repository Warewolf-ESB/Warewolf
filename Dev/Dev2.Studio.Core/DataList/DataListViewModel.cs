
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Core.ViewModels.Base;
using ServiceStack.Common.Extensions;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.DataList
{
    public class DataListViewModel : BaseViewModel, IDataListViewModel, IUpdatesHelp
    {
        #region Fields

        private DelegateCommand _addRecordsetCommand;
        private ObservableCollection<DataListHeaderItemModel> _baseCollection;
        private RelayCommand _findUnusedAndMissingDataListItems;
        private ObservableCollection<IDataListItemModel> _recsetCollection;
        private ObservableCollection<IDataListItemModel> _scalarCollection;
        private string _searchText;
        private RelayCommand _sortCommand;
        private bool _viewSortDelete;

        #endregion Fields

        #region Properties

        public bool CanSortItems
        {
            get
            {
                return HasItems();
            }
        }

        public ObservableCollection<DataListHeaderItemModel> BaseCollection
        {
            get { return _baseCollection; }
            set
            {
                _baseCollection = value;
                NotifyOfPropertyChange(() => BaseCollection);
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                FilterItems();
                NotifyOfPropertyChange(() => SearchText);
            }
        }

        public bool ViewSortDelete
        {
            get { return _viewSortDelete; }
            set
            {
                _viewSortDelete = value;
                NotifyOfPropertyChange(() => ViewSortDelete);
            }
        }

        public IResourceModel Resource { get; private set; }

        public ObservableCollection<IDataListItemModel> DataList
        {
            get { return CreateFullDataList(); }
        }
        public bool HasErrors
        {
            get
            {
                if(DataList == null || DataList.Count == 0)
                {
                    return false;
                }
                return DataList.Any(model =>
                {
                    if(RecordSetHasChildren(model))
                    {
                        return model.HasError || model.Children.Any(child => child.HasError);
                    }
                    return model.HasError;
                });
            }
        }

        public ObservableCollection<IDataListItemModel> ScalarCollection
        {
            get
            {
                if(_scalarCollection == null)
                {
                   _scalarCollection = new ObservableCollection<IDataListItemModel>();
                    
                    _scalarCollection.CollectionChanged += (o, args) =>
                    {
                        RemoveItemPropertyChangeEvent(args);
                        AddItemPropertyChangeEvent(args);
                    };
                }
                return _scalarCollection;
            }
        }

        void AddItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if(args.NewItems != null)
            {
                foreach(INotifyPropertyChanged item in args.NewItems)
                {
                    if(item != null)
                    {
                        item.PropertyChanged += ItemPropertyChanged;
                    }
                }
            }
        }

        void RemoveItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if(args.OldItems != null)
            {
                foreach(INotifyPropertyChanged item in args.OldItems)
                {
                    if(item != null)
                    {
                        item.PropertyChanged -= ItemPropertyChanged;
                    }
                }
            }
        }

        void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(FindUnusedAndMissingCommand != null)
            {
                FindUnusedAndMissingCommand.RaiseCanExecuteChanged();
                SortCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<IDataListItemModel> RecsetCollection
        {
            get
            {
                if(_recsetCollection == null)
                {
                    _recsetCollection = new ObservableCollection<IDataListItemModel>();
                    _recsetCollection.CollectionChanged += (o, args) =>
                        {
                            RemoveItemPropertyChangeEvent(args);
                            AddItemPropertyChangeEvent(args);
                        };                    
                }
                return _recsetCollection;
            }
        }

        bool _toggleSortOrder = true;
        ObservableCollection<IDataListItemModel> _backupScalars;
        ObservableCollection<IDataListItemModel> _backupRecsets;

        #endregion Properties

        #region Ctor

        public DataListViewModel()
            : this(EventPublishers.Aggregator)
        {
        }

        public DataListViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
            DeleteCommand = new RelayCommand(item =>
            {
                RemoveDataListItem(item as IDataListItemModel);
                WriteToResourceModel();
            }, CanDelete);
            ClearSearchTextCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => SearchText = "");
            ViewSortDelete = true;
        }

        bool CanDelete(Object itemx)
        {

           var item =itemx as IDataListItemModel;
            return item != null && !item.IsUsed;
        }

        #endregion

        #region Commands

        public ICommand ClearSearchTextCommand { get; private set; }

        public ICommand AddRecordsetCommand
        {
            get
            {
                return _addRecordsetCommand ??
                       (_addRecordsetCommand = new DelegateCommand(method => AddRecordSet()));
            }
        }

        public RelayCommand SortCommand
        {
            get
            {
                return _sortCommand ??
                       (_sortCommand = new RelayCommand(method => SortItems(), p => CanSortItems));
            }
        }

        public RelayCommand FindUnusedAndMissingCommand
        {
            get
            {
                return _findUnusedAndMissingDataListItems ??
                       (_findUnusedAndMissingDataListItems =
                       new RelayCommand(method => RemoveUnusedDataListItems(), o => HasAnyUnusedItems()));
            }
        }

        public RelayCommand DeleteCommand { get; set; }

        #endregion Commands

        #region Add/Remove Missing Methods

        public void AddMissingDataListItems(IList<IDataListVerifyPart> parts)
        {
            AddMissingDataListItems(parts, false);
        }

        public void SetIsUsedDataListItems(IList<IDataListVerifyPart> parts, bool isUsed)
        {
            foreach(var part in parts)
            {
                if(part.IsScalar)
                {
                    SetScalarPartIsUsed(part, isUsed);
                }
                else
                {
                    SetRecordSetPartIsUsed(part, isUsed);
                }
            }

            WriteToResourceModel();
            EventPublisher.Publish(new UpdateIntellisenseMessage());
        }

        void SetRecordSetPartIsUsed(IDataListVerifyPart part, bool isUsed)
        {
            var recsetsToRemove = RecsetCollection.Where(c => c.Name == part.Recordset && c.IsRecordset);
            recsetsToRemove.ToList().ForEach(recsetToRemove => ProcessFoundRecordSets(part, recsetToRemove, isUsed));
        }

        static void ProcessFoundRecordSets(IDataListVerifyPart part, IDataListItemModel recsetToRemove, bool isUsed)
        {
            if(string.IsNullOrEmpty(part.Field))
            {
                if(recsetToRemove != null)
                {
                    recsetToRemove.IsUsed = isUsed;
                }
            }
            else
            {
                if(recsetToRemove == null) return;
                var childrenToRemove = recsetToRemove.Children.Where(c => c.Name == part.Field && c.IsField);
                childrenToRemove.ToList().ForEach(childToRemove =>
                {
                    if(childToRemove != null)
                    {
                        childToRemove.IsUsed = isUsed;
                    }
                });
            }
        }

        void SetScalarPartIsUsed(IDataListVerifyPart part, bool isUsed)
        {
            var scalarsToRemove = ScalarCollection.Where(c => c.Name == part.Field);
            scalarsToRemove.ToList().ForEach(scalarToRemove =>
            {
                if(scalarToRemove != null)
                {
                    scalarToRemove.IsUsed = isUsed;
                }
            });
        }

        public void RemoveUnusedDataListItems()
        {
            var unusedScalars = ScalarCollection.Where(c => c.IsUsed == false).ToList();
            if(unusedScalars.Any())
            {
                foreach(var dataListItemModel in unusedScalars)
                {
                    ScalarCollection.Remove(dataListItemModel);
                }
            }
            var unusedRecordsets = RecsetCollection.Where(c => c.IsUsed == false).ToList();
            if(unusedRecordsets.Any())
            {
                foreach(var dataListItemModel in unusedRecordsets)
                {
                    RecsetCollection.Remove(dataListItemModel);
                }
            }
            foreach(var recset in RecsetCollection)
            {
                if(recset.Children.Count > 0)
                {
                    var unusedRecsetChildren = recset.Children.Where(c => c.IsUsed == false).ToList();
                    if(unusedRecsetChildren.Any())
                    {
                        foreach(var unusedRecsetChild in unusedRecsetChildren)
                        {
                            recset.Children.Remove(unusedRecsetChild);
                        }
                    }
                }
            }

            WriteToResourceModel();
            EventPublisher.Publish(new UpdateIntellisenseMessage());
            FindUnusedAndMissingCommand.RaiseCanExecuteChanged();
        }

        public void AddMissingDataListItems(IList<IDataListVerifyPart> parts, bool async)
        {
            IList<IDataListItemModel> tmpRecsetList = new List<IDataListItemModel>();
            foreach(var part in parts)
            {
                if(part.IsScalar)
                {
                    if(ScalarCollection.FirstOrDefault(c => c.Name == part.Field) == null)
                    {
                        IDataListItemModel scalar = DataListItemModelFactory.CreateDataListModel(part.Field,part.Description,enDev2ColumnArgumentDirection.None);
                        if(ScalarCollection.Count > ScalarCollection.Count - 1 && ScalarCollection.Count > 0)
                        {
                            ScalarCollection.Insert(ScalarCollection.Count - 1, scalar);
                        }
                        else
                        {
                            ScalarCollection.Insert(ScalarCollection.Count, scalar);
                        }
                    }
                }
                else
                {
                    IDataListItemModel recsetToAddTo = RecsetCollection.
                        FirstOrDefault(c => c.Name == part.Recordset && c.IsRecordset);

                    IDataListItemModel tmpRecset = tmpRecsetList.FirstOrDefault(c => c.Name == part.Recordset);

                    if(recsetToAddTo != null)
                    {
                        if(recsetToAddTo.Children.FirstOrDefault(c => c.Name == part.Field) == null)
                        {
                            IDataListItemModel child = DataListItemModelFactory.CreateDataListModel(part.Field,
                                                                                                    part.Description,
                                                                                                    recsetToAddTo);
                            if(recsetToAddTo.Children.Count > 0)
                            {
                                recsetToAddTo.Children.Insert(recsetToAddTo.Children.Count - 1, child);
                            }
                            else
                            {
                                recsetToAddTo.Children.Add(child);
                            }
                        }
                    }
                    else if(tmpRecset != null)
                    {
                        IDataListItemModel child = DataListItemModelFactory.CreateDataListModel
                            (part.Field, part.Description, tmpRecset);
                        child.Name = part.Recordset + "()." + part.Field;
                        tmpRecset.Children.Add(child);
                    }
                    else
                    {
                        IDataListItemModel recset = DataListItemModelFactory.CreateDataListModel
                            (part.Recordset, part.Description, enDev2ColumnArgumentDirection.None);

                        tmpRecsetList.Add(recset);
                    }
                }
            }

            foreach(var item in tmpRecsetList)
            {
                if(item.Children.Count == 0)
                {
                    item.Children.Add(DataListItemModelFactory.CreateDataListModel(string.Empty, string.Empty, item));
                }
                if(RecsetCollection.Count > 0)
                {
                    RecsetCollection.Insert(RecsetCollection.Count - 1, item);
                }
                else
                {
                    RecsetCollection.Add(item);
                }
            }

            WriteToResourceModel();
            EventPublisher.Publish(new UpdateIntellisenseMessage());
            RemoveBlankScalars();
            RemoveBlankRecordsets();
            RemoveBlankRecordsetFields();

            if(parts.Count > 0)
            {
                AddBlankRow(null);
            }
        }

        #endregion Add/Remove Missing Methods

        #region Methods
        public void InitializeDataListViewModel(IResourceModel resourceModel)
        {
            Resource = resourceModel;
            if(Resource == null) return;

            string errorString;
            CreateListsOfIDataListItemModelToBindTo(out errorString);
            if(!string.IsNullOrEmpty(errorString))
            {
                throw new Exception(errorString);
            }
        }
        public void InitializeDataListViewModel()
        {
            if(Resource == null) return;
            InitializeDataListViewModel(Resource);
        }

        void FilterItems()
        {
            //ConvertDataListStringToCollections(Resource.DataList);
            if (_backupScalars != null)
            {
                ScalarCollection.Clear();
                foreach (var dataListItemModel in _backupScalars)
                {
                    ScalarCollection.Add(dataListItemModel);
                }
            }
            if (_backupRecsets != null)
            {
                RecsetCollection.Clear();
                foreach (var dataListItemModel in _backupRecsets)
                {
                    RecsetCollection.Add(dataListItemModel);
                }
            }


            if (SearchText == null)
            {

                return;
            }
            _backupScalars = new ObservableCollection<IDataListItemModel>();
            _backupRecsets = new ObservableCollection<IDataListItemModel>();
            foreach (var dataListItemModel in ScalarCollection)
            {
                _backupScalars.Add(dataListItemModel);
            }
            foreach (var dataListItemModel in RecsetCollection)
            {
                _backupRecsets.Add(dataListItemModel);
            }
            
            for(int index = 0; index < ScalarCollection.Count; index++)
            {
                var item = ScalarCollection[index];
                if (!item.Name.ToUpper().Contains(SearchText.ToUpper()))
                    ScalarCollection.Remove(item);
                
            }

            for (int index = 0; index < RecsetCollection.Count; index++)
            {
                var item = RecsetCollection[index];
                item.Filter(SearchText);
                if (!item.FilterText.ToUpper().Contains(SearchText.ToUpper()))
                    RecsetCollection.Remove(item);
               
            }

         
        }

        public void AddBlankRow(IDataListItemModel item)
        {
            if(item != null)
            {
                if(!item.IsRecordset && !item.IsField)
                {
                    AddRowToScalars();
                }
                else
                {
                    AddRowToRecordsets();
                }
            }
            else
            {
                AddRowToScalars();
                AddRowToRecordsets();
            }
        }

        public void RemoveBlankRows(IDataListItemModel item)
        {
            if(item == null) return;

            if(!item.IsRecordset && !item.IsField)
            {
                RemoveBlankScalars();
            }
            else if(item.IsRecordset)
            {
                RemoveBlankRecordsets();
            }
            else
            {
                RemoveBlankRecordsetFields();
            }
        }

        public void RemoveDataListItem(IDataListItemModel itemToRemove)
        {
            if(itemToRemove == null) return;

            if(!itemToRemove.IsRecordset && !itemToRemove.IsField)
            {
                ScalarCollection.Remove(itemToRemove);
                CheckDataListItemsForDuplicates(DataList);
            }
            else if(itemToRemove.IsRecordset)
            {
                RecsetCollection.Remove(itemToRemove);
                CheckDataListItemsForDuplicates(DataList);
            }
            else
            {
                foreach(var recset in RecsetCollection)
                {
                    recset.Children.Remove(itemToRemove);
                    CheckDataListItemsForDuplicates(recset.Children);
                }
            }
        }

        public string WriteToResourceModel()
        {
            ScalarCollection.ForEach(FixNamingForScalar);
            AddRecordsetNamesIfMissing();
            var result = GetDataListString();
            if(Resource != null)
            {
                    Resource.DataList = result;
            }
            return result;
        }

        public void AddRecordsetNamesIfMissing()
        {
            var recsetNum = RecsetCollection != null ? RecsetCollection.Count : 0;
            int recsetCount = 0;

            while(recsetCount < recsetNum)
            {
                if(RecsetCollection != null)
                {
                    IDataListItemModel recset = RecsetCollection[recsetCount];

                    if(!string.IsNullOrWhiteSpace(recset.DisplayName))
                    {
                        FixNamingForRecset(recset);
                        int childrenNum = recset.Children.Count;
                        int childrenCount = 0;

                        while(childrenCount < childrenNum)
                        {
                            IDataListItemModel child = recset.Children[childrenCount];
                            
                            if(!string.IsNullOrWhiteSpace(child.DisplayName))
                            {
                                int indexOfDot = child.DisplayName.IndexOf(".", StringComparison.Ordinal);
                                if(indexOfDot > -1)
                                {
                                    string recsetName = child.DisplayName.Substring(0, indexOfDot + 1);
                                    child.DisplayName = child.DisplayName.Replace(recsetName, child.Parent.DisplayName + ".");
                                }
                                else
                                {
                                    child.DisplayName = string.Concat(child.Parent.DisplayName, ".", child.DisplayName);
                                }
                                FixCommonNamingProblems(child);
                            }
                            childrenCount++;
                        }
                    }
                }
                recsetCount++;
            }
            NotifyOfPropertyChange(() => DataList);
        }

        public void ValidateNames(IDataListItemModel item)
        {
            if(item == null) return;

            if(item.IsRecordset)
            {
                ValidateRecordset();
            }
            else if(item.IsField)
            {
                ValidateRecordsetChildren(item.Parent);
            }
            else
            {
                ValidateScalar();
            }
        }

        public void RemoveBlankRecordsets()
        {
            List<IDataListItemModel> blankList = RecsetCollection.Where
                (c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank)
                                                                 .ToList();

            if(blankList.Count <= 1) return;

            RecsetCollection.Remove(blankList.First());
        }

        public void RemoveBlankScalars()
        {
            List<IDataListItemModel> blankList = ScalarCollection.Where(c => c.IsBlank).ToList();

            if(blankList.Count <= 1) return;

            ScalarCollection.Remove(blankList.First());
        }

        public void RemoveBlankRecordsetFields()
        {
            foreach(var recset in RecsetCollection)
            {
                List<IDataListItemModel> blankChildList = recset.Children.Where(c => c.IsBlank).ToList();

                if(blankChildList.Count <= 1) continue;

                recset.Children.Remove(blankChildList.First());
            }
        }

        #endregion Methods

        #region Private Methods

        void ValidateRecordsetChildren(IDataListItemModel recset)
        {
            CheckForEmptyRecordset();

            CheckDataListItemsForDuplicates(recset.Children);

            CheckForFixedEmptyRecordsets();
        }

        void ValidateRecordset()
        {
            CheckForEmptyRecordset();

            CheckDataListItemsForDuplicates(DataList);

            CheckForFixedEmptyRecordsets();
        }

        void ValidateScalar()
        {
            CheckDataListItemsForDuplicates(DataList);
        }

        void CheckForEmptyRecordset()
        {
            foreach(var recordset in RecsetCollection.Where(c => c.Children.Count == 0 || c.Children.Count == 1 && string.IsNullOrEmpty(c.Children[0].Name) && !string.IsNullOrEmpty(c.Name)))
            {
                recordset.SetError(StringResources.ErrorMessageEmptyRecordSet);
            }
        }

        void CheckForFixedEmptyRecordsets()
        {
            foreach(var recset in RecsetCollection.Where(c => c.ErrorMessage == StringResources.ErrorMessageEmptyRecordSet && c.Children.Count >= 1 && !string.IsNullOrEmpty(c.Children[0].Name)))
            {
                if(recset.ErrorMessage != StringResources.ErrorMessageDuplicateRecordset || recset.ErrorMessage != StringResources.ErrorMessageInvalidChar)
                {
                    recset.RemoveError();
                }

            }
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        void CheckDataListItemsForDuplicates(IEnumerable<IDataListItemModel> itemsToCheck)
        {
            List<IGrouping<string, IDataListItemModel>> duplicates = itemsToCheck.ToLookup(x => x.Name).ToList();
            foreach(var duplicate in duplicates)
            {
                if(duplicate.Count() > 1 && !String.IsNullOrEmpty(duplicate.Key))
                {
                    duplicate.ForEach(model => model.SetError(StringResources.ErrorMessageDuplicateValue));
                }
                else
                {
                    duplicate.ForEach(model =>
                    {
                        if(model.ErrorMessage != null && model.ErrorMessage.Contains(StringResources.ErrorMessageDuplicateValue))
                        {
                            model.RemoveError();
                        }
                    });
                }
            }
        }

        void AddRowToScalars()
        {
            List<IDataListItemModel> blankList = ScalarCollection.Where(c => c.IsBlank).ToList();
            if(blankList.Count != 0) return;

            IDataListItemModel scalar = DataListItemModelFactory.CreateDataListModel(string.Empty);
            ScalarCollection.Add(scalar);
        }

        void AddRowToRecordsets()
        {
            List<IDataListItemModel> blankList = RecsetCollection.Where
                (c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();

            if(blankList.Count == 0)
            {
                AddRecordSet();
            }

            foreach(var recset in RecsetCollection)
            {
                List<IDataListItemModel> blankChildList = recset.Children.Where(c => c.IsBlank).ToList();
                if(blankChildList.Count != 0) continue;

                IDataListItemModel newChild = DataListItemModelFactory.CreateDataListModel(string.Empty);
                newChild.Parent = recset;
                recset.Children.Add(newChild);
            }
        }

        static void FixNamingForRecset(IDataListItemModel recset)
        {
            if(!recset.DisplayName.EndsWith("()"))
            {
                recset.DisplayName = string.Concat(recset.DisplayName, "()");
            }
            FixCommonNamingProblems(recset);
        }

        static void FixNamingForScalar(IDataListItemModel scalar)
        {
            if(scalar.DisplayName.Contains("()"))
            {
                scalar.DisplayName = scalar.DisplayName.Replace("()", "");
            }
            FixCommonNamingProblems(scalar);
        }

        static void FixCommonNamingProblems(IDataListItemModel recset)
        {
            if(recset.DisplayName.Contains("[") || recset.DisplayName.Contains("]"))
            {
                recset.DisplayName = recset.DisplayName.Replace("[", "").Replace("]", "");
            }
        }

        private bool HasAnyUnusedItems()
        {
            if(!HasItems())
            {
                return false;
            }

            bool hasUnused = false;

            if(ScalarCollection != null)
            {
                hasUnused = ScalarCollection.Any(sc => !sc.IsUsed);
                if(hasUnused)
                {
                    DeleteCommand.RaiseCanExecuteChanged();
                    return true;
                }
            }

            if(RecsetCollection != null)
            {
                hasUnused = RecsetCollection.Any(sc => !sc.IsUsed);
                if(hasUnused)
                {
                    DeleteCommand.RaiseCanExecuteChanged();
                    return true;
                }

                hasUnused = RecsetCollection.SelectMany(sc => sc.Children).Any(sc => !sc.IsUsed);
            }
            return hasUnused;
        }

        /// <summary>
        ///     Creates the full data list.
        /// </summary>
        /// <returns></returns>
        private OptomizedObservableCollection<IDataListItemModel> CreateFullDataList()
        {
            var fullDataList = new OptomizedObservableCollection<IDataListItemModel>();
            foreach(var item in ScalarCollection)
            {
                fullDataList.Add(item);
            }
            foreach(var item in RecsetCollection)
            {
                fullDataList.Add(item);
            }

            return fullDataList;
        }
        /// <summary>
        ///     Adds a record set.
        /// </summary>
        private void AddRecordSet()
        {
            IDataListItemModel recset = DataListItemModelFactory.CreateDataListModel(string.Empty);
            IDataListItemModel childItem = DataListItemModelFactory.CreateDataListModel(string.Empty);
            recset.IsExpanded = false;
            childItem.Parent = recset;
            recset.Children.Add(childItem);
            RecsetCollection.Add(recset);
        }

        /// <summary>
        ///     Sorts the items.
        /// </summary>
        private void SortItems()
        {
            try
            {
                IsSorting = true;
                SortScalars(_toggleSortOrder);
                SortRecset(_toggleSortOrder);
                _toggleSortOrder = !_toggleSortOrder;
            }
            finally { IsSorting = false; }

        
        }

        public bool IsSorting  { get; set; }

        /// <summary>
        ///     Sorts the scalars.
        /// </summary>
        private void SortScalars(bool ascending)
        {
            IList<IDataListItemModel> newScalarCollection;
            if(ascending)
            {
                newScalarCollection = ScalarCollection.OrderBy(c => c.DisplayName).Where(c => !c.IsBlank).ToList();
            }
            else
            {
                newScalarCollection = ScalarCollection.OrderByDescending(c => c.DisplayName).Where(c => !c.IsBlank).ToList();
            }
            ScalarCollection.Clear();
            foreach (var item in newScalarCollection)
            {
                ScalarCollection.Add(item);
            }
            ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel(string.Empty));

        }

        /// <summary>
        ///     Sorts the recordsets.
        /// </summary>
        private void SortRecset(bool ascending)
        {
            IList<IDataListItemModel> newRecsetCollection = @ascending ? RecsetCollection.OrderBy(c => c.DisplayName).ToList() : RecsetCollection.OrderByDescending(c => c.DisplayName).ToList();
            RecsetCollection.Clear();
            foreach(var item in newRecsetCollection.Where(c => !c.IsBlank))
            {
                RecsetCollection.Add(item);
            }
            AddRecordSet();
        }


        /// <summary>
        ///     Creates the list of data list item view model to bind to.
        /// </summary>
        /// <param name="errorString">The error string.</param>
        /// <returns></returns>
        public void CreateListsOfIDataListItemModelToBindTo(out string errorString)
        {
            errorString = string.Empty;
            if(!string.IsNullOrEmpty(Resource.DataList))
            {
                ErrorResultTO errors = new ErrorResultTO();
                try
                {
                    ConvertDataListStringToCollections(Resource.DataList);
                }
                catch(Exception)
                {
                    errors.AddError("Invalid variable list. Please ensure that your variable list has valid entries");
                }
            }
            else
            {
                RecsetCollection.Clear();
                AddRecordSet();
                ScalarCollection.Clear();
            }

            BaseCollection = new OptomizedObservableCollection<DataListHeaderItemModel>();

            DataListHeaderItemModel varNode = DataListItemModelFactory.CreateDataListHeaderItem("Variable");
            if(ScalarCollection.Count == 0)
            {
                var dataListItemModel = DataListItemModelFactory.CreateDataListModel(string.Empty);
                ScalarCollection.Add(dataListItemModel);
            }
            varNode.Children = ScalarCollection;
            BaseCollection.Add(varNode);

            DataListHeaderItemModel recordsetsNode = DataListItemModelFactory.CreateDataListHeaderItem("Recordset");
            if(RecsetCollection.Count == 0)
            {
                AddRecordSet();
            }
            recordsetsNode.Children = RecsetCollection;
            BaseCollection.Add(recordsetsNode);
        }

        public void ClearCollections()
        {
            BaseCollection.Clear();
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        public void ConvertDataListStringToCollections(string dataList)
        {
            if (string.IsNullOrEmpty(dataList))
            {
                return;
            }
            RecsetCollection.Clear();
            ScalarCollection.Clear();
            try
            {
                var xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);
                if (xDoc.DocumentElement != null)
                {
                    var children = xDoc.DocumentElement.ChildNodes;
                    foreach (XmlNode c in children)
                    {
                        if (!DataListUtil.IsSystemTag(c.Name))
                        {
                            if (c.HasChildNodes)
                            {
                                AddRecordSets(c);
                            }
                            else
                            {
                                AddScalars(c);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
            }
            

        }

        void AddScalars(XmlNode c)
        {
            if(c.Attributes != null)
            {
                var scalar = DataListItemModelFactory.CreateDataListModel(c.Name, ParseDescription(c.Attributes[Description]), ParseColumnIODirection(c.Attributes[GlobalConstants.DataListIoColDirection]));
                scalar.IsEditable = ParseIsEditable(c.Attributes[IsEditable]);
                if(String.IsNullOrEmpty(_searchText))
                ScalarCollection.Add(scalar);
                else if(scalar.Name.ToUpper().StartsWith(_searchText.ToUpper()))
                {
                    ScalarCollection.Add(scalar);
                }
            }
            else
            {
                var scalar = DataListItemModelFactory.CreateDataListModel(c.Name, ParseDescription(null), ParseColumnIODirection(null));
                scalar.IsEditable = ParseIsEditable(null);
                if (String.IsNullOrEmpty(_searchText))
                    ScalarCollection.Add(scalar);
                else if (scalar.Name.ToUpper().StartsWith(_searchText.ToUpper()))
                {
                    ScalarCollection.Add(scalar);
                }
            }
        }

        void AddRecordSets(XmlNode c)
        {
            var cols = new List<IDataListItemModel>();
            {
                foreach(XmlNode subc in c.ChildNodes)
                {
                    // It is possible for the .Attributes property to be null, a check should be added
                    CreateColumns(subc, cols);
                }
                var recset = CreateRecordSet(c);
                AddColumnsToRecordSet(cols, recset);
            }
        }

        static void AddColumnsToRecordSet(IEnumerable<IDataListItemModel> cols, IDataListItemModel recset)
        {
            foreach(var col in cols)
            {
                col.Parent = recset;
                recset.Children.Add(col);
            }
        }

        IDataListItemModel CreateRecordSet(XmlNode c)
        {
            IDataListItemModel recset;
            if(c.Attributes != null)
            {
                recset = DataListItemModelFactory.CreateDataListModel(c.Name, ParseDescription(c.Attributes[Description]), ParseColumnIODirection(c.Attributes[GlobalConstants.DataListIoColDirection]));
                recset.IsEditable = ParseIsEditable(c.Attributes[IsEditable]);
                RecsetCollection.Add(recset);
            }
            else
            {
                recset = DataListItemModelFactory.CreateDataListModel(c.Name, ParseDescription(null), ParseColumnIODirection(null));
                recset.IsEditable = ParseIsEditable(null);

                RecsetCollection.Add(recset);
            }
            return recset;
        }

        void CreateColumns(XmlNode subc, List<IDataListItemModel> cols)
        {
            if(subc.Attributes != null)
            {
                var child = DataListItemModelFactory.CreateDataListModel(subc.Name, ParseDescription(subc.Attributes[Description]), ParseColumnIODirection(subc.Attributes[GlobalConstants.DataListIoColDirection]));
                child.IsEditable = ParseIsEditable(subc.Attributes[IsEditable]);
                cols.Add(child);
            }
            else
            {
                var child = DataListItemModelFactory.CreateDataListModel(subc.Name, ParseDescription(null), ParseColumnIODirection(null));
                child.IsEditable = ParseIsEditable(null);
                cols.Add(child);
            }
        }

        private string ParseDescription(XmlAttribute attr)
        {
            var result = string.Empty;
            if (attr != null)
            {
                result = attr.Value;
            }
            return result;
        }

        private bool ParseIsEditable(XmlAttribute attr)
        {
            var result = true;
            if (attr != null)
            {
                Boolean.TryParse(attr.Value, out result);
            }

            return result;
        }

        // ReSharper disable InconsistentNaming
        private enDev2ColumnArgumentDirection ParseColumnIODirection(XmlAttribute attr)
        // ReSharper restore InconsistentNaming
        {
            enDev2ColumnArgumentDirection result = enDev2ColumnArgumentDirection.None;

            if (attr == null)
            {
                return result;
            }
            if (!Enum.TryParse(attr.Value, true, out result))
            {
                result = enDev2ColumnArgumentDirection.None;
            }
            return result;
        }


        private const string RootTag = "DataList";
        private const string Description = "Description";
        private const string IsEditable = "IsEditable";

        public string GetDataListString()
        {
            StringBuilder result = new StringBuilder("<" + RootTag + ">");
            foreach (var recSet in RecsetCollection ?? new OptomizedObservableCollection<IDataListItemModel>())
            {
                if (string.IsNullOrEmpty(recSet.Name))
                {
                    continue;
                }
                IEnumerable<IDataListItemModel> filledRecordSet = recSet.Children.Where(c => !c.IsBlank && !c.HasError);
                IList<Dev2Column> cols = filledRecordSet.Select(child => DataListFactory.CreateDev2Column(child.Name, child.Description, child.IsEditable, child.ColumnIODirection))
                                                         .ToList();

                AddItemToBuilder(result, recSet);
                result.Append(">");
                foreach (var col in cols)
                {
                    result.Append("<");
                    result.Append(col.ColumnName);
                    result.Append(" " + Description + "=\"");
                    result.Append(col.ColumnDescription);
                    result.Append("\" ");
                    result.Append(IsEditable + "=\"");
                    result.Append(col.IsEditable);
                    result.Append("\" ");
                    // Travis.Frisinger - Added Column direction
                    result.Append(GlobalConstants.DataListIoColDirection + "=\"");
                    result.Append(col.ColumnIODirection);
                    result.Append("\" ");
                    result.Append("/>");
                }
                result.Append("</");
                result.Append(recSet.Name);
                result.Append(">");
            }

            IList<IDataListItemModel> filledScalars =
                ScalarCollection != null ? ScalarCollection.Where(scalar => !scalar.IsBlank && !scalar.HasError).ToList() : new List<IDataListItemModel>();
            foreach (var scalar in filledScalars)
            {
                AddItemToBuilder(result, scalar);
                result.Append("/>");
            }
            result.Append("</" + RootTag + ">");
            return result.ToString();
        }

        void AddItemToBuilder(StringBuilder result, IDataListItemModel recSet)
        {
            result.Append("<");
            result.Append(recSet.Name);
            result.Append(" " + Description + "=\"");
            result.Append(recSet.Description);
            result.Append("\" ");
            result.Append(IsEditable + "=\"");
            result.Append(recSet.IsEditable);
            result.Append("\" ");
            result.Append(GlobalConstants.DataListIoColDirection + "=\"");
            result.Append(recSet.ColumnIODirection);
            result.Append("\" ");
        }

        /// <summary>
        /// Determines whether this instance has items in either calar or recset collection.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has items; otherwise, <c>false</c>.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/06/25</date>
        private bool HasItems()
        {
            return (ScalarCollection != null && ScalarCollection.Count > 1) || (RecsetCollection != null && RecsetCollection.Count > 1);
        }
        #endregion Private Methods

        #region Override Methods

        protected override void OnDispose()
        {
            ClearCollections();
            Resource = null;
        }

        #endregion Override Methods

        #region Implementation of ShowUnusedDataListVariables

        void ShowUnusedDataListVariables(IResourceModel resourceModel, IList<IDataListVerifyPart> listOfUnused, IList<IDataListVerifyPart> listOfUsed)
        {
            if(resourceModel == Resource)
            {
                if(listOfUnused != null && listOfUnused.Count != 0)
                {
                    SetIsUsedDataListItems(listOfUnused, false);
                }
                else
                {
                    // do we need to process ;)
                    UpdateDataListItemsAsUsed();
                }
                if (listOfUsed != null && listOfUsed.Count > 0)
                {
                    SetIsUsedDataListItems(listOfUsed,true);
                }
            }
        }

        void UpdateDataListItemsAsUsed()
        {
            SetScalarItemsAsUsed();
            SetRecordSetItemsAsUsed();
        }

        void SetRecordSetItemsAsUsed()
        {
            if(RecsetCollection.Any(rc => rc.IsUsed == false))
            {
                foreach(var dataListItemModel in RecsetCollection)
                {
                    dataListItemModel.IsUsed = true;
                    foreach(var listItemModel in dataListItemModel.Children)
                    {
                        listItemModel.IsUsed = true;
                    }
                }
            }
        }

        void SetScalarItemsAsUsed()
        {
            if(ScalarCollection.Any(sc => sc.IsUsed == false))
            {
                foreach(var dataListItemModel in ScalarCollection)
                {
                    dataListItemModel.IsUsed = true;
                }
            }
        }

        #endregion

        /// <summary>
        /// Finds the missing workflow data regions.
        /// </summary>
        /// <param name="partsToVerify">The parts to verify.</param>
        /// <param name="excludeUnusedItems"></param>
        /// <returns></returns>
        public List<IDataListVerifyPart> MissingWorkflowItems(IList<IDataListVerifyPart> partsToVerify, bool excludeUnusedItems = false)
        {
            var missingWorkflowParts = new List<IDataListVerifyPart>();

            if(DataList != null)
            {
                foreach(var dataListItem in DataList)
                {
                    if(String.IsNullOrEmpty(dataListItem.Name))
                    {
                        continue;
                    }
                    if(dataListItem.Children.Count > 0)
                    {
                        if(partsToVerify.Count(part => part.Recordset == dataListItem.Name) == 0)
                        {
                            //19.09.2012: massimo.guerrera - Added in the description to creating the part
                            if(dataListItem.IsEditable)
                            {
                                // skip it if unused and exclude is on ;)
                                if(excludeUnusedItems && !dataListItem.IsUsed)
                                {
                                    continue;
                                }
                                missingWorkflowParts.Add(
                                    IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.Name,
                                                                                              String.Empty,
                                                                                              dataListItem.Description));
                                // ReSharper disable LoopCanBeConvertedToQuery
                                foreach(var child in dataListItem.Children)
                                // ReSharper restore LoopCanBeConvertedToQuery
                                {
                                    if(!String.IsNullOrEmpty(child.Name))
                                    {
                                        //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                        if(dataListItem.IsEditable)
                                        {
                                            missingWorkflowParts.Add(
                                                IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                                    dataListItem.Name, child.Name, child.Description));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // ReSharper disable LoopCanBeConvertedToQuery
                            foreach(var child in dataListItem.Children)
                                // ReSharper restore LoopCanBeConvertedToQuery
                                if(partsToVerify.Count(part => part.Field == child.Name && part.Recordset == child.Parent.Name) == 0)
                                {
                                    //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                    if(child.IsEditable)
                                    {
                                        // skip it if unused and exclude is on ;)
                                        if(excludeUnusedItems && !dataListItem.IsUsed)
                                        {
                                            continue;
                                        }

                                        missingWorkflowParts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.Name, child.Name, child.Description));
                                    }
                                }
                        }
                    }
                    else if(partsToVerify.Count(part => part.Field == dataListItem.Name && part.IsScalar) == 0)
                    {
                        if(dataListItem.IsEditable)
                        {
                            // skip it if unused and exclude is on ;)
                            if(excludeUnusedItems && !dataListItem.IsUsed)
                            {
                                continue;
                            }

                            if (!dataListItem.IsField)
                            {
                                //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                missingWorkflowParts.Add(
                                    IntellisenseFactory.CreateDataListValidationScalarPart(dataListItem.Name,
                                                                                           dataListItem.Description));
                            }
                        }

                    }
                }
            }

            return missingWorkflowParts;
        }

        public List<IDataListVerifyPart> MissingDataListParts(IList<IDataListVerifyPart> partsToVerify)
        {
            var missingDataParts = new List<IDataListVerifyPart>();
            foreach(var part in partsToVerify)
            {
                if(DataList != null)
                {
                    FindMissingPartsForRecordset(part, missingDataParts);
                    FindMissingForScalar(part, missingDataParts);
                }
            }
            return missingDataParts;
        }

        void FindMissingForScalar(IDataListVerifyPart part, List<IDataListVerifyPart> missingDataParts)
        {
            if(part.IsScalar)
            {
                if(DataList.Count(c => c.Name == part.Field && !c.IsRecordset) == 0)
                {
                    missingDataParts.Add(part);
                }
            }
        }

        void FindMissingPartsForRecordset(IDataListVerifyPart part, List<IDataListVerifyPart> missingDataParts)
        {
            if(!part.IsScalar)
            {
                var recset = DataList.Where(c => c.Name == part.Recordset && c.IsRecordset).ToList();
                if(!recset.Any())
                {
                    missingDataParts.Add(part);
                }
                else
                {
                    if(!string.IsNullOrEmpty(part.Field) && recset[0].Children.Count(c => c.Name == part.Field) == 0)
                    {
                        missingDataParts.Add(part);
                    }
                }
            }
        }

        public List<IDataListVerifyPart> UpdateDataListItems(IResourceModel resourceModel, IList<IDataListVerifyPart> workflowFields)
        {
            IList<IDataListVerifyPart> removeParts = MissingWorkflowItems(workflowFields);
            var filteredDataListParts = MissingDataListParts(workflowFields);
            ShowUnusedDataListVariables(resourceModel, removeParts,workflowFields);

            if(resourceModel == Resource)
            {
                AddMissingDataListItems(filteredDataListParts);
                return filteredDataListParts;
            }

            return new List<IDataListVerifyPart>();
        }

        public string DataListErrorMessage
        {
            get
            {
                if(HasErrors)
                {
                    var allErrorMessages = DataList.Select(model =>
                    {
                        string errorMessage;
                        if(BuildRecordSetErrorMessages(model, out errorMessage))
                        {
                            return errorMessage;
                        }
                        if(model.HasError)
                        {
                            return BuildErrorMessage(model);
                        }
                        return null;
                    });
                    var completeErrorMessage = string.Join(Environment.NewLine, allErrorMessages.Where(s => !string.IsNullOrEmpty(s)));
                    return completeErrorMessage;
                }
                return "";
            }
        }

        static bool BuildRecordSetErrorMessages(IDataListItemModel model, out string errorMessage)
        {
            errorMessage = "";
            if(RecordSetHasChildren(model))
            {
                if(model.HasError)
                {
                    {
                        errorMessage = BuildErrorMessage(model);
                        return true;
                    }
                }
                var childErrors = model.Children.Where(child => child.HasError);
                {
                    errorMessage = string.Join(Environment.NewLine, childErrors.Select(BuildErrorMessage));
                    return true;
                }
            }
            return false;
        }

        static bool RecordSetHasChildren(IDataListItemModel model)
        {
            return model.IsRecordset && model.Children != null && model.Children.Count > 0;
        }

        static string BuildErrorMessage(IDataListItemModel model)
        {
            return DataListUtil.AddBracketsToValueIfNotExist(model.DisplayName) + " : " + model.ErrorMessage;
        }
    }
}
