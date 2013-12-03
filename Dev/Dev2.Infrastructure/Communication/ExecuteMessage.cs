using System;
using System.Text;

namespace Dev2.Communication
{
    public class ExecuteMessage
    {
        public StringBuilder Data { get; set; }
        public Guid WorkspaceID { get; set; }
        public Guid DataListID { get; set; }
    }
}