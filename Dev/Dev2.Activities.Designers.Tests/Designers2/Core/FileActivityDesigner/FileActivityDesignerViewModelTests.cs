
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.FileActivityDesigner
{
    [TestClass]
    public class FileActivityDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileActivityDesignerViewModel_Constructor_NullInputPathLabel_ThrowsArgumentNullException()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileActivityDesignerViewModel_Constructor_NullOutputPathLabel_ThrowsArgumentNullException()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel("xxx", null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_Constructor")]
        public void FileActivityDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------
            var expectedUriSchemes = new List<string> { "file", "ftp", "ftps", "sftp" };

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("Input Label", viewModel.InputPathLabel);
            Assert.AreEqual("Output Label", viewModel.OutputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(0, viewModel.TitleBarToggles.Count);

            CollectionAssert.AreEqual(expectedUriSchemes, FileActivityDesignerViewModel.ValidUriSchemes);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidateInputPath")]
        public void FileActivityDesignerViewModel_ValidateInputPath_InvokesValidatePath_Done()
        {
            //------------Setup for test-------------------------            
            var viewModel = CreateViewModel(inputPath: "invalid");
            Assert.IsFalse(viewModel.IsInputPathFocused);

            //------------Execute Test---------------------------
            viewModel.TestValidateInputPath();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidatePathHitCount);
            Assert.IsTrue(viewModel.ValidatePathIsRequired);

            // Verify that correct on error action was assigned
            viewModel.Errors[0].Do();
            Assert.IsTrue(viewModel.IsInputPathFocused);
            Assert.AreEqual("Please supply a valid " + viewModel.InputPathLabel, viewModel.Errors[0].Message);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidateOutputPath")]
        public void FileActivityDesignerViewModel_ValidateOutputPath_InvokesValidatePath_Done()
        {
            //------------Setup for test-------------------------            
            var viewModel = CreateViewModel(outputPath: "invalid");
            Assert.IsFalse(viewModel.IsOutputPathFocused);

            //------------Execute Test---------------------------
            viewModel.TestValidateOutputPath();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidatePathHitCount);
            Assert.IsTrue(viewModel.ValidatePathIsRequired);

            // Verify that correct on error action was assigned
            viewModel.Errors[0].Do();
            Assert.IsTrue(viewModel.IsOutputPathFocused);
            Assert.AreEqual("Please supply a valid " + viewModel.OutputPathLabel, viewModel.Errors[0].Message);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidateInputAndOutputPaths")]
        public void FileActivityDesignerViewModel_ValidateInputAndOutputPaths_InvokesBoth_Done()
        {
            //------------Setup for test-------------------------            
            var viewModel = CreateViewModel();

            //------------Execute Test---------------------------
            viewModel.TestValidateInputAndOutputPaths();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateOutputPathHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModel_ValidatePath_PathIsEmptyAndIsNotRequired_NoErrors()
        {
            //------------Setup for test-------------------------
            var viewModel = CreateViewModel();

            //------------Execute Test---------------------------
            var result = viewModel.TestValidatePath(path: null, pathIsRequired: false, onError: () => { }, label: "Label");

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, result);
            Assert.IsNull(viewModel.Errors);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModel_ValidatePath_PathIsEmptyAndIsRequired_HasErrors()
        {
            var path = string.Empty;
            Verify_ValidatePath(path: path, pathIsRequired: true, expectedResult: path, expectedMessageFormat: "{0} must have a value");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModel_ValidatePath_PathIsNotValidUriAndDoesNotExist_HasErrors()
        {
            var path = Guid.NewGuid().ToString();
            Verify_ValidatePath(path: path, pathIsRequired: true, expectedResult: path, expectedMessageFormat: "Please supply a valid {0}");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModel_ValidatePath_PathIsNotValidUriAndDoesExist_NoErrors()
        {
            const string Path = "C:\\";
            Verify_ValidatePath(path: Path, pathIsRequired: true, expectedResult: Path, expectedMessageFormat: null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModel_ValidatePath_PathIsValidUriAndSchemIsNotValid_HasErrors()
        {
            const string Path = "http://";
            Verify_ValidatePath(path: Path, pathIsRequired: true, expectedResult: Path, expectedMessageFormat: "Please supply a valid {0}");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModel_ValidatePath_PathIsValidUriAndSchemIsValid_NoErrors()
        {
            foreach(var scheme in FileActivityDesignerViewModel.ValidUriSchemes)
            {
                var path = scheme + "://tmp.txt";
                Verify_ValidatePath(path: scheme + "://tmp.txt", pathIsRequired: true, expectedResult: path, expectedMessageFormat: null);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModel_ValidatePath_PathIsInvalidExpression_HasErrors()
        {
            const string Path = "a]]";
            Verify_ValidatePath(path: Path, pathIsRequired: true, expectedResult: Path, expectedMessageFormat: "Invalid expression: opening and closing brackets don't match.");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModel_ValidatePath_PathIsValidExpression_NoErrors()
        {
            const string Path = "[[a]]";
            var expectedPath = FileActivityDesignerViewModel.ValidUriSchemes[0] + "://temp";
            Verify_ValidatePath(path: Path, pathIsRequired: true, expectedResult: expectedPath, expectedMessageFormat: null);
        }

        static void Verify_ValidatePath(string path, bool pathIsRequired, string expectedResult, string expectedMessageFormat)
        {
            //------------Setup for test-------------------------
            const string LabelText = "Label";
            var onErrorAssigned = false;

            var viewModel = CreateViewModel();

            //------------Execute Test---------------------------
            var result = viewModel.TestValidatePath(path: path, pathIsRequired: pathIsRequired, onError: () => { onErrorAssigned = true; }, label: LabelText);

            Assert.AreEqual(expectedResult, result);
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
                Assert.IsTrue(onErrorAssigned);
            }
        }

        static TestFileActivityDesignerViewModel CreateViewModel(string inputPathLabel = "Input Label", string outputPathLabel = "Output Label", string inputPath = null, string outputPath = null)
        {
            var viewModel = new TestFileActivityDesignerViewModel(ModelItemUtils.CreateModelItem(
                new DsfPathCopy
                {
                    InputPath = inputPath,
                    OutputPath = outputPath
                }), inputPathLabel, outputPathLabel);
            return viewModel;
        }
    }
}
