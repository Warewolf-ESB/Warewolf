
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Instrumentation;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.ViewModels.Workflow;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Workflow
// ReSharper restore CheckNamespace
{
    public class WorkflowInputDataViewModel : SimpleBaseViewModel
    {
        #region Fields
        //2012.10.11: massimo.guerrera - Added for PBI 5781
        private OptomizedObservableCollection<IDataListItem> _workflowInputs;
        private RelayCommand _executeCommmand;
        private DelegateCommand _cancelComand;
        private string _xmlData;
        private static IContextualResourceModel _resourceModel;
        private bool _rememberInputs;
        RelayCommand _viewInBrowserCommmand;
        static DataListConversionUtils _dataListConversionUtils;
        bool _canViewInBrowser;
        bool _canDebug;
        readonly IPopupController _popupController;
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
            Tracker.TrackEvent(TrackerEventGroup.Workflows, TrackerEventName.DebugClicked);
            var handler = DebugExecutionStart;
            if(handler != null)
            {
                handler();
            }
        }

        #region Ctor

        public WorkflowInputDataViewModel(IServiceDebugInfoModel input, Guid sessionId)
        {
            VerifyArgument.IsNotNull("input", input);
            CanDebug = true;
            CanViewInBrowser = true;

            DebugTo = new DebugTO
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
                SessionID = sessionId
            };

            if(input.DebugModeSetting == DebugMode.DebugInteractive)
            {
                DebugTo.IsDebugMode = true;
            }

            _resourceModel = input.ResourceModel;
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            DisplayName = "Debug input data";
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
            _dataListConversionUtils = new DataListConversionUtils();
            _popupController = CustomContainer.Get<IPopupController>();
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
        IDataListModel DataList { get; set; }

        /// <summary>
        /// The transfer object that holds all the information needed for a debug session
        /// </summary>
        public DebugTO DebugTo { get; set; }

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
                ViewInBrowserCommand.RaiseCanExecuteChanged();
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

        public ICommand CancelCommand
        {
            get
            {
                return _cancelComand ?? (_cancelComand = new DelegateCommand(param => Cancel()));
            }
        }
        public bool IsInError { get; set; }

        #endregion Cammands

        #region Methods

        /// <summary>
        /// Used for saving the data input by the user to the file system and pushing the data back at the workflow
        /// </summary>
        public void Save()
        {
            //2012.10.11: massimo.guerrera - Added for PBI 5781
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
            if(DebugTo.DataList != null)
            {
                Broker.PersistDebugSession(DebugTo);
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
                var dataList = XElement.Parse(DebugTo.XmlData);
                //
                dataList.Add(new XElement("BDSDebugMode", DebugTo.IsDebugMode));
                dataList.Add(new XElement("DebugSessionID", DebugTo.SessionID));
                dataList.Add(new XElement("EnvironmentID", _resourceModel.Environment.ID));
                OnDebugExecutionStart();
                SendExecuteRequest(dataList);
            }
        }

        protected virtual void SendExecuteRequest(XElement payload)
        {
            WebServer.Send(_resourceModel, payload.ToString(), new AsyncWorker());
        }

        public void ShowInvalidDataPopupMessage()
        {
            _popupController.Show(StringResources.DataInput_Error,
                                  StringResources.DataInput_Error_Title,
                                  MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false);

            IsInError = true;
        }

        public void ViewInBrowser()
        {
            if (!IsInError)
            {
                Tracker.TrackEvent(TrackerEventGroup.Workflows, TrackerEventName.ViewInBrowserClicked);
                DoSaveActions();
                string payload = BuildWebPayLoad();
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
            var allScalars = WorkflowInputs.All(item => !item.CanHaveMutipleRows);
            if(allScalars && WorkflowInputs.Count > 0)
            {
                return WorkflowInputs.Aggregate("", (current, workflowInput) => current + workflowInput.Field + "=" + workflowInput.Value + "&").TrimEnd('&');
            }
            try
            {
                return XElement.Parse(XmlData).ToString(SaveOptions.DisableFormatting);
            }
            catch(Exception)
            {
                //Error in the xml payload
                return "";
            }
        }

        protected virtual void SendViewInBrowserRequest(string payload)
        {
            WebServer.OpenInBrowser(_resourceModel, payload);
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
            DebugTo.XmlData = XmlData;
            DebugTo.RememberInputs = RememberInputs;
            if(DebugTo.DataList != null) Broker.PersistDebugSession(DebugTo); //2013.01.22: Ashley Lewis - Bug 7837

            SendFinishedMessage();
            RequestClose(ViewModelDialogResults.Cancel);
        }

        /// <summary>
        /// Used for loading all the inputs from the saved data or the data list
        /// </summary>
        public void LoadWorkflowInputs()
        {
            WorkflowInputs.Clear();
            if (Broker != null)
                Broker.Dispose();
            Broker = Dev2StudioSessionFactory.CreateBroker();
            if (DebugTo != null)
                DebugTo.CleanUp();
            DebugTo = Broker.InitDebugSession(DebugTo);
            XmlData = DebugTo.XmlData;
            RememberInputs = DebugTo.RememberInputs;
            DataList = DebugTo.BinaryDataList;

            var myList = _dataListConversionUtils.CreateListToBindTo(DebugTo.BinaryDataList);

            WorkflowInputs.AddRange(myList);
        }

        /// <summary>
        /// Used for intelligently determining if a extra row should be added for the selected recordset
        /// </summary>
        /// <param name="itemToAdd">The item that is currently selected</param>
        public void AddRow(IDataListItem itemToAdd)
        {
            if(itemToAdd != null && itemToAdd.CanHaveMutipleRows)
            {
                if (itemToAdd.IsObject)
                {
                    var parts = itemToAdd.DisplayValue.Split('.');
                    if (parts.Length >= 1)
                    {
                        var parentItem = DataList.ShapeComplexObjects.FirstOrDefault(o => o.Name == DataListUtil.ReplaceRecordsetIndexWithBlank(parts[0]));
                        if (parentItem != null)
                        {
                            for (int i = 1; i < parts.Length - 1; i++)
                            {

                                foreach (var child in parentItem.Children)
                                {
                                    var foundItem = child.Value.FirstOrDefault(o =>
                                    {
                                        var nameToCheck = DataListUtil.ReplaceRecordsetIndexWithBlank(parts[i]);
                                        return o.Name == nameToCheck;
                                    });
                                    if (foundItem != null)
                                    {
                                        parentItem = foundItem;
                                        break;
                                    }
                                }
                            }
                            if (parentItem.IsArray)
                            {
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
                                    AddBlankComplexObjectRow(itemToAdd,parentItem, indexToInsertAt, indexNum);
                                }
                            }
                        }                        
                    }
                    
                }
                IRecordSet recordset = DataList.ShapeRecordSets.FirstOrDefault(set => set.Name == itemToAdd.Recordset);
                if(recordset != null)
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
                    foreach(var item in lastRow)
                    {
                        if(item.Value != string.Empty)
                        {
                            addRow = true;
                        }
                    }
                    if(addRow)
                    {
                        AddBlankRowToRecordset(itemToAdd, recsetCols, indexToInsertAt, indexNum);
                    }
                }
            }
        }

        private void AddBlankComplexObjectRow(IDataListItem itemToAdd,IComplexObject parentItem, int indexToInsertAt, int indexNum)
        {
            var complexObjects = parentItem.Children[indexNum-1];
            foreach(var complexObject in complexObjects)
            {
                WorkflowInputs.Insert(indexToInsertAt + 1, new DataListItem
                    {
                        DisplayValue = string.Concat(itemToAdd.Recordset.TrimEnd('(',')'), "(", indexNum, ").", complexObject.Name),
                        Value = string.Empty,
                        CanHaveMutipleRows = itemToAdd.CanHaveMutipleRows,
                        Recordset = itemToAdd.Recordset,
                        Field = complexObject.Name,
                        Description = complexObject.Description,
                        Index = indexNum.ToString(CultureInfo.InvariantCulture)
                    });
                    indexToInsertAt++;
            }
            //
            //
            //            foreach (var col in parentItem..Distinct(new ScalarNameComparer()))
            //            {
            //                if (string.IsNullOrEmpty(colName) || !colName.Equals(col.Name))
            //                {
            //                    WorkflowInputs.Insert(indexToInsertAt + 1, new DataListItem
            //                    {
            //                        DisplayValue = string.Concat(dlItem.Recordset, "(", indexNum, ").", col.Name),
            //                        Value = string.Empty,
            //                        CanHaveMutipleRows = dlItem.CanHaveMutipleRows,
            //                        Recordset = dlItem.Recordset,
            //                        Field = col.Name,
            //                        Description = col.Description,
            //                        Index = indexNum.ToString(CultureInfo.InvariantCulture)
            //                    });
            //                    indexToInsertAt++;
            //                }
            //                colName = col.Name;
            //                itemsAdded = true;
            //
            //            }
        }

        /// <summary>
        /// Used for removing a row for the collection
        /// </summary>
        /// <param name="itemToRemove">The item that will be removed from the collection</param>
        /// <param name="indexToSelect"></param>
        public bool RemoveRow(IDataListItem itemToRemove, out int indexToSelect)
        {
            indexToSelect = 1;
            var itemsRemoved = false;
            if(itemToRemove != null && itemToRemove.CanHaveMutipleRows)
            {
                // ReSharper disable InconsistentNaming
                IEnumerable<IDataListItem> NumberOfRows = WorkflowInputs.Where(c => c.Recordset == itemToRemove.Recordset && c.Field == itemToRemove.Field);
                // ReSharper restore InconsistentNaming
                var numberOfRows = NumberOfRows.Count();
                List<IDataListItem> listToRemove = WorkflowInputs.Where(c => c.Index == numberOfRows.ToString(CultureInfo.InvariantCulture) && c.Recordset == itemToRemove.Recordset).ToList();

                if(numberOfRows == 2)
                {
                    IEnumerable<IDataListItem> firstRow = WorkflowInputs.Where(c => c.Index == "1" && c.Recordset == itemToRemove.Recordset);
                    bool removeRow = firstRow.All(item => string.IsNullOrWhiteSpace(item.Value));

                    if(removeRow)
                    {

                        IEnumerable<IDataListItem> listToChange = WorkflowInputs.Where(c => c.Index == "2" && c.Recordset == itemToRemove.Recordset);

                        foreach(IDataListItem item in listToChange)
                        {
                            item.Value = string.Empty;
                        }
                        foreach(IDataListItem item in listToRemove)
                        {
                            WorkflowInputs.Remove(item);
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if(itemToRemove.Index == item.Index)
                            {
                                IDataListItem item1 = item;
                                indexToSelect = WorkflowInputs.IndexOf(WorkflowInputs.Last(c => c.Recordset == item1.Recordset));
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
                    IEnumerable<IDataListItem> listToChange = WorkflowInputs.Where(c => c.Index == (numberOfRows - 1).ToString(CultureInfo.InvariantCulture) && c.Recordset == itemToRemove.Recordset);
                    foreach(IDataListItem item in listToChange)
                    {
                        item.Value = string.Empty;
                    }
                    foreach(IDataListItem item in listToRemove)
                    {
                        WorkflowInputs.Remove(item);
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if(itemToRemove.Index == item.Index)
                        {
                            IDataListItem item1 = item;
                            indexToSelect = WorkflowInputs.IndexOf(WorkflowInputs.Last(c => c.Recordset == item1.Recordset));
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
        protected const string RootTag = "DataList";
        /// <summary>
        /// Used to transform the WorkflowInputs into XML
        /// </summary>
        public void SetXmlData()
        {
            // For some damn reason this does not always bind like it should! ;)
            Thread.Sleep(150);
            StringBuilder result = new StringBuilder("<" + RootTag + ">");
            var recordSets = new List<IRecordSet>();

            foreach(var item in WorkflowInputs)
            {
                string val = item.Value;
                if (val != null && val.IsXml())
                {
                    val = string.Format(GlobalConstants.XMLPrefix + "{0}", Convert.ToBase64String(Encoding.UTF8.GetBytes(val)));
                }
                if (item.CanHaveMutipleRows)
                {
                    var recordSet = recordSets.FirstOrDefault(set => set.Name == item.Recordset);
                    if (recordSet==null)
                    {
                        recordSet = new RecordSet
                        {
                            Name = item.Recordset,
                            Columns = new Dictionary<int, List<IScalar>>()
                        };
                        recordSets.Add(recordSet);
                    }
                    recordSet.AddColumn(val, item.Field, Convert.ToInt32(item.Index));
                }
                else if(!item.CanHaveMutipleRows)
                {
                    DoScalarAppending(result,item);
                }
                }
            foreach(var recordSet in recordSets)
            {
                
                DoRecordSetAppending(recordSet,result);
            }
            result.Append("</" + RootTag + ">");
            try
            {
                XmlData = XElement.Parse(result.ToString()).ToString();
            }
            catch(Exception)
            {
                XmlData = "Invalid characters entered";
            }
        }

        private static void DoScalarAppending(StringBuilder result, IDataListItem val)
        {

            var fName = val.Field;
            result.Append("<");
            result.Append(fName);
            result.Append(">");
            try
            {
                result.Append(val.Value);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
            result.Append("</");
            result.Append(fName);
            result.Append(">");
        }

        private static void DoRecordSetAppending(IRecordSet recordSet, StringBuilder result)
        {
            var cnt = recordSet.Columns.Max(pair => pair.Key);
            
            for (var i = 1; i <= cnt; i++)
            {
                var rowData = recordSet.Columns[i];
                
                if (rowData.All(scalar => string.IsNullOrEmpty(scalar.Value)))
                {
                    continue;
                }
                result.Append("<");
                result.Append(recordSet.Name);
                result.Append(">");
                foreach (var col in rowData)
                {

                    var fName = col.Name;

                    

                        result.Append("<");
                        result.Append(fName);
                        result.Append(">");
                        try
                        {
                            result.Append(col.Value);
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch (Exception)
                        {
                        }
                        result.Append("</");
                        result.Append(fName);
                        result.Append(">");
                    }

                result.Append("</");
                result.Append(recordSet.Name);
                result.Append(">");
            }
        }
        /// <summary>
        /// Used to transform the XML into WorkflowInputs
        /// </summary>
        public void SetWorkflowInputData()
        {
                WorkflowInputs.Clear();
            DataList.PopulateWithData(XmlData);
                _dataListConversionUtils.CreateListToBindTo(DataList).ToList().ForEach(i => WorkflowInputs.Add(i));
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
            if(selectedItem != null && selectedItem.CanHaveMutipleRows)
            {
                var recordset = DataList.RecordSets.FirstOrDefault(set => set.Name == selectedItem.Recordset);
                if(recordset != null)
                {
                    var recsetCols = new List<IScalar>();
                    foreach(var column in recordset.Columns)
                    {
                        recsetCols.AddRange(column.Value);
                    }
                    IEnumerable<IDataListItem> numberOfRows = WorkflowInputs.Where(c => c.Recordset == selectedItem.Recordset);
                    IDataListItem lastItem = numberOfRows.Last();
                    int indexToInsertAt = WorkflowInputs.IndexOf(lastItem);
                    string indexString = lastItem.Index;
                    int indexNum = Convert.ToInt32(indexString) + 1;
                    indexToSelect = indexToInsertAt + 1;
                    itemsAdded = AddBlankRowToRecordset(selectedItem, recsetCols, indexToInsertAt, indexNum);
                }
            }
            return itemsAdded;
        }

        public IDataListItem GetNextRow(IDataListItem selectedItem)
        {
            if(selectedItem != null)
            {
                int indexToInsertAt = WorkflowInputs.IndexOf(selectedItem);
                if(indexToInsertAt != -1)
                {
                    var indexOfItemToGet = indexToInsertAt+1;
                    if(indexOfItemToGet == WorkflowInputs.Count)
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
            if(selectedItem != null)
            {
                int indexToInsertAt = WorkflowInputs.IndexOf(selectedItem);
                if(indexToInsertAt != -1)
                {
                    var indexOfItemToGet = indexToInsertAt-1;
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

        #endregion Methods

        #region Private Methods

        private bool AddBlankRowToRecordset(IDataListItem dlItem, IList<IScalar> columns, int indexToInsertAt, int indexNum)
        {
            bool itemsAdded = false;
            if(dlItem.CanHaveMutipleRows)
            {
                IList<IScalar> recsetCols = columns.Distinct(Scalar.Comparer).ToList();
                string colName = null;
                foreach(var col in recsetCols.Distinct(new ScalarNameComparer()))
                {
                    if(string.IsNullOrEmpty(colName) || !colName.Equals(col.Name))
                    {
                        WorkflowInputs.Insert(indexToInsertAt + 1, new DataListItem
                        {
                            DisplayValue = string.Concat(dlItem.Recordset, "(", indexNum, ").", col.Name),
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

        public static WorkflowInputDataViewModel Create(IContextualResourceModel resourceModel, Guid sessionId, DebugMode debugMode)
        {
            VerifyArgument.IsNotNull("resourceModel", resourceModel);
            var debugInfoModel = ServiceDebugInfoModelFactory.CreateServiceDebugInfoModel(resourceModel, string.Empty, debugMode);

            var result = new WorkflowInputDataViewModel(debugInfoModel, sessionId);
            if(resourceModel != null && resourceModel.Environment != null && resourceModel.Environment.AuthorizationService != null)
            {
                result.CanDebug = resourceModel.Environment.AuthorizationService.GetResourcePermissions(resourceModel.ID).CanDebug();
            }

            return result;
        }
    }

    internal class ScalarNameComparer : IEqualityComparer<IScalar>
    {
        #region Implementation of IEqualityComparer<in IScalar>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <paramref name="x"/> to compare.</param><param name="y">The second object of type <paramref name="y"/> to compare.</param>
        public bool Equals(IScalar x, IScalar y)
        {
            if (x == null) return false;
            if (y == null) return false;
            if (x.Name == null && y.Name == null) return true;
            if (x.Name != null)
            {
                return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(IScalar obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }
}
