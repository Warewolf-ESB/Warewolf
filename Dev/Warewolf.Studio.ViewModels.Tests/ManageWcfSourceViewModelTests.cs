﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Common.Interfaces.Core;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ManageWcfSourceViewModelTests
    {
        public const string TestOwner = "Bernardt Joubert";
        public const string Category = "Exchange Email";

        public ManageWcfSourceViewModel GetModel()
        {
            var sourceModel = new WcfServiceSourceDefinition()
            {
                Id = Guid.NewGuid(),
                Name = "TestWcf",
                EndpointUrl = "http/test/com"
            };

            return new ManageWcfSourceViewModel(UpdateManager, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), sourceModel, new Mock<IAsyncWorker>().Object, new Mock<IEnvironmentModel>().Object);
        }

        private static ManageWcfSourceModel UpdateManager
        {
            get
            {
                var manager = new ManageWcfSourceModel(new Mock<IStudioUpdateManager>().Object, "") { ServerName = "Test" };
                var serverName = manager.ServerName;

                return manager;
            }
        }

        public async Task<IRequestServiceNameViewModel> GetRequestModel()
        {
            return await RequestServiceNameViewModel.CreateAsync(new EnvironmentViewModel(new Mock<IServer>().Object, new Mock<IShellViewModel>().Object), "test", "test");
        }
        public ManageWcfSourceViewModel TestModel()
        {
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            mockRequestServiceNameViewModel.Setup(m => m.ResourceName).Returns(new ResourceName("", "test"));
            mockRequestServiceNameViewModel.Setup(m => m.ShowSaveDialog()).Returns(MessageBoxResult.OK);


            task.Start();

            return new ManageWcfSourceViewModel(new ManageWcfSourceModel(new Mock<IStudioUpdateManager>().Object, ""),task, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), new Mock<IAsyncWorker>().Object, new Mock<IEnvironmentModel>().Object);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_InstantiateNewModel_Returns_Success()
        {
            var model = GetModel();
            model.UpdateHelpDescriptor("Test");

            Assert.IsNotNull(model);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_InstantiateNewTestModel_Returns_Success()
        {
            var model = TestModel();

            Assert.IsNotNull(model);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_InitialIzeProperties_Returns_Success()
        {
            var model = GetModel();

            model.ResourceName = "Test";
            model.RefreshCommand = null;
            model.DispatcherAction = null;
            model.Path = "test";
            model.Name = " test";
            model.HeaderText = "Testwcf";
            model.CancelTestCommand = null;
            

            Assert.IsNull(model.RefreshCommand);
            Assert.IsNull(model.CancelTestCommand);
            Assert.IsNotNull(model.ResourceName);
            Assert.IsNotNull(model.Name);
            Assert.IsNotNull(model.AsyncWorker);
            Assert.IsNull(model.DispatcherAction);
            Assert.IsNotNull(model.Path);
            Assert.IsFalse(model.TestPassed);
            Assert.IsNotNull(model.Name);
            Assert.IsNotNull(model.HeaderText);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_InitialIzeTestProperties_Returns_Success()
        {
            var model = GetModel();
            model.TestMessage = "Testing";
            model.Testing = true;
            model.EndpointUrl = "test";
           
            Assert.IsNotNull(model.EndpointUrl);
            Assert.IsNotNull(model.TestMessage);
            Assert.IsFalse(model.TestFailed);
            Assert.IsFalse(model.CanTest());
            Assert.IsTrue(model.Testing);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_InitialIzeTestNullEndpointUrl_Returns_Success()
        {
            var model = GetModel();
            model.TestMessage = "Testing";
            model.Testing = false;
            model.EndpointUrl = null;
            model.TestFailed = false;

            Assert.IsNull(model.EndpointUrl);
            Assert.IsNotNull(model.TestMessage);
            Assert.IsFalse(model.TestFailed);
            Assert.IsFalse(model.CanTest());
            Assert.IsFalse(model.Testing);
            Assert.IsFalse(model.TestFailed);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_InitialIzeTestCanTestTrue_Returns_Success()
        {
            var model = GetModel();
            model.TestMessage = "Testing";
            model.Testing = false;

            model.TestFailed = false;

            Assert.IsNotNull(model.EndpointUrl);
            Assert.IsNotNull(model.TestMessage);
            Assert.IsFalse(model.TestFailed);
            Assert.IsTrue(model.CanTest());
            Assert.IsFalse(model.Testing);
            Assert.IsFalse(model.TestFailed);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_CancelTest_Returns_Success()
        {
            var model = GetModel();
            model.TestMessage = "Testing";
            model.Testing = false;

            
            Assert.IsFalse(model.CanCancelTest());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_TestConnection_Returns_Success()
        {
            var model = GetModel();
           model.TestCommand.Execute(null);

            Assert.IsFalse(model.CanCancelTest());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_CancelTestCommand_Returns_Success()
        {
            var model = GetModel();
            model.CancelTestCommand.Execute(null);

            Assert.IsFalse(model.CanCancelTest());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_ToNewSource_Returns_Success()
        {
            var model = GetModel();
            model.ToNewSource();

            Assert.IsFalse(model.CanCancelTest());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_ToModel_Returns_Success()
        {
            var model = GetModel();
            model.ToModel();

            Assert.IsFalse(model.CanCancelTest());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_ToModelItemNotNull_Returns_Success()
        {
            var model = GetModel();
            model.Item = new WcfServiceSourceDefinition();
            model.ToModel();

            Assert.IsFalse(model.CanCancelTest());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_SaveConnection_Returns_Success()
        {
            var model = TestModel();


            model.Item = new WcfServiceSourceDefinition()
            {
                Name = "Test",

            };
            model.SaveCommand.Execute(null);
            model.Save();

            Assert.IsFalse(model.CanCancelTest());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void WcfSource_ToSourceNullResource_Returns_Success()
        {
            var model = TestModel();

            model.ToSource();
            model.TestPassed = true;
            
            Assert.IsNotNull(model.RequestServiceNameViewModel);
            Assert.IsFalse(model.CanCancelTest());
            Assert.IsTrue(model.CanSave());
          
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(NullReferenceException))]
        public void WcfSource_RequestServiceModelThrows_Returns_Exeption()
        {
            var model = GetModel();

            model.ToSource();
            model.TestPassed = true;

            var requestModel = model.RequestServiceNameViewModel;
        }
    }
}
