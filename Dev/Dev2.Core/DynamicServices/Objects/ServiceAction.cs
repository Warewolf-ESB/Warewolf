using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Util;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.DynamicServices {

    #region Service Action Class - Represents a single action within a service
    /// <summary>
    /// Represents a service action which is the single unit of work that can occur
    /// in a dynamic service. This could be stored procedure invocation in a 
    /// data store or a webservice call or a static Service Method invocation
    /// that will occur locally on the service
    /// </summary>
    public class ServiceAction : DynamicServiceObjectBase {
        #region Private Fields
        private bool _resultsToClient = true;
        private bool _terminateServiceOnFault = true;
        private int _commandTimeout = 30;
        private Activity _workflowActivity;
        private string _xamlDefinition;

        private object _poolGuard = new object();
        private int _generation;
        private Queue<PooledServiceActivity> _workflowPool = new Queue<PooledServiceActivity>();

        private MemoryStream _xamlStream;
        #endregion

        #region Public Properties
        public int CommandTimeout {
            get {
                return _commandTimeout;
            }
            set {
                _commandTimeout = value;

            }

        }

        public MemoryStream XamlStream { get { return _xamlStream; } }

        /// <summary>
        /// The type of action that this action will invoke
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enActionType ActionType { get; set; }
        /// <summary>
        /// The name of the data source - maps to a source in the sources list of the service directory
        /// </summary>
        public string SourceName { get; set; }
        /// <summary>
        /// The method to invoke at the data source
        /// </summary>
        public string SourceMethod { get; set; }
        /// <summary>
        /// The instance of the source itself
        /// </summary>
        public Source Source { get; set; }
        /// <summary>
        /// The xaml definition of the workflow that will be hosted
        /// </summary>
        public string XamlDefinition
        {
            get
            {
                return _xamlDefinition;
            }
            set
            {
                lock (_poolGuard)
                {
                    // Default operation
                    _xamlDefinition = value;

                    // Travis.Frisinger : 13.11.2012 - Remove bad namespaces
                    if (GlobalConstants.runtimeNamespaceClean) {

                        _xamlDefinition = Dev2XamlCleaner.CleanServiceDef(_xamlDefinition);
                    } 
                    // End Mods

                    if (string.IsNullOrEmpty(value))
                    {
                        throw new ArgumentNullException("XamlDefinition");
                    }

                    _workflowActivity = ActivityXamlServices.Load(_xamlStream = new MemoryStream(Encoding.UTF8.GetBytes(XamlDefinition)));
                    _xamlStream.Seek(0, SeekOrigin.Begin);
                    _workflowPool.Clear();

                    _generation++;

                    for (int i = 0; i < GlobalConstants._xamlPoolSize; i++)
                    {
                        Activity activity = ActivityXamlServices.Load(_xamlStream);
                        _xamlStream.Seek(0, SeekOrigin.Begin);
                        _workflowPool.Enqueue(new PooledServiceActivity(_generation, activity));
                    }
                }
            }

        }
        /// <summary>
        /// The activity implementation created from the workflow xaml
        /// </summary>
        public Activity WorkflowActivity {
            get {
                return _workflowActivity;
            }
        }
        /// <summary>
        /// The name of the bizrule from the bizrule list of the service directory that this
        /// action represents
        /// </summary>
        public string BizRuleName { get; set; }
        /// <summary>
        /// The instance of the bizrule class itself
        /// </summary>
        public BizRule BizRule { get; set; }
        /// <summary>
        /// The inputs for this service action
        /// </summary>
        public List<ServiceActionInput> ServiceActionInputs { get; set; }
        /// <summary>
        /// The name of the service that will be invoked if this is an InvokeDynamicService type service action
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// The instance of the service that will be invoked
        /// </summary>
        public DynamicService Service { get; set; }
        /// <summary>
        /// Represents whether the results of this service action will be returned to the data consumer
        /// </summary>
        public bool ResultsToClient { get { return _resultsToClient; } set { _resultsToClient = value; } }
        /// <summary>
        /// Represents whether the service will terminate if this action faults i.e has errors
        /// </summary>
        public bool TerminateServiceOnFault { get { return _terminateServiceOnFault; } set { _terminateServiceOnFault = value; } }
        /// <summary>
        /// Stores the parent of the current Service Action
        /// Applicable only if the Service Action Type is Switch
        /// </summary>
        public dynamic Parent { get; set; }
        /// <summary>
        /// Stores the cases to be evaluated against this action.
        /// Applicable only if the Service Action Type is Switch
        /// </summary>
        public ServiceActionCases Cases { get; set; }
        /// <summary>
        /// Stores the parent case that this action belongs to
        /// Applicable only if the Service Action Type is Switch
        /// </summary>
        public ServiceActionCase ParentCase { get; set; }
        /// <summary>
        /// The output mapping for the service action - Travis.Frisinger
        /// </summary>
        public IList<IDev2Definition> ServiceActionOutputs
        {
            get; 
            set;
        }

        /// <summary>
        /// The information used for format the output of the plugin using an IOutputFormatter. 
        /// </summary>
        public string OutputDescription { get; set; }        

        public AppDomain PluginDomain { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the Service Action Class
        /// </summary>
        public ServiceAction() : base(enDynamicServiceObjectType.ServiceAction) {
            ServiceActionInputs = new List<ServiceActionInput>();
            ServiceActionOutputs = new List<IDev2Definition>(); // Travis.Frisinger
            this.ActionType = enActionType.Unknown;
        }
        #endregion

        /// <summary>
        /// Acquires a composite PooledServiceActivity that holds a reference to the underlying activity, once you are done working
        /// with the activity you must return it to the pool via PushActivity.
        /// </summary>
        /// <returns></returns>
        public PooledServiceActivity PopActivity()
        {
            PooledServiceActivity result = null;

            lock (_poolGuard)
            {
                if (_workflowPool.Count == 0)
                {
                    if (_xamlStream == null)
                    {
                        result = new PooledServiceActivity(_generation, _workflowActivity);
                    }
                    else
                    {
                        Activity activity = ActivityXamlServices.Load(_xamlStream);
                        _xamlStream.Seek(0, SeekOrigin.Begin);
                        result = new PooledServiceActivity(_generation, activity);
                    }
                }
                else result = _workflowPool.Dequeue();
            }

            return result;
        }

        /// <summary>
        /// Pushes a previously released activity back into the activity pool.
        /// </summary>
        /// <param name="activity"></param>
        public void PushActivity(PooledServiceActivity activity)
        {
            if (activity != null)
            {
                lock (_poolGuard)
                {
                    if (activity.Generation == _generation)
                    {
                        if (_workflowPool.Count < 10)
                            if (_xamlStream != null)
                                _workflowPool.Enqueue(activity);
                    }
                }
            }
        }

        public override bool Compile() {
            base.Compile();

            foreach (ServiceActionInput sai in this.ServiceActionInputs) {
                sai.Compile();
                sai.CompilerErrors.ToList().ForEach(c => this.CompilerErrors.Add(c));
            }

            if (this.CompilerErrors.Count > 0) {
                return this.IsCompiled;
            }

            switch (this.ActionType) {
                case enActionType.BizRule:
                if (string.IsNullOrEmpty(BizRuleName)) {
                    WriteCompileError(Resources.CompilerError_MissingBizRuleName);
                }
                break;

                case enActionType.Switch:
                    bool writeWarning = this.Cases == null;
                    if (!this.Cases.Cases.Any()) {
                        writeWarning = true;
                    }

                    if (writeWarning) {
                        WriteCompileWarning(Resources.CompilerWarning_SwitchCasesNotFound);
                    }

                    if (string.IsNullOrEmpty(Cases.DataElementName)) {
                        WriteCompileError(Resources.CompilerError_MissingDataElementName);
                    }

                    if (Cases.DefaultCase == null) {
                        WriteCompileError(Resources.CompilerError_SwitchHasNoDefault);
                    }
                break;

                case enActionType.InvokeDynamicService:
                    if (string.IsNullOrEmpty(this.ServiceName)) {
                        WriteCompileError(Resources.CompilerError_MissingServiceName);
                    }
                break;

                case enActionType.Workflow:
                //if (WorkflowActivity == null) {
                //    WriteCompileError(Resources.CompilerError_InvalidWorkflowXaml);
                //}
                break;

                default:
                    //A Source Name is required except in the case of Management Dynamic Services
                    if(string.IsNullOrEmpty(this.SourceName) && ActionType != enActionType.InvokeManagementDynamicService){
                        WriteCompileError(Resources.CompilerError_MissingSourceName);
                    }
                    if (string.IsNullOrEmpty(this.SourceMethod)) {
                        WriteCompileError(Resources.CompilerError_MissingSourceMethod);
                    }
                    //A source is required except in the case of Management Dynamic Services
                    if (Source == null && ActionType != enActionType.InvokeManagementDynamicService) {
                        WriteCompileError(Resources.CompilerError_SourceNotFound);
                    }
                break;
                

            }

            return this.IsCompiled;

        }

        public void SetActivity(Activity activity)
        {
            _workflowActivity = activity;
        }

        
    }
    #endregion

    public sealed class PooledServiceActivity
    {
        private int _generation;
        private Activity _value;

        public int Generation { get { return _generation; } }
        public Activity Value { get { return _value; } }

        internal PooledServiceActivity(int generation, Activity value)
        {
            _generation = generation;
            _value = value;
        }
    }
}
