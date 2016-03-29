using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageWebServiceViewModelTests
    {
        #region Fields

        private Mock<IWebServiceModel> _modelMock;
        private Mock<IWebService> _serviceMock;
        private Mock<IWebServiceSource> _serviceSourceMock;
        private Mock<IStudioUpdateManager> _updateRepositoryMock;
        private Mock<IRequestServiceNameViewModel> _requestServiceNameViewModelMock;
        private Task<IRequestServiceNameViewModel> _requestServiceNameViewModelTask;

        private List<string> _changedProperties;
        private ManageWebServiceViewModel _target;

        private List<string> _changedPropertiesRequestServiceViewModel;
        private ManageWebServiceViewModel _targetRequestServiceViewModel;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _modelMock = new Mock<IWebServiceModel>();
            _serviceMock = new Mock<IWebService>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _serviceSourceMock = new Mock<IWebServiceSource>();

            _serviceMock.Setup(it => it.Inputs).Returns(() => new List<IServiceInput>());
            _serviceSourceMock.Setup(it => it.Equals(It.IsAny<IWebServiceSource>()))
                .Returns<IWebServiceSource>((eq) => ReferenceEquals(eq, _serviceSourceMock.Object));
            
            _requestServiceNameViewModelMock = new Mock<IRequestServiceNameViewModel>();
            _requestServiceNameViewModelTask = Task.FromResult(_requestServiceNameViewModelMock.Object);

            _modelMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _modelMock.SetupGet(it => it.Sources)
                .Returns(() => new ObservableCollection<IWebServiceSource>() { _serviceSourceMock.Object });

            _changedProperties = new List<string>();
            _target = new ManageWebServiceViewModel(_modelMock.Object, _serviceMock.Object);
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };

            _changedPropertiesRequestServiceViewModel = new List<string>();
            _targetRequestServiceViewModel = new ManageWebServiceViewModel(
                _modelMock.Object,
                _requestServiceNameViewModelTask);
            _targetRequestServiceViewModel.PropertyChanged +=
                (sender, args) => { _changedPropertiesRequestServiceViewModel.Add(args.PropertyName); };
        }

        #endregion Test initialize

        #region Test commands

        [TestMethod]
        public void TestAddHeaderCommandCanExecute()
        {
            //act
            var result = _target.AddHeaderCommand.CanExecute(null);
            var resultRequestServiceViewModel = _targetRequestServiceViewModel.AddHeaderCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
            Assert.IsTrue(resultRequestServiceViewModel);
        }

        [TestMethod]
        public void TestAddHeaderCommandExecute()
        {
            //arrange
            _target.Headers = new List<NameValue>();
            _target.SelectedRow = new NameValue("someName", "someValue");
            _target.Headers.Add(_target.SelectedRow);

            //act
            _target.AddHeaderCommand.Execute(null);

            //assert
            Assert.IsFalse(_target.Headers.Contains(_target.SelectedRow));
        }

        [TestMethod]
        public void TestAddHeaderCommandExecuteRequestServiceViewModel()
        {
            //arrange
            _targetRequestServiceViewModel.Headers = new List<NameValue>();
            _targetRequestServiceViewModel.SelectedRow = new NameValue("someName", "someValue");
            _targetRequestServiceViewModel.Headers.Add(_targetRequestServiceViewModel.SelectedRow);

            //act
            _targetRequestServiceViewModel.AddHeaderCommand.Execute(null);

            //assert
            Assert.IsFalse(_targetRequestServiceViewModel.Headers.Contains(_targetRequestServiceViewModel.SelectedRow));
        }

        [TestMethod]
        public void TestRemoveHeaderCommandCanExecute()
        {
            //act
            var result = _target.RemoveHeaderCommand.CanExecute(null);
            var resultRequestServiceViewModel = _targetRequestServiceViewModel.RemoveHeaderCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
            Assert.IsTrue(resultRequestServiceViewModel);
        }

        [TestMethod]
        public void TestRemoveHeaderCommandExecute()
        {
            //arrange
            _target.Headers = new List<NameValue>();
            _target.SelectedRow = new NameValue("someName", "someValue");
            _target.Headers.Add(_target.SelectedRow);

            //act
            _target.RemoveHeaderCommand.Execute(null);

            //assert
            Assert.IsFalse(_target.Headers.Contains(_target.SelectedRow));
        }

        [TestMethod]
        public void TestRemoveHeaderCommandExecuteRequestServiceViewModel()
        {
            //arrange
            _targetRequestServiceViewModel.Headers = new List<NameValue>();
            _targetRequestServiceViewModel.SelectedRow = new NameValue("someName", "someValue");
            _targetRequestServiceViewModel.Headers.Add(_targetRequestServiceViewModel.SelectedRow);

            //act
            _targetRequestServiceViewModel.RemoveHeaderCommand.Execute(null);

            //assert
            Assert.IsFalse(_targetRequestServiceViewModel.Headers.Contains(_targetRequestServiceViewModel.SelectedRow));
        }

        [TestMethod]
        public void TestPasteResponseCommandCanExecute()
        {
            //act
            var result = _target.PasteResponseCommand.CanExecute(null);
            var resultRequestServiceViewModel = _targetRequestServiceViewModel.PasteResponseCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
            Assert.IsTrue(resultRequestServiceViewModel);
        }

        [TestMethod]
        public void TestPasteResponseCommandExecute()
        {
            //arrange
            var expectedResponse = "someResponse";
            var expectedResponseToHandle = "someOtherResponse";
            _target.Response = expectedResponseToHandle;
            _modelMock.Setup(it => it.HandlePasteResponse(expectedResponseToHandle)).Returns(expectedResponse);

            //act
            _target.PasteResponseCommand.Execute(null);

            //assert
            Assert.AreEqual(expectedResponse, _target.Response);
            _modelMock.Verify(it => it.HandlePasteResponse(expectedResponseToHandle));
            _serviceMock.VerifySet(it => it.Response = expectedResponse);
            _modelMock.Verify(it => it.TestService(_serviceMock.Object));
        }

        [TestMethod]
        public void TestPasteResponseCommandExecuteResponseNull()
        {
            //arrange
            var expectedResponse = "someResponse";
            _target.Response = null;
            _modelMock.Setup(it => it.HandlePasteResponse(It.IsAny<string>())).Returns(expectedResponse);

            //act
            _target.PasteResponseCommand.Execute(null);

            //assert
            Assert.AreEqual(expectedResponse, _target.Response);
            _modelMock.Verify(it => it.HandlePasteResponse(""));
            _serviceMock.VerifySet(it => it.Response = expectedResponse);
            _modelMock.Verify(it => it.TestService(_serviceMock.Object));
        }

        [TestMethod]
        public void TestPasteResponseCommandExecuteRequestServiceViewModel()
        {
            //arrange
            var expectedResponse = "someResponse";
            var expectedResponseToHandle = "someOtherResponse";
            _targetRequestServiceViewModel.Response = expectedResponseToHandle;
            _modelMock.Setup(it => it.HandlePasteResponse(expectedResponseToHandle)).Returns(expectedResponse);

            //act
            _targetRequestServiceViewModel.PasteResponseCommand.Execute(null);

            //assert
            Assert.AreEqual(expectedResponse, _targetRequestServiceViewModel.Response);
            _modelMock.Verify(it => it.HandlePasteResponse(expectedResponseToHandle));
            _modelMock.Verify(it => it.TestService(It.Is<IWebService>(x => x.Response == expectedResponse)));
        }

        [TestMethod]
        public void TestEditWebSourceCommandCanExecute()
        {
            //act
            var result = _target.EditWebSourceCommand.CanExecute(null);
            var resultRequestServiceViewModel = _targetRequestServiceViewModel.EditWebSourceCommand.CanExecute(null);
            
            //assert
            Assert.IsTrue(result);
            Assert.IsTrue(resultRequestServiceViewModel);
        }

        [TestMethod]
        public void TestEditWebSourceCommandExecute()
        {
            //arrange
            var selectedSourceMock = new Mock<IWebServiceSource>();
            _target.SelectedSource = selectedSourceMock.Object;

            //act
            _target.EditWebSourceCommand.Execute(null);

            //assert
            _modelMock.Verify(it => it.EditSource(_target.SelectedSource));
        }

        [TestMethod]
        public void TestEditWebSourceCommandExecuteRequestServiceViewModel()
        {
            //arrange
            var selectedSourceMock = new Mock<IWebServiceSource>();
            _target.SelectedSource = selectedSourceMock.Object;

            //act
            _targetRequestServiceViewModel.EditWebSourceCommand.Execute(null);

            //assert
            _modelMock.Verify(it => it.EditSource(_targetRequestServiceViewModel.SelectedSource));
        }

        [TestMethod]
        public void TestNewWebSourceCommandCanExecute()
        {
            //act
            var result = _target.NewWebSourceCommand.CanExecute(null);
            var resultRequestServiceViewModel = _targetRequestServiceViewModel.NewWebSourceCommand.CanExecute(null);
            
            //assert
            Assert.IsTrue(result);
            Assert.IsTrue(resultRequestServiceViewModel);    
        }

        [TestMethod]
        public void TestNewWebSourceCommandExecute()
        {
            //act
            _target.NewWebSourceCommand.Execute(null);
            
            //assert
            _modelMock.Verify(it=>it.CreateNewSource());
        }

        [TestMethod]
        public void TestNewWebSourceCommandExecuteRequestServiceViewModel()
        {
            //act
            _targetRequestServiceViewModel.NewWebSourceCommand.Execute(null);

            //assert
            _modelMock.Verify(it => it.CreateNewSource());
        }

        [TestMethod]
        public void TestTestCommandCanExecuteFalse()
        {
            //arrange
            _target.SelectedSource = null;

            //act
            var result = _target.TestCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestTestCommandCanExecuteTrue()
        {
            //arrange
            var selectedSourceMock = new Mock<IWebServiceSource>();
            _target.SelectedSource = selectedSourceMock.Object;

            //act
            var result = _target.TestCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestTestCommandExecute()
        {
            //arrange
            var expectedResponse = "someResponse";
            var expectedRecordsetName = "someRecordsetName";
            var expectedMappedFrom = "mappedFrom";
            var expectedMapping = "[[someRecordsetName().mapping]]";
            var expectedResponseService = new Dev2.Runtime.ServiceModel.Data.WebService();
            expectedResponseService.RequestResponse = expectedResponse;
            expectedResponseService.Recordsets = new RecordsetList()
                                                     {
                                                         new Recordset()
                                                             {
                                                                 Name = expectedRecordsetName,
                                                                 Fields =
                                                                     new List<RecordsetField>(
                                                                     )
                                                                         {
                                                                             new RecordsetField(
                                                                                 )
                                                                                 {
                                                                                     Alias =
                                                                                         expectedMapping,
                                                                                     Name =
                                                                                         expectedMappedFrom
                                                                                 }
                                                                         }
                                                             }
                                                     };
            var serializer = new Dev2.Communication.Dev2JsonSerializer();
            _target.SelectedSource = null;
            _modelMock
                .Setup(it => it.TestService(It.IsAny<IWebService>()))
                .Returns(serializer.Serialize(expectedResponseService));

            //act
            _target.TestCommand.Execute(null);

            //assert
            Assert.IsFalse(_target.IsTesting);
            Assert.IsTrue(string.IsNullOrEmpty(_target.ErrorMessage));
            Assert.IsTrue(_target.CanEditMappings);
            Assert.IsTrue(_target.CanEditResponse);
            Assert.AreEqual(expectedRecordsetName, _target.RecordsetName);
            Assert.IsTrue(_target.OutputMapping.Any(it=>it.MappedFrom == expectedMappedFrom && it.MappedTo==expectedMapping && it.RecordSetName == expectedRecordsetName));
        }

        [TestMethod]
        public void TestTestCommandExecuteException()
        {
            //arrange
            var expectedMessage = "someMessage";
            _target.SelectedSource = null;
            _modelMock
                .Setup(it => it.TestService(It.IsAny<IWebService>()))
                .Throws(new Exception(expectedMessage));

            //act
            _target.TestCommand.Execute(null);

            //assert
            Assert.IsFalse(_target.IsTesting);
            Assert.IsFalse(_target.CanEditMappings);
            Assert.IsFalse(_target.CanEditResponse);
            Assert.AreEqual(expectedMessage, _target.ErrorMessage);
            Assert.IsNotNull(_target.OutputMapping);
        }

        [TestMethod]
        public void TestSaveCommandCanExecuteFalse()
        {
            //arrange
            _target.Response = "";

            //act
            var result = _target.SaveCommand.CanExecute(null);
            
            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestSaveCommandCanExecuteTrue()
        {
            //arrange
            _target.Response = "someResponse";

            //act
            var result = _target.SaveCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestSaveCommandExecute()
        {
            //arrange
            var itemMock = new Mock<IWebService>();
            var itemId = Guid.NewGuid();
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _target.Response = "someResponse";
            _target.Item = itemMock.Object;

            //act
            _target.SaveCommand.Execute(null);

            //assert
            _modelMock.Verify(it => it.SaveService(_target.Item));
            Assert.IsTrue(string.IsNullOrEmpty(_target.ErrorMessage));
            Assert.AreNotSame(itemMock.Object, _target.Item);
        }

        [TestMethod]
        public void TestSaveCommandExecuteException()
        {
            //arrange
            var expectedErrorMessage = "someErrorMessage";
            _modelMock.Setup(it => it.SaveService(It.IsAny<IWebService>())).Throws(new Exception(expectedErrorMessage));
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                           .Returns(new ResourceName("somePath", "someValue"));

            //act
            _target.SaveCommand.Execute(null);
            _targetRequestServiceViewModel.SaveCommand.Execute(null);

            //assert
            Assert.AreEqual(expectedErrorMessage, _target.ErrorMessage);
            Assert.AreEqual(expectedErrorMessage, _targetRequestServiceViewModel.ErrorMessage);
        }

        [TestMethod]
        public void TestSaveCommandExecuteRequestServiceNameViewModelOK()
        {
            //arrange
            var expectedPath = "somePath";
            var expectedName = "someName";
            var itemMock = new Mock<IWebService>();
            var itemId = Guid.NewGuid();
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(expectedPath, expectedName));
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _targetRequestServiceViewModel.Response = "someResponse";
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _targetRequestServiceViewModel.Item = itemMock.Object;

            //act
            _targetRequestServiceViewModel.SaveCommand.Execute(null);

            //assert
            _modelMock.Verify(it => it.SaveService(_targetRequestServiceViewModel.Item));
            Assert.AreNotEqual(Guid.Empty, _targetRequestServiceViewModel.Id);
            Assert.AreEqual(expectedName,_targetRequestServiceViewModel.Name);
            Assert.AreEqual(expectedPath, _targetRequestServiceViewModel.Path);
            Assert.AreEqual(expectedPath+expectedName, _targetRequestServiceViewModel.Header);
            Assert.IsTrue(string.IsNullOrEmpty(_targetRequestServiceViewModel.ErrorMessage));
            Assert.AreNotSame(itemMock.Object, _targetRequestServiceViewModel.Item);
        }

        [TestMethod]
        public void TestSaveCommandExecuteRequestServiceNameViewModelYes()
        {
            //arrange
            var expectedPath = "somePath";
            var expectedName = "someName";
            var itemMock = new Mock<IWebService>();
            var itemId = Guid.NewGuid();
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.Yes);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(expectedPath, expectedName));
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _targetRequestServiceViewModel.Response = "someResponse";
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _targetRequestServiceViewModel.Item = itemMock.Object;

            //act
            _targetRequestServiceViewModel.SaveCommand.Execute(null);

            //assert
            _modelMock.Verify(it => it.SaveService(_targetRequestServiceViewModel.Item));
            Assert.AreNotEqual(Guid.Empty, _targetRequestServiceViewModel.Id);
            Assert.AreEqual(expectedName, _targetRequestServiceViewModel.Name);
            Assert.AreEqual(expectedPath, _targetRequestServiceViewModel.Path);
            Assert.AreEqual(expectedPath + expectedName, _targetRequestServiceViewModel.Header);
            Assert.IsTrue(string.IsNullOrEmpty(_targetRequestServiceViewModel.ErrorMessage));
            Assert.AreNotSame(itemMock.Object, _targetRequestServiceViewModel.Item);
        }

        [TestMethod]
        public void TestCancelCommand()
        {
            //arrange
            var expectedValueMock = new Mock<ICommand>();
            _changedProperties.Clear();

            //act
            _target.CancelCommand = expectedValueMock.Object;
            var value = _target.CancelCommand;

            //asert
            Assert.AreSame(expectedValueMock.Object, value);
            Assert.IsTrue(_changedProperties.Contains("CancelCommand"));
        }

        [TestMethod]
        public void TestNewWebSourceCommand()
        {
            //arrange
            var expectedValueMock = new Mock<ICommand>();
            _changedProperties.Clear();

            //act
            _target.NewWebSourceCommand = expectedValueMock.Object;
            var value = _target.NewWebSourceCommand;

            //asert
            Assert.AreSame(expectedValueMock.Object, value);
            Assert.IsTrue(_changedProperties.Contains("NewWebSourceCommand"));
        }

        [TestMethod]
        public void TestNewWebSourceCommandNull()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _target.NewWebSourceCommand = null;
            var value = _target.NewWebSourceCommand;

            //asert
            Assert.IsNull(value);
            Assert.IsFalse(_changedProperties.Contains("NewWebSourceCommand"));
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        public void TestPath()
        {
            //arrange
            var expectedValue = "someValue";
            _changedProperties.Clear();

            //act
            _target.Path = expectedValue;
            var value = _target.Path;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Path"));
        }

        [TestMethod]
        public void TestName()
        {
            //arrange
            var expectedValue = "someValue";
            _changedProperties.Clear();

            //act
            _target.Name = expectedValue;
            var value = _target.Name;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Name"));
        }

        [TestMethod]
        public void TestCanEditHeadersAndUrl()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.CanEditHeadersAndUrl = expectedValue;
            var value = _target.CanEditHeadersAndUrl;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("CanEditHeadersAndUrl"));
        }

        [TestMethod]
        public void TestCanEditResponse()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.CanEditResponse = expectedValue;
            var value = _target.CanEditResponse;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("CanEditResponse"));
        }

        [TestMethod]
        public void TestSelectedRow()
        {
            //arrange
            var expectedValue = new NameValue();

            //act
            _target.SelectedRow = expectedValue;
            var value = _target.SelectedRow;

            //asert
            Assert.AreSame(expectedValue, value);
        }

        [TestMethod]
        public void TestSelectedDataItems()
        {
            //arrange
            var expectedValue = new List<object>();

            //act
            _target.SelectedDataItems = expectedValue;
            var value = _target.SelectedDataItems;

            //asert
            Assert.AreSame(expectedValue, value);
        }

        [TestMethod]
        public void TestId()
        {
            //arrange
            var expectedValue = Guid.NewGuid();
            _changedProperties.Clear();

            //act
            _target.Id = expectedValue;
            var value = _target.Id;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Id"));
        }

        [TestMethod]
        public void TestCanEditMappings()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.CanEditMappings = expectedValue;
            var value = _target.CanEditMappings;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("CanEditMappings"));
        }

        [TestMethod]
        public void TestIsTesting()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.IsTesting = expectedValue;
            var value = _target.IsTesting;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsTesting"));
        }

        [TestMethod]
        public void TestErrorMessage()
        {
            //arrange
            var expectedValue = "someErrorMessage";
            _changedProperties.Clear();

            //act
            _target.ErrorMessage = expectedValue;
            var value = _target.ErrorMessage;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains(expectedValue));
        }

        [TestMethod]
        public void TestSelectedWebRequestMethod()
        {
            //arrange
            var expectedValue = WebRequestMethod.Head;
            _changedProperties.Clear();

            //act
            _target.SelectedWebRequestMethod = expectedValue;
            var value = _target.SelectedWebRequestMethod;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("SelectedWebRequestMethod"));
            Assert.IsTrue(_changedProperties.Contains("RequestBodyEnabled"));
        }

        [TestMethod]
        public void TestWebRequestMethods()
        {
            //arrange
            var expectedValue = new List<WebRequestMethod>();
            _changedProperties.Clear();

            //act
            _target.WebRequestMethods = expectedValue;
            var value = _target.WebRequestMethods;

            //asert
            Assert.AreSame(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("WebRequestMethods"));
        }

        [TestMethod]
        public void TestSources()
        {
            //arrange
            var expectedValue = new ObservableCollection<IWebServiceSource>();
            _changedProperties.Clear();

            //act
            _target.Sources = expectedValue;
            var value = _target.Sources;

            //asert
            Assert.AreSame(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Sources"));
        }

        [TestMethod]
        public void TestSelectedSource()
        {
            //arrange
            var expectedValueMock = new Mock<IWebServiceSource>();
            var expectedRequestUrlQuery = "someUrlQuery";
            var expectedSourceUrl = "someSourceUrl";
            expectedValueMock.SetupGet(it => it.DefaultQuery).Returns(expectedRequestUrlQuery);
            expectedValueMock.SetupGet(it => it.HostName).Returns(expectedSourceUrl);
            _changedProperties.Clear();

            //act
            _target.SelectedSource = expectedValueMock.Object;
            var value = _target.SelectedSource;

            //asert
            Assert.AreSame(expectedValueMock.Object, value);
            Assert.IsTrue(_target.CanEditHeadersAndUrl);
            Assert.AreEqual(expectedRequestUrlQuery, _target.RequestUrlQuery);
            Assert.AreEqual(expectedSourceUrl, _target.SourceUrl);
            Assert.IsTrue(_changedProperties.Contains("SelectedSource"));
            Assert.IsFalse(_target.CanEditResponse);
            Assert.IsFalse(_target.CanEditMappings);
        }

        [TestMethod]
        public void TestSelectedSourceDefaultQueryNull()
        {
            //arrange
            var expectedValueMock = new Mock<IWebServiceSource>();
            var expectedSourceUrl = "someSourceUrl";
            expectedValueMock.SetupGet(it => it.DefaultQuery).Returns((string)null);
            expectedValueMock.SetupGet(it => it.HostName).Returns(expectedSourceUrl);
            _changedProperties.Clear();

            //act
            _target.SelectedSource = expectedValueMock.Object;
            var value = _target.SelectedSource;

            //asert
            Assert.AreSame(expectedValueMock.Object, value);
            Assert.IsTrue(_target.CanEditHeadersAndUrl);
            Assert.IsNotNull(_target.RequestUrlQuery);
            Assert.AreEqual(expectedSourceUrl, _target.SourceUrl);
            Assert.IsTrue(_changedProperties.Contains("SelectedSource"));
            Assert.IsFalse(_target.CanEditResponse);
            Assert.IsFalse(_target.CanEditMappings);
        }

        [TestMethod]
        public void TestSelectedSourceNull()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _target.SelectedSource = null;
            var value = _target.SelectedSource;

            //asert
            Assert.IsNull(value);
            Assert.IsFalse(_target.CanEditHeadersAndUrl);
            Assert.IsTrue(_changedProperties.Contains("SelectedSource"));
            Assert.IsFalse(_target.CanEditResponse);
            Assert.IsFalse(_target.CanEditMappings);
        }

        [TestMethod]
        public void TestSelectedSourcePerformRefreshException()
        {
            //arrange
            var expectedExceptionMessage = "someExceptionMessage";
            _changedProperties.Clear();
            _target.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "CanEditMappings")
                    {
                        throw new Exception(expectedExceptionMessage);
                    }
                };

            //act
            _target.SelectedSource = null;
            var value = _target.SelectedSource;

            //asert
            Assert.IsNull(value);
            Assert.IsFalse(_target.CanEditHeadersAndUrl);
            Assert.IsTrue(_changedProperties.Contains("SelectedSource"));
            Assert.IsFalse(_target.CanEditResponse);
            Assert.IsFalse(_target.CanEditMappings);
            Assert.AreEqual(expectedExceptionMessage, _target.ErrorMessage);
        }

        [TestMethod]
        public void TestRequestBodyEnabledTrue()
        {
            //arrange
            var expectedValue = true;
            _target.SelectedWebRequestMethod = WebRequestMethod.Post;

            //act
            _target.RequestBodyEnabled = expectedValue;
            var value = _target.RequestBodyEnabled;

            //asert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        public void TestRequestBodyEnabledFalse()
        {
            //arrange
            var expectedValue = false;
            _target.SelectedWebRequestMethod = WebRequestMethod.Get;

            //act
            _target.RequestBodyEnabled = expectedValue;
            var value = _target.RequestBodyEnabled;

            //asert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        public void TestResourceName()
        {
            //arrange
            var expectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.ResourceName = expectedValue;
            var value = _target.ResourceName;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains(expectedValue));
        }

        [TestMethod]
        public void TestRequestUrlQuery()
        {
            //arrange
            var expectedValue = "[[s]]";
            _changedProperties.Clear();

            //act
            _target.RequestUrlQuery = expectedValue;
            var value = _target.RequestUrlQuery;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_target.Variables.Any(it => it.Name == expectedValue && it.Value == ""));
            Assert.IsTrue(_changedProperties.Contains("RequestUrlQuery"));
        }

        [TestMethod]
        public void TestSourceUrl()
        {
            //arrange
            var expectedValue = "someSourceUrl";
            _changedProperties.Clear();

            //act
            _target.SourceUrl = expectedValue;
            var value = _target.SourceUrl;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("SourceUrl"));
        }

        [TestMethod]
        public void TestRequestBody()
        {
            //arrange
            var expectedValue = "someStr";
            _changedProperties.Clear();

            //act
            _target.RequestBody = expectedValue;
            var value = _target.RequestBody;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("RequestBody"));
        }

        [TestMethod]
        public void TestResponse()
        {
            //arrange
            var expectedValue = "someStr";
            _changedProperties.Clear();

            //act
            _target.Response = expectedValue;
            var value = _target.Response;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Response"));
        }

        [TestMethod]
        public void TestTestCommandButtonText()
        {
            //arrange
            var expectedValue = "SomeStr";

            //act
            _target.TestCommandButtonText = expectedValue;
            var value = _target.TestCommandButtonText;

            //asert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        public void TestSaveCommandText()
        {
            //arrange
            var expectedValue = "someStr";

            //act
            _target.SaveCommandText = expectedValue;
            var value = _target.SaveCommandText;

            //asert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        public void TestVariables()
        {
            //arrange
            var expectedValue = new List<NameValue>();
            _changedProperties.Clear();

            //act
            _target.Variables = expectedValue;
            var value = _target.Variables;

            //asert
            Assert.AreSame(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Variables"));
        }

        [TestMethod]
        public void TestRecordsetName()
        {
            //arrange
            var expectedValue = "someRecordSet";
            _changedProperties.Clear();

            //act
            _target.RecordsetName = expectedValue;
            var value = _target.RecordsetName;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("RecordsetName"));
        }

        [TestMethod]
        public void TestIsInputsEmptyRows()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.IsInputsEmptyRows = expectedValue;
            var value = _target.IsInputsEmptyRows;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsInputsEmptyRows"));
        }

        [TestMethod]
        public void TestIsOutputMappingEmptyRows()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.IsOutputMappingEmptyRows = expectedValue;
            var value = _target.IsOutputMappingEmptyRows;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsOutputMappingEmptyRows"));
        }

        [TestMethod]
        public void TestOutputName()
        {
            //arrange
            var expectedValue = "someOutputName";
            _changedProperties.Clear();

            //act
            _target.OutputName = expectedValue;
            var value = _target.OutputName;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("OutputName"));
        }

        [TestMethod]
        public void TestOutputAliasHeader()
        {
            //arrange
            var expectedValue = "someOutputName";

            //act
            _target.OutputAliasHeader = expectedValue;
            var value = _target.OutputAliasHeader;

            //asert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        public void TestWebService()
        {
            //arrange
            var expectedValueMock = new Mock<IWebService>();

            //act
            _target.WebService = expectedValueMock.Object;
            var value = _target.WebService;

            //asert
            Assert.AreSame(expectedValueMock.Object, value);
        }

        [TestMethod]
        public void TestHeaders()
        {
            //arrange
            var expectedVariableName = "[[s]]";
            var expectedValue = new List<NameValue>() {new NameValue(expectedVariableName, expectedVariableName)};
            _changedProperties.Clear();

            //act
            _target.Headers = expectedValue;
            var value = _target.Headers;

            //asert
            Assert.AreSame(expectedValue, value);
            Assert.IsTrue(_target.Variables.Any(it=>it.Name == expectedVariableName && it.Value == ""));
            Assert.IsTrue(_changedProperties.Contains("Headers"));
        }

        [TestMethod]
        public void TestOutputMappingEmpty()
        {
            //arrange
            var expectedValue = new List<IServiceOutputMapping>();
            _changedProperties.Clear();

            //act
            _target.OutputMapping = expectedValue;
            var value = _target.OutputMapping;

            //asert
            Assert.AreSame(expectedValue, value);
            Assert.IsTrue(_target.IsOutputMappingEmptyRows);
            Assert.IsTrue(_changedProperties.Contains("OutputMapping"));
        }

        [TestMethod]
        public void TestOutputMappingNotEmpty()
        {
            //arrange
            var serviceOutputMappingMock = new Mock<IServiceOutputMapping>();
            var expectedValue = new List<IServiceOutputMapping>() { serviceOutputMappingMock.Object };
            _changedProperties.Clear();

            //act
            _target.OutputMapping = expectedValue;
            var value = _target.OutputMapping;

            //asert
            Assert.AreSame(expectedValue, value);
            Assert.IsFalse(_target.IsOutputMappingEmptyRows);
            Assert.IsTrue(_changedProperties.Contains("OutputMapping"));
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        public void TestToModelItemNotNullServiceNotNull()
        {
            //arrange
            var sourceMock = new Mock<IWebServiceSource>();
            sourceMock.SetupGet(it => it.DefaultQuery).Returns("someRequestUrl");
            var itemMock = new Mock<IWebService>();
            var itemId = Guid.NewGuid();
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _target.Item = itemMock.Object;
            _target.SelectedSource = sourceMock.Object;
            _target.OutputMapping = new List<IServiceOutputMapping>();
            _target.Name = "someName";
            _target.Path = "somePath";
            _target.RequestBody = "someBody";
            var expectedName = "[[someVar]]";
            var expectedValue = "[[someVar1]]";
            _target.Headers = new List<NameValue>() { new NameValue(expectedName, expectedValue) };
            _target.SourceUrl = "someSourceUrl";
            _target.Response = "someResponse";

            //act
            var result = _target.ToModel();

            //assert
            Assert.IsTrue(
                 result.Inputs.Any(
                    input =>
                    input.Name == DataListUtil.RemoveLanguageBrackets(expectedName)
                    && input.Value == ""));
            Assert.IsTrue(
                    result.Inputs.Any(
                       input =>
                       input.Name == DataListUtil.RemoveLanguageBrackets(expectedValue)
                       && input.Value == ""));
            Assert.AreSame(_target.OutputMapping, result.OutputMappings);
            Assert.AreSame(_target.SelectedSource, result.Source);
            Assert.AreEqual(_target.Name, result.Name);
            Assert.AreEqual(_target.Path, result.Path);
            Assert.AreNotEqual(Guid.Empty, result.Id);
            Assert.AreEqual(DataListUtil.RemoveLanguageBrackets(_target.RequestBody), result.PostData);
            Assert.IsTrue(
                result.Headers.Any(
                    hdr =>
                    hdr.Name == DataListUtil.RemoveLanguageBrackets(expectedName)
                    && hdr.Value == DataListUtil.RemoveLanguageBrackets(expectedValue)));
            Assert.AreEqual(_target.RequestUrlQuery, result.QueryString);
            Assert.AreEqual(_target.SourceUrl, result.SourceUrl);
            Assert.AreEqual(_target.RequestUrlQuery, result.RequestUrl);
            Assert.AreEqual(_target.Response, result.Response);
        }

        [TestMethod]
        public void TestToModelItemNullServiceNotNull()
        {
            //arrange
            var sourceMock = new Mock<IWebServiceSource>();
            sourceMock.SetupGet(it => it.DefaultQuery).Returns("someRequestUrl");
            _target.Item = null;
            _target.SelectedSource = sourceMock.Object;
            _target.OutputMapping = new List<IServiceOutputMapping>();
            _target.Name = "someName";
            _target.Path = "somePath";
            _target.RequestBody = "someBody";
            var expectedName = "[[someVar]]";
            var expectedValue = "[[someVar1]]";
            _target.Headers = new List<NameValue>() { new NameValue(expectedName, expectedValue) };
            _target.SourceUrl = "someSourceUrl";
            _target.Response = "someResponse";

            //act
            var result = _target.ToModel();

            //assert
            Assert.AreSame(_serviceMock.Object, result);
            _serviceMock.VerifySet(
                it =>
                it.Inputs =
                It.Is<IList<IServiceInput>>(
                    val =>
                    val.Any(
                        input => input.Name == DataListUtil.RemoveLanguageBrackets(expectedName) && input.Value == "")
                    && val.Any(
                        input => input.Name == DataListUtil.RemoveLanguageBrackets(expectedValue) && input.Value == "")));
            _serviceMock.VerifySet(it => it.OutputMappings = _target.OutputMapping);
            _serviceMock.VerifySet(it => it.Source = _target.SelectedSource);
            _serviceMock.VerifySet(it => it.Name = _target.Name);
            _serviceMock.VerifySet(it => it.Path = _target.Path);
            _serviceMock.VerifySet(it => it.Id = It.Is<Guid>(id=>id!=Guid.Empty));
            _serviceMock.VerifySet(it => it.PostData = DataListUtil.RemoveLanguageBrackets(_target.RequestBody));
            _serviceMock.VerifySet(
                it =>
                it.Headers =
                It.Is<List<NameValue>>(
                    val =>
                    val.Any(
                        hdr =>
                        hdr.Name == DataListUtil.RemoveLanguageBrackets(expectedName)
                        && hdr.Value == DataListUtil.RemoveLanguageBrackets(expectedValue))));
            _serviceMock.VerifySet(it => it.QueryString = _target.RequestUrlQuery);
            _serviceMock.VerifySet(it => it.SourceUrl = _target.SourceUrl);
            _serviceMock.VerifySet(it => it.RequestUrl = _target.RequestUrlQuery);
            _serviceMock.VerifySet(it => it.Response = _target.Response);
        }

        [TestMethod]
        public void TestToModelItemNull()
        {
            //arrange
            var sourceMock = new Mock<IWebServiceSource>();
            sourceMock.SetupGet(it => it.DefaultQuery).Returns("someRequestUrl");

            _targetRequestServiceViewModel.Item = null;
            _targetRequestServiceViewModel.SelectedSource = sourceMock.Object;
            _targetRequestServiceViewModel.OutputMapping = new List<IServiceOutputMapping>();
            _targetRequestServiceViewModel.Name = "someName";
            _targetRequestServiceViewModel.Path = "somePath";
            _targetRequestServiceViewModel.RequestBody = "someBody";
            var expectedName = "[[someVar]]";
            var expectedValue = "[[someVar1]]";
            _targetRequestServiceViewModel.Headers = new List<NameValue>() { new NameValue(expectedName, expectedValue) };
            _targetRequestServiceViewModel.SourceUrl = "someSourceUrl";
            _targetRequestServiceViewModel.Response = "someResponse";

            //act
            var result = _targetRequestServiceViewModel.ToModel();

            //assert
            Assert.IsTrue(
                 result.Inputs.Any(
                    input =>
                    input.Name == DataListUtil.RemoveLanguageBrackets(expectedName)
                    && input.Value == ""));
            Assert.IsTrue(
                    result.Inputs.Any(
                       input =>
                       input.Name == DataListUtil.RemoveLanguageBrackets(expectedValue)
                       && input.Value == ""));
            Assert.AreSame(_targetRequestServiceViewModel.OutputMapping, result.OutputMappings);
            Assert.AreSame(_targetRequestServiceViewModel.SelectedSource, result.Source);
            Assert.AreEqual(_targetRequestServiceViewModel.Name, result.Name);
            Assert.AreEqual(_targetRequestServiceViewModel.Path, result.Path);
            Assert.AreNotEqual(Guid.Empty, result.Id);
            Assert.AreEqual(DataListUtil.RemoveLanguageBrackets(_targetRequestServiceViewModel.RequestBody), result.PostData);
            Assert.IsTrue(
                result.Headers.Any(
                    hdr =>
                    hdr.Name == DataListUtil.RemoveLanguageBrackets(expectedName)
                    && hdr.Value == DataListUtil.RemoveLanguageBrackets(expectedValue)));
            Assert.AreEqual(_targetRequestServiceViewModel.RequestUrlQuery, result.QueryString);
            Assert.AreEqual(_targetRequestServiceViewModel.SourceUrl, result.SourceUrl);
            Assert.AreEqual(_targetRequestServiceViewModel.RequestUrlQuery, result.RequestUrl);
            Assert.AreEqual(_targetRequestServiceViewModel.Response, result.Response);
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
        public void TestCanSaveFalse()
        {
            //arrange
            _target.Response = "";

            //act
            var result = _target.CanSave();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanSaveTrue()
        {
            //arrange
            _target.Response = "someResponse";

            //act
            var result = _target.CanSave();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestSaveException()
        {
            //arrange
            var expectedErrorMessage = "someErrorMessage";
            _modelMock.Setup(it => it.SaveService(It.IsAny<IWebService>())).Throws(new Exception(expectedErrorMessage));
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName("somePath", "someName"));
            //act
            _target.Save();
            _targetRequestServiceViewModel.Save();

            //assert
            Assert.AreEqual(expectedErrorMessage, _target.ErrorMessage);
            Assert.AreEqual(expectedErrorMessage, _targetRequestServiceViewModel.ErrorMessage);
        }

        [TestMethod]
        public void TestSave()
        {
            //arrange
            var itemMock = new Mock<IWebService>();
            var itemId = Guid.NewGuid();
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _target.Response = "someResponse";
            _target.Item = itemMock.Object;

            //act
            _target.Save();

            //assert
            _modelMock.Verify(it => it.SaveService(_target.Item));
            Assert.IsTrue(string.IsNullOrEmpty(_target.ErrorMessage));
            Assert.AreNotSame(itemMock.Object, _target.Item);
        }

        [TestMethod]
        public void TestSaveRequestServiceNameViewModelOK()
        {
            //arrange
            var expectedPath = "somePath";
            var expectedName = "someName";
            var itemMock = new Mock<IWebService>();
            var itemId = Guid.NewGuid();
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(expectedPath, expectedName));
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _targetRequestServiceViewModel.Response = "someResponse";
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _targetRequestServiceViewModel.Item = itemMock.Object;

            //act
            _targetRequestServiceViewModel.Save();

            //assert
            _modelMock.Verify(it => it.SaveService(_targetRequestServiceViewModel.Item));
            Assert.AreNotEqual(Guid.Empty, _targetRequestServiceViewModel.Id);
            Assert.AreEqual(expectedName, _targetRequestServiceViewModel.Name);
            Assert.AreEqual(expectedPath, _targetRequestServiceViewModel.Path);
            Assert.AreEqual(expectedPath + expectedName, _targetRequestServiceViewModel.Header);
            Assert.IsTrue(string.IsNullOrEmpty(_targetRequestServiceViewModel.ErrorMessage));
            Assert.AreNotSame(itemMock.Object, _targetRequestServiceViewModel.Item);
        }

        [TestMethod]
        public void TestSaveRequestServiceNameViewModelYes()
        {
            //arrange
            var expectedPath = "somePath";
            var expectedName = "someName";
            var itemMock = new Mock<IWebService>();
            var itemId = Guid.NewGuid();
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.Yes);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(expectedPath, expectedName));
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _targetRequestServiceViewModel.Response = "someResponse";
            itemMock.SetupGet(it => it.Id).Returns(itemId);
            _targetRequestServiceViewModel.Item = itemMock.Object;

            //act
            _targetRequestServiceViewModel.Save();

            //assert
            _modelMock.Verify(it => it.SaveService(_targetRequestServiceViewModel.Item));
            Assert.AreNotEqual(Guid.Empty, _targetRequestServiceViewModel.Id);
            Assert.AreEqual(expectedName, _targetRequestServiceViewModel.Name);
            Assert.AreEqual(expectedPath, _targetRequestServiceViewModel.Path);
            Assert.AreEqual(expectedPath + expectedName, _targetRequestServiceViewModel.Header);
            Assert.IsTrue(string.IsNullOrEmpty(_targetRequestServiceViewModel.ErrorMessage));
            Assert.AreNotSame(itemMock.Object, _targetRequestServiceViewModel.Item);
        }

        [TestMethod]
        public void TestInitSourcesFailed()
        {
            //arrange
            var expectedErrorMessage = "someMessage1";
            var expectedOuterErrorMessage = "someMessage2";
            _modelMock.Setup(it => it.Sources).Throws(new Exception(expectedOuterErrorMessage, new Exception(expectedErrorMessage)));

            //act
            _targetRequestServiceViewModel = new ManageWebServiceViewModel(
                 _modelMock.Object,
                 _requestServiceNameViewModelTask);

            //assert
            Assert.AreEqual(expectedErrorMessage, _targetRequestServiceViewModel.ErrorMessage);
        }

        [TestMethod]
        public void TestUpdateSourcesCollection()
        {
            //arrange
            var serviceSourceMock = new Mock<IWebServiceSource>();
            var serviceId = Guid.NewGuid();
            serviceSourceMock.SetupGet(it => it.Id).Returns(serviceId);
            serviceSourceMock.Setup(it => it.Equals(It.IsAny<IWebServiceSource>()))
                .Returns<IWebServiceSource>((eq) => ReferenceEquals(eq, serviceSourceMock.Object));

            //act
            _updateRepositoryMock.Raise(it => it.WebServiceSourceSaved += null, serviceSourceMock.Object);

            //assert
            Assert.IsTrue(_target.Sources.Contains(serviceSourceMock.Object));
            Assert.IsTrue(_targetRequestServiceViewModel.Sources.Contains(serviceSourceMock.Object));
        }

        [TestMethod]
        public void TestUpdateSourcesCollectionAlreadyExist()
        {
            //arrange
            var serviceSourceMock = new Mock<IWebServiceSource>();
            var serviceId = Guid.NewGuid();
            _serviceSourceMock.SetupGet(it => it.Id).Returns(serviceId);
            serviceSourceMock.SetupGet(it => it.Id).Returns(serviceId);
            serviceSourceMock.Setup(it => it.Equals(It.IsAny<IWebServiceSource>()))
                .Returns<IWebServiceSource>((eq) => ReferenceEquals(eq, serviceSourceMock.Object));
            _target.SelectedSource = null;
            _targetRequestServiceViewModel.SelectedSource = null;

            //act
            _updateRepositoryMock.Raise(it => it.WebServiceSourceSaved += null, serviceSourceMock.Object);

            //assert
            Assert.IsFalse(_target.Sources.Contains(_serviceSourceMock.Object));
            Assert.IsFalse(_targetRequestServiceViewModel.Sources.Contains(_serviceSourceMock.Object));
            Assert.IsTrue(_target.Sources.Contains(serviceSourceMock.Object));
            Assert.IsTrue(_targetRequestServiceViewModel.Sources.Contains(serviceSourceMock.Object));
            Assert.IsFalse(_target.CanEditHeadersAndUrl);
            Assert.IsFalse(_targetRequestServiceViewModel.CanEditHeadersAndUrl);
        }

        [TestMethod]
        public void TestUpdateSourcesCollectionAlreadyExistNotNull()
        {
            //arrange
            var serviceSourceMock = new Mock<IWebServiceSource>();
            var serviceId = Guid.NewGuid();
            _serviceSourceMock.SetupGet(it => it.Id).Returns(serviceId);
            serviceSourceMock.SetupGet(it => it.Id).Returns(serviceId);
            serviceSourceMock.Setup(it => it.Equals(It.IsAny<IWebServiceSource>()))
                .Returns<IWebServiceSource>((eq) => ReferenceEquals(eq, serviceSourceMock.Object));
            var selectedSourceMock = new Mock<IWebServiceSource>();
            var expectedHostName = "someHostName";
            var expectedDefaultQuery = "someDefaultQuery";
            selectedSourceMock.SetupGet(it => it.DefaultQuery).Returns(expectedDefaultQuery);
            selectedSourceMock.SetupGet(it => it.HostName).Returns(expectedHostName);
            _target.SelectedSource = selectedSourceMock.Object;
            _targetRequestServiceViewModel.SelectedSource = selectedSourceMock.Object;

            //act
            _updateRepositoryMock.Raise(it => it.WebServiceSourceSaved += null, serviceSourceMock.Object);

            //assert
            Assert.IsFalse(_target.Sources.Contains(_serviceSourceMock.Object));
            Assert.IsFalse(_targetRequestServiceViewModel.Sources.Contains(_serviceSourceMock.Object));
            Assert.IsTrue(_target.Sources.Contains(serviceSourceMock.Object));
            Assert.IsTrue(_targetRequestServiceViewModel.Sources.Contains(serviceSourceMock.Object));
            Assert.IsTrue(_target.CanEditHeadersAndUrl);
            Assert.AreEqual(expectedHostName, _target.SourceUrl);
            Assert.AreEqual(expectedDefaultQuery, _target.RequestUrlQuery);
            Assert.IsTrue(_target.CanEditHeadersAndUrl);
            Assert.AreEqual(expectedHostName, _targetRequestServiceViewModel.SourceUrl);
            Assert.AreEqual(expectedDefaultQuery, _targetRequestServiceViewModel.RequestUrlQuery);
            Assert.IsTrue(_targetRequestServiceViewModel.CanEditHeadersAndUrl);
        }

        [TestMethod]
        public void TestUpdateSourcesCollectionAlreadyExistNotNullDefaultQueryNull()
        {
            //arrange
            var serviceSourceMock = new Mock<IWebServiceSource>();
            var serviceId = Guid.NewGuid();
            _serviceSourceMock.SetupGet(it => it.Id).Returns(serviceId);
            serviceSourceMock.SetupGet(it => it.Id).Returns(serviceId);
            serviceSourceMock.Setup(it => it.Equals(It.IsAny<IWebServiceSource>()))
                .Returns<IWebServiceSource>((eq) => ReferenceEquals(eq, serviceSourceMock.Object));
            var selectedSourceMock = new Mock<IWebServiceSource>();
            var expectedHostName = "someHostName";
            var expectedDefaultQuery = "";
            selectedSourceMock.SetupGet(it => it.DefaultQuery).Returns((string)null);
            selectedSourceMock.SetupGet(it => it.HostName).Returns(expectedHostName);
            _target.SelectedSource = selectedSourceMock.Object;
            _targetRequestServiceViewModel.SelectedSource = selectedSourceMock.Object;

            //act
            _updateRepositoryMock.Raise(it => it.WebServiceSourceSaved += null, serviceSourceMock.Object);

            //assert
            Assert.IsFalse(_target.Sources.Contains(_serviceSourceMock.Object));
            Assert.IsFalse(_targetRequestServiceViewModel.Sources.Contains(_serviceSourceMock.Object));
            Assert.IsTrue(_target.Sources.Contains(serviceSourceMock.Object));
            Assert.IsTrue(_targetRequestServiceViewModel.Sources.Contains(serviceSourceMock.Object));
            Assert.IsTrue(_target.CanEditHeadersAndUrl);
            Assert.AreEqual(expectedHostName, _target.SourceUrl);
            Assert.AreEqual(expectedDefaultQuery, _target.RequestUrlQuery);
            Assert.AreEqual(expectedHostName, _targetRequestServiceViewModel.SourceUrl);
            Assert.AreEqual(expectedDefaultQuery, _targetRequestServiceViewModel.RequestUrlQuery);
            Assert.IsTrue(_targetRequestServiceViewModel.CanEditHeadersAndUrl);
        }

        [TestMethod]
        public void TestVariablesOnCollectionChanged()
        {
            //arrange
            var expectedName = "someName";
            var expectedValue = "someValue";

            //act
            _target.Variables.Add(new NameValue(expectedName, expectedValue));
            _targetRequestServiceViewModel.Variables.Add(new NameValue(expectedName, expectedValue));

            //assert
            Assert.IsTrue(_target.Inputs.Any(item=>item.Name== DataListUtil.RemoveLanguageBrackets(expectedName) && item.Value==expectedValue));
            Assert.IsTrue(
                _targetRequestServiceViewModel.Inputs.Any(
                    item =>
                    item.Name == DataListUtil.RemoveLanguageBrackets(expectedName) && item.Value == expectedValue));
        }

        [TestMethod]
        public void TestHeaderCollectionOnCollectionComplexVariablesNull()
        {
            //arrange
            var expectedNameRecordSet = "[[s(3).s]]";
            var expectedNameScalar = "[[s]]";
            var expectedNameComplex = expectedNameRecordSet + expectedNameScalar;
            var expectedUnusedVar = "someUnusedVar";
            _target.Variables = null;
            _targetRequestServiceViewModel.Variables = null;

            //act
            _target.Headers.Add(new NameValue(expectedNameComplex, expectedNameComplex));
            _targetRequestServiceViewModel.Headers.Add(new NameValue(expectedNameComplex, expectedNameComplex));

            //assert
            Assert.IsNull(_target.Variables);
            Assert.IsNull(_targetRequestServiceViewModel.Variables);
        }

        [TestMethod]
        public void TestHeaderCollectionOnCollectionComplex()
        {
            //arrange
            var expectedNameRecordSet = "[[s(3).s]]";
            var expectedNameScalar = "[[s]]";
            var expectedNameComplex = expectedNameRecordSet + expectedNameScalar;
            var expectedUnusedVar = "someUnusedVar";
            _target.Variables.Add(new NameValue(expectedUnusedVar, "0"));
            _target.RequestBody = "";
            _target.RequestUrlQuery = "";

            //act
            _target.Headers.Add(new NameValue(expectedNameComplex, expectedNameComplex));
            _targetRequestServiceViewModel.Headers.Add(new NameValue(expectedNameComplex, expectedNameComplex));

            //assert
            Assert.IsTrue(_target.Variables.Any(item => item.Name == expectedNameRecordSet && item.Value == ""));
            Assert.IsTrue(
                _targetRequestServiceViewModel.Variables.Any(item => item.Name == expectedNameRecordSet && item.Value == ""));
            Assert.IsTrue(_target.Variables.Any(item => item.Name == expectedNameScalar && item.Value == ""));
            Assert.IsTrue(
                _targetRequestServiceViewModel.Variables.Any(item => item.Name == expectedNameScalar && item.Value == ""));
            Assert.IsFalse(_target.Variables.Any(item => item.Name == expectedUnusedVar && item.Value == ""));
            Assert.IsFalse(
                _targetRequestServiceViewModel.Variables.Any(item => item.Name == expectedUnusedVar && item.Value == ""));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void TestDispose()
        {
            var vm = new ManageWebserviceSourceViewModel();
            var ns = new Mock<IRequestServiceNameViewModel>();

            vm.RequestServiceNameViewModel = ns.Object;

            vm.Dispose();
            ns.Verify(a => a.Dispose());
        }

        [TestMethod]
        public void TestFromModel()
        {
            //arrange
            var serviceMock = new Mock<IWebService>();
            var expectedName = "someName";
            var expectedHeaderText = expectedName;
            var expectedHeader = expectedName + " *";
            var expectedId = Guid.NewGuid();
            var expectedPath = "somePath";
            var expectedRequestUrlQuery = "expectedRequestUrlQuery";
            var sourceHostName = "someSource";
            var expectedItemRequestUrlQuery = "2123";
            var expectedInputs = new List<IServiceInput>();
            var expectedOutputMapping = new List<IServiceOutputMapping>();
            var expectedSourceMock = new Mock<IWebServiceSource>();
            var expectedHeadersItem = new NameValue();
            serviceMock.SetupGet(it => it.Name).Returns(expectedName);
            serviceMock.SetupGet(it => it.Id).Returns(expectedId);
            serviceMock.SetupGet(it => it.Headers).Returns(new List<NameValue>() { expectedHeadersItem });
            serviceMock.SetupGet(it => it.Path).Returns(expectedPath);
            serviceMock.SetupGet(it => it.RequestUrl).Returns(expectedRequestUrlQuery);
            serviceMock.SetupGet(it => it.Inputs).Returns(expectedInputs);
            serviceMock.SetupGet(it => it.OutputMappings).Returns(expectedOutputMapping);
            expectedSourceMock.SetupGet(it => it.Id).Returns(expectedId);
            expectedSourceMock.SetupGet(it => it.HostName).Returns(sourceHostName);
            serviceMock.SetupGet(it => it.QueryString).Returns(sourceHostName + expectedItemRequestUrlQuery);
            _target.Sources.Add(expectedSourceMock.Object);
            var itemMock = new Mock<IWebService>();
            _target.Item = itemMock.Object;

            //act
            _target.FromModel(serviceMock.Object);

            //assert
            Assert.AreEqual(expectedName, _target.Name);
            Assert.AreEqual(expectedHeaderText, _target.HeaderText);
            Assert.AreEqual(expectedHeader, _target.Header);
            Assert.AreEqual(expectedId, _target.Id);
            Assert.AreEqual(expectedPath, _target.Path);
            Assert.AreSame(expectedSourceMock.Object, _target.SelectedSource);
            itemMock.VerifySet(it => it.Source = expectedSourceMock.Object);
            itemMock.VerifySet(it => it.RequestUrl = expectedItemRequestUrlQuery);
            Assert.IsTrue(_target.Headers.Contains(expectedHeadersItem));
            Assert.AreEqual(expectedRequestUrlQuery, _target.RequestUrlQuery);
            Assert.AreSame(expectedInputs, _target.Inputs);
            Assert.AreSame(expectedOutputMapping, _target.OutputMapping);
            Assert.IsTrue(_target.CanEditMappings);
            Assert.IsTrue(_target.CanEditResponse);
        }

        [TestMethod]
        public void TestFromModelSelectedSourceNull()
        {
            //arrange
            var serviceMock = new Mock<IWebService>();
            var expectedName = "someName";
            var expectedHeaderText = expectedName;
            var expectedHeader = expectedName + " *";
            var expectedId = Guid.NewGuid();
            var expectedPath = "somePath";
            var expectedRequestUrlQuery = "expectedRequestUrlQuery";
            var sourceHostName = "someSource";
            var expectedItemRequestUrlQuery = "2123";
            var expectedInputs = new List<IServiceInput>();
            var expectedOutputMapping = new List<IServiceOutputMapping>();
            var expectedSourceMock = new Mock<IWebServiceSource>();
            var expectedHeadersItem = new NameValue();
            serviceMock.SetupGet(it => it.Name).Returns(expectedName);
            serviceMock.SetupGet(it => it.Id).Returns(expectedId);
            serviceMock.SetupGet(it => it.Headers).Returns(new List<NameValue>() { expectedHeadersItem });
            serviceMock.SetupGet(it => it.Path).Returns(expectedPath);
            serviceMock.SetupGet(it => it.RequestUrl).Returns(expectedRequestUrlQuery);
            serviceMock.SetupGet(it => it.Inputs).Returns(expectedInputs);
            serviceMock.SetupGet(it => it.OutputMappings).Returns(expectedOutputMapping);
            var newId = Guid.Empty;
            expectedSourceMock.SetupGet(it => it.Id).Returns(newId);
            expectedSourceMock.SetupGet(it => it.HostName).Returns(sourceHostName);
            serviceMock.SetupGet(it => it.QueryString).Returns(sourceHostName + expectedItemRequestUrlQuery);
            _target.Sources.Add(expectedSourceMock.Object);
            var itemMock = new Mock<IWebService>();
            _target.Item = itemMock.Object;

            //act
            _target.FromModel(serviceMock.Object);

            //assert
            Assert.AreEqual(expectedName, _target.Name);
            Assert.AreEqual(expectedHeaderText, _target.HeaderText);
            Assert.AreEqual(expectedHeader, _target.Header);
            Assert.AreEqual(expectedId, _target.Id);
            Assert.AreEqual(expectedPath, _target.Path);
            Assert.IsNull(_target.SelectedSource);
            itemMock.VerifySet(it => it.Source = null);
            Assert.IsTrue(_target.Headers.Contains(expectedHeadersItem));
            Assert.AreEqual(expectedRequestUrlQuery, _target.RequestUrlQuery);
            Assert.AreSame(expectedInputs, _target.Inputs);
            Assert.AreSame(expectedOutputMapping, _target.OutputMapping);
            Assert.IsTrue(_target.CanEditMappings);
            Assert.IsTrue(_target.CanEditResponse);
        }

        [TestMethod]
        public void TestFromModelHeadersNull()
        {
            //arrange
            var serviceMock = new Mock<IWebService>();
            var expectedName = "someName";
            var expectedHeaderText = expectedName;
            var expectedHeader = expectedName + " *";
            var expectedId = Guid.NewGuid();
            var expectedPath = "somePath";
            var expectedRequestUrlQuery = "expectedRequestUrlQuery";
            var sourceHostName = "someSource";
            var expectedItemRequestUrlQuery = "2123";
            var expectedInputs = new List<IServiceInput>();
            var expectedOutputMapping = new List<IServiceOutputMapping>();
            var expectedSourceMock = new Mock<IWebServiceSource>();
            serviceMock.SetupGet(it => it.Name).Returns(expectedName);
            serviceMock.SetupGet(it => it.Id).Returns(expectedId);
            serviceMock.SetupGet(it => it.Headers).Returns((List<NameValue>)null);
            serviceMock.SetupGet(it => it.Path).Returns(expectedPath);
            serviceMock.SetupGet(it => it.RequestUrl).Returns(expectedRequestUrlQuery);
            serviceMock.SetupGet(it => it.Inputs).Returns(expectedInputs);
            serviceMock.SetupGet(it => it.OutputMappings).Returns(expectedOutputMapping);
            expectedSourceMock.SetupGet(it => it.Id).Returns(expectedId);
            expectedSourceMock.SetupGet(it => it.HostName).Returns(sourceHostName);
            serviceMock.SetupGet(it => it.QueryString).Returns(sourceHostName + expectedItemRequestUrlQuery);
            _target.Sources.Add(expectedSourceMock.Object);
            var itemMock = new Mock<IWebService>();
            _target.Item = itemMock.Object;

            //act
            _target.FromModel(serviceMock.Object);

            //assert
            Assert.AreEqual(expectedName, _target.Name);
            Assert.AreEqual(expectedHeaderText, _target.HeaderText);
            Assert.AreEqual(expectedHeader, _target.Header);
            Assert.AreEqual(expectedId, _target.Id);
            Assert.AreEqual(expectedPath, _target.Path);
            Assert.AreSame(expectedSourceMock.Object, _target.SelectedSource);
            itemMock.VerifySet(it => it.Source = expectedSourceMock.Object);
            itemMock.VerifySet(it => it.RequestUrl = expectedItemRequestUrlQuery);
            serviceMock.VerifySet(it => it.Headers = It.IsNotNull<List<NameValue>>());
            Assert.AreEqual(expectedRequestUrlQuery, _target.RequestUrlQuery);
            Assert.AreSame(expectedInputs, _target.Inputs);
            Assert.AreSame(expectedOutputMapping, _target.OutputMapping);
            Assert.IsTrue(_target.CanEditMappings);
            Assert.IsTrue(_target.CanEditResponse);
        }

        [TestMethod]
        public void TestFromModelHostNameEmpty()
        {
            //arrange
            var serviceMock = new Mock<IWebService>();
            var expectedName = "someName";
            var expectedHeaderText = expectedName;
            var expectedHeader = expectedName + " *";
            var expectedId = Guid.NewGuid();
            var expectedPath = "somePath";
            var expectedRequestUrlQuery = "expectedRequestUrlQuery";
            var sourceHostName = "";
            var expectedItemRequestUrlQuery = "2123";
            var expectedInputs = new List<IServiceInput>();
            var expectedOutputMapping = new List<IServiceOutputMapping>();
            var expectedSourceMock = new Mock<IWebServiceSource>();
            serviceMock.SetupGet(it => it.Name).Returns(expectedName);
            serviceMock.SetupGet(it => it.Id).Returns(expectedId);
            serviceMock.SetupGet(it => it.Headers).Returns((List<NameValue>)null);
            serviceMock.SetupGet(it => it.Path).Returns(expectedPath);
            serviceMock.SetupGet(it => it.RequestUrl).Returns(expectedRequestUrlQuery);
            serviceMock.SetupGet(it => it.Inputs).Returns(expectedInputs);
            serviceMock.SetupGet(it => it.OutputMappings).Returns(expectedOutputMapping);
            expectedSourceMock.SetupGet(it => it.Id).Returns(expectedId);
            expectedSourceMock.SetupGet(it => it.HostName).Returns(sourceHostName);
            expectedSourceMock.SetupGet(it => it.DefaultQuery).Returns(expectedItemRequestUrlQuery);
            _target.Sources.Add(expectedSourceMock.Object);
            var itemMock = new Mock<IWebService>();
            _target.Item = itemMock.Object;

            //act
            _target.FromModel(serviceMock.Object);

            //assert
            Assert.AreEqual(expectedName, _target.Name);
            Assert.AreEqual(expectedHeaderText, _target.HeaderText);
            Assert.AreEqual(expectedHeader, _target.Header);
            Assert.AreEqual(expectedId, _target.Id);
            Assert.AreEqual(expectedPath, _target.Path);
            Assert.AreSame(expectedSourceMock.Object, _target.SelectedSource);
            itemMock.VerifySet(it => it.Source = expectedSourceMock.Object);
            itemMock.VerifySet(it => it.RequestUrl = expectedItemRequestUrlQuery);
            serviceMock.VerifySet(it => it.Headers = It.IsNotNull<List<NameValue>>());
            Assert.AreEqual(expectedRequestUrlQuery, _target.RequestUrlQuery);
            Assert.AreSame(expectedInputs, _target.Inputs);
            Assert.AreSame(expectedOutputMapping, _target.OutputMapping);
            Assert.IsTrue(_target.CanEditMappings);
            Assert.IsTrue(_target.CanEditResponse);
        }

        #endregion Test methods
    }
}