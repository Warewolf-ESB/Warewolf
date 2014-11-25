
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
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Diagnostics.Logging;
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
            var result = new ExecuteMessage { HasError = false };

            Dev2Logger.Log.Info("Find Log Directory");
            try
            {
                var logdir = WorkflowLoggger.GetDirectoryPath(SettingsProvider.Instance.Configuration.Logging);
                var cleanedDir = CleanUp(logdir);
                result.Message.Append("<JSON>");
                result.Message.Append(@"{""PathToSerialize"":""");
                result.Message.Append(cleanedDir);
                result.Message.Append(@"""}");
                result.Message.Append("</JSON>");    
            }
            catch (Exception ex)
            {
                Dev2Logger.Log.Error(ex);
                result.Message.Append(ex.Message);
                result.HasError = true;
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDirectoryService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            ServiceAction findDirectoryServiceAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

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

        #endregion
    }
}
