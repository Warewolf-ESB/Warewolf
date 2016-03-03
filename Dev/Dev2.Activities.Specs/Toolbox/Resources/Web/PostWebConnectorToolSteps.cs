using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dev2.Activities.Designers2.Web_Service_Post;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;

// ReSharper disable UnusedMember.Global

namespace Dev2.Activities.Specs.Toolbox.Resources.Web
{
    [Binding]
    public class PostWebConnectorToolSteps
    {
        private WebServiceSourceDefinition _dev2CountriesWebServiceWebSource;
        private WebServiceSourceDefinition _webHelooWebSource;
        private WebServiceSourceDefinition _googleWebSource;

        [Given(@"I drag Web Post Request Connector Tool onto the design surface")]
        public void GivenIDragWebPostRequestConnectorToolOntoTheDesignSurface()
        {
            var activity = new DsfWebPostActivity();
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var mockServiceInputViewModel = new Mock<IManageWebServiceInputViewModel>();
            var mockServiceModel = new Mock<IWebServiceModel>();
            var mockEnvironmentRepo = new Mock<IEnvironmentRepository>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveEnvironment).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(mockEnvironmentModel.Object);

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
            var viewModel = new WebServicePostViewModel(modelItem, mockServiceModel.Object);


            ScenarioContext.Current.Add("viewModel", viewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockServiceModel", mockServiceModel);
        }

        private static WebServicePostViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<WebServicePostViewModel>("viewModel");
        }

        private static Mock<IWebServiceModel> GetServiceModel()
        {
            return ScenarioContext.Current.Get<Mock<IWebServiceModel>>("mockServiceModel");
        }

        [When(@"Test Inputs is Successful")]
        public void WhenTestInputsIsSuccessful()
        {
            
            GetViewModel().ManageServiceInputViewModel.TestCommand.Execute(null);
        }

