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
    public class PublisherTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(IPublisher))]
        public void IPublisher_()
        {
            var mockPublisher = new Mock<IPublisher<SBase>>();
            var publisher = mockPublisher.Object;
            var myS = new S("s");
            publisher.Publish(myS);
            var myS1 = new SBase("s1");
            publisher.Publish(myS1);

            mockPublisher.Verify(o => o.Publish(myS), Times.Once);
            mockPublisher.Verify(o => o.Publish(myS1), Times.Once);
        }
    }
}
