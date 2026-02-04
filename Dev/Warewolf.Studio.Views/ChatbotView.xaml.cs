#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ChatbotView.xaml
    /// </summary>
    public partial class ChatbotView : UserControl
    {
        private bool _isFirstLoad = true;

        public ChatbotView()
        {
            InitializeComponent();
            IsVisibleChanged += ChatbotView_IsVisibleChanged;
        }

        private void ChatbotView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Only refresh when becoming visible, and only once on first load
            if (IsVisible && _isFirstLoad)
            {
                _isFirstLoad = false;
                RefreshChatbotConfiguration();
            }
        }

        private void RefreshChatbotConfiguration()
        {
            if (DataContext is ChatbotViewModel viewModel)
            {
                viewModel.RefreshConfiguration();
            }
        }

        private void CopyMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuItem menuItem && menuItem.Tag is ChatMessage message)
                {
                    Clipboard.SetText(message.Content);
                }
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("Error copying message to clipboard", ex, "Warewolf Error");
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void CopyAllMessages_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is ChatbotViewModel vm && vm.Messages.Count > 0)
                {
                    var allText = string.Join(Environment.NewLine + Environment.NewLine,
                        vm.Messages.Select(m => m.DisplayText));
                    Clipboard.SetText(allText);
                }
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("Error copying all messages to clipboard", ex, "Warewolf Error");
            }
        }
    }
}
