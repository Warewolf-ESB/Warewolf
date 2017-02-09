using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces.Threading;
using Dev2.Threading;

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageRabbitMQSourceViewModelTests
    {
        #region Fields

        private Mock<IRabbitMQSourceModel> _rabbitMQSourceModel;
        private Mock<IRequestServiceNameViewModel> _requestServiceNameViewModel;
        private Task<IRequestServiceNameViewModel> _requestServiceNameViewModelTask;
        private Mock<IRabbitMQServiceSourceDefinition> _rabbitMQServiceSourceDefinition;
        private List<string> _changedProperties;
        private ManageRabbitMQSourceViewModel _manageRabbitMQSourceViewModelWithTask;
        private ManageRabbitMQSourceViewModel _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition;
        private Mock<IAsyncWorker> _asyncWorkerMock;
        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _rabbitMQSourceModel = new Mock<IRabbitMQSourceModel>();
            _requestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            _rabbitMQServiceSourceDefinition = new Mock<IRabbitMQServiceSourceDefinition>();
            _requestServiceNameViewModelTask = Task.FromResult(_requestServiceNameViewModel.Object);
            _changedProperties = new List<string>();
            _manageRabbitMQSourceViewModelWithTask = new ManageRabbitMQSourceViewModel(_rabbitMQSourceModel.Object, _requestServiceNameViewModelTask);
            _manageRabbitMQSourceViewModelWithTask.PropertyChanged += (sender, e) => { _changedProperties.Add(e.PropertyName); };
            _asyncWorkerMock = new Mock<IAsyncWorker>();
            _rabbitMQSourceModel.Setup(model => model.FetchSource(It.IsAny<Guid>()))
            .Returns(_rabbitMQServiceSourceDefinition.Object);
            _asyncWorkerMock.Setup(worker =>
                                   worker.Start(
                                            It.IsAny<Func<IRabbitMQServiceSourceDefinition>>(),
                                            It.IsAny<Action<IRabbitMQServiceSourceDefinition>>()))
                            .Callback<Func<IRabbitMQServiceSourceDefinition>, Action<IRabbitMQServiceSourceDefinition>>((func, action) =>
                            {
                                var dbSource = func.Invoke();
                                action(dbSource);
                            });
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition = new ManageRabbitMQSourceViewModel(_rabbitMQSourceModel.Object, _rabbitMQServiceSourceDefinition.Object, _asyncWorkerMock.Object);
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageRabbitMQSourceViewModel_Constructor_Null_IRabbitMQSourceModel_ThrowsException()
        {
            //act
            new ManageRabbitMQSourceViewModel(null, _requestServiceNameViewModelTask);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageRabbitMQSourceViewModel_Constructor_Null_IRequestServiceNameViewModelTask_ThrowsException()
        {
            //arrange
            _requestServiceNameViewModelTask = null;

            //act
            new ManageRabbitMQSourceViewModel(_rabbitMQSourceModel.Object, _requestServiceNameViewModelTask);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageRabbitMQSourceViewModel_Constructor_Null_IRabbitMQServiceSourceDefinition_ThrowsException()
        {
            IRabbitMQServiceSourceDefinition rabbitMQServiceSourceDefinition = null;

            //act
            new ManageRabbitMQSourceViewModel(_rabbitMQSourceModel.Object, rabbitMQServiceSourceDefinition, new SynchronousAsyncWorker());
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Constructor")]
        public void TestManageRabbitMQSourceViewModel_Constructor1()
        {
            //act
            new ManageRabbitMQSourceViewModel(_rabbitMQSourceModel.Object, _requestServiceNameViewModelTask);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Constructor")]
        public void TestManageRabbitMQSourceViewModel_Constructor2()
        {
            //act
            new ManageRabbitMQSourceViewModel(_rabbitMQSourceModel.Object, _rabbitMQServiceSourceDefinition.Object, new SynchronousAsyncWorker());
        }

        #endregion Test construction

        #region Test commands

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Commands")]
        public void TestPublishCommand_CanExecute()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.Testing = false;
            _manageRabbitMQSourceViewModelWithTask.HostName = "HostName";
            _manageRabbitMQSourceViewModelWithTask.Port = 123;
            _manageRabbitMQSourceViewModelWithTask.UserName = "UserName";
            _manageRabbitMQSourceViewModelWithTask.Password = "Password";
            _manageRabbitMQSourceViewModelWithTask.VirtualHost = "VirtualHost";

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.PublishCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Commands")]
        public void TestPublishCommand_CanNotExecute_Testing()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.Testing = true;
            _manageRabbitMQSourceViewModelWithTask.HostName = "HostName";
            _manageRabbitMQSourceViewModelWithTask.Port = 123;
            _manageRabbitMQSourceViewModelWithTask.UserName = "UserName";
            _manageRabbitMQSourceViewModelWithTask.Password = "Password";
            _manageRabbitMQSourceViewModelWithTask.VirtualHost = "VirtualHost";

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.PublishCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Commands")]
        public void TestPublishCommand_CanNotExecute_NoHost()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.Testing = false;
            _manageRabbitMQSourceViewModelWithTask.HostName = null;
            _manageRabbitMQSourceViewModelWithTask.Port = 123;
            _manageRabbitMQSourceViewModelWithTask.UserName = "UserName";
            _manageRabbitMQSourceViewModelWithTask.Password = "Password";
            _manageRabbitMQSourceViewModelWithTask.VirtualHost = "VirtualHost";

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.PublishCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Commands")]
        public void TestPublishCommand_Execute()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.Testing = false;
            _manageRabbitMQSourceViewModelWithTask.HostName = "HostName";
            _manageRabbitMQSourceViewModelWithTask.Port = 123;
            _manageRabbitMQSourceViewModelWithTask.UserName = "UserName";
            _manageRabbitMQSourceViewModelWithTask.Password = "Password";
            _manageRabbitMQSourceViewModelWithTask.VirtualHost = "VirtualHost";

            //act
            _manageRabbitMQSourceViewModelWithTask.PublishCommand.Execute(null);
        }

        [TestMethod]
        public void TestPublishCommand_Execute_Exception()
        {
            //arrange
            _rabbitMQSourceModel.Setup(x => x.TestSource(It.IsAny<IRabbitMQServiceSourceDefinition>())).Throws(new Exception());

            //act
            _manageRabbitMQSourceViewModelWithTask.PublishCommand.Execute(null);

            //assert
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithTask.TestPassed);
            Assert.IsTrue(_manageRabbitMQSourceViewModelWithTask.TestFailed);
            Assert.IsNotNull(_manageRabbitMQSourceViewModelWithTask.TestErrorMessage);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Commands")]
        public void TestOkCommand_CanExecute()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.TestPassed = true;

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.OkCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Commands")]
        public void TestOkCommand_Execute()
        {
            //act
            _manageRabbitMQSourceViewModelWithTask.OkCommand.Execute(null);

            //assert
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithTask.TestPassed);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Commands")]
        public void TestOkCommand_Execute_SaveSource()
        {
            //arrange
            string expectedResourceName = "ResourceName";
            string expectedResourcePath = "ResourcePath";
            string expectedHeader = expectedResourceName + " *";
            string expectedHostName = "HostName";
            int expectedPort = 1234;
            string expectedUserName = "UserName";
            string expectedPassword = "Password";
            string expectedVirtualHost = "VirtualHost";

            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.HostName = expectedHostName;
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Port = expectedPort;
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.UserName = expectedUserName;
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Password = expectedPassword;
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.VirtualHost = expectedVirtualHost;
            _rabbitMQServiceSourceDefinition.SetupGet(it => it.ResourcePath).Returns(expectedResourcePath);
            _rabbitMQServiceSourceDefinition.SetupGet(it => it.ResourceName).Returns(expectedResourceName);
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Item = _rabbitMQServiceSourceDefinition.Object;

            //act
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.OkCommand.Execute(null);

            //assert
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.TestPassed);
            Assert.AreSame(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Item, _rabbitMQServiceSourceDefinition.Object);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.HeaderText, expectedResourceName);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Header, expectedHeader);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.HostName, expectedHostName);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Port, expectedPort);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.UserName, expectedUserName);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Password, expectedPassword);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.VirtualHost, expectedVirtualHost);
            _rabbitMQServiceSourceDefinition.VerifySet(x => x.HostName = expectedHostName);
            _rabbitMQServiceSourceDefinition.VerifySet(x => x.Port = expectedPort);
            _rabbitMQServiceSourceDefinition.VerifySet(x => x.UserName = expectedUserName);
            _rabbitMQServiceSourceDefinition.VerifySet(x => x.Password = expectedPassword);
            _rabbitMQServiceSourceDefinition.VerifySet(x => x.VirtualHost = expectedVirtualHost);
            _rabbitMQSourceModel.Verify(x => x.SaveSource(_rabbitMQServiceSourceDefinition.Object));
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestName()
        {
            //arrange
            string expectedValue = "expectedValue";

            //act
            _manageRabbitMQSourceViewModelWithTask.Name = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.Name;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.AreEqual(expectedValue, _manageRabbitMQSourceViewModelWithTask.ResourceName);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestRequestServiceNameViewModel()
        {
            //arrange
            var valueMock = new Mock<IRequestServiceNameViewModel>();
            var expectedValue = Task.FromResult(valueMock.Object);

            //act
            _manageRabbitMQSourceViewModelWithTask.RequestServiceNameViewModel = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.RequestServiceNameViewModel;

            //assert
            Assert.AreSame(expectedValue, value);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestResourceName()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _manageRabbitMQSourceViewModelWithTask.ResourceName = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.ResourceName;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains(expectedValue));
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestHostName()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _manageRabbitMQSourceViewModelWithTask.HostName = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.HostName;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("HostName"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_manageRabbitMQSourceViewModelWithTask.TestErrorMessage));
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithTask.TestPassed);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestPort()
        {
            //arrange
            var expectedValue = 1234;
            _changedProperties.Clear();

            //act
            _manageRabbitMQSourceViewModelWithTask.Port = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.Port;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Port"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_manageRabbitMQSourceViewModelWithTask.TestErrorMessage));
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithTask.TestPassed);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestUserName()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _manageRabbitMQSourceViewModelWithTask.UserName = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.UserName;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("UserName"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_manageRabbitMQSourceViewModelWithTask.TestErrorMessage));
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithTask.TestPassed);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestPassword()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _manageRabbitMQSourceViewModelWithTask.Password = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.Password;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Password"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_manageRabbitMQSourceViewModelWithTask.TestErrorMessage));
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithTask.TestPassed);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestVirtualHost()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _manageRabbitMQSourceViewModelWithTask.VirtualHost = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.VirtualHost;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("VirtualHost"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_manageRabbitMQSourceViewModelWithTask.TestErrorMessage));
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithTask.TestPassed);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestTestPassed()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _manageRabbitMQSourceViewModelWithTask.TestPassed = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.TestPassed;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("TestPassed"));
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestHeaderText()
        {
            //arrange
            var expectedValue = "HeaderText";
            _changedProperties.Clear();

            //act
            _manageRabbitMQSourceViewModelWithTask.HeaderText = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.HeaderText;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("HeaderText"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Properties")]
        public void TestTestFailed()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _manageRabbitMQSourceViewModelWithTask.TestFailed = expectedValue;
            var value = _manageRabbitMQSourceViewModelWithTask.TestFailed;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("TestFailed"));
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Methods")]
        public void TestToModelItemNull()
        {
            //arrange
            string expectedHostName = "HostName";
            int expectedPort = 1234;
            string expectedUserName = "UserName";
            string expectedPassword = "Password";
            string expectedVirtualHost = "VirtualHost";

            _manageRabbitMQSourceViewModelWithTask.HostName = expectedHostName;
            _manageRabbitMQSourceViewModelWithTask.Port = expectedPort;
            _manageRabbitMQSourceViewModelWithTask.UserName = expectedUserName;
            _manageRabbitMQSourceViewModelWithTask.Password = expectedPassword;
            _manageRabbitMQSourceViewModelWithTask.VirtualHost = expectedVirtualHost;
            _manageRabbitMQSourceViewModelWithTask.Item = null;

            //act
            IRabbitMQServiceSourceDefinition result = _manageRabbitMQSourceViewModelWithTask.ToModel();

            //assert
            Assert.IsNotNull(result);
            Assert.AreSame(result, _manageRabbitMQSourceViewModelWithTask.Item);
            Assert.AreEqual(expectedHostName, result.HostName);
            Assert.AreEqual(expectedPort, result.Port);
            Assert.AreEqual(expectedUserName, result.UserName);
            Assert.AreEqual(expectedPassword, result.Password);
            Assert.AreEqual(expectedVirtualHost, result.VirtualHost);
            Assert.AreNotEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Methods")]
        public void TestToModelItemNotNull()
        {
            //arrange
            string expectedHostName = "HostName";
            int expectedPort = 1234;
            string expectedUserName = "UserName";
            string expectedPassword = "Password";
            string expectedVirtualHost = "VirtualHost";

            _manageRabbitMQSourceViewModelWithTask.HostName = expectedHostName;
            _manageRabbitMQSourceViewModelWithTask.Port = expectedPort;
            _manageRabbitMQSourceViewModelWithTask.UserName = expectedUserName;
            _manageRabbitMQSourceViewModelWithTask.Password = expectedPassword;
            _manageRabbitMQSourceViewModelWithTask.VirtualHost = expectedVirtualHost;
            _manageRabbitMQSourceViewModelWithTask.Item = _rabbitMQServiceSourceDefinition.Object;

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.ToModel();

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedHostName, result.HostName);
            Assert.AreEqual(expectedPort, result.Port);
            Assert.AreEqual(expectedUserName, result.UserName);
            Assert.AreEqual(expectedPassword, result.Password);
            Assert.AreEqual(expectedVirtualHost, result.VirtualHost);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Methods")]
        public void TestSaveSource()
        {
            //arrange
            string expectedResourceName = "ResourceName";
            string expectedResourcePath = "ResourcePath";
            string expectedHeader = expectedResourceName + " *";
            string expectedHostName = "HostName";
            int expectedPort = 1234;
            string expectedUserName = "UserName";
            string expectedPassword = "Password";
            string expectedVirtualHost = "VirtualHost";

            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.HostName = expectedHostName;
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Port = expectedPort;
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.UserName = expectedUserName;
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Password = expectedPassword;
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.VirtualHost = expectedVirtualHost;
            _rabbitMQServiceSourceDefinition.SetupGet(it => it.ResourcePath).Returns(expectedResourcePath);
            _rabbitMQServiceSourceDefinition.SetupGet(it => it.ResourceName).Returns(expectedResourceName);
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Item = _rabbitMQServiceSourceDefinition.Object;

            //act
            _manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Save();

            //assert
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.TestPassed);
            Assert.AreSame(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Item, _rabbitMQServiceSourceDefinition.Object);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.HeaderText, expectedResourceName);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Header, expectedHeader);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.HostName, expectedHostName);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Port, expectedPort);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.UserName, expectedUserName);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.Password, expectedPassword);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithRabbitMQServiceSourceDefinition.VirtualHost, expectedVirtualHost);
            _rabbitMQServiceSourceDefinition.VerifySet(x => x.HostName = expectedHostName);
            _rabbitMQServiceSourceDefinition.VerifySet(x => x.Port = expectedPort);
            _rabbitMQServiceSourceDefinition.VerifySet(x => x.UserName = expectedUserName);
            _rabbitMQServiceSourceDefinition.VerifySet(x => x.Password = expectedPassword);
            _rabbitMQServiceSourceDefinition.VerifySet(x => x.VirtualHost = expectedVirtualHost);
            _rabbitMQSourceModel.Verify(x => x.SaveSource(_rabbitMQServiceSourceDefinition.Object));
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Methods")]
        public void TestSaveNewSource()
        {
            //arrange
            string expectedResourceName = "ResourceName";
            string expectedResourcePath = "ResourcePath";

            _requestServiceNameViewModel.Setup(x => x.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModel.SetupGet(it => it.ResourceName).Returns(new ResourceName(expectedResourcePath, expectedResourceName));

            //act
            _manageRabbitMQSourceViewModelWithTask.Save();

            //assert
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithTask.TestPassed);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithTask.HeaderText, expectedResourceName);
            Assert.AreEqual(_manageRabbitMQSourceViewModelWithTask.ResourceName, expectedResourceName);
            _rabbitMQSourceModel.Verify(x => x.SaveSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Methods")]
        public void TestSave()
        {
            //act
            _manageRabbitMQSourceViewModelWithTask.Save();

            // MessageBoxResult
            //assert
            Assert.IsFalse(_manageRabbitMQSourceViewModelWithTask.TestPassed);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("ManageRabbitMQSourceViewModel_Methods")]
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            string helpText = "helpText";
            Mock<IMainViewModel> mainViewModelMock = new Mock<IMainViewModel>();
            Mock<IHelpWindowViewModel> helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);

            //act
            _manageRabbitMQSourceViewModelWithTask.UpdateHelpDescriptor(helpText);

            //assert
            helpViewModelMock.Verify(it => it.UpdateHelpText(helpText));
        }

        [TestMethod]
        public void TestCanSaveTrue()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.TestPassed = true;

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.CanSave();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCanSaveFalse()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.TestPassed = false;

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.CanSave();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestTrue()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.HostName = "HostName";
            _manageRabbitMQSourceViewModelWithTask.UserName = "UserName";
            _manageRabbitMQSourceViewModelWithTask.Password = "Password";

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.CanTest();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCanTestFalseHostNameEmpty()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.HostName = "";
            _manageRabbitMQSourceViewModelWithTask.UserName = "UserName";
            _manageRabbitMQSourceViewModelWithTask.Password = "Password";

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestFalseHostNameUserNameEmpty()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.HostName = "";
            _manageRabbitMQSourceViewModelWithTask.UserName = "";
            _manageRabbitMQSourceViewModelWithTask.Password = "Password";

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestFalseHostNameUserNamePasswordEmpty()
        {
            //arrange
            _manageRabbitMQSourceViewModelWithTask.HostName = "";
            _manageRabbitMQSourceViewModelWithTask.UserName = "";
            _manageRabbitMQSourceViewModelWithTask.Password = "";

            //act
            var result = _manageRabbitMQSourceViewModelWithTask.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        #endregion Test methods
    }
}