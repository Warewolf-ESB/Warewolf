using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManagePluginSourceViewModelTests
    {
        #region Fields

        private Mock<IManagePluginSourceModel> _updateManagerMock;
        private Mock<IRequestServiceNameViewModel> _requestServiceNameViewModelMock;
        private Task<IRequestServiceNameViewModel> _requestServiceNameViewModelTask;
        private Mock<IFileListing> _selectedDllMock;
        private Mock<IPluginSource> _pluginSourceMock;
        private Mock<IEventAggregator> _aggregatorMock;
        private Mock<IAsyncWorker> _asyncWorkerMock;

        private string _pluginSourceName;
        private string _warewolfServerName;
        private string _selectedDllFullName;

        private List<string> _changedProperties;
        private ManagePluginSourceViewModel _target;
        private List<string> _changedPropertiesSource;
        private ManagePluginSourceViewModel _targetSource;

        private List<string> _changedPropertiesRequestServiceNameViewModel;
        private ManagePluginSourceViewModel _targetRequestServiceNameViewModel;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateManagerMock = new Mock<IManagePluginSourceModel>();
            _requestServiceNameViewModelMock = new Mock<IRequestServiceNameViewModel>();
            _aggregatorMock = new Mock<IEventAggregator>();
            _asyncWorkerMock = new Mock<IAsyncWorker>();
            _pluginSourceMock = new Mock<IPluginSource>();
            _selectedDllMock = new Mock<IFileListing>();
            _pluginSourceName = "someName";
            _warewolfServerName = "warewolfServerName";
            _selectedDllFullName = "selectedDllFullName";
            _updateManagerMock.SetupGet(it => it.ServerName).Returns(_warewolfServerName);
            _updateManagerMock.Setup(it => it.GetDllListings(null)).Returns(new List<IFileListing>());
            _pluginSourceMock.SetupGet(it => it.Name).Returns(_pluginSourceName);
            _pluginSourceMock.SetupGet(it => it.Name).Returns(_pluginSourceName);
            _pluginSourceMock.SetupGet(it => it.SelectedDll).Returns(_selectedDllMock.Object);
            _updateManagerMock.Setup(model => model.FetchSource(It.IsAny<Guid>()))
             .Returns(_pluginSourceMock.Object);
            _requestServiceNameViewModelTask = Task.FromResult(_requestServiceNameViewModelMock.Object);
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
            _updateManagerMock.Setup(model => model.FetchSource(It.IsAny<Guid>()))
            .Returns(_pluginSourceMock.Object);
            _asyncWorkerMock.Setup(worker =>
                                   worker.Start(
                                            It.IsAny<Func<IPluginSource>>(),
                                            It.IsAny<Action<IPluginSource>>()))
                            .Callback<Func<IPluginSource>, Action<IPluginSource>>((func, action) =>
                            {
                                var dbSource = func.Invoke();
                                action(dbSource);
                            });
            _changedProperties = new List<string>();
            _target = new ManagePluginSourceViewModel(_updateManagerMock.Object, _requestServiceNameViewModelTask, _aggregatorMock.Object, _asyncWorkerMock.Object);
            _target.PropertyChanged += (sender, e) => { _changedProperties.Add(e.PropertyName); };

            _changedPropertiesSource = new List<string>();
         
            _targetSource = new ManagePluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _pluginSourceMock.Object, _asyncWorkerMock.Object);
            _targetSource.PropertyChanged += (sender, e) => { _changedPropertiesSource.Add(e.PropertyName); };
            
            _changedPropertiesRequestServiceNameViewModel = new List<string>();
            _targetRequestServiceNameViewModel = new ManagePluginSourceViewModel(_updateManagerMock.Object, _requestServiceNameViewModelTask, _aggregatorMock.Object, new SynchronousAsyncWorker());
            _targetRequestServiceNameViewModel.PropertyChanged += (sender, args) => { _changedPropertiesRequestServiceNameViewModel.Add(args.PropertyName); };
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
        public void TestChooseFileSystemDLLCommandCanExecuteDllChooserIsNotNull()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            var ddlChooserMock = new Mock<IDLLChooser>();

            var mockChooseDLLView = new Mock<IChooseDLLView>();
            CustomContainer.RegisterInstancePerRequestType<IChooseDLLView>(() => mockChooseDLLView.Object);
            mockChooseDLLView.Setup(s => s.ShowView(ddlChooserMock.Object));
            mockChooseDLLView.Setup(view => view.RequestClose()).Verifiable();

            //act
            _target.ChooseFileSystemDLLCommand.Execute(null);

            //assert
            mockChooseDLLView.Verify(view => view.ShowView(It.IsAny<IDLLChooser>()));
        }

        [TestMethod]
        public void TestChooseGACDLLCommandCanExecuteDllChooserIsNotNull()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            var ddlChooserMock = new Mock<IDLLChooser>();

            var mockChooseDLLView = new Mock<IChooseDLLView>();
            CustomContainer.RegisterInstancePerRequestType<IChooseDLLView>(() => mockChooseDLLView.Object);
            mockChooseDLLView.Setup(s => s.ShowView(ddlChooserMock.Object));
            mockChooseDLLView.Setup(view => view.RequestClose()).Verifiable();

            //act
            _target.ChooseGACDLLCommand.Execute(null);

            //assert
            mockChooseDLLView.Verify(view => view.ShowView(It.IsAny<IDLLChooser>()));
        }

        [TestMethod]
        public void TestChooseConfigFileCommandCanExecuteDllChooserIsNotNull()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;

            var mockFileChooserView = new Mock<IFileChooserView>();
            mockFileChooserView.Setup(view => view.DataContext).Returns(new Mock<FileChooser>());
            CustomContainer.RegisterInstancePerRequestType<IFileChooserView>(() => mockFileChooserView.Object);
            mockFileChooserView.Setup(s => s.ShowView(false));
            mockFileChooserView.Setup(view => view.RequestClose()).Verifiable();

            //act
            _target.ChooseConfigFileCommand.Execute(null);

            //assert
            mockFileChooserView.Verify(view => view.ShowView(false));
            Assert.AreEqual("", _target.ConfigFilePath);
        }

        [TestMethod]
        public void TestOkCommandCanExecuteAssemblyNameIsEmpty()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            _target.DllChooser.AssemblyName = "";

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
            _target.DllChooser.AssemblyName = "someAssemblyName";
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
            _target.FileSystemAssemblyName = someassemblynameDll;
            var itemMock = new Mock<IPluginSource>();
            _target.Item = itemMock.Object;

            //act
            var result = _target.OkCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
            File.Delete(someassemblynameDll);
        }

        [TestMethod]
        public void TestCanSelectConfigFilesTrue()
        {
            //arrange
            string someassemblynameDll = EnvironmentVariables.WorkspacePath + @"\someAssemblyName.dll";
            _target.FileSystemAssemblyName = someassemblynameDll;

            //assert
            Assert.IsTrue(_target.CanSelectConfigFiles);
        }

        [TestMethod]
        public void TestCanSelectConfigFilesFalse()
        {
            //arrange
            string someassemblynameDll = EnvironmentVariables.WorkspacePath + @"\someAssemblyName";
            _target.FileSystemAssemblyName = someassemblynameDll;

            //assert
            Assert.IsFalse(_target.CanSelectConfigFiles);
        }

        [TestMethod]
        public void TestOkCommandCanExecuteGac()
        {
            //arrange
            _target.GACAssemblyName = "GAC:someAssemblyName";
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
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Path);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Name);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.ResourceName);
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
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(ExpectedPath, _targetRequestServiceNameViewModel.Item.Path);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Name);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.ResourceName);
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
            _targetSource.SelectedDll = selectedDllMock.Object;
            _changedPropertiesSource.Clear();

            //act
            _targetSource.OkCommand.Execute(null);

            //assert
            _updateManagerMock.Verify(it => it.Save(It.IsAny<IPluginSource>()));
            Assert.IsTrue(_changedPropertiesSource.Contains("Header"));
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
        #endregion

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
            Assert.AreEqual(ExpectedPath, result.Path);
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
            _targetSource.DllChooser.DllListingModels = new List<IDllListingModel>() { dllListingMock.Object };

            //act
            _targetSource.FromModel(_pluginSourceMock.Object);

            //assert
            Assert.AreEqual(ExpectedName, _targetSource.Name);
            Assert.AreEqual(ExpectedPath, _targetSource.Path);
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
            _targetSource.DllChooser.DllListingModels = new List<IDllListingModel>() { dllListingMock.Object };

            //act
            _targetSource.FromModel(_pluginSourceMock.Object);

            //assert
            Assert.AreEqual(ExpectedName, _targetSource.Name);
            Assert.AreEqual(ExpectedPath, _targetSource.Path);
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
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Path);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Name);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.ResourceName);
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
            Assert.AreNotEqual(Guid.NewGuid(), _targetRequestServiceNameViewModel.Item.Id);
            Assert.AreEqual(ExpectedPath, _targetRequestServiceNameViewModel.Item.Path);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.Item.Name);
            Assert.AreEqual(ExpectedName, _targetRequestServiceNameViewModel.ResourceName);
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
            _targetSource.SelectedDll = selectedDllMock.Object;
            _changedPropertiesSource.Clear();

            //act
            _targetSource.Save();

            //assert
            _updateManagerMock.Verify(it => it.Save(It.IsAny<IPluginSource>()));
            Assert.IsTrue(_changedPropertiesSource.Contains("Header"));
        }

        [TestMethod]
        public void TestCanSaveAssemblyNameIsEmpty()
        {
            //arrange
            var selectedDllMock = new Mock<IDllListingModel>();
            _target.SelectedDll = selectedDllMock.Object;
            _target.DllChooser.AssemblyName = "";

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
            _target.DllChooser.AssemblyName = "someAssemblyName";
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
            _target.FileSystemAssemblyName = someassemblynameDll;
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
            _target.GACAssemblyName = "GAC:someAssemblyName";
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
        #endregion

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
            var value = _targetSource.RequestServiceNameViewModel;

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
        public void TestResourceNameLocalhost()
        {
            //arrange
            _warewolfServerName = "localhost";
            _updateManagerMock.SetupGet(it => it.ServerName).Returns(_warewolfServerName);
            _changedProperties = new List<string>();
            _target = new ManagePluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _asyncWorkerMock.Object);
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
            _changedPropertiesSource.Clear();

            //act
            _targetSource.ResourceName = ExpectedValue;
            var value = _targetSource.ResourceName;

            //asert
            Assert.AreEqual(ExpectedValue, value);
            Assert.IsTrue(_changedPropertiesSource.Contains(ExpectedValue));
            Assert.AreEqual(ExpectedHeader, _targetSource.Header);
            Assert.AreEqual(ExpectedHeaderText, _targetSource.HeaderText);
        }

        [TestMethod]
        public void TestResourceNameLocalhostSource()
        {
            //arrange
            _warewolfServerName = "localhost";
            _updateManagerMock.SetupGet(it => it.ServerName).Returns(_warewolfServerName);
            _changedPropertiesSource = new List<string>();
            _targetSource = new ManagePluginSourceViewModel(_updateManagerMock.Object, _aggregatorMock.Object, _pluginSourceMock.Object, _asyncWorkerMock.Object);
            _targetSource.PropertyChanged += (sender, args) =>
            {
                _changedPropertiesSource.Add(args.PropertyName);
            };
            const string ExpectedValue = "someResourceName";
            const string PluginSourceName = "pluginSourceName";
            _pluginSourceMock.SetupGet(it => it.Name).Returns(PluginSourceName);
            const string ExpectedHeader = PluginSourceName + " *";
            const string ExpectedHeaderText = PluginSourceName;
            _changedPropertiesSource.Clear();

            //act
            _targetSource.ResourceName = ExpectedValue;
            var value = _targetSource.ResourceName;

            //asert
            Assert.AreEqual(ExpectedValue, value);
            Assert.IsTrue(_changedPropertiesSource.Contains(ExpectedValue));
            Assert.AreEqual(ExpectedHeader, _targetSource.Header);
            Assert.AreEqual(ExpectedHeaderText, _targetSource.HeaderText);
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
            _target.DllChooser.AssemblyName = ExpectedValue;
            var value = _target.DllChooser.AssemblyName;

            //asert
            Assert.AreSame(ExpectedValue, value);
            Assert.AreSame(selectedDllMock.Object, _target.SelectedDll);
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
            _target.DllChooser.SelectedDll = expectedValueMock.Object;
            var value = _target.DllChooser.SelectedDll;
            //asert
            Assert.AreSame(expectedValueMock.Object, value);
            Assert.AreEqual(expectedAssemblyName, _target.DllChooser.AssemblyName);
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
        #endregion
    }
}