using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.DataList;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;

namespace Dev2.Studio.ViewModels
{
    public class ContextViewModel : BaseViewModel
    {
        public Tuple<Guid, Guid> ID { get; set; }

        public DebugOutputViewModel DebugOutputViewModel { get; set; }

        public DataListViewModel DataListViewModel { get; set; }

        public WorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }
    }
}
