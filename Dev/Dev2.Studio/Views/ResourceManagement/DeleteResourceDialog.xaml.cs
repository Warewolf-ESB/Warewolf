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

namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    /// Interaction logic for DeleteResourceDialog.xaml
    /// </summary>
    public partial class DeleteResourceDialog : Window
    {
        private bool _openDependencyGraph = false;

        public bool OpenDependencyGraph { get { return _openDependencyGraph; } }

        public DeleteResourceDialog()
        {
            InitializeComponent();
        }

        public DeleteResourceDialog(string title, string message, bool allowShowDependency)
        {
            InitializeComponent();
            Title = title;
            tbDisplay.Text = message;

            if (!allowShowDependency)
            {
                button3.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            _openDependencyGraph = true;
            DialogResult = false;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
