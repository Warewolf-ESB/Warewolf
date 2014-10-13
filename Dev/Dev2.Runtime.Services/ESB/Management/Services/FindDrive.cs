
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// The FindDrive service
    /// </summary>
    public class FindDrive : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string username = string.Empty;
            string domain = string.Empty;
            string password = string.Empty;
            StringBuilder tmp;
            Dev2Logger.Log.Info("Find Drive");
            values.TryGetValue("Username", out tmp);
            if(tmp != null)
            {
                username = tmp.ToString();
            }
            values.TryGetValue("Password", out tmp);
            if(tmp != null)
            {
                password = tmp.ToString();
            }
            values.TryGetValue("Domain", out tmp);
            if(tmp != null)
            {
                domain = tmp.ToString();
            }

            IntPtr accessToken = IntPtr.Zero;
// ReSharper disable InconsistentNaming
            const int LOGON32_PROVIDER_DEFAULT = 0;
// ReSharper restore InconsistentNaming
// ReSharper disable InconsistentNaming
            const int LOGON32_LOGON_INTERACTIVE = 2;
// ReSharper restore InconsistentNaming

            StringBuilder result = new StringBuilder();

            try
            {
                if(!string.IsNullOrEmpty(username))
                {
                    domain = (domain.Length > 0 && domain != ".") ? domain : Environment.UserDomainName;
                    bool success = LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE,
                                             LOGON32_PROVIDER_DEFAULT, ref accessToken);
                    if(success)
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
                        result.Append("<result>Logon failure: unknown user name or bad password</result>");
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
            catch(Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                result.Append(ex.Message);
            }

            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDriveService = new DynamicService
                {
                    Name = HandlesType(),
                    DataListSpecification = new StringBuilder("<DataList><Domain ColumnIODirection=\"Input\"/><Username ColumnIODirection=\"Input\"/><Password ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
                };

            ServiceAction findDriveServiceAction = new ServiceAction
                {
                    Name = HandlesType(),
                    ActionType = enActionType.InvokeManagementDynamicService,
                    SourceMethod = HandlesType()
                };

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

// ReSharper disable InconsistentNaming
        private static string GetDriveInfoAsJSON(IEnumerable<DriveInfo> drives)
// ReSharper restore InconsistentNaming
        {
            string json = "[";
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(DriveInfo drive in drives)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if(drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Network)
                {
                    var directory = new DirectoryInfo(drive.Name);
                    string name = Regex.Replace(directory.Name, @"\\", @"/");
                    json += @"{""driveLetter"":""" + name + @""", ""isFolder"": true, ""key"":""" +
                            name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") +
                            @""", ""isLazy"": true, ""title"": """ + name[0] + @":""},";
                }
            }
            json = json.Substring(0, (json.Length - 1));
            json += "]";
            return json;
        }
        #endregion
    }
}
