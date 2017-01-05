/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Services.Execution
{
    public class WebserviceExecution : ServiceExecutionAbstract<WebService, WebSource>
    {
        #region Constuctors

        public WebserviceExecution(IDSFDataObject dataObj, bool handlesFormatting)
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

        protected virtual void ExecuteWebRequest(WebService service, out ErrorResultTO errors)
        {
            WebServices.ExecuteRequest(service, true, out errors);
        }

        protected override object ExecuteService(int update, out ErrorResultTO errors, IOutputFormatter formater = null)
        {
            Service.Source = Source;
            ExecuteWebRequest(Service, out errors);
            string result = String.IsNullOrEmpty(Service.JsonPath) || String.IsNullOrEmpty(Service.JsonPathResult)
                ? Scrubber.Scrub(Service.RequestResponse)
                : Scrubber.Scrub(Service.JsonPathResult);
            Service.RequestResponse = null;
            return result;
        }

        #endregion
    }
}
