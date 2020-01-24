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
using System.Activities;
using Warewolf.Data.Options;

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
                result.PropertyChanged += (o, e) => { prop.SetValue(instance, ((OptionAutocomplete)o).Value); };

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

                optionInt.PropertyChanged += (o, e) => { prop.SetValue(instance, ((OptionInt)o).Value); };
                return optionInt;
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(IWorkflow)))
            {
                var workflow = (IWorkflow)prop.GetValue(instance);
                var optionWorkflow = new OptionWorkflow
                {
                    Name = prop.Name,
                    Workflow = new WorkflowWithInputs { Name = workflow.Name, Value = workflow.Value, Inputs = workflow.Inputs },
                };
                if (helptextAttr != null)
                {
                    optionWorkflow.HelpText = helptextAttr.Get();
                }
                if (tooltipAttr != null)
                {
                    optionWorkflow.Tooltip = tooltipAttr.Get();
                }
                optionWorkflow.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == nameof(OptionWorkflow.Workflow))
                    {
                        prop.SetValue(instance, ((OptionWorkflow)o).Workflow);
                    }
                };
                return optionWorkflow;
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(System.Activities.Presentation.Model.ModelItem)))
            {
                var optionSelectedActivity = new OptionActivity
                {
                    Name = prop.Name,
                    Value = (System.Activities.Presentation.Model.ModelItem)prop.GetValue(instance),
                };
                if (helptextAttr != null)
                {
                    optionSelectedActivity.HelpText = helptextAttr.Get();
                }
                if (tooltipAttr != null)
                {
                    optionSelectedActivity.Tooltip = tooltipAttr.Get();
                }
                optionSelectedActivity.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == nameof(OptionActivity.Value))
                    {
                        prop.SetValue(instance, ((OptionActivity)o).Value);
                    }
                };
                return optionSelectedActivity;
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
                optionBool.PropertyChanged += (o, e) => { prop.SetValue(instance, ((OptionBool)o).Value); };

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
                result.PropertyChanged += (o, e) => { prop.SetValue(instance, ((OptionEnum)o).Value); };

                return result;
            }
            else
            {
                var attrs = prop.GetCustomAttributes();
                var optionTypeUXAttr = attrs.Where(o => o is OptionUXAttribute).Cast<OptionUXAttribute>().FirstOrDefault();
                var fieldValueAttr = attrs.Where(o => o is DataValueAttribute).Cast<DataValueAttribute>().FirstOrDefault();
                var dataProviderAttr = attrs.Where(o => o is MultiDataProviderAttribute).Cast<MultiDataProviderAttribute>().FirstOrDefault();

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

                IOptionMultiData returnVal;
                var enumValue = fieldNameProp.GetValue(propertyValue);

                Orientation? orientation = null;
                if (optionTypeUXAttr?.Get() == typeof(OptionRadioButtons))
                {
                    var orientationAttr = attrs.Where(o => o is OrientationAttribute).Cast<OrientationAttribute>().FirstOrDefault();
                    if (orientationAttr != null)
                    {
                        orientation = orientationAttr.Get();
                    }
                    returnVal = new OptionRadioButtons
                    {
                        Name = prop.Name,
                        Value = Enum.GetName(fieldNameProp.PropertyType, enumValue),
                    };
                }
                else
                { // optionTypeUXAttr is OptionCombobox
                    returnVal = new OptionCombobox
                    {
                        Name = prop.Name,
                        Value = Enum.GetName(fieldNameProp.PropertyType, enumValue),
                    };
                }
                if (helptextAttr != null)
                {
                    returnVal.HelpText = helptextAttr.Get();
                }
                if (tooltipAttr != null)
                {
                    returnVal.Tooltip = tooltipAttr.Get();
                }

                var optionCount = 0;
                if (dataProviderAttr != null)
                {
                    var optionTypes = dataProviderAttr.Get();
                    foreach (var optionType in optionTypes)
                    {
                        object value = propertyValue != null && propertyValue.GetType() == optionType.GetType() ? propertyValue : optionType;

                        var type = optionType.GetType();
                        returnVal.Options[type.Name] = OptionConvertor.Convert(value).Where(o => o.Name != fieldValueName);
                    }
                    optionCount = optionTypes.Length;
                }
                if (returnVal is OptionRadioButtons optionRadioButton)
                {
                    var calcOrientation = optionCount == 2 ? Orientation.Horizontal : Orientation.Vertical;
                    optionRadioButton.Orientation = orientation ?? calcOrientation;

                    returnVal.PropertyChanged += (o, e) => { prop.SetValue(instance, ExtractValueFromOptionMultiData(instance, prop, (OptionRadioButtons)o)); };
                }
                else
                {
                    returnVal.PropertyChanged += (o, e) => { prop.SetValue(instance, ExtractValueFromOptionMultiData(instance, prop, (OptionCombobox)o)); };
                }
                return returnVal;
            }
            throw UnhandledException;
        }

        public static readonly System.Exception FailedMappingException = new System.Exception("option to property conversion failed");
        public static readonly System.Exception UnhandledException = new System.Exception("unhandled property type for option conversion");
        public static readonly System.Exception NullException = new System.NullReferenceException("property value null for option conversion");

        public static IList<IOption> ConvertFromListOfT<T>(IEnumerable<T> options)
            where T : class, new()
        {
            var instance = new List<IOption>();
            foreach (var option in options)
            {
                instance.AddRange(Convert(option));
            }
            return instance;
        }
        public static IList<T> ConvertToListOfT<T>(IEnumerable<IOption> options)
            where T : class, new()
        {
            var instance = new List<T>();
            foreach (var option in options)
            {
                instance.Add(Convert(typeof(T), new[] { option }) as T);
            }
            return instance;
        }
        public static object Convert(Type type, IEnumerable<IOption> options)
        {
            var instance = Activator.CreateInstance(type);
            return Convert(type, options, instance);
        }
        public static object Convert<T>(Type type, IEnumerable<IOption> options, T parentInstance)
        {
            if (parentInstance is IOptionConvertable convertable)
            {
                var firstOption = options.First();
                convertable.FromOption(firstOption);
            }
            else
            {
                foreach (var option in options)
                {
                    var prop = type.GetProperty(option.Name);

                    SetProperty(parentInstance, prop, option);
                }
            }

            return parentInstance;
        }
        private static void SetProperty(object parentInstance, PropertyInfo prop, IOption option)
        {
            if (prop.PropertyType.IsAssignableFrom(typeof(string)))
            {
                if (option is IOptionAutocomplete optionAutocomplete)
                {
                    prop.SetValue(parentInstance, optionAutocomplete.Value);
                }
                else
                {
                    throw FailedMappingException;
                }
            }
            if (prop.PropertyType.IsAssignableFrom(typeof(IWorkflow)))
            {
                if (option is OptionWorkflow optionWorkflow)
                {
                    prop.SetValue(parentInstance, optionWorkflow.Workflow);
                }
                else
                {
                    throw FailedMappingException;
                }
            }
            if (prop.PropertyType.IsAssignableFrom(typeof(System.Activities.Presentation.Model.ModelItem)))
            {
                if (option is OptionActivity optionSelectedActivity)
                {
                    prop.SetValue(parentInstance, optionSelectedActivity.Value);
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
                    prop.SetValue(parentInstance, optionInt.Value);
                }
                else
                {
                    throw FailedMappingException;
                }
            }
            else if (option is OptionRadioButtons optionRadioButton)
            {
                object value = ExtractValueFromOptionMultiData(parentInstance, prop, optionRadioButton);
                prop.SetValue(parentInstance, value);
            }
            else if (option is OptionCombobox optionComboBox)
            {
                object value = ExtractValueFromOptionMultiData(parentInstance, prop, optionComboBox);
                prop.SetValue(parentInstance, value);
            }
            else if (option is OptionEnum optionEnum)
            {
                prop.SetValue(parentInstance, optionEnum.Value);
            }
            else
            {
                // unhandled

            }
        }

        private static object ExtractValueFromOptionMultiData(object parentInstance, PropertyInfo prop, IOptionMultiData optionMultiData)
        {
            var name = optionMultiData.Value;
            var propertyType = prop.PropertyType;
            var type = propertyType.Assembly.GetType(propertyType.Namespace + "." + name);
            var currentValue = prop.GetValue(parentInstance);
            if (currentValue.GetType() == type)
            {
                var value = Convert(type, optionMultiData.SelectedOptions, currentValue);
                return value;
            }
            else
            {
                var value = Convert(type, optionMultiData.SelectedOptions);
                return value;
            }
        }
    }
}
