using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public class ShapedInputTO {


        public string ShapedDL { get; private set; }

        public string TransientDLItems { get; private set; }

        internal ShapedInputTO(string shapedDL, string dlItems) {
            ShapedDL = shapedDL;
            TransientDLItems = dlItems;
        }
    }
}
