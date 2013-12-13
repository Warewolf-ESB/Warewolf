using System;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel.Helper;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using Newtonsoft.Json;
using enActionType = Dev2.DynamicServices.enActionType;

namespace Dev2.Runtime.Compiler.CompileRules
{
    /// <summary>
    /// Detect IO mapping changes for WFs
    /// </summary>
    internal class DBService_IsRequiredChangeRule : IServiceCompileRule
    {
        public ServerCompileMessageType HandlesType()
        {
            return ServerCompileMessageType.DbIsRequireChangeRule;
        }

        public CompileMessageTO ApplyRule(Guid serviceID, StringBuilder beforeAction, StringBuilder afterAction)
        {
            // Inputs, Outputs ;)
            var inputMappingsPost = ServiceUtils.ExtractInputMapping(afterAction);

            var inputParser = DataListFactory.CreateInputParser();
            
            var postInputs = inputParser.Parse(inputMappingsPost);

            if (postInputs.Any(definition => definition.IsRequired))
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
            return null;

        }
    }
}