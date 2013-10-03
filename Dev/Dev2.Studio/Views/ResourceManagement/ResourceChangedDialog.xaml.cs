using System;
using System.Globalization;
using System.Windows;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    /// Interaction logic for ResourceChangedDialog.xaml
    /// </summary>
    public partial class ResourceChangedDialog : Window
    {
      private bool _openDependencyGraph = false;

        public bool OpenDependencyGraph { get { return _openDependencyGraph; } }

        public ResourceChangedDialog(IContextualResourceModel model, int numOfDependances, string title)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            if(numOfDependances <=1)
            {
                tbDisplay.Text = String.Format("{0} is used by another workflow. That instance needs to be updated.", model.ResourceName);
                button3.Content = "Open Affected Workflow";
            }
            else
            {
                tbDisplay.Text = String.Format("{0} is used by {1} other workflows. Those instances need to be updated.",model.ResourceName,numOfDependances.ToString());
                button3.Content = "Show Affected Workflows";
            }            
        }        

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            _openDependencyGraph = true;
            DialogResult = false;
        }        
    }
}
