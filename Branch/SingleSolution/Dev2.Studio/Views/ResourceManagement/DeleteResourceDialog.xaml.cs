using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dev2.Common.ExtMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    /// Interaction logic for DeleteResourceDialog.xaml
    /// </summary>
    public partial class DeleteResourceDialog : Window
    {
        private bool _openDependencyGraph = false;

        public bool OpenDependencyGraph { get { return _openDependencyGraph; } }

        public DeleteResourceDialog(IContextualResourceModel model)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Title = String.Format(StringResources.DialogTitle_HasDependencies, model.ResourceType.GetDescription());
            tbDisplay.Text = String.Format(StringResources.DialogBody_HasDependencies, model.ResourceName,
                                                    model.ResourceType.GetDescription());
        }

        public DeleteResourceDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            tbDisplay.Text = message;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            _openDependencyGraph = true;
            DialogResult = false;
        }

        private void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
