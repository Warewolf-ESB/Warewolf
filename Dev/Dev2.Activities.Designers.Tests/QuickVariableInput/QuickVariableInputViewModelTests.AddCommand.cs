using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.QuickVariableInput;
using Dev2.Providers.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.QuickVariableInput
{
    public partial class QuickVariableInputViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_WiredUpCorrectly()
        {
            var qviViewModel = new QuickVariableInputViewModelMock();

            qviViewModel.AddCommand.Execute(null);

            Assert.AreEqual(1, qviViewModel.DoAddHitCount);

            qviViewModel.CanAdd = true;
            Assert.IsTrue(qviViewModel.AddCommand.CanExecute(null));

            qviViewModel.CanAdd = false;
            Assert.IsFalse(qviViewModel.AddCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_ValidInputs_AddsListToActivityCollectionViewModel()
        {
            List<string> actualListToAdd = null;
            var actualOverwrite = false;

            var collectionViewModel = new Mock<IActivityCollectionViewModel>();
            collectionViewModel.Setup(m => m.AddListToCollection(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()))
                               .Callback((IEnumerable<string> listToAdd, bool overwrite) =>
                               {
                                   actualListToAdd = listToAdd.ToList();
                                   actualOverwrite = overwrite;
                               })
                               .Verifiable();

            var qviViewModel = new QuickVariableInputViewModel(collectionViewModel.Object)
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            qviViewModel.AddCommand.Execute(null);

            collectionViewModel.Verify(m => m.AddListToCollection(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()));
            Assert.IsNotNull(actualListToAdd);
            Assert.AreEqual(3, actualListToAdd.Count());

            Assert.AreEqual(qviViewModel.Overwrite, actualOverwrite);
            var expected = qviViewModel.PreviewViewModel.Inputs.Select(input => input.Key).ToList();
            CollectionAssert.AreEqual(expected, actualListToAdd);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_ValidInputs_InvokesDoClear()
        {
            var qviViewModel = new QuickVariableInputViewModelMock
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            qviViewModel.AddCommand.Execute(null);

            Assert.AreEqual(1, qviViewModel.DoClearHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_ValidationErrorsCountNotZero_DoesNotAddListToPreviewInputs()
        {
            var qviViewModel = new QuickVariableInputViewModelMock();
            qviViewModel.ValidationErrorsValue.Add(new ErrorInfo());

            var previewViewModelInputsCollectionChanged = false;
            qviViewModel.PreviewViewModel.Inputs.CollectionChanged += (sender, args) => { previewViewModelInputsCollectionChanged = true; };

            qviViewModel.AddCommand.Execute(null);

            Assert.IsFalse(previewViewModelInputsCollectionChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_ValidationErrorsCountNotZero_DoesNotAddListToActivityCollectionViewModel()
        {
            var collectionViewModel = new Mock<IActivityCollectionViewModel>();
            collectionViewModel.Setup(m => m.AddListToCollection(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Verifiable();

            var qviViewModel = new QuickVariableInputViewModelMock(collectionViewModel.Object);
            qviViewModel.ValidationErrorsValue.Add(new ErrorInfo());

            qviViewModel.AddCommand.Execute(null);

            collectionViewModel.Verify(m => m.AddListToCollection(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Never());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_ValidationErrorsCountNotZero_DoesNotInvokeDoClear()
        {
            var qviViewModel = new QuickVariableInputViewModelMock();
            qviViewModel.ValidationErrorsValue.Add(new ErrorInfo());

            qviViewModel.AddCommand.Execute(null);

            Assert.AreEqual(0, qviViewModel.DoClearHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_IncompleteVariableList_CorrectResultsReturned()
        {
            var qviViewModel = new QuickVariableInputViewModel(new Mock<IActivityCollectionViewModel>().Object)
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "FName,LName,",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };
            qviViewModel.AddCommand.Execute(null);

            var expected = new List<string> { "[[Customer().FName]]", "[[Customer().LName]]" };
            var actual = qviViewModel.PreviewViewModel.Inputs.Select(input => input.Key).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_SplitTypeWithChars_CorrectResultsReturned()
        {
            var qviViewModel = new QuickVariableInputViewModel(new Mock<IActivityCollectionViewModel>().Object)
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "FName,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };
            qviViewModel.AddCommand.Execute(null);

            var expected = new List<string> { "[[Customer().FName]]", "[[Customer().LName]]", "[[Customer().TelNo]]" };
            var actual = qviViewModel.PreviewViewModel.Inputs.Select(input => input.Key).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_SplitTypeWithChars_SplitTokenSpace_CorrectResultsReturned()
        {
            var qviViewModel = new QuickVariableInputViewModel(new Mock<IActivityCollectionViewModel>().Object)
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "FName LName TelNo",
                SplitType = "Chars",
                SplitToken = " ",
                Overwrite = false
            };

            Assert.IsTrue(qviViewModel.CanAdd);
            Assert.IsTrue(qviViewModel.AddCommand.CanExecute(null));

            qviViewModel.AddCommand.Execute(null);
            var expected = new List<string> { "[[Customer().FName]]", "[[Customer().LName]]", "[[Customer().TelNo]]" };
            var actual = qviViewModel.PreviewViewModel.Inputs.Select(input => input.Key).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_SplitTypeWithIndex_CorrectResultsReturned()
        {
            var qviViewModel = new QuickVariableInputViewModel(new Mock<IActivityCollectionViewModel>().Object)
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "FNameLNameTelNo",
                SplitType = "Index",
                SplitToken = "4",
                Overwrite = false
            };

            qviViewModel.AddCommand.Execute(null);

            var actual = qviViewModel.PreviewViewModel.Inputs.Select(input => input.Key).ToList();
            var expected = new List<string> { "[[Customer().FNam]]", "[[Customer().eLNa]]", "[[Customer().meTe]]", "[[Customer().lNo]]" };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_SplitTypeWithNewLine_CorrectResultsReturned()
        {
            VerifySplitTypeWithNewLine("\r\n", doesVariableListContainNewLine: true);
            VerifySplitTypeWithNewLine("\n", doesVariableListContainNewLine: true);
            VerifySplitTypeWithNewLine("\r", doesVariableListContainNewLine: true, doPreview: false);

            VerifySplitTypeWithNewLine("\r\n", doesVariableListContainNewLine: false);
            VerifySplitTypeWithNewLine("\n", doesVariableListContainNewLine: false);
            VerifySplitTypeWithNewLine("\r", doesVariableListContainNewLine: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_SplitTypeWithSpace_CorrectResultsReturned()
        {
            var qviViewModel = new QuickVariableInputViewModel(new Mock<IActivityCollectionViewModel>().Object)
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = @"FName LName TelNo Email",
                SplitType = "Space",
                SplitToken = "",
                Overwrite = false
            };

            qviViewModel.AddCommand.Execute(null);

            var actual = qviViewModel.PreviewViewModel.Inputs.Select(input => input.Key).ToList();
            var expected = new List<string> { "[[Customer().FName]]", "[[Customer().LName]]", "[[Customer().TelNo]]", "[[Customer().Email]]" };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_SplitTypeWithTab_CorrectResultsReturned()
        {
            var qviViewModel = new QuickVariableInputViewModel(new Mock<IActivityCollectionViewModel>().Object)
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = @"FName	LName	TelNo	Email",
                SplitType = "Tab",
                SplitToken = "",
                Overwrite = false
            };

            qviViewModel.AddCommand.Execute(null);

            var actual = qviViewModel.PreviewViewModel.Inputs.Select(input => input.Key).ToList();
            var expected = new List<string> { "[[Customer().FName]]", "[[Customer().LName]]", "[[Customer().TelNo]]", "[[Customer().Email]]" };

            CollectionAssert.AreEqual(expected, actual);
        }


        static void VerifySplitTypeWithNewLine(string newLine, bool doesVariableListContainNewLine, bool doPreview = false)
        {
            var variableListStringFormat = doesVariableListContainNewLine ? "FName{0}LName{0}TelNo{0}Email" : "FName";
            var expected = new List<string> { "[[Customer().FName]]" };
            var expectedPreviewOutput = "1 [[Customer().FName]]";

            if(doesVariableListContainNewLine)
            {
                expected.AddRange(new[] { "[[Customer().LName]]", "[[Customer().TelNo]]", "[[Customer().Email]]" });
                expectedPreviewOutput += string.Format("{0}2 [[Customer().LName]]{0}3 [[Customer().TelNo]]{0}...", Environment.NewLine);
            }

            var qviViewModel = new QuickVariableInputViewModel(new Mock<IActivityCollectionViewModel>().Object)
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = string.Format(variableListStringFormat, newLine),
                SplitType = "New Line",
                SplitToken = "",
                Overwrite = false
            };

            if(doPreview)
            {
                qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);
                Assert.AreEqual(expectedPreviewOutput, qviViewModel.PreviewViewModel.Output);
            }
            else
            {
                qviViewModel.AddCommand.Execute(null);
            }
            var actual = qviViewModel.PreviewViewModel.Inputs.Select(input => input.Key).ToList();
            Assert.AreEqual(expected.Count, actual.Count);
            CollectionAssert.AreEqual(expected, actual);
        }

    }
}
