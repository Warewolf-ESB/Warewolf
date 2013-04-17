using System.Windows.Controls;
using Dev2.Studio.ViewModels.Workflow;
using System.Windows.Input;

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
        }

        void Designer_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var vm = this.DataContext as WorkflowDesignerViewModel;
            if (vm != null)
            {
                vm.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            }
        }
    }
}
