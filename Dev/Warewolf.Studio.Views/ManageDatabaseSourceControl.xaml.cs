#pragma warning disable
 using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Studio.ViewModels;
using Dev2.Common;

namespace Warewolf.Studio.Views
{
    public partial class ManageDatabaseSourceControl : IManageDatabaseSourceView, ICheckControlEnabledView
    {
        public ManageDatabaseSourceControl()
        {
            InitializeComponent();
        }

        void EnterServerName(string serverName)
        {
            ServerTextBox.Text = serverName;
        }

        public Visibility GetDatabaseDropDownVisibility()
        {
            var be = DatabaseComboxContainer.GetBindingExpression(VisibilityProperty);
            be?.UpdateTarget();
            return DatabaseComboxContainer.Visibility;
        }

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    var viewModel = DataContext as DatabaseSourceViewModelBase;
                    return viewModel != null && viewModel.OkCommand.CanExecute(null);
                case "Test Connection":
                    return TestConnectionButton.Command.CanExecute(null);
                default:
                    break;
            }
            return false;
        }

        public void SetAuthenticationType(AuthenticationType authenticationType)
        {
            if (authenticationType == AuthenticationType.Windows)
            {
                WindowsRadioButton.IsChecked = true;
            }
            else
            {
                UserRadioButton.IsChecked = true;
            }
        }

        public void SelectDatabase(string databaseName)
        {
            try
            {
                if (DataContext is DatabaseSourceViewModelBase viewModelBase)
                {
                    viewModelBase.DatabaseName = databaseName;
                }
                DatabaseComboxBox.SelectedItem = databaseName;
            }
            catch (Exception)
            {
                //Stupid exception when running from tests
            }
        }

        public void SelectServer(string serverName)
        {
            try
            {
                EnterServerName(serverName);
            }
            catch (Exception e)
            {
                Dev2Logger.Warn(e.Message, "Warewolf Warn");
            }
        }

        public Visibility GetUsernameVisibility()
        {
            var be = UserNamePasswordContainer.GetBindingExpression(VisibilityProperty);
            be?.UpdateTarget();
            return UserNamePasswordContainer.Visibility;
        }

        public Visibility GetPasswordVisibility()
        {
            var be = UserNamePasswordContainer.GetBindingExpression(VisibilityProperty);
            be?.UpdateTarget();
            return UserNamePasswordContainer.Visibility;
        }

        public void PerformTestConnection()
        {
            TestConnectionButton.Command.Execute(null);
        }

        public void PerformSave()
        {
            var viewModel = DataContext as DatabaseSourceViewModelBase;
            viewModel?.OkCommand.Execute(null);
        }
        public void EnterTimeout(int connectionTimeout)
        {
            if (DataContext is DatabaseSourceViewModelBase viewModel)
            {
                viewModel.ConnectionTimeout = connectionTimeout;
            }
            ConnectionTimeoutTextBox.Text = connectionTimeout.ToString();
        }
        public void EnterUserName(string userName)
        {
            if (DataContext is DatabaseSourceViewModelBase viewModel)
            {
                viewModel.UserName = userName;
            }
            UserNameTextBox.Text = userName;
        }

        public void EnterPassword(string password)
        {
            if (DataContext is DatabaseSourceViewModelBase viewModel)
            {
                viewModel.Password = password;
            }
            PasswordTextBox.Password = password;
        }

        public string GetErrorMessage() => ((DatabaseSourceViewModelBase)DataContext).TestMessage;

        void XamComboEditor_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void VerifyServerExistsintComboBox(string serverName)
        {
           
        }

        public IEnumerable<string> GetServerOptions() => new List<string>();

        public void Test()
        {
            TestConnectionButton.Command.Execute(null);
        }

        public string GetUsername() => UserNameTextBox.Text;

        public object GetPassword() => PasswordTextBox.Password;

        public object GetConnectionTimeout() => ConnectionTimeoutTextBox.Text;

        public string GetHeader() => ((DatabaseSourceViewModelBase)DataContext).HeaderText;

        public string GetTabHeader() => ((DatabaseSourceViewModelBase)DataContext).Header;

        public void CancelTest()
        {
            CancelTestButton.Command.Execute(null);
        }

        void ManageServerControl_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowsRadioButton.Focus();
        }

        public void SetDatabaseComboxBindingVisibility(Visibility collapsed)
        {
            DatabaseComboxContainer.Visibility = collapsed;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
