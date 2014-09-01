using System.Windows;
using Dev2.Common.ExtMethods;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    /// Interaction logic for DeleteResourceDialog.xaml
    /// </summary>
    public partial class RenameResourceDialog
    {
        // ReSharper disable once ConvertToConstant.Local
        private readonly bool _openDependencyGraph = false;

        public bool OpenDependencyGraph { get { return _openDependencyGraph; } }

        public RenameResourceDialog(IContextualResourceModel model, string newName, Window owner)
        {
            InitializeComponent();
            Owner = owner ?? Application.Current.MainWindow;
            Title = string.Format(StringResources.DialogTitle_HasDuplicateName, newName);
            tbDisplay.Text = string.Format(StringResources.DialogBody_HasDuplicateName,
                                model.ResourceType.GetDescription(), model.ResourceName);
        }

        public RenameResourceDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            tbDisplay.Text = message;
        }

        private void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
