
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
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class TextboxSelectAllOnFocusBehavior : Behavior<TextBox>, IDisposable
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += AssociatedObject_GotFocus;           
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            routedEventArgs.Handled = true;
        }

        void AssociatedObject_GotFocus(object sender, EventArgs e)
        {
            AssociatedObject.SelectAll();
        }       

        protected override void OnDetaching()
        {         
            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            base.OnDetaching();
        }

        public void Dispose()
        {
            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
        }
    }
}
