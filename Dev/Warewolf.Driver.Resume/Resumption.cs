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
using Dev2.Communication;
using Dev2.Network;
using Dev2.Studio.Interfaces;
using Dev2.Util;
using Warewolf.Common;
using Warewolf.Execution;

namespace Warewolf.Driver.Resume
{
    public interface IResumptionFactory
    {
        IResumption New();
    }

    public class ResumptionFactory : IResumptionFactory
    {
        public IResumption New()
        {
            return new Resumption();
        }
    }

    public class Resumption : IResumption
    {
        private Uri _serverEndpoint;

        private IEnvironmentConnection _environmentConnection;
        private IExecutionLogPublisher _logger;

        private Uri ServerEndpoint
        {
            get
            {
                _serverEndpoint = new Uri($"https://{System.Net.Dns.GetHostName()}:3143");
                return _serverEndpoint;
            }
        }

        public ExecuteMessage Resume(Dictionary<string, StringBuilder> values)
        {
            values.TryGetValue("resourceID", out StringBuilder resourceId);
            values.TryGetValue("environment", out StringBuilder environment);
            values.TryGetValue("startActivityId", out StringBuilder startActivityId);
            values.TryGetValue("versionNumber", out StringBuilder versionNumber);
            values.TryGetValue("currentuserprincipal", out StringBuilder currentuserprincipal);

            var resourceCatalogProxyFactory = new ResourceCatalogProxyFactory();
            var resourceCatalogProxy = resourceCatalogProxyFactory.New(_environmentConnection);
            var executeMessage = resourceCatalogProxy.ResumeWorkflowExecution(resourceId?.ToString(), environment?.ToString(), startActivityId?.ToString(), versionNumber?.ToString(), currentuserprincipal?.ToString());
            return executeMessage;
        }

        public bool Connect(IExecutionLogPublisher executionLogPublisher)
        {
            try
            {
                _logger = executionLogPublisher;
                _logger.Info("Connecting to server: " + ServerEndpoint + "...");
                var serverProxyFactory = new ServerProxyFactory();
                _environmentConnection = serverProxyFactory.New(_serverEndpoint);
                Task<bool> connectTask = TryConnectingToWarewolfServer(_environmentConnection);
                if (connectTask.Result is false)
                {
                    _logger.Error("Connecting to server: " + _serverEndpoint + "... unsuccessful");
                    return false;
                }
                _logger.Info("Connecting to server: " + _serverEndpoint + "... successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Connecting to server: " + _serverEndpoint + "... unsuccessful " + ex.InnerException + " " + ex.InnerException);
                return false;
            }
        }

        private Task<bool> TryConnectingToWarewolfServer(IEnvironmentConnection environmentConnection)
        {
            try
            {
                var connectTask = environmentConnection.ConnectAsync(Guid.Empty);
                connectTask.Wait(600);
                return connectTask;
            }
            catch (Exception ex)
            {
                _logger.Error("Connecting to server: " + _serverEndpoint + "... unsuccessful " + ex.Message + " " + ex.InnerException);
                return Task.FromResult(false);
            }
        }
    }
}