using Dev2.Common.Interfaces.Toolbox;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

namespace Dev2.Activities.Exchange
{
    [ToolDescriptorInfo("Resources-Service", "Exchange Email Connector", ToolType.Native, "8926E59B-18A3-03BB-A92F-6090C5C3EA80", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfExchangeActivity : DsfActivity
    {
        public DsfExchangeActivity()
        {
            Type = "Exchange Email Connector";
            DisplayName = " Exchange Email Connector";
        }

        public string To { get; set; }
        public string Username { get; set; }
        public string Paswword { get; set; }
        public string Body { get; set; }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            
        }
    }
}
