using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Oracle;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.Database;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.OracleServer
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TestOracleServerViewModel
    {
        public const string TestOwner = "";
        public const string Category = "Oracle activity designer";

        public OracleDatabaseDesignerViewModel GetViewModel()
        {
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            return new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
        }

        public OracleDatabaseDesignerViewModel GetViewModelwith()
        {
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            return new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
        }


        public OracleDatabaseDesignerViewModel GetViewModel_onePar()
        {

            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            return new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act));
        }


        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_Instantiate_New_ReturnsNewViewModel()
        {
            var model = GetViewModel();

            Assert.IsNotNull(model);
        }
        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_Instantiate_New_ReturnsNewViewModelWithAction()
        {
            var model = GetViewModelwith();

            Assert.IsNotNull(model);
        }
        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        [ExpectedException(typeof(NullReferenceException))]
        public void Oracle_Instantiate_New_ReturnsNewViewModelError()
        {
            var model = GetViewModel_onePar();

            Assert.IsNull(model);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_WorstError()
        {
            var model = GetViewModel();
            var errType = model.WorstError;
            var err = model.IsWorstErrorReadOnly;
            var fix = model.FixErrorsCommand;
            var labelwidth = model.LabelWidth;
            model.GenerateOutputsVisible = false;
            model.SetDisplayName(" -");
            var bdv = model.ButtonDisplayValue;
            Assert.IsNotNull(labelwidth);
            Assert.IsNotNull(errType);
            Assert.IsNotNull(fix);
            Assert.IsNotNull(bdv);
            Assert.IsTrue(err);
            Assert.IsFalse(model.OutputsRegion.IsEnabled);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_ValidResource_ReturnSuccess()
        {
            var model = GetViewModel();
            Assert.IsTrue(model.SourceRegion.IsEnabled);
            Assert.IsFalse(model.ActionRegion.IsEnabled);
            Assert.IsFalse(model.InputArea.IsEnabled);
            Assert.IsTrue(model.ErrorRegion.IsEnabled);
            model.ValidateTestComplete();
            Assert.IsTrue(model.OutputsRegion.IsEnabled);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_MethodName_ValidateExpectErrors()
        {
            //------------Setup for test--------------------------
            var model = GetViewModel();
            model.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(model.Errors.Count, 1);
            Assert.AreEqual(model.DesignValidationErrors.Count, 2);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_MethodName_ClearErrors()
        {
            //------------Setup for test--------------------------
            var model = GetViewModel();
            model.Validate();

            //------------Execute Test---------------------------
            model.ClearValidationMemoWithNoFoundError();
            //------------Assert Results-------------------------
            Assert.IsNotNull(model.Errors);
            Assert.AreEqual(model.DesignValidationErrors.Count, 1);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_TestActionSetSource()
        {
            //------------Setup for test--------------------------
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------

            var model = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            model.ManageServiceInputViewModel = new InputViewForTest(model, mod);
            model.SourceRegion.SelectedSource = model.SourceRegion.Sources.First();

            //------------Assert Results-------------------------
            Assert.IsTrue(model.SourceRegion.IsEnabled);
            Assert.IsFalse(model.OutputsRegion.IsEnabled);
            Assert.IsFalse(model.InputArea.IsEnabled);
            Assert.IsTrue(model.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //------------Setup for test--------------------------
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var model = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            model.ManageServiceInputViewModel = new InputViewForTest(model, mod);
            model.SourceRegion.SelectedSource = model.SourceRegion.Sources.First();
#pragma warning disable 4014
            model.TestInputCommand.Execute();
            model.ManageServiceInputViewModel.TestCommand.Execute(null);
            model.ManageServiceInputViewModel.IsEnabled = true;
            model.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            model.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(model.SourceRegion.IsEnabled);
            Assert.IsFalse(model.OutputsRegion.IsEnabled);
            Assert.IsFalse(model.InputArea.IsEnabled);
            Assert.IsTrue(model.ErrorRegion.IsEnabled);
            Assert.IsTrue(model.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.AreEqual(0, model.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //------------Setup for test--------------------------
            var mod = new OracleModel();
            mod.HasRecError = true;
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var model = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            model.ManageServiceInputViewModel = new InputViewForTest(model, mod);
            model.SourceRegion.SelectedSource = model.SourceRegion.Sources.First();
#pragma warning disable 4014
            model.TestInputCommand.Execute();
            model.ManageServiceInputViewModel.TestCommand.Execute(null);
            model.ManageServiceInputViewModel.IsEnabled = true;
            model.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            model.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(model.ErrorRegion.IsEnabled);
            Assert.AreEqual(1, model.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();

            //------------Execute Test---------------------------
            var model = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            model.ManageServiceInputViewModel = new InputViewForTest(model, mod);
            model.SourceRegion.SelectedSource = model.SourceRegion.Sources.First();
            model.ActionRegion.SelectedAction = model.ActionRegion.Actions.First();
            model.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014

            model.TestInputCommand.Execute();
            model.ManageServiceInputViewModel.TestCommand.Execute(null);

            model.ManageServiceInputViewModel.IsEnabled = true;
            model.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            model.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsFalse(model.SourceRegion.IsEnabled);
            Assert.IsFalse(model.OutputsRegion.IsEnabled);
            Assert.IsFalse(model.InputArea.IsEnabled);
            Assert.IsFalse(model.ErrorRegion.IsEnabled);
            Assert.AreEqual(2, model.ManageServiceInputViewModel.InputArea.Inputs.Count);
            Assert.IsTrue(model.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[fname]]");
            Assert.AreEqual(0, model.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void Oracle_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //------------Setup for test--------------------------
            //var mod = new OracleModel();
            var act = new DsfOracleDatabaseActivity();
            var dbServiceModel = new Mock<IDbServiceModel>();
            dbServiceModel.Setup(serviceModel => serviceModel.TestService(It.IsAny<IDatabaseService>())).Returns(new DataTable());
            dbServiceModel.Setup(serviceModel => serviceModel.RetrieveSources()).Returns(new ObservableCollection<IDbSource>() {new DbSourceDefinition()  });
            //dbServiceModel.Setup(serviceModel => serviceModel./*/*RetrieveSources*/*/()).Returns(new ObservableCollection<IDbSource>());
            //------------Execute Test---------------------------
            var model = new OracleDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), dbServiceModel.Object);
            model.ManageServiceInputViewModel = new InputViewForTest(model, dbServiceModel.Object);
            model.SourceRegion.SelectedSource = model.SourceRegion.Sources.First();
            model.ActionRegion.SelectedAction = model.ActionRegion.Actions.First();
            model.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            model.TestInputCommand.Execute();
#pragma warning restore 4014

            model.ManageServiceInputViewModel.TestCommand.Execute(null);
            model.ManageServiceInputViewModel.IsEnabled = true;
            model.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            model.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsFalse(model.SourceRegion.IsEnabled);
            Assert.IsFalse(model.OutputsRegion.IsEnabled);
            Assert.IsFalse(model.InputArea.IsEnabled);
            Assert.IsFalse(model.ErrorRegion.IsEnabled);
            Assert.IsTrue(model.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(model.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[fname]]");
            Assert.IsTrue(model.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
            Assert.AreEqual(0, model.ManageServiceInputViewModel.Errors.Count);
        }
    }

    public class OracleModel : IDbServiceModel
    {
#pragma warning disable 649
        private IStudioUpdateManager _updateRepository;
#pragma warning restore 649
#pragma warning disable 169
        private IQueryManager _queryProxy;
#pragma warning restore 169

        private readonly ObservableCollection<IDbSource> _sources = new ObservableCollection<IDbSource>
        {
            new DbSourceDefinition()
            {
                Name = "DemoOracle",
                Type = enSourceType.Oracle,
                ServerName = "Localhost",
                UserName = "oracle",
                Password = "sa",
                AuthenticationType = AuthenticationType.User
            }
        };

        private readonly ObservableCollection<IDbAction> _actions = new ObservableCollection<IDbAction>
        {
            new DbAction()
            {
                Name = "getemployees",
                Inputs = new List<IServiceInput>() { new ServiceInput("[[fname]]", "Bill") }
            }
        };

        public bool HasRecError { get; set; }

        #region Implementation of IDbServiceModel

        public ObservableCollection<IDbSource> RetrieveSources()
        {
            return Sources;
        }

        public ObservableCollection<IDbSource> Sources
        {
            get
            {
                return _sources;
            }
        }

        public ICollection<IDbAction> GetActions(IDbSource source)
        {
            return Actions;
        }

        public ICollection<IDbAction> Actions
        {
            get
            {
                return _actions;
            }
        }

        [ExcludeFromCodeCoverage]
        public void CreateNewSource()
        {
        }
        [ExcludeFromCodeCoverage]
        public void EditSource(IDbSource selectedSource)
        {
        }

        public DataTable TestService(IDatabaseService inputValues)
        {
            return _updateRepository.TestDbService(inputValues);
        }

        [ExcludeFromCodeCoverage]
        public IStudioUpdateManager UpdateRepository
        {
            get
            {
                return _updateRepository;
            }
        }

        #endregion
    }

    public class InputViewForTest : ManageDatabaseServiceInputViewModel
    {
        public InputViewForTest(IDatabaseServiceViewModel model, IDbServiceModel serviceModel)
            : base(model, serviceModel)
        {
        }
    }
}
