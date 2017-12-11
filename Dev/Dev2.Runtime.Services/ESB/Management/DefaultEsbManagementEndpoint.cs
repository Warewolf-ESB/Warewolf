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
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public abstract class DefaultEsbManagementEndpoint : IEsbManagementEndpoint
    {
        public abstract StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace);
        public abstract DynamicService CreateServiceEntry();
        public abstract string HandlesType();
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }
        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }
    }
}