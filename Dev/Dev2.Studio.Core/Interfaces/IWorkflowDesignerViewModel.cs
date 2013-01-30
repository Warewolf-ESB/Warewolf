using System.Activities;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Input;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.ViewModels {
    public interface IWorkflowDesignerViewModel {
        IMediatorRepo MediatorRepo { get; set; }
        WorkflowDesigner wfDesigner { get; }
        bool HasErrors { get; set; }
        IContextualResourceModel ResourceModel { get; }
        object SelectedModelItem { get; }
        string WorkflowName { get; }
        string ServiceDefinition { get; set; }
        bool RequiredSignOff { get; }
        string AuthorRoles { get; set; }
        WorkflowDesigner Designer { get; }
        UIElement DesignerView { get; }
        UIElement PropertyView { get; }
        ICommand NewWorkflowCommand { get; }
        ICommand EditWorkflowCommand { get; }
        void BindToModel();
        void InvalidateUI();
        void Dispose();
        bool NotifyItemSelected(object primarySelection);
        ActivityBuilder GetBaseUnlimitedFlowchartActivity(); 
    }
}