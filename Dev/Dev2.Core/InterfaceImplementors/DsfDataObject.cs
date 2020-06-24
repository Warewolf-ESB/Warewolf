/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Persistence;
using System.Collections.Concurrent;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Xml.Linq;

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.Interfaces.Enums;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Web;
using Warewolf.Auditing;
using Warewolf.Data;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.DynamicServices
{
    /// <summary>
    ///     The core TO used in the execution engine ;)
    /// </summary>
    public class DsfDataObject : PersistenceParticipant, IDSFDataObject
    {
        #region Class Members

        readonly XNamespace _dSfDataObjectNs = XNamespace.Get("http://dev2.co.za/");
        string _parentWorkflowInstanceId = string.Empty;
        readonly ConcurrentStack<IExecutionEnvironment> _environments;
        #endregion Class Members

        #region Constructor

        DsfDataObject()
        {
            Environment = new ExecutionEnvironment();
            _environments = new ConcurrentStack<IExecutionEnvironment>();
        }

        public DsfDataObject(string xmldata, Guid dataListId)
            : this(xmldata, dataListId, "")
        {
        }

        public DsfDataObject(string xmldata, Guid dataListId, string rawPayload)
        {
            Environment = new ExecutionEnvironment();
            _environments = new ConcurrentStack<IExecutionEnvironment>();
            ThreadsToDispose = new Dictionary<int, List<Guid>>();
            AuthCache = new ConcurrentDictionary<(IPrincipal, AuthorizationContext, string), bool>();

            if (xmldata != null)
            {
                XElement xe = null;
                try
                {
                    if (!string.IsNullOrEmpty(xmldata))
                    {
                        xe = XElement.Parse(xmldata);
                    }
                }
                catch (Exception)
                {
                    // try parse ;)
                }

                if (xe != null)
                {
                    ExtractXmlValues(xe);

                    // set the ID ;)
                    DataListID = dataListId;

                    // finally set raw payload
                    RawPayload = new StringBuilder(xmldata);
                }
            }

            if (!IsDebug && !string.IsNullOrEmpty(rawPayload))
            {
                RawPayload = new StringBuilder(rawPayload);
            }
        }

        void ExtractXmlValues(XElement xe)
        {
            bool isDebug;
            var debugString = ExtractValue(xe, "IsDebug");
            if (!string.IsNullOrEmpty(debugString))
            {
                bool.TryParse(debugString, out isDebug);
            }
            else
            {
                debugString = ExtractValue(xe, "BDSDebugMode");
                bool.TryParse(debugString, out isDebug);
            }
            IsDebug = isDebug;

            Int32.TryParse(ExtractValue(xe, "VersionNumber"), out int versionNumber);
            VersionNumber = versionNumber;


            Guid.TryParse(ExtractValue(xe, "DebugSessionID"), out Guid debugSessionId);
            DebugSessionID = debugSessionId;

            if (Guid.TryParse(ExtractValue(xe, "EnvironmentID"), out Guid environmentId))
            {
                EnvironmentID = environmentId;
                DebugEnvironmentId = environmentId;
            }

            var isOnDemandSimulation = false;
            var onDemandSimulationString = ExtractValue(xe, "IsOnDemandSimulation");
            if (!string.IsNullOrEmpty(onDemandSimulationString))
            {
                bool.TryParse(onDemandSimulationString, out isOnDemandSimulation);
            }
            IsOnDemandSimulation = isOnDemandSimulation;

            ParentServiceName = ExtractValue(xe, "ParentServiceName");
            _parentWorkflowInstanceId = ExtractValue(xe, "ParentWorkflowInstanceId");

            Guid.TryParse(ExtractValue(xe, "ExecutionCallbackID"), out Guid executionCallbackId);
            ExecutionCallbackID = executionCallbackId;

            Guid.TryParse(ExtractValue(xe, "BookmarkExecutionCallbackID"), out Guid bookmarkExecutionCallbackId);
            BookmarkExecutionCallbackID = bookmarkExecutionCallbackId;

            if (BookmarkExecutionCallbackID == Guid.Empty && ExecutionCallbackID != Guid.Empty)
            {
                BookmarkExecutionCallbackID = ExecutionCallbackID;
            }

            Guid.TryParse(ExtractValue(xe, "BookmarkExecutionCallbackID"), out var _);

            ParentInstanceID = ExtractValue(xe, "ParentInstanceID");

            Int32.TryParse(ExtractValue(xe, "NumberOfSteps"), out int numberOfSteps);
            NumberOfSteps = numberOfSteps;

            CurrentBookmarkName = ExtractValue(xe, "CurrentBookmarkName");

            if (Guid.TryParse(ExtractValue(xe, "WorkflowInstanceId"), out Guid instId))
            {
                WorkflowInstanceId = instId;
            }

            //
            // Extract merge data form request
            //
            ExtractInMergeDataFromRequest(xe);
            ExtractOutMergeDataFromRequest(xe);

            // set the IsDataListScoped flag ;)
            bool.TryParse(ExtractValue(xe, "IsDataListScoped"), out bool isScoped);
            IsDataListScoped = isScoped;

            // Set incoming service name ;)
            ServiceName = ExtractValue(xe, "Service");
        }

        public Guid DebugEnvironmentId { get; set; }

        string ExtractValue(XElement xe, string elementName)
        {
            var tmp = xe.Descendants(elementName);
            var targetElement = tmp.FirstOrDefault();

            return targetElement?.Value ?? string.Empty;
        }

        #endregion Constructor

        #region Properties

        StringBuilder _rawPayload;
        public ServiceAction ExecuteAction { get; set; }
        public string ParentWorkflowXmlData { get; set; }
        public Guid DebugSessionID { get; set; }
        public Guid ParentID { get; set; }
        public int VersionNumber { get; set; }
        public bool RunWorkflowAsync { get; set; }
        public bool IsDebugNested { get; set; }
        public Guid[] TestsResourceIds { get; set; } = new Guid[] { };
        public bool IsSubExecution { get; set; }
        public string QueryString { get; set; }
        public bool IsRemoteInvoke => EnvironmentID != Guid.Empty;

        public bool IsRemoteInvokeOverridden { get; set; }

        bool IDSFDataObject.IsRemoteWorkflow()
        {
            if (!IsRemoteInvokeOverridden)
            {
                return IsRemoteInvoke;
            }

            return false;
        }

        public IExecutionEnvironment Environment { get; set; }
        public IExecutionToken ExecutionToken { get; set; }

        public void PopEnvironment()
        {
            var tryPop = _environments.TryPop(out IExecutionEnvironment localEnv);
            if (tryPop)
            {
                Environment = localEnv;
            }
        }

        public void PushEnvironment(IExecutionEnvironment env)
        {
            _environments.Push(Environment);
            Environment = env;
        }

        public IEsbChannel EsbChannel { get; set; }
        public DateTime StartTime { get; set; }
        public int ForEachUpdateValue { get; set; }

        public int ForEachNestingLevel { get; set; }

        public Guid ClientID { get; set; }

        public Guid EnvironmentID { get; set; }

        public string ParentInstanceID { get; set; }
        public Dictionary<int, List<Guid>> ThreadsToDispose { get; set; }
        public string CurrentBookmarkName { get; set; }
        public string ServiceName { get; set; }
        public string TestName { get; set; }
        public bool IsServiceTestExecution { get; set; }
        public bool IsDebugFromWeb { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public bool IsDebug { get; set; }
        public Guid WorkspaceID { get; set; }
        public Guid SourceResourceID { get; set; }
        public Guid OriginalInstanceID { get; set; }
        public bool IsOnDemandSimulation { get; set; }
        public Guid ServerID { get; set; }
        public Guid ResourceID { get; set; }

        public IWarewolfResource Resource { get; set; }



        public int NumberOfSteps { get; set; }
        public IPrincipal ExecutingUser { get; set; }
        public Guid DatalistOutMergeID { get; set; }
        public enTranslationDepth DatalistOutMergeDepth { get; set; }
        public DataListMergeFrequency DatalistOutMergeFrequency { get; set; }
        public enDataListMergeTypes DatalistOutMergeType { get; set; }

        public Guid DatalistInMergeID { get; set; }
        public enTranslationDepth DatalistInMergeDepth { get; set; }
        public enDataListMergeTypes DatalistInMergeType { get; set; }

        public Guid ExecutionCallbackID { get; set; }
        public Guid BookmarkExecutionCallbackID { get; set; }

        public Guid DataListID { get; set; }

        public StringBuilder RawPayload
        {
            get { return _rawPayload ?? new StringBuilder(); }
            set { _rawPayload = value; }
        }

        public EmitionTypes ReturnType { get; set; }

        // Remote workflow additions ;)
        public StringBuilder RemoteInvokeResultShape { get; set; }
        public bool RemoteInvoke { get; set; }
        public string RemoteInvokerID { get; set; }
        public IList<IDebugState> RemoteDebugItems { get; set; }
        public string RemoteServiceType { get; set; }

        public int ParentThreadID { get; set; }

        public bool WorkflowResumeable { get; set; }

        public string ParentServiceName { get; set; } = string.Empty;

        public string ParentWorkflowInstanceId
        {
            get { return _parentWorkflowInstanceId ?? string.Empty; }
            set { _parentWorkflowInstanceId = value; }
        }

        public StringBuilder DataList { get; set; }

        public ExecutionOrigin ExecutionOrigin { get; set; }
        public string ExecutionOriginDescription { get; set; }
        public bool IsFromWebServer { get; set; }


        public bool IsDataListScoped { get; set; }
        public bool ForceDeleteAtNextNativeActivityCleanup { get; set; }
        public bool RemoteNonDebugInvoke { get; set; }
        public bool StopExecution { get; set; }
        public IServiceTestModelTO ServiceTest { get; set; }

        public Guid? ExecutionID { get; set; }
        public string CustomTransactionID { get; set; } = "";
        public string WebUrl { get; set; }
        public IStateNotifier StateNotifier { get; set; }
        public IDev2WorkflowSettings Settings { get; set; }
        public ConcurrentDictionary<(IPrincipal, AuthorizationContext, string), bool> AuthCache { get; set; }
        public Exception ExecutionException { get; set; }
        public IDictionary<IDev2Activity, (RetryState, IEnumerator<bool>)> Gates { get; } = new Dictionary<IDev2Activity, (RetryState, IEnumerator<bool>)>();
        public string OriginalServiceName { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns></returns>
        public IDSFDataObject Clone()
        {
            IDSFDataObject result = new DsfDataObject();
            result.BookmarkExecutionCallbackID = BookmarkExecutionCallbackID;
            result.CurrentBookmarkName = CurrentBookmarkName;
            result.DebugSessionID = DebugSessionID;
            result.DataList = DataList;
            result.DataListID = DataListID;
            result.DatalistOutMergeDepth = DatalistOutMergeDepth;
            result.DatalistOutMergeFrequency = DatalistOutMergeFrequency;
            result.DatalistOutMergeID = DatalistOutMergeID;
            result.DatalistOutMergeType = DatalistOutMergeType;
            result.DatalistInMergeDepth = DatalistInMergeDepth;
            result.DatalistInMergeID = DatalistInMergeID;
            result.DatalistInMergeType = DatalistInMergeType;
            result.EnvironmentID = EnvironmentID;
            result.DebugEnvironmentId = DebugEnvironmentId;
            result.ExecutionCallbackID = ExecutionCallbackID;
            result.ExecutionOrigin = ExecutionOrigin;
            result.ExecutionOriginDescription = ExecutionOriginDescription;
            result.ForceDeleteAtNextNativeActivityCleanup = ForceDeleteAtNextNativeActivityCleanup;
            result.IsDataListScoped = IsDataListScoped;
            result.IsDebug = IsDebug;
            result.IsOnDemandSimulation = IsOnDemandSimulation;
            result.IsFromWebServer = IsFromWebServer;
            result.NumberOfSteps = NumberOfSteps;
            result.OriginalInstanceID = OriginalInstanceID;
            result.ParentInstanceID = ParentInstanceID;
            result.ParentServiceName = ParentServiceName;
            result.ParentThreadID = ParentThreadID;
            result.ParentWorkflowInstanceId = ParentWorkflowInstanceId;
            result.RawPayload = RawPayload;
            result.RemoteDebugItems = RemoteDebugItems;
            result.RemoteInvoke = RemoteInvoke;
            result.RemoteNonDebugInvoke = RemoteNonDebugInvoke;
            result.IsRemoteInvokeOverridden = IsRemoteInvokeOverridden;
            result.RemoteInvokeResultShape = RemoteInvokeResultShape;
            result.RemoteInvokerID = RemoteInvokerID;
            result.RemoteServiceType = RemoteServiceType;
            result.ResourceID = ResourceID;
            result.ReturnType = ReturnType;
            result.ServerID = ServerID;
            result.ClientID = ClientID;
            result.ServiceName = ServiceName;
            result.WorkflowInstanceId = WorkflowInstanceId;
            result.WorkflowResumeable = WorkflowResumeable;
            result.WorkspaceID = WorkspaceID;
            result.ThreadsToDispose = ThreadsToDispose;
            result.ParentID = ParentID;
            result.VersionNumber = VersionNumber;
            result.RunWorkflowAsync = RunWorkflowAsync;
            result.IsDebugNested = IsDebugNested;
            result.ForEachNestingLevel = ForEachNestingLevel;
            result.Environment = Environment;
            result.EsbChannel = EsbChannel;
            result.ExecutionToken = ExecutionToken;
            result.ForEachUpdateValue = ForEachUpdateValue;
            result.TestName = TestName;
            result.SourceResourceID = SourceResourceID;
            result.IsServiceTestExecution = IsServiceTestExecution;
            result.IsDebugFromWeb = IsDebugFromWeb;
            result.ExecutionID = ExecutionID;
            result.CustomTransactionID = CustomTransactionID;
            result.WebUrl = WebUrl;
            result.IsSubExecution = IsSubExecution;
            result.QueryString = QueryString;
            result.ExecutingUser = ExecutingUser;
            result.StateNotifier = StateNotifier;
            result.AuthCache = new ConcurrentDictionary<(IPrincipal, AuthorizationContext, string), bool>(AuthCache);
            result.ExecutionException = ExecutionException;
            result.OriginalServiceName = OriginalServiceName;

            var serializer = new Dev2JsonSerializer();
            if (ServiceTest != null)
            {
                var json = serializer.Serialize(ServiceTest);
                result.ServiceTest = serializer.Deserialize<IServiceTestModelTO>(json);
            }
            if (Settings != null)
            {
                var json = serializer.Serialize(Settings);
                result.Settings = serializer.Deserialize<IDev2WorkflowSettings>(json);
            }
            return result;
        }

        // TODO: move WorkflowLogger.ShouldLog configuration to Config.ServerSettings? This does not seem to be used
        public bool IsDebugMode() => (IsDebug || RemoteInvoke) && !RunWorkflowAsync;

        #endregion

        #region Override Methods

        /// <summary>
        ///     A host invokes this method on a custom persistence participant to collect read-write values and write-only values,
        ///     to be persisted.
        /// </summary>
        /// <param name="readWriteValues">The read-write values to be persisted.</param>
        /// <param name="writeOnlyValues">The write-only values to be persisted.</param>
        protected override void CollectValues(out IDictionary<XName, object> readWriteValues,
            out IDictionary<XName, object> writeOnlyValues)
        {
            // Sashen: 05-07-2012
            // These methods are used on completion of any workflow application instance
            // The Data Transfer object will need to be serialized to the persistence store
            // as we require the dataTransfer object once the workflow resumes as there may be parent workflows to bring out
            // of perstistence, so...

            // We serialize the DataTransfer object into the persistance store XAML  

            readWriteValues = new Dictionary<XName, object>();

            foreach (PropertyInfo pi in typeof(IDSFDataObject).GetProperties())
            {
                readWriteValues.Add(_dSfDataObjectNs.GetName(pi.Name).LocalName, pi.GetValue(this, null));
            }

            writeOnlyValues = null;
        }


        /// <summary>
        ///     The host invokes this method and passes all the loaded values in the collection (filled by the or ) as a dictionary
        ///     parameter.
        /// </summary>
        /// <param name="readWriteValues">
        ///     The read-write values that were loaded from the persistence store. This dictionary
        ///     corresponds to the dictionary of read-write values persisted in the most recent persistence episode.
        /// </param>
        protected override void PublishValues(IDictionary<XName, object> readWriteValues)
        {
            foreach (XName key in readWriteValues.Keys)
            {
                var pi = typeof(IDSFDataObject).GetProperty(key.LocalName);

                if (pi != null)
                {
                    pi.SetValue(this, readWriteValues[key], null);
                }
            }
        }

        #endregion Override Methods

        #region Private Methods

        void ExtractOutMergeDataFromRequest(XElement xe)
        {
            Guid.TryParse(ExtractValue(xe, "DatalistOutMergeID"), out Guid datalistOutMergeId);
            DatalistOutMergeID = datalistOutMergeId;


            DatalistOutMergeType = Enum.TryParse(ExtractValue(xe, "DatalistOutMergeType"), true, out enDataListMergeTypes datalistOutMergeType) ? datalistOutMergeType : enDataListMergeTypes.Intersection;

            DatalistOutMergeDepth = Enum.TryParse(ExtractValue(xe, "DatalistOutMergeDepth"), true,
                out enTranslationDepth datalistOutMergeDepth)
                ? datalistOutMergeDepth
                : enTranslationDepth.Data_With_Blank_OverWrite;

            DatalistOutMergeFrequency = Enum.TryParse(ExtractValue(xe, "DatalistOutMergeFrequency"), true,
                out DataListMergeFrequency datalistOutMergeFrequency)
                ? datalistOutMergeFrequency
                : DataListMergeFrequency.OnCompletion;
        }

        void ExtractInMergeDataFromRequest(XElement xe)
        {
            Guid.TryParse(ExtractValue(xe, "DatalistInMergeID"), out Guid datalistInMergeId);
            DatalistInMergeID = datalistInMergeId;

            DatalistInMergeType = Enum.TryParse(ExtractValue(xe, "DatalistInMergeType"), true, out enDataListMergeTypes datalistInMergeType)
                ? datalistInMergeType
                : enDataListMergeTypes.Intersection;

            DatalistInMergeDepth = Enum.TryParse(ExtractValue(xe, "DatalistInMergeDepth"), true,
                out enTranslationDepth datalistInMergeDepth)
                ? datalistInMergeDepth
                : enTranslationDepth.Data_With_Blank_OverWrite;
        }

        #endregion Private Methods
    }
}
