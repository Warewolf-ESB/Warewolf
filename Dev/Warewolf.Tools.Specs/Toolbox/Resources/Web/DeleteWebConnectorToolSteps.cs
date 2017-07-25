using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dev2.Activities.Designers2.Web_Service_Delete;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Resources.Web
{
    [Binding]
    public sealed class DeleteWebConnectorToolSteps
    {
        private readonly ScenarioContext scenarioContext;

        public DeleteWebConnectorToolSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        private WebServiceSourceDefinition _dev2CountriesWebServiceWebSource;
        private WebServiceSourceDefinition _webHelooWebSource;
        private WebServiceSourceDefinition _googleWebSource;

        [Given(@"I drag Web Delete Request Connector Tool onto the design surface")]
        public void GivenIDragWebDeleteRequestConnectorToolOntoTheDesignSurface()
        {
            var activity = new DsfWebDeleteActivity();
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

            _webHelooWebSource = new WebServiceSourceDefinition
            {
                Name = "WebHeloo"
            };

            _dev2CountriesWebServiceWebSource = new WebServiceSourceDefinition
            {
                Name = "Dev2CountriesWebService",
                HostName = "http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx"
            };

            _googleWebSource = new WebServiceSourceDefinition
            {
                Name = "Google Address Lookup"
            };
            var sources = new ObservableCollection<IWebServiceSource> { _dev2CountriesWebServiceWebSource };
            mockServiceModel.Setup(model => model.RetrieveSources()).Returns(sources);
            mockServiceModel.Setup(model => model.EditSource(It.IsAny<IWebServiceSource>())).Verifiable();
            mockServiceInputViewModel.SetupAllProperties();
            var viewModel = new WebServiceDeleteViewModel(modelItem, mockServiceModel.Object);


            scenarioContext.Add("viewModel", viewModel);
            scenarioContext.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            scenarioContext.Add("mockServiceModel", mockServiceModel);
        }

        WebServiceDeleteViewModel GetViewModel()
        {
            return scenarioContext.Get<WebServiceDeleteViewModel>("viewModel");
        }

        Mock<IWebServiceModel> GetServiceModel()
        {
            return scenarioContext.Get<Mock<IWebServiceModel>>("mockServiceModel");
        }

        [When(@"Delete Test Inputs is Successful")]
        public void WhenTestInputsIsSuccessful()
        {
            GetViewModel().ManageServiceInputViewModel.TestCommand.Execute(null);
        }

        [Given(@"Delete New is Enabled")]
        [When(@"Delete New is Enabled")]
        [Then(@"Delete New is Enabled")]
        public void WhenDeleteNewIsEnabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"Delete Edit is Enabled")]
        [When(@"Delete Edit is Enabled")]
        [Then(@"Delete Edit is Enabled")]
        public void WhenDeleteEditIsEnabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"Delete Edit is Disabled")]
        public void GivenEditIsDisabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecuteNewCommand);
        }

        [When(@"I click Delete Edit")]
        public void WhenDeleteIClickEdit()
        {
            GetViewModel().SourceRegion.EditSourceCommand.Execute(null);
        }

        [Then(@"Delete Header is Enabled")]
        public void ThenHeaderIsEnabled()
        {
            Assert.AreEqual<int>(1, GetViewModel().InputArea.Headers.Count);
        }

        [Then(@"Delete Header appears as")]
        public void ThenHeaderAppearsAs(Table table)
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

        [Then(@"Delete Body is Enabled")]
        public void ThenDeleteBodyIsEnabled()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
        }

        [Then(@"Delete Url is Visible")]
        public void ThenDeleteUrlIsVisible()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
        }

        [Then(@"Delete Query is Enabled")]
        public void ThenDeleteQueryIsEnabled()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
        }

        [Then(@"Delete Generate Outputs is Enabled")]
        public void ThenDeleteGenerateOutputsIsEnabled()
        {
            var canGenerateOutputs = GetViewModel().TestInputCommand.CanExecute();
            Assert.IsTrue(canGenerateOutputs);
        }

        [Then(@"the Delete Generate Outputs window is shown")]
        public void ThenTheDeleteGenerateOutputsWindowIsShown()
        {
            var webServiceDeleteViewModel = GetViewModel();
            Assert.IsTrue(webServiceDeleteViewModel.GenerateOutputsVisible);
            Assert.IsTrue(webServiceDeleteViewModel.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.IsFalse(webServiceDeleteViewModel.ManageServiceInputViewModel.OutputArea.IsEnabled);
        }

        [Then(@"Delete Variables are Enabled")]
        public void ThenDeleteVariablesAreEnabled()
        {
            var webServiceDeleteViewModel = GetViewModel();
            Assert.IsTrue(webServiceDeleteViewModel.ManageServiceInputViewModel.InputArea.IsEnabled);
        }

        [Then(@"the Delete response is loaded")]
        public void ThenTheDeleteResponseIsLoaded()
        {
            var webServiceDeleteViewModel = GetViewModel();
            Assert.IsTrue(webServiceDeleteViewModel.ManageServiceInputViewModel.OutputArea.IsEnabled);
        }
        
        [Then(@"Delete Mapping is Enabled")]
        public void ThenDeleteMappingIsEnabled()
        {
            var webServiceDeleteViewModel = GetViewModel();
            Assert.IsTrue(webServiceDeleteViewModel.OutputsRegion.IsEnabled);
        }

        [Then(@"I enter ""(.*)"" as Delete Query String")]
        public void ThenIEnterAsDeleteQueryString(string queryString)
        {
            var webServiceDeleteViewModel = GetViewModel();
            webServiceDeleteViewModel.InputArea.QueryString = queryString;
        }

        [Then(@"Delete Url as ""(.*)""")]
        public void ThenDeleteUrlAs(string url)
        {
            var webServiceDeleteViewModel = GetViewModel();
            Assert.AreEqual<string>(url, webServiceDeleteViewModel.InputArea.RequestUrl);
        }

        [Then(@"I add Delete Header as")]
        public void ThenIAddDeleteHeaderAs(Table table)
        {
            var headers = GetViewModel().InputArea.Headers;
            foreach (var tableRow in table.Rows)
            {
                var name = tableRow["Name"];
                var value = tableRow["Value"];
                headers.Add(new NameValue(name, value));
            }
        }

        [Then(@"Delete Input variables are")]
        public void ThenDeleteInputVariablesAre(Table table)
        {
            var serviceInputs = GetViewModel().ManageServiceInputViewModel.InputArea.Inputs;
            foreach (var tableRow in table.Rows)
            {
                var inputName = tableRow["Name"];
                var found = serviceInputs.FirstOrDefault(input => input.Name == inputName);
                Assert.IsNotNull(found);
            }
        }

        [Then(@"Delete Test is Enabled")]
        public void ThenDeleteTestIsEnabled()
        {
            var webServiceDeleteViewModel = GetViewModel();
            var canExecuteTest = webServiceDeleteViewModel.ManageServiceInputViewModel.TestCommand.CanExecute(null);
            Assert.IsTrue(canExecuteTest);
        }

        [Then(@"Delete Paste is Enabled")]
        public void ThenDeletePasteIsEnabled()
        {
            var webServiceDeleteViewModel = GetViewModel();
            var canPaste = webServiceDeleteViewModel.ManageServiceInputViewModel.PasteResponseCommand.CanExecute(null);
            Assert.IsTrue(canPaste);
        }

        [Then(@"the ""(.*)"" Delete Source tab is opened")]
        public void ThenTheDeleteSourceTabIsOpened(string p0)
        {
            GetServiceModel().Verify(model => model.EditSource(It.IsAny<IWebServiceSource>()));
        }

        [Given(@"I click Delete Generate Outputs")]
        [When(@"I click Delete Generate Outputs")]
        [Then(@"I click Delete Generate Outputs")]
        public async Task ThenIClickDeleteGenerateOutputs()
        {
            var webServiceDeleteViewModel = GetViewModel();
            await webServiceDeleteViewModel.TestInputCommand.Execute();
        }

        [Then(@"Delete Response appears as ""(.*)""")]
        public void ThenDeleteResponseAppearsAs(string response)
        {
            var webServiceDeleteViewModel = GetViewModel();
            Assert.AreEqual<string>(response, webServiceDeleteViewModel.ManageServiceInputViewModel.TestResults);
        }

        [Then(@"Delete Mappings is Disabled")]
        public void ThenDeleteMappingsIsDisabled()
        {
            var webServiceDeleteViewModel = GetViewModel();
            Assert.IsFalse(webServiceDeleteViewModel.OutputsRegion.IsEnabled);
        }

        [Given(@"I click Delete Done")]
        [When(@"I click Delete Done")]
        [Then(@"I click Delete Done")]
        public void ThenIClickDeleteDone()
        {
            var webServiceDeleteViewModel = GetViewModel();
            webServiceDeleteViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
        }

        [Then(@"Delete output mappings are")]
        public void ThenDeleteOutputMappingsAre(Table table)
        {
            var webServiceDeleteViewModel = GetViewModel();
            var outputs = webServiceDeleteViewModel.OutputsRegion.Outputs;
            foreach (var tableRow in table.Rows)
            {
                var mappedFrom = tableRow["Mapped From"];
                var mappedTo = tableRow["Mapped To"];
                var found = outputs.FirstOrDefault(mapping => mapping.MappedFrom == mappedFrom && mapping.MappedTo == mappedTo);
                Assert.IsNotNull(found);
            }
        }

        [When(@"I Select ""(.*)"" as a Delete web Source")]
        public void WhenISelectAsADeleteWebSource(string sourceName)
        {
            if (sourceName == "Dev2CountriesWebService")
            {
                var serviceModel = GetServiceModel();
                var webService = new WebService
                {
                    RequestResponse = "{\"CountryID\" : \"a\",\"Description\":\"a\"}",
                    Recordsets = new RecordsetList
                {
                    new Runtime.ServiceModel.Data.Recordset
                    {
                        Name = "",
                        Fields = new List<RecordsetField>
                        {
                            new RecordsetField
                            {
                                Alias = "CountryID",
                                Name = "CountryID",
                                RecordsetAlias = ""
                            },
                            new RecordsetField
                            {
                                Alias = "Description",
                                Name = "Description",
                                RecordsetAlias = ""
                            }
                        }
                    }
                }
                };
                var serializer = new Dev2JsonSerializer();
                var testResult = serializer.Serialize(webService);
                serviceModel.Setup(model => model.TestService(It.IsAny<IWebService>())).Returns(testResult);
                GetViewModel().SourceRegion.SelectedSource = _dev2CountriesWebServiceWebSource;
            }
            else if (sourceName == "Google Address Lookup")
            {
                GetViewModel().SourceRegion.SelectedSource = _googleWebSource;
            }
            else
            {
                var serviceModel = GetServiceModel();
                var webService = new WebService
                {
                    RequestResponse = "{\"rec\" : [{\"a\":\"1\",\"b\":\"a\"}]}",
                    Recordsets = new RecordsetList
                {
                    new Runtime.ServiceModel.Data.Recordset
                    {
                        Name = "rec",
                        Fields = new List<RecordsetField>
                        {
                            new RecordsetField
                            {
                                Alias = "a",
                                Name = "a",
                                RecordsetAlias = "rec"
                            },
                            new RecordsetField
                            {
                                Alias = "b",
                                Name = "b",
                                RecordsetAlias = "rec"
                            }
                        }
                    }
                }
                };
                var serializer = new Dev2JsonSerializer();
                var testResult = serializer.Serialize(webService);
                serviceModel.Setup(model => model.TestService(It.IsAny<IWebService>())).Returns(testResult);
                GetViewModel().SourceRegion.SelectedSource = _webHelooWebSource;
            }
        }

        [Then(@"Delete mapped outputs are")]
        public void ThenDeleteMappedOutputsAre(Table table)
        {
            var vm = GetViewModel();
            if (table.Rows.Count == 0)
            {
                if (vm.OutputsRegion.Outputs != null)
                    Assert.AreEqual<int>(vm.OutputsRegion.Outputs.Count, 0);
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
    }
}
