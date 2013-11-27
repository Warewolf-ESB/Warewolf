using System;
using System.Windows;

namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    /// Interaction logic for DeleteResourceDialog.xaml
    /// </summary>
    public partial class DeleteFolderDialog : Window
    {
        public DeleteFolderDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Title = String.Format(StringResources.DialogTitle_FolderHasDependencies);
            tbDisplay.Text = String.Format(StringResources.DialogBody_FolderContentsHaveDependencies);
        }

        public DeleteFolderDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            tbDisplay.Text = message;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
