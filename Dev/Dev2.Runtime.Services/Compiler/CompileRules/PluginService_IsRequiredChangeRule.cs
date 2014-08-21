using System;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel.Helper;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DataList.Contract;
using Newtonsoft.Json;

namespace Dev2.Runtime.Compiler.CompileRules
{
    /// <summary>
    /// Detect IO mapping changes for WFs
    /// </summary>
    // ReSharper disable InconsistentNaming
    internal class PluginService_IsRequiredChangeRule : IServiceCompileRule
    {
        public ServerCompileMessageType HandlesType()
        {
            return ServerCompileMessageType.PluginIsRequiredChangeRule;
        }

        public CompileMessageTO ApplyRule(Guid serviceID, StringBuilder beforeAction, StringBuilder afterAction)
        {
            // Inputs, Outputs ;)
            var inputMappingsPost = ServiceUtils.ExtractInputMapping(afterAction);

            var inputParser = DataListFactory.CreateInputParser();

            var postInputs = inputParser.Parse(inputMappingsPost);

            if(postInputs.Any(definition => definition.IsRequired))
            {
                var tmpInput = inputParser.Parse(inputMappingsPost);

                var defStr = "<Args><Input>" + JsonConvert.SerializeObject(tmpInput) + "</Input></Args>";

                return
                    (new CompileMessageTO
                    {
                        MessageType = CompileMessageType.MappingIsRequiredChanged,
                        ServiceID = serviceID,
                        MessageID = Guid.NewGuid(),
                        MessagePayload = defStr,
                        ErrorType = ErrorType.Critical
                    });
            }
            else
            {
                var tmpInput = inputParser.Parse(inputMappingsPost);

                var defStr = "<Args><Input>" + JsonConvert.SerializeObject(tmpInput) + "</Input></Args>";

                return
                    (new CompileMessageTO
                    {
                        MessageType = CompileMessageType.MappingIsRequiredChanged,
                        ServiceID = serviceID,
                        MessageID = Guid.NewGuid(),
                        MessagePayload = defStr,
                        ErrorType = ErrorType.None
                    });
            }

        }
    }
}