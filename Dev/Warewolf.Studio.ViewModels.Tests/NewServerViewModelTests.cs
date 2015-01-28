using System;
using System.Windows.Input;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.ServerDialogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class NewServerViewModelTests
    {
        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void NewServerViewModel_Constructor_NullShellViewModel_ExceptionThrown()
       
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
          //  new NewServerViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Constructor")]
        public void NewServerViewModel_Constructor_ShouldSetupViewModel()
        {
            //------------Setup for test--------------------------
            var mockNewServerSource = new Mock<IServerSource>();

            var mockCommand = new Mock<ICommand>();

            mockNewServerSource.Setup(a => a.Address).Returns("bob");
            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Public);
            mockNewServerSource.Setup(a => a.Password).Returns("bobthe");
            mockNewServerSource.Setup(a => a.TestMessage).Returns("themessage");
            mockNewServerSource.Setup(a => a.UserName).Returns("hairy");
          //  mockNewServerSource.Setup(a => a.TestCommand).Returns(mockCommand.Verify());


         //   var constructed = new NewServerViewModel(mockNewServerSource.Object);

            //------------Execute Test---------------------------

            //Assert.AreEqual("bob",constructed.Address);
            //Assert.AreEqual(AuthenticationType.Public, constructed.AuthenticationType);
            //Assert.AreEqual("bobthe", constructed.Password);
            //Assert.AreEqual("themessage", constructed.TestMessage);
            //Assert.AreEqual("hairy", constructed.UserName);

            //------------Assert Results-------------------------

        }
        // ReSharper restore InconsistentNaming
    }
}
