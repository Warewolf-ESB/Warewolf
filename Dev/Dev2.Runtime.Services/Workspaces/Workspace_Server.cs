
using System;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;

namespace Dev2.Workspaces
{
    public partial class Workspace
    {

        #region Save

        /// <summary>
        /// Saves the specified object - does not load into.
        /// </summary>
        /// <param name="objectXml">The object's definition XML.</param>
        /// <param name="roles">The roles.</param>
        /// <exception cref="System.ArgumentNullException">objectXml</exception>
        public string Save(string objectXml, string roles = null)
        {
            if(string.IsNullOrEmpty(objectXml))
            {
                throw new ArgumentNullException("objectXml");
            }

            if(roles == null)
            {
                roles = string.Empty;
            }
            var result = ResourceCatalog.Instance.SaveResource(ID, objectXml, roles);
            return result.Message;
        }

        #endregion

        #region Update

        /// <summary>
        /// Performs the <see cref="IWorkspaceItem.Action" /> on the specified workspace item.
        /// </summary>
        /// <param name="workspaceItem">The workspace item to be actioned.</param>
        /// <param name="isLocalSave">if set to <c>true</c> [is local save].</param>
        /// <param name="roles">The roles.</param>
        /// <exception cref="System.ArgumentNullException">workspaceItem</exception>
        public void Update(IWorkspaceItem workspaceItem, bool isLocalSave, string roles = null)
        {
            if(workspaceItem == null)
            {
                throw new ArgumentNullException("workspaceItem");
            }

            if(roles == null)
            {
                roles = string.Empty;
            }

            switch(workspaceItem.Action)
            {
                case WorkspaceItemAction.None:
                    break;

                case WorkspaceItemAction.Discard:   // overwrite workspace item with copy of server item
                case WorkspaceItemAction.Edit:      // create copy of the server item in this workspace
                    //06.03.2013: Ashley Lewis - PBI 8720
                    if(workspaceItem.ServiceType != enDynamicServiceObjectType.Source.ToString())
                    {
                        Copy(WorkspaceRepository.Instance.ServerWorkspace, this, workspaceItem, roles);
                    }
                    else
                    {
                        Copy(this, WorkspaceRepository.Instance.ServerWorkspace, workspaceItem, roles);
                    }
                    break;

                case WorkspaceItemAction.Commit:    // overwrite server item with workspace item
                    if (!isLocalSave)
                    {
                        Copy(this, WorkspaceRepository.Instance.ServerWorkspace, workspaceItem, roles);
                    }
                    break;
            }
        }

        #endregion

        #region Copy

        static void Copy(IWorkspace source, IWorkspace destination, IWorkspaceItem workspaceItem, string roles)
        {
            if(source.Equals(destination))
            {
                return;
            }

            var resourceType = ResourceType.Unknown;

            enDynamicServiceObjectType serviceType;
            if(!Enum.TryParse(workspaceItem.ServiceType, out serviceType))
            {
                serviceType = enDynamicServiceObjectType.DynamicService;
            }

            #region TODO: Fix Map ResourceType from workspaceItem.ServiceType

            // TODO: FIX mapping ResourceType from workspaceItem.ServiceType
            switch(serviceType)
            {
                case enDynamicServiceObjectType.BizRule:
                    break;
                case enDynamicServiceObjectType.DynamicService:
                    break;
                case enDynamicServiceObjectType.ServiceAction:
                    break;
                case enDynamicServiceObjectType.ServiceActionCase:
                    break;
                case enDynamicServiceObjectType.ServiceActionCases:
                    break;
                case enDynamicServiceObjectType.ServiceActionInput:
                    break;
                case enDynamicServiceObjectType.Source:
                    break;
                case enDynamicServiceObjectType.Validator:
                    break;
                case enDynamicServiceObjectType.WorkflowActivity:
                    resourceType = ResourceType.WorkflowService;
                    break;
                case enDynamicServiceObjectType.UnitTest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            #endregion

            ResourceCatalog.Instance.CopyResource(workspaceItem.ServiceName, resourceType, source.ID, destination.ID, roles);
        }

        #endregion
    }
}
