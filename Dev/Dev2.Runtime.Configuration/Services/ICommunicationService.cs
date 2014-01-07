using System.Collections.Generic;
using Dev2.Runtime.Configuration.ComponentModel;

namespace Dev2.Runtime.Configuration.Services
{
    public interface ICommunicationService
    {
        IEnumerable<WorkflowDescriptor> GetResources(string uri);
        IEnumerable<DataListVariable> GetDataListInputs(string uri, string resourceID);
    }
}
