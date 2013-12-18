namespace Dev2.Studio.UI.Tests.UIMaps.ServiceDetailsUIMapClasses
{
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    
    
    public partial class ServiceDetailsUIMap
    {
        public bool ServiceDetailsWindowExists()
        {
            WpfWindow theWindow = ServiceDetailsWindow();
            Point p = new Point();
            bool doesExist = theWindow.TryGetClickablePoint(out p);
            return doesExist;
        }

    }
}
