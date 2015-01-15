using System;
using System.Windows;
using System.Windows.Input;
using Infragistics.Themes;
using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Events;
using Warewolf.Studio.Themes.Luna;

namespace Warewolf.Studio
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();
            ThemeManager.ApplicationTheme = new LunaTheme(); 
            
        }

        private void ContentPane_MouseLeave(object sender, MouseEventArgs e)
        {
            var contentPane = sender as ContentPane;
            if(contentPane != null)
            {
                contentPane.ExecuteCommand(ContentPaneCommands.FlyIn);
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
    }
}
