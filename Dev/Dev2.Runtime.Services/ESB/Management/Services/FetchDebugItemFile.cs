
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchDebugItemFile : IEsbManagementEndpoint
    {

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {

            Dev2Logger.Log.Info("Fetch Debug Item File Started");
            try
            {

        
            var result = new ExecuteMessage { HasError = false };

            if(values == null)
            {
                Dev2Logger.Log.Debug("values are missing");
                throw new InvalidDataContractException("values are missing");
            }

            StringBuilder tmp;
            values.TryGetValue("DebugItemFilePath", out tmp);
            if(tmp == null || tmp.Length == 0)
            {
                Dev2Logger.Log.Debug("DebugItemFilePath is missing");
                throw new InvalidDataContractException("DebugItemFilePath is missing");
            }

            string debugItemFilePath = tmp.ToString();

            if(File.Exists(debugItemFilePath))
            {
                Dev2Logger.Log.Debug("DebugItemFilePath found");

                var lines = File.ReadLines(debugItemFilePath);
                foreach(var line in lines)
                {
                    result.Message.AppendLine(line);
                }

                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(result);
            }
            Dev2Logger.Log.Debug("DebugItemFilePath not found, throwing an exception");
            throw new InvalidDataContractException(string.Format("DebugItemFilePath {0} not found", debugItemFilePath));
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error(e);
                throw;
            }

        }

        public DynamicService CreateServiceEntry()
        {
            var findDirectoryService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><DebugItemFilePath ColumnIODirection=\"Input\"></DebugItemFilePath><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
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
            return "FetchDebugItemFileService";
        }
    }
}
