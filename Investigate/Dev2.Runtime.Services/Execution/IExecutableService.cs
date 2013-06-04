using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Runtime.Execution
{
    public interface IExecutableService
    {
        Guid ID { get; set; }
        Guid WorkspaceID { get; set; }
        IList<IExecutableService> AssociatedServices { get; }

        void Run();
        Task Terminate();
        void Resume(IDSFDataObject dataObject);
    }
}
