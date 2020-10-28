/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageSqliteSourceViewModelTests
    {
        Mock<IManageDatabaseSourceModel> _updateManagerMock;
        Mock<IEventAggregator> _aggregatorMock;
        Mock<IAsyncWorker> _asyncWorkerMock;
        Mock<IDbSource> _dbSourceMock;

        Mock<IRequestServiceNameViewModel> _requestServiceNameViewMock;
        Task<IRequestServiceNameViewModel> _requestServiceNameView;
        List<string> _changedPropertiesAsyncWorker;
        List<string> _changedPropertiesUpdateManagerAggregatorDbSource;
        List<string> _changedUpdateManagerRequestServiceName;

        ManageSqliteSourceViewModel _targetAsyncWorker;
        ManageSqliteSourceViewModel _targetUpdateManagerAggregatorDbSource;
        ManageSqliteSourceViewModel _targetUpdateManagerRequestServiceName;

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
            _dbSourceMock.SetupGet(it => it.ConnectionTimeout).Returns(30);
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
                            success?.Invoke(progress?.Invoke());
                        }
                        catch (Exception ex)
                        {
                            fail?.Invoke(ex);
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
                                action?.Invoke(dbSource);
                            });
            _targetAsyncWorker = new ManageSqliteSourceViewModel(_asyncWorkerMock.Object);
            _changedPropertiesAsyncWorker = new List<string>();
            _targetAsyncWorker.PropertyChanged += (sender, args) =>
            {
                _changedPropertiesAsyncWorker.Add(args.PropertyName);
            };


            _targetUpdateManagerAggregatorDbSource = new ManageSqliteSourceViewModel(
                _updateManagerMock.Object,
                _aggregatorMock.Object,
                _dbSourceMock.Object,
                _asyncWorkerMock.Object);
            _changedPropertiesUpdateManagerAggregatorDbSource = new List<string>();

            _targetUpdateManagerRequestServiceName = new ManageSqliteSourceViewModel(
                _updateManagerMock.Object,
                _requestServiceNameView,
                _aggregatorMock.Object,
                _asyncWorkerMock.Object);
            _changedUpdateManagerRequestServiceName = new List<string>();

        }
        [TestMethod]
        [Timeout(250)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManageSqliteSourceViewModel))]
        public void ManageSqliteSourceViewModel_UpdateManagerThrowsExceptionWithInnerException()
        {
            var expectedExceptionMessage = "someExceptionMessage";
            _updateManagerMock.Setup(it => it.GetComputerNames())
                .Throws(new Exception("someOuterExceptionMessage", new Exception(expectedExceptionMessage)));

            _targetUpdateManagerRequestServiceName = new ManageSqliteSourceViewModel(
                 _updateManagerMock.Object,
                 _requestServiceNameView,
                 _aggregatorMock.Object,
                 _asyncWorkerMock.Object);

            Assert.IsTrue(_targetUpdateManagerRequestServiceName.TestFailed);
            Assert.IsFalse(_targetUpdateManagerRequestServiceName.TestPassed);
            Assert.AreEqual(expectedExceptionMessage, _targetUpdateManagerRequestServiceName.TestMessage);
        }
		
        [TestMethod]
        [Timeout(250)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManageSqliteSourceViewModel))]
        public void ManageSqliteSourceViewModel_TestUpdateHelpDescriptor()
        {
            var mainViewModelMock = new Mock<IShellViewModel>();
            var helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);
            var helpText = "someText";

            _targetAsyncWorker.UpdateHelpDescriptor(helpText);
            helpViewModelMock.Verify(it => it.UpdateHelpText(helpText));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManageSqliteSourceViewModel))]
        public void ManageSqliteSourceViewModel_TestName()
        {
            var expectedValue = "someName";
            _targetAsyncWorker.Name = expectedValue;
            var actualValue = _targetAsyncWorker.Name;

            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(expectedValue, _targetAsyncWorker.ResourceName);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManageSqliteSourceViewModel))]
        public void ManageSqliteSourceViewModel_TestToModelItemNull()
        {
            //arrange
            var gd = Guid.NewGuid();
            _targetAsyncWorker.Item = null;
            var expectedType = enSourceType.SQLiteDatabase;
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
        [Timeout(250)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManageSqliteSourceViewModel))]
        public void ManageSqliteSourceViewModel_TestToModeldbSourceIsNotNull_idIsNull()
        {
            //arrange
            _targetAsyncWorker.Item = null;
            var expectedType = enSourceType.SQLiteDatabase;
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
            

        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManageSqliteSourceViewModel))]
        public void ManageSqliteSourceViewModel_TestHeaderText()
        {
            var expectedValue = "someHeaderText";
            _changedPropertiesAsyncWorker.Clear();
            _targetAsyncWorker.HeaderText = expectedValue;
            var actualValue = _targetAsyncWorker.HeaderText;
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("HeaderText"));
            Assert.IsTrue(_changedPropertiesAsyncWorker.Contains("Header"));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManageSqliteSourceViewModel))]
        public void ManageSqliteSourceViewModel_dbSourceNotNull()
        {
            var expectedId = Guid.NewGuid();
            
            _asyncWorkerMock = new Mock<IAsyncWorker>();
            _updateManagerMock = new Mock<IManageDatabaseSourceModel>();
            _aggregatorMock = new Mock<IEventAggregator>();
            _dbSourceMock = new Mock<IDbSource>();
            _requestServiceNameViewMock = new Mock<IRequestServiceNameViewModel>();
            _requestServiceNameView = Task.FromResult(_requestServiceNameViewMock.Object);

            _updateManagerMock.Setup(it => it.GetComputerNames())
                .Returns(new List<string>() { "someName1", "someName2" });

            _dbSourceMock.SetupGet(it => it.Name).Returns("someDbSourceName");
            _dbSourceMock.SetupGet(it => it.ConnectionTimeout).Returns(30);
            _dbSourceMock.Setup(it => it.Id).Returns(expectedId);
        
            _updateManagerMock.Setup(model => model.FetchDbSource(It.IsAny<Guid>()))
            .Returns(_dbSourceMock.Object);
            _asyncWorkerMock.Setup(worker =>
                                   worker.Start(
                                            It.IsAny<Func<IDbSource>>(),
                                            It.IsAny<Action<IDbSource>>()))
                            .Callback<Func<IDbSource>, Action<IDbSource>>((func, action) =>
                            {
                                var dbSource = func.Invoke();
                                action?.Invoke(dbSource);
                            });

            _targetAsyncWorker = new ManageSqliteSourceViewModel(
                _updateManagerMock.Object,
                _aggregatorMock.Object,
                _dbSourceMock.Object,
                _asyncWorkerMock.Object);
        
            //arrange
            var expectedAuthenticationType = AuthenticationType.Anonymous;
            var expectedServerName = "expectedServerName";
            var expectedPassword = "expectedPassword";
            var expectedUsername = "expectedUsername";
            var expectedType = enSourceType.SQLiteDatabase;
            var expectedPath = "somePath";
            var expectedDbName = "someDbName";

            _targetAsyncWorker.AuthenticationType = expectedAuthenticationType;
            _targetAsyncWorker.ServerName = new ComputerName() { Name = expectedServerName };
            _targetAsyncWorker.Password = expectedPassword;
            _targetAsyncWorker.UserName = expectedUsername;
            _targetAsyncWorker.Path = expectedPath;
            _targetAsyncWorker.DatabaseName = expectedDbName;
            _targetAsyncWorker.Item = _dbSourceMock.Object;

            //act
            var value = _targetAsyncWorker.ToModel();

            //assert
            Assert.AreSame(_targetAsyncWorker.Item, value);
            Assert.AreEqual(expectedAuthenticationType, value.AuthenticationType);
            Assert.AreEqual(expectedPassword, value.Password);
            Assert.AreEqual(expectedType, value.Type);
            Assert.AreEqual(expectedDbName, value.DbName);
            Assert.AreEqual(expectedId, value.Id);

        }
    }
}
