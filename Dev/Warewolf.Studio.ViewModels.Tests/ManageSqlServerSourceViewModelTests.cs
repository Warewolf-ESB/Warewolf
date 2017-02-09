using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageSqlServerSourceViewModelTests
    {
        #region Fields

        private Mock<IManageDatabaseSourceModel> _updateManagerMock;
        private Mock<IEventAggregator> _aggregatorMock;
        private Mock<IAsyncWorker> _asyncWorkerMock;
        private Mock<IDbSource> _dbSourceMock;

        private Mock<IRequestServiceNameViewModel> _requestServiceNameViewMock;
        private Task<IRequestServiceNameViewModel> _requestServiceNameView;
        private List<string> _changedPropertiesAsyncWorker;
        private List<string> _changedPropertiesUpdateManagerAggregatorDbSource;
        private List<string> _changedUpdateManagerRequestServiceName;

        private ManageSqlServerSourceViewModel _targetAsyncWorker;
        private ManageSqlServerSourceViewModel _targetUpdateManagerAggregatorDbSource;
        private ManageSqlServerSourceViewModel _targetUpdateManagerRequestServiceName;


        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _asyncWorkerMock = new Mock<IAsyncWorker>();
            _updateManagerMock = new Mock<IManageDatabaseSourceModel>();
            _aggregatorMock = new Mock<IEventAggregator>();
            _dbSourceMock = new Mock<IDbSource>();
            _requestServiceNameViewMock = new Mock<IRequestServiceNameViewModel>();
            _requestServiceNameView = Task.FromResult(_requestServiceNameViewMock.Object);

            _updateManagerMock.Setup(it => it.GetComputerNames())
                .Returns(new List<string>() { "someName1", "someName2" });

            _dbSourceMock.SetupGet(it => it.Name).Returns("someDbSourceName");
            _asyncWorkerMock.Setup(
                it =>
                it.Start(
                    It.IsAny<Func<List<ComputerName>>>(),
                    It.IsAny<Action<List<ComputerName>>>(),
                    It.IsAny<Action<Exception>>()))
                .Callback<Func<List<ComputerName>>, Action<List<ComputerName>>, Action<Exception>>(
                    (progress, success, fail) =>
                    {
                        try
                        {
                            success(progress());
                        }
                        catch (Exception ex)
                        {
                            fail(ex);
                        }
                    });

            _updateManagerMock.Setup(model => model.FetchDbSource(It.IsAny<Guid>()))
            .Returns(_dbSourceMock.Object);
            _asyncWorkerMock.Setup(worker =>
                                   worker.Start(
                                            It.IsAny<Func<IDbSource>>(),
                                            It.IsAny<Action<IDbSource>>()))
                            .Callback<Func<IDbSource>, Action<IDbSource>>((func, action) =>
                            {
                                var dbSource = func.Invoke();
                                action(dbSource);
                            });
            _targetAsyncWorker = new ManageSqlServerSourceViewModel(_asyncWorkerMock.Object);
            _changedPropertiesAsyncWorker = new List<string>();
            _targetAsyncWorker.PropertyChanged += (sender, args) =>
            {
                _changedPropertiesAsyncWorker.Add(args.PropertyName);
            };


            _targetUpdateManagerAggregatorDbSource = new ManageSqlServerSourceViewModel(
                _updateManagerMock.Object,
                _aggregatorMock.Object,
                _dbSourceMock.Object,
                _asyncWorkerMock.Object);
            _changedPropertiesUpdateManagerAggregatorDbSource = new List<string>();

            _targetUpdateManagerAggregatorDbSource.PropertyChanged += (sender, args) =>
            {
                _changedPropertiesUpdateManagerAggregatorDbSource.Add(args.PropertyName);
            };

            _targetUpdateManagerRequestServiceName = new ManageSqlServerSourceViewModel(
                _updateManagerMock.Object,
                _requestServiceNameView,
                _aggregatorMock.Object,
                _asyncWorkerMock.Object);
            _changedUpdateManagerRequestServiceName = new List<string>();
            _targetUpdateManagerRequestServiceName.PropertyChanged += (sender, args) =>
            {
                _changedUpdateManagerRequestServiceName.Add(args.PropertyName);
            };
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        public void TestManageDatabaseSourceViewModelUpdateManagerThrowsExceptionWithInnerException()
        {
            //arrange
            var expectedExceptionMessage = "someExceptionMessage";
            _updateManagerMock.Setup(it => it.GetComputerNames())
                .Throws(new Exception("someOuterExceptionMessage", new Exception(expectedExceptionMessage)));

            //act
            _targetUpdateManagerRequestServiceName = new ManageSqlServerSourceViewModel(
                 _updateManagerMock.Object,
                 _requestServiceNameView,
                 _aggregatorMock.Object,
                 _asyncWorkerMock.Object);

            //assert
            Assert.IsTrue(_targetUpdateManagerRequestServiceName.TestFailed);
            Assert.IsFalse(_targetUpdateManagerRequestServiceName.TestPassed);
            Assert.AreEqual(expectedExceptionMessage, _targetUpdateManagerRequestServiceName.TestMessage);
        }

        [TestMethod]
        public void TestManageDatabaseSourceViewModelUpdateManagerThrowsException()
        {
            //arrange
            var expectedExceptionMessage = "someExceptionMessage";
            _updateManagerMock.Setup(it => it.GetComputerNames()).Throws(new Exception(expectedExceptionMessage));

            //act
            _targetUpdateManagerRequestServiceName = new ManageSqlServerSourceViewModel(
                 _updateManagerMock.Object,
                 _requestServiceNameView,
                 _aggregatorMock.Object,
                 _asyncWorkerMock.Object);

            //assert
            Assert.IsTrue(_targetUpdateManagerRequestServiceName.TestFailed);
            Assert.IsFalse(_targetUpdateManagerRequestServiceName.TestPassed);
            Assert.AreEqual(expectedExceptionMessage, _targetUpdateManagerRequestServiceName.TestMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageDatabaseSourceViewModelUpdateManagerNull()
        {
            new ManageSqlServerSourceViewModel(
                 null,
                 _requestServiceNameView,
                 _aggregatorMock.Object,
                 _asyncWorkerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageDatabaseSourceViewModelAggregatorNull()
        {
            new ManageSqlServerSourceViewModel(
                 _updateManagerMock.Object,
                 _requestServiceNameView,
                 null,
                 _asyncWorkerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageDatabaseSourceViewModelRequestServiceNameViewModelNull()
        {
            new ManageSqlServerSourceViewModel(
                 _updateManagerMock.Object,
                 null,
                  _aggregatorMock.Object,
                 _asyncWorkerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageDatabaseSourceViewModelDbSourceNull()
        {
            new ManageSqlServerSourceViewModel(
                 _updateManagerMock.Object,
                 _aggregatorMock.Object,
                 null,
                 _asyncWorkerMock.Object);
        }

        #endregion Test construction

        #region Test properties

        [TestMethod]
        public void TestIsEmptyServerNameNonEmptyToString()
        {
            //arrange
            _targetAsyncWorker.ServerName = new ComputerName() { Name = "someName" };

            //act


            //assert
            Assert.AreEqual("someName", _targetAsyncWorker.ServerName.ToString());
        }

        [TestMethod]
        public void TestAuthenticationType_dbSourceNull()
        {
            //arrange
            var expectedValue = AuthenticationType.User;
            _changedPropertiesAsyncWorker.Clear();
            _targetAsyncWorker.DatabaseNames = new List<string>() { "DatabaseNamesItem" };

            //act
            _targetAsyncWorker.AuthenticationType = expectedValue;
            var actualValue = _targetAsyncWorker.AuthenticationType;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.Password));
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.UserName));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("AuthenticationType"));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Header"));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("UserAuthenticationSelected"));
            Assert.IsFalse(_targetAsyncWorker.DatabaseNames.Any());
            Assert.IsFalse(_targetAsyncWorker.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.TestMessage));
            Assert.IsFalse(_targetAsyncWorker.TestFailed);
            Assert.IsFalse(_targetAsyncWorker.Testing);
        }

        [TestMethod]
        public void TestAuthenticationType_dbSourceNotNullPublic()
        {
            //arrange
            var expectedValue = AuthenticationType.Public;
            _changedPropertiesUpdateManagerAggregatorDbSource.Clear();
            _targetUpdateManagerAggregatorDbSource.DatabaseNames = new List<string>() { "DatabaseNamesItem" };

            //act
            _targetUpdateManagerAggregatorDbSource.AuthenticationType = expectedValue;
            var actualValue = _targetUpdateManagerAggregatorDbSource.AuthenticationType;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(string.IsNullOrEmpty(_targetUpdateManagerAggregatorDbSource.Password));
            Assert.IsTrue(string.IsNullOrEmpty(_targetUpdateManagerAggregatorDbSource.UserName));
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("AuthenticationType"));
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("Header"));
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("UserAuthenticationSelected"));
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.DatabaseNames.Any());
            Assert.IsFalse(_targetAsyncWorker.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.TestMessage));
            Assert.IsFalse(_targetAsyncWorker.TestFailed);
            Assert.IsFalse(_targetAsyncWorker.Testing);
        }

        [TestMethod]
        public void TestAuthenticationType_dbSourceNotNullUser()
        {
            //arrange
            var expectedValue = AuthenticationType.User;
            _dbSourceMock.SetupGet(it => it.AuthenticationType).Returns(AuthenticationType.Public);
            _changedPropertiesUpdateManagerAggregatorDbSource.Clear();
            _targetUpdateManagerAggregatorDbSource.DatabaseNames = new List<string>() { "DatabaseNamesItem" };

            //act
            _targetUpdateManagerAggregatorDbSource.AuthenticationType = expectedValue;
            var actualValue = _targetUpdateManagerAggregatorDbSource.AuthenticationType;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(string.IsNullOrEmpty(_targetUpdateManagerAggregatorDbSource.Password));
            Assert.IsTrue(string.IsNullOrEmpty(_targetUpdateManagerAggregatorDbSource.UserName));
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("AuthenticationType"));
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("Header"));
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("UserAuthenticationSelected"));
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.DatabaseNames.Any());
            Assert.IsFalse(_targetAsyncWorker.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.TestMessage));
            Assert.IsFalse(_targetAsyncWorker.TestFailed);
            Assert.IsFalse(_targetAsyncWorker.Testing);
        }

        [TestMethod]
        public void TestAuthenticationType_dbSourceNotNullUserSameDbSource()
        {
            //arrange
            var expectedValue = AuthenticationType.User;
            _dbSourceMock.SetupGet(it => it.AuthenticationType).Returns(AuthenticationType.User);
            var expectedPassword = "somePassword";
            var expectedUsername = "someUsername";
            _dbSourceMock.SetupGet(it => it.Password).Returns(expectedPassword);
            _dbSourceMock.SetupGet(it => it.UserName).Returns(expectedUsername);
            _changedPropertiesUpdateManagerAggregatorDbSource.Clear();
            _targetUpdateManagerAggregatorDbSource.DatabaseNames = new List<string>() { "DatabaseNamesItem" };

            //act
            _targetUpdateManagerAggregatorDbSource.AuthenticationType = expectedValue;
            var actualValue = _targetUpdateManagerAggregatorDbSource.AuthenticationType;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(expectedUsername, _targetUpdateManagerAggregatorDbSource.UserName);
            Assert.AreEqual(expectedPassword, _targetUpdateManagerAggregatorDbSource.Password);
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("AuthenticationType"));
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("Header"));
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("UserAuthenticationSelected"));
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.DatabaseNames.Any());
            Assert.IsFalse(_targetAsyncWorker.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.TestMessage));
            Assert.IsFalse(_targetAsyncWorker.TestFailed);
            Assert.IsFalse(_targetAsyncWorker.Testing);
        }

        [TestMethod]
        public void TestName()
        {
            //arrange
            var expectedValue = "someName";

            //act
            _targetAsyncWorker.Name = expectedValue;
            var actualValue = _targetAsyncWorker.Name;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(expectedValue, _targetAsyncWorker.ResourceName);
        }

        [TestMethod]
        public void TestResourceNameNotEmpty()
        {
            //arrange
            var expectedValue = "someName";
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.ResourceName = expectedValue;
            var actualValue = _targetAsyncWorker.ResourceName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(expectedValue, _targetAsyncWorker.HeaderText);
            Assert.AreEqual(expectedValue, _targetAsyncWorker.Header);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains(expectedValue));
        }

        [TestMethod]
        public void TestResourceNameEmpty()
        {
            //arrange
            var expectedValue = "";
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.ResourceName = expectedValue;
            var actualValue = _targetAsyncWorker.ResourceName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains(expectedValue));
        }

        [TestMethod]
        public void TestUserAuthenticationSelectedFalse()
        {
            //arrange
            _targetAsyncWorker.AuthenticationType = AuthenticationType.Public;

            //act
            var actualValue = _targetAsyncWorker.UserAuthenticationSelected;

            //assert
            Assert.IsFalse(actualValue);
        }

        [TestMethod]
        public void TestUserAuthenticationSelectedTrue()
        {
            //arrange
            _targetAsyncWorker.AuthenticationType = AuthenticationType.User;

            //act
            var actualValue = _targetAsyncWorker.UserAuthenticationSelected;

            //assert
            Assert.IsTrue(actualValue);
        }

        [TestMethod]
        public void TestComputerNames()
        {
            //arrange
            var expectedValue = new List<ComputerName>();
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.ComputerNames = expectedValue;
            var actualValue = _targetAsyncWorker.ComputerNames;

            //assert
            Assert.AreSame(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("ComputerNames"));
        }

        [TestMethod]
        public void TestPath()
        {
            //arrange
            var expectedValue = "somePath";
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.Path = expectedValue;
            var actualValue = _targetAsyncWorker.Path;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Path"));
        }

        [TestMethod]
        public void TestRequestServiceNameViewModel()
        {
            var valueMock = new Mock<IRequestServiceNameViewModel>();
            var expectedValue = Task.FromResult(valueMock.Object);

            //act
            _targetAsyncWorker.RequestServiceNameViewModel = expectedValue;
            var actualValue = _targetAsyncWorker.RequestServiceNameViewModel;

            //assert
            Assert.AreSame(expectedValue, actualValue);
        }

        [TestMethod]
        public void TestDatabaseName()
        {
            //arrange
            var expectedValue = "someDatabaseName";
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.DatabaseName = expectedValue;
            var actualValue = _targetAsyncWorker.DatabaseName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("DatabaseName"));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Header"));
        }

        [TestMethod]
        public void TestDatabaseNames()
        {
            //arrange
            var expectedValue = new List<string>();
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.DatabaseNames = expectedValue;
            var actualValue = _targetAsyncWorker.DatabaseNames;

            //assert
            Assert.AreSame(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("DatabaseNames"));
        }

        [TestMethod]
        public void TestUserName()
        {
            //arrange
            var expectedValue = "someDatabaseName";
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.UserName = expectedValue;
            var actualValue = _targetAsyncWorker.UserName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("UserName"));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Header"));
            Assert.IsFalse(_targetAsyncWorker.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.TestMessage));
            Assert.IsFalse(_targetAsyncWorker.TestFailed);
            Assert.IsFalse(_targetAsyncWorker.Testing);
        }

        [TestMethod]
        public void TestPassword()
        {
            //arrange
            var expectedValue = "someDatabaseName";
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.Password = expectedValue;
            var actualValue = _targetAsyncWorker.Password;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Password"));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Header"));
            Assert.IsFalse(_targetAsyncWorker.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.TestMessage));
            Assert.IsFalse(_targetAsyncWorker.TestFailed);
            Assert.IsFalse(_targetAsyncWorker.Testing);
        }

        [TestMethod]
        public void TestTestPassed()
        {
            //arrange
            var expectedValue = true;
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.TestPassed = expectedValue;
            var actualValue = _targetAsyncWorker.TestPassed;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("TestPassed"));
        }

        [TestMethod]
        public void TestTestFailed()
        {
            //arrange
            var expectedValue = true;
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.TestFailed = expectedValue;
            var actualValue = _targetAsyncWorker.TestFailed;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("TestFailed"));
        }

        [TestMethod]
        public void TestHeaderText()
        {
            //arrange
            var expectedValue = "someHeaderText";
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.HeaderText = expectedValue;
            var actualValue = _targetAsyncWorker.HeaderText;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("HeaderText"));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Header"));
        }

        [TestMethod]
        public void TestServerName()
        {
            //arrange
            var expectedValue = new ComputerName();
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.ServerName = expectedValue;
            var actualValue = _targetAsyncWorker.ServerName;

            //assert
            Assert.AreSame(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("ServerName"));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Header"));
            Assert.IsFalse(_targetAsyncWorker.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.TestMessage));
            Assert.IsFalse(_targetAsyncWorker.TestFailed);
            Assert.IsFalse(_targetAsyncWorker.Testing);
        }

        [TestMethod]
        public void TestEmptyServerName()
        {
            //arrange
            var expectedValue = "someName2";
            _targetAsyncWorker.ServerName = new ComputerName() { Name = "someName2" };
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.EmptyServerName = expectedValue;
            var actualValue = _targetAsyncWorker.EmptyServerName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(expectedValue, _targetAsyncWorker.ServerName.Name);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("EmptyServerName"));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Header"));
            Assert.IsFalse(_targetAsyncWorker.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.TestMessage));
            Assert.IsFalse(_targetAsyncWorker.TestFailed);
            Assert.IsFalse(_targetAsyncWorker.Testing);
        }

        [TestMethod]
        public void TestEmptyServerNameEmpty()
        {
            //arrange
            var expectedValue = "someEmptyValue";
            _targetAsyncWorker.EmptyServerName = "";
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.EmptyServerName = expectedValue;
            var actualValue = _targetAsyncWorker.EmptyServerName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(expectedValue, _targetAsyncWorker.ServerName.Name);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("EmptyServerName"));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Header"));
            Assert.IsFalse(_targetAsyncWorker.TestPassed);
            Assert.IsTrue(string.IsNullOrEmpty(_targetAsyncWorker.TestMessage));
            Assert.IsFalse(_targetAsyncWorker.TestFailed);
            Assert.IsFalse(_targetAsyncWorker.Testing);
        }

        #endregion Test properties

        #region Test commands

        [TestMethod]
        public void TestTestCommandCanExecute()
        {
            //arrange
            _targetUpdateManagerAggregatorDbSource.ServerName = new ComputerName() { Name = "someName" };
            _targetUpdateManagerAggregatorDbSource.AuthenticationType = AuthenticationType.Public;

            //act
            var result = _targetUpdateManagerAggregatorDbSource.TestCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestTestCommandExecuteAsyncWorkerSuccess()
        {
            //arrange
            _targetUpdateManagerAggregatorDbSource.DatabaseNames = new List<string>() { "someName" };
            var expectedDatabaseNames = new List<string>();

            _asyncWorkerMock.Setup(
                it =>
                it.Start<IList<string>>(
                    It.IsAny<Func<IList<string>>>(),
                    It.IsAny<Action<IList<string>>>(),
                    It.IsAny<CancellationTokenSource>(),
                    It.IsAny<Action<Exception>>()))
                    .Callback<Func<IList<string>>, Action<IList<string>>, CancellationTokenSource, Action<Exception>>(
                        (a1, a2, t, ae) =>
                        {
                            a2(expectedDatabaseNames);
                        });
            _changedPropertiesUpdateManagerAggregatorDbSource.Clear();

            //act
            _targetUpdateManagerAggregatorDbSource.TestCommand.Execute(null);

            //assert
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("DatabaseNames"));
            Assert.AreSame(expectedDatabaseNames, _targetUpdateManagerAggregatorDbSource.DatabaseNames);
            Assert.AreEqual("Passed", _targetUpdateManagerAggregatorDbSource.TestMessage);
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.TestFailed);
            Assert.IsTrue(_targetUpdateManagerAggregatorDbSource.TestPassed);
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.Testing);
        }

        [TestMethod]
        public void TestTestCommandExecuteAsyncWorkerExceptionNull()
        {
            //arrange
            _targetUpdateManagerAggregatorDbSource.DatabaseNames = new List<string>() { "someName" };

            _asyncWorkerMock.Setup(
                it =>
                it.Start<IList<string>>(
                    It.IsAny<Func<IList<string>>>(),
                    It.IsAny<Action<IList<string>>>(),
                    It.IsAny<CancellationTokenSource>(),
                    It.IsAny<Action<Exception>>()))
                    .Callback<Func<IList<string>>, Action<IList<string>>, CancellationTokenSource, Action<Exception>>(
                        (a1, a2, t, ae) =>
                        {
                            ae(null);
                        });
            _changedPropertiesUpdateManagerAggregatorDbSource.Clear();

            //act
            _targetUpdateManagerAggregatorDbSource.TestCommand.Execute(null);

            //assert
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("DatabaseNames"));
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.DatabaseNames.Any());
            Assert.AreEqual("Failed", _targetUpdateManagerAggregatorDbSource.TestMessage);
            Assert.IsTrue(_targetUpdateManagerAggregatorDbSource.TestFailed);
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.TestPassed);
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.Testing);
        }

        [TestMethod]
        public void TestTestCommandExecuteAsyncWorkerExceptionNotNull()
        {
            //arrange
            _targetUpdateManagerAggregatorDbSource.DatabaseNames = new List<string>() { "someName" };
            var expectedExceptionMessage = "some message";
            _asyncWorkerMock.Setup(
                it =>
                it.Start<IList<string>>(
                    It.IsAny<Func<IList<string>>>(),
                    It.IsAny<Action<IList<string>>>(),
                    It.IsAny<CancellationTokenSource>(),
                    It.IsAny<Action<Exception>>()))
                    .Callback<Func<IList<string>>, Action<IList<string>>, CancellationTokenSource, Action<Exception>>(
                        (a1, a2, t, ae) =>
                        {
                            ae(new Exception(expectedExceptionMessage));
                        });
            _changedPropertiesUpdateManagerAggregatorDbSource.Clear();

            //act
            _targetUpdateManagerAggregatorDbSource.TestCommand.Execute(null);

            //assert
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("DatabaseNames"));
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.DatabaseNames.Any());
            Assert.AreEqual("Exception: " + expectedExceptionMessage, _targetUpdateManagerAggregatorDbSource.TestMessage);
            Assert.IsTrue(_targetUpdateManagerAggregatorDbSource.TestFailed);
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.TestPassed);
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.Testing);
        }

        [TestMethod]
        public void TestTestCommandExecuteAsyncWorkerThrowsException()
        {
            //arrange
            _targetUpdateManagerAggregatorDbSource.DatabaseNames = new List<string>() { "someName" };
            var expectedExceptionMessage = "some message";
            _asyncWorkerMock.Setup(
                it =>
                it.Start<IList<string>>(
                    It.IsAny<Func<IList<string>>>(),
                    It.IsAny<Action<IList<string>>>(),
                    It.IsAny<CancellationTokenSource>(),
                    It.IsAny<Action<Exception>>()))
                    .Throws(new Exception(expectedExceptionMessage));
            _changedPropertiesUpdateManagerAggregatorDbSource.Clear();

            //act
            _targetUpdateManagerAggregatorDbSource.TestCommand.Execute(null);

            //assert
            Assert.IsTrue(_changedPropertiesUpdateManagerAggregatorDbSource.Contains("DatabaseNames"));
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.DatabaseNames.Any());
            Assert.AreEqual("Exception: " + expectedExceptionMessage, _targetUpdateManagerAggregatorDbSource.TestMessage);
            Assert.IsTrue(_targetUpdateManagerAggregatorDbSource.TestFailed);
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.TestPassed);
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.Testing);
        }

        [TestMethod]
        public void TestOkCommandCanExecute()
        {
            //arrange
            _targetUpdateManagerAggregatorDbSource.TestPassed = true;
            _targetUpdateManagerAggregatorDbSource.DatabaseName = "someDbName";

            //act
            var result = _targetUpdateManagerAggregatorDbSource.OkCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestOkCommandExecute_dbSourceNull()
        {
            //arrange
            var resPath = "resPath";
            var resName = "resName";
            _requestServiceNameViewMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewMock.Setup(it => it.ResourceName).Returns(new ResourceName(resPath, resName));
            var expectedAuthenticationType = AuthenticationType.Anonymous;
            var expectedServerName = "expectedServerName";
            var expectedPassword = "expectedPassword";
            var expectedUsername = "expectedUsername";
            var expectedType = enSourceType.SqlDatabase;
            var expectedPath = "somePath";
            var expectedName = "someName";
            var expectedDbName = "someDbName";
            _targetUpdateManagerRequestServiceName.AuthenticationType = expectedAuthenticationType;
            _targetUpdateManagerRequestServiceName.ServerName = new ComputerName() { Name = expectedServerName };
            _targetUpdateManagerRequestServiceName.Password = expectedPassword;
            _targetUpdateManagerRequestServiceName.UserName = expectedUsername;
            _targetUpdateManagerRequestServiceName.Path = expectedPath;
            _targetUpdateManagerRequestServiceName.ResourceName = expectedName;
            _targetUpdateManagerRequestServiceName.DatabaseName = expectedDbName;
            _targetUpdateManagerRequestServiceName.SelectedGuid = Guid.NewGuid();
            //act
            _targetUpdateManagerRequestServiceName.OkCommand.Execute(null);

            //assert
            Assert.IsInstanceOfType(_targetUpdateManagerRequestServiceName.Item, typeof(DbSourceDefinition));
            _updateManagerMock.Verify(it => it.Save(_targetUpdateManagerRequestServiceName.Item));
            Assert.AreEqual(_targetUpdateManagerRequestServiceName.Item.Name, _targetUpdateManagerRequestServiceName.HeaderText);
            Assert.AreEqual(_targetUpdateManagerRequestServiceName.Item.Name, _targetUpdateManagerRequestServiceName.Header);
            Assert.AreEqual(expectedAuthenticationType, _targetUpdateManagerRequestServiceName.Item.AuthenticationType);
            Assert.AreEqual(expectedServerName, _targetUpdateManagerRequestServiceName.Item.ServerName);
            Assert.AreEqual(expectedPassword, _targetUpdateManagerRequestServiceName.Item.Password);
            Assert.AreEqual(expectedUsername, _targetUpdateManagerRequestServiceName.Item.UserName);
            Assert.AreEqual(expectedType, _targetUpdateManagerRequestServiceName.Item.Type);
            Assert.AreEqual(resPath, _targetUpdateManagerRequestServiceName.Item.Path);
            Assert.AreEqual(resName, _targetUpdateManagerRequestServiceName.Item.Name);
            Assert.AreEqual(expectedDbName, _targetUpdateManagerRequestServiceName.Item.DbName);
            Assert.AreNotEqual(Guid.Empty, _targetUpdateManagerRequestServiceName.Item.Id);
        }

        [TestMethod]
        public void TestOkCommandExecute_dbSourceNotNull()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.Anonymous;
            var expectedServerName = "expectedServerName";
            var expectedPassword = "expectedPassword";
            var expectedUsername = "expectedUsername";
            var expectedType = enSourceType.SqlDatabase;
            var expectedPath = "somePath";
            var expectedDbName = "someDbName";
            var dbSourceName = "dbSourceName";
            var expectedId = Guid.NewGuid();
            _targetUpdateManagerAggregatorDbSource.AuthenticationType = expectedAuthenticationType;
            _targetUpdateManagerAggregatorDbSource.ServerName = new ComputerName() { Name = expectedServerName };
            _targetUpdateManagerAggregatorDbSource.Password = expectedPassword;
            _targetUpdateManagerAggregatorDbSource.UserName = expectedUsername;
            _targetUpdateManagerAggregatorDbSource.Path = expectedPath;
            _targetUpdateManagerAggregatorDbSource.DatabaseName = expectedDbName;
            _dbSourceMock.SetupGet(it => it.Id).Returns(expectedId);
            _dbSourceMock.SetupGet(it => it.Name).Returns(dbSourceName);
            _targetUpdateManagerAggregatorDbSource.ResourceName = dbSourceName;

            //act
            _targetUpdateManagerAggregatorDbSource.OkCommand.Execute(null);

            //assert
            Assert.IsInstanceOfType(_targetUpdateManagerAggregatorDbSource.Item, typeof(DbSourceDefinition));
            _updateManagerMock.Verify(it => it.Save(_targetUpdateManagerAggregatorDbSource.Item));
            Assert.AreEqual(_targetUpdateManagerAggregatorDbSource.Item.Name, _targetUpdateManagerAggregatorDbSource.HeaderText);
            Assert.AreEqual(_targetUpdateManagerAggregatorDbSource.Item.Name, _targetUpdateManagerAggregatorDbSource.Header);
            Assert.AreEqual(expectedAuthenticationType, _targetUpdateManagerAggregatorDbSource.Item.AuthenticationType);
            Assert.AreEqual(expectedServerName, _targetUpdateManagerAggregatorDbSource.Item.ServerName);
            Assert.AreEqual(expectedPassword, _targetUpdateManagerAggregatorDbSource.Item.Password);
            Assert.AreEqual(expectedUsername, _targetUpdateManagerAggregatorDbSource.Item.UserName);
            Assert.AreEqual(expectedType, _targetUpdateManagerAggregatorDbSource.Item.Type);
            Assert.AreEqual(expectedPath, _targetUpdateManagerAggregatorDbSource.Item.Path);
            Assert.AreEqual(dbSourceName, _targetUpdateManagerAggregatorDbSource.Item.Name);
            Assert.AreEqual(expectedDbName, _targetUpdateManagerAggregatorDbSource.Item.DbName);
            Assert.AreEqual(expectedId, _targetUpdateManagerAggregatorDbSource.Item.Id);
        }

        [TestMethod]
        public void TestCancelTestCommandCanExecute()
        {
            //act
            var result = _targetUpdateManagerAggregatorDbSource.CancelTestCommand.CanExecute(null);

            //assert    
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCancelDBSourceTestCommandExecute()
        {
            //act
            _targetUpdateManagerAggregatorDbSource.CancelTestCommand.Execute(null);
        }

        #endregion Test commands

        #region Test methods

        [TestMethod]
        public void TestToModelItemNull()
        {
            //arrange
            var gd = Guid.NewGuid();
            _targetAsyncWorker.Item = null;
            var expectedType = enSourceType.SqlDatabase;
            var expectedAuthenticationType = AuthenticationType.User;
            _targetAsyncWorker.AuthenticationType = expectedAuthenticationType;
            var expectedServerName = "serverName";
            _targetAsyncWorker.ServerName = new ComputerName() { Name = expectedServerName };
            var expectedPassword = "password";
            _targetAsyncWorker.Password = expectedPassword;
            var expectedUserName = "userName";
            _targetAsyncWorker.UserName = expectedUserName;
            var expectedPath = "somePath";
            _targetAsyncWorker.Path = expectedPath;
            var expectedName = "someName";
            _targetAsyncWorker.ResourceName = expectedName;
            var expectedDbName = "someDbName";
            _targetAsyncWorker.DatabaseName = expectedDbName;
            _targetAsyncWorker.SelectedGuid = gd;
            //act
            var value = _targetAsyncWorker.ToModel();

            //assert
            Assert.AreSame(_targetAsyncWorker.Item, value);
            Assert.AreEqual(expectedAuthenticationType, value.AuthenticationType);
            Assert.AreEqual(expectedPassword, value.Password);
            Assert.AreEqual(expectedUserName, value.UserName);
            Assert.AreEqual(expectedType, value.Type);
            Assert.AreEqual(expectedPath, value.Path);
            Assert.AreEqual(expectedName, value.Name);
            Assert.AreEqual(expectedDbName, value.DbName);
            Assert.AreEqual(gd, value.Id);

        }

        [TestMethod]
        public void TestToModelItemNotNull()
        {
            //arrange
            var dbSourceMock = new Mock<IDbSource>();
            var expectedId = Guid.NewGuid();
            dbSourceMock.Setup(it => it.Id).Returns(expectedId);
            _targetAsyncWorker.Item = dbSourceMock.Object;
            var expectedType = enSourceType.SqlDatabase;
            var expectedAuthenticationType = AuthenticationType.User;
            _targetAsyncWorker.AuthenticationType = expectedAuthenticationType;
            var expectedServerName = "serverName";
            _targetAsyncWorker.ServerName = new ComputerName() { Name = expectedServerName };
            var expectedPassword = "password";
            _targetAsyncWorker.Password = expectedPassword;
            var expectedUserName = "userName";
            _targetAsyncWorker.UserName = expectedUserName;
            var expectedName = "someName";
            _targetAsyncWorker.ResourceName = expectedName;
            var expectedDbName = "someDbName";
            _targetAsyncWorker.DatabaseName = expectedDbName;

            //act
            var value = _targetAsyncWorker.ToModel();

            //assert
            Assert.AreNotSame(_targetAsyncWorker.Item, value);
            Assert.AreEqual(expectedAuthenticationType, value.AuthenticationType);
            Assert.AreEqual(expectedPassword, value.Password);
            Assert.AreEqual(expectedUserName, value.UserName);
            Assert.AreEqual(expectedType, value.Type);
            Assert.AreEqual(expectedName, value.Name);
            Assert.AreEqual(expectedDbName, value.DbName);
            Assert.AreEqual(expectedId, value.Id);
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
            _targetAsyncWorker.UpdateHelpDescriptor(helpText);

            //assert
            helpViewModelMock.Verify(it => it.UpdateHelpText(helpText));
        }

        [TestMethod]
        public void TestSave()
        {
            //arrange
            var exceptionMessage = "exceptionMessage";
            _updateManagerMock.Setup(it => it.Save(It.IsAny<IDbSource>())).Throws(new Exception(exceptionMessage));

            //act
            _targetUpdateManagerAggregatorDbSource.Save();

            //assert
            _updateManagerMock.Verify(it => it.Save(_targetUpdateManagerAggregatorDbSource.Item));
        }

        [TestMethod]
        public void TestSaveException()
        {
            //arrange
            var exceptionMessage = "exceptionMessage";
            _updateManagerMock.Setup(it => it.Save(It.IsAny<IDbSource>())).Throws(new Exception(exceptionMessage));

            //act
            _targetUpdateManagerAggregatorDbSource.Save();

            //assert
            Assert.AreEqual(exceptionMessage, _targetUpdateManagerAggregatorDbSource.TestMessage);
            Assert.IsTrue(_targetUpdateManagerAggregatorDbSource.TestFailed);
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.TestPassed);
        }

        [TestMethod]
        public void TestSavePassed()
        {
            //arrange
            var exceptionMessage = "";
            _updateManagerMock.Setup(it => it.Save(It.IsAny<IDbSource>()));

            //act
            _targetUpdateManagerAggregatorDbSource.Save();

            //assert
            _updateManagerMock.Verify(it => it.Save(_targetUpdateManagerAggregatorDbSource.Item));
            Assert.AreEqual(exceptionMessage, _targetUpdateManagerAggregatorDbSource.TestMessage);
            Assert.IsFalse(_targetUpdateManagerAggregatorDbSource.TestFailed);
        }

        [TestMethod]
        public void TestCanSaveTestPassedFalse()
        {
            //arrange
            _targetAsyncWorker.TestPassed = false;

            //act
            var result = _targetAsyncWorker.CanSave();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanSaveTestPassedTrueDatabaseNameIsEmpty()
        {
            //arrange
            _targetAsyncWorker.TestPassed = true;
            _targetAsyncWorker.DatabaseName = "";

            //act
            var result = _targetAsyncWorker.CanSave();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanSaveTestPassedTrueDatabaseNameNonEmpty()
        {
            //arrange
            _targetAsyncWorker.TestPassed = true;
            _targetAsyncWorker.DatabaseName = "someDbName";

            //act
            var result = _targetAsyncWorker.CanSave();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCanTestServerNameNotNull()
        {
            //arrange
            _targetAsyncWorker.ServerName = new ComputerName() { Name = "" };

            //act
            var result = _targetAsyncWorker.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestAuthenticationTypeUserUserNamePasswordEmpty()
        {
            //arrange
            _targetAsyncWorker.AuthenticationType = AuthenticationType.User;
            _targetAsyncWorker.UserName = "";
            _targetAsyncWorker.Password = "";
            _targetAsyncWorker.ServerName = new ComputerName() { Name = "someName" };

            //act
            var result = _targetAsyncWorker.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestAuthenticationTypeUserUserNameNonEmptyPasswordEmpty()
        {
            //arrange
            _targetAsyncWorker.AuthenticationType = AuthenticationType.User;
            _targetAsyncWorker.UserName = "someUser";
            _targetAsyncWorker.Password = "";
            _targetAsyncWorker.ServerName = new ComputerName() { Name = "someName" };

            //act
            var result = _targetAsyncWorker.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestAuthenticationTypeUser()
        {
            //arrange
            _targetAsyncWorker.AuthenticationType = AuthenticationType.User;
            _targetAsyncWorker.UserName = "someUser";
            _targetAsyncWorker.Password = "somePassword";
            _targetAsyncWorker.ServerName = new ComputerName() { Name = "someName" };

            //act
            var result = _targetAsyncWorker.CanTest();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCanTestServerNameNull()
        {
            //arrange
            _targetAsyncWorker.ServerName = new ComputerName() { Name = "someName" };
            _targetAsyncWorker.AuthenticationType = AuthenticationType.Public;

            //act
            var result = _targetAsyncWorker.CanTest();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCanTest()
        {
            //arrange
            _targetAsyncWorker.ServerName = new ComputerName() { Name = "SomeName" };
            _targetAsyncWorker.AuthenticationType = AuthenticationType.Public;

            //act
            var result = _targetAsyncWorker.CanTest();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestFromModel()
        {
            //arrange
            var dbSourceMock = new Mock<IDbSource>();
            var expectedResourceName = "expectedResourceName";
            var expectedAuthenticationType = AuthenticationType.Windows;
            var expectedUserName = "expectedUserName";
            var dbSourceServerName = "dbSoureServerName";
            var dbSourceType = enSourceType.DynamicService;
            var expectedPassword = "expectedPassword";
            var expectedPath = "expectedPath";
            var expectedServerName = new ComputerName() { Name = dbSourceServerName };
            _targetAsyncWorker.ComputerNames = new List<ComputerName> { expectedServerName };
            var expectedDatabaseName = "expectedDatabaseName";

            dbSourceMock.SetupGet(it => it.Name).Returns(expectedResourceName);
            dbSourceMock.SetupGet(it => it.AuthenticationType).Returns(expectedAuthenticationType);
            dbSourceMock.SetupGet(it => it.UserName).Returns(expectedUserName);
            dbSourceMock.SetupGet(it => it.ServerName).Returns(dbSourceServerName);
            dbSourceMock.SetupGet(it => it.Password).Returns(expectedPassword);
            dbSourceMock.SetupGet(it => it.Path).Returns(expectedPath);
            dbSourceMock.SetupGet(it => it.Type).Returns(dbSourceType);
            dbSourceMock.SetupGet(it => it.DbName).Returns(expectedDatabaseName);
            _changedPropertiesAsyncWorker.Clear();

            //act
            _targetAsyncWorker.FromModel(dbSourceMock.Object);

            //assert
            Assert.AreEqual(expectedResourceName, _targetAsyncWorker.ResourceName);
            Assert.AreEqual(expectedAuthenticationType, _targetAsyncWorker.AuthenticationType);
            Assert.AreEqual(expectedUserName, _targetAsyncWorker.UserName);
            Assert.AreSame(expectedServerName, _targetAsyncWorker.ServerName);
            Assert.AreEqual(dbSourceServerName, _targetAsyncWorker.EmptyServerName);
            Assert.AreEqual(expectedPassword, _targetAsyncWorker.Password);
            Assert.AreEqual(expectedPath, _targetAsyncWorker.Path);
            Assert.AreEqual(expectedDatabaseName, _targetAsyncWorker.DatabaseName);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("DatabaseNames"));
        }

        #endregion Test methods
    }
}