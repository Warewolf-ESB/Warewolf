using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2 {
    public interface IFrameworkWebServer {
        void Start();
        void Start(string endpointAddress);
        void Stop();
    }
}
