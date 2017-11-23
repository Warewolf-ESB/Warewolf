using System.Windows;
using System.Windows.Controls;
using Dev2.Common.Interfaces;
using Dev2.UI;
using Infragistics.Controls.Menus;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    public partial class ManagePluginSourceControl : IView, ICheckControlEnabledView
    {
        public ManagePluginSourceControl() => InitializeComponent();

        public string GetHeaderText()
        {
            var be = HeaderTextBlock.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateTarget();
            return HeaderTextBlock.Text;
        }

        public string GetAssemblyName()
        {
            var be = AssemblyNameTextBox.GetBindingExpression(TextBox.TextProperty);
            be?.UpdateTarget();
            return AssemblyNameTextBox.Text;
        }

        public void PerformSave()
        {
            var viewModel = DataContext as ManagePluginSourceViewModel;
            viewModel?.OkCommand.Execute(null);
        }

        public void SetAssemblyName(string assemblyName)
        {
            AssemblyNameTextBox.Text = assemblyName;
            var be = AssemblyNameTextBox.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateSource();
        }

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    var viewModel = DataContext as ManagePluginSourceViewModel;
                    return viewModel != null && viewModel.OkCommand.CanExecute(null);
                case "AssemblyName":
                    return AssemblyNameTextBox.IsEnabled;
                case "ConfigFile":
                    return ConfigFileTextbox.IsEnabled;
                case "GacAssemblyName":
                    return GacAssemblyNameTextBox.IsEnabled;
                case nameof(AssemblyNameButton):
                    return AssemblyNameButton.IsEnabled;
                case nameof(ConfigFileButton):
                    return ConfigFileButton.IsEnabled;
                case nameof(GacAssemblyNameButton):
                    return GacAssemblyNameButton.IsEnabled;
                default:
                    break;
            }
            return false;
        }

        void GacAssemblyNameTextBox_OnTextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is IntellisenseTextBox intellisenseTextBox)
            {
                if (string.IsNullOrWhiteSpace(intellisenseTextBox.Text))
                {
                    if (Application.Current != null)
                    {
                        intellisenseTextBox.Style = Application.Current.TryFindResource("AutoCompleteBoxStyle") as Style;
                    }
                }
                else
                {
                    if (Application.Current != null)
                    {
                        intellisenseTextBox.Style = Application.Current.TryFindResource("DisabledAutoCompleteBoxStyle") as Style;
                    }
                }
            }
        }
    }
}
