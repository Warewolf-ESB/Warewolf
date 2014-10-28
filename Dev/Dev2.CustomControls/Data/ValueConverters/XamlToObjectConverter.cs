/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - July 2006

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;

namespace WPF.JoshSmith.Data.ValueConverters
{
    /// <summary>
    ///     This value converter creates a .NET object from the XAML contained in an XmlElement.  The object
    ///     created can be used as the content of a WPF control or ui element, such as the ContentPresenter.
    ///     The inner xml of the XmlElement passed to the converter must contain valid XAML.
    /// </summary>
    [ValueConversion(typeof (XmlElement), typeof (object))]
    public class XamlToObjectConverter : IValueConverter
    {
        #region Data

        // Every call to the XamlReader requires a ParserContext, so a static instance is kept
        // to reduce the overhead of creating one every time a value is converted.
        private static readonly ParserContext parserContext;

        #endregion // Data

        #region Static Constructor

        static XamlToObjectConverter()
        {
            // Initialize the parser context, which provides xml namespace mappings used when
            // the loose XAML is loaded and converted into a .NET object.
            parserContext = new ParserContext();
            parserContext.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            parserContext.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
        }

        #endregion // Static Constructor

        #region Convert

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The 'value' parameter must be an XmlElement.
            var elem = value as XmlElement;

            // We need to create a MemoryStream because the XamlReader requires a stream
            // from which the XAML is read.  
            using (var stream = new MemoryStream())
            {
                // Convert the inner xml of the element into a byte array so
                // that it can be loaded into the memory stream.
                if (elem != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(elem.InnerXml);

                    // Write the XAML element into the memory stream.
                    stream.Write(bytes, 0, bytes.Length);
                }

                // Reset the stream's current position back to the beginning so that when it
                // is read from, the reading will begin at the correct place.
                stream.Position = 0;

                // This is the magic method call which converts XAML into a .NET object.
                return XamlReader.Load(stream, parserContext);
            }
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