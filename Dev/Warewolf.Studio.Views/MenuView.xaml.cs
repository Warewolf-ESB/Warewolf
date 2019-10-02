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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for MenuView.xaml
    /// </summary>
    public partial class MenuView
    {
        public MenuView()
        {
            InitializeComponent();
            StartTimer();
        }

        DispatcherTimer _timer;

        void StartTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(18) };
            _timer.Tick += TimerElapsed;
            _timer.Start();
        }

        void TimerElapsed(object sender, EventArgs e)
        {
            _timer.Stop();

            VersionButton.Style = TryFindResource("SideMenuButtonStyle") as Style;
        }

        private void MenuTaskButton_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (MenuTaskButton is Button menuTaskButton && !menuTaskButton.IsMouseOver ||
                MenuQueueEventsButton is Button menuQueueEventsButton && !menuQueueEventsButton.IsMouseOver)
            {
                TasksPopup.IsOpen = true;
                if (DataContext is MenuViewModel menuViewModel)
                {
                    menuViewModel.IsPopoutViewOpen = true;
                }
            }
        }

        private void MenuTaskButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (MenuTaskButton is Button menuTaskButton && !menuTaskButton.IsMouseOver &&
                MenuQueueEventsButton is Button menuQueueEventsButton && !menuQueueEventsButton.IsMouseOver)
            {
                TasksPopup.IsOpen = false;
                if (DataContext is MenuViewModel menuViewModel)
                {
                    menuViewModel.IsPopoutViewOpen = false;
                    menuViewModel.SlideClosedCommand.Execute(null);
                }
            }
        }
    }
}
