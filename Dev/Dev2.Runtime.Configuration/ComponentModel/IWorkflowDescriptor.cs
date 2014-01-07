using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.ComponentModel
{
    public interface IWorkflowDescriptor
    {
        string ResourceID { get; set; }
        string ResourceName { get; set; }
        bool IsSelected { get; set; }
        XElement ToXml();
    }
}
