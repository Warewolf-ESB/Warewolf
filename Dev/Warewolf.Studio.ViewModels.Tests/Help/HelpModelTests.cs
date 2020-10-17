using System;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Common.Interfaces.Help;

namespace Warewolf.Studio.Models.Help.Tests
{
    [TestClass]
    public class HelpModelTests
    {
        #region Fields

        Mock<IEventAggregator> _eventAggregatorMock;

        HelpChangedEvent _helpChangedEvent;
        HelpModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _helpChangedEvent = new HelpChangedEvent();
            _eventAggregatorMock.Setup(it => it.GetEvent<HelpChangedEvent>()).Returns(_helpChangedEvent);
            _target = new HelpModel(_eventAggregatorMock.Object);
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [Timeout(100)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestHelpModel()
        {
            new HelpModel(null);
        }

        #endregion Test construction

        #region Test methods

        [TestMethod]
        [Timeout(100)]
        public void TestFireOnHelpReceived()
        {
            //arrange
            var isOnHelpTextReceivedRaised = false;
            var helpDesriptorMock = new Mock<IHelpDescriptor>();
            _target.OnHelpTextReceived +=
                (sender, args) =>
                    {
                        isOnHelpTextReceivedRaised = sender == _target && helpDesriptorMock.Object == args;
                    };
            
            //act
            _helpChangedEvent.Publish(helpDesriptorMock.Object);

            //assert
            Assert.IsTrue(isOnHelpTextReceivedRaised);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestDispose()
        {
            //arrange
            var isOnHelpTextReceivedRaised = false;
            var helpDesriptorMock = new Mock<IHelpDescriptor>();
            _target.OnHelpTextReceived +=
                (sender, args) =>
                    {
                        isOnHelpTextReceivedRaised = sender == _target && helpDesriptorMock.Object == args;
                    };

            //act
            _target.Dispose();

            //assert
            _helpChangedEvent.Publish(helpDesriptorMock.Object);
            Assert.IsFalse(isOnHelpTextReceivedRaised);
        }

        #endregion Test methods
    }
}