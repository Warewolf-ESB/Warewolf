using System.Activities.Presentation;
using System.Windows;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.ViewModels
{
    public interface IWorkflowDesignerViewModel : IDesignerViewModel
    {
        bool HasErrors { get; set; }
        object SelectedModelItem { get; }
        string WorkflowName { get; }
        bool RequiredSignOff { get; }
        WorkflowDesigner Designer { get; }
        UIElement DesignerView { get; }

        void Dispose();
        bool NotifyItemSelected(object primarySelection);
        void BindToModel();
        void AddMissingWithNoPopUpAndFindUnusedDataListItems();
    }
}