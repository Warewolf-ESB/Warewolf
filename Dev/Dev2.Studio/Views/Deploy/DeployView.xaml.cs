using System.Collections.Generic;
using Dev2.Models;
using Dev2.Studio.ViewModels.Deploy;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.Deploy
{
    /// <summary>
    /// Interaction logic for DeployView.xaml
    /// </summary>
    public partial class DeployView
    {
        public DeployView()
        {
            InitializeComponent();
        }

        void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if(frameworkElement != null)
            {
                ExplorerItemModel rtvm = frameworkElement.DataContext as ExplorerItemModel;
                if(rtvm != null)
                {
                    DeployViewModel viewModel = DataContext as DeployViewModel;
                    if(viewModel != null)
                    {
                        viewModel.SelectDependencies(new List<IExplorerItemModel> { rtvm });
                    }
                }
            }
        }
    }
}
