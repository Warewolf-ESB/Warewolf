using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract.TO
{
    public class DataListPayloadFrameTO<T>
    {
        public T Value { get; private set; }
        public string Expression { get; private set; }

        public DataListPayloadFrameTO(string exp, T val)
        {
            Value = val;
            Expression = exp;
        }
    }
}
