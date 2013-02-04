using System;
using System.Activities.Persistence;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Dev2.DataList.Contract;
using Unlimited.Framework;

namespace Dev2.DynamicServices
{
    public class DsfDataObject : PersistenceParticipant, IDSFDataObject
    {
        #region Class Members

        private string _parentServiceName = string.Empty;
        private string _parentWorkflowInstanceId = string.Empty;
        private bool _workflowResumeable;
        private List<object> items = new List<object>();
        private XNamespace _dSFDataObjectNS = XNamespace.Get("http://dev2.co.za/");

        #endregion Class Members

        #region Constructor

        private DsfDataObject() { }

        public DsfDataObject(string xmldata, Guid dataListID)
        {
            if(!string.IsNullOrEmpty(xmldata))
            {
                dynamic dataObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(xmldata);

                bool isDebug;
                var debugString = dataObject.GetValue("IsDebug") as string;
                if(!string.IsNullOrEmpty(debugString))
                {
                    bool.TryParse(debugString, out isDebug);
                }
                else
                {
                    bool.TryParse(dataObject.GetValue("BDSDebugMode"), out isDebug);
                }
                IsDebug = isDebug;

                var isOnDemandSimulation = false;
                var onDemandSimulationString = dataObject.GetValue("IsOnDemandSimulation") as string;
                if(!string.IsNullOrEmpty(onDemandSimulationString))
                {
                    bool.TryParse(onDemandSimulationString, out isOnDemandSimulation);
                }               
                IsOnDemandSimulation = isOnDemandSimulation;

                _parentWorkflowInstanceId = dataObject.GetValue("ParentWorkflowInstanceId");

                Guid executionCallbackID;
                Guid.TryParse(dataObject.GetValue("ExecutionCallbackID"), out executionCallbackID);
                ExecutionCallbackID = executionCallbackID;

                Guid bookmarkExecutionCallbackID;
                Guid.TryParse(dataObject.GetValue("BookmarkExecutionCallbackID"), out bookmarkExecutionCallbackID);
                BookmarkExecutionCallbackID = bookmarkExecutionCallbackID;

                if (BookmarkExecutionCallbackID == Guid.Empty && ExecutionCallbackID != Guid.Empty)
                {
                    BookmarkExecutionCallbackID = ExecutionCallbackID;
                }

                ParentInstanceID = dataObject.GetValue("ParentInstanceID");
                ParentServiceName = dataObject.GetValue("ParentServiceName");

                if(dataObject.Bookmark is string)
                {
                    Bookmark = dataObject.Bookmark;
                }
                if(dataObject.InstanceId is string)
                {

                    Guid instID;

                    if(Guid.TryParse(dataObject.InstanceId, out instID))
                    {
                        InstanceID = instID;
                    }
                }

                //
                // Extract merge data form request
                //
                ExtractInMergeDataFromRequest(dataObject);
                ExtractOutMergeDataFromRequest(dataObject);

                // set the ID ;)
                DataListID = dataListID;

                // set the IsDataListScoped flag ;)
                bool isScoped;
                bool.TryParse(dataObject.GetValue("IsDataListScoped"), out isScoped);
                IsDataListScoped = isScoped;
            }
        }

        #endregion Constructor

        #region Properties

        public string ParentInstanceID { get; set; }
        public string CurrentBookmarkName { get; set; }
        public string ServiceName { get; set; }
        public string WorkflowInstanceId { get; set; }
        public bool IsDebug { get; set; }
        public Guid WorkspaceID { get; set; }
        public bool IsOnDemandSimulation { get; set; }
        public Guid ServerID { get; set; }

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
        public string Bookmark { get; set; }
        public Guid InstanceID { get; set; }


        public bool WorkflowResumeable
        {
            get
            {
                return _workflowResumeable;
            }
            set
            {
                _workflowResumeable = value;
            }
        }

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

