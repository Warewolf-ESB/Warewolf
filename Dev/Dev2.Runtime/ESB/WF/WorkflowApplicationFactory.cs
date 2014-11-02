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
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.DynamicServices.Objects.Base;
using Dev2.Instrumentation;
using Dev2.Network.Execution;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using ServiceStack.Common.Extensions;

namespace Dev2.Runtime.ESB.WF
{
    /// <summary>
    ///     The class responsible for creating a workflow entry point
    /// </summary>
    public class WorkflowApplicationFactory
    {
        public static long Balance = 0;
        private static readonly FileSystemInstanceStore InstanceStore = new FileSystemInstanceStore();
        private DateTime _runTime;

        public ErrorResultTO AllErrors { get; private set; }

        /// <summary>
        ///     Invokes the workflow.
        /// </summary>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <param name="dataTransferObject">The data transfer object.</param>
        /// <param name="executionExtensions">The execution extensions.</param>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="bookmarkName">Name of the bookmark.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IDSFDataObject InvokeWorkflow(Activity workflowActivity, IDSFDataObject dataTransferObject,
            IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName,
            out ErrorResultTO errors)
        {
            return InvokeWorkflowImpl(workflowActivity, dataTransferObject, executionExtensions, instanceId, workspace,
                bookmarkName, dataTransferObject.IsDebug, out errors);
        }

        private Guid FetchParentInstanceId(IDSFDataObject dataTransferObject)
        {
            Guid parentWorkflowInstanceId;

            Guid.TryParse(dataTransferObject.ParentWorkflowInstanceId, out parentWorkflowInstanceId);

            return parentWorkflowInstanceId;
        }

