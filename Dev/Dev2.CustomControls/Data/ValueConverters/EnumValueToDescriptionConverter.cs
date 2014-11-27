/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - August 2006

using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace WPF.JoshSmith.Data.ValueConverters
{
    /// <summary>
    ///     Supports conversion from an enum value to the description of that value.  If the enum value is decorated
    ///     with the <see cref="System.ComponentModel.DescriptionAttribute" /> attribute, its Description value is returned.
    ///     Otherwise, the name of the enum value is returned.
    /// </summary>
    /// <remarks>
    ///     Documentation:
    ///     http://web.archive.org/web/20070404140911/http://www.infusionblogs.com/blogs/jsmith/archive/2006/08/29/835.aspx
    /// </remarks>
    [ValueConversion(typeof (Enum), typeof (string))]
    public class EnumValueToDescriptionConverter : IValueConverter
    {
        #region Convert

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum == false)
                throw new ArgumentException("'value' must be an enum.");

            // Get the field in the enum type which represents the argument value.
            FieldInfo field = value.GetType().GetField(value.ToString());

            // Get the DescriptionAttribute on that field, if it exists.
            object[] attributes = field.GetCustomAttributes(typeof (DescriptionAttribute), false);

            // The enum value is not decorated with the attribute, so just return the value itself.
            if (attributes.Length == 0)
                return value.ToString();

            // Return the description applied to the enum value.
            var descriptionAttribute = attributes[0] as DescriptionAttribute;
            if (descriptionAttribute != null)
            {
                return descriptionAttribute.Description;
            }

            return string.Empty;
        }

        #endregion // Convert

        #region ConvertBack

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack not supported.");
        }

        #endregion // ConvertBack
    }
}