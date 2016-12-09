
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageServerControl.xaml
    /// </summary>
    public partial class ManageServerControl : IView, ICheckControlEnabledView
    {
        public ManageServerControl()
        {
            InitializeComponent();
        }

        public void SelectServer(string serverName)
        {
            try
            {
                EnterServerName(serverName);
            }
            catch (Exception)
            {
                //Stupid exception when running from tests
            }
        }

        public void EnterServerName(string serverName, bool add = false)
        {
            try
            {
                AddressTextBox.Text = serverName;
            }
            catch(Exception)
            {
                //Stupid exception when running from tests
            }
        }

        public void EnterUserName(string username)
        {
            UsernameTextBox.Text = username;
        }

        public void EnterPassword(string password)
        {
            PasswordTextBox.Password = password;
        }

        public string GetPort()
        {
            return PortTextBox.Text;
        }

        public string GetUsername()
        {
            return UsernameTextBox.Text;
        }

        public string GetPassword()
        {
            return PasswordTextBox.Password;
        }

        public void SetAuthenticationType(AuthenticationType authenticationType)
        {
            switch(authenticationType)
            {
                case AuthenticationType.Windows:
                    WindowsRadioButton.IsChecked = true;
                    break;
                case AuthenticationType.User:
                    UserRadioButton.IsChecked = true;
                    break;
                case AuthenticationType.Public:
                    PublicRadioButton.IsChecked = true;
                    break;
                default:
                    WindowsRadioButton.IsChecked = true;
                    break;
            }
        }

        public void SetProtocol(string protocol)
        {
            try
            {
                if (ProtocolItems.Items.Count == 0)
                {
                    BindingExpression be = ProtocolItems.GetBindingExpression(ItemsControl.ItemsSourceProperty);
                    be?.UpdateTarget();
                    ProtocolItems.DataContext = DataContext;                    
                }
                ProtocolItems.SelectedItem = protocol;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }
        }

        public void SetPort(string port)
        {
            try
            {
                PortTextBox.Text = port;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }
        }

        public void PerformSave()
        {
            var viewModel = DataContext as ManageNewServerViewModel;
            viewModel?.OkCommand.Execute(null);
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
        public void TestAction()
        {
            TestConnectionButton.Command.Execute(null);
        }

        public string GetErrorMessage()
        {
            BindingExpression be = ErrorTextBlock.GetBindingExpression(TextBox.TextProperty);
            be?.UpdateTarget();
            return ErrorTextBlock.Text;
        }

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    var viewModel = DataContext as IManageNewServerViewModel;
                    return viewModel != null && viewModel.OkCommand.CanExecute(null);
                case "Test":
                    return TestConnectionButton.Command.CanExecute(null);
            }
            return false;
        }

        #endregion
    }
}
