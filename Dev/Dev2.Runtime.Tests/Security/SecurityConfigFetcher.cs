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
