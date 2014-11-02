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
using System.Linq;
using System.Text;
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
    internal class DBService_IsRequiredChangeRule : IServiceCompileRule
    {
        public ServerCompileMessageType HandlesType()
        {
            return ServerCompileMessageType.DbIsRequireChangeRule;
        }

        public CompileMessageTO ApplyRule(Guid serviceID, StringBuilder beforeAction, StringBuilder afterAction)
        {
            // Inputs, Outputs ;)
            string inputMappingsPost = ServiceUtils.ExtractInputMapping(afterAction);

            IDev2LanguageParser inputParser = DataListFactory.CreateInputParser();

            IList<IDev2Definition> postInputs = inputParser.Parse(inputMappingsPost);

            if (postInputs.Any(definition => definition.IsRequired))
            {
                IList<IDev2Definition> tmpInput = inputParser.Parse(inputMappingsPost);

                string defStr = "<Args><Input>" + JsonConvert.SerializeObject(tmpInput) + "</Input></Args>";

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
                IList<IDev2Definition> tmpInput = inputParser.Parse(inputMappingsPost);

                string defStr = "<Args><Input>" + JsonConvert.SerializeObject(tmpInput) + "</Input></Args>";

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