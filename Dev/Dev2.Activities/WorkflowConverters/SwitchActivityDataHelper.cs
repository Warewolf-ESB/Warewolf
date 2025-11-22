using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Activities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Activities.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common.X6;

namespace Dev2.Activities.WorkflowConverters
{
    public class SwitchActivityDataHelper
    {
        // Cached reflection info for performance
        private static readonly ConcurrentDictionary<Type, PropertyAccessors> _propertyCache = new();
        private static readonly Regex _variableExtractionRegex = new(@"\[\[([^\]]+)\]\]", RegexOptions.Compiled);

        public int CurrentX { get; set; }
        public int CurrentY { get; set; }

        public SwitchActivityDataHelper(int currentX, int currentY)
        {
            CurrentX = currentX;
            CurrentY = currentY;
        }

        // Cached property accessors for better performance
        private sealed record PropertyAccessors(
            PropertyInfo? DisplayName,
            PropertyInfo? ExpressionText,
            PropertyInfo? UniqueID,
            PropertyInfo? OnErrorVariable,
            PropertyInfo? OnErrorWorkflow,
            PropertyInfo? IsEndedOnError
        );

        public Cell CreateSwitchNode(FlowSwitch<object> flowSwitch, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                shape = Constants.POLYGON,
                position = new Position(CurrentX, CurrentY),
                label = Constants.SWITCH,
                data = new Dictionary<string, object>
                {
                    [Constants.TYPE] = Constants.FLOWSWITCH,
                    [Constants.EXPRESSION] = GetCleanExpressionText(flowSwitch.Expression) ?? Constants.SWITCH
                }
            };

            CurrentY += 150;

            var expression = flowSwitch.Expression;
            if (expression != null && expression.GetType().Name.Contains(nameof(DsfFlowSwitchActivity)))
            {
                ProcessSwitchActivityReflection(expression, flowSwitch, cell);
            }
            else
            {
                ProcessBasicExpression(expression, cell);
            }

            return cell;
        }

        public Cell CreateSwitchNodeString(FlowSwitch<string> flowSwitch, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                shape = Constants.POLYGON,
                position = new Position(CurrentX, CurrentY),
                label = Constants.SWITCH,
                data = new Dictionary<string, object>
                {
                    [Constants.TYPE] = Constants.FLOWSWITCH,
                    [Constants.EXPRESSION] = GetCleanExpressionText(flowSwitch.Expression) ?? Constants.SWITCH
                    //[Constants.EXPRESSION] = flowSwitch.Expression?.ToString() ?? Constants.SWITCH
                }
            };

            CurrentY += 150;

            // If the expression is a DsfFlowSwitchActivity, extract more detailed information
            if (flowSwitch.Expression != null)
            {
                var expression = flowSwitch.Expression;

                if (expression.GetType().Name.Contains(nameof(DsfFlowSwitchActivity)))
                {
                    ProcessSwitchActivityReflectionString(expression, flowSwitch, cell);
                }
                else
                {
                    ProcessBasicExpression(expression, cell);
                }
            }

