using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
// ReSharper disable InconsistentNaming

namespace Warewolf.AcceptanceTesting.PluginService
{
    [Binding]
    public class PluginServiceSteps
    {
        private static PluginSourceDefinition _testPluginSourceDefinition;
        private static PluginSourceDefinition _demoPluginSourceDefinition;
        static PluginAction _pluginAction;
        static PluginAction _dbInsertDummyAction;

        [BeforeFeature("PluginService")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var view = new ManagePluginServiceControl();
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var mockPluginServiceModel = new Mock<IPluginServiceModel>();
            SetupModel(mockPluginServiceModel);
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.RunSynchronously();
            var viewModel = new ManagePluginServiceViewModel(mockPluginServiceModel.Object, task);
            view.DataContext = viewModel;

            Utils.ShowTheViewForTesting(view);
            FeatureContext.Current.Add(Utils.ViewNameKey, view);
            FeatureContext.Current.Add("viewModel", viewModel);
            FeatureContext.Current.Add("model", mockPluginServiceModel);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);

        }

        [BeforeScenario]
        public static void Cleanup()
        {
            try
            {
                var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
                mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
                var mockPluginServiceModel = new Mock<IPluginServiceModel>();
                SetupModel(mockPluginServiceModel);
                var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
                task.Start();
                var viewModel = (ManagePluginServiceViewModel)FeatureContext.Current["viewModel"];
                viewModel.Model = mockPluginServiceModel.Object;
                var p = new PrivateObject(viewModel);
                p.SetField("_saveDialog", task);
                //viewModel.SaveDialog = task;
                viewModel.SelectedNamespace = null;
                viewModel.SelectedAction = null;
                viewModel.SelectedSource = null;
                viewModel.CanSelectMethod = false;
                viewModel.CanEditNamespace = false;
                viewModel.CanEditSource = false;
                //FeatureContext.Current.Add(Utils.ViewNameKey, view);
                FeatureContext.Current["viewModel"] = viewModel;
                FeatureContext.Current["model"] = mockPluginServiceModel;
                FeatureContext.Current["requestServiceNameViewModel"] = mockRequestServiceNameViewModel;
            }
            catch (Exception)
            {
                //
            }
        }

