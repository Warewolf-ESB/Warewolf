using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Services.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Dev2.Runtime.Services
{
    public class ServiceActions : ExceptionManager
    {
        public string List(string source, Guid workspaceID, Guid dataListID)
        {
            //TODO
            //1. Hydrate source string into a source data model
            //2. Get a list of actions for that source
            //3. Create instances for ServiceAction for each action
            //4. Return the JSON representation of the service actions

            List<ServiceActionWrapper> actions = new List<ServiceActionWrapper>();
            actions.Add(new ServiceActionWrapper { Name = "Action1"});
            actions.Add(new ServiceActionWrapper { Name = "Action2" });
            actions.Add(new ServiceActionWrapper { Name = "Action3" });

            return JsonConvert.SerializeObject(actions);
        }
    }
}
