using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

namespace Dev2
{
 [ToolDescriptorInfo("Resources-Service", "Web Service Get", ToolType.Native, "6AEB1038-6332-46F9-8BDD-641DE4EA038E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfWebGetActivity : DsfActivity
    {
     
         IList<NameValue> Headers { get; set; }

         String QueryString { get; set; }

         IPluginSource Source { get; set; }

         public DsfWebGetActivity()
         {
                Type = "Web Get Request Connector";
                DisplayName = "Web Get Request Connector";
         }

    }
}
