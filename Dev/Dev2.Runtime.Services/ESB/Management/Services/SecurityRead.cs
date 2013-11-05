using System;
using System.Collections.Generic;
using Dev2.Data.Settings.Security;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class SecurityRead : IEsbManagementEndpoint
    {
        public List<WindowsGroupPermission> Permissions { get; private set; }

        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            Permissions = new List<WindowsGroupPermission>();
            
            // TODO: Read permissions
            // NOTE: If config does not exist MUST ALWAYS auto-add "BuiltIn\\Administrators" permission below
            Permissions.Add(new WindowsGroupPermission
            {
                IsServer = true, WindowsGroup = "BuiltIn\\Administrators",
                View = false, Execute = false, Contribute = true, DeployTo = true, DeployFrom = true, Administrator = true
            });

            Permissions.AddRange(new[]
            {
                new WindowsGroupPermission 
                { 
                    IsServer = true, WindowsGroup = "Deploy Admins", 
                    View = false, Execute = false, Contribute = false, DeployTo = true, DeployFrom = true, Administrator = false 
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 1", View = false, Execute = true, Contribute = false
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 2", View = false, Execute = false, Contribute = true
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow2",
                    WindowsGroup = "Windows Group 1", View = true, Execute = true, Contribute = false
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow3",
                    WindowsGroup = "Windows Group 3", View = true, Execute = false, Contribute = false
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow3",
                    WindowsGroup = "Windows Group 4", View = false, Execute = true, Contribute = false
                },


                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow4",
                    WindowsGroup = "Windows Group 3", View = false, Execute = false, Contribute = true
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 4", View = false, Execute = false, Contribute = true
                }
            });

            return JsonConvert.SerializeObject(Permissions);
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Security/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
            };

            var serviceAction = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            dynamicService.Actions.Add(serviceAction);

            return dynamicService;
        }

        public string HandlesType()
        {
            return "SecurityReadService";
        }
    }
}