        private static void SetupModel(Mock<IPluginServiceModel> mockPluginServiceModel, bool errortest = false)
        {
            _demoPluginSourceDefinition = new PluginSourceDefinition
            {
                Name = "testingPluginSrc"
            };
            _testPluginSourceDefinition = new PluginSourceDefinition
            {
                Name = "IntegrationTestPluginNull"
            };
            mockPluginServiceModel.Setup(model => model.RetrieveSources()).Returns(new ObservableCollection<IPluginSource>
            {
                _demoPluginSourceDefinition,
                _testPluginSourceDefinition
            });
            mockPluginServiceModel.Setup(model => model.GetNameSpaces(It.IsAny<IPluginSource>())).Returns(new List<INamespaceItem>
            {
                new NamespaceItem(){FullName = "Unlimited Framework Plugins EmailPlugin"},
                new NamespaceItem(){FullName = "Dev2.PrimitiveTestDLL.TestClass"}
            });
            var nameValue = new NameValue
            {
                Name = "data (System.Object)",
                Value = ""
            };
            _pluginAction = new PluginAction
            {
                FullName = "FetchStringvalue",
                Variables = new List<INameValue>
                {
                    nameValue
                },
                Inputs = new List<IServiceInput> { new ServiceInput("data", "") }
            };
            var pluginInputs = new List<IServiceInput>
            {
                new ServiceInput("data","")
            };
            _dbInsertDummyAction = new PluginAction
            {
                FullName = "FetchStringvalue",
                Inputs = pluginInputs
            };
            mockPluginServiceModel.Setup(model => model.GetActions(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginAction>
            {
                _pluginAction,
                _dbInsertDummyAction
            });
            var recordset = new Recordset
            {
                Name = ""
            };
            var countryID = new RecordsetField
            {
                Name = "Name",
                Alias = "Name"
            };
            var recordSetRec = new RecordsetRecord
            {
                Name = "Name",
                Label = "Name"
            };

            recordset.Fields.Add(countryID);
            recordset.Records.Add(recordSetRec);

            var serializer = new Dev2JsonSerializer();
            var lst = new RecordsetList();
            if (errortest)
                mockPluginServiceModel.Setup(model => model.TestService(It.IsAny<IPluginService>()))
                .Throws(new Exception());
            else
            {
                mockPluginServiceModel.Setup(model => model.TestService(It.IsAny<IPluginService>()))
                .Returns(serializer.Serialize(lst));
            }
            try
            {
                Utils.GetViewModel<ManagePluginServiceViewModel>().Model = mockPluginServiceModel.Object;
            }
            catch
            {
                // ignored
            }
        }

        [BeforeScenario("PluginService")]
        public void SetupForDatabaseService()
        {
            ScenarioContext.Current.Add("view", FeatureContext.Current.Get<ManagePluginServiceControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("viewModel", FeatureContext.Current.Get<ManagePluginServiceViewModel>("viewModel"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add("model", FeatureContext.Current.Get<Mock<IPluginServiceModel>>("model"));
        }

        [Given(@"I open ""(.*)""")]
        public void GivenIOpen(string serviceName)
        {
            PluginServiceDefinition pluginService;
            switch(serviceName)
            {
                case"testingPluginSrc":
                    pluginService = new PluginServiceDefinition { Name = serviceName, Source = _demoPluginSourceDefinition, Action = _pluginAction, Inputs = new List<IServiceInput>() };
                    break;
                case "IntegrationTestPluginNull":
                    pluginService = new PluginServiceDefinition { Name = serviceName, Source = _testPluginSourceDefinition, Action = _pluginAction, Inputs = new List<IServiceInput>() };
                    break;
                default:
                    pluginService = new PluginServiceDefinition { Name = serviceName, Source = _demoPluginSourceDefinition, Action = _pluginAction, Inputs = new List<IServiceInput>() };
                    break;
            }

            var dbOutputMapping = new ServiceOutputMapping("Name", "Name","");
            pluginService.OutputMappings = new List<IServiceOutputMapping> { dbOutputMapping };
            ScenarioContext.Current.Remove("viewModel");
            var model = ScenarioContext.Current.Get<Mock<IPluginServiceModel>>("model");
            model.Setup(a => a.GetActions(It.IsAny<IPluginSource>(), It.IsAny<INamespaceItem>())).Returns(new List<IPluginAction> { new PluginAction() { FullName = "bob" } });
            model.Setup(a => a.GetNameSpaces(It.IsAny<IPluginSource>())).Returns(new List<INamespaceItem> { new NamespaceItem() { FullName = "blob" } });
            var viewModel = (ManagePluginServiceViewModel)FeatureContext.Current["viewModel"];
            viewModel.Item = pluginService;
            viewModel.PluginService = pluginService;
            viewModel.SelectedNamespace = new NamespaceItem() { FullName = "bob" };
            viewModel.SelectedAction = _pluginAction;

            ScenarioContext.Current.Add("viewModel", viewModel);
        }

        [Given(@"I open New Plugin Service Connector")]
        public void GivenIOpenNewPluginServiceConnector()
        {
            var sourceControl = ScenarioContext.Current.Get<ManagePluginServiceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(sourceControl);
            Assert.IsNotNull(sourceControl.DataContext);
        }

        [Given(@"""(.*)"" tab is opened")]
        public void GivenTabIsOpened(string headerText)
        {
            var viewModel = Utils.GetViewModel<ManagePluginServiceViewModel>();
            Assert.AreEqual(headerText, viewModel.Header);
        }

        [Given(@"Select a source is focused")]
        public void GivenSelectASourceIsFocused()
        {
        }

        [Given(@"all other steps are ""(.*)""")]
        public void GivenAllOtherStepsAre(string p0)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            const string State = "Disabled";
            Utils.CheckControlEnabled("2 Select a Namespace", State, view);
            Utils.CheckControlEnabled("3 Select an Action", State, view);
            Utils.CheckControlEnabled("4 Provide Test Values", State, view);
            Utils.CheckControlEnabled("5 Defaults and Mapping", State, view);
        }

        [When(@"Test Connection is ""(.*)""")]
        public void WhenTestConnectionIs(string p0)
        {
            var pluginServiceModel = ScenarioContext.Current.Get<Mock<IPluginServiceModel>>("model");
            SetupModel(pluginServiceModel);
            var view = Utils.GetView<ManagePluginServiceControl>();
            view.TestAction();
            var viewModel = Utils.GetViewModel<ManagePluginServiceViewModel>();
            Assert.IsFalse(viewModel.IsTesting);
            Assert.AreEqual(String.Empty, viewModel.ErrorText);
        }

        [Then(@"the Test Connection is ""(.*)""")]
        [When(@"the Test Connection is ""(.*)""")]
        public void ThenTheTestConnectionIs(string p0)
        {
            //SetupModel(pluginServiceModel);
            var view = Utils.GetView<ManagePluginServiceControl>();
            view.TestAction();
            var viewModel = Utils.GetViewModel<ManagePluginServiceViewModel>();
            viewModel.TestPluginCommand.Execute(null);
            Assert.AreNotSame(string.Empty, viewModel.ErrorText);
        }

        [When(@"""(.*)"" is clicked and expeced to be unsuccessful")]
        public void WhenIsClickedAndExpecedToBeUnsuccessful(string p0)
        {
            var pluginServiceModel = ScenarioContext.Current.Get<Mock<IPluginServiceModel>>("model");
            SetupModel(pluginServiceModel,true);
            var view = Utils.GetView<ManagePluginServiceControl>();
            view.TestAction();
            var viewModel = Utils.GetViewModel<ManagePluginServiceViewModel>();
            Assert.AreNotSame(string.Empty, viewModel.ErrorText);
        }

        [When(@"""(.*)"" is clicked")]
        public void WhenIsClicked(string name)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();

            switch (name)
            {
                case "Save":
                    view.Save();
                    break;
                case "Test":
                    view.TestAction();
                    break;
                case "Refresh":
                    view.RefreshAction();
                    break;
            }
        }

        [Then(@"the Save Dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify(a => a.ShowSaveDialog());
        }

        [Then(@"""(.*)"" is not an Action")]
        public void ThenIsNotAnAction(string p0)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            var names =view.GetActionNames();
            Assert.IsFalse(names.Contains(p0));
        }

        [Then(@"(.*) Select an Action VM is ""(.*)""")]
        public void ThenSelectAnActionVMIs(int p0, string state)
        {
            var vm = Utils.GetViewModel<ManagePluginServiceViewModel>();
            Assert.IsTrue(vm.CanSelectMethod==(state=="Enabled"));
        }

        [Then(@"Edit Default and Mapping Names VM is ""(.*)""")]
        public void ThenEditDefaultAndMappingNamesVMIs(string state)
        {
            var vm = Utils.GetViewModel<ManagePluginServiceViewModel>();
            Assert.IsTrue(vm.CanEditMappings == (state == "Enabled"));
        }

        [Then(@"Provide Test Values VM is ""(.*)""")]
        public void ThenProvideTestValuesVMIs(string state)
        {
            var vm = Utils.GetViewModel<ManagePluginServiceViewModel>();
            Assert.IsTrue(vm.CanTest == (state == "Enabled"));
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        [Given(@"""(.*)"" is ""(.*)""")]
        public void GivenIs(string name, string state)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            switch (name)
            {
                case "1 Select a Source":
                    Utils.CheckControlEnabled(name, state, view);
                    break;
                case "2 Select a Namespace":
                    Utils.CheckControlEnabled(name, state, view);
                    break;
                case "3 Select an Action":
                    Utils.CheckControlEnabled(name, state, view);
                    break;
                case "4 Provide Test Values":
                    Utils.CheckControlEnabled(name, state, view);
                    break;
                case "5 Defaults and Mapping":
                    Utils.CheckControlEnabled(name, state, view);
                    break;
            }
        }

        [Given(@"the ""(.*)"" button is clicked")]
        public void GivenTheButtonIsClicked(string p0)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            view.NewAction();
        }

        [Then(@"the ""(.*)"" tab is opened")]
        public void ThenTheTabIsOpened(string headerText)
        {
            var viewModel = Utils.GetViewModel<ManagePluginServiceViewModel>();
            Assert.AreEqual(headerText, viewModel.Header);
        }

        [Then(@"""(.*)"" isopened in another tab")]
        public void ThenIsopenedInAnotherTab(string p0)
        {
            var pluginServiceModel = ScenarioContext.Current.Get<Mock<IPluginServiceModel>>("model");
            pluginServiceModel.Verify(a => a.CreateNewSource());
        }

        [Given(@"""(.*)"" is selected as source")]
        public void GivenIsSelectedAsSource(string selectedSourceName)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            var selectedDataSource = view.GetSelectedPluginSource();
            Assert.AreEqual(selectedSourceName, selectedDataSource.Name);
        }

        [Then(@"""(.*)"" is selected as action")]
        public void ThenIsSelectedAsAction(string selectedActionName)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            var selectedAction = view.GetSelectedActionSource();
            Assert.AreEqual(selectedActionName, selectedAction.FullName);
        }

        [When(@"I save as ""(.*)""")]
        public void WhenISaveAs(string resourceName)
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ResourceName).Returns(new ResourceName("", resourceName));
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            var managePluginServiceControl = ScenarioContext.Current.Get<ManagePluginServiceControl>(Utils.ViewNameKey);
            managePluginServiceControl.Save();
        }

        [Then(@"title is ""(.*)""")]
        public void ThenTitleIs(string title)
        {
            var viewModel = ScenarioContext.Current.Get<ManagePluginServiceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.Header);
        }

        [Given(@"input mappings are")]
        [Then(@"input mappings are")]
        public void GivenInputMappingsAre(Table table)
        {
            var vm  = Utils.GetViewModel<ManagePluginServiceViewModel>();
            var inputMappings = vm.Inputs;
            var i = 0;
            if (inputMappings == null)
            {
                Assert.AreEqual(0, table.RowCount);
            }
            else
            {
                foreach (var input in inputMappings)
                {
                    var inputMapping = input;
                    if (inputMapping != null)
                    {
                        Assert.AreEqual(inputMapping.Name, table.Rows.ToList()[i][0]);
                    }
                    i++;
                }
            }
        }

        [Then(@"output mappings are")]
        public void ThenOutputMappingsAre(Table table)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            var outputMappings = ((ManagePluginServiceViewModel)view.DataContext).OutputMapping;

            if (outputMappings == null)
            {
                Assert.AreEqual(0, table.RowCount);
            }
            else
            {
                foreach (var output in outputMappings)
                {
                    var outputMapping = output;
                    if (outputMapping != null)
                    {
                    }
                }
            }
        }

        [When(@"I select ""(.*)"" as source")]
        public void WhenISelectAsSource(string pluginName)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            var pluginSourceDefinition = new PluginSourceDefinition
            {
                Name = pluginName
            };
            view.SelectPluginSource(pluginSourceDefinition);
            var viewModel = Utils.GetViewModel<ManagePluginServiceViewModel>();
            viewModel.SelectedSource = pluginSourceDefinition;
            Assert.AreEqual(pluginName, viewModel.SelectedSource.Name);
        }

