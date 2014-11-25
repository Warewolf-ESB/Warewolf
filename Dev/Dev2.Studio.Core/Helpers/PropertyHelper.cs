
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
using System.Linq;
using System.Reflection;

namespace Dev2.Helpers
{
    public static class PropertyHelper
    {
        public static void SetValues(Object instance, Tuple<string, object>[] properties)
        {
            if (instance == null) return;
            if (properties == null || !properties.Any()) return;

            foreach (var property in properties)
            {
                SetValue(instance, property);
            }
        }

        public static void SetValue(Object instance, Tuple<string, object> tuple)
        {
            if (instance == null) return;

            var prop = instance.GetType().GetProperty(tuple.Item1, BindingFlags.Public | BindingFlags.Instance);
            if (null != prop && prop.CanWrite)
            {
                prop.SetValue(instance, tuple.Item2, null);
            }
        }
    }
}
