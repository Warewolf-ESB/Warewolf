using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unlimited.Framework;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Studio.Core.Models {
    public class WorkflowInputDataModel : BaseValidatable {
        public string ElementName { get; set; }
        public string ElementValue { get; set; }

    }
}
