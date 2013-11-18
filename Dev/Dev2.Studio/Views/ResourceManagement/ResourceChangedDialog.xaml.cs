using System;
using System.Globalization;
using System.Windows;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

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
    public partial class ResourceChangedDialog : Window, IResourceChangedDialog
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
                tbDisplay.Text = String.Format("{0} is used in {1} instances. Those instances need to be updated.",model.ResourceName,numOfDependances.ToString());
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
