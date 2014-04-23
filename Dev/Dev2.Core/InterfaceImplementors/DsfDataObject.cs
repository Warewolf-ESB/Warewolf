using System;
using System.Activities.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices.Objects;
using Dev2.Web;

// ReSharper disable CheckNamespace
namespace Dev2.DynamicServices
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// The core TO used in the execution engine ;)
    /// </summary>
    public class DsfDataObject : PersistenceParticipant, IDSFDataObject
    {
        #region Class Members

        private string _parentServiceName = string.Empty;
        private string _parentWorkflowInstanceId = string.Empty;
        private readonly XNamespace _dSfDataObjectNs = XNamespace.Get("http://dev2.co.za/");
        private ErrorResultTO _errors;

        #endregion Class Members

        #region Constructor

        private DsfDataObject() { }

        private string ExtractValue(XElement xe, string elementName)
        {
            var tmp = xe.Descendants(elementName);
            var targetElement = tmp.FirstOrDefault();

            return targetElement != null ? targetElement.Value : string.Empty;
        }

        public DsfDataObject(string xmldata, Guid dataListID, string rawPayload = "")
        {
            ThreadsToDispose = new Dictionary<int, List<Guid>>();

            if(xmldata != null)
            {
                XElement xe = null;
                try
                {
                    xe = XElement.Parse(xmldata);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch(Exception)
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // we only trying to parse ;)
                }

                if(xe != null)
                {
                    bool isDebug;
                    var debugString = ExtractValue(xe, "IsDebug");
                    if(!string.IsNullOrEmpty(debugString))
                    {
                        bool.TryParse(debugString, out isDebug);
                    }
                    else
                    {
                        debugString = ExtractValue(xe, "BDSDebugMode");
                        bool.TryParse(debugString, out isDebug);
                    }
                    IsDebug = isDebug;

                    Guid debugSessionID;
                    Guid.TryParse(ExtractValue(xe, "DebugSessionID"), out debugSessionID);
                    DebugSessionID = debugSessionID;

                    Guid environmentID;
                    if(Guid.TryParse(ExtractValue(xe, "EnvironmentID"), out environmentID))
                    {
                        EnvironmentID = environmentID;
                    }

                    var isOnDemandSimulation = false;
                    var onDemandSimulationString = ExtractValue(xe, "IsOnDemandSimulation");
                    if(!string.IsNullOrEmpty(onDemandSimulationString))
                    {
                        bool.TryParse(onDemandSimulationString, out isOnDemandSimulation);
                    }
                    IsOnDemandSimulation = isOnDemandSimulation;

                    _parentServiceName = ExtractValue(xe, "ParentServiceName");
                    _parentWorkflowInstanceId = ExtractValue(xe, "ParentWorkflowInstanceId");

                    Guid executionCallbackID;
                    Guid.TryParse(ExtractValue(xe, "ExecutionCallbackID"), out executionCallbackID);
                    ExecutionCallbackID = executionCallbackID;

                    Guid bookmarkExecutionCallbackID;
                    Guid.TryParse(ExtractValue(xe, "BookmarkExecutionCallbackID"), out bookmarkExecutionCallbackID);
                    BookmarkExecutionCallbackID = bookmarkExecutionCallbackID;

                    if(BookmarkExecutionCallbackID == Guid.Empty && ExecutionCallbackID != Guid.Empty)
                    {
                        BookmarkExecutionCallbackID = ExecutionCallbackID;
                    }

                    Guid parentInstanceID;
                    Guid.TryParse(ExtractValue(xe, "BookmarkExecutionCallbackID"), out parentInstanceID);

                    ParentInstanceID = ExtractValue(xe, "ParentInstanceID");

                    int numberOfSteps;
                    Int32.TryParse(ExtractValue(xe, "NumberOfSteps"), out numberOfSteps);
                    NumberOfSteps = numberOfSteps;

                    CurrentBookmarkName = ExtractValue(xe, "CurrentBookmarkName");

                    Guid instID;

                    if(Guid.TryParse(ExtractValue(xe, "WorkflowInstanceId"), out instID))
                    {
                        WorkflowInstanceId = instID;
                    }


                    //
                    // Extract merge data form request
                    //
                    ExtractInMergeDataFromRequest(xe);
                    ExtractOutMergeDataFromRequest(xe);

                    // set the ID ;)
                    DataListID = dataListID;

                    // set the IsDataListScoped flag ;)
                    bool isScoped;
                    bool.TryParse(ExtractValue(xe, "IsDataListScoped"), out isScoped);
                    IsDataListScoped = isScoped;

                    // Set incoming service name ;)
                    ServiceName = ExtractValue(xe, "Service");

                    // finally set raw payload
                    RawPayload = xmldata;
                }

            }
            else
            {
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                if(xmldata == null)
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                {
                    xmldata = "NULL";
                }

                Errors.AddError("Failed to parse XML INPUT [ " + xmldata + " ]");
            }

            if(!IsDebug && !string.IsNullOrEmpty(rawPayload))
            {
                RawPayload = rawPayload;
            }

        }

        #endregion Constructor

        #region Properties

        public Guid DebugSessionID { get; set; }
        public Guid ParentID { get; set; }
        public bool RunWorkflowAsync { get; set; }
        public bool IsDebugNested { get; set; }
        public Guid ClientID { get; set; }

        public Guid EnvironmentID { get; set; }
        public bool IsRemoteWorkflow { get { return EnvironmentID != Guid.Empty; } }

        public string ParentInstanceID { get; set; }
        public Dictionary<int, List<Guid>> ThreadsToDispose { get; set; }
        public string CurrentBookmarkName { get; set; }
        public string ServiceName { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public bool IsDebug { get; set; }
        public Guid WorkspaceID { get; set; }
        public Guid OriginalInstanceID { get; set; }
        public bool IsOnDemandSimulation { get; set; }
        public Guid ServerID { get; set; }
        public Guid ResourceID { get; set; }

        public ErrorResultTO Errors
        {
            get { return _errors ?? (_errors = new ErrorResultTO()); }
            set { _errors = value; }
        }

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
        public ServiceAction ExecuteAction { get; set; }

        public string RawPayload { get; set; }
        public EmitionTypes ReturnType { get; set; }

        // Remote workflow additions ;)
        public string RemoteInvokeResultShape { get; set; }
        public bool RemoteInvoke { get; set; }
        public string RemoteInvokerID { get; set; }
        public IList<DebugState> RemoteDebugItems { get; set; }
        public string RemoteServiceType { get; set; }

        public int ParentThreadID { get; set; }

        public bool WorkflowResumeable { get; set; }

        public string ParentServiceName
        {
            get
            {
                return _parentServiceName;
            }
            set
            {
                _parentServiceName = value;
            }
        }

        public string ParentWorkflowInstanceId
        {
            get
            {

                return _parentWorkflowInstanceId ?? string.Empty;
            }
            set
            {
                _parentWorkflowInstanceId = value;
            }
        }

        public string ParentWorkflowXmlData { get; set; }

        public string DataList { get; set; }

        public ExecutionOrigin ExecutionOrigin { get; set; }
        public string ExecutionOriginDescription { get; set; }
        public bool IsFromWebServer { get; set; }


        public bool IsDataListScoped { get; set; }
        public bool ForceDeleteAtNextNativeActivityCleanup { get; set; }

        public bool IsWebpage { get; set; }

        #endregion Properties

        #region Methods
        /// <summary>
        /// Clones this instance.
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
            result.Errors = Errors;
            result.ExecutionCallbackID = ExecutionCallbackID;
            result.ExecutionOrigin = ExecutionOrigin;
            result.ExecutionOriginDescription = ExecutionOriginDescription;
            result.ForceDeleteAtNextNativeActivityCleanup = ForceDeleteAtNextNativeActivityCleanup;
            result.IsDataListScoped = IsDataListScoped;
            result.IsDebug = IsDebug;
            result.IsOnDemandSimulation = IsOnDemandSimulation;
            result.IsFromWebServer = IsFromWebServer;
            result.IsWebpage = IsWebpage;
            result.NumberOfSteps = NumberOfSteps;
            result.OriginalInstanceID = OriginalInstanceID;
            result.ParentInstanceID = ParentInstanceID;
            result.ParentServiceName = ParentServiceName;
            result.ParentThreadID = ParentThreadID;
            result.ParentWorkflowInstanceId = ParentWorkflowInstanceId;
            result.RawPayload = RawPayload;
            result.RemoteDebugItems = RemoteDebugItems;
            result.RemoteInvoke = RemoteInvoke;
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
            result.RunWorkflowAsync = RunWorkflowAsync;
            result.IsDebugNested = IsDebugNested;
            return result;
        }

        public bool IsDebugMode()
        {
            return (IsDebug || ServerLogger.ShouldLog(ResourceID) || RemoteInvoke) && !RunWorkflowAsync;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// A host invokes this method on a custom persistence participant to collect read-write values and write-only values, to be persisted.
        /// </summary>
        /// <param name="readWriteValues">The read-write values to be persisted.</param>
        /// <param name="writeOnlyValues">The write-only values to be persisted.</param>
        protected override void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
        {
            // Sashen: 05-07-2012
            // These methods are used on completion of any workflow application instance
            // The Data Transfer object will need to be serialized to the persistence store
            // as we require the dataTransfer object once the workflow resumes as there may be parent workflows to bring out
            // of perstistence, so...

            // We serialize the DataTransfer object into the persistance store XAML  

            readWriteValues = new Dictionary<XName, object>();

            foreach(PropertyInfo pi in typeof(IDSFDataObject).GetProperties())
            {
                readWriteValues.Add(_dSfDataObjectNs.GetName(pi.Name).LocalName, pi.GetValue(this, null));
            }

            writeOnlyValues = null;
        }



        /// <summary>
        /// The host invokes this method and passes all the loaded values in the collection (filled by the or ) as a dictionary parameter.
        /// </summary>
        /// <param name="readWriteValues">The read-write values that were loaded from the persistence store. This dictionary corresponds to the dictionary of read-write values persisted in the most recent persistence episode.</param>
        protected override void PublishValues(IDictionary<XName, object> readWriteValues)
        {
            foreach(XName key in readWriteValues.Keys)
            {
                PropertyInfo pi = typeof(IDSFDataObject).GetProperty(key.LocalName);

                if(pi != null)
                {
                    pi.SetValue(this, readWriteValues[key], null);
                }
            }
        }

        #endregion Override Methods

        #region Private Methods

        private void ExtractOutMergeDataFromRequest(XElement xe)
        {
            Guid datalistOutMergeID;
            Guid.TryParse(ExtractValue(xe, "DatalistOutMergeID"), out datalistOutMergeID);
            DatalistOutMergeID = datalistOutMergeID;

            enDataListMergeTypes datalistOutMergeType;
            if(Enum.TryParse(ExtractValue(xe, "DatalistOutMergeType"), true, out datalistOutMergeType))
            {
                DatalistOutMergeType = datalistOutMergeType;
            }
            else
            {
                DatalistOutMergeType = enDataListMergeTypes.Intersection;
            }

            enTranslationDepth datalistOutMergeDepth;
            if(Enum.TryParse(ExtractValue(xe, "DatalistOutMergeDepth"), true, out datalistOutMergeDepth))
            {
                DatalistOutMergeDepth = datalistOutMergeDepth;
            }
            else
            {
                DatalistOutMergeDepth = enTranslationDepth.Data_With_Blank_OverWrite;
            }

            DataListMergeFrequency datalistOutMergeFrequency;
            if(Enum.TryParse(ExtractValue(xe, "DatalistOutMergeFrequency"), true, out datalistOutMergeFrequency))
            {
                DatalistOutMergeFrequency = datalistOutMergeFrequency;
            }
            else
            {
                DatalistOutMergeFrequency = DataListMergeFrequency.OnCompletion;
            }
        }

        private void ExtractInMergeDataFromRequest(XElement xe)
        {
            Guid datalistInMergeID;
            Guid.TryParse(ExtractValue(xe, "DatalistInMergeID"), out datalistInMergeID);
            DatalistInMergeID = datalistInMergeID;

            enDataListMergeTypes datalistInMergeType;
            if(Enum.TryParse(ExtractValue(xe, "DatalistInMergeType"), true, out datalistInMergeType))
            {
                DatalistInMergeType = datalistInMergeType;
            }
            else
            {
                DatalistInMergeType = enDataListMergeTypes.Intersection;
            }

            enTranslationDepth datalistInMergeDepth;
            if(Enum.TryParse(ExtractValue(xe, "DatalistInMergeDepth"), true, out datalistInMergeDepth))
            {
                DatalistInMergeDepth = datalistInMergeDepth;
            }
            else
            {
                DatalistInMergeDepth = enTranslationDepth.Data_With_Blank_OverWrite;
            }
        }

        #endregion Private Methods
    }
}
