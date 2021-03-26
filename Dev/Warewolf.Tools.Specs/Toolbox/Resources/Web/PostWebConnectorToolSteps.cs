using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dev2.Activities.Designers2.Web_Service_Post;
using Dev2.Common;
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
        readonly ScenarioContext scenarioContext;

        public PostWebConnectorToolSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        WebServiceSourceDefinition _dev2CountriesWebServiceWebSource;
        WebServiceSourceDefinition _webHelooWebSource;
        WebServiceSourceDefinition _googleWebSource;

        [Given(@"I drag Web Post Request Connector Tool onto the design surface")]
        public void GivenIDragWebPostRequestConnectorToolOntoTheDesignSurface()
        {
            var activity = new WebPostActivity();
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
                HostName = "http://TFSBLD.premier.local/integrationTestSite/GetCountries.ashx"
            };

            _googleWebSource = new WebServiceSourceDefinition
            {
                Name = "Google Address Lookup",
            };
            var sources = new ObservableCollection<IWebServiceSource> { _dev2CountriesWebServiceWebSource };
            mockServiceModel.Setup(model => model.RetrieveSources()).Returns(sources);
            mockServiceModel.Setup(model => model.EditSource(It.IsAny<IWebServiceSource>())).Verifiable();
            mockServiceInputViewModel.SetupAllProperties();
            var viewModel = new WebPostActivity();


            scenarioContext.Add("viewModel", viewModel);
            scenarioContext.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            scenarioContext.Add("mockServiceModel", mockServiceModel);
        }

        WebPostActivity PostViewModel()
        {
            return scenarioContext.Get<WebPostActivity>("viewModel");
        }

        Mock<IWebServiceModel> PostServiceModel()
        {
            return scenarioContext.Get<Mock<IWebServiceModel>>("mockServiceModel");
        }

        [Then(@"Post Header is Enabled")]
        public void ThenHeaderIsEnabled()
        {
            Assert.AreEqual(null, PostViewModel().Headers);
        }

        [Then(@"Post Header appears as")]
        public void ThenHeaderAppearsAs(Table table)
        {
            var headers = PostViewModel().Headers;
            foreach(var tableRow in table.Rows)
            {
                var name = tableRow["Header"];
                var value = tableRow["Value"];
                var found = headers.FirstOrDefault(nameValue => nameValue.Name == name && nameValue.Value == value);
                Assert.IsNotNull(found);
            }
        }

        [Then(@"Post Query is Enabled")]
        public void ThenQueryIsEnabled()
        {
            Assert.AreEqual(null, PostViewModel().QueryString);
        }

        [Then(@"Post the response is loaded")]
        public void ThenTheResponseIsLoaded()
        {
            Assert.AreEqual(0, PostViewModel().ResponseManager.Outputs.Count);
        }


        [Then(@"Post Mapping is Enabled")]
        public void ThenMappingIsEnabled()
        {
            Assert.AreEqual(0, PostViewModel().Outputs.Count);
        }

        [Then(@"I enter ""(.*)"" as Post Query String")]
        public void ThenIEnterAsQueryString(string queryString)
        {
            PostViewModel().QueryString = queryString;
        }

        [Then(@"I add Post Header as")]
        public void ThenIAddHeaderAs(Table table)
        {
            var headers = PostViewModel().Headers;
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
            var serviceInputs = PostViewModel().Inputs;
            foreach(var tableRow in table.Rows)
            {
                var inputName = tableRow["Name"];
                var found = serviceInputs.FirstOrDefault(input => input.Name == inputName);
                Assert.IsNotNull(found);
            }
        }

        [Then(@"Post Response appears as ""(.*)""")]
        public void ThenResponseAppearsAs(string response)
        {
            Assert.AreEqual(response,PostViewModel().ResponseManager.ObjectName);
        }

        [Then(@"Post Mappings is Disabled")]
        public void ThenMappingsIsDisabled()
        {
            Assert.AreNotEqual(1, PostViewModel().Outputs.Count);
        }

        [Then(@"Post output mappings are")]
        public void ThenOutputMappingsAre(Table table)
        {
            var webServicePostViewModel = PostViewModel();
            var outputs = webServicePostViewModel.Outputs;
            foreach(var tableRow in table.Rows)
            {
                var mappedFrom = tableRow["Mapped From"];
                var mappedTo = tableRow["Mapped To"];
                var found = outputs.FirstOrDefault(mapping => mapping.MappedFrom == mappedFrom && mapping.MappedTo == mappedTo);
                Assert.IsNotNull(found);
            }
        }

        [Then(@"Post mapped outputs are")]
        public void ThenMappedOutputsAre(Table table)
        {
            var vm = PostViewModel();
            if (table.Rows.Count == 0)
            {
                if (vm.Outputs != null)
                {
                    Assert.AreEqual<int>(vm.Outputs.Count, 0);
                }
            }
            else
            {
                var matched = table.Rows.Zip(vm.Outputs, (a, b) => new Tuple<TableRow, IServiceOutputMapping>(a, b));
                foreach (var a in matched)
                {
                    Assert.AreEqual<string>(a.Item1[0], a.Item2.MappedFrom);
                    Assert.AreEqual<string>(a.Item1[1], a.Item2.MappedTo);

                }
            }
        }

    }
}
