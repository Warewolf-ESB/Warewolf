/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Patterns;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Network;
using Dev2.Workspaces;
using Warewolf.Esb;
using Warewolf.Execution;

namespace Dev2.Runtime.ESB.Management
{
    public class InternalExecutionContext : IInternalExecutionContext
    {
        private IWorkspace _workspace;
        private readonly IEsbHub _esbHub;

        public InternalExecutionContext(EsbExecuteRequest request, IWorkspace workspace, IEsbHub esbHub)
        {
            _workspace = workspace;
            Request = request;
            _esbHub = esbHub;
        }

        public void RegisterAsClusterEventListener()
        {
            ClusterDispatcher.Instance.AddListener(_workspace.ID, _esbHub);
        }

        public IEsbRequest Request { get; }

        public object Workspace
        {
            get => _workspace;
            set => _workspace = value as IWorkspace;
        }
    }


    public interface IEsbManagementEndpoint : ISpookyLoadable<string>
    {
        StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace);        
        DynamicService CreateServiceEntry();
        Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs);
        AuthorizationContext GetAuthorizationContextForService();
        bool CanExecute(CanExecuteArg arg);
    }
}
