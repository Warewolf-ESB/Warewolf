using System;
using Dev2.Common;
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
    internal class WebService_MappingChangeRule : IServiceCompileRule
    {
        public enActionType HandlesType()
        {
            return enActionType.InvokeWebService;
        }

        public CompileMessageTO ApplyRule(Guid serviceID, string beforeAction, string afterAction)
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

            if (ServiceUtils.MappingsChanged(preInputs, postInputs) || ServiceUtils.MappingsChanged(preOutputs, postOutputs))
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
            //            // are there differences ;)
            //            if (!outputMappings.Equals(outputMappingsPost) || !inputMappings.Equals(inputMappingsPost))
            //            {
            //                var tmpInput = inputParser.Parse(inputMappingsPost);
            //                var tmpOutput = outputParser.Parse(outputMappingsPost);
            //
            //                var defStr = "<Args><Input>" + JsonConvert.SerializeObject(tmpInput) + "</Input><Output>" + JsonConvert.SerializeObject(tmpOutput) + "</Output></Args>";
            //
            //                return
            //                    (new CompileMessageTO
            //                    {
            //                        MessageType = CompileMessageType.MappingChange,
            //                        ServiceID = serviceID,
            //                        MessageID = Guid.NewGuid(),
            //                        MessagePayload = defStr
            //                    });
            //            }

            return null;

        }
    }
}