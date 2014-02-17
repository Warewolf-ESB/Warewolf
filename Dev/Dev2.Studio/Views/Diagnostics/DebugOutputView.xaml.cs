// ReSharper disable once CheckNamespace

using System;
using Dev2.Studio.ViewModels.Diagnostics;
//using System.Windows;

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
            DebugOutputTree.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            DebugOutputViewModel viewmodel = DataContext as DebugOutputViewModel;
            if(viewmodel != null && viewmodel.ContentItemCount == 0)
            {
                return;
            }
            if(viewmodel != null)
            {
                if(DebugOutputTree.Items.Count == viewmodel.ContentItemCount && viewmodel.DebugStatus == DebugStatus.Stopping)
                {
                    viewmodel.DebugStatus = DebugStatus.Finished;
                }
            }
        }

    }
}
