using Dev2.Activities.Specs.BaseTypes;
using Dev2.Messages;
using Dev2.Services;
using Dev2.Studio.Interfaces;
using Dev2.Util;
using Moq;
using System;
using System.Threading;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs
{
    [Binding]
    public class MergeExecutionSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly CommonSteps _commonSteps;
        private SubscriptionService<DebugWriterWriteMessage> _debugWriterSubscriptionService;
        private SpecExternalProcessExecutor _externalProcessExecutor;
        private readonly AutoResetEvent _resetEvt = new AutoResetEvent(false);

        public MergeExecutionSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            _commonSteps = new CommonSteps(_scenarioContext);
            AppSettings.LocalHost = "http://localhost:3142";
        }

        void TryGetValue<T>(string keyName, out T value)
        {
            _scenarioContext.TryGetValue(keyName, out value);
        }

        [BeforeScenario]
        public void Setup()
        {
            if (_debugWriterSubscriptionService != null)
            {
                _debugWriterSubscriptionService.Unsubscribe();
                _debugWriterSubscriptionService.Dispose();
            }

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            _externalProcessExecutor = new SpecExternalProcessExecutor();
        }

        [AfterScenario]
        public void CleanUp()
        {
            if (_debugWriterSubscriptionService != null)
            {
                _debugWriterSubscriptionService.Unsubscribe();
                _debugWriterSubscriptionService.Dispose();
            }
            _resetEvt?.Close();
        }

        [When(@"workflow ""(.*)"" merge is opened")]
        public void WhenWorkflowMergeIsOpened(string mergeWfName)
        {
            TryGetValue(mergeWfName, out IContextualResourceModel resourceModel);

            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            shellViewModel.OpenMergeConflictsView(resourceModel, resourceModel, false);
        }

        [Then(@"Current workflow contains ""(.*)"" tools")]
        public void ThenCurrentWorkflowContainsTools(int toolCount)
        {
            
        }

        [Then(@"Different workflow contains ""(.*)"" tools")]
        public void ThenDifferentWorkflowContainsTools(int toolCount)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
