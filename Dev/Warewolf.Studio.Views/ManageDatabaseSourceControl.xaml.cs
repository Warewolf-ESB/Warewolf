﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Runtime.ServiceModel;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageServerControl.xaml
    /// </summary>
    public partial class ManageDatabaseSourceControl:IManageDatabaseSourceView,ICheckControlEnabledView
    {
        public ManageDatabaseSourceControl()
        {
            InitializeComponent();
            ServerTextBox.Focus();
        }

        public void EnterServerName(string serverName)
        {
            ServerTextBox.EmptyText = serverName;
        }

        public Visibility GetDatabaseDropDownVisibility()
        {
            BindingExpression be = DatabaseComboxContainer.GetBindingExpression(VisibilityProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return DatabaseComboxContainer.Visibility;
        }

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    return SaveButton.Command.CanExecute(null);
                case "Test Connection":
                    return TestConnectionButton.Command.CanExecute(null);
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
            DatabaseComboxBox.SelectedItem = databaseName;
        }

        public Visibility GetUsernameVisibility()
        {
            BindingExpression be = UserNamePasswordContainer.GetBindingExpression(VisibilityProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return UserNamePasswordContainer.Visibility;
        }

        public Visibility GetPasswordVisibility()
        {
            BindingExpression be = UserNamePasswordContainer.GetBindingExpression(VisibilityProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return UserNamePasswordContainer.Visibility;
        }

        public void PerformTestConnection()
        {
            TestConnectionButton.Command.Execute(null);
        }

        public void PerformSave()
        {
            SaveButton.Command.Execute(null);
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
            return ErrorTextBlock.Text;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}