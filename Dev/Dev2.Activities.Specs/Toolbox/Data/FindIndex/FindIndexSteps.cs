using System.Activities.Statements;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.FindIndex
{
    [Binding]
    public class FindIndexSteps : BaseActivityUnitTest
    {
        private const string ResultVariable = "[[result]]";
        private DsfIndexActivity _findIndex;
        private IDSFDataObject _result;
        private string _characters;
        private string _direction;
        private string _inField;
        private string _index;

        private void BuildDataList()
        {
            _findIndex = new DsfIndexActivity
                {
                    Result = ResultVariable,
                    InField = _inField,
                    Index = _index,
                    Characters = _characters,
                    Direction = _direction
                };

            TestStartNode = new FlowStep
                {
                    Action = _findIndex
                };


            var data = new StringBuilder();
            data.Append("<root>");
            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");

            CurrentDl = data.ToString();
            TestData = data.ToString();
        }

        [Given(@"Given the sentence ""(.*)""")]
        public void GivenGivenTheSentence(string inField)
        {
            _inField = inField;
        }

        [Given(@"I selected Index ""(.*)""")]
        public void GivenISelectedIndex(string index)
        {
            _index = index;
        }

        [Given(@"I search for characters ""(.*)""")]
        public void GivenISearchForCharacters(string characters)
        {
            _characters = characters;
        }

        [Given(@"I selected direction as ""(.*)""")]
        public void GivenISelectedDirectionAs(string direction)
        {
            _direction = direction;
        }


        [When(@"the data find index tool is executed")]
        public void WhenTheDataFindIndexToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the find index result is ""(.*)""")]
        public void ThenTheFindIndexResultIs(string results)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(results, actualValue);
        }
    }
}