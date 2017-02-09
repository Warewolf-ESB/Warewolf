using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;

using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SharepointServerSourceViewModelTests
    {
        #region Fields

        private Mock<ISharePointSourceModel> _updateManagerMock;
        private Mock<IEventAggregator> _aggregatorMock;
        private Mock<IAsyncWorker> _asyncWorkerMock;
        private Mock<IEnvironmentModel> _environmentMock;
        private Mock<ISharepointServerSource> _sharepointServerSourceMock;
        private Mock<IRequestServiceNameViewModel> _requestServiceNameViewModelMock;
        private Task<IRequestServiceNameViewModel> _requestServiceNameViewModelTask;

        private List<string> _changedProperties;
        private SharepointServerSourceViewModel _target;

        private List<string> _changedPropertiesSource;
        private SharepointServerSourceViewModel _targetSource;

        private List<string> _changedPropertiesRequestServiceViewModel;
        private SharepointServerSourceViewModel _targetRequestServiceViewModel;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateManagerMock = new Mock<ISharePointSourceModel>();
            _aggregatorMock = new Mock<IEventAggregator>();
            _asyncWorkerMock = new Mock<IAsyncWorker>();
            _environmentMock = new Mock<IEnvironmentModel>();
            
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
           
            _sharepointServerSourceMock = new Mock<ISharepointServerSource>();
            _sharepointServerSourceMock.Setup(it => it.Name).Returns("someService");
            _requestServiceNameViewModelMock = new Mock<IRequestServiceNameViewModel>();
            _requestServiceNameViewModelTask = Task.FromResult(_requestServiceNameViewModelMock.Object);
            _updateManagerMock.Setup(model => model.FetchSource(It.IsAny<Guid>()))
           .Returns(_sharepointServerSourceMock.Object);
            _asyncWorkerMock.Setup(worker =>
                                   worker.Start(
                                            It.IsAny<Func<ISharepointServerSource>>(),
                                            It.IsAny<Action<ISharepointServerSource>>()))
                            .Callback<Func<ISharepointServerSource>, Action<ISharepointServerSource>>((func, action) =>
                            {
                                var dbSource = func.Invoke();
                                action(dbSource);
                            });

            _changedProperties = new List<string>();
            _target = new SharepointServerSourceViewModel(
                _updateManagerMock.Object,
                _aggregatorMock.Object,
                _asyncWorkerMock.Object,
                _environmentMock.Object);
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };

            _changedPropertiesSource = new List<string>();
            _targetSource = new SharepointServerSourceViewModel(
                _updateManagerMock.Object,
                _aggregatorMock.Object,
                _sharepointServerSourceMock.Object,
                _asyncWorkerMock.Object,
                _environmentMock.Object);
            _targetSource.PropertyChanged += (sender, args) => { _changedPropertiesSource.Add(args.PropertyName); };

            _changedPropertiesRequestServiceViewModel = new List<string>();
            _targetRequestServiceViewModel = new SharepointServerSourceViewModel(
                _updateManagerMock.Object,
                _requestServiceNameViewModelTask,
                _aggregatorMock.Object,
                _asyncWorkerMock.Object,
                _environmentMock.Object);
            _targetRequestServiceViewModel.PropertyChanged +=
                (sender, args) => { _changedPropertiesRequestServiceViewModel.Add(args.PropertyName); };
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointServerSourceViewModelAsyncWorkerNull()
        {
            new SharepointServerSourceViewModel(
                 _updateManagerMock.Object,
                 _aggregatorMock.Object,
                 null,
                 _environmentMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointServerSourceViewModelUpdateManagerNull()
        {
            new SharepointServerSourceViewModel(
                 null,
                 _aggregatorMock.Object,
                 _asyncWorkerMock.Object,
                 _environmentMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointServerSourceViewModelAggregatorManagerNull()
        {
            new SharepointServerSourceViewModel(
                 _updateManagerMock.Object,
                 null,
                 _asyncWorkerMock.Object,
                 _environmentMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointServerSourceViewModelRequestServiceViewModel()
        {
            new SharepointServerSourceViewModel(
                 _updateManagerMock.Object,
                 null,
                 _aggregatorMock.Object,
                 _asyncWorkerMock.Object,
                 _environmentMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointServerSourceViewModelSourceNull()
        {
            new SharepointServerSourceViewModel(
                 _updateManagerMock.Object,
                 _aggregatorMock.Object,
                 null,
                 _asyncWorkerMock.Object,
                 _environmentMock.Object);
        }

        #endregion Test construction

        #region Test commands

        [TestMethod]
        public void TestCancelTestCommandCanExecute()
        {
            //act
            var result = _target.CancelTestCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCancelSharepointSourceTestCommandExecute()
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
            _target.TestCommand.Execute(null);

            //act
            _target.CancelTestCommand.Execute(null);
            task.Wait();

            //assert
            Assert.IsTrue(isCancelled, "Cancel test command does not cancel sharepoint source test.");
        }

        [TestMethod]
        public void TestTestCommandCanExecute()
        {
            //arrange
            _target.AuthenticationType = AuthenticationType.User;
            _target.UserName = "someUserName";
            _target.Password = "somePassword";
            _target.ServerName = "someServer";

            //act
            var result = _target.TestCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestTestCommandExecutePass()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";
            var expectedName = "someExpectedName";
            var expectedServer = "someServer";

            _target.AuthenticationType = expectedAuthenticationType;
            _target.Password = expectedPassword;
            _target.UserName = expectedUserName;
            _target.ResourceName = expectedName;
            _target.ServerName = expectedServer;

            //act
            _target.TestCommand.Execute(null);

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsTrue(_target.TestPassed);
            Assert.IsFalse(_target.TestFailed);
            Assert.IsFalse(_target.Testing);
            _updateManagerMock.Verify(
                it =>
                it.TestConnection(
                    It.Is<ISharepointServerSource>(
                        src =>
                        src.AuthenticationType == expectedAuthenticationType && src.Server == expectedServer
                        && src.Password == expectedPassword && src.UserName == expectedUserName
                        && src.Name == expectedName && src.Id != Guid.Empty)));
        }

        [TestMethod]
        public void TestTestCommandExecutePassWorkerFailed()
        {
            //arrange
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
            var expectedExceptionMessage = "someExceptionMessage";
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

            //act
            _target.TestCommand.Execute(null);

            //assert
            Assert.AreEqual("Exception: " + expectedExceptionMessage, _target.TestMessage);
            Assert.IsFalse(_target.TestPassed);
            Assert.IsFalse(_target.Testing);
        }

        [TestMethod]
        public void TestSaveCommandCanExecute()
        {
            //arrange
            _target.TestPassed = true;

            //act
            var result = _target.SaveCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestSaveCommandExecute_serverSourceNotNull()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedServer = "someServer";
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";
            var expectedName = "someExpectedName";

            var sourceName = "someSourceName";

            var expectedHeaderText = sourceName;
            var expectedHeader = sourceName + " *";

            _sharepointServerSourceMock.Setup(it => it.Name).Returns(sourceName);

            _targetSource.AuthenticationType = expectedAuthenticationType;
            _targetSource.Password = expectedPassword;
            _targetSource.UserName = expectedUserName;
            _targetSource.ResourceName = expectedName;
            _targetSource.ServerName = expectedServer;

            //act
            _targetSource.SaveCommand.Execute(null);

            //assert
            Assert.AreSame(_sharepointServerSourceMock.Object, _targetSource.Item);
            _sharepointServerSourceMock.VerifySet(it => it.AuthenticationType = expectedAuthenticationType);
            _sharepointServerSourceMock.VerifySet(it => it.Server = expectedServer);
            _sharepointServerSourceMock.VerifySet(it => it.Password = expectedPassword);
            _sharepointServerSourceMock.VerifySet(it => it.UserName = expectedUserName);
            _updateManagerMock.Verify(it => it.Save(_sharepointServerSourceMock.Object));
            Assert.AreEqual(expectedHeaderText, _targetSource.HeaderText);
            Assert.AreEqual(expectedHeader, _targetSource.Header);
        }

        [TestMethod]
        public void TestSaveCommandExecute_serverSourceNull()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedServerName = "someServerName";
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";
            var expectedName = "someExpectedName";
            var expectedHeaderText = expectedName;
            var expectedHeader = expectedName;
            var expectedPath = "somePath";
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(expectedPath, expectedName));
            _targetRequestServiceViewModel.AuthenticationType = expectedAuthenticationType;
            _targetRequestServiceViewModel.Password = expectedPassword;
            _targetRequestServiceViewModel.UserName = expectedUserName;
            _targetRequestServiceViewModel.ServerName = expectedServerName;

            //act
            _targetRequestServiceViewModel.SaveCommand.Execute(null);

            //assert
            _requestServiceNameViewModelMock.Verify(it => it.ShowSaveDialog());
            Assert.IsInstanceOfType(_targetRequestServiceViewModel.Item, typeof(SharePointServiceSourceDefinition));
            _updateManagerMock.Verify(it => it.Save(_targetRequestServiceViewModel.Item));
            Assert.AreEqual(expectedAuthenticationType, _targetRequestServiceViewModel.Item.AuthenticationType);
            Assert.AreEqual(expectedServerName, _targetRequestServiceViewModel.Item.Server);
            Assert.AreEqual(expectedPassword, _targetRequestServiceViewModel.Item.Password);
            Assert.AreEqual(expectedName, _targetRequestServiceViewModel.ResourceName);
            Assert.AreEqual(expectedName, _targetRequestServiceViewModel.Item.Name);
            Assert.AreNotEqual(Guid.Empty, _targetRequestServiceViewModel.Item.Id);
            _updateManagerMock.Verify(it => it.Save(It.IsAny<ISharepointServerSource>()));
            Assert.AreEqual(expectedHeaderText, _targetRequestServiceViewModel.HeaderText);
            Assert.AreEqual(expectedHeader, _targetRequestServiceViewModel.Header);
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
            var expectedValue = "someName";
            _changedProperties.Clear();

            //act
            _target.ServerName = expectedValue;
            var actualValue = _target.ServerName;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("ServerName"));
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
        public void TestPath()
        {
            //arrange
            var expectedValue = "someValue";
            _changedProperties.Clear();

            //act
            _target.Path = expectedValue;
            var actualValue = _target.Path;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("Path"));
        }

        [TestMethod]
        public void TestAuthenticationType()
        {
            //arrange
            var expectedValue = AuthenticationType.User;
            _changedProperties.Clear();

            //act
            _target.AuthenticationType = expectedValue;
            var actualValue = _target.AuthenticationType;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("AuthenticationType"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("UserAuthenticationSelected"));
            Assert.IsTrue(_changedProperties.Contains("IsUser"));
            Assert.IsTrue(_target.IsUser);
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestAuthenticationTypeWindows()
        {
            //arrange
            var expectedValue = AuthenticationType.Windows;
            _target.AuthenticationType = AuthenticationType.User;
            _changedProperties.Clear();

            //act
            _target.AuthenticationType = expectedValue;
            var actualValue = _target.AuthenticationType;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("AuthenticationType"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("UserAuthenticationSelected"));
            Assert.IsTrue(_changedProperties.Contains("IsWindows"));
            Assert.IsTrue(_target.IsWindows);
            Assert.IsFalse(_target.TestPassed);
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
        }

        [TestMethod]
        public void TestIsWindowsTrue()
        {
            //arrange
            _target.IsWindows = false;
            _changedProperties.Clear();

            //act
            _target.IsWindows = true;
            var actualValue = _target.IsWindows;

            //assert
            Assert.IsTrue(actualValue);
            Assert.AreEqual(AuthenticationType.Windows, _target.AuthenticationType);
            Assert.IsTrue(_changedProperties.Contains("IsWindows"));
        }

        [TestMethod]
        public void TestIsWindowsNoChanges()
        {
            //arrange
            _target.IsWindows = false;
            _changedProperties.Clear();

            //act
            _target.IsWindows = false;
           
            //assert
            Assert.IsFalse(_changedProperties.Any());
        }

        [TestMethod]
        public void TestIsUserTrue()
        {
            //arrange
            _target.IsWindows = false;
            _changedProperties.Clear();

            //act
            _target.IsUser = true;
            var actualValue = _target.IsUser;

            //assert
            Assert.IsTrue(actualValue);
            Assert.AreEqual(AuthenticationType.User, _target.AuthenticationType);
            Assert.IsTrue(_changedProperties.Contains("IsUser"));
        }

        [TestMethod]
        public void TestIsUserNoChanges()
        {
            //arrange
            _target.IsWindows = false;
            _changedProperties.Clear();

            //act
            _target.IsUser = false;

            //assert
            Assert.IsFalse(_changedProperties.Any());
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
        public void TestResourceXamlNonNullEmpty()
        {
            //arrange
            var expectedServerName = "someServerName";
            var expectedUserName = "someUserName";
            var expectedPassword = "somePassword";
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedValueMock = new Mock<IContextualResourceModel>();
            var source = new SharepointSource()
                          {
                              Server = expectedServerName,
                              UserName = expectedUserName,
                              Password = expectedPassword,
                              AuthenticationType = expectedAuthenticationType
                          };
            var xaml = new StringBuilder(source.ToXml().ToString());

            expectedValueMock.SetupGet(it => it.WorkflowXaml).Returns(xaml);
            _changedProperties.Clear();

            //act
            _target.Resource = expectedValueMock.Object;
            var actualValue = _target.Resource;

            //assert
            Assert.AreSame(expectedValueMock.Object, actualValue);
            Assert.AreEqual(expectedServerName, _target.ServerName);
            Assert.AreEqual(expectedUserName, _target.UserName);
            Assert.AreEqual(expectedPassword, _target.Password);
            Assert.AreEqual(expectedAuthenticationType, _target.AuthenticationType);
        }

        [TestMethod]
        public void TestResourceXamlNullEmpty()
        {
            //arrange
            var expectedServerName = "someServerName";
            var expectedUserName = "someUserName";
            var expectedPassword = "somePassword";
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedValueMock = new Mock<IContextualResourceModel>();
            var source = new SharepointSource()
            {
                Server = expectedServerName,
                UserName = expectedUserName,
                Password = expectedPassword,
                AuthenticationType = expectedAuthenticationType
            };
            var xaml = new StringBuilder(source.ToXml().ToString());


            expectedValueMock.SetupGet(it => it.WorkflowXaml).Returns(new StringBuilder());
            var resourceId = Guid.NewGuid();
            expectedValueMock.SetupGet(it => it.ID).Returns(resourceId);
            var resourceRepositoryMock = new Mock<IResourceRepository>();
            resourceRepositoryMock.Setup(
                it => it.FetchResourceDefinition(_environmentMock.Object, It.IsAny<Guid>(), resourceId, false))
                .Returns(new ExecuteMessage() { Message = xaml, HasError = false });
            _environmentMock.SetupGet(it => it.ResourceRepository).Returns(resourceRepositoryMock.Object);
            _changedProperties.Clear();

            //act
            _target.Resource = expectedValueMock.Object;
            var actualValue = _target.Resource;

            //assert
            Assert.AreSame(expectedValueMock.Object, actualValue);
            Assert.AreEqual(expectedServerName, _target.ServerName);
            Assert.AreEqual(expectedUserName, _target.UserName);
            Assert.AreEqual(expectedPassword, _target.Password);
            Assert.AreEqual(expectedAuthenticationType, _target.AuthenticationType);
        }

        [TestMethod]
        public void TestUIsSharepointOnline()
        {
            //arrange
            var expectedValue = true;
           
            //act
            _target.IsSharepointOnline = expectedValue;
            var actualValue = _target.IsSharepointOnline;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
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
        public void TestTestResult()
        {
            //arrange
            var expectedValue = "SomeResult";
            _target.TestComplete = false;
            _changedProperties.Clear();

            //act
            _target.TestResult = expectedValue;
            var actualValue = _target.TestResult;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_target.TestComplete);
            Assert.IsTrue(_changedProperties.Contains("TestResult"));
        }

        [TestMethod]
        public void TestTestResultFailed()
        {
            //arrange
            var expectedValue = "Failed";
            _target.TestComplete = false;
            _changedProperties.Clear();

            //act
            _target.TestResult = expectedValue;
            var actualValue = _target.TestResult;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsFalse(_target.TestComplete);
            Assert.IsTrue(_changedProperties.Contains("TestResult"));
        }

        [TestMethod]
        public void TestIsLoading()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.IsLoading = expectedValue;
            var actualValue = _target.IsLoading;

            //assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedProperties.Contains("IsLoading"));
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
            _target.AuthenticationType = AuthenticationType.Windows;

            //act
            var actualValue = _target.UserAuthenticationSelected;

            //assert
            Assert.IsFalse(actualValue);
        }

        #endregion Test properties

        #region Test methods

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
        public void TestSave()
        {
            //arrange
            var sourceMock = new Mock<ISharepointServerSource>();

            //act
            _target.Save(sourceMock.Object);

            //assert
            _updateManagerMock.Verify(it => it.Save(sourceMock.Object));
        }

        [TestMethod]
        public void TestSave_serverSourceNotNull()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedServerName = "someServerName";
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";

            var sourceName = "someSourceName";

            var expectedHeaderText = sourceName;
            var expectedHeader = sourceName + " *";

            _sharepointServerSourceMock.Setup(it => it.Name).Returns(sourceName);

            _targetSource.AuthenticationType = expectedAuthenticationType;
            _targetSource.Password = expectedPassword;
            _targetSource.UserName = expectedUserName;
            _targetSource.ServerName = expectedServerName;

            //act
            _targetSource.Save();

            //assert
            Assert.AreSame(_sharepointServerSourceMock.Object, _targetSource.Item);
            _sharepointServerSourceMock.VerifySet(it=>it.AuthenticationType = expectedAuthenticationType);
            _sharepointServerSourceMock.VerifySet(it => it.Server = expectedServerName);
            _sharepointServerSourceMock.VerifySet(it => it.Password = expectedPassword);
            _sharepointServerSourceMock.VerifySet(it => it.UserName = expectedUserName);
            Assert.AreEqual(expectedHeaderText, _targetSource.HeaderText);
            Assert.AreEqual(expectedHeader, _targetSource.Header);
            _updateManagerMock.Verify(it=>it.Save(_sharepointServerSourceMock.Object));
        }

        [TestMethod]
        public void TesSave_serverSourceNull()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedServerName = "someServerName";
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";
            var expectedName = "someExpectedName";
            var expectedHeaderText = expectedName;
            var expectedHeader = expectedName;
            var expectedPath = "somePath";
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(expectedPath, expectedName));
            _targetRequestServiceViewModel.AuthenticationType = expectedAuthenticationType;
            _targetRequestServiceViewModel.Password = expectedPassword;
            _targetRequestServiceViewModel.UserName = expectedUserName;
            _targetRequestServiceViewModel.ServerName = expectedServerName;
            _targetRequestServiceViewModel.ResourceName = expectedName;

            //act
            _targetRequestServiceViewModel.Save();

            //assert
            _requestServiceNameViewModelMock.Verify(it => it.ShowSaveDialog());
            Assert.IsInstanceOfType(_targetRequestServiceViewModel.Item, typeof(SharePointServiceSourceDefinition));
            _updateManagerMock.Verify(it => it.Save(_targetRequestServiceViewModel.Item));
            Assert.AreEqual(expectedAuthenticationType, _targetRequestServiceViewModel.Item.AuthenticationType);
            Assert.AreEqual(expectedServerName, _targetRequestServiceViewModel.Item.Server);
            Assert.AreEqual(expectedPassword, _targetRequestServiceViewModel.Item.Password);
            Assert.AreEqual(expectedName, _targetRequestServiceViewModel.Item.Name);
            Assert.AreNotEqual(Guid.Empty, _targetRequestServiceViewModel.Item.Id);
            _updateManagerMock.Verify(it => it.Save(It.IsAny<ISharepointServerSource>()));
            Assert.AreEqual(expectedHeaderText, _targetRequestServiceViewModel.HeaderText);
            Assert.AreEqual(expectedHeader, _targetRequestServiceViewModel.Header);
        }

        [TestMethod]
        public void TestFromModel()
        {
            //arrange
            var source = new Mock<ISharepointServerSource>();
            var expectedResourceName = "someName";
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedUserName = "userName";
            var expectedIsSharepointOnline = true;
            var expectedServerName = "someServerName";
            var expectedPassword = "somePassword";

            source.SetupGet(it => it.Name).Returns(expectedResourceName);
            source.SetupGet(it => it.Server).Returns(expectedServerName);
            source.SetupGet(it => it.AuthenticationType).Returns(expectedAuthenticationType);
            source.SetupGet(it => it.UserName).Returns(expectedUserName);
            source.SetupGet(it => it.IsSharepointOnline).Returns(expectedIsSharepointOnline);
            source.SetupGet(it => it.Password).Returns(expectedPassword);

            //act
            _target.FromModel(source.Object);

            //assert
            Assert.AreEqual(expectedResourceName, _target.ResourceName);
            Assert.AreEqual(expectedAuthenticationType, _target.AuthenticationType);
            Assert.AreEqual(expectedUserName, _target.UserName);
            Assert.AreEqual(expectedServerName, _target.ServerName);
            Assert.AreEqual(expectedPassword, _target.Password);
            Assert.AreEqual(expectedIsSharepointOnline, _target.IsSharepointOnline);
        }

        [TestMethod]
        public void TestToModelItemNotNull()
        {
            //arrange
            var serverSourceMock = new Mock<ISharepointServerSource>();
            _target.Item = serverSourceMock.Object;

            var expectedAuthenticationType = AuthenticationType.User;
            var expectedName = "someName";
            var expectedPassword = "somePassword";
            var expectedServer = "someServerName";
            var expectedId = Guid.NewGuid();
            var expectedUserName = "someUserName";
            var expectedPath = "somePath";

            serverSourceMock.SetupGet(it => it.Id).Returns(expectedId);

            _target.ResourceName = expectedName;
            _target.ServerName = expectedServer;
            _target.AuthenticationType = expectedAuthenticationType;
            _target.UserName = expectedUserName;
            _target.Password = expectedPassword;
            _target.Path = expectedPath;

            //act
            var value = _target.ToModel();
            var hashcode = value.GetHashCode();
            //assert
            Assert.AreEqual(expectedAuthenticationType, value.AuthenticationType);
            Assert.AreEqual(expectedServer, value.Server);
            Assert.AreEqual(expectedPassword, value.Password);
            Assert.AreEqual(expectedName, value.Name);
            Assert.AreEqual(expectedUserName, value.UserName);
            Assert.AreEqual(expectedId, value.Id);
            Assert.AreEqual(expectedPath, value.Path);
            Assert.AreEqual(hashcode, value.GetHashCode());
        }

        [TestMethod]
        public void TestToModelItemNullSourceNotNull()
        {
            //arrange
            var expectedAuthenticationType = AuthenticationType.User;
            var expectedServerName = "someServerName";
            var expectedPassword = "somePassword";
            var expectedUserName = "someUserName";

            _targetSource.AuthenticationType = expectedAuthenticationType;
            _targetSource.Password = expectedPassword;
            _targetSource.UserName = expectedUserName;
            _targetSource.ServerName = expectedServerName;
            _targetSource.Item = null;

            //act
            var result = _targetSource.ToModel();

            //assert
            Assert.AreSame(result, _targetSource.Item);
            Assert.AreSame(_sharepointServerSourceMock.Object, _targetSource.Item);
            _sharepointServerSourceMock.VerifySet(it => it.AuthenticationType = expectedAuthenticationType);
            _sharepointServerSourceMock.VerifySet(it => it.Server = expectedServerName);
            _sharepointServerSourceMock.VerifySet(it => it.Password = expectedPassword);
            _sharepointServerSourceMock.VerifySet(it => it.UserName = expectedUserName);
        }

        [TestMethod]
        public void TestCanTestServerNameEmpty()
        {
            //arrange
            _target.ServerName = "";

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestUserAuthenticationEmptyUserName()
        {
            //arrange
            _target.ServerName = "someServerName";
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
            _target.ServerName = "someServerName";
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
            _target.ServerName = "someServerName";
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
            _target.ServerName = "someServerName";
            _target.AuthenticationType = AuthenticationType.Windows;

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsTrue(result);
        }

        #endregion Test methods
    }
}