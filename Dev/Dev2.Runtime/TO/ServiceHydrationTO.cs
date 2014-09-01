using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.DynamicServices {
    public class ServiceHydrationTO {

        public string DataList { get; set; }

        public IList<IDev2Definition> InputMapping { get; set; }

        public IList<IDev2Definition> OutputMapping { get; set; }

    }
}
