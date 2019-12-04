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
                return new OptionInt
                {
                    Name = prop.Name,
                    Value = (int)prop.GetValue(instance)
                };
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(Guid)))
            {
                return new OptionWorkflow
                {
                    Name = prop.Name,
                    Value = (Guid)prop.GetValue(instance),
                    Workflow = new NamedGuid { Name = "", Value = (Guid)prop.GetValue(instance) }
                };
            }
            else if (prop.PropertyType.IsAssignableFrom(typeof(bool)))
            {
                return new OptionBool
                {
                    Name = prop.Name,
                    Value = (bool)prop.GetValue(instance)
                };
            }
            //else if (prop.PropertyType.IsAssignableFrom(typeof(Guid)))
            //{
            //    return new OptionConditionList
            //    {
            //        Name = prop.Name
            //    };
            //}
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
                    Value = (int)prop.GetValue(instance)
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

        public static readonly System.Exception UnhandledException = new System.Exception("unhandled property type for option conversion");
        public static readonly System.Exception NullException = new System.NullReferenceException("property value null for option conversion");

        public static object Convert(Type type, IEnumerable<IOption> options)
        {
            var optionsType = Activator.CreateInstance(type);

            var gateOptions = new Data.Options.GateOptions();

            if (optionsType is Data.Options.GateOptions)
            {
                foreach (var option in options)
                {
                    if (option is IOptionInt optionInt)
                    {
                        gateOptions.Count = optionInt.Value;
                    }
                    if (option is IOptionEnum optionEnum)
                    {
                        switch (optionEnum.Value)
                        {
                            case 1:
                                gateOptions.Resume = Data.Options.YesNo.Yes;
                                break;
                            case 0:
                            default:
                                gateOptions.Resume = Data.Options.YesNo.No;
                                break;
                        }
                    }
                    if (option is IOptionWorkflow optionWorkflow)
                    {
                        gateOptions.ResumeEndpoint = optionWorkflow.Value;
                    }
                    if (option is IOptionComboBox optionCombobox)
                    {
                        switch (optionCombobox.Value)
                        {
                            case "NoBackoff":
                                gateOptions.Strategy = new Data.Options.NoBackoff
                                {
                                    RetryAlgorithm = Data.Options.Enums.RetryAlgorithm.NoBackoff
                                };
                                break;
                            case "ConstantBackoff":
                                gateOptions.Strategy = new Data.Options.ConstantBackoff
                                {
                                    RetryAlgorithm = Data.Options.Enums.RetryAlgorithm.ConstantBackoff
                                };
                                break;
                            case "LinearBackoff":
                                gateOptions.Strategy = new Data.Options.LinearBackoff
                                {
                                    RetryAlgorithm = Data.Options.Enums.RetryAlgorithm.LinearBackoff
                                };
                                break;
                            case "FibonacciBackoff":
                                gateOptions.Strategy = new Data.Options.FibonacciBackoff
                                {
                                    RetryAlgorithm = Data.Options.Enums.RetryAlgorithm.FibonacciBackoff
                                };
                                break;
                            case "QuadraticBackoff":
                                gateOptions.Strategy = new Data.Options.QuadraticBackoff
                                {
                                    RetryAlgorithm = Data.Options.Enums.RetryAlgorithm.QuadraticBackoff
                                };
                                break;
                        }
                    }
                }
            }



            return gateOptions;
        }
    }
}
