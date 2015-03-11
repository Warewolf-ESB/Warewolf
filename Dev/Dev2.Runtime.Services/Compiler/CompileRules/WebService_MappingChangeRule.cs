
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
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
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
    internal class WebService_MappingChangeRule : IServiceCompileRule
    {
        public ServerCompileMessageType HandlesType()
        {
            return ServerCompileMessageType.WebServiceMappingChangeRule;
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
