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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Utils;
using Dev2.Communication;
using Dev2.Network;
using Dev2.Studio.Interfaces;
using Warewolf.Auditing;
using Warewolf.Common;
using Warewolf.Execution;
using LogLevel = Warewolf.Logging.LogLevel;

namespace Warewolf.Driver.Resume
{
    public interface IResumptionFactory
    {
        IResumption New(IExecutionLogPublisher logger);
    }

    public class ResumptionFactory : IResumptionFactory
    {
        public IResumption New(IExecutionLogPublisher logger)
        {
            return new Resumption(logger, new ServerProxyFactory(), new ResourceCatalogProxyFactory());
        }
    }

    public class Resumption : IResumption
    {

        private readonly IExecutionLogPublisher _logger;
        private readonly IServerProxyFactory _serverProxyFactory;
        private readonly IEnvironmentConnection _environmentConnection;
        private readonly IResourceCatalogProxyFactory _resourceCatalogProxyFactory;

        public Resumption(IExecutionLogPublisher logger, IServerProxyFactory serverProxyFactory, IResourceCatalogProxyFactory resourceCatalogProxyFactory)
        {
            _logger = logger;
            VerifyArgument.IsNotNull(nameof(serverProxyFactory), serverProxyFactory);
            VerifyArgument.IsNotNull(nameof(resourceCatalogProxyFactory), resourceCatalogProxyFactory);
            _serverProxyFactory = serverProxyFactory;
            _environmentConnection = _serverProxyFactory.New(ServerEndpoint);
            _resourceCatalogProxyFactory = resourceCatalogProxyFactory;
        }

        private Uri ServerEndpoint => new Uri($"https://{System.Net.Dns.GetHostName()}:3143");

        public ExecuteMessage Resume(Dictionary<string, StringBuilder> values)
        {
            values.TryGetValue("resourceID", out var resourceId);
            values.TryGetValue("environment", out var environment);
            values.TryGetValue("startActivityId", out var startActivityId);
            values.TryGetValue("versionNumber", out var versionNumber);
            values.TryGetValue("currentuserprincipal", out var currentuserprincipal);

            _logger.LogResumedExecution(new Audit
            {
                AuditDate = DateTime.Now,
                WorkflowID = resourceId?.ToString(),
                Environment = string.Empty,
                VersionNumber = versionNumber?.ToString(),
                NextActivityId = startActivityId?.ToString(),
                AuditType = "LogResumeExecutionState",
                LogLevel = LogLevel.Info,
                ExecutingUser = currentuserprincipal?.ToString()
            });

            var resourceCatalogProxy = _resourceCatalogProxyFactory.New(_environmentConnection);
            var executeMessage = resourceCatalogProxy.ResumeWorkflowExecution(resourceId.ToString(), environment.ToString(), startActivityId.ToString(), versionNumber.ToString(), currentuserprincipal.ToString());
            return executeMessage;
        }

        public bool Connect()
        {
            _logger.Info($"Connecting to server: { ServerEndpoint }...");
            var connectTask = TryConnectingToWarewolfServer(_environmentConnection);
            if (connectTask.Result == true)
            {
                _logger.Info($"Connecting to server: { ServerEndpoint }... successful");
                return true;
            }
            return false;
        }

        private Task<bool> TryConnectingToWarewolfServer(IEnvironmentConnection environmentConnection)
        {
            try
            {
                var connectTask = environmentConnection.ConnectAsync(Guid.Empty);
                connectTask.Wait(600);
                return connectTask;
            }
            catch (Exception)
            {
                _logger.Error("Connecting to server: " + ServerEndpoint + "... unsuccessful");
                return Task.FromResult(false);
            }
        }
    }
}