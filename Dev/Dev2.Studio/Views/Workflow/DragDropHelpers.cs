/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Windows;
using Dev2.Studio.Interfaces;
using Warewolf.Studio.ViewModels;


namespace Dev2.Studio.Views.Workflow

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
            //If we didnt attach any data for the format - don't allow
            if(!formats.Any())
            {
                return false;
            }


            //if it is a ReourceTreeViewModel, get the data for this string
            var modelItemString = formats.FirstOrDefault(s => s.IndexOf(@"ExplorerItemViewModel", StringComparison.Ordinal) >= 0);

            if(string.IsNullOrEmpty(modelItemString))
            {
                //else if it is a workflowItemType, get data for this
                modelItemString = formats.FirstOrDefault(s => s.IndexOf(@"WorkflowItemTypeNameFormat", StringComparison.Ordinal) >= 0);

                //else just bounce out, we didnt set it.
                if(!string.IsNullOrEmpty(modelItemString))
                {
                    return false;
                }
            }

            var serviceFromToolBox = formats.FirstOrDefault(s => s.IndexOf(@"FromToolBox", StringComparison.Ordinal) >= 0);
            if (!string.IsNullOrEmpty(serviceFromToolBox))
            {
                return false;
            }


            if (string.IsNullOrEmpty(modelItemString))
            {
                return false;
            }
            //now get data for whichever was set above
            var objectData = dataObject.GetData(modelItemString);

            if(objectData == null)
            {
                return false;
            }

            if (_workflowDesignerView.DataContext is IWorkflowDesignerViewModel workflowDesignerViewModel)
            {
                if (objectData is ExplorerItemViewModel explorerItemViewModel)
                {
                    if (workflowDesignerViewModel.Server.EnvironmentID != explorerItemViewModel.Server.EnvironmentID && !explorerItemViewModel.IsService)
                    {
                        return true;
                    }
                    if (workflowDesignerViewModel.Server.EnvironmentID != explorerItemViewModel.Server.EnvironmentID &&
                        !workflowDesignerViewModel.Server.IsLocalHostCheck() && explorerItemViewModel.IsService)
                    {
                        return true;
                    }

                    if (workflowDesignerViewModel.ResourceModel.ID == explorerItemViewModel.ResourceId)
                    {
                        return true;
                    }

                    if (explorerItemViewModel.CanExecute && explorerItemViewModel.CanView && explorerItemViewModel.IsService && !explorerItemViewModel.IsSource)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
