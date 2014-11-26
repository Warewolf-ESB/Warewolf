
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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

// ReSharper disable once CheckNamespace
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
            var modelProperty = modelItem.Properties[propertyName];
            if(modelProperty != null)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if(modelProperty.PropertyType == typeof(InArgument<T>))
                {
                    modelProperty.SetValue(InArgument<T>.FromValue(value));
                }
                else
                {
                    modelProperty.SetValue(value);
                }
            }
        }

        public static ModelItem CreateModelItem(object objectToMakeModelItem)
        {
            EditingContext ec = new EditingContext();
            ModelTreeManager mtm = new ModelTreeManager(ec);

            mtm.Load(objectToMakeModelItem);


            return mtm.Root;
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

        public static T GetProperty<T>(this ModelItem modelItem, string propertyName)
        {
            var modelProperty = modelItem.Properties[propertyName];
            object value = default(T);
            if(modelProperty != null)
            {
                if(modelProperty.PropertyType == typeof(InArgument<T>))
                {
                    var arg = modelProperty.ComputedValue as InArgument<T>;
                    if(arg != null)
                    {
                        value = arg.Expression.ToString();
                    }
                }
                else
                {
                    value = modelProperty.ComputedValue;
                }

                if(value != null)
                {
                    if(typeof(T) == typeof(Guid))
                    {
                        Guid guid;
                        Guid.TryParse(value.ToString(), out guid);
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

        /// <summary>
        /// Determines whether [is local service] [the specified URI].
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>
        ///   <c>true</c> if [is local service] [the specified URI]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLocalService(string uri)
        {
            if(uri.IndexOf("localhost:", StringComparison.Ordinal) >= 0 || uri.IndexOf("127.0.0.1:", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            return false;
        }

        public static Guid GetUniqueID(ModelItem modelItem)
        {
            var instanceIDStr = GetProperty("UniqueID", modelItem) as string;
            Guid instanceID;
            Guid.TryParse(instanceIDStr, out instanceID);
            return instanceID;
        }

        public static Guid TryGetResourceID(ModelItem modelItem)
        {
            var resourceIDArg = modelItem.Properties["ResourceID"];
            if(resourceIDArg != null && resourceIDArg.ComputedValue != null)
            {
                if(resourceIDArg.ComputedValue is InArgument<Guid>)
                {
                    var resourceIDStr = (resourceIDArg.ComputedValue as InArgument<Guid>).Expression;
                    return Guid.Parse(resourceIDStr.ToString());
                }
                return (Guid)resourceIDArg.ComputedValue;
            }
            return Guid.Empty;
        }
    }
}
