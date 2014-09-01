//--------------------------------------------------------------------------------
// This file is part of the downloadable code for the Apress book:
// Pro WF: Windows Workflow in .NET 4.0
// Copyright (c) Bruce Bukovics.  All rights reserved.
//
// This code is provided as is without warranty of any kind, either expressed
// or implied, including but not limited to fitness for any particular purpose. 
// You may use the code for any commercial or noncommercial purpose, and combine 
// it with your own code, but cannot reproduce it in whole or in part for 
// publication purposes without prior approval. 
//--------------------------------------------------------------------------------      

using System;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.Runtime.DurableInstancing;
using System.Threading;
using System.Xml.Linq;

namespace Dev2.DynamicServices
{
    public class FileSystemInstanceStore : InstanceStore
    {
        private readonly Guid _ownerId = Guid.NewGuid();
        private readonly Guid _lockToken = Guid.NewGuid();
        private readonly FileSystemInstanceStoreIO _dataStore;

        public FileSystemInstanceStore()
        {
            _dataStore = new FileSystemInstanceStoreIO();
        }

        #region InstanceStore overrides

        protected override IAsyncResult BeginTryCommand(
            InstancePersistenceContext context,
            InstancePersistenceCommand command,
            TimeSpan timeout, AsyncCallback callback, object state)
        {

            switch(command.GetType().Name)
            {
                case "CreateWorkflowOwnerCommand":

                    Func<Exception> createFunc = () => ProcessCreateWorkflowOwner(context,
                                                                                  command as CreateWorkflowOwnerCommand);

                    return createFunc.BeginInvoke(ar =>
                        {
                            Exception ex = createFunc.EndInvoke(ar);
                            callback(new InstanceStoreAsyncResult(ar, ex));
                        }, state);

                case "LoadWorkflowCommand":
                    Func<Exception> loadFunc = () => ProcessLoadWorkflow(context,
                                                                         command as LoadWorkflowCommand);

                    return loadFunc.BeginInvoke(ar =>
                        {
                            Exception ex = loadFunc.EndInvoke(ar);
                            callback(new InstanceStoreAsyncResult(ar, ex));
                        }, state);

                case "LoadWorkflowByInstanceKeyCommand":
                    Func<Exception> loadByKeyFunc = () => ProcessLoadWorkflowByInstanceKey(context,
                                                                                           command as LoadWorkflowByInstanceKeyCommand);

                    return loadByKeyFunc.BeginInvoke(ar =>
                        {
                            Exception ex = loadByKeyFunc.EndInvoke(ar);
                            callback(new InstanceStoreAsyncResult(ar, ex));
                        }, state);

                case "SaveWorkflowCommand":
                    Func<Exception> saveFunc = () => ProcessSaveWorkflow(context,
                                                                         command as SaveWorkflowCommand);

                    return saveFunc.BeginInvoke(ar =>
                        {
                            Exception ex = saveFunc.EndInvoke(ar);
                            callback(new InstanceStoreAsyncResult(ar, ex));
                        }, state);

                default:
                    return base.BeginTryCommand(
                        context, command, timeout, callback, state);
            }


        }

        protected override bool EndTryCommand(IAsyncResult ar)
        {
            InstanceStoreAsyncResult result = ar as InstanceStoreAsyncResult;
            if(result != null)
            {
                Exception exception = result.Exception;
                if(exception != null)
                {
                    throw exception;
                }
            }

            return true;
        }

        protected override bool TryCommand(
            InstancePersistenceContext context,
            InstancePersistenceCommand command, TimeSpan timeout)
        {
            ManualResetEvent waitEvent = new ManualResetEvent(false);
            IAsyncResult asyncResult = BeginTryCommand(
                context, command, timeout, ar => waitEvent.Set(), null);

            waitEvent.WaitOne(timeout);
            return EndTryCommand(asyncResult);
        }

        #endregion

        #region Command processing

        private Exception ProcessCreateWorkflowOwner(
            InstancePersistenceContext context,
            // ReSharper disable UnusedParameter.Local
            CreateWorkflowOwnerCommand command)
        // ReSharper restore UnusedParameter.Local
        {

            try
            {
                context.BindInstanceOwner(_ownerId, _lockToken);
                return null;
            }
            catch(InstancePersistenceException exception)
            {
                Console.WriteLine(
                    // ReSharper disable LocalizableElement
                    "ProcessCreateWorkflowOwner exception: {0}",
                    // ReSharper restore LocalizableElement
                    exception.Message);
                return exception;
            }

        }

