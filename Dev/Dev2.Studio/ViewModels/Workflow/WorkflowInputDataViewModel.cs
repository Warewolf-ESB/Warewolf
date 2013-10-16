using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Input;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.WorkSurface;

namespace Dev2.Studio.ViewModels.Workflow
{
    public class WorkflowInputDataViewModel : SimpleBaseViewModel
    {
        #region Fields
        //2012.10.11: massimo.guerrera - Added for PBI 5781
        private OptomizedObservableCollection<IDataListItem> _workflowInputs;
        private RelayCommand _executeCommmand;
        private RelayCommand _cancelComand;
        private DebugTO _debugTO;
        private string _xmlData;
        private bool _workflowInputCount_rememberInputs;
        private readonly IContextualResourceModel _resourceModel;
        private IBinaryDataList _dataList;
        private int _workflowInputCount;
        private bool _rememberInputs;
        readonly DebugOutputViewModel _debugOutputViewModel;

        #endregion Fields

        #region Ctor

        public WorkflowInputDataViewModel( IServiceDebugInfoModel input, DebugOutputViewModel debugOutputViewModel)
        {
            VerifyArgument.IsNotNull("debugOutputViewModel", debugOutputViewModel);
            _debugOutputViewModel = debugOutputViewModel;

            DebugTO = new DebugTO
            {
                DataList = !string.IsNullOrEmpty(input.ResourceModel.DataList)
                               ? input.ResourceModel.DataList
                               : "<DataList></DataList>",
                ServiceName = input.ResourceModel.ResourceName,
                WorkflowID = input.ResourceModel.ResourceName,
                WorkflowXaml = input.ResourceModel.WorkflowXaml,
                XmlData = input.ServiceInputData,
                ResourceID = input.ResourceModel.ID,
                ServerID = input.ResourceModel.ServerID,
                RememberInputs = input.RememberInputs,
                SessionID = debugOutputViewModel.SessionID
            };

            if(input.DebugModeSetting == DebugMode.DebugInteractive)
            {
                DebugTO.IsDebugMode = true;
            }

            _resourceModel = input.ResourceModel;
            DisplayName = "Debug input data";
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
        private IBinaryDataList DataList
        {
            get { return _dataList; }
            set { _dataList = value; }
        }

        /// <summary>
        /// The transfer object that holds all the information needed for a debug session
        /// </summary>
        public DebugTO DebugTO
        {
            get
            {
                return _debugTO;
            }
            set
            {
                _debugTO = value;
            }
        }

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

        public ICommand OKCommand
        {
            get
            {
                if(_executeCommmand == null)
                {
                    _executeCommmand = new RelayCommand(param => Save(), param => true);
                }
                return _executeCommmand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if(_cancelComand == null)
                {
                    _cancelComand = new RelayCommand(param => Cancel(), param => true);
                }
                return _cancelComand;
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
            SetXMLData();
            DebugTO.XmlData = XmlData;
            DebugTO.RememberInputs = RememberInputs;
            if(DebugTO.DataList != null) Broker.PersistDebugSession(DebugTO);
            ExecuteWorkflow();
            RequestClose();
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

            var clientContext = _resourceModel.Environment.DsfChannel as IStudioClientContext;
            if(clientContext != null)
            {
                var dataList = XElement.Parse(DebugTO.XmlData);
                dataList.Add(new XElement("BDSDebugMode", DebugTO.IsDebugMode));
                dataList.Add(new XElement("DebugSessionID", DebugTO.SessionID));
                dataList.Add(new XElement("EnvironmentID", _resourceModel.Environment.ID));
                _debugOutputViewModel.DebugStatus = DebugStatus.Executing;

                SendExecuteRequest(dataList);
            }
            }

        protected virtual void SendExecuteRequest(XElement payload)
        {
            WebServer.SendAsync(WebServerMethod.POST, _resourceModel, payload.ToString(), ExecutionCallback);
        }

        private void ExecutionCallback(UploadStringCompletedEventArgs args)
        {
            //dont do anything 
        }

        private void SendFinishedMessage()
        {
            _debugOutputViewModel.DebugStatus = DebugStatus.Finished;
        }

        /// <summary>
        /// Used for saving the data input by the user to the file system when the cacel button is clicked
        /// </summary>
        public void Cancel()
        {
            //2012.10.11: massimo.guerrera - Added for PBI 5781
            SetXMLData();
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
            var myList = CreateListToBindTo(DebugTO.BinaryDataList);

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
                IList<Dev2Column> recsetCols = new List<Dev2Column>();
                IBinaryDataListEntry recordset;
                DataList.TryGetEntry(itemToAdd.Recordset, out recordset, out error);
                if(recordset != null)
                {
                    recsetCols = recordset.Columns;
                    IEnumerable<IDataListItem> NumberOfRows = WorkflowInputs.Where(c => c.Recordset == itemToAdd.Recordset);
                    IDataListItem lastItem = NumberOfRows.Last();
                    int IndexToInsertAt = WorkflowInputs.IndexOf(lastItem);
                    string indexString = lastItem.RecordsetIndex;
                    int indexNum = Convert.ToInt32(indexString) + 1;
                    IEnumerable<IDataListItem> lastRow = NumberOfRows.Where(c => c.RecordsetIndex == indexString);
                    bool DontAddRow = true;
                    foreach(IDataListItem item in lastRow)
                    {
                        if(item.Value != string.Empty)
                        {
                            DontAddRow = false;
                        }
                    }
                    if(!DontAddRow)
                    {
                        AddBlankRowToRecordset(itemToAdd, recsetCols, IndexToInsertAt, indexNum, false);
                    }
                }
            }
        }

        /// <summary>
        /// Used for removing a row for the collection
        /// </summary>
        /// <param name="itemToRemove">The item that will be removed from the collection</param>
        public bool RemoveRow(IDataListItem itemToRemove, out int indexToSelect)
        {
            indexToSelect = 1;
            bool itemsRemoved = false;
            if(itemToRemove != null && itemToRemove.IsRecordset)
            {
                IEnumerable<IDataListItem> NumberOfRows = WorkflowInputs.Where(c => c.Recordset == itemToRemove.Recordset && c.Field == itemToRemove.Field);
                int numberOfRows = NumberOfRows.Count();
                List<IDataListItem> listToRemove = WorkflowInputs.Where(c => c.RecordsetIndex == numberOfRows.ToString() && c.Recordset == itemToRemove.Recordset).ToList();

                if(numberOfRows == 2)
                {
                    IEnumerable<IDataListItem> firstRow = WorkflowInputs.Where(c => (c.RecordsetIndex == "1") && c.Recordset == itemToRemove.Recordset);
                    bool removeRow = true;
                    foreach(IDataListItem item in firstRow)
                    {
                        if(!string.IsNullOrWhiteSpace(item.Value))
                        {
                            removeRow = false;
                            break;
                        }
                    }

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
                    IEnumerable<IDataListItem> listToChange = WorkflowInputs.Where(c => (c.RecordsetIndex == (numberOfRows - 1).ToString()) && c.Recordset == itemToRemove.Recordset);
                    foreach(IDataListItem item in listToChange)
                    {
                        item.Value = string.Empty;
                    }
                    foreach(IDataListItem item in listToRemove)
                    {
                        WorkflowInputs.Remove(item);
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
        public void SetXMLData()
        {
            string error = "";
            CreateDataListObjectFromList();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors = new ErrorResultTO();
            Guid dlId = compiler.PushBinaryDataList(DataList.UID, DataList, out errors);
            string dataListString = compiler.ConvertFrom(dlId, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);
            if(string.IsNullOrEmpty(error))
            {
                try
                {
                    XmlData = XElement.Parse(dataListString).ToString();
                }
                catch(Exception)
                {
                    XmlData = "Invalid characters entered";
                }
            }
        }

        /// <summary>
        /// Used to transform the XML into WorkflowInputs
        /// </summary>
        public void SetWorkflowInputData()
        {
            string error = "";
            DataList = Broker.DeSerialize(XmlData, DebugTO.DataList, enTranslationTypes.XML, out error);
            if(string.IsNullOrEmpty(error))
            {
                WorkflowInputs.Clear();
                CreateListToBindTo(DataList).ToList().ForEach(i => WorkflowInputs.Add(i));
            }
        }

        /// <summary>
        /// Used for just adding a blank row to a recordset
        /// </summary>
        /// <param name="selectedItem">The item that is currently selected</param>
        public bool AddBlankRow(IDataListItem selectedItem, out int indexToSelect)
        {
            indexToSelect = 1;
            bool itemsAdded = false;
            if(selectedItem != null && selectedItem.IsRecordset)
            {
                string error = "";
                IBinaryDataListEntry recordset;
                DataList.TryGetEntry(selectedItem.Recordset, out recordset, out error);
                if(recordset != null)
                {
                    IList<Dev2Column> recsetCols = recordset.Columns;
                    IEnumerable<IDataListItem> NumberOfRows = WorkflowInputs.Where(c => c.Recordset == selectedItem.Recordset);
                    IDataListItem lastItem = NumberOfRows.Last();
                    int IndexToInsertAt = WorkflowInputs.IndexOf(lastItem);
                    string indexString = lastItem.RecordsetIndex;
                    int indexNum = Convert.ToInt32(indexString) + 1;
                    indexToSelect = IndexToInsertAt + 1;
                    itemsAdded = AddBlankRowToRecordset(selectedItem, recsetCols, IndexToInsertAt, indexNum, true);
                }
            }
            return itemsAdded;
        }
        #endregion Methods

        #region Private Methods
        private void CreateDataListObjectFromList()
        {
            string error = "";
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

        private OptomizedObservableCollection<IDataListItem> CreateListToBindTo(IBinaryDataList dataList)
        {
            var result = new OptomizedObservableCollection<IDataListItem>();

            if (dataList != null)
            {
                var listOfEntries = dataList.FetchAllEntries();

                foreach (IBinaryDataListEntry entry in listOfEntries
                    .Where(e => (e.ColumnIODirection == enDev2ColumnArgumentDirection.Input ||
                                 e.ColumnIODirection == enDev2ColumnArgumentDirection.Both)))
                {
                    result.AddRange(ConvertIBinaryDataListEntryToIDataListItem(entry));
                }
            }

            return result;
        }

        private IList<IDataListItem> ConvertIBinaryDataListEntryToIDataListItem(IBinaryDataListEntry dataListEntry)
        {
            IList<IDataListItem> result = new List<IDataListItem>();
            if(dataListEntry.IsRecordset)
            {
                int sizeOfCollection = dataListEntry.ItemCollectionSize();
                if(sizeOfCollection == 0) { sizeOfCollection++; }
                int count = 0;
                string error = string.Empty;

                while(count < sizeOfCollection)
                {

                    IList<IBinaryDataListItem> items = dataListEntry.FetchRecordAt(count + 1, out error);
                    foreach(IBinaryDataListItem item in items)
                    {
                        IDataListItem singleRes = new DataListItem();
                        singleRes.IsRecordset = true;
                        singleRes.Recordset = item.Namespace;
                        singleRes.Field = item.FieldName;
                        singleRes.RecordsetIndex = (count + 1).ToString();
                        singleRes.Value = item.TheValue;

                        if (string.IsNullOrEmpty(item.DisplayValue))
                        {
                            
                        }

                        singleRes.DisplayValue = item.DisplayValue;
                        var desc = dataListEntry.Columns.FirstOrDefault(c => c.ColumnName == item.FieldName);
                        if(desc == null)
                        {
                            singleRes.Description = null;
                        }
                        else
                        {
                            singleRes.Description = desc.ColumnDescription;
                        }
                        result.Add(singleRes);
                    }
                    count++;
                }
            }
            else
            {
                IBinaryDataListItem item = dataListEntry.FetchScalar();
                if(item != null)
                {
                    IDataListItem singleRes = new DataListItem();
                    singleRes.IsRecordset = false;
                    singleRes.Field = item.FieldName;
                    singleRes.DisplayValue = item.FieldName;
                    singleRes.Value = item.TheValue;
                    string desc = dataListEntry.Description;
                    if(string.IsNullOrWhiteSpace(desc))
                    {
                        singleRes.Description = null;
                    }
                    else
                    {
                        singleRes.Description = desc;
                    }
                    result.Add(singleRes);
                }
            }
            return result;
        }

        private bool AddBlankRowToRecordset(IDataListItem dlItem, IList<Dev2Column> columns, int indexToInsertAt, int indexNum, bool setFocus)
        {
            bool itemsAdded = false;
            if(dlItem.IsRecordset)
            {
                IList<Dev2Column> recsetCols = columns;
                foreach(Dev2Column col in recsetCols)
                {
                    WorkflowInputs.Insert(indexToInsertAt + 1, new DataListItem()
                    {
                        DisplayValue = string.Concat(dlItem.Recordset, "(", indexNum, ").", col.ColumnName),
                        Value = string.Empty,
                        IsRecordset = dlItem.IsRecordset,
                        Recordset = dlItem.Recordset,
                        Field = col.ColumnName,
                        Description = col.ColumnDescription,
                        RecordsetIndex = indexNum.ToString()
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
    }
}
