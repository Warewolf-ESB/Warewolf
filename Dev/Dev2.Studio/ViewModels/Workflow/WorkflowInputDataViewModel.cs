using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Input;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.Enums;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Services.Security;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.ViewModels.Workflow;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Workflow
{
    public class WorkflowInputDataViewModel : SimpleBaseViewModel
    {
        #region Fields
        //2012.10.11: massimo.guerrera - Added for PBI 5781
        private OptomizedObservableCollection<IDataListItem> _workflowInputs;
        private RelayCommand _executeCommmand;
        private RelayCommand _cancelComand;
        private string _xmlData;
        private readonly IContextualResourceModel _resourceModel;
        private bool _rememberInputs;
        RelayCommand _viewInBrowserCommmand;
        readonly DataListConversionUtils _dataListConversionUtils;
        bool _canViewInBrowser;
        bool _canDebug;

        #endregion Fields

        public event Action DebugExecutionStart;
        public event Action DebugExecutionFinished;

        void OnDebugExecutionFinished()
        {
            var handler = DebugExecutionFinished;
            if(handler != null)
            {
                handler();
            }
        }

        void OnDebugExecutionStart()
        {
            var handler = DebugExecutionStart;
            if(handler != null)
            {
                handler();
            }
        }

        #region Ctor

        public WorkflowInputDataViewModel(IServiceDebugInfoModel input, Guid sessionID)
        {
            VerifyArgument.IsNotNull("input", input);
            CanDebug = true;
            CanViewInBrowser = true;

            DebugTO = new DebugTO
            {
                DataList = !string.IsNullOrEmpty(input.ResourceModel.DataList)
                               ? input.ResourceModel.DataList
                               : "<DataList></DataList>",
                ServiceName = input.ResourceModel.ResourceName,
                WorkflowID = input.ResourceModel.ResourceName,
                WorkflowXaml = string.Empty,
                //WorkflowXaml = input.ResourceModel.WorkflowXaml, - 05.12.2013 ;)
                XmlData = input.ServiceInputData,
                ResourceID = input.ResourceModel.ID,
                ServerID = input.ResourceModel.ServerID,
                RememberInputs = input.RememberInputs,
                SessionID = sessionID
            };

            if(input.DebugModeSetting == DebugMode.DebugInteractive)
            {
                DebugTO.IsDebugMode = true;
            }

            _resourceModel = input.ResourceModel;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            DisplayName = "Debug input data";
            _dataListConversionUtils = new DataListConversionUtils();
        }
        #endregion Ctor

        #region Properties
        /// <summary>
        /// Broker that contain all session data and methods
        /// </summary>
        private IDev2StudioSessionBroker Broker { get; set; }

        /// <summary>
        /// Binary backing object for the data list
        /// </summary>
        IBinaryDataList DataList { get; set; }

        /// <summary>
        /// The transfer object that holds all the information needed for a debug session
        /// </summary>
        public DebugTO DebugTO { get; set; }

        /// <summary>
        /// Collection of IDataListItems that contain all the inputs
        /// </summary>
        public OptomizedObservableCollection<IDataListItem> WorkflowInputs
        {
            get
            {
                if(_workflowInputs == null)
                {
                    _workflowInputs = new OptomizedObservableCollection<IDataListItem>();
                    _workflowInputs.CollectionChanged += (o, args) => NotifyOfPropertyChange(() => WorkflowInputCount);
                }
                return _workflowInputs;
            }
        }

        /// <summary>
        /// Int that contains the count of variables marked as inputs
        /// </summary>
        public int WorkflowInputCount
        {
            get
            {
                return _workflowInputs.Count;
            }
        }

        /// <summary>
        /// Boolean that contains the option for remembering the inputs for that workflow
        /// </summary>
        public bool RememberInputs
        {
            get
            {
                return _rememberInputs;
            }
            set
            {
                _rememberInputs = value;
                OnPropertyChanged("RememberInputs");
            }
        }

        /// <summary>
        /// String that hold the XML representation of the inputs
        /// </summary>
        public string XmlData
        {
            get
            {
                return _xmlData;
            }
            set
            {
                _xmlData = value;
                OnPropertyChanged("XmlData");
            }
        }
        #endregion Properties

        #region Commands

        public bool CanViewInBrowser
        {
            get
            {
                return _canViewInBrowser;
            }
            set
            {
                if(value.Equals(_canViewInBrowser))
                {
                    return;
                }
                _canViewInBrowser = value;
                NotifyOfPropertyChange(() => CanViewInBrowser);
            }
        }

        public bool CanDebug
        {
            get
            {
                return _canDebug;
            }
            set
            {
                if(value.Equals(_canDebug))
                {
                    return;
                }
                _canDebug = value;
                NotifyOfPropertyChange(() => CanDebug);
            }
        }

        public ICommand OkCommand
        {
            get
            {
                return _executeCommmand ?? (_executeCommmand = new RelayCommand(param => Save(), param => CanDebug));
            }
        }

        public ICommand ViewInBrowserCommand
        {
            get
            {
                return _viewInBrowserCommmand ?? (_viewInBrowserCommmand = new RelayCommand(param => ViewInBrowser(), param => CanViewInBrowser));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return _cancelComand ?? (_cancelComand = new RelayCommand(param => Cancel(), param => true));
            }
        }
        #endregion Cammands

        #region Methods

        /// <summary>
        /// Used for saving the data input by the user to the file system and pushing the data back at the workflow
        /// </summary>
        public void Save()
        {
            //2012.10.11: massimo.guerrera - Added for PBI 5781
            DoSaveActions();
            ExecuteWorkflow();
            RequestClose();
        }

        void DoSaveActions()
        {
            SetXmlData();
            DebugTO.XmlData = XmlData;
            DebugTO.RememberInputs = RememberInputs;
            if(DebugTO.DataList != null)
            {
                Broker.PersistDebugSession(DebugTO);
            }
        }

        public void ExecuteWorkflow()
        {
            if(_resourceModel == null || _resourceModel.Environment == null)
            {
                return;
            }

            var context = Parent as WorkSurfaceContextViewModel;
            if(context != null)
            {
                context.BindToModel();
            }

            var clientContext = _resourceModel.Environment.Connection;
            if(clientContext != null)
            {
                var dataList = XElement.Parse(DebugTO.XmlData);
                dataList.Add(new XElement("BDSDebugMode", DebugTO.IsDebugMode));
                dataList.Add(new XElement("DebugSessionID", DebugTO.SessionID));
                dataList.Add(new XElement("EnvironmentID", _resourceModel.Environment.ID));
                OnDebugExecutionStart();
                SendExecuteRequest(dataList);
            }
        }

        protected virtual void SendExecuteRequest(XElement payload)
        {
            WebServer.SendAsync(WebServerMethod.POST, _resourceModel, payload.ToString(), ExecutionCallback);
        }

        public void ViewInBrowser()
        {
            DoSaveActions();
            var isXml = false;
            string payload;
            var allScalars = WorkflowInputs.All(item => !item.IsRecordset);
            if(allScalars && WorkflowInputs.Count > 0)
            {
                payload = WorkflowInputs.Aggregate("", (current, workflowInput) => current + (workflowInput.Field + "=" + workflowInput.Value + "&")).TrimEnd('&');
            }
            else
            {
                payload = XElement.Parse(XmlData).ToString(SaveOptions.DisableFormatting);
                isXml = true;
            }
            SendViewInBrowserRequest(payload, isXml);
            SendFinishedMessage();
            RequestClose();
        }

        protected virtual void SendViewInBrowserRequest(string payload, bool isXml)
        {
            WebServer.OpenInBrowser(WebServerMethod.POST, _resourceModel, payload, isXml);
        }

        private void ExecutionCallback(UploadStringCompletedEventArgs args)
        {
            //dont do anything 
        }

        private void SendFinishedMessage()
        {
            OnDebugExecutionFinished();
        }

        /// <summary>
        /// Used for saving the data input by the user to the file system when the cacel button is clicked
        /// </summary>
        public void Cancel()
        {
            //2012.10.11: massimo.guerrera - Added for PBI 5781
            SetXmlData();
            DebugTO.XmlData = XmlData;
            DebugTO.RememberInputs = RememberInputs;
            if(DebugTO.DataList != null) Broker.PersistDebugSession(DebugTO); //2013.01.22: Ashley Lewis - Bug 7837

            SendFinishedMessage();
            RequestClose(ViewModelDialogResults.Cancel);
        }

        /// <summary>
        /// Used for loading all the inputs from the saved data or the data list
        /// </summary>
        public void LoadWorkflowInputs()
        {
            WorkflowInputs.Clear();
            Broker = Dev2StudioSessionFactory.CreateBroker();
            DebugTO = Broker.InitDebugSession(DebugTO);
            XmlData = DebugTO.XmlData;
            RememberInputs = DebugTO.RememberInputs;
            DataList = DebugTO.BinaryDataList;

            // Flipping Jurie....
            var myList = _dataListConversionUtils.CreateListToBindTo(DebugTO.BinaryDataList);

            WorkflowInputs.AddRange(myList);
        }

        /// <summary>
        /// Used for intelligently determining if a extra row should be added for the selected recordset
        /// </summary>
        /// <param name="itemToAdd">The item that is currently selected</param>
        public void AddRow(IDataListItem itemToAdd)
        {
            if(itemToAdd != null && itemToAdd.IsRecordset)
            {
                string error;
                IBinaryDataListEntry recordset;
                DataList.TryGetEntry(itemToAdd.Recordset, out recordset, out error);
                if(recordset != null)
                {
                    IList<Dev2Column> recsetCols = recordset.Columns;
                    IEnumerable<IDataListItem> numberOfRows = WorkflowInputs.Where(c => c.Recordset == itemToAdd.Recordset);
                    IEnumerable<IDataListItem> dataListItems = numberOfRows as IDataListItem[] ?? numberOfRows.ToArray();
                    IDataListItem lastItem = dataListItems.Last();
                    int indexToInsertAt = WorkflowInputs.IndexOf(lastItem);
                    string indexString = lastItem.RecordsetIndex;
                    int indexNum = Convert.ToInt32(indexString) + 1;
                    IEnumerable<IDataListItem> lastRow = dataListItems.Where(c => c.RecordsetIndex == indexString);
                    bool dontAddRow = true;
                    foreach(IDataListItem item in lastRow)
                    {
                        if(item.Value != string.Empty)
                        {
                            dontAddRow = false;
                        }
                    }
                    if(!dontAddRow)
                    {
                        AddBlankRowToRecordset(itemToAdd, recsetCols, indexToInsertAt, indexNum);
                    }
                }
            }
        }

        /// <summary>
        /// Used for removing a row for the collection
        /// </summary>
        /// <param name="itemToRemove">The item that will be removed from the collection</param>
        /// <param name="indexToSelect"></param>
        public bool RemoveRow(IDataListItem itemToRemove, out int indexToSelect)
        {
            indexToSelect = 1;
            bool itemsRemoved = false;
            if(itemToRemove != null && itemToRemove.IsRecordset)
            {
                // ReSharper disable once InconsistentNaming
                IEnumerable<IDataListItem> NumberOfRows = WorkflowInputs.Where(c => c.Recordset == itemToRemove.Recordset && c.Field == itemToRemove.Field);
                int numberOfRows = NumberOfRows.Count();
                List<IDataListItem> listToRemove = WorkflowInputs.Where(c => c.RecordsetIndex == numberOfRows.ToString(CultureInfo.InvariantCulture) && c.Recordset == itemToRemove.Recordset).ToList();

                if(numberOfRows == 2)
                {
                    IEnumerable<IDataListItem> firstRow = WorkflowInputs.Where(c => (c.RecordsetIndex == "1") && c.Recordset == itemToRemove.Recordset);
                    bool removeRow = firstRow.All(item => string.IsNullOrWhiteSpace(item.Value));

                    if(removeRow)
                    {

                        IEnumerable<IDataListItem> listToChange = WorkflowInputs.Where(c => (c.RecordsetIndex == "2") && c.Recordset == itemToRemove.Recordset);

                        foreach(IDataListItem item in listToChange)
                        {
                            item.Value = string.Empty;
                        }
                        foreach(IDataListItem item in listToRemove)
                        {
                            WorkflowInputs.Remove(item);
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if(itemToRemove.RecordsetIndex == item.RecordsetIndex)
                            {
                                indexToSelect = WorkflowInputs.IndexOf(WorkflowInputs.Last(c => c.Recordset == item.Recordset));
                            }
                            else
                            {
                                indexToSelect = WorkflowInputs.IndexOf(itemToRemove);
                            }
                            itemsRemoved = true;
                        }
                    }
                }
                else if(numberOfRows > 2)
                {
                    IEnumerable<IDataListItem> listToChange = WorkflowInputs.Where(c => (c.RecordsetIndex == (numberOfRows - 1).ToString(CultureInfo.InvariantCulture)) && c.Recordset == itemToRemove.Recordset);
                    foreach(IDataListItem item in listToChange)
                    {
                        item.Value = string.Empty;
                    }
                    foreach(IDataListItem item in listToRemove)
                    {
                        WorkflowInputs.Remove(item);
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if(itemToRemove.RecordsetIndex == item.RecordsetIndex)
                        {
                            indexToSelect = WorkflowInputs.IndexOf(WorkflowInputs.Last(c => c.Recordset == item.Recordset));
                        }
                        else
                        {
                            indexToSelect = WorkflowInputs.IndexOf(itemToRemove);
                        }
                        itemsRemoved = true;
                    }
                }
            }
            return itemsRemoved;
        }

        /// <summary>
        /// Used to transform the WorkflowInputs into XML
        /// </summary>
        public void SetXmlData()
        {
            CreateDataListObjectFromList();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlId = compiler.PushBinaryDataList(DataList.UID, DataList, out errors);
            string dataListString = compiler.ConvertFrom(dlId, DataListFormat.CreateFormat(GlobalConstants._XML_Inputs_Only), enTranslationDepth.Data, out errors);
            try
            {
                XmlData = XElement.Parse(dataListString).ToString();
            }
            catch(Exception)
            {
                XmlData = "Invalid characters entered";
            }
        }

        /// <summary>
        /// Used to transform the XML into WorkflowInputs
        /// </summary>
        public void SetWorkflowInputData()
        {
            string error;
            DataList = Broker.DeSerialize(XmlData, DebugTO.DataList, enTranslationTypes.XML, out error);
            if(string.IsNullOrEmpty(error))
            {
                WorkflowInputs.Clear();
                _dataListConversionUtils.CreateListToBindTo(DataList).ToList().ForEach(i => WorkflowInputs.Add(i));
            }
        }

        /// <summary>
        /// Used for just adding a blank row to a recordset
        /// </summary>
        /// <param name="selectedItem">The item that is currently selected</param>
        /// <param name="indexToSelect"></param>
        public bool AddBlankRow(IDataListItem selectedItem, out int indexToSelect)
        {
            indexToSelect = 1;
            bool itemsAdded = false;
            if(selectedItem != null && selectedItem.IsRecordset)
            {
                string error;
                IBinaryDataListEntry recordset;
                DataList.TryGetEntry(selectedItem.Recordset, out recordset, out error);
                if(recordset != null)
                {
                    IList<Dev2Column> recsetCols = recordset.Columns;
                    IEnumerable<IDataListItem> numberOfRows = WorkflowInputs.Where(c => c.Recordset == selectedItem.Recordset);
                    IDataListItem lastItem = numberOfRows.Last();
                    int indexToInsertAt = WorkflowInputs.IndexOf(lastItem);
                    string indexString = lastItem.RecordsetIndex;
                    int indexNum = Convert.ToInt32(indexString) + 1;
                    indexToSelect = indexToInsertAt + 1;
                    itemsAdded = AddBlankRowToRecordset(selectedItem, recsetCols, indexToInsertAt, indexNum);
                }
            }
            return itemsAdded;
        }


        protected override void OnDispose()
        {
            if(DataList != null)
            {
                var compiler = DataListFactory.CreateDataListCompiler();
                compiler.ForceDeleteDataListByID(DataList.UID);
            }
        }

        #endregion Methods

        #region Private Methods
        private void CreateDataListObjectFromList()
        {
            string error;
            DataList = Broker.DeSerialize("<Datalist></Datalist>", DebugTO.DataList ?? "<Datalist></Datalist>", enTranslationTypes.XML, out error);//2013.01.22: Ashley Lewis - Bug 7837
            // For some damn reason this does not always bind like it should! ;)
            Thread.Sleep(150);

            foreach(IDataListItem item in WorkflowInputs)
            {
                if(item.IsRecordset && !string.IsNullOrEmpty(item.Value))
                {
                    DataList.TryCreateRecordsetValue(item.Value, item.Field, item.Recordset, Convert.ToInt32(item.RecordsetIndex), out error);
                }
                else if(!item.IsRecordset)
                {
                    DataList.TryCreateScalarValue(item.Value, item.Field, out error);
                }
            }
        }

        private bool AddBlankRowToRecordset(IDataListItem dlItem, IList<Dev2Column> columns, int indexToInsertAt, int indexNum)
        {
            bool itemsAdded = false;
            if(dlItem.IsRecordset)
            {
                IList<Dev2Column> recsetCols = columns;
                foreach(Dev2Column col in recsetCols)
                {
                    WorkflowInputs.Insert(indexToInsertAt + 1, new DataListItem
                    {
                        DisplayValue = string.Concat(dlItem.Recordset, "(", indexNum, ").", col.ColumnName),
                        Value = string.Empty,
                        IsRecordset = dlItem.IsRecordset,
                        Recordset = dlItem.Recordset,
                        Field = col.ColumnName,
                        Description = col.ColumnDescription,
                        RecordsetIndex = indexNum.ToString(CultureInfo.InvariantCulture)
                    });
                    itemsAdded = true;
                    indexToInsertAt++;
                }
            }
            return itemsAdded;
        }

        protected override void OnViewAttached(object view, object context)
        {
            LoadWorkflowInputs();
            base.OnViewAttached(view, context);
        }
        #endregion Private Methods

        public void ViewClosed()
        {
            if(!CloseRequested)
                SendFinishedMessage();
        }

        public static WorkflowInputDataViewModel Create(IContextualResourceModel resourceModel)
        {
            return Create(resourceModel, Guid.Empty, DebugMode.Run);
        }

        public static WorkflowInputDataViewModel Create(IContextualResourceModel resourceModel, Guid sessionID, DebugMode debugMode)
        {
            VerifyArgument.IsNotNull("resourceModel", resourceModel);
            var debugInfoModel = ServiceDebugInfoModelFactory.CreateServiceDebugInfoModel(resourceModel, string.Empty, debugMode);

            var result = new WorkflowInputDataViewModel(debugInfoModel, sessionID)
            {
                CanDebug = resourceModel.UserPermissions.HasFlag(Permissions.Contribute)
            };
            return result;
        }
    }
}
