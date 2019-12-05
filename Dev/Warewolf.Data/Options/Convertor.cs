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
            var helptextAttr = prop.GetCustomAttributes().Where(o => o is HelpTextAttribute).Cast<HelpTextAttribute>().FirstOrDefault();
            var tooltipAttr = prop.GetCustomAttributes().Where(o => o is TooltipAttribute).Cast<TooltipAttribute>().FirstOrDefault();
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
                if (helptextAttr != null)
                {
                    result.HelpText = helptextAttr.Get();
                }
                if (tooltipAttr != null)
                {
                    result.Tooltip = tooltipAttr.Get();
                }
                return result;
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(int)))
            {              
                var optionInt = new OptionInt
                {
                    Name = prop.Name,
                    Value = (int)prop.GetValue(instance)                    
                };
                if (helptextAttr != null)
                {
                    optionInt.HelpText = helptextAttr.Get();
                }
                if (tooltipAttr != null)
                {
                    optionInt.Tooltip = tooltipAttr.Get();
                }
                return optionInt;
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(Guid)))
            {
               var optionWorkflow = new OptionWorkflow
                {
                    Name = prop.Name,
                    Value = (Guid)prop.GetValue(instance),
                    Workflow = new NamedGuid { Name = "", Value = (Guid)prop.GetValue(instance) },
                   
                };
                if (helptextAttr != null)
                {
                    optionWorkflow.HelpText = helptextAttr.Get();
                }
                if (tooltipAttr != null)
                {
                    optionWorkflow.Tooltip = tooltipAttr.Get();
                }
                return optionWorkflow;
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(bool)))
            {
                var optionBool = new OptionBool
                {
                    Name = prop.Name,
                    Value = (bool)prop.GetValue(instance),
                };
                if (helptextAttr != null)
                {
                    optionBool.HelpText = helptextAttr.Get();
                }
                if (tooltipAttr != null)
                {
                    optionBool.Tooltip = tooltipAttr.Get();
                }
                return optionBool;
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
                };
                if (helptextAttr != null)
                {
                    result.HelpText = helptextAttr.Get();
                }
                if (tooltipAttr != null)
                {
                    result.Tooltip = tooltipAttr.Get();
                }
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
                };
                if (helptextAttr != null)
                {
                    returnVal.HelpText = helptextAttr.Get();
                }
                if (tooltipAttr != null)
                {
                    returnVal.Tooltip = tooltipAttr.Get();
                }
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
            return Convert(type, options, null);
        }
        public static object Convert(Type type, IEnumerable<IOption> options, object instance)
        {
            var firstOption = options.FirstOrDefault();
            if (instance is null)
            {
                instance = Activator.CreateInstance(type);
            }

            if (instance is IOptionConvertable convertable)
            {
                convertable.FromOption(firstOption);
            }
            else
            {
                foreach (var option in options)
                {
                    var prop = type.GetProperty(option.Name);

                    SetProperty(instance, prop, option);
                }
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
                var name = optionComboBox.Value;
                //var newInstance = Activator.CreateInstance(prop.PropertyType.Assembly.GetType(prop.PropertyType.Namespace + "." + name)).GetType();
                var t = prop.PropertyType.Assembly.GetType(prop.PropertyType.Namespace + "." + name);
                var value = Convert(t, optionComboBox.SelectedOptions);
                prop.SetValue(instance, value);
            }
            else if (option is OptionEnum optionEnum)
            {
                prop.SetValue(instance, optionEnum.Value);
            }
            else
            {
                // unhandled

            }
        }
    }
}
