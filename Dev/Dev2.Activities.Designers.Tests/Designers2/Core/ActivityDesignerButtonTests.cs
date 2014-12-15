
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Providers.Errors;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Designers2.Core
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class ActivityDesignerButtonTests
    {
        #region Tests
        [TestMethod]
        [TestCategory("ActivityDesignerButton_Construct")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerButton_Construct_CommandObjectIsInstantiatedAndIsValidIsSetToTrue()
        {
            var button = new ActivityDesignerButton();
            Assert.IsNotNull(button.Command);
            Assert.IsTrue(button.IsValid);
            Assert.IsFalse(button.IsValidatedBefore);
            Assert.IsFalse(button.IsClosedAfter);
        }

        [TestMethod]
        [TestCategory("ActivityDesignerButton_Execute")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerButton_Execute_IsValidatedBeforeIsSetToFalse_ValidationErrorsIsNotCalled()
        {
            VerifyExecution(false, false, true, 0, null);
        }

        [TestMethod]
        [TestCategory("ActivityDesignerButton_Execute")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerButton_Execute_IsClosedAfterIsSetToFalse_HideContentIsNotCalled()
        {
            VerifyExecution(false, false, true, 0, null);
        }

        [TestMethod]
        [TestCategory("ActivityDesignerButton_Execute")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerButton_Execute_IsClosedAfterIsSetToTrue_HideContentIsCalledOnce()
        {
            VerifyExecution(false, true, true, 0, null);
        }

        [TestMethod]
        [TestCategory("ActivityDesignerButton_Execute")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerButton_Execute_IsValidatedBeforeAndIsClosedAfterAreSetToTrueAndThereAreNoValidationErrors_ValidationErrorsAndHideContentAreCalledOnce()
        {
            VerifyExecution(true, true, true, 0, null);
        }

        [TestMethod]
        [TestCategory("ActivityDesignerButton_Execute")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerButton_Execute_IsValidatedBeforIsSetToTrueAndIsClosedAfterAreSetToFalseAndThereAreNoValidationErrors_ValidationErrorsIsCalledOnceAndHideContentIsNotCalled()
        {
            VerifyExecution(true, false, true, 0, null);
        }

        [TestMethod]
        [TestCategory("ActivityDesignerButton_Execute")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerButton_Execute_IsValidatedBeforeIsToTrueAndThereAreNoValidationErrors_CustomCommandIsExecuted()
        {
            var customCommandExecuted = false;
            VerifyExecution(true, false, true, 0, new RelayCommand(o => { customCommandExecuted = true; }));

            Assert.IsTrue(customCommandExecuted);
        }

        [TestMethod]
        [TestCategory("ActivityDesignerButton_Execute")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerButton_Execute_IsValidatedBeforeIsToTrueAndThereAreValidationErrors_CustomCommandIsNotExecuted()
        {
            var customCommandExecuted = false;
            VerifyExecution(true, false, false, 1, new RelayCommand(o => { customCommandExecuted = true; }));

            Assert.IsFalse(customCommandExecuted);
        }

        [TestMethod]
        [TestCategory("ActivityDesignerButton_Execute")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerButton_Execute_IsValidatedBeforeAndIsClosedAfterAreSetToFalse_CustomCommandIsExecuted()
        {
            var customCommandExecuted = false;
            VerifyExecution(false, false, true, 0, new RelayCommand(o => { customCommandExecuted = true; }));

            Assert.IsTrue(customCommandExecuted);
        }

        [TestMethod]
        [TestCategory("ActivityDesignerButton_Execute")]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityDesignerButton_Execute_IsClosedAfterIsSetToTrueAndCustomCommandIsNotSet_HideContentIsCalled()
        {
            VerifyExecution(false, true, true, 0, null);
        }

        #endregion

        #region Helper
        static void VerifyExecution(bool isValidatedBefore, bool isClosedAfter, bool isValid, int validationErrorCount, ICommand customCommand)
        {
            var validatonErrors = new List<IErrorInfo>();
            for(var i = 0; i < validationErrorCount; i++)
            {
                validatonErrors.Add(new ErrorInfo { Message = "Error " + i });
            }

            var mockDataContext = new Mock<ITestDataContext>();
            mockDataContext.SetupGet(v => v.IsValid).Returns(isValid);

            var button = new ActivityDesignerButton
            {
                DataContext = mockDataContext.Object,
                IsValidatedBefore = isValidatedBefore,
                IsClosedAfter = isClosedAfter,
                CustomCommand = customCommand
            };

            button.Command.Execute(null);
            Assert.AreEqual(isValid, button.IsValid);
        }
        #endregion
    }

    #region Fakes
    public interface ITestDataContext : IValidator, IErrorsSource
    {
    }
    #endregion
}
