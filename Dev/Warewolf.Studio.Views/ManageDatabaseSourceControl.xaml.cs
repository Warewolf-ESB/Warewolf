#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
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
