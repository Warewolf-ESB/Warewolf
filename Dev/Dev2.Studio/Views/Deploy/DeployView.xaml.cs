
using System.Windows;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.Studio.ViewModels.Navigation;

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
                    DeployViewModel vm = DataContext as DeployViewModel;
                    if(vm != null)
                    {
                        vm.SelectDependencies(rtvm.DataContext as IContextualResourceModel);
                    }
                }
            }
        }
    }
}
