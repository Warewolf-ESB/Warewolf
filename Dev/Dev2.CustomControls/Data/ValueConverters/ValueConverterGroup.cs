/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - August 2006

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPF.JoshSmith.Data.ValueConverters
{
    /// <summary>
    ///     A value converter which contains a list of IValueConverters and invokes their Convert or ConvertBack methods
    ///     in the order that they exist in the list.  Every converter in the group must be decorated exactly once with
    ///     the ValueConversion attribute, otherwise an InvalidOperationException will be thrown.
    /// </summary>
    /// <remarks>
    ///     The output of one converter is piped into the next converter allowing for modular value
    ///     converters to be chained together.  If the ConvertBack method is invoked, the value converters
    ///     are executed in reverse order (highest to lowest index).  Do not leave an element in the
    ///     Converters property collection null, every element must reference a valid IValueConverter
    ///     instance. If a value converter's type is not decorated with the ValueConversionAttribute,
    ///     an InvalidOperationException will be thrown when the converter is added to the Converters collection.
    ///     Documentation: http://www.codeproject.com/KB/WPF/PipingValueConverters_WPF.aspx
    /// </remarks>
    [ContentProperty("Converters")]
    public class ValueConverterGroup : IValueConverter
    {
        #region Data

        private readonly Dictionary<IValueConverter, ValueConversionAttribute> cachedAttributes =
            new Dictionary<IValueConverter, ValueConversionAttribute>();

        private readonly ObservableCollection<IValueConverter> converters = new ObservableCollection<IValueConverter>();

        #endregion // Data

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of ValueConverterGroup.
        /// </summary>
        public ValueConverterGroup()
        {
            converters.CollectionChanged += OnConvertersCollectionChanged;
        }

        #endregion // Constructor

        #region Converters

        /// <summary>
        ///     Returns the list of IValueConverters contained in this converter.
        /// </summary>
        public ObservableCollection<IValueConverter> Converters
        {
            get { return converters; }
        }

        #endregion // Converters

        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object output = value;

            for (int i = 0; i < Converters.Count; ++i)
            {
                IValueConverter converter = Converters[i];
                Type currentTargetType = GetTargetType(i, targetType, true);
                output = converter.Convert(output, currentTargetType, parameter, culture);

                // If the converter returns 'DoNothing' then the binding operation should terminate.
                if (output == Binding.DoNothing)
                    break;
            }

            return output;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object output = value;

            for (int i = Converters.Count - 1; i > -1; --i)
            {
                IValueConverter converter = Converters[i];
                Type currentTargetType = GetTargetType(i, targetType, false);
                output = converter.ConvertBack(output, currentTargetType, parameter, culture);

                // When a converter returns 'DoNothing' the binding operation should terminate.
                if (output == Binding.DoNothing)
                    break;
            }

            return output;
        }

        #endregion // IValueConverter Members

        #region Private Helpers

        #region GetTargetType

        /// <summary>
        ///     Returns the target type for a conversion operation.
        /// </summary>
        /// <param name="converterIndex">The index of the current converter about to be executed.</param>
        /// <param name="finalTargetType">The 'targetType' argument passed into the conversion method.</param>
        /// <param name="convert">Pass true if calling from the Convert method, or false if calling from ConvertBack.</param>
        protected virtual Type GetTargetType(int converterIndex, Type finalTargetType, bool convert)
        {
            // If the current converter is not the last/first in the list, 
            // get a reference to the next/previous converter.
            IValueConverter nextConverter = null;
            if (convert)
            {
                if (converterIndex < Converters.Count - 1)
                {
                    nextConverter = Converters[converterIndex + 1];
                    if (nextConverter == null)
                        throw new InvalidOperationException(
                            "The Converters collection of the ValueConverterGroup contains a null reference at index: "
                            + (converterIndex + 1));
                }
            }
            else
            {
                if (converterIndex > 0)
                {
                    nextConverter = Converters[converterIndex - 1];
                    if (nextConverter == null)
                        throw new InvalidOperationException(
                            "The Converters collection of the ValueConverterGroup contains a null reference at index: "
                            + (converterIndex - 1));
                }
            }

            if (nextConverter != null)
            {
                ValueConversionAttribute conversionAttribute = cachedAttributes[nextConverter];

                // If the Convert method is going to be called, we need to use the SourceType of the next 
                // converter in the list.  If ConvertBack is called, use the TargetType.
                return convert ? conversionAttribute.SourceType : conversionAttribute.TargetType;
            }

            // If the current converter is the last one to be executed return the 
            // target type passed into the conversion method.
            return finalTargetType;
        }

        #endregion // GetTargetType

        #region OnConvertersCollectionChanged

        private void OnConvertersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // The 'Converters' collection has been modified, so validate that each value converter it now
            // contains is decorated with ValueConversionAttribute and then cache the attribute value.

            IList convertersToProcess = null;
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                convertersToProcess = e.NewItems;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IValueConverter converter in e.OldItems)
                    cachedAttributes.Remove(converter);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                cachedAttributes.Clear();
                convertersToProcess = converters;
            }

            if (convertersToProcess != null && convertersToProcess.Count > 0)
            {
                foreach (IValueConverter converter in convertersToProcess)
                {
                    object[] attributes = converter.GetType()
                        .GetCustomAttributes(typeof (ValueConversionAttribute), false);

                    if (attributes.Length != 1)
                        throw new InvalidOperationException(
                            "All value converters added to a ValueConverterGroup must be decorated with the ValueConversionAttribute attribute exactly once.");

                    cachedAttributes.Add(converter, attributes[0] as ValueConversionAttribute);
                }
            }
        }

        #endregion // OnConvertersCollectionChanged

        #endregion // Private Helpers
    }
}