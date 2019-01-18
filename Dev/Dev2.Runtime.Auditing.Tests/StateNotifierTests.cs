using System;
using Dev2.Interfaces;
using Dev2.Runtime.Auditing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Auditing
{
    [TestClass]
    public class StateNotifierTests
    {
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void StateNotifier_SubscribeToEventNotifications_Test()
        {
            var activityMock = new Mock<IDev2Activity>();
            var activity = activityMock.Object;

            var nextActivityMock = new Mock<IDev2Activity>();
            var nextActivity = nextActivityMock.Object;

            var exception = new Exception("some exception");

            var additionalDetailObject = new { Message = "Some Message" };
            var additionalDetailMethodName = nameof(StateNotifier_SubscribeToEventNotifications_Test);

            using (var notifier = new StateNotifier())
            {
                var listenerMock = new Mock<IStateListener>();
                listenerMock.Setup(o => o.LogPreExecuteState(activity)).Verifiable();
                listenerMock.Setup(o => o.LogPostExecuteState(activity, nextActivity)).Verifiable();
                listenerMock.Setup(o => o.LogExecuteException(exception, nextActivity)).Verifiable();
                listenerMock.Setup(o => o.LogAdditionalDetail(It.IsAny<object>(), It.IsAny<string>())).Verifiable();
                listenerMock.Setup(o => o.LogExecuteCompleteState(activity)).Verifiable();
                listenerMock.Setup(o => o.LogStopExecutionState(activity)).Verifiable();
                var listener = listenerMock.Object;
                // test
                notifier.Subscribe(listener);

                notifier.LogPreExecuteState(activity);
                notifier.LogPostExecuteState(activity, nextActivity);
                notifier.LogExecuteException(exception, nextActivity);
                notifier.LogAdditionalDetail(additionalDetailObject, additionalDetailMethodName);
                notifier.LogExecuteCompleteState(activity);
                notifier.LogStopExecutionState(activity);

                // verify
                listenerMock.Verify();
            }
        }
    }
}
