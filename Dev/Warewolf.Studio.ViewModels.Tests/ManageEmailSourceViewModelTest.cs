using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageEmailSourceViewModelTest
    {
        #region Fields

        private Mock<IManageEmailSourceModel> _updateManagerMock;
        private Mock<IRequestServiceNameViewModel> _requestServiceNameViewModelMock;
        private Task<IRequestServiceNameViewModel> _requestServiceNameViewModelTask;
        private Mock<IEmailServiceSource> _emailSource;
        private Mock<IEventAggregator> _aggregatorMock;

        private string _emailServiceSourceResourceName;

        private List<string> _changedProperties;
        private ManageEmailSourceViewModel _target;
        private List<string> _changedPropertiesSource;
        private ManageEmailSourceViewModel _targetSource;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateManagerMock = new Mock<IManageEmailSourceModel>();
            _requestServiceNameViewModelMock = new Mock<IRequestServiceNameViewModel>();
            _aggregatorMock = new Mock<IEventAggregator>();
            _emailSource = new Mock<IEmailServiceSource>();
            _emailServiceSourceResourceName = "emailServiceSourceResourceName";
            _emailSource.SetupGet(it => it.ResourceName).Returns(_emailServiceSourceResourceName);
            _requestServiceNameViewModelTask = Task.FromResult(_requestServiceNameViewModelMock.Object);

            _changedProperties = new List<string>();
            _target = new ManageEmailSourceViewModel(_updateManagerMock.Object, _requestServiceNameViewModelTask, _aggregatorMock.Object);
            _target.PropertyChanged += (sender, e) => { _changedProperties.Add(e.PropertyName); };

            _changedPropertiesSource = new List<string>();
            _updateManagerMock.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                .Returns(_emailSource.Object);
            _targetSource = new ManageEmailSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _emailSource.Object, new SynchronousAsyncWorker());
            _targetSource.PropertyChanged += (sender, e) => { _changedPropertiesSource.Add(e.PropertyName); };
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestUpdateManagerNull()
        {
            //act
            new ManageEmailSourceViewModel(null, _requestServiceNameViewModelTask, _aggregatorMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestAggregatorNull()
        {
            //act
            new ManageEmailSourceViewModel(_updateManagerMock.Object, _requestServiceNameViewModelTask, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestRequestServiceNameViewModelNull()
        {
            //act
            new ManageEmailSourceViewModel(_updateManagerMock.Object, null, _aggregatorMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_emailServiceSourceMockNull()
        {
            //act
            new ManageEmailSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, null, new SynchronousAsyncWorker());
        }

        #endregion Test construction

        #region Test commands

        [TestMethod]
        public void TestSendCommandCanExecute()
        {
            //arrange
            _target.HostName = "HostName";
            _target.UserName = "UserName";
            _target.Password = "Password";


            //act
            var result = _target.SendCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestSendCommandExecute()
        {
            //arrange
            _target.HostName = "HostName";
            _target.UserName = "UserName";
            _target.Password = "Password";

            //act
            _target.SendCommand.Execute(null);
        }

        [TestMethod]
        public void TestSendCommandExecute_emailServiceSource()
        {
            //arrange
            _target.HostName = "HostName";
            _target.UserName = "UserName";
            _target.Password = "Password";
            var emailId = Guid.NewGuid();
            _emailSource.SetupGet(it => it.Id).Returns(emailId);

            //act
            _targetSource.SendCommand.Execute(null);
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
        public void TestOkCommandExecute()
        {
            //act
            _target.OkCommand.Execute(null);

            //assert
            Assert.IsFalse(_target.TestPassed);
        }

     
        [TestMethod]
        public void TestOkCommandExecuteSource()
        {
            //arrange
            var expectedHostName = "HostName";
            var expectedPassword = "Password";
            var expectedUserName = "UserName";
            var expectedPort = 423423;
            var expectedTimeout = 314313;
            var expectedEnableSsl = true;
            var expectedEmailFrom = "EmailFrom";
            var expectedEmailTo = "EmailTo@example.com";
            var sourceResourceName = "sourceResourceName";
            var expectedHeaderText = sourceResourceName;
            var expectedHeader = sourceResourceName + " *";
            var expectedPath = "somePath";
            var expectedResourceName = "someResourceName";

            _targetSource.HostName = expectedHostName;
            _targetSource.Password = expectedPassword;
            _targetSource.UserName = expectedUserName;
            _targetSource.Port = expectedPort;
            _targetSource.Timeout = expectedTimeout;
            _targetSource.EnableSsl = expectedEnableSsl;
            _targetSource.EmailFrom = expectedEmailFrom;
            _targetSource.EmailTo = expectedEmailTo;
            _emailSource.SetupGet(it => it.ResourceName).Returns(sourceResourceName);
            var source = new Mock<IEmailServiceSource>();
            source.SetupGet(it => it.Path).Returns(expectedPath);
            source.SetupGet(it => it.ResourceName).Returns(expectedResourceName);
            _targetSource.Item = source.Object;

            //act
            _targetSource.OkCommand.Execute(null);

            //assert
            Assert.IsFalse(_targetSource.TestPassed);
            Assert.AreSame(_emailSource.Object, _targetSource.Item);
            _emailSource.VerifySet(it => it.HostName = expectedHostName);
            _emailSource.VerifySet(it => it.UserName = expectedUserName);
            _emailSource.VerifySet(it => it.Password = expectedPassword);
            _emailSource.VerifySet(it => it.Port = expectedPort);
            _emailSource.VerifySet(it => it.Timeout = expectedTimeout);
            _emailSource.VerifySet(it => it.EnableSsl = expectedEnableSsl);
            _emailSource.VerifySet(it => it.EmailFrom = expectedEmailFrom);
            _emailSource.VerifySet(it => it.EmailTo = expectedEmailTo);
            _emailSource.VerifySet(it => it.Path = expectedPath);
            _emailSource.VerifySet(it => it.ResourceName = expectedResourceName);
            Assert.AreEqual(expectedHeaderText, _targetSource.HeaderText);
            Assert.AreEqual(expectedHeader, _targetSource.Header);
            _updateManagerMock.Verify(it => it.Save(_emailSource.Object));
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        public void TestName()
        {
            //arrange
            var expectedValue = "expectedValue";

            //act
            _target.Name = expectedValue;
            var value = _target.Name;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.AreEqual(expectedValue, _target.ResourceName);
        }

        [TestMethod]
        public void TestRequestServiceNameViewModel()
        {
            //arrange
            var valueMock = new Mock<IRequestServiceNameViewModel>();
            var expectedValue = Task.FromResult(valueMock.Object);

            //act
            _target.RequestServiceNameViewModel = expectedValue;
            var value = _target.RequestServiceNameViewModel;

            //assert
            Assert.AreSame(expectedValue, value);
        }

        [TestMethod]
        public void TestResourceName()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _target.ResourceName = expectedValue;
            var value = _target.ResourceName;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains(expectedValue));
        }

        [TestMethod]
        public void TestHostName()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _target.HostName = expectedValue;
            var value = _target.HostName;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("HostName"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestUserName()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _target.UserName = expectedValue;
            var value = _target.UserName;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.AreEqual(expectedValue, _target.EmailFrom);
            Assert.IsTrue(_changedProperties.Contains("UserName"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestPassword()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _target.Password = expectedValue;
            var value = _target.Password;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Password"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEmailFromNotEmail()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _target.EmailFrom = expectedValue;
            var value = _target.EmailFrom;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsFalse(_target.EnableSend);
            Assert.IsTrue(_changedProperties.Contains("EmailFrom"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("EnableSend"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEmailFromEmailEmailToNull()
        {
            //arrange
            var expectedValue = "expectedvalue@example.com";
            _changedProperties.Clear();
            _target.EmailTo = null;

            //act
            _target.EmailFrom = expectedValue;
            var value = _target.EmailFrom;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsFalse(_target.EnableSend);
            Assert.IsTrue(_changedProperties.Contains("EmailFrom"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("EnableSend"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEmailFromEmailEmailToNotEmail()
        {
            //arrange
            var expectedValue = "expectedvalue@example.com";
            _changedProperties.Clear();
            _target.EmailTo = "SomeEmailTo";

            //act
            _target.EmailFrom = expectedValue;
            var value = _target.EmailFrom;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsFalse(_target.EnableSend);
            Assert.IsTrue(_changedProperties.Contains("EmailFrom"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("EnableSend"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEmailFromEmailEmailToCorrect()
        {
            //arrange
            var expectedValue = "expectedvalue@example.com";
            _changedProperties.Clear();
            _target.EmailTo = "someemailto@example.com";

            //act
            _target.EmailFrom = expectedValue;
            var value = _target.EmailFrom;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_target.EnableSend);
            Assert.IsTrue(_changedProperties.Contains("EmailFrom"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("EnableSend"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEmailToNotEmail()
        {
            //arrange
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            //act
            _target.EmailTo = expectedValue;
            var value = _target.EmailTo;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsFalse(_target.EnableSend);
            Assert.IsTrue(_changedProperties.Contains("EmailTo"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("EnableSend"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEmailToEmailEmailFromNull()
        {
            //arrange
            var expectedValue = "expectedvalue@example.com";
            _changedProperties.Clear();
            _target.EmailFrom = null;

            //act
            _target.EmailTo = expectedValue;
            var value = _target.EmailTo;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsFalse(_target.EnableSend);
            Assert.IsTrue(_changedProperties.Contains("EmailTo"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("EnableSend"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEmailToEmailEmailFromNotEmail()
        {
            //arrange
            var expectedValue = "expectedvalue@example.com";
            _changedProperties.Clear();
            _target.EmailFrom = "SomeEmailTo";

            //act
            _target.EmailTo = expectedValue;
            var value = _target.EmailTo;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsFalse(_target.EnableSend);
            Assert.IsTrue(_changedProperties.Contains("EmailTo"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("EnableSend"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEmailToEmailEmailFromCorrect()
        {
            //arrange
            var expectedValue = "expectedvalue@example.com";
            _changedProperties.Clear();
            _target.EmailTo = "someemailto@example.com";

            //act
            _target.EmailFrom = expectedValue;
            var value = _target.EmailFrom;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_target.EnableSend);
            Assert.IsTrue(_changedProperties.Contains("EmailTo"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("EnableSend"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestPort()
        {
            //arrange
            var expectedValue = 423432;
            _changedProperties.Clear();

            //act
            _target.Port = expectedValue;
            var value = _target.Port;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Port"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestTestPassed()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.TestPassed = expectedValue;
            var value = _target.TestPassed;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("TestPassed"));
        }

        [TestMethod]
        public void TestHeaderText()
        {
            //arrange
            var expectedValue = "HeaderText";
            _changedProperties.Clear();

            //act
            _target.HeaderText = expectedValue;
            var value = _target.HeaderText;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("HeaderText"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
        }

        [TestMethod]
        public void TestTimeout()
        {
            //arrange
            var expectedValue = 42432;
            _changedProperties.Clear();

            //act
            _target.Timeout = expectedValue;
            var value = _target.Timeout;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Timeout"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEnableSsl()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.EnableSsl = expectedValue;
            var value = _target.EnableSsl;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("EnableSsl"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("TestMessage"));
            Assert.IsTrue(string.IsNullOrEmpty(_target.TestMessage));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEnableSslYes()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.EnableSslYes = expectedValue;
            var value = _target.EnableSslYes;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_target.EnableSsl);
            Assert.IsTrue(_changedProperties.Contains("EnableSslYes"));
            Assert.IsTrue(_changedProperties.Contains("EnableSsl"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestEnableSslNo()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.EnableSslNo = expectedValue;
            var value = _target.EnableSslNo;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsFalse(_target.EnableSsl);
            Assert.IsTrue(_changedProperties.Contains("EnableSslNo"));
            Assert.IsTrue(_changedProperties.Contains("EnableSsl"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsFalse(_target.TestPassed);
        }

        [TestMethod]
        public void TestTestFailed()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.TestFailed = expectedValue;
            var value = _target.TestFailed;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("TestFailed"));
        }

        [TestMethod]
        public void TestEnableSend()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.EnableSend = expectedValue;
            var value = _target.EnableSend;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("EnableSend"));
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        public void TestToModelItemNull()
        {
            //arrange
            _target.Item = null;
            var expectedHostName = "HostName";
            var expectedPassword = "Password";
            var expectedUserName = "UserName";
            var expectedPort = 423423;
            var expectedTimeout = 314313;
            var expectedEnableSsl = true;
            var expectedEmailFrom = "EmailFrom";
            var expectedEmailTo = "EmailTo@example.com";
            var expectedPath = "";
            var expectedResourceName = "ResourceName";

            _target.HostName = expectedHostName;
            _target.Password = expectedPassword;
            _target.UserName = expectedUserName;
            _target.Port = expectedPort;
            _target.Timeout = expectedTimeout;
            _target.EnableSsl = expectedEnableSsl;
            _target.EmailFrom = expectedEmailFrom;
            _target.EmailTo = expectedEmailTo;

            //act
            var result = _target.ToModel();
            result.Path = expectedPath;
            result.ResourceName = expectedResourceName;
            var hashcode = result.GetHashCode();

            //assert
            Assert.IsNotNull(result);
            Assert.AreSame(result, _target.Item);
            Assert.AreEqual(expectedHostName, result.HostName);
            Assert.AreEqual(expectedPassword, result.Password);
            Assert.AreEqual(expectedUserName, result.UserName);
            Assert.AreEqual(expectedPort, result.Port);
            Assert.AreEqual(expectedResourceName, result.ResourceName);
            Assert.AreEqual(expectedPath, result.Path);
            Assert.AreEqual(hashcode, result.GetHashCode());
            Assert.AreEqual(expectedTimeout, result.Timeout);
            Assert.AreEqual(expectedEnableSsl, result.EnableSsl);
            Assert.AreEqual(expectedEmailFrom, result.EmailFrom);
            Assert.AreEqual(expectedEmailTo, result.EmailTo);
            Assert.AreNotEqual(Guid.Empty, result.Id);
        }

        [TestMethod]
        public void TestToModelItemNotNull_emailServiceSource()
        {
            //arrange
            var itemMock = new Mock<IEmailServiceSource>();
            _targetSource.Item = itemMock.Object;
            var expectedHostName = "HostName";
            var expectedPassword = "Password";
            var expectedUserName = "UserName";
            var expectedPort = 423423;
            var expectedTimeout = 314313;
            var expectedEnableSsl = true;
            var expectedEmailFrom = "EmailFrom";
            var expectedEmailTo = "EmailTo@example.com";
            var expectedId = Guid.NewGuid();

            _targetSource.HostName = expectedHostName;
            _targetSource.Password = expectedPassword;
            _targetSource.UserName = expectedUserName;
            _targetSource.Port = expectedPort;
            _targetSource.Timeout = expectedTimeout;
            _targetSource.EnableSsl = expectedEnableSsl;
            _targetSource.EmailFrom = expectedEmailFrom;
            _targetSource.EmailTo = expectedEmailTo;
            _emailSource.SetupGet(it => it.Id).Returns(expectedId);

            //act
            var result = _targetSource.ToModel();

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedHostName, result.HostName);
            Assert.AreEqual(expectedPassword, result.Password);
            Assert.AreEqual(expectedUserName, result.UserName);
            Assert.AreEqual(expectedPort, result.Port);
            Assert.AreEqual(expectedTimeout, result.Timeout);
            Assert.AreEqual(expectedEnableSsl, result.EnableSsl);
            Assert.AreEqual(expectedEmailFrom, result.EmailFrom);
            Assert.AreEqual(expectedEmailTo, result.EmailTo);
            Assert.AreEqual(expectedId, result.Id);
        }

        [TestMethod]
        public void TestToModelItemNotNull()
        {
            //arrange
            var itemMock = new Mock<IEmailServiceSource>();
            _target.Item = itemMock.Object;
            var expectedHostName = "HostName";
            var expectedPassword = "Password";
            var expectedUserName = "UserName";
            var expectedPort = 423423;
            var expectedTimeout = 314313;
            var expectedEnableSsl = true;
            var expectedEmailFrom = "EmailFrom";
            var expectedEmailTo = "EmailTo@example.com";

            _target.HostName = expectedHostName;
            _target.Password = expectedPassword;
            _target.UserName = expectedUserName;
            _target.Port = expectedPort;
            _target.Timeout = expectedTimeout;
            _target.EnableSsl = expectedEnableSsl;
            _target.EmailFrom = expectedEmailFrom;
            _target.EmailTo = expectedEmailTo;

            //act
            var result = _target.ToModel();

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedHostName, result.HostName);
            Assert.AreEqual(expectedPassword, result.Password);
            Assert.AreEqual(expectedUserName, result.UserName);
            Assert.AreEqual(expectedPort, result.Port);
            Assert.AreEqual(expectedTimeout, result.Timeout);
            Assert.AreEqual(expectedEnableSsl, result.EnableSsl);
            Assert.AreEqual(expectedEmailFrom, result.EmailFrom);
            Assert.AreEqual(expectedEmailTo, result.EmailTo);
            Assert.AreNotEqual(Guid.Empty, result.Id);
        }

        [TestMethod]
        public void TestSaveSource()
        {
            //arrange
            var expectedHostName = "HostName";
            var expectedPassword = "Password";
            var expectedUserName = "UserName";
            var expectedPort = 423423;
            var expectedTimeout = 314313;
            var expectedEnableSsl = true;
            var expectedEmailFrom = "EmailFrom";
            var expectedEmailTo = "EmailTo@example.com";
            var sourceResourceName = "sourceResourceName";
            var expectedHeaderText = sourceResourceName;
            var expectedHeader = sourceResourceName + " *";
            var expectedPath = "somePath";
            var expectedResourceName = "someResourceName";

            _targetSource.HostName = expectedHostName;
            _targetSource.Password = expectedPassword;
            _targetSource.UserName = expectedUserName;
            _targetSource.Port = expectedPort;
            _targetSource.Timeout = expectedTimeout;
            _targetSource.EnableSsl = expectedEnableSsl;
            _targetSource.EmailFrom = expectedEmailFrom;
            _targetSource.EmailTo = expectedEmailTo;
            _emailSource.SetupGet(it => it.ResourceName).Returns(sourceResourceName);
            var emailServiceSourceMock = new Mock<IEmailServiceSource>();
            emailServiceSourceMock.SetupGet(it => it.Path).Returns(expectedPath);
            emailServiceSourceMock.SetupGet(it => it.ResourceName).Returns(expectedResourceName);
            _targetSource.Item = emailServiceSourceMock.Object;

            //act
            _targetSource.Save();

            //assert
            Assert.IsFalse(_targetSource.TestPassed);
            Assert.AreSame(_emailSource.Object, _targetSource.Item);
            _emailSource.VerifySet(it => it.HostName = expectedHostName);
            _emailSource.VerifySet(it => it.UserName = expectedUserName);
            _emailSource.VerifySet(it => it.Password = expectedPassword);
            _emailSource.VerifySet(it => it.Port = expectedPort);
            _emailSource.VerifySet(it => it.Timeout = expectedTimeout);
            _emailSource.VerifySet(it => it.EnableSsl = expectedEnableSsl);
            _emailSource.VerifySet(it => it.EmailFrom = expectedEmailFrom);
            _emailSource.VerifySet(it => it.EmailTo = expectedEmailTo);
            _emailSource.VerifySet(it => it.Path = expectedPath);
            _emailSource.VerifySet(it => it.ResourceName = expectedResourceName);
            Assert.AreEqual(expectedHeaderText, _targetSource.HeaderText);
            Assert.AreEqual(expectedHeader, _targetSource.Header);
            _updateManagerMock.Verify(it => it.Save(_emailSource.Object));
        }

        [TestMethod]
        public void TestSave()
        {
            //act
            _target.Save();

            //assert
            Assert.IsFalse(_target.TestPassed);
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
        public void TestCanSaveTrue()
        {
            //arrange
            _target.TestPassed = true;

            //act
            var result = _target.CanSave();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCanSaveFalse()
        {
            //arrange
            _target.TestPassed = false;

            //act
            var result = _target.CanSave();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanTestTrue()
        {
            //arrange
            _target.HostName = "HostName";
            _target.UserName = "UserName";
            _target.Password = "Password";

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCanTestTrueHostNameEmpty()
        {
            //arrange
            _target.HostName = "";
            _target.UserName = "UserName";
            _target.Password = "Password";

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCanTestTrueHostNameUserNameEmpty()
        {
            //arrange
            _target.HostName = "";
            _target.UserName = "";
            _target.Password = "Password";

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCanTestTrueHostNameUserNamePasswordEmpty()
        {
            //arrange
            _target.HostName = "";
            _target.UserName = "";
            _target.Password = "";

            //act
            var result = _target.CanTest();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestFromModel()
        {
            //arrange
            var expectedHostName = "expectedHostName";
            var expectedUserName = "expectedUserName";
            var expectedPassword = "expectedPassword";
            var expectedEnableSsl = true;
            var expectedPort = 1342;
            var expectedTimeout = 43242;
            var expectedEmailFrom = "kidjwgfwg";
            var expectedEmailTo = "gwgesgfewg";
            var expectedResourceName = "expectedResourceName";
            var emailServiceSourceMock = new Mock<IEmailServiceSource>();
            emailServiceSourceMock.SetupGet(it => it.HostName).Returns(expectedHostName);
            emailServiceSourceMock.SetupGet(it => it.UserName).Returns(expectedUserName);
            emailServiceSourceMock.SetupGet(it => it.Password).Returns(expectedPassword);
            emailServiceSourceMock.SetupGet(it => it.EnableSsl).Returns(expectedEnableSsl);
            emailServiceSourceMock.SetupGet(it => it.Port).Returns(expectedPort);
            emailServiceSourceMock.SetupGet(it => it.Timeout).Returns(expectedTimeout);
            emailServiceSourceMock.SetupGet(it => it.EmailFrom).Returns(expectedEmailFrom);
            emailServiceSourceMock.SetupGet(it => it.EmailTo).Returns(expectedEmailTo);
            emailServiceSourceMock.SetupGet(it => it.ResourceName).Returns(expectedResourceName);

            //act
            _target.FromModel(emailServiceSourceMock.Object);

            //assert
            Assert.AreEqual(expectedHostName, _target.HostName);
            Assert.AreEqual(expectedUserName, _target.UserName);
            Assert.AreEqual(expectedPassword, _target.Password);
            Assert.AreEqual(expectedEnableSsl, _target.EnableSsl);
            Assert.IsTrue(_target.EnableSslYes);
            Assert.AreEqual(expectedPort, _target.Port);
            Assert.AreEqual(expectedTimeout, _target.Timeout);
            Assert.AreEqual(expectedEmailFrom, _target.EmailFrom);
            Assert.AreEqual(expectedEmailTo, _target.EmailTo);
            Assert.AreEqual(expectedResourceName, _target.ResourceName);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void TestDispose()
        {
            var vm = new ManageEmailSourceViewModel();
            var ns = new Mock<IRequestServiceNameViewModel>();
            var t = new Task<IRequestServiceNameViewModel>(() => ns.Object);
            t.Start();
            vm.RequestServiceNameViewModel = t;

            vm.Dispose();
            ns.Verify(a => a.Dispose());
        }

        #endregion Test methods
    }
}