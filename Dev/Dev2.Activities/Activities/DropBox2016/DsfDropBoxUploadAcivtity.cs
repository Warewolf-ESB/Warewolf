using Dev2.Common.Interfaces.Toolbox;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

namespace Dev2.Activities.DropBox2016
{
    [ToolDescriptorInfo("DropBoxLogo", "Upload to Drop Box", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C8C9EA2E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Connectors", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfDropBoxUploadAcivtity : DsfActivity
    {
        public DsfDropBoxUploadAcivtity()
        {
            DisplayName = "Upload to Drop Box";
            Type = "Upload to Drop Box";
        }
    }
}