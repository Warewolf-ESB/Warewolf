
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Linq;
using Dev2.Core.Tests;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Intellisense.Helper;
using Dev2.Intellisense.Provider;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Specs.Helper;
using Dev2.Studio.InterfaceImplementors;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Studio.Core.Specs.IntellisenseSpecs
{
    [Binding]
    public class IntellisenseProviderSteps
    {
        [Given(@"I have the following variable list '(.*)'")]
        public void GivenIHaveTheFollowingVariableList(string variableList)
        {
            const string Root = "<DataList>##</DataList>";
            var datalist = Root.Replace("##", variableList);
            ScenarioContext.Current.Add("dataList", datalist);

            var testEnvironmentModel = ResourceModelTest.CreateMockEnvironment();

            var resourceModel = new ResourceModel(testEnvironmentModel.Object)
            {
                ResourceName = "test",
                ResourceType = ResourceType.Service,
                DataList = datalist
            };

            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(resourceModel);
        }

        [Given(@"the filter type is '(.*)'")]
        public void GivenTheFilterTypeIs(string filterType)
        {
            var filterTypeEnum = (enIntellisensePartType)Enum.Parse(typeof(enIntellisensePartType), filterType);
            ScenarioContext.Current.Add("filterType", filterTypeEnum);
        }

        [Given(@"the current text in the textbox is '(.*)'")]
        public void GivenTheCurrentTextInTheTextboxIs(string inputText)
        {
            ScenarioContext.Current.Add("inputText", inputText);
        }

        [Given(@"the cursor is at index '(.*)'")]
        public void GivenTheCursorIsAtIndex(int cursorIndex)
        {
            ScenarioContext.Current.Add("cursorIndex", cursorIndex);
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

            ScenarioContext.Current.Add("IsInCalculate", providerName.Contains("Calc"));
            ScenarioContext.Current.Add("provider", provider);
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

            ScenarioContext.Current.Add("fileQueryHelper", fileQueryHelper);
        }


        static IIntellisenseProvider GetProvider(string providerName)
        {
            IIntellisenseProvider provider = new DefaultIntellisenseProvider();

            switch(providerName.Trim())
            {
                case "Calculate":
                    provider = new CalculateIntellisenseProvider();
                    break;
                case "File":
                    var fileSystemIntellisenseProvider = new FileSystemIntellisenseProvider { FileSystemQuery = ScenarioContext.Current.Get<IFileSystemQuery>("fileQueryHelper") };
                    provider = fileSystemIntellisenseProvider;
                    ScenarioContext.Current.Add("isFileProvider", true);
                    break;
                case "DateTime":
                    provider = new DateTimeIntellisenseProvider();
                    break;
            }

            return provider;
        }

        [Given(@"the drop down list as '(.*)'")]
        public void GivenTheDropDownListAs(string dropDownList)
        {
            var expectedList = string.IsNullOrEmpty(dropDownList) ? new string[] { } : dropDownList.Split(',');
            var calc = ScenarioContext.Current.Get<bool>("IsInCalculate");

            var context = new IntellisenseProviderContext
            {
                CaretPosition = ScenarioContext.Current.Get<int>("cursorIndex"),
                CaretPositionOnPopup = ScenarioContext.Current.Get<int>("cursorIndex"),
                InputText = ScenarioContext.Current.Get<string>("inputText"),
                DesiredResultSet = calc ? IntellisenseDesiredResultSet.ClosestMatch : IntellisenseDesiredResultSet.Default,
                FilterType = ScenarioContext.Current.Get<enIntellisensePartType>("filterType"),
                IsInCalculateMode = calc
            };

            ScenarioContext.Current.Add("context", context);

            IIntellisenseProvider provider = ScenarioContext.Current.Get<IIntellisenseProvider>("provider");

            var getResults = provider.GetIntellisenseResults(context);
            var actualist = getResults.Where(i => !i.IsError).Select(i => i.Name).ToArray();
            Assert.AreEqual(expectedList.Length, actualist.Length);
            CollectionAssert.AreEqual(expectedList, actualist);
        }

        [When(@"I select the following option '(.*)'")]
        public void WhenISelectTheFollowingOption(string option)
        {
            var context = ScenarioContext.Current.Get<IntellisenseProviderContext>("context");
            string result;
            bool isFileProvider;

            if(ScenarioContext.Current.TryGetValue("isFileProvider", out isFileProvider))
            {
                if(DataListUtil.IsEvaluated(option) || string.IsNullOrEmpty(option))
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

            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result text should be '(.*)'")]
        public void ThenTheResultTextShouldBe(string result)
        {
            var actual = ScenarioContext.Current.Get<string>("result");
            Assert.AreEqual(result, actual);
        }

        [Then(@"the caret position will be '(.*)'")]
        public void ThenTheCaretPositionWillBe(int caretpostion)
        {
            var context = ScenarioContext.Current.Get<IntellisenseProviderContext>("context");
            Assert.AreEqual(caretpostion, context.CaretPosition);
        }

    }
}
