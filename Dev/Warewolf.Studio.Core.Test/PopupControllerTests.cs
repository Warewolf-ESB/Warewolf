using System;
using System.Windows;
using Dev2.Common.Interfaces.PopupController;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core.Popup;

namespace Warewolf.Studio.Core.Test
{
    [TestClass]
    public class PopupControllerTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PopupController_ShowMessage")]
        public void PopupController_ShowMessage_ValidMessage_ShowCalled()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<IPopupMessageBoxFactory>();
            var mockPopupWindow = new Mock<IPopupWindow>();
            var vm = new Mock<IDev2MessageBoxViewModel>();
            factory.Setup(a => a.Create(It.IsAny<IPopupMessage>())).Returns(vm.Object);
            var popupController = new PopupController(factory.Object,mockPopupWindow.Object);
            
            //------------Execute Test---------------------------
            popupController.Show( PopupMessages.GetDeleteConfirmation("bob"));

            //------------Assert Results-------------------------
            vm.Verify(a=>a.Show(),Times.Once());
            factory.Verify(a => a.Create(It.IsAny<IPopupMessage>()));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PopupController_GetServerVersionMessage")]
        public void PopupController_GetServerVersionMessage_CorrectMessage()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<IPopupMessageBoxFactory>();
            var vm = new Mock<IDev2MessageBoxViewModel>();
            factory.Setup(a => a.Create(It.IsAny<IPopupMessage>())).Returns(vm.Object);
            
            //------------Execute Test---------------------------
            var serverVersionMessage = PopupMessages.GetServerVersionMessage("0.3.5","0.4.5");

            //------------Assert Results-------------------------
            Assert.AreEqual(MessageBoxButton.OK,serverVersionMessage.Buttons);
            Assert.AreEqual(MessageBoxImage.Information,serverVersionMessage.Image);
            var correctDescription = "Studio: Version 0.3.5" +
                                     Environment.NewLine +
                                     "Server: Version 0.4.5" +
                                     Environment.NewLine +
                                     Environment.NewLine +
                                     "Sofware development by: www.dev2.co.za"+
                                     Environment.NewLine;
            Assert.AreEqual(correctDescription, serverVersionMessage.Description);
            Assert.AreEqual("About Server and Studio", serverVersionMessage.Header);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PopupController_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopupController_Ctor_Null()
        {

            var popupController = new PopupController(null,null);



        }
         
    }
}