        [Given(@"New is Enabled")]
        [When(@"New is Enabled")]
        [Then(@"New is Enabled")]
        public void WhenNewIsEnabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"Edit is Enabled")]
        [When(@"Edit is Enabled")]
        [Then(@"Edit is Enabled")]
        public void WhenEditIsEnabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"Edit is Disabled")]
        public void GivenEditIsDisabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecuteNewCommand);
        }

        [When(@"I click Edit")]
        public void WhenIClickEdit()
        {
           GetViewModel().SourceRegion.EditSourceCommand.Execute(null);
        }              
        
        [Then(@"Header is Enabled")]
        public void ThenHeaderIsEnabled()
        {
            Assert.AreEqual(1, GetViewModel().InputArea.Headers.Count);
        }
        
        [Then(@"Header appears as")]
        public void ThenHeaderAppearsAs(Table table)
        {
            var headers = GetViewModel().InputArea.Headers;
            foreach(var tableRow in table.Rows)
            {
                var name = tableRow["Header"];
                var value = tableRow["Value"];
                var found = headers.FirstOrDefault(nameValue => nameValue.Name == name && nameValue.Value == value);
                Assert.IsNotNull(found);
            }
        }

        [Then(@"Body is Enabled")]
        public void ThenBodyIsEnabled()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsVisible);
        }
        
        [Then(@"Url is Visible")]
        public void ThenUrlIsVisible()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsVisible);
        }
        
        [Then(@"Query is Enabled")]
        public void ThenQueryIsEnabled()
        {
            Assert.IsTrue(GetViewModel().InputArea.IsVisible);
        }
              
        [Then(@"Generate Outputs is Enabled")]
        public void ThenGenerateOutputsIsEnabled()
        {
            var canGenerateOutputs = GetViewModel().TestInputCommand.CanExecute();
            Assert.IsTrue(canGenerateOutputs);
        }

        [Then(@"the Generate Outputs window is shown")]
        public void ThenTheGenerateOutputsWindowIsShown()
        {
            var webServicePostViewModel = GetViewModel();
            Assert.IsTrue(webServicePostViewModel.GenerateOutputsVisible);
            Assert.IsTrue(webServicePostViewModel.ManageServiceInputViewModel.InputArea.IsVisible);
            Assert.IsFalse(webServicePostViewModel.ManageServiceInputViewModel.OutputArea.IsVisible);
            Assert.IsTrue(webServicePostViewModel.ManageServiceInputViewModel.IsVisible);
        }

        [Then(@"Variables are Enabled")]
        public void ThenVariablesAreEnabled()
        {
            var webServicePostViewModel = GetViewModel();
            Assert.IsTrue(webServicePostViewModel.ManageServiceInputViewModel.InputArea.IsVisible);
        }

        [Then(@"the response is loaded")]
        public void ThenTheResponseIsLoaded()
        {
            var webServicePostViewModel = GetViewModel();
            Assert.IsTrue(webServicePostViewModel.ManageServiceInputViewModel.OutputArea.IsVisible);
        }


        [Then(@"Mapping is Enabled")]
        public void ThenMappingIsEnabled()
        {
            var webServicePostViewModel = GetViewModel();
            Assert.IsTrue(webServicePostViewModel.OutputsRegion.IsVisible);
        }
        
        [Then(@"I enter ""(.*)"" as Query String")]
        public void ThenIEnterAsQueryString(string queryString)
        {
            var webServicePostViewModel = GetViewModel();
            webServicePostViewModel.InputArea.QueryString = queryString;
        }

        [Then(@"Url as ""(.*)""")]
        public void ThenUrlAs(string url)
        {
            var webServicePostViewModel = GetViewModel();
            Assert.AreEqual(url,webServicePostViewModel.InputArea.RequestUrl);
        }
        
        [Then(@"I add Header as")]
        public void ThenIAddHeaderAs(Table table)
        {
            var headers = GetViewModel().InputArea.Headers;
            foreach(var tableRow in table.Rows)
            {
                var name = tableRow["Name"];
                var value = tableRow["Value"];
                headers.Add(new NameValue(name,value));
            }
        }

        [Then(@"Input variables are")]
        public void ThenInputVariablesAre(Table table)
        {
            var serviceInputs = GetViewModel().ManageServiceInputViewModel.InputArea.Inputs;
            foreach(var tableRow in table.Rows)
            {
                var inputName = tableRow["Name"];
                var found = serviceInputs.FirstOrDefault(input => input.Name == inputName);
                Assert.IsNotNull(found);
            }
        }

        [Then(@"Test is Enabled")]
        public void ThenTestIsEnabled()
        {
            var webServicePostViewModel = GetViewModel();
            var canExecuteTest = webServicePostViewModel.ManageServiceInputViewModel.TestCommand.CanExecute(null);
            Assert.IsTrue(canExecuteTest);
        }
        
        [Then(@"Paste is Enabled")]
        public void ThenPasteIsEnabled()
        {
            var webServicePostViewModel = GetViewModel();
            var canPaste = webServicePostViewModel.ManageServiceInputViewModel.PasteResponseCommand.CanExecute(null);
            Assert.IsTrue(canPaste);
        }
        
        [Then(@"the ""(.*)"" Source tab is opened")]
        public void ThenTheSourceTabIsOpened(string p0)
        {
            GetServiceModel().Verify(model => model.EditSource(It.IsAny<IWebServiceSource>()));
        }
        
        [Given(@"I click Generate Outputs")]
        [When(@"I click Generate Outputs")]
        [Then(@"I click Generate Outputs")]
        public async Task ThenIClickGenerateOutputs()
        {
            var webServicePostViewModel = GetViewModel();
            await webServicePostViewModel.TestInputCommand.Execute();
        }
        
        [Then(@"Response appears as ""(.*)""")]
        public void ThenResponseAppearsAs(string response)
        {
            var webServicePostViewModel = GetViewModel();
            Assert.AreEqual(response,webServicePostViewModel.ManageServiceInputViewModel.TestResults);
        }
        
        [Then(@"Mappings is Disabled")]
        public void ThenMappingsIsDisabled()
        {
            var webServicePostViewModel = GetViewModel();
            Assert.IsFalse(webServicePostViewModel.OutputsRegion.IsVisible);
        }
                      
        
        [Given(@"I click Done")]
        [When(@"I click Done")]
        [Then(@"I click Done")]
        public void ThenIClickDone()
        {
            var webServicePostViewModel = GetViewModel();
            webServicePostViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
        }
        
        [Then(@"output mappings are")]
        public void ThenOutputMappingsAre(Table table)
        {
            var webServicePostViewModel = GetViewModel();
            var outputs = webServicePostViewModel.OutputsRegion.Outputs;
            foreach(var tableRow in table.Rows)
            {
                var mappedFrom = tableRow["Mapped From"];
                var mappedTo = tableRow["Mapped To"];
                var found = outputs.FirstOrDefault(mapping => mapping.MappedFrom == mappedFrom && mapping.MappedTo == mappedTo);
                Assert.IsNotNull(found);
            }
        }

        [When(@"I Select ""(.*)"" as a web Source")]
        public void WhenISelectAsAWebSource(string sourceName)
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

        [Then(@"mapped outputs are")]
        public void ThenMappedOutputsAre(Table table)
        {
            var vm = GetViewModel();
            if (table.Rows.Count == 0)
            {
                if (vm.OutputsRegion.Outputs != null)
                    Assert.AreEqual(vm.OutputsRegion.Outputs.Count, 0);
            }
            else
            {
                var matched = table.Rows.Zip(vm.OutputsRegion.Outputs, (a, b) => new Tuple<TableRow, IServiceOutputMapping>(a, b));
                foreach (var a in matched)
                {
                    Assert.AreEqual(a.Item1[0], a.Item2.MappedFrom);
                    Assert.AreEqual(a.Item1[1], a.Item2.MappedTo);

                }
            }
        }

    }
}
