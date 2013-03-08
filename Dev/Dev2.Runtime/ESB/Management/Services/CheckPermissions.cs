using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.DynamicServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Checks a users permissions on the local file system
    /// </summary>
    public class CheckPermissions : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            StringBuilder result = new StringBuilder();
            IDynamicServicesHost theHost = theWorkspace.Host;

            string username;
            string path;
            string password;

            values.TryGetValue("Username", out username);
            values.TryGetValue("Password", out password);
            values.TryGetValue("Path", out path);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(path))
            {
                throw new InvalidDataContractException("Path or Username or Password is missing");
            }

            if (username == string.Empty)
            {
                if (!FileIO.CheckPermissions(WindowsIdentity.GetCurrent(), path, FileSystemRights.Read) &&
                    !FileIO.CheckPermissions(WindowsIdentity.GetCurrent(), path, FileSystemRights.ReadData))
                {
                    result.Append(string.Concat("Insufficient permission for '", path, "'."));
                }
            }
            else
            {
                // we have a username and password set :)

                if (!FileIO.CheckPermissions(username, password, path, FileSystemRights.Read) &&
                    FileIO.CheckPermissions(username, password, path, FileSystemRights.ReadData))
                {
                    result.Append("<Errors>");
                    result.Append(string.Concat("Insufficient permission for '", path, "'."));
                    result.Append("</Error>");
                }
            }

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService checkPermissionsService = new DynamicService();
            checkPermissionsService.Name = HandlesType();
            checkPermissionsService.DataListSpecification = "<root><Path/><Username/><Password/></root>";

            ServiceAction checkPermissionsServiceAction = new ServiceAction();
            checkPermissionsServiceAction.Name = HandlesType();
            checkPermissionsServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            checkPermissionsServiceAction.SourceMethod = HandlesType();

            checkPermissionsService.Actions.Add(checkPermissionsServiceAction);

            return checkPermissionsService;
        }

        public string HandlesType()
        {
            return "CheckPermissionsService";
        }
    }
}
