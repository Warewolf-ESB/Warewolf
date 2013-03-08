using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.DurableInstancing;
using System.Activities;
using Dev2.DataList.Contract;
using Dev2.Network.Execution;
using Dev2.Workspaces;

namespace Dev2.DynamicServices
{
    /// <summary>
    /// The class responsible for creating a workflow entry point
    /// </summary>
    public class WorkflowApplicationFactory
    {
        public static long Balance = 0;

        private static readonly FileSystemInstanceStore _InstanceStore = new FileSystemInstanceStore();


        /// <summary>
        /// Invokes the workflow.
        /// </summary>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <param name="dataTransferObject">The data transfer object.</param>
        /// <param name="executionExtensions">The execution extensions.</param>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="bookmarkName">Name of the bookmark.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IDSFDataObject InvokeWorkflow(Activity workflowActivity, IDSFDataObject dataTransferObject, IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName, out ErrorResultTO errors)
        {
            return InvokeWorkflowImpl(workflowActivity, dataTransferObject, executionExtensions, instanceId, workspace, bookmarkName, dataTransferObject.IsDebug, out errors);
        }

        /// <summary>
        /// Invokes the workflow.
        /// </summary>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <param name="dataTransferObject">The data transfer object.</param>
        /// <param name="executionExtensions">The execution extensions.</param>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IDSFDataObject InvokeWorkflow(Activity workflowActivity, IDSFDataObject dataTransferObject, IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, out ErrorResultTO errors)
        {
            return InvokeWorkflowImpl(workflowActivity, dataTransferObject, executionExtensions, instanceId, workspace, string.Empty, dataTransferObject.IsDebug, out errors);
        }

        private Guid FetchParentInstanceID(IDSFDataObject dataTransferObject)
        {
            Guid parentWorkflowInstanceId;

            Guid.TryParse(dataTransferObject.ParentWorkflowInstanceId, out parentWorkflowInstanceId);

            return parentWorkflowInstanceId;
        }

        /// <summary>
        /// Inits the entry point.
        /// </summary>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <param name="dataTransferObject">The data transfer object.</param>
        /// <param name="executionExtensions">The execution extensions.</param>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="bookmarkName">Name of the bookmark.</param>
        /// <param name="isDebug">if set to <c>true</c> [is debug].</param>
        /// <returns></returns>
        private WorkflowApplication InitEntryPoint(Activity workflowActivity, IDSFDataObject dataTransferObject, IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName, bool isDebug)
        {
            dataTransferObject.WorkspaceID = workspace.ID;

            Dictionary<string, object> inputarguments = new Dictionary<string, object>();
            
            WorkflowApplication wfApp = null;

            Guid parentInstanceID = FetchParentInstanceID(dataTransferObject);

            if (parentInstanceID != Guid.Empty)
            {
                inputarguments.Add("ParentWorkflowInstanceId", parentInstanceID);

                if (!string.IsNullOrEmpty(dataTransferObject.ParentServiceName))
                {
                    inputarguments.Add("ParentServiceName", dataTransferObject.ParentServiceName);
                }
            }

            // Set the old AmbientDatalist as the DataListID ;)
            inputarguments.Add("AmbientDataList", new List<string> { dataTransferObject.DataListID.ToString() });

            if ((parentInstanceID != Guid.Empty && instanceId == Guid.Empty) || string.IsNullOrEmpty(bookmarkName))
            {
                wfApp = new WorkflowApplication(workflowActivity, inputarguments);
            }
            else
            {
                if (!string.IsNullOrEmpty(bookmarkName))
                {
                    wfApp = new WorkflowApplication(workflowActivity);
                }
            }

            if(wfApp != null)
            {

                wfApp.InstanceStore = _InstanceStore;

                if(executionExtensions != null)
                {
                    executionExtensions.ToList().ForEach(exec => wfApp.Extensions.Add(exec));
                }

                // Force a save to the server ;)
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();

                wfApp.Extensions.Add(dataTransferObject);
                wfApp.Extensions.Add(compiler);
                wfApp.Extensions.Add(parser);
            }

            return wfApp;
        }

