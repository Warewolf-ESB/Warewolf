
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
    public class DeleteLog : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string filePath = null;
            string directory = null;

            ExecuteMessage msg = new ExecuteMessage { HasError = false };

            StringBuilder tmp;
            values.TryGetValue("FilePath", out tmp);
            if(tmp != null)
            {
                filePath = tmp.ToString();
            }
            values.TryGetValue("Directory", out tmp);
            if(tmp != null)
            {
                directory = tmp.ToString();
            }

            if(String.IsNullOrWhiteSpace(filePath))
            {
                msg.HasError = true;
                msg.SetMessage(FormatMessage("Can't delete a file if no filename is passed.", filePath, directory));
                Dev2Logger.Log.Info(msg.Message.ToString());
            }
            else if(String.IsNullOrWhiteSpace(directory))
            {
                msg.HasError = true;
                msg.SetMessage(FormatMessage("Can't delete a file if no directory is passed.", filePath, directory));
                Dev2Logger.Log.Info(msg.Message.ToString());
            }
            else if(!Directory.Exists(directory))
            {
                msg.HasError = true;
                msg.SetMessage(FormatMessage("No such directory exists on the server.", filePath, directory));
                Dev2Logger.Log.Info(msg.Message.ToString());
            }
            else
            {
                var path = Path.Combine(directory, filePath);

                if(!File.Exists(path))
                {
                    msg.HasError = true;
                    msg.SetMessage(FormatMessage("No such file exists on the server.", filePath, directory));
                    Dev2Logger.Log.Info(msg.Message.ToString());
                }
                else
                {
                    try
                    {
                        File.Delete(path);
                        msg.SetMessage("Success");
                    }
                    catch(Exception ex)
                    {
                        msg.HasError = true;
                        msg.SetMessage(FormatMessage(ex.Message, filePath, directory));
                        Dev2Logger.Log.Error(ex);
                    }
                }
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDirectoryService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Directory ColumnIODirection=\"Input\"/><FilePath ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            ServiceAction findDirectoryServiceAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            return findDirectoryService;
        }

        public string HandlesType()
        {
            return "DeleteLogService";
        }

        static string FormatMessage(string message, string filePath, string directory)
        {
            return string.Format("DeleteLog: Error deleting '{0}' from '{1}'...{2}", filePath, directory, message);
        }
    }
}
