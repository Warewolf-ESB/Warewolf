#pragma warning disable
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Runtime.Auditing
{
    public class AuditFilter
    {
        public AuditFilter() { }
        public string AuditType { get; set; }

        public DateTime CompletedDateTime { get; set; }

        public string ExecutingUser { get; set; }

        public string ExecutionID { get; set; }

        public bool IsRemoteWorkflow { get; set; }

        public bool IsSubExecution { get; set; }

        public string ParentID { get; set; }

        public string ServerID { get; set; }

        public DateTime StartDateTime { get; set; }

        public string WorkflowID { get; set; }

        public string WorkflowName { get; set; }

    }
}
