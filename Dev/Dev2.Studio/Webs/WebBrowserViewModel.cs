
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;

namespace Dev2.Webs
{
    public class WebBrowserViewModel : DependencyObject
    {
        public string RightTitle
        {
            get { return (string)GetValue(RightTitleProperty); }
            set { SetValue(RightTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightTitleProperty = DependencyProperty.Register("RightTitle", typeof(string), typeof(WebBrowserViewModel), new PropertyMetadata(string.Empty));



        public string LeftTitle
        {
            get { return (string)GetValue(LeftTitleProperty); }
            set { SetValue(LeftTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftTitleProperty = DependencyProperty.Register("LeftTitle", typeof(string), typeof(WebBrowserViewModel), new PropertyMetadata(string.Empty));


    }
}
