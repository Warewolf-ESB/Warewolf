
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
using System.Windows;
using Dev2.Activities.Designers2.Core.QuickVariableInput;
using Dev2.Activities.Preview;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.QuickVariableInput
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public partial class QuickVariableInputViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_Constructor")]
        public void QuickVariableInputViewModel_Constructor_PreviewViewModel_NotNull()
        {
            //------------Setup for test--------------------------
            var qviViewModel = new QuickVariableInputViewModelMock();

            //------------Execute Test---------------------------
            var previewViewModel = qviViewModel.PreviewViewModel;

            //------------Assert Results-------------------------
            Assert.IsNotNull(previewViewModel);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QuickVariableInputViewModel_Constructor")]
        public void QuickVariableInputViewModel_Constructor_PreviewViewModel_PreviewRequestedWiredUp()
        {
            //------------Setup for test--------------------------
            var qviViewModel = new QuickVariableInputViewModelMock();

            //------------Execute Test---------------------------
            qviViewModel.PreviewViewModel.PreviewCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, qviViewModel.DoPreviewHitCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("QuickVariableInputViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QuickVariableInputViewModel_Constructor_NullAddToCollectionArguments_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
#pragma warning disable 168
            var qviViewModel = new QuickVariableInputViewModel(null);
#pragma warning restore 168
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("QuickVariableInputViewModel_Constructor")]
        public void QuickVariableInputViewModel_Constructor_WithParameter_SetsDefaultPropertyValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var qviViewModel = new QuickVariableInputViewModel((source, overwrite) => { });

            //------------Assert Results-------------------------
            Assert.IsNotNull(qviViewModel);
            Assert.IsInstanceOfType(qviViewModel, typeof(DependencyObject));
            Assert.AreEqual(string.Empty, qviViewModel.SplitToken);
            Assert.AreEqual(string.Empty, qviViewModel.VariableListString);
            Assert.AreEqual(string.Empty, qviViewModel.Prefix);
            Assert.AreEqual(string.Empty, qviViewModel.Suffix);

            // The following 3 are related - SplitType determines the value of the other 2 properties
            Assert.AreEqual("Chars", qviViewModel.SplitType);
            Assert.IsFalse(qviViewModel.CanAdd);
            Assert.IsTrue(qviViewModel.IsSplitTokenEnabled);
            Assert.IsTrue(qviViewModel.IsOverwriteEnabled);
            Assert.IsTrue(qviViewModel.RemoveEmptyEntries);

            Assert.AreEqual(5, qviViewModel.SplitTypeList.Count);
            CollectionAssert.Contains(qviViewModel.SplitTypeList, QuickVariableInputViewModel.SplitTypeIndex);
            CollectionAssert.Contains(qviViewModel.SplitTypeList, QuickVariableInputViewModel.SplitTypeChars);
            CollectionAssert.Contains(qviViewModel.SplitTypeList, QuickVariableInputViewModel.SplitTypeNewLine);
            CollectionAssert.Contains(qviViewModel.SplitTypeList, QuickVariableInputViewModel.SplitTypeSpace);
            CollectionAssert.Contains(qviViewModel.SplitTypeList, QuickVariableInputViewModel.SplitTypeTab);

            Assert.IsNotNull(qviViewModel.ClearCommand);
            Assert.IsInstanceOfType(qviViewModel.ClearCommand, typeof(DelegateCommand));
            Assert.IsTrue(qviViewModel.ClearCommand.CanExecute(null));

            Assert.IsNotNull(qviViewModel.AddCommand);
            Assert.IsInstanceOfType(qviViewModel.AddCommand, typeof(RelayCommand));
            Assert.IsFalse(qviViewModel.AddCommand.CanExecute(null));

            Assert.IsNotNull(qviViewModel.PreviewViewModel);
            Assert.IsInstanceOfType(qviViewModel.PreviewViewModel, typeof(PreviewViewModel));
            Assert.AreEqual(Visibility.Collapsed, qviViewModel.PreviewViewModel.InputsVisibility);
        }
    }
}
