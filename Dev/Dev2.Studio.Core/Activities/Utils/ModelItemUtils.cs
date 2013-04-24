using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;

namespace Dev2.Studio.Core.Activities.Utils
{
    public static class ModelItemUtils
    {
        public static void SetProperty<T>(string propertyName, T value, ModelItem modelItem)
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

        public static ModelItem CreateModelItem(object objectToMakeModelItem)
        {
            EditingContext ec = new EditingContext();
            ModelTreeManager mtm = new ModelTreeManager(ec);

            mtm.Load(objectToMakeModelItem);

            return mtm.Root;
        }

        public static object GetProperty(string propertyName, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties[propertyName];
            return modelProperty != null ? modelProperty.ComputedValue : null;
        }
    }
}
