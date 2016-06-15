/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Interactivity;

namespace Dev2.CustomControls.Behavior
{
    public class VisibilityChangedToFocusBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsVisibleChanged += AssociatedObjectOnIsVisibleChanged;
        }

        protected override void OnDetaching()
        {
            base.OnAttached();
            AssociatedObject.IsVisibleChanged -= AssociatedObjectOnIsVisibleChanged;
        }


        private void AssociatedObjectOnIsVisibleChanged(object o, DependencyPropertyChangedEventArgs args)
        {
            if ((bool) args.NewValue && !(bool) args.OldValue)
            {
                AssociatedObject.Focus();
            }
        }
    }
}