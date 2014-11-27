
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
using Dev2.Communication;
using Dev2.Providers.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Providers.Events
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EventPublisherTests
    {
        [TestMethod]
        [Description("GetEvent must add a new subject when invoked for the first time for the type.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void EventPublisherGetEvent_UnitTest_FirstTimeForType_New()
        // ReSharper restore InconsistentNaming
        {
            var publisher = new EventPublisher();
            Assert.AreEqual(0, publisher.Count);

            var actual = publisher.GetEvent<object>();
            Assert.AreEqual(1, publisher.Count);
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(IObservable<object>));
        }

        [TestMethod]
        [Description("GetEvent must return an existing subject when invoked for the second time for the type.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void EventPublisherGetEvent_UnitTest_SecondTimeForType_Existing()
        // ReSharper restore InconsistentNaming
        {
            var publisher = new EventPublisher();
            Assert.AreEqual(0, publisher.Count);

            var first = publisher.GetEvent<object>();
            Assert.AreEqual(1, publisher.Count);
            Assert.IsNotNull(first);
            Assert.IsInstanceOfType(first, typeof(IObservable<object>));

            var second = publisher.GetEvent<object>();
            Assert.AreEqual(1, publisher.Count);
            Assert.IsNotNull(second);
            Assert.IsInstanceOfType(second, typeof(IObservable<object>));
        }

        [TestMethod]
        [Description("Publish must find the subject and invoke OnNext on it for a type that has been previously requested by GetEvent.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void EventPublisherPublish_UnitTest_RegisteredType_FindsSubjectAndInvokesOnNext()
        // ReSharper restore InconsistentNaming
        {
            var memo = new DesignValidationMemo();

            var publisher = new EventPublisher();
            var subscription = publisher.GetEvent<DesignValidationMemo>().Subscribe(m => Assert.AreSame(memo, m));

            publisher.Publish(memo);
            subscription.Dispose();
        }

        [TestMethod]
        [Description("Publish must find the subject and invoke OnNext on it for an object whose type has been previously requested by GetEvent.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void EventPublisherPublish_UnitTest_RegisteredObjectType_FindsSubjectAndInvokesOnNext()
        // ReSharper restore InconsistentNaming
        {
            var memo = new DesignValidationMemo() as object;

            var publisher = new EventPublisher();
            var subscription = publisher.GetEvent<DesignValidationMemo>().Subscribe(m => Assert.AreSame(memo, m));

            publisher.PublishObject(memo);
            subscription.Dispose();
        }


        [TestMethod]
        [Description("Publish must not find the subject and not invoke OnNext for a type that has not been previously requested by GetEvent.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void EventPublisherPublish_UnitTest_UnregisteredType_DoesNotFindSubject()
        // ReSharper restore InconsistentNaming
        {
            var memo = new Memo();

            var publisher = new EventPublisher();

            publisher.Publish(memo);
            Assert.IsTrue(true);
        }


        [TestMethod]
        [TestCategory("EventPublisherPublish_RemoveEvent")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void EventPublisherPublish_RemoveEvent_RegisteredObjectType_Removed()
        // ReSharper restore InconsistentNaming
        {
            var publisher = new EventPublisher();
            var subscription = publisher.GetEvent<DesignValidationMemo>();
            Assert.AreEqual(1, publisher.Count);

            var result = publisher.RemoveEvent<DesignValidationMemo>();
            Assert.AreEqual(0, publisher.Count);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestCategory("EventPublisherPublish_RemoveEvent")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void EventPublisherPublish_RemoveEvent_UnregisteredObjectType_NotRemoved()
        // ReSharper restore InconsistentNaming
        {
            var publisher = new EventPublisher();
            var subscription = publisher.GetEvent<DesignValidationMemo>();
            Assert.AreEqual(1, publisher.Count);

            var result = publisher.RemoveEvent<Memo>();
            Assert.AreEqual(1, publisher.Count);
            Assert.IsFalse(result);
        }
    }
}
