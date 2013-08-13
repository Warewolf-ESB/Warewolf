using System.Windows;

namespace Dev2.Studio.Views.Workflow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WorkflowDesignerView:IWorkflowDesignerView
    {
        readonly DragDropHelpers _dragDropHelpers;

        public WorkflowDesignerView()
        {
            InitializeComponent();
            this.PreviewDrop += DropPointOnDragEnter;
            this.PreviewDragOver += DropPointOnDragEnter;
            _dragDropHelpers = new DragDropHelpers(this);
        }

        //a return from here without settings handled to true and DragDropEffects.None implies that the item drop is allowed
        //TODO Also extract this to some kind of method - AllowDrop and take it out of this codebehind so it can be tested.
        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            var dataObject = e.Data;
            if(_dragDropHelpers.PreventDrop(dataObject))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }            
        }
    }

    public interface IWorkflowDesignerView
    {
        object DataContext { get; set; }
    }
}
