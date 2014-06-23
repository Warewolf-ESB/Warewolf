using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.CustomControls.Progress;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Dialogs
{
    [TestClass]    
    // ReSharper disable InconsistentNaming
    public class ProgressDialogViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("ProgressDialogViewModel_CTOR")]
        public void ProgressDialogViewModel_CTOR_CancelActionIsNull_Exception()
        {
// ReSharper disable ObjectCreationAsStatement
            new ProgressDialogViewModel(null, () => {}, () => {});
// ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("ProgressDialogViewModel_CTOR")]
        public void ProgressDialogViewModel_CTOR_ShowDialogActionIsNull_Exception()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ProgressDialogViewModel(() => { },null, () => { });
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("ProgressDialogViewModel_CTOR")]
        public void ProgressDialogViewModel_CTOR_ClosegActionIsNull_Exception()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ProgressDialogViewModel(() => { }, () => { }, null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProgressDialogViewModel_CancelCommand")]
        public void ProgressDialogViewModel_CancelCommand_CancelCommandExecuted_CallsCancelAction()
        {
            //------------Setup for test--------------------------
            bool cancelActionCalled = false;
            var vm = new ProgressDialogViewModel(() => { cancelActionCalled = true; }, () => { }, () => { });
            //------------Execute Test---------------------------
            vm.CancelCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(cancelActionCalled);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProgressDialogViewModel_Close")]
        public void ProgressDialogViewModel_Close_Executed_CallsCloseAction()
        {
            //------------Setup for test--------------------------
            var closeActionCalled = false;
            var vm = new ProgressDialogViewModel(() => { }, () => { }, () => { closeActionCalled = true; });
            //------------Execute Test---------------------------
            vm.Close();
            //------------Assert Results-------------------------
            Assert.IsTrue(closeActionCalled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProgressDialogViewModel_Show")]
        public void ProgressDialogViewModel_Show_Executed_CallsCloseAction()
        {
            //------------Setup for test--------------------------
            var showActionCalled = false;
            var vm = new ProgressDialogViewModel(() => { }, () => { showActionCalled = true;}, () => {});
            //------------Execute Test---------------------------
            vm.Show();
            //------------Assert Results-------------------------
            Assert.IsTrue(showActionCalled);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProgressDialogViewModel_StartCancel")]
        public void ProgressDialogViewModel_StartCancel_Exected_SetsSubLabelAndCancelButtonEnabled()
        {
            //------------Setup for test--------------------------
            var vm = new ProgressDialogViewModel(() => { }, () => { }, () => { });
            vm.SubLabel = "Downloading ...";
            vm.IsCancelButtonEnabled = true;
            //------------Execute Test---------------------------
            vm.StartCancel();
            //------------Assert Results-------------------------
            Assert.AreEqual("Please wait while the process is being cancelled...", vm.SubLabel);
            Assert.IsFalse(vm.IsCancelButtonEnabled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProgressDialogViewModel_StatusChanged")]
        public void ProgressDialogViewModel_StatusChanged_Exected_SetsLabelAndProgressValue()
        {
            //------------Setup for test--------------------------
            var vm = new ProgressDialogViewModel(() => { }, () => { }, () => { });
            //------------Execute Test---------------------------
            const long totalBytes = 25895554;
            const int progressPercent = 85;
            vm.Label = "Warewolf.msi downloaded 60% of 25288 KB";
            vm.ProgressValue = 60;
            vm.StatusChanged("Warewolf.msi", progressPercent , totalBytes);
            //------------Assert Results-------------------------
            Assert.AreEqual("Warewolf.msi downloaded 85% of 25288 KB", vm.Label);
            Assert.AreEqual(progressPercent, vm.ProgressValue);
        }
    }
}
