/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Dev2.Core.Tests;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.Intellisense;
using Dev2.Intellisense.Helper;
using Dev2.Intellisense.Provider;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.DataList;
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
            const string root = "<DataList>##</DataList>";
            var datalist = root.Replace("##", variableList);
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
            DataListSingleton.ActiveDataList.UpdateDataListItems(resourceModel, new List<IDataListVerifyPart>());

        }

        [Given(@"I have the following intellisense options '(.*)'")]
        public void GivenIHaveTheFollowingIntellisenseOptions(string p0)
        {
            ScenarioContext.Current["datalistOptions"] = p0.Split( new char[]{','});
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


        [Then(@"the result has '(.*)' errors")]
        public void ThenTheResultHasErrors(string p0)
        {
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
            var actualist = getResults.Where(i => i.IsError);
            Assert.AreEqual(!actualist.Any(),bool.Parse(p0));
        }

        [Then(@"the result has the error '(.*)'")]
        public void ThenTheResultHasTheError(string errorMessage)
        {
            var inputText = ScenarioContext.Current.Get<string>("inputText");
            var error = IntellisenseStringProvider.parseLanguageExpressionAndValidate(inputText);
            Assert.AreEqual(errorMessage.TrimEnd(' '), error.Item2.TrimEnd(' '));
        }


        [Given(@"the options as '(.*)'")]
        public void GivenTheOptionsAs(string option)
        {
            //Dev2TrieSugggestionProvider provider = new Dev2TrieSugggestionProvider(IntellisenseStringProvider.FilterOption.All,1);
            //provider.VariableList = new ObservableCollection<string>( ScenarioContext.Current["datalistOptions"] as IEnumerable<string>);
            //provider.GetSuggestions(option);
        }


       

        [Given(@"the suggestion list as '(.*)'")]
        public void GivenTheSuggestionListAs(string p0)
        {
            Dev2TrieSugggestionProvider provider = new Dev2TrieSugggestionProvider();
            provider.VariableList = new ObservableCollection<string>(ScenarioContext.Current["datalistOptions"] as IEnumerable<string>);
            var filterType = ScenarioContext.Current["filterType"] is enIntellisensePartType ? (enIntellisensePartType)ScenarioContext.Current["filterType"] : enIntellisensePartType.All;
            int caretpos = int.Parse(ScenarioContext.Current["cursorIndex"].ToString());
            var options = provider.GetSuggestions(ScenarioContext.Current["inputText"].ToString(), caretpos, true,filterType);
            var selected = p0.Split(new char[] { ',' });
             if(p0=="" && !options.Any())
                 return;
            bool all = true;
            foreach(var a in selected)
            {
                if(!String.IsNullOrEmpty(a)&& !options.Contains(a))
                {
                    all = false;
                    break;
                }
            }
            Assert.IsTrue(all);
            ScenarioContext.Current["stringOptions"] = options;
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
            bool all = true;
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


        [When(@"I select the following string option '(.*)'")]
        public void WhenISelectTheFollowingStringOption(string option)
        { 
            string originalText =ScenarioContext.Current["inputText"].ToString();
             int caretpos = int.Parse(ScenarioContext.Current["cursorIndex"].ToString());
            if(option=="")
            {
                ScenarioContext.Current["stringResult"] = new IntellisenseStringResult(originalText,caretpos) ;
                return;
            }

            var options =ScenarioContext.Current["stringOptions"] as IEnumerable<string>;
            Assert.IsTrue(options.Contains(option));
           
            IntellisenseStringResultBuilder builder = new IntellisenseStringResultBuilder();
            var res = builder.Build(option,caretpos, originalText,originalText);
            ScenarioContext.Current["stringResult"] = res;
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

        [Then(@"the result text should be ""(.*)"" with caret position will be '(.*)'")]
        public void ThenTheResultTextShouldBeWithCaretPositionWillBe(string p0, int p1)
        {
           var res =  ScenarioContext.Current["stringResult"] as IIntellisenseStringResult;
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual(res.Result,p0);
            Assert.AreEqual(p1,res.CaretPosition);
        }
    }
}
