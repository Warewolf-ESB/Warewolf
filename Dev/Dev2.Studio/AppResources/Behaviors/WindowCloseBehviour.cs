
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Windows;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class WindowCloseBehviour : Behavior<Window>
    {
        #region Dependency Properties

        #region CloseIndicator

        public string CloseIndicator
        {
            get { return (string)GetValue(CloseIndicatorProperty); }
            set { SetValue(CloseIndicatorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseIndicatorProperty =
            DependencyProperty.Register("CloseIndicator", typeof(string), typeof(WindowCloseBehviour), new PropertyMetadata(CloseIndicatorChanged));

        private static void CloseIndicatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WindowCloseBehviour windowCloseBehviour = d as WindowCloseBehviour;

            if(windowCloseBehviour == null || windowCloseBehviour.AssociatedObject == null) return;

            bool value = Convert.ToBoolean(e.NewValue);
            if(value)
            {
                windowCloseBehviour.AssociatedObject.Close();
            }
        }

        #endregion CloseIndicator

        #endregion Dependency Properties
    }
}
