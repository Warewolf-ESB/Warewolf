using System;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class StateNotifierTests
    {
        [TestMethod]
        public void Dev2StateLogger_SubscribeToEventNotifications_Tests()
        {
            var activityMock = new Mock<IDev2Activity>();
            var activity = activityMock.Object;

            var nextActivityMock = new Mock<IDev2Activity>();
            var nextActivity = nextActivityMock.Object;

            var exception = new Exception("some exception");

            var additionalDetailObject = new { Message = "Some Message" };
            var additionalDetailMethodName = nameof(Dev2StateLogger_SubscribeToEventNotifications_Tests);

            using (var notifier = new StateNotifier())
            {
                var listenerMock = new Mock<IStateListener>();
                listenerMock.Setup(o => o.LogPreExecuteState(activity)).Verifiable();
                listenerMock.Setup(o => o.LogPostExecuteState(activity, nextActivity)).Verifiable();
                listenerMock.Setup(o => o.LogExecuteException(exception, nextActivity)).Verifiable();
                listenerMock.Setup(o => o.LogAdditionalDetail(It.IsAny<object>(), It.IsAny<string>())).Verifiable();
                listenerMock.Setup(o => o.LogExecuteCompleteState()).Verifiable();
                listenerMock.Setup(o => o.LogStopExecutionState()).Verifiable();
                var listener = listenerMock.Object;
                // test
                notifier.Subscribe(listener);

                notifier.LogPreExecuteState(activity);
                notifier.LogPostExecuteState(activity, nextActivity);
                notifier.LogExecuteException(exception, nextActivity);
                notifier.LogAdditionalDetail(additionalDetailObject, additionalDetailMethodName);
                notifier.LogExecuteCompleteState();
                notifier.LogStopExecutionState();

                // verify
                listenerMock.Verify();
            }
        }
    }
}
