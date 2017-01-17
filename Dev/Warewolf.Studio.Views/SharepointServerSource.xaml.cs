/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
    /// Interaction logic for SharepointServerSource.xaml
    /// </summary>
    public partial class SharepointServerSource : IView, ICheckControlEnabledView
    {
        public SharepointServerSource()
        {
            InitializeComponent();
        }


        public string GetHeaderText()
        {
            BindingExpression be = HeaderTextBlock.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateTarget();
            return HeaderTextBlock.Text;
        }

        public void EnterServerName(string serverName)
        {
            ServerName.Text = serverName;
        }

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    var viewModel = DataContext as SharepointServerSourceViewModel;
                    return viewModel != null && viewModel.SaveCommand.CanExecute(null);
                case "Test Connection":
                    return TestConnection.Command.CanExecute(null);
            }
            return false;
        }

        #endregion

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
            TestConnection.Command.Execute(null);
        }

        public void PerformSave()
        {
            var viewModel = DataContext as SharepointServerSourceViewModel;
            viewModel?.SaveCommand.Execute(null);
        }

        public string GetErrorMessage()
        {
            BindingExpression be = ErrorTextBlock.GetBindingExpression(TextBox.TextProperty);
            be?.UpdateTarget();
            return ErrorTextBlock.Text;
        }

        public void CancelTest()
        {
            CancelButton.Command.Execute(null);
        }

        public void EnterUserName(string userName)
        {
            UserNameTextBox.Text = userName;
        }

        public void EnterPassword(string password)
        {
            PasswordTextBox.Password = password;
        }

        public string GetUsername()
        {
            return UserNameTextBox.Text;
        }

        public string GetPassword()
        {
            return PasswordTextBox.Password;
        }

        public string GetAddress()
        {
            return ServerName.Text;
        }
    }
}
