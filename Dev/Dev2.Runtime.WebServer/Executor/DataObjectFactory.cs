/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Principal;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.WebServer.TransferObjects;

namespace Dev2.Runtime.WebServer.Executor
{
    public class DataObjectFactory : IDataObjectFactory
    {
#pragma warning disable CC0044
        public IDSFDataObject New(Guid workspaceGuid, IPrincipal user, string serviceName, WebRequestTO webRequest) =>
#pragma warning restore CC0044
            new DsfDataObject(webRequest.RawRequestPayload, GlobalConstants.NullDataListID, webRequest.RawRequestPayload)
            {
                IsFromWebServer = true,
                ExecutingUser = user,
                ServiceName = serviceName,
                WorkspaceID = workspaceGuid
            };
    }
}
