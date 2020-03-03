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

namespace Warewolf.Streams
{
    [TestClass]
    public class ConsumerTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(IConsumer))]
        public void IConsumer_()
        {
            var mockPublisher = new Mock<IConsumer<SBase>>();
            var publisher = mockPublisher.Object;
            var myS = new S("s");
            publisher.Consume(myS, null);
            var myS1 = new SBase("s1");
            publisher.Consume(myS1, null);

            mockPublisher.Verify(o => o.Consume(myS, null), Times.Once);
            mockPublisher.Verify(o => o.Consume(myS1, null), Times.Once);
        }
    }
}
