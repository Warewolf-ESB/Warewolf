using System;
using System.Windows.Input;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class NewServerViewModelTests
    {
        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Constructor")]
        // ReSharper disable InconsistentNaming
        public void NewServerViewModel_Constructor_NullShellViewModel_ExceptionThrown()
        {
            //------------Assert Results-------------------------
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IServerSource>().Object, new Mock<IServerConnectionTest>().Object, new Mock<IStudioUpdateManager>().Object }, typeof(NewServerViewModel));
        }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Constructor")]
        public void NewServerViewModel_Constructor_ShouldSetupViewModel()
        {
            //------------Setup for test--------------------------
            var mockNewServerSource = new Mock<IServerSource>();
            var mockServerConnectionTest = new Mock<IServerConnectionTest>();
            var mockCommand = new Mock<ICommand>();

            mockNewServerSource.Setup(a => a.Address).Returns("bob");
            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Public);
            mockNewServerSource.Setup(a => a.Password).Returns("bobthe");
            mockNewServerSource.Setup(a => a.UserName).Returns("hairy");

            //------------Execute Test---------------------------
            var constructed = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, new Mock<IStudioUpdateManager>().Object);

            //------------Assert Results-------------------------

            Assert.AreEqual("bob", constructed.Address);
            Assert.AreEqual(AuthenticationType.Public, constructed.AuthenticationType);
            Assert.AreEqual("bobthe", constructed.Password);
            Assert.AreEqual("hairy", constructed.UserName);


        }

        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_Interactions_AuthenticationTypeChangesTextboxVisibility()
        {
            //------------Setup for test--------------------------
            var mockNewServerSource = new Mock<IServerSource>();
            var mockServerConnectionTest = new Mock<IServerConnectionTest>();
            var mockCommand = new Mock<ICommand>();

            mockNewServerSource.Setup(a => a.Address).Returns("bob");
           mockNewServerSource.Setup(a => a.Password).Returns("bobthe");
            mockNewServerSource.Setup(a => a.UserName).Returns("hairy");

            //------------Execute Test---------------------------
            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Public);
            var constructed = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, new Mock<IStudioUpdateManager>().Object);
            Assert.AreEqual(false, constructed.IsUserNameVisible);
            Assert.AreEqual(false, constructed.IsPasswordVisible);

            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.User);
            var constructed2 = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, new Mock<IStudioUpdateManager>().Object);
            Assert.AreEqual(true, constructed2.IsUserNameVisible);
            Assert.AreEqual(true, constructed2.IsPasswordVisible);

            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Anonymous);
            var constructed3 = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, new Mock<IStudioUpdateManager>().Object);
            Assert.AreEqual(false, constructed3.IsUserNameVisible);
            Assert.AreEqual(false, constructed3.IsPasswordVisible);

            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Windows);
            var constructed4 = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, new Mock<IStudioUpdateManager>().Object);
            Assert.AreEqual(false, constructed4.IsUserNameVisible);
            Assert.AreEqual(false, constructed4.IsPasswordVisible);

            //------------Assert Results-------------------------

        }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_Interactions_AdressEnablesSaveCommand()
        {
            //------------Setup for test--------------------------
            var mockNewServerSource = new Mock<IServerSource>();
            var mockServerConnectionTest = new Mock<IServerConnectionTest>();
            var mockCommand = new Mock<ICommand>();

            
            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Public);
            mockNewServerSource.Setup(a => a.Password).Returns("bobthe");
            mockNewServerSource.Setup(a => a.UserName).Returns("hairy");

            //------------Execute Test---------------------------
            var constructed = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, new Mock<IStudioUpdateManager>().Object);

            Assert.AreEqual("The server address cannot be empty", constructed.Validate);

            constructed.TestPassed = false;
            constructed.Address = "hello";

            Assert.AreEqual("The server connection must be tested before saving", constructed.Validate);

            constructed.TestPassed = true;
            constructed.Address = "hello";

            Assert.AreEqual(String.Empty, constructed.Validate);

            //------------Assert Results-------------------------

        }



        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_Interactions_AdressTestSuccessEnablesSaveCommand()
        {
            //------------Setup for test--------------------------
            var mockNewServerSource = new Mock<IServerSource>();
            var mockServerConnectionTest = new Mock<IServerConnectionTest>();
            var mockCommand = new Mock<ICommand>();

            mockNewServerSource.Setup(a => a.Address).Returns("bob");
            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Public);
            mockNewServerSource.Setup(a => a.Password).Returns("bobthe");
            mockNewServerSource.Setup(a => a.UserName).Returns("hairy");

            //------------Execute Test---------------------------
            var constructed = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, new Mock<IStudioUpdateManager>().Object);
            constructed.TestPassed = true;
            constructed.Address = "hello";

            Assert.AreNotEqual(true, constructed.IsOkEnabled);
            var validate = constructed.Validate;
            Assert.AreEqual(true, constructed.IsOkEnabled);


            //------------Assert Results-------------------------

        }




        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_SaveCommand_CallsUpdateManager()
        {
            //------------Setup for test--------------------------
            var mockNewServerSource = new Mock<IServerSource>();
            var mockServerConnectionTest = new Mock<IServerConnectionTest>();
            var mockCommand = new Mock<ICommand>();

            mockNewServerSource.Setup(a => a.Address).Returns("bob");
            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Public);
            mockNewServerSource.Setup(a => a.Password).Returns("bobthe");
            mockNewServerSource.Setup(a => a.UserName).Returns("hairy");

            //------------Execute Test---------------------------
            var constructed = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, new Mock<IStudioUpdateManager>().Object);
            constructed.TestPassed = true;
            constructed.Address = "hello";
            var validate = constructed.Validate;

            //constructed.OkCommand.Execute();


            //------------Assert Results-------------------------

        }



        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_TestCommand_CallsUpdateManager()
        {
            //------------Setup for test--------------------------

            var inner = new Mock<INewServerDialogue>();
           

            var mockNewServerSource = new Mock<IServerSource>();
            var mockServerConnectionTest = new Mock<IServerConnectionTest>();
            var mockStudioUpdateManager = new Mock<IStudioUpdateManager>();
            mockStudioUpdateManager.Setup(a => a.TestConnection(It.IsAny<ServerSource>())).Returns("bob");
            var mockCommand = new Mock<ICommand>();

            mockNewServerSource.Setup(a => a.Address).Returns("http://localhost:3142");
            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Public);
            mockNewServerSource.Setup(a => a.Password).Returns("bobthe");
            mockNewServerSource.Setup(a => a.UserName).Returns("hairy");


            mockStudioUpdateManager.Setup(a => a.TestConnection(mockNewServerSource.Object)).Returns("Success");

            //------------Execute Test---------------------------
            var constructed = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, mockStudioUpdateManager.Object);

            //------------Assert Results-------------------------
            //constructed.TestPassed = true;
            //var validate = constructed.Validate;
            constructed.TestCommand.Execute(null);





        }



        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_TestCommand_SetsConnectionMessage()
        {
            //------------Setup for test--------------------------
            var mockNewServerSource = new Mock<IServerSource>();
            var mockServerConnectionTest = new Mock<IServerConnectionTest>();
            var mockCommand = new Mock<ICommand>();

            mockNewServerSource.Setup(a => a.Address).Returns("bob");
            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Public);
            mockNewServerSource.Setup(a => a.Password).Returns("bobthe");
            mockNewServerSource.Setup(a => a.UserName).Returns("hairy");

            //------------Execute Test---------------------------
            var constructed = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, new Mock<IStudioUpdateManager>().Object);

            //------------Assert Results-------------------------

        }



        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_TestCommand_EnablesSave()
        {
            //------------Setup for test--------------------------
            var mockNewServerSource = new Mock<IServerSource>();
            var mockServerConnectionTest = new Mock<IServerConnectionTest>();
            var mockCommand = new Mock<ICommand>();

            mockNewServerSource.Setup(a => a.Address).Returns("bob");
            mockNewServerSource.Setup(a => a.AuthenticationType).Returns(AuthenticationType.Public);
            mockNewServerSource.Setup(a => a.Password).Returns("bobthe");
            mockNewServerSource.Setup(a => a.UserName).Returns("hairy");

            //------------Execute Test---------------------------
            var constructed = new NewServerViewModel(mockNewServerSource.Object, mockServerConnectionTest.Object, new Mock<IStudioUpdateManager>().Object);

            //------------Assert Results-------------------------

        }




    }
}
