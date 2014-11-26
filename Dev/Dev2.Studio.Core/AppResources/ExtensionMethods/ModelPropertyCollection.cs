
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;

// ReSharper disable once CheckNamespace
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
            if(mp != null)
            {
                mp.SetValue(value);
            }
        }
    }
}
