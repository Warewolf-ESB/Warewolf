using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;

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
        List<NameValue> Headers { get; set; }
        string PostData { get; set; }
        string SourceUrl { get; set; }
        string Response { get; set; }
    }
}

