// ReSharper disable CheckNamespace

namespace Dev2.Studio.Views.Diagnostics
{
    /// <summary>
    /// Interaction logic for DebugOutputWindow.xaml
    /// </summary>
    public partial class DebugOutputView
    {

        public DebugOutputView()
        {
            InitializeComponent();
            //DebugOutputTree.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        //        // ReSharper disable InconsistentNaming
        //        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        //        // ReSharper restore InconsistentNaming
        //        {
        //            DebugOutputViewModel viewmodel = DataContext as DebugOutputViewModel;
        //            if(viewmodel != null && viewmodel.ContentItemCount == 0)
        //            {
        //                return;
        //            }
        //            if(viewmodel != null && DebugOutputTree.Items.Count == viewmodel.RootItems.Count && viewmodel.DebugStatus == DebugStatus.Stopping)
        //            {
        //                viewmodel.DebugStatus = DebugStatus.Finished;
        //            }
        //        }

    }
}
