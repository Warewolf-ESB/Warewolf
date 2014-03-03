using System;
using System.Collections.Generic;

namespace Dev2.Runtime.Execution
{
    public interface IExecutableService
    {
        Guid ID { get; set; }
        Guid WorkspaceID { get; set; }
        IList<IExecutableService> AssociatedServices { get; }
        Guid ParentID { get; set; }

        void Run();
        void Terminate();
        void Resume(IDSFDataObject dataObject);
        void Terminate(Exception exception);
    }
}
