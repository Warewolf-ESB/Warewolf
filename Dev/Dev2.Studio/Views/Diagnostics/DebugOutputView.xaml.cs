/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Input;

namespace Dev2.Studio.Views.Diagnostics
{
    /// <summary>
    /// Interaction logic for DebugOutputWindow.xaml
    /// </summary>
    public partial class DebugOutputView
    {
        public DebugOutputView()
        {
            InitializeComponent();
        }

        void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            DebugOutputTree.AddHandler(MouseWheelEvent, new RoutedEventHandler(MyMouseWheelH), true);
        }

        void MyMouseWheelH(object sender, RoutedEventArgs e)
        {
            var eargs = (MouseWheelEventArgs)e;
            var x = (double)eargs.Delta;
            var y = instScroll.VerticalOffset;
            instScroll.ScrollToVerticalOffset(y - x);
        }
    }
}
