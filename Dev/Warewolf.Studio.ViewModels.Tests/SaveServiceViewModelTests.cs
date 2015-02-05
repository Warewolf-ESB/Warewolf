using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SaveServiceViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveServiceViewModel_Constructor")]
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
        [TestCategory("SaveServiceViewModel_ValidateName")]
        public void ValidateName_InvalidCharactersName_ShouldReturnErrorMessage()
        {
            //------------Setup for test--------------------------
            var saveServiceViewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
            //------------Execute Test---------------------------
            saveServiceViewModel.Name = "Bad#$Name";
            //------------Assert Results-------------------------
            Assert.AreEqual("'Name' contains invalid characters.",saveServiceViewModel.ErrorMessage);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveServiceViewModel_ValidateName")]
        public void ValidateName_ValidCharactersName_ShouldReturnNoErrorMessage()
        {
            //------------Setup for test--------------------------
            var saveServiceViewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
            //------------Execute Test---------------------------
            saveServiceViewModel.Name = "Good Name";
            //------------Assert Results-------------------------
            Assert.AreEqual("",saveServiceViewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveServiceViewModel_ValidateName")]
        public void ValidateName_NullName_ShouldReturnErrorMessage()
        {
            //------------Setup for test--------------------------
            var saveServiceViewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
            //------------Execute Test---------------------------
            saveServiceViewModel.Name = null;
            //------------Assert Results-------------------------
            Assert.AreEqual("'Name' cannot be empty.", saveServiceViewModel.ErrorMessage);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveServiceViewModel_ValidateName")]
        public void ValidateName_EmptyName_ShouldReturnErrorMessage()
        {
            //------------Setup for test--------------------------
            var saveServiceViewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
            //------------Execute Test---------------------------
            saveServiceViewModel.Name = "";
            //------------Assert Results-------------------------
            Assert.AreEqual("'Name' cannot be empty.", saveServiceViewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveServiceViewModel_Name")]
        public void Name_Set_ShouldFirePropertyChangedEvent()
        {
            //------------Setup for test--------------------------
            var propertyChangeFired = false;
            var saveServiceViewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
            saveServiceViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Name")
                {
                    propertyChangeFired = true;
                }
            };
            //------------Execute Test---------------------------
            saveServiceViewModel.Name = "Test";
            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChangeFired);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveServiceViewModel_ErrorMessage")]
        public void ErrorMessage_Set_ShouldFirePropertyChangedEvent()
        {
            //------------Setup for test--------------------------
            var propertyChangeFired = false;
            var saveServiceViewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
            saveServiceViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ErrorMessage")
                {
                    propertyChangeFired = true;
                }
            };
            //------------Execute Test---------------------------
            saveServiceViewModel.ErrorMessage = "Test";
            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChangeFired);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveServiceViewModel_Constructor")]
        public void Constructor_ShouldSetupOkCommand()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------

        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveServiceViewModel_OkCommand")]
        public void OkCommand_CanExecute_HasErrorMessage_False()
        {
            //------------Setup for test--------------------------
            var saveServiceViewModel = new RequestServiceNameViewModel(new Mock<IEnvironmentViewModel>().Object);
            //------------Execute Test---------------------------
            saveServiceViewModel.Name = "Bad**Name";
            //------------Assert Results-------------------------
            var canExecute = saveServiceViewModel.OkCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
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
            }
        }

        public ICommand OkCommand { get; set; }
    }
}
