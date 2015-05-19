
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
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class TextboxSetFocusOnCreationBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateVisibility();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        public bool IsRenaming
        {
            get { return (bool)GetValue(IsRenamingProperty); }
            set { SetValue(IsRenamingProperty, value); }
        }

        public static readonly DependencyProperty IsRenamingProperty =
            DependencyProperty.Register("IsRenaming", typeof(bool), typeof(TextboxSetFocusOnCreationBehavior),
            new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var behavior = (TextboxSetFocusOnCreationBehavior)o;
            behavior.UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if(IsRenaming)
            {
                if(AssociatedObject != null)
                {
                    AssociatedObject.Visibility = Visibility.Visible;
                    AssociatedObject.Focus();
                }
            }
            else
            {
                AssociatedObject.Visibility = Visibility.Collapsed;
            }
        }
    }
}
