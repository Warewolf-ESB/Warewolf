using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageWebserviceSourceControl.xaml
    /// </summary>
    public partial class ManageWebserviceSourceControl : IView, ICheckControlEnabledView
    {
        public ManageWebserviceSourceControl()
        {
            InitializeComponent();
            ServerTextBox.Focus();
        }

        public void EnterServerName(string serverName)
        {
            ServerTextBox.Text = serverName;
        }
        
        public void EnterDefaultQuery(string defaultQuery)
        {
            DefaultQueryTextBox.Text = defaultQuery;
        }

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    var viewModel = DataContext as ManageWebserviceSourceViewModel;
                    return viewModel != null && viewModel.OkCommand.CanExecute(null);
                case "Test Connection":
                    return TestConnectionButton.Command.CanExecute(null);
                case "TestQuery":
                    return TestDefault.IsEnabled;
            }
            return false;
        }

        public void SetAuthenticationType(AuthenticationType authenticationType)
        {
            if (authenticationType == AuthenticationType.Anonymous)
            {
                AnonymousRadioButton.IsChecked = true;
            }
            else
            {
                UserRadioButton.IsChecked = true;
            }
        }

        public Visibility GetUsernameVisibility()
        {
            BindingExpression be = UserNamePasswordContainer.GetBindingExpression(VisibilityProperty);
            be?.UpdateTarget();
            return UserNamePasswordContainer.Visibility;
        }

        public Visibility GetPasswordVisibility()
        {
            BindingExpression be = UserNamePasswordContainer.GetBindingExpression(VisibilityProperty);
            be?.UpdateTarget();
            return UserNamePasswordContainer.Visibility;
        }

        public void PerformTestConnection()
        {
            TestConnectionButton.Command.Execute(null);
        }

        public void PerformSave()
        {
            var viewModel = DataContext as ManageWebserviceSourceViewModel;
            viewModel?.OkCommand.Execute(null);
        }

        public void EnterUserName(string userName)
        {
            UserNameTextBox.Text = userName;
        }

        public void EnterPassword(string password)
        {
            PasswordTextBox.Password = password;
        }

        public string GetErrorMessage()
        {
            BindingExpression be = ErrorTextBlock.GetBindingExpression(TextBox.TextProperty);
            be?.UpdateTarget();
            return ErrorTextBlock.Text;
        }

        public string GetAddress()
        {
            return ServerTextBox.Text;
        }

        public string GetDefaultQuery()
        {
            return DefaultQueryTextBox.Text;
        }

        public string GetUsername()
        {
            return UserNameTextBox.Text;
        }

        public string GetPassword()
        {
            return PasswordTextBox.Password;
        }

        public string GetTestDefault()
        {
            BindingExpression be = TestDefault.GetBindingExpression(Hyperlink.NavigateUriProperty);
            be?.UpdateTarget();
            return TestDefault.NavigateUri.ToString();
        }

        public void ClickHyperLink()
        {
            TestDefault.Command.Execute(null);
        }

        public string GetHeaderText()
        {
            BindingExpression be = HeaderTextBlock.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateTarget();
            return HeaderTextBlock.Text;
        }

        public void CancelTest()
        {
            CancelButton.Command.Execute(null);
        }
    }
}
