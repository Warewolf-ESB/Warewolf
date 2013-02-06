using Dev2.DataList.Contract;
using Dev2.Network.Execution;
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Threading;
using Unlimited.Framework;

// ReSharper disable CheckNamespace
namespace Dev2.DynamicServices
// ReSharper restore CheckNamespace
{
    public class WorkflowApplicationFactory
    {
        public static long Balance = 0;

        public WorkflowApplicationFactory()
        {
        }

        public IDSFDataObject InvokeWorkflow(Activity workflowActivity, IDSFDataObject dataTransferObject, IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName = "", bool isDebug = false)
        {
            //long startBalance = Interlocked.Read(ref Balance);
            IDSFDataObject result = InvokeWorkflowImpl(workflowActivity, dataTransferObject, executionExtensions, instanceId, workspace, bookmarkName, isDebug);
            //if (Interlocked.Read(ref Balance) != startBalance) System.Diagnostics.Debugger.Break();
            return result;
        }

        private IDSFDataObject InvokeWorkflowImpl(Activity workflowActivity, IDSFDataObject dataTransferObject, IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName, bool isDebug)
        {
            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID, ExecutionStatusCallbackMessageType.StartedCallback);

            dataTransferObject.WorkspaceID = workspace.ID;

            // NOTE: Don't remove this Guid.Empty check otherwise Activity unit tests will fail!
            //       See BaseActivityUnitTest.ExecuteProcess
            if(dataTransferObject.ServerID == Guid.Empty)
            {
                dataTransferObject.ServerID = HostSecurityProvider.Instance.ServerID;
            }

            //string modifiedData = dataTransferObject.XmlData;

            Dictionary<string, object> inputarguments = new Dictionary<string, object>();
            WorkflowApplication wfApp = null;

            // Is this still needed????
            Guid parentWorkflowInstanceId = Guid.Empty;
            if(Guid.TryParse(dataTransferObject.ParentWorkflowInstanceId, out parentWorkflowInstanceId))
            {
                inputarguments.Add("ParentWorkflowInstanceId", parentWorkflowInstanceId);

                if(!string.IsNullOrEmpty(dataTransferObject.ParentServiceName))
                {
                    inputarguments.Add("ParentServiceName", dataTransferObject.ParentServiceName);
                }
            }

            // Travis.Frisinger : 2012.11.07 - Removed for new DataList Server
            //inputarguments.Add("AmbientDataList", new List<string> { dataTransferObject.XmlData });
            // changed to passing in DataListID for decision nodes ;)
            inputarguments.Add("AmbientDataList", new List<string> { dataTransferObject.DataListID.ToString() });

            if((parentWorkflowInstanceId != Guid.Empty && instanceId == Guid.Empty) || string.IsNullOrEmpty(bookmarkName))
            {
                wfApp = new WorkflowApplication(workflowActivity, inputarguments);
            }
            else
            {
                if(!string.IsNullOrEmpty(bookmarkName))
                {
                    wfApp = new WorkflowApplication(workflowActivity);
                }
            }

            wfApp.InstanceStore = new FileSystemInstanceStore();

            if(executionExtensions != null)
            {
                executionExtensions.ToList().ForEach(exec => wfApp.Extensions.Add(exec));
            }

            // Travis.Frisinger : Amend
            IDataListBinder binder = new DataListBinder();
            // Force a save to the server ;)
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();

            wfApp.Extensions.Add(dataTransferObject);
            wfApp.Extensions.Add(binder);
            wfApp.Extensions.Add(compiler);
            wfApp.Extensions.Add(parser);

            using(ManualResetEventSlim waitHandle = new ManualResetEventSlim(false))
            {
                WorkflowApplicationRun run = new WorkflowApplicationRun(this, waitHandle, dataTransferObject, wfApp, workspace, executionExtensions, parentWorkflowInstanceId, isDebug);

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
                            var existingDLID = dataTransferObject.DataListID;

                            run.Instance.Load(instanceId);
                            // Changed it so the "child data" can pass through ;) -- It is already present in the AbmientData var, hence the null ;)
                            run.Instance.ResumeBookmark(bookmarkName, existingDLID);
                            //run.Instance.ResumeBookmark(bookmarkName, run.DataTransferObject);
                            // PBI : 5376 Removed below
                            //run.Instance.ResumeBookmark(bookmarkName, run.DataTransferObject.XmlData);
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
                        var error = new UnlimitedObject(ex);
                        //var returnData = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(run.DataTransferObject.XmlData);
                        //returnData.AddResponse(error);
                        //run.DataTransferObject.XmlData = returnData.XmlString;

                        // PBI : 5376 Push Error to DataList ;)
                        ErrorResultTO errors = new ErrorResultTO();
                        compiler.UpsertSystemTag(dataTransferObject.DataListID, enSystemTag.Error, ex.Message, out errors);

                        ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);

                        return run.DataTransferObject;
                    }
                }

                Interlocked.Decrement(ref Balance);
                dataTransferObject = run.DataTransferObject;
            }

            #region Removed
            //_wfApp.PersistableIdle = (e) =>
            //{
            //    // Round here we are going to create a Resumption point
            //    dataTransferObject = e.GetInstanceExtensions<IDSFDataObject>().ToList().First();


            //    _completedEvent.Set();
            //    //return PersistableIdleAction.Unload;
            //    return PersistableIdleAction.Unload;
            //};



            //_wfApp.Unloaded = (e) =>
            //{
            //    _completedEvent.Set();
            //};

            //_wfApp.Completed = (e) =>
            //{
            //    dataTransferObject = e.GetInstanceExtensions<IDSFDataObject>().ToList().First();

            //    // Travis replace & with &amp;
            //    dataTransferObject.XmlData = dataTransferObject.XmlData.Replace("&", "&amp;");

            //    if (!isDebug)
            //    {
            //        var outputs = e.Outputs;

            //        bool createResumptionPoint = false;

            //        object parentId = outputs["ParentWorkflowInstanceId"];
            //        object parentServiceName = outputs["ParentServiceName"];
            //        object objcreateResumptionPoint;


            //        if (outputs.TryGetValue("CreateResumuptionPoint", out objcreateResumptionPoint))
            //        {
            //            createResumptionPoint = (bool)objcreateResumptionPoint;
            //        }

            //        Activity wfActivity = null;

            //        if (!string.IsNullOrEmpty(parentServiceName == null ? string.Empty : parentServiceName.ToString()) && Guid.TryParse(parentId.ToString(), out parentWorkflowInstanceId))
            //        {
            //            if (parentWorkflowInstanceId != CurrentInstanceId)
            //            {

            //                if (host != null)
            //                {

            //                    var services = host.Services.Where(svc => svc.Name.Equals(parentServiceName == null ? string.Empty : parentServiceName.ToString(), StringComparison.InvariantCultureIgnoreCase));
            //                    if (services.Count() > 0)
            //                    {
            //                        wfActivity = services.First().Actions.First().WorkflowActivity;
            //                        var actionSet = services.First().Actions;
            //                        CurrentInstanceId = parentWorkflowInstanceId;

            //                        if (dataTransferObject.WorkflowResumeable)
            //                        {
            //                            dataTransferObject = InvokeWorkflow(wfActivity, dataTransferObject, executionExtensions, parentWorkflowInstanceId, host, "dsfResumption");
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    _completedEvent.Set();
            //};

            //_wfApp.OnUnhandledException = (e) =>
            //{
            //    _completedEvent.Set();
            //    return UnhandledExceptionAction.Abort;
            //};
            #endregion

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

            private void OnCompleted(WorkflowApplicationCompletedEventArgs args)
            {
                _result = args.GetInstanceExtensions<IDSFDataObject>().ToList().First();
                IDataListCompiler compiler = args.GetInstanceExtensions<IDataListCompiler>().First();
                // PBI : 5376 Removed line below
                //_result.XmlData = _result.XmlData.Replace("&", "&amp;");

                try
                {
                    if(!_isDebug)
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


                        if(outputs.TryGetValue("CreateResumuptionPoint", out objcreateResumptionPoint))
                        {
                            createResumptionPoint = (bool)objcreateResumptionPoint;
                        }

                        PooledServiceActivity wfActivity = null;

                        if(!String.IsNullOrEmpty(parentServiceName == null ? String.Empty : parentServiceName.ToString()) && Guid.TryParse(parentId.ToString(), out _parentWorkflowInstanceID))
                        {
                            if(_parentWorkflowInstanceID != _currentInstanceID)
                            {
                                if(_workspace.Host != null)
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

                                    if(services.Count() > 0)
                                    {
                                        _currentInstanceID = _parentWorkflowInstanceID;
                                        var actionSet = services.First().Actions;

                                        if(_result.WorkflowResumeable)
                                        {
                                            ServiceAction serviceAction = actionSet.First();
                                            wfActivity = serviceAction.PopActivity();

                                            try
                                            {
                                                _result = _owner.InvokeWorkflow(wfActivity.Value, _result, _executionExtensions, _parentWorkflowInstanceID, _workspace, "dsfResumption");
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
                finally
                {
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
