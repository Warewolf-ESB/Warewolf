using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.DynamicServices.Objects.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.DynamicServices.Objects
{


    #region Service Action Class - Represents a single action within a service
    /// <summary>
    /// Represents a service action which is the single unit of work that can occur
    /// in a dynamic service. This could be stored procedure invocation in a 
    /// data store or a webservice call or a static Service Method invocation
    /// that will occur locally on the service
    /// </summary>
    public class ServiceAction : DynamicServiceObjectBase, IDisposable
    {
        #region Private Fields
        private bool _resultsToClient = true;
        private bool _terminateServiceOnFault = true;
        private int _commandTimeout = 30;
        private Activity _workflowActivity;
        private StringBuilder _xamlDefinition;

        private readonly object _poolGuard = new object();
        private const int _generation = 0;
        private Queue<PooledServiceActivity> _workflowPool = new Queue<PooledServiceActivity>();

        private Stream _xamlStream;
        bool _disposing;
        #endregion

        #region Public Properties
        public int CommandTimeout
        {

            get
            {
                return _commandTimeout;
            }

            set
            {
                _commandTimeout = value;
            }

        }

        public Stream XamlStream { get { return _xamlStream; } }

        /// <summary>
        /// The type of action that this action will invoke
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Common.Interfaces.Core.DynamicServices.enActionType ActionType { get; set; }
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
        public StringBuilder XamlDefinition
        {
            get
            {
                return _xamlDefinition;
            }
            set
            {
                lock(_poolGuard)
                {
                    // Default operation
                    _xamlDefinition = value;
                    if(_xamlStream != null)
                    {
                        _xamlStream.Close();
                        _xamlStream.Dispose();
                        _xamlStream = new MemoryStream();
                    }

                    // now load it all up ;)
                    // extracted to avoid memory leak in MS utilities and root references 
                    var xamlLoader = new Dev2XamlLoader();
                    xamlLoader.Load(value, ref _xamlStream, ref _workflowPool, ref _workflowActivity);
                }
            }

        }
        /// <summary>
        /// The activity implementation created from the workflow xaml
        /// </summary>
        public Activity WorkflowActivity
        {
            get
            {
                return _workflowActivity;
            }
        }

        /// <summary>
        /// The inputs for this service action
        /// </summary>
        public List<ServiceActionInput> ServiceActionInputs { get; set; }
        /// <summary>
        /// The name of the service that will be invoked if this is an InvokeDynamicService type service action
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// The ID of the service that will be invoked if this is an InvokeDynamicService type service action, this is prefered over using serviceName to resolve the service
        /// </summary>
        public Guid ServiceID { get; set; }
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
        public ServiceAction()
            : base(enDynamicServiceObjectType.ServiceAction)
        {
            ServiceActionInputs = new List<ServiceActionInput>();
            ServiceActionOutputs = new List<IDev2Definition>(); // Travis.Frisinger
            ActionType = Common.Interfaces.Core.DynamicServices.enActionType.Unknown;
        }
        #endregion

        /// <summary>
        /// Acquires a composite PooledServiceActivity that holds a reference to the underlying activity, once you are done working
        /// with the activity you must return it to the pool via PushActivity.
        /// </summary>
        /// <returns></returns>
        public PooledServiceActivity PopActivity()
        {
            PooledServiceActivity result;

            lock(_poolGuard)
            {
                if(_workflowPool.Count == 0)
                {
                    if(_xamlStream == null)
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
            if(activity != null)
            {
                lock(_poolGuard)
                {
                    if(activity.Generation == _generation)
                    {
                        if(_workflowPool.Count < 10)
                            if(_xamlStream != null)
                                _workflowPool.Enqueue(activity);
                    }
                }
            }
        }

        public override bool Compile()
        {
            base.Compile();

            foreach(ServiceActionInput sai in ServiceActionInputs)
            {
                sai.Compile();
                sai.CompilerErrors.ToList().ForEach(c => CompilerErrors.Add(c));
            }

            if(CompilerErrors.Count > 0)
            {
                return IsCompiled;
            }

            switch(ActionType)
            {

                case Common.Interfaces.Core.DynamicServices.enActionType.InvokeDynamicService:
                    if(string.IsNullOrEmpty(ServiceName))
                    {
                        WriteCompileError(Resources.CompilerError_MissingServiceName);
                    }
                    break;

                case Common.Interfaces.Core.DynamicServices.enActionType.Workflow:
                    break;

                default:
                    //A Source Name is required except in the case of Management Dynamic Services
                    if(string.IsNullOrEmpty(SourceName) && ActionType != Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService)
                    {
                        WriteCompileError(Resources.CompilerError_MissingSourceName);
                    }
                    if(string.IsNullOrEmpty(SourceMethod))
                    {
                        WriteCompileError(Resources.CompilerError_MissingSourceMethod);
                    }
                    //A source is required except in the case of Management Dynamic Services
                    if(Source == null && ActionType != Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService)
                    {
                        WriteCompileError(Resources.CompilerError_SourceNotFound);
                    }
                    break;


            }

            return IsCompiled;

        }

        public void SetActivity(Activity activity)
        {
            _workflowActivity = activity;
        }



        // Picked up hanging references via .net memory profiler ;)
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposing)
            {
                if(disposing)
                {
                    _xamlStream.Close();
                    _xamlStream.Dispose();
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }
            _disposing = true;

        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
    #endregion

    public sealed class PooledServiceActivity
    {
        private readonly int _generation;
        private readonly Activity _value;

        public int Generation { get { return _generation; } }
        public Activity Value { get { return _value; } }

        internal PooledServiceActivity(int generation, Activity value)
        {
            _generation = generation;
            _value = value;
        }
    }
}
