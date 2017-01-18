using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Dev2.UI;
using Infragistics.Controls.Menus;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManagePluginSourceControl.xaml
    /// </summary>
    public partial class ManagePluginSourceControl : IView, ICheckControlEnabledView
    {
        public ManagePluginSourceControl()
        {
            InitializeComponent();
        }

        public string GetHeaderText()
        {
            BindingExpression be = HeaderTextBlock.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateTarget();
            return HeaderTextBlock.Text;
        }


        public IDllListingModel SelectItem(string itemName)
        {

            var xamDataTreeNode = GetItem(itemName);
            return xamDataTreeNode?.Data as IDllListingModel;
        }

        public bool IsItemVisible(string itemName)
        {
            var xamDataTreeNode = GetItem(itemName);
            return xamDataTreeNode != null;
        }

        XamDataTreeNode GetItem(string itemName)
        {
            return null;
        }

        public string GetAssemblyName()
        {
            BindingExpression be = AssemblyNameTextBox.GetBindingExpression(TextBox.TextProperty);
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
            BindingExpression be = AssemblyNameTextBox.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateSource();
        }

        public IDllListingModel OpenItem(string itemNameToOpen)
        {
            var xamDataTreeNode = GetItem(itemNameToOpen);
            if (xamDataTreeNode != null)
            {
                xamDataTreeNode.IsExpanded = true;
            }
            return xamDataTreeNode?.Data as IDllListingModel;
        }

        public void SetTextBoxValue(string controlName, string input)
        {
            switch (controlName)
            {
                case "AssemblyName":
                    AssemblyNameTextBox.Text = input;
                    BindingExpression assem = AssemblyNameTextBox.GetBindingExpression(TextBlock.TextProperty);
                    assem?.UpdateSource();
                    break;
                case "ConfigFile":
                    ConfigFileTextbox.Text = input;
                    BindingExpression config = ConfigFileTextbox.GetBindingExpression(TextBlock.TextProperty);
                    config?.UpdateSource();
                    break;
                case "GacAssemblyName":
                    GacAssemblyNameTextBox.Text = input;
                    BindingExpression gac = GacAssemblyNameTextBox.GetBindingExpression(TextBlock.TextProperty);
                    gac?.UpdateSource();
                    break;
            }
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
                case "AssemblyNameButton":
                    return AssemblyNameButton.IsEnabled;
                case "ConfigFileButton":
                    return ConfigFileButton.IsEnabled;
                case "GacAssemblyNameButton":
                    return GacAssemblyNameButton.IsEnabled;
            }
            return false;
        }

        private void GacAssemblyNameTextBox_OnTextChanged(object sender, RoutedEventArgs e)
        {
            var intellisenseTextBox = sender as IntellisenseTextBox;
            if (intellisenseTextBox != null)
            {
                if (string.IsNullOrWhiteSpace(intellisenseTextBox.Text))
                {
                    if (Application.Current != null)
                        intellisenseTextBox.Style = Application.Current.TryFindResource("AutoCompleteBoxStyle") as Style;
                }
                else
                {
                    if (Application.Current != null)
                        intellisenseTextBox.Style = Application.Current.TryFindResource("DisabledAutoCompleteBoxStyle") as Style;
                }
            }
        }
    }
}
