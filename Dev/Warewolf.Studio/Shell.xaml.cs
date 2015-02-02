using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Effects;
using Dev2.Common.Interfaces;
using Infragistics.Windows;
using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Events;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell
    {
        public Shell(IShellViewModel shellViewModel)
        {
            InitializeComponent();
            DataContext = shellViewModel;            
            Loaded+=OnLoaded;
           // Explorer.Effect = new BlurEffect(){Radius = 10};
        }
            
        void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            LoadShellViewModel();
        }

        protected virtual void LoadShellViewModel()
        {
            var viewModel = DataContext as ShellViewModel;
            if (viewModel != null)
            {
                viewModel.Initialize();
            }
        }

        private void ContentPane_MouseLeave(object sender, MouseEventArgs e)
        {
            var contentPane = sender as ContentPane;
            if(contentPane != null)
            {
                contentPane.ExecuteCommand(ContentPaneCommands.FlyIn);
            }
        }

        private void PaneTabItem_Loaded(object sender, RoutedEventArgs e)
        {
            var paneTabItem = sender as PaneTabItem;
            if(paneTabItem != null)
            {
                paneTabItem.Margin = new Thickness(0, 12, 0, 0);
            }
        }

        private void UnpinnedTabArea_Loaded(object sender, RoutedEventArgs e)
        {
            var repeatButton = Utilities.GetDescendantFromName(sender as DependencyObject, "PART_ScrollDown") as RepeatButton;
            if(repeatButton != null)
            {
                repeatButton.Visibility = Visibility.Collapsed;
            }
        }

        void DockManager_OnPaneDragStarting(object sender, PaneDragStartingEventArgs e)
        {
            var dragPane = e.RootPane as PaneHeaderPresenter;
            if (dragPane != null)
            {
                var content = dragPane.Content as string;
                if (!String.IsNullOrEmpty(content) && (content.ToLowerInvariant() == "menu" || content.ToLowerInvariant() == "help"))
                {
                    e.Handled = true;
                    e.Cancel = true;
                }
            }
        }

        void PaneResize(object sender, MouseButtonEventArgs e)
        {
            //e.Handled = true;
        }

        void SlidingMenuPane_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var vm = DataContext as ShellViewModel;
            if (vm != null)
            {
                vm.MenuPanelWidth = e.NewSize.Width;
            }
        }
    }
}
