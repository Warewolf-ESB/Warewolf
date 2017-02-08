using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageExchangeViewModelTests
    {
        public const string TestOwner = "Bernardt Joubert";
        public const string Category = "Exchange Email";

        public ManageExchangeSourceViewModel GetViewModelWithSource()
        {
            var mock = new Mock<IManageExchangeSourceModel>();
            var exchangeSourceDefinition = new ExchangeSourceDefinition()
            {
                AutoDiscoverUrl = "test",
                Password = "test",
                UserName = "test",
                Path = "test",
                ResourceName = "test exchange",
                Type = enSourceType.ExchangeSource,
            };
            mock.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                .Returns(exchangeSourceDefinition);
            return new ManageExchangeSourceViewModel(mock.Object, new Mock<IEventAggregator>().Object,exchangeSourceDefinition, new SynchronousAsyncWorker())
            {
                Name = "Exchange Source",
                AutoDiscoverUrl = "test Url",
                EmailTo = "test",
                IsActive = true,
                ResourceName = "Exchange Soure",
                UserName = "test user",
                Password = "test password",
                Timeout = 10000,
                HeaderText = "Exchange Source",
                TestPassed = false,
                TestFailed = false,
                TestMessage = "testing",
                EnableSend = true,
                Testing = false,
            };
        }

        public ManageExchangeSourceViewModel GetViewModelWithNoSource()
        {
            return new ManageExchangeSourceViewModel
            {
                Name = "Exchange Source",
                AutoDiscoverUrl = "test Url",
                EmailTo = "test",
                IsActive = true,
                ResourceName = "Exchange Soure",
                UserName = "test user",
                Password = "test password",
                Timeout = 10000,
                HeaderText = "Exchange Source",
                TestPassed = false,
                TestFailed = false,
                TestMessage = "testing",
                EnableSend = true,
                Testing = false,
            };
        }

        public ManageExchangeSourceViewModel GetViewModelFullSource()
        {
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();

            return new ManageExchangeSourceViewModel(new Mock<IManageExchangeSourceModel>().Object, task, new Mock<IEventAggregator>().Object )
            {
                Name = "Exchange Source",
                AutoDiscoverUrl = "test Url",
                EmailTo = "test",
                IsActive = true,
                ResourceName = "Exchange Soure",
                UserName = "test user",
                Password = "test password",
                Timeout = 10000,
                HeaderText = "Exchange Source",
                TestPassed = false,
                TestFailed = false,
                TestMessage = "testing",
                EnableSend = true,
                Testing = false,
            };
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageExchangeModel_Initialize_NewEmpty_Returns_Success()
        {
            var model = new ManageExchangeSourceViewModel();

            Assert.IsNotNull(model);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageExchangeModel_Initialize_Properties_Returns_Success()
        {
            var viewModel = GetViewModelFullSource();

            viewModel.SendCommand = null;
            viewModel.OkCommand = null;

            Assert.IsNotNull(viewModel.Name);
            Assert.IsNotNull(viewModel.AutoDiscoverUrl);
            Assert.IsNotNull(viewModel.AutoDiscoverLabel);
            Assert.IsNotNull(viewModel.EmailTo);
            Assert.IsNotNull(viewModel.EmailFromLabel);
            Assert.IsNotNull(viewModel.EmailToLabel);
            Assert.IsNotNull(viewModel.Password);
            Assert.IsNotNull(viewModel.PasswordLabel);
            Assert.IsNotNull(viewModel.Timeout);
            Assert.AreEqual(viewModel.Timeout, 10000);
            Assert.IsFalse(viewModel.TestPassed);
            Assert.IsFalse(viewModel.TestFailed);
            Assert.IsTrue(viewModel.EnableSend);
            Assert.IsFalse(viewModel.Testing);
            Assert.IsNotNull(viewModel.TestMessage);
            Assert.IsNotNull(viewModel.HeaderText);
            Assert.IsNull(viewModel.SendCommand);
            Assert.IsNull(viewModel.OkCommand);
            Assert.IsNotNull(viewModel.RequestServiceNameViewModel);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageexchangeModel_Initialize_Properties_Returns_Success()
        {
            var viewModel = GetViewModelWithSource();

            viewModel.SendCommand = null;
            viewModel.OkCommand = null;

            Assert.IsNotNull(viewModel.Name);
            Assert.IsNotNull(viewModel.AutoDiscoverUrl);
            Assert.IsNotNull(viewModel.AutoDiscoverLabel);
            Assert.IsNotNull(viewModel.EmailTo);
            Assert.IsNotNull(viewModel.EmailFromLabel);
            Assert.IsNotNull(viewModel.EmailToLabel);
            Assert.IsNotNull(viewModel.Password);
            Assert.IsNotNull(viewModel.PasswordLabel);
            Assert.IsNotNull(viewModel.Timeout);
            Assert.AreEqual(viewModel.Timeout,10000);
            Assert.IsFalse(viewModel.TestPassed);
            Assert.IsFalse(viewModel.TestFailed);
            Assert.IsTrue(viewModel.EnableSend);
            Assert.IsFalse(viewModel.Testing);
            Assert.IsNotNull(viewModel.TestMessage);
            Assert.IsNotNull(viewModel.HeaderText);
            Assert.IsNull(viewModel.SendCommand);
            Assert.IsNull(viewModel.OkCommand);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageexchangeModel_CanTest_False_Returns_Success()
        {
            var viewModel = GetViewModelWithSource();

            viewModel.Testing = false;
            viewModel.AutoDiscoverUrl = string.Empty;
            viewModel.UserName = string.Empty;
            viewModel.Password = string.Empty;

            Assert.IsFalse(viewModel.CanTest());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageexchangeModel_CanTest_True_Returns_Success()
        {
            var viewModel = GetViewModelWithSource();

            viewModel.Testing = false;
            viewModel.TestPassed = true;

            Assert.IsTrue(viewModel.CanTest());
            Assert.IsTrue(viewModel.CanSave());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageexchangeModel_CanTest_True_Returns_False()
        {
            var viewModel = GetViewModelWithSource();

            viewModel.Testing = true;

            Assert.IsFalse(viewModel.CanTest());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageexchangeModel_SaveConnection_Returns_False()
        {
            var viewModel = GetViewModelWithSource();

            viewModel.Testing = true;
            viewModel.Item = new ExchangeSourceDefinition();

           viewModel.Save();

            Assert.IsFalse(viewModel.TestPassed);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageexchangeModel_TestConnectionWithSource_Returns_False()
        {
            var viewModel = GetViewModelWithSource();

            viewModel.Testing = true;
            viewModel.Item = new ExchangeSourceDefinition();
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();

            viewModel.TestConnection();
          
            Assert.IsFalse(viewModel.TestPassed);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageexchangeModel_ToModel_Returns_True()
        {
            var viewModel = GetViewModelWithSource();

            viewModel.Testing = true;
            viewModel.Item = new ExchangeSourceDefinition();

            var model = viewModel.ToModel();

            Assert.IsNotNull(model.AutoDiscoverUrl);
            Assert.IsNotNull(model.UserName);
            Assert.IsNotNull(model.Password);
            Assert.IsNotNull(model.Type);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageexchangeModel_ToModel_ItemIsNull_Returns_True()
        {
            var viewModel = GetViewModelWithSource();

            viewModel.Testing = true;
            viewModel.Item = null;

            var model = viewModel.ToModel();
            viewModel.UpdateHelpDescriptor("tests");

            Assert.IsNotNull(model.AutoDiscoverUrl);
            Assert.IsNotNull(model.UserName);
            Assert.IsNotNull(model.Password);
            Assert.IsNotNull(model.Type);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageexchangeModel_ToSource_Returns_True()
        {
            var viewModel = GetViewModelWithSource();

            var model = viewModel.ToSource();

            Assert.IsNotNull(model.AutoDiscoverUrl);
            Assert.IsNotNull(model.UserName);
            Assert.IsNotNull(model.Password);
            Assert.IsNotNull(model.Type);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void ManageexchangeModel_ToSource_SourceNull_Returns_True()
        {
            var viewModel = GetViewModelWithNoSource();

            var model = viewModel.ToSource();

            Assert.IsNotNull(model.AutoDiscoverUrl);
            Assert.IsNotNull(model.UserName);
            Assert.IsNotNull(model.Password);
            Assert.IsNotNull(model.Type);
        }
    }
}
