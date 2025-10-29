#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ChatbotSource.xaml
    /// </summary>
    public partial class ChatbotSource : IView, ICheckControlEnabledView
    {
        public ChatbotSource()
        {
            InitializeComponent();
        }

        public string GetHeaderText()
        {
            var be = HeaderTextBlock.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateTarget();
            return HeaderTextBlock.Text;
        }

        public void EnterCompletionsEndpoint(string endpoint)
        {
            CompletionsEndpointTextBox.Text = endpoint;
        }

        public void EnterApiKey(string apiKey)
        {
            ApiKeyPasswordBox.Text = apiKey;
        }

        public string GetCompletionsEndpoint() => CompletionsEndpointTextBox.Text;

        public string GetApiKey() => ApiKeyPasswordBox.Text;

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    var viewModel = DataContext as ChatbotSourceViewModel;
                    return viewModel != null && viewModel.SaveCommand.CanExecute(null);
                case "Test Connection":
                    return TestConnectionButton.Command.CanExecute(null);
                default:
                    break;
            }
            return false;
        }

        #endregion

        public void PerformTestConnection()
        {
            TestConnectionButton.Command.Execute(null);
        }

        public void PerformSave()
        {
            var viewModel = DataContext as ChatbotSourceViewModel;
            viewModel?.SaveCommand.Execute(null);
        }

        public string GetErrorMessage()
        {
            var be = TestMessageTextBlock.GetBindingExpression(TextBox.TextProperty);
            be?.UpdateTarget();
            return TestMessageTextBlock.Text;
        }

        public void CancelTest()
        {
            CancelButton.Command.Execute(null);
        }
    }
}
