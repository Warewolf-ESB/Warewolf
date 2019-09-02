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
using Warewolf.Trigger.Queue;

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
    }
}
