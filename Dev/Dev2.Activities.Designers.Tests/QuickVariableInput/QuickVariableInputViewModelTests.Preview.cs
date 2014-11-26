
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
using Dev2.Activities.Designers2.Core.QuickVariableInput;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.QuickVariableInput
{
    public partial class QuickVariableInputViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_PreviewRequested")]
        public void QuickVariableInputViewModel_PreviewRequested_ClearsPreviewOutputAndChecksValidationErrors()
        {
            //------------Setup for test--------------------------
            var qviViewModel = new QuickVariableInputViewModelMock();
            qviViewModel.PreviewViewModel.Output = "xxxx";

            //------------Execute Test---------------------------
            qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, qviViewModel.ValidateHitCount);
            Assert.AreEqual(string.Empty, qviViewModel.PreviewViewModel.Output);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_PreviewRequested_ValidationErrorsCountNotZero_DoesNotGetPreviewOutput()
        {
            var qviViewModel = new QuickVariableInputViewModelMock();
            qviViewModel.Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() };

            qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.AreEqual(0, qviViewModel.GetPreviewOutputHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_PreviewRequested_ValidationErrorsCountZero_DoesGetPreviewOutput()
        {
            var qviViewModel = new QuickVariableInputViewModelMock();

            qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.AreEqual(1, qviViewModel.GetPreviewOutputHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_PreviewRequested")]
        public void QuickVariableInputViewModel_PreviewRequested_ValidationErrorsReturnsEmpty_PreviewRequestedArgsOutputIsEmpty()
        {
            //------------Setup for test--------------------------
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => {} );

            //------------Execute Test---------------------------
            qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);

            //------------Assert Results-------------------------
            //Assert.IsTrue(string.IsNullOrEmpty(qviViewModel.OnPreviewRequestedArgs.Output));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_PreviewRequested")]
        public void QuickVariableInputViewModel_PreviewRequested_AllFieldsPopulatedOverWriteFalse_CorrectPreviewOutput()
        {
            const string Expected = @"1 [[Customer().Fname]]
2 [[Customer().LName]]
3 [[Customer().TelNo]]";

            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => {} )
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.AreEqual(Expected, qviViewModel.PreviewViewModel.Output);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_PreviewRequested")]
        public void QuickVariableInputViewModel_PreviewRequested_AllFieldsPopulatedOverWriteTrue_CorrectPreviewOutput()
        {
            const string Expected = @"1 [[Customer().Fname]]
2 [[Customer().LName]]
3 [[Customer().TelNo]]";

            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => {} )
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = true
            };
            qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.AreEqual(Expected, qviViewModel.PreviewViewModel.Output);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_PreviewRequested")]
        public void QuickVariableInputViewModel_PreviewRequested_MoreThenThreeItemsBeingAdded_CorrectPreviewOutput()
        {
            const string Expected = @"1 [[Customer().Fname]]
2 [[Customer().LName]]
3 [[Customer().TelNo]]
...";

            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => {} )
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo,Email",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = true
            };

            qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.AreEqual(Expected, qviViewModel.PreviewViewModel.Output);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_PreviewRequested")]
        public void QuickVariableInputViewModel_PreviewRequested_SplitTypeWithNewLine_CorrectResultsReturned()
        {
            VerifySplitTypeWithNewLine("\r\n", true, true);
            VerifySplitTypeWithNewLine("\n", true, true);
            VerifySplitTypeWithNewLine("\r", true, true);

            VerifySplitTypeWithNewLine("\r\n", false, true);
            VerifySplitTypeWithNewLine("\n", false, true);
            VerifySplitTypeWithNewLine("\r", false, true);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_PreviewRequested")]
        public void QuickVariableInputViewModel_PreviewRequested_RemoveEmptyEntriesFalse_CorrectPreviewOutput()
        {
            const string Expected = @"1 [[Customer().Fname]]
2 
3 [[Customer().TelNo]]";

            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false,
                RemoveEmptyEntries = false
            };

           
            qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.AreEqual(Expected, qviViewModel.PreviewViewModel.Output);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_PreviewRequested")]
        public void QuickVariableInputViewModel_PreviewRequested_RemoveEmptyEntriesTrue_CorrectPreviewOutput()
        {
            const string Expected = @"1 [[Customer().Fname]]
2 [[Customer().LName]]
3 [[Customer().TelNo]]";

            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = true,
                RemoveEmptyEntries = true
            };
            qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.AreEqual(Expected, qviViewModel.PreviewViewModel.Output);
        }
    }
}
