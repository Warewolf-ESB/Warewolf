using Dev2.Studio.AppResources.Behaviors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Infragistics.Windows.DockManager;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class TabGroupPaneBindingBehaviorTests
    {
        [TestMethod]
        [DoNotParallelize]
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
