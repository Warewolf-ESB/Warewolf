using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Core.Interfaces {
    public interface IWebCommunicationResponse {
        string ContentType { get; set; }
        long ContentLength { get; set; }
        string Content { get; set; }
    }
}
