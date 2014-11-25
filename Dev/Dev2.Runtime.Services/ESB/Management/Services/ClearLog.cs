
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
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class ClearLog : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage result = new ExecuteMessage { HasError = false };
            StringBuilder msg = new StringBuilder();
            string directory = null;

            StringBuilder tmp;
            values.TryGetValue("Directory", out tmp);
            if(tmp != null)
            {
                directory = tmp.ToString();
            }

            if(String.IsNullOrWhiteSpace(directory))
            {
                AppendError(msg, directory, "Cant delete a file if no directory is passed.");
            }
            else if(!Directory.Exists(directory))
            {
                AppendError(msg, directory, "No such directory exists on the server.");
            }
            else
            {
                try
                {
                    var files = Directory.GetFiles(directory);

                    foreach(var file in files)
                    {
                        File.Delete(file);
                    }

                    msg.Append("Success");
                }
                catch(Exception ex)
                {
                    AppendError(msg, directory, ex.Message);
                    Dev2Logger.Log.Info(ex.StackTrace);
                }
            }

            result.Message.Append(msg);

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            return serializer.SerializeToBuilder(result);

        }

        private static void AppendError(StringBuilder result, string directory, string msg)
        {
            result.AppendFormat("Error clearing '{0}'...", directory);
            result.AppendLine();
            result.Append(String.Format("Error: {0}", msg));
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDirectoryService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Directory ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            ServiceAction findDirectoryServiceAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            return findDirectoryService;
        }

        public string HandlesType()
        {
            return "ClearLogService";
        }

    }
}
