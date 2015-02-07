using System;
using System.Windows.Navigation;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class RequestServiceNameViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullEnvironment_ArgumentNullExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new RequestServiceNameViewModel(null,null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_InvalidCharactersName_ShouldReturnErrorMessage()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, null);
            //------------Execute Test---------------------------
            viewModel.Name = "Bad#$Name";
            //------------Assert Results-------------------------
            Assert.AreEqual("'Name' contains invalid characters.", viewModel.ErrorMessage);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_ValidCharactersName_ShouldReturnNoErrorMessage()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, null);
            //------------Execute Test---------------------------
            viewModel.Name = "Good Name";
            //------------Assert Results-------------------------
            Assert.AreEqual("", viewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_NullName_ShouldReturnErrorMessage()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, null);
            //------------Execute Test---------------------------
            viewModel.Name = null;
            //------------Assert Results-------------------------
            Assert.AreEqual("'Name' cannot be empty.", viewModel.ErrorMessage);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_EmptyName_ShouldReturnErrorMessage()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, null);
            //------------Execute Test---------------------------
            viewModel.Name = "";
            //------------Assert Results-------------------------
            Assert.AreEqual("'Name' cannot be empty.", viewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_Name")]
        public void Name_Set_ShouldFirePropertyChangedEvent()
        {
            //------------Setup for test--------------------------
            var propertyChangeFired = false;
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, null);
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Name")
                {
                    propertyChangeFired = true;
                }
            };
            //------------Execute Test---------------------------
            viewModel.Name = "Test";
            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChangeFired);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ErrorMessage")]
        public void ErrorMessage_Set_ShouldFirePropertyChangedEvent()
        {
            //------------Setup for test--------------------------
            var propertyChangeFired = false;
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, null);
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ErrorMessage")
                {
                    propertyChangeFired = true;
                }
            };
            //------------Execute Test---------------------------
            viewModel.ErrorMessage = "Test";
            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChangeFired);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_Constructor")]
        public void Constructor_ShouldSetupOkCommand()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.OkCommand);

        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_OkCommand")]
        public void OkCommand_CanExecute_HasErrorMessage_False()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, null);
            //------------Execute Test---------------------------
            viewModel.Name = "Bad**Name";
            //------------Assert Results-------------------------
            var canExecute = viewModel.OkCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_OkCommand")]
        public void OkCommand_CanExecute_NoErrorMessage_True()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, null);
            //------------Execute Test---------------------------
            viewModel.Name = "Resource Name";
            //------------Assert Results-------------------------
            var canExecute = viewModel.OkCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ErrorMessage")]
        public void ErrorMessage_Set_ShouldRaiseCanExecuteChangeForOkCommand()
        {
            //------------Setup for test--------------------------
            var canExecuteChangedFired = false;
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object, null);
            viewModel.OkCommand.CanExecuteChanged += (sender, args) =>
            {
                canExecuteChangedFired = true;
            };
            //------------Assert Preconditions-------------------
            Assert.IsFalse(canExecuteChangedFired);
            //------------Execute Test---------------------------
            viewModel.ErrorMessage = "Some error message";
            //------------Assert Results-------------------------
            Assert.IsTrue(canExecuteChangedFired);

        }
    }
}
