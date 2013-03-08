using System.Xml.Linq;

namespace Dev2.Runtime.Configuration
{
    public static class ExtensionMethods
    {
        public static string AttributeSafe(this XElement elem, string name)
        {
            if(elem == null || string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            var attr = elem.Attribute(name);
            return attr == null ? string.Empty : attr.Value;
        }

        public static string ElementSafe(this XElement elem, string name)
        {
            if(elem == null || string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var child = elem.Element(name);
            return child == null ? string.Empty : child.Value;
        }

    }
}
