using Dev2.Studio.ViewModels.Deploy;
using Dev2.Studio.ViewModels.Navigation;
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
                ResourceTreeViewModel rtvm = frameworkElement.DataContext as ResourceTreeViewModel;
                if(rtvm != null)
                {
                    DeployViewModel viewModel = DataContext as DeployViewModel;
                    if(viewModel != null)
                    {
                        viewModel.SelectDependencies(rtvm.DataContext);
                    }
                }
            }
        }
    }
}
