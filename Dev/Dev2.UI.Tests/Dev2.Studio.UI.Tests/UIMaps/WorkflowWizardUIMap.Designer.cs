using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public partial class WorkflowWizardUIMap : UIMapBase
    {
        public WpfWindow GetWindow()
        {
            #region Variable Declarations
            WpfWindow theWindow = StudioWindow.GetChildren()[0] as WpfWindow;
            #endregion

            return theWindow;
        }
    }
}
