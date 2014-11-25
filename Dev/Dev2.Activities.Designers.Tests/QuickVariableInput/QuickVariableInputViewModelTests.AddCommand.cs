
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
using System.Linq;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.QuickVariableInput;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Services.Events;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels.WorkSurface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var hitCount = 0;
            qviViewModel.AddCommand.CanExecuteChanged += (sender, args) =>
            {
                hitCount++;
            };
            Assert.AreEqual(1, qviViewModel.DoAddHitCount);

            qviViewModel.CanAdd = true;
            Assert.IsTrue(qviViewModel.AddCommand.CanExecute(null));

            qviViewModel.CanAdd = false;
            Assert.IsFalse(qviViewModel.AddCommand.CanExecute(null));

            Assert.AreEqual(2,hitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_ValidInputs_AddsListToActivityCollectionViewModel()
        {
            List<string> actualListToAdd = null;
            var actualOverwrite = false;

            var addListToCollection = new Action<IEnumerable<string>, bool>((source, overwrite) =>
            {
                actualListToAdd = source.ToList();
                actualOverwrite = overwrite;
            });

            var qviViewModel = new QuickVariableInputViewModel(addListToCollection)
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = "Fname,LName,TelNo",
                SplitType = "Chars",
                SplitToken = ",",
                Overwrite = false
            };

            qviViewModel.AddCommand.Execute(null);

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
        public void QuickVariableInputViewModel_AddCommand_DoesNotValidate()
        {
            var qviViewModel = new QuickVariableInputViewModelMock { Errors = new List<IActionableErrorInfo>() { new ActionableErrorInfo() } };

            qviViewModel.AddCommand.Execute(null);
            Assert.AreEqual(0, qviViewModel.ValidateHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_IncompleteVariableList_CorrectResultsReturned()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
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
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
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
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
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
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
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
            VerifySplitTypeWithNewLine("\r", doesVariableListContainNewLine: true);

            VerifySplitTypeWithNewLine("\r\n", doesVariableListContainNewLine: false);
            VerifySplitTypeWithNewLine("\n", doesVariableListContainNewLine: false);
            VerifySplitTypeWithNewLine("\r", doesVariableListContainNewLine: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_SplitTypeWithSpace_CorrectResultsReturned()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
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
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
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

            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
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

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_RemoveEmptyEntriesTrue_CorrectResultsReturned()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = @"FName LName TelNo Email",
                SplitType = "Space",
                SplitToken = "",
                Overwrite = false,
                RemoveEmptyEntries = true
            };

            qviViewModel.AddCommand.Execute(null);

            var actual = qviViewModel.PreviewViewModel.Inputs.Select(input => input.Key).ToList();
            var expected = new List<string> { "[[Customer().FName]]", "[[Customer().LName]]", "[[Customer().TelNo]]", "[[Customer().Email]]" };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_RemoveEmptyEntriesFalse_CorrectResultsReturned()
        {
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { })
            {
                Suffix = "",
                Prefix = "Customer().",
                VariableListString = @"FName  TelNo Email",
                SplitType = "Space",
                SplitToken = "",
                Overwrite = false,
                RemoveEmptyEntries = false
            };

            qviViewModel.AddCommand.Execute(null);

            var actual = qviViewModel.PreviewViewModel.Inputs.Select(input => input.Key).ToList();
            var expected = new List<string> { "[[Customer().FName]]", "", "[[Customer().TelNo]]", "[[Customer().Email]]" };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("QuickVariableInputViewModel_AddCommand")]
        public void QuickVariableInputViewModel_AddCommand_PublishesAAddStringListToDataListMessage()
        {
            var qviViewModel = new QuickVariableInputViewModelMock();
            var workflowdesignerVm = new FakeWorkflowDesignerViewModel(EventPublishers.Aggregator);
            qviViewModel.AddCommand.Execute(null);
            Assert.AreEqual(1, workflowdesignerVm.WasHandled);
        }
    }

    internal class FakeWorkflowDesignerViewModel : BaseWorkSurfaceViewModel,
                                       IHandle<AddStringListToDataListMessage>
    {
        public int WasHandled = 0;

        public FakeWorkflowDesignerViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {

        }

        public void Handle(AddStringListToDataListMessage message)
        {
            WasHandled++;
        }
    }
}
