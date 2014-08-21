using System;
using System.Text;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel.Helper;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DataList.Contract;
using Newtonsoft.Json;
using Dev2.Common;

namespace Dev2.Runtime.Compiler.CompileRules
{
    /// <summary>
    /// Detect IO mapping changes for WFs
    /// </summary>
// ReSharper disable InconsistentNaming
    internal class DBService_MappingChangeRule : IServiceCompileRule
    {
        public ServerCompileMessageType HandlesType()
        {
            return ServerCompileMessageType.DbMappingChangeRule;
        }

        public CompileMessageTO ApplyRule(Guid serviceID, StringBuilder beforeAction, StringBuilder afterAction)
        {
            // Inputs, Outputs ;)
            beforeAction = beforeAction.Replace(GlobalConstants.EmptyNativeTypeTag, ""); // clean up stilly XML tags
            beforeAction = beforeAction.Replace(GlobalConstants.EmptyValidatorTag, ""); // clean up stilly XML tags


            var outputMappings = ServiceUtils.ExtractOutputMapping(beforeAction);
            var inputMappings = ServiceUtils.ExtractInputMapping(beforeAction);

            var outputMappingsPost = ServiceUtils.ExtractOutputMapping(afterAction);
            var inputMappingsPost = ServiceUtils.ExtractInputMapping(afterAction);

            var inputParser = DataListFactory.CreateInputParser();
            var outputParser = DataListFactory.CreateOutputParser();

            var preInputs = inputParser.Parse(inputMappings);
            var preOutputs = outputParser.Parse(outputMappings);

            var postInputs = inputParser.Parse(inputMappingsPost);
            var postOutputs = outputParser.Parse(outputMappingsPost);

            if (ServiceUtils.MappingValuesChanged(preInputs, postInputs) || ServiceUtils.MappingValuesChanged(preOutputs, postOutputs))
            {
                var tmpInput = inputParser.Parse(inputMappingsPost);
                var tmpOutput = outputParser.Parse(outputMappingsPost);

                var defStr = "<Args><Input>" + JsonConvert.SerializeObject(tmpInput) + "</Input><Output>" + JsonConvert.SerializeObject(tmpOutput) + "</Output></Args>";

                return
                    (new CompileMessageTO
                    {
                        MessageType = CompileMessageType.MappingChange,
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
