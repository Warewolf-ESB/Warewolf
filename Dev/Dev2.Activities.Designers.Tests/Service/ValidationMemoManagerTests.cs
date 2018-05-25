using System;
using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.Service;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Service
{
    [TestClass]
    public class ValidationMemoManagerTests
    {
        [TestMethod]
        public void ValidationMemoManager_UpdateWorstError_WorstErrorIsFirstCriticalError()
        {
            //------------Setup for test-------------------------
            var validationMemoManager = InitValidationMemoManager();
            //------------Execute Test---------------------------
            validationMemoManager.UpdateWorstError();
            //------------Assert Results-------------------------
            Assert.AreEqual(ErrorType.Critical, validationMemoManager.WorstError, "Worst error is not updated to first critical error");
        }

        [TestMethod]
        public void ValidationMemoManager_UpdateLastValidationMemoWithSourceNotFoundError_UpdatesToSourceNotFoundError()
        {
            //------------Setup for test-------------------------
            var validationMemoManager = InitValidationMemoManager();
            //------------Execute Test---------------------------
            validationMemoManager.UpdateLastValidationMemoWithSourceNotFoundError();
            //------------Assert Results-------------------------
            Assert.AreEqual(ErrorType.Critical, validationMemoManager.WorstError, "Worst error is not updated to SourceNotFoundError");
            Assert.AreEqual(1, validationMemoManager.DesignValidationErrors.Count, "Designer validation errors not reset to just SourceNotFoundError");
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Core.ServiceDesignerSourceNotFound, validationMemoManager.DesignValidationErrors[0].Message, "Worst error is not updated to SourceNotFoundError");
            Assert.AreEqual(ErrorType.Critical, validationMemoManager.DesignValidationErrors[0].ErrorType, "Worst error is not updated to SourceNotFoundError");
        }

        static ValidationMemoManager InitValidationMemoManager()
        {
            var validationMemoManager = new ValidationMemoManager(ServiceDesignerViewModelTests.CreateServiceDesignerViewModel(Guid.NewGuid()))
            {
                DesignValidationErrors = new ObservableCollection<IErrorInfo>()
                {
                    new ErrorInfo() { Message = "This is the first error" },
                    new ErrorInfo() { Message = "This is the second error, first critical error", ErrorType = ErrorType.Critical },
                    new ErrorInfo() { Message = "This is the third error" }
                }
            };
            Assert.AreEqual(ErrorType.None, validationMemoManager.WorstError);
            return validationMemoManager;
        }
    }
}
