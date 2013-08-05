#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Studio.Core;
using Dev2.Studio.Core.DataList;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.WorkSurface;
using Infragistics.Collections;

#endregion

namespace Dev2.Studio.ViewModels.DataList
{
    public class DataListViewModel : SimpleBaseViewModel, IDataListViewModel,
                                     IHandle<ShowUnusedDataListVariablesMessage>, IHandle<AddMissingDataListItems>
    {
        #region Fields

        private RelayCommand _addRecordsetCommand;
        private OptomizedObservableCollection<DataListHeaderItemModel> _baseCollection;
        private IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        private RelayCommand _findUnusedAndMissingDataListItems;
        private OptomizedObservableCollection<IDataListItemModel> _recsetCollection;
        private OptomizedObservableCollection<IDataListItemModel> _scalarCollection;
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

        public OptomizedObservableCollection<DataListHeaderItemModel> BaseCollection
        {
            get { return _baseCollection; }
            set
            {
                _baseCollection = value;
                NotifyOfPropertyChange(() => BaseCollection);
            }
        }

        private IDataListValidator Validator { get; set; }

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

        public IResourceModel Resource { get; private set; }

        public OptomizedObservableCollection<IDataListItemModel> DataList
        {
            get { return CreateFullDataList(); }
        }

        public OptomizedObservableCollection<IDataListItemModel> ScalarCollection
        {
            get
            {
                if (_scalarCollection == null)
                {
                    _scalarCollection = new OptomizedObservableCollection<IDataListItemModel>();
                    _scalarCollection.CollectionChanged += (o, args) =>
                    {
                        NotifyOfPropertyChange(() => FindUnusedAndMissingCommand);
                        NotifyOfPropertyChange(() => SortCommand);
                    };
                }
                return _scalarCollection;
            }
        }

        public OptomizedObservableCollection<IDataListItemModel> RecsetCollection
        {
            get
            {
                if (_recsetCollection == null)
                {
                    _recsetCollection = new OptomizedObservableCollection<IDataListItemModel>();
                    _recsetCollection.CollectionChanged += (o, args) =>
                        {
                            NotifyOfPropertyChange(() => FindUnusedAndMissingCommand);
                            NotifyOfPropertyChange(() => SortCommand);
                        };
                }
                return _recsetCollection;
            }
        }

        //2013.06.04: Ashley Lewis for bug 9280 - sort both ways
        bool _toggleSortOrder = true;

        #endregion Properties

        #region Ctor

        public DataListViewModel()
        {
            Validator = new DataListValidator();
        }

        #endregion

        #region Commands

        public ICommand AddRecordsetCommand
        {
            get
            {
                return _addRecordsetCommand ??
                       (_addRecordsetCommand = new RelayCommand(method => AddRecordSet()));
            }
        }

        public ICommand SortCommand
        {
            get
            {
                return _sortCommand ??
                       (_sortCommand = new RelayCommand(method => SortItems(), (p) => CanSortItems));
            }
        }

        public ICommand FindUnusedAndMissingCommand
        {
            get
            {
                return _findUnusedAndMissingDataListItems ??
                       (_findUnusedAndMissingDataListItems = 
                       new RelayCommand(method => FindUnusedAndMissing(), o => HasAnyUnusedItems()));
            }
        }

        #endregion Commands

        #region Add/Remove Missing Methods
        public void FindMissing()
        {
            var model = Parent as WorkSurfaceContextViewModel;

            if (model == null)
            {
                return;
            }

            var vm = model;
            vm.FindMissing();
        }

        public void AddMissingDataListItems(IList<IDataListVerifyPart> parts)
        {
            AddMissingDataListItems(parts, false);
        }

        public void SetUnusedDataListItems(IList<IDataListVerifyPart> parts)
        {

            //if (ScalarCollection.Any(sc => sc.IsUsed == false))
            //{
            //    foreach (var dataListItemModel in ScalarCollection)
            //    {
            //        dataListItemModel.IsUsed = true;
            //    }    
            //}
            
            //if (RecsetCollection.Any(rc => rc.IsUsed == false))
            //{
            //    foreach (var dataListItemModel in RecsetCollection)
            //    {
            //        dataListItemModel.IsUsed = true;
            //        foreach (var listItemModel in dataListItemModel.Children)
            //        {
            //            listItemModel.IsUsed = true;
            //        }
            //    }    
            //}
            
            IList<IDataListItemModel> tmpRecsets = new List<IDataListItemModel>();
            foreach (var part in parts)
            {
                if (part.IsScalar)
                {
                    var scalarsToRemove = ScalarCollection.Where(c => c.Name == part.Field);
                    scalarsToRemove.ToList().ForEach(scalarToRemove =>
                    {
                        if (scalarToRemove != null)
                        {
                            scalarToRemove.IsUsed = false;
                        }
                    });
                    
                }
                else
                {
                    var recsetsToRemove = RecsetCollection.Where(c => c.Name == part.Recordset && c.IsRecordset);
                    recsetsToRemove.ToList().ForEach(recsetToRemove =>
                    {

                        if (string.IsNullOrEmpty(part.Field))
                        {
                            if (recsetToRemove != null)
                            {
                                recsetToRemove.IsUsed = false;
                            }
                        }
                        else
                        {
                            if (recsetToRemove != null)
                            {
                                var childrenToRemove = recsetToRemove.Children.Where(c => c.Name == part.Field && c.IsField);
                                childrenToRemove.ToList().ForEach(childToRemove =>
                                {
                                    if (childToRemove != null)
                                    {
                                        childToRemove.IsUsed = false;
                                    }
                                });
                                
                            }
                        }
                    });
                    
                }
            }

            foreach (var item in tmpRecsets)
            {
                RecsetCollection.Remove(item);
                Validator.Remove(item);
            }

            WriteToResourceModel();
            EventAggregator.Publish(new UpdateIntellisenseMessage());
        }

        public void RemoveUnusedDataListItems()
        {
            var unusedScalars = ScalarCollection.Where(c => c.IsUsed == false).ToList();
            if (unusedScalars.Any())
            {
                foreach (var dataListItemModel in unusedScalars)
                {
                    ScalarCollection.Remove(dataListItemModel);
                    Validator.Remove(dataListItemModel);
                }
            }
            var unusedRecordsets = RecsetCollection.Where(c => c.IsUsed == false).ToList();
            if (unusedRecordsets.Any())
            {
                foreach (var dataListItemModel in unusedRecordsets)
                {
                    RecsetCollection.Remove(dataListItemModel);
                    Validator.Remove(dataListItemModel);
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
                            recset.Validator.Remove(unusedRecsetChild);
                        }
                    }
                }
            }

