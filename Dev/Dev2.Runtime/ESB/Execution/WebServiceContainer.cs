
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
using Dev2.DataList.Contract;
using Dev2.DynamicServices.Objects;
using Dev2.Services.Execution;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    // BUG 9619 - 2013.06.05 - TWR - Refactored
    public class WebServiceContainer : EsbExecutionContainer
    {
        protected IServiceExecution WebserviceExecution;

        public WebServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            WebserviceExecution = new WebserviceExecution(dataObj, false);
        }

        public WebServiceContainer(IServiceExecution webServiceExecution)
        {
            WebserviceExecution = webServiceExecution;
        }

        public override Guid Execute(out ErrorResultTO errors)
        {
            WebserviceExecution.InstanceOutputDefintions = InstanceOutputDefinition;
            var result = WebserviceExecution.Execute(out errors);
            return result;
        }
    }
}
