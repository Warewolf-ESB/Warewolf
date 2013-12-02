namespace Dev2.Studio.UI.Tests.UIMaps.WebServiceWizardUIMapClasses
{
    using System.Drawing;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


    public partial class WebServiceWizardUIMap
    {
        private void ClickNewWebSource()
        {
            var control = StudioWindow.GetChildren()[0].GetChildren()[0];
            control.WaitForControlEnabled();
            Mouse.Click(control, new Point(410, 79));
        }
    }
}
