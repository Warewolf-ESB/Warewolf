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
        string AuthorRoles { get; set; }
        WorkflowDesigner Designer { get; }
        UIElement DesignerView { get; }
        /// <summary>
        /// Gets the environment model.
        /// </summary>
        /// <value>
        /// The environment model.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        new IEnvironmentModel EnvironmentModel { get; }

        void Dispose();
        bool NotifyItemSelected(object primarySelection);
        void BindToModel();
        void AddMissingWithNoPopUpAndFindUnusedDataListItems();
    }
}