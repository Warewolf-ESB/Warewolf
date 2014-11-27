
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
using System.Text;

namespace Dev2.Tests.Runtime.Security
{
    public class SecurityConfigFetcher
    {
        /// <summary>
        /// Fetches the contents of the embedded XML file with the specified name.
        /// </summary>
        /// <param name="name">The name of the XML file excluding extension.</param>
        /// <returns>The contents of the embedded XML file.</returns>
        public static string Fetch(string name)
        {
            var resourceName = string.Format("Dev2.Tests.Runtime.Security.{0}", name);
            var assembly = Assembly.GetExecutingAssembly();
            using(var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if(stream == null)
                {
                    return string.Empty;
                }

                var len = stream.Length;
                byte[] bytes = new byte[len];
                stream.Read(bytes, 0, (int)len);

                return Encoding.UTF8.GetString(bytes);

            }
        }
    }
}
