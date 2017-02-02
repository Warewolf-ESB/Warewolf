/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Utils;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.DataList;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Core.ViewModels.Base;
using ServiceStack.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml;
using Dev2.Data.TO;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.DataList
{
    public class DataListViewModel : BaseViewModel, IDataListViewModel, IUpdatesHelp
    {
        #region Fields

        private readonly IComplexObjectHandler _complexObjectHandler;
        private readonly IScalarHandler _scalarHandler;
        private readonly IRecordsetHandler _recordsetHandler;
        private readonly IDataListViewModelHelper _helper;
        private ObservableCollection<DataListHeaderItemModel> _baseCollection;
        private RelayCommand _findUnusedAndMissingDataListItems;
        private ObservableCollection<IRecordSetItemModel> _recsetCollection;
        private ObservableCollection<IScalarItemModel> _scalarCollection;
        private string _searchText;
        private RelayCommand _sortCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _viewComplexObjectsCommand;
        private bool _viewSortDelete;

        private readonly IMissingDataList _missingDataList;
        private readonly IPartIsUsed _partIsUsed;

        #endregion Fields

        #region Properties

        public bool CanSortItems => HasItems();

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
                if (!string.Equals(_searchText, value, StringComparison.InvariantCultureIgnoreCase))
                {
                    _searchText = value;
                    NotifyOfPropertyChange(() => SearchText);
                    FilterCollection(_searchText);
                }
            }
        }

        private void FilterCollection(string searchText)
        {
            if (_scalarCollection != null && _scalarCollection.Count > 1)
            {
                for (int index = _scalarCollection.Count - 1; index >= 0; index--)
                {
                    var scalarItemModel = _scalarCollection[index];
                    scalarItemModel.Filter(searchText);
                }
            }
            if (_recsetCollection != null && _recsetCollection.Count > 1)
            {
                for(int index = _recsetCollection.Count-1; index >= 0; index--)
                {
                    var recordSetItemModel = _recsetCollection[index];
                    recordSetItemModel.Filter(searchText);
                }
            }
            if (_complexObjectCollection != null && _complexObjectCollection.Count > 0)
            {
                for (int index = _complexObjectCollection.Count - 1; index >= 0; index--)
                {
                    var complexObjectItemModel = _complexObjectCollection[index];
                    complexObjectItemModel.Filter(searchText);
                }
            }
            NotifyOfPropertyChange(() => ScalarCollection);
            NotifyOfPropertyChange(() => RecsetCollection);
            NotifyOfPropertyChange(() => ComplexObjectCollection);
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

        public ObservableCollection<IDataListItemModel> DataList => CreateFullDataList();

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
                var hasErrors = recSetsHasErrors || scalasHasErrors || complexObjectHasErrors;
                return hasErrors;
            }
        }



        public ObservableCollection<IScalarItemModel> ScalarCollection
        {
            get
            {
                if (_scalarCollection != null)
                {
                    if (string.IsNullOrEmpty(_searchText))
                    {
                        return _scalarCollection;
                    }
                    return _scalarCollection.Where(model => model.IsVisible).ToObservableCollection();
                }
                _scalarCollection = new ObservableCollection<IScalarItemModel>();
                _scalarCollection.CollectionChanged += OnScalarCollectionOnCollectionChanged;
                return _scalarCollection;
            }
        }

        private void OnScalarCollectionOnCollectionChanged(object o, NotifyCollectionChangedEventArgs args)
        {
            RemoveItemPropertyChangeEvent(args);
            AddItemPropertyChangeEvent(args);
        }        

        public ObservableCollection<IComplexObjectItemModel> ComplexObjectCollection
        {
            get
            {
                if (_complexObjectCollection != null)
                {
                    if (string.IsNullOrEmpty(_searchText))
                    {
                        return _complexObjectCollection;
                    }
                    return _complexObjectCollection.Where(model => model.IsVisible).ToObservableCollection();
                }
                _complexObjectCollection = new ObservableCollection<IComplexObjectItemModel>();

                _complexObjectCollection.CollectionChanged += (o, args) =>
                {
                    RemoveItemPropertyChangeEvent(args);
                    AddItemPropertyChangeEvent(args);
                };
                return _complexObjectCollection;
            }
        }

        private void AddItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems == null) return;
            foreach (INotifyPropertyChanged item in args.NewItems)
            {
                if (item != null)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        private void RemoveItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems == null) return;
            foreach (INotifyPropertyChanged item in args.OldItems)
            {
                if (item != null)
                {
                    item.PropertyChanged -= ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (FindUnusedAndMissingCommand != null)
            {
                FindUnusedAndMissingCommand.RaiseCanExecuteChanged();
                SortCommand.RaiseCanExecuteChanged();
            }
            ViewComplexObjectsCommand?.RaiseCanExecuteChanged();
            DeleteCommand?.RaiseCanExecuteChanged();
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

                if (string.IsNullOrEmpty(_searchText))
                {
                    return _recsetCollection;
                }
                var recordSetItemModels = _recsetCollection.Where(model => model.IsVisible).ToObservableCollection();
                return recordSetItemModels;
            }
        }

        private bool _toggleSortOrder = true;
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
            ClearSearchTextCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => SearchText = "");
            ViewSortDelete = true;
            Provider = new Dev2TrieSugggestionProvider();
            _missingDataList = new MissingDataList(RecsetCollection, ScalarCollection);
            _partIsUsed = new PartIsUsed(RecsetCollection, ScalarCollection, ComplexObjectCollection);
            _complexObjectHandler = new ComplexObjectHandler(this);
            _scalarHandler = new ScalarHandler(this);
            _recordsetHandler = new RecordsetHandler(this);
            _helper = new DataListViewModelHelper(this);
        }

        public IJsonObjectsView JsonObjectsView => CustomContainer.GetInstancePerRequestType<IJsonObjectsView>();

        private void ViewJsonObjects(IComplexObjectItemModel item)
        {
            if (item != null)
            {
                if (JsonObjectsView != null)
                {
                    var json = item.GetJson();
                    JsonObjectsView.ShowJsonString(JSONUtils.Format(json));
                }
            }
        }

        private bool CanViewComplexObjects(Object itemx)
        {
            var item = itemx as IDataListItemModel;
            return item != null && !item.IsComplexObject;
        }

        private bool CanDelete(Object itemx)
        {
            var item = itemx as IDataListItemModel;
            return item != null && !item.IsUsed;
        }

        #endregion Ctor

        #region Commands

        public ICommand ClearSearchTextCommand { get; private set; }

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

        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand = new RelayCommand(item =>
                {
                    RemoveDataListItem(item as IDataListItemModel);
                    WriteToResourceModel();
                    FindUnusedAndMissingCommand.RaiseCanExecuteChanged();
                    DeleteCommand.RaiseCanExecuteChanged();
                    UpdateIntellisenseList();
                }, CanDelete));
            }
        }

        public RelayCommand ViewComplexObjectsCommand
        {
            get
            {
                return _viewComplexObjectsCommand ?? (_viewComplexObjectsCommand = new RelayCommand(item =>
                {
                    ViewJsonObjects(item as IComplexObjectItemModel);
                }, CanViewComplexObjects));
            }
        }

        #endregion Commands

        #region Add/Remove Missing Methods

        public void SetIsUsedDataListItems(IList<IDataListVerifyPart> parts, bool isUsed)
        {
            foreach (var part in parts)
            {
                if (part.IsScalar)
                {
                    _partIsUsed.SetScalarPartIsUsed(part, isUsed);
                }
                else
                {
                    _partIsUsed.SetRecordSetPartIsUsed(part, isUsed);
                }
                if (part.IsJson)
                {
                    _partIsUsed.SetComplexObjectSetPartIsUsed(part, isUsed);
                }
            }
        }

        public void RemoveUnusedDataListItems()
        {
            _scalarHandler.RemoveUnusedScalars();
            _recordsetHandler.RemoveUnusedRecordSets();
            _complexObjectHandler.RemoveUnusedComplexObjects();

            WriteToResourceModel();
            FindUnusedAndMissingCommand.RaiseCanExecuteChanged();
            ViewComplexObjectsCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            UpdateIntellisenseList();
        }

        public void AddMissingDataListItems(IList<IDataListVerifyPart> parts)
        {
            var tmpRecsetList = new List<IRecordSetItemModel>();
            foreach (var part in parts)
            {
                if (part.IsJson)
                {
                    _complexObjectHandler.AddComplexObject(part);
                }
                else
                {
                    if (part.IsScalar)
                    {
                        _scalarHandler.AddMissingScalarParts(part);
                    }
                    else
                    {
                        var recsetToAddTo = RecsetCollection.FirstOrDefault(c => c.DisplayName == part.Recordset);

                        var tmpRecset = tmpRecsetList.FirstOrDefault(c => c.DisplayName == part.Recordset);

                        if (recsetToAddTo != null)
                        {
                            _recordsetHandler.AddMissingRecordSetPart(recsetToAddTo, part);
                        }
                        else if (tmpRecset != null)
                        {
                            _recordsetHandler.AddMissingTempRecordSet(part, tmpRecset);
                        }
                        else
                        {
                            var recset = DataListItemModelFactory.CreateRecordSetItemModel(part.Recordset, part.Description);
                            tmpRecsetList.Add(recset);
                        }
                    }
                }
            }

            _recordsetHandler.AddMissingTempRecordSetList(tmpRecsetList);

            _scalarHandler.RemoveBlankScalars();
            _recordsetHandler.RemoveBlankRecordsets();
            _recordsetHandler.RemoveBlankRecordsetFields();
            _complexObjectHandler.RemoveBlankComplexObjects();
            if (parts.Count > 0)
                AddBlankRow(null);

            UpdateIntellisenseList();

            WriteToResourceModel();
        }

        private void UpdateIntellisenseList()
        {
            if(_scalarCollection != null && _recsetCollection != null && _complexObjectCollection != null && _complexObjectHandler != null)
            {
                var items = RefreshTries(_scalarCollection.ToList(), new List<string>()).Union(RefreshRecordSets(_recsetCollection.ToList(), new List<string>())).Union(_complexObjectHandler.RefreshJsonObjects(_complexObjectCollection.ToList()));
                if(Provider != null)
                {
                    Provider.VariableList = new ObservableCollection<string>(items);
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
            var items = RefreshTries(_scalarCollection.ToList(), new List<string>()).Union(RefreshRecordSets(_recsetCollection.ToList(), new List<string>())).Union(_complexObjectHandler.RefreshJsonObjects(_complexObjectCollection.ToList()));
            Provider.VariableList = new ObservableCollection<string>(items);
        }
        
        private IEnumerable<string> RefreshTries(IEnumerable<IScalarItemModel> toList, IList<string> accList)
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

        private IEnumerable<string> RefreshRecordSets(IEnumerable<IRecordSetItemModel> toList, IList<string> accList)
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
                    _scalarHandler.AddRowToScalars();
                }
                else if (item is IRecordSetItemModel)
                {
                    _recordsetHandler.AddRowToRecordsets();
                }
            }
            else
            {
                _scalarHandler.AddRowToScalars();
                _recordsetHandler.AddRowToRecordsets();
            }
        }

        public void RemoveBlankRows(IDataListItemModel item)
        {
            if (item == null) return;

            if (!(item is IRecordSetItemModel) && item is IScalarItemModel)
            {
                _scalarHandler.RemoveBlankScalars();
            }
            else if (item is IRecordSetItemModel)
            {
                _recordsetHandler.RemoveBlankRecordsets();
            }
            else
            {
                _recordsetHandler.RemoveBlankRecordsetFields();
            }
        }

        public void RemoveDataListItem(IDataListItemModel itemToRemove)
        {
            if (itemToRemove == null)
                return;

            var complexObj = itemToRemove as IComplexObjectItemModel;
            if (complexObj != null)
            {
                var complexObjectItemModels = complexObj.Children;
                var allChildren = complexObjectItemModels.Flatten(model => model.Children).ToList();
                var notUsedItems = allChildren.Where(x => !x.IsUsed).ToList();
                foreach (var complexObjectItemModel in notUsedItems)
                {
                    RemoveUnusedChildComplexObjects(complexObj, complexObjectItemModel);
                }
                if (complexObj.Children.Count == 0)
                {
                    ComplexObjectCollection.Remove(complexObj);
                }
                else
                {
                    complexObj.IsUsed = true;
                }
            }

            if (itemToRemove is IScalarItemModel)
            {
                var item = itemToRemove as IScalarItemModel;
                ScalarCollection.Remove(item);
                CheckDataListItemsForDuplicates(DataList);
            }
            else if (itemToRemove is IRecordSetItemModel)
            {
                var item = itemToRemove as IRecordSetItemModel;
                RecsetCollection.Remove(item);
                CheckDataListItemsForDuplicates(DataList);
            }
            else
            {
                foreach (var recset in RecsetCollection)
                {
                    var item = itemToRemove as IRecordSetFieldItemModel;
                    recset.Children.Remove(item);
                    CheckDataListItemsForDuplicates(recset.Children);
                }
            }
            FindUnusedAndMissingCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }

        private void RemoveUnusedChildComplexObjects(IComplexObjectItemModel parentItemModel, IComplexObjectItemModel itemToRemove)
        {
            _complexObjectHandler.RemoveUnusedChildComplexObjects(parentItemModel, itemToRemove);
        }

        public string WriteToResourceModel()
        {
            ScalarCollection.ForEach(_scalarHandler.FixNamingForScalar);
            _recordsetHandler.AddRecordsetNamesIfMissing();
            var result = GetDataListString();
            if (Resource != null)
            {
                Resource.DataList = result;
            }

            return result;
        }

        public void AddRecordsetNamesIfMissing()
        {
            _recordsetHandler.AddRecordsetNamesIfMissing();
        }

        public void ValidateNames(IDataListItemModel item)
        {
            if (item == null)
                return;

            if (item is IRecordSetItemModel)
            {
                _recordsetHandler.ValidateRecordset();
            }
            else if (item is IRecordSetFieldItemModel)
            {
                IRecordSetFieldItemModel rs = (IRecordSetFieldItemModel)item;
                _recordsetHandler.ValidateRecordsetChildren(rs.Parent);
            }
            else
            {
                ValidateScalar();
            }
        }

        #endregion Methods

        #region Private Methods

        private void ValidateScalar()
        {
            CheckDataListItemsForDuplicates(DataList);
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private void CheckDataListItemsForDuplicates(IEnumerable<IDataListItemModel> itemsToCheck)
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

        private bool HasAnyUnusedItems()
        {
            if (!HasItems())
            {
                return false;
            }

            bool hasUnused;

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
                if (!hasUnused)
                {
                    hasUnused = RecsetCollection.SelectMany(sc => sc.Children).Any(sc => !sc.IsUsed);
                }
                if (hasUnused)
                {
                    DeleteCommand.RaiseCanExecuteChanged();
                    return true;
                }
            }

            if (ComplexObjectCollection != null)
            {
                hasUnused = ComplexObjectCollection.Any(sc => !sc.IsUsed);
                if (hasUnused)
                {
                    DeleteCommand.RaiseCanExecuteChanged();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Creates the full data list.
        /// </summary>
        /// <returns></returns>
        private OptomizedObservableCollection<IDataListItemModel> CreateFullDataList() => _helper.CreateFullDataList();

        /// <summary>
        ///     Sorts the items.
        /// </summary>
        private void SortItems()
        {
            try
            {
                IsSorting = true;
                if (_scalarCollection.Any(model => !model.IsBlank))
                {
                    _scalarHandler.SortScalars(_toggleSortOrder);
                }
                if (_recsetCollection.Any(model => !model.IsBlank))
                {
                    _recordsetHandler.SortRecset(_toggleSortOrder);
                }
                if (_complexObjectCollection.Any(model => !model.IsBlank))
                {
                    _complexObjectHandler.SortComplexObjects(_toggleSortOrder);
                }
                _toggleSortOrder = !_toggleSortOrder;
                WriteToResourceModel();
            }
            finally { IsSorting = false; }
        }

        public bool IsSorting { get; set; }

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
                    if (!string.IsNullOrEmpty(Resource.DataList))
                        ConvertDataListStringToCollections(Resource.DataList);
                }
                catch (Exception)
                {
                    errors.AddError(ErrorResource.InvalidVariableList);
                }
            }
            else
            {
                RecsetCollection.Clear();
                ScalarCollection.Clear();
                ComplexObjectCollection.Clear();

                _recordsetHandler.AddRecordSet();
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
                _recordsetHandler.AddRecordSet();
            }
            BaseCollection.Add(recordsetsNode);

            DataListHeaderItemModel complexObjectNode = DataListItemModelFactory.CreateDataListHeaderItem("Object");
            BaseCollection.Add(complexObjectNode);

            AddBlankRow(null);

            BaseCollection[0].Children = ScalarCollection;
            BaseCollection[1].Children = RecsetCollection;
            BaseCollection[2].Children = ComplexObjectCollection;

            WriteToResourceModel();
        }

        public void ClearCollections()
        {
            BaseCollection.Clear();
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public void GenerateComplexObjectFromJson(string parentObjectName, string json)
        {
            _complexObjectHandler.GenerateComplexObjectFromJson(parentObjectName, json);
        }

        public void ConvertDataListStringToCollections(string dataList)
        {
            RecsetCollection.Clear();
            ScalarCollection.Clear();
            ComplexObjectCollection.Clear();
            try
            {
                var xDoc = new XmlDocument();
                xDoc.LoadXml(dataList);
                if (xDoc.DocumentElement == null) return;
                var children = xDoc.DocumentElement.ChildNodes;
                foreach (XmlNode child in children)
                {
                    if (DataListUtil.IsSystemTag(child.Name)) continue;
                    if (IsJsonAttribute(child))
                    {
                        _complexObjectHandler.AddComplexObjectFromXmlNode(child, null);
                    }
                    else
                    {
                        if (child.HasChildNodes)
                        {
                            _recordsetHandler.AddRecordSets(child);
                        }
                        else
                        {
                            AddScalars(child);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
            }
        }

        private bool IsJsonAttribute(XmlNode child) => _helper.IsJsonAttribute(child);

        private void AddScalars(XmlNode c)
        {
            _scalarHandler.AddScalars(c);
        }

        // ReSharper disable InconsistentNaming

        private const string RootTag = "DataList";
        private const string Description = "Description";
        private const string IsEditable = "IsEditable";

        private string GetDataListString()
        {
            StringBuilder result = new StringBuilder("<" + RootTag + ">");
            foreach (var recSet in RecsetCollection.Where(model => !string.IsNullOrEmpty(model.DisplayName)))
            {
                IEnumerable<IDataListItemModel> filledRecordSet = recSet.Children.Where(c => !c.IsBlank && !c.HasError);
                IList<Dev2Column> cols = filledRecordSet.Select(child => DataListFactory.CreateDev2Column(child.DisplayName, child.Description, child.IsEditable, child.ColumnIODirection)).ToList();

                AddItemToBuilder(result, recSet);
                result.Append(">");
                foreach (var col in cols)
                {
                    result.AppendFormat("<{0} {1}=\"{2}\" {3}=\"{4}\" {5}=\"{6}\" />", col.ColumnName
                    , Description
                    , col.ColumnDescription
                    , IsEditable
                    , col.IsEditable
                    , GlobalConstants.DataListIoColDirection
                    , col.ColumnIODirection);
                }
                result.Append("</");
                result.Append(recSet.DisplayName);
                result.Append(">");
            }

            IList<IScalarItemModel> filledScalars = ScalarCollection?.Where(scalar => !scalar.IsBlank && !scalar.HasError).ToList() ?? new List<IScalarItemModel>();
            foreach (var scalar in filledScalars)
            {
                AddItemToBuilder(result, scalar);
                result.Append("/>");
            }
            var complexObjectItemModels = ComplexObjectCollection.Where(model => !string.IsNullOrEmpty(model.DisplayName) && !model.HasError);
            foreach (var complexObjectItemModel in complexObjectItemModels)
            {
                _complexObjectHandler.AddComplexObjectsToBuilder(result, complexObjectItemModel);
            }

            result.Append("</" + RootTag + ">");
            return result.ToString();
        }

        private void AddItemToBuilder(StringBuilder result, IDataListItemModel item)
        {
            _helper.AddItemToBuilder(result, item);
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
            return (ScalarCollection != null && ScalarCollection.Count > 1) || (RecsetCollection != null && RecsetCollection.Count > 1) || (ComplexObjectCollection != null && ComplexObjectCollection.Count >= 1);
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

        private void ShowUnusedDataListVariables(IResourceModel resourceModel, IList<IDataListVerifyPart> listOfUnused, IList<IDataListVerifyPart> listOfUsed)
        {
            if (resourceModel != Resource) return;
            if (listOfUnused != null && listOfUnused.Count != 0)
                SetIsUsedDataListItems(listOfUnused, false);
            else
                UpdateDataListItemsAsUsed();
            if (listOfUsed != null && listOfUsed.Count > 0)
                SetIsUsedDataListItems(listOfUsed, true);
        }

        private void UpdateDataListItemsAsUsed()
        {
            _scalarHandler.SetScalarItemsAsUsed();
            _recordsetHandler.SetRecordSetItemsAsUsed();
        }

        #endregion Implementation of ShowUnusedDataListVariables

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
                missingWorkflowParts.AddRange(_missingDataList.MissingScalars(partsToVerify, excludeUnusedItems));
                missingWorkflowParts.AddRange(_missingDataList.MissingRecordsets(partsToVerify, excludeUnusedItems));
            }
            _complexObjectHandler.DetectUnusedComplexObjects(partsToVerify);
            FindUnusedAndMissingCommand.RaiseCanExecuteChanged();
            return missingWorkflowParts;
        }

        public List<IDataListVerifyPart> MissingDataListParts(IList<IDataListVerifyPart> partsToVerify)
        {
            var missingDataParts = new List<IDataListVerifyPart>();
            foreach (var part in partsToVerify.Where(part => DataList != null))
            {
                _recordsetHandler.FindMissingPartsForRecordset(part, missingDataParts);
                _scalarHandler.FindMissingForScalar(part, missingDataParts);
            }
            return missingDataParts;
        }

        public List<IDataListVerifyPart> UpdateDataListItems(IResourceModel resourceModel, IList<IDataListVerifyPart> workflowFields)
        {
            IList<IDataListVerifyPart> removeParts = MissingWorkflowItems(workflowFields);
            var filteredDataListParts = MissingDataListParts(workflowFields);
            ShowUnusedDataListVariables(resourceModel, removeParts, workflowFields);

            ViewModelUtils.RaiseCanExecuteChanged(DeleteCommand);

            if (resourceModel != Resource) return new List<IDataListVerifyPart>();
            AddMissingDataListItems(filteredDataListParts);
            return filteredDataListParts;
        }

        public string DataListErrorMessage
        {
            get
            {
                if (!HasErrors) return "";
                var allErrorMessages = RecsetCollection.Select(model =>
                {
                    string errorMessage;
                    if (_recordsetHandler.BuildRecordSetErrorMessages(model, out errorMessage))
                    {
                        return errorMessage;
                    }
                    if (model.HasError)
                    {
                        return BuildErrorMessage(model);
                    }
                    return null;
                });

                var errorMessages = allErrorMessages as IList<string> ?? allErrorMessages.ToList();
                string.Join(Environment.NewLine, errorMessages.Where(s => !string.IsNullOrEmpty(s)));

                allErrorMessages = errorMessages.Union(ScalarCollection.Select(model => model.HasError ? BuildErrorMessage(model) : null));

                allErrorMessages = allErrorMessages.Union(ComplexObjectCollection.Flatten(model => model.Children).Select(model => model.HasError ? BuildErrorMessage(model) : null));

                var completeErrorMessage = Environment.NewLine + string.Join(Environment.NewLine, allErrorMessages.Where(s => !string.IsNullOrEmpty(s)));

                return completeErrorMessage;
            }
        }

        public ISuggestionProvider Provider { get; set; }
        

        private static string BuildErrorMessage(IDataListItemModel model)
        {
            return DataListUtil.AddBracketsToValueIfNotExist(model.DisplayName) + " : " + model.ErrorMessage;
        }
    }
}