using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public class DataListMutationException : Exception{

        public DataListMutationException(string msg) : base(msg) { } 
    }
}
