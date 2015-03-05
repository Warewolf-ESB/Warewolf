
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using ServiceStack.Common.Extensions;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.DataList
{
    public class DataListViewModel : IDataListViewModel
    {
        #region Fields

        private DelegateCommand _addRecordsetCommand;
        private ObservableCollection<DataListHeaderItemModel> _baseCollection;
        private RelayCommand _findUnusedAndMissingDataListItems;
        private ObservableCollection<IDataListItemModel> _recsetCollection;
        private ObservableCollection<IDataListItemModel> _scalarCollection;
        private string _searchText;
        private RelayCommand _sortCommand;

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
                //NotifyOfPropertyChange(() => BaseCollection);
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                FilterItems();
                //NotifyOfPropertyChange(() => SearchText);
            }
        }

        public IResourceDefinition Resource { get; private set; }

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

        #endregion Properties

      
        #region Commands

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
            //EventPublisher.Publish(new UpdateIntellisenseMessage());
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
            //EventPublisher.Publish(new UpdateIntellisenseMessage());
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
                        if(ScalarCollection.Count > ScalarCollection.Count - 1)
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
            //EventPublisher.Publish(new UpdateIntellisenseMessage());
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
            //Resource = resourceModel;
            if(Resource == null) return;

            string errorString;
            CreateListsOfIDataListItemModelToBindTo(out errorString);
            if(!string.IsNullOrEmpty(errorString))
            {
                throw new Exception(errorString);
            }
        }
        
        public void InitializeDataListViewModel(IResourceDefinition resourceDefinition)
        {
            Resource = resourceDefinition;
            if (Resource == null) return;

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
            if(SearchText == null) return;

            foreach(var item in ScalarCollection)
            {
                item.IsVisable = item.Name.Contains(SearchText);
            }

            foreach(var item in RecsetCollection)
            {
                bool parentVis = false;
                foreach(var child in item.Children)
                {
                    if(child.Name.Contains(SearchText))
                    {
                        child.IsVisable = true;
                        parentVis = true;
                    }
                    else
                    {
                        child.IsVisable = false;
                    }
                }

                item.IsVisable = parentVis || item.Name.Contains(SearchText);
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
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            string result = string.Empty;
            string errorString;
            ScalarCollection.ForEach(FixNamingForScalar);
            AddRecordsetNamesIfMissing();
            IBinaryDataList postDl = ConvertIDataListItemModelsToIBinaryDataList(out errorString);

            if(string.IsNullOrEmpty(errorString))
            {
                ErrorResultTO errors;
                result = CreateXmlDataFromBinaryDataList(postDl, out errors);
                if(Resource != null)
                {
                    Resource.DataList = result;
                }
            }

            compiler.ForceDeleteDataListByID(postDl.UID);
            if(!string.IsNullOrEmpty(errorString))
            {
                throw new Exception(errorString);
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
            //NotifyOfPropertyChange(() => DataList);
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
            foreach(var recordset in RecsetCollection.Where(c => c.Children.Count == 0 || (c.Children.Count == 1 && string.IsNullOrEmpty(c.Children[0].Name)) && !string.IsNullOrEmpty(c.Name)))
            {
                recordset.SetError(Warewolf.Studio.Resources.Languages.Core.ErrorMessageEmptyRecordSet);
            }
        }

        void CheckForFixedEmptyRecordsets()
        {
            foreach (var recset in RecsetCollection.Where(c => c.ErrorMessage == Warewolf.Studio.Resources.Languages.Core.ErrorMessageEmptyRecordSet && (c.Children.Count >= 1 && !string.IsNullOrEmpty(c.Children[0].Name))))
            {
                if (recset.ErrorMessage != Warewolf.Studio.Resources.Languages.Core.ErrorMessageDuplicateRecordset || recset.ErrorMessage != Warewolf.Studio.Resources.Languages.Core.ErrorMessageInvalidChar)
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
                    duplicate.ForEach(model => model.SetError(Warewolf.Studio.Resources.Languages.Core.ErrorMessageDuplicateValue));
                }
                else
                {
                    duplicate.ForEach(model =>
                    {
                        if (model.ErrorMessage != null && model.ErrorMessage.Contains(Warewolf.Studio.Resources.Languages.Core.ErrorMessageDuplicateValue))
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
                    return true;
                }
            }

            if(RecsetCollection != null)
            {
                hasUnused = RecsetCollection.Any(sc => !sc.IsUsed);
                if(hasUnused)
                {
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
            SortScalars(_toggleSortOrder);
            SortRecset(_toggleSortOrder);
            _toggleSortOrder = !_toggleSortOrder;
        }

        /// <summary>
        ///     Sorts the scalars.
        /// </summary>
        private void SortScalars(bool ascending)
        {
            IList<IDataListItemModel> newScalarCollection;
            if(ascending)
            {
                newScalarCollection = ScalarCollection.OrderBy(c => c.DisplayName)
                                                                                .Where(c => !c.IsBlank).ToList();
            }
            else
            {
                newScalarCollection = ScalarCollection.OrderByDescending(c => c.DisplayName)
                                                                                .Where(c => !c.IsBlank).ToList();
            }
            ScalarCollection.Clear();
            foreach(var item in newScalarCollection)
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
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            if (!string.IsNullOrEmpty(Resource.DataList))
            {
                ErrorResultTO errors = new ErrorResultTO();
                try
                {
                    IBinaryDataList binarnyDl = CreateBinaryDataListFromXmlData(Resource.DataList, out errors);
                    if (!errors.HasErrors())
                    {
                        ConvertBinaryDataListToListOfIDataListItemModels(binarnyDl, out errorString);
                    }
                    else
                    {
                        string errorMessage = errors.FetchErrors().Aggregate(string.Empty, (current, error) => current + error);
                        throw new Exception(errorMessage);
                    }
                    if (binarnyDl != null)
                        compiler.ForceDeleteDataListByID(binarnyDl.UID);
                }
                catch(Exception)
                {
                    errors.AddError("Invalid variable list. Please insure that your variable list has valid entries");
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

            //AddRecordsetNamesIfMissing();

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

        /// <summary>
        ///     Creates a binary data list from XML data.
        /// </summary>
        /// <param name="xmlDataList">The XML data list.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private IBinaryDataList CreateBinaryDataListFromXmlData(string xmlDataList, out ErrorResultTO errors)
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList result = null;
            var allErrors = new ErrorResultTO();
            Guid dlGuid = compiler.ConvertTo(
                DataListFormat.CreateFormat(GlobalConstants._Studio_XML), xmlDataList.ToStringBuilder(), xmlDataList.ToStringBuilder(), out errors);

            if(!errors.HasErrors())
            {
                result = compiler.FetchBinaryDataList(dlGuid, out errors);
                if(errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }
            }

            compiler.ForceDeleteDataListByID(dlGuid);
            return result;
        }


        /// <summary>
        ///     Creates the XML data from binary data list.
        /// </summary>
        /// <param name="binaryDataList">The binary data list.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private string CreateXmlDataFromBinaryDataList(IBinaryDataList binaryDataList, out ErrorResultTO errors)
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid dlGuid = compiler.PushBinaryDataList(binaryDataList.UID, binaryDataList, out errors);
            string result = compiler.ConvertFrom(dlGuid,
                                                  DataListFormat.CreateFormat(GlobalConstants._Studio_XML),
                                                  enTranslationDepth.Shape, out errors).ToString();

            return result;
        }

        /// <summary>
        ///     Converts a binary data list to list a data list item view models.
        /// </summary>
        /// <param name="dataListToConvert">The data list to convert.</param>
        /// <param name="errorString">The error string.</param>
        /// <returns></returns>
        private void ConvertBinaryDataListToListOfIDataListItemModels(IBinaryDataList dataListToConvert,
                                                                      out string errorString)
        {
            errorString = string.Empty;
            RecsetCollection.Clear();
            ScalarCollection.Clear();

            IList<IBinaryDataListEntry> listOfEntries = dataListToConvert.FetchAllEntries();

            foreach(var entry in listOfEntries)
            {
                if(entry.IsRecordset)
                {
                    var recset = DataListItemModelFactory.CreateDataListModel(entry.Namespace, entry.Description, entry.ColumnIODirection);

                    recset.IsEditable = entry.IsEditable;

                    foreach(var col in entry.Columns)
                    {
                        var child = DataListItemModelFactory.CreateDataListModel(col.ColumnName, col.ColumnDescription, col.ColumnIODirection);

                        child.Parent = recset;
                        child.IsEditable = col.IsEditable;
                        recset.Children.Add(child);
                    }

                    RecsetCollection.Add(recset);
                }
                else
                {
                    var scalar = DataListItemModelFactory.CreateDataListModel(entry.Namespace, entry.Description, entry.ColumnIODirection);

                    scalar.IsEditable = entry.IsEditable;
                    ScalarCollection.Add(scalar);
                }
            }
        }

        /// <summary>
        ///     Converts a list of data list item view models to a binary data list.
        /// </summary>
        /// <param name="errorString">The error string.</param>
        /// <returns></returns>
        private IBinaryDataList ConvertIDataListItemModelsToIBinaryDataList(out string errorString)
        {
            errorString = string.Empty;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();

            IList<IDataListItemModel> filledScalars =
                ScalarCollection != null ? ScalarCollection.Where(scalar => !scalar.IsBlank && !scalar.HasError).ToList() : new List<IDataListItemModel>();
            foreach(var scalar in filledScalars)
            {
                result.TryCreateScalarTemplate
                    (string.Empty, scalar.Name, scalar.Description
                     , true, scalar.IsEditable, scalar.ColumnIODirection, out errorString);
            }

            foreach(var recset in RecsetCollection ?? new OptomizedObservableCollection<IDataListItemModel>())
            {
                if(recset.IsBlank) continue;
                IEnumerable<IDataListItemModel> filledRecordSets = recset.Children.Where(c => !c.IsBlank && !c.HasError);
                IList<Dev2Column> cols = filledRecordSets.Select(child => DataListFactory.CreateDev2Column(child.Name, child.Description, child.IsEditable, child.ColumnIODirection))
                                                         .ToList();

                result.TryCreateRecordsetTemplate(recset.Name, recset.Description, cols, true, recset.IsEditable, recset.ColumnIODirection, out errorString);
            }

            return result;
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

        protected void OnDispose()
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
                    if((dataListItem.Children.Count > 0))
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
                                    if(!(String.IsNullOrEmpty(child.Name)))
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
            if(!(part.IsScalar))
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

        public void Dispose()
        {
            OnDispose();
        }
    }
}
