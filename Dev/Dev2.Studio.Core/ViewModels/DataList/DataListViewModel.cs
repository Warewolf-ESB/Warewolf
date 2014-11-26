
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Caliburn.Micro;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Studio.Core.DataList;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Core.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Dev2.Studio.Core.ViewModels.DataList
{
    public class DataListViewModel : SimpleBaseViewModel, IDataListViewModel, IHandle<ShowUnusedDataListVariablesMessage>, IHandle<AddMissingDataListItems>
    {
        #region Fields

        private IResourceModel _resource;
        private RelayCommand _addRecordsetCommand;
        private RelayCommand _findUnusedAndMissingDataListItems;
        private RelayCommand _sortCommand;
        private IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        private OptomizedObservableCollection<IDataListItemModel> _scalarCollection;
        private OptomizedObservableCollection<IDataListItemModel> _recsetCollection;
        private OptomizedObservableCollection<DataListHeaderItemModel> _baseCollection;
        private string _searchText;

        #endregion Fields

        #region Properties

        public IResourceModel Resource
        {
            get
            {
                return _resource;
            }
            private set
            {
                _resource = value;
            }
        }

        public OptomizedObservableCollection<IDataListItemModel> DataList
        {
            get
            {
                return CreateFullDataList();
            }
        }

        public OptomizedObservableCollection<DataListHeaderItemModel> BaseCollection
        {
            get
            {
                return _baseCollection;
            }
            set
            {
                _baseCollection = value;
                OnPropertyChanged("BaseCollection");
            }
        }

        public OptomizedObservableCollection<IDataListItemModel> ScalarCollection
        {
            get
            {
                return _scalarCollection;
            }
            set
            {
                _scalarCollection = value;
                OnPropertyChanged("ScalarColletion");
            }
        }

        public OptomizedObservableCollection<IDataListItemModel> RecsetCollection
        {
            get
            {
                return _recsetCollection;
            }

            set
            {
                _recsetCollection = value;
                OnPropertyChanged("RecsetColletion");
            }
        }

        private IDataListValidator Validator { get; set; }

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                FilterItems();
                OnPropertyChanged("SearchText");
            }
        }

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
                if (_addRecordsetCommand == null)
                {
                    _addRecordsetCommand = new RelayCommand(method => AddRecordSet());
                }
                return _addRecordsetCommand;
            }
        }

        public ICommand SortCommand
        {
            get
            {
                if (_sortCommand == null)
                {
                    _sortCommand = new RelayCommand(method => SortItems());
                }
                return _sortCommand;
            }
        }

        public ICommand FindUnusedAndMissingCommand
        {
            get
            {
                if (_findUnusedAndMissingDataListItems == null)
                {
                    _findUnusedAndMissingDataListItems = new RelayCommand(method => FindUnusedAndMissing());
                }
                return _findUnusedAndMissingDataListItems;
            }
        }

        #endregion Commands

        #region Add/Remove Missing Methods

        public void AddMissingDataListItems(IList<IDataListVerifyPart> parts)
        {
            AddMissingDataListItems(parts, false);
        }

        public void AddMissingDataListItems(IList<IDataListVerifyPart> parts, bool async)
        {
            IList<IDataListItemModel> tmpRecsetList = new List<IDataListItemModel>();
            foreach (IDataListVerifyPart part in parts)
            {
                if (part.IsScalar)
                {
                    IDataListItemModel scalar = DataListItemModelFactory.CreateDataListModel(part.Field, part.Description, enDev2ColumnArgumentDirection.None);
                    ScalarCollection.Insert(ScalarCollection.Count - 1, scalar);
                    Validator.Add(scalar);
                }
                else
                {
                    IDataListItemModel recsetToAddTo = RecsetCollection.FirstOrDefault(c => c.Name == part.Recordset && c.IsRecordset);
                    IDataListItemModel tmpRecset = tmpRecsetList.FirstOrDefault(c => c.Name == part.Recordset);

                    if (recsetToAddTo != null)
                    {
                        IDataListItemModel child = DataListItemModelFactory.CreateDataListModel(part.Field, part.Description, recsetToAddTo);
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
                    else if (tmpRecset != null)
                    {
                        IDataListItemModel child = DataListItemModelFactory.CreateDataListModel(part.Field, part.Description, tmpRecset);
                        tmpRecset.Children.Add(child);
                        tmpRecset.Validator.Add(child);
                    }
                    else
                    {
                        IDataListItemModel recset = DataListItemModelFactory.CreateDataListModel(part.Recordset, part.Description, enDev2ColumnArgumentDirection.None);
                        tmpRecsetList.Add(recset);
                    }
                }
            }
            foreach (IDataListItemModel item in tmpRecsetList)
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
            Mediator.SendMessage(MediatorMessages.UpdateIntelisense, this);
            RemoveBlankScalars();
            RemoveBlankRecordsets();
            RemoveBlankRecordsetFields();
            AddBlankRow(null);
        }

        public void RemoveUnusedDataListItems(IList<IDataListVerifyPart> parts)
        {
            RemoveUnusedDataListItems(parts, false);
        }

        public void RemoveUnusedDataListItems(IList<IDataListVerifyPart> parts, bool async)
        {
            IList<IDataListItemModel> tmpRecsets = new List<IDataListItemModel>();
            foreach (IDataListVerifyPart part in parts)
            {
                if (part.IsScalar)
                {
                    IDataListItemModel scalarToRemove = ScalarCollection.FirstOrDefault(c => c.Name == part.Field);
                    if (scalarToRemove != null)
                    {
                        ScalarCollection.Remove(scalarToRemove);
                        Validator.Remove(scalarToRemove);
                    }
                }
                else
                {
                    IDataListItemModel recsetToRemove = RecsetCollection.FirstOrDefault(c => c.Name == part.Recordset && c.IsRecordset);
                    if (string.IsNullOrEmpty(part.Field))
                    {
                        if (recsetToRemove != null)
                        {
                            tmpRecsets.Add(recsetToRemove);
                        }
                    }
                    else
                    {
                        IDataListItemModel childToRemove = recsetToRemove.Children.FirstOrDefault(c => c.Name == part.Field && c.IsField);
                        if (childToRemove != null)
                        {
                            recsetToRemove.Children.Remove(childToRemove);
                            recsetToRemove.Validator.Remove(childToRemove);
                        }
                    }
                }
            }

            foreach (IDataListItemModel item in tmpRecsets)
            {
                RecsetCollection.Remove(item);
                Validator.Remove(item);
            }
            WriteToResourceModel();
            Mediator.SendMessage(MediatorMessages.UpdateIntelisense, this);
        }

        public IList<IDataListItemModel> CreateDataListItems(IList<IDataListVerifyPart> parts, bool isAdd)
        {
            IList<IDataListItemModel> results = new List<IDataListItemModel>();
            foreach (IDataListVerifyPart part in parts)
            {
                IDataListItemModel item;
                if (part.IsScalar)
                {
                    item = DataListItemModelFactory.CreateDataListItemViewModel(this, part.Field, part.Description, null);

                    results.Add(item);
                }
                else if (string.IsNullOrEmpty(part.Field))
                {
                    item = DataListItemModelFactory.CreateDataListItemViewModel(this, part.Recordset, part.Description, null, true);

                    results.Add(item);
                }
                else
                {
                    IDataListItemModel recset;

                    recset = results.FirstOrDefault(c => c.IsRecordset && c.Name == part.Recordset);

                    if (recset == null)
                    {
                        recset = DataList.FirstOrDefault(c => c.IsRecordset && c.Name == part.Recordset);
                    }

                    if (recset == null && isAdd)
                    {
                        item = DataListItemModelFactory.CreateDataListItemViewModel(this, part.Recordset, part.Description, null, true);

                        results.Add(item);
                    }

                    if (recset != null)
                    {
                        if (isAdd)
                        {
                            item = DataListItemModelFactory.CreateDataListItemViewModel(this, part.Field, part.Description, recset);

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

        public void SetUnusedDataListItems(IList<IDataListVerifyPart> parts)
        {
            foreach (IDataListItemModel dataListItemModel in ScalarCollection)
            {
                dataListItemModel.IsUsed = true;
            }

            foreach (IDataListItemModel dataListItemModel in RecsetCollection)
            {
                dataListItemModel.IsUsed = true;
                foreach (IDataListItemModel listItemModel in dataListItemModel.Children)
                {
                    listItemModel.IsUsed = true;
                }
            }

            IList<IDataListItemModel> tmpRecsets = new List<IDataListItemModel>();
            foreach (IDataListVerifyPart part in parts)
            {
                if (part.IsScalar)
                {
                    IDataListItemModel scalarToRemove = ScalarCollection.FirstOrDefault(c => c.Name == part.Field);
                    if (scalarToRemove != null)
                    {
                        scalarToRemove.IsUsed = false;
                    }
                }
                else
                {
                    IDataListItemModel recsetToRemove = RecsetCollection.FirstOrDefault(c => c.Name == part.Recordset && c.IsRecordset);
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
                            IDataListItemModel childToRemove = recsetToRemove.Children.FirstOrDefault(c => c.Name == part.Field && c.IsField);
                            if (childToRemove != null)
                            {
                                childToRemove.IsUsed = false;
                            }
                        }
                    }
                }
            }

            foreach (IDataListItemModel item in tmpRecsets)
            {
                RecsetCollection.Remove(item);
                Validator.Remove(item);
            }
            WriteToResourceModel();
            Mediator.SendMessage(MediatorMessages.UpdateIntelisense, this);
        }

        #endregion Add/Remove Missing Methods

        #region Methods

        public void InitializeDataListViewModel(IResourceModel resourceModel)
        {
            string errorString = string.Empty;
            _resource = resourceModel;
            if (_resource != null)
            {
                CreateListsOfIDataListItemModelToBindTo(out errorString);

                if (!string.IsNullOrEmpty(errorString))
                {
                    throw new Exception(errorString);
                }
                AddRecordsetNamesIfMissing();
            }
        }

        public void FilterItems()
        {
            if (SearchText != null)
            {
                foreach (IDataListItemModel item in ScalarCollection)
                {

                    if (item.Name.Contains(SearchText))
                    {
                        item.IsVisable = true;
                    }
                    else
                    {
                        item.IsVisable = false;
                    }

                }
                foreach (IDataListItemModel item in RecsetCollection)
                {
                    bool parentVis = false;
                    foreach (IDataListItemModel child in item.Children)
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
                    item.IsVisable = parentVis;
                    if (item.Name.Contains(SearchText))
                    {
                        item.IsVisable = true;
                    }
                }
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

        private void AddRowToScalars()
        {
                    IList<IDataListItemModel> blankList = ScalarCollection.Where(c => c.IsBlank).ToList();
                    if (blankList.Count == 0)
                    {
                        IDataListItemModel scalar = DataListItemModelFactory.CreateDataListModel(string.Empty);
                        ScalarCollection.Add(scalar);
                        Validator.Add(scalar);
                    }
                }

        private void AddRowToRecordsets()
                {
                    IList<IDataListItemModel> blankList = RecsetCollection.Where(c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();
                    if (blankList.Count == 0)
                    {
                        AddRecordSet();
                    }

                    foreach (IDataListItemModel recset in RecsetCollection)
                    {
                        IList<IDataListItemModel> blankChildList = recset.Children.Where(c => c.IsBlank).ToList();
                        if (blankChildList.Count == 0)
                        {
                            IDataListItemModel newChild = DataListItemModelFactory.CreateDataListModel(string.Empty);
                            newChild.Parent = recset;
                            recset.Children.Add(newChild);
                            recset.Validator.Add(newChild);
                        }
                    }
                }

        public void RemoveBlankRows(IDataListItemModel item)
        {
            if (item != null)
            {
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
        }

        public void RemoveBlankRecordsets()
        {
            IList<IDataListItemModel> blankList = RecsetCollection.Where(c => c.IsBlank && c.Children.Count == 1 && c.Children[0].IsBlank).ToList();
            if (blankList.Count > 1)
            {
                RecsetCollection.Remove(blankList.First());
                Validator.Remove(blankList.First());
            }
        }

        public void RemoveBlankScalars()
        {
            IList<IDataListItemModel> blankList = ScalarCollection.Where(c => c.IsBlank).ToList();
            if (blankList.Count > 1)
            {
                ScalarCollection.Remove(blankList.First());
                Validator.Remove(blankList.First());
            }
        }

        public void RemoveBlankRecordsetFields()
        {
            foreach (IDataListItemModel recset in RecsetCollection)
            {
                IList<IDataListItemModel> blankChildList = recset.Children.Where(c => c.IsBlank).ToList();

                if (blankChildList.Count > 1)
                {
                    recset.Children.Remove(blankChildList.First());
                    recset.Validator.Remove(blankChildList.First());
                }
            }
        }

        public void RemoveDataListItem(IDataListItemModel itemToRemove)
        {
            if (itemToRemove != null)
            {
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
                    foreach (IDataListItemModel recset in RecsetCollection)
                    {
                        recset.Children.Remove(itemToRemove);
                        recset.Validator.Remove(itemToRemove);
                    }
                }
            }
        }

        public string WriteToResourceModel()
        {
            string result = string.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            string errorString = string.Empty;
            AddRecordsetNamesIfMissing();
            IBinaryDataList postDl = ConvertIDataListItemModelsToIBinaryDataList(out errorString);
            if (string.IsNullOrEmpty(errorString))
            {
                result = CreateXmlDataFromBinaryDataList(postDl, out errors);
                _resource.DataList = result;
                // Mediator.SendMessage(MediatorMessages.UpdateIntelisense, this);
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
                item.DisplayName = item.DisplayName.Remove(item.DisplayName.IndexOf("("));
            }
            if (item.IsField && item.DisplayName.Contains("()."))
            {
                int startIndex = item.DisplayName.IndexOf(".") + 1;
                item.DisplayName = item.DisplayName.Substring(startIndex, item.DisplayName.Length - startIndex);
            }
        }

        public void AddRecordsetNamesIfMissing()
        {
            int recsetNum = RecsetCollection.Count;
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
                            int indexOfDot = child.DisplayName.IndexOf(".");
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
            if (item != null)
            {
                if (item.IsField)
                {
                    item.Parent.Validator.Move(item);
                }
                else
                {
                    Validator.Move(item);
                }
            }
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Creates the full data list.
        /// </summary>
        /// <returns></returns>
        private OptomizedObservableCollection<IDataListItemModel> CreateFullDataList()
        {
            OptomizedObservableCollection<IDataListItemModel> fullDataList = new OptomizedObservableCollection<IDataListItemModel>();
            foreach (IDataListItemModel item in ScalarCollection)
            {
                fullDataList.Add(item);
            }
            foreach (IDataListItemModel item in RecsetCollection)
            {
                fullDataList.Add(item);
            }

            return fullDataList;
        }

        /// <summary>
        /// Finds the unused and missing data list items.
        /// </summary>
        private void FindUnusedAndMissing()
        {
            Mediator.SendMessage(MediatorMessages.AddRemoveDataListItems, this);
        }

        /// <summary>
        /// Adds a record set.
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
        /// Sorts the items.
        /// </summary>
        private void SortItems()
        {
            SortScalars();
            SortRecset();
        }


        /// <summary>
        /// Sorts the scalars.
        /// </summary>
        private void SortScalars()
        {
            IList<IDataListItemModel> newScalarCollection = ScalarCollection.OrderBy(c => c.DisplayName).Where(c => !c.IsBlank).ToList();
            ScalarCollection.Clear();
            foreach (IDataListItemModel item in newScalarCollection)
            {
                ScalarCollection.Add(item);
            }
            ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel(string.Empty));
        }

        /// <summary>
        /// Sorts the recordsets.
        /// </summary>
        private void SortRecset()
        {
            IList<IDataListItemModel> newRecsetCollection = RecsetCollection.OrderBy(c => c.DisplayName).ToList();
            RecsetCollection.Clear();
            foreach (IDataListItemModel item in newRecsetCollection.Where(c => !c.IsBlank))
            {
                RecsetCollection.Add(item);
            }
            AddRecordSet();
        }


        /// <summary>
        /// Creates the list of data list item view model to bind to.
        /// </summary>
        /// <param name="_resource">The resource model.</param>
        /// <param name="errorString">The error string.</param>
        /// <returns></returns>
        private void CreateListsOfIDataListItemModelToBindTo(out string errorString)
        {
            ErrorResultTO errors = new ErrorResultTO();
            errorString = string.Empty;
            if (!string.IsNullOrEmpty(_resource.DataList))
            {
                IBinaryDataList binarnyDL = CreateBinaryDataListFromXmlData(_resource.DataList, out errors);
                if (!errors.HasErrors())
                {
                    ConvertBinaryDataListToListOfIDataListItemModels(binarnyDL, out errorString);
                }
                else
                {
                    string errorMessage = string.Empty;
                    foreach (string error in errors.FetchErrors())
                    {
                        errorMessage += error;
                    }
                    throw new Exception(errorMessage);
                }
                _compiler.ForceDeleteDataListByID(binarnyDL.UID);
            }
            else
            {

                RecsetCollection = new OptomizedObservableCollection<IDataListItemModel>();
                AddRecordSet();
                ScalarCollection = new OptomizedObservableCollection<IDataListItemModel>();
                ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel(string.Empty));
            }
            BaseCollection = new OptomizedObservableCollection<DataListHeaderItemModel>();
            DataListHeaderItemModel VarNode = DataListItemModelFactory.CreateDataListHeaderItem("Variables");
            if (ScalarCollection.Count == 0)
            {
                ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel(string.Empty));
            }
            VarNode.Children = ScalarCollection;
            BaseCollection.Add(VarNode);
            DataListHeaderItemModel RecordsetsNode = DataListItemModelFactory.CreateDataListHeaderItem("Recordsets");
            if (RecsetCollection.Count == 0)
            {
                AddRecordSet();
            }
            RecordsetsNode.Children = RecsetCollection;
            BaseCollection.Add(RecordsetsNode);

        }

        /// <summary>
        /// Creates a binary data list from XML data.
        /// </summary>
        /// <param name="xmlDataList">The XML data list.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private IBinaryDataList CreateBinaryDataListFromXmlData(string xmlDataList, out ErrorResultTO errors)
        {
            IBinaryDataList result = null;
            ErrorResultTO allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();
            Guid dlGuid = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), xmlDataList, xmlDataList, out errors);
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
        /// Creates the XML data from binary data list.
        /// </summary>
        /// <param name="binaryDataList">The binary data list.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private string CreateXmlDataFromBinaryDataList(IBinaryDataList binaryDataList, out ErrorResultTO errors)
        {
            string result = string.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();
            Guid dlGuid = _compiler.PushBinaryDataList(binaryDataList.UID, binaryDataList, out errors);
            result = _compiler.ConvertFrom(dlGuid, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Shape, out errors);
            return result;
        }

        /// <summary>
        /// Converts a binary data list to list a data list item view models.
        /// </summary>
        /// <param name="dataListToConvert">The data list to convert.</param>
        /// <param name="errorString">The error string.</param>
        /// <returns></returns>
        private void ConvertBinaryDataListToListOfIDataListItemModels(IBinaryDataList dataListToConvert, out string errorString)
        {
            errorString = string.Empty;
            RecsetCollection = new OptomizedObservableCollection<IDataListItemModel>();
            ScalarCollection = new OptomizedObservableCollection<IDataListItemModel>();

            IList<IBinaryDataListEntry> listOfEntries = dataListToConvert.FetchAllEntries();

            foreach (IBinaryDataListEntry entry in listOfEntries)
            {
                if (entry.IsRecordset)
                {
                    IDataListItemModel recset = DataListItemModelFactory.CreateDataListModel(entry.Namespace, entry.Description, entry.ColumnIODirection);
                    recset.IsEditable = entry.IsEditable;
                    Validator.Add(recset);
                    foreach (Dev2Column col in entry.Columns)
                    {
                        IDataListItemModel child = DataListItemModelFactory.CreateDataListModel(col.ColumnName, col.ColumnDescription, col.ColumnIODirection);
                        child.Parent = recset;
                        child.IsEditable = col.IsEditable;
                        recset.Children.Add(child);
                        recset.Validator.Add(child);
                    }

                    RecsetCollection.Add(recset);
                }
                else
                {
                    IDataListItemModel scalar = DataListItemModelFactory.CreateDataListModel(entry.Namespace, entry.Description, entry.ColumnIODirection);
                    scalar.IsEditable = entry.IsEditable;
                    ScalarCollection.Add(scalar);
                    Validator.Add(scalar);
                }
            }
        }

        /// <summary>
        /// Converts a list of data list item view models to a binary data list.
        /// </summary>
        /// <param name="listToConvert">The list to convert.</param>
        /// <param name="errorString">The error string.</param>
        /// <returns></returns>
        private IBinaryDataList ConvertIDataListItemModelsToIBinaryDataList(out string errorString)
        {
            errorString = string.Empty;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();

            foreach (IDataListItemModel scalar in ScalarCollection)
            {
                if (!scalar.IsBlank && !scalar.HasError)
                {
                    result.TryCreateScalarTemplate(string.Empty, scalar.Name, scalar.Description, true, scalar.IsEditable, scalar.ColumnIODirection, out errorString);
                }
            }

            foreach (IDataListItemModel recset in RecsetCollection)
            {
                if (!recset.IsBlank && !recset.HasError)
                {
                    IList<Dev2Column> cols = new List<Dev2Column>();
                    foreach (IDataListItemModel child in recset.Children.Where(c => !c.IsBlank && !c.HasError))
                    {
                        cols.Add(DataListFactory.CreateDev2Column(child.Name, child.Description, child.IsEditable, child.ColumnIODirection));
                    }

                    result.TryCreateRecordsetTemplate(recset.Name, recset.Description, cols, true, recset.IsEditable, recset.ColumnIODirection, out errorString);
                }
            }
            return result;
        }

        #endregion Private Methods

        #region Override Methods

        protected override void OnDispose()
        {
            _resource = null;
            _compiler = null;
        }

        #endregion Override Methods

        #region Implementation of IHandle<ShowUnusedDataListVariablesMessage>

        public void Handle(ShowUnusedDataListVariablesMessage message)
        {
            if (message.ResourceModel == Resource)
            {
                SetUnusedDataListItems(message.ListOfUnused);
            }
        }

        #endregion

        #region Implementation of IHandle<AddMissingDataListItems>

        public void Handle(AddMissingDataListItems message)
        {
            if (message.ResourceModel == Resource)
            {
                AddMissingDataListItems(message.ListToAdd);
            }
        }

        #endregion
    }
}
