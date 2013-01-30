using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Xml;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.Runtime.InterfaceImplementors;
using Dev2.Server.Datalist;
using Dev2.Workspaces;
using Unlimited.Framework;

// ReSharper disable CheckNamespace
namespace Dev2.DynamicServices
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Author:     Sameer Chunilall
    /// Date:       2009-12-22-
    /// Desc:       Service Implementation that allows the calling application to structure an Xml Request and send it to this service for 
    ///             execution. The service sends back the Xml Request with an appropriate response for consumption.
    /// </summary>
    /// 
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DynamicServicesEndpoint : IFrameworkDuplexDataChannel, IFrameworkActivityChannel, IFrameworkWorkspaceChannel
    {

        #region IFrameworkDuplexDataChannel Members
        readonly Dictionary<string, IFrameworkDuplexCallbackChannel> _users = new Dictionary<string, IFrameworkDuplexCallbackChannel>();
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
            StringBuilder sb = new StringBuilder();
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
                if (UserExists(userName))
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

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        bool _loggingEnabled;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private ParallelCommandExecutor _parallel;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        // ReSharper disable InconsistentNaming
        private readonly static IServerDataListCompiler _svrCompiler = DataListFactory.CreateServerDataListCompiler();
        // ReSharper restore InconsistentNaming

        // 5782: Removed static global variable: ds
        //static Unlimited.Framework.DynamicServices.DynamicServicesHost ds;

        /// <summary>
        ///Loads service definitions.
        ///This is a singleton service so this object
        ///will be visible in every call 
        /// </summary>
        public DynamicServicesEndpoint()
        {
            try
            {
                _parallel = new ParallelCommandExecutor(this);
                var loggingEnabled = ConfigurationManager.AppSettings["LoggingEnabled"];
                _loggingEnabled = !string.IsNullOrEmpty(loggingEnabled);

                //ConfigurePerformanceCounters();

                TraceWriter.WriteTrace("Initializing Dynamic Service Engine...");

                // 2012.10.17 - 5782: TWR - Replaced with server workspace host
                //ds = WorkspaceRepository.Instance.ServerWorkspace.Host;

                // 2012.10.17 - 5782: TWR - Moved reserved initialization to DynamicServicesHost constructor
            }
            catch (Exception)
            {
                TraceWriter.WriteTrace("Error Loading Endpoint...");
                throw;
            }
        }

        public bool LoggingEnabled
        {
            get
            {
                return _loggingEnabled;
            }
        }

        #region Public Entry Point

        /// <summary>
        /// Entry point for all client requests
        /// </summary>
        /// <param name="xmlRequest">The XML request.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns>
        /// string containing the response(s) to the request
        /// </returns>
        public string ExecuteCommand(string xmlRequest, Guid dataListID)
        {

            // Break this to facilitate Better DataList managent
            string executionPayload = ExecuteCommand(xmlRequest, WorkspaceRepository.Instance.ServerWorkspace, dataListID);

            // 2012.10.17 - 5782: TWR - Added call with server workspace parameter
            return executionPayload;
        }

        /// <summary>
        /// Entry point for all client requests
        /// </summary>
        /// <param name="xmlRequest">The XML request.</param>
        /// <param name="workID">The work ID.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns>
        /// string containing the response(s) to the request
        /// </returns>
        public string ExecuteCommand(string xmlRequest, Guid workID, Guid dataListID)
        {

            // Break this to facilitate Better DataList managent
            string executionPayload = ExecuteCommand(xmlRequest, WorkspaceRepository.Instance.Get(workID), dataListID);

            // 2012.10.17 - 5782: TWR - Added call with server workspace parameter
            return executionPayload;

        }

        /// <summary>
        /// Entry point for all client requests
        /// </summary>
        /// <param name="xmlRequest">The XML request.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns>
        /// string containing the response(s) to the request
        /// </returns>
        public string ExecuteCommand(string xmlRequest, IWorkspace workspace, Guid dataListID)
        {
            // 2012.10.17 - 5782: TWR - Added workspace parameter

            //TODO: Add security validation
            //Create allowed callers section in the service definition 
            //that must store a list if I.P addresses that are allowed to 
            //call this service.
            //If the caller is not in the allowed callers list then 
            //reject this request immediately.


            dynamic dynXmlRequest = new UnlimitedObject();

          
            try
            {
                dynXmlRequest = new UnlimitedObject();
                dynXmlRequest.Load(xmlRequest);

                //TraceWriter.WriteTrace(this, string.Format("Executing Service '{0}'...", dynXmlRequest.Service), Resources.TraceMessageType_Message);

                if (dynXmlRequest.HasError && dynXmlRequest.Error.Length > 0)
                {
                    dynXmlRequest.DynamicServiceFrameworkMessage.Error = "Error in request - inbound message contains error tag";
                    return dynXmlRequest.XmlString;
                }


            }
            catch (XmlException handlerEx)
            {

                ExceptionHandling.WriteEventLogEntry(
                     Resources.DynamicService_EventLogTarget
                     , Resources.DynamicService_EventLogSource
                     , handlerEx.ToString()
                     , EventLogEntryType.Error);

                return dynXmlRequest.XmlString;
            }

            string result;
            Guid resultID = GlobalConstants.NullDataListID;
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();

            bool hadEx = false;

            try
            {
                // 2012.10.17 - 5782: TWR - Added workspace parameter
                IDynamicServicesInvoker invoker = new DynamicServicesInvoker(this, this, _loggingEnabled, workspace);

                // Should return the top level DLID
                resultID = invoker.Invoke(workspace.Host, dynXmlRequest, dataListID, out errors);
                allErrors.MergeErrors(errors);
                //GCWriter.WriteData("Invoker ID " + resultID);

                if (errors.HasErrors())
                {
                    // ReSharper disable RedundantAssignment
                    //result = errors.MakeUserReady();
                    // ReSharper restore RedundantAssignment
                }
            }
            catch (Exception e)
            {
                
                allErrors.AddError(e.Message);
                TraceWriter.WriteTrace("Payload [ " + dynXmlRequest + " ] caused [ " + e.Message + " ]");
                hadEx = true;
            }finally{
                // PBI : 5376 
                if (dataListID == GlobalConstants.NullDataListID)
                {
                    // Plant any errors that have bubbled
                    if (errors.HasErrors() && !hadEx) {
                        string error;
                        IBinaryDataListItem tmpI = Dev2BinaryDataListFactory.CreateBinaryItem(errors.MakeDataListReady(), "Error");
                        IBinaryDataListEntry tmpE = Dev2BinaryDataListFactory.CreateEntry("Error", string.Empty);
                        tmpE.TryPutScalar(tmpI, out error);
                        _svrCompiler.Upsert(null, resultID, (GlobalConstants.SystemTagNamespace + "." + enSystemTag.Error), tmpE, out errors);
                        allErrors.MergeErrors(errors);
                    }

                    // top level ;)
                    // Else return the string
                    DataListTranslatedPayloadTO data = null;
                    if(!hadEx)
                    {
                        data = _svrCompiler.ConvertFrom(null, resultID, enTranslationDepth.Data, DataListFormat.CreateFormat(GlobalConstants._XML), out errors);
                        allErrors.MergeErrors(errors);
                    }

                    if (data != null)
                    {
                        result = data.FetchAsString();
                    }
                    else
                    {
                        if(resultID != GlobalConstants.NullDataListID && !hadEx)
                        {
                            result = "<Error>Cannot fetch final DataList [ " + resultID + " ]</Error>";
                        }
                        else
                        {
                            allErrors.AddError("An internal error occured while executing.");
                            result = "<Error>"+allErrors.MakeDataListReady()+"</Error>";
                        }
                    }

                    // Clean up ;)
                    _svrCompiler.DeleteDataListByID(resultID, true);
                }
                else
                {
                    // merge data back into this DataList
                    if (dataListID != resultID)
                    {
                        TraceWriter.WriteTrace("Fatal DynamicServiceEndpoint Invoke... Failed to return input datalist!");
                        allErrors.AddError("FATAL ERROR : Execution Mis-Match");
                        result = GlobalConstants.NullDataListID.ToString();
                    }
                    else
                    {
                        result = resultID.ToString();
                    }
                }
            }

            return result;
        }

        public bool ExecuteParallel(IFrameworkActivityInstruction[] instructions)
        {
            int performed = _parallel.Perform(instructions);
            return performed == instructions.Length;
        }

        private string ExecuteCommandParallel(string xmlRequest, Guid dataListID)
        {
            return ExecuteCommand(xmlRequest, dataListID);
        }
        #endregion

        //private void ConfigurePerformanceCounters()
        //{
        //    if (!PerformanceCounterCategory.Exists("TUW Dynamic Service Framework"))
        //    {

        //        CounterCreationDataCollection counters = new CounterCreationDataCollection();

        //        CounterCreationData totalOps = new CounterCreationData();
        //        totalOps.CounterName = "# operations executed";
        //        totalOps.CounterHelp = "Total number of operations executed";
        //        totalOps.CounterType = PerformanceCounterType.NumberOfItems32;

        //        counters.Add(totalOps);

        //        // 2. counter for counting operations per second:

        //        //        PerformanceCounterType.RateOfCountsPerSecond32

        //        CounterCreationData opsPerSecond = new CounterCreationData();
        //        opsPerSecond.CounterName = "# operations / sec";
        //        opsPerSecond.CounterHelp = "Number of operations executed per second";
        //        opsPerSecond.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
        //        counters.Add(opsPerSecond);
        //        PerformanceCounterCategory.Create("TUW Dynamic Service Framework"
        //            , "Performance counters for The Unlimited World Dynamic Service Framework"
        //            , counters);
        //    }
        //}

        //private PerformanceCounter GetPerfCounter(string counterName)
        //{
        //    PerformanceCounter perf = new PerformanceCounter("TUW Dynamic Service Framework", counterName);
        //    perf.ReadOnly = false;
        //    return perf;
        //}

        #region ParallelCommandExecutor
        private sealed class ParallelCommandExecutor
        {
            private readonly DynamicServicesEndpoint _owner;
            private IFrameworkActivityInstruction[] _instructions;
            private ParameterizedThreadStart _threadStart;
            private ManualResetEvent _wait;
            private object _performGuard;
            private object _counterGuard;

            private int _length;
            private int _counter;

            public ParallelCommandExecutor(DynamicServicesEndpoint owner)
            {
                _owner = owner;
                _performGuard = new object();
                _counterGuard = new object();
                _threadStart = OnThreadStart;
                _wait = new ManualResetEvent(false);
            }

            public int Perform(IFrameworkActivityInstruction[] instructions)
            {
                int performed;

                lock (_performGuard)
                {
                    _wait.Reset();
                    _counter = 0;
                    _instructions = instructions;
                    _length = _instructions.Length;

                    int totalThreads = 6;

                    if (_length < totalThreads)
                    {
                        totalThreads = _length;
                    }

                    int batchSize = _length / totalThreads;
                    int remainder = _length - (batchSize * totalThreads);
                    int offset = 0;


                    if (_length > 0)
                    {
                        for (int i = 0; i < totalThreads - 1; i++)
                        {
                            // ReSharper disable SuggestUseVarKeywordEvident
                            ArraySegment<IFrameworkActivityInstruction> array = new ArraySegment<IFrameworkActivityInstruction>(_instructions, offset, batchSize);
                            // ReSharper restore SuggestUseVarKeywordEvident
                            offset += batchSize;
                            new Thread(_threadStart).Start(array);
                        }

                        {
                            // ReSharper disable SuggestUseVarKeywordEvident
                            ArraySegment<IFrameworkActivityInstruction> array = new ArraySegment<IFrameworkActivityInstruction>(_instructions, offset, batchSize + remainder);
                            // ReSharper restore SuggestUseVarKeywordEvident
                            //offset += batchSize;
                            new Thread(_threadStart).Start(array);
                        }

                        _wait.WaitOne();
                    }

                    performed = _counter;
                    _instructions = null;
                }

                return performed;
            }

            private void OnThreadStart(object obj)
            {
                int increment = 1;

                if (obj is ArraySegment<IFrameworkActivityInstruction>)
                {
                    ArraySegment<IFrameworkActivityInstruction> arraySegment = (ArraySegment<IFrameworkActivityInstruction>)obj;
                    increment = arraySegment.Count;
                    IFrameworkActivityInstruction[] rawArray = arraySegment.Array;

                    for (int i = 0; i < arraySegment.Count; i++)
                    {
                        IFrameworkActivityInstruction activity = rawArray[i + arraySegment.Offset];
                        activity.Result = _owner.ExecuteCommandParallel(activity.Instruction, activity.dataListID);
                    }

                }
                else
                {
                    // ReSharper disable SuggestUseVarKeywordEvident
                    IFrameworkActivityInstruction activity = obj as IFrameworkActivityInstruction;
                    // ReSharper restore SuggestUseVarKeywordEvident
                    if (activity != null)
                    {
                    activity.Result = _owner.ExecuteCommandParallel(activity.Instruction, activity.dataListID);
                }
                }


                bool resume = false;

                lock (_counterGuard)
                {
                    if ((_counter += increment) == _length)
                    {
                        resume = true;
                    }
                }

                if (resume) _wait.Set();
            }

        }
        #endregion
    }
}
