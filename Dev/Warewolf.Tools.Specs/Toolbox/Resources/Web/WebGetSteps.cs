using System;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces.WebServices;
using System.Linq;
using Dev2.Communication;
using System.Linq.Expressions;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Common.Interfaces;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Activities;
using TechTalk.SpecFlow;
using Dev2.Activities.Designers2.Web_Service_Get;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common;

namespace Warewolf.Tools.Specs.Toolbox.Resources.Web
{
    [Binding]
    public class WebGetSteps
    {
        readonly ScenarioContext _scenarioContext;

        public WebGetSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }
            this._scenarioContext = scenarioContext;
            CustomContainer.Register<IFieldAndPropertyMapper>(new FieldAndPropertyMapper());
        }
        WebServiceSourceDefinition _weblocalhostsource;
        WebServiceSourceDefinition _otherwebsource;


        [Given(@"I drag Web Get Request Connector Tool onto the design surface")]
        [When(@"I drag Web Get Request Connector Tool onto the design surface")]
        [Then(@"I drag Web Get Request Connector Tool onto the design surface")]
        public void GivenIDragWebGetRequestConnectorToolOntoTheDesignSurface()
        {
            var activity = new DsfWebGetActivityWithBase64();
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var mockServiceInputViewModel = new Mock<IManageWebServiceInputViewModel>();
            var mockServiceModel = new Mock<IWebServiceModel>();
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            _otherwebsource = new WebServiceSourceDefinition
            {
                Name = "OtherWebSource",
                HostName = @"http://www.google.com",
                Id = Guid.NewGuid()
            };

            
            _weblocalhostsource = new WebServiceSourceDefinition
            {
                Name = "LocalhostSource",
                HostName = @"http://TFSBLD.premier.local:9810/api/products/Get",
                Id = Guid.NewGuid()
            };

            var webService = new WebService
            {
                RequestResponse = "[{\"Category\":\"Electronic\",\"Id\":\"1\",\"Name\":\"Television\",\"Price\":\"82000\"},{\"Category\":\"Electronic\",\"Id\":\"2\",\"Name\":\"Refrigerator\",\"Price\":\"23000\"},{\"Category\":\"Electronic\",\"Id\":\"3\",\"Name\":\"Mobiles\",\"Price\":\"20000\"},{\"Category\":\"Electronic\",\"Id\":\"4\",\"Name\":\"Laptops\",\"Price\":\"45000\"},{\"Category\":\"Electronic\",\"Id\":\"5\",\"Name\":\"iPads\",\"Price\":\"67000\"},{\"Category\":\"Gift Items\",\"Id\":\"6\",\"Name\":\"Toys\",\"Price\":\"15000\"}]",
                Recordsets = new RecordsetList
                {
                    new Dev2.Runtime.ServiceModel.Data.Recordset
                    {
                        Name = "UnnamedArrayData",
                        Fields = new List<RecordsetField>
                        {
                            new RecordsetField
                            {
                                Alias = "Id",
                                Name = "Id",
                                RecordsetAlias = ""
                            },
                            new RecordsetField
                            {
                                Alias = "Name",
                                Name = "Name",
                                RecordsetAlias = ""
                            },
                            new RecordsetField
                            {
                                Alias = "Category",
                                Name = "Category",
                                RecordsetAlias = ""
                            },
                            new RecordsetField
                            {
                                Alias = "Price",
                                Name = "Price",
                                RecordsetAlias = ""
                            }
                        }
                    }
                }
            };
            var serializer = new Dev2JsonSerializer();
            var testResult = serializer.Serialize(webService);

            var sources = new ObservableCollection<IWebServiceSource> { _weblocalhostsource, _otherwebsource};
            mockServiceModel.Setup(model => model.RetrieveSources()).Returns(sources);
            mockServiceModel.Setup(model => model.Sources).Returns(sources);
            mockServiceModel.Setup(model => model.EditSource(It.IsAny<IWebServiceSource>())).Verifiable();
            mockServiceInputViewModel.SetupAllProperties();
            mockServiceModel.Setup(model => model.TestService(It.IsAny<IWebService>())).Returns(testResult);
            var viewModel = new WebServiceGetViewModel(modelItem, mockServiceModel.Object);

            _scenarioContext.Add("viewModel", viewModel);
            _scenarioContext.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            _scenarioContext.Add("mockServiceModel", mockServiceModel);
        }

        [Given(@"New Source is Enabled")]
        [When(@"New Source is Enabled")]
        [Then(@"New Source is Enabled")]
        public void GivenNewSourceIsEnabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"Edit Source is Disabled")]
        [When(@"Edit Source is Disabled")]
        [Then(@"Edit Source is Disabled")]
        public void GivenEditSourceIsDisabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecuteNewCommand);
        }

        [When(@"Get Header appears as")]
        [Then(@"Get Header appears as")]
        public void ThenGetHeaderAppearsAs(Table table)
        {
            var headers = GetViewModel().InputArea.Headers;
            foreach (var tableRow in table.Rows)
            {
                var name = tableRow["Header"];
                var value = tableRow["Value"];
                var found = headers.FirstOrDefault(nameValue => nameValue.Name == name && nameValue.Value == value);
                Assert.IsNotNull(found);
            }
        }

        [Then(@"Get Header is Enabled")]
        [When(@"Get Header is Enabled")]
        public void ThenGetHeaderIsEnabled()
        {
            Assert.AreEqual<int>(1, GetViewModel().InputArea.Headers.Count);
        }

        [When(@"Get Body is Enabled")]
        [Then(@"Get Body is Enabled")]
        public void ThenGetBodyIsEnabled()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
        }

        [When(@"Get Url is Visible")]
        [Then(@"Get Url is Visible")]
        public void ThenGetUrlIsVisible()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
        }
        [When(@"Get Query is Enabled")]
        [Then(@"Get Query is Enabled")]
        public void ThenGetQueryIsEnabled()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
        }
        [When(@"Get Generate Outputs is Enabled")]
        [Then(@"Get Generate Outputs is Enabled")]
        public void ThenGetGenerateOutputsIsEnabled()
        {
            var canGenerateOutputs = GetViewModel().TestInputCommand.CanExecute();
            Assert.IsTrue(canGenerateOutputs);
        }
        [When(@"Get mapped outputs are")]
        [Then(@"Get mapped outputs are")]
        public void ThenGetMappedOutputsAre(Table table)
        {
            var vm = GetViewModel();
            if (table.Rows.Count == 0)
            {
                if (vm.OutputsRegion.Outputs != null)
                {
                    Assert.AreEqual<int>(vm.OutputsRegion.Outputs.Count, 0);
                }
            }
            else
            {
                var matched = table.Rows.Zip(vm.OutputsRegion.Outputs, (a, b) => new Tuple<TableRow, IServiceOutputMapping>(a, b));
                foreach (var a in matched)
                {
                    Assert.AreEqual<string>(a.Item1[0], a.Item2.MappedFrom);
                    Assert.AreEqual<string>(a.Item1[1], a.Item2.MappedTo);

                }
            }
        }

        [When(@"Get Edit is Enabled")]
        [Then(@"Get Edit is Enabled")]
        public void ThenGetEditIsEnabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"I Select ""(.*)"" as a Get web Source")]
        [When(@"I Select ""(.*)"" as a Get web Source")]
        [Then(@"I Select ""(.*)"" as a Get web Source")]
        public void WhenISelectAsAGetWebSource(string sourceName)
        {
            var vm = GetViewModel();
            var sourceToSelect = GetServiceModel().Object.Sources.FirstOrDefault(s => s.Name.ToLowerInvariant() == sourceName.ToLowerInvariant());
            Assert.IsNotNull(sourceToSelect);
            vm.SourceRegion.SelectedSource = sourceToSelect;

            Assert.AreEqual(sourceToSelect.HostName, vm.InputArea.RequestUrl);
        }

        [Given(@"the ""(.*)"" Get Source tab is opened")]
        [When(@"the ""(.*)"" Get Source tab is opened")]
        [Then(@"the ""(.*)"" Get Source tab is opened")]
        public void ThenTheGetSourceTabIsOpened(string p0)
        {
            GetServiceModel().Verify(model => model.EditSource(It.IsAny<IWebServiceSource>()));
        }
        [Given(@"I click Get Edit")]
        [When(@"I click Get Edit")]
        [Then(@"I click Get Edit")]
        public void WhenIClickGetEdit()
        {
            GetViewModel().SourceRegion.EditSourceCommand.Execute(null);
        }

        WebServiceGetViewModel GetViewModel()
        {
            return _scenarioContext.Get<WebServiceGetViewModel>("viewModel");
        }
        Mock<IWebServiceModel> GetServiceModel()
        {
            return _scenarioContext.Get<Mock<IWebServiceModel>>("mockServiceModel");
        }

        [Given(@"I click Get Generate Outputs")]
        [When(@"I click Get Generate Outputs")]
        [Then(@"I click Get Generate Outputs")]
        public async Task ThenIClickGetGenerateOutputs()
        {
            var webServiceGetViewModel = GetViewModel();
            await webServiceGetViewModel.TestInputCommand.Execute();
        }
        [Given(@"Get the Generate Outputs window is shown")]
        [When(@"Get the Generate Outputs window is shown")]
        [Then(@"Get the Generate Outputs window is shown")]
        public void ThenGetTheGenerateOutputsWindowIsShown()
        {
            var webServiceGetViewModel = GetViewModel();
            Assert.IsTrue(webServiceGetViewModel.GenerateOutputsVisible);
            Assert.IsTrue(webServiceGetViewModel.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.IsFalse(webServiceGetViewModel.ManageServiceInputViewModel.OutputArea.IsEnabled);
        }

        [Given(@"Get Test Inputs is Successful")]
        [Then(@"Get Test Inputs is Successful")]
        [When(@"Get Test Inputs is Successful")]
        public void WhenGetTestInputsIsSuccessful()
        {
            GetViewModel().ManageServiceInputViewModel.TestCommand.Execute(null);
        }

        [Given(@"Get Response contains Data")]
        [When(@"Get Response contains Data")]
        [Then(@"Get Response contains Data")]
        public void ThenGetResponseContainsData()
        {
            var webServiceGetViewModel = GetViewModel();
            Assert.IsFalse(string.IsNullOrEmpty(webServiceGetViewModel.ManageServiceInputViewModel.TestResults), "Expected data after clicking Test, but got nothing.");
        }
        [Given(@"I click Get Done")]
        [When(@"I click Get Done")]
        [Then(@"I click Get Done")]
        public void WhenIClickGetDone()
        {
            var webServiceGetViewModel = GetViewModel();
            webServiceGetViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
        }

        [Given(@"Get Mapping is Enabled")]
        [When(@"Get Mapping is Enabled")]
        [Then(@"Get Mapping is Enabled")]
        public void ThenGetMappingIsEnabled()
        {
            var webServiceGetViewModel = GetViewModel();
            Assert.IsTrue(webServiceGetViewModel.OutputsRegion.IsEnabled);
        }

        [Given(@"I change Recordset Name to ""(.*)""")]
        [When(@"I change Recordset Name to ""(.*)""")]
        [Then(@"I change Recordset Name to ""(.*)""")]
        public void WhenIChangeRecordsetNameTo(string recordsetName)
        {
            var webServiceGetViewModel = GetViewModel();
            webServiceGetViewModel.OutputsRegion.RecordsetName = recordsetName;
            Assert.IsTrue(webServiceGetViewModel.OutputsRegion.IsEnabled);
        }

    }
}
