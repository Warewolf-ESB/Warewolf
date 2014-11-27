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
using System.Collections;
using System.Linq;

namespace Dev2
{
    public static class TypeExtensionMethods
    {
        public static bool IsEnumerable(this Type type)
        {
            return type != typeof (string) && type.GetInterfaces().Contains(typeof (IEnumerable));
        }

        public static bool IsPrimitive(this Type type)
        {
            return type == typeof (string) || type.IsValueType || type.IsPrimitive;
        }
    }
}