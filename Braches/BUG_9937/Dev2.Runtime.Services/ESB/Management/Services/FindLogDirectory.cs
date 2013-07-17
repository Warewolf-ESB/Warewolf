using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.DynamicServices;
using System.Text;
using Dev2.Runtime.Configuration;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// The find directory service
    /// </summary>
    public class FindLogDirectory : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            var result = new StringBuilder();

            try
            {
                var logdir = ServerLogger.GetDirectoryPath(SettingsProvider.Instance.Configuration.Logging);
                var cleanedDir = CleanUp(logdir);
                result.Append("<JSON>");
                result.Append(@"{""PathToSerialize"":""");
                result.Append(cleanedDir);
                result.Append(@"""}");
                result.Append("</JSON>");    
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
            findDirectoryService.DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";

            ServiceAction findDirectoryServiceAction = new ServiceAction();
            findDirectoryServiceAction.Name = HandlesType();
            findDirectoryServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findDirectoryServiceAction.SourceMethod = HandlesType();

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            return findDirectoryService;
        }

        public string HandlesType()
        {
            return "FindLogDirectoryService";
        }

        #region Private Methods

        private object CleanUp(string logdir)
        {
            return Regex.Replace(logdir, @"\\", @"/");
        }

        //We use the following to impersonate a user in the current execution environment
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
                                             int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

    
        #endregion
    }
}
