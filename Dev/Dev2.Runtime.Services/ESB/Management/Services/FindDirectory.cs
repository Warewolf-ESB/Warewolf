using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Dev2.DynamicServices;
using System.Text;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// The find directory service
    /// </summary>
    public class FindDirectory : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string username;
            string domain;
            string password;
            string dir;

            values.TryGetValue("Username", out username);
            values.TryGetValue("Password", out password);
            values.TryGetValue("Domain", out domain);
            values.TryGetValue("DirectoryPath", out dir);

            if (string.IsNullOrEmpty(dir))
            {
                throw new InvalidDataContractException("Directory is required and not provided");
            }


            IntPtr accessToken = IntPtr.Zero;
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_INTERACTIVE = 2;
            
            StringBuilder result = new StringBuilder();

            bool isDir = false;
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
                        // get the file attributes for file or directory
                        FileAttributes attr = File.GetAttributes(dir);

                        //detect whether its a directory or file
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            isDir = true;
                        }

                        if (isDir)
                        {
                            var directory = new DirectoryInfo(dir);

                            result.Append("<JSON>");
                            result.Append(GetDirectoryInfoAsJSON(directory));
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
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        isDir = true;
                    }


                    if (isDir)
                    {
                        var directory = new DirectoryInfo(dir);
                        result.Append("<JSON>");
                        result.Append(GetDirectoryInfoAsJSON(directory));
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
            catch (Exception ex)
            {
                result.Append(ex.Message);
            }

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDirectoryService = new DynamicService();
            findDirectoryService.Name = HandlesType();
            findDirectoryService.DataListSpecification = "<root><Domain/><Username/><Password/><DirectoryPath/></root>";

            ServiceAction findDirectoryServiceAction = new ServiceAction();
            findDirectoryServiceAction.Name = HandlesType();
            findDirectoryServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findDirectoryServiceAction.SourceMethod = HandlesType();

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
        private string GetDirectoryInfoAsJSON(DirectoryInfo directory)
        {
            int count = 0;
            string name = string.Empty;
            string json = "[";
            try
            {
                foreach (DirectoryInfo d in directory.GetDirectories())
                {
                    name = Regex.Replace(d.Name, @"\\", @"\\");
                    json += @"{""title"":""" + name + @""", ""isFolder"": true, ""key"":""" +
                            name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") + @""", ""isLazy"": true}";
                    if (count < (directory.GetDirectories().Length + directory.GetFiles().Length - 1))
                    {
                        json += ',';
                    }
                    count++;
                }

                foreach (FileInfo f in directory.GetFiles())
                {
                    json += @"{""title"":""" + f.Name + @""", ""key"":""" +
                            f.Name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") + @""", ""isLazy"": true}";
                    if (count < (directory.GetDirectories().Length + directory.GetFiles().Length - 1))
                    {
                        json += ',';
                    }
                    count++;
                }
            }
            catch
            {
            }
            json += "]";
            return json;
        }

        #endregion
    }
}
