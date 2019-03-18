#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
                var modelProperty = modelItem?.Properties[propertyName];
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

        public static ModelItem CreateModelItem(object parent, object objectToMakeModelItem)
        {
            var ec = new EditingContext();
            var mtm = new ModelTreeManager(ec);

            return mtm.CreateModelItem(CreateModelItem(parent), objectToMakeModelItem);
        }


        public static ModelItem CreateModelItem() => CreateModelItem(new object());

        public static ModelItem CreateModelItem(object objectToMakeModelItem)
        {
            var ec = new EditingContext();
            var mtm = new ModelTreeManager(ec);

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

                if (value != null && typeof(T) == typeof(Guid))
                {
                    Guid.TryParse(value.ToString(), out Guid guid);
                    value = guid;
                }
            }
            return (T)value;
        }

        public static object GetProperty(string propertyName, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties[propertyName];
            return modelProperty?.ComputedValue;
        }

        public static object GetProperty(this ModelItem modelItem, string propertyName) => GetProperty(propertyName, modelItem);

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
            if (computedValue is FlowStep && modelItem.Content?.Value != null)
            {
                computedValue = modelItem.Content.Value.GetCurrentValue();
            }

            var type = computedValue.GetType();
            var image = GetImageSourceForToolFromType(type);
            return image;
        }
        public static ImageSource GetImageSourceForToolFromType(Type itemType)
        {
            var type = itemType;
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
