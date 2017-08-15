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
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;



namespace Dev2.Activities.Specs.Toolbox.Resources.Web
{
    [Binding]
    public class PostWebConnectorToolSteps
    {
        private readonly ScenarioContext scenarioContext;

        public PostWebConnectorToolSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

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
            var viewModel = new WebServicePostViewModel(modelItem, mockServiceModel.Object);


            scenarioContext.Add("viewModel", viewModel);
            scenarioContext.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            scenarioContext.Add("mockServiceModel", mockServiceModel);
        }

        WebServicePostViewModel PostViewModel()
        {
            return scenarioContext.Get<WebServicePostViewModel>("viewModel");
        }

        Mock<IWebServiceModel> PostServiceModel()
        {
            return scenarioContext.Get<Mock<IWebServiceModel>>("mockServiceModel");
        }

        [When(@"Post Test Inputs is Successful")]
        public void WhenTestInputsIsSuccessful()
        {
            
            PostViewModel().ManageServiceInputViewModel.TestCommand.Execute(null);
        }

        [Given(@"Post New is Enabled")]
        [When(@"Post New is Enabled")]
        [Then(@"Post New is Enabled")]
        public void WhenNewIsEnabled()
        {
            var canExecuteNewCommand = PostViewModel().SourceRegion.NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"Post Edit is Enabled")]
        [When(@"Post Edit is Enabled")]
        [Then(@"Post Edit is Enabled")]
        public void WhenEditIsEnabled()
        {
            var canExecuteNewCommand = PostViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"Post Edit is Disabled")]
        public void GivenEditIsDisabled()
        {
            var canExecuteNewCommand = PostViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecuteNewCommand);
        }

        [When(@"I click Post Edit")]
        public void WhenIClickEdit()
        {
           PostViewModel().SourceRegion.EditSourceCommand.Execute(null);
        }

        [Then(@"Post Header is Enabled")]
        public void ThenHeaderIsEnabled()
        {
            Assert.AreEqual<int>(1, PostViewModel().InputArea.Headers.Count);
        }

        [Then(@"Post Header appears as")]
        public void ThenHeaderAppearsAs(Table table)
        {
            var headers = PostViewModel().InputArea.Headers;
            foreach(var tableRow in table.Rows)
            {
                var name = tableRow["Header"];
                var value = tableRow["Value"];
                var found = headers.FirstOrDefault(nameValue => nameValue.Name == name && nameValue.Value == value);
                Assert.IsNotNull(found);
            }
        }

        [Then(@"Post Body is Enabled")]
        public void ThenBodyIsEnabled()
        {
            Assert.IsTrue(PostViewModel().InputArea.IsEnabled);
        }

        [Then(@"Post Url is Visible")]
        public void ThenUrlIsVisible()
        {
            Assert.IsTrue(PostViewModel().InputArea.IsEnabled);
        }

        [Then(@"Post Query is Enabled")]
        public void ThenQueryIsEnabled()
        {
            Assert.IsTrue(PostViewModel().InputArea.IsEnabled);
        }

        [Then(@"Post Generate Outputs is Enabled")]
        public void ThenGenerateOutputsIsEnabled()
        {
            var canGenerateOutputs = PostViewModel().TestInputCommand.CanExecute();
            Assert.IsTrue(canGenerateOutputs);
        }

        [Then(@"Post the Generate Outputs window is shown")]
        public void ThenTheGenerateOutputsWindowIsShown()
        {
            var webServicePostViewModel = PostViewModel();
            Assert.IsTrue(webServicePostViewModel.GenerateOutputsVisible);
            Assert.IsTrue(webServicePostViewModel.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.IsFalse(webServicePostViewModel.ManageServiceInputViewModel.OutputArea.IsEnabled);
        }

        [Then(@"Post Variables are Enabled")]
        public void ThenVariablesAreEnabled()
        {
            var webServicePostViewModel = PostViewModel();
            Assert.IsTrue(webServicePostViewModel.ManageServiceInputViewModel.InputArea.IsEnabled);
        }

        [Then(@"Post the response is loaded")]
        public void ThenTheResponseIsLoaded()
        {
            var webServicePostViewModel = PostViewModel();
            Assert.IsTrue(webServicePostViewModel.ManageServiceInputViewModel.OutputArea.IsEnabled);
        }


        [Then(@"Post Mapping is Enabled")]
        public void ThenMappingIsEnabled()
        {
            var webServicePostViewModel = PostViewModel();
            Assert.IsTrue(webServicePostViewModel.OutputsRegion.IsEnabled);
        }

        [Then(@"I enter ""(.*)"" as Post Query String")]
        public void ThenIEnterAsQueryString(string queryString)
        {
            var webServicePostViewModel = PostViewModel();
            webServicePostViewModel.InputArea.QueryString = queryString;
        }

        [Then(@"Post Url as ""(.*)""")]
        public void ThenUrlAs(string url)
        {
            var webServicePostViewModel = PostViewModel();
            Assert.AreEqual<string>(url,webServicePostViewModel.InputArea.RequestUrl);
        }

        [Then(@"I add Post Header as")]
        public void ThenIAddHeaderAs(Table table)
        {
            var headers = PostViewModel().InputArea.Headers;
            foreach(var tableRow in table.Rows)
            {
                var name = tableRow["Name"];
                var value = tableRow["Value"];
                headers.Add(new NameValue(name,value));
            }
        }

        [Then(@"Post Input variables are")]
        public void ThenInputVariablesAre(Table table)
        {
            var serviceInputs = PostViewModel().ManageServiceInputViewModel.InputArea.Inputs;
            foreach(var tableRow in table.Rows)
            {
                var inputName = tableRow["Name"];
                var found = serviceInputs.FirstOrDefault(input => input.Name == inputName);
                Assert.IsNotNull(found);
            }
        }

        [Then(@"Post Test is Enabled")]
        public void ThenTestIsEnabled()
        {
            var webServicePostViewModel = PostViewModel();
            var canExecuteTest = webServicePostViewModel.ManageServiceInputViewModel.TestCommand.CanExecute(null);
            Assert.IsTrue(canExecuteTest);
        }

        [Then(@"Post Paste is Enabled")]
        public void ThenPasteIsEnabled()
        {
            var webServicePostViewModel = PostViewModel();
            var canPaste = webServicePostViewModel.ManageServiceInputViewModel.PasteResponseCommand.CanExecute(null);
            Assert.IsTrue(canPaste);
        }

        [Then(@"the ""(.*)"" Post Source tab is opened")]
        public void ThenTheSourceTabIsOpened(string p0)
        {
            PostServiceModel().Verify(model => model.EditSource(It.IsAny<IWebServiceSource>()));
        }

        [Given(@"I click Post Generate Outputs")]
        [When(@"I click Post Generate Outputs")]
        [Then(@"I click Post Generate Outputs")]
        public async Task ThenIClickGenerateOutputs()
        {
            var webServicePostViewModel = PostViewModel();
            await webServicePostViewModel.TestInputCommand.Execute();
        }

        [Then(@"Post Response appears as ""(.*)""")]
        public void ThenResponseAppearsAs(string response)
        {
            var webServicePostViewModel = PostViewModel();
            Assert.AreEqual<string>(response,webServicePostViewModel.ManageServiceInputViewModel.TestResults);
        }

        [Then(@"Post Mappings is Disabled")]
        public void ThenMappingsIsDisabled()
        {
            var webServicePostViewModel = PostViewModel();
            Assert.IsFalse(webServicePostViewModel.OutputsRegion.IsEnabled);
        }


        [Given(@"I click Post Done")]
        [When(@"I click Post Done")]
        [Then(@"I click Post Done")]
        public void ThenIClickDone()
        {
            var webServicePostViewModel = PostViewModel();
            webServicePostViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
        }

        [Then(@"Post output mappings are")]
        public void ThenOutputMappingsAre(Table table)
        {
            var webServicePostViewModel = PostViewModel();
            var outputs = webServicePostViewModel.OutputsRegion.Outputs;
            foreach(var tableRow in table.Rows)
            {
                var mappedFrom = tableRow["Mapped From"];
                var mappedTo = tableRow["Mapped To"];
                var found = outputs.FirstOrDefault(mapping => mapping.MappedFrom == mappedFrom && mapping.MappedTo == mappedTo);
                Assert.IsNotNull(found);
            }
        }

        [When(@"I Select ""(.*)"" as a Post web Source")]
        public void WhenISelectAsAWebSource(string sourceName)
        {
            if (sourceName == "Dev2CountriesWebService")
            {
                var serviceModel = PostServiceModel();
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
                PostViewModel().SourceRegion.SelectedSource = _dev2CountriesWebServiceWebSource;
            }
            else if (sourceName == "Google Address Lookup")
            {
                PostViewModel().SourceRegion.SelectedSource = _googleWebSource;
            }
            else
            {
                var serviceModel = PostServiceModel();
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
                PostViewModel().SourceRegion.SelectedSource = _webHelooWebSource;
            }
        }

        [Then(@"Post mapped outputs are")]
        public void ThenMappedOutputsAre(Table table)
        {
            var vm = PostViewModel();
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
