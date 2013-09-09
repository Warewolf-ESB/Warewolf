using System;
using System.IO;
using Dev2.DataList.Contract;

namespace Dev2.Data.Decisions.Operations
{
    public class IsXml : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsXML;
        }

        public bool Invoke(string[] cols)
        {
            return DataListUtil.IsXml(cols[0]);
        }
    }
}
