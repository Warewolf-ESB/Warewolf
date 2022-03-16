/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Warewolf.Trigger.Queue;
using Xceed.Wpf.Toolkit;

namespace Dev2.Triggers.QueueEvents
{
    /// <summary>
    /// Interaction logic for QueueEventsView.xaml
    /// </summary>
    public partial class QueueEventsView
    {
        public QueueEventsView()
        {
            InitializeComponent();
        }

        private void DeleteQueueButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var queueEventsViewModel = DataContext as QueueEventsViewModel;
                queueEventsViewModel.SelectedQueue = button.DataContext as TriggerQueueView;
            }
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                var queueEventsViewModel = DataContext as QueueEventsViewModel;
                queueEventsViewModel.SelectedQueue = checkBox.DataContext as TriggerQueueView;
            }
        }

#pragma warning disable CC0091
        private void Concurrency_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsNumber(e.Text.FirstOrDefault()))
            {
                e.Handled = true;
            }
        }
        
        private void Concurrency_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(ConcurrencyIntegerUpDown.Text) && (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control))
            {
                var queueEventsViewModel = DataContext as QueueEventsViewModel;
                queueEventsViewModel.SelectedQueue.Concurrency = 0;
            }
        }
        
        private void Concurrency_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ConcurrencyIntegerUpDown.Text))
            {
                var queueEventsViewModel = DataContext as QueueEventsViewModel;
                queueEventsViewModel.SelectedQueue.Concurrency = 0;
            } 
        }
#pragma warning restore CC0091
    }
}
