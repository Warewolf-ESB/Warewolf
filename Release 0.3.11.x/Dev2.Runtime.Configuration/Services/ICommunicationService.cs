using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Runtime.Configuration.ComponentModel;

namespace Dev2.Runtime.Configuration.Services
{
    public interface ICommunicationService
    {
        IEnumerable<WorkflowDescriptor> GetResources(string uri);
        IEnumerable<DataListVariable> GetDataListInputs(string uri, string resourceID);
    }
}
