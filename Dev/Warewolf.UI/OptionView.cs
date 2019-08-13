/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using Warewolf.Options;

namespace Warewolf.UI
{
    [ExcludeFromCodeCoverage]
    public class OptionViewDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item != null && item is OptionView)
            {
                var optionView = item as OptionView;
                return optionView.DataTemplate;
            }

            return null;
        }
    }

    [ExcludeFromCodeCoverage]
    public class OptionView
    {
        public OptionView(IOption option)
        {
            DataContext = option;
        }

        public IOption DataContext { get; set; }
        public DataTemplate DataTemplate
        {
            get
            {
                string dataTemplateName = "OptionNoneStyle";
                if (DataContext is IOptionBool)
                {
                    dataTemplateName = "OptionBoolStyle";
                }
                if (DataContext is IOptionInt)
                {
                    dataTemplateName = "OptionIntStyle";
                }
                if (DataContext is IOptionAutocomplete)
                {
                    dataTemplateName = "OptionAutocompleteStyle";
                }
                return Application.Current.TryFindResource(dataTemplateName) as DataTemplate;
            }
        }
    }
    [ExcludeFromCodeCoverage]
    public class OptionViewFactory
    {
        public static OptionView New(IOption option)
        {
            return new OptionView(option);
        }
    }
}
