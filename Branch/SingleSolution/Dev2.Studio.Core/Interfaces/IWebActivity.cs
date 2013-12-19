using System;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.Interfaces {

    public interface IWebActivity : IWorkSurfaceObject {
        object WebActivityObject { get; set; }
        IContextualResourceModel ResourceModel { get; set; }
        string WebsiteServiceName { get; set; }
        string MetaTags { get; set; }
        string FormEncodingType { get; set; }
        string XMLConfiguration { get; set; }
        string Html { get; set; }
        string ServiceName { get; set; }
        string LiveInputMapping { get; set; }
        string LiveOutputMapping { get; set; }
        string SavedOutputMapping { get; set; }
        string SavedInputMapping { get; set; }
        Type UnderlyingWebActivityObjectType { get; }
    }

}
