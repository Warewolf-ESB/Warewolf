using System;
using System.Windows;
using Dev2.Common.ExtMethods;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    /// Interaction logic for DeleteResourceDialog.xaml
    /// </summary>
    public partial class DeleteResourceDialog
    {
        private bool _openDependencyGraph;

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

        private void Button3Click(object sender, RoutedEventArgs e)
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