        /// <summary>
        /// Invokes the workflow impl.
        /// </summary>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <param name="dataTransferObject">The data transfer object.</param>
        /// <param name="executionExtensions">The execution extensions.</param>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="bookmarkName">Name of the bookmark.</param>
        /// <param name="isDebug">if set to <c>true</c> [is debug].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private IDSFDataObject InvokeWorkflowImpl(Activity workflowActivity, IDSFDataObject dataTransferObject, IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName, bool isDebug, out ErrorResultTO errors)
        {
            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID, ExecutionStatusCallbackMessageType.StartedCallback);
            WorkflowApplication wfApp = InitEntryPoint(workflowActivity, dataTransferObject, executionExtensions, instanceId, workspace, bookmarkName, isDebug);
            errors = new ErrorResultTO();
            
            if(wfApp != null)
            {
                using(ManualResetEventSlim waitHandle = new ManualResetEventSlim(false))
                {
                    WorkflowApplicationRun run = new WorkflowApplicationRun(this, waitHandle, dataTransferObject, wfApp, workspace, executionExtensions, FetchParentInstanceID(dataTransferObject), isDebug);

                    if(instanceId == Guid.Empty)
                    {
                        Interlocked.Increment(ref Balance);
                        run.Instance.Run();
                        waitHandle.Wait();
                    }
                    else
                    {
                        Interlocked.Increment(ref Balance);

                        try
                        {
                            if(!string.IsNullOrEmpty(bookmarkName))
                            {
                                //IDSFDataObject resumptionDataObject = dataTransferObject.Clone();
                                var existingDlid = dataTransferObject.DataListID;

                                run.Instance.Load(instanceId);
                                // Changed it so the "child data" can pass through ;) -- It is already present in the AbmientData var, hence the null ;)
                                run.Instance.ResumeBookmark(bookmarkName, existingDlid);
                            }
                            else
                            {
                                run.Instance.Run();
                            }

                            waitHandle.Wait();
                        }
                        catch(InstanceNotReadyException)
                        {
                            Interlocked.Decrement(ref Balance);
                            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);

                            return null;
                        }
                        catch(InstancePersistenceException)
                        {
                            Interlocked.Decrement(ref Balance);
                            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);

                            return run.DataTransferObject;
                        }
                        catch(Exception ex)
                        {
                            Interlocked.Decrement(ref Balance);

                            errors.AddError(ex.Message);

                            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);

                            return run.DataTransferObject;
                        }
                        //finally
                        //{
                        //    waitHandle.Dispose(); // clean up ;)
                        //}
                    }

