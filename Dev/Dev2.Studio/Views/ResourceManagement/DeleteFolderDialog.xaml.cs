using System;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    /// Interaction logic for DeleteResourceDialog.xaml
    /// </summary>
    public partial class DeleteFolderDialog
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
    }
}
