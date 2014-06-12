using System.Reflection;
using System.Xml.Linq;

namespace Dev2.Activities.Specs.Composition.DBSource
{
    /// <summary>
    /// Utility class for retrieving embedded XML resources.
    /// <remarks>
    /// The target XML files must be in the same folder as this class
    /// with their build action set to "Embedded Resource".
    /// </remarks>
    /// </summary>
    public static class XmlFetch
    {
        /// <summary>
        /// Fetches the contents of the embedded XML file with the specified name.
        /// </summary>
        /// <param name="name">The name of the XML file excluding extension.</param>
        /// <returns>The contents of the embedded XML file.</returns>
        public static XElement Fetch(string name)
        {
            var resourceName = string.Format("Dev2.Activities.Specs.Composition.DBSource.{0}.xml", name);
            var assembly = Assembly.GetExecutingAssembly();
            using(var stream = assembly.GetManifestResourceStream(resourceName))
            {
                return XElement.Load(stream);
            }
        }

    }
}
