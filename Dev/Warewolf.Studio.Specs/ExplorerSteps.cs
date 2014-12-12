using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Specs
{
    [Binding]
    public class ExplorerSteps
    {
        [Given(@"I have connected to localhost")]
        public void GivenIHaveConnectedToLocalhost()
        {
            var localHostEnvironment = new EnvironmentViewModel(new Server())
            {
                DisplayName = "localhost"                
            };
            localHostEnvironment.Connect();
            Assert.IsTrue(localHostEnvironment.IsConnected);
            ScenarioContext.Current.Add("localhost",localHostEnvironment);
        }

        [Given(@"localhost has loaded")]
        [When(@"localhost has loaded")]
        [Then(@"localhost has loaded")]
        public void WhenLocalhostHasLoaded()
        {
            EnvironmentViewModel localHostEnvironment;
            if (ScenarioContext.Current.TryGetValue("localhost", out localHostEnvironment))
            {
                localHostEnvironment.Load();
                Assert.IsTrue(localHostEnvironment.IsLoaded);
            }
        }

        [Then(@"I should see localhost resources")]
        public void ThenIShouldSeeLocalhostResources()
        {
            Assert.Fail("Check for localhost resources");
        }
    }

    public class Server : IServer
    {
        #region Implementation of IServer

        public bool Connect()
        {
            return true;
        }

        #endregion
    }
}
