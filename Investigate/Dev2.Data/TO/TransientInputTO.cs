using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public class TransientInputTO {
        // WHY IS THE PROPERTY ACCESSOR !!!
        // THIS SHOULD BE ACCESSING A VARIABLE, WHY WOULD WE BOTHER WITH PROPERTIES IF THIS WERE THE CASE
        // EXAMPLE public string CleanDL { get { return _cleanDL; }; } 
        public string CleanDL { get; private set; }

        public IList<IDev2Definition> TransientRegions { get; private set; }

        internal TransientInputTO(string cleanDL) {
            CleanDL = cleanDL;
            TransientRegions = new List<IDev2Definition>();
        }
    }
}
