using System;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.IO;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace Microsoft.Samples.WF.PurchaseProcess {

    public class DSFWorkflowInstanceStore : InstanceStore {


        private Guid _ownerGuid = Guid.NewGuid();


        public DSFWorkflowInstanceStore() {

        }

        protected override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state) {

            switch (command.GetType().Name) {

                case "LoadWorkflowCommand":
                    Func<Exception> loadFunc = () => {
                        return Load(context, command as LoadWorkflowCommand);
                    };

                    return loadFunc.BeginInvoke((ar) => {

                        var ex = loadFunc.EndInvoke(ar);

                        callback(new InstanceStoreAsyncResult(ar, ex));

                    }, state);

                case "LoadWorkflowByInstanceKeyCommand":
                    Func<Exception> loadByKeyFunc = () => {

                        return LoadInstanceByKey(context, command as LoadWorkflowByInstanceKeyCommand);
                    };

                    return loadByKeyFunc.BeginInvoke((ar) => {

                        var ex = loadByKeyFunc.EndInvoke(ar);
                        callback(new InstanceStoreAsyncResult(ar, ex));

                    }, state);

                case "SaveWorkflowCommand":
                    Func<Exception> saveFunc = () => {
                        return Save(context, command as SaveWorkflowCommand);
                    };

                    return saveFunc.BeginInvoke((ar) => {
                        var ex = saveFunc.EndInvoke(ar);
                        callback(new InstanceStoreAsyncResult(ar, ex));

                    }, state);

                case "CreateWorkflowOwnerCommand":
                    Func<Exception> createOwnerFunc = () => {
                        return CreateOwner(context, command as CreateWorkflowOwnerCommand);
                    };

                    return createOwnerFunc.BeginInvoke((ar) => {
                        var ex = createOwnerFunc.EndInvoke(ar);
                        callback(new InstanceStoreAsyncResult(ar, ex));

                    }, state);


                default:
                    return base.BeginTryCommand(context, command, timeout, callback, state);
            }
        }

        private Exception Save(InstancePersistenceContext context, SaveWorkflowCommand command) {

            return null;
        }

        private Exception LoadInstanceByKey(InstancePersistenceContext context, LoadWorkflowByInstanceKeyCommand command){
        
            return null;
        }

        private Exception Load(InstancePersistenceContext context, LoadWorkflowCommand command) {

            return null;
        }

        private Exception CreateOwner(InstancePersistenceContext context, CreateWorkflowOwnerCommand command) {

            return null;
        }        

    }


    class InstanceStoreAsyncResult : IAsyncResult {
        public InstanceStoreAsyncResult(
            IAsyncResult ar, Exception exception) {
            AsyncWaitHandle = ar.AsyncWaitHandle;
            AsyncState = ar.AsyncState;
            IsCompleted = true;
            Exception = exception;
        }

        public bool IsCompleted { get; private set; }
        public Object AsyncState { get; private set; }
        public WaitHandle AsyncWaitHandle { get; private set; }
        public bool CompletedSynchronously { get; private set; }
        public Exception Exception { get; private set; }
    }
}
