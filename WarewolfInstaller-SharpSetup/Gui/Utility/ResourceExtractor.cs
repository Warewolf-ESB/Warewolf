using System.IO;
using System.Reflection;

namespace Gui.Utility
{
    /// <summary>
    /// Extract resources for the installer ;)
    /// </summary>
    public static class ResourceExtractor
    {

        public static Stream Fetch(string name)
        {
            var resourceName = string.Format("Gui.UnpackResources.{0}", name);
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(resourceName);
        }

        public static Stream FetchDependency(string name)
        {
            var resourceName = string.Format("Gui.UnpackResources.Dependencies.{0}", name);
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(resourceName);   
        }
    }
}
