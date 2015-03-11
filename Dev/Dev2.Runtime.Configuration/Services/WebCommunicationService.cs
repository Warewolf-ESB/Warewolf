
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dev2.Runtime.Configuration.ComponentModel;
using Newtonsoft.Json;

namespace Dev2.Runtime.Configuration.Services
{
    public class WebCommunicationService : ICommunicationService
    {
        private readonly WebClient _webClient;

        public WebCommunicationService()
        {
            _webClient = new WebClient { Credentials = CredentialCache.DefaultCredentials };
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
