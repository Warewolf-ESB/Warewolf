/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows;
using System.Windows.Controls;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ElasticsearchSourceControl.xaml
    /// </summary>
    public partial class ElasticsearchSourceControl : IView, ICheckControlEnabledView
    {
        public ElasticsearchSourceControl()
        {
            InitializeComponent();
            ElasticsearchHostNameTextbox.Focus();
        }

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    return DataContext is ElasticsearchSourceViewModel viewModel && viewModel.OkCommand.CanExecute(null);
                case "Test Connection":
                    return TestConnectionButton.Command.CanExecute(null);
                default:
                    break;
            }
            return false;
        }

        public string GetHeaderText()
        {
            var be = HeaderTextBlock.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateTarget();
            return HeaderTextBlock.Text;
        }

        public string GetHostName() => ElasticsearchHostNameTextbox.Text;

        public string GetPassword() => PasswordTextBox.Password;
        
        public string GetUsername() => UserNameTextBox.Text;

        public string GetPort() => ElasticsearchPortTextbox.Text;

        public void CancelTest()
        {
            CancelButton.Command.Execute(null);
        }

        public string GetErrorMessage()
        {
            var be = ErrorTextBlock.GetBindingExpression(TextBox.TextProperty);
            be?.UpdateTarget();
            return ErrorTextBlock.Text;
        }

        public Visibility GetPasswordVisibility()
        {
            var be = PasswordContainer.GetBindingExpression(VisibilityProperty);
            be?.UpdateTarget();
            return PasswordContainer.Visibility;
        }

        public void EnterPassword(string password)
        {
            PasswordTextBox.Password = password;
        }
        public void EnterUsername(string username)
        {
            UserNameTextBox.Text = username;
        }
        public void EnterHostName(string hostName)
        {
            ElasticsearchHostNameTextbox.Text = hostName;
        }

        public void EnterPortNumber(string portNumber)
        {
            ElasticsearchPortTextbox.Text = portNumber;
        }

        public void PerformTestConnection()
        {
            TestConnectionButton.Command.Execute(null);
        }

        public void PerformSave()
        {
            var viewModel = DataContext as ElasticsearchSourceViewModel;
            viewModel?.OkCommand.Execute(null);
        }

        public void SetAuthenticationType(AuthenticationType authenticationType)
        {
            if (authenticationType == AuthenticationType.Anonymous)
            {
                AnonymousRadioButton.IsChecked = true;
            }
            else
            {
                PasswordRadioButton.IsChecked = true;
            }
        }
    }
}
