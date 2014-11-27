
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Reflection;
using System.Xml.Linq;

namespace Dev2.Tests.Activities.XML
{
    /// <summary>
    /// Utility class for retrieving embedded XML resources.
    /// <remarks>
    /// The target XML files must be in the same folder as this class
    /// with their build action set to "Embedded Resource".
    /// </remarks>
    /// </summary>
    public static class XmlResource
    {
        /// <summary>
        /// Fetches the contents of the embedded XML file with the specified name.
        /// </summary>
        /// <param name="name">The name of the XML file excluding extension.</param>
        /// <returns>The contents of the embedded XML file.</returns>
        public static XElement Fetch(string name)
        {
            var resourceName = string.Format("Dev2.Tests.Activities.XML.{0}.xml", name);
            var assembly = Assembly.GetExecutingAssembly();
            using(var stream = assembly.GetManifestResourceStream(resourceName))
            {
                return XElement.Load(stream);
            }
        }

    }
}
