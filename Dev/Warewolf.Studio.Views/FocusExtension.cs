﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Dev2.CustomControls.Converters;

namespace Warewolf.Studio.Views
{
    // ReSharper disable UnusedMember.Global
    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused", typeof(bool), typeof(FocusExtension),
                new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;
            if ((bool)e.NewValue)
            {
                uie.Focus(); // Don't care about false values.
                Keyboard.Focus(uie);
            }
        }
    }

    public class ConnectedToImageConvertor : IValueConverter
    {
        readonly BoolToVisibilityConverter _convertor;

        public ConnectedToImageConvertor()
        {
            _convertor = new BoolToVisibilityConverter();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _convertor.Convert(!(bool)value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _convertor.Convert(!(bool)value, targetType, parameter, culture);
        }
    }
    // ReSharper restore UnusedMember.Global
}
