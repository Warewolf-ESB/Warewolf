using System;
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
            factory.Setup(a => a.Create(It.IsAny<IPopupMessage>(),It.IsAny<IPopupWindow>())).Returns(vm.Object);
            var popupController = new PopupController(factory.Object,mockPopupWindow.Object);
            
            //------------Execute Test---------------------------
            popupController.Show( PopupMessages.GetDeleteConfirmation("bob"));

            //------------Assert Results-------------------------
            vm.Verify(a=>a.Show(),Times.Once());
            factory.Verify(a => a.Create(It.IsAny<IPopupMessage>(), It.IsAny<IPopupWindow>()));
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