/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;


namespace Dev2.Studio.Core.Activities.Utils
{
    public static class ModelItemUtils
    {
        public static void SetProperty<T>(this ModelItem modelItem, string propertyName, T value)
        {
            SetProperty(propertyName, value, modelItem);
        }

        public static void SetProperty<T>(string propertyName, T value, ModelItem modelItem)
        {
            if (propertyName != null)
            {
                if (modelItem != null)
                {
                    var modelProperty = modelItem.Properties[propertyName];
                    if (modelProperty != null)
                    {

                        if (modelProperty.PropertyType == typeof(InArgument<T>))
                        {
                            modelProperty.SetValue(InArgument<T>.FromValue(value));
                        }
                        else
                        {
                            modelProperty.SetValue(value);
                        }
                    }
                }
            }
        }


        public static ModelItem CreateModelItem(object parent, object objectToMakeModelItem)
        {
            EditingContext ec = new EditingContext();
            ModelTreeManager mtm = new ModelTreeManager(ec);

            return mtm.CreateModelItem(CreateModelItem(parent), objectToMakeModelItem);
        }


        public static ModelItem CreateModelItem()
        {
            return CreateModelItem(new object());
        }

        public static ModelItem CreateModelItem(object objectToMakeModelItem)
        {
            EditingContext ec = new EditingContext();
            ModelTreeManager mtm = new ModelTreeManager(ec);

            mtm.Load(objectToMakeModelItem);

            return mtm.Root;
        }

        public static T GetProperty<T>(this ModelItem modelItem, string propertyName)
        {
            var modelProperty = modelItem.Properties[propertyName];
            object value = default(T);
            if (modelProperty != null)
            {
                if (modelProperty.PropertyType == typeof(InArgument<T>))
                {
                    if (modelProperty.ComputedValue is InArgument<T> arg)
                    {
                        value = arg.Expression.ToString();
                    }
                }
                else
                {
                    value = modelProperty.ComputedValue;
                }

                if (value != null)
                {
                    if (typeof(T) == typeof(Guid))
                    {
                        Guid.TryParse(value.ToString(), out Guid guid);
                        value = guid;
                    }
                }
            }
            return (T)value;
        }

        public static object GetProperty(string propertyName, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties[propertyName];
            return modelProperty != null ? modelProperty.ComputedValue : null;
        }

        public static object GetProperty(this ModelItem modelItem, string propertyName)
        {
            return GetProperty(propertyName, modelItem);
        }

        public static Guid GetUniqueID(ModelItem modelItem)
        {
            var instanceIDStr = GetProperty("UniqueID", modelItem) as string;
            Guid.TryParse(instanceIDStr, out var instanceID);
            return instanceID;
        }

        public static Guid TryGetResourceID(ModelItem modelItem)
        {
            var resourceIdArg = modelItem.Properties["ResourceID"];
            if (resourceIdArg != null && resourceIdArg.ComputedValue != null)
            {
                if (resourceIdArg.ComputedValue is InArgument<Guid> argument)
                {
                    var resourceIdStr = argument.Expression;
                    return Guid.Parse(resourceIdStr.ToString());
                }
                return (Guid)resourceIdArg.ComputedValue;
            }
            return Guid.Empty;
        }

        public static T GetCurrentValue<T>(this ModelItem modelItem) where T : class
        {
            var currentValue = modelItem.GetCurrentValue();
            return currentValue as T;
        }

        public static ImageSource GetImageSourceForTool(this ModelItem modelItem)
        {
            var computedValue = modelItem.GetCurrentValue();
            if (computedValue is FlowStep)
            {
                if (modelItem.Content?.Value != null)
                {
                    computedValue = modelItem.Content.Value.GetCurrentValue();
                }
            }
            var type = computedValue.GetType();
            if (type.Name == "DsfDecision" || type.Name == "FlowDecision")
            {
                type = typeof(DsfFlowDecisionActivity);
            }
            if (type.Name == "DsfSwitch")
            {
                type = typeof(DsfFlowSwitchActivity);
            }
            var currentApp = CustomContainer.Get<IApplicationAdaptor>();
            var application = currentApp ?? new ApplicationAdaptor(Application.Current);
            if (type.GetCustomAttributes().Any(a => a is ToolDescriptorInfo))
            {
                var desc = type.GetDescriptorFromAttribute();
                return application?.TryFindResource(desc.Icon) as ImageSource;
            }
            return application?.TryFindResource("Explorer-WorkflowService") as ImageSource;

        }
    }
}
