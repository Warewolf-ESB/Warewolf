using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Core.AppResources.ExtensionMethods
{
    public static class ModelPropertyCollectionExtensions
    {
        /// <summary>
        /// Sets the value of a property with in a model properties collection, if the property doesn't exist no action is taken.
        /// </summary>
        /// <param name="properties">The model property collection.</param>
        /// <param name="propertyName">Name of the property to set.</param>
        /// <param name="value">The value to set.</param>
        public static void SetValue(this ModelPropertyCollection properties, string propertyName, object value)
        {
            ModelProperty mp = properties.Find(propertyName);
            if (mp != null)
            {
                mp.SetValue(value);
            }
        }
    }
}
