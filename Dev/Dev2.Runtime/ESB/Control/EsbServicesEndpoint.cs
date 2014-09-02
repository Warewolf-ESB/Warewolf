using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Dev2.Data.Storage;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Runtime.ESB.Control
{

    /// <summary>
    /// Amended as per PBI 7913
    /// </summary>
    /// IEsbActivityChannel
    public class EsbServicesEndpoint : IFrameworkDuplexDataChannel, IEsbWorkspaceChannel
    {

        #region IFrameworkDuplexDataChannel Members

        readonly Dictionary<string, IFrameworkDuplexCallbackChannel> _users = new Dictionary<string, IFrameworkDuplexCallbackChannel>();
        bool _doNotWipeDataList;

        public void Register(string userName)
        {
            if(_users.ContainsKey(userName))
            {
                _users.Remove(userName);
            }

            _users.Add(userName, OperationContext.Current.GetCallbackChannel<IFrameworkDuplexCallbackChannel>());
            NotifyAllClients(string.Format("User '{0}' logged in", userName));

        }

        public void Unregister(string userName)
        {
            if(UserExists(userName))
            {
                _users.Remove(userName);
                NotifyAllClients(string.Format("User '{0}' logged out", userName));
            }
        }

        public void ShowUsers(string userName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("=========Current Users==========");
            sb.Append("\r\n");
            _users.ToList().ForEach(c => sb.Append(c.Key + "\r\n"));
            SendPrivateMessage("System", userName, sb.ToString());

        }

        public void SendMessage(string userName, string message)
        {
            string suffix = " Said:";
            if(userName == "System")
            {
                suffix = string.Empty;
            }
            NotifyAllClients(string.Format("{0} {1} {2}", userName, suffix, message));
        }

        public void SendPrivateMessage(string userName, string targetUserName, string message)
        {
            string suffix = " Said:";
            if(userName == "System")
            {
                suffix = string.Empty;
            }
            if(UserExists(userName))
            {
                if(!UserExists(targetUserName))
                {
                    NotifyClient(userName, string.Format("System: Message failed - User '{0}' has logged out ", targetUserName));
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

        bool UserExists(string userName)
        {
            return _users.ContainsKey(userName) || userName.Equals("System", StringComparison.InvariantCultureIgnoreCase);
        }

        void NotifyAllClients(string message)
        {
            _users.ToList().ForEach(c => NotifyClient(c.Key, message));
        }

        void NotifyClient(string userName, string message)
        {

            try
            {
                if(UserExists(userName))
                {
                    _users[userName].CallbackNotification(message);
                }
            }
            catch(Exception ex)
            {
                this.LogError(ex);
                _users.Remove(userName);
            }
        }

        #endregion


        /// <summary>
        ///Loads service definitions.
        ///This is a singleton service so this object
        ///will be visible in every call 
        /// </summary>
        public EsbServicesEndpoint()
        {
            try
            {

            }
            catch(Exception ex)
            {
                this.LogError(ex);
                throw;
            }
        }

        public bool LoggingEnabled
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Executes the request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="request"></param>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errors)
        {
            ServerLogger.LogMessage("START MEMORY USAGE [ " + BinaryDataListStorageLayer.GetUsedMemoryInMb().ToString("####.####") + " MBs ]");
            var resultID = GlobalConstants.NullDataListID;
            errors = new ErrorResultTO();
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var compiler = DataListFactory.CreateDataListCompiler();

            var principle = Thread.CurrentPrincipal;
            ServerLogger.LogMessage("EXECUTION USER CONTEXT IS [ " + principle.Identity.Name + " ] FOR SERVICE [ " + dataObject.ServiceName + " ]");

            // If no DLID, we need to make it based upon the request ;)
            if(dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                string theShape;

                try
                {
                    theShape = dataObject.ResourceID == Guid.Empty ? FindServiceShape(workspaceId, dataObject.ServiceName) : FindServiceShape(workspaceId, dataObject.ResourceID);
                }
                catch(Exception ex)
                {
                    this.LogError(ex);
                    errors.AddError(string.Format("Service [ {0} ] not found.", dataObject.ServiceName));
                    return resultID;
                }

                // TODO : Amend here to respect Inputs only when creating shape ;)
                ErrorResultTO invokeErrors;
                dataObject.DataListID = compiler.ConvertAndOnlyMapInputs(DataListFormat.CreateFormat(GlobalConstants._XML), dataObject.RawPayload, theShape, out invokeErrors);
                // The act of doing this moves the index data correctly ;)
                // We need to remove this in the future.
#pragma warning disable 168
                // ReSharper disable UnusedVariable
                var convertFrom = compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out errors);
                // ReSharper restore UnusedVariable
#pragma warning restore 168
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = string.Empty;

                // We need to create the parentID around the system ;)
                dataObject.ParentThreadID = Thread.CurrentThread.ManagedThreadId;

            }

            try
            {
                // Setup the invoker endpoint ;)
                using(var invoker = new EsbServiceInvoker(this, this, theWorkspace, request))
                {
                    // Should return the top level DLID
                    ErrorResultTO invokeErrors;
                    resultID = invoker.Invoke(dataObject, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
            }
            catch(Exception ex)
            {
                errors.AddError(ex.Message);
            }
            finally
            {
                // clean up after the request has executed ;)
                if(dataObject.IsDebug && !_doNotWipeDataList && !dataObject.IsRemoteInvoke)
                {
                    DataListRegistar.ClearDataList();
                }
                else
                {
                    foreach(var thread in dataObject.ThreadsToDispose)
                    {
                        DataListRegistar.DisposeScope(thread.Key, resultID);
                    }

                    DataListRegistar.DisposeScope(Thread.CurrentThread.ManagedThreadId, resultID);
                }

            }

            var memoryUse = BinaryDataListStorageLayer.GetUsedMemoryInMb();
            var logMemoryValue = memoryUse.ToString("####.####");

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if(memoryUse == 0.0)
            // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                logMemoryValue = "0.0";
            }
            ServerLogger.LogMessage("FINAL MEMORY USAGE AFTER DISPOSE [ " + logMemoryValue + " MBs ]");

            return resultID;
        }

        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceId, string uri, out ErrorResultTO errors)
        {
            errors = null;
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var executionContainer = new RemoteWorkflowExecutionContainer(null, dataObject, theWorkspace, this);
            executionContainer.PerformLogExecution(uri);
        }

        /// <summary>
        /// Executes the sub request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceId, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var invoker = CreateEsbServicesInvoker(theWorkspace);
            ErrorResultTO invokeErrors;
            var oldID = dataObject.DataListID;
            var compiler = DataListFactory.CreateDataListCompiler();

            var remainingMappings = ShapeForSubRequest(dataObject, inputDefs, outputDefs, out errors);

            // local non-scoped execution ;)
            var isLocal = !dataObject.IsRemoteWorkflow();

            var principle = Thread.CurrentPrincipal;
            ServerLogger.LogMessage("SUB-EXECUTION USER CONTEXT IS [ " + principle.Identity.Name + " ] FOR SERVICE  [ " + dataObject.ServiceName + " ]");

            var result = dataObject.DataListID;
            _doNotWipeDataList = false;
            if(dataObject.RunWorkflowAsync)
            {
                _doNotWipeDataList = true;
                ExecuteRequestAsync(dataObject, compiler, invoker, isLocal, oldID, out invokeErrors);
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                var executionContainer = invoker.GenerateInvokeContainer(dataObject, dataObject.ServiceName, isLocal, oldID);
                if(executionContainer != null)
                {
                    if(!isLocal)
                    {
                        _doNotWipeDataList = true;
                        SetRemoteExecutionDataList(dataObject, executionContainer);
                    }
                    executionContainer.InstanceOutputDefinition = outputDefs;
                    result = executionContainer.Execute(out invokeErrors);
                    errors.MergeErrors(invokeErrors);

                    // If Web-service or Plugin, skip the final shaping junk ;)
                    if(SubExecutionRequiresShape(workspaceId, dataObject.ServiceName))
                    {
                        if(!dataObject.IsDataListScoped && remainingMappings != null)
                        {
                            // Adjust the remaining output mappings ;)
                            compiler.SetParentID(dataObject.DataListID, oldID);
                            var outputMappings = remainingMappings.FirstOrDefault(c => c.Key == enDev2ArgumentType.Output);
                            compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, outputMappings.Value,
                                           out invokeErrors);
                            errors.MergeErrors(invokeErrors);
                        }
                        else
                        {
                            compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, outputDefs, out invokeErrors);
                            errors.MergeErrors(invokeErrors);
                        }
                    }

                    // The act of doing this moves the index data correctly ;)
                    // We need to remove this in the future.
#pragma warning disable 168
                    // ReSharper disable UnusedVariable
                    var dl1 = compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out invokeErrors);
                    var dl2 = compiler.ConvertFrom(oldID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out invokeErrors);
                    // ReSharper restore UnusedVariable
#pragma warning restore 168

                    return result;
                }
                errors.AddError("Null container returned");
            }
            return result;
        }

        static void SetRemoteExecutionDataList(IDSFDataObject dataObject, EsbExecutionContainer executionContainer)
        {
            var remoteContainer = executionContainer as RemoteWorkflowExecutionContainer;
            if(remoteContainer != null)
            {
                var fetchRemoteResource = remoteContainer.FetchRemoteResource(dataObject.ServiceName);
                if(fetchRemoteResource != null)
                {
                    fetchRemoteResource.DataList = fetchRemoteResource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "'");
                    var remoteDataList = fetchRemoteResource.DataList;
                    dataObject.RemoteInvokeResultShape = remoteDataList;
                }
            }
        }

        void ExecuteRequestAsync(IDSFDataObject dataObject, IDataListCompiler compiler, IEsbServiceInvoker invoker, bool isLocal, Guid oldID, out ErrorResultTO invokeErrors)
        {
            var clonedDataObject = dataObject.Clone();
            var dl1 = compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out invokeErrors);
            var shapeOfData = FindServiceShape(dataObject.WorkspaceID, dataObject.ResourceID);
            var executionContainer = invoker.GenerateInvokeContainer(clonedDataObject, clonedDataObject.ServiceName, isLocal, oldID);
            if(executionContainer != null)
            {
                if(!isLocal)
                {
                    var remoteContainer = executionContainer as RemoteWorkflowExecutionContainer;
                    if(remoteContainer != null)
                    {
                        if(!remoteContainer.ServerIsUp())
                        {
                            invokeErrors.AddError("Asynchronous execution failed: Remote server unreachable");
                        }
                        SetRemoteExecutionDataList(dataObject, executionContainer);
                        shapeOfData = dataObject.RemoteInvokeResultShape;
                    }
                }
                if(!invokeErrors.HasErrors())
                {
                    var task = Task.Factory.StartNew(() =>
                    {
                        ServerLogger.LogMessage("ASYNC EXECUTION USER CONTEXT IS [ " + Thread.CurrentPrincipal.Identity.Name + " ]");
                        ErrorResultTO error;
                        clonedDataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), dl1, shapeOfData, out error);
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
        /// Shapes for sub request. Returns a key valid pair with remaining output mappings to be processed later!
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        public IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubRequest(IDSFDataObject dataObject, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            // We need to make it based upon the request ;)
            var oldID = dataObject.DataListID;
            var compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO invokeErrors;
            errors = new ErrorResultTO();

            string theShape;

            if(IsServiceWorkflow(dataObject.WorkspaceID, dataObject.ResourceID))
            {
                theShape = FindServiceShape(dataObject.WorkspaceID, dataObject.ResourceID);
            }
            else
            {
                var isDbService = dataObject.RemoteServiceType == "DbService" || dataObject.RemoteServiceType == "InvokeStoredProc";
                theShape = ShapeMappingsToTargetDataList(inputDefs, outputDefs, out invokeErrors, isDbService);
                errors.MergeErrors(invokeErrors);
            }

            var shapeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, theShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            dataObject.RawPayload = string.Empty;


            IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> remainingMappings = null;

            if(!dataObject.IsDataListScoped)
            {
                // Now ID flow through mappings ;)
                remainingMappings = compiler.ShapeForSubExecution(oldID, shapeID, inputDefs, outputDefs, out invokeErrors);
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
            var serviceID = dataObject.ResourceID;
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var invoker = new EsbServiceInvoker(this, this, theWorkspace);
            var generateInvokeContainer = invoker.GenerateInvokeContainer(dataObject, serviceID, true);
            var curDlid = generateInvokeContainer.Execute(out errors);
            var compiler = DataListFactory.CreateDataListCompiler();
            var convertFrom = compiler.ConvertFrom(curDlid, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);
            var jsonSerializerSettings = new JsonSerializerSettings();
            var deserializeObject = JsonConvert.DeserializeObject<T>(convertFrom, jsonSerializerSettings);
            return deserializeObject;
        }


        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="resourceId">Name of the service.</param>
        /// <returns></returns>
        public string FindServiceShape(Guid workspaceId, Guid resourceId)
        {
            var result = "<DataList></DataList>";
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceId) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);

            if(resource == null)
            {
                return result;
            }

            result = resource.DataList;

            // Handle services ;)
            if(result == "<DataList />" && resource.ResourceType != ResourceType.WorkflowService)
            {
                ErrorResultTO errors;
                result = GlueInputAndOutputMappingSegments(resource.Outputs, resource.Inputs, out errors);
            }


            if(string.IsNullOrEmpty(result))
            {
                result = "<DataList></DataList>";
            }

            return result;
        }

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="resourceName">Name of the service.</param>
        /// <returns></returns>
        public string FindServiceShape(Guid workspaceId, string resourceName)
        {
            var result = "<DataList></DataList>";
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceName) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceName);

            if(resource == null)
            {
                return result;
            }

            result = resource.DataList;

            // Handle services ;)
            if(result == "<DataList />" && resource.ResourceType != ResourceType.WorkflowService)
            {
                ErrorResultTO errors;
                result = GlueInputAndOutputMappingSegments(resource.Outputs, resource.Inputs, out errors);
            }


            if(string.IsNullOrEmpty(result))
            {
                result = "<DataList></DataList>";
            }

            return result;
        }

        /// <summary>
        /// Fetches the execution payload.
        /// </summary>
        /// <param name="dataObj">The data obj.</param>
        /// <param name="targetFormat">The target format.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string FetchExecutionPayload(IDSFDataObject dataObj, DataListFormat targetFormat, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var targetShape = FindServiceShape(dataObj.WorkspaceID, dataObj.ResourceID);
            var result = string.Empty;

            if(!string.IsNullOrEmpty(targetShape))
            {
                string translatorShape = ManipulateDataListShapeForOutput(targetShape);
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO invokeErrors;
                result = compiler.ConvertAndFilter(dataObj.DataListID, targetFormat, translatorShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                errors.AddError("Could not locate service shape for " + dataObj.ServiceName);
            }

            return result;
        }

        /// <summary>
        /// Gets the correct data list.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="compiler">The compiler.</param>
        /// <returns></returns>
        public Guid CorrectDataList(IDSFDataObject dataObject, Guid workspaceId, out ErrorResultTO errors, IDataListCompiler compiler)
        {
            string theShape;
            ErrorResultTO invokeErrors;
            errors = new ErrorResultTO();

            // If no DLID, we need to make it based upon the request ;)
            if(dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                theShape = FindServiceShape(workspaceId, dataObject.ResourceID);
                dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = string.Empty;
            }

            // force all items to exist in the DL ;)
            theShape = FindServiceShape(workspaceId, dataObject.ResourceID);
            var innerDatalistID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), string.Empty, theShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // Add left to right
            var left = compiler.FetchBinaryDataList(dataObject.DataListID, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            var right = compiler.FetchBinaryDataList(innerDatalistID, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            DataListUtil.MergeDataList(left, right, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            compiler.PushBinaryDataList(left.UID, left, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            return innerDatalistID;
        }

        /// <summary>
        /// Manipulates the data list shape for output.
        /// </summary>
        /// <param name="preShape">The pre shape.</param>
        /// <returns></returns>
        public string ManipulateDataListShapeForOutput(string preShape)
        {

            using(var stringReader = new StringReader(preShape))
            {
                var xDoc = XDocument.Load(stringReader);

                var rootEl = xDoc.Element("DataList");
                if(rootEl == null) return xDoc.ToString();

                rootEl.Elements().Where(el =>
                {
                    var firstOrDefault = el.Attributes("ColumnIODirection").FirstOrDefault();
                    var removeCondition = firstOrDefault != null &&
                                          (firstOrDefault.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                           firstOrDefault.Value == enDev2ColumnArgumentDirection.None.ToString());
                    return (removeCondition && !el.HasElements);
                }).Remove();

                var xElements = rootEl.Elements().Where(el => el.HasElements);
                var enumerable = xElements as IList<XElement> ?? xElements.ToList();
                enumerable.Elements().Where(element =>
                {
                    var xAttribute = element.Attributes("ColumnIODirection").FirstOrDefault();
                    var removeCondition = xAttribute != null &&
                                          (xAttribute.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                           xAttribute.Value == enDev2ColumnArgumentDirection.None.ToString());
                    return removeCondition;
                }).Remove();
                enumerable.Where(element => !element.HasElements).Remove();
                return xDoc.ToString();
            }
        }

        static bool IsServiceWorkflow(Guid workspaceID, Guid resourceID)
        {
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, resourceID) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceID);
            if(resource == null)
            {
                return false;
            }

            return resource.ResourceType == ResourceType.WorkflowService;
        }

        /// <summary>
        /// Shapes the mappings automatic target data list.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <param name="outputs">The outputs.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="isDbService"></param>
        /// <returns></returns>
        string ShapeMappingsToTargetDataList(string inputs, string outputs, out ErrorResultTO errors, bool isDbService)
        {
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;
            var oDL = DataListUtil.ShapeDefinitionsToDataList(outputs, enDev2ArgumentType.Output, out invokeErrors, isDbService: isDbService);
            errors.MergeErrors(invokeErrors);
            var iDL = DataListUtil.ShapeDefinitionsToDataList(inputs, enDev2ArgumentType.Input, out invokeErrors, isDbService: isDbService);
            errors.MergeErrors(invokeErrors);

            var result = GlueInputAndOutputMappingSegments(iDL, oDL, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            return result;
        }

        /// <summary>
        /// Glues the input and output mapping segments.
        /// </summary>
        /// <param name="inputFragment">The input fragment.</param>
        /// <param name="outputFragment">The output fragment.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string GlueInputAndOutputMappingSegments(string inputFragment, string outputFragment, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            if(!string.IsNullOrEmpty(outputFragment))
            {
                try
                {
                    // finally glue the two together ;)
                    XmlDocument oDLXDoc = new XmlDocument();
                    oDLXDoc.LoadXml(outputFragment);

                    if(oDLXDoc.DocumentElement != null)
                    {
                        outputFragment = oDLXDoc.DocumentElement.InnerXml;
                    }
                }
                catch(Exception e)
                {
                    this.LogError(e);
                    errors.AddError(e.Message);
                }
            }

            if(!string.IsNullOrEmpty(inputFragment))
            {

                try
                {
                    // finally glue the two together ;)
                    XmlDocument iDLXDoc = new XmlDocument();
                    iDLXDoc.LoadXml(inputFragment);

                    if(iDLXDoc.DocumentElement != null)
                    {
                        inputFragment = iDLXDoc.DocumentElement.InnerXml;
                    }
                }
                catch(Exception e)
                {
                    this.LogError(e);
                }
            }

            return "<DataList>" + outputFragment + inputFragment + "</DataList>";
        }


        /// <summary>
        /// Subs the execution requires shape.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        static bool SubExecutionRequiresShape(Guid workspaceID, string serviceName)
        {
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, serviceName);
            return resource == null || (resource.ResourceType != ResourceType.WebService && resource.ResourceType != ResourceType.PluginService);

        }

        protected virtual IEsbServiceInvoker CreateEsbServicesInvoker(IWorkspace theWorkspace)
        {
            return new EsbServiceInvoker(this, this, theWorkspace);
        }
    }
}
