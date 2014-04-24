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

                int len = (int)stream.Length;
                byte[] data = new byte[len];
                stream.Read(data, 0, len);

                var location = name + ".dll";

                if(!string.IsNullOrEmpty(dirToPlaceIn) && !Directory.Exists(dirToPlaceIn))
                {
                    Directory.CreateDirectory(dirToPlaceIn);
                    location = Path.Combine(dirToPlaceIn, location);

                    // its already there ;)
                    if(File.Exists(location))
                    {
                        return location;
                    }
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
