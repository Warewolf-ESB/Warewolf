
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
using System.Runtime.Serialization;
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
    /// The find directory service
    /// </summary>
    public class FindDirectory : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {

            Dev2Logger.Log.Info("Find Directory");
            string username = string.Empty;
            string domain = string.Empty;
            string password = string.Empty;
            string dir = string.Empty;

            StringBuilder tmp;

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
            values.TryGetValue("DirectoryPath", out tmp);
            if(tmp != null)
            {
                dir = tmp.ToString();
            }

            if(string.IsNullOrEmpty(dir))
            {
                throw new InvalidDataContractException("Directory is required and not provided");
            }

            IntPtr accessToken = IntPtr.Zero;
            // ReSharper disable once InconsistentNaming
// ReSharper disable InconsistentNaming
            const int LOGON32_PROVIDER_DEFAULT = 0;
// ReSharper restore InconsistentNaming
            // ReSharper disable once InconsistentNaming
// ReSharper disable InconsistentNaming
            const int LOGON32_LOGON_INTERACTIVE = 2;
// ReSharper restore InconsistentNaming

            StringBuilder result = new StringBuilder();

            bool isDir = false;
            try
            {
                if(username.Length > 0)
                {
                    domain = (domain.Length > 0 && domain != ".") ? domain : Environment.UserDomainName;
                    bool success = LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE,
                                             LOGON32_PROVIDER_DEFAULT, ref accessToken);
                    if(success)
                    {
                        var identity = new WindowsIdentity(accessToken);
                        WindowsImpersonationContext context = identity.Impersonate();
                        // get the file attributes for file or directory
                        FileAttributes attr = File.GetAttributes(dir);

                        //detect whether its a directory or file
                        if((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            isDir = true;
                        }

                        if(isDir)
                        {
                            var directory = new DirectoryInfo(dir);

                            result.Append("<JSON>");
                            result.Append(GetDirectoryInfoAsJson(directory));
                            result.Append("</JSON>");
                        }
                        else
                        {
                            result.Append("<JSON>");
                            result.Append("[]");
                            result.Append("</JSON>");
                        }
                        context.Undo();
                    }
                    else
                    {
                        result.Append("<Result>Login Failure : Unknown username or password</Result>");
                    }
                }
                else
                {
                    // get the file attributes for file or directory
                    FileAttributes attr = File.GetAttributes(dir);

                    //detect whether its a directory or file
                    if((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        isDir = true;
                    }

                    if(isDir)
                    {
                        var directory = new DirectoryInfo(dir);
                        result.Append("<JSON>");
                        result.Append(GetDirectoryInfoAsJson(directory));
                        result.Append("</JSON>");
                    }
                    else
                    {
                        result.Append("<JSON>");
                        result.Append("[]");
                        result.Append("</JSON>");
                    }
                }
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                result.AppendFormat("Error: {0}", ex.Message);
            }

            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDirectoryService = new DynamicService
                {
                    Name = HandlesType(),
                    DataListSpecification = new StringBuilder("<DataList><Domain ColumnIODirection=\"Input\"/><Username ColumnIODirection=\"Input\"/><Password ColumnIODirection=\"Input\"/><DirectoryPath ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
                };

            ServiceAction findDirectoryServiceAction = new ServiceAction
                {
                    Name = HandlesType(),
                    ActionType = enActionType.InvokeManagementDynamicService,
                    SourceMethod = HandlesType()
                };

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            return findDirectoryService;
        }

        public string HandlesType()
        {
            return "FindDirectoryService";
        }

        #region Private Methods

        //We use the following to impersonate a user in the current execution environment
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
                                             int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        /// <summary>
        /// Gets the directory info as JSON.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        private string GetDirectoryInfoAsJson(DirectoryInfo directory)
        {
           
            int count = 0;
            string json = "[";
            try
            {
                foreach(DirectoryInfo d in directory.GetDirectories())
                {
                    string name = Regex.Replace(d.Name, @"\\", @"\\");
                    json += @"{""title"":""" + name + @""", ""isFolder"": true, ""key"":""" +
                            name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") + @""", ""isLazy"": true}";
                    if(count < (directory.GetDirectories().Length + directory.GetFiles().Length - 1))
                    {
                        json += ',';
                    }
                    count++;
                }

                foreach(FileInfo f in directory.GetFiles())
                {
                    json += @"{""title"":""" + f.Name + @""", ""key"":""" +
                            f.Name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") + @""", ""isLazy"": true}";
                    if(count < (directory.GetDirectories().Length + directory.GetFiles().Length - 1))
                    {
                        json += ',';
                    }
                    count++;
                }
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error(ex);
            }
            json += "]";
            return json;
        }

        #endregion
    }
}
