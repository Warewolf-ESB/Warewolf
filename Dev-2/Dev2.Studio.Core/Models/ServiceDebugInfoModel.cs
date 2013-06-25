using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Models {
    public class ServiceDebugInfoModel : IServiceDebugInfoModel 
    {
        public IContextualResourceModel ResourceModel { get; set; }
        public string ServiceInputData { get; set; }
        public DebugMode DebugModeSetting { get; set; }
        public bool RememberInputs { get; set; }
    }
}
