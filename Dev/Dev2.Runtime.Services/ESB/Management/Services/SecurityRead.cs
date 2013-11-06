using System;
using System.Collections.Generic;
using System.IO;
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

        List<WindowsGroupPermission> DefaultPermissions
        {
            get
            {
                var windowsGroupPermissions = new List<WindowsGroupPermission>();
                windowsGroupPermissions.Add(new WindowsGroupPermission
                {
                    IsServer = true,
                    WindowsGroup = "BuiltIn\\Administrators",
                    View = false,
                    Execute = false,
                    Contribute = true,
                    DeployTo = true,
                    DeployFrom = true,
                    Administrator = true
                    
                });

                return windowsGroupPermissions;
            }
        }

        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {

            if(File.Exists("secure.config"))
            {
                var encryptedData = File.ReadAllText("secure.config");
                var decryptData = SecurityEncryption.Decrypt(encryptedData);
                return decryptData;
            }
            return JsonConvert.SerializeObject(DefaultPermissions);
        }

        public DynamicService CreateServiceEntry()
        {
            var dynamicService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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