using System.Activities.Presentation.View;
using System.Collections.ObjectModel;
using System.Windows;
using Dev2.Common.Interfaces;
using Infragistics.DragDrop;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for WorkflowDesignerView.xaml
    /// </summary>
    public partial class WorkflowDesignerView
    {
        public WorkflowDesignerView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) =>
            {
                var viewModel = args.NewValue as IWorkflowServiceDesignerViewModel;
                if (viewModel != null)
                {
//                    DropTarget target = new DropTarget();
//                    target.DropChannels = new ObservableCollection<string> { "Tool" };
//                    target.IsDropTarget = true;
                    var designerView = viewModel.DesignerView;
                    designerView.AllowDrop = true;
                    //DragDropManager.SetDropTarget(designerView, target);                    
                }

            };
            
            
        }
    }
}
