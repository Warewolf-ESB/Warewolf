
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
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.FileActivityDesigner
{
    [TestClass]
    public class FileActivityDesignerViewModelTests
    {

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_Constructor")]
        public void FileActivityDesignerViewModelConstructorPropertiesInitialized()
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
        public void FileActivityDesignerViewModelValidateInputPathInvokesValidatePathDone()
        {
            //------------Setup for test-------------------------      
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><a></a></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            var viewModel = CreateViewModel(inputPath: "invalid");
            Assert.IsFalse(viewModel.IsInputPathFocused);

            //------------Execute Test---------------------------
            viewModel.TestValidateInputPath();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidatePathHitCount);
            Assert.IsTrue(viewModel.ValidatePathIsRequired);

        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidateOutputPath")]
        public void FileActivityDesignerViewModelValidateOutputPathInvokesValidatePathDone()
        {
            //------------Setup for test-------------------------  
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><a></a></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            var viewModel = CreateViewModel(outputPath: "invalid");
            Assert.IsFalse(viewModel.IsOutputPathFocused);

            //------------Execute Test---------------------------
            viewModel.TestValidateOutputPath();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidatePathHitCount);
            Assert.IsTrue(viewModel.ValidatePathIsRequired);

        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidateInputAndOutputPaths")]
        public void FileActivityDesignerViewModelValidateInputAndOutputPathsInvokesBothDone()
        {
            //------------Setup for test-------------------------         
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><a></a></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);

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
        public void FileActivityDesignerViewModelValidatePathPathIsEmptyAndIsNotRequiredNoErrors()
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
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModelValidatePathPathIsEmptyAndIsRequiredHasErrors()
        {
            var path = string.Empty;
            const string ExpectedMessageFormat = "{0} cannot be empty or only white space";
            const string LabelText = "Label";
            var viewModel = VerifyValidatePath(path: path, pathIsRequired: true, expectedResult: path, expectedMessageFormat: ExpectedMessageFormat);
            if (string.IsNullOrEmpty(ExpectedMessageFormat))
            {
                Assert.IsNull(viewModel.Errors);
            }
            else
            {
                if (viewModel.Errors != null)
                {

                    Assert.IsNotNull(viewModel.Errors);
                    Assert.AreEqual(1, viewModel.Errors.Count);

                    var error = viewModel.Errors[0];
                    Assert.AreEqual(string.Format(ExpectedMessageFormat, LabelText), error.Message);

                    error.Do();
                    Assert.IsTrue(_onErrorAssigned);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModelValidatePathPathIsNotValidUriAndDoesNotExistHasErrors()
        {
            var path = Guid.NewGuid().ToString();
            const string ExpectedMessageFormat = "Please supply a valid {0}";
            const string LabelText = "Label";
            var viewModel = VerifyValidatePath(path: path, pathIsRequired: true, expectedResult: path, expectedMessageFormat: ExpectedMessageFormat);
            if (string.IsNullOrEmpty(ExpectedMessageFormat))
            {
                Assert.IsNull(viewModel.Errors);
            }
            else
            {
                if (viewModel.Errors != null)
                {

                    Assert.IsNotNull(viewModel.Errors);
                    Assert.AreEqual(1, viewModel.Errors.Count);

                    var error = viewModel.Errors[0];
                    Assert.AreEqual(string.Format(ExpectedMessageFormat, LabelText), error.Message);

                    error.Do();
                    Assert.IsTrue(_onErrorAssigned);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModelValidatePathPathIsNotValidUriAndDoesExistNoErrors()
        {
            const string Path = "C:\\";
            const string ExpectedMessageFormat = null;
            var viewModel = VerifyValidatePath(path: Path, pathIsRequired: true, expectedResult: Path, expectedMessageFormat: ExpectedMessageFormat);
            if (string.IsNullOrEmpty(ExpectedMessageFormat))
            {
                Assert.IsNull(viewModel.Errors);
            }
            else
            {
                if (viewModel.Errors != null)
                {

                    Assert.IsNotNull(viewModel.Errors);
                    Assert.AreEqual(1, viewModel.Errors.Count);

                    var error = viewModel.Errors[0];
                    Assert.AreEqual(string.Format(string.Empty), error.Message);

                    error.Do();
                    Assert.IsTrue(_onErrorAssigned);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModelValidatePathPathIsValidUriAndSchemIsNotValidHasErrors()
        {
            const string Path = "http://";
            const string ExpectedMessageFormat = "Please supply a valid {0}";
            const string LabelText = "Label";
            var viewModel = VerifyValidatePath(path: Path, pathIsRequired: true, expectedResult: Path, expectedMessageFormat: ExpectedMessageFormat);
            if (string.IsNullOrEmpty(ExpectedMessageFormat))
            {
                Assert.IsNull(viewModel.Errors);
            }
            else
            {
                if (viewModel.Errors != null)
                {

                    Assert.IsNotNull(viewModel.Errors);
                    Assert.AreEqual(1, viewModel.Errors.Count);

                    var error = viewModel.Errors[0];
                    Assert.AreEqual(string.Format(ExpectedMessageFormat, LabelText), error.Message);

                    error.Do();
                    Assert.IsTrue(_onErrorAssigned);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModelValidatePathPathIsValidUriAndSchemIsValidNoErrors()
        {
            foreach (var scheme in FileActivityDesignerViewModel.ValidUriSchemes)
            {
                var path = scheme + "://tmp.txt";
                const string ExpectedMessageFormat = null;
                var viewModel = VerifyValidatePath(path: scheme + "://tmp.txt", pathIsRequired: true, expectedResult: path, expectedMessageFormat: ExpectedMessageFormat);
                if (string.IsNullOrEmpty(ExpectedMessageFormat))
                {
                    Assert.IsNull(viewModel.Errors);
                }
                else
                {
                    if (viewModel.Errors != null)
                    {

                        Assert.IsNotNull(viewModel.Errors);
                        Assert.AreEqual(1, viewModel.Errors.Count);

                        var error = viewModel.Errors[0];
                        Assert.AreEqual(string.Format(string.Empty), error.Message);

                        error.Do();
                        Assert.IsTrue(_onErrorAssigned);
                    }
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModelValidatePathPathIsInvalidExpressionHasErrors()
        {
            const string Path = "a]]";
            const string ExpectedMessageFormat = "Label - Invalid expression: opening and closing brackets don't match.";
            var viewModel = VerifyValidatePath(path: Path, pathIsRequired: true, expectedResult: Path, expectedMessageFormat: ExpectedMessageFormat);
            if (string.IsNullOrEmpty(ExpectedMessageFormat))
            {
                Assert.IsNull(viewModel.Errors);
            }
            else
            {
                if (viewModel.Errors != null)
                {

                    Assert.IsNotNull(viewModel.Errors);
                    Assert.AreEqual(1, viewModel.Errors.Count);

                    var error = viewModel.Errors[0];
                    Assert.AreEqual(string.Format(ExpectedMessageFormat), error.Message);

                    error.Do();
                    Assert.IsTrue(_onErrorAssigned);
                }
            }
        }

        bool _onErrorAssigned;

        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidatePath")]
        public void FileActivityDesignerViewModelValidateUrlError()
        {
            const string Path = "htp://www.cowbell.co.za";
            const string ExpectedMessageFormat = "Please supply a valid Label";

            var viewModel = VerifyValidatePath(path: Path, pathIsRequired: true, expectedResult: Path, expectedMessageFormat: ExpectedMessageFormat);

            if (string.IsNullOrEmpty(ExpectedMessageFormat))
            {
                Assert.IsNull(viewModel.Errors);
            }
            else
            {
                if (viewModel.Errors != null)
                {

                    Assert.IsNotNull(viewModel.Errors);
                    Assert.AreEqual(1, viewModel.Errors.Count);

                    var error = viewModel.Errors[0];
                    Assert.AreEqual(string.Format(ExpectedMessageFormat), error.Message);

                    error.Do();
                    Assert.IsTrue(_onErrorAssigned);
                }
            }
        }

        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidateFileContent")]
        public void FileActivityDesignerViewModelVerifyValidateFileContentEmptyPass()
        {
            var content = string.Empty;
            string expectedResult = string.Empty;

            var viewModel = VerifyValidateFileContent(content, expectedResult);
            Assert.IsNull(viewModel.Errors);

        }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidateFileContent")]
        public void FileActivityDesignerViewModelVerifyValidateFileContentPass()
        {
            const string Content = "File Contents Stored Here";
            const string ExpectedResult = "File Contents Stored Here";

            var viewModel = VerifyValidateFileContent(Content, ExpectedResult);
            Assert.IsNull(viewModel.Errors);

        }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidateFileContent")]
        public void FileActivityDesignerViewModelVerifyValidateFileContentValidExpressionPass()
        {
            const string Content = "File [[contains]] Stored Here";
            const string ExpectedResult = "File [[contains]] Stored Here";
            var viewModel = VerifyValidateFileContent(Content, ExpectedResult);
            Assert.IsNull(viewModel.Errors);

        }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidateFileContent")]
        public void FileActivityDesignerViewModelVerifyValidateFileContentInValidExpressionPass()
        {
            const string Content = "File [[contains&]] Stored Here";
            const string ExpectedResult = "File [[contains&]] Stored Here";
            const string ExpectedMessageFormat = "Label - Variable name [[contains&]] contains invalid character(s)";

            var viewModel = VerifyValidateFileContent(Content, ExpectedResult);
            Assert.AreEqual(1, viewModel.Errors.Count);
            var error = viewModel.Errors[0];
            Assert.AreEqual(string.Format(ExpectedMessageFormat), error.Message);
            error.Do();
            Assert.IsTrue(_onErrorAssigned);
        }



        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidateArchivePassword")]
        public void FileActivityDesignerViewModelValidateArchivePasswordPass()
        {
            const string Password = "Password";
            const string ExpectedResult = "Password";
            var viewModel = VerifyValidateArchivePassword(Password, ExpectedResult);
            Assert.IsNull(viewModel.Errors);
        }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidateArchivePassword")]
        public void FileActivityDesignerViewModelValidateArchiveNoPasswordPass()
        {
            string password = String.Empty;
            string expectedResult = String.Empty;
            var viewModel = VerifyValidateArchivePassword(password, expectedResult);
            Assert.IsNull(viewModel.Errors);
        }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidateArchivePassword")]
        public void FileActivityDesignerViewModelValidateArchiveVariablePasswordPass()
        {
            const string Password = "[[password]]";
            const string ExpectedResult = "[[password]]";
            var viewModel = VerifyValidateArchivePassword(Password, ExpectedResult);
            Assert.IsNull(viewModel.Errors);

        }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidateArchivePassword")]
        public void FileActivityDesignerViewModelValidateArchiveVariablePasswordFail()
        {
            const string Password = "[[password&]]";
            const string ExpectedResult = "[[password&]]";
            const string ExpectedMessageFormat = "Label - Variable name [[password&]] contains invalid character(s)";

            var viewModel = VerifyValidateArchivePassword(Password, ExpectedResult);
            
            Assert.AreEqual(1, viewModel.Errors.Count);
            var error = viewModel.Errors[0];
            Assert.AreEqual(string.Format(ExpectedMessageFormat), error.Message);
            error.Do();
            Assert.IsTrue(_onErrorAssigned);
        }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidateFileContent")]
        public void FileActivityDesignerViewModelValidateFileContent()
        {
            //------------Setup for test-------------------------  
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><a></a></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            var viewModel = CreateViewModel();
            Assert.IsFalse(viewModel.IsOutputPathFocused);

            const string Content = "This is file Content";
            const string Label = "Label";

            //------------Execute Test---------------------------
            viewModel.TestValidateBaseFileContent(Content, Label);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidateFileContentHitCount);
            Assert.AreEqual(Content, viewModel.FileContentValue);


        }


        [TestMethod]
        [Owner("Robin van den Heever")]
        [TestCategory("FileActivityDesignerViewModel_ValidateFileContent")]
        public void FileActivityDesignerViewModelValidatePassword()
        {
            //------------Setup for test-------------------------  
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><a></a></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            var viewModel = CreateViewModel();
            Assert.IsFalse(viewModel.IsOutputPathFocused);

            const string Password = "P4ssw0rd";
            const string Label = "Password";

            //------------Execute Test---------------------------
            viewModel.TestValidateBaseArchivePassword(Password, Label);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidateArchivePasswordCount);
            Assert.AreEqual(Password, viewModel.ArchivePasswordValue);

        }



        public TestFileActivityDesignerViewModel VerifyValidatePath(string path, bool pathIsRequired, string expectedResult, string expectedMessageFormat)
        {
            //------------Setup for test-------------------------
            const string LabelText = "Label";


            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><contains></contains></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            var viewModel = CreateViewModel();

            //------------Execute Test---------------------------
            var result = viewModel.TestValidatePath(path: path, pathIsRequired: pathIsRequired, onError: () => { _onErrorAssigned = true; }, label: LabelText);

            Assert.AreEqual(expectedResult, result);
            return viewModel;

        }

        public TestFileActivityDesignerViewModel VerifyValidateFileContent(string content, string expectedResult)
        {
            //------------Setup for test-------------------------
            const string LabelText = "Label";
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><contains></contains></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            var viewModel = CreateViewModel();

            //------------Execute Test---------------------------
            var result = viewModel.TestValidateFileContent(content: content, label: LabelText, onError: () => { _onErrorAssigned = true; }, contentIsRequired: true);
            Assert.AreEqual(expectedResult, result);
            return viewModel;

        }


        public TestFileActivityDesignerViewModel VerifyValidateArchivePassword(string password, string expectedResult)
        {
            //------------Setup for test-------------------------
            const string LabelText = "Label";
            Mock<IDataListViewModel> mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<DataList><password></password></DataList>");
            mockDataListViewModel.Setup(model => model.Resource).Returns(mockResourceModel.Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            var viewModel = CreateViewModel();

            //------------Execute Test---------------------------
            var result = viewModel.TestValidateArchivePassword(password: password, label: LabelText, onError: () => { _onErrorAssigned = true; }, contentIsRequired: true);
            Assert.AreEqual(expectedResult, result);
            return viewModel;

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
