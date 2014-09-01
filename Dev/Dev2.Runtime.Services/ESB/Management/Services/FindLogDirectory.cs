using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Configuration;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// The find directory service
    /// </summary>
    public class FindLogDirectory : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var result = new ExecuteMessage() { HasError = false };

            try
            {
                var logdir = ServerLogger.GetDirectoryPath(SettingsProvider.Instance.Configuration.Logging);
                var cleanedDir = CleanUp(logdir);
                result.Message.Append("<JSON>");
                result.Message.Append(@"{""PathToSerialize"":""");
                result.Message.Append(cleanedDir);
                result.Message.Append(@"""}");
                result.Message.Append("</JSON>");    
            }
            catch (Exception ex)
            {
                result.Message.Append(ex.Message);
                result.HasError = true;
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
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
