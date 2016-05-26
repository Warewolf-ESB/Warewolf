using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Explorer.Specs;
using TechTalk.SpecFlow;

namespace Warewolf.Security.Specs
{
    [Binding]
    public sealed class SecuritySteps
    {
        [Given(@"I have a server ""(.*)""")]
        public void GivenIHaveAServer(string p0)
        {
            var explorerSteps = new ExplorerSteps();
            explorerSteps.GivenIHaveAServer(p0);
        }

        [Given(@"it has ""(.*)"" with ""(.*)""")]
        public void GivenItHasWith(string p0, string p1)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"connected as user part of ""(.*)""")]
        public void WhenConnectedAsUserPartOf(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"resources should have ""(.*)""")]
        public void ThenResourcesShouldHave(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }
    }
}
