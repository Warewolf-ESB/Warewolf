using System.IO;
using System.Reflection;

namespace Tu
{
    public static class ResourceFetcher
    {
        public static string Fetch(string resourceNamespace, string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using(var stream = assembly.GetManifestResourceStream(string.Format("{0}.{1}", resourceNamespace, resourceName)))
            {
                using(var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
