using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Studio.Core.Activities.Utils;
using System.Windows.Controls;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for MergeWorkflowView.xaml
    /// </summary>
    public partial class MergeWorkflowView : UserControl
    {
        public MergeWorkflowView()
        {
            InitializeComponent();
            //var activity = MergeCurrentTool;
            //var selectAndApplyActivity = new DsfMultiAssignActivity();
            //var modelItem = ModelItemUtils.CreateModelItem(selectAndApplyActivity);
            //MergeCurrentTool.DataContext = new MultiAssignDesignerViewModel(modelItem);
        }

        private void MergeCurrentTool_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var multiAssignActivity = new DsfMultiAssignActivity();
            var modelItem = ModelItemUtils.CreateModelItem(multiAssignActivity);
            var vm = new MultiAssignDesignerViewModel(modelItem);
            MergeCurrentTool.DataContext = vm;
            MergeCurrentTool.Content = vm.ModelItem.Content;
        }
    }
}
