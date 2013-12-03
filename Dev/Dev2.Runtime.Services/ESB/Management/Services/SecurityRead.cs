using System.Collections.Generic;
using System.IO;
using Dev2.DynamicServices;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class SecurityRead : IEsbManagementEndpoint
    {
        static List<WindowsGroupPermission> DefaultPermissions
        {
            get
            {
                return new List<WindowsGroupPermission>
                {
                    WindowsGroupPermission.CreateDefault()
                };
            }
        }

        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            if(File.Exists(SecurityConfigService.FileName))
            {
                var encryptedData = File.ReadAllText(SecurityConfigService.FileName);
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