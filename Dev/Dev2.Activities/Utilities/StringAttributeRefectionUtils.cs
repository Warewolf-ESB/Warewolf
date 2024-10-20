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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dev2.Utilities
{
    public static class StringAttributeRefectionUtils
    {
        public static IEnumerable<PropertyInfo> ExtractAdornedProperties<T>(object objectToReflect)
        {
            var sourceType = objectToReflect.GetType();
            var properties = sourceType.GetProperties().Where(c => c.GetCustomAttributes(typeof(T), true).Any());
            return properties;
        }    
    }
}
