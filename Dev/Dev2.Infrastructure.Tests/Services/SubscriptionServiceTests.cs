
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Providers.Events;
using Dev2.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Infrastructure.Tests.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SubscriptionServiceTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubscriptionServiceConstructorWithNullArgsExpectedThrowsArgumentNullException()
        {
            var service = new SubscriptionService<object>(null);
        }

        [TestMethod]
        public void SubscriptionServiceSubscribeWithArgsExpectedAddsSubscription()
        {
            var publisher = CreatePublisher();
            var service = new SubscriptionService<object>(publisher.Object);
            Assert.AreEqual(0, service.Count);
            service.Subscribe(null, memo => { });
            service.Subscribe(o => o != null, memo => { });
            Assert.AreEqual(2, service.Count);
        }

        [TestMethod]
        public void SubscriptionServiceDisposeExpectedDisposesSubscriptions()
        {
            var publisher = CreatePublisher();
            var service = new SubscriptionService<object>(publisher.Object);

            service.Subscribe(null, obj => { });
            Assert.AreEqual(1, service.Count);
            service.Dispose();
            Assert.AreEqual(0, service.Count);
        }

        static Mock<IEventPublisher> CreatePublisher()
        {
            var subscription = new Mock<IDisposable>();

            var observable = new Mock<IObservable<object>>();
            observable.Setup(o => o.Subscribe(It.IsAny<IObserver<object>>())).Returns(subscription.Object);

            var publisher = new Mock<IEventPublisher>();
            publisher.Setup(p => p.GetEvent<object>()).Returns(observable.Object);
            return publisher;
        }
    }
}
