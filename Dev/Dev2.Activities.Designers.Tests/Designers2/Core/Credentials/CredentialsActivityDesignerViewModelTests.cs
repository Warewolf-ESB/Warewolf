
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Credentials
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class CredentialsActivityDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CredentialsActivityDesignerViewModel_Constructor")]
        public void CredentialsActivityDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel();

            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(0, viewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword")]
        public void CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword_UserNameAndPasswordBlank_NoErrors()
        {
            Verify_ValidateUserNameAndPassword("", "", true, null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword")]
        public void CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword_UserNameIsNotBlankAndPasswordIsBlank_HasErrors()
        {
            Verify_ValidateUserNameAndPassword("aaaa", "", true, "Password cannot be empty or only white space");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword")]
        public void CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword_UserNameIsBlankAndPasswordIsNotBlank_HasErrors()
        {
            Verify_ValidateUserNameAndPassword("", "xxx", false, "Username cannot be empty or only white space");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword")]
        public void CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword_UserNameAndPasswordAreNotBlank_NoErrors()
        {
            Verify_ValidateUserNameAndPassword("aaa", "xxx", false, null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword")]
        public void CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword_UserNameIsInvalidExpression_HasErrors()
        {
            Verify_ValidateUserNameAndPassword("a]]", "", false, "Username - Invalid expression: opening and closing brackets don't match.");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword")]
        public void CredentialsActivityDesignerViewModel_ValidateUserNameAndPassword_PasswordIsInvalidExpression_HasErrors()
        {
            Verify_ValidateUserNameAndPassword("afaf", "a]]", true, "Password - Invalid expression: opening and closing brackets don't match.");
        }

        // ReSharper disable UnusedParameter.Local
        static void Verify_ValidateUserNameAndPassword(string userName, string password, bool isPasswordError, string expectedMessageFormat)
        // ReSharper restore UnusedParameter.Local
        {
            //------------Setup for test-------------------------
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><a></a></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            const string LabelText = "Password";

            var viewModel = CreateViewModel(userName, password);

            //------------Execute Test---------------------------
            viewModel.TestValidateUserNameAndPassword();

            if(string.IsNullOrEmpty(expectedMessageFormat))
            {
                Assert.IsNull(viewModel.Errors);
            }
            else
            {
                Assert.IsNotNull(viewModel.Errors);
                Assert.AreEqual(1, viewModel.Errors.Count);

                var error = viewModel.Errors[0];
                Assert.AreEqual(string.Format(expectedMessageFormat, LabelText), error.Message);

                error.Do();
                Assert.IsTrue(isPasswordError ? viewModel.IsPasswordFocused : viewModel.IsUserNameFocused);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CredentialsActivityDesignerViewModel_UpdateErrors")]
        public void CredentialsActivityDesignerViewModel_UpdateErrors_ErrorsPropertyIsNotNull_ErrorsAreAdded()
        {
            //------------Setup for test--------------------------
            var errors = new List<IActionableErrorInfo>
            {
                new ActionableErrorInfo(() => { }) { ErrorType = ErrorType.Critical, Message = "Error 2" }, 
                new ActionableErrorInfo(() => { }) { ErrorType = ErrorType.Critical, Message = "Error 3" }
            };

            var viewModel = CreateViewModel();
            viewModel.Errors = new List<IActionableErrorInfo>
            {
                new ActionableErrorInfo(() => { }) { ErrorType = ErrorType.Critical, Message = "Error 1" }, 
            };

            Assert.AreEqual(1, viewModel.Errors.Count);

            //------------Execute Test---------------------------
            viewModel.TestUpdateErrors(errors);

            //------------Assert Results-------------------------
            Assert.AreEqual(3, viewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CredentialsActivityDesignerViewModel_UpdateErrors")]
        public void CredentialsActivityDesignerViewModel_UpdateErrors_ErrorsParameterIsEmptyList_ErrorsAreNotAdded()
        {
            //------------Setup for test--------------------------
            var errors = new List<IActionableErrorInfo>();

            var viewModel = CreateViewModel();
            viewModel.Errors = new List<IActionableErrorInfo>
            {
                new ActionableErrorInfo(() => { }) { ErrorType = ErrorType.Critical, Message = "Error 1" }, 
            };

            Assert.AreEqual(1, viewModel.Errors.Count);

            //------------Execute Test---------------------------
            viewModel.TestUpdateErrors(errors);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CredentialsActivityDesignerViewModel_UpdateErrors")]
        public void CredentialsActivityDesignerViewModel_UpdateErrors_ErrorsParameterIsNull_ErrorsAreNotAdded()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateViewModel();
            viewModel.Errors = new List<IActionableErrorInfo>
            {
                new ActionableErrorInfo(() => { }) { ErrorType = ErrorType.Critical, Message = "Error 1" }, 
            };

            Assert.AreEqual(1, viewModel.Errors.Count);

            //------------Execute Test---------------------------
            viewModel.TestUpdateErrors(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.Errors.Count);
        }

        static TestCredentialsActivityDesignerViewModel CreateViewModel(string userName = "", string password = "")
        {
            var viewModel = new TestCredentialsActivityDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFileRead { Username = userName, Password = password }));
            return viewModel;
        }
    }
}
