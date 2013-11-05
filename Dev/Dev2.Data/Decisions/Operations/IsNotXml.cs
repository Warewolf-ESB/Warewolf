using System;
using Dev2.DataList.Contract;

namespace Dev2.Data.Decisions.Operations
{
    public class IsNotXml : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsNotXML;
        }

        public bool Invoke(string[] cols)
        {
            var data = DataListUtil.AdjustForEncodingIssues(cols[0]);
            bool isFragment;
            var isXml = DataListUtil.IsXml(data, out isFragment);
            return !(isXml || isFragment);
        }
    }
}
