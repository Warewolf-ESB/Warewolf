using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class TestFrameworkViewModelTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRenameCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestFrameworkViewModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RenameCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RenameCommand.CanExecute());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveSaveCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestFrameworkViewModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.SaveCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.SaveCommand.CanExecute());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveEnableTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestFrameworkViewModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.EnableTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.EnableTestCommand.CanExecute());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveDisableTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestFrameworkViewModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.DisableTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DisableTestCommand.CanExecute());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveDeleteTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestFrameworkViewModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.DeleteTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DeleteTestCommand.CanExecute());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveDuplicateTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestFrameworkViewModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.DuplicateTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.DuplicateTestCommand.CanExecute());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveRunTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestFrameworkViewModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.RunTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.RunTestCommand.CanExecute());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldHaveWebRunTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestFrameworkViewModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.WebRunTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.WebRunTestCommand.CanExecute());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsNew_ShouldStopTestCommand()
        {
            //---------------Set up test pack-------------------
            var vm = new TestFrameworkViewModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(vm.StopTestCommand);
            //---------------Test Result -----------------------
            Assert.IsFalse(vm.StopTestCommand.CanExecute());
        }
    }
}
