using System.Collections.Generic;

namespace Dev2.DataList.Contract
{
    public class TransientInputTO {

        public string CleanDL { get; private set; }

        public IList<IDev2Definition> TransientRegions { get; private set; }

        internal TransientInputTO(string cleanDL) {
            CleanDL = cleanDL;
            TransientRegions = new List<IDev2Definition>();
        }
    }
}
