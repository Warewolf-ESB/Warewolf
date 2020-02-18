/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
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

    public class OptionView
    {
        private readonly System.Action _valueUpdatedAction;

        public OptionView(IOption option, System.Action valueUpdatedAction)
        {
            DataContext = option;
            _valueUpdatedAction = valueUpdatedAction;
        }

        public IOption DataContext { get; set; }
        public DataTemplate DataTemplate
        {
            get
            {
                string dataTemplateName = "OptionNoneStyle";
                if (DataContext is BindableBase bindableBase)
                {
                    bindableBase.PropertyChanged += BindableBase_PropertyChanged;
                }
                if (DataContext is IOptionBool optionBool)
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
                if (DataContext is IOptionActivity)
                {
                    dataTemplateName = "OptionActivityStyle";
                }
                if (DataContext is IOptionEnum optionEnum)
                {
                    dataTemplateName = "OptionEnumComboBoxStyle";
                }
                if (DataContext is IOptionComboBox optionComboBox)
                {
                    dataTemplateName = "OptionComboBoxStyle";
                }
                if (DataContext is IOptionRadioButton optionRadioButton)
                {
                    dataTemplateName = "OptionRadioButtonStyle";
                }
                if (DataContext is OptionConditionExpression optionConditionList)
                {
                    dataTemplateName = "ConditionExpressionStyle";
                }
                var currentApp = CustomContainer.Get<IApplicationAdaptor>();
                var application = currentApp ?? new ApplicationAdaptor(Application.Current);

                return application?.TryFindResource(dataTemplateName) as DataTemplate;
            }
        }

        private void BindableBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _valueUpdatedAction();
        }

        public OptionView GetClone()
        {
            return new OptionView(DataContext.Clone() as IOption, () => { });
        }
    }
}
