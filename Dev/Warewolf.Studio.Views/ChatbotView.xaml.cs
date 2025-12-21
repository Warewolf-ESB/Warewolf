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

using System.Windows.Controls;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ChatbotView.xaml
    /// </summary>
    public partial class ChatbotView : UserControl
    {
        public ChatbotView()
        {
            InitializeComponent();
            Loaded += ChatbotView_Loaded;
            IsVisibleChanged += ChatbotView_IsVisibleChanged;
        }

        private void ChatbotView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            RefreshChatbotConfiguration();
        }

        private void ChatbotView_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
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
    }
}
