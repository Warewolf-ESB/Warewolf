using System;
using System.Reflection;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DuplicateResourceViewModelTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            var resourceView = new Mock<ICreateDuplicateResourceView>();
            var resourceId = Guid.NewGuid();
            DuplicateResourceViewModel vm = new DuplicateResourceViewModel(resourceView.Object, resourceId);
            Assert.IsNull(vm.NewResourceName);
            Assert.IsFalse(vm.FixReferences);
            Assert.IsFalse(vm.FixReferences);
            var fieldInfo = vm.GetType().GetField("_resourceId", BindingFlags.GetField | BindingFlags.NonPublic| BindingFlags.Instance);

            //---------------Test Result -----------------------
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual(resourceId, fieldInfo.GetValue(vm));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetPropertyValues_ShouldSetCorrectly()
        {
            //---------------Set up test pack-------------------
            var resourceView = new Mock<ICreateDuplicateResourceView>();
            var resourceId = Guid.NewGuid();
            DuplicateResourceViewModel vm = new DuplicateResourceViewModel(resourceView.Object, resourceId);
            Assert.IsNull(vm.NewResourceName);
            Assert.IsFalse(vm.FixReferences);
            var fieldInfo = vm.GetType().GetField("_resourceId", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual(resourceId, fieldInfo.GetValue(vm));
            //---------------Execute Test ----------------------
            vm.FixReferences = true;
            vm.NewResourceName = "NewResourceName";
            //---------------Test Result -----------------------
            Assert.IsNotNull(vm.NewResourceName);
            Assert.IsTrue(vm.FixReferences);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ShowDialog_GivenResourceView_ShouldShowCorrectly()
        {
            //---------------Set up test pack-------------------
            var resourceView = new Mock<ICreateDuplicateResourceView>();
            resourceView.Setup(view => view.ShowView());
            resourceView.SetupProperty(view => view.DataContext);
            var resourceId = Guid.NewGuid();
            DuplicateResourceViewModel vm = new DuplicateResourceViewModel(resourceView.Object, resourceId);

            //---------------Assert Precondition----------------
            Assert.IsNull(vm.NewResourceName);
            Assert.IsFalse(vm.FixReferences);
            //---------------Execute Test ----------------------
            vm.ShowDialog();
            //---------------Test Result -----------------------
            resourceView.Verify(view => view.ShowView());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CancelAndClose_GivenResourceView_ShouldCancelAndCloseCorrectly()
        {
            //---------------Set up test pack-------------------
            var resourceView = new Mock<ICreateDuplicateResourceView>();
            resourceView.Setup(view => view.CloseView());
            var resourceId = Guid.NewGuid();
            DuplicateResourceViewModel vm = new DuplicateResourceViewModel(resourceView.Object, resourceId);

            //---------------Assert Precondition----------------
            Assert.IsNull(vm.NewResourceName);
            Assert.IsFalse(vm.FixReferences);
            //---------------Execute Test ----------------------
            vm.CancelCommand.Execute(null);
            //---------------Test Result -----------------------
            resourceView.Verify(view => view.CloseView());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateDuplicates_GivenCommunicationController_ShouldExecuteCorrectlyAndAddPayloads()
        {
            //---------------Set up test pack-------------------
            var controller = new Mock<ICommunicationController>();
            var con = new Mock<IEnvironmentConnection>();
            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(model => model.Connection).Returns(con.Object);
            controller.Setup(communicationController => communicationController.AddPayloadArgument("ResourceID", It.IsAny<string>()));
            controller.Setup(communicationController => communicationController.AddPayloadArgument("NewResourceName", It.IsAny<string>()));
            controller.Setup(communicationController => communicationController.AddPayloadArgument("FixRefs", It.IsAny<string>()));
            controller.Setup(communicationController => communicationController.ExecuteCommand<ExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>()));
            DuplicateResourceViewModel vm = new DuplicateResourceViewModel(controller.Object);

           
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            try
            {
                vm.CreateCommand.Execute(envModel.Object);
            }
            catch(Exception)
            {
                //
            }
            
            //---------------Test Result -----------------------
            controller.Verify(communicationController => communicationController.AddPayloadArgument("ResourceID", It.IsAny<string>()));
            controller.Verify(communicationController => communicationController.AddPayloadArgument("NewResourceName", It.IsAny<string>()));
            controller.Verify(communicationController => communicationController.AddPayloadArgument("FixRefs", It.IsAny<string>()));
            controller.Verify(communicationController => communicationController.ExecuteCommand<ExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>()));
        }
    }
}
