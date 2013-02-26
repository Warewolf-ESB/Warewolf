using Dev2.Studio.ViewModels.Workflow;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace Unlimited.Applications.BusinessDesignStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WorkflowDesignerWindow : UserControl
    {

        public WorkflowDesignerWindow()
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
            //IEventAggregator eventAggregator = ImportService.GetExportValue<IEventAggregator>();

            //if (eventAggregator != null)
            //{
            //    eventAggregator.Publish(new AddMissingAndFindUnusedDataListItemsMessage());
            //}
        }
    }
}
