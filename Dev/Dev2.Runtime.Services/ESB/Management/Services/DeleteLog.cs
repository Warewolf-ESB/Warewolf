/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Services.Security;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    // ReSharper disable once MemberCanBeInternal
    public class DeleteLog : IEsbManagementEndpoint
    {

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string filePath = null;
            string directory = null;

            ExecuteMessage msg = new ExecuteMessage { HasError = false };

            StringBuilder tmp;
            values.TryGetValue("ResourcePath", out tmp);
            if(tmp != null)
            {
                filePath = tmp.ToString();
            }
            values.TryGetValue("Directory", out tmp);
            if(tmp != null)
            {
                directory = tmp.ToString();
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                msg.HasError = true;
                msg.SetMessage(FormatMessage(ErrorResource.CannotDeleteFileWithoutFilename, filePath, directory));
                Dev2Logger.Info(msg.Message.ToString());
            }
            else if(string.IsNullOrWhiteSpace(directory))
            {
                msg.HasError = true;
                msg.SetMessage(FormatMessage(ErrorResource.CannotDeleteFileWithoughtDirectory, filePath, directory));
                Dev2Logger.Info(msg.Message.ToString());
            }
            else if(!Directory.Exists(directory))
            {
                msg.HasError = true;
                msg.SetMessage(FormatMessage(string.Format(ErrorResource.DirectoryDoesNotExist,directory), filePath, directory));
                Dev2Logger.Info(msg.Message.ToString());
            }
            else
            {
                var path = Path.Combine(directory, filePath);

                if(!File.Exists(path))
                {
                    msg.HasError = true;
                    msg.SetMessage(FormatMessage(ErrorResource.FileDoesNotExist, filePath, directory));
                    Dev2Logger.Info(msg.Message.ToString());
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
                        Dev2Logger.Error(ex);
                    }
                }
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDirectoryService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Directory ColumnIODirection=\"Input\"/><ResourcePath ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

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
            return $"DeleteLog: Error deleting '{filePath}' from '{directory}'...{message}";
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Administrator;
        }
    }
}
