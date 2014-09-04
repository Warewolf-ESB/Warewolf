// ReSharper disable CheckNamespace

using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

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
        }

        void DebugOutputGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            (sender as Grid).Parent.SetValue(AutomationProperties.AutomationIdProperty, "UI_DebugOutputGrid_AutoID");
        }
    }
}
