using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeServiceViewModel
    {
        ISourceToolRegion<IExchangeSource> SourceRegion { get; set; }
        IExchangeInputRegion InputArea { get; set; }
        IOutputsToolRegion OutputsRegion { get; set; }
        void ErrorMessage(Exception exception, bool hasError);
        void SetDisplayName(string displayName);
        bool GenerateOutputsVisible { get; set; }
    }

    public interface IExchangeInputRegion : IToolRegion
    {
        IList<IServiceInput> Inputs { get; set; }
    }
}
