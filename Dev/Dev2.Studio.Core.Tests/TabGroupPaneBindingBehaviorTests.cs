using Dev2.Studio.AppResources.Behaviors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Infragistics.Windows.DockManager;
using Dev2.Net6.Compatibility;

namespace Dev2.Core.Tests
{
    [STATestClass]
    public class TabGroupPaneBindingBehaviorTests
    {
        [STATestMethod]
        public void TabGroupPaneBindingBehavior_SetDocumentHost_CanSetDocumentHost()
        {
            //------------Setup for test-------------------------
            var myTabGroupPaneBindingBehavior = new TabGroupPaneBindingBehavior
            {
                //------------Execute Test---------------------------
                DocumentHost = new DocumentContentHost()
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(myTabGroupPaneBindingBehavior.DocumentHost);
        }
    }
}
