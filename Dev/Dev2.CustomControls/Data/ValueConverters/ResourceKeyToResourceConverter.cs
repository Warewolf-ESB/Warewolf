// Copyright (C) Josh Smith - July 2008

using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace WPF.JoshSmith.Data.ValueConverters
{
    /// <summary>
    /// A value converter that performs a resource lookup on the conversion value.
    /// NOTE: This class depends on the use of reflection to manipulate WPF to allow
    /// an instance of the converter to perform a resource lookup.
    /// </summary>
    /// <remarks>
    /// Documentation: http://www.codeproject.com/KB/WPF/SelectDetailLevels.aspx
    /// </remarks>
    [ValueConversion(typeof(object), typeof(object))]
    public class ResourceKeyToResourceConverter
        : Freezable, // Enable this converter to be the source of a resource lookup.
        IValueConverter
    {
        static readonly DependencyProperty DummyProperty =
            DependencyProperty.Register(
            "Dummy",
            typeof(object),
            typeof(ResourceKeyToResourceConverter));

        /// <summary>
        /// Performs a resource lookup using the value argument as the resource key.
        /// </summary>
        public object Convert(
          object value, Type targetType, object parameter, CultureInfo culture)
        {
            return FindResource(value);
        }

        /// <summary>
        /// Do not invoke.
        /// </summary>
        public object ConvertBack(
          object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }

        object FindResource(object resourceKey)
        {
            // NOTE: This code depends on internal implementation details of WPF and 
            // might break in a future release of the platform.  Use at your own risk!

            var resourceReferenceExpression =
                new DynamicResourceExtension(resourceKey).ProvideValue(null)
                as Expression;

            MethodInfo getValue = typeof(Expression).GetMethod(
                "GetValue",
                BindingFlags.Instance | BindingFlags.NonPublic);

            object result = getValue.Invoke(
                resourceReferenceExpression,
                new object[] { this, DummyProperty });

            // Either we do not have an inheritance context or the  
            // requested resource does not exist, so return null.
            if(result == DependencyProperty.UnsetValue)
                return null;

            // The requested resource was found, so we will receive a 
            // DeferredResourceReference object as a result of calling 
            // GetValue.  The only way to resolve that to the actual 
            // resource, without using reflection, is to have a Setter's 
            // Value property unwrap it for us.
            var deferredResourceReference = result;
            Setter setter = new Setter(DummyProperty, deferredResourceReference);
            return setter.Value;
        }

        /// <summary>
        /// Do not invoke.
        /// </summary>
        protected override Freezable CreateInstanceCore()
        {
            // We are required to override this abstract method.
            throw new NotImplementedException();
        }
    }
}