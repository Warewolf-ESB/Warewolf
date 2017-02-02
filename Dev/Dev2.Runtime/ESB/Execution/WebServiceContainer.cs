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
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Services.Execution;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    public class WebServiceContainer : EsbExecutionContainer
    {
        private readonly IServiceExecution _webserviceExecution;

        public WebServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _webserviceExecution = new WebserviceExecution(dataObj, false);
        }

        public WebServiceContainer(IServiceExecution webServiceExecution)
        {
            _webserviceExecution = webServiceExecution;
        }

        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            _webserviceExecution.InstanceInputDefinitions = InstanceInputDefinition;
            _webserviceExecution.InstanceOutputDefintions = InstanceOutputDefinition;
            var result = _webserviceExecution.Execute(out errors, update);
            return result;
        }

        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext)
        {
            return true;
        }

        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity)
        {
            return null;
        }
    }
}
