using System;
using Dev2.Studio.AppResources.Behaviors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class TabGroupPaneBindingBehaviorTests
    {
        [TestMethod]
        public void TabGroupPaneBindingBehavior_SetDocumentHost_CanSetDocumentHost()
        {
            //------------Setup for test-------------------------
            var myTabGroupPaneBindingBehavior = new TabGroupPaneBindingBehavior();
            //------------Execute Test---------------------------
            myTabGroupPaneBindingBehavior.DocumentHost = new Infragistics.Windows.DockManager.DocumentContentHost();
            //------------Assert Results-------------------------
            Assert.IsNotNull(myTabGroupPaneBindingBehavior.DocumentHost);
        }
    }
}
