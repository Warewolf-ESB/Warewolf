using System;
using System.Windows;
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
            //StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
            //{
            //    DefaultValue = FindResource(typeof(Window))
            //});
            
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
