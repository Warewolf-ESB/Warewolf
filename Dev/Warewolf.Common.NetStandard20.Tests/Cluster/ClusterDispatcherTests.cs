using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;
using Warewolf.Debugging;

namespace Warewolf.Cluster
{
    [TestClass]
    public class ClusterDispatcherTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [Category(nameof(ClusterDispatcher))]
        public void ClusterDispatcher_AddListener_ExpectAllListenersNotified()
        {
            var clusterDispatcher = new ClusterDispatcherImplementation();
            var listener1 = AddNewListener(clusterDispatcher);
            var listener2 = AddNewListener(clusterDispatcher);

            clusterDispatcher.Write(new { Woot = true });

            listener1.Item2.Verify(o => o.Write(It.IsAny<ChangeNotification>()), Times.Once);
            listener2.Item2.Verify(o => o.Write(It.IsAny<ChangeNotification>()), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [Category(nameof(ClusterDispatcher))]
        public void ClusterDispatcher_AddListener_ExpectAllListenersAreInFollowersList()
        {
            var clusterDispatcher = new ClusterDispatcherImplementation();
            var listener1 = AddNewListener(clusterDispatcher);
            var listener2 = AddNewListener(clusterDispatcher);

            clusterDispatcher.Write(new { Woot = true });

            listener1.Item2.Verify(o => o.Write(It.IsAny<ChangeNotification>()), Times.Once);
            listener2.Item2.Verify(o => o.Write(It.IsAny<ChangeNotification>()), Times.Once);

            Assert.AreEqual(2, clusterDispatcher.Count);
            var followers = clusterDispatcher.Followers;
            Assert.AreEqual(listener1.Item2.Object, followers[0]);
            Assert.AreEqual(listener2.Item2.Object, followers[1]);
        }


        private (Guid, Mock<INotificationListener<ChangeNotification>>) AddNewListener(ClusterDispatcherImplementation clusterDispatcher)
        {
            var workspace1 = Guid.NewGuid();
            var notifier1 = new Mock<INotificationListener<ChangeNotification>>();
            clusterDispatcher.AddListener(workspace1, notifier1.Object);
            return (workspace1, notifier1);
        }

    }
}