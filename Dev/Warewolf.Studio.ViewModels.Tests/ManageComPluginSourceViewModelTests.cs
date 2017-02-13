using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageComPluginSourceViewModelTests
    {
        #region Fields

        private Mock<IManageComPluginSourceModel> _updateManagerMock;
        private Mock<IEventAggregator> _aggregatorMock;
        private Mock<IAsyncWorker> _asyncWorkerMock;
        private Mock<IComPluginSource> _pluginSourceMock;
        private Mock<IRequestServiceNameViewModel> _requestServiceNameViewModelMock;
        private Task<IRequestServiceNameViewModel> _requestServiceNameViewModelTask;
        private Mock<IFileListing> _selectedDllMock;
        private Action<Action> _dispatcherAction;
        private string _pluginSourceName;
        private string _warewolfServerName;
        private string _selectedDllFullName;

        private List<string> _changedProperties;
        private ManageComPluginSourceViewModel _target;

        private List<string> _changedPropertiesParameterless;
        private ManageComPluginSourceViewModel _targetParameterless;

        private List<string> _changedPropertiesPluginSource;
        private ManageComPluginSourceViewModel _targetPluginSource;

        private List<string> _changedPropertiesRequestServiceNameViewModel;
        private ManageComPluginSourceViewModel _targetRequestServiceNameViewModel;

        private List<string> _changedPropertiesPluginSourceAction;
        private ManageComPluginSourceViewModel _targetPluginSourceAction;

        private List<string> _changedPropertiesRequestServiceNameViewModelAction;
        private ManageComPluginSourceViewModel _targetRequestServiceNameViewModelAction;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateManagerMock = new Mock<IManageComPluginSourceModel>();
            _aggregatorMock = new Mock<IEventAggregator>();
            _asyncWorkerMock = new Mock<IAsyncWorker>();
            _pluginSourceMock = new Mock<IComPluginSource>();
            _requestServiceNameViewModelMock = new Mock<IRequestServiceNameViewModel>();
            _requestServiceNameViewModelTask = Task.FromResult(_requestServiceNameViewModelMock.Object);
            _pluginSourceName = "someName";
            _warewolfServerName = "warewolfServerName";
            _selectedDllFullName = "selectedDllFullName";
            _selectedDllMock = new Mock<IFileListing>();
            _dispatcherAction = action => action();
            _updateManagerMock.SetupGet(it => it.ServerName).Returns(_warewolfServerName);
            _pluginSourceMock.SetupGet(it => it.ResourceName).Returns(_pluginSourceName);
            _pluginSourceMock.SetupGet(it => it.SelectedDll).Returns(_selectedDllMock.Object);
            _selectedDllMock.SetupGet(it => it.FullName).Returns(_selectedDllFullName);
            _updateManagerMock.Setup(model => model.GetComDllListings(It.IsAny<IFileListing>())).Returns(new List<IFileListing> { _selectedDllMock.Object });
            _asyncWorkerMock.Setup(
                it => it.Start(It.IsAny<Action>(), It.IsAny<Action>(), It.IsAny<Action<Exception>>()))
                .Callback<Action, Action, Action<Exception>>(
                    (start, finish, exception) =>
                    {
                        try
                        {
                            start();
                            finish();
                        }
                        catch (Exception e)
                        {
                            exception(e);
                        }
                    });
            _updateManagerMock.Setup(model => model.FetchSource(It.IsAny<Guid>()))
              .Returns(_pluginSourceMock.Object);
            _asyncWorkerMock.Setup(worker =>
                                  worker.Start(
                                           It.IsAny<Func<Tuple<IComPluginSource, List<DllListingModel>>>>(),
                                           It.IsAny<Action<Tuple<IComPluginSource, List<DllListingModel>>>>()))
                           .Callback<Func<Tuple<IComPluginSource, List<DllListingModel>>>, Action<Tuple<IComPluginSource, List<DllListingModel>>>>((func, action) =>
                           {
                               var dbSource = func.Invoke();
                               action(dbSource);
                           });
            _changedPropertiesParameterless = new List<string>();
            _targetParameterless = new ManageComPluginSourceViewModel {DispatcherAction = action => action()};
            _targetParameterless.PropertyChanged += (sender, args) => { _changedPropertiesParameterless.Add(args.PropertyName); };

            _changedProperties = new List<string>();
            _target = new ManageComPluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object,
                _asyncWorkerMock.Object) {DispatcherAction = action => action()};
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };

            _changedPropertiesPluginSource = new List<string>();
            _targetPluginSource = new ManageComPluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object,
                _pluginSourceMock.Object, _asyncWorkerMock.Object) {DispatcherAction = action => action()};
            _targetPluginSource.PropertyChanged += (sender, args) => { _changedPropertiesPluginSource.Add(args.PropertyName); };

            _changedPropertiesRequestServiceNameViewModel = new List<string>();
            _targetRequestServiceNameViewModel = new ManageComPluginSourceViewModel(_updateManagerMock.Object,
                _requestServiceNameViewModelTask, _aggregatorMock.Object, _asyncWorkerMock.Object)
            {
                DispatcherAction = action => action()
            };
            _targetRequestServiceNameViewModel.PropertyChanged += (sender, args) => { _changedPropertiesRequestServiceNameViewModel.Add(args.PropertyName); };

            _changedPropertiesPluginSourceAction = new List<string>();
            _updateManagerMock.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                .Returns(_pluginSourceMock.Object);
            _targetPluginSourceAction = new ManageComPluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _pluginSourceMock.Object, _asyncWorkerMock.Object, _dispatcherAction);
            _targetPluginSourceAction.PropertyChanged += (sender, args) => { _changedPropertiesPluginSourceAction.Add(args.PropertyName); };

            _changedPropertiesRequestServiceNameViewModelAction = new List<string>();
            _targetRequestServiceNameViewModelAction = new ManageComPluginSourceViewModel(_updateManagerMock.Object, _requestServiceNameViewModelTask, _aggregatorMock.Object, _asyncWorkerMock.Object, _dispatcherAction);
            _targetRequestServiceNameViewModelAction.PropertyChanged += (sender, args) => { _changedPropertiesRequestServiceNameViewModelAction.Add(args.PropertyName); };
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManagePluginSourceViewModelAsyncWorkerNull()
        {
            //act
            new ManageComPluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManagePluginSourceViewModelUpdateManagerNull()
        {
            //act
            new ManageComPluginSourceViewModel(null, _aggregatorMock.Object, _asyncWorkerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManagePluginSourceViewModelAggregatorNull()
        {
            //act
            new ManageComPluginSourceViewModel(_updateManagerMock.Object, null, _asyncWorkerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManagePluginSourceViewModelRequestServiceNameViewModelNull()
        {
            //act
            new ManageComPluginSourceViewModel(_updateManagerMock.Object, null, _aggregatorMock.Object, _asyncWorkerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManagePluginSourceViewModePluginSourceNull()
        {
            //act
            new ManageComPluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, null, _asyncWorkerMock.Object);
        }

        #endregion Test construction

        #region Test commands

        [TestMethod]
        public void TestOkCommandCanExecuteAssemblyNameIsEmpty()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            _target.AssemblyName = "";

            //act
            var result = _target.OkCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestOkCommandCanExecuteSelectedDllIsNull()
        {
            //arrange
            _target.SelectedDll = null;

            //act
            var result = _target.OkCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestOkCommandCanExecuteAssemblyNameIsNotGacDll()
        {

            //arrange
            
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            _target.AssemblyName = "someAssemblyName";
            var itemMock = new Mock<IComPluginSource>();
            _target.Item = itemMock.Object;

            //act
            var result = _target.OkCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestOkCommandExecutePluginSourceNullPathNull()
        {
            //arrange
            var expectedName = "someName";
            var selectedDllMock = new Mock<IDllListingModel>();
            selectedDllMock.SetupGet(it => it.FullName).Returns(_selectedDllFullName);
            _targetRequestServiceNameViewModel.SelectedDll = selectedDllMock.Object;
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(null, expectedName));
            _changedPropertiesRequestServiceNameViewModel.Clear();

            //act
            _targetRequestServiceNameViewModel.OkCommand.Execute(null);

            //assert
            Assert.IsTrue(_changedPropertiesRequestServiceNameViewModel.Contains("Header"));
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(expectedName, _targetRequestServiceNameViewModel.Item.ResourceName);
            Assert.AreEqual(expectedName, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreSame(selectedDllMock.Object, _targetRequestServiceNameViewModel.Item.SelectedDll);
            Assert.AreEqual(_targetRequestServiceNameViewModel.HeaderText, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreEqual(_targetRequestServiceNameViewModel.Header, _targetRequestServiceNameViewModel.ResourceName);
        }

        [TestMethod]
        public void TestOkCommandExecutePluginSourceNull()
        {
            //arrange
            var expectedPath = "somePath";
            var expectedName = "someName";
            var selectedDllMock = new Mock<IDllListingModel>();
            selectedDllMock.SetupGet(it => it.FullName).Returns(_selectedDllFullName);
            _targetRequestServiceNameViewModel.SelectedDll = selectedDllMock.Object;
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(expectedPath, expectedName));
            _changedPropertiesRequestServiceNameViewModel.Clear();

            //act
            _targetRequestServiceNameViewModel.OkCommand.Execute(null);

            //assert
            Assert.IsTrue(_changedPropertiesRequestServiceNameViewModel.Contains("Header"));
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(expectedName, _targetRequestServiceNameViewModel.Item.ResourceName);
            Assert.AreEqual(expectedName, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreSame(selectedDllMock.Object, _targetRequestServiceNameViewModel.Item.SelectedDll);
            Assert.AreEqual(_targetRequestServiceNameViewModel.HeaderText, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreEqual(_targetRequestServiceNameViewModel.Header, _targetRequestServiceNameViewModel.ResourceName);
        }

        [TestMethod]
        public void TestOkCommandExecute()
        {
            //arrange
            var expectedId = Guid.NewGuid();
            var expectedName = "someName";
            var expectedPath = "somePath";
            _pluginSourceMock.SetupGet(it => it.Id).Returns(expectedId);
            _pluginSourceMock.SetupGet(it => it.ResourceName).Returns(expectedName);
            _pluginSourceMock.SetupGet(it => it.ResourcePath).Returns(expectedPath);
            var selectedDllMock = new Mock<IDllListingModel>();
            _targetPluginSource.SelectedDll = selectedDllMock.Object;
            _changedPropertiesPluginSource.Clear();

            //act
            _targetPluginSource.OkCommand.Execute(null);

            //assert
            _updateManagerMock.Verify(it=>it.Save(It.IsAny<IComPluginSource>()));
            Assert.IsTrue(_changedPropertiesPluginSource.Contains("Header"));
        }

        [TestMethod]
        public void TestCancelCommandCanExecute()
        {
            //act
            var result = _target.CancelCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCancelPluginSourceCommandExecute()
        {
            //arrange
            var closeActionInvoked = false;
            _target.CloseAction = () =>
            { closeActionInvoked = true; };

            //act
            _target.CancelCommand.Execute(null);

            //assert
            Assert.IsTrue(closeActionInvoked);
        }

        [TestMethod]
        public void TestClearSearchTextCommandCanExecute()
        {
            //act
            var result = _target.ClearSearchTextCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestClearSearchTextCommandExecute()
        {
            //arrange
            _target.SearchTerm = "someSearchTerm";

            //act
            _target.ClearSearchTextCommand.Execute(null);

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(_target.SearchTerm));
        }

        [TestMethod]
        public void TestRefreshCommandCanExecute()
        {
            //act
            var result = _target.RefreshCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestRefreshCommandExecute()
        {
            //arrange    
            var fileListingMock1 = new Mock<IFileListing>();
            var fileListingName1 = "fileListingMock1";
            fileListingMock1.SetupGet(it => it.FullName).Returns(fileListingName1);
            var fileListingMock2 = new Mock<IFileListing>();
            var fileListingName2 = "fileListingMock2";
            fileListingMock2.SetupGet(it => it.FullName).Returns(fileListingName2);
            _updateManagerMock.Setup(it => it.GetComDllListings(null))
                .Returns(new List<IFileListing> { fileListingMock1.Object, fileListingMock2.Object });

            //act
            _target.RefreshCommand.Execute(null);

            //assert
            Assert.IsFalse(_target.IsLoading);
            Assert.IsTrue(_target.DllListings.Any(it => it.FullName == fileListingName1));
            Assert.IsTrue(_target.DllListings.Any(it => it.FullName == fileListingName2));
        }

        #endregion Test commands

        #region Test methods

        [TestMethod]
        public void TestToModel()
        {
            //arrange
            var expectedName = "SomeExpectedName";
            var expectedPath = "SomeExpectedPath";
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            _target.ResourceName = expectedName;
            _target.Path = expectedPath;

            //act
            var result = _target.ToModel();

            //assert
            Assert.AreEqual(expectedName, result.ResourceName);
            Assert.AreSame(selectedDllMock.Object, result.SelectedDll);
        }

        [TestMethod]
        public void TestToModelSource()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _targetPluginSource.SelectedDll = selectedDllMock.Object;

            //act
            var result = _targetPluginSource.ToModel();

            //assert
            Assert.AreEqual(_pluginSourceMock.Object.ResourceName, result.ResourceName);
        }

        [TestMethod]
        public void TestFromModelFileSystem()
        {
            //arrange
            const string ExpectedName = "someexpectedName";
            const string ExpectedPath = "someexpectedPath";
            var pluginSourceMock = new Mock<IComPluginSource>();
            var selectedDllMock = new Mock<IFileListing>();
            const string SelectedDllName = "someSelectedDLLName";
            selectedDllMock.SetupGet(listing => listing.Name).Returns("File System");
            pluginSourceMock.SetupGet(it => it.SelectedDll).Returns(selectedDllMock.Object);
            _pluginSourceMock.SetupGet(it => it.ResourceName).Returns(ExpectedName);
            _pluginSourceMock.SetupGet(it => it.ResourcePath).Returns(ExpectedPath);
            var dllListingMock = new Mock<IDllListingModel>();
            dllListingMock.SetupGet(it => it.Name).Returns("File System");
            var dllListingChildrenMock = new Mock<IDllListingModel>();
            dllListingMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IDllListingModel>() { dllListingChildrenMock.Object });
            dllListingChildrenMock.SetupGet(it => it.Name).Returns(SelectedDllName);
            _targetPluginSource.DllListings = new ObservableCollection<IDllListingModel>() { dllListingMock.Object };

            //act
            _targetPluginSource.FromModel(pluginSourceMock.Object);

            //assert
            dllListingMock.VerifySet(it => it.IsExpanded = true);
            dllListingMock.VerifySet(it => it.IsExpanded = true);
            dllListingMock.VerifySet(it => it.IsSelected = true);
            Assert.AreSame(dllListingMock.Object, _targetPluginSource.SelectedDll);
        }

        [TestMethod]
        public void TestSavePluginSourceNullPathNull()
        {
            //arrange
            var expectedName = "someName";
            var selectedDllMock = new Mock<IDllListingModel>();
            selectedDllMock.SetupGet(it => it.FullName).Returns(_selectedDllFullName);
            _targetRequestServiceNameViewModel.SelectedDll = selectedDllMock.Object;
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(null, expectedName));
            _changedPropertiesRequestServiceNameViewModel.Clear();

            //act
            _targetRequestServiceNameViewModel.Save();

            //assert
            Assert.IsTrue(_changedPropertiesRequestServiceNameViewModel.Contains("Header"));
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(expectedName, _targetRequestServiceNameViewModel.Item.ResourceName);
            Assert.AreEqual(expectedName, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreSame(selectedDllMock.Object, _targetRequestServiceNameViewModel.Item.SelectedDll);
            Assert.AreEqual(_targetRequestServiceNameViewModel.HeaderText, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreEqual(_targetRequestServiceNameViewModel.Header, _targetRequestServiceNameViewModel.ResourceName);
        }

        [TestMethod]
        public void TestSavePluginSourceNull()
        {
            //arrange
            var expectedPath = "somePath";
            var expectedName = "someName";
            var selectedDllMock = new Mock<IDllListingModel>();
            var correctGuid = false;
            selectedDllMock.SetupGet(it => it.FullName).Returns(_selectedDllFullName);
            _targetRequestServiceNameViewModel.SelectedDll = selectedDllMock.Object;
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(expectedPath, expectedName));
            _changedPropertiesRequestServiceNameViewModel.Clear();
            var gd = Guid.NewGuid();
            _targetRequestServiceNameViewModel.SelectedGuid = gd;
            _updateManagerMock.Setup(a => a.Save(It.IsAny<IComPluginSource>())).Callback((IComPluginSource a) => { correctGuid = a.Id ==gd; });
            //act
            _targetRequestServiceNameViewModel.Save();

            //assert
            Assert.IsTrue(_changedPropertiesRequestServiceNameViewModel.Contains("Header"));
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(expectedName, _targetRequestServiceNameViewModel.Item.ResourceName);
            Assert.AreEqual(expectedName, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreSame(selectedDllMock.Object, _targetRequestServiceNameViewModel.Item.SelectedDll);
            Assert.AreEqual(_targetRequestServiceNameViewModel.HeaderText, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreEqual(_targetRequestServiceNameViewModel.Header, _targetRequestServiceNameViewModel.ResourceName);
            _updateManagerMock.Verify(a=>a.Save(It.IsAny<IComPluginSource>()));
            Assert.IsTrue(correctGuid);
        }

        [TestMethod]
        public void TestSave()
        {
            //arrange
            var expectedId = Guid.NewGuid();
            var expectedName = "someName";
            var expectedPath = "somePath";
            _pluginSourceMock.SetupGet(it => it.Id).Returns(expectedId);
            _pluginSourceMock.SetupGet(it => it.ResourceName).Returns(expectedName);
            _pluginSourceMock.SetupGet(it => it.ResourcePath).Returns(expectedPath);
            var selectedDllMock = new Mock<IDllListingModel>();
            _targetPluginSource.SelectedDll = selectedDllMock.Object;
            _changedPropertiesPluginSource.Clear();

            //act
            _targetPluginSource.Save();

            //assert
            _pluginSourceMock.VerifySet(it => it.SelectedDll = It.IsAny<IFileListing>());
            _updateManagerMock.Verify(it => it.Save(It.IsAny<IComPluginSource>()));
            Assert.IsTrue(_changedPropertiesPluginSource.Contains("Header"));
        }

        [TestMethod]
        public void TestCanSaveAssemblyNameIsEmpty()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            _target.AssemblyName = "";

            //act
            var result = _target.CanSave();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanSaveSelectedDllIsNull()
        {
            //arrange
            _target.SelectedDll = null;

            //act
            var result = _target.CanSave();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanSaveAssemblyNameIsNotGacDll()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            _target.AssemblyName = "someAssemblyName";
            var itemMock = new Mock<IComPluginSource>();
            _target.Item = itemMock.Object;

            //act
            var result = _target.CanSave();

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestCanSaveDll()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            selectedDllMock.SetupGet(it => it.ClsId).Returns("ClsId");
            _target.SelectedDll = selectedDllMock.Object;
            _target.AssemblyName = "someAssemblyName.dll";
            var itemMock = new Mock<IComPluginSource>();
            _target.Item = itemMock.Object;

            //act
            var result = _target.CanSave();

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void TestDispose()
        {
            var vm = new ManageComPluginSourceViewModel();
            var ns = new Mock<IRequestServiceNameViewModel>();

            vm.RequestServiceNameViewModel = ns.Object;

            vm.Dispose();
            ns.Verify(a => a.Dispose());
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

        #endregion Test methods

        #region Test properties

        [TestMethod]
        public void TestRequestServiceNameViewModel()
        {
            //arrange
            var expectedValueMock = new Mock<IRequestServiceNameViewModel>();

            //act
            _target.RequestServiceNameViewModel = expectedValueMock.Object;
            var value = _target.RequestServiceNameViewModel;

            //asert
            Assert.AreSame(expectedValueMock.Object, value);
        }

        [TestMethod]
        public void TestRequestServiceNameViewModelNull()
        {
            //act
            var value = _targetPluginSource.RequestServiceNameViewModel;

            //asert
            Assert.IsNull(value);
        }

        [TestMethod]
        public void TestRequestServiceNameViewModelNotNull()
        {
            //act
            var value = _targetRequestServiceNameViewModel.RequestServiceNameViewModel;

            //asert
            Assert.AreSame(_requestServiceNameViewModelMock.Object, value);
        }

        [TestMethod]
        public void TestHeaderText()
        {
            //arrange
            var expectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.HeaderText = expectedValue;
            var value = _target.HeaderText;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("HeaderText"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
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
            Assert.AreEqual(expectedValue, _target.Header);
            Assert.AreEqual(expectedValue, _target.HeaderText);
        }

        [TestMethod]
        public void TestResourceNameLocalhost()
        {
            //arrange
            _warewolfServerName = "localhost";
            _updateManagerMock.SetupGet(it => it.ServerName).Returns(_warewolfServerName);
            _changedProperties = new List<string>();
            _target = new ManageComPluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _asyncWorkerMock.Object);
            _target.DispatcherAction = action => action();
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };
            var expectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.ResourceName = expectedValue;
            var value = _target.ResourceName;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains(expectedValue));
            Assert.AreEqual(expectedValue, _target.Header);
            Assert.AreEqual(expectedValue, _target.HeaderText);
        }

        [TestMethod]
        public void TestResourceNameSource()
        {
            //arrange
            var expectedValue = "someResourceName";
            var pluginSourceName = "pluginSourceName";
            _pluginSourceMock.SetupGet(it => it.ResourceName).Returns(pluginSourceName);
            var expectedHeader = pluginSourceName + " *";
            var expectedHeaderText = pluginSourceName;
            _changedPropertiesPluginSource.Clear();

            //act
            _targetPluginSource.ResourceName = expectedValue;
            var value = _targetPluginSource.ResourceName;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedPropertiesPluginSource.Contains(expectedValue));
            Assert.AreEqual(expectedHeader, _targetPluginSource.Header);
            Assert.AreEqual(expectedHeaderText, _targetPluginSource.HeaderText);
        }

        [TestMethod]
        public void TestResourceNameLocalhostSource()
        {
            //arrange
            _warewolfServerName = "localhost";
            _updateManagerMock.SetupGet(it => it.ServerName).Returns(_warewolfServerName);
            _changedPropertiesPluginSource = new List<string>();
            _targetPluginSource = new ManageComPluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _pluginSourceMock.Object, _asyncWorkerMock.Object);
            _targetPluginSource.DispatcherAction = action => action();
            _targetPluginSource.PropertyChanged += (sender, args) => { _changedPropertiesPluginSource.Add(args.PropertyName); };
            var expectedValue = "someResourceName";
            var pluginSourceName = "pluginSourceName";
            _pluginSourceMock.SetupGet(it => it.ResourceName).Returns(pluginSourceName);
            var expectedHeader = pluginSourceName + " *";
            var expectedHeaderText = pluginSourceName;
            _changedPropertiesPluginSource.Clear();

            //act
            _targetPluginSource.ResourceName = expectedValue;
            var value = _targetPluginSource.ResourceName;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedPropertiesPluginSource.Contains(expectedValue));
            Assert.AreEqual(expectedHeader, _targetPluginSource.Header);
            Assert.AreEqual(expectedHeaderText, _targetPluginSource.HeaderText);
        }

        [TestMethod]
        public void TestAssemblyName()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            var expectedValue = "someAssemblyName";
            _changedProperties.Clear();

            //act
            _target.AssemblyName = expectedValue;
            var value = _target.AssemblyName;

            //asert
            Assert.AreSame(expectedValue, value);
            Assert.AreSame(selectedDllMock.Object, _target.SelectedDll);
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("AssemblyName"));
        }

        [TestMethod]
        public void TestSelectedDll()
        {
            //arrange
            var expectedAssemblyName = "someAssemblyName";
            var expectedValueMock = new Mock<IDllListingModel>();
            expectedValueMock.SetupGet(it => it.FullName).Returns(expectedAssemblyName);
            _changedProperties.Clear();

            //act
            _target.SelectedDll = expectedValueMock.Object;
            var value = _target.SelectedDll;

            //asert
            Assert.AreSame(expectedValueMock.Object, value);
            Assert.IsTrue(_changedProperties.Contains("SelectedDll"));
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
            Assert.AreEqual(expectedValue, _target.ResourceName);
        }

        [TestMethod]
        public void TestDllListings()
        {
            //arrange
            var expectedValue = new ObservableCollection<IDllListingModel>();
            _changedProperties.Clear();

            //act
            _target.DllListings = expectedValue;
            var value = _target.DllListings;

            //asert
            Assert.AreSame(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("DllListings"));
        }

        [TestMethod]
        public void TestIsLoading()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.IsLoading = expectedValue;
            var value = _target.IsLoading;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsLoading"));
        }

        [TestMethod]
        public void TestSearchTerm()
        {
            //arrange
            var expectedValue = "SearchTerm";
            var listingMock = new Mock<IDllListingModel>();
            listingMock.Setup(model => model.Name).Returns("DllSource1");
            var listingMock1 = new Mock<IDllListingModel>();
            listingMock1.Setup(model => model.Name).Returns("SearchTerm");
            var listingMock2 = new Mock<IDllListingModel>();
            listingMock2.Setup(model => model.Name).Returns("DllSource1");
            var originalList = new AsyncObservableCollection<IDllListingModel> { listingMock.Object, listingMock1.Object, listingMock2.Object };
            
            _target.DllListings = new ObservableCollection<IDllListingModel> { listingMock.Object,listingMock1.Object,listingMock2.Object };
            _changedProperties.Clear();

            PrivateObject p = new PrivateObject(_target);
            p.SetField("_originalDllListings", originalList);
            //act
            _target.SearchTerm = expectedValue;
            var value = _target.SearchTerm;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("DllListings"));
            Assert.IsTrue(_changedProperties.Contains("SearchTerm"));

            Assert.AreEqual(1,_target.DllListings.Count);
            Assert.AreEqual(expectedValue, _target.DllListings[0].Name);

            expectedValue = "";
            //act
            _target.SearchTerm = expectedValue;
            value = _target.SearchTerm;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("DllListings"));
            Assert.IsTrue(_changedProperties.Contains("SearchTerm"));

            Assert.AreEqual(3, _target.DllListings.Count);
        }

        #endregion Test properties
    }
}