        /// <summary>
        ///     Inits the entry point.
        /// </summary>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <param name="dataTransferObject">The data transfer object.</param>
        /// <param name="executionExtensions">The execution extensions.</param>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="bookmarkName">Name of the bookmark.</param>
        /// <returns></returns>
        // ReSharper disable once UnusedParameter.Local // ignored as not sure how this method is invoked and what the knock on effect maybe
        private WorkflowApplication InitEntryPoint(Activity workflowActivity, IDSFDataObject dataTransferObject,
            IEnumerable<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName)
        {
            dataTransferObject.WorkspaceID = workspace.ID;

            var inputarguments = new Dictionary<string, object>();

            WorkflowApplication wfApp = null;

            Guid parentInstanceID = FetchParentInstanceId(dataTransferObject);

            if (parentInstanceID != Guid.Empty)
            {
                inputarguments.Add("ParentWorkflowInstanceId", parentInstanceID);

                if (!string.IsNullOrEmpty(dataTransferObject.ParentServiceName))
                {
                    inputarguments.Add("ParentServiceName", dataTransferObject.ParentServiceName);
                }
            }

            // Set the old AmbientDatalist as the DataListID ;)
            inputarguments.Add("AmbientDataList", new List<string> {dataTransferObject.DataListID.ToString()});

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

            if (wfApp != null)
            {
                wfApp.InstanceStore = InstanceStore;

                if (executionExtensions != null)
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
        ///     Invokes the workflow impl.
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
        private IDSFDataObject InvokeWorkflowImpl(Activity workflowActivity, IDSFDataObject dataTransferObject,
            IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName, bool isDebug,
            out ErrorResultTO errors)
        {
            Tracker.TrackEvent(TrackerEventGroup.Workflows, TrackerEventName.Executed,
                string.Format("RES-{0}", dataTransferObject.ResourceID));

            if (AllErrors == null)
            {
                AllErrors = new ErrorResultTO();
            }
            IExecutionToken exeToken = new ExecutionToken {IsUserCanceled = false};

            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID,
                ExecutionStatusCallbackMessageType.StartedCallback);
            WorkflowApplication wfApp = InitEntryPoint(workflowActivity, dataTransferObject, executionExtensions,
                instanceId, workspace, bookmarkName);
            errors = new ErrorResultTO();

            if (wfApp != null)
            {
                // add termination token
                wfApp.Extensions.Add(exeToken);

                using (var waitHandle = new ManualResetEventSlim(false))
                {
                    var run = new WorkflowApplicationRun(this, waitHandle, dataTransferObject, wfApp, workspace,
                        executionExtensions, FetchParentInstanceId(dataTransferObject), isDebug, errors, exeToken);


                    if (instanceId == Guid.Empty)
                    {
                        Interlocked.Increment(ref Balance);
                        run.Run();
                        _runTime = DateTime.Now;
                        waitHandle.Wait();
                    }
                    else
                    {
                        Interlocked.Increment(ref Balance);

                        try
                        {
                            if (!string.IsNullOrEmpty(bookmarkName))
                            {
                                dataTransferObject.CurrentBookmarkName = bookmarkName;
                                run.Resume(dataTransferObject);
                            }
                            else
                            {
                                run.Run();
                                _runTime = DateTime.Now;
                            }

                            waitHandle.Wait();
                        }
                        catch (InstanceNotReadyException e1)
                        {
                            Interlocked.Decrement(ref Balance);
                            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID,
                                ExecutionStatusCallbackMessageType.ErrorCallback);

                            errors.AddError(e1.Message);
                            AllErrors.AddError(e1.Message);
                            return null;
                        }
                        catch (InstancePersistenceException e2)
                        {
                            Interlocked.Decrement(ref Balance);
                            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID,
                                ExecutionStatusCallbackMessageType.ErrorCallback);

                            errors.AddError(e2.Message);
                            AllErrors.AddError(e2.Message);
                            return run.DataTransferObject.Clone();
                        }
                        catch (Exception ex)
                        {
                            Interlocked.Decrement(ref Balance);

                            errors.AddError(ex.Message);
                            AllErrors.AddError(ex.Message);

                            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID,
                                ExecutionStatusCallbackMessageType.ErrorCallback);

                            return run.DataTransferObject.Clone();
                        }
                    }

                    Interlocked.Decrement(ref Balance);
                    dataTransferObject = run.DataTransferObject.Clone();
                    var wfappUtils = new WfApplicationUtils();

                    ErrorResultTO invokeErrors;
                    wfappUtils.DispatchDebugState(dataTransferObject, StateType.End, AllErrors.HasErrors(),
                        AllErrors.MakeDisplayReady(), out invokeErrors, _runTime, false, true);
                    AllErrors.MergeErrors(invokeErrors);
                    // avoid memory leak ;)
                    run.Dispose();
                }
            }
            else
            {
                errors.AddError("Internal System Error : Could not create workflow execution wrapper");
            }

            return dataTransferObject;
        }

        private sealed class WorkflowApplicationRun : IExecutableService, IDisposable
        {
            #region Instance Fields

            private readonly IList<object> _executionExtensions;
            private readonly IExecutionToken _executionToken;
            private readonly bool _isDebug;
            private Guid _currentInstanceID;
            private WorkflowApplication _instance;
            private bool _isDisposed;
            private WorkflowApplicationFactory _owner;
            private Guid _parentWorkflowInstanceID;
            private IDSFDataObject _result;
            private ManualResetEventSlim _waitHandle;
            private IWorkspace _workspace;

            #endregion

            #region Public Properties

            private IList<IExecutableService> _associatedServices;
            private int _previousNumberOfSteps;

            public IDSFDataObject DataTransferObject
            {
                get { return _result; }
            }

            private ErrorResultTO AllErrors { get; set; }

            #endregion

            #region Constructor

            public WorkflowApplicationRun(WorkflowApplicationFactory owner, ManualResetEventSlim waitHandle,
                IDSFDataObject dataTransferObject, WorkflowApplication instance, IWorkspace workspace,
                IList<object> executionExtensions, Guid parentWorkflowInstanceId, bool isDebug, ErrorResultTO errors,
                IExecutionToken executionToken)
            {
                _owner = owner;
                _waitHandle = waitHandle;
                _result = dataTransferObject;
                _instance = instance;
                _workspace = workspace;
                _executionExtensions = executionExtensions;
                _isDebug = isDebug;
                _parentWorkflowInstanceID = parentWorkflowInstanceId;
                _executionToken = executionToken;

                _instance.PersistableIdle = OnPersistableIdle;
                _instance.Unloaded = OnUnloaded;
                _instance.Completed = OnCompleted;
                _instance.Aborted = OnAborted;
                _instance.OnUnhandledException = OnUnhandledException;

                AllErrors = errors;
            }

            private void OnAborted(WorkflowApplicationAbortedEventArgs obj)
            {
            }

            #endregion

            public Guid ID { get; set; }
            public Guid WorkspaceID { get; set; }

            public IList<IExecutableService> AssociatedServices
            {
                get { return _associatedServices ?? (_associatedServices = new List<IExecutableService>()); }
            }

            public Guid ParentID { get; set; }

            public void Run()
            {
                Guid id;
                Guid.TryParse(DataTransferObject.ParentInstanceID, out id);
                ID = DataTransferObject.ResourceID;
                ParentID = DataTransferObject.ParentID;
                WorkspaceID = DataTransferObject.WorkspaceID;

                // handle queuing 
                WaitForSlot();

                // abort the execution - no space at the inn
                if (!ExecutableServiceRepository.Instance.DoesQueueHaveSpace())
                {
                    _instance.Abort();
                }
                else
                {
                    ExecutableServiceRepository.Instance.Add(this);
                    // here is space at the inn ;)
                    var wfappUtils = new WfApplicationUtils();

                    ErrorResultTO invokeErrors;
                    wfappUtils.DispatchDebugState(DataTransferObject, StateType.Start, AllErrors.HasErrors(),
                        AllErrors.MakeDisplayReady(), out invokeErrors, null, true);
                    AllErrors.MergeErrors(invokeErrors);
                    _previousNumberOfSteps = DataTransferObject.NumberOfSteps;
                    DataTransferObject.NumberOfSteps = 0;
                    _instance.Run();
                }
            }

            public void Terminate()
            {
                try
                {
                    // signal user termination ;)
                    _executionToken.IsUserCanceled = true;

                    // This was cancel which left the activities resident in the background and caused chaos!
                    _instance.Terminate(new Exception("User Termination"), new TimeSpan(0, 0, 0, 60, 0));
                }
                catch (Exception e)
                {
                    Dev2Logger.Log.Error(e);
                    _instance.Abort();
                }

                ExecutableServiceRepository.Instance.Remove(this);
                AssociatedServices.ForEach(s => s.Terminate());
                Dispose();
            }

            public void Terminate(Exception exception)
            {
                try
                {
                    // signal user termination ;)
                    _executionToken.IsUserCanceled = true;

                    // This was cancel which left the activities resident in the background and caused chaos!
                    _instance.Terminate(exception);
                }
                catch (Exception e)
                {
                    Dev2Logger.Log.Error(e);
                }
                finally
                {
                    ExecutableServiceRepository.Instance.Remove(this);
                    AssociatedServices.ForEach(s => s.Terminate());
                    Dispose();
                }
            }

            public void Resume(IDSFDataObject dataObject)
            {
                Guid instanceID = dataObject.WorkflowInstanceId;
                string bookmarkName = dataObject.CurrentBookmarkName;
                Guid existingDlid = dataObject.DataListID;
                _instance.Load(instanceID);
                _instance.ResumeBookmark(bookmarkName, existingDlid);
            }

            #region Completion Handling

            private void OnCompleted(WorkflowApplicationCompletedEventArgs args)
            {
                _result = args.GetInstanceExtensions<IDSFDataObject>().ToList().First();
                IDictionary<string, object> outputs = args.Outputs;

                try
                {
                    if (!_isDebug)
                    {
                        // Travis.Frisinger : 19.10.2012 - Duplicated Recordset Data Bug 6038

                        object parentId;

                        outputs.TryGetValue("ParentWorkflowInstanceId", out parentId);

                        parentId = _result.ParentWorkflowInstanceId;

                        object parentServiceName;
                        outputs.TryGetValue("ParentServiceName", out parentServiceName);

                        string parentServiceNameStr = string.IsNullOrEmpty(_result.ParentServiceName)
                            ? string.Empty
                            : _result.ParentServiceName;

                        if (!string.IsNullOrEmpty(parentServiceNameStr) &&
                            Guid.TryParse(parentId.ToString(), out _parentWorkflowInstanceID))
                        {
                            if (_parentWorkflowInstanceID != _currentInstanceID)
                            {
                                // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
                                List<DynamicServiceObjectBase> services =
                                    ResourceCatalog.Instance.GetDynamicObjects<DynamicServiceObjectBase>(_workspace.ID,
                                        parentServiceNameStr);
                                if (services != null && services.Count > 0)
                                {
                                    var service = services[0] as DynamicService;
                                    if (service != null)
                                    {
                                        _currentInstanceID = _parentWorkflowInstanceID;

                                        List<ServiceAction> actionSet = service.Actions;

                                        if (_result.WorkflowResumeable)
                                        {
                                            ServiceAction serviceAction = actionSet.First();
                                            PooledServiceActivity wfActivity = serviceAction.PopActivity();

                                            try
                                            {
                                                ErrorResultTO invokeErrors;
                                                _result = _owner.InvokeWorkflow(wfActivity.Value, _result,
                                                    _executionExtensions, _parentWorkflowInstanceID, _workspace,
                                                    "dsfResumption", out invokeErrors);
                                                // attach any execution errors
                                                if (AllErrors != null)
                                                {
                                                    AllErrors.MergeErrors(invokeErrors);
                                                }
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
                catch (Exception ex)
                {
                    Dev2Logger.Log.Error(ex);
                    ExecutionStatusCallbackDispatcher.Instance.Post(_result.ExecutionCallbackID,
                        ExecutionStatusCallbackMessageType.ErrorCallback);
                }
                finally
                {
                    try
                    {
                        if (_waitHandle != null) _waitHandle.Set();
                        ExecutableServiceRepository.Instance.Remove(this);
                        if (DataTransferObject != null)
                        {
                            DataTransferObject.NumberOfSteps = _previousNumberOfSteps;
                        }
                    }
                    catch (Exception e)
                    {
                        // Best effort ;)
                        Dev2Logger.Log.Error(e);
                    }
                }


                // force a throw to kill the engine ;)
                if (args.TerminationException != null)
                {
                    _instance.Terminate("Force Terminate", new TimeSpan(0, 0, 1, 0));
                }

                // Not compatable with run.Dispose() 
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
                if (_waitHandle != null)
                {
                    _waitHandle.Set();
                }
            }

            private UnhandledExceptionAction OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs args)
            {
                ExecutableServiceRepository.Instance.Remove(this);
                _waitHandle.Set();
                ExecutionStatusCallbackDispatcher.Instance.Post(_result.ExecutionCallbackID,
                    ExecutionStatusCallbackMessageType.ErrorCallback);
                return UnhandledExceptionAction.Abort;
            }

            #endregion

            #region Disposal Handling

            public void Dispose()
            {
                if (_isDisposed)
                    return;
                _isDisposed = true;
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~WorkflowApplicationRun()
            {
                Dispose(false);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _owner = null;
                    _instance = null;
                    _workspace = null;
                    _result = null;
                    _waitHandle = null;
                }
            }

            #endregion

            /// <summary>
            ///     Waits for slot.
            ///     Waits for up to 10 minutes before it aborts
            /// </summary>
            private void WaitForSlot()
            {
                int waitCnt = 0;
                while (!ExecutableServiceRepository.Instance.DoesQueueHaveSpace() &&
                       waitCnt < GlobalConstants.MaxNumberOfWorkflowWaits)
                {
                    Thread.Sleep(GlobalConstants.WorkflowWaitTime);
                    waitCnt++;
                }
            }
        }
    }
}