
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
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Models;
using Dev2.Studio.Core.ViewModels;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Views.Workflow
// ReSharper restore CheckNamespace
{
    public class DragDropHelpers
    {
        readonly IWorkflowDesignerView _workflowDesignerView;

        public DragDropHelpers(IWorkflowDesignerView workflowDesignerView)
        {
            _workflowDesignerView = workflowDesignerView;
        }


        public bool PreventDrop(IDataObject dataObject)
        {
            if(dataObject == null)
            {
                return false;
            }
            var formats = dataObject.GetFormats();
            //If we didnt attach any data for the format - dont allow
            if(!formats.Any())
            {
                return false;
            }

            //if it is a ReourceTreeViewModel, get the data for this string
            var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ExplorerItemModel", StringComparison.Ordinal) >= 0);
            if(String.IsNullOrEmpty(modelItemString))
            {
                //else if it is a workflowItemType, get data for this
                modelItemString = formats.FirstOrDefault(s => s.IndexOf("WorkflowItemTypeNameFormat", StringComparison.Ordinal) >= 0);

                //else just bounce out, we didnt set it.
                if(!String.IsNullOrEmpty(modelItemString))
                {
                    return false;
                }
            }

            if(string.IsNullOrEmpty(modelItemString))
            {
                return false;
            }
            //now get data for whichever was set above
            var objectData = dataObject.GetData(modelItemString);

            if(objectData == null)
            {
                return false;
            }

            IWorkflowDesignerViewModel workflowDesignerViewModel = _workflowDesignerView.DataContext as IWorkflowDesignerViewModel;
            if(workflowDesignerViewModel != null)
            {
                ExplorerItemModel explorerItemModel = objectData as ExplorerItemModel;
                if(explorerItemModel != null)
                {
                    if(workflowDesignerViewModel.EnvironmentModel.ID != explorerItemModel.EnvironmentId && explorerItemModel.ResourceType >= Common.Interfaces.Data.ResourceType.DbService)
                    {
                        return true;
                    }
                    if (workflowDesignerViewModel.EnvironmentModel.ID != explorerItemModel.EnvironmentId && !workflowDesignerViewModel.EnvironmentModel.IsLocalHostCheck() && explorerItemModel.ResourceType == Common.Interfaces.Data.ResourceType.WorkflowService)
                    {
                        CustomContainer.Get<IPopupController>().Show(Warewolf.Studio.Resources.Languages.Core.DragRemoteNotSupported, Warewolf.Studio.Resources.Languages.Core.DragRemoteNotSupportedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null);
                        return true;
                    }
                    if(explorerItemModel.Permissions >= Permissions.Execute && explorerItemModel.ResourceType <= Common.Interfaces.Data.ResourceType.WebService)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