                return _parentWorkflowInstanceId == null ? string.Empty : _parentWorkflowInstanceId.ToString();
            }
            set
            {
                _parentWorkflowInstanceId = value;
            }
        }

        public string ParentWorkflowXmlData { get; set; }

        public string DataList { get; set; }

        public bool IsDataListScoped { get; set; }
        public bool ForceDeleteAtNextNativeActivityCleanup { get; set; }

        #endregion Properties

        #region Methods
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public IDSFDataObject Clone()
        {
            IDSFDataObject result = new DsfDataObject();

            result.CurrentBookmarkName = this.CurrentBookmarkName;
            result.DataList = this.DataList;
            result.DataListID = this.DataListID;
            result.DatalistOutMergeDepth = this.DatalistOutMergeDepth;
            result.DatalistOutMergeFrequency = this.DatalistOutMergeFrequency;
            result.DatalistOutMergeID = this.DatalistOutMergeID;
            result.DatalistOutMergeType = this.DatalistOutMergeType;
            result.DatalistInMergeDepth = this.DatalistInMergeDepth;
            result.DatalistInMergeID = this.DatalistInMergeID;
            result.DatalistInMergeType = this.DatalistInMergeType;
            result.ExecutionCallbackID = this.ExecutionCallbackID;
            result.BookmarkExecutionCallbackID = this.BookmarkExecutionCallbackID;
            result.IsDebug = this.IsDebug;
            result.ParentServiceName = this.ParentServiceName;
            result.ParentWorkflowInstanceId = this.ParentWorkflowInstanceId;
            result.ServiceName = this.ServiceName;
            result.WorkflowInstanceId = this.WorkflowInstanceId;
            result.WorkflowResumeable = this.WorkflowResumeable;
            result.WorkspaceID = this.WorkspaceID;
            result.IsDataListScoped = this.IsDataListScoped;

            return result;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// A host invokes this method on a custom persistence participant to collect read-write values and write-only values, to be persisted.
        /// </summary>
        /// <param name="readWriteValues">The read-write values to be persisted.</param>
        /// <param name="writeOnlyValues">The write-only values to be persisted.</param>
        protected override void CollectValues(out IDictionary<System.Xml.Linq.XName, object> readWriteValues, out IDictionary<System.Xml.Linq.XName, object> writeOnlyValues)
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
                readWriteValues.Add(_dSFDataObjectNS.GetName(pi.Name).LocalName, pi.GetValue(this, null));
            }

            writeOnlyValues = null;
        }

        /// <summary>
        /// A host invokes this method after it is done with collecting the values in the first stage. The host forwards two read-only dictionaries of values it collected from all persistence participants during the first stage (CollectValues stage) to this method for mapping. The host adds values in the dictionary returned by this method to the collection of write-only values.
        /// </summary>
        /// <param name="readWriteValues">The read-write values to be persisted.</param>
        /// <param name="writeOnlyValues">The write-only values to be persisted.</param>
        /// <returns>
        /// A dictionary containing additional write-only values to be persisted.
        /// </returns>
        protected override IDictionary<XName, object> MapValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
        {
            return base.MapValues(readWriteValues, writeOnlyValues);
        }

        // We deserialize the persistance information and rehydrate the Data Transfer object

        /// <summary>
        /// The host invokes this method and passes all the loaded values in the <see cref="P:System.Activities.Persistence.SaveWorkflowCommand.InstanceData" /> collection (filled by the <see cref="T:System.Activities.Persistence.LoadWorkflowCommand" /> or <see cref="T:System.Activities.Persistence.LoadWorkflowByInstanceKeyCommand" />) as a dictionary parameter.
        /// </summary>
        /// <param name="readWriteValues">The read-write values that were loaded from the persistence store. This dictionary corresponds to the dictionary of read-write values persisted in the most recent persistence episode.</param>
        protected override void PublishValues(IDictionary<System.Xml.Linq.XName, object> readWriteValues)
        {
            foreach (System.Xml.Linq.XName key in readWriteValues.Keys)
            {
                PropertyInfo pi = typeof(IDSFDataObject).GetProperty(key.LocalName);

                if (pi != null)
                {
                    pi.SetValue(this, readWriteValues[key], null);
                }
            }
        }

        #endregion Override Methods

        #region Private Methods

        private void ExtractOutMergeDataFromRequest(dynamic dataObject)
        {
            Guid datalistOutMergeID;
            Guid.TryParse(dataObject.GetValue("DatalistOutMergeID"), out datalistOutMergeID);
            DatalistOutMergeID = datalistOutMergeID;

            enDataListMergeTypes datalistOutMergeType;
            if (Enum.TryParse<enDataListMergeTypes>(dataObject.GetValue("DatalistOutMergeType"), true, out datalistOutMergeType))
            {
                DatalistOutMergeType = datalistOutMergeType;
            }
            else
            {
                DatalistOutMergeType = enDataListMergeTypes.Intersection;
            }

            enTranslationDepth datalistOutMergeDepth;
            if (Enum.TryParse<enTranslationDepth>(dataObject.GetValue("DatalistOutMergeDepth"), true, out datalistOutMergeDepth))
            {
                DatalistOutMergeDepth = datalistOutMergeDepth;
            }
            else
            {
                DatalistOutMergeDepth = enTranslationDepth.Data_With_Blank_OverWrite;
            }

            DataListMergeFrequency datalistOutMergeFrequency;
            if (Enum.TryParse<DataListMergeFrequency>(dataObject.GetValue("DatalistOutMergeFrequency"), true, out datalistOutMergeFrequency))
            {
                DatalistOutMergeFrequency = datalistOutMergeFrequency;
            }
            else
            {
                DatalistOutMergeFrequency = DataListMergeFrequency.OnCompletion;
            }
        }

        private void ExtractInMergeDataFromRequest(dynamic dataObject)
        {
            Guid datalistInMergeID;
            Guid.TryParse(dataObject.GetValue("DatalistInMergeID"), out datalistInMergeID);
            DatalistInMergeID = datalistInMergeID;

            enDataListMergeTypes datalistInMergeType;
            if (Enum.TryParse<enDataListMergeTypes>(dataObject.GetValue("DatalistInMergeType"), true, out datalistInMergeType))
            {
                DatalistInMergeType = datalistInMergeType;
            }
            else
            {
                DatalistInMergeType = enDataListMergeTypes.Intersection;
            }

            enTranslationDepth datalistInMergeDepth;
            if (Enum.TryParse<enTranslationDepth>(dataObject.GetValue("DatalistInMergeDepth"), true, out datalistInMergeDepth))
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
