
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

namespace Dev2.Studio.AppResources.AttachedProperties {
    public static class FocusAttachedProperty {
        public static bool GetIsFocused(DependencyObject obj) {
            return (bool)obj.GetValue(IsFocusedProperty);
        }
        public static void SetIsFocused(DependencyObject obj, bool value) {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
                DependencyProperty.RegisterAttached(
                 "IsFocused", typeof(bool), typeof(FocusAttachedProperty),
                 new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(DependencyObject d,
                DependencyPropertyChangedEventArgs e) {
            var uie = (UIElement)d;
            if ((bool)e.NewValue) {
                //uie.Focus(); // Don't care about false values.
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Keyboard.Focus(uie);
                }), null);
                
            }
        }
    }
}
