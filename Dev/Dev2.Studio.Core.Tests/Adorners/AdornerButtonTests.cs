using System.Collections.Generic;
using Dev2.Activities.Adorners;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Adorners
{
    [TestClass]
    public class AdornerButtonTests
    {
        Mock<ITestDataContext> GetMockDataContext()
        {
            var mockDataContext = new Mock<ITestDataContext>();
            mockDataContext.Setup(m => m.ValidationErrors()).Verifiable();
            mockDataContext.Setup(m => m.HideContent()).Verifiable();
            return mockDataContext;
        }

        AdornerButton GetButtonInstance(Mock<ITestDataContext> mockDataContext)
        {
            var button = new AdornerButton { DataContext = mockDataContext.Object };
            return button;
        }

        AdornerButton GetButtonInstance()
        {
            return new AdornerButton();
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Construct_CommandObjectIsInstantiatedAndIsValidIsSetToTrue()
        {
            var button = GetButtonInstance();
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
            var mockDataContext = GetMockDataContext();
            var button = GetButtonInstance(mockDataContext);
            button.IsValidatedBefore = false;

            button.Command.Execute(null);

            mockDataContext.Verify(m => m.ValidationErrors(), Times.Never());
            Assert.IsTrue(button.IsValid);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsClosedAfterIsSetToFalse_HideContentIsNotCalled()
        {
            var mockDataContext = GetMockDataContext();
            var button = GetButtonInstance(mockDataContext);
            button.IsClosedAfter = false;

            button.Command.Execute(null);

            mockDataContext.Verify(m => m.HideContent(), Times.Never());
            Assert.IsTrue(button.IsValid);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsClosedAfterIsSetToTrue_HideContentIsCalledOnce()
        {
            var mockDataContext = GetMockDataContext();
            var button = GetButtonInstance(mockDataContext);
            button.IsValidatedBefore = false;
            button.IsClosedAfter = true;

            button.Command.Execute(null);

            mockDataContext.Verify(m => m.ValidationErrors(), Times.Never());
            mockDataContext.Verify(m => m.HideContent(), Times.Once());
            Assert.IsTrue(button.IsValid);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforeAndIsClosedAfterAreSetToTrueAndThereAreNoValidationErrors_ValidationErrorsAndHideContentAreCalledOnce()
        {
            var mockDataContext = GetMockDataContext();
            mockDataContext.Setup(m => m.ValidationErrors())
                           .Returns(new IErrorInfo[0])
                           .Verifiable();

            var button = GetButtonInstance(mockDataContext);
            button.IsValidatedBefore = true;
            button.IsClosedAfter = true;

            button.Command.Execute(null);

            mockDataContext.Verify(m => m.ValidationErrors(), Times.Once());
            mockDataContext.Verify(m => m.HideContent(), Times.Once());
            Assert.IsTrue(button.IsValid);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforIsSetToTrueAndIsClosedAfterAreSetToFalseAndThereAreNoValidationErrors_ValidationErrorsIsCalledOnceAndHideContentIsNotCalled()
        {
            var mockDataContext = GetMockDataContext();
            mockDataContext.Setup(m => m.ValidationErrors())
                           .Returns(new IErrorInfo[0])
                           .Verifiable();

            var button = GetButtonInstance(mockDataContext);
            button.IsValidatedBefore = true;
            button.IsClosedAfter = false;

            button.Command.Execute(null);

            mockDataContext.Verify(m => m.ValidationErrors(), Times.Once());
            mockDataContext.Verify(m => m.HideContent(), Times.Never());
            Assert.IsTrue(button.IsValid);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforeIsToTrueAndThereAreNoValidationErrors_CustomCommandIsExecuted()
        {
            var mockDataContext = GetMockDataContext();
            mockDataContext.Setup(m => m.ValidationErrors())
                           .Returns(new IErrorInfo[0])
                           .Verifiable();

            var button = GetButtonInstance(mockDataContext);
            var customCommandExecuted = false;
            button.CustomCommand = new RelayCommand(o => { customCommandExecuted = true; });
            button.IsValidatedBefore = true;
            button.IsClosedAfter = false;

            button.Command.Execute(null);

            Assert.IsTrue(customCommandExecuted);
            mockDataContext.Verify(m => m.ValidationErrors(), Times.Once());
            mockDataContext.Verify(m => m.HideContent(), Times.Never());
            Assert.IsTrue(button.IsValid);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforeIsToTrueAndThereAreValidationErrors_CustomCommandIsNotExecuted()
        {
            var mockDataContext = GetMockDataContext();
            mockDataContext.Setup(m => m.ValidationErrors())
                           .Returns(new List<IErrorInfo> { new ErrorInfo { Message = "More testing is needed" } })
                           .Verifiable();

            var button = GetButtonInstance(mockDataContext);
            var customCommandExecuted = false;
            button.CustomCommand = new RelayCommand(o => { customCommandExecuted = true; });
            button.IsValidatedBefore = true;
            button.IsClosedAfter = false;

            button.Command.Execute(null);

            Assert.IsFalse(customCommandExecuted);
            mockDataContext.Verify(m => m.ValidationErrors(), Times.Once());
            mockDataContext.Verify(m => m.HideContent(), Times.Never());
            Assert.IsFalse(button.IsValid);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsValidatedBeforeAndIsClosedAfterAreSetToFalse_CustomCommandIsExecuted()
        {
            var mockDataContext = GetMockDataContext();

            var button = GetButtonInstance(mockDataContext);
            var customCommandExecuted = false;
            button.CustomCommand = new RelayCommand(o => { customCommandExecuted = true; });
            button.IsValidatedBefore = false;
            button.IsClosedAfter = false;

            button.Command.Execute(null);

            Assert.IsTrue(customCommandExecuted);
            mockDataContext.Verify(m => m.ValidationErrors(), Times.Never());
            mockDataContext.Verify(m => m.HideContent(), Times.Never());
            Assert.IsTrue(button.IsValid);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Tshepo Ntlhokoa")]
        public void AdornerButton_Execute_IsClosedAfterIsSetToTrueAndCustomCommandIsNotSet_HideContentIsCalled()
        {
            var mockDataContext = GetMockDataContext();
            var button = GetButtonInstance(mockDataContext);
            button.IsValidatedBefore = false;
            button.IsClosedAfter = true;

            button.Command.Execute(null);

            mockDataContext.Verify(m => m.ValidationErrors(), Times.Never());
            mockDataContext.Verify(m => m.HideContent(), Times.Once());
            Assert.IsTrue(button.IsValid);
        }
    }

    public interface ITestDataContext : IValidator, IOverlayManager
    {
    }
}