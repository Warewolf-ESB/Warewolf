
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Data.Enums;
using Dev2.Data.Parsers;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Language_Parser
{
    [Binding]
    public class LanguageParserSteps
    {
        [Given(@"I have text '(.*)' as input")]
        public void GivenIHaveTextAsInput(string variable)
        {
            ScenarioContext.Current.Add("variable", variable);
        }

        [When(@"I validate")]
        public void WhenIValidate()
        {
            IList<IIntellisenseResult> results = new List<IIntellisenseResult>();

            var variable = ScenarioContext.Current.Get<string>("variable");


            var parser = new Dev2DataLanguageParser();

            IntellisenseFilterOpsTO filterTO = new IntellisenseFilterOpsTO { FilterType = enIntellisensePartType.All };

            var datalist = new StringBuilder();
            datalist.Append("<DataList>");

            datalist.Append("<a>");
            datalist.Append("</a>");

            datalist.Append("<b>");
            datalist.Append("</b>");

            datalist.Append("<c>");
            datalist.Append("</c>");

            datalist.Append("<y>");
            datalist.Append("</y>");

            datalist.Append("<d>");
            datalist.Append("</d>");

            datalist.Append("<test>");
            datalist.Append("</test>");

            datalist.Append("<v>");
            datalist.Append("</v>");

            datalist.Append("<var>");
            datalist.Append("</var>");

            datalist.Append("<var1>");
            datalist.Append("</var1>");

            datalist.Append("<rec>");
            datalist.Append("<a/>");
            datalist.Append("<b/>");
            datalist.Append("</rec>");

            datalist.Append("<r>");
            datalist.Append("<a/>");
            datalist.Append("<b/>");
            datalist.Append("</r>");

            datalist.Append("</DataList>");

            var dataList = datalist.ToString();
            var tmpResults = parser.ParseDataLanguageForIntellisense(variable, dataList, true, filterTO, false);
            tmpResults.ToList().ForEach(r =>
            {
                if(r.Type == enIntellisenseResultType.Error)
                {
                    results.Add(r);
                }
            });

            ScenarioContext.Current.Add("results", results);
        }

        [Then(@"has error will be '(.*)'\.")]
        public void ThenHasErrorWillBe_(string error)
        {
            IList<IIntellisenseResult> results = ScenarioContext.Current.Get<IList<IIntellisenseResult>>("results");
            var actual = results != null && results.Count > 0;
            var expected = !string.IsNullOrEmpty(error) && error.ToLower().Equals("true");
            Assert.AreEqual(expected, actual);
        }

        [Then(@"the error message will be '(.*)'")]
        public void ThenTheErrorMessageWillBe(string errorMessage)
        {
            var errorMessages = errorMessage.Split(new[] { "/n" }, StringSplitOptions.None).ToList();
            if(!string.IsNullOrEmpty(errorMessage))
            {
                IList<IIntellisenseResult> results = ScenarioContext.Current.Get<IList<IIntellisenseResult>>("results");
                const int index = 0;
                Assert.AreEqual(errorMessages.Count, results.Count, "Number of error messgaes is not equal");
                foreach(var message in errorMessages)
                {
                    Assert.IsTrue(results.Any(e => e.Message.Contains(message.Trim())), string.Format("Expected error message [{0}] but was [{1}]", errorMessage, results[index].Message));
                }
            }
        }
    }
}
