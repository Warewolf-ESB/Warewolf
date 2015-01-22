using System;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Warewolf.Studio.ServerProxyLayer;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class StudioServerProxy:IExplorerRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public StudioServerProxy(ICommunicationControllerFactory controllerFactory,IEnvironmentConnection environmentConnection)
        {
            if (controllerFactory == null)
            {
                throw new ArgumentNullException("controllerFactory");
            }
            if (environmentConnection == null)
            {
                throw new ArgumentNullException("environmentConnection");
            }
            QueryManagerProxy = new QueryManagerProxy(controllerFactory, environmentConnection);
            UpdateManagerProxy = new ExplorerUpdateManagerProxy(controllerFactory,environmentConnection);
        }

        public QueryManagerProxy QueryManagerProxy { get; set; }
        public ExplorerUpdateManagerProxy UpdateManagerProxy { get; set; }

        #region Implementation of IExplorerRepository

        public bool Rename(IExplorerItemViewModel vm, string newName)
        {
            
          UpdateManagerProxy.Rename(vm.ResourceId, newName);
          return true;
      
            
        }

        public bool Move(IExplorerItemViewModel explorerItemViewModel, IExplorerItemViewModel destination)
        {
            UpdateManagerProxy.MoveItem(explorerItemViewModel.ResourceId,destination.ResourceId);
            return true;

        }

        #endregion
    }
}
