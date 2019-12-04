/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Warewolf.Data;
using System;

namespace Warewolf.Options
{
    public static class OptionConvertor
    {
        public static IOption[] Convert(object o)
        {
            var result = new List<IOption>();

            if (o is IOptionConvertable convertable)
            {
                return convertable.ToOptions();
            }
            else
            {
                var type = o.GetType();
                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    result.Add(PropertyToOption(o, prop));
                }

                return result.ToArray();
            }
        }

        private static IOption PropertyToOption(object instance, PropertyInfo prop)
        {
            if (prop.PropertyType.IsAssignableFrom(typeof(string)))
            {
                var attr = prop.GetCustomAttributes().Where(o => o is DataProviderAttribute).Cast<DataProviderAttribute>().FirstOrDefault();
                var result = new OptionAutocomplete()
                {
                    Name = prop.Name,
                    Value = (string)prop.GetValue(instance)
                };
                if (attr != null)
                {
                    result.Suggestions = ((IOptionDataList)attr.Get()).Options;
                }
                return result;
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(int)))
            {
                var helpText = Studio.Resources.Languages.HelpText.OptionIntHelpText;
                var Tooltip = Studio.Resources.Languages.Tooltips.OptionIntTooltip;
                switch (prop.Name)
                {
                    case "Count":
                        helpText = Studio.Resources.Languages.HelpText.OptionGateCountHelpText;
                        Tooltip = Studio.Resources.Languages.Tooltips.OptionGateCountToolTip;
                        break;
                    case "TimeOut":
                        helpText = Studio.Resources.Languages.HelpText.OptionGateTimeoutHelpText;
                        Tooltip = Studio.Resources.Languages.Tooltips.OptionGateTimeoutToolTip;
                        break;
                    case "MaxRetries":
                        helpText = Studio.Resources.Languages.HelpText.OptionGateMaxRetriesHelpText;
                        Tooltip = Studio.Resources.Languages.Tooltips.OptionGateMaxRetriesToolTip;
                        break;
                    case "Increment":
                        helpText = Studio.Resources.Languages.HelpText.OptionGateIncrementHelpText;
                        Tooltip = Studio.Resources.Languages.Tooltips.OptionGateIncrementToolTip;
                        break;
                }
                return new OptionInt
                {
                    Name = prop.Name,
                    Value = (int)prop.GetValue(instance),
                    HelpText = helpText,
                    Tooltip = Tooltip
                };
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(Guid)))
            {
                return new OptionWorkflow
                {
                    Name = prop.Name,
                    Value = (Guid)prop.GetValue(instance),
                    Workflow = new NamedGuid { Name = "", Value = (Guid)prop.GetValue(instance) },
                    HelpText = Studio.Resources.Languages.HelpText.OptionGateResumeEndpointHelpText,
                    Tooltip = Studio.Resources.Languages.Tooltips.OptionGateResumeEndpointToolTip
                };
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(bool)))
            {
                return new OptionBool
                {
                    Name = prop.Name,
                    Value = (bool)prop.GetValue(instance),
                };
            }
            else if (prop.PropertyType.IsEnum)
            {
                var values = new List<KeyValuePair<string, int>>();
                foreach (var option in Enum.GetValues(prop.PropertyType))
                {
                    var key = Enum.GetName(prop.PropertyType, option);
                    values.Add(new KeyValuePair<string, int>(key, (int)option));
                }

                var result = new OptionEnum
                {
                    Values = values,
                    Name = prop.Name,
                    Value = (int)prop.GetValue(instance),
                    HelpText = Studio.Resources.Languages.HelpText.OptionGateResumeHelpText,
                    Tooltip = Studio.Resources.Languages.Tooltips.OptionGateResumeToolTip
                };

                return result;
            }
            else
            {
                var fieldValueAttr = prop.GetCustomAttributes().Where(o => o is DataValueAttribute).Cast<DataValueAttribute>().FirstOrDefault();
                var dataProviderAttr = prop.GetCustomAttributes().Where(o => o is MultiDataProviderAttribute).Cast<MultiDataProviderAttribute>().FirstOrDefault();

                if (fieldValueAttr == null)
                {
                    throw new Exception("FieldValueAttribute required");
                }
                var fieldValueName = fieldValueAttr.Get();

                var propertyValue = prop.GetValue(instance);
                var types = prop.PropertyType.GetProperties();
                var fieldNameProp = types.First(type => type.Name == fieldValueName);
                if (fieldNameProp == null)
                {
                    throw new Exception("property does not exist");
                }
                if (!fieldNameProp.PropertyType.IsEnum)
                {
                    throw new Exception("currently only enums supported");
                }
                if (propertyValue is null)
                {
                    throw NullException;
                }

                var enumValue = fieldNameProp.GetValue(propertyValue);
                var returnVal = new OptionCombobox
                {
                    Name = prop.Name,
                    Value = Enum.GetName(fieldNameProp.PropertyType, enumValue),
                    HelpText = Studio.Resources.Languages.HelpText.OptionGateStrategyHelpText,
                    Tooltip = Studio.Resources.Languages.Tooltips.OptionGateStrategyToolTip
                };

                if (dataProviderAttr != null)
                {
                    var optionTypes = dataProviderAttr.Get();
                    foreach (var optionType in optionTypes)
                    {
                        var type = optionType.GetType();
                        returnVal.Options[type.Name] = OptionConvertor.Convert(optionType).Where(o => o.Name != fieldValueName);
                    }
                }
                return returnVal;
            }
            throw UnhandledException;
        }

        public static readonly System.Exception FailedMappingException = new System.Exception("option to property conversion failed");
        public static readonly System.Exception UnhandledException = new System.Exception("unhandled property type for option conversion");
        public static readonly System.Exception NullException = new System.NullReferenceException("property value null for option conversion");

        public static object Convert(Type type, IEnumerable<IOption> options)
        {
            var instance = Activator.CreateInstance(type);
            foreach (var option in options)
            {
                var prop = type.GetProperty(option.Name);

                SetProperty(instance, prop, option);
            }

            return instance;
        }
        private static void SetProperty(object instance, PropertyInfo prop, IOption option)
        {
            if (prop.PropertyType.IsAssignableFrom(typeof(string)))
            {
                if (option is IOptionAutocomplete optionAutocomplete)
                {
                    prop.SetValue(instance, optionAutocomplete.Value);
                }
                else
                {
                    throw FailedMappingException;
                }
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(int)))
            {
                if (option is IOptionInt optionInt)
                {
                    prop.SetValue(instance, optionInt.Value);
                }
                else
                {
                    throw FailedMappingException;
                }
            }
            else if (option is OptionCombobox optionComboBox)
            {
                var value = Convert(prop.PropertyType, optionComboBox.SelectedOptions);
                prop.SetValue(instance, value);
            } else
            {
                // unhandled
            }
        }
    }
}
