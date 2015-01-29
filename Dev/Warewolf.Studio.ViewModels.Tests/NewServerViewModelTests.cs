using System;
using System.Windows.Input;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.ServerDialogue;
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
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void NewServerViewModel_Constructor_NullShellViewModel_ExceptionThrown()
       
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var constructed = new NewServerViewModel();
            //------------Assert Results-------------------------
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IServerSource>().Object, new Mock<IServerConnectionTest>().Object, new Mock<IStudioUpdateManager>().Object },typeof(NewServerViewModel));
          

        }

        [TestMethod]
        [Owner("Robin Van Den Heever")]
        [TestCategory("NewServerViewModel_Ctor")]
        public void NewServerViewModel_Ctor_NullValues_ExpectErrors()
        {
            //------------Setup for test--------------------------
            var newServerViewModel = new NewServerViewModel();
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
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
        public void NewServerViewModel_Interactions_AuthenticationTypeChangesTextboxVisibility() { }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_Interactions_AdressEnablesSaveCommand() { }



        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_Interactions_AdressTestSuccessEnablesSaveCommand() { }



        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_SaveCommand_CallsUpdateManager() { }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_TestCommand_CallsUpdateManager() { }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_TestCommand_SetsConnectionMessage() { }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("NewServerViewModel_Interactions")]
        public void NewServerViewModel_TestCommand_EnablesSave() { }



    }
}
