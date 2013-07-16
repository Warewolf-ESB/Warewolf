using System;
using Dev2.Data.Binary_Objects;
using Dev2.Data.ServiceModel.Helper;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using enActionType = Dev2.DynamicServices.enActionType;
using Newtonsoft.Json;

namespace Dev2.Runtime.Compiler.CompileRules
{
    /// <summary>
    /// Detect IO mapping changes for WFs
    /// </summary>
    internal class Workflow_MappingChangeRule : IServiceCompileRule
    {
        public enActionType HandlesType()
        {
            return enActionType.Workflow;
        }

        public CompileMessageTO ApplyRule(Guid serviceID, string beforeAction, string afterAction)
        {
            var preDlStr = beforeAction;

            var preDL = ServiceUtils.ExtractDataList(preDlStr);
            var postDL = ServiceUtils.ExtractDataList(afterAction);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();


            var outputMappings = compiler.GenerateDefsFromDataList(preDL, enDev2ColumnArgumentDirection.Output);
            var inputMappings = compiler.GenerateDefsFromDataList(preDL, enDev2ColumnArgumentDirection.Input);

            var outputMappingsPost = compiler.GenerateDefsFromDataList(postDL, enDev2ColumnArgumentDirection.Output);
            var inputMappingsPost = compiler.GenerateDefsFromDataList(postDL, enDev2ColumnArgumentDirection.Input);

            if(inputMappings.Count != inputMappingsPost.Count || outputMappings.Count != outputMappingsPost.Count)
            {
                var inputDefs = compiler.GenerateDefsFromDataList(postDL, enDev2ColumnArgumentDirection.Input);
                var outputDefs = compiler.GenerateDefsFromDataList(postDL, enDev2ColumnArgumentDirection.Output);
                var defStr = "<Args><Input>" + JsonConvert.SerializeObject(inputDefs) + "</Input><Output>" + JsonConvert.SerializeObject(outputDefs) + "</Output></Args>";

                return (new CompileMessageTO { MessageID = Guid.NewGuid(), MessageType = CompileMessageType.MappingChange, ServiceID = serviceID, MessagePayload = defStr,ErrorType = ErrorType.Critical});
            }

            if (ServiceUtils.MappingsChanged(inputMappings, inputMappingsPost) || ServiceUtils.MappingsChanged(outputMappings, outputMappingsPost))
            {
                var inputDefs = compiler.GenerateDefsFromDataList(postDL, enDev2ColumnArgumentDirection.Input);
                var outputDefs = compiler.GenerateDefsFromDataList(postDL, enDev2ColumnArgumentDirection.Output);
                var defStr = "<Args><Input>" + JsonConvert.SerializeObject(inputDefs) + "</Input><Output>" + JsonConvert.SerializeObject(outputDefs) + "</Output></Args>";

                return (new CompileMessageTO { MessageID = Guid.NewGuid(), MessageType = CompileMessageType.MappingChange, ServiceID = serviceID, MessagePayload = defStr,ErrorType = ErrorType.Critical});
            }
            return null;
        }
    }
}
