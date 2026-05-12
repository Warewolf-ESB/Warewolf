/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using System.Reflection;

namespace Dev2.Tests.Runtime.Plugins
{
    public static class DllExtractor
    {
        /// <summary>
        /// Fetches the contents of the embedded XML file with the specified name.
        /// </summary>
        /// <param name="name">The name of the XML file excluding extension.</param>
        /// <param name="dirToPlaceIn">The dir automatic place information.</param>
        /// <returns>
        /// The contents of the embedded XML file.
        /// </returns>
        public static string UnloadToFileSystem(string name, string dirToPlaceIn)
        {
            var resourceName = string.Format("Dev2.Tests.Runtime.Plugins.{0}.dll", name);
            var assembly = Assembly.GetExecutingAssembly();
            using(var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if(stream == null)
                {
                    return string.Empty;
                }

                var len = (int)stream.Length;
                var data = new byte[len];
                stream.Read(data, 0, len);

                var baseDir = Path.GetDirectoryName(assembly.Location);
                var location = name + ".dll";

                if(!string.IsNullOrEmpty(dirToPlaceIn))
                {
                    var fullDirPath = Path.IsPathFullyQualified(dirToPlaceIn)
                        ? dirToPlaceIn
                        : Path.Combine(baseDir, dirToPlaceIn);

                    if(!Directory.Exists(fullDirPath))
                    {
                        Directory.CreateDirectory(fullDirPath);
                    }

                    location = Path.Combine(fullDirPath, location);
                }
                else
                {
                    location = Path.Combine(baseDir, location);
                }

                if(File.Exists(location))
                {
                    return location;
                }

                using(FileStream fs = new FileStream(location, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(data, 0, len);
                }

                return location;
            }
        }
    }
}
