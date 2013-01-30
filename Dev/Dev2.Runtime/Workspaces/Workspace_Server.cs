
using System;
using Dev2.DynamicServices;
using Unlimited.Framework;

namespace Dev2.Workspaces {
    public partial class Workspace {
        #region Host

        /// <summary>
        /// Gets or sets the host - TODO: Remove and use Services instead.
        /// </summary>
        public DynamicServicesHost Host {
            get;
            set;
        }

        #endregion

        #region ServiceRepository

        /// <summary>
        /// Gets or sets the service repository for this workspace.
        /// </summary>
        public IDynamicServiceRepository ServiceRepository {
            get;
            set;
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves the specified object - does not load into.
        /// </summary>
        /// <param name="objectXml">The object's definition XML.</param>
        /// <param name="roles">The roles.</param>
        /// <exception cref="System.ArgumentNullException">objectXml</exception>
        public string Save(string objectXml, string roles = null) {
            if (string.IsNullOrEmpty(objectXml)) {
                throw new ArgumentNullException("objectXml");
            }

            if (roles == null) {
                roles = string.Empty;
            }

            UnlimitedObject unlimitedObject = Host.AddResources(Host.GenerateObjectGraphFromString(objectXml), roles);
            return unlimitedObject.XmlString;
        }

        #endregion

        #region Update

        /// <summary>
        /// Performs the <see cref="IWorkspaceItem.Action" /> on the specified workspace item.
        /// </summary>
        /// <param name="workspaceItem">The workspace item to be actioned.</param>
        /// <param name="roles">The roles.</param>
        /// <exception cref="System.ArgumentNullException">workspaceItem</exception>
        public void Update(IWorkspaceItem workspaceItem, string roles = null) {
            if (workspaceItem == null) {
                throw new ArgumentNullException("workspaceItem");
            }

            if (roles == null) {
                roles = string.Empty;
            }

            switch (workspaceItem.Action) {
                case WorkspaceItemAction.None:
                    break;

                case WorkspaceItemAction.Discard:   // overwrite workspace item with copy of server item
                case WorkspaceItemAction.Edit:      // create copy of the server item in this workspace
                    Copy(WorkspaceRepository.Instance.ServerWorkspace, this, workspaceItem, roles);
                    break;

                case WorkspaceItemAction.Commit:    // overwrite server item with workspace item
                    Copy(this, WorkspaceRepository.Instance.ServerWorkspace, workspaceItem, roles);
                    break;
            }
        }

        #endregion

        #region Copy

        static void Copy(IWorkspace source, IWorkspace destination, IWorkspaceItem workspaceItem, string roles) {
            if (source.Equals(destination)) {
                return;
            }

            // HACK: DynamicServicesHost dependent implementation
            // TODO: Remove DynamicServicesHost dependent implementation
            var serviceType = (enDynamicServiceObjectType)Enum.Parse(typeof(enDynamicServiceObjectType), workspaceItem.ServiceType);
            IDynamicServiceObject dsoServer;
            if ((dsoServer = source.Host.Find(workspaceItem.ServiceName, serviceType)) == null) {
                return;
            }

            var dsoThis = DynamicServiceRepository.DeepCopy(dsoServer);
            switch (serviceType) {
                case enDynamicServiceObjectType.DynamicService:
                    destination.Host.AddDynamicService((DynamicService)dsoThis, roles);
                    break;
                case enDynamicServiceObjectType.Source:
                    destination.Host.AddSource((Source)dsoThis, roles);
                    break;
                case enDynamicServiceObjectType.WorkflowActivity:
                    destination.Host.AddWorkflowActivity((WorkflowActivityDef)dsoThis, roles);
                    break;
            }
        }

        #endregion
    }
}
