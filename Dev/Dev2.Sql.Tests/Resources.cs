using System.IO;
using System.Reflection;

namespace Dev2.Sql.Tests
{
    public static class Resources
    {
        public static string GetFromResources(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(string.Format("Dev2.Sql.Tests.Resources.{0}", resourceName)))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            return string.Empty;
        }
    }
}
