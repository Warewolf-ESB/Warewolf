/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Dev2.Threading;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ElasticsearchSourceViewModelTests
    {
        Mock<IElasticsearchSourceModel> _elasticsearchSourceModel;
        Mock<IRequestServiceNameViewModel> _requestServiceNameViewModel;
        Task<IRequestServiceNameViewModel> _requestServiceNameViewModelTask;
        Mock<IElasticsearchSourceDefinition> _elasticsearchSourceDefinition;
        List<string> _changedProperties;
        ElasticsearchSourceViewModel _elasticsearchourceViewModelWithTask;
        ElasticsearchSourceViewModel _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition;
        Mock<IAsyncWorker> _asyncWorkerMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _elasticsearchSourceModel = new Mock<IElasticsearchSourceModel>();
            _requestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            _elasticsearchSourceDefinition = new Mock<IElasticsearchSourceDefinition>();
            _requestServiceNameViewModelTask = Task.FromResult(_requestServiceNameViewModel.Object);
            _changedProperties = new List<string>();
            _elasticsearchourceViewModelWithTask = new ElasticsearchSourceViewModel(_elasticsearchSourceModel.Object, _requestServiceNameViewModelTask);
            _elasticsearchourceViewModelWithTask.PropertyChanged += (sender, e) => { _changedProperties.Add(e.PropertyName); };
            _asyncWorkerMock = new Mock<IAsyncWorker>();
            _elasticsearchSourceModel.Setup(model => model.FetchSource(It.IsAny<Guid>())).Returns(_elasticsearchSourceDefinition.Object);
            _asyncWorkerMock.Setup(worker =>
                    worker.Start(
                        It.IsAny<Func<IElasticsearchSourceDefinition>>(),
                        It.IsAny<Action<IElasticsearchSourceDefinition>>()))
                .Callback<Func<IElasticsearchSourceDefinition>, Action<IElasticsearchSourceDefinition>>(
                    (func, action) =>
                    {
                        var dbSource = func.Invoke();
                        action?.Invoke(dbSource);
                    });
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition = new ElasticsearchSourceViewModel(
                _elasticsearchSourceModel.Object, _elasticsearchSourceDefinition.Object, _asyncWorkerMock.Object);
        }


        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ElasticsearchSourceViewModel_Constructor_Null_IElasticsearchSourceModel_ThrowsException()
        {
            new ElasticsearchSourceViewModel(null, _requestServiceNameViewModelTask);
        }


        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ElasticsearchSourceViewModel_Constructor_Null_IRequestServiceNameViewModelTask_ThrowsException()
        {
            //arrange
            _requestServiceNameViewModelTask = null;

            //act
            new ElasticsearchSourceViewModel(_elasticsearchSourceModel.Object, _requestServiceNameViewModelTask);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ElasticsearchSourceViewModel_Constructor_Null_ElasticsearchSourceDefinition_ThrowsException()
        {
            ElasticsearchSourceDefinition elasticsearchSourceDefinition = null;

            new ElasticsearchSourceViewModel(_elasticsearchSourceModel.Object, elasticsearchSourceDefinition, new SynchronousAsyncWorker());
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void
            ElasticsearchSourceViewModel_Constructor_IElasticsearchSourceModel_IRequestServiceNameViewModel_IsNotNull()
        {
            var source =
                new ElasticsearchSourceViewModel(_elasticsearchSourceModel.Object, _requestServiceNameViewModelTask);
            Assert.IsNotNull(source);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void
            ElasticsearchSourceViewModel_Constructor_IElasticsearchSourceModel_ElasticsearchSourceDefinition_IAsyncWorker_IsNotNull()
        {
            var source = new ElasticsearchSourceViewModel(_elasticsearchSourceModel.Object, _elasticsearchSourceDefinition.Object, new SynchronousAsyncWorker());
            Assert.IsNotNull(source);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_CanExecute()
        {
            _elasticsearchourceViewModelWithTask.Testing = false;
            _elasticsearchourceViewModelWithTask.HostName = "HostName";
            _elasticsearchourceViewModelWithTask.Port = "9200";
            _elasticsearchourceViewModelWithTask.Username = "Username";
            _elasticsearchourceViewModelWithTask.Password = "Password";
            _elasticsearchourceViewModelWithTask.AuthenticationType = AuthenticationType.Anonymous;

            var result = _elasticsearchourceViewModelWithTask.TestCommand.CanExecute(null);

            Assert.IsTrue(result);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestOkCommand_CanExecute()
        {
            _elasticsearchourceViewModelWithTask.TestPassed = true;
            var result = _elasticsearchourceViewModelWithTask.OkCommand.CanExecute(null);
            Assert.IsTrue(result);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestCancelTestCommand_CanExecute()
        {
            var result = _elasticsearchourceViewModelWithTask.CancelTestCommand.CanExecute(null);
            Assert.IsFalse(result);
        }

        [TestMethod, Timeout(60000)]
        public void ElasticsearchSourceViewModel_TestCancelTestCommand_Execute()
        {
            _elasticsearchourceViewModelWithTask.CancelTestCommand.Execute(null);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestOkCommand_Execute()
        {
            _elasticsearchourceViewModelWithTask.OkCommand.Execute(null);
            Assert.IsFalse(_elasticsearchourceViewModelWithTask.TestPassed);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestOkCommand_Execute_SaveSource()
        {
            var expectedResourceName = "ResourceName";
            var expectedResourcePath = "ResourcePath";
            var expectedHeader = expectedResourceName + " *";
            var expectedHostName = "HostName";
            var expectedPort = "9200";
            var expectedUsername = "UserName";
            var expectedPassword = "Password";

            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.HostName = expectedHostName;
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Port = expectedPort;
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Username = expectedUsername;
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Password = expectedPassword;
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.AuthenticationType = AuthenticationType.Password;

            _elasticsearchSourceDefinition.SetupGet(it => it.Path).Returns(expectedResourcePath);
            _elasticsearchSourceDefinition.SetupGet(it => it.Name).Returns(expectedResourceName);
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Item = _elasticsearchSourceDefinition.Object;

            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.OkCommand.Execute(null);

            //assert
            Assert.IsFalse(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.TestPassed);
            Assert.AreSame(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Item, _elasticsearchSourceDefinition.Object);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.HeaderText, expectedResourceName);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Header, expectedHeader);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.HostName, expectedHostName);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Port, expectedPort);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Username, expectedUsername);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Password, expectedPassword);

            _elasticsearchSourceDefinition.VerifySet(x => x.HostName = expectedHostName);
            _elasticsearchSourceDefinition.VerifySet(x => x.Port = expectedPort);
            _elasticsearchSourceDefinition.VerifySet(x => x.Username = expectedUsername);
            _elasticsearchSourceDefinition.VerifySet(x => x.Password = expectedPassword);
            _elasticsearchSourceDefinition.VerifySet(x => x.AuthenticationType = AuthenticationType.Password);
            _elasticsearchSourceModel.Verify(x => x.Save(_elasticsearchSourceDefinition.Object));
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestName_AreEqual()
        {
            var expectedValue = "expectedValue";

            _elasticsearchourceViewModelWithTask.Name = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.Name;

            Assert.AreEqual(expectedValue, value);
            Assert.AreEqual(expectedValue, _elasticsearchourceViewModelWithTask.ResourceName);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_RequestServiceNameViewModel_AreSame()
        {
            var valueMock = new Mock<IRequestServiceNameViewModel>();
            var expectedValue = Task.FromResult(valueMock.Object);

            _elasticsearchourceViewModelWithTask.RequestServiceNameViewModel = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.RequestServiceNameViewModel;

            Assert.AreSame(expectedValue, value);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestResourceName()
        {
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            _elasticsearchourceViewModelWithTask.ResourceName = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.ResourceName;


            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains(expectedValue));
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestHostName_Success()
        {
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            _elasticsearchourceViewModelWithTask.HostName = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.HostName;

            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("HostName"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_elasticsearchourceViewModelWithTask.TestMessage));
            Assert.IsFalse(_elasticsearchourceViewModelWithTask.TestPassed);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestPort_Success()
        {
            var expectedValue = "1234";
            _changedProperties.Clear();

            _elasticsearchourceViewModelWithTask.Port = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.Port;

            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Port"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_elasticsearchourceViewModelWithTask.TestMessage));
            Assert.IsFalse(_elasticsearchourceViewModelWithTask.TestPassed);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestUsername()
        {
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            _elasticsearchourceViewModelWithTask.Username = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.Username;

            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Username"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_elasticsearchourceViewModelWithTask.TestMessage));
            Assert.IsFalse(_elasticsearchourceViewModelWithTask.TestPassed);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestPassword()
        {
            var expectedValue = "expectedValue";
            _changedProperties.Clear();

            _elasticsearchourceViewModelWithTask.Password = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.Password;

            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Password"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_elasticsearchourceViewModelWithTask.TestMessage));
            Assert.IsFalse(_elasticsearchourceViewModelWithTask.TestPassed);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestAuthenticationType_Anonymous()
        {
            var expectedValue = AuthenticationType.Anonymous;
            _changedProperties.Clear();

            _elasticsearchourceViewModelWithTask.AuthenticationType = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.AuthenticationType;

            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("AuthenticationType"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_elasticsearchourceViewModelWithTask.TestMessage));
            Assert.IsFalse(_elasticsearchourceViewModelWithTask.TestPassed);
        }
        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestAuthenticationType_Password()
        {
            var expectedValue = AuthenticationType.Password;
            _changedProperties.Clear();

            _elasticsearchourceViewModelWithTask.AuthenticationType = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.AuthenticationType;

            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("AuthenticationType"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(string.IsNullOrEmpty(_elasticsearchourceViewModelWithTask.TestMessage));
            Assert.IsFalse(_elasticsearchourceViewModelWithTask.TestPassed);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestTestPassed()
        {
            var expectedValue = true;
            _changedProperties.Clear();

            _elasticsearchourceViewModelWithTask.TestPassed = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.TestPassed;

            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("TestPassed"));
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestHeaderText()
        {
            var expectedValue = "HeaderText";
            _changedProperties.Clear();

            _elasticsearchourceViewModelWithTask.HeaderText = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.HeaderText;

            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("HeaderText"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestTestFailed()
        {
            var expectedValue = true;
            _changedProperties.Clear();

            _elasticsearchourceViewModelWithTask.TestFailed = expectedValue;
            var value = _elasticsearchourceViewModelWithTask.TestFailed;

            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("TestFailed"));
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestToModelItemNull()
        {
            //arrange
            var expectedHostName = "HostName";
            var expectedPort = "9200";
            var expectedUsername = "Username";
            var expectedPassword = "Password";
            var expectedAuthenticationType = AuthenticationType.Password;

            _elasticsearchourceViewModelWithTask.HostName = expectedHostName;
            _elasticsearchourceViewModelWithTask.Port = expectedPort;
            _elasticsearchourceViewModelWithTask.Username = expectedUsername;
            _elasticsearchourceViewModelWithTask.Password = expectedPassword;
            _elasticsearchourceViewModelWithTask.AuthenticationType = expectedAuthenticationType;
            _elasticsearchourceViewModelWithTask.Item = null;

            var result = _elasticsearchourceViewModelWithTask.ToModel();

            Assert.IsNotNull(result);
            Assert.AreSame(result, _elasticsearchourceViewModelWithTask.Item);
            Assert.AreEqual(expectedHostName, result.HostName);
            Assert.AreEqual(expectedPort, result.Port);
            Assert.AreEqual(expectedUsername, result.Username);
            Assert.AreEqual(expectedPassword, result.Password);
            Assert.AreEqual(expectedAuthenticationType, result.AuthenticationType);
            Assert.AreNotEqual(Guid.Empty, result.Id);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestToModelItemNotNull()
        {
            //arrange
            var expectedHostName = "HostName";
            var expectedPort = "9200";
            var expectedUsername = "Username";
            var expectedPassword = "Password";
            var expectedAuthenticationType = AuthenticationType.Password;

            _elasticsearchourceViewModelWithTask.HostName = expectedHostName;
            _elasticsearchourceViewModelWithTask.Port = expectedPort;
            _elasticsearchourceViewModelWithTask.Username = expectedUsername;
            _elasticsearchourceViewModelWithTask.Password = expectedPassword;
            _elasticsearchourceViewModelWithTask.AuthenticationType = expectedAuthenticationType;
            _elasticsearchourceViewModelWithTask.Item = _elasticsearchSourceDefinition.Object;

            //act
            var result = _elasticsearchourceViewModelWithTask.ToModel();

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedHostName, result.HostName);
            Assert.AreEqual(expectedPort, result.Port);
            Assert.AreEqual(expectedUsername, result.Username);
            Assert.AreEqual(expectedPassword, result.Password);
            Assert.AreEqual(expectedAuthenticationType, result.AuthenticationType);
            Assert.AreEqual(Guid.Empty, result.Id);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestSaveSource()
        {
            var expectedResourceName = "ResourceName";
            var expectedResourcePath = "ResourcePath";
            var expectedHeader = expectedResourceName + " *";
            var expectedHostName = "HostName";
            var expectedPort = "9200";
            var expectedUsername = "Username";
            var expectedPassword = "Password";
            var expectedAuthenticationType = AuthenticationType.Password;

            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.HostName = expectedHostName;
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Port = expectedPort;
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Username = expectedUsername;
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Password = expectedPassword;
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.AuthenticationType = expectedAuthenticationType;
            _elasticsearchSourceDefinition.SetupGet(it => it.Path).Returns(expectedResourcePath);
            _elasticsearchSourceDefinition.SetupGet(it => it.Name).Returns(expectedResourceName);
            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Item = _elasticsearchSourceDefinition.Object;

            _elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Save();

            //assert
            Assert.IsFalse(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.TestPassed);
            Assert.AreSame(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Item, _elasticsearchSourceDefinition.Object);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.HeaderText, expectedResourceName);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Header, expectedHeader);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.HostName, expectedHostName);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Port, expectedPort);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Username, expectedUsername);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.Password, expectedPassword);
            Assert.AreEqual(_elasticsearchSourceViewModelWithElasticsearchServiceSourceDefinition.AuthenticationType, expectedAuthenticationType);

            _elasticsearchSourceDefinition.VerifySet(x => x.HostName = expectedHostName);
            _elasticsearchSourceDefinition.VerifySet(x => x.Port = expectedPort);
            _elasticsearchSourceDefinition.VerifySet(x => x.Username = expectedUsername);
            _elasticsearchSourceDefinition.VerifySet(x => x.Password = expectedPassword);
            _elasticsearchSourceDefinition.VerifySet(x => x.AuthenticationType = expectedAuthenticationType);
            _elasticsearchSourceModel.Verify(x => x.Save(_elasticsearchSourceDefinition.Object));
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestSaveNewSource()
        {
            var expectedResourceName = "ResourceName";
            var expectedResourcePath = "ResourcePath";

            _requestServiceNameViewModel.Setup(x => x.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModel.SetupGet(it => it.ResourceName).Returns(new ResourceName(expectedResourcePath, expectedResourceName));

            _elasticsearchourceViewModelWithTask.Save();

            Assert.IsFalse(_elasticsearchourceViewModelWithTask.TestPassed);
            Assert.AreEqual(_elasticsearchourceViewModelWithTask.HeaderText, expectedResourceName);
            Assert.AreEqual(_elasticsearchourceViewModelWithTask.ResourceName, expectedResourceName);
            _elasticsearchSourceModel.Verify(x => x.Save(It.IsAny<ElasticsearchSourceDefinition>()));
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestSave()
        {
            _elasticsearchourceViewModelWithTask.Save();
            Assert.IsFalse(_elasticsearchourceViewModelWithTask.TestPassed);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestUpdateHelpDescriptor()
        {
            var helpText = "helpText";
            var mainViewModelMock = new Mock<IShellViewModel>();
            var helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);

            _elasticsearchourceViewModelWithTask.UpdateHelpDescriptor(helpText);
            helpViewModelMock.Verify(it => it.UpdateHelpText(helpText));
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestCanSaveTrue()
        {
            _elasticsearchourceViewModelWithTask.TestPassed = true;
            var result = _elasticsearchourceViewModelWithTask.CanSave();
            Assert.IsTrue(result);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestCanSaveFalse()
        {
            _elasticsearchourceViewModelWithTask.TestPassed = false;
            var result = _elasticsearchourceViewModelWithTask.CanSave();
            Assert.IsFalse(result);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestCanTestTrue()
        {
            _elasticsearchourceViewModelWithTask.HostName = "HostName";
            _elasticsearchourceViewModelWithTask.Username = "Username";
            _elasticsearchourceViewModelWithTask.Password = "Password";
            _elasticsearchourceViewModelWithTask.AuthenticationType = AuthenticationType.Password;

            var result = _elasticsearchourceViewModelWithTask.CanTest();

            Assert.IsTrue(result);
        }

        [TestMethod, Timeout(60000)]
        public void ElasticsearchSourceViewModel_TestCanTestFalseHostNameEmpty()
        {
            _elasticsearchourceViewModelWithTask.HostName = "";
            _elasticsearchourceViewModelWithTask.Username = "Username";
            _elasticsearchourceViewModelWithTask.Password = "Password";
            _elasticsearchourceViewModelWithTask.AuthenticationType = AuthenticationType.Password;

            var result = _elasticsearchourceViewModelWithTask.CanTest();

            Assert.IsFalse(result);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestCanTestFalseHostNameUsernameEmpty()
        {
            _elasticsearchourceViewModelWithTask.HostName = "";
            _elasticsearchourceViewModelWithTask.Username = "";
            _elasticsearchourceViewModelWithTask.Password = "Password";
            _elasticsearchourceViewModelWithTask.AuthenticationType = AuthenticationType.Password;

            var result = _elasticsearchourceViewModelWithTask.CanTest();

            Assert.IsFalse(result);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestCanTestFalseHostNameUsernamePasswordEmpty()
        {
            _elasticsearchourceViewModelWithTask.HostName = "";
            _elasticsearchourceViewModelWithTask.Username = "";
            _elasticsearchourceViewModelWithTask.Password = "";
            _elasticsearchourceViewModelWithTask.AuthenticationType = AuthenticationType.Password;

            var result = _elasticsearchourceViewModelWithTask.CanTest();

            Assert.IsFalse(result);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ElasticsearchSourceViewModel))]
        public void ElasticsearchSourceViewModel_TestCanTestFalseHostNameUsernamePasswordEmptyAuthenticationTypeNotSet()
        {
            _elasticsearchourceViewModelWithTask.HostName = "";
            _elasticsearchourceViewModelWithTask.Username = "";
            _elasticsearchourceViewModelWithTask.Password = "";

            var result = _elasticsearchourceViewModelWithTask.CanTest();

            Assert.IsFalse(result);
        }
    }
}