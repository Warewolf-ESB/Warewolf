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
using System.Threading.Tasks;
using Dev2.Communication;
using Dev2.Network;
using Dev2.Studio.Interfaces;
using Dev2.Util;
using Warewolf.Common;

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

        private Uri ServerEndpoint
        {
            get
            {
                var applicationServerUri = new Uri(string.IsNullOrEmpty(AppUsageStats.LocalHost) ? $"https://{Environment.MachineName.ToLowerInvariant()}:3143" : AppUsageStats.LocalHost);
                _serverEndpoint = new Uri(applicationServerUri.ToString().ToUpper().Replace("localhost".ToUpper(), Environment.MachineName));
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
            var executeMessage = resourceCatalogProxy.ResumeWorkflowExecution(resourceId?.ToString(), environment?.ToString(), startActivityId?.ToString(), versionNumber?.ToString(),currentuserprincipal?.ToString());
            return executeMessage;
        }

        public bool Connect()
        {
            try
            {
                var serverProxyFactory = new ServerProxyFactory();
                _environmentConnection = serverProxyFactory.New(ServerEndpoint);
                Task<bool> connectTask = TryConnectingToWarewolfServer(_environmentConnection);
                if (connectTask.Result is false)
                {
                    //TODO: Add with logging: _logger.Error("Connecting to server: " + _serverEndpoint + "... unsuccessful");
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Task<bool> TryConnectingToWarewolfServer(IEnvironmentConnection environmentConnection)
        {
            try
            {
                var connectTask = environmentConnection.ConnectAsync(Guid.Empty);
                connectTask.Wait();
                return connectTask;
            }
            catch (Exception)
            {
                //TODO: Add with logging: _logger.Error(ex.Message, _options.ServerEndpoint);
                return Task.FromResult(false);
            }
        }
    }
}