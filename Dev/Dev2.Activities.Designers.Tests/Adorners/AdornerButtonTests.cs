using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Studio.Core.ViewModels.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Adorners
{
    [TestClass]
    public class AdornerButtonTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Construct_CommandObjectIsInstantiatedAndIsValidIsSetToTrue()
        {
            var button = new AdornerButton();
            Assert.IsNotNull(button.Command);
            Assert.IsTrue(button.IsValid);
            Assert.IsFalse(button.IsValidatedBefore);
            Assert.IsFalse(button.IsClosedAfter);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforeIsSetToFalse_ValidationErrorsIsNotCalled()
        {
            VerifyExecution(isValidatedBefore: false, isClosedAfter: false, valiationErrorsHitCount: 0, hideContentHitCount: 0, isValid: true, validationErrorCount: 0, customCommand: null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsClosedAfterIsSetToFalse_HideContentIsNotCalled()
        {
            VerifyExecution(isValidatedBefore: false, isClosedAfter: false, valiationErrorsHitCount: 0, hideContentHitCount: 0, isValid: true, validationErrorCount: 0, customCommand: null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsClosedAfterIsSetToTrue_HideContentIsCalledOnce()
        {
            VerifyExecution(isValidatedBefore: false, isClosedAfter: true, valiationErrorsHitCount: 0, hideContentHitCount: 1, isValid: true, validationErrorCount: 0, customCommand: null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforeAndIsClosedAfterAreSetToTrueAndThereAreNoValidationErrors_ValidationErrorsAndHideContentAreCalledOnce()
        {
            VerifyExecution(isValidatedBefore: true, isClosedAfter: true, valiationErrorsHitCount: 1, hideContentHitCount: 1, isValid: true, validationErrorCount: 0, customCommand: null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforIsSetToTrueAndIsClosedAfterAreSetToFalseAndThereAreNoValidationErrors_ValidationErrorsIsCalledOnceAndHideContentIsNotCalled()
        {
            VerifyExecution(isValidatedBefore: true, isClosedAfter: false, valiationErrorsHitCount: 1, hideContentHitCount: 0, isValid: true, validationErrorCount: 0, customCommand: null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforeIsToTrueAndThereAreNoValidationErrors_CustomCommandIsExecuted()
        {
            var customCommandExecuted = false;
            VerifyExecution(isValidatedBefore: true, isClosedAfter: false, valiationErrorsHitCount: 1, hideContentHitCount: 0, isValid: true, validationErrorCount: 0,
                customCommand: new RelayCommand(o => { customCommandExecuted = true; }));

            Assert.IsTrue(customCommandExecuted);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforeIsToTrueAndThereAreValidationErrors_CustomCommandIsNotExecuted()
        {
            var customCommandExecuted = false;
            VerifyExecution(isValidatedBefore: true, isClosedAfter: false, valiationErrorsHitCount: 1, hideContentHitCount: 0, isValid: false, validationErrorCount: 1,
                customCommand: new RelayCommand(o => { customCommandExecuted = true; }));

            Assert.IsFalse(customCommandExecuted);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforeAndIsClosedAfterAreSetToFalse_CustomCommandIsExecuted()
        {
            var customCommandExecuted = false;
            VerifyExecution(isValidatedBefore: false, isClosedAfter: false, valiationErrorsHitCount: 0, hideContentHitCount: 0, isValid: true, validationErrorCount: 0,
                customCommand: new RelayCommand(o => { customCommandExecuted = true; }));

            Assert.IsTrue(customCommandExecuted);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsClosedAfterIsSetToTrueAndCustomCommandIsNotSet_HideContentIsCalled()
        {
            VerifyExecution(isValidatedBefore: false, isClosedAfter: true, valiationErrorsHitCount: 0, hideContentHitCount: 1, isValid: true, validationErrorCount: 0, customCommand: null);
        }


        static void VerifyExecution(bool isValidatedBefore, bool isClosedAfter, int valiationErrorsHitCount, int hideContentHitCount, bool isValid, int validationErrorCount, ICommand customCommand)
        {
            var validatonErrors = new List<IErrorInfo>();
            for(var i = 0; i < validationErrorCount; i++)
            {
                validatonErrors.Add(new ErrorInfo { Message = "Error " + i });
            }

            var activityViewModelBase = new Mock<IActivityViewModelBase>();
            activityViewModelBase.Setup(m => m.HideContent()).Verifiable();

            var mockDataContext = new Mock<ITestDataContext>();
            mockDataContext.Setup(m => m.ValidationErrors()).Returns(() => validatonErrors).Verifiable();
            mockDataContext.Setup(m => m.ActivityViewModelBase).Returns(activityViewModelBase.Object);

            var button = new AdornerButton
            {
                DataContext = mockDataContext.Object,
                IsValidatedBefore = isValidatedBefore,
                IsClosedAfter = isClosedAfter,
                CustomCommand = customCommand
            };

            button.Command.Execute(null);

            mockDataContext.Verify(m => m.ValidationErrors(), Times.Exactly(valiationErrorsHitCount));
            activityViewModelBase.Verify(m => m.HideContent(), Times.Exactly(hideContentHitCount));
            Assert.AreEqual(isValid, button.IsValid);
        }

    }

    public interface ITestDataContext : IHasActivityViewModelBase, IValidator
    {

    }
}