

using System.Windows.Input;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfFindRecordsActivityDesigner.xaml
    public partial class DsfReplaceActivityDesigner
    {
        public DsfReplaceActivityDesigner()
        {
            InitializeComponent();
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfReplaceActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