            return cell;
        }

        private static void ProcessBasicExpression(object? expression, Cell cell)
        {
            string? displayName = null;

            // Try to get DisplayName if it's an Activity
            if (expression is Activity activity)
            {
                displayName = activity.DisplayName;
            }

            cell.data[Constants.DISPLAYNAME] = displayName ?? Constants.SWITCH;
            cell.label = displayName ?? Constants.SWITCH;
        }

        private static void ProcessSwitchActivityReflection(object expression, FlowSwitch<object> flowSwitch, Cell cell)
        {
            try
            {
                var accessors = GetOrCreatePropertyAccessors(expression.GetType());

                var displayName = GetPropertyValueSafely<string>(accessors.DisplayName, expression) ?? Constants.SWITCH;
                var expressionText = GetPropertyValueSafely<string>(accessors.ExpressionText, expression);
                var uniqueId = GetPropertyValueSafely<string>(accessors.UniqueID, expression);

                // Handle error properties
                var onErrorVariable = GetPropertyValueSafely<string>(accessors.OnErrorVariable, expression) ?? string.Empty;
                var onErrorWorkflow = GetPropertyValueSafely<string>(accessors.OnErrorWorkflow, expression) ?? string.Empty;
                var isEndedOnError = GetPropertyValueSafely<bool>(accessors.IsEndedOnError, expression);

                DsfNativeActivity<object>.SetOnErrorData(cell, isEndedOnError, onErrorVariable, onErrorWorkflow);

                cell.data[Constants.DISPLAYNAME] = displayName;
                cell.label = displayName;

                if (!string.IsNullOrEmpty(expressionText))
                {
                    var cleanVariable = ExtractSwitchVariable(expressionText);
                    if (!string.IsNullOrEmpty(cleanVariable))
                    {
                        cell.data[Constants.EXPRESSION] = $"[[{cleanVariable}]]";
                    }

                    var switchExpressionJson = CreateSwitchExpressionJson(expressionText, flowSwitch);
                    cell.data[Constants.SWITCH_EXPRESSION] = switchExpressionJson;
                }

                if (!string.IsNullOrEmpty(uniqueId))
                {
                    cell.data[Constants.UNIQUEID] = uniqueId;
                }
            }
            catch (Exception)
            {
                ProcessBasicExpression(expression, cell);
            }
        }

        private static void ProcessSwitchActivityReflectionString(object expression, FlowSwitch<string> flowSwitch, Cell cell)
        {
            try
            {
                var accessors = GetOrCreatePropertyAccessors(expression.GetType());

                var displayName = GetPropertyValueSafely<string>(accessors.DisplayName, expression) ?? Constants.SWITCH;
                var expressionText = GetPropertyValueSafely<string>(accessors.ExpressionText, expression);
                var uniqueId = GetPropertyValueSafely<string>(accessors.UniqueID, expression);

                // Handle error properties
                var onErrorVariable = GetPropertyValueSafely<string>(accessors.OnErrorVariable, expression) ?? string.Empty;
                var onErrorWorkflow = GetPropertyValueSafely<string>(accessors.OnErrorWorkflow, expression) ?? string.Empty;
                var isEndedOnError = GetPropertyValueSafely<bool>(accessors.IsEndedOnError, expression);

                DsfNativeActivity<object>.SetOnErrorData(cell, isEndedOnError, onErrorVariable, onErrorWorkflow);

                cell.data[Constants.DISPLAYNAME] = displayName;
                cell.label = displayName;

                if (!string.IsNullOrEmpty(expressionText))
                {
                    var cleanVariable = ExtractSwitchVariable(expressionText);
                    if (!string.IsNullOrEmpty(cleanVariable))
                    {
                        cell.data[Constants.EXPRESSION] = $"[[{cleanVariable}]]";
                    }

                    var switchExpressionJson = CreateSwitchExpressionJsonString(expressionText, flowSwitch);
                    cell.data[Constants.SWITCH_EXPRESSION] = switchExpressionJson;
                }

                if (!string.IsNullOrEmpty(uniqueId))
                {
                    cell.data[Constants.UNIQUEID] = uniqueId;
                }
            }
            catch (Exception)
            {
                ProcessBasicExpression(expression, cell);
            }
        }

        private static PropertyAccessors GetOrCreatePropertyAccessors(Type type)
        {
            return _propertyCache.GetOrAdd(type, CreatePropertyAccessors);
        }

        private static PropertyAccessors CreatePropertyAccessors(Type type)
        {
            const System.Reflection.BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            return new PropertyAccessors(
                DisplayName: type.GetProperty(Constants.PROPERTY_DISPLAYNAME, bindingFlags),
                ExpressionText: type.GetProperty(Constants.PROPERTY_EXPRESSIONTEXT, bindingFlags),
                UniqueID: type.GetProperty(Constants.PROPERTY_UNIQUEID, bindingFlags),
                OnErrorVariable: type.GetProperty(Constants.PROPERTY_ONERRORVARIABLE, bindingFlags),
                OnErrorWorkflow: type.GetProperty(Constants.PROPERTY_ONERRORWORKFLOW, bindingFlags),
                IsEndedOnError: type.GetProperty(Constants.PROPERTY_ISENDEDONERROR, bindingFlags)
            );
        }

        private static T? GetPropertyValueSafely<T>(PropertyInfo? property, object instance)
        {
            if (property?.CanRead == true && instance != null)
            {
                try
                {
                    var value = property.GetValue(instance);

                    if (value == null)
                        return default(T);

                    if (typeof(T) == typeof(bool) && value is bool boolValue)
                        return (T)(object)boolValue;

                    if (typeof(T) == typeof(string))
                        return (T)(object)(value.ToString() ?? string.Empty);

                    if (typeof(T).IsAssignableFrom(value.GetType()))
                        return (T)value;

                    return default(T);
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
            return default(T);
        }

        private static string CreateSwitchExpressionJson(string expressionText, FlowSwitch<object> flowSwitch)
        {
            try
            {
                var switchVariable = ExtractSwitchVariable(expressionText);

                var cases = flowSwitch.Cases.Select(c =>
                {
                    var key = c.Key?.ToString();
                    var value = c.Key?.ToString();
                    return new { Key = key, Value = value };
                }).ToList();

                var defaultCase = flowSwitch.Default != null ? Constants.SWITCH_DEFAULT : null;

                var switchExpression = new
                {
                    SwitchVariable = switchVariable,
                    Cases = cases,
                    DefaultCase = defaultCase
                };

                return JsonConvert.SerializeObject(switchExpression);
            }
            catch (Exception)
            {
                // If serialization fails, return a basic expression - exactly like original
                var fallback = JsonConvert.SerializeObject(new { SwitchVariable =  Constants.SWITCH_VARIABLE, Cases = new object[0] });
                return fallback;
            }
        }

        private static string CreateSwitchExpressionJsonString(string expressionText, FlowSwitch<string> flowSwitch)
        {
            try
            {
                var switchVariable = ExtractSwitchVariable(expressionText);

                var cases = flowSwitch.Cases.Select(c =>
                {
                    var key = c.Key?.ToString();
                    var value = c.Key?.ToString();
                    return new { Key = key, Value = value };
                }).ToList();

                var defaultCase = flowSwitch.Default != null ? Constants.SWITCH_DEFAULT : null;

                var switchExpression = new
                {
                    SwitchVariable = switchVariable,
                    Cases = cases,
                    DefaultCase = defaultCase
                };

                return JsonConvert.SerializeObject(switchExpression);
            }
            catch (Exception)
            {
                // If serialization fails, return a basic expression - exactly like original
                return JsonConvert.SerializeObject(new { SwitchVariable = Constants.SWITCH_VARIABLE, Cases = new object[0] });
            }
        }

        private static string ExtractSwitchVariable(string expressionText)
        {
            if (string.IsNullOrEmpty(expressionText))
            {
                return Constants.SWITCH_VARIABLE;
            }

            var match = _variableExtractionRegex.Match(expressionText);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Fallback to basic extraction
            var startIndex = expressionText.IndexOf("\"[[") + 3;
            var endIndex = expressionText.IndexOf("]]\"");
            if (startIndex > 2 && endIndex > startIndex)
            {
                return expressionText.Substring(startIndex, endIndex - startIndex);
            }

            return Constants.SWITCH_VARIABLE;
        }

        private static string? GetCleanExpressionText(object? expression)
        {
            if (expression == null) return null;

            // For DsfFlowSwitchActivity, get the clean ExpressionText directly
            if (expression is IFlowNodeActivity flowNodeActivity)
            {
                var expressionText = flowNodeActivity.ExpressionText;
                if (!string.IsNullOrEmpty(expressionText))
                {
                    var cleanVariable = ExtractSwitchVariable(expressionText);
                    if (!string.IsNullOrEmpty(cleanVariable))
                    {
                        return $"[[{cleanVariable}]]";
                    }
                }
            }

            // Try to get DisplayName if it's an Activity
            if (expression is Activity activity)
            {
                return activity.ToString();
            }

            // Final fallback
            return expression.ToString();
        }
    }

    public static class SwitchNodeProcessorExtensions
    {
        public static Cell CreateSwitchNode(this SwitchActivityDataHelper processor,
            FlowSwitch<object> flowSwitch, string nodeId) =>
            processor.CreateSwitchNode(flowSwitch, nodeId);

        public static Cell CreateSwitchNodeString(this SwitchActivityDataHelper processor,
            FlowSwitch<string> flowSwitch, string nodeId) =>
            processor.CreateSwitchNodeString(flowSwitch, nodeId);
    }
}