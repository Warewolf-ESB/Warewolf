
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Dev2.Activities.Preview;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.Preview
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PreviewViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("PreviewViewModel_Constructor")]
        public void PreviewViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var previewViewModel = new PreviewViewModel();

            //------------Assert Results-------------------------
            Assert.IsNotNull(previewViewModel.Inputs);
            Assert.IsInstanceOfType(previewViewModel.Inputs, typeof(ObservableCollection<ObservablePair<string, string>>));
            Assert.AreEqual(0, previewViewModel.Inputs.Count);

            Assert.IsNotNull(previewViewModel.PreviewCommand);
            Assert.AreEqual(Visibility.Visible, previewViewModel.InputsVisibility);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("PreviewViewModel_Output")]
        public void PreviewViewModel_Implementation_INotifyPropertyChanged()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var previewViewModel = new PreviewViewModel();

            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(previewViewModel, typeof(INotifyPropertyChanged));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("PreviewViewModel_Output")]
        public void PreviewViewModel_Output_Changed_ValueSetAndFiresNotifyPropertyChangedEvent()
        {
            //------------Setup for test--------------------------
            const string OutputValue = "Test Output";

            var actualPropertyName = string.Empty;

            var previewViewModel = new PreviewViewModel();
            previewViewModel.PropertyChanged += (sender, args) => actualPropertyName = args.PropertyName;

            //------------Execute Test---------------------------
            previewViewModel.Output = OutputValue;

            //------------Assert Results-------------------------
            Assert.AreEqual("Output", actualPropertyName);
            Assert.AreEqual(OutputValue, previewViewModel.Output);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("PreviewViewModel_Output")]
        public void PreviewViewModel_PreviewCommand_Executed_FiresPreviewRequestedEvent()
        {
            //------------Setup for test--------------------------
            var previewRequested = false;

            var previewViewModel = new PreviewViewModel();
            previewViewModel.PreviewRequested += (sender, args) =>
            {
                previewRequested = true;
            };

            //------------Execute Test---------------------------
            previewViewModel.PreviewCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(previewRequested);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("PreviewViewModel_Output")]
        public void PreviewViewModel_PreviewCommand_CanExecute_EqualsCanPreview()
        {
            //------------Setup for test--------------------------
            var previewViewModel = new PreviewViewModel();

            //------------Execute Test---------------------------
            previewViewModel.CanPreview = false;
            var canExecuteFalse = previewViewModel.PreviewCommand.CanExecute(null);

            previewViewModel.CanPreview = true;
            var canExecuteTrue = previewViewModel.PreviewCommand.CanExecute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(canExecuteFalse);
            Assert.IsTrue(canExecuteTrue);
        }
    }
}
