
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
using Warewolf.Storage;
// ReSharper disable UseNullPropagation
// ReSharper disable ConvertPropertyToExpressionBody

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.DataList
{
    public class DataListViewModel : BaseViewModel, IDataListViewModel, IUpdatesHelp
    {
        #region Fields

        private DelegateCommand _addRecordsetCommand;
        private ObservableCollection<DataListHeaderItemModel> _baseCollection;
        private RelayCommand _findUnusedAndMissingDataListItems;
        private ObservableCollection<IRecordSetItemModel> _recsetCollection;
        private ObservableCollection<IScalarItemModel> _scalarCollection;
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
                if (DataList == null || DataList.Count == 0)
                {
                    return false;
                }
                var recSetsHasErrors = RecsetCollection.Any(model => model.HasError || (model.Children != null && model.Children.Any(itemModel => itemModel.HasError)));
                var complexObjectHasErrors = ComplexObjectCollection.Any(model => model.HasError || (model.Children != null && model.Children.Any(itemModel => itemModel.HasError)));
                var scalasHasErrors = ScalarCollection.Any(model => model.HasError);
                /* return RecsetCollection.Any(model =>
                 {
                     if (RecordSetHasChildren(model))
                     {
                         return model.HasError || model.Children.Any(child => child.HasError);
                     }
                     return model.HasError;
                 });*/

                var hasErrors = recSetsHasErrors || scalasHasErrors|| complexObjectHasErrors;
                return hasErrors;
            }
        }

        public ObservableCollection<IScalarItemModel> ScalarCollection
        {
            get
            {
                if (_scalarCollection == null)
                {
                    _scalarCollection = new ObservableCollection<IScalarItemModel>();

                    _scalarCollection.CollectionChanged += (o, args) =>
                    {
                        RemoveItemPropertyChangeEvent(args);
                        AddItemPropertyChangeEvent(args);
                    };
                }
                return _scalarCollection;
            }
        }

        public ObservableCollection<IComplexObjectItemModel> ComplexObjectCollection
        {
            get
            {
                if (_complexObjectCollection == null)
                {
                    _complexObjectCollection = new ObservableCollection<IComplexObjectItemModel>();

                    _complexObjectCollection.CollectionChanged += (o, args) =>
                    {
                        RemoveItemPropertyChangeEvent(args);
                        AddItemPropertyChangeEvent(args);
                    };
                }
                return _complexObjectCollection;
            }
        }

        void AddItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in args.NewItems)
                {
                    if (item != null)
                    {
                        item.PropertyChanged += ItemPropertyChanged;
                    }
                }
            }
        }

        void RemoveItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in args.OldItems)
                {
                    if (item != null)
                    {
                        item.PropertyChanged -= ItemPropertyChanged;
                    }
                }
            }
        }

        void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (FindUnusedAndMissingCommand != null)
            {
                FindUnusedAndMissingCommand.RaiseCanExecuteChanged();
                SortCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<IRecordSetItemModel> RecsetCollection
        {
            get
            {
                if (_recsetCollection == null)
                {
                    _recsetCollection = new ObservableCollection<IRecordSetItemModel>();
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
        ObservableCollection<IScalarItemModel> _backupScalars;
        ObservableCollection<IRecordSetItemModel> _backupRecsets;
        private ObservableCollection<IComplexObjectItemModel> _complexObjectCollection;

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
            AddNoteCommand = new RelayCommand(item =>
            {

            }, CanAddNotes);
            ViewComplexObjectsCommand = new RelayCommand(item =>
            {

            }, CanViewComplexObjects);
            ClearSearchTextCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => SearchText = "");
            ViewSortDelete = true;
            Provider = new Dev2TrieSugggestionProvider();
        }

        bool CanViewComplexObjects(Object itemx)
        {
            var item = itemx as IDataListItemModel;
            return item != null && !item.IsComplexObject;
        }

        bool CanAddNotes(Object itemx)
        {
            var item = itemx as IDataListItemModel;
            return item != null && !item.AllowNotes;
        }

        bool CanDelete(Object itemx)
        {

            var item = itemx as IDataListItemModel;
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
                       (_findUnusedAndMissingDataListItems = new RelayCommand(method => RemoveUnusedDataListItems(), o => HasAnyUnusedItems()));
            }
        }

        public RelayCommand DeleteCommand { get; set; }

        public RelayCommand AddNoteCommand { get; set; }

        public RelayCommand ViewComplexObjectsCommand { get; set; }

        #endregion Commands

        #region Add/Remove Missing Methods

        public void AddMissingDataListItems(IList<IDataListVerifyPart> parts)
        {
            AddMissingDataListItems(parts, false);
        }

        public void SetIsUsedDataListItems(IList<IDataListVerifyPart> parts, bool isUsed)
        {
            foreach (var part in parts)
            {
                if (part.IsScalar)
                {
                    SetScalarPartIsUsed(part, isUsed);
                }
                if (part.IsJson)
                {
                    SetJsonPartIsUsed(part, isUsed);
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
            var recsetsToRemove = RecsetCollection.Where(c => c.DisplayName == part.Recordset);
            recsetsToRemove.ToList().ForEach(recsetToRemove => ProcessFoundRecordSets(part, recsetToRemove, isUsed));
        }
        void SetJsonPartIsUsed(IDataListVerifyPart part, bool isUsed)
        {
            var recsetsToRemove = ComplexObjectCollection.Where(c => c.DisplayName == part.Recordset);
            recsetsToRemove.ToList().ForEach(recsetToRemove => ProcessFoundJsonObjects(part, recsetToRemove, isUsed));
        }

        static void ProcessFoundRecordSets(IDataListVerifyPart part, IRecordSetItemModel recsetToRemove, bool isUsed)
        {
            if (string.IsNullOrEmpty(part.Field))
            {
                if (recsetToRemove != null)
                {
                    recsetToRemove.IsUsed = isUsed;
                }
            }
            else
            {
                if (recsetToRemove == null) return;
                var childrenToRemove = recsetToRemove.Children.Where(c => c.DisplayName == part.Field);
                childrenToRemove.ToList().ForEach(childToRemove =>
                {
                    if (childToRemove != null)
                    {
                        childToRemove.IsUsed = isUsed;
                    }
                });
            }
        }
        static void ProcessFoundJsonObjects(IDataListVerifyPart part, IComplexObjectItemModel jsonProperty, bool isUsed)
        {
            if (string.IsNullOrEmpty(part.Field))
            {
                if (jsonProperty != null)
                {
                    jsonProperty.IsUsed = isUsed;
                }
            }
            else
            {
                if (jsonProperty == null) return;
                var childrenToRemove = jsonProperty.Children.Where(c => c.DisplayName == part.Field);
                childrenToRemove.ToList().ForEach(childToRemove =>
                {
                    if (childToRemove != null)
                    {
                        childToRemove.IsUsed = isUsed;
                    }
                });
            }
        }

        void SetScalarPartIsUsed(IDataListVerifyPart part, bool isUsed)
        {
            var scalarsToRemove = ScalarCollection.Where(c => c.DisplayName == part.Field);
            scalarsToRemove.ToList().ForEach(scalarToRemove =>
            {
                if (scalarToRemove != null)
                {
                    scalarToRemove.IsUsed = isUsed;
                }
            });
        }

        public void RemoveUnusedDataListItems()
        {
            var unusedScalars = ScalarCollection.Where(c => c.IsUsed == false).ToList();
            if (unusedScalars.Any())
            {
                foreach (var dataListItemModel in unusedScalars)
                {
                    ScalarCollection.Remove(dataListItemModel);
                }
            }
            var unusedRecordsets = RecsetCollection.Where(c => c.IsUsed == false).ToList();
            if (unusedRecordsets.Any())
            {
                foreach (var dataListItemModel in unusedRecordsets)
                {
                    RecsetCollection.Remove(dataListItemModel);
                }
            }
            foreach (var recset in RecsetCollection)
            {
                if (recset.Children.Count > 0)
                {
                    var unusedRecsetChildren = recset.Children.Where(c => c.IsUsed == false).ToList();
                    if (unusedRecsetChildren.Any())
                    {
                        foreach (var unusedRecsetChild in unusedRecsetChildren)
                        {
                            recset.Children.Remove(unusedRecsetChild);
                        }
                    }
                }
            }

            var unusedComplexObjects = ComplexObjectCollection.Where(c => c.IsUsed == false).ToList();
            if (unusedComplexObjects.Any())
            {
                foreach (var dataListItemModel in unusedComplexObjects)
                {
                    ComplexObjectCollection.Remove(dataListItemModel);
                }
            }
            foreach (var complexObject in ComplexObjectCollection)
            {
                if (complexObject.Children.Count > 0)
                {
                    var unUsedComplexObjectCollection = complexObject.Children.Where(c => c.IsUsed == false).ToList();
                    if (unUsedComplexObjectCollection.Any())
                    {
                        foreach (var unusedRecsetChild in unUsedComplexObjectCollection)
                        {
                            complexObject.Children.Remove(unusedRecsetChild);
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
            IList<IRecordSetItemModel> tmpRecsetList = new List<IRecordSetItemModel>();
            foreach (var part in parts)
            {
                if (part.IsJson)
                {
                    AddComplexObject(part);
                }
                else
                {
                    if (part.IsScalar)
                    {
                        if (ScalarCollection.FirstOrDefault(c => c.DisplayName == part.Field) == null)
                        {
                            var scalar = DataListItemModelFactory.CreateScalarItemModel(part.Field, part.Description, enDev2ColumnArgumentDirection.None);
                            if (ScalarCollection.Count > ScalarCollection.Count - 1 && ScalarCollection.Count > 0)
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
                        IRecordSetItemModel recsetToAddTo = RecsetCollection.FirstOrDefault(c => c.DisplayName == part.Recordset);

                        IRecordSetItemModel tmpRecset = tmpRecsetList.FirstOrDefault(c => c.DisplayName == part.Recordset);

                        if (recsetToAddTo != null)
                        {
                            if (recsetToAddTo.Children.FirstOrDefault(c => c.DisplayName == part.Field) == null)
                            {
                                IRecordSetFieldItemModel child = DataListItemModelFactory.CreateRecordSetFieldItemModel(part.Field, part.Description, recsetToAddTo);
                                if (recsetToAddTo.Children.Count > 0)
                                {
                                    recsetToAddTo.Children.Insert(recsetToAddTo.Children.Count - 1, child);
                                }
                                else
                                {
                                    recsetToAddTo.Children.Add(child);
                                }
                            }
                        }
                        else if (tmpRecset != null)
                        {
                            IRecordSetFieldItemModel child = DataListItemModelFactory.CreateRecordSetFieldItemModel(part.Field, part.Description, tmpRecset);
                            if (child != null)
                            {
                                child.DisplayName = part.Recordset + "()." + part.Field;
                                tmpRecset.Children.Add(child);
                            }
                        }
                        else
                        {
                            IRecordSetItemModel recset = DataListItemModelFactory.CreateRecordSetItemModel(part.Recordset, part.Description);

                            tmpRecsetList.Add(recset);
                        }
                    }
                }
            }

            foreach (var item in tmpRecsetList)
            {
                if (item.Children.Count == 0)
                {
                    item.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel(string.Empty, string.Empty, item));
                }
                if (RecsetCollection.Count > 0)
                {
                    RecsetCollection.Insert(RecsetCollection.Count - 1, item);
                }
                else
                {
                    RecsetCollection.Add(item);
                }
            }



            RemoveBlankScalars();
            RemoveBlankRecordsets();
            RemoveBlankRecordsetFields();
            RemoveBlankComplexObjects();
            if (parts.Count > 0)
            {
                AddBlankRow(null);
            }

            var items = RefreshTries(_scalarCollection.ToList(), new List<string>()).Union(RefreshRecordSets(_recsetCollection.ToList(), new List<string>())).Union(RefreshJsonObjects(_complexObjectCollection.ToList()));
            Provider.VariableList = new ObservableCollection<string>(items);

            WriteToResourceModel();
            EventPublisher.Publish(new UpdateIntellisenseMessage());
        }

        private void RemoveBlankComplexObjects()
        {
            var complexObjectItemModels = ComplexObjectCollection.Where(model => string.IsNullOrEmpty(model.DisplayName));
            var objectItemModels = complexObjectItemModels as IList<IComplexObjectItemModel> ?? complexObjectItemModels.ToList();
            for (int i = objectItemModels.Count; i > 0; i--)
            {
                ComplexObjectCollection.Remove(objectItemModels[i - 1]);
            }

        }

        private void AddComplexObject(IDataListVerifyPart part)
        {
            if (part == null)
                return;

            var paths = part.DisplayValue.Split('.');
            IComplexObjectItemModel itemModel = null;
            for (int index = 0; index < paths.Length; index++)
            {
                string path = paths[index];
                var isArray = false;
                if (path.Contains("()") || path.Contains("(*)"))
                {
                    isArray = true;
                    path = path.Replace("(*)", "()");
                }
                if (itemModel == null)
                {
                    itemModel = ComplexObjectCollection.FirstOrDefault(model => model.DisplayName == path);
                }
                if (itemModel == null)
                {

                    itemModel = new ComplexObjectItemModel(path) { IsArray = isArray };
                    ComplexObjectCollection.Add(itemModel);
                }
                else
                {
                    if (itemModel.DisplayName != path)
                    {
                        var item = itemModel.Children.FirstOrDefault(model => model.DisplayName == path);
                        if (item == null)
                        {
                            item = new ComplexObjectItemModel(path) { Parent = itemModel, IsArray = isArray };
                            itemModel.Children.Add(item);
                        }
                        itemModel = item;
                    }

                }
            }
        }

        #endregion Add/Remove Missing Methods

        #region Methods
        public void InitializeDataListViewModel(IResourceModel resourceModel)
        {
            Resource = resourceModel;
            if (Resource == null) return;

            string errorString;
            CreateListsOfIDataListItemModelToBindTo(out errorString);
            if (!string.IsNullOrEmpty(errorString))
            {
                throw new Exception(errorString);
            }
            var items = RefreshTries(_scalarCollection.ToList(), new List<string>()).Union(RefreshRecordSets(_recsetCollection.ToList(), new List<string>())).Union(RefreshJsonObjects(_complexObjectCollection.ToList()));
            Provider.VariableList = new ObservableCollection<string>(items);
        }

        private IEnumerable<string> RefreshJsonObjects(IEnumerable<IComplexObjectItemModel> toList)
        {
            List<string> accList = new List<string>();
            foreach (var dataListItemModel in toList)
            {
                if (!string.IsNullOrEmpty(dataListItemModel.Name))
                {
                    var rec = "[[" + dataListItemModel.Name + "]]";
                    accList.Add(rec);
                    accList.AddRange(RefreshJsonObjects(dataListItemModel.Children.ToList()));
                }
            }
            return accList;
        }

        public void InitializeDataListViewModel()
        {
            if (Resource == null) return;
            InitializeDataListViewModel(Resource);
        }

        void FilterItems()
        {
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
            _backupScalars = new ObservableCollection<IScalarItemModel>();
            _backupRecsets = new ObservableCollection<IRecordSetItemModel>();
            foreach (var dataListItemModel in ScalarCollection)
            {
                _backupScalars.Add(dataListItemModel);
            }
            foreach (var dataListItemModel in RecsetCollection)
            {
                _backupRecsets.Add(dataListItemModel);
            }

            for (int index = 0; index < ScalarCollection.Count; index++)
            {
                var item = ScalarCollection[index];
                if (!item.DisplayName.ToUpper().Contains(SearchText.ToUpper()))
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


        private IList<string> RefreshTries(List<IScalarItemModel> toList, IList<string> accList)
        {
            foreach (var dataListItemModel in toList)
            {
                if (!string.IsNullOrEmpty(dataListItemModel.DisplayName))
                {
                    accList.Add("[[" + dataListItemModel.DisplayName + "]]");
                }
            }
            return accList;
        }


        private IList<string> RefreshRecordSets(List<IRecordSetItemModel> toList, IList<string> accList)
        {
            foreach (var dataListItemModel in toList)
            {
                if (!string.IsNullOrEmpty(dataListItemModel.DisplayName))
                {
                    var recsetAppend = DataListUtil.MakeValueIntoHighLevelRecordset(dataListItemModel.DisplayName);
                    var recsetStar = DataListUtil.MakeValueIntoHighLevelRecordset(dataListItemModel.DisplayName, true);

                    accList.Add(DataListUtil.AddBracketsToValueIfNotExist(recsetAppend));
                    accList.Add(DataListUtil.AddBracketsToValueIfNotExist(recsetStar));
                }
                foreach (var listItemModel in dataListItemModel.Children)
                {
                    if (!string.IsNullOrEmpty(listItemModel.Name))
                    {
                        var rec = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(dataListItemModel.DisplayName, listItemModel.DisplayName, ""));
                        if (ExecutionEnvironment.IsRecordsetIdentifier(rec))
                        {
                            accList.Add(DataListUtil.ReplaceRecordBlankWithStar(rec));
                            accList.Add(rec);
                        }
                    }
                }
                foreach (var listItemModel in ScalarCollection)
                {
                    if (!string.IsNullOrEmpty(listItemModel.DisplayName))
                    {
                        var rec = "[[" + listItemModel.DisplayName + "]]";
                        if (ExecutionEnvironment.IsScalar(rec))
                        {
                            accList.Add(rec);
                        }
                    }
                }

            }
            return accList;
        }


        public void AddBlankRow(IDataListItemModel item)
        {
            if (item != null)
            {
                if (!(item is IRecordSetItemModel) && !(item is IScalarItemModel))
                {
                    AddRowToScalars();
                }
                else if (item is IRecordSetItemModel)
                {
                    AddRowToRecordsets();
                }
                else if (item is ComplexObjectItemModel)
                {
                    AddRowToComplexObjects();
                }
            }
            else
            {
                AddRowToScalars();
                AddRowToRecordsets();
                AddRowToComplexObjects();
                
            }
        }

        public void RemoveBlankRows(IDataListItemModel item)
        {
            if (item == null) return;

            if (!(item is IRecordSetItemModel) && !(item is IScalarItemModel))
            {
                RemoveBlankScalars();
            }
            else if (item is IRecordSetItemModel)
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
            if (itemToRemove == null)
                return;

            if ((itemToRemove is IScalarItemModel))
            {
                IScalarItemModel item = ScalarCollection.Where(x => x.DisplayName == itemToRemove.DisplayName).SingleOrDefault();
                if (item != null)
                {
                    ScalarCollection.Remove(item);
                }
                CheckDataListItemsForDuplicates(DataList);
            }
            else if (itemToRemove is IRecordSetItemModel)
            {
                IRecordSetItemModel item = RecsetCollection.Where(x => x.DisplayName == itemToRemove.DisplayName).SingleOrDefault();
                if (item != null)
                {
                    RecsetCollection.Remove(item);
                }
                CheckDataListItemsForDuplicates(DataList);
            }
            else
            {
                foreach (var recset in RecsetCollection)
                {
                    IRecordSetFieldItemModel item = recset.Children.Where(x => x.DisplayName == itemToRemove.DisplayName).SingleOrDefault();
                    if (item != null)
                    {
                        recset.Children.Remove(item);
                    }
                    CheckDataListItemsForDuplicates(recset.Children);
                }
            }
        }

        public string WriteToResourceModel()
        {
            ScalarCollection.ForEach(FixNamingForScalar);
            AddRecordsetNamesIfMissing();
            var result = GetDataListString();
            if (Resource != null)
            {
                Resource.DataList = result;
            }

            BaseCollection[0].Children = new ObservableCollection<IDataListItemModel>(ScalarCollection);
            BaseCollection[1].Children = new ObservableCollection<IDataListItemModel>(RecsetCollection);
            BaseCollection[2].Children = new ObservableCollection<IDataListItemModel>(ComplexObjectCollection);
            AddBlankRow(null);
            return result;
        }

        public void AddRecordsetNamesIfMissing()
        {
            var recsetNum = RecsetCollection != null ? RecsetCollection.Count : 0;
            int recsetCount = 0;

            while (recsetCount < recsetNum)
            {
                if (RecsetCollection != null)
                {
                    IRecordSetItemModel recset = RecsetCollection[recsetCount];

                    if (!string.IsNullOrWhiteSpace(recset.DisplayName))
                    {
                        FixNamingForRecset(recset);
                        int childrenNum = recset.Children.Count;
                        int childrenCount = 0;

                        while (childrenCount < childrenNum)
                        {
                            IRecordSetFieldItemModel child = recset.Children[childrenCount];

                            if (child != null && !string.IsNullOrWhiteSpace(child.DisplayName))
                            {
                                int indexOfDot = child.DisplayName.IndexOf(".", StringComparison.Ordinal);
                                if (indexOfDot > -1)
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
            if (item == null)
                return;

            if (item is IRecordSetItemModel)
            {
                ValidateRecordset();
            }
            else if (item is IRecordSetFieldItemModel)
            {
                IRecordSetFieldItemModel rs = (IRecordSetFieldItemModel)item;
                ValidateRecordsetChildren(rs.Parent);
            }
            else
            {
                ValidateScalar();
            }
        }

        public void RemoveBlankRecordsets()
        {
            List<IRecordSetItemModel> blankList = RecsetCollection.Where(c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();

            if (blankList.Count <= 1) return;

            RecsetCollection.Remove(blankList.First());
        }

        public void RemoveBlankScalars()
        {
            List<IScalarItemModel> blankList = ScalarCollection.Where(c => c.IsBlank).ToList();

            if (blankList.Count <= 1) return;

            ScalarCollection.Remove(blankList.First());
        }

        public void RemoveBlankRecordsetFields()
        {
            foreach (var recset in RecsetCollection)
            {
                List<IRecordSetFieldItemModel> blankChildList = recset.Children.Where(c => c.IsBlank).ToList();

                if (blankChildList.Count <= 1) continue;

                recset.Children.Remove(blankChildList.First());
            }
        }

        #endregion Methods

        #region Private Methods

        void ValidateRecordsetChildren(IRecordSetItemModel recset)
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
            foreach (var recordset in RecsetCollection.Where(c => c.Children.Count == 0 || c.Children.Count == 1 && string.IsNullOrEmpty(c.Children[0].DisplayName) && !string.IsNullOrEmpty(c.DisplayName)))
            {
                recordset.SetError(StringResources.ErrorMessageEmptyRecordSet);
            }
        }

        void CheckForFixedEmptyRecordsets()
        {
            foreach (var recset in RecsetCollection.Where(c => c.ErrorMessage == StringResources.ErrorMessageEmptyRecordSet && c.Children.Count >= 1 && !string.IsNullOrEmpty(c.Children[0].DisplayName)))
            {
                if (recset.ErrorMessage != StringResources.ErrorMessageDuplicateRecordset || recset.ErrorMessage != StringResources.ErrorMessageInvalidChar)
                {
                    recset.RemoveError();
                }
            }
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        void CheckDataListItemsForDuplicates(IEnumerable<IDataListItemModel> itemsToCheck)
        {
            List<IGrouping<string, IDataListItemModel>> duplicates = itemsToCheck.ToLookup(x => x.DisplayName).ToList();
            foreach (var duplicate in duplicates)
            {
                if (duplicate.Count() > 1 && !String.IsNullOrEmpty(duplicate.Key))
                {
                    duplicate.ForEach(model => model.SetError(StringResources.ErrorMessageDuplicateValue));
                }
                else
                {
                    duplicate.ForEach(model =>
                    {
                        if (model.ErrorMessage != null && model.ErrorMessage.Contains(StringResources.ErrorMessageDuplicateValue))
                        {
                            model.RemoveError();
                        }
                    });
                }
            }
        }

        void AddRowToScalars()
        {
            List<IScalarItemModel> blankList = ScalarCollection.Where(c => c.IsBlank).ToList();
            if (blankList.Count != 0) return;

            var scalar = DataListItemModelFactory.CreateScalarItemModel(string.Empty);
            ScalarCollection.Add(scalar);
        }

        void AddRowToRecordsets()
        {
            List<IRecordSetItemModel> blankList = RecsetCollection.Where(c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();

            if (blankList.Count == 0)
            {
                AddRecordSet();
            }

            foreach (var recset in RecsetCollection)
            {
                List<IRecordSetFieldItemModel> blankChildList = recset.Children.Where(c => c.IsBlank).ToList();
                if (blankChildList.Count != 0) continue;

                IRecordSetFieldItemModel newChild = DataListItemModelFactory.CreateRecordSetFieldItemModel(string.Empty);
                if (newChild != null)
                {
                    newChild.Parent = recset;
                    recset.Children.Add(newChild);
                }
            }
        }
        void AddRowToComplexObjects()
        {
            List<IComplexObjectItemModel> blankList = ComplexObjectCollection.Where(c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();
            if (blankList.Count == 0)
            {
               AddComplexObject();
            }

            foreach (var complexObjectItemModel in ComplexObjectCollection)
            {
                List<IComplexObjectItemModel> blankChildList = complexObjectItemModel.Children.Where(c => c.IsBlank).ToList();
                if (blankChildList.Count != 0) continue;

                IComplexObjectItemModel newChild = DataListItemModelFactory.CreateComplexObjectItemModel(string.Empty);
                if (newChild != null)
                {
                    newChild.Parent = complexObjectItemModel;
                    complexObjectItemModel.Children.Add(newChild);
                }
            }
        }

        static void FixNamingForRecset(IDataListItemModel recset)
        {
            if (!recset.DisplayName.EndsWith("()"))
            {
                recset.DisplayName = string.Concat(recset.DisplayName, "()");
            }
            FixCommonNamingProblems(recset);
        }

        static void FixNamingForScalar(IDataListItemModel scalar)
        {
            if (scalar.DisplayName.Contains("()"))
            {
                scalar.DisplayName = scalar.DisplayName.Replace("()", "");
            }
            FixCommonNamingProblems(scalar);
        }

        static void FixCommonNamingProblems(IDataListItemModel recset)
        {
            if (recset.DisplayName.Contains("[") || recset.DisplayName.Contains("]"))
            {
                recset.DisplayName = recset.DisplayName.Replace("[", "").Replace("]", "");
            }
        }

        private bool HasAnyUnusedItems()
        {
            if (!HasItems())
            {
                return false;
            }

            bool hasUnused = false;

            if (ScalarCollection != null)
            {
                hasUnused = ScalarCollection.Any(sc => !sc.IsUsed);
                if (hasUnused)
                {
                    DeleteCommand.RaiseCanExecuteChanged();
                    return true;
                }
            }

            if (RecsetCollection != null)
            {
                hasUnused = RecsetCollection.Any(sc => !sc.IsUsed);
                if (hasUnused)
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
            foreach (var item in ScalarCollection)
            {
                fullDataList.Add(item);
            }
            foreach (var item in RecsetCollection)
            {
                fullDataList.Add(item);
            }
            foreach (var item in ComplexObjectCollection)
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
            IRecordSetItemModel recset = DataListItemModelFactory.CreateRecordSetItemModel(string.Empty);
            IRecordSetFieldItemModel childItem = DataListItemModelFactory.CreateRecordSetFieldItemModel(string.Empty);
            if (recset != null)
            {
                recset.IsComplexObject = false;
                recset.AllowNotes = false;
                recset.IsExpanded = false;
                if (childItem != null)
                {
                    childItem.IsComplexObject = false;
                    childItem.AllowNotes = false;
                    childItem.Parent = recset;
                    recset.Children.Add(childItem);
                }
                RecsetCollection.Add(recset);
            }
        }

        private void AddComplexObject()
        {
            IComplexObjectItemModel complexObjectItemModel = DataListItemModelFactory.CreateComplexObjectItemModel(string.Empty);
            IComplexObjectItemModel childItem = DataListItemModelFactory.CreateComplexObjectItemModel(string.Empty);
            if (complexObjectItemModel != null)
            {
                complexObjectItemModel.IsExpanded = false;
                if (childItem != null)
                {
                    if (childItem.Parent == null)
                    {
                        childItem.IsComplexObject = true;
                        childItem.AllowNotes = false;
                    }
                    childItem.Parent = complexObjectItemModel;
                    complexObjectItemModel.Children.Add(childItem);
                }
                ComplexObjectCollection.Add(complexObjectItemModel);
            }
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

        public bool IsSorting { get; set; }

        /// <summary>
        ///     Sorts the scalars.
        /// </summary>
        private void SortScalars(bool ascending)
        {
            IList<IScalarItemModel> newScalarCollection = @ascending ? ScalarCollection.OrderBy(c => c.DisplayName).Where(c => !c.IsBlank).ToList() : ScalarCollection.OrderByDescending(c => c.DisplayName).Where(c => !c.IsBlank).ToList();
            ScalarCollection.Clear();
            foreach (var item in newScalarCollection)
            {
                ScalarCollection.Add(item);
            }
            ScalarCollection.Add(DataListItemModelFactory.CreateScalarItemModel(string.Empty));
        }

        /// <summary>
        ///     Sorts the recordsets.
        /// </summary>
        private void SortRecset(bool ascending)
        {
            IList<IRecordSetItemModel> newRecsetCollection = @ascending ? RecsetCollection.OrderBy(c => c.DisplayName).ToList() : RecsetCollection.OrderByDescending(c => c.DisplayName).ToList();
            RecsetCollection.Clear();
            foreach (var item in newRecsetCollection.Where(c => !c.IsBlank))
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
            if (!string.IsNullOrEmpty(Resource.DataList))
            {
                ErrorResultTO errors = new ErrorResultTO();
                try
                {
                    ConvertDataListStringToCollections(Resource.DataList);
                }
                catch (Exception)
                {
                    errors.AddError("Invalid variable list. Please ensure that your variable list has valid entries");
                }
            }
            else
            {
                RecsetCollection.Clear();
                ScalarCollection.Clear();
                ComplexObjectCollection.Clear();

                AddRecordSet();
                AddComplexObject();
            }

            BaseCollection = new OptomizedObservableCollection<DataListHeaderItemModel>();

            DataListHeaderItemModel variableNode = DataListItemModelFactory.CreateDataListHeaderItem("Variable");
            if (ScalarCollection.Count == 0)
            {
                IScalarItemModel dataListItemModel = DataListItemModelFactory.CreateScalarItemModel(string.Empty);
                dataListItemModel.IsComplexObject = false;
                dataListItemModel.AllowNotes = false;
                ScalarCollection.Add(dataListItemModel);
            }
            BaseCollection.Add(variableNode);

            DataListHeaderItemModel recordsetsNode = DataListItemModelFactory.CreateDataListHeaderItem("Recordset");
            if (RecsetCollection.Count == 0)
            {
                AddRecordSet();
            }
            BaseCollection.Add(recordsetsNode);

            DataListHeaderItemModel complexObjectNode = DataListItemModelFactory.CreateDataListHeaderItem("Object");
            if (ComplexObjectCollection.Count == 0)
            {
                AddComplexObject();
            }
            BaseCollection.Add(complexObjectNode);
            WriteToResourceModel();
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
            ComplexObjectCollection.Clear();
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
                            var jsonAttribute = false;
                            if (c.Attributes != null)
                            {
                                var xmlAttribute = c.Attributes["IsJson"];
                                if (xmlAttribute != null)
                                {
                                    bool.TryParse(xmlAttribute.Value, out jsonAttribute);
                                }
                            }
                            if (jsonAttribute)
                            {
                                AddComplexObjectFromXmlNode(c);
                            }
                            else
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
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
            }
        }

        private void AddComplexObjectFromXmlNode(XmlNode xmlNode)
        {
            var children = xmlNode.ChildNodes;
            var isArray = false;
            if (xmlNode.Attributes != null)
            {
                 isArray = ParseBoolAttribute(xmlNode.Attributes["IsArray"]);
            }
            var name = GetNameForArrayComplexObject(xmlNode, isArray);
            var parent = new ComplexObjectItemModel(name) {IsArray = isArray};
            ComplexObjectCollection.Add(parent);
            foreach (XmlNode c in children)
            {
                AddComplexObjectFromXmlNode(c,parent);
                
            }
        }

        private static string GetNameForArrayComplexObject(XmlNode xmlNode, bool isArray)
        {
            var name = isArray ? xmlNode.Name + "()" : xmlNode.Name;
            return name;
        }

        private void AddComplexObjectFromXmlNode(XmlNode c, ComplexObjectItemModel parent)
        {
            var isArray = false;
            if (c.Attributes != null)
            {
                isArray = ParseBoolAttribute(c.Attributes["IsArray"]);
            }
            var name = GetNameForArrayComplexObject(c, isArray);
            var complexObjectItemModel = new ComplexObjectItemModel(name) { IsArray = isArray };
            parent.Children.Add(complexObjectItemModel);
            if (c.HasChildNodes)
            {
                var children = c.ChildNodes;
                foreach (XmlNode childNode in children)
                {
                    AddComplexObjectFromXmlNode(childNode, complexObjectItemModel);
                }
            }
        }

        void AddScalars(XmlNode c)
        {
            if (c.Attributes != null)
            {
                IScalarItemModel scalar = DataListItemModelFactory.CreateScalarItemModel(c.Name, ParseDescription(c.Attributes[Description]), ParseColumnIODirection(c.Attributes[GlobalConstants.DataListIoColDirection]));
                if (scalar != null)
                {
                    scalar.IsEditable = ParseIsEditable(c.Attributes[IsEditable]);
                    if (String.IsNullOrEmpty(_searchText))
                        ScalarCollection.Add(scalar);
                    else if (scalar.DisplayName.ToUpper().StartsWith(_searchText.ToUpper()))
                    {
                        ScalarCollection.Add(scalar);
                    }
                }
            }
            else
            {
                IScalarItemModel scalar = DataListItemModelFactory.CreateScalarItemModel(c.Name, ParseDescription(null), ParseColumnIODirection(null));
                if (scalar != null)
                {
                    scalar.IsEditable = ParseIsEditable(null);
                    if (String.IsNullOrEmpty(_searchText))
                        ScalarCollection.Add(scalar);
                    else if (scalar.DisplayName.ToUpper().StartsWith(_searchText.ToUpper()))
                    {
                        ScalarCollection.Add(scalar);
                    }
                }
            }
        }

        void AddRecordSets(XmlNode c)
        {
            var cols = new List<IDataListItemModel>();
            {
                var recset = CreateRecordSet(c);
                foreach (XmlNode subc in c.ChildNodes)
                {
                    // It is possible for the .Attributes property to be null, a check should be added
                    CreateColumns(subc, cols, recset);
                }


                //                var castCols = cols.Select(dataListItemModel => dataListItemModel as IRecordSetFieldItemModel).ToList();
                //
                //                AddColumnsToRecordSet(castCols, recset);
            }
        }

        static void AddColumnsToRecordSet(IEnumerable<IRecordSetFieldItemModel> cols, IRecordSetItemModel recset)
        {
            foreach (var col in cols)
            {
                col.Parent = recset;
                recset.Children.Add(col);
            }
        }

        IRecordSetItemModel CreateRecordSet(XmlNode c)
        {
            IRecordSetItemModel recset;
            if (c.Attributes != null)
            {
                recset = DataListItemModelFactory.CreateRecordSetItemModel(c.Name, ParseDescription(c.Attributes[Description]), null, null, false, "", true, true, false, ParseColumnIODirection(c.Attributes[GlobalConstants.DataListIoColDirection]));
                if (recset != null)
                {
                    recset.IsEditable = ParseIsEditable(c.Attributes[IsEditable]);
                    RecsetCollection.Add(recset);
                }
            }
            else
            {
                recset = DataListItemModelFactory.CreateRecordSetItemModel(c.Name, ParseDescription(null));
                if (recset != null)
                {
                    recset.IsEditable = ParseIsEditable(null);

                    RecsetCollection.Add(recset);
                }
            }
            return recset;
        }

        void CreateColumns(XmlNode subc, List<IDataListItemModel> cols, IRecordSetItemModel recset)
        {
            if (subc.Attributes != null)
            {
                var child = DataListItemModelFactory.CreateDataListModel(subc.Name, ParseDescription(subc.Attributes[Description]), recset, false, "", ParseIsEditable(subc.Attributes[IsEditable]), true, false, ParseColumnIODirection(subc.Attributes[GlobalConstants.DataListIoColDirection]));
                recset.Children.Add(child);
            }
            else
            {
                var child = DataListItemModelFactory.CreateDataListModel(subc.Name, ParseDescription(null), ParseColumnIODirection(null), recset);
                child.IsEditable = ParseIsEditable(null);
                recset.Children.Add(child);
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
           return ParseBoolAttribute(attr);
        }

        private bool ParseBoolAttribute(XmlAttribute attr)
        {
            var result = true;
            if (attr != null)
            {
                bool.TryParse(attr.Value, out result);
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
            foreach (var recSet in RecsetCollection ?? new OptomizedObservableCollection<IRecordSetItemModel>())
            {
                if (string.IsNullOrEmpty(recSet.DisplayName))
                {
                    continue;
                }
                IEnumerable<IDataListItemModel> filledRecordSet = recSet.Children.Where(c => !c.IsBlank && !c.HasError);
                IList<Dev2Column> cols = filledRecordSet.Select(child => DataListFactory.CreateDev2Column(child.DisplayName, child.Description, child.IsEditable, child.ColumnIODirection)).ToList();

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
                    result.Append(GlobalConstants.DataListIoColDirection + "=\"");
                    result.Append(col.ColumnIODirection);
                    result.Append("\" ");
                    result.Append("/>");
                }
                result.Append("</");
                result.Append(recSet.DisplayName);
                result.Append(">");
            }

            IList<IScalarItemModel> filledScalars = ScalarCollection != null ? ScalarCollection.Where(scalar => !scalar.IsBlank && !scalar.HasError).ToList() : new List<IScalarItemModel>();
            foreach (var scalar in filledScalars)
            {
                AddItemToBuilder(result, scalar);
                result.Append("/>");
            }
            var complexObjectItemModels = ComplexObjectCollection.Where(model => !string.IsNullOrEmpty(model.DisplayName));
            foreach (var complexObjectItemModel in complexObjectItemModels)
            {
                AddComplexObjectsToBuilder(result, complexObjectItemModel);

            }

            result.Append("</" + RootTag + ">");
            return result.ToString();
        }

        private void AddComplexObjectsToBuilder(StringBuilder result, IComplexObjectItemModel complexObjectItemModel)
        {
            result.Append("<");
            var name = complexObjectItemModel.DisplayName;
            if (complexObjectItemModel.IsArray)
            {
                name = name.Replace("()", "");
            }
            result.Append(name);
            result.Append(" " + Description + "=\"");
            result.Append(complexObjectItemModel.Description);
            result.Append("\" ");
            result.Append(IsEditable + "=\"");
            result.Append(complexObjectItemModel.IsEditable);
            result.Append("\" ");
            result.Append("IsJson" + "=\"");
            result.Append(true);
            result.Append("\" ");
            result.Append("IsArray" + "=\"");
            result.Append(complexObjectItemModel.IsArray);
            result.Append("\" ");
            result.Append(GlobalConstants.DataListIoColDirection + "=\"");
            result.Append(complexObjectItemModel.ColumnIODirection);
            result.Append("\" ");
            result.Append(">");
            var complexObjectItemModels = complexObjectItemModel.Children.Where(model => !string.IsNullOrEmpty(model.DisplayName));
            foreach (var itemModel in complexObjectItemModels)
            {
                AddComplexObjectsToBuilder(result, itemModel);
            }
            result.Append("</");
            result.Append(name);
            result.Append(">");
        }

        void AddItemToBuilder(StringBuilder result, IDataListItemModel item)
        {
            result.Append("<");
            result.Append(item.DisplayName);
            result.Append(" " + Description + "=\"");
            result.Append(item.Description);
            result.Append("\" ");
            result.Append(IsEditable + "=\"");
            result.Append(item.IsEditable);
            result.Append("\" ");
            result.Append(GlobalConstants.DataListIoColDirection + "=\"");
            result.Append(item.ColumnIODirection);
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
            if (resourceModel == Resource)
            {
                if (listOfUnused != null && listOfUnused.Count != 0)
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
                    SetIsUsedDataListItems(listOfUsed, true);
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
            if (RecsetCollection.Any(rc => rc.IsUsed == false))
            {
                foreach (var dataListItemModel in RecsetCollection)
                {
                    dataListItemModel.IsUsed = true;
                    foreach (var listItemModel in dataListItemModel.Children)
                    {
                        listItemModel.IsUsed = true;
                    }
                }
            }
        }

        void SetScalarItemsAsUsed()
        {
            if (ScalarCollection.Any(sc => sc.IsUsed == false))
            {
                foreach (var dataListItemModel in ScalarCollection)
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

            if (DataList != null)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var dataListItem in ScalarCollection)
                {
                    if (string.IsNullOrEmpty(dataListItem.DisplayName))
                    {
                        continue;
                    }

                    if (partsToVerify.Count(part => part.Field == dataListItem.DisplayName && part.IsScalar) == 0)
                    {
                        if (dataListItem.IsEditable)
                        {
                            // skip it if unused and exclude is on ;)
                            if (excludeUnusedItems && !dataListItem.IsUsed)
                            {
                                continue;
                            }
                            missingWorkflowParts.Add(IntellisenseFactory.CreateDataListValidationScalarPart(dataListItem.DisplayName, dataListItem.Description));
                        }
                    }
                }

                foreach (var dataListItem in RecsetCollection)
                {
                    if (String.IsNullOrEmpty(dataListItem.DisplayName))
                    {
                        continue;
                    }
                    if (dataListItem.Children.Count > 0)
                    {
                        if (partsToVerify.Count(part => part.Recordset == dataListItem.DisplayName) == 0)
                        {
                            //19.09.2012: massimo.guerrera - Added in the description to creating the part
                            if (dataListItem.IsEditable)
                            {
                                // skip it if unused and exclude is on ;)
                                if (excludeUnusedItems && !dataListItem.IsUsed)
                                {
                                    continue;
                                }
                                missingWorkflowParts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.DisplayName, String.Empty, dataListItem.Description));
                                // ReSharper disable LoopCanBeConvertedToQuery
                                foreach (var child in dataListItem.Children)
                                // ReSharper restore LoopCanBeConvertedToQuery
                                {
                                    if (!String.IsNullOrEmpty(child.DisplayName))
                                    {
                                        //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                        if (dataListItem.IsEditable)
                                        {
                                            missingWorkflowParts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.DisplayName, child.DisplayName, child.Description));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // ReSharper disable LoopCanBeConvertedToQuery
                            foreach (var child in dataListItem.Children)
                            {
                                // ReSharper restore LoopCanBeConvertedToQuery
                                if (partsToVerify.Count(part => child.Parent != null && part.Field == child.DisplayName && part.Recordset == child.Parent.DisplayName) == 0)
                                {
                                    //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                    if (child.IsEditable)
                                    {
                                        // skip it if unused and exclude is on ;)
                                        if (excludeUnusedItems && !dataListItem.IsUsed)
                                        {
                                            continue;
                                        }
                                        missingWorkflowParts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.DisplayName, child.DisplayName, child.Description));
                                    }
                                }
                            }
                        }
                    }
                    else if (partsToVerify.Count(part => part.Field == dataListItem.DisplayName && part.IsScalar) == 0)
                    {
                        if (dataListItem.IsEditable)
                        {
                            // skip it if unused and exclude is on ;)
                            if (excludeUnusedItems && !dataListItem.IsUsed)
                            {
                                continue;
                            }
                            missingWorkflowParts.Add(IntellisenseFactory.CreateDataListValidationScalarPart(dataListItem.DisplayName, dataListItem.Description));
                        }
                    }
                }
            }

            return missingWorkflowParts;
        }

        public List<IDataListVerifyPart> MissingDataListParts(IList<IDataListVerifyPart> partsToVerify)
        {
            var missingDataParts = new List<IDataListVerifyPart>();
            foreach (var part in partsToVerify)
            {
                if (DataList != null)
                {
                    FindMissingPartsForRecordset(part, missingDataParts);
                    FindMissingForScalar(part, missingDataParts);
                }
            }
            return missingDataParts;
        }

        void FindMissingForScalar(IDataListVerifyPart part, List<IDataListVerifyPart> missingDataParts)
        {
            if (part.IsScalar)
            {
                if (ScalarCollection.Count(c => c.DisplayName == part.Field) == 0)
                {
                    missingDataParts.Add(part);
                }
            }
        }

        void FindMissingPartsForRecordset(IDataListVerifyPart part, List<IDataListVerifyPart> missingDataParts)
        {
            if (!part.IsScalar)
            {
                var recset = RecsetCollection.Where(c => c.DisplayName == part.Recordset).ToList();
                if (!recset.Any())
                {
                    missingDataParts.Add(part);
                }
                else
                {
                    if (!string.IsNullOrEmpty(part.Field) && recset[0].Children.Count(c => c.DisplayName == part.Field) == 0)
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
            ShowUnusedDataListVariables(resourceModel, removeParts, workflowFields);

            if (resourceModel == Resource)
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
                if (HasErrors)
                {
                    IEnumerable<string> allErrorMessages = RecsetCollection.Select(model =>
                    {
                        string errorMessage;
                        if (BuildRecordSetErrorMessages(model, out errorMessage))
                        {
                            return errorMessage;
                        }
                        if (model.HasError)
                        {
                            return BuildErrorMessage(model);
                        }
                        return null;
                    });

                    string.Join(Environment.NewLine, allErrorMessages.Where(s => !string.IsNullOrEmpty(s)));

                    allErrorMessages = ScalarCollection.Select(model =>
                    {
                        if (model.HasError)
                        {
                            return BuildErrorMessage(model);
                        }
                        return null;
                    });
                    string completeErrorMessage = Environment.NewLine + string.Join(Environment.NewLine, allErrorMessages.Where(s => !string.IsNullOrEmpty(s)));

                    return completeErrorMessage;
                }
                return "";
            }
        }

        public ISuggestionProvider Provider { get; set; }

        static bool BuildRecordSetErrorMessages(IRecordSetItemModel model, out string errorMessage)
        {
            errorMessage = "";
            if (RecordSetHasChildren(model))
            {
                if (model.HasError)
                {
                    {
                        errorMessage = BuildErrorMessage(model);
                        return true;
                    }
                }
                var childErrors = model.Children.Where(child => child.HasError).ToList();
                if (childErrors.Any())
                    errorMessage = string.Join(Environment.NewLine, childErrors.Select(BuildErrorMessage));
                return true;
            }
            return false;
        }

        static bool RecordSetHasChildren(IRecordSetItemModel model)
        {
            return model.Children != null && model.Children.Count > 0;
        }

        static string BuildErrorMessage(IDataListItemModel model)
        {
            return DataListUtil.AddBracketsToValueIfNotExist(model.DisplayName) + " : " + model.ErrorMessage;
        }
    }
}
