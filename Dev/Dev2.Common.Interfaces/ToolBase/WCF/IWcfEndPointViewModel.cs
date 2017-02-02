using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.ToolBase.WCF
{
    public interface IWcfEndPointViewModel
    {
        ISourceToolRegion<IWcfServerSource> SourceRegion { get; set; }
        IActionToolRegion<IWcfAction> ActionRegion { get; set; } 
        IWcfInputRegion InputArea { get; set; }
        IOutputsToolRegion OutputsRegion { get; set; }
        bool GenerateOutputsVisible { get; set; }
        IWcfService ToModel();
        void ErrorMessage(Exception exception, bool hasError);
        void SetDisplayName(string displayName);
    }

    public interface IWcfInputRegion : IToolRegion
    {
        ICollection<IServiceInput> Inputs { get; set; } 
    }
}
