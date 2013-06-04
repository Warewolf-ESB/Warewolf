using System;
using Dev2.Data.ServiceModel.Helper;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using enActionType = Dev2.DynamicServices.enActionType;
using Newtonsoft.Json;

namespace Dev2.Runtime.Compiler.CompileRules
{
    /// <summary>
    /// Detect IO mapping changes for WFs
    /// </summary>
    internal class WfMappingChangeRule : IServiceCompileRule
    {
        public enActionType HandlesType()
        {
            return enActionType.Workflow;
        }

        public CompileMessageTO ApplyRule(Guid serviceID, ServiceAction beforeAction, string afterAction)
        {
            var preDLStr = beforeAction.Parent.XmlString;

            var preDL = ServiceUtils.ExtractDataList(preDLStr);
            var postDL = ServiceUtils.ExtractDataList(afterAction);

            if (!preDL.Equals(postDL))
            {
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                var defs = compiler.GenerateDefsFromDataList(postDL);
                var defStr = JsonConvert.SerializeObject(defs);

                return (new CompileMessageTO() { MessageID = Guid.NewGuid(), MessageType = CompileMessageType.MappingChange, ServiceID = serviceID, MessagePayload = defStr });
            }

            return null;
        }
    }
}
