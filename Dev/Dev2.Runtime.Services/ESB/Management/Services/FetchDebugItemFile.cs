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
                DataListSpecification = "<DataList><DebugItemFilePath ColumnIODirection=\"Input\"></DebugItemFilePath><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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