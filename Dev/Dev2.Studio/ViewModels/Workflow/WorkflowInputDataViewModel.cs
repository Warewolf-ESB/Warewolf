/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Instrumentation;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Dev2.Studio.ViewModels.Workflow
{
    public class WorkflowInputDataViewModel : SimpleBaseViewModel
    {
        #region Fields
        OptomizedObservableCollection<IDataListItem> _workflowInputs;
        RelayCommand _executeCommmand;
        string _xmlData;
        readonly IContextualResourceModel _resourceModel;
        bool _rememberInputs;
        RelayCommand _viewInBrowserCommmand;
        static DataListConversionUtils _dataListConversionUtils = new DataListConversionUtils();
        bool _canViewInBrowser;
        bool _canDebug;
        readonly IPopupController _popupController;
        RelayCommand _cancelCommand;

        #endregion Fields

        public event Action DebugExecutionStart;
        public event Action DebugExecutionFinished;

        void OnDebugExecutionFinished()
        {
            var handler = DebugExecutionFinished;
            handler?.Invoke();
        }

        void OnDebugExecutionStart()
        {
            Tracker.TrackEvent(TrackerEventGroup.Workflows, TrackerEventName.DebugClicked);
            var handler = DebugExecutionStart;
            handler?.Invoke();
        }

        public WorkflowInputDataViewModel(IServiceDebugInfoModel input, Guid sessionId)
        {
            VerifyArgument.IsNotNull(@"input", input);
            CanDebug = true;
            CanViewInBrowser = true;

            DebugTo = new DebugTO
            {
                DataList = !string.IsNullOrEmpty(input.ResourceModel.DataList)
                               ? input.ResourceModel.DataList
                               : @"<DataList></DataList>",
                ServiceName = input.ResourceModel.ResourceName,
                WorkflowID = input.ResourceModel.ResourceName,
                WorkflowXaml = string.Empty,
                XmlData = input.ServiceInputData,
                ResourceID = input.ResourceModel.ID,
                ServerID = input.ResourceModel.ServerID,
                RememberInputs = input.RememberInputs,
                SessionID = sessionId
            };

            if (input.DebugModeSetting == DebugMode.DebugInteractive)
            {
                DebugTo.IsDebugMode = true;
            }

            _resourceModel = input.ResourceModel;
            
            DisplayName = @"Debug input data";
            
            _popupController = CustomContainer.Get<IPopupController>();
        }

        IDev2StudioSessionBroker Broker { get; set; }

        IDataListModel DataList { get; set; }
        
        public DebugTO DebugTo { get; private set; }
        
        public OptomizedObservableCollection<IDataListItem> WorkflowInputs
        {
            get
            {
                if (_workflowInputs == null)
                {
                    _workflowInputs = new OptomizedObservableCollection<IDataListItem>();
                    _workflowInputs.CollectionChanged += (o, args) => NotifyOfPropertyChange(() => WorkflowInputCount);
                }
                return _workflowInputs;
            }
        }

        public void ViewClosed()
        {
            if (!CloseRequested)
            {
                SendFinishedMessage();
            }
        }
        
        public int WorkflowInputCount => _workflowInputs.Count;
        
        public bool RememberInputs
        {
            get
            {
                return _rememberInputs;
            }
            set
            {
                _rememberInputs = value;
                OnPropertyChanged(@"RememberInputs");
            }
        }
        
        public string XmlData
        {
            get
            {
                return _xmlData;
            }
            set
            {
                _xmlData = value;
                OnPropertyChanged(@"XmlData");
            }
        }

        bool CanViewInBrowser
        {
            get
            {
                return _canViewInBrowser;
            }
            set
            {
                if (value.Equals(_canViewInBrowser))
                {
                    return;
                }
                _canViewInBrowser = value;
                NotifyOfPropertyChange(() => CanViewInBrowser);
                ViewInBrowserCommand.RaiseCanExecuteChanged();
            }
        }

        bool CanDebug
        {
            get
            {
                return _canDebug;
            }
            set
            {
                if (value.Equals(_canDebug))
                {
                    return;
                }
                _canDebug = value;
                NotifyOfPropertyChange(() => CanDebug);
                OkCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand OkCommand
        {
            get
            {
                return _executeCommmand ?? (_executeCommmand = new RelayCommand(param => Save(), param => CanDebug));
            }
        }

        public RelayCommand ViewInBrowserCommand
        {
            get
            {
                return _viewInBrowserCommmand ?? (_viewInBrowserCommmand = new RelayCommand(param => ViewInBrowser(), param => CanViewInBrowser));
            }
        }


        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(param => Cancel()));
            }
        }

        public bool IsInError { private get; set; }
        
        public void Save()
        {
            if (!IsInError)
            {
                DoSaveActions();
                ExecuteWorkflow();
                RequestClose();
            }
            else
            {
                ShowInvalidDataPopupMessage();
            }
        }

        public void DoSaveActions()
        {
            SetXmlData();
            DebugTo.XmlData = XmlData;
            DebugTo.RememberInputs = RememberInputs;
            if (DebugTo.DataList != null)
            {
                Broker.PersistDebugSession(DebugTo);
            }
        }

        public void ExecuteWorkflow()
        {
            if (_resourceModel?.Environment == null)
            {
                return;
            }

            var context = Parent as WorkSurfaceContextViewModel;
            context?.BindToModel();

            var clientContext = _resourceModel.Environment.Connection;
            if (clientContext == null)
            {
                return;
            }

            var dataList = XElement.Parse(DebugTo.XmlData);
            dataList.Add(new XElement(@"BDSDebugMode", DebugTo.IsDebugMode));
            dataList.Add(new XElement(@"DebugSessionID", DebugTo.SessionID));
            dataList.Add(new XElement(@"EnvironmentID", _resourceModel.Environment.EnvironmentID));
            OnDebugExecutionStart();
            SendExecuteRequest(dataList);
        }

        protected virtual void SendExecuteRequest(XElement payload)
        {
            WebServer.Send(_resourceModel, payload.ToString(), new AsyncWorker());
        }

        public void ShowInvalidDataPopupMessage()
        {
            _popupController.Show(StringResources.DataInput_Error,
                                  StringResources.DataInput_Error_Title,
                                  MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false);

            IsInError = true;
        }

        public void ViewInBrowser()
        {
            if (!IsInError)
            {
                Tracker.TrackEvent(TrackerEventGroup.Workflows, TrackerEventName.ViewInBrowserClicked);
                DoSaveActions();
                var payload = BuildWebPayLoad();
                SendViewInBrowserRequest(payload);
                SendFinishedMessage();
                RequestClose();
            }
            else
            {
                ShowInvalidDataPopupMessage();
            }
        }

        public string BuildWebPayLoad()
        {
            var allScalars = WorkflowInputs.All(item => !item.CanHaveMutipleRows && !item.IsObject);
            if (allScalars && WorkflowInputs.Count > 0)
            {
                return WorkflowInputs.Aggregate(string.Empty, (current, workflowInput) => current + workflowInput.Field + @"=" + workflowInput.Value + @"&").TrimEnd('&');
            }
            try
            {
                return XElement.Parse(XmlData).ToString(SaveOptions.DisableFormatting);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        protected virtual void SendViewInBrowserRequest(string payload)
        {
            WebServer.OpenInBrowser(_resourceModel, payload);
        }

        internal void SendFinishedMessage()
        {
            OnDebugExecutionFinished();
        }
        
        public void Cancel()
        {
            SetXmlData();
            DebugTo.XmlData = XmlData;
            DebugTo.RememberInputs = RememberInputs;
            if (DebugTo.DataList != null)
            {
                Broker.PersistDebugSession(DebugTo);
            }

            SendFinishedMessage();
            RequestClose(ViewModelDialogResults.Cancel);
        }
        
        public void LoadWorkflowInputs()
        {
            WorkflowInputs.Clear();
            Broker?.Dispose();
            Broker = Dev2StudioSessionFactory.CreateBroker();
            DebugTo?.CleanUp();
            DebugTo = Broker.InitDebugSession(DebugTo);
            XmlData = DebugTo.XmlData;
            RememberInputs = DebugTo.RememberInputs;
            DataList = DebugTo.BinaryDataList;

            var myList = _dataListConversionUtils.CreateListToBindTo(DebugTo.BinaryDataList);

            WorkflowInputs.AddRange(myList);
        }

        public void AddRow(IDataListItem itemToAdd)
        {
            if (itemToAdd != null && itemToAdd.CanHaveMutipleRows)
            {
                var recordset = DataList.ShapeRecordSets.FirstOrDefault(set => set.Name == itemToAdd.Recordset);
                if (recordset != null)
                {
                    var recsetCols = new List<IScalar>();
                    foreach (var column in recordset.Columns)
                    {
                        var cols = column.Value.Where(scalar => scalar.IODirection == enDev2ColumnArgumentDirection.Input || scalar.IODirection == enDev2ColumnArgumentDirection.Both);
                        recsetCols.AddRange(cols);
                    }

                    var numberOfRows = WorkflowInputs.Where(c => c.Recordset == itemToAdd.Recordset);
                    IEnumerable<IDataListItem> dataListItems = numberOfRows as IDataListItem[] ?? numberOfRows.ToArray();
                    var lastItem = dataListItems.Last();
                    var indexToInsertAt = WorkflowInputs.IndexOf(lastItem);
                    var indexString = lastItem.Index;
                    var indexNum = Convert.ToInt32(indexString) + 1;
                    var lastRow = dataListItems.Where(c => c.Index == indexString);
                    var addRow = false;
                    foreach (var item in lastRow)
                    {
                        if (item.Value != string.Empty)
                        {
                            addRow = true;
                        }
                    }
                    if (addRow)
                    {
                        AddBlankRowToRecordset(itemToAdd, recsetCols, indexToInsertAt, indexNum);
                    }
                }
            }
        }
        
        public bool RemoveRow(IDataListItem itemToRemove, out int indexToSelect)
        {
            indexToSelect = 1;
            var itemsRemoved = false;
            if (itemToRemove != null && itemToRemove.CanHaveMutipleRows)
            {
                var numberOfRows = WorkflowInputs.Count(c => c.Recordset == itemToRemove.Recordset && c.Field == itemToRemove.Field);
                var listToRemove = WorkflowInputs.Where(c => c.Index == numberOfRows.ToString(CultureInfo.InvariantCulture) && c.Recordset == itemToRemove.Recordset).ToList();

                if (numberOfRows == 2)
                {
                    var firstRow = WorkflowInputs.Where(c => c.Index == @"1" && c.Recordset == itemToRemove.Recordset);
                    var removeRow = firstRow.All(item => string.IsNullOrWhiteSpace(item.Value));

                    if (removeRow)
                    {
                        var listToChange = WorkflowInputs.Where(c => c.Index == @"2" && c.Recordset == itemToRemove.Recordset);

                        foreach (IDataListItem item in listToChange)
                        {
                            item.Value = string.Empty;
                        }
                        foreach (IDataListItem item in listToRemove)
                        {
                            WorkflowInputs.Remove(item);
                            indexToSelect = UpdateIndexToSelect(itemToRemove, item);
                            itemsRemoved = true;
                        }
                    }
                }
                else
                {
                    if (numberOfRows > 2)
                    {
                        var listToChange = WorkflowInputs.Where(c => c.Index == (numberOfRows - 1).ToString(CultureInfo.InvariantCulture) && c.Recordset == itemToRemove.Recordset);
                        foreach (IDataListItem item in listToChange)
                        {
                            item.Value = string.Empty;
                        }
                        foreach (IDataListItem item in listToRemove)
                        {
                            WorkflowInputs.Remove(item);
                            indexToSelect = UpdateIndexToSelect(itemToRemove, item);
                            itemsRemoved = true;
                        }
                    }
                }
            }
            return itemsRemoved;
        }

        int UpdateIndexToSelect(IDataListItem itemToRemove, IDataListItem item)
        {
            if (itemToRemove.Index == item.Index)
            {
                var item1 = item;
                return WorkflowInputs.IndexOf(WorkflowInputs.Last(c => c.Recordset == item1.Recordset));
            }
            return WorkflowInputs.IndexOf(itemToRemove);
        }

        public void SetXmlData() => SetXmlData(false);
        public void SetXmlData(bool includeBlank)
        {
            var dataListObject = new JObject();
            var objects = WorkflowInputs.Where(item => item.IsObject);
            var recSets = WorkflowInputs.Where(item => item.CanHaveMutipleRows && !item.IsObject);
            var scalars = WorkflowInputs.Where(item => !item.CanHaveMutipleRows && !item.IsObject);
            AddScalarsToObject(scalars, dataListObject);
            AddRecordsetsToObject(recSets, dataListObject,includeBlank);
            AddObjectsToObject(objects, dataListObject);

            var dataListString = dataListObject.ToString(Formatting.Indented);
            JsonData = dataListString;
            var xml = JsonConvert.DeserializeXNode(dataListString, @"DataList", true);
            try
            {
                if (xml.Descendants().Count() == 1)
                {
                    xml = XDocument.Parse(@"<DataList></DataList>");
                }
                XmlData = XElement.Parse(xml.ToString()).ToString();
            }
            catch (Exception e)
            {
                XmlData = @"Invalid characters entered";
                Dev2Logger.Error(e.StackTrace, e, "Warewolf Error");
            }
        }

        public string JsonData { get; private set; }

        void AddObjectsToObject(IEnumerable<IDataListItem> objects, JObject dataListObject)
        {
            foreach (var o in objects)
            {
                var json = string.Empty;
                if (DataListSingleton.ActiveDataList != null)
                {
                    if (DataListSingleton.ActiveDataList.ComplexObjectCollection != null)
                    {
                        var complexObjectItemModel = DataListSingleton.ActiveDataList.ComplexObjectCollection.SingleOrDefault(model => model.Name == o.DisplayValue);

                        if (complexObjectItemModel != null)
                        {
                            json = complexObjectItemModel.GetJson();
                        }
                    }
                    AddToDataListObject(dataListObject, o, json);
                }
            }
        }

        void AddToDataListObject(JObject dataListObject, IDataListItem o, string json)
        {
            try
            {
                var objValue = string.IsNullOrEmpty(o.Value) ? json : o.Value;
                var value = JsonConvert.DeserializeObject(objValue) as JObject;
                var prop = value?.Properties().FirstOrDefault(property => property.Name == o.Field);
                if (prop != null)
                {
                    value = prop.Value as JObject;
                }
                dataListObject.Add(o.Field, value);
            }
            catch (Exception)
            {
                ShowInvalidDataPopupMessage();
            }
        }

        static void AddRecordsetsToObject(IEnumerable<IDataListItem> recSets, JObject dataListObject, bool includeBlank = false)
        {
            var groupedRecsets = recSets.GroupBy(item => item.Recordset);
            foreach (var groupedRecset in groupedRecsets)
            {
                var arrayName = groupedRecset.Key;
                var newArray = new JArray();
                var dataListItems = groupedRecset.GroupBy(item => item.Index);
                foreach (var dataListItem in dataListItems)
                {
                    var jObjForArray = new JObject();
                    var empty = true;
                    foreach (var listItem in dataListItem)
                    {
                        if (!string.IsNullOrEmpty(listItem.Value))
                        {
                            empty = false;
                        }
                        jObjForArray.Add(new JProperty(listItem.Field, listItem.Value ?? string.Empty));
                    }
                    if (!empty || includeBlank)
                    {
                        newArray.Add(jObjForArray);
                    }
                }
                dataListObject.Add(arrayName, newArray);
            }
        }

        static void AddScalarsToObject(IEnumerable<IDataListItem> scalars, JObject dataListObject)
        {
            foreach (var dataListItem in scalars)
            {
                dataListObject.Add(dataListItem.DisplayValue, new JValue(dataListItem.Value ?? string.Empty));
            }
        }

        public void SetWorkflowInputData()
        {
            WorkflowInputs.Clear();
            DataList.PopulateWithData(XmlData);
            _dataListConversionUtils.CreateListToBindTo(DataList).ToList().ForEach(i => WorkflowInputs.Add(i));
        }
        
        public bool AddBlankRow(IDataListItem selectedItem, out int indexToSelect)
        {
            indexToSelect = 1;
            var itemsAdded = false;
            if (selectedItem != null && selectedItem.CanHaveMutipleRows)
            {
                var recordset = DataList.RecordSets.FirstOrDefault(set => set.Name == selectedItem.Recordset);
                if (recordset != null)
                {
                    var recsetCols = new List<IScalar>();
                    foreach (var column in recordset.Columns)
                    {
                        recsetCols.AddRange(column.Value);
                    }
                    var numberOfRows = WorkflowInputs.Where(c => c.Recordset == selectedItem.Recordset);
                    var lastItem = numberOfRows.Last();
                    var indexToInsertAt = WorkflowInputs.IndexOf(lastItem);
                    var indexString = lastItem.Index;
                    var indexNum = Convert.ToInt32(indexString) + 1;
                    indexToSelect = indexToInsertAt + 1;
                    itemsAdded = AddBlankRowToRecordset(selectedItem, recsetCols, indexToInsertAt, indexNum);
                }
            }
            return itemsAdded;
        }

        public IDataListItem GetNextRow(IDataListItem selectedItem)
        {
            if (selectedItem != null)
            {
                var indexToInsertAt = WorkflowInputs.IndexOf(selectedItem);
                if (indexToInsertAt != -1)
                {
                    var indexOfItemToGet = indexToInsertAt + 1;
                    if (indexOfItemToGet == WorkflowInputs.Count)
                    {
                        return selectedItem;
                    }
                    return WorkflowInputs[indexOfItemToGet];
                }
            }
            return null;
        }

        public IDataListItem GetPreviousRow(IDataListItem selectedItem)
        {
            if (selectedItem != null)
            {
                var indexToInsertAt = WorkflowInputs.IndexOf(selectedItem);
                if (indexToInsertAt != -1)
                {
                    var indexOfItemToGet = indexToInsertAt - 1;
                    if (indexOfItemToGet == WorkflowInputs.Count || indexOfItemToGet == -1)
                    {
                        return selectedItem;
                    }
                    return WorkflowInputs[indexOfItemToGet];
                }
            }
            return null;
        }
        
        protected override void OnDispose()
        {
        }

        bool AddBlankRowToRecordset(IDataListItem dlItem, IList<IScalar> columns, int indexToInsertAt, int indexNum)
        {
            var itemsAdded = false;
            if (dlItem.CanHaveMutipleRows)
            {
                IList<IScalar> recsetCols = columns.Distinct(Scalar.Comparer).ToList();
                string colName = null;
                foreach (var col in recsetCols.Distinct(new ScalarNameComparer()))
                {
                    if (string.IsNullOrEmpty(colName) || !colName.Equals(col.Name))
                    {
                        WorkflowInputs.Insert(indexToInsertAt + 1, new DataListItem
                        {
                            DisplayValue = string.Concat(dlItem.Recordset, @"(", indexNum, @").", col.Name),
                            Value = string.Empty,
                            CanHaveMutipleRows = dlItem.CanHaveMutipleRows,
                            Recordset = dlItem.Recordset,
                            Field = col.Name,
                            Description = col.Description,
                            Index = indexNum.ToString(CultureInfo.InvariantCulture)
                        });
                        indexToInsertAt++;
                    }
                    colName = col.Name;
                    itemsAdded = true;

                }
            }
            return itemsAdded;
        }

        protected override void OnViewAttached(object view, object context)
        {
            LoadWorkflowInputs();
            base.OnViewAttached(view, context);
        }

        public static WorkflowInputDataViewModel Create(IContextualResourceModel resourceModel)
        {
            return Create(resourceModel, Guid.Empty, DebugMode.Run);
        }

        public static WorkflowInputDataViewModel Create(IContextualResourceModel resourceModel, Guid sessionId, DebugMode debugMode)
        {
            VerifyArgument.IsNotNull(@"resourceModel", resourceModel);
            var debugInfoModel = ServiceDebugInfoModelFactory.CreateServiceDebugInfoModel(resourceModel, string.Empty, debugMode);

            var result = new WorkflowInputDataViewModel(debugInfoModel, sessionId);
            if (resourceModel?.Environment?.AuthorizationService != null)
            {
                result.CanDebug = resourceModel.UserPermissions.CanDebug();
            }

            return result;
        }
    }
}
