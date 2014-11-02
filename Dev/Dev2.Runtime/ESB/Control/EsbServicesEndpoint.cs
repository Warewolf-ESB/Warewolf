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
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.Binary_Objects;
using Dev2.Data.ServiceModel;
using Dev2.Data.Storage;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Dev2.Runtime.ESB.Control
{
    /// <summary>
    ///     Amended as per PBI 7913
    /// </summary>
    /// IEsbActivityChannel
    public class EsbServicesEndpoint : IFrameworkDuplexDataChannel, IEsbWorkspaceChannel
    {
        #region IFrameworkDuplexDataChannel Members

        private readonly Dictionary<string, IFrameworkDuplexCallbackChannel> _users =
            new Dictionary<string, IFrameworkDuplexCallbackChannel>();

        private bool _doNotWipeDataList;

        public void Register(string userName)
        {
            if (_users.ContainsKey(userName))
            {
                _users.Remove(userName);
            }

            _users.Add(userName, OperationContext.Current.GetCallbackChannel<IFrameworkDuplexCallbackChannel>());
            NotifyAllClients(string.Format("User '{0}' logged in", userName));
        }

        public void Unregister(string userName)
        {
            if (UserExists(userName))
            {
                _users.Remove(userName);
                NotifyAllClients(string.Format("User '{0}' logged out", userName));
            }
        }

        public void ShowUsers(string userName)
        {
            var sb = new StringBuilder();
            sb.Append("=========Current Users==========");
            sb.Append("\r\n");
            _users.ToList().ForEach(c => sb.Append(c.Key + "\r\n"));
            SendPrivateMessage("System", userName, sb.ToString());
        }

        public void SendMessage(string userName, string message)
        {
            string suffix = " Said:";
            if (userName == "System")
            {
                suffix = string.Empty;
            }
            NotifyAllClients(string.Format("{0} {1} {2}", userName, suffix, message));
        }

        public void SendPrivateMessage(string userName, string targetUserName, string message)
        {
            string suffix = " Said:";
            if (userName == "System")
            {
                suffix = string.Empty;
            }
            if (UserExists(userName))
            {
                if (!UserExists(targetUserName))
                {
                    NotifyClient(userName,
                        string.Format("System: Message failed - User '{0}' has logged out ", targetUserName));
                }
                else
                {
                    NotifyClient(targetUserName, string.Format("{0} {1} {2}", userName, suffix, message));
                }
            }
        }

        public void SetDebug(string userName, string serviceName, bool debugOn)
        {
        }

        public void Rollback(string userName, string serviceName, int versionNo)
        {
        }

        public void Rename(string userName, string resourceType, string resourceName, string newResourceName)
        {
        }

        public void ReloadSpecific(string userName, string serviceName)
        {
        }

        public void Reload()
        {
        }

        private bool UserExists(string userName)
        {
            return _users.ContainsKey(userName) ||
                   userName.Equals("System", StringComparison.InvariantCultureIgnoreCase);
        }

        private void NotifyAllClients(string message)
        {
            _users.ToList().ForEach(c => NotifyClient(c.Key, message));
        }

        private void NotifyClient(string userName, string message)
        {
            try
            {
                if (UserExists(userName))
                {
                    _users[userName].CallbackNotification(message);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                _users.Remove(userName);
            }
        }

        #endregion

        private static readonly StringBuilder EmptyDataList = new StringBuilder("<DataList></DataList>");

        /// <summary>
        ///     Loads service definitions.
        ///     This is a singleton service so this object
        ///     will be visible in every call
        /// </summary>
        public EsbServicesEndpoint()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                throw;
            }
        }

        public bool LoggingEnabled
        {
            get { return true; }
        }

        /// <summary>
        ///     Executes the request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="request"></param>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceId,
            out ErrorResultTO errors)
        {
            Dev2Logger.Log.Info("START MEMORY USAGE [ " +
                                BinaryDataListStorageLayer.GetUsedMemoryInMb().ToString("####.####") + " MBs ]");
            Guid resultID = GlobalConstants.NullDataListID;
            errors = new ErrorResultTO();
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            IPrincipal principle = Thread.CurrentPrincipal;
            string name = principle.Identity.Name;
            Dev2Logger.Log.Info("EXECUTION USER CONTEXT IS [ " + name + " ] FOR SERVICE [ " + dataObject.ServiceName +
                                " ]");

            // If no DLID, we need to make it based upon the request ;)
            if (dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                StringBuilder theShape;

                try
                {
                    theShape = dataObject.ResourceID == Guid.Empty
                        ? FindServiceShape(workspaceId, dataObject.ServiceName)
                        : FindServiceShape(workspaceId, dataObject.ResourceID);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Log.Error(ex);
                    errors.AddError(string.Format("Service [ {0} ] not found.", dataObject.ServiceName));
                    return resultID;
                }

                // TODO : Amend here to respect Inputs only when creating shape ;)
                ErrorResultTO invokeErrors;
                dataObject.DataListID =
                    compiler.ConvertAndOnlyMapInputs(DataListFormat.CreateFormat(GlobalConstants._XML),
                        dataObject.RawPayload, theShape, out invokeErrors);
                // The act of doing this moves the index data correctly ;)
                // We need to remove this in the future.
#pragma warning disable 168
                // ReSharper disable UnusedVariable
                StringBuilder convertFrom = compiler.ConvertFrom(dataObject.DataListID,
                    DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data,
                    out errors);
                // ReSharper restore UnusedVariable
#pragma warning restore 168
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = new StringBuilder();

                // We need to create the parentID around the system ;)
                dataObject.ParentThreadID = Thread.CurrentThread.ManagedThreadId;
            }

            try
            {
                // Setup the invoker endpoint ;)
                using (var invoker = new EsbServiceInvoker(this, this, theWorkspace, request))
                {
                    // Should return the top level DLID
                    ErrorResultTO invokeErrors;
                    resultID = invoker.Invoke(dataObject, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
            }
            catch (Exception ex)
            {
                errors.AddError(ex.Message);
            }
            finally
            {
                // clean up after the request has executed ;)
                if (dataObject.IsDebug && !_doNotWipeDataList && !dataObject.IsRemoteInvoke)
                {
                    DataListRegistar.ClearDataList();
                }
                else
                {
                    foreach (var thread in dataObject.ThreadsToDispose)
                    {
                        DataListRegistar.DisposeScope(thread.Key, resultID);
                    }

                    DataListRegistar.DisposeScope(Thread.CurrentThread.ManagedThreadId, resultID);
                }
            }

            double memoryUse = BinaryDataListStorageLayer.GetUsedMemoryInMb();
            string logMemoryValue = memoryUse.ToString("####.####");

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (memoryUse == 0.0)
                // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                logMemoryValue = "0.0";
            }
            Dev2Logger.Log.Info("FINAL MEMORY USAGE AFTER DISPOSE [ " + logMemoryValue + " MBs ]");

            return resultID;
        }

        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceId, string uri,
            out ErrorResultTO errors)
        {
            errors = null;
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var executionContainer = new RemoteWorkflowExecutionContainer(null, dataObject, theWorkspace, this);
            executionContainer.PerformLogExecution(uri);
        }

        /// <summary>
        ///     Executes the sub request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceId, string inputDefs, string outputDefs,
            out ErrorResultTO errors)
        {
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            IEsbServiceInvoker invoker = CreateEsbServicesInvoker(theWorkspace);
            ErrorResultTO invokeErrors;
            Guid oldID = dataObject.DataListID;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> remainingMappings =
                ShapeForSubRequest(dataObject, inputDefs, outputDefs, out errors);

            // local non-scoped execution ;)
            bool isLocal = !dataObject.IsRemoteWorkflow();

            IPrincipal principle = Thread.CurrentPrincipal;
            Dev2Logger.Log.Info("SUB-EXECUTION USER CONTEXT IS [ " + principle.Identity.Name + " ] FOR SERVICE  [ " +
                                dataObject.ServiceName + " ]");

            Guid result = dataObject.DataListID;
            _doNotWipeDataList = false;
            if (dataObject.RunWorkflowAsync)
            {
                _doNotWipeDataList = true;
                ExecuteRequestAsync(dataObject, compiler, invoker, isLocal, oldID, out invokeErrors);
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                EsbExecutionContainer executionContainer = invoker.GenerateInvokeContainer(dataObject,
                    dataObject.ServiceName, isLocal, oldID);
                if (executionContainer != null)
                {
                    if (!isLocal)
                    {
                        _doNotWipeDataList = true;
                        SetRemoteExecutionDataList(dataObject, executionContainer);
                    }

                    executionContainer.InstanceOutputDefinition = outputDefs;
                    result = executionContainer.Execute(out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                    string errorString = compiler.FetchErrors(dataObject.DataListID, true);
                    invokeErrors = ErrorResultTO.MakeErrorResultFromDataListString(errorString);
                    errors.MergeErrors(invokeErrors);
                    // If Web-service or Plugin, skip the final shaping junk ;)
                    if (SubExecutionRequiresShape(workspaceId, dataObject.ServiceName))
                    {
                        if (!dataObject.IsDataListScoped && remainingMappings != null)
                        {
                            // Adjust the remaining output mappings ;)
                            compiler.SetParentID(dataObject.DataListID, oldID);
                            KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>> outputMappings =
                                remainingMappings.FirstOrDefault(c => c.Key == enDev2ArgumentType.Output);
                            compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, outputMappings.Value,
                                out invokeErrors);
                            errors.MergeErrors(invokeErrors);
                        }
                        else
                        {
                            compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, outputDefs,
                                out invokeErrors);
                            errors.MergeErrors(invokeErrors);
                        }
                    }

                    // The act of doing this moves the index data correctly ;)
                    // We need to remove this in the future.
#pragma warning disable 168
                    // ReSharper disable UnusedVariable
                    StringBuilder dl1 = compiler.ConvertFrom(dataObject.DataListID,
                        DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data,
                        out invokeErrors);
                    StringBuilder dl2 = compiler.ConvertFrom(oldID,
                        DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data,
                        out invokeErrors);
                    // ReSharper restore UnusedVariable
#pragma warning restore 168

                    return result;
                }
                errors.AddError("Null container returned");
            }
            return result;
        }

        /// <summary>
        ///     Shapes for sub request. Returns a key valid pair with remaining output mappings to be processed later!
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        public IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubRequest(
            IDSFDataObject dataObject, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            // We need to make it based upon the request ;)
            Guid oldID = dataObject.DataListID;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO invokeErrors;
            errors = new ErrorResultTO();

            StringBuilder theShape;

            if (IsServiceWorkflow(dataObject.WorkspaceID, dataObject.ResourceID))
            {
                theShape = FindServiceShape(dataObject.WorkspaceID, dataObject.ResourceID);
            }
            else
            {
                bool isDbService = dataObject.RemoteServiceType == "DbService" ||
                                   dataObject.RemoteServiceType == "InvokeStoredProc";
                theShape = ShapeMappingsToTargetDataList(inputDefs, outputDefs, out invokeErrors, isDbService);
                errors.MergeErrors(invokeErrors);
            }

            Guid shapeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(),
                theShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            dataObject.RawPayload = new StringBuilder();


            IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> remainingMappings = null;

            if (!dataObject.IsDataListScoped)
            {
                // Now ID flow through mappings ;)
                remainingMappings = compiler.ShapeForSubExecution(oldID, shapeID, inputDefs, outputDefs,
                    out invokeErrors);
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                // we have a scoped datalist ;)
                compiler.SetParentID(shapeID, oldID);
                shapeID = compiler.Shape(oldID, enDev2ArgumentType.Input, inputDefs, out invokeErrors, shapeID);
                errors.MergeErrors(invokeErrors);
            }

            // set execution ID ;)
            dataObject.DataListID = shapeID;

            return remainingMappings;
        }

        public T FetchServerModel<T>(IDSFDataObject dataObject, Guid workspaceId, out ErrorResultTO errors)
        {
            Guid serviceID = dataObject.ResourceID;
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var invoker = new EsbServiceInvoker(this, this, theWorkspace);
            EsbExecutionContainer generateInvokeContainer = invoker.GenerateInvokeContainer(dataObject, serviceID, true);
            Guid curDlid = generateInvokeContainer.Execute(out errors);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            StringBuilder convertFrom = compiler.ConvertFrom(curDlid, DataListFormat.CreateFormat(GlobalConstants._XML),
                enTranslationDepth.Data, out errors);
            var jsonSerializerSettings = new JsonSerializerSettings();
            var deserializeObject = JsonConvert.DeserializeObject<T>(convertFrom.ToString(), jsonSerializerSettings);
            return deserializeObject;
        }


        /// <summary>
        ///     Finds the service shape.
        /// </summary>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="resourceId">Name of the service.</param>
        /// <returns></returns>
        public StringBuilder FindServiceShape(Guid workspaceId, Guid resourceId)
        {
            var result = new StringBuilder();
            IResource resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceId) ??
                                 ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);

            if (resource == null)
            {
                return EmptyDataList;
            }

            if (resource.DataList != null)
            {
                result = resource.DataList;
            }

            // Handle services ;)
            if (result.ToString() == "<DataList />" && resource.ResourceType != ResourceType.WorkflowService)
            {
                ErrorResultTO errors;
                result = GlueInputAndOutputMappingSegments(resource.Outputs, resource.Inputs, out errors);
            }


            if (string.IsNullOrEmpty(result.ToString()))
            {
                return EmptyDataList;
            }
            result.Replace(GlobalConstants.SerializableResourceQuote, "\"");
            result.Replace(GlobalConstants.SerializableResourceSingleQuote, "\'");
            return result;
        }

        /// <summary>
        ///     Gets the correct data list.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="compiler">The compiler.</param>
        /// <returns></returns>
        public Guid CorrectDataList(IDSFDataObject dataObject, Guid workspaceId, out ErrorResultTO errors,
            IDataListCompiler compiler)
        {
            StringBuilder theShape;
            ErrorResultTO invokeErrors;
            errors = new ErrorResultTO();

            // If no DLID, we need to make it based upon the request ;)
            if (dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                theShape = FindServiceShape(workspaceId, dataObject.ResourceID);
                dataObject.DataListID =
                    compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags),
                        dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = new StringBuilder();
            }

            // force all items to exist in the DL ;)
            theShape = FindServiceShape(workspaceId, dataObject.ResourceID);
            Guid innerDatalistID =
                compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags),
                    new StringBuilder(), theShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // Add left to right
            IBinaryDataList left = compiler.FetchBinaryDataList(dataObject.DataListID, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            IBinaryDataList right = compiler.FetchBinaryDataList(innerDatalistID, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            DataListUtil.MergeDataList(left, right, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            compiler.PushBinaryDataList(left.UID, left, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            return innerDatalistID;
        }

        private static void SetRemoteExecutionDataList(IDSFDataObject dataObject,
            EsbExecutionContainer executionContainer)
        {
            var remoteContainer = executionContainer as RemoteWorkflowExecutionContainer;
            if (remoteContainer != null)
            {
                SerializableResource fetchRemoteResource = remoteContainer.FetchRemoteResource(dataObject.ServiceName);
                if (fetchRemoteResource != null)
                {
                    fetchRemoteResource.DataList =
                        fetchRemoteResource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"")
                            .Replace(GlobalConstants.SerializableResourceSingleQuote, "'");
                    string remoteDataList = fetchRemoteResource.DataList;
                    dataObject.RemoteInvokeResultShape = new StringBuilder(remoteDataList);
                }
            }
        }

        private void ExecuteRequestAsync(IDSFDataObject dataObject, IDataListCompiler compiler,
            IEsbServiceInvoker invoker, bool isLocal, Guid oldID, out ErrorResultTO invokeErrors)
        {
            IDSFDataObject clonedDataObject = dataObject.Clone();
            StringBuilder dl1 = compiler.ConvertFrom(dataObject.DataListID,
                DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data,
                out invokeErrors);
            StringBuilder shapeOfData = FindServiceShape(dataObject.WorkspaceID, dataObject.ResourceID);
            EsbExecutionContainer executionContainer = invoker.GenerateInvokeContainer(clonedDataObject,
                clonedDataObject.ServiceName, isLocal, oldID);
            if (executionContainer != null)
            {
                if (!isLocal)
                {
                    var remoteContainer = executionContainer as RemoteWorkflowExecutionContainer;
                    if (remoteContainer != null)
                    {
                        if (!remoteContainer.ServerIsUp())
                        {
                            invokeErrors.AddError("Asynchronous execution failed: Remote server unreachable");
                        }
                        SetRemoteExecutionDataList(dataObject, executionContainer);
                        shapeOfData = dataObject.RemoteInvokeResultShape;
                    }
                }
                if (!invokeErrors.HasErrors())
                {
                    Task task = Task.Factory.StartNew(() =>
                    {
                        Dev2Logger.Log.Info("ASYNC EXECUTION USER CONTEXT IS [ " + Thread.CurrentPrincipal.Identity.Name +
                                            " ]");
                        ErrorResultTO error;
                        clonedDataObject.DataListID =
                            compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), dl1,
                                shapeOfData, out error);
                        executionContainer.Execute(out error);
                    });

                    // ReSharper disable ImplicitlyCapturedClosure
                    task.ContinueWith(o =>
                        // ReSharper restore ImplicitlyCapturedClosure
                    {
                        DataListRegistar.DisposeScope(o.Id, clonedDataObject.DataListID);
                        o.Dispose();
                    });
                }
            }
            else
            {
                invokeErrors.AddError("Asynchronous execution failed: Resource not found");
            }
        }

        /// <summary>
        ///     Finds the service shape.
        /// </summary>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="resourceName">Name of the service.</param>
        /// <returns></returns>
        public StringBuilder FindServiceShape(Guid workspaceId, string resourceName)
        {
            IResource resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceName) ??
                                 ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceName);
            var result = new StringBuilder();
            if (resource == null)
            {
                return EmptyDataList;
            }

            if (resource.DataList != null)
            {
                result = resource.DataList;
            }

            // Handle services ;)
            if (result.ToString() == "<DataList />" && resource.ResourceType != ResourceType.WorkflowService)
            {
                ErrorResultTO errors;
                result = GlueInputAndOutputMappingSegments(resource.Outputs, resource.Inputs, out errors);
            }


            if (string.IsNullOrEmpty(result.ToString()))
            {
                return EmptyDataList;
            }
            result.Replace(GlobalConstants.SerializableResourceQuote, "\"");
            result.Replace(GlobalConstants.SerializableResourceSingleQuote, "\'");
            return result;
        }

        /// <summary>
        ///     Fetches the execution payload.
        /// </summary>
        /// <param name="dataObj">The data obj.</param>
        /// <param name="targetFormat">The target format.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string FetchExecutionPayload(IDSFDataObject dataObj, DataListFormat targetFormat,
            out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            string targetShape = FindServiceShape(dataObj.WorkspaceID, dataObj.ResourceID).ToString();
            var result = new StringBuilder();

            if (!string.IsNullOrEmpty(targetShape))
            {
                string translatorShape = ManipulateDataListShapeForOutput(targetShape);
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO invokeErrors;
                result = compiler.ConvertAndFilter(dataObj.DataListID, targetFormat, new StringBuilder(translatorShape),
                    out invokeErrors);
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                errors.AddError("Could not locate service shape for " + dataObj.ServiceName);
            }

            return result.ToString();
        }

        /// <summary>
        ///     Manipulates the data list shape for output.
        /// </summary>
        /// <param name="preShape">The pre shape.</param>
        /// <returns></returns>
        public string ManipulateDataListShapeForOutput(string preShape)
        {
            using (var stringReader = new StringReader(preShape))
            {
                XDocument xDoc = XDocument.Load(stringReader);

                XElement rootEl = xDoc.Element("DataList");
                if (rootEl == null) return xDoc.ToString();

                rootEl.Elements().Where(el =>
                {
                    XAttribute firstOrDefault = el.Attributes("ColumnIODirection").FirstOrDefault();
                    bool removeCondition = firstOrDefault != null &&
                                           (firstOrDefault.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                            firstOrDefault.Value == enDev2ColumnArgumentDirection.None.ToString());
                    return (removeCondition && !el.HasElements);
                }).Remove();

                IEnumerable<XElement> xElements = rootEl.Elements().Where(el => el.HasElements);
                IList<XElement> enumerable = xElements as IList<XElement> ?? xElements.ToList();
                enumerable.Elements().Where(element =>
                {
                    XAttribute xAttribute = element.Attributes("ColumnIODirection").FirstOrDefault();
                    bool removeCondition = xAttribute != null &&
                                           (xAttribute.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                            xAttribute.Value == enDev2ColumnArgumentDirection.None.ToString());
                    return removeCondition;
                }).Remove();
                enumerable.Where(element => !element.HasElements).Remove();
                return xDoc.ToString();
            }
        }

        private static bool IsServiceWorkflow(Guid workspaceID, Guid resourceID)
        {
            IResource resource = ResourceCatalog.Instance.GetResource(workspaceID, resourceID) ??
                                 ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceID);
            if (resource == null)
            {
                return false;
            }

            return resource.ResourceType == ResourceType.WorkflowService;
        }

        /// <summary>
        ///     Shapes the mappings automatic target data list.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <param name="outputs">The outputs.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="isDbService"></param>
        /// <returns></returns>
        private StringBuilder ShapeMappingsToTargetDataList(string inputs, string outputs, out ErrorResultTO errors,
            bool isDbService)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;
            StringBuilder oDL = DataListUtil.ShapeDefinitionsToDataList(outputs, enDev2ArgumentType.Output,
                out invokeErrors, isDbService: isDbService);
            errors.MergeErrors(invokeErrors);
            StringBuilder iDL = DataListUtil.ShapeDefinitionsToDataList(inputs, enDev2ArgumentType.Input,
                out invokeErrors, isDbService: isDbService);
            errors.MergeErrors(invokeErrors);

            StringBuilder result = GlueInputAndOutputMappingSegments(iDL.ToString(), oDL.ToString(), out invokeErrors);
            errors.MergeErrors(invokeErrors);
            return result;
        }

        /// <summary>
        ///     Glues the input and output mapping segments.
        /// </summary>
        /// <param name="inputFragment">The input fragment.</param>
        /// <param name="outputFragment">The output fragment.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private StringBuilder GlueInputAndOutputMappingSegments(string inputFragment, string outputFragment,
            out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if (!string.IsNullOrEmpty(outputFragment))
            {
                try
                {
                    // finally glue the two together ;)
                    var oDLXDoc = new XmlDocument();
                    oDLXDoc.LoadXml(outputFragment);

                    if (oDLXDoc.DocumentElement != null)
                    {
                        outputFragment = oDLXDoc.DocumentElement.InnerXml;
                    }
                }
                catch (Exception e)
                {
                    Dev2Logger.Log.Error(e);
                    errors.AddError(e.Message);
                }
            }

            if (!string.IsNullOrEmpty(inputFragment))
            {
                try
                {
                    // finally glue the two together ;)
                    var iDLXDoc = new XmlDocument();
                    iDLXDoc.LoadXml(inputFragment);

                    if (iDLXDoc.DocumentElement != null)
                    {
                        inputFragment = iDLXDoc.DocumentElement.InnerXml;
                    }
                }
                catch (Exception e)
                {
                    Dev2Logger.Log.Error(e);
                }
            }
            var result = new StringBuilder();
            result.Append("<DataList>");
            result.Append(outputFragment);
            result.Append(inputFragment);
            result.Append("</DataList>");
            return result;
        }


        /// <summary>
        ///     Subs the execution requires shape.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        private static bool SubExecutionRequiresShape(Guid workspaceID, string serviceName)
        {
            IResource resource = ResourceCatalog.Instance.GetResource(workspaceID, serviceName);
            return resource == null ||
                   (resource.ResourceType != ResourceType.WebService &&
                    resource.ResourceType != ResourceType.PluginService);
        }

        protected virtual IEsbServiceInvoker CreateEsbServicesInvoker(IWorkspace theWorkspace)
        {
            return new EsbServiceInvoker(this, this, theWorkspace);
        }
    }
}