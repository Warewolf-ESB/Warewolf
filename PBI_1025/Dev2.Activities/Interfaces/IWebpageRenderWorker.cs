using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public interface IWebpageRenderWorker {

        Guid UID { get; }

        void WorkerCallback(Object threadContext);
    }
}