        [When(@"I select ""(.*)"" as namespace")]
        public void WhenISelectAsNamespace(string nameSpace)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            var nameValue = new NamespaceItem
            {
                FullName = nameSpace
            };
            view.SelectNamespace(nameValue);
            var viewModel = Utils.GetViewModel<ManagePluginServiceViewModel>();
            try
            {
                viewModel.SelectedNamespace = new NamespaceItem() { FullName = nameSpace };
            }
            catch(Exception)
            {
                // view sometimes in  invalid state onpropertychange throws exception inside the command
            }
            Assert.AreEqual(nameSpace, viewModel.SelectedNamespace.FullName);
        }

        [When(@"I select ""(.*)"" as action")]
        public void WhenISelectAsAction(string actionName)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            var pluginAction = new PluginAction
            {
                FullName = actionName,
                Inputs = new List<IServiceInput>() { new ServiceInput("data", "data") }
            };
            view.SelectAction(pluginAction);
            var viewModel = Utils.GetViewModel<ManagePluginServiceViewModel>();
            try
            {
                viewModel.SelectedAction = pluginAction;
            }
            catch(Exception)
            {
               // vm not updating
               // throw;
            }
            Assert.AreEqual(actionName, viewModel.SelectedAction.FullName);
        }

        [Given(@"I change the source to ""(.*)""")]
        public void GivenIChangeTheSourceTo(string pluginName)
        {
            var view = Utils.GetView<ManagePluginServiceControl>();
            var pluginSourceDefinition = new PluginSourceDefinition
            {
                Name = pluginName
            };
            view.SelectPluginSource(pluginSourceDefinition);
           
            var viewModel = Utils.GetViewModel<ManagePluginServiceViewModel>();
            viewModel.SelectedSource = pluginSourceDefinition;
            Assert.AreEqual(pluginName, viewModel.SelectedSource.Name);
        }
    }
}
