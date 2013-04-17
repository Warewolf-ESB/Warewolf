using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;

namespace Dev2.DynamicServices {
    public class ServiceHydrationTO {

        public string DataList { get; set; }

        public IList<IDev2Definition> InputMapping { get; set; }

        public IList<IDev2Definition> OutputMapping { get; set; }

    }
}
