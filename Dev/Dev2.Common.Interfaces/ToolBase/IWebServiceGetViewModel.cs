using System;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebServices;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IWebServiceGetViewModel
    {
        IOutputsToolRegion OutputsRegion { get; set; }
        IWebGetInputArea InputArea { get; set; }
        ISourceToolRegion<IWebServiceSource> SourceRegion { get; set; }
        bool GenerateOutputsVisible { get; set; }

        IWebService ToModel();

        void ErrorMessage(Exception exception, bool hasError);

        void SetDisplayName(string displayName);
    }
}