        private Exception ProcessLoadWorkflow(
            InstancePersistenceContext context,
            LoadWorkflowCommand command)
        {


            try
            {
                if(command.AcceptUninitializedInstance)
                {
                    context.LoadedInstance(InstanceState.Uninitialized,
                        null, null, null, null);
                }
                else
                {
                    SharedLoadWorkflow(context, context.InstanceView.InstanceId);
                }
                return null;
            }
            catch(InstancePersistenceException exception)
            {
                Console.WriteLine(
                    // ReSharper disable LocalizableElement
                    "ProcessLoadWorkflow exception: {0}",
                    // ReSharper restore LocalizableElement
                    exception.Message);
                return exception;
            }


        }

        private Exception ProcessLoadWorkflowByInstanceKey(
            InstancePersistenceContext context,
            LoadWorkflowByInstanceKeyCommand command)
        {

            try
            {
                Guid instanceId = _dataStore.GetInstanceAssociation(
                    command.LookupInstanceKey);
                if(instanceId == Guid.Empty)
                {
                    throw new InstanceKeyNotReadyException(
                        String.Format("Unable to load instance for key: {0}",
                            command.LookupInstanceKey));
                }

                SharedLoadWorkflow(context, instanceId);
                return null;
            }
            catch(InstancePersistenceException exception)
            {
                Console.WriteLine(
                    // ReSharper disable LocalizableElement
                    "ProcessLoadWorkflowByInstanceKey exception: {0}",
                    // ReSharper restore LocalizableElement
                    exception.Message);
                return exception;
            }

        }

        private void SharedLoadWorkflow(InstancePersistenceContext context,
            Guid instanceId)
        {
            if(instanceId != Guid.Empty)
            {
                IDictionary<XName, InstanceValue> instanceData;
                IDictionary<XName, InstanceValue> instanceMetadata;
                _dataStore.LoadInstance(instanceId,
                    out instanceData, out instanceMetadata);
                if(context.InstanceView.InstanceId == Guid.Empty)
                {
                    context.BindInstance(instanceId);
                }
                context.LoadedInstance(InstanceState.Initialized,
                    instanceData, instanceMetadata, null, null);
            }
            else
            {
                throw new InstanceNotReadyException(
                    String.Format("Unable to load instance: {0}", instanceId));
            }
        }

        private Exception ProcessSaveWorkflow(
            InstancePersistenceContext context,
            SaveWorkflowCommand command)
        {
            try
            {
                if(command.CompleteInstance)
                {
                    _dataStore.DeleteInstance(
                        context.InstanceView.InstanceId);
                    _dataStore.DeleteInstanceAssociation(
                        context.InstanceView.InstanceId);
                    return null;
                }

                if(command.InstanceData.Count > 0 ||
                    command.InstanceMetadataChanges.Count > 0)
                {
                    if(!_dataStore.SaveAllInstanceData(
                        context.InstanceView.InstanceId, command))
                    {
                        _dataStore.SaveAllInstanceMetaData(
                            context.InstanceView.InstanceId, command);
                    }
                }

                if(command.InstanceKeysToAssociate.Count > 0)
                {
                    foreach(var entry in command.InstanceKeysToAssociate)
                    {
                        _dataStore.SaveInstanceAssociation(
                            context.InstanceView.InstanceId, entry.Key, false);
                    }
                }
                return null;
            }
            catch(InstancePersistenceException exception)
            {
                Console.WriteLine(
                    // ReSharper disable LocalizableElement
                    "ProcessSaveWorkflow exception: {0}", exception.Message);
                // ReSharper restore LocalizableElement
                return exception;
            }
        }

        #endregion

        #region Private types

        private class InstanceStoreAsyncResult : IAsyncResult
        {
            public InstanceStoreAsyncResult(
                IAsyncResult ar, Exception exception)
            {
                AsyncWaitHandle = ar.AsyncWaitHandle;
                AsyncState = ar.AsyncState;
                IsCompleted = true;
                Exception = exception;
            }

            public bool IsCompleted { get; private set; }
            public Object AsyncState { get; private set; }
            public WaitHandle AsyncWaitHandle { get; private set; }
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public bool CompletedSynchronously { get; private set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
            public Exception Exception { get; private set; }
        }

        #endregion
    }
}

