using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dev2.Activities.Designers2.Web_Service_Put;
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
    public sealed class PutWebConnectoToolSteps
    {
        private readonly ScenarioContext scenarioContext;

        public PutWebConnectoToolSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        private WebServiceSourceDefinition _dev2CountriesWebServiceWebSource;
        private WebServiceSourceDefinition _webHelooWebSource;
        private WebServiceSourceDefinition _googleWebSource;

        [Given(@"I drag Web Put Request Connector Tool onto the design surface")]
        public void GivenIDragWebPutRequestConnectorToolOntoTheDesignSurface()
        {
            var activity = new DsfWebPutActivity();
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
                Name = "Google Address Lookup",
            };
            var sources = new ObservableCollection<IWebServiceSource> { _dev2CountriesWebServiceWebSource };
            mockServiceModel.Setup(model => model.RetrieveSources()).Returns(sources);
            mockServiceModel.Setup(model => model.EditSource(It.IsAny<IWebServiceSource>())).Verifiable();
            mockServiceInputViewModel.SetupAllProperties();
            var viewModel = new WebServicePutViewModel(modelItem, mockServiceModel.Object);


            scenarioContext.Add("viewModel", viewModel);
            scenarioContext.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            scenarioContext.Add("mockServiceModel", mockServiceModel);
        }

        private WebServicePutViewModel GetViewModel()
        {
            return scenarioContext.Get<WebServicePutViewModel>("viewModel");
        }

        private Mock<IWebServiceModel> GetServiceModel()
        {
            return scenarioContext.Get<Mock<IWebServiceModel>>("mockServiceModel");
        }

        [When(@"Put Test Inputs is Successful")]
        public void WhenTestInputsIsSuccessful()
        {

            GetViewModel().ManageServiceInputViewModel.TestCommand.Execute(null);
        }

        [Given(@"Put New is Enabled")]
        [When(@"Put New is Enabled")]
        [Then(@"Put New is Enabled")]
        public void WhenPutNewIsEnabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"Put Edit is Enabled")]
        [When(@"Put Edit is Enabled")]
        [Then(@"Put Edit is Enabled")]
        public void WhenPutEditIsEnabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"Put Edit is Disabled")]
        public void GivenEditIsDisabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecuteNewCommand);
        }

        [When(@"I click Put Edit")]
        public void WhenPutIClickEdit()
        {
            GetViewModel().SourceRegion.EditSourceCommand.Execute(null);
        }

        [Then(@"Put Header is Enabled")]
        public void ThenHeaderIsEnabled()
        {
            Assert.AreEqual<int>(1, GetViewModel().InputArea.Headers.Count);
        }

        [Then(@"Put Header appears as")]
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

        [Then(@"Put Body is Enabled")]
        public void ThenPutBodyIsEnabled()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
        }

        [Then(@"Put Url is Visible")]
        public void ThenPutUrlIsVisible()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
        }

        [Then(@"Put Query is Enabled")]
        public void ThenPutQueryIsEnabled()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
        }

        [Then(@"Put Generate Outputs is Enabled")]
        public void ThenPutGenerateOutputsIsEnabled()
        {
            var canGenerateOutputs = GetViewModel().TestInputCommand.CanExecute();
            Assert.IsTrue(canGenerateOutputs);
        }

        [Then(@"the Put Generate Outputs window is shown")]
        public void ThenThePutGenerateOutputsWindowIsShown()
        {
            var webServicePutViewModel = GetViewModel();
            Assert.IsTrue(webServicePutViewModel.GenerateOutputsVisible);
            Assert.IsTrue(webServicePutViewModel.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.IsFalse(webServicePutViewModel.ManageServiceInputViewModel.OutputArea.IsEnabled);
        }

        [Then(@"Put Variables are Enabled")]
        public void ThenPutVariablesAreEnabled()
        {
            var webServicePutViewModel = GetViewModel();
            Assert.IsTrue(webServicePutViewModel.ManageServiceInputViewModel.InputArea.IsEnabled);
        }

        [Then(@"the Put response is loaded")]
        public void ThenThePutResponseIsLoaded()
        {
            var webServicePutViewModel = GetViewModel();
            Assert.IsTrue(webServicePutViewModel.ManageServiceInputViewModel.OutputArea.IsEnabled);
        }


        [Then(@"Put Mapping is Enabled")]
        public void ThenPutMappingIsEnabled()
        {
            var webServicePutViewModel = GetViewModel();
            Assert.IsTrue(webServicePutViewModel.OutputsRegion.IsEnabled);
        }

        [Then(@"I enter ""(.*)"" as Put Query String")]
        public void ThenIEnterAsPutQueryString(string queryString)
        {
            var webServicePutViewModel = GetViewModel();
            webServicePutViewModel.InputArea.QueryString = queryString;
        }

        [Then(@"Put Url as ""(.*)""")]
        public void ThenPutUrlAs(string url)
        {
            var webServicePutViewModel = GetViewModel();
            Assert.AreEqual<string>(url, webServicePutViewModel.InputArea.RequestUrl);
        }

        [Then(@"I add Put Header as")]
        public void ThenIAddPutHeaderAs(Table table)
        {
            var headers = GetViewModel().InputArea.Headers;
            foreach (var tableRow in table.Rows)
            {
                var name = tableRow["Name"];
                var value = tableRow["Value"];
                headers.Add(new NameValue(name, value));
            }
        }

        [Then(@"Put Input variables are")]
        public void ThenPutInputVariablesAre(Table table)
        {
            var serviceInputs = GetViewModel().ManageServiceInputViewModel.InputArea.Inputs;
            foreach (var tableRow in table.Rows)
            {
                var inputName = tableRow["Name"];
                var found = serviceInputs.FirstOrDefault(input => input.Name == inputName);
                Assert.IsNotNull(found);
            }
        }

        [Then(@"Put Test is Enabled")]
        public void ThenPutTestIsEnabled()
        {
            var webServicePutViewModel = GetViewModel();
            var canExecuteTest = webServicePutViewModel.ManageServiceInputViewModel.TestCommand.CanExecute(null);
            Assert.IsTrue(canExecuteTest);
        }

        [Then(@"Put Paste is Enabled")]
        public void ThenPutPasteIsEnabled()
        {
            var webServicePutViewModel = GetViewModel();
            var canPaste = webServicePutViewModel.ManageServiceInputViewModel.PasteResponseCommand.CanExecute(null);
            Assert.IsTrue(canPaste);
        }

        [Then(@"the ""(.*)"" Put Source tab is opened")]
        public void ThenThePutSourceTabIsOpened(string p0)
        {
            GetServiceModel().Verify(model => model.EditSource(It.IsAny<IWebServiceSource>()));
        }

        [Given(@"I click Put Generate Outputs")]
        [When(@"I click Put Generate Outputs")]
        [Then(@"I click Put Generate Outputs")]
        public async Task ThenIClickPutGenerateOutputs()
        {
            var webServicePutViewModel = GetViewModel();
            await webServicePutViewModel.TestInputCommand.Execute();
        }

        [Then(@"Put Response appears as ""(.*)""")]
        public void ThenPutResponseAppearsAs(string response)
        {
            var webServicePutViewModel = GetViewModel();
            Assert.AreEqual<string>(response, webServicePutViewModel.ManageServiceInputViewModel.TestResults);
        }

        [Then(@"Put Mappings is Disabled")]
        public void ThenPutMappingsIsDisabled()
        {
            var webServicePutViewModel = GetViewModel();
            Assert.IsFalse(webServicePutViewModel.OutputsRegion.IsEnabled);
        }


        [Given(@"I click Put Done")]
        [When(@"I click Put Done")]
        [Then(@"I click Put Done")]
        public void ThenIClickPutDone()
        {
            var webServicePutViewModel = GetViewModel();
            webServicePutViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
        }

        [Then(@"Put output mappings are")]
        public void ThenPutOutputMappingsAre(Table table)
        {
            var webServicePutViewModel = GetViewModel();
            var outputs = webServicePutViewModel.OutputsRegion.Outputs;
            foreach (var tableRow in table.Rows)
            {
                var mappedFrom = tableRow["Mapped From"];
                var mappedTo = tableRow["Mapped To"];
                var found = outputs.FirstOrDefault(mapping => mapping.MappedFrom == mappedFrom && mapping.MappedTo == mappedTo);
                Assert.IsNotNull(found);
            }
        }

        [When(@"I Select ""(.*)"" as a Put web Source")]
        public void WhenISelectAsAPutWebSource(string sourceName)
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

        [Then(@"Put mapped outputs are")]
        public void ThenPutMappedOutputsAre(Table table)
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
    }
}
