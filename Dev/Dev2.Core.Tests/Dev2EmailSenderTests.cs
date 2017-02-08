using System;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests
{
    [TestClass]
    public class Dev2EmailSenderTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenInstance_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                var sender = new Dev2EmailSender();

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.StackTrace);

            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SendEmail_GivenSetUpInfo_ShouldReturnSucces()
        {
            //---------------Set up test pack-------------------
            var exMailSender = new Mock<IExchangeEmailSender>();
            var excSource = new Mock<IExchange>();
            var i1 = new Mock<IWarewolfIterator>();

            i1.Setup(iterator => iterator.GetNextValue()).Returns("Micky@Dev2.co.za");
            i1.Setup(iterator => iterator.GetLength()).Returns(1);
            var i2 = new Mock<IWarewolfIterator>();
            i2.Setup(iterator => iterator.GetLength()).Returns(1);
            i2.Setup(iterator => iterator.GetNextValue()).Returns("Micky@Dev2.co.za");
            var i3 = new Mock<IWarewolfIterator>();
            i3.Setup(iterator => iterator.GetLength()).Returns(1);
            i3.Setup(iterator => iterator.GetNextValue()).Returns("Micky1@Dev2.co.za");
            var i4 = new Mock<IWarewolfIterator>();
            i4.Setup(iterator => iterator.GetLength()).Returns(1);
            i4.Setup(iterator => iterator.GetNextValue()).Returns("Subject");
            var i5 = new Mock<IWarewolfIterator>();
            i5.Setup(iterator => iterator.GetLength()).Returns(1);
            i5.Setup(iterator => iterator.GetNextValue()).Returns("Body Text");
            var i6 = new Mock<IWarewolfIterator>();
            i5.Setup(iterator => iterator.GetLength()).Returns(1);
            i6.Setup(iterator => iterator.GetNextValue()).Returns("Attachments");

            var iList = new WarewolfListIterator();
            iList.AddVariableToIterateOn(i1.Object);
            iList.AddVariableToIterateOn(i2.Object);
            iList.AddVariableToIterateOn(i3.Object);
            iList.AddVariableToIterateOn(i4.Object);
            iList.AddVariableToIterateOn(i5.Object);
            iList.AddVariableToIterateOn(i6.Object);
            var sender = new Mock<IDev2EmailSender>();
            var excEmailSender = new Mock<IExchangeEmailSender>();
            // ReSharper disable once RedundantAssignment
            var eR = new ErrorResultTO();
            excEmailSender.Setup(p => p.Send(It.IsAny<ExchangeService>(), It.IsAny<EmailMessage>()));
            sender.SetupGet(emailSender => emailSender.ExchangeService).Returns(new ExchangeService());
            sender.SetupGet(emailSender => emailSender.EmailSender).Returns(excEmailSender.Object);
            sender.Setup(q => q.SendEmail(It.IsAny<IExchange>(), It.IsAny<IWarewolfListIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), out eR))
                .Returns("Succes");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sender);
            //---------------Execute Test ----------------------
            // ReSharper disable once RedundantAssignment
            var er = new ErrorResultTO();
            try
            {
                var sendEmailResult = sender.Object.SendEmail(excSource.Object, iList, i1.Object, i2.Object, i3.Object, i4.Object, i5.Object, i6.Object, out er);
                Assert.AreEqual("Succes", sendEmailResult);
            }
            catch (Exception ex)
            {

                Assert.Fail(ex.StackTrace);
            }

            //---------------Test Result -----------------------
        }
    }
}
