
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
    public class FetchCurrentServerLog : IEsbManagementEndpoint
    {
        readonly string _serverLogPath;

        public FetchCurrentServerLog()
            : this(Path.Combine(EnvironmentVariables.ApplicationPath, "WareWolf-Server.log"))
        {
        }

        public FetchCurrentServerLog(string serverLogPath)
        {
            _serverLogPath = serverLogPath;
        }

        public string ServerLogPath { get { return _serverLogPath; } }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {

            Dev2Logger.Log.Info("Fetch Server Log Started");
            var result = new ExecuteMessage { HasError = false };
            if(File.Exists(_serverLogPath))
            {
                var lines = File.ReadLines(_serverLogPath);

                foreach (var line in lines)
                {
                    result.Message.Append(line);
                }

                File.Delete(_serverLogPath);
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
            }
            catch (Exception err)
            {
                Dev2Logger.Log.Error("Fetch Server Log Error",err);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var findDirectoryService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
            };

            var findDirectoryServiceAction = new ServiceAction
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
            return "FetchCurrentServerLogService";
        }
    }
}
