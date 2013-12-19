using System;
using System.Runtime.DurableInstancing;
using System.Threading;

namespace Dev2.Runtime.ESB.WF {

    public class DSFWorkflowInstanceStore : InstanceStore {

        protected override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state) {

            switch (command.GetType().Name) {

                case "LoadWorkflowCommand":
                    Func<Exception> loadFunc = () => {
                        return Load();
                    };

                    return loadFunc.BeginInvoke(ar => {

                        var ex = loadFunc.EndInvoke(ar);

                        callback(new InstanceStoreAsyncResult(ar, ex));

                    }, state);

                case "LoadWorkflowByInstanceKeyCommand":
                    Func<Exception> loadByKeyFunc = () => LoadInstanceByKey();

                    return loadByKeyFunc.BeginInvoke(ar => {

                        var ex = loadByKeyFunc.EndInvoke(ar);
                        callback(new InstanceStoreAsyncResult(ar, ex));

                    }, state);

                case "SaveWorkflowCommand":
                    Func<Exception> saveFunc = () => Save();

                    return saveFunc.BeginInvoke(ar => {
                        var ex = saveFunc.EndInvoke(ar);
                        callback(new InstanceStoreAsyncResult(ar, ex));

                    }, state);

                case "CreateWorkflowOwnerCommand":
                    Func<Exception> createOwnerFunc = () => CreateOwner();

                    return createOwnerFunc.BeginInvoke(ar => {
                        var ex = createOwnerFunc.EndInvoke(ar);
                        callback(new InstanceStoreAsyncResult(ar, ex));

                    }, state);


                default:
                    return base.BeginTryCommand(context, command, timeout, callback, state);
            }
        }

        private Exception Save() {

            return null;
        }

        private Exception LoadInstanceByKey(){
        
            return null;
        }

        private Exception Load() {

            return null;
        }

        private Exception CreateOwner() {

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
