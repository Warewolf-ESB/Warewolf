/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Services.Security;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class GetLogFileServiceHandler : AbstractWebRequestHandler
    {
        public GetLogFileServiceHandler()
            : base(ResourceCatalog.Instance, TestCatalog.Instance, TestCoverageCatalog.Instance, new DefaultEsbChannelFactory(), new SecuritySettings())
        {
        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            ctx.Send(new FileResponseWriter(EnvironmentVariables.ServerLogFile));
        }
    }
}
