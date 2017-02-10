using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;

using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;
// ReSharper disable MethodSupportsCancellation
// ReSharper disable ConvertToConstant.Local
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageNewServerViewModelTests
    {
        #region Fields

        private Mock<IManageServerSourceModel> _updateManagerMock;
        private Mock<IEventAggregator> _aggregatorMock;
        private Mock<IAsyncWorker> _asyncWorkerMock;
        private Mock<IExternalProcessExecutor> _executorMock;
        private Mock<IServerSource> _serverSourceMock;
        private Mock<IRequestServiceNameViewModel> _requestServiceNameViewModelMock;
        private Task<IRequestServiceNameViewModel> _requestServiceNameViewModelTask;

        private List<string> _changedProperties;
        private ManageNewServerViewModel _target;

        private List<string> _changedPropertiesSource;
        private ManageNewServerViewModel _targetSource;

        private List<string> _changedPropertiesRequestServiceViewModel;
        private ManageNewServerViewModel _targetRequestServiceViewModel;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateManagerMock = new Mock<IManageServerSourceModel>();
            _aggregatorMock = new Mock<IEventAggregator>();
            _asyncWorkerMock = new Mock<IAsyncWorker>();
            _executorMock = new Mock<IExternalProcessExecutor>();
            _serverSourceMock = new Mock<IServerSource>();
            _serverSourceMock.Setup(it => it.Name).Returns("someService");
            _requestServiceNameViewModelMock = new Mock<IRequestServiceNameViewModel>();
            _requestServiceNameViewModelTask = Task.FromResult(_requestServiceNameViewModelMock.Object);
            _serverSourceMock.SetupGet(it => it.Address).Returns("https://ddkksfsw:3143");
            _asyncWorkerMock.Setup(
                it =>
                it.Start(
                    It.IsAny<Action>(),
                    It.IsAny<Action>(),
                    It.IsAny<CancellationTokenSource>(),
                    It.IsAny<Action<Exception>>()))
                .Callback<Action, Action, CancellationTokenSource, Action<Exception>>(
                    (progress, success, token, errorAction) =>
                        {
                            try
                            {
                                progress();
                                success();
                            }
                            catch (Exception ex)
                            {
                                errorAction(ex);
                            }
                        });

            _updateManagerMock.Setup(model => model.FetchSource(It.IsAny<Guid>()))
              .Returns(_serverSourceMock.Object);
            _asyncWorkerMock.Setup(worker =>
                                   worker.Start(
                                            It.IsAny<Func<IServerSource>>(),
                                            It.IsAny<Action<IServerSource>>()))
                            .Callback<Func<IServerSource>, Action<IServerSource>>((func, action) =>
                            {
                                var dbSource = func.Invoke();
                                action(dbSource);
                            });
            _updateManagerMock.Setup(it => it.GetComputerNames())
                .Returns(new List<string> { "computerName1", "computerName2" });
            _asyncWorkerMock.Setup(
                it =>
                it.Start(
                    It.IsAny<Func<List<ComputerName>>>(),
                    It.IsAny<Action<List<ComputerName>>>(),
                    It.IsAny<Action<Exception>>()))
                .Callback<Func<List<ComputerName>>, Action<List<ComputerName>>, Action<Exception>>(
                    (progress, success, errorAction) =>
                        {
                            try
                            {
                                success(progress());
                            }
                            catch (Exception ex)
                            {
                                errorAction(ex);
                            }
                        });

            _changedProperties = new List<string>();
            _target = new ManageNewServerViewModel(
                _updateManagerMock.Object,
                _aggregatorMock.Object,
                _asyncWorkerMock.Object,
                _executorMock.Object);
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };

            _changedPropertiesSource = new List<string>();
            _targetSource = new ManageNewServerViewModel(
                _updateManagerMock.Object,
                _aggregatorMock.Object,
                _serverSourceMock.Object,
                _asyncWorkerMock.Object,
                _executorMock.Object);
            _targetSource.PropertyChanged += (sender, args) => { _changedPropertiesSource.Add(args.PropertyName); };

            _changedPropertiesRequestServiceViewModel = new List<string>();
            _targetRequestServiceViewModel = new ManageNewServerViewModel(
                _updateManagerMock.Object,
                _requestServiceNameViewModelTask,
                _aggregatorMock.Object,
                _asyncWorkerMock.Object,
                _executorMock.Object);
            _targetRequestServiceViewModel.PropertyChanged +=
                (sender, args) => { _changedPropertiesRequestServiceViewModel.Add(args.PropertyName); };
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManageNewServerViewModelExecutorNull()
        {
            new ManageNewServerViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _asyncWorkerMock.Object, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManageNewServerViewModelAsyncWorkerNull()
        {
            new ManageNewServerViewModel(_updateManagerMock.Object, _aggregatorMock.Object, null, _executorMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManageNewServerViewModelUpdateManagerNull()
        {
            new ManageNewServerViewModel(null, _aggregatorMock.Object, _asyncWorkerMock.Object, _executorMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManageNewServerViewModelAggregatorNull()
        {
            new ManageNewServerViewModel(_updateManagerMock.Object, null, _asyncWorkerMock.Object, _executorMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManageNewServerViewModelSourceNull()
        {
            new ManageNewServerViewModel(_updateManagerMock.Object, _aggregatorMock.Object, null, _asyncWorkerMock.Object, _executorMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManageNewServerViewModelRequestServiceViewModelNull()
        {
            new ManageNewServerViewModel(_updateManagerMock.Object, null, _aggregatorMock.Object, _asyncWorkerMock.Object, _executorMock.Object);
        }

        #endregion Test construction

        #region Test commands

        [TestMethod]
        public void TestCancelTestCommandCanExecuteFalse()
        {
            //arrange
            _target.Testing = false;

            //act
            var result = _target.CancelTestCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCancelTestCommandCanExecute()
        {
            //arrange
            _target.Testing = true;

            //act
            var result = _target.CancelTestCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCancelServerSourceTestCommandExecute()
        {
            //arrange
            Task task = null;
            var isCancelled = false;
            _asyncWorkerMock.Setup(
                it =>
                it.Start(
                    It.IsAny<Action>(),
                    It.IsAny<Action>(),
                    It.IsAny<CancellationTokenSource>(),
                    It.IsAny<Action<Exception>>()))
                .Callback<Action, Action, CancellationTokenSource, Action<Exception>>(
                    (progress, success, token, errorAction) =>
                        {
                            task = Task.Factory.StartNew(
                                () =>
                                    {
                                        while (!token.IsCancellationRequested) ;
                                        isCancelled = true;
                                    });
                        });
            _target.Protocol = "http";
            _target.SelectedPort = "3412";
            _target.ServerName = new ComputerName { Name = "localhost" };
            _target.Address = "http://localhost/";
            _target.TestCommand.Execute(null);

            //act
            _target.CancelTestCommand.Execute(null);
            task.Wait();

            //assert
            Assert.IsTrue(isCancelled);
        }

        [TestMethod]
        public void TestTestCommandCanExecute()
        {
            //arrange
            _target.Testing = false;
            _target.Address = "someAddress";
            _target.AuthenticationType = AuthenticationType.User;
            _target.UserName = "someUserName";
            _target.Password = "somePassword";

            //act
            var result = _target.TestCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestTestCommandExecuteFailed()
        {
            //arrange
            _target.Address = null;

            //act
            _target.TestCommand.Execute(null);

            //assert
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestTestCommandExecutePass()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedAddress = "http://somecomputer:8080";
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";
            var expectedName = "someExpectedName";

            _target.AuthenticationType = expectedAuthenticationType;
            _target.Password = expectedPassword;
            _target.UserName = expectedUserName;
            _target.ResourceName = expectedName;
            _target.ServerName = new ComputerName() { Name = "somecomputer" };
            _target.Protocol = "http";
            _target.SelectedPort = "8080";

            _target.Address = "http://localhost/";

            //act
            _target.TestCommand.Execute(null);

            //assert
            Assert.AreEqual("Passed", _target.TestMessage);
            Assert.IsTrue(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
            _updateManagerMock.Verify(
                it =>
                it.TestConnection(
                    It.Is<IServerSource>(
                        src =>
                        src.AuthenticationType == expectedAuthenticationType && src.Address == expectedAddress
                        && src.Password == expectedPassword && src.UserName == expectedUserName
                        && src.Name == expectedName && src.ID != Guid.Empty)));
        }

        [TestMethod]
        public void TestTestCommandExecutePassWorkerFailed()
        {
            //arrange
            _target.Address = "http://localhost/";
            _asyncWorkerMock.Setup(
                it =>
                it.Start(
                    It.IsAny<Action>(),
                    It.IsAny<Action>(),
                    It.IsAny<CancellationTokenSource>(),
                    It.IsAny<Action<Exception>>()))
                .Callback<Action, Action, CancellationTokenSource, Action<Exception>>(
                    (progress, success, token, errorAction) =>
                    {
                        errorAction(null);
                    });
            _target.Protocol = "http";
            _target.SelectedPort = "3412";
            _target.ServerName = new ComputerName { Name = "localhost" };
            //act
            _target.TestCommand.Execute(null);

            //assert
            Assert.AreEqual("Failed", _target.TestMessage);
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestTestCommandExecutePassWorkerFailedException()
        {
            //arrange
            var expectedExceptionMessage = "Exception: someExceptionMessage";
            _asyncWorkerMock.Setup(
                it =>
                it.Start(
                    It.IsAny<Action>(),
                    It.IsAny<Action>(),
                    It.IsAny<CancellationTokenSource>(),
                    It.IsAny<Action<Exception>>()))
                .Callback<Action, Action, CancellationTokenSource, Action<Exception>>(
                    (progress, success, token, errorAction) =>
                    {
                        errorAction(new Exception(expectedExceptionMessage));
                    });
            _target.Protocol = "http";
            _target.SelectedPort = "3412";
            _target.ServerName = new ComputerName { Name = "localhost" };
            _target.Address = "http://localhost/";

            //act
            _target.TestCommand.Execute(null);

            //assert
            Assert.AreEqual("Exception: " + expectedExceptionMessage, _target.TestMessage);
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestOkCommandCanExecute()
        {
            //arrange
            _target.TestPassed = true;

            //act
            var result = _target.OkCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestOkCommandExecute_serverSourceNotNull()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedAddress = "http://somecomputer:8080";
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";
            var expectedName = "someExpectedName";

            var sourceName = "someSourceName";

            var expectedHeaderText = sourceName;
            var expectedHeader = sourceName + " *";

            _serverSourceMock.Setup(it => it.Name).Returns(sourceName);

            _targetSource.AuthenticationType = expectedAuthenticationType;
            _targetSource.Password = expectedPassword;
            _targetSource.UserName = expectedUserName;
            _targetSource.ResourceName = expectedName;
            _targetSource.ServerName = new ComputerName() { Name = "somecomputer" };
            _targetSource.Protocol = "http";
            _targetSource.SelectedPort = "8080";

            //act
            _targetSource.OkCommand.Execute(null);

            //assert
            Assert.AreSame(_serverSourceMock.Object, _targetSource.Item);
            _serverSourceMock.VerifySet(it => it.AuthenticationType = expectedAuthenticationType);
            _serverSourceMock.VerifySet(it => it.Address = expectedAddress);
            _serverSourceMock.VerifySet(it => it.Password = expectedPassword);
            _serverSourceMock.VerifySet(it => it.UserName = expectedUserName);
            _updateManagerMock.Verify(it => it.Save(_serverSourceMock.Object));
            Assert.AreEqual(expectedHeaderText, _targetSource.HeaderText);
            Assert.AreEqual(expectedHeader, _targetSource.Header);
        }

        [TestMethod]
        public void TestOkCommandExecute_serverSourceNull()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedAddress = "http://somecomputer:8080";
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";
            var expectedName = "someExpectedName";
            var expectedHeaderText = expectedName;
            var expectedHeader = expectedName + " *";
            var expectedPath = "somePath";
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(expectedPath, expectedName));
            _targetRequestServiceViewModel.AuthenticationType = expectedAuthenticationType;
            _targetRequestServiceViewModel.Password = expectedPassword;
            _targetRequestServiceViewModel.UserName = expectedUserName;
            _targetRequestServiceViewModel.ServerName = new ComputerName { Name = "somecomputer" };
            _targetRequestServiceViewModel.Protocol = "http";
            _targetRequestServiceViewModel.SelectedPort = "8080";
            _targetRequestServiceViewModel.SelectedGuid = Guid.NewGuid();
            //act
            _targetRequestServiceViewModel.OkCommand.Execute(null);

            //assert
            _requestServiceNameViewModelMock.Verify(it => it.ShowSaveDialog());
            Assert.IsInstanceOfType(_targetRequestServiceViewModel.Item, typeof(ServerSource));
            _updateManagerMock.Verify(it => it.Save(_targetRequestServiceViewModel.Item));
            Assert.AreEqual(expectedAuthenticationType, _targetRequestServiceViewModel.Item.AuthenticationType);
            Assert.AreEqual(expectedAddress, _targetRequestServiceViewModel.Item.Address);
            Assert.AreEqual(expectedPassword, _targetRequestServiceViewModel.Item.Password);
            Assert.AreEqual(expectedName, _targetRequestServiceViewModel.ResourceName);
            Assert.AreEqual(expectedName, _targetRequestServiceViewModel.Item.Name);
            Assert.AreEqual(expectedPath, _targetRequestServiceViewModel.Item.ResourcePath);
            Assert.AreNotEqual(Guid.Empty, _targetRequestServiceViewModel.Item.ID);

            Assert.AreEqual(expectedHeaderText, _targetRequestServiceViewModel.HeaderText);
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        public void TestName()
        {
            //arrange
            var expectedValue = "someName";

            //act
            _target.Name = expectedValue;
            var actualValue = _target.Name;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(expectedValue, _target.ResourceName);
        }

        [TestMethod]
        public void TestServerName()
        {
            //arrange
            var expectedValue = new ComputerName() { Name = "someName" };
            _target.Protocol = "http";
            _target.SelectedPort = "3142";
            _changedProperties.Clear();

            //act
            _target.ServerName = expectedValue;
            var actualValue = _target.ServerName;

            //assert
            Assert.AreSame(expectedValue, actualValue);
            Assert.AreEqual("http://someName:3142", _target.Address);
            Assert.IsTrue(_changedProperties.Contains("ServerName"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestComputerNames()
        {
            //arrange
            var expectedValue = new List<ComputerName>();
            _changedProperties.Clear();

            //act
            _target.ComputerNames = expectedValue;
            var actualValue = _target.ComputerNames;

            //assert
            Assert.AreSame(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("ComputerNames"));
        }

        [TestMethod]
        public void TestHeaderText()
        {
            //arrange
            var expectedValue = "someValue";
            _changedProperties.Clear();

            //act
            _target.HeaderText = expectedValue;
            var actualValue = _target.HeaderText;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("HeaderText"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
        }

        [TestMethod]
        public void TestAddress()
        {
            //arrange
            var expectedValue = "someValue";
            _changedProperties.Clear();

            //act
            _target.Address = expectedValue;
            var actualValue = _target.Address;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("Address"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestAuthenticationType()
        {
            //arrange
            var expectedValue = AuthenticationType.Anonymous;
            _changedProperties.Clear();

            //act
            _target.AuthenticationType = expectedValue;
            var actualValue = _target.AuthenticationType;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("AuthenticationType"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("UserAuthenticationSelected"));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestUserName()
        {
            //arrange
            var expectedValue = "userValue";
            _changedProperties.Clear();

            //act
            _target.UserName = expectedValue;
            var actualValue = _target.UserName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("UserName"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestPassword()
        {
            //arrange
            var expectedValue = "userValue";
            _changedProperties.Clear();

            //act
            _target.Password = expectedValue;
            var actualValue = _target.Password;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("Password"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestResourceName()
        {
            //arrange
            var expectedValue = "userValue";
            _changedProperties.Clear();

            //act
            _target.ResourceName = expectedValue;
            var actualValue = _target.ResourceName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains(_target.ResourceName));
        }

        [TestMethod]
        public void TestTestPassed()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.TestPassed = expectedValue;
            var actualValue = _target.TestPassed;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("TestPassed"));
        }

        [TestMethod]
        public void TestTestFailed()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.TestFailed = expectedValue;
            var actualValue = _target.TestFailed;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("TestFailed"));
        }

        [TestMethod]
        public void TestTesting()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.Testing = expectedValue;
            var actualValue = _target.Testing;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("Testing"));
        }

        [TestMethod]
        public void TestSelectedPort()
        {
            //arrange
            var expectedValue = "someValue";
            _changedProperties.Clear();

            //act
            _target.SelectedPort = expectedValue;
            var actualValue = _target.SelectedPort;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("SelectedPort"));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestProtocol()
        {
            //arrange
            var expectedValue = "someValue";
            _changedProperties.Clear();

            //act
            _target.Protocol = expectedValue;
            var actualValue = _target.Protocol;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains(_target.Protocol));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestProtocolCorrect3143()
        {
            //arrange
            var expectedValue = "https";
            _target.SelectedPort = "3142";
            _target.Protocol = "";
            _changedProperties.Clear();

            //act
            _target.Protocol = expectedValue;
            var actualValue = _target.Protocol;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains(_target.Protocol));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.Testing);
            Assert.AreEqual("3143", _target.SelectedPort);
        }

        [TestMethod]
        public void TestProtocolCorrect3142()
        {
            //arrange
            var expectedValue = "http";
            _target.SelectedPort = "3143";
            _changedProperties.Clear();

            //act
            _target.Protocol = expectedValue;
            var actualValue = _target.Protocol;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains(_target.Protocol));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
            Assert.AreEqual("3142", _target.SelectedPort);
        }

        [TestMethod]
        public void TestUserAuthenticationSelected()
        {
            //arrange
            _target.AuthenticationType = AuthenticationType.User;

            //act
            var actualValue = _target.UserAuthenticationSelected;

            //assert
            Assert.IsTrue(actualValue);
        }

        [TestMethod]
        public void TestUserAuthenticationSelectedFalse()
        {
            //arrange
            _target.AuthenticationType = AuthenticationType.Public;

            //act
            var actualValue = _target.UserAuthenticationSelected;

            //assert
            Assert.IsFalse(actualValue);
        }

        [TestMethod]
        public void TestEmptyServerName()
        {
            //arrange
            var expectedValue = "someName";
            _target.Protocol = "http";
            _target.SelectedPort = "3142";
            _target.ServerName = new ComputerName() { Name = "someName1" };
            _changedProperties.Clear();

            //act
            _target.EmptyServerName = expectedValue;
            var actualValue = _target.EmptyServerName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(expectedValue, _target.ServerName.Name);
            Assert.AreEqual("http://someName:3142", _target.Address);
            Assert.IsTrue(_changedProperties.Contains("EmptyServerName"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestEmptyServerNameNullServerName()
        {
            //arrange
            var expectedValue = "someName";
            _target.Protocol = "http";
            _target.SelectedPort = "3142";
            _target.EmptyServerName = " ";
            _changedProperties.Clear();

            //act
            _target.EmptyServerName = expectedValue;
            var actualValue = _target.EmptyServerName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(expectedValue, _target.ServerName.Name);
            Assert.AreEqual("http://someName:3142", _target.Address);
            Assert.IsTrue(_changedProperties.Contains("EmptyServerName"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        public void TestToString()
        {
            //arrange
            var expectedResult = "someResult";
            _target.HeaderText = expectedResult;

            //act
            var result = _target.ToString();

            //assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            var mainViewModelMock = new Mock<IMainViewModel>();
            var helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);
            var helpText = "someText";

            //act
            _target.UpdateHelpDescriptor(helpText);

            //assert
            helpViewModelMock.Verify(it => it.UpdateHelpText(helpText));
        }

        [TestMethod]
        public void TestSave_Given_ValidSource_Should_Save()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";
            var expectedName = "someExpectedName";
            var sourceName = "someSourceName";

            _serverSourceMock.Setup(it => it.Name).Returns(sourceName);

            _targetSource.AuthenticationType = expectedAuthenticationType;
            _targetSource.Password = expectedPassword;
            _targetSource.UserName = expectedUserName;
            _targetSource.ResourceName = expectedName;
            _targetSource.ServerName = new ComputerName() { Name = "somecomputer" };
            _targetSource.Protocol = "http";
            _targetSource.SelectedPort = "8080";

            //act
            _targetSource.Save();
        }

        [TestMethod]
        public void TesSave_GivenNoServerSource_Should_Save()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedAddress = "http://somecomputer:8080";
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";
            var expectedName = "someExpectedName";
            var expectedHeaderText = expectedName;
            var expectedHeader = expectedName + " *";
            var expectedPath = "somePath";
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(expectedPath, expectedName));
            _targetRequestServiceViewModel.AuthenticationType = expectedAuthenticationType;
            _targetRequestServiceViewModel.Password = expectedPassword;
            _targetRequestServiceViewModel.UserName = expectedUserName;
            _targetRequestServiceViewModel.ServerName = new ComputerName() { Name = "somecomputer" };
            _targetRequestServiceViewModel.Protocol = "http";
            _targetRequestServiceViewModel.SelectedPort = "8080";
            _targetRequestServiceViewModel.SelectedGuid = Guid.NewGuid();

            //act
            _targetRequestServiceViewModel.Save();

            //assert
            _requestServiceNameViewModelMock.Verify(it => it.ShowSaveDialog());
            Assert.IsInstanceOfType(_targetRequestServiceViewModel.Item, typeof(ServerSource));
            _updateManagerMock.Verify(it => it.Save(_targetRequestServiceViewModel.Item));
            Assert.AreEqual(expectedAuthenticationType, _targetRequestServiceViewModel.Item.AuthenticationType);
            Assert.AreEqual(expectedAddress, _targetRequestServiceViewModel.Item.Address);
            Assert.AreEqual(expectedPassword, _targetRequestServiceViewModel.Item.Password);
            Assert.AreEqual(expectedName, _targetRequestServiceViewModel.ResourceName);
            Assert.AreEqual(expectedName, _targetRequestServiceViewModel.Item.Name);
            Assert.AreEqual(expectedPath, _targetRequestServiceViewModel.Item.ResourcePath);
            Assert.AreNotEqual(Guid.Empty, _targetRequestServiceViewModel.Item.ID);
            _updateManagerMock.Verify(it => it.Save(It.IsAny<IServerSource>()));
            Assert.AreEqual(expectedHeaderText, _targetRequestServiceViewModel.HeaderText);
        }


        [TestMethod]
        public void OnCreation_GivenAddress_ShouldSetCorrectBindingValues()
        {
            //---------------Set up test pack-------------------
            var expectedAddress = "http://example.com:8080";
            var sourceId = Guid.NewGuid();
            var serverSourceModel = new Mock<IManageServerSourceModel>();
            var evtAggregator = new Mock<IEventAggregator>();
            var serverSource = new Mock<IServerSource>();
            serverSource.SetupGet(source => source.Address).Returns(expectedAddress);
            serverSource.SetupGet(source => source.ID).Returns(sourceId);
            serverSource.SetupGet(source => source.AuthenticationType).Returns(AuthenticationType.Public);
            serverSourceModel.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                .Returns(serverSource.Object);
            serverSourceModel.Setup(it => it.GetComputerNames())
                .Returns(new List<string> { "computerName1", "computerName2" });
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(worker => worker.Start(It.IsAny<Func<IServerSource>>(), It.IsAny<Action<IServerSource>>()))
             .Callback<Func<IServerSource>, Action<IServerSource>>((func, action) =>
             {
                 var dbSource = func.Invoke();
                 action(dbSource);
             });
            asyncWorker.Setup(it =>it.Start(It.IsAny<Func<List<ComputerName>>>(),
                                            It.IsAny<Action<List<ComputerName>>>(),
                                            It.IsAny<Action<Exception>>()))
                                    .Callback<Func<List<ComputerName>>, Action<List<ComputerName>>, Action<Exception>>(
                                        (progress, success, errorAction) =>
                                        {
                                            try
                                            {
                                                success(progress());
                                            }
                                            catch (Exception ex)
                                            {
                                                errorAction(ex);
                                            }
                                        });
            var executor = new Mock<IExternalProcessExecutor>();
            var vm = new ManageNewServerViewModel(serverSourceModel.Object, evtAggregator.Object, serverSource.Object, asyncWorker.Object, executor.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.Item);
            //---------------Execute Test ----------------------
            Assert.AreEqual("http", vm.Protocol);
            Assert.AreEqual(expectedAddress, vm.Address);
            Assert.IsTrue(string.IsNullOrEmpty(vm.Password));
            Assert.IsTrue(string.IsNullOrEmpty(vm.UserName));
            Assert.AreEqual(AuthenticationType.Public, vm.AuthenticationType);
            Assert.AreEqual("8080", vm.SelectedPort);
        }
        [TestMethod]
        public void TestGetLoadComputerNamesTask()
        {
            //assert          
            Assert.IsNotNull(_targetSource.ComputerNames);
            Assert.IsTrue(_targetSource.ComputerNames.Any(it => it.Name == "computerName1"));
            Assert.IsTrue(_targetSource.ComputerNames.Any(it => it.Name == "computerName2"));
        }

        [TestMethod]
        public void TestGetLoadComputerNamesTaskException()
        {
            //arrange
            var expectedTestMessage = "expectedTestMessage";
            _asyncWorkerMock.Setup(
                it =>
                it.Start(
                    It.IsAny<Func<List<ComputerName>>>(),
                    It.IsAny<Action<List<ComputerName>>>(),
                    It.IsAny<Action<Exception>>()))
                .Callback<Func<List<ComputerName>>, Action<List<ComputerName>>, Action<Exception>>(
                    (progress, success, errorAction) =>
                        {
                            errorAction(new Exception(expectedTestMessage));
                        });

            //act
            _targetSource = new ManageNewServerViewModel(
                _updateManagerMock.Object,
                _aggregatorMock.Object,
                _serverSourceMock.Object,
                _asyncWorkerMock.Object,
                _executorMock.Object);

            //assert          
            Assert.AreEqual(expectedTestMessage, _targetSource.TestMessage);
            Assert.IsFalse(_targetSource.TestPassed);
            Assert.IsFalse(_targetSource.Testing);
        }

        [TestMethod]
        public void TestGetLoadComputerNamesTaskInnerException()
        {
            //arrange
            var expectedTestMessage = "expectedTestMessage";
            _asyncWorkerMock.Setup(
                it =>
                it.Start(
                    It.IsAny<Func<List<ComputerName>>>(),
                    It.IsAny<Action<List<ComputerName>>>(),
                    It.IsAny<Action<Exception>>()))
                .Callback<Func<List<ComputerName>>, Action<List<ComputerName>>, Action<Exception>>(
                    (progress, success, errorAction) =>
                        {
                            errorAction(new Exception("outerException", new Exception(expectedTestMessage)));
                        });

            //act
            _targetSource = new ManageNewServerViewModel(
                _updateManagerMock.Object,
                _aggregatorMock.Object,
                _serverSourceMock.Object,
                _asyncWorkerMock.Object,
                _executorMock.Object);

            //assert          
            Assert.AreEqual(expectedTestMessage, _targetSource.TestMessage);
            Assert.IsFalse(_targetSource.TestPassed);
            Assert.IsFalse(_targetSource.Testing);
        }

        [TestMethod]
        public void TestFromModel()
        {
            //arrange
            var source = new Mock<IServerSource>();
            var expectedResourceName = "someName";
            var expectedAuthenticationType = AuthenticationType.Public;
            var expectedUserName = "userName";
            var expectedServerName = "someServerName";
            var expectedEmptyServerName = expectedServerName;
            var expectedProtocol = "http";
            var expectedSelectedPort = "3142";
            var expectedAddress = expectedProtocol + "://" + expectedServerName + ":" + expectedSelectedPort;
            var expectedPassword = "somePassword";
            var expectedHeader = expectedResourceName + " *";
            _target.ComputerNames = new List<ComputerName>() { new ComputerName() { Name = expectedServerName } };
            source.SetupGet(it => it.Name).Returns(expectedResourceName);
            source.SetupGet(it => it.ServerName).Returns(expectedServerName);
            source.SetupGet(it => it.AuthenticationType).Returns(expectedAuthenticationType);
            source.SetupGet(it => it.UserName).Returns(expectedUserName);
            source.SetupGet(it => it.Address).Returns(expectedAddress);
            source.SetupGet(it => it.Password).Returns(expectedPassword);

            //act
            _target.FromModel(source.Object);

            //assert
            Assert.AreEqual(expectedResourceName, _target.ResourceName);
            Assert.AreEqual(expectedAuthenticationType, _target.AuthenticationType);
            Assert.AreEqual(expectedUserName, _target.UserName);
            Assert.AreSame(_target.ComputerNames[0], _target.ServerName);
            Assert.AreEqual(expectedEmptyServerName, _target.EmptyServerName);
            Assert.AreEqual(expectedProtocol, _target.Protocol);
            Assert.AreEqual(expectedSelectedPort, _target.SelectedPort);
            Assert.AreEqual(expectedAddress, _target.Address);
            Assert.AreEqual(expectedPassword, _target.Password);
        }

        [TestMethod]
        public void TestFromModelhttps3143()
        {
            //arrange
            var source = new Mock<IServerSource>();
            var expectedResourceName = "someName";
            var expectedAuthenticationType = AuthenticationType.Public;
            var expectedUserName = "userName";
            var expectedServerName = "someServerName";
            var expectedEmptyServerName = expectedServerName;
            var expectedProtocol = "https";
            var expectedSelectedPort = "3143";
            var expectedAddress = expectedProtocol + "://" + expectedServerName + ":" + expectedSelectedPort;
            var expectedPassword = "somePassword";
            var expectedHeader = expectedResourceName + " *";
            _target.ComputerNames = new List<ComputerName>() { new ComputerName() { Name = expectedServerName } };
            source.SetupGet(it => it.Name).Returns(expectedResourceName);
            source.SetupGet(it => it.ServerName).Returns(expectedServerName);
            source.SetupGet(it => it.AuthenticationType).Returns(expectedAuthenticationType);
            source.SetupGet(it => it.UserName).Returns(expectedUserName);
            source.SetupGet(it => it.Address).Returns(expectedAddress);
            source.SetupGet(it => it.Password).Returns(expectedPassword);

            //act
            _target.FromModel(source.Object);

            //assert
            Assert.AreEqual(expectedResourceName, _target.ResourceName);
            Assert.AreEqual(expectedAuthenticationType, _target.AuthenticationType);
            Assert.AreEqual(expectedUserName, _target.UserName);
            Assert.AreSame(_target.ComputerNames[0], _target.ServerName);
            Assert.AreEqual(expectedEmptyServerName, _target.EmptyServerName);
            Assert.AreEqual(expectedProtocol, _target.Protocol);
            Assert.AreEqual(expectedSelectedPort, _target.SelectedPort);
            Assert.AreEqual(expectedAddress, _target.Address);
            Assert.AreEqual(expectedPassword, _target.Password);
        }

        [TestMethod]
        public void TestToModelItemNull()
        {
            //arrange
            var gd = Guid.NewGuid();
            _target.Item = null;
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedName = "someName";
            var expectedPassword = "somePassword";
            var expectedServerName = "someServerName";
            var expectedProtocol = "https";
            var expectedSelectedPort = "3143";
            var expectedAddress = expectedProtocol + "://" + expectedServerName + ":" + expectedSelectedPort;
            _target.AuthenticationType = expectedAuthenticationType;
            _target.SelectedPort = expectedSelectedPort;
            _target.ServerName = new ComputerName() { Name = expectedServerName };
            _target.Protocol = expectedProtocol;
            _target.ResourceName = expectedName;
            _target.Password = expectedPassword;
            _target.SelectedGuid = gd;
            //act
            var value = _target.ToModel();

            //assert
            Assert.AreSame(_target.Item, value);
            Assert.AreEqual(expectedAuthenticationType, value.AuthenticationType);
            Assert.AreEqual(expectedAddress, value.Address);
            Assert.AreEqual(expectedPassword, value.Password);
            Assert.AreEqual(expectedName, value.Name);
            Assert.AreNotEqual(Guid.Empty, value.ID);
            Assert.AreEqual(value.ID, gd);
        }

        [TestMethod]
        public void TestToModelItemNotNull()
        {
            //arrange
            var serverSourceMock = new Mock<IServerSource>();
            _target.Item = serverSourceMock.Object;
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedName = "someName";
            var expectedPassword = "somePassword";
            var expectedServerName = "someServerName";
            var expectedProtocol = "https";
            var expectedSelectedPort = "3143";
            var expectedResourcePath = "expectedResourcePath";
            var expectedId = Guid.NewGuid();
            var expectedAddress = expectedProtocol + "://" + expectedServerName + ":" + expectedSelectedPort;
            serverSourceMock.SetupGet(it => it.Name).Returns(expectedName);
            serverSourceMock.SetupGet(it => it.ID).Returns(expectedId);
            serverSourceMock.SetupGet(it => it.ResourcePath).Returns(expectedResourcePath);
            _target.AuthenticationType = expectedAuthenticationType;
            _target.SelectedPort = expectedSelectedPort;
            _target.ServerName = new ComputerName() { Name = expectedServerName };
            _target.Protocol = expectedProtocol;
            _target.Password = expectedPassword;

            //act
            var value = _target.ToModel();

            //assert
            Assert.AreEqual(expectedAuthenticationType, value.AuthenticationType);
            Assert.AreEqual(expectedAddress, value.Address);
            Assert.AreEqual(expectedPassword, value.Password);
            Assert.AreEqual(expectedName, value.Name);
            Assert.AreEqual(expectedId, value.ID);
            Assert.AreEqual(expectedResourcePath, value.ResourcePath);
        }

        [TestMethod]
        public void TestToModelItemNullSourceNotNull()
        {
            //arrange
            _targetSource.Item = null;
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedUserName = "someUserName";
            var expectedPassword = "somePassword";
            var expectedServerName = "someServerName";
            var expectedProtocol = "https";
            var expectedSelectedPort = "3143";
            var expectedAddress = expectedProtocol + "://" + expectedServerName + ":" + expectedSelectedPort;
            _targetSource.AuthenticationType = expectedAuthenticationType;
            _targetSource.SelectedPort = expectedSelectedPort;
            _targetSource.ServerName = new ComputerName() { Name = expectedServerName };
            _targetSource.Protocol = expectedProtocol;
            _targetSource.UserName = expectedUserName;
            _targetSource.Password = expectedPassword;

            //act
            var value = _targetSource.ToModel();

            //assert
            Assert.AreSame(_targetSource.Item, _serverSourceMock.Object);
            Assert.AreSame(_targetSource.Item, value);
            _serverSourceMock.VerifySet(it => it.AuthenticationType = expectedAuthenticationType);
            _serverSourceMock.VerifySet(it => it.Address = expectedAddress);
            _serverSourceMock.VerifySet(it => it.Password = expectedPassword);
            _serverSourceMock.VerifySet(it => it.UserName = expectedUserName);
        }

        [TestMethod]
        public void TestCanTestTestingTrue()
        {
            //arrange
            _target.Testing = true;

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestAddressEmpty()
        {
            //arrange
            _target.Address = "";
            _target.Testing = false;

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestUserAuthenticationEmptyUserName()
        {
            //arrange
            _target.Address = "http://localhost:8080";
            _target.Testing = false;
            _target.AuthenticationType = AuthenticationType.User;
            _target.UserName = "";
            _target.Password = "";

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestUserAuthenticationEmptyPassword()
        {
            //arrange
            _target.Address = "http://localhost:8080";
            _target.Testing = false;
            _target.AuthenticationType = AuthenticationType.User;
            _target.UserName = "";
            _target.Password = "somePassword";

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestUserAuthentication()
        {
            //arrange
            _target.Address = "http://localhost:8080";
            _target.Testing = false;
            _target.AuthenticationType = AuthenticationType.User;
            _target.UserName = "someUserName";
            _target.Password = "somePassword";

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCanTest()
        {
            //arrange
            _target.Address = "http://localhost:8080";
            _target.Testing = false;
            _target.AuthenticationType = AuthenticationType.Public;

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsTrue(result);
        }

        #endregion Test methods
    }
}