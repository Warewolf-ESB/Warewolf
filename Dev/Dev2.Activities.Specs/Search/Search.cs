using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Search;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Search
{
    [Binding]
    public sealed class Search
    {
        readonly ScenarioContext _scenarioContext;
        readonly CommonSteps _commonSteps;
        IServer localHost;
        readonly IServerRepository environmentModel = ServerRepository.Instance;
        Common.Search.Search searchValue;

        public Search(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            _commonSteps = new CommonSteps(_scenarioContext);
        }
        
        [BeforeScenario("WarewolfSearch")]
        public void Setup()
        {
            var search = new Search(_scenarioContext);
            localHost = environmentModel.Source;
            localHost.Connect();
            if (!localHost.IsConnected)
            {
                Assert.Fail("expected valid connection to localhost");
            }
            searchValue = new Common.Search.Search();
        }

        [Given(@"I have the Search View open")]
        public void GivenIHaveTheSearchViewOpen()
        {
            Assert.IsNotNull(localHost);
            Assert.IsNotNull(localHost.ResourceRepository);
        }

        [Given(@"I check the ""(.*)"" checkbox")]
        public void GivenICheckTheCheckbox(string searchOption)
        {
            switch (searchOption)
            {
                case "IsWorkflowNameSelected":
                    searchValue.SearchOptions.IsWorkflowNameSelected = true;
                    break;
                case "IsTestNameSelected":
                    searchValue.SearchOptions.IsTestNameSelected = true;
                    break;
                case "IsScalarNameSelected":
                    searchValue.SearchOptions.IsScalarNameSelected = true;
                    break;
                case "IsObjectNameSelected":
                    searchValue.SearchOptions.IsObjectNameSelected = true;
                    break;
                case "IsRecSetNameSelected":
                    searchValue.SearchOptions.IsRecSetNameSelected = true;
                    break;
                case "IsToolTitleSelected":
                    searchValue.SearchOptions.IsToolTitleSelected = true;
                    break;
                case "IsInputVariableSelected":
                    searchValue.SearchOptions.IsInputVariableSelected = true;
                    break;
                case "IsOutputVariableSelected":
                    searchValue.SearchOptions.IsOutputVariableSelected = true;
                    break;
                default:
                    searchValue.SearchOptions.IsAllSelected = true;
                    break;
            }
        }

        [When(@"I search for ""(.*)""")]
        public void WhenISearchFor(string input)
        {
            searchValue.SearchInput = input;
        }

        [Then(@"the search result contains")]
        public void ThenTheSearchResultContains(Table table)
        {
            var results = localHost.ResourceRepository.Filter(searchValue);

            var resourceId = Guid.Parse(table.Rows[0]["ResourceId"]);
            var name = table.Rows[0]["Name"];
            var path = table.Rows[0]["Path"];
            var type = GetSearchItemType(table.Rows[0]["Type"]);
            var match = table.Rows[0]["Match"];

            ISearchResult expectedSearchResult = new SearchResult(resourceId, name, path, type, match);
            var expectedSearchResults = new List<ISearchResult>
            {
                expectedSearchResult
            };
            Assert.IsTrue(results.Contains(expectedSearchResult));
        }

        private static SearchItemType GetSearchItemType(string type)
        {
            SearchItemType searchResultType;
            switch (type)
            {
                case "SourceName":
                    searchResultType = SearchItemType.SourceName;
                    break;
                case "ToolTitle":
                    searchResultType = SearchItemType.ToolTitle;
                    break;
                case "Scalar":
                    searchResultType = SearchItemType.Scalar;
                    break;
                case "ScalarInput":
                    searchResultType = SearchItemType.ScalarInput;
                    break;
                case "ScalarOutput":
                    searchResultType = SearchItemType.ScalarOutput;
                    break;
                case "RecordSet":
                    searchResultType = SearchItemType.RecordSet;
                    break;
                case "RecordSetInput":
                    searchResultType = SearchItemType.RecordSetInput;
                    break;
                case "RecordSetOutput":
                    searchResultType = SearchItemType.RecordSetOutput;
                    break;
                case "Object":
                    searchResultType = SearchItemType.Object;
                    break;
                case "ObjectInput":
                    searchResultType = SearchItemType.ObjectInput;
                    break;
                case "ObjectOutput":
                    searchResultType = SearchItemType.ObjectOutput;
                    break;
                case "TestName":
                    searchResultType = SearchItemType.TestName;
                    break;
                default:
                    searchResultType = SearchItemType.WorkflowName;
                    break;
            }

            return searchResultType;
        }
    }
}