                    Interlocked.Decrement(ref Balance);
                    dataTransferObject = run.DataTransferObject;
                }
            }else{
                errors.AddError("Internal System Error : Could not create workflow execution wrapper");
            }

            return dataTransferObject;
        }

        private sealed class WorkflowApplicationRun : IDisposable
        {
            #region Instance Fields
            private bool _isDisposed;
            private WorkflowApplicationFactory _owner;
            private WorkflowApplication _instance;
            private IWorkspace _workspace;
            private bool _isDebug;
            private Guid _parentWorkflowInstanceID;
            private Guid _currentInstanceID;
            private IList<object> _executionExtensions;
            private ManualResetEventSlim _waitHandle;
            private IDSFDataObject _result;
            #endregion

            #region Public Properties
            public IDSFDataObject DataTransferObject { get { return _result; } }
            public WorkflowApplication Instance { get { return _instance; } }
            #endregion

            #region Constructor
            public WorkflowApplicationRun(WorkflowApplicationFactory owner, ManualResetEventSlim waitHandle, IDSFDataObject dataTransferObject, WorkflowApplication instance, IWorkspace workspace, IList<object> executionExtensions, Guid parentWorkflowInstanceId, bool isDebug)
            {
                _owner = owner;
                _waitHandle = waitHandle;
                _result = dataTransferObject;
                _instance = instance;
                _workspace = workspace;
                _executionExtensions = executionExtensions;
                _isDebug = isDebug;
                _parentWorkflowInstanceID = parentWorkflowInstanceId;

                _instance.PersistableIdle = OnPersistableIdle;
                _instance.Unloaded = OnUnloaded;
                _instance.Completed = OnCompleted;
                _instance.OnUnhandledException = OnUnhandledException;
            }
            #endregion

            #region Completion Handling
            private void LockServices()
            {
            }

            private void UnlockServices()
            {
            }

            private void OnCompleted(WorkflowApplicationCompletedEventArgs args) {
                _result = args.GetInstanceExtensions<IDSFDataObject>().ToList().First();
                IDataListCompiler compiler = args.GetInstanceExtensions<IDataListCompiler>().First();
                // PBI : 5376 Removed line below
                //_result.XmlData = _result.XmlData.Replace("&", "&amp;");

                try
                {
                    if (!_isDebug)
                    {
                        IDictionary<string, object> outputs = args.Outputs;

                        bool createResumptionPoint = false;

                        // Travis.Frisinger : 19.10.2012 - Duplicated Recordset Data Bug 6038

                        object parentId;

                        outputs.TryGetValue("ParentWorkflowInstanceId", out parentId);

                        parentId = _result.ParentWorkflowInstanceId;

                        //outputs.TryGetValue("ParentWorkflowInstanceId", out parentId);
                        //object parentId = outputs["ParentWorkflowInstanceId"];

                        object parentServiceName;
                        outputs.TryGetValue("ParentServiceName", out parentServiceName);

                        parentServiceName = _result.ParentServiceName;

                        //object parentServiceName = outputs["ParentServiceName"];

                        object objcreateResumptionPoint;


                        if (outputs.TryGetValue("CreateResumuptionPoint", out objcreateResumptionPoint))
                        {
                            createResumptionPoint = (bool)objcreateResumptionPoint;
                        }

                        PooledServiceActivity wfActivity = null;

                        if (!String.IsNullOrEmpty(parentServiceName == null ? String.Empty : parentServiceName.ToString()) && Guid.TryParse(parentId.ToString(), out _parentWorkflowInstanceID))
                        {
                            if (_parentWorkflowInstanceID != _currentInstanceID)
                            {
                                if (_workspace.Host != null)
                                {
                                    IEnumerable<DynamicService> services;
                                    _workspace.Host.LockServices();

                                    try
                                    {
                                        services = _workspace.Host.Services.Where(svc => svc.Name.Equals(parentServiceName == null ? string.Empty : parentServiceName.ToString(), StringComparison.InvariantCultureIgnoreCase));
                                    }
                                    finally
                                    {
                                        _workspace.Host.UnlockServices();
                                    }

                                    if (services.Count() > 0)
                                    {
                                        _currentInstanceID = _parentWorkflowInstanceID;
                                        var actionSet = services.First().Actions;

                                        if (_result.WorkflowResumeable)
                                        {
                                            ServiceAction serviceAction = actionSet.First();
                                            wfActivity = serviceAction.PopActivity();

                                            try
                                            {
                                                //_result = _owner.InvokeWorkflow(wfActivity.Value, _result, _executionExtensions, _parentWorkflowInstanceID, _workspace, "dsfResumption");
                                            }
                                            finally
                                            {
                                                serviceAction.PushActivity(wfActivity);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    ExecutionStatusCallbackDispatcher.Instance.Post(_result.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);
                }
                finally {
                    _waitHandle.Set();
                }

                ExecutionStatusCallbackDispatcher.Instance.Post(_result.ExecutionCallbackID, ExecutionStatusCallbackMessageType.CompletedCallback);
            }

            #endregion

            #region Persistence Handling
            private PersistableIdleAction OnPersistableIdle(WorkflowApplicationIdleEventArgs args)
            {
                _result = args.GetInstanceExtensions<IDSFDataObject>().ToList().First();
                _waitHandle.Set();
                return PersistableIdleAction.Unload;
            }
            #endregion

            #region [Unload/Exception] Handling
            private void OnUnloaded(WorkflowApplicationEventArgs args)
            {
                _waitHandle.Set();
            }

            private UnhandledExceptionAction OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs args)
            {
                _waitHandle.Set();
                ExecutionStatusCallbackDispatcher.Instance.Post(_result.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);
                return UnhandledExceptionAction.Abort;
            }
            #endregion

            #region Disposal Handling
            ~WorkflowApplicationRun()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                if(_isDisposed)
                    return;
                _isDisposed = true;
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                _owner = null;
                _instance = null;
                _workspace = null;
                _result = null;
                _waitHandle = null;
            }
            #endregion
        }
    }
}
