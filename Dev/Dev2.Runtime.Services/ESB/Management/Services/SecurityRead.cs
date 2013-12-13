using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Services.Security;
using Dev2.Workspaces;

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
                    WindowsGroupPermission.CreateAdministrators(),
                    WindowsGroupPermission.CreateEveryone()
                };
            }
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if(File.Exists(ServerSecurityService.FileName))
            {
                var encryptedData = File.ReadAllText(ServerSecurityService.FileName);
                var decryptData = SecurityEncryption.Decrypt(encryptedData);
                return new StringBuilder(decryptData);
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(DefaultPermissions);
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