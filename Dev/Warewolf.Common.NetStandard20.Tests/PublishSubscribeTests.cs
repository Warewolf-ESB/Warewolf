/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Text;

namespace Warewolf.Streams
{
    [TestClass]
    public class PublishSubscribeTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(IPublisher))]
        public void IPublisher_IConsumer_GivenDeserializedData_ExpectConsumeCalled()
        {
            var publishedValue = new S("hello");

            var config = new Mock<IStreamConfig>().Object;
            var mockSerializer = new Mock<ISerializer>();
            var expectedSerializedBytes = UTF8Encoding.UTF8.GetBytes("test");
            mockSerializer.Setup(o => o.Serialize<SBase>(It.IsAny<SBase>())).Returns(expectedSerializedBytes);
            var serializer = mockSerializer.Object;
            
            var mockDeserializer = new Mock<IDeserializer>();
            var expectedDeserializedValue = publishedValue;
            mockDeserializer.Setup(o => o.Deserialize<SBase>(expectedSerializedBytes)).Returns(publishedValue);
            var deserializer = mockDeserializer.Object;
            

            var connection = new ConnectionForTesting();

            var publisher = connection.NewPublisher<SBase>(config, serializer);
            var mockConsumer = new Mock<IConsumer<SBase>>();
            var consumer = mockConsumer.Object;

            var t = new Thread(() =>
            {
                connection.StartConsuming<SBase>(config, consumer, deserializer);
            });
            t.Start();

            publisher.Publish(publishedValue);

            t.Join();

            mockDeserializer.Verify(o => o.Deserialize<SBase>(It.IsAny<byte[]>()), Times.Once);
            mockConsumer.Verify(o => o.Consume(expectedDeserializedValue), Times.Once);
        }

        class ConnectionForTesting : IConnection
        {
            readonly IPublisher _publisher;
            byte[] _publishedBytes = null;
            string _customTransactionID = "";
            readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            public ConnectionForTesting()
            {
                var publisher = new Mock<IPublisher>();
                publisher.Setup(o => o.Publish(It.IsAny<byte[]>())).Callback<byte[]>(bytes
                    => {
                        _publishedBytes = bytes;
                        _customTransactionID = "";
                        _waitHandle.Set();
                    });

                _publisher = publisher.Object;
            }

            public IPublisher NewPublisher(IStreamConfig config) => _publisher;
            public void StartConsuming(IStreamConfig config, IConsumer consumer)
            {
                _waitHandle.WaitOne();
                consumer.Consume(_publishedBytes, _customTransactionID);
            }
        }
    }

    public class S : SBase
    {
        public S(string s)
            : base(s)
        {
        }
    }
    public class SBase
    {
        string _s;
        public SBase(string s)
        {
            _s = s;
        }
    }
}
