using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.Settings;
using Newtonsoft.Json;

namespace Dev2.Runtime.Configuration.Services
{
    public class WebCommunicationService : ICommunicationService
    {
        private readonly WebClient _webClient;

        public WebCommunicationService()
        {
            _webClient = new WebClient();
        }

        public IEnumerable<WorkflowDescriptor> GetResources(string uri)
        {
            var workflowsJSON = _webClient.UploadString(uri, "WorkflowService");
            var workFlowlist = JsonConvert.DeserializeObject<IEnumerable<WorkflowDescriptor>>(workflowsJSON);

            return workFlowlist.ToList();
        }

        public IEnumerable<DataListVariable> GetDataListInputs(string uri, string resourceID)
        {
            var datalistJSON = _webClient.UploadString(uri, resourceID);
            return JsonConvert.DeserializeObject<IEnumerable<DataListVariable>>(datalistJSON);
        }
    }
}
