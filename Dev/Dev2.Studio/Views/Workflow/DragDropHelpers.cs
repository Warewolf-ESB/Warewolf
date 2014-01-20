using System;
using System.Linq;
using System.Windows;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.ViewModels;

// ReSharper disable once CheckNamespace
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
            //If we didnt attach any data for the format - dont allow
            if(!formats.Any())
            {
                return false;
            }

            //if it is a ReourceTreeViewModel, get the data for this string
            var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ResourceTreeViewModel", StringComparison.Ordinal) >= 0);
            if(String.IsNullOrEmpty(modelItemString))
            {
                //else if it is a workflowItemType, get data for this
                modelItemString = formats.FirstOrDefault(s => s.IndexOf("WorkflowItemTypeNameFormat", StringComparison.Ordinal) >= 0);

                //else just bounce out, we didnt set it.
                if(String.IsNullOrEmpty(modelItemString))
                {
                    return false;
                }
            }

            //now get data for whichever was set above
            var objectData = dataObject.GetData(modelItemString);

            if(objectData == null)
            {
                return false;
            }
            //get the contextual resource for the data
            var contextualResourceModel = ResourceHelper.GetContextualResourceModel(objectData);
            if(contextualResourceModel == null)
            {
                return false;
            }

            if(!contextualResourceModel.IsAuthorized(AuthorizationContext.Execute))
            {
                return true;
            }

            //if source dont allow the drop
            if(contextualResourceModel.ResourceType == ResourceType.Source)
            {
                return true;
            }

            //if it is a workflowservice or serverresource, bounce out and allow the drop
            if(contextualResourceModel.ResourceType == ResourceType.WorkflowService ||
               contextualResourceModel.ServerResourceType == ResourceType.WorkflowService.ToString())
            {
                return false;
            }

            //gets the viewmodel on which we gonna drop it.
            var currentViewModel = _workflowDesignerView.DataContext as IWorkflowDesignerViewModel;
            if(currentViewModel == null)
            {
                return false;
            }

            //if the service is from the same environment bounce out and allow it
            if(currentViewModel.EnvironmentModel.ID == contextualResourceModel.Environment.ID)
            {
                return false;
            }
            return true;
        }
    }
}