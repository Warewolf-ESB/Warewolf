using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Deploy;
using Moq;
using TechTalk.SpecFlow;

namespace Dev2.Studio.Core.Specs.Deploy
{
    [Binding]
    public class DeploySteps
    {
        [Given(@"I have selected a work flow in the source server")]
        public void GivenIHaveSelectedAWorkFlowInTheSourceServer()
        {
            //ScenarioContext.Current.Pending();
        }

        [Given(@"the workflow does not exist in the destination server")]
        public void GivenTheWorkflowDoesNotExistInTheDestinationServer()
        {
            //ScenarioContext.Current.Pending();
        }

        [When(@"I click the deploy")]
        public void WhenIClickTheDeploy()
        {
            var source = new Mock<IEnvironmentModel>();
            var e1 = new Mock<IEnvironmentModel>();
            e1.Setup(e => e.Disconnect()).Verifiable();
            var e2 = new Mock<IEnvironmentModel>();
            e2.Setup(e => e.Disconnect()).Verifiable();
            var serverProvider = new Mock<IEnvironmentModelProvider>();
            
            var s2 = e2.Object;
            var c2 = Mock.Get(e2.Object.Connection);
            c2.Setup(c => c.Disconnect()).Verifiable();

            var s1 = e1.Object;
            var c1 = Mock.Get(e1.Object.Connection);
            c1.Setup(c => c.Disconnect()).Verifiable();
            
            serverProvider.Setup(s => s.Load()).Returns(new List<IEnvironmentModel> { s1, s2 });
            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);

            var deployViewModel = new DeployViewModel(serverProvider.Object, repo, new Mock<IEventAggregator>().Object);
        }

        [Then(@"the workflow should be deployed on the destination server")]
        public void ThenTheWorkflowShouldBeDeployedOnTheDestinationServer()
        {
            //ScenarioContext.Current.Pending();
        }

        [Given(@"The workflow does exists in the destination server")]
        public void GivenTheWorkflowDoesExistsInTheDestinationServer()
        {
            //ScenarioContext.Current.Pending();
        }

        [Then(@"The system prompts the user to overwrite with OK or Cancel buttons")]
        public void ThenTheSystemPromptsTheUserToOverwriteWithOKOrCancelButtons()
        {
            //ScenarioContext.Current.Pending();
        }

        [When(@"The user selects OK from the message")]
        public void WhenTheUserSelectsOKFromTheMessage()
        {
            //ScenarioContext.Current.Pending();
        }

        [When(@"The user selects Cancel from the message")]
        public void WhenTheUserSelectsCancelFromTheMessage()
        {
            //ScenarioContext.Current.Pending();
        }

        [Then(@"the deploy should be cancelled")]
        public void ThenTheDeployShouldBeCancelled()
        {
            //ScenarioContext.Current.Pending();
        }

    }
}
