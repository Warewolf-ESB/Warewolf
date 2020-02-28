/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using Warewolf.Options;

namespace Warewolf.UI
{
    public delegate void OptionChangedHandler();
    public class OptionsWithNotifier
    {
        public event OptionChangedHandler OptionChanged;
        public IList<IOption> Options { get; set; }
        public void Notify()
        {
            OptionChanged?.Invoke();
        }
    }

    public class OptionViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OptionsWithNotifier optionsWithNotifier)
            {
                return Convert(optionsWithNotifier, targetType, parameter, culture);
            }
            if (value is IEnumerable<IOption> options)
            {
                return Convert(options, targetType, parameter, culture);
            }
            if (value is object)
            {
                return Convert(OptionConvertor.Convert(value), targetType, parameter, culture);
            }

            return Binding.DoNothing;
        }

        object Convert(OptionsWithNotifier options, Type targetType, object parameter, CultureInfo culture)
        {
            var optionViews = new ObservableCollection<OptionView>();
            if (options.Options is null)
            {
                return optionViews;
            }
            foreach (var option in options.Options)
            {
                var optionView = new OptionView(option, () => { options.Notify(); });
                optionViews.Add(optionView);
            }

            return optionViews;
        }
        object Convert(IEnumerable<IOption> options, Type targetType, object parameter, CultureInfo culture)
        {
            var optionViews = new ObservableCollection<OptionView>();
            foreach (var option in options)
            {
                var optionView = new OptionView(option, () => { });
                optionViews.Add(optionView);
            }

            return optionViews;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
