using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;

namespace Dev2.Runtime.ESB.Management.Sources
{
    /// <summary>
    /// Root source for management services
    /// Replace : GetDefaultSources() in DynamicServicesHost
    /// </summary>
    public class ManagementServiceSource 
    {
        public Source CreateSourceEntry()
        {
            Source s = new Source();
            s.Name = HandlesType();
            s.Type = enSourceType.ManagementDynamicService;

            return s;
        }

        public string HandlesType()
        {
            return "ManagementDynamicService";
        }
    }
}
