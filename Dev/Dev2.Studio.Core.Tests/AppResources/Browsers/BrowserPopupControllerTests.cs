
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.AppResources.Browsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BrowserPopupControllerTests
    {
        #region ShowPopup

        [TestMethod]
        public void ExternalBrowserPopupControllerShowPopupExpectedReturnsTrue()
        {
            var controller = new ExternalBrowserPopupController();
            var result = controller.ShowPopup(null);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void InternalBrowserPopupControllerShowPopupExpectedReturnsFalse()
        {
            var controller = new InternalBrowserPopupController();
            var result = controller.ShowPopup(null);
            Assert.IsFalse(result);
        }

        #endregion

        #region ConfigurePopup

        [TestMethod]
        public void BrowserPopupControllerConfigurePopupWithoutPopupHandleExpectedDoesNothing()
        {
            var controller = new Mock<BrowserPopupControllerAbstract>();
            controller.Setup(c => c.FindPopup()).Returns(new IntPtr(0));
            controller.Setup(c => c.SetPopupForeground(It.IsAny<IntPtr>())).Verifiable();
            controller.Setup(c => c.SetPopupIcon(It.IsAny<IntPtr>())).Verifiable();
            controller.Setup(c => c.SetPopupTitle(It.IsAny<IntPtr>())).Verifiable();

            controller.Object.ConfigurePopup();

            controller.Verify(c => c.FindPopup(), Times.Once());
            controller.Verify(c => c.SetPopupForeground(It.IsAny<IntPtr>()), Times.Never());
            controller.Verify(c => c.SetPopupIcon(It.IsAny<IntPtr>()), Times.Never());
            controller.Verify(c => c.SetPopupTitle(It.IsAny<IntPtr>()), Times.Never());

        }

        [TestMethod]
        public void BrowserPopupControllerConfigurePopupWithPopupHandleExpectedSetsPopupProperties()
        {
            var controller = new Mock<BrowserPopupControllerAbstract>();
            controller.Setup(c => c.FindPopup()).Returns(new IntPtr(1));
            controller.Setup(c => c.SetPopupForeground(It.IsAny<IntPtr>())).Verifiable();
            controller.Setup(c => c.SetPopupIcon(It.IsAny<IntPtr>())).Verifiable();
            controller.Setup(c => c.SetPopupTitle(It.IsAny<IntPtr>())).Verifiable();

            controller.Object.ConfigurePopup();

            controller.Verify(c => c.FindPopup(), Times.Once());
            controller.Verify(c => c.SetPopupForeground(It.IsAny<IntPtr>()), Times.Once());
            controller.Verify(c => c.SetPopupIcon(It.IsAny<IntPtr>()), Times.Once());
            controller.Verify(c => c.SetPopupTitle(It.IsAny<IntPtr>()), Times.Once());
        }

        #endregion

    }
}
