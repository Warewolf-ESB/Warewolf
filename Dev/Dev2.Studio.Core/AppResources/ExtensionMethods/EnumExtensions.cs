
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
using System.ComponentModel.DataAnnotations;
using Dev2.Studio.Core.AppResources.Attributes;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.ExtensionMethods
{
    public static class EnumExtensions
    {

        public static string GetTreeDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(TreeCategory))
                        as TreeCategory;

            return attribute == null ? value.ToString() : attribute.Value;
        }

        public static string GetIconLocation(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(IconLocation))
                        as IconLocation;

            return attribute == null ? string.Empty : attribute.Value;
        }

        public static int GetDisplayOrder(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(DisplayAttribute))
                        as DisplayAttribute;

            return attribute == null ? 0 : attribute.Order;
        }
    }
}
