/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Dev2.Intellisense;
using Dev2.Intellisense.Helper;
using Dev2.Intellisense.Provider;
using Dev2.Studio.Core.DataList;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Specs.Helper;
using Dev2.Studio.InterfaceImplementors;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Data.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Providers.Events;
using Moq;
using Dev2.Common.Interfaces.Infrastructure.Events;
using System.Text;

namespace Dev2.Studio.Core.Specs.IntellisenseSpecs
{
    [Binding]
    public class IntellisenseProviderSteps
    {
        readonly ScenarioContext _scenarioContext;

        public IntellisenseProviderSteps(ScenarioContext scenarioContext) => _scenarioContext = scenarioContext;
        
        [Given(@"I have the following variable list '(.*)'")]
        public void GivenIHaveTheFollowingVariableList(string variableList)
        {
            const string root = "<DataList>##</DataList>";
            var datalist = root.Replace("##", variableList);
            _scenarioContext.Add("dataList", datalist);

            var testEnvironmentModel = CreateMockEnvironment(new EventPublisher());

            var resourceModel = new ResourceModel(testEnvironmentModel.Object)
            {
                ResourceName = "test",
                ResourceType = ResourceType.Service,
                DataList = datalist
            };

            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(resourceModel);
            DataListSingleton.ActiveDataList.UpdateDataListItems(resourceModel, new List<IDataListVerifyPart>());
        }

        public static Mock<IServer> CreateMockEnvironment(IEventPublisher eventPublisher)
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(model => model.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder());
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);

