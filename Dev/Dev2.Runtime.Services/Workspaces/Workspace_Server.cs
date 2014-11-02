
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.Hosting;

namespace Dev2.Workspaces
{
    public partial class Workspace
    {
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
                    break;
                case enDynamicServiceObjectType.UnitTest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            #endregion

            ResourceCatalog.Instance.CopyResource(workspaceItem.ID, source.ID, destination.ID, roles);
        }

        #endregion
    }
}
