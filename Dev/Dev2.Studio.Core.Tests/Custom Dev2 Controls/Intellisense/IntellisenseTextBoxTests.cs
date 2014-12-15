
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Core.Tests.Utils;
using Dev2.DataList.Contract;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels.DataList;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Core.Tests.Custom_Dev2_Controls.Intellisense
// ReSharper restore CheckNamespace
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
    public class IntellisenseTextBoxTests
    {

        [TestInitialize]
        public void MyTestInitialize()
        {
            var mockDataListViewModel = new Mock<IDataListViewModel>();
            mockDataListViewModel.Setup(model => model.Resource).Returns(new Mock<IResourceModel>().Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            CustomContainer.Register(new Mock<IPopupController>().Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }

        #region Test Initialization

        //BUG 9639
        [TestMethod]
        // This test is here for when the designers load. The check is to prevent them from hammering the providers on load ;)
        public void IntellisenseBoxDoesntQueryProvidersWhenTextLengthIsZero()
        {

            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>())).Verifiable();

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IntellisenseProvider = intellisenseProvider.Object;
            textBox.Text = "";

            // Ensure the get results method is never called, mimics a initalize event from the design surface ;)
            intellisenseProvider.Verify(s => s.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()), Times.Exactly(0));
        }


        //BUG 8761
        [TestMethod]
        public void IntellisenseBoxDoesntCrashWhenGettingResultsGivenAProviderThatThrowsAnException()
        {
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                                .Throws(new Exception());

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IntellisenseProvider = intellisenseProvider.Object;
            textBox.Text = "[[City([[Scalar]]).Na";

            // When exceptions are thrown, no results are to be displayed
            Assert.AreEqual(0, textBox.Items.Count);
            //The desired result is that an exception isn't thrown


            // GetIntellisenseResults -> OnIntellisenseProviderChanged
        }

        //BUG 8761
        [TestMethod]
        public void IntellisenseBoxDoesntCrashWhenInsertingResultsGivenAProviderThatThrowsAnException()
        {
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(
                a => a.PerformResultInsertion(It.IsAny<string>(), It.IsAny<IntellisenseProviderContext>()))
                                .Throws(new Exception());
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, "City", "cake");

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.InsertItem(intellisenseProviderResult, true);

            // When exepctions are thrown, no results are to be displayed
            Assert.AreEqual(0, textBox.Items.Count, "Expected [ 0 ] But got [ " + textBox.Items.Count + " ]");
            //The desired result is that an exception isn't thrown
        }

        [TestMethod]
        public void TextContaningTabIsPasedIntoAnIntellisenseTextBoxExpectedTabInsertedEventIsRaised()
        {
            bool eventRaised = false;
            IntellisenseTextBox sender = null;
            EventManager.RegisterClassHandler(typeof(IntellisenseTextBox), IntellisenseTextBox.TabInsertedEvent,
                                              new RoutedEventHandler((s, e) =>
                                                  {
                                                      eventRaised = true;
                                                      sender = s as IntellisenseTextBox;
                                                  }));

            Clipboard.SetText("Cake\t");

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();

            textBox.Paste();

            Assert.IsTrue(eventRaised,
                          "The 'IntellisenseTextBox.TabInsertedEvent' wasn't raised when text containing a tab was pasted into the IntellisenseTextBox.");
            Assert.AreEqual(textBox, sender,
                            "The IntellisenseTextBox in which the text containg a tab was pasted was different from the one which raised teh event.");

        }

        [TestMethod]
        public void TextContaningNoTabIsPasedIntoAnIntellisenseTextBoxExpectedTabInsertedEventNotRaised()
        {
            var preserveClipboard = Clipboard.GetText();
            try
            {
                bool eventRaised = false;
                EventManager.RegisterClassHandler(typeof(IntellisenseTextBox), IntellisenseTextBox.TabInsertedEvent,
                                                  new RoutedEventHandler((s, e) =>
                                                      {
                                                          eventRaised = true;
                                                      }));

                Clipboard.SetText("Cake");

                IntellisenseTextBox textBox = new IntellisenseTextBox();
                textBox.CreateVisualTree();
                textBox.Paste();

                Assert.IsFalse(eventRaised,
                               "The 'IntellisenseTextBox.TabInsertedEvent' was raised when text that didn't contain a tab was pasted into the IntellisenseTextBox.");
            }
            finally
            {
                Clipboard.SetText(preserveClipboard);
            }

        }

        #endregion Test Initialization

        [TestMethod]
        public void InsertItemExpectedTextboxTextChangedAndErrorStatusUpdated()
        {
            var mockDataListViewModel = new Mock<IDataListViewModel>();
            mockDataListViewModel.Setup(model => model.Resource).Returns(new Mock<IResourceModel>().Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            const string ExpectedText = "[[City()";
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);
            intellisenseProvider.Setup(
                a => a.PerformResultInsertion(It.IsAny<string>(), It.IsAny<IntellisenseProviderContext>())).Returns(ExpectedText);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, ExpectedText, "cake");

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.InsertItem(intellisenseProviderResult, true);

            Thread.Sleep(250);
            Thread.Sleep(100);

            Assert.AreEqual(ExpectedText, textBox.Text, "Expected [ " + ExpectedText + " ] But got [ " + textBox.Text + " ]");
            Assert.AreEqual(true, textBox.HasError, "Expected [ True ] But got [ " + textBox.HasError + " ]");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_FilterTypeIsRecordsetFieldsButTextIsScalar_ToolTipHasErrorMessage()
        {
            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.RecordsetFields };
            textBox.EnsureIntellisenseResults("[[var]]", false, IntellisenseDesiredResultSet.Default);
            Assert.IsTrue(textBox.HasError);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IntellisenseTextBoxTests_Handle")]
        public void IntellisenseTextBoxTests_HandlePasteMessageCallsEnsureIntellisenseResults()
        {
            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.RecordsetFields };
            Assert.IsFalse(textBox.HasError);
            textBox.Text = "[[var]]";
            textBox.Handle(new UpdateAllIntellisenseMessage());
            Assert.IsTrue(textBox.HasError);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_FilterTypeIsRecordsetFieldsAndTextIsRecordset_ToolTipHasNoErrorMessage()
        {
            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.RecordsetFields };
            textBox.EnsureIntellisenseResults("[[var()]]", false, IntellisenseDesiredResultSet.Default);
            Assert.IsFalse(textBox.HasError);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IntellisenseTextBox_KeyDown")]
        public void IntellisenseTextBox_KeyDown_CannotWrapInBracketsWhenNotFSix()
        {
            //------------Setup for test--------------------------
            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.RecordsetFields, WrapInBrackets = true, Text = "var()" };
            //------------Execute Test---------------------------
            textBox.HandleWrapInBrackets(Key.F10);
            //------------Assert Results-------------------------
            Assert.AreEqual("var()", textBox.Text);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IntellisenseTextBox_KeyDown")]
        public void IntellisenseTextBox_KeyDown_CannotCauseWrapInBrackets_WhenWrapInBrackets()
        {

            RunWrappedKeyPress(Key.F6);
            RunWrappedKeyPress(Key.F7);
        }

        private static void RunWrappedKeyPress(Key key)
        {
            //------------Setup for test--------------------------
            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.RecordsetFields, WrapInBrackets = false, Text = "var()" };
            //------------Execute Test---------------------------
            textBox.HandleWrapInBrackets(key);
            //------------Assert Results-------------------------
            Assert.AreEqual("var()", textBox.Text);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IntellisenseTextBox_KeyDown")]
        public void IntellisenseTextBox_KeyDown_CannotCauseWrapInBrackets_WhenNotWrapInBrackets()
        {
            //------------Setup for test--------------------------
            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.RecordsetFields, WrapInBrackets = false, Text = "var()" };
            //------------Execute Test---------------------------
            textBox.HandleWrapInBrackets(Key.F6);
            //------------Assert Results-------------------------
            Assert.AreEqual("var()", textBox.Text);
        }


        [TestMethod]
        public void InsertItemExpectedTextboxTextChanged_InvalidSyntax_ErrorStatusUpdated()
        {
            const string ExpectedText = "[[City(1.Name]]";
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<ADL><City><Name></Name></City></ADL>");

            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            DataListSingleton.SetDataList(dataListViewModel);
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);
            intellisenseProvider.Setup(
                a => a.PerformResultInsertion(It.IsAny<string>(), It.IsAny<IntellisenseProviderContext>())).Returns(ExpectedText);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, ExpectedText, "cake");

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.InsertItem(intellisenseProviderResult, true);

            Thread.Sleep(250);
            Thread.Sleep(100);

            Assert.AreEqual(ExpectedText, textBox.Text, "Expected [ " + ExpectedText + " ] But got [ " + textBox.Text + " ]");
            Assert.IsTrue(textBox.HasError, "Expected [ True ] But got [ " + textBox.HasError + " ]");
        }

        [TestMethod]
        public void InsertItemExpectedTextboxTextChanged_SpaceInFieldName_ErrorStatusUpdated()
        {
            const string ExpectedText = "[[City(). Name]]";
            Mock<IResourceModel> mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.DataList).Returns("<ADL><City><Name></Name></City></ADL>");

            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(mockResourceModel.Object);
            DataListSingleton.SetDataList(dataListViewModel);
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);
            intellisenseProvider.Setup(
                a => a.PerformResultInsertion(It.IsAny<string>(), It.IsAny<IntellisenseProviderContext>())).Returns(ExpectedText);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, ExpectedText, "cake");

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.InsertItem(intellisenseProviderResult, true);

            Thread.Sleep(250);
            Thread.Sleep(100);

            Assert.AreEqual(ExpectedText, textBox.Text, "Expected [ " + ExpectedText + " ] But got [ " + textBox.Text + " ]");
            Assert.IsTrue(textBox.HasError, "Expected [ True ] But got [ " + textBox.HasError + " ]");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_FilterTypeIsScalarsOnlyAndTextIsScalar_ToolTipHasNoErrorMessage()
        {
            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.ScalarsOnly };
            textBox.EnsureIntellisenseResults("[[var]]", false, IntellisenseDesiredResultSet.Default);
            Assert.IsFalse(textBox.HasError);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_FilterTypeIsScalarsOnlyButTextIsRecordset_ToolTipHaErrorMessage()
        {
            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.ScalarsOnly };
            textBox.EnsureIntellisenseResults("[[var()]]", false, IntellisenseDesiredResultSet.Default);
            Assert.IsTrue(textBox.HasError);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_TextChanged")]
        public void IntellisenseTextBox_TextChanged_LessThen200msBetweenTextChanged_TextChangedFiredOnce()
        {
            //------------Setup for test--------------------------            
            var mockIntellisenseTextBox = new MockIntellisenseTextbox();

            var chars = new[] { 'a', 'b', 'c', 'd' };
            mockIntellisenseTextBox.InitTestClass();

            //------------Execute Test---------------------------

            foreach(var c in chars)
            {
                mockIntellisenseTextBox.Text = c.ToString(CultureInfo.InvariantCulture);
                Thread.Sleep(50);
            }

            Thread.Sleep(250);

            //------------Assert Results-------------------------
            Thread.Sleep(100);
            Assert.AreEqual(1, mockIntellisenseTextBox.TextChangedCounter);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_TextChanged")]
        public void IntellisenseTextBox_TextChanged_GreaterThen200msBetweenTextChanged_TextChangedFiredFourTimes()
        {
            //------------Setup for test--------------------------            
            var mockIntellisenseTextBox = new MockIntellisenseTextbox();

            var chars = new[] { 'a', 'b', 'c', 'd' };
            mockIntellisenseTextBox.InitTestClass();

            //------------Execute Test---------------------------

            foreach(var c in chars)
            {
                mockIntellisenseTextBox.Text = c.ToString(CultureInfo.InvariantCulture);
                Thread.Sleep(250);
            }

            //------------Assert Results-------------------------
            Thread.Sleep(100);
            Assert.AreEqual(4, mockIntellisenseTextBox.TextChangedCounter);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_InsertItem")]
        public void IntellisenseTextBox_InsertItem_InsertDateTimeParts_InsertsCorrectly()
        {
            //------------Setup for test--------------------------            
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();

            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(false);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, "yyyy", "yyyy");
            //------------Execute Test---------------------------
            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IsOpen = true;
            textBox.Text = "ddyy";
            textBox.CaretIndex = 4;
            textBox.InsertItem(intellisenseProviderResult, false);
            //------------Assert Results-------------------------
            Assert.AreEqual("ddyyyy", textBox.Text);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_InsertItem")]
        public void IntellisenseTextBox_InsertItem_AppendDateTimePartsWithSpace_InsertsCorrectly()
        {
            //------------Setup for test--------------------------            
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();

            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(false);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, "yyyy", "yyyy");
            //------------Execute Test---------------------------
            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IsOpen = true;
            textBox.Text = "dd yy";
            textBox.CaretIndex = 5;
            textBox.InsertItem(intellisenseProviderResult, false);
            //------------Assert Results-------------------------
            Assert.AreEqual("dd yyyy", textBox.Text);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_InsertItem")]
        public void IntellisenseTextBox_InsertItem_AppendDateTimePartsWithDifferentCase_InsertsCorrectly()
        {
            //------------Setup for test--------------------------            
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();

            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(false);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, "yyyy", "yyyy");
            //------------Execute Test---------------------------
            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IsOpen = true;
            textBox.Text = "dd YY";
            textBox.CaretIndex = 5;
            textBox.InsertItem(intellisenseProviderResult, false);
            //------------Assert Results-------------------------
            Assert.AreEqual("dd yyyy", textBox.Text);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseTextBox_Text")]
        public void IntellisenseTextBox_Text_NotLatinCharacter_ShowMessageBox_TextMadeEmpty()
        {
            //------------Setup for test--------------------------            
            CustomContainer.DeRegister<IPopupController>();
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.ShowInvalidCharacterMessage(It.IsAny<string>()));
            CustomContainer.Register(mockPopupController.Object);
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(false);
            //------------Execute Test---------------------------
            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.Text = "أَبْجَدِي";
            var checkHasUnicodeInText = textBox.CheckHasUnicodeInText(textBox.Text);
            //------------Assert Results-------------------------
            Assert.IsTrue(checkHasUnicodeInText);
            Assert.AreEqual("", textBox.Text);
            mockPopupController.Verify(controller => controller.ShowInvalidCharacterMessage(It.IsAny<string>()), Times.AtLeastOnce());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IntellisenseTextBox_InsertItem")]
        public void IntellisenseTextBox_InsertItem_InsertDateTimePartsIn_InsertsCorrectly()
        {
            //------------Setup for test--------------------------            
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();

            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(false);

            IntellisenseProviderResult intellisenseProviderResult =
                new IntellisenseProviderResult(intellisenseProvider.Object, "DW", "DW");
            //------------Execute Test---------------------------
            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IsOpen = true;
            textBox.Text = "d YY mm";
            textBox.CaretIndex = 1;
            textBox.InsertItem(intellisenseProviderResult, false);
            //------------Assert Results-------------------------
            Assert.AreEqual("DW YY mm", textBox.Text);
        }
    }
}
