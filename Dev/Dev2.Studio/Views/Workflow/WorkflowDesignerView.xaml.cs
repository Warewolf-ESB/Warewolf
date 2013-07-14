
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Utils.ActivityDesignerUtils;
using Dev2.Studio.ViewModels.Workflow;

namespace Dev2.Studio.Views.Workflow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WorkflowDesignerView
    {
        public WorkflowDesignerView()
        {
            InitializeComponent();
            this.PreviewDrop += DropPointOnDragEnter;
            this.PreviewDragOver += DropPointOnDragEnter;
        }

        //a return from here without settings handled to true and DragDropEffects.None implies that the item drop is allowed
        //TODO this logic seems faulty. Please verify with Barney what should be allowed to drop and what not.
        //TODO Also extract this to some kind of method - AllowDrop and take it out of this codebehind so it can be tested.
        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            var formats = e.Data.GetFormats();
            //If we didnt attach any data for the format - dont allow
            if (!formats.Any())
            {
                return;
            }

            //if it is a ReourceTreeViewModel, get the data for this string
            var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ResourceTreeViewModel", StringComparison.Ordinal) >= 0);
            if (String.IsNullOrEmpty(modelItemString))
            {
                //else if it is a workflowItemType, get data for this
                modelItemString = formats.FirstOrDefault(s => s.IndexOf("WorkflowItemTypeNameFormat", 
                    StringComparison.Ordinal) >= 0);

                //else just bounce out, we didnt set it.
                if (String.IsNullOrEmpty(modelItemString))
                {
                    return;
                }
            }

            //now get data for whichever was set above
            var objectData = e.Data.GetData(modelItemString);

            //get the contextual resource for the data
            var contextualResourceModel = ResourceHelper.GetContextualResourceModel(objectData);
            if (contextualResourceModel == null)
            {
                return;
            }

            //if it is a workflowservice or serverresource, bounce out and allow the drop
            if (contextualResourceModel.ResourceType == ResourceType.WorkflowService ||
                contextualResourceModel.ServerResourceType == ResourceType.WorkflowService.ToString())
            {
                return;
            }

            //gets the viewmodel on which we gonna drop it.
            var currentViewModel = (WorkflowDesignerViewModel)this.DataContext;
            if (currentViewModel == null)
            {
                return;
            }

            //if the resource is from the same environment bounce out and allow it
            if(currentViewModel.EnvironmentModel.ID == contextualResourceModel.Environment.ID)
            {
                return;
            }

            //else dont allow it.
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        void WorkflowDesignerView_OnMouseMove(object sender, MouseEventArgs e)
        {
            var i = 0;
            var willfail = 1 / i;
        }
    }
}
