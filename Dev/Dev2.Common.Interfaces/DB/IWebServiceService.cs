using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Dev2.Common.Interfaces.DB
{
    public interface IWebService
    {
        string Name { get; set; }
        string Path { get; set; }
        IWebServiceSource Source { get; set; }
        IList<IWebserviceInputs> Inputs { get; set; }
        IList<IWebserviceOutputs> OutputMappings { get; set; }
        string QueryString { get; set; }
        Guid Id { get; set; }
    }
}