using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core {
    public class WebCommunicationResponse : IWebCommunicationResponse
    {
        public string ContentType { get; set; }

        public long ContentLength { get; set; }

        public string Content { get; set; }
    }
}
