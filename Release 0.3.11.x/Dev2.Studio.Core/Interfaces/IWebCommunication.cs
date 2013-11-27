using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IWebCommunication
    {
        IWebCommunicationResponse Get(string uri);
        IWebCommunicationResponse Post(string uri, string data);
    }
}
