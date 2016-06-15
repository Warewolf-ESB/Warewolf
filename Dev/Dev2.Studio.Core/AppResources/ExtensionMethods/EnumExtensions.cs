
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Studio.Core.AppResources.Attributes;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.ExtensionMethods
{
    public static class EnumExtensions
    {
        public static string GetIconLocation(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(IconLocation))
                        as IconLocation;

            return attribute == null ? string.Empty : attribute.Value;
        }
    }
}
