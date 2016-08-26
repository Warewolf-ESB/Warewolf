using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManagePluginSourceViewModelTests
    {
        #region Fields

        private Mock<IManagePluginSourceModel> _updateManagerMock;
        private Mock<IEventAggregator> _aggregatorMock;
        private Mock<IAsyncWorker> _asyncWorkerMock;
        private Mock<IPluginSource> _pluginSourceMock;
        private Mock<IRequestServiceNameViewModel> _requestServiceNameViewModelMock;
        private Task<IRequestServiceNameViewModel> _requestServiceNameViewModelTask;
        private Mock<IFileListing> _selectedDllMock;
        private Action<Action> _dispatcherAction;
        private string _pluginSourceName;
        private string _warewolfServerName;
        private string _selectedDllFullName;

        private List<string> _changedProperties;
        private ManagePluginSourceViewModel _target;

        private List<string> _changedPropertiesParameterless;
        private ManagePluginSourceViewModel _targetParameterless;

        private List<string> _changedPropertiesPluginSource;
        private ManagePluginSourceViewModel _targetPluginSource;

        private List<string> _changedPropertiesRequestServiceNameViewModel;
        private ManagePluginSourceViewModel _targetRequestServiceNameViewModel;

        private List<string> _changedPropertiesPluginSourceAction;
        private ManagePluginSourceViewModel _targetPluginSourceAction;

        private List<string> _changedPropertiesRequestServiceNameViewModelAction;
        private ManagePluginSourceViewModel _targetRequestServiceNameViewModelAction;
        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateManagerMock = new Mock<IManagePluginSourceModel>();
            _aggregatorMock = new Mock<IEventAggregator>();
            _asyncWorkerMock = new Mock<IAsyncWorker>();
            _pluginSourceMock = new Mock<IPluginSource>();
            _requestServiceNameViewModelMock = new Mock<IRequestServiceNameViewModel>();
            _requestServiceNameViewModelTask = Task.FromResult(_requestServiceNameViewModelMock.Object);
            _pluginSourceName = "someName";
            _warewolfServerName = "warewolfServerName";
            _selectedDllFullName = "selectedDllFullName";
            _selectedDllMock = new Mock<IFileListing>();
            _dispatcherAction = action => action();
            _updateManagerMock.SetupGet(it => it.ServerName).Returns(_warewolfServerName);
            _pluginSourceMock.SetupGet(it => it.Name).Returns(_pluginSourceName);
            _pluginSourceMock.SetupGet(it => it.SelectedDll).Returns(_selectedDllMock.Object);
            _selectedDllMock.SetupGet(it => it.FullName).Returns(_selectedDllFullName);
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

            _changedPropertiesParameterless = new List<string>();
            _targetParameterless = new ManagePluginSourceViewModel { DispatcherAction = action => action() };
            _targetParameterless.PropertyChanged += (sender, args) => { _changedPropertiesParameterless.Add(args.PropertyName); };

            _changedProperties = new List<string>();
            _target = new ManagePluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _asyncWorkerMock.Object) { DispatcherAction = action => action() };
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };

            _changedPropertiesPluginSource = new List<string>();
            _targetPluginSource = new ManagePluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _pluginSourceMock.Object, _asyncWorkerMock.Object) { DispatcherAction = action => action() };
            _targetPluginSource.PropertyChanged += (sender, args) => { _changedPropertiesPluginSource.Add(args.PropertyName); };

            _changedPropertiesRequestServiceNameViewModel = new List<string>();
            _targetRequestServiceNameViewModel = new ManagePluginSourceViewModel(_updateManagerMock.Object, _requestServiceNameViewModelTask, _aggregatorMock.Object, _asyncWorkerMock.Object) { DispatcherAction = action => action() };
            _targetRequestServiceNameViewModel.PropertyChanged += (sender, args) => { _changedPropertiesRequestServiceNameViewModel.Add(args.PropertyName); };

            _changedPropertiesPluginSourceAction = new List<string>();
            _targetPluginSourceAction = new ManagePluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _pluginSourceMock.Object, _asyncWorkerMock.Object, _dispatcherAction);
            _targetPluginSourceAction.PropertyChanged += (sender, args) => { _changedPropertiesPluginSourceAction.Add(args.PropertyName); };

            _changedPropertiesRequestServiceNameViewModelAction = new List<string>();
            _targetRequestServiceNameViewModelAction = new ManagePluginSourceViewModel(_updateManagerMock.Object, _requestServiceNameViewModelTask, _aggregatorMock.Object, _asyncWorkerMock.Object, _dispatcherAction);
            _targetRequestServiceNameViewModelAction.PropertyChanged += (sender, args) => { _changedPropertiesRequestServiceNameViewModelAction.Add(args.PropertyName); };
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManagePluginSourceViewModelAsyncWorkerNull()
        {
            //act
            new ManagePluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManagePluginSourceViewModelUpdateManagerNull()
        {
            //act
            new ManagePluginSourceViewModel(null, _aggregatorMock.Object, _asyncWorkerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManagePluginSourceViewModelAggregatorNull()
        {
            //act
            new ManagePluginSourceViewModel(_updateManagerMock.Object, null, _asyncWorkerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManagePluginSourceViewModelRequestServiceNameViewModelNull()
        {
            //act
            new ManagePluginSourceViewModel(_updateManagerMock.Object, null, _aggregatorMock.Object, _asyncWorkerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManagePluginSourceViewModePluginSourceNull()
        {
            //act
            new ManagePluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, null, _asyncWorkerMock.Object);
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
            var itemMock = new Mock<IPluginSource>();
            _target.Item = itemMock.Object;

            //act
            var result = _target.OkCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestOkCommandCanExecuteDll()
        {
            //arrange
            string someassemblynameDll = EnvironmentVariables.WorkspacePath + @"\someAssemblyName.dll";
            using (File.Create(someassemblynameDll))
            {
            }
            var selectedDllMock = new Mock<IDllListingModel>();
            selectedDllMock.SetupGet(model => model.FullName).Returns(someassemblynameDll);
            _target.AssemblyName = someassemblynameDll;
            _target.SelectedDll = selectedDllMock.Object;
            var itemMock = new Mock<IPluginSource>();
            _target.Item = itemMock.Object;

            //act
            var result = _target.OkCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
            File.Delete(someassemblynameDll);
        }

        [TestMethod]
        public void TestOkCommandCanExecuteGac()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            _target.AssemblyName = "GAC:someAssemblyName";
            var itemMock = new Mock<IPluginSource>();
            _target.Item = itemMock.Object;

            //act
            var result = _target.OkCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestOkCommandExecutePluginSourceNullPathNull()
        {
            //arrange
            const string ExpectedName = "someName";
            var selectedDllMock = new Mock<IDllListingModel>();
            selectedDllMock.SetupGet(it => it.FullName).Returns(_selectedDllFullName);
            _targetRequestServiceNameViewModel.SelectedDll = selectedDllMock.Object;
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(null, ExpectedName));
            _changedPropertiesRequestServiceNameViewModel.Clear();

            //act
            _targetRequestServiceNameViewModel.OkCommand.Execute(null);

            //assert
            Assert.IsTrue(_changedPropertiesRequestServiceNameViewModel.Contains("Header"));
            Assert.AreEqual(_selectedDllFullName, _targetRequestServiceNameViewModel.AssemblyName);
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Path);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Name);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreSame(selectedDllMock.Object, _targetRequestServiceNameViewModel.Item.SelectedDll);
            Assert.AreEqual(_targetRequestServiceNameViewModel.HeaderText, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreEqual(_targetRequestServiceNameViewModel.Header, _targetRequestServiceNameViewModel.ResourceName);
        }

        [TestMethod]
        public void TestOkCommandExecutePluginSourceNull()
        {
            //arrange
            const string ExpectedPath = "somePath";
            const string ExpectedName = "someName";
            var selectedDllMock = new Mock<IDllListingModel>();
            selectedDllMock.SetupGet(it => it.FullName).Returns(_selectedDllFullName);
            _targetRequestServiceNameViewModel.SelectedDll = selectedDllMock.Object;
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(ExpectedPath, ExpectedName));
            _changedPropertiesRequestServiceNameViewModel.Clear();

            //act
            _targetRequestServiceNameViewModel.OkCommand.Execute(null);

            //assert
            Assert.IsTrue(_changedPropertiesRequestServiceNameViewModel.Contains("Header"));
            Assert.AreEqual(_selectedDllFullName, _targetRequestServiceNameViewModel.AssemblyName);
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(ExpectedPath, _targetRequestServiceNameViewModel.Item.Path);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Name);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreSame(selectedDllMock.Object, _targetRequestServiceNameViewModel.Item.SelectedDll);
            Assert.AreEqual(_targetRequestServiceNameViewModel.HeaderText, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreEqual(_targetRequestServiceNameViewModel.Header, _targetRequestServiceNameViewModel.ResourceName);
        }

        [TestMethod]
        public void TestOkCommandExecute()
        {
            //arrange
            var expectedId = Guid.NewGuid();
            const string ExpectedName = "someName";
            const string ExpectedPath = "somePath";
            _pluginSourceMock.SetupGet(it => it.Id).Returns(expectedId);
            _pluginSourceMock.SetupGet(it => it.Name).Returns(ExpectedName);
            _pluginSourceMock.SetupGet(it => it.Path).Returns(ExpectedPath);
            var selectedDllMock = new Mock<IDllListingModel>();
            _targetPluginSource.SelectedDll = selectedDllMock.Object;
            _changedPropertiesPluginSource.Clear();

            //act
            _targetPluginSource.OkCommand.Execute(null);

            //assert
            _pluginSourceMock.VerifySet(it => it.SelectedDll = It.IsAny<IFileListing>());
            _updateManagerMock.Verify(it => it.Save(_pluginSourceMock.Object));
            Assert.IsTrue(_changedPropertiesPluginSource.Contains("Header"));
            Assert.AreEqual(_selectedDllFullName, _targetPluginSource.AssemblyName);
            Assert.AreEqual(expectedId, _targetPluginSource.Item.Id);
            Assert.AreEqual(ExpectedName, _targetPluginSource.Item.Name);
            Assert.AreEqual(ExpectedPath, _targetPluginSource.Item.Path);
            Assert.AreSame(selectedDllMock.Object, _targetPluginSource.Item.SelectedDll);
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
            const string FileListingName1 = "fileListingMock1";
            fileListingMock1.SetupGet(it => it.FullName).Returns(FileListingName1);
            var fileListingMock2 = new Mock<IFileListing>();
            const string FileListingName2 = "fileListingMock2";
            fileListingMock2.SetupGet(it => it.FullName).Returns(FileListingName2);
            _updateManagerMock.Setup(it => it.GetDllListings(null))
                .Returns(new List<IFileListing> { fileListingMock1.Object, fileListingMock2.Object });

            //act
            _target.RefreshCommand.Execute(null);

            //assert
            Assert.IsFalse(_target.IsLoading);
            Assert.IsTrue(_target.DllListings.Any(it => it.FullName == FileListingName1));
            Assert.IsTrue(_target.DllListings.Any(it => it.FullName == FileListingName2));
            Assert.AreEqual(FileListingName2, _target.GacItem.FullName);
        }

        #endregion Test commands

        #region Test methods

        [TestMethod]
        public void TestToModel()
        {
            //arrange
            const string ExpectedName = "SomeExpectedName";
            const string ExpectedPath = "SomeExpectedPath";
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            _target.ResourceName = ExpectedName;
            _target.Path = ExpectedPath;

            //act
            var result = _target.ToModel();

            //assert
            Assert.AreEqual(ExpectedName, result.Name);
            Assert.AreSame(selectedDllMock.Object, result.SelectedDll);
            Assert.AreEqual(ExpectedPath, result.Path);
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
            Assert.AreSame(_pluginSourceMock.Object, result);
            _pluginSourceMock.VerifySet(it => it.SelectedDll = selectedDllMock.Object);
        }

        [TestMethod]
        public void TestFromModelGAC()
        {
            //arrange
            const string ExpectedName = "someexpectedName";
            const string ExpectedPath = "someexpectedPath";
            var pluginSourceMock = new Mock<IPluginSource>();
            var selectedDllMock = new Mock<IFileListing>();
            const string SelectedDllFullName = "GAC:someSelectedDLLFullName";
            selectedDllMock.SetupGet(it => it.FullName).Returns(SelectedDllFullName);
            pluginSourceMock.SetupGet(it => it.SelectedDll).Returns(selectedDllMock.Object);
            _pluginSourceMock.SetupGet(it => it.Name).Returns(ExpectedName);
            _pluginSourceMock.SetupGet(it => it.Path).Returns(ExpectedPath);
            var dllListingMock = new Mock<IDllListingModel>();
            dllListingMock.SetupGet(it => it.Name).Returns("GAC");
            var dllListingChildreMock = new Mock<IDllListingModel>();
            dllListingMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IDllListingModel>() { dllListingChildreMock.Object });
            dllListingChildreMock.SetupGet(it => it.FullName).Returns(SelectedDllFullName);
            _targetPluginSource.DllListings = new List<IDllListingModel>() { dllListingMock.Object };

            //act
            _targetPluginSource.FromModel(pluginSourceMock.Object);

            //assert
            dllListingMock.VerifySet(it => it.IsExpanded = true);
            dllListingChildreMock.VerifySet(it => it.IsSelected = true);
            Assert.AreSame(dllListingChildreMock.Object, _targetPluginSource.SelectedDll);
            Assert.AreEqual(ExpectedName, _targetPluginSource.Name);
            Assert.AreEqual(ExpectedPath, _targetPluginSource.Path);
        }

        [TestMethod]
        public void TestFromModelFileSystem()
        {
            //arrange
            const string ExpectedName = "someexpectedName";
            const string ExpectedPath = "someexpectedPath";
            var pluginSourceMock = new Mock<IPluginSource>();
            var selectedDllMock = new Mock<IFileListing>();
            const string SelectedDllFullName = "someSelectedDLLFullName";
            selectedDllMock.SetupGet(it => it.FullName).Returns(SelectedDllFullName);
            pluginSourceMock.SetupGet(it => it.SelectedDll).Returns(selectedDllMock.Object);
            _pluginSourceMock.SetupGet(it => it.Name).Returns(ExpectedName);
            _pluginSourceMock.SetupGet(it => it.Path).Returns(ExpectedPath);
            var dllListingMock = new Mock<IDllListingModel>();
            dllListingMock.SetupGet(it => it.Name).Returns("File System");
            var dllListingChildrenMock = new Mock<IDllListingModel>();
            dllListingMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IDllListingModel>() { dllListingChildrenMock.Object });
            dllListingChildrenMock.SetupGet(it => it.Name).Returns(SelectedDllFullName);
            _targetPluginSource.DllListings = new List<IDllListingModel>() { dllListingMock.Object };

            //act
            _targetPluginSource.FromModel(pluginSourceMock.Object);

            //assert
            dllListingMock.VerifySet(it => it.IsExpanded = true);
            dllListingChildrenMock.VerifySet(it => it.IsExpanded = true);
            dllListingChildrenMock.VerifySet(it => it.IsSelected = true);
            Assert.AreSame(dllListingChildrenMock.Object, _targetPluginSource.SelectedDll);
            Assert.AreEqual(ExpectedName, _targetPluginSource.Name);
            Assert.AreEqual(ExpectedPath, _targetPluginSource.Path);
        }

        [TestMethod]
        public void TestSavePluginSourceNullPathNull()
        {
            //arrange
            const string ExpectedName = "someName";
            var selectedDllMock = new Mock<IDllListingModel>();
            selectedDllMock.SetupGet(it => it.FullName).Returns(_selectedDllFullName);
            _targetRequestServiceNameViewModel.SelectedDll = selectedDllMock.Object;
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(null, ExpectedName));
            _changedPropertiesRequestServiceNameViewModel.Clear();

            //act
            _targetRequestServiceNameViewModel.Save();

            //assert
            Assert.IsTrue(_changedPropertiesRequestServiceNameViewModel.Contains("Header"));
            Assert.AreEqual(_selectedDllFullName, _targetRequestServiceNameViewModel.AssemblyName);
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Path);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Name);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreSame(selectedDllMock.Object, _targetRequestServiceNameViewModel.Item.SelectedDll);
            Assert.AreEqual(_targetRequestServiceNameViewModel.HeaderText, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreEqual(_targetRequestServiceNameViewModel.Header, _targetRequestServiceNameViewModel.ResourceName);
        }

        [TestMethod]
        public void TestSavePluginSourceNull()
        {
            //arrange
            const string ExpectedPath = "somePath";
            const string ExpectedName = "someName";
            var selectedDllMock = new Mock<IDllListingModel>();
            var correctGuid = false;
            selectedDllMock.SetupGet(it => it.FullName).Returns(_selectedDllFullName);
            _targetRequestServiceNameViewModel.SelectedDll = selectedDllMock.Object;
            _requestServiceNameViewModelMock.Setup(it => it.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            _requestServiceNameViewModelMock.SetupGet(it => it.ResourceName)
                .Returns(new ResourceName(ExpectedPath, ExpectedName));
            _changedPropertiesRequestServiceNameViewModel.Clear();
            var gd = Guid.NewGuid();
            _targetRequestServiceNameViewModel.SelectedGuid = gd;
            _updateManagerMock.Setup(a => a.Save(It.IsAny<IPluginSource>())).Callback((IPluginSource a) => { correctGuid = a.Id == gd; });
            //act
            _targetRequestServiceNameViewModel.Save();

            //assert
            Assert.IsTrue(_changedPropertiesRequestServiceNameViewModel.Contains("Header"));
            Assert.AreEqual(_selectedDllFullName, _targetRequestServiceNameViewModel.AssemblyName);
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(ExpectedPath, _targetRequestServiceNameViewModel.Item.Path);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Name);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreSame(selectedDllMock.Object, _targetRequestServiceNameViewModel.Item.SelectedDll);
            Assert.AreEqual(_targetRequestServiceNameViewModel.HeaderText, _targetRequestServiceNameViewModel.ResourceName);
            Assert.AreEqual(_targetRequestServiceNameViewModel.Header, _targetRequestServiceNameViewModel.ResourceName);
            _updateManagerMock.Verify(a => a.Save(It.IsAny<IPluginSource>()));
            Assert.IsTrue(correctGuid);
        }

        [TestMethod]
        public void TestSave()
        {
            //arrange
            var expectedId = Guid.NewGuid();
            const string ExpectedName = "someName";
            const string ExpectedPath = "somePath";
            _pluginSourceMock.SetupGet(it => it.Id).Returns(expectedId);
            _pluginSourceMock.SetupGet(it => it.Name).Returns(ExpectedName);
            _pluginSourceMock.SetupGet(it => it.Path).Returns(ExpectedPath);
            var selectedDllMock = new Mock<IDllListingModel>();
            _targetPluginSource.SelectedDll = selectedDllMock.Object;
            _changedPropertiesPluginSource.Clear();

            //act
            _targetPluginSource.Save();

            //assert
            _pluginSourceMock.VerifySet(it => it.SelectedDll = It.IsAny<IFileListing>());
            _updateManagerMock.Verify(it => it.Save(_pluginSourceMock.Object));
            Assert.IsTrue(_changedPropertiesPluginSource.Contains("Header"));
            Assert.AreEqual(_selectedDllFullName, _targetPluginSource.AssemblyName);
            Assert.AreEqual(expectedId, _targetPluginSource.Item.Id);
            Assert.AreEqual(ExpectedName, _targetPluginSource.Item.Name);
            Assert.AreEqual(ExpectedPath, _targetPluginSource.Item.Path);
            Assert.AreSame(selectedDllMock.Object, _targetPluginSource.Item.SelectedDll);
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
            var itemMock = new Mock<IPluginSource>();
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
            var someassemblynameDll = EnvironmentVariables.WorkspacePath + @"\someAssemblyName.dll";
            using (File.Create(someassemblynameDll))
            {
            }
            var selectedDllMock = new Mock<IDllListingModel>();
            selectedDllMock.Setup(model => model.FullName).Returns(someassemblynameDll);
            _target.AssemblyName = someassemblynameDll;
            var itemMock = new Mock<IPluginSource>();
            _target.Item = itemMock.Object;

            //act
            var result = _target.CanSave();

            //assert
            Assert.IsTrue(result);
            File.Delete(someassemblynameDll);
        }

        [TestMethod]
        public void TestCanSaveGac()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            _target.AssemblyName = "GAC:someAssemblyName";
            var itemMock = new Mock<IPluginSource>();
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
            var vm = new ManagePluginSourceViewModel();
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
            const string HelpText = "someText";

            //act
            _target.UpdateHelpDescriptor(HelpText);

            //assert
            helpViewModelMock.Verify(it => it.UpdateHelpText(HelpText));
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
            const string ExpectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.HeaderText = ExpectedValue;
            var value = _target.HeaderText;

            //asert
            Assert.AreEqual(ExpectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("HeaderText"));
            Assert.IsTrue(_changedProperties.Contains("Header"));
        }

        [TestMethod]
        public void TestResourceName()
        {
            //arrange
            const string ExpectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.ResourceName = ExpectedValue;
            var value = _target.ResourceName;

            //asert
            Assert.AreEqual(ExpectedValue, value);
            Assert.IsTrue(_changedProperties.Contains(ExpectedValue));
            Assert.AreEqual(ExpectedValue, _target.Header);
            Assert.AreEqual(ExpectedValue, _target.HeaderText);
        }

        [TestMethod]
        public void TestResourceNameLocalhost()
        {
            //arrange
            _warewolfServerName = "localhost";
            _updateManagerMock.SetupGet(it => it.ServerName).Returns(_warewolfServerName);
            _changedProperties = new List<string>();
            _target = new ManagePluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _asyncWorkerMock.Object) { DispatcherAction = action => action() };
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };
            const string ExpectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.ResourceName = ExpectedValue;
            var value = _target.ResourceName;

            //asert
            Assert.AreEqual(ExpectedValue, value);
            Assert.IsTrue(_changedProperties.Contains(ExpectedValue));
            Assert.AreEqual(ExpectedValue, _target.Header);
            Assert.AreEqual(ExpectedValue, _target.HeaderText);
        }

        [TestMethod]
        public void TestResourceNameSource()
        {
            //arrange
            const string ExpectedValue = "someResourceName";
            const string PluginSourceName = "pluginSourceName";
            _pluginSourceMock.SetupGet(it => it.Name).Returns(PluginSourceName);
            const string ExpectedHeader = PluginSourceName + " *";
            const string ExpectedHeaderText = PluginSourceName;
            _changedPropertiesPluginSource.Clear();

            //act
            _targetPluginSource.ResourceName = ExpectedValue;
            var value = _targetPluginSource.ResourceName;

            //asert
            Assert.AreEqual(ExpectedValue, value);
            Assert.IsTrue(_changedPropertiesPluginSource.Contains(ExpectedValue));
            Assert.AreEqual(ExpectedHeader, _targetPluginSource.Header);
            Assert.AreEqual(ExpectedHeaderText, _targetPluginSource.HeaderText);
        }

        [TestMethod]
        public void TestResourceNameLocalhostSource()
        {
            //arrange
            _warewolfServerName = "localhost";
            _updateManagerMock.SetupGet(it => it.ServerName).Returns(_warewolfServerName);
            _changedPropertiesPluginSource = new List<string>();
            _targetPluginSource = new ManagePluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _pluginSourceMock.Object, _asyncWorkerMock.Object) { DispatcherAction = action => action() };
            _targetPluginSource.PropertyChanged += (sender, args) => { _changedPropertiesPluginSource.Add(args.PropertyName); };
            const string ExpectedValue = "someResourceName";
            const string PluginSourceName = "pluginSourceName";
            _pluginSourceMock.SetupGet(it => it.Name).Returns(PluginSourceName);
            const string ExpectedHeader = PluginSourceName + " *";
            const string ExpectedHeaderText = PluginSourceName;
            _changedPropertiesPluginSource.Clear();

            //act
            _targetPluginSource.ResourceName = ExpectedValue;
            var value = _targetPluginSource.ResourceName;

            //asert
            Assert.AreEqual(ExpectedValue, value);
            Assert.IsTrue(_changedPropertiesPluginSource.Contains(ExpectedValue));
            Assert.AreEqual(ExpectedHeader, _targetPluginSource.Header);
            Assert.AreEqual(ExpectedHeaderText, _targetPluginSource.HeaderText);
        }

        [TestMethod]
        public void TestAssemblyName()
        {
            //arrange
            const string SomeassemblynameDll = "someAssemblyName.dll";
            var selectedDllMock = new Mock<IDllListingModel>();
            selectedDllMock.SetupGet(model => model.FullName).Returns(SomeassemblynameDll);
            _target.SelectedDll = selectedDllMock.Object;
            const string ExpectedValue = "someAssemblyName";
            _changedProperties.Clear();

            //act
            _target.AssemblyName = ExpectedValue;
            var value = _target.AssemblyName;

            //asert
            Assert.AreSame(ExpectedValue, value);
            Assert.AreSame(selectedDllMock.Object, _target.SelectedDll);
            Assert.IsTrue(_changedProperties.Contains("Header"));
            Assert.IsTrue(_changedProperties.Contains("AssemblyName"));
        }

        [TestMethod]
        public void TestSelectedDll()
        {
            //arrange
            var expectedAssemblyName = EnvironmentVariables.WorkspacePath + @"\someAssemblyName.dll";
            using (File.Create(expectedAssemblyName))
            {
            }
            var expectedValueMock = new Mock<IDllListingModel>();
            expectedValueMock.SetupGet(it => it.FullName).Returns(expectedAssemblyName);
            _changedProperties.Clear();
            //act
            _target.SelectedDll = expectedValueMock.Object;
            var value = _target.SelectedDll;
            //asert
            Assert.AreSame(expectedValueMock.Object, value);
            Assert.IsTrue(_changedProperties.Contains("SelectedDll"));
            expectedValueMock.VerifySet(it => it.IsExpanded = true);
            Assert.AreEqual(expectedAssemblyName, _target.AssemblyName);
            File.Delete(expectedAssemblyName);
        }

        [TestMethod]
        public void TestName()
        {
            //arrange
            const string ExpectedValue = "someValue";
            _changedProperties.Clear();

            //act
            _target.Name = ExpectedValue;
            var value = _target.Name;

            //asert
            Assert.AreEqual(ExpectedValue, value);
            Assert.AreEqual(ExpectedValue, _target.ResourceName);
        }

        [TestMethod]
        public void TestDllListings()
        {
            //arrange
            var expectedValue = new List<IDllListingModel>();
            _changedProperties.Clear();

            //act
            _target.DllListings = expectedValue;
            var value = _target.DllListings;

            //asert
            Assert.AreSame(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("DllListings"));
        }

        [TestMethod]
        public void TestGacItem()
        {
            //arrange
            var expectedValueMock = new Mock<IDllListingModel>();
            _changedProperties.Clear();

            //act
            _target.GacItem = expectedValueMock.Object;
            var value = _target.GacItem;

            //asert
            Assert.AreSame(expectedValueMock.Object, value);
            Assert.IsTrue(_changedProperties.Contains("GacItem"));
        }

        [TestMethod]
        public void TestIsLoading()
        {
            //arrange
            const bool ExpectedValue = true;
            _changedProperties.Clear();

            //act
            _target.IsLoading = ExpectedValue;
            var value = _target.IsLoading;

            //asert
            Assert.AreEqual(ExpectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsLoading"));
        }

        [TestMethod]
        public void TestSearchTerm()
        {
            //arrange
            const string ExpectedValue = "SearchTerm";
            var listingMock = new Mock<IDllListingModel>();
            _target.DllListings = new List<IDllListingModel> { listingMock.Object };
            _changedProperties.Clear();

            //act
            _target.SearchTerm = ExpectedValue;
            var value = _target.SearchTerm;

            //asert
            Assert.AreEqual(ExpectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("DllListings"));
            Assert.IsTrue(_changedProperties.Contains("SearchTerm"));
            listingMock.Verify(it => it.Filter(ExpectedValue));
        }

        #endregion Test properties
    }
}