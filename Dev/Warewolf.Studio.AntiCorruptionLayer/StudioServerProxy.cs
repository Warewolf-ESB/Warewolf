using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Versioning;
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
            VersionManager = new VersionManagerProxy(environmentConnection, controllerFactory); //todo:swap
        }

        public Dev2.Common.Interfaces.ServerProxyLayer.IVersionManager VersionManager { get; set; }
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

        public bool Delete(IExplorerItemViewModel explorerItemViewModel)
        {
            UpdateManagerProxy.DeleteResource(explorerItemViewModel.ResourceId);
            return true;
        }



        public ICollection<IVersionInfo> GetVersions(Guid id)
        {
            return new List<IVersionInfo>(VersionManager.GetVersions(id).Select(a => a.VersionInfo));
        }

        public IRollbackResult Rollback(Guid resourceId, string version)
        {
           return  VersionManager.RollbackTo(resourceId,version);
        }

        #endregion
    }
}
