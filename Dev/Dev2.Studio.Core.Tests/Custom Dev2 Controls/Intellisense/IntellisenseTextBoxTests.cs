/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Common.Interfaces;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Core.Tests.Custom_Dev2_Controls.Intellisense
// ReSharper restore CheckNamespace
{
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            Assert.AreEqual(0, textBox.View.Count);
            
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
            Assert.AreEqual(0, textBox.View.Count, "Expected [ 0 ] But got [ " + textBox.View.Count + " ]");
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

            System.Windows.Clipboard.SetText("Cake\t");

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
            var preserveClipboard = System.Windows.Clipboard.GetText();
            try
            {
                bool eventRaised = false;
                EventManager.RegisterClassHandler(typeof(IntellisenseTextBox), IntellisenseTextBox.TabInsertedEvent,
                                                  new RoutedEventHandler((s, e) =>
                                                  {
                                                      eventRaised = true;
                                                  }));

                System.Windows.Clipboard.SetText("Cake");

                IntellisenseTextBox textBox = new IntellisenseTextBox();
                textBox.CreateVisualTree();
                textBox.Paste();

                Assert.IsFalse(eventRaised,
                               "The 'IntellisenseTextBox.TabInsertedEvent' was raised when text that didn't contain a tab was pasted into the IntellisenseTextBox.");
            }
            finally
            {
                System.Windows.Clipboard.SetText(preserveClipboard);
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
            var mockDataListViewModel = new Mock<IDataListViewModel>();
            mockDataListViewModel.Setup(model => model.Resource).Returns(new Mock<IResourceModel>().Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);
            intellisenseProvider.Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                .Returns(default(IList<IntellisenseProviderResult>));

            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.RecordsetFields, IntellisenseProvider = intellisenseProvider.Object };
            textBox.Text = "[[var]]";
            textBox.EnsureIntellisenseResults("[[var]]", false, IntellisenseDesiredResultSet.Default);
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
            Assert.IsTrue(textBox.WrapInBrackets);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IntellisenseTextBox_OnPreviewKeyDown")]
        public void GivenAnExpression_IntellisenseTextBox_AddBracketsToExpression_ShouldAddBrackets()
        {
            //------------Setup for test--------------------------
            var textBox = new IntellisenseTextBox
            {
                FilterType = enIntellisensePartType.ScalarsOnly
                ,
                WrapInBrackets = true
            };
            //------------Execute Test---------------------------
            var bracketsToExpression = textBox.AddBracketsToExpression("Person");
            //------------Assert Results-------------------------
            Assert.IsNotNull(bracketsToExpression);
            Assert.AreEqual("[[Person]]", bracketsToExpression);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IntellisenseTextBox_OnPreviewKeyDown")]
        public void GivenAnExpression_IntellisenseTextBox_SetAppendTextBasedOnSelection()
        {
            //------------Setup for test--------------------------
            var textBox = new IntellisenseTextBox
            {
                FilterType = enIntellisensePartType.ScalarsOnly
                ,
                WrapInBrackets = true
            };
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IntellisenseTextBox_OnPreviewKeyDown")]
        public void GivenJsonExpression_IntellisenseTextBox_AddBracketsToExpression_ShouldAddBrackets()
        {
            //------------Setup for test--------------------------
            var textBox = new IntellisenseTextBox
            {
                FilterType = enIntellisensePartType.JsonObject
                ,
                WrapInBrackets = true
            };
            //------------Execute Test---------------------------
            var bracketsToExpression = textBox.AddBracketsToExpression("Person");
            //------------Assert Results-------------------------
            Assert.IsNotNull(bracketsToExpression);
            Assert.AreEqual("[[@Person]]", bracketsToExpression);
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

            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);
            intellisenseProvider.Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                .Returns(default(IList<IntellisenseProviderResult>));
            var textBox = new IntellisenseTextBox
            {
                FilterType = enIntellisensePartType.ScalarsOnly,
                Text = "[[var()]]",
                IntellisenseProvider = intellisenseProvider.Object
            };
            textBox.EnsureIntellisenseResults("[[var()]]", false, IntellisenseDesiredResultSet.Default);
            Assert.IsTrue(textBox.HasError);

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
            textBox.IsDropDownOpen = true;
            textBox.Text = "ddyy";
            textBox.CaretIndex = 4;
            textBox.InsertItem(intellisenseProviderResult, true);
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
            textBox.IsDropDownOpen = true;
            textBox.Text = "dd yy";
            textBox.CaretIndex = 5;
            textBox.InsertItem(intellisenseProviderResult, true);
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
            textBox.IsDropDownOpen = true;
            textBox.Text = "dd YY";
            textBox.CaretIndex = 5;
            textBox.InsertItem(intellisenseProviderResult, true);
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
            var checkHasUnicodeInText = textBox.CheckHasUnicodeInText("أَبْجَدِي");
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
            textBox.IsDropDownOpen = true;
            textBox.Text = "d YY mm";
            textBox.CaretIndex = 1;
            textBox.InsertItem(intellisenseProviderResult, true);
            //------------Assert Results-------------------------
            Assert.AreEqual("DW YY mm", textBox.Text);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IntellisenseTextBox_KeyDown")]
        public void IntellisenseTextBox_Properties_Not_SetTo_Null()
        {
            //------------Setup for test--------------------------
            var textBox = new IntellisenseTextBox();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(textBox.MinLines);
            Assert.IsNotNull(textBox.LineCount);
            Assert.IsNotNull(textBox.IsPaste);
            Assert.IsNotNull(textBox.IsInCalculateMode);
            Assert.IsNotNull(textBox.HorizontalScrollBarVisibility);
            Assert.IsNotNull(textBox.VerticalScrollBarVisibility);
            Assert.IsNotNull(textBox.TextWrapping);
            Assert.IsNotNull(textBox.AcceptsTab);
            Assert.IsNotNull(textBox.AcceptsReturn);
            Assert.IsNotNull(textBox.AllowMultipleVariables);
            Assert.IsNotNull(textBox.AllowMultilinePaste);
            Assert.IsNotNull(textBox.AllowUserCalculateMode);
            Assert.IsFalse(textBox.SelectAllOnGotFocus);
            Assert.IsTrue(textBox.IsUndoEnabled);
            Assert.IsFalse(textBox.HasError);
        }

        [TestMethod]
        public void IntellisenseBox_GivenWrappedInBrackets_AStringWithNoBrackets_Should_AddBrackets()
        {
            IntellisenseTextBoxTestHelper textBoxTest = new IntellisenseTextBoxTestHelper { WrapInBrackets = true };
            textBoxTest.Text = "Variable";
            textBoxTest.HandleWrapInBrackets(Key.F6);
            Assert.IsFalse(textBoxTest.HasError);
            Assert.AreEqual("[[Variable]]", textBoxTest.Text);
        }

        [TestMethod]
        public void IntellisenseBox_OnPreviewKeyDown_GivenI_Hit_Enter_Key_And_AlloInsertLine_Is_True_Should_AddLine()
        {
            var mockPresentationSource = new Mock<PresentationSource>();
            var textBox = new Mock<TextBox>();
            textBox.Object.MinLines = 5;
            textBox.Object.Text = "Var";
            IntellisenseTextBoxTestHelper testHelper = new IntellisenseTextBoxTestHelper
            {
                AllowUserInsertLine = true,
                TextBox = textBox.Object,
                MinLines = textBox.Object.MinLines
            };
            testHelper.OnPreviewKeyDown(new KeyEventArgs(Keyboard.PrimaryDevice, mockPresentationSource.Object, 0, Key.Enter));
            Assert.IsFalse(testHelper.HasError);
        }

        [TestMethod]
        public void IntellisenseBox_OnPreviewKeyDown_Given_EnterKey_And_AllowInsertLineIsTrueButLineCountIsEqualToMaximumLine_ShouldNotAddLine()
        {
            var mockPresentationSource = new Mock<PresentationSource>();
            var textBox = new Mock<TextBox>();
            textBox.Object.MinLines = 5;
            var givenText = textBox.Object.Text = "Var";
            Assert.IsFalse(givenText.Contains("\r\n"));
            IntellisenseTextBoxTestHelper testHelper = new IntellisenseTextBoxTestHelper
            {
                AllowUserInsertLine = true,
                TextBox = textBox.Object,
                MinLines = textBox.Object.MinLines,
                LineCount = 5
            };
            testHelper.OnPreviewKeyDown(new KeyEventArgs(Keyboard.PrimaryDevice, mockPresentationSource.Object, 0, Key.Enter));
            Assert.IsFalse(testHelper.HasError);
            Assert.IsFalse(testHelper.TextBox.Text.Contains("\r\n"));
        }
        [TestMethod]
        public void IntellisenseBox_OnPreviewKeyDown_GivenI_Hit_Tab_Key_Should_AppendText()
        {
            var mockPresentationSource = new Mock<PresentationSource>();
            var textBox = new Mock<TextBox>();
            textBox.Object.MinLines = 5;
            var givenText = textBox.Object.Text = "Var";
            Assert.IsFalse(givenText.Contains("\r\n"));
            IntellisenseTextBoxTestHelper testHelper = new IntellisenseTextBoxTestHelper
            {
                AllowUserInsertLine = true,
                TextBox = textBox.Object,
                IsDropDownOpen = true,
            };
            testHelper.OnPreviewKeyDown(new KeyEventArgs(Keyboard.PrimaryDevice, mockPresentationSource.Object, 0, Key.Tab));
            Assert.IsFalse(testHelper.HasError);
            Assert.IsFalse(testHelper.TextBox.Text.Contains("\r\n"));
        }        

        [TestMethod]
        public void IntellisenseBox_GivenMultipleValidVariables_HasNoError()
        {
            IntellisenseTextBoxTestHelper textBoxTest = new IntellisenseTextBoxTestHelper { AllowMultipleVariables = true };
            textBoxTest.CreateVisualTree();
            textBoxTest.Text = "\"[[Var]]\", \"[[Var()]]\"";
            textBoxTest.EnsureErrorStatus();
            Assert.IsFalse(textBoxTest.HasError);
            Assert.AreEqual(textBoxTest.DefaultText, textBoxTest.ToolTip);
        }

        [TestMethod]
        public void IntellisenseBox_Function_HasIsCalcMode_SetTo_True()
        {
            IntellisenseTextBoxTestHelper textBoxTest = new IntellisenseTextBoxTestHelper { AllowUserCalculateMode = true };
            textBoxTest.EnsureIntellisenseResults(null, false,IntellisenseDesiredResultSet.Default);
            Assert.IsTrue(string.IsNullOrEmpty(textBoxTest.Text));
            var input = textBoxTest.Text = "=Sum(5,5)";
            textBoxTest.EnsureIntellisenseResults(input,false,IntellisenseDesiredResultSet.Default);
            Assert.IsTrue(textBoxTest.IsInCalculateMode);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_FilterTypeIsAllAndTextIsRecordset_ToolTipHasNoErrorMessage()
        {
            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.All };
            textBox.Text = "[[People(*)]]";
            Assert.IsFalse(textBox.HasError);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_FilterTypeIsRecodsetFieldsAndTextMultipleRecordSetFields_ToolTipHasNoErrorMessage()
        {
            var textBox = new IntellisenseTextBox
            {
                FilterType = enIntellisensePartType.RecordsetFields,
                AllowMultipleVariables = true
            };
            textBox.Text = "[[rec(*).warewolf]],[[rec(*).soa]]";
            Assert.IsFalse(textBox.HasError);
        }
        

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IntellisenseTextBoxTests_ValidateText")]
        public void IntellisenseTextBoxTests_ValidateText_FilterTypeIsJsonObjectAndTextIsJson_ToolTipHasNoErrorMessage()
        {
            IntellisenseTextBox textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.JsonObject };
            textBox.CreateVisualTree();
            textBox.Text = "[[@City]]";
            Assert.IsFalse(textBox.HasError);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IntellisenseTextBoxTests_ValidateText")]
        public void IntellisenseTextBoxTests_ValidateText_FilterTypeIsJsonObjectAndTextIsScalar_ToolTipHasNoErrorMessage()
        {
            var mockPresentationSource =new Mock<PresentationSource>();
            IntellisenseTextBoxTestHelper testHelper = new IntellisenseTextBoxTestHelper();
            testHelper.OnKeyDown(new KeyEventArgs(null, mockPresentationSource.Object, 0, Key.Escape));
            Assert.IsFalse(testHelper.IsDropDownOpen);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseTextBoxTests_SetText")]
        public void IntellisenseTextBoxTests_SetText_InvalidJsonArrayIndex_ShouldError()
        {
            var mockDataListViewModel = new Mock<IDataListViewModel>();
            mockDataListViewModel.Setup(model => model.Resource).Returns(new Mock<IResourceModel>().Object);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);
            intellisenseProvider.Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                .Returns(default(IList<IntellisenseProviderResult>));

            var textBox = new IntellisenseTextBox { FilterType = enIntellisensePartType.JsonObject, IntellisenseProvider = intellisenseProvider.Object };
            textBox.Text = "[[@this.new(1).val(x).s]]";
            Assert.IsTrue(textBox.HasError);
            Assert.AreEqual("Variable name [[@this.new(1).val(x).s]] contains invalid character(s). Only use alphanumeric _ and - ", textBox.ToolTip.ToString());
        }
    }

    public class IntellisenseTextBoxTestHelper : IntellisenseTextBox
    {        
        public new void OnKeyDown(KeyEventArgs e)
        {
            base.IsDropDownOpen = true;
            FilterType = enIntellisensePartType.JsonObject;
            if(e == null)
                throw new ArgumentNullException(nameof(e));
            base.OnKeyDown(e);
        }
   
        public new void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }
        public new void EnsureErrorStatus()
        {
            base.EnsureErrorStatus();
        }
    }
}
