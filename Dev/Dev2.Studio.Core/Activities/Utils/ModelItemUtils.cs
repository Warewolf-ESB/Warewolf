using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using Dev2.Network.Messaging.Messages;

namespace Dev2.Studio.Core.Activities.Utils
{
    public static class ModelItemUtils
    {
        public static void SetProperty<T>(string propertyName, T value, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties[propertyName];
            if(modelProperty != null)
            {
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
        
        public static ModelItem CreateModelItem()
        {
            return CreateModelItem(new object());
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
                if (resourceIDArg.ComputedValue is InArgument<Guid>)
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
