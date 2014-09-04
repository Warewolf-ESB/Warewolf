using System;
using System.Windows;
using System.Windows.Automation;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.ResourceManagement
{
    public interface IResourceChangedDialog
    {
        bool OpenDependencyGraph { get; }
        bool? ShowDialog();
    }

    /// <summary>
    /// Interaction logic for ResourceChangedDialog.xaml
    /// </summary>
    public partial class ResourceChangedDialog : IResourceChangedDialog
    {
        private bool _openDependencyGraph;

        public bool OpenDependencyGraph { get { return _openDependencyGraph; } }

        public ResourceChangedDialog(IContextualResourceModel model, int numOfDependances)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            if(numOfDependances <= 1)
            {
                tbDisplay.Text = String.Format("{0} is used by another workflow. That instance needs to be updated.", model.ResourceName);
                button3.Content = "Open Affected Workflow";
                button3.SetValue(AutomationProperties.AutomationIdProperty, "UI_ShowAffectedWorkflowsButton_AutoID");
            }
            else
            {
                tbDisplay.Text = String.Format("{0} is used in {1} instances. Those instances need to be updated.", model.ResourceName, numOfDependances);
                button3.Content = "Show Affected Workflows";
                button3.SetValue(AutomationProperties.AutomationIdProperty, "UI_ShowAffectedWorkflowsButton_AutoID");
            }
        }

        private void Button3Click(object sender, RoutedEventArgs e)
        {
            _openDependencyGraph = true;
            DialogResult = false;
        }
    }
}
