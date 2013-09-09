using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public class ExtractionException : Exception {

        public ExtractionException(string msg) : base(msg) { }
    }
}
