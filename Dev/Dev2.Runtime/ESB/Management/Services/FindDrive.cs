using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// The FindDrive service
    /// </summary>
    public class FindDrive : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {

            string username;
            string domain;
            string password;

            values.TryGetValue("Username", out username);
            values.TryGetValue("Password", out password);
            values.TryGetValue("Domain", out domain);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(domain))
            {
                throw new InvalidDataContractException("Username or Password or Domain not provided");
            }

            IntPtr accessToken = IntPtr.Zero;
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_INTERACTIVE = 2;
            
            StringBuilder result = new StringBuilder();

            try
            {
                if (username.Length > 0)
                {
                    domain = (domain.Length > 0 && domain != ".") ? domain : Environment.UserDomainName;
                    bool success = LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE,
                                             LOGON32_PROVIDER_DEFAULT, ref accessToken);
                    if (success)
                    {
                        var identity = new WindowsIdentity(accessToken);
                        WindowsImpersonationContext context = identity.Impersonate();
                        DriveInfo[] drives = DriveInfo.GetDrives();

                        result.Append("<JSON>");
                        result.Append(GetDriveInfoAsJSON(drives));
                        result.Append("</JSON>");

                        context.Undo();
                    }
                    else
                    {
                        result.Append(Marshal.GetLastWin32Error());
                    }
                }
                else
                {
                    DriveInfo[] drives = DriveInfo.GetDrives();
                    result.Append("<JSON>");
                    result.Append(GetDriveInfoAsJSON(drives));
                    result.Append("</JSON>");
                }
            }
            catch (Exception ex)
            {
                result.Append(ex.Message);
            }

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDriveService = new DynamicService();
            findDriveService.Name = HandlesType();
            findDriveService.DataListSpecification = "<root><Domain/><Username/><Password/></root>";

            ServiceAction findDriveServiceAction = new ServiceAction();
            findDriveServiceAction.Name = HandlesType();
            findDriveServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findDriveServiceAction.SourceMethod = HandlesType();

            findDriveService.Actions.Add(findDriveServiceAction);

            return findDriveService;
        }

        public string HandlesType()
        {
            return "FindDriveService";
        }

        #region Private Methods
        //We use the following to impersonate a user in the current execution environment
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
                                             int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        private string GetDriveInfoAsJSON(DriveInfo[] drives)
        {
            int j = 0;
            string json = "[";
            foreach (DriveInfo drive in drives)
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    var directory = new DirectoryInfo(drive.Name);
                    string name = Regex.Replace(directory.Name, @"\\", @"/");
                    json += @"{""title"":""" + name + @""", ""isFolder"": true, ""key"":""" +
                            name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") +
                            @""", ""isLazy"": true, ""children"": [";
                    // Travis.Frisinger : 20.09.2012 - Removed for speed
                    json += "]}";
                    //if (j < drives.Count() - 1) {
                    json += ",";
                    //}
                    j++;
                }
            }
            json = json.Substring(0, (json.Length - 1));
            json += "]";
            return json;
        }
        #endregion
    }
}
