using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.BrowserDebug
{
    [Binding]
    public sealed class BrowserDebugSteps
    {
        private readonly SpecExternalProcessExecutor _externalProcessExecutor = new SpecExternalProcessExecutor();

        [Then(@"I Debug ""(.*)"" in Browser")]
        [When(@"I Debug ""(.*)"" in Browser")]
        [Given(@"I Debug ""(.*)"" in Browser")]
        public void ThenIDebugInBrowser(string urlString)
        {
            _externalProcessExecutor.OpenInBrowser(new Uri(urlString));
        }

        [Then(@"Browser content is ""(.*)""")]
        public void ThenBrowserContentIs(string p0)
        {
            var webResult = _externalProcessExecutor.WebResult.First();
            var contains = webResult.Contains(p0);
            Assert.IsTrue(contains);
        }

        [Then(@"The Debug in Browser content contains ""(.*)""")]
        public void ThenTheDebugInBrowserContentContains(string containedText)
        {
            Assert.IsTrue(_externalProcessExecutor.WebResult.First().Contains(containedText),
                _externalProcessExecutor.WebResult.First());
        }

        private List<IDebugState> GetDebugStates()
        {
            var deserialize = _externalProcessExecutor.WebResult.First().DeserializeToObject<List<IDebugState>>();
            return deserialize.ToList();
        }

        [Given(@"The Debug in Browser content contains has error messagge ""(.*)""")]
        [When(@"The Debug in Browser content contains has error messagge ""(.*)""")]
        [Then(@"The Debug in Browser content contains has error messagge ""(.*)""")]
        public void WhenTheDebugInBrowserContentContainsHasErrorMessagge(string p0)
        {
            var debugStates = GetDebugStates();
            var msg = debugStates[1].ErrorMessage;
            Assert.IsTrue(debugStates.Last().HasError);
            Assert.AreEqual("invalid variable assigned to d@teMonth", msg);
        }

        [Given(@"The Debug in Browser content contains has children with no Inputs and Ouputs")]
        [When(@"The Debug in Browser content contains has children with no Inputs and Ouputs")]
        [Then(@"The Debug in Browser content contains has children with no Inputs and Ouputs")]
        public void TheDebugInBrowserContentHaveGivenVariable()
        {
            var deserialize = GetDebugStates();
            Assert.IsNotNull(deserialize);
            foreach (var debugState in deserialize)
            {
                Assert.AreEqual(0, debugState.Inputs.Count);
                Assert.AreEqual(0, debugState.Outputs.Count);
            }
        }

        [Then(@"The Debug in Browser content contains has ""(.*)"" inputs and ""(.*)"" outputs for ""(.*)""")]
        public void TheDebugInBrowserContentContainsHasInputsAndOutputsFor(int inputCount, int outputCount, string stepName)
        {
            var allDebugStates = GetDebugStates();
            var debugState = allDebugStates.FirstOrDefault(p => p.DisplayName == stepName);
            Assert.IsNotNull(debugState);
            Assert.AreEqual(inputCount, debugState.Inputs.Count);
            Assert.AreEqual(outputCount, debugState.Outputs.Count);
        }

        [Given(@"The Debug in Browser hello world content has inputs and outputs")]
        [When(@"The Debug in Browser hello world content has inputs and outputs")]
        [Then(@"The Debug in Browser hello world content has inputs and outputs")]
        public void TheDebugInBrowserContentForHelloWorldHasInputsAndOutputs()
        {
            var allDebugStates = GetDebugStates();
            foreach (var debugState in allDebugStates)
            {
                if (debugState.IsFirstStep())
                {
                    continue;
                }

                if (!debugState.IsFinalStep())
                {
                    Assert.IsTrue(debugState.Inputs.Count > 0);
                }

                Assert.IsTrue(debugState.Outputs.Count > 0);
            }
        }

        [When(@"The Debug in Browser content contains for Dotnet has (.*) states")]
        [Then(@"The Debug in Browser content contains for Dotnet has (.*) states")]
        [Given(@"The Debug in Browser content contains for Dotnet has (.*) states")]
        public void WhenTheDebugInBrowserContentContainsForDotnetHasStates(int numberOfStates)
        {
            var allDebugStates = GetDebugStates();
            Assert.AreEqual(numberOfStates, allDebugStates.Count);
        }

        [When(@"The (.*) debug state has (.*) children")]
        [Then(@"The (.*) debug state has (.*) children")]
        [Given(@"The (.*) debug state has (.*) children")]
        public void WhenTheDebugStateHasChildren(int stateIndex, int numberOfChildren)
        {
            var allDebugStates = GetDebugStates();
            var debugState = allDebugStates[stateIndex];
            Assert.AreEqual(numberOfChildren, debugState.Children.Count);
        }


        [Given(@"The Debug in Browser content contains order of ""(.*)"", ""(.*)"" and ""(.*)"" in SequenceFlow")]
        [When(@"The Debug in Browser content contains order of ""(.*)"", ""(.*)"" and ""(.*)"" in SequenceFlow")]
        [Then(@"The Debug in Browser content contains order of ""(.*)"", ""(.*)"" and ""(.*)"" in SequenceFlow")]
        public void ThenTheDebugInBrowserContentContainsOrderOfAndIn(string sequenceflow1, string sequenceflow2, string sequenceflow3)
        {
            var allDebugStates = GetDebugStates();
            List<string> expectedflow = new List<string> { sequenceflow1, sequenceflow2, sequenceflow3 };
            List<string> actualflow = new List<string>();

            
            foreach (var debugState in allDebugStates)
            {
                if (debugState.IsFirstStep())
                {
                    continue;
                }

                if (debugState.StateType != StateType.End)
                {

                    
                    foreach (var debugStateChild in debugState.Children)
                    {
                        actualflow.Add(debugStateChild.DisplayName);
                    }
                }
            }
            Assert.AreEqual(expectedflow.Count, actualflow.Count);
            Assert.AreEqual(expectedflow[0], actualflow[0]);
            Assert.AreEqual(expectedflow[1], actualflow[1]);
            Assert.AreEqual(expectedflow[2], actualflow[2]);
        }

        [Given(@"The Debug in Browser content contains the variable assigned executed ""(.*)"" times")]
        [When(@"The Debug in Browser content contains the variable assigned executed ""(.*)"" times")]
        [Then(@"The Debug in Browser content contains the variable assigned executed ""(.*)"" times")]
        public void ThenTheDebugInBrowserContentContainsTheVariableAssignedExecutedTimes(int numExecutions)
        {
            var allDebugStates = GetDebugStates();
            foreach (var debugState in allDebugStates)
            {
                if (debugState.IsFirstStep())
                {
                    continue;
                }

                if (debugState.StateType != StateType.End)
                {
                    Assert.IsTrue(debugState.Children.Count == numExecutions);
                }
            }
        }

        [Then(@"Debugstate in index (.*) has output as")]
        public void ThenDebugstateInIndexHasOutputAs(int p0, Table table)
        {
            var allDebugStates = GetDebugStates();
            var actualOrder = allDebugStates[p0].Outputs;
            List<TableRow> expectedOrder = table.Rows.ToList();

            Assert.AreEqual(expectedOrder.Count, actualOrder[0].ResultsList.Count);

            for (int i = 0; i < expectedOrder.Count; i++)
            {
                var val1 = actualOrder[0].ResultsList[i].Value;
                var val2 = expectedOrder[i].Values.Single();

                Assert.AreEqual(val1, val2);
            }
        }
    }
}
