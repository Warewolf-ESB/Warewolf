
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
using System.Windows.Input;

namespace Dev2.Activities.AttachedProperties
{
    public static class FocusSetter
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool?), typeof(FocusSetter), new FrameworkPropertyMetadata(IsFocusedChanged));

        public static bool? GetIsFocused(DependencyObject element)
        {
            if(element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (bool?)element.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject element, bool? value)
        {
            if(element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(IsFocusedProperty, value);
        }

        static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)d;

            if(e.OldValue == null)
            {
                fe.GotFocus += FrameworkElementGotFocus;
                fe.LostFocus += FrameworkElementLostFocus;
                fe.LostKeyboardFocus += FrameworkElementOnLostKeyboardFocus;
                fe.MouseLeave += FrameworkElementOnLostMouseCapture;
            }

            if(!fe.IsVisible)
            {
                fe.IsVisibleChanged += FeIsVisibleChanged;
            }

            var isFocused = e.NewValue is Boolean && (bool)e.NewValue;
            if(isFocused)
            {
                fe.Focus();
                Keyboard.Focus(fe);
            }
        }

        static void FrameworkElementOnLostMouseCapture(object sender, MouseEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, false);
        }

        static void FrameworkElementOnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs keyboardFocusChangedEventArgs)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, false);
        }

        static void FeIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            var isFocused = ((FrameworkElement)sender).GetValue(IsFocusedProperty);
            if(fe.IsVisible && isFocused != null && (bool)isFocused)
            {
                fe.IsVisibleChanged -= FeIsVisibleChanged;
                fe.Focus();
            }
        }

        static void FrameworkElementGotFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, true);
        }

        static void FrameworkElementLostFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, false);
        }
    }
}
