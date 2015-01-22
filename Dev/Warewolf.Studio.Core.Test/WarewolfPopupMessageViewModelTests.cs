using System;
using System.Windows;
using Dev2.Common.Interfaces.PopupController;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core.Popup;

namespace Warewolf.Studio.Core.Test
{
    [TestClass]
    public class WarewolfPopupMessageViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfPopupMessage_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void WarewolfPopupMessage_Ctor_NullMessage_ExpectException()
  
        {
            //------------Setup for test--------------------------
            new WarewolfPopupMessage(null, null);
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfPopupMessage_Ctor")]
        public void WarewolfPopupMessage_Ctor_Valid_ExpectpropertiesSet()
        {
            //------------Setup for test--------------------------
            var popupmessage = new Mock<IPopupMessage>();
            var popupWindow = new Mock<IPopupWindow>();
            var msg = new WarewolfPopupMessage(popupmessage.Object, popupWindow.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(popupmessage.Object,msg.Message);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfPopupMessage_Focused")]
        public void WarewolfPopupMessage_Focused_Cancel()
        {
            //------------Setup for test--------------------------
            var popupmessage = new Mock<IPopupMessage>();
            var popupWindow = new Mock<IPopupWindow>();
            popupmessage.Setup(a => a.DefaultResult).Returns(MessageBoxResult.Cancel);
            var msg = new WarewolfPopupMessage(popupmessage.Object, popupWindow.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(msg.FocusCancel);
            Assert.IsFalse(msg.FocusNo);
            Assert.IsFalse(msg.FocusOk);
            Assert.IsFalse(msg.FocusYes);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfPopupMessage_Focused")]
        public void WarewolfPopupMessage_Focused_Ok()
        {
            //------------Setup for test--------------------------
            var popupmessage = new Mock<IPopupMessage>();
            var popupWindow = new Mock<IPopupWindow>();
            popupmessage.Setup(a => a.DefaultResult).Returns(MessageBoxResult.OK);
            var msg = new WarewolfPopupMessage(popupmessage.Object, popupWindow.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(msg.FocusCancel);
            Assert.IsFalse(msg.FocusNo);
            Assert.IsTrue(msg.FocusOk);
            Assert.IsFalse(msg.FocusYes);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfPopupMessage_Focused")]
        public void WarewolfPopupMessage_Focused_Yes()
        {
            //------------Setup for test--------------------------
            var popupmessage = new Mock<IPopupMessage>();
            var popupWindow = new Mock<IPopupWindow>();
            popupmessage.Setup(a => a.DefaultResult).Returns(MessageBoxResult.Yes);
            var msg = new WarewolfPopupMessage(popupmessage.Object, popupWindow.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(msg.FocusCancel);
            Assert.IsFalse(msg.FocusNo);
            Assert.IsFalse(msg.FocusOk);
            Assert.IsTrue(msg.FocusYes);
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfPopupMessage_Focused")]
        public void WarewolfPopupMessage_Focused_No()
        {
            //------------Setup for test--------------------------
            var popupmessage = new Mock<IPopupMessage>();
            var popupWindow = new Mock<IPopupWindow>();
            popupmessage.Setup(a => a.DefaultResult).Returns(MessageBoxResult.No);
            var msg = new WarewolfPopupMessage(popupmessage.Object, popupWindow.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(msg.FocusCancel);
            Assert.IsTrue(msg.FocusNo);
            Assert.IsFalse(msg.FocusOk);
            Assert.IsFalse(msg.FocusYes);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfPopupMessage_Focused")]
        public void WarewolfPopupMessage_ButtonsClicked_AssertResult()
        {
            //------------Setup for test--------------------------
            var popupmessage = new Mock<IPopupMessage>();
            var popupWindow = new Mock<IPopupWindow>();
            popupmessage.Setup(a => a.DefaultResult).Returns(MessageBoxResult.No);
            var msg = new WarewolfPopupMessage(popupmessage.Object, popupWindow.Object);

            //------------Assert Results-------------------------
            msg.CancelClicked();
            Assert.AreEqual(MessageBoxResult.Cancel ,msg.Result);
            msg.YesClicked();
            Assert.AreEqual(MessageBoxResult.Yes, msg.Result);
            msg.NoClicked();
            Assert.AreEqual(MessageBoxResult.No, msg.Result);
            msg.OkClicked();
            Assert.AreEqual(MessageBoxResult.OK, msg.Result);
        }
        // ReSharper restore InconsistentNaming
    }
}
