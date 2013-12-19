using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Studio.Core.Messages;

namespace Dev2.Messages
{
    public class SetSelectedItemInExplorerTree: IMessage
    {
        public string NodeNameToSelect { get; set; }

        public SetSelectedItemInExplorerTree(string nodeNameToSelect)
        {
            NodeNameToSelect = nodeNameToSelect;
        }
    }
}
