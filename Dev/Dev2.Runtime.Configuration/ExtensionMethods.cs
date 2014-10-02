
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Xml.Linq;

namespace Dev2.Runtime.Configuration
{
    public static class ExtensionMethods
    {
        public static string AttributeSafe(this XElement elem, string name)
        {
            if(elem == null || string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            var attr = elem.Attribute(name);
            return attr == null ? string.Empty : attr.Value;
        }

        public static string ElementSafe(this XElement elem, string name)
        {
            if(elem == null || string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var child = elem.Element(name);
            return child == null ? string.Empty : child.Value;
        }

    }
}
