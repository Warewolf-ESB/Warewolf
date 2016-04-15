﻿
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Threading;

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

        private void StartTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(18) };
            _timer.Tick += TimerElapsed;
            _timer.Start();
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            _timer.Stop();

            VersionButton.Style = TryFindResource("SideMenuButtonStyle") as Style;
        }

        #region Implementation of IComponentConnector

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }

        #endregion
    }
}
