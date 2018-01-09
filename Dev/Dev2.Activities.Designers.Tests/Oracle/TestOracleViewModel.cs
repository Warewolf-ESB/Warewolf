using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Oracle;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Studio.ViewModels;

namespace Dev2.Activities.Designers.Tests.Oracle
{
    [TestClass]
    public class TestOracleViewModel
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_MethodName")]
        public void Oracle_MethodName_ValidateExpectErrors()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var oracle = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            oracle.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, oracle.Errors.Count);
            Assert.AreEqual(2, oracle.DesignValidationErrors.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_MethodName")]
        public void Oracle_MethodName_ClearErrors()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            var oracle = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            //------------Execute Test---------------------------
            oracle.ClearValidationMemoWithNoFoundError();
            //------------Assert Results-------------------------
            Assert.IsNull(oracle.Errors);
            Assert.AreEqual(oracle.DesignValidationErrors.Count, 1);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_MethodName")]
        public void Oracle_Ctor_EmptyModelItem()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var oracle = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder());

            //------------Assert Results-------------------------
            Assert.IsTrue(oracle.SourceRegion.IsEnabled);
            Assert.IsFalse(oracle.OutputsRegion.IsEnabled);
            Assert.IsFalse(oracle.InputArea.IsEnabled);
            Assert.IsTrue(oracle.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_MethodName")]
        public void Oracle_TestActionSetSource()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var oracle = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            oracle.ManageServiceInputViewModel = new InputViewForTest(oracle, mod);
            oracle.SourceRegion.SelectedSource = mod.Sources.First();

            //------------Assert Results-------------------------
            Assert.IsTrue(oracle.SourceRegion.IsEnabled);
            Assert.IsFalse(oracle.OutputsRegion.IsEnabled);
            Assert.IsFalse(oracle.InputArea.IsEnabled);
            Assert.IsTrue(oracle.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_MethodName")]
        public void Oracle_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var oracle = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            oracle.ManageServiceInputViewModel = new InputViewForTest(oracle, mod);
            oracle.SourceRegion.SelectedSource = mod.Sources.First();
#pragma warning disable 4014
            oracle.TestInputCommand.Execute(null);
            oracle.ManageServiceInputViewModel.TestCommand.Execute(null);
            oracle.ManageServiceInputViewModel.IsEnabled = true;
            oracle.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            oracle.ManageServiceInputViewModel.Model = new DatabaseService() { Action = new DbAction() { Inputs = new List<IServiceInput>(), Name = "bob" } };

            oracle.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(oracle.SourceRegion.IsEnabled);
            Assert.IsTrue(oracle.OutputsRegion.IsEnabled);
            Assert.IsTrue(oracle.InputArea.IsEnabled);
            Assert.IsTrue(oracle.ErrorRegion.IsEnabled);
            Assert.IsFalse(oracle.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.AreEqual(0, oracle.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_MethodName")]
        public void Oracle_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);
            var mod = new OracleModel();
            mod.HasRecError = true;
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var oracle = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            oracle.ManageServiceInputViewModel = new InputViewForTest(oracle, mod);
            oracle.SourceRegion.SelectedSource = mod.Sources.First();
#pragma warning disable 4014
            oracle.TestInputCommand.Execute(null);
            oracle.ManageServiceInputViewModel.TestCommand.Execute(null);
            oracle.ManageServiceInputViewModel.IsEnabled = true;
            oracle.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            oracle.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(oracle.ErrorRegion.IsEnabled);
            Assert.AreNotEqual(0, oracle.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_Handle")]
        public void Oracle_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------   
        

            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);

            var mod = new OracleModel();
            mod.HasRecError = true;
            var act = new DsfOracleDatabaseActivity();
            var oracle = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            
            //------------Execute Test---------------------------
            oracle.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_MethodName")]
        public void Oracle_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var oracle = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            oracle.ManageServiceInputViewModel = new InputViewForTest(oracle, mod);
            oracle.SourceRegion.SelectedSource = mod.Sources.First();
            oracle.ActionRegion.SelectedAction = mod.Actions.First();
            oracle.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            oracle.TestInputCommand.Execute(null);
            oracle.ManageServiceInputViewModel.TestCommand.Execute(null);
            oracle.ManageServiceInputViewModel.IsEnabled = true;
            oracle.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            oracle.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(oracle.SourceRegion.IsEnabled);
            Assert.IsTrue(oracle.OutputsRegion.IsEnabled);
            Assert.IsTrue(oracle.InputArea.IsEnabled);
            Assert.IsTrue(oracle.ErrorRegion.IsEnabled);
            Assert.AreEqual(1, oracle.ManageServiceInputViewModel.InputArea.Inputs.Count);
            Assert.IsTrue(oracle.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.AreEqual(0, oracle.ManageServiceInputViewModel.Errors.Count);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_MethodName")]
        public void Oracle_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var oracleDatabaseDesignerViewModel = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            oracleDatabaseDesignerViewModel.ManageServiceInputViewModel = new InputViewForTest(oracleDatabaseDesignerViewModel, mod);
            oracleDatabaseDesignerViewModel.SourceRegion.SelectedSource = mod.Sources.First();
            oracleDatabaseDesignerViewModel.ActionRegion.SelectedAction = mod.Actions.First();
            oracleDatabaseDesignerViewModel.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            oracleDatabaseDesignerViewModel.TestInputCommand.Execute(null);
            oracleDatabaseDesignerViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            oracleDatabaseDesignerViewModel.ManageServiceInputViewModel.IsEnabled = true;
            oracleDatabaseDesignerViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            oracleDatabaseDesignerViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(oracleDatabaseDesignerViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(oracleDatabaseDesignerViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(oracleDatabaseDesignerViewModel.InputArea.IsEnabled);
            Assert.IsTrue(oracleDatabaseDesignerViewModel.ErrorRegion.IsEnabled);
            Assert.IsTrue(oracleDatabaseDesignerViewModel.ManageServiceInputViewModel.InputArea.Inputs.Count == 1);
            Assert.IsTrue(oracleDatabaseDesignerViewModel.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.IsTrue(oracleDatabaseDesignerViewModel.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
            Assert.AreEqual(0, oracleDatabaseDesignerViewModel.ManageServiceInputViewModel.Errors.Count);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_Refresh")]
        public void Oracle_Refresh_ShouldLoadRefreshActions()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();
            var oracle = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act),mod, new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            oracle.ManageServiceInputViewModel = new InputViewForTest(oracle, mod);
            oracle.SourceRegion.SelectedSource = mod.Sources.First();
            oracle.ActionRegion.IsRefreshing = false;
            //------------Execute Test---------------------------
            oracle.ActionRegion.RefreshActionsCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(oracle.SourceRegion.IsEnabled);
            Assert.AreEqual(1, oracle.ActionRegion.Actions.Count);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Oracle_TestAction")]
        public void Oracle_TestActionSetSourceHasRecSet()
        {
            //------------Setup for test--------------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageDbServiceModel)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            var server = new Mock<IServer>();
            var updatemanager = new Mock<IStudioUpdateManager>();
            var queryManager = new Mock<IQueryManager>();
            queryManager.Setup(manager => manager.FetchDbSources()).Returns(new List<IDbSource>());
            server.Setup(server1 => server1.UpdateRepository).Returns(updatemanager.Object);
            server.Setup(server1 => server1.QueryProxy).Returns(queryManager.Object);
            mockMainViewModel.Setup(model => model.ActiveServer).Returns(server.Object);

            CustomContainer.Register(mockMainViewModel.Object);
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var mySql = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            mySql.ManageServiceInputViewModel = new InputViewForTest(mySql, mod);
            mySql.SourceRegion.SelectedSource = mod.Sources.First();
            mySql.ActionRegion.SelectedAction = mod.Actions.First();
            mySql.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            mySql.TestInputCommand.Execute(null);
            mySql.ManageServiceInputViewModel.TestCommand.Execute(null);
            mySql.ManageServiceInputViewModel.IsEnabled = true;
            mySql.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            mySql.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(mySql.SourceRegion.IsEnabled);
            Assert.IsTrue(mySql.OutputsRegion.IsEnabled);
            Assert.IsTrue(mySql.InputArea.IsEnabled);
            Assert.IsTrue(mySql.ErrorRegion.IsEnabled);
            Assert.IsTrue(mySql.ManageServiceInputViewModel.InputArea.Inputs.Count == 1);
            Assert.IsTrue(mySql.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.IsTrue(mySql.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
        }

    }
}