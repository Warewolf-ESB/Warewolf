using Dev2.Common.Interfaces.PopupController;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core.Popup;

namespace Warewolf.Studio.Core.Test
{
    [TestClass]
    public class PopupMessageBoxFactoryTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PopupMessageBoxFactory_Create")]
        public void PopupMessageBoxFactory_Create_CorrectPropertySet_ExpectWellFormedObject()
        {
            //------------Setup for test--------------------------
            var popupMessageBoxFactory = new PopupMessageBoxFactory();
            var msg =  PopupMessages.GetNotConnected();
            //------------Execute Test---------------------------
            var x = popupMessageBoxFactory.Create(msg);
            //------------Assert Results-------------------------
            Assert.AreEqual(msg,x.Message);
            Assert.IsFalse(x.FocusCancel);
            Assert.IsFalse(x.FocusNo);
            Assert.IsFalse(x.FocusYes);
            Assert.IsFalse(x.FocusOk);
            Assert.IsFalse(x.IsActive);
        }
    }
}
