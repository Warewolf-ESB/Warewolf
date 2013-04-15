using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Runtime.ESB;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Dev2.DynamicServices
{

    /// <summary>
    /// Amended as per PBI 7913
    /// </summary>
    /// IEsbActivityChannel
    public class EsbServicesEndpoint : IFrameworkDuplexDataChannel, IEsbWorkspaceChannel
    {

        #region IFrameworkDuplexDataChannel Members
        Dictionary<string, IFrameworkDuplexCallbackChannel> _users = new Dictionary<string, IFrameworkDuplexCallbackChannel>();
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
            // 5782: Removed static global variable: ds
            //if (UserExists(userName))
            //{
            //    var targetService = ds.Services.Find(c => c.Name == serviceName);
            //    if (targetService != null)
            //    {

            //        var debuggerUser = targetService.Debuggers.Find(debugger => debugger == userName);
            //        if (!string.IsNullOrEmpty(debuggerUser))
            //        {
            //            if (!debugOn)
            //            {
            //                targetService.Debuggers.Remove(userName);
            //            }
            //        }
            //        else
            //        {
            //            if (debugOn)
            //            {
            //                targetService.Debuggers.Add(userName);
            //            }
            //        }
            //    }
            //}
        }

        public void Rollback(string userName, string serviceName, int versionNo)
        {
            //if (UserExists(userName)) {
            //    var targetService = ds.Services.Find(service => service.Name == serviceName);
            //    if (targetService != null) {
            //        string fileName = string.Format("{0}\\{1}.V{2}.xml", "Services\\VersionControl", serviceName, versionNo.ToString());
            //        if (File.Exists(fileName)) {
            //            var items = ds.GenerateObjectGraphFromString(File.ReadAllText(fileName));
            //            dynamic response = ds.AddResources(items, "Domain Admins,Business Design Studio Developers,Business Design Studio Testers");
            //            SendPrivateMessage("System", userName, response.XmlString);
            //        }
            //        else {
            //            SendPrivateMessage("System", userName, "Version not found!");

            //        }
            //    }
            //}
        }

        public void Rename(string userName, string resourceType, string resourceName, string newResourceName)
        {


        }

        public void ReloadSpecific(string userName, string serviceName)
        {


        }

        public void Reload()
        {
            // 5782: Removed static global variable: ds
            //ds.RestoreResources(new string[] { "Sources", "Services", "ActivityDefs" });
        }

        private bool UserExists(string userName)
        {
            return _users.ContainsKey(userName) || userName.Equals("System", StringComparison.InvariantCultureIgnoreCase);
        }

        private void NotifyAllClients(string message)
        {
            _users.ToList().ForEach(c => NotifyClient(c.Key, message));
        }

        private void NotifyClient(string userName, string message)
        {

            try
            {
                if(UserExists(userName))
                {
                    _users[userName].CallbackNotification(message);
                }
            }
            catch
            {
                _users.Remove(userName);
            }
        }

        #endregion
        //private ParallelCommandExecutor _parallel;

        /// <summary>
        ///Loads service definitions.
        ///This is a singleton service so this object
        ///will be visible in every call 
        /// </summary>
        public EsbServicesEndpoint()
        {
            try
            {
                //_parallel = new ParallelCommandExecutor(this);
            }
            catch(Exception ex)
            {
                TraceWriter.WriteTrace("Error Loading Endpoint...");
                throw ex;
            }
        }

        public bool LoggingEnabled
        {
            get
            {
                return true;
            }
        }

        #region Travis' New Entry Point
        /// <summary>
        /// Executes the request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteRequest(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors)
        {
            Guid resultID = GlobalConstants.NullDataListID;
            errors = new ErrorResultTO();
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceID);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            // If no DLID, we need to make it based upon the request ;)
            if(dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                string theShape = null;
                try
                {
                    theShape = FindServiceShape(workspaceID, dataObject.ServiceName);
                }
                catch(Exception e)
                {
                   errors.AddError(string.Format("Unable to find the service '{0}'.", dataObject.ServiceName));
                   return resultID;
                }

                ErrorResultTO invokeErrors;
                dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = string.Empty;
            }

            try
            {
                ErrorResultTO invokeErrors;
                // Setup the invoker endpoint ;)
                var invoker = new DynamicServicesInvoker(this, this, theWorkspace);

                // Should return the top level DLID
                resultID = invoker.Invoke(dataObject, out invokeErrors);
                errors.MergeErrors(invokeErrors);

            }
            catch(Exception ex)
            {
                errors.AddError(ex.Message);
            }

            return resultID;
        }

        /// <summary>
        /// Executes the transactionally scoped request, caller must delete datalist
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteTransactionallyScopedRequest(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors)
        {
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceID);
            var invoker = new DynamicServicesInvoker(this, this, theWorkspace);
            errors = new ErrorResultTO();
            string theShape;
            Guid oldID = new Guid();
            Guid innerDatalistID = new Guid();
            ErrorResultTO invokeErrors;

            // Account for silly webpages...
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            // If no DLID, we need to make it based upon the request ;)
            if (dataObject.DataListID == GlobalConstants.NullDataListID)
            { 
                theShape= FindServiceShape(workspaceID, dataObject.ServiceName);
                dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), 
                    dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = string.Empty;
            }

            if (!dataObject.IsDataListScoped)
            {
                theShape = FindServiceShape(workspaceID, dataObject.ServiceName);
                innerDatalistID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                    dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                var mergedID = compiler.Merge(innerDatalistID, dataObject.DataListID,
                                                      enDataListMergeTypes.Union, enTranslationDepth.Data,
                                                      true, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                oldID = dataObject.DataListID;
                dataObject.DataListID = mergedID;
            }


            EsbExecutionContainer executionContainer = invoker.GenerateInvokeContainer(dataObject, dataObject.ServiceName);
            Guid result = dataObject.DataListID;

            if (!dataObject.IsDataListScoped)
            {
                compiler.DeleteDataListByID(oldID);
                compiler.DeleteDataListByID(innerDatalistID);
            }

            if (executionContainer != null)
            {
                result = executionContainer.Execute(out errors);
            }
            else
            {
                errors.AddError("Null container returned");
            }


            return result;

        }

        #endregion

        #region Public Entry Point

        ///// <summary>
        ///// Entry point for all client requests
        ///// </summary>
        ///// <param name="XmlRequest">string for well-formed XML containing the request data</param>
        ///// <returns>string containing the response(s) to the request</returns>
        //public string ExecuteRequest(string xmlRequest, Guid dataListID)
        //{

        //    // Break this to facilitate Better DataList managent
        //    string executionPayload = ExecuteRequest(xmlRequest, WorkspaceRepository.Instance.ServerWorkspace, dataListID);

        //    // 2012.10.17 - 5782: TWR - Added call with server workspace parameter
        //    return executionPayload;
        //}

        ///// <summary>
        ///// Entry point for all client requests
        ///// </summary>
        ///// <param name="XmlRequest">string for well-formed XML containing the request data</param>
        ///// <returns>string containing the response(s) to the request</returns>
        //public string ExecuteRequest(string xmlRequest, Guid workID, Guid dataListID) {

        //    // Break this to facilitate Better DataList managent
        //    string executionPayload = ExecuteRequest(xmlRequest, WorkspaceRepository.Instance.Get(workID), dataListID);

        //    // 2012.10.17 - 5782: TWR - Added call with server workspace parameter
        //    return executionPayload;
        //}

        ///// <summary>
        ///// Entry point for all client requests
        ///// </summary>
        ///// <param name="XmlRequest">string for well-formed XML containing the request data</param>
        ///// <returns>string containing the response(s) to the request</returns>
        //public string ExecuteRequest(string xmlRequest, IWorkspace workspace, Guid dataListID)
        //{
        //    // 2012.10.17 - 5782: TWR - Added workspace parameter

        //    //TODO: Add security validation
        //    //Create allowed callers section in the service definition 
        //    //that must store a list if I.P addresses that are allowed to 
        //    //call this service.
        //    //If the caller is not in the allowed callers list then 
        //    //reject this request immediately.


        //    dynamic dynXmlRequest = new UnlimitedObject();


        //    #region Identify Caller
        //    //OperationContext context = OperationContext.Current;
        //    //MessageProperties properties = context.IncomingMessageProperties;
        //    //RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

        //    #endregion

        //    try
        //    {
        //        dynXmlRequest = new UnlimitedObject();
        //        dynXmlRequest.Load(xmlRequest);

        //        //TraceWriter.WriteTrace(this, string.Format("Executing Service '{0}'...", dynXmlRequest.Service), Resources.TraceMessageType_Message);

        //        if (dynXmlRequest.HasError && dynXmlRequest.Error.Length > 0)
        //        {
        //            dynXmlRequest.DynamicServiceFrameworkMessage.Error = "Error in request - inbound message contains error tag";
        //            return dynXmlRequest.XmlString;
        //        }


        //    }
        //    catch (XmlException handlerEx)
        //    {

        //        ExceptionHandling.WriteEventLogEntry(
        //             Resources.DynamicService_EventLogTarget
        //             , Resources.DynamicService_EventLogSource
        //             , handlerEx.ToString()
        //             , EventLogEntryType.Error);

        //        return dynXmlRequest.XmlString;
        //    }

        //    string result = string.Empty;
        //    Guid resultID = GlobalConstants.NullDataListID;
        //    ErrorResultTO errors = new ErrorResultTO();

        //    try
        //    {
        //        // 2012.10.17 - 5782: TWR - Added workspace parameter
        //        IDynamicServicesInvoker invoker = new DynamicServicesInvoker(this, this, _loggingEnabled, workspace);

        //        // Should return the top level DLID
        //        resultID = invoker.Invoke(workspace.Host, dynXmlRequest, dataListID, out errors);
        //        if (errors.HasErrors()) {
        //            result = errors.MakeUserReady();
        //    }

        //        //result = serviceResult.XmlString;
        //    }
        //    catch (Exception e)
        //    {
        //        result = "<Error>" + e.Message + "</Error>";
        //        TraceWriter.WriteTrace("Payload [ " + dynXmlRequest + " ] caused [ " + e.Message + " ]");
        //    }finally{
        //        // PBI : 5376 
        //        if(dataListID == GlobalConstants.NullDataListID){
        //            // Plant any errors that have bubbled
        //            if (errors.HasErrors()) {
        //                string error = string.Empty;
        //                IBinaryDataListItem tmpI = Dev2BinaryDataListFactory.CreateBinaryItem(errors.MakeDataListReady(), "Error");
        //                IBinaryDataListEntry tmpE = Dev2BinaryDataListFactory.CreateEntry("Error", string.Empty);
        //                tmpE.TryPutScalar(tmpI, out error);
        //                _svrCompiler.Upsert(null, resultID, (GlobalConstants.SystemTagNamespace + "." + enSystemTag.Error), tmpE, out errors);
        //            }

        //            // top level ;)
        //            // Else return the string
        //            DataListTranslatedPayloadTO data = _svrCompiler.ConvertFrom(null, resultID, enTranslationDepth.Data, DataListFormat.CreateFormat(GlobalConstants._XML), out errors);

        //            if (data != null) {
        //                result = data.FetchAsString();
        //            } else {
        //                result = "<Error>Cannot fetch final DataList [ " + resultID + " ]</Error>";
        //            }

        //            // Clean up ;)
        //            _svrCompiler.DeleteDataListByID(resultID, true);
        //        }else{
        //            ErrorResultTO mergeErrors = new ErrorResultTO();
        //            // merge data back into this DataList
        //            if (dataListID != resultID) {
        //                TraceWriter.WriteTrace("Fatal DynamicServiceEndpoint Invoke... Failed to return input datalist!");
        //                errors.AddError("FATAL ERROR : Execution Mis-Match");
        //                result = GlobalConstants.NullDataListID.ToString();
        //            } else {
        //                result = resultID.ToString();
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public bool ExecuteParallel(IEsbActivityInstruction[] instructions)
        //{
        //    int performed = _parallel.Perform(instructions);
        //    return performed == instructions.Length;
        //}

        //private string ExecuteCommandParallel(string xmlRequest, Guid dataListID)
        //{
        //    return string.Empty;
        //    //return ExecuteRequest(xmlRequest, dataListID);
        //}

        #endregion

        public string FindServiceShape(Guid workspaceID, string serviceName)
        {
            var services = ResourceCatalog.Instance.GetDynamicObjects<DynamicService>(workspaceID, serviceName);

            var tmp = services.FirstOrDefault();
            var result = "<DataList></DataList>";

            if(tmp != null)
            {
                result = tmp.DataListSpecification;
            }

            return result;
        }

        //#region ParallelCommandExecutor
        //private sealed class ParallelCommandExecutor
        //{
        //    private EsbServicesEndpoint _owner;
        //    private IEsbActivityInstruction[] _instructions;
        //    private ParameterizedThreadStart _threadStart;
        //    private ManualResetEvent _wait;
        //    private object _performGuard;
        //    private object _counterGuard;

        //    private int _length;
        //    private int _counter;

        //    public ParallelCommandExecutor(EsbServicesEndpoint owner)
        //    {
        //        _owner = owner;
        //        _performGuard = new object();
        //        _counterGuard = new object();
        //        _threadStart = OnThreadStart;
        //        _wait = new ManualResetEvent(false);
        //    }

        //    public int Perform(IEsbActivityInstruction[] instructions)
        //    {
        //        int performed = 0;

        //        lock (_performGuard)
        //        {
        //            _wait.Reset();
        //            _counter = 0;
        //            _instructions = instructions;
        //            _length = _instructions.Length;

        //            int totalThreads = 6;

        //            if (_length < totalThreads)
        //            {
        //                totalThreads = _length;
        //            }

        //            int batchSize = _length / totalThreads;
        //            int remainder = _length - (batchSize * totalThreads);
        //            int offset = 0;


        //            if (_length > 0)
        //            {
        //                for (int i = 0; i < totalThreads - 1; i++)
        //                {
        //                    ArraySegment<IEsbActivityInstruction> array = new ArraySegment<IEsbActivityInstruction>(_instructions, offset, batchSize);
        //                    offset += batchSize;
        //                    new Thread(_threadStart).Start(array);
        //                }

        //                {
        //                    ArraySegment<IEsbActivityInstruction> array = new ArraySegment<IEsbActivityInstruction>(_instructions, offset, batchSize + remainder);
        //                    offset += batchSize;
        //                    new Thread(_threadStart).Start(array);
        //                }

        //                _wait.WaitOne();
        //            }

        //            performed = _counter;
        //            _instructions = null;
        //        }

        //        return performed;
        //    }

        //    private void OnThreadStart(object obj)
        //    {
        //        int increment = 1;

        //        if (obj is ArraySegment<IEsbActivityInstruction>)
        //        {
        //            ArraySegment<IEsbActivityInstruction> arraySegment = (ArraySegment<IEsbActivityInstruction>)obj;
        //            increment = arraySegment.Count;
        //            IEsbActivityInstruction[] rawArray = arraySegment.Array;

        //            for (int i = 0; i < arraySegment.Count; i++)
        //            {
        //                IEsbActivityInstruction activity = rawArray[i + arraySegment.Offset];
        //                activity.Result = _owner.ExecuteCommandParallel(activity.Instruction, activity.DataListID);
        //            }

        //        }
        //        else
        //        {
        //            IEsbActivityInstruction activity = obj as IEsbActivityInstruction;
        //            activity.Result = _owner.ExecuteCommandParallel(activity.Instruction, activity.DataListID);
        //        }


        //        bool resume = false;

        //        lock (_counterGuard)
        //        {
        //            if ((_counter += increment) == _length)
        //            {
        //                resume = true;
        //            }
        //        }

        //        if (resume) _wait.Set();
        //    }

        //}
        //#endregion
    }
}
