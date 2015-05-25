using System;
using System.Collections.Generic;
using System.Dynamic;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Dev2.Common.Interfaces.WebServices
{
    public interface IWebService
    {
        string Name { get; set; }
        string Path { get; set; }
        IWebServiceSource Source { get; set; }
        IList<IServiceInput> Inputs { get; set; }
        IList<IServiceOutputMapping> OutputMappings { get; set; }
        string QueryString { get; set; }
        string RequestUrl    { get; set; }
        Guid Id { get; set; }
        IList<INameValue> Headers { get; set; }
        string PostData { get; set; }
    }
}

