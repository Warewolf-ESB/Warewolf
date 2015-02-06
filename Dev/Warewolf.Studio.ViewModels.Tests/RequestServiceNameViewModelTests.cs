using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
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
            new RequestServiceNameViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_ValidateName")]
        public void ValidateName_InvalidCharactersName_ShouldReturnErrorMessage()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
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
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
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
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
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
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
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
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
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
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
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
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.OkCommand);

        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RequestServiceNameViewModel_OkCommand")]
        public void OkCommand_CanExecute_HasErrorMessage_False()
        {
            //------------Setup for test--------------------------
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
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
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
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
            var viewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
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


    public class RequestServiceNameViewModel : BindableBase, IRequestServiceNameViewModel
    {
        private string _name;
        private string _errorMessage;
        private ResourceName _resourceName;

        public RequestServiceNameViewModel(IEnvironmentViewModel environmentViewModel)
        {
            if (environmentViewModel == null)
            {
                throw new ArgumentNullException("environmentViewModel");
            }
            OkCommand = new DelegateCommand(() => _resourceName=new ResourceName("",Name),() => String.IsNullOrEmpty(ErrorMessage));            
        }

        private void RaiseCanExecuteChanged()
        {
            var command = OkCommand as DelegateCommand;
            if (command != null)
            {
                command.RaiseCanExecuteChanged();
            }
        }

        public MessageBoxResult ShowSaveDialog()
        {
            return MessageBoxResult.None;
        }

        public ResourceName ResourceName
        {
            get { return _resourceName; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(() => Name);
                if (String.IsNullOrEmpty(Name))
                {
                    ErrorMessage = "'Name' cannot be empty.";
                }
                else if (NameHasInvalidCharacters(Name))
                {
                    ErrorMessage = "'Name' contains invalid characters.";
                }
                else
                {
                    ErrorMessage = "";
                }
            }
        }

        private bool NameHasInvalidCharacters(string name)
        {
            return Regex.IsMatch(name, @"[^a-zA-Z0-9._\s-]");
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(() => ErrorMessage);
                RaiseCanExecuteChanged();
            }
        }

        public ICommand OkCommand { get; set; }
    }
}
