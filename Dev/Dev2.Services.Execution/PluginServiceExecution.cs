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
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;

namespace Dev2.Services.Execution
{
    public class PluginServiceExecution : ServiceExecutionAbstract<PluginService, PluginSource>
    {
        #region Constructors

        public PluginServiceExecution(IDSFDataObject dataObj, bool handlesFormatting)
            : base(dataObj, handlesFormatting)
        {
        }

        #endregion

        #region Execute

        public override void BeforeExecution(ErrorResultTO errors)
        {
        }

        public override void AfterExecution(ErrorResultTO errors)
        {
        }

        protected override object ExecuteService(List<MethodParameter> methodParameters, out ErrorResultTO errors, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();

            var args = new PluginInvokeArgs
            {
                AssemblyLocation = Source.AssemblyLocation,
                AssemblyName = Source.AssemblyName,
                Fullname = Service.Namespace,
                Method = Service.Method.Name,
                Parameters = Service.Method.Parameters,
                OutputFormatter = formater
            };

            object result = null;

            try
            {
                result = PluginServiceExecutionFactory.InvokePlugin(args);
            }
            catch (Exception e)
            {
                errors.AddError(e.Message);
            }

            return result;
        }

        #endregion
    }
}
