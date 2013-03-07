using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public interface ISettings
    {
        XElement ToXml();
    }
}
