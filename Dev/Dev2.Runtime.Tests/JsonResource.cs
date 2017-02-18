using System.IO;
using System.Reflection;

namespace Dev2.Tests.Runtime
{
    public class JsonResource
    {
        /// <summary>
        /// Fetches the contents of the embedded XML file with the specified name.
        /// </summary>
        /// <param name="name">The name of the XML file excluding extension.</param>
        /// <returns>The contents of the embedded XML file.</returns>
        public static string Fetch(string name)
        {
            var resourceName = $"Dev2.Tests.Runtime.Json.{name}.json";
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return string.Empty;
                }

                using (StreamReader sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