            WriteToResourceModel();
            EventAggregator.Publish(new UpdateIntellisenseMessage());
        }

        public void AddMissingDataListItems(IList<IDataListVerifyPart> parts, bool async)
        {
            IList<IDataListItemModel> tmpRecsetList = new List<IDataListItemModel>();
            foreach (var part in parts)
            {
                if (part.IsScalar)
                {
                    if (ScalarCollection.FirstOrDefault(c => c.Name == part.Field) == null)
                    {
                        IDataListItemModel scalar = DataListItemModelFactory.CreateDataListModel(part.Field,
                                                                                                 part.Description,
                                                                                                 enDev2ColumnArgumentDirection
                                                                                                     .None);
                        ScalarCollection.Insert(ScalarCollection.Count - 1, scalar);
                        Validator.Add(scalar);
                    }
                }
                else
                {
                    IDataListItemModel recsetToAddTo = RecsetCollection.
                        FirstOrDefault(c => c.Name == part.Recordset && c.IsRecordset);

                    IDataListItemModel tmpRecset = tmpRecsetList.FirstOrDefault(c => c.Name == part.Recordset);

                    if (recsetToAddTo != null)
                    {
                        if (recsetToAddTo.Children.FirstOrDefault(c => c.Name == part.Field) == null)
                        {
                            IDataListItemModel child = DataListItemModelFactory.CreateDataListModel(part.Field,
                                                                                                    part.Description,
                                                                                                    recsetToAddTo);
                            if (recsetToAddTo.Children.Count > 0)
                            {
                                recsetToAddTo.Children.Insert(recsetToAddTo.Children.Count - 1, child);
                            }
                            else
                            {
                                recsetToAddTo.Children.Add(child);
                            }

                            recsetToAddTo.Validator.Add(child);
                        }
                    }
                    else if (tmpRecset != null)
                    {
                        IDataListItemModel child = DataListItemModelFactory.CreateDataListModel
                            (part.Field, part.Description, tmpRecset);
                        child.Name = part.Recordset+"()."+part.Field;
                        tmpRecset.Children.Add(child);
                        tmpRecset.Validator.Add(child);
                    }
                    else
                    {
                        IDataListItemModel recset = DataListItemModelFactory.CreateDataListModel
                            (part.Recordset, part.Description, enDev2ColumnArgumentDirection.None);

                        tmpRecsetList.Add(recset);
                    }
                }
            }

            foreach (var item in tmpRecsetList)
            {
                if (item.Children.Count == 0)
                {
                    item.Children.Add(DataListItemModelFactory.CreateDataListModel(string.Empty, string.Empty, item));
                }
                if (RecsetCollection.Count > 0)
                {
                    RecsetCollection.Insert(RecsetCollection.Count - 1, item);
                }
                else
                {
                    RecsetCollection.Add(item);
                }
                Validator.Add(item);
            }

            WriteToResourceModel();
            EventAggregator.Publish(new UpdateIntellisenseMessage());
            RemoveBlankScalars();
            RemoveBlankRecordsets();
            RemoveBlankRecordsetFields();
            AddBlankRow(null);
        }

        public IList<IDataListItemModel> CreateDataListItems(IList<IDataListVerifyPart> parts, bool isAdd)
        {
            var results = new List<IDataListItemModel>();

            foreach (var part in parts)
            {
                IDataListItemModel item;
                if (part.IsScalar)
                {
                    item = DataListItemModelFactory.CreateDataListItemViewModel
                        (this, part.Field, part.Description, null);

                    results.Add(item);
                }
                else if (string.IsNullOrEmpty(part.Field))
                {
                    item = DataListItemModelFactory.CreateDataListItemViewModel
                        (this, part.Recordset, part.Description, null, true);
                    results.Add(item);
                }
                else
                {
                    IDataListItemModel recset
                        = results.FirstOrDefault(c => c.IsRecordset && c.Name == part.Recordset) ??
                          DataList.FirstOrDefault(c => c.IsRecordset && c.Name == part.Recordset);

                    if (recset == null && isAdd)
                    {
                        item = DataListItemModelFactory.CreateDataListItemViewModel(this, part.Recordset,
                                                                                    part.Description, null, true);

                        results.Add(item);
                    }

                    if (recset != null)
                    {
                        if (isAdd)
                        {
                            item = DataListItemModelFactory.CreateDataListItemViewModel(this, part.Field,
                                                                                        part.Description, recset);

                            recset.Children.Add(item);
                        }
                        else
                        {
                            IDataListItemModel removeItem = recset.Children.FirstOrDefault(c => c.Name == part.Field);
                            if (removeItem != null)
                            {
                                if (recset.Children.Count == 1)
                                {
                                    recset.Children[0].DisplayName = "";
                                    recset.Children[0].Description = "";
                                }
                                else
                                {
                                    recset.Children.Remove(removeItem);
                                }
                            }
                        }
                    }
                }
            }
            return results;
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
            AddRecordsetNamesIfMissing();
        }

        public void FilterItems()
        {
            if (SearchText == null) return;

            foreach (var item in ScalarCollection)
            {
                item.IsVisable = item.Name.Contains(SearchText);
            }

            foreach (var item in RecsetCollection)
            {
                bool parentVis = false;
                foreach (var child in item.Children)
                {
                    if (child.Name.Contains(SearchText))
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
            if (item != null)
            {
                if (!item.IsRecordset && !item.IsField)
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
            if (item == null) return;

            if (!item.IsRecordset && !item.IsField)
            {
                RemoveBlankScalars();
            }
            else if (item.IsRecordset)
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
            if (itemToRemove == null) return;

            if (!itemToRemove.IsRecordset && !itemToRemove.IsField)
            {
                ScalarCollection.Remove(itemToRemove);
                Validator.Remove(itemToRemove);
            }
            else if (itemToRemove.IsRecordset)
            {
                RecsetCollection.Remove(itemToRemove);
                Validator.Remove(itemToRemove);
            }
            else
            {
                foreach (var recset in RecsetCollection)
                {
                    recset.Children.Remove(itemToRemove);
                    recset.Validator.Remove(itemToRemove);
                }
            }
        }

        public string WriteToResourceModel()
        {
            string result = string.Empty;
            string errorString;
            AddRecordsetNamesIfMissing();
            IBinaryDataList postDl = ConvertIDataListItemModelsToIBinaryDataList(out errorString);

            if (string.IsNullOrEmpty(errorString))
            {
                ErrorResultTO errors;
                result = CreateXmlDataFromBinaryDataList(postDl, out errors);
                if(Resource != null)
                {
                    Resource.DataList = result;
                }
            }

            _compiler.ForceDeleteDataListByID(postDl.UID);
            if (!string.IsNullOrEmpty(errorString))
            {
                throw new Exception(errorString);
            }

            return result;
        }

        public void CheckName(IDataListItemModel item)
        {
            if (item.IsRecordset && item.DisplayName.Contains("()"))
            {
                item.DisplayName = item.DisplayName.Remove
                    (item.DisplayName.IndexOf("(", StringComparison.Ordinal));
            }
            if (!item.IsField || !item.DisplayName.Contains("().")) return;

            int startIndex = item.DisplayName.IndexOf(".", StringComparison.Ordinal) + 1;

            item.DisplayName = item.DisplayName.Substring(startIndex, item.DisplayName.Length - startIndex);
        }

        public void AddRecordsetNamesIfMissing()
        {
            var recsetNum = RecsetCollection != null ? RecsetCollection.Count : 0;
            int recsetCount = 0;

            while (recsetCount < recsetNum)
            {
                IDataListItemModel recset = RecsetCollection[recsetCount];

                if (!string.IsNullOrWhiteSpace(recset.DisplayName))
                {
                    if (!recset.DisplayName.EndsWith("()"))
                    {
                        recset.DisplayName = string.Concat(recset.DisplayName, "()");
                    }
                    int childrenNum = recset.Children.Count;
                    int childrenCount = 0;

                    while (childrenCount < childrenNum)
                    {
                        IDataListItemModel child = recset.Children[childrenCount];

                        if (!string.IsNullOrWhiteSpace(child.DisplayName))
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
                        }
                        childrenCount++;
                    }
                }
                recsetCount++;
            }
        }

        public void ValidateNames(IDataListItemModel item)
        {
            if (item == null) return;

            if (item.IsField)
            {
                item.Parent.Validator.Move(item);
            }
            else
            {
                Validator.Move(item);
            }
        }

        private void AddRowToScalars()
        {
            List<IDataListItemModel> blankList = ScalarCollection.Where(c => c.IsBlank).ToList();
            if (blankList.Count != 0) return;

            IDataListItemModel scalar = DataListItemModelFactory.CreateDataListModel(string.Empty);
            ScalarCollection.Add(scalar);
            Validator.Add(scalar);
        }

        private void AddRowToRecordsets()
        {
            List<IDataListItemModel> blankList = RecsetCollection.Where
                (c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();

            if (blankList.Count == 0)
            {
                AddRecordSet();
            }

            foreach (var recset in RecsetCollection)
            {
                List<IDataListItemModel> blankChildList = recset.Children.Where(c => c.IsBlank).ToList();
                if (blankChildList.Count != 0) continue;

                IDataListItemModel newChild = DataListItemModelFactory.CreateDataListModel(string.Empty);
                newChild.Parent = recset;
                recset.Children.Add(newChild);
                recset.Validator.Add(newChild);
            }
        }

        public void RemoveBlankRecordsets()
        {
            List<IDataListItemModel> blankList = RecsetCollection.Where
                (c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank)
                                                                 .ToList();

            if (blankList.Count <= 1) return;

            RecsetCollection.Remove(blankList.First());
            Validator.Remove(blankList.First());
        }

        public void RemoveBlankScalars()
        {
            List<IDataListItemModel> blankList = ScalarCollection.Where(c => c.IsBlank).ToList();

            if (blankList.Count <= 1) return;

            ScalarCollection.Remove(blankList.First());
            Validator.Remove(blankList.First());
        }

        public void RemoveBlankRecordsetFields()
        {
            foreach (var recset in RecsetCollection)
            {
                List<IDataListItemModel> blankChildList = recset.Children.Where(c => c.IsBlank).ToList();

                if (blankChildList.Count <= 1) continue;

                recset.Children.Remove(blankChildList.First());
                recset.Validator.Remove(blankChildList.First());
            }
        }

        #endregion Methods

        #region Private Methods      
        private bool HasAnyUnusedItems()
        {
            if (!HasItems()) return false;

            bool hasUnused = false;

            if (ScalarCollection != null)
            {
                hasUnused = ScalarCollection.Any(sc => !sc.IsUsed);
                if (hasUnused)
                {
                    return true;
                }
            }

            if (RecsetCollection != null)
            {
                hasUnused = RecsetCollection.Any(sc => !sc.IsUsed);
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

            return fullDataList;
        }

        /// <summary>
        ///     Finds the unused and missing data list items.
        /// </summary>
        private void FindUnusedAndMissing()
        {
            EventAggregator.Publish(new AddRemoveDataListItemsMessage(this));
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
            recset.Validator.Add(childItem);
            RecsetCollection.Add(recset);
            Validator.Add(recset);
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
            IList<IDataListItemModel> newRecsetCollection;
            if(ascending)
            {
                newRecsetCollection = RecsetCollection.OrderBy(c => c.DisplayName).ToList();
            }
            else
            {
                newRecsetCollection = RecsetCollection.OrderByDescending(c => c.DisplayName).ToList();
            }
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
        private void CreateListsOfIDataListItemModelToBindTo(out string errorString)
        {
            errorString = string.Empty;
            if (!string.IsNullOrEmpty(Resource.DataList))
            {
                ErrorResultTO errors;
                IBinaryDataList binarnyDL = CreateBinaryDataListFromXmlData(Resource.DataList, out errors);
                if (!errors.HasErrors())
                {
                    ConvertBinaryDataListToListOfIDataListItemModels(binarnyDL, out errorString);
                }
                else
                {
                    string errorMessage = errors.FetchErrors()
                                                .Aggregate(string.Empty, (current, error) => current + error);
                    throw new Exception(errorMessage);
                }
                _compiler.ForceDeleteDataListByID(binarnyDL.UID);
            }
            else
            {
                RecsetCollection.Clear();
                AddRecordSet();
                ScalarCollection.Clear();
            }

            BaseCollection = new OptomizedObservableCollection<DataListHeaderItemModel>();

            DataListHeaderItemModel varNode = DataListItemModelFactory.CreateDataListHeaderItem("Variable");
            if (ScalarCollection.Count == 0)
            {
                ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel(string.Empty));
            }
            varNode.Children = ScalarCollection;
            BaseCollection.Add(varNode);

            DataListHeaderItemModel recordsetsNode = DataListItemModelFactory.CreateDataListHeaderItem("Recordset");
            if (RecsetCollection.Count == 0)
            {
                AddRecordSet();
            }
            recordsetsNode.Children = RecsetCollection;
            BaseCollection.Add(recordsetsNode);
        }

        /// <summary>
        ///     Creates a binary data list from XML data.
        /// </summary>
        /// <param name="xmlDataList">The XML data list.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private IBinaryDataList CreateBinaryDataListFromXmlData(string xmlDataList, out ErrorResultTO errors)
        {
            IBinaryDataList result = null;
            var allErrors = new ErrorResultTO();
            Guid dlGuid = _compiler.ConvertTo(
                DataListFormat.CreateFormat(GlobalConstants._Studio_XML), xmlDataList, xmlDataList, out errors);

            if (!errors.HasErrors())
            {
                result = _compiler.FetchBinaryDataList(dlGuid, out errors);
                if (errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }
            }

            _compiler.ForceDeleteDataListByID(dlGuid);
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
            Guid dlGuid = _compiler.PushBinaryDataList(binaryDataList.UID, binaryDataList, out errors);
            string result = _compiler.ConvertFrom(dlGuid,
                                                  DataListFormat.CreateFormat(GlobalConstants._Studio_XML),
                                                  enTranslationDepth.Shape, out errors);

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

            foreach (var entry in listOfEntries)
            {
                if (entry.IsRecordset)
                {
                    IDataListItemModel recset = DataListItemModelFactory.CreateDataListModel(
                        entry.Namespace, entry.Description, entry.ColumnIODirection);

                    recset.IsEditable = entry.IsEditable;
                    Validator.Add(recset);

                    foreach (var col in entry.Columns)
                    {
                        IDataListItemModel child = DataListItemModelFactory.CreateDataListModel(
                            col.ColumnName, col.ColumnDescription, col.ColumnIODirection);

                        child.Parent = recset;
                        child.IsEditable = col.IsEditable;
                        recset.Children.Add(child);
                        recset.Validator.Add(child);
                    }

                    RecsetCollection.Add(recset);
                }
                else
                {
                    IDataListItemModel scalar = DataListItemModelFactory.CreateDataListModel(
                        entry.Namespace, entry.Description, entry.ColumnIODirection);

                    scalar.IsEditable = entry.IsEditable;
                    ScalarCollection.Add(scalar);
                    Validator.Add(scalar);
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
                ScalarCollection != null?ScalarCollection.Where(scalar => !scalar.IsBlank && !scalar.HasError).ToList():new List<IDataListItemModel>();
            foreach (var scalar in filledScalars)
            {
                result.TryCreateScalarTemplate
                    (string.Empty, scalar.Name, scalar.Description
                     , true, scalar.IsEditable, scalar.ColumnIODirection, out errorString);
            }

            foreach (var recset in RecsetCollection ?? new OptomizedObservableCollection<IDataListItemModel>())
            {
                if (recset.IsBlank || recset.HasError) continue;
                IEnumerable<IDataListItemModel> filledRecordSets = recset.Children.Where(c => !c.IsBlank && !c.HasError);
                IList<Dev2Column> cols = filledRecordSets.Select(child =>
                                                                 DataListFactory.CreateDev2Column(
                                                                     child.Name, child.Description, child.IsEditable,
                                                                     child.ColumnIODirection))
                                                         .ToList();

                result.TryCreateRecordsetTemplate
                    (recset.Name, recset.Description, cols, true,
                     recset.IsEditable, recset.ColumnIODirection, out errorString);
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
            return (ScalarCollection != null && ScalarCollection.Count > 1) ||
                   (RecsetCollection != null && RecsetCollection.Count > 1);
        }
        #endregion Private Methods

        #region Override Methods

        protected override void OnDispose()
        {
            Resource = null;
            _compiler = null;
        }

        #endregion Override Methods

        #region Implementation of IHandle<ShowUnusedDataListVariablesMessage>

        public void Handle(ShowUnusedDataListVariablesMessage message)
        {
            if (message.ResourceModel == Resource)
            {
                if (message.ListOfUnused != null && message.ListOfUnused.Count != 0)
                {
                    SetUnusedDataListItems(message.ListOfUnused);
                }
                else
                {
                    // do we need to process ;)
                    if (ScalarCollection.Any(sc => sc.IsUsed == false) || RecsetCollection.Any(rc=>rc.IsUsed == false))
                    {
                        foreach (var dataListItemModel in ScalarCollection)
                        {
                            dataListItemModel.IsUsed = true;
                        }

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
            }
        }

        #endregion

        #region Implementation of IHandle<AddMissingDataListItems>

        public void Handle(AddMissingDataListItems message)
        {
            if (message.ResourceModel == Resource)
            {
                if (message.ListToAdd != null && message.ListToAdd.Count != 0)
                    AddMissingDataListItems(message.ListToAdd);
            }
        }

        #endregion
    }
}