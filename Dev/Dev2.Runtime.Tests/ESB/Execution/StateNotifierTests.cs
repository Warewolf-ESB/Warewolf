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
            using (var notifier = new StateNotifier())
            {
                var listenerMock = new Mock<IStateListener>();
                listenerMock.Setup(o => o.Notify("LogPreExecuteState", It.IsAny<object>())).Returns(true).Verifiable();
                listenerMock.Setup(o => o.Notify("LogPostExecuteState", It.IsAny<object>())).Returns(true).Verifiable();
                listenerMock.Setup(o => o.Notify("LogExecuteException", It.IsAny<object>())).Returns(true).Verifiable();
                listenerMock.Setup(o => o.Notify("LogAdditionalDetail", It.IsAny<object>())).Returns(true).Verifiable();
                listenerMock.Setup(o => o.Notify("LogExecuteCompleteState", It.IsAny<object>())).Returns(true).Verifiable();
                listenerMock.Setup(o => o.Notify("LogStopExecutionState", It.IsAny<object>())).Returns(true).Verifiable();
                var listener = listenerMock.Object;
                // test
                notifier.Subscribe(listener);
                var activityMock = new Mock<IDev2Activity>();
                var activity = activityMock.Object;

                notifier.LogPreExecuteState(activity);
                var nextActivityMock = new Mock<IDev2Activity>();
                var nextActivity = nextActivityMock.Object;
                notifier.LogPostExecuteState(activity, nextActivity);
                notifier.LogExecuteException(new Exception("some exception"), nextActivity);
                notifier.LogAdditionalDetail(new { Message = "Some Message" }, nameof(Dev2StateLogger_SubscribeToEventNotifications_Tests));
                notifier.LogExecuteCompleteState();
                notifier.LogStopExecutionState();

                // verify
                listenerMock.Verify();
            }
        }
    }
}
