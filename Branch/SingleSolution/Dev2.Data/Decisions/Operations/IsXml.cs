using System;
using Dev2.Data.Util;

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
            var data = DataListUtil.AdjustForEncodingIssues(cols[0]);
            bool isFragment;
            var isXml = DataListUtil.IsXml(data, out isFragment);
            return isXml || isFragment;
        }
    }
}
