using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2 {
    interface IFrameworkPluginComponent {
        string Invoke(DynamicServices.DynamicService test);
    }
}
