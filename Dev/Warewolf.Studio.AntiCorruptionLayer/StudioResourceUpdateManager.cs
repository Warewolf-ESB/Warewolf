using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Warewolf.Studio.ServerProxyLayer;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class StudioResourceUpdateManager : IStudioUpdateManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public StudioResourceUpdateManager(ICommunicationControllerFactory controllerFactory, IEnvironmentConnection environmentConnection)
        {
            if (controllerFactory == null)
            {
                throw new ArgumentNullException("controllerFactory");
            }
            if (environmentConnection == null)
            {
                throw new ArgumentNullException("environmentConnection");
            }

             UpdateManagerProxy = new UpdateProxy(controllerFactory, environmentConnection);

        }

        IUpdateManager UpdateManagerProxy { get; set; }

        public void Save(IServerSource serverSource)
        {

            UpdateManagerProxy.SaveServerSource(serverSource, GlobalConstants.ServerWorkspaceID);
        }



        public string TestConnection(IServerSource serverSource)
        {


            return UpdateManagerProxy.TestConnection(serverSource);

        }

        public IList<string> TestDbConnection(IDbSource serverSource)
        {
            return UpdateManagerProxy.TestDbConnection(serverSource);
        }

        public void Save(IDbSource toDbSource)
        {
            UpdateManagerProxy.SaveDbSource( toDbSource, GlobalConstants.ServerWorkspaceID);

        }

        public void Save(IDatabaseService toDbSource)
        {
            UpdateManagerProxy.SaveDbService(toDbSource);
        }

        public DataTable TestDbService(IDatabaseService inputValues)
        {
            return UpdateManagerProxy.TestDbService(inputValues);
        }
    }
}