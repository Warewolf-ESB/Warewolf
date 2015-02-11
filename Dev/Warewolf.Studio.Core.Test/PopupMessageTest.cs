using System;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Studio.Core.Popup;

namespace Warewolf.Studio.Core.Test
{
    [TestClass]
    public class PopupMessageTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PopupMessage_SillyTest")]
        public void PopupMessage_SillyTest_Create_ExpectValuesSet()
        {
            //------------Setup for test--------------------------
            var popupMessage = new PopupMessage()
            {
                Buttons = MessageBoxButton.OK,
                DefaultResult = MessageBoxResult.No,Description = "bob",Header="b",DontShowAgainKey = "mopo",Image = MessageBoxImage.Asterisk

            };

            Assert.AreEqual(popupMessage.Buttons,MessageBoxButton.OK);
            Assert.AreEqual("bob",popupMessage.Description);
            Assert.AreEqual("b",popupMessage.Header);
            Assert.AreEqual(MessageBoxImage.Asterisk,popupMessage.Image);
            Assert.AreEqual("mopo",popupMessage.DontShowAgainKey);
        }
    }
}
