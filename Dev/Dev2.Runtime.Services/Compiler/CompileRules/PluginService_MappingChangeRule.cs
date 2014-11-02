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
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
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
    internal class PluginService_MappingChangeRule : IServiceCompileRule
    {
        public ServerCompileMessageType HandlesType()
        {
            return ServerCompileMessageType.PluginMappingChangeRule;
        }

        public CompileMessageTO ApplyRule(Guid serviceID, StringBuilder beforeAction, StringBuilder afterAction)
        {
            // Inputs, Outputs ;)
            beforeAction = beforeAction.Replace(GlobalConstants.EmptyNativeTypeTag, ""); // clean up stilly XML tags
            beforeAction = beforeAction.Replace(GlobalConstants.EmptyValidatorTag, ""); // clean up stilly XML tags


            string outputMappings = ServiceUtils.ExtractOutputMapping(beforeAction);
            string inputMappings = ServiceUtils.ExtractInputMapping(beforeAction);

            string outputMappingsPost = ServiceUtils.ExtractOutputMapping(afterAction);
            string inputMappingsPost = ServiceUtils.ExtractInputMapping(afterAction);

            IDev2LanguageParser inputParser = DataListFactory.CreateInputParser();
            IDev2LanguageParser outputParser = DataListFactory.CreateOutputParser();

            IList<IDev2Definition> preInputs = inputParser.Parse(inputMappings);
            IList<IDev2Definition> preOutputs = outputParser.Parse(outputMappings);

            IList<IDev2Definition> postInputs = inputParser.Parse(inputMappingsPost);
            IList<IDev2Definition> postOutputs = outputParser.Parse(outputMappingsPost);

            if (ServiceUtils.MappingValuesChanged(preInputs, postInputs) ||
                ServiceUtils.MappingValuesChanged(preOutputs, postOutputs))
            {
                IList<IDev2Definition> tmpInput = inputParser.Parse(inputMappingsPost);
                IList<IDev2Definition> tmpOutput = outputParser.Parse(outputMappingsPost);

                string defStr = "<Args><Input>" + JsonConvert.SerializeObject(tmpInput) + "</Input><Output>" +
                                JsonConvert.SerializeObject(tmpOutput) + "</Output></Args>";

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