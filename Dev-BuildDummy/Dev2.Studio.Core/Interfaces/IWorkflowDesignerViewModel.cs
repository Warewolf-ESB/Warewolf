using System.Activities;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Input;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.ViewModels {
    public interface IWorkflowDesignerViewModel : IDesignerViewModel 
    {
        bool HasErrors { get; set; }
        object SelectedModelItem { get; }
        string WorkflowName { get; }
        bool RequiredSignOff { get; }
        string AuthorRoles { get; set; }
        WorkflowDesigner Designer { get; }
        UIElement DesignerView { get; }
        void Dispose();
        bool NotifyItemSelected(object primarySelection);
        ActivityBuilder GetBaseUnlimitedFlowchartActivity();
        void BindToModel();
        void AddMissingWithNoPopUpAndFindUnusedDataListItems();
    }
}