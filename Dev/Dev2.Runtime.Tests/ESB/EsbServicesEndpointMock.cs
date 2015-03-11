
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Control;
using Dev2.Workspaces;

namespace Dev2.Tests.Runtime.ESB
{
    public class EsbServicesEndpointMock : EsbServicesEndpoint
    {
        readonly IEsbServiceInvoker _esbServiceInvoker;

        public EsbServicesEndpointMock(IEsbServiceInvoker esbServiceInvoker)
        {
            VerifyArgument.IsNotNull("esbServiceInvoker", esbServiceInvoker);
            _esbServiceInvoker = esbServiceInvoker;
        }

        protected override IEsbServiceInvoker CreateEsbServicesInvoker(IWorkspace theWorkspace)
        {
            return _esbServiceInvoker;
        }
    }
}
