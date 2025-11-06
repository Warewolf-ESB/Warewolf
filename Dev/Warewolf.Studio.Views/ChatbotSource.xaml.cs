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

using System;
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
        private bool _isUpdatingFromModelsEndpoint = false;

        public ChatbotSource()
        {
            InitializeComponent();
            ModelsEndpointTextBox.TextChanged += ModelsEndpointTextBox_TextChanged;
        }

        private void ModelsEndpointTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromModelsEndpoint)
                return;

            try
            {
                _isUpdatingFromModelsEndpoint = true;
                var modelsEndpoint = ModelsEndpointTextBox.Text;

                if (!string.IsNullOrWhiteSpace(modelsEndpoint))
                {
                    // Convert models endpoint to completions endpoint
                    var completionsEndpoint = ConvertModelsToCompletionsEndpoint(modelsEndpoint);
                    
                    // Update the CompletionsEndpoint TextBox
                    CompletionsEndpointTextBox.Text = completionsEndpoint;
                }
            }
            finally
            {
                _isUpdatingFromModelsEndpoint = false;
            }
        }

        private string ConvertModelsToCompletionsEndpoint(string modelsEndpoint)
        {
            if (string.IsNullOrWhiteSpace(modelsEndpoint))
                return string.Empty;

            try
            {
                // Try to parse as URI
                if (Uri.TryCreate(modelsEndpoint, UriKind.Absolute, out Uri uri))
                {
                    var path = uri.AbsolutePath;

                    // Replace "models" with "chat/completions"
                    if (path.EndsWith("/models"))
                    {
                        path = path.Substring(0, path.Length - "/models".Length) + "/chat/completions";
                    }
                    else if (path.Contains("/models"))
                    {
                        path = path.Replace("/models", "/chat/completions");
                    }
                    else
                    {
                        // If no "models" found, just append /chat/completions
                        path = path.TrimEnd('/') + "/chat/completions";
                    }

                    return $"{uri.Scheme}://{uri.Authority}{path}";
                }
                else
                {
                    // If not a valid URI, just do simple string replacement
                    if (modelsEndpoint.Contains("models"))
                    {
                        return modelsEndpoint.Replace("models", "chat/completions");
                    }
                    return modelsEndpoint;
                }
            }
            catch
            {
                // If any error occurs, just return the original value
                return modelsEndpoint;
            }
        }

        public string GetHeaderText()
        {
            var be = HeaderTextBlock.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateTarget();
            return HeaderTextBlock.Text;
        }

        public void EnterModelsEndpoint(string endpoint)
        {
            ModelsEndpointTextBox.Text = endpoint;
        }

        public void EnterCompletionsEndpoint(string endpoint)
        {
            CompletionsEndpointTextBox.Text = endpoint;
        }

        public void EnterApiKey(string apiKey)
        {
            ApiKeyPasswordBox.Text = apiKey;
        }

        public string GetModelsEndpoint() => ModelsEndpointTextBox.Text;

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
