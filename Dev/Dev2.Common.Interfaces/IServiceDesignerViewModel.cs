using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface IWorkflowServiceDesignerViewModel : IServiceDesignerViewModel
    {
        /// <summary>
        /// Should the hyperlink to execute the service in browser
        /// </summary>
        bool IsServiceLinkVisible { get; }
        /// <summary>
        /// Command to execute when the hyperlink is clicked
        /// </summary>
        ICommand OpenWorkflowLinkCommand { get; }
        /// <summary>
        /// The hyperlink text shown
        /// </summary>
        string DisplayWorkflowLink { get; }
        /// <summary>
        /// The designer for the resource
        /// </summary>
        UIElement DesignerView { get; }
        bool IsNewWorkflow { get; set; }
    }

    public interface IServiceDesignerViewModel
    {
        /// <summary>
        /// The resource that is being represented
        /// </summary>
        IResource Resource { get; set; }       
    }

}