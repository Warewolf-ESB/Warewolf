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
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Data.Binary_Objects;
using Dev2.Data.ServiceModel.Helper;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DataList.Contract;
using Newtonsoft.Json;

namespace Dev2.Runtime.Compiler.CompileRules
{
    /// <summary>
    ///     Detect IO mapping changes for WFs
    /// </summary>
    // ReSharper disable InconsistentNaming
    internal class Workflow_MappingChangeRule : IServiceCompileRule
    {
        public ServerCompileMessageType HandlesType()
        {
            return ServerCompileMessageType.WorkflowMappingChangeRule;
        }

        public CompileMessageTO ApplyRule(Guid serviceID, StringBuilder beforeAction, StringBuilder afterAction)
        {
            StringBuilder preDlStr = beforeAction;

            string preDL = ServiceUtils.ExtractDataList(preDlStr);
            string postDL = ServiceUtils.ExtractDataList(afterAction);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();


            IList<IDev2Definition> outputMappings = compiler.GenerateDefsFromDataList(preDL,
                enDev2ColumnArgumentDirection.Output);
            IList<IDev2Definition> inputMappings = compiler.GenerateDefsFromDataList(preDL,
                enDev2ColumnArgumentDirection.Input);

            IList<IDev2Definition> outputMappingsPost = compiler.GenerateDefsFromDataList(postDL,
                enDev2ColumnArgumentDirection.Output);
            IList<IDev2Definition> inputMappingsPost = compiler.GenerateDefsFromDataList(postDL,
                enDev2ColumnArgumentDirection.Input);

            if (inputMappings.Count != inputMappingsPost.Count || outputMappings.Count != outputMappingsPost.Count)
            {
                IList<IDev2Definition> inputDefs = compiler.GenerateDefsFromDataList(postDL,
                    enDev2ColumnArgumentDirection.Input);
                IList<IDev2Definition> outputDefs = compiler.GenerateDefsFromDataList(postDL,
                    enDev2ColumnArgumentDirection.Output);
                string defStr = "<Args><Input>" + JsonConvert.SerializeObject(inputDefs) + "</Input><Output>" +
                                JsonConvert.SerializeObject(outputDefs) + "</Output></Args>";

                return
                    (new CompileMessageTO
                    {
                        MessageID = Guid.NewGuid(),
                        MessageType = CompileMessageType.MappingChange,
                        ServiceID = serviceID,
                        MessagePayload = defStr,
                        ErrorType = ErrorType.Critical
                    });
            }

            if (ServiceUtils.MappingNamesChanged(inputMappings, inputMappingsPost) ||
                ServiceUtils.MappingNamesChanged(outputMappings, outputMappingsPost))
            {
                IList<IDev2Definition> inputDefs = compiler.GenerateDefsFromDataList(postDL,
                    enDev2ColumnArgumentDirection.Input);
                IList<IDev2Definition> outputDefs = compiler.GenerateDefsFromDataList(postDL,
                    enDev2ColumnArgumentDirection.Output);
                string defStr = "<Args><Input>" + JsonConvert.SerializeObject(inputDefs) + "</Input><Output>" +
                                JsonConvert.SerializeObject(outputDefs) + "</Output></Args>";

                return
                    (new CompileMessageTO
                    {
                        MessageID = Guid.NewGuid(),
                        MessageType = CompileMessageType.MappingChange,
                        ServiceID = serviceID,
                        MessagePayload = defStr,
                        ErrorType = ErrorType.Critical
                    });
            }
            return null;
        }
    }
}