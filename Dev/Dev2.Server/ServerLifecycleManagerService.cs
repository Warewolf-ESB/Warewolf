#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Dev2
{
    public class ServerLifecycleManagerService : ServiceBase, IDisposable
    {
        private readonly IServerLifecycleManager _serverLifecycleManager;
        public bool RunSuccessful { get; private set; }

        public ServerLifecycleManagerService()
            :this(new ServerLifecycleManager(new ServerEnvironmentPreparer()))
        {
        }

        public ServerLifecycleManagerService(IServerLifecycleManager serverLifecycleManager)
        {
            serverLifecycleManager.InteractiveMode = false;
            _serverLifecycleManager = serverLifecycleManager;
            CanPauseAndContinue = false;
        }

        protected override void OnStart(string[] args)
        {
            Dev2Logger.Info("** Service Starting **", GlobalConstants.WarewolfInfo);
            RunSuccessful = true;
            // OnStart must return promptly or the Service Control Manager reports 1053
            // ("service did not respond to the start or control request in a timely fashion")
            // and can tear down the process before it finishes initializing. Large resource
            // catalogues (e.g. the Load pipeline's 2000+ resource set) can take well beyond
            // the SCM's default start-request timeout to load. Run() already performs its work
            // on a background Task, so check the outcome asynchronously via a continuation
            // instead of blocking here with Wait().
            _serverLifecycleManager.Run(new LifeCycleInitializationList()).ContinueWith(t =>
            {
                if (EnvironmentVariables.IsServerOnline)
                {
                    Dev2Logger.Info("** Service Started **", GlobalConstants.WarewolfInfo);
                }
                else
                {
                    Stop();
                }
            });
        }

        protected override void OnStop()
        {
            Dev2Logger.Info("** Service Stopped **", GlobalConstants.WarewolfInfo);
            _serverLifecycleManager.Stop(false, 0, false);
        }

        public new void Dispose()
        {
            _serverLifecycleManager.Dispose();
            base.Dispose();
        }
    }
}
