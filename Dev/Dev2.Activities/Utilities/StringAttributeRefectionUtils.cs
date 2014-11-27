
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dev2.Utilities
{
    public class StringAttributeRefectionUtils
    {
        public static IEnumerable<PropertyInfo> ExtractAdornedProperties<T>(object objectToReflect)
        {
            Type sourceType = objectToReflect.GetType();
            IEnumerable<PropertyInfo> properties = sourceType.GetProperties().Where(c => c.GetCustomAttributes(typeof(T), true).Any());
            return properties;
        }    
    }
}
