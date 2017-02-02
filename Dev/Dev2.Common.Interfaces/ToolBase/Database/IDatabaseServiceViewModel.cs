using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces.ToolBase.Database
{
    public interface IDatabaseServiceViewModel
    {
        ISourceToolRegion<IDbSource> SourceRegion { get; set; }
        IActionToolRegion<IDbAction> ActionRegion { get; set; } 
        IDatabaseInputRegion InputArea { get; set; }
        IOutputsToolRegion OutputsRegion { get; set; }

        bool GenerateOutputsVisible { get; set; }

        IDatabaseService ToModel();

        void ErrorMessage(Exception exception, bool hasError);

        void SetDisplayName(string displayName);
    }

    public interface IDatabaseInputRegion : IToolRegion
    {
        ICollection<IServiceInput> Inputs { get; set; }
    }
}
