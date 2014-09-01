using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Runtime.ServiceModel.Data
{
    // BUG 9626 - 2013.06.11 - TWR: refactored
    internal class RecordsetNode
    {
        public RecordsetNode()
        {
            MyProps = new Dictionary<string, IPath>();
            ChildNodes = new List<RecordsetNode>();
        }

        public string Name { get; set; }
        public Dictionary<string, IPath> MyProps { get; set; }
        public List<RecordsetNode> ChildNodes { get; set; }
    }
}