            var environmentModel = new Mock<IServer>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);
            return environmentModel;
        }

        [Given(@"I have the following intellisense options '(.*)'")]
        public void GivenIHaveTheFollowingIntellisenseOptions(string p0)
        {
            _scenarioContext["datalistOptions"] = p0.Split( new[]{','});
        }

        [Given(@"the filter type is '(.*)'")]
        public void GivenTheFilterTypeIs(string filterType)
        {
            var filterTypeEnum = (enIntellisensePartType)Enum.Parse(typeof(enIntellisensePartType), filterType);
            _scenarioContext.Add("filterType", filterTypeEnum);
        }

        [Given(@"the current text in the textbox is '(.*)'")]
        public void GivenTheCurrentTextInTheTextboxIs(string inputText)
        {
            _scenarioContext.Add("inputText", inputText);
        }

        [Given(@"the cursor is at index '(.*)'")]
        public void GivenTheCursorIsAtIndex(int cursorIndex)
        {
            _scenarioContext.Add("cursorIndex", cursorIndex);
        }

        [Given(@"the provider used is '(.*)'")]
        public void GivenTheProviderUsedIs(string providerName)
        {
            IIntellisenseProvider provider;

            var providers = providerName.Split(',');

            if(providers.Length > 1)
            {
                var composite = new CompositeIntellisenseProvider();
                composite.AddRange(providers.Select(GetProvider));
                provider = composite;
            }
            else
            {
                provider = GetProvider(providerName);
            }

            _scenarioContext.Add("IsInCalculate", providerName.Contains("Calc"));
            _scenarioContext.Add("provider", provider);
        }




        [Given(@"the file path structure is '(.*)'")]
        public void GivenTheFilePathStructureIs(string pathStructure)
        {
            var fileQueryHelper = new FileQueryHelper();

            if(!string.IsNullOrEmpty(pathStructure))
            {
                var paths = pathStructure.Split(',').ToList();
                paths.ForEach(fileQueryHelper.Add);
            }

            _scenarioContext.Add("fileQueryHelper", fileQueryHelper);
        }


        IIntellisenseProvider GetProvider(string providerName)
        {
            IIntellisenseProvider provider = new DefaultIntellisenseProvider();

            switch (providerName.Trim())
            {
                case "Calculate":
                    provider = new CalculateIntellisenseProvider();
                    break;
                case "File":
                    var fileSystemIntellisenseProvider = new FileSystemIntellisenseProvider { FileSystemQuery = _scenarioContext.Get<IFileSystemQuery>("fileQueryHelper") };
                    provider = fileSystemIntellisenseProvider;
                    _scenarioContext.Add("isFileProvider", true);
                    break;
                case "DateTime":
                    provider = new DateTimeIntellisenseProvider();
                    break;
                default:
                    break;
            }

            return provider;
        }


        [Then(@"the result has '(.*)' errors")]
        public void ThenTheResultHasErrors(string p0)
        {
            var calc = _scenarioContext.Get<bool>("IsInCalculate");

            var context = new IntellisenseProviderContext
            {
                CaretPosition = _scenarioContext.Get<int>("cursorIndex"),
                CaretPositionOnPopup = _scenarioContext.Get<int>("cursorIndex"),
                InputText = _scenarioContext.Get<string>("inputText"),
                DesiredResultSet = calc ? IntellisenseDesiredResultSet.ClosestMatch : IntellisenseDesiredResultSet.Default,
                FilterType = _scenarioContext.Get<enIntellisensePartType>("filterType"),
                IsInCalculateMode = calc
            };

            _scenarioContext.Add("context", context);

            var provider = _scenarioContext.Get<IIntellisenseProvider>("provider");

            var getResults = provider.GetIntellisenseResults(context);
            var actualist = getResults.Where(i => i.IsError);
            Assert.AreEqual(!actualist.Any(),bool.Parse(p0));
        }

        [Then(@"the result has the error '(.*)'")]
        public void ThenTheResultHasTheError(string errorMessage)
        {
            var inputText = _scenarioContext.Get<string>("inputText");
            var error = IntellisenseStringProvider.parseLanguageExpressionAndValidate(inputText);
            Assert.AreEqual(errorMessage.TrimEnd(' '), error.Item2.TrimEnd(' '));
        }
        
        [Given(@"the suggestion list as '(.*)'")]
        public void GivenTheSuggestionListAs(string p0)
        {
            var provider = new Dev2TrieSuggestionProvider();
            provider.VariableList = new ObservableCollection<string>(_scenarioContext["datalistOptions"] as IEnumerable<string>);
            var filterType = _scenarioContext["filterType"] is enIntellisensePartType ? (enIntellisensePartType)_scenarioContext["filterType"] : enIntellisensePartType.None;
            var caretpos = int.Parse(_scenarioContext["cursorIndex"].ToString());
            var options = provider.GetSuggestions(_scenarioContext["inputText"].ToString(), caretpos, true,filterType);
            var selected = p0.Split(new[] { ',' });
             if(p0=="" && !options.Any())
            {
                return;
            }

            var all = true;
            foreach (var a in selected)
            {
                if(!String.IsNullOrEmpty(a)&& !options.Contains(a))
                {
                    all = false;
                    break;
                }
            }
            Assert.IsTrue(all);
            _scenarioContext["stringOptions"] = options;
        }



        [Given(@"the drop down list as '(.*)'")]
        public void GivenTheDropDownListAs(string dropDownList)
        {
            var expectedList = string.IsNullOrEmpty(dropDownList) ? new string[] { } : dropDownList.Split(',');
            var calc = _scenarioContext.Get<bool>("IsInCalculate");

            var context = new IntellisenseProviderContext
            {
                CaretPosition = _scenarioContext.Get<int>("cursorIndex"),
                CaretPositionOnPopup = _scenarioContext.Get<int>("cursorIndex"),
                InputText = _scenarioContext.Get<string>("inputText"),
                DesiredResultSet = calc ? IntellisenseDesiredResultSet.ClosestMatch : IntellisenseDesiredResultSet.Default,
                FilterType = _scenarioContext.Get<enIntellisensePartType>("filterType"),
                IsInCalculateMode = calc
            };

            _scenarioContext.Add("context", context);

            var provider = _scenarioContext.Get<IIntellisenseProvider>("provider");

            var getResults = provider.GetIntellisenseResults(context);
            var actualist = getResults.Where(i => !i.IsError).Select(i => i.Name).ToArray();
            Assert.AreEqual(expectedList.Length, actualist.Length, "Items not equal. Expected list: \"" + string.Join(",", expectedList) + "\", Actual list: \"" + string.Join(",", actualist) + "\"");
            var all = true;
            foreach (var a in expectedList)
            {
                if (!String.IsNullOrEmpty(a) && !actualist.Contains(a))
                {
                    all = false;
                    break;
                }
            }
            Assert.IsTrue(all);
        }

        [When(@"I select the following option '(.*)'")]
        public void WhenISelectTheFollowingOption(string option)
        {
            var context = _scenarioContext.Get<IntellisenseProviderContext>("context");
            string result;

            if (_scenarioContext.TryGetValue("isFileProvider", out bool isFileProvider))
            {
                if (DataListUtil.IsEvaluated(option) || string.IsNullOrEmpty(option))
                {
                    result = new DefaultIntellisenseProvider().PerformResultInsertion(option, context);
                }
                else
                {
                    result = new FileSystemIntellisenseProvider().PerformResultInsertion(option, context);
                }
            }
            else
            {
                result = new DefaultIntellisenseProvider().PerformResultInsertion(option, context);
            }

            _scenarioContext.Add("result", result);
        }


        [When(@"I select the following string option '(.*)'")]
        public void WhenISelectTheFollowingStringOption(string option)
        { 
            var originalText =_scenarioContext["inputText"].ToString();
            var caretpos = int.Parse(_scenarioContext["cursorIndex"].ToString());
            if (option=="")
            {
                _scenarioContext["stringResult"] = new IntellisenseStringResult(originalText,caretpos) ;
                return;
            }

            var options =_scenarioContext["stringOptions"] as IEnumerable<string>;
            Assert.IsTrue(options.Contains(option));
           
            var builder = new IntellisenseStringResultBuilder();
            var res = builder.Build(option,caretpos, originalText,originalText);
            _scenarioContext["stringResult"] = res;
        }

        [Then(@"the result text should be '(.*)'")]
        public void ThenTheResultTextShouldBe(string result)
        {
            var actual = _scenarioContext.Get<string>("result");
            Assert.AreEqual(result, actual);
        }

        [Then(@"the caret position will be '(.*)'")]
        public void ThenTheCaretPositionWillBe(int caretpostion)
        {
            var context = _scenarioContext.Get<IntellisenseProviderContext>("context");
            Assert.AreEqual(caretpostion, context.CaretPosition);
        }

        [Then(@"the result text should be ""(.*)"" with caret position will be '(.*)'")]
        public void ThenTheResultTextShouldBeWithCaretPositionWillBe(string p0, int p1)
        {
            var res =  _scenarioContext["stringResult"] as IIntellisenseStringResult;
            Assert.IsNotNull(res);
            Assert.AreEqual(res.Result,p0);
            Assert.AreEqual(p1,res.CaretPosition);
        }
    }
}
