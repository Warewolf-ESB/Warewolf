using System;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public interface IWebpageRenderWorker {

        Guid UID { get; }

        void WorkerCallback(Object threadContext);
